using System;
using System.Threading;
using System.Threading.Tasks;

namespace VaraniumSharp.Concurrency
{
    /// <summary>
    /// Helper for executing asynchronous operations on <see cref="ApartmentState.STA"/> threads
    /// </summary>
    public static class StaTask
    {
        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        static StaTask()
        {
            Scheduler = new StaTaskScheduler();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Queues an asynchronous action on a <see cref="ApartmentState.STA"/> thread and returns a <see cref="Task"/> that represents that work
        /// </summary>
        /// <param name="action">The work to execute asynchronously</param>
        /// <returns>Representation of asynchronous work</returns>
        public static Task Run(Action action)
        {
            return Run(action, CancellationToken.None);
        }

        /// <summary>
        /// Queues an asynchronous action on a <see cref="ApartmentState.STA"/> thread and returns a <see cref="Task"/> that represents that work
        /// </summary>
        /// <param name="action">The work to execute asynchronously</param>
        /// <param name="cancellationToken">Token that can be used to cancel the execution of the action</param>
        /// <returns>Representation of asynchronous work</returns>
        public static Task Run(Action action, CancellationToken cancellationToken)
        {
            return new TaskFactory().StartNew(action, cancellationToken, TaskCreationOptions.None, Scheduler);
        }

        #endregion

        #region Variables

        /// <summary>
        /// StaTaskScheduler instance
        /// </summary>
        private static readonly StaTaskScheduler Scheduler;

        #endregion
    }
}