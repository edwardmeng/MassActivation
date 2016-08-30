using System;
using System.IO;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.PlatformAbstractions;

namespace MassActivation.UnitTests
{
    public static class CompileHelper
    {
        public static bool CreateAssembly(string fileName, params string[] sourceCodes)
        {
            var compilation = CSharpCompilation.Create(Path.GetFileNameWithoutExtension(fileName),
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release),
                    references: new MetadataReference[]
                    {
                        MetadataReference.CreateFromFile(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "MassActivation.dll"))
                    })
                .AddSyntaxTrees(sourceCodes.Concat(new[]
                {
                    "using System.Reflection;\r\n" +
                    "[assembly: AssemblyVersion(\"1.0.5\")]\r\n" +
                    "[assembly: AssemblyFileVersion(\"1.0.5\")]\r\n" +
                    "[assembly: AssemblyProduct(\"MassActivation\")]"
                }).Select(source => CSharpSyntaxTree.ParseText(SourceText.From(source, Encoding.UTF8))));
            using (var assemblyStream = new MemoryStream())
            {
                var result = compilation.Emit(assemblyStream);
                if (!result.Success)
                {
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        if (IsError(diagnostic))
                        {
                            Console.WriteLine(diagnostic.GetMessage());
                        }
                    }
                    return false;
                }
                assemblyStream.Seek(0, SeekOrigin.Begin);
                using (var fileStream = File.OpenWrite(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, fileName)))
                {
                    assemblyStream.CopyTo(fileStream);
                }
                //System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
                return true;
            }
        }

        private static bool IsError(Diagnostic diagnostic)
        {
            return diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error;
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
