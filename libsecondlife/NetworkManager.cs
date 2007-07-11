/*
 * Copyright (c) 2006-2007, Second Life Reverse Engineering Team
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
using System.Net;
using System.Net.Sockets;
using System.Globalization;
using System.IO;
using libsecondlife.Packets;

namespace libsecondlife
{
    /// <summary>
    /// This exception is thrown whenever a network operation is attempted 
    /// without a network connection.
    /// </summary>
    public class NotConnectedException : ApplicationException { }

    /// <summary>
    /// NetworkManager is responsible for managing the network layer of 
    /// libsecondlife. It tracks all the server connections, serializes 
    /// outgoing traffic and deserializes incoming traffic, and provides
    /// instances of delegates for network-related events.
    /// </summary>
    public partial class NetworkManager
    {
        /// <summary>
        /// Holds a simulator reference and a packet, these structs are put in
        /// the packet inbox for decoding
        /// </summary>
        public struct IncomingPacket
        {
            /// <summary>Reference to the simulator that this packet came from</summary>
            public Simulator Simulator;
            /// <summary>The packet that needs to be processed</summary>
            public Packet Packet;
        }

        /// <summary>
        /// Object that is passed to worker threads in the ThreadPool for
        /// firing packet callbacks
        /// </summary>
        private struct PacketCallbackWrapper
        {
            /// <summary>Callback to fire for this packet</summary>
            public PacketCallback Callback;
            /// <summary>Reference to the simulator that this packet came from</summary>
            public Simulator Simulator;
            /// <summary>The packet that needs to be processed</summary>
            public Packet Packet;
        }

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
        /// An event for the connection to a simulator other than the currently
        /// occupied one disconnecting
        /// </summary>
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
        /// <summary>The simulator that the logged in avatar is currently 
        /// occupying</summary>
        public Simulator CurrentSim = null;
        /// <summary>All of the simulators we are currently connected to</summary>
        public List<Simulator> Simulators = new List<Simulator>();

        /// <summary>
        /// Shows whether the network layer is logged in to the grid or not
        /// </summary>
        public bool Connected { get { return connected; } }
        public int InboxCount { get { return PacketInbox.Count; } }

        /// <summary></summary>
        internal List<Caps.EventQueueCallback> EventQueueCallbacks = new List<Caps.EventQueueCallback>();
        /// <summary>Incoming packets that are awaiting handling</summary>
        internal BlockingQueue PacketInbox = new BlockingQueue(Settings.PACKET_INBOX_SIZE);

        private SecondLife Client;
        private Dictionary<PacketType, List<PacketCallback>> Callbacks = new Dictionary<PacketType, List<PacketCallback>>();
        private System.Timers.Timer DisconnectTimer, LogoutTimer;
        private bool connected = false;
        private ManualResetEvent LogoutReplyEvent = new ManualResetEvent(false);


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
            RegisterCallback(PacketType.CompletePingCheck, new PacketCallback(PongHandler));
			RegisterCallback(PacketType.SimStats, new PacketCallback(SimStatsHandler));
			
            // The proper timeout for this will get set again at Login
            DisconnectTimer = new System.Timers.Timer();
            DisconnectTimer.Elapsed += new ElapsedEventHandler(DisconnectTimer_Elapsed);

            // Don't force Expect-100: Continue headers on HTTP POST calls
            ServicePointManager.Expect100Continue = false;

            // Catch exceptions from threads in the managed threadpool
            Toub.Threading.ManagedThreadPool.UnhandledException += 
                new UnhandledExceptionEventHandler(ManagedThreadPool_UnhandledException);
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
            lock (EventQueueCallbacks) EventQueueCallbacks.Add(callback);
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
            IPEndPoint endPoint = new IPEndPoint(ip, (int)port);
            Simulator simulator = FindSimulator(endPoint);

            if (simulator == null)
            {
                // We're not tracking this sim, create a new Simulator object
                simulator = new Simulator(Client, endPoint);

                // Immediately add this simulator to the list of current sims. It will be removed if the
                // connection fails
                lock (Simulators) Simulators.Add(simulator);
            }

            if (!simulator.Connected)
            {
                if (!connected)
                {
                    // Mark that we are connected to the grid
                    // HACK: This sucks but right now decodeThread loops while connected 
                    // is true, so we have to be "connected" before we start connecting
                    connected = true;

                    // Start the packet decoding thread
                    Thread decodeThread = new Thread(new ThreadStart(PacketHandler));
                    decodeThread.Start();
                }

                // We're not connected to this simulator, attempt to establish a connection
                if (simulator.Connect(setDefault))
                {
                    // Start a timer that checks if we've been disconnected
                    DisconnectTimer.Start();

                    // If enabled, send an AgentThrottle packet to the server to increase our bandwidth
                    if (Client.Settings.SEND_AGENT_THROTTLE) Client.Throttle.Set(simulator);

                    if (setDefault) SetCurrentSim(simulator, seedcaps);
                }
                else
                {
                    // Connection failed, so remove this simulator from our list and destroy it
                    lock (Simulators) Simulators.Remove(simulator);
                    simulator = null;
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
                    Client.Self.Status.SendUpdate(true, simulator);
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
            // LogoutDemandPacket logoutDemand = new LogoutDemandPacket(); // FIXME: packet is no more
            // logoutDemand.LogoutBlock.SessionID = SessionID;
            // CurrentSim.SendPacket(logoutDemand, true);

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

                if (Simulators.Count == 0) Shutdown(DisconnectType.SimShutdown);
            }
            else
            {
                Client.Log("DisconnectSim() called with a null Simulator reference", Helpers.LogLevel.Warning);
            }
        }

        private void PacketHandler()
        {
            IncomingPacket incomingPacket = new IncomingPacket();
            Packet packet = null;
            Simulator simulator = null;
            WaitCallback callback = new WaitCallback(CallPacketDelegate);

            while (connected)
            {
                // Reset packet to null for the check below
                packet = null;

                if (PacketInbox.Dequeue(500, ref incomingPacket))
                {
                    packet = incomingPacket.Packet;
                    simulator = incomingPacket.Simulator;

                    if (packet != null)
                    {
                        #region Archive Duplicate Search

                        // TODO: Replace PacketArchive Queue<> with something more efficient

                        // Check the archives to see whether we already received this packet
                        lock (simulator.PacketArchive)
                        {
                            if (simulator.PacketArchive.Contains(packet.Header.Sequence))
                            {
                                if (packet.Header.Resent)
                                {
                                    Client.DebugLog("Received resent packet #" + packet.Header.Sequence);
                                }
                                else
                                {
                                    Client.Log("Received a duplicate " + packet.Type.ToString() + " packet!",
                                        Helpers.LogLevel.Error);
                                }

                                // Avoid firing a callback twice for the same packet
                                goto End;
                            }
                            else
                            {
                                // Keep the Inbox size within a certain capacity
                                while (simulator.PacketArchive.Count >= Settings.PACKET_ARCHIVE_SIZE)
                                {
                                    simulator.PacketArchive.Dequeue(); simulator.PacketArchive.Dequeue();
                                    simulator.PacketArchive.Dequeue(); simulator.PacketArchive.Dequeue();
                                }

                                simulator.PacketArchive.Enqueue(packet.Header.Sequence);
                            }
                        }

                        #endregion Archive Duplicate Search

                        #region ACK handling

                        // Handle appended ACKs
                        if (packet.Header.AppendedAcks)
                        {
                            lock (simulator.NeedAck)
                            {
                                for (int i = 0; i < packet.Header.AckList.Length; i++)
                                    simulator.NeedAck.Remove(packet.Header.AckList[i]);
                            }
                        }

                        // Handle PacketAck packets
                        if (packet.Type == PacketType.PacketAck)
                        {
                            PacketAckPacket ackPacket = (PacketAckPacket)packet;

                            lock (simulator.NeedAck)
                            {
                                for (int i = 0; i < ackPacket.Packets.Length; i++)
                                    simulator.NeedAck.Remove(ackPacket.Packets[i].ID);
                            }
                        }

                        #endregion ACK handling

                        #region FireCallbacks

                        if (Callbacks.ContainsKey(packet.Type))
                        {
                            List<NetworkManager.PacketCallback> callbackArray = Callbacks[packet.Type];

                            // Fire any registered callbacks
                            for (int i = 0; i < callbackArray.Count; i++)
                            {
                                if (callbackArray[i] != null)
                                {
                                    bool sync = Client.Settings.SYNC_PACKETCALLBACKS;
                                    if (sync)
                                    {
                                        callbackArray[i](packet, simulator);
                                    }
                                    else
                                    {
                                        PacketCallbackWrapper wrapper;
                                        wrapper.Callback = callbackArray[i];
                                        wrapper.Packet = packet;
                                        wrapper.Simulator = simulator;
                                        Toub.Threading.ManagedThreadPool.QueueUserWorkItem(callback, wrapper);
                                    }
                                }
                            }
                        }

                        if (Callbacks.ContainsKey(PacketType.Default))
                        {
                            List<NetworkManager.PacketCallback> callbackArray = Callbacks[PacketType.Default];

                            // Fire any registered callbacks
                            for (int i = 0; i < callbackArray.Count; i++)
                            {
                                if (callbackArray[i] != null)
                                {
                                    bool sync = Client.Settings.SYNC_PACKETCALLBACKS;
                                    if (sync)
                                    {
                                        callbackArray[i](packet, simulator);
                                    }
                                    else
                                    {
                                        PacketCallbackWrapper wrapper;
                                        wrapper.Callback = callbackArray[i];
                                        wrapper.Packet = packet;
                                        wrapper.Simulator = simulator;
                                        Toub.Threading.ManagedThreadPool.QueueUserWorkItem(callback, wrapper);
                                    }
                                }
                            }
                        }

                        #endregion FireCallbacks

                    End: ;
                    }
                }
            }
        }

        private void CallPacketDelegate(Object state)
        {
            PacketCallbackWrapper wrapper = (PacketCallbackWrapper)state;
            wrapper.Callback(wrapper.Packet, wrapper.Simulator);
        }

        private void SetCurrentSim(Simulator simulator, string seedcaps)
        {
            if (simulator != CurrentSim)
            {
                Simulator oldSim = CurrentSim;
                lock (Simulators) CurrentSim = simulator; // CurrentSim is synchronized against Simulators

		simulator.setSeedCaps(seedcaps);

                // If the current simulator changed fire the callback
                if (OnCurrentSimChanged != null && simulator != oldSim)
                {
                    try { OnCurrentSimChanged(oldSim); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
            }
        }

        /// <summary>
        /// Finalize the logout procedure. Close down sockets, etc.
        /// </summary>
        private void FinalizeLogout()
        {
            LogoutTimer.Stop();

            // Shutdown the network layer
            Shutdown(DisconnectType.ClientInitiated);

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
        private void Shutdown(DisconnectType type)
        {
            Client.Log("NetworkManager shutdown initiated", Helpers.LogLevel.Info);

            lock (Simulators)
            {
                // Disconnect all simulators except the current one
                for (int i = 0; i < Simulators.Count; i++)
                {
                    if (Simulators[i] != null && Simulators[i] != CurrentSim)
                    {
                        Simulators[i].Disconnect();

                        // Fire the SimDisconnected event if a handler is registered
                        // FIXME: This is a recipe for disaster, locking Simulators and
                        // firing a callback
                        if (OnSimDisconnected != null)
                        {
                            try { OnSimDisconnected(Simulators[i], type); }
                            catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                        }
                    }
                }

                Simulators.Clear();
            }

            if (CurrentSim != null)
            {
                // Kill the connection to the curent simulator
                CurrentSim.Disconnect();

                // Fire the SimDisconnected event if a handler is registered
                if (OnSimDisconnected != null)
                {
                    try { OnSimDisconnected(CurrentSim, type); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }

                // Destroy the CurrentSim object
                lock (Simulators) CurrentSim = null;
            }


            // Clear out all of the packets that never had time to process
            lock (PacketInbox) PacketInbox.Clear();

            connected = false;
        }

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

        private void ManagedThreadPool_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // An exception occurred in a packet callback, log it
            Client.Log(((Exception)e.ExceptionObject).ToString(), Helpers.LogLevel.Error);
        }

        #region Timers

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
                    Shutdown(DisconnectType.NetworkTimeout);

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
            ping.Header.Reliable = false;
            // TODO: We can use OldestUnacked to correct transmission errors
            //   I don't think that's right.  As far as I can tell, the Viewer
            //   only uses this to prune its duplicate-checking buffer. -bushing

            SendPacket(ping, simulator);
        }

        private void PongHandler(Packet packet, Simulator simulator)
        {
            CompletePingCheckPacket pong = (CompletePingCheckPacket)packet;
            String retval = "Pong2: " + (Environment.TickCount - simulator.LastPingSent);
            if ((pong.PingID.PingID - simulator.LastPingID + 1) != 0)
                retval += " (gap of " + (pong.PingID.PingID - simulator.LastPingID + 1) + ")";

            simulator.LastLag = Environment.TickCount - simulator.LastPingSent;
            simulator.ReceivedPongs++;
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
						simulator.Dilation = s.StatValue;
						break;
					case 1:
						simulator.FPS = Convert.ToInt32(s.StatValue);
						break;
					case 2:
						simulator.PhysicsFPS = s.StatValue;
						break;
					case 3:
						simulator.AgentUpdates = s.StatValue;
						break;
					case 4:
						simulator.FrameTime = s.StatValue;
						break;
					case 5:
						simulator.NetTime = s.StatValue;
						break;
					case 7:
						simulator.PhysicsTime = s.StatValue;
						break;
					case 8:
						simulator.ImageTime = s.StatValue;
						break;
					case 9:
						simulator.ScriptTime = s.StatValue;
						break;
					case 10:
						simulator.OtherTime = s.StatValue;
						break;
					case 11:
						simulator.Objects = Convert.ToInt32(s.StatValue);
						break;
					case 12:
						simulator.ScriptedObjects = Convert.ToInt32(s.StatValue);
						break;
					case 13:
						simulator.Agents = Convert.ToInt32(s.StatValue);
						break;
					case 14:
						simulator.ChildAgents = Convert.ToInt32(s.StatValue);
						break;
					case 15:
						simulator.ActiveScripts = Convert.ToInt32(s.StatValue);
						break;
					case 16:
						simulator.LSLIPS = Convert.ToInt32(s.StatValue);
						break;
					case 17:
						simulator.INPPS = Convert.ToInt32(s.StatValue);
						break;
					case 18:
						simulator.OUTPPS = Convert.ToInt32(s.StatValue);
						break;
					case 19:
						simulator.PendingDownloads = Convert.ToInt32(s.StatValue);
						break;
					case 20:
						simulator.PendingUploads = Convert.ToInt32(s.StatValue);
						break;
					case 21:
						simulator.VirtualSize = Convert.ToInt32(s.StatValue);
						break;
					case 22:
						simulator.ResidentSize = Convert.ToInt32(s.StatValue);
						break;
					case 23:
						simulator.PendingLocalUploads = Convert.ToInt32(s.StatValue);
						break;
					case 24:
						simulator.UnackedBytes = Convert.ToInt32(s.StatValue);
						break;
				}
			}
		}

        private void RegionHandshakeHandler(Packet packet, Simulator simulator)
        {
            RegionHandshakePacket handshake = (RegionHandshakePacket)packet;

            simulator.ID = handshake.RegionInfo.CacheID;

            simulator.IsEstateManager = handshake.RegionInfo.IsEstateManager;
            simulator.Name = Helpers.FieldToUTF8String(handshake.RegionInfo.SimName);
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
            simulator.Flags = (Simulator.RegionFlags)handshake.RegionInfo.RegionFlags;
            simulator.BillableFactor = handshake.RegionInfo.BillableFactor;
            simulator.Access = (Simulator.SimAccess)handshake.RegionInfo.SimAccess;

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
                Buffer.BlockCopy(overlay.ParcelData.Data, 0, simulator.ParcelOverlay, 
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
            if (!Client.Settings.MULTIPLE_SIMS) return;

            EnableSimulatorPacket p = (EnableSimulatorPacket)packet;
            IPEndPoint endPoint = new IPEndPoint(p.SimulatorInfo.IP, p.SimulatorInfo.Port);

            // First, check to see if we've already started connecting to this sim
            if (FindSimulator(endPoint) != null) return;

            IPAddress address = new IPAddress(p.SimulatorInfo.IP);
            if (Connect(address, p.SimulatorInfo.Port, false, LoginSeedCapability) == null)
            {
                Client.Log("Unabled to connect to new sim " + address + ":" + p.SimulatorInfo.Port, 
                    Helpers.LogLevel.Error);
                return;
            }
        }

        private void KickUserHandler(Packet packet, Simulator simulator)
        {
            string message = Helpers.FieldToUTF8String(((KickUserPacket)packet).UserInfo.Reason);

            // Shutdown the network layer
            Shutdown(DisconnectType.ServerInitiated);

            if (OnDisconnected != null)
            {
                try { OnDisconnected(DisconnectType.ServerInitiated, message); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        #endregion Packet Callbacks
    }
}
