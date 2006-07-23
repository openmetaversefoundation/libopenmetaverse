using System;
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
		public bool loggedin = false;
		private System.Windows.Forms.GroupBox grpLogin;
		private System.Windows.Forms.Button btnLogin;
		private System.Windows.Forms.TextBox txtfirstname;
		private System.Windows.Forms.TextBox txtlastname;
		private System.Windows.Forms.TextBox txtpassword;
		public bool exitApp = false;
		public string accountid;
		public bool chatCreated;
		public static ChatScreen winChat;
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
			this.mainStatusBar.Location = new System.Drawing.Point(0, 170);
			this.mainStatusBar.Name = "mainStatusBar";
			this.mainStatusBar.Size = new System.Drawing.Size(174, 23);
			this.mainStatusBar.TabIndex = 2;
			// 
			// LoginPanel
			// 
			this.LoginPanel.Controls.Add(this.grpLogin);
			this.LoginPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LoginPanel.Location = new System.Drawing.Point(0, 0);
			this.LoginPanel.Name = "LoginPanel";
			this.LoginPanel.Size = new System.Drawing.Size(174, 193);
			this.LoginPanel.TabIndex = 3;
			this.LoginPanel.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.LoginPanel_Keypress);
			// 
			// grpLogin
			// 
			this.grpLogin.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.grpLogin.Controls.Add(this.btnCancel);
			this.grpLogin.Controls.Add(this.txtpassword);
			this.grpLogin.Controls.Add(this.txtlastname);
			this.grpLogin.Controls.Add(this.txtfirstname);
			this.grpLogin.Controls.Add(this.btnLogin);
			this.grpLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.grpLogin.Location = new System.Drawing.Point(12, 4);
			this.grpLogin.Name = "grpLogin";
			this.grpLogin.Size = new System.Drawing.Size(150, 160);
			this.grpLogin.TabIndex = 0;
			this.grpLogin.TabStop = false;
			this.grpLogin.Text = "Sign In";
			// 
			// btnCancel
			// 
			this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnCancel.Location = new System.Drawing.Point(77, 121);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(66, 33);
			this.btnCancel.TabIndex = 4;
			this.btnCancel.Text = "&Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancelClick);
			// 
			// txtpassword
			// 
			this.txtpassword.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.txtpassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtpassword.Location = new System.Drawing.Point(6, 92);
			this.txtpassword.Name = "txtpassword";
			this.txtpassword.PasswordChar = '*';
			this.txtpassword.Size = new System.Drawing.Size(138, 22);
			this.txtpassword.TabIndex = 2;
			// 
			// txtlastname
			// 
			this.txtlastname.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.txtlastname.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtlastname.Location = new System.Drawing.Point(6, 64);
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
			this.txtfirstname.Location = new System.Drawing.Point(6, 36);
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
			this.btnLogin.Location = new System.Drawing.Point(6, 120);
			this.btnLogin.Name = "btnLogin";
			this.btnLogin.Size = new System.Drawing.Size(65, 34);
			this.btnLogin.TabIndex = 3;
			this.btnLogin.Text = "&Login";
			this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
			// 
			// winLogin
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
			this.ClientSize = new System.Drawing.Size(174, 193);
			this.Controls.Add(this.mainStatusBar);
			this.Controls.Add(this.LoginPanel);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
			this.MinimumSize = new System.Drawing.Size(182, 223);
			this.Name = "winLogin";
			this.Text = "SLChat - Login";
			this.Load += new System.EventHandler(this.winLogin_Load);
			//TODO: make this.Closing work. See "OnClosing"
			this.Closing += new CancelEventHandler(this.OnClosing);
			this.LoginPanel.ResumeLayout(false);
			this.grpLogin.ResumeLayout(false);
			this.grpLogin.PerformLayout();
			this.ResumeLayout(false);
		}
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
			if(txtlastname.Text != "" & txtfirstname.Text != "" & txtpassword.Text != "")
			{
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
					//Why not call Login directly from netcom?
					//Well we want winChat to have the easiest access
					//to NetCom and creating a bunch of instances
					//doesn't seem desirable, so we go through winChat.
					winChat.callLogin(txtfirstname.Text,txtlastname.Text,txtpassword.Text);
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
	}
}
