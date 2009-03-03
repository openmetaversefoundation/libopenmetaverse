using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian
{
    public struct IncomingPacket
    {
        public UDPClient Client;
        public Packet Packet;
    }

    public class OutgoingPacket
    {
        public Packet Packet;
        /// <summary>Number of times this packet has been resent</summary>
        public int ResendCount;
        /// <summary>Environment.TickCount when this packet was last sent over the wire</summary>
        public int TickCount;
        /// <summary>Category this packet belongs to</summary>
        public PacketCategory Category;

        public OutgoingPacket(Packet packet, PacketCategory category)
        {
            Packet = packet;
            Category = category;
        }
    }

    public class UDPClient
    {
        /// <summary></summary>
        public Agent Agent;
        /// <summary></summary>
        public IPEndPoint Address;
        /// <summary>Sequence numbers of packets we've received (for duplicate checking)</summary>
        public Queue<uint> PacketArchive = new Queue<uint>();
        /// <summary>Packets we have sent that need to be ACKed by the client</summary>
        public Dictionary<uint, OutgoingPacket> NeedAcks = new Dictionary<uint, OutgoingPacket>();
        /// <summary>ACKs that are queued up, waiting to be sent to the client</summary>
        public SortedList<uint, uint> PendingAcks = new SortedList<uint, uint>();
        /// <summary>Current packet sequence number</summary>
        public int CurrentSequence = 0;

        Timer ackTimer;
        UDPServer udpServer;

        public UDPClient(UDPServer server, Agent agent, IPEndPoint address)
        {
            udpServer = server;
            Agent = agent;
            Address = address;
            ackTimer = new Timer(new TimerCallback(AckTimer_Elapsed), null, Settings.NETWORK_TICK_INTERVAL,
                Settings.NETWORK_TICK_INTERVAL);
        }

        public void Shutdown()
        {
            ackTimer.Dispose();
        }

        private void AckTimer_Elapsed(object obj)
        {
            udpServer.SendAcks(this);
            udpServer.ResendUnacked(this);
        }
    }

    public class UDPManager : IExtension<Simian>, IUDPProvider
    {
        Simian server;
        UDPServer udpServer;

        public event OutgoingPacketCallback OnOutgoingPacket;

        public UDPManager()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;
            udpServer = new UDPServer(server.UDPPort, server);
        }

        public void Stop()
        {
            if (udpServer != null)
                udpServer.Stop();
        }

        public void AddClient(Agent agent, IPEndPoint endpoint)
        {
            udpServer.AddClient(agent, endpoint);
        }

        public bool RemoveClient(Agent agent)
        {
            return udpServer.RemoveClient(agent);
        }

        public uint CreateCircuit(Agent agent)
        {
            return udpServer.CreateCircuit(agent);
        }

        public void SendPacket(UUID agentID, Packet packet, PacketCategory category)
        {
            if (OnOutgoingPacket == null || OnOutgoingPacket(packet, agentID, category))
                udpServer.SendPacket(agentID, packet, category);
        }

        public void BroadcastPacket(Packet packet, PacketCategory category)
        {
            if (OnOutgoingPacket == null || OnOutgoingPacket(packet, UUID.Zero, category))
                udpServer.BroadcastPacket(packet, category);
        }

        public void RegisterPacketCallback(PacketType type, PacketCallback callback)
        {
            udpServer.RegisterPacketCallback(type, callback);
        }
    }

    public class UDPServer : UDPBase
    {
        /// <summary>This is only used to fetch unassociated agents, which will
        /// be exposed through a login interface at some point</summary>
        Simian server;
        /// <summary>Handlers for incoming packets</summary>
        PacketEventDictionary packetEvents = new PacketEventDictionary();
        /// <summary>Incoming packets that are awaiting handling</summary>
        BlockingQueue<IncomingPacket> packetInbox = new BlockingQueue<IncomingPacket>(Settings.PACKET_INBOX_SIZE);
        /// <summary></summary>
        DoubleDictionary<UUID, IPEndPoint, UDPClient> clients = new DoubleDictionary<UUID, IPEndPoint, UDPClient>();
        /// <summary></summary>
        Dictionary<uint, Agent> unassociatedAgents = new Dictionary<uint, Agent>();
        /// <summary></summary>
        int currentCircuitCode = 0;

        public UDPServer(int port, Simian server)
            : base(port)
        {
            this.server = server;

            Start();

            // Start the incoming packet processing thread
            Thread incomingThread = new Thread(new ThreadStart(IncomingPacketHandler));
            incomingThread.Start();
        }

        public void RegisterPacketCallback(PacketType type, PacketCallback callback)
        {
            packetEvents.RegisterEvent(type, callback);
        }

        public void AddClient(Agent agent, IPEndPoint endpoint)
        {
            UDPClient client = new UDPClient(this, agent, endpoint);
            clients.Add(agent.ID, endpoint, client);
        }

        public bool RemoveClient(Agent agent)
        {
            UDPClient client;
            if (clients.TryGetValue(agent.ID, out client))
            {
                client.Shutdown();
                return clients.Remove(agent.ID, client.Address);
            }
            else
                return false;
        }

        public uint CreateCircuit(Agent agent)
        {
            uint circuitCode = (uint)Interlocked.Increment(ref currentCircuitCode);

            // Put this client in the list of clients that have not been associated with an IPEndPoint yet
            lock (unassociatedAgents)
                unassociatedAgents[circuitCode] = agent;

            Logger.Log("Created circuit " + circuitCode + " for " + agent.FirstName, Helpers.LogLevel.Info);

            return circuitCode;
        }

        public void BroadcastPacket(Packet packet, PacketCategory category)
        {
            clients.ForEach(
                delegate(UDPClient client) { SendPacket(client, new OutgoingPacket(packet, category)); });
        }

        public void SendPacket(UUID agentID, Packet packet, PacketCategory category)
        {
            // Look up the UDPClient this is going to
            UDPClient client;
            if (clients.TryGetValue(agentID, out client))
                SendPacket(client, new OutgoingPacket(packet, category));
        }

        void SendPacket(UDPClient client, OutgoingPacket outgoingPacket)
        {
            Packet packet = outgoingPacket.Packet;
            byte[] buffer;
            int bytes;

            // Update the sent time for this packet
            outgoingPacket.TickCount = Environment.TickCount;

            if (!packet.Header.Resent)
            {
                // Reset to zero if we've hit the upper sequence number limit
                Interlocked.CompareExchange(ref client.CurrentSequence, 0, 0xFFFFFF);
                // Increment and fetch the current sequence number
                uint sequence = (uint)Interlocked.Increment(ref client.CurrentSequence);
                packet.Header.Sequence = sequence;

                if (packet.Header.Reliable)
                {
                    // Add this packet to the list of ACK responses we are waiting on from this client
                    lock (client.NeedAcks)
                        client.NeedAcks[sequence] = outgoingPacket;

                    // This packet is reliable and not a resend, check if the conditions are favorable
                    // to ACK appending
                    if (packet.Type != PacketType.PacketAck)
                    {
                        lock (client.PendingAcks)
                        {
                            int count = client.PendingAcks.Count;

                            if (count > 0 && count < 10)
                            {
                                // Append all of the queued up outgoing ACKs to this packet
                                packet.Header.AckList = new uint[count];

                                for (int i = 0; i < count; i++)
                                    packet.Header.AckList[i] = client.PendingAcks.Values[i];

                                client.PendingAcks.Clear();
                                packet.Header.AppendedAcks = true;
                            }
                        }
                    }
                }
            }
            else
            {
                // This packet has already been sent out once, strip any appended ACKs
                // off it and reinsert them into the outgoing ACK queue under the 
                // assumption that this packet will continually be rejected from the
                // client or that the appended ACKs are possibly making the delivery fail
                if (packet.Header.AckList.Length > 0)
                {
                    Logger.DebugLog(String.Format("Purging ACKs from packet #{0} ({1}) which will be resent.",
                        packet.Header.Sequence, packet.GetType()));

                    lock (client.PendingAcks)
                    {
                        foreach (uint ack in packet.Header.AckList)
                        {
                            if (!client.PendingAcks.ContainsKey(ack))
                                client.PendingAcks[ack] = ack;
                        }
                    }

                    packet.Header.AppendedAcks = false;
                    packet.Header.AckList = new uint[0];
                }
            }

            // Serialize the packet
            buffer = packet.ToBytes();
            bytes = buffer.Length;
            //Stats.SentBytes += (ulong)bytes;
            //++Stats.SentPackets;

            UDPPacketBuffer buf = new UDPPacketBuffer(client.Address);

            // Zerocode if needed
            if (packet.Header.Zerocoded)
                bytes = Helpers.ZeroEncode(buffer, bytes, buf.Data);
            else
                Buffer.BlockCopy(buffer, 0, buf.Data, 0, bytes);

            buf.DataLength = bytes;

            AsyncBeginSend(buf);
        }

        void QueueAck(UDPClient client, uint ack)
        {
            // Add this packet to the list of ACKs that need to be sent out
            lock (client.PendingAcks)
                client.PendingAcks[ack] = ack;

            // Send out ACKs if we have a lot of them
            if (client.PendingAcks.Count >= 10)
                SendAcks(client);
        }

        void ProcessAcks(UDPClient client, List<uint> acks)
        {
            lock (client.NeedAcks)
            {
                foreach (uint ack in acks)
                    client.NeedAcks.Remove(ack);                    
            }
        }

        void SendAck(UDPClient client, uint ack)
        {
            PacketAckPacket acks = new PacketAckPacket();
            acks.Header.Reliable = false;
            acks.Packets = new PacketAckPacket.PacketsBlock[1];
            acks.Packets[0] = new PacketAckPacket.PacketsBlock();
            acks.Packets[0].ID = ack;

            SendPacket(client, new OutgoingPacket(acks, PacketCategory.Overhead));
        }

        public void SendAcks(UDPClient client)
        {
            PacketAckPacket acks = null;

            lock (client.PendingAcks)
            {
                int count = client.PendingAcks.Count;

                if (count > 250)
                {
                    Logger.Log("Too many ACKs queued up!", Helpers.LogLevel.Error);
                    return;
                }
                else if (count > 0)
                {
                    acks = new PacketAckPacket();
                    acks.Header.Reliable = false;
                    acks.Packets = new PacketAckPacket.PacketsBlock[count];

                    for (int i = 0; i < count; i++)
                    {
                        acks.Packets[i] = new PacketAckPacket.PacketsBlock();
                        acks.Packets[i].ID = client.PendingAcks.Values[i];
                    }

                    client.PendingAcks.Clear();
                }
            }

            if (acks != null)
                SendPacket(client, new OutgoingPacket(acks, PacketCategory.Overhead));
        }

        public void ResendUnacked(UDPClient client)
        {
            if (client.NeedAcks.Count > 0)
            {
                OutgoingPacket[] array;
                int now = Environment.TickCount;

                lock (client.NeedAcks)
                {
                    // Create a temporary copy of the outgoing packets array to iterate over
                    array = new OutgoingPacket[client.NeedAcks.Count];
                    client.NeedAcks.Values.CopyTo(array, 0);
                }

                // Resend packets
                for (int i = 0; i < array.Length; i++)
                {
                    OutgoingPacket outgoing = array[i];

                    // FIXME: Make 4000 and 3 .ini settings
                    if (outgoing.TickCount != 0 && now - outgoing.TickCount > 4000)
                    {
                        if (outgoing.ResendCount < 3)
                        {
                            //Logger.DebugLog(String.Format("Resending packet #{0} ({1}), {2}ms have passed",
                            //        outgoing.Packet.Header.Sequence, outgoing.Packet.GetType(), now - outgoing.TickCount));

                            // The TickCount will be set to the current time when the packet
                            // is actually sent out again
                            outgoing.TickCount = 0;
                            outgoing.Packet.Header.Resent = true;
                            ++outgoing.ResendCount;

                            //++Stats.ResentPackets;

                            try
                            {
                                SendPacket(client, outgoing);
                            }
                            catch (NullReferenceException)
                            {
                                // Programming error elsewhere can put bad packets in the outgoing queue.
                                // These will never successfully serialize, so just drop them
                                Logger.Log(String.Format("Dropping bad packet #{0} ({1}) from the outgoing queue",
                                    outgoing.Packet.Header.Sequence, outgoing.Packet.GetType()),
                                    Helpers.LogLevel.Warning);

                                lock (client.NeedAcks) client.NeedAcks.Remove(outgoing.Packet.Header.Sequence);
                            }
                        }
                        else
                        {
                            Logger.Log(String.Format("Dropping packet #{0} ({1}) after {2} failed attempts",
                                outgoing.Packet.Header.Sequence, outgoing.Packet.GetType(), outgoing.ResendCount),
                                Helpers.LogLevel.Warning);

                            lock (client.NeedAcks) client.NeedAcks.Remove(outgoing.Packet.Header.Sequence);

                            //Disconnect an agent if no packets are received for some time
                            //FIXME: Make 60000 an .ini setting
                            if (Environment.TickCount - client.Agent.TickLastPacketReceived > 60000)
                            {
                                Logger.Log(String.Format("Ack timeout for {0}, disconnecting", client.Agent.FullName),
                                    Helpers.LogLevel.Warning);

                                server.Scene.ObjectRemove(this, client.Agent.ID);
                                return;
                            }
                        }
                    }
                }
            }
        }

        protected override void PacketReceived(UDPPacketBuffer buffer)
        {
            UDPClient client = null;
            Packet packet = null;
            int packetEnd = buffer.DataLength - 1;
            IPEndPoint address = (IPEndPoint)buffer.RemoteEndPoint;

            // Decoding
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
                Logger.Log("Couldn't build a message from the incoming data", Helpers.LogLevel.Warning);
                return;
            }

            //Stats.RecvBytes += (ulong)buffer.DataLength;
            //++Stats.RecvPackets;

            if (packet.Type == PacketType.UseCircuitCode)
            {
                UseCircuitCodePacket useCircuitCode = (UseCircuitCodePacket)packet;

                Agent agent;
                if (CompleteAgentConnection(useCircuitCode.CircuitCode.Code, out agent))
                {
                    // Sanity check that the agent isn't already logged in
                    if (clients.ContainsKey(agent.ID))
                    {
                        Logger.Log("Client UDP reference already exists for " + agent.ID.ToString() + ", removing",
                            Helpers.LogLevel.Warning);
                        server.Scene.ObjectRemove(this, agent.ID);
                        clients.Remove(agent.ID);
                    }

                    AddClient(agent, address);
                    if (clients.TryGetValue(agent.ID, out client))
                    {
                        Logger.Log("Activated UDP circuit " + useCircuitCode.CircuitCode.Code, Helpers.LogLevel.Info);
                    }
                    else
                    {
                        Logger.Log("Failed to locate newly created UDPClient", Helpers.LogLevel.Error);
                        return;
                    }
                }
                else
                {
                    Logger.Log("Received a UseCircuitCode packet for an unrecognized circuit: " +
                        useCircuitCode.CircuitCode.Code.ToString(), Helpers.LogLevel.Warning);
                    return;
                }
            }
            else
            {
                // Determine which agent this packet came from
                if (!clients.TryGetValue(address, out client))
                {
                    Logger.Log("Received UDP packet from an unrecognized source: " + address.ToString(),
                        Helpers.LogLevel.Warning);
                    return;
                }
            }

            client.Agent.TickLastPacketReceived = Environment.TickCount;

            // Reliable handling
            if (packet.Header.Reliable)
            {
                // Queue up this sequence number for acknowledgement
                QueueAck(client, (uint)packet.Header.Sequence);
                //if (packet.Header.Resent) ++Stats.ReceivedResends;
            }

            // Inbox insertion
            IncomingPacket incomingPacket;
            incomingPacket.Client = client;
            incomingPacket.Packet = packet;

            // TODO: Prioritize the queue
            packetInbox.Enqueue(incomingPacket);
        }

        protected override void PacketSent(UDPPacketBuffer buffer, int bytesSent)
        {
        }

        void IncomingPacketHandler()
        {
            IncomingPacket incomingPacket = new IncomingPacket();
            Packet packet = null;
            UDPClient client = null;

            while (IsRunning)
            {
                // Reset packet to null for the check below
                packet = null;

                if (packetInbox.Dequeue(100, ref incomingPacket))
                {
                    packet = incomingPacket.Packet;
                    client = incomingPacket.Client;

                    if (packet != null)
                    {
                        #region ACK accounting

                        // Check the archives to see whether we already received this packet
                        lock (client.PacketArchive)
                        {
                            if (client.PacketArchive.Contains(packet.Header.Sequence))
                            {
                                if (packet.Header.Resent)
                                {
                                    Logger.DebugLog("Received resent packet #" + packet.Header.Sequence);
                                }
                                else
                                {
                                    Logger.Log(String.Format("Received a duplicate of packet #{0}, current type: {1}",
                                        packet.Header.Sequence, packet.Type), Helpers.LogLevel.Warning);
                                }

                                // Avoid firing a callback twice for the same packet
                                continue;
                            }
                            else
                            {
                                // Keep the PacketArchive size within a certain capacity
                                while (client.PacketArchive.Count >= Settings.PACKET_ARCHIVE_SIZE)
                                {
                                    client.PacketArchive.Dequeue(); client.PacketArchive.Dequeue();
                                    client.PacketArchive.Dequeue(); client.PacketArchive.Dequeue();
                                }

                                client.PacketArchive.Enqueue(packet.Header.Sequence);
                            }
                        }

                        #endregion ACK accounting

                        #region ACK handling

                        // Handle appended ACKs
                        if (packet.Header.AppendedAcks)
                        {
                            lock (client.NeedAcks)
                            {
                                for (int i = 0; i < packet.Header.AckList.Length; i++)
                                    client.NeedAcks.Remove(packet.Header.AckList[i]);
                            }
                        }

                        // Handle PacketAck packets
                        if (packet.Type == PacketType.PacketAck)
                        {
                            PacketAckPacket ackPacket = (PacketAckPacket)packet;

                            lock (client.NeedAcks)
                            {
                                for (int i = 0; i < ackPacket.Packets.Length; i++)
                                    client.NeedAcks.Remove(ackPacket.Packets[i].ID);
                            }
                        }

                        #endregion ACK handling

                        packetEvents.BeginRaiseEvent(packet.Type, packet, client.Agent);
                    }
                }
            }
        }

        bool CompleteAgentConnection(uint circuitCode, out Agent agent)
        {
            lock (unassociatedAgents)
            {
                if (unassociatedAgents.TryGetValue(circuitCode, out agent))
                {
                    unassociatedAgents.Remove(circuitCode);
                    server.Scene.AgentAdd(this, agent, PrimFlags.None);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
