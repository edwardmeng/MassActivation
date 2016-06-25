using System;

namespace Wheatech.Activation
{
    internal class ActivatorBuilder : IActivatorBuilder
    {
        public IActivatorBuilder UseEnvironment(string environmentName)
        {
            ApplicationActivator.UseEnvironment(environmentName);
            return this;
        }

        public IActivatorBuilder UseApplicationName(string applicationName)
        {
            ApplicationActivator.UseApplicationName(applicationName);
            return this;
        }

        public IActivatorBuilder UseApplicationVersion(Version applicationVersion)
        {
            ApplicationActivator.UseApplicationVersion(applicationVersion);
            return this;
        }

        public IActivatorBuilder UseStartupSteps(params string[] methodNames)
        {
            ApplicationActivator.UseStartupSteps(methodNames);
            return this;
        }

        public IActivatorBuilder UseShutdownSteps(params string[] methodNames)
        {
            ApplicationActivator.UseShutdownSteps(methodNames);
            return this;
        }

        public void Startup()
        {
            ApplicationActivator.Startup();
        }
    }
}
