using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using VaraniumSharp.Attributes;
using VaraniumSharp.Enumerations;

namespace VaraniumSharp.Tests.Attributes
{
    public class AutomaticContainerRegistrationAttributeTest
    {
        [Test]
        public void ReadCustomAttributesForDefaultSetup()
        {
            // arrange
            // act
            var instance = new DefaultTestDummy();

            // assert
            var attribute = (AutomaticContainerRegistrationAttribute)instance.GetType().GetCustomAttributes(typeof(AutomaticContainerRegistrationAttribute), false).FirstOrDefault();
            attribute.Should().NotBeNull();
            attribute?.Reuse.Should().Be(ServiceReuse.Default);
            attribute?.ServiceType.Should().Be<DefaultTestDummy>();
        }

        [Test]
        public void ReadCustomAttributesForSingletonSetup()
        {
            // arrange
            // act
            var instance = new SingletonTestDummy();

            // assert
            var attribute = (AutomaticContainerRegistrationAttribute)instance.GetType().GetCustomAttributes(typeof(AutomaticContainerRegistrationAttribute), false).FirstOrDefault();
            attribute.Should().NotBeNull();
            attribute?.Reuse.Should().Be(ServiceReuse.Singleton);
            attribute?.ServiceType.Should().Be<IInterface1>();
        }

        #region Types

        [AutomaticContainerRegistration(typeof(DefaultTestDummy))]
        private class DefaultTestDummy
        {}

        private interface IInterface1
        { }

        [AutomaticContainerRegistration(typeof(IInterface1), Reuse = ServiceReuse.Singleton)]
        private class SingletonTestDummy : IInterface1
        { }

        #endregion
    }
}