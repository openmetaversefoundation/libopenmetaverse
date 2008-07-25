using System;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class JumpCommand: Command
    {
        public JumpCommand(TestClient testClient)
		{
			Name = "jump";
			Description = "Jumps or flies up";
		}

        public override string Execute(string[] args, UUID fromAgentID)
		{
            Client.Self.Jump();
            return "Jumped";
		}
    }
}
