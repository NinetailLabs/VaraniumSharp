using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using VaraniumSharp.Extensions;
using Xunit;

namespace VaraniumSharp.Tests
{
    public class StringExtensionsTest
    {
        private enum TestEnum
        {
            EnumResult
        }

        [Fact]
        public void RetrieveEmptyKeyFromConfiguration()
        {
            // arrange
            StringExtensions.ConfigurationLocation = Assembly.GetExecutingAssembly().Location;
            const string key = "Empty";

            // act
            var result = key.GetConfigurationValue<DateTime>();

            // assert
            result.Should().Be(default(DateTime));
        }

        /// <summary>
        /// #Issue-9 - Cannot retrieve Enum from configuration
        /// </summary>
        [Fact]
        public void RetrieveEnumerationValueFromConfiguration()
        {
            // arrange
            StringExtensions.ConfigurationLocation = Assembly.GetExecutingAssembly().Location;
            const string key = "EnumValue";

            // act
            var result = key.GetConfigurationValue<TestEnum>();

            // assert
            result.Should().Be(TestEnum.EnumResult);
        }

        [Fact]
        public void RetrieveIntegerKeyFromConfiguration()
        {
            // arrange
            StringExtensions.ConfigurationLocation = Assembly.GetExecutingAssembly().Location;
            const string key = "IntValue";
            const int expectedValue = 20787;

            // act
            var result = key.GetConfigurationValue<int>();

            // assert
            result.GetType().Should().Be<int>();
            result.Should().Be(expectedValue);
        }

        [Fact]
        public void RetrieveKeyThatDoesNotExistFromConfiguration()
        {
            // arrange
            StringExtensions.ConfigurationLocation = Assembly.GetExecutingAssembly().Location;
            const string key = "InvalidKey";
            var action = new Action(() => key.GetConfigurationValue<string>());

            // act
            // assert
            action.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void RetrieveStringKeyFromConfiguration()
        {
            // arrange
            StringExtensions.ConfigurationLocation = Assembly.GetExecutingAssembly().Location;
            const string key = "StringValue";
            const string expectedValue = "TestValue";

            // act
            var result = key.GetConfigurationValue<string>();

            // assert
            result.GetType().Should().Be<string>();
            result.Should().Be(expectedValue);
        }
    }
}