using System;
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
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

                package.GetEntry(storagePath).Delete();
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
            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #region Private Methods

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