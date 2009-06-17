namespace GridImageUpload
{
    partial class frmGridImageUpload
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
            this.grpLogin = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cboLoginURL = new System.Windows.Forms.ComboBox();
            this.cmdConnect = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtLastName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFirstName = new System.Windows.Forms.TextBox();
            this.grpUpload = new System.Windows.Forms.GroupBox();
            this.txtAssetID = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblSize = new System.Windows.Forms.Label();
            this.prgUpload = new System.Windows.Forms.ProgressBar();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.cmdLoad = new System.Windows.Forms.Button();
            this.txtSendtoName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.chkLossless = new System.Windows.Forms.CheckBox();
            this.cmdUpload = new System.Windows.Forms.Button();
            this.cmdSave = new System.Windows.Forms.Button();
            this.grpLogin.SuspendLayout();
            this.grpUpload.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // grpLogin
            // 
            this.grpLogin.Controls.Add(this.label5);
            this.grpLogin.Controls.Add(this.cboLoginURL);
            this.grpLogin.Controls.Add(this.cmdConnect);
            this.grpLogin.Controls.Add(this.label3);
            this.grpLogin.Controls.Add(this.txtPassword);
            this.grpLogin.Controls.Add(this.label2);
            this.grpLogin.Controls.Add(this.txtLastName);
            this.grpLogin.Controls.Add(this.label1);
            this.grpLogin.Controls.Add(this.txtFirstName);
            this.grpLogin.Location = new System.Drawing.Point(11, 260);
            this.grpLogin.Name = "grpLogin";
            this.grpLogin.Size = new System.Drawing.Size(379, 145);
            this.grpLogin.TabIndex = 67;
            this.grpLogin.TabStop = false;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(6, 65);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(120, 16);
            this.label5.TabIndex = 74;
            this.label5.Text = "Login URL";
            // 
            // cboLoginURL
            // 
            this.cboLoginURL.FormattingEnabled = true;
            this.cboLoginURL.Location = new System.Drawing.Point(6, 84);
            this.cboLoginURL.Name = "cboLoginURL";
            this.cboLoginURL.Size = new System.Drawing.Size(365, 21);
            this.cboLoginURL.TabIndex = 3;
            // 
            // cmdConnect
            // 
            this.cmdConnect.Location = new System.Drawing.Point(251, 111);
            this.cmdConnect.Name = "cmdConnect";
            this.cmdConnect.Size = new System.Drawing.Size(120, 24);
            this.cmdConnect.TabIndex = 4;
            this.cmdConnect.Text = "Connect";
            this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(251, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(120, 16);
            this.label3.TabIndex = 72;
            this.label3.Text = "Password";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(251, 36);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(120, 20);
            this.txtPassword.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(132, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 16);
            this.label2.TabIndex = 70;
            this.label2.Text = "Last Name";
            // 
            // txtLastName
            // 
            this.txtLastName.Location = new System.Drawing.Point(132, 36);
            this.txtLastName.Name = "txtLastName";
            this.txtLastName.Size = new System.Drawing.Size(112, 20);
            this.txtLastName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(6, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 16);
            this.label1.TabIndex = 68;
            this.label1.Text = "First Name";
            // 
            // txtFirstName
            // 
            this.txtFirstName.Location = new System.Drawing.Point(6, 36);
            this.txtFirstName.Name = "txtFirstName";
            this.txtFirstName.Size = new System.Drawing.Size(120, 20);
            this.txtFirstName.TabIndex = 0;
            // 
            // grpUpload
            // 
            this.grpUpload.Controls.Add(this.cmdSave);
            this.grpUpload.Controls.Add(this.txtAssetID);
            this.grpUpload.Controls.Add(this.label4);
            this.grpUpload.Controls.Add(this.lblSize);
            this.grpUpload.Controls.Add(this.prgUpload);
            this.grpUpload.Controls.Add(this.picPreview);
            this.grpUpload.Controls.Add(this.cmdLoad);
            this.grpUpload.Controls.Add(this.txtSendtoName);
            this.grpUpload.Controls.Add(this.label6);
            this.grpUpload.Controls.Add(this.chkLossless);
            this.grpUpload.Controls.Add(this.cmdUpload);
            this.grpUpload.Location = new System.Drawing.Point(12, 12);
            this.grpUpload.Name = "grpUpload";
            this.grpUpload.Size = new System.Drawing.Size(379, 242);
            this.grpUpload.TabIndex = 68;
            this.grpUpload.TabStop = false;
            // 
            // txtAssetID
            // 
            this.txtAssetID.Location = new System.Drawing.Point(90, 204);
            this.txtAssetID.Name = "txtAssetID";
            this.txtAssetID.ReadOnly = true;
            this.txtAssetID.Size = new System.Drawing.Size(280, 20);
            this.txtAssetID.TabIndex = 10;
            this.txtAssetID.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 207);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 13);
            this.label4.TabIndex = 79;
            this.label4.Text = "Asset UUID:";
            // 
            // lblSize
            // 
            this.lblSize.AutoSize = true;
            this.lblSize.Location = new System.Drawing.Point(79, 96);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(0, 13);
            this.lblSize.TabIndex = 77;
            // 
            // prgUpload
            // 
            this.prgUpload.Location = new System.Drawing.Point(9, 175);
            this.prgUpload.Name = "prgUpload";
            this.prgUpload.Size = new System.Drawing.Size(362, 23);
            this.prgUpload.TabIndex = 76;
            // 
            // picPreview
            // 
            this.picPreview.Location = new System.Drawing.Point(9, 96);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(64, 64);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picPreview.TabIndex = 75;
            this.picPreview.TabStop = false;
            // 
            // cmdLoad
            // 
            this.cmdLoad.Location = new System.Drawing.Point(86, 136);
            this.cmdLoad.Name = "cmdLoad";
            this.cmdLoad.Size = new System.Drawing.Size(91, 24);
            this.cmdLoad.TabIndex = 7;
            this.cmdLoad.Text = "Load Texture";
            this.cmdLoad.UseVisualStyleBackColor = true;
            this.cmdLoad.Click += new System.EventHandler(this.cmdLoad_Click);
            // 
            // txtSendtoName
            // 
            this.txtSendtoName.Location = new System.Drawing.Point(131, 64);
            this.txtSendtoName.Name = "txtSendtoName";
            this.txtSendtoName.Size = new System.Drawing.Size(239, 20);
            this.txtSendtoName.TabIndex = 6;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 67);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(112, 13);
            this.label6.TabIndex = 71;
            this.label6.Text = "Send Copy To Avatar:";
            // 
            // chkLossless
            // 
            this.chkLossless.Location = new System.Drawing.Point(9, 19);
            this.chkLossless.Name = "chkLossless";
            this.chkLossless.Size = new System.Drawing.Size(362, 37);
            this.chkLossless.TabIndex = 5;
            this.chkLossless.Text = "Single Layer Lossless (only useful for pixel perfect reproductions of small image" +
                "s, such as sculpt maps)";
            this.chkLossless.UseVisualStyleBackColor = true;
            this.chkLossless.CheckedChanged += new System.EventHandler(this.chkLossless_CheckedChanged);
            // 
            // cmdUpload
            // 
            this.cmdUpload.Enabled = false;
            this.cmdUpload.Location = new System.Drawing.Point(280, 136);
            this.cmdUpload.Name = "cmdUpload";
            this.cmdUpload.Size = new System.Drawing.Size(91, 24);
            this.cmdUpload.TabIndex = 9;
            this.cmdUpload.Text = "Upload Texture";
            this.cmdUpload.UseVisualStyleBackColor = true;
            this.cmdUpload.Click += new System.EventHandler(this.cmdUpload_Click);
            // 
            // cmdSave
            // 
            this.cmdSave.Enabled = false;
            this.cmdSave.Location = new System.Drawing.Point(183, 136);
            this.cmdSave.Name = "cmdSave";
            this.cmdSave.Size = new System.Drawing.Size(91, 24);
            this.cmdSave.TabIndex = 8;
            this.cmdSave.Text = "Save Texture";
            this.cmdSave.UseVisualStyleBackColor = true;
            this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
            // 
            // frmGridImageUpload
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(402, 417);
            this.Controls.Add(this.grpUpload);
            this.Controls.Add(this.grpLogin);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(410, 400);
            this.Name = "frmGridImageUpload";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Image Upload";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmGridImageUpload_FormClosed);
            this.grpLogin.ResumeLayout(false);
            this.grpLogin.PerformLayout();
            this.grpUpload.ResumeLayout(false);
            this.grpUpload.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpLogin;
        private System.Windows.Forms.Button cmdConnect;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtLastName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFirstName;
        private System.Windows.Forms.GroupBox grpUpload;
        private System.Windows.Forms.Button cmdUpload;
        private System.Windows.Forms.CheckBox chkLossless;
        private System.Windows.Forms.TextBox txtSendtoName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.Button cmdLoad;
        private System.Windows.Forms.ProgressBar prgUpload;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.TextBox txtAssetID;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cboLoginURL;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button cmdSave;
    }
}

