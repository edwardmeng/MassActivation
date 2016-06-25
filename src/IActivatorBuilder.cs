using System;

namespace Wheatech.Activation
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
        /// Startup the hosting application.
        /// </summary>
        void Startup();
    }
}
