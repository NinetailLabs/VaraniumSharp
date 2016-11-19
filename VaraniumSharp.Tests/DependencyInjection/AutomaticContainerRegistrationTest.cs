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
        public void ConcretionRegistrationAvoidsAbstractChildClasses()
        {
            // arrange
            const bool autoRegister = false;
            var sut = new AutomaticRegistrationMock();

            // act
            sut.RetrieveConcretionClassesRequiringRegistration(autoRegister);

            // assert
            sut.DiscoveredConcretionClasses[typeof(BaseClassDummy)].Should().NotContain(typeof(InheritingAbstractDummy));
        }

        [Test]
        public void ConcretionRegistrationIgnoresInterfacesThatImplementInterfaces()
        {
            // arrange
            const bool autoRegister = false;
            var sut = new AutomaticRegistrationMock();

            // act
            sut.RetrieveConcretionClassesRequiringRegistration(autoRegister);

            // assert
            sut.DiscoveredConcretionClasses[typeof(IInterfaceDummy)].Should().NotContain(typeof(IDeepInterfaceDummy));
        }

        [Test]
        public void ConcretionRegistrationPicksUpClassesThatInheritFromInterfacesInheritingBaseInterface()
        {
            // arrange
            const bool autoRegister = false;
            var sut = new AutomaticRegistrationMock();

            // act
            sut.RetrieveConcretionClassesRequiringRegistration(autoRegister);

            // assert
            sut.DiscoveredConcretionClasses[typeof(IInterfaceDummy)].Should().Contain(typeof(DoubleInterfaceImplementationDummy));
        }

        [Test]
        public void GaterAndAutoRegisterConcretionClasses()
        {
            // arrange
            const bool autoRegister = true;
            var sut = new AutomaticRegistrationMock();

            // act
            sut.RetrieveConcretionClassesRequiringRegistration(autoRegister);

            // assert
            sut.RegisterConcretionClassesCalled.Should().BeTrue();
        }

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

        [Test]
        public void RetrieveAllClassesImplementingABaseClass()
        {
            // arrange
            const bool autoRegister = false;
            const int knowConcretionClasses = 2;
            const int knownConcretionBases = 2;

            var sut = new AutomaticRegistrationMock();

            // act
            sut.RetrieveConcretionClassesRequiringRegistration(autoRegister);

            // assert
            sut.ConcretionClassesRegistered.Should().BeGreaterOrEqualTo(knowConcretionClasses);
            sut.ConcretionBaseClasses.Should().BeGreaterOrEqualTo(knownConcretionBases);
            sut.RegisterConcretionClassesCalled.Should().BeFalse();
            sut.DiscoveredConcretionClasses.Should().ContainKey(typeof(BaseClassDummy));
            sut.DiscoveredConcretionClasses.Should().ContainKey(typeof(IInterfaceDummy));
            sut.DiscoveredConcretionClasses[typeof(BaseClassDummy)].Should().Contain(typeof(ConcretionClassDummy));
            sut.DiscoveredConcretionClasses[typeof(IInterfaceDummy)].Should()
                .Contain(typeof(InterfaceImplementationDummy));
        }

        #endregion

        #region Types

        private class AutomaticRegistrationMock : AutomaticContainerRegistration
        {
            #region Constructor

            public AutomaticRegistrationMock()
            {
                RegisterClassesCalled = false;
                RegisterConcretionClassesCalled = false;
            }

            #endregion

            #region Properties

            public int ConcretionBaseClasses => ConcretionClassesToRegister.Count;

            public int ConcretionClassesRegistered => ConcretionClassesToRegister.Values.Count;

            public Dictionary<Type, List<Type>> DiscoveredConcretionClasses => ConcretionClassesToRegister;

            public IEnumerable<Type> DiscoveredTypes => ClassesToRegister;

            public bool RegisterClassesCalled { get; private set; }

            public bool RegisterConcretionClassesCalled { get; private set; }

            public int RegisteredClasses => ClassesToRegister.Count;

            #endregion

            #region Public Methods

            public override TService Resolve<TService>()
            {
                throw new NotImplementedException();
            }

            public override IEnumerable<TService> ResolveMany<TService>()
            {
                throw new NotImplementedException();
            }

            #endregion

            #region Private Methods

            protected override void RegisterClasses()
            {
                RegisterClassesCalled = true;
            }

            protected override void RegisterConcretionClasses()
            {
                RegisterConcretionClassesCalled = true;
            }

            #endregion
        }

        [AutomaticContainerRegistration(typeof(AutomaticRegistrationDummy))]
        private class AutomaticRegistrationDummy
        { }

        [AutomaticConcretionContainerRegistration]
        private abstract class BaseClassDummy
        { }

        private class ConcretionClassDummy : BaseClassDummy
        { }

        [AutomaticConcretionContainerRegistration]
        private interface IInterfaceDummy
        { }

        private interface IDeepInterfaceDummy : IInterfaceDummy
        { }

        private abstract class InheritingAbstractDummy : BaseClassDummy
        { }

        private class DoubleInterfaceImplementationDummy : IDeepInterfaceDummy
        { }

        private class InterfaceImplementationDummy : IInterfaceDummy
        { }

        #endregion Types
    }
}