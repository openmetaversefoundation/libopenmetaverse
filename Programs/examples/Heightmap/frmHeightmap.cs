using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Heightmap
{
    public partial class frmHeightmap : Form
    {
        private GridClient Client = new GridClient();
        private PictureBox[,] Boxes = new PictureBox[16, 16];
        private System.Timers.Timer UpdateTimer = new System.Timers.Timer(1000);
        private string FirstName, LastName, Password;

        double heading = -Math.PI;

        public frmHeightmap(string firstName, string lastName, string password)
        {
            FirstName = firstName;
            LastName = lastName;
            Password = password;

            Client.Network.LoginProgress += Network_OnLogin;

            // Throttle land up and other things down
            Client.Throttle.Cloud = 0;
            Client.Throttle.Land = 1000000;
            Client.Throttle.Wind = 0;

            Client.Settings.MULTIPLE_SIMS = false;

            // Build the picture boxes
            this.SuspendLayout();
            for (int y = 0; y < 16; y++) // Box 0,0 is on the top left
            {
                for (int x = 0; x < 16; x++)
                {
                    Boxes[x, y] = new System.Windows.Forms.PictureBox();
                    PictureBox box = Boxes[x, y];
                    ((System.ComponentModel.ISupportInitialize)(box)).BeginInit();
                    box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                    box.Name = x + "," + y;
                    box.Location = new System.Drawing.Point(x * 18, y * 18);
                    box.Size = new System.Drawing.Size(18, 18);
                    box.Visible = true;
                    box.MouseUp += new MouseEventHandler(box_MouseUp);
                    ((System.ComponentModel.ISupportInitialize)(box)).EndInit();

                    this.Controls.Add(box);
                }
            }
            this.ResumeLayout();

            InitializeComponent();
        }

        private void Network_OnLogin(object sender, LoginProgressEventArgs e)
        {
            if (e.Status == LoginStatus.Success)
            {
                UpdateTimer.Elapsed += new System.Timers.ElapsedEventHandler(UpdateTimer_Elapsed);
                UpdateTimer.Start();
            }
            else if (e.Status == LoginStatus.Failed)
            {
                Console.WriteLine("Login failed: " + Client.Network.LoginMessage);
                Console.ReadKey();
                this.Close();
                return;
            }
        }

        private void frmHeightmap_Load(object sender, EventArgs e)
        {
            Client.Terrain.OnLandPatch += new TerrainManager.LandPatchCallback(Terrain_OnLandPatch);
            // Only needed so we can do lookups with TerrainHeightAtPoint
            Client.Settings.STORE_LAND_PATCHES = true;

            LoginParams loginParams = Client.Network.DefaultLoginParams(FirstName, LastName, Password, "Heightmap",
                "1.0.0");
            Client.Network.BeginLogin(loginParams);

            this.SetDesktopLocation(1600, 0);
            // FIXME: This really should be modified in frmHeightmap.Designer.cs, but the Prebuild bug is
            // preventing that right now
            this.SetClientSizeCore(18 * 16, 18 * 16);
        }

        private void box_MouseUp(object sender, MouseEventArgs e)
        {
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    if (Boxes[x, y] == sender)
                    {
                        float height;
                        if (Client.Terrain.TerrainHeightAtPoint(Client.Network.CurrentSim.Handle,
                            x * 16 + e.X, y * 16 + e.Y, out height))
                        {
                            MessageBox.Show( string.Format("{0},{1}:{2}",x*16+e.X,255-(y*16+e.Y),height) );
                        }
                        else
                        {
                            MessageBox.Show("Unknown height");
                        }
                        return;
                    }
                }
            }
        }

        void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Spin our camera in circles at the center of the sim to load all the terrain
            heading += 0.5d;
            if (heading > Math.PI) heading = -Math.PI;

            Client.Self.Movement.UpdateFromHeading(heading, false);
        }

        void Terrain_OnLandPatch(Simulator simulator, int x, int y, int width, float[] data)
        {
            if (x >= 16 || y >= 16)
            {
                Console.WriteLine("Bad patch coordinates, x = " + x + ", y = " + y);
                return;
            }

            if (width != 16)
            {
                Console.WriteLine("Unhandled patch size " + width + "x" + width);
                return;
            }

            Bitmap patch = new Bitmap(16, 16, PixelFormat.Format24bppRgb);

            for (int yp = 0; yp < 16; yp++)
            {
                for (int xp = 0; xp < 16; xp++)
                {
                    float height = data[(15-yp) * 16 + xp]; // data[0] is south west
                    Color color;
                    if (height >= simulator.WaterHeight)
                    {
                        float maxVal = (float)Math.Log(Math.Abs(512+1-simulator.WaterHeight),2);
                        float lgHeight = (float)Math.Log(Math.Abs(height + 1 - simulator.WaterHeight), 2);
                        int colorVal1 = Utils.FloatToByte(lgHeight, simulator.WaterHeight, maxVal);
                        int colorVal2 = Utils.FloatToByte(height, simulator.WaterHeight, 25.0f);
                        color = Color.FromArgb(255, colorVal2, colorVal1);
                    }
                    else
                    {
                        const float minVal = -5.0f;
                        float maxVal = simulator.WaterHeight;
                        int colorVal1 = Utils.FloatToByte(height, -5.0f, minVal + (maxVal - minVal) * 1.5f);
                        int colorVal2 = Utils.FloatToByte(height, -5.0f, maxVal);
                        color = Color.FromArgb(colorVal1, colorVal2, 255);
                    }
                    patch.SetPixel(xp, yp, color); // 0, 0 is top left
                }
            }

            Boxes[x, 15-y].Image = (System.Drawing.Image)patch;
        }

        private void frmHeightmap_FormClosing(object sender, FormClosingEventArgs e)
        {
            Client.Network.Logout();
        }
    }
}
