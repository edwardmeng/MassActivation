using System;

namespace MassActivation
{
    /// <summary>
    /// Extension methods for <see cref="IActivatingEnvironment"/>. 
    /// </summary>
    public static class ActivatingEnvironmentExtensions
    {
        /// <summary>
        /// Checks if the current hosting environment name is "Development". 
        /// </summary>
        /// <param name="environment">An instance of <see cref="IActivatingEnvironment"/>.</param>
        /// <returns><c>true</c> if the environment name is "Development", otherwise <c>false</c>.</returns>
        public static bool IsDevelopment(this IActivatingEnvironment environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }
            return environment.IsEnvironment(EnvironmentName.Development);
        }

        /// <summary>
        /// Compares the current hosting environment name against the specified value. 
        /// </summary>
        /// <param name="environment">An instance of <see cref="IActivatingEnvironment"/>.</param>
        /// <param name="environmentName">Environment name to validate against.</param>
        /// <returns><c>true</c> if the specified name is the same as the current environment, otherwise <c>false</c>.</returns>
        public static bool IsEnvironment(this IActivatingEnvironment environment, string environmentName)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }
            return string.Equals(environment.Environment, environmentName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if the current hosting environment name is "Production". 
        /// </summary>
        /// <param name="environment">An instance of <see cref="IActivatingEnvironment"/>.</param>
        /// <returns><c>true</c> if the environment name is "Production", otherwise <c>false</c>.</returns>
        public static bool IsProduction(this IActivatingEnvironment environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }
            return environment.IsEnvironment(EnvironmentName.Production);
        }

        /// <summary>
        /// Checks if the current hosting environment name is "Staging".
        /// </summary>
        /// <param name="environment">An instance of <see cref="IActivatingEnvironment"/>.</param>
        /// <returns><c>true</c> if the environment name is "Staging", otherwise <c>false</c>.</returns>
        public static bool IsStaging(this IActivatingEnvironment environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }
            return environment.IsEnvironment(EnvironmentName.Staging);
        }

        /// <summary>
        /// Specify the environment to be used by the hosting application. 
        /// </summary>
        /// <param name="environment">An instance of <see cref="IActivatingEnvironment"/>.</param>
        /// <param name="environmentName">The environment to host the application in.</param>
        /// <returns>The <see cref="IActivatingEnvironment"/>.</returns>
        public static IActivatingEnvironment UseEnvironment(this IActivatingEnvironment environment, string environmentName)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }
            if (environmentName == null)
            {
                throw new ArgumentNullException(nameof(environmentName));
            }
            environment.Environment = environmentName;
            return environment;
        }

        /// <summary>
        /// Specify the application name to be used by the hosting application. 
        /// </summary>
        /// <param name="environment">An instance of <see cref="IActivatingEnvironment"/>.</param>
        /// <param name="applicationName">The application name to host the application in.</param>
        /// <returns>The <see cref="IActivatingEnvironment"/>.</returns>
        public static IActivatingEnvironment UseApplicationName(this IActivatingEnvironment environment, string applicationName)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }
            if (applicationName == null)
            {
                throw new ArgumentNullException(nameof(applicationName));
            }
            environment.ApplicationName = applicationName;
            return environment;
        }

        /// <summary>
        /// Specify the application version to be used by the hosting application. 
        /// </summary>
        /// <param name="environment">An instance of <see cref="IActivatingEnvironment"/>.</param>
        /// <param name="applicationVersion">The application version to host the application in.</param>
        /// <returns>The <see cref="IActivatingEnvironment"/>.</returns>
        public static IActivatingEnvironment UseApplicationVersion(this IActivatingEnvironment environment, Version applicationVersion)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }
            if (applicationVersion == null)
            {
                throw new ArgumentNullException(nameof(applicationVersion));
            }
            environment.ApplicationVersion = applicationVersion;
            return environment;
        }

        /// <summary>
        /// Specify the component to be used by the hosting application.
        /// </summary>
        /// <typeparam name="T">The requested type of the component.</typeparam>
        /// <param name="environment">An instance of <see cref="IActivatingEnvironment"/>.</param>
        /// <param name="instance">The instance of the component.</param>
        /// <returns>The <see cref="IActivatingEnvironment"/>.</returns>
        public static IActivatingEnvironment Use<T>(this IActivatingEnvironment environment, T instance)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }
            return environment.Use(typeof(T), instance);
        }

        /// <summary>
        /// Removes the registered component by using service type.
        /// </summary>
        /// <typeparam name="T">The registered type of the component.</typeparam>
        /// <param name="environment">An instance of <see cref="IActivatingEnvironment"/>.</param>
        /// <returns>The <see cref="IActivatingEnvironment"/>.</returns>
        public static IActivatingEnvironment Remove<T>(this IActivatingEnvironment environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }
            return environment.Remove(typeof(T));
        }

        /// <summary>
        /// Gets the component by using the registered service type.
        /// </summary>
        /// <typeparam name="T">The registered type of the component.</typeparam>
        /// <param name="environment">An instance of <see cref="IActivatingEnvironment"/>.</param>
        /// <returns>The instance of the component.</returns>
        public static T Get<T>(this IActivatingEnvironment environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }
            return (T)(environment.Get(typeof(T)) ?? default(T));
        }
    }
}
