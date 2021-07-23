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
        public FileInformation()
        {
            Filename = string.Empty;
            Folder = string.Empty;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The name of the file
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// The folder where the file is located
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// The size of the file in bytes
        /// </summary>
        public long Size { get; set; }

        #endregion
    }
}