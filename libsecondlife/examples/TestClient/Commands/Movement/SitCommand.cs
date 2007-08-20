using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class SitCommand: Command
    {
        public SitCommand(TestClient testClient)
		{
			Name = "sit";
			Description = "Attempt to sit on the closest prim";
		}
			
        public override string Execute(string[] args, LLUUID fromAgentID)
		{
            Primitive closest = null;
		    double closestDistance = Double.MaxValue;

		    lock (Client.Network.CurrentSim.Objects.Prims)
		    {
                foreach (Primitive p in Client.Network.CurrentSim.Objects.Prims.Values)
                {
                    float distance = Helpers.VecDist(Client.Self.Position, p.Position);

                    if (closest == null || distance < closestDistance)
                    {
                        closest = p;
                        closestDistance = distance;
                    }
                }
		    }

            if (closest != null)
            {
                Client.Self.RequestSit(closest.ID, LLVector3.Zero);
                Client.Self.Sit();

                return "Sat on " + closest.ID + ". Distance: " + closestDistance;
            }
            else
            {
                return "Couldn't find a nearby prim to sit on";
            }
		}
    }
}
