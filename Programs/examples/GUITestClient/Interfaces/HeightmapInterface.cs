using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using OpenMetaverse;

namespace OpenMetaverse.GUITestClient
{
    public class HeightmapInterface : Interface
    {
        private PictureBox[,] Boxes = new PictureBox[16, 16];

        public HeightmapInterface(frmTestClient testClient)
        {
            Name = "Heightmap";
            Description = "Displays a graphical heightmap of the current simulator";
        }

        public override void Initialize()
        {
            Client.Terrain.OnLandPatch += new TerrainManager.LandPatchCallback(Terrain_OnLandPatch);
            Client.Settings.STORE_LAND_PATCHES = true;

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
                    //box.MouseUp += new MouseEventHandler(box_MouseUp);
                    ((System.ComponentModel.ISupportInitialize)(box)).EndInit();

                    TabPage.Controls.Add(box);
                }
            }
        }

        public override void Paint(object sender, PaintEventArgs e)
        {
            ;
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
                    int colorVal = Utils.FloatToByte(height, 0.0f, 60.0f);
                    int lesserVal = (int)((float)colorVal * 0.75f);
                    Color color;

                    if (height >= simulator.WaterHeight)
                        color = Color.FromArgb(lesserVal, colorVal, lesserVal);
                    else
                        color = Color.FromArgb(lesserVal, lesserVal, colorVal);

                    patch.SetPixel(xp, yp, color);
                }
            }

            Boxes[x, y].Image = (System.Drawing.Image)patch;
        }
    }
}
