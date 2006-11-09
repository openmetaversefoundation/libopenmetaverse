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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
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
    /// Simulator is a wrapper for a network connection to a simulator and the
    /// Region class representing the block of land in the metaverse.
    /// </summary>
	public class Simulator
	{
        /// <summary>The maximum size of the sequence number inbox, used to 
        /// check for resent and/or duplicate packets</summary>
        public const int INBOX_SIZE = 200;

        /// <summary>The Region class that this Simulator wraps</summary>
        public Region Region;

        /// <summary>
        /// The ID number associated with this particular connection to the 
        /// simulator, used to emulate TCP connections. This is used 
        /// internally for packets that have a CircuitCode field.
        /// </summary>
        public uint CircuitCode
        {
            get { return circuitCode; }
            set { circuitCode = value; }
        }

        /// <summary>
        /// The IP address and port of the server.
        /// </summary>
        public IPEndPoint IPEndPoint
        {
            get { return ipEndPoint; }
        }

        /// <summary>
        /// A boolean representing whether there is a working connection to the
        /// simulator or not.
        /// </summary>
        public bool Connected
        {
            get { return connected; }
        }

        /// <summary>
        /// Used internally to track sim disconnections, do not modify this 
        /// variable.
        /// </summary>
        public bool DisconnectCandidate;
        
        private SecondLife Client;
		private NetworkManager Network;
		private Dictionary<PacketType,List<NetworkManager.PacketCallback>> Callbacks;
		private ushort Sequence;
		private byte[] RecvBuffer;
		private Socket Connection;
		private AsyncCallback ReceivedData;
		private Dictionary<int, Packet> NeedAck;
        private Queue<ushort> Inbox;
        private List<uint> PendingAcks;
		private bool connected;
		private uint circuitCode;
		private IPEndPoint ipEndPoint;
		private EndPoint endPoint;
        private System.Timers.Timer AckTimer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="callbacks"></param>
        /// <param name="circuit"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public Simulator(SecondLife client, Dictionary<PacketType, List<NetworkManager.PacketCallback>> callbacks, 
            uint circuit, IPAddress ip, int port)
		{
            Client = client;
            Network = client.Network;
            Callbacks = callbacks;
            Region = new Region(client);
            circuitCode = circuit;
            Sequence = 0;
            RecvBuffer = new byte[2048];
            Connection = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            connected = false;
            DisconnectCandidate = false;
            AckTimer = new System.Timers.Timer(500);
            AckTimer.Elapsed += new ElapsedEventHandler(AckTimer_Elapsed);

            // Initialize the dictionary for reliable packets waiting on ACKs from the server
            NeedAck = new Dictionary<int, Packet>();

            // Initialize the lists of sequence numbers we've received so far
            Inbox = new Queue<ushort>(INBOX_SIZE);
            PendingAcks = new List<uint>();

            Client.Log("Connecting to " + ip.ToString() + ":" + port, Helpers.LogLevel.Info);

            try
            {
                // Setup the callback
                ReceivedData = new AsyncCallback(OnReceivedData);

                // Create an endpoint that we will be communicating with (need it in two 
                // types due to .NET weirdness)
                ipEndPoint = new IPEndPoint(ip, port);
                endPoint = (EndPoint)ipEndPoint;

                // Associate this simulator's socket with the given ip/port and start listening
                Connection.Connect(endPoint);
                Connection.BeginReceiveFrom(RecvBuffer, 0, RecvBuffer.Length, SocketFlags.None, ref endPoint, ReceivedData, null);

                // Send the UseCircuitCode packet to initiate the connection
                UseCircuitCodePacket use = new UseCircuitCodePacket();
                use.CircuitCode.Code = circuitCode;
                use.CircuitCode.ID = Network.AgentID;
                use.CircuitCode.SessionID = Network.SessionID;

                // Start the ACK timer
                AckTimer.Start();

                // Send the initial packet out
                SendPacket(use, true);

                // Track the current time for timeout purposes
                int start = Environment.TickCount;

                while (true)
                {
                    if (connected || Environment.TickCount - start > 8000)
                    {
                        return;
                    }
                    System.Threading.Thread.Sleep(10);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Client.Log(e.ToString(), Helpers.LogLevel.Error);
            }
		}

        /// <summary>
        /// 
        /// </summary>
		public void Disconnect()
		{
			// Send the CloseCircuit notice
            CloseCircuitPacket close = new CloseCircuitPacket();
			
            try
            {
                Connection.Send(close.ToBytes());
			}
			catch (SocketException)
			{
				// There's a high probability of this failing if the network is
                // disconnected, so don't even bother logging the error
			}

            try
            {
                // Shut the socket communication down
                Connection.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException e)
            {
                Client.Log(e.ToString(), Helpers.LogLevel.Error);
            }

            connected = false;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="incrementSequence"></param>
		public void SendPacket(Packet packet, bool incrementSequence)
		{
			byte[] buffer;
			int bytes;

			if (!connected && packet.Type != PacketType.UseCircuitCode)
			{
				Client.Log("Trying to send a " + packet.Type.ToString() + " packet when the socket is closed",
					Helpers.LogLevel.Warning);
				
				throw new NotConnectedException();
			}

            if (incrementSequence)
            {
                // Set the sequence number here since we are manually serializing the packet
                packet.Header.Sequence = ++Sequence;

                if (packet.Header.Reliable)
                {
                    // Keep track of when this packet was sent out
                    packet.TickCount = Environment.TickCount;

                    lock (NeedAck)
                    {
                        if (!NeedAck.ContainsKey(packet.Header.Sequence))
                        {
                            NeedAck.Add(packet.Header.Sequence, packet);
                        }
                        else
                        {
                            Client.Log("Attempted to add a duplicate sequence number (" +
                                packet.Header.Sequence + ") to the NeedAck dictionary for packet type " +
                                packet.Type.ToString(), Helpers.LogLevel.Warning);
                        }
                    }

                    // Append any ACKs that need to be sent out to this packet
                    lock (PendingAcks)
                    {
                        if (PendingAcks.Count > 0 && packet.Type != PacketType.PacketAck &&
                            packet.Type != PacketType.LogoutRequest)
                        {
                            packet.Header.AckList = new uint[PendingAcks.Count];

                            int i = 0;

                            foreach (uint ack in PendingAcks)
                            {
                                packet.Header.AckList[i] = ack;
                                i++;
                            }

                            PendingAcks.Clear();
                            packet.Header.AppendedAcks = true;
                        }
                    }
                }
            }

            // Serialize the packet
            buffer = packet.ToBytes();
            bytes = buffer.Length;

            // Zerocode if needed
            if (packet.Header.Zerocoded)
            {
                byte[] zeroBuffer = new byte[4096];
                bytes = Helpers.ZeroEncode(buffer, bytes, zeroBuffer);

                buffer = zeroBuffer;
            }

            try
            {
                Connection.Send(buffer, bytes, SocketFlags.None);
            }
            catch (SocketException e)
            {
                Client.Log(e.ToString(), Helpers.LogLevel.Error);
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        public void SendPacket(byte[] payload)
        {
            if (!connected)
            {
                throw new NotConnectedException();
            }

            try
            {
                Connection.Send(payload, payload.Length, SocketFlags.None);
            }
            catch (SocketException e)
            {
                Client.Log(e.ToString(), Helpers.LogLevel.Error);
            }
        }

        private void SendAck(ushort id)
        {
            PacketAckPacket ack = new PacketAckPacket();

            ack.Packets = new PacketAckPacket.PacketsBlock[1];
            ack.Packets[0] = new PacketAckPacket.PacketsBlock();
            ack.Packets[0].ID = id;
            ack.Header.Reliable = false;

            lock (PendingAcks)
            {
                if (PendingAcks.Contains(id))
                {
                    PendingAcks.Remove(id);
                }
            }

            SendPacket(ack, true);
        }

        private void SendAcks()
        {
            lock (PendingAcks)
            {
                if (PendingAcks.Count > 0)
                {
                    int i = 0;
                    PacketAckPacket acks = new PacketAckPacket();
                    acks.Packets = new PacketAckPacket.PacketsBlock[PendingAcks.Count];

                    foreach (uint ack in PendingAcks)
                    {
                        acks.Packets[i] = new PacketAckPacket.PacketsBlock();
                        acks.Packets[i].ID = ack;
                        i++;
                    }

                    acks.Header.Reliable = false;

                    SendPacket(acks, true);

                    PendingAcks.Clear();
                }
            }
        }

		private void OnReceivedData(IAsyncResult result)
		{
            Packet packet = null;
            int numBytes;

            // If we're receiving data the sim connection is open
            connected = true;

            // Update the disconnect flag so this sim doesn't time out
            DisconnectCandidate = false;

            lock (RecvBuffer)
            {
                // Retrieve the incoming packet
                try
                {
                    numBytes = Connection.EndReceiveFrom(result, ref endPoint);

                    int packetEnd = numBytes - 1;
                    packet = Packet.BuildPacket(RecvBuffer, ref packetEnd);

                    Connection.BeginReceiveFrom(RecvBuffer, 0, RecvBuffer.Length, SocketFlags.None, ref endPoint, ReceivedData, null);
                }
                catch (SocketException)
                {
                    Client.Log(endPoint.ToString() + " socket is closed, shutting down " + this.Region.Name, 
                        Helpers.LogLevel.Info);

                    connected = false;
                    Network.DisconnectSim(this);
                    return;
                }
            }

            // Fail-safe check
            if (packet == null)
            {
                Client.Log("Couldn't build a message from the incoming data", Helpers.LogLevel.Warning);
                return;
            }

            // Track the sequence number for this packet if it's marked as reliable
            if (packet.Header.Reliable)
            {
                if (PendingAcks.Count > 10)
                {
                    SendAcks();
                }

                // Check if we already received this packet
                lock (Inbox)
                {
                    if (Inbox.Contains(packet.Header.Sequence))
                    {
                        Client.Log("Received a duplicate " + packet.Type.ToString() + ", sequence=" +
                            packet.Header.Sequence + ", resent=" + ((packet.Header.Resent) ? "Yes" : "No"),
                            Helpers.LogLevel.Info);

                        // Send an ACK for this packet immediately
                        SendAck(packet.Header.Sequence);

                        // Avoid firing a callback twice for the same packet
                        return;
                    }
                    else
                    {
                        if (Inbox.Count >= INBOX_SIZE)
                        {
                            Inbox.Dequeue();
                        }

                        Inbox.Enqueue(packet.Header.Sequence);

                        lock (PendingAcks)
                        {
                            PendingAcks.Add((uint)packet.Header.Sequence);
                        }
                    }
                }
            }

            // Handle appended ACKs
            if (packet.Header.AppendedAcks)
            {
                lock (NeedAck)
                {
                    foreach (ushort ack in packet.Header.AckList)
                    {
                        if (NeedAck.ContainsKey(ack))
                        {
                            NeedAck.Remove(ack);
                        }
                        else
                        {
                            Client.Log("Appended ACK for a packet we didn't send: " + ack, Helpers.LogLevel.Warning);
                        }
                    }
                }
            }

            // Handle PacketAck packets
            if (packet.Type == PacketType.PacketAck)
            {
                lock (NeedAck)
                {
                    PacketAckPacket ackPacket = (PacketAckPacket)packet;

                    foreach (PacketAckPacket.PacketsBlock block in ackPacket.Packets)
                    {
                        NeedAck.Remove((ushort)block.ID);
                    }
                }
            }

            // Fire the registered packet events
            #region FireCallbacks
            try
            {
                if (Callbacks.ContainsKey(packet.Type))
                {
                    List<NetworkManager.PacketCallback> callbackArray = Callbacks[packet.Type];

                    // Fire any registered callbacks
                    foreach (NetworkManager.PacketCallback callback in callbackArray)
                    {
                        if (callback != null)
                        {
                            callback(packet, this);
                        }
                    }
                }
                
                if (Callbacks.ContainsKey(PacketType.Default))
                {
                    List<NetworkManager.PacketCallback> callbackArray = Callbacks[PacketType.Default];

                    // Fire any registered callbacks
                    foreach (NetworkManager.PacketCallback callback in callbackArray)
                    {
                        if (callback != null)
                        {
                            callback(packet, this);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Client.Log("Caught an exception in a packet callback: " + e.ToString(), Helpers.LogLevel.Warning);
            }
            #endregion FireCallbacks
        }

        private void AckTimer_Elapsed(object sender, ElapsedEventArgs ea)
        {
            if (!connected)
            {
                AckTimer.Stop();
                return;
            }

            SendAcks();
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
        /// The permanent UUID for the logged in avatar
        /// </summary>
		public LLUUID AgentID;
        /// <summary>
        /// A temporary UUID assigned to this session, used for secure 
        /// transactions
        /// </summary>
		public LLUUID SessionID;
        /// <summary>
        /// A string holding a descriptive error on login failure, empty
        /// otherwise
        /// </summary>
		public string LoginError;
        /// <summary>
        /// The simulator that the logged in avatar is currently occupying
        /// </summary>
		public Simulator CurrentSim;
        /// <summary>
        /// The complete dictionary of all the login values returned by the 
        /// RPC login server, converted to native data types wherever possible
        /// </summary>
		public Dictionary<string, object> LoginValues;
        /// <summary>
        /// Shows whether the network layer is logged in to the grid or not
        /// </summary>
        public bool Connected
        {
            get { return connected; }
        }
        /// <summary>
        /// An event for the connection to a simulator other than the currently
        /// occupied one disconnecting
        /// </summary>
        public SimDisconnectCallback OnSimDisconnected;
        /// <summary>
        /// An event for being logged out either through client request, server
        /// forced, or network error
        /// </summary>
        public DisconnectCallback OnDisconnected;

		private Dictionary<PacketType,List<PacketCallback>> Callbacks;
		private SecondLife Client;
		private List<Simulator> Simulators;
        private System.Timers.Timer DisconnectTimer;
        private bool connected;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
		public NetworkManager(SecondLife client)
		{
			Client = client;
			Simulators = new List<Simulator>();
			Callbacks = new Dictionary<PacketType, List<PacketCallback>>();
			CurrentSim = null;
			LoginValues = null;

			// Register the internal callbacks
			RegisterCallback(PacketType.RegionHandshake, new PacketCallback(RegionHandshakeHandler));
			RegisterCallback(PacketType.StartPingCheck, new PacketCallback(StartPingCheckHandler));
			RegisterCallback(PacketType.ParcelOverlay, new PacketCallback(ParcelOverlayHandler));
			RegisterCallback(PacketType.EnableSimulator, new PacketCallback(EnableSimulatorHandler));
            RegisterCallback(PacketType.KickUser, new PacketCallback(KickUserHandler));

            // Disconnect a sim if no network traffic has been received for 15 seconds
            DisconnectTimer = new System.Timers.Timer(15000);
            DisconnectTimer.Elapsed += new ElapsedEventHandler(DisconnectTimer_Elapsed);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="callback"></param>
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
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="callback"></param>
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
        /// 
        /// </summary>
        /// <param name="packet"></param>
		public void SendPacket(Packet packet)
		{
			if (CurrentSim != null && CurrentSim.Connected)
			{
				CurrentSim.SendPacket(packet, true);
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
		public void SendPacket(Packet packet, Simulator simulator)
		{
            if (simulator.Connected)
            {
                simulator.SendPacket(packet, true);
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        public void SendPacket(byte[] payload)
        {
            if (CurrentSim != null)
            {
                CurrentSim.SendPacket(payload);
            }
            else
            {
                throw new NotConnectedException();
            }
        }
        /// <summary>
        /// Use this if you want to login to a specific location
        /// </summary>
        /// <param name="sim"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>string with a value that can be used in the start field in .DefaultLoginValues()</returns>

        public static string StartLocation(string sim, int x, int y, int z)
        {
            //uri:sim&x&y&z
            return "uri:" + sim.ToLower() + "&" + x + "&" + y + "&" + z;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="password"></param>
        /// <param name="userAgent"></param>
        /// <param name="author"></param>
        /// <returns></returns>
        public static Dictionary<string, object> DefaultLoginValues(
            string firstName, string lastName, string password, string userAgent, string author)
		{
			return DefaultLoginValues(firstName, lastName, password, "00:00:00:00:00:00", "last", 
				1, 50, 50, 50, "Win", "0", userAgent, author, false);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="password"></param>
        /// <param name="userAgent"></param>
        /// <param name="author"></param>
        /// <returns></returns>
        public static Dictionary<string, object> DefaultLoginValues(
            string firstName, string lastName, string password, string startLocation, string userAgent, string author, 
            bool md5pass)
        {
            return DefaultLoginValues(firstName, lastName, password, "00:00:00:00:00:00", startLocation,
                1, 50, 50, 50, "Win", "0", userAgent, author, md5pass);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="password"></param>
        /// <param name="mac"></param>
        /// <param name="startLocation"></param>
        /// <param name="platform"></param>
        /// <param name="viewerDigest"></param>
        /// <param name="userAgent"></param>
        /// <param name="author"></param>
        /// <returns></returns>
        public static Dictionary<string, object> DefaultLoginValues(string firstName, 
            string lastName, string password, string mac, string startLocation, string platform, 
            string viewerDigest, string userAgent, string author)
        {
            return DefaultLoginValues(firstName, lastName, password, mac, startLocation,
                1, 50, 50, 50, platform, viewerDigest, userAgent, author, false);
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
        /// <returns></returns>
        public static Dictionary<string, object> DefaultLoginValues(string firstName, 
            string lastName, string password, string mac, string startLocation, int major, int minor, 
            int patch, int build, string platform, string viewerDigest, string userAgent, string author, bool md5pass)
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
			values["user-agent"] = userAgent + " (" + Helpers.VERSION + ")";
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
        /// 
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="password"></param>
        /// <param name="userAgent"></param>
        /// <param name="author"></param>
        /// <returns></returns>
        public bool Login(string firstName, string lastName, string password, string userAgent, string author)
        {
            Dictionary<string, object> loginParams = NetworkManager.DefaultLoginValues(firstName, lastName, 
                password, "last", userAgent, author, false);
            return Login(loginParams);
        }

        public bool Login(string firstName, string lastName, string password, string userAgent, string start, 
            string author, bool md5pass)
        {
            Dictionary<string, object> loginParams = NetworkManager.DefaultLoginValues(firstName, lastName,
                password, userAgent, start, author, md5pass);
            return Login(loginParams);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginParams"></param>
        /// <returns></returns>
        public bool Login(Dictionary<string, object> loginParams)
		{
            return Login(loginParams, SecondLife.LOGIN_SERVER);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginParams"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool Login(Dictionary<string, object> loginParams, string url)
		{
			XmlRpcResponse result;
			XmlRpcRequest xmlrpc = new XmlRpcRequest();
			xmlrpc.MethodName = "login_to_simulator";
			xmlrpc.Params.Clear();
			xmlrpc.Params.Add(loginParams);

			try
			{
				result = (XmlRpcResponse)xmlrpc.Send(url);
			}
			catch (Exception e)
			{
				LoginError = "XML-RPC Error: " + e.Message;
				LoginValues = null;
				return false;
			}

			if (result.IsFault)
			{
				Client.Log("Fault " + result.FaultCode + ": " + result.FaultString, Helpers.LogLevel.Error);
				LoginError = "XML-RPC Fault: " + result.FaultCode + ": " + result.FaultString;
				LoginValues = null;
				return false;
			}

			LoginValues = (Dictionary<string, object>)result.Value;

            if ((string)LoginValues["login"] == "indeterminate")
            {
                //FIXME: We need to do another XML-RPC, handle this case
                LoginError = "Got a redirect, login with the official client to update";
                return false;
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
			LLVector3 vector = null;
			LLVector3 posVector = null;
			LLVector3 lookatVector = null;
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
                    vector = new LLVector3(Convert.ToSingle(jsonVector[0]),
                        Convert.ToSingle(jsonVector[1]), Convert.ToSingle(jsonVector[2]));

                    LoginValues["look_at"] = vector;
                }

                if (LoginValues.ContainsKey("home"))
                {
                    Dictionary<string, object> home;

                    // Convert LLSD string to JSON
                    json = LLSDtoJSON.Replace((string)LoginValues["home"], "$2");

                    // Convert JSON string to an object
                    jsonObject = JsonFacade.fromJSON(json);

                    // Create the position vector
                    JSONArray array = (JSONArray)jsonObject["position"];
                    posVector = new LLVector3(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]),
                        Convert.ToSingle(array[2]));

                    // Create the look_at vector
                    array = (JSONArray)jsonObject["look_at"];
                    lookatVector = new LLVector3(Convert.ToSingle(array[0]),
                        Convert.ToSingle(array[1]), Convert.ToSingle(array[2]));

                    // Create the regionhandle
                    array = (JSONArray)jsonObject["region_handle"];
                    regionHandle = Helpers.UIntsToLong((uint)(int)array[0], (uint)(int)array[1]);

                    Client.Self.Position = posVector;
                    Client.Self.LookAt = lookatVector;

                    // Create a dictionary to hold the home values
                    home = new Dictionary<string,object>();
                    home["position"] = posVector;
                    home["look_at"] = lookatVector;
                    home["region_handle"] = regionHandle;
                    LoginValues["home"] = home;
                }

                this.AgentID = new LLUUID((string)LoginValues["agent_id"]);
                this.SessionID = new LLUUID((string)LoginValues["session_id"]);
                Client.Self.ID = this.AgentID;
                Client.Self.FirstName = (string)LoginValues["first_name"];
                Client.Self.LastName = (string)LoginValues["last_name"];
                Client.Self.LookAt = vector;
                Client.Self.HomePosition = posVector;
                Client.Self.HomeLookAt = lookatVector;

                // Connect to the sim given in the login reply
                Simulator simulator = new Simulator(Client, this.Callbacks, (uint)(int)LoginValues["circuit_code"],
                    IPAddress.Parse((string)LoginValues["sim_ip"]), (int)LoginValues["sim_port"]);
                if (!simulator.Connected)
                {
                    return false;
                }

                simulator.Region.Handle = regionHandle;
                CurrentSim = simulator;

                // Simulator is successfully connected, add it to the list and set it as default
                Simulators.Add(simulator);

                // Move our agent in to the sim to complete the connection
                Client.Self.CompleteAgentMovement(simulator);

                SendInitialPackets();

                DisconnectTimer.Start();
                connected = true;
                if (OnConnected != null) OnConnected(this.Client);
                return true;
            }
            catch (Exception e)
            {
                Client.Log("Login error: " + e.ToString(), Helpers.LogLevel.Error);
                return false;
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="circuitCode"></param>
        /// <param name="setDefault"></param>
        /// <returns></returns>
		public Simulator Connect(IPAddress ip, ushort port, uint circuitCode, bool setDefault)
		{
			Simulator simulator = new Simulator(Client, this.Callbacks, circuitCode, ip, (int)port);

			if (!simulator.Connected)
			{
                simulator = null;
				return null;
            }

            lock (Simulators)
            {
                Simulators.Add(simulator);
            }

            if (setDefault)
			{
				CurrentSim = simulator;
			}

            DisconnectTimer.Start();
            connected = true;
			return simulator;
		}

        /// <summary>
        /// 
        /// </summary>
		public void Logout()
        {
            // This will catch a Logout when the client is not logged in
            if (CurrentSim == null || !connected)
            {
                return;
            }

            Client.Log("Logging out", Helpers.LogLevel.Info);

            DisconnectTimer.Stop();
            connected = false;

            // Send a logout request to the current sim
            LogoutRequestPacket logout = new LogoutRequestPacket();
            logout.AgentData.AgentID = AgentID;
            logout.AgentData.SessionID = SessionID;

			CurrentSim.SendPacket(logout, true);

            // TODO: We should probably check if the server actually received the logout request
            
            // Shutdown the network layer
            Shutdown();

            if (OnDisconnected != null)
            {
                OnDisconnected(DisconnectType.ClientInitiated, "");
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sim"></param>
        public void DisconnectSim(Simulator sim)
        {
            sim.Disconnect();

            // Fire the SimDisconnected event if a handler is registered
            if (OnSimDisconnected != null)
            {
                OnSimDisconnected(sim, DisconnectType.NetworkTimeout);
            }

            lock (Simulators)
            {
                Simulators.Remove(sim);
            }
        }

        /// <summary>
        /// Shutdown will disconnect all the sims except for the current sim
        /// first, and then kill the connection to CurrentSim.
        /// </summary>
        private void Shutdown()
        {
            lock (Simulators)
            {
                // Disconnect all simulators except the current one
                foreach (Simulator simulator in Simulators)
                {
                    // Don't disconnect the current sim, we'll use LogoutRequest for that
                    if (simulator != CurrentSim)
                    {
                        simulator.Disconnect();

                        // Fire the SimDisconnected event if a handler is registered
                        if (OnSimDisconnected != null)
                        {
                            OnSimDisconnected(simulator, DisconnectType.NetworkTimeout);
                        }
                    }
                }

                Simulators.Clear();
            }

            CurrentSim.Disconnect();
            CurrentSim = null;
        }

        private void SendInitialPackets()
        {
            // Request the economy data
            SendPacket(new EconomyDataRequestPacket());

            // TODO: The client sends this early in the login sequence, should we?
            //ViewerEffectPacket effect = new ViewerEffectPacket();
            //effect.Effect = new ViewerEffectPacket.EffectBlock[1];
            //effect.Effect[0] = new ViewerEffectPacket.EffectBlock();
            //effect.Effect[0].Color = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
            //effect.Effect[0].Duration = 0.5f;
            //effect.Effect[0].ID = new LLUUID("c696075e53c6153f3d8e0c3e24541936");
            //effect.Effect[0].Type = 9;
            //effect.Effect[0].TypeData = new byte[56];
            //Array.Copy(new byte[] { 0x28, 0xF0, 0x10, 0x41 }, 0, effect.Effect[0].TypeData, 36, 4);
            //Array.Copy(new byte[] { 0x50, 0xD0, 0x0E, 0x41 }, 0, effect.Effect[0].TypeData, 44, 4);
            //Array.Copy(new byte[] { 0x24, 0x40 }, 0, effect.Effect[0].TypeData, 54, 2);
            //effect.Header.Reliable = false;
            //SendPacket(effect);

            // TODO: Is this throttle necessary/good, and what does it do?
            AgentThrottlePacket throttle = new AgentThrottlePacket();
            throttle.AgentData.AgentID = this.AgentID;
            throttle.AgentData.SessionID = this.SessionID;
            throttle.AgentData.CircuitCode = this.CurrentSim.CircuitCode;
            throttle.Throttle.GenCounter = 0;
            throttle.Throttle.Throttles = new byte[] 
                { 0x00, 0x00, 0x96, 0x47, 0x00, 0x00, 0xAA, 0x47, 0x00, 0x00, 0x88, 0x46, 0x00, 0x00, 0x88, 0x46, 
                  0x00, 0x00, 0x5F, 0x48, 0x00, 0x00, 0x5F, 0x48, 0x00, 0x00, 0xDC, 0x47 };
            SendPacket(throttle);

            // TODO: We should be setting the initial avatar height/width around here 
            //Client.Avatar.SetHeightWidth(676, 909);

            // Set the initial avatar camera position
            Client.Self.UpdateCamera(true);

            // TODO: What animation are we stopping here?
            //AgentAnimationPacket animation = new AgentAnimationPacket();
            //animation.AgentData.AgentID = AgentID;
            //animation.AgentData.SessionID = SessionID;
            //animation.AnimationList = new AgentAnimationPacket.AnimationListBlock[1];
            //animation.AnimationList[0] = new AgentAnimationPacket.AnimationListBlock();
            //animation.AnimationList[0].AnimID = new LLUUID("efcf670c2d188128973a034ebc806b67");
            //animation.AnimationList[0].StartAnim = false;
            //SendPacket(animation);

            // TODO: Do we ever want to set this to true?
            SetAlwaysRunPacket run = new SetAlwaysRunPacket();
            run.AgentData.AgentID = AgentID;
            run.AgentData.SessionID = SessionID;
            run.AgentData.AlwaysRun = false;
            SendPacket(run);

            // TODO: This information is currently unused
            MuteListRequestPacket mute = new MuteListRequestPacket();
            mute.AgentData.AgentID = AgentID;
            mute.AgentData.SessionID = SessionID;
            mute.MuteData.MuteCRC = 0;
            SendPacket(mute);

            // Get the current avatar balance
            MoneyBalanceRequestPacket money = new MoneyBalanceRequestPacket();
            money.AgentData.AgentID = AgentID;
            money.AgentData.SessionID = SessionID;
            money.MoneyData.TransactionID = LLUUID.Zero;
            SendPacket(money);

            // FIXME: MainAvatar can request the info if it wants to use it
            //AgentDataUpdateRequestPacket update = new AgentDataUpdateRequestPacket();
            //update.AgentData.AgentID = AgentID;
            //update.AgentData.SessionID = SessionID;
            //SendPacket(update);

            // TODO: What is the purpose of this? Information is currently unused
            RequestGrantedProxiesPacket proxies = new RequestGrantedProxiesPacket();
            proxies.AgentData.AgentID = AgentID;
            proxies.AgentData.SessionID = SessionID;
            SendPacket(proxies);
        }

        private void DisconnectTimer_Elapsed(object sender, ElapsedEventArgs ev)
        {
            // If the current simulator is disconnected, shutdown+callback+return
            if (CurrentSim.DisconnectCandidate)
            {
                Client.Log("Network timeout for the current simulator (" +
                    CurrentSim.Region.Name + "), logging out", Helpers.LogLevel.Warning);

                DisconnectTimer.Stop();
                connected = false;

                // Shutdown the network layer
                Shutdown();

                if (OnDisconnected != null)
                {
                    OnDisconnected(DisconnectType.NetworkTimeout, "");
                }

                // We're completely logged out and shut down, leave this function
                return;
            }

            List<Simulator> disconnectedSims = null;

            // Check all of the connected sims for disconnects
            lock (Simulators)
            {
                foreach (Simulator sim in Simulators)
                {
                    if (sim.DisconnectCandidate)
                    {
                        if (disconnectedSims == null)
                        {
                            disconnectedSims = new List<Simulator>();
                        }

                        disconnectedSims.Add(sim);
                    }
                    else
                    {
                        sim.DisconnectCandidate = true;
                    }
                }
            }

            // Actually disconnect each sim we detected as disconnected
            if (disconnectedSims != null)
            {
                foreach (Simulator sim in disconnectedSims)
                {
                    // This sim hasn't received any network traffic since the 
                    // timer last elapsed, consider it disconnected
                    Client.Log("Network timeout for simulator " + sim.Region.Name +
                        ", disconnecting", Helpers.LogLevel.Warning);

                    DisconnectSim(sim);
                }
            }
        }

		private void StartPingCheckHandler(Packet packet, Simulator simulator)
		{
            StartPingCheckPacket incomingPing = (StartPingCheckPacket)packet;
            CompletePingCheckPacket ping = new CompletePingCheckPacket();
            ping.PingID.PingID = incomingPing.PingID.PingID;

            // TODO: We can use OldestUnacked to correct transmission errors

            SendPacket((Packet)ping, simulator);
		}

		private void RegionHandshakeHandler(Packet packet, Simulator simulator)
		{
            // Send a RegionHandshakeReply
            RegionHandshakeReplyPacket reply = new RegionHandshakeReplyPacket();
            reply.AgentData.AgentID = AgentID;
            reply.AgentData.SessionID = SessionID;
            reply.RegionInfo.Flags = 0;
            SendPacket(reply, simulator);

            // TODO: Do we need to send an AgentUpdate to each sim upon connection?

            RegionHandshakePacket handshake = (RegionHandshakePacket)packet;

            simulator.Region.ID = handshake.RegionInfo.CacheID;

            // FIXME:
            //handshake.RegionInfo.BillableFactor;
            //handshake.RegionInfo.RegionFlags;
            //handshake.RegionInfo.SimAccess;

            simulator.Region.IsEstateManager = handshake.RegionInfo.IsEstateManager;
            simulator.Region.Name = Helpers.FieldToString(handshake.RegionInfo.SimName);
            simulator.Region.SimOwner = handshake.RegionInfo.SimOwner;
            simulator.Region.TerrainBase0 = handshake.RegionInfo.TerrainBase0;
            simulator.Region.TerrainBase1 = handshake.RegionInfo.TerrainBase1;
            simulator.Region.TerrainBase2 = handshake.RegionInfo.TerrainBase2;
            simulator.Region.TerrainBase3 = handshake.RegionInfo.TerrainBase3;
            simulator.Region.TerrainDetail0 = handshake.RegionInfo.TerrainDetail0;
            simulator.Region.TerrainDetail1 = handshake.RegionInfo.TerrainDetail1;
            simulator.Region.TerrainDetail2 = handshake.RegionInfo.TerrainDetail2;
            simulator.Region.TerrainDetail3 = handshake.RegionInfo.TerrainDetail3;
            simulator.Region.TerrainHeightRange00 = handshake.RegionInfo.TerrainHeightRange00;
            simulator.Region.TerrainHeightRange01 = handshake.RegionInfo.TerrainHeightRange01;
            simulator.Region.TerrainHeightRange10 = handshake.RegionInfo.TerrainHeightRange10;
            simulator.Region.TerrainHeightRange11 = handshake.RegionInfo.TerrainHeightRange11;
            simulator.Region.TerrainStartHeight00 = handshake.RegionInfo.TerrainStartHeight00;
            simulator.Region.TerrainStartHeight01 = handshake.RegionInfo.TerrainStartHeight01;
            simulator.Region.TerrainStartHeight10 = handshake.RegionInfo.TerrainStartHeight10;
            simulator.Region.TerrainStartHeight11 = handshake.RegionInfo.TerrainStartHeight11;
            simulator.Region.WaterHeight = handshake.RegionInfo.WaterHeight;

            Client.Log("Received a region handshake for " + simulator.Region.Name, Helpers.LogLevel.Info);
		}

		private void ParcelOverlayHandler(Packet packet, Simulator simulator)
		{
            ParcelOverlayPacket overlay = (ParcelOverlayPacket)packet;

            if (overlay.ParcelData.SequenceID >= 0 && overlay.ParcelData.SequenceID <= 3)
            {
                Array.Copy(overlay.ParcelData.Data, 0, simulator.Region.ParcelOverlay,
                    overlay.ParcelData.SequenceID * 1024, 1024);
                simulator.Region.ParcelOverlaysReceived++;

                if (simulator.Region.ParcelOverlaysReceived > 3)
                {
                    Client.Log("Finished building the " + simulator.Region.Name + " parcel overlay",
                        Helpers.LogLevel.Info);
                }
            }
            else
            {
                Client.Log("Parcel overlay with sequence ID of " + overlay.ParcelData.SequenceID + 
                    " received from " + simulator.Region.Name, Helpers.LogLevel.Warning);
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
                OnDisconnected(DisconnectType.ServerInitiated, message);
            }
        }
	}
}
