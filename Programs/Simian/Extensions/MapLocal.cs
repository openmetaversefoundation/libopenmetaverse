using System;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    public class MapLocal : IExtension<Simian>
    {
        Simian server;

        public MapLocal()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;

            server.UDP.RegisterPacketCallback(PacketType.MapLayerRequest, MapLayerRequestHandler);
            server.UDP.RegisterPacketCallback(PacketType.MapBlockRequest, MapBlockRequestHandler);
            server.UDP.RegisterPacketCallback(PacketType.TeleportRequest, TeleportRequestHandler);
            server.UDP.RegisterPacketCallback(PacketType.TeleportLocationRequest, TeleportLocationRequestHandler);
        }

        public void Stop()
        {
        }

        void MapLayerRequestHandler(Packet packet, Agent agent)
        {
            MapLayerRequestPacket request = (MapLayerRequestPacket)packet;
            GridLayerType type = (GridLayerType)request.AgentData.Flags;

            MapLayerReplyPacket reply = new MapLayerReplyPacket();
            reply.AgentData.AgentID = agent.Avatar.ID;
            reply.AgentData.Flags = (uint)type;
            reply.LayerData = new MapLayerReplyPacket.LayerDataBlock[1];
            reply.LayerData[0] = new MapLayerReplyPacket.LayerDataBlock();
            reply.LayerData[0].Bottom = 0;
            reply.LayerData[0].Left = 0;
            reply.LayerData[0].Top = UInt16.MaxValue;
            reply.LayerData[0].Right = UInt16.MaxValue;
            reply.LayerData[0].ImageID = new UUID("89556747-24cb-43ed-920b-47caed15465f");

            server.UDP.SendPacket(agent.Avatar.ID, reply, PacketCategory.Transaction);
        }

        void MapBlockRequestHandler(Packet packet, Agent agent)
        {
            MapBlockRequestPacket request = (MapBlockRequestPacket)packet;
            GridLayerType type = (GridLayerType)request.AgentData.Flags;

            MapBlockReplyPacket reply = new MapBlockReplyPacket();
            reply.AgentData.AgentID = agent.Avatar.ID;
            reply.AgentData.Flags = (uint)type;

            reply.Data = new MapBlockReplyPacket.DataBlock[2];

            reply.Data[0] = new MapBlockReplyPacket.DataBlock();
            reply.Data[0].Access = (byte)SimAccess.Min;
            reply.Data[0].Agents = (byte)server.Scene.AgentCount();
            reply.Data[0].MapImageID = new UUID("89556747-24cb-43ed-920b-47caed15465f");
            reply.Data[0].Name = Utils.StringToBytes(server.Scene.RegionName);
            reply.Data[0].RegionFlags = (uint)server.Scene.RegionFlags;
            reply.Data[0].WaterHeight = (byte)server.Scene.WaterHeight;
            reply.Data[0].X = (ushort)server.Scene.RegionX;
            reply.Data[0].Y = (ushort)server.Scene.RegionY;

            reply.Data[1] = new MapBlockReplyPacket.DataBlock();
            reply.Data[1].Access = (byte)SimAccess.Min;
            reply.Data[1].Agents = 0;
            reply.Data[1].MapImageID = new UUID("89556747-24cb-43ed-920b-47caed15465f");
            reply.Data[1].Name = Utils.StringToBytes("HyperGrid Portal to OSGrid");
            reply.Data[1].RegionFlags = (uint)server.Scene.RegionFlags;
            reply.Data[1].WaterHeight = (byte)server.Scene.WaterHeight;
            reply.Data[1].X = (ushort)(server.Scene.RegionX + 1);
            reply.Data[1].Y = (ushort)server.Scene.RegionY;

            server.UDP.SendPacket(agent.Avatar.ID, reply, PacketCategory.Transaction);
        }

        void TeleportRequestHandler(Packet packet, Agent agent)
        {
            TeleportRequestPacket request = (TeleportRequestPacket)packet;

            if (request.Info.RegionID == server.Scene.RegionID)
            {
                // Local teleport
                agent.Avatar.Position = request.Info.Position;
                agent.CurrentLookAt = request.Info.LookAt;

                TeleportLocalPacket reply = new TeleportLocalPacket();
                reply.Info.AgentID = agent.Avatar.ID;
                reply.Info.LocationID = 0; // Unused by the client
                reply.Info.LookAt = agent.CurrentLookAt;
                reply.Info.Position = agent.Avatar.Position;
                // TODO: Need a "Flying" boolean for Agent
                reply.Info.TeleportFlags = (uint)TeleportFlags.ViaRegionID;

                server.UDP.SendPacket(agent.Avatar.ID, reply, PacketCategory.Transaction);
            }
            else
            {
                Logger.Log("Ignoring teleport request to " + request.Info.RegionID, Helpers.LogLevel.Warning);
            }
        }

        void TeleportLocationRequestHandler(Packet packet, Agent agent)
        {
            TeleportLocationRequestPacket request = (TeleportLocationRequestPacket)packet;

            if (request.Info.RegionHandle == server.Scene.RegionHandle)
            {
                // Local teleport
                agent.Avatar.Position = request.Info.Position;
                agent.CurrentLookAt = request.Info.LookAt;

                TeleportLocalPacket reply = new TeleportLocalPacket();
                reply.Info.AgentID = agent.Avatar.ID;
                reply.Info.LocationID = 0; // Unused by the client
                reply.Info.LookAt = agent.CurrentLookAt;
                reply.Info.Position = agent.Avatar.Position;
                // TODO: Need a "Flying" boolean for Agent
                reply.Info.TeleportFlags = (uint)TeleportFlags.ViaLocation;

                server.UDP.SendPacket(agent.Avatar.ID, reply, PacketCategory.Transaction);
            }
            else
            {
                Logger.Log("Ignoring teleport request to " + request.Info.RegionHandle, Helpers.LogLevel.Warning);
            }
        }
    }
}
