using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using VaraniumSharp.Wrappers;

namespace VaraniumSharp.Tests.Wrappers
{
    public class HttpClientWrapperTests
    {
        #region Public Methods

        [Test]
        public void BaseAddressIsSetCorrectly()
        {
            // arrange
            var testUri = new Uri(@"https://test.com");
            using (var sut = new HttpClientWrapper())
            {
                // act
                sut.BaseAddress = testUri;

                // assert
                sut.BaseAddress.Should().Be(testUri);
            }
        }

        [Test]
        public void CancellationTokenStopsWebRequest()
        {
            // arrange
            const string url = "http://localhost:8819/";
            const string urlPath = "test";
            var cancellationTokenSource = new CancellationTokenSource();
            var httpMock = new HttpMockSlim.HttpMock();
            httpMock.Start(url);

            httpMock.Add("GET", $"/{urlPath}", (request, response) =>
            {
                cancellationTokenSource.Cancel();
                Thread.Sleep(50000);
            });

            var sut = new HttpClientWrapper { BaseAddress = new Uri(url) };
            // ReSharper disable once MethodSupportsCancellation - We do not want the Wait operation to be cancelled
            var act = new Action(() => sut.GetAsync(new Uri($"{url}{urlPath}"), cancellationTokenSource.Token).Wait());

            // act
            // assert
            act.ShouldThrow<TaskCanceledException>();

            httpMock.Stop();
        }

        [Test]
        public void CancellingRequestCorrectlyCancelsTheHttpRequests()
        {
            // arrange
            const string url = "http://localhost:8819/";
            const string urlPath = "test";
            var httpMock = new HttpMockSlim.HttpMock();
            httpMock.Start(url);

            httpMock.Add("GET", $"/{urlPath}", (request, response) =>
            {
                Thread.Sleep(50000);
            });

            var sut = new HttpClientWrapper { BaseAddress = new Uri(url) };
            var result = sut.GetAsync(new Uri($"{url}{urlPath}"));
            Thread.Sleep(1000);

            // act
            sut.CancelPendingRequests();
            Thread.Sleep(10000);

            // assert
            result.IsCanceled.Should().BeTrue();

            httpMock.Stop();
        }

        [Test]
        public void DefaultRequestHeaderIsCorrectlyReturned()
        {
            // arrange
            var sut = new HttpClientWrapper();

            // act
            var headers = sut.DefaultRequestHeaders;

            // assert
            headers.Should().NotBeNull();
        }

        [Test]
        public async Task GetAsyncWorksCorrectly()
        {
            // arrange
            const string url = "http://localhost:8819/";
            const string urlPath = "test";
            var endpointHit = false;
            var httpMock = new HttpMockSlim.HttpMock();
            httpMock.Start(url);

            httpMock.Add("GET", $"/{urlPath}", (request, response) =>
            {
                endpointHit = true;
                response.SetBody("Success");
            });

            var sut = new HttpClientWrapper { BaseAddress = new Uri(url) };

            // act
            var result = await sut.GetAsync(new Uri($"{url}{urlPath}"));

            // assert
            result.IsSuccessStatusCode.Should().BeTrue();
            endpointHit.Should().BeTrue();

            httpMock.Stop();
        }

        [Test]
        public void MaxResponseContentBufferIsCorrectlySet()
        {
            // arrange
            const long maxResponseBufferSize = 1245;
            var sut = new HttpClientWrapper();

            // act
            sut.MaxResponseContentBufferSize = maxResponseBufferSize;

            // assert
            sut.MaxResponseContentBufferSize.Should().Be(maxResponseBufferSize);
        }

        [Test]
        public async Task RetrievingHttpBodyAsStreamWorksCorrectly()
        {
            // arrange
            const string url = "http://localhost:8819/";
            const string urlPath = "test";
            var httpMock = new HttpMockSlim.HttpMock();
            httpMock.Start(url);

            httpMock.Add("GET", $"/{urlPath}", (request, response) =>
            {
                response.SetBody("Success");
            });

            var sut = new HttpClientWrapper { BaseAddress = new Uri(url) };

            // act
            var result = await sut.GetStreamAsync(new Uri($"{url}{urlPath}"));

            // assert
            result.GetType().Name.Should().Be("ReadOnlyStream");
        }

        [Test]
        public void TimeoutIsCorrectlySet()
        {
            // arrange
            var timeout = TimeSpan.FromMilliseconds(1000);
            var sut = new HttpClientWrapper();

            // act
            sut.Timeout = timeout;

            // assert
            sut.Timeout.Should().Be(timeout);
        }

        #endregion
    }
}