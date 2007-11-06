using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class JumpCommand: Command
    {
        public JumpCommand(TestClient testClient)
		{
			Name = "jump";
			Description = "Teleports to the specified height. (e.g. \"jump 10\")";
		}

        public override string Execute(string[] args, LLUUID fromAgentID)
		{
			if (args.Length != 1)
                return "Usage: jump 10";

			float height = 0;
            if (!float.TryParse(args[0], out height))
                return "Usage: jump 10";

            LLVector3 dest = Client.Self.SimPosition;
            dest.Z += height;

			Client.Self.Teleport(Client.Network.CurrentSim.Name, dest);

            return "Attempted to jump " + height + " meters";
		}
    }
}
