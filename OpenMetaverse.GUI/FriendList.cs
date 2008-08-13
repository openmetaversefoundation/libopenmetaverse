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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace OpenMetaverse.GUI
{

    /// <summary>
    /// ListView GUI component for viewing a client's friend list
    /// </summary>
    public class FriendList : ListView
    {
        private GridClient _Client;
        private ColumnSorter _ColumnSorter = new ColumnSorter();

        public delegate void FriendDoubleClickCallback(FriendInfo friend);

        /// <summary>
        /// Triggered when the user double clicks on a friend in the list
        /// </summary>
        public event FriendDoubleClickCallback OnFriendDoubleClick;        

        /// <summary>
        /// Gets or sets the GridClient associated with this control
        /// </summary>
        public GridClient Client
        {
            get { return _Client; }
            set { if (value != null) InitializeClient(value); }
        }

        /// <summary>
        /// TreeView control for an unspecified client's friend list
        /// </summary>
        public FriendList()
        {
            ColumnHeader header1 = this.Columns.Add("Friend");
            header1.Width = this.Width - 20;

            ColumnHeader header2 = this.Columns.Add(" ");
            header2.Width = 40;

            _ColumnSorter.SortColumn = 1;
            _ColumnSorter.Ascending = false;

            this.DoubleBuffered = true;
            this.ListViewItemSorter = _ColumnSorter;
            this.View = View.Details;

            this.ColumnClick += new ColumnClickEventHandler(FriendList_ColumnClick);
            this.DoubleClick += new System.EventHandler(FriendList_DoubleClick);
        }

        /// <summary>
        /// TreeView control for the specified client's friend list
        /// </summary>
        public FriendList(GridClient client) : this ()
        {
            InitializeClient(client);
        }

        private void InitializeClient(GridClient client)
        {
            _Client = client;
            _Client.Friends.OnFriendNamesReceived += new FriendsManager.FriendNamesReceived(Friends_OnFriendNamesReceived);
            _Client.Friends.OnFriendOffline += new FriendsManager.FriendOfflineEvent(Friends_OnFriendOffline);
            _Client.Friends.OnFriendOnline += new FriendsManager.FriendOnlineEvent(Friends_OnFriendOnline);
            _Client.Network.OnLogin += new NetworkManager.LoginCallback(Network_OnLogin);
        }

        private void RefreshFriends()
        {
            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { RefreshFriends(); });
            else
            {
                Client.Friends.FriendList.ForEach(delegate(FriendInfo friend)
                {
                    string key = friend.UUID.ToString();
                    string onlineText;
                    string name = friend.Name == null ? "(loading...)" : friend.Name;
                    int image;
                    Color color;

                    if (friend.IsOnline)
                    {
                        image = 1;
                        onlineText = "*";
                        color = Color.FromKnownColor(KnownColor.ControlText);
                    }
                    else
                    {
                        image = 0;
                        onlineText = " ";
                        color = Color.FromKnownColor(KnownColor.GrayText);
                    }

                    if (!this.Items.ContainsKey(key))
                    {
                        this.Items.Add(key, name, image);
                        this.Items[key].SubItems.Add(onlineText);
                    }
                    else
                    {
                        if (this.Items[key].Text == string.Empty || friend.Name != null)
                            this.Items[key].Text = name;

                        this.Items[key].SubItems[1].Text = onlineText;
                    }

                    this.Items[key].ForeColor = color;
                    this.Items[key].ImageIndex = image;
                    this.Items[key].Tag = friend;
                });
            }
        }

        private void Friends_OnFriendOffline(FriendInfo friend)
        {
            RefreshFriends();
        }

        private void Friends_OnFriendOnline(FriendInfo friend)
        {
            RefreshFriends();
        }

        private void Friends_OnFriendNamesReceived(Dictionary<UUID, string> names)
        {
            RefreshFriends();
        }

        private void FriendList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            _ColumnSorter.SortColumn = e.Column;
            if ((_ColumnSorter.Ascending = (this.Sorting == SortOrder.Ascending))) this.Sorting = SortOrder.Descending;
            else this.Sorting = SortOrder.Ascending;
            this.ListViewItemSorter = _ColumnSorter;
        }

        private void FriendList_DoubleClick(object sender, System.EventArgs e)
        {
            if (OnFriendDoubleClick != null)
            {
                ListView list = (ListView)sender;
                if (list.SelectedItems.Count > 0 && list.SelectedItems[0].Tag is FriendInfo)
                {
                    FriendInfo friend = (FriendInfo)list.SelectedItems[0].Tag;
                    try { OnFriendDoubleClick(friend); }
                    catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }
                }
            }
        }

        private void Network_OnLogin(LoginStatus login, string message)
        {
            if (login == LoginStatus.Success) RefreshFriends();
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
                    string onlineA = itemA.SubItems.Count < 2 ? string.Empty : itemA.SubItems[1].Text;
                    string onlineB = itemB.SubItems.Count < 2 ? string.Empty : itemB.SubItems[1].Text;
                    if (Ascending) return string.Compare(onlineA + itemA.Text, onlineB + itemB.Text);
                    else return -string.Compare(onlineA + itemA.Text, onlineB + itemB.Text);
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
