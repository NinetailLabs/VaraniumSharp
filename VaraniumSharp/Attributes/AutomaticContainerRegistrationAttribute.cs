using System;
using VaraniumSharp.Enumerations;

namespace VaraniumSharp.Attributes
{
    /// <summary>
    /// Used to set values to assist in automatically registering it with a DI container
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class AutomaticContainerRegistrationAttribute : AutomaticContainerRegistrationBaseAttribute
    {
        #region Constructor

        /// <summary>
        /// Construct attribute with service type and default reuse
        /// </summary>
        /// <param name="serviceType">The type that the service should be registered as</param>
        public AutomaticContainerRegistrationAttribute(Type serviceType)
            : this(serviceType, ServiceReuse.Default)
        { }

        /// <summary>
        /// Construct attribute with service type and reuse
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="reuse"></param>
        public AutomaticContainerRegistrationAttribute(Type serviceType, ServiceReuse reuse)
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