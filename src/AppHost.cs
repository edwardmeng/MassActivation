using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Compilation;
using Wheatech.Hosting.Properties;

namespace Wheatech.Hosting
{
    /// <summary>
    /// The entry point for the application host startup.
    /// </summary>
    public static class AppHost
    {
        #region Fields

        private static string _environment;
        private static string _applicationName;
        private static Version _applicationVersion;
        private static string[] _startupMethodNames = { "Configuration" };
        private static string[] _unloadMethodNames = { "Unload" };
        private static HostingEnvironment _hostingEnvironment;
        private static IDictionary<Type, object> _instances;

        #endregion

        #region Instantiate

        private static IEnumerable<Type> SearchStartupTypes()
        {
            if (_hostingEnvironment == null) yield break;
            foreach (var assembly in _hostingEnvironment.GetAssemblies())
            {
                // Detect the startup type declared using AssemblyStartupAttribute
                AssemblyStartupAttribute attribute;
                try
                {
                    attribute = assembly.GetCustomAttribute<AssemblyStartupAttribute>();
                }
                catch (CustomAttributeFormatException)
                {
                    continue;
                }
                var startupAssemblyName = assembly.GetName().Name;
                if (attribute != null)
                {
                    if (attribute.StartupType.IsGenericTypeDefinition)
                    {
                        throw new HostingException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_Startup_GenericType, TypeNameHelper.GetTypeDisplayName(attribute.StartupType), startupAssemblyName));
                    }
                    yield return attribute.StartupType;
                }
                else
                {
                    // Detect the startup type by using the convension name.
                    var startupNameWithoutEnv = "Startup";
                    var startupNameWithEnv = "Startup" + _hostingEnvironment.Environment;
                    var startupType =
                        assembly.GetType(startupNameWithEnv, false) ??
                        assembly.GetType(startupAssemblyName + "." + startupNameWithEnv, false) ??
                        assembly.GetType(startupNameWithoutEnv, false) ??
                        assembly.GetType(startupAssemblyName + "." + startupNameWithoutEnv, false);
                    if (startupType != null)
                    {
                        yield return startupType;
                    }
                }
            }
        }

        private static ConstructorInfo LookupConstructor(Type type)
        {
            if (type.IsAbstract || type.IsInterface)
            {
                return null;
            }
            var constructors = type.GetConstructors();
            if (constructors.Length == 0)
            {
                throw new HostingException(string.Format(CultureInfo.CurrentCulture, Strings.NoPublicConstructor, TypeNameHelper.GetTypeDisplayName(type)));
            }
            if (constructors.Length > 1)
            {
                throw new HostingException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_Multiple_PublicConstructor, TypeNameHelper.GetTypeDisplayName(type)));
            }
            return constructors[0];
        }

        private static IDictionary<Type, object> CreateInstances(IEnumerable<Type> types)
        {
            var constructors = (from type in types
                                let constructor = ValidateMethod(LookupConstructor(type))
                                orderby constructor.GetParameters().Length
                                select Tuple.Create(type, constructor)).ToList();

            var instances = new Dictionary<Type, object>();
            int instanceCount = -1;
            while (instanceCount != 0)
            {
                instanceCount = 0;
                for (int i = 0; i < constructors.Count; i++)
                {
                    var ctor = constructors[i];
                    object instance;
                    if (TryCreateInstance(ctor.Item2, out instance))
                    {
                        instances.Add(ctor.Item1, instance);
                        constructors.RemoveAt(i);
                        instanceCount++;
                        i--;
                    }
                }
            }
            if (constructors.Count > 0)
            {
                throw new HostingException(string.Format(CultureInfo.CurrentCulture, Strings.CannotCreateInstances,
                    string.Join(", ", constructors.Select(x => TypeNameHelper.GetTypeDisplayName(x.Item1)))));
            }
            return instances;
        }

        private static bool TryCreateInstance(ConstructorInfo constructor, out object instance)
        {
            instance = null;
            if (_hostingEnvironment == null) return false;
            var arguments = new List<object>();
            if (constructor == null) return true;
            foreach (var parameter in constructor.GetParameters())
            {
                object value;
                if (_hostingEnvironment.Components.TryGetValue(parameter.ParameterType, out value))
                {
                    arguments.Add(value);
                }
                else
                {
                    return false;
                }
            }
            instance = constructor.Invoke(arguments.ToArray());
            return true;
        }

        private static void DisposeInstances()
        {
            if (_instances == null) return;
            foreach (var instance in _instances)
            {
                (instance.Value as IDisposable)?.Dispose();
            }
        }

        #endregion

        #region Invocation

        private static MethodInfo LookupMethod(Type type, string methodName, string environment)
        {
            var genericMethodsWithEnv = new List<MethodInfo>();
            var nomalMethodsWithEnv = new List<MethodInfo>();
            var genericMethodsWithoutEnv = new List<MethodInfo>();
            var nomalMethodsWithoutEnv = new List<MethodInfo>();
            var methodNameWithEnv = methodName + environment;
            var methodNameWithoutEnv = methodName;
            foreach (var method in type.GetMethods())
            {
                if (type.Name != "Startup" + environment)
                {
                    if (method.Name == methodNameWithEnv)
                    {
                        if (method.IsGenericMethodDefinition)
                        {
                            genericMethodsWithEnv.Add(method);
                        }
                        else
                        {
                            nomalMethodsWithEnv.Add(method);
                        }
                    }
                }
                if (method.Name == methodNameWithoutEnv)
                {
                    if (method.IsGenericMethodDefinition)
                    {
                        genericMethodsWithoutEnv.Add(method);
                    }
                    else
                    {
                        nomalMethodsWithoutEnv.Add(method);
                    }
                }
            }
            if (nomalMethodsWithEnv.Count > 1)
            {
                throw new HostingException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_Multiple_StartupMethod, methodNameWithEnv, TypeNameHelper.GetTypeDisplayName(type)));
            }
            if (nomalMethodsWithEnv.Count == 1)
            {
                return nomalMethodsWithEnv[0];
            }
            if (nomalMethodsWithoutEnv.Count > 1)
            {
                throw new HostingException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_Multiple_StartupMethod, methodNameWithoutEnv, TypeNameHelper.GetTypeDisplayName(type)));
            }
            if (nomalMethodsWithoutEnv.Count == 1)
            {
                return nomalMethodsWithoutEnv[0];
            }
            if (genericMethodsWithEnv.Count > 0)
            {
                throw new HostingException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_GenericMethod, methodNameWithEnv, TypeNameHelper.GetTypeDisplayName(type)));
            }
            if (genericMethodsWithoutEnv.Count > 0)
            {
                throw new HostingException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_GenericMethod, methodNameWithoutEnv, TypeNameHelper.GetTypeDisplayName(type)));
            }
            return null;
        }

        private static T ValidateMethod<T>(T method)
            where T : MethodBase
        {
            foreach (var parameter in method.GetParameters())
            {
                if (parameter.IsOut || parameter.ParameterType.IsByRef)
                {
                    if (method.IsConstructor)
                    {
                        throw new HostingException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidConstructorParameter, parameter.Name,
                            TypeNameHelper.GetTypeDisplayName(method.DeclaringType)));
                    }
                    else
                    {
                        throw new HostingException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidMethodParameter, parameter.Name, method.Name,
                            TypeNameHelper.GetTypeDisplayName(method.DeclaringType)));
                    }
                }
            }
            return method;
        }

        private static void InvokeMethods(string methodName, bool startup)
        {
            if (_hostingEnvironment == null || _instances == null) return;
            var methods = (from type in _instances.Keys
                           let method = ValidateMethod(LookupMethod(type, methodName, _hostingEnvironment.Environment))
                           orderby method.GetParameters().Length
                           select Tuple.Create(type, method)).ToList();
            if (!startup)
            {
                methods.Reverse();
            }
            var invokeMethodCount = -1;
            if (invokeMethodCount != 0)
            {
                invokeMethodCount = 0;
                for (int i = 0; i < methods.Count; i++)
                {
                    if (TryInvokeMethod(methods[i].Item2, _instances[methods[i].Item1]))
                    {
                        methods.RemoveAt(i);
                        invokeMethodCount++;
                        i--;
                    }
                }
            }
            if (methods.Count > 0)
            {
                throw new AggregateException(
                    methods.Select(method =>
                            new HostingException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_InvokeMethod, method.Item2.Name,
                                TypeNameHelper.GetTypeDisplayName(method.Item2.DeclaringType)))));
            }
        }

        private static bool TryInvokeMethod(MethodInfo method, object instance)
        {
            if (_hostingEnvironment == null) return false;
            var arguments = new List<object>();
            foreach (var parameter in method.GetParameters())
            {
                object value;
                if (_hostingEnvironment.Components.TryGetValue(parameter.ParameterType, out value))
                {
                    arguments.Add(value);
                }
                else
                {
                    return false;
                }
            }
            method.Invoke(method.IsStatic ? null : instance, arguments.ToArray());
            return true;
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Specify the environment to be used by the hosting application. 
        /// </summary>
        /// <param name="environmentName">The environment to host the application in.</param>
        /// <returns>The <see cref="IAppHostBuilder"/>.</returns>
        public static IAppHostBuilder UseEnvironment(string environmentName)
        {
            if (environmentName == null)
            {
                throw new ArgumentNullException(nameof(environmentName));
            }
            lock (typeof(AppHost))
            {
                _environment = environmentName;
            }
            return new AppHostBuilder();
        }

        /// <summary>
        /// Specify the application name to be used by the hosting application. 
        /// </summary>
        /// <param name="applicationName">The application name to host the application in.</param>
        /// <returns>The <see cref="IAppHostBuilder"/>.</returns>
        public static IAppHostBuilder UseApplicationName(string applicationName)
        {
            if (applicationName == null)
            {
                throw new ArgumentNullException(nameof(applicationName));
            }
            lock (typeof(AppHost))
            {
                _applicationName = applicationName;
            }
            return new AppHostBuilder();
        }

        /// <summary>
        /// Specify the application version to be used by the hosting application. 
        /// </summary>
        /// <param name="applicationVersion">The application version to host the application in.</param>
        /// <returns>The <see cref="IAppHostBuilder"/>.</returns>
        public static IAppHostBuilder UseApplicationVersion(Version applicationVersion)
        {
            if (applicationVersion == null)
            {
                throw new ArgumentNullException(nameof(applicationVersion));
            }
            lock (typeof(AppHost))
            {
                _applicationVersion = applicationVersion;
            }
            return new AppHostBuilder();
        }

        /// <summary>
        /// Specifiy the startup method names to be used by the hosting application.
        /// </summary>
        /// <param name="methodNames">The method name to startup the hosting appliction.</param>
        /// <returns>The <see cref="IAppHostBuilder"/>.</returns>
        public static IAppHostBuilder UseStartSteps(params string[] methodNames)
        {
            lock (typeof(AppHost))
            {
                _startupMethodNames = methodNames;
            }
            return new AppHostBuilder();
        }

        /// <summary>
        /// Specifiy the unload method names to be used by the hosting application.
        /// </summary>
        /// <param name="methodNames">The method name to unload the hosting appliction.</param>
        /// <returns>The <see cref="IAppHostBuilder"/>.</returns>
        public static IAppHostBuilder UseUnloadSteps(params string[] methodNames)
        {
            lock (typeof(AppHost))
            {
                _unloadMethodNames = methodNames;
            }
            return new AppHostBuilder();
        }

        #endregion

        #region Event Handlers

        private static void OnDomainUnload(object sender, EventArgs e)
        {
            Unload();
        }

        private static void OnStopListening(object sender, EventArgs e)
        {
            Unload();
        }

        private static void OnAppDomainShutdown(object sender, BuildManagerHostUnloadEventArgs args)
        {
            Unload();
        }

        #endregion

        #region Entry Points

        /// <summary>
        /// Startup the hosting application.
        /// </summary>
        public static void Startup()
        {
            _hostingEnvironment = new HostingEnvironment();
            lock (_hostingEnvironment)
            {
                if (!string.IsNullOrEmpty(_environment))
                {
                    _hostingEnvironment.Environment = _environment;
                }
                if (!string.IsNullOrEmpty(_applicationName))
                {
                    _hostingEnvironment.ApplicationName = _applicationName;
                }
                if (_applicationVersion != null)
                {
                    _hostingEnvironment.ApplicationVersion = _applicationVersion;
                }
                _instances = CreateInstances(SearchStartupTypes());
                foreach (var methodName in _startupMethodNames)
                {
                    InvokeMethods(methodName, true);
                }
                Thread.GetDomain().DomainUnload += OnDomainUnload;
                typeof(HttpRuntime).GetEvent("AppDomainShutdown", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?
                    .AddMethod.Invoke(null, new object[] { new BuildManagerHostUnloadEventHandler(OnAppDomainShutdown) });
                System.Web.Hosting.HostingEnvironment.StopListening += OnStopListening;
            }
        }

        /// <summary>
        /// Unload the hosting application.
        /// </summary>
        public static void Unload()
        {
            if (_hostingEnvironment == null) return;
            lock (_hostingEnvironment)
            {
                foreach (var methodName in _unloadMethodNames)
                {
                    InvokeMethods(methodName, false);
                }
                DisposeInstances();
                _instances = null;
                _hostingEnvironment = null;
            }
        }

        #endregion
    }
}
