using System;

namespace Wheatech.Hosting
{
    /// <summary>
    /// Extension methods for <see cref="IHostingEnvironment"/>. 
    /// </summary>
    public static class HostingEnvironmentExtensions
    {
        /// <summary>
        /// Checks if the current hosting environment name is "Development". 
        /// </summary>
        /// <param name="hostingEnvironment">An instance of <see cref="IHostingEnvironment"/>.</param>
        /// <returns><c>true</c> if the environment name is "Development", otherwise <c>false</c>.</returns>
        public static bool IsDevelopment(this IHostingEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }
            return hostingEnvironment.IsEnvironment(EnvironmentName.Development);
        }

        /// <summary>
        /// Compares the current hosting environment name against the specified value. 
        /// </summary>
        /// <param name="hostingEnvironment">An instance of <see cref="IHostingEnvironment"/>.</param>
        /// <param name="environmentName">Environment name to validate against.</param>
        /// <returns><c>true</c> if the specified name is the same as the current environment, otherwise <c>false</c>.</returns>
        public static bool IsEnvironment(this IHostingEnvironment hostingEnvironment, string environmentName)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }
            return string.Equals(hostingEnvironment.Environment, environmentName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if the current hosting environment name is "Production". 
        /// </summary>
        /// <param name="hostingEnvironment">An instance of <see cref="IHostingEnvironment"/>.</param>
        /// <returns><c>true</c> if the environment name is "Production", otherwise <c>false</c>.</returns>
        public static bool IsProduction(this IHostingEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }
            return hostingEnvironment.IsEnvironment(EnvironmentName.Production);
        }

        /// <summary>
        /// Checks if the current hosting environment name is "Staging".
        /// </summary>
        /// <param name="hostingEnvironment">An instance of <see cref="IHostingEnvironment"/>.</param>
        /// <returns><c>true</c> if the environment name is "Staging", otherwise <c>false</c>.</returns>
        public static bool IsStaging(this IHostingEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }
            return hostingEnvironment.IsEnvironment(EnvironmentName.Staging);
        }

        /// <summary>
        /// Specify the environment to be used by the hosting application. 
        /// </summary>
        /// <param name="hostingEnvironment">An instance of <see cref="IHostingEnvironment"/>.</param>
        /// <param name="environmentName">The environment to host the application in.</param>
        /// <returns>The <see cref="IHostingEnvironment"/>.</returns>
        public static IHostingEnvironment UseEnvironment(this IHostingEnvironment hostingEnvironment, string environmentName)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }
            if (environmentName == null)
            {
                throw new ArgumentNullException(nameof(environmentName));
            }
            hostingEnvironment.Environment = environmentName;
            return hostingEnvironment;
        }

        /// <summary>
        /// Specify the application name to be used by the hosting application. 
        /// </summary>
        /// <param name="hostingEnvironment">An instance of <see cref="IHostingEnvironment"/>.</param>
        /// <param name="applicationName">The application name to host the application in.</param>
        /// <returns>The <see cref="IHostingEnvironment"/>.</returns>
        public static IHostingEnvironment UseApplicationName(this IHostingEnvironment hostingEnvironment, string applicationName)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }
            if (applicationName == null)
            {
                throw new ArgumentNullException(nameof(applicationName));
            }
            hostingEnvironment.ApplicationName = applicationName;
            return hostingEnvironment;
        }

        /// <summary>
        /// Specify the application version to be used by the hosting application. 
        /// </summary>
        /// <param name="hostingEnvironment">An instance of <see cref="IHostingEnvironment"/>.</param>
        /// <param name="applicationVersion">The application version to host the application in.</param>
        /// <returns>The <see cref="IHostingEnvironment"/>.</returns>
        public static IHostingEnvironment UseApplicationVersion(this IHostingEnvironment hostingEnvironment, Version applicationVersion)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }
            if (applicationVersion == null)
            {
                throw new ArgumentNullException(nameof(applicationVersion));
            }
            hostingEnvironment.ApplicationVersion = applicationVersion;
            return hostingEnvironment;
        }

        /// <summary>
        /// Specify the component to be used by the hosting application.
        /// </summary>
        /// <typeparam name="T">The requested type of the component.</typeparam>
        /// <param name="hostingEnvironment">An instance of <see cref="IHostingEnvironment"/>.</param>
        /// <param name="instance">The instance of the component.</param>
        /// <returns>The <see cref="IHostingEnvironment"/>.</returns>
        public static IHostingEnvironment Use<T>(this IHostingEnvironment hostingEnvironment, T instance)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }
            return hostingEnvironment.Use(typeof(T), instance);
        }
    }
}
