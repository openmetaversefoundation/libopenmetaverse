/*
 * Created by SharpDevelop.
 * User: ${USER}
 * Date: ${DATE}
 * Time: ${TIME}
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace SLChat
{
	partial class frmPrefs : System.Windows.Forms.Form
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.lbxChoices = new System.Windows.Forms.ListBox();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnApply = new System.Windows.Forms.Button();
			this.gbxGeneral = new System.Windows.Forms.GroupBox();
			this.gbxChat = new System.Windows.Forms.GroupBox();
			this.chkListUserName = new System.Windows.Forms.CheckBox();
			this.chkYouName = new System.Windows.Forms.CheckBox();
			this.gbxProfiles = new System.Windows.Forms.GroupBox();
			this.lblProfNote = new System.Windows.Forms.Label();
			this.lblProfNoteTxt = new System.Windows.Forms.Label();
			this.btnPDelete = new System.Windows.Forms.Button();
			this.cbxProfiles = new System.Windows.Forms.ComboBox();
			this.lblProfile = new System.Windows.Forms.Label();
			this.gbxIM = new System.Windows.Forms.GroupBox();
			this.gbxTimestamps = new System.Windows.Forms.GroupBox();
			this.chkSyncStamps = new System.Windows.Forms.CheckBox();
			this.gbxIMStamps = new System.Windows.Forms.GroupBox();
			this.txtIMStampFormat = new System.Windows.Forms.TextBox();
			this.lblIMStampFormat = new System.Windows.Forms.Label();
			this.numIMTimeZ = new System.Windows.Forms.NumericUpDown();
			this.lblIMTimeZ = new System.Windows.Forms.Label();
			this.gbxChatStamps = new System.Windows.Forms.GroupBox();
			this.txtChatStampFormat = new System.Windows.Forms.TextBox();
			this.lblChatStampFormat = new System.Windows.Forms.Label();
			this.numChatTimeZ = new System.Windows.Forms.NumericUpDown();
			this.lblChatTimeZ = new System.Windows.Forms.Label();
			this.chkIMTimestamps = new System.Windows.Forms.CheckBox();
			this.chkChatTimestamps = new System.Windows.Forms.CheckBox();
			this.btnDefaults = new System.Windows.Forms.Button();
			this.gbxChat.SuspendLayout();
			this.gbxProfiles.SuspendLayout();
			this.gbxTimestamps.SuspendLayout();
			this.gbxIMStamps.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numIMTimeZ)).BeginInit();
			this.gbxChatStamps.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numChatTimeZ)).BeginInit();
			this.SuspendLayout();
			// 
			// lbxChoices
			// 
			this.lbxChoices.FormattingEnabled = true;
			this.lbxChoices.Items.AddRange(new object[] {
									"General",
									"Profiles",
									"Chat",
									"IM",
									"Timestamps"});
			this.lbxChoices.Location = new System.Drawing.Point(3, 6);
			this.lbxChoices.Name = "lbxChoices";
			this.lbxChoices.Size = new System.Drawing.Size(100, 316);
			this.lbxChoices.TabIndex = 0;
			this.lbxChoices.SelectedIndexChanged += new System.EventHandler(this.LbxChoicesSelectedIndexChanged);
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.Location = new System.Drawing.Point(250, 325);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 1;
			this.btnOk.Text = "OK";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.BtnOkClick);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(330, 325);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
			// 
			// btnApply
			// 
			this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnApply.Location = new System.Drawing.Point(410, 325);
			this.btnApply.Name = "btnApply";
			this.btnApply.Size = new System.Drawing.Size(75, 23);
			this.btnApply.TabIndex = 3;
			this.btnApply.Text = "Apply";
			this.btnApply.UseVisualStyleBackColor = true;
			this.btnApply.Click += new System.EventHandler(this.BtnApplyClick);
			// 
			// gbxGeneral
			// 
			this.gbxGeneral.Location = new System.Drawing.Point(106, 6);
			this.gbxGeneral.Name = "gbxGeneral";
			this.gbxGeneral.Size = new System.Drawing.Size(380, 315);
			this.gbxGeneral.TabIndex = 4;
			this.gbxGeneral.TabStop = false;
			this.gbxGeneral.Text = "General";
			// 
			// gbxChat
			// 
			this.gbxChat.Controls.Add(this.chkListUserName);
			this.gbxChat.Controls.Add(this.chkYouName);
			this.gbxChat.Location = new System.Drawing.Point(106, 6);
			this.gbxChat.Name = "gbxChat";
			this.gbxChat.Size = new System.Drawing.Size(380, 315);
			this.gbxChat.TabIndex = 5;
			this.gbxChat.TabStop = false;
			this.gbxChat.Text = "Chat";
			// 
			// chkListUserName
			// 
			this.chkListUserName.Location = new System.Drawing.Point(6, 46);
			this.chkListUserName.Name = "chkListUserName";
			this.chkListUserName.Size = new System.Drawing.Size(193, 24);
			this.chkListUserName.TabIndex = 2;
			this.chkListUserName.Text = "List you in local chat\'s resident list";
			this.chkListUserName.UseVisualStyleBackColor = true;
			// 
			// chkYouName
			// 
			this.chkYouName.Location = new System.Drawing.Point(6, 20);
			this.chkYouName.Name = "chkYouName";
			this.chkYouName.Size = new System.Drawing.Size(179, 24);
			this.chkYouName.TabIndex = 0;
			this.chkYouName.Text = "Use full name instead of \"You\"";
			this.chkYouName.UseVisualStyleBackColor = true;
			// 
			// gbxProfiles
			// 
			this.gbxProfiles.Controls.Add(this.lblProfNote);
			this.gbxProfiles.Controls.Add(this.lblProfNoteTxt);
			this.gbxProfiles.Controls.Add(this.btnPDelete);
			this.gbxProfiles.Controls.Add(this.cbxProfiles);
			this.gbxProfiles.Controls.Add(this.lblProfile);
			this.gbxProfiles.Location = new System.Drawing.Point(106, 6);
			this.gbxProfiles.Name = "gbxProfiles";
			this.gbxProfiles.Size = new System.Drawing.Size(380, 315);
			this.gbxProfiles.TabIndex = 5;
			this.gbxProfiles.TabStop = false;
			this.gbxProfiles.Text = "Profiles";
			// 
			// lblProfNote
			// 
			this.lblProfNote.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblProfNote.ForeColor = System.Drawing.Color.Red;
			this.lblProfNote.Location = new System.Drawing.Point(6, 25);
			this.lblProfNote.Name = "lblProfNote";
			this.lblProfNote.Size = new System.Drawing.Size(38, 14);
			this.lblProfNote.TabIndex = 4;
			this.lblProfNote.Text = "Note:";
			// 
			// lblProfNoteTxt
			// 
			this.lblProfNoteTxt.Location = new System.Drawing.Point(41, 25);
			this.lblProfNoteTxt.Name = "lblProfNoteTxt";
			this.lblProfNoteTxt.Size = new System.Drawing.Size(336, 14);
			this.lblProfNoteTxt.TabIndex = 3;
			this.lblProfNoteTxt.Text = "Changes to profiles are effective immedietly and can not be undone.";
			// 
			// btnPDelete
			// 
			this.btnPDelete.Location = new System.Drawing.Point(141, 75);
			this.btnPDelete.Name = "btnPDelete";
			this.btnPDelete.Size = new System.Drawing.Size(75, 23);
			this.btnPDelete.TabIndex = 2;
			this.btnPDelete.Text = "Delete";
			this.btnPDelete.UseVisualStyleBackColor = true;
			this.btnPDelete.Click += new System.EventHandler(this.BtnPDeleteClick);
			// 
			// cbxProfiles
			// 
			this.cbxProfiles.FormattingEnabled = true;
			this.cbxProfiles.Location = new System.Drawing.Point(50, 48);
			this.cbxProfiles.Name = "cbxProfiles";
			this.cbxProfiles.Size = new System.Drawing.Size(166, 21);
			this.cbxProfiles.TabIndex = 1;
			// 
			// lblProfile
			// 
			this.lblProfile.Location = new System.Drawing.Point(6, 51);
			this.lblProfile.Name = "lblProfile";
			this.lblProfile.Size = new System.Drawing.Size(50, 17);
			this.lblProfile.TabIndex = 0;
			this.lblProfile.Text = "Profile:";
			// 
			// gbxIM
			// 
			this.gbxIM.Location = new System.Drawing.Point(106, 6);
			this.gbxIM.Name = "gbxIM";
			this.gbxIM.Size = new System.Drawing.Size(380, 315);
			this.gbxIM.TabIndex = 6;
			this.gbxIM.TabStop = false;
			this.gbxIM.Text = "IM";
			// 
			// gbxTimestamps
			// 
			this.gbxTimestamps.Controls.Add(this.chkSyncStamps);
			this.gbxTimestamps.Controls.Add(this.gbxIMStamps);
			this.gbxTimestamps.Controls.Add(this.gbxChatStamps);
			this.gbxTimestamps.Controls.Add(this.chkIMTimestamps);
			this.gbxTimestamps.Controls.Add(this.chkChatTimestamps);
			this.gbxTimestamps.Location = new System.Drawing.Point(106, 6);
			this.gbxTimestamps.Name = "gbxTimestamps";
			this.gbxTimestamps.Size = new System.Drawing.Size(380, 315);
			this.gbxTimestamps.TabIndex = 7;
			this.gbxTimestamps.TabStop = false;
			this.gbxTimestamps.Text = "Timestamps";
			// 
			// chkSyncStamps
			// 
			this.chkSyncStamps.Location = new System.Drawing.Point(6, 132);
			this.chkSyncStamps.Name = "chkSyncStamps";
			this.chkSyncStamps.Size = new System.Drawing.Size(270, 24);
			this.chkSyncStamps.TabIndex = 2;
			this.chkSyncStamps.Text = "Sync Settings (IM matches Chat)(Not yet working)";
			this.chkSyncStamps.UseVisualStyleBackColor = true;
			// 
			// gbxIMStamps
			// 
			this.gbxIMStamps.Controls.Add(this.txtIMStampFormat);
			this.gbxIMStamps.Controls.Add(this.lblIMStampFormat);
			this.gbxIMStamps.Controls.Add(this.numIMTimeZ);
			this.gbxIMStamps.Controls.Add(this.lblIMTimeZ);
			this.gbxIMStamps.Location = new System.Drawing.Point(6, 200);
			this.gbxIMStamps.Name = "gbxIMStamps";
			this.gbxIMStamps.Size = new System.Drawing.Size(367, 68);
			this.gbxIMStamps.TabIndex = 4;
			this.gbxIMStamps.TabStop = false;
			this.gbxIMStamps.Text = "IM";
			// 
			// txtIMStampFormat
			// 
			this.txtIMStampFormat.Location = new System.Drawing.Point(115, 38);
			this.txtIMStampFormat.Name = "txtIMStampFormat";
			this.txtIMStampFormat.Size = new System.Drawing.Size(100, 21);
			this.txtIMStampFormat.TabIndex = 1;
			// 
			// lblIMStampFormat
			// 
			this.lblIMStampFormat.Location = new System.Drawing.Point(6, 41);
			this.lblIMStampFormat.Name = "lblIMStampFormat";
			this.lblIMStampFormat.Size = new System.Drawing.Size(100, 15);
			this.lblIMStampFormat.TabIndex = 3;
			this.lblIMStampFormat.Text = "Timestamp Format:";
			// 
			// numIMTimeZ
			// 
			this.numIMTimeZ.Location = new System.Drawing.Point(170, 15);
			this.numIMTimeZ.Maximum = new decimal(new int[] {
									24,
									0,
									0,
									0});
			this.numIMTimeZ.Minimum = new decimal(new int[] {
									24,
									0,
									0,
									-2147483648});
			this.numIMTimeZ.Name = "numIMTimeZ";
			this.numIMTimeZ.Size = new System.Drawing.Size(45, 21);
			this.numIMTimeZ.TabIndex = 0;
			// 
			// lblIMTimeZ
			// 
			this.lblIMTimeZ.Location = new System.Drawing.Point(6, 17);
			this.lblIMTimeZ.Name = "lblIMTimeZ";
			this.lblIMTimeZ.Size = new System.Drawing.Size(158, 15);
			this.lblIMTimeZ.TabIndex = 2;
			this.lblIMTimeZ.Text = "Time Zone Offset (UTC/GMT):";
			// 
			// gbxChatStamps
			// 
			this.gbxChatStamps.Controls.Add(this.txtChatStampFormat);
			this.gbxChatStamps.Controls.Add(this.lblChatStampFormat);
			this.gbxChatStamps.Controls.Add(this.numChatTimeZ);
			this.gbxChatStamps.Controls.Add(this.lblChatTimeZ);
			this.gbxChatStamps.Location = new System.Drawing.Point(6, 42);
			this.gbxChatStamps.Name = "gbxChatStamps";
			this.gbxChatStamps.Size = new System.Drawing.Size(367, 68);
			this.gbxChatStamps.TabIndex = 1;
			this.gbxChatStamps.TabStop = false;
			this.gbxChatStamps.Text = "Chat";
			// 
			// txtChatStampFormat
			// 
			this.txtChatStampFormat.Location = new System.Drawing.Point(115, 38);
			this.txtChatStampFormat.Name = "txtChatStampFormat";
			this.txtChatStampFormat.Size = new System.Drawing.Size(100, 21);
			this.txtChatStampFormat.TabIndex = 1;
			// 
			// lblChatStampFormat
			// 
			this.lblChatStampFormat.Location = new System.Drawing.Point(6, 41);
			this.lblChatStampFormat.Name = "lblChatStampFormat";
			this.lblChatStampFormat.Size = new System.Drawing.Size(100, 15);
			this.lblChatStampFormat.TabIndex = 3;
			this.lblChatStampFormat.Text = "Timestamp Format:";
			// 
			// numChatTimeZ
			// 
			this.numChatTimeZ.Location = new System.Drawing.Point(170, 15);
			this.numChatTimeZ.Maximum = new decimal(new int[] {
									24,
									0,
									0,
									0});
			this.numChatTimeZ.Minimum = new decimal(new int[] {
									24,
									0,
									0,
									-2147483648});
			this.numChatTimeZ.Name = "numChatTimeZ";
			this.numChatTimeZ.Size = new System.Drawing.Size(45, 21);
			this.numChatTimeZ.TabIndex = 0;
			// 
			// lblChatTimeZ
			// 
			this.lblChatTimeZ.Location = new System.Drawing.Point(6, 17);
			this.lblChatTimeZ.Name = "lblChatTimeZ";
			this.lblChatTimeZ.Size = new System.Drawing.Size(158, 15);
			this.lblChatTimeZ.TabIndex = 2;
			this.lblChatTimeZ.Text = "Time Zone Offset (UTC/GMT):";
			// 
			// chkIMTimestamps
			// 
			this.chkIMTimestamps.Location = new System.Drawing.Point(6, 172);
			this.chkIMTimestamps.Name = "chkIMTimestamps";
			this.chkIMTimestamps.Size = new System.Drawing.Size(143, 24);
			this.chkIMTimestamps.TabIndex = 3;
			this.chkIMTimestamps.Text = "Show timestamps in IMs";
			this.chkIMTimestamps.UseVisualStyleBackColor = true;
			// 
			// chkChatTimestamps
			// 
			this.chkChatTimestamps.Location = new System.Drawing.Point(6, 20);
			this.chkChatTimestamps.Name = "chkChatTimestamps";
			this.chkChatTimestamps.Size = new System.Drawing.Size(153, 24);
			this.chkChatTimestamps.TabIndex = 0;
			this.chkChatTimestamps.Text = "Show timestamps in chat";
			this.chkChatTimestamps.UseVisualStyleBackColor = true;
			// 
			// btnDefaults
			// 
			this.btnDefaults.Location = new System.Drawing.Point(3, 325);
			this.btnDefaults.Name = "btnDefaults";
			this.btnDefaults.Size = new System.Drawing.Size(100, 23);
			this.btnDefaults.TabIndex = 4;
			this.btnDefaults.Text = "Restore Defaults";
			this.btnDefaults.UseVisualStyleBackColor = true;
			this.btnDefaults.Click += new System.EventHandler(this.BtnDefaultsClick);
			// 
			// frmPrefs
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(494, 352);
			this.Controls.Add(this.gbxTimestamps);
			this.Controls.Add(this.gbxIM);
			this.Controls.Add(this.btnDefaults);
			this.Controls.Add(this.gbxChat);
			this.Controls.Add(this.gbxProfiles);
			this.Controls.Add(this.gbxGeneral);
			this.Controls.Add(this.btnApply);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.lbxChoices);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmPrefs";
			this.ShowInTaskbar = false;
			this.Text = "Preferences";
			this.gbxChat.ResumeLayout(false);
			this.gbxProfiles.ResumeLayout(false);
			this.gbxTimestamps.ResumeLayout(false);
			this.gbxIMStamps.ResumeLayout(false);
			this.gbxIMStamps.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numIMTimeZ)).EndInit();
			this.gbxChatStamps.ResumeLayout(false);
			this.gbxChatStamps.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numChatTimeZ)).EndInit();
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Button btnDefaults;
		private System.Windows.Forms.CheckBox chkSyncStamps;
		private System.Windows.Forms.Label lblChatTimeZ;
		private System.Windows.Forms.NumericUpDown numChatTimeZ;
		private System.Windows.Forms.Label lblChatStampFormat;
		private System.Windows.Forms.TextBox txtChatStampFormat;
		private System.Windows.Forms.GroupBox gbxChatStamps;
		private System.Windows.Forms.Label lblIMTimeZ;
		private System.Windows.Forms.NumericUpDown numIMTimeZ;
		private System.Windows.Forms.Label lblIMStampFormat;
		private System.Windows.Forms.TextBox txtIMStampFormat;
		private System.Windows.Forms.GroupBox gbxIMStamps;
		private System.Windows.Forms.GroupBox gbxTimestamps;
		private System.Windows.Forms.CheckBox chkListUserName;
		private System.Windows.Forms.Label lblProfNoteTxt;
		private System.Windows.Forms.Label lblProfNote;
		private System.Windows.Forms.CheckBox chkIMTimestamps;
		private System.Windows.Forms.GroupBox gbxIM;
		private System.Windows.Forms.Label lblProfile;
		private System.Windows.Forms.ComboBox cbxProfiles;
		private System.Windows.Forms.Button btnPDelete;
		private System.Windows.Forms.GroupBox gbxProfiles;
		private System.Windows.Forms.CheckBox chkYouName;
		private System.Windows.Forms.CheckBox chkChatTimestamps;
		private System.Windows.Forms.GroupBox gbxChat;
		private System.Windows.Forms.GroupBox gbxGeneral;
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.ListBox lbxChoices;
	}
}
