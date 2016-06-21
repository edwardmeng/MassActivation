using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Wheatech.Hosting
{
    public class DefaultLoader
    {
        private static IEnumerable<Type> SearchStartupTypes(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var attribute = assembly.GetCustomAttribute<AssemblyStartupAttribute>();
                if (attribute != null)
                {
                    yield return attribute.StartupType;
                }
                else
                {
                    var type = assembly.GetType("Startup", false) ?? assembly.GetType(assembly.GetName().Name + ".Startup", false);
                    if (type != null)
                    {
                        yield return type;
                    }
                }
            }
        }

        private static IDictionary<Type, object> CreateInstances(IEnumerable<Type> types, HostingEnvironment environment)
        {
            var constructors = new List<Tuple<Type, ConstructorInfo>>();
            foreach (var type in types)
            {
                var ctors = type.GetConstructors();
                if (ctors.Length != 1)
                {
                    throw new InvalidOperationException($"The startup type {type} must have the only one public constructor.");
                }
                foreach (var parameter in ctors[0].GetParameters())
                {
                    if (parameter.IsOut || parameter.ParameterType.IsByRef)
                    {
                        throw new InvalidOperationException($"The parameter for the constructor of type {type} cannot be out or ref.");
                    }
                }
                constructors.Add(Tuple.Create(type, ctors[0]));
            }
            constructors.Sort((x, y) => x.Item2.GetParameters().Length - y.Item2.GetParameters().Length);
            var instances = new Dictionary<Type, object>();
            int createdInstanceCount = -1;
            while (createdInstanceCount != 0)
            {
                createdInstanceCount = 0;
                for (int i = 0; i < constructors.Count; i++)
                {
                    var ctor = constructors[i];
                    object instance;
                    if (TryCreateInstance(ctor.Item2, environment, out instance))
                    {
                        instances.Add(ctor.Item1, instance);
                        constructors.RemoveAt(i);
                        createdInstanceCount++;
                        i--;
                    }
                }
            }
            if (constructors.Count > 0)
            {
                throw new InvalidOperationException($"The startup types cannot be instantiated: {string.Join(", ", constructors.Select(x => x.Item1.FullName))}.");
            }
            return instances;
        }

        private static bool TryCreateInstance(ConstructorInfo constructor, HostingEnvironment environment, out object instance)
        {
            var arguments = new List<object>();
            instance = null;
            foreach (var parameter in constructor.GetParameters())
            {
                object value;
                if (environment.Components.TryGetValue(parameter.ParameterType, out value))
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
    }
}
