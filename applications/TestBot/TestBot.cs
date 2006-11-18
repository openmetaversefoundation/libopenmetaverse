using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace TestBot
{
    public class TestBot
    {
        public SecondLife Client;
        public LLUUID GroupID;
        public Dictionary<LLUUID, GroupMember> GroupMembers;
        public Dictionary<uint, PrimObject> Prims = new Dictionary<uint,PrimObject>();
        public Dictionary<uint, Avatar> Avatars = new Dictionary<uint,Avatar>();
        public Dictionary<string, Command> Commands = new Dictionary<string,Command>();
        public bool Running = true;
		public string FirstName;
		public string LastName;

        LLQuaternion bodyRotation;
        System.Timers.Timer updateTimer;

        public TestBot(string first, string last, string password)
        {
            Client = new SecondLife();

			FirstName = first;
			LastName = last;

            GroupID = LLUUID.Zero;
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

			RegisterAllCommands(Assembly.GetExecutingAssembly());

            if (Client.Network.Login(first, last, password, "TestBot", "contact@libsecondlife.org"))
            {
                updateTimer.Start();
            }
        }

		public void RegisterAllCommands(Assembly assembly)
		{
			foreach (Type t in assembly.GetTypes())
			{
				if (t.IsSubclassOf(typeof(Command)))
				{
					Command command = (Command)t.GetConstructor(new Type[0]).Invoke(new object[0]);
					RegisterCommand(command);
				}
			}
		}

        public void RegisterCommand(Command command)
        {
			command.Bot = this;
			Commands.Add(command.Name.ToLower(), command);
        }

        public void DoCommand(string cmd, LLUUID fromAgentID, LLUUID imSessionID)
        {
            string[] tokens = cmd.Trim().Split(new char[] { ' ', '\t' });
            if (tokens.Length == 0)
                return;

            string response = "";
            if (Commands.ContainsKey(tokens[0].ToLower()))
            {
                string[] args = new string[tokens.Length - 1];
                Array.Copy(tokens, 1, args, 0, args.Length);
                response = Commands[tokens[0].ToLower()].Execute(args, fromAgentID);
            }
            else
            {
                response = "Unknown command.";
            }

            if (response.Length > 0)
            {
                if (fromAgentID != null)
                    Client.Self.InstantMessage(fromAgentID, response, imSessionID);
                Console.WriteLine(response);
            }
        }

		public void Run()
		{
            Console.WriteLine("Type quit to exit.  Type help for a command list.");

            while (Running && Client.Network.Connected)
            {
				PrintPrompt();
                string input = Console.ReadLine();
                DoCommand(input, null, null);
            }

            if (Client.Network.Connected)
                Client.Network.Logout();
		}

		public void PrintPrompt()
		{
			Console.Write(String.Format("{0} {1} - {2}> ", FirstName, LastName, Client.Network.CurrentSim.Region.Name));
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
                GroupID = p.AgentData.ActiveGroupID;

                Client.Groups.BeginGetGroupMembers(GroupID, new GroupManager.GroupMembersCallback(OnGroupMembers));
            }
        }

        void OnGroupMembers(Dictionary<LLUUID,GroupMember> members)
        {
            Console.WriteLine("Got " + members.Count + " group members.");
            GroupMembers = members;
			PrintPrompt();
        }

        void Objects_OnObjectKilled(Simulator simulator, uint objectID)
        {
            lock (Prims)
            {
                if (Prims.ContainsKey(objectID))
                    Prims.Remove(objectID);
            }

            lock (Avatars)
            {
                if (Avatars.ContainsKey(objectID))
                    Avatars.Remove(objectID);
            }
        }

        void Objects_OnPrimMoved(Simulator simulator, PrimUpdate prim, ulong regionHandle, ushort timeDilation)
        {
            lock (Prims)
            {
                if (Prims.ContainsKey(prim.LocalID))
                {
                    Prims[prim.LocalID].Position = prim.Position;
                    Prims[prim.LocalID].Rotation = prim.Rotation;
                }
            }
        }

        void Objects_OnNewPrim(Simulator simulator, PrimObject prim, ulong regionHandle, ushort timeDilation)
        {
            lock (Prims)
            {
                Prims[prim.LocalID] = prim;
            }
        }

        void Objects_OnNewAvatar(Simulator simulator, Avatar avatar, ulong regionHandle, ushort timeDilation)
        {
            lock (Avatars)
            {
                Avatars[avatar.LocalID] = avatar;
            }
        }

        void Objects_OnAvatarMoved(Simulator simulator, AvatarUpdate avatar, ulong regionHandle, ushort timeDilation)
        {
            lock (Avatars)
            {
                if (Avatars.ContainsKey(avatar.LocalID))
                {
                    Avatars[avatar.LocalID].Position = avatar.Position;
                    Avatars[avatar.LocalID].Rotation = avatar.Rotation;
                }
            }
        }

        void Self_OnInstantMessage(LLUUID fromAgentID, string fromAgentName, LLUUID toAgentID, uint parentEstateID, LLUUID regionID, LLVector3 position, byte dialog, bool groupIM, LLUUID imSessionID, DateTime timestamp, string message, byte offline, byte[] binaryBucket)
        {
            Console.WriteLine("<IM>" + fromAgentName + ": " + message + "\n");

            if (GroupMembers != null && !GroupMembers.ContainsKey(fromAgentID))
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
