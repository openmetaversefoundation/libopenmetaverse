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

        public event ObjectAddCallback OnObjectAdd;
        public event ObjectRemoveCallback OnObjectRemove;
        public event ObjectTransformCallback OnObjectTransform;
        public event ObjectFlagsCallback OnObjectFlags;
        public event ObjectModifyCallback OnObjectModify;
        public event ObjectModifyTexturesCallback OnObjectModifyTextures;
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
            ForEachAgent(delegate(Agent agent) { AgentRemove(this, agent); });
        }

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

        public bool ObjectAddOrUpdate(object sender, SimulationObject obj, UUID ownerID, int scriptStartParam, PrimFlags creatorFlags)
        {
            if (OnObjectAdd != null)
            {
                OnObjectAdd(sender, obj, ownerID, scriptStartParam, creatorFlags);
            }

            // Check if the object already exists in the scene
            SimulationObject oldObj;
            if (sceneObjects.TryGetValue(obj.Prim.ID, out oldObj))
            {
                sceneObjects.Remove(oldObj.Prim.LocalID, oldObj.Prim.ID);

                // Point the new object at the old undo/redo queues
                obj.UndoSteps = oldObj.UndoSteps;
                obj.RedoSteps = oldObj.RedoSteps;
            }
            else
            {
                // Enable some default flags that all objects will have
                obj.Prim.Flags |= server.Permissions.GetDefaultObjectFlags();

                // Object did not exist before, so there's no way it could contain inventory
                obj.Prim.Flags |= PrimFlags.InventoryEmpty;

                // Fun Fact: Prim.OwnerID is only used by the LL viewer to mute sounds
                obj.Prim.OwnerID = ownerID;

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

                // Set default textures if none are set
                if (obj.Prim.Textures == null)
                    obj.Prim.Textures = new Primitive.TextureEntry(new UUID("89556747-24cb-43ed-920b-47caed15465f")); // Plywood
            }

            // Reset the prim CRC
            obj.CRC = 0;

            // Add the object to the scene dictionary
            sceneObjects.Add(obj.Prim.LocalID, obj.Prim.ID, obj);

            if (sceneAgents.ContainsKey(obj.Prim.OwnerID))
            {
                // Send an update out to the creator
                ObjectUpdatePacket updateToOwner = SimulationObject.BuildFullUpdate(obj.Prim, regionHandle,
                    obj.Prim.Flags | creatorFlags | PrimFlags.ObjectYouOwner, obj.CRC);
                server.UDP.SendPacket(obj.Prim.OwnerID, updateToOwner, PacketCategory.State);
            }

            // Send an update out to everyone else
            ObjectUpdatePacket updateToOthers = SimulationObject.BuildFullUpdate(obj.Prim, regionHandle,
                obj.Prim.Flags, obj.CRC);
            server.Scene.ForEachAgent(
                delegate(Agent recipient)
                {
                    if (recipient.ID != obj.Prim.OwnerID)
                        server.UDP.SendPacket(recipient.ID, updateToOthers, PacketCategory.State);
                }
            );

            return true;
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

            if (sceneAgents.TryGetValue(id, out agent))
                AgentRemove(sender, agent);

            if (sceneObjects.TryGetValue(id, out obj))
            {
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

        void AgentRemove(object sender, Agent agent)
        {
            if (OnAgentRemove != null)
                OnAgentRemove(sender, agent);

            Logger.Log("Removing agent " + agent.FullName + " from the scene", Helpers.LogLevel.Info);

            sceneAgents.Remove(agent.ID);

            KillObjectPacket kill = new KillObjectPacket();
            kill.ObjectData = new KillObjectPacket.ObjectDataBlock[1];
            kill.ObjectData[0] = new KillObjectPacket.ObjectDataBlock();
            kill.ObjectData[0].ID = agent.Avatar.Prim.LocalID;

            server.UDP.BroadcastPacket(kill, PacketCategory.State);

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

        public void ObjectTransform(object sender, SimulationObject obj, Vector3 position, Quaternion rotation,
            Vector3 velocity, Vector3 acceleration, Vector3 angularVelocity)
        {
            if (OnObjectTransform != null)
            {
                OnObjectTransform(sender, obj, position, rotation, velocity, acceleration, angularVelocity);
            }

            // Add an undo step for prims (not avatars)
            if (!(obj.Prim is Avatar))
                obj.CreateUndoStep();

            // Update the object
            obj.Prim.Position = position;
            obj.Prim.Rotation = rotation;
            obj.Prim.Velocity = velocity;
            obj.Prim.Acceleration = acceleration;
            obj.Prim.AngularVelocity = angularVelocity;

            // Reset the prim CRC
            obj.CRC = 0;

            // Inform clients
            BroadcastObjectUpdate(obj.Prim);
        }

        public void ObjectFlags(object sender, SimulationObject obj, PrimFlags flags)
        {
            if (OnObjectFlags != null)
            {
                OnObjectFlags(sender, obj, flags);
            }

            // Add an undo step for prims (not avatars)
            if (!(obj.Prim is Avatar))
                obj.CreateUndoStep();

            // Update the object
            obj.Prim.Flags = flags;

            // Reset the prim CRC
            obj.CRC = 0;

            // Inform clients
            BroadcastObjectUpdate(obj.Prim);
        }

        public void ObjectModify(object sender, SimulationObject obj, Primitive.ConstructionData data)
        {
            if (OnObjectModify != null)
            {
                OnObjectModify(sender, obj, data);
            }

            // Add an undo step for prims (not avatars)
            if (!(obj.Prim is Avatar))
                obj.CreateUndoStep();

            // Update the object
            obj.Prim.PrimData = data;

            // Reset the prim CRC
            obj.CRC = 0;

            // Inform clients
            BroadcastObjectUpdate(obj.Prim);
        }

        public void ObjectModifyTextures(object sender, SimulationObject obj, string mediaURL, Primitive.TextureEntry textureEntry)
        {
            if (OnObjectModifyTextures != null)
            {
                OnObjectModifyTextures(sender, obj, mediaURL, textureEntry);
            }

            // Add an undo step for prims (not avatars)
            if (!(obj.Prim is Avatar))
                obj.CreateUndoStep();

            // Update the object
            obj.Prim.Textures = textureEntry;
            obj.Prim.MediaURL = mediaURL;

            // Reset the prim CRC
            obj.CRC = 0;

            // Inform clients
            BroadcastObjectUpdate(obj.Prim);
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
                BroadcastObjectUpdate(obj.Prim);
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
                BroadcastObjectUpdate(obj.Prim);
            }
            else
            {
                Logger.Log(String.Format("Redo requested on object {0} with no remaining redo steps", obj.Prim.ID),
                    Helpers.LogLevel.Debug);
            }
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

        public bool AgentAdd(object sender, Agent agent, PrimFlags creatorFlags)
        {
            // Check if the agent already exists in the scene
            if (sceneAgents.ContainsKey(agent.ID))
                sceneAgents.Remove(agent.ID);

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
            sceneAgents[agent.ID] = agent;

            // Send out an update to everyone
            //ObjectAdd(this, agent.Avatar, agent.Avatar.Prim.OwnerID, 0, PrimFlags.None);

            return true;
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

        public void ForEachObject(Action<SimulationObject> action)
        {
            sceneObjects.ForEach(action);
        }

        public SimulationObject FindObject(Predicate<SimulationObject> predicate)
        {
            return sceneObjects.FindValue(predicate);
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

        bool EventQueueHandler(IHttpClientContext context, IHttpRequest request, IHttpResponse response, object state)
        {
            EventQueueServer eqServer = (EventQueueServer)state;
            return eqServer.EventQueueHandler(context, request, response);
        }

        // FIXME: This function needs to go away as soon as we stop sending full object updates for everything
        void BroadcastObjectUpdate(Primitive prim)
        {
            SimulationObject obj;
            if (TryGetObject(prim.ID, out obj))
                ObjectAddOrUpdate(this, obj, obj.Prim.OwnerID, 0, PrimFlags.None);
        }

        void CompleteAgentMovementHandler(Packet packet, Agent agent)
        {
            // Add this avatar as an object in the scene
            if (ObjectAddOrUpdate(this, agent.Avatar, agent.Avatar.Prim.OwnerID, 0, PrimFlags.None))
            {
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
            else
            {
                Logger.Log("Received a CompleteAgentMovement but failed to insert avatar into the scene: " +
                    agent.FullName, Helpers.LogLevel.Warning);
            }
        }

        // HACK: The reduction provider will deprecate this at some point
        void SynchronizeStateTo(Agent agent)
        {
            // Send the parcel overlay
            server.Parcels.SendParcelOverlay(agent);

            // Send object updates for objects and avatars
            sceneObjects.ForEach(delegate(SimulationObject obj)
            {
                ObjectUpdatePacket update = SimulationObject.BuildFullUpdate(obj.Prim,
                    obj.Prim.RegionHandle, obj.Prim.Flags, obj.CRC);
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

        static AvatarAppearancePacket BuildAppearancePacket(Agent agent)
        {
            AvatarAppearancePacket appearance = new AvatarAppearancePacket();
            appearance.ObjectData.TextureEntry = agent.Avatar.Prim.Textures.ToBytes();
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
