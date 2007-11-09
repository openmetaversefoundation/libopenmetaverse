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

            Client.Network.CurrentSim.Objects.ForEach(
                delegate(Primitive prim)
                {
                    float distance = LLVector3.Dist(Client.Self.SimPosition, prim.Position);

                    if (closest == null || distance < closestDistance)
                    {
                        closest = prim;
                        closestDistance = distance;
                    }
                }
            );

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
