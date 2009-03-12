using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;
using System.Threading;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using ExtensionLoader;
using ExtensionLoader.Config;
using HttpServer;
using HttpListener = HttpServer.HttpListener;
using OpenMetaverse;
using OpenMetaverse.Http;

namespace Simian
{
    public partial class Simian
    {
        public const string DATA_DIR = "SimianData/";
        public const string CONFIG_FILE = DATA_DIR + "Simian.ini";
        public const string REGION_CONFIG_DIR = DATA_DIR + "RegionConfig/";
        public const string ASSET_CACHE_DIR = DATA_DIR + "AssetCache/";
        public const string DEFAULT_ASSET_DIR = DATA_DIR + "DefaultAssets/";

        public Uri HttpUri;
        public HttpListener HttpServer;
        public IniConfigSource ConfigFile;
        public List<string> ExtensionList;

        // Server Interfaces
        public IAccountProvider Accounts;
        public IAssetProvider Assets;
        public IAuthenticationProvider Authentication;
        public ICapabilitiesProvider Capabilities;
        public IGridProvider Grid;
        public IInventoryProvider Inventory;
        public IMeshingProvider Mesher;
        public IMessagingProvider Messages;
        public IPermissionsProvider Permissions;

        // Regions/Scenes
        public List<ISceneProvider> Scenes = new List<ISceneProvider>();
        // Persistent extensions
        public List<IPersistable> PersistentExtensions = new List<IPersistable>();

        ExtensionLoader<Simian> extensions = new ExtensionLoader<Simian>();

        public Simian()
        {
        }

