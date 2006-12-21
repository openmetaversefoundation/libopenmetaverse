using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class QuitCommand: Command
    {
        SecondLife Client;

        public QuitCommand(TestClient testClient)
		{
            TestClient = testClient;
            Client = (SecondLife)TestClient;

			Name = "quit";
			Description = "Log all avatars out and shut down";
		}

        public override string Execute(string[] args, LLUUID fromAgentID)
		{
			TestClient.ClientManager.LogoutAll();
            TestClient.ClientManager.Running = false;
            return "All avatars logged out";
		}
    }
}
