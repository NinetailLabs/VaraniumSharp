namespace VaraniumSharp.Interfaces.Caching
{
    /// <summary>
    /// Entry for use in ICacheManagerBase
    /// </summary>
    public interface ICacheEntry
    {
        #region Properties

        /// <summary>
        /// Id of the entry
        /// </summary>
        int Id { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method is to be called when the item is removed from the cache
        /// </summary>
        void CacheItemRemoved();

        #endregion
    }
}