        public bool Start()
        {
            IConfig httpConfig;

            try
            {
                // Load the extension list (and ordering) from our config file
                ConfigFile = new IniConfigSource(CONFIG_FILE);
                httpConfig = ConfigFile.Configs["Http"];
                IConfig extensionConfig = ConfigFile.Configs["Extensions"];
                ExtensionList = new List<string>(extensionConfig.GetKeys());
            }
            catch (Exception)
            {
                Logger.Log("Failed to load [Extensions] section from " + CONFIG_FILE, Helpers.LogLevel.Error);
                return false;
            }

            #region HTTP Server

            int port = httpConfig.GetInt("ListenPort");
            string hostname = httpConfig.GetString("Hostname", null);
            string sslCertFile = httpConfig.GetString("SSLCertFile", null);
            IPAddress address = IPAddress.Any;

            if (String.IsNullOrEmpty(hostname))
            {
                hostname = Dns.GetHostName();
            }
            else
            {
                IPHostEntry entry = Dns.GetHostEntry(hostname);
                if (entry != null && entry.AddressList.Length > 0)
                    address = entry.AddressList[0];
            }

            if (!String.IsNullOrEmpty(sslCertFile))
            {
                // HTTPS mode
                X509Certificate serverCert;
                try { serverCert = X509Certificate.CreateFromCertFile(sslCertFile); }
                catch (Exception ex)
                {
                    Logger.Log("Failed to load SSL certificate file \"" + sslCertFile + "\": " + ex.Message,
                        Helpers.LogLevel.Error);
                    return false;
                }
                HttpServer = HttpListener.Create(log4netLogWriter.Instance, address, port, serverCert);
                HttpUri = new Uri("https://" + hostname + (port != 80 ? (":" + port) : String.Empty));
            }
            else
            {
                // HTTP mode
                HttpServer = HttpListener.Create(log4netLogWriter.Instance, address, port);
                HttpUri = new Uri("http://" + hostname + (port != 80 ? (":" + port) : String.Empty));
            }

            HttpServer.Start(10);
            Logger.Log("Simian is listening at " + HttpUri.ToString(), Helpers.LogLevel.Info);

            #endregion HTTP Server

            #region Server Extensions

            try
            {
                // Create a list of references for .cs extensions that are compiled at runtime
                List<string> references = new List<string>();
                references.Add("OpenMetaverseTypes.dll");
                references.Add("OpenMetaverse.dll");
                references.Add("Simian.exe");

                // Load extensions from the current executing assembly, Simian.*.dll assemblies on disk, and
                // Simian.*.cs source files on disk.
                extensions.LoadAllExtensions(Assembly.GetExecutingAssembly(),
                    AppDomain.CurrentDomain.BaseDirectory, ExtensionList, references,
                    "Simian.*.dll", "Simian.*.cs");

                // Automatically assign extensions that implement interfaces to the list of interface
                // variables in "assignables"
                extensions.AssignExtensions(this, extensions.GetInterfaces(this));
            }
            catch (ExtensionException ex)
            {
                Logger.Log("Extension loading failed, shutting down: " + ex.Message, Helpers.LogLevel.Error);
                Stop();
                return false;
            }

            foreach (IExtension<Simian> extension in extensions.Extensions)
            {
                // Track all of the extensions with persistence
                if (extension is IPersistable)
                    PersistentExtensions.Add((IPersistable)extension);
            }

            // Start all of the extensions
            foreach (IExtension<Simian> extension in extensions.Extensions)
            {
                Logger.Log("Starting Simian extension " + extension.GetType().Name, Helpers.LogLevel.Info);
                extension.Start(this);
            }

            #endregion Server Extensions

            #region Region Loading

            try
            {
                string[] configFiles = Directory.GetFiles(REGION_CONFIG_DIR, "*.ini", SearchOption.AllDirectories);

                for (int i = 0; i < configFiles.Length; i++)
                {
                    IniConfigSource source = new IniConfigSource(configFiles[i]);
                    IConfig regionConfig = source.Configs["Region"];

                    string name = regionConfig.GetString("Name", null);
                    string defaultTerrain = regionConfig.GetString("DefaultTerrain", null);
                    int udpPort = regionConfig.GetInt("UDPPort", 0);
                    uint regionX, regionY;
                    UInt32.TryParse(regionConfig.GetString("RegionX", "0"), out regionX);
                    UInt32.TryParse(regionConfig.GetString("RegionY", "0"), out regionY);
                    int staticObjectLimit = regionConfig.GetInt("StaticObjectLimit", 0);
                    int physicalObjectLimit = regionConfig.GetInt("PhysicalObjectLimit", 0);

                    if (String.IsNullOrEmpty(name) || regionX == 0 || regionY == 0)
                    {
                        Logger.Log("Incomplete information in " + configFiles[i] + ", skipping", Helpers.LogLevel.Warning);
                        continue;
                    }

                    // Get the IPEndPoint for this region
                    IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 9000);

                    // TODO: Support non-SceneManager scenes?
                    ISceneProvider scene = new SceneManager();
                    scene.Start(this, name, endpoint, regionX, regionY, defaultTerrain, staticObjectLimit, physicalObjectLimit);
                    Scenes.Add(scene);

                    // FIXME: Use IGridProvider to actually register the scenes into spaces
                    break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to load region config files: " + ex.Message, Helpers.LogLevel.Error);
                return false;
            }

            #endregion Region Loading

            return true;
        }

        public void Stop()
        {
            // FIXME: Scenes shouldn't be publically exposed. This will end in tears
            for (int i = 0; i < Scenes.Count; i++)
            {
                Scenes[i].Stop();
            }

            foreach (IExtension<Simian> extension in extensions.Extensions)
            {
                // Stop persistence providers first
                if (extension is IPersistenceProvider)
                {
                    Logger.Log("Stopping Simian extension " + extension.GetType().Name, Helpers.LogLevel.Info);
                    extension.Stop();
                }
            }

            foreach (IExtension<Simian> extension in extensions.Extensions)
            {
                // Stop all other extensions
                if (!(extension is IPersistenceProvider))
                {
                    Logger.Log("Stopping Simian extension " + extension.GetType().Name, Helpers.LogLevel.Info);
                    extension.Stop();
                }
            }

            HttpServer.Stop();
        }
    }
}
