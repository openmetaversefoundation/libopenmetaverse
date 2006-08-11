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

        public IMTabWindow(SLNetCom netcom, LLUUID target, LLUUID session, string toName)
        {
            InitializeComponent();

            this.netcom = netcom;
            this.target = target;
            this.session = session;
            this.toName = toName;

            textManager = new IMTextManager(new RichTextBoxPrinter(rtbIMText), this.netcom);
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
