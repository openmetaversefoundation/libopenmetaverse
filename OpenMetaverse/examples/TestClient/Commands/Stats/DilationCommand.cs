using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class DilationCommand : Command
    {
		public DilationCommand(TestClient testClient)
        {
            Name = "dilation";
            Description = "Shows time dilation for current sim.";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            return "Dilation is " + Client.Network.CurrentSim.Stats.Dilation.ToString();
        }
    }
}