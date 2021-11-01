using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using VaraniumSharp.Tests.Fixtures;
using Xunit;

namespace VaraniumSharp.Tests.Caching
{
    public class CacheBaseTest
    {
        [Fact]
        public async Task RetrievingCacheEntryRetrievesTheEntryFromTheRepository()
        {
            // arrange
            const int key = 1;
            var sut = new CacheBaseFixture();

            // act
            var result = await sut.RetrieveEntryItemAsync(key);

            // assert
            result.Should().NotBeNull();
            sut.SingleEntryRepositoryRetrievalCalls.Should().Be(1);
            sut.ItemsInCache.Should().Be(1);
            sut.CacheRequests.Should().Be(1);
            sut.CacheHit.Should().Be(0);
            sut.AverageSingleRetrievalTime.TotalMilliseconds.Should().BeGreaterThan(0);
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
            sut.CacheRequests.Should().Be(2);
            sut.CacheHit.Should().Be(1);
            sut.AverageSingleRetrievalTime.TotalMilliseconds.Should().BeGreaterThan(0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ContainsKeyReturnsTheCorrectResult(bool isInCache)
        {
            // arrange
            const int key = 1;
            var sut = new CacheBaseFixture();
            if (isInCache)
            {
                await sut.RetrieveEntryItemAsync(key);
            }

            // act
            var result = await sut.ContainsKeyAsync(key);

            // assert
            result.Should().Be(isInCache);
        }

        [Fact]
        public async Task RequestingRemovalOfItemFromTheCacheRemovesTheItem()
        {
            // arrange
            const int key = 1;
            var sut = new CacheBaseFixture();
            await sut.RetrieveEntryItemAsync(key);

            // act
            await sut.RemoveItemFromCacheAsync(key);

            // assert
            sut.ItemsInCache.Should().Be(0);
            (await sut.ContainsKeyAsync(key)).Should().BeFalse();
        }

        [Fact]
        public async Task RetrievingMultipleEntriesWorksCorrectly()
        {
            // arrange
            const int key = 1;
            var sut = new CacheBaseFixture();
            await sut.RetrieveEntryItemAsync(key);

            // act
            var result = await sut.RetrieveEntryItemsAsync(new List<int>{ key });

            // assert
            result.Count.Should().Be(1);
            sut.MultiEntryRepositoryRetrievalCalls.Should().Be(1);
            sut.AverageBatchSize.Should().Be(1);
            sut.AverageBatchRetrievalTime.TotalMilliseconds.Should().BeGreaterThan(0);
        }
    }
}