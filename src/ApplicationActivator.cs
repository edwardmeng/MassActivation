﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using MassActivation.Properties;

namespace MassActivation
{
    /// <summary>
    /// The entry point for the application host activation.
    /// </summary>
    public static partial class ApplicationActivator
    {
        private sealed class Pair<TFirst, TSecond>
        {
            public TFirst First { get; }
            public TSecond Second { get; }

            public Pair(TFirst x, TSecond y)
            {
                First = x;
                Second = y;
            }
        }

        internal static ActivatingEnvironment Environment => _environment;

        private static Pair<TFirst, TSecond> CreatePair<TFirst, TSecond>(TFirst x, TSecond y)
        {
            return new Pair<TFirst, TSecond>(x, y);
        }

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
                                select CreatePair(type, constructor)).ToList();

            var instanceCount = -1;
            while (instanceCount != 0 && constructors.Count > 0)
            {
                instanceCount = 0;
                for (int i = 0; i < constructors.Count; i++)
                {
                    var ctor = constructors[i];
                    object instance;
                    if (TryCreateInstance((ConstructorInfo)ctor.Second.TargetMember, out instance))
                    {
                        ctor.First.TargetInstance = instance;
                        constructors.RemoveAt(i);
                        instanceCount++;
                        i--;
                    }
                }
            }
            if (constructors.Count > 0)
            {
                throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.CannotCreateInstances,
                    string.Join(", ", constructors.Select(x => TypeNameHelper.GetTypeDisplayName(x.First.GetTargetType())).ToArray())));
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
#if NetCore
            var methods = ((TypeInfo)type.TargetMember).DeclaredMethods.Where(method => method.IsPublic);
#else
            var methods = ((Type)type.TargetMember).GetMethods();
#endif

            foreach (var method in methods)
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
            var typeName = TypeNameHelper.GetTypeDisplayName(type.GetTargetType());
            if (nomalMethodsWithEnv.Count > 1)
            {
                throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_Multiple_StartupMethod, methodNameWithEnv, typeName));
            }
            if (nomalMethodsWithEnv.Count == 1)
            {
                return new ActivationMetadata(nomalMethodsWithEnv[0], type.Priority);
            }
            if (nomalMethodsWithoutEnv.Count > 1)
            {
                throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_Multiple_StartupMethod, methodNameWithoutEnv, typeName));
            }
            if (nomalMethodsWithoutEnv.Count == 1)
            {
                return new ActivationMetadata(nomalMethodsWithoutEnv[0], type.Priority);
            }
            if (genericMethodsWithEnv.Count > 0)
            {
                throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_GenericMethod, methodNameWithEnv, typeName));
            }
            if (genericMethodsWithoutEnv.Count > 0)
            {
                throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_GenericMethod, methodNameWithoutEnv, typeName));
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
                          select CreatePair(instance, method);
            if (startup)
            {
                methods = methods.OrderBy(x => x.Second.Priority).ThenBy(x => ((MethodBase)x.Second.TargetMember).GetParameters().Length);
            }
            else
            {
                methods = methods.OrderBy(x => x.Second.Priority).ThenByDescending(x => ((MethodBase)x.Second.TargetMember).GetParameters().Length);
            }
            var methodList = methods.ToList();
            var invokeMethodCount = -1;
            while (invokeMethodCount != 0 && methodList.Count > 0)
            {
                invokeMethodCount = 0;
                for (int i = 0; i < methodList.Count; i++)
                {
                    if (TryInvokeMethod((MethodInfo)methodList[i].Second.TargetMember, methodList[i].First.TargetInstance))
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
                    throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_InvokeMethod, methodList[0].Second.TargetMember.Name,
                        TypeNameHelper.GetTypeDisplayName(methodList[0].First.GetTargetType())));
                }
                else
                {
                    throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_Invoke_MultipleMethod,
                        string.Join(", ",
                            methodList.Select(method => TypeNameHelper.GetTypeDisplayName(method.First.GetTargetType()) + "." + TypeNameHelper.GetMethodDisplayName((MethodInfo)method.Second.TargetMember)).ToArray())));
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
                AttachEventListeners();
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
            Startup((IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Startup the specified assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies to be started.</param>
        /// <remarks>
        /// It is useful for the startup of dynamically loaded assemblies.
        /// </remarks>
        public static void Startup(IEnumerable<Assembly> assemblies)
        {
            if (_environment == null) return;
            lock (_environment)
            {
                var activators = SearchActivatorTypes(assemblies).ToArray();
                if (_activators == null)
                {
                    _activators = activators.ToList();
                }
                else
                {
                    _activators.AddRange(activators);
                }
                Startup(activators);
            }
        }

        /// <summary>
        /// Invoke the specified methods sequentially in all the registered startup types. 
        /// </summary>
        /// <param name="methods">The methods to be invoked.</param>
        public static void Invoke(params string[] methods)
        {
            Invoke((IEnumerable<string>)methods);
        }

        /// <summary>
        /// Invoke the specified methods sequentially in all the registered startup types. 
        /// </summary>
        /// <param name="methods">The methods to be invoked.</param>
        public static void Invoke(IEnumerable<string> methods)
        {
            if (_environment == null || methods == null) return;
            foreach (var methodName in methods)
            {
                InvokeMethods(_activators, methodName, true);
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
                if (_activators != null)
                {
                    Shutdown(_activators.ToArray());
                    _activators = null;
                }
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
            Shutdown((IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Shutdown the specified assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies to be shutdown.</param>
        /// <remarks>
        /// It is useful for the startup of dynamically loaded assemblies.
        /// </remarks>
        public static void Shutdown(IEnumerable<Assembly> assemblies)
        {
            if (_environment == null || _activators == null) return;
            lock (_environment)
            {
                Shutdown((from metadata in _activators
                          join assembly in assemblies on metadata.GetTargetAssembly() equals assembly
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

        private static void DisposeInstances(IEnumerable<ActivationMetadata> types)
        {
            if (types == null) return;
            foreach (var instance in types)
            {
                (instance.TargetInstance as IDisposable)?.Dispose();
            }
        }

        #endregion
    }
}
