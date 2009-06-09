using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using Tao.OpenGl;
using Tao.Platform.Windows;
using ICSharpCode.SharpZipLib.Zip;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Imaging;
using OpenMetaverse.Rendering;
using OpenMetaverse.Assets;

namespace PrimWorkshop
{
    public partial class frmBrowser : Form
    {
        const float DEG_TO_RAD = 0.0174532925f;
        const uint TERRAIN_START = (uint)Int32.MaxValue + 1;

        ContextMenu ExportPrimMenu;
        ContextMenu ExportTerrainMenu;

        GridClient Client;
        Camera Camera;
        Dictionary<uint, Primitive> RenderFoliageList = new Dictionary<uint, Primitive>();
        Dictionary<uint, RenderablePrim> RenderPrimList = new Dictionary<uint, RenderablePrim>();
        Dictionary<UUID, GlacialComponents.Controls.GLItem> DownloadList = new Dictionary<UUID, GlacialComponents.Controls.GLItem>();
        EventHandler IdleEvent;

        System.Timers.Timer ProgressTimer;
        int TotalPrims;

        // Textures
        Dictionary<UUID, TextureInfo> Textures = new Dictionary<UUID, TextureInfo>();

        // Terrain
        float MaxHeight = 0.1f;
        TerrainPatch[,] Heightmap;
        HeightmapLookupValue[] LookupHeightTable;

        // Picking globals
        bool Clicked = false;
        int ClickX = 0;
        int ClickY = 0;
        uint LastHit = 0;

        //
        Vector3 PivotPosition = Vector3.Zero;
        private bool Pivoting;
        Point LastPivot;

        //
        const int SELECT_BUFSIZE = 512;
        uint[] SelectBuffer = new uint[SELECT_BUFSIZE];

        //
        NativeMethods.Message msg;
        private bool AppStillIdle
        {
            get { return !NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0); }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public frmBrowser()
        {
            InitializeComponent();

            // Setup OpenGL
            glControl.InitializeContexts();
            glControl.SwapBuffers();
            glControl.MouseWheel += new MouseEventHandler(glControl_MouseWheel);

            // Login server URLs
            cboServer.Items.Add(Settings.AGNI_LOGIN_SERVER);
            cboServer.Items.Add(Settings.ADITI_LOGIN_SERVER);
            cboServer.Items.Add("http://osgrid.org:8002/");
            cboServer.SelectedIndex = 0;

            // Context menus
            ExportPrimMenu = new ContextMenu();
            ExportPrimMenu.MenuItems.Add("Download", new EventHandler(DownloadMenu_Clicked));
            ExportPrimMenu.MenuItems.Add("Download All Objects", new EventHandler(DownloadAllMenu_Clicked));
            ExportTerrainMenu = new ContextMenu();
            ExportTerrainMenu.MenuItems.Add("Teleport", new EventHandler(TeleportMenu_Clicked));
            ExportTerrainMenu.MenuItems.Add("Export Terrain", new EventHandler(ExportTerrainMenu_Clicked));
            ExportTerrainMenu.MenuItems.Add("Import Object", new EventHandler(ImportObjectMenu_Clicked));
            ExportTerrainMenu.MenuItems.Add("Import Sim", new EventHandler(ImportSimMenu_Clicked));

            // Setup a timer for updating the progress bar
            ProgressTimer = new System.Timers.Timer(250);
            ProgressTimer.Elapsed +=
                delegate(object sender, System.Timers.ElapsedEventArgs e)
                {
                    UpdatePrimProgress();
                };
            ProgressTimer.Start();

            IdleEvent = new EventHandler(Application_Idle);
            Application.Idle += IdleEvent;

            // Show a flat sim before login so the screen isn't so boring
            InitHeightmap();
            InitOpenGL();
            InitCamera();

            glControl_Resize(null, null);
        }

        private void InitLists()
        {
            TotalPrims = 0;

            lock (Textures)
            {
                foreach (TextureInfo tex in Textures.Values)
                {
                    int id = tex.ID;
                    Gl.glDeleteTextures(1, ref id);
                }

                Textures.Clear();
            }

            lock (RenderPrimList) RenderPrimList.Clear();
            lock (RenderFoliageList) RenderFoliageList.Clear();
        }

        private void InitializeObjects()
        {
            InitLists();

            if (DownloadList != null)
                lock (DownloadList)
                    DownloadList.Clear();

            // Initialize the SL client
            Client = new GridClient();
            Settings.MULTIPLE_SIMS = false;
            Settings.ALWAYS_DECODE_OBJECTS = true;
            Settings.ALWAYS_REQUEST_OBJECTS = true;
            Settings.SEND_AGENT_UPDATES = true;
            Settings.USE_TEXTURE_CACHE = true;
            //Client.Settings.TEXTURE_CACHE_DIR = Application.StartupPath + System.IO.Path.DirectorySeparatorChar + "cache";
            Settings.ALWAYS_REQUEST_PARCEL_ACL = false;
            Settings.ALWAYS_REQUEST_PARCEL_DWELL = false;
            // Crank up the throttle on texture downloads
            Client.Throttle.Texture = 446000.0f;

            // FIXME: Write our own avatar tracker so we don't double store prims
            Client.Settings.OBJECT_TRACKING = false; // We use our own object tracking system
            Client.Settings.AVATAR_TRACKING = true; //but we want to use the libsl avatar system

            Client.Network.OnLogin += new NetworkManager.LoginCallback(Network_OnLogin);
            Client.Network.OnDisconnected += new NetworkManager.DisconnectedCallback(Network_OnDisconnected);
            Client.Network.OnCurrentSimChanged += new NetworkManager.CurrentSimChangedCallback(Network_OnCurrentSimChanged);
            Client.Network.OnEventQueueRunning += new NetworkManager.EventQueueRunningCallback(Network_OnEventQueueRunning);
            Client.Objects.OnNewPrim += new ObjectManager.NewPrimCallback(Objects_OnNewPrim);
            Client.Objects.OnObjectKilled += new ObjectManager.KillObjectCallback(Objects_OnObjectKilled);
            Client.Terrain.OnLandPatch += new TerrainManager.LandPatchCallback(Terrain_OnLandPatch);
            Client.Parcels.OnSimParcelsDownloaded += new ParcelManager.SimParcelsDownloaded(Parcels_OnSimParcelsDownloaded);

            Client.Assets.OnImageRecieveProgress += new AssetManager.ImageReceiveProgressCallback(Assets_OnImageRecieveProgress);
            // Initialize the camera object
            InitCamera();

            // Setup the libsl camera to match our Camera struct
            UpdateCamera();
            glControl_Resize(null, null);

            /*
            // Enable lighting
            Gl.glEnable(Gl.GL_LIGHTING);
            Gl.glEnable(Gl.GL_LIGHT0);
            float[] lightPosition = { 128.0f, 64.0f, 96.0f, 0.0f };
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, lightPosition);

            // Setup ambient property
            float[] ambientLight = { 0.2f, 0.2f, 0.2f, 0.0f };
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, ambientLight);

            // Setup specular property
            float[] specularLight = { 0.5f, 0.5f, 0.5f, 0.0f };
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPECULAR, specularLight);
            */
        }


