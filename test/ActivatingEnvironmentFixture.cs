using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
#if NetCore
using System.Runtime.Loader;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit;
#else
using System.Reflection.Emit;
using NUnit.Framework;
#endif

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

#if NetCore
        public ActivatingEnvironmentFixture()
        {
            CompileHelper.ClearAssemblies();
        }
#else
        [SetUp]
        public void ClearAssemblies()
        {
            CompileHelper.ClearAssemblies();
        }
#endif

#if !NetCore
        [Test]
        public void DefaultApplicationName()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            Assert.AreEqual(GetAssemblyName(typeof(ActivatingEnvironmentFixture)).Name, environment.ApplicationName);
        }
#endif

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void SpecifyApplicationName()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            environment.UseApplicationName("MassActivation");
#if NetCore
            Assert.Equal("MassActivation", environment.ApplicationName);
#else
            Assert.AreEqual("MassActivation", environment.ApplicationName);
#endif
        }

#if !NetCore
        [Test]
        public void DefaultApplicationVersion()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            Assert.AreEqual(GetAssemblyName(typeof(ActivatingEnvironmentFixture)).Version, environment.ApplicationVersion);
        }
#endif

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void SpecifyApplicationVersion()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            environment.UseApplicationVersion(new Version("1.0.5"));
#if NetCore
            Assert.Equal(new Version("1.0.5"), environment.ApplicationVersion);
#else
            Assert.AreEqual(new Version("1.0.5"), environment.ApplicationVersion);
#endif
        }

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void DefaultEnvironment()
        {
            Environment.SetEnvironmentVariable("ACTIVATION_ENVIRONMENT", null);
            IActivatingEnvironment environment = new ActivatingEnvironment();
#if NetCore
            Assert.Equal(EnvironmentName.Production, environment.Environment);
#else
            Assert.AreEqual(EnvironmentName.Production, environment.Environment);
#endif
        }

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void DefaultEnvironmentFromSystemVariable()
        {
            Environment.SetEnvironmentVariable("ACTIVATION_ENVIRONMENT", EnvironmentName.Development);
            IActivatingEnvironment environment = new ActivatingEnvironment();
#if NetCore
            Assert.Equal(EnvironmentName.Development, environment.Environment);
#else
            Assert.AreEqual(EnvironmentName.Development, environment.Environment);
#endif
        }

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void SpecifyEnvironment()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            environment.UseEnvironment(EnvironmentName.Staging);
#if NetCore
            Assert.Equal(EnvironmentName.Staging, environment.Environment);
#else
            Assert.AreEqual(EnvironmentName.Staging, environment.Environment);
#endif
        }

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void DefaultServiceRegistration()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
#if NetCore
            Assert.Equal(environment, environment.Get<IActivatingEnvironment>());
            Assert.Equal(environment, environment.Get<IServiceProvider>());
            Assert.Null(environment.Get<ICustomFormatter>());
#else
            Assert.AreEqual(environment, environment.Get<IActivatingEnvironment>());
            Assert.AreEqual(environment, environment.Get<IServiceProvider>());
            Assert.Null(environment.Get<ICustomFormatter>());
#endif
        }

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void CustomServiceRegistration()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            environment.Use<IFormatProvider>(CultureInfo.CurrentCulture);
#if NetCore
            Assert.Equal(CultureInfo.CurrentCulture, environment.Get<IFormatProvider>());
#else
            Assert.AreEqual(CultureInfo.CurrentCulture, environment.Get<IFormatProvider>());
#endif
        }

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void RemoveServiceRegistration()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
#if NetCore
            Assert.Equal(environment, environment.Get<IServiceProvider>());
#else
            Assert.AreEqual(environment, environment.Get<IServiceProvider>());
#endif
            environment.Remove<IServiceProvider>();
            Assert.Null(environment.Get<IServiceProvider>());
        }

#if !NetCore

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

#endif

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void NotReferenceStaticAssembly()
        {
            Assert.True(CompileHelper.CreateAssembly("test.dll"));
            IActivatingEnvironment environment = new ActivatingEnvironment();
            var assembly = environment.GetAssemblies().SingleOrDefault(x => x.GetName().Name == "test");
            Assert.NotNull(assembly);
        }

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void RuntimeLoadedAssembly()
        {
#if NetCore
            var basePath = PlatformServices.Default.Application.ApplicationBasePath;
#else
            var basePath =  AppDomain.CurrentDomain.BaseDirectory;
#endif
            var assemblyPath = Path.Combine(new DirectoryInfo(basePath).Parent?.FullName, "test.dll");
            Assert.True(CompileHelper.CreateAssembly("../test.dll"));
            IActivatingEnvironment environment = new ActivatingEnvironment();
            var assembly = environment.GetAssemblies().SingleOrDefault(x => x.GetName().Name == "test");
            Assert.Null(assembly);
#if NetCore
            AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
#else
            Assembly.LoadFile(assemblyPath);
#endif
            assembly = environment.GetAssemblies().SingleOrDefault(x => x.GetName().Name == "test");
            Assert.NotNull(assembly);
        }
    }
}
