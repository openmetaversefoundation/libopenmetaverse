/*
 * Copyright (c) 2007, Second Life Reverse Engineering Team
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
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using libsecondlife.Packets;

namespace libsecondlife
{
    /// <summary>
    /// Simulator is a wrapper for a network connection to a simulator and the
    /// Region class representing the block of land in the metaverse
    /// </summary>
    public class Simulator
    {
        /// <summary>A public reference to the client that this Simulator object
        /// is attached to</summary>
        public SecondLife Client;
        /// <summary></summary>
        public LLUUID ID = LLUUID.Zero;
        /// <summary></summary>
        public ulong Handle;
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
        public LLUUID SimOwner = LLUUID.Zero;
        /// <summary></summary>
        public LLUUID TerrainBase0 = LLUUID.Zero;
        /// <summary></summary>
        public LLUUID TerrainBase1 = LLUUID.Zero;
        /// <summary></summary>
        public LLUUID TerrainBase2 = LLUUID.Zero;
        /// <summary></summary>
        public LLUUID TerrainBase3 = LLUUID.Zero;
        /// <summary></summary>
        public LLUUID TerrainDetail0 = LLUUID.Zero;
        /// <summary></summary>
        public LLUUID TerrainDetail1 = LLUUID.Zero;
        /// <summary></summary>
        public LLUUID TerrainDetail2 = LLUUID.Zero;
        /// <summary></summary>
        public LLUUID TerrainDetail3 = LLUUID.Zero;
        /// <summary></summary>
        public bool IsEstateManager;
        /// <summary></summary>
        public EstateTools Estate;
        /// <summary>Current time dilation of this simulator</summary>
        public float Dilation;

        /// <summary>
        /// The ID number associated with this particular connection to the 
        /// simulator, used to emulate TCP connections. This is used 
        /// internally for packets that have a CircuitCode field
        /// </summary>
        public uint CircuitCode
        {
            get { return circuitCode; }
            set { circuitCode = value; }
        }
        /// <summary>The IP address and port of the server</summary>
        public IPEndPoint IPEndPoint { get { return ipEndPoint; } }
        /// <summary>Whether there is a working connection to the simulator or 
        /// not</summary>
        public bool Connected { get { return connected; } }

        /// <summary>Used internally to track sim disconnections</summary>
        internal bool DisconnectCandidate = false;
        /// <summary></summary>
        internal ManualResetEvent ConnectedEvent = new ManualResetEvent(false);
        /// <summary></summary>
        internal bool connected;

        private NetworkManager Network;
        private uint Sequence = 0;
        private object SequenceLock = new object();
        private byte[] RecvBuffer = new byte[4096];
        private byte[] ZeroBuffer = new byte[8192];
        private byte[] ZeroOutBuffer = new byte[4096];
        private Socket Connection = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private AsyncCallback ReceivedData;
        // Packets we sent out that need ACKs from the simulator
        private Dictionary<uint, Packet> NeedAck = new Dictionary<uint, Packet>();
        // Sequence numbers of packets we've received from the simulator
        private Queue<uint> Inbox;
        // ACKs that are queued up to be sent to the simulator
        private Dictionary<uint, uint> PendingAcks = new Dictionary<uint, uint>();
        private uint circuitCode;
        private IPEndPoint ipEndPoint;
        private EndPoint endPoint;
        private System.Timers.Timer AckTimer;


        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">Reference to the SecondLife client</param>
        /// <param name="circuit">Integer to uniquely identify the connection to this simulator</param>
        /// <param name="ip">IP address of the simulator</param>
        /// <param name="port">Port on the simulator to connect to</param>
        /// <param name="moveToSim">Whether to move our agent in to this sim or not</param>
        public Simulator(SecondLife client, uint circuit, IPAddress ip, int port, bool moveToSim)
        {
            Client = client;
            Estate = new EstateTools(Client);
            Network = client.Network;
            circuitCode = circuit;
            Inbox = new Queue<uint>(Client.Settings.INBOX_SIZE);
            AckTimer = new System.Timers.Timer(Client.Settings.NETWORK_TICK_LENGTH);
            AckTimer.Elapsed += new System.Timers.ElapsedEventHandler(AckTimer_Elapsed);

            // Initialize the callback for receiving a new packet
            ReceivedData = new AsyncCallback(OnReceivedData);

            Client.Log("Connecting to " + ip.ToString() + ":" + port, Helpers.LogLevel.Info);

            try
            {
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

                // Move our agent in to the sim to complete the connection
                if (moveToSim) Client.Self.CompleteAgentMovement(this);

                ConnectedEvent.Reset();
                ConnectedEvent.WaitOne(Client.Settings.SIMULATOR_TIMEOUT, false);
            }
            catch (Exception e)
            {
                Client.Log(e.ToString(), Helpers.LogLevel.Error);
            }
        }

        /// <summary>
        /// Disconnect a Simulator
        /// </summary>
        public void Disconnect()
        {
            if (connected)
            {
                connected = false;
                AckTimer.Stop();

                // Send the CloseCircuit notice
                CloseCircuitPacket close = new CloseCircuitPacket();

                if (Connection.Connected)
                {
                    try
                    {
                        Connection.Send(close.ToBytes());
                    }
                    catch (SocketException)
                    {
                        // There's a high probability of this failing if the network is
                        // disconnecting, so don't even bother logging the error
                    }
                }

                try
                {
                    // Shut the socket communication down
                    Connection.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException)
                {
                }
            }
        }

        /// <summary>
        /// Sends a packet
        /// </summary>
        /// <param name="packet">Packet to be sent</param>
        /// <param name="incrementSequence">Increment sequence number?</param>
        public void SendPacket(Packet packet, bool incrementSequence)
        {
            byte[] buffer;
            int bytes;

            if (packet.Header.AckList.Length > 0)
            {
                // Scrub any appended ACKs since all of the ACK handling is done here
                packet.Header.AckList = new uint[0];
            }
            packet.Header.AppendedAcks = false;

            // Keep track of when this packet was sent out
            packet.TickCount = Environment.TickCount;

            if (incrementSequence)
            {
                // Set the sequence number
                lock (SequenceLock)
                {
                    if (Sequence > Client.Settings.MAX_SEQUENCE)
                        Sequence = 1;
                    else
                        Sequence++;
                    packet.Header.Sequence = Sequence;
                }

                if (packet.Header.Reliable)
                {
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

                    // Don't append ACKs to resent packets, in case that's what was causing the
                    // delivery to fail
                    if (!packet.Header.Resent)
                    {
                        // Append any ACKs that need to be sent out to this packet
                        lock (PendingAcks)
                        {
                            if (PendingAcks.Count > 0 && PendingAcks.Count < Client.Settings.MAX_APPENDED_ACKS &&
                                packet.Type != PacketType.PacketAck &&
                                packet.Type != PacketType.LogoutRequest)
                            {
                                packet.Header.AckList = new uint[PendingAcks.Count];

                                int i = 0;

                                foreach (uint ack in PendingAcks.Values)
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
            }

            // Serialize the packet
            buffer = packet.ToBytes();
            bytes = buffer.Length;

            try
            {
                // Zerocode if needed
                if (packet.Header.Zerocoded)
                {
                    lock (ZeroOutBuffer)
                    {
                        bytes = Helpers.ZeroEncode(buffer, bytes, ZeroOutBuffer);
                        Connection.Send(ZeroOutBuffer, bytes, SocketFlags.None);
                    }
                }
                else
                {
                    Connection.Send(buffer, bytes, SocketFlags.None);
                }
            }
            catch (SocketException)
            {
                Client.Log("Tried to send a " + packet.Type.ToString() + " on a closed socket, shutting down " +
                    this.ToString(), Helpers.LogLevel.Info);

                Network.DisconnectSim(this);
                return;
            }
        }

        /// <summary>
        /// Send a raw byte array payload as a packet
        /// </summary>
        /// <param name="payload">The packet payload</param>
        /// <param name="setSequence">Whether the second, third, and fourth bytes
        /// should be modified to the current stream sequence number</param>
        public void SendPacket(byte[] payload, bool setSequence)
        {
            try
            {
                if (setSequence && payload.Length > 3)
                {
                    lock (SequenceLock)
                    {
                        payload[1] = (byte)(Sequence >> 16);
                        payload[2] = (byte)(Sequence >> 8);
                        payload[3] = (byte)(Sequence % 256);
                        Sequence++;
                    }
                }

                Connection.Send(payload, payload.Length, SocketFlags.None);
            }
            catch (SocketException)
            {
                Client.Log("Tried to send a " + payload.Length + " byte payload on a closed socket, shutting down " +
                    this.ToString(), Helpers.LogLevel.Info);

                Network.DisconnectSim(this);
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="west"></param>
        /// <param name="south"></param>
        /// <param name="east"></param>
        /// <param name="north"></param>
        public void ParcelSubdivide(float west, float south, float east, float north)
        {
            ParcelDividePacket divide = new ParcelDividePacket();
            divide.AgentData.AgentID = Client.Network.AgentID;
            divide.AgentData.SessionID = Client.Network.SessionID;
            divide.ParcelData.East = east;
            divide.ParcelData.North = north;
            divide.ParcelData.South = south;
            divide.ParcelData.West = west;

            SendPacket(divide, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="west"></param>
        /// <param name="south"></param>
        /// <param name="east"></param>
        /// <param name="north"></param>
        public void ParcelJoin(float west, float south, float east, float north)
        {
            ParcelJoinPacket join = new ParcelJoinPacket();
            join.AgentData.AgentID = Client.Network.AgentID;
            join.AgentData.SessionID = Client.Network.SessionID;
            join.ParcelData.East = east;
            join.ParcelData.North = north;
            join.ParcelData.South = south;
            join.ParcelData.West = west;

            SendPacket(join, true);
        }

        /// <summary>
        /// Returns Simulator Name as a String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Name.Length > 0)
                return Name + " (" + ipEndPoint.ToString() + ")";
            else
                return "(" + ipEndPoint.ToString() + ")";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ID.GetHashCode();
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
            return ID.Equals(sim.ID);
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
                        // FIXME: Handle the odd case where we have too many pending ACKs queued up
                        Client.Log("Too many ACKs queued up!", Helpers.LogLevel.Error);
                        return;
                    }

                    int i = 0;
                    PacketAckPacket acks = new PacketAckPacket();
                    acks.Packets = new PacketAckPacket.PacketsBlock[PendingAcks.Count];

                    foreach (uint ack in PendingAcks.Values)
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
        /// <summary>
        /// Resend unacknowledged packets
        /// </summary>
        private void ResendUnacked()
        {
            if (connected)
            {
                int now = Environment.TickCount;

                lock (NeedAck)
                {
                    foreach (Packet packet in NeedAck.Values)
                    {
                        if (now - packet.TickCount > Client.Settings.RESEND_TIMEOUT)
                        {
                            Client.Log("Resending " + packet.Type.ToString() + " packet (" + packet.Header.Sequence +
                                "), " + (now - packet.TickCount) + "ms have passed", Helpers.LogLevel.Info);

                            packet.Header.Resent = true;
                            SendPacket(packet, false);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Callback handler for incomming data
        /// </summary>
        /// <param name="result"></param>
        private void OnReceivedData(IAsyncResult result)
        {
            Packet packet = null;
            int numBytes;

            // Update the disconnect flag so this sim doesn't time out
            DisconnectCandidate = false;

            lock (RecvBuffer)
            {
                // Retrieve the incoming packet
                try
                {
                    numBytes = Connection.EndReceiveFrom(result, ref endPoint);

                    int packetEnd = numBytes - 1;
                    packet = Packet.BuildPacket(RecvBuffer, ref packetEnd, ZeroBuffer);

                    Connection.BeginReceiveFrom(RecvBuffer, 0, RecvBuffer.Length, SocketFlags.None, ref endPoint, ReceivedData, null);
                }
                catch (SocketException)
                {
                    Client.Log(endPoint.ToString() + " socket is closed, shutting down " + this.ToString(),
                        Helpers.LogLevel.Info);
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
                if (PendingAcks.Count > Client.Settings.MAX_PENDING_ACKS)
                {
                    SendAcks();
                }

                // Check if we already received this packet
                if (Inbox.Contains(packet.Header.Sequence))
                {
                    Client.Log("Received a duplicate " + packet.Type.ToString() + ", sequence=" +
                        packet.Header.Sequence + ", resent=" + ((packet.Header.Resent) ? "Yes" : "No") +
                        ", Inbox.Count=" + Inbox.Count + ", NeedAck.Count=" + NeedAck.Count,
                        Helpers.LogLevel.Info);

                    // Send an ACK for this packet immediately
                    //SendAck(packet.Header.Sequence);

                    // TESTING: Try just queuing up ACKs for resent packets instead of immediately triggering an ACK
                    lock (PendingAcks)
                    {
                        uint sequence = (uint)packet.Header.Sequence;
                        if (!PendingAcks.ContainsKey(sequence)) { PendingAcks[sequence] = sequence; }
                    }

                    // Avoid firing a callback twice for the same packet
                    return;
                }
                else
                {
                    lock (PendingAcks)
                    {
                        uint sequence = (uint)packet.Header.Sequence;
                        if (!PendingAcks.ContainsKey(sequence)) { PendingAcks[sequence] = sequence; }
                    }
                }
            }

            // Add this packet to our inbox
            lock (Inbox)
            {
                while (Inbox.Count >= Client.Settings.INBOX_SIZE)
                {
                    Inbox.Dequeue();
                    Inbox.Dequeue();
                }
                Inbox.Enqueue(packet.Header.Sequence);
            }

            // Handle appended ACKs
            if (packet.Header.AppendedAcks)
            {
                lock (NeedAck)
                {
                    foreach (uint ack in packet.Header.AckList)
                    {
                        NeedAck.Remove(ack);
                    }
                }
            }

            // Handle PacketAck packets
            if (packet.Type == PacketType.PacketAck)
            {
                PacketAckPacket ackPacket = (PacketAckPacket)packet;

                lock (NeedAck)
                {
                    foreach (PacketAckPacket.PacketsBlock block in ackPacket.Packets)
                    {
                        NeedAck.Remove(block.ID);
                    }
                }
            }


            // Fire the registered packet events
            #region FireCallbacks
            if (Network.Callbacks.ContainsKey(packet.Type))
            {
                List<NetworkManager.PacketCallback> callbackArray = Network.Callbacks[packet.Type];

                // Fire any registered callbacks
                foreach (NetworkManager.PacketCallback callback in callbackArray)
                {
                    if (callback != null)
                    {
                        try
                        {
                            callback(packet, this);
                        }
                        catch (Exception e)
                        {
                            Client.Log("Caught an exception in a packet callback: " + e.ToString(),
                                Helpers.LogLevel.Error);
                        }
                    }
                }
            }

            if (Network.Callbacks.ContainsKey(PacketType.Default))
            {
                List<NetworkManager.PacketCallback> callbackArray = Network.Callbacks[PacketType.Default];

                // Fire any registered callbacks
                foreach (NetworkManager.PacketCallback callback in callbackArray)
                {
                    if (callback != null)
                    {
                        try
                        {
                            callback(packet, this);
                        }
                        catch (Exception e)
                        {
                            Client.Log("Caught an exception in a packet callback: " + e.ToString(),
                                Helpers.LogLevel.Error);
                        }
                    }
                }
            }
            #endregion FireCallbacks
        }

        private void AckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs ea)
        {
            SendAcks();
            ResendUnacked();
        }
    }
}
