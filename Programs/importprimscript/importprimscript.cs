using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Drawing;
using Mono.Simd.Math;
using OpenMetaverse;
using OpenMetaverse.Capabilities;
using OpenMetaverse.Imaging;

namespace importprimscript
{
    class Sculpt
    {
        public string Name;
        public string TextureFile;
        public Guid TextureID;
        public string SculptFile;
        public Guid SculptID;
        public Vector3 Scale;
        public Vector3 Offset;
    }

    class importprimscript
    {
        static GridClient Client = new GridClient();
        static Sculpt CurrentSculpt = null;
        static AutoResetEvent RezzedEvent = new AutoResetEvent(false);
        static Vector3 RootPosition = Vector3f.Zero;
        static List<uint> RezzedPrims = new List<uint>();
        static Guid UploadFolderID = Guid.Empty;

        static void Main(string[] args)
        {
            if (args.Length != 8 && args.Length != 9)
            {
                Console.WriteLine("Usage: importprimscript.exe [firstname] [lastname] [password] " +
                    "[loginuri] [Simulator] [x] [y] [z] [input.primscript]" +
                    Environment.NewLine + "Example: importprimscript.exe My Bot password " + 
                    "Hooper 128 128 40 maya-export" + Path.DirectorySeparatorChar + "ant.primscript" +
                    Environment.NewLine + "(the loginuri is optional and only used for logging in to another grid)");
                Environment.Exit(-1);
            }

            // Strip quotes from any arguments
            for (int i = 0; i < args.Length; i++)
                args[i] = args[i].Trim(new char[] { '"' });

            // Parse the primscript file
            string scriptfilename = args[args.Length - 1];
            string error;
            List<Sculpt> sculpties = ParsePrimscript(scriptfilename, out error);
            scriptfilename = Path.GetFileNameWithoutExtension(scriptfilename);

            // Check for parsing errors
            if (error != String.Empty)
            {
                Console.WriteLine("An error was encountered reading the input file: " + error);
                Environment.Exit(-2);
            }
            else if (sculpties.Count == 0)
            {
                Console.WriteLine("No primitives were read from the input file");
                Environment.Exit(-3);
            }

            // Add callback handlers for asset uploads finishing. new prims spotted, and logging
            Client.Objects.OnNewPrim += new ObjectManager.NewPrimCallback(Objects_OnNewPrim);
            Logger.OnLogMessage += new Logger.LogCallback(Client_OnLogMessage);

            // Optimize the connection for our purposes
            Client.Self.Movement.Camera.Far = 64f;
            Client.Settings.MULTIPLE_SIMS = false;
            Client.Settings.SEND_AGENT_UPDATES = true;
            Settings.LOG_LEVEL = Helpers.LogLevel.None;
            Client.Settings.ALWAYS_REQUEST_OBJECTS = true;
            Client.Settings.ALWAYS_DECODE_OBJECTS = true;
            Client.Throttle.Land = 0;
            Client.Throttle.Wind = 0;
            Client.Throttle.Cloud = 0;
            // Not sure if Asset or Texture will help with uploads, but it won't hurt
            Client.Throttle.Asset = 220000.0f;
            Client.Throttle.Texture = 446000.0f;
            Client.Throttle.Task = 446000.0f;

            // Create a handler for the event queue connecting, so we know when
            // it is safe to start uploading
            AutoResetEvent eventQueueEvent = new AutoResetEvent(false);
            NetworkManager.EventQueueRunningCallback eventQueueCallback =
                delegate(Simulator simulator)
                {
                    if (simulator == Client.Network.CurrentSim)
                        eventQueueEvent.Set();
                };
            Client.Network.OnEventQueueRunning += eventQueueCallback;

            int x = Int32.Parse(args[args.Length - 4]);
            int y = Int32.Parse(args[args.Length - 3]);
            int z = Int32.Parse(args[args.Length - 2]);
            string start = NetworkManager.StartLocation(args[args.Length - 5], x, y, z);

            LoginParams loginParams = Client.Network.DefaultLoginParams(args[0], args[1], args[2],
                "importprimscript", "1.4.0");
            loginParams.Start = start;
            if (args.Length == 9) loginParams.URI = args[3];

            // Attempt to login
            if (!Client.Network.Login(loginParams))
            {
                Console.WriteLine("Login failed: " + Client.Network.LoginMessage);
                Environment.Exit(-4);
            }

            // Need to be connected to the event queue before we can upload
            Console.WriteLine("Login succeeded, waiting for the event handler to connect...");
            if (!eventQueueEvent.WaitOne(1000 * 90, false))
            {
                Console.WriteLine("Event queue connection timed out, disconnecting...");
                Client.Network.Logout();
                Environment.Exit(-5);
            }

            // Don't need this anymore
            Client.Network.OnEventQueueRunning -= eventQueueCallback;

            // Set the root position for the import
            RootPosition = Client.Self.SimPosition;
            RootPosition.Z += 3.0f;

            // TODO: Check if our account balance is high enough to upload everything
            //

            // Create a folder to hold all of our texture uploads
            UploadFolderID = Client.Inventory.CreateFolder(Client.Inventory.Store.RootFolder.Guid, scriptfilename);

            // Loop through each sculpty and do what we need to do
            for (int i = 0; i < sculpties.Count; i++)
            {
                // Upload the sculpt map and texture
                sculpties[i].SculptID = UploadImage(sculpties[i].SculptFile, true);
                sculpties[i].TextureID = UploadImage(sculpties[i].TextureFile, false);

                // Check for failed uploads
                if (sculpties[i].SculptID == Guid.Empty)
                {
                    Console.WriteLine("Sculpt map " + sculpties[i].SculptFile + " failed to upload, skipping " + sculpties[i].Name);
                    continue;
                }
                else if (sculpties[i].TextureID == Guid.Empty)
                {
                    Console.WriteLine("Texture " + sculpties[i].TextureFile + " failed to upload, skipping " + sculpties[i].Name);
                    continue;
                }

                // Create basic spherical volume parameters. It will be set to
                // a scultpy in the callback for new objects being created
                Primitive.ConstructionData volume = new Primitive.ConstructionData();
                volume.PCode = PCode.Prim;
                volume.Material = Material.Wood;
                volume.PathScaleY = 0.5f;
                volume.PathCurve = PathCurve.Circle;
                volume.ProfileCurve = ProfileCurve.Circle;

                // Rez this prim
                CurrentSculpt = sculpties[i];
                Client.Objects.AddPrim(Client.Network.CurrentSim, volume, Guid.Empty, 
                    RootPosition + CurrentSculpt.Offset, CurrentSculpt.Scale, Quaternionf.Identity);

                // Wait for the prim to rez and the properties be set for it
                if (!RezzedEvent.WaitOne(1000 * 10, false))
                {
                    Console.WriteLine("Timed out waiting for prim " + CurrentSculpt.Name + " to rez, skipping");
                    continue;
                }
            }

            CurrentSculpt = null;

            lock (RezzedPrims)
            {
                // Set full permissions for all of the objects
                Client.Objects.SetPermissions(Client.Network.CurrentSim, RezzedPrims, PermissionWho.All,
                    PermissionMask.All, true);

                // Link the entire object together
                Client.Objects.LinkPrims(Client.Network.CurrentSim, RezzedPrims);
            }

            Console.WriteLine("Rezzed, textured, and linked " + RezzedPrims.Count + " sculpted prims, logging out...");

            Client.Network.Logout();
        }

