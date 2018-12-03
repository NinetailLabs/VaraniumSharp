using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VaraniumSharp.Concurrency;
using Xunit;

namespace VaraniumSharp.Tests.Concurrency
{
    public class StaTaskTests
    {
        [Fact]
        public void CancellationTokenIsPassedToTheTask()
        {
            // arrange
            var wasExecuted = false;
            var act = new Action(() => { wasExecuted = true; });
            var cancellationToken = new CancellationToken(true);
            var task = StaTask.Run(act, cancellationToken);
            var taskAct = new Action(() => task.Start());

            // act
            // assert
            wasExecuted.Should().BeFalse();
            taskAct.Should().Throw<Exception>();
            task.Status.Should().Be(TaskStatus.Canceled);
        }

        [Fact]
        public async Task TaskIsCorrectlyScheduledOnStaThread()
        {
            // arrange
            ApartmentState? receivedState = null;
            var act = new Action(() => { receivedState = Thread.CurrentThread.GetApartmentState(); });

            // act
            await StaTask.Run(act);

            // assert
            receivedState.Should().NotBeNull();
            receivedState.Should().Be(ApartmentState.STA);
        }
    }
}