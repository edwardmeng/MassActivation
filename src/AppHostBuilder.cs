using System;

namespace Wheatech.Hosting
{
    internal class AppHostBuilder : IAppHostBuilder
    {
        public IAppHostBuilder UseEnvironment(string environmentName)
        {
            AppHost.UseEnvironment(environmentName);
            return this;
        }

        public IAppHostBuilder UseApplicationName(string applicationName)
        {
            AppHost.UseApplicationName(applicationName);
            return this;
        }

        public IAppHostBuilder UseApplicationVersion(Version applicationVersion)
        {
            AppHost.UseApplicationVersion(applicationVersion);
            return this;
        }

        public IAppHostBuilder UseStartSteps(params string[] methodNames)
        {
            AppHost.UseStartSteps(methodNames);
            return this;
        }

        public IAppHostBuilder UseUnloadSteps(params string[] methodNames)
        {
            AppHost.UseUnloadSteps(methodNames);
            return this;
        }

        public void Startup()
        {
            AppHost.Startup();
        }
    }
}
