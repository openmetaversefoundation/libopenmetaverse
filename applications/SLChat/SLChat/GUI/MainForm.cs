using System;
using System.Collections;
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
    public partial class frmMain : Form
    {
        private SLNetCom netcom;
        private ChatTextManager chatManager;
        private frmLogin LoginForm;

        private frmIMs IMForm;
        
        public string fname;
        public string lname;
        public string pwrd;
        public bool loginVisible;
		//Stores residents who have spoken Name and Key
		//used for printing key to text (and later things).
		Hashtable userHash = new Hashtable();
        
        public frmMain(frmLogin Login, SLNetCom net)
        {
            InitializeComponent();

            LoginForm = Login;
            netcom = net;

            netcom.NetcomSync = this;
            netcom.LoginOptions.UserAgent = "SLChat v0.0.0.2";
            netcom.LoginOptions.Author = "ozspade@slinked.net";
            this.AddNetcomEvents();

            chatManager = new ChatTextManager(new RichTextBoxPrinter(rtbChat), netcom);

            this.RefreshWindowTitle();

            IMForm = new frmIMs(netcom);
            IMForm.VisibleChanged += new EventHandler(IMForm_VisibleChanged);
        }

        private void IMForm_VisibleChanged(object sender, EventArgs e)
        {
            mnuViewIMs.Checked = tbtnIM.Checked = IMForm.Visible;
        }
        
        private void AddNetcomEvents()
        {
            netcom.ClientLoggedIn += new EventHandler<ClientLoginEventArgs>(netcom_ClientLoggedIn);
            netcom.ClientLoggedOut += new EventHandler<ClientLoginEventArgs>(netcom_ClientLoggedOut);
            netcom.ChatReceived += new EventHandler<ChatEventArgs>(netcom_ChatReceived);
            netcom.InstantMessageReceived += new EventHandler<InstantMessageEventArgs>(netcom_InstantMessageReceived);
        }

        private void netcom_InstantMessageReceived(object sender, InstantMessageEventArgs e)
        {
            if (!tbtnIM.Checked) tbtnIM.ForeColor = Color.Red;
        }

        private void netcom_ChatReceived(object sender, ChatEventArgs e)
        {
            if (e.FromName == netcom.LoginOptions.FullName) return;

            if (!lbxUsers.Items.Contains(e.FromName))
            {
                lbxUsers.Items.Add(e.FromName);
            	//Add the Name (extra1) and Key (extra2) of the
				//resident who spoke. Used for printing key.
				userHash.Add(e.FromName,e.SourceId);
            }
        }

        private void netcom_ClientLoggedOut(object sender, ClientLoginEventArgs e)
        {
            mnuLoginout.Text = "&Login...";
			
            cbxChatType.Enabled = false;
            txtInput.Enabled = false;
            btnSend.Enabled = false;

            this.RefreshWindowTitle();
        }

        private void netcom_ClientLoggedIn(object sender, ClientLoginEventArgs e)
        {
            mnuLoginout.Text = "&Logout";

            txtInput.Enabled = true;
            cbxChatType.Enabled = true;

            this.RefreshWindowTitle();
        }

        private void mnuLoginout_Click(object sender, EventArgs e)
        {
            switch (mnuLoginout.Text)
            {
                case "&Login...":
            		if(loginVisible)
            		{
            			LoginForm.Focus();
            		}else{
            			loginVisible = true;
            			LoginForm.Show();
            		}
                    break;

                case "&Logout":
                    netcom.Logout();
                    break;
            }
        }

        private void txtInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            if (e.Control)
            {
            	if(e.Shift)
            	{
            		netcom.ChatOut(txtInput.Text, SLChatType.Whisper, 0);
            	}else{
                	netcom.ChatOut(txtInput.Text, SLChatType.Shout, 0);
            	}
            }
            else
            {
                if(cbxChatType.Text=="Say")
        		{
        			netcom.ChatOut(txtInput.Text, SLChatType.Say, 0);
        		}else if(cbxChatType.Text=="Shout"){
        			netcom.ChatOut(txtInput.Text, SLChatType.Shout, 0);
        		}else if(cbxChatType.Text=="Whisper"){
        			netcom.ChatOut(txtInput.Text, SLChatType.Whisper, 0);
        		}
            }

            this.ClearChatInput();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
        	if(cbxChatType.Text=="Say")
        	{
        		netcom.ChatOut(txtInput.Text, SLChatType.Say, 0);
        	}else if(cbxChatType.Text=="Shout"){
        		netcom.ChatOut(txtInput.Text, SLChatType.Shout, 0);
        	}else if(cbxChatType.Text=="Whisper"){
        		netcom.ChatOut(txtInput.Text, SLChatType.Whisper, 0);
        	}
            this.ClearChatInput();
        }

        private void ClearChatInput()
        {
            //txtInput.Items.Add(txtInput.Text); Need to instead add items to an array to be recalled by Arrow keys?
            txtInput.Text = string.Empty;
        }

        private void mnuFileExit_Click(object sender, EventArgs e)
        {
        	this.Close();
        }

        private void frmMain_StopClose(object sender, EventArgs e)
        {
        	if (netcom.LoggedIn)
        		netcom.Logout();
            
        	if(!netcom.LoggedIn)
            	Application.Exit();
        }
        
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (netcom.LoggedIn)
            	netcom.Logout();
            
            if(!netcom.LoggedIn)
            	Application.Exit();
        }

        private void RefreshWindowTitle()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SLChat: ");

            if (netcom.LoggedIn)
            {
                sb.Append(netcom.LoginOptions.FullName);
            }
            else
            {
                sb.Append("Logged Out");
            }

            this.Text = sb.ToString();
            sb = null;
        }

        private void tbtnIM_Click(object sender, EventArgs e)
        {
            IMForm.Visible = tbtnIM.Checked;
            tbtnIM.ForeColor = Color.FromKnownColor(KnownColor.ControlText);
        }

        private void mnuViewIMs_Click(object sender, EventArgs e)
        {
            IMForm.Visible = mnuViewIMs.Checked;
        }

        private void txtInput_TextChanged(object sender, EventArgs e)
        {
            btnSend.Enabled = (txtInput.Text.Length > 0);
        }

        private void mnuHelpAbout_Click(object sender, EventArgs e)
        {
            frmAbout about = new frmAbout();
            about.ShowDialog();
        }
        
        private void MnuKeyClick(object sender, System.EventArgs e)
        {
        	//This prints the selected user's Key to rChatHistory
			//with the pretext of "Program:"
			if(userHash.ContainsKey(lbxUsers.SelectedItem.ToString()))
			{
				IDictionaryEnumerator myEnum = userHash.GetEnumerator();
      			while (myEnum.MoveNext())
      			{
      				if(myEnum.Key.ToString()==lbxUsers.SelectedItem.ToString())
      				{
      					string name = myEnum.Key.ToString();
      					string key = myEnum.Value.ToString();
      					key = key.Insert(8,"-");
						key = key.Insert(13,"-");
						key = key.Insert(18,"-");
						key = key.Insert(23,"-");
						chatManager.PrintProgramMessage("Program: "+ name + " == " + key);
      				}
      			}
			}
        }
        
        private void MnuRemoveClick(object sender, System.EventArgs e)
        {
        	//Removing a user from the Names List. This should
			//perhaps be done automaticly somehow, maybe by offline
			//status or distance?
			if(this.lbxUsers.Items.ToString() != "")
			{
				//This is basicly removing the person from our
				//userHash, hashtable, which stores Name and Key used
				//for printing key to chat.
				if(userHash.ContainsKey(lbxUsers.SelectedItem.ToString()))
				{
					//Remove from our hashtable, just to keep things clean.
					userHash.Remove(lbxUsers.SelectedItem.ToString());
      				//Remove from the actual list element.
					lbxUsers.Items.Remove(lbxUsers.SelectedItem);
				}
			}
        }
    }
}
