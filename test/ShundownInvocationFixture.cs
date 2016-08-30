using System;
using System.Reflection;
using NUnit.Framework;

namespace MassActivation.UnitTests
{
    public class ShundownInvocationFixture
    {
        [SetUp]
        public void ClearAssemblies()
        {
            CompileHelper.ClearAssemblies();
        }

        [Test]
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
            Assert.AreNotEqual("TestApplication", ApplicationActivator.Environment.ApplicationName);
            ApplicationActivator.Shutdown(Assembly.Load(new AssemblyName("test")));
            Assert.AreEqual("TestApplication", ApplicationActivator.Environment.ApplicationName);
        }

        [Test]
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
            Assert.AreNotEqual("TestApplication", ApplicationActivator.Environment.ApplicationName);
            ApplicationActivator.Shutdown(Assembly.Load(new AssemblyName("test")));
            Assert.AreEqual("TestApplication", ApplicationActivator.Environment.ApplicationName);
        }

        [Test]
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
            Assert.AreNotEqual("MassActivation", Environment.GetEnvironmentVariable("TestVariable"));
            ApplicationActivator.Shutdown(Assembly.Load(new AssemblyName("test")));
            Assert.AreEqual("MassActivation", Environment.GetEnvironmentVariable("TestVariable"));
        }

        [Test]
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
            Assert.AreEqual("TestApplication2", ApplicationActivator.Environment.ApplicationName);
        }

        [Test]
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
            Assert.AreEqual("TestApplication1", ApplicationActivator.Environment.ApplicationName);
        }

        [Test]
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
            Assert.AreEqual("TestApplication1", ApplicationActivator.Environment.ApplicationName);
        }

        [Test]
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
            Assert.AreEqual(new Version("1.0.5"), ApplicationActivator.Environment.ApplicationVersion);
        }
    }
}
