using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using VaraniumSharp.Caching;

namespace VaraniumSharp.Tests.Caching
{
    public class MemoryCacheWrapperTests
    {
        #region Public Methods

        [Test]
        public void AddItemToTheCache()
        {
            // arrange
            const string key = "test";
            const int returnValue = 12;
            var retrievalFixture = new RetrievalFixture<int>(returnValue);
            var sut = new MemoryCacheWrapper<int>
            {
                CachePolicy = new CacheItemPolicy(),
                DataRetrievalFunc = retrievalFixture.DataRetrievalFunc
            };

            // act
            var result = sut.GetAsync(key).Result;

            // assert
            sut.ItemsInCache.Should().Be(1);
            retrievalFixture.WasCalled.Should().BeTrue();
            result.Should().Be(returnValue);
        }

        [Test]
        public void AttemptToRemoveItemThatIsNotInTheCache()
        {
            // arrange
            const string key = "test";
            const int returnValue = 12;
            var retrievalFixture = new RetrievalFixture<int>(returnValue);
            var sut = new MemoryCacheWrapper<int>
            {
                CachePolicy = new CacheItemPolicy(),
                DataRetrievalFunc = retrievalFixture.DataRetrievalFunc
            };

            // act
            var removed = sut.RemoveFromCacheAsync(key).Result;

            // assert
            removed.Should().BeFalse();
        }

        [Test]
        public void CachePolicyCanOnlyBeSetOnce()
        {
            // arrange
            var sut = new MemoryCacheWrapper<int>
            {
                CachePolicy = new CacheItemPolicy()
            };

            var act = new Action(() => sut.CachePolicy = new CacheItemPolicy());

            // act
            // assert
            act.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void CannotRetrieveCacheItemIfPolicyIsNotSet()
        {
            // arrange
            const string key = "test";
            var sut = new MemoryCacheWrapper<int>();
            var act = new Action(() => sut.GetAsync(key).Wait());

            // act
            // assert
            act.ShouldThrow<InvalidOperationException>("Cache Policy has not been set");
        }

        [Test]
        public void CannotRetrieveCacheItemIfRetrievalFuncIsNotSet()
        {
            // arrange
            const string key = "test";
            var sut = new MemoryCacheWrapper<int>
            {
                CachePolicy = new CacheItemPolicy()
            };
            var act = new Action(() => sut.GetAsync(key).Wait());

            // act
            // assert
            act.ShouldThrow<InvalidOperationException>("DataRetrievalFunc has not been set");
        }

        [Test]
        public void DataRetrievalFuncCanOnlyBeSetOnce()
        {
            // arrange
            var sut = new MemoryCacheWrapper<int>
            {
                DataRetrievalFunc = DataRetrievalFunc
            };

            var act = new Action(() => sut.DataRetrievalFunc = SecondRetrievalFunc);

            // act
            // assert
            act.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void MultipleRetrievalsOnlyHitTheRetrievalFuncitonOnce()
        {
            // arrange
            const string key = "test";
            const int returnValue = 12;
            var retrievalFixture = new RetrievalFixture<int>(returnValue);
            var sut = new MemoryCacheWrapper<int>
            {
                CachePolicy = new CacheItemPolicy(),
                DataRetrievalFunc = retrievalFixture.DataRetrievalFunc
            };

            // act
            Parallel.For(0, 10, t =>
            {
                sut.GetAsync(key).Wait();
            });

            // assert
            retrievalFixture.CallCount.Should().Be(1);
        }

        [Test]
        public void RemoveItemFromCache()
        {
            // arrange
            const string key = "test";
            const int returnValue = 12;
            var retrievalFixture = new RetrievalFixture<int>(returnValue);
            var sut = new MemoryCacheWrapper<int>
            {
                CachePolicy = new CacheItemPolicy(),
                DataRetrievalFunc = retrievalFixture.DataRetrievalFunc
            };
            sut.GetAsync(key).Wait();

            // act
            var removed = sut.RemoveFromCacheAsync(key).Result;

            // assert
            removed.Should().BeTrue();
            sut.ItemsInCache.Should().Be(0);
        }

        [Test]
        public void RetrievingItemFromTheCacheReturnsTheCorrectItem()
        {
            // arrange
            const string key = "test";
            var cacheItemDummy = new CacheItemFixture();
            var retrievalFixture = new RetrievalFixture<CacheItemFixture>(cacheItemDummy);
            var sut = new MemoryCacheWrapper<CacheItemFixture>
            {
                CachePolicy = new CacheItemPolicy(),
                DataRetrievalFunc = retrievalFixture.DataRetrievalFunc
            };
            var firstRetrieval = sut.GetAsync(key).Result;

            // act
            var secondRetrieval = sut.GetAsync(key).Result;

            // assert
            firstRetrieval.Should().Be(secondRetrieval);
        }

        #endregion

        #region Private Methods

        private async Task<int> DataRetrievalFunc(string s)
        {
            await Task.Delay(10);
            return 0;
        }

        private async Task<int> SecondRetrievalFunc(string s)
        {
            await Task.Delay(10);
            return 1;
        }

        #endregion

        private class CacheItemFixture
        { }

        private class RetrievalFixture<T>
        {
            #region Constructor

            public RetrievalFixture(T returnValue)
            {
                _returnValue = returnValue;
            }

            #endregion

            #region Properties

            public int CallCount { get; private set; }

            public bool WasCalled { get; private set; }

            #endregion

            #region Public Methods

            public async Task<T> DataRetrievalFunc(string s)
            {
                await Task.Delay(10);
                WasCalled = true;
                CallCount++;
                return _returnValue;
            }

            #endregion

            #region Variables

            private readonly T _returnValue;

            #endregion
        }
    }
}