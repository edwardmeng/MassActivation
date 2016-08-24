using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Xml;

namespace MassActivation
{
    internal partial class ActivatingEnvironment
    {
        private const string AspNetNamespace = "ASP";
        private static bool _applicationAssembliesLoaded;
        private static readonly object _lockObj = new object();

        private void Initialize()
        {
            var entryAssembly = GetEntryAssembly();
            if (entryAssembly != null)
            {
                ApplicationName = entryAssembly.GetName().Name;
                ApplicationVersion = entryAssembly.GetName().Version;
            }
            else
            {
                ApplicationName = AppDomain.CurrentDomain.FriendlyName;
            }
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
            LoadApplicationAssemblies();
            return (assemblies ?? Enumerable.Empty<Assembly>()).Union(AppDomain.CurrentDomain.GetAssemblies());
        }

        /// <summary>
        /// Return the entry assembly for difference hosting environments.
        /// 1. Windows Application & WPF Application
        /// 2. ASP.NET Application
        /// 3. Unit Test Application
        /// 4. Console Application
        /// 5. WCF Application
        /// </summary>
        /// <returns></returns>
        private Assembly GetEntryAssembly()
        {
            // windows applications or console applications.
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null) return entryAssembly;
            // asp.net web applications
            var type = HttpContext.Current?.ApplicationInstance?.GetType();
            while (type?.Namespace == AspNetNamespace)
            {
                type = type.BaseType;
            }
            entryAssembly = type?.Assembly;
            if (entryAssembly != null) return entryAssembly;
            // unit test
            var methodFrames = new StackTrace().GetFrames()?.Select(t => t.GetMethod()).ToArray();
            if (methodFrames == null) return null;
            MethodBase entryMethod = null;
            int firstInvokeMethod = 0;
            for (int i = 0; i < methodFrames.Length; i++)
            {
                var method = methodFrames[i] as MethodInfo;
                if (method == null)
                    continue;
                if (method.Name == "Main" && method.ReturnType == typeof(void))
                    entryMethod = method;
                else if (firstInvokeMethod == 0 && method.Name == "InvokeMethod" && method.IsStatic && method.DeclaringType == typeof(RuntimeMethodHandle))
                    firstInvokeMethod = i;
            }

            if (entryMethod == null)
                entryMethod = firstInvokeMethod != 0 ? methodFrames[firstInvokeMethod - 1] : methodFrames.Last();

            return entryMethod.Module.Assembly;
        }

        /// <summary>
        /// Load all the assemblies from private bin path.
        /// </summary>
        private void LoadPrivateAssemblies()
        {
            var info = AppDomain.CurrentDomain.SetupInformation;
            IEnumerable<string> searchPaths = new string[0];
            if (info.PrivateBinPathProbe == null || string.IsNullOrEmpty(info.PrivateBinPath))
            {
                // Check the current directory
                searchPaths = searchPaths.Concat(new[] { string.Empty });
            }
            if (!string.IsNullOrEmpty(info.PrivateBinPath))
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
