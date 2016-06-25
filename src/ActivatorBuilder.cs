using System;

namespace Wheatech.Activation
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
            ApplicationActivator.UseEnvironment(environmentName);
            return this;
        }

        /// <summary>
        /// Specify the application name to be used by the hosting application. 
        /// </summary>
        /// <param name="applicationName">The application name to host the application in.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public IActivatorBuilder UseApplicationName(string applicationName)
        {
            ApplicationActivator.UseApplicationName(applicationName);
            return this;
        }

        /// <summary>
        /// Specify the application version to be used by the hosting application. 
        /// </summary>
        /// <param name="applicationVersion">The application version to host the application in.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public IActivatorBuilder UseApplicationVersion(Version applicationVersion)
        {
            ApplicationActivator.UseApplicationVersion(applicationVersion);
            return this;
        }

        /// <summary>
        /// Specifiy the startup method names to be used by the hosting application.
        /// </summary>
        /// <param name="methodNames">The method name to startup the hosting appliction.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public IActivatorBuilder UseStartupSteps(params string[] methodNames)
        {
            ApplicationActivator.UseStartupSteps(methodNames);
            return this;
        }

        /// <summary>
        /// Specifiy the shutdown method names to be used by the hosting application.
        /// </summary>
        /// <param name="methodNames">The method name to shutdown the hosting appliction.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public IActivatorBuilder UseShutdownSteps(params string[] methodNames)
        {
            ApplicationActivator.UseShutdownSteps(methodNames);
            return this;
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
