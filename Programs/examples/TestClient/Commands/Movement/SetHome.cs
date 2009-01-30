using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class SetHomeCommand : Command
    {
		public SetHomeCommand(TestClient testClient)
        {
            Name = "sethome";
            Description = "Sets home to the current location.";
            Category = CommandCategory.Movement;
        }

        public override string Execute(string[] args, Guid fromAgentID)
        {
			Client.Self.SetHome();
            return "Home Set";
        }
    }
}
