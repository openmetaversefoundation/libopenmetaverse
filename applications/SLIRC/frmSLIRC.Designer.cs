namespace SLIRC
{
    partial class frmSLIRC
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
            try
            {
                ircclient.Disconnect();
            }
            catch
            {

            }
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
            this.lstLog = new System.Windows.Forms.ListBox();
            this.lstAllowedUsers = new System.Windows.Forms.ListBox();
            this.txtServerName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.txtChannel = new System.Windows.Forms.TextBox();
            this.btnJoin = new System.Windows.Forms.Button();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnSay = new System.Windows.Forms.Button();
            this.grpDebug = new System.Windows.Forms.GroupBox();
            this.btnGetPos = new System.Windows.Forms.Button();
            this.grpLogin.SuspendLayout();
            this.grpDebug.SuspendLayout();
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
            this.grpLogin.Enabled = false;
            this.grpLogin.Location = new System.Drawing.Point(2, 242);
            this.grpLogin.Name = "grpLogin";
            this.grpLogin.Size = new System.Drawing.Size(560, 77);
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
            this.cmdConnect.Location = new System.Drawing.Point(424, 40);
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
            // lstLog
            // 
            this.lstLog.Enabled = false;
            this.lstLog.FormattingEnabled = true;
            this.lstLog.Location = new System.Drawing.Point(19, 11);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(315, 225);
            this.lstLog.TabIndex = 52;
            // 
            // lstAllowedUsers
            // 
            this.lstAllowedUsers.Enabled = false;
            this.lstAllowedUsers.FormattingEnabled = true;
            this.lstAllowedUsers.Location = new System.Drawing.Point(349, 13);
            this.lstAllowedUsers.Name = "lstAllowedUsers";
            this.lstAllowedUsers.Size = new System.Drawing.Size(212, 108);
            this.lstAllowedUsers.TabIndex = 53;
            // 
            // txtServerName
            // 
            this.txtServerName.Location = new System.Drawing.Point(349, 141);
            this.txtServerName.Name = "txtServerName";
            this.txtServerName.Size = new System.Drawing.Size(130, 20);
            this.txtServerName.TabIndex = 54;
            this.txtServerName.Text = "irc.efnet.pl";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(351, 125);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 13);
            this.label4.TabIndex = 55;
            this.label4.Text = "Server Name";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(483, 124);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(26, 13);
            this.label5.TabIndex = 55;
            this.label5.Text = "Port";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(486, 141);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(60, 20);
            this.txtPort.TabIndex = 56;
            this.txtPort.Text = "6667";
            // 
            // txtChannel
            // 
            this.txtChannel.Location = new System.Drawing.Point(349, 167);
            this.txtChannel.Name = "txtChannel";
            this.txtChannel.Size = new System.Drawing.Size(130, 20);
            this.txtChannel.TabIndex = 54;
            this.txtChannel.Text = "#libsl";
            // 
            // btnJoin
            // 
            this.btnJoin.Enabled = false;
            this.btnJoin.Location = new System.Drawing.Point(487, 167);
            this.btnJoin.Name = "btnJoin";
            this.btnJoin.Size = new System.Drawing.Size(58, 20);
            this.btnJoin.TabIndex = 57;
            this.btnJoin.Text = "Join";
            this.btnJoin.UseVisualStyleBackColor = true;
            this.btnJoin.Click += new System.EventHandler(this.btnJoin_Click);
            // 
            // txtMessage
            // 
            this.txtMessage.Enabled = false;
            this.txtMessage.Location = new System.Drawing.Point(350, 199);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(194, 20);
            this.txtMessage.TabIndex = 58;
            // 
            // btnSay
            // 
            this.btnSay.Enabled = false;
            this.btnSay.Location = new System.Drawing.Point(470, 219);
            this.btnSay.Name = "btnSay";
            this.btnSay.Size = new System.Drawing.Size(74, 25);
            this.btnSay.TabIndex = 59;
            this.btnSay.Text = "Say";
            this.btnSay.UseVisualStyleBackColor = true;
            this.btnSay.Click += new System.EventHandler(this.btnSay_Click);
            // 
            // grpDebug
            // 
            this.grpDebug.Controls.Add(this.btnGetPos);
            this.grpDebug.Location = new System.Drawing.Point(2, 325);
            this.grpDebug.Name = "grpDebug";
            this.grpDebug.Size = new System.Drawing.Size(559, 64);
            this.grpDebug.TabIndex = 60;
            this.grpDebug.TabStop = false;
            this.grpDebug.Text = "Debugging";
            // 
            // btnGetPos
            // 
            this.btnGetPos.Location = new System.Drawing.Point(14, 21);
            this.btnGetPos.Name = "btnGetPos";
            this.btnGetPos.Size = new System.Drawing.Size(87, 24);
            this.btnGetPos.TabIndex = 0;
            this.btnGetPos.Text = "Get Position";
            this.btnGetPos.UseVisualStyleBackColor = true;
            this.btnGetPos.Click += new System.EventHandler(this.btnGetPos_Click);
            // 
            // frmSLIRC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(566, 393);
            this.Controls.Add(this.grpDebug);
            this.Controls.Add(this.btnSay);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.btnJoin);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtChannel);
            this.Controls.Add(this.txtServerName);
            this.Controls.Add(this.lstAllowedUsers);
            this.Controls.Add(this.lstLog);
            this.Controls.Add(this.grpLogin);
            this.Name = "frmSLIRC";
            this.Text = "Second Life <-> IRC";
            this.Load += new System.EventHandler(this.frmSLIRC_Load);
            this.grpLogin.ResumeLayout(false);
            this.grpLogin.PerformLayout();
            this.grpDebug.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpLogin;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtLastName;
        private System.Windows.Forms.Button cmdConnect;
        private System.Windows.Forms.TextBox txtFirstName;
        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.ListBox lstAllowedUsers;
        private System.Windows.Forms.TextBox txtServerName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.TextBox txtChannel;
        private System.Windows.Forms.Button btnJoin;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnSay;
        private System.Windows.Forms.GroupBox grpDebug;
        private System.Windows.Forms.Button btnGetPos;
    }
}

