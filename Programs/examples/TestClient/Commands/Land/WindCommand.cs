using System;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class WindCommand : Command
    {
        public WindCommand(TestClient testClient)
        {
            Name = "wind";
            Description = "Displays current wind data";
            Category = CommandCategory.Simulator;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            // Get the agent's current "patch" position, where each patch of
            // wind data is a 16x16m square
            Vector3 agentPos = Client.Self.SimPosition;
            int xPos = (int)Utils.Clamp(agentPos.X, 0.0f, 255.0f) / 16;
            int yPos = (int)Utils.Clamp(agentPos.Y, 0.0f, 255.0f) / 16;

            Vector2 windSpeed = Client.Terrain.WindSpeeds[yPos * 16 + xPos];

            return "Local wind speed is " + windSpeed.ToString();
        }
    }
}
