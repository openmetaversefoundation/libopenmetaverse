using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using libsecondlife;

namespace Heightmap
{
    public partial class frmHeightmap : Form
    {
        private SecondLife Client = new SecondLife();
        private System.Windows.Forms.PictureBox[,] Boxes = new System.Windows.Forms.PictureBox[16, 16];
        private string FirstName, LastName, Password;

        public frmHeightmap(string firstName, string lastName, string password)
        {
            FirstName = firstName;
            LastName = lastName;
            Password = password;

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

            Dictionary<string, object> loginvals = Client.Network.DefaultLoginValues(FirstName, LastName, Password,
                "Heightmap", "jhurliman@wsu.edu");

            if (!Client.Network.Login(loginvals)) //, "https://login.aditi.lindenlab.com/cgi-bin/login.cgi"))
            {
                Console.WriteLine("Login failed: " + Client.Network.LoginError);
                Console.ReadKey();
                this.Close();
                return;
            }
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
                    int color = Helpers.FloatToByte(height, 0.0f, 40.0f);
                    patch.SetPixel(xp, yp, Color.FromArgb(color, color, color));
                }
            }

            Boxes[x, y].Image = (Image)patch;
        }
    }
}