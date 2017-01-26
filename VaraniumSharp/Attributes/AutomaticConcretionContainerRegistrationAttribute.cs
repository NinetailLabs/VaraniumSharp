using System;
using VaraniumSharp.Enumerations;

namespace VaraniumSharp.Attributes
{
    /// <summary>
    /// Used to register all classes or structs that implement a specific interface or inherits from a base class.
    /// This attribute should be applied to the interface or base class that the class should be registered as in the Container
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
    public sealed class AutomaticConcretionContainerRegistrationAttribute : AutomaticContainerRegistrationBaseAttribute
    {
        #region Constructor

        /// <summary>
        /// Parameterless Constructor sets Reuse to default
        /// </summary>
        public AutomaticConcretionContainerRegistrationAttribute()
            : this(ServiceReuse.Default)
        { }

        /// <summary>
        /// Constructor that allows setting Reuse directly
        /// </summary>
        /// <param name="reuse">Indicate service reuse type</param>
        public AutomaticConcretionContainerRegistrationAttribute(ServiceReuse reuse)
            : this(reuse, false)
        { }

        /// <summary>
        /// Constructor that allows setting Reuse directly
        /// </summary>
        /// <param name="reuse">Indicate service reuse type</param>
        /// <param name="multipleConstructors">Indicate if the service has multiple constructors</param>
        public AutomaticConcretionContainerRegistrationAttribute(ServiceReuse reuse, bool multipleConstructors)
            : base(reuse, multipleConstructors)
        { }

        #endregion
    }
}