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
    	private frmMain MainForm;
    	
        public frmAbout(frmMain main)
        {
            InitializeComponent();
            MainForm = main;
            rtbCredits.SelectionFont = new Font(rtbCredits.Font, FontStyle.Bold);
            rtbCredits.AppendText("SLChat v0.0.0.3");
            rtbCredits.SelectionFont = new Font(rtbCredits.Font, FontStyle.Regular);
           	rtbCredits.AppendText("\nhttp://www.libsecondlife.org/content/view/16/32/"+
                                  "\n\nUsing the libsecondlife library (http://libsecondlife.org)"+
                                  "\n\nDeveloped by: Oz Spade, Delta Czukor, Baba Yamamoto"+
                                  "\n\nBuilt off of Delta Czukor's SLeek"+
                                  "\n\n\n\nA nark posing as someone who poses as a nark.");
        }
        
        private void Link_Clicked (object sender, System.Windows.Forms.LinkClickedEventArgs e)
		{
   			System.Diagnostics.Process.Start(e.LinkText);
		}

        private void btnClose_Click(object sender, EventArgs e)
        {
        	MainForm.aboutCreated = false;
            this.Close();
        }
    }
}
