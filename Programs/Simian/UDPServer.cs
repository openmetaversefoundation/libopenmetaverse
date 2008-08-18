using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian
{
    public struct IncomingPacket
    {
        public Agent Agent;
        public Packet Packet;
    }

    public class UDPServer : UDPBase
    {
        /// <summary>
        /// Coupled with RegisterCallback(), this is triggered whenever a packet
        /// of a registered type is received
        /// </summary>
        public delegate void PacketCallback(Packet packet, Agent agent);

        /// <summary>This is only used to fetch unassociated agents, which will
        /// be exposed through a login interface at some point</summary>
        Simian server;
        /// <summary>Handlers for incoming packets</summary>
        PacketEventDictionary packetEvents = new PacketEventDictionary();
        /// <summary>Incoming packets that are awaiting handling</summary>
        BlockingQueue<IncomingPacket> packetInbox = new BlockingQueue<IncomingPacket>(Settings.PACKET_INBOX_SIZE);

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

        protected override void PacketReceived(UDPPacketBuffer buffer)
        {
            Agent agent = null;
            Packet packet = null;
            int packetEnd = buffer.DataLength - 1;

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

                if (server.TryGetUnassociatedAgent(useCircuitCode.CircuitCode.Code, out agent))
                {
                    agent.Initialize((IPEndPoint)buffer.RemoteEndPoint);

                    lock (server.Agents)
                        server.Agents[(IPEndPoint)buffer.RemoteEndPoint] = agent;

                    Logger.Log("Activated UDP circuit " + useCircuitCode.CircuitCode.Code, Helpers.LogLevel.Info);

                    //agent.SendAck(useCircuitCode.Header.Sequence);
                }
                else
                {
                    Logger.Log("Received a UseCircuitCode packet for an unrecognized circuit: " + useCircuitCode.CircuitCode.Code.ToString(),
                        Helpers.LogLevel.Warning);
                    return;
                }
            }
            else
            {
                // Determine which agent this packet came from
                if (!server.Agents.TryGetValue((IPEndPoint)buffer.RemoteEndPoint, out agent))
                {
                    Logger.Log("Received UDP packet from an unrecognized source: " + ((IPEndPoint)buffer.RemoteEndPoint).ToString(),
                        Helpers.LogLevel.Warning);
                    return;
                }
            }

            // Reliable handling
            if (packet.Header.Reliable)
            {
                // Queue up this sequence number for acknowledgement
                agent.QueueAck((uint)packet.Header.Sequence);

                //if (packet.Header.Resent) ++Stats.ReceivedResends;
            }

            // Inbox insertion
            IncomingPacket incomingPacket;
            incomingPacket.Agent = agent;
            incomingPacket.Packet = packet;

            // TODO: Prioritize the queue
            packetInbox.Enqueue(incomingPacket);
        }

        protected override void PacketSent(UDPPacketBuffer buffer, int bytesSent)
        {
        }

        private void IncomingPacketHandler()
        {
            IncomingPacket incomingPacket = new IncomingPacket();
            Packet packet = null;
            Agent agent = null;

            while (IsRunning)
            {
                // Reset packet to null for the check below
                packet = null;

                if (packetInbox.Dequeue(100, ref incomingPacket))
                {
                    packet = incomingPacket.Packet;
                    agent = incomingPacket.Agent;

                    if (packet != null)
                    {
                        #region ACK accounting

                        // Check the archives to see whether we already received this packet
                        lock (agent.packetArchive)
                        {
                            if (agent.packetArchive.Contains(packet.Header.Sequence))
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
                                while (agent.packetArchive.Count >= Settings.PACKET_ARCHIVE_SIZE)
                                {
                                    agent.packetArchive.Dequeue(); agent.packetArchive.Dequeue();
                                    agent.packetArchive.Dequeue(); agent.packetArchive.Dequeue();
                                }

                                agent.packetArchive.Enqueue(packet.Header.Sequence);
                            }
                        }

                        #endregion ACK accounting

                        #region ACK handling

                        // Handle appended ACKs
                        if (packet.Header.AppendedAcks)
                        {
                            lock (agent.needAcks)
                            {
                                for (int i = 0; i < packet.Header.AckList.Length; i++)
                                    agent.needAcks.Remove(packet.Header.AckList[i]);
                            }
                        }

                        // Handle PacketAck packets
                        if (packet.Type == PacketType.PacketAck)
                        {
                            PacketAckPacket ackPacket = (PacketAckPacket)packet;

                            lock (agent.needAcks)
                            {
                                for (int i = 0; i < ackPacket.Packets.Length; i++)
                                    agent.needAcks.Remove(ackPacket.Packets[i].ID);
                            }
                        }

                        #endregion ACK handling

                        packetEvents.BeginRaiseEvent(packet.Type, packet, agent);
                    }
                }
            }
        }
    }
}
