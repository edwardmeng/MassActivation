using System;
using ReferenceAssembly;
using Xunit;

namespace Wheatech.Activation.Tests
{
    public class AppHostFixture
    {
        [Fact]
        public void ConstructorParameters()
        {
            ApplicationActivator
                .UseEnvironment(EnvironmentName.Development)
                .UseApplicationName("Wheatech.Activation")
                .UseApplicationVersion(new Version("1.0"))
                .UseStartSteps().Startup();

            Assert.Equal(EnvironmentName.Development, AssemblyEnvironment.Environment);
            Assert.Equal("Wheatech.Activation", AssemblyEnvironment.ApplicationName);
            Assert.Equal(new Version("1.0"), AssemblyEnvironment.ApplicationVersion);
            Assert.Equal("Wheatech", AssemblyEnvironment.UserName);
        }
    }
}
