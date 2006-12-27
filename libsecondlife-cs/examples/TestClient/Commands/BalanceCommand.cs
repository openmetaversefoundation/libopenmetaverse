using System;
using System.Collections.Generic;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class BalanceCommand: Command
    {
        SecondLife Client;

        public BalanceCommand(TestClient testClient)
		{
            TestClient = testClient;
            Client = (SecondLife)TestClient;

			Name = "balance";
			Description = "Shows the amount of L$.";
		}

        public override string Execute(string[] args, LLUUID fromAgentID)
		{
			return Client.ToString() + " has L$: " + Client.Self.Balance;
		}
    }
}
