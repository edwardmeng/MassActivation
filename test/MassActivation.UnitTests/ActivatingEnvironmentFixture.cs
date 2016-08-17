using Xunit;

namespace MassActivation.Tests
{
    public class ActivatingEnvironmentFixture
    {
        [Fact]
        public void DefaultApplicationName()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            Assert.Equal(typeof (ActivatingEnvironmentFixture).Assembly.GetName().Name, environment.ApplicationName);
        }
    }
}
