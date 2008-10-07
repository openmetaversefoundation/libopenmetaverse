using System;
using System.Collections.Generic;
using System.Reflection;
using System.CodeDom.Compiler;
using System.IO;

namespace ExtensionLoader
{
    /// <summary>
    /// Exception thrown when there is a problem with an extension
    /// </summary>
    public class ExtensionException : Exception
    {
        public ExtensionException(string message)
            : base(message)
        {
        }

        public ExtensionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public static class ExtensionLoader<TOwner>
    {
        /// <summary>Currently loaded extensions</summary>
        public static List<IExtension> Extensions;
        /// <summary></summary>
        public static CodeDomProvider CSCompiler;
        /// <summary></summary>
        public static CompilerParameters CSCompilerParams;

        static ExtensionLoader()
        {
            Extensions = new List<IExtension>();

            CSCompiler = CodeDomProvider.CreateProvider("C#");

            CSCompilerParams = new CompilerParameters();
            CSCompilerParams.GenerateExecutable = false;
            CSCompilerParams.GenerateInMemory = true;
            if (System.Diagnostics.Debugger.IsAttached)
                CSCompilerParams.IncludeDebugInformation = true;
            else
                CSCompilerParams.IncludeDebugInformation = false;
        }

        /// <summary>
        /// Load extensions within the current assembly, from assembly files in
        /// a given directory, or from source code files in a given directory
        /// </summary>
        /// <param name="assembly">Main assembly to load extensions from</param>
        /// <param name="path">Directory to load assembly and source code
        /// extensions from</param>
        /// <param name="owner">Object that owns the extensions. A reference to
        /// this is passed to the constructor of each extension</param>
        /// <param name="referencedAssemblies">List of assemblies the
        /// extensions need references to</param>
        /// <param name="assemblySearchPattern">Search pattern for extension
        /// dlls, for example MyApp.Extension.*.dll</param>
        /// <param name="sourceSearchPattern">Search pattern for extension
        /// source code files, for example MyApp.Extension.*.cs</param>
        /// <param name="assignablesParent">The object containing the 
        /// assignable interfaces</param>
        /// <param name="assignableInterfaces">A list of interface types and
        /// interface references to assign extensions to</param>
        public static void LoadAllExtensions(Assembly assembly, string path, TOwner owner,
            List<string> referencedAssemblies, string assemblySearchPattern, string sourceSearchPattern,
            object assignablesParent, Dictionary<Type, FieldInfo> assignableInterfaces)
        {
            // Add referenced assemblies to the C# compiler
            CSCompilerParams.ReferencedAssemblies.Clear();
            if (referencedAssemblies != null)
            {
                for (int i = 0; i < referencedAssemblies.Count; i++)
                    CSCompilerParams.ReferencedAssemblies.Add(referencedAssemblies[i]);
            }

            // Load internal extensions
            LoadAssemblyExtensions(assembly, owner);

            // Load extensions from external assemblies
            List<string> extensionNames = ListExtensionAssemblies(path, assemblySearchPattern);
            foreach (string name in extensionNames)
                LoadAssemblyExtensions(Assembly.LoadFile(name), owner);

            // Load extensions from external code files
            extensionNames = ListExtensionSourceFiles(path, sourceSearchPattern);
            foreach (string name in extensionNames)
            {
                CompilerResults results = CSCompiler.CompileAssemblyFromFile(CSCompilerParams, name);
                if (results.Errors.Count == 0)
                    LoadAssemblyExtensions(results.CompiledAssembly, owner);
                else
                    throw new ExtensionException("Error(s) compiling " + name);
            }

            if (assignableInterfaces != null)
            {
                // Assign extensions to interfaces
                foreach (KeyValuePair<Type, FieldInfo> kvp in assignableInterfaces)
                {
                    Type type = kvp.Key;
                    FieldInfo assignable = kvp.Value;

                    for (int i = 0; i < Extensions.Count; i++)
                    {
                        IExtension extension = Extensions[i];

                        if (extension.GetType().GetInterface(type.Name) != null)
                            assignable.SetValue(assignablesParent, extension);
                    }
                }

                // Check for unassigned interfaces
                foreach (KeyValuePair<Type, FieldInfo> kvp in assignableInterfaces)
                {
                    if (kvp.Value.GetValue(assignablesParent) == null)
                        throw new ExtensionException("Unassigned interface " + kvp.Key.Name);
                }
            }
        }

        public static List<string> ListExtensionAssemblies(string path, string searchPattern)
        {
            List<string> plugins = new List<string>();
            string[] files = Directory.GetFiles(path, searchPattern);

            foreach (string f in files)
            {
                try
                {
                    Assembly a = Assembly.LoadFrom(f);
                    System.Type[] types = a.GetTypes();
                    foreach (System.Type type in types)
                    {
                        if (type.GetInterface("IExtension") != null)
                        {
                            plugins.Add(f);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new ExtensionException("Unrecognized extension " + f, e);
                }
            }

            return plugins;
        }

        public static List<string> ListExtensionSourceFiles(string path, string searchPattern)
        {
            List<string> plugins = new List<string>();
            string[] files = Directory.GetFiles(path, searchPattern);

            foreach (string f in files)
            {
                if (File.ReadAllText(f).Contains("IExtension"))
                    plugins.Add(f);
            }

            return plugins;
        }

        public static void LoadAssemblyExtensions(Assembly assembly, TOwner owner)
        {
            Type[] constructorParams = new Type[] { typeof(TOwner) };

            foreach (Type t in assembly.GetTypes())
            {
                try
                {
                    if (t.GetInterface("IExtension") != null)
                    {
                        ConstructorInfo info = t.GetConstructor(constructorParams);
                        IExtension extension = (IExtension)info.Invoke(new object[] { owner });
                        Extensions.Add(extension);
                    }
                }
                catch (Exception e)
                {
                    throw new ExtensionException(String.Format(
                        "Failed to load IExtension {0} from assembly {1}", t.FullName, assembly.FullName), e);
                }
            }
        }
    }
}
