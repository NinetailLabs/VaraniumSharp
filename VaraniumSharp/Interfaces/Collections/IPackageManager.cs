using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace VaraniumSharp.Interfaces.Collections
{
    /// <summary>
    /// Manager used to pack files into a <see cref="ZipArchive"/> without compression.
    /// <remark>
    /// All methods in this class are Thread safe
    /// </remark>
    /// </summary>
    public interface IPackageManager : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets or Sets if the PackageManager should automatically dispose and recreate the connection to the <see cref="ZipArchive"/>
        /// to prevent data only being written to disk on Disposal
        /// </summary>
        bool AutoFlush { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Store a data stream inside the Package.
        /// If the data already exists in the Package it will be overwritten.
        /// </summary>
        /// <param name="packagePath">Path to the Package</param>
        /// <param name="data">The stream that should be stored in the Package</param>
        /// <param name="storagePath">The relative path where the data should be stored inside the Package</param>
        Task AddItemToPackageAsync(string packagePath, Stream data, string storagePath);

        /// <summary>
        /// Closes the package for the specified path
        /// </summary>
        /// <param name="packagePath">Path to the Package</param>
        void ClosePackage(string packagePath);

        /// <summary>
        /// Get the paths and sizes of all the entries in the package
        /// </summary>
        /// <param name="packagePath">Path to the Package</param>
        /// <returns>Collection containing all the entries and their sized that are in the Package</returns>
        Task<List<PackageEntry>> GetPackageContentAsync(string packagePath);

        /// <summary>
        /// Remove data from a Package
        /// <remark>
        /// If the data does not exist in the package nothing will be done
        /// </remark>
        /// </summary>
        /// <param name="packagePath">Path to the Package</param>
        /// <param name="storagePath">The relative path where data is stored inside the Package</param>
        Task RemoveDataFromPackageAsync(string packagePath, string storagePath);

        /// <summary>
        /// Retrieves data from the Package.
        /// </summary>
        /// <param name="packagePath">Path to the Package</param>
        /// <param name="storagePath">The relative path where data is stored inside the Package</param>
        /// <exception cref="KeyNotFoundException">Thrown if the storagePath could not be found in the package</exception>
        /// <returns>MemoryStream containing the data from the Package</returns>
        Task<Stream> RetrieveDataFromPackageAsync(string packagePath, string storagePath);

        /// <summary>
        /// Delete all data from the Package that is not contained in the list of paths to keep
        /// </summary>
        /// <param name="packagePath">Path to the Package</param>
        /// <param name="storagePathsToKeep">List of relative paths of data in the package that should be kept</param>
        Task ScrubStorageAsync(string packagePath, List<string> storagePathsToKeep);

        /// <summary>
        /// Delete all data from the Package that is not contained in the list of paths to keep.
        /// </summary>
        /// <param name="packagePath">Path to the Package</param>
        /// <param name="storagePathsToKeep">List of relative paths of data in the package that should be kept</param>
        /// <returns>Collection containing the details of entries removed during the scrub</returns>
        Task<List<PackageEntry>> ScrubStorageWithFeedbackAsync(string packagePath, List<string> storagePathsToKeep);

        #endregion
    }
}