namespace SLChat
{
    partial class frmIMs
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
            this.tabIMs = new System.Windows.Forms.TabControl();
            this.tpgNewIM = new System.Windows.Forms.TabPage();
            this.btnNewIM = new System.Windows.Forms.Button();
            this.lbxFriendsList = new System.Windows.Forms.ListBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.tabIMs.SuspendLayout();
            this.tpgNewIM.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabIMs
            // 
            this.tabIMs.Controls.Add(this.tpgNewIM);
            this.tabIMs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabIMs.Location = new System.Drawing.Point(0, 0);
            this.tabIMs.Name = "tabIMs";
            this.tabIMs.SelectedIndex = 0;
            this.tabIMs.Size = new System.Drawing.Size(364, 266);
            this.tabIMs.TabIndex = 0;
            // 
            // tpgNewIM
            // 
            this.tpgNewIM.Controls.Add(this.btnClose);
            this.tpgNewIM.Controls.Add(this.btnNewIM);
            this.tpgNewIM.Controls.Add(this.lbxFriendsList);
            this.tpgNewIM.Location = new System.Drawing.Point(4, 22);
            this.tpgNewIM.Name = "tpgNewIM";
            this.tpgNewIM.Padding = new System.Windows.Forms.Padding(3);
            this.tpgNewIM.Size = new System.Drawing.Size(356, 240);
            this.tpgNewIM.TabIndex = 0;
            this.tpgNewIM.Text = "New IM";
            this.tpgNewIM.UseVisualStyleBackColor = true;
            // 
            // btnNewIM
            // 
            this.btnNewIM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNewIM.Location = new System.Drawing.Point(192, 209);
            this.btnNewIM.Name = "btnNewIM";
            this.btnNewIM.Size = new System.Drawing.Size(75, 23);
            this.btnNewIM.TabIndex = 2;
            this.btnNewIM.Text = "New IM";
            this.btnNewIM.UseVisualStyleBackColor = true;
            // 
            // lbxFriendsList
            // 
            this.lbxFriendsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lbxFriendsList.FormattingEnabled = true;
            this.lbxFriendsList.IntegralHeight = false;
            this.lbxFriendsList.Items.AddRange(new object[] {
            "You have no friends, loser. :P"});
            this.lbxFriendsList.Location = new System.Drawing.Point(11, 6);
            this.lbxFriendsList.Name = "lbxFriendsList";
            this.lbxFriendsList.Size = new System.Drawing.Size(337, 197);
            this.lbxFriendsList.TabIndex = 0;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(273, 209);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // frmIMs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(364, 266);
            this.Controls.Add(this.tabIMs);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "frmIMs";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SLChat - IMs";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmIMs_FormClosing);
            this.tabIMs.ResumeLayout(false);
            this.tpgNewIM.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabIMs;
        private System.Windows.Forms.TabPage tpgNewIM;
        private System.Windows.Forms.ListBox lbxFriendsList;
        private System.Windows.Forms.Button btnNewIM;
        private System.Windows.Forms.Button btnClose;
    }
}
