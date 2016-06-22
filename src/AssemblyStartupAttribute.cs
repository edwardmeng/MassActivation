using System;

namespace Wheatech.Hosting
{
    /// <summary>
    /// Used to mark which class in an assembly should be used for automatic startup. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyStartupAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyStartupAttribute"/> class 
        /// </summary>
        /// <param name="startupType">The startup class</param>
        public AssemblyStartupAttribute(Type startupType)
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
