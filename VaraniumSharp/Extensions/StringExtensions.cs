using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace VaraniumSharp.Extensions
{
    /// <summary>
    /// Extension methods for strings
    /// </summary>
    public static class StringExtensions
    {
        #region Public Methods

        /// <summary>
        /// Read a value from App.config and return it as the spesified type
        /// </summary>
        /// <typeparam name="T">Type of the value that is to be retrieved</typeparam>
        /// <param name="key">Key for which the value should be retrieved</param>
        /// <returns>Value from App.config that is associated with the key</returns>
        public static T GetConfigurationValue<T>(this string key)
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                throw new KeyNotFoundException($"Could not locate key {key} in the configuration");
            }
            
            var value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(value))
            {
                return default(T);
            }
            var typedValue = (T)Convert.ChangeType(value, typeof(T));
            return typedValue;
        }

        #endregion
    }
}