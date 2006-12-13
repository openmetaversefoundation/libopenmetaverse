using System;
using System.Collections.Generic;
using libsecondlife;

namespace Teleport
{
    class Teleport
    {
        protected SecondLife Client;
        protected bool DoneTeleporting = false;
        protected ulong RegionHandle = 0;
        protected string Sim = "";

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

            Teleport app = new Teleport(sim);
            bool success = app.Connect(args[0], args[1], args[2]);

            if (success)
            {
                // Get the current sim name
                while (app.Client.Network.CurrentSim.Region.Name == "")
                {
                    System.Threading.Thread.Sleep(100);
                }

                Console.WriteLine("Starting in " + app.Client.Network.CurrentSim.Region.Name);

                if (sim.ToLower() == app.Client.Network.CurrentSim.Region.Name.ToLower())
                {
                    Console.WriteLine("TODO: Add the ability to teleport somewhere in the local region. " +
                        "Exiting for now, please specify a region other than the current one");
                }
                else
                {
                    app.doStuff(sim, new LLVector3(x, y, z));
                }
                
                app.Disconnect();
            }
        }

        protected Teleport(string sim)
		{
            Sim = sim;

            try
            {
                Client = new SecondLife();
            }
            catch (Exception e)
            {
                // Error initializing the client
                Console.WriteLine();
                Console.WriteLine(e.ToString());
            }
		}

        protected bool Connect(string FirstName, string LastName, string Password)
        {
            Console.WriteLine("Attempting to connect and login to Second Life.");

            // Setup Login to Second Life
            Dictionary<string, object> loginParams = Client.Network.DefaultLoginValues(FirstName, 
                LastName, Password, "00:00:00:00:00:00", "last", "Win", "0", "createnotecard", 
                "static.sprocket@gmail.com");
            Dictionary<string, object> loginReply = new Dictionary<string,object>();

            // Login
            if (!Client.Network.Login(loginParams))
            {
                // Login failed
                Console.WriteLine("Error logging in: " + Client.Network.LoginError);
                return false;
            }

            // Login was successful
            Console.WriteLine("Login was successful");

            return true;
        }

        protected void Disconnect()
        {
            // Logout of Second Life
            Console.WriteLine("Requesting logout");
            Client.Network.Logout();
        }

        protected void doStuff(string sim, LLVector3 coords)
        {
            Client.Grid.OnRegionAdd += new GridRegionCallback(GridRegionHandler);

            Console.WriteLine("Caching estate sims...");
            Client.Grid.AddEstateSims();
            System.Threading.Thread.Sleep(3000);

            if (RegionHandle == 0)
            {
                Client.Grid.BeginGetGridRegion(sim, new GridRegionCallback(GridRegionHandler));

                int start = Environment.TickCount;

                while (RegionHandle == 0)
                {
                    Client.Tick();

                    if (Environment.TickCount - start > 10000)
                    {
                        Console.WriteLine("Region handle lookup failed");
                        Disconnect();
                        return;
                    }
                }
            }

            Client.Self.OnTeleport += new TeleportCallback(Avatar_OnTeleportMessage);

            DoneTeleporting = false;
            Client.Self.Teleport(RegionHandle, coords);

            while (!DoneTeleporting)
            {
                Client.Tick();
            }
        }

        private void Avatar_OnTeleportMessage(Simulator currentSim, string message, TeleportStatus status)
        {
            Console.WriteLine(message);

            if (status == TeleportStatus.Finished || status == TeleportStatus.Failed)
            {
                DoneTeleporting = true;
            }
        }

        private void GridRegionHandler(GridRegion region)
        {
            if (region.Name.ToLower() == Sim.ToLower())
            {
                RegionHandle = region.RegionHandle;
                Console.WriteLine("Resolved " + Sim + " to region handle " + RegionHandle);
            }
        }
    }
}
