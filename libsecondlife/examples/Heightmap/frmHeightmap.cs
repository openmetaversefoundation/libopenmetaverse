using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using libsecondlife;
using libsecondlife.Packets;

namespace Heightmap
{
    public partial class frmHeightmap : Form
    {
        private SecondLife Client = new SecondLife();
        private PictureBox[,] Boxes = new PictureBox[16, 16];
        private System.Timers.Timer UpdateTimer = new System.Timers.Timer(1000);
        private string FirstName, LastName, Password;

        double heading = -Math.PI;

        public frmHeightmap(string firstName, string lastName, string password)
        {
            FirstName = firstName;
            LastName = lastName;
            Password = password;

            // Throttle land up and other things down
            Client.Throttle.Cloud = 0;
            Client.Throttle.Land = 1000000;
            Client.Throttle.Wind = 0;

            Client.Settings.MULTIPLE_SIMS = false;

            // Build the picture boxes
            this.SuspendLayout();
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    Boxes[x, y] = new System.Windows.Forms.PictureBox();
                    PictureBox box = Boxes[x, y];
                    ((System.ComponentModel.ISupportInitialize)(box)).BeginInit();
                    box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                    box.Name = x + "," + y;
                    box.Location = new System.Drawing.Point(x * 16, y * 16);
                    box.Size = new System.Drawing.Size(16, 16);
                    box.Visible = true;
                    box.MouseUp += new MouseEventHandler(box_MouseUp);
                    ((System.ComponentModel.ISupportInitialize)(box)).EndInit();

                    this.Controls.Add(box);
                }
            }
            this.ResumeLayout();

            InitializeComponent();
        }

        private void frmHeightmap_Load(object sender, EventArgs e)
        {
            Client.Terrain.OnLandPatch += new TerrainManager.LandPatchCallback(Terrain_OnLandPatch);
            // Only needed so we can do lookups with TerrainHeightAtPoint
            Client.Settings.STORE_LAND_PATCHES = true;

            if (!Client.Network.Login(FirstName, LastName, Password, "Heightmap", "jhurliman@wsu.edu"))
            {
                Console.WriteLine("Login failed: " + Client.Network.LoginMessage);
                Console.ReadKey();
                this.Close();
                return;
            }
            else
            {
                UpdateTimer.Elapsed += new System.Timers.ElapsedEventHandler(UpdateTimer_Elapsed);
                UpdateTimer.Start();
            }
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
                            MessageBox.Show(height.ToString());
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

            Client.Self.Status.UpdateFromHeading(heading, false);
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
                    float height = data[yp * 16 + xp];
                    int colorVal = Helpers.FloatToByte(height, 0.0f, 60.0f);
                    int lesserVal = (int)((float)colorVal * 0.75f);
                    Color color;

                    if (height >= simulator.WaterHeight)
                        color = Color.FromArgb(lesserVal, colorVal, lesserVal);
                    else
                        color = Color.FromArgb(lesserVal, lesserVal, colorVal);

                    patch.SetPixel(xp, yp, color);
                }
            }

            Boxes[x, y].Image = (Image)patch;
        }

        private void frmHeightmap_FormClosing(object sender, FormClosingEventArgs e)
        {
            Client.Network.Logout();
        }
    }
}