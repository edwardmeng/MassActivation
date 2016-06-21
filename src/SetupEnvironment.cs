using System;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;

namespace Wheatech.Hosting
{
    public sealed class SetupEnvironment
    {
        private string _applicationBasePath;
        private FrameworkName _runtimeFramework;

        internal SetupEnvironment()
        {
        }

        public Version ApplicationVersion { get; } = Assembly.GetEntryAssembly()?.GetName().Version;

        public string ApplicationBasePath
        {
            get
            {
                return _applicationBasePath ??
                       (_applicationBasePath = Path.GetFullPath(((string) AppDomain.CurrentDomain.GetData("APP_CONTEXT_BASE_DIRECTORY")) ?? AppDomain.CurrentDomain.BaseDirectory));
            }
        }

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

        public Version RuntimeVersion { get; } = typeof(object).GetTypeInfo().Assembly.GetName().Version;
    }
}
