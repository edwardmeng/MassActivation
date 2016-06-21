using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;

namespace Wheatech.Hosting
{
    internal class HostingEnvironment : IHostingEnvironment
    {
        private readonly Hashtable _environment = new Hashtable();

        internal HostingEnvironment()
        {
            Environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? EnvironmentName.Production;
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                ApplicationName = entryAssembly.GetName().Name;
                ApplicationVersion = entryAssembly.GetName().Version;
            }
            Components.Add(typeof(IHostingEnvironment), this);
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

        public IHostingEnvironment Use(Type serviceType, object instance)
        {
            Components[serviceType] = instance;
            return this;
        }
    }
}
