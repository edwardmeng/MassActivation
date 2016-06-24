using ReferenceAssembly;
using Wheatech.Activation;

[assembly:AssemblyStartup(typeof(AssemblyLoader))]

namespace ReferenceAssembly
{
    public class AssemblyLoader
    {
        public AssemblyLoader(IActivatingEnvironment environment)
        {
            AssemblyEnvironment.Environment = environment.Environment;
            AssemblyEnvironment.ApplicationName = environment.ApplicationName;
            AssemblyEnvironment.ApplicationVersion = environment.ApplicationVersion;

            environment.Use<IAppContext>(new AppContext());
        }
    }
}
