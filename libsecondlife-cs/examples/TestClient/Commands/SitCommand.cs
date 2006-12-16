using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class SitCommand: Command
    {
		public SitCommand()
		{
			Name = "sit";
			Description = "Attempt to sit on the closest prim";
		}
			
        public override string Execute(SecondLife Client, string[] args, LLUUID fromAgentID)
		{
		    PrimObject closest = null;
		    double closestDistance = Double.MaxValue;

		    lock (TestClient.SimPrims)
		    {
                if (TestClient.SimPrims.ContainsKey(Client.Network.CurrentSim))
                {
                    foreach (PrimObject p in TestClient.SimPrims[Client.Network.CurrentSim].Values)
                    {
                        float distance = Helpers.VecDist(Client.Self.Position, p.Position);

                        if (closest == null || distance < closestDistance)
                        {
                            closest = p;
                            closestDistance = distance;
                        }
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
