using System;

namespace MassActivation
{
    /// <summary>
    /// The implementation of <see cref="IActivatorBuilder"/> to adapter <see cref="ApplicationActivator"/> in fluent method chain.
    /// </summary>
    internal class ActivatorBuilder : IActivatorBuilder
    {
        /// <summary>
        /// Specify the environment to be used by the hosting application. 
        /// </summary>
        /// <param name="environmentName">The environment to host the application in.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public IActivatorBuilder UseEnvironment(string environmentName)
        {
            return ApplicationActivator.UseEnvironment(environmentName);
        }

        /// <summary>
        /// Specify the application name to be used by the hosting application. 
        /// </summary>
        /// <param name="applicationName">The application name to host the application in.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public IActivatorBuilder UseApplicationName(string applicationName)
        {
            return ApplicationActivator.UseApplicationName(applicationName);
        }

        /// <summary>
        /// Specify the application version to be used by the hosting application. 
        /// </summary>
        /// <param name="applicationVersion">The application version to host the application in.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public IActivatorBuilder UseApplicationVersion(Version applicationVersion)
        {
            return ApplicationActivator.UseApplicationVersion(applicationVersion);
        }

        /// <summary>
        /// Specifiy the startup method names to be used by the hosting application.
        /// </summary>
        /// <param name="methodNames">The method name to startup the hosting appliction.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public IActivatorBuilder UseStartupSteps(params string[] methodNames)
        {
            return ApplicationActivator.UseStartupSteps(methodNames);
        }

        /// <summary>
        /// Specifiy the shutdown method names to be used by the hosting application.
        /// </summary>
        /// <param name="methodNames">The method name to shutdown the hosting appliction.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public IActivatorBuilder UseShutdownSteps(params string[] methodNames)
        {
            return ApplicationActivator.UseShutdownSteps(methodNames);
        }

        /// <summary>
        /// Specify the component to be used by the hosting application.
        /// </summary>
        /// <param name="serviceType">The requested type of the component.</param>
        /// <param name="instance">The instance of the component.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public IActivatorBuilder UseService(Type serviceType, object instance)
        {
            return ApplicationActivator.UseService(serviceType, instance);
        }

        /// <summary>
        /// Specify the component to be used by the hosting application.
        /// </summary>
        /// <typeparam name="T">The requested type of the component.</typeparam>
        /// <param name="instance">The instance of the component.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public IActivatorBuilder UseService<T>(T instance)
        {
            return ApplicationActivator.UseService(instance);
        }

        /// <summary>
        /// Removes the registered component by using service type.
        /// </summary>
        /// <param name="serviceType">The registered type of the component.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public IActivatorBuilder RemoveService(Type serviceType)
        {
            return ApplicationActivator.RemoveService(serviceType);
        }

        /// <summary>
        /// Removes the registered component by using service type.
        /// </summary>
        /// <typeparam name="T">The registered type of the component.</typeparam>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public IActivatorBuilder RemoveService<T>()
        {
            return ApplicationActivator.RemoveService<T>();
        }

        /// <summary>
        /// Startup the hosting application.
        /// </summary>
        public void Startup()
        {
            ApplicationActivator.Startup();
        }
    }
}
