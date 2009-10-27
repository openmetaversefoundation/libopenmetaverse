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
            _Client.Avatars.AvatarAppearance += Avatars_OnAvatarAppearance;
            _Client.Avatars.UUIDNameReply += new EventHandler<UUIDNameReplyEventArgs>(Avatars_UUIDNameReply);
            _Client.Grid.CoarseLocationUpdate += Grid_CoarseLocationUpdate;
            _Client.Network.OnCurrentSimChanged += new NetworkManager.CurrentSimChangedCallback(Network_OnCurrentSimChanged);
            _Client.Objects.AvatarUpdate += Objects_OnNewAvatar;
            _Client.Objects.TerseObjectUpdate += Objects_OnObjectUpdated;
            
        }

        void Avatars_UUIDNameReply(object sender, UUIDNameReplyEventArgs e)
        {
            lock (_UntrackedAvatars)
            {
                foreach (KeyValuePair<UUID, string> name in e.Names)
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

        void Grid_CoarseLocationUpdate(object sender, CoarseLocationUpdateEventArgs e)
        {
            UpdateCoarseInfo(e.Simulator, e.NewEntries, e.RemovedEntries);
        }

        private void AddAvatar(UUID avatarID, Avatar avatar, Vector3 coarsePosition)
        {
            if (!this.IsHandleCreated) return;

            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { AddAvatar(avatar.ID, avatar, coarsePosition); });
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
                    {
                        if (_TrackedAvatars.ContainsKey(avatarID))
                            _TrackedAvatars.Remove(avatarID);

                        _TrackedAvatars.Add(avatar.LocalID, avatarID, trackedAvatar);
                    }

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

        private void RemoveAvatar(UUID id)
        {
            if (!this.IsHandleCreated) return;

            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { RemoveAvatar(id); });
            else
            {
                TrackedAvatar trackedAvatar;

                lock (_TrackedAvatars)
                {
                    if (_TrackedAvatars.TryGetValue(id, out trackedAvatar))
                    {
                        this.Items.Remove(trackedAvatar.ListViewItem);
                        _TrackedAvatars.Remove(id);
                    }
                }

                lock (_UntrackedAvatars)
                {
                    if (_UntrackedAvatars.TryGetValue(id, out trackedAvatar))
                    {
                        this.Items.Remove(trackedAvatar.ListViewItem);
                        _UntrackedAvatars.Remove(trackedAvatar.ID);
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
                        AddAvatar(avatar.ID, avatar, Vector3.Zero);
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
                if (sim == null) return;

                if (removedEntries != null)
                {
                    for (int i = 0; i < removedEntries.Count; i++)
                        RemoveAvatar(removedEntries[i]);
                }

                if (newEntries != null)
                {
                    for (int i = 0; i < newEntries.Count; i++)
                    {
                        int index = this.Items.IndexOfKey(newEntries[i].ToString());
                        if (index == -1)
                        {
                            Vector3 coarsePos;
                            if (!sim.AvatarPositions.TryGetValue(newEntries[i], out coarsePos))
                                continue;

                            AddAvatar(newEntries[i], null, coarsePos);
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

        void Avatars_OnAvatarAppearance(object sender, AvatarAppearanceEventArgs e)
        {
            if (e.VisualParams.Count > 31)
            {
                lock (_TrackedAvatars)
                {
                    TrackedAvatar trackedAvatar;
                    if (_TrackedAvatars.TryGetValue(e.AvatarID, out trackedAvatar))
                    {                        
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            byte param = e.VisualParams[31];
                            if (param > 0)
                                trackedAvatar.ListViewItem.ForeColor = Color.Blue;
                            else
                                trackedAvatar.ListViewItem.ForeColor = Color.Magenta;
                        });                        
                    }
                }
            }
        }

        void Network_OnCurrentSimChanged(Simulator PreviousSimulator)
        {
            lock (_TrackedAvatars)
                _TrackedAvatars.Clear();

            lock (_UntrackedAvatars)
                _UntrackedAvatars.Clear();

            ClearItems();
        }

        void Objects_OnNewAvatar(object sender, AvatarUpdateEventArgs e)
        {
            UpdateAvatar(e.Avatar);
        }

        void Objects_OnObjectUpdated(object sender, TerseObjectUpdateEventArgs e)
        {
            lock (_TrackedAvatars)
            {
                if (_TrackedAvatars.ContainsKey(e.Update.LocalID))
                {
                    Avatar av;
                    if (e.Simulator.ObjectsAvatars.TryGetValue(e.Update.LocalID, out av))
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
