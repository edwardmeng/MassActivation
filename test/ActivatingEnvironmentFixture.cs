using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
#if NetCore
using Microsoft.Extensions.PlatformAbstractions;
#endif
using NUnit.Framework;

namespace MassActivation.UnitTests
{
    public class ActivatingEnvironmentFixture
    {
        private AssemblyName GetAssemblyName(Type type)
        {
#if NetCore
            return type.GetTypeInfo().Assembly.GetName();
#else
            return type.Assembly.GetName();
#endif
        }

        private string GetBaseDirectory()
        {
#if NetCore
            return PlatformServices.Default.Application.ApplicationBasePath;
#else
            return AppDomain.CurrentDomain.BaseDirectory;
#endif
        }
        [SetUp]
        public void ClearAssemblies()
        {
            CompileHelper.ClearAssemblies();
        }

        [Test]
        public void DefaultApplicationName()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            Assert.AreEqual(GetAssemblyName(typeof(ActivatingEnvironmentFixture)).Name, environment.ApplicationName);
        }

        [Test]
        public void SpecifyApplicationName()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            environment.UseApplicationName("MassActivation");
            Assert.AreEqual("MassActivation", environment.ApplicationName);
        }

        [Test]
        public void DefaultApplicationVersion()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            Assert.AreEqual(GetAssemblyName(typeof(ActivatingEnvironmentFixture)).Version, environment.ApplicationVersion);
        }

        [Test]
        public void SpecifyApplicationVersion()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            environment.UseApplicationVersion(new Version("1.0.5"));
            Assert.AreEqual(new Version("1.0.5"), environment.ApplicationVersion);
        }

        [Test]
        public void DefaultEnvironment()
        {
            Environment.SetEnvironmentVariable("ACTIVATION_ENVIRONMENT", null);
            IActivatingEnvironment environment = new ActivatingEnvironment();
            Assert.AreEqual(EnvironmentName.Production, environment.Environment);
        }

        [Test]
        public void DefaultEnvironmentFromSystemVariable()
        {
            Environment.SetEnvironmentVariable("ACTIVATION_ENVIRONMENT", EnvironmentName.Development);
            IActivatingEnvironment environment = new ActivatingEnvironment();
            Assert.AreEqual(EnvironmentName.Development, environment.Environment);
        }

        [Test]
        public void SpecifyEnvironment()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            environment.UseEnvironment(EnvironmentName.Staging);
            Assert.AreEqual(EnvironmentName.Staging, environment.Environment);
        }

        [Test]
        public void DefaultServiceRegistration()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            Assert.AreEqual(environment, environment.Get<IActivatingEnvironment>());
            Assert.AreEqual(environment, environment.Get<IServiceProvider>());
            Assert.Null(environment.Get<ICustomFormatter>());
        }

        [Test]
        public void CustomServiceRegistration()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            environment.Use<IFormatProvider>(CultureInfo.CurrentCulture);
            Assert.AreEqual(CultureInfo.CurrentCulture, environment.Get<IFormatProvider>());
        }

        [Test]
        public void RemoveServiceRegistration()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            Assert.AreEqual(environment, environment.Get<IServiceProvider>());
            environment.Remove<IServiceProvider>();
            Assert.Null(environment.Get<IServiceProvider>());
        }

        [Test]
        public void RuntimeDynamicAssembly()
        {
            var name = new AssemblyName("DynamicAssembly");
            AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            IActivatingEnvironment environment = new ActivatingEnvironment();
            var assembly = environment.GetAssemblies().SingleOrDefault(x => x.GetName().Name == "DynamicAssembly");
            Assert.NotNull(assembly);
            Assert.True(assembly.IsDynamic);
        }

        [Test]
        public void NotReferenceStaticAssembly()
        {
            Assert.True(CompileHelper.CreateAssembly("test.dll"));
            IActivatingEnvironment environment = new ActivatingEnvironment();
            var assembly = environment.GetAssemblies().SingleOrDefault(x => x.GetName().Name == "test");
            Assert.NotNull(assembly);
        }

        [Test]
        public void RuntimeLoadedAssembly()
        {
            var assemblyPath = Path.Combine(new DirectoryInfo(GetBaseDirectory()).Parent?.FullName, "test.dll");
            Assert.True(CompileHelper.CreateAssembly("../test.dll"));
            IActivatingEnvironment environment = new ActivatingEnvironment();
            var assembly = environment.GetAssemblies().SingleOrDefault(x => x.GetName().Name == "test");
            Assert.Null(assembly);
            Assembly.LoadFile(assemblyPath);
            assembly = environment.GetAssemblies().SingleOrDefault(x => x.GetName().Name == "test");
            Assert.NotNull(assembly);
        }
    }
}
