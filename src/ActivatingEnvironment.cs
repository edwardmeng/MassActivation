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
    }
}
