using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace VaraniumSharp.Extensions
{
    /// <summary>
    /// Extension methods for strings
    /// </summary>
    public static class StringExtensions
    {
        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        static StringExtensions()
        {
            ConfigurationLocation = Assembly.GetEntryAssembly().Location;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The location of the assembly of the configuration file that should be loaded.
        /// Defaults to the EntryAssembly's configuration file similar to the static <see cref="ConfigurationManager"/>
        /// </summary>
        public static string ConfigurationLocation { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Read a value from App.config and return it as the specified type
        /// </summary>
        /// <typeparam name="T">Type of the value that is to be retrieved</typeparam>
        /// <param name="key">Key for which the value should be retrieved</param>
        /// <returns>Value from App.config that is associated with the key</returns>
        public static T GetConfigurationValue<T>(this string key)
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationLocation).AppSettings;
            if (!configuration.Settings.AllKeys.Contains(key))
            {
                throw new KeyNotFoundException($"Could not locate key {key} in the configuration");
            }

            var value = configuration.Settings[key].Value;
            if (string.IsNullOrEmpty(value))
            {
                return default(T);
            }
            var typedValue = typeof(T).IsEnum
                ? (T)Enum.Parse(typeof(T), value)
                : (T)Convert.ChangeType(value, typeof(T));

            return typedValue;
        }

        #endregion
    }
}