using System;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class GridLayerCommand : Command
    {
        public GridLayerCommand(TestClient testClient)
        {
            Name = "gridlayer";
            Description = "Downloads all of the layer chunks for the grid object map";
            Category = CommandCategory.Simulator;

            testClient.Grid.GridLayer += Grid_GridLayer;
        }

        void Grid_GridLayer(object sender, GridLayerEventArgs e)
        {
            Console.WriteLine(String.Format("Layer({0}) Bottom: {1} Left: {2} Top: {3} Right: {4}",
                   e.Layer.ImageID.ToString(), e.Layer.Bottom, e.Layer.Left, e.Layer.Top, e.Layer.Right));
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            Client.Grid.RequestMapLayer(GridLayerType.Objects);

            return "Sent.";
        }
    }
}
