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

        public IActivatorBuilder UseStartSteps(params string[] methodNames)
        {
            ApplicationActivator.UseStartSteps(methodNames);
            return this;
        }

        public IActivatorBuilder UseUnloadSteps(params string[] methodNames)
        {
            ApplicationActivator.UseUnloadSteps(methodNames);
            return this;
        }

        public void Startup()
        {
            ApplicationActivator.Startup();
        }
    }
}
