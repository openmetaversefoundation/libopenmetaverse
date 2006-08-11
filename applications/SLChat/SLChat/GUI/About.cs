using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SLChat
{
    public partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
            rtbCredits.SelectionFont = new Font(rtbCredits.Font, FontStyle.Bold);
            rtbCredits.AppendText("SLChat v0.0.0.2");
            rtbCredits.SelectionFont = new Font(rtbCredits.Font, FontStyle.Regular);
           	rtbCredits.AppendText("\nhttp://www.libsecondlife.org/content/view/16/32/\n\nUsing the libsecondlife library (http://libsecondlife.org)\n\nDeveloped by: Oz Spade, Delta Czukor, Baba Yamamoto\n\nBuilt off of Delta Czukor's SLeek\n\n\n\nBut how do the snowmen talk?");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
