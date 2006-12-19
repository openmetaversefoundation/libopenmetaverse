using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

using IA_SimpleInventory;

using libsecondlife;
using libsecondlife.AssetSystem;

namespace IA_TestAsyncImage
{
    class TestAsync
    {
        private SecondLife _Client;
        private ManualResetEvent ConnectedSignal = new ManualResetEvent(false);


        private Queue<LLUUID> TextureQueue = new Queue<LLUUID>();

        private string OutputDirectory = "IA_TestAsyncImages";

        [STAThread]
        static void Main(string[] args)
        {
            TestAsync app = new TestAsync();

            app._Client.Objects.OnNewPrim += new ObjectManager.NewPrimCallback(app.Objects_OnNewPrim);
            app._Client.Objects.OnNewAvatar += new ObjectManager.NewAvatarCallback(app.Objects_OnNewAvatar);


            app.Connect(args[0], args[1], args[2]);
            if (app.ConnectedSignal.WaitOne(TimeSpan.FromMinutes(1), false))
            {
                app.doStuff();
                app.Disconnect();
            }
            Console.WriteLine("Done...");
        }

        public TestAsync()
        {
            try
            {
                _Client = new SecondLife();
                _Client.Images = new ImageManager(_Client, ImageManager.CacheTypes.Disk, OutputDirectory);
                _Client.Network.OnConnected += new NetworkManager.ConnectedCallback(Network_OnConnected);
            }
            catch (Exception e)
            {
                // Error initializing the client
                Console.WriteLine();
                Console.WriteLine(e.ToString());
            }
        }

        private void Objects_OnNewAvatar(Simulator simulator, Avatar avatar, ulong regionHandle, ushort timeDilation)
        {
            if (avatar.FirstLifeImage != null)
            {
                if (_Client.Images.isCachedImage(avatar.FirstLifeImage) == false)
                {
                    _Client.Images.RequestImageAsync(avatar.FirstLifeImage);
                }
            }

            if (avatar.ProfileImage != null)
            {
                if (_Client.Images.isCachedImage(avatar.FirstLifeImage) == false)
                {
                    _Client.Images.RequestImageAsync(avatar.ProfileImage);
                }
            }

            if (avatar.Textures != null)
            {
                foreach (TextureEntryFace tef in avatar.Textures.FaceTextures.Values)
                {
                    if (_Client.Images.isCachedImage(tef.TextureID) == false)
                    {
                        _Client.Images.RequestImageAsync(tef.TextureID);
                    }
                    else
                    {
                        Console.WriteLine("Already cached: " + tef.TextureID);
                    }
                }
            }
        }

        private void Objects_OnNewPrim(Simulator simulator, PrimObject prim, ulong regionHandle, ushort timeDilation)
        {
            if ((prim.Textures.DefaultTexture != null) && (prim.Textures.DefaultTexture.TextureID != null))
            {
                if (_Client.Images.isCachedImage(prim.Textures.DefaultTexture.TextureID) == false)
                {
                    _Client.Images.RequestImageAsync(prim.Textures.DefaultTexture.TextureID);
                }
                else
                {
                    Console.WriteLine("Already cached: " + prim.Textures.DefaultTexture.TextureID);
                }
            }

            if (prim.Textures.FaceTextures != null)
            {
                foreach (TextureEntryFace tef in prim.Textures.FaceTextures.Values)
                {
                    if (_Client.Images.isCachedImage(tef.TextureID) == false)
                    {
                        _Client.Images.RequestImageAsync(tef.TextureID);
                    }
                    else
                    {
                        Console.WriteLine("Already cached: " + tef.TextureID);
                    }
                }
            }
        }

        private void NewImageRetrievedCallBack( LLUUID ImageID, byte[] data, bool wasCached, string statusMsg )
        {
            if (wasCached)
            {
                Console.WriteLine("Cache ( " + data.Length + "): " + ImageID);
            }
            else
            {
                if (data == null)
                {
                    Console.WriteLine("Image Data is null (" + statusMsg + "): " + ImageID);
                }
                else
                {
                    Console.WriteLine("Finished ( " + data.Length + "): " + ImageID);

                    String filename = Path.Combine(OutputDirectory, ImageID.ToStringHyphenated()) + ".tif";

                    TiffJob tj = new TiffJob(filename, data);
                    Thread t = new Thread(tj.RunMe);
                    t.Start();
                }
            }
        }

        protected void doStuff()
        {
            
            _Client.Images.OnImageRetrieved += new ImageRetrievedCallback(NewImageRetrievedCallBack);

            while (TextureQueue.Count > 0)
            {
                _Client.Images.RequestImageAsync(TextureQueue.Dequeue());
            }

            Console.WriteLine("Press any key to stop.");
            Console.Read();
        }

        protected class TiffJob
        {
            string filename;
            byte[] j2cdata;

            public TiffJob(string path, byte[] data)
            {
                filename = path;
                j2cdata  = data;
            }

            public void RunMe()
            {
                File.WriteAllBytes(filename, JasperWrapper.jasper_decode_j2c_to_tiff(j2cdata));
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
            if (!_Client.Network.Login(FirstName, LastName, Password, "createnotecard", "static.sprocket@gmail.com"))
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
