using System;
#if NetCore
using Xunit;
#else
using NUnit.Framework;
#endif

namespace MassActivation.UnitTests
{
    public class StartupInvocationFixture
    {
#if NetCore
        public StartupInvocationFixture()
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
        public void DefaultConvensionClassName()
        {
            Assert.True(CompileHelper.CreateAssembly("test.dll",
                "using MassActivation;\r\n"+
                "public class Startup{\r\n" +
                    "public void Configuration(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationName(\"TestApplication\");\r\n" +
                    "}\r\n" +
                "}"));
            ApplicationActivator.Startup();
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
        public void DefaultConvensionEnvironmentClassName()
        {
            Assert.True(CompileHelper.CreateAssembly("test.dll",
                "using MassActivation;\r\n" +
                "public class Startup{\r\n" +
                    "public void Configuration(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationName(\"DefaultApplication\");\r\n" +
                    "}\r\n" +
                "}",
                "using MassActivation;\r\n" +
                "public class StartupDevelopment{\r\n" +
                    "public void Configuration(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationName(\"DevelopmentApplication\");\r\n" +
                    "}\r\n" +
                "}"
                ));
            ApplicationActivator.UseEnvironment(EnvironmentName.Development).Startup();
#if NetCore
            Assert.Equal("DevelopmentApplication", ApplicationActivator.Environment.ApplicationName);
#else
            Assert.AreEqual("DevelopmentApplication", ApplicationActivator.Environment.ApplicationName);
#endif
        }

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void DefaultConvensionEnvironmentMethodName()
        {
            Assert.True(CompileHelper.CreateAssembly("test.dll",
                "using MassActivation;\r\n" +
                "public class Startup{\r\n" +
                    "public void Configuration(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationName(\"DefaultApplication\");\r\n" +
                    "}\r\n" +
                    "public void ConfigurationDevelopment(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationName(\"DevelopmentApplication\");\r\n" +
                    "}\r\n" +
                "}"
                ));
            ApplicationActivator.UseEnvironment(EnvironmentName.Development).Startup();
#if NetCore
            Assert.Equal("DevelopmentApplication", ApplicationActivator.Environment.ApplicationName);
#else
            Assert.AreEqual("DevelopmentApplication", ApplicationActivator.Environment.ApplicationName);
#endif
        }

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void AttributeSpecifiedDefaultStartupClass()
        {
            Assert.True(CompileHelper.CreateAssembly("test.dll",
                "using MassActivation;\r\n" +
                "[assembly: AssemblyActivator(typeof(Activator))]\r\n" +
                "public class Activator{\r\n" +
                    "public void Configuration(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationName(\"TestApplication\");\r\n" +
                    "}\r\n" +
                "}"));
            ApplicationActivator.Startup();
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
        public void AttributeSpecifiedEnvironmentStartupClass()
        {
            Assert.True(CompileHelper.CreateAssembly("test.dll",
                "using MassActivation;\r\n" +
                "[assembly: AssemblyActivator(typeof(Activator))]\r\n" +
                "public class Activator{\r\n" +
                    "public void Configuration(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationName(\"DefaultApplication\");\r\n" +
                    "}\r\n" +
                "}",
                "using MassActivation;\r\n" +
                "[assembly: AssemblyActivator(typeof(ActivatorDevelopment),\"Development\")]\r\n" +
                "public class ActivatorDevelopment{\r\n" +
                    "public void Configuration(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationName(\"DevelopmentApplication\");\r\n" +
                    "}\r\n" +
                "}"
                ));
            ApplicationActivator.UseEnvironment(EnvironmentName.Development).Startup();
#if NetCore
            Assert.Equal("DevelopmentApplication", ApplicationActivator.Environment.ApplicationName);
#else
            Assert.AreEqual("DevelopmentApplication", ApplicationActivator.Environment.ApplicationName);
#endif
        }

#if NetCore
        [Fact]
#else
        [Test]
#endif
        public void InvokeStaticConstructor()
        {
            Assert.True(CompileHelper.CreateAssembly("test.dll",
                "using MassActivation;\r\n" +
                "public class Startup{\r\n" +
                    "static Startup(){\r\n" +
                        "System.Environment.SetEnvironmentVariable(\"TestVariable\",\"MassActivation\");\r\n" +
                    "}\r\n" +
                    "public void Configuration(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationName(\"TestApplication\");\r\n" +
                    "}\r\n" +
                "}"));
            ApplicationActivator.Startup();
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
        public void InvokeInstanceConstructor()
        {
            Environment.SetEnvironmentVariable("TestVariable", "Initialize");
#if NetCore
            Assert.Equal("Initialize", Environment.GetEnvironmentVariable("TestVariable"));
#else
            Assert.AreEqual("Initialize", Environment.GetEnvironmentVariable("TestVariable"));
#endif
            Assert.True(CompileHelper.CreateAssembly("test.dll",
             "using MassActivation;\r\n" +
             "public class Startup{\r\n" +
                 "public Startup(){\r\n" +
                     "System.Environment.SetEnvironmentVariable(\"TestVariable\",\"MassActivation\");\r\n" +
                 "}\r\n" +
                 "public void Configuration(IActivatingEnvironment environment){\r\n" +
                     "environment.UseApplicationName(\"TestApplication\");\r\n" +
                 "}\r\n" +
             "}"));
            ApplicationActivator.Startup();
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
        public void InvokeInstanceConstructorWithParameters()
        {
            Assert.True(CompileHelper.CreateAssembly("test.dll",
             "using MassActivation;\r\n" +
             "public class Startup{\r\n" +
                 "public Startup(IActivatingEnvironment environment){\r\n" +
                     "environment.UseApplicationName(\"TestApplication\");\r\n" +
                 "}\r\n" +
             "}"));
            ApplicationActivator.Startup();
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
        public void InvokeStaticMethod()
        {
            Assert.True(CompileHelper.CreateAssembly("test.dll",
                "using MassActivation;\r\n" +
                "public class Startup{\r\n" +
                    "public static void Configuration(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationName(\"TestApplication\");\r\n" +
                    "}\r\n" +
                "}"));
            ApplicationActivator.Startup();
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
        public void InstanceConstructorDefaultInvokeSequence()
        {
            Assert.True(CompileHelper.CreateAssembly("test1.dll",
             "using MassActivation;\r\n" +
             "public class Startup{\r\n" +
                 "public Startup(IActivatingEnvironment environment, System.IServiceProvider provider){\r\n" +
                     "environment.UseApplicationName(\"TestApplication1\");\r\n" +
                 "}\r\n" +
             "}"));
            Assert.True(CompileHelper.CreateAssembly("test2.dll",
              "using MassActivation;\r\n" +
              "public class Startup{\r\n" +
                  "public Startup(IActivatingEnvironment environment){\r\n" +
                      "environment.UseApplicationName(\"TestApplication2\");\r\n" +
                  "}\r\n" +
              "}"));
            ApplicationActivator.Startup();
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
        public void InstanceMethodDefaultInvokeSequence()
        {
            Assert.True(CompileHelper.CreateAssembly("test1.dll",
             "using MassActivation;\r\n" +
             "public class Startup{\r\n" +
                 "public void Configuration(IActivatingEnvironment environment, System.IServiceProvider provider){\r\n" +
                     "environment.UseApplicationName(\"TestApplication1\");\r\n" +
                 "}\r\n" +
             "}"));
            Assert.True(CompileHelper.CreateAssembly("test2.dll",
              "using MassActivation;\r\n" +
              "public class Startup{\r\n" +
                  "public void Configuration(IActivatingEnvironment environment){\r\n" +
                      "environment.UseApplicationName(\"TestApplication2\");\r\n" +
                  "}\r\n" +
              "}"));
            ApplicationActivator.Startup();
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
        public void InstanceConstructorSpecifyPriority()
        {
            Assert.True(CompileHelper.CreateAssembly("test1.dll",
             "using MassActivation;\r\n" +
             "public class Startup{\r\n" +
                "[ActivationPriority(ActivationPriority.High)]" +
                 "public Startup(IActivatingEnvironment environment, System.IServiceProvider provider){\r\n" +
                     "environment.UseApplicationName(\"TestApplication1\");\r\n" +
                 "}\r\n" +
             "}"));
            Assert.True(CompileHelper.CreateAssembly("test2.dll",
              "using MassActivation;\r\n" +
              "public class Startup{\r\n" +
                  "public Startup(IActivatingEnvironment environment){\r\n" +
                      "environment.UseApplicationName(\"TestApplication2\");\r\n" +
                  "}\r\n" +
              "}"));
            ApplicationActivator.Startup();
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
        public void InstanceMethodSpecifyPriority()
        {
            Assert.True(CompileHelper.CreateAssembly("test1.dll",
             "using MassActivation;\r\n" +
             "public class Startup{\r\n" +
                "[ActivationPriority(ActivationPriority.High)]" +
                 "public void Configuration(IActivatingEnvironment environment, System.IServiceProvider provider){\r\n" +
                     "environment.UseApplicationName(\"TestApplication1\");\r\n" +
                 "}\r\n" +
             "}"));
            Assert.True(CompileHelper.CreateAssembly("test2.dll",
              "using MassActivation;\r\n" +
              "public class Startup{\r\n" +
                  "public void Configuration(IActivatingEnvironment environment){\r\n" +
                      "environment.UseApplicationName(\"TestApplication2\");\r\n" +
                  "}\r\n" +
              "}"));
            ApplicationActivator.Startup();
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
        public void StartupClassSpecifyPriority()
        {
            Assert.True(CompileHelper.CreateAssembly("test1.dll",
             "using MassActivation;\r\n" +
             "[ActivationPriority(ActivationPriority.High)]" +
             "public class Startup{\r\n" +
                 "public void Configuration(IActivatingEnvironment environment, System.IServiceProvider provider){\r\n" +
                     "environment.UseApplicationName(\"TestApplication1\");\r\n" +
                 "}\r\n" +
             "}"));
            Assert.True(CompileHelper.CreateAssembly("test2.dll",
              "using MassActivation;\r\n" +
              "public class Startup{\r\n" +
                  "public void Configuration(IActivatingEnvironment environment){\r\n" +
                      "environment.UseApplicationName(\"TestApplication2\");\r\n" +
                  "}\r\n" +
              "}"));
            ApplicationActivator.Startup();
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
        public void StaticConstructorSpecifyPriority()
        {
            Assert.True(CompileHelper.CreateAssembly("test1.dll",
                "using MassActivation;\r\n" +
                "public class Startup{\r\n" +
                    "static Startup(){\r\n" +
                        "System.Environment.SetEnvironmentVariable(\"TestVariable\",\"TestApplication1\");\r\n" +
                    "}\r\n" +
                    "public void Configuration(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationName(\"TestApplication\");\r\n" +
                    "}\r\n" +
                "}"));
            Assert.True(CompileHelper.CreateAssembly("test2.dll",
                "using MassActivation;\r\n" +
                "public class Startup{\r\n" +
                    "[ActivationPriority(ActivationPriority.High)]" +
                    "static Startup(){\r\n" +
                        "System.Environment.SetEnvironmentVariable(\"TestVariable\",\"TestApplication2\");\r\n" +
                    "}\r\n" +
                    "public void Configuration(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationName(\"TestApplication\");\r\n" +
                    "}\r\n" +
                "}"));
            ApplicationActivator.Startup();
#if NetCore
            Assert.Equal("TestApplication1", Environment.GetEnvironmentVariable("TestVariable"));
#else
            Assert.AreEqual("TestApplication1", Environment.GetEnvironmentVariable("TestVariable"));
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
                    "static Startup(){\r\n" +
                        "System.Environment.SetEnvironmentVariable(\"TestVariable\",\"TestApplication1\");\r\n" +
                    "}\r\n" +
                    "[ActivationPriority(ActivationPriority.High)]" +
                     "public Startup(IActivatingEnvironment environment, System.IServiceProvider provider){\r\n" +
                         "environment.UseApplicationName(\"TestApplication1\");\r\n" +
                     "}\r\n" +
                    "public void Configuration(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationVersion(new System.Version(\"1.0.1\"));\r\n" +
                    "}\r\n" +
                "}"));
            Assert.True(CompileHelper.CreateAssembly("test2.dll",
                "using MassActivation;\r\n" +
                "public class Startup{\r\n" +
                    "[ActivationPriority(ActivationPriority.High)]" +
                    "static Startup(){\r\n" +
                        "System.Environment.SetEnvironmentVariable(\"TestVariable\",\"TestApplication2\");\r\n" +
                    "}\r\n" +
                    "public Startup(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationName(\"TestApplication2\");\r\n" +
                    "}\r\n" +
                    "public void Configuration(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationVersion(new System.Version(\"1.0.5\"));\r\n" +
                    "}\r\n" +
                "}"));
            ApplicationActivator.Startup();
#if NetCore
            Assert.Equal("TestApplication1", Environment.GetEnvironmentVariable("TestVariable"));
            Assert.Equal("TestApplication2", ApplicationActivator.Environment.ApplicationName);
            Assert.Equal(new Version("1.0.1"), ApplicationActivator.Environment.ApplicationVersion);
#else
            Assert.AreEqual("TestApplication1", Environment.GetEnvironmentVariable("TestVariable"));
            Assert.AreEqual("TestApplication2", ApplicationActivator.Environment.ApplicationName);
            Assert.AreEqual(new Version("1.0.1"), ApplicationActivator.Environment.ApplicationVersion);
#endif
        }
    }
}
