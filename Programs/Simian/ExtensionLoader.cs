using System;
using System.Collections.Generic;
using System.Reflection;
using System.CodeDom.Compiler;
using System.IO;
using OpenMetaverse;

namespace Simian
{
    public static class ExtensionLoader
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

        /// <summary>Currently loaded extensions</summary>
        public static List<ISimianExtension> Extensions;
        /// <summary></summary>
        public static CodeDomProvider CSCompiler;
        /// <summary></summary>
        public static CompilerParameters CSCompilerParams;

        static ExtensionLoader()
        {
            Extensions = new List<ISimianExtension>();

            CSCompiler = CodeDomProvider.CreateProvider("C#");

            CSCompilerParams = new CompilerParameters();
            CSCompilerParams.GenerateExecutable = false;
            CSCompilerParams.GenerateInMemory = true;
            if (System.Diagnostics.Debugger.IsAttached)
                CSCompilerParams.IncludeDebugInformation = true;
            else
                CSCompilerParams.IncludeDebugInformation = false;
            CSCompilerParams.ReferencedAssemblies.Add("OpenMetaverseTypes.dll");
            CSCompilerParams.ReferencedAssemblies.Add("OpenMetaverse.dll");
            CSCompilerParams.ReferencedAssemblies.Add("Simian.exe");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="owner"><seealso cref="Simian"/> server that these extensions belong to</param>
        public static void LoadAllExtensions(string path, Simian owner)
        {
            // Load internal extensions
            LoadAssemblyExtensions(Assembly.GetExecutingAssembly(), owner);

            // Load extensions from external assemblies
            List<string> extensionNames = ListExtensionAssemblies(path);
            foreach (string name in extensionNames)
                LoadAssemblyExtensions(Assembly.LoadFile(name), owner);

            // Load extensions from external code files
            extensionNames = ListExtensionSourceFiles(path);
            foreach (string name in extensionNames)
            {
                CompilerResults results = CSCompiler.CompileAssemblyFromFile(CSCompilerParams, name);
                if (results.Errors.Count == 0)
                {
                    LoadAssemblyExtensions(results.CompiledAssembly, owner);
                }
                else
                {
                    Logger.Log("Error(s) compiling " + name, Helpers.LogLevel.Error);
                    foreach (CompilerError error in results.Errors)
                        Logger.Log(error.ToString(), Helpers.LogLevel.Error);
                }
            }
        }

        public static List<string> ListExtensionAssemblies(string path)
        {
            List<string> plugins = new List<string>();
            string[] files = Directory.GetFiles(path, "Simian.*.dll");

            foreach (string f in files)
            {
                try
                {
                    Assembly a = Assembly.LoadFrom(f);
                    System.Type[] types = a.GetTypes();
                    foreach (System.Type type in types)
                    {
                        if (type.GetInterface("ISimianExtension") != null)
                        {
                            plugins.Add(f);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Log(String.Format("Unrecognized extension {0}: {1}", f, e.Message),
                        Helpers.LogLevel.Warning, e);
                }
            }

            return plugins;
        }

        public static List<string> ListExtensionSourceFiles(string path)
        {
            List<string> plugins = new List<string>();
            string[] files = Directory.GetFiles(path, "Simian.*.cs");

            foreach (string f in files)
            {
                if (File.ReadAllText(f).Contains("ISimianExtension"))
                    plugins.Add(f);
            }

            return plugins;
        }

        public static void LoadAssemblyExtensions(Assembly assembly, Simian owner)
        {
            foreach (Type t in assembly.GetTypes())
            {
                try
                {
                    if (t.GetInterface("ISimianExtension") != null)
                    {
                        ConstructorInfo info = t.GetConstructor(new Type[] { typeof(Simian) });
                        ISimianExtension extension = (ISimianExtension)info.Invoke(new object[] { owner });
                        Extensions.Add(extension);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log("LoadAssemblyExtensions(): " + e.Message, Helpers.LogLevel.Warning);
                }
            }
        }
    }
}
