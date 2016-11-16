using System;
using VaraniumSharp.Enumerations;

namespace VaraniumSharp.Attributes
{
    /// <summary>
    /// Base class that can be used by attribute classes that deal with automatic container registration
    /// </summary>
    public abstract class AutomaticContainerRegistrationBaseAttribute : Attribute
    {
        #region Constructor

        /// <summary>
        /// Construcor that allows setting Reuse
        /// </summary>
        /// <param name="reuse"></param>
        protected AutomaticContainerRegistrationBaseAttribute(ServiceReuse reuse)
        {
            Reuse = reuse;
        }

        #endregion

        #region Properties

        /// <summary>
        /// How the service should be setup for reuse
        /// </summary>
        public ServiceReuse Reuse { get; }

        #endregion
    }
}