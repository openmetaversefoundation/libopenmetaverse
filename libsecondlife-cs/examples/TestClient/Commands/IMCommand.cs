using System;
using System.Collections.Generic;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class ImCommand : Command
    {
        bool DirLookupComplete = false;

        public ImCommand()
        {
            Name = "im";
            Description = "Instant message someone. Usage: im [firstname] [lastname] [message]";
        }

        public override string Execute(SecondLife Client, string[] args, LLUUID fromAgentID)
        {
            return "FIXME";
            // How do we register the callback only once for each client?

            if (args.Length < 3)
                return "Usage: im [firstname] [lastname] [message]";

            string toAgentName = args[0] + " " + args[1];

            string message = String.Empty;
            for (int ct = 2; ct < args.Length; ct++)
                message += args[ct] + " ";
            message = message.TrimEnd();
            if (message.Length > 1023) message = message.Remove(1023);

            if (!TestClient.SharedValues.ContainsKey("name2key"))
            {
                // Initialize the shared name2key dictionary
                TestClient.SharedValues["name2key"] = new Dictionary<string, LLUUID>();
            }

            Dictionary<string, LLUUID> name2key = (Dictionary<string, LLUUID>)TestClient.SharedValues["name2key"];

            if (name2key.ContainsKey(toAgentName))
            {
                if (name2key[toAgentName] != LLUUID.Zero)
                {
                    Client.Self.InstantMessage(name2key[toAgentName], message);
                    return "IM sent to " + name2key[toAgentName].ToStringHyphenated();
                }
                else
                {
                    return "Lookup failed for " + toAgentName;
                }
            }
            else
            {
                // Send the Query
                DirFindQueryPacket find = new DirFindQueryPacket();
                find.AgentData.AgentID = Client.Network.AgentID;
                find.AgentData.SessionID = Client.Network.SessionID;
                find.QueryData.QueryFlags = 1;
                find.QueryData.QueryText = Helpers.StringToField(toAgentName);
                find.QueryData.QueryID = new LLUUID("00000000000000000000000000000001");
                find.QueryData.QueryStart = 0;

                Client.Network.SendPacket(find);

                while (!DirLookupComplete)
                {
                    // Wait for 
                }
            }

            return "ERROR: IM TERMINATED";
        }

        private void OnDirFindReply(Simulator simulator, Packet packet)
        {
            ;
        }
    }
}
