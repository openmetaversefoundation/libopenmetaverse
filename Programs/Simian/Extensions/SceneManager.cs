using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using ExtensionLoader;
using HttpServer;
using OpenMetaverse;
using OpenMetaverse.Imaging;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Http;

namespace Simian.Extensions
{
    class EventQueueServerCap
    {
        public EventQueueServer Server;
        public Uri Capability;

        public EventQueueServerCap(EventQueueServer server, Uri capability)
        {
            Server = server;
            Capability = capability;
        }
    }

    public class SceneManager : IExtension<Simian>, ISceneProvider
    {
        Simian server;
        // Contains all scene objects, including prims and avatars
        DoubleDictionary<uint, UUID, SimulationObject> sceneObjects = new DoubleDictionary<uint, UUID, SimulationObject>();
        // A duplicate of the avatar information stored in sceneObjects, improves operations such as iterating over all agents
        Dictionary<UUID, Agent> sceneAgents = new Dictionary<UUID, Agent>();
        // Event queues for each avatar in the scene
        Dictionary<UUID, EventQueueServerCap> eventQueues = new Dictionary<UUID, EventQueueServerCap>();
        int currentLocalID = 1;
        ulong regionHandle;
        UUID regionID = UUID.Random();
        TerrainPatch[,] heightmap = new TerrainPatch[16, 16];
        Vector2[,] windSpeeds = new Vector2[16, 16];

        public event ObjectAddOrUpdateCallback OnObjectAddOrUpdate;
        public event ObjectRemoveCallback OnObjectRemove;
        public event ObjectSetRotationAxisCallback OnObjectSetRotationAxis;
        public event ObjectApplyImpulseCallback OnObjectApplyImpulse;
        public event ObjectApplyRotationalImpulseCallback OnObjectApplyRotationalImpulse;
        public event ObjectSetTorqueCallback OnObjectSetTorque;
        public event ObjectAnimateCallback OnObjectAnimate;
        public event ObjectChatCallback OnObjectChat;
        public event ObjectUndoCallback OnObjectUndo;
        public event ObjectRedoCallback OnObjectRedo;
        public event AgentAddCallback OnAgentAdd;
        public event AgentRemoveCallback OnAgentRemove;
        public event AgentAppearanceCallback OnAgentAppearance;
        public event TriggerSoundCallback OnTriggerSound;
        public event TriggerEffectsCallback OnTriggerEffects;
        public event TerrainUpdateCallback OnTerrainUpdate;
        public event WindUpdateCallback OnWindUpdate;

        public uint RegionX { get { return 7777; } }
        public uint RegionY { get { return 7777; } }
        public ulong RegionHandle { get { return regionHandle; } }
        public UUID RegionID { get { return regionID; } }
        public string RegionName { get { return "Simian"; } }
        public RegionFlags RegionFlags { get { return RegionFlags.None; } }

        public float WaterHeight { get { return 20f; } }

        public uint TerrainPatchWidth { get { return 16; } }
        public uint TerrainPatchHeight { get { return 16; } }
        public uint TerrainPatchCountWidth { get { return 16; } }
        public uint TerrainPatchCountHeight { get { return 16; } }

        public SceneManager()
        {
            regionHandle = Utils.UIntsToLong(RegionX * 256, RegionY * 256);
        }

        public void Start(Simian server)
        {
            this.server = server;

            server.UDP.RegisterPacketCallback(PacketType.CompleteAgentMovement, new PacketCallback(CompleteAgentMovementHandler));
            LoadTerrain(Simian.DATA_DIR + "heightmap.tga");
        }

        public void Stop()
        {
            lock (sceneAgents)
            {
                List<Agent> agents = new List<Agent>(sceneAgents.Values);
                for (int i = 0; i < agents.Count; i++)
                    ObjectRemove(this, agents[i].ID);
            }

            Logger.DebugLog("SceneManager is stopped");
        }

        #region Object Interfaces

        public void ObjectAddOrUpdate(object sender, SimulationObject obj, UUID ownerID, int scriptStartParam, PrimFlags creatorFlags, UpdateFlags updateFlags)
        {
            if (OnObjectAddOrUpdate != null)
            {
                OnObjectAddOrUpdate(sender, obj, ownerID, scriptStartParam, creatorFlags, updateFlags);
            }

            #region Initialize new objects

            // Check if the object already exists in the scene
            if (!sceneObjects.ContainsKey(obj.Prim.ID))
            {
                // Enable some default flags that all objects will have
                obj.Prim.Flags |= server.Permissions.GetDefaultObjectFlags();

                // Object did not exist before, so there's no way it could contain inventory
                obj.Prim.Flags |= PrimFlags.InventoryEmpty;

                // Fun Fact: Prim.OwnerID is only used by the LL viewer to mute sounds
                obj.Prim.OwnerID = ownerID;

                // Other than storing tree species, I have no idea what this does
                obj.Prim.ScratchPad = Utils.EmptyBytes;

                // Assign a unique LocalID to this object if no LocalID is set
                if (obj.Prim.LocalID == 0)
                    obj.Prim.LocalID = (uint)Interlocked.Increment(ref currentLocalID);

                // Assign a random ID to this object if no ID is set
                if (obj.Prim.ID == UUID.Zero)
                    obj.Prim.ID = UUID.Random();

                // Set the RegionHandle if no RegionHandle is set
                if (obj.Prim.RegionHandle == 0)
                    obj.Prim.RegionHandle = server.Scene.RegionHandle;

                // Make sure this object has properties
                if (obj.Prim.Properties == null)
                {
                    obj.Prim.Properties = new Primitive.ObjectProperties();
                    obj.Prim.Properties.CreationDate = DateTime.Now;
                    obj.Prim.Properties.CreatorID = ownerID;
                    obj.Prim.Properties.Name = "New Object";
                    obj.Prim.Properties.ObjectID = obj.Prim.ID;
                    obj.Prim.Properties.OwnerID = ownerID;
                    obj.Prim.Properties.Permissions = server.Permissions.GetDefaultPermissions();
                    obj.Prim.Properties.SalePrice = 10;
                }

                // Set the default scale
                if (obj.Prim.Scale == Vector3.Zero)
                    obj.Prim.Scale = new Vector3(0.5f, 0.5f, 0.5f);

                // Set the collision plane
                if (obj.Prim.CollisionPlane == Vector4.Zero)
                    obj.Prim.CollisionPlane = Vector4.UnitW;

                // Set default textures if none are set
                if (obj.Prim.Textures == null)
                    obj.Prim.Textures = new Primitive.TextureEntry(new UUID("89556747-24cb-43ed-920b-47caed15465f")); // Plywood

                // Add the object to the scene dictionary
                sceneObjects.Add(obj.Prim.LocalID, obj.Prim.ID, obj);
            }

            #endregion Initialize new objects

            // Reset the prim CRC
            obj.CRC = 0;

            #region UpdateFlags to packet type conversion

            bool canUseCompressed = true;
            bool canUseImproved = true;

            if ((updateFlags & UpdateFlags.FullUpdate) == UpdateFlags.FullUpdate || creatorFlags != PrimFlags.None)
            {
                canUseCompressed = false;
                canUseImproved = false;
            }
            else
            {
                if ((updateFlags & UpdateFlags.Velocity) != 0 ||
                    (updateFlags & UpdateFlags.Acceleration) != 0 ||
                    (updateFlags & UpdateFlags.CollisionPlane) != 0 ||
                    (updateFlags & UpdateFlags.Joint) != 0)
                {
                    canUseCompressed = false;
                }
                
                if ((updateFlags & UpdateFlags.PrimFlags) != 0 ||
                    (updateFlags & UpdateFlags.ParentID) != 0 ||
                    (updateFlags & UpdateFlags.Scale) != 0 ||
                    (updateFlags & UpdateFlags.PrimData) != 0 ||
                    (updateFlags & UpdateFlags.Text) != 0 ||
                    (updateFlags & UpdateFlags.NameValue) != 0 ||
                    (updateFlags & UpdateFlags.ExtraData) != 0 ||
                    (updateFlags & UpdateFlags.TextureAnim) != 0 ||
                    (updateFlags & UpdateFlags.Sound) != 0 ||
                    (updateFlags & UpdateFlags.Particles) != 0 ||
                    (updateFlags & UpdateFlags.Material) != 0 ||
                    (updateFlags & UpdateFlags.ClickAction) != 0 ||
                    (updateFlags & UpdateFlags.MediaURL) != 0 ||
                    (updateFlags & UpdateFlags.Joint) != 0)
                {
                    canUseImproved = false;
                }
            }

            #endregion UpdateFlags to packet type conversion

            SendObjectPacket(obj, canUseCompressed, canUseImproved, creatorFlags, updateFlags);
        }

