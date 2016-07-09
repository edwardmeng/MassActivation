using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Xml;

namespace Wheatech.Activation
{
    internal class ActivatingEnvironment : IActivatingEnvironment
    {
        private readonly Hashtable _environment = new Hashtable();
        private static bool _applicationAssembliesLoaded;
        private static readonly object _lockObj = new object();

        internal ActivatingEnvironment()
        {
            Environment = System.Environment.GetEnvironmentVariable("ACTIVATION_ENVIRONMENT") ?? EnvironmentName.Production;
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                ApplicationName = entryAssembly.GetName().Name;
                ApplicationVersion = entryAssembly.GetName().Version;
            }
            Components.Add(typeof(IActivatingEnvironment), this);
        }

        /// <summary>
        /// Gets or sets the environment variable by using the specified name.
        /// </summary>
        /// <param name="name">The environment variable name.</param>
        /// <returns>The value of the environment variable.</returns>
        public object this[string name]
        {
            get { return _environment[name]; }
            set { _environment[name] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the environment. This property is automatically set by the host to the value of the "ASPNETCORE_ENVIRONMENT" environment variable. 
        /// </summary>
        public string Environment
        {
            get { return (string)this["Environment"]; }
            set { this["Environment"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the application. This property is automatically set by the host to the assembly containing the application entry point. 
        /// </summary>
        public string ApplicationName
        {
            get { return (string)this["ApplicationName"]; }
            set { this["ApplicationName"] = value; }
        }

        /// <summary>
        /// Gets or sets the version of the application. This property is automatically set by the host to the assembly containing the application entry point. 
        /// </summary>
        public Version ApplicationVersion
        {
            get { return (Version)this["ApplicationVersion"]; }
            set { this["ApplicationVersion"] = value; }
        }

        /// <summary>
        /// Gets all the registered components.
        /// </summary>
        public IDictionary<Type, object> Components { get; } = new Dictionary<Type, object>();

        /// <summary>
        /// Specify the component to be used by the hosting application.
        /// </summary>
        /// <param name="serviceType">The requested type of the component.</param>
        /// <param name="instance">The instance of the component.</param>
        /// <returns>The <see cref="IActivatingEnvironment"/>.</returns>
        public IActivatingEnvironment Use(Type serviceType, object instance)
        {
            Components[serviceType] = instance;
            return this;
        }

        /// <summary>
        /// Removes the registered component by using service type.
        /// </summary>
        /// <param name="serviceType">The registered type of the component.</param>
        /// <returns>The <see cref="IActivatingEnvironment"/>.</returns>
        public IActivatingEnvironment Remove(Type serviceType)
        {
            Components.Remove(serviceType);
            return this;
        }

        /// <summary>
        /// Gets the component by using the registered service type.
        /// </summary>
        /// <param name="serviceType">The registered type of the component.</param>
        /// <returns>The instance of the component.</returns>
        public object Get(Type serviceType)
        {
            object instance;
            return Components.TryGetValue(serviceType, out instance) ? instance : null;
        }

        /// <summary>
        /// Returns all the assemblies to be used by the hosting application.
        /// </summary>
        /// <returns>All the assemblies to be used by the application.</returns>
        public IEnumerable<Assembly> GetAssemblies()
        {
            IEnumerable<Assembly> assemblies = null;
            if (HostingEnvironment.IsHosted)
            {
                try
                {
                    assemblies = BuildManager.GetReferencedAssemblies().OfType<Assembly>();
                }
                catch (Exception ex) when (ex is InvalidOperationException || ex is HttpException)
                {
                }
            }
            if (assemblies == null)
            {
                LoadApplicationAssemblies();
            }
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        /// <summary>
        /// Load all the assemblies from private bin path.
        /// </summary>
        private void LoadPrivateAssemblies()
        {
            var info = AppDomain.CurrentDomain.SetupInformation;
            IEnumerable<string> searchPaths = new string[0];
            if (info.PrivateBinPathProbe == null || string.IsNullOrWhiteSpace(info.PrivateBinPath))
            {
                // Check the current directory
                searchPaths = searchPaths.Concat(new[] { string.Empty });
            }
            if (!string.IsNullOrWhiteSpace(info.PrivateBinPath))
            {
                // PrivateBinPath may be a semicolon separated list of subdirectories.
                searchPaths = searchPaths.Concat(info.PrivateBinPath.Split(';'));
            }
            foreach (var searchPath in searchPaths)
            {
                string assembliesPath = Path.Combine(info.ApplicationBase, searchPath);
                if (!Directory.Exists(assembliesPath)) continue;
                var files = Directory.GetFiles(assembliesPath, "*.dll").Concat(Directory.GetFiles(assembliesPath, "*.exe"));
                foreach (var file in files)
                {
                    try
                    {
                        Assembly.Load(AssemblyName.GetAssemblyName(file));
                    }
                    catch (Exception ex)
                        when (
                            ex is BadImageFormatException || ex is Win32Exception || ex is ArgumentException || ex is FileNotFoundException ||
                            ex is PathTooLongException || ex is SecurityException)
                    {
                        // Not a managed dll/exe
                    }
                }
            }
        }

        /// <summary>
        /// Load all the assemblies from configuration dependent assemblies.
        /// </summary>
        private void LoadConfigurationAssemblies()
        {
            var appBase = new DirectoryInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
            FileInfo configFile = null;
            if (HostingEnvironment.IsHosted)
            {
                configFile = appBase.GetFiles("web.config", SearchOption.TopDirectoryOnly).SingleOrDefault();
            }
            else
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                {
                    configFile = appBase.GetFiles(entryAssembly.FullName + ".config", SearchOption.TopDirectoryOnly).SingleOrDefault();
                }
            }
            if (configFile != null)
            {
                var document = new XmlDocument();
                document.Load(configFile.FullName);
                var dependentAssemblyNodes = document.SelectNodes("configuration/runtime/assemblyBinding/dependentAssembly");
                if (dependentAssemblyNodes != null)
                {
                    foreach (XmlNode dependentAssemblyNode in dependentAssemblyNodes)
                    {
                        if (dependentAssemblyNode.NodeType == XmlNodeType.Element)
                        {
                            foreach (XmlNode childNode in dependentAssemblyNode.ChildNodes)
                            {
                                if (childNode.NodeType == XmlNodeType.Element && childNode.Name == "codeBase")
                                {
                                    var location = childNode.Attributes?.GetNamedItem("href")?.Value;
                                    if (!string.IsNullOrEmpty(location))
                                    {
                                        if (location.StartsWith("~/"))
                                        {
                                            location = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, location.Substring(2));
                                        }
                                        try
                                        {
                                            Assembly.LoadFrom(location);
                                        }
                                        catch (Exception ex)
                                            when (
                                                ex is BadImageFormatException || ex is Win32Exception || ex is ArgumentException || ex is FileNotFoundException ||
                                                ex is PathTooLongException || ex is SecurityException)
                                        {
                                            // Not a managed dll/exe
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load all the assemblies for the current application.
        /// </summary>
        private void LoadApplicationAssemblies()
        {
            if (!_applicationAssembliesLoaded)
            {
                lock (_lockObj)
                {
                    if (!_applicationAssembliesLoaded)
                    {
                        LoadPrivateAssemblies();
                        LoadConfigurationAssemblies();
                        _applicationAssembliesLoaded = true;
                    }
                }
            }
        }
    }
}
