using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace MassActivation
{
    public static partial class ApplicationActivator
    {
        #region Instantiate

        private static ActivationMetadata SearchActivator(Assembly assembly)
        {
            if (_environment == null) return null;
            // Detect the startup type declared using AssemblyStartupAttribute
            var attributes = assembly.GetCustomAttributes<AssemblyActivatorAttribute>().ToArray();
            var startupAssemblyName = assembly.GetName().Name;
            var activatorAttribute = attributes.FirstOrDefault(attr => attr.Environment == Environment.Environment) ??
                                     attributes.FirstOrDefault(attr => string.Equals(attr.Environment, Environment.Environment, StringComparison.OrdinalIgnoreCase)) ??
                                     attributes.FirstOrDefault(attr => string.IsNullOrEmpty(attr.Environment));
            if (activatorAttribute != null)
            {
                var startupType = activatorAttribute.StartupType.GetTypeInfo();
                if (_excludedTypes != null && _excludedTypes.Contains(startupType.GetType()))
                {
                    return null;
                }
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
                var info = type?.GetTypeInfo();
                if (info != null && !info.IsGenericTypeDefinition && !info.IsInterface && !info.IsAbstract)
                {
                    return new ActivationMetadata(info);
                }
                return null;
            };
            var startupWithEnv = resolveActivator(assembly.GetType(startupNameWithEnv));
            if (startupWithEnv != null && (_excludedTypes == null || !_excludedTypes.Contains(startupWithEnv.GetTargetType())))
            {
                return startupWithEnv;
            }
            var startupWithEnvAndNamespace =
                resolveActivator(assembly.GetType(startupAssemblyName + "." + startupNameWithEnv));
            if (startupWithEnvAndNamespace != null && (_excludedTypes == null || !_excludedTypes.Contains(startupWithEnvAndNamespace.GetTargetType())))
            {
                return startupWithEnvAndNamespace;
            }
            var startupWithoutEnv = resolveActivator(assembly.GetType(startupNameWithoutEnv));
            if (startupWithoutEnv != null && (_excludedTypes == null || !_excludedTypes.Contains(startupWithoutEnv.GetTargetType())))
            {
                return startupWithoutEnv;
            }
            var startupWithNamespaceWithoutEnv = resolveActivator(assembly.GetType(startupAssemblyName + "." + startupNameWithoutEnv));
            if (startupWithNamespaceWithoutEnv != null && (_excludedTypes == null || !_excludedTypes.Contains(startupWithNamespaceWithoutEnv.GetTargetType())))
            {
                return startupWithNamespaceWithoutEnv;
            }
            return null;
        }

        private static ActivationMetadata LookupClassConstructor(ActivationType type)
        {
            var constructor = ((TypeInfo)type.Metadata.TargetMember).DeclaredConstructors.SingleOrDefault(ctor => ctor.IsStatic);
            return constructor == null ? null : new ActivationMetadata(constructor, type.Metadata.Priority);
        }

        private static ActivationMetadata LookupInstanceConstructor(ActivationType type)
        {
            var targetType = (TypeInfo)type.Metadata.TargetMember;
            if (targetType.IsAbstract && targetType.IsSealed) return null;
            var constructors = targetType.DeclaredConstructors.Where(ctor => !ctor.IsStatic && ctor.IsPublic).ToArray();
            if (constructors.Length == 0)
            {
                throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.NoPublicConstructor, TypeNameHelper.GetTypeDisplayName(targetType.AsType())));
            }
            if (constructors.Length > 1)
            {
                throw new ActivationException(string.Format(CultureInfo.CurrentCulture, Strings.Cannot_Multiple_PublicConstructor, TypeNameHelper.GetTypeDisplayName(targetType.AsType())));
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
                RuntimeHelpers.RunClassConstructor(((TypeInfo)constructor.TargetMember).AsType().TypeHandle);
            }
        }

        #endregion

        #region Event Listener

        private static void AttachEventListeners()
        {
            AssemblyLoadContext.Default.Unloading += OnUnloading;
        }

        private static void OnUnloading(AssemblyLoadContext obj)
        {
            Shutdown();
            AssemblyLoadContext.Default.Unloading -= OnUnloading;
        }

        #endregion
    }
}
