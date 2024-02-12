using System.Collections.Generic;
using System.IO;
using System.Linq;
using VaraniumSharp.Attributes;
using VaraniumSharp.Enumerations;
using VaraniumSharp.Interfaces.Wrappers;

namespace VaraniumSharp.Wrappers
{
    /// <summary>
    /// Assist with Directory operations
    /// </summary>
    [AutomaticContainerRegistration(typeof(IDirectoryWrapper), ServiceReuse.Singleton)]
    public class DirectoryWrapper : IDirectoryWrapper
    {
        #region Public Methods

        /// <inheritdoc />
        public DirectoryInfo CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }

        /// <inheritdoc />
        public void DeleteEmptySubDirectories(string directoryPath, bool deleteBaseDirectory)
        {
            DeleteEmptySubDirectories(directoryPath);
            if (!deleteBaseDirectory)
            {
                return;
            }

            if (!Directory.EnumerateFileSystemEntries(directoryPath).Any())
            {
                Directory.Delete(directoryPath);
            }
        }

        /// <inheritdoc />
        public void DeleteEmptySubDirectories(string directoryPath)
        {
            var subDirectories = Directory.EnumerateDirectories(directoryPath, "*", SearchOption.AllDirectories).Reverse();
            var files = Directory
                .EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories)
                .ToList();

            foreach (var subDir in subDirectories)
            {
                if (files.Any(x => x.StartsWith(subDir)))
                {
                    continue;
                }

                Directory.Delete(subDir);
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> EnumerateFiles(string path)
        {
            return Directory.EnumerateFiles(path);
        }

        /// <inheritdoc />
        public bool Exists(string folderPath)
        {
            return Directory.Exists(folderPath);
        }

        #endregion
    }
}