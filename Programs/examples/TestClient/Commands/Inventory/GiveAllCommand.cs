using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class GiveAllCommand: Command
    {
		public GiveAllCommand(TestClient testClient)
		{
			Name = "giveAll";
			Description = "Gives you all it's money.";
            Category = CommandCategory.Other;
		}

        public override string Execute(string[] args, UUID fromAgentID)
		{
			if (fromAgentID == UUID.Zero)
				return "Unable to send money to console.  This command only works when IMed.";

		    int amount = Client.Self.Balance;
		    Client.Self.GiveAvatarMoney(fromAgentID, Client.Self.Balance, "TestClient.GiveAll");
		    return "Gave $" + amount + " to " + fromAgentID;
		}
    }
}
