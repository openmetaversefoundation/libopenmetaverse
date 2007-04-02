using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using libsecondlife;
using libsecondlife.Packets;

namespace groupmanager
{
    public partial class frmGroupManager : Form
    {
        SecondLife Client;
        Dictionary<LLUUID, Group> Groups;

        public frmGroupManager()
        {
            Client = new SecondLife();

            // Throttle unnecessary things down
            Client.Throttle.Land = 0;
            Client.Throttle.Wind = 0;
            Client.Throttle.Cloud = 0;

            Client.Groups.OnCurrentGroups += new GroupManager.CurrentGroupsCallback(GroupsUpdatedHandler);
            
            InitializeComponent();
        }

        void GroupsUpdatedHandler(Dictionary<LLUUID, Group> groups)
        {
            Groups = groups;

            Invoke(new MethodInvoker(UpdateGroups));
        }

        void UpdateGroups()
        {
            lock (lstGroups)
            {
                lstGroups.Items.Clear();

                foreach (Group group in Groups.Values)
                {
                    lstGroups.Items.Add(group);
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

        private void cmdConnect_Click(object sender, EventArgs e)
        {
            if (cmdConnect.Text == "Connect")
            {
                cmdConnect.Text = "Disconnect";
                txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = false;

                if (Client.Network.Login(txtFirstName.Text, txtLastName.Text, txtPassword.Text, "GroupManager",
                    "jhurliman@wsu.edu"))
                {
                    groupBox.Enabled = true;

                    Client.Groups.BeginGetCurrentGroups();
                }
                else
                {
                    MessageBox.Show(this, "Error logging in: " + Client.Network.LoginMessage);
                    cmdConnect.Text = "Connect";
                    txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = true;
                    groupBox.Enabled = false;
                    lstGroups.Items.Clear();
                }
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
    }
}
