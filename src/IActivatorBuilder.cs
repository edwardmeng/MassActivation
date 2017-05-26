﻿using System;
using System.Reflection;

namespace MassActivation
{
    /// <summary>
    /// The configuration interface for the <see cref="ApplicationActivator"/> environment variables.
    /// </summary>
    public interface IActivatorBuilder
    {
        /// <summary>
        /// Specify the environment to be used by the hosting application. 
        /// </summary>
        /// <param name="environmentName">The environment to host the application in.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        IActivatorBuilder UseEnvironment(string environmentName);

        /// <summary>
        /// Specify the application name to be used by the hosting application. 
        /// </summary>
        /// <param name="applicationName">The application name to host the application in.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        IActivatorBuilder UseApplicationName(string applicationName);

        /// <summary>
        /// Specify the application version to be used by the hosting application. 
        /// </summary>
        /// <param name="applicationVersion">The application version to host the application in.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        IActivatorBuilder UseApplicationVersion(Version applicationVersion);

        /// <summary>
        /// Specifiy the startup method names to be used by the hosting application.
        /// </summary>
        /// <param name="methodNames">The method name to startup the hosting appliction.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        IActivatorBuilder UseStartupSteps(params string[] methodNames);

        /// <summary>
        /// Specifiy the shutdown method names to be used by the hosting application.
        /// </summary>
        /// <param name="methodNames">The method name to shutdown the hosting appliction.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        IActivatorBuilder UseShutdownSteps(params string[] methodNames);

        /// <summary>
        /// Specify the component to be used by the hosting application.
        /// </summary>
        /// <param name="serviceType">The requested type of the component.</param>
        /// <param name="instance">The instance of the component.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        IActivatorBuilder UseService(Type serviceType, object instance);

        /// <summary>
        /// Specify the component to be used by the hosting application.
        /// </summary>
        /// <typeparam name="T">The requested type of the component.</typeparam>
        /// <param name="instance">The instance of the component.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        IActivatorBuilder UseService<T>(T instance);

        /// <summary>
        /// Removes the registered component by using service type.
        /// </summary>
        /// <param name="serviceType">The registered type of the component.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        IActivatorBuilder RemoveService(Type serviceType);

        /// <summary>
        /// Removes the registered component by using service type.
        /// </summary>
        /// <typeparam name="T">The registered type of the component.</typeparam>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        IActivatorBuilder RemoveService<T>();

        /// <summary>
        /// Excludes the specified startup type from the startup process.
        /// </summary>
        /// <param name="type">The type to exclude from the startup process.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        IActivatorBuilder ExcludeStartup(Type type);

        /// <summary>
        /// Excludes the specified startup type from the startup process.
        /// </summary>
        /// <typeparam name="T">The type to exclude from the startup process.</typeparam>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        IActivatorBuilder ExcludeStartup<T>();

#if NetCore
        /// <summary>
        /// Specify the dynamically loaded assembly.
        /// </summary>
        /// <param name="assembly">The assembly has been dynamically loaded.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        IActivatorBuilder UseAssembly(Assembly assembly);
#endif

        /// <summary>
        /// Startup the hosting application.
        /// </summary>
        void Startup();
    }
}
