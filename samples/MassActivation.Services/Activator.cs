using MassActivation;

[assembly: AssemblyActivator(typeof(MassActivation.Services.Activator))]
namespace MassActivation.Services
{
    public class Activator
    {
        public Activator(IActivatingEnvironment environment)
        {
            environment.Use<ICacheService>(new MemoryCacheService());
        }
    }
}
