using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using VaraniumSharp.Attributes;
using VaraniumSharp.Enumerations;

namespace VaraniumSharp.Tests.Attributes
{
    public class AutomaticConcretionContainerRegistrationTests
    {
        [Test]
        public void ReadCustomAttributeForDefaultSetup()
        {
            // arrange
            // act
            var sut = new IheritorDummy();

            // assert
            var attribute =
                (AutomaticConcretionContainerRegistrationAttribute)
                sut.GetType()
                    .GetCustomAttributes(typeof(AutomaticConcretionContainerRegistrationAttribute), true)
                    .FirstOrDefault();
            attribute.Should().NotBeNull();
            attribute?.Reuse.Should().Be(ServiceReuse.Default);
        }

        [Test]
        public void ReadCustomAttributeForSingletonSetup()
        {
            // arrange
            // act
            // assert
            var attribute =
                (AutomaticConcretionContainerRegistrationAttribute)
                typeof(IInterfaceDummy)
                    .GetCustomAttributes(typeof(AutomaticConcretionContainerRegistrationAttribute), true)
                    .FirstOrDefault();
            attribute.Should().NotBeNull();
            attribute?.Reuse.Should().Be(ServiceReuse.Singleton);
        }

        [AutomaticConcretionContainerRegistration]
        private class BaseClassDummy
        {}

        private class IheritorDummy : BaseClassDummy
        { }

        [AutomaticConcretionContainerRegistration(Reuse = ServiceReuse.Singleton)]
        private interface IInterfaceDummy
        {}
    }
}