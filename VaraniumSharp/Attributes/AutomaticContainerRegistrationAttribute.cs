using System;
using VaraniumSharp.Enumerations;

namespace VaraniumSharp.Attributes
{
    /// <summary>
    /// Used to set values to assist in automatically registering it with a DI container
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class AutomaticContainerRegistrationAttribute : AutomaticContainerRegistrationAttributeBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceType">The type that the service should be registered as</param>
        /// <param name="reuse">Indicate service reuse type</param>
        public AutomaticContainerRegistrationAttribute(Type serviceType, ServiceReuse reuse = ServiceReuse.Default)
            : base(reuse)
        {
            ServiceType = serviceType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The type that the service is registered as
        /// </summary>
        public Type ServiceType { get; }

        #endregion
    }
}