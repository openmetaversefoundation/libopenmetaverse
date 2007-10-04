using System;
using System.Collections.Generic;
using libsecondlife;

namespace libsecondlife.TestClient
{
    public class AgentLocationsCommand : Command
    {
        public AgentLocationsCommand(TestClient testClient)
        {
            Name = "agentlocations";
            Description = "Downloads all of the agent locations in a specified region. Usage: agentlocations [regionhandle]";

            testClient.Grid.OnGridItems += new GridManager.GridItemsCallback(Grid_OnGridItems);
        }

        private void Grid_OnGridItems(GridItemType type, List<GridItem> items)
        {
            if (type == GridItemType.AgentLocations)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    GridAgentLocation location = (GridAgentLocation)items[i];

                    Console.WriteLine(String.Format("{0} avatars at {1},{2}", location.AvatarCount,
                        location.LocalX, location.LocalY));
                }
            }
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            ulong regionHandle;

            if (args.Length == 0)
                regionHandle = Client.Network.CurrentSim.Handle;
            else if (!(args.Length == 1 && UInt64.TryParse(args[0], out regionHandle)))
                return "Usage: agentlocations [regionhandle]";

            Client.Grid.RequestMapItems(regionHandle, GridItemType.AgentLocations, GridLayerType.Objects);

            return "Sent.";
        }
    }
}
