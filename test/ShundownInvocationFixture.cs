using System;
using System.Reflection;
#if NetCore
using Xunit;
#else
using NUnit.Framework;
#endif

namespace MassActivation.UnitTests
{
    public class ShundownInvocationFixture
    {
#if NetCore
        public ShundownInvocationFixture()
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

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void InvokeStaticMethod()
        {
            Assert.True(CompileHelper.CreateAssembly("test.dll",
                "using MassActivation;\r\n" +
                "public class Startup{\r\n" +
                    "public static void Shutdown(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationName(\"TestApplication\");\r\n" +
                    "}\r\n" +
                "}"));
            ApplicationActivator.Startup();
#if NetCore
            Assert.NotEqual("TestApplication", ApplicationActivator.Environment.ApplicationName);
#else
            Assert.AreNotEqual("TestApplication", ApplicationActivator.Environment.ApplicationName);
#endif
            ApplicationActivator.Shutdown(Assembly.Load(new AssemblyName("test")));
#if NetCore
            Assert.Equal("TestApplication", ApplicationActivator.Environment.ApplicationName);
#else
            Assert.AreEqual("TestApplication", ApplicationActivator.Environment.ApplicationName);
#endif
        }

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void InvokeInstanceMethod()
        {
            Assert.True(CompileHelper.CreateAssembly("test.dll",
                "using MassActivation;\r\n" +
                "public class Startup{\r\n" +
                    "public void Shutdown(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationName(\"TestApplication\");\r\n" +
                    "}\r\n" +
                "}"));
            ApplicationActivator.Startup();
#if NetCore
            Assert.NotEqual("TestApplication", ApplicationActivator.Environment.ApplicationName);
#else
            Assert.AreNotEqual("TestApplication", ApplicationActivator.Environment.ApplicationName);
#endif
            ApplicationActivator.Shutdown(Assembly.Load(new AssemblyName("test")));
#if NetCore
            Assert.Equal("TestApplication", ApplicationActivator.Environment.ApplicationName);
#else
            Assert.AreEqual("TestApplication", ApplicationActivator.Environment.ApplicationName);
#endif
        }

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void InvokeDisposeMethod()
        {
            Assert.True(CompileHelper.CreateAssembly("test.dll",
                "using MassActivation;\r\n" +
                "public class Startup:System.IDisposable{\r\n" +
                    "public void Dispose(){\r\n" +
                        "System.Environment.SetEnvironmentVariable(\"TestVariable\",\"MassActivation\");\r\n" +
                    "}\r\n" +
                "}"));
            ApplicationActivator.Startup();
#if NetCore
            Assert.NotEqual("MassActivation", Environment.GetEnvironmentVariable("TestVariable"));
#else
            Assert.AreNotEqual("MassActivation", Environment.GetEnvironmentVariable("TestVariable"));
#endif
            ApplicationActivator.Shutdown(Assembly.Load(new AssemblyName("test")));
#if NetCore
            Assert.Equal("MassActivation", Environment.GetEnvironmentVariable("TestVariable"));
#else
            Assert.AreEqual("MassActivation", Environment.GetEnvironmentVariable("TestVariable"));
#endif
        }

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void InvokeMethodDefaultSequence()
        {
            Assert.True(CompileHelper.CreateAssembly("test1.dll",
             "using MassActivation;\r\n" +
             "public class Startup{\r\n" +
                 "public void Shutdown(IActivatingEnvironment environment, System.IServiceProvider provider){\r\n" +
                     "environment.UseApplicationName(\"TestApplication1\");\r\n" +
                 "}\r\n" +
             "}"));
            Assert.True(CompileHelper.CreateAssembly("test2.dll",
              "using MassActivation;\r\n" +
              "public class Startup{\r\n" +
                  "public void Shutdown(IActivatingEnvironment environment){\r\n" +
                      "environment.UseApplicationName(\"TestApplication2\");\r\n" +
                  "}\r\n" +
              "}"));
            ApplicationActivator.Startup();
            ApplicationActivator.Shutdown(Assembly.Load(new AssemblyName("test1")), Assembly.Load(new AssemblyName("test2")));
#if NetCore
            Assert.Equal("TestApplication2", ApplicationActivator.Environment.ApplicationName);
#else
            Assert.AreEqual("TestApplication2", ApplicationActivator.Environment.ApplicationName);
#endif
        }

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void InvokeMethodSpecifiedPriority()
        {
            Assert.True(CompileHelper.CreateAssembly("test1.dll",
             "using MassActivation;\r\n" +
             "public class Startup{\r\n" +
                 "public void Shutdown(IActivatingEnvironment environment, System.IServiceProvider provider){\r\n" +
                     "environment.UseApplicationName(\"TestApplication1\");\r\n" +
                 "}\r\n" +
             "}"));
            Assert.True(CompileHelper.CreateAssembly("test2.dll",
              "using MassActivation;\r\n" +
              "public class Startup{\r\n" +
                "[ActivationPriority(ActivationPriority.High)]" +
                  "public void Shutdown(IActivatingEnvironment environment){\r\n" +
                      "environment.UseApplicationName(\"TestApplication2\");\r\n" +
                  "}\r\n" +
              "}"));
            ApplicationActivator.Startup();
            ApplicationActivator.Shutdown(Assembly.Load(new AssemblyName("test1")), Assembly.Load(new AssemblyName("test2")));
#if NetCore
            Assert.Equal("TestApplication1", ApplicationActivator.Environment.ApplicationName);
#else
            Assert.AreEqual("TestApplication1", ApplicationActivator.Environment.ApplicationName);
#endif
        }

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void StartupClassSpecifyPriority()
        {
            Assert.True(CompileHelper.CreateAssembly("test1.dll",
             "using MassActivation;\r\n" +
             "public class Startup{\r\n" +
                 "public void Shutdown(IActivatingEnvironment environment, System.IServiceProvider provider){\r\n" +
                     "environment.UseApplicationName(\"TestApplication1\");\r\n" +
                 "}\r\n" +
             "}"));
            Assert.True(CompileHelper.CreateAssembly("test2.dll",
              "using MassActivation;\r\n" +
             "[ActivationPriority(ActivationPriority.High)]" +
              "public class Startup{\r\n" +
                  "public void Shutdown(IActivatingEnvironment environment){\r\n" +
                      "environment.UseApplicationName(\"TestApplication2\");\r\n" +
                  "}\r\n" +
              "}"));
            ApplicationActivator.Startup();
            ApplicationActivator.Shutdown(Assembly.Load(new AssemblyName("test1")), Assembly.Load(new AssemblyName("test2")));
#if NetCore
            Assert.Equal("TestApplication1", ApplicationActivator.Environment.ApplicationName);
#else
            Assert.AreEqual("TestApplication1", ApplicationActivator.Environment.ApplicationName);
#endif
        }

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void MixedPrioritySpecification()
        {
            Assert.True(CompileHelper.CreateAssembly("test1.dll",
                "using MassActivation;\r\n" +
                "[ActivationPriority(ActivationPriority.Low)]" +
                "public class Startup{\r\n" +
                "[ActivationPriority(ActivationPriority.High)]" +
                    "public void Shutdown(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationVersion(new System.Version(\"1.0.1\"));\r\n" +
                    "}\r\n" +
                "}"));
            Assert.True(CompileHelper.CreateAssembly("test2.dll",
                "using MassActivation;\r\n" +
                "public class Startup{\r\n" +
                    "public void Shutdown(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationVersion(new System.Version(\"1.0.5\"));\r\n" +
                    "}\r\n" +
                "}"));
            ApplicationActivator.Startup();
            ApplicationActivator.Shutdown(Assembly.Load(new AssemblyName("test1")), Assembly.Load(new AssemblyName("test2")));
#if NetCore
            Assert.Equal(new Version("1.0.5"), ApplicationActivator.Environment.ApplicationVersion);
#else
            Assert.AreEqual(new Version("1.0.5"), ApplicationActivator.Environment.ApplicationVersion);
#endif
        }
    }
}
