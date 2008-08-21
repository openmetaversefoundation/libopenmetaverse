using System;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    class AvatarManager : ISimianExtension
    {
        Simian Server;
        int currentWearablesSerialNum = -1;

        public AvatarManager(Simian server)
        {
            Server = server;
        }

        public void Start()
        {
            Server.UDPServer.RegisterPacketCallback(PacketType.AvatarPropertiesRequest, new UDPServer.PacketCallback(AvatarPropertiesRequestHandler));
            Server.UDPServer.RegisterPacketCallback(PacketType.AgentWearablesRequest, new UDPServer.PacketCallback(AgentWearablesRequestHandler));
            Server.UDPServer.RegisterPacketCallback(PacketType.AgentIsNowWearing, new UDPServer.PacketCallback(AgentIsNowWearingHandler));
            Server.UDPServer.RegisterPacketCallback(PacketType.AgentSetAppearance, new UDPServer.PacketCallback(AgentSetAppearanceHandler));
        }

        public void Stop()
        {
        }

        void AvatarPropertiesRequestHandler(Packet packet, Agent agent)
        {
            AvatarPropertiesRequestPacket request = (AvatarPropertiesRequestPacket)packet;

            lock (Server.Agents)
            {
                foreach (Agent agt in Server.Agents.Values)
                {
                    if (agent.AgentID == request.AgentData.AvatarID)
                    {
                        AvatarPropertiesReplyPacket reply = new AvatarPropertiesReplyPacket();
                        reply.AgentData.AgentID = agt.AgentID;
                        reply.AgentData.AvatarID = request.AgentData.AvatarID;
                        reply.PropertiesData.AboutText = Utils.StringToBytes("Profile info unavailable");
                        reply.PropertiesData.BornOn = Utils.StringToBytes("Unknown");
                        reply.PropertiesData.CharterMember = Utils.StringToBytes("Test User");
                        reply.PropertiesData.FLAboutText = Utils.StringToBytes("First life info unavailable");
                        reply.PropertiesData.Flags = 0;
                        //TODO: at least generate static image uuids based on name.
                        //this will prevent re-caching the default image for the same av name.
                        reply.PropertiesData.FLImageID = agent.AgentID; //temporary hack
                        reply.PropertiesData.ImageID = agent.AgentID; //temporary hack
                        reply.PropertiesData.PartnerID = UUID.Zero;
                        reply.PropertiesData.ProfileURL = Utils.StringToBytes(String.Empty);

                        agent.SendPacket(reply);

                        break;
                    }
                }
            }
        }

        void AgentWearablesRequestHandler(Packet packet, Agent agent)
        {
            AgentWearablesUpdatePacket update = new AgentWearablesUpdatePacket();
            update.AgentData.AgentID = agent.AgentID;
            // Technically this should be per-agent, but if the only requirement is that it
            // increments this is easier
            update.AgentData.SerialNum = (uint)Interlocked.Increment(ref currentWearablesSerialNum);
            update.WearableData = new AgentWearablesUpdatePacket.WearableDataBlock[13];
            for (int i = 0; i < 13; i++)
            {
                update.WearableData[i] = new AgentWearablesUpdatePacket.WearableDataBlock();
                update.WearableData[i].AssetID = UUID.Random();
                update.WearableData[i].ItemID = UUID.Random();
                update.WearableData[i].WearableType = (byte)i;
            }

            //HACK
            //update.WearableData = new AgentWearablesUpdatePacket.WearableDataBlock[0];

            agent.SendPacket(update);
        }

        void AgentIsNowWearingHandler(Packet packet, Agent agent)
        {
            AgentIsNowWearingPacket wearing = (AgentIsNowWearingPacket)packet;

            Logger.DebugLog("Updating agent wearables");

            lock (agent.Wearables)
            {
                agent.Wearables.Clear();

                for (int i = 0; i < wearing.WearableData.Length; i++)
                    agent.Wearables[(WearableType)wearing.WearableData[i].WearableType] = wearing.WearableData[i].ItemID;
            }
        }

        void AgentSetAppearanceHandler(Packet packet, Agent agent)
        {
            AgentSetAppearancePacket set = (AgentSetAppearancePacket)packet;

            agent.Avatar.Textures = new LLObject.TextureEntry(set.ObjectData.TextureEntry, 0,
                set.ObjectData.TextureEntry.Length);

            Logger.DebugLog("Updating avatar appearance");

            AvatarAppearancePacket appearance = new AvatarAppearancePacket();
            appearance.ObjectData.TextureEntry = set.ObjectData.TextureEntry;
            appearance.Sender.ID = agent.AgentID;
            appearance.Sender.IsTrial = false;

            // TODO: Store these visual params in Agent
            appearance.VisualParam = new AvatarAppearancePacket.VisualParamBlock[set.VisualParam.Length];
            for (int i = 0; i < set.VisualParam.Length; i++)
            {
                appearance.VisualParam[i] = new AvatarAppearancePacket.VisualParamBlock();
                appearance.VisualParam[i].ParamValue = set.VisualParam[i].ParamValue;
            }

            //TODO: What is WearableData used for?

            ObjectUpdatePacket update = Movement.BuildFullUpdate(agent, agent.Avatar, Server.RegionHandle,
                agent.State, agent.Flags | LLObject.ObjectFlags.ObjectYouOwner);
            agent.SendPacket(update);

            lock (Server.Agents)
            {
                foreach (Agent recipient in Server.Agents.Values)
                {
                    if (recipient != agent)
                        recipient.SendPacket(appearance);
                }
            }
        }
    }
}
