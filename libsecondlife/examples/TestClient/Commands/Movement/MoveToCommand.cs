using System;
using System.Collections.Generic;
using System.Text;

namespace libsecondlife.TestClient.Commands.Movement
{
    class MovetoCommand : Command
    {
        public MovetoCommand(TestClient client)
        {
            Name = "moveto";
            Description = "Moves the avatar to the specified global position using simulator autopilot. Usage: moveto x y z";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            if (args.Length != 3)
                return "Usage: moveto x y z";

            uint regionX, regionY;
            Helpers.LongToUInts(Client.Network.CurrentSim.Handle, out regionX, out regionY);

            double x, y, z;
            Double.TryParse(args[0], out x);
            Double.TryParse(args[1], out y);
            Double.TryParse(args[2], out z);

            Client.Self.AutoPilot(x, y, z);

            return String.Format("Attempting to move to <{0},{1},{2}>", x, y, z);
        }
    }
}
