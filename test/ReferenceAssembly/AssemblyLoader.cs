using ReferenceAssembly;
using Wheatech.Hosting;

[assembly:AssemblyStartup(typeof(AssemblyLoader))]

namespace ReferenceAssembly
{
    public class AssemblyLoader
    {
        public AssemblyLoader(IHostingEnvironment environment)
        {
            AssemblyEnvironment.Environment = environment.Environment;
            AssemblyEnvironment.ApplicationName = environment.ApplicationName;
            AssemblyEnvironment.ApplicationVersion = environment.ApplicationVersion;

            environment.Use<IAppContext>(new AppContext());
        }
    }
}
