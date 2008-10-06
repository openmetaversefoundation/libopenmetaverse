using System;
using System.Collections.Generic;
using System.Threading;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    class AvatarManager : IExtension, IAvatarProvider
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
            Server.UDP.RegisterPacketCallback(PacketType.AvatarPropertiesRequest, new PacketCallback(AvatarPropertiesRequestHandler));
            Server.UDP.RegisterPacketCallback(PacketType.AgentWearablesRequest, new PacketCallback(AgentWearablesRequestHandler));
            Server.UDP.RegisterPacketCallback(PacketType.AgentIsNowWearing, new PacketCallback(AgentIsNowWearingHandler));
            Server.UDP.RegisterPacketCallback(PacketType.AgentSetAppearance, new PacketCallback(AgentSetAppearanceHandler));
            Server.UDP.RegisterPacketCallback(PacketType.AgentCachedTexture, new PacketCallback(AgentCachedTextureHandler));
            Server.UDP.RegisterPacketCallback(PacketType.AgentAnimation, new PacketCallback(AgentAnimationHandler));
            Server.UDP.RegisterPacketCallback(PacketType.SoundTrigger, new PacketCallback(SoundTriggerHandler));
            Server.UDP.RegisterPacketCallback(PacketType.ViewerEffect, new PacketCallback(ViewerEffectHandler));
            Server.UDP.RegisterPacketCallback(PacketType.UUIDNameRequest, new PacketCallback(UUIDNameRequestHandler));
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

            Server.UDP.BroadcastPacket(sendAnim, PacketCategory.State);
        }

        public void TriggerSound(Agent agent, UUID soundID, float gain)
        {
            SoundTriggerPacket sound = new SoundTriggerPacket();
            sound.SoundData.Handle = Server.RegionHandle;
            sound.SoundData.ObjectID = agent.AgentID;
            sound.SoundData.ParentID = agent.AgentID;
            sound.SoundData.OwnerID = agent.AgentID;
            sound.SoundData.Position = agent.Avatar.Position;
            sound.SoundData.SoundID = soundID;
            sound.SoundData.Gain = gain;

            Server.UDP.BroadcastPacket(sound, PacketCategory.State);
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

            Server.UDP.BroadcastPacket(effect, PacketCategory.State);
        }

        void AvatarPropertiesRequestHandler(Packet packet, Agent agent)
        {
            AvatarPropertiesRequestPacket request = (AvatarPropertiesRequestPacket)packet;

            Agent foundAgent;
            if (Server.Agents.TryGetValue(request.AgentData.AvatarID, out foundAgent))
            {
                AvatarPropertiesReplyPacket reply = new AvatarPropertiesReplyPacket();
                reply.AgentData.AgentID = agent.AgentID;
                reply.AgentData.AvatarID = request.AgentData.AvatarID;
                reply.PropertiesData.AboutText = Utils.StringToBytes(foundAgent.ProfileAboutText);
                reply.PropertiesData.BornOn = Utils.StringToBytes(foundAgent.ProfileBornOn);
                reply.PropertiesData.CharterMember = new byte[1];
                reply.PropertiesData.FLAboutText = Utils.StringToBytes(foundAgent.ProfileFirstText);
                reply.PropertiesData.Flags = (uint)foundAgent.ProfileFlags;
                reply.PropertiesData.FLImageID = foundAgent.ProfileFirstImage;
                reply.PropertiesData.ImageID = foundAgent.ProfileImage;
                reply.PropertiesData.PartnerID = foundAgent.PartnerID;
                reply.PropertiesData.ProfileURL = Utils.StringToBytes(foundAgent.ProfileURL);

                Server.UDP.SendPacket(agent.AgentID, reply, PacketCategory.Transaction);
            }
            else
            {
                Logger.Log("AvatarPropertiesRequest for unknown agent " + request.AgentData.AvatarID.ToString(),
                    Helpers.LogLevel.Warning);
            }
        }

        int CountWearables(Agent agent)
        {
            int wearables = 0;

            if (agent.ShapeAsset != UUID.Zero) ++wearables;
            if (agent.SkinAsset != UUID.Zero) ++wearables;
            if (agent.HairAsset != UUID.Zero) ++wearables;
            if (agent.EyesAsset != UUID.Zero) ++wearables;
            if (agent.ShirtAsset != UUID.Zero) ++wearables;
            if (agent.PantsAsset != UUID.Zero) ++wearables;
            if (agent.ShoesAsset != UUID.Zero) ++wearables;
            if (agent.SocksAsset != UUID.Zero) ++wearables;
            if (agent.JacketAsset != UUID.Zero) ++wearables;
            if (agent.GlovesAsset != UUID.Zero) ++wearables;
            if (agent.UndershirtAsset != UUID.Zero) ++wearables;
            if (agent.UnderpantsAsset != UUID.Zero) ++wearables;
            if (agent.SkirtAsset != UUID.Zero) ++wearables;

            return wearables;
        }

        void SendWearables(Agent agent)
        {
            AgentWearablesUpdatePacket update = new AgentWearablesUpdatePacket();
            update.AgentData.AgentID = agent.AgentID;

            // Count the number of WearableData blocks needed
            int wearableCount = CountWearables(agent);
            int i = 0;

            update.WearableData = new AgentWearablesUpdatePacket.WearableDataBlock[wearableCount];

            #region WearableData Blocks

            if (agent.ShapeAsset != UUID.Zero)
            {
                update.WearableData[i] = new AgentWearablesUpdatePacket.WearableDataBlock();
                update.WearableData[i].AssetID = agent.ShapeAsset;
                update.WearableData[i].ItemID = agent.ShapeItem;
                update.WearableData[i].WearableType = (byte)WearableType.Shape;
                ++i;
            }
            if (agent.SkinAsset != UUID.Zero)
            {
                update.WearableData[i] = new AgentWearablesUpdatePacket.WearableDataBlock();
                update.WearableData[i].AssetID = agent.SkinAsset;
                update.WearableData[i].ItemID = agent.SkinItem;
                update.WearableData[i].WearableType = (byte)WearableType.Skin;
                ++i;
            }
            if (agent.HairAsset != UUID.Zero)
            {
                update.WearableData[i] = new AgentWearablesUpdatePacket.WearableDataBlock();
                update.WearableData[i].AssetID = agent.HairAsset;
                update.WearableData[i].ItemID = agent.HairItem;
                update.WearableData[i].WearableType = (byte)WearableType.Hair;
                ++i;
            }
            if (agent.EyesAsset != UUID.Zero)
            {
                update.WearableData[i] = new AgentWearablesUpdatePacket.WearableDataBlock();
                update.WearableData[i].AssetID = agent.EyesAsset;
                update.WearableData[i].ItemID = agent.EyesItem;
                update.WearableData[i].WearableType = (byte)WearableType.Eyes;
                ++i;
            }
            if (agent.ShirtAsset != UUID.Zero)
            {
                update.WearableData[i] = new AgentWearablesUpdatePacket.WearableDataBlock();
                update.WearableData[i].AssetID = agent.ShirtAsset;
                update.WearableData[i].ItemID = agent.ShirtItem;
                update.WearableData[i].WearableType = (byte)WearableType.Shirt;
                ++i;
            }
            if (agent.PantsAsset != UUID.Zero)
            {
                update.WearableData[i] = new AgentWearablesUpdatePacket.WearableDataBlock();
                update.WearableData[i].AssetID = agent.PantsAsset;
                update.WearableData[i].ItemID = agent.PantsItem;
                update.WearableData[i].WearableType = (byte)WearableType.Pants;
                ++i;
            }
            if (agent.ShoesAsset != UUID.Zero)
            {
                update.WearableData[i] = new AgentWearablesUpdatePacket.WearableDataBlock();
                update.WearableData[i].AssetID = agent.ShoesAsset;
                update.WearableData[i].ItemID = agent.ShoesItem;
                update.WearableData[i].WearableType = (byte)WearableType.Shoes;
                ++i;
            }
            if (agent.SocksAsset != UUID.Zero)
            {
                update.WearableData[i] = new AgentWearablesUpdatePacket.WearableDataBlock();
                update.WearableData[i].AssetID = agent.SocksAsset;
                update.WearableData[i].ItemID = agent.SocksItem;
                update.WearableData[i].WearableType = (byte)WearableType.Socks;
                ++i;
            }
            if (agent.JacketAsset != UUID.Zero)
            {
                update.WearableData[i] = new AgentWearablesUpdatePacket.WearableDataBlock();
                update.WearableData[i].AssetID = agent.JacketAsset;
                update.WearableData[i].ItemID = agent.JacketItem;
                update.WearableData[i].WearableType = (byte)WearableType.Jacket;
                ++i;
            }
            if (agent.GlovesAsset != UUID.Zero)
            {
                update.WearableData[i] = new AgentWearablesUpdatePacket.WearableDataBlock();
                update.WearableData[i].AssetID = agent.GlovesAsset;
                update.WearableData[i].ItemID = agent.GlovesItem;
                update.WearableData[i].WearableType = (byte)WearableType.Gloves;
                ++i;
            }
            if (agent.UndershirtAsset != UUID.Zero)
            {
                update.WearableData[i] = new AgentWearablesUpdatePacket.WearableDataBlock();
                update.WearableData[i].AssetID = agent.UndershirtAsset;
                update.WearableData[i].ItemID = agent.UndershirtItem;
                update.WearableData[i].WearableType = (byte)WearableType.Undershirt;
                ++i;
            }
            if (agent.UnderpantsAsset != UUID.Zero)
            {
                update.WearableData[i] = new AgentWearablesUpdatePacket.WearableDataBlock();
                update.WearableData[i].AssetID = agent.UnderpantsAsset;
                update.WearableData[i].ItemID = agent.UnderpantsItem;
                update.WearableData[i].WearableType = (byte)WearableType.Underpants;
                ++i;
            }
            if (agent.SkirtAsset != UUID.Zero)
            {
                update.WearableData[i] = new AgentWearablesUpdatePacket.WearableDataBlock();
                update.WearableData[i].AssetID = agent.SkirtAsset;
                update.WearableData[i].ItemID = agent.SkirtItem;
                update.WearableData[i].WearableType = (byte)WearableType.Skirt;
                ++i;
            }

            #endregion WearableData Blocks

            // Technically this should be per-agent, but if the only requirement is that it
            // increments this is easier
            update.AgentData.SerialNum = (uint)Interlocked.Increment(ref currentWearablesSerialNum);

            Logger.DebugLog(String.Format("Sending info about {0} wearables", wearableCount));

            Server.UDP.SendPacket(agent.AgentID, update, PacketCategory.Asset);
        }

        void AgentWearablesRequestHandler(Packet packet, Agent agent)
        {
            SendWearables(agent);
        }

        void ClearWearables(Agent agent)
        {
            agent.ShapeItem = UUID.Zero;
            agent.ShapeAsset = UUID.Zero;
            agent.SkinItem = UUID.Zero;
            agent.SkinAsset = UUID.Zero;
            agent.HairItem = UUID.Zero;
            agent.HairAsset = UUID.Zero;
            agent.EyesItem = UUID.Zero;
            agent.EyesAsset = UUID.Zero;
            agent.ShirtItem = UUID.Zero;
            agent.ShirtAsset = UUID.Zero;
            agent.PantsItem = UUID.Zero;
            agent.PantsAsset = UUID.Zero;
            agent.ShoesItem = UUID.Zero;
            agent.ShoesAsset = UUID.Zero;
            agent.SocksItem = UUID.Zero;
            agent.SocksAsset = UUID.Zero;
            agent.JacketItem = UUID.Zero;
            agent.JacketAsset = UUID.Zero;
            agent.GlovesItem = UUID.Zero;
            agent.GlovesAsset = UUID.Zero;
            agent.UndershirtItem = UUID.Zero;
            agent.UndershirtAsset = UUID.Zero;
            agent.UnderpantsItem = UUID.Zero;
            agent.UnderpantsAsset = UUID.Zero;
            agent.SkirtItem = UUID.Zero;
            agent.SkirtAsset = UUID.Zero;
        }

        void AgentIsNowWearingHandler(Packet packet, Agent agent)
        {
            AgentIsNowWearingPacket wearing = (AgentIsNowWearingPacket)packet;

            ClearWearables(agent);

            for (int i = 0; i < wearing.WearableData.Length; i++)
            {
                UUID assetID = UUID.Zero;
                UUID itemID = wearing.WearableData[i].ItemID;

                InventoryObject invObj;
                if (Server.Inventory.TryGetInventory(agent.AgentID, itemID, out invObj) && invObj is InventoryItem)
                    assetID = ((InventoryItem)invObj).AssetID;

                #region Update Wearables

                switch ((WearableType)wearing.WearableData[i].WearableType)
                {
                    case WearableType.Shape:
                        agent.ShapeAsset = assetID;
                        agent.ShapeItem = itemID;
                        break;
                    case WearableType.Skin:
                        agent.SkinAsset = assetID;
                        agent.SkinItem = itemID;
                        break;
                    case WearableType.Hair:
                        agent.HairAsset = assetID;
                        agent.HairItem = itemID;
                        break;
                    case WearableType.Eyes:
                        agent.EyesAsset = assetID;
                        agent.EyesItem = itemID;
                        break;
                    case WearableType.Shirt:
                        agent.ShirtAsset = assetID;
                        agent.ShirtItem = itemID;
                        break;
                    case WearableType.Pants:
                        agent.PantsAsset = assetID;
                        agent.PantsItem = itemID;
                        break;
                    case WearableType.Shoes:
                        agent.ShoesAsset = assetID;
                        agent.ShoesItem = itemID;
                        break;
                    case WearableType.Socks:
                        agent.SocksAsset = assetID;
                        agent.SocksItem = itemID;
                        break;
                    case WearableType.Jacket:
                        agent.JacketAsset = assetID;
                        agent.JacketItem = itemID;
                        break;
                    case WearableType.Gloves:
                        agent.GlovesAsset = assetID;
                        agent.GlovesItem = itemID;
                        break;
                    case WearableType.Undershirt:
                        agent.UndershirtAsset = assetID;
                        agent.UndershirtItem = itemID;
                        break;
                    case WearableType.Underpants:
                        agent.UnderpantsAsset = assetID;
                        agent.UnderpantsItem = itemID;
                        break;
                    case WearableType.Skirt:
                        agent.SkirtAsset = assetID;
                        agent.SkirtItem = itemID;
                        break;
                }

                #endregion Update Wearables
            }

            Logger.DebugLog("Updated agent wearables, new count: " + CountWearables(agent));
        }

        void AgentSetAppearanceHandler(Packet packet, Agent agent)
        {
            AgentSetAppearancePacket set = (AgentSetAppearancePacket)packet;

            Logger.DebugLog("Updating avatar appearance");

            agent.Avatar.Textures = new Primitive.TextureEntry(set.ObjectData.TextureEntry, 0,
                set.ObjectData.TextureEntry.Length);

            // Update agent visual params
            if (agent.VisualParams == null)
                agent.VisualParams = new byte[set.VisualParam.Length];

            for (int i = 0; i < set.VisualParam.Length; i++)
                agent.VisualParams[i] = set.VisualParam[i].ParamValue;

            //TODO: Store this for cached bake responses
            for (int i = 0; i < set.WearableData.Length; i++)
            {
                AppearanceManager.TextureIndex index = (AppearanceManager.TextureIndex)set.WearableData[i].TextureIndex;
                UUID cacheID = set.WearableData[i].CacheID;

                Logger.DebugLog(String.Format("WearableData: {0} is now {1}", index, cacheID));
            }

            ObjectUpdatePacket update = SimulationObject.BuildFullUpdate(agent.Avatar,
                Server.RegionHandle, agent.State, agent.Flags | PrimFlags.ObjectYouOwner);
            Server.UDP.SendPacket(agent.AgentID, update, PacketCategory.State);

            // Send out this appearance to all other connected avatars
            AvatarAppearancePacket appearance = BuildAppearancePacket(agent);
            lock (Server.Agents)
            {
                foreach (Agent recipient in Server.Agents.Values)
                {
                    if (recipient != agent)
                        Server.UDP.SendPacket(recipient.AgentID, appearance, PacketCategory.State);
                }
            }
        }

        void AgentCachedTextureHandler(Packet packet, Agent agent)
        {
            AgentCachedTexturePacket cached = (AgentCachedTexturePacket)packet;

            AgentCachedTextureResponsePacket response = new AgentCachedTextureResponsePacket();
            response.AgentData.AgentID = agent.AgentID;
            response.AgentData.SerialNum = cached.AgentData.SerialNum;

            response.WearableData = new AgentCachedTextureResponsePacket.WearableDataBlock[cached.WearableData.Length];

            // TODO: Respond back with actual cache entries if we have them
            for (int i = 0; i < cached.WearableData.Length; i++)
            {
                response.WearableData[i] = new AgentCachedTextureResponsePacket.WearableDataBlock();
                response.WearableData[i].TextureIndex = cached.WearableData[i].TextureIndex;
                response.WearableData[i].TextureID = UUID.Zero;
                response.WearableData[i].HostName = new byte[0];
            }

            response.Header.Zerocoded = true;

            Server.UDP.SendPacket(agent.AgentID, response, PacketCategory.Transaction);
        }

        void SoundTriggerHandler(Packet packet, Agent agent)
        {
            SoundTriggerPacket trigger = (SoundTriggerPacket)packet;
            TriggerSound(agent, trigger.SoundData.SoundID, trigger.SoundData.Gain);
        }

        void UUIDNameRequestHandler(Packet packet, Agent agent)
        {
            UUIDNameRequestPacket request = (UUIDNameRequestPacket)packet;

            UUIDNameReplyPacket reply = new UUIDNameReplyPacket();
            reply.UUIDNameBlock = new UUIDNameReplyPacket.UUIDNameBlockBlock[request.UUIDNameBlock.Length];

            for (int i = 0; i < request.UUIDNameBlock.Length; i++)
            {
                UUID id = request.UUIDNameBlock[i].ID;

                reply.UUIDNameBlock[i] = new UUIDNameReplyPacket.UUIDNameBlockBlock();
                reply.UUIDNameBlock[i].ID = id;

                Agent foundAgent;
                if (Server.Agents.TryGetValue(id, out foundAgent))
                {
                    reply.UUIDNameBlock[i].FirstName = Utils.StringToBytes(foundAgent.FirstName);
                    reply.UUIDNameBlock[i].LastName = Utils.StringToBytes(foundAgent.LastName);
                }
                else
                {
                    reply.UUIDNameBlock[i].FirstName = new byte[0];
                    reply.UUIDNameBlock[i].LastName = new byte[0];
                }
            }

            Server.UDP.SendPacket(agent.AgentID, reply, PacketCategory.Transaction);
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
