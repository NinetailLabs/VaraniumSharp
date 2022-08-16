namespace VaraniumSharp.Interfaces.Collections
{
    /// <summary>
    /// Details about an entry in a Package
    /// </summary>
    public struct PackageEntry
    {
        /// <summary>
        /// The path of the entry in the package
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The size of the entry in bytes
        /// </summary>
        public long Size { get; set; }
    }
}