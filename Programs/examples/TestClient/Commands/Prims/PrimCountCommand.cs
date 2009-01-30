using System;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class PrimCountCommand: Command
    {
        public PrimCountCommand(TestClient testClient)
		{
			Name = "primcount";
			Description = "Shows the number of objects currently being tracked.";
            Category = CommandCategory.TestClient;
		}

        public override string Execute(string[] args, Guid fromAgentID)
		{
            int count = 0;

            lock (Client.Network.Simulators)
            {
                for (int i = 0; i < Client.Network.Simulators.Count; i++)
                {
                    int avcount = Client.Network.Simulators[i].ObjectsAvatars.Count;
                    int primcount = Client.Network.Simulators[i].ObjectsPrimitives.Count;

                    Console.WriteLine("{0} (Avatars: {1} Primitives: {2})", 
                        Client.Network.Simulators[i].Name, avcount, primcount);

                    count += avcount;
                    count += primcount;
                }
            }

			return "Tracking a total of " + count + " objects";
		}
    }
}
