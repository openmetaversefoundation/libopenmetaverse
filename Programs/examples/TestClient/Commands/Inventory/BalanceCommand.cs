using System;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class BalanceCommand: Command
    {
        public BalanceCommand(TestClient testClient)
		{
			Name = "balance";
			Description = "Shows the amount of L$.";
            Category = CommandCategory.Other;
		}

        public override string Execute(string[] args, UUID fromAgentID)
		{
            System.Threading.AutoResetEvent waitBalance = new System.Threading.AutoResetEvent(false);
            
            EventHandler<BalanceEventArgs> del = delegate(object sender, BalanceEventArgs e) { waitBalance.Set(); };
            Client.Self.MoneyBalance += del;            
            Client.Self.RequestBalance();
            String result = "Timeout waiting for balance reply";
            if (waitBalance.WaitOne(10000, false))
            {
                result = Client.ToString() + " has L$: " + Client.Self.Balance;
            }
            Client.Self.MoneyBalance -= del;
            return result;            
		}
    }
}
