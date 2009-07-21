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
    /// Access level for a simulator
    /// </summary>
    public enum SimAccess : byte
    {
        /// <summary>Minimum access level, no additional checks</summary>
        Min = 0,
        /// <summary>Trial accounts allowed</summary>
        Trial = 7,
        /// <summary>PG rating</summary>
        PG = 13,
        /// <summary>Mature rating</summary>
        Mature = 21,
        /// <summary>Simulator is offline</summary>
        Down = 254,
        /// <summary>Simulator does not exist</summary>
        NonExistent = 255
    }

    #endregion Enums

    /// <summary>
    /// 
    /// </summary>
    public class Simulator : UDPBase, IDisposable
    {
        #region Structs
        /// <summary>
        /// Simulator Statistics
        /// </summary>
        public struct SimStats
        {
            /// <summary>Total number of packets sent by this simulator to this agent</summary>
            public long SentPackets;
            /// <summary>Total number of packets received by this simulator to this agent</summary>
            public long RecvPackets;
            /// <summary>Total number of bytes sent by this simulator to this agent</summary>
            public long SentBytes;
            /// <summary>Total number of bytes received by this simulator to this agent</summary>
            public long RecvBytes;
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
        /// <summary>A Unique Cache identifier for this simulator</summary>
        public UUID ID = UUID.Zero;
        /// <summary>The capabilities for this simulator</summary>
        public Caps Caps = null;
        /// <summary></summary>
        public ulong Handle;
        /// <summary>The current version of software this simulator is running</summary>
        public string SimVersion = String.Empty;
        /// <summary></summary>
        public string Name = String.Empty;
        /// <summary>A 64x64 grid of parcel coloring values. The values stored 
        /// in this array are of the <seealso cref="ParcelArrayType"/> type</summary>
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
        /// <summary>true if your agent has Estate Manager rights on this region</summary>
        public bool IsEstateManager;
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
        /// <summary>The regions Unique ID</summary>
        public UUID RegionID = UUID.Zero;
        /// <summary>The physical data center the simulator is located</summary>
        /// <remarks>Known values are:
        /// <list type="table">
        /// <item>Dallas</item>
        /// <item>Chandler</item>
        /// <item>SF</item>
        /// </list>
        /// </remarks>
        public string ColoLocation;
        /// <summary>The CPU Class of the simulator</summary>
        /// <remarks>Most full mainland/estate sims appear to be 5,
        /// Homesteads and Openspace appear to be 501</remarks>
        public int CPUClass;
        /// <summary>The number of regions sharing the same CPU as this one</summary>
        /// <remarks>"Full Sims" appear to be 1, Homesteads appear to be 4</remarks>
        public int CPURatio;
        /// <summary>The billing product name</summary>
        /// <remarks>Known values are:
        /// <list type="table">
        /// <item>Mainland / Full Region (Sku: 023)</item>
        /// <item>Estate / Full Region (Sku: 024)</item>
        /// <item>Estate / Openspace (Sku: 027)</item>
        /// <item>Estate / Homestead (Sku: 029)</item>
        /// <item>Mainland / Homestead (Sku: 129) (Linden Owned)</item>
        /// </list>
        /// </remarks>
        public string ProductName;
        /// <summary>The billing product SKU</summary>
        /// <remarks>Known values are:
        /// <list type="table">
        /// <item>023 Mainland / Full Region</item>
        /// <item>024 Estate / Full Region</item>
        /// <item>027 Estate / Openspace</item>
        /// <item>029 Estate / Homestead</item>
        /// <item>129 Mainland / Homestead (Linden Owned)</item>
        /// </list>
        /// </remarks>
        public string ProductSku;
            
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
        public IPEndPoint IPEndPoint { get { return remoteEndPoint; } }
        /// <summary>Whether there is a working connection to the simulator or 
        /// not</summary>
        public bool Connected { get { return connected; } }
        /// <summary>Coarse locations of avatars in this simulator</summary>
        public InternalDictionary<UUID, Vector3> AvatarPositions { get { return avatarPositions; } }
        /// <summary>AvatarPositions key representing TrackAgent target</summary>
        public UUID PreyID { get { return preyID; } }

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
        internal InternalDictionary<UUID, Vector3> avatarPositions = new InternalDictionary<UUID, Vector3>();
        /// <summary>AvatarPositions key representing TrackAgent target</summary>
        internal UUID preyID = UUID.Zero;
        /// <summary>Sequence numbers of packets we've received
        /// (for duplicate checking)</summary>
        internal IncomingPacketIDCollection PacketArchive;
        /// <summary>Packets we sent out that need ACKs from the simulator</summary>
        internal SortedDictionary<uint, NetworkManager.OutgoingPacket> NeedAck = new SortedDictionary<uint, NetworkManager.OutgoingPacket>();
        /// <summary>Sequence number for pause/resume</summary>
        internal int pauseSerial;

        private NetworkManager Network;
        private Queue<long> InBytes, OutBytes;
        // ACKs that are queued up to be sent to the simulator
        private LocklessQueue<uint> PendingAcks = new LocklessQueue<uint>();
        private int PendingAckCount = 0;
        private Timer AckTimer;
        private Timer PingTimer;
        private Timer StatsTimer;
        // simulator <> parcel LocalID Map
        private int[,] _ParcelMap = new int[64, 64];
        internal bool DownloadingParcelMap = false;

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

            Handle = handle;
            Network = Client.Network;
            PacketArchive = new IncomingPacketIDCollection(Settings.PACKET_ARCHIVE_SIZE);
            InBytes = new Queue<long>(Client.Settings.STATS_QUEUE_SIZE);
            OutBytes = new Queue<long>(Client.Settings.STATS_QUEUE_SIZE);
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

            // Timer for sending out queued packet acknowledgements
            if (AckTimer == null)
                AckTimer = new Timer(AckTimer_Elapsed, null, Settings.NETWORK_TICK_INTERVAL, Timeout.Infinite);

            // Timer for recording simulator connection statistics
            if (StatsTimer == null)
                StatsTimer = new Timer(StatsTimer_Elapsed, null, 1000, 1000);

            // Timer for periodically pinging the simulator
            if (PingTimer == null && Client.Settings.SEND_PINGS)
                PingTimer = new Timer(PingTimer_Elapsed, null, Settings.PING_INTERVAL, Settings.PING_INTERVAL);

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
                SendPacket(use);

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

                AckTimer = null;
                StatsTimer = null;
                PingTimer = null;

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
                    UDPPacketBuffer buf = new UDPPacketBuffer(remoteEndPoint);
                    byte[] data = close.ToBytes();
                    Buffer.BlockCopy(data, 0, buf.Data, 0, data.Length);
                    buf.DataLength = data.Length;

                    AsyncBeginSend(buf);
                }

                // Shut the socket communication down
                Stop();
            }
        }

        /// <summary>
        /// Instructs the simulator to stop sending update (and possibly other) packets
        /// </summary>
        public void Pause()
        {
            AgentPausePacket pause = new AgentPausePacket();
            pause.AgentData.AgentID = Client.Self.AgentID;
            pause.AgentData.SessionID = Client.Self.SessionID;
            pause.AgentData.SerialNum = (uint)Interlocked.Exchange(ref pauseSerial, pauseSerial + 1);

            Client.Network.SendPacket(pause, this);
        }

        /// <summary>
        /// Instructs the simulator to resume sending update packets (unpause)
        /// </summary>
        public void Resume()
        {
            AgentResumePacket resume = new AgentResumePacket();
            resume.AgentData.AgentID = Client.Self.AgentID;
            resume.AgentData.SessionID = Client.Self.SessionID;
            resume.AgentData.SerialNum = (uint)Interlocked.Exchange(ref pauseSerial, pauseSerial + 1);

            Client.Network.SendPacket(resume, this);
        }

        #region Packet Sending

        /// <summary>
        /// Sends a packet
        /// </summary>
        /// <param name="packet">Packet to be sent</param>
        public void SendPacket(Packet packet)
        {
            // DEBUG: This can go away after we are sure nothing in the library is trying to do this
            if (packet.Header.AppendedAcks || (packet.Header.AckList != null && packet.Header.AckList.Length > 0))
                Logger.Log("Attempting to send packet " + packet.Type + " with ACKs appended before serialization", Helpers.LogLevel.Error);

            if (packet.HasVariableBlocks)
            {
                byte[][] datas;
                try { datas = packet.ToBytesMultiple(); }
                catch (NullReferenceException)
                {
                    Logger.Log("Failed to serialize " + packet.Type + " packet to one or more payloads due to a missing block or field. StackTrace: " +
                        Environment.StackTrace, Helpers.LogLevel.Error);
                    return;
                }
                int packetCount = datas.Length;

                if (packetCount > 1)
                    Logger.DebugLog("Split " + packet.Type + " packet into " + packetCount + " packets");

                for (int i = 0; i < packetCount; i++)
                {
                    byte[] data = datas[i];
                    SendPacketData(data, data.Length, packet.Type, packet.Header.Zerocoded);
                }
            }
            else
            {
                byte[] data = packet.ToBytes();
                SendPacketData(data, data.Length, packet.Type, packet.Header.Zerocoded);
            }
        }

        public void SendPacketData(byte[] data, int dataLength, PacketType type, bool doZerocode)
        {
            UDPPacketBuffer buffer = new UDPPacketBuffer(remoteEndPoint, Packet.MTU);

            // Zerocode if needed
            if (doZerocode)
            {
                try { dataLength = Helpers.ZeroEncode(data, dataLength, buffer.Data); }
                catch (IndexOutOfRangeException)
                {
                    // The packet grew larger than Packet.MTU bytes while zerocoding.
                    // Remove the MSG_ZEROCODED flag and send the unencoded data
                    // instead
                    data[0] = (byte)(data[0] & ~Helpers.MSG_ZEROCODED);
                    Buffer.BlockCopy(data, 0, buffer.Data, 0, dataLength);
                }
            }
            else
            {
                Buffer.BlockCopy(data, 0, buffer.Data, 0, dataLength);
            }
            buffer.DataLength = dataLength;

            #region Queue or Send

            NetworkManager.OutgoingPacket outgoingPacket = new NetworkManager.OutgoingPacket(this, buffer);

            // Send ACK and logout packets directly, everything else goes through the queue
            if (Client.Settings.THROTTLE_OUTGOING_PACKETS == false ||
                type == PacketType.PacketAck ||
                type == PacketType.LogoutRequest)
            {
                SendPacketFinal(outgoingPacket);
            }
            else
            {
                Network.PacketOutbox.Enqueue(outgoingPacket);
            }

            #endregion Queue or Send
        }

        internal void SendPacketFinal(NetworkManager.OutgoingPacket outgoingPacket)
        {
            UDPPacketBuffer buffer = outgoingPacket.Buffer;
            byte flags = buffer.Data[0];
            bool isResend = (flags & Helpers.MSG_RESENT) != 0;
            bool isReliable = (flags & Helpers.MSG_RELIABLE) != 0;

            // Keep track of when this packet was sent out (right now)
            outgoingPacket.TickCount = Environment.TickCount;

            #region ACK Appending

            int dataLength = buffer.DataLength;

            // Keep appending ACKs until there is no room left in the packet or there are
            // no more ACKs to append
            uint ackCount = 0;
            uint ack;
            while (dataLength + 5 < Packet.MTU && PendingAcks.Dequeue(out ack))
            {
                Utils.UIntToBytesBig(ack, buffer.Data, dataLength);
                dataLength += 4;
                ++ackCount;
            }

            if (ackCount > 0)
            {
                // Set the last byte of the packet equal to the number of appended ACKs
                buffer.Data[dataLength++] = (byte)ackCount;
                // Set the appended ACKs flag on this packet
                buffer.Data[0] = (byte)(buffer.Data[0] | Helpers.MSG_APPENDED_ACKS);
            }

            buffer.DataLength = dataLength;

            #endregion ACK Appending

            if (!isResend)
            {
                // Not a resend, assign a new sequence number
                uint sequenceNumber = (uint)Interlocked.Increment(ref Sequence);
                Utils.UIntToBytesBig(sequenceNumber, buffer.Data, 1);
                outgoingPacket.SequenceNumber = sequenceNumber;

                if (isReliable)
                {
                    // Add this packet to the list of ACK responses we are waiting on from the server
                    lock (NeedAck) NeedAck[sequenceNumber] = outgoingPacket;
                }
            }

            // Put the UDP payload on the wire
            AsyncBeginSend(buffer);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SendPing()
        {
            uint oldestUnacked = 0;

            // Get the oldest NeedAck value, the first entry in the sorted dictionary
            lock (NeedAck)
            {
                if (NeedAck.Count > 0)
                {
                    SortedDictionary<uint, NetworkManager.OutgoingPacket>.KeyCollection.Enumerator en = NeedAck.Keys.GetEnumerator();
                    en.MoveNext();
                    oldestUnacked = en.Current;
                }
            }

            //if (oldestUnacked != 0)
            //    Logger.DebugLog("Sending ping with oldestUnacked=" + oldestUnacked);

            StartPingCheckPacket ping = new StartPingCheckPacket();
            ping.PingID.PingID = Stats.LastPingID++;
            ping.PingID.OldestUnacked = oldestUnacked;
            ping.Header.Reliable = false;
            SendPacket(ping);
            Stats.LastPingSent = Environment.TickCount;
        }

        #endregion Packet Sending

        /// <summary>
        /// Returns Simulator Name as a String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!String.IsNullOrEmpty(Name))
                return String.Format("{0} ({1})", Name, remoteEndPoint);
            else
                return String.Format("({0})", remoteEndPoint);
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
            return (remoteEndPoint.Equals(sim.remoteEndPoint));
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

            return lhs.remoteEndPoint.Equals(rhs.remoteEndPoint);
        }

        public static bool operator !=(Simulator lhs, Simulator rhs)
        {
            return !(lhs == rhs);
        }

        protected override void PacketReceived(UDPPacketBuffer buffer)
        {
            Packet packet = null;

            // Check if this packet came from the server we expected it to come from
            if (!remoteEndPoint.Address.Equals(((IPEndPoint)buffer.RemoteEndPoint).Address))
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
                packet = Packet.BuildPacket(buffer.Data, ref packetEnd,
                    // Only allocate a buffer for zerodecoding if the packet is zerocoded
                    ((buffer.Data[0] & Helpers.MSG_ZEROCODED) != 0) ? new byte[8192] : null);
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

            Interlocked.Add(ref Stats.RecvBytes, buffer.DataLength);
            Interlocked.Increment(ref Stats.RecvPackets);

            #endregion Packet Decoding

            if (packet.Header.Resent)
                Interlocked.Increment(ref Stats.ReceivedResends);

            #region ACK Receiving

            // Handle appended ACKs
            if (packet.Header.AppendedAcks && packet.Header.AckList != null)
            {
                lock (NeedAck)
                {
                    for (int i = 0; i < packet.Header.AckList.Length; i++)
                        NeedAck.Remove(packet.Header.AckList[i]);
                }
            }

            // Handle PacketAck packets
            if (packet.Type == PacketType.PacketAck)
            {
                PacketAckPacket ackPacket = (PacketAckPacket)packet;

                lock (NeedAck)
                {
                    for (int i = 0; i < ackPacket.Packets.Length; i++)
                        NeedAck.Remove(ackPacket.Packets[i].ID);
                }
            }

            #endregion ACK Receiving

            #region ACK Sending

            // Add this packet to the list of ACKs that need to be sent out
            uint sequence = (uint)packet.Header.Sequence;
            PendingAcks.Enqueue(sequence);
            int pendingAckCount = Interlocked.Increment(ref PendingAckCount);

            // Send out ACKs if we have a lot of them
            if (pendingAckCount >= Client.Settings.MAX_PENDING_ACKS)
                SendAcks();

            #endregion ACK Sending

            // Check the archive of received packet IDs to see whether we already received this packet
            if (packet.Header.Reliable && !PacketArchive.TryEnqueue(packet.Header.Sequence))
            {
                if (packet.Header.Resent)
                    Logger.DebugLog("Received a resend of already processed packet #" + packet.Header.Sequence + ", type: " + packet.Type);
                else
                    Logger.Log("Received a duplicate (not marked as resend) of packet #" + packet.Header.Sequence + ", type: " + packet.Type,
                        Helpers.LogLevel.Warning);

                // Avoid firing a callback twice for the same packet
                return;
            }

            #region Inbox Insertion

            NetworkManager.IncomingPacket incomingPacket;
            incomingPacket.Simulator = this;
            incomingPacket.Packet = packet;

            Network.PacketInbox.Enqueue(incomingPacket);

            #endregion Inbox Insertion
        }

        protected override void PacketSent(UDPPacketBuffer buffer, int bytesSent)
        {
            // Stats tracking
            Interlocked.Add(ref Stats.SentBytes, bytesSent);
            Interlocked.Increment(ref Stats.SentPackets);

            Client.Network.PacketSent(buffer.Data, bytesSent, this);
        }

        /// <summary>
        /// Sends out pending acknowledgements
        /// </summary>
        private void SendAcks()
        {
            uint ack;

            if (PendingAcks.Dequeue(out ack))
            {
                Interlocked.Decrement(ref PendingAckCount);

                List<PacketAckPacket.PacketsBlock> blocks = new List<PacketAckPacket.PacketsBlock>();
                PacketAckPacket.PacketsBlock block = new PacketAckPacket.PacketsBlock();
                block.ID = ack;
                blocks.Add(block);

                while (PendingAcks.Dequeue(out ack))
                {
                    Interlocked.Decrement(ref PendingAckCount);

                    block = new PacketAckPacket.PacketsBlock();
                    block.ID = ack;
                    blocks.Add(block);
                }

                PacketAckPacket packet = new PacketAckPacket();
                packet.Header.Reliable = false;
                packet.Packets = blocks.ToArray();

                SendPacket(packet);
            }
        }

        /// <summary>
        /// Resend unacknowledged packets
        /// </summary>
        private void ResendUnacked()
        {
            if (NeedAck.Count > 0)
            {
                NetworkManager.OutgoingPacket[] array;

                lock (NeedAck)
                {
                    // Create a temporary copy of the outgoing packets array to iterate over
                    array = new NetworkManager.OutgoingPacket[NeedAck.Count];
                    NeedAck.Values.CopyTo(array, 0);
                }

                int now = Environment.TickCount;

                // Resend packets
                for (int i = 0; i < array.Length; i++)
                {
                    NetworkManager.OutgoingPacket outgoing = array[i];

                    if (outgoing.TickCount != 0 && now - outgoing.TickCount > Client.Settings.RESEND_TIMEOUT)
                    {
                        if (outgoing.ResendCount < Client.Settings.MAX_RESEND_COUNT)
                        {
                            // The TickCount will be set to the current time when the packet
                            // is actually sent out again
                            outgoing.TickCount = 0;

                            // Set the resent flag
                            outgoing.Buffer.Data[0] = (byte)(outgoing.Buffer.Data[0] | Helpers.MSG_RESENT);

                            // Stats tracking
                            Interlocked.Increment(ref outgoing.ResendCount);
                            Interlocked.Increment(ref Stats.ResentPackets);

                            if (Client.Settings.LOG_RESENDS)
                            {
                                Logger.DebugLog(String.Format("Resending packet #{0}, {1}ms have passed",
                                    outgoing.SequenceNumber, now - outgoing.TickCount), Client);
                            }

                            SendPacketFinal(outgoing);
                        }
                        else
                        {
                            Logger.DebugLog(String.Format("Dropping packet #{0} after {1} failed attempts",
                                outgoing.SequenceNumber, outgoing.ResendCount));

                            lock (NeedAck) NeedAck.Remove(outgoing.SequenceNumber);
                        }
                    }
                }
            }
        }

        private void AckTimer_Elapsed(object obj)
        {
            SendAcks();
            ResendUnacked();

            // Start the ACK handling functions again after NETWORK_TICK_INTERVAL milliseconds
            try { AckTimer.Change(Settings.NETWORK_TICK_INTERVAL, Timeout.Infinite); }
            catch (Exception) { }
        }

        private void StatsTimer_Elapsed(object obj)
        {
            long old_in = 0, old_out = 0;
            long recv = Stats.RecvBytes;
            long sent = Stats.SentBytes;

            if (InBytes.Count >= Client.Settings.STATS_QUEUE_SIZE)
                old_in = InBytes.Dequeue();
            if (OutBytes.Count >= Client.Settings.STATS_QUEUE_SIZE)
                old_out = OutBytes.Dequeue();

            InBytes.Enqueue(recv);
            OutBytes.Enqueue(sent);

            if (old_in > 0 && old_out > 0)
            {
                Stats.IncomingBPS = (int)(recv - old_in) / Client.Settings.STATS_QUEUE_SIZE;
                Stats.OutgoingBPS = (int)(sent - old_out) / Client.Settings.STATS_QUEUE_SIZE;
                //Client.Log("Incoming: " + IncomingBPS + " Out: " + OutgoingBPS +
                //    " Lag: " + LastLag + " Pings: " + ReceivedPongs +
                //    "/" + SentPings, Helpers.LogLevel.Debug); 
            }
        }

        private void PingTimer_Elapsed(object obj)
        {
            SendPing();
            Interlocked.Increment(ref Stats.SentPings);
        }
    }

    public sealed class IncomingPacketIDCollection
    {
        readonly uint[] Items;
        HashSet<uint> hashSet;
        int first;
        int next;
        int capacity;

        public IncomingPacketIDCollection(int capacity)
        {
            this.capacity = capacity;
            Items = new uint[capacity];
            hashSet = new HashSet<uint>();
        }

        public bool TryEnqueue(uint ack)
        {
            lock (hashSet)
            {
                if (hashSet.Add(ack))
                {
                    Items[next] = ack;
                    next = (next + 1) % capacity;
                    if (next == first)
                    {
                        hashSet.Remove(Items[first]);
                        first = (first + 1) % capacity;
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
