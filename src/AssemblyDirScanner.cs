using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Wheatech.Activation
{
    internal class AssemblyDirScanner: IEnumerable<Assembly>
    {
        public IEnumerator<Assembly> GetEnumerator()
        {
            var info = AppDomain.CurrentDomain.SetupInformation;
            IEnumerable<string> searchPaths = new string[0];
            if (info.PrivateBinPathProbe == null || string.IsNullOrWhiteSpace(info.PrivateBinPath))
            {
                // Check the current directory
                searchPaths = searchPaths.Concat(new[] { string.Empty });
            }
            if (!string.IsNullOrWhiteSpace(info.PrivateBinPath))
            {
                // PrivateBinPath may be a semicolon separated list of subdirectories.
                searchPaths = searchPaths.Concat(info.PrivateBinPath.Split(';'));
            }
            foreach (var searchPath in searchPaths)
            {
                string assembliesPath = Path.Combine(info.ApplicationBase, searchPath);
                if (!Directory.Exists(assembliesPath)) continue;
                var files = Directory.GetFiles(assembliesPath, "*.dll").Concat(Directory.GetFiles(assembliesPath, "*.exe"));
                foreach (var file in files)
                {
                    Assembly assembly;

                    try
                    {
                        assembly = Assembly.Load(AssemblyName.GetAssemblyName(file));
                    }
                    catch (BadImageFormatException)
                    {
                        // Not a managed dll/exe
                        continue;
                    }

                    yield return assembly;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
