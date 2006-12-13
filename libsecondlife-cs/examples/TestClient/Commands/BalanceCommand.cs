using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class BalanceCommand: Command
    {
		public BalanceCommand()
		{
			Name = "balance";
			Description = "Shows the amount of L$.";
		}

        public override string Execute(SecondLife Client, string[] args, LLUUID fromAgentID)
		{
			return Client.ToString() + " has L$: " + Client.Self.Balance;
		}
    }
}
