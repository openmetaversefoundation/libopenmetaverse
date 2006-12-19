using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using libsecondlife;
using libsecondlife.Packets;
using libsecondlife.AssetSystem;

namespace libsecondlife.TestClient
{
    public class TestClient : SecondLife
    {
        public Dictionary<Simulator, Dictionary<uint, PrimObject>> SimPrims;
        public LLUUID GroupID = LLUUID.Zero;
        public Dictionary<LLUUID, GroupMember> GroupMembers;
        public Dictionary<uint, Avatar> AvatarList = new Dictionary<uint,Avatar>();
		public Dictionary<LLUUID, AvatarAppearancePacket> Appearances = new Dictionary<LLUUID, AvatarAppearancePacket>();
		public Dictionary<string, Command> Commands = new Dictionary<string,Command>();
        public Dictionary<string, object> SharedValues = new Dictionary<string, object>();
        public bool Running = true;
	    public string Master = "";
		public ClientManager ClientManager;

        public delegate void PrimCreatedCallback(Simulator simulator, PrimObject prim);
        public event PrimCreatedCallback OnPrimCreated;

        private LLQuaternion bodyRotation = LLQuaternion.Identity;
        private LLVector3 forward = new LLVector3(0, 0.9999f, 0);
        private LLVector3 left = new LLVector3(0.9999f, 0, 0);
        private LLVector3 up = new LLVector3(0, 0, 0.9999f);
        private int DrawDistance = 64;
        private System.Timers.Timer updateTimer;


        /// <summary>
        /// 
        /// </summary>
        public TestClient(ClientManager manager)
        {
			ClientManager = manager;

            updateTimer = new System.Timers.Timer(1000);
            updateTimer.Elapsed += new System.Timers.ElapsedEventHandler(updateTimer_Elapsed);

            RegisterAllCommands(Assembly.GetExecutingAssembly());


            Debug = false;

            Network.RegisterCallback(PacketType.AgentDataUpdate, new NetworkManager.PacketCallback(AgentDataUpdateHandler));

            Objects.OnNewPrim += new ObjectManager.NewPrimCallback(Objects_OnNewPrim);
            Objects.OnPrimMoved += new ObjectManager.PrimMovedCallback(Objects_OnPrimMoved);
            Objects.OnObjectKilled += new ObjectManager.KillObjectCallback(Objects_OnObjectKilled);
			Objects.OnNewAvatar += new ObjectManager.NewAvatarCallback(Objects_OnNewAvatar);
			Objects.OnAvatarMoved += new ObjectManager.AvatarMovedCallback(Objects_OnAvatarMoved);
            Self.OnInstantMessage += new InstantMessageCallback(Self_OnInstantMessage);

            Network.RegisterCallback(PacketType.AvatarAppearance, new NetworkManager.PacketCallback(AvatarAppearanceHandler));

            Objects.RequestAllObjects = true;


            updateTimer.Start();
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

        private void RegisterCommand(Command command)
        {
			if (!Commands.ContainsKey(command.Name.ToLower()))
			{
				command.TestClient = this;
				Commands.Add(command.Name.ToLower(), command);
			}
        }

        //breaks up large responses to deal with the max IM size
        private void SendResponseIM(SecondLife client, LLUUID fromAgentID, string data, LLUUID imSessionID)
        {
            for ( int i = 0 ; i < data.Length ; i += 1024 ) {
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
                client.Self.InstantMessage(fromAgentID, message, imSessionID);
            }
        }

		public void DoCommand(string cmd, LLUUID fromAgentID, LLUUID imSessionID)
        {
            string[] tokens = cmd.Trim().Split(new char[] { ' ', '\t' });
            string firstToken = tokens[0].ToLower();

            if (tokens.Length == 0)
                return;

            // "all balance" will send the balance command to all currently logged in bots
			if (firstToken == "all" && tokens.Length > 1)
			{
			    cmd = "";

			    // Reserialize all of the arguments except for "all"
			    for (int i = 1; i < tokens.Length; i++)
			    {
			        cmd += tokens[i] + " ";
			    }

			    ClientManager.DoCommandAll(cmd, fromAgentID, imSessionID);

			    return;
			}

            if (Commands.ContainsKey(firstToken))
            {
                string[] args = new string[tokens.Length - 1];
                Array.Copy(tokens, 1, args, 0, args.Length);
                string response = response = Commands[firstToken].Execute(this, args, fromAgentID);

                if (response.Length > 0)
                {
                    if (fromAgentID != null && Network.Connected) 
                        SendResponseIM(this, fromAgentID, response, imSessionID);
                        
                    Console.WriteLine(response);
                }
            }
        }

        private void updateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Self.UpdateCamera(0, Self.Position, forward, left, up, bodyRotation,
                LLQuaternion.Identity, DrawDistance, false);

            foreach (Command c in Commands.Values)
                if (c.Active)
                    c.Think(this);
        }

