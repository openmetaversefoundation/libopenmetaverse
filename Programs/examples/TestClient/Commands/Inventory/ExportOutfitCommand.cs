using System;
using System.Collections.Generic;
using System.IO;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class ExportOutfitCommand : Command
    {
        public ExportOutfitCommand(TestClient testClient)
        {
            Name = "exportoutfit";
            Description = "Exports an avatars outfit to an xml file. Usage: exportoutfit [avataruuid] outputfile.xml";
            Category = CommandCategory.Inventory;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            UUID id;
            string path;

            if (args.Length == 1)
            {
                id = Client.Self.AgentID;
                path = args[0];
            }
            else if (args.Length == 2)
            {
                if (!UUID.TryParse(args[0], out id))
                    return "Usage: exportoutfit [avataruuid] outputfile.xml";
                path = args[1];
            }
            else
                return "Usage: exportoutfit [avataruuid] outputfile.xml";

            lock (Client.Appearances)
            {
                if (Client.Appearances.ContainsKey(id))
                {
                    try
                    {
                        File.WriteAllText(path, Packet.ToXmlString(Client.Appearances[id]));
                    }
                    catch (Exception e)
                    {
                        return e.ToString();
                    }

                    return "Exported appearance for avatar " + id.ToString() + " to " + args[1];
                }
                else
                {
                    return "Couldn't find an appearance for avatar " + id.ToString();
                }
            }
        }
    }
}