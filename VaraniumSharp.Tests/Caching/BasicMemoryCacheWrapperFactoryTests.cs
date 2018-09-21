using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using FluentAssertions;
using VaraniumSharp.Caching;
using Xunit;

namespace VaraniumSharp.Tests.Caching
{
    public class BasicMemoryCacheWrapperFactoryTests
    {
        private static async Task<int> DataRetrievalFunc(string s)
        {
            await Task.Delay(10);
            return 0;
        }

        [Fact]
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

        [Fact]
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
    }
}