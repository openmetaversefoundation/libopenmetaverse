using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using libsecondlife;

namespace SecondSuite.Plugins
{
	/// <summary>
	/// Summary description for frmAccountant.
	/// </summary>
	public class frmAccountant : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ListView lstFind;
		private System.Windows.Forms.ColumnHeader colName;
		private System.Windows.Forms.ColumnHeader colOnline;
		private System.Windows.Forms.ColumnHeader colUuid;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button cmdTransfer;
		private System.Windows.Forms.TextBox txtTransfer;
		private System.Windows.Forms.TextBox txtFind;
		private System.Windows.Forms.Button cmdFind;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label lblBalance;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label lblName;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtDescription;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		// libsecondlife instance
		private SecondLife Client;
		// Mutex for locking the listview
		Mutex lstFindMutex;

		public frmAccountant(SecondLife client)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Client = client;
			lstFindMutex = new Mutex(false, "lstFindMutex");

			// Install our packet handlers
			Client.Network.RegisterCallback("MoneyBalanceReply", new PacketCallback(BalanceHandler));
			Client.Network.RegisterCallback("DirPeopleReply", new PacketCallback(DirPeopleHandler));
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

		public void Connected()
		{
			lblName.Text = Client.Network.LoginValues["first_name"] + " " + 
				Client.Network.LoginValues["last_name"];

			// MoneyBalanceRequest
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();
			blocks = new Hashtable();
			fields = new Hashtable();
			fields["AgentID"] = Client.Network.AgentID;
			fields["TransactionID"] = LLUUID.GenerateUUID();
			blocks[fields] = "MoneyData";
			Packet packet = PacketBuilder.BuildPacket("MoneyBalanceRequest", Client.Protocol, blocks,
				Helpers.MSG_RELIABLE);

			Client.Network.SendPacket(packet);
		}

		private void BalanceHandler(Packet packet, Circuit circuit)
		{
			if (packet.Layout.Name == "MoneyBalanceReply")
			{
				int balance = 0;
				int squareMetersCredit = 0;
				string description = "";
				LLUUID transactionID = null;
				bool transactionSuccess = false;

				foreach (Block block in packet.Blocks())
				{
					foreach (Field field in block.Fields)
					{
						if (field.Layout.Name == "MoneyBalance")
						{
							balance = (int)field.Data;
						}
						else if (field.Layout.Name == "SquareMetersCredit")
						{
							squareMetersCredit = (int)field.Data;
						}
						else if (field.Layout.Name == "Description")
						{
							byte[] byteArray = (byte[])field.Data;
							description = System.Text.Encoding.ASCII.GetString(byteArray).Replace("\0", "");
						}
						else if (field.Layout.Name == "TransactionID")
						{
							transactionID = (LLUUID)field.Data;
						}
						else if (field.Layout.Name == "TransactionSuccess")
						{
							transactionSuccess = (bool)field.Data;
						}
					}
				}

				lblBalance.Text = balance.ToString();
			}
		}

