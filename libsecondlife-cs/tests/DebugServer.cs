using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Nwc.XmlRpc;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.Tests
{
    public class DebugServer
    {
        public bool Initialized = false;

        private SecondLife libsl;
        private bool done = false;
        private Socket Listener;
        private IPEndPoint Endpoint;
        EndPoint RemoteEndpoint = new IPEndPoint(IPAddress.Loopback, 0);
        private ushort Sequence = 0;

        public DebugServer(string keywordFile, string mapFile, int port)
        {
            libsl = new SecondLife();

            BindSocket(port);
        }

        private void BindSocket(int port)
        {
            Endpoint = new IPEndPoint(IPAddress.Loopback, port);
            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            Console.WriteLine("[SERVER] Binding a UDP socket to " + Endpoint.ToString());

            try
            {
                Listener.Bind(Endpoint);
            }
            catch (SocketException)
            {
                Console.WriteLine("[SERVER] Failed to bind to " + Endpoint.ToString());
                return;
            }

            // Start listening for incoming data
            Thread thread = new Thread(new ThreadStart(Listen));
            thread.Start();

            Initialized = true;
        }

        private void Listen()
        {
            Packet packet;
            int length;
            byte[] bytes = new byte[4096];

            Console.WriteLine("[SERVER] Listening for incoming data on " + Endpoint.ToString());

            while (!done)
            {
                packet = null;

                // Grab the next packet
                length = Listener.ReceiveFrom(bytes, ref RemoteEndpoint);

                Console.WriteLine("[SERVER] Received a packet from {0}", RemoteEndpoint.ToString());

                if (Helpers.FieldToString(bytes).StartsWith("stopserver"))
                {
                    Console.WriteLine("[SERVER] Received a shutdown request, stopping the server");
                    done = true;
                    break;
                }

                int packetEnd = length - 1;
                packet = Packet.BuildPacket(bytes, ref packetEnd);

                if (packet.Header.AppendedAcks)
                {
                    Console.WriteLine("[SERVER] Found " + packet.Header.AckList.Length + " appended acks");
                }

                if (packet.Header.Reliable)
                {
                    SendACK((uint)packet.Header.Sequence);
                }

                Console.WriteLine(packet.ToString());
            }

            Console.WriteLine("[SERVER] Shutting down the socket on " + Endpoint.ToString());
            Listener.Close();
        }

        private void SendACK(uint id)
        {
            try
            {
                PacketAckPacket ack = new PacketAckPacket();
                ack.Packets = new PacketAckPacket.PacketsBlock[1];
                ack.Packets[0].ID = id;

                ack.Header.Reliable = false;

                // Set the sequence number
                ack.Header.Sequence = ++Sequence;

                Listener.SendTo(ack.ToBytes(), RemoteEndpoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
