using System;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian
{
    public class ConnectionManagement : ISimianExtension
    {
        Simian server;

        public ConnectionManagement(Simian server)
        {
            this.server = server;
        }

        public void Start()
        {
            server.UDPServer.RegisterPacketCallback(PacketType.UseCircuitCode, new UDPServer.PacketCallback(UseCircuitCodeHandler));
            server.UDPServer.RegisterPacketCallback(PacketType.StartPingCheck, new UDPServer.PacketCallback(StartPingCheckHandler));
            server.UDPServer.RegisterPacketCallback(PacketType.LogoutRequest, new UDPServer.PacketCallback(LogoutRequestHandler));
            
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
            handshake.RegionInfo.RegionFlags = 1;
            handshake.RegionInfo.SimOwner = UUID.Random();
            handshake.RegionInfo.SimAccess = 1;
            handshake.RegionInfo.SimName = Utils.StringToBytes("Simian");
            handshake.RegionInfo.WaterHeight = 20.0f;
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

            agent.SendPacket(handshake);
        }

        void StartPingCheckHandler(Packet packet, Agent agent)
        {
            StartPingCheckPacket start = (StartPingCheckPacket)packet;

            CompletePingCheckPacket complete = new CompletePingCheckPacket();
            complete.Header.Reliable = false;
            complete.PingID.PingID = start.PingID.PingID;

            agent.SendPacket(complete);
        }

        void LogoutRequestHandler(Packet packet, Agent agent)
        {
            LogoutRequestPacket request = (LogoutRequestPacket)packet;

            LogoutReplyPacket reply = new LogoutReplyPacket();
            reply.AgentData.AgentID = agent.AgentID;
            reply.AgentData.SessionID = agent.SessionID;
            reply.InventoryData = new LogoutReplyPacket.InventoryDataBlock[1];
            reply.InventoryData[0] = new LogoutReplyPacket.InventoryDataBlock();
            reply.InventoryData[0].ItemID = UUID.Zero;

            lock (server.Agents)
            {
                if (server.Agents.ContainsKey(agent.Address))
                {
                    KillObjectPacket kill = new KillObjectPacket();
                    kill.ObjectData = new KillObjectPacket.ObjectDataBlock[1];
                    kill.ObjectData[0] = new KillObjectPacket.ObjectDataBlock();
                    kill.ObjectData[0].ID = agent.Avatar.LocalID;

                    server.Agents.Remove(agent.Address);

                    foreach (Agent recipient in server.Agents.Values)
                        recipient.SendPacket(kill);
                }
            }
        }

    }
}
