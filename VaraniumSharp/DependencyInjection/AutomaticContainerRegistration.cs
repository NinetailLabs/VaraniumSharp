using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VaraniumSharp.Attributes;
using VaraniumSharp.Logging;

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
            ClassesToAutoRegister = new List<Type>();
            _logger = StaticLogger.GetLogger<AutomaticContainerRegistration>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Contains all classes that should be auto-resolved after container construction
        /// </summary>
        protected List<Type> ClassesToAutoRegister { get; }

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
            using (_logger.BeginScope("AutomaticRegistration"))
            {
                var classList = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .Where(
                        t =>
                            t.IsClass &&
                            t.GetCustomAttributes(typeof(AutomaticContainerRegistrationAttribute), false).Length > 0)
                    .Select(x => new Tuple<AutomaticContainerRegistrationAttribute, Type>(
                        (AutomaticContainerRegistrationAttribute)x
                            .GetCustomAttributes(typeof(AutomaticContainerRegistrationAttribute), false)
                            .First(), x))
                    .ToList();

                var detectedTypes = classList.Select(x =>
                    new
                    {
                        Class = x.Item2.FullName,
                        x.Item1.ServiceType,
                        x.Item1.Priority,
                        x.Item1.AutoResolveAtStartup
                    });

                _logger.LogDebug(
                    "Discovered {AttributeImplementations} that implement the AutomaticContainerRegistration attribute. {@DetectedTypes}",
                    classList.Count, detectedTypes);

                var classesToRegister = classList
                    .GroupBy(x => x.Item1.ServiceType)
                    .SelectMany(x =>
                        x.Where(z => z.Item1.Priority == x.Max(q => q.Item1.Priority)))
                            .ToList();
                ClassesToRegister.AddRange(classesToRegister.Select(z => z.Item2));

                ClassesToAutoRegister.AddRange(classesToRegister
                    .Where(x => x.Item1.AutoResolveAtStartup)
                    .Select(x => x.Item2));

                var registrationClasses = classList.Select(x =>
                    new
                    {
                        Class = x.Item2.FullName,
                        x.Item1.ServiceType,
                        x.Item1.Priority
                    });

                _logger.LogDebug(
                    "After removing duplicates {AttributeImplementations} remain and that will be passed for DI registrations. {@RegistrationClasses}",
                    ClassesToRegister.Count,
                    registrationClasses);

                if (!handleRegistration)
                {
                    _logger.LogDebug("Automatic registration is not enabled - Skipping");
                    return;
                }
                RegisterClasses();
            }
        }

        /// <summary>
        /// Call to retrieve classes that request Automatic Concretion registration
        /// </summary>
        /// <param name="handleRegistration">Set to false to manually register class, otherwise use true</param>
        public virtual void RetrieveConcretionClassesRequiringRegistration(bool handleRegistration)
        {
            using (_logger.BeginScope("AutomaticConcretionRegistration"))
            {
                var classes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(t => t.GetTypes())
                    .Where(
                        t =>
                            (t.IsClass && !t.IsSealed || t.IsInterface)
                            &&
                            t.GetCustomAttributes(typeof(AutomaticConcretionContainerRegistrationAttribute), false)
                                .Length >
                            0
                    )
                    .ToList();

                foreach (var @class in classes)
                {
                    var attribute = (AutomaticConcretionContainerRegistrationAttribute)@class
                        .GetCustomAttributes(typeof(AutomaticConcretionContainerRegistrationAttribute), false)
                        .First();

                    var children = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes())
                        .Where(
                            x =>
                                !x.IsInterface && !x.IsAbstract && (x.IsSubclassOf(@class) 
                                                                    || x.GetInterfaces().Contains(@class))).ToList();

                    ConcretionClassesToRegister.Add(@class, children);
                    if (@attribute.AutoResolveAtStartup)
                    {
                        ClassesToAutoRegister.Add(@class);
                    }
                }

                var concretionClasses = ConcretionClassesToRegister.Select(x =>
                    new
                    {
                        ServiceType = x.Key.FullName,
                        Inheritors = x.Value.Select(z => z.FullName)
                    });

                _logger.LogDebug(
                    "Discovered {AttributeImplementations} that implement the AutomaticConcretionContainerRegistration attribute. {@ConcretionClasses}",
                    classes.Count, concretionClasses);

                if (!handleRegistration)
                {
                    _logger.LogDebug("Automatic registration is not enabled - Skipping");
                    return;
                }
                RegisterConcretionClasses();
            }
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

        /// <summary>
        /// Request that auto-resolve instance are resolved
        /// </summary>
        protected abstract void AutoResolveStartupInstance();

        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly ILogger _logger;

        #endregion
    }
}