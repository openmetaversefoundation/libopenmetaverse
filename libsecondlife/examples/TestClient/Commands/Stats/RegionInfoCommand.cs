using System;
using System.Text;
using libsecondlife;

namespace libsecondlife.TestClient
{
    public class RegionInfoCommand : Command
    {
        public RegionInfoCommand(TestClient testClient)
		{
			Name = "regioninfo";
			Description = "Prints out info about all the current region";
		}

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine(Client.Network.CurrentSim.ToString());
            output.Append("UUID: ");
            output.AppendLine(Client.Network.CurrentSim.ID.ToString());
            uint x, y;
            Helpers.LongToUInts(Client.Network.CurrentSim.Handle, out x, out y);
            output.AppendLine(String.Format("Handle: {0} (X: {1} Y: {2})", Client.Network.CurrentSim.Handle, x, y));
            output.Append("Access: ");
            output.AppendLine(Client.Network.CurrentSim.Access.ToString());
            output.Append("Flags: ");
            output.AppendLine(Client.Network.CurrentSim.Flags.ToString());
            output.Append("TerrainBase0: ");
            output.AppendLine(Client.Network.CurrentSim.TerrainBase0.ToString());
            output.Append("TerrainBase1: ");
            output.AppendLine(Client.Network.CurrentSim.TerrainBase1.ToString());
            output.Append("TerrainBase2: ");
            output.AppendLine(Client.Network.CurrentSim.TerrainBase2.ToString());
            output.Append("TerrainBase3: ");
            output.AppendLine(Client.Network.CurrentSim.TerrainBase3.ToString());
            output.Append("TerrainDetail0: ");
            output.AppendLine(Client.Network.CurrentSim.TerrainDetail0.ToString());
            output.Append("TerrainDetail1: ");
            output.AppendLine(Client.Network.CurrentSim.TerrainDetail1.ToString());
            output.Append("TerrainDetail2: ");
            output.AppendLine(Client.Network.CurrentSim.TerrainDetail2.ToString());
            output.Append("TerrainDetail3: ");
            output.AppendLine(Client.Network.CurrentSim.TerrainDetail3.ToString());
            output.Append("Water Height: ");
            output.AppendLine(Client.Network.CurrentSim.WaterHeight.ToString());

            return output.ToString();
        }
    }
}
