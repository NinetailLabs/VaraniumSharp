using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using VaraniumSharp.Extensions;

namespace VaraniumSharp.Tests.Extensions
{
    public class StringExtensionsTest
    {
        [Test]
        public void RetrieveStringKeyFromConfiguration()
        {
            // arrange
            const string key = "StringValue";
            const string expectedValue = "TestValue";

            // act
            var result = key.GetConfigurationValue<string>();

            // assert
            result.GetType().Should().Be<string>();
            result.Should().Be(expectedValue);
        }

        [Test]
        public void RetrieveIntegerKeyFromConfiguration()
        {
            // arrange
            const string key = "IntValue";
            const int expectedValue = 20787;

            // act
            var result = key.GetConfigurationValue<int>();

            // assert
            result.GetType().Should().Be<int>();
            result.Should().Be(expectedValue);
        }

        [Test]
        public void RetrieveEmptyKeyFromConfiguration()
        {
            // arrange
            const string key = "Empty";

            // act
            var result = key.GetConfigurationValue<DateTime>();

            // assert
            result.Should().Be(default(DateTime));
        }

        [Test]
        public void RetrieveKeyThatDoesNotExistFromConfiguration()
        {
            // arrange
            const string key = "InvalidKey";
            var action = new Action(() => key.GetConfigurationValue<string>());

            // act
            // assert
            action.ShouldThrow<KeyNotFoundException>();
        }
    }
}