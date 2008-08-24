using System;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    class AvatarManager : ISimianExtension, IAvatarProvider
    {
        Simian Server;
        int currentWearablesSerialNum = -1;
        int currentAnimSequenceNum = 0;

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
            Server.UDPServer.RegisterPacketCallback(PacketType.AgentAnimation, new UDPServer.PacketCallback(AgentAnimationHandler));
            Server.UDPServer.RegisterPacketCallback(PacketType.ViewerEffect, new UDPServer.PacketCallback(ViewerEffectHandler));
        }

        public void Stop()
        {
        }

        public bool SetDefaultAnimation(Agent agent, UUID animID)
        {
            return agent.Animations.SetDefaultAnimation(animID, ref currentAnimSequenceNum);
        }

        public bool AddAnimation(Agent agent, UUID animID)
        {
            return agent.Animations.Add(animID, ref currentAnimSequenceNum);
        }

        public bool RemoveAnimation(Agent agent, UUID animID)
        {
            return agent.Animations.Remove(animID);
        }

        public void SendAnimations(Agent agent)
        {
            AvatarAnimationPacket sendAnim = new AvatarAnimationPacket();
            sendAnim.Sender.ID = agent.AgentID;
            sendAnim.AnimationSourceList = new AvatarAnimationPacket.AnimationSourceListBlock[1];
            sendAnim.AnimationSourceList[0] = new AvatarAnimationPacket.AnimationSourceListBlock();
            sendAnim.AnimationSourceList[0].ObjectID = agent.AgentID;

            UUID[] animIDS;
            int[] sequenceNums;
            agent.Animations.GetArrays(out animIDS, out sequenceNums);

            sendAnim.AnimationList = new AvatarAnimationPacket.AnimationListBlock[animIDS.Length];
            for (int i = 0; i < animIDS.Length; i++)
            {
                sendAnim.AnimationList[i] = new AvatarAnimationPacket.AnimationListBlock();
                sendAnim.AnimationList[i].AnimID = animIDS[i];
                sendAnim.AnimationList[i].AnimSequenceID = sequenceNums[i];
            }

            lock (Server.Agents)
            {
                foreach (Agent recipient in Server.Agents.Values)
                    recipient.SendPacket(sendAnim);
            }
        }

        void AgentAnimationHandler(Packet packet, Agent agent)
        {
            AgentAnimationPacket animPacket = (AgentAnimationPacket)packet;
            bool changed = false;

            for (int i = 0; i < animPacket.AnimationList.Length; i++)
            {
                AgentAnimationPacket.AnimationListBlock block = animPacket.AnimationList[i];

                if (block.StartAnim)
                {
                    if (agent.Animations.Add(block.AnimID, ref currentAnimSequenceNum))
                        changed = true;
                }
                else
                {
                    if (agent.Animations.Remove(block.AnimID))
                        changed = true;
                }
            }

            if (changed)
                SendAnimations(agent);
        }

        void ViewerEffectHandler(Packet packet, Agent agent)
        {
            ViewerEffectPacket effect = (ViewerEffectPacket)packet;

            effect.AgentData.AgentID = UUID.Zero;
            effect.AgentData.SessionID = UUID.Zero;

            // Broadcast this to everyone
            lock (Server.Agents)
            {
                foreach (Agent recipient in Server.Agents.Values)
                    recipient.SendPacket(effect);
            }
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
            update.WearableData = new AgentWearablesUpdatePacket.WearableDataBlock[5];

            // TODO: These are hardcoded in for now, should change that
            update.WearableData[0] = new AgentWearablesUpdatePacket.WearableDataBlock();
            update.WearableData[0].AssetID = new UUID("dc675529-7ba5-4976-b91d-dcb9e5e36188");
            update.WearableData[0].ItemID = UUID.Random();
            update.WearableData[0].WearableType = (byte)WearableType.Hair;

            update.WearableData[1] = new AgentWearablesUpdatePacket.WearableDataBlock();
            update.WearableData[1].AssetID = new UUID("3e8ee2d6-4f21-4a55-832d-77daa505edff");
            update.WearableData[1].ItemID = UUID.Random();
            update.WearableData[1].WearableType = (byte)WearableType.Pants;

            update.WearableData[2] = new AgentWearablesUpdatePacket.WearableDataBlock();
            update.WearableData[2].AssetID = new UUID("530a2614-052e-49a2-af0e-534bb3c05af0");
            update.WearableData[2].ItemID = UUID.Random();
            update.WearableData[2].WearableType = (byte)WearableType.Shape;

            update.WearableData[3] = new AgentWearablesUpdatePacket.WearableDataBlock();
            update.WearableData[3].AssetID = new UUID("6a714f37-fe53-4230-b46f-8db384465981");
            update.WearableData[3].ItemID = UUID.Random();
            update.WearableData[3].WearableType = (byte)WearableType.Shirt;

            update.WearableData[4] = new AgentWearablesUpdatePacket.WearableDataBlock();
            update.WearableData[4].AssetID = new UUID("5f787f25-f761-4a35-9764-6418ee4774c4");
            update.WearableData[4].ItemID = UUID.Random();
            update.WearableData[4].WearableType = (byte)WearableType.Skin;

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

            Logger.DebugLog("Updating avatar appearance");

            agent.Avatar.Textures = new Primitive.TextureEntry(set.ObjectData.TextureEntry, 0,
                set.ObjectData.TextureEntry.Length);

            // Update agent visual params
            for (int i = 0; i < set.VisualParam.Length; i++)
                agent.VisualParams[i] = set.VisualParam[i].ParamValue;

            //TODO: What is WearableData used for?

            ObjectUpdatePacket update = Movement.BuildFullUpdate(agent.Avatar,
                NameValue.NameValuesToString(agent.Avatar.NameValues), Server.RegionHandle,
                agent.State, agent.Flags | PrimFlags.ObjectYouOwner);
            agent.SendPacket(update);

            // Send out this appearance to all other connected avatars
            AvatarAppearancePacket appearance = BuildAppearancePacket(agent);

            lock (Server.Agents)
            {
                foreach (Agent recipient in Server.Agents.Values)
                {
                    if (recipient != agent)
                        recipient.SendPacket(appearance);
                }
            }
        }

        public static AvatarAppearancePacket BuildAppearancePacket(Agent agent)
        {
            AvatarAppearancePacket appearance = new AvatarAppearancePacket();
            appearance.ObjectData.TextureEntry = agent.Avatar.Textures.ToBytes();
            appearance.Sender.ID = agent.AgentID;
            appearance.Sender.IsTrial = false;

            appearance.VisualParam = new AvatarAppearancePacket.VisualParamBlock[218];
            for (int i = 0; i < 218; i++)
            {
                appearance.VisualParam[i] = new AvatarAppearancePacket.VisualParamBlock();
                appearance.VisualParam[i].ParamValue = agent.VisualParams[i];
            }

            return appearance;
        }
    }
}
