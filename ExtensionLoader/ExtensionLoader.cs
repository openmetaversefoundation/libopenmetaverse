using System;
using System.Collections.Generic;
using System.Reflection;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;

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
        public static List<IExtension<TOwner>> Extensions;
        
        /// <summary></summary>
        static CodeDomProvider CSCompiler;
        /// <summary></summary>
        static CompilerParameters CSCompilerParams;

        static ExtensionLoader()
        {
            Extensions = new List<IExtension<TOwner>>();

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
        /// Load extensions within the given assembly, from assembly files in
        /// a given directory, or from source code files in a given directory
        /// </summary>
        /// <param name="assembly">Main assembly to load extensions from</param>
        /// <param name="path">Directory to load assembly and source code
        /// extensions from</param>
        /// <param name="owner">Object that owns the extensions. A reference to
        /// this is passed to the constructor of each extension</param>
        /// <param name="extensionList">An optional whitelist of extensions to
        /// load</param>
        /// <param name="referencedAssemblies">List of assemblies the
        /// extensions need references to</param>
        /// <param name="assemblySearchPattern">Search pattern for extension
        /// dlls, for example MyApp.Extension.*.dll</param>
        /// <param name="sourceSearchPattern">Search pattern for extension
        /// source code files, for example MyApp.Extension.*.cs</param>
        /// <param name="assignablesParent">The object containing the 
        /// assignable interfaces</param>
        /// <param name="assignableInterfaces">A list of interface references
        /// to assign extensions to</param>
        public static void LoadAllExtensions(Assembly assembly, string path, TOwner owner,
            List<string> extensionList, List<string> referencedAssemblies,
            string assemblySearchPattern, string sourceSearchPattern,
            object assignablesParent, List<FieldInfo> assignableInterfaces)
        {
            // Add referenced assemblies to the C# compiler
            CSCompilerParams.ReferencedAssemblies.Clear();
            if (referencedAssemblies != null)
            {
                for (int i = 0; i < referencedAssemblies.Count; i++)
                    CSCompilerParams.ReferencedAssemblies.Add(referencedAssemblies[i]);
            }

            // Load internal extensions
            LoadAssemblyExtensions(assembly, extensionList);

            // Load extensions from external assemblies
            List<string> extensionNames = ListExtensionAssemblies(path, assemblySearchPattern);
            foreach (string name in extensionNames)
                LoadAssemblyExtensions(Assembly.LoadFile(name), extensionList);

            // Load extensions from external code files
            extensionNames = ListExtensionSourceFiles(path, sourceSearchPattern);
            foreach (string name in extensionNames)
            {
                CompilerResults results = CSCompiler.CompileAssemblyFromFile(CSCompilerParams, name);
                if (results.Errors.Count == 0)
                {
                    LoadAssemblyExtensions(results.CompiledAssembly, extensionList);
                }
                else
                {
                    StringBuilder errors = new StringBuilder();
                    errors.AppendLine("Error(s) compiling " + name);
                    foreach (CompilerError error in results.Errors)
                        errors.AppendFormat(" Line {0}: {1}{2}", error.Line, error.ErrorText, Environment.NewLine);
                    throw new ExtensionException(errors.ToString());
                }
            }

            if (assignableInterfaces != null)
            {
                // Assign extensions to interfaces
                foreach (FieldInfo assignable in assignableInterfaces)
                {
                    Type type = assignable.FieldType;

                    for (int i = 0; i < Extensions.Count; i++)
                    {
                        IExtension<TOwner> extension = Extensions[i];

                        if (extension.GetType().GetInterface(type.Name) != null)
                            assignable.SetValue(assignablesParent, extension);
                    }
                }

                // Check for unassigned interfaces
                foreach (FieldInfo assignable in assignableInterfaces)
                {
                    if (assignable.GetValue(assignablesParent) == null)
                        throw new ExtensionException("Unassigned interface " + assignable.FieldType.Name);
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
                        if (type.GetInterface(typeof(IExtension<TOwner>).Name) != null)
                        {
                            plugins.Add(f);
                            break;
                        }
                    }
                }
                catch (Exception) { }
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

        public static void LoadAssemblyExtensions(Assembly assembly, List<string> whitelist)
        {
            Type[] constructorParams = new Type[] { };
            object[] parameters = new object[] { };

            foreach (Type t in assembly.GetTypes())
            {
                try
                {
                    if (t.GetInterface(typeof(IExtension<TOwner>).Name) != null && 
                        (whitelist == null || whitelist.Contains(t.Name)))
                    {
                        ConstructorInfo info = t.GetConstructor(constructorParams);
                        IExtension<TOwner> extension = (IExtension<TOwner>)info.Invoke(parameters);
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

        public static FieldInfo GetInterface(Type ownerType, string memberName)
        {
            FieldInfo fieldInfo = ownerType.GetField(memberName);
            if (fieldInfo.FieldType.IsInterface)
                return fieldInfo;
            else
                return null;
        }

        public static List<FieldInfo> GetInterfaces(object ownerObject)
        {
            List<FieldInfo> interfaces = new List<FieldInfo>();

            foreach (FieldInfo field in ownerObject.GetType().GetFields())
            {
                if (field.FieldType.IsInterface)
                    interfaces.Add(field);
            }

            return interfaces;
        }
    }
}
