using MassActivation.Services;

namespace MassActivation.Web
{
    public class Startup
    {
        private static IActivatingEnvironment _environment;
        public void Configuration(IActivatingEnvironment environment, ICacheService cache)
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