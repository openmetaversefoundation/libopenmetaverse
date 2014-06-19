using System;
using System.IO;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class PacketLogCommand : Command
    {
        private TestClient m_client;
        private bool m_isLogging;
        private int m_packetsToLogRemaining;
        private StreamWriter m_logStreamWriter;

        public PacketLogCommand(TestClient testClient)
        {
            Name = "logpacket";
            Description = "Logs a given number of packets to a file.  For example, packetlog 10 tenpackets.xml";
            Category = CommandCategory.TestClient;

            m_client = testClient;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length != 2)
                return string.Format("Usage: {0} no-of-packets filename", Name);

            string rawNumberOfPackets = args[0];
            string path = args[1];
            int numberOfPackets;

            if (!int.TryParse(args[0], out numberOfPackets) || numberOfPackets <= 0)
                return string.Format(
                    "{0} is not a valid number of packets for {1}", rawNumberOfPackets, m_client.Self.Name);

            lock (this)
            {
                if (m_isLogging)
                    return string.Format(
                        "Still waiting to finish logging {0} packets for {1}", 
                        m_packetsToLogRemaining, m_client.Self.Name);

                try
                {
                    m_logStreamWriter = new StreamWriter(path);
                }
                catch (Exception e)
                {
                    return string.Format("Could not open file with path [{0}], exception {1}", path, e);
                }

                m_isLogging = true;
            }

            m_packetsToLogRemaining = numberOfPackets;
            m_client.Network.RegisterCallback(PacketType.Default, HandlePacket);
            return string.Format("Now logging {0} packets for {1}", m_packetsToLogRemaining, m_client.Self.Name);
        }

        /// <summary>
        /// Logs the packet received for this client.
        /// </summary>
        /// <remarks>
        /// This handler assumes that packets are processed one at a time.
        /// </remarks>
        /// <param name="sender">Sender.</param>
        /// <param name="args">Arguments.</param>
        private void HandlePacket(object sender, PacketReceivedEventArgs args)
        {
//            Console.WriteLine(
//                "Received packet {0} from {1} for {2}", args.Packet.Type, args.Simulator.Name, m_client.Self.Name);

            lock (this)
            {
                if (!m_isLogging)
                    return;               

                m_logStreamWriter.WriteLine("Received: {0}", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff"));

                try
                {
                    m_logStreamWriter.WriteLine(PacketDecoder.PacketToString(args.Packet));
                }
                catch (Exception e)
                {
                    m_logStreamWriter.WriteLine("Failed to write decode of {0}, exception {1}", args.Packet.Type, e);
                }

                if (--m_packetsToLogRemaining <= 0)
                {
                    m_client.Network.UnregisterCallback(PacketType.Default, HandlePacket);
                    m_logStreamWriter.Close();
                    Console.WriteLine("Finished logging packets for {0}", m_client.Self.Name);
                    m_isLogging = false;
                }
            }
        }
    }
}