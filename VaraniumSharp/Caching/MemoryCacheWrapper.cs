#if NETSTANDARD2_1_OR_GREATER
#nullable enable
#endif
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using VaraniumSharp.Attributes;
using VaraniumSharp.Interfaces.Caching;

#pragma warning disable 0067

namespace VaraniumSharp.Caching
{
    /// <summary>
    /// Wrapper for <see cref="MemoryCache"/> that provides an easy double-checked lock method for retrieving items from the cache
    /// </summary>
    /// <typeparam name="T">Type of items that will be stored in the cache</typeparam>
    [AutomaticContainerRegistration(typeof(IMemoryCacheWrapper<>))]
    public class MemoryCacheWrapper<T> : IMemoryCacheWrapper<T> where T: class
    {
        #region Constructor

        /// <summary>
        /// Parameterless Constructor
        /// </summary>
        public MemoryCacheWrapper()
        {
            var cacheName = Guid.NewGuid().ToString();
            _memoryCache = new MemoryCache(cacheName);
            _cacheLockDictionary = new ConcurrentDictionary<string, SemaphoreSlim>();
        }

        #endregion

        #region Events

        /// <summary>Occurs when a property value changes.</summary>
#if NETSTANDARD2_1_OR_GREATER
        public event PropertyChangedEventHandler? PropertyChanged;
#else
        public event PropertyChangedEventHandler PropertyChanged;
#endif

        #endregion

        #region Properties

        /// <inheritdoc />
        public Func<List<string>, Task<Dictionary<string, T>>> BatchRetrievalFunc
        {
            get => _batchRetrievalFunc;
            set
            {
                lock (_funcLock)
                {
                    if (_batchFuncAlreadySet)
                    {
                        throw new InvalidOperationException("Func can only be set once");
                    }

                    _batchRetrievalFunc = value;
                    _batchFuncAlreadySet = true;
                }
            }
        }

        /// <summary>
        /// Policy to use for cached items
        /// <see>
        ///     <cref>
        ///         https://msdn.microsoft.com/en-us/library/system.runtime.caching.cacheitempolicy(v=vs.110).aspx
        ///     </cref>
        /// </see>
        /// <exception cref="InvalidOperationException">Thrown if the policy has already been set</exception>
        /// </summary>
        public CacheItemPolicy CachePolicy
        {
            get => _cacheItemPolicy;
            set
            {
                lock (_policyLock)
                {
                    if (_policyAlreadySet)
                    {
                        throw new InvalidOperationException("Cache policy can only be set once");
                    }
                    _cacheItemPolicy = value;
                    _policyAlreadySet = true;
                }
            }
        }

        /// <summary>
        /// Func that will be used to retrieve data if it is not available in the cache
        /// <exception cref="InvalidOperationException">Thrown if the Func has already been set</exception>
        /// </summary>
#if NETSTANDARD2_1_OR_GREATER
        public Func<string, Task<T?>> DataRetrievalFunc
#else
        public Func<string, Task<T>> DataRetrievalFunc
#endif
        {
            get => _dataRetrievalFunc;
            set
            {
                lock (_funcLock)
                {
                    if (_funcAlreadySet)
                    {
                        throw new InvalidOperationException("Func can only be set once");
                    }
                    _dataRetrievalFunc = value;
                    _funcAlreadySet = true;
                }
            }
        }

        /// <summary>
        /// The number of entries in the cache
        /// </summary>
        public long ItemsInCache { get; private set; }

#endregion

#region Public Methods

        /// <inheritdoc />
        public async Task<int> BatchAddToCacheAsync(List<string> keys)
        {
            if (_batchRetrievalFunc == null)
            {
                throw new InvalidOperationException("BatchRetrievalFunc has not been set");
            }

            var keysToRetrieve = new ConcurrentBag<string>();
            var lockedSemaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
            var entriesAdded = 0;

            try
            {
                foreach(var key in keys)
                {
                    var semaphore = _cacheLockDictionary.GetOrAdd(key, new SemaphoreSlim(1));
                    lockedSemaphores.TryAdd(key, semaphore);
                    await semaphore.WaitAsync();

                    if (_memoryCache.Contains(key))
                    {
                        semaphore.Release();
                        lockedSemaphores.TryRemove(key, out _);
                    }
                    else
                    {
                        keysToRetrieve.Add(key);
                    }
                }

                var batchResult = await _batchRetrievalFunc
                    .Invoke(keysToRetrieve.ToList())
                    .ConfigureAwait(false);

                foreach(var entry in batchResult)
                {
                    _memoryCache.Add(entry.Key, entry.Value, CachePolicy);
                    lockedSemaphores[entry.Key].Release();
                    lockedSemaphores.TryRemove(entry.Key, out _);
                    entriesAdded++;
                }
            }
            finally
            {
                foreach (var semaphore in lockedSemaphores)
                {
                    semaphore.Value.Release();
                }
            }

            return entriesAdded;
        }

