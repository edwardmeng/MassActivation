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
        private void Initialize()
        {
            var application = PlatformServices.Default.Application;
            ApplicationName = application.ApplicationName;
            ApplicationVersion = string.IsNullOrEmpty(application.ApplicationVersion) ? null : new Version(application.ApplicationVersion);
        }

        #region Methods

        /// <summary>
        /// Returns all the assemblies to be used by the hosting application.
        /// </summary>
        /// <returns>All the assemblies to be used by the application.</returns>
        public IEnumerable<Assembly> GetAssemblies()
        {
            return DependencyContext.Default.GetDefaultAssemblyNames().Select(Assembly.Load);
        }

        #endregion
    }
}
