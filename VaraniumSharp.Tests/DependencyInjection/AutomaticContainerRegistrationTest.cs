using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using VaraniumSharp.Attributes;
using VaraniumSharp.DependencyInjection;

namespace VaraniumSharp.Tests.DependencyInjection
{
    public class AutomaticContainerRegistrationTest
    {
        #region Public Methods

        [Test]
        public void GatherAndAutoRegisterClasses()
        {
            // arrange
            const bool autoRegister = true;
            var sut = new AutomaticRegistrationMock();

            // act
            sut.RetrieveClassesRequiringRegistration(autoRegister);

            // assert
            sut.RegisterClassesCalled.Should().BeTrue();
        }

        [Test]
        public void GatherClassesForAutomaticRegistration()
        {
            // arrange
            const bool autoRegister = false;
            const int knownClassesWithAutomaticRegistration = 1;
            var sut = new AutomaticRegistrationMock();

            // act
            sut.RetrieveClassesRequiringRegistration(autoRegister);

            // assert
            sut.RegisterClassesCalled.Should().BeFalse();
            sut.RegisteredClasses.Should().BeGreaterOrEqualTo(knownClassesWithAutomaticRegistration);
            sut.DiscoveredTypes.Should().Contain(typeof(AutomaticRegistrationDummy));
        }

        #endregion Public Methods

        #region Types

        private class AutomaticRegistrationMock : AutomaticContainerRegistration
        {
            #region Constructor

            public AutomaticRegistrationMock()
            {
                RegisterClassesCalled = false;
            }

            #endregion Constructor

            #region Properties

            public List<Type> DiscoveredTypes => ClassesToRegister;

            public bool RegisterClassesCalled { get; private set; }

            public int RegisteredClasses => ClassesToRegister.Count;

            #endregion Properties

            #region Private Methods

            protected override void RegisterClasses()
            {
                RegisterClassesCalled = true;
            }

            #endregion Private Methods
        }

        [AutomaticContainerRegistration(typeof(AutomaticRegistrationDummy))]
        private class AutomaticRegistrationDummy
        {}

        #endregion Types
    }
}