        public bool ObjectRemove(object sender, uint localID)
        {
            SimulationObject obj;
            Agent agent;

            if (sceneObjects.TryGetValue(localID, out obj))
            {
                if (sceneAgents.TryGetValue(obj.Prim.ID, out agent))
                    AgentRemove(sender, agent);

                if (OnObjectRemove != null)
                    OnObjectRemove(sender, obj);

                sceneObjects.Remove(obj.Prim.LocalID, obj.Prim.ID);

                KillObjectPacket kill = new KillObjectPacket();
                kill.ObjectData = new KillObjectPacket.ObjectDataBlock[1];
                kill.ObjectData[0] = new KillObjectPacket.ObjectDataBlock();
                kill.ObjectData[0].ID = obj.Prim.LocalID;

                server.UDP.BroadcastPacket(kill, PacketCategory.State);
                return true;
            }

            return false;
        }

        public bool ObjectRemove(object sender, UUID id)
        {
            SimulationObject obj;
            Agent agent;

            if (sceneObjects.TryGetValue(id, out obj))
            {
                if (sceneAgents.TryGetValue(id, out agent))
                    AgentRemove(sender, agent);

                if (OnObjectRemove != null)
                    OnObjectRemove(sender, obj);

                sceneObjects.Remove(obj.Prim.LocalID, obj.Prim.ID);

                KillObjectPacket kill = new KillObjectPacket();
                kill.ObjectData = new KillObjectPacket.ObjectDataBlock[1];
                kill.ObjectData[0] = new KillObjectPacket.ObjectDataBlock();
                kill.ObjectData[0].ID = obj.Prim.LocalID;

                server.UDP.BroadcastPacket(kill, PacketCategory.State);
                return true;
            }

            return false;
        }

        public void ObjectSetRotationAxis(object sender, SimulationObject obj, Vector3 rotationAxis)
        {
            if (OnObjectSetRotationAxis != null)
            {
                OnObjectSetRotationAxis(sender, obj, rotationAxis);
            }

            // Update the object
            obj.RotationAxis = rotationAxis;
        }

        public void ObjectApplyImpulse(object sender, SimulationObject obj, Vector3 impulse)
        {
            if (OnObjectApplyImpulse != null)
            {
                OnObjectApplyImpulse(sender, obj, impulse);
            }

            // FIXME:
        }

        public void ObjectApplyRotationalImpulse(object sender, SimulationObject obj, Vector3 impulse)
        {
            if (OnObjectApplyRotationalImpulse != null)
            {
                OnObjectApplyRotationalImpulse(sender, obj, impulse);
            }

            // FIXME:
        }

        public void ObjectSetTorque(object sender, SimulationObject obj, Vector3 torque)
        {
            if (OnObjectSetTorque != null)
            {
                OnObjectSetTorque(sender, obj, torque);
            }

            obj.Torque = torque;
        }

        public void ObjectAnimate(object sender, UUID senderID, UUID objectID, AnimationTrigger[] animations)
        {
            if (OnObjectAnimate != null)
            {
                OnObjectAnimate(sender, senderID, objectID, animations);
            }

            AvatarAnimationPacket sendAnim = new AvatarAnimationPacket();
            sendAnim.Sender.ID = senderID;
            sendAnim.AnimationSourceList = new AvatarAnimationPacket.AnimationSourceListBlock[1];
            sendAnim.AnimationSourceList[0] = new AvatarAnimationPacket.AnimationSourceListBlock();
            sendAnim.AnimationSourceList[0].ObjectID = objectID;

            sendAnim.AnimationList = new AvatarAnimationPacket.AnimationListBlock[animations.Length];
            for (int i = 0; i < animations.Length; i++)
            {
                sendAnim.AnimationList[i] = new AvatarAnimationPacket.AnimationListBlock();
                sendAnim.AnimationList[i].AnimID = animations[i].AnimationID;
                sendAnim.AnimationList[i].AnimSequenceID = animations[i].SequenceID;
            }

            server.UDP.BroadcastPacket(sendAnim, PacketCategory.State);
        }

        public void ObjectChat(object sender, UUID ownerID, UUID sourceID, ChatAudibleLevel audible, ChatType type, ChatSourceType sourceType,
            string fromName, Vector3 position, int channel, string message)
        {
            if (OnObjectChat != null)
            {
                OnObjectChat(sender, ownerID, sourceID, audible, type, sourceType, fromName, position, channel, message);
            }

            if (channel == 0)
            {
                // TODO: Reduction provider will impose the chat radius
                ChatFromSimulatorPacket chat = new ChatFromSimulatorPacket();
                chat.ChatData.Audible = (byte)audible;
                chat.ChatData.ChatType = (byte)type;
                chat.ChatData.OwnerID = ownerID;
                chat.ChatData.SourceID = sourceID;
                chat.ChatData.SourceType = (byte)sourceType;
                chat.ChatData.Position = position;
                chat.ChatData.FromName = Utils.StringToBytes(fromName);
                chat.ChatData.Message = Utils.StringToBytes(message);

                server.UDP.BroadcastPacket(chat, PacketCategory.Messaging);
            }
        }

