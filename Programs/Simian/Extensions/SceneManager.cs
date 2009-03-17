using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using ExtensionLoader;
using HttpServer;
using OpenMetaverse;
using OpenMetaverse.Imaging;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Http;

namespace Simian
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

    public class SceneManager : ISceneProvider
    {
        const int TARGET_FRAMES_PER_SECOND = 45;

        // Interfaces. Although no other classes will access these interfaces directly
        // (getters are used instead), they must be marked public so ExtensionLoader 
        // can automatically assign them
        public IAvatarProvider avatars;
        public IParcelProvider parcels;
        public IPhysicsProvider physics;
        public IScriptEngine scriptEngine;
        public ITaskInventoryProvider taskInventory;
        public IUDPProvider udp;

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

        public Simian Server { get { return server; } }
        public IAvatarProvider Avatars { get { return avatars; } }
        public IParcelProvider Parcels { get { return parcels; } }
        public IPhysicsProvider Physics { get { return physics; } }
        public IScriptEngine ScriptEngine { get { return scriptEngine; } }
        public ITaskInventoryProvider TaskInventory { get { return taskInventory; } }
        public IUDPProvider UDP { get { return udp; } }

        public X509Certificate2 RegionCertificate { get { return regionCert; } }
        public uint RegionX
        {
            get { return regionX; }
            set
            {
                regionX = value;
                regionHandle = Utils.UIntsToLong(regionX * 256, regionY * 256);
            }
        }
        public uint RegionY
        {
            get { return regionY; }
            set
            {
                regionY = value;
                regionHandle = Utils.UIntsToLong(regionX * 256, regionY * 256);
            }
        }
        public ulong RegionHandle { get { return regionHandle; } }
        public UUID RegionID { get { return regionID; } }
        public string RegionName { get { return regionName; } set { regionName = value; } }
        public RegionFlags RegionFlags { get { return regionFlags; } }
        public IPEndPoint IPAndPort { get { return endpoint; } set { endpoint = value; } }
        public Vector3 DefaultPosition { get { return defaultPosition; } }
        public Vector3 DefaultLookAt { get { return defaultLookAt; } }
        public UUID MapTextureID { get { return mapTextureID; } }
        public float WaterHeight { get { return waterHeight; } }
        public uint TerrainPatchWidth { get { return 16; } }
        public uint TerrainPatchHeight { get { return 16; } }
        public uint TerrainPatchCountWidth { get { return 16; } }
        public uint TerrainPatchCountHeight { get { return 16; } }
        public float TimeDilation { get { return TimeDilation; } }

        Simian server;
        // Contains all scene objects, including prims and avatars
        DoubleDictionary<uint, UUID, SimulationObject> sceneObjects = new DoubleDictionary<uint, UUID, SimulationObject>();
        // A duplicate of the avatar information stored in sceneObjects, improves operations such as iterating over all agents
        Dictionary<UUID, Agent> sceneAgents = new Dictionary<UUID, Agent>();
        // Event queues for each avatar in the scene
        Dictionary<UUID, EventQueueServerCap> eventQueues = new Dictionary<UUID, EventQueueServerCap>();
        int currentLocalID = 1;
        X509Certificate2 regionCert;
        ulong regionHandle;
        RegionFlags regionFlags;
        UUID regionID = UUID.Random();
        TerrainPatch[,] heightmap = new TerrainPatch[16, 16];
        Vector2[,] windSpeeds = new Vector2[16, 16];
        ExtensionLoader<ISceneProvider> extensions = new ExtensionLoader<ISceneProvider>();
        IPEndPoint endpoint;
        uint regionX;
        uint regionY;
        string regionName;
        float waterHeight;
        UUID mapTextureID;
        Vector3 defaultPosition = new Vector3(128f, 128f, 30f);
        Vector3 defaultLookAt = Vector3.UnitZ;
        /// <summary>Track the eight neighboring tiles around us</summary>
        RegionInfo[] neighbors = new RegionInfo[8];
        /// <summary>List of callback URIs for pending client connections. When a new client connection
        /// is established for a client in this dictionary, an enable_client_complete message will be
        /// sent to the associated URI</summary>
        Dictionary<UUID, Uri> enableClientCompleteCallbacks = new Dictionary<UUID, Uri>();
        float timeDilation;
        bool running;

        public SceneManager()
        {
        }

        public bool Start(Simian server, RegionInfo regionInfo, X509Certificate2 regionCert, string defaultTerrainFile, int staticObjects, int physicalObjects)
        {
            running = true;

            this.server = server;
            this.regionName = regionInfo.Name;
            this.endpoint = regionInfo.IPAndPort;
            this.regionID = regionInfo.ID;
            this.regionCert = regionCert;
            this.regionFlags = regionInfo.Flags;
            this.mapTextureID = regionInfo.MapTextureID;
            this.waterHeight = regionInfo.WaterHeight;

            // Set the properties because this will automatically update the regionHandle
            RegionX = regionInfo.X;
            RegionY = regionInfo.Y;

            #region ISceneProvider Extension Loading

            try
            {
                // Create a list of references for .cs extensions that are compiled at runtime
                List<string> references = new List<string>();
                references.Add("OpenMetaverseTypes.dll");
                references.Add("OpenMetaverse.dll");
                references.Add("Simian.exe");

                // Load extensions from the current executing assembly, Simian.*.dll assemblies on disk, and
                // Simian.*.cs source files on disk.
                extensions.LoadAllExtensions(Assembly.GetExecutingAssembly(),
                    AppDomain.CurrentDomain.BaseDirectory, server.ExtensionList, references,
                    "Simian.*.dll", "Simian.*.cs");

                // Automatically assign extensions that implement interfaces to the list of interface
                // variables in "assignables"
                extensions.AssignExtensions(this, extensions.GetInterfaces(this));

                // Start all of the extensions
                foreach (IExtension<ISceneProvider> extension in extensions.Extensions)
                {
                    // Only print the extension names if this is the first loaded scene
                    if (server.Scenes.Count == 0)
                        Logger.Log("Starting Scene extension " + extension.GetType().Name, Helpers.LogLevel.Info);

                    extension.Start(this);
                }
            }
            catch (ExtensionException ex)
            {
                Logger.Log("SceneManager extension loading failed, shutting down: " + ex.Message, Helpers.LogLevel.Error);
                Stop();
                return false;
            }

            #endregion ISceneProvider Extension Loading

            // Callback registration
            server.Grid.OnRegionUpdate += Grid_OnRegionUpdate;
            udp.OnAgentConnection += udp_OnAgentConnection;
            udp.RegisterPacketCallback(PacketType.CompleteAgentMovement, CompleteAgentMovementHandler);
            
            // Load the default terrain for this sim
            if (!String.IsNullOrEmpty(defaultTerrainFile))
                LoadTerrain(Simian.DATA_DIR + defaultTerrainFile);

            // Start the physics thread
            Thread physicsThread = new Thread(new ThreadStart(PhysicsThread));
            physicsThread.Name = "Physics";
            physicsThread.Start();

            Logger.Log(String.Format("Region {0} online at ({1},{2}) listening on {3}", regionName, regionX, regionY, endpoint),
                Helpers.LogLevel.Info);

            // Tell the grid that this region is online
            regionInfo.Online = true;
            server.Grid.RegionUpdate(regionInfo, regionCert);

            return true;
        }

        public void Stop()
        {
            Logger.Log("Stopping region " + regionName, Helpers.LogLevel.Info);

            running = false;

            // Remove all of the agents from the scene. This will shutdown UDP connections and event queues to
            // each of the agents as well
            lock (sceneAgents)
            {
                List<Agent> agents = new List<Agent>(sceneAgents.Values);
                for (int i = 0; i < agents.Count; i++)
                    ObjectRemove(this, agents[i].ID);
            }

            // Stop ISceneProvider extensions
            foreach (IExtension<ISceneProvider> extension in extensions.Extensions)
            {
                Logger.Log("Stopping Scene extension " + extension.GetType().Name, Helpers.LogLevel.Info);
                extension.Stop();
            }

            Logger.Log("Region " + regionName + " is stopped", Helpers.LogLevel.Info);
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
                    obj.Prim.RegionHandle = regionHandle;

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

                udp.BroadcastPacket(kill, PacketCategory.State);
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

                udp.BroadcastPacket(kill, PacketCategory.State);
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

            udp.BroadcastPacket(sendAnim, PacketCategory.State);
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

                udp.BroadcastPacket(chat, PacketCategory.Messaging);
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
                ObjectAddOrUpdate(this, obj, obj.Prim.OwnerID, 0, PrimFlags.None, UpdateFlags.FullUpdate);
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
                ObjectAddOrUpdate(this, obj, obj.Prim.OwnerID, 0, PrimFlags.None, UpdateFlags.FullUpdate);
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
            sound.SoundData.Handle = regionHandle;
            sound.SoundData.ObjectID = objectID;
            sound.SoundData.ParentID = parentID;
            sound.SoundData.OwnerID = ownerID;
            sound.SoundData.Position = position;
            sound.SoundData.SoundID = soundID;
            sound.SoundData.Gain = gain;

            udp.BroadcastPacket(sound, PacketCategory.State);
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

            udp.BroadcastPacket(effect, PacketCategory.State);
        }

        #endregion Object Interfaces

        #region Agent Interfaces

        public bool AgentAdd(object sender, Agent agent, PrimFlags creatorFlags)
        {
            // Sanity check, since this should have already been done
            agent.Avatar.Prim.ID = agent.Info.ID;

            // Check if the agent already exists in the scene
            lock (sceneAgents)
            {
                if (sceneAgents.ContainsKey(agent.ID))
                    sceneAgents.Remove(agent.ID);
            }

            // Update the current region handle
            agent.Avatar.Prim.RegionHandle = regionHandle;

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
                NameValue.SendtoType.SimViewer, agent.Info.FirstName);
            name[1] = new NameValue("LastName", NameValue.ValueType.String, NameValue.ClassType.ReadWrite,
                NameValue.SendtoType.SimViewer, agent.Info.LastName);
            agent.Avatar.Prim.NameValues = name;

            // Give testers a provisionary balance of 1000L
            agent.Info.Balance = 1000;

            // Some default avatar prim properties
            agent.Avatar.Prim.Properties = new Primitive.ObjectProperties();
            agent.Avatar.Prim.Properties.CreationDate = Utils.UnixTimeToDateTime(agent.Info.CreationTime);
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
            udp.RemoveClient(agent);

            // Notify everyone in the scene that this agent has gone offline
            OfflineNotificationPacket offline = new OfflineNotificationPacket();
            offline.AgentBlock = new OfflineNotificationPacket.AgentBlockBlock[1];
            offline.AgentBlock[0] = new OfflineNotificationPacket.AgentBlockBlock();
            offline.AgentBlock[0].AgentID = agent.ID;
            udp.BroadcastPacket(offline, PacketCategory.State);
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
            //scene.UDP.BroadcastPacket(update, PacketCategory.State);

            // Update the avatar
            agent.Avatar.Prim.Textures = textures;
            if (visualParams != null && visualParams.Length > 1)
                agent.Info.VisualParams = visualParams;

            if (agent.Info.VisualParams != null)
            {
                // Send the appearance packet to all other clients
                AvatarAppearancePacket appearance = agent.BuildAppearancePacket();
                ForEachAgent(
                    delegate(Agent recipient)
                    {
                        if (recipient != agent)
                            udp.SendPacket(recipient.ID, appearance, PacketCategory.State);
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
            udp.BroadcastPacket(layer, PacketCategory.Terrain);
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

        #endregion Capabilities Interfaces

        public void InformClientOfNeighbors(Agent agent)
        {
            for (int i = 0; i < 8; i++)
            {
                if (!agent.NeighborConnections[i] && neighbors[i].Online)
                {
                    Logger.Log("Sending enable_client for " + agent.FullName + " to neighbor " + neighbors[i].Name, Helpers.LogLevel.Info);

                    // Create a callback for enable_client_complete
                    Uri callbackUri = server.Capabilities.CreateCapability(EnableClientCompleteCapHandler, false, null);

                    OSDMap enableClient = CapsMessages.EnableClient(agent.ID, agent.SessionID, agent.SecureSessionID,
                        (int)agent.CircuitCode, agent.Info.FirstName, agent.Info.LastName, callbackUri);

                    AutoResetEvent waitEvent = new AutoResetEvent(false);

                    CapsClient request = new CapsClient(neighbors[i].EnableClientCap);
                    request.OnComplete +=
                    delegate(CapsClient client, OSD result, Exception error)
                    {
                        OSDMap response = result as OSDMap;
                        if (response != null)
                        {
                            bool success = response["success"].AsBoolean();
                            Logger.Log("enable_client response: " + success, Helpers.LogLevel.Info);

                            if (success)
                            {
                                // Send the EnableSimulator capability to clients
                                RegionInfo neighbor = neighbors[i];
                                OSDMap enableSimulator = CapsMessages.EnableSimulator(neighbor.Handle, neighbor.IPAndPort.Address, neighbor.IPAndPort.Port);

                                SendEvent(agent, "EnableSimulator", enableSimulator);
                            }
                        }
                        waitEvent.Set();
                    };
                    request.StartRequest(enableClient);

                    if (!waitEvent.WaitOne(30 * 1000, false))
                        Logger.Log("enable_client request timed out", Helpers.LogLevel.Warning);
                }
            }
        }

        #region Callback Handlers

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

        public bool EnableClientCapHandler(IHttpClientContext context, IHttpRequest request, IHttpResponse response, object state)
        {
            OSDMap map = OSDParser.DeserializeLLSDXml(request.Body) as OSDMap;
            OSDMap osdResponse = new OSDMap();

            if (map != null)
            {
                UUID agentID = map["agent_id"].AsUUID();
                UUID sessionID = map["session_id"].AsUUID();
                UUID secureSessionID = map["secure_session_id"].AsUUID();
                uint circuitCode = (uint)map["circuit_code"].AsInteger();
                // TODO: Send an identity url and token instead so we can pull down all of the information
                string firstName = map["first_name"].AsString();
                string lastName = map["last_name"].AsString();
                Uri callbackUri = map["callback_uri"].AsUri();

                Logger.Log(String.Format(
                    "enable_client request. agent_id: {0}, session_id: {1}, secureSessionID: {2}, " +
                    "first_name: {3}, last_name: {4}, callback_uri: {5}", agentID, sessionID, secureSessionID,
                    firstName, lastName, callbackUri), Helpers.LogLevel.Info);

                if (agentID != UUID.Zero && sessionID != UUID.Zero && secureSessionID != UUID.Zero &&
                    !String.IsNullOrEmpty(firstName) && !String.IsNullOrEmpty(lastName))
                {
                    AgentInfo info = new AgentInfo();
                    info.AccessLevel = "M";
                    info.FirstName = firstName;
                    info.Height = 1.9f;
                    info.HomeLookAt = Vector3.UnitZ;
                    info.HomePosition = new Vector3(128f, 128f, 25f);
                    info.HomeRegionHandle = regionHandle;
                    info.ID = agentID;
                    info.LastName = lastName;
                    info.PasswordHash = String.Empty;

                    Agent agent = new Agent(new SimulationObject(new Avatar(), this), info);

                    // Set the avatar ID
                    agent.Avatar.Prim.ID = agentID;

                    // Random session IDs
                    agent.SessionID = sessionID;
                    agent.SecureSessionID = secureSessionID;

                    // Create a seed capability for this agent
                    agent.SeedCapability = server.Capabilities.CreateCapability(SeedCapabilityHandler, false, agentID);

                    agent.TickLastPacketReceived = Environment.TickCount;
                    agent.Info.LastLoginTime = Utils.DateTimeToUnixTime(DateTime.Now);

                    // Add the callback URI to the list of pending enable_client_complete callbacks
                    lock (enableClientCompleteCallbacks)
                        enableClientCompleteCallbacks[agentID] = callbackUri;

                    // Assign a circuit code and track the agent as an unassociated agent (no UDP connection yet)
                    udp.CreateCircuit(agent, circuitCode);
                    agent.CircuitCode = circuitCode;

                    osdResponse["success"] = OSD.FromBoolean(true);
                }
                else
                {
                    osdResponse["success"] = OSD.FromBoolean(false);
                    osdResponse["message"] = OSD.FromString("missing required fields for enable_client");
                }
            }
            else
            {
                osdResponse["success"] = OSD.FromBoolean(false);
                osdResponse["message"] = OSD.FromString("failed to parse enable_client message");
            }

            byte[] responseData = OSDParser.SerializeLLSDXmlBytes(osdResponse);
            response.ContentLength = responseData.Length;
            response.Body.Write(responseData, 0, responseData.Length);
            response.Body.Flush();

            return true;
        }

        public bool EnableClientCompleteCapHandler(IHttpClientContext context, IHttpRequest request, IHttpResponse response, object state)
        {
            OSDMap map = OSDParser.DeserializeLLSDXml(request.Body) as OSDMap;
            OSDMap osdResponse = new OSDMap();

            if (map != null)
            {
                UUID agentID = map["agent_id"].AsUUID();
                Uri seedCapability = map["seed_capability"].AsUri();

                Logger.Log(String.Format("enable_client_complete response. agent_id: {0}, seed_capability: {1}",
                    agentID, seedCapability), Helpers.LogLevel.Info);

                if (enableClientCompleteCallbacks.Remove(agentID))
                {
                    // FIXME: Finish this
                }
            }

            return true;
        }

        bool EventQueueHandler(IHttpClientContext context, IHttpRequest request, IHttpResponse response, object state)
        {
            EventQueueServer eqServer = (EventQueueServer)state;
            return eqServer.EventQueueHandler(context, request, response);
        }

        void Grid_OnRegionUpdate(RegionInfo regionInfo)
        {
            // TODO: Detect regions coming online so we can call
            // InformClientOfNeighbors(agent);
            // for every agent

            // Check if the sim was a neighbor
            if (regionInfo.Handle == Utils.UIntsToLong(256 * (regionX - 1), 256 * (regionY + 1)))
                neighbors[0] = regionInfo;
            else if (regionInfo.Handle == Utils.UIntsToLong(256 * regionX, 256 * (regionY + 1)))
                neighbors[1] = regionInfo;
            else if (regionInfo.Handle == Utils.UIntsToLong(256 * (regionX + 1), 256 * (regionY + 1)))
                neighbors[2] = regionInfo;
            else if (regionInfo.Handle == Utils.UIntsToLong(256 * (regionX - 1), 256 * regionY))
                neighbors[3] = regionInfo;
            else if (regionInfo.Handle == Utils.UIntsToLong(256 * (regionX + 1), 256 * regionY))
                neighbors[4] = regionInfo;
            else if (regionInfo.Handle == Utils.UIntsToLong(256 * (regionX - 1), 256 * (regionY - 1)))
                neighbors[5] = regionInfo;
            else if (regionInfo.Handle == Utils.UIntsToLong(256 * regionX, 256 * (regionY - 1)))
                neighbors[6] = regionInfo;
            else if (regionInfo.Handle == Utils.UIntsToLong(256 * (regionX + 1), 256 * (regionY - 1)))
                neighbors[7] = regionInfo;
        }

        void udp_OnAgentConnection(Agent agent, uint circuitCode)
        {
            Uri callbackUri;
            if (enableClientCompleteCallbacks.TryGetValue(agent.ID, out callbackUri))
            {
                lock (enableClientCompleteCallbacks)
                    enableClientCompleteCallbacks.Remove(agent.ID);

                Logger.Log("Sending enable_client_complete callback to " + callbackUri.ToString(), Helpers.LogLevel.Info);

                OSDMap enableClientComplete = CapsMessages.EnableClientComplete(agent.ID);

                AutoResetEvent waitEvent = new AutoResetEvent(false);

                CapsClient request = new CapsClient(callbackUri);
                request.OnComplete +=
                    delegate(CapsClient client, OSD result, Exception error)
                    {
                        OSDMap response = result as OSDMap;
                        if (response != null)
                        {
                            bool success = response["success"].AsBoolean();
                            Logger.Log("enable_client_complete response: " + success, Helpers.LogLevel.Info);

                            if (success)
                            {
                                Uri seedCapability = response["seed_capability"].AsUri();
                            }
                        }
                        waitEvent.Set();
                    };
                request.StartRequest(enableClientComplete);

                if (!waitEvent.WaitOne(30 * 1000, false))
                    Logger.Log("enable_client_complete request timed out", Helpers.LogLevel.Warning);
            }
        }

        void CompleteAgentMovementHandler(Packet packet, Agent agent)
        {
            // Add this avatar as an object in the scene
            ObjectAddOrUpdate(this, agent.Avatar, agent.Avatar.Prim.OwnerID, 0, PrimFlags.None, UpdateFlags.FullUpdate);

            // Send a response back to the client
            AgentMovementCompletePacket complete = new AgentMovementCompletePacket();
            complete.AgentData.AgentID = agent.ID;
            complete.AgentData.SessionID = agent.SessionID;
            complete.Data.LookAt = Vector3.UnitZ; // TODO: Properly implement LookAt someday
            complete.Data.Position = agent.Avatar.Prim.Position;
            complete.Data.RegionHandle = regionHandle;
            complete.Data.Timestamp = Utils.DateTimeToUnixTime(DateTime.Now);
            complete.SimData.ChannelVersion = Utils.StringToBytes("Simian");

            udp.SendPacket(agent.ID, complete, PacketCategory.Transaction);

            //HACK: Notify everyone when someone logs on to the simulator
            OnlineNotificationPacket online = new OnlineNotificationPacket();
            online.AgentBlock = new OnlineNotificationPacket.AgentBlockBlock[1];
            online.AgentBlock[0] = new OnlineNotificationPacket.AgentBlockBlock();
            online.AgentBlock[0].AgentID = agent.ID;
            udp.BroadcastPacket(online, PacketCategory.State);
        }

        #endregion Callback Handlers

        void PhysicsThread()
        {
            const float TARGET_FRAME_TIME = 1f / (float)TARGET_FRAMES_PER_SECOND;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            float elapsedTime = 0f;
            int sleepMS;

            while (running)
            {
                stopwatch.Start();

                physics.Update(elapsedTime);

                // Measure the duration of this frame
                stopwatch.Stop();
                elapsedTime = (float)stopwatch.Elapsed.TotalSeconds;
                stopwatch.Reset();

                // Calculate time dilation and decide if we need to sleep to limit FPS
                if (elapsedTime < TARGET_FRAME_TIME)
                {
                    timeDilation = (1f / elapsedTime) / (float)TARGET_FRAMES_PER_SECOND;
                    sleepMS = (int)((TARGET_FRAME_TIME - elapsedTime) * 1000f);
                    Thread.Sleep(sleepMS);
                    elapsedTime = TARGET_FRAME_TIME;
                }
                else
                {
                    timeDilation = 1f;
                }
            }
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
                    updateToOwner.RegionData.TimeDilation = (ushort)(timeDilation * (float)UInt16.MaxValue);
                    updateToOwner.ObjectData = new ObjectUpdatePacket.ObjectDataBlock[1];
                    updateToOwner.ObjectData[0] = SimulationObject.BuildUpdateBlock(obj.Prim,
                        obj.Prim.Flags | creatorFlags | PrimFlags.ObjectYouOwner, obj.CRC);

                    udp.SendPacket(obj.Prim.OwnerID, updateToOwner, PacketCategory.State);
                }

                // Send an update out to everyone else
                ObjectUpdatePacket updateToOthers = new ObjectUpdatePacket();
                updateToOthers.RegionData.RegionHandle = regionHandle;
                updateToOthers.RegionData.TimeDilation = UInt16.MaxValue;
                updateToOthers.ObjectData = new ObjectUpdatePacket.ObjectDataBlock[1];
                updateToOthers.ObjectData[0] = SimulationObject.BuildUpdateBlock(obj.Prim,
                    obj.Prim.Flags, obj.CRC);

                ForEachAgent(
                    delegate(Agent recipient)
                    {
                        if (recipient.ID != obj.Prim.OwnerID)
                            udp.SendPacket(recipient.ID, updateToOthers, PacketCategory.State);
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
                    updateToOwner.RegionData.TimeDilation = (ushort)(timeDilation * (float)UInt16.MaxValue);
                    updateToOwner.ObjectData = new ObjectUpdateCompressedPacket.ObjectDataBlock[1];
                    updateToOwner.ObjectData[0] = new ObjectUpdateCompressedPacket.ObjectDataBlock();
                    updateToOwner.ObjectData[0].UpdateFlags = (uint)(obj.Prim.Flags | creatorFlags | PrimFlags.ObjectYouOwner);
                    updateToOwner.ObjectData[0].Data = data;

                    udp.SendPacket(obj.Prim.OwnerID, updateToOwner, PacketCategory.State);
                }

                // Send an update out to everyone else
                ObjectUpdateCompressedPacket updateToOthers = new ObjectUpdateCompressedPacket();
                updateToOthers.RegionData.RegionHandle = regionHandle;
                updateToOthers.RegionData.TimeDilation = UInt16.MaxValue;
                updateToOthers.ObjectData = new ObjectUpdateCompressedPacket.ObjectDataBlock[1];
                updateToOthers.ObjectData[0] = new ObjectUpdateCompressedPacket.ObjectDataBlock();
                updateToOthers.ObjectData[0].UpdateFlags = (uint)obj.Prim.Flags;
                updateToOthers.ObjectData[0].Data = data;

                ForEachAgent(
                    delegate(Agent recipient)
                    {
                        if (recipient.ID != obj.Prim.OwnerID)
                            udp.SendPacket(recipient.ID, updateToOthers, PacketCategory.State);
                    }
                );

                #endregion Packet sending

                #endregion ObjectUpdateCompressed
            }
            else
            {
                #region ImprovedTerseObjectUpdate

                //Logger.DebugLog("Sending ImprovedTerseObjectUpdate");

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
                update.RegionData.TimeDilation = (ushort)(timeDilation * (float)UInt16.MaxValue);
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

                udp.BroadcastPacket(update, PacketCategory.State);

                #endregion ImprovedTerseObjectUpdate
            }
        }
    }
}
