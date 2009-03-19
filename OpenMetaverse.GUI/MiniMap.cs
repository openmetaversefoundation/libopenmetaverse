/*
 * Copyright (c) 2007-2008, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names 
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 */

using OpenMetaverse.Imaging;
using System.Drawing;
using System.Windows.Forms;

namespace OpenMetaverse.GUI
{
    public class MiniMap : PictureBox
    {
        private UUID _MapImageID;
        private GridClient _Client;
        private Image _MapLayer;

        /// <summary>
        /// Gets or sets the GridClient associated with this control
        /// </summary>
        public GridClient Client
        {
            get { return _Client; }
            set { if (value != null) InitializeClient(value); }
        }

        /// <summary>
        /// PictureBox control for an unspecified client's mini-map
        /// </summary>
        public MiniMap()
        {
            this.BorderStyle = BorderStyle.FixedSingle;
            this.SizeMode = PictureBoxSizeMode.Zoom;
        }

        /// <summary>
        /// PictureBox control for the specified client's mini-map
        /// </summary>
        public MiniMap(GridClient client) : this ()
        {
            InitializeClient(client);
        }

        private void InitializeClient(GridClient client)
        {
            _Client = client;
            _Client.Assets.OnImageReceived += new AssetManager.ImageReceivedCallback(Assets_OnImageReceived);
            _Client.Grid.OnCoarseLocationUpdate += new GridManager.CoarseLocationUpdateCallback(Grid_OnCoarseLocationUpdate);
            _Client.Network.OnCurrentSimChanged += new NetworkManager.CurrentSimChangedCallback(Network_OnCurrentSimChanged);
        }

        void Assets_OnImageReceived(ImageDownload image, AssetTexture asset)
        {
            if (asset.AssetID == _MapImageID)
            {
                ManagedImage nullImage;
                OpenJPEG.DecodeToImage(asset.AssetData, out nullImage, out _MapLayer);
            }
        }

        void Network_OnCurrentSimChanged(Simulator PreviousSimulator)
        {
            GridRegion region;
            if (Client.Grid.GetGridRegion(Client.Network.CurrentSim.Name, GridLayerType.Objects, out region))
            {
                _MapImageID = region.MapImageID;
                Client.Assets.RequestImage(_MapImageID, ImageType.Baked);
            }
        }

        private void UpdateMiniMap(Simulator sim)
        {
            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { UpdateMiniMap(sim); });
            else
            {
                Bitmap bmp = _MapLayer == null ? new Bitmap(256, 256) : (Bitmap)_MapLayer.Clone();
                Graphics g = Graphics.FromImage(bmp);

                if (_MapLayer == null)
                {
                    g.Clear(this.BackColor);
                    g.FillRectangle(Brushes.White, 0f, 0f, 256f, 256f);
                }

                if (!sim.AvatarPositions.ContainsKey(Client.Self.AgentID)) return;

                int i = 0;

                Vector3 myPos = sim.AvatarPositions[_Client.Self.AgentID];

                foreach (Vector3 pos in _Client.Network.CurrentSim.AvatarPositions.Values)
                {
                    int x = (int)pos.X;
                    int y = 255 - (int)pos.Y;
                    if (i == _Client.Network.CurrentSim.PositionIndexYou)
                    {
                        g.FillEllipse(Brushes.PaleGreen, x - 5, y - 5, 10, 10);
                        g.DrawEllipse(Pens.Green, x - 5, y - 5, 10, 10);
                    }
                    else
                    {
                        if (myPos.Z - (pos.Z * 4) > 5)
                        {
                            Point[] points = new Point[3] { new Point(x - 6, y - 6), new Point(x + 6, y - 6), new Point(x, y + 6) };
                            g.FillPolygon(Brushes.Red, points);
                            g.DrawPolygon(Pens.DarkRed, points);
                        }

                        else if (myPos.Z - (pos.Z * 4) < -5)
                        {
                            Point[] points = new Point[3] { new Point(x - 6, y + 6), new Point(x + 6, y + 6), new Point(x, y - 6) };
                            g.FillPolygon(Brushes.Red, points);
                            g.DrawPolygon(Pens.DarkRed, points);
                        }

                        else
                        {
                            g.FillEllipse(Brushes.Red, x - 5, y - 5, 10, 10);
                            g.DrawEllipse(Pens.DarkRed, x - 5, y - 5, 10, 10);
                        }
                    }
                    i++;
                };

                g.DrawImage(bmp, 0, 0);
                this.Image = bmp;
            }
        }

        private void Grid_OnCoarseLocationUpdate(Simulator sim)
        {
            UpdateMiniMap(sim);
        }

    }
}
