[assembly: MassActivation.AssemblyActivator(typeof(MassActivation.NetCore.Web.Activator))]
namespace MassActivation.NetCore.Web
{
    public class Activator
    {
        private static IActivatingEnvironment _environment;
        public void Configuration(IActivatingEnvironment environment, MassActivation.Services.ICacheService cache)
        {
            cache.Set("ApplicationName", environment.ApplicationName);
            cache.Set("ApplicationVersion", environment.ApplicationVersion);

            _environment = environment;
        }

        public static T GetService<T>()
        {
            return _environment.Get<T>();
        }
    }
}
