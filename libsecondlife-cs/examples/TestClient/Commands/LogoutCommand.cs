using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class LogoutCommand : Command
    {
        public LogoutCommand()
        {
            Name = "logout";
            Description = "Log this avatar out";
        }

        public override string Execute(SecondLife Client, string[] args, LLUUID fromAgentID)
        {
            string name = Client.ToString();
			TestClient.ClientManager.Logout(TestClient);
            return "Logged " + name + " out";
        }
    }
}
