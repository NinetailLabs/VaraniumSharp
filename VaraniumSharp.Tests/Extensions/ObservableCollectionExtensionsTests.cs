using FluentAssertions;
using NUnit.Framework;
using System.Collections.ObjectModel;
using VaraniumSharp.Extensions;

namespace VaraniumSharp.Tests.Extensions
{
    public class ObservableCollectionExtensionsTests
    {
        #region Public Methods

        [Test]
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

        [Test]
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

        [Test]
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

        #endregion

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
    }
}