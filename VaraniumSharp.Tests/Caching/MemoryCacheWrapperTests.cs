using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using VaraniumSharp.Caching;
using VaraniumSharp.Tests.Fixtures;
using Xunit;

namespace VaraniumSharp.Tests.Caching
{
    public class MemoryCacheWrapperTests
    {
        private async Task<CacheEntryFixture> DataRetrievalFunc(string s)
        {
            await Task.Delay(10);
            return new CacheEntryFixture
            {
                Id = int.Parse(s)
            };
        }

        private async Task<CacheEntryFixture> SecondRetrievalFunc(string s)
        {
            await Task.Delay(10);
            return new CacheEntryFixture
            {
                Id = int.Parse(s)
            };
        }

        private class CacheItemFixture
        {}

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

        private class BlockingRetrievalFixture<T>
        {
            #region Constructor

            public BlockingRetrievalFixture(T returnValue)
            {
                _returnValue = returnValue;
            }

            #endregion

            #region Properties

            public SemaphoreSlim SemaphoreToBlock { get; } = new(1);

            #endregion

            #region Public Methods

            public async Task<T> DataRetrievalFunc(string s)
            {
                try
                {
                    await SemaphoreToBlock.WaitAsync();
                    return _returnValue;
                }
                finally
                {
                    SemaphoreToBlock.Release();
                }
            }

            #endregion

            #region Variables

            private readonly T _returnValue;

            #endregion
        }

        [Fact]
        public void AddItemToTheCache()
        {
            // arrange
            const string key = "12";
            var returnValue = new CacheEntryFixture
            {
                Id = int.Parse(key)
            };
            var retrievalFixture = new RetrievalFixture<CacheEntryFixture>(returnValue);
            var sut = new MemoryCacheWrapper<CacheEntryFixture>
            {
                CachePolicy = new CacheItemPolicy(),
                DataRetrievalFunc = retrievalFixture.DataRetrievalFunc
            };

            // act
            var result = sut.GetAsync(key).Result;

            // assert
            sut.CacheRequests.Should().Be(1);
            sut.CacheHits.Should().Be(0);
            sut.ItemsInCache.Should().Be(1);
            sut.AverageSingleRetrievalTime.TotalMilliseconds.Should().BeGreaterThan(0);
            retrievalFixture.WasCalled.Should().BeTrue();
            result.Should().Be(returnValue);
        }

