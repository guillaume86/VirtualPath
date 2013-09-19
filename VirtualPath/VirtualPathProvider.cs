using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualPath
{
    public class VirtualPathProvider
    {
        public static IVirtualPathProvider Open(string providerName)
        {
            return Open(providerName, new Dictionary<string,object>());
        }
        public static IVirtualPathProvider Open(string providerName, string configurationString)
        {
            var config = configurationString.Split(';')
                .Select(p => p.Split('='))
                .ToDictionary(
                    p => p.Length == 2 ? p[0] : "", 
                    p => (object)(p.Length == 2 ? p[1] : p[0]));

            return Open(providerName, config);
        }

        public static IVirtualPathProvider Open(string providerName, object configurationObject)
        {
            var config = configurationObject.GetType()
                .GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(configurationObject, null));
            return Open(providerName, config);
        }

        public static IVirtualPathProvider Open(string providerName, IDictionary<string,object> configuration)
        {
            var config = new Dictionary<string, object>(configuration, StringComparer.OrdinalIgnoreCase);
            return Materializer.Default.Materialize<IVirtualPathProvider>(providerName, config);
        }
    }
}
