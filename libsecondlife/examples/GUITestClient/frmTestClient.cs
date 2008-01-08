using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using libsecondlife;

namespace libsecondlife.GUITestClient
{
    public partial class frmTestClient : Form
    {
        private SecondLife Client = new SecondLife();
        private Dictionary<Interface, TabPage> Interfaces = new Dictionary<Interface, TabPage>();

        public frmTestClient()
        {
            Client.Network.OnLogin += new NetworkManager.LoginCallback(Network_OnLogin);
            Client.Settings.MULTIPLE_SIMS = false;

            InitializeComponent();

            RegisterAllPlugins(Assembly.GetExecutingAssembly());
            EnablePlugins(false);
        }

        private void Network_OnLogin(LoginStatus login, string message)
        {
            if (login == LoginStatus.Success)
            {
                EnablePlugins(true);
            }
            else if (login == LoginStatus.Failed)
            {
                BeginInvoke(
                    (MethodInvoker)delegate()
                    {
                        MessageBox.Show(this, String.Format("Error logging in ({0}): {1}",
                            Client.Network.LoginErrorKey, Client.Network.LoginMessage));
                        cmdConnect.Text = "Connect";
                        txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = true;
                        EnablePlugins(false);
                    });
            }
        }

        private void cmdConnect_Click(object sender, EventArgs e)
        {
            if (cmdConnect.Text == "Connect")
            {
                cmdConnect.Text = "Disconnect";
                txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = false;

                LoginParams loginParams = Client.Network.DefaultLoginParams(txtFirstName.Text, txtLastName.Text,
                    txtPassword.Text, "GUITestClient", "1.0.0");
                Client.Network.BeginLogin(loginParams);
            }
            else
            {
                Client.Network.Logout();
                cmdConnect.Text = "Connect";
                txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = true;
                EnablePlugins(false);
            }
        }

        private void EnablePlugins(bool enable)
        {
            tabControl.TabPages.Clear();
            tabControl.TabPages.Add(tabLogin);

            if (enable)
            {
                lock (Interfaces)
                {
                    foreach (TabPage page in Interfaces.Values)
                    {
                        tabControl.TabPages.Add(page);
                    }
                }
            }
        }

        private void RegisterAllPlugins(Assembly assembly)
        {
            foreach (Type t in assembly.GetTypes())
            {
                try
                {
                    if (t.IsSubclassOf(typeof(Interface)))
                    {
                        ConstructorInfo[] infos = t.GetConstructors();
                        Interface iface = (Interface)infos[0].Invoke(new object[] { this });
                        RegisterPlugin(iface);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }

        private void RegisterPlugin(Interface iface)
        {
            TabPage page = new TabPage();
            tabControl.TabPages.Add(page);

            iface.Client = Client;
            iface.TabPage = page;

            if (!Interfaces.ContainsKey(iface))
            {
                lock (Interfaces) Interfaces.Add(iface, page);
            }

            iface.Initialize();

            page.Text = iface.Name;
            page.ToolTipText = iface.Description;
        }

        private void frmTestClient_Paint(object sender, PaintEventArgs e)
        {
            lock (Interfaces)
            {
                foreach (Interface iface in Interfaces.Keys)
                {
                    iface.Paint(sender, e);
                }
            }
        }
    }
}
