using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using VaraniumSharp.Attributes;
using VaraniumSharp.Interfaces.GenericHelpers;

namespace VaraniumSharp.GenericHelpers
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    [AutomaticContainerRegistration(typeof(IStaticMethodWrapper))]
    public class StaticMethodWrapper : IStaticMethodWrapper
    {
        #region Public Methods

        /// <inheritdoc />
        public void StartProcess(string filename)
        {
            if (filename.StartsWith("http"))
            {
                filename = filename.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {filename}") { CreateNoWindow = true });
                return;
            }

            if (filename.EndsWith(".exe"))
            {
                Process.Start(filename);
                return;
            }

            var attr = File.GetAttributes(filename);
            Process.Start("explorer.exe",
                attr.HasFlag(FileAttributes.Directory) 
                    ? $"-p, \"{filename}\"" 
                    : $"/select, \"{filename}\"");
        }

        #endregion
    }
}