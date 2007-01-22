using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using libsecondlife;
using libsecondlife.Packets;
using libsecondlife.AssetSystem;

namespace libsecondlife.TestClient
{
    public class LoginDetails
    {
        public string FirstName;
        public string LastName;
        public string Password;
		public string Master;
    }

    public class ClientManager
    {
        public Dictionary<LLUUID, SecondLife> Clients = new Dictionary<LLUUID, SecondLife>();
        public Dictionary<Simulator, Dictionary<uint, PrimObject>> SimPrims = new Dictionary<Simulator, Dictionary<uint, PrimObject>>();

        public bool Running = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accounts"></param>
        public ClientManager(List<LoginDetails> accounts)
        {
            foreach (LoginDetails account in accounts)
                Login(account);
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

			client.SimPrims = SimPrims;
			client.Master = account.Master;

            if ( ! client.Network.Login(account.FirstName, account.LastName, account.Password, "TestClient", "contact@libsecondlife.org") ) {
				Console.WriteLine("Failed to login " + account.FirstName + " " + account.LastName + ": " + client.Network.LoginError);
			}

            if (client.Network.Connected)
            {
                Clients[client.Network.AgentID] = client;

                Console.WriteLine("Logged in " + client.ToString());

                // Throttle the connection to not receive LayerData or asset packets
                client.Throttle.Total = 0.0f;
                client.Throttle.Task = 1536000.0f;
                client.Throttle.Set();
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
                DoCommandAll(input, null, null);
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
        public void DoCommandAll(string cmd, LLUUID fromAgentID, LLUUID imSessionID)
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
					client.DoCommand(cmd, fromAgentID, imSessionID);
			}
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="client"></param>
		public void Logout(TestClient client)
		{
            Clients.Remove(client.Network.AgentID);
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
