using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class StandCommand: Command
    {
        public StandCommand(TestClient testClient)
	{
		Name = "stand";
		Description = "Stand";
        Category = CommandCategory.Movement;
	}
	
        public override string Execute(string[] args, Guid fromAgentID)
	    {
            Client.Self.Stand();
		    return "Standing up.";  
	    }
    }
}
