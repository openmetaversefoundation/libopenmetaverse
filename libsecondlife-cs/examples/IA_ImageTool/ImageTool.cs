using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using IA_SimpleInventory;

using libsecondlife;
using libsecondlife.InventorySystem;
using libsecondlife.AssetSystem;

namespace IA_ImageTool
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    class ImageTool
    {
        private SecondLife _Client;
        private ManualResetEvent ConnectedSignal = new ManualResetEvent(false);


        private List<LLUUID> _ImageIDs = new List<LLUUID>();
        private string _FileName;
        private bool _Put;
        private double _Rate;

        /// <summary>
        /// Used to upload/download images.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if ( (File.Exists("libjasper.dll") == false) )
            {
                Console.WriteLine("You need a copy of libjasper.dll, it can be found in SVN in the main trunk inside libjaspernet");
                return;
            }

            if (args.Length < 5)
            {
                ImageTool.Usage();
                return;
            }


            List<LLUUID> uuidList = new List<LLUUID>();
            string filename = "";
            bool put = false;
            double rate = 0;

            if (args[3].ToLower().Equals("put"))
            {
                put = true;
                if (args.Length == 6)
                {
                    double.TryParse(args[4], out rate);
                    filename = args[5];
                }
                else
                {
                    filename = args[4];
                }
            }
            else if (args[3].ToLower().Equals("getfile"))
            {
                if (args.Length < 5)
                {
                    ImageTool.Usage();
                    return;
                }

                foreach( string id in File.ReadAllLines(args[4]) )
                {
                    uuidList.Add(id);
                }


            } 
            else 
            {
                if (args.Length < 6)
                {
                    ImageTool.Usage();
                    return;
                }

                uuidList = new List<LLUUID>();
                uuidList.Add( new LLUUID(args[4]) );
                if (args.Length == 6)
                {
                    filename = args[5];
                }
                else if (!args[4].ToLower().EndsWith(".j2c"))
                {
                    filename = args[4] + ".j2c";
                }
            }

            ImageTool it = new ImageTool(uuidList, filename, put, rate);


            if (it.Connect(args[0], args[1], args[2]))
            {
                if (it.ConnectedSignal.WaitOne(TimeSpan.FromMinutes(1), false))
                {
                    it.doStuff();

                    it.Disconnect();
                }

            }
        }

        protected ImageTool(List<LLUUID> imageIDs, string filename, bool put, double rate)
        {
            _ImageIDs = imageIDs;
            _FileName = filename;
            _Put = put;
            _Rate = rate;

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
            if (!_Client.Network.Login(FirstName, LastName, Password, "ImageTool", "static.sprocket@gmail.com"))
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

        protected void doStuff()
        {
            if (_Put)
            {
                

                Console.WriteLine("Reading: " + _FileName);

                byte[] j2cdata;

                if (_Rate != 0)
                {
                    j2cdata = KakaduWrap.ReadJ2CData(_FileName, _Rate);
                }
                else
                {
                    j2cdata = KakaduWrap.ReadJ2CData(_FileName);
                }
                

                Console.WriteLine("Connecting to your Texture folder...");
                InventoryFolder iFolder = _Client.Inventory.getFolder("Textures");

                Console.WriteLine("Uploading Texture...");
                InventoryImage image = iFolder.NewImage(_FileName, "ImageTool Upload", j2cdata);
                Console.WriteLine("Asset id = " + image.AssetID.ToStringHyphenated());
            }
            else
            {
                foreach( LLUUID ImageID in _ImageIDs )
                {
                    string FileName;
                    if (_ImageIDs.Count > 1)
                    {
                        FileName = ImageID.ToString();
                    }
                    else
                    {
                        FileName = _FileName;
                    }

                    Console.WriteLine("Downloading: " + ImageID);

                    int start = Environment.TickCount;
                    byte[] j2cdata;

                    try
                    {
                        j2cdata = _Client.Images.RequestImage(ImageID);

                        int end = Environment.TickCount;
                        Console.WriteLine("Elapsed download time, in TickCounts: " + (end - start));

                        Console.WriteLine("Image Data Length :" + j2cdata.Length);

                        Console.WriteLine("Writing to: " + FileName + ".tif");
                        File.WriteAllBytes(FileName + ".tif", JasperWrapper.jasper_decode_j2c_to_tiff(j2cdata));

                        Console.WriteLine("Writing to: " + FileName + ".tga");
                        File.WriteAllBytes(FileName + ".tga", JasperWrapper.jasper_decode_j2c_to_tga(j2cdata));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("ERROR: Can't download image :: " + e.Message);
                    }
                }

            }

            Console.WriteLine("Done...");

        }

        protected static void Usage()
        {
            Console.WriteLine("Usage: ImageTool [first] [last] [password] [get] [uuid] [(filename)]");
            Console.WriteLine("Usage: ImageTool [first] [last] [password] [getfile] [filename]");
            Console.WriteLine("Usage: ImageTool [first] [last] [password] [put] [filename]");
            Console.WriteLine("Usage: ImageTool [first] [last] [password] [put] [bit-rate] [filename]");

            Console.WriteLine();
            Console.WriteLine("Example: ImageTool John Doe Password get 0444bf21-f77e-7f63-89e9-b839ec66bc15 cloud (this will output a bmp and a tiff)");
            Console.WriteLine("Example: ImageTool John Doe Password getfile uuids.txt (this will download a list of textures, one per line)");
            Console.WriteLine("Example: ImageTool John Doe Password put Sample.tiff");
            Console.WriteLine("Example: ImageTool John Doe Password put 1.0 BigImage.tiff (this will compress the file with the given bit-rate)");
        }
    }
}
