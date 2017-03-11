using System;
using System.Collections.Concurrent;
using System.ComponentModel;
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
    public class MemoryCacheWrapper<T> : IMemoryCacheWrapper<T>
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
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

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
            get { return _cacheItemPolicy; }
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
        public Func<string, Task<T>> DataRetrievalFunc
        {
            get { return _dataRetrievalFunc; }
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

        /// <summary>
        /// Retrieve an item from the cache
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if CachePolicy or DataRetrievalFunc has not been set</exception>
        /// <param name="key">The key under which the item is stored</param>
        /// <returns>Cached copy of T</returns>
        public async Task<T> GetAsync(string key)
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
            _memoryCache.Add(key, result, CachePolicy);
            ItemsInCache = _memoryCache.GetCount();

            semaphore.Release();

            return result;
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

        private readonly ConcurrentDictionary<string, SemaphoreSlim> _cacheLockDictionary;
        private readonly object _funcLock = new object();

        private readonly MemoryCache _memoryCache;
        private readonly object _policyLock = new object();
        private CacheItemPolicy _cacheItemPolicy;
        private Func<string, Task<T>> _dataRetrievalFunc;
        private bool _funcAlreadySet;
        private bool _policyAlreadySet;

        #endregion
    }
}