        /// <inheritdoc />
        public async Task<bool> ContainsKeyAsync(string key)
        {
            var semaphore = _cacheLockDictionary.GetOrAdd(key, new SemaphoreSlim(1));
            await semaphore.WaitAsync();

            try
            {
                return _memoryCache.Contains(key);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc />
        public async Task<bool> ContainsKeyAsync(string key, TimeSpan timeout)
        {
            var semaphore = _cacheLockDictionary.GetOrAdd(key, new SemaphoreSlim(1));

            try
            {

                if (!await semaphore.WaitAsync(timeout))
                {
                    throw new TimeoutException();
                }

                return _memoryCache.Contains(key);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Retrieve an item from the cache
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if CachePolicy or DataRetrievalFunc has not been set</exception>
        /// <param name="key">The key under which the item is stored</param>
        /// <returns>Cached copy of T</returns>
#if NETSTANDARD2_1_OR_GREATER
        public async Task<T?> GetAsync(string key)
#else
        public async Task<T> GetAsync(string key)
#endif
        {
            if (!_policyAlreadySet || !_funcAlreadySet)
            {
                var error = !_policyAlreadySet ? "Cache Policy" : "DataRetrievalFunc";
                throw new InvalidOperationException($"{error} has not been set");
            }

            var response = _memoryCache.Get(key);
            if (response != null)
            {
                return (T)response;
            }

            var semaphore = _cacheLockDictionary.GetOrAdd(key, new SemaphoreSlim(1));
            await semaphore.WaitAsync();

            var lockedResponse = _memoryCache.Get(key);
            if (lockedResponse != null)
            {
                semaphore.Release();
                return (T)lockedResponse;
            }

            var result = await DataRetrievalFunc.Invoke(key);
            if (result != null)
            {
                _memoryCache.Add(key, result, CachePolicy);
                ItemsInCache = _memoryCache.GetCount();
            }

            semaphore.Release();

            return result;
        }

        /// <inheritdoc />
        public async Task<Dictionary<string, T>> GetAsync(List<string> keys)
        {
            if (!_policyAlreadySet || !_funcAlreadySet)
            {
                var error = !_policyAlreadySet ? "Cache Policy" : "DataRetrievalFunc";
                throw new InvalidOperationException($"{error} has not been set");
            }

            await BatchAddToCacheAsync(keys).ConfigureAwait(false);

            var response = _memoryCache.GetValues(keys);
            return response
                .Select(x => new KeyValuePair<string, T>(x.Key, (T)x.Value))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        /// <inheritdoc />
        public List<T> GetForKeysAlreadyInCache(IEnumerable<string> keys)
        {
            var resultBag = new ConcurrentBag<T>();
            Parallel.ForEach(keys, key =>
            {
                var semaphore = _cacheLockDictionary.GetOrAdd(key, new SemaphoreSlim(1));

                try
                {
                    semaphore.Wait();

                    if (_memoryCache.Contains(key))
                    {
                        resultBag.Add((T)_memoryCache.Get(key));
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

            return resultBag.ToList();
        }

        /// <summary>
        /// Remove an item from the Cache
        /// </summary>
        /// <param name="key">The key under which the item is stored</param>
        /// <returns>True if the item has been removed. False if it is not in the cache</returns>
        public async Task<bool> RemoveFromCacheAsync(string key)
        {
            var removed = false;
            var semaphore = _cacheLockDictionary.GetOrAdd(key, new SemaphoreSlim(1));
            await semaphore.WaitAsync();

            if (_memoryCache.Contains(key))
            {
                _memoryCache.Remove(key);
                ItemsInCache = _memoryCache.GetCount();
                removed = true;
            }

            semaphore.Release();
            return removed;
        }

#endregion

#region Variables

        /// <summary>
        /// Dictionary used to lock cache access per key
        /// </summary>
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _cacheLockDictionary;

        /// <summary>
        /// Object used to lock access to Func properties for setting
        /// </summary>
        private readonly object _funcLock = new object();

        /// <summary>
        /// Memory cache instance
        /// </summary>
        private readonly MemoryCache _memoryCache;

        /// <summary>
        /// Object used to lock access to the <see cref="CachePolicy"/> property
        /// </summary>
        private readonly object _policyLock = new object();

        /// <summary>
        /// Variable used to indicate if the <see cref="BatchRetrievalFunc"/> has already been set
        /// </summary>
        private bool _batchFuncAlreadySet;

        /// <summary>
        /// Backing variable for the <see cref="BatchRetrievalFunc"/> property
        /// </summary>
        private Func<List<string>, Task<Dictionary<string, T>>> _batchRetrievalFunc;

        /// <summary>
        /// Variable backing the <see cref="CachePolicy"/> property
        /// </summary>
        private CacheItemPolicy _cacheItemPolicy;

        /// <summary>
        /// Func used to retrieve entries that are not in the cache
        /// </summary>
#if NETSTANDARD2_1_OR_GREATER
        private Func<string, Task<T?>> _dataRetrievalFunc;
#else
        private Func<string, Task<T>> _dataRetrievalFunc;
#endif

        /// <summary>
        /// Indicates if the <see cref="DataRetrievalFunc"/> has already been set
        /// </summary>
        private bool _funcAlreadySet;

        /// <summary>
        /// Indicates if the <see cref="CachePolicy"/> has already been set
        /// </summary>
        private bool _policyAlreadySet;

#endregion
    }
}