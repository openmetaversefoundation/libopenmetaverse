using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class GiveAllCommand: Command
    {
        SecondLife Client;

		public GiveAllCommand(TestClient testClient)
		{
            TestClient = testClient;
            Client = (SecondLife)TestClient;

			Name = "giveAll";
			Description = "Gives you all it's money.";
		}

        public override string Execute(string[] args, LLUUID fromAgentID)
		{
			if (fromAgentID == null)
				return "Unable to send money to console.  This command only works when IMed.";

		    int amount = Client.Self.Balance;
		    Client.Self.GiveMoney(fromAgentID, Client.Self.Balance, String.Empty);
		    return "Gave $" + amount + " to " + fromAgentID;
		}
    }
}
