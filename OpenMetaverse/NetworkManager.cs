/*
 * Copyright (c) 2007-2009, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names 
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
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Globalization;
using System.IO;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Interfaces;
using OpenMetaverse.Messages.Linden;

namespace OpenMetaverse
{
    /// <summary>
    /// This exception is thrown whenever a network operation is attempted 
    /// without a network connection.
    /// </summary>
    public class NotConnectedException : ApplicationException { }

    /// <summary>
    /// NetworkManager is responsible for managing the network layer of 
    /// OpenMetaverse. It tracks all the server connections, serializes 
    /// outgoing traffic and deserializes incoming traffic, and provides
    /// instances of delegates for network-related events.
    /// </summary>
    public partial class NetworkManager : INetworkManager
    {
        #region Enums

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
            NetworkTimeout,
            /// <summary>The last active simulator shut down</summary>
            SimShutdown
        }

        #endregion Enums

        #region Structs

        /// <summary>
        /// Holds a simulator reference and a decoded packet, these structs are put in
        /// the packet inbox for event handling
        /// </summary>
        public struct IncomingPacket
        {
            /// <summary>Reference to the simulator that this packet came from</summary>
            public Simulator Simulator;
            /// <summary>Packet that needs to be processed</summary>
            public Packet Packet;

            public IncomingPacket(Simulator simulator, Packet packet)
            {
                Simulator = simulator;
                Packet = packet;
            }
        }

        /// <summary>
        /// Holds a simulator reference and a serialized packet, these structs are put in
        /// the packet outbox for sending
        /// </summary>
        public class OutgoingPacket
        {
            /// <summary>Reference to the simulator this packet is destined for</summary>
            public readonly Simulator Simulator;
            /// <summary>Packet that needs to be sent</summary>
            public readonly UDPPacketBuffer Buffer;
            /// <summary>Sequence number of the wrapped packet</summary>
            public uint SequenceNumber;
            /// <summary>Number of times this packet has been resent</summary>
            public int ResendCount;
            /// <summary>Environment.TickCount when this packet was last sent over the wire</summary>
            public int TickCount;

            public OutgoingPacket(Simulator simulator, UDPPacketBuffer buffer)
            {
                Simulator = simulator;
                Buffer = buffer;
            }
        }

        #endregion Structs

        #region Delegates

        /// <summary>
        /// Coupled with RegisterCallback(), this is triggered whenever a packet
        /// of a registered type is received
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        public delegate void PacketCallback(Packet packet, Simulator simulator);
        /// <summary>
        /// Triggered whenever an outgoing packet is sent
        /// </summary>
        /// <param name="data">Buffer holding the outgoing packet payload</param>
        /// <param name="bytesSent">Number of bytes of the data buffer that were sent</param>
        /// <param name="simulator">Simulator this packet was sent to</param>
        public delegate void PacketSentCallback(byte[] data, int bytesSent, Simulator simulator);
        /// <summary>
        /// Assigned by the OnConnected event. Raised when login was a success
        /// </summary>
        /// <param name="sender">Reference to the GridClient object that called the event</param>
        public delegate void ConnectedCallback(object sender);
        /// <summary>
        /// Assigned by the OnLogoutReply callback. Raised upone receipt of a LogoutReply packet during logout process.
        /// </summary>
        /// <param name="inventoryItems"></param>
        public delegate void LogoutCallback(List<UUID> inventoryItems);
        /// <summary>
        /// Triggered before a new connection to a simulator is established
        /// </summary>
        /// <remarks>The connection to the new simulator won't be established
        /// until this callback returns</remarks>
        /// <param name="simulator">The simulator that is being connected to</param>
        /// <returns>Whether to continue connecting to the simulator or abort
        /// the connection</returns>
        public delegate bool SimConnectingCallback(Simulator simulator);
        /// <summary>
        /// Triggered when a new connection to a simulator is established
        /// </summary>
        /// <param name="simulator">The simulator that is being connected to</param>
        public delegate void SimConnectedCallback(Simulator simulator);
        /// <summary>
        /// Triggered when a simulator other than the simulator that is currently
        /// being occupied disconnects for whatever reason
        /// </summary>
        /// <param name="simulator">The simulator that disconnected, which will become a null
        /// reference after the callback is finished</param>
        /// <param name="reason">Enumeration explaining the reason for the disconnect</param>
        public delegate void SimDisconnectedCallback(Simulator simulator, DisconnectType reason);
        /// <summary>
        /// Triggered when we are logged out of the grid due to a simulator request,
        /// client request, network timeout, or any other cause
        /// </summary>
        /// <param name="reason">Enumeration explaining the reason for the disconnect</param>
        /// <param name="message">If we were logged out by the simulator, this 
        /// is a message explaining why</param>
        public delegate void DisconnectedCallback(DisconnectType reason, string message);
        /// <summary>
        /// Triggered when CurrentSim changes
        /// </summary>
        /// <param name="PreviousSimulator">A reference to the old value of CurrentSim</param>
        public delegate void CurrentSimChangedCallback(Simulator PreviousSimulator);
        /// <summary>
        /// Triggered when an event queue makes the initial connection
        /// </summary>
        /// <param name="simulator">Simulator this event queue is tied to</param>
        public delegate void EventQueueRunningCallback(Simulator simulator);

        #endregion Delegates

        #region Events

        /// <summary>
        /// Event raised when an outgoing packet is sent to a simulator
        /// </summary>
        public event PacketSentCallback OnPacketSent;
        /// <summary>
        /// Event raised when the client was able to connected successfully.
        /// </summary>
        /// <remarks>Uses the ConnectedCallback delegate.</remarks>
        public event ConnectedCallback OnConnected;
        /// <summary>
        /// Event raised when a logout is confirmed by the simulator
        /// </summary>
        public event LogoutCallback OnLogoutReply;
        /// <summary>
        /// Event raised when a before a connection to a simulator is 
        /// initialized
        /// </summary>
        public event SimConnectingCallback OnSimConnecting;
        /// <summary>
        /// Event raised when a connection to a simulator is established
        /// </summary>
        public event SimConnectedCallback OnSimConnected;
        /// <summary>
        /// An event for the connection to a simulator other than the currently
        /// occupied one disconnecting
        /// </summary>
        /// <remarks>The Simulators list is locked when this event is 
        /// triggered, do not attempt to modify the collection or acquire a
        /// lock on it when this callback is fired</remarks>
        public event SimDisconnectedCallback OnSimDisconnected;
        /// <summary>
        /// An event for being logged out either through client request, server
        /// forced, or network error
        /// </summary>
        public event DisconnectedCallback OnDisconnected;
        /// <summary>
        /// An event for when CurrentSim changes
        /// </summary>
        public event CurrentSimChangedCallback OnCurrentSimChanged;
        /// <summary>
        /// Triggered when an event queue makes the initial connection
        /// </summary>
        public event EventQueueRunningCallback OnEventQueueRunning;

        #endregion Events

        #region Properties

        /// <summary>Unique identifier associated with our connections to
        /// simulators</summary>
        public uint CircuitCode
        {
            get { return _CircuitCode; }
            set { _CircuitCode = value; }
        }
        /// <summary>The simulator that the logged in avatar is currently 
        /// occupying</summary>
        public Simulator CurrentSim
        {
            get { return _CurrentSim; }
            set { _CurrentSim = value; }
        }
        /// <summary>Shows whether the network layer is logged in to the
        /// grid or not</summary>
        public bool Connected { get { return connected; } }
        /// <summary>Number of packets in the incoming queue</summary>
        public int InboxCount { get { return PacketInbox.Count; } }
        /// <summary>Number of packets in the outgoing queue</summary>
        public int OutboxCount { get { return PacketOutbox.Count; } }

        #endregion Properties

        /// <summary>All of the simulators we are currently connected to</summary>
        public List<Simulator> Simulators = new List<Simulator>();

        /// <summary>Handlers for incoming capability events</summary>
        internal CapsEventDictionary CapsEvents;
        /// <summary>Handlers for incoming packets</summary>
        internal PacketEventDictionary PacketEvents;
        /// <summary>Incoming packets that are awaiting handling</summary>
        internal BlockingQueue<IncomingPacket> PacketInbox = new BlockingQueue<IncomingPacket>(Settings.PACKET_INBOX_SIZE);
        /// <summary>Outgoing packets that are awaiting handling</summary>
        internal BlockingQueue<OutgoingPacket> PacketOutbox = new BlockingQueue<OutgoingPacket>(Settings.PACKET_INBOX_SIZE);

        private GridClient Client;
        private Timer DisconnectTimer;
        private uint _CircuitCode;
        private Simulator _CurrentSim = null;
        private bool connected = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">Reference to the GridClient object</param>
        public NetworkManager(GridClient client)
        {
            Client = client;

            PacketEvents = new PacketEventDictionary(client);
            CapsEvents = new CapsEventDictionary(client);

            // Register internal CAPS callbacks
            RegisterEventCallback("EnableSimulator", new Caps.EventQueueCallback(EnableSimulatorHandler));

            // Register the internal callbacks
            RegisterCallback(PacketType.RegionHandshake, new PacketCallback(RegionHandshakeHandler));
            RegisterCallback(PacketType.StartPingCheck, new PacketCallback(StartPingCheckHandler));
            
            RegisterCallback(PacketType.DisableSimulator, new PacketCallback(DisableSimulatorHandler));
            RegisterCallback(PacketType.KickUser, new PacketCallback(KickUserHandler));
            RegisterCallback(PacketType.LogoutReply, new PacketCallback(LogoutReplyHandler));
            RegisterCallback(PacketType.CompletePingCheck, new PacketCallback(PongHandler));
			RegisterCallback(PacketType.SimStats, new PacketCallback(SimStatsHandler));

            // GLOBAL SETTING: Don't force Expect-100: Continue headers on HTTP POST calls
            ServicePointManager.Expect100Continue = false;
        }

        /// <summary>
        /// Register an event handler for a packet. This is a low level event
        /// interface and should only be used if you are doing something not
        /// supported in the library
        /// </summary>
        /// <param name="type">Packet type to trigger events for</param>
        /// <param name="callback">Callback to fire when a packet of this type
        /// is received</param>
        public void RegisterCallback(PacketType type, PacketCallback callback)
        {
            PacketEvents.RegisterEvent(type, callback);
        }

        /// <summary>
        /// Unregister an event handler for a packet. This is a low level event
        /// interface and should only be used if you are doing something not 
        /// supported in the library
        /// </summary>
        /// <param name="type">Packet type this callback is registered with</param>
        /// <param name="callback">Callback to stop firing events for</param>
        public void UnregisterCallback(PacketType type, PacketCallback callback)
        {
            PacketEvents.UnregisterEvent(type, callback);
        }

        /// <summary>
        /// Register a CAPS event handler. This is a low level event interface
        /// and should only be used if you are doing something not supported in
        /// the library
        /// </summary>
        /// <param name="capsEvent">Name of the CAPS event to register a handler for</param>
        /// <param name="callback">Callback to fire when a CAPS event is received</param>
        public void RegisterEventCallback(string capsEvent, Caps.EventQueueCallback callback)
        {
            CapsEvents.RegisterEvent(capsEvent, callback);
        }

        /// <summary>
        /// Unregister a CAPS event handler. This is a low level event interface
        /// and should only be used if you are doing something not supported in
        /// the library
        /// </summary>
        /// <param name="capsEvent">Name of the CAPS event this callback is
        /// registered with</param>
        /// <param name="callback">Callback to stop firing events for</param>
        public void UnregisterEventCallback(string capsEvent, Caps.EventQueueCallback callback)
        {
            CapsEvents.UnregisterEvent(capsEvent, callback);
        }

        /// <summary>
        /// Send a packet to the simulator the avatar is currently occupying
        /// </summary>
        /// <param name="packet">Packet to send</param>
        public void SendPacket(Packet packet)
        {
            if (CurrentSim != null && CurrentSim.Connected)
                CurrentSim.SendPacket(packet);
        }

        /// <summary>
        /// Send a packet to a specified simulator
        /// </summary>
        /// <param name="packet">Packet to send</param>
        /// <param name="simulator">Simulator to send the packet to</param>
        public void SendPacket(Packet packet, Simulator simulator)
        {
            if (simulator != null)
                simulator.SendPacket(packet);
        }

        /// <summary>
        /// Connect to a simulator
        /// </summary>
        /// <param name="ip">IP address to connect to</param>
        /// <param name="port">Port to connect to</param>
        /// <param name="handle">Handle for this simulator, to identify its
        /// location in the grid</param>
        /// <param name="setDefault">Whether to set CurrentSim to this new
        /// connection, use this if the avatar is moving in to this simulator</param>
        /// <param name="seedcaps">URL of the capabilities server to use for
        /// this sim connection</param>
        /// <returns>A Simulator object on success, otherwise null</returns>
        public Simulator Connect(IPAddress ip, ushort port, ulong handle, bool setDefault, string seedcaps)
        {
            IPEndPoint endPoint = new IPEndPoint(ip, (int)port);
            return Connect(endPoint, handle, setDefault, seedcaps);
        }

        /// <summary>
        /// Connect to a simulator
        /// </summary>
        /// <param name="endPoint">IP address and port to connect to</param>
        /// <param name="handle">Handle for this simulator, to identify its
        /// location in the grid</param>
        /// <param name="setDefault">Whether to set CurrentSim to this new
        /// connection, use this if the avatar is moving in to this simulator</param>
        /// <param name="seedcaps">URL of the capabilities server to use for
        /// this sim connection</param>
        /// <returns>A Simulator object on success, otherwise null</returns>
        public Simulator Connect(IPEndPoint endPoint, ulong handle, bool setDefault, string seedcaps)
        {
            Simulator simulator = FindSimulator(endPoint);

            if (simulator == null)
            {
                // We're not tracking this sim, create a new Simulator object
                simulator = new Simulator(Client, endPoint, handle);

                // Immediately add this simulator to the list of current sims. It will be removed if the
                // connection fails
                lock (Simulators) Simulators.Add(simulator);
            }

            if (!simulator.Connected)
            {
                if (!connected)
                {
                    // Mark that we are connecting/connected to the grid
                    connected = true;

                    // Open the queues in case this is a reconnect and they were shut down
                    PacketInbox.Open();
                    PacketOutbox.Open();

                    // Start the packet decoding thread
                    Thread decodeThread = new Thread(new ThreadStart(IncomingPacketHandler));
                    decodeThread.Start();

                    // Start the packet sending thread
                    Thread sendThread = new Thread(new ThreadStart(OutgoingPacketHandler));
                    sendThread.Start();
                }

                // Fire the OnSimConnecting event
                if (OnSimConnecting != null)
                {
                    try
                    {
                        if (!OnSimConnecting(simulator))
                        {
                            // Callback is requesting that we abort this connection
                            lock (Simulators) Simulators.Remove(simulator);
                            return null;
                        }
                    }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }

                // Attempt to establish a connection to the simulator
                if (simulator.Connect(setDefault))
                {
                    if (DisconnectTimer == null)
                    {
                        // Start a timer that checks if we've been disconnected
                        DisconnectTimer = new Timer(new TimerCallback(DisconnectTimer_Elapsed), null,
                            Client.Settings.SIMULATOR_TIMEOUT, Client.Settings.SIMULATOR_TIMEOUT);
                    }

                    if (setDefault) SetCurrentSim(simulator, seedcaps);

                    // Fire the simulator connection callback if one is registered
                    if (OnSimConnected != null)
                    {
                        try { OnSimConnected(simulator); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                    }

                    // If enabled, send an AgentThrottle packet to the server to increase our bandwidth
                    if (Client.Settings.SEND_AGENT_THROTTLE) Client.Throttle.Set(simulator);

                    return simulator;
                }
                else
                {
                    // Connection failed, remove this simulator from our list and destroy it
                    lock (Simulators) Simulators.Remove(simulator);
                    return null;
                }
            }
            else if (setDefault)
            {
                // We're already connected to this server, but need to set it to the default
                SetCurrentSim(simulator, seedcaps);

                // Move in to this simulator
                Client.Self.CompleteAgentMovement(simulator);

                // Send an initial AgentUpdate to complete our movement in to the sim
                if (Client.Settings.SEND_AGENT_UPDATES)
                    Client.Self.Movement.SendUpdate(true, simulator);

                return simulator;
            }
            else
            {
                // Already connected to this simulator and wasn't asked to set it as the default,
                // just return a reference to the existing object
                return simulator;
            }
        }

        /// <summary>
        /// Initiate a blocking logout request. This will return when the logout
        /// handshake has completed or when <code>Settings.LOGOUT_TIMEOUT</code>
        /// has expired and the network layer is manually shut down
        /// </summary>
        public void Logout()
        {
            AutoResetEvent logoutEvent = new AutoResetEvent(false);
            LogoutCallback callback = 
                delegate(List<UUID> inventoryItems) { logoutEvent.Set(); };
            OnLogoutReply += callback;

            // Send the packet requesting a clean logout
            RequestLogout();

            // Wait for a logout response. If the response is received, shutdown
            // will be fired in the callback. Otherwise we fire it manually with
            // a NetworkTimeout type
            if (!logoutEvent.WaitOne(Client.Settings.LOGOUT_TIMEOUT, false))
                Shutdown(DisconnectType.NetworkTimeout);

            OnLogoutReply -= callback;
        }

        /// <summary>
        /// Initiate the logout process. Check if logout succeeded with the
        /// <code>OnLogoutReply</code> event, and if this does not fire the
        /// <code>Shutdown()</code> function needs to be manually called
        /// </summary>
        public void RequestLogout()
        {
            // No need to run the disconnect timer any more
            if (DisconnectTimer != null) DisconnectTimer.Dispose();

            // This will catch a Logout when the client is not logged in
            if (CurrentSim == null || !connected)
            {
                Logger.Log("Ignoring RequestLogout(), client is already logged out", Helpers.LogLevel.Warning, Client);
                return;
            }

            Logger.Log("Logging out", Helpers.LogLevel.Info, Client);

            // Send a logout request to the current sim
            LogoutRequestPacket logout = new LogoutRequestPacket();
            logout.AgentData.AgentID = Client.Self.AgentID;
            logout.AgentData.SessionID = Client.Self.SessionID;
            SendPacket(logout);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sim"></param>
        /// <param name="sendCloseCircuit"></param>
        public void DisconnectSim(Simulator sim, bool sendCloseCircuit)
        {
            if (sim != null)
            {
                sim.Disconnect(sendCloseCircuit);

                // Fire the SimDisconnected event if a handler is registered
                if (OnSimDisconnected != null)
                {
                    try { OnSimDisconnected(sim, DisconnectType.NetworkTimeout); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }

                lock (Simulators) Simulators.Remove(sim);

                if (Simulators.Count == 0) Shutdown(DisconnectType.SimShutdown);
            }
            else
            {
                Logger.Log("DisconnectSim() called with a null Simulator reference", Helpers.LogLevel.Warning, Client);
            }
        }

        /// <summary>
        /// Shutdown will disconnect all the sims except for the current sim
        /// first, and then kill the connection to CurrentSim. This should only
        /// be called if the logout process times out on <code>RequestLogout</code>
        /// </summary>
        public void Shutdown(DisconnectType type)
        {
            Logger.Log("NetworkManager shutdown initiated", Helpers.LogLevel.Info, Client);

            // Send a CloseCircuit packet to simulators if we are initiating the disconnect
            bool sendCloseCircuit = (type == DisconnectType.ClientInitiated || type == DisconnectType.NetworkTimeout);

            lock (Simulators)
            {
                // Disconnect all simulators except the current one
                for (int i = 0; i < Simulators.Count; i++)
                {
                    if (Simulators[i] != null && Simulators[i] != CurrentSim)
                    {
                        Simulators[i].Disconnect(sendCloseCircuit);

                        // Fire the SimDisconnected event if a handler is registered
                        if (OnSimDisconnected != null)
                        {
                            try { OnSimDisconnected(Simulators[i], type); }
                            catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                        }
                    }
                }

                Simulators.Clear();
            }

            if (CurrentSim != null)
            {
                // Kill the connection to the curent simulator
                CurrentSim.Disconnect(sendCloseCircuit);

                // Fire the SimDisconnected event if a handler is registered
                if (OnSimDisconnected != null)
                {
                    try { OnSimDisconnected(CurrentSim, type); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }

            // Clear out all of the packets that never had time to process
            PacketInbox.Close();
            PacketOutbox.Close();

            connected = false;

            // Fire the disconnected callback
            if (OnDisconnected != null)
            {
                try { OnDisconnected(DisconnectType.ClientInitiated, String.Empty); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary>
        /// Searches through the list of currently connected simulators to find
        /// one attached to the given IPEndPoint
        /// </summary>
        /// <param name="endPoint">IPEndPoint of the Simulator to search for</param>
        /// <returns>A Simulator reference on success, otherwise null</returns>
        public Simulator FindSimulator(IPEndPoint endPoint)
        {
            lock (Simulators)
            {
                for (int i = 0; i < Simulators.Count; i++)
                {
                    if (Simulators[i].IPEndPoint.Equals(endPoint))
                        return Simulators[i];
                }
            }

            return null;
        }

        internal void PacketSent(byte[] data, int bytesSent, Simulator simulator)
        {
            if (OnPacketSent != null)
                OnPacketSent(data, bytesSent, simulator);
        }

        /// <summary>
        /// Fire an event when an event queue connects for capabilities
        /// </summary>
        /// <param name="simulator">Simulator the event queue is attached to</param>
        internal void RaiseConnectedEvent(Simulator simulator)
        {
            if (OnEventQueueRunning != null)
            {
                try { OnEventQueueRunning(simulator); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void OutgoingPacketHandler()
        {
            OutgoingPacket outgoingPacket = null;
            Simulator simulator;
            
            // FIXME: This is kind of ridiculous. Port the HTB code from Simian over ASAP!
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            while (connected)
            {
                if (PacketOutbox.Dequeue(100, ref outgoingPacket))
                {
                    simulator = outgoingPacket.Simulator;
                    
                    // Very primitive rate limiting, keeps a fixed buffer of time between each packet
                    stopwatch.Stop();
                    if (stopwatch.ElapsedMilliseconds < 10)
                    {
                        //Logger.DebugLog(String.Format("Rate limiting, last packet was {0}ms ago", ms));
                        Thread.Sleep(10 - (int)stopwatch.ElapsedMilliseconds);
                    }

                    simulator.SendPacketFinal(outgoingPacket);
                    stopwatch.Start();
                }
            }
        }

        private void IncomingPacketHandler()
        {
            IncomingPacket incomingPacket = new IncomingPacket();
            Packet packet = null;
            Simulator simulator = null;

            while (connected)
            {
                // Reset packet to null for the check below
                packet = null;

                if (PacketInbox.Dequeue(100, ref incomingPacket))
                {
                    packet = incomingPacket.Packet;
                    simulator = incomingPacket.Simulator;

                    if (packet != null)
                    {
                        // skip blacklisted packets
                        if (UDPBlacklist.Contains(packet.Type.ToString()))
                        {
                            Logger.Log(String.Format("Discarding Blacklisted packet {0} from {1}", 
                                packet.Type, simulator.IPEndPoint), Helpers.LogLevel.Warning);
                            return;
                        }

                        #region Fire callbacks

                        if (Client.Settings.SYNC_PACKETCALLBACKS)
                            PacketEvents.RaiseEvent(packet.Type, packet, simulator);
                        else
                            PacketEvents.BeginRaiseEvent(packet.Type, packet, simulator);

                        #endregion Fire callbacks
                    }
                }
            }
        }

        private void SetCurrentSim(Simulator simulator, string seedcaps)
        {
            if (simulator != CurrentSim)
            {
                Simulator oldSim = CurrentSim;
                lock (Simulators) CurrentSim = simulator; // CurrentSim is synchronized against Simulators

		        simulator.SetSeedCaps(seedcaps);

                // If the current simulator changed fire the callback
                if (OnCurrentSimChanged != null && simulator != oldSim)
                {
                    try { OnCurrentSimChanged(oldSim); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }
        }

        #region Timers

        private void DisconnectTimer_Elapsed(object obj)
        {
            if (!connected || CurrentSim == null)
            {
                Timer timer = DisconnectTimer;
                if (timer != null)
                    timer.Dispose();
                connected = false;
            }
            else if (CurrentSim.DisconnectCandidate)
            {
                // The currently occupied simulator hasn't sent us any traffic in a while, shutdown
                Logger.Log("Network timeout for the current simulator (" +
                    CurrentSim.ToString() + "), logging out", Helpers.LogLevel.Warning, Client);

                if (DisconnectTimer != null) DisconnectTimer.Dispose();
                connected = false;

                // Shutdown the network layer
                Shutdown(DisconnectType.NetworkTimeout);
            }
            else
            {
                // Mark the current simulator as potentially disconnected each time this timer fires.
                // If the timer is fired again before any packets are received, an actual disconnect
                // sequence will be triggered
                CurrentSim.DisconnectCandidate = true;
            }
        }

        #endregion Timers

        #region Packet Callbacks

        /// <summary>
        /// Called to deal with LogoutReply packet and fires off callback
        /// </summary>
        /// <param name="packet">Full packet of type LogoutReplyPacket</param>
        /// <param name="simulator"></param>
        private void LogoutReplyHandler(Packet packet, Simulator simulator)
        {
            LogoutReplyPacket logout = (LogoutReplyPacket)packet;

            if ((logout.AgentData.SessionID == Client.Self.SessionID) && (logout.AgentData.AgentID == Client.Self.AgentID))
            {
                Logger.DebugLog("Logout reply received", Client);

                // Deal with callbacks, if any
                if (OnLogoutReply != null)
                {
                    List<UUID> itemIDs = new List<UUID>();

                    foreach (LogoutReplyPacket.InventoryDataBlock InventoryData in logout.InventoryData)
                    {
                        itemIDs.Add(InventoryData.ItemID);
                    }

                    try { OnLogoutReply(itemIDs); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }

                // If we are receiving a LogoutReply packet assume this is a client initiated shutdown
                Shutdown(DisconnectType.ClientInitiated);
            }
            else
            {
                Logger.Log("Invalid Session or Agent ID received in Logout Reply... ignoring", Helpers.LogLevel.Warning, Client);
            }
        }

        private void StartPingCheckHandler(Packet packet, Simulator simulator)
        {
            StartPingCheckPacket incomingPing = (StartPingCheckPacket)packet;
            CompletePingCheckPacket ping = new CompletePingCheckPacket();
            ping.PingID.PingID = incomingPing.PingID.PingID;
            ping.Header.Reliable = false;
            // TODO: We can use OldestUnacked to correct transmission errors
            //   I don't think that's right.  As far as I can tell, the Viewer
            //   only uses this to prune its duplicate-checking buffer. -bushing

            SendPacket(ping, simulator);
        }

        private void PongHandler(Packet packet, Simulator simulator)
        {
            CompletePingCheckPacket pong = (CompletePingCheckPacket)packet;
            String retval = "Pong2: " + (Environment.TickCount - simulator.Stats.LastPingSent);
            if ((pong.PingID.PingID - simulator.Stats.LastPingID + 1) != 0)
                retval += " (gap of " + (pong.PingID.PingID - simulator.Stats.LastPingID + 1) + ")";

            simulator.Stats.LastLag = Environment.TickCount - simulator.Stats.LastPingSent;
            simulator.Stats.ReceivedPongs++;
            //			Client.Log(retval, Helpers.LogLevel.Info);
        }
		
		private void SimStatsHandler(Packet packet, Simulator simulator)
		{
			if ( ! Client.Settings.ENABLE_SIMSTATS ) {
				return;
			}
			SimStatsPacket stats = (SimStatsPacket)packet;
			for ( int i = 0 ; i < stats.Stat.Length ; i++ ) {
				SimStatsPacket.StatBlock s = stats.Stat[i];
				switch (s.StatID )
				{
					case 0:
                        simulator.Stats.Dilation = s.StatValue;
						break;
					case 1:
                        simulator.Stats.FPS = Convert.ToInt32(s.StatValue);
						break;
					case 2:
                        simulator.Stats.PhysicsFPS = s.StatValue;
						break;
					case 3:
                        simulator.Stats.AgentUpdates = s.StatValue;
						break;
					case 4:
                        simulator.Stats.FrameTime = s.StatValue;
						break;
					case 5:
                        simulator.Stats.NetTime = s.StatValue;
						break;
                    case 6:
                        simulator.Stats.OtherTime = s.StatValue;
                        break;
					case 7:
                        simulator.Stats.PhysicsTime = s.StatValue;
						break;
					case 8:
                        simulator.Stats.AgentTime = s.StatValue;
						break;
					case 9:
                        simulator.Stats.ImageTime = s.StatValue;
						break;
					case 10:
                        simulator.Stats.ScriptTime = s.StatValue;
                        break;
					case 11:
                        simulator.Stats.Objects = Convert.ToInt32(s.StatValue);
						break;
					case 12:
                        simulator.Stats.ScriptedObjects = Convert.ToInt32(s.StatValue);
						break;
					case 13:
                        simulator.Stats.Agents = Convert.ToInt32(s.StatValue);
						break;
					case 14:
                        simulator.Stats.ChildAgents = Convert.ToInt32(s.StatValue);
						break;
					case 15:
                        simulator.Stats.ActiveScripts = Convert.ToInt32(s.StatValue);
						break;
					case 16:
                        simulator.Stats.LSLIPS = Convert.ToInt32(s.StatValue);
						break;
					case 17:
                        simulator.Stats.INPPS = Convert.ToInt32(s.StatValue);
						break;
					case 18:
                        simulator.Stats.OUTPPS = Convert.ToInt32(s.StatValue);
						break;
					case 19:
                        simulator.Stats.PendingDownloads = Convert.ToInt32(s.StatValue);
						break;
					case 20:
                        simulator.Stats.PendingUploads = Convert.ToInt32(s.StatValue);
						break;
					case 21:
                        simulator.Stats.VirtualSize = Convert.ToInt32(s.StatValue);
						break;
					case 22:
                        simulator.Stats.ResidentSize = Convert.ToInt32(s.StatValue);
						break;
					case 23:
                        simulator.Stats.PendingLocalUploads = Convert.ToInt32(s.StatValue);
						break;
					case 24:
                        simulator.Stats.UnackedBytes = Convert.ToInt32(s.StatValue);
						break;
				}
			}
		}

        private void RegionHandshakeHandler(Packet packet, Simulator simulator)
        {
            RegionHandshakePacket handshake = (RegionHandshakePacket)packet;

            simulator.ID = handshake.RegionInfo.CacheID;

            simulator.IsEstateManager = handshake.RegionInfo.IsEstateManager;
            simulator.Name = Utils.BytesToString(handshake.RegionInfo.SimName);
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
            simulator.Flags = (RegionFlags)handshake.RegionInfo.RegionFlags;
            simulator.BillableFactor = handshake.RegionInfo.BillableFactor;
            simulator.Access = (SimAccess)handshake.RegionInfo.SimAccess;


            simulator.RegionID = handshake.RegionInfo2.RegionID;
            simulator.ColoLocation = Utils.BytesToString(handshake.RegionInfo3.ColoName);
            simulator.CPUClass = handshake.RegionInfo3.CPUClassID;
            simulator.CPURatio = handshake.RegionInfo3.CPURatio;
            simulator.ProductName = Utils.BytesToString(handshake.RegionInfo3.ProductName);
            simulator.ProductSku = Utils.BytesToString(handshake.RegionInfo3.ProductSKU);
            

            Logger.Log("Received a region handshake for " + simulator.ToString(), Helpers.LogLevel.Info, Client);

            // Send a RegionHandshakeReply
            RegionHandshakeReplyPacket reply = new RegionHandshakeReplyPacket();
            reply.AgentData.AgentID = Client.Self.AgentID;
            reply.AgentData.SessionID = Client.Self.SessionID;
            reply.RegionInfo.Flags = 0;
            SendPacket(reply, simulator);

            // We're officially connected to this sim
            simulator.connected = true;
            simulator.ConnectedEvent.Set();
        }

        private void EnableSimulatorHandler(string capsKey, IMessage message, Simulator simulator)
        {
            if (!Client.Settings.MULTIPLE_SIMS) return;

            EnableSimulatorMessage msg = (EnableSimulatorMessage)message;

            for (int i = 0; i < msg.Simulators.Length; i++)
            {
                IPAddress ip = msg.Simulators[i].IP;
                ushort port = (ushort)msg.Simulators[i].Port;
                ulong handle = msg.Simulators[i].RegionHandle;

                IPEndPoint endPoint = new IPEndPoint(ip, port);

                if (FindSimulator(endPoint) != null) return;

                if (Connect(ip, port, handle, false, null) == null)
                {
                    Logger.Log("Unabled to connect to new sim " + ip + ":" + port,
                        Helpers.LogLevel.Error, Client);
                }
            }
        }

        private void DisableSimulatorHandler(Packet packet, Simulator simulator)
        {
            Logger.DebugLog("Received a DisableSimulator packet from " + simulator + ", shutting it down", Client);

            DisconnectSim(simulator, false);
        }

        private void KickUserHandler(Packet packet, Simulator simulator)
        {
            string message = Utils.BytesToString(((KickUserPacket)packet).UserInfo.Reason);

            // Fire the callback to let client apps know we are shutting down
            if (OnDisconnected != null)
            {
                try { OnDisconnected(DisconnectType.ServerInitiated, message); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }

            // Shutdown the network layer
            Shutdown(DisconnectType.ServerInitiated);
        }

        #endregion Packet Callbacks
    }
}
