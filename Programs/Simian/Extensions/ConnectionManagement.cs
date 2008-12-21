using System;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    public class ConnectionManagement : IExtension<Simian>
    {
        Simian server;

        public ConnectionManagement()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;

            server.UDP.RegisterPacketCallback(PacketType.UseCircuitCode, UseCircuitCodeHandler);
            server.UDP.RegisterPacketCallback(PacketType.StartPingCheck, StartPingCheckHandler);
            server.UDP.RegisterPacketCallback(PacketType.LogoutRequest, LogoutRequestHandler);
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
            handshake.RegionInfo.RegionFlags = (uint)(RegionFlags.AllowDirectTeleport | RegionFlags.AllowLandmark |
                RegionFlags.AllowParcelChanges | RegionFlags.AllowSetHome | RegionFlags.AllowVoice | RegionFlags.PublicAllowed |
                RegionFlags.Sandbox | RegionFlags.TaxFree);
            handshake.RegionInfo.SimOwner = UUID.Random();
            handshake.RegionInfo.SimAccess = (byte)SimAccess.Min;
            handshake.RegionInfo.SimName = Utils.StringToBytes("Simian");
            handshake.RegionInfo.WaterHeight = server.Scene.WaterHeight;
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
            handshake.RegionInfo2.RegionID = UUID.Random();

            server.UDP.SendPacket(agent.Avatar.ID, handshake, PacketCategory.Transaction);
        }

        void StartPingCheckHandler(Packet packet, Agent agent)
        {
            StartPingCheckPacket start = (StartPingCheckPacket)packet;

            CompletePingCheckPacket complete = new CompletePingCheckPacket();
            complete.Header.Reliable = false;
            complete.PingID.PingID = start.PingID.PingID;

            server.UDP.SendPacket(agent.Avatar.ID, complete, PacketCategory.Overhead);
        }

        void LogoutRequestHandler(Packet packet, Agent agent)
        {
            LogoutRequestPacket request = (LogoutRequestPacket)packet;

            LogoutReplyPacket reply = new LogoutReplyPacket();
            reply.AgentData.AgentID = agent.Avatar.ID;
            reply.AgentData.SessionID = agent.SessionID;
            reply.InventoryData = new LogoutReplyPacket.InventoryDataBlock[1];
            reply.InventoryData[0] = new LogoutReplyPacket.InventoryDataBlock();
            reply.InventoryData[0].ItemID = UUID.Zero;

            server.UDP.SendPacket(agent.Avatar.ID, reply, PacketCategory.Transaction);

            server.Avatars.Disconnect(agent);
        }
    }
}
