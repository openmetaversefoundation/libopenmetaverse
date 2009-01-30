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
            Category = CommandCategory.Simulator;
        }

        public override string Execute(string[] args, Guid fromAgentID)
        {
            return "Dilation is " + Client.Network.CurrentSim.Stats.Dilation.ToString();
        }
    }
}