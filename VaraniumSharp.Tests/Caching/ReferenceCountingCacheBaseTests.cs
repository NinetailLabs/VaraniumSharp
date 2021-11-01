using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;
using FluentAssertions;
using VaraniumSharp.Caching;
using VaraniumSharp.Interfaces.Caching;
using VaraniumSharp.Tests.Fixtures;
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

            #region Properties

            public int MultiEntryRepositoryRetrievalCalls { get; private set; }
            public int SingleEntryRepositoryRetrievalCalls { get; private set; }

            #endregion

            #region Public Methods

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
                MultiEntryRepositoryRetrievalCalls++;
                var results = new Dictionary<string, CacheEntityFixture>();
                foreach (var key in keys)
                {
                    results.Add(key, new CacheEntityFixture(int.Parse(key)));
                }

                return results;
            }

            protected override async Task<CacheEntityFixture> RetrieveEntryFromRepositoryAsync(string key)
            {
                SingleEntryRepositoryRetrievalCalls++;
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
        public async Task CacheEvictionStatisticsAreUpdatedDuringEntryRemoval()
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
            sut.LastEvictionSize.Should().BeGreaterOrEqualTo(0);
            sut.LastEvictionTime.Should().NotBeNull();
            sut.NextCacheEvictionTime.Should().BeAfter(sut.LastEvictionTime.Value);
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

        [Fact]
        public async Task RetrievingAnEntryAlreadyInTheCacheRetrievesItFromTheCache()
        {
            // arrange
            const int key = 1;
            var sut = new CacheBaseFixture();
            await sut.RetrieveEntryItemAsync(key);

            // act
            var result = await sut.RetrieveEntryItemAsync(key);

            // assert
            result.Should().NotBeNull();
            sut.ItemsInCache.Should().Be(1);
        }

        [Fact]
        public async Task RetrievingCacheEntryRetrievesTheEntryFromTheRepository()
        {
            // arrange
            const int key = 1;
            var cacheCleanDuration = TimeSpan.FromMilliseconds(200);
            var sut = new ReferenceCountingBaseFixture(cacheCleanDuration);

            // act
            var result = await sut.RetrieveEntryItemAsync(key);

            // assert
            result.Should().NotBeNull();
            sut.SingleEntryRepositoryRetrievalCalls.Should().Be(1);
            sut.ItemsInCache.Should().Be(1);
        }

        [Fact]
        public async Task RetrievingMultipleEntriesWorksCorrectly()
        {
            // arrange
            const int key = 1;
            var cacheCleanDuration = TimeSpan.FromMilliseconds(200);
            var sut = new ReferenceCountingBaseFixture(cacheCleanDuration);
            await sut.RetrieveEntryItemAsync(key);

            // act
            var result = await sut.RetrieveEntryItemsAsync(new List<int> { key });

            // assert
            result.Count.Should().Be(1);
            sut.MultiEntryRepositoryRetrievalCalls.Should().Be(1);
        }
    }
}