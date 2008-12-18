using System;
using System.Collections.Generic;
using System.Threading;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    public class Periscope : IExtension<Simian>
    {
        public Agent MasterAgent = null;

        Simian server;
        GridClient client;
        PeriscopeImageDelivery imageDelivery;
        PeriscopeMovement movement;
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

            client.Network.OnCurrentSimChanged += new NetworkManager.CurrentSimChangedCallback(Network_OnCurrentSimChanged);
            client.Objects.OnNewPrim += new OpenMetaverse.ObjectManager.NewPrimCallback(Objects_OnNewPrim);
            client.Objects.OnNewAvatar += new OpenMetaverse.ObjectManager.NewAvatarCallback(Objects_OnNewAvatar);
            client.Objects.OnNewAttachment += new OpenMetaverse.ObjectManager.NewAttachmentCallback(Objects_OnNewAttachment);
            client.Objects.OnObjectKilled += new OpenMetaverse.ObjectManager.KillObjectCallback(Objects_OnObjectKilled);
            client.Objects.OnObjectUpdated += new OpenMetaverse.ObjectManager.ObjectUpdatedCallback(Objects_OnObjectUpdated);
            client.Avatars.OnAvatarAppearance += new OpenMetaverse.AvatarManager.AvatarAppearanceCallback(Avatars_OnAvatarAppearance);
            client.Terrain.OnLandPatch += new TerrainManager.LandPatchCallback(Terrain_OnLandPatch);
            client.Self.OnChat += new AgentManager.ChatCallback(Self_OnChat);
            client.Self.OnTeleport += new AgentManager.TeleportCallback(Self_OnTeleport);
            client.Network.RegisterCallback(PacketType.AvatarAnimation, AvatarAnimationHandler);
            client.Network.RegisterCallback(PacketType.RegionHandshake, RegionHandshakeHandler);

            server.UDP.RegisterPacketCallback(PacketType.AgentUpdate, AgentUpdateHandler);
            server.UDP.RegisterPacketCallback(PacketType.ChatFromViewer, ChatFromViewerHandler);

            imageDelivery = new PeriscopeImageDelivery(server, client);
            movement = new PeriscopeMovement(server, this);
        }

        public void Stop()
        {
            movement.Stop();
            imageDelivery.Stop();

            if (client.Network.Connected)
                client.Network.Logout();
        }

        void Objects_OnNewPrim(Simulator simulator, Primitive prim, ulong regionHandle, ushort timeDilation)
        {
            SimulationObject simObj = new SimulationObject(prim, server);
            server.Scene.ObjectAdd(this, simObj, PrimFlags.None);
        }

        void Objects_OnNewAttachment(Simulator simulator, Primitive prim, ulong regionHandle, ushort timeDilation)
        {
            SimulationObject simObj = new SimulationObject(prim, server);
            server.Scene.ObjectAdd(this, simObj, PrimFlags.None);
        }

        void Objects_OnNewAvatar(Simulator simulator, Avatar avatar, ulong regionHandle, ushort timeDilation)
        {
            // Add the avatar to both the agents list and the scene objects
            Agent agent = new Agent();
            agent.AgentID = avatar.ID;
            agent.Avatar = avatar;
            agent.CurrentRegionHandle = server.RegionHandle;
            agent.FirstName = avatar.FirstName;
            agent.LastName = avatar.LastName;
            
            lock (server.Agents)
                server.Agents[agent.AgentID] = agent;

            SimulationObject simObj = new SimulationObject(avatar, server);
            server.Scene.ObjectAdd(this, simObj, avatar.Flags);
        }

        void Objects_OnObjectUpdated(Simulator simulator, ObjectUpdate update, ulong regionHandle, ushort timeDilation)
        {
            server.Scene.ObjectTransform(this, update.LocalID, update.Position, update.Rotation, update.Velocity,
                update.Acceleration, update.AngularVelocity);

            if (update.LocalID == client.Self.LocalID)
            {
                MasterAgent.Avatar.Acceleration = update.Acceleration;
                MasterAgent.Avatar.AngularVelocity = update.AngularVelocity;
                MasterAgent.Avatar.CollisionPlane = update.CollisionPlane;
                MasterAgent.Avatar.Position = update.Position;
                MasterAgent.Avatar.Rotation = update.Rotation;
                MasterAgent.Avatar.Velocity = update.Velocity;

                if (update.Textures != null)
                    MasterAgent.Avatar.Textures = update.Textures;
            }
        }

        void Objects_OnObjectKilled(Simulator simulator, uint objectID)
        {
            server.Scene.ObjectRemove(this, objectID);
        }

        void Avatars_OnAvatarAppearance(UUID avatarID, bool isTrial, Primitive.TextureEntryFace defaultTexture,
            Primitive.TextureEntryFace[] faceTextures, List<byte> visualParams)
        {
            Agent agent;
            if (server.Agents.TryGetValue(avatarID, out agent))
            {
                Primitive.TextureEntry te = new Primitive.TextureEntry(defaultTexture);
                te.FaceTextures = faceTextures;

                byte[] vp = (visualParams != null && visualParams.Count > 1 ? visualParams.ToArray() : null);

                Logger.Log("[Periscope] Updating foreign avatar appearance for " + agent.FirstName + " " + agent.LastName, Helpers.LogLevel.Info);

                server.Scene.AvatarAppearance(this, agent, te, vp);

                if (agent.AgentID == client.Self.AgentID)
                    server.Scene.AvatarAppearance(this, MasterAgent, te, vp);
            }
            else
            {
                Logger.Log("[Periscope] Received a foreign avatar appearance for an unknown avatar", Helpers.LogLevel.Warning);
            }
        }

        void Terrain_OnLandPatch(Simulator simulator, int x, int y, int width, float[] data)
        {
            // TODO: When Simian gets a terrain editing interface, switch this over to
            // edit the scene heightmap instead of sending packets direct to clients
            int[] patches = new int[1];
            patches[0] = (y * 16) + x;
            LayerDataPacket layer = TerrainCompressor.CreateLandPacket(data, x, y);
            server.UDP.BroadcastPacket(layer, PacketCategory.Terrain);
        }

        void Self_OnChat(string message, ChatAudibleLevel audible, ChatType type, ChatSourceType sourceType,
            string fromName, UUID id, UUID ownerid, Vector3 position)
        {
            // TODO: Inject chat into the Scene instead of relaying it
            ChatFromSimulatorPacket chat = new ChatFromSimulatorPacket();
            chat.ChatData.Audible = (byte)ChatAudibleLevel.Fully;
            chat.ChatData.ChatType = (byte)type;
            chat.ChatData.OwnerID = ownerid;
            chat.ChatData.SourceID = id;
            chat.ChatData.SourceType = (byte)sourceType;
            chat.ChatData.Position = position;
            chat.ChatData.FromName = Utils.StringToBytes(fromName);
            chat.ChatData.Message = Utils.StringToBytes(message);

            server.UDP.BroadcastPacket(chat, PacketCategory.Transaction);
        }

        void Self_OnTeleport(string message, AgentManager.TeleportStatus status, AgentManager.TeleportFlags flags)
        {
            if (status == AgentManager.TeleportStatus.Finished)
            {
                // Kill off any prims from the previous sim
                IDictionary<uint, SimulationObject> scene = server.Scene.GetSceneCopy();

                foreach (SimulationObject obj in scene.Values)
                {
                    if (obj.Prim.RegionHandle != client.Network.CurrentSim.Handle)
                        server.Scene.ObjectRemove(this, obj.Prim.ID);
                }
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
            if (server.Agents.TryGetValue(animations.Sender.ID, out agent))
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

            handshake.RegionInfo.SimOwner = (MasterAgent != null ? MasterAgent.AgentID : UUID.Zero);

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
                        //string simName;
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

            if (MasterAgent == null)
            {
                lock (loginLock)
                {
                    // Double-checked locking to avoid hitting the loginLock each time
                    if (MasterAgent == null &&
                        server.Agents.TryGetValue(update.AgentData.AgentID, out MasterAgent))
                    {
                        Logger.Log(String.Format("[Periscope] {0} {1} is the controlling agent",
                            MasterAgent.FirstName, MasterAgent.LastName), Helpers.LogLevel.Info);

                        LoginParams login = client.Network.DefaultLoginParams(agent.FirstName, agent.LastName, agent.PasswordHash, "Simian Periscope", "1.0.0");
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

            if (MasterAgent == null || update.AgentData.AgentID == MasterAgent.AgentID)
            {
                // Forward AgentUpdate packets with the AgentID/SessionID set to the bots ID
                update.AgentData.AgentID = client.Self.AgentID;
                update.AgentData.SessionID = client.Self.SessionID;
                client.Network.SendPacket(update);
            }
        }

        #endregion Simian client packet handlers
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
