#if NETSTANDARD2_1_OR_GREATER
#nullable enable
#endif
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VaraniumSharp.Interfaces.Caching
{
    /// <summary>
    /// Base class for managing caching of IEntities
    /// </summary>
    public interface ICacheBase<T> where T : class, ICacheEntry
    {
        #region Properties

        /// <summary>
        /// The number of entries in the cache
        /// </summary>
        long ItemsInCache { get; }

#endregion

#region Public Methods

        /// <summary>
        /// Check if the cache contains an entry for a specific keys
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>True if the cache has an entry for the keys, otherwise false</returns>
        Task<bool> ContainsKeyAsync(int key);

        /// <summary>
        /// Remove an entry from the cache
        /// </summary>
        /// <param name="key">Key for the entry</param>
        Task RemoveItemFromCacheAsync(int key);

        /// <summary>
        /// Retrieve an entry from the cache
        /// </summary>
        /// <param name="key">Key of the entry to retrieve</param>
        /// <returns>The entry if it exists in the cache</returns>
#if NETSTANDARD2_1_OR_GREATER
        Task<T?> RetrieveEntryItemAsync(int key);
#else
        Task<T> RetrieveEntryItemAsync(int key);
#endif

        /// <summary>
        /// Retrieve a collection of entries from the cache
        /// </summary>
        /// <param name="keys">Collection of keys to retrieve</param>
        /// <returns>Items that were retrieved from the cache or data store</returns>
        Task<List<T>> RetrieveEntryItemsAsync(List<int> keys);

        /// <summary>
        /// Method for saving an entry to the database
        /// </summary>
        /// <param name="entry">The entry to save.</param>
        /// <returns>Indicate if save was successful or not</returns>
        Task<bool> SaveReadmodelAsync(IEntity entry);

        /// <summary>
        /// Method for saving multiple entries to the database
        /// </summary>
        /// <param name="entries">The entries to save</param>
        /// <returns>Dictionary indicating which entries succeeded or failed to save</returns>
        Task<Dictionary<int, bool>> SaveReadmodelsAsync(List<IEntity> entries);

#endregion
    }
}
