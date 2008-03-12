using System;
using System.Collections.Generic;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class BalanceCommand: Command
    {
        public BalanceCommand(TestClient testClient)
		{
			Name = "balance";
			Description = "Shows the amount of L$.";
		}

        public override string Execute(string[] args, LLUUID fromAgentID)
		{
            System.Threading.AutoResetEvent waitBalance = new System.Threading.AutoResetEvent(false);
            AgentManager.BalanceCallback del = delegate(int balance) { waitBalance.Set(); };
            Client.Self.OnBalanceUpdated += del;
            Client.Self.RequestBalance();
            String result = "Timeout waiting for balance reply";
            if (waitBalance.WaitOne(10000, false))
            {
                result = Client.ToString() + " has L$: " + Client.Self.Balance;
            }            
            Client.Self.OnBalanceUpdated -= del;
            return result;

		}
    }
}
