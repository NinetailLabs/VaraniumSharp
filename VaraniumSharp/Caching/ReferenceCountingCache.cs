#if NETSTANDARD2_1_OR_GREATER
#nullable enable
#endif
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VaraniumSharp.Interfaces.Caching;

namespace VaraniumSharp.Caching
{
    /// <summary>
    /// Cache that uses counts of items in use to decide when items should be evicted
    /// </summary>
    public abstract class ReferenceCountingCacheBase<T> : CacheBase<T>, IReferenceCountingCache<T>, IDisposable where T : class, ICacheEntry
    {
        /// <summary>
        /// Timespan the cache will wait before purging un-references entries
        /// </summary>
        private readonly TimeSpan _cacheRetentionPeriod;

        #region Constructor

        /// <summary>
        /// Construct with a logger
        /// </summary>
        /// <param name="cacheRetentionPeriod">Timespan the cache will wait before purging un-referenced entries</param>
        protected ReferenceCountingCacheBase(TimeSpan cacheRetentionPeriod)
        {
            _cacheRetentionPeriod = cacheRetentionPeriod;
            ItemReferenceCount = new ConcurrentDictionary<int, int>();
            _cacheCleanupTimer = new Timer(CacheCleanupCallback, null, cacheRetentionPeriod, cacheRetentionPeriod);
            NextCacheEvictionTime = DateTime.Now.Add(_cacheRetentionPeriod);
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public int LastEvictionSize { get; private set; }

        /// <inheritdoc />
        public DateTime? LastEvictionTime { get; private set; }

        /// <inheritdoc />
        public DateTime NextCacheEvictionTime { get; private set; }

        /// <inheritdoc />
        public void EntryNoLongerUsed(int entityId)
        {
            if (ItemReferenceCount.ContainsKey(entityId))
            {
                ItemReferenceCount[entityId] -= 1;
            }
        }

        /// <inheritdoc />
#if NETSTANDARD2_1_OR_GREATER
        public override async Task<T?> RetrieveEntryItemAsync(int key)
#else
        public override async Task<T> RetrieveEntryItemAsync(int key)
#endif
        {
            var entry = await base
                .RetrieveEntryItemAsync(key)
                .ConfigureAwait(false);
            if (entry != null)
            {
                IncreaseReferenceCount(key);
            }

            return entry;
        }

        /// <inheritdoc />
        public override async Task<List<T>> RetrieveEntryItemsAsync(List<int> keys)
        {
            var entries = await base
                .RetrieveEntryItemsAsync(keys)
                .ConfigureAwait(false);
            var idsToIncrease = entries.Select(x => x.Id);
            foreach (var id in idsToIncrease)
            {
                IncreaseReferenceCount(id);
            }

            return entries;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Occurs when the <see cref="_cacheCleanupTimer"/> fires
        /// </summary>
        /// <param name="state">Always null</param>
#if NETSTANDARD2_1_OR_GREATER
        protected virtual async void CacheCleanupCallback(object? state)
#else
        protected virtual async void CacheCleanupCallback(object state)
#endif

        {
            try
            {
                LastEvictionTime = DateTime.Now;
                NextCacheEvictionTime = LastEvictionTime.Value.Add(_cacheRetentionPeriod);
                LastEvictionSize = 0;
                await ReferenceCleanupSemaphore
                    .WaitAsync()
                    .ConfigureAwait(false);

                var itemsToClean = ItemReferenceCount.Where(x => x.Value <= 0).ToList();
#if NETSTANDARD2_1_OR_GREATER
                foreach (var (id, _) in itemsToClean)
                {
#else
                foreach (var item in itemsToClean)
                {
                    var id = item.Key;
#endif
                    var key = id.ToString(CultureInfo.InvariantCulture);
                    await EntryCache
                        .RemoveFromCacheAsync(key)
                        .ConfigureAwait(false);
                    LastEvictionSize++;
#if NETSTANDARD2_1_OR_GREATER
                    ItemReferenceCount.Remove(id, out _);
#else
                    ItemReferenceCount.TryRemove(id, out _);
#endif
                }
            }
            finally
            {
                ReferenceCleanupSemaphore.Release();
            }
        }

        /// <summary>
        /// Performs tasks related to freeing resources etc
        /// </summary>
        /// <param name="disposing">Indicate if the class is being disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cacheCleanupTimer.Dispose();
            }
        }

        /// <summary>
        /// Increase the reference count for an AniList Id
        /// </summary>
        /// <param name="entityId">Id for which the reference count should be increased</param>
        protected void IncreaseReferenceCount(int entityId)
        {
            if (ItemReferenceCount.ContainsKey(entityId))
            {
                ItemReferenceCount[entityId] += 1;
            }
            else
            {
                ItemReferenceCount.TryAdd(entityId, 1);
            }
        }

        #endregion

        #region Variables

        /// <summary>
        /// Timer used to handle cache cleanup operations
        /// </summary>
        private readonly Timer _cacheCleanupTimer;

        /// <summary>
        /// Dictionary used to count the number of classes that hold a specific item instance
        /// </summary>
        protected readonly ConcurrentDictionary<int, int> ItemReferenceCount;

        /// <summary>
        /// Semaphore used to lock <see cref="CacheCleanupCallback"/>
        /// </summary>
        protected readonly SemaphoreSlim ReferenceCleanupSemaphore = new SemaphoreSlim(1);

        #endregion
    }
}