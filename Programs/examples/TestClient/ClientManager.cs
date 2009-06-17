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

    public sealed class ClientManager
    {
        const string VERSION = "1.0.0";

        class Singleton { internal static readonly ClientManager Instance = new ClientManager(); }
        public static ClientManager Instance { get { return Singleton.Instance; } }

        public Dictionary<UUID, TestClient> Clients = new Dictionary<UUID, TestClient>();
        public Dictionary<Simulator, Dictionary<uint, Primitive>> SimPrims = new Dictionary<Simulator, Dictionary<uint, Primitive>>();

        public bool Running = true;
        public bool GetTextures = false;
        public volatile int PendingLogins = 0;

        ClientManager()
        {
        }

        public void Start(List<LoginDetails> accounts, bool getTextures)
        {
            GetTextures = getTextures;

            foreach (LoginDetails account in accounts)
                Login(account);
        }

        public TestClient Login(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: login firstname lastname password [simname] [login server url]");
                return null;
            }
            LoginDetails account = new LoginDetails();
            account.FirstName = args[0];
            account.LastName = args[1];
            account.Password = args[2];

            if (args.Length > 3)
            {
                // If it looks like a full starting position was specified, parse it
                if (args[3].StartsWith("http"))
                {
                    account.URI = args[3];
                }
                else
                {
                    if (args[3].IndexOf('/') >= 0)
                    {
                        char sep = '/';
                        string[] startbits = args[3].Split(sep);
                        try
                        {
                            account.StartLocation = NetworkManager.StartLocation(startbits[0], Int32.Parse(startbits[1]),
                              Int32.Parse(startbits[2]), Int32.Parse(startbits[3]));
                        }
                        catch (FormatException) { }
                    }

                    // Otherwise, use the center of the named region
                    if (account.StartLocation == null)
                        account.StartLocation = NetworkManager.StartLocation(args[3], 128, 128, 40);
                }
            }

            if (args.Length > 4)
                if (args[4].StartsWith("http"))
                    account.URI = args[4];

            if (string.IsNullOrEmpty(account.URI))
                account.URI = Program.LoginURI;
            Logger.Log("Using login URI " + account.URI, Helpers.LogLevel.Info);

            return Login(account);
        }

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

            ++PendingLogins;

            TestClient client = new TestClient(this);
            client.Network.OnLogin +=
                delegate(LoginStatus login, string message)
                {
                    Logger.Log(String.Format("Login {0}: {1}", login, message), Helpers.LogLevel.Info, client);

                    if (login == LoginStatus.Success)
                    {
                        Clients[client.Self.AgentID] = client;

                        if (client.MasterKey == UUID.Zero)
                        {
                            UUID query = UUID.Random();
                            DirectoryManager.DirPeopleReplyCallback peopleDirCallback =
                                delegate(UUID queryID, List<DirectoryManager.AgentSearchData> matchedPeople)
                                {
                                    if (queryID == query)
                                    {
                                        if (matchedPeople.Count != 1)
                                        {
                                            Logger.Log("Unable to resolve master key from " + client.MasterName, Helpers.LogLevel.Warning);
                                        }
                                        else
                                        {
                                            client.MasterKey = matchedPeople[0].AgentID;
                                            Logger.Log("Master key resolved to " + client.MasterKey, Helpers.LogLevel.Info);
                                        }
                                    }
                                };

                            client.Directory.OnDirPeopleReply += peopleDirCallback;
                            client.Directory.StartPeopleSearch(DirectoryManager.DirFindFlags.People, client.MasterName, 0, query);
                        }

                        Logger.Log("Logged in " + client.ToString(), Helpers.LogLevel.Info);
                        --PendingLogins;
                    }
                    else if (login == LoginStatus.Failed)
                    {
                        Logger.Log("Failed to login " + account.FirstName + " " + account.LastName + ": " +
                            client.Network.LoginMessage, Helpers.LogLevel.Warning);
                        --PendingLogins;
                    }
                };

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
                    account.FirstName, account.LastName, account.Password, "TestClient", VERSION);

            if (!String.IsNullOrEmpty(account.StartLocation))
                loginParams.Start = account.StartLocation;

            if (!String.IsNullOrEmpty(account.URI))
                loginParams.URI = account.URI;

            client.Network.BeginLogin(loginParams);
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
            if (tokens.Length == 0)
                return;
            
            string firstToken = tokens[0].ToLower();
            if (String.IsNullOrEmpty(firstToken))
                return;

            // Allow for comments when cmdline begins with ';' or '#'
            if (firstToken[0] == ';' || firstToken[0] == '#')
                return;
            
            string[] args = new string[tokens.Length - 1];
            if (args.Length > 0)
                Array.Copy(tokens, 1, args, 0, args.Length);

            if (firstToken == "login")
            {
                Login(args);
            }
            else if (firstToken == "quit")
            {
                Quit();
                Logger.Log("All clients logged out and program finished running.", Helpers.LogLevel.Info);
            }
            else if (firstToken == "help")
            {
                if (Clients.Count > 0)
                {
                    foreach (TestClient client in Clients.Values)
                    {
                        Console.WriteLine(client.Commands["help"].Execute(args, UUID.Zero));
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("You must login at least one bot to use the help command");
                }
            }
            else if (firstToken == "script")
            {
                // No reason to pass this to all bots, and we also want to allow it when there are no bots
                ScriptCommand command = new ScriptCommand(null);
                Logger.Log(command.Execute(args, UUID.Zero), Helpers.LogLevel.Info);
            }
            else if (firstToken == "waitforlogin")
            {
                // Special exception to allow this to run before any bots have logged in
                if (ClientManager.Instance.PendingLogins > 0)
                {
                    WaitForLoginCommand command = new WaitForLoginCommand(null);
                    Logger.Log(command.Execute(args, UUID.Zero), Helpers.LogLevel.Info);
                }
                else
                {
                    Logger.Log("No pending logins", Helpers.LogLevel.Info);
                }
            }
            else
            {
                // Make an immutable copy of the Clients dictionary to safely iterate over
                Dictionary<UUID, TestClient> clientsCopy = new Dictionary<UUID, TestClient>(Clients);

                int completed = 0;

                foreach (TestClient client in clientsCopy.Values)
                {
                    ThreadPool.QueueUserWorkItem((WaitCallback)
                        delegate(object state)
                        {
                            TestClient testClient = (TestClient)state;
                            if (testClient.Commands.ContainsKey(firstToken))
                                Logger.Log(testClient.Commands[firstToken].Execute(args, fromAgentID),
                                    Helpers.LogLevel.Info, testClient);
                            else
                                Logger.Log("Unknown command " + firstToken, Helpers.LogLevel.Warning);

                            ++completed;
                        },
                        client);
                }

                while (completed < clientsCopy.Count)
                    Thread.Sleep(50);
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
        public void Quit()
        {
            Running = false;
            // TODO: It would be really nice if we could figure out a way to abort the ReadLine here in so that Run() will exit.
        }
    }
}
