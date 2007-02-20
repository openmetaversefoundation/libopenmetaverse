/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names 
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Timers;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using System.Globalization;
using System.IO;
using Nwc.XmlRpc;
using Nii.JSON;
using libsecondlife.Packets;

namespace libsecondlife
{
    /// <summary>
    /// This exception is thrown whenever a network operation is attempted 
    /// without a network connection.
    /// </summary>
    public class NotConnectedException : ApplicationException { }

    /// <summary>
    /// Capabilities is the name of the bi-directional HTTP REST protocol that
    /// Second Life uses to communicate transactions such as teleporting or
    /// asset transfers
    /// </summary>
    public class Caps
    {
        /// <summary>
        /// Triggered when an event is received via the EventQueueGet capability;
        /// </summary>
        /// <param name="message"></param>
        /// <param name="body"></param>
        public delegate void EventQueueCallback(string message, object body);

        /// <summary>Reference to the SecondLife client this system is connected to</summary>
        public SecondLife Client;
        /// <summary>Reference to the simulator this system is connected to</summary>
        public Simulator Simulator;

        internal bool Dead = false;
        internal string Seedcaps;

        private StringDictionary Capabilities = new StringDictionary();
        private Thread EventThread;
        private List<EventQueueCallback> Callbacks;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="simulator"></param>
        /// <param name="seedcaps"></param>
        /// <param name="callbacks"></param>
        public Caps(SecondLife client, Simulator simulator, string seedcaps, List<EventQueueCallback> callbacks)
        {
            Client = client;
            Simulator = simulator;
            Seedcaps = seedcaps;
            Callbacks = callbacks;
            ArrayList req = new ArrayList();
            req.Add("MapLayer");
            req.Add("MapLayerGod");
            req.Add("NewAgentInventory");
            req.Add("EventQueueGet");
            Hashtable resp = (Hashtable)LLSDRequest(seedcaps, req);

            if (resp != null)
            {
                foreach (string cap in resp.Keys)
                {
                    Client.Log("Got cap " + cap + ": " + (string)resp[cap], Helpers.LogLevel.Info);
                    Capabilities[cap] = (string)resp[cap];
                }

                if (Capabilities.ContainsKey("EventQueueGet"))
                {
                    Client.Log("Running event queue", Helpers.LogLevel.Info);
                    EventThread = new Thread(new ThreadStart(EventQueue));
                    EventThread.Start();
                }
            }
            else
            {
                Client.Log("Disabling caps for " + Simulator.ToString(), Helpers.LogLevel.Info);
                Dead = true;
            }
        }

        private void EventQueue()
        {
            bool gotresp = false; long ack = 0;
            string cap = Capabilities["EventQueueGet"];

            while (!Dead)
            {
                try
                {
                    Hashtable req = new Hashtable();

                    if (gotresp)
                        req["ack"] = ack;
                    else
                        req["ack"] = null;

                    req["done"] = false;

                    Hashtable resp = (Hashtable)LLSDRequest(cap, req);
                    if (resp != null)
                    {
                        ack = (long)resp["id"];
                        gotresp = true;
                        ArrayList events = (ArrayList)resp["events"];

                        foreach (Hashtable evt in events)
                        {
                            string msg = (string)evt["message"];
                            object body = (object)evt["body"];

                            Client.DebugLog("Event " + msg + ":" + Environment.NewLine + LLSD.LLSDDump(body, 0));

                            if (!Dead)
                            {
                                foreach (EventQueueCallback callback in Callbacks)
                                {
                                    try { callback(msg, body); }
                                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                                }
                            }
                        }
                    }
                    else
                    {
                        Client.Log("Disabling caps for " + Simulator.ToString(), Helpers.LogLevel.Info);
                        Dead = true;
                    }
                }
                catch (WebException e)
                {
                    // Perfectly normal
                    Client.DebugLog("EventQueueGet: " + e.Message);
                }
            }
        }

        private object LLSDRequest(string uri, object req)
        {
            byte[] respBuf = null;
            byte[] data = LLSD.LLSDSerialize(req);

        Start:

            try
            {
                WebRequest wreq = WebRequest.Create(uri);
                wreq.Method = "POST";
                wreq.ContentLength = data.Length;
                //wreq.ContentType = "text/xml";

                Stream reqStream = wreq.GetRequestStream();
                reqStream.Write(data, 0, data.Length);
                
                // FIXME: Switch this to BeginGetResponse so this blocking request doesn't prevent
                // us from killing the app
                HttpWebResponse wresp = (HttpWebResponse)wreq.GetResponse();
                BinaryReader reader = new BinaryReader(wresp.GetResponseStream());
                respBuf = reader.ReadBytes((int)wresp.ContentLength);
                reqStream.Close();
                wresp.Close();
            }
            catch (WebException e)
            {
                // We seem to get HTTP errors fairly commonly from the CAPS servers
                Client.DebugLog(e.Message);

                if (Client.Network.Connected)
                {
                    switch (e.Status)
                    {
                        case WebExceptionStatus.ProtocolError:
                        case WebExceptionStatus.Timeout:
                        case WebExceptionStatus.ConnectFailure:
                            Client.DebugLog("CAPS error: " + e.Status.ToString());
                            goto Start;
                        default:
                            Client.Log("CAPS error, " + e.Status.ToString(), Helpers.LogLevel.Warning);
                            return null;
                    }
                }
            }

            if (respBuf != null) return LLSD.LLSDDeserialize(respBuf);
            else return null;
        }

        public void Disconnect()
        {
            Dead = true;
        }
    }

    /// <summary>
    /// NetworkManager is responsible for managing the network layer of 
    /// libsecondlife. It tracks all the server connections, serializes 
    /// outgoing traffic and deserializes incoming traffic, and provides
    /// instances of delegates for network-related events.
    /// </summary>
    public class NetworkManager
    {
        /// <summary>
        /// Explains why a simulator or the grid disconnected from us
        /// </summary>
        public enum DisconnectType
        {
            /// <summary>The client requested the logout or simulator disconnect</summary>
            ClientInitiated,
            /// <summary>The server notified us that it is disconnecting</summary>
            ServerInitiated,
            /// <summary>Either a socket was closed or network traffic timed out</summary>
            NetworkTimeout
        }


