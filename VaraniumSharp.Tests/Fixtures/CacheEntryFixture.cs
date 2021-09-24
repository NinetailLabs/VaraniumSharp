using VaraniumSharp.Interfaces.Caching;

namespace VaraniumSharp.Tests.Fixtures
{
    public class CacheEntryFixture : ICacheEntry
    {
        #region Properties

        public int CacheItemRemovedCalls { get; private set; }

        public int Id { get; set; }

        #endregion

        #region Public Methods

        public void CacheItemRemoved()
        {
            CacheItemRemovedCalls++;
        }

        #endregion
    }
}