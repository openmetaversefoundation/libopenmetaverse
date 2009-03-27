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
        LoginParams ClientLogin;

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

            //initialize the client object and related controls
            InitializeClient(true);

            //double-click events
            avatarList1.OnAvatarDoubleClick += new AvatarList.AvatarDoubleClickCallback(avatarList1_OnAvatarDoubleClick);
            friendsList1.OnFriendDoubleClick += new FriendList.FriendDoubleClickCallback(friendsList1_OnFriendDoubleClick);
            groupList1.OnGroupDoubleClick += new GroupList.GroupDoubleClickCallback(groupList1_OnGroupDoubleClick);

            //login
            ClientLogin = Client.Network.DefaultLoginParams(firstName, lastName, password, "OpenMetaverse Dashboard", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            ClientLogin.Start = "last";
            Client.Network.BeginLogin(ClientLogin);
        }

        private void InitializeClient(bool initialize)
        {
            if (Client != null)
            {
                if (Client.Network.Connected)
                    Client.Network.Logout();

                Client = null;
            }

            if (!initialize) return;

            //initialize client object
            Client = new GridClient();
            Client.Network.OnLogin += new NetworkManager.LoginCallback(Network_OnLogin);
            Client.Settings.USE_TEXTURE_CACHE = true;

            //define the client object for each GUI element
            avatarList1.Client = Client;
            friendsList1.Client = Client;
            groupList1.Client = Client;
            inventoryTree1.Client = Client;
            localChat1.Client = Client;
            miniMap1.Client = Client;
            statusOutput1.Client = Client;
        }

        private void Dashboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            InitializeClient(false);
            Environment.Exit(0);
        }

        private void avatarList1_OnAvatarDoubleClick(TrackedAvatar trackedAvatar)
        {
            MessageBox.Show(trackedAvatar.Name + " = " + trackedAvatar.ID);
        }

        private void friendsList1_OnFriendDoubleClick(FriendInfo friend)
        {
            MessageBox.Show(friend.Name + " = " + friend.UUID);
        }

        private void groupList1_OnGroupDoubleClick(Group group)
        {
            MessageBox.Show(group.Name + " = " + group.ID);
        }

        private void Network_OnLogin(LoginStatus login, string message)
        {
            if (login == LoginStatus.Failed)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    if (MessageBox.Show(this, "Login failed. Try again?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    {
                        InitializeClient(true);
                        Client.Network.BeginLogin(ClientLogin);
                    }
                });
            }
        }

    }
}