        /// <summary>
        /// Coupled with RegisterCallback(), this is triggered whenever a packet
        /// of a registered type is received
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        public delegate void PacketCallback(Packet packet, Simulator simulator);
        /// <summary>
        /// Triggered when a simulator other than the simulator that is currently
        /// being occupied disconnects for whatever reason
        /// </summary>
        /// <param name="simulator">The simulator that disconnected, which will become a null
        /// reference after the callback is finished</param>
        /// <param name="reason">Enumeration explaining the reason for the disconnect</param>
        public delegate void SimDisconnectCallback(Simulator simulator, DisconnectType reason);
        /// <summary>
        /// Triggered when we are logged out of the grid due to a simulator request,
        /// client request, network timeout, or any other cause
        /// </summary>
        /// <param name="reason">Enumeration explaining the reason for the disconnect</param>
        /// <param name="message">If we were logged out by the simulator, this 
        /// is a message explaining why</param>
        public delegate void DisconnectCallback(DisconnectType reason, string message);
        /// <summary>
        /// Triggered when CurrentSim changes
        /// </summary>
        /// <param name="PreviousSimulator">A reference to the old value of CurrentSim</param>
        public delegate void CurrentSimChangedCallback(Simulator PreviousSimulator);


        /// <summary>
        /// An event for the connection to a simulator other than the currently
        /// occupied one disconnecting
        /// </summary>
        public event SimDisconnectCallback OnSimDisconnected;
        /// <summary>
        /// An event for being logged out either through client request, server
        /// forced, or network error
        /// </summary>
        public event DisconnectCallback OnDisconnected;
        /// <summary>
        /// An event for when CurrentSim changes
        /// </summary>
        public event CurrentSimChangedCallback OnCurrentSimChanged;

        /// <summary>The permanent UUID for the logged in avatar</summary>
        public LLUUID AgentID = LLUUID.Zero;
        /// <summary>Temporary UUID assigned to this session, used for 
        /// verifying our identity in packets</summary>
        public LLUUID SessionID = LLUUID.Zero;
        /// <summary>Shared secret UUID that is never sent over the wire</summary>
        public LLUUID SecureSessionID = LLUUID.Zero;
        /// <summary>Uniquely identifier associated with our connections to
        /// simulators</summary>
        public uint CircuitCode;
        /// <summary>String holding a descriptive error on login failure, empty
        /// otherwise</summary>
        public string LoginError = String.Empty;
        /// <summary>The simulator that the logged in avatar is currently 
        /// occupying</summary>
        public Simulator CurrentSim = null;
        /// <summary>The capabilities for the current simulator</summary>
        public Caps CurrentCaps = null;
        /// <summary>The complete dictionary of all the login values returned 
        /// by the RPC login server, converted to native data types wherever 
        /// possible</summary>
        public Dictionary<string, object> LoginValues = new Dictionary<string, object>();

        /// <summary>
        /// Shows whether the network layer is logged in to the grid or not
        /// </summary>
        public bool Connected { get { return connected; } }

        /// <summary></summary>
        internal Dictionary<PacketType, List<PacketCallback>> Callbacks = new Dictionary<PacketType, List<PacketCallback>>();

        private SecondLife Client;
        private List<Simulator> Simulators = new List<Simulator>();
        private System.Timers.Timer DisconnectTimer;
        private System.Timers.Timer LogoutTimer;
        private bool connected = false;
        private List<Caps.EventQueueCallback> EventQueueCallbacks = new List<Caps.EventQueueCallback>();

        ManualResetEvent LogoutReplyEvent = new ManualResetEvent(false);

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">Reference to the SecondLife client</param>
        public NetworkManager(SecondLife client)
        {
            Client = client;
            CurrentSim = null;

            // Register the internal callbacks
            RegisterCallback(PacketType.RegionHandshake, new PacketCallback(RegionHandshakeHandler));
            RegisterCallback(PacketType.StartPingCheck, new PacketCallback(StartPingCheckHandler));
            RegisterCallback(PacketType.ParcelOverlay, new PacketCallback(ParcelOverlayHandler));
            RegisterCallback(PacketType.EnableSimulator, new PacketCallback(EnableSimulatorHandler));
            RegisterCallback(PacketType.KickUser, new PacketCallback(KickUserHandler));
            RegisterCallback(PacketType.LogoutReply, new PacketCallback(LogoutReplyHandler));

            // The proper timeout for this will get set at Login
            DisconnectTimer = new System.Timers.Timer();
            DisconnectTimer.Elapsed += new ElapsedEventHandler(DisconnectTimer_Elapsed);
        }

        /// <summary>
        /// Register an event handler for a packet. This is a low level event
        /// interface and should only be used if you are doing something not
        /// supported in libsecondlife
        /// </summary>
        /// <param name="type">Packet type to trigger events for</param>
        /// <param name="callback">Callback to fire when a packet of this type
        /// is received</param>
        public void RegisterCallback(PacketType type, PacketCallback callback)
        {
            if (!Callbacks.ContainsKey(type))
            {
                Callbacks[type] = new List<PacketCallback>();
            }

            List<PacketCallback> callbackArray = Callbacks[type];
            callbackArray.Add(callback);
        }

        /// <summary>
        /// Unregister an event handler for a packet. This is a low level event
        /// interface and should only be used if you are doing something not 
        /// supported in libsecondlife
        /// </summary>
        /// <param name="type">Packet type this callback is registered with</param>
        /// <param name="callback">Callback to stop firing events for</param>
        public void UnregisterCallback(PacketType type, PacketCallback callback)
        {
            if (!Callbacks.ContainsKey(type))
            {
                Client.Log("Trying to unregister a callback for packet " + type.ToString() +
                    " when no callbacks are setup for that packet", Helpers.LogLevel.Info);
                return;
            }

            List<PacketCallback> callbackArray = Callbacks[type];

            if (callbackArray.Contains(callback))
            {
                callbackArray.Remove(callback);
            }
            else
            {
                Client.Log("Trying to unregister a non-existant callback for packet " + type.ToString(),
                    Helpers.LogLevel.Info);
            }
        }

        /// <summary>
        /// Register a CAPS event handler
        /// </summary>
        /// <param name="callback">Callback to fire when a CAPS event is received</param>
        public void RegisterEventCallback(Caps.EventQueueCallback callback)
        {
            EventQueueCallbacks.Add(callback);
        }

