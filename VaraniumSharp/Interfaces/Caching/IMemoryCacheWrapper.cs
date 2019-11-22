using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace VaraniumSharp.Interfaces.Caching
{
    /// <summary>
    /// Provides a wrapper around <see cref="MemoryCache"/> that provides an easy way to get and remove items from the cache
    /// </summary>
    /// <typeparam name="T">Type of items that will be stored in the cache</typeparam>
    public interface IMemoryCacheWrapper<T> : INotifyPropertyChanged
    {
        #region Properties

        /// <summary>
        /// Func that will be used to batch retrieve data to warm up the cache
        /// <exception cref="InvalidOperationException">Thrown if the Func has already been set</exception>
        /// </summary>
        Func<List<string>, Task<Dictionary<string, T>>> BatchRetrievalFunc { get; set; }

        /// <summary>
        /// Policy to use for cached items
        /// <see>
        ///     <cref>
        ///         https://msdn.microsoft.com/en-us/library/system.runtime.caching.cacheitempolicy(v=vs.110).aspx
        ///     </cref>
        /// </see>
        /// <exception cref="InvalidOperationException">Thrown if the policy has already been set</exception>
        /// </summary>
        CacheItemPolicy CachePolicy { get; set; }

        /// <summary>
        /// Func that will be used to retrieve data if it is not available in the cache
        /// <exception cref="InvalidOperationException">Thrown if the Func has already been set</exception>
        /// </summary>
        Func<string, Task<T>> DataRetrievalFunc { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Request that a batch of entries be added to the cache.
        /// This method will not retrieve entries that are already in the cache again.
        /// <exception cref="InvalidOperationException">Thrown if the <see cref="BatchRetrievalFunc"/> has not been set</exception>
        /// </summary>
        /// <param name="keys">Collection of keys of the entries that should be added to the cache</param>
        /// <returns>Number of entries that were added to the cache</returns>
        Task<int> BatchAddToCacheAsync(List<string> keys);

        /// <summary>
        /// Check if an item with a specific key is currently in the cache
        /// </summary>
        /// <param name="key">The key under which the item is stored</param>
        /// <returns>True - Item is currently cache, otherwise false</returns>
        Task<bool> ContainsKeyAsync(string key);

        /// <summary>
        /// Check if an item with a specific key is currently in the cache.
        /// </summary>
        /// <exception cref="TimeoutException">Thrown if the timeout expires without acquiring the Semaphore</exception>
        /// <param name="key">The key under which the item is stored</param>
        /// <param name="timeout">Duration to wait for the semaphore before timing out</param>
        /// <returns></returns>
        Task<bool> ContainsKeyAsync(string key, TimeSpan timeout);

        /// <summary>
        /// Retrieve an item from the cache
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if CachePolicy or DataRetrievalFunc has not been set</exception>
        /// <param name="key">The key under which the item is stored</param>
        /// <returns>Cached copy of T</returns>
        Task<T> GetAsync(string key);

        /// <summary>
        /// Remove an item from the Cache
        /// </summary>
        /// <param name="key">The key under which the item is stored</param>
        /// <returns>True if the item has been removed. False if it is not in the cache</returns>
        Task<bool> RemoveFromCacheAsync(string key);

        #endregion
    }
}