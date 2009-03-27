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

using OpenMetaverse;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace OpenMetaverse.GUI
{

    /// <summary>
    /// Contains any available information for an avatar in the simulator.
    /// A null value for .Avatar indicates coarse data for an avatar outside of visible range.
    /// </summary>
    public class TrackedAvatar
    {
        /// <summary>Assigned if the avatar is within visible range</summary>
        public Avatar Avatar = null;

        /// <summary>Last known coarse location of avatar</summary>
        public Vector3 CoarseLocation;

        /// <summary>Avatar ID</summary>
        public UUID ID;

        /// <summary>ListViewItem associated with this avatar</summary>
        public ListViewItem ListViewItem;

        /// <summary>Populated by RequestAvatarName if avatar is not visible</summary>
        public string Name = "(Loading...)";
    }

    /// <summary>
    /// ListView GUI component for viewing a client's nearby avatars list
    /// </summary>
    public class AvatarList : ListView
    {
        private GridClient _Client;
        private ColumnSorter _ColumnSorter = new ColumnSorter();

        private DoubleDictionary<uint, UUID, TrackedAvatar> _TrackedAvatars = new DoubleDictionary<uint, UUID, TrackedAvatar>();
        private Dictionary<UUID, TrackedAvatar> _UntrackedAvatars = new Dictionary<UUID, TrackedAvatar>();

        public delegate void AvatarDoubleClickCallback(TrackedAvatar trackedAvatar);

        /// <summary>
        /// Triggered when the user double clicks on an avatar in the list
        /// </summary>
        public event AvatarDoubleClickCallback OnAvatarDoubleClick;

        /// <summary>
        /// Gets or sets the GridClient associated with this control
        /// </summary>
        public GridClient Client
        {
            get { return _Client; }
            set { if (value != null) InitializeClient(value); }
        }

        /// <summary>
        /// TreeView control for an unspecified client's nearby avatar list
        /// </summary>
        public AvatarList()
        {
            ColumnHeader header1 = this.Columns.Add("Name");
            header1.Width = this.Width - 20;

            ColumnHeader header2 = this.Columns.Add(" ");
            header2.Width = 40;

            _ColumnSorter.SortColumn = 1;
            this.Sorting = SortOrder.Ascending;
            this.ListViewItemSorter = _ColumnSorter;

            this.DoubleBuffered = true;
            this.ListViewItemSorter = _ColumnSorter;
            this.View = View.Details;
            this.ColumnClick += new ColumnClickEventHandler(AvatarList_ColumnClick);
            this.DoubleClick += new EventHandler(AvatarList_DoubleClick);
        }

        /// <summary>
        /// TreeView control for the specified client's nearby avatar list
        /// </summary>
        /// <param name="client"></param>
        public AvatarList(GridClient client) : this ()
        {
            InitializeClient(client);
        }

        /// <summary>
        /// Thread-safe method for clearing the TreeView control
        /// </summary>
        public void ClearItems()
        {
            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { ClearItems(); });
            else
            {
                if (this.Handle != IntPtr.Zero)
                    this.Items.Clear();
            }
        }

        private void InitializeClient(GridClient client)
        {
            _Client = client;
            _Client.Avatars.OnAvatarAppearance += new AvatarManager.AvatarAppearanceCallback(Avatars_OnAvatarAppearance);
            _Client.Avatars.OnAvatarNames += new AvatarManager.AvatarNamesCallback(Avatars_OnAvatarNames);
            _Client.Grid.OnCoarseLocationUpdate += new GridManager.CoarseLocationUpdateCallback(Grid_OnCoarseLocationUpdate);
            _Client.Network.OnCurrentSimChanged += new NetworkManager.CurrentSimChangedCallback(Network_OnCurrentSimChanged);
            _Client.Objects.OnNewAvatar += new ObjectManager.NewAvatarCallback(Objects_OnNewAvatar);
            _Client.Objects.OnObjectKilled += new ObjectManager.KillObjectCallback(Objects_OnObjectKilled);
            _Client.Objects.OnObjectUpdated += new ObjectManager.ObjectUpdatedCallback(Objects_OnObjectUpdated);
        }

        private void AddAvatar(uint localID, UUID avatarID, Vector3 coarsePosition, Avatar avatar)
        {
            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { AddAvatar(localID, avatarID, coarsePosition, avatar); });
            else
            {
                if (!this.IsHandleCreated) return;

                TrackedAvatar trackedAvatar = new TrackedAvatar();
                trackedAvatar.CoarseLocation = coarsePosition;
                trackedAvatar.ID = avatarID;
                trackedAvatar.ListViewItem = this.Items.Add(avatarID.ToString(), trackedAvatar.Name, null);

                string strDist = avatarID == _Client.Self.AgentID ? "--" : (int)Vector3.Distance(_Client.Self.SimPosition, coarsePosition) + "m";
                trackedAvatar.ListViewItem.SubItems.Add(strDist);

                if (avatar != null)
                {
                    trackedAvatar.Name = avatar.Name;
                    trackedAvatar.ListViewItem.Text = avatar.Name;

                    lock (_TrackedAvatars)
                        _TrackedAvatars.Add(localID, avatarID, trackedAvatar);
                }
                else
                {
                    lock (_UntrackedAvatars)
                    {
                        _UntrackedAvatars.Add(avatarID, trackedAvatar);

                        trackedAvatar.ListViewItem.ForeColor = Color.FromKnownColor(KnownColor.GrayText);

                        if (avatarID == _Client.Self.AgentID)
                        {
                            trackedAvatar.Name = _Client.Self.Name;
                            trackedAvatar.ListViewItem.Text = _Client.Self.Name;
                        }

                        else Client.Avatars.RequestAvatarName(avatarID);
                    }
                }
            }
        }

        private void RemoveAvatar(uint localID)
        {
            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { RemoveAvatar(localID); });
            else
            {
                if (!this.IsHandleCreated) return;

                lock (_TrackedAvatars)
                {
                    TrackedAvatar trackedAvatar;
                    if (_TrackedAvatars.TryGetValue(localID, out trackedAvatar))
                    {
                        this.Items.Remove(trackedAvatar.ListViewItem);
                        _TrackedAvatars.Remove(localID);
                    }
                }
            }
        }

        private void UpdateAvatar(Avatar avatar)
        {
            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { UpdateAvatar(avatar); });
            else
            {
                if (!this.IsHandleCreated) return;

                lock (_TrackedAvatars)
                {
                    TrackedAvatar trackedAvatar;

                    lock (_UntrackedAvatars)
                    {
                        if (_UntrackedAvatars.TryGetValue(avatar.ID, out trackedAvatar))
                        {
                            trackedAvatar.Name = avatar.Name;
                            trackedAvatar.ListViewItem.Text = avatar.Name;
                            trackedAvatar.ListViewItem.ForeColor = Color.FromKnownColor(KnownColor.ControlText);

                            _TrackedAvatars.Add(avatar.LocalID, avatar.ID, trackedAvatar);
                            _UntrackedAvatars.Remove(avatar.ID);
                        }
                    }
                    
                    if (_TrackedAvatars.TryGetValue(avatar.ID, out trackedAvatar))
                    {
                        trackedAvatar.Avatar = avatar;
                        trackedAvatar.Name = avatar.Name;
                        trackedAvatar.ID = avatar.ID;

                        string strDist = avatar.ID == _Client.Self.AgentID ? "--" : (int)Vector3.Distance(_Client.Self.SimPosition, avatar.Position) + "m";
                        trackedAvatar.ListViewItem.SubItems[1].Text = strDist;
                    }
                    else
                    {
                        AddAvatar(avatar.LocalID, avatar.ID, Vector3.Zero, avatar);
                    }               
                }

                this.Sort();
            }
        }

        private void UpdateCoarseInfo(Simulator sim, List<UUID> newEntries, List<UUID> removedEntries)
        {
            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { UpdateCoarseInfo(sim, newEntries, removedEntries); });
            else
            {
                lock (_UntrackedAvatars)
                {
                    for (int i = 0; i < removedEntries.Count; i++)
                    {
                        TrackedAvatar trackedAvatar;
                        if (_UntrackedAvatars.TryGetValue(removedEntries[i], out trackedAvatar))
                        {
                            this.Items.Remove(trackedAvatar.ListViewItem);
                            _UntrackedAvatars.Remove(trackedAvatar.ID);
                        }
                    }
                    for (int i = 0; i < newEntries.Count; i++)
                    {
                        int index = this.Items.IndexOfKey(newEntries[i].ToString());
                        if (index == -1)
                        {
                            Vector3 coarsePos;
                            if (!sim.AvatarPositions.TryGetValue(newEntries[i], out coarsePos))
                                continue;

                            AddAvatar(0, newEntries[i], coarsePos, null);
                        }
                    }
                }
            }
        }

        private void AvatarList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            _ColumnSorter.SortColumn = e.Column;
            if ((_ColumnSorter.Ascending = (this.Sorting == SortOrder.Ascending))) this.Sorting = SortOrder.Descending;
            else this.Sorting = SortOrder.Ascending;
            this.ListViewItemSorter = _ColumnSorter;
        }

        private void AvatarList_DoubleClick(object sender, EventArgs e)
        {
            if (OnAvatarDoubleClick != null)
            {
                ListView list = (ListView)sender;
                if (list.SelectedItems.Count > 0)
                {
                    TrackedAvatar trackedAvatar;
                    if (!_TrackedAvatars.TryGetValue(new UUID(list.SelectedItems[0].Name), out trackedAvatar)
                        && !_UntrackedAvatars.TryGetValue(new UUID(list.SelectedItems[0].Name), out trackedAvatar))
                        return;

                    try { OnAvatarDoubleClick(trackedAvatar); }
                    catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }
                }
            }
        }

        private void Avatars_OnAvatarAppearance(UUID avatarID, bool isTrial, Primitive.TextureEntryFace defaultTexture, Primitive.TextureEntryFace[] faceTextures, List<byte> visualParams)
        {
            if (visualParams.Count > 105)
            {
                lock (_TrackedAvatars)
                {
                    TrackedAvatar trackedAvatar;
                    if (_TrackedAvatars.TryGetValue(avatarID, out trackedAvatar))
                    {
                        
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            byte param = visualParams[80];
                            if (param > 117)
                                trackedAvatar.ListViewItem.ForeColor = Color.Blue;
                            else
                                trackedAvatar.ListViewItem.ForeColor = Color.Magenta;
                        });
                        
                    }
                }
            }
        }

        private void Avatars_OnAvatarNames(Dictionary<UUID, string> names)
        {
            lock (_UntrackedAvatars)
            {
                foreach (KeyValuePair<UUID, string> name in names)
                {
                    TrackedAvatar trackedAvatar;
                    if (_UntrackedAvatars.TryGetValue(name.Key, out trackedAvatar))
                    {
                        trackedAvatar.Name = name.Value;

                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            trackedAvatar.ListViewItem.Text = name.Value;
                        });
                    }
                }
            }
        }

        private void Grid_OnCoarseLocationUpdate(Simulator sim, List<UUID> newEntries, List<UUID> removedEntries)
        {
            UpdateCoarseInfo(sim, newEntries, removedEntries);
        }

        private void Network_OnCurrentSimChanged(Simulator PreviousSimulator)
        {
            lock (_TrackedAvatars)
                _TrackedAvatars.Clear();

            lock (_UntrackedAvatars)
                _UntrackedAvatars.Clear();

            ClearItems();
        }

        private void Objects_OnNewAvatar(Simulator simulator, Avatar avatar, ulong regionHandle, ushort timeDilation)
        {
            UpdateAvatar(avatar);
        }

        private void Objects_OnObjectKilled(Simulator simulator, uint objectID)
        {
            lock (_TrackedAvatars)
            {
                if (_TrackedAvatars.ContainsKey(objectID)) RemoveAvatar(objectID);
            }
        }

        private void Objects_OnObjectUpdated(Simulator simulator, ObjectUpdate update, ulong regionHandle, ushort timeDilation)
        {
            lock (_TrackedAvatars)
            {
                if (_TrackedAvatars.ContainsKey(update.LocalID))
                {
                    Avatar av;
                    if (simulator.ObjectsAvatars.TryGetValue(update.LocalID, out av))
                        UpdateAvatar(av);
                }
            }
        }

        private class ColumnSorter : IComparer
        {
            public bool Ascending = true;
            public int SortColumn = 0;

            public int Compare(object a, object b)
            {
                ListViewItem itemA = (ListViewItem)a;
                ListViewItem itemB = (ListViewItem)b;

                if (SortColumn == 1)
                {
                    int valueA = itemB.SubItems.Count > 1 ? int.Parse(itemA.SubItems[1].Text.Replace("m", "").Replace("--", "0")) : 0;
                    int valueB = itemB.SubItems.Count > 1 ? int.Parse(itemB.SubItems[1].Text.Replace("m", "").Replace("--", "0")) : 0;
                    if (Ascending)
                    {
                        if (valueA == valueB) return 0;
                        return valueA < valueB ? -1 : 1;
                    }
                    else
                    {
                        if (valueA == valueB) return 0;
                        return valueA < valueB ? 1 : -1;
                    }
                }
                else
                {
                    if (Ascending) return string.Compare(itemA.Text, itemB.Text);
                    else return -string.Compare(itemA.Text, itemB.Text);
                }
            }
        }

    }

}
