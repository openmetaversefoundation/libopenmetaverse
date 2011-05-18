using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace groupmanager
{
    public partial class frmGroupManager : Form
    {
        GridClient Client;
        Dictionary<UUID, Group> Groups;

        public frmGroupManager()
        {
            Client = new GridClient();

            Client.Settings.MULTIPLE_SIMS = false;

            // Throttle unnecessary things down
            Client.Throttle.Land = 0;
            Client.Throttle.Wind = 0;
            Client.Throttle.Cloud = 0;

            Client.Network.LoginProgress += Network_OnLogin;
            Client.Network.EventQueueRunning += Network_OnEventQueueRunning;
            Client.Groups.CurrentGroups += Groups_CurrentGroups;
            
            InitializeComponent();
        }

        void Groups_CurrentGroups(object sender, CurrentGroupsEventArgs e)
        {
            Groups = e.Groups;

            Invoke(new MethodInvoker(UpdateGroups));
        }

        private void UpdateGroups()
        {
            lock (lstGroups)
            {
                Invoke((MethodInvoker)delegate() { lstGroups.Items.Clear(); });

                foreach (Group group in Groups.Values)
                {
                    Logger.Log(String.Format("Adding group {0} ({1})", group.Name, group.ID), Helpers.LogLevel.Info, Client);

                    Invoke((MethodInvoker)delegate() { lstGroups.Items.Add(group); });
                }
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            frmGroupManager frm = new frmGroupManager();
            frm.ShowDialog();
        }

        #region GUI Callbacks

        private void cmdConnect_Click(object sender, EventArgs e)
        {
            if (cmdConnect.Text == "Connect")
            {
                cmdConnect.Text = "Disconnect";
                txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = false;

                LoginParams loginParams = Client.Network.DefaultLoginParams(txtFirstName.Text, txtLastName.Text,
                    txtPassword.Text, "GroupManager", "1.0.0");
                Client.Network.BeginLogin(loginParams);
            }
			else
			{
				Client.Network.Logout();
				cmdConnect.Text = "Connect";
				txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = true;
                groupBox.Enabled = false;
                lstGroups.Items.Clear();
			}
        }

        private void lstGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstGroups.SelectedIndex >= 0)
            {
                cmdActivate.Enabled = cmdInfo.Enabled = cmdLeave.Enabled = true;
            }
            else
            {
                cmdActivate.Enabled = cmdInfo.Enabled = cmdLeave.Enabled = false;
            }
        }

        private void cmdInfo_Click(object sender, EventArgs e)
        {
            if (lstGroups.SelectedIndex >= 0 && lstGroups.Items[lstGroups.SelectedIndex].ToString() != "none")
            {
                Group group = (Group)lstGroups.Items[lstGroups.SelectedIndex];

                frmGroupInfo frm = new frmGroupInfo(group, Client);
                frm.ShowDialog();
            }
        }

        #endregion GUI Callbacks

        #region Network Callbacks

        private void Network_OnLogin(object sender, LoginProgressEventArgs e)
        {
            if (e.Status == LoginStatus.Success)
            {
                BeginInvoke(
                    (MethodInvoker)delegate()
                    {
                        groupBox.Enabled = true;
                    });
            }
            else if (e.Status == LoginStatus.Failed)
            {
                BeginInvoke(
                    (MethodInvoker)delegate()
                    {
                        MessageBox.Show(this, "Error logging in: " + Client.Network.LoginMessage);
                        cmdConnect.Text = "Connect";
                        txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = true;
                        groupBox.Enabled = false;
                        lstGroups.Items.Clear();
                    });
            }
        }

        private void Network_OnEventQueueRunning(object sender, EventQueueRunningEventArgs e)
        {
            if (e.Simulator == Client.Network.CurrentSim)
            {
                Console.WriteLine("Event queue connected for the primary simulator, requesting group info");

                Client.Groups.RequestCurrentGroups();
            }
        }

        #endregion
    }
}
