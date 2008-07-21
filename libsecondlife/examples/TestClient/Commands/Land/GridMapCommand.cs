using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class GridMapCommand : Command
    {
        public GridMapCommand(TestClient testClient)
        {
            Name = "gridmap";
            Description = "Downloads all visible information about the grid map";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            //if (args.Length < 1)
            //    return "";

            Client.Grid.RequestMainlandSims(GridLayerType.Objects);
            
            return "Sent.";
        }
    }
}
