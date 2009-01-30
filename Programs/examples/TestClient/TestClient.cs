using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using OpenMetaverse;
using OpenMetaverse.Packets;
using OpenMetaverse.Utilities;

namespace OpenMetaverse.TestClient
{
    public class TestClient : GridClient
    {
        public Guid GroupID = Guid.Empty;
        public Dictionary<Guid, GroupMember> GroupMembers;
        public Dictionary<Guid, AvatarAppearancePacket> Appearances = new Dictionary<Guid, AvatarAppearancePacket>();
        public Dictionary<string, Command> Commands = new Dictionary<string, Command>();
        public bool Running = true;
        public bool GroupCommands = false;
        public string MasterName = String.Empty;
        public Guid MasterKey = Guid.Empty;
        public bool AllowObjectMaster = false;
        public ClientManager ClientManager;
        public VoiceManager VoiceManager;
        // Shell-like inventory commands need to be aware of the 'current' inventory folder.
        public InventoryFolder CurrentDirectory = null;

        private Quaternion bodyRotation = Quaternion.Identity;
        private Vector3 forward = new Vector3(0, 0.9999f, 0);
        private Vector3 left = new Vector3(0.9999f, 0, 0);
        private Vector3 up = new Vector3(0, 0, 0.9999f);
        private System.Timers.Timer updateTimer;

        /// <summary>
        /// 
        /// </summary>
        public TestClient(ClientManager manager)
        {
            ClientManager = manager;

            updateTimer = new System.Timers.Timer(500);
            updateTimer.Elapsed += new System.Timers.ElapsedEventHandler(updateTimer_Elapsed);

            RegisterAllCommands(Assembly.GetExecutingAssembly());

            Settings.LOG_LEVEL = Helpers.LogLevel.Debug;
            Settings.LOG_RESENDS = false;
            Settings.STORE_LAND_PATCHES = true;
            Settings.ALWAYS_DECODE_OBJECTS = true;
            Settings.ALWAYS_REQUEST_OBJECTS = true;
            Settings.SEND_AGENT_UPDATES = true;
            Settings.USE_TEXTURE_CACHE = true;

            Network.RegisterCallback(PacketType.AgentDataUpdate, new NetworkManager.PacketCallback(AgentDataUpdateHandler));
            Network.OnLogin += new NetworkManager.LoginCallback(LoginHandler);
            Self.OnInstantMessage += new AgentManager.InstantMessageCallback(Self_OnInstantMessage);
            Groups.OnGroupMembers += new GroupManager.GroupMembersCallback(GroupMembersHandler);
            Inventory.OnObjectOffered += new InventoryManager.ObjectOfferedCallback(Inventory_OnInventoryObjectReceived);

            Network.RegisterCallback(PacketType.AvatarAppearance, new NetworkManager.PacketCallback(AvatarAppearanceHandler));
            Network.RegisterCallback(PacketType.AlertMessage, new NetworkManager.PacketCallback(AlertMessageHandler));

            VoiceManager = new VoiceManager(this);

            updateTimer.Start();
        }

        /// <summary>
        /// Initialize everything that needs to be initialized once we're logged in.
        /// </summary>
        /// <param name="login">The status of the login</param>
        /// <param name="message">Error message on failure, MOTD on success.</param>
        public void LoginHandler(LoginStatus login, string message)
        {
            if (login == LoginStatus.Success)
            {
                // Start in the inventory root folder.
                CurrentDirectory = Inventory.Store.RootFolder;
            }
        }

        public void RegisterAllCommands(Assembly assembly)
        {
            foreach (Type t in assembly.GetTypes())
            {
                try
                {
                    if (t.IsSubclassOf(typeof(Command)))
                    {
                        ConstructorInfo info = t.GetConstructor(new Type[] { typeof(TestClient) });
                        Command command = (Command)info.Invoke(new object[] { this });
                        RegisterCommand(command);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        public void RegisterCommand(Command command)
        {
            command.Client = this;
            if (!Commands.ContainsKey(command.Name.ToLower()))
            {
                Commands.Add(command.Name.ToLower(), command);
            }
        }

        //breaks up large responses to deal with the max IM size
        private void SendResponseIM(GridClient client, Guid fromAgentID, string data)
        {
            for (int i = 0; i < data.Length; i += 1024)
            {
                int y;
                if ((i + 1023) > data.Length)
                {
                    y = data.Length - i;
                }
                else
                {
                    y = 1023;
                }
                string message = data.Substring(i, y);
                client.Self.InstantMessage(fromAgentID, message);
            }
        }

        private void updateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (Command c in Commands.Values)
                if (c.Active)
                    c.Think();
        }

        private void AgentDataUpdateHandler(Packet packet, Simulator sim)
        {
            AgentDataUpdatePacket p = (AgentDataUpdatePacket)packet;
            if (p.AgentData.AgentID == sim.Client.Self.AgentID)
            {
                Console.WriteLine("Got the group ID for " + sim.Client.ToString() + ", requesting group members...");
                GroupID = p.AgentData.ActiveGroupID;

                sim.Client.Groups.RequestGroupMembers(GroupID);
            }
        }

        private void GroupMembersHandler(Dictionary<Guid, GroupMember> members)
        {
            Console.WriteLine("Got " + members.Count + " group members.");
            GroupMembers = members;
        }

        private void AvatarAppearanceHandler(Packet packet, Simulator simulator)
        {
            AvatarAppearancePacket appearance = (AvatarAppearancePacket)packet;

            lock (Appearances) Appearances[appearance.Sender.ID] = appearance;
        }

        private void AlertMessageHandler(Packet packet, Simulator simulator)
        {
            AlertMessagePacket message = (AlertMessagePacket)packet;

            Logger.Log("[AlertMessage] " + Utils.BytesToString(message.AlertData.Message), Helpers.LogLevel.Info, this);
        }

        private void Self_OnInstantMessage(InstantMessage im, Simulator simulator)
        {
            bool groupIM = im.GroupIM && GroupMembers != null && GroupMembers.ContainsKey(im.FromAgentID) ? true : false;

            if (im.FromAgentID == MasterKey || (GroupCommands && groupIM))
            {
                // Received an IM from someone that is authenticated
                Console.WriteLine("<{0} ({1})> {2}: {3} (@{4}:{5})", im.GroupIM ? "GroupIM" : "IM", im.Dialog, im.FromAgentName, im.Message, im.RegionID, im.Position);

                if (im.Dialog == InstantMessageDialog.RequestTeleport)
                {
                    Console.WriteLine("Accepting teleport lure.");
                    Self.TeleportLureRespond(im.FromAgentID, true);
                }
                else if (
                    im.Dialog == InstantMessageDialog.MessageFromAgent ||
                    im.Dialog == InstantMessageDialog.MessageFromObject)
                {
                    ClientManagerRef.ClientManager.DoCommandAll(im.Message, im.FromAgentID);
                }
            }
            else
            {
                // Received an IM from someone that is not the bot's master, ignore
                Console.WriteLine("<{0} ({1})> {2} (not master): {3} (@{4}:{5})", im.GroupIM ? "GroupIM" : "IM", im.Dialog, im.FromAgentName, im.Message,
                    im.RegionID, im.Position);
                return;
            }
        }

        private bool Inventory_OnInventoryObjectReceived(InstantMessage offer, AssetType type,
            Guid objectID, bool fromTask)
        {
            if (MasterKey != Guid.Empty)
            {
                if (offer.FromAgentID != MasterKey)
                    return false;
            }
            else if (GroupMembers != null && !GroupMembers.ContainsKey(offer.FromAgentID))
            {
                return false;
            }

            return true;
        }
    }
}
