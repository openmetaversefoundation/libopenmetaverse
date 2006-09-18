using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using SecondSuite.Plugins;
using libsecondlife;

namespace SecondSuite
{
	/// <summary>
	/// Summary description for frmLogin.
	/// </summary>
	public class frmLogin : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox grpLogin;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.TextBox txtLastName;
		private System.Windows.Forms.Button cmdConnect;
		private System.Windows.Forms.TextBox txtFirstName;
		public System.Windows.Forms.ListBox lstAvatars;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button cmdSelect;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//
		private frmSecondSuite SecondSuite;
		private SSPlugin Plugin;

		public frmLogin(frmSecondSuite secondSuite, SSPlugin plugin)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			SecondSuite = secondSuite;
			Plugin = plugin;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
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
			this.grpLogin = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.txtLastName = new System.Windows.Forms.TextBox();
			this.cmdConnect = new System.Windows.Forms.Button();
			this.txtFirstName = new System.Windows.Forms.TextBox();
			this.lstAvatars = new System.Windows.Forms.ListBox();
			this.label4 = new System.Windows.Forms.Label();
			this.cmdSelect = new System.Windows.Forms.Button();
			this.grpLogin.SuspendLayout();
			this.SuspendLayout();
			// 
			// grpLogin
			// 
			this.grpLogin.Controls.Add(this.label3);
			this.grpLogin.Controls.Add(this.label2);
			this.grpLogin.Controls.Add(this.label1);
			this.grpLogin.Controls.Add(this.txtPassword);
			this.grpLogin.Controls.Add(this.txtLastName);
			this.grpLogin.Controls.Add(this.cmdConnect);
			this.grpLogin.Controls.Add(this.txtFirstName);
			this.grpLogin.Location = new System.Drawing.Point(16, 304);
			this.grpLogin.Name = "grpLogin";
			this.grpLogin.Size = new System.Drawing.Size(552, 80);
			this.grpLogin.TabIndex = 51;
			this.grpLogin.TabStop = false;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(280, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(120, 16);
			this.label3.TabIndex = 50;
			this.label3.Text = "Password";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(152, 24);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(120, 16);
			this.label2.TabIndex = 50;
			this.label2.Text = "Last Name";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(120, 16);
			this.label1.TabIndex = 50;
			this.label1.Text = "First Name";
			// 
			// txtPassword
			// 
			this.txtPassword.Location = new System.Drawing.Point(280, 40);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.PasswordChar = '*';
			this.txtPassword.Size = new System.Drawing.Size(120, 20);
			this.txtPassword.TabIndex = 2;
			this.txtPassword.Text = "";
			// 
			// txtLastName
			// 
			this.txtLastName.Location = new System.Drawing.Point(152, 40);
			this.txtLastName.Name = "txtLastName";
			this.txtLastName.Size = new System.Drawing.Size(112, 20);
			this.txtLastName.TabIndex = 1;
			this.txtLastName.Text = "";
			// 
			// cmdConnect
			// 
			this.cmdConnect.Location = new System.Drawing.Point(416, 40);
			this.cmdConnect.Name = "cmdConnect";
			this.cmdConnect.Size = new System.Drawing.Size(120, 24);
			this.cmdConnect.TabIndex = 3;
			this.cmdConnect.Text = "Connect";
			this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
			// 
			// txtFirstName
			// 
			this.txtFirstName.Location = new System.Drawing.Point(16, 40);
			this.txtFirstName.Name = "txtFirstName";
			this.txtFirstName.Size = new System.Drawing.Size(120, 20);
			this.txtFirstName.TabIndex = 0;
			this.txtFirstName.Text = "";
			// 
			// lstAvatars
			// 
			this.lstAvatars.Location = new System.Drawing.Point(16, 40);
			this.lstAvatars.Name = "lstAvatars";
			this.lstAvatars.Size = new System.Drawing.Size(216, 225);
			this.lstAvatars.TabIndex = 52;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 16);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(216, 16);
			this.label4.TabIndex = 53;
			this.label4.Text = "Logged in Avatars:";
			// 
			// cmdSelect
			// 
			this.cmdSelect.Enabled = false;
			this.cmdSelect.Location = new System.Drawing.Point(112, 272);
			this.cmdSelect.Name = "cmdSelect";
			this.cmdSelect.Size = new System.Drawing.Size(120, 24);
			this.cmdSelect.TabIndex = 54;
			this.cmdSelect.Text = "Select";
			this.cmdSelect.Click += new System.EventHandler(this.cmdSelect_Click);
			// 
			// frmLogin
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(584, 397);
			this.Controls.Add(this.cmdSelect);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.lstAvatars);
			this.Controls.Add(this.grpLogin);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(592, 424);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(592, 424);
			this.Name = "frmLogin";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "New Plugin Connection";
			this.grpLogin.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void cmdConnect_Click(object sender, System.EventArgs e)
		{
			SecondLife client = null;

			cmdConnect.Enabled = false;
			cmdSelect.Enabled = false;

			try
			{
				client = new SecondLife("keywords.txt", "message_template.msg");
			}
			catch (Exception error)
			{
				MessageBox.Show(this, error.ToString());
				this.Close();
			}

			// Initialize the plugin, to allow it to register callbacks and get ready
			Plugin.Init(client);

			Hashtable loginParams = NetworkManager.DefaultLoginValues(txtFirstName.Text, 
				txtLastName.Text, txtPassword.Text, "00:00:00:00:00:00", "last", 1, 10, 10, 0, 
				"Win", "0", "accountant", "jhurliman@wsu.edu");

			if (client.Network.Login(loginParams))
			{
				// Register this logged in avatar
				SecondSuite.AddClient(client);

				// Show the plugin form
				Form form = Plugin.Load();
				form.MdiParent = SecondSuite;
				form.Show();

				Plugin.ConnectionHandler();

				// Exit this form
				this.Close();
			}
			else
			{
				// Show an error
				MessageBox.Show(this, "Error logging in: " + client.Network.LoginError);
				cmdConnect.Enabled = true;
				cmdSelect.Enabled = true;
			}
		}

		private void cmdSelect_Click(object sender, System.EventArgs e)
		{
			cmdConnect.Enabled = false;
			cmdSelect.Enabled = false;

			// FIXME
		}
	}
}