        static void Client_OnLogMessage(object message, Helpers.LogLevel level)
        {
            if (level >= Helpers.LogLevel.Warning)
                Console.WriteLine(level + ": " + message);
        }

        static Guid UploadImage(string filename, bool lossless)
        {
            Guid newAssetID = Guid.Empty;
            byte[] jp2data = null;

            try
            {
                Bitmap image = (Bitmap)Bitmap.FromFile(filename);
                jp2data = OpenJPEG.EncodeFromImage(image, lossless);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to encode image file " + filename + ": " + ex.ToString());
                return Guid.Empty;
            }

            AutoResetEvent uploadEvent = new AutoResetEvent(false);
            Client.Inventory.RequestCreateItemFromAsset(jp2data, Path.GetFileNameWithoutExtension(filename),
                "Uploaded with importprimscript", AssetType.Texture, InventoryType.Texture, UploadFolderID,

                delegate(CapsClient client, long bytesReceived, long bytesSent, long totalBytesToReceive, long totalBytesToSend)
                {
                    // FIXME: Do something with progress?
                },

                delegate(bool success, string status, Guid itemID, Guid assetID)
                {
                    if (success)
                    {
                        Console.WriteLine("Finished uploading image " + filename + ", AssetID: " + assetID.ToString());
                        newAssetID = assetID;
                    }
                    else
                    {
                        Console.WriteLine("Failed to upload image file " + filename + ": " + status);
                    }

                    uploadEvent.Set();
                }
            );

            // The images are small, 60 seconds should be plenty
            uploadEvent.WaitOne(1000 * 60, false);

            return newAssetID;
        }

