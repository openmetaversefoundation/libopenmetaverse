using OpenMetaverse;
using OpenMetaverse.GUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Dashboard
{
    public partial class Dashboard : Form
    {

        GridClient Client;

        /// <summary>
        /// Provides a full representation of OpenMetaverse.GUI
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="password"></param>
        public Dashboard(string firstName, string lastName, string password)
        {
            InitializeComponent();

            //force logout and exit when form is closed
            this.FormClosing += new FormClosingEventHandler(Dashboard_FormClosing);

            //initialize client object
            Client = new GridClient();
            Client.Settings.USE_TEXTURE_CACHE = true;
            LoginParams ClientLogin = Client.Network.DefaultLoginParams(firstName, lastName, password, "OpenMetaverse Dashboard", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            ClientLogin.Start = "last";

            //define the client object for each GUI element
            avatarList1.Client = Client;
            friendsList1.Client = Client;
            groupList1.Client = Client;
            inventoryTree1.Client = Client;
            localChat1.Client = Client;
            miniMap1.Client = Client;
            statusOutput1.Client = Client;

            //double-click events
            avatarList1.OnAvatarDoubleClick += new AvatarList.AvatarDoubleClickCallback(avatarList1_OnAvatarDoubleClick);
            friendsList1.OnFriendDoubleClick += new FriendList.FriendDoubleClickCallback(friendsList1_OnFriendDoubleClick);
            groupList1.OnGroupDoubleClick += new GroupList.GroupDoubleClickCallback(groupList1_OnGroupDoubleClick);

            //login
            Client.Network.BeginLogin(ClientLogin);
        }

        private void Dashboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            if (Client != null && Client.Network.Connected) Client.Network.Logout();
            Environment.Exit(0);
        }

        private void avatarList1_OnAvatarDoubleClick(Avatar avatar)
        {
            MessageBox.Show(avatar.Name + " = " + avatar.ID);
        }

        private void friendsList1_OnFriendDoubleClick(FriendInfo friend)
        {
            MessageBox.Show(friend.Name + " = " + friend.UUID);
        }

        private void groupList1_OnGroupDoubleClick(Group group)
        {
            MessageBox.Show(group.Name + " = " + group.ID);
        }

    }
}