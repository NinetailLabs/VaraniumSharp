using System;
using VaraniumSharp.Enumerations;

namespace VaraniumSharp.Attributes
{
    /// <summary>
    /// Used to register all classes or structs that implement a specific interface or inherits from a base class.
    /// This attribute should be applied to the interface or base class that the class should be registered as in the Container
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
    public sealed class AutomaticConcretionContainerRegistrationAttribute : Attribute
    {
        #region Properties

        /// <summary>
        /// How the serivce should be setup for reuse
        /// </summary>
        public ServiceReuse Reuse { get; set; }

        #endregion Properties
    }
}