using FluentAssertions;
using VaraniumSharp.Collections;
using Xunit;

namespace VaraniumSharp.Tests.Collections
{
    public class ObservableKeyValuePairTests
    {
        [Fact]
        public void OnValueChangeTheListenerIsNotified()
        {
            // arrange
            const string key = "key";
            const string value = "value";
            const string newValue = "newValue";
            var valueChanged = false;

            var sut = new ObservableKeyValuePair<string, string>(key, value);
            sut.PropertyChanged += (_, _) => valueChanged = true;

            // act
            sut.Value = newValue;

            // assert
            valueChanged.Should().BeTrue();
        }
    }
}