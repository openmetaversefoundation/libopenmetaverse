using System;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian
{
    public class LLConnections : IExtension<ISceneProvider>
    {
        ISceneProvider scene;

        public LLConnections()
        {
        }

        public bool Start(ISceneProvider scene)
        {
            this.scene = scene;

            scene.UDP.RegisterPacketCallback(PacketType.UseCircuitCode, UseCircuitCodeHandler);
            scene.UDP.RegisterPacketCallback(PacketType.StartPingCheck, StartPingCheckHandler);
            scene.UDP.RegisterPacketCallback(PacketType.LogoutRequest, LogoutRequestHandler);
            scene.UDP.RegisterPacketCallback(PacketType.AgentThrottle, AgentThrottleHandler);
            scene.UDP.RegisterPacketCallback(PacketType.RegionHandshakeReply, RegionHandshakeReplyHandler);
            return true;
        }

        public void Stop()
        {
        }

        void UseCircuitCodeHandler(Packet packet, Agent agent)
        {
            RegionHandshakePacket handshake = new RegionHandshakePacket();
            handshake.RegionInfo.BillableFactor = 0f;
            handshake.RegionInfo.CacheID = UUID.Random();
            handshake.RegionInfo.IsEstateManager = false;
            handshake.RegionInfo.RegionFlags = (uint)scene.RegionFlags;
            handshake.RegionInfo.SimOwner = UUID.Random();
            handshake.RegionInfo.SimAccess = (byte)SimAccess.Min;
            handshake.RegionInfo.SimName = Utils.StringToBytes(scene.RegionName);
            handshake.RegionInfo.WaterHeight = scene.WaterHeight;
            handshake.RegionInfo.TerrainBase0 = UUID.Zero;
            handshake.RegionInfo.TerrainBase1 = UUID.Zero;
            handshake.RegionInfo.TerrainBase2 = UUID.Zero;
            handshake.RegionInfo.TerrainBase3 = UUID.Zero;
            handshake.RegionInfo.TerrainDetail0 = UUID.Zero;
            handshake.RegionInfo.TerrainDetail1 = UUID.Zero;
            handshake.RegionInfo.TerrainDetail2 = UUID.Zero;
            handshake.RegionInfo.TerrainDetail3 = UUID.Zero;
            handshake.RegionInfo.TerrainHeightRange00 = 0f;
            handshake.RegionInfo.TerrainHeightRange01 = 20f;
            handshake.RegionInfo.TerrainHeightRange10 = 0f;
            handshake.RegionInfo.TerrainHeightRange11 = 20f;
            handshake.RegionInfo.TerrainStartHeight00 = 0f;
            handshake.RegionInfo.TerrainStartHeight01 = 40f;
            handshake.RegionInfo.TerrainStartHeight10 = 0f;
            handshake.RegionInfo.TerrainStartHeight11 = 40f;
            handshake.RegionInfo2.RegionID = scene.RegionID;

            scene.UDP.SendPacket(agent.ID, handshake, PacketCategory.Transaction);
        }

        void StartPingCheckHandler(Packet packet, Agent agent)
        {
            StartPingCheckPacket start = (StartPingCheckPacket)packet;

            CompletePingCheckPacket complete = new CompletePingCheckPacket();
            complete.Header.Reliable = false;
            complete.PingID.PingID = start.PingID.PingID;

            scene.UDP.SendPacket(agent.ID, complete, PacketCategory.Overhead);
        }

        void LogoutRequestHandler(Packet packet, Agent agent)
        {
            LogoutReplyPacket reply = new LogoutReplyPacket();
            reply.AgentData.AgentID = agent.ID;
            reply.AgentData.SessionID = agent.SessionID;
            reply.InventoryData = new LogoutReplyPacket.InventoryDataBlock[1];
            reply.InventoryData[0] = new LogoutReplyPacket.InventoryDataBlock();
            reply.InventoryData[0].ItemID = UUID.Zero;

            scene.UDP.SendPacket(agent.ID, reply, PacketCategory.Transaction);

            scene.ObjectRemove(this, agent.ID);
        }

        void AgentThrottleHandler(Packet packet, Agent agent)
        {
            AgentThrottlePacket throttle = (AgentThrottlePacket)packet;

            // TODO: These need to be transmitted to neighbor sims before child agent connections can be established
            //throttle.Throttle.Throttles

            // Initiate the connection process for this agent to neighboring regions
            scene.InformClientOfNeighbors(agent);
        }

        void RegionHandshakeReplyHandler(Packet packet, Agent agent)
        {
            // Send updates and appearances for every avatar to this new avatar
            SynchronizeStateTo(agent);
        }

        // HACK: The reduction provider will deprecate this at some point
        void SynchronizeStateTo(Agent agent)
        {
            // Send the parcel overlay
            scene.Parcels.SendParcelOverlay(agent);

            // Send object updates for objects and avatars
            scene.ForEachObject(delegate(SimulationObject obj)
            {
                ObjectUpdatePacket update = new ObjectUpdatePacket();
                update.RegionData.RegionHandle = scene.RegionHandle;
                update.RegionData.TimeDilation = (ushort)(scene.TimeDilation * (float)UInt16.MaxValue);
                update.ObjectData = new ObjectUpdatePacket.ObjectDataBlock[1];
                update.ObjectData[0] = SimulationObject.BuildUpdateBlock(obj.Prim, obj.Prim.Flags, obj.CRC);

                scene.UDP.SendPacket(agent.ID, update, PacketCategory.State);
            });

            // Send appearances for all avatars
            scene.ForEachAgent(
                delegate(Agent otherAgent)
                {
                    if (otherAgent != agent)
                    {
                        // Send appearances for this avatar
                        AvatarAppearancePacket appearance = otherAgent.BuildAppearancePacket();
                        scene.UDP.SendPacket(agent.ID, appearance, PacketCategory.State);
                    }
                }
            );

            // Send terrain data
            SendLayerData(agent);
        }

        void SendLayerData(Agent agent)
        {
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    float[,] heightmap = scene.GetTerrainPatch((uint)x, (uint)y);
                    LayerDataPacket layer = TerrainCompressor.CreateLandPacket(heightmap, x, y);
                    scene.UDP.SendPacket(agent.ID, layer, PacketCategory.Terrain);
                }
            }
        }
    }
}
