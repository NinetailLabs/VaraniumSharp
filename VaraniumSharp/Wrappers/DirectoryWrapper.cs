using System.Collections.Generic;
using System.IO;
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