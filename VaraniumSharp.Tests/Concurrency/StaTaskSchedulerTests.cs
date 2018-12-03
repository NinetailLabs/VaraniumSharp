using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VaraniumSharp.Concurrency;
using Xunit;

namespace VaraniumSharp.Tests.Concurrency
{
    public class StaTaskSchedulerTests
    {
        private class SchedulerWrapper : StaTaskScheduler
        {
            #region Constructor

            public SchedulerWrapper(int threads)
                : base(threads)
            {
            }

            #endregion

            #region Public Methods

            public void AddTaskToQueue(Task taskToQueue)
            {
                QueueTask(taskToQueue);
            }

            public IEnumerable<Task> GetTasks()
            {
                return GetScheduledTasks();
            }

            #endregion
        }

        [Fact]
        public void ConstructingWithCustomNumberOfThreadsSetsTheCorrectNumberOfThreads()
        {
            // arrange
            const int threadsToUse = 2;

            // act
            var sut = new StaTaskScheduler(threadsToUse);

            // assert
            sut.Threads.Should().Be(threadsToUse);
        }

        [Fact]
        public void DefaultConstructorSetCorrectNumberOfThreads()
        {
            // arrange
            var expectedThreads = Environment.ProcessorCount;

            // act
            var sut = new StaTaskScheduler();

            // assert
            sut.Threads.Should().Be(expectedThreads);
        }

        [Fact(Skip = "Runner issue")]
        public void RunningTaskSynchronouslyWillCorrectlySwitchToStaThread()
        {
            // arrange
            ApartmentState? receivedState = null;
            var sut = new StaTaskScheduler(1);
            var act1 = new Action(() => { receivedState = Thread.CurrentThread.GetApartmentState(); });
            var task = new Task(act1);

            Thread.CurrentThread.GetApartmentState().Should().Be(ApartmentState.MTA);

            // act
            task.RunSynchronously(sut);

            // assert
            receivedState.Should().NotBeNull();
            receivedState.Should().Be(ApartmentState.STA);
        }

        [Fact]
        public void SettingThreadCountLessThanZeroThrowsAnArgumentOutOfRangeException()
        {
            // arrange
            var act = new Action(() => new StaTaskScheduler(0));

            // act
            // assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void TasksAreCorrectlyQueued()
        {
            // arrange
            var sut = new SchedulerWrapper(1);
            sut.AddTaskToQueue(new Task(() => { Thread.Sleep(5000); }));
            sut.AddTaskToQueue(new Task(() => { }));
            sut.AddTaskToQueue(new Task(() => { }));

            // act
            var tasks = sut.GetTasks();

            // assert
            tasks.Count().Should().BeGreaterOrEqualTo(1);
        }

        [Fact]
        public async Task ThreadIsCorrectlySetAsSta()
        {
            // arrange
            var sut = new StaTaskScheduler();
            ApartmentState? receivedState = null;
            var act = new Action(() =>
            {
                receivedState = Thread.CurrentThread.GetApartmentState();
            });

            // act
            await Task.Factory.StartNew(act, CancellationToken.None, TaskCreationOptions.None, sut);

            // assert
            receivedState.Should().NotBeNull();
            receivedState.Should().Be(ApartmentState.STA);
        }
    }
}