        /// <summary>
        /// Send a packet to the simulator the avatar is currently occupying
        /// </summary>
        /// <param name="packet">Packet to send</param>
        public void SendPacket(Packet packet)
        {
            if (CurrentSim != null && CurrentSim.Connected)
                CurrentSim.SendPacket(packet, true);
        }

        /// <summary>
        /// Send a packet to a specified simulator
        /// </summary>
        /// <param name="packet">Packet to send</param>
        /// <param name="simulator">Simulator to send the packet to</param>
        public void SendPacket(Packet packet, Simulator simulator)
        {
            if (simulator != null)
                simulator.SendPacket(packet, true);
        }

        /// <summary>
        /// Send a raw byte array as a packet to the current simulator
        /// </summary>
        /// <param name="payload">Byte array containing a packet</param>
        /// <param name="setSequence">Whether to set the second, third, and fourth
        /// bytes of the payload to the current sequence number</param>
        public void SendPacket(byte[] payload, bool setSequence)
        {
            if (CurrentSim != null)
                CurrentSim.SendPacket(payload, setSequence);
        }

        /// <summary>
        /// Send a raw byte array as a packet to the specified simulator
        /// </summary>
        /// <param name="payload">Byte array containing a packet</param>
        /// <param name="simulator">Simulator to send the packet to</param>
        /// <param name="setSequence">Whether to set the second, third, and fourth
        /// bytes of the payload to the current sequence number</param>
        public void SendPacket(byte[] payload, Simulator simulator, bool setSequence)
        {
            if (simulator != null)
                simulator.SendPacket(payload, setSequence);
        }

        /// <summary>
        /// Build a start location URI for passing to the Login function
        /// </summary>
        /// <param name="sim">Name of the simulator to start in</param>
        /// <param name="x">X coordinate to start at</param>
        /// <param name="y">Y coordinate to start at</param>
        /// <param name="z">Z coordinate to start at</param>
        /// <returns>String with a URI that can be used to login to a specified
        /// location</returns>
        public static string StartLocation(string sim, int x, int y, int z)
        {
            // uri:sim name&x&y&z
            return "uri:" + sim.ToLower() + "&" + x + "&" + y + "&" + z;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="password"></param>
        /// <param name="mac"></param>
        /// <param name="startLocation"></param>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="patch"></param>
        /// <param name="build"></param>
        /// <param name="platform"></param>
        /// <param name="viewerDigest"></param>
        /// <param name="userAgent"></param>
        /// <param name="author"></param>
        /// <param name="md5pass"></param>
        /// <returns></returns>
        public Dictionary<string, object> DefaultLoginValues(string firstName, string lastName,
            string password, string mac, string startLocation, int major, int minor, int patch,
            int build, string platform, string viewerDigest, string userAgent, string author,
            bool md5pass)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();

            values["first"] = firstName;
            values["last"] = lastName;
            values["passwd"] = md5pass ? password : Helpers.MD5(password);
            values["start"] = startLocation;
            values["major"] = major;
            values["minor"] = minor;
            values["patch"] = patch;
            values["build"] = build;
            values["platform"] = platform;
            values["mac"] = mac;
            values["agree_to_tos"] = "true";
            values["read_critical"] = "true";
            values["viewer_digest"] = viewerDigest;
            values["user-agent"] = userAgent + " (" + Client.Settings.VERSION + ")";
            values["author"] = author;

            // Build the options array
            List<object> optionsArray = new List<object>();
            optionsArray.Add("inventory-root");
            optionsArray.Add("inventory-skeleton");
            optionsArray.Add("inventory-lib-root");
            optionsArray.Add("inventory-lib-owner");
            optionsArray.Add("inventory-skel-lib");
            optionsArray.Add("initial-outfit");
            optionsArray.Add("gestures");
            optionsArray.Add("event_categories");
            optionsArray.Add("event_notifications");
            optionsArray.Add("classified_categories");
            optionsArray.Add("buddy-list");
            optionsArray.Add("ui-config");
            optionsArray.Add("login-flags");
            optionsArray.Add("global-textures");

            values["options"] = optionsArray;

            return values;
        }

        /// <summary>
        /// Assigned by the OnConnected event. Raised when login was a success
        /// </summary>
        /// <param name="sender">Reference to the SecondLife class that called the event</param>
        public delegate void ConnectedCallback(object sender);


        /// <summary>
        /// Event raised when the client was able to connected successfully.
        /// </summary>
        /// <remarks>Uses the ConnectedCallback delegate.</remarks>
        public event ConnectedCallback OnConnected;
        /// <summary>
        /// Assigned by the OnLogoutReply callback. Raised upone receipt of a LogoutReply packet during logout process.
        /// </summary>
        /// <param name="inventoryItems"></param>
        public delegate void LogoutCallback(List<LLUUID> inventoryItems);
        /// <summary>
        /// Event raised when a logout is confirmed by the simulator
        /// </summary>
        public event LogoutCallback OnLogoutReply;

