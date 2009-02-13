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
        DoubleDictionary<uint, UUID, SimulationObject> sceneObjects = new DoubleDictionary<uint, UUID, SimulationObject>();
        DoubleDictionary<uint, UUID, Agent> sceneAgents = new DoubleDictionary<uint, UUID, Agent>();
        Dictionary<UUID, EventQueueServerCap> eventQueues = new Dictionary<UUID, EventQueueServerCap>();
        int currentLocalID = 1;
        ulong regionHandle;
        UUID regionID = UUID.Random();
        TerrainPatch[,] heightmap = new TerrainPatch[16, 16];

        public event ObjectAddCallback OnObjectAdd;
        public event ObjectRemoveCallback OnObjectRemove;
        public event ObjectTransformCallback OnObjectTransform;
        public event ObjectFlagsCallback OnObjectFlags;
        public event ObjectModifyCallback OnObjectModify;
        public event ObjectModifyTexturesCallback OnObjectModifyTextures;
        public event ObjectAnimateCallback OnObjectAnimate;
        public event AgentAddCallback OnAgentAdd;
        public event AgentRemoveCallback OnAgentRemove;
        public event AgentAppearanceCallback OnAgentAppearance;
        public event TriggerSoundCallback OnTriggerSound;
        public event TriggerEffectsCallback OnTriggerEffects;
        public event TerrainUpdateCallback OnTerrainUpdate;

        public uint RegionX { get { return 7777; } }
        public uint RegionY { get { return 7777; } }
        public ulong RegionHandle { get { return regionHandle; } }
        public UUID RegionID { get { return regionID; } }
        public string RegionName { get { return "Simian"; } }
        public RegionFlags RegionFlags { get { return RegionFlags.None; } }

        public float WaterHeight { get { return 20f; } }

        public uint TerrainPatchWidth { get { return 16; } }
        public uint TerrainPatchHeight { get { return 16; } }

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
            lock (eventQueues)
            {
                foreach (EventQueueServerCap eventQueue in eventQueues.Values)
                {
                    server.Capabilities.RemoveCapability(eventQueue.Capability);
                    eventQueue.Server.Stop();
                }
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

        public bool ObjectAdd(object sender, SimulationObject obj, PrimFlags creatorFlags)
        {
            // Check if the object already exists in the scene
            if (sceneObjects.ContainsKey(obj.Prim.ID))
                sceneObjects.Remove(obj.Prim.LocalID, obj.Prim.ID);

            if (obj.Prim.LocalID == 0)
            {
                // Assign a unique LocalID to this object
                obj.Prim.LocalID = (uint)Interlocked.Increment(ref currentLocalID);
            }

            if (OnObjectAdd != null)
                OnObjectAdd(sender, obj, creatorFlags);

            // Add the object to the scene dictionary
            sceneObjects.Add(obj.Prim.LocalID, obj.Prim.ID, obj);

            if (sceneAgents.ContainsKey(obj.Prim.OwnerID))
            {
                // Send an update out to the creator
                ObjectUpdatePacket updateToOwner = SimulationObject.BuildFullUpdate(obj.Prim, regionHandle,
                    obj.Prim.Flags | creatorFlags);
                server.UDP.SendPacket(obj.Prim.OwnerID, updateToOwner, PacketCategory.State);
            }

            // Send an update out to everyone else
            ObjectUpdatePacket updateToOthers = SimulationObject.BuildFullUpdate(obj.Prim, regionHandle,
                obj.Prim.Flags);
            server.Scene.ForEachAgent(
                delegate(Agent recipient)
                {
                    if (recipient.Avatar.ID != obj.Prim.OwnerID)
                        server.UDP.SendPacket(recipient.Avatar.ID, updateToOthers, PacketCategory.State);
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
            else if (sceneAgents.TryGetValue(localID, out agent))
            {
                AgentRemove(sender, agent);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ObjectRemove(object sender, UUID id)
        {
            SimulationObject obj;
            Agent agent;

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
            else if (sceneAgents.TryGetValue(id, out agent))
            {
                AgentRemove(sender, agent);
                return true;
            }
            else
            {
                return false;
            }
        }

        void AgentRemove(object sender, Agent agent)
        {
            if (OnAgentRemove != null)
                OnAgentRemove(sender, agent);

            Logger.Log("Removing agent " + agent.FullName + " from the scene", Helpers.LogLevel.Info);

            sceneAgents.Remove(agent.Avatar.LocalID, agent.Avatar.ID);

            KillObjectPacket kill = new KillObjectPacket();
            kill.ObjectData = new KillObjectPacket.ObjectDataBlock[1];
            kill.ObjectData[0] = new KillObjectPacket.ObjectDataBlock();
            kill.ObjectData[0].ID = agent.Avatar.LocalID;

            server.UDP.BroadcastPacket(kill, PacketCategory.State);

            // Kill the EventQueue
            RemoveEventQueue(agent.Avatar.ID);

            // Remove the UDP client
            server.UDP.RemoveClient(agent);

            // Notify everyone in the scene that this agent has gone offline
            OfflineNotificationPacket offline = new OfflineNotificationPacket();
            offline.AgentBlock = new OfflineNotificationPacket.AgentBlockBlock[1];
            offline.AgentBlock[0] = new OfflineNotificationPacket.AgentBlockBlock();
            offline.AgentBlock[0].AgentID = agent.Avatar.ID;
            server.UDP.BroadcastPacket(offline, PacketCategory.State);
        }

        public void ObjectTransform(object sender, uint localID, Vector3 position, Quaternion rotation,
            Vector3 velocity, Vector3 acceleration, Vector3 angularVelocity)
        {
            SimulationObject obj;
            Agent agent;

            if (sceneObjects.TryGetValue(localID, out obj))
            {
                if (OnObjectTransform != null)
                {
                    OnObjectTransform(sender, obj, position, rotation, velocity,
                        acceleration, angularVelocity);
                }

                // Update the object
                obj.Prim.Position = position;
                obj.Prim.Rotation = rotation;
                obj.Prim.Velocity = velocity;
                obj.Prim.Acceleration = acceleration;
                obj.Prim.AngularVelocity = angularVelocity;

                // Inform clients
                BroadcastObjectUpdate(obj.Prim);
            }
            else if (sceneAgents.TryGetValue(localID, out agent))
            {
                if (OnObjectTransform != null)
                {
                    OnObjectTransform(sender, obj, position, rotation, velocity,
                        acceleration, angularVelocity);
                }

                // Update the avatar
                agent.Avatar.Position = position;
                agent.Avatar.Rotation = rotation;
                agent.Avatar.Velocity = velocity;
                agent.Avatar.Acceleration = acceleration;
                agent.Avatar.AngularVelocity = angularVelocity;

                // Inform clients
                BroadcastObjectUpdate(agent.Avatar);
            }
        }

        public void ObjectFlags(object sender, SimulationObject obj, PrimFlags flags)
        {
            if (OnObjectFlags != null)
            {
                OnObjectFlags(sender, obj, flags);
            }

            // Update the object
            obj.Prim.Flags = flags;

            // Inform clients
            BroadcastObjectUpdate(obj.Prim);
        }

        public void ObjectModify(object sender, uint localID, Primitive.ConstructionData data)
        {
            SimulationObject obj;
            if (sceneObjects.TryGetValue(localID, out obj))
            {
                if (OnObjectModify != null)
                {
                    OnObjectModify(sender, obj, data);
                }

                // Update the object
                obj.Prim.PrimData = data;

                // Inform clients
                BroadcastObjectUpdate(obj.Prim);
            }
        }

        public void ObjectModifyTextures(object sender, SimulationObject obj, string mediaURL, Primitive.TextureEntry textureEntry)
        {
            if (OnObjectModifyTextures != null)
            {
                OnObjectModifyTextures(sender, obj, mediaURL, textureEntry);
            }

            // Update the object
            obj.Prim.Textures = textureEntry;
            obj.Prim.MediaURL = mediaURL;

            // Inform clients
            BroadcastObjectUpdate(obj.Prim);
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
            }

            server.UDP.BroadcastPacket(effect, PacketCategory.State);
        }

        public bool ContainsObject(uint localID)
        {
            return sceneObjects.ContainsKey(localID) || sceneAgents.ContainsKey(localID);
        }

        public bool ContainsObject(UUID id)
        {
            return sceneObjects.ContainsKey(id) || sceneAgents.ContainsKey(id);
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
            if (sceneAgents.ContainsKey(agent.Avatar.ID))
                sceneAgents.Remove(agent.Avatar.LocalID, agent.Avatar.ID);

            if (agent.Avatar.LocalID == 0)
            {
                // Assign a unique LocalID to this agent
                agent.Avatar.LocalID = (uint)Interlocked.Increment(ref currentLocalID);
            }

            if (OnAgentAdd != null)
                OnAgentAdd(sender, agent, creatorFlags);

            // Add the agent to the scene dictionary
            sceneAgents.Add(agent.Avatar.LocalID, agent.Avatar.ID, agent);

            // Send an update out to the agent
            ObjectUpdatePacket updateToOwner = SimulationObject.BuildFullUpdate(agent.Avatar, regionHandle,
                agent.Avatar.Flags | creatorFlags);
            server.UDP.SendPacket(agent.Avatar.ID, updateToOwner, PacketCategory.State);

            // Send an update out to everyone else
            ObjectUpdatePacket updateToOthers = SimulationObject.BuildFullUpdate(agent.Avatar, regionHandle,
                agent.Avatar.Flags);
            server.Scene.ForEachAgent(
                delegate(Agent recipient)
                {
                    if (recipient.Avatar.ID != agent.Avatar.ID)
                        server.UDP.SendPacket(recipient.Avatar.ID, updateToOthers, PacketCategory.State);
                }
            );

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
            ObjectUpdatePacket update = SimulationObject.BuildFullUpdate(agent.Avatar,
                regionHandle, agent.Flags);
            server.UDP.BroadcastPacket(update, PacketCategory.State);

            // Update the avatar
            agent.Avatar.Textures = textures;
            if (visualParams != null && visualParams.Length > 1)
                agent.VisualParams = visualParams;

            if (agent.VisualParams != null)
            {
                // Send the appearance packet to all other clients
                AvatarAppearancePacket appearance = BuildAppearancePacket(agent);
                sceneAgents.ForEach(
                    delegate(Agent recipient)
                    {
                        if (recipient != agent)
                            server.UDP.SendPacket(recipient.Avatar.ID, appearance, PacketCategory.State);
                    }
                );
            }
        }

        public void ForEachObject(Action<SimulationObject> action)
        {
            sceneObjects.ForEach(action);
        }

        public bool TryGetAgent(uint localID, out Agent agent)
        {
            return sceneAgents.TryGetValue(localID, out agent);
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
            sceneAgents.ForEach(action);
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

            eventQueues.Add(agentID, eqServerCap);
            
            return eqServerCap.Capability;
        }

        public bool RemoveEventQueue(UUID agentID)
        {
            return eventQueues.Remove(agentID);
        }

        public bool HasRunningEventQueue(Agent agent)
        {
            return eventQueues.ContainsKey(agent.Avatar.ID);
        }

        public void SendEvent(Agent agent, string name, OSDMap body)
        {
            EventQueueServerCap eventQueue;
            if (eventQueues.TryGetValue(agent.Avatar.ID, out eventQueue))
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

        void BroadcastObjectUpdate(Primitive prim)
        {
            ObjectUpdatePacket update =
                SimulationObject.BuildFullUpdate(prim, regionHandle, prim.Flags);

            server.UDP.BroadcastPacket(update, PacketCategory.State);
        }

        void CompleteAgentMovementHandler(Packet packet, Agent agent)
        {
            CompleteAgentMovementPacket request = (CompleteAgentMovementPacket)packet;

            // Create a representation for this agent
            Avatar avatar = new Avatar();
            avatar.ID = agent.Avatar.ID;
            avatar.LocalID = (uint)Interlocked.Increment(ref currentLocalID);
            avatar.Position = new Vector3(128f, 128f, 25f);
            avatar.Rotation = Quaternion.Identity;
            avatar.Scale = new Vector3(0.45f, 0.6f, 1.9f);
            avatar.PrimData.Material = Material.Flesh;
            avatar.PrimData.PCode = PCode.Avatar;

            // Create a default outfit for the avatar
            Primitive.TextureEntry te = new Primitive.TextureEntry(new UUID("c228d1cf-4b5d-4ba8-84f4-899a0796aa97"));
            avatar.Textures = te;

            // Set the avatar name
            NameValue[] name = new NameValue[2];
            name[0] = new NameValue("FirstName", NameValue.ValueType.String, NameValue.ClassType.ReadWrite,
                NameValue.SendtoType.SimViewer, agent.FirstName);
            name[1] = new NameValue("LastName", NameValue.ValueType.String, NameValue.ClassType.ReadWrite,
                NameValue.SendtoType.SimViewer, agent.LastName);
            avatar.NameValues = name;

            // Link this avatar up with the corresponding agent
            agent.Avatar = avatar;

            // Give testers a provisionary balance of 1000L
            agent.Balance = 1000;

            // Add this avatar as an object in the scene
            if (ObjectAdd(this, new SimulationObject(agent.Avatar, server), PrimFlags.None))
            {
                // Send a response back to the client
                AgentMovementCompletePacket complete = new AgentMovementCompletePacket();
                complete.AgentData.AgentID = agent.Avatar.ID;
                complete.AgentData.SessionID = agent.SessionID;
                complete.Data.LookAt = Vector3.UnitX;
                complete.Data.Position = avatar.Position;
                complete.Data.RegionHandle = regionHandle;
                complete.Data.Timestamp = Utils.DateTimeToUnixTime(DateTime.Now);
                complete.SimData.ChannelVersion = Utils.StringToBytes("Simian");

                server.UDP.SendPacket(agent.Avatar.ID, complete, PacketCategory.Transaction);

                // Send updates and appearances for every avatar to this new avatar
                SynchronizeStateTo(agent);

                //HACK: Notify everyone when someone logs on to the simulator
                OnlineNotificationPacket online = new OnlineNotificationPacket();
                online.AgentBlock = new OnlineNotificationPacket.AgentBlockBlock[1];
                online.AgentBlock[0] = new OnlineNotificationPacket.AgentBlockBlock();
                online.AgentBlock[0].AgentID = agent.Avatar.ID;
                server.UDP.BroadcastPacket(online, PacketCategory.State);
            }
            else
            {
                Logger.Log("Received a CompleteAgentMovement from an avatar already in the scene, " +
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
                    obj.Prim.RegionHandle, obj.Prim.Flags);
                server.UDP.SendPacket(agent.Avatar.ID, update, PacketCategory.State);
            });

            // Send appearances for all avatars
            sceneAgents.ForEach(
                delegate(Agent otherAgent)
                {
                    if (otherAgent != agent)
                    {
                        // Send appearances for this avatar
                        AvatarAppearancePacket appearance = BuildAppearancePacket(otherAgent);
                        server.UDP.SendPacket(agent.Avatar.ID, appearance, PacketCategory.State);
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
                    server.UDP.SendPacket(agent.Avatar.ID, layer, PacketCategory.Terrain);
                }
            }
        }

        static AvatarAppearancePacket BuildAppearancePacket(Agent agent)
        {
            AvatarAppearancePacket appearance = new AvatarAppearancePacket();
            appearance.ObjectData.TextureEntry = agent.Avatar.Textures.ToBytes();
            appearance.Sender.ID = agent.Avatar.ID;
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
