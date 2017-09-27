using System.Diagnostics;

namespace VaraniumSharp.Interfaces.GenericHelpers
{
    /// <summary>
    /// Wrapper for static methods that are difficult to use in Unit testing.
    /// An example would be <see cref="Process.Start()"/>
    /// </summary>
    public interface IStaticMethodWrapper
    {
        #region Public Methods

        /// <summary>
        /// Invoke <see cref="Process.Start()"/> for a filename.
        /// This method can also be invoked with a Url to start launch it in the default browser
        /// </summary>
        /// <param name="filename">Name of the file to open or Url to launch</param>
        void StartProcess(string filename);

        #endregion
    }
}