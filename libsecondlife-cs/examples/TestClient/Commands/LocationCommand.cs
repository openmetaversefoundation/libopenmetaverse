using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class LocationCommand: Command
    {
        SecondLife Client;

        public LocationCommand(TestClient testClient)
		{
            TestClient = testClient;
            Client = (SecondLife)TestClient;

			Name = "location";
			Description = "Show the location.";
		}

		public override string Execute(string[] args, LLUUID fromAgentID)
		{
            return "CurrentSim: '" + Client.Network.CurrentSim.Region.Name + "' Position: " + Client.Self.Position.ToString();
		}
    }
}
