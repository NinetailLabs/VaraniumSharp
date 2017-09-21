using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using VaraniumSharp.Attributes;
using VaraniumSharp.Enumerations;

namespace VaraniumSharp.Tests.Attributes
{
    public class AutomaticContainerRegistrationAttributeTest
    {
        #region Public Methods

        [Test]
        public void ReadCustomAttributeForMultipleConstructorSetup()
        {
            // arrange
            // act
            var instance = new MultipleConstructorDummy();

            // assert
            var attribute = (AutomaticContainerRegistrationAttribute)instance
                .GetType()
                .GetCustomAttributes(typeof(AutomaticContainerRegistrationAttribute), false)
                .First();
            attribute?.MultipleConstructors.Should().BeTrue();
        }

        [Test]
        public void ReadCustomAttributesForDefaultSetup()
        {
            // arrange
            // act
            var instance = new DefaultTestDummy();

            // assert
            var attribute = (AutomaticContainerRegistrationAttribute)instance
                .GetType()
                .GetCustomAttributes(typeof(AutomaticContainerRegistrationAttribute), false)
                .FirstOrDefault();
            attribute.Should().NotBeNull();
            attribute?.Reuse.Should().Be(ServiceReuse.Default);
            attribute?.ServiceType.Should().Be<DefaultTestDummy>();
            attribute?.MultipleConstructors.Should().BeFalse();
            attribute?.Priority.Should().Be(0);
        }

        [Test]
        public void ReadCustomAttributesForSingletonSetup()
        {
            // arrange
            // act
            var instance = new SingletonTestDummy();

            // assert
            var attribute = (AutomaticContainerRegistrationAttribute)instance
                .GetType()
                .GetCustomAttributes(typeof(AutomaticContainerRegistrationAttribute), false)
                .FirstOrDefault();
            attribute.Should().NotBeNull();
            attribute?.Reuse.Should().Be(ServiceReuse.Singleton);
            attribute?.ServiceType.Should().Be<IInterface1>();
            attribute?.MultipleConstructors.Should().BeFalse();
        }

        [Test]
        public void ReadCustomAttributeWithNonDefaultPriority()
        {
            // arrange
            // act
            var sut = new HighPriorityDummy();

            // assert
            var attribute = (AutomaticContainerRegistrationAttribute)sut
                .GetType()
                .GetCustomAttributes(typeof(AutomaticContainerRegistrationAttribute), false)
                .FirstOrDefault();
            attribute.Should().NotBeNull();
            attribute?.Priority.Should().Be(1);
        }

        #endregion

        #region Types

        [AutomaticContainerRegistration(typeof(DefaultTestDummy))]
        private class DefaultTestDummy
        { }

        private interface IInterface1
        { }

        [AutomaticContainerRegistration(typeof(IInterface1), ServiceReuse.Singleton)]
        private class SingletonTestDummy : IInterface1
        { }

        [AutomaticContainerRegistration(typeof(MultipleConstructorDummy), ServiceReuse.Default, true)]
        private class MultipleConstructorDummy
        { }

        [AutomaticContainerRegistration(typeof(HighPriorityDummy), Priority = 1)]
        private class HighPriorityDummy
        { }

        #endregion
    }
}