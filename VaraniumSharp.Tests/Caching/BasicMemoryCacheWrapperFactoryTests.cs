using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using VaraniumSharp.Caching;

namespace VaraniumSharp.Tests.Caching
{
    public class BasicMemoryCacheWrapperFactoryTests
    {
        #region Public Methods

        [Test]
        public void CreateInstanceWithDefaultPolicy()
        {
            // arrange
            var func = new Func<string, Task<int>>(DataRetrievalFunc);
            var sut = new BasicMemoryCacheWrapperFactory();

            // act
            var cache = sut.CreateWithDefaultSlidingPolicy(func);

            // assert
            cache.Should().NotBeNull();
            cache.DataRetrievalFunc.Should().Be(func);
            cache.CachePolicy.SlidingExpiration.Minutes.Should().Be(5);
        }

        [Test]
        public void CreateInstanceWithPolicy()
        {
            // arrange
            var func = new Func<string, Task<int>>(DataRetrievalFunc);
            var policy = new CacheItemPolicy();
            var sut = new BasicMemoryCacheWrapperFactory();

            // act
            var cache = sut.Create(policy, func);

            // assert
            cache.Should().NotBeNull();
            cache.CachePolicy.Should().Be(policy);
            cache.DataRetrievalFunc.Should().Be(func);
        }

        #endregion

        #region Private Methods

        private static async Task<int> DataRetrievalFunc(string s)
        {
            await Task.Delay(10);
            return 0;
        }

        #endregion
    }
}