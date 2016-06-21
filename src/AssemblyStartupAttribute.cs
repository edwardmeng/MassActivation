using System;

namespace Wheatech.Hosting
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyStartupAttribute : Attribute
    {
        public AssemblyStartupAttribute(Type startupType)
        {
            if (startupType == null)
            {
                throw new ArgumentNullException(nameof(startupType));
            }
            StartupType = startupType;
        }

        public Type StartupType { get; private set; }
    }
}
