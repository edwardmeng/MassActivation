using System;

namespace MassActivation
{
    /// <summary>
    /// Used to mark which class in an assembly should be used for automatic startup and shutdown. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class AssemblyActivatorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyActivatorAttribute"/> class 
        /// </summary>
        /// <param name="startupType">The startup class</param>
        /// <param name="environment">The environment to host the application in.</param>
        public AssemblyActivatorAttribute(Type startupType, string environment = null)
        {
            if (startupType == null)
            {
                throw new ArgumentNullException(nameof(startupType));
            }
            StartupType = startupType;
            Environment = environment;
        }

        /// <summary>
        /// The startup class
        /// </summary>
        public Type StartupType { get; private set; }

        /// <summary>
        /// Gets the name of the environment. 
        /// </summary>
        public string Environment { get; private set; }
    }
}