        public void ObjectUndo(object sender, SimulationObject obj)
        {
            if (OnObjectUndo != null)
            {
                OnObjectUndo(sender, obj);
            }

            Primitive prim = obj.UndoSteps.DequeueLast();
            if (prim != null)
            {
                Logger.Log(String.Format("Performing undo on object {0}", obj.Prim.ID), Helpers.LogLevel.Debug);

                obj.RedoSteps.Enqueue(prim);
                obj.Prim = prim;

                // Inform clients
                server.Scene.ObjectAddOrUpdate(this, obj, obj.Prim.OwnerID, 0, PrimFlags.None, UpdateFlags.FullUpdate);
            }
            else
            {
                Logger.Log(String.Format("Undo requested on object {0} with no remaining undo steps", obj.Prim.ID),
                    Helpers.LogLevel.Debug);
            }
        }

        public void ObjectRedo(object sender, SimulationObject obj)
        {
            if (OnObjectRedo != null)
            {
                OnObjectRedo(sender, obj);
            }

            Primitive prim = obj.RedoSteps.DequeueLast();
            if (prim != null)
            {
                Logger.Log(String.Format("Performing redo on object {0}", obj.Prim.ID), Helpers.LogLevel.Debug);

                obj.UndoSteps.Enqueue(prim);
                obj.Prim = prim;

                // Inform clients
                server.Scene.ObjectAddOrUpdate(this, obj, obj.Prim.OwnerID, 0, PrimFlags.None, UpdateFlags.FullUpdate);
            }
            else
            {
                Logger.Log(String.Format("Redo requested on object {0} with no remaining redo steps", obj.Prim.ID),
                    Helpers.LogLevel.Debug);
            }
        }

        public bool ContainsObject(uint localID)
        {
            return sceneObjects.ContainsKey(localID);
        }

        public bool ContainsObject(UUID id)
        {
            return sceneObjects.ContainsKey(id);
        }

        public int ObjectCount()
        {
            return sceneObjects.Count;
        }

        public bool TryGetObject(uint localID, out SimulationObject obj)
        {
            return sceneObjects.TryGetValue(localID, out obj);
        }

        public bool TryGetObject(UUID id, out SimulationObject obj)
        {
            return sceneObjects.TryGetValue(id, out obj);
        }

        public void ForEachObject(Action<SimulationObject> action)
        {
            sceneObjects.ForEach(action);
        }

        public SimulationObject FindObject(Predicate<SimulationObject> predicate)
        {
            return sceneObjects.FindValue(predicate);
        }

        public int RemoveAllObjects(Predicate<SimulationObject> predicate)
        {
            return sceneObjects.RemoveAll(predicate);
        }

        public void TriggerSound(object sender, UUID objectID, UUID parentID, UUID ownerID, UUID soundID, Vector3 position, float gain)
        {
            if (OnTriggerSound != null)
            {
                OnTriggerSound(sender, objectID, parentID, ownerID, soundID, position, gain);
            }

            SoundTriggerPacket sound = new SoundTriggerPacket();
            sound.SoundData.Handle = server.Scene.RegionHandle;
            sound.SoundData.ObjectID = objectID;
            sound.SoundData.ParentID = parentID;
            sound.SoundData.OwnerID = ownerID;
            sound.SoundData.Position = position;
            sound.SoundData.SoundID = soundID;
            sound.SoundData.Gain = gain;

            server.UDP.BroadcastPacket(sound, PacketCategory.State);
        }

        public void TriggerEffects(object sender, ViewerEffect[] effects)
        {
            if (OnTriggerEffects != null)
            {
                OnTriggerEffects(sender, effects);
            }

            ViewerEffectPacket effect = new ViewerEffectPacket();
            effect.AgentData.AgentID = UUID.Zero;
            effect.AgentData.SessionID = UUID.Zero;

            effect.Effect = new ViewerEffectPacket.EffectBlock[effects.Length];

            for (int i = 0; i < effects.Length; i++)
            {
                ViewerEffect currentEffect = effects[i];
                ViewerEffectPacket.EffectBlock block = new ViewerEffectPacket.EffectBlock();

                block.AgentID = currentEffect.AgentID;
                block.Color = currentEffect.Color.GetBytes(true);
                block.Duration = currentEffect.Duration;
                block.ID = currentEffect.EffectID;
                block.Type = (byte)currentEffect.Type;
                block.TypeData = currentEffect.TypeData;

                effect.Effect[i] = block;
            }

            server.UDP.BroadcastPacket(effect, PacketCategory.State);
        }

        #endregion Object Interfaces

        #region Agent Interfaces

        public bool AgentAdd(object sender, Agent agent, PrimFlags creatorFlags)
        {
            // Check if the agent already exists in the scene
            lock (sceneAgents)
            {
                if (sceneAgents.ContainsKey(agent.ID))
                    sceneAgents.Remove(agent.ID);
            }

            // Avatars always have physics
            agent.Avatar.Prim.Flags |= PrimFlags.Physics;

            // Default avatar values
            agent.Avatar.Prim.Position = new Vector3(128f, 128f, 25f);
            agent.Avatar.Prim.Rotation = Quaternion.Identity;
            agent.Avatar.Prim.Scale = new Vector3(0.45f, 0.6f, 1.9f);
            agent.Avatar.Prim.PrimData.Material = Material.Flesh;
            agent.Avatar.Prim.PrimData.PCode = PCode.Avatar;
            agent.Avatar.Prim.Textures = new Primitive.TextureEntry(new UUID("c228d1cf-4b5d-4ba8-84f4-899a0796aa97"));

            // Set the avatar name
            NameValue[] name = new NameValue[2];
            name[0] = new NameValue("FirstName", NameValue.ValueType.String, NameValue.ClassType.ReadWrite,
                NameValue.SendtoType.SimViewer, agent.FirstName);
            name[1] = new NameValue("LastName", NameValue.ValueType.String, NameValue.ClassType.ReadWrite,
                NameValue.SendtoType.SimViewer, agent.LastName);
            agent.Avatar.Prim.NameValues = name;

            // Give testers a provisionary balance of 1000L
            agent.Balance = 1000;

            // Some default avatar prim properties
            agent.Avatar.Prim.Properties = new Primitive.ObjectProperties();
            agent.Avatar.Prim.Properties.CreationDate = Utils.UnixTimeToDateTime(agent.CreationTime);
            agent.Avatar.Prim.Properties.Name = agent.FullName;
            agent.Avatar.Prim.Properties.ObjectID = agent.ID;

            if (agent.Avatar.Prim.LocalID == 0)
            {
                // Assign a unique LocalID to this agent
                agent.Avatar.Prim.LocalID = (uint)Interlocked.Increment(ref currentLocalID);
            }

            if (OnAgentAdd != null)
                OnAgentAdd(sender, agent, creatorFlags);

            // Add the agent to the scene dictionary
            lock (sceneAgents) sceneAgents[agent.ID] = agent;

            Logger.Log("Added agent " + agent.FullName + " to the scene", Helpers.LogLevel.Info);
            return true;
        }

