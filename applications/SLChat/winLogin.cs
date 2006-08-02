using System;
using System.IO;
using System.Security.Cryptography;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.Xml.XPath;
using System.Threading;
using libsecondlife;

namespace SLChat
{
	/// <summary>
	/// Summary description for winLogin.
	/// From here the user will login to SL.
	/// </summary>
	public class winLogin : System.Windows.Forms.Form
	{

		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.StatusBar mainStatusBar;

		private System.Windows.Forms.Panel LoginPanel;
		private System.Windows.Forms.GroupBox grpLogin;
		private System.Windows.Forms.Button btnLogin;
		private System.Windows.Forms.TextBox txtfirstname;
		private System.Windows.Forms.TextBox txtlastname;
		private System.Windows.Forms.TextBox txtpassword;
		public bool exitApp = false;
		public string accountid;
		public bool chatCreated; //Used to check if we've created the chat window or not
		public static ChatScreen winChat; //Easy calling to the chat window.
		//These are used for our settings loading/saving
		//to see if we have changed our settings since they were last loaded
		public string setFirstName;
		public string setLastName;
		public string setLLocation; //Login Location
		public bool setSaveLogin; //Save Login check box
		
		public winLogin()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.mainStatusBar = new System.Windows.Forms.StatusBar();
			this.LoginPanel = new System.Windows.Forms.Panel();
			this.grpLogin = new System.Windows.Forms.GroupBox();
			this.button1 = new System.Windows.Forms.Button();
			this.chkSaveLogin = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.cmbLocation = new System.Windows.Forms.ComboBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.txtpassword = new System.Windows.Forms.TextBox();
			this.txtlastname = new System.Windows.Forms.TextBox();
			this.txtfirstname = new System.Windows.Forms.TextBox();
			this.btnLogin = new System.Windows.Forms.Button();
			this.LoginPanel.SuspendLayout();
			this.grpLogin.SuspendLayout();
			this.SuspendLayout();
			// 
			// mainStatusBar
			// 
			this.mainStatusBar.Location = new System.Drawing.Point(0, 167);
			this.mainStatusBar.Name = "mainStatusBar";
			this.mainStatusBar.Size = new System.Drawing.Size(312, 23);
			this.mainStatusBar.TabIndex = 2;
			// 
			// LoginPanel
			// 
			this.LoginPanel.Controls.Add(this.grpLogin);
			this.LoginPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LoginPanel.Location = new System.Drawing.Point(0, 0);
			this.LoginPanel.Name = "LoginPanel";
			this.LoginPanel.Size = new System.Drawing.Size(312, 190);
			this.LoginPanel.TabIndex = 3;
			this.LoginPanel.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.LoginPanel_Keypress);
			// 
			// grpLogin
			// 
			this.grpLogin.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.grpLogin.Controls.Add(this.button1);
			this.grpLogin.Controls.Add(this.chkSaveLogin);
			this.grpLogin.Controls.Add(this.label2);
			this.grpLogin.Controls.Add(this.label1);
			this.grpLogin.Controls.Add(this.cmbLocation);
			this.grpLogin.Controls.Add(this.btnCancel);
			this.grpLogin.Controls.Add(this.txtpassword);
			this.grpLogin.Controls.Add(this.txtlastname);
			this.grpLogin.Controls.Add(this.txtfirstname);
			this.grpLogin.Controls.Add(this.btnLogin);
			this.grpLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.grpLogin.Location = new System.Drawing.Point(2, 3);
			this.grpLogin.Name = "grpLogin";
			this.grpLogin.Size = new System.Drawing.Size(307, 160);
			this.grpLogin.TabIndex = 0;
			this.grpLogin.TabStop = false;
			this.grpLogin.Text = "Sign In";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(106, 118);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(39, 34);
			this.button1.TabIndex = 9;
			this.button1.Text = "button1";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Visible = false;
			this.button1.Click += new System.EventHandler(this.Button1Click);
			// 
			// chkSaveLogin
			// 
			this.chkSaveLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.chkSaveLogin.Location = new System.Drawing.Point(10, 118);
			this.chkSaveLogin.Name = "chkSaveLogin";
			this.chkSaveLogin.Size = new System.Drawing.Size(120, 28);
			this.chkSaveLogin.TabIndex = 8;
			this.chkSaveLogin.Text = "Save Login Info";
			this.chkSaveLogin.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.label2.Location = new System.Drawing.Point(154, 59);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(138, 20);
			this.label2.TabIndex = 7;
			this.label2.Text = "Location:";
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.label1.Location = new System.Drawing.Point(9, 59);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(138, 20);
			this.label1.TabIndex = 6;
			this.label1.Text = "Password:";
			// 
			// cmbLocation
			// 
			this.cmbLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.cmbLocation.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cmbLocation.FormattingEnabled = true;
			this.cmbLocation.Items.AddRange(new object[] {
									"Home",
									"Last"});
			this.cmbLocation.Location = new System.Drawing.Point(153, 80);
			this.cmbLocation.Name = "cmbLocation";
			this.cmbLocation.Size = new System.Drawing.Size(140, 24);
			this.cmbLocation.TabIndex = 3;
			this.cmbLocation.Text = "Last";
			// 
			// btnCancel
			// 
			this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnCancel.Location = new System.Drawing.Point(226, 119);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(66, 33);
			this.btnCancel.TabIndex = 5;
			this.btnCancel.Text = "&Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancelClick);
			// 
			// txtpassword
			// 
			this.txtpassword.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.txtpassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtpassword.Location = new System.Drawing.Point(9, 80);
			this.txtpassword.Name = "txtpassword";
			this.txtpassword.PasswordChar = '*';
			this.txtpassword.Size = new System.Drawing.Size(138, 22);
			this.txtpassword.TabIndex = 2;
			// 
			// txtlastname
			// 
			this.txtlastname.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.txtlastname.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtlastname.Location = new System.Drawing.Point(154, 34);
			this.txtlastname.Name = "txtlastname";
			this.txtlastname.Size = new System.Drawing.Size(138, 22);
			this.txtlastname.TabIndex = 1;
			this.txtlastname.Text = "Last Name";
			this.txtlastname.GotFocus += new System.EventHandler(this.txtLastName_Focus);
			// 
			// txtfirstname
			// 
			this.txtfirstname.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.txtfirstname.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtfirstname.Location = new System.Drawing.Point(9, 34);
			this.txtfirstname.Name = "txtfirstname";
			this.txtfirstname.Size = new System.Drawing.Size(138, 22);
			this.txtfirstname.TabIndex = 0;
			this.txtfirstname.Text = "First Name";
			this.txtfirstname.GotFocus += new System.EventHandler(this.txtFirstName_Focus);
			// 
			// btnLogin
			// 
			this.btnLogin.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btnLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnLogin.Location = new System.Drawing.Point(153, 118);
			this.btnLogin.Name = "btnLogin";
			this.btnLogin.Size = new System.Drawing.Size(65, 34);
			this.btnLogin.TabIndex = 4;
			this.btnLogin.Text = "&Login";
			this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
			// 
			// winLogin
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
			this.ClientSize = new System.Drawing.Size(312, 190);
			this.Controls.Add(this.mainStatusBar);
			this.Controls.Add(this.LoginPanel);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
			this.MaximizeBox = false;
			this.Name = "winLogin";
			this.Text = "SLChat - Login";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.OnClosing);
			this.Load += new System.EventHandler(this.winLogin_Load);
			this.LoginPanel.ResumeLayout(false);
			this.grpLogin.ResumeLayout(false);
			this.grpLogin.PerformLayout();
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.CheckBox chkSaveLogin;
		private System.Windows.Forms.ComboBox cmbLocation;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnCancel;
		#endregion

		[STAThread]
		static void Main() 
		{
			Application.Run(new winLogin());
		}
		
		private void winLogin_Load(object sender, System.EventArgs e)
		{
			try
			{
				//Try to load the login settings.
				LoadLoginSettings();
			}
			catch (Exception error)
			{
				MessageBox.Show(this, error.ToString());
				return;
			}
		}

		private void btnLogin_Click(object sender, System.EventArgs e)
		{
			//Make sure our fields aren't blank (could possibly be
			//handled better to specify which one is blank).
			if(txtlastname.Text != "" & txtfirstname.Text != "" & txtpassword.Text != "" & cmbLocation.Text != "")
			{
				//Save our settings on the following conditions:
				//The user has checked the appropriet checkbox
				//The content has changed from last being loaded.
				if(chkSaveLogin.Checked != false)
				{
					//If the checkbox is checked
					if(chkSaveLogin.Checked != setSaveLogin | txtfirstname.Text != setFirstName | txtlastname.Text != setLastName | cmbLocation.Text != setLLocation)
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
				
				//Check if we've already created the chatscreen
				//and react approprietly.
				if(chatCreated==false){
					//create the chat screen and send ourselves
					//so the chatscreen can reference us easily.
					winChat = new ChatScreen(this);
					winChat.Show();
					chatCreated = true;
					winChat.loginVisible = false;
					this.Hide();
				}else{
					winChat.Focus();
				}
				//We call this seperately because regardless if
				//the chatscreen is created or not we still want
				//to login.
				if(winChat.loggedin==false)
				{
					string logLocation = cmbLocation.Text.ToLower();
					//Why not call Login directly from netcom?
					//Well we want winChat to have the easiest access
					//to NetCom and creating a bunch of instances
					//doesn't seem desirable, so we go through winChat.
					winChat.callLogin(txtfirstname.Text,txtlastname.Text,txtpassword.Text,logLocation);
					winChat.loginVisible = false;
					this.Hide();
				}
			}else{
				//Could probably be a dialog or something, I dunno.
				mainStatusBar.Text = "Login property blank!";
			}
		}
		
		public void OnClosing(object sender, System.EventArgs e)
		{
			//TODO: Fix this. What should it do? This should prevent
			//the whole application from closing if winChat has
			//been created and is visible. If winChat hasn't been
			//created, then the whole applicaiton should be closed.
			
			if(chatCreated==true)
			{
				winChat.loginVisible = false;
				this.Hide();
			}else{
				this.Dispose();
			}
		}
		
		private void txtFirstName_Focus(object sender, System.EventArgs e)
		{
			if(this.txtfirstname.Text == "First Name")
			{
				this.txtfirstname.Text = "";
			}
		}
		
		private void txtLastName_Focus(object sender, System.EventArgs e)
		{
			if(this.txtlastname.Text == "Last Name")
			{
				this.txtlastname.Text = "";
			}
		}
		
		void btnCancelClick(object sender, System.EventArgs e)
		{
			if(chatCreated==true)
			{
				winChat.loginVisible = false;
				this.Hide();
			}else{
				exitApp = true;
				this.Dispose();
			}
		}
		
		void LoginPanel_Keypress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			int key = (int) e.KeyChar;
			if(key == (int) Keys.Enter)
			{
				btnLogin_Click(sender, e);
				e.Handled = true;
			}
		}
		
		public void SaveLoginSettings()
		{
			//Saving our login settings
			//pick whatever filename with .xml extension
			string filename = "settings.xml";

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
			string strSettings = "<FirstName value=\""+txtfirstname.Text+"\"/>"+
									"<LastName value=\""+txtlastname.Text+"\"/>"+
									"<LoginLocation value=\""+cmbLocation.Text+"\"/>"+
				"<SaveLoginInfo value=\""+chkSaveLogin.Checked.ToString().ToLower()+"\"/>";
			loginSettings.InnerXml = strSettings;
			
			root.AppendChild(loginSettings);
				
			xmlDoc.Save(filename);
		}
		
		public void LoadLoginSettings()
		{
			//Load our login settings.
			
			string filename = "settings.xml";

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
					txtfirstname.Text = setFirstName = nodeFName[0].Attributes["value"].InnerText;
					XmlNodeList nodeLName = xmlDoc.GetElementsByTagName("LastName");
					txtlastname.Text = setLastName = nodeLName[0].Attributes["value"].InnerText;
					XmlNodeList nodeLLocation = xmlDoc.GetElementsByTagName("LoginLocation");
					cmbLocation.Text = setLLocation = nodeLLocation[0].Attributes["value"].InnerText;
					chkSaveLogin.Checked = setSaveLogin = true;
				}else{
					chkSaveLogin.Checked = setSaveLogin = false;
						
				}
			}

		}
		
		public void DeleteLoginSettings()
		{
			//Delete our login settings.
			
			string filename = "settings.xml";

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
		
		/*
		 * The below commented out code was basicly saving the login
		 * settings to a .ini (or really a .txt file), I'm leaving this
		 * incase its needed for now, although this seems unlikely.
		 * - Oz
		 * 
		public void SaveSettings()
		{
    		string path = @"settings.ini";
        	// This text is added only once to the file.
        	if (!File.Exists(path)) 
       		{
            	// Create a file to write to.
            	using (StreamWriter sw = File.CreateText(path)) 
            	{
               		sw.WriteLine("SLChat Settings");
            	}    
        	}
        	
        	//string s = txtpassword.Text;
        	//SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
        	//sha1.ComputeHash(s);

        	// This text is always added, making the file longer over time
        	// if it is not deleted.
        	using (StreamWriter sw = File.AppendText(path)) 
        	{
        		sw.WriteLine("savesettings="+chkSaveLogin.CheckState.ToString());
            	sw.WriteLine("firstname="+txtfirstname.Text);
            	sw.WriteLine("lastname="+txtlastname.Text);
            	//sw.WriteLine("password="+sha1);
            	sw.WriteLine("loginlocation="+cmbLocation.Text);
        	}    
		}
		
		public void DeleteSettings()
		{
			string path = @"settings.ini";
        	FileInfo file = new FileInfo(path);
        	if(File.Exists(path))
        	{
        		file.Delete();
        	}
		}
		
		
		public void LoadSettings()
		{
			string path = @"settings.ini";
        	// This text is added only once to the file.
        	if (File.Exists(path)) 
       		{
				// Open the file to read from.
       			using (StreamReader sr = File.OpenText(path)) 
        		{
           			string s = "";
           			while ((s = sr.ReadLine()) != null) 
           			{
           				char cher = '=';
           				string[] split = s.Split(cher);
           				if(split[0]=="savesettings")
           				{
           					if(split[1]=="Checked")
           					{
           						chkSaveLogin.Checked = true;
           					}
           				}else if(split[0]=="firstname")
           				{
           					txtfirstname.Text = split[1];
           				}else if(split[0]=="lastname"){
           					txtlastname.Text = split[1];
           				}else if(split[0]=="loginlocation"){
           					cmbLocation.Text = split[1];
           				}
            		}
        		}
        	}
		}*/
		
		//This is a hidden button used to call the IM window
		//which I'm messing around with, this was so I didn't have
		//to login to load an IM window each time. This serves no
		//actual purpose other than testing. - Oz
		void Button1Click(object sender, System.EventArgs e)
		{
			winIM IM = new winIM();
			IM.Show();
		}
	}
}
