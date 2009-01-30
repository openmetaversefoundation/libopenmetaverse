using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class SitOnCommand : Command
    {
        public SitOnCommand(TestClient testClient)
        {
            Name = "siton";
            Description = "Attempt to sit on a particular prim, with specified Guid";
            Category = CommandCategory.Movement;
        }

        public override string Execute(string[] args, Guid fromAgentID)
        {
            if (args.Length != 1)
                return "Usage: siton Guid";

            Guid target;

            if (GuidExtensions.TryParse(args[0], out target))
            {
                Primitive targetPrim = Client.Network.CurrentSim.ObjectsPrimitives.Find(
                    delegate(Primitive prim)
                    {
                        return prim.ID == target;
                    }
                );

                if (targetPrim != null)
                {
                    Client.Self.RequestSit(targetPrim.ID, Vector3.Zero);
                    Client.Self.Sit();
                    return "Requested to sit on prim " + targetPrim.ID.ToString() +
                        " (" + targetPrim.LocalID + ")";
                }
            }

            return "Couldn't find a prim to sit on with Guid " + args[0];
        }
    }
}
