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
            : this(serviceType, reuse, multipleConstructors, 0)
        { }

        /// <summary>
        /// Construct attribute with service type, reuse, multiple constructor flag and a priority
        /// </summary>
        /// <param name="serviceType">The type that the service should be registered as</param>
        /// <param name="reuse">Indicate reuse policy for the service</param>
        /// <param name="multipleConstructors">Indicate if the service has multiple constructors</param>
        /// <param name="priority">Priority of the class in case of multiple implementers. Higher values takes priority</param>
        public AutomaticContainerRegistrationAttribute(Type serviceType, ServiceReuse reuse, bool multipleConstructors,
            int priority)
            : base(reuse, multipleConstructors)
        {
            ServiceType = serviceType;
            Priority = priority;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Injection priority of the class.
        /// This value is used when there are multiple implementation of the same class with the one with the highest priority being chosen.
        /// Default priority is 0
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// The type that the service is registered as
        /// </summary>
        public Type ServiceType { get; }

        #endregion
    }
}