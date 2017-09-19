using System;

namespace VaraniumSharp.EventArguments
{
    /// <summary>
    /// Event arguments for Scrubbing progress
    /// </summary>
    public class ScrubProgressArgs : EventArgs
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="packagePath">Path of the pacakge that is being scrubbed</param>
        /// <param name="totalEntries">Total number of entries in the package</param>
        /// <param name="entriesProcessed">Number of entries that has already been processed</param>
        public ScrubProgressArgs(string packagePath, int totalEntries, int entriesProcessed)
        {
            PackagePath = packagePath;
            TotalEntries = totalEntries;
            EntriesProcessed = entriesProcessed;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The number of entries that has been processed
        /// </summary>
        public int EntriesProcessed { get; }

        /// <summary>
        /// Path of the Package that is being scrubbed
        /// </summary>
        public string PackagePath { get; }

        /// <summary>
        /// Percentage of entries in the package that has been processed
        /// </summary>
        public double PercentageCompleted => EntriesProcessed / (double)TotalEntries * 100;

        /// <summary>
        /// The original number of entries in the Package
        /// </summary>
        public int TotalEntries { get; }

        #endregion
    }
}