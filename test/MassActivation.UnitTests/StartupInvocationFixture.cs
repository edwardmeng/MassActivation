using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using Xunit;

namespace MassActivation.Tests
{
    public class StartupInvocationFixture
    {
        private bool CreateAssembly(string fileName, params string[] sourceCodes)
        {
            var result = new CSharpCodeProvider().CompileAssemblyFromSource(new CompilerParameters(new[] {"MassActivation.dll"})
            {
                GenerateExecutable = false,
                GenerateInMemory = false,
                IncludeDebugInformation = false,
                OutputAssembly = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName)
            }, sourceCodes.Concat(new[]
            {
                string.Format(
                    "using System.Reflection;\r\n" +
                    "[assembly: AssemblyVersion(\"1.0.5\")]\r\n" +
                    "[assembly: AssemblyFileVersion(\"1.0.5\")]\r\n" +
                    "[assembly: AssemblyProduct(\"MassActivation\")]")
            }).ToArray());
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
        public void DefaultConvensionClassName()
        {
            Assert.True(CreateAssembly("test.dll",
                "using MassActivation;\r\n"+
                "public class Startup{\r\n" +
                    "public void Configuration(IActivatingEnvironment environment){\r\n" +
                        "environment.UseApplicationName(\"TestApplication\");\r\n" +
                    "}\r\n" +
                "}"));
            ApplicationActivator.Startup();
            Assert.Equal("TestApplication", ApplicationActivator.Environment.ApplicationName);
        }

        [Fact]
        public void DefaultConvensionEnvironmentClassName()
        {
            Assert.True(CreateAssembly("test.dll",
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
            Assert.Equal("DevelopmentApplication", ApplicationActivator.Environment.ApplicationName);
        }
    }
}
