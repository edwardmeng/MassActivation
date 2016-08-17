using System.Windows;
using MassActivation.Services;

namespace MassActivation.WPF
{
    public class Startup
    {
        public void Configuration(IActivatingEnvironment environment, ICacheService cache, Application application)
        {
            cache.Set("ApplicationName", environment.ApplicationName);
            cache.Set("ApplicationVersion", environment.ApplicationVersion);

            ((App)application).Configuration(cache);
        }
    }
}
