using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;
using FluentAssertions;
using VaraniumSharp.Caching;
using VaraniumSharp.Interfaces.Caching;
using Xunit;

namespace VaraniumSharp.Tests.Caching
{
    public class ReferenceCountingCacheBaseTests
    {
        private class ReferenceCountingBaseFixture : ReferenceCountingCacheBase<CacheEntityFixture>
        {
            #region Constructor

            public ReferenceCountingBaseFixture(TimeSpan cacheRetentionPeriod)
                : base(cacheRetentionPeriod)
            { }

            #endregion

            #region Public Methods

            public override async Task<CacheEntityFixture> RetrieveEntryItemAsync(int key)
            {
                var stringKey = key.ToString();
                IncreaseReferenceCount(key);
                return await EntryCache.GetAsync(stringKey);
            }

            public override async Task<List<CacheEntityFixture>> RetrieveEntryItemsAsync(List<int> keys)
            {
                throw new NotImplementedException();
            }

            public override async Task<bool> SaveReadmodelAsync(IEntity entry)
            {
                // Does nothing for this fixture
                await Task.Delay(1);
                return true;
            }

            public override async Task<Dictionary<int, bool>> SaveReadmodelsAsync(List<IEntity> entries)
            {
                throw new NotImplementedException();
            }

            #endregion

            #region Private Methods

            protected override void CacheItemRemoved(CacheEntryRemovedArguments removedEntryDetails)
            {
                if (removedEntryDetails.CacheItem.Value is CacheEntityFixture entry)
                {
                    entry.CacheItemRemoved();
                }
            }

            protected override async Task<Dictionary<string, CacheEntityFixture>> RetrieveEntriesFromRepositoryAsync(List<string> keys)
            {
                throw new NotImplementedException();
            }

            protected override async Task<CacheEntityFixture> RetrieveEntryFromRepositoryAsync(string key)
            {
                await Task.Delay(1);
                if (int.TryParse(key, out var intKey))
                {
                    return new CacheEntityFixture(intKey);
                }

                throw new KeyNotFoundException("Key could not be found");
            }

            #endregion
        }

        private class CacheEntityFixture : ICacheEntry
        {
            #region Constructor

            public CacheEntityFixture(int id)
            {
                Id = id;
            }

            #endregion

            #region Properties

            public int CacheItemRemovedCallCount { get; private set; }

            public int Id { get; }

            #endregion

            #region Public Methods

            public void CacheItemRemoved()
            {
                CacheItemRemovedCallCount++;
            }

            #endregion
        }

        [Fact]
        public async Task IfAnEntryIsNoLongerUsedItIsRemovedFromTheCache()
        {
            // arrange
            const int key = 12;
            var cacheCleanDuration = TimeSpan.FromMilliseconds(200);
            using var sut = new ReferenceCountingBaseFixture(cacheCleanDuration);
            var entry = await sut.RetrieveEntryItemAsync(key);

            // act
            sut.EntryNoLongerUsed(key);
            await Task.Delay(TimeSpan.FromMilliseconds(600));

            // assert
            entry.CacheItemRemovedCallCount.Should().Be(1);
        }

        [Fact]
        public async Task IfAnEntryWasRemovedFromTheCacheItIsRetrievedFromTheDataStoreOnASubsequentRequest()
        {
            // arrange
            const int key = 12;
            var cacheCleanDuration = TimeSpan.FromMilliseconds(200);
            using var sut = new ReferenceCountingBaseFixture(cacheCleanDuration);
            var entry = await sut.RetrieveEntryItemAsync(key);
            sut.EntryNoLongerUsed(key);
            await Task.Delay(TimeSpan.FromMilliseconds(600));

            // act
            var newEntry = await sut.RetrieveEntryItemAsync(key);

            // assert
            entry.Should().NotBe(newEntry);
        }

        [Fact]
        public async Task IfEntryIsNotMarkedAsNotUsedItIsNotRemovedWhenTheCacheIsCleared()
        {
            // arrange
            const int key = 12;
            var cacheCleanDuration = TimeSpan.FromMilliseconds(200);
            using var sut = new ReferenceCountingBaseFixture(cacheCleanDuration);

            // act
            var entry = await sut.RetrieveEntryItemAsync(key);
            await Task.Delay(TimeSpan.FromMilliseconds(600));

            // assert
            entry.CacheItemRemovedCallCount.Should().Be(0);
        }

        [Fact]
        public async Task IfMultipleReferencesAreHeldAndOneIsReleasedTheEntryIsNotRemovedFromTheCache()
        {
            // arrange
            const int key = 12;
            var cacheCleanDuration = TimeSpan.FromMilliseconds(200);
            using var sut = new ReferenceCountingBaseFixture(cacheCleanDuration);
            var entry = await sut.RetrieveEntryItemAsync(key);
            await sut.RetrieveEntryItemAsync(key);

            // act
            sut.EntryNoLongerUsed(key);
            await Task.Delay(TimeSpan.FromMilliseconds(600));

            // assert
            entry.CacheItemRemovedCallCount.Should().Be(0);
        }
    }
}