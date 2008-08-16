using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Capabilities;
using OpenMetaverse.Packets;

namespace Simian
{
    public partial class Simian
    {
        HttpServer httpServer;
        UDPServer udpServer;
        Dictionary<uint, Agent> unassociatedAgents;
        int currentCircuitCode;
        int tcpPort;
        int udpPort;

        public Simian()
        {
            unassociatedAgents = new Dictionary<uint, Agent>();
            currentCircuitCode = 0;
        }

        public void Start(int port, bool ssl)
        {
            // Put UDP listening on the same port number as the HTTP server for simplicity
            tcpPort = port;
            udpPort = port;

            InitUDPServer(udpPort);
            InitHttpServer(tcpPort, ssl);
        }

        public void Stop()
        {
            udpServer.Stop();
            httpServer.Stop();
        }

        public bool TryGetUnassociatedAgent(uint circuitCode, out Agent agent)
        {
            if (unassociatedAgents.TryGetValue(circuitCode, out agent))
            {
                lock (unassociatedAgents)
                    unassociatedAgents.Remove(circuitCode);

                return true;
            }
            else
            {
                return false;
            }
        }

        void InitUDPServer(int port)
        {
            udpServer = new UDPServer(port, this);

            udpServer.RegisterPacketCallback(PacketType.UseCircuitCode, new UDPServer.PacketCallback(UseCircuitCodeHandler));
            udpServer.RegisterPacketCallback(PacketType.StartPingCheck, new UDPServer.PacketCallback(StartPingCheckHandler));
            udpServer.RegisterPacketCallback(PacketType.CompleteAgentMovement, new UDPServer.PacketCallback(CompleteAgentMovementHandler));
            udpServer.RegisterPacketCallback(PacketType.AgentUpdate, new UDPServer.PacketCallback(AgentUpdateHandler));
        }

        void InitHttpServer(int port, bool ssl)
        {
            httpServer = new HttpServer(tcpPort, ssl);

            // Login webpage HEAD request, used to check if the login webpage is alive
            HttpRequestSignature signature = new HttpRequestSignature();
            signature.Method = "head";
            signature.ContentType = String.Empty;
            signature.Path = "/loginpage";
            HttpServer.HttpRequestCallback callback = new HttpServer.HttpRequestCallback(LoginWebpageHeadHandler);
            HttpServer.HttpRequestHandler handler = new HttpServer.HttpRequestHandler(signature, callback);
            httpServer.AddHandler(handler);

            // Login webpage GET request, gets the login webpage data (purely aesthetic)
            signature.Method = "get";
            signature.ContentType = String.Empty;
            signature.Path = "/loginpage";
            callback = new HttpServer.HttpRequestCallback(LoginWebpageGetHandler);
            handler.Signature = signature;
            handler.Callback = callback;
            httpServer.AddHandler(handler);

            // Client XML-RPC login
            signature.Method = "post";
            signature.ContentType = "text/xml";
            signature.Path = "/login";
            callback = new HttpServer.HttpRequestCallback(LoginXmlRpcPostHandler);
            handler.Signature = signature;
            handler.Callback = callback;
            httpServer.AddHandler(handler);

            // Client LLSD login
            signature.Method = "post";
            signature.ContentType = "application/xml";
            signature.Path = "/login";
            callback = new HttpServer.HttpRequestCallback(LoginLLSDPostHandler);
            handler.Signature = signature;
            handler.Callback = callback;
            httpServer.AddHandler(handler);

            httpServer.Start();
        }

        void LoginWebpageHeadHandler(ref HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.StatusDescription = "OK";
        }

        void LoginWebpageGetHandler(ref HttpListenerContext context)
        {
            string pageContent = "<html><head><title>Simian</title></head><body><br/><h1>Welcome to Simian</h1></body></html>";
            byte[] pageData = Encoding.UTF8.GetBytes(pageContent);
            context.Response.OutputStream.Write(pageData, 0, pageData.Length);
            context.Response.Close();
        }

        void LoginXmlRpcPostHandler(ref HttpListenerContext context)
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
                                            default:
                                                if (reader.Name == "string")
                                                    Console.WriteLine(String.Format("Ignore login xml value: name={0}, value={1}", name, reader.ReadInnerXml()));
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
                XmlWriter writer = XmlWriter.Create(context.Response.OutputStream);
                responseData.ToXmlRpc(writer);
                writer.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        void LoginLLSDPostHandler(ref HttpListenerContext context)
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
            uint regionX = 256000;
            uint regionY = 256000;

            // Setup default login response values
            LoginResponseData response;

            response.AgentID = UUID.Random();
            response.SecureSessionID = UUID.Random();
            response.SessionID = UUID.Random();
            response.CircuitCode = CreateAgentCircuit(response.AgentID, response.SessionID, response.SecureSessionID);
            response.AgentAccess = "M";
            response.BuddyList = null;
            response.FirstName = firstName;
            response.HomeLookAt = Vector3.UnitX;
            response.HomePosition = new Vector3(128f, 128f, 25f);
            response.HomeRegion = Helpers.UIntsToLong(regionX, regionY);
            response.InventoryFolders = null;
            response.InventoryRoot = UUID.Random();
            response.LastName = lastName;
            response.LibraryFolders = null;
            response.LibraryOwner = response.AgentID;
            response.LibraryRoot = UUID.Random();
            response.LookAt = Vector3.UnitX;
            response.Message = "Welcome to Simian";
            response.Reason = String.Empty;
            response.RegionX = regionX;
            response.RegionY = regionY;
            response.SecondsSinceEpoch = DateTime.Now;
            // FIXME: Actually generate a seed capability
            response.SeedCapability = String.Format("http://{0}:{1}/seed_caps", IPAddress.Loopback, tcpPort);
            response.SimIP = IPAddress.Loopback;
            response.SimPort = (ushort)udpPort;
            response.StartLocation = "last";
            response.Success = true;
            
            return response;
        }

        uint CreateAgentCircuit(UUID agentID, UUID sessionID, UUID secureSessionID)
        {
            uint circuitCode = (uint)Interlocked.Increment(ref currentCircuitCode);

            Agent agent = new Agent(udpServer, agentID, sessionID, secureSessionID, circuitCode);

            // Put this client in the list of clients that have not been associated with an IPEndPoint yet
            lock (unassociatedAgents)
                unassociatedAgents[circuitCode] = agent;

            Logger.Log("Created a circuit for agent " + agentID.ToString(), Helpers.LogLevel.Info);

            return circuitCode;
        }
    }
}
