using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace VaraniumSharp.Concurrency
{
    /// <summary>
    /// Semaphore helper that wraps regular try-finally logic into a disposable for easier usage
    /// </summary>
    /// <typeparam name="T">Type of key to use for the Semaphore collection</typeparam>
    public sealed class SemaphoreSlimHelper<T>
    {
        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SemaphoreSlimHelper()
        {
            _semaphoreDictionary = new ConcurrentDictionary<T, SemaphoreSlim>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The Number of SemaphoreSlim that are currently in the Dictionary
        /// </summary>
        public int NumberOfSemaphoresIssued => _semaphoreDictionary.Count;

        #endregion

        #region Public Methods

        /// <summary>
        /// Attempt to acquire a <see cref="SemaphoreSlim"/> for a specific key.
        /// If the Semaphore has already been acquired the call will block until the Semaphore is available
        /// </summary>
        /// <param name="key">Key for the Semaphore</param>
        /// <returns>Disposable SemaphoreSlim wrapper</returns>
        public DisposableSemaphoreSlim<T> AcquireSemaphoreSlim(T key)
        {
            var semaphore = _semaphoreDictionary.GetOrAdd(key, arg => new SemaphoreSlim(1));
            semaphore.Wait();

            var disposableSemaphore = new DisposableSemaphoreSlim<T>(key, semaphore, DisposalAction);
            return disposableSemaphore;
        }

        /// <summary>
        /// Attempt to acquire a <see cref="SemaphoreSlim"/> for a specific key.
        /// If the Semaphore has already been acquired the call will block until the Semaphore is available
        /// </summary>
        /// <param name="key">Key for the Semaphore</param>
        /// <returns>Disposable SemaphoreSlim wrapper</returns>
        public async Task<DisposableSemaphoreSlim<T>> AcquireSemaphoreSlimAsync(T key)
        {
            var semaphore = _semaphoreDictionary.GetOrAdd(key, arg => new SemaphoreSlim(1));
            await semaphore.WaitAsync();

            var disposableSemaphore = new DisposableSemaphoreSlim<T>(key, semaphore, DisposalAction);
            return disposableSemaphore;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Action that is invoked when a <see cref="DisposableSemaphoreSlim{T}"/> is disposed so dictionary cleaning can be done if required
        /// </summary>
        /// <param name="entryKey">Key of the Semaphore to check</param>
        private void DisposalAction(T entryKey)
        {
            _semaphoreDictionary.TryGetValue(entryKey, out var semaphore);
            if (semaphore?.CurrentCount == 0)
            {
                _semaphoreDictionary.TryRemove(entryKey, out _);
            }
        }

        #endregion

        #region Variables

        /// <summary>
        /// Dictionary containing SemaphoreSlim entries
        /// </summary>
        private readonly ConcurrentDictionary<T, SemaphoreSlim> _semaphoreDictionary;

        #endregion
    }
}