using System;
using System.Collections;

using libsecondlife;


namespace Teleport
{
    class Teleport
    {
        protected SecondLife client;
        protected bool DoneTeleporting = false;

        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: Teleport [loginfirstname] [loginlastname] [password] [sim/x/y/z]");
                return;
            }

            char[] seps = { '/' };
            string[] destination = args[3].Split(seps);
            if( destination.Length != 4 )
            {
                Console.WriteLine("Destination should be specified as: sim/x/y/z");
                return;
            }

            string sim = destination[0];
            float x = float.Parse(destination[1]);
            float y = float.Parse(destination[2]);
            float z = float.Parse(destination[3]);

            Console.WriteLine("Will attempt a teleport to " + sim + " {" + x + "," + y + "," + z + "}...");
            Console.WriteLine();

            Teleport app = new Teleport();
            app.Connect(args[0], args[1], args[2]);
            app.doStuff(sim, new LLVector3(x,y,z));
            app.Disconnect();

        }

        protected Teleport()
		{
            try
            {
                client = new SecondLife();
            }
            catch (Exception e)
            {
                // Error initializing the client
                Console.WriteLine();
                Console.WriteLine(e.ToString());
            }		
		}

        protected void Connect(string FirstName, string LastName, string Password)
        {
            Console.WriteLine("Attempting to connect and login to SecondLife.");

            // Setup Login to Second Life
            Hashtable loginParams = NetworkManager.DefaultLoginValues(FirstName, LastName, Password, "00:00:00:00:00:00",
                "last", 1, 12, 12, 12, "Win", "0", "createnotecard", "static.sprocket@gmail.com");
            Hashtable loginReply = new Hashtable();

            // Login
            if (!client.Network.Login(loginParams))
            {
                // Login failed
                Console.WriteLine("Error logging in: " + client.Network.LoginError);
                return;
            }

            // Login was successful
            Console.WriteLine("Login was successful.");
            Console.WriteLine("AgentID:   " + client.Network.AgentID);
            Console.WriteLine("SessionID: " + client.Network.SessionID);
        }

        protected void Disconnect()
        {
            // Logout of Second Life
            Console.WriteLine("Request logout");
            client.Network.Logout();
        }

        protected void doStuff(string sim, LLVector3 coords)
        {
            // Load up the list of estate simulators, incase we get a request to teleport to one.
            // This doesn't block, and there's no easy way to know when it's done, so we'll wait a bit
            // and hope for the best?
            client.Grid.AddEstateSims();

            System.Threading.Thread.Sleep(6000);
            Console.WriteLine();
            Console.WriteLine("Okay, hopefully all the initial connect stuff is done, trying now...");

            client.Avatar.OnTeleport += new TeleportCallback(Avatar_OnTeleportMessage);

            DoneTeleporting = false;
            client.Avatar.Teleport(sim, coords);

            while (!DoneTeleporting)
            {
                client.Tick();
            }
        }

        protected void Avatar_OnTeleportMessage(string message)
        {
            Console.WriteLine(message);
            if (!message.Equals("Teleport started"))
            {
                DoneTeleporting = true;
            }
        }
    }
}