        void AgentRemove(object sender, Agent agent)
        {
            if (OnAgentRemove != null)
                OnAgentRemove(sender, agent);

            Logger.Log("Removing agent " + agent.FullName + " from the scene", Helpers.LogLevel.Info);

            lock (sceneAgents) sceneAgents.Remove(agent.ID);

            // Kill the EventQueue
            RemoveEventQueue(agent.ID);

            // Remove the UDP client
            server.UDP.RemoveClient(agent);

            // Notify everyone in the scene that this agent has gone offline
            OfflineNotificationPacket offline = new OfflineNotificationPacket();
            offline.AgentBlock = new OfflineNotificationPacket.AgentBlockBlock[1];
            offline.AgentBlock[0] = new OfflineNotificationPacket.AgentBlockBlock();
            offline.AgentBlock[0].AgentID = agent.ID;
            server.UDP.BroadcastPacket(offline, PacketCategory.State);
        }

        public void AgentAppearance(object sender, Agent agent, Primitive.TextureEntry textures, byte[] visualParams)
        {
            if (OnAgentAppearance != null)
            {
                OnAgentAppearance(sender, agent, textures, visualParams);
            }

            // Broadcast an object update for this avatar
            // TODO: Is this necessary here?
            //ObjectUpdatePacket update = SimulationObject.BuildFullUpdate(agent.Avatar,
            //    regionHandle, agent.Flags);
            //server.UDP.BroadcastPacket(update, PacketCategory.State);

            // Update the avatar
            agent.Avatar.Prim.Textures = textures;
            if (visualParams != null && visualParams.Length > 1)
                agent.VisualParams = visualParams;

            if (agent.VisualParams != null)
            {
                // Send the appearance packet to all other clients
                AvatarAppearancePacket appearance = BuildAppearancePacket(agent);
                ForEachAgent(
                    delegate(Agent recipient)
                    {
                        if (recipient != agent)
                            server.UDP.SendPacket(recipient.ID, appearance, PacketCategory.State);
                    }
                );
            }
        }

        public bool TryGetAgent(UUID id, out Agent agent)
        {
            return sceneAgents.TryGetValue(id, out agent);
        }

        public int AgentCount()
        {
            return sceneAgents.Count;
        }

        public void ForEachAgent(Action<Agent> action)
        {
            lock (sceneAgents)
            {
                foreach (Agent agent in sceneAgents.Values)
                    action(agent);
            }
        }

        public Agent FindAgent(Predicate<Agent> predicate)
        {
            lock (sceneAgents)
            {
                foreach (Agent agent in sceneAgents.Values)
                {
                    if (predicate(agent))
                        return agent;
                }
            }

            return null;
        }

        public int RemoveAllAgents(Predicate<Agent> predicate)
        {
            List<UUID> list = new List<UUID>();

            lock (sceneAgents)
            {
                foreach (KeyValuePair<UUID, Agent> kvp in sceneAgents)
                {
                    if (predicate(kvp.Value))
                        list.Add(kvp.Key);
                }

                for (int i = 0; i < list.Count; i++)
                    sceneAgents.Remove(list[i]);
            }

            return list.Count;
        }

        #endregion Agent Interfaces

        #region Terrain and Wind

        public float GetTerrainHeightAt(float fx, float fy)
        {
            int x = (int)fx;
            int y = (int)fy;

            if (x > 255) x = 255;
            else if (x < 0) x = 0;
            if (y > 255) y = 255;
            else if (y < 0) y = 0;

            int patchX = x / 16;
            int patchY = y / 16;

            if (heightmap[patchY, patchX] != null)
            {
                float center = heightmap[patchY, patchX].Height[y - (patchY * 16), x - (patchX * 16)];

                float distX = fx - (int)fx;
                float distY = fy - (int)fy;

                float nearestX;
                float nearestY;

                if (distX > 0f)
                {
                    int i = x < 255 ? 1 : 0;
                    int newPatchX = (x + i) / 16;
                    nearestX = heightmap[patchY, newPatchX].Height[y - (patchY * 16), (x + i) - (newPatchX * 16)];
                }
                else
                {
                    int i = x > 0 ? 1 : 0;
                    int newPatchX = (x - i) / 16;
                    nearestX = heightmap[patchY, newPatchX].Height[y - (patchY * 16), (x - i) - (newPatchX * 16)];
                }

                if (distY > 0f)
                {
                    int i = y < 255 ? 1 : 0;
                    int newPatchY = (y + i) / 16;
                    nearestY = heightmap[newPatchY, patchX].Height[(y + i) - (newPatchY * 16), x - (patchX * 16)];
                }
                else
                {
                    int i = y > 0 ? 1 : 0;
                    int newPatchY = (y - i) / 16;
                    nearestY = heightmap[newPatchY, patchX].Height[(y - i) - (newPatchY * 16), x - (patchX * 16)];
                }

                float lerpX = Utils.Lerp(center, nearestX, Math.Abs(distX));
                float lerpY = Utils.Lerp(center, nearestY, Math.Abs(distY));

                return ((lerpX + lerpY) / 2);
            }
            else
            {
                return 0f;
            }
        }

        public float[,] GetTerrainPatch(uint x, uint y)
        {
            float[,] copy = new float[16, 16];
            Buffer.BlockCopy(heightmap[y, x].Height, 0, copy, 0, 16 * 16 * sizeof(float));
            return copy;
        }

        public void SetTerrainPatch(object sender, uint x, uint y, float[,] patchData)
        {
            if (OnTerrainUpdate != null)
                OnTerrainUpdate(sender, x, y, patchData);

            float[,] copy = new float[16, 16];
            Buffer.BlockCopy(patchData, 0, copy, 0, 16 * 16 * sizeof(float));
            heightmap[y, x].Height = copy;

            LayerDataPacket layer = TerrainCompressor.CreateLandPacket(heightmap[y, x].Height, (int)x, (int)y);
            server.UDP.BroadcastPacket(layer, PacketCategory.Terrain);
        }

        public Vector2 GetWindSpeedAt(float fx, float fy)
        {
            int x = (int)fx;
            int y = (int)fy;

            if (x > 255) x = 255;
            else if (x < 0) x = 0;
            if (y > 255) y = 255;
            else if (y < 0) y = 0;

            int patchX = x / 16;
            int patchY = y / 16;

            return windSpeeds[patchY, patchX];
        }

        public Vector2 GetWindSpeed(uint x, uint y)
        {
            return windSpeeds[y, x];
        }

