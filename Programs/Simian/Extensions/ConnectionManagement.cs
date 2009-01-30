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

            server.UDP.RegisterPacketCallback(PacketType.UseCircuitCode, new PacketCallback(UseCircuitCodeHandler));
            server.UDP.RegisterPacketCallback(PacketType.StartPingCheck, new PacketCallback(StartPingCheckHandler));
            server.UDP.RegisterPacketCallback(PacketType.LogoutRequest, new PacketCallback(LogoutRequestHandler));
        }

        public void Stop()
        {
        }

        void UseCircuitCodeHandler(Packet packet, Agent agent)
        {
            RegionHandshakePacket handshake = new RegionHandshakePacket();
            handshake.RegionInfo.BillableFactor = 0f;
            handshake.RegionInfo.CacheID = Guid.NewGuid();
            handshake.RegionInfo.IsEstateManager = false;
            handshake.RegionInfo.RegionFlags = (uint)(RegionFlags.AllowDirectTeleport | RegionFlags.AllowLandmark |
                RegionFlags.AllowParcelChanges | RegionFlags.AllowSetHome | RegionFlags.AllowVoice | RegionFlags.PublicAllowed |
                RegionFlags.Sandbox | RegionFlags.TaxFree);
            handshake.RegionInfo.SimOwner = Guid.NewGuid();
            handshake.RegionInfo.SimAccess = (byte)SimAccess.Min;
            handshake.RegionInfo.SimName = Utils.StringToBytes("Simian");
            handshake.RegionInfo.WaterHeight = server.Scene.WaterHeight;
            handshake.RegionInfo.TerrainBase0 = Guid.Empty;
            handshake.RegionInfo.TerrainBase1 = Guid.Empty;
            handshake.RegionInfo.TerrainBase2 = Guid.Empty;
            handshake.RegionInfo.TerrainBase3 = Guid.Empty;
            handshake.RegionInfo.TerrainDetail0 = Guid.Empty;
            handshake.RegionInfo.TerrainDetail1 = Guid.Empty;
            handshake.RegionInfo.TerrainDetail2 = Guid.Empty;
            handshake.RegionInfo.TerrainDetail3 = Guid.Empty;
            handshake.RegionInfo.TerrainHeightRange00 = 0f;
            handshake.RegionInfo.TerrainHeightRange01 = 20f;
            handshake.RegionInfo.TerrainHeightRange10 = 0f;
            handshake.RegionInfo.TerrainHeightRange11 = 20f;
            handshake.RegionInfo.TerrainStartHeight00 = 0f;
            handshake.RegionInfo.TerrainStartHeight01 = 40f;
            handshake.RegionInfo.TerrainStartHeight10 = 0f;
            handshake.RegionInfo.TerrainStartHeight11 = 40f;
            handshake.RegionInfo2.RegionID = Guid.NewGuid();

            server.UDP.SendPacket(agent.AgentID, handshake, PacketCategory.Transaction);
        }

        void StartPingCheckHandler(Packet packet, Agent agent)
        {
            StartPingCheckPacket start = (StartPingCheckPacket)packet;

            CompletePingCheckPacket complete = new CompletePingCheckPacket();
            complete.Header.Reliable = false;
            complete.PingID.PingID = start.PingID.PingID;

            server.UDP.SendPacket(agent.AgentID, complete, PacketCategory.Overhead);
        }

        void LogoutRequestHandler(Packet packet, Agent agent)
        {
            LogoutRequestPacket request = (LogoutRequestPacket)packet;

            LogoutReplyPacket reply = new LogoutReplyPacket();
            reply.AgentData.AgentID = agent.AgentID;
            reply.AgentData.SessionID = agent.SessionID;
            reply.InventoryData = new LogoutReplyPacket.InventoryDataBlock[1];
            reply.InventoryData[0] = new LogoutReplyPacket.InventoryDataBlock();
            reply.InventoryData[0].ItemID = Guid.Empty;

            server.UDP.SendPacket(agent.AgentID, reply, PacketCategory.Transaction);

            server.DisconnectClient(agent);
        }
    }
}
