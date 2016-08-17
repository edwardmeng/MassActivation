using MassActivation.Services;

namespace MassActivation.Console
{
    public class Startup
    {
        public void Configuration(IActivatingEnvironment environment, ICacheService cache)
        {
            System.Console.WriteLine($"Application Name: {environment.ApplicationName}");
            System.Console.WriteLine($"Application Version: {environment.ApplicationVersion}");

            cache.Set("ApplicationName", environment.ApplicationName);
            cache.Set("ApplicationVersion", environment.ApplicationVersion);
        }
    }
}
