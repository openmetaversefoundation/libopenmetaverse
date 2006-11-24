using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class UptimeCommand: Command
    {
		public DateTime Created = DateTime.Now;

		public UptimeCommand()
		{
			Name = "uptime";
			Description = "Shows the login time and length of time logged on.";
		}

		public override string Execute(string[] args, LLUUID fromAgentID)
		{
			return "Up Since: " + Created + " (" + (DateTime.Now - Created) + ")";
		}
    }
}
