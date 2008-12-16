using System;
using System.Collections.Generic;
using System.Threading;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    public class Periscope : IExtension<Simian>
    {
        const string FIRST_NAME = "Testing";
        const string LAST_NAME = "Anvil";
        const string PASSWORD = "testinganvil";

        Simian server;
        GridClient client;

        public Periscope()
        {
            client = new GridClient();
            client.Settings.SEND_AGENT_UPDATES = false;

            client.Objects.OnNewPrim += new OpenMetaverse.ObjectManager.NewPrimCallback(Objects_OnNewPrim);
            client.Terrain.OnLandPatch += new TerrainManager.LandPatchCallback(Terrain_OnLandPatch);
        }

        public void Start(Simian server)
        {
            this.server = server;
            server.UDP.RegisterPacketCallback(PacketType.AgentUpdate, AgentUpdateHandler);

            // Start the login process
            Thread loginThread = new Thread(new ThreadStart(
                delegate()
                {
                    client.Network.Login(FIRST_NAME, LAST_NAME, PASSWORD, "Simian Periscope", "1.0.0");

                    if (client.Network.Connected)
                    {
                        Logger.Log("Periscope is connected: " + client.Network.LoginMessage, Helpers.LogLevel.Info);
                    }
                    else
                    {
                        Logger.Log("Periscope failed to connect to the foreign grid: " + client.Network.LoginErrorKey, Helpers.LogLevel.Error);
                    }
                }
            ));
            loginThread.Start();
        }

        public void Stop()
        {
            if (client.Network.Connected)
                client.Network.Logout();
        }

        void Objects_OnNewPrim(Simulator simulator, Primitive prim, ulong regionHandle, ushort timeDilation)
        {
            SimulationObject simObj = new SimulationObject(prim, server);
            server.Scene.ObjectAdd(this, simObj, prim.Flags);
        }

        void Terrain_OnLandPatch(Simulator simulator, int x, int y, int width, float[] data)
        {
            //throw new NotImplementedException();
        }

        void AgentUpdateHandler(Packet packet, Agent agent)
        {
            AgentUpdatePacket update = (AgentUpdatePacket)packet;

            // Forward AgentUpdate packets with the AgentID/SessionID set to the bots ID
            update.AgentData.AgentID = client.Self.AgentID;
            update.AgentData.SessionID = client.Self.SessionID;
            client.Network.SendPacket(update);
        }
    }
}
