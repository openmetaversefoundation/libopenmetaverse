using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class QuitCommand: Command
    {
        public QuitCommand(TestClient testClient)
		{
			Name = "quit";
			Description = "Log all avatars out and shut down";
            Category = CommandCategory.TestClient;
		}

        public override string Execute(string[] args, Guid fromAgentID)
		{
            // This is a dummy command. Calls to it should be intercepted and handled specially
            return "This command should not be executed directly";
		}
    }
}
