using System;

namespace Wheatech.Hosting
{
    /// <summary>
    /// The configuration interface for the <see cref="AppHost"/> environment variables.
    /// </summary>
    public interface IAppHostBuilder
    {
        /// <summary>
        /// Specify the environment to be used by the hosting application. 
        /// </summary>
        /// <param name="environmentName">The environment to host the application in.</param>
        /// <returns>The <see cref="IAppHostBuilder"/>.</returns>
        IAppHostBuilder UseEnvironment(string environmentName);

        /// <summary>
        /// Specify the application name to be used by the hosting application. 
        /// </summary>
        /// <param name="applicationName">The application name to host the application in.</param>
        /// <returns>The <see cref="IAppHostBuilder"/>.</returns>
        IAppHostBuilder UseApplicationName(string applicationName);

        /// <summary>
        /// Specify the application version to be used by the hosting application. 
        /// </summary>
        /// <param name="applicationVersion">The application version to host the application in.</param>
        /// <returns>The <see cref="IAppHostBuilder"/>.</returns>
        IAppHostBuilder UseApplicationVersion(Version applicationVersion);

        /// <summary>
        /// Specifiy the startup method names to be used by the hosting application.
        /// </summary>
        /// <param name="methodNames">The method name to startup the hosting appliction.</param>
        /// <returns>The <see cref="IAppHostBuilder"/>.</returns>
        IAppHostBuilder UseStartSteps(params string[] methodNames);

        /// <summary>
        /// Specifiy the unload method names to be used by the hosting application.
        /// </summary>
        /// <param name="methodNames">The method name to unload the hosting appliction.</param>
        /// <returns>The <see cref="IAppHostBuilder"/>.</returns>
        IAppHostBuilder UseUnloadSteps(params string[] methodNames);

        /// <summary>
        /// Startup the hosting application.
        /// </summary>
        void Startup();
    }
}
