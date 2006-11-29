using System;
using System.Collections.Generic;
using System.Reflection;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class LoginCommand : Command
    {
        public LoginCommand()
        {
            Name = "login";
            Description = "Logs in another avatar";
        }

        public override string Execute(SecondLife Client, string[] args, LLUUID fromAgentID)
        {
            if (args.Length != 3)
                return "usage: login firstname lastname password";

            LoginDetails account = new LoginDetails();
            account.FirstName = args[0];
            account.LastName = args[1];
            account.Password = args[2];

            // Check if this client is already logged in
            foreach (SecondLife client in TestClient.Clients.Values)
            {
                if (client.Self.FirstName == account.FirstName && client.Self.LastName == account.LastName)
                {
                    TestClient.Clients.Remove(client.Network.AgentID);

                    client.Network.Logout();

                    break;
                }
            }

            SecondLife newClient = TestClient.InitializeClient(account);

            if (newClient.Network.Connected)
            {
                TestClient.Clients[newClient.Network.AgentID] = newClient;

                return "Logged in " + newClient.ToString();
            }
            else
            {
                return "Failed to login " + account.FirstName + " " + account.LastName + ": " +
                    newClient.Network.LoginError;
            }
        }
    }
}
