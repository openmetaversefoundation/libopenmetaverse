using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class WhoCommand: Command
    {
        SecondLife Client;

        public WhoCommand(TestClient testClient)
		{
            TestClient = testClient;
            Client = (SecondLife)TestClient;

			Name = "who";
			Description = "Lists seen avatars.";
		}

        public override string Execute(string[] args, LLUUID fromAgentID)
		{
			StringBuilder result = new StringBuilder();
			foreach (Avatar av in TestClient.AvatarList.Values)
			{
				result.AppendFormat("\n{0} {1} {2}/{3} ID: {4}", av.Name, av.GroupName, av.CurrentRegion != null ? av.CurrentRegion.Name : String.Empty, av.Position, av.ID);
			}

            return result.ToString();
		}
    }
}
