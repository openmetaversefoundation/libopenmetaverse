using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Threading;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class LoginDetails
    {
        public string FirstName;
        public string LastName;
        public string Password;
        public string StartLocation;
        public bool GroupCommands;
        public string MasterName;
        public LLUUID MasterKey;
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
        public Dictionary<LLUUID, SecondLife> Clients = new Dictionary<LLUUID, SecondLife>();
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

            LoginParams loginParams = client.Network.DefaultLoginParams(
                    account.FirstName, account.LastName, account.Password, "TestClient", version);

            if (!String.IsNullOrEmpty(account.StartLocation))
                loginParams.Start = account.StartLocation;

            if (!String.IsNullOrEmpty(account.URI))
                loginParams.URI = account.URI;
            
            if (client.Network.Login(loginParams))
            {
                if (account.MasterKey == LLUUID.Zero && !String.IsNullOrEmpty(account.MasterName))
                {
                    // To prevent security issues, we must resolve the specified master name to a key.
                    ManualResetEvent keyResolution = new ManualResetEvent(false);
                    List<DirectoryManager.AgentSearchData> masterMatches = new List<DirectoryManager.AgentSearchData>();
                    
                    // Set up the callback that handles the search results:
                    DirectoryManager.DirPeopleReplyCallback callback = 
                        delegate (LLUUID queryID, List<DirectoryManager.AgentSearchData> matches) {
                            // This may be called several times with additional search results.
                            if (matches.Count > 0)
                            {
                                lock (masterMatches)
                                {
                                    masterMatches.AddRange(matches);
                                }
                            }
                            else
                            {
                                // No results to show.
                                keyResolution.Set();
                            }
                        };
                    // Find master's key from name
                    Console.WriteLine("Resolving {0}'s UUID", account.MasterName);
                    client.Directory.OnDirPeopleReply += callback;
                    client.Directory.StartPeopleSearch(DirectoryManager.DirFindFlags.People, account.MasterName, 0);
                    keyResolution.WaitOne(TimeSpan.FromSeconds(30), false);
                    client.Directory.OnDirPeopleReply -= callback;

                    LLUUID masterKey = LLUUID.Zero;
                    string masterName = account.MasterName;
                    lock (masterMatches) {
                        if (masterMatches.Count == 1) {
                            masterKey = masterMatches[0].AgentID;
                            masterName = masterMatches[0].FirstName + " " + masterMatches[0].LastName;
                        } else if (masterMatches.Count > 0) {
                            // Print out numbered list of masters:
                            Console.WriteLine("Possible masters:");
                            for (int i = 0; i < masterMatches.Count; ++i)
                            {
                                Console.WriteLine("{0}: {1}", i, masterMatches[i].FirstName + " " + masterMatches[i].LastName);
                            }
                            Console.Write("Ambiguous master, choose one: ");
                            // Read number from the console:
                            string read = null;
                            do
                            {
                                read = Console.ReadLine();
                                int choice = 0;
                                if (int.TryParse(read, out choice))
                                {
                                    if (choice == -1)
                                    {
                                        break;
                                    } 
                                    else if (choice < masterMatches.Count)
                                    {
                                        masterKey = masterMatches[choice].AgentID;
                                        masterName = masterMatches[choice].FirstName + " " + masterMatches[choice].LastName;
                                        break;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Please type a number from the above list, -1 to cancel.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("You didn't type a number.");
                                    Console.WriteLine("Please type a number from the above list, -1 to cancel.");
                                }
                            } while (read != null); // Do it until the user selects a master.
                        }
                    }
                    if (masterKey != LLUUID.Zero)
                    {
                        Console.WriteLine("\"{0}\" resolved to {1} ({2})", account.MasterName, masterName, masterKey);
                        account.MasterName = masterName;
                        account.MasterKey = masterKey;
                    }
                    else
                    {
                        Console.WriteLine("Unable to obtain UUID for \"{0}\". No master will be used. Try specifying a key with --masterkey.", account.MasterName);
                    }
                }

                client.MasterKey = account.MasterKey;
                Clients[client.Self.AgentID] = client;

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
                DoCommandAll(input, LLUUID.Zero);
            }

            foreach (SecondLife client in Clients.Values)
            {
                if (client.Network.Connected)
                    client.Network.Logout();
            }
        }

        private void PrintPrompt()
        {
            int online = 0;

            foreach (SecondLife client in Clients.Values)
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
        public void DoCommandAll(string cmd, LLUUID fromAgentID)
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
                Dictionary<LLUUID, SecondLife> clientsCopy = new Dictionary<LLUUID, SecondLife>(Clients);

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
            Dictionary<LLUUID, SecondLife> clientsCopy = new Dictionary<LLUUID, SecondLife>(Clients);

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
