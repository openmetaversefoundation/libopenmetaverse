using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace TestBot
{
    public class UptimeCommand: Command
    {
		public DateTime Created = DateTime.Now;

		public UptimeCommand()
		{
			Name = "uptime";
			Description = "Shows the login time and length of time the bot has been logged in.";
		}

		public override string Execute(string[] args, LLUUID fromAgentID)
		{
			return "Up Since: " + Created + " (" + (DateTime.Now - Created) + ")";
		}
    }
}