		private void DirPeopleHandler(Packet packet, Circuit circuit)
		{
			lstFindMutex.WaitOne();

			foreach (Block block in packet.Blocks())
			{
				if (block.Layout.Name == "QueryReplies")
				{
					LLUUID id = null;
					string firstName = "";
					string lastName = "";
					bool online = false;
					
					foreach (Field field in block.Fields)
					{
						if (field.Layout.Name == "AgentID")
						{
							id = (LLUUID)field.Data;
						}
						else if (field.Layout.Name == "LastName")
						{
							lastName = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
						}
						else if (field.Layout.Name == "FirstName")
						{
							firstName = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
						}
						else if (field.Layout.Name == "Online")
						{
							online = (bool)field.Data;
						}
					}

					if (id != null)
					{
						ListViewItem listItem = new ListViewItem(new string[] 
						{ firstName + " " + lastName, (online ? "Yes" : "No"), id.ToString() });
						lstFind.Items.Add(listItem);
					}
				}
			}

			lstFindMutex.ReleaseMutex();
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lstFind = new System.Windows.Forms.ListView();
			this.colName = new System.Windows.Forms.ColumnHeader();
			this.colOnline = new System.Windows.Forms.ColumnHeader();
			this.colUuid = new System.Windows.Forms.ColumnHeader();
			this.label7 = new System.Windows.Forms.Label();
			this.cmdTransfer = new System.Windows.Forms.Button();
			this.txtTransfer = new System.Windows.Forms.TextBox();
			this.txtFind = new System.Windows.Forms.TextBox();
			this.cmdFind = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.lblBalance = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.lblName = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.txtDescription = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// lstFind
			// 
			this.lstFind.Activation = System.Windows.Forms.ItemActivation.OneClick;
			this.lstFind.AllowColumnReorder = true;
			this.lstFind.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					  this.colName,
																					  this.colOnline,
																					  this.colUuid});
			this.lstFind.FullRowSelect = true;
			this.lstFind.HideSelection = false;
			this.lstFind.Location = new System.Drawing.Point(16, 88);
			this.lstFind.Name = "lstFind";
			this.lstFind.Size = new System.Drawing.Size(336, 248);
			this.lstFind.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.lstFind.TabIndex = 53;
			this.lstFind.View = System.Windows.Forms.View.Details;
			// 
			// colName
			// 
			this.colName.Text = "Name";
			this.colName.Width = 120;
			// 
			// colOnline
			// 
			this.colOnline.Text = "Online";
			this.colOnline.Width = 50;
			// 
			// colUuid
			// 
			this.colUuid.Text = "UUID";
			this.colUuid.Width = 150;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(360, 152);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(88, 16);
			this.label7.TabIndex = 56;
			this.label7.Text = "Amount:";
			// 
			// cmdTransfer
			// 
			this.cmdTransfer.Location = new System.Drawing.Point(472, 248);
			this.cmdTransfer.Name = "cmdTransfer";
			this.cmdTransfer.Size = new System.Drawing.Size(104, 24);
			this.cmdTransfer.TabIndex = 55;
			this.cmdTransfer.Text = "Transfer Lindens";
			this.cmdTransfer.Click += new System.EventHandler(this.cmdTransfer_Click);
			// 
			// txtTransfer
			// 
			this.txtTransfer.Location = new System.Drawing.Point(360, 168);
			this.txtTransfer.MaxLength = 7;
			this.txtTransfer.Name = "txtTransfer";
			this.txtTransfer.Size = new System.Drawing.Size(104, 20);
			this.txtTransfer.TabIndex = 54;
			this.txtTransfer.Text = "";
			// 
			// txtFind
			// 
			this.txtFind.Location = new System.Drawing.Point(16, 56);
			this.txtFind.Name = "txtFind";
			this.txtFind.Size = new System.Drawing.Size(184, 20);
			this.txtFind.TabIndex = 51;
			this.txtFind.Text = "";
			// 
			// cmdFind
			// 
			this.cmdFind.Location = new System.Drawing.Point(208, 56);
			this.cmdFind.Name = "cmdFind";
			this.cmdFind.Size = new System.Drawing.Size(48, 24);
			this.cmdFind.TabIndex = 52;
			this.cmdFind.Text = "Find";
			this.cmdFind.Click += new System.EventHandler(this.cmdFind_Click);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 40);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(88, 16);
			this.label5.TabIndex = 60;
			this.label5.Text = "People Search";
			// 
			// lblBalance
			// 
			this.lblBalance.Location = new System.Drawing.Point(512, 8);
			this.lblBalance.Name = "lblBalance";
			this.lblBalance.Size = new System.Drawing.Size(64, 16);
			this.lblBalance.TabIndex = 61;
			this.lblBalance.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(456, 8);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(56, 16);
			this.label6.TabIndex = 59;
			this.label6.Text = "Balance:";
			// 
			// lblName
			// 
			this.lblName.Location = new System.Drawing.Point(64, 8);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(184, 16);
			this.lblName.TabIndex = 57;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 8);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(48, 16);
			this.label4.TabIndex = 58;
			this.label4.Text = "Name:";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(360, 200);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(168, 16);
			this.label1.TabIndex = 63;
			this.label1.Text = "Description (optional):";
			// 
			// txtDescription
			// 
			this.txtDescription.Location = new System.Drawing.Point(360, 216);
			this.txtDescription.MaxLength = 7;
			this.txtDescription.Name = "txtDescription";
			this.txtDescription.Size = new System.Drawing.Size(216, 20);
			this.txtDescription.TabIndex = 62;
			this.txtDescription.Text = "";
			// 
			// frmAccountant
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(592, 349);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtDescription);
			this.Controls.Add(this.txtTransfer);
			this.Controls.Add(this.txtFind);
			this.Controls.Add(this.lstFind);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.cmdTransfer);
			this.Controls.Add(this.cmdFind);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.lblBalance);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.lblName);
			this.Controls.Add(this.label4);
			this.Name = "frmAccountant";
			this.Text = "Accountant";
			this.ResumeLayout(false);

		}
		#endregion

		private void cmdFind_Click(object sender, System.EventArgs e)
		{
			lstFind.Items.Clear();

			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();
			fields["QueryID"] = LLUUID.GenerateUUID();
			fields["QueryFlags"] = (uint)1;
			fields["QueryStart"] = (int)0;
			fields["QueryText"] = txtFind.Text;
			blocks[fields] = "QueryData";

			fields = new Hashtable();
			fields["AgentID"] = Client.Network.AgentID;
			fields["SessionID"] = Client.Network.SessionID;
			blocks[fields] = "AgentData";

			Packet packet = PacketBuilder.BuildPacket("DirFindQuery", Client.Protocol, blocks,
				Helpers.MSG_RELIABLE);

			Client.Network.SendPacket(packet);
		}

		private void cmdTransfer_Click(object sender, System.EventArgs e)
		{
			int amount = 0;
			
			try
			{
				amount = System.Convert.ToInt32(txtTransfer.Text);
			}
			catch (Exception)
			{
				MessageBox.Show(txtTransfer.Text + " is not a valid amount");
				return;
			}
			
			if (lstFind.SelectedItems.Count != 1)
			{
				MessageBox.Show("Find an avatar using the directory search and select " + 
					"their name to transfer money");
				return;
			}
			
			Client.Avatar.GiveMoney(new LLUUID(lstFind.SelectedItems[0].SubItems[2].Text),
				amount, "SLAccountant payment");
		}
	}
}
