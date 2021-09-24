using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using VaraniumSharp.Interfaces.Caching;

namespace VaraniumSharp.Caching
{
    /// <summary>
    /// Creates and initializes instances of <see cref="IMemoryCacheWrapper{T}"/>
    /// <remarks>
    /// This class should not be used when Dependency Injection is used as it directly new up instances of <see cref="IMemoryCacheWrapper{T}"/> rather than resolving them from the container.
    /// For a DI compatible solution look for a factory implementation in a Ring 2 library like VaraniumSharp.Initiator
    /// </remarks>
    /// </summary>
    public class BasicMemoryCacheWrapperFactory<T> : IMemoryCacheWrapperFactory<T> where T: class
    {
        #region Public Methods

        /// <summary>
        /// Creates a new instance of the MemoryCacheWrapper and initialize it
        /// </summary>
        /// <typeparam name="T">Type of items that will be stored in the cache</typeparam>
        /// <param name="policy">Cache eviction policy</param>
        /// <param name="dataRetrievalFunc">Function used to retrieve items if they are not in the cache</param>
        /// <returns>Initialized instance of MemoryCacheWrapper</returns>
        public virtual IMemoryCacheWrapper<T> Create(CacheItemPolicy policy, Func<string, Task<T>> dataRetrievalFunc)
        {
            return new MemoryCacheWrapper<T>
            {
                CachePolicy = policy,
                DataRetrievalFunc = dataRetrievalFunc
            };
        }

        /// <summary>
        /// Creates a new instance of the MemoryCacheWrapper and initialize it with a default cache policy.
        /// The default policy uses SlidingExpiration with a duration 5 minute
        /// </summary>
        /// <typeparam name="T">Type of items that will be stored in the cache</typeparam>
        /// <param name="dataRetrievalFunc">Function used to retrieve items if they are not in the cache</param>
        /// <returns>Initialized instance of MemoryCacheWrapper</returns>
        public virtual IMemoryCacheWrapper<T> CreateWithDefaultSlidingPolicy(Func<string, Task<T>> dataRetrievalFunc)
        {
            var defaultPolicy = new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromMinutes(DefaultTimeoutInMinutes)
            };

            return Create(defaultPolicy, dataRetrievalFunc);
        }

        #endregion

        #region Variables

        private const int DefaultTimeoutInMinutes = 5;

        #endregion
    }
}