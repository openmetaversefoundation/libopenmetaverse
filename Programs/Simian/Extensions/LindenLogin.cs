using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using ExtensionLoader;
using HttpServer;
using OpenMetaverse;

namespace Simian
{
    public class LindenLogin : IExtension<Simian>
    {
        Simian server;

        public LindenLogin()
        {
        }

        public bool Start(Simian server)
        {
            this.server = server;

            // Login webpage HEAD request, used to check if the login webpage is alive
            server.HttpServer.AddHandler("head", null, "^/$", LoginWebpageHeadHandler);

            // Login webpage GET request, gets the login webpage data (purely aesthetic)
            server.HttpServer.AddHandler("get", null, @"^/(\?.*)?$", LoginWebpageGetHandler);

            // Client XML-RPC login
            server.HttpServer.AddHandler("post", "text/xml", "^/$", LoginXmlRpcPostHandler);

            // Client LLSD login
            server.HttpServer.AddHandler("post", "application/xml", "^/$", LoginLLSDPostHandler);
            return true;
        }

        public void Stop()
        {
        }

        bool LoginWebpageHeadHandler(IHttpClientContext client, IHttpRequest request, IHttpResponse response)
        {
            return true;
        }

        bool LoginWebpageGetHandler(IHttpClientContext client, IHttpRequest request, IHttpResponse response)
        {
            string pageContent = "<html><head><title>Simian</title></head><body><br/><h1>Welcome to Simian</h1></body></html>";
            byte[] pageData = Encoding.UTF8.GetBytes(pageContent);
            response.Body.Write(pageData, 0, pageData.Length);
            response.Body.Flush();
            return true;
        }

        bool LoginXmlRpcPostHandler(IHttpClientContext client, IHttpRequest request, IHttpResponse response)
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
                XmlReader reader = XmlReader.Create(request.Body);

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
                    responseData.InventorySkeleton = server.Inventory.CreateInventorySkeleton(responseData.AgentID);

                MemoryStream memoryStream = new MemoryStream();
                using (XmlWriter writer = XmlWriter.Create(memoryStream))
                {
                    responseData.ToXmlRpc(writer);
                    writer.Flush();
                }

                response.ContentLength = memoryStream.Length;
                response.Body.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                response.Body.Flush();
            }
            catch (Exception ex)
            {
                Logger.Log("XmlRpc login error: " + ex.Message, Helpers.LogLevel.Error, ex);
            }

            return true;
        }

        bool LoginLLSDPostHandler(IHttpClientContext client, IHttpRequest request, IHttpResponse response)
        {
            string body = String.Empty;

            using (StreamReader reader = new StreamReader(request.Body, request.ContentEncoding))
            {
                body = reader.ReadToEnd();
            }

            Logger.DebugLog("LLSD login is not implemented:\n" + body);
            return true;
        }

        LoginResponseData HandleLogin(string firstName, string lastName, string password, string start, string version, string channel)
        {
            ISceneProvider scene = server.Grid.GetDefaultLocalScene();

            LoginResponseData response = new LoginResponseData();
            AgentInfo agentInfo;

            // Attempt to authenticate
            UUID agentID = server.Authentication.Authenticate(firstName, lastName, password);
            if (agentID != UUID.Zero)
            {
                // Authentication successful, create a login instance of this agent
                agentInfo = server.Accounts.CreateInstance(agentID);
                if (agentInfo != null)
                {
                    Agent agent = new Agent(new SimulationObject(new Avatar(), scene), agentInfo);
                    
                    // Set the avatar ID
                    agent.Avatar.Prim.ID = agentInfo.ID;

                    // Random session IDs
                    agent.SessionID = UUID.Random();
                    agent.SecureSessionID = UUID.Random();

                    // Create a seed capability for this agent
                    agent.SeedCapability = server.Capabilities.CreateCapability(scene.SeedCapabilityHandler, false, agentID);

                    agent.TickLastPacketReceived = Environment.TickCount;
                    agent.Info.LastLoginTime = Utils.DateTimeToUnixTime(DateTime.Now);

                    // Assign a circuit code and track the agent as an unassociated agent (no UDP connection yet)
                    agent.CircuitCode = scene.UDP.CreateCircuit(agent);

                    // Get the IP address of the sim (IPAndPort may be storing IPAdress.Any, aka 0.0.0.0)
                    IPAddress simIP = scene.IPAndPort.Address;
                    if (simIP == IPAddress.Any)
                    {
                        // Get this machine's IP address
                        IPHostEntry entry = Dns.GetHostEntry(server.HttpUri.DnsSafeHost);
                        simIP = entry.AddressList.Length > 0 ? entry.AddressList[entry.AddressList.Length - 1] : IPAddress.Loopback;
                    }

                    response.AgentID = agent.ID;
                    response.SecureSessionID = agent.SecureSessionID;
                    response.SessionID = agent.SessionID;
                    response.CircuitCode = agent.CircuitCode;
                    response.AgentAccess = agent.Info.AccessLevel;
                    response.BuddyList = null; // FIXME:
                    response.FirstName = agent.Info.FirstName;
                    response.HomeLookAt = agent.Info.HomeLookAt;
                    response.HomePosition = agent.Info.HomePosition;
                    response.HomeRegion = agent.Info.HomeRegionHandle;
                    response.InventoryRoot = agent.Info.InventoryRoot;
                    response.InventorySkeleton = null; // FIXME:
                    response.LastName = agent.Info.LastName;
                    response.LibraryOwner = agent.Info.InventoryLibraryOwner;
                    response.LibraryRoot = agent.Info.InventoryLibraryRoot;
                    response.LibrarySkeleton = null; // FIXME:
                    response.LookAt = agent.CurrentLookAt;
                    response.Message = "Welcome to Simian";
                    response.Reason = String.Empty;

                    response.RegionX = scene.RegionX * 256;
                    response.RegionY = scene.RegionY * 256;

                    response.SecondsSinceEpoch = DateTime.Now;
                    response.SeedCapability = agent.SeedCapability.ToString();
                    response.SimIP = simIP;
                    response.SimPort = (ushort)scene.IPAndPort.Port;
                    response.StartLocation = "last"; // FIXME:
                    response.Success = true;

                    Logger.DebugLog("Sending a login success response with circuit_code " + response.CircuitCode);
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