        /// <summary>
        /// Simplified login that takes the most common fields as parameters
        /// and uses defaults for the rest
        /// </summary>
        /// <param name="firstName">Account first name</param>
        /// <param name="lastName">Account last name</param>
        /// <param name="password">Account password</param>
        /// <param name="userAgent">Client application name and version</param>
        /// <param name="author">Client application author</param>
        /// <returns>Whether the login was successful or not. On failure the
        /// LoginError string will contain the error</returns>
        public bool Login(string firstName, string lastName, string password, string userAgent, string author)
        {
            Dictionary<string, object> loginParams = DefaultLoginValues(firstName, lastName, password, 
                "00:00:00:00:00:00", "last", 1, 50, 50, 50, "Win", "0", userAgent, author, false);
            return Login(loginParams);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="password"></param>
        /// <param name="userAgent"></param>
        /// <param name="author"></param>
        /// <param name="md5pass"></param>
        /// <returns></returns>
        public bool Login(string firstName, string lastName, string password, string userAgent, string author, 
            bool md5pass)
        {
            Dictionary<string, object> loginParams = DefaultLoginValues(firstName, lastName, password, 
                "00:00:00:00:00:00", "last", 1, 50, 50, 50, "Win", "0", userAgent, author, md5pass);
            return Login(loginParams);
        }

        /// <summary>
        /// Simplified login that takes the most common fields along with a
        /// starting location URI, and can accept an MD5 string instead of a
        /// plaintext password
        /// </summary>
        /// <param name="firstName">Account first name</param>
        /// <param name="lastName">Account last name</param>
        /// <param name="password">Account password or MD5 hash of the password
        /// such as $1$1682a1e45e9f957dcdf0bb56eb43319c</param>
        /// <param name="userAgent">Client application name and version</param>
        /// <param name="start">Starting location URI that can be built with
        /// StartLocation()</param>
        /// <param name="author">Client application author</param>
        /// <param name="md5pass">If true, the password field contains </param>
        /// <returns>Whether the login was successful or not. On failure the
        /// LoginError string will contain the error</returns>
        public bool Login(string firstName, string lastName, string password, string userAgent, string start,
            string author, bool md5pass)
        {
            Dictionary<string, object> loginParams = DefaultLoginValues(firstName, lastName, password,
                "00:00:00:00:00:00", start, 1, 50, 50, 50, "Win", "0", userAgent, author, md5pass);
            return Login(loginParams);
        }

        /// <summary>
        /// Login that takes a custom built dictionary of login parameters and
        /// values
        /// </summary>
        /// <param name="loginParams">Dictionary of login parameters and values
        /// that can be created with DefaultLoginValues()</param>
        /// <returns>Whether the login was successful or not. On failure the
        /// LoginError string will contain the error</returns>
        public bool Login(Dictionary<string, object> loginParams)
        {
            return Login(loginParams, Client.Settings.LOGIN_SERVER, "login_to_simulator");
        }

        /// <summary>
        /// Login that takes a custom built dictionary of login parameters and
        /// values and the URL of the login server
        /// </summary>
        /// <param name="loginParams">Dictionary of login parameters and values
        /// that can be created with DefaultLoginValues()</param>
        /// <param name="url">URL of the login server to authenticate with</param>
        /// <returns>Whether the login was successful or not. On failure the
        /// LoginError string will contain the error</returns>
        public bool Login(Dictionary<string, object> loginParams, string url)
        {
            return Login(loginParams, url, "login_to_simulator");
        }

        /// <summary>
        /// Login that takes a custom built dictionary of login parameters and
        /// values, URL of the login server, and the name of the XML-RPC method
        /// to use
        /// </summary>
        /// <param name="loginParams">Dictionary of login parameters and values
        /// that can be created with DefaultLoginValues()</param>
        /// <param name="url">URL of the login server to authenticate with</param>
        /// <param name="method">The XML-RPC method to execute on the login 
        /// server. This is generally login_to_simulator</param>
        /// <returns>Whether the login was successful or not. On failure the
        /// LoginError string will contain the error</returns>
        public bool Login(Dictionary<string, object> loginParams, string url, string method)
        {
            XmlRpcResponse result;
            XmlRpcRequest xmlrpc;
            Hashtable loginValues;

            // Re-read the timeout value for the DisconnectTimer
            DisconnectTimer.Interval = Client.Settings.SIMULATOR_TIMEOUT;

            // Clear possible old values from the last login
            LoginValues.Clear();

            // Rebuild the Dictionary<> in to a Hashtable for compatibility with XmlRpcCS
            loginValues = new Hashtable(loginParams.Count);
            foreach (KeyValuePair<string, object> kvp in loginParams)
            {
                if (kvp.Value is IList)
                {
                    IList list = ((IList)kvp.Value);
                    ArrayList array = new ArrayList(list.Count);
                    foreach (object obj in list)
                    {
                        array.Add(obj);
                    }
                    loginValues[kvp.Key] = array;
                }
                else
                {
                    loginValues[kvp.Key] = kvp.Value;
                }
            }

            // Build the XML-RPC request
            xmlrpc = new XmlRpcRequest();
            xmlrpc.MethodName = method;
            xmlrpc.Params.Clear();
            xmlrpc.Params.Add(loginValues);

            try
            {
                result = (XmlRpcResponse)xmlrpc.Send(url, Client.Settings.LOGIN_TIMEOUT);
            }
            catch (Exception e)
            {
                LoginError = "XML-RPC Error: " + e.Message;
                return false;
            }

            if (result.IsFault)
            {
                Client.Log("Fault " + result.FaultCode + ": " + result.FaultString, Helpers.LogLevel.Error);
                LoginError = "XML-RPC Fault: " + result.FaultCode + ": " + result.FaultString;
                return false;
            }

            Hashtable values = (Hashtable)result.Value;
            foreach (DictionaryEntry entry in values)
            {
                LoginValues[(string)entry.Key] = entry.Value;
            }

            if ((string)LoginValues["login"] == "indeterminate")
            {
                string nexturl = (string)LoginValues["next_url"];
                string nextmethod = (string)LoginValues["next_method"];
                string message = (string)LoginValues["message"];
                Client.Log("Login redirected: " + nexturl + ", message: " + message, Helpers.LogLevel.Info);

                return Login(loginParams, nexturl, nextmethod);
            }
            else if ((string)LoginValues["login"] == "false")
            {
                LoginError = LoginValues["reason"] + ": " + LoginValues["message"];
                return false;
            }
            else if ((string)LoginValues["login"] != "true")
            {
                LoginError = "Unknown error";
                return false;
            }

            System.Text.RegularExpressions.Regex LLSDtoJSON =
                new System.Text.RegularExpressions.Regex(@"('|r([0-9])|r(\-))");
            string json;
            Dictionary<string, object> jsonObject = null;
            LLVector3 vector = LLVector3.Zero;
            LLVector3 posVector = LLVector3.Zero;
            LLVector3 lookatVector = LLVector3.Zero;
            ulong regionHandle = 0;

            try
            {
                if (LoginValues.ContainsKey("look_at"))
                {
                    // Replace LLSD variables with object representations

                    // Convert LLSD string to JSON
                    json = "{vector:" + LLSDtoJSON.Replace((string)LoginValues["look_at"], "$2") + "}";

                    // Convert JSON string to a JSON object
                    jsonObject = JsonFacade.fromJSON(json);
                    JSONArray jsonVector = (JSONArray)jsonObject["vector"];

                    // Convert the JSON object to an LLVector3
                    vector = new LLVector3(Convert.ToSingle(jsonVector[0], CultureInfo.InvariantCulture),
                        Convert.ToSingle(jsonVector[1], CultureInfo.InvariantCulture), Convert.ToSingle(jsonVector[2], CultureInfo.InvariantCulture));

                    LoginValues["look_at"] = vector;
                }
            }
            catch (Exception e)
            {
                Client.Log(e.ToString(), Helpers.LogLevel.Warning);
                LoginValues["look_at"] = null;
            }

            try
            {
                if (LoginValues.ContainsKey("home"))
                {
                    Dictionary<string, object> home;

                    // Convert LLSD string to JSON
                    json = LLSDtoJSON.Replace((string)LoginValues["home"], "$2");

                    // Convert JSON string to an object
                    jsonObject = JsonFacade.fromJSON(json);

                    // Create the position vector
                    JSONArray array = (JSONArray)jsonObject["position"];
                    posVector = new LLVector3(Convert.ToSingle(array[0], CultureInfo.InvariantCulture), Convert.ToSingle(array[1], CultureInfo.InvariantCulture),
                        Convert.ToSingle(array[2], CultureInfo.InvariantCulture));

                    // Create the look_at vector
                    array = (JSONArray)jsonObject["look_at"];
                    lookatVector = new LLVector3(Convert.ToSingle(array[0], CultureInfo.InvariantCulture),
                        Convert.ToSingle(array[1], CultureInfo.InvariantCulture), Convert.ToSingle(array[2], CultureInfo.InvariantCulture));

                    // Create the regionhandle
                    array = (JSONArray)jsonObject["region_handle"];
                    regionHandle = Helpers.UIntsToLong((uint)(int)array[0], (uint)(int)array[1]);

                    Client.Self.Position = posVector;
                    Client.Self.LookAt = lookatVector;

                    // Create a dictionary to hold the home values
                    home = new Dictionary<string, object>();
                    home["position"] = posVector;
                    home["look_at"] = lookatVector;
                    home["region_handle"] = regionHandle;
                    LoginValues["home"] = home;
                }
            }
            catch (Exception e)
            {
                Client.Log(e.ToString(), Helpers.LogLevel.Warning);
                LoginValues["home"] = null;
            }

            try
            {
                this.AgentID = new LLUUID((string)LoginValues["agent_id"]);
                this.SessionID = new LLUUID((string)LoginValues["session_id"]);
                this.SecureSessionID = new LLUUID((string)LoginValues["secure_session_id"]);
                Client.Self.ID = this.AgentID;
                // Names are wrapped in quotes now, have to strip those
                Client.Self.FirstName = ((string)LoginValues["first_name"]).Trim(new char[] { '"' });
                Client.Self.LastName = ((string)LoginValues["last_name"]).Trim(new char[] { '"' });
                Client.Self.LookAt = vector;
                Client.Self.HomePosition = posVector;
                Client.Self.HomeLookAt = lookatVector;

                // Get Inventory Root Folder
                ArrayList alInventoryRoot = (ArrayList)LoginValues["inventory-root"];
                Hashtable htInventoryRoot = (Hashtable)alInventoryRoot[0];
                Client.Self.InventoryRootFolderUUID = new LLUUID((string)htInventoryRoot["folder_id"]);

                // Set the Circuit Code
                CircuitCode = (uint)(int)LoginValues["circuit_code"];

                // Connect to the sim given in the login reply
                if (Connect(IPAddress.Parse((string)LoginValues["sim_ip"]), (ushort)(int)LoginValues["sim_port"],
                    true, (string)LoginValues["seed_capability"]) == null)
                {
                    LoginError = "Unable to connect to the simulator";
                    return false;
                }

                // Send a couple packets that are useful right after login
                SendInitialPackets();

                // Fire an event for connecting to the grid
                if (OnConnected != null)
                {
                    try { OnConnected(this.Client); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }

                return true;
            }
            catch (Exception e)
            {
                Client.Log("Login error: " + e.ToString(), Helpers.LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// Connect to a simulator
        /// </summary>
        /// <param name="ip">IP address to connect to</param>
        /// <param name="port">Port to connect to</param>
        /// <param name="setDefault">Whether to set CurrentSim to this new
        /// connection, use this if the avatar is moving in to this simulator</param>
        /// <param name="seedcaps">URL of the capabilities server to use for
        /// this sim connection</param>
        /// <returns>A Simulator object on success, otherwise null</returns>
        public Simulator Connect(IPAddress ip, ushort port, bool setDefault, string seedcaps)
        {
            Simulator simulator = new Simulator(Client, ip, (int)port, setDefault);

            if (!simulator.Connected)
            {
                simulator = null;
                return null;
            }

            lock (Simulators) Simulators.Add(simulator);

            // Mark that we are connected to the grid (in case we weren't before)
            connected = true;

            // Start a timer that checks if we've been disconnected
            DisconnectTimer.Start();

            if (setDefault)
            {
                Simulator oldSim = CurrentSim;
                lock (Simulators) CurrentSim = simulator;

                if (CurrentCaps != null) CurrentCaps.Disconnect();
                CurrentCaps = null;

                if (seedcaps != null && seedcaps.Length > 0)
                    CurrentCaps = new Caps(Client, simulator, seedcaps, EventQueueCallbacks);

                if (OnCurrentSimChanged != null && simulator != oldSim)
                {
                    try { OnCurrentSimChanged(oldSim); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
            }

            return simulator;
        }

        /// <summary>
        /// Initiate a blocking logout request. This will return when the logout
        /// handshake has completed or when Settings.LOGOUT_TIMEOUT has expired
        /// and a LogoutDemand packet has been sent
        /// </summary>
        public void Logout()
        {
            LogoutReplyEvent.Reset();
            RequestLogout();
            LogoutReplyEvent.WaitOne(Client.Settings.LOGOUT_TIMEOUT, false);
        }

        /// <summary>
        /// Initiate the logout process (three step process!)
        /// </summary>
        public void RequestLogout()
        {
            // This will catch a Logout when the client is not logged in
            if (CurrentSim == null || !connected)
            {
                LogoutReplyEvent.Set();
                return;
            }

            Client.Log("Logging out", Helpers.LogLevel.Info);

            DisconnectTimer.Stop();

            // Send a logout request to the current sim
            LogoutRequestPacket logout = new LogoutRequestPacket();
            logout.AgentData.AgentID = AgentID;
            logout.AgentData.SessionID = SessionID;
            CurrentSim.SendPacket(logout, true);

            LogoutTimer = new System.Timers.Timer(Client.Settings.LOGOUT_TIMEOUT);
            LogoutTimer.AutoReset = false;
            LogoutTimer.Elapsed += new ElapsedEventHandler(LogoutTimer_Elapsed);
            LogoutTimer.Start();
        }

        /// <summary>
        /// Uses a LogoutDemand packet to force initiate a logout
        /// </summary>
        public void ForceLogout()
        {
            Client.Log("Forcing a logout", Helpers.LogLevel.Info);

            DisconnectTimer.Stop();

            // Insist on shutdown
            LogoutDemandPacket logoutDemand = new LogoutDemandPacket();
            logoutDemand.LogoutBlock.SessionID = SessionID;
            CurrentSim.SendPacket(logoutDemand, true);

            FinalizeLogout();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sim"></param>
        public void DisconnectSim(Simulator sim)
        {
            if (sim != null)
            {
                sim.Disconnect();

                // Fire the SimDisconnected event if a handler is registered
                if (OnSimDisconnected != null)
                {
                    try
                    {
                        OnSimDisconnected(sim, DisconnectType.NetworkTimeout);
                    }
                    catch (Exception e)
                    {
                        Client.Log("Caught an exception in OnSimDisconnected(): " + e.ToString(),
                            Helpers.LogLevel.Error);
                    }
                }

                lock (Simulators) Simulators.Remove(sim);
            }
            else
            {
                Client.Log("DisconnectSim() called with a null Simulator reference", Helpers.LogLevel.Warning);
            }
        }

        /// <summary>
        /// Finalize the logout procedure. Close down sockets, etc.
        /// </summary>
        private void FinalizeLogout()
        {
            LogoutTimer.Stop();

            // Shutdown the network layer
            Shutdown();

            if (OnDisconnected != null)
            {
                try
                {
                    OnDisconnected(DisconnectType.ClientInitiated, "");
                }
                catch (Exception e)
                {
                    Client.Log("Caught an exception in OnDisconnected(): " + e.ToString(),
                        Helpers.LogLevel.Error);
                }
            }

            // In case we are blocking in Logout()
            LogoutReplyEvent.Set();
        }

        /// <summary>
        /// Shutdown will disconnect all the sims except for the current sim
        /// first, and then kill the connection to CurrentSim.
        /// </summary>
        private void Shutdown()
        {
            Client.Log("NetworkManager shutdown initiated", Helpers.LogLevel.Info);

            lock (Simulators)
            {
                // Disconnect all simulators except the current one
                for (int i = 0; i < Simulators.Count; i++)
                {
                    if (Simulators[i] != null && Simulators[i] != CurrentSim)
                    {
                        DisconnectSim(Simulators[i]);

                        // Fire the SimDisconnected event if a handler is registered
                        if (OnSimDisconnected != null)
                        {
                            try { OnSimDisconnected(Simulators[i], DisconnectType.NetworkTimeout); }
                            catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                        }
                    }
                }

                Simulators.Clear();
            }

            if (CurrentSim != null)
            {
                Simulator oldSim = CurrentSim;

                DisconnectSim(CurrentSim);
                lock (Simulators) CurrentSim = null;

                if (OnCurrentSimChanged != null)
                {
                    try { OnCurrentSimChanged(oldSim); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
            }

            if (CurrentCaps != null)
            {
                CurrentCaps.Disconnect();
                CurrentCaps = null;
            }

            connected = false;
        }

        private void SendInitialPackets()
        {
            // Request the economy data
            SendPacket(new EconomyDataRequestPacket());

            // Send an AgentUpdate packet to truly move our avatar in to the sim
            MainAvatar.AgentUpdateFlags controlFlags = MainAvatar.AgentUpdateFlags.AGENT_CONTROL_FINISH_ANIM;
            LLVector3 position = new LLVector3(128, 128, 32);
            LLVector3 forwardAxis = new LLVector3(0, 0.999999f, 0);
            LLVector3 leftAxis = new LLVector3(0.999999f, 0, 0);
            LLVector3 upAxis = new LLVector3(0, 0, 0.999999f);
            Client.Self.UpdateCamera(controlFlags, position, forwardAxis, leftAxis, upAxis, LLQuaternion.Identity,
                LLQuaternion.Identity, 384.0f, true);
        }

        /// <summary>
        /// Triggered if a LogoutReply is not received
        /// </summary>
        private void LogoutTimer_Elapsed(object sender, ElapsedEventArgs ev)
        {
            LogoutTimer.Stop();
            Client.Log("Logout due to timeout on server acknowledgement", Helpers.LogLevel.Debug);
            ForceLogout();
        }

        private void DisconnectTimer_Elapsed(object sender, ElapsedEventArgs ev)
        {
            if (connected)
            {
                if (CurrentSim == null)
                {
                    DisconnectTimer.Stop();
                    connected = false;
                    return;
                }

                // If the current simulator is disconnected, shutdown+callback+return
                if (CurrentSim.DisconnectCandidate)
                {
                    Client.Log("Network timeout for the current simulator (" +
                        CurrentSim.ToString() + "), logging out", Helpers.LogLevel.Warning);

                    DisconnectTimer.Stop();
                    connected = false;

                    // Shutdown the network layer
                    Shutdown();

                    if (OnDisconnected != null)
                    {
                        try
                        {
                            OnDisconnected(DisconnectType.NetworkTimeout, "");
                        }
                        catch (Exception e)
                        {
                            Client.Log("Caught an exception in OnDisconnected(): " + e.ToString(),
                                Helpers.LogLevel.Error);
                        }
                    }

                    // We're completely logged out and shut down, leave this function
                    return;
                }

                List<Simulator> disconnectedSims = null;

                // Check all of the connected sims for disconnects
                lock (Simulators)
                {
                    for (int i = 0; i < Simulators.Count; i++)
                    {
                        if (Simulators[i].DisconnectCandidate)
                        {
                            if (disconnectedSims == null)
                                disconnectedSims = new List<Simulator>();

                            disconnectedSims.Add(Simulators[i]);
                        }
                        else
                        {
                            Simulators[i].DisconnectCandidate = true;
                        }
                    }
                }

                // Actually disconnect each sim we detected as disconnected
                if (disconnectedSims != null)
                {
                    for (int i = 0; i < disconnectedSims.Count; i++)
                    {
                        if (disconnectedSims[i] != null)
                        {
                            // This sim hasn't received any network traffic since the 
                            // timer last elapsed, consider it disconnected
                            Client.Log("Network timeout for simulator " + disconnectedSims[i].ToString() +
                                ", disconnecting", Helpers.LogLevel.Warning);

                            DisconnectSim(disconnectedSims[i]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called to deal with LogoutReply packet and fires off callback
        /// </summary>
        /// <param name="packet">Full packet of type LogoutReplyPacket</param>
        /// <param name="simulator"></param>
        private void LogoutReplyHandler(Packet packet, Simulator simulator)
        {
            LogoutReplyPacket logout = (LogoutReplyPacket)packet;

            if ((logout.AgentData.SessionID == SessionID) && (logout.AgentData.AgentID == AgentID))
            {
                Client.Log("Logout negotiated with server", Helpers.LogLevel.Debug);

                // Deal with callbacks, if any
                if (OnLogoutReply != null)
                {
                    List<LLUUID> itemIDs = new List<LLUUID>();

                    foreach (LogoutReplyPacket.InventoryDataBlock InventoryData in logout.InventoryData)
                    {
                        itemIDs.Add(InventoryData.ItemID);
                    }

                    try { OnLogoutReply(itemIDs); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }

                FinalizeLogout();
            }
            else
            {
                Client.Log("Invalid Session or Agent ID received in Logout Reply... ignoring", Helpers.LogLevel.Warning);
            }
        }

        private void StartPingCheckHandler(Packet packet, Simulator simulator)
        {
            StartPingCheckPacket incomingPing = (StartPingCheckPacket)packet;
            CompletePingCheckPacket ping = new CompletePingCheckPacket();
            ping.PingID.PingID = incomingPing.PingID.PingID;

            // TODO: We can use OldestUnacked to correct transmission errors

            SendPacket(ping, simulator);
        }

        private void RegionHandshakeHandler(Packet packet, Simulator simulator)
        {
            RegionHandshakePacket handshake = (RegionHandshakePacket)packet;

            simulator.ID = handshake.RegionInfo.CacheID;

            // TODO: What do we need these for? RegionFlags probably contains good stuff
            //handshake.RegionInfo.BillableFactor;
            //handshake.RegionInfo.RegionFlags;
            //handshake.RegionInfo.SimAccess;

            simulator.IsEstateManager = handshake.RegionInfo.IsEstateManager;
            simulator.Name = Helpers.FieldToString(handshake.RegionInfo.SimName);
            simulator.SimOwner = handshake.RegionInfo.SimOwner;
            simulator.TerrainBase0 = handshake.RegionInfo.TerrainBase0;
            simulator.TerrainBase1 = handshake.RegionInfo.TerrainBase1;
            simulator.TerrainBase2 = handshake.RegionInfo.TerrainBase2;
            simulator.TerrainBase3 = handshake.RegionInfo.TerrainBase3;
            simulator.TerrainDetail0 = handshake.RegionInfo.TerrainDetail0;
            simulator.TerrainDetail1 = handshake.RegionInfo.TerrainDetail1;
            simulator.TerrainDetail2 = handshake.RegionInfo.TerrainDetail2;
            simulator.TerrainDetail3 = handshake.RegionInfo.TerrainDetail3;
            simulator.TerrainHeightRange00 = handshake.RegionInfo.TerrainHeightRange00;
            simulator.TerrainHeightRange01 = handshake.RegionInfo.TerrainHeightRange01;
            simulator.TerrainHeightRange10 = handshake.RegionInfo.TerrainHeightRange10;
            simulator.TerrainHeightRange11 = handshake.RegionInfo.TerrainHeightRange11;
            simulator.TerrainStartHeight00 = handshake.RegionInfo.TerrainStartHeight00;
            simulator.TerrainStartHeight01 = handshake.RegionInfo.TerrainStartHeight01;
            simulator.TerrainStartHeight10 = handshake.RegionInfo.TerrainStartHeight10;
            simulator.TerrainStartHeight11 = handshake.RegionInfo.TerrainStartHeight11;
            simulator.WaterHeight = handshake.RegionInfo.WaterHeight;

            Client.Log("Received a region handshake for " + simulator.ToString(), Helpers.LogLevel.Info);

            // Send a RegionHandshakeReply
            RegionHandshakeReplyPacket reply = new RegionHandshakeReplyPacket();
            reply.AgentData.AgentID = AgentID;
            reply.AgentData.SessionID = SessionID;
            reply.RegionInfo.Flags = 0;
            SendPacket(reply, simulator);

            // We're officially connected to this sim
            simulator.connected = true;
            simulator.ConnectedEvent.Set();
        }

        private void ParcelOverlayHandler(Packet packet, Simulator simulator)
        {
            ParcelOverlayPacket overlay = (ParcelOverlayPacket)packet;

            if (overlay.ParcelData.SequenceID >= 0 && overlay.ParcelData.SequenceID <= 3)
            {
                Array.Copy(overlay.ParcelData.Data, 0, simulator.ParcelOverlay,
                    overlay.ParcelData.SequenceID * 1024, 1024);
                simulator.ParcelOverlaysReceived++;

                if (simulator.ParcelOverlaysReceived > 3)
                {
                    // TODO: ParcelOverlaysReceived should become internal, and reset to zero every 
                    // time it hits four. Also need a callback here
                }
            }
            else
            {
                Client.Log("Parcel overlay with sequence ID of " + overlay.ParcelData.SequenceID +
                    " received from " + simulator.ToString(), Helpers.LogLevel.Warning);
            }
        }

        private void EnableSimulatorHandler(Packet packet, Simulator simulator)
        {
            // TODO: Actually connect to the simulator

            // TODO: Sending ConfirmEnableSimulator completely screws things up. :-?

            // Respond to the simulator connection request
            //Packet replyPacket = Packets.Network.ConfirmEnableSimulator(Protocol, AgentID, SessionID);
            //SendPacket(replyPacket, circuit);
        }

        private void KickUserHandler(Packet packet, Simulator simulator)
        {
            string message = Helpers.FieldToString(((KickUserPacket)packet).UserInfo.Reason);

            // Shutdown the network layer
            Shutdown();

            if (OnDisconnected != null)
            {
                try
                {
                    OnDisconnected(DisconnectType.ServerInitiated, message);
                }
                catch (Exception e)
                {
                    Client.Log("Caught an exception in OnDisconnected(): " + e.ToString(),
                        Helpers.LogLevel.Error);
                }
            }
        }
    }

    /// <summary>
    /// Throttles the network traffic for various different traffic types.
    /// Access this class through SecondLife.Throttle
    /// </summary>
    public class AgentThrottle
    {
        /// <summary>Maximum bytes per second for resending unacknowledged packets</summary>
        public float Resend
        {
            get { return resend; }
            set
            {
                if (value > 150000.0f) resend = 150000.0f;
                else if (value < 10000.0f) resend = 10000.0f;
                else resend = value;
            }
        }
        /// <summary>Maximum bytes per second for LayerData terrain</summary>
        public float Land
        {
            get { return land; }
            set
            {
                if (value > 170000.0f) land = 170000.0f;
                else if (value < 0.0f) land = 0.0f; // We don't have control of these so allow throttling to 0
                else land = value;
            }
        }
        /// <summary>Maximum bytes per second for LayerData wind data</summary>
        public float Wind
        {
            get { return wind; }
            set
            {
                if (value > 34000.0f) wind = 34000.0f;
                else if (value < 0.0f) wind = 0.0f; // We don't have control of these so allow throttling to 0
                else wind = value;
            }
        }
        /// <summary>Maximum bytes per second for LayerData clouds</summary>
        public float Cloud
        {
            get { return cloud; }
            set
            {
                if (value > 34000.0f) cloud = 34000.0f;
                else if (value < 0.0f) cloud = 0.0f; // We don't have control of these so allow throttling to 0
                else cloud = value;
            }
        }
        /// <summary>Unknown, includes object data</summary>
        public float Task
        {
            get { return task; }
            set
            {
                if (value > 446000.0f) task = 446000.0f;
                else if (value < 4000.0f) task = 4000.0f;
                else task = value;
            }
        }
        /// <summary>Maximum bytes per second for textures</summary>
        public float Texture
        {
            get { return texture; }
            set
            {
                if (value > 446000.0f) texture = 446000.0f;
                else if (value < 4000.0f) texture = 4000.0f;
                else texture = value;
            }
        }
        /// <summary>Maximum bytes per second for downloaded assets</summary>
        public float Asset
        {
            get { return asset; }
            set
            {
                if (value > 220000.0f) asset = 220000.0f;
                else if (value < 10000.0f) asset = 10000.0f;
                else asset = value;
            }
        }

        /// <summary>Maximum bytes per second the entire connection, divided up
        /// between invidiual streams using default multipliers</summary>
        public float Total
        {
            get { return Resend + Land + Wind + Cloud + Task + Texture + Asset; }
            set
            {
                // These sane initial values were pulled from the Second Life client
                Resend = (value * 0.1f);
                Land = (float)(value * 0.52f / 3f);
                Wind = (float)(value * 0.05f);
                Cloud = (float)(value * 0.05f);
                Task = (float)(value * 0.704f / 3f);
                Texture = (float)(value * 0.704f / 3f);
                Asset = (float)(value * 0.484f / 3f);
            }
        }

        private SecondLife Client;
        private float resend;
        private float land;
        private float wind;
        private float cloud;
        private float task;
        private float texture;
        private float asset;

        /// <summary>
        /// Default constructor, uses a default high total of 1500 KBps (1536000)
        /// </summary>
        public AgentThrottle(SecondLife client)
        {
            Client = client;
            Total = 1536000.0f;
        }

        /// <summary>
        /// Constructor that decodes an existing AgentThrottle packet in to
        /// individual values
        /// </summary>
        /// <param name="data">Reference to the throttle data in an AgentThrottle
        /// packet</param>
        /// <param name="pos">Offset position to start reading at in the 
        /// throttle data</param>
        /// <remarks>This is generally not needed in libsecondlife clients as 
        /// the server will never send a throttle packet to the client</remarks>
        public AgentThrottle(byte[] data, int pos)
        {
            int i;
            if (!BitConverter.IsLittleEndian)
                for (i = 0; i < 7; i++)
                    Array.Reverse(data, pos + i * 4, 4);

            Resend = BitConverter.ToSingle(data, pos); pos += 4;
            Land = BitConverter.ToSingle(data, pos); pos += 4;
            Wind = BitConverter.ToSingle(data, pos); pos += 4;
            Cloud = BitConverter.ToSingle(data, pos); pos += 4;
            Task = BitConverter.ToSingle(data, pos); pos += 4;
            Texture = BitConverter.ToSingle(data, pos); pos += 4;
            Asset = BitConverter.ToSingle(data, pos);
        }

        /// <summary>
        /// Send an AgentThrottle packet to the server using the current values
        /// </summary>
        public void Set()
        {
            AgentThrottlePacket throttle = new AgentThrottlePacket();
            throttle.AgentData.AgentID = Client.Network.AgentID;
            throttle.AgentData.SessionID = Client.Network.SessionID;
            throttle.AgentData.CircuitCode = Client.Network.CircuitCode;
            throttle.Throttle.GenCounter = 0;
            throttle.Throttle.Throttles = this.ToBytes();

            Client.Network.SendPacket(throttle);
        }

        /// <summary>
        /// Convert the current throttle values to a byte array that can be put
        /// in an AgentThrottle packet
        /// </summary>
        /// <returns>Byte array containing all the throttle values</returns>
        public byte[] ToBytes()
        {
            byte[] data = new byte[7 * 4];
            int i = 0;

            BitConverter.GetBytes(Resend).CopyTo(data, i); i += 4;
            BitConverter.GetBytes(Land).CopyTo(data, i); i += 4;
            BitConverter.GetBytes(Wind).CopyTo(data, i); i += 4;
            BitConverter.GetBytes(Cloud).CopyTo(data, i); i += 4;
            BitConverter.GetBytes(Task).CopyTo(data, i); i += 4;
            BitConverter.GetBytes(Texture).CopyTo(data, i); i += 4;
            BitConverter.GetBytes(Asset).CopyTo(data, i); i += 4;

            if (!BitConverter.IsLittleEndian)
                for (i = 0; i < 7; i++)
                    Array.Reverse(data, i * 4, 4);

            return data;
        }
    }
}
