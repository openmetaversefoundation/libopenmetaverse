namespace SLChat
{
    partial class frmLogin
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
        	this.label1 = new System.Windows.Forms.Label();
        	this.txtFirstName = new System.Windows.Forms.TextBox();
        	this.txtLastName = new System.Windows.Forms.TextBox();
        	this.label2 = new System.Windows.Forms.Label();
        	this.txtPassword = new System.Windows.Forms.TextBox();
        	this.label3 = new System.Windows.Forms.Label();
        	this.cbxLocation = new System.Windows.Forms.ComboBox();
        	this.btnCancel = new System.Windows.Forms.Button();
        	this.btnLogin = new System.Windows.Forms.Button();
        	this.rtbStatus = new System.Windows.Forms.RichTextBox();
        	this.chkSaveLogin = new System.Windows.Forms.CheckBox();
        	this.cbxProfiles = new System.Windows.Forms.ComboBox();
        	this.lblProfiles = new System.Windows.Forms.Label();
        	this.SuspendLayout();
        	// 
        	// label1
        	// 
        	this.label1.AutoSize = true;
        	this.label1.Location = new System.Drawing.Point(11, 46);
        	this.label1.Name = "label1";
        	this.label1.Size = new System.Drawing.Size(96, 13);
        	this.label1.TabIndex = 0;
        	this.label1.Text = "First && Last Names";
        	// 
        	// txtFirstName
        	// 
        	this.txtFirstName.Location = new System.Drawing.Point(11, 62);
        	this.txtFirstName.Name = "txtFirstName";
        	this.txtFirstName.Size = new System.Drawing.Size(130, 21);
        	this.txtFirstName.TabIndex = 1;
        	// 
        	// txtLastName
        	// 
        	this.txtLastName.Location = new System.Drawing.Point(147, 62);
        	this.txtLastName.Name = "txtLastName";
        	this.txtLastName.Size = new System.Drawing.Size(130, 21);
        	this.txtLastName.TabIndex = 2;
        	// 
        	// label2
        	// 
        	this.label2.AutoSize = true;
        	this.label2.Location = new System.Drawing.Point(11, 86);
        	this.label2.Name = "label2";
        	this.label2.Size = new System.Drawing.Size(53, 13);
        	this.label2.TabIndex = 3;
        	this.label2.Text = "Password";
        	// 
        	// txtPassword
        	// 
        	this.txtPassword.Location = new System.Drawing.Point(11, 103);
        	this.txtPassword.Name = "txtPassword";
        	this.txtPassword.Size = new System.Drawing.Size(130, 21);
        	this.txtPassword.TabIndex = 4;
        	this.txtPassword.UseSystemPasswordChar = true;
        	// 
        	// label3
        	// 
        	this.label3.AutoSize = true;
        	this.label3.Location = new System.Drawing.Point(147, 86);
        	this.label3.Name = "label3";
        	this.label3.Size = new System.Drawing.Size(47, 13);
        	this.label3.TabIndex = 5;
        	this.label3.Text = "Location";
        	// 
        	// cbxLocation
        	// 
        	this.cbxLocation.FormattingEnabled = true;
        	this.cbxLocation.Items.AddRange(new object[] {
        	        	        	"Home",
        	        	        	"Last"});
        	this.cbxLocation.Location = new System.Drawing.Point(147, 103);
        	this.cbxLocation.Name = "cbxLocation";
        	this.cbxLocation.Size = new System.Drawing.Size(130, 21);
        	this.cbxLocation.TabIndex = 6;
        	// 
        	// btnCancel
        	// 
        	this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        	this.btnCancel.Location = new System.Drawing.Point(202, 130);
        	this.btnCancel.Name = "btnCancel";
        	this.btnCancel.Size = new System.Drawing.Size(75, 23);
        	this.btnCancel.TabIndex = 7;
        	this.btnCancel.Text = "Cancel";
        	this.btnCancel.UseVisualStyleBackColor = true;
        	this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        	// 
        	// btnLogin
        	// 
        	this.btnLogin.Location = new System.Drawing.Point(119, 130);
        	this.btnLogin.Name = "btnLogin";
        	this.btnLogin.Size = new System.Drawing.Size(75, 23);
        	this.btnLogin.TabIndex = 8;
        	this.btnLogin.Text = "Login";
        	this.btnLogin.UseVisualStyleBackColor = true;
        	this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
        	// 
        	// rtbStatus
        	// 
        	this.rtbStatus.Location = new System.Drawing.Point(11, 168);
        	this.rtbStatus.Name = "rtbStatus";
        	this.rtbStatus.ReadOnly = true;
        	this.rtbStatus.Size = new System.Drawing.Size(266, 42);
        	this.rtbStatus.TabIndex = 9;
        	this.rtbStatus.Text = "Status: Ready";
        	// 
        	// chkSaveLogin
        	// 
        	this.chkSaveLogin.Location = new System.Drawing.Point(11, 130);
        	this.chkSaveLogin.Name = "chkSaveLogin";
        	this.chkSaveLogin.Size = new System.Drawing.Size(102, 25);
        	this.chkSaveLogin.TabIndex = 10;
        	this.chkSaveLogin.Text = "Save Login Info";
        	this.chkSaveLogin.UseVisualStyleBackColor = true;
        	// 
        	// cbxProfiles
        	// 
        	this.cbxProfiles.FormattingEnabled = true;
        	this.cbxProfiles.Location = new System.Drawing.Point(11, 22);
        	this.cbxProfiles.Name = "cbxProfiles";
        	this.cbxProfiles.Size = new System.Drawing.Size(182, 21);
        	this.cbxProfiles.TabIndex = 12;
        	this.cbxProfiles.SelectedIndexChanged += new System.EventHandler(this.CbxProfilesSelectedIndexChanged);
        	// 
        	// lblProfiles
        	// 
        	this.lblProfiles.Location = new System.Drawing.Point(11, 6);
        	this.lblProfiles.Name = "lblProfiles";
        	this.lblProfiles.Size = new System.Drawing.Size(67, 13);
        	this.lblProfiles.TabIndex = 13;
        	this.lblProfiles.Text = "Profiles";
        	// 
        	// frmLogin
        	// 
        	this.Closing += new System.ComponentModel.CancelEventHandler(this.OnClosing);
        	this.AcceptButton = this.btnLogin;
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.CancelButton = this.btnCancel;
        	this.ClientSize = new System.Drawing.Size(294, 222);
        	this.Controls.Add(this.lblProfiles);
        	this.Controls.Add(this.cbxProfiles);
        	this.Controls.Add(this.chkSaveLogin);
        	this.Controls.Add(this.rtbStatus);
        	this.Controls.Add(this.btnLogin);
        	this.Controls.Add(this.btnCancel);
        	this.Controls.Add(this.cbxLocation);
        	this.Controls.Add(this.label3);
        	this.Controls.Add(this.txtPassword);
        	this.Controls.Add(this.label2);
        	this.Controls.Add(this.txtLastName);
        	this.Controls.Add(this.txtFirstName);
        	this.Controls.Add(this.label1);
        	this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        	this.MaximizeBox = false;
        	this.Name = "frmLogin";
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        	this.Text = "Login";
        	this.Shown += new System.EventHandler(this.frmLogin_Shown);
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }
        private System.Windows.Forms.Label lblProfiles;
        private System.Windows.Forms.ComboBox cbxProfiles;
        private System.Windows.Forms.CheckBox chkSaveLogin;
        private System.Windows.Forms.RichTextBox rtbStatus;

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFirstName;
        private System.Windows.Forms.TextBox txtLastName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbxLocation;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnLogin;
    }
}
