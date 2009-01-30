using System;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class GoHomeCommand : Command
    {
		public GoHomeCommand(TestClient testClient)
        {
            Name = "gohome";
            Description = "Teleports home";
            Category = CommandCategory.Movement;
        }

        public override string Execute(string[] args, Guid fromAgentID)
        {
			if ( Client.Self.GoHome() ) {
				return "Teleport Home Succesful";
			} else {
				return "Teleport Home Failed";
			}
        }
    }
}
