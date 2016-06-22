using System;
using ReferenceAssembly;
using Xunit;

namespace Wheatech.Hosting.Tests
{
    public class AppHostFixture
    {
        [Fact]
        public void ConstructorParameters()
        {
            AppHost
                .UseEnvironment(EnvironmentName.Development)
                .UseApplicationName("Wheatech.Hosting")
                .UseApplicationVersion(new Version("1.0"))
                .UseStartSteps().Startup();

            Assert.Equal(EnvironmentName.Development, AssemblyEnvironment.Environment);
            Assert.Equal("Wheatech.Hosting", AssemblyEnvironment.ApplicationName);
            Assert.Equal(new Version("1.0"), AssemblyEnvironment.ApplicationVersion);
            Assert.Equal("Wheatech", AssemblyEnvironment.UserName);
        }
    }
}
