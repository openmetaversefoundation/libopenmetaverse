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
            Description = "Logs in another avatar. Usage: login firstname lastname password [simname] [loginuri]";
            Category = CommandCategory.TestClient;
        }

        public override string Execute(string[] args, Guid fromAgentID)
        {
            // This is a dummy command. Calls to it should be intercepted and handled specially
            return "This command should not be executed directly";
        }
    }
}
