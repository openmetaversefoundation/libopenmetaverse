using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class WhoCommand: Command
    {
		public WhoCommand()
		{
			Name = "who";
			Description = "Lists seen avatars.";
		}

        public override string Execute(SecondLife Client, string[] args, LLUUID fromAgentID)
		{
			StringBuilder result = new StringBuilder();
			foreach (Avatar av in TestClient.Avatars.Values)
			{
				result.AppendFormat("\n{0} {1}", av.Name, av.GroupName);
			}

            return result.ToString();
		}
    }
}
