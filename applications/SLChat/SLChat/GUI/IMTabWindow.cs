using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SLNetworkComm;
using libsecondlife;

namespace SLChat
{
    public partial class IMTabWindow : UserControl
    {
        private SLNetCom netcom;
        private LLUUID target;
        private LLUUID session;
        private string toName;
        private IMTextManager textManager;
        private PrefsManager prefs;
        public frmMain formMain;
        public frmIMs formIM;

        public IMTabWindow(SLNetCom netcom, LLUUID target, LLUUID session, string toName, PrefsManager preferences)
        {
            InitializeComponent();

            this.netcom = netcom;
            this.target = target;
            this.session = session;
            this.toName = toName;
            this.prefs = preferences;

            textManager = new IMTextManager(new RichTextBoxPrinter(rtbIMText), this.netcom, prefs);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            netcom.SendInstantMessage(cbxInput.Text, target, session);
            this.ClearIMInput();
        }

        private void cbxInput_TextChanged(object sender, EventArgs e)
        {
            btnSend.Enabled = (cbxInput.Text.Length > 0);
        }

        private void cbxInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            if (cbxInput.Text.Length == 0) return;

            netcom.SendInstantMessage(cbxInput.Text, target, session);
            this.ClearIMInput();
        }
        
        private void Link_Clicked (object sender, System.Windows.Forms.LinkClickedEventArgs e)
		{
   			System.Diagnostics.Process.Start(e.LinkText);
		}
        
        private void BtnCloseClick(object sender, System.EventArgs e)
        {
        	if(formMain!=null)
        	{
        		formMain.RemoveIMTab(this.toName);
        	}
        	
        	if(formIM!=null)
        	{
        		formIM.RemoveIMTab(this.toName);
        	}
        }
        
        private void BtnPrintKeyClick(object sender, System.EventArgs e)
        {
        	string key = target.ToString();
        	key = key.Insert(8,"-");
			key = key.Insert(13,"-");
			key = key.Insert(18,"-");
			key = key.Insert(23,"-");
			textManager.PrintProgramMessage("Program: "+ toName + " == " + key);
        }

        private void ClearIMInput()
        {
            cbxInput.Items.Add(cbxInput.Text);
            cbxInput.Text = string.Empty;
        }

        public LLUUID TargetId
        {
            get { return target; }
            set { target = value; }
        }

        public string TargetName
        {
            get { return toName; }
            set { toName = value; }
        }

        public LLUUID SessionId
        {
            get { return session; }
            set { session = value; }
        }

        public IMTextManager TextManager
        {
            get { return textManager; }
            set { textManager = value; }
        }
    }
}
