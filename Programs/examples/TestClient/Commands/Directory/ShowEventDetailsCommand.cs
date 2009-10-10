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

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length < 1)
                return "Usage: showevent [eventID] (use searchevents to get ID)";
            
            Client.Directory.EventInfoReply += Directory_EventDetails;
            uint eventID;

            if (UInt32.TryParse(args[0], out eventID))
            {
                Client.Directory.EventInfoRequest(eventID);
                return "Sent query for Event " + eventID;
            }
            else
            {
                return "Usage: showevent [eventID] (use searchevents to get ID)";
            }
        }

        void Directory_EventDetails(object sender, EventInfoReplyEventArgs e)
        {
            float x, y;
            Helpers.GlobalPosToRegionHandle((float)e.MatchedEvent.GlobalPos.X, (float)e.MatchedEvent.GlobalPos.Y, out x, out y);
            StringBuilder sb = new StringBuilder("secondlife://" + e.MatchedEvent.SimName + "/" + x + "/" + y + "/0" + System.Environment.NewLine);
            sb.AppendLine(e.MatchedEvent.ToString());
            
            //sb.AppendFormat("       Name: {0} ({1})" + System.Environment.NewLine, e.MatchedEvent.Name, e.MatchedEvent.ID);
            //sb.AppendFormat("   Location: {0}/{1}/{2}" + System.Environment.NewLine, e.MatchedEvent.SimName, x, y);
            //sb.AppendFormat("       Date: {0}" + System.Environment.NewLine, e.MatchedEvent.Date);
            //sb.AppendFormat("Description: {0}" + System.Environment.NewLine, e.MatchedEvent.Desc);
            Console.WriteLine(sb.ToString());
            Client.Directory.EventInfoReply -= Directory_EventDetails;
        }
    }
}
