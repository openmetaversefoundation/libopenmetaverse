using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class LocationCommand: Command
    {
		public LocationCommand()
		{
			Name = "location";
			Description = "Show the location.";
		}

		public override string Execute(SecondLife Client, string[] args, LLUUID fromAgentID)
		{
            return "CurrentSim: '" + Client.Network.CurrentSim.Region.Name + "' Position: " + Client.Self.Position.ToString();
		}
    }
}
