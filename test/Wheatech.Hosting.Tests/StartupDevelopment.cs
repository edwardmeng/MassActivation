using ReferenceAssembly;

namespace Wheatech.Hosting.Tests
{
    public class StartupDevelopment
    {
        public StartupDevelopment(IHostingEnvironment environment, IAppContext context)
        {
            context.UserName = "Wheatech";
        }
    }
}
