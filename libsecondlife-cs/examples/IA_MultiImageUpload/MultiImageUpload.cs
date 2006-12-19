using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using IA_SimpleInventory;
using IA_ImageTool;

using libsecondlife;
using libsecondlife.InventorySystem;
using libsecondlife.AssetSystem;

namespace IA_MultiImageUpload
{
    class MultiImageUpload
    {
        private SecondLife _Client;
        private ManualResetEvent ConnectedSignal = new ManualResetEvent(false);

        protected string ImageDirectory;

        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                MultiImageUpload.Usage();
                return;
            }

            string fullpath = Path.GetFullPath(args[3]);

            if (!Directory.Exists(fullpath))
            {
                Console.WriteLine("Directory does not exist: " + fullpath);
                return;
            }

            MultiImageUpload app = new MultiImageUpload( fullpath );
            if (app.Connect(args[0], args[1], args[2]))
            {
                if (app.ConnectedSignal.WaitOne(TimeSpan.FromMinutes(1), false))
                {
                    app.doStuff();
                    app.Disconnect();
                }
            }
        }

        public MultiImageUpload(string dir)
        {
            ImageDirectory = dir;
            try
            {
                _Client = new SecondLife();
                _Client.Network.OnConnected += new NetworkManager.ConnectedCallback(Network_OnConnected);
            }
            catch (Exception e)
            {
                // Error initializing the client
                Console.WriteLine();
                Console.WriteLine(e.ToString());
            }

        }

        public static void Usage()
        {
            System.Console.WriteLine("MultiImageUpload [FirstName] [LastName] [Password] [Directory]");
        }

        protected void doStuff()
        {
            InventoryFolder iFolder = _Client.Inventory.getFolder("Textures");
            iFolder = iFolder.CreateFolder(Helpers.GetUnixTime().ToString());

            Console.WriteLine("Uploading images:");

            string[] files = Directory.GetFiles(ImageDirectory, "*.tif");

            int filesUploaded = 0;
            foreach (string file in files)
            {
                byte[] j2cdata = null;
                try
                {
                    j2cdata = KakaduWrap.ReadJ2CData(file);
                } catch ( Exception e )
                {
                    Console.WriteLine(e.Message);
                }
                if ( j2cdata != null )
                {
                    Console.WriteLine(file);
                    iFolder.NewImage(Path.GetFileName(file), "ImageTool Upload", j2cdata);

                    if (++filesUploaded >= 20)
                    {
                        break;
                    }
                }
            }
        }
        void Network_OnConnected(object sender)
        {
            ConnectedSignal.Set();
        }

        protected bool Connect(string FirstName, string LastName, string Password)
        {
            Console.WriteLine("Attempting to connect and login to SecondLife.");

            // Setup Login to Second Life
            Dictionary<string, object> loginReply = new Dictionary<string, object>();

            // Login
            if (!_Client.Network.Login(FirstName, LastName, Password, "MultiImageUpload", "static.sprocket@gmail.com"))
            {
                // Login failed
                Console.WriteLine("Error logging in: " + _Client.Network.LoginError);
                return false;
            }

            // Login was successful
            Console.WriteLine("Login was successful.");
            Console.WriteLine("AgentID:   " + _Client.Network.AgentID);
            Console.WriteLine("SessionID: " + _Client.Network.SessionID);

            return true;
        }

        protected void Disconnect()
        {
            // Logout of Second Life
            Console.WriteLine("Request logout");
            _Client.Network.Logout();
        }
    }
}
