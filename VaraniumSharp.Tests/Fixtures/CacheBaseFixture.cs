using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;
using VaraniumSharp.Caching;
using VaraniumSharp.Interfaces.Caching;

namespace VaraniumSharp.Tests.Fixtures
{
    public class CacheBaseFixture : CacheBase<CacheEntryFixture>
    {
        #region Properties

        public int MultiEntryRepositoryRetrievalCalls { get; private set; }
        public int SingleEntryRepositoryRetrievalCalls { get; private set; }

        #endregion

        #region Public Methods

        public override async Task<bool> SaveReadmodelAsync(IEntity entry)
        {
            throw new System.NotImplementedException();
        }

        public override async Task<Dictionary<int, bool>> SaveReadmodelsAsync(List<IEntity> entries)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Private Methods

        protected override void CacheItemRemoved(CacheEntryRemovedArguments removedEntryDetails)
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<Dictionary<string, CacheEntryFixture>> RetrieveEntriesFromRepositoryAsync(List<string> keys)
        {
            MultiEntryRepositoryRetrievalCalls++;
            var results = new Dictionary<string, CacheEntryFixture>();
            foreach (var key in keys)
            {
                results.Add(key, new CacheEntryFixture
                {
                    Id = int.Parse(key)
                });
            }

            return results;
        }

        protected override async Task<CacheEntryFixture?> RetrieveEntryFromRepositoryAsync(string key)
        {
            SingleEntryRepositoryRetrievalCalls++;
            return new CacheEntryFixture
            {
                Id = int.Parse(key)
            };
        }

        #endregion
    }
}