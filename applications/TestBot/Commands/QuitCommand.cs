using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace TestBot
{
    public class QuitCommand: Command
    {
		public QuitCommand()
		{
			Name = "quit";
			Description = "Bot will log off and bot program will exit.";
		}

		public override string Execute(string[] args, LLUUID fromAgentID)
		{
			Bot.Running = false;
            Client.Network.Logout();
            return "Logging off.";
		}
    }
}
