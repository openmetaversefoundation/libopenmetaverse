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

        public override string Execute(string[] args, UUID fromAgentID)
		{
			Client.ClientManager.LogoutAll();
            Client.ClientManager.Running = false;
            return "All avatars logged out";
		}
    }
}
