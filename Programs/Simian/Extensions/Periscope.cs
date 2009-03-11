using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    public class Periscope : IExtension<Simian>
    {
        // Change this to login to a different grid
        const string PERISCOPE_LOGIN_URI = Settings.AGNI_LOGIN_SERVER;

        public Agent MasterAgent = null;

        Simian server;
        GridClient client;
        PeriscopeImageDelivery imageDelivery;
        PeriscopeMovement movement;
        PeriscopeTransferManager transferManager;
        bool ignoreObjectKill = false;
        object loginLock = new object();

        public Periscope()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;

            client = new GridClient();
            Settings.LOG_LEVEL = Helpers.LogLevel.Info;
            client.Settings.MULTIPLE_SIMS = false;
            client.Settings.SEND_AGENT_UPDATES = false;

            client.Network.OnCurrentSimChanged += Network_OnCurrentSimChanged;
            client.Objects.OnNewPrim += Objects_OnNewPrim;
            client.Objects.OnNewAvatar += Objects_OnNewAvatar;
            client.Objects.OnNewAttachment += Objects_OnNewAttachment;
            client.Objects.OnObjectKilled += Objects_OnObjectKilled;
            client.Objects.OnObjectUpdated += Objects_OnObjectUpdated;
            client.Avatars.OnAvatarAppearance += Avatars_OnAvatarAppearance;
            client.Terrain.OnLandPatch += new TerrainManager.LandPatchCallback(Terrain_OnLandPatch);
            client.Self.OnChat += new AgentManager.ChatCallback(Self_OnChat);
            client.Self.OnTeleport += new AgentManager.TeleportCallback(Self_OnTeleport);
            client.Network.RegisterCallback(PacketType.AvatarAnimation, AvatarAnimationHandler);
            client.Network.RegisterCallback(PacketType.RegionHandshake, RegionHandshakeHandler);

            server.UDP.RegisterPacketCallback(PacketType.AgentUpdate, AgentUpdateHandler);
            server.UDP.RegisterPacketCallback(PacketType.ChatFromViewer, ChatFromViewerHandler);
            server.UDP.RegisterPacketCallback(PacketType.ObjectGrab, ObjectGrabHandler);
            server.UDP.RegisterPacketCallback(PacketType.ObjectGrabUpdate, ObjectGrabUpdateHandler);
            server.UDP.RegisterPacketCallback(PacketType.ObjectDeGrab, ObjectDeGrabHandler);
            server.UDP.RegisterPacketCallback(PacketType.ViewerEffect, ViewerEffectHandler);
            server.UDP.RegisterPacketCallback(PacketType.AgentAnimation, AgentAnimationHandler);

            imageDelivery = new PeriscopeImageDelivery(server, client);
            movement = new PeriscopeMovement(server, this);
            transferManager = new PeriscopeTransferManager(server, client);
        }

        public void Stop()
        {
            transferManager.Stop();
            movement.Stop();
            imageDelivery.Stop();

            if (client.Network.Connected)
                client.Network.Logout();
        }

        void Objects_OnNewPrim(Simulator simulator, Primitive prim, ulong regionHandle, ushort timeDilation)
        {
            SimulationObject simObj = new SimulationObject(prim, server);
            if (MasterAgent != null)
                simObj.Prim.OwnerID = MasterAgent.ID;
            server.Scene.ObjectAddOrUpdate(this, simObj, MasterAgent.ID, 0, PrimFlags.None, UpdateFlags.FullUpdate);
        }

        void Objects_OnNewAttachment(Simulator simulator, Primitive prim, ulong regionHandle, ushort timeDilation)
        {
            SimulationObject simObj = new SimulationObject(prim, server);
            server.Scene.ObjectAddOrUpdate(this, simObj, MasterAgent.ID, 0, PrimFlags.None, UpdateFlags.FullUpdate);
        }

        void Objects_OnNewAvatar(Simulator simulator, Avatar avatar, ulong regionHandle, ushort timeDilation)
        {
            ulong localRegionHandle = Utils.UIntsToLong(256 * server.Scene.RegionX, 256 * server.Scene.RegionY);

            // Add the avatar to both the agents list and the scene objects
            SimulationObject obj = new SimulationObject(avatar, server);
            Agent agent = new Agent(obj);
            agent.Avatar.Prim.ID = avatar.ID;
            agent.CurrentRegionHandle = localRegionHandle;
            agent.FirstName = avatar.FirstName;
            agent.LastName = avatar.LastName;

            server.Scene.AgentAdd(this, agent, avatar.Flags);
            server.Scene.ObjectAddOrUpdate(this, obj, obj.Prim.OwnerID, 0, PrimFlags.None, UpdateFlags.FullUpdate);
        }

        void Objects_OnObjectUpdated(Simulator simulator, ObjectUpdate update, ulong regionHandle, ushort timeDilation)
        {
            SimulationObject obj;
            UpdateFlags flags = UpdateFlags.Acceleration | UpdateFlags.AngularVelocity | UpdateFlags.Position |
                UpdateFlags.Rotation | UpdateFlags.Velocity;

            if (update.Avatar) flags |= UpdateFlags.CollisionPlane;
            if (update.Textures != null) flags |= UpdateFlags.Textures;

            if (server.Scene.TryGetObject(update.LocalID, out obj))
            {
                obj.Prim.Acceleration = update.Acceleration;
                obj.Prim.AngularVelocity = update.AngularVelocity;
                obj.Prim.Position = update.Position;
                obj.Prim.Rotation = update.Rotation;
                obj.Prim.Velocity = update.Velocity;
                if (update.Avatar) obj.Prim.CollisionPlane = update.CollisionPlane;
                if (update.Textures != null) obj.Prim.Textures = update.Textures;

                server.Scene.ObjectAddOrUpdate(this, obj, obj.Prim.OwnerID, 0, PrimFlags.None, flags);
            }

            if (update.LocalID == client.Self.LocalID)
            {
                MasterAgent.Avatar.Prim.Acceleration = update.Acceleration;
                MasterAgent.Avatar.Prim.AngularVelocity = update.AngularVelocity;
                MasterAgent.Avatar.Prim.Position = update.Position;
                MasterAgent.Avatar.Prim.Rotation = update.Rotation;
                MasterAgent.Avatar.Prim.Velocity = update.Velocity;
                if (update.Avatar) MasterAgent.Avatar.Prim.CollisionPlane = update.CollisionPlane;
                if (update.Textures != null) MasterAgent.Avatar.Prim.Textures = update.Textures;

                server.Scene.ObjectAddOrUpdate(this, MasterAgent.Avatar, MasterAgent.ID, 0, PrimFlags.None, flags);
            }
        }

        void Objects_OnObjectKilled(Simulator simulator, uint objectID)
        {
            if (!ignoreObjectKill)
                server.Scene.ObjectRemove(this, objectID);
        }

        void Avatars_OnAvatarAppearance(UUID avatarID, bool isTrial, Primitive.TextureEntryFace defaultTexture,
            Primitive.TextureEntryFace[] faceTextures, List<byte> visualParams)
        {
            Primitive.TextureEntry te = new Primitive.TextureEntry(defaultTexture);
            te.FaceTextures = faceTextures;
            byte[] vp = (visualParams != null && visualParams.Count > 1 ? visualParams.ToArray() : null);

            Agent agent;
            if (server.Scene.TryGetAgent(avatarID, out agent))
            {
                Logger.Log("[Periscope] Updating foreign avatar appearance for " + agent.FirstName + " " + agent.LastName, Helpers.LogLevel.Info);
                server.Scene.AgentAppearance(this, agent, te, vp);
            }

            if (avatarID == client.Self.AgentID)
            {
                Logger.Log("[Periscope] Updating foreign avatar appearance for the MasterAgent", Helpers.LogLevel.Info);
                server.Scene.AgentAppearance(this, MasterAgent, te, vp);
            }
        }

        void Terrain_OnLandPatch(Simulator simulator, int x, int y, int width, float[] data)
        {
            float[,] patchData = new float[16, 16];
            for (int py = 0; py < 16; py++)
            {
                for (int px = 0; px < 16; px++)
                {
                    patchData[py, px] = data[py * 16 + px];
                }
            }

            server.Scene.SetTerrainPatch(this, (uint)x, (uint)y, patchData);
        }

        void Self_OnChat(string message, ChatAudibleLevel audible, ChatType type, ChatSourceType sourceType,
            string fromName, UUID id, UUID ownerid, Vector3 position)
        {
            server.Scene.ObjectChat(this, ownerid, id, audible, type, sourceType, fromName, position, 0, message);
        }

        void Self_OnTeleport(string message, TeleportStatus status, TeleportFlags flags)
        {
            if (status == TeleportStatus.Finished)
            {
                ulong localRegionHandle = server.Scene.RegionHandle;

                server.Scene.RemoveAllAgents(
                    delegate(Agent agent)
                    { return agent.Avatar.Prim.RegionHandle != client.Network.CurrentSim.Handle && agent.Avatar.Prim.RegionHandle != localRegionHandle; });

                server.Scene.RemoveAllObjects(
                    delegate(SimulationObject obj)
                    { return obj.Prim.RegionHandle != client.Network.CurrentSim.Handle && obj.Prim.RegionHandle != localRegionHandle; });
            }
        }

        void Network_OnCurrentSimChanged(Simulator PreviousSimulator)
        {
            Logger.Log("[Periscope] Sending bot appearance", Helpers.LogLevel.Info);
            client.Appearance.SetPreviousAppearance(false);
        }

        void AvatarAnimationHandler(Packet packet, Simulator simulator)
        {
            AvatarAnimationPacket animations = (AvatarAnimationPacket)packet;
            
            Agent agent;
            if (server.Scene.TryGetAgent(animations.Sender.ID, out agent))
            {
                agent.Animations.Clear();

                for (int i = 0; i < animations.AnimationList.Length; i++)
                {
                    AvatarAnimationPacket.AnimationListBlock block = animations.AnimationList[i];
                    agent.Animations.Add(block.AnimID, block.AnimSequenceID);
                }

                server.Avatars.SendAnimations(agent);
            }

            if (animations.Sender.ID == client.Self.AgentID)
            {
                MasterAgent.Animations.Clear();

                for (int i = 0; i < animations.AnimationList.Length; i++)
                {
                    AvatarAnimationPacket.AnimationListBlock block = animations.AnimationList[i];
                    MasterAgent.Animations.Add(block.AnimID, block.AnimSequenceID);
                }

                server.Avatars.SendAnimations(MasterAgent);
            }
        }

        void RegionHandshakeHandler(Packet packet, Simulator simulator)
        {
            RegionHandshakePacket handshake = (RegionHandshakePacket)packet;

            handshake.RegionInfo.SimOwner = (MasterAgent != null ? MasterAgent.ID : UUID.Zero);
            handshake.RegionInfo.RegionFlags &= ~(uint)RegionFlags.NoFly;
            handshake.RegionInfo2.RegionID = server.Scene.RegionID;

            // TODO: Need more methods to manipulate the scene so we can apply these properties.
            // Right now this only gets sent out to people who are logged in when the master avatar
            // is already logged in
            server.UDP.BroadcastPacket(handshake, PacketCategory.Transaction);
        }

        #region Simian client packet handlers

        void ChatFromViewerHandler(Packet packet, Agent agent)
        {
            ChatFromViewerPacket chat = (ChatFromViewerPacket)packet;

            // Forward chat from the viewer to the foreign simulator
            string message = Utils.BytesToString(chat.ChatData.Message);

            if (!String.IsNullOrEmpty(message) && message[0] == '/')
            {
                string[] messageParts = CommandLineParser.Parse(message);

                switch (messageParts[0])
                {
                    case "/teleport":
                    {
                        float x, y, z;

                        if (messageParts.Length == 5 &&
                            Single.TryParse(messageParts[2], out x) &&
                            Single.TryParse(messageParts[3], out y) &&
                            Single.TryParse(messageParts[4], out z))
                        {
                            server.Avatars.SendAlert(agent, String.Format("Attempting teleport to {0} <{1}, {2}, {3}>",
                                messageParts[1], messageParts[2], messageParts[3], messageParts[4]));
                            client.Self.Teleport(messageParts[1], new Vector3(x, y, z));
                        }
                        else
                        {
                            server.Avatars.SendAlert(agent, "Usage: /teleport \"sim name\" x y z");
                        }
                        return;
                    }
                    case "/stats":
                        server.Avatars.SendAlert(agent, String.Format("Downloading textures: {0}, Queued textures: {1}",
                            imageDelivery.Pipeline.CurrentCount, imageDelivery.Pipeline.QueuedCount));
                        return;
                    case "/objectkill":
                        if (messageParts.Length == 2)
                        {
                            if (messageParts[1] == "off" || messageParts[1] == "0")
                            {
                                ignoreObjectKill = true;
                                server.Avatars.SendAlert(agent, "Ignoring upstream ObjectKill packets");
                            }
                            else
                            {
                                ignoreObjectKill = false;
                                server.Avatars.SendAlert(agent, "Enabling upstream ObjectKill packets");
                            }
                        }
                        return;
                    case "/save":
                    {
                        if (messageParts.Length == 2)
                        {
                            string filename = messageParts[1];
                            string directoryname = Path.GetFileNameWithoutExtension(filename);

                            Thread saveThread = new Thread(new ThreadStart(
                                delegate()
                                {
                                    Logger.Log(String.Format("Preparing to serialize {0} objects", server.Scene.ObjectCount()), Helpers.LogLevel.Info);
                                    OarFile.SavePrims(server, directoryname + "/objects", directoryname + "/assets", Simian.ASSET_CACHE_DIR);

                                    try { Directory.Delete(directoryname + "/terrains", true); }
                                    catch (Exception) { }

                                    try
                                    {
                                        Directory.CreateDirectory(directoryname + "/terrains");

                                        using (FileStream stream = new FileStream(directoryname + "/terrains/heightmap.r32", FileMode.Create, FileAccess.Write))
                                        {
                                            for (int y = 0; y < 256; y++)
                                            {
                                                for (int x = 0; x < 256; x++)
                                                {
                                                    float t = server.Scene.GetTerrainHeightAt(x, y);
                                                    stream.Write(BitConverter.GetBytes(t), 0, 4);
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Log("Failed saving terrain: " + ex.Message, Helpers.LogLevel.Error);
                                    }

                                    Logger.Log("Saving " + directoryname, Helpers.LogLevel.Info);
                                    OarFile.PackageArchive(directoryname, filename);

                                    try
                                    { System.IO.Directory.Delete(directoryname, true); }
                                    catch (Exception ex)
                                    { Logger.Log("Failed to delete temporary directory " + directoryname + ": " + ex.Message, Helpers.LogLevel.Error); }

                                    server.Avatars.SendAlert(agent, "Finished OAR export to " + filename);
                                }));

                            saveThread.Start();
                            server.Avatars.SendAlert(agent, "Starting OAR export to " + filename);
                        }
                        return;
                    }
                    case "/nudemod":
                        //int count = 0;
                        // FIXME: AvatarAppearance locks the agents dictionary. Need to be able to copy the agents dictionary?
                        /*foreach (Agent curAgent in agents.Values)
                        {
                            if (curAgent != agent && curAgent.VisualParams != null)
                            {
                                curAgent.Avatar.Textures.FaceTextures[(int)AppearanceManager.TextureIndex.LowerBaked] = null;
                                curAgent.Avatar.Textures.FaceTextures[(int)AppearanceManager.TextureIndex.LowerJacket] = null;
                                curAgent.Avatar.Textures.FaceTextures[(int)AppearanceManager.TextureIndex.LowerPants] = null;
                                curAgent.Avatar.Textures.FaceTextures[(int)AppearanceManager.TextureIndex.LowerShoes] = null;
                                curAgent.Avatar.Textures.FaceTextures[(int)AppearanceManager.TextureIndex.LowerSocks] = null;
                                curAgent.Avatar.Textures.FaceTextures[(int)AppearanceManager.TextureIndex.LowerUnderpants] = null;

                                curAgent.Avatar.Textures.FaceTextures[(int)AppearanceManager.TextureIndex.UpperBaked] = null;
                                curAgent.Avatar.Textures.FaceTextures[(int)AppearanceManager.TextureIndex.UpperGloves] = null;
                                curAgent.Avatar.Textures.FaceTextures[(int)AppearanceManager.TextureIndex.UpperJacket] = null;
                                curAgent.Avatar.Textures.FaceTextures[(int)AppearanceManager.TextureIndex.UpperShirt] = null;
                                curAgent.Avatar.Textures.FaceTextures[(int)AppearanceManager.TextureIndex.UpperUndershirt] = null;

                                server.Scene.AvatarAppearance(this, curAgent, curAgent.Avatar.Textures, curAgent.VisualParams);
                                ++count;
                            }
                        }

                        server.Avatars.SendAlert(agent, String.Format("Modified appearances for {0} avatar(s)", count));*/
                        return;
                }
            }
            
            string finalMessage;
            if (agent.FirstName == client.Self.FirstName && agent.LastName == client.Self.LastName)
                finalMessage = message;
            else
                finalMessage = String.Format("<{0} {1}> {2}", agent.FirstName, agent.LastName, message);

            client.Self.Chat(finalMessage, chat.ChatData.Channel, (ChatType)chat.ChatData.Type);
        }

        void AgentUpdateHandler(Packet packet, Agent agent)
        {
            AgentUpdatePacket update = (AgentUpdatePacket)packet;

            if (MasterAgent == null || (!client.Network.Connected && client.Network.LoginStatusCode == LoginStatus.Failed))
            {
                MasterAgent = null;

                lock (loginLock)
                {
                    // Double-checked locking to avoid hitting the loginLock each time
                    if (MasterAgent == null && server.Scene.TryGetAgent(update.AgentData.AgentID, out MasterAgent))
                    {
                        Logger.Log(String.Format("[Periscope] {0} {1} is the controlling agent",
                            MasterAgent.FirstName, MasterAgent.LastName), Helpers.LogLevel.Info);

                        LoginParams login = client.Network.DefaultLoginParams(agent.FirstName, agent.LastName, agent.PasswordHash,
                            "Simian Periscope", "1.0.0");
                        login.URI = PERISCOPE_LOGIN_URI;
                        login.Start = "last";
                        client.Network.Login(login);

                        if (client.Network.Connected)
                        {
                            Logger.Log("[Periscope] Connected: " + client.Network.LoginMessage, Helpers.LogLevel.Info);
                        }
                        else
                        {
                            string error = String.Format("Failed to connect to the foreign grid: ({0}): {1}", client.Network.LoginErrorKey,
                                client.Network.LoginMessage);
                            Logger.Log("[Periscope] " + error, Helpers.LogLevel.Error);
                        }
                    }
                }
            }

            if (MasterAgent == null || update.AgentData.AgentID == MasterAgent.ID)
            {
                update.AgentData.AgentID = client.Self.AgentID;
                update.AgentData.SessionID = client.Self.SessionID;
                client.Network.SendPacket(update);
            }
        }

        void ObjectGrabHandler(Packet packet, Agent agent)
        {
            ObjectGrabPacket grab = (ObjectGrabPacket)packet;

            if (MasterAgent == null || grab.AgentData.AgentID == MasterAgent.ID)
            {
                grab.AgentData.AgentID = client.Self.AgentID;
                grab.AgentData.SessionID = client.Self.SessionID;

                client.Network.SendPacket(grab);
            }
        }

        void ObjectGrabUpdateHandler(Packet packet, Agent agent)
        {
            ObjectGrabUpdatePacket grabUpdate = (ObjectGrabUpdatePacket)packet;

            if (MasterAgent == null || grabUpdate.AgentData.AgentID == MasterAgent.ID)
            {
                grabUpdate.AgentData.AgentID = client.Self.AgentID;
                grabUpdate.AgentData.SessionID = client.Self.SessionID;

                client.Network.SendPacket(grabUpdate);
            }
        }

        void ObjectDeGrabHandler(Packet packet, Agent agent)
        {
            ObjectDeGrabPacket degrab = (ObjectDeGrabPacket)packet;

            if (MasterAgent == null || degrab.AgentData.AgentID == MasterAgent.ID)
            {
                degrab.AgentData.AgentID = client.Self.AgentID;
                degrab.AgentData.SessionID = client.Self.SessionID;

                client.Network.SendPacket(degrab);
            }
        }

        void ViewerEffectHandler(Packet packet, Agent agent)
        {
            ViewerEffectPacket effect = (ViewerEffectPacket)packet;

            if (MasterAgent == null || effect.AgentData.AgentID == MasterAgent.ID)
            {
                effect.AgentData.AgentID = client.Self.AgentID;
                effect.AgentData.SessionID = client.Self.SessionID;

                client.Network.SendPacket(effect);
            }
        }

        void AgentAnimationHandler(Packet packet, Agent agent)
        {
            AgentAnimationPacket animation = (AgentAnimationPacket)packet;

            if (MasterAgent == null || animation.AgentData.AgentID == MasterAgent.ID)
            {
                animation.AgentData.AgentID = client.Self.AgentID;
                animation.AgentData.SessionID = client.Self.SessionID;

                client.Network.SendPacket(animation);
            }
        }

        #endregion Simian client packet handlers

        static void EraseTexture(Avatar avatar, AppearanceManager.TextureIndex texture)
        {
            Primitive.TextureEntryFace face = avatar.Textures.FaceTextures[(int)texture];
            if (face != null) face.TextureID = UUID.Zero;
        }
    }

    public static class CommandLineParser
    {
        public static string[] Parse(string str)
        {
            if (str == null || !(str.Length > 0)) return new string[0];
            int idx = str.Trim().IndexOf(" ");
            if (idx == -1) return new string[] { str };
            int count = str.Length;
            List<string> list = new List<string>();

            while (count > 0)
            {
                if (str[0] == '"')
                {
                    int temp = str.IndexOf("\"", 1, str.Length - 1);
                    while (str[temp - 1] == '\\')
                    {
                        temp = str.IndexOf("\"", temp + 1, str.Length - temp - 1);
                    }
                    idx = temp + 1;
                }
                if (str[0] == '\'')
                {
                    int temp = str.IndexOf("\'", 1, str.Length - 1);
                    while (str[temp - 1] == '\\')
                    {
                        temp = str.IndexOf("\'", temp + 1, str.Length - temp - 1);
                    }
                    idx = temp + 1;
                }
                string s = str.Substring(0, idx);
                int left = count - idx;
                str = str.Substring(idx, left).Trim();
                list.Add(s.Trim('"'));
                count = str.Length;
                idx = str.IndexOf(" ");
                if (idx == -1)
                {
                    string add = str.Trim('"', ' ');
                    if (add.Length > 0)
                    {
                        list.Add(add);
                    }
                    break;
                }
            }

            return list.ToArray();
        }
    }
}
