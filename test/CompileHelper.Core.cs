using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.PlatformAbstractions;

namespace MassActivation.UnitTests
{
    public static class CompileHelper
    {
        public static bool CreateAssembly(string fileName, params string[] sourceCodes)
        {
            var result = new Microsoft.CSharp.CSharpCodeProvider().CompileAssemblyFromSource(new System.CodeDom.Compiler.CompilerParameters(new[] { "MassActivation.dll" })
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
                foreach (System.CodeDom.Compiler.CompilerError err in result.Errors)
                {
                    Console.Error.WriteLine(err.ErrorText);
                }
                return false;
            }
            return true;
        }

        public static void ClearAssemblies()
        {
            var baseDir = new DirectoryInfo(PlatformServices.Default.Application.ApplicationBasePath);
            foreach (var file in baseDir.GetFiles("test*.dll"))
            {
                file.Delete();
            }
            baseDir = baseDir.Parent;
            if (baseDir != null)
            {
                foreach (var file in baseDir.GetFiles("*.dll"))
                {
                    file.Delete();
                }
            }
        }
    }
}