        private void AgentDataUpdateHandler(Packet packet, Simulator sim)
        {
            AgentDataUpdatePacket p = (AgentDataUpdatePacket)packet;
            if (p.AgentData.AgentID == sim.Client.Network.AgentID)
            {
                Console.WriteLine("Got the group ID for " + sim.Client.ToString() + ", requesting group members...");
                GroupID = p.AgentData.ActiveGroupID;

                sim.Client.Groups.BeginGetGroupMembers(GroupID, new GroupManager.GroupMembersCallback(OnGroupMembers));
            }
        }

        private void OnGroupMembers(Dictionary<LLUUID, GroupMember> members)
        {
            Console.WriteLine("Got " + members.Count + " group members.");
            GroupMembers = members;
        }

        private void Objects_OnObjectKilled(Simulator simulator, uint objectID)
        {
            lock (SimPrims)
            {
                if (SimPrims.ContainsKey(simulator) && SimPrims[simulator].ContainsKey(objectID))
                    SimPrims[simulator].Remove(objectID);
            }

			lock (AvatarList)
			{
			    if (AvatarList.ContainsKey(objectID))
			        AvatarList.Remove(objectID);
			}
        }

        private void Objects_OnPrimMoved(Simulator simulator, PrimUpdate prim, ulong regionHandle, ushort timeDilation)
        {
            lock (SimPrims)
            {
                if (SimPrims.ContainsKey(simulator) && SimPrims[simulator].ContainsKey(prim.LocalID))
                {
                    SimPrims[simulator][prim.LocalID].Position = prim.Position;
                    SimPrims[simulator][prim.LocalID].Rotation = prim.Rotation;
                }
            }
        }

        private void Objects_OnNewPrim(Simulator simulator, PrimObject prim, ulong regionHandle, ushort timeDilation)
        {
            lock (SimPrims)
            {
                if (!SimPrims.ContainsKey(simulator))
                {
                    SimPrims[simulator] = new Dictionary<uint, PrimObject>(10000);
                }

                SimPrims[simulator][prim.LocalID] = prim;
            }

            if ((prim.Flags & ObjectFlags.CreateSelected) != 0 && OnPrimCreated != null)
            {
                OnPrimCreated(simulator, prim);
            }
        }

		private void Objects_OnNewAvatar(Simulator simulator, Avatar avatar, ulong regionHandle, ushort timeDilation)
		{
		    lock (AvatarList)
		    {
		        AvatarList[avatar.LocalID] = avatar;
		    }
		}

		private void Objects_OnAvatarMoved(Simulator simulator, AvatarUpdate avatar, ulong regionHandle, ushort timeDilation)
		{
		    lock (AvatarList)
		    {
		        if (AvatarList.ContainsKey(avatar.LocalID))
		        {
		            AvatarList[avatar.LocalID].Position = avatar.Position;
		            AvatarList[avatar.LocalID].Rotation = avatar.Rotation;
		        }
		    }
		}

        private void AvatarAppearanceHandler(Packet packet, Simulator simulator)
        {
            AvatarAppearancePacket appearance = (AvatarAppearancePacket)packet;

            lock (Appearances) Appearances[appearance.Sender.ID] = appearance;
        }

        private void Self_OnInstantMessage(LLUUID fromAgentID, string fromAgentName, LLUUID toAgentID, uint parentEstateID, 
            LLUUID regionID, LLVector3 position, byte dialog, bool groupIM, LLUUID imSessionID, DateTime timestamp, 
            string message, byte offline, byte[] binaryBucket)
        {
            if (Master.Length > 0)
            {
                if (fromAgentName.ToLower().Trim() != Master.ToLower().Trim())
                {
                    // Received an IM from someone that is not the bot's master, ignore
                    Console.WriteLine("<IM>" + fromAgentName + " (not master): " + message + "@"  + regionID.ToString() + ":" + position.ToString() );
                    return;
                }
            }
            else
            {
                if (GroupMembers != null && !GroupMembers.ContainsKey(fromAgentID))
                {
                    // Received an IM from someone outside the bot's group, ignore
                    Console.WriteLine("<IM>" + fromAgentName + " (not in group): " + message + "@" + regionID.ToString() + ":" + position.ToString());
                    return;
                }
            }

            Console.WriteLine("<IM>" + fromAgentName + ": " + message);

            if (Self.ID == toAgentID)
            {
                if (dialog == 22)
                {
                    Console.WriteLine("Accepting teleport lure");
                    Self.TeleportLureRespond(fromAgentID, true);
                }
                else
                {
                    DoCommand(message, fromAgentID, imSessionID);
                }
            }
            else
            {
                // This shouldn't happen
                Console.WriteLine("A bot that we aren't tracking received an IM?");
            }
        }
	}
}