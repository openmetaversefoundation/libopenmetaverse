using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SLNetworkComm;
using libsecondlife;

namespace SLChat
{
    public partial class frmIMs : Form
    {
        private SLNetCom netcom;
        private Dictionary<string, IMTabWindow> IMTabs;
        private PrefsManager prefs;

        public frmIMs(SLNetCom netcom, PrefsManager preferences)
        {
            InitializeComponent();

            this.netcom = netcom;
            IMTabs = new Dictionary<string, IMTabWindow>();
            prefs = preferences;

            this.AddNetcomEvents();
        }

        private void AddNetcomEvents()
        {
            netcom.InstantMessageReceived += new EventHandler<InstantMessageEventArgs>(netcom_InstantMessageReceived);
        }

        private void netcom_InstantMessageReceived(object sender, InstantMessageEventArgs e)
        {
            if (IMTabs.ContainsKey(e.FromAgentName)) return;

            this.AddIMTab(e.FromAgentId, e.Id, e.FromAgentName, e);
        }

        public IMTabWindow AddIMTab(LLUUID target, LLUUID session, string targetName)
        {
            TabPage tabpage = new TabPage(targetName);
            IMTabWindow imTab = new IMTabWindow(netcom, target, session, targetName, prefs);
            imTab.Dock = DockStyle.Fill;
			imTab.formIM = this;
            
            tabpage.Controls.Add(imTab);

            tabIMs.TabPages.Add(tabpage);
            IMTabs.Add(targetName, imTab);

            return imTab;
        }

        public IMTabWindow AddIMTab(LLUUID target, LLUUID session, string targetName, InstantMessageEventArgs e)
        {
            IMTabWindow imTab = this.AddIMTab(target, session, targetName);
            imTab.TextManager.PassIMEvent(e);

            return imTab;
        }

        public void RemoveIMTab(string targetName)
        {
            IMTabWindow imTab = IMTabs[targetName];
            TabPage tabpage = (TabPage)imTab.Parent;

            IMTabs.Remove(targetName);
            imTab = null;

            tabIMs.TabPages.Remove(tabpage);
            tabpage.Dispose();
        }

        private void frmIMs_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }
    }
}
