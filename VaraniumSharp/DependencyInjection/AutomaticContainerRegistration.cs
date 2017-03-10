﻿using System;
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
            ConcretionClassesToRegister = new Dictionary<Type, List<Type>>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Contains all classes that have the AutomaticContainerRegistrationAttribute
        /// </summary>
        protected List<Type> ClassesToRegister { get; }

        /// <summary>
        /// Contains all classes that should be registered under a single type
        /// </summary>
        protected Dictionary<Type, List<Type>> ConcretionClassesToRegister { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resolve a Service from the Container
        /// </summary>
        /// <typeparam name="TService">Service to resolve</typeparam>
        /// <returns>Resolved service</returns>
        public abstract TService Resolve<TService>();

        /// <summary>
        /// Resolve Services from the container via a shared interface of parent class
        /// </summary>
        /// <typeparam name="TService">Interface or parent class that children are registered under</typeparam>
        /// <returns>Collection of children classes that inherit from the parent or implement the interface</returns>
        public abstract IEnumerable<TService> ResolveMany<TService>();

        /// <summary>
        /// Call to retrieve classes that request Automatic registration
        /// Can optionally continue with registration
        /// </summary>
        /// <param name="handleRegistration">Set to false to manually register class, otherwise use true</param>
        public virtual void RetrieveClassesRequiringRegistration(bool handleRegistration)
        {
            ClassesToRegister.AddRange(
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(t => t.GetTypes())
                    .Where(
                        t =>
                            t.IsClass &&
                            t.GetCustomAttributes(typeof(AutomaticContainerRegistrationAttribute), false).Length > 0));

            if (!handleRegistration)
            {
                return;
            }
            RegisterClasses();
        }

        /// <summary>
        /// Call to retrieve classes that request Automatic Concretion registation
        /// </summary>
        /// <param name="handleRegistration">Set to false to manually register class, otherwise use true</param>
        public virtual void RetrieveConcretionClassesRequiringRegistration(bool handleRegistration)
        {
            var classes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => t.GetTypes())
                .Where(
                    t =>
                        (t.IsClass && !t.IsSealed || t.IsInterface)
                        &&
                        t.GetCustomAttributes(typeof(AutomaticConcretionContainerRegistrationAttribute), false).Length >
                        0
                );

            foreach (var @class in classes)
            {
                var children = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes())
                    .Where(
                        x =>
                            !x.IsInterface && !x.IsAbstract && (x.IsSubclassOf(@class) ||
                            x.GetInterfaces().Contains(@class))).ToList();

                ConcretionClassesToRegister.Add(@class, children);
            }

            if (!handleRegistration)
            {
                return;
            }
            RegisterConcretionClasses();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Register classes with Container
        /// </summary>
        protected abstract void RegisterClasses();

        /// <summary>
        /// Register concretion classes with Container
        /// </summary>
        protected abstract void RegisterConcretionClasses();

        #endregion
    }
}