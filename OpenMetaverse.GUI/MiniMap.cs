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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenMetaverse.Imaging;
using OpenMetaverse.Assets;

namespace OpenMetaverse.GUI
{
    /// <summary>
    /// PictureBox GUI component for displaying a client's mini-map
    /// </summary>
    public class MiniMap : PictureBox
    {
        private static Brush BG_COLOR = Brushes.Navy;

        private UUID _MapImageID;
        private GridClient _Client;
        private Image _MapLayer;
        //warning CS0414: The private field `OpenMetaverse.GUI.MiniMap._MousePosition' is assigned but its value is never used
        //private Point _MousePosition;
        ToolTip _ToolTip;

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

            _ToolTip = new ToolTip();
            _ToolTip.Active = true;
            _ToolTip.AutomaticDelay = 1;

            this.MouseHover += new System.EventHandler(MiniMap_MouseHover);
            this.MouseMove += new MouseEventHandler(MiniMap_MouseMove);
        }

        /// <summary>Sets the map layer to the specified bitmap image</summary>
        /// <param name="mapImage"></param>
        public void SetMapLayer(Bitmap mapImage)
        {
            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { SetMapLayer(mapImage); });
            else
            {
                if (mapImage == null)
                {
                    Bitmap bmp = new Bitmap(256, 256);
                    Graphics g = Graphics.FromImage(bmp);
                    g.Clear(this.BackColor);
                    g.FillRectangle(BG_COLOR, 0f, 0f, 256f, 256f);
                    g.DrawImage(bmp, 0, 0);

                    _MapLayer = bmp;
                }
                else _MapLayer = mapImage;
            }
        }

        private void InitializeClient(GridClient client)
        {
            _Client = client;
            _Client.Grid.OnCoarseLocationUpdate += new GridManager.CoarseLocationUpdateCallback(Grid_OnCoarseLocationUpdate);
            _Client.Network.OnCurrentSimChanged += new NetworkManager.CurrentSimChangedCallback(Network_OnCurrentSimChanged);
        }

        private void UpdateMiniMap(Simulator sim)
        {
            if (!this.IsHandleCreated) return;

            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { UpdateMiniMap(sim); });
            else
            {
                if (_MapLayer == null)
                    SetMapLayer(null);

                Bitmap bmp = (Bitmap)_MapLayer.Clone();
                Graphics g = Graphics.FromImage(bmp);

                Vector3 myCoarsePos;

                if (!sim.AvatarPositions.TryGetValue(Client.Self.AgentID, out myCoarsePos)) return;

                int i = 0;

                _Client.Network.CurrentSim.AvatarPositions.ForEach(
                    delegate(KeyValuePair<UUID, Vector3> coarse)
                    {
                        int x = (int)coarse.Value.X;
                        int y = 255 - (int)coarse.Value.Y;
                        if (coarse.Key == Client.Self.AgentID)
                        {
                            g.FillEllipse(Brushes.Yellow, x - 5, y - 5, 10, 10);
                            g.DrawEllipse(Pens.Khaki, x - 5, y - 5, 10, 10);
                        }
                        else
                        {
                            Pen penColor;
                            Brush brushColor;

                            if (Client.Network.CurrentSim.ObjectsAvatars.Find(delegate(Avatar av) { return av.ID == coarse.Key; }) != null)
                            {
                                brushColor = Brushes.PaleGreen;
                                penColor = Pens.Green;
                            }
                            else
                            {
                                brushColor = Brushes.LightGray;
                                penColor = Pens.Gray;
                            }

                            if (myCoarsePos.Z - coarse.Value.Z > 1)
                            {
                                Point[] points = new Point[3] { new Point(x - 6, y - 6), new Point(x + 6, y - 6), new Point(x, y + 6) };
                                g.FillPolygon(brushColor, points);
                                g.DrawPolygon(penColor, points);
                            }

                            else if (myCoarsePos.Z - coarse.Value.Z < -1)
                            {
                                Point[] points = new Point[3] { new Point(x - 6, y + 6), new Point(x + 6, y + 6), new Point(x, y - 6) };
                                g.FillPolygon(brushColor, points);
                                g.DrawPolygon(penColor, points);
                            }

                            else
                            {
                                g.FillEllipse(brushColor, x - 5, y - 5, 10, 10);
                                g.DrawEllipse(penColor, x - 5, y - 5, 10, 10);
                            }
                        }
                        i++;
                    }
                );

                g.DrawImage(bmp, 0, 0);
                this.Image = bmp;
            }
        }

        void MiniMap_MouseHover(object sender, System.EventArgs e)
        {
            _ToolTip.SetToolTip(this, "test");
            _ToolTip.Show("test", this);
            //TODO: tooltip popup with closest avatar's name, if within range
        }

        void MiniMap_MouseMove(object sender, MouseEventArgs e)
        {
            _ToolTip.Hide(this);
            //warning CS0414: The private field `OpenMetaverse.GUI.MiniMap._MousePosition' is assigned but its value is never used
            //_MousePosition = e.Location;
        }

        void Grid_OnCoarseLocationUpdate(Simulator sim, List<UUID> newEntries, List<UUID> removedEntries)
        {
            UpdateMiniMap(sim);
        }

        void Network_OnCurrentSimChanged(Simulator PreviousSimulator)
        {
            GridRegion region;
            if (Client.Grid.GetGridRegion(Client.Network.CurrentSim.Name, GridLayerType.Objects, out region))
            {
                SetMapLayer(null);

                _MapImageID = region.MapImageID;
                ManagedImage nullImage;

                Client.Assets.RequestImage(_MapImageID, ImageType.Baked, 
                    delegate(TextureRequestState state, AssetTexture asset)
                        {
                            if(state == TextureRequestState.Finished)
                                OpenJPEG.DecodeToImage(asset.AssetData, out nullImage, out _MapLayer);
                        });
            }
        }       

    }
}
