using FluentAssertions;
using System.Linq;
using VaraniumSharp.Attributes;
using VaraniumSharp.Enumerations;
using Xunit;

namespace VaraniumSharp.Tests.Attributes
{
    public class AutomaticConcretionContainerRegistrationTests
    {
        [AutomaticConcretionContainerRegistration]
        private class BaseClassDummy
        { }

        private class IheritorDummy : BaseClassDummy
        { }

        [AutomaticConcretionContainerRegistration(ServiceReuse.Singleton)]
        private interface IInterfaceDummy
        { }

        [AutomaticConcretionContainerRegistration(ServiceReuse.Default, true)]
        private class MultiConstructorBase
        { }

        private class MultiClassInheritor : MultiConstructorBase
        { }

        [Fact]
        public void ReadCustomAttributeForDefaultSetup()
        {
            // arrange
            // act
            var sut = new IheritorDummy();

            // assert
            var attribute = (AutomaticConcretionContainerRegistrationAttribute)sut
                .GetType()
                .GetCustomAttributes(typeof(AutomaticConcretionContainerRegistrationAttribute), true)
                .FirstOrDefault();
            attribute.Should().NotBeNull();
            attribute?.Reuse.Should().Be(ServiceReuse.Default);
            attribute?.MultipleConstructors.Should().BeFalse();
        }

        [Fact]
        public void ReadCustomAttributeForMultipleConstructorSetup()
        {
            // arrange
            // act
            var sut = new MultiClassInheritor();

            // assert
            var attribute = (AutomaticConcretionContainerRegistrationAttribute)sut
                .GetType()
                .GetCustomAttributes(typeof(AutomaticConcretionContainerRegistrationAttribute), true)
                .First();
            attribute.MultipleConstructors.Should().BeTrue();
        }

        [Fact]
        public void ReadCustomAttributeForSingletonSetup()
        {
            // arrange
            // act
            // assert
            var attribute = (AutomaticConcretionContainerRegistrationAttribute)typeof(IInterfaceDummy)
                    .GetCustomAttributes(typeof(AutomaticConcretionContainerRegistrationAttribute), true)
                    .FirstOrDefault();
            attribute.Should().NotBeNull();
            attribute?.Reuse.Should().Be(ServiceReuse.Singleton);
            attribute?.MultipleConstructors.Should().BeFalse();
        }
    }
}