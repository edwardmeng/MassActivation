using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.PlatformAbstractions;

namespace MassActivation
{
    internal partial class ActivatingEnvironment
    {
        private readonly HashSet<Assembly> _dynamicAssemblies = new HashSet<Assembly>();
        private void Initialize()
        {
            var application = PlatformServices.Default.Application;
            ApplicationName = application.ApplicationName;
            ApplicationVersion = string.IsNullOrEmpty(application.ApplicationVersion) ? null : new Version(application.ApplicationVersion);
        }

        /// <summary>
        /// Returns all the assemblies to be used by the hosting application.
        /// </summary>
        /// <returns>All the assemblies to be used by the application.</returns>
        public IEnumerable<Assembly> GetAssemblies()
        {
            return DependencyContext.Default.GetDefaultAssemblyNames().Select(Assembly.Load).Union(_dynamicAssemblies);
        }

        /// <summary>
        /// Specify the dynamically loaded assembly.
        /// </summary>
        /// <param name="assembly">The assembly has been dynamically loaded.</param>
        public void UseAssembly(Assembly assembly)
        {
            if (assembly != null)
            {
                _dynamicAssemblies.Add(assembly);
            }
        }
    }
}
