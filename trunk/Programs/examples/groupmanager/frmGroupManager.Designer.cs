namespace groupmanager
{
    partial class frmGroupManager
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
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.lstGroups = new System.Windows.Forms.ListBox();
            this.grpLogin = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtLastName = new System.Windows.Forms.TextBox();
            this.cmdConnect = new System.Windows.Forms.Button();
            this.txtFirstName = new System.Windows.Forms.TextBox();
            this.cmdLeave = new System.Windows.Forms.Button();
            this.cmdCreate = new System.Windows.Forms.Button();
            this.cmdActivate = new System.Windows.Forms.Button();
            this.cmdInfo = new System.Windows.Forms.Button();
            this.groupBox.SuspendLayout();
            this.grpLogin.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox
            // 
            this.groupBox.Controls.Add(this.cmdInfo);
            this.groupBox.Controls.Add(this.cmdActivate);
            this.groupBox.Controls.Add(this.cmdCreate);
            this.groupBox.Controls.Add(this.cmdLeave);
            this.groupBox.Controls.Add(this.lstGroups);
            this.groupBox.Enabled = false;
            this.groupBox.Location = new System.Drawing.Point(12, 12);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(419, 214);
            this.groupBox.TabIndex = 0;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "Groups";
            // 
            // lstGroups
            // 
            this.lstGroups.FormattingEnabled = true;
            this.lstGroups.Location = new System.Drawing.Point(19, 31);
            this.lstGroups.Name = "lstGroups";
            this.lstGroups.Size = new System.Drawing.Size(384, 134);
            this.lstGroups.TabIndex = 0;
            this.lstGroups.SelectedIndexChanged += new System.EventHandler(this.lstGroups_SelectedIndexChanged);
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
            this.grpLogin.Location = new System.Drawing.Point(12, 232);
            this.grpLogin.Name = "grpLogin";
            this.grpLogin.Size = new System.Drawing.Size(419, 108);
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
            // 
            // txtLastName
            // 
            this.txtLastName.Location = new System.Drawing.Point(152, 40);
            this.txtLastName.Name = "txtLastName";
            this.txtLastName.Size = new System.Drawing.Size(112, 20);
            this.txtLastName.TabIndex = 1;
            // 
            // cmdConnect
            // 
            this.cmdConnect.Location = new System.Drawing.Point(280, 66);
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
            // 
            // cmdLeave
            // 
            this.cmdLeave.Enabled = false;
            this.cmdLeave.Location = new System.Drawing.Point(313, 174);
            this.cmdLeave.Name = "cmdLeave";
            this.cmdLeave.Size = new System.Drawing.Size(90, 23);
            this.cmdLeave.TabIndex = 7;
            this.cmdLeave.Text = "Leave";
            this.cmdLeave.UseVisualStyleBackColor = true;
            // 
            // cmdCreate
            // 
            this.cmdCreate.Location = new System.Drawing.Point(19, 174);
            this.cmdCreate.Name = "cmdCreate";
            this.cmdCreate.Size = new System.Drawing.Size(90, 23);
            this.cmdCreate.TabIndex = 8;
            this.cmdCreate.Text = "Create";
            this.cmdCreate.UseVisualStyleBackColor = true;
            // 
            // cmdActivate
            // 
            this.cmdActivate.Enabled = false;
            this.cmdActivate.Location = new System.Drawing.Point(116, 174);
            this.cmdActivate.Name = "cmdActivate";
            this.cmdActivate.Size = new System.Drawing.Size(90, 23);
            this.cmdActivate.TabIndex = 9;
            this.cmdActivate.Text = "Activate";
            this.cmdActivate.UseVisualStyleBackColor = true;
            // 
            // cmdInfo
            // 
            this.cmdInfo.Enabled = false;
            this.cmdInfo.Location = new System.Drawing.Point(216, 174);
            this.cmdInfo.Name = "cmdInfo";
            this.cmdInfo.Size = new System.Drawing.Size(90, 23);
            this.cmdInfo.TabIndex = 10;
            this.cmdInfo.Text = "Info";
            this.cmdInfo.UseVisualStyleBackColor = true;
            this.cmdInfo.Click += new System.EventHandler(this.cmdInfo_Click);
            // 
            // frmGroupManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(445, 351);
            this.Controls.Add(this.grpLogin);
            this.Controls.Add(this.groupBox);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(453, 378);
            this.MinimumSize = new System.Drawing.Size(453, 378);
            this.Name = "frmGroupManager";
            this.Text = "Group Manager";
            //this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmGroupManager_FormClosing);
            this.groupBox.ResumeLayout(false);
            this.grpLogin.ResumeLayout(false);
            this.grpLogin.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.ListBox lstGroups;
        private System.Windows.Forms.GroupBox grpLogin;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtLastName;
        private System.Windows.Forms.Button cmdConnect;
        private System.Windows.Forms.TextBox txtFirstName;
        private System.Windows.Forms.Button cmdInfo;
        private System.Windows.Forms.Button cmdActivate;
        private System.Windows.Forms.Button cmdCreate;
        private System.Windows.Forms.Button cmdLeave;
    }
}