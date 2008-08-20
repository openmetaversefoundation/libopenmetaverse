using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian
{
    public class Agent
    {
        public UUID AgentID;
        public UUID SessionID;
        public UUID SecureSessionID;
        public uint CircuitCode;
        public string FirstName;
        public string LastName;
        public Avatar Avatar = new Avatar();
        public int Balance;
        public bool Running;
        public AgentManager.ControlFlags ControlFlags = AgentManager.ControlFlags.NONE;

        /// <summary>Sequence numbers of packets we've received (for duplicate checking)</summary>
        internal Queue<uint> packetArchive = new Queue<uint>();
        /// <summary>Packets we have sent that need to be ACKed by the client</summary>
        internal Dictionary<uint, Packet> needAcks = new Dictionary<uint, Packet>();

        UDPServer udpServer;
        IPEndPoint address;
        /// <summary>ACKs that are queued up, waiting to be sent to the client</summary>
        SortedList<uint, uint> pendingAcks = new SortedList<uint, uint>();
        int currentSequence = 0;
        Timer ackTimer;

        public IPEndPoint Address
        {
            get { return address; }
            set { address = value; }
        }

        public Agent(UDPServer udpServer)
        {
            this.udpServer = udpServer;
        }

        public void Initialize(IPEndPoint address)
        {
            this.address = address;

            ackTimer = new Timer(new TimerCallback(AckTimer_Elapsed), null, Settings.NETWORK_TICK_INTERVAL,
                Settings.NETWORK_TICK_INTERVAL);
        }

        public void SendPacket(Packet packet)
        {
            SendPacket(packet, true);
        }

        public void SendPacket(Packet packet, bool setSequence)
        {
            byte[] buffer;
            int bytes;

            // Keep track of when this packet was sent out
            packet.TickCount = Environment.TickCount;

            if (setSequence)
            {
                // Reset to zero if we've hit the upper sequence number limit
                Interlocked.CompareExchange(ref currentSequence, 0, 0xFFFFFF);
                // Increment and fetch the current sequence number
                uint sequence = (uint)Interlocked.Increment(ref currentSequence);
                packet.Header.Sequence = sequence;

                if (packet.Header.Reliable)
                {
                    // Add this packet to the list of ACK responses we are waiting on from the client
                    lock (needAcks)
                        needAcks[sequence] = packet;

                    if (packet.Header.Resent)
                    {
                        // This packet has already been sent out once, strip any appended ACKs
                        // off it and reinsert them into the outgoing ACK queue under the 
                        // assumption that this packet will continually be rejected from the
                        // client or that the appended ACKs are possibly making the delivery fail
                        if (packet.Header.AckList.Length > 0)
                        {
                            Logger.DebugLog(String.Format("Purging ACKs from packet #{0} ({1}) which will be resent.",
                                packet.Header.Sequence, packet.GetType()));

                            lock (pendingAcks)
                            {
                                foreach (uint ack in packet.Header.AckList)
                                {
                                    if (!pendingAcks.ContainsKey(ack))
                                        pendingAcks[ack] = ack;
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
                        if (packet.Type != PacketType.PacketAck)
                        {
                            lock (pendingAcks)
                            {
                                if (pendingAcks.Count > 0 &&
                                    pendingAcks.Count < 10)
                                {
                                    // Append all of the queued up outgoing ACKs to this packet
                                    packet.Header.AckList = new uint[pendingAcks.Count];

                                    for (int i = 0; i < pendingAcks.Count; i++)
                                        packet.Header.AckList[i] = pendingAcks.Values[i];

                                    pendingAcks.Clear();
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
            //Stats.SentBytes += (ulong)bytes;
            //++Stats.SentPackets;

            UDPPacketBuffer buf;

            // Zerocode if needed
            if (packet.Header.Zerocoded)
            {
                buf = new UDPPacketBuffer(address, true, false);

                bytes = Helpers.ZeroEncode(buffer, bytes, buf.Data);
                buf.DataLength = bytes;
            }
            else
            {
                buf = new UDPPacketBuffer(address, false, false);

                buf.Data = buffer;
                buf.DataLength = bytes;
            }

            udpServer.AsyncBeginSend(buf);
        }

        public void QueueAck(uint ack)
        {
            // Add this packet to the list of ACKs that need to be sent out
            lock (pendingAcks)
                pendingAcks[ack] = ack;

            // Send out ACKs if we have a lot of them
            if (pendingAcks.Count >= 10)
                SendAcks();
        }

        public void ProcessAcks(List<uint> acks)
        {
            lock (needAcks)
            {
                foreach (uint ack in acks)
                    needAcks.Remove(ack);
            }
        }

        public void SendAck(uint ack)
        {
            PacketAckPacket acks = new PacketAckPacket();
            acks.Header.Reliable = false;
            acks.Packets = new PacketAckPacket.PacketsBlock[1];
            acks.Packets[0] = new PacketAckPacket.PacketsBlock();
            acks.Packets[0].ID = ack;

            SendPacket(acks, true);
        }

        void SendAcks()
        {
            PacketAckPacket acks = null;

            lock (pendingAcks)
            {
                if (pendingAcks.Count > 0)
                {
                    if (pendingAcks.Count > 250)
                    {
                        Logger.Log("Too many ACKs queued up!", Helpers.LogLevel.Error);
                        return;
                    }

                    acks = new PacketAckPacket();
                    acks.Header.Reliable = false;
                    acks.Packets = new PacketAckPacket.PacketsBlock[pendingAcks.Count];

                    for (int i = 0; i < pendingAcks.Count; i++)
                    {
                        acks.Packets[i] = new PacketAckPacket.PacketsBlock();
                        acks.Packets[i].ID = pendingAcks.Values[i];
                    }

                    pendingAcks.Clear();
                }
            }

            if (acks != null)
                SendPacket(acks, true);
        }

        void ResendUnacked()
        {
            lock (needAcks)
            {
                List<uint> dropAck = new List<uint>();
                int now = Environment.TickCount;

                // Resend packets
                foreach (Packet packet in needAcks.Values)
                {
                    if (packet.TickCount != 0 && now - packet.TickCount > 4000)
                    {
                        if (packet.ResendCount < 3)
                        {
                            Logger.DebugLog(String.Format("Resending packet #{0} ({1}), {2}ms have passed",
                                    packet.Header.Sequence, packet.GetType(), now - packet.TickCount));

                            packet.TickCount = 0;
                            packet.Header.Resent = true;
                            //++Stats.ResentPackets;
                            ++packet.ResendCount;

                            SendPacket(packet, false);
                        }
                        else
                        {
                            Logger.Log(String.Format("Dropping packet #{0} ({1}) after {2} failed attempts",
                                packet.Header.Sequence, packet.GetType(), packet.ResendCount), Helpers.LogLevel.Warning);

                            dropAck.Add(packet.Header.Sequence);
                        }
                    }
                }

                if (dropAck.Count != 0)
                {
                    foreach (uint seq in dropAck)
                        needAcks.Remove(seq);
                }
            }
        }

        private void AckTimer_Elapsed(object obj)
        {
            SendAcks();
            ResendUnacked();
        }
    }
}
