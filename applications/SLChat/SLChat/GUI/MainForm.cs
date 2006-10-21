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
        private frmAbout AboutForm;
        private frmLogin LoginForm;
        private frmPrefs PrefsForm;
        private Dictionary<string, IMTabWindow> IMTabs;
        private PrefsManager prefs;

        private frmIMs IMForm;
        
        public bool loginVisible;
        public bool aboutCreated;
        public bool prefsCreated;
		Hashtable settings = new Hashtable();
		//Stores residents who have spoken Name and Key
		//used for printing key to text (and later things).
		Hashtable userHash = new Hashtable();
		private int lastchan;
        
        public frmMain(frmLogin Login, SLNetCom net,PrefsManager preferences)
        {
            InitializeComponent();

            LoginForm = Login;
            netcom = net;
			
            prefs = preferences;
            
            netcom.NetcomSync = this;
            netcom.LoginOptions.UserAgent = "SLChat v0.0.0.2";
            netcom.LoginOptions.Author = "ozspade@slinked.net";
            this.AddNetcomEvents();

            chatManager = new ChatTextManager(new RichTextBoxPrinter(rtbChat), netcom, prefs);

            this.RefreshWindowTitle();

            IMTabs = new Dictionary<string, IMTabWindow>();
            IMForm = new frmIMs(netcom, prefs);
            IMForm.VisibleChanged += new EventHandler(IMForm_VisibleChanged);
        }
        
        private void frmMain_VisibleChanged(object sender, EventArgs e)
        {
        	LoadSettings("ChatSettings");
            LoadSettings("TimestampSettings");
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
            if (!tbtnIM.Checked)
            {
            	tbtnIM.ForeColor = Color.Red;
            }
            
            if (IMTabs.ContainsKey(e.FromAgentName))
            {
            	return;
            }

            this.AddIMTab(e.FromAgentId, e.Id, e.FromAgentName, e);
        }

        private void netcom_ChatReceived(object sender, ChatEventArgs e)
        {
            if (e.FromName == netcom.LoginOptions.FullName & !prefs.setListUserName) return;

            string name = e.FromName;
            if (e.FromName == netcom.LoginOptions.FullName & !prefs.setUseFullName & e.SourceType == SLSourceType.Avatar) name = "You";
            
            if(e.SourceType == SLSourceType.Object) name = "(Ob) " + e.FromName;
            
            //Send our name and UUID to the function along
            //with telling it that we are adding (true) not
            //deleting (false)
            UpdateUserList(name,e.SourceId.ToString(),true);
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
            if (e.Control)
            {
            	if(e.Shift)
            	{
            		if(e.KeyCode == Keys.Enter){
            			//Ctrl+Shift+Enter = Whisper
            			int chan = ChannelGrabber(txtInput.Text);
            			string message = MessageCleanup(txtInput.Text,chan);
            			netcom.ChatOut(message, SLChatType.Whisper, chan);
            			this.ClearChatInput();
            		}
            	}else{
            		if(e.KeyCode == Keys.Enter){
            			//Ctrl+Enter = Shout
            			int chan = ChannelGrabber(txtInput.Text);
            			string message = MessageCleanup(txtInput.Text,chan);
                		netcom.ChatOut(message, SLChatType.Shout, chan);
                		this.ClearChatInput();
            		}else if(e.KeyCode == Keys.A){
            			//Ctrl+A = Select all
            			txtInput.SelectAll();
            		}
            	}
            }
            else if(e.KeyCode == Keys.Enter)
            {
            	//Enter only = whatever is in the combobox
            	int chan = ChannelGrabber(txtInput.Text);
            	string message = MessageCleanup(txtInput.Text,chan);
                if(cbxChatType.Text=="Say")
        		{
        			netcom.ChatOut(message, SLChatType.Say, chan);
        		}else if(cbxChatType.Text=="Shout"){
        			netcom.ChatOut(message, SLChatType.Shout, chan);
        		}else if(cbxChatType.Text=="Whisper"){
        			netcom.ChatOut(message, SLChatType.Whisper, chan);
        		}
                this.ClearChatInput();
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
        	//Send whatever is in the combobox
        	int chan = ChannelGrabber(txtInput.Text);
           	string message = MessageCleanup(txtInput.Text,chan);
        	if(cbxChatType.Text=="Say")
        	{
        		netcom.ChatOut(message, SLChatType.Say, chan);
        	}else if(cbxChatType.Text=="Shout"){
        		netcom.ChatOut(message, SLChatType.Shout, chan);
        	}else if(cbxChatType.Text=="Whisper"){
        		netcom.ChatOut(message, SLChatType.Whisper, chan);
        	}
            this.ClearChatInput();
        }
        
        private string MessageCleanup(string message, int chan)
        {
        	//Here we clean up the message by removing "//"
        	//and "/<channel> " from strings if the Channel is not
        	//0. Returns the cleaned up message.
        	string mess = message;
        	if(chan!=0)
        	{
        		if(message.StartsWith("//")){
        			mess = message.Remove(0,2);
        		}else{
        			mess = message.Remove(0,message.Split(' ')[0].Length + 1);
        		}
        	}
        	
        	return mess;
        }
        
        private int ChannelGrabber(string message)
        {
        	//Grabs the channel from the message if a channel
        	//is given. Returns the found channel.
        	if(message.StartsWith("//")){
        		return lastchan;
        	}else if(message.StartsWith("/")){
        	   	string mes = message.Split(' ')[0];
        	   	if(mes=="/me"){
        	   		return 0;
        	   	}
        	   	mes = mes.Remove(0,1);
        	   	int res = int.Parse(mes);
        	   	lastchan = res;
        	   	return res;
        	}else{
        	   	return 0;
        	}
        }

        private void ClearChatInput()
        {
            //txtInput.Items.Add(txtInput.Text); Need to instead add items to an array to be recalled by Arrow keys?
            txtInput.Text = string.Empty;
        }

        private void mnuFileExit_Click(object sender, EventArgs e)
        {
        	if (netcom.LoggedIn)
        	{
        		netcom.Logout();
        	}
        	
        	LoginForm.CloseApp();
        }

        private void frmMain_Closing(object sender, FormClosingEventArgs e)
        {
        	if (netcom.LoggedIn)
        	{
        		netcom.Logout();
        	}
            
        	if(loginVisible==false)
        	{
        		LoginForm.CloseApp();
        	}else{
        		e.Cancel = true;
        		this.Hide();
        	}
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
        
        private void Link_Clicked (object sender, System.Windows.Forms.LinkClickedEventArgs e)
		{
        	//User clicked a link in the chat, launch default browser
   			System.Diagnostics.Process.Start(e.LinkText);
		}

        private void mnuHelpAbout_Click(object sender, EventArgs e)
        {
        	if(!aboutCreated)
        	{
           		AboutForm = new frmAbout(this);
            	AboutForm.ShowDialog();
            	aboutCreated = true;
        	}else{
        		AboutForm.Focus();
        	}
        }
        
        private void MnuEditPrefsClick(object sender, System.EventArgs e)
        {
        	if(!prefsCreated)
        	{
        		PrefsForm = new frmPrefs(this,netcom.LoginOptions.FirstName+"_"+netcom.LoginOptions.LastName,prefs);
        		PrefsForm.Show();
        		prefsCreated = true;
        	}else{
        		PrefsForm.Focus();
        	}
        }
        
        private void MnuKeyClick(object sender, System.EventArgs e)
        {
        	//This prints the selected user's Key to rChatHistory
			//with the pretext of "Program:"
			if(userHash.ContainsValue(lbxUsers.SelectedItem.ToString()))
			{
				IDictionaryEnumerator myEnum = userHash.GetEnumerator();
      			while (myEnum.MoveNext())
      			{
      				if(myEnum.Value.ToString()==lbxUsers.SelectedItem.ToString())
      				{
      					string name = myEnum.Value.ToString();
      					string key = myEnum.Key.ToString();
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
        	//Remove a user from the list and update user list
        	if(lbxUsers.SelectedItem.ToString() != null)
        	{
        		UpdateUserList(lbxUsers.SelectedItem.ToString(),null,false);
        	}
        }
        
        public IMTabWindow AddIMTab(LLUUID target, LLUUID session, string targetName)
        {
            TabPage tabpage = new TabPage(targetName);
            IMTabWindow imTab = new IMTabWindow(netcom, target, session, targetName, prefs);
            imTab.Dock = DockStyle.Fill;
            imTab.formMain = this;

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
        
        private void UpdateUserList(string name, string key, bool adding)
        {
        	//Update the user list to either add a user to the
        	//userhash and list box or remove a user based on "adding"
        	//True = add
        	//False = remove
        	if(adding==true)
        	{
        		if(userHash.ContainsKey(key))
				{
        			//We loop through the enumerator so that we can
        			//compare the matching value.
					IDictionaryEnumerator myEnum = userHash.GetEnumerator();
      				while (myEnum.MoveNext())
      				{
      					//If we find a matching key
      					if(myEnum.Key.ToString()==key)
      					{
      						//And if the name is not the same and
      						//has changed
      						if(myEnum.Value.ToString()!=name)
      						{
      							//Remove the old name and key.
      							userHash.Remove(key);
      							lbxUsers.Items.Remove(myEnum.Value.ToString());
    							//Add the Name and Key of the
								//resident who spoke. Used for printing key.
								userHash.Add(key,name);
								//Finaly add the name of the resident to the
								//list box.
      							lbxUsers.Items.Add(name);
      							return;
      						}
            				
      					}
      				}
        		}else{
        			//We add the fresh new name and key.
        			//Add the Name and Key of the
					//resident who spoke. Used for printing key.
					userHash.Add(key,name);
					//and then add it tot he listbox again.
               		lbxUsers.Items.Add(name);
            	}
        	}else{
        		//removing from the user list
        		//Removing a user from the Names List. This should
				//perhaps be done automaticly somehow, maybe by offline
				//status or distance?
				if(name != null)
				{
					//This is basicly removing the person from our
					//userHash, hashtable, which stores Name and Key used
					//for printing key to chat.
					if(userHash.ContainsValue(name))
					{
						IDictionaryEnumerator myEnum = userHash.GetEnumerator();
      					while (myEnum.MoveNext())
      					{
      						if(myEnum.Value.ToString()==name)
      						{
      							//Remove from our hashtable, just to keep things clean.
      							userHash.Remove(myEnum.Key);
      							//Remove from the actual list element.
								lbxUsers.Items.Remove(name);
								return;
      						}
      					}
					}
				}
        	}
        }
        
        public void LoadSettings(string parentnode)
		{
        	//Our local load settings, used when we start up the
        	//chat/main form.
			prefs.LoadSettings(netcom.LoginOptions.FirstName+"_"+netcom.LoginOptions.LastName,parentnode);
        	
        	settings = prefs.settings;
        	
        	if(!settings.ContainsKey("Error"))
        	{
        		IDictionaryEnumerator myEnum = settings.GetEnumerator();
      			while (myEnum.MoveNext())
      			{
      				if(parentnode=="ChatSettings")
      				{
      					if(myEnum.Key.ToString()=="UseFullName")
      					{
      						if(myEnum.Value.ToString()=="true"){
      							prefs.setUseFullName = true;
      						}else{
      							prefs.setUseFullName = false;
      						}
      					}else if(myEnum.Key.ToString()=="ListUserName"){
      						if(myEnum.Value.ToString()=="true"){
      							prefs.setListUserName = true;
      						}else{
      							prefs.setListUserName = false;
      							UpdateUserList(netcom.LoginOptions.FullName,null,false);
      							UpdateUserList("You",null,false);
      						}
      					}
      				}else if(parentnode=="TimestampSettings"){
      					if(myEnum.Key.ToString()=="ShowIMTimestamps")
      					{
      						if(myEnum.Value.ToString()=="true")
      						{
      							prefs.setIMTimestamps = true;
      						}else{
      							prefs.setIMTimestamps = false;
      						}
      					}else if(myEnum.Key.ToString()=="ShowChatTimestamps"){
      						if(myEnum.Value.ToString()=="true"){
      							prefs.setChatTimestamps = true;
      						}else{
      							prefs.setChatTimestamps = false;
      						}
      					}else if(myEnum.Key.ToString()=="IMTimeZone"){
      						prefs.setIMTimeZ = myEnum.Value.ToString();
      					}else if(myEnum.Key.ToString()=="ChatTimeZone"){
      						prefs.setChatTimeZ = myEnum.Value.ToString();
      					}else if(myEnum.Key.ToString()=="IMStampFormat"){
      						prefs.setIMStampFormat = myEnum.Value.ToString();
      					}else if(myEnum.Key.ToString()=="ChatStampFormat"){
      						prefs.setChatStampFormat = myEnum.Value.ToString();
      					}else if(myEnum.Key.ToString()=="SyncStampSettings"){
      						if(myEnum.Value.ToString()=="true"){
      							prefs.setSyncTimestamps = true;
      						}else{
      							prefs.setSyncTimestamps = false;
      						}
      					}
      				}
      			}
        	}else{
        		IDictionaryEnumerator myEnum = settings.GetEnumerator();
      			while (myEnum.MoveNext())
      			{
      				if(myEnum.Key.ToString()=="Error")
      				{
      					//rtbStatus.Text = "Error loading settings: "+myEnum.Value.ToString();
      				}
      			}
        	}
        	settings.Clear();
        	prefs.settings.Clear();
		}
    }
}
