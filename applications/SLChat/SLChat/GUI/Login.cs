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
    public partial class frmLogin : Form
    {
    	private bool mainCreated; //Checks if we've created a mainform
    	public frmMain MainForm; //main form tossing back and forth
    	private SLNetCom netcom; //Network communication
    	public PrefsManager prefs; //Our preferences manger.
    	//These are used for our settings loading/saving
		//to see if we have changed our settings since they were last loaded
		private string setFirstName;
		private string setLastName;
		private string setLLocation; //Login Location
		private bool setSaveLogin; //Save Login check box
		private Hashtable settings = new Hashtable(); //Hashtable for our loaded settings.

        public frmLogin()
        {
            InitializeComponent();
            
            //Try to load the login settings.
			prefs = new PrefsManager();
			prefs.LoadProfiles();
			//Load the profiles to our combobox.
			string[] profiles = new string[100];
			profiles = prefs.profiles;
			for(int i=0;i<profiles.Length;i++)
			{
				if(profiles[i]!=null)
				{
					cbxProfiles.Items.Add(profiles[i].ToString());
				}else{
					//Exit when we hit a null profile.
					break;
				}
			}
			
            netcom = new SLNetCom();
            MainForm = new frmMain(this,netcom,prefs);
            this.AddNetcomEvents();
        }
        
        private void frmLogin_Shown(object sender, EventArgs e)
        {
        	if(!netcom.LoggedIn) btnLogin.Enabled = true;
        }

        private void AddNetcomEvents()
        {
            netcom.ClientLoggedIn += new EventHandler<ClientLoginEventArgs>(netcom_ClientLoggedIn);
            netcom.ClientLoggedOut += new EventHandler<ClientLoginEventArgs>(netcom_ClientLoggedOut);
            netcom.ClientLoginError += new EventHandler<ClientLoginEventArgs>(netcom_ClientLoginError);
        }

        private void netcom_ClientLoginError(object sender, ClientLoginEventArgs e)
        {
            rtbStatus.Text = e.LoginReply;
            btnLogin.Enabled = true;
        }

        private void netcom_ClientLoggedIn(object sender, ClientLoginEventArgs e)
        {
        	//Check if we've already created the chatscreen
			//and react approprietly.
			if(!mainCreated){
				//create the chat screen and send ourselves
				//so the chatscreen can reference us easily.
				MainForm.Show();
				mainCreated = true;
				MainForm.loginVisible = false;
				this.Hide();
			}else{
				MainForm.Focus();
				this.Hide();
			}
        }
        
        private void netcom_ClientLoggedOut(object sender, ClientLoginEventArgs e)
        {
        	btnLogin.Enabled = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
        	if(mainCreated)
        	{
        		MainForm.loginVisible = false;
        		this.Hide();
        	}else{
            	this.Close();
        	}
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
        	//Save our settings on the following conditions:
			//The user has checked the appropriet checkbox
			//The content has changed from last being loaded.
			if(chkSaveLogin.Checked != false)
			{
				//If the checkbox is checked
				if(chkSaveLogin.Checked != setSaveLogin | txtFirstName.Text != setFirstName | txtLastName.Text != setLastName | cbxLocation.Text != setLLocation)
				{
					//If there have been any changes from when
					//the settings were last loaded.
					//Setting up our child node settings.
					string strSettings = "<SaveLoginInfo value=\""+chkSaveLogin.Checked.ToString().ToLower()+"\"/>"+
									"<FirstName value=\""+txtFirstName.Text+"\"/>"+
									"<LastName value=\""+txtLastName.Text+"\"/>"+
									"<LoginLocation value=\""+cbxLocation.Text+"\"/>";
					//Sending off the settings to be saved.
					prefs.SaveSettings(txtFirstName.Text+"_"+txtLastName.Text,"LoginSettings",strSettings);
					//Clean up the combo box that lists profiles.
					if(cbxProfiles.Items.Contains("No profiles found."))
					{
						cbxProfiles.Items.Remove("No profiles found.");
					}
					cbxProfiles.Items.Add(txtFirstName.Text+"_"+txtLastName.Text);
					cbxProfiles.SelectedItem = txtFirstName.Text+"_"+txtLastName.Text;
				}
			}else{
				//If unchecked, delete the login settings
				//no point in having unused information remain.
				prefs.DeleteSettings(cbxProfiles.Text,"LoginSettings");
			}
				
            btnLogin.Enabled = false;

            rtbStatus.Text = "Logging in...";
            Application.DoEvents();

            netcom.LoginOptions.FirstName = txtFirstName.Text;
            netcom.LoginOptions.LastName = txtLastName.Text;
            netcom.LoginOptions.Password = txtPassword.Text;
            netcom.LoginOptions.StartLocation = cbxLocation.Text;
           	netcom.Login();
        }
        
        public void CloseApp()
        {
        	this.Dispose();
        }
        
        private void frmLogin_Closing(object sender, FormClosingEventArgs e)
        {
        	if(mainCreated==true)
			{
        		if(MainForm.Visible==true)
        		{
					MainForm.loginVisible = false;
					e.Cancel = true;
					this.Hide();
        		}else{
        			this.Dispose();
        		}
			}else{
				this.Dispose();
			}
        }
        
        private void CbxProfilesSelectedIndexChanged(object sender, System.EventArgs e)
        {
      
        	//Load login settings based on what profile is selected
        	prefs.LoadSettings(cbxProfiles.Text,"LoginSettings");
        	
        	settings = prefs.settings;
        	
        	if(!settings.ContainsKey("Error"))
        	{
        		IDictionaryEnumerator myEnum = settings.GetEnumerator();
      			while (myEnum.MoveNext())
      			{
      				if(myEnum.Key.ToString()=="SaveLoginInfo")
      				{
      					chkSaveLogin.Checked = setSaveLogin = true;
      				}else if(myEnum.Key.ToString()=="FirstName"){
      					txtFirstName.Text = setFirstName = myEnum.Value.ToString();
      				}else if(myEnum.Key.ToString()=="LastName"){
      					txtLastName.Text = setLastName = myEnum.Value.ToString();
      				}else if(myEnum.Key.ToString()=="LoginLocation"){
      					cbxLocation.Text = setLLocation = myEnum.Value.ToString();
      				}
      			}
        	}else{
        		IDictionaryEnumerator myEnum = settings.GetEnumerator();
      			while (myEnum.MoveNext())
      			{
      				if(myEnum.Key.ToString()=="Error")
      				{
      					rtbStatus.Text = "Error loading settings: "+myEnum.Value.ToString();
      				}
      			}
        	}
        	settings.Clear();
        	prefs.settings.Clear();
        }
    }
}
