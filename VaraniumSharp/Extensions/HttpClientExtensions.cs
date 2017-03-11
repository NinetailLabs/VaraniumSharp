using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace VaraniumSharp.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="HttpClient"/>
    /// </summary>
    public static class HttpClientExtensions
    {
        #region Public Methods

        /// <summary>
        /// Set Basic Authentication header
        /// </summary>
        /// <param name="client">HttpClient for which the header should be set</param>
        /// <param name="username">Username for authentication</param>
        /// <param name="password">Password for authentication</param>
        /// <returns>HttpClient</returns>
        public static HttpClient SetBasicAuthHeader(this HttpClient client, string username, string password)
        {
            var credentials = Convert.ToBase64String(Encoding.Default.GetBytes($"{username}:{password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            return client;
        }

        #endregion
    }
}