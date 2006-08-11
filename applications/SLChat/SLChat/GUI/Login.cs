using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using SLNetworkComm;
using libsecondlife;

namespace SLChat
{
    public partial class frmLogin : Form
    {
    	private bool mainCreated;
    	public frmMain MainForm;
    	private SLNetCom netcom;
    	//These are used for our settings loading/saving
		//to see if we have changed our settings since they were last loaded
		private string setFirstName;
		private string setLastName;
		private string setLLocation; //Login Location
		private bool setSaveLogin; //Save Login check box

        public frmLogin()
        {
            InitializeComponent();
            
            //Try to load the login settings.
			//LoadLoginSettings();
			LoadProfiles(@"Profiles\\");

            netcom = new SLNetCom();
            MainForm = new frmMain(this,netcom);
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
					SaveLoginSettings();
				}
			}else{
				//If unchecked, delete the login settings
				//no point in having unused information remain.
				DeleteLoginSettings();
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
        
        public void OnClosing(object sender, System.EventArgs e)
		{
			//TODO: Fix this. What should it do? This should prevent
			//the whole application from closing if winChat has
			//been created and is visible. If winChat hasn't been
			//created, then the whole applicaiton should be closed.
			
			if(mainCreated==true)
			{
				MainForm.loginVisible = false;
				this.Hide();
			}else{
				this.Dispose();
			}
		}
        
        public void SaveLoginSettings()
		{
			//Saving our login settings
			//pick whatever filename with .xml extension
			string filename = @"Profiles\\"+txtFirstName.Text+"_"+txtLastName.Text+"\\settings.xml";
			SaveDirectory(@"Profiles\\"+txtFirstName.Text+"_"+txtLastName.Text+"\\");
			
			XmlDocument xmlDoc = new XmlDocument();

			try
			{
				xmlDoc.Load(filename);
			}
			catch(System.IO.FileNotFoundException)
			{
				//if file is not found, create a new xml file
				XmlTextWriter xmlWriter = new XmlTextWriter(filename, System.Text.Encoding.UTF8);
				xmlWriter.Formatting = Formatting.Indented;
				xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
				xmlWriter.WriteStartElement("SLChatSettings");
				//If WriteProcessingInstruction is used as above,
				//Do not use WriteEndElement() here
				//xmlWriter.WriteEndElement();
				//it will cause the &ltRoot></Root> to be &ltRoot />
				xmlWriter.Close();
				xmlDoc.Load(filename);
			}
				
			XmlNode root = xmlDoc.DocumentElement;
			XmlNodeList nodeList = xmlDoc.GetElementsByTagName("LoginSettings");
			if(nodeList.Count!=0){
				root.RemoveChild(nodeList[0]);
			}
			XmlElement loginSettings = xmlDoc.CreateElement("LoginSettings");
			string strSettings = "<FirstName value=\""+txtFirstName.Text+"\"/>"+
									"<LastName value=\""+txtLastName.Text+"\"/>"+
									"<LoginLocation value=\""+cbxLocation.Text+"\"/>"+
				"<SaveLoginInfo value=\""+chkSaveLogin.Checked.ToString().ToLower()+"\"/>";
			loginSettings.InnerXml = strSettings;
			
			root.AppendChild(loginSettings);
				
			xmlDoc.Save(filename);
			if(cbxProfiles.Items.Contains("No profiles found."))
			{
				cbxProfiles.Items.Remove("No profiles found.");
			}
			cbxProfiles.Items.Add(txtFirstName.Text+"_"+txtLastName.Text);
			cbxProfiles.SelectedItem = txtFirstName.Text+"_"+txtLastName.Text;
		}
		
        protected void SaveDirectory(string PathName)
        {
        	try
        	{
        		DirectoryInfo TheFolder = new DirectoryInfo(PathName);
                if (TheFolder.Exists)
                {
                    return;
                }

                throw new FileNotFoundException();
        	}
        	catch(FileNotFoundException )
        	{
        		DirectoryInfo TheDir = new DirectoryInfo(PathName);
        		TheDir.Create();
        		return;
        	}
        }
        
        protected void LoadProfiles(string PathName)
        {
            try
            {
                DirectoryInfo TheFolder = new DirectoryInfo(PathName);
                if (TheFolder.Exists)
                {
                	DirectoryInfo[] dirs = TheFolder.GetDirectories();
                	foreach(DirectoryInfo di in dirs)
                	{
                		cbxProfiles.Items.Add(di.Name);
                	}
                    //ListContentsOfFolder(TheFolder);
                    return;
                }

                throw new FileNotFoundException();
            }
            catch(FileNotFoundException )
            {
                //tbFolder.Text = PathName;
                cbxProfiles.Items.Add(
                     "No profiles found.");
            }
            catch(Exception e)
            {
                //tbFolder.Text = PathName;
                cbxProfiles.Items.Add("Problem occurred:");
                cbxProfiles.Items.Add(e.Message);
            }
            finally
            {
               // ListItems = false;
            }
        }
        
		public void LoadLoginSettings()
		{
			//Load our login settings.

			string filename = @"Profiles\\"+cbxProfiles.Text+"\\settings.xml";
			
			XmlDocument xmlDoc = new XmlDocument();

			try
			{
				xmlDoc.Load(filename);
			}
			catch(System.IO.FileNotFoundException)
			{
				return;
			}
				
			XmlNode root = xmlDoc.DocumentElement;
			XmlNodeList nodeList = xmlDoc.GetElementsByTagName("LoginSettings");
			if(nodeList.Count!=0){
				XmlNodeList nodeSLogin = xmlDoc.GetElementsByTagName("SaveLoginInfo");
				if(nodeSLogin[0].Attributes["value"].InnerText=="true"){
					//There may be a better way to get the values of each element
					//but I could not find one. - Oz
					XmlNodeList nodeFName = xmlDoc.GetElementsByTagName("FirstName");
					txtFirstName.Text = setFirstName = nodeFName[0].Attributes["value"].InnerText;
					XmlNodeList nodeLName = xmlDoc.GetElementsByTagName("LastName");
					txtLastName.Text = setLastName = nodeLName[0].Attributes["value"].InnerText;
					XmlNodeList nodeLLocation = xmlDoc.GetElementsByTagName("LoginLocation");
					cbxLocation.Text = setLLocation = nodeLLocation[0].Attributes["value"].InnerText;
					chkSaveLogin.Checked = setSaveLogin = true;
				}else{
					chkSaveLogin.Checked = setSaveLogin = false;
						
				}
			}

		}
		
		public void DeleteLoginSettings()
		{
			//Delete our login settings.
			
			string filename = @"Profiles\\"+cbxProfiles.Text+"\\settings.xml";

			XmlDocument xmlDoc = new XmlDocument();

			try
			{
				xmlDoc.Load(filename);
			}
			catch(System.IO.FileNotFoundException)
			{
				return;
			}
			XmlNode root = xmlDoc.DocumentElement;
			XmlNodeList nodeList = xmlDoc.GetElementsByTagName("LoginSettings");
			if(nodeList.Count!=0){
				//Simply remove that NodeList that composes our "LoginSettings"
				root.RemoveChild(nodeList[0]);
			}
			xmlDoc.Save(filename);
		}
        
        void CbxProfilesSelectedIndexChanged(object sender, System.EventArgs e)
        {
        	LoadLoginSettings();
        }
    }
}
