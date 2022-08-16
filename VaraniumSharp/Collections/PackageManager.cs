﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VaraniumSharp.Attributes;
using VaraniumSharp.Enumerations;
using VaraniumSharp.EventArguments;
using VaraniumSharp.Interfaces.Collections;

namespace VaraniumSharp.Collections
{
    /// <inheritdoc />
    /// <summary>
    /// Manager used to pack files into a <see cref="T:System.IO.Compression.ZipArchive" /> without compression.
    /// <remark>
    /// All methods in this class are Thread safe
    /// </remark>
    /// </summary>
    [AutomaticContainerRegistration(typeof(IPackageManager), ServiceReuse.Singleton)]
    [Obsolete("This class should no longer be used as it continually rewrite the file on disk - Instead use https://github.com/NinetailLabs/VaraniumSharp.CompoundDocumentFormatStorage which does not have the issue")]
    public class PackageManager : IPackageManager
    {
        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PackageManager()
        {
            PackageDictionary = new Dictionary<string, ZipArchive>();
            LockingDictionary = new ConcurrentDictionary<string, SemaphoreSlim>();
        }

        #endregion

        #region Events

        /// <summary>
        /// Event used to indicate scrubbing progress on a Package
        /// </summary>
        public event EventHandler<ScrubProgressArgs> ScrubProgress;

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool AutoFlush { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Store a data stream inside the Pacakge.
        /// If the data already exists in the Package it will be overwritten.
        /// </summary>
        /// <param name="packagePath">Path to the Package</param>
        /// <param name="data">The stream that should be stored in the Package</param>
        /// <param name="storagePath">The relative path where the data should be stored inside the Package</param>
        public async Task AddItemToPackageAsync(string packagePath, Stream data, string storagePath)
        {
            var package = await RetrievePackageAsync(packagePath);

            var semaphore = LockingDictionary.GetOrAdd(packagePath, new SemaphoreSlim(1));
            try
            {
                await semaphore.WaitAsync();
                var part = package.Entries.Any(x => x.FullName == storagePath)
                    ? package.GetEntry(storagePath)
                    : package.CreateEntry(storagePath);

                using (var partStream = part.Open())
                {
                    await data.CopyToAsync(partStream);
                }

                AutoFlushPackage(packagePath);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc />
        public void ClosePackage(string packagePath)
        {
            throw new NotImplementedException("This is a new function and is not supported as this class has been deprecated - Instead use https://github.com/NinetailLabs/VaraniumSharp.CompoundDocumentFormatStorage to enable closing of a package");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public Task<List<PackageEntry>> GetPackageContentAsync(string packagePath, List<string> storagePaths)
        {
            throw new NotImplementedException("PackageManager is deprecated and this method will not be implemented");
        }

        /// <summary>
        /// Remove data from a Package
        /// <remark>
        /// If the data does not exist in the package nothing will be done
        /// </remark>
        /// </summary>
        /// <param name="packagePath">Path to the Package</param>
        /// <param name="storagePath">The relative path where data is stored inside the Package</param>
        public async Task RemoveDataFromPackageAsync(string packagePath, string storagePath)
        {
            var package = await RetrievePackageAsync(packagePath);

            var semaphore = LockingDictionary.GetOrAdd(packagePath, new SemaphoreSlim(1));
            try
            {
                await semaphore.WaitAsync();

                if (package.Entries.All(x => x.FullName != storagePath))
                {
                    return;
                }

                package.GetEntry(storagePath)?.Delete();

                AutoFlushPackage(packagePath);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Retrieves data from the Package.
        /// </summary>
        /// <param name="packagePath">Path to the Package</param>
        /// <param name="storagePath">The relative path where data is stored inside the Package</param>
        /// <exception cref="KeyNotFoundException">Thrown if the storagePath could not be found in the package</exception>
        /// <returns>MemoryStream containing the data from the Package</returns>
        public async Task<Stream> RetrieveDataFromPackageAsync(string packagePath, string storagePath)
        {
            var package = await RetrievePackageAsync(packagePath);

            var semaphore = LockingDictionary.GetOrAdd(packagePath, new SemaphoreSlim(1));
            try
            {
                await semaphore.WaitAsync();

                if (package.Entries.All(x => x.FullName != storagePath))
                {
                    throw new KeyNotFoundException($"The path {storagePath} could not be found in the Package");
                }

                var part = package.GetEntry(storagePath);

                var returnStream = new MemoryStream();
                using (var partStream = part.Open())
                {
                    await partStream.CopyToAsync(returnStream);
                    returnStream.Position = 0;
                }

                return returnStream;
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Delete all data from the Package that is not contained in the list of paths to keep
        /// </summary>
        /// <param name="packagePath">Path to the Package</param>
        /// <param name="storagePathsToKeep">List of relative paths of data in the package that should be kept</param>
        public async Task ScrubStorageAsync(string packagePath, List<string> storagePathsToKeep)
        {
            var package = await RetrievePackageAsync(packagePath);

            var semaphore = LockingDictionary.GetOrAdd(packagePath, new SemaphoreSlim(1));
            try
            {
                await semaphore.WaitAsync();
                var parts = package.Entries.ToList();
                var totalEntries = parts.Count;
                var progress = 0;
                foreach (var part in parts)
                {
                    if (!storagePathsToKeep.Contains(part.FullName))
                    {
                        part.Delete();
                    }
                    progress++;
                    ScrubProgress?.Invoke(this, new ScrubProgressArgs(packagePath, totalEntries, progress));
                }

                AutoFlushPackage(packagePath);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc />
        public Task<List<PackageEntry>> ScrubStorageWithFeedbackAsync(string packagePath, List<string> storagePathsToKeep)
        {
            throw new NotImplementedException("PackageManager is deprecated and this method will not be implemented");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Flush (Dispose) the <see cref="ZipArchive"/> in <see cref="PackageDictionary"/> and reconnect to it
        /// to force data to be persisted to disk after an operation.
        /// Flush will only occur if the pacakge is in the <see cref="PackageDictionary"/> and <see cref="AutoFlush"/> is set to true.
        /// <remarks>
        /// This method does not lock use the Semaphore from the <see cref="LockingDictionary"/> so it must be called when the archive is already locked
        /// </remarks>
        /// </summary>
        /// <param name="packagePath">Package path used to retrieve it from the Dictionary</param>
        private void AutoFlushPackage(string packagePath)
        {
            if (!AutoFlush
                || !PackageDictionary.ContainsKey(packagePath))
            {
                return;
            }

            PackageDictionary[packagePath].Dispose();
            PackageDictionary[packagePath] = new ZipArchive(File.Open(packagePath, FileMode.OpenOrCreate), ZipArchiveMode.Update);
        }

        /// <summary>
        /// Handle disposal if we are disposing the class
        /// </summary>
        /// <param name="disposing">Indicate if disposal has already been done</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var item in PackageDictionary)
                {
                    item.Value.Dispose();
                }
            }
        }

        /// <summary>
        /// Retrieve a Package for usage. This method will create the Package if it does not exist yet or open it if it exists
        /// </summary>
        /// <param name="packagePath">Path to the Package to initialize</param>
        private async Task<ZipArchive> RetrievePackageAsync(string packagePath)
        {
            var semaphore = LockingDictionary.GetOrAdd(packagePath, new SemaphoreSlim(1));
            try
            {
                await semaphore.WaitAsync();
                if (PackageDictionary.ContainsKey(packagePath))
                {
                    return PackageDictionary[packagePath];
                }

                if (!File.Exists(packagePath))
                {
                    var createdZip = new ZipArchive(File.Open(packagePath, FileMode.OpenOrCreate), ZipArchiveMode.Create);
                    createdZip.Dispose();
                }

                var tmpPackage = new ZipArchive(File.Open(packagePath, FileMode.OpenOrCreate), ZipArchiveMode.Update);

                PackageDictionary.Add(packagePath, tmpPackage);
                return tmpPackage;
            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #region Variables

        /// <summary>
        /// Dictionary used to lock access to entries in the archive
        /// </summary>
        protected readonly ConcurrentDictionary<string, SemaphoreSlim> LockingDictionary;

        /// <summary>
        ///  Dictionary to Store package instances
        /// </summary>
        protected readonly Dictionary<string, ZipArchive> PackageDictionary;

        #endregion
    }
}