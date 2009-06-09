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
        bool ShuttingDown = false;

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
            loginPanel1.LoginParams = ClientLogin;

            ClientLogin.Start = "last";
            
            if (firstName != String.Empty && lastName != String.Empty && password != String.Empty)
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
            Settings.USE_TEXTURE_CACHE = true;

            Client.Network.OnCurrentSimChanged += new NetworkManager.CurrentSimChangedCallback(Network_OnCurrentSimChanged);
            Client.Network.OnDisconnected += new NetworkManager.DisconnectedCallback(Network_OnDisconnected);
            Client.Self.OnInstantMessage += new AgentManager.InstantMessageCallback(Self_OnInstantMessage);

            //define the client object for each GUI element
            avatarList1.Client = Client;
            friendsList1.Client = Client;
            groupList1.Client = Client;
            inventoryTree1.Client = Client;
            localChat1.Client = Client;
            loginPanel1.Client = Client;
            messageBar1.Client = Client;
            miniMap1.Client = Client;
            statusOutput1.Client = Client;
        }

        void Dashboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            ShuttingDown = true;
            InitializeClient(false);
            Environment.Exit(0);
        }

        void avatarList1_OnAvatarDoubleClick(TrackedAvatar trackedAvatar)
        {
            messageBar1.CreateSession(trackedAvatar.Name, trackedAvatar.ID, trackedAvatar.ID, true);
        }

        void friendsList1_OnFriendDoubleClick(FriendInfo friend)
        {
            messageBar1.CreateSession(friend.Name, friend.UUID, friend.UUID, true);
        }

        void groupList1_OnGroupDoubleClick(Group group)
        {
            MessageBox.Show(group.Name + " = " + group.ID);
        }

        void Network_OnCurrentSimChanged(Simulator PreviousSimulator)
        {
            Client.Appearance.SetPreviousAppearance(false);
        }

        void Network_OnDisconnected(NetworkManager.DisconnectType reason, string message)
        {
            InitializeClient(!ShuttingDown);
        }

        void Self_OnInstantMessage(InstantMessage im, Simulator simulator)
        {
            if (im.Dialog == InstantMessageDialog.RequestTeleport)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    DialogResult result = MessageBox.Show(this, im.FromAgentName + " has offered you a teleport request:" + Environment.NewLine + im.Message, this.Text, MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                        Client.Self.TeleportLureRespond(im.FromAgentID, true);
                });
            }
        }

    }
}