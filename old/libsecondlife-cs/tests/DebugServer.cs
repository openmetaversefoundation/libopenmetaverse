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
            try
            {
                libsl = new SecondLife(keywordFile, mapFile);
            }
            catch (Exception)
            {
                return;
            }

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

                if (System.Text.ASCIIEncoding.UTF8.GetString(bytes).Substring(0, 10) == "stopserver")
                {
                    Console.WriteLine("[SERVER] Received a shutdown request, stopping the server");
                    done = true;
                    break;
                }

                if ((bytes[0] & Helpers.MSG_APPENDED_ACKS) != 0)
                {
                    byte numAcks = bytes[length - 1];

                    Console.WriteLine("[SERVER] Found " + numAcks + " appended acks");

                    length = (length - numAcks * 4) - 1;
                }

                if ((bytes[0] & Helpers.MSG_ZEROCODED) != 0)
                {
                    // Allocate a temporary buffer for the zerocoded packet
                    byte[] zeroBuffer = new byte[4096];
                    int zeroBytes = Helpers.ZeroDecode(bytes, length, zeroBuffer);
                    length = zeroBytes;
                    packet = new Packet(zeroBuffer, length, libsl.Protocol);
                }
                else
                {
                    // Create the packet object from our byte array
                    packet = new Packet(bytes, length, libsl.Protocol);
                }

                if ((packet.Data[0] & Helpers.MSG_RELIABLE) != 0)
                {
                    SendACK((uint)packet.Sequence);
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
                Packet packet = new Packet("PacketAck", libsl.Protocol, 13);
                packet.Data[8] = 1;
                Array.Copy(BitConverter.GetBytes(id), 0, packet.Data, 9, 4);

                // Set the sequence number
                packet.Sequence = ++Sequence;

                Listener.SendTo(packet.Data, RemoteEndpoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