        private void InitOpenGL()
        {
            Gl.glShadeModel(Gl.GL_SMOOTH);

            Gl.glClearDepth(1.0f);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glDepthMask(Gl.GL_TRUE);
            Gl.glDepthFunc(Gl.GL_LEQUAL);
            Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_NICEST);
        }

        private void InitHeightmap()
        {
            // Initialize the heightmap
            Heightmap = new TerrainPatch[16, 16];
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    Heightmap[y, x] = new TerrainPatch();
                    Heightmap[y, x].Data = new float[16 * 16];
                }
            }

            // Speed up terrain exports with a lookup table
            LookupHeightTable = new HeightmapLookupValue[256 * 256];
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    LookupHeightTable[i + (j * 256)] = new HeightmapLookupValue(i + (j * 256), ((float)i * ((float)j / 127.0f)));
                }
            }
            Array.Sort<HeightmapLookupValue>(LookupHeightTable);
        }

        private void InitCamera()
        {
            Camera = new Camera();
            Camera.Position = new Vector3(128f, -192f, 90f);
            Camera.FocalPoint = new Vector3(128f, 128f, 0f);
            Camera.Zoom = 1.0d;
            Camera.Far = 512.0d;
        }

        private void UpdatePrimProgress()
        {
            if (this.InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate() { UpdatePrimProgress(); });
            }
            else
            {
                try
                {
                    if (RenderPrimList != null && RenderFoliageList != null)
                    {
                        int count = RenderPrimList.Count + RenderFoliageList.Count;

                        lblPrims.Text = String.Format("Prims: {0} / {1}", count, TotalPrims);
                        progPrims.Maximum = (TotalPrims > count) ? TotalPrims : count;
                        progPrims.Value = count;
                    }
                    else
                    {
                        lblPrims.Text = String.Format("Prims: 0 / {0}", TotalPrims);
                        progPrims.Maximum = TotalPrims;
                        progPrims.Value = 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private void UpdateCamera()
        {
            if (Client != null)
            {
                Client.Self.Movement.Camera.LookAt(Camera.Position, Camera.FocalPoint);
                Client.Self.Movement.Camera.Far = (float)Camera.Far;
            }

            Gl.glPushMatrix();
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();

            SetPerspective();

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPopMatrix();
        }

        private bool ExportObject(RenderablePrim parent, string fileName, out int prims, out int textures, out string error)
        {
            // Build a list of primitives (parent+children) to export
            List<Primitive> primList = new List<Primitive>();
            primList.Add(parent.Prim);
            
            lock (RenderPrimList)
            {
                foreach (RenderablePrim render in RenderPrimList.Values)
                {
                    if (render.Prim.ParentID == parent.Prim.LocalID)
                        primList.Add(render.Prim);
                }
            }

            return ExportObjects(primList, fileName, out prims, out textures, out error);
        }

        private bool ExportSim(string fileName, out int prims, out int textures, out string error)
        {
            // Add all of the prims in this sim to the export list
            List<Primitive> primList = new List<Primitive>();

            lock (RenderPrimList)
            {
                foreach (RenderablePrim render in RenderPrimList.Values)
                {
                    primList.Add(render.Prim);
                }
            }

            return ExportObjects(primList, fileName, out prims, out textures, out error);
        }

        private bool ExportObjects(List<Primitive> primList, string fileName, out int prims, out int textures, out string error)
        {
            List<UUID> textureList = new List<UUID>();
            prims = 0;
            textures = 0;
            
            // Write the LLSD to the hard drive in XML format
            string output = OSDParser.SerializeLLSDXmlString(Helpers.PrimListToOSD(primList));
            try
            {
                // Create a temporary directory
                string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
                Directory.CreateDirectory(tempPath);

                // Write the prim XML file
                File.WriteAllText(System.IO.Path.Combine(tempPath, "prims.xml"), output);
                prims = primList.Count;

                // Build a list of all the referenced textures in this prim list
                foreach (Primitive prim in primList)
                {
                    for (int i = 0; i < prim.Textures.FaceTextures.Length; i++)
                    {
                        Primitive.TextureEntryFace face = prim.Textures.FaceTextures[i];
                        if (face != null && face.TextureID != UUID.Zero && face.TextureID != Primitive.TextureEntry.WHITE_TEXTURE)
                        {
                            if (!textureList.Contains(face.TextureID))
                                textureList.Add(face.TextureID);
                        }
                    }
                }

                // Copy all of relevant textures from the cache to the temp directory
                foreach (UUID texture in textureList)
                {
                    string tempFileName = Client.Assets.Cache.ImageFileName(texture);

                    if (!String.IsNullOrEmpty(tempFileName))
                    {
                        File.Copy(tempFileName, System.IO.Path.Combine(tempPath, texture.ToString() + ".jp2"));
                        ++textures;
                    }
                    else
                    {
                        Console.WriteLine("Missing texture file during download: " + texture.ToString());
                    }
                }

                // Zip up the directory
                string[] filenames = Directory.GetFiles(tempPath);
                using (ZipOutputStream s = new ZipOutputStream(File.Create(fileName)))
                {
                    s.SetLevel(9);
                    byte[] buffer = new byte[4096];

                    foreach (string file in filenames)
                    {
                        ZipEntry entry = new ZipEntry(System.IO.Path.GetFileName(file));
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);

                        using (FileStream fs = File.OpenRead(file))
                        {
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                        }
                    }

                    s.Finish();
                    s.Close();
                }

                error = null;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private List<Primitive> ImportObjects(string fileName, out string tempPath, out string error)
        {
            tempPath = null;
            error = null;
            string primFile = null;

            try
            {
                // Create a temporary directory
                tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
                Directory.CreateDirectory(tempPath);

                // Unzip the primpackage
                using (ZipInputStream s = new ZipInputStream(File.OpenRead(fileName)))
                {
                    ZipEntry theEntry;

                    // Loop through and confirm there is a prims.xml file
                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        if (String.Equals("prims.xml", theEntry.Name.ToLower()))
                        {
                            primFile = theEntry.Name;
                            break;
                        }
                    }

                    if (primFile != null)
                    {
                        // Prepend the path to the primFile (that will be created in the next loop)
                        primFile = System.IO.Path.Combine(tempPath, primFile);
                    }
                    else
                    {
                        // Didn't find a prims.xml file, bail out
                        error = "No prims.xml file found in the archive";
                        return null;
                    }

                    // Reset to the beginning of the zip file
                    s.Seek(0, SeekOrigin.Begin);

                    Logger.DebugLog("Unpacking archive to " + tempPath);

                    // Unzip all of the texture and xml files
                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        string directory = System.IO.Path.GetDirectoryName(theEntry.Name);
                        string file = System.IO.Path.GetFileName(theEntry.Name);

                        // Skip directories
                        if (directory.Length > 0)
                            continue;

                        if (!String.IsNullOrEmpty(file))
                        {
                            string filelow = file.ToLower();

                            if (filelow.EndsWith(".jp2") || filelow.EndsWith(".tga") || filelow.EndsWith(".xml"))
                            {
                                Logger.DebugLog("Unpacking " + file);

                                // Create the full path from the temp path and new filename
                                string filePath = System.IO.Path.Combine(tempPath, file);

                                using (FileStream streamWriter = File.Create(filePath))
                                {
                                    const int READ_BUFFER_SIZE = 2048;
                                    int size = READ_BUFFER_SIZE;
                                    byte[] data = new byte[READ_BUFFER_SIZE];

                                    while (true)
                                    {
                                        size = s.Read(data, 0, data.Length);
                                        if (size > 0)
                                            streamWriter.Write(data, 0, size);
                                        else
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                Logger.Log("Skipping file " + file, Helpers.LogLevel.Info);
                            }
                        }
                    }
                }

                // Decode the .prims file
                string raw = File.ReadAllText(primFile);
                OSD osd = OSDParser.DeserializeLLSDXml(raw);
                return Helpers.OSDToPrimList(osd);
            }
            catch (Exception e)
            {
                error = e.Message;
                return null;
            }
        }

        private void DownloadMenu_Clicked(object sender, EventArgs e)
        {
            // Confirm that there actually is a selected object
            RenderablePrim parent;
            if (RenderPrimList.TryGetValue(LastHit, out parent))
            {
                if (parent.Prim.ParentID == 0)
                {
                    // Valid parent prim is selected, throw up the save file dialog
                    SaveFileDialog dialog = new SaveFileDialog();
                    dialog.Filter = "Prim Package (*.zip)|*.zip";

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string error;
                        int prims, textures;
                        if (ExportObject(parent, dialog.FileName, out prims, out textures, out error))
                            MessageBox.Show(String.Format("Exported {0} prims and {1} textures", prims, textures));
                        else
                            MessageBox.Show("Export failed: " + error);
                    }
                }
                else
                {
                    // This should have already been fixed in the picking processing code
                    Console.WriteLine("Download menu clicked when a child prim is selected!");
                    glControl.ContextMenu = null;
                    LastHit = 0;
                }
            }
            else
            {
                Console.WriteLine("Download menu clicked when there is no selected prim!");
                glControl.ContextMenu = null;
                LastHit = 0;
            }
        }

        private void DownloadAllMenu_Clicked(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Prim Package (*.zip)|*.zip";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string error;
                int prims, textures;
                if (ExportSim(dialog.FileName, out prims, out textures, out error))
                    MessageBox.Show(String.Format("Exported {0} prims and {1} textures", prims, textures));
                else
                    MessageBox.Show("Export failed: " + error);
            }
        }

        private void ExportTerrainMenu_Clicked(object sender, EventArgs e)
        {
            // Valid parent prim is selected, throw up the save file dialog
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Terrain RAW (*.raw)|*.raw";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += delegate(object obj, DoWorkEventArgs args)
                {
                    byte red, green, blue, alpha1, alpha2, alpha3, alpha4, alpha5, alpha6, alpha7, alpha8, alpha9, alpha10;

                    try
                    {
                        FileInfo file = new FileInfo(dialog.FileName);
                        FileStream s = file.Open(FileMode.OpenOrCreate, FileAccess.Write);
                        BinaryWriter binStream = new BinaryWriter(s);

                        for (int y = 0; y < 256; y++)
                        {
                            for (int x = 0; x < 256; x++)
                            {
                                int xBlock = x / 16;
                                int yBlock = y / 16;
                                int xOff = x - (xBlock * 16);
                                int yOff = y - (yBlock * 16);

                                float t = Heightmap[yBlock, xBlock].Data[yOff * 16 + xOff];
                                //float min = Single.MaxValue;
                                int index = 0;

                                // The lookup table is pre-sorted, so we either find an exact match or 
                                // the next closest (smaller) match with a binary search
                                index = Array.BinarySearch<HeightmapLookupValue>(LookupHeightTable, new HeightmapLookupValue(0, t));
                                if (index < 0)
                                    index = ~index - 1;

                                index = LookupHeightTable[index].Index;

                                /*for (int i = 0; i < 65536; i++)
                                {
                                    if (Math.Abs(t - LookupHeightTable[i].Value) < min)
                                    {
                                        min = Math.Abs(t - LookupHeightTable[i].Value);
                                        index = i;
                                    }
                                }*/

                                red = (byte)(index & 0xFF);
                                green = (byte)((index >> 8) & 0xFF);
                                blue = 20;
                                alpha1 = 0; // Land Parcels
                                alpha2 = 0; // For Sale Land
                                alpha3 = 0; // Public Edit Object
                                alpha4 = 0; // Public Edit Land
                                alpha5 = 255; // Safe Land
                                alpha6 = 255; // Flying Allowed
                                alpha7 = 255; // Create Landmark
                                alpha8 = 255; // Outside Scripts
                                alpha9 = red;
                                alpha10 = green;

                                binStream.Write(red);
                                binStream.Write(green);
                                binStream.Write(blue);
                                binStream.Write(alpha1);
                                binStream.Write(alpha2);
                                binStream.Write(alpha3);
                                binStream.Write(alpha4);
                                binStream.Write(alpha5);
                                binStream.Write(alpha6);
                                binStream.Write(alpha7);
                                binStream.Write(alpha8);
                                binStream.Write(alpha9);
                                binStream.Write(alpha10);
                            }
                        }

                        binStream.Close();
                        s.Close();

                        BeginInvoke((MethodInvoker)delegate() { System.Windows.Forms.MessageBox.Show("Exported heightmap"); });
                    }
                    catch (Exception ex)
                    {
                        BeginInvoke((MethodInvoker)delegate() { System.Windows.Forms.MessageBox.Show("Error exporting heightmap: " + ex.Message); });
                    }
                };

                worker.RunWorkerAsync();
            }
        }

        private void TeleportMenu_Clicked(object sender, EventArgs e)
        {
            if (Client != null && Client.Network.CurrentSim != null)
            {
                if (LastHit >= TERRAIN_START)
                {
                    // Determine which piece of terrain was clicked on
                    int y = (int)(LastHit - TERRAIN_START) / 16;
                    int x = (int)(LastHit - (TERRAIN_START + (y * 16)));

                    Vector3 targetPos = new Vector3(x * 16 + 8, y * 16 + 8, 0f);

                    Console.WriteLine("Starting local teleport to " + targetPos.ToString());
                    Client.Self.RequestTeleport(Client.Network.CurrentSim.Handle, targetPos);
                }
                else
                {
                    // This shouldn't have happened...
                    glControl.ContextMenu = null;
                }
            }
        }

        private void ImportObjectMenu_Clicked(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Prim Package (*.zip,*.primpackage)|*.zip;*.primpackage";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                // FIXME: Disable any further imports or exports until this is finished

                // Import the prims
                string error, texturePath;
                List<Primitive> primList = ImportObjects(dialog.FileName, out texturePath, out error);
                if (primList != null)
                {
                    // Determine the total height of the object
                    float minHeight = Single.MaxValue;
                    float maxHeight = Single.MinValue;

                    //float totalHeight = 0f;

                    for (int i = 0; i < primList.Count; i++)
                    {
                        Primitive prim = primList[i];

                        // Find the largest scale dimension (quick cheat to avoid figuring in the rotation)
                        float scale = prim.Scale.X;
                        if (prim.Scale.Y > scale) scale = prim.Scale.Y;
                        if (prim.Scale.Z > scale) scale = prim.Scale.Z;

                        float top = prim.Position.Z + (scale * 0.5f);
                        float bottom = top - scale;

                        if (top > maxHeight) maxHeight = top;
                        if (bottom < minHeight) minHeight = bottom;
                    }

                    //totalHeight = maxHeight - minHeight;

                    // Create a progress bar for the import process
                    ProgressBar prog = new ProgressBar();
                    prog.Minimum = 0;
                    prog.Maximum = primList.Count;
                    prog.Value = 0;

                    // List item
                    GlacialComponents.Controls.GLItem item = new GlacialComponents.Controls.GLItem();
                    item.SubItems[0].Text = "Import process";
                    item.SubItems[1].Control = prog;

                    lstDownloads.Items.Add(item);
                    lstDownloads.Invalidate();

                    // Start the import process in the background
                    BackgroundWorker worker = new BackgroundWorker();

                    worker.DoWork += delegate(object s, DoWorkEventArgs ea)
                    {
                        // Set the spot choosing state

                        // Wait for a spot to be chosen

                        // mouse2dto3d()

                        // Add (0, 0, totalHeight * 0.5f) to the clicked position

                        for (int i = 0; i < primList.Count; i++)
                        {
                            Primitive prim = primList[i];

                            for (int j = 0; j < prim.Textures.FaceTextures.Length; j++)
                            {
                                // Check if this texture exists

                                // If not, wait while it uploads
                            }

                            // Create this prim (using weird SL math to get the correct position)

                            // Wait for the callback to fire for this prim being created

                            // Add this prim's localID to a list

                            // Set any additional properties. If this is the root prim, do not apply rotation

                            // Update the progress bar
                            BeginInvoke((MethodInvoker)delegate() { prog.Value = i; });
                        }

                        // Link all of the prims together

                        // Apply root prim rotation
                    };

                    worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs ea)
                    {
                        BeginInvoke(
                        (MethodInvoker)delegate()
                        {
                            lstDownloads.Items.Remove(item);
                            lstDownloads.Invalidate();
                        });
                    };

                    worker.RunWorkerAsync();
                }
                else
                {
                    // FIXME: Re-enable imports and exports

                    MessageBox.Show(error);
                    return;
                }
            }
        }

        private void ImportSimMenu_Clicked(object sender, EventArgs e)
        {
        }

        private void SetPerspective()
        {
            Glu.gluPerspective(50.0d * Camera.Zoom, 1.0d, 0.1d, Camera.Far);
        }

        private void StartPicking(int cursorX, int cursorY)
        {
            int[] viewport = new int[4];

            Gl.glSelectBuffer(SELECT_BUFSIZE, SelectBuffer);
            Gl.glRenderMode(Gl.GL_SELECT);

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();

            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);
            Glu.gluPickMatrix(cursorX, viewport[3] - cursorY, 5, 5, viewport);

            SetPerspective();

            Gl.glMatrixMode(Gl.GL_MODELVIEW);

            Gl.glInitNames();
        }

        private void StopPicking()
        {
            int hits;

            // Resotre the original projection matrix
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glFlush();

            // Return to normal rendering mode
            hits = Gl.glRenderMode(Gl.GL_RENDER);

            // If there are hits process them
            if (hits != 0)
            {
                ProcessHits(hits, SelectBuffer);
            }
            else
            {
                LastHit = 0;
                glControl.ContextMenu = null;
            }
        }

        private void ProcessHits(int hits, uint[] selectBuffer)
        {
            uint names = 0;
            uint numNames = 0;
            uint minZ = 0xffffffff;
            uint ptr = 0;
            uint ptrNames = 0;

            for (uint i = 0; i < hits; i++)
            {
                names = selectBuffer[ptr];
                ++ptr;
                if (selectBuffer[ptr] < minZ)
                {
                    numNames = names;
                    minZ = selectBuffer[ptr];
                    ptrNames = ptr + 2;
                }

                ptr += names + 2;
            }

            ptr = ptrNames;

            for (uint i = 0; i < numNames; i++, ptr++)
            {
                LastHit = selectBuffer[ptr];
            }

            if (LastHit >= TERRAIN_START)
            {
                // Terrain was clicked on, turn off the context menu
                glControl.ContextMenu = ExportTerrainMenu;
            }
            else
            {
                RenderablePrim render;
                if (RenderPrimList.TryGetValue(LastHit, out render))
                {
                    if (render.Prim.ParentID == 0)
                    {
                        Camera.FocalPoint = render.Prim.Position;
                        UpdateCamera();
                    }
                    else
                    {
                        // See if we have the parent
                        RenderablePrim renderParent;
                        if (RenderPrimList.TryGetValue(render.Prim.ParentID, out renderParent))
                        {
                            // Turn on the context menu
                            glControl.ContextMenu = ExportPrimMenu;

                            // Change the clicked on prim to the parent. Camera position stays on the
                            // clicked child but the highlighting is applied to all the children
                            LastHit = renderParent.Prim.LocalID;

                            Camera.FocalPoint = renderParent.Prim.Position + render.Prim.Position;
                            UpdateCamera();
                        }
                        else
                        {
                            Console.WriteLine("Clicked on a child prim with no parent!");
                            LastHit = 0;
                        }
                    }
                }
            }
        }

        private void Objects_OnNewPrim(Simulator simulator, Primitive prim, ulong regionHandle, ushort timeDilation)
        {
            if (prim.PrimData.PCode == PCode.Grass || prim.PrimData.PCode == PCode.Tree || prim.PrimData.PCode == PCode.NewTree)
            {
                lock (RenderFoliageList)
                    RenderFoliageList[prim.LocalID] = prim;
                return;
            }

            RenderablePrim render = new RenderablePrim();
            render.Prim = prim;
            render.Mesh = Render.Plugin.GenerateFacetedMesh(prim, DetailLevel.High);

            // Create a FaceData struct for each face that stores the 3D data
            // in a Tao.OpenGL friendly format
            for (int j = 0; j < render.Mesh.Faces.Count; j++)
            {
                Face face = render.Mesh.Faces[j];
                FaceData data = new FaceData();

                // Vertices for this face
                data.Vertices = new float[face.Vertices.Count * 3];
                for (int k = 0; k < face.Vertices.Count; k++)
                {
                    data.Vertices[k * 3 + 0] = face.Vertices[k].Position.X;
                    data.Vertices[k * 3 + 1] = face.Vertices[k].Position.Y;
                    data.Vertices[k * 3 + 2] = face.Vertices[k].Position.Z;
                }

                // Indices for this face
                data.Indices = face.Indices.ToArray();

                // Texture transform for this face
                Primitive.TextureEntryFace teFace = prim.Textures.GetFace((uint)j);
                Render.Plugin.TransformTexCoords(face.Vertices, face.Center, teFace);

                // Texcoords for this face
                data.TexCoords = new float[face.Vertices.Count * 2];
                for (int k = 0; k < face.Vertices.Count; k++)
                {
                    data.TexCoords[k * 2 + 0] = face.Vertices[k].TexCoord.X;
                    data.TexCoords[k * 2 + 1] = face.Vertices[k].TexCoord.Y;
                }

                // Texture for this face
                if (teFace.TextureID != UUID.Zero &&
                    teFace.TextureID != Primitive.TextureEntry.WHITE_TEXTURE)
                { 
                    lock (Textures)
                    {
                        if (!Textures.ContainsKey(teFace.TextureID))
                        {
                            // We haven't constructed this image in OpenGL yet, get ahold of it
                            Client.Assets.RequestImage(teFace.TextureID, ImageType.Normal, TextureDownloader_OnDownloadFinished);
                        }
                    }
                }

                // Set the UserData for this face to our FaceData struct
                face.UserData = data;
                render.Mesh.Faces[j] = face;
            }

            lock (RenderPrimList) RenderPrimList[prim.LocalID] = render;
        }

        private void Objects_OnObjectKilled(Simulator simulator, uint objectID)
        {
            //
        }

        private void Parcels_OnSimParcelsDownloaded(Simulator simulator, InternalDictionary<int, Parcel> simParcels, int[,] parcelMap)
        {
            TotalPrims = 0;

            simParcels.ForEach(
                delegate(Parcel parcel)
                {
                    TotalPrims += parcel.TotalPrims;
                });

            UpdatePrimProgress();
        }

        private void Terrain_OnLandPatch(Simulator simulator, int x, int y, int width, float[] data)
        {
            if (Client != null && Client.Network.CurrentSim == simulator)
            {
                Heightmap[y, x].Data = data;
            }

            // Find the new max height
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] > MaxHeight)
                    MaxHeight = data[i];
            }
        }

        private void Network_OnLogin(LoginStatus login, string message)
        {
            if (login == LoginStatus.Success)
            {
                // Success!
            }
            else if (login == LoginStatus.Failed)
            {
                BeginInvoke(
                    (MethodInvoker)delegate()
                    {
                        MessageBox.Show(this, String.Format("Error logging in ({0}): {1}",
                            Client.Network.LoginErrorKey, Client.Network.LoginMessage));
                        cmdLogin.Text = "Login";
                        txtFirst.Enabled = txtLast.Enabled = txtPass.Enabled = true;
                    });
            }
        }

        private void Network_OnDisconnected(NetworkManager.DisconnectType reason, string message)
        {
            BeginInvoke(
                (MethodInvoker)delegate()
                {
                    cmdTeleport.Enabled = false;
                    DoLogout();
                });
        }

        private void Network_OnCurrentSimChanged(Simulator PreviousSimulator)
        {
            Console.WriteLine("CurrentSim set to " + Client.Network.CurrentSim + ", downloading parcel information");
            
            BeginInvoke((MethodInvoker)delegate() { txtSim.Text = Client.Network.CurrentSim.Name; });

            //InitHeightmap();
            InitLists();

            // Disable teleports until the new event queue comes online
            if (!Client.Network.CurrentSim.Caps.IsEventQueueRunning)
                BeginInvoke((MethodInvoker)delegate() { cmdTeleport.Enabled = false; });
        }

        private void Network_OnEventQueueRunning(Simulator simulator)
        {
            if (simulator == Client.Network.CurrentSim)
                BeginInvoke((MethodInvoker)delegate() { cmdTeleport.Enabled = true; });

            // Now seems like a good time to start requesting parcel information
            Client.Parcels.RequestAllSimParcels(Client.Network.CurrentSim, false, 100);
        }

        private void RenderScene()
        {
            try
            {
                Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
                Gl.glLoadIdentity();
                Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
                Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);

                if (Clicked)
                    StartPicking(ClickX, ClickY);

                // Setup wireframe or solid fill drawing mode
                Gl.glPolygonMode(Gl.GL_FRONT, Gl.GL_LINE);

                // Position the camera
                Glu.gluLookAt(
                    Camera.Position.X, Camera.Position.Y, Camera.Position.Z,
                    Camera.FocalPoint.X, Camera.FocalPoint.Y, Camera.FocalPoint.Z,
                    0f, 0f, 1f);

                RenderSkybox();

                // Push the world matrix
                Gl.glPushMatrix();

                RenderTerrain();
                RenderPrims();
                RenderAvatars();

                Gl.glDisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
                Gl.glDisableClientState(Gl.GL_VERTEX_ARRAY);

                if (Clicked)
                {
                    Clicked = false;
                    StopPicking();
                }

                // Pop the world matrix
                Gl.glPopMatrix();
                Gl.glFlush();

                glControl.Invalidate();
            }
            catch (Exception)
            {
            }
        }

        static readonly Vector3[] SkyboxVerts = new Vector3[]
        {
	        // Right side
	        new Vector3(	 10.0f,		10.0f,		-10.0f	), //Top left
	        new Vector3(	 10.0f,		10.0f,		10.0f	), //Top right
	        new Vector3(	 10.0f,		-10.0f,		10.0f	), //Bottom right
	        new Vector3(	 10.0f,		-10.0f,		-10.0f	), //Bottom left
	        // Left side
	        new Vector3(	-10.0f,		10.0f,		10.0f	), //Top left
	        new Vector3(	-10.0f,		10.0f,		-10.0f	), //Top right
	        new Vector3(	-10.0f,		-10.0f,		-10.0f	), //Bottom right
	        new Vector3(	-10.0f,		-10.0f,		10.0f	), //Bottom left
	        // Top side
	        new Vector3(	-10.0f,		10.0f,		10.0f	), //Top left
	        new Vector3(	 10.0f,		10.0f,		10.0f	), //Top right
	        new Vector3(	 10.0f,		10.0f,		-10.0f	), //Bottom right
	        new Vector3(	-10.0f,		10.0f,		-10.0f	), //Bottom left
	        // Bottom side
	        new Vector3(	-10.0f,		-10.0f,		-10.0f	), //Top left
	        new Vector3(	 10.0f,		-10.0f,		-10.0f	), //Top right
	        new Vector3(	 10.0f,		-10.0f,		10.0f	), //Bottom right
	        new Vector3(	-10.0f,		-10.0f,		10.0f	), //Bottom left
	        // Front side
	        new Vector3(	-10.0f,		10.0f,		-10.0f	), //Top left
	        new Vector3(	 10.0f,		10.0f,		-10.0f	), //Top right
	        new Vector3(	 10.0f,		-10.0f,		-10.0f	), //Bottom right
	        new Vector3(	-10.0f,		-10.0f,		-10.0f	), //Bottom left
	        // Back side
	        new Vector3(	10.0f,		10.0f,		10.0f	), //Top left
	        new Vector3(	-10.0f,		10.0f,		10.0f	), //Top right
	        new Vector3(	-10.0f,		-10.0f,		10.0f	), //Bottom right
	        new Vector3(	 10.0f,		-10.0f,		10.0f	), //Bottom left
        };

        private void RenderSkybox()
        {
            //Gl.glTranslatef(0f, 0f, 0f);
        }

        private void RenderTerrain()
        {
            if (Heightmap != null)
            {
                int i = 0;

                // No texture
                Gl.glBindTexture(Gl.GL_TEXTURE_2D, 0);

                for (int hy = 0; hy < 16; hy++)
                {
                    for (int hx = 0; hx < 16; hx++)
                    {
                        uint patchName = (uint)(TERRAIN_START + i);
                        Gl.glPushName(patchName);
                        ++i;

                        // Check if this patch is currently selected
                        bool selected = (LastHit == patchName);

                        for (int y = 0; y < 15; y++)
                        {
                            Gl.glBegin(Gl.GL_TRIANGLE_STRIP);

                            for (int x = 0; x < 15; x++)
                            {
                                // Vertex 0
                                float height = Heightmap[hy, hx].Data[y * 16 + x];
                                float color = height / MaxHeight;
                                float red = (selected) ? 1f : color;

                                Gl.glColor3f(red, color, color);
                                Gl.glTexCoord2f(0f, 0f);
                                Gl.glVertex3f(hx * 16 + x, hy * 16 + y, height);

                                // Vertex 1
                                height = Heightmap[hy, hx].Data[y * 16 + (x + 1)];
                                color = height / MaxHeight;
                                red = (selected) ? 1f : color;

                                Gl.glColor3f(red, color, color);
                                Gl.glTexCoord2f(1f, 0f);
                                Gl.glVertex3f(hx * 16 + x + 1, hy * 16 + y, height);

                                // Vertex 2
                                height = Heightmap[hy, hx].Data[(y + 1) * 16 + x];
                                color = height / MaxHeight;
                                red = (selected) ? 1f : color;

                                Gl.glColor3f(red, color, color);
                                Gl.glTexCoord2f(0f, 1f);
                                Gl.glVertex3f(hx * 16 + x, hy * 16 + y + 1, height);

                                // Vertex 3
                                height = Heightmap[hy, hx].Data[(y + 1) * 16 + (x + 1)];
                                color = height / MaxHeight;
                                red = (selected) ? 1f : color;

                                Gl.glColor3f(red, color, color);
                                Gl.glTexCoord2f(1f, 1f);
                                Gl.glVertex3f(hx * 16 + x + 1, hy * 16 + y + 1, height);
                            }

                            Gl.glEnd();
                        }

                        Gl.glPopName();
                    }
                }
            }
        }

        //int[] CubeMapDefines = new int[]
        //{
        //    Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_X_ARB,
        //    Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_X_ARB,
        //    Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_Y_ARB,
        //    Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_Y_ARB,
        //    Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_Z_ARB,
        //    Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_Z_ARB
        //};

        private void RenderPrims()
        {
            if (RenderPrimList != null && RenderPrimList.Count > 0)
            {
                Gl.glEnable(Gl.GL_TEXTURE_2D);

                lock (RenderPrimList)
                {
                    bool firstPass = true;
                    Gl.glDisable(Gl.GL_BLEND);
                    Gl.glEnable(Gl.GL_DEPTH_TEST);

StartRender:

                    foreach (RenderablePrim render in RenderPrimList.Values)
                    {
                        RenderablePrim parentRender = RenderablePrim.Empty;
                        Primitive prim = render.Prim;

                        if (prim.ParentID != 0)
                        {
                            // Get the parent reference
                            if (!RenderPrimList.TryGetValue(prim.ParentID, out parentRender))
                            {
                                // Can't render a child with no parent prim, skip it
                                continue;
                            }
                        }

                        Gl.glPushName(prim.LocalID);
                        Gl.glPushMatrix();

                        if (prim.ParentID != 0)
                        {
                            // Child prim
                            Primitive parent = parentRender.Prim;

                            // Apply parent translation and rotation
                            Gl.glMultMatrixf(Math3D.CreateTranslationMatrix(parent.Position));
                            Gl.glMultMatrixf(Math3D.CreateRotationMatrix(parent.Rotation));
                        }

                        // Apply prim translation and rotation
                        Gl.glMultMatrixf(Math3D.CreateTranslationMatrix(prim.Position));
                        Gl.glMultMatrixf(Math3D.CreateRotationMatrix(prim.Rotation));

                        // Scale the prim
                        Gl.glScalef(prim.Scale.X, prim.Scale.Y, prim.Scale.Z);

                        // Draw the prim faces
                        for (int j = 0; j < render.Mesh.Faces.Count; j++)
                        {
                            Face face = render.Mesh.Faces[j];
                            FaceData data = (FaceData)face.UserData;
                            Color4 color = face.TextureFace.RGBA;
                            bool alpha = false;
                            int textureID = 0;

                            if (color.A < 1.0f)
                                alpha = true;

                            #region Texturing

                            TextureInfo info;
                            if (Textures.TryGetValue(face.TextureFace.TextureID, out info))
                            {
                                if (info.Alpha)
                                    alpha = true;

                                textureID = info.ID;

                                // Enable texturing for this face
                                Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL);
                            }
                            else
                            {
                                if (face.TextureFace.TextureID == Primitive.TextureEntry.WHITE_TEXTURE ||
                                    face.TextureFace.TextureID == UUID.Zero)
                                {
                                    Gl.glPolygonMode(Gl.GL_FRONT, Gl.GL_FILL);
                                }
                                else
                                {
                                    Gl.glPolygonMode(Gl.GL_FRONT, Gl.GL_LINE);
                                }
                            }

                            if (firstPass && !alpha || !firstPass && alpha)
                            {
                                // Color this prim differently based on whether it is selected or not
                                if (LastHit == prim.LocalID || (LastHit != 0 && LastHit == prim.ParentID))
                                {
                                    Gl.glColor4f(1f, color.G * 0.3f, color.B * 0.3f, color.A);
                                }
                                else
                                {
                                    Gl.glColor4f(color.R, color.G, color.B, color.A);
                                }

                                // Bind the texture
                                Gl.glBindTexture(Gl.GL_TEXTURE_2D, textureID);

                                Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, data.TexCoords);
                                Gl.glVertexPointer(3, Gl.GL_FLOAT, 0, data.Vertices);
                                Gl.glDrawElements(Gl.GL_TRIANGLES, data.Indices.Length, Gl.GL_UNSIGNED_SHORT, data.Indices);
                            }

                            #endregion Texturing
                        }

                        Gl.glPopMatrix();
                        Gl.glPopName();
                    }

                    if (firstPass)
                    {
                        firstPass = false;
                        Gl.glEnable(Gl.GL_BLEND);
                        Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
                        //Gl.glDisable(Gl.GL_DEPTH_TEST);

                        goto StartRender;
                    }
                }

                Gl.glEnable(Gl.GL_DEPTH_TEST);
                Gl.glDisable(Gl.GL_TEXTURE_2D);
            }
        }

        private void RenderAvatars()
        {
            if (Client != null && Client.Network.CurrentSim != null)
            {
                Gl.glColor3f(0f, 1f, 0f);

                Client.Network.CurrentSim.ObjectsAvatars.ForEach(
                    delegate(Avatar avatar)
                    {
                        Gl.glPushMatrix();
                        Gl.glTranslatef(avatar.Position.X, avatar.Position.Y, avatar.Position.Z);

                        Glu.GLUquadric quad = Glu.gluNewQuadric();
                        Glu.gluSphere(quad, 1.0d, 10, 10);
                        Glu.gluDeleteQuadric(quad);

                        Gl.glPopMatrix();
                    }
                );
                
                Gl.glColor3f(1f, 1f, 1f);
            }
        }

        #region Texture Downloading

        private void TextureDownloader_OnDownloadFinished(TextureRequestState state, AssetTexture asset)
        {
            bool alpha = false;
            ManagedImage imgData = null;
            byte[] raw = null;
            
            bool success = (state == TextureRequestState.Finished);

            UUID id = asset.AssetID;

            try
            {
                // Load the image off the disk
                if (success)
                {
                    //ImageDownload download = TextureDownloader.GetTextureToRender(id);
                    if (OpenJPEG.DecodeToImage(asset.AssetData, out imgData))
                    {
                        raw = imgData.ExportRaw();

                        if ((imgData.Channels & ManagedImage.ImageChannels.Alpha) != 0)
                            alpha = true;
                    }
                    else
                    {
                        success = false;
                        Console.WriteLine("Failed to decode texture");
                    }
                }

                // Make sure the OpenGL commands run on the main thread
                BeginInvoke(
                       (MethodInvoker)delegate()
                       {
                           if (success)
                           {
                               int textureID = 0;

                               try
                               {
                                   Gl.glGenTextures(1, out textureID);
                                   Gl.glBindTexture(Gl.GL_TEXTURE_2D, textureID);

                                   Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_NEAREST); //Gl.GL_NEAREST);
                                   Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
                                   Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
                                   Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
                                   Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_GENERATE_MIPMAP, Gl.GL_TRUE); //Gl.GL_FALSE);

                                   //Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA, bitmap.Width, bitmap.Height, 0, Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE,
                                   //    bitmapData.Scan0);
                                   //int error = Gl.glGetError();

                                   int error = Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, Gl.GL_RGBA, imgData.Width, imgData.Height, Gl.GL_BGRA,
                                       Gl.GL_UNSIGNED_BYTE, raw);

                                   if (error == 0)
                                   {
                                       Textures[id] = new TextureInfo(textureID, alpha);
                                       Console.WriteLine("Created OpenGL texture for " + id.ToString());
                                   }
                                   else
                                   {
                                       Textures[id] = new TextureInfo(0, false);
                                       Console.WriteLine("Error creating OpenGL texture: " + error);
                                   }
                               }
                               catch (Exception ex)
                               {
                                   Console.WriteLine(ex);
                               }
                           }

                           // Remove this image from the download listbox
                           lock (DownloadList)
                           {
                               GlacialComponents.Controls.GLItem item;
                               if (DownloadList.TryGetValue(id, out item))
                               {
                                   DownloadList.Remove(id);
                                   try { lstDownloads.Items.Remove(item); }
                                   catch (Exception) { }
                                   lstDownloads.Invalidate();
                               }
                           }
                       });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        
        private void Assets_OnImageRecieveProgress(UUID imageID, int recieved, int total)
        {
            lock (DownloadList)
            {
                GlacialComponents.Controls.GLItem item;
                if (DownloadList.TryGetValue(imageID, out item))
                {
                    // Update an existing item
                    BeginInvoke(
                        (MethodInvoker)delegate()
                        {
                            ProgressBar prog = (ProgressBar)item.SubItems[1].Control;
                            if (total >= recieved)
                                prog.Value = (int)Math.Round((((double)recieved / (double)total) * 100.0d));
                        });
                }
                else
                {
                    // Progress bar
                    ProgressBar prog = new ProgressBar();
                    prog.Minimum = 0;
                    prog.Maximum = 100;
                    if (total >= recieved)
                        prog.Value = (int)Math.Round((((double)recieved / (double)total) * 100.0d));
                    else
                        prog.Value = 0;

                    // List item
                    item = new GlacialComponents.Controls.GLItem();
                    item.SubItems[0].Text = imageID.ToString();
                    item.SubItems[1].Control = prog;

                    DownloadList[imageID] = item;

                    BeginInvoke(
                        (MethodInvoker)delegate()
                        {
                            lstDownloads.Items.Add(item);
                            lstDownloads.Invalidate();
                        });
                }
            }
        }

        #endregion Texture Downloading

        private void frmBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            DoLogout();

            Application.Idle -= IdleEvent;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            while (AppStillIdle)
            {
                RenderScene();
            }
        }

        private void cmdLogin_Click(object sender, EventArgs e)
        {
            if (cmdLogin.Text == "Login")
            {
                // Check that all the input boxes are filled in
                if (txtFirst.Text.Length == 0)
                {
                    txtFirst.Select();
                    return;
                }
                if (txtLast.Text.Length == 0)
                {
                    txtLast.Select();
                    return;
                }
                if (txtPass.Text.Length == 0)
                {
                    txtPass.Select();
                    return;
                }

                // Disable input controls
                txtFirst.Enabled = txtLast.Enabled = txtPass.Enabled = false;
                cmdLogin.Text = "Logout";

                // Sanity check that we aren't already logged in
                if (Client != null && Client.Network.Connected)
                {
                    Client.Network.Logout();
                }

                // Re-initialize everything
                InitializeObjects();

                // Start the login
                LoginParams loginParams = Client.Network.DefaultLoginParams(txtFirst.Text, txtLast.Text,
                    txtPass.Text, "Prim Preview", "0.0.1");

                if (!String.IsNullOrEmpty(cboServer.Text))
                    loginParams.URI = cboServer.Text;

                Client.Network.BeginLogin(loginParams);
            }
            else
            {
                DoLogout();
            }
        }

        private void DoLogout()
        {
            if (Client != null && Client.Network.Connected)
            {
                Client.Network.Logout();
                return;
            }

            // Clear the download list
            lstDownloads.Items.Clear();

            // Set the login button back to login state
            cmdLogin.Text = "Login";

            // Enable input controls
            txtFirst.Enabled = txtLast.Enabled = txtPass.Enabled = true;
        }

        private void glControl_Resize(object sender, EventArgs e)
        {
            Gl.glClearColor(0.39f, 0.58f, 0.93f, 1.0f);

            Gl.glViewport(0, 0, glControl.Width, glControl.Height);

            Gl.glPushMatrix();
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();

            SetPerspective();

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPopMatrix();

            // Set the center of the glControl as the default pivot point
            LastPivot = glControl.PointToScreen(new Point(glControl.Width / 2, glControl.Height / 2));
        }

        private void glControl_MouseClick(object sender, MouseEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Alt) == 0 && e.Button == MouseButtons.Left)
            {
                // Only allow clicking if alt is not being held down
                ClickX = e.X;
                ClickY = e.Y;
                Clicked = true;
            }
        }

        private void glControl_MouseDown(object sender, MouseEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Alt) != 0 && LastHit > 0)
            {
                // Alt is held down and we have a valid target
                Pivoting = true;
                PivotPosition = Camera.FocalPoint;

                Control control = (Control)sender;
                LastPivot = control.PointToScreen(new Point(e.X, e.Y));
            }
        }

        private void glControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (Pivoting)
            {
                float a,x,y,z;

                Control control = (Control)sender;
                Point mouse = control.PointToScreen(new Point(e.X, e.Y));

                // Calculate the deltas from the center of the control to the current position
                int deltaX = (int)((mouse.X - LastPivot.X) * -0.5d);
                int deltaY = (int)((mouse.Y - LastPivot.Y) * -0.5d);

                // Translate so the focal point is the origin
                Vector3 altered = Camera.Position - Camera.FocalPoint;

                // Rotate the translated point by deltaX
                a = (float)deltaX * DEG_TO_RAD;
                x = (float)((altered.X * Math.Cos(a)) - (altered.Y * Math.Sin(a)));
                y = (float)((altered.X * Math.Sin(a)) + (altered.Y * Math.Cos(a)));

                altered.X = x;
                altered.Y = y;

                // Rotate the translated point by deltaY
                a = (float)deltaY * DEG_TO_RAD;
                y = (float)((altered.Y * Math.Cos(a)) - (altered.Z * Math.Sin(a)));
                z = (float)((altered.Y * Math.Sin(a)) + (altered.Z * Math.Cos(a)));

                altered.Y = y;
                altered.Z = z;

                // Translate back to world space
                altered += Camera.FocalPoint;

                // Update the camera
                Camera.Position = altered;
                UpdateCamera();

                // Update the pivot point
                LastPivot = mouse;
            }
        }

        private void glControl_MouseWheel(object sender, MouseEventArgs e)
        {
            /*if (e.Delta != 0)
            {
                Camera.Zoom = Camera.Zoom + (double)(e.Delta / 120) * -0.1d;
                if (Camera.Zoom < 0.05d) Camera.Zoom = 0.05d;
                UpdateCamera();
            }*/

            if (e.Delta != 0)
            {
                // Calculate the distance to move to/away
                float dist = (float)(e.Delta / 120) * 10.0f;

                if (Vector3.Distance(Camera.Position, Camera.FocalPoint) > dist)
                {
                    // Move closer or further away from the focal point
                    Vector3 toFocal = Camera.FocalPoint - Camera.Position;
                    toFocal.Normalize();

                    toFocal = toFocal * dist;

                    Camera.Position += toFocal;
                    UpdateCamera();
                }
            }
        }

        private void glControl_MouseUp(object sender, MouseEventArgs e)
        {
            // Stop pivoting if we were previously
            Pivoting = false;
        }

        private void txtLogin_Enter(object sender, EventArgs e)
        {
            TextBox input = (TextBox)sender;
            input.SelectAll();
        }

        private void cmdTeleport_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtSim.Text))
            {
                // Parse X/Y/Z
                int x, y, z;
                if (!Int32.TryParse(txtX.Text, out x))
                {
                    txtX.SelectAll();
                    return;
                }
                if (!Int32.TryParse(txtY.Text, out y))
                {
                    txtY.SelectAll();
                    return;
                }
                if (!Int32.TryParse(txtZ.Text, out z))
                {
                    txtZ.SelectAll();
                    return;
                }

                string simName = txtSim.Text.Trim().ToLower();
                Vector3 position = new Vector3(x, y, z);

                if (Client != null && Client.Network.CurrentSim != null)
                {
                    // Check for a local teleport to shortcut the process
                    if (simName == Client.Network.CurrentSim.Name.ToLower())
                    {
                        // Local teleport
                        Client.Self.RequestTeleport(Client.Network.CurrentSim.Handle, position);
                    }
                    else
                    {
                        // Cross-sim teleport
                        bool success = false;

                        BackgroundWorker worker = new BackgroundWorker();
                        worker.DoWork += delegate(object s, DoWorkEventArgs ea) { success = Client.Self.Teleport(simName, position); };
                        worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs ea)
                        {
                            BeginInvoke((MethodInvoker)
                            delegate()
                            {
                                if (!success)
                                    System.Windows.Forms.MessageBox.Show("Teleport failed");
                            });
                        };

                        worker.RunWorkerAsync();
                    }
                }
                else
                {
                    // Oops! How did the user click this...
                    cmdTeleport.Enabled = false;
                }
            }
        }
    }

    public struct TextureInfo
    {
        /// <summary>OpenGL Texture ID</summary>
        public int ID;
        /// <summary>True if this texture has an alpha component</summary>
        public bool Alpha;

        public TextureInfo(int id, bool alpha)
        {
            ID = id;
            Alpha = alpha;
        }
    }

    public struct HeightmapLookupValue : IComparable<HeightmapLookupValue>
    {
        public int Index;
        public float Value;

        public HeightmapLookupValue(int index, float value)
        {
            Index = index;
            Value = value;
        }

        public int CompareTo(HeightmapLookupValue val)
        {
            return Value.CompareTo(val.Value);
        }
    }

    public struct RenderablePrim
    {
        public Primitive Prim;
        public FacetedMesh Mesh;

        public readonly static RenderablePrim Empty = new RenderablePrim();
    }

    public struct Camera
    {
        public Vector3 Position;
        public Vector3 FocalPoint;
        public double Zoom;
        public double Far;
    }

    public struct NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Message
        {
            public IntPtr HWnd;
            public uint Msg;
            public IntPtr WParam;
            public IntPtr LParam;
            public uint Time;
            public System.Drawing.Point Point;
        }

        //[System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);
    }

    public static class Math3D
    {
        // Column-major:
        // |  0  4  8 12 |
        // |  1  5  9 13 |
        // |  2  6 10 14 |
        // |  3  7 11 15 |

        public static float[] CreateTranslationMatrix(Vector3 v)
        {
            float[] mat = new float[16];

            mat[12] = v.X;
            mat[13] = v.Y;
            mat[14] = v.Z;
            mat[0] = mat[5] = mat[10] = mat[15] = 1;

            return mat;
        }

        public static float[] CreateRotationMatrix(Quaternion q)
        {
            float[] mat = new float[16];

            // Transpose the quaternion (don't ask me why)
            q.X = q.X * -1f;
            q.Y = q.Y * -1f;
            q.Z = q.Z * -1f;

            float x2 = q.X + q.X;
            float y2 = q.Y + q.Y;
            float z2 = q.Z + q.Z;
            float xx = q.X * x2;
            float xy = q.X * y2;
            float xz = q.X * z2;
            float yy = q.Y * y2;
            float yz = q.Y * z2;
            float zz = q.Z * z2;
            float wx = q.W * x2;
            float wy = q.W * y2;
            float wz = q.W * z2;

            mat[0] = 1.0f - (yy + zz);
            mat[1] = xy - wz;
            mat[2] = xz + wy;
            mat[3] = 0.0f;

            mat[4] = xy + wz;
            mat[5] = 1.0f - (xx + zz);
            mat[6] = yz - wx;
            mat[7] = 0.0f;

            mat[8] = xz - wy;
            mat[9] = yz + wx;
            mat[10] = 1.0f - (xx + yy);
            mat[11] = 0.0f;

            mat[12] = 0.0f;
            mat[13] = 0.0f;
            mat[14] = 0.0f;
            mat[15] = 1.0f;

            return mat;
        }

        public static float[] CreateScaleMatrix(Vector3 v)
        {
            float[] mat = new float[16];

            mat[0] = v.X;
            mat[5] = v.Y;
            mat[10] = v.Z;
            mat[15] = 1;

            return mat;
        }
    }
}
