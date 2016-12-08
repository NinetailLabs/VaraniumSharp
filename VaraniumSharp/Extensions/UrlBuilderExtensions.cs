using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace VaraniumSharp.Extensions
{
    /// <summary>
    /// Assists in the building of Urls
    /// </summary>
    public static class UrlBuilderExtensions
    {
        #region Public Methods

        /// <summary>
        /// Append a query string to the Url.
        /// <example>
        /// Passing in `http://test.com` with a keyvalue pair containing the key `user` and the value `testUser` will produce
        /// `http://test.com?user=testUser
        /// </example>
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException">If the base path already contain a query string we cannot append another one to it</exception>
        /// <param name="basePath">The base url to which the query string should be appended</param>
        /// <param name="queryStringParameters">Parameters for the query string</param>
        /// <returns></returns>
        public static string AppendQueryString(this string basePath, params KeyValuePair<string, string>[] queryStringParameters)
        {
            if (basePath.Contains('?'))
            {
                throw new InvalidEnumArgumentException($"The base path {basePath} already has a query string appended");
            }

            var queryString = string.Join("&", queryStringParameters.Select(x => $"{x.Key}={x.Value}"));
            return $"{basePath}?{queryString}";
        }

        /// <summary>
        /// Build a path for the Url.
        /// <example>
        /// Passing in `http://test.com` with `hello` and `world` as parameters will produce
        /// `http://test.com/hello/world`
        /// </example>
        /// </summary>
        /// <param name="base">The base to which the path should be appended</param>
        /// <param name="pathElements">The elements to append to the path</param>
        /// <returns></returns>
        public static string BuildPath(this string @base, params string[] pathElements)
        {
            var path = string.Join("/", pathElements.Select(x => x.TrimEnd('/')));
            return $"{@base.TrimEnd('/')}/{path}";
        }

        #endregion
    }
}