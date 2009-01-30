using System;
using System.Collections.Generic;
using System.Text;

namespace OpenMetaverse.TestClient.Commands
{
    class ShowEventDetailsCommand : Command
    {
        public ShowEventDetailsCommand(TestClient testClient)
        {
            Name = "showevent";
            Description = "Shows an Events details. Usage: showevent [eventID]";
            Category = CommandCategory.Other;
        }

        public override string Execute(string[] args, Guid fromAgentID)
        {
            if (args.Length < 1)
                return "Usage: showevent [eventID] (use searchevents to get ID)";

            Client.Directory.OnEventInfo += new DirectoryManager.EventInfoCallback(Directory_OnEventInfo);
            uint eventID;

            if (UInt32.TryParse(args[0], out eventID))
            {
                Client.Directory.EventInfoRequest(eventID);
                return "Query Sent";
            }
            else
            {
                return "Usage: showevent [eventID] (use searchevents to get ID)";
            }
        }

        void Directory_OnEventInfo(DirectoryManager.EventInfo matchedEvent)
        {
            float x,y;
            Helpers.GlobalPosToRegionHandle((float)matchedEvent.GlobalPos.X, (float)matchedEvent.GlobalPos.Y, out x, out y);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("       Name: {0} ({1})" + System.Environment.NewLine, matchedEvent.Name, matchedEvent.ID);
            sb.AppendFormat("   Location: {0}/{1}/{2}" + System.Environment.NewLine, matchedEvent.SimName, x, y);
            sb.AppendFormat("       Date: {0}" + System.Environment.NewLine, matchedEvent.Date);
            sb.AppendFormat("Description: {0}" + System.Environment.NewLine, matchedEvent.Desc);
            Console.WriteLine(sb.ToString());
        }
    }
}
