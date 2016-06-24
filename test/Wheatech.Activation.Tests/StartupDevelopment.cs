using ReferenceAssembly;

namespace Wheatech.Activation.Tests
{
    public class StartupDevelopment
    {
        public StartupDevelopment(IActivatingEnvironment environment, IAppContext context)
        {
            context.UserName = "Wheatech";
        }
    }
}
