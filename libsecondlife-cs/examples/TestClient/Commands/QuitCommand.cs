using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class QuitCommand: Command
    {
		public QuitCommand()
		{
			Name = "quit";
			Description = "Log all avatars out and shut down";
		}

        public override string Execute(SecondLife Client, string[] args, LLUUID fromAgentID)
		{
			TestClient.ClientManager.LogoutAll();
            TestClient.ClientManager.Running = false;
            return "All avatars logged out";
		}
    }
}
