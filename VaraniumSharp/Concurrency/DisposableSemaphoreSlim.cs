using System;
using System.Threading;

namespace VaraniumSharp.Concurrency
{
    /// <summary>
    /// Disposable SemaphoreSlim wrapper for use with <see cref="SemaphoreSlimHelper{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DisposableSemaphoreSlim<T> : IDisposable
    {
        #region Constructor

        /// <summary>
        /// Construct with required values
        /// </summary>
        /// <param name="key">Key the Semaphore was acquired with</param>
        /// <param name="semaphore">The SemaphoreSlim for the instance</param>
        /// <param name="disposalAction">Action that will be invoked when the instance is disposed</param>
        internal DisposableSemaphoreSlim(T key, SemaphoreSlim semaphore, Action<T> disposalAction)
        {
            _key = key;
            _semaphore = semaphore;
            _disposalAction = disposalAction;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public void Dispose()
        {
            _disposalAction.Invoke(_key);
            _semaphore.Release();
        }

        #endregion

        #region Variables

        /// <summary>
        /// Action that will be invoked on disposal to notify <see cref="SemaphoreSlimHelper{T}"/> of disposal
        /// </summary>
        private readonly Action<T> _disposalAction;

        /// <summary>
        /// Key the SemaphoreSlim was acquired with
        /// </summary>
        private readonly T _key;

        /// <summary>
        /// SemaphoreSlim for this instance
        /// </summary>
        private readonly SemaphoreSlim _semaphore;

        #endregion
    }
}