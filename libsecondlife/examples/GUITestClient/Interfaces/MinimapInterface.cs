using System;
using System.ComponentModel;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using libsecondlife;
using System.Net;
using System.Diagnostics;

namespace libsecondlife.GUITestClient
{
    public class MinimapInterface : Interface
    {
        //A magic number to calculate index sim y coord from actual coord
        private const int GRID_Y_OFFSET = 1279;
        //Base URL for web map api sim images
        private const String MAP_IMG_URL = "http://secondlife.com/apps/mapapi/grid/map_image/";
        private PictureBox world = new PictureBox();
        private Button cmdRefresh = new Button();
        private System.Drawing.Image mMapImage = null;
        private string oldSim = String.Empty;

        public MinimapInterface(frmTestClient testClient)
        {
            Name = "Minimap";
            Description = "Displays a graphical minimap of the current simulator";
        }

        private void map_onclick(object sender, System.EventArgs e)
        {
            ;
        }

        private void cmdRefresh_onclick(object sender, System.EventArgs e)
        {
            printMap();
        }

        public void printMap()
        {
            Bitmap map      = new Bitmap(256, 256, PixelFormat.Format32bppRgb);
            Font font       = new Font("Tahoma", 8, FontStyle.Bold);
            Pen mAvPen      = new Pen(Brushes.GreenYellow, 1);
            Brush mAvBrush  = new SolidBrush(Color.Green);
            String strInfo  = String.Empty;

            // Get new background map if necessary
            if (mMapImage == null || oldSim != Client.Network.CurrentSim.Name)
            {
                oldSim = Client.Network.CurrentSim.Name;
                mMapImage = DownloadWebMapImage();
            }

            // Create in memory bitmap   
            using (Graphics g = Graphics.FromImage(map))
            {
                // Draw background map
                g.DrawImage(mMapImage, new Rectangle(0, 0, 256, 256), 0, 0, 256, 256, GraphicsUnit.Pixel);

                // Draw all avatars
                Client.Network.CurrentSim.AvatarPositions.ForEach(
                    delegate(LLVector3 pos)
                    {
                        Rectangle rect = new Rectangle((int)Math.Round(pos.X, 0) - 2, 255 - ((int)Math.Round(pos.Y, 0) - 2), 4, 4);
                        g.FillEllipse(mAvBrush, rect);
                        g.DrawEllipse(mAvPen, rect);
                    }
                );

                // Draw self ;)
                Rectangle myrect = new Rectangle((int)Math.Round(Client.Self.SimPosition.X, 0) - 3, 255 - ((int)Math.Round(Client.Self.SimPosition.Y, 0) - 3), 6, 6);
                g.FillEllipse(new SolidBrush(Color.Yellow), myrect);
                g.DrawEllipse(new Pen(Brushes.Goldenrod, 1), myrect);

                // Draw region info
                strInfo = string.Format("Sim {0}/{1}/{2}/{3}\nAvatars {4}", Client.Network.CurrentSim.Name,
                                                                            Math.Round(Client.Self.SimPosition.X, 0),
                                                                            Math.Round(Client.Self.SimPosition.Y, 0),
                                                                            Math.Round(Client.Self.SimPosition.Z, 0),
                                                                            Client.Network.CurrentSim.AvatarPositions.Count);
                g.DrawString(strInfo, font, Brushes.DarkOrange, 4, 4);
            }
            // update picture box with new map bitmap
            world.BackgroundImage = map;
        }

        public override void Initialize()
        {
            ((System.ComponentModel.ISupportInitialize)(world)).BeginInit();
            world.BorderStyle   = System.Windows.Forms.BorderStyle.FixedSingle;
            world.Size          = new System.Drawing.Size(256, 256);
            world.Visible       = true;
            world.Click         += new System.EventHandler(this.map_onclick);
            ((System.ComponentModel.ISupportInitialize)(world)).EndInit();

            //((System.ComponentModel.ISupportInitialize)(cmdRefresh)).BeginInit();
            cmdRefresh.Text     = "Refresh";
            cmdRefresh.Size     = new System.Drawing.Size(80, 24);
            cmdRefresh.Left     = world.Left + world.Width + 20;
            cmdRefresh.Click    += new System.EventHandler(this.cmdRefresh_onclick);
            cmdRefresh.Visible  = true;
            //((System.ComponentModel.ISupportInitialize)(world)).EndInit();

            TabPage.Controls.Add(world);
            TabPage.Controls.Add(cmdRefresh);
        }

        // Ripped from "Terrain Scultor" by Cadroe with minors changes
        // http://spinmass.blogspot.com/2007/08/terrain-sculptor-maps-sims-and-creates.html
        private System.Drawing.Image DownloadWebMapImage()
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            String imgURL = "";
            GridRegion currRegion;

            Client.Grid.GetGridRegion(Client.Network.CurrentSim.Name, GridLayerType.Terrain, out currRegion);
            try
            {
                //Form the URL using the sim coordinates
                imgURL = MAP_IMG_URL + currRegion.X.ToString() + "-" +
                        (GRID_Y_OFFSET - currRegion.Y).ToString() + "-1-0";
                //Make the http request
                request = (HttpWebRequest)HttpWebRequest.Create(imgURL);
                request.Timeout = 5000;
                request.ReadWriteTimeout = 20000;
                response = (HttpWebResponse)request.GetResponse();

                return System.Drawing.Image.FromStream(response.GetResponseStream());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error Downloading Web Map Image");
                return null;
            }
        }

        public override void Paint(object sender, PaintEventArgs e)
        {
            ;
        }
    }
}
