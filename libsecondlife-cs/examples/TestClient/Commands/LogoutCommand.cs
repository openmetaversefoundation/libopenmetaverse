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
            TestClient.Clients.Remove(Client.Network.AgentID);
            Client.Network.Logout();
            Client = null;

            if (TestClient.Clients.Count > 0)
            {
                return "Logged " + name + " out";
            }
            else
            {
                TestClient.Running = false;
                return "All avatars logged out";
            }
        }
    }
}
