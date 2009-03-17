using System;
using System.Collections.Generic;
using System.Threading;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian
{
    public class LLAgents : IExtension<ISceneProvider>
    {
        ISceneProvider scene;
        int currentWearablesSerialNum = -1;
        int currentAnimSequenceNum = 0;
        Timer coarseLocationTimer;

        public LLAgents()
        {
        }

        public bool Start(ISceneProvider scene)
        {
            this.scene = scene;

            scene.UDP.RegisterPacketCallback(PacketType.AvatarPropertiesRequest, AvatarPropertiesRequestHandler);
            scene.UDP.RegisterPacketCallback(PacketType.AgentWearablesRequest, AgentWearablesRequestHandler);
            scene.UDP.RegisterPacketCallback(PacketType.AgentIsNowWearing, AgentIsNowWearingHandler);
            scene.UDP.RegisterPacketCallback(PacketType.AgentSetAppearance, AgentSetAppearanceHandler);
            scene.UDP.RegisterPacketCallback(PacketType.AgentCachedTexture, AgentCachedTextureHandler);
            scene.UDP.RegisterPacketCallback(PacketType.AgentHeightWidth, AgentHeightWidthHandler);
            scene.UDP.RegisterPacketCallback(PacketType.AgentAnimation, AgentAnimationHandler);
            scene.UDP.RegisterPacketCallback(PacketType.SoundTrigger, SoundTriggerHandler);
            scene.UDP.RegisterPacketCallback(PacketType.ViewerEffect, ViewerEffectHandler);
            scene.UDP.RegisterPacketCallback(PacketType.UUIDNameRequest, UUIDNameRequestHandler);

            if (coarseLocationTimer != null) coarseLocationTimer.Dispose();
            coarseLocationTimer = new Timer(coarseLocationTimer_Elapsed);
            coarseLocationTimer.Change(1000, 1000);
            return true;
        }

        public void Stop()
        {
            if (coarseLocationTimer != null)
            {
                coarseLocationTimer.Dispose();
                coarseLocationTimer = null;
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
                scene.Avatars.SendAnimations(agent);
        }

        void ViewerEffectHandler(Packet packet, Agent agent)
        {
            ViewerEffectPacket incomingEffects = (ViewerEffectPacket)packet;

            ViewerEffect[] outgoingEffects = new ViewerEffect[incomingEffects.Effect.Length];

            for (int i = 0; i < outgoingEffects.Length; i++)
            {
                ViewerEffectPacket.EffectBlock block = incomingEffects.Effect[i];
                outgoingEffects[i] = new ViewerEffect(block.ID, (EffectType)block.Type, block.AgentID,
                    new Color4(block.Color, 0, true), block.Duration, block.TypeData);
            }

            scene.TriggerEffects(this, outgoingEffects);
        }

        void AvatarPropertiesRequestHandler(Packet packet, Agent agent)
        {
            AvatarPropertiesRequestPacket request = (AvatarPropertiesRequestPacket)packet;

            Agent foundAgent;
            if (scene.TryGetAgent(request.AgentData.AvatarID, out foundAgent))
            {
                AvatarPropertiesReplyPacket reply = new AvatarPropertiesReplyPacket();
                reply.AgentData.AgentID = agent.ID;
                reply.AgentData.AvatarID = request.AgentData.AvatarID;
                reply.PropertiesData.AboutText = Utils.StringToBytes(foundAgent.Info.ProfileAboutText);
                reply.PropertiesData.BornOn = Utils.StringToBytes(foundAgent.Info.ProfileBornOn);
                reply.PropertiesData.CharterMember = new byte[1];
                reply.PropertiesData.FLAboutText = Utils.StringToBytes(foundAgent.Info.ProfileFirstText);
                reply.PropertiesData.Flags = (uint)foundAgent.Info.ProfileFlags;
                reply.PropertiesData.FLImageID = foundAgent.Info.ProfileFirstImage;
                reply.PropertiesData.ImageID = foundAgent.Info.ProfileImage;
                reply.PropertiesData.PartnerID = foundAgent.Info.PartnerID;
                reply.PropertiesData.ProfileURL = Utils.StringToBytes(foundAgent.Info.ProfileURL);

                scene.UDP.SendPacket(agent.ID, reply, PacketCategory.Transaction);
            }
            else
            {
                Logger.Log("AvatarPropertiesRequest for unknown agent " + request.AgentData.AvatarID.ToString(),
                    Helpers.LogLevel.Warning);
            }
        }

        void AgentWearablesRequestHandler(Packet packet, Agent agent)
        {
            SendWearables(agent);
        }

        void AgentIsNowWearingHandler(Packet packet, Agent agent)
        {
            AgentIsNowWearingPacket wearing = (AgentIsNowWearingPacket)packet;

            for (int i = 0; i < wearing.WearableData.Length; i++)
            {
                UUID itemID = wearing.WearableData[i].ItemID;

                #region Update Wearables

                switch ((WearableType)wearing.WearableData[i].WearableType)
                {
                    case WearableType.Shape:
                        agent.Info.ShapeItem = itemID;
                        break;
                    case WearableType.Skin:
                        agent.Info.SkinItem = itemID;
                        break;
                    case WearableType.Hair:
                        agent.Info.HairItem = itemID;
                        break;
                    case WearableType.Eyes:
                        agent.Info.EyesItem = itemID;
                        break;
                    case WearableType.Shirt:
                        agent.Info.ShirtItem = itemID;
                        break;
                    case WearableType.Pants:
                        agent.Info.PantsItem = itemID;
                        break;
                    case WearableType.Shoes:
                        agent.Info.ShoesItem = itemID;
                        break;
                    case WearableType.Socks:
                        agent.Info.SocksItem = itemID;
                        break;
                    case WearableType.Jacket:
                        agent.Info.JacketItem = itemID;
                        break;
                    case WearableType.Gloves:
                        agent.Info.GlovesItem = itemID;
                        break;
                    case WearableType.Undershirt:
                        agent.Info.UndershirtItem = itemID;
                        break;
                    case WearableType.Underpants:
                        agent.Info.UnderpantsItem = itemID;
                        break;
                    case WearableType.Skirt:
                        agent.Info.SkirtItem = itemID;
                        break;
                }

                #endregion Update Wearables
            }

            // FIXME: GetCurrentWearables() is a very expensive call, remove it from this debug line
            Logger.DebugLog("Updated agent wearables, new count: " + GetCurrentWearables(agent).Count);
        }

        void AgentSetAppearanceHandler(Packet packet, Agent agent)
        {
            AgentSetAppearancePacket set = (AgentSetAppearancePacket)packet;

            Logger.DebugLog("Updating avatar appearance");

            //TODO: Store this for cached bake responses
            for (int i = 0; i < set.WearableData.Length; i++)
            {
                AppearanceManager.TextureIndex index = (AppearanceManager.TextureIndex)set.WearableData[i].TextureIndex;
                UUID cacheID = set.WearableData[i].CacheID;

                Logger.DebugLog(String.Format("WearableData: {0} is now {1}", index, cacheID));
            }

            // Create a TextureEntry
            Primitive.TextureEntry textureEntry = new Primitive.TextureEntry(set.ObjectData.TextureEntry, 0,
                set.ObjectData.TextureEntry.Length);

            // Create a block of VisualParams
            byte[] visualParams = new byte[set.VisualParam.Length];
            for (int i = 0; i < set.VisualParam.Length; i++)
                visualParams[i] = set.VisualParam[i].ParamValue;

            scene.AgentAppearance(this, agent, textureEntry, visualParams);
        }

        void AgentCachedTextureHandler(Packet packet, Agent agent)
        {
            AgentCachedTexturePacket cached = (AgentCachedTexturePacket)packet;

            AgentCachedTextureResponsePacket response = new AgentCachedTextureResponsePacket();
            response.AgentData.AgentID = agent.ID;
            response.AgentData.SerialNum = cached.AgentData.SerialNum;

            response.WearableData = new AgentCachedTextureResponsePacket.WearableDataBlock[cached.WearableData.Length];

            // TODO: Respond back with actual cache entries if we have them
            for (int i = 0; i < cached.WearableData.Length; i++)
            {
                response.WearableData[i] = new AgentCachedTextureResponsePacket.WearableDataBlock();
                response.WearableData[i].TextureIndex = cached.WearableData[i].TextureIndex;
                response.WearableData[i].TextureID = UUID.Zero;
                response.WearableData[i].HostName = Utils.EmptyBytes;
            }

            response.Header.Zerocoded = true;

            scene.UDP.SendPacket(agent.ID, response, PacketCategory.Transaction);
        }

        void AgentHeightWidthHandler(Packet packet, Agent agent)
        {
            //AgentHeightWidthPacket heightWidth = (AgentHeightWidthPacket)packet;

            // TODO: These are the screen size dimensions. Useful when we start doing frustum culling
            //Logger.Log(String.Format("Agent wants to set height={0}, width={1}",
            //    heightWidth.HeightWidthBlock.Height, heightWidth.HeightWidthBlock.Width), Helpers.LogLevel.Info);
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
                if (scene.TryGetAgent(id, out foundAgent))
                {
                    reply.UUIDNameBlock[i].FirstName = Utils.StringToBytes(foundAgent.Info.FirstName);
                    reply.UUIDNameBlock[i].LastName = Utils.StringToBytes(foundAgent.Info.LastName);
                }
                else
                {
                    reply.UUIDNameBlock[i].FirstName = Utils.EmptyBytes;
                    reply.UUIDNameBlock[i].LastName = Utils.EmptyBytes;
                }
            }

            scene.UDP.SendPacket(agent.ID, reply, PacketCategory.Transaction);
        }

        void TriggerSound(Agent agent, UUID soundID, float gain)
        {
            scene.TriggerSound(this, agent.ID, agent.ID, agent.ID, soundID, agent.Avatar.Prim.Position, gain);
        }

        void SendWearables(Agent agent)
        {
            AgentWearablesUpdatePacket update = new AgentWearablesUpdatePacket();
            update.AgentData.AgentID = agent.ID;

            Dictionary<WearableType, InventoryItem> wearables = GetCurrentWearables(agent);
            update.WearableData = new AgentWearablesUpdatePacket.WearableDataBlock[wearables.Count];
            int i = 0;

            foreach (KeyValuePair<WearableType, InventoryItem> kvp in wearables)
            {
                update.WearableData[i] = new AgentWearablesUpdatePacket.WearableDataBlock();
                update.WearableData[i].AssetID = kvp.Value.AssetID;
                update.WearableData[i].ItemID = kvp.Value.ID;
                update.WearableData[i].WearableType = (byte)kvp.Key;
                ++i;
            }

            // Technically this should be per-agent, but if the only requirement is that it
            // increments this is easier
            update.AgentData.SerialNum = (uint)Interlocked.Increment(ref currentWearablesSerialNum);

            Logger.DebugLog(String.Format("Sending info about {0} wearables", wearables.Count));

            scene.UDP.SendPacket(agent.ID, update, PacketCategory.Asset);
        }

        Dictionary<WearableType, InventoryItem> GetCurrentWearables(Agent agent)
        {
            Dictionary<WearableType, InventoryItem> wearables = new Dictionary<WearableType, InventoryItem>();

            TryAddWearable(agent.ID, wearables, WearableType.Shape, agent.Info.ShapeItem);
            TryAddWearable(agent.ID, wearables, WearableType.Skin, agent.Info.SkinItem);
            TryAddWearable(agent.ID, wearables, WearableType.Hair, agent.Info.HairItem);
            TryAddWearable(agent.ID, wearables, WearableType.Eyes, agent.Info.EyesItem);
            TryAddWearable(agent.ID, wearables, WearableType.Shirt, agent.Info.ShirtItem);
            TryAddWearable(agent.ID, wearables, WearableType.Pants, agent.Info.PantsItem);
            TryAddWearable(agent.ID, wearables, WearableType.Shoes, agent.Info.ShoesItem);
            TryAddWearable(agent.ID, wearables, WearableType.Socks, agent.Info.SocksItem);
            TryAddWearable(agent.ID, wearables, WearableType.Jacket, agent.Info.JacketItem);
            TryAddWearable(agent.ID, wearables, WearableType.Gloves, agent.Info.GlovesItem);
            TryAddWearable(agent.ID, wearables, WearableType.Undershirt, agent.Info.UndershirtItem);
            TryAddWearable(agent.ID, wearables, WearableType.Underpants, agent.Info.UnderpantsItem);
            TryAddWearable(agent.ID, wearables, WearableType.Skirt, agent.Info.SkirtItem);

            return wearables;
        }

        bool TryAddWearable(UUID agentID, Dictionary<WearableType, InventoryItem> wearables, WearableType type, UUID itemID)
        {
            InventoryObject obj;
            if (itemID != UUID.Zero && scene.Server.Inventory.TryGetInventory(agentID, itemID, out obj) &&
                obj is InventoryItem)
            {
                wearables.Add(type, (InventoryItem)obj);
                return true;
            }
            else
            {
                return false;
            }
        }

        void coarseLocationTimer_Elapsed(object sender)
        {
            // Create lists containing all of the agent blocks
            List<CoarseLocationUpdatePacket.AgentDataBlock> agentDatas = new List<CoarseLocationUpdatePacket.AgentDataBlock>();
            List<CoarseLocationUpdatePacket.LocationBlock> agentLocations = new List<CoarseLocationUpdatePacket.LocationBlock>();

            scene.ForEachAgent(
                delegate(Agent agent)
                {
                    CoarseLocationUpdatePacket.AgentDataBlock dataBlock = new CoarseLocationUpdatePacket.AgentDataBlock();
                    dataBlock.AgentID = agent.ID;
                    CoarseLocationUpdatePacket.LocationBlock locationBlock = new CoarseLocationUpdatePacket.LocationBlock();
                    locationBlock.X = (byte)agent.Avatar.Prim.Position.X;
                    locationBlock.Y = (byte)agent.Avatar.Prim.Position.Y;
                    locationBlock.Z = (byte)((int)agent.Avatar.Prim.Position.Z / 4);

                    agentDatas.Add(dataBlock);
                    agentLocations.Add(locationBlock);
                }
            );

            // Send location updates out to each agent
            scene.ForEachAgent(
                delegate(Agent agent)
                {
                    CoarseLocationUpdatePacket update = new CoarseLocationUpdatePacket();
                    update.Index.Prey = -1;
                    update.Index.You = 0;

                    // Count the number of blocks to send out
                    int count = 0;
                    for (int i = 0; i < agentDatas.Count; i++)
                    {
                        if (agentDatas[i].AgentID != agent.ID)
                            ++count;
                    }

                    update.AgentData = new CoarseLocationUpdatePacket.AgentDataBlock[count];
                    update.Location = new CoarseLocationUpdatePacket.LocationBlock[count];

                    int j = 0;
                    for (int i = 0; i < agentDatas.Count; i++)
                    {
                        if (agentDatas[i].AgentID != agent.ID)
                        {
                            update.AgentData[j] = agentDatas[i];
                            update.Location[j] = agentLocations[i];
                            ++j;
                        }
                    }

                    scene.UDP.SendPacket(agent.ID, update, PacketCategory.State);
                }
            );
        }
    }
}
