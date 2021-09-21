using FluentAssertions;
using System;
using System.Collections.Generic;
using VaraniumSharp.Attributes;
using VaraniumSharp.DependencyInjection;
using Xunit;

namespace VaraniumSharp.Tests.DependencyInjection
{
    public class AutomaticContainerRegistrationTest
    {
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

            public bool AutoResolveAtStartupCalled { get; private set; }

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

            protected override void AutoResolveStartupInstance()
            {
                AutoResolveAtStartupCalled = true;
            }

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
        {}

        [AutomaticConcretionContainerRegistration]
        private abstract class BaseClassDummy
        {}

        private class ConcretionClassDummy : BaseClassDummy
        {}

        [AutomaticConcretionContainerRegistration]
        private interface IInterfaceDummy
        {}

        private interface IDeepInterfaceDummy : IInterfaceDummy
        {}

        private abstract class InheritingAbstractDummy : BaseClassDummy
        {}

        private class DoubleInterfaceImplementationDummy : IDeepInterfaceDummy
        {}

        private class InterfaceImplementationDummy : IInterfaceDummy
        {}

        private interface IMultiImplementationDummy
        {}

        [AutomaticContainerRegistration(typeof(IMultiImplementationDummy))]
        private class DefaultPriorityDummy : IMultiImplementationDummy
        {}

        [AutomaticContainerRegistration(typeof(IMultiImplementationDummy), Priority = 2)]
        private class HigherPriorityDummy : IMultiImplementationDummy
        {}

        [Fact]
        public void AutoResolveCorrectlyInvokesImplementation()
        {
            // arrange
            var sut = new AutomaticRegistrationMock();

            // act
            sut.AutoResolveRequiredClasses();
            
            // assert
            sut.AutoResolveAtStartupCalled.Should().BeTrue();
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public void GatherAndAutoRegisterConcretionClasses()
        {
            // arrange
            const bool autoRegister = true;
            var sut = new AutomaticRegistrationMock();

            // act
            sut.RetrieveConcretionClassesRequiringRegistration(autoRegister);

            // assert
            sut.RegisterConcretionClassesCalled.Should().BeTrue();
        }

        [Fact]
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

        [Fact]
        public void HigherPriorityClassIsPickedOverLowerPriorityDuringRegistration()
        {
            // arrange
            var sut = new AutomaticRegistrationMock();

            // act
            sut.RetrieveClassesRequiringRegistration(true);

            // assert
            sut.DiscoveredTypes.Should().Contain(typeof(HigherPriorityDummy));
            sut.DiscoveredTypes.Should().NotContain(typeof(DefaultPriorityDummy));
        }

        [Fact]
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
    }
}