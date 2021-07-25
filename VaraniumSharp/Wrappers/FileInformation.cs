using System.IO;

namespace VaraniumSharp.Wrappers
{
    /// <summary>
    /// Basic information about a file
    /// </summary>
    public sealed class FileInformation
    {
        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public FileInformation(FileInfo fileInfo)
        {
            Filename = fileInfo.Name;
            Folder = fileInfo.Directory.FullName;
            Size = fileInfo.Length;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The name of the file
        /// </summary>
        public string Filename { get; }

        /// <summary>
        /// The folder where the file is located
        /// </summary>
        public string Folder { get; }

        /// <summary>
        /// The size of the file in bytes
        /// </summary>
        public long Size { get; }

        #endregion
    }
}