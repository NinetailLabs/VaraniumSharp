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
        /// <param name="serviceType">The type that the service should be registered as</param>
        /// <param name="reuse">Indicate reuse policy for the service</param>
        public AutomaticContainerRegistrationAttribute(Type serviceType, ServiceReuse reuse)
            : this(serviceType, reuse, false)
        { }

        /// <summary>
        /// Construct attribute with service type, reuse and multiple constructor flag
        /// </summary>
        /// <param name="serviceType">The type that the service should be registered as</param>
        /// <param name="reuse">Indicate reuse policy for the service</param>
        /// <param name="multipleConstructors">Indicate if the service has multiple constructors</param>
        public AutomaticContainerRegistrationAttribute(Type serviceType, ServiceReuse reuse,
            bool multipleConstructors)
            : base(reuse, multipleConstructors)
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