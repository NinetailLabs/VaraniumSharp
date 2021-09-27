namespace VaraniumSharp.Interfaces.Caching
{
    /// <summary>
    /// Cache that uses counts of items in use to decide when items should be evicted
    /// </summary>
    public interface IReferenceCountingCache<T> : ICacheBase<T> where T : class, ICacheEntry
    {
        #region Public Methods

        /// <summary>
        /// Indicate that the entry is no longer used by an object.
        /// </summary>
        /// <param name="entityId">Id of the entity to remove</param>
        void EntryNoLongerUsed(int entityId);

        #endregion
    }
}