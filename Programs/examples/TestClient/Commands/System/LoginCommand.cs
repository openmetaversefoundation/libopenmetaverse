using System;
using System.Collections.Generic;
using System.Reflection;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class LoginCommand : Command
    {
        public LoginCommand(TestClient testClient)
        {
            Name = "login";
            Description = "Logs in another avatar";
            Category = CommandCategory.TestClient;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length != 3 && args.Length != 4)
                return "usage: login firstname lastname password [simname]";

            GridClient newClient = Client.ClientManager.Login(args);

            if (newClient.Network.Connected)
            {
                return "Logged in " + newClient.ToString();
            }
            else
            {
                return "Failed to login: " + newClient.Network.LoginMessage;
            }
        }
    }
}
