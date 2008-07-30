using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class LoginDetails
    {
        public string FirstName;
        public string LastName;
        public string Password;
        public string StartLocation;
        public bool GroupCommands;
        public string MasterName;
        public UUID MasterKey;
        public string URI;
    }

    public class StartPosition
    {
        public string sim;
        public int x;
        public int y;
        public int z;

        public StartPosition()
        {
            this.sim = null;
            this.x = 0;
            this.y = 0;
            this.z = 0;
        }
    }

    public class ClientManager
    {
        public Dictionary<UUID, GridClient> Clients = new Dictionary<UUID, GridClient>();
        public Dictionary<Simulator, Dictionary<uint, Primitive>> SimPrims = new Dictionary<Simulator, Dictionary<uint, Primitive>>();

        public bool Running = true;

        string version = "1.0.0";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accounts"></param>
        public ClientManager(List<LoginDetails> accounts)
        {
            foreach (LoginDetails account in accounts)
                Login(account);
        }

        public ClientManager(List<LoginDetails> accounts, string s)
        {
            char sep = '/';
            string[] startbits = s.Split(sep);

            foreach (LoginDetails account in accounts)
            {
                account.StartLocation = NetworkManager.StartLocation(startbits[0], Int32.Parse(startbits[1]),
                    Int32.Parse(startbits[2]), Int32.Parse(startbits[3]));
                Login(account);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public TestClient Login(LoginDetails account)
        {
            // Check if this client is already logged in
            foreach (TestClient c in Clients.Values)
            {
                if (c.Self.FirstName == account.FirstName && c.Self.LastName == account.LastName)
                {
                    Logout(c);
                    break;
                }
            }

            TestClient client = new TestClient(this);

            // Optimize the throttle
            client.Throttle.Wind = 0;
            client.Throttle.Cloud = 0;
            client.Throttle.Land = 1000000;
            client.Throttle.Task = 1000000;

            client.GroupCommands = account.GroupCommands;
			client.MasterName = account.MasterName;
            client.MasterKey = account.MasterKey;
            client.AllowObjectMaster = client.MasterKey != UUID.Zero; // Require UUID for object master.

            LoginParams loginParams = client.Network.DefaultLoginParams(
                    account.FirstName, account.LastName, account.Password, "TestClient", version);

            if (!String.IsNullOrEmpty(account.StartLocation))
                loginParams.Start = account.StartLocation;

            if (!String.IsNullOrEmpty(account.URI))
                loginParams.URI = account.URI;
            
            if (client.Network.Login(loginParams))
            {
                Clients[client.Self.AgentID] = client;
                if (client.MasterKey == UUID.Zero)
                {
                    UUID query = UUID.Random();
                    client.Directory.OnDirPeopleReply +=
                        delegate(UUID queryID, List<DirectoryManager.AgentSearchData> matchedPeople)
                        {
                            if (queryID != query)
                                return;
                            if (matchedPeople.Count != 1)
                                Console.WriteLine("Unable to resolve master key.");
                            else
                                client.MasterKey = matchedPeople[0].AgentID;
                        };
                    client.Directory.StartPeopleSearch(DirectoryManager.DirFindFlags.People, client.MasterName, 0, query);
                }
                Console.WriteLine("Logged in " + client.ToString());
            }
            else
            {
                Console.WriteLine("Failed to login " + account.FirstName + " " + account.LastName + ": " +
                    client.Network.LoginMessage);
            }

            return client;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public TestClient Login(string[] args)
        {
            LoginDetails account = new LoginDetails();
            account.FirstName = args[0];
            account.LastName = args[1];
            account.Password = args[2];

            if (args.Length == 4)
            {
                account.StartLocation = NetworkManager.StartLocation(args[3], 128, 128, 40);
            }

            return Login(account);
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
                DoCommandAll(input, UUID.Zero);
            }

            foreach (GridClient client in Clients.Values)
            {
                if (client.Network.Connected)
                    client.Network.Logout();
            }
        }

        private void PrintPrompt()
        {
            int online = 0;

            foreach (GridClient client in Clients.Values)
            {
                if (client.Network.Connected) online++;
            }

            Console.Write(online + " avatars online> ");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="fromAgentID"></param>
        /// <param name="imSessionID"></param>
        public void DoCommandAll(string cmd, UUID fromAgentID)
        {
            string[] tokens = cmd.Trim().Split(new char[] { ' ', '\t' });
            string firstToken = tokens[0].ToLower();

            if (tokens.Length == 0)
                return;

            if (firstToken == "login")
            {
                // Special login case: Only call it once, and allow it with
                // no logged in avatars
                string[] args = new string[tokens.Length - 1];
                Array.Copy(tokens, 1, args, 0, args.Length);
                Login(args);
            }
            else if (firstToken == "quit")
            {
                Quit();
                Console.WriteLine("All clients logged out and program finished running.");
            }
            else
            {
                // make a copy of the clients list so that it can be iterated without fear of being changed during iteration
                Dictionary<UUID, GridClient> clientsCopy = new Dictionary<UUID, GridClient>(Clients);

                foreach (TestClient client in clientsCopy.Values)
                    client.DoCommand(cmd, fromAgentID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public void Logout(TestClient client)
        {
            Clients.Remove(client.Self.AgentID);
            client.Network.Logout();
        }

        /// <summary>
        /// 
        /// </summary>
        public void LogoutAll()
        {
            // make a copy of the clients list so that it can be iterated without fear of being changed during iteration
            Dictionary<UUID, GridClient> clientsCopy = new Dictionary<UUID, GridClient>(Clients);

            foreach (TestClient client in clientsCopy.Values)
                Logout(client);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Quit()
        {
            LogoutAll();
            Running = false;
            // TODO: It would be really nice if we could figure out a way to abort the ReadLine here in so that Run() will exit.
        }
    }
}
