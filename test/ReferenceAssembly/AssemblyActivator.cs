using System.Linq;
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
            AssemblyEnvironment.Assemblies = environment.GetAssemblies().ToArray();

            environment.Use<IAppContext>(new AppContext());
        }
    }
}
