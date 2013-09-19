using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace VirtualPath
{
    class MefHelper : Materializer
    {
        private static CompositionContainer CreateFolderContainer()
        {
            var path = GetVirtualPathAssemblyPath();

            var assemblyCatalog = new AssemblyCatalog(ThisAssembly);
            var aggregateCatalog = new AggregateCatalog(assemblyCatalog);
            foreach (string file in System.IO.Directory.GetFiles(path, "VirtualPath.*.dll"))
            {
                var catalog = new AssemblyCatalog(file);
                aggregateCatalog.Catalogs.Add(catalog);
            }
            return new CompositionContainer(aggregateCatalog);
        }

        private static CompositionContainer CreateAppDomainContainer()
        {
            var aggregateCatalog = new AggregateCatalog();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(IsVirtualPathAssembly))
            {
                aggregateCatalog.Catalogs.Add(new AssemblyCatalog(assembly));
            }
            return new CompositionContainer(aggregateCatalog);
        }

        private static bool IsVirtualPathAssembly(Assembly assembly)
        {
            return assembly.FullName.StartsWith("VirtualPath.", StringComparison.OrdinalIgnoreCase);
        }

        private static Type ComposablePartExportType<T>(ComposablePartDefinition part, string contractName)
        {
            if (part.ExportDefinitions.Any(def => 
                    def.Metadata.ContainsKey("ExportTypeIdentity") 
                    && String.Equals(def.ContractName, contractName, StringComparison.OrdinalIgnoreCase)
                    && def.Metadata["ExportTypeIdentity"].Equals(typeof(T).FullName)))
            {
                return ReflectionModelServices.GetPartType(part).Value;
            }
            return null;
        }

        internal override Type FindType<T>(string contractName)
        {
            using (var container = CreateAppDomainContainer())
            {
                var exportedTypes = container.Catalog
                    .Select(part => ComposablePartExportType<T>(part, contractName))
                    .Where(t => t != null)
                    .ToArray();

                if (exportedTypes.Count() == 1)
                {
                    return exportedTypes.Single();
                }
            }
            using (var container = CreateFolderContainer())
            {
                var exportedTypes = container.Catalog
                    .Select(part => ComposablePartExportType<T>(part, contractName))
                    .Where(t => t != null)
                    .ToArray();

                if (exportedTypes.Count() == 0) throw new VirtualPathException(string.Format("No {0} Provider found.", contractName));
                if (exportedTypes.Count() > 1) throw new VirtualPathException("Multiple Providers found; specify provider name or remove unwanted assemblies.");
                return exportedTypes.Single();
            }
        }
    }
}