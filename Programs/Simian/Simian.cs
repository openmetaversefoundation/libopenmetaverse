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
using OpenMetaverse;
using OpenMetaverse.Capabilities;

namespace Simian
{
    public partial class Simian
    {
        public const string CONFIG_FILE = "Simian.ini";

        // TODO: Don't hard-code these
        public const uint REGION_X = 256000;
        public const uint REGION_Y = 256000;

        public int UDPPort = 9000;
        public int HttpPort = 9000;
        public string DataDir = "SimianData/";

        public HttpServer HttpServer;
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

        // Persistent extensions
        public List<IPersistable> PersistentExtensions = new List<IPersistable>();

        /// <summary>All of the agents currently connected to this UDP server</summary>
        public Dictionary<UUID, Agent> Agents = new Dictionary<UUID, Agent>();

        public Simian()
        {
        }

        public bool Start(int port, bool ssl)
        {
            HttpPort = port;
            UDPPort = port;
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

            InitHttpServer(HttpPort, ssl);

            RegionHandle = Utils.UIntsToLong(REGION_X, REGION_Y);

            try
            {
                // Load all of the extensions
                List<string> references = new List<string>();
                references.Add("OpenMetaverseTypes.dll");
                references.Add("OpenMetaverse.dll");
                references.Add("Simian.exe");

                List<FieldInfo> assignables = ExtensionLoader<Simian>.GetInterfaces(this);

                ExtensionLoader<Simian>.LoadAllExtensions(Assembly.GetExecutingAssembly(),
                    AppDomain.CurrentDomain.BaseDirectory, extensionList, references,
                    "Simian.*.dll", "Simian.*.cs", this, assignables);
            }
            catch (ExtensionException ex)
            {
                Logger.Log("Interface loading failed, shutting down: " + ex.Message, Helpers.LogLevel.Error);
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
            HttpServer = new HttpServer(port, ssl);

            // Login webpage HEAD request, used to check if the login webpage is alive
            HttpServer.AddHandler("head", null, "^/$", LoginWebpageHeadHandler);

            // Login webpage GET request, gets the login webpage data (purely aesthetic)
            HttpServer.AddHandler("get", null, @"^/(\?.*)?$", LoginWebpageGetHandler);

            // Client XML-RPC login
            HttpServer.AddHandler("post", "text/xml", "^/$", LoginXmlRpcPostHandler);

            // Client LLSD login
            HttpServer.AddHandler("post", "application/xml", "^/$", LoginLLSDPostHandler);

            HttpServer.Start();
        }

        void LoginWebpageHeadHandler(HttpRequestSignature signature, ref HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.StatusDescription = "OK";
            context.Response.Close();
        }

        void LoginWebpageGetHandler(HttpRequestSignature signature, ref HttpListenerContext context)
        {
            string pageContent = "<html><head><title>Simian</title></head><body><br/><h1>Welcome to Simian</h1></body></html>";
            byte[] pageData = Encoding.UTF8.GetBytes(pageContent);
            context.Response.OutputStream.Write(pageData, 0, pageData.Length);
            context.Response.Close();
        }

        void LoginXmlRpcPostHandler(HttpRequestSignature signature, ref HttpListenerContext context)
        {
            string
                firstName = String.Empty,
                lastName = String.Empty,
                password = String.Empty,
                start = String.Empty,
                version = String.Empty,
                channel = String.Empty;

            try
            {
                // Parse the incoming XML
                XmlReader reader = XmlReader.Create(context.Request.InputStream);

                reader.ReadStartElement("methodCall");
                {
                    string methodName = reader.ReadElementContentAsString("methodName", String.Empty);

                    if (methodName == "login_to_simulator")
                    {
                        reader.ReadStartElement("params");
                        reader.ReadStartElement("param");
                        reader.ReadStartElement("value");
                        reader.ReadStartElement("struct");
                        {
                            while (reader.Name == "member")
                            {
                                reader.ReadStartElement("member");
                                {
                                    string name = reader.ReadElementContentAsString("name", String.Empty);

                                    reader.ReadStartElement("value");
                                    {
                                        switch (name)
                                        {
                                            case "first":
                                                firstName = reader.ReadElementContentAsString("string", String.Empty);
                                                break;
                                            case "last":
                                                lastName = reader.ReadElementContentAsString("string", String.Empty);
                                                break;
                                            case "passwd":
                                                password = reader.ReadElementContentAsString("string", String.Empty);
                                                break;
                                            case "start":
                                                start = reader.ReadElementContentAsString("string", String.Empty);
                                                break;
                                            case "version":
                                                version = reader.ReadElementContentAsString("string", String.Empty);
                                                break;
                                            case "channel":
                                                channel = reader.ReadElementContentAsString("string", String.Empty);
                                                break;
                                            case "platform":
                                            case "mac":
                                            case "id0":
                                            case "options":
                                                // Ignored values
                                                reader.ReadInnerXml();
                                                break;
                                            default:
                                                if (reader.Name == "string")
                                                    Console.WriteLine(String.Format("Ignore login xml value: name={0}", name, reader.ReadInnerXml()));
                                                else
                                                    Console.WriteLine(String.Format("Unknown login xml: name={0}, value={1}", name, reader.ReadInnerXml()));
                                                break;
                                        }
                                    }
                                    reader.ReadEndElement();
                                }
                                reader.ReadEndElement();
                            }
                        }
                        reader.ReadEndElement();
                        reader.ReadEndElement();
                        reader.ReadEndElement();
                        reader.ReadEndElement();
                    }
                }
                reader.ReadEndElement();
                reader.Close();

                LoginResponseData responseData = HandleLogin(firstName, lastName, password, start, version, channel);
                if (responseData.Success)
                    responseData.InventorySkeleton = Inventory.CreateInventorySkeleton(responseData.AgentID);
                XmlWriter writer = XmlWriter.Create(context.Response.OutputStream);
                responseData.ToXmlRpc(writer);
                writer.Close();
                context.Response.Close();
            }
            catch (Exception ex)
            {
                Logger.Log("XmlRpc login error: " + ex.Message, Helpers.LogLevel.Error, ex);
            }
        }

        void LoginLLSDPostHandler(HttpRequestSignature signature, ref HttpListenerContext context)
        {
            string body = String.Empty;

            using (StreamReader reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                body = reader.ReadToEnd();
            }

            Console.WriteLine(body);
        }

        LoginResponseData HandleLogin(string firstName, string lastName, string password, string start, string version, string channel)
        {
            LoginResponseData response = new LoginResponseData();
            Agent agent;

            UUID agentID = Authentication.Authenticate(firstName, lastName, password);
            if (agentID != UUID.Zero)
            {
                // Authentication successful, create a login instance of this agent
                agent = Accounts.CreateInstance(agentID);

                if (agent != null)
                {
                    // Assign a circuit code and insert the agent into the unassociatedAgents dictionary
                    agent.CircuitCode = UDP.CreateCircuit(agent);

                    agent.TickLastPacketReceived = Environment.TickCount;
                    agent.LastLoginTime = Utils.DateTimeToUnixTime(DateTime.Now);

                    // Get this machine's IP address
                    IPHostEntry addresses = Dns.GetHostByName(Dns.GetHostName());
                    IPAddress simIP = addresses.AddressList.Length > 0 ?
                        addresses.AddressList[addresses.AddressList.Length - 1] :IPAddress.Loopback;

                    response.AgentID = agent.AgentID;
                    response.SecureSessionID = agent.SecureSessionID;
                    response.SessionID = agent.SessionID;
                    response.CircuitCode = agent.CircuitCode;
                    response.AgentAccess = agent.AccessLevel;
                    response.BuddyList = null; // FIXME:
                    response.FirstName = agent.FirstName;
                    response.HomeLookAt = agent.HomeLookAt;
                    response.HomePosition = agent.HomePosition;
                    response.HomeRegion = agent.HomeRegionHandle;
                    response.InventoryRoot = agent.InventoryRoot;
                    response.InventorySkeleton = null; // FIXME:
                    response.LastName = agent.LastName;
                    response.LibraryOwner = agent.InventoryLibraryOwner;
                    response.LibraryRoot = agent.InventoryLibraryRoot;
                    response.LibrarySkeleton = null; // FIXME:
                    response.LookAt = agent.CurrentLookAt;
                    response.Message = "Welcome to Simian";
                    response.Reason = String.Empty;

                    uint regionX, regionY;
                    Utils.LongToUInts(agent.CurrentRegionHandle, out regionX, out regionY);
                    response.RegionX = regionX;
                    response.RegionY = regionY;

                    response.SecondsSinceEpoch = DateTime.Now;
                    // FIXME: Actually generate a seed capability
                    response.SeedCapability = String.Format("http://{0}:{1}/seed_caps", simIP, HttpPort);
                    response.SimIP = simIP;
                    response.SimPort = (ushort)UDPPort;
                    response.StartLocation = "last"; // FIXME:
                    response.Success = true;
                }
                else
                {
                    // Something went wrong creating an agent instance, return a fail response
                    response.AgentID = agentID;
                    response.FirstName = firstName;
                    response.LastName = lastName;
                    response.Message = "Failed to create an account instance";
                    response.Reason = "account";
                    response.Success = false;
                }
            }
            else
            {
                // Authentication failed, return a fail response
                response.AgentID = agentID;
                response.FirstName = firstName;
                response.LastName = lastName;
                response.Message = "Authentication failed";
                response.Reason = "key";
                response.Success = false;
            }

            return response;
        }
    }
}
