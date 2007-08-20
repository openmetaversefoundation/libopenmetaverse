using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class SitOnCommand : Command
    {
        public SitOnCommand(TestClient testClient)
        {
            Name = "siton";
            Description = "Attempt to sit on a particular prim, with specified UUID";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            if (args.Length != 1)
                return "Usage: siton UUID";

            LLUUID target;

            if (LLUUID.TryParse(args[0], out target))
            {
                lock (Client.Network.CurrentSim.Objects.Prims)
                {
                    foreach (Primitive prim in Client.Network.CurrentSim.Objects.Prims.Values)
                    {
                        if (prim.ID == target)
                        {
                            Client.Self.RequestSit(prim.ID, LLVector3.Zero);
                            Client.Self.Sit();
                            return "Requested to sit on prim " + prim.ID.ToStringHyphenated() + 
                                " (" + prim.LocalID + ")";
                        }
                    }
                }
            }

            return "Couldn't find a prim to sit on with UUID " + args[0];
        }
    }
}