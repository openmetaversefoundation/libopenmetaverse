using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class QuitCommand: Command
    {
		public QuitCommand()
		{
			Name = "quit";
			Description = "Log off";
		}

		public override string Execute(string[] args, LLUUID fromAgentID)
		{
            Client.Network.Logout();
			TestClient.Running = false;
            return "Logging off.";
		}
    }
}
