using System;
using libsecondlife;

namespace libsecondlife.TestClient
{
    public class PacketLogCommand : Command
    {
        public PacketLogCommand(TestClient testClient)
        {
            Name = "packetlog";
            Description = "Logs a given number of packets to an xml file. Usage: packetlog 10 tenpackets.xml";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            if (args.Length != 2)
                return "Usage: packetlog 10 tenpackets.xml";

            return "This function is currently unimplemented";
        }
    }
}
