using System.IO;
using System.Threading.Tasks;
using VaraniumSharp.Wrappers;

namespace VaraniumSharp.Interfaces.Wrappers
{
    /// <summary>
    /// Wrapper for <see cref="File"/> methods to allow easy injection for testings purposes
    /// </summary>
    public interface IFileWrapper
    {
        #region Public Methods

        /// <summary>
        /// Delete a file.
        /// Note that this method does **not** guard against exceptions, this is left to the caller.
        /// </summary>
        /// <param name="filePath">Path of the file to delete</param>
        void DeleteFile(string filePath);

        /// <summary>
        /// Check if a file exists
        /// </summary>
        /// <param name="filePath">Absolute path of the file to check</param>
        /// <returns>True - File exists, otherwise false</returns>
        bool FileExists(string filePath);

        /// <summary>
        /// Gather some information about a file
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>Basic information about the file</returns>
        FileInformation GetFileInformation(string filePath);

        /// <summary>
        /// Retrieve the size of a file
        /// </summary>
        /// <param name="filePath">Path of the file for which size should be retrieved</param>
        /// <returns>Size of the file</returns>
        long GetFileSize(string filePath);

        /// <summary>
        /// Retrieve version of a file
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>String containing file version information</returns>
        string GetFileVersion(string filePath);

        /// <summary>
        /// Open a file
        /// </summary>
        /// <param name="filePath">Path of the file to open</param>
        /// <param name="mode">FileMode to use when opening the file</param>
        /// <param name="fileAccess">Kind of access to the file that is required</param>
        /// <param name="fileShare">FileShare level</param>
        /// <returns>FileStream of the opened file</returns>
        FileStream OpenFile(string filePath, FileMode mode, FileAccess fileAccess, FileShare fileShare);

        /// <summary>
        /// Read text from a file. 
        /// </summary>
        /// <param name="filePath">Path to the file from which text should be read</param>
        /// <returns>All text that was in the file</returns>
        string ReadAllText(string filePath);

        /// <summary>
        /// Rename a file
        /// </summary>
        /// <param name="source">FilePath of the file to rename</param>
        /// <param name="target">Filepath that files should be renamed to</param>
        void RenameFile(string source, string target);

        /// <summary>
        /// Write text to a file.
        /// If the file does not exist it will be created, if the file exists it will be overwritten
        /// </summary>
        /// <param name="filePath">Path to the file to which text should be written</param>
        /// <param name="textToWrite">The text to write to the file</param>
        void WriteAllText(string filePath, string textToWrite);

        #endregion

#if NETSTANDARD2_1_OR_GREATER

        /// <summary>
        /// Read text from a file. 
        /// </summary>
        /// <param name="filePath">Path to the file from which text should be read</param>
        /// <returns>All text that was in the file</returns>
        Task<string> ReadAllTextAsync(string filePath);

        /// <summary>
        /// Write text to a file.
        /// If the file does not exist it will be created, if the file exists it will be overwritten
        /// </summary>
        /// <param name="filePath">Path to the file to which text should be written</param>
        /// <param name="textToWrite">The text to write to the file</param>
        Task WriteAllTextAsync(string filePath, string textToWrite);
#endif
    }
}