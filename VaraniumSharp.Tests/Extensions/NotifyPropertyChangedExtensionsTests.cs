using System.ComponentModel;
using FluentAssertions;
using VaraniumSharp.Attributes;
using VaraniumSharp.Extensions;
using Xunit;

#pragma warning disable 0067

namespace VaraniumSharp.Tests.Extensions
{
    public class NotifyPropertyChangedExtensionsTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ClassWithoutAttributeReturnsACompletedTask(bool startValue)
        {
            // arrange
            var classDummy = new NoAttributeFixture(startValue);

            // act
            var result = classDummy.BlockUntil();

            // assert
            result.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public void MultiPropertyClassWillBlockIfOnePropertyIsNotAtTheExpectedValue()
        {
            // arrange
            var multiDummy = new MultiAttributeFixture(false, true);

            // act
            var result = multiDummy.BlockUntil();

            // assert
            result.IsCompleted.Should().BeFalse();
        }

        [Fact]
        public void MultiPropertyClassWithAllPropertiesSetToExpectedValueReturnsCompletedTask()
        {
            // arrange
            var multiDummy = new MultiAttributeFixture(false, false);

            // act
            var result = multiDummy.BlockUntil();

            // assert
            result.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public void MultiPropertyCorrectlyCompletesTaskIfAllPropertiesAreSetToExpectedValues()
        {
            // arrange
            var multiDummy = new MultiAttributeFixture(true, true);
            var result = multiDummy.BlockUntil();

            // act
            // assert
            multiDummy.Property1 = false;
            result.IsCompleted.Should().BeFalse();
            multiDummy.Property2 = false;
            result.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public void PropertyChangesToExpectedValueCompletesTheTask()
        {
            // arrange
            var classDummy = new SingleAttributeFixture(true);
            var result = classDummy.BlockUntil();

            // act
            classDummy.IsLoading = false;

            // assert
            result.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public void PropertyIsAlreadyInExpectedStateReturnsACompletedTask()
        {
            // arrange
            var classDummy = new SingleAttributeFixture(false);

            // act
            var result = classDummy.BlockUntil();

            // assert
            result.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public void PropertyWithIncorrectStateReturnsAWaitingTask()
        {
            // arrange
            var classDummy = new SingleAttributeFixture(true);

            // act
            var result = classDummy.BlockUntil();

            // assert
            result.IsCompleted.Should().BeFalse();
        }
    }

    public class SingleAttributeFixture : INotifyPropertyChanged
    {
        #region Constructor

        public SingleAttributeFixture(bool startValue)
        {
            IsLoading = startValue;
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        [BlockingProperty(false)]
        public bool IsLoading { get; set; }

        #endregion
    }

    public class NoAttributeFixture : INotifyPropertyChanged
    {
        #region Constructor

        public NoAttributeFixture(bool startValue)
        {
            IsLoading = startValue;
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public bool IsLoading { get; set; }

        #endregion
    }

    public class MultiAttributeFixture : INotifyPropertyChanged
    {
        #region Constructor

        public MultiAttributeFixture(bool prop1StartValue, bool prop2StartValue)
        {
            Property1 = prop1StartValue;
            Property2 = prop2StartValue;
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        [BlockingProperty(false)]
        public bool Property1 { get; set; }

        [BlockingProperty(false)]
        public bool Property2 { get; set; }

        #endregion
    }
}