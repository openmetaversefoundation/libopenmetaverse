/*
 * Copyright (c) 2007-2008, openmetaverse.org
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
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using OpenMetaverse.Packets;

namespace OpenMetaverse
{
    /// <summary>
    /// 
    /// </summary>
    public class Simulator : UDPBase, IDisposable
    {
        #region Enums

        /// <summary>
        /// Simulator (region) properties
        /// </summary>
        [Flags]
        public enum RegionFlags
        {
            /// <summary>No flags set</summary>
            None = 0,
            /// <summary>Agents can take damage and be killed</summary>
            AllowDamage = 1 << 0,
            /// <summary>Landmarks can be created here</summary>
            AllowLandmark = 1 << 1,
            /// <summary>Home position can be set in this sim</summary>
            AllowSetHome = 1 << 2,
            /// <summary>Home position is reset when an agent teleports away</summary>
            ResetHomeOnTeleport = 1 << 3,
            /// <summary>Sun does not move</summary>
            SunFixed = 1 << 4,
            /// <summary>No object, land, etc. taxes</summary>
            TaxFree = 1 << 5,
            /// <summary>Disable heightmap alterations (agents can still plant
            /// foliage)</summary>
            BlockTerraform = 1 << 6,
            /// <summary>Land cannot be released, sold, or purchased</summary>
            BlockLandResell = 1 << 7,
            /// <summary>All content is wiped nightly</summary>
            Sandbox = 1 << 8,
            /// <summary></summary>
            NullLayer = 1 << 9,
            /// <summary></summary>
            SkipAgentAction = 1 << 10,
            /// <summary></summary>
            SkipUpdateInterestList = 1 << 11,
            /// <summary>No collision detection for non-agent objects</summary>
            SkipCollisions = 1 << 12,
            /// <summary>No scripts are ran</summary>
            SkipScripts = 1 << 13,
            /// <summary>All physics processing is turned off</summary>
            SkipPhysics = 1 << 14,
            /// <summary></summary>
            ExternallyVisible = 1 << 15,
            /// <summary></summary>
            MainlandVisible = 1 << 16,
            /// <summary></summary>
            PublicAllowed = 1 << 17,
            /// <summary></summary>
            BlockDwell = 1 << 18,
            /// <summary>Flight is disabled (not currently enforced by the sim)</summary>
            NoFly = 1 << 19,
            /// <summary>Allow direct (p2p) teleporting</summary>
            AllowDirectTeleport = 1 << 20,
            /// <summary>Estate owner has temporarily disabled scripting</summary>
            EstateSkipScripts = 1 << 21,
            /// <summary></summary>
            RestrictPushObject = 1 << 22,
            /// <summary>Deny agents with no payment info on file</summary>
            DenyAnonymous = 1 << 23,
            /// <summary>Deny agents with payment info on file</summary>
            DenyIdentified = 1 << 24,
            /// <summary>Deny agents who have made a monetary transaction</summary>
            DenyTransacted = 1 << 25,
            /// <summary></summary>
            AllowParcelChanges = 1 << 26,
            /// <summary></summary>
            AbuseEmailToEstateOwner = 1 << 27,
            /// <summary>Region is Voice Enabled</summary>
            AllowVoice = 1 << 28
        }

        /// <summary>
        /// 
        /// </summary>
        public enum SimAccess : byte
        {
            /// <summary></summary>
            Min = 0,
            /// <summary></summary>
            Trial = 7,
            /// <summary></summary>
            PG = 13,
            /// <summary></summary>
            Mature = 21,
            /// <summary></summary>
            Down = 254,
            /// <summary></summary>
            NonExistent = 255
        }

        #endregion Enums

        #region Structs
        /// <summary>
        /// Simulator Statistics
        /// </summary>
        public struct SimStats
        {
            /// <summary>Total number of packets sent by this simulator to this agent</summary>
            public ulong SentPackets;
            /// <summary>Total number of packets received by this simulator to this agent</summary>
            public ulong RecvPackets;
            /// <summary>Total number of bytes sent by this simulator to this agent</summary>
            public ulong SentBytes;
            /// <summary>Total number of bytes received by this simulator to this agent</summary>
            public ulong RecvBytes;
            /// <summary>Time in seconds agent has been connected to simulator</summary>
            public int ConnectTime;
            /// <summary>Total number of packets that have been resent</summary>
            public int ResentPackets;
            /// <summary>Total number of resent packets recieved</summary>
            public int ReceivedResends;
            /// <summary>Total number of pings sent to this simulator by this agent</summary>
            public int SentPings;
            /// <summary>Total number of ping replies sent to this agent by this simulator</summary>
            public int ReceivedPongs;
            /// <summary>
            /// Incoming bytes per second
            /// </summary>
            /// <remarks>It would be nice to have this claculated on the fly, but
            /// this is far, far easier</remarks>
            public int IncomingBPS;
            /// <summary>
            /// Outgoing bytes per second
            /// </summary>
            /// <remarks>It would be nice to have this claculated on the fly, but
            /// this is far, far easier</remarks>
            public int OutgoingBPS;
            /// <summary>Time last ping was sent</summary>
            public int LastPingSent;
            /// <summary>ID of last Ping sent</summary>
            public byte LastPingID;
            /// <summary></summary>
            public int LastLag;
            /// <summary></summary>
            public int MissedPings;
            /// <summary>Current time dilation of this simulator</summary>
            public float Dilation;
            /// <summary>Current Frames per second of simulator</summary>
            public int FPS;
            /// <summary>Current Physics frames per second of simulator</summary>
            public float PhysicsFPS;
            /// <summary></summary>
            public float AgentUpdates;
            /// <summary></summary>
            public float FrameTime;
            /// <summary></summary>
            public float NetTime;
            /// <summary></summary>
            public float PhysicsTime;
            /// <summary></summary>
            public float ImageTime;
            /// <summary></summary>
            public float ScriptTime;
            /// <summary></summary>
            public float AgentTime;
            /// <summary></summary>
            public float OtherTime;
            /// <summary>Total number of objects Simulator is simulating</summary>
            public int Objects;
            /// <summary>Total number of Active (Scripted) objects running</summary>
            public int ScriptedObjects;
            /// <summary>Number of agents currently in this simulator</summary>
            public int Agents;
            /// <summary>Number of agents in neighbor simulators</summary>
            public int ChildAgents;
            /// <summary>Number of Active scripts running in this simulator</summary>
            public int ActiveScripts;
            /// <summary></summary>
            public int LSLIPS;
            /// <summary></summary>
            public int INPPS;
            /// <summary></summary>
            public int OUTPPS;
            /// <summary>Number of downloads pending</summary>
            public int PendingDownloads;
            /// <summary>Number of uploads pending</summary>
            public int PendingUploads;
            /// <summary></summary>
            public int VirtualSize;
            /// <summary></summary>
            public int ResidentSize;
            /// <summary>Number of local uploads pending</summary>
            public int PendingLocalUploads;
            /// <summary>Unacknowledged bytes in queue</summary>
            public int UnackedBytes;
        }

        #endregion Structs

        #region Public Members

        /// <summary>A public reference to the client that this Simulator object
        /// is attached to</summary>
        public GridClient Client;
        /// <summary></summary>
        public UUID ID = UUID.Zero;
        /// <summary>The capabilities for this simulator</summary>
        public Caps Caps = null;
        /// <summary></summary>
        public ulong Handle;
        /// <summary>The current version of software this simulator is running</summary>
        public string SimVersion = String.Empty;
        /// <summary></summary>
        public string Name = String.Empty;
        /// <summary></summary>
        public byte[] ParcelOverlay = new byte[4096];
        /// <summary></summary>
        public int ParcelOverlaysReceived;
        /// <summary></summary>
        public float TerrainHeightRange00;
        /// <summary></summary>
        public float TerrainHeightRange01;
        /// <summary></summary>
        public float TerrainHeightRange10;
        /// <summary></summary>
        public float TerrainHeightRange11;
        /// <summary></summary>
        public float TerrainStartHeight00;
        /// <summary></summary>
        public float TerrainStartHeight01;
        /// <summary></summary>
        public float TerrainStartHeight10;
        /// <summary></summary>
        public float TerrainStartHeight11;
        /// <summary></summary>
        public float WaterHeight;
        /// <summary></summary>
        public UUID SimOwner = UUID.Zero;
        /// <summary></summary>
        public UUID TerrainBase0 = UUID.Zero;
        /// <summary></summary>
        public UUID TerrainBase1 = UUID.Zero;
        /// <summary></summary>
        public UUID TerrainBase2 = UUID.Zero;
        /// <summary></summary>
        public UUID TerrainBase3 = UUID.Zero;
        /// <summary></summary>
        public UUID TerrainDetail0 = UUID.Zero;
        /// <summary></summary>
        public UUID TerrainDetail1 = UUID.Zero;
        /// <summary></summary>
        public UUID TerrainDetail2 = UUID.Zero;
        /// <summary></summary>
        public UUID TerrainDetail3 = UUID.Zero;
        /// <summary></summary>
        public bool IsEstateManager;
        /// <summary></summary>
        public EstateTools Estate;
        /// <summary></summary>
        public RegionFlags Flags;
        /// <summary></summary>
        public SimAccess Access;
        /// <summary></summary>
        public float BillableFactor;
        /// <summary>Statistics information for this simulator and the
        /// connection to the simulator, calculated by the simulator itself
        /// and the library</summary>
        public SimStats Stats;

        /// <summary>Provides access to two thread-safe dictionaries containing
        /// avatars and primitives found in this simulator</summary>
        //public ObjectTracker Objects = new ObjectTracker();

        public InternalDictionary<uint, Avatar> ObjectsAvatars = new InternalDictionary<uint, Avatar>();

        public InternalDictionary<uint, Primitive> ObjectsPrimitives = new InternalDictionary<uint, Primitive>();

        /// <summary>The current sequence number for packets sent to this
        /// simulator. Must be Interlocked before modifying. Only
        /// useful for applications manipulating sequence numbers</summary>
        public int Sequence;

        /// <summary>
        /// Provides access to an internal thread-safe dictionary containing parcel
        /// information found in this simulator
        /// </summary>
        public InternalDictionary<int, Parcel> Parcels = new InternalDictionary<int, Parcel>();

        /// <summary>
        /// Provides access to an internal thread-safe multidimensional array containing a x,y grid mapped
        /// each 64x64 parcel's LocalID.
        /// </summary>
        public int[,] ParcelMap
        {
            get
            {
                lock (this)
                    return _ParcelMap;
            }
            set
            {
                lock (this)
                    _ParcelMap = value;
            }
        }

        /// <summary>
        /// Checks simulator parcel map to make sure it has downloaded all data successfully
        /// </summary>
        /// <returns>true if map is full (contains no 0's)</returns>
        public bool IsParcelMapFull()
        {
            int ny = this.ParcelMap.GetLength(0);
            int nx = this.ParcelMap.GetLength(1);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (this.ParcelMap[y, x] == 0)
                        return false;
                }
            }
            return true;
        }

        #endregion Public Members

        #region Properties

        /// <summary>The IP address and port of the server</summary>
        public IPEndPoint IPEndPoint { get { return ipEndPoint; } }
        /// <summary>Whether there is a working connection to the simulator or 
        /// not</summary>
        public bool Connected { get { return connected; } }
        /// <summary>Coarse locations of avatars in this simulator</summary>
        public List<Vector3> AvatarPositions { get { return avatarPositions; } }
        /// <summary>AvatarPositions index representing your avatar</summary>
        public int PositionIndexYou { get { return positionIndexYou; } }
        /// <summary>AvatarPositions index representing TrackAgent target</summary>
        public int PositionIndexPrey { get { return positionIndexPrey; } }

        #endregion Properties

        #region Internal/Private Members
        /// <summary>Used internally to track sim disconnections</summary>
        internal bool DisconnectCandidate = false;
        /// <summary>Event that is triggered when the simulator successfully
        /// establishes a connection</summary>
        internal AutoResetEvent ConnectedEvent = new AutoResetEvent(false);
        /// <summary>Whether this sim is currently connected or not. Hooked up
        /// to the property Connected</summary>
        internal bool connected;
        /// <summary>Coarse locations of avatars in this simulator</summary>
        internal List<Vector3> avatarPositions = new List<Vector3>();
        /// <summary>AvatarPositions index representing your avatar</summary>
        internal int positionIndexYou = -1;
        /// <summary>AvatarPositions index representing TrackAgent target</summary>
        internal int positionIndexPrey = -1;
        /// <summary>Sequence numbers of packets we've received
        /// (for duplicate checking)</summary>
        internal Queue<uint> PacketArchive;
        /// <summary>Packets we sent out that need ACKs from the simulator</summary>
        internal Dictionary<uint, Packet> NeedAck = new Dictionary<uint, Packet>();

        private NetworkManager Network;
        private Queue<ulong> InBytes, OutBytes;
        // ACKs that are queued up to be sent to the simulator
        private SortedList<uint, uint> PendingAcks = new SortedList<uint, uint>();
        private IPEndPoint ipEndPoint;
        private Timer AckTimer;
        private Timer PingTimer;
        private Timer StatsTimer;
        // simulator <> parcel LocalID Map
        private int[,] _ParcelMap = new int[64, 64];
        #endregion Internal/Private Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client">Reference to the GridClient object</param>
        /// <param name="address">IPEndPoint of the simulator</param>
        /// <param name="handle">handle of the simulator</param>
        public Simulator(GridClient client, IPEndPoint address, ulong handle)
            : base(address)
        {
            Client = client;

            ipEndPoint = address;
            Handle = handle;
            Estate = new EstateTools(Client);
            Network = Client.Network;
            PacketArchive = new Queue<uint>(Settings.PACKET_ARCHIVE_SIZE);
            InBytes = new Queue<ulong>(Client.Settings.STATS_QUEUE_SIZE);
            OutBytes = new Queue<ulong>(Client.Settings.STATS_QUEUE_SIZE);
        }

        /// <summary>
        /// Called when this Simulator object is being destroyed
        /// </summary>
        public void Dispose()
        {
            // Force all the CAPS connections closed for this simulator
            if (Caps != null)
            {
                Caps.Disconnect(true);
            }
        }

        /// <summary>
        /// Attempt to connect to this simulator
        /// </summary>
        /// <param name="moveToSim">Whether to move our agent in to this sim or not</param>
        /// <returns>True if the connection succeeded or connection status is
        /// unknown, false if there was a failure</returns>
        public bool Connect(bool moveToSim)
        {
            if (connected)
            {
                Client.Self.CompleteAgentMovement(this);
                return true;
            }

            #region Start Timers

            // Destroy the timers
            if (AckTimer != null) AckTimer.Dispose();
            if (StatsTimer != null) StatsTimer.Dispose();
            if (PingTimer != null) PingTimer.Dispose();

            // Timer for sending out queued packet acknowledgements
            AckTimer = new Timer(new TimerCallback(AckTimer_Elapsed), null, Settings.NETWORK_TICK_INTERVAL,
                Settings.NETWORK_TICK_INTERVAL);
            // Timer for recording simulator connection statistics
            StatsTimer = new Timer(new TimerCallback(StatsTimer_Elapsed), null, 1000, 1000);
            // Timer for periodically pinging the simulator
            if (Client.Settings.SEND_PINGS)
                PingTimer = new Timer(new TimerCallback(PingTimer_Elapsed), null, Settings.PING_INTERVAL,
                    Settings.PING_INTERVAL);

            #endregion Start Timers

            Logger.Log("Connecting to " + this.ToString(), Helpers.LogLevel.Info, Client);

            try
            {
                // Create the UDP connection
                Start();

                // Mark ourselves as connected before firing everything else up
                connected = true;

                // Send the UseCircuitCode packet to initiate the connection
                UseCircuitCodePacket use = new UseCircuitCodePacket();
                use.CircuitCode.Code = Network.CircuitCode;
                use.CircuitCode.ID = Client.Self.AgentID;
                use.CircuitCode.SessionID = Client.Self.SessionID;

                // Send the initial packet out
                SendPacket(use, true);

                Stats.ConnectTime = Environment.TickCount;

                // Move our agent in to the sim to complete the connection
                if (moveToSim) Client.Self.CompleteAgentMovement(this);

                if (Client.Settings.SEND_AGENT_UPDATES)
                    Client.Self.Movement.SendUpdate(true, this);

                if (!ConnectedEvent.WaitOne(Client.Settings.SIMULATOR_TIMEOUT, false))
                {
                    Logger.Log("Giving up on waiting for RegionHandshake for " + this.ToString(),
                        Helpers.LogLevel.Warning, Client);
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e);
            }

            return false;
        }

        public void SetSeedCaps(string seedcaps)
        {
            if (Caps != null)
            {
                if (Caps._SeedCapsURI == seedcaps) return;

                Logger.Log("Unexpected change of seed capability", Helpers.LogLevel.Warning, Client);
                Caps.Disconnect(true);
                Caps = null;
            }

            if (Client.Settings.ENABLE_CAPS)
            {
                // Connect to the new CAPS system
                if (!String.IsNullOrEmpty(seedcaps))
                    Caps = new Caps(this, seedcaps);
                else
                    Logger.Log("Setting up a sim without a valid capabilities server!", Helpers.LogLevel.Error, Client);
            }

        }

        /// <summary>
        /// Disconnect from this simulator
        /// </summary>
        public void Disconnect(bool sendCloseCircuit)
        {
            if (connected)
            {
                connected = false;

                // Destroy the timers
                if (AckTimer != null) AckTimer.Dispose();
                if (StatsTimer != null) StatsTimer.Dispose();
                if (PingTimer != null) PingTimer.Dispose();

                // Kill the current CAPS system
                if (Caps != null)
                {
                    Caps.Disconnect(true);
                    Caps = null;
                }

                if (sendCloseCircuit)
                {
                    // Try to send the CloseCircuit notice
                    CloseCircuitPacket close = new CloseCircuitPacket();
                    UDPPacketBuffer buf = new UDPPacketBuffer(ipEndPoint, false, false);
                    buf.Data = close.ToBytes();
                    buf.DataLength = buf.Data.Length;

                    AsyncBeginSend(buf);
                }

                // Shut the socket communication down
                Stop();
            }
        }

        /// <summary>
        /// Sends a packet
        /// </summary>
        /// <param name="packet">Packet to be sent</param>
        /// <param name="setSequence">True to set the sequence number, false to
        /// leave it as is</param>
        public void SendPacket(Packet packet, bool setSequence)
        {
            // Send ACK and logout packets directly, everything else goes through the queue
            if (packet.Type == PacketType.PacketAck ||
                packet.Header.AppendedAcks ||
                packet.Type == PacketType.LogoutRequest)
            {
                SendPacketUnqueued(packet, setSequence);
            }
            else
            {
                Network.PacketOutbox.Enqueue(new NetworkManager.OutgoingPacket(this, packet, setSequence));
            }
            
        }

        /// <summary>
        /// Sends a packet directly to the simulator without queuing
        /// </summary>
        /// <param name="packet">Packet to be sent</param>
        /// <param name="setSequence">True to set the sequence number, false to
        /// leave it as is</param>
        public void SendPacketUnqueued(Packet packet, bool setSequence)
        {
            byte[] buffer;
            int bytes;

            // Keep track of when this packet was sent out
            packet.TickCount = Environment.TickCount;

            if (setSequence)
            {
                // Reset to zero if we've hit the upper sequence number limit
                Interlocked.CompareExchange(ref Sequence, 0, Settings.MAX_SEQUENCE);
                // Increment and fetch the current sequence number
                packet.Header.Sequence = (uint)Interlocked.Increment(ref Sequence);

                if (packet.Header.Reliable)
                {
                    // Add this packet to the list of ACK responses we are waiting on from the server
                    lock (NeedAck)
                    {
                        NeedAck[packet.Header.Sequence] = packet;
                    }

                    if (packet.Header.Resent)
                    {
                        // This packet has already been sent out once, strip any appended ACKs
                        // off it and reinsert them into the outgoing ACK queue under the 
                        // assumption that this packet will continually be rejected from the
                        // server or that the appended ACKs are possibly making the delivery fail
                        if (packet.Header.AckList.Length > 0)
                        {
                            Logger.DebugLog(String.Format("Purging ACKs from packet #{0} ({1}) which will be resent.",
                                packet.Header.Sequence, packet.GetType()));

                            lock (PendingAcks)
                            {
                                foreach (uint sequence in packet.Header.AckList)
                                {
                                    if (!PendingAcks.ContainsKey(sequence))
                                        PendingAcks[sequence] = sequence;
                                }
                            }

                            packet.Header.AppendedAcks = false;
                            packet.Header.AckList = new uint[0];
                        }
                    }
                    else
                    {
                        // This packet is not a resend, check if the conditions are favorable
                        // to ACK appending
                        if (packet.Type != PacketType.PacketAck &&
                            packet.Type != PacketType.LogoutRequest)
                        {
                            lock (PendingAcks)
                            {
                                if (PendingAcks.Count > 0 &&
                                    PendingAcks.Count < Client.Settings.MAX_APPENDED_ACKS)
                                {
                                    // Append all of the queued up outgoing ACKs to this packet
                                    packet.Header.AckList = new uint[PendingAcks.Count];

                                    for (int i = 0; i < PendingAcks.Count; i++)
                                        packet.Header.AckList[i] = PendingAcks.Values[i];

                                    PendingAcks.Clear();
                                    packet.Header.AppendedAcks = true;
                                }
                            }
                        }
                    }
                }
                else if (packet.Header.AckList.Length > 0)
                {
                    // Sanity check for ACKS appended on an unreliable packet, this is bad form
                    Logger.Log("Sending appended ACKs on an unreliable packet", Helpers.LogLevel.Warning);
                }
            }

            // Serialize the packet
            buffer = packet.ToBytes();
            bytes = buffer.Length;
            Stats.SentBytes += (ulong)bytes;
            ++Stats.SentPackets;

            UDPPacketBuffer buf;

            // Zerocode if needed
            if (packet.Header.Zerocoded)
            {
                buf = new UDPPacketBuffer(ipEndPoint, true, false);

                bytes = Helpers.ZeroEncode(buffer, bytes, buf.Data);
                buf.DataLength = bytes;
            }
            else
            {
                buf = new UDPPacketBuffer(ipEndPoint, false, false);

                buf.Data = buffer;
                buf.DataLength = bytes;
            }

            AsyncBeginSend(buf);
        }

        /// <summary>
        /// Send a raw byte array payload as a packet
        /// </summary>
        /// <param name="payload">The packet payload</param>
        /// <param name="setSequence">Whether the second, third, and fourth bytes
        /// should be modified to the current stream sequence number</param>
        public void SendPacketUnqueued(byte[] payload, bool setSequence)
        {
            try
            {
                if (setSequence && payload.Length > 3)
                {
                    uint sequence = (uint)Interlocked.Increment(ref Sequence);

                    payload[1] = (byte)(sequence >> 16);
                    payload[2] = (byte)(sequence >> 8);
                    payload[3] = (byte)(sequence % 256);
                }

                Stats.SentBytes += (ulong)payload.Length;
                ++Stats.SentPackets;

                UDPPacketBuffer buf = new UDPPacketBuffer(ipEndPoint, false, false);
                buf.Data = payload;
                buf.DataLength = payload.Length;

                AsyncBeginSend(buf);
            }
            catch (SocketException)
            {
                Logger.Log("Tried to send a " + payload.Length +
                    " byte payload on a closed socket, shutting down " + this.ToString(),
                    Helpers.LogLevel.Info, Client);

                Network.DisconnectSim(this, false);
                return;
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SendPing()
        {
            StartPingCheckPacket ping = new StartPingCheckPacket();
            ping.PingID.PingID = Stats.LastPingID++;
            ping.PingID.OldestUnacked = 0; // FIXME
            ping.Header.Reliable = false;
            SendPacket(ping, true);
            Stats.LastPingSent = Environment.TickCount;
        }

        /// <summary>
        /// Returns Simulator Name as a String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!String.IsNullOrEmpty(Name))
                return String.Format("{0} ({1})", Name, ipEndPoint);
            else
                return String.Format("({0})", ipEndPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Simulator sim = obj as Simulator;
            if (sim == null)
                return false;
            return (ipEndPoint.Equals(sim.ipEndPoint));
        }

        public static bool operator ==(Simulator lhs, Simulator rhs)
        {
            // If both are null, or both are same instance, return true
            if (System.Object.ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)lhs == null) || ((object)rhs == null))
            {
                return false;
            }

            return lhs.ipEndPoint.Equals(rhs.ipEndPoint);
        }

        public static bool operator !=(Simulator lhs, Simulator rhs)
        {
            return !(lhs == rhs);
        }

        protected override void PacketReceived(UDPPacketBuffer buffer)
        {
            Packet packet = null;

            // Check if this packet came from the server we expected it to come from
            if (!ipEndPoint.Address.Equals(((IPEndPoint)buffer.RemoteEndPoint).Address))
            {
                Logger.Log("Received " + buffer.DataLength + " bytes of data from unrecognized source " +
                    ((IPEndPoint)buffer.RemoteEndPoint).ToString(), Helpers.LogLevel.Warning, Client);
                return;
            }

            // Update the disconnect flag so this sim doesn't time out
            DisconnectCandidate = false;

            #region Packet Decoding

            int packetEnd = buffer.DataLength - 1;

            try
            {
                packet = Packet.BuildPacket(buffer.Data, ref packetEnd, buffer.ZeroData);
            }
            catch (MalformedDataException)
            {
                Logger.Log(String.Format("Malformed data, cannot parse packet:\n{0}",
                    Utils.BytesToHexString(buffer.Data, buffer.DataLength, null)), Helpers.LogLevel.Error);
            }

            // Fail-safe check
            if (packet == null)
            {
                Logger.Log("Couldn't build a message from the incoming data", Helpers.LogLevel.Warning, Client);
                return;
            }

            Stats.RecvBytes += (ulong)buffer.DataLength;
            ++Stats.RecvPackets;

            #endregion Packet Decoding

            #region Reliable Handling

            if (packet.Header.Reliable)
            {
                // Add this packet to the list of ACKs that need to be sent out
                lock (PendingAcks)
                {
                    uint sequence = (uint)packet.Header.Sequence;
                    if (!PendingAcks.ContainsKey(sequence)) PendingAcks[sequence] = sequence;
                }

                // Send out ACKs if we have a lot of them
                if (PendingAcks.Count >= Client.Settings.MAX_PENDING_ACKS)
                    SendAcks();

                if (packet.Header.Resent) ++Stats.ReceivedResends;
            }

            #endregion Reliable Handling

            #region Inbox Insertion

            NetworkManager.IncomingPacket incomingPacket;
            incomingPacket.Simulator = this;
            incomingPacket.Packet = packet;

            // TODO: Prioritize the queue
            Network.PacketInbox.Enqueue(incomingPacket);

            #endregion Inbox Insertion
        }

        protected override void PacketSent(UDPPacketBuffer buffer, int bytesSent)
        {
        }

        /// <summary>
        /// Sends out pending acknowledgements
        /// </summary>
        private void SendAcks()
        {
            lock (PendingAcks)
            {
                if (PendingAcks.Count > 0)
                {
                    if (PendingAcks.Count > 250)
                    {
                        Logger.Log("Too many ACKs queued up!", Helpers.LogLevel.Error, Client);
                        return;
                    }

                    PacketAckPacket acks = new PacketAckPacket();
                    acks.Header.Reliable = false;
                    acks.Packets = new PacketAckPacket.PacketsBlock[PendingAcks.Count];

                    for (int i = 0; i < PendingAcks.Count; i++)
                    {
                        acks.Packets[i] = new PacketAckPacket.PacketsBlock();
                        acks.Packets[i].ID = PendingAcks.Values[i];
                    }

                    SendPacket(acks, true);

                    PendingAcks.Clear();
                }
            }
        }

        /// <summary>
        /// Resend unacknowledged packets
        /// </summary>
        private void ResendUnacked()
        {
            lock (NeedAck)
            {
                List<uint> dropAck = new List<uint>();
                int now = Environment.TickCount;

                // Resend packets
                foreach (Packet packet in NeedAck.Values)
                {
                    if (packet.TickCount != 0 && now - packet.TickCount > Client.Settings.RESEND_TIMEOUT)
                    {
                        if (packet.ResendCount < Client.Settings.MAX_RESEND_COUNT)
                        {
                            try
                            {
                                if (Client.Settings.LOG_RESENDS)
                                {
                                    Logger.DebugLog(String.Format("Resending packet #{0} ({1}), {2}ms have passed",
                                        packet.Header.Sequence, packet.GetType(), now - packet.TickCount), Client);
                                }

                                packet.TickCount = 0;
                                packet.Header.Resent = true;
                                ++Stats.ResentPackets;
                                ++packet.ResendCount;

                                SendPacket(packet, false);
                            }
                            catch (Exception ex)
                            {
                                Logger.DebugLog("Exception trying to resend packet: " + ex.ToString(), Client);
                            }
                        }
                        else
                        {
                            if (Client.Settings.LOG_RESENDS)
                            {
                                Logger.DebugLog(String.Format("Dropping packet #{0} ({1}) after {2} failed attempts",
                                    packet.Header.Sequence, packet.GetType(), packet.ResendCount));
                            }

                            dropAck.Add(packet.Header.Sequence);
                        }
                    }
                }

                if (dropAck.Count != 0)
                {
                    foreach (uint seq in dropAck)
                        NeedAck.Remove(seq);
                }
            }
        }

        private void AckTimer_Elapsed(object obj)
        {
            SendAcks();
            ResendUnacked();
        }

        private void StatsTimer_Elapsed(object obj)
        {
            ulong old_in = 0, old_out = 0;

            if (InBytes.Count >= Client.Settings.STATS_QUEUE_SIZE)
                old_in = InBytes.Dequeue();
            if (OutBytes.Count >= Client.Settings.STATS_QUEUE_SIZE)
                old_out = OutBytes.Dequeue();

            InBytes.Enqueue(Stats.RecvBytes);
            OutBytes.Enqueue(Stats.SentBytes);

            if (old_in > 0 && old_out > 0)
            {
                Stats.IncomingBPS = (int)(Stats.RecvBytes - old_in) / Client.Settings.STATS_QUEUE_SIZE;
                Stats.OutgoingBPS = (int)(Stats.SentBytes - old_out) / Client.Settings.STATS_QUEUE_SIZE;
                //Client.Log("Incoming: " + IncomingBPS + " Out: " + OutgoingBPS +
                //    " Lag: " + LastLag + " Pings: " + ReceivedPongs +
                //    "/" + SentPings, Helpers.LogLevel.Debug); 
            }
        }

        private void PingTimer_Elapsed(object obj)
        {
            SendPing();
            Stats.SentPings++;
        }
    }
}
