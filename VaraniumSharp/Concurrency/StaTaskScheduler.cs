using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VaraniumSharp.Concurrency
{
    /// <inheritdoc />
    /// <summary>
    /// Code to help make a Task scheduler run in STA mode
    /// Code source: http://blogs.msdn.com/b/pfxteam/archive/2010/04/07/9990421.aspx
    /// </summary>
    public class StaTaskScheduler : TaskScheduler
    {
        #region Constructor

        /// <summary>
        /// Default Constructor will create the scheduler with the same number of threads as there are available processors in the system
        /// </summary>
        public StaTaskScheduler()
            : this(Environment.ProcessorCount)
        {
        }

        /// <summary>
        /// Construct with number of threads that should be used
        /// </summary>
        /// <param name="numberOfThreads">The number of STA threads to create in the pool</param>
        public StaTaskScheduler(int numberOfThreads)
        {
            Threads = numberOfThreads;
            if (numberOfThreads < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfThreads));
            }

            _tasks = new BlockingCollection<Task>();

            var threads = Enumerable.Range(0, numberOfThreads)
                .Select(i =>
                {
                    var thread = new Thread(() =>
                    {
                        foreach (var t in _tasks.GetConsumingEnumerable())
                        {
                            TryExecuteTask(t);
                        }
                    })
                    {
                        IsBackground = true
                    };
                    thread.SetApartmentState(ApartmentState.STA);
                    return thread;
                })
                .ToList();

            threads.ForEach(t => t.Start());
        }

        #endregion

        #region Properties

        /// <summary>
        /// Number of threads available in the scheduler
        /// </summary>
        public int Threads { get; }

        #endregion

        #region Private Methods

        /// <inheritdoc />
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _tasks.ToArray();
        }

        /// <inheritdoc />
        protected override void QueueTask(Task task)
        {
            _tasks.Add(task);
        }

        /// <inheritdoc />
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return Thread.CurrentThread.GetApartmentState() == ApartmentState.STA && TryExecuteTask(task);
        }

        #endregion

        #region Variables

        /// <summary>
        /// Collection of tasks that have been scheduled
        /// </summary>
        private readonly BlockingCollection<Task> _tasks;

        #endregion
    }
}