        static void Objects_OnNewPrim(Simulator simulator, Primitive prim, ulong regionHandle, ushort timeDilation)
        {
            if (CurrentSculpt != null && (prim.Flags & PrimFlags.CreateSelected) != 0 &&
                !RezzedPrims.Contains(prim.LocalID))
            {
                lock (RezzedPrims) RezzedPrims.Add(prim.LocalID);

                Console.WriteLine("Rezzed prim " + CurrentSculpt.Name + ", setting properties");

                // Deselect the prim
                Client.Objects.DeselectObject(Client.Network.CurrentSim, prim.LocalID);

                // Set the prim position
                Client.Objects.SetPosition(Client.Network.CurrentSim, prim.LocalID,
                    RootPosition + CurrentSculpt.Offset);

                // Set the texture
                Primitive.TextureEntry textures = new Primitive.TextureEntry(CurrentSculpt.TextureID);
                Client.Objects.SetTextures(Client.Network.CurrentSim, prim.LocalID, textures);

                // Turn it in to a sculpted prim
                Primitive.SculptData sculpt = new Primitive.SculptData();
                sculpt.SculptTexture = CurrentSculpt.SculptID;
                sculpt.Type = SculptType.Sphere;
                Client.Objects.SetSculpt(Client.Network.CurrentSim, prim.LocalID, sculpt);

                // Set the prim name
                if (!String.IsNullOrEmpty(CurrentSculpt.Name))
                    Client.Objects.SetName(Client.Network.CurrentSim, prim.LocalID, CurrentSculpt.Name);

                RezzedEvent.Set();
            }
        }

        static List<Sculpt> ParsePrimscript(string primscriptfile, out string error)
        {
            string line;
            Sculpt current = null;
            List<Sculpt> sculpties = new List<Sculpt>();
            error = String.Empty;
            StreamReader primscript = null;

            // Parse a directory out of the primscriptfile string, if one exists
            string path = Path.GetDirectoryName(primscriptfile);
            if (!String.IsNullOrEmpty(path))
                path += Path.DirectorySeparatorChar;
            else
                path = String.Empty;

            try
            {
                primscript = File.OpenText(primscriptfile);

                while ((line = primscript.ReadLine()) != null)
                {
                    string[] words = line.Split(new char[] { ' ' });

                    if (words.Length > 0)
                    {
                        if (current != null)
                        {
                            switch (words[0])
                            {
                                case "newPrim":
                                    if (current.Scale != Vector3f.Zero &&
                                        !String.IsNullOrEmpty(current.SculptFile) &&
                                        !String.IsNullOrEmpty(current.TextureFile))
                                    {
                                        // Add the previous prim to the list as it is now finalized
                                        sculpties.Add(current);
                                    }

                                    // Start a new prim
                                    current = new Sculpt();

                                    break;
                                case "prim":
                                    // The only useful bit of information here is the prim name
                                    if (words.Length >= 3)
                                        current.Name = words[2];
                                    break;
                                case "shape":
                                    // This line has the name of the sculpt texture
                                    if (words.Length >= 3)
                                        current.SculptFile = path + words[2] + ".bmp";
                                    break;
                                case "texture":
                                    // This line has the name of the actual texture
                                    if (words.Length >= 3)
                                        current.TextureFile = path + words[2] + ".bmp";
                                    break;
                                case "transform":
                                    // Get some primitive params
                                    if (words.Length >= 9)
                                    {
                                        float x, y, z;
                                        x = Single.Parse(words[2]);
                                        y = Single.Parse(words[3]);
                                        z = Single.Parse(words[4]);
                                        current.Scale = new Vector3f(x, y, z);

                                        x = Single.Parse(words[6]);
                                        y = Single.Parse(words[7]);
                                        z = Single.Parse(words[8]);
                                        current.Offset = new Vector3f(x, y, z);
                                    }
                                    break;
                            }
                        }
                        else if (words[0] == "newPrim")
                        {
                            // Start a new prim
                            current = new Sculpt();
                        }
                    }
                }

                // Add the final prim
                if (current != null && current.Scale != Vector3f.Zero &&
                    !String.IsNullOrEmpty(current.SculptFile) &&
                    !String.IsNullOrEmpty(current.TextureFile))
                {
                    // Add the previous prim to the list as it is now finalized
                    sculpties.Add(current);
                }
            }
            catch (Exception ex)
            {
                error = ex.ToString();
            }
            finally
            {
                if (primscript != null)
                    primscript.Close();
            }

            return sculpties;
        }
    }
}
