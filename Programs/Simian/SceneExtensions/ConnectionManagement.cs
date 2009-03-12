using System;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian
{
    public class ConnectionManagement : IExtension<ISceneProvider>
    {
        ISceneProvider scene;

        public ConnectionManagement()
        {
        }

        public bool Start(ISceneProvider scene)
        {
            this.scene = scene;

            scene.UDP.RegisterPacketCallback(PacketType.UseCircuitCode, UseCircuitCodeHandler);
            scene.UDP.RegisterPacketCallback(PacketType.StartPingCheck, StartPingCheckHandler);
            scene.UDP.RegisterPacketCallback(PacketType.LogoutRequest, LogoutRequestHandler);
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
    }
}
