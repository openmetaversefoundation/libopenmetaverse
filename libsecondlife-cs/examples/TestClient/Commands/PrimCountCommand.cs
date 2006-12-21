using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class PrimCountCommand: Command
    {
        SecondLife Client;

        public PrimCountCommand(TestClient testClient)
		{
            TestClient = testClient;
            Client = (SecondLife)TestClient;

			Name = "primCount";
			Description = "Shows the number of prims that have been received.";
		}

        public override string Execute(string[] args, LLUUID fromAgentID)
		{
            int count = 0;

            lock (TestClient.SimPrims)
            {
                foreach (Dictionary<uint, PrimObject> prims in TestClient.SimPrims.Values)
                {
                    count += prims.Count;
                }
            }

			return count.ToString();
		}
    }
}
