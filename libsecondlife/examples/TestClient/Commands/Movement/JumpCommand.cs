using System;
using libsecondlife;

namespace libsecondlife.TestClient
{
    public class JumpCommand: Command
    {
        public JumpCommand(TestClient testClient)
		{
			Name = "jump";
			Description = "Jumps or flies up";
		}

        public override string Execute(string[] args, LLUUID fromAgentID)
		{
            Client.Self.Jump();
            return "Jumped";
		}
    }
}
