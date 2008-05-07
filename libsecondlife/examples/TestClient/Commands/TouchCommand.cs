using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class TouchCommand: Command
    {
        public TouchCommand(TestClient testClient)
		{
			Name = "touch";
			Description = "Attempt to touch a prim with specified UUID";
		}
		
        public override string Execute(string[] args, LLUUID fromAgentID)
		{
            LLUUID target;

            if (args.Length != 1)
                return "Usage: touch UUID";
            
            if (LLUUID.TryParse(args[0], out target))
            {
                Primitive targetPrim = Client.Network.CurrentSim.ObjectsPrimitives.Find(
                    delegate(Primitive prim)
                    {
                        return prim.ID == target;
                    }
                );

                if (targetPrim != null)
                {
                    Client.Self.Touch(targetPrim.LocalID);
                    return "Touched prim " + targetPrim.LocalID;
                }
            }

            return "Couldn't find a prim to touch with UUID " + args[0];
		}
    }
}
