using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace VaraniumSharp.Interfaces.Caching
{
    /// <summary>
    /// Creates and initializes instances of <see cref="IMemoryCacheWrapper{T}"/>
    /// </summary>
    public interface IMemoryCacheWrapperFactory<T> where T: class
    {
        #region Public Methods

        /// <summary>
        /// Creates a new instance of the MemoryCacheWrapper and initialize it
        /// </summary>
        /// <typeparam name="T">Type of items that will be stored in the cache</typeparam>
        /// <param name="policy">Cache eviction policy</param>
        /// <param name="dataRetrievalFunc">Function used to retrieve items if they are not in the cache</param>
        /// <returns>Initialized instance of MemoryCacheWrapper</returns>
        IMemoryCacheWrapper<T> Create(CacheItemPolicy policy, Func<string, Task<T>> dataRetrievalFunc);

        /// <summary>
        /// Creates a new instance of the MemoryCacheWrapper and initialize it with a default cache policy.
        /// The default policy uses SlidingExpiration with a duration 5 minute
        /// </summary>
        /// <typeparam name="T">Type of items that will be stored in the cache</typeparam>
        /// <param name="dataRetrievalFunc">Function used to retrieve items if they are not in the cache</param>
        /// <returns>Initialized instance of MemoryCacheWrapper</returns>
        IMemoryCacheWrapper<T> CreateWithDefaultSlidingPolicy(Func<string, Task<T>> dataRetrievalFunc);

        #endregion
    }
}