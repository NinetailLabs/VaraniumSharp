using System;
using System.Collections.Generic;
using System.Linq;
using VaraniumSharp.Attributes;

namespace VaraniumSharp.DependencyInjection
{
    /// <summary>
    /// Assist in gathering and registering classes that require automated registration
    /// </summary>
    public abstract class AutomaticContainerRegistration
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        protected AutomaticContainerRegistration()
        {
            ClassesToRegister = new List<Type>();
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Contains all classes that have the AutomaticContainerRegistrationAttribute
        /// </summary>
        protected List<Type> ClassesToRegister { get; }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Call to retrieve classes that request Automatic registration
        /// Can optionally continue with registration
        /// </summary>
        public virtual void RetrieveClassesRequiringRegistration(bool handleRegistration)
        {
            ClassesToRegister.AddRange(
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(t => t.GetTypes())
                    .Where(
                        t =>
                            t.IsClass &&
                            (t.GetCustomAttributes(typeof(AutomaticContainerRegistrationAttribute), false).Length > 0)));

            if (!handleRegistration)
                return;
            RegisterClasses();
        }

        #endregion Public Methods

        #region Private Methods

        #region Protected Methods

        /// <summary>
        /// Register classes with Container
        /// </summary>
        protected abstract void RegisterClasses();

        #endregion Protected Methods

        #endregion Private Methods
    }
}