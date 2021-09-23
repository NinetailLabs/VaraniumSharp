#if NETSTANDARD2_1_OR_GREATER
#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Caching;
using System.Threading.Tasks;
using VaraniumSharp.Interfaces.Caching;

namespace VaraniumSharp.Caching
{
    /// <inheritdoc />
    public abstract class CacheBase<T> : ICacheBase<T> where T : class, ICacheEntry
    {
        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        protected CacheBase()
        {
            SetupCache();
        }

        #endregion

        #region Events

        /// <inheritdoc />
        public event EventHandler<T>? EntryChanged;

        #endregion

        #region Properties

        /// <inheritdoc />
        public long ItemsInCache => EntryCache.ItemsInCache;

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task<bool> ContainsKeyAsync(int key)
        {
            return await EntryCache
                .ContainsKeyAsync(key.ToString(CultureInfo.InvariantCulture))
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task RemoveItemFromCacheAsync(int key)
        {
            await EntryCache
                .RemoveFromCacheAsync(key.ToString(CultureInfo.InvariantCulture))
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public abstract Task<T?> RetrieveEntryItemAsync(int key);

        /// <inheritdoc />
        public abstract Task<List<T>> RetrieveEntryItemsAsync(List<int> keys);

        /// <inheritdoc />
        public abstract Task<bool> SaveReadmodelAsync(IEntity entry);

        /// <inheritdoc />
        public abstract Task<Dictionary<int, bool>> SaveReadmodelsAsync(List<IEntity> entries);

        #endregion

        #region Private Methods

        /// <summary>
        /// Fired when an item is removed from the cache
        /// </summary>
        /// <param name="removedEntryDetails">Details about the removed entry</param>
        protected abstract void CacheItemRemoved(CacheEntryRemovedArguments removedEntryDetails);

        /// <summary>
        /// Raise the <see cref="EntryChanged"/> event
        /// </summary>
        /// <param name="argument"></param>
        protected void RaiseEntryChanged(T argument)
        {
            EntryChanged?.Invoke(this, argument);
        }

        /// <summary>
        /// Retrieve multiple entries from the data store
        /// </summary>
        /// <param name="keys">The keys that should be retrieved from the data store</param>
        /// <returns>Collection of retrieved entries</returns>
        protected abstract Task<Dictionary<string, T>> RetrieveEntriesFromRepositoryAsync(List<string> keys);

        /// <summary>
        /// Retrieve an entry from the data store
        /// </summary>
        /// <param name="key">The key of the entry to retrieve</param>
        /// <returns>Retrieved entry</returns>
        protected abstract Task<T?> RetrieveEntryFromRepositoryAsync(string key);

        /// <summary>
        /// Initialize the cache setting up its <see cref="CacheItemPolicy"/> as well as the function to retrieve the entry from the data store
        /// </summary>
        private void SetupCache()
        {
            EntryCache = new MemoryCacheWrapper<T>
            {
                CachePolicy = new CacheItemPolicy
                {
                    RemovedCallback = CacheItemRemoved
                },
                DataRetrievalFunc = RetrieveEntryFromRepositoryAsync,
                BatchRetrievalFunc = RetrieveEntriesFromRepositoryAsync
            };
        }

        #endregion

        #region Variables

        /// <summary>
        /// Cache for storing entries
        /// </summary>
        protected MemoryCacheWrapper<T> EntryCache = null!;

        #endregion
    }
}
#endif