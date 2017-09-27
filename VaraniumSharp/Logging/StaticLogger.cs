using Microsoft.Extensions.Logging;

namespace VaraniumSharp.Logging
{
    /// <summary>
    /// Static helper that is used by VaraniumSharp to access the <see cref="LoggerFactory"/> instance.
    /// This class allows for easy configuration and usage of the factory by implementers.
    /// To have the factory log to your provider simply use <code>LoggerFactory.AddProvider()</code> to add your provider, unless your provider has helper classes in which case those can be used
    /// </summary>
    public static class StaticLogger
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        static StaticLogger()
        {
            LoggerFactory = new LoggerFactory();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The factory that is used to create Logger instances for Logging in VaraniumSharp
        /// </summary>
        public static LoggerFactory LoggerFactory { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Create an ILogger instance for the specified type
        /// </summary>
        /// <typeparam name="T">Type of class that the logger instance is for</typeparam>
        /// <returns></returns>
        public static ILogger GetLogger<T>()
        {
            return LoggerFactory.CreateLogger(typeof(T));
        }

        #endregion
    }
}