using System;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using VaraniumSharp.Extensions;
using Xunit;

namespace VaraniumSharp.Tests.Extensions
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public void SettingAuthenticationHeaderWorks()
        {
            // arrange
            const string username = "testuser";
            const string password = "testpass";
            var authValue = $"Basic {Convert.ToBase64String(Encoding.Default.GetBytes($"{username}:{password}"))}";
            var sut = new HttpClient();

            // act
            sut.SetBasicAuthHeader(username, password);

            // assert
            sut.DefaultRequestHeaders.GetValues("Authorization")
                .Should().Contain(authValue);
        }
    }
}