using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestTool
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
			TestTool.Running = false;
            Client.Network.Logout();
            return "Logging off.";
		}
    }
}
