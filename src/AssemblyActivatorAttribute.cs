using System;

namespace Wheatech.Activation
{
    /// <summary>
    /// Used to mark which class in an assembly should be used for automatic startup and shutdown. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyActivatorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyActivatorAttribute"/> class 
        /// </summary>
        /// <param name="startupType">The startup class</param>
        public AssemblyActivatorAttribute(Type startupType)
        {
            if (startupType == null)
            {
                throw new ArgumentNullException(nameof(startupType));
            }
            StartupType = startupType;
        }

        /// <summary>
        /// The startup class
        /// </summary>
        public Type StartupType { get; private set; }
    }
}