        public void SetWindSpeed(object sender, uint x, uint y, Vector2 windSpeed)
        {
            if (OnWindUpdate != null)
            {
                OnWindUpdate(sender, x, y, windSpeed);
            }

            windSpeeds[y, x] = windSpeed;
        }

        #endregion Terrain and Wind

        #region Capabilities Interfaces

        public Uri CreateEventQueue(UUID agentID)
        {
            EventQueueServer eqServer = new EventQueueServer(server.HttpServer);
            EventQueueServerCap eqServerCap = new EventQueueServerCap(eqServer,
                server.Capabilities.CreateCapability(EventQueueHandler, false, eqServer));

            lock (eventQueues)
                eventQueues.Add(agentID, eqServerCap);
            
            return eqServerCap.Capability;
        }

        public bool RemoveEventQueue(UUID agentID)
        {
            lock (eventQueues)
            {
                EventQueueServerCap queue;
                if (eventQueues.TryGetValue(agentID, out queue))
                {
                    queue.Server.Stop();
                    return eventQueues.Remove(agentID);
                }
                else
                {
                    return false;
                }
            }
        }

        public bool HasRunningEventQueue(Agent agent)
        {
            return eventQueues.ContainsKey(agent.ID);
        }

        public void SendEvent(Agent agent, string name, OSDMap body)
        {
            EventQueueServerCap eventQueue;
            if (eventQueues.TryGetValue(agent.ID, out eventQueue))
            {
                eventQueue.Server.SendEvent(name, body);
            }
            else
            {
                Logger.Log(String.Format("Cannot send the event {0} to agent {1}, no event queue for that avatar",
                    name, agent.FullName), Helpers.LogLevel.Warning);
            }
        }

        public bool SeedCapabilityHandler(IHttpClientContext context, IHttpRequest request, IHttpResponse response, object state)
        {
            UUID agentID = (UUID)state;

            OSDArray array = OSDParser.DeserializeLLSDXml(request.Body) as OSDArray;
            if (array != null)
            {
                OSDMap osdResponse = new OSDMap();

                for (int i = 0; i < array.Count; i++)
                {
                    string capName = array[i].AsString();

                    switch (capName)
                    {
                        case "EventQueueGet":
                            Uri eqCap = null;

                            // Check if this agent already has an event queue
                            EventQueueServerCap eqServer;
                            if (eventQueues.TryGetValue(agentID, out eqServer))
                                eqCap = eqServer.Capability;

                            // If not, create one
                            if (eqCap == null)
                                eqCap = CreateEventQueue(agentID);

                            osdResponse.Add("EventQueueGet", OSD.FromUri(eqCap));
                            break;
                    }
                }

                byte[] responseData = OSDParser.SerializeLLSDXmlBytes(osdResponse);
                response.ContentLength = responseData.Length;
                response.Body.Write(responseData, 0, responseData.Length);
                response.Body.Flush();
            }
            else
            {
                response.Status = HttpStatusCode.BadRequest;
            }

            return true;
        }

        #endregion Capabilities Interfaces

        #region Callback Handlers

        bool EventQueueHandler(IHttpClientContext context, IHttpRequest request, IHttpResponse response, object state)
        {
            EventQueueServer eqServer = (EventQueueServer)state;
            return eqServer.EventQueueHandler(context, request, response);
        }

        void CompleteAgentMovementHandler(Packet packet, Agent agent)
        {
            // Add this avatar as an object in the scene
            ObjectAddOrUpdate(this, agent.Avatar, agent.Avatar.Prim.OwnerID, 0, PrimFlags.None, UpdateFlags.FullUpdate);

            // Send a response back to the client
            AgentMovementCompletePacket complete = new AgentMovementCompletePacket();
            complete.AgentData.AgentID = agent.ID;
            complete.AgentData.SessionID = agent.SessionID;
            complete.Data.LookAt = Vector3.UnitX;
            complete.Data.Position = agent.Avatar.Prim.Position;
            complete.Data.RegionHandle = regionHandle;
            complete.Data.Timestamp = Utils.DateTimeToUnixTime(DateTime.Now);
            complete.SimData.ChannelVersion = Utils.StringToBytes("Simian");

            server.UDP.SendPacket(agent.ID, complete, PacketCategory.Transaction);

            // Send updates and appearances for every avatar to this new avatar
            SynchronizeStateTo(agent);

            //HACK: Notify everyone when someone logs on to the simulator
            OnlineNotificationPacket online = new OnlineNotificationPacket();
            online.AgentBlock = new OnlineNotificationPacket.AgentBlockBlock[1];
            online.AgentBlock[0] = new OnlineNotificationPacket.AgentBlockBlock();
            online.AgentBlock[0].AgentID = agent.ID;
            server.UDP.BroadcastPacket(online, PacketCategory.State);
        }

        #endregion Callback Handlers

        // HACK: The reduction provider will deprecate this at some point
        void SynchronizeStateTo(Agent agent)
        {
            // Send the parcel overlay
            server.Parcels.SendParcelOverlay(agent);

            // Send object updates for objects and avatars
            sceneObjects.ForEach(delegate(SimulationObject obj)
            {
                ObjectUpdatePacket update = new ObjectUpdatePacket();
                update.RegionData.RegionHandle = regionHandle;
                update.RegionData.TimeDilation = (ushort)(server.Physics.TimeDilation * (float)UInt16.MaxValue);
                update.ObjectData = new ObjectUpdatePacket.ObjectDataBlock[1];
                update.ObjectData[0] = SimulationObject.BuildUpdateBlock(obj.Prim, obj.Prim.Flags, obj.CRC);

                server.UDP.SendPacket(agent.ID, update, PacketCategory.State);
            });

            // Send appearances for all avatars
            ForEachAgent(
                delegate(Agent otherAgent)
                {
                    if (otherAgent != agent)
                    {
                        // Send appearances for this avatar
                        AvatarAppearancePacket appearance = BuildAppearancePacket(otherAgent);
                        server.UDP.SendPacket(agent.ID, appearance, PacketCategory.State);
                    }
                }
            );

            // Send terrain data
            SendLayerData(agent);
        }

