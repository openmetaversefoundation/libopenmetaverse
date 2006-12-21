using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class LogoutCommand : Command
    {
        SecondLife Client;

        public LogoutCommand(TestClient testClient)
        {
            TestClient = testClient;
            Client = (SecondLife)TestClient;

            Name = "logout";
            Description = "Log this avatar out";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            string name = Client.ToString();
			TestClient.ClientManager.Logout(TestClient);
            return "Logged " + name + " out";
        }
    }
}
