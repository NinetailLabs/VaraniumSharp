using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using VaraniumSharp.Attributes;
using VaraniumSharp.Interfaces.Wrappers;

namespace VaraniumSharp.Wrappers
{
    /// <inheritdoc />
    [AutomaticContainerRegistration(typeof(IHttpClient))]
    public class HttpClientWrapper : IHttpClient
    {
        #region Constructor

        /// <summary>
        /// DI Constructor
        /// </summary>
        public HttpClientWrapper()
        {
            _httpClient = new HttpClient();
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public Uri BaseAddress
        {
            get => _httpClient.BaseAddress;
            set => _httpClient.BaseAddress = value;
        }

        /// <inheritdoc />
        public HttpRequestHeaders DefaultRequestHeaders => _httpClient.DefaultRequestHeaders;

        /// <inheritdoc />
        public long MaxResponseContentBufferSize
        {
            get => _httpClient.MaxResponseContentBufferSize;
            set => _httpClient.MaxResponseContentBufferSize = value;
        }

        /// <inheritdoc />
        public TimeSpan Timeout
        {
            get => _httpClient.Timeout;
            set => _httpClient.Timeout = value;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public void CancelPendingRequests()
        {
            _httpClient.CancelPendingRequests();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> GetAsync(Uri requestUri)
        {
            return await _httpClient.GetAsync(requestUri);
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> GetAsync(Uri requestUri, CancellationToken cancellationToken)
        {
            return await _httpClient.GetAsync(requestUri, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Stream> GetStreamAsync(Uri requestUri)
        {
            return await _httpClient.GetStreamAsync(requestUri);
        }

        #endregion

        #region Variables

        /// <summary>
        /// HttpClient instance
        /// </summary>
        private readonly HttpClient _httpClient;

        #endregion
    }
}