        void LoadTerrain(string mapFile)
        {
            byte[] rgbValues = new byte[256 * 256 * 3];

            if (File.Exists(mapFile))
            {
                lock (heightmap)
                {
                    Bitmap bmp = LoadTGAClass.LoadTGA(mapFile);

                    if (bmp.Width == 256 && bmp.Height == 256)
                    {
                        Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                        BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                        Marshal.Copy(bmpData.Scan0, rgbValues, 0, rgbValues.Length);
                        bmp.UnlockBits(bmpData);
                    }
                    else
                    {
                        Logger.Log("Map file " + mapFile + " has the wrong dimensions or wrong pixel format (must be 256x256 RGB). Defaulting to 25m",
                            Helpers.LogLevel.Warning);
                        for (int i = 0; i < rgbValues.Length; i++)
                            rgbValues[i] = 25;
                    }
                }
            }
            else
            {
                Logger.Log("Map file " + mapFile + " not found, defaulting to 25m", Helpers.LogLevel.Info);
                for (int i = 0; i < rgbValues.Length; i++)
                    rgbValues[i] = 25;
            }

            uint patchX = 0, patchY = 0, x = 0, y = 0;
            for (int i = 0; i < rgbValues.Length; i += 3)
            {
                if (heightmap[patchY, patchX] == null)
                    heightmap[patchY, patchX] = new TerrainPatch(16, 16);

                heightmap[patchY, patchX].Height[y, x] = (float)rgbValues[i];

                ++x;
                if (x > 15)
                {
                    if (y == 15)
                    {
                        if (OnTerrainUpdate != null)
                            OnTerrainUpdate(this, patchX, patchY, heightmap[patchY, patchX].Height);
                    }

                    x = 0;
                    ++patchX;
                }

                if (patchX > 15)
                {
                    patchX = 0;
                    ++y;
                }

                if (y > 15)
                {
                    y = 0;
                    ++patchY;
                }
            }
        }

