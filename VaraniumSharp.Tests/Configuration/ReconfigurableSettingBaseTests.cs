using FluentAssertions;
using NUnit.Framework;
using VaraniumSharp.Tests.Fixtures;

namespace VaraniumSharp.Tests.Configuration
{
    public class ReconfigurableSettingBaseTests
    {
        #region Public Methods

        [Test]
        public void AdjustingValuesInInheritedClassCorrectlyUpdatesBaseProperties()
        {
            // arrange
            const bool executeSaveSuccess = true;
            // ReSharper disable once UseObjectOrCollectionInitializer - Do not set here, it is the action to invoke for testing purposes
            var sut = new ReconfigurableImplementationFixture(executeSaveSuccess);

            // act
            sut.TestProperty = true;

            // assert
            sut.DataCanBePersisted.Should().BeTrue();
            sut.UnsavedChanges.Should().BeTrue();
        }

        [Test]
        public void CancellingChangesCorrectlyRestoresOriginalValues()
        {
            // arrange
            const bool executeSaveSuccess = true;
            var sut = new ReconfigurableImplementationFixture(executeSaveSuccess);
            sut.LoadSettingsAsync().Wait();
            sut.TestProperty = false;
            sut.TestProperty.Should().BeFalse();

            // act
            sut.CancelChanges();

            // assert
            sut.TestProperty.Should().BeTrue();
            sut.DataCanBePersisted.Should().BeFalse();
            sut.UnsavedChanges.Should().BeFalse();
        }

        [Test]
        public void FailedSaveDoesNotBroadcastUpdate()
        {
            // arrange
            var settingUpdateCalled = false;
            const bool executeSaveSuccess = false;
            var sut = new ReconfigurableImplementationFixture(executeSaveSuccess);
            sut.SettingUpdated += (sender, args) =>
            {
                settingUpdateCalled = true;
            };
            sut.TestProperty = true;

            // act
            sut.PersistSettingsAsync().Wait();

            // assert
            settingUpdateCalled.Should().BeFalse();
            sut.DataSaveExecutions.Should().Be(1);
            sut.DataCanBePersisted.Should().BeTrue();
            sut.UnsavedChanges.Should().BeTrue();
        }

        [Test]
        public void LoadingSettingsWorksCorrectly()
        {
            // arrange
            var settingUpdateCalled = false;
            const bool executeSaveSuccess = true;
            var sut = new ReconfigurableImplementationFixture(executeSaveSuccess);
            sut.SettingUpdated += (sender, args) =>
            {
                settingUpdateCalled = true;
            };

            // act
            sut.LoadSettingsAsync().Wait();

            // assert
            settingUpdateCalled.Should().BeFalse();
            sut.DataLoadExecutions.Should().Be(1);
            sut.DataCanBePersisted.Should().BeFalse();
            sut.UnsavedChanges.Should().BeFalse();
        }

        [Test]
        public void SaveSettingsWorksCorrectly()
        {
            // arrange
            var settingUpdateCalled = false;
            const bool executeSaveSuccess = true;
            var sut = new ReconfigurableImplementationFixture(executeSaveSuccess);
            sut.SettingUpdated += (sender, args) =>
            {
                settingUpdateCalled = true;
            };
            sut.TestProperty = true;

            // act
            sut.PersistSettingsAsync().Wait();

            // assert
            settingUpdateCalled.Should().BeTrue();
            sut.DataSaveExecutions.Should().Be(1);
            sut.DataCanBePersisted.Should().BeFalse();
            sut.UnsavedChanges.Should().BeFalse();
        }

        #endregion
    }
}