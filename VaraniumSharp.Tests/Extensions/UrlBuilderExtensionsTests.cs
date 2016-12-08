using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using VaraniumSharp.Extensions;

namespace VaraniumSharp.Tests.Extensions
{
    public class UrlBuilderExtensionsTests
    {
        #region Public Methods

        [Test]
        public void AppendMultiplePathElementsToBase()
        {
            // arrange
            const string basePath = "http://test.com";
            const string pathElement1 = "hello";
            const string pathElement2 = "world";

            // act
            var result = basePath.BuildPath(pathElement1, pathElement2);

            // assert
            result.Should().Be($"{basePath}/{pathElement1}/{pathElement2}");
        }

        [Test]
        public void AppendMultipleQueryStringParametersToPath()
        {
            // arrange
            const string basePath = "http://test.com";
            const string key1 = "user";
            const string value1 = "testUser";
            const string key2 = "password";
            const string value2 = "12345";

            var paramToAppend1 = new KeyValuePair<string, string>(key1, value1);
            var paramToAppend2 = new KeyValuePair<string, string>(key2, value2);

            // act
            var result = basePath.AppendQueryString(paramToAppend1, paramToAppend2);

            // assert
            result.Should().Be($"{basePath}?{key1}={value1}&{key2}={value2}");
        }

        [Test]
        public void AppendSinglePathElementToBase()
        {
            // arrange
            const string basePath = "http://test.com";
            const string pathElement = "hello";

            // act
            var result = basePath.BuildPath(pathElement);

            // assert
            result.Should().Be($"{basePath}/{pathElement}");
        }

        [Test]
        public void AppendSingleQueryStringParameterToPath()
        {
            // arrange
            const string basePath = "http://test.com";
            const string key = "user";
            const string value = "testUser";
            var paramToAppend = new KeyValuePair<string, string>(key, value);

            // act
            var result = basePath.AppendQueryString(paramToAppend);

            // assert
            result.Should().Be($"{basePath}?{key}={value}");
        }

        [Test]
        public void CannotAppendAQueryStringToABaseThatAlreadyHasOne()
        {
            // arrange
            const string basePath = "http://test.com?hello=world";
            const string key = "user";
            const string value = "testUser";
            var paramToAppend = new KeyValuePair<string, string>(key, value);
            var act = new Action(() => basePath.AppendQueryString(paramToAppend));

            // act
            // assert
            act.ShouldThrow<InvalidEnumArgumentException>($"The base path {basePath} already has a query string appended");
        }

        #endregion
    }
}