        void SendLayerData(Agent agent)
        {
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    LayerDataPacket layer = TerrainCompressor.CreateLandPacket(heightmap[y, x].Height, x, y);
                    server.UDP.SendPacket(agent.ID, layer, PacketCategory.Terrain);
                }
            }
        }

        void SendObjectPacket(SimulationObject obj, bool canUseCompressed, bool canUseImproved, PrimFlags creatorFlags, UpdateFlags updateFlags)
        {
            if (!canUseImproved && !canUseCompressed)
            {
                #region ObjectUpdate

                Logger.DebugLog("Sending ObjectUpdate");

                if (sceneAgents.ContainsKey(obj.Prim.OwnerID))
                {
                    // Send an update out to the creator
                    ObjectUpdatePacket updateToOwner = new ObjectUpdatePacket();
                    updateToOwner.RegionData.RegionHandle = regionHandle;
                    updateToOwner.RegionData.TimeDilation = (ushort)(server.Physics.TimeDilation * (float)UInt16.MaxValue);
                    updateToOwner.ObjectData = new ObjectUpdatePacket.ObjectDataBlock[1];
                    updateToOwner.ObjectData[0] = SimulationObject.BuildUpdateBlock(obj.Prim,
                        obj.Prim.Flags | creatorFlags | PrimFlags.ObjectYouOwner, obj.CRC);

                    server.UDP.SendPacket(obj.Prim.OwnerID, updateToOwner, PacketCategory.State);
                }

                // Send an update out to everyone else
                ObjectUpdatePacket updateToOthers = new ObjectUpdatePacket();
                updateToOthers.RegionData.RegionHandle = regionHandle;
                updateToOthers.RegionData.TimeDilation = UInt16.MaxValue;
                updateToOthers.ObjectData = new ObjectUpdatePacket.ObjectDataBlock[1];
                updateToOthers.ObjectData[0] = SimulationObject.BuildUpdateBlock(obj.Prim,
                    obj.Prim.Flags, obj.CRC);

                server.Scene.ForEachAgent(
                    delegate(Agent recipient)
                    {
                        if (recipient.ID != obj.Prim.OwnerID)
                            server.UDP.SendPacket(recipient.ID, updateToOthers, PacketCategory.State);
                    }
                );

                #endregion ObjectUpdate
            }
            else if (!canUseImproved)
            {
                #region ObjectUpdateCompressed

                #region Size calculation and field serialization

                CompressedFlags flags = 0;
                int size = 84;
                byte[] textBytes = null;
                byte[] mediaURLBytes = null;
                byte[] particleBytes = null;
                byte[] extraParamBytes = null;
                byte[] nameValueBytes = null;
                byte[] textureBytes = null;
                byte[] textureAnimBytes = null;

                if ((updateFlags & UpdateFlags.AngularVelocity) != 0)
                {
                    flags |= CompressedFlags.HasAngularVelocity;
                    size += 12;
                }
                if ((updateFlags & UpdateFlags.ParentID) != 0)
                {
                    flags |= CompressedFlags.HasParent;
                    size += 4;
                }
                if ((updateFlags & UpdateFlags.ScratchPad) != 0)
                {
                    switch (obj.Prim.PrimData.PCode)
                    {
                        case PCode.Grass:
                        case PCode.Tree:
                        case PCode.NewTree:
                            flags |= CompressedFlags.Tree;
                            size += 2; // Size byte plus one byte
                            break;
                        default:
                            flags |= CompressedFlags.ScratchPad;
                            size += 1 + obj.Prim.ScratchPad.Length; // Size byte plus bytes
                            break;
                    }
                }
                if ((updateFlags & UpdateFlags.Text) != 0)
                {
                    flags |= CompressedFlags.HasText;
                    textBytes = Utils.StringToBytes(obj.Prim.Text);
                    size += textBytes.Length; // Null-terminated, no size byte
                    size += 4; // Text color
                }
                if ((updateFlags & UpdateFlags.MediaURL) != 0)
                {
                    flags |= CompressedFlags.MediaURL;
                    mediaURLBytes = Utils.StringToBytes(obj.Prim.MediaURL);
                    size += mediaURLBytes.Length; // Null-terminated, no size byte
                }
                if ((updateFlags & UpdateFlags.Particles) != 0)
                {
                    flags |= CompressedFlags.HasParticles;
                    particleBytes = obj.Prim.ParticleSys.GetBytes();
                    size += particleBytes.Length; // Should be exactly 86 bytes
                }

                // Extra Params
                extraParamBytes = obj.Prim.GetExtraParamsBytes();
                size += extraParamBytes.Length;

                if ((updateFlags & UpdateFlags.Sound) != 0)
                {
                    flags |= CompressedFlags.HasSound;
                    size += 25; // SoundID, SoundGain, SoundFlags, SoundRadius
                }
                if ((updateFlags & UpdateFlags.NameValue) != 0)
                {
                    flags |= CompressedFlags.HasNameValues;
                    nameValueBytes = Utils.StringToBytes(NameValue.NameValuesToString(obj.Prim.NameValues));
                    size += nameValueBytes.Length; // Null-terminated, no size byte
                }

                size += 23; // PrimData
                size += 4; // Texture Length
                textureBytes = obj.Prim.Textures.GetBytes();
                size += textureBytes.Length; // Texture Entry

                if ((updateFlags & UpdateFlags.TextureAnim) != 0)
                {
                    flags |= CompressedFlags.TextureAnimation;
                    size += 4; // TextureAnim Length
                    textureAnimBytes = obj.Prim.TextureAnim.GetBytes();
                    size += textureAnimBytes.Length; // TextureAnim
                }

                #endregion Size calculation and field serialization

                #region Packet serialization

                int pos = 0;
                byte[] data = new byte[size];

                // UUID
                obj.Prim.ID.ToBytes(data, 0);
                pos += 16;
                // LocalID
                Utils.UIntToBytes(obj.Prim.LocalID, data, pos);
                pos += 4;
                // PCode
                data[pos++] = (byte)obj.Prim.PrimData.PCode;
                // State
                data[pos++] = obj.Prim.PrimData.State;
                // CRC
                Utils.UIntToBytes(obj.CRC, data, pos);
                pos += 4;
                // Material
                data[pos++] = (byte)obj.Prim.PrimData.Material;
                // ClickAction
                data[pos++] = (byte)obj.Prim.ClickAction;
                // Scale
                obj.Prim.Scale.ToBytes(data, pos);
                pos += 12;
                // Position
                obj.Prim.Position.ToBytes(data, pos);
                pos += 12;
                // Rotation
                obj.Prim.Rotation.ToBytes(data, pos);
                pos += 12;
                // Compressed Flags
                Utils.UIntToBytes((uint)flags, data, pos);
                pos += 4;
                // OwnerID
                obj.Prim.OwnerID.ToBytes(data, pos);
                pos += 16;

                if ((flags & CompressedFlags.HasAngularVelocity) != 0)
                {
                    obj.Prim.AngularVelocity.ToBytes(data, pos);
                    pos += 12;
                }
                if ((flags & CompressedFlags.HasParent) != 0)
                {
                    Utils.UIntToBytes(obj.Prim.ParentID, data, pos);
                    pos += 4;
                }
                if ((flags & CompressedFlags.ScratchPad) != 0)
                {
                    data[pos++] = (byte)obj.Prim.ScratchPad.Length;
                    Buffer.BlockCopy(obj.Prim.ScratchPad, 0, data, pos, obj.Prim.ScratchPad.Length);
                    pos += obj.Prim.ScratchPad.Length;
                }
                else if ((flags & CompressedFlags.Tree) != 0)
                {
                    data[pos++] = 1;
                    data[pos++] = (byte)obj.Prim.TreeSpecies;
                }
                if ((flags & CompressedFlags.HasText) != 0)
                {
                    Buffer.BlockCopy(textBytes, 0, data, pos, textBytes.Length);
                    pos += textBytes.Length;
                    obj.Prim.TextColor.ToBytes(data, pos, false);
                    pos += 4;
                }
                if ((flags & CompressedFlags.MediaURL) != 0)
                {
                    Buffer.BlockCopy(mediaURLBytes, 0, data, pos, mediaURLBytes.Length);
                    pos += mediaURLBytes.Length;
                }
                if ((flags & CompressedFlags.HasParticles) != 0)
                {
                    Buffer.BlockCopy(particleBytes, 0, data, pos, particleBytes.Length);
                    pos += particleBytes.Length;
                }

                // Extra Params
                Buffer.BlockCopy(extraParamBytes, 0, data, pos, extraParamBytes.Length);
                pos += extraParamBytes.Length;

                if ((flags & CompressedFlags.HasSound) != 0)
                {
                    obj.Prim.Sound.ToBytes(data, pos);
                    pos += 16;
                    Utils.FloatToBytes(obj.Prim.SoundGain, data, pos);
                    pos += 4;
                    data[pos++] = (byte)obj.Prim.SoundFlags;
                    Utils.FloatToBytes(obj.Prim.SoundRadius, data, pos);
                    pos += 4;
                }
                if ((flags & CompressedFlags.HasNameValues) != 0)
                {
                    Buffer.BlockCopy(nameValueBytes, 0, data, pos, nameValueBytes.Length);
                    pos += nameValueBytes.Length;
                }

                // Path PrimData
                data[pos++] = (byte)obj.Prim.PrimData.PathCurve;
                Utils.UInt16ToBytes(Primitive.PackBeginCut(obj.Prim.PrimData.PathBegin), data, pos); pos += 2;
                Utils.UInt16ToBytes(Primitive.PackEndCut(obj.Prim.PrimData.PathEnd), data, pos); pos += 2;
                data[pos++] = Primitive.PackPathScale(obj.Prim.PrimData.PathScaleX);
                data[pos++] = Primitive.PackPathScale(obj.Prim.PrimData.PathScaleY);
                data[pos++] = (byte)Primitive.PackPathShear(obj.Prim.PrimData.PathShearX);
                data[pos++] = (byte)Primitive.PackPathShear(obj.Prim.PrimData.PathShearY);
                data[pos++] = (byte)Primitive.PackPathTwist(obj.Prim.PrimData.PathTwist);
                data[pos++] = (byte)Primitive.PackPathTwist(obj.Prim.PrimData.PathTwistBegin);
                data[pos++] = (byte)Primitive.PackPathTwist(obj.Prim.PrimData.PathRadiusOffset);
                data[pos++] = (byte)Primitive.PackPathTaper(obj.Prim.PrimData.PathTaperX);
                data[pos++] = (byte)Primitive.PackPathTaper(obj.Prim.PrimData.PathTaperY);
                data[pos++] = Primitive.PackPathRevolutions(obj.Prim.PrimData.PathRevolutions);
                data[pos++] = (byte)Primitive.PackPathTwist(obj.Prim.PrimData.PathSkew);
                // Profile PrimData
                data[pos++] = obj.Prim.PrimData.profileCurve;
                Utils.UInt16ToBytes(Primitive.PackBeginCut(obj.Prim.PrimData.ProfileBegin), data, pos); pos += 2;
                Utils.UInt16ToBytes(Primitive.PackEndCut(obj.Prim.PrimData.ProfileEnd), data, pos); pos += 2;
                Utils.UInt16ToBytes(Primitive.PackProfileHollow(obj.Prim.PrimData.ProfileHollow), data, pos); pos += 2;

                // Texture Length
                Utils.UIntToBytes((uint)textureBytes.Length, data, pos);
                pos += 4;
                // Texture Entry
                Buffer.BlockCopy(textureBytes, 0, data, pos, textureBytes.Length);
                pos += textureBytes.Length;

                if ((flags & CompressedFlags.TextureAnimation) != 0)
                {
                    Utils.UIntToBytes((uint)textureAnimBytes.Length, data, pos);
                    pos += 4;
                    Buffer.BlockCopy(textureAnimBytes, 0, data, pos, textureAnimBytes.Length);
                    pos += textureAnimBytes.Length;
                }

                #endregion Packet serialization

                #region Packet sending

                //Logger.DebugLog("Sending ObjectUpdateCompressed with " + flags.ToString());

                if (sceneAgents.ContainsKey(obj.Prim.OwnerID))
                {
                    // Send an update out to the creator
                    ObjectUpdateCompressedPacket updateToOwner = new ObjectUpdateCompressedPacket();
                    updateToOwner.RegionData.RegionHandle = regionHandle;
                    updateToOwner.RegionData.TimeDilation = (ushort)(server.Physics.TimeDilation * (float)UInt16.MaxValue);
                    updateToOwner.ObjectData = new ObjectUpdateCompressedPacket.ObjectDataBlock[1];
                    updateToOwner.ObjectData[0] = new ObjectUpdateCompressedPacket.ObjectDataBlock();
                    updateToOwner.ObjectData[0].UpdateFlags = (uint)(obj.Prim.Flags | creatorFlags | PrimFlags.ObjectYouOwner);
                    updateToOwner.ObjectData[0].Data = data;

                    server.UDP.SendPacket(obj.Prim.OwnerID, updateToOwner, PacketCategory.State);
                }

                // Send an update out to everyone else
                ObjectUpdateCompressedPacket updateToOthers = new ObjectUpdateCompressedPacket();
                updateToOthers.RegionData.RegionHandle = regionHandle;
                updateToOthers.RegionData.TimeDilation = UInt16.MaxValue;
                updateToOthers.ObjectData = new ObjectUpdateCompressedPacket.ObjectDataBlock[1];
                updateToOthers.ObjectData[0] = new ObjectUpdateCompressedPacket.ObjectDataBlock();
                updateToOthers.ObjectData[0].UpdateFlags = (uint)obj.Prim.Flags;
                updateToOthers.ObjectData[0].Data = data;

                server.Scene.ForEachAgent(
                    delegate(Agent recipient)
                    {
                        if (recipient.ID != obj.Prim.OwnerID)
                            server.UDP.SendPacket(recipient.ID, updateToOthers, PacketCategory.State);
                    }
                );

                #endregion Packet sending

                #endregion ObjectUpdateCompressed
            }
            else
            {
                #region ImprovedTerseObjectUpdate

                Logger.DebugLog("Sending ImprovedTerseObjectUpdate");

                int pos = 0;
                byte[] data = new byte[(obj.Prim is Avatar ? 60 : 44)];

                // LocalID
                Utils.UIntToBytes(obj.Prim.LocalID, data, pos);
                pos += 4;
                // Avatar/CollisionPlane
                data[pos++] = obj.Prim.PrimData.State;
                if (obj.Prim is Avatar)
                {
                    data[pos++] = 1;
                    obj.Prim.CollisionPlane.ToBytes(data, pos);
                    pos += 16;
                }
                else
                {
                    ++pos;
                }
                // Position
                obj.Prim.Position.ToBytes(data, pos);
                pos += 12;

                // Velocity
                Utils.UInt16ToBytes(Utils.FloatToUInt16(obj.Prim.Velocity.X, -128.0f, 128.0f), data, pos); pos += 2;
                Utils.UInt16ToBytes(Utils.FloatToUInt16(obj.Prim.Velocity.Y, -128.0f, 128.0f), data, pos); pos += 2;
                Utils.UInt16ToBytes(Utils.FloatToUInt16(obj.Prim.Velocity.Z, -128.0f, 128.0f), data, pos); pos += 2;
                // Acceleration
                Utils.UInt16ToBytes(Utils.FloatToUInt16(obj.Prim.Acceleration.X, -64.0f, 64.0f), data, pos); pos += 2;
                Utils.UInt16ToBytes(Utils.FloatToUInt16(obj.Prim.Acceleration.Y, -64.0f, 64.0f), data, pos); pos += 2;
                Utils.UInt16ToBytes(Utils.FloatToUInt16(obj.Prim.Acceleration.Z, -64.0f, 64.0f), data, pos); pos += 2;
                // Rotation
                Utils.UInt16ToBytes(Utils.FloatToUInt16(obj.Prim.Rotation.X, -1.0f, 1.0f), data, pos); pos += 2;
                Utils.UInt16ToBytes(Utils.FloatToUInt16(obj.Prim.Rotation.Y, -1.0f, 1.0f), data, pos); pos += 2;
                Utils.UInt16ToBytes(Utils.FloatToUInt16(obj.Prim.Rotation.Z, -1.0f, 1.0f), data, pos); pos += 2;
                Utils.UInt16ToBytes(Utils.FloatToUInt16(obj.Prim.Rotation.W, -1.0f, 1.0f), data, pos); pos += 2;
                // Angular Velocity
                Utils.UInt16ToBytes(Utils.FloatToUInt16(obj.Prim.AngularVelocity.X, -64.0f, 64.0f), data, pos); pos += 2;
                Utils.UInt16ToBytes(Utils.FloatToUInt16(obj.Prim.AngularVelocity.Y, -64.0f, 64.0f), data, pos); pos += 2;
                Utils.UInt16ToBytes(Utils.FloatToUInt16(obj.Prim.AngularVelocity.Z, -64.0f, 64.0f), data, pos); pos += 2;

                ImprovedTerseObjectUpdatePacket update = new ImprovedTerseObjectUpdatePacket();
                update.RegionData.RegionHandle = RegionHandle;
                update.RegionData.TimeDilation = (ushort)(server.Physics.TimeDilation * (float)UInt16.MaxValue);
                update.ObjectData = new ImprovedTerseObjectUpdatePacket.ObjectDataBlock[1];
                update.ObjectData[0] = new ImprovedTerseObjectUpdatePacket.ObjectDataBlock();
                update.ObjectData[0].Data = data;

                if ((updateFlags & UpdateFlags.Textures) != 0)
                {
                    byte[] textureBytes = obj.Prim.Textures.GetBytes();
                    byte[] textureEntry = new byte[textureBytes.Length + 4];

                    // Texture Length
                    Utils.IntToBytes(textureBytes.Length, textureEntry, 0);
                    // Texture
                    Buffer.BlockCopy(textureBytes, 0, textureEntry, 4, textureBytes.Length);

                    update.ObjectData[0].TextureEntry = textureEntry;
                }
                else
                {
                    update.ObjectData[0].TextureEntry = Utils.EmptyBytes;
                }

                server.UDP.BroadcastPacket(update, PacketCategory.State);

                #endregion ImprovedTerseObjectUpdate
            }
        }

        static AvatarAppearancePacket BuildAppearancePacket(Agent agent)
        {
            AvatarAppearancePacket appearance = new AvatarAppearancePacket();
            appearance.ObjectData.TextureEntry = agent.Avatar.Prim.Textures.GetBytes();
            appearance.Sender.ID = agent.ID;
            appearance.Sender.IsTrial = false;

            appearance.VisualParam = new AvatarAppearancePacket.VisualParamBlock[agent.VisualParams.Length];
            for (int i = 0; i < agent.VisualParams.Length; i++)
            {
                appearance.VisualParam[i] = new AvatarAppearancePacket.VisualParamBlock();
                appearance.VisualParam[i].ParamValue = agent.VisualParams[i];
            }

            if (agent.VisualParams.Length != 218)
                Logger.Log("Built an appearance packet with VisualParams.Length=" + agent.VisualParams.Length,
                    Helpers.LogLevel.Warning);

            return appearance;
        }
    }
}
