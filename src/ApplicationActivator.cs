using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Compilation;
using Wheatech.Activation.Properties;

namespace Wheatech.Activation
{
    /// <summary>
    /// The entry point for the application host activation.
    /// </summary>
    public static class ApplicationActivator
    {
        #region Fields

        private static string _environmentName;
        private static string _applicationName;
        private static Version _applicationVersion;
        private static string[] _startupMethodNames = { "Configuration" };
        private static string[] _shutdownMethodNames = { "Unload" };
        private static ActivatingEnvironment _environment;
        private static List<ActivationMetadata> _activators;

        #endregion

        #region Instantiate

        private static IEnumerable<ActivationMetadata> SearchActivatorTypes(IEnumerable<Assembly> assemblies)
        {
            if (_environment == null)
            {
                return Enumerable.Empty<ActivationMetadata>();
            }
            return from assembly in assemblies
                   let metadata = SearchActivator(assembly)
                   where metadata != null
                   select metadata;
        }

        private static ActivationMetadata SearchActivator(Assembly assembly)
        {
            if (_environment == null) return null;
            // Detect the startup type declared using AssemblyStartupAttribute
            AssemblyActivatorAttribute attribute;
            try
            {
                attribute = assembly.GetCustomAttribute<AssemblyActivatorAttribute>();
            }
            catch (CustomAttributeFormatException)
            {
                return null;
            }
            var startupAssemblyName = assembly.GetName().Name;
            if (attribute != null)
            {
                if (attribute.StartupType.IsGenericTypeDefinition)
                {
                    throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_Startup_GenericType, TypeNameHelper.GetTypeDisplayName(attribute.StartupType), startupAssemblyName));
                }
                if (attribute.StartupType.IsInterface || attribute.StartupType.IsAbstract)
                {
                    throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_AbstractOrInterface, TypeNameHelper.GetTypeDisplayName(attribute.StartupType), startupAssemblyName));
                }
                return new ActivationMetadata(attribute.StartupType);
            }
            else
            {
                // Detect the startup type by using the convension name.
                var startupNameWithoutEnv = "Startup";
                var startupNameWithEnv = "Startup" + _environment.Environment;
                var startupType =
                    assembly.GetType(startupNameWithEnv, false) ??
                    assembly.GetType(startupAssemblyName + "." + startupNameWithEnv, false) ??
                    assembly.GetType(startupNameWithoutEnv, false) ??
                    assembly.GetType(startupAssemblyName + "." + startupNameWithoutEnv, false);
                if (startupType != null && !startupType.IsGenericTypeDefinition && !startupType.IsInterface && !startupType.IsAbstract)
                {
                    return new ActivationMetadata(startupType);
                }
            }
            return null;
        }

        private static ActivationMetadata LookupClassConstructor(ActivationMetadata type)
        {
            var constructor = ((Type)type.TargetMember).GetTypeInfo().DeclaredConstructors.FirstOrDefault(ctor => ctor.IsStatic);
            return constructor == null ? null : new ActivationMetadata(constructor, type.Priority);
        }

        private static ActivationMetadata LookupInstanceConstructor(ActivationMetadata type)
        {
            var targetType = (Type)type.TargetMember;
            if (targetType.IsAbstract && targetType.IsSealed)
            {
                return null;
            }
            var constructors = targetType.GetConstructors();
            if (constructors.Length == 0)
            {
                throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.NoPublicConstructor, TypeNameHelper.GetTypeDisplayName(type)));
            }
            if (constructors.Length > 1)
            {
                throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_Multiple_PublicConstructor, TypeNameHelper.GetTypeDisplayName(type)));
            }
            return new ActivationMetadata(constructors[0], type.Priority);
        }

        /// <summary>
        /// Invoke static constructors according to their priority.
        /// </summary>
        private static void InvokeClassConstructors(IEnumerable<ActivationMetadata> types)
        {
            if (_environment == null || types == null) return;
            var staticConstructors = from type in types
                                     let constructor = LookupClassConstructor(type)
                                     where constructor != null
                                     orderby constructor.Priority
                                     select type;
            foreach (var constructor in staticConstructors)
            {
                RuntimeHelpers.RunClassConstructor(((Type)constructor.TargetMember).TypeHandle);
            }
        }

        /// <summary>
        /// Invoke instance constructors according to their priority, parameter number and parameter types.
        /// </summary>
        private static void CreateInstances(IEnumerable<ActivationMetadata> types)
        {
            if (_environment == null || types == null) return;
            var constructors = (from type in types
                                let constructor = ValidateMethod(LookupInstanceConstructor(type))
                                where constructor != null
                                orderby constructor.Priority, ((MethodBase)constructor.TargetMember).GetParameters().Length
                                select Tuple.Create(type, constructor)).ToList();

            var instanceCount = -1;
            while (instanceCount != 0)
            {
                instanceCount = 0;
                for (int i = 0; i < constructors.Count; i++)
                {
                    var ctor = constructors[i];
                    object instance;
                    if (TryCreateInstance((ConstructorInfo)ctor.Item2.TargetMember, out instance))
                    {
                        ctor.Item1.TargetInstance = instance;
                        constructors.RemoveAt(i);
                        instanceCount++;
                        i--;
                    }
                }
            }
            if (constructors.Count > 0)
            {
                throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.CannotCreateInstances,
                    string.Join(", ", constructors.Select(x => TypeNameHelper.GetTypeDisplayName(x.Item1)))));
            }
        }

        private static bool TryCreateInstance(ConstructorInfo constructor, out object instance)
        {
            instance = null;
            if (_environment == null) return false;
            var arguments = new List<object>();
            if (constructor == null) return true;
            foreach (var parameter in constructor.GetParameters())
            {
                object value;
                if (_environment.Components.TryGetValue(parameter.ParameterType, out value))
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

        #endregion

        #region Invocation

        private static ActivationMetadata LookupMethod(ActivationMetadata type, string methodName, string environment)
        {
            var genericMethodsWithEnv = new List<MethodInfo>();
            var nomalMethodsWithEnv = new List<MethodInfo>();
            var genericMethodsWithoutEnv = new List<MethodInfo>();
            var nomalMethodsWithoutEnv = new List<MethodInfo>();
            var methodNameWithEnv = methodName + environment;
            var methodNameWithoutEnv = methodName;
            foreach (var method in ((Type)type.TargetMember).GetMethods())
            {
                if (type.TargetMember.Name != "Startup" + environment)
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
                throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_Multiple_StartupMethod, methodNameWithEnv, TypeNameHelper.GetTypeDisplayName(type)));
            }
            if (nomalMethodsWithEnv.Count == 1)
            {
                return new ActivationMetadata(nomalMethodsWithEnv[0], type.Priority);
            }
            if (nomalMethodsWithoutEnv.Count > 1)
            {
                throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_Multiple_StartupMethod, methodNameWithoutEnv, TypeNameHelper.GetTypeDisplayName(type)));
            }
            if (nomalMethodsWithoutEnv.Count == 1)
            {
                return new ActivationMetadata(nomalMethodsWithoutEnv[0], type.Priority);
            }
            if (genericMethodsWithEnv.Count > 0)
            {
                throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_GenericMethod, methodNameWithEnv, TypeNameHelper.GetTypeDisplayName(type)));
            }
            if (genericMethodsWithoutEnv.Count > 0)
            {
                throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_GenericMethod, methodNameWithoutEnv, TypeNameHelper.GetTypeDisplayName(type)));
            }
            return null;
        }

        private static ActivationMetadata ValidateMethod(ActivationMetadata metadata)
        {
            if (metadata == null) return null;
            var method = (MethodBase)metadata.TargetMember;
            foreach (var parameter in method.GetParameters())
            {
                if (parameter.IsOut || parameter.ParameterType.IsByRef)
                {
                    if (method.IsConstructor)
                    {
                        throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidConstructorParameter, parameter.Name,
                            TypeNameHelper.GetTypeDisplayName(method.DeclaringType)));
                    }
                    else
                    {
                        throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidMethodParameter, parameter.Name, method.Name,
                            TypeNameHelper.GetTypeDisplayName(method.DeclaringType)));
                    }
                }
            }
            return metadata;
        }

        private static void InvokeMethods(IEnumerable<ActivationMetadata> types, string methodName, bool startup)
        {
            if (_environment == null || types == null) return;
            var methods = from instance in types
                          let method = ValidateMethod(LookupMethod(instance, methodName, _environment.Environment))
                          where method != null
                          orderby method.Priority, ((MethodBase)method.TargetMember).GetParameters().Length
                          select Tuple.Create(instance, method);
            if (startup)
            {
                methods = methods.OrderBy(x => x.Item2.Priority).ThenBy(x => ((MethodBase)x.Item2.TargetMember).GetParameters().Length);
            }
            else
            {
                methods = methods.OrderBy(x => x.Item2.Priority).ThenByDescending(x => ((MethodBase)x.Item2.TargetMember).GetParameters().Length);
            }
            var methodList = methods.ToList();
            var invokeMethodCount = -1;
            if (invokeMethodCount != 0)
            {
                invokeMethodCount = 0;
                for (int i = 0; i < methodList.Count; i++)
                {
                    if (TryInvokeMethod((MethodInfo)methodList[i].Item2.TargetMember, methodList[i].Item1.TargetInstance))
                    {
                        methodList.RemoveAt(i);
                        invokeMethodCount++;
                        i--;
                    }
                }
            }
            if (methodList.Count > 0)
            {
                if (methodList.Count == 1)
                {
                    throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_InvokeMethod, methodList[0].Item2.TargetMember.Name,
                        TypeNameHelper.GetTypeDisplayName(methodList[0].Item1.TargetMember)));
                }
                else
                {
                    throw new AggregateException(
                        methodList.Select(method =>
                                new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_InvokeMethod, method.Item2.TargetMember.Name,
                                    TypeNameHelper.GetTypeDisplayName(method.Item1.TargetMember)))));
                }
            }
        }

        private static bool TryInvokeMethod(MethodInfo method, object instance)
        {
            if (_environment == null) return false;
            var arguments = new List<object>();
            foreach (var parameter in method.GetParameters())
            {
                object value;
                if (_environment.Components.TryGetValue(parameter.ParameterType, out value))
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
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public static IActivatorBuilder UseEnvironment(string environmentName)
        {
            if (environmentName == null)
            {
                throw new ArgumentNullException(nameof(environmentName));
            }
            lock (typeof(ApplicationActivator))
            {
                _environmentName = environmentName;
            }
            return new ActivatorBuilder();
        }

        /// <summary>
        /// Specify the application name to be used by the hosting application. 
        /// </summary>
        /// <param name="applicationName">The application name to host the application in.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public static IActivatorBuilder UseApplicationName(string applicationName)
        {
            if (applicationName == null)
            {
                throw new ArgumentNullException(nameof(applicationName));
            }
            lock (typeof(ApplicationActivator))
            {
                _applicationName = applicationName;
            }
            return new ActivatorBuilder();
        }

        /// <summary>
        /// Specify the application version to be used by the hosting application. 
        /// </summary>
        /// <param name="applicationVersion">The application version to host the application in.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public static IActivatorBuilder UseApplicationVersion(Version applicationVersion)
        {
            if (applicationVersion == null)
            {
                throw new ArgumentNullException(nameof(applicationVersion));
            }
            lock (typeof(ApplicationActivator))
            {
                _applicationVersion = applicationVersion;
            }
            return new ActivatorBuilder();
        }

        /// <summary>
        /// Specifiy the startup method names to be used by the hosting application.
        /// </summary>
        /// <param name="methodNames">The method name to startup the hosting appliction.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public static IActivatorBuilder UseStartupSteps(params string[] methodNames)
        {
            lock (typeof(ApplicationActivator))
            {
                _startupMethodNames = methodNames;
            }
            return new ActivatorBuilder();
        }

        /// <summary>
        /// Specifiy the shutdown method names to be used by the hosting application.
        /// </summary>
        /// <param name="methodNames">The method name to shutdown the hosting appliction.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public static IActivatorBuilder UseShutdownSteps(params string[] methodNames)
        {
            lock (typeof(ApplicationActivator))
            {
                _shutdownMethodNames = methodNames;
            }
            return new ActivatorBuilder();
        }

        /// <summary>
        /// Specify the component to be used by the hosting application.
        /// </summary>
        /// <param name="serviceType">The requested type of the component.</param>
        /// <param name="instance">The instance of the component.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public static IActivatorBuilder UseService(Type serviceType, object instance)
        {
            if (_environment == null)
            {
                _environment = new ActivatingEnvironment();
            }
            _environment.Use(serviceType, instance);
            return new ActivatorBuilder();
        }

        /// <summary>
        /// Specify the component to be used by the hosting application.
        /// </summary>
        /// <typeparam name="T">The requested type of the component.</typeparam>
        /// <param name="instance">The instance of the component.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public static IActivatorBuilder UseService<T>(T instance)
        {
            return UseService(typeof(T), instance);
        }

        /// <summary>
        /// Removes the registered component by using service type.
        /// </summary>
        /// <param name="serviceType">The registered type of the component.</param>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public static IActivatorBuilder RemoveService(Type serviceType)
        {
            _environment?.Remove(serviceType);
            return new ActivatorBuilder();
        }

        /// <summary>
        /// Removes the registered component by using service type.
        /// </summary>
        /// <typeparam name="T">The registered type of the component.</typeparam>
        /// <returns>The <see cref="IActivatorBuilder"/>.</returns>
        public static IActivatorBuilder RemoveService<T>()
        {
            RemoveService(typeof(T));
            return new ActivatorBuilder();
        }

        #endregion

        #region Startup

        /// <summary>
        /// Startup the hosting application.
        /// </summary>
        public static void Startup()
        {
            if (_environment == null)
            {
                _environment = new ActivatingEnvironment();
            }
            lock (_environment)
            {
                // Apply the configuration variables to environment.
                if (!string.IsNullOrEmpty(_environmentName))
                {
                    _environment.Environment = _environmentName;
                }
                if (!string.IsNullOrEmpty(_applicationName))
                {
                    _environment.ApplicationName = _applicationName;
                }
                if (_applicationVersion != null)
                {
                    _environment.ApplicationVersion = _applicationVersion;
                }
                _activators = SearchActivatorTypes(_environment.GetAssemblies()).ToList();
                Startup(_activators.ToArray());
                // Attach events to shutdown the application.
                AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
                typeof(HttpRuntime).GetEvent("AppDomainShutdown", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?
                    .AddMethod.Invoke(null, new object[] { new BuildManagerHostUnloadEventHandler(OnAppDomainShutdown) });
                System.Web.Hosting.HostingEnvironment.StopListening += OnStopListening;
            }
        }

        /// <summary>
        /// Startup the specified assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies to be started.</param>
        /// <remarks>
        /// It is useful for the startup of dynamically loaded assemblies.
        /// </remarks>
        public static void Startup(params Assembly[] assemblies)
        {
            if (_environment == null) return;
            lock (_environment)
            {
                var activators = SearchActivatorTypes(assemblies).ToArray();
                _activators.AddRange(activators);
                Startup(activators);
            }
        }

        /// <summary>
        /// Process startup steps: static constructor, instance constructors, startup methods.
        /// </summary>
        private static void Startup(ActivationMetadata[] types)
        {
            InvokeClassConstructors(types);
            CreateInstances(types);
            foreach (var methodName in _startupMethodNames)
            {
                InvokeMethods(types, methodName, true);
            }
        }

        #endregion

        #region Shutdown

        /// <summary>
        /// Shutdown the hosting application.
        /// </summary>
        public static void Shutdown()
        {
            if (_environment == null) return;
            lock (_environment)
            {
                Shutdown(_activators.ToArray());
                _activators = null;
                _environment = null;
            }
        }

        /// <summary>
        /// Shutdown the specified assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies to be shutdown.</param>
        /// <remarks>
        /// It is useful for the startup of dynamically loaded assemblies.
        /// </remarks>
        public static void Shutdown(params Assembly[] assemblies)
        {
            if (_environment == null) return;
            lock (_environment)
            {
                Shutdown((from metadata in _activators
                          join assembly in assemblies on ((Type)metadata.TargetMember).Assembly equals assembly
                          select metadata).ToArray());
            }
        }

        /// <summary>
        /// Process the shutdown steps.
        /// </summary>
        private static void Shutdown(ActivationMetadata[] types)
        {
            if (types == null) return;
            foreach (var methodName in _shutdownMethodNames)
            {
                InvokeMethods(types, methodName, false);
            }
            DisposeInstances(types);
            foreach (var activator in types)
            {
                _activators.Remove(activator);
            }
        }
		
        private void RemoveEventHandlers()
        {
            AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
            System.Web.Hosting.HostingEnvironment.StopListening -= OnStopListening;
            typeof(HttpRuntime).GetEvent("AppDomainShutdown", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?
              .RemoveMethod.Invoke(null, new object[] { new BuildManagerHostUnloadEventHandler(OnAppDomainShutdown) });
        }

        private static void DisposeInstances(IEnumerable<ActivationMetadata> types)
        {
            if (types == null) return;
            foreach (var instance in types)
            {
                (instance.TargetInstance as IDisposable)?.Dispose();
            }
        }

        private static void OnDomainUnload(object sender, EventArgs e)
        {
            Shutdown();
			RemoveEventHandlers();
        }

        private static void OnStopListening(object sender, EventArgs e)
        {
            Shutdown();
			RemoveEventHandlers();
        }

        private static void OnAppDomainShutdown(object sender, BuildManagerHostUnloadEventArgs args)
        {
            Shutdown();
			RemoveEventHandlers();
        }

        #endregion
    }
}
