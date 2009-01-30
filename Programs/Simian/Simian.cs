using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;
using System.Threading;
using System.Reflection;
using ExtensionLoader;
using ExtensionLoader.Config;
using HttpServer;
using OpenMetaverse;
using OpenMetaverse.Http;

namespace Simian
{
    public partial class Simian
    {
        public const string CONFIG_FILE = "Simian.ini";

        // TODO: Don't hard-code these
        public const uint REGION_X = 256000;
        public const uint REGION_Y = 256000;

        public int UDPPort = 9000;
        public int HttpPort = 8002;
        public string DataDir = "SimianData/";

        public WebServer HttpServer;
        public IniConfigSource ConfigFile;
        public ulong RegionHandle;

        // Interfaces
        public IAuthenticationProvider Authentication;
        public IAccountProvider Accounts;
        public IUDPProvider UDP;
        public ISceneProvider Scene;
        public IAssetProvider Assets;
        public IAvatarProvider Avatars;
        public IInventoryProvider Inventory;
        public IParcelProvider Parcels;
        public IMeshingProvider Mesher;
        //public ICapabilitiesProvider Capabilities;

        // Persistent extensions
        public List<IPersistable> PersistentExtensions = new List<IPersistable>();

        public Simian()
        {
        }

        public bool Start()
        {
            List<string> extensionList = null;

            try
            {
                // Load the extension list (and ordering) from our config file
                ConfigFile = new IniConfigSource(CONFIG_FILE);
                IConfig extensionConfig = ConfigFile.Configs["Extensions"];
                extensionList = new List<string>(extensionConfig.GetKeys());
            }
            catch (Exception)
            {
                Logger.Log("Failed to load [Extensions] section from " + CONFIG_FILE, Helpers.LogLevel.Error);
                return false;
            }

            InitHttpServer(HttpPort, true);

            RegionHandle = Utils.UIntsToLong(REGION_X, REGION_Y);

            try
            {
                // Create a list of references for .cs extensions that are compiled at runtime
                List<string> references = new List<string>();
                references.Add("OpenMetaverseTypes.dll");
                references.Add("OpenMetaverse.dll");
                references.Add("Simian.exe");

                // Search the Simian class for member variables that are interfaces
                List<FieldInfo> assignables = ExtensionLoader<Simian>.GetInterfaces(this);

                // Load extensions from the current executing assembly, Simian.*.dll assemblies on disk, and
                // Simian.*.cs source files on disk. Automatically assign extensions that implement interfaces
                // to the list of interface variables in "assignables"
                ExtensionLoader<Simian>.LoadAllExtensions(Assembly.GetExecutingAssembly(),
                    AppDomain.CurrentDomain.BaseDirectory, extensionList, references,
                    "Simian.*.dll", "Simian.*.cs", this, assignables);
            }
            catch (ExtensionException ex)
            {
                Logger.Log("Extension loading failed, shutting down: " + ex.Message, Helpers.LogLevel.Error);
                Stop();
                return false;
            }

            foreach (IExtension<Simian> extension in ExtensionLoader<Simian>.Extensions)
            {
                // Track all of the extensions with persistence
                if (extension is IPersistable)
                    PersistentExtensions.Add((IPersistable)extension);
            }

            // Start all of the extensions
            foreach (IExtension<Simian> extension in ExtensionLoader<Simian>.Extensions)
            {
                Logger.Log("Starting extension " + extension.GetType().Name, Helpers.LogLevel.Info);
                extension.Start(this);
            }

            return true;
        }

        public void Stop()
        {
            foreach (IExtension<Simian> extension in ExtensionLoader<Simian>.Extensions)
            {
                // Stop persistence providers first
                if (extension is IPersistenceProvider)
                    extension.Stop();
            }

            foreach (IExtension<Simian> extension in ExtensionLoader<Simian>.Extensions)
            {
                // Stop all other extensions
                if (!(extension is IPersistenceProvider))
                    extension.Stop();
            }

            HttpServer.Stop();
        }

        void InitHttpServer(int port, bool ssl)
        {
            HttpServer = new WebServer(IPAddress.Any, port);

            HttpServer.Start();
        }
    }
}
