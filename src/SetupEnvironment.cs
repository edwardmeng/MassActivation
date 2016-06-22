using System;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;

namespace Wheatech.Hosting
{
    /// <summary>
    /// Provides the informations for the application setup environment.
    /// </summary>
    public sealed class SetupEnvironment
    {
        private string _applicationBasePath;
        private FrameworkName _runtimeFramework;

        internal SetupEnvironment()
        {
        }

        /// <summary>
        /// Gets the base application physical directory of the hosting application.
        /// </summary>
        public string ApplicationBasePath
        {
            get
            {
                return _applicationBasePath ??
                       (_applicationBasePath = Path.GetFullPath(((string) AppDomain.CurrentDomain.GetData("APP_CONTEXT_BASE_DIRECTORY")) ?? AppDomain.CurrentDomain.BaseDirectory));
            }
        }

        /// <summary>
        /// Gets the runtime framework name of the hosting application.
        /// </summary>
        public FrameworkName RuntimeFramework
        {
            get
            {
                if (_runtimeFramework == null)
                {
                    var frameworkName = AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName;
                    if (string.IsNullOrEmpty(frameworkName))
                    {
                        frameworkName = Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
                    }
                    if (!string.IsNullOrEmpty(frameworkName))
                    {
                        _runtimeFramework = new FrameworkName(frameworkName);
                    }
                    else
                    {
                        _runtimeFramework = new FrameworkName(".NETFramework,Version=v4.0");
                    }
                }
                return _runtimeFramework;
            }
        }

        /// <summary>
        /// Gets the runtime .Net Framework version that the hosting application is running against.
        /// </summary>
        public Version RuntimeVersion { get; } = typeof(object).GetTypeInfo().Assembly.GetName().Version;
    }
}
