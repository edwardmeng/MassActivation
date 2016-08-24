using System;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.CSharp;
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

        [Fact]
        public void SpecifyApplicationName()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            environment.UseApplicationName("MassActivation");
            Assert.Equal("MassActivation", environment.ApplicationName);
        }

        [Fact]
        public void DefaultApplicationVersion()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            Assert.Equal(typeof(ActivatingEnvironmentFixture).Assembly.GetName().Version, environment.ApplicationVersion);
        }

        [Fact]
        public void SpecifyApplicationVersion()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            environment.UseApplicationVersion(new Version("1.0.5"));
            Assert.Equal(new Version("1.0.5"), environment.ApplicationVersion);
        }

        [Fact]
        public void DefaultEnvironment()
        {
            Environment.SetEnvironmentVariable("ACTIVATION_ENVIRONMENT",null);
            IActivatingEnvironment environment = new ActivatingEnvironment();
            Assert.Equal(EnvironmentName.Production, environment.Environment);
        }

        [Fact]
        public void DefaultEnvironmentFromSystemVariable()
        {
            Environment.SetEnvironmentVariable("ACTIVATION_ENVIRONMENT", EnvironmentName.Development);
            IActivatingEnvironment environment = new ActivatingEnvironment();
            Assert.Equal(EnvironmentName.Development, environment.Environment);
        }

        [Fact]
        public void SpecifyEnvironment()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            environment.UseEnvironment(EnvironmentName.Staging);
            Assert.Equal(EnvironmentName.Staging, environment.Environment);
        }

        [Fact]
        public void DefaultServiceRegistration()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            Assert.Equal(environment, environment.Get<IActivatingEnvironment>());
            Assert.Equal(environment, environment.Get<IServiceProvider>());
            Assert.Null(environment.Get<ICustomFormatter>());
        }

        [Fact]
        public void CustomServiceRegistration()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            environment.Use<IFormatProvider>(CultureInfo.CurrentCulture);
            Assert.Equal(CultureInfo.CurrentCulture, environment.Get<IFormatProvider>());
        }

        [Fact]
        public void RemoveServiceRegistration()
        {
            IActivatingEnvironment environment = new ActivatingEnvironment();
            Assert.Equal(environment, environment.Get<IServiceProvider>());
            environment.Remove<IServiceProvider>();
            Assert.Null(environment.Get<IServiceProvider>());
        }

        [Fact]
        public void RuntimeDynamicAssembly()
        {
            var name = new AssemblyName("DynamicAssembly");
            AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            IActivatingEnvironment environment = new ActivatingEnvironment();
            var assembly = environment.GetAssemblies().SingleOrDefault(x => x.GetName().Name == "DynamicAssembly");
            Assert.NotNull(assembly);
            Assert.True(assembly.IsDynamic);
        }

        [Fact]
        public void NotReferenceStaticAssembly()
        {
            Assert.True(CreateAssembly(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.dll")));
            IActivatingEnvironment environment = new ActivatingEnvironment();
            var assembly = environment.GetAssemblies().SingleOrDefault(x => x.GetName().Name == "test");
            Assert.NotNull(assembly);
        }

        private bool CreateAssembly(string path)
        {
            var result = new CSharpCodeProvider().CompileAssemblyFromSource(new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = false,
                IncludeDebugInformation = false,
                OutputAssembly = path
            }, string.Format(
                "using System.Reflection;\r\n" +
                "[assembly: AssemblyVersion(\"1.0.5\")]\r\n" +
                "[assembly: AssemblyFileVersion(\"1.0.5\")]\r\n" +
                "[assembly: AssemblyProduct(\"MassActivation\")]"));
            if (result.Errors.HasErrors)
            {
                foreach (CompilerError err in result.Errors)
                {
                    Console.Error.WriteLine(err.ErrorText);
                }
                return false;
            }
            return true;
        }

        [Fact]
        public void MethodReflect()
        {
            var method = typeof (ActivatingEnvironmentFixture).GetMethod("ReferenceMethod");
        }

        public void ReferenceMethod(ref int value)
        {
            
        }
    }
}
