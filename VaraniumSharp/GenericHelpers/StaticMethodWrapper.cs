using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
            Process.Start(filename);
        }

        #endregion
    }
}