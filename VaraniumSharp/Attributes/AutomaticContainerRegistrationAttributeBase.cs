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
        /// Constructor that allows setting Reuse
        /// </summary>
        /// <param name="reuse">Indicate reuse policy for the service</param>
        /// <param name="multipleConstructors">Indicate if the service has multiple constructors</param>
        protected AutomaticContainerRegistrationBaseAttribute(ServiceReuse reuse, bool multipleConstructors)
        {
            Reuse = reuse;
            MultipleConstructors = multipleConstructors;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Indicate if the service should be automatically resolved during startup.
        /// </summary>
        public bool AutoResolveAtStartup { get; set; }

        /// <summary>
        /// Indicate if the service has multiple constructors.
        /// Containers like DryIoC need this in order to be able to properly register classes that have more than one constructor.
        /// <see>
        ///     <cref>https://bitbucket.org/dadhi/dryioc/wiki/SelectConstructorOrFactoryMethod#markdown-header-selecting-constructor-with-resolvable-parameters</cref>
        /// </see>
        /// <para>If the implementing container does not need it, this property can be ignored</para>
        /// </summary>
        public bool MultipleConstructors { get; }

        /// <summary>
        /// How the service should be setup for reuse
        /// </summary>
        public ServiceReuse Reuse { get; }

        #endregion
    }
}