using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestTool
{
    public class GotoCommand: Command
    {
		public GotoCommand()
		{
			Name = "goto";
			Description = "Goto location. (e.g. \"goto simname/100/100/30\")";
		}

		public override string Execute(string[] args, LLUUID fromAgentID)
		{
			if (args.Length < 1)
                return "Destination should be specified as: sim/x/y/z";

            char[] seps = { '/' };
            string[] destination = args[0].Split(seps);
            if( destination.Length != 4 )
                return "Destination should be specified as: sim/x/y/z";

            string sim = destination[0];
			float x = Client.Self.Position.X;
			float y = Client.Self.Position.Y;
			float z = Client.Self.Position.Z;
			float.TryParse(destination[1], out x);
			float.TryParse(destination[2], out y);
			float.TryParse(destination[3], out z);

			Client.Self.Teleport(sim, new LLVector3(x, y, z));

            return "Teleported to " + sim + " {" + x + "," + y + "," + z + "}";
		}
    }
}
