using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace TestBot
{
    public class BalanceCommand: Command
    {
		public BalanceCommand()
		{
			Name = "balance";
			Description = "Shows the amount of $L the bot has.";
		}

		public override string Execute(string[] args, LLUUID fromAgentID)
		{
			return "$L: " + Client.Self.Balance;
		}
    }
}
