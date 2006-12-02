using System;
using System.Collections.Generic;
using System.Xml;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class PacketLogCommand : Command
    {
        XmlWriter Writer;
        bool Done = false;
        int Count = 0;
        int Total = 0;

        public PacketLogCommand()
        {
            Name = "packetlog";
            Description = "Logs a given number of packets to an xml file. Usage: packetlog 10 tenpackets.xml";
        }

        public override string Execute(SecondLife Client, string[] args, LLUUID fromAgentID)
        {
            if (args.Length != 2)
                return "Usage: packetlog 10 tenpackets.xml";

            Done = false;
            Count = 0;
            NetworkManager.PacketCallback callback = new NetworkManager.PacketCallback(OnPacket);

            try
            {
                Total = Int32.Parse(args[0]);
                Writer = XmlWriter.Create(args[1]);
                Writer.WriteStartElement("packets");

                Client.Network.RegisterCallback(PacketType.Default, callback);
            }
            catch (Exception e)
            {
                return "Usage: packetlog 10 tenpackets.xml" + Environment.NewLine + e;
            }

            while (!Done)
            {
                System.Threading.Thread.Sleep(100);
            }

            Client.Network.UnregisterCallback(PacketType.Default, callback);

            lock (Writer)
            {
                Writer.WriteEndElement();
                Writer.Close();
            }

            return "Exported " + Count + " packets to " + args[1];
        }

        private void OnPacket(Packet packet, Simulator simulator)
        {
            lock (Writer)
            {
                if (Writer.WriteState == WriteState.Error)
                {
                    Done = true;
                }
                else if (Count >= Total)
                {
                    Done = true;
                }
                else
                {
                    packet.ToXml(Writer);
                    Count++;
                }
            }
        }
    }
}
