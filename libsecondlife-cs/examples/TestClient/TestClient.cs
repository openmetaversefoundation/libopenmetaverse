using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using libsecondlife;
using libsecondlife.Packets;
using libsecondlife.AssetSystem;

namespace libsecondlife.TestClient
{
    public struct LoginDetails
    {
        public string FirstName;
        public string LastName;
        public string Password;
    }

    public class TestClient
    {
        public Dictionary<LLUUID, SecondLife> Clients = new Dictionary<LLUUID, SecondLife>();
        public LLUUID GroupID = LLUUID.Zero;
        public Dictionary<LLUUID, GroupMember> GroupMembers;
        public Dictionary<Simulator, Dictionary<uint, PrimObject>> SimPrims = new Dictionary<Simulator, Dictionary<uint, PrimObject>>();
        public Dictionary<LLUUID, AvatarAppearancePacket> Appearances = new Dictionary<LLUUID, AvatarAppearancePacket>();
        public Dictionary<uint, Avatar> Avatars = new Dictionary<uint,Avatar>();
        public Dictionary<string, Command> Commands = new Dictionary<string,Command>();
        public Dictionary<string, object> SharedValues = new Dictionary<string, object>();
        public bool Running = true;
	    public string Master = "";

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
        /// <param name="accounts"></param>
        public TestClient(List<LoginDetails> accounts)
        {
            updateTimer = new System.Timers.Timer(1000);
            updateTimer.Elapsed += new System.Timers.ElapsedEventHandler(updateTimer_Elapsed);

            RegisterAllCommands(Assembly.GetExecutingAssembly());

            foreach (LoginDetails account in accounts)
            {
                SecondLife client = InitializeClient(account);

                if (client.Network.Connected)
                {
                    Clients[client.Network.AgentID] = client;

                    Console.WriteLine("Logged in " + client.ToString());

                    // Throttle the connection to not receive LayerData or asset packets
                    client.Throttle.Total = 0.0f;
                    client.Throttle.Task = 1536000.0f;
                    client.Throttle.Set();
                }
                else
                {
                    Console.WriteLine("Failed to login " + account.FirstName + " " + account.LastName +
                        ": " + client.Network.LoginError);
                }
            }

            updateTimer.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public SecondLife InitializeClient(LoginDetails account)
        {
            SecondLife client = new SecondLife();

            client.Debug = false;

            client.Network.RegisterCallback(PacketType.AgentDataUpdate, new NetworkManager.PacketCallback(AgentDataUpdateHandler));

            client.Objects.OnNewPrim += new ObjectManager.NewPrimCallback(Objects_OnNewPrim);
            client.Objects.OnPrimMoved += new ObjectManager.PrimMovedCallback(Objects_OnPrimMoved);
            client.Objects.OnObjectKilled += new ObjectManager.KillObjectCallback(Objects_OnObjectKilled);
            client.Objects.OnNewAvatar += new ObjectManager.NewAvatarCallback(Objects_OnNewAvatar);
            client.Objects.OnAvatarMoved += new ObjectManager.AvatarMovedCallback(Objects_OnAvatarMoved);
            client.Self.OnInstantMessage += new InstantMessageCallback(Self_OnInstantMessage);

            client.Network.RegisterCallback(PacketType.AvatarAppearance, new NetworkManager.PacketCallback(AvatarAppearanceHandler));

            client.Objects.RequestAllObjects = true;

            bool login = client.Network.Login(account.FirstName, account.LastName, account.Password, 
                "TestClient", "contact@libsecondlife.org");

            if (login)
            {
                // Set our appearance
                //AppearanceManager appearance = new AppearanceManager(client);
                //appearance.SendAgentSetAppearance();
            }

            return client;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Run()
        {
            Console.WriteLine("Type quit to exit.  Type help for a command list.");

            while (Running)
            {
                PrintPrompt();
                string input = Console.ReadLine();
                DoCommandAll(input, null, null);
            }

            foreach (SecondLife client in Clients.Values)
            {
                if (client.Network.Connected)
                    client.Network.Logout();
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
        private void DoCommand(SecondLife client, string cmd, LLUUID fromAgentID, LLUUID imSessionID)
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

                DoCommandAll(cmd, fromAgentID, imSessionID);

                return;
            }

            if (Commands.ContainsKey(firstToken))
            {
                string[] args = new string[tokens.Length - 1];
                Array.Copy(tokens, 1, args, 0, args.Length);
                string response = response = Commands[firstToken].Execute(client, args, fromAgentID);

                if (response.Length > 0)
                {
                    if (fromAgentID != null && client.Network.Connected) 
                        SendResponseIM(client, fromAgentID, response, imSessionID);
                        
                    Console.WriteLine(response);
                }
            }
        }

        private void DoCommandAll(string cmd, LLUUID fromAgentID, LLUUID imSessionID)
        {
            string[] tokens = cmd.Trim().Split(new char[] { ' ', '\t' });
            string firstToken = tokens[0].ToLower();

            if (tokens.Length == 0)
                return;

Begin:

            int avatars = Clients.Count;

            if (Commands.ContainsKey(firstToken))
            {
                if (firstToken == "login")
                {
                    // Special login case: Only call it once, and allow it with
                    // no logged in avatars
                    string[] args = new string[tokens.Length - 1];
                    Array.Copy(tokens, 1, args, 0, args.Length);
                    string response = Commands["login"].Execute(null, args, null);

                    if (response.Length > 0)
                    {
                        Console.WriteLine(response);
                    }
                }
                else if (firstToken == "quit")
                {
                    // Special quit case: This allows us to quit even when there
                    // are zero avatars logged in
                    Commands["quit"].Execute(null, null, null);
                }
                else
                {
                    foreach (SecondLife client in Clients.Values)
                    {
                        if (client.Network.Connected)
                        {
                            string[] args = new string[tokens.Length - 1];
                            Array.Copy(tokens, 1, args, 0, args.Length);
                            string response = Commands[firstToken].Execute(client, args, fromAgentID);

                            if (response.Length > 0)
                            {
                                if (fromAgentID != null && client.Network.Connected)
                                    SendResponseIM(client, fromAgentID, response, imSessionID);
                                Console.WriteLine(response);
                            }
                        }

                        if (avatars != Clients.Count)
                        {
                            // The dictionary size changed, start over since the 
                            // foreach is shot
                            goto Begin;
                        }
                    }
                }
            }
        }

		private void PrintPrompt()
		{
			//Console.Write(String.Format("{0} {1} - {2}> ", client.Self.FirstName, client.Self.LastName, client.Network.CurrentSim.Region.Name));

            int online = 0;

            foreach (SecondLife client in Clients.Values)
            {
                if (client.Network.Connected) online++;
            }

            Console.Write(online + " avatars online> ");
		}

        private void updateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (SecondLife client in Clients.Values)
            {
                client.Self.UpdateCamera(0, client.Self.Position, forward, left, up, bodyRotation,
                    LLQuaternion.Identity, DrawDistance, false);

                foreach (Command c in Commands.Values)
                    if (c.Active)
                        c.Think(client);
            }
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
			PrintPrompt();
        }

        private void Objects_OnObjectKilled(Simulator simulator, uint objectID)
        {
            lock (SimPrims)
            {
                if (SimPrims.ContainsKey(simulator) && SimPrims[simulator].ContainsKey(objectID))
                    SimPrims[simulator].Remove(objectID);
            }

            lock (Avatars)
            {
                if (Avatars.ContainsKey(objectID))
                    Avatars.Remove(objectID);
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
            lock (Avatars)
            {
                Avatars[avatar.LocalID] = avatar;
            }
        }

        private void Objects_OnAvatarMoved(Simulator simulator, AvatarUpdate avatar, ulong regionHandle, ushort timeDilation)
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

            if (Clients.ContainsKey(toAgentID))
            {
                if (dialog == 22)
                {
                    Console.WriteLine("Accepting teleport lure");
                    Clients[toAgentID].Self.TeleportLureRespond(fromAgentID, true);
                }
                else
                {
                    DoCommand(Clients[toAgentID], message, fromAgentID, imSessionID);
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
