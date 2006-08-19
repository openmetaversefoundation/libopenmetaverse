namespace SLChat
{
    partial class frmMain
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
        	this.components = new System.ComponentModel.Container();
        	System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
        	this.menuStrip1 = new System.Windows.Forms.MenuStrip();
        	this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
        	this.mnuLoginout = new System.Windows.Forms.ToolStripMenuItem();
        	this.mnuFileSep = new System.Windows.Forms.ToolStripSeparator();
        	this.mnuFileExit = new System.Windows.Forms.ToolStripMenuItem();
        	this.mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
        	this.mnuEditPrefs = new System.Windows.Forms.ToolStripMenuItem();
        	this.mnuView = new System.Windows.Forms.ToolStripMenuItem();
        	this.mnuViewFriends = new System.Windows.Forms.ToolStripMenuItem();
        	this.mnuViewIMs = new System.Windows.Forms.ToolStripMenuItem();
        	this.mnuViewInventory = new System.Windows.Forms.ToolStripMenuItem();
        	this.mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
        	this.mnuHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
        	this.rtbChat = new System.Windows.Forms.RichTextBox();
        	this.lbxUsers = new System.Windows.Forms.ListBox();
        	this.cnxListNames = new System.Windows.Forms.ContextMenuStrip(this.components);
        	this.mnuKey = new System.Windows.Forms.ToolStripMenuItem();
        	this.tolSeperator = new System.Windows.Forms.ToolStripSeparator();
        	this.mnuRemove = new System.Windows.Forms.ToolStripMenuItem();
        	this.pnlInput = new System.Windows.Forms.Panel();
        	this.cbxChatType = new System.Windows.Forms.ComboBox();
        	this.txtInput = new System.Windows.Forms.TextBox();
        	this.btnSend = new System.Windows.Forms.Button();
        	this.toolStrip1 = new System.Windows.Forms.ToolStrip();
        	this.tbtnIM = new System.Windows.Forms.ToolStripButton();
        	this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
        	this.tbtnFriends = new System.Windows.Forms.ToolStripButton();
        	this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
        	this.tbtnInventory = new System.Windows.Forms.ToolStripButton();
        	this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
        	this.sptChat = new System.Windows.Forms.SplitContainer();
        	this.tabIMs = new System.Windows.Forms.TabControl();
        	this.tabLocalChat = new System.Windows.Forms.TabPage();
        	this.menuStrip1.SuspendLayout();
        	this.cnxListNames.SuspendLayout();
        	this.pnlInput.SuspendLayout();
        	this.toolStrip1.SuspendLayout();
        	this.sptChat.Panel1.SuspendLayout();
        	this.sptChat.Panel2.SuspendLayout();
        	this.sptChat.SuspendLayout();
        	this.tabIMs.SuspendLayout();
        	this.tabLocalChat.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// menuStrip1
        	// 
        	this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        	        	        	this.mnuFile,
        	        	        	this.mnuEdit,
        	        	        	this.mnuView,
        	        	        	this.mnuHelp});
        	this.menuStrip1.Location = new System.Drawing.Point(0, 0);
        	this.menuStrip1.Name = "menuStrip1";
        	this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
        	this.menuStrip1.Size = new System.Drawing.Size(540, 24);
        	this.menuStrip1.TabIndex = 6;
        	this.menuStrip1.Text = "menuStrip1";
        	// 
        	// mnuFile
        	// 
        	this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        	        	        	this.mnuLoginout,
        	        	        	this.mnuFileSep,
        	        	        	this.mnuFileExit});
        	this.mnuFile.Name = "mnuFile";
        	this.mnuFile.Size = new System.Drawing.Size(35, 20);
        	this.mnuFile.Text = "&File";
        	// 
        	// mnuLoginout
        	// 
        	this.mnuLoginout.Name = "mnuLoginout";
        	this.mnuLoginout.Size = new System.Drawing.Size(111, 22);
        	this.mnuLoginout.Text = "&Login...";
        	this.mnuLoginout.Click += new System.EventHandler(this.mnuLoginout_Click);
        	// 
        	// mnuFileSep
        	// 
        	this.mnuFileSep.Name = "mnuFileSep";
        	this.mnuFileSep.Size = new System.Drawing.Size(108, 6);
        	// 
        	// mnuFileExit
        	// 
        	this.mnuFileExit.Name = "mnuFileExit";
        	this.mnuFileExit.Size = new System.Drawing.Size(111, 22);
        	this.mnuFileExit.Text = "E&xit";
        	this.mnuFileExit.Click += new System.EventHandler(this.mnuFileExit_Click);
        	// 
        	// mnuEdit
        	// 
        	this.mnuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        	        	        	this.mnuEditPrefs});
        	this.mnuEdit.Name = "mnuEdit";
        	this.mnuEdit.Size = new System.Drawing.Size(37, 20);
        	this.mnuEdit.Text = "&Edit";
        	// 
        	// mnuEditPrefs
        	// 
        	this.mnuEditPrefs.Name = "mnuEditPrefs";
        	this.mnuEditPrefs.Size = new System.Drawing.Size(144, 22);
        	this.mnuEditPrefs.Text = "&Preferences...";
        	this.mnuEditPrefs.Click += new System.EventHandler(this.MnuEditPrefsClick);
        	// 
        	// mnuView
        	// 
        	this.mnuView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        	        	        	this.mnuViewFriends,
        	        	        	this.mnuViewIMs,
        	        	        	this.mnuViewInventory});
        	this.mnuView.Name = "mnuView";
        	this.mnuView.Size = new System.Drawing.Size(41, 20);
        	this.mnuView.Text = "&View";
        	// 
        	// mnuViewFriends
        	// 
        	this.mnuViewFriends.CheckOnClick = true;
        	this.mnuViewFriends.Name = "mnuViewFriends";
        	this.mnuViewFriends.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
        	        	        	| System.Windows.Forms.Keys.F)));
        	this.mnuViewFriends.Size = new System.Drawing.Size(197, 22);
        	this.mnuViewFriends.Text = "&Friends";
        	// 
        	// mnuViewIMs
        	// 
        	this.mnuViewIMs.CheckOnClick = true;
        	this.mnuViewIMs.Name = "mnuViewIMs";
        	this.mnuViewIMs.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
        	this.mnuViewIMs.Size = new System.Drawing.Size(197, 22);
        	this.mnuViewIMs.Text = "&Instant Messages";
        	this.mnuViewIMs.Click += new System.EventHandler(this.mnuViewIMs_Click);
        	// 
        	// mnuViewInventory
        	// 
        	this.mnuViewInventory.CheckOnClick = true;
        	this.mnuViewInventory.Name = "mnuViewInventory";
        	this.mnuViewInventory.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
        	this.mnuViewInventory.Size = new System.Drawing.Size(197, 22);
        	this.mnuViewInventory.Text = "I&nventory";
        	// 
        	// mnuHelp
        	// 
        	this.mnuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        	        	        	this.mnuHelpAbout});
        	this.mnuHelp.Name = "mnuHelp";
        	this.mnuHelp.Size = new System.Drawing.Size(40, 20);
        	this.mnuHelp.Text = "&Help";
        	// 
        	// mnuHelpAbout
        	// 
        	this.mnuHelpAbout.Name = "mnuHelpAbout";
        	this.mnuHelpAbout.Size = new System.Drawing.Size(115, 22);
        	this.mnuHelpAbout.Text = "&About...";
        	this.mnuHelpAbout.Click += new System.EventHandler(this.mnuHelpAbout_Click);
        	// 
        	// rtbChat
        	// 
        	this.rtbChat.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.rtbChat.BackColor = System.Drawing.Color.White;
        	this.rtbChat.HideSelection = false;
        	this.rtbChat.Location = new System.Drawing.Point(0, 0);
        	this.rtbChat.Name = "rtbChat";
        	this.rtbChat.ReadOnly = true;
        	this.rtbChat.ShowSelectionMargin = true;
        	this.rtbChat.Size = new System.Drawing.Size(394, 298);
        	this.rtbChat.TabIndex = 4;
        	this.rtbChat.Text = "";
        	this.rtbChat.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.Link_Clicked);
        	// 
        	// lbxUsers
        	// 
        	this.lbxUsers.ContextMenuStrip = this.cnxListNames;
        	this.lbxUsers.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.lbxUsers.FormattingEnabled = true;
        	this.lbxUsers.IntegralHeight = false;
        	this.lbxUsers.Location = new System.Drawing.Point(0, 0);
        	this.lbxUsers.Name = "lbxUsers";
        	this.lbxUsers.Size = new System.Drawing.Size(128, 298);
        	this.lbxUsers.TabIndex = 3;
        	// 
        	// cnxListNames
        	// 
        	this.cnxListNames.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        	        	        	this.mnuKey,
        	        	        	this.tolSeperator,
        	        	        	this.mnuRemove});
        	this.cnxListNames.Name = "cnxListNames";
        	this.cnxListNames.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
        	this.cnxListNames.Size = new System.Drawing.Size(154, 54);
        	// 
        	// mnuKey
        	// 
        	this.mnuKey.Name = "mnuKey";
        	this.mnuKey.Size = new System.Drawing.Size(153, 22);
        	this.mnuKey.Text = "Print Key (UUID)";
        	this.mnuKey.Click += new System.EventHandler(this.MnuKeyClick);
        	// 
        	// tolSeperator
        	// 
        	this.tolSeperator.Name = "tolSeperator";
        	this.tolSeperator.Size = new System.Drawing.Size(150, 6);
        	// 
        	// mnuRemove
        	// 
        	this.mnuRemove.Name = "mnuRemove";
        	this.mnuRemove.Size = new System.Drawing.Size(153, 22);
        	this.mnuRemove.Text = "Remove";
        	this.mnuRemove.Click += new System.EventHandler(this.MnuRemoveClick);
        	// 
        	// pnlInput
        	// 
        	this.pnlInput.Controls.Add(this.cbxChatType);
        	this.pnlInput.Controls.Add(this.txtInput);
        	this.pnlInput.Controls.Add(this.btnSend);
        	this.pnlInput.Dock = System.Windows.Forms.DockStyle.Bottom;
        	this.pnlInput.Location = new System.Drawing.Point(3, 301);
        	this.pnlInput.Name = "pnlInput";
        	this.pnlInput.Size = new System.Drawing.Size(526, 24);
        	this.pnlInput.TabIndex = 4;
        	// 
        	// cbxChatType
        	// 
        	this.cbxChatType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.cbxChatType.FlatStyle = System.Windows.Forms.FlatStyle.System;
        	this.cbxChatType.FormattingEnabled = true;
        	this.cbxChatType.Items.AddRange(new object[] {
        	        	        	"Say",
        	        	        	"Shout",
        	        	        	"Whisper"});
        	this.cbxChatType.Location = new System.Drawing.Point(371, 0);
        	this.cbxChatType.Name = "cbxChatType";
        	this.cbxChatType.Size = new System.Drawing.Size(73, 21);
        	this.cbxChatType.TabIndex = 1;
        	this.cbxChatType.Text = "Say";
        	// 
        	// txtInput
        	// 
        	this.txtInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.txtInput.Location = new System.Drawing.Point(0, 0);
        	this.txtInput.MaxLength = 977;
        	this.txtInput.Name = "txtInput";
        	this.txtInput.Size = new System.Drawing.Size(365, 21);
        	this.txtInput.TabIndex = 0;
        	this.txtInput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtInput_KeyUp);
        	this.txtInput.TextChanged += new System.EventHandler(this.txtInput_TextChanged);
        	// 
        	// btnSend
        	// 
        	this.btnSend.Dock = System.Windows.Forms.DockStyle.Right;
        	this.btnSend.Enabled = false;
        	this.btnSend.Location = new System.Drawing.Point(450, 0);
        	this.btnSend.Name = "btnSend";
        	this.btnSend.Size = new System.Drawing.Size(76, 24);
        	this.btnSend.TabIndex = 2;
        	this.btnSend.Text = "Send";
        	this.btnSend.UseVisualStyleBackColor = true;
        	this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
        	// 
        	// toolStrip1
        	// 
        	this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
        	this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        	        	        	this.tbtnIM,
        	        	        	this.toolStripSeparator2,
        	        	        	this.tbtnFriends,
        	        	        	this.toolStripSeparator1,
        	        	        	this.tbtnInventory,
        	        	        	this.toolStripSeparator3});
        	this.toolStrip1.Location = new System.Drawing.Point(0, 24);
        	this.toolStrip1.Name = "toolStrip1";
        	this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
        	this.toolStrip1.Size = new System.Drawing.Size(540, 25);
        	this.toolStrip1.TabIndex = 5;
        	this.toolStrip1.Text = "toolStrip1";
        	// 
        	// tbtnIM
        	// 
        	this.tbtnIM.CheckOnClick = true;
        	this.tbtnIM.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
        	this.tbtnIM.Image = ((System.Drawing.Image)(resources.GetObject("tbtnIM.Image")));
        	this.tbtnIM.ImageTransparentColor = System.Drawing.Color.Magenta;
        	this.tbtnIM.Name = "tbtnIM";
        	this.tbtnIM.Size = new System.Drawing.Size(23, 22);
        	this.tbtnIM.Text = "IM";
        	this.tbtnIM.Click += new System.EventHandler(this.tbtnIM_Click);
        	// 
        	// toolStripSeparator2
        	// 
        	this.toolStripSeparator2.Name = "toolStripSeparator2";
        	this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
        	// 
        	// tbtnFriends
        	// 
        	this.tbtnFriends.CheckOnClick = true;
        	this.tbtnFriends.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
        	this.tbtnFriends.Image = ((System.Drawing.Image)(resources.GetObject("tbtnFriends.Image")));
        	this.tbtnFriends.ImageTransparentColor = System.Drawing.Color.Magenta;
        	this.tbtnFriends.Name = "tbtnFriends";
        	this.tbtnFriends.Size = new System.Drawing.Size(46, 22);
        	this.tbtnFriends.Text = "Friends";
        	// 
        	// toolStripSeparator1
        	// 
        	this.toolStripSeparator1.Name = "toolStripSeparator1";
        	this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
        	// 
        	// tbtnInventory
        	// 
        	this.tbtnInventory.CheckOnClick = true;
        	this.tbtnInventory.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
        	this.tbtnInventory.Image = ((System.Drawing.Image)(resources.GetObject("tbtnInventory.Image")));
        	this.tbtnInventory.ImageTransparentColor = System.Drawing.Color.Magenta;
        	this.tbtnInventory.Name = "tbtnInventory";
        	this.tbtnInventory.Size = new System.Drawing.Size(59, 22);
        	this.tbtnInventory.Text = "Inventory";
        	// 
        	// toolStripSeparator3
        	// 
        	this.toolStripSeparator3.Name = "toolStripSeparator3";
        	this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
        	// 
        	// sptChat
        	// 
        	this.sptChat.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.sptChat.Location = new System.Drawing.Point(3, 3);
        	this.sptChat.Name = "sptChat";
        	// 
        	// sptChat.Panel1
        	// 
        	this.sptChat.Panel1.Controls.Add(this.rtbChat);
        	this.sptChat.Panel1MinSize = 0;
        	// 
        	// sptChat.Panel2
        	// 
        	this.sptChat.Panel2.Controls.Add(this.lbxUsers);
        	this.sptChat.Panel2MinSize = 0;
        	this.sptChat.Size = new System.Drawing.Size(526, 298);
        	this.sptChat.SplitterDistance = 394;
        	this.sptChat.TabIndex = 7;
        	// 
        	// tabIMs
        	// 
        	this.tabIMs.Appearance = System.Windows.Forms.TabAppearance.Buttons;
        	this.tabIMs.Controls.Add(this.tabLocalChat);
        	this.tabIMs.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.tabIMs.Location = new System.Drawing.Point(0, 49);
        	this.tabIMs.Name = "tabIMs";
        	this.tabIMs.SelectedIndex = 0;
        	this.tabIMs.Size = new System.Drawing.Size(540, 357);
        	this.tabIMs.TabIndex = 5;
        	// 
        	// tabLocalChat
        	// 
        	this.tabLocalChat.Controls.Add(this.sptChat);
        	this.tabLocalChat.Controls.Add(this.pnlInput);
        	this.tabLocalChat.Location = new System.Drawing.Point(4, 25);
        	this.tabLocalChat.Name = "tabLocalChat";
        	this.tabLocalChat.Padding = new System.Windows.Forms.Padding(3);
        	this.tabLocalChat.Size = new System.Drawing.Size(532, 328);
        	this.tabLocalChat.TabIndex = 0;
        	this.tabLocalChat.Text = "Local Chat";
        	this.tabLocalChat.UseVisualStyleBackColor = true;
        	// 
        	// frmMain
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(540, 406);
        	this.Controls.Add(this.tabIMs);
        	this.Controls.Add(this.toolStrip1);
        	this.Controls.Add(this.menuStrip1);
        	this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.MainMenuStrip = this.menuStrip1;
        	this.Name = "frmMain";
        	this.Text = "SLChat";
        	this.VisibleChanged += new System.EventHandler(this.frmMain_VisibleChanged);
        	this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_Closing);
        	this.menuStrip1.ResumeLayout(false);
        	this.menuStrip1.PerformLayout();
        	this.cnxListNames.ResumeLayout(false);
        	this.pnlInput.ResumeLayout(false);
        	this.pnlInput.PerformLayout();
        	this.toolStrip1.ResumeLayout(false);
        	this.toolStrip1.PerformLayout();
        	this.sptChat.Panel1.ResumeLayout(false);
        	this.sptChat.Panel2.ResumeLayout(false);
        	this.sptChat.ResumeLayout(false);
        	this.tabIMs.ResumeLayout(false);
        	this.tabLocalChat.ResumeLayout(false);
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }
        private System.Windows.Forms.TabPage tabLocalChat;
        private System.Windows.Forms.TabControl tabIMs;
        private System.Windows.Forms.ToolStripMenuItem mnuRemove;
        private System.Windows.Forms.ToolStripSeparator tolSeperator;
        private System.Windows.Forms.ToolStripMenuItem mnuKey;
        private System.Windows.Forms.ContextMenuStrip cnxListNames;
        private System.Windows.Forms.ToolStripSeparator mnuFileSep;
        private System.Windows.Forms.Panel pnlInput;
        private System.Windows.Forms.SplitContainer sptChat;
        private System.Windows.Forms.ComboBox cbxChatType;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox txtInput;

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem mnuFileExit;
        private System.Windows.Forms.ToolStripMenuItem mnuEdit;
        private System.Windows.Forms.ToolStripMenuItem mnuEditPrefs;
        private System.Windows.Forms.ToolStripMenuItem mnuHelp;
        private System.Windows.Forms.ToolStripMenuItem mnuHelpAbout;
        private System.Windows.Forms.RichTextBox rtbChat;
        private System.Windows.Forms.ListBox lbxUsers;
        private System.Windows.Forms.ToolStripMenuItem mnuLoginout;
        private System.Windows.Forms.ToolStripMenuItem mnuView;
        private System.Windows.Forms.ToolStripMenuItem mnuViewIMs;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tbtnIM;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tbtnInventory;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mnuViewInventory;
        private System.Windows.Forms.ToolStripMenuItem mnuViewFriends;
        private System.Windows.Forms.ToolStripButton tbtnFriends;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    }
}
