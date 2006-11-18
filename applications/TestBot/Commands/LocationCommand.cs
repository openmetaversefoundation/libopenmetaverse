using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace TestBot
{
    public class LocationCommand: Command
    {
		public LocationCommand()
		{
			Name = "location";
			Description = "Show the bots location.";
		}

		public override string Execute(string[] args, LLUUID fromAgentID)
		{
            return "CurrentSim: '" + Client.Network.CurrentSim.Region.Name + "' Position: " + Client.Self.Position.ToString();
		}
    }
}
