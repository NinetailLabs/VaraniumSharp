using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace VaraniumSharp.Interfaces.Wrappers
{
    /// <summary>
    /// Wrapper for <see cref="HttpClient"/> to make it injectable
    /// <remarks>
    /// The wrapper currently only covers GetAsync and GetStreamAsync method. If a method is required create a ticket or submit a pull request
    /// </remarks>
    /// </summary>
    public interface IHttpClient : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets or sets the base address used when sending requests
        /// </summary>
        Uri BaseAddress { get; set; }

        /// <summary>
        /// Gets the headers that should be sent with each request
        /// </summary>
        HttpRequestHeaders DefaultRequestHeaders { get; }

        /// <summary>
        /// Gets or sets the maximum number of bytes to buffer when reading the response content
        /// </summary>
        long MaxResponseContentBufferSize { get; set; }

        /// <summary>
        /// Gets or sets the length of time before the request times out
        /// </summary>
        TimeSpan Timeout { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Cancel all pending requests on the instance
        /// </summary>
        void CancelPendingRequests();

        /// <summary>
        /// Sends a GET request to the specified URI as an async operation
        /// </summary>
        /// <param name="requestUri">The URI the request is sent to</param>
        /// <returns>The response received from the endpoint</returns>
        Task<HttpResponseMessage> GetAsync(Uri requestUri);

        /// <summary>
        /// Sends a GET request to the specified URI with a cancellation token as an async operation
        /// </summary>
        /// <param name="requestUri">The URI the request is sent to</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of a cancellation</param>
        /// <returns>The response received from the endpoint</returns>
        Task<HttpResponseMessage> GetAsync(Uri requestUri, CancellationToken cancellationToken);

        /// <summary>
        /// Sends a GET request to the specified URI and return the response body as a stream in an async operation
        /// </summary>
        /// <param name="requestUri">The URI the request is sent to</param>
        /// <returns>Stream containing the response body</returns>
        Task<Stream> GetStreamAsync(Uri requestUri);

        #endregion
    }
}