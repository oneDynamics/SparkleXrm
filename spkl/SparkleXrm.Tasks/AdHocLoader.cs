using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NuGet.Packaging;

namespace SparkleXrm.Tasks
{
    internal class AdHocLoader : MarshalByRefObject
    {
        public IEnumerable<Type> GetTypes(string dir, string file, PluginAssembly plugin)
        {
            Directory.CreateDirectory(dir);
            ZipFile.ExtractToDirectory(file, dir);

            using (var stream = new FileStream(file, FileMode.Open))
            {
                var archive = new ZipArchive(stream);
                var fileName = archive.GetFiles().FirstOrDefault(f => f.Contains($"{plugin.Name}.dll"));

                var peekAssembly = Assembly.LoadFrom($"{dir}\\{fileName}");
                return Reflection.GetTypesImplementingInterface(peekAssembly, typeof(Microsoft.Xrm.Sdk.IPlugin));

            }
        }
    }
}
