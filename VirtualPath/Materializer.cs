using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;

namespace VirtualPath
{
    public abstract class Materializer
    {
        protected static readonly Assembly ThisAssembly = typeof(Materializer).Assembly;
        private static Materializer _composer = new MefHelper();

        public static Materializer Default
        {
            get { return _composer; }
        }

        internal static void SetDefault(Materializer composer)
        {
            _composer = composer;
        }

        internal abstract Type FindType<T>(string contractName);

        public T Materialize<T>(string contractName, IDictionary<string, object> config)
        {
            var type = FindType<T>(contractName);
            var constructors = type.GetConstructors();
            var constructor = FindMatchingConstructor(constructors, config);
            var arguments = constructor.GetParameters()
                .Select(p => new
                    {
                        Type = p.ParameterType,
                        Value = config.ContainsKey(p.Name)
                            ? config[p.Name]
                            : config[""]
                    })
                .Select(p => Convert.ChangeType(p.Value, p.Type, CultureInfo.InvariantCulture))
                .ToArray();
            var instance = constructor.Invoke(arguments);
            return (T)instance;
        }

        /// <summary>
        /// Find constructor matching the configuration
        /// Priority:
        /// 1. Parameters count (must be exact)
        /// 2. Parameters names (not case sensitive)
        /// 3. Parameter types (primitives types can be converted from string)
        /// </summary>
        /// <param name="constructors"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private ConstructorInfo FindMatchingConstructor(ConstructorInfo[] constructors, IDictionary<string, object> config)
        {
            var parametersCount = config.Count();
            constructors = constructors
                .Where(c => c.GetParameters().Count() == parametersCount)
                .ToArray();

            if (constructors.Count() == 1)
            {
                return constructors.Single();
            }
            if (!constructors.Any())
            {
                throw new VirtualPathException(
                    "Can't find a constructor matching parameters count of configuration.");
            }

            constructors = constructors
                .Where(c => 
                    c.GetParameters()
                        .All(parameter => 
                            config.ContainsKey(parameter.Name) || config.ContainsKey("")))
                .ToArray();

            if (constructors.Count() == 1)
            {
                return constructors.Single();
            }
            if (!constructors.Any())
            {
                throw new VirtualPathException(
                    "Can't find a constructor matching parameters names of configuration.");
            }

            constructors = constructors
                .Where(c => MatchTypes(c.GetParameters(), config))
                .ToArray();

            if (constructors.Count() == 1)
            {
                return constructors.Single();
            }
            else if (!constructors.Any())
            {
                throw new VirtualPathException(
                    "Can't find a constructor matching parameters names of configuration.");
            }
            else
            {
                throw new VirtualPathException(
                    "Found several constructors matching parameters count, names and types of configuration.");
            }
        }

        private bool MatchTypes(ParameterInfo[] parameterInfos, IDictionary<string, object> config)
        {
            foreach (var parameterInfo in parameterInfos)
            {
                var parameterType = parameterInfo.ParameterType;
                parameterType = Nullable.GetUnderlyingType(parameterType) ?? parameterType;

                var configElement = config.ContainsKey(parameterInfo.Name) 
                    ? config[parameterInfo.Name]
                    : config[""];

                if (configElement == null)
                {
                    return false;
                }

                var configElType = configElement.GetType();
                if (parameterType == configElType)
                {
                    return true;
                }
                if (configElType == typeof(String) 
                    && IsConvertible(configElement, parameterType))
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        private bool IsConvertible(object configElement, Type parameterType)
        {
            try
            {
                Convert.ChangeType(configElement, parameterType, CultureInfo.InvariantCulture);
                return true;
            }
            catch(InvalidCastException)
            {
                return false;
            }
        }

        public static string GetVirtualPathAssemblyPath()
        {
            var path = ThisAssembly.CodeBase.Replace("file:///", "").Replace("file://", "//");
            path = Path.GetDirectoryName(path);
            if (path == null) throw new ArgumentException("Unrecognised assemblyFile.");
            if (!Path.IsPathRooted(path))
            {
                path = Path.DirectorySeparatorChar + path;
            }
            return path;
        }

        public static bool TryLoadAssembly(string assemblyFile, out Assembly assembly)
        {
            if (assemblyFile == null) throw new ArgumentNullException("assemblyFile");
            if (assemblyFile.Length == 0) throw new ArgumentException("Assembly file name is empty.", "assemblyFile");
            try
            {
                assembly = Assembly.ReflectionOnlyLoadFrom(assemblyFile);
                return true;
            }
            catch (FileNotFoundException)
            {
                assembly = null;
                return false;
            }
            catch (FileLoadException)
            {
                assembly = null;
                return false;
            }
            catch (BadImageFormatException)
            {
                assembly = null;
                return false;
            }
            catch (SecurityException)
            {
                assembly = null;
                return false;
            }
            catch (PathTooLongException)
            {
                assembly = null;
                return false;
            }
        }
    }
}