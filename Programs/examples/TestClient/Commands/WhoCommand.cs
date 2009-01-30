using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class WhoCommand: Command
    {
        public WhoCommand(TestClient testClient)
		{
			Name = "who";
			Description = "Lists seen avatars.";
            Category = CommandCategory.Other;
		}

        public override string Execute(string[] args, Guid fromAgentID)
		{
			StringBuilder result = new StringBuilder();

            lock (Client.Network.Simulators)
            {
                for (int i = 0; i < Client.Network.Simulators.Count; i++)
                {
                    Client.Network.Simulators[i].ObjectsAvatars.ForEach(
                        delegate(Avatar av)
                        {
                            result.AppendLine();
                            result.AppendFormat("{0} (Group: {1}, Location: {2}, Guid: {3})",
                                av.Name, av.GroupName, av.Position, av.ID.ToString());
                        }
                    );
                }
            }

            return result.ToString();
		}
    }
}
