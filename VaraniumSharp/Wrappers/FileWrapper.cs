using System.IO;
using VaraniumSharp.Attributes;
using VaraniumSharp.Enumerations;
using VaraniumSharp.Interfaces.Wrappers;

namespace VaraniumSharp.Wrappers
{
    /// <summary>
    /// Wrapper for <see cref="File"/> methods to allow easy injection for testings purposes
    /// </summary>
    [AutomaticContainerRegistration(typeof(IFileWrapper), ServiceReuse.Singleton)]
    public class FileWrapper : IFileWrapper
    {
        #region Public Methods

        /// <inheritdoc />
        public void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }

        /// <inheritdoc />
        public bool FileExists(string filePath)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public FileInformation GetFileInformation(string filePath)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public long GetFileSize(string filePath)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public string GetFileVersion(string filePath)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public FileStream OpenFile(string filePath, FileMode mode, FileAccess fileAccess, FileShare fileShare)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public string ReadAllText(string filePath)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void RenameFile(string source, string target)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void WriteAllText(string filePath, string textToWrite)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}