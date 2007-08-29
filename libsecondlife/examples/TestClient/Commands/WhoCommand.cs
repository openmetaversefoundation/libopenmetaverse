using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class WhoCommand: Command
    {
        public WhoCommand(TestClient testClient)
		{
			Name = "who";
			Description = "Lists seen avatars.";
		}

        public override string Execute(string[] args, LLUUID fromAgentID)
		{
			StringBuilder result = new StringBuilder();

            lock (Client.Network.Simulators)
            {
                for (int i = 0; i < Client.Network.Simulators.Count; i++)
                {
                    Client.Network.Simulators[i].Objects.ForEach(
                        delegate(Avatar av)
                        {
                            result.AppendLine();
                            result.AppendFormat("{0} (Group: {1}, Location: {2}/{3}, UUID: {4})", av.Name,
                                av.GroupName, (av.CurrentSim != null ? av.CurrentSim.Name : String.Empty),
                                av.Position, av.ID.ToStringHyphenated());
                        }
                    );
                }
            }

            return result.ToString();
		}
    }
}
