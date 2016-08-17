using ReferenceAssembly;

namespace MassActivation.Tests
{
    public class StartupDevelopment
    {
        public StartupDevelopment(IActivatingEnvironment environment, IAppContext context)
        {
            context.UserName = "Wheatech";
        }
    }
}
