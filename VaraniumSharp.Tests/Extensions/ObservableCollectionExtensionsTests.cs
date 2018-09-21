using FluentAssertions;
using System.Collections.ObjectModel;
using VaraniumSharp.Extensions;
using Xunit;

namespace VaraniumSharp.Tests.Extensions
{
    public class ObservableCollectionExtensionsTests
    {
        private class CleanupHelper
        {
            #region Properties

            public ObservableCollection<CollectionContentFixture> CollectionPassedIn { get; private set; }

            public bool WasInvoked { get; private set; }

            #endregion

            #region Public Methods

            public void ClearAction(ObservableCollection<CollectionContentFixture> collection)
            {
                CollectionPassedIn = collection;
                foreach (var item in collection)
                {
                    item.DoCleanup();
                }
                WasInvoked = true;
            }

            #endregion
        }

        private class CollectionContentFixture
        {
            #region Properties

            public bool CleanupWasInvoked { get; private set; }

            #endregion

            #region Public Methods

            public void DoCleanup()
            {
                CleanupWasInvoked = true;
            }

            #endregion
        }

        [Fact]
        public void CleaningCollectionInvokesAction()
        {
            // arrange
            var itemDummy = new CollectionContentFixture();
            var sut = new ObservableCollection<CollectionContentFixture> { itemDummy };
            var helper = new CleanupHelper();

            // act
            sut.Clear(helper.ClearAction);

            // assert
            helper.WasInvoked.Should().BeTrue();
            helper.CollectionPassedIn.Should().BeSameAs(sut);
        }

        [Fact]
        public void ClearingCollectionExecutesActionAgainstItemsInTheCollectionPriorToClearingTheCollection()
        {
            // arrange
            var itemDummy = new CollectionContentFixture();
            var sut = new ObservableCollection<CollectionContentFixture> { itemDummy };
            var helper = new CleanupHelper();

            // act
            sut.Clear(helper.ClearAction);

            // assert
            itemDummy.CleanupWasInvoked.Should().BeTrue();
        }

        [Fact]
        public void CollectionIsCleared()
        {
            // arrange
            var itemDummy = new CollectionContentFixture();
            var sut = new ObservableCollection<CollectionContentFixture> { itemDummy };
            var helper = new CleanupHelper();

            // act
            sut.Clear(helper.ClearAction);

            // assert
            sut.Count.Should().Be(0);
        }
    }
}