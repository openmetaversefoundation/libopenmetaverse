/*
 * Copyright (c) 2007-2009, openmetaverse.org
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
    /// ListView GUI component for viewing a client's nearby avatars list
    /// </summary>
    public class AvatarList : ListView
    {
        private GridClient _Client;
        private ListColumnSorter _ColumnSorter = new ListColumnSorter();
        private TrackedAvatar _SelectedAvatar;

        private DoubleDictionary<uint, UUID, TrackedAvatar> _TrackedAvatars = new DoubleDictionary<uint, UUID, TrackedAvatar>();
        private Dictionary<UUID, TrackedAvatar> _UntrackedAvatars = new Dictionary<UUID, TrackedAvatar>();

        public delegate void AvatarCallback(TrackedAvatar trackedAvatar);

        /// <summary>
        /// Triggered when the user double clicks on an avatar in the list
        /// </summary>
        public event AvatarCallback OnAvatarDoubleClick;

        /// <summary>
        /// Triggered when a new avatar is added to the list
        /// </summary>
        public event AvatarCallback OnAvatarAdded;

        /// <summary>
        /// Triggered when an avatar is removed from the list
        /// </summary>
        public event AvatarCallback OnAvatarRemoved;

        /// <summary>
        /// Gets or sets the GridClient associated with this control
        /// </summary>
        public GridClient Client
        {
            get { return _Client; }
            set { if (value != null) InitializeClient(value); }
        }

        /// <summary>
        /// Returns the current selected avatar in the tracked avatars list
        /// </summary>
        public TrackedAvatar SelectedAvatar
        {
            get { return _SelectedAvatar; }
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

            this.MultiSelect = false;
            this.SelectedIndexChanged += new EventHandler(AvatarList_SelectedIndexChanged);

            _ColumnSorter.SortColumn = 1;
            this.Sorting = SortOrder.Ascending;
            this.ListViewItemSorter = _ColumnSorter;

            EventHandler clickHandler = new EventHandler(defaultMenuItem_Click);
            this.ContextMenu = new ContextMenu();
            this.ContextMenu.MenuItems.Add("Offer Teleport", clickHandler);
            this.ContextMenu.MenuItems.Add("Teleport To", clickHandler);
            this.ContextMenu.MenuItems.Add("Walk To", clickHandler);

            this.DoubleBuffered = true;
            this.ListViewItemSorter = _ColumnSorter;
            this.View = View.Details;
            this.ColumnClick += new ColumnClickEventHandler(AvatarList_ColumnClick);
            this.DoubleClick += new EventHandler(AvatarList_DoubleClick);
        }

        void AvatarList_SelectedIndexChanged(object sender, EventArgs e)
        {
            lock (_TrackedAvatars)
            {
                lock (_UntrackedAvatars)
                {
                    if (this.SelectedItems.Count > 0)
                    {
                        UUID selectedID = new UUID(this.SelectedItems[0].Name);
                        TrackedAvatar selectedAV;
                        if (!_TrackedAvatars.TryGetValue(selectedID, out selectedAV) && !_UntrackedAvatars.TryGetValue(selectedID, out selectedAV))
                            selectedAV = null;

                        _SelectedAvatar = selectedAV;
                    }
                }
            }
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

        public TrackedAvatar GetAvatar(UUID avatarID)
        {
            TrackedAvatar av;
            _TrackedAvatars.TryGetValue(avatarID, out av);
            return av;
        }

        private void InitializeClient(GridClient client)
        {
            _Client = client;
            _Client.Avatars.OnAvatarAppearance += new AvatarManager.AvatarAppearanceCallback(Avatars_OnAvatarAppearance);
            _Client.Avatars.OnAvatarNames += new AvatarManager.AvatarNamesCallback(Avatars_OnAvatarNames);
            _Client.Grid.OnCoarseLocationUpdate += new GridManager.CoarseLocationUpdateCallback(Grid_OnCoarseLocationUpdate);
            _Client.Network.OnCurrentSimChanged += new NetworkManager.CurrentSimChangedCallback(Network_OnCurrentSimChanged);
            _Client.Objects.OnNewAvatar += new ObjectManager.NewAvatarCallback(Objects_OnNewAvatar);
            _Client.Objects.OnObjectUpdated += new ObjectManager.ObjectUpdatedCallback(Objects_OnObjectUpdated);
        }

        private void AddAvatar(uint localID, UUID avatarID, Vector3 coarsePosition, Avatar avatar)
        {
            if (!this.IsHandleCreated) return;

            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { AddAvatar(localID, avatarID, coarsePosition, avatar); });
            else
            {
                TrackedAvatar trackedAvatar = new TrackedAvatar();
                trackedAvatar.CoarseLocation = coarsePosition;
                trackedAvatar.ID = avatarID;
                trackedAvatar.ListViewItem = this.Items.Add(avatarID.ToString(), trackedAvatar.Name, null);
                trackedAvatar.ListViewItem.Name = avatarID.ToString();

                string strDist = avatarID == _Client.Self.AgentID ? "--" : (int)Vector3.Distance(_Client.Self.SimPosition, coarsePosition) + "m";
                trackedAvatar.ListViewItem.SubItems.Add(strDist);

                if (avatar != null)
                {
                    trackedAvatar.Name = avatar.Name;
                    trackedAvatar.ListViewItem.Text = avatar.Name;

                    lock (_TrackedAvatars)
                        _TrackedAvatars.Add(localID, avatarID, trackedAvatar);

                    if (OnAvatarAdded != null)
                    {
                        try { OnAvatarAdded(trackedAvatar); }
                        catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }
                    }
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
            if (!this.IsHandleCreated) return;

            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { RemoveAvatar(localID); });
            else
            {
                TrackedAvatar trackedAvatar;

                lock (_TrackedAvatars)
                {
                    if (_TrackedAvatars.TryGetValue(localID, out trackedAvatar))
                    {
                        this.Items.Remove(trackedAvatar.ListViewItem);
                        _TrackedAvatars.Remove(localID);
                    }
                }

                if (OnAvatarRemoved != null)
                {
                    try { OnAvatarRemoved(trackedAvatar); }
                    catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }
                }
            }
        }

        private void UpdateAvatar(Avatar avatar)
        {
            if (!this.IsHandleCreated) return;

            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { UpdateAvatar(avatar); });
            else
            {
                TrackedAvatar trackedAvatar;

                lock (_TrackedAvatars)
                {
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
                        lock (_TrackedAvatars)
                        {
                            if (_TrackedAvatars.TryGetValue(removedEntries[i], out trackedAvatar)) RemoveAvatar(trackedAvatar.Avatar.LocalID);
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

        private void defaultMenuItem_Click(object sender, EventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;

            switch (menuItem.Text)
            {
                case "Offer Teleport":
                    {
                        Client.Self.SendTeleportLure(_SelectedAvatar.ID);
                        break;
                    }
                case "Teleport To":
                    {
                        Vector3 pos;
                        if (Client.Network.CurrentSim.AvatarPositions.TryGetValue(_SelectedAvatar.ID, out pos))
                            Client.Self.Teleport(Client.Network.CurrentSim.Name, pos);

                        break;
                    }
                case "Walk To":
                    {
                        Vector3 pos;
                        if (Client.Network.CurrentSim.AvatarPositions.TryGetValue(_SelectedAvatar.ID, out pos))
                            Client.Self.AutoPilotLocal((int)pos.X, (int)pos.Y, pos.Z);

                        break;
                    }
            }
        }

        void AvatarList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            _ColumnSorter.SortColumn = e.Column;
            if ((_ColumnSorter.Ascending = (this.Sorting == SortOrder.Ascending))) this.Sorting = SortOrder.Descending;
            else this.Sorting = SortOrder.Ascending;
            this.ListViewItemSorter = _ColumnSorter;
        }

        void AvatarList_DoubleClick(object sender, EventArgs e)
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

        void Avatars_OnAvatarAppearance(UUID avatarID, bool isTrial, Primitive.TextureEntryFace defaultTexture, Primitive.TextureEntryFace[] faceTextures, List<byte> visualParams)
        {
            if (visualParams.Count > 31)
            {
                lock (_TrackedAvatars)
                {
                    TrackedAvatar trackedAvatar;
                    if (_TrackedAvatars.TryGetValue(avatarID, out trackedAvatar))
                    {                        
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            byte param = visualParams[31];
                            if (param > 0)
                                trackedAvatar.ListViewItem.ForeColor = Color.Blue;
                            else
                                trackedAvatar.ListViewItem.ForeColor = Color.Magenta;
                        });
                        
                    }
                }
            }
        }

        void Avatars_OnAvatarNames(Dictionary<UUID, string> names)
        {
            lock (_UntrackedAvatars)
            {
                foreach (KeyValuePair<UUID, string> name in names)
                {
                    TrackedAvatar trackedAvatar;
                    if (_UntrackedAvatars.TryGetValue(name.Key, out trackedAvatar))
                    {
                        trackedAvatar.Name = name.Value;

                        if (OnAvatarAdded != null && trackedAvatar.ListViewItem.Text == "(Loading...)")
                        {
                            try { OnAvatarAdded(trackedAvatar); }
                            catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }
                        }

                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            trackedAvatar.ListViewItem.Text = name.Value;
                        });
                    }
                }
            }
        }

        void Grid_OnCoarseLocationUpdate(Simulator sim, List<UUID> newEntries, List<UUID> removedEntries)
        {
            UpdateCoarseInfo(sim, newEntries, removedEntries);
        }

        void Network_OnCurrentSimChanged(Simulator PreviousSimulator)
        {
            lock (_TrackedAvatars)
                _TrackedAvatars.Clear();

            lock (_UntrackedAvatars)
                _UntrackedAvatars.Clear();

            ClearItems();
        }

        void Objects_OnNewAvatar(Simulator simulator, Avatar avatar, ulong regionHandle, ushort timeDilation)
        {
            UpdateAvatar(avatar);
        }

        void Objects_OnObjectUpdated(Simulator simulator, ObjectUpdate update, ulong regionHandle, ushort timeDilation)
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

    }

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

}
