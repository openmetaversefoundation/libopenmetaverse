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
	/// Summary description for frmOverview.
	/// </summary>
	public class frmOverview : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ListBox lstAvatars;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		public System.Windows.Forms.ListBox lstPlugins;
		private System.Windows.Forms.GroupBox framePluginInfo;
		private System.Windows.Forms.Label lblDescription;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label lblAuthor;
		private System.Windows.Forms.LinkLabel lblHomepage;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button cmdNewInstance;
		private System.Windows.Forms.Button cmdDisconnect;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//
		private frmSecondSuite SecondSuite;

		public frmOverview(frmSecondSuite secondSuite)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			SecondSuite = secondSuite;
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
			this.lstAvatars = new System.Windows.Forms.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.lstPlugins = new System.Windows.Forms.ListBox();
			this.framePluginInfo = new System.Windows.Forms.GroupBox();
			this.label7 = new System.Windows.Forms.Label();
			this.lblHomepage = new System.Windows.Forms.LinkLabel();
			this.lblAuthor = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.lblDescription = new System.Windows.Forms.Label();
			this.cmdNewInstance = new System.Windows.Forms.Button();
			this.cmdDisconnect = new System.Windows.Forms.Button();
			this.framePluginInfo.SuspendLayout();
			this.SuspendLayout();
			// 
			// lstAvatars
			// 
			this.lstAvatars.Location = new System.Drawing.Point(16, 40);
			this.lstAvatars.Name = "lstAvatars";
			this.lstAvatars.Size = new System.Drawing.Size(184, 277);
			this.lstAvatars.TabIndex = 0;
			this.lstAvatars.SelectedIndexChanged += new System.EventHandler(this.lstAvatars_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(184, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Online Avatars";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(208, 16);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(184, 16);
			this.label2.TabIndex = 3;
			this.label2.Text = "Loaded Plugins";
			// 
			// lstPlugins
			// 
			this.lstPlugins.Location = new System.Drawing.Point(208, 40);
			this.lstPlugins.Name = "lstPlugins";
			this.lstPlugins.Size = new System.Drawing.Size(184, 277);
			this.lstPlugins.TabIndex = 2;
			this.lstPlugins.SelectedIndexChanged += new System.EventHandler(this.lstPlugins_SelectedIndexChanged);
			// 
			// framePluginInfo
			// 
			this.framePluginInfo.Controls.Add(this.label7);
			this.framePluginInfo.Controls.Add(this.lblHomepage);
			this.framePluginInfo.Controls.Add(this.lblAuthor);
			this.framePluginInfo.Controls.Add(this.label5);
			this.framePluginInfo.Controls.Add(this.label4);
			this.framePluginInfo.Controls.Add(this.lblDescription);
			this.framePluginInfo.Location = new System.Drawing.Point(400, 32);
			this.framePluginInfo.Name = "framePluginInfo";
			this.framePluginInfo.Size = new System.Drawing.Size(288, 288);
			this.framePluginInfo.TabIndex = 4;
			this.framePluginInfo.TabStop = false;
			this.framePluginInfo.Text = "Plugin Information";
			// 
			// label7
			// 
			this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label7.Location = new System.Drawing.Point(8, 120);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(88, 16);
			this.label7.TabIndex = 5;
			this.label7.Text = "Description:";
			// 
			// lblHomepage
			// 
			this.lblHomepage.Location = new System.Drawing.Point(8, 96);
			this.lblHomepage.Name = "lblHomepage";
			this.lblHomepage.Size = new System.Drawing.Size(272, 16);
			this.lblHomepage.TabIndex = 4;
			// 
			// lblAuthor
			// 
			this.lblAuthor.Location = new System.Drawing.Point(8, 48);
			this.lblAuthor.Name = "lblAuthor";
			this.lblAuthor.Size = new System.Drawing.Size(272, 16);
			this.lblAuthor.TabIndex = 3;
			// 
			// label5
			// 
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label5.Location = new System.Drawing.Point(8, 72);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(88, 16);
			this.label5.TabIndex = 2;
			this.label5.Text = "Homepage:";
			// 
			// label4
			// 
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label4.Location = new System.Drawing.Point(8, 24);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(88, 16);
			this.label4.TabIndex = 1;
			this.label4.Text = "Author:";
			// 
			// lblDescription
			// 
			this.lblDescription.Location = new System.Drawing.Point(8, 144);
			this.lblDescription.Name = "lblDescription";
			this.lblDescription.Size = new System.Drawing.Size(272, 136);
			this.lblDescription.TabIndex = 0;
			// 
			// cmdNewInstance
			// 
			this.cmdNewInstance.Enabled = false;
			this.cmdNewInstance.Location = new System.Drawing.Point(280, 328);
			this.cmdNewInstance.Name = "cmdNewInstance";
			this.cmdNewInstance.Size = new System.Drawing.Size(112, 24);
			this.cmdNewInstance.TabIndex = 5;
			this.cmdNewInstance.Text = "New Instance";
			this.cmdNewInstance.Click += new System.EventHandler(this.cmdNewInstance_Click);
			// 
			// cmdDisconnect
			// 
			this.cmdDisconnect.Enabled = false;
			this.cmdDisconnect.Location = new System.Drawing.Point(88, 328);
			this.cmdDisconnect.Name = "cmdDisconnect";
			this.cmdDisconnect.Size = new System.Drawing.Size(112, 24);
			this.cmdDisconnect.TabIndex = 6;
			this.cmdDisconnect.Text = "Disconnect";
			this.cmdDisconnect.Click += new System.EventHandler(this.cmdDisconnect_Click);
			// 
			// frmOverview
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(704, 365);
			this.ControlBox = false;
			this.Controls.Add(this.cmdDisconnect);
			this.Controls.Add(this.cmdNewInstance);
			this.Controls.Add(this.framePluginInfo);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.lstPlugins);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lstAvatars);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(712, 392);
			this.MinimumSize = new System.Drawing.Size(712, 392);
			this.Name = "frmOverview";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Overview";
			this.Load += new System.EventHandler(this.frmOverview_Load);
			this.framePluginInfo.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void frmOverview_Load(object sender, System.EventArgs e)
		{
		}

		public void ClientAdded(SecondLife client)
		{
			lstAvatars.Items.Add(client);
		}

		public void ClientRemoved(SecondLife client)
		{
			lstAvatars.Items.Remove(client);

			if (lstAvatars.SelectedIndex == -1)
			{
				cmdDisconnect.Enabled = false;
			}
		}

		private void lstPlugins_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			SSPlugin plugin = (SSPlugin)lstPlugins.Items[lstPlugins.SelectedIndex];
			lblAuthor.Text = plugin.Author;
			lblHomepage.Text = plugin.Homepage;
			lblDescription.Text = plugin.Description;

			cmdNewInstance.Enabled = true;
		}

		private void cmdNewInstance_Click(object sender, System.EventArgs e)
		{
			SSPlugin plugin = (SSPlugin)lstPlugins.Items[lstPlugins.SelectedIndex];
			
			frmLogin login = new frmLogin(SecondSuite, plugin);
			login.ShowDialog(this);
		}

		private void cmdDisconnect_Click(object sender, System.EventArgs e)
		{
			if (lstAvatars.SelectedIndex >=0)
			{
				SecondSuite.RemoveClient((SecondLife)lstAvatars.Items[lstAvatars.SelectedIndex]);
			}

			if (lstAvatars.SelectedIndex == -1)
			{
				cmdDisconnect.Enabled = false;
			}
		}

		private void lstAvatars_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			cmdDisconnect.Enabled = true;
		}
	}
}
