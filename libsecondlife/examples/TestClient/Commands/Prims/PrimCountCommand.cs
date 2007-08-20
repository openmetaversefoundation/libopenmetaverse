using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class PrimCountCommand: Command
    {
        public PrimCountCommand(TestClient testClient)
		{
			Name = "primcount";
			Description = "Shows the number of objects currently being tracked.";
		}

        public override string Execute(string[] args, LLUUID fromAgentID)
		{
            int count = 0;

            lock (Client.Network.Simulators)
            {
                for (int i = 0; i < Client.Network.Simulators.Count; i++)
                {
                    Console.WriteLine("{0} (Avatars: {1} Primitives: {2})", 
                        Client.Network.Simulators[i].Name,
                        Client.Network.Simulators[i].Objects.Avatars.Count,
                        Client.Network.Simulators[i].Objects.Prims.Count);

                    count += Client.Network.Simulators[i].Objects.Avatars.Count;
                    count += Client.Network.Simulators[i].Objects.Prims.Count;
                }
            }

			return "Tracking a total of " + count + " objects";
		}
    }
}
