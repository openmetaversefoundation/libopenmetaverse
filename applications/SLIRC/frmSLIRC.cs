using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Threading;
using libsecondlife;
using Meebey.SmartIrc4net;
namespace SLIRC
{
    public partial class frmSLIRC : Form
    {
        private SecondLife client;
        private IrcClient ircclient;
        private Thread listenthread;
        public frmSLIRC()
        {
            InitializeComponent();
            ircclient = new IrcClient();
        }
        public void Listen()
        {
            ircclient.Listen();
        }
        private void cmdConnect_Click(object sender, EventArgs e)
        {
            if (cmdConnect.Text == "Connect")
            {
                cmdConnect.Text = "Disconnect";
                txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = false;

                Hashtable loginParams = NetworkManager.DefaultLoginValues(txtFirstName.Text,
                    txtLastName.Text, txtPassword.Text, "00:00:00:00:00:00", "last", 1, 50, 50, 50,
                    "Win", "0", "accountant", "jhurliman@wsu.edu");
                if (client.Network.Login(loginParams))
                {
                    LogMessage("Logged into Second Life");
                    lstAllowedUsers.Enabled = lstLog.Enabled = btnJoin.Enabled = txtMessage.Enabled = btnSay.Enabled = true;
                    ircclient.OnChannelMessage += new IrcEventHandler(ircclient_OnChannelMessage);
                    //Connect to IRC server, yaydey yadah
                    try
                    {
                        ircclient.Connect(new string[] { txtServerName.Text }, int.Parse(txtPort.Text));
                        LogMessage("Connected to IRC Server.");
                        ircclient.Login(client.Avatar.FirstName + client.Avatar.LastName, "SLIRC Gateway");
                        ircclient.RfcJoin(txtChannel.Text);
                        LogMessage("Logged in");
                        if(listenthread != null) listenthread.Abort();
                        listenthread = new Thread(Listen);
                        listenthread.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("OH NOES! " + ex.Message);
                       
                    }

                }
                else
                {
                    MessageBox.Show(this, "Error logging in: " + client.Network.LoginError);
                    //if(listenthread) listenthread.Abort();
                    cmdConnect.Text = "Connect";
                    lstAllowedUsers.Enabled = lstLog.Enabled = btnJoin.Enabled = btnSay.Enabled = false;
                    txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = true;
                }
            }
            else
            {
                cmdConnect.Text = "Connect";
                lstAllowedUsers.Enabled = btnJoin.Enabled = txtMessage.Enabled = btnSay.Enabled = false;
                txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = true;
                ircclient.RfcQuit("SLIRC Disconnect.");
                listenthread.Abort();
                client.Network.Logout();
            }

        }

        void ircclient_OnChannelMessage(object sender, IrcEventArgs e)
        {
            LogMessage(e.Data.Nick + ": " + e.Data.Message);
            //From IRC -> Inject to SL
            client.Avatar.Say(e.Data.Nick + ": " + e.Data.Message, 0);
        }

        private void frmSLIRC_Load(object sender, EventArgs e)
        {
            try
            {
                client = new SecondLife("keywords.txt", "protocol.txt");
                client.Avatar.OnChat += new ChatCallback(Avatar_OnChat);
                grpLogin.Enabled = true;
            }
            catch (Exception error)
            {
                MessageBox.Show(this, error.ToString());
            }
        }
        private delegate void SingleStringDelegate(string s);
        void LogMessage(string msg)
        {
            if (!this.InvokeRequired)
            {
                int i = lstLog.Items.Add(msg);
                lstLog.SelectedIndex = i;
            }
            else
            {
                Invoke(new SingleStringDelegate(LogMessage), new object[] { msg });
            }
        }
        void AddToAllowedList(string name)
        {
            if (!this.InvokeRequired)
            {
                lstAllowedUsers.Items.Add(name);
            }
            else
            {
                Invoke(new SingleStringDelegate(AddToAllowedList), new object[] { name });
            }
        }
        void Avatar_OnChat(string message, byte audible, byte type, byte sourcetype, string name, LLUUID id, byte command, LLUUID commandID)
        {
            if (message.Equals("addme"))
            {
                //Add to the list
                LogMessage("Adding " + name + " to the allowed list");
                AddToAllowedList(name);
            }
            else
            {
                if (lstAllowedUsers.Items.Contains(name) && audible == 1 && !message.Equals(""))
                {
                    LogMessage(name + ": " + message);
                    ircclient.SendMessage(SendType.Message, txtChannel.Text, name + " : " + message);
                }
            }
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            ircclient.RfcJoin(txtChannel.Text);
            LogMessage("Joining " + txtChannel.Text);
        }

        private void btnGetPos_Click(object sender, EventArgs e)
        {
            LogMessage("Position: " + client.Avatar.Position.X.ToString() + " " + client.Avatar.Position.Y.ToString());
        }

        private void btnSay_Click(object sender, EventArgs e)
        {
            client.Avatar.Say(ircclient.Nickname + ": " + txtMessage.Text, 0);
            ircclient.SendMessage(SendType.Message, txtChannel.Text, txtMessage.Text);
        }

    }
}