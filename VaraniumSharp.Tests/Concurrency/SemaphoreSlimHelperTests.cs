using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VaraniumSharp.Concurrency;
using Xunit;

namespace VaraniumSharp.Tests.Concurrency
{
    public class SemaphoreSlimHelperTests
    {
        [Fact]
        public async Task IfSemaphoreDoesNotExistOneIsCreated()
        {
            // arrange
            const int key = 1;
            var sut = new SemaphoreSlimHelper<int>();

            // act
            var semaphore = await sut.AcquireSemaphoreSlimAsync(key);

            // assert
            sut.NumberOfSemaphoresIssued.Should().Be(1);
            semaphore.Dispose();
        }

        [Fact]
        public void IfSemaphoreDoesNotExistOneWillBeCreatedNonAsync()
        {
            // arrange
            const int key = 1;
            var sut = new SemaphoreSlimHelper<int>();

            // act
            var semaphore = sut.AcquireSemaphoreSlim(key);

            // assert
            sut.NumberOfSemaphoresIssued.Should().Be(1);
            semaphore.Dispose();
        }

        [Fact]
        public async Task IfSemaphoreIsAlreadyInUsedNextCallBlocks()
        {
            // arrange
            const int key = 1;
            var sut = new SemaphoreSlimHelper<int>();
            await sut.AcquireSemaphoreSlimAsync(key);

            // act
            var secondSemaphore = sut.AcquireSemaphoreSlimAsync(key);

            // assert
            secondSemaphore.Status.Should().Be(TaskStatus.WaitingForActivation);
            sut.NumberOfSemaphoresIssued.Should().Be(1);
        }

        [Fact]
        public async Task IfSemaphoreIsDisposedAndThereAreNoWaitingMethodsTheEntryIsRemovedFromTheDictionary()
        {
            // arrange
            const int key = 1;
            var sut = new SemaphoreSlimHelper<int>();

            var semaphore = await sut.AcquireSemaphoreSlimAsync(key);

            // act
            semaphore.Dispose();

            // assert
            sut.NumberOfSemaphoresIssued.Should().Be(0);
        }

        [Fact]
        public async Task IfSemaphoreIsDisposedItIsCorrectlyReleased()
        {
            // arrange
            const int key = 1;
            var sut = new SemaphoreSlimHelper<int>();
            var semaphore = await sut.AcquireSemaphoreSlimAsync(key);
            var secondSemaphore = sut.AcquireSemaphoreSlimAsync(key);

            // act
            semaphore.Dispose();
            Thread.Sleep(250);

            // assert
            secondSemaphore.Status.Should().Be(TaskStatus.RanToCompletion);
        }
    }
}