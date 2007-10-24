using System;
using System.Collections.Generic;
using System.IO;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class ExportOutfitCommand : Command
    {
        public ExportOutfitCommand(TestClient testClient)
        {
            Name = "exportoutfit";
            Description = "Exports an avatars outfit to an xml file. Usage: exportoutfit [avataruuid] outputfile.xml";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            LLUUID id;
            string path;

            if (args.Length == 1)
            {
                id = Client.Self.AgentID;
                path = args[0];
            }
            else if (args.Length == 2)
            {
                if (!LLUUID.TryParse(args[0], out id))
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
                        File.WriteAllText(path, Packet.SerializeToXml(Client.Appearances[id]));
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