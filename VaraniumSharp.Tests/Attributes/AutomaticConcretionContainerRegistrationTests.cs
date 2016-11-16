using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using VaraniumSharp.Attributes;
using VaraniumSharp.Enumerations;

namespace VaraniumSharp.Tests.Attributes
{
    public class AutomaticConcretionContainerRegistrationTests
    {
        #region Public Methods

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

        #endregion

        [AutomaticConcretionContainerRegistration]
        private class BaseClassDummy
        { }

        private class IheritorDummy : BaseClassDummy
        { }

        [AutomaticConcretionContainerRegistration(ServiceReuse.Singleton)]
        private interface IInterfaceDummy
        { }
    }
}