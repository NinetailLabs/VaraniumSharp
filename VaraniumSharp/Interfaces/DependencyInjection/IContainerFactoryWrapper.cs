using System.Collections.Generic;

namespace VaraniumSharp.Interfaces.DependencyInjection
{
    /// <summary>
    /// Wrapper for the DI container.
    /// This provides an abstracted way to resolve dependencies from the DI system without having to directly reference the container in your code.
    /// This interface is meant to provide factories in level 2 VaraniumSharp libraries access to the container without the code having to take a direct dependency on the IoC library.
    /// </summary>
    public interface IContainerFactoryWrapper
    {
        #region Public Methods

        /// <summary>
        /// Resolve a Service from the Container
        /// </summary>
        /// <typeparam name="TService">Service to resolve</typeparam>
        /// <returns>Resolved service</returns>
        TService Resolve<TService>();

        /// <summary>
        /// Resolve Services from the container via a shared interface of parent class
        /// </summary>
        /// <typeparam name="TService">Interface or parent class that children are registered under</typeparam>
        /// <returns>Collection of children classes that inherit from the parent or implement the interface</returns>
        IEnumerable<TService> ResolveMany<TService>();

        #endregion
    }
}