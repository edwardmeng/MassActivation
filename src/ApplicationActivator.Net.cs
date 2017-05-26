using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Compilation;

namespace MassActivation
{
    public static partial class ApplicationActivator
    {
        #region Instantiate

        private static ActivationMetadata SearchActivator(Assembly assembly)
        {
            if (_environment == null) return null;
            // Detect the startup type declared using AssemblyStartupAttribute
            AssemblyActivatorAttribute[] attributes;
            try
            {
                attributes = Attribute.GetCustomAttributes(assembly, typeof(AssemblyActivatorAttribute)).OfType<AssemblyActivatorAttribute>().ToArray();
            }
            catch (CustomAttributeFormatException)
            {
                return null;
            }
            var startupAssemblyName = assembly.GetName().Name;
            var activatorAttribute = attributes.FirstOrDefault(attr => attr.Environment == Environment.Environment) ??
                                     attributes.FirstOrDefault(attr => string.Equals(attr.Environment, Environment.Environment, StringComparison.OrdinalIgnoreCase)) ??
                                     attributes.FirstOrDefault(attr => string.IsNullOrEmpty(attr.Environment));
            if (activatorAttribute != null)
            {
                var startupType = activatorAttribute.StartupType;
                if (_excludedTypes != null && _excludedTypes.Contains(startupType)) return null;
                if (startupType.IsGenericTypeDefinition)
                {
                    throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_Startup_GenericType, TypeNameHelper.GetTypeDisplayName(activatorAttribute.StartupType), startupAssemblyName));
                }
                if (startupType.IsInterface || startupType.IsAbstract)
                {
                    throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_AbstractOrInterface, TypeNameHelper.GetTypeDisplayName(activatorAttribute.StartupType), startupAssemblyName));
                }
                return new ActivationMetadata(startupType);
            }
            // Detect the startup type by using the convension name.
            var startupNameWithoutEnv = "Startup";
            var startupNameWithEnv = "Startup" + _environment.Environment;
            Func<Type, ActivationMetadata> resolveActivator = type =>
            {
                if (type != null && !type.IsGenericTypeDefinition && !type.IsInterface && !type.IsAbstract)
                {
                    return new ActivationMetadata(type);
                }
                return null;
            };
            var startupWithEnv = resolveActivator(assembly.GetType(startupNameWithEnv, false));
            if (startupWithEnv != null && (_excludedTypes == null || !_excludedTypes.Contains(startupWithEnv.GetType())))
            {
                return startupWithEnv;
            }
            var startupWithEnvAndNamespace =
                resolveActivator(assembly.GetType(startupAssemblyName + "." + startupNameWithEnv, false));
            if (startupWithEnvAndNamespace != null && (_excludedTypes == null || !_excludedTypes.Contains(startupWithEnvAndNamespace.GetType())))
            {
                return startupWithEnvAndNamespace;
            }
            var startupWithoutEnv = resolveActivator(assembly.GetType(startupNameWithoutEnv, false));
            if (startupWithoutEnv != null && (_excludedTypes == null || !_excludedTypes.Contains(startupWithoutEnv.GetType())))
            {
                return startupWithoutEnv;
            }
            var startupWithNamespaceWithoutEnv = resolveActivator(assembly.GetType(startupAssemblyName + "." + startupNameWithoutEnv, false));
            if (startupWithNamespaceWithoutEnv != null && (_excludedTypes == null || !_excludedTypes.Contains(startupWithNamespaceWithoutEnv.GetType())))
            {
                return startupWithNamespaceWithoutEnv;
            }
            return null;
        }

        private static ActivationMetadata LookupClassConstructor(ActivationType type)
        {
            var constructor = ((Type)type.Metadata.TargetMember).GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).FirstOrDefault();
            return constructor == null ? null : new ActivationMetadata(constructor, type.Metadata.Priority);
        }

        private static ActivationMetadata LookupInstanceConstructor(ActivationType type)
        {
            var targetType = (Type)type.Metadata.TargetMember;
            if (targetType.IsAbstract && targetType.IsSealed) return null;
            var constructors = targetType.GetConstructors();
            if (constructors.Length == 0)
            {
                throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.NoPublicConstructor, TypeNameHelper.GetTypeDisplayName(targetType)));
            }
            if (constructors.Length > 1)
            {
                throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_Multiple_PublicConstructor, TypeNameHelper.GetTypeDisplayName(targetType)));
            }
            return new ActivationMetadata(constructors[0], type.Metadata.Priority);
        }

        /// <summary>
        /// Invoke static constructors according to their priority.
        /// </summary>
        private static void InvokeClassConstructors(IEnumerable<ActivationType> types)
        {
            if (_environment == null || types == null) return;
            var staticConstructors = from type in types
                                     let constructor = LookupClassConstructor(type)
                                     where constructor != null
                                     orderby constructor.Priority
                                     select type.Metadata;
            foreach (var constructor in staticConstructors)
            {
                RuntimeHelpers.RunClassConstructor(((Type)constructor.TargetMember).TypeHandle);
            }
        }

        #endregion

        #region Event Listener

        private static void AttachEventListeners()
        {
            // Attach events to shutdown the application.
            AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
            typeof(HttpRuntime).GetEvent("AppDomainShutdown", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?
                .GetAddMethod(true).Invoke(null, new object[] { new BuildManagerHostUnloadEventHandler(OnAppDomainShutdown) });
#if Net451
                System.Web.Hosting.HostingEnvironment.StopListening += OnStopListening;
#endif
        }


#if Net451
        private static void OnStopListening(object sender, EventArgs e)
        {
            Shutdown();
            RemoveEventListeners();
        }
#endif

        private static void OnDomainUnload(object sender, EventArgs e)
        {
            Shutdown();
            RemoveEventListeners();
        }

        private static void OnAppDomainShutdown(object sender, BuildManagerHostUnloadEventArgs args)
        {
            Shutdown();
            RemoveEventListeners();
        }

        private static void RemoveEventListeners()
        {
            AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
#if Net451
            System.Web.Hosting.HostingEnvironment.StopListening -= OnStopListening;
#endif
            typeof(HttpRuntime).GetEvent("AppDomainShutdown", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?
              .GetRemoveMethod(true).Invoke(null, new object[] { new BuildManagerHostUnloadEventHandler(OnAppDomainShutdown) });
        }

        #endregion
    }
}
