using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using VaraniumSharp.Collections;
using VaraniumSharp.Interfaces.Collections;
using VaraniumSharp.Tests.Helpers;
using Xunit;

namespace VaraniumSharp.Tests.Collections
{
    public class PriorityCollectionTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void CopyingToArrayWorksCorrectly(int startIndex)
        {
            // arrange
            var itemDummy = PriorityItemHelper.CreatePriorityMock(1);
            var higherPriorityDummy = PriorityItemHelper.CreatePriorityMock(0);
            var arr = new IPriorityItem[3];

            var sut = new PriorityCollection<IPriorityItem>();
            sut.TryAdd(itemDummy.Object);
            sut.TryAdd(higherPriorityDummy.Object);

            // act
            sut.CopyTo(arr, startIndex);

            // assert
            for (var r = 0; r < startIndex; r++)
            {
                arr[r].Should().BeNull();
            }
            arr[startIndex].Should().Be(higherPriorityDummy.Object);
            arr[startIndex + 1].Should().Be(itemDummy.Object);
        }

        [Fact]
        public void AddingPrioritylessItemToEmptyCollectionDoesNotThrowAnException()
        {
            // arrange
            var unprioritizedDummy = PriorityItemHelper.CreatePriorityMock(null);
            unprioritizedDummy.SetupAllProperties();

            var sut = new PriorityCollection<IPriorityItem>();
            var act = new Action(() => sut.TryAdd(unprioritizedDummy.Object));

            // act
            // assert
            act.Should().NotThrow<InvalidOperationException>();
        }

        [Fact]
        public void AddingPrioritylessItemWillAddItToTheEndOfTheCollection()
        {
            // arrange
            var itemDummy = PriorityItemHelper.CreatePriorityMock(1);
            var unprioritizedDummy = PriorityItemHelper.CreatePriorityMock(null);
            unprioritizedDummy.SetupAllProperties();

            var sut = new PriorityCollection<IPriorityItem>();
            sut.TryAdd(itemDummy.Object);

            // act
            var result = sut.TryAdd(unprioritizedDummy.Object);

            // assert
            result.Should().BeTrue();
            sut.AsEnumerable().Last().Should().Be(unprioritizedDummy.Object);
            unprioritizedDummy.Object.Priority.Should().Be(1);
        }

        [Fact]
        public void AddingSecondItemToCollectionAddsItInTheCorrectPosition()
        {
            // arrange
            var itemDummy = PriorityItemHelper.CreatePriorityMock(1);
            var higherPriorityDummy = PriorityItemHelper.CreatePriorityMock(0);

            var sut = new PriorityCollection<IPriorityItem>();
            sut.TryAdd(itemDummy.Object);

            // act
            var result = sut.TryAdd(higherPriorityDummy.Object);

            // assert
            result.Should().BeTrue();
            sut.AsEnumerable().First().Should().Be(higherPriorityDummy.Object);
        }

        [Fact]
        public void AddingSingleItemToCollectionWorksCorrectly()
        {
            // arrange
            var itemDummy = PriorityItemHelper.CreatePriorityMock(1);

            var sut = new PriorityCollection<IPriorityItem>();

            // act
            var result = sut.TryAdd(itemDummy.Object);

            // assert
            result.Should().BeTrue();
            sut.AsEnumerable().First().Should().Be(itemDummy.Object);
        }

        [Fact]
        public void AdjustingItemPriorityMovesTheItemToTheCorrectLocation()
        {
            // arrange
            var itemDummy = PriorityItemHelper.CreatePriorityMock(1);
            var higherPriorityDummy = PriorityItemHelper.CreatePriorityMock(0);

            var sut = new PriorityCollection<IPriorityItem>();
            sut.TryAdd(itemDummy.Object);
            sut.TryAdd(higherPriorityDummy.Object);

            higherPriorityDummy
                .Setup(t => t.Priority)
                .Returns(2);

            // act
            higherPriorityDummy.Raise(t => t.PriorityChanged += null, new EventArgs());

            // assert
            sut.AsEnumerable().Last().Should().Be(higherPriorityDummy.Object);
        }

        [Fact]
        public void ArraySizeIsCheckedCorrectlyForCopying()
        {
            // arrange
            var itemDummy = PriorityItemHelper.CreatePriorityMock(1);
            var higherPriorityDummy = PriorityItemHelper.CreatePriorityMock(0);
            var arr = new IPriorityItem[1];

            var sut = new PriorityCollection<IPriorityItem>();
            sut.TryAdd(itemDummy.Object);
            sut.TryAdd(higherPriorityDummy.Object);
            var act = new Action(() => sut.CopyTo(arr, 0));

            // act
            // assert
            act.Should().Throw<ArgumentException>("Target array is too small to copy items");
        }

