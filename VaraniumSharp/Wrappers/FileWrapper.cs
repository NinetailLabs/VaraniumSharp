using System.Diagnostics;
using System;
using System.IO;
using System.Threading.Tasks;
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
            return File.Exists(filePath);
        }

        /// <inheritdoc />
        public FileInformation GetFileInformation(string filePath)
        {
            return new FileInformation(new FileInfo(filePath));
        }

        /// <inheritdoc />
        public long GetFileSize(string filePath)
        {
            return new FileInfo(filePath).Length;
        }

        /// <inheritdoc />
        public string GetFileVersion(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath);
            if (fileExtension != ".dll"
                && fileExtension != ".exe")
            {
                throw new InvalidOperationException("Cannot retrieve file version for files that are not DLL or EXE");
            }

            var versionInfo = FileVersionInfo.GetVersionInfo(filePath);
            return versionInfo.ProductVersion;
        }

        /// <inheritdoc />
        public FileStream OpenFile(string filePath, FileMode mode, FileAccess fileAccess, FileShare fileShare)
        {
            return File.Open(filePath, mode, fileAccess, fileShare);
        }

        /// <inheritdoc />
        public string ReadAllText(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        /// <inheritdoc />
        public void RenameFile(string source, string target)
        {
            File.Move(source, target);
        }

        /// <inheritdoc />
        public void WriteAllText(string filePath, string textToWrite)
        {
            File.WriteAllText(filePath, textToWrite);
        }

        #endregion

#if NETSTANDARD2_1_OR_GREATER

        /// <inheritdoc />
        public async Task<string> ReadAllTextAsync(string filePath)
        {
            return await File.ReadAllTextAsync(filePath);
        }

        /// <inheritdoc />
        public async Task WriteAllTextAsync(string filePath, string textToWrite)
        {
            await File.WriteAllTextAsync(filePath, textToWrite);
        }
#endif
    }
}