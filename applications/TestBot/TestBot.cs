using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace TestBot
{
    class TestBot
    {
        SecondLife Client;
        LLUUID myGroupID;
        Dictionary<LLUUID, GroupMember> myGroupMembers;
        Dictionary<uint, PrimObject> prims;
        Dictionary<uint, Avatar> avatars;

        LLQuaternion bodyRotation;
        System.Timers.Timer updateTimer;

        public bool running = true;

        public delegate string CommandHandler(string[] args);
        Dictionary<string, CommandHandler> commands;

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: TestBot.exe [firstname] [lastname] [password]");
                return;
            }

            TestBot bot = new TestBot(args[0], args[1], args[2]);
            Console.WriteLine("Type quit to exit");
            string input = "";

            while (bot.running && bot.Client.Network.Connected)
            {
                input = Console.ReadLine();
                bot.DoCommand(input, null, null);
            }

            if (bot.Client.Network.Connected)
            {
                bot.Client.Network.Logout();
            }
        }

        public TestBot(string first, string last, string password)
        {
            Client = new SecondLife();

            myGroupID = LLUUID.Zero;
            prims = new Dictionary<uint, PrimObject>();
            avatars = new Dictionary<uint, Avatar>();
            commands = new Dictionary<string, CommandHandler>();

            Client.Objects.RequestAllObjects = true;
            bodyRotation = LLQuaternion.Identity;
            updateTimer = new System.Timers.Timer(500);
            updateTimer.Elapsed += new System.Timers.ElapsedEventHandler(updateTimer_Elapsed);

            Client.Network.RegisterCallback(PacketType.AgentDataUpdate, new NetworkManager.PacketCallback(AgentDataUpdateHandler));

            Client.Objects.OnNewPrim += new ObjectManager.NewPrimCallback(Objects_OnNewPrim);
            Client.Objects.OnPrimMoved += new ObjectManager.PrimMovedCallback(Objects_OnPrimMoved);
            Client.Objects.OnObjectKilled += new ObjectManager.KillObjectCallback(Objects_OnObjectKilled);
            Client.Objects.OnNewAvatar += new ObjectManager.NewAvatarCallback(Objects_OnNewAvatar);
            Client.Objects.OnAvatarMoved += new ObjectManager.AvatarMovedCallback(Objects_OnAvatarMoved);
            Client.Self.OnInstantMessage += new InstantMessageCallback(Self_OnInstantMessage);

            RegisterCommand("quit", new CommandHandler(QuitCmd));
            RegisterCommand("tree", new CommandHandler(TreeCmd));
            RegisterCommand("location", new CommandHandler(LocationCmd));

            if (Client.Network.Login(first, last, password, "TestBot", "contact@libsecondlife.org"))
            {
                updateTimer.Start();
            }
        }

        public void RegisterCommand(string commandName, CommandHandler commandHandler)
        {
            commands[commandName] = commandHandler;
        }

        public void DoCommand(string cmd, LLUUID fromAgentID, LLUUID imSessionID)
        {
            string[] tokens = cmd.Trim().Split(new char[] { ' ', '\t' });
            if (tokens.Length == 0)
                return;

            string response = "";
            if (commands.ContainsKey(tokens[0]))
            {
                string[] args = new string[tokens.Length - 1];
                Array.Copy(tokens, 1, args, 0, args.Length);
                response = commands[tokens[0]].Invoke(args);
            }
            else
            {
                response = "Unknown command.";
            }

            if (response.Length > 0)
            {
                if (fromAgentID != null)
                {
                    Client.Self.InstantMessage(fromAgentID, response, imSessionID);
                }
                else
                {
                    Console.WriteLine(response);
                }
            }
        }

        string QuitCmd(string[] args)
        {
            running = false;
            Client.Network.Logout();
            return "Logging off.";
        }

        string LocationCmd(string[] args)
        {
            return "CurrentSim: '" + Client.Network.CurrentSim.Region.Name + "' Position: " + Client.Self.Position.ToString();
        }

        string TreeCmd(string[] args)
        {
            if (args.Length == 1)
            {
                try
                {
                    string treeName = args[0].Trim(new char[] { ' ' });
                    ObjectManager.Tree tree = (ObjectManager.Tree)Enum.Parse(typeof(ObjectManager.Tree), treeName);

                    LLVector3 treePosition = new LLVector3(Client.Self.Position.X, Client.Self.Position.Y,
                        Client.Self.Position.Z);
                    treePosition.Z += 3.0f;

                    Client.Objects.AddTree(Client.Network.CurrentSim, new LLVector3(0.5f, 0.5f, 0.5f),
                        LLQuaternion.Identity, treePosition, tree, myGroupID, false);

                    return "Attempted to rez a " + treeName + " tree";
                }
                catch (Exception)
                {
                    return "Type !tree for usage";
                }
            }

            string usage = "Usage: !tree [";
            foreach (string value in Enum.GetNames(typeof(ObjectManager.Tree)))
            {
                usage += value + ",";
            }
            usage = usage.TrimEnd(new char[] { ',' });
            usage += "]";
            return usage;
        }

        void updateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            LLVector3 forward = new LLVector3(0, 0.9999f, 0);
            LLVector3 left = new LLVector3(0.9999f, 0, 0);
            LLVector3 up = new LLVector3(0, 0, 0.9999f);

            Client.Self.UpdateCamera(0, Client.Self.Position, forward, left, up, bodyRotation,
                LLQuaternion.Identity, 64, false);
        }

        void AgentDataUpdateHandler(Packet packet, Simulator sim)
        {
            AgentDataUpdatePacket p = (AgentDataUpdatePacket)packet;
            if (p.AgentData.AgentID == Client.Network.AgentID)
            {
                Console.WriteLine("Got my group ID, requesting group members...");
                myGroupID = p.AgentData.ActiveGroupID;

                Client.Groups.BeginGetGroupMembers(myGroupID, new GroupManager.GroupMembersCallback(OnGroupMembers));
            }
        }

        void OnGroupMembers(Dictionary<LLUUID,GroupMember> members)
        {
            Console.WriteLine("Got " + members.Count + " group members.");
            myGroupMembers = members;
        }

        void Objects_OnObjectKilled(Simulator simulator, uint objectID)
        {
            lock (prims)
            {
                if (prims.ContainsKey(objectID))
                    prims.Remove(objectID);
            }

            lock (avatars)
            {
                if (avatars.ContainsKey(objectID))
                    avatars.Remove(objectID);
            }
        }

        void Objects_OnPrimMoved(Simulator simulator, PrimUpdate prim, ulong regionHandle, ushort timeDilation)
        {
            lock (prims)
            {
                if (prims.ContainsKey(prim.LocalID))
                {
                    prims[prim.LocalID].Position = prim.Position;
                    prims[prim.LocalID].Rotation = prim.Rotation;
                }
            }
        }

        void Objects_OnNewPrim(Simulator simulator, PrimObject prim, ulong regionHandle, ushort timeDilation)
        {
            lock (prims)
            {
                prims[prim.LocalID] = prim;
            }
        }

        void Objects_OnNewAvatar(Simulator simulator, Avatar avatar, ulong regionHandle, ushort timeDilation)
        {
            lock (avatars)
            {
                avatars[avatar.LocalID] = avatar;
            }
        }

        void Objects_OnAvatarMoved(Simulator simulator, AvatarUpdate avatar, ulong regionHandle, ushort timeDilation)
        {
            lock (avatars)
            {
                if (avatars.ContainsKey(avatar.LocalID))
                {
                    avatars[avatar.LocalID].Position = avatar.Position;
                    avatars[avatar.LocalID].Rotation = avatar.Rotation;
                }
            }
        }

        void Self_OnInstantMessage(LLUUID fromAgentID, string fromAgentName, LLUUID toAgentID, uint parentEstateID, LLUUID regionID, LLVector3 position, byte dialog, bool groupIM, LLUUID imSessionID, DateTime timestamp, string message, byte offline, byte[] binaryBucket)
        {
            Console.WriteLine("<IM>" + fromAgentName + ": " + message + "\n");

            if (myGroupMembers != null && !myGroupMembers.ContainsKey(fromAgentID))
            {
                //Not a member of my group, ignore the IM.
                return;
            }

            if (dialog == 22)
            {
                Console.WriteLine("Accepting teleport lure.");
                Client.Self.TeleportLureRespond(fromAgentID, true);

                return;
            }

            DoCommand(message, fromAgentID, imSessionID);
        }
    }
}
