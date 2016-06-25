using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Wheatech.Activation
{
    internal class ActivatingEnvironment : IActivatingEnvironment
    {
        private readonly Hashtable _environment = new Hashtable();

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

        public object this[string name]
        {
            get { return _environment[name]; }
            set { _environment[name] = value; }
        }

        public string Environment
        {
            get { return (string)this["Environment"]; }
            set { this["Environment"] = value; }
        }

        public string ApplicationName
        {
            get { return (string)this["ApplicationName"]; }
            set { this["ApplicationName"] = value; }
        }

        public Version ApplicationVersion
        {
            get { return (Version)this["ApplicationVersion"]; }
            set { this["ApplicationVersion"] = value; }
        }

        public SetupEnvironment Setup { get; } = new SetupEnvironment();

        public IDictionary<Type, object> Components { get; } = new Dictionary<Type, object>();

        public IActivatingEnvironment Use(Type serviceType, object instance)
        {
            Components[serviceType] = instance;
            return this;
        }
    }
}
