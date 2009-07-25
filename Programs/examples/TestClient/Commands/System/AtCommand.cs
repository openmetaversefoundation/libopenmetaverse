using System;
using System.Collections.Generic;
using System.Reflection;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class AtCommand : Command
    {
        public AtCommand(TestClient testClient)
        {
            Name = "@";
            Description = "Restrict the following commands to one or all avatars. Usage: @ [firstname lastname]";
            Category = CommandCategory.TestClient;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            // This is a dummy command. Calls to it should be intercepted and handled specially
            return "This command should not be executed directly";
        }
    }
}