        [Fact]
        public void CastingToIEnumerableAndEnumeratingTheCollectionWorksCorrectly()
        {
            // arrange
            var itemDummy = PriorityItemHelper.CreatePriorityMock(1);
            var higherPriorityDummy = PriorityItemHelper.CreatePriorityMock(0);

            var sut = new PriorityCollection<IPriorityItem>();
            sut.TryAdd(itemDummy.Object);
            sut.TryAdd(higherPriorityDummy.Object);

            // act
            var enumerator = ((IEnumerable)sut).GetEnumerator();

            // assert
            enumerator.MoveNext();
            enumerator.Current.Should().Be(higherPriorityDummy.Object);
        }

        [Fact]
        public void CollectionIsSynchronized()
        {
            // arrange
            // act
            var result = new PriorityCollection<IPriorityItem>().IsSynchronized;

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void EnumeratingCollectionWorksCorrectly()
        {
            // arrange
            var itemDummy = PriorityItemHelper.CreatePriorityMock(1);
            var higherPriorityDummy = PriorityItemHelper.CreatePriorityMock(0);

            var sut = new PriorityCollection<IPriorityItem>();
            sut.TryAdd(itemDummy.Object);
            sut.TryAdd(higherPriorityDummy.Object);

            // act
            // assert
            var counter = 0;
            foreach (var item in sut)
            {
                item
                    .Should()
                    .Be(counter == 0 ? higherPriorityDummy.Object : itemDummy.Object);
                counter++;
            }
        }

        [Fact]
        public void IndexOutOfRangeIsCheckedCorrectly()
        {
            // arrange
            var itemDummy = PriorityItemHelper.CreatePriorityMock(1);
            var higherPriorityDummy = PriorityItemHelper.CreatePriorityMock(0);
            var arr = new IPriorityItem[1];

            var sut = new PriorityCollection<IPriorityItem>();
            sut.TryAdd(itemDummy.Object);
            sut.TryAdd(higherPriorityDummy.Object);
            var act = new Action(() => sut.CopyTo(arr, -1));

            // act
            // assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void TakingAnItemFromTheCollectionAlwaysReturnsTheItemWithTheHighestPriority()
        {
            // arrange
            var itemDummy = PriorityItemHelper.CreatePriorityMock(1);
            var higherPriorityDummy = PriorityItemHelper.CreatePriorityMock(0);

            var sut = new PriorityCollection<IPriorityItem>();
            sut.TryAdd(itemDummy.Object);
            sut.TryAdd(higherPriorityDummy.Object);

            IPriorityItem retrievedItem;

            // act
            var result = sut.TryTake(out retrievedItem);

            // assert
            result.Should().BeTrue();
            retrievedItem.Should().Be(higherPriorityDummy.Object);
        }

        [Fact]
        [SuppressMessage("ReSharper", "NotAccessedVariable", Justification = "We need to remove the item from the sut event though we don't do anything with it")]
        public void TakingAnItemFromTheCollectionCorrectlyDetachesItsEvent()
        {
            // arrange
            var itemDummy = PriorityItemHelper.CreatePriorityMock(1);

            var sut = new PriorityCollection<IPriorityItem>();
            sut.TryAdd(itemDummy.Object);

            IPriorityItem item;
            sut.TryTake(out item);

            // act
            itemDummy.Raise(t => t.PriorityChanged += null, new EventArgs());

            // assert
            sut.Count.Should().Be(0);
        }

        [Fact]
        public void ToArrayCopiesItemsInCollectionToANewArray()
        {
            // arrange
            var itemDummy = PriorityItemHelper.CreatePriorityMock(1);
            var higherPriorityDummy = PriorityItemHelper.CreatePriorityMock(0);

            var sut = new PriorityCollection<IPriorityItem>();
            sut.TryAdd(itemDummy.Object);
            sut.TryAdd(higherPriorityDummy.Object);

            // act
            var array = sut.ToArray();

            // assert
            array.Length.Should().Be(2);
            array.First().Should().Be(higherPriorityDummy.Object);
        }

        [Fact]
        public void TryingToTakeAnItemWhenTheCollectionIsEmptyReturnsFalse()
        {
            // arrange
            var sut = new PriorityCollection<IPriorityItem>();
            IPriorityItem retrievedItem;

            // act
            var result = sut.TryTake(out retrievedItem);

            // assert
            result.Should().BeFalse();
            retrievedItem.Should().BeNull();
        }
    }
}