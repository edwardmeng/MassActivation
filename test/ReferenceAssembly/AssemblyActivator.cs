using ReferenceAssembly;
using Wheatech.Activation;

[assembly:AssemblyActivator(typeof(AssemblyActivator))]

namespace ReferenceAssembly
{
    public class AssemblyActivator
    {
        public AssemblyActivator(IActivatingEnvironment environment)
        {
            AssemblyEnvironment.Environment = environment.Environment;
            AssemblyEnvironment.ApplicationName = environment.ApplicationName;
            AssemblyEnvironment.ApplicationVersion = environment.ApplicationVersion;

            environment.Use<IAppContext>(new AppContext());
        }
    }
}
