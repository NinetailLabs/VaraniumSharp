using System.Collections.Generic;
using System.IO;

namespace VaraniumSharp.Interfaces.Wrappers
{
    /// <summary>
    /// Wrapper for <see cref="Directory"/> methods to allow easy injection for testings purposes
    /// </summary>
    public interface IDirectoryWrapper
    {
        #region Public Methods

        /// <summary>
        /// Creates all directories and subdirectories in the path unless they already exist.
        /// </summary>
        /// <param name="path">Path of directories to create</param>
        /// <returns>An object that represent the directory at the specified path</returns>
        DirectoryInfo CreateDirectory(string path);

        /// <summary>
        /// Returns an enumerable collection of file names in a specified directory
        /// </summary>
        /// <param name="path">The directory to search</param>
        /// <returns>Collection of files found in the directory</returns>
        IEnumerable<string> EnumerateFiles(string path);

        /// <summary>
        /// Wrapper around <see cref="Directory.Exists(string)"/> method
        /// </summary>
        /// <param name="folderPath">Path of the folder to check</param>
        /// <returns>Indicates if the folder exists or not</returns>
        bool Exists(string folderPath);

        #endregion
    }
}