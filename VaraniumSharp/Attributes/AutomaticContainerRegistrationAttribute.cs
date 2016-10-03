using System;
using VaraniumSharp.Enumerations;

namespace VaraniumSharp.Attributes
{
    /// <summary>
    /// Used to set values to assist in automatically registering it with a DI container
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class AutomaticContainerRegistrationAttribute : Attribute
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceType">The type that the service should be registered as</param>
        public AutomaticContainerRegistrationAttribute(Type serviceType)
        {
            ServiceType = serviceType;
            Reuse = ServiceReuse.Default;
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// How the service should be setup for reuse
        /// </summary>
        public ServiceReuse Reuse { get; set; }

        /// <summary>
        /// The type that the service is registered as
        /// </summary>
        public Type ServiceType { get; }

        #endregion Properties
    }
}