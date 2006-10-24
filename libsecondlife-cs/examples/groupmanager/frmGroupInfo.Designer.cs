namespace groupmanager
{
    partial class frmGroupInfo
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.picInsignia = new System.Windows.Forms.PictureBox();
            this.lblGroupName = new System.Windows.Forms.Label();
            this.lblFoundedBy = new System.Windows.Forms.Label();
            this.txtCharter = new System.Windows.Forms.TextBox();
            this.lstMembers = new System.Windows.Forms.ListView();
            this.colName = new System.Windows.Forms.ColumnHeader();
            this.colTitle = new System.Windows.Forms.ColumnHeader();
            this.colLasLogin = new System.Windows.Forms.ColumnHeader();
            this.grpPreferences = new System.Windows.Forms.GroupBox();
            this.lblMemberTitle = new System.Windows.Forms.Label();
            this.chkMature = new System.Windows.Forms.CheckBox();
            this.numFee = new System.Windows.Forms.NumericUpDown();
            this.chkGroupNotices = new System.Windows.Forms.CheckBox();
            this.chkFee = new System.Windows.Forms.CheckBox();
            this.chkOpenEnrollment = new System.Windows.Forms.CheckBox();
            this.chkPublish = new System.Windows.Forms.CheckBox();
            this.chkShow = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.picInsignia)).BeginInit();
            this.grpPreferences.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFee)).BeginInit();
            this.SuspendLayout();
            // 
            // picInsignia
            // 
            this.picInsignia.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picInsignia.Location = new System.Drawing.Point(12, 51);
            this.picInsignia.Name = "picInsignia";
            this.picInsignia.Size = new System.Drawing.Size(150, 150);
            this.picInsignia.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picInsignia.TabIndex = 0;
            this.picInsignia.TabStop = false;
            // 
            // lblGroupName
            // 
            this.lblGroupName.AutoSize = true;
            this.lblGroupName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGroupName.Location = new System.Drawing.Point(12, 8);
            this.lblGroupName.Name = "lblGroupName";
            this.lblGroupName.Size = new System.Drawing.Size(99, 17);
            this.lblGroupName.TabIndex = 1;
            this.lblGroupName.Text = "Group Name";
            // 
            // lblFoundedBy
            // 
            this.lblFoundedBy.AutoSize = true;
            this.lblFoundedBy.Location = new System.Drawing.Point(12, 31);
            this.lblFoundedBy.Name = "lblFoundedBy";
            this.lblFoundedBy.Size = new System.Drawing.Size(137, 13);
            this.lblFoundedBy.TabIndex = 2;
            this.lblFoundedBy.Text = "Founded by Group Founder";
            // 
            // txtCharter
            // 
            this.txtCharter.Location = new System.Drawing.Point(177, 51);
            this.txtCharter.Multiline = true;
            this.txtCharter.Name = "txtCharter";
            this.txtCharter.Size = new System.Drawing.Size(316, 221);
            this.txtCharter.TabIndex = 3;
            // 
            // lstMembers
            // 
            this.lstMembers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colTitle,
            this.colLasLogin});
            this.lstMembers.Location = new System.Drawing.Point(15, 296);
            this.lstMembers.Name = "lstMembers";
            this.lstMembers.Size = new System.Drawing.Size(478, 126);
            this.lstMembers.TabIndex = 6;
            this.lstMembers.UseCompatibleStateImageBehavior = false;
            this.lstMembers.View = System.Windows.Forms.View.Details;
            // 
            // colName
            // 
            this.colName.Text = "Member Name";
            this.colName.Width = 194;
            // 
            // colTitle
            // 
            this.colTitle.Text = "Title";
            this.colTitle.Width = 152;
            // 
            // colLasLogin
            // 
            this.colLasLogin.Text = "Last Login";
            this.colLasLogin.Width = 121;
            // 
            // grpPreferences
            // 
            this.grpPreferences.Controls.Add(this.lblMemberTitle);
            this.grpPreferences.Controls.Add(this.chkMature);
            this.grpPreferences.Controls.Add(this.numFee);
            this.grpPreferences.Controls.Add(this.chkGroupNotices);
            this.grpPreferences.Controls.Add(this.chkFee);
            this.grpPreferences.Controls.Add(this.chkOpenEnrollment);
            this.grpPreferences.Controls.Add(this.chkPublish);
            this.grpPreferences.Controls.Add(this.chkShow);
            this.grpPreferences.Location = new System.Drawing.Point(15, 428);
            this.grpPreferences.Name = "grpPreferences";
            this.grpPreferences.Size = new System.Drawing.Size(478, 122);
            this.grpPreferences.TabIndex = 8;
            this.grpPreferences.TabStop = false;
            this.grpPreferences.Text = "Group Preferences";
            // 
            // lblMemberTitle
            // 
            this.lblMemberTitle.AutoSize = true;
            this.lblMemberTitle.Location = new System.Drawing.Point(162, 43);
            this.lblMemberTitle.Name = "lblMemberTitle";
            this.lblMemberTitle.Size = new System.Drawing.Size(68, 13);
            this.lblMemberTitle.TabIndex = 7;
            this.lblMemberTitle.Text = "Member Title";
            // 
            // chkMature
            // 
            this.chkMature.AutoSize = true;
            this.chkMature.Location = new System.Drawing.Point(162, 19);
            this.chkMature.Name = "chkMature";
            this.chkMature.Size = new System.Drawing.Size(95, 17);
            this.chkMature.TabIndex = 6;
            this.chkMature.Text = "Mature publish";
            this.chkMature.UseVisualStyleBackColor = true;
            // 
            // numFee
            // 
            this.numFee.Location = new System.Drawing.Point(162, 87);
            this.numFee.Name = "numFee";
            this.numFee.Size = new System.Drawing.Size(144, 20);
            this.numFee.TabIndex = 5;
            // 
            // chkGroupNotices
            // 
            this.chkGroupNotices.AutoSize = true;
            this.chkGroupNotices.Location = new System.Drawing.Point(312, 87);
            this.chkGroupNotices.Name = "chkGroupNotices";
            this.chkGroupNotices.Size = new System.Drawing.Size(137, 17);
            this.chkGroupNotices.TabIndex = 4;
            this.chkGroupNotices.Text = "Receive Group Notices";
            this.chkGroupNotices.UseVisualStyleBackColor = true;
            // 
            // chkFee
            // 
            this.chkFee.AutoSize = true;
            this.chkFee.Location = new System.Drawing.Point(36, 88);
            this.chkFee.Name = "chkFee";
            this.chkFee.Size = new System.Drawing.Size(114, 17);
            this.chkFee.TabIndex = 3;
            this.chkFee.Text = "Enrollment Fee: L$";
            this.chkFee.UseVisualStyleBackColor = true;
            // 
            // chkOpenEnrollment
            // 
            this.chkOpenEnrollment.AutoSize = true;
            this.chkOpenEnrollment.Location = new System.Drawing.Point(16, 65);
            this.chkOpenEnrollment.Name = "chkOpenEnrollment";
            this.chkOpenEnrollment.Size = new System.Drawing.Size(104, 17);
            this.chkOpenEnrollment.TabIndex = 2;
            this.chkOpenEnrollment.Text = "Open Enrollment";
            this.chkOpenEnrollment.UseVisualStyleBackColor = true;
            // 
            // chkPublish
            // 
            this.chkPublish.AutoSize = true;
            this.chkPublish.Location = new System.Drawing.Point(16, 42);
            this.chkPublish.Name = "chkPublish";
            this.chkPublish.Size = new System.Drawing.Size(116, 17);
            this.chkPublish.TabIndex = 1;
            this.chkPublish.Text = "Publish on the web";
            this.chkPublish.UseVisualStyleBackColor = true;
            // 
            // chkShow
            // 
            this.chkShow.AutoSize = true;
            this.chkShow.Location = new System.Drawing.Point(16, 19);
            this.chkShow.Name = "chkShow";
            this.chkShow.Size = new System.Drawing.Size(116, 17);
            this.chkShow.TabIndex = 0;
            this.chkShow.Text = "Show In Group List";
            this.chkShow.UseVisualStyleBackColor = true;
            // 
            // frmGroupInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(505, 562);
            this.Controls.Add(this.grpPreferences);
            this.Controls.Add(this.lstMembers);
            this.Controls.Add(this.txtCharter);
            this.Controls.Add(this.lblFoundedBy);
            this.Controls.Add(this.lblGroupName);
            this.Controls.Add(this.picInsignia);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(513, 548);
            this.Name = "frmGroupInfo";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Group Information";
            this.Shown += new System.EventHandler(this.frmGroupInfo_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.picInsignia)).EndInit();
            this.grpPreferences.ResumeLayout(false);
            this.grpPreferences.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFee)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picInsignia;
        private System.Windows.Forms.Label lblGroupName;
        private System.Windows.Forms.Label lblFoundedBy;
        private System.Windows.Forms.TextBox txtCharter;
        private System.Windows.Forms.ListView lstMembers;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colTitle;
        private System.Windows.Forms.ColumnHeader colLasLogin;
        private System.Windows.Forms.GroupBox grpPreferences;
        private System.Windows.Forms.CheckBox chkPublish;
        private System.Windows.Forms.CheckBox chkShow;
        private System.Windows.Forms.NumericUpDown numFee;
        private System.Windows.Forms.CheckBox chkGroupNotices;
        private System.Windows.Forms.CheckBox chkFee;
        private System.Windows.Forms.CheckBox chkOpenEnrollment;
        private System.Windows.Forms.CheckBox chkMature;
        private System.Windows.Forms.Label lblMemberTitle;
    }
}