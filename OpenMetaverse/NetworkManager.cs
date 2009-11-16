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
    /// NetworkManager is responsible for managing the network layer of 
    /// OpenMetaverse. It tracks all the server connections, serializes 
    /// outgoing traffic and deserializes incoming traffic, and provides
    /// instances of delegates for network-related events.
    /// </summary>
    public partial class NetworkManager
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

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<PacketSentEventArgs> m_PacketSent;

        ///<summary>Raises the PacketSent Event</summary>
        /// <param name="e">A PacketSentEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnPacketSent(PacketSentEventArgs e)
        {
            EventHandler<PacketSentEventArgs> handler = m_PacketSent;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_PacketSentLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<PacketSentEventArgs> PacketSent
        {
            add { lock (m_PacketSentLock) { m_PacketSent += value; } }
            remove { lock (m_PacketSentLock) { m_PacketSent -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<LoggedOutEventArgs> m_LoggedOut;

        ///<summary>Raises the LoggedOut Event</summary>
        /// <param name="e">A LoggedOutEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnLoggedOut(LoggedOutEventArgs e)
        {
            EventHandler<LoggedOutEventArgs> handler = m_LoggedOut;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_LoggedOutLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<LoggedOutEventArgs> LoggedOut
        {
            add { lock (m_LoggedOutLock) { m_LoggedOut += value; } }
            remove { lock (m_LoggedOutLock) { m_LoggedOut -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<SimConnectingEventArgs> m_SimConnecting;

        ///<summary>Raises the SimConnecting Event</summary>
        /// <param name="e">A SimConnectingEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnSimConnecting(SimConnectingEventArgs e)
        {
            EventHandler<SimConnectingEventArgs> handler = m_SimConnecting;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_SimConnectingLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<SimConnectingEventArgs> SimConnecting
        {
            add { lock (m_SimConnectingLock) { m_SimConnecting += value; } }
            remove { lock (m_SimConnectingLock) { m_SimConnecting -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<SimConnectedEventArgs> m_SimConnected;

        ///<summary>Raises the SimConnected Event</summary>
        /// <param name="e">A SimConnectedEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnSimConnected(SimConnectedEventArgs e)
        {
            EventHandler<SimConnectedEventArgs> handler = m_SimConnected;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_SimConnectedLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<SimConnectedEventArgs> SimConnected
        {
            add { lock (m_SimConnectedLock) { m_SimConnected += value; } }
            remove { lock (m_SimConnectedLock) { m_SimConnected -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<SimDisconnectedEventArgs> m_SimDisconnected;

        ///<summary>Raises the SimDisconnected Event</summary>
        /// <param name="e">A SimDisconnectedEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnSimDisconnected(SimDisconnectedEventArgs e)
        {
            EventHandler<SimDisconnectedEventArgs> handler = m_SimDisconnected;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_SimDisconnectedLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<SimDisconnectedEventArgs> SimDisconnected
        {
            add { lock (m_SimDisconnectedLock) { m_SimDisconnected += value; } }
            remove { lock (m_SimDisconnectedLock) { m_SimDisconnected -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<DisconnectedEventArgs> m_Disconnected;

        ///<summary>Raises the Disconnected Event</summary>
        /// <param name="e">A DisconnectedEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnDisconnected(DisconnectedEventArgs e)
        {
            EventHandler<DisconnectedEventArgs> handler = m_Disconnected;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_DisconnectedLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<DisconnectedEventArgs> Disconnected
        {
            add { lock (m_DisconnectedLock) { m_Disconnected += value; } }
            remove { lock (m_DisconnectedLock) { m_Disconnected -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<SimChangedEventArgs> m_SimChanged;

        ///<summary>Raises the SimChanged Event</summary>
        /// <param name="e">A SimChangedEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnSimChanged(SimChangedEventArgs e)
        {
            EventHandler<SimChangedEventArgs> handler = m_SimChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_SimChangedLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<SimChangedEventArgs> SimChanged
        {
            add { lock (m_SimChangedLock) { m_SimChanged += value; } }
            remove { lock (m_SimChangedLock) { m_SimChanged -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<EventQueueRunningEventArgs> m_EventQueueRunning;

        ///<summary>Raises the EventQueueRunning Event</summary>
        /// <param name="e">A EventQueueRunningEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnEventQueueRunning(EventQueueRunningEventArgs e)
        {
            EventHandler<EventQueueRunningEventArgs> handler = m_EventQueueRunning;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_EventQueueRunningLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<EventQueueRunningEventArgs> EventQueueRunning
        {
            add { lock (m_EventQueueRunningLock) { m_EventQueueRunning += value; } }
            remove { lock (m_EventQueueRunningLock) { m_EventQueueRunning -= value; } }
        }

        #endregion Delegates

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
            RegisterCallback(PacketType.RegionHandshake, RegionHandshakeHandler);
            RegisterCallback(PacketType.StartPingCheck, StartPingCheckHandler);
            RegisterCallback(PacketType.DisableSimulator, DisableSimulatorHandler);
            RegisterCallback(PacketType.KickUser, KickUserHandler);
            RegisterCallback(PacketType.LogoutReply, LogoutReplyHandler);
            RegisterCallback(PacketType.CompletePingCheck, CompletePingCheckHandler);
            RegisterCallback(PacketType.SimStats, SimStatsHandler);

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
        public void RegisterCallback(PacketType type, EventHandler<PacketReceivedEventArgs> callback)
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
        public void UnregisterCallback(PacketType type, EventHandler<PacketReceivedEventArgs> callback)
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
            // try CurrentSim, however directly after login this will
            // be null, so if it is, we'll try to find the first simulator
            // we're connected to in order to send the packet.
            Simulator simulator = CurrentSim;

            if (simulator == null && Client.Network.Simulators.Count >= 1)
            {
                Logger.DebugLog("CurrentSim object was null, using first found connected simulator", Client);
                simulator = Client.Network.Simulators[0];
            }            

            if (simulator != null && simulator.Connected)
            {
                simulator.SendPacket(packet);
            }
            else
            {
                //throw new NotConnectedException("Packet received before simulator packet processing threads running, make certain you are completely logged in");
                Logger.Log("Packet received before simulator packet processing threads running, make certain you are completely logged in.", Helpers.LogLevel.Error);
            }
        }

        /// <summary>
        /// Send a packet to a specified simulator
        /// </summary>
        /// <param name="packet">Packet to send</param>
        /// <param name="simulator">Simulator to send the packet to</param>
        public void SendPacket(Packet packet, Simulator simulator)
        {
            if (simulator != null)
            {
                simulator.SendPacket(packet);
            }
            else
            {
                Logger.Log("Packet received before simulator packet processing threads running, make certain you are completely logged in", Helpers.LogLevel.Error);
            }
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
                    // 
                    connected = true;

                    // Open the queues in case this is a reconnect and they were shut down
                    PacketInbox.Open();
                    PacketOutbox.Open();

                    // Start the packet decoding thread
                    Thread decodeThread = new Thread(new ThreadStart(IncomingPacketHandler));
                    decodeThread.Name = "Incoming UDP packet dispatcher";
                    decodeThread.Start();

                    // Start the packet sending thread
                    Thread sendThread = new Thread(new ThreadStart(OutgoingPacketHandler));
                    sendThread.Name = "Outgoing UDP packet dispatcher";
                    sendThread.Start();                    
                }

                // raise the SimConnecting event and allow any event
                // subscribers to cancel the connection
                if (m_SimConnecting != null)
                {
                    SimConnectingEventArgs args = new SimConnectingEventArgs(simulator);
                    OnSimConnecting(args);

                    if (args.Cancel)
                    {
                        // Callback is requesting that we abort this connection
                        lock (Simulators)
                        {
                            Simulators.Remove(simulator);
                        }
                        return null;
                    }
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

                    if (setDefault)
                    {
                        SetCurrentSim(simulator, seedcaps);
                    }

                    // Raise the SimConnected event
                    if (m_SimConnected != null)
                    {
                        OnSimConnected(new SimConnectedEventArgs(simulator));
                    }
                    
                    // If enabled, send an AgentThrottle packet to the server to increase our bandwidth
                    if (Client.Settings.SEND_AGENT_THROTTLE)
                    {
                        Client.Throttle.Set(simulator);
                    }

                    return simulator;
                }
                else
                {
                    // Connection failed, remove this simulator from our list and destroy it
                    lock (Simulators)
                    {
                        Simulators.Remove(simulator);
                    }                    

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
                {
                    Client.Self.Movement.SendUpdate(true, simulator);
                }

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
            EventHandler<LoggedOutEventArgs> callback = delegate(object sender, LoggedOutEventArgs e) { logoutEvent.Set(); };

            LoggedOut += callback;

            // Send the packet requesting a clean logout
            RequestLogout();

            // Wait for a logout response. If the response is received, shutdown
            // will be fired in the callback. Otherwise we fire it manually with
            // a NetworkTimeout type
            if (!logoutEvent.WaitOne(Client.Settings.LOGOUT_TIMEOUT, false))
                Shutdown(DisconnectType.NetworkTimeout);

            LoggedOut -= callback;
        }

        /// <summary>
        /// Initiate the logout process. Check if logout succeeded with the
        /// <code>OnLogoutReply</code> event, and if this does not fire the
        /// <code>Shutdown()</code> function needs to be manually called
        /// </summary>
        public void RequestLogout()
        {
            // No need to run the disconnect timer any more
            if (DisconnectTimer != null)
            {
                DisconnectTimer.Dispose();
                DisconnectTimer = null;
            }

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
        /// Close a connection to the given simulator
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="sendCloseCircuit"></param>
        public void DisconnectSim(Simulator simulator, bool sendCloseCircuit)
        {
            if (simulator != null)
            {
                simulator.Disconnect(sendCloseCircuit);

                // Fire the SimDisconnected event if a handler is registered
                if (m_SimDisconnected != null)
                {
                    OnSimDisconnected(new SimDisconnectedEventArgs(simulator, DisconnectType.NetworkTimeout));
                }

                lock (Simulators) Simulators.Remove(simulator);

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
        /// <param name="type">Type of shutdown</param>
        public void Shutdown(DisconnectType type)
        {
            Shutdown(type, type.ToString());
        }

        /// <summary>
        /// Shutdown will disconnect all the sims except for the current sim
        /// first, and then kill the connection to CurrentSim. This should only
        /// be called if the logout process times out on <code>RequestLogout</code>
        /// </summary>
        /// <param name="type">Type of shutdown</param>
        /// <param name="message">Shutdown message</param>
        public void Shutdown(DisconnectType type, string message)
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
                        if (m_SimDisconnected != null)
                        {
                            OnSimDisconnected(new SimDisconnectedEventArgs(Simulators[i], type));
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
                if (m_SimDisconnected != null)
                {
                    OnSimDisconnected(new SimDisconnectedEventArgs(CurrentSim, type));
                }
            }

            // Clear out all of the packets that never had time to process
            PacketInbox.Close();
            PacketOutbox.Close();

            connected = false;

            // Fire the disconnected callback
            if (m_Disconnected != null)
            {
                OnDisconnected(new DisconnectedEventArgs(type, message));
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

        internal void RaisePacketSentEvent(byte[] data, int bytesSent, Simulator simulator)
        {
            if (m_PacketSent != null)
            {
                OnPacketSent(new PacketSentEventArgs(data, bytesSent, simulator));
            }
        }

        /// <summary>
        /// Fire an event when an event queue connects for capabilities
        /// </summary>
        /// <param name="simulator">Simulator the event queue is attached to</param>
        internal void RaiseConnectedEvent(Simulator simulator)
        {
            if (m_EventQueueRunning != null)
            {
                OnEventQueueRunning(new EventQueueRunningEventArgs(simulator));
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
                if (m_SimChanged != null && simulator != oldSim)
                {
                    OnSimChanged(new SimChangedEventArgs(oldSim));
                }
            }
        }

        #region Timers

        private void DisconnectTimer_Elapsed(object obj)
        {
            if (!connected || CurrentSim == null)
            {
                if (DisconnectTimer != null)
                {
                    DisconnectTimer.Dispose();
                    DisconnectTimer = null;
                }
                connected = false;
            }
            else if (CurrentSim.DisconnectCandidate)
            {
                // The currently occupied simulator hasn't sent us any traffic in a while, shutdown
                Logger.Log("Network timeout for the current simulator (" +
                    CurrentSim.ToString() + "), logging out", Helpers.LogLevel.Warning, Client);

                if (DisconnectTimer != null)
                {
                    DisconnectTimer.Dispose();
                    DisconnectTimer = null;
                }

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

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void LogoutReplyHandler(object sender, PacketReceivedEventArgs e)
        {
            LogoutReplyPacket logout = (LogoutReplyPacket)e.Packet;

            if ((logout.AgentData.SessionID == Client.Self.SessionID) && (logout.AgentData.AgentID == Client.Self.AgentID))
            {
                Logger.DebugLog("Logout reply received", Client);

                // Deal with callbacks, if any
                if (m_LoggedOut != null)
                {
                    List<UUID> itemIDs = new List<UUID>();

                    foreach (LogoutReplyPacket.InventoryDataBlock InventoryData in logout.InventoryData)
                    {
                        itemIDs.Add(InventoryData.ItemID);
                    }

                    OnLoggedOut(new LoggedOutEventArgs(itemIDs));
                }

                // If we are receiving a LogoutReply packet assume this is a client initiated shutdown
                Shutdown(DisconnectType.ClientInitiated);
            }
            else
            {
                Logger.Log("Invalid Session or Agent ID received in Logout Reply... ignoring", Helpers.LogLevel.Warning, Client);
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void StartPingCheckHandler(object sender, PacketReceivedEventArgs e)
        {
            StartPingCheckPacket incomingPing = (StartPingCheckPacket)e.Packet;
            CompletePingCheckPacket ping = new CompletePingCheckPacket();
            ping.PingID.PingID = incomingPing.PingID.PingID;
            ping.Header.Reliable = false;
            // TODO: We can use OldestUnacked to correct transmission errors
            //   I don't think that's right.  As far as I can tell, the Viewer
            //   only uses this to prune its duplicate-checking buffer. -bushing

            SendPacket(ping, e.Simulator);
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void CompletePingCheckHandler(object sender, PacketReceivedEventArgs e)
        {
            CompletePingCheckPacket pong = (CompletePingCheckPacket)e.Packet;
            String retval = "Pong2: " + (Environment.TickCount - e.Simulator.Stats.LastPingSent);
            if ((pong.PingID.PingID - e.Simulator.Stats.LastPingID + 1) != 0)
                retval += " (gap of " + (pong.PingID.PingID - e.Simulator.Stats.LastPingID + 1) + ")";

            e.Simulator.Stats.LastLag = Environment.TickCount - e.Simulator.Stats.LastPingSent;
            e.Simulator.Stats.ReceivedPongs++;
            //			Client.Log(retval, Helpers.LogLevel.Info);
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void SimStatsHandler(object sender, PacketReceivedEventArgs e)
        {
            if (!Client.Settings.ENABLE_SIMSTATS)
            {
                return;
            }
            SimStatsPacket stats = (SimStatsPacket)e.Packet;
            for (int i = 0; i < stats.Stat.Length; i++)
            {
                SimStatsPacket.StatBlock s = stats.Stat[i];
                switch (s.StatID)
                {
                    case 0:
                        e.Simulator.Stats.Dilation = s.StatValue;
                        break;
                    case 1:
                        e.Simulator.Stats.FPS = Convert.ToInt32(s.StatValue);
                        break;
                    case 2:
                        e.Simulator.Stats.PhysicsFPS = s.StatValue;
                        break;
                    case 3:
                        e.Simulator.Stats.AgentUpdates = s.StatValue;
                        break;
                    case 4:
                        e.Simulator.Stats.FrameTime = s.StatValue;
                        break;
                    case 5:
                        e.Simulator.Stats.NetTime = s.StatValue;
                        break;
                    case 6:
                        e.Simulator.Stats.OtherTime = s.StatValue;
                        break;
                    case 7:
                        e.Simulator.Stats.PhysicsTime = s.StatValue;
                        break;
                    case 8:
                        e.Simulator.Stats.AgentTime = s.StatValue;
                        break;
                    case 9:
                        e.Simulator.Stats.ImageTime = s.StatValue;
                        break;
                    case 10:
                        e.Simulator.Stats.ScriptTime = s.StatValue;
                        break;
                    case 11:
                        e.Simulator.Stats.Objects = Convert.ToInt32(s.StatValue);
                        break;
                    case 12:
                        e.Simulator.Stats.ScriptedObjects = Convert.ToInt32(s.StatValue);
                        break;
                    case 13:
                        e.Simulator.Stats.Agents = Convert.ToInt32(s.StatValue);
                        break;
                    case 14:
                        e.Simulator.Stats.ChildAgents = Convert.ToInt32(s.StatValue);
                        break;
                    case 15:
                        e.Simulator.Stats.ActiveScripts = Convert.ToInt32(s.StatValue);
                        break;
                    case 16:
                        e.Simulator.Stats.LSLIPS = Convert.ToInt32(s.StatValue);
                        break;
                    case 17:
                        e.Simulator.Stats.INPPS = Convert.ToInt32(s.StatValue);
                        break;
                    case 18:
                        e.Simulator.Stats.OUTPPS = Convert.ToInt32(s.StatValue);
                        break;
                    case 19:
                        e.Simulator.Stats.PendingDownloads = Convert.ToInt32(s.StatValue);
                        break;
                    case 20:
                        e.Simulator.Stats.PendingUploads = Convert.ToInt32(s.StatValue);
                        break;
                    case 21:
                        e.Simulator.Stats.VirtualSize = Convert.ToInt32(s.StatValue);
                        break;
                    case 22:
                        e.Simulator.Stats.ResidentSize = Convert.ToInt32(s.StatValue);
                        break;
                    case 23:
                        e.Simulator.Stats.PendingLocalUploads = Convert.ToInt32(s.StatValue);
                        break;
                    case 24:
                        e.Simulator.Stats.UnackedBytes = Convert.ToInt32(s.StatValue);
                        break;
                }
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void RegionHandshakeHandler(object sender, PacketReceivedEventArgs e)
        {
            RegionHandshakePacket handshake = (RegionHandshakePacket)e.Packet;
            Simulator simulator = e.Simulator;
            e.Simulator.ID = handshake.RegionInfo.CacheID;

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

        protected void EnableSimulatorHandler(string capsKey, IMessage message, Simulator simulator)
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

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void DisableSimulatorHandler(object sender, PacketReceivedEventArgs e)
        {
            DisconnectSim(e.Simulator, false);
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void KickUserHandler(object sender, PacketReceivedEventArgs e)
        {
            string message = Utils.BytesToString(((KickUserPacket)e.Packet).UserInfo.Reason);

            // Shutdown the network layer
            Shutdown(DisconnectType.ServerInitiated, message);
        }

        #endregion Packet Callbacks
    }
    #region EventArgs

    public class PacketReceivedEventArgs : EventArgs
    {
        private readonly Packet m_Packet;
        private readonly Simulator m_Simulator;

        public Packet Packet { get { return m_Packet; } }
        public Simulator Simulator { get { return m_Simulator; } }

        public PacketReceivedEventArgs(Packet packet, Simulator simulator)
        {
            this.m_Packet = packet;
            this.m_Simulator = simulator;
        }
    }

    public class LoggedOutEventArgs : EventArgs
    {
        private readonly List<UUID> m_InventoryItems;
        public List<UUID> InventoryItems;

        public LoggedOutEventArgs(List<UUID> inventoryItems)
        {
            this.m_InventoryItems = inventoryItems;
        }
    }

    public class PacketSentEventArgs : EventArgs
    {
        private readonly byte[] m_Data;
        private readonly int m_SentBytes;
        private readonly Simulator m_Simulator;

        public byte[] Data { get { return m_Data; } }
        public int SentBytes { get { return m_SentBytes; } }
        public Simulator Simulator { get { return m_Simulator; } }

        public PacketSentEventArgs(byte[] data, int bytesSent, Simulator simulator)
        {
            this.m_Data = data;
            this.m_SentBytes = bytesSent;
            this.m_Simulator = simulator;
        }
    }

    public class SimConnectingEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private bool m_Cancel;

        public Simulator Simulator { get { return m_Simulator; } }

        public bool Cancel
        {
            get { return m_Cancel; }
            set { m_Cancel = value; }
        }

        public SimConnectingEventArgs(Simulator simulator)
        {
            this.m_Simulator = simulator;
            this.m_Cancel = false;
        }
    }

    public class SimConnectedEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        public Simulator Simulator { get { return m_Simulator; } }

        public SimConnectedEventArgs(Simulator simulator)
        {
            this.m_Simulator = simulator;
        }
    }

    public class SimDisconnectedEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly NetworkManager.DisconnectType m_Reason;

        public Simulator Simulator { get { return m_Simulator; } }
        public NetworkManager.DisconnectType Reason { get { return m_Reason; } }

        public SimDisconnectedEventArgs(Simulator simulator, NetworkManager.DisconnectType reason)
        {
            this.m_Simulator = simulator;
            this.m_Reason = reason;
        }
    }

    public class DisconnectedEventArgs : EventArgs
    {
        private readonly NetworkManager.DisconnectType m_Reason;
        private readonly String m_Message;

        public NetworkManager.DisconnectType Reason { get { return m_Reason; } }
        public String Message { get { return m_Message; } }

        public DisconnectedEventArgs(NetworkManager.DisconnectType reason, String message)
        {
            this.m_Reason = reason;
            this.m_Message = message;
        }
    }

    public class SimChangedEventArgs : EventArgs
    {
        private readonly Simulator m_PreviousSimulator;

        public Simulator PreviousSimulator { get { return m_PreviousSimulator; } }

        public SimChangedEventArgs(Simulator previousSimulator)
        {
            this.m_PreviousSimulator = previousSimulator;
        }
    }

    public class EventQueueRunningEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;

        public Simulator Simulator { get { return m_Simulator; } }

        public EventQueueRunningEventArgs(Simulator simulator)
        {
            this.m_Simulator = simulator;
        }
    }
    #endregion
}
