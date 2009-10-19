using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    /// <summary>
    /// Display a list of all agent locations in a specified region
    /// </summary>
    public class AgentLocationsCommand : Command
    {
        public AgentLocationsCommand(TestClient testClient)
        {
            Name = "agentlocations";
            Description = "Downloads all of the agent locations in a specified region. Usage: agentlocations [regionhandle]";
            Category = CommandCategory.Simulator;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            ulong regionHandle;

            if (args.Length == 0)
                regionHandle = Client.Network.CurrentSim.Handle;
            else if (!(args.Length == 1 && UInt64.TryParse(args[0], out regionHandle)))
                return "Usage: agentlocations [regionhandle]";

            List<MapItem> items = Client.Grid.MapItems(regionHandle, GridItemType.AgentLocations, 
                GridLayerType.Objects, 1000 * 20);

            if (items != null)
            {
                StringBuilder ret = new StringBuilder();
                ret.AppendLine("Agent locations:");

                for (int i = 0; i < items.Count; i++)
                {
                    MapAgentLocation location = (MapAgentLocation)items[i];

                    ret.AppendLine(String.Format("{0} avatar(s) at {1},{2}", location.AvatarCount, location.LocalX,
                        location.LocalY));
                }

                return ret.ToString();
            }
            else
            {
                return "Failed to fetch agent locations";
            }
        }
    }
}