        [Fact]
        public void AttemptingToSetTheBatchRetrievalASecondTimeThrowsAnException()
        {
            // arrange
            var dummyFunc = new Func<List<string>, Task<Dictionary<string, CacheEntryFixture>>>(async _ =>
            {
                await Task.Delay(1);
                return new Dictionary<string, CacheEntryFixture>();
            });
            var newDummyFunc = new Func<List<string>, Task<Dictionary<string, CacheEntryFixture>>>(async _ =>
            {
                await Task.Delay(1);
                return new Dictionary<string, CacheEntryFixture>();
            });

            var sut = new MemoryCacheWrapper<CacheEntryFixture> { BatchRetrievalFunc = dummyFunc };

            var act = new Action(() => sut.BatchRetrievalFunc = newDummyFunc);

            // act
            // assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void AttemptToRemoveItemThatIsNotInTheCache()
        {
            // arrange
            const string key = "12";
            var returnValue = new CacheEntryFixture
            {
                Id = int.Parse(key)
            };
            var retrievalFixture = new RetrievalFixture<CacheEntryFixture>(returnValue);
            var sut = new MemoryCacheWrapper<CacheEntryFixture>
            {
                CachePolicy = new CacheItemPolicy(),
                DataRetrievalFunc = retrievalFixture.DataRetrievalFunc
            };

            // act
            var removed = sut.RemoveFromCacheAsync(key).Result;

            // assert
            removed.Should().BeFalse();
        }

        [Fact]
        public async Task BatchRequestingItemsReturnsAllEntries()
        {
            // arrange
            const string key = "12";
            var returnValue = new CacheEntryFixture
            {
                Id = int.Parse(key)
            };
            var requestedKeys = new List<string>();
            var dummyFunc = new Func<List<string>, Task<Dictionary<string, CacheEntryFixture>>>(async list =>
            {
                await Task.Delay(1);
                requestedKeys = list;
                return new Dictionary<string, CacheEntryFixture>
                {
                    { key, new CacheEntryFixture
                        {
                            Id = 1
                        }
                    }
                };
            });
            var retrievalFixture = new RetrievalFixture<CacheEntryFixture>(returnValue);
            var sut = new MemoryCacheWrapper<CacheEntryFixture>
            {
                CachePolicy = new CacheItemPolicy(),
                DataRetrievalFunc = retrievalFixture.DataRetrievalFunc,
                BatchRetrievalFunc = dummyFunc
            };

            // act
            var entriesAdded = await sut.GetAsync(new List<string> { key });

            // assert
            sut.ItemsInCache.Should().Be(1);
            entriesAdded.Count.Should().Be(1);
            requestedKeys.Count.Should().Be(1);
            sut.CacheHits.Should().Be(0);
            sut.CacheRequests.Should().Be(1);
            sut.AverageBatchSize.Should().Be(1);
            sut.AverageBatchRetrievalTime.TotalMilliseconds.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task BatchRetrievalAddsAllBatchEntriesToTheCache()
        {
            // arrange
            const string key = "12";
            var returnValue = new CacheEntryFixture
            {
                Id = int.Parse(key)
            };
            var requestedKeys = new List<string>();
            var dummyFunc = new Func<List<string>, Task<Dictionary<string, CacheEntryFixture>>>(async list =>
            {
                await Task.Delay(1);
                requestedKeys = list;
                return new Dictionary<string, CacheEntryFixture>
                {
                    { key, new CacheEntryFixture
                        {
                            Id = 1
                        }
                    }
                };
            });
            var retrievalFixture = new RetrievalFixture<CacheEntryFixture>(returnValue);
            var sut = new MemoryCacheWrapper<CacheEntryFixture>
            {
                CachePolicy = new CacheItemPolicy(),
                DataRetrievalFunc = retrievalFixture.DataRetrievalFunc,
                BatchRetrievalFunc = dummyFunc
            };

            // act
            var entriesAdded = await sut.BatchAddToCacheAsync(new List<string> { key });

            // assert
            entriesAdded.Should().Be(1);
            requestedKeys.Count.Should().Be(1);
        }

        [Fact]
        public void CachePolicyCanOnlyBeSetOnce()
        {
            // arrange
            var sut = new MemoryCacheWrapper<CacheEntryFixture>
            {
                CachePolicy = new CacheItemPolicy()
            };

            var act = new Action(() => sut.CachePolicy = new CacheItemPolicy());

            // act
            // assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public async Task CannotBatchRetrieveCacheItemIfPolicyIsNotSet()
        {
            // arrange
            const string key = "12";
            const string key2 = "22";
            var keyList = new List<string> { key, key2 };
            var sut = new MemoryCacheWrapper<CacheEntryFixture>();
            var act = new Func<Task>(async () => await sut.GetAsync(keyList));

            // act
            // assert
            await act.Should().ThrowAsync<InvalidOperationException>("Cache Policy has not been set");
        }

        [Fact]
        public async Task CannotBatchRetrieveCacheItemIfRetrievalFuncIsNotSet()
        {
            // arrange
            const string key = "12";
            const string key2 = "22";
            var keyList = new List<string> { key, key2 };
            var sut = new MemoryCacheWrapper<CacheEntryFixture>
            {
                CachePolicy = new CacheItemPolicy()
            };
            var act = new Func<Task>(async () => await sut.GetAsync(keyList));

            // act
            // assert
            await act.Should().ThrowAsync<InvalidOperationException>("DataRetrievalFunc has not been set");
        }

        [Fact]
        public void CannotRetrieveCacheItemIfPolicyIsNotSet()
        {
            // arrange
            const string key = "12";
            var sut = new MemoryCacheWrapper<CacheEntryFixture>();
            var act = new Action(() => sut.GetAsync(key).Wait());

            // act
            // assert
            act.Should().Throw<InvalidOperationException>("Cache Policy has not been set");
        }

        [Fact]
        public void CannotRetrieveCacheItemIfRetrievalFuncIsNotSet()
        {
            // arrange
            const string key = "12";
            var sut = new MemoryCacheWrapper<CacheEntryFixture>
            {
                CachePolicy = new CacheItemPolicy()
            };
            var act = new Action(() => sut.GetAsync(key).Wait());

            // act
            // assert
            act.Should().Throw<InvalidOperationException>("DataRetrievalFunc has not been set");
        }

        [Fact]
        public async Task CheckingIfCacheContainsKeyWhenTheCacheContainsTheItemReturnsTrue()
        {
            // arrange
            const string key = "12";
            var cacheItemDummy = new CacheItemFixture();
            var retrievalFixture = new RetrievalFixture<CacheItemFixture>(cacheItemDummy);
            var sut = new MemoryCacheWrapper<CacheItemFixture>
            {
                CachePolicy = new CacheItemPolicy(),
                DataRetrievalFunc = retrievalFixture.DataRetrievalFunc
            };
            await sut.GetAsync(key);

            // act
            var result = await sut.ContainsKeyAsync(key);

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CheckingIfCacheContainsKeyWhenTheCacheDoesNotContainTheItemReturnsFalse()
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

            // act
            var result = await sut.ContainsKeyAsync(key);

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void DataRetrievalFuncCanOnlyBeSetOnce()
        {
            // arrange
            var sut = new MemoryCacheWrapper<CacheEntryFixture>
            {
                DataRetrievalFunc = DataRetrievalFunc
            };

            var act = new Action(() => sut.DataRetrievalFunc = SecondRetrievalFunc);

            // act
            // assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void IfAnErrorOccursDuringBatchRetrievalAnExceptionIsThrown()
        {
            // arrange
            const string key = "12";
            var returnValue = new CacheEntryFixture
            {
                Id = int.Parse(key)
            };
            var dummyFunc = new Func<List<string>, Task<Dictionary<string, CacheEntryFixture>>>(async list =>
            {
                await Task.Delay(1);
                throw new Exception();
            });
            var retrievalFixture = new RetrievalFixture<CacheEntryFixture>(returnValue);
            var sut = new MemoryCacheWrapper<CacheEntryFixture>
            {
                CachePolicy = new CacheItemPolicy(),
                DataRetrievalFunc = retrievalFixture.DataRetrievalFunc,
                BatchRetrievalFunc = dummyFunc
            };

            var act = new Action(() => sut.BatchAddToCacheAsync(new List<string> { key }).Wait());

            // act
            // assert
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void IfBatchRetrievalFuncIsNotSetAttemptingToBatchAddThrowsAnException()
        {
            // arrange
            var sut = new MemoryCacheWrapper<CacheEntryFixture>();

            var act = new Action(() => sut.BatchAddToCacheAsync(new List<string>()).Wait());

            // act
            // assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public async Task IfKeyIsAlreadyInTheCacheBatchAddingWillNotAddItAgain()
        {
            // arrange
            const string key = "1";
            var returnValue = new CacheEntryFixture
            {
                Id = int.Parse(key)
            };
            var requestedKeys = new List<string>();
            var dummyFunc = new Func<List<string>, Task<Dictionary<string, CacheEntryFixture>>>(async list =>
            {
                await Task.Delay(1);
                requestedKeys = list;
                return new Dictionary<string, CacheEntryFixture>();
            });
            var retrievalFixture = new RetrievalFixture<CacheEntryFixture>(returnValue);
            var sut = new MemoryCacheWrapper<CacheEntryFixture>
            {
                CachePolicy = new CacheItemPolicy(),
                DataRetrievalFunc = retrievalFixture.DataRetrievalFunc,
                BatchRetrievalFunc = dummyFunc
            };
            var _ = await sut.GetAsync(key);

            // act
            await sut.BatchAddToCacheAsync(new List<string> { key });

            // assert
            requestedKeys.Count.Should().Be(0);
        }

        [Fact]
        public async Task IfSemaphoreIsBlockedAndTimeoutExpiresContainsKeyThrowsATimeoutException()
        {
            // arrange
            const string key = "test";
            var timeout = new TimeSpan(0, 0, 2);
            var cacheItemDummy = new CacheItemFixture();
            var retrievalFixture = new BlockingRetrievalFixture<CacheItemFixture>(cacheItemDummy);
            var sut = new MemoryCacheWrapper<CacheItemFixture>
            {
                CachePolicy = new CacheItemPolicy(),
                DataRetrievalFunc = retrievalFixture.DataRetrievalFunc
            };
            await retrievalFixture.SemaphoreToBlock.WaitAsync();
#pragma warning disable 4014 //Do not await, otherwise the test will not work correctly
            sut.GetAsync(key);
#pragma warning restore 4014

            var act = new Action(() => sut.ContainsKeyAsync(key, timeout).Wait());

            // act
            // assert
            act.Should().Throw<TimeoutException>();

            retrievalFixture.SemaphoreToBlock.Release();
        }

        [Fact]
        public async Task IfSemaphoreIsNotBlockedTheExceptionIsNotThrown()
        {
            // arrange
            const string key = "test";
            var timeout = new TimeSpan(0, 0, 2);
            var cacheItemDummy = new CacheItemFixture();
            var retrievalFixture = new BlockingRetrievalFixture<CacheItemFixture>(cacheItemDummy);
            var sut = new MemoryCacheWrapper<CacheItemFixture>
            {
                CachePolicy = new CacheItemPolicy(),
                DataRetrievalFunc = retrievalFixture.DataRetrievalFunc
            };
            await sut.GetAsync(key);

            var act = new Action(() => sut.ContainsKeyAsync(key, timeout).Wait());

            // act
            // assert
            act.Should().NotThrow<TimeoutException>();
        }

        [Fact]
        public void MultipleRetrievalsOnlyHitTheRetrievalFunctionOnce()
        {
            // arrange
            const string key = "12";
            var returnValue = new CacheEntryFixture
            {
                Id = int.Parse(key)
            };
            var retrievalFixture = new RetrievalFixture<CacheEntryFixture>(returnValue);
            var sut = new MemoryCacheWrapper<CacheEntryFixture>
            {
                CachePolicy = new CacheItemPolicy(),
                DataRetrievalFunc = retrievalFixture.DataRetrievalFunc
            };

            // act
            var tasks = new Task[10];
            for (var r = 0; r < 10; r++)
            {
                tasks[r] = sut.GetAsync(key);
            }

            Task.WaitAll(tasks);

            // assert
            retrievalFixture.CallCount.Should().Be(1);
            sut.CacheRequests.Should().Be(10);
            sut.CacheHits.Should().Be(9);
        }

        [Fact]
        public void RemoveItemFromCache()
        {
            // arrange
            const string key = "12";
            var returnValue = new CacheEntryFixture
            {
                Id = int.Parse(key)
            };
            var retrievalFixture = new RetrievalFixture<CacheEntryFixture>(returnValue);
            var sut = new MemoryCacheWrapper<CacheEntryFixture>
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

        [Fact]
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

        [Fact]
        public async Task RetrievingKeysAlreadyInCacheReturnsOnlyCachedEntries()
        {
            // arrange
            const string key = "12";
            const string key2 = "55";
            var returnValue = new CacheEntryFixture
            {
                Id = int.Parse(key)
            };
            var retrievalFixture = new RetrievalFixture<CacheEntryFixture>(returnValue);
            var sut = new MemoryCacheWrapper<CacheEntryFixture>
            {
                CachePolicy = new CacheItemPolicy(),
                DataRetrievalFunc = retrievalFixture.DataRetrievalFunc
            };

            await sut.GetAsync(key);

            // act
            var result = sut.GetForKeysAlreadyInCache(new[] { key, key2 });

            // assert
            result.Count.Should().Be(1);
            result.First().Id.Should().Be(12);
        }
    }
}