namespace WinGridProxy
{
    partial class FormWinGridProxy
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormWinGridProxy));
            this.panelProxyConfig = new System.Windows.Forms.Panel();
            this.comboBoxListenAddress = new System.Windows.Forms.ComboBox();
            this.comboBoxLoginURL = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxProxyPort = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.panelMainWindow = new System.Windows.Forms.Panel();
            this.splitContainerSessionsTabs = new System.Windows.Forms.SplitContainer();
            this.listViewSessions = new WinGridProxy.ListViewNoFlicker();
            this.columnHeaderCounter = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderProtocol = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderType = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderSize = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderUrl = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderContentType = new System.Windows.Forms.ColumnHeader();
            this.contextMenuStripSessions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemAutoScroll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuSessionsRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripRemove = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemRemoveAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRemoveSelected = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRemoveUnselected = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.selectToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripSelect = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.allToolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.invertToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.noneToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorSelectPacketProto = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemSelectPacketName = new System.Windows.Forms.ToolStripMenuItem();
            this.selectToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorFilterPacketByName = new System.Windows.Forms.ToolStripSeparator();
            this.enableDisableFilterByNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.markToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripMark = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.redToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.goldToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.greenToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.blueToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.orangeToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            this.unmarkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this.findToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageSummary = new System.Windows.Forms.TabPage();
            this.panelStats = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.labelPacketsTotal = new System.Windows.Forms.Label();
            this.label1PacketsOut = new System.Windows.Forms.Label();
            this.labelPacketsIn = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelCapsTotal = new System.Windows.Forms.Label();
            this.labelCapsOut = new System.Windows.Forms.Label();
            this.labelCapsIn = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.tabPageFilters = new System.Windows.Forms.TabPage();
            this.splitContainerFilters = new System.Windows.Forms.SplitContainer();
            this.checkBoxCheckAllPackets = new System.Windows.Forms.CheckBox();
            this.grpUDPFilters = new System.Windows.Forms.GroupBox();
            this.listViewPacketFilters = new WinGridProxy.ListViewNoFlicker();
            this.columnHeaderPacketName = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderPacketType = new System.Windows.Forms.ColumnHeader();
            this.checkBoxCheckAllMessages = new System.Windows.Forms.CheckBox();
            this.grpCapsFilters = new System.Windows.Forms.GroupBox();
            this.listViewMessageFilters = new WinGridProxy.ListViewNoFlicker();
            this.columnHeaderName = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderMessageType = new System.Windows.Forms.ColumnHeader();
            this.tabPageInspect = new System.Windows.Forms.TabPage();
            this.splitContainerInspectorTab = new System.Windows.Forms.SplitContainer();
            this.tabControlInspectorRequest = new System.Windows.Forms.TabControl();
            this.tabPageDecodedRequest = new System.Windows.Forms.TabPage();
            this.richTextBoxDecodedRequest = new System.Windows.Forms.RichTextBox();
            this.tabPageRawRequest = new System.Windows.Forms.TabPage();
            this.richTextBoxRawRequest = new System.Windows.Forms.RichTextBox();
            this.tabPageXMLRequest = new System.Windows.Forms.TabPage();
            this.treeViewXMLRequest = new System.Windows.Forms.TreeView();
            this.tabPageRequestJson = new System.Windows.Forms.TabPage();
            this.richTextBoxNotationRequest = new System.Windows.Forms.RichTextBox();
            this.tabPageHexRequest = new System.Windows.Forms.TabPage();
            this.statusStrip2 = new System.Windows.Forms.StatusStrip();
            this.labelRequestHex = new System.Windows.Forms.ToolStripStatusLabel();
            this.hexBoxRequest = new Be.Windows.Forms.HexBox();
            this.tabControlInspectorResponse = new System.Windows.Forms.TabControl();
            this.tabPageDecodeResponse = new System.Windows.Forms.TabPage();
            this.richTextBoxDecodedResponse = new System.Windows.Forms.RichTextBox();
            this.tabPageInspectorRAWResponse = new System.Windows.Forms.TabPage();
            this.richTextBoxRawResponse = new System.Windows.Forms.RichTextBox();
            this.tabPageInspectorXMLResponse = new System.Windows.Forms.TabPage();
            this.treeViewXmlResponse = new System.Windows.Forms.TreeView();
            this.tabPageResponseJson = new System.Windows.Forms.TabPage();
            this.richTextBoxNotationResponse = new System.Windows.Forms.RichTextBox();
            this.tabPageHexViewResponse = new System.Windows.Forms.TabPage();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.labelResponseHex = new System.Windows.Forms.ToolStripStatusLabel();
            this.hexBoxResponse = new Be.Windows.Forms.HexBox();
            this.tabPageInject = new System.Windows.Forms.TabPage();
            this.radioButtonViewer = new System.Windows.Forms.RadioButton();
            this.radioButtonSimulator = new System.Windows.Forms.RadioButton();
            this.button3 = new System.Windows.Forms.Button();
            this.buttonInjectPacket = new System.Windows.Forms.Button();
            this.richTextBoxInject = new System.Windows.Forms.RichTextBox();
            this.markToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripLabelHexEditorRequest = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripFileMenu = new System.Windows.Forms.ToolStripDropDownButton();
            this.captureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.saveSessionArchiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadSessionArchiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoScrollSessionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableStatisticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveOptionsOnExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startProxyOnStartupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.EditToolStripButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.copyToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripCopy = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.requestDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.responseDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hostAddressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.packetMessageTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton5 = new System.Windows.Forms.ToolStripDropDownButton();
            this.saveFilterSelectionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadFilterSelectionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoAddNewDiscoveredMessagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton4 = new System.Windows.Forms.ToolStripDropDownButton();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutWinGridProxyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.captureTrafficToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveSessionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadSessionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unselectedSessionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.noneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.invertSelectionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.markToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.goldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.orangeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.purpleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.yellowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.removeToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.findSessionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton3 = new System.Windows.Forms.ToolStripDropDownButton();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog2 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip3 = new System.Windows.Forms.StatusStrip();
            this.toolStripMainLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.contextMenuStripFilterOptions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.uncheckAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.autoColorizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.panelProxyConfig.SuspendLayout();
            this.panelMainWindow.SuspendLayout();
            this.splitContainerSessionsTabs.Panel1.SuspendLayout();
            this.splitContainerSessionsTabs.Panel2.SuspendLayout();
            this.splitContainerSessionsTabs.SuspendLayout();
            this.contextMenuStripSessions.SuspendLayout();
            this.contextMenuStripRemove.SuspendLayout();
            this.contextMenuStripSelect.SuspendLayout();
            this.contextMenuStripMark.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageSummary.SuspendLayout();
            this.panelStats.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPageFilters.SuspendLayout();
            this.splitContainerFilters.Panel1.SuspendLayout();
            this.splitContainerFilters.Panel2.SuspendLayout();
            this.splitContainerFilters.SuspendLayout();
            this.grpUDPFilters.SuspendLayout();
            this.grpCapsFilters.SuspendLayout();
            this.tabPageInspect.SuspendLayout();
            this.splitContainerInspectorTab.Panel1.SuspendLayout();
            this.splitContainerInspectorTab.Panel2.SuspendLayout();
            this.splitContainerInspectorTab.SuspendLayout();
            this.tabControlInspectorRequest.SuspendLayout();
            this.tabPageDecodedRequest.SuspendLayout();
            this.tabPageRawRequest.SuspendLayout();
            this.tabPageXMLRequest.SuspendLayout();
            this.tabPageRequestJson.SuspendLayout();
            this.tabPageHexRequest.SuspendLayout();
            this.statusStrip2.SuspendLayout();
            this.tabControlInspectorResponse.SuspendLayout();
            this.tabPageDecodeResponse.SuspendLayout();
            this.tabPageInspectorRAWResponse.SuspendLayout();
            this.tabPageInspectorXMLResponse.SuspendLayout();
            this.tabPageResponseJson.SuspendLayout();
            this.tabPageHexViewResponse.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabPageInject.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.contextMenuStripCopy.SuspendLayout();
            this.statusStrip3.SuspendLayout();
            this.contextMenuStripFilterOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelProxyConfig
            // 
            this.panelProxyConfig.Controls.Add(this.comboBoxListenAddress);
            this.panelProxyConfig.Controls.Add(this.comboBoxLoginURL);
            this.panelProxyConfig.Controls.Add(this.label3);
            this.panelProxyConfig.Controls.Add(this.label2);
            this.panelProxyConfig.Controls.Add(this.textBoxProxyPort);
            this.panelProxyConfig.Controls.Add(this.button1);
            this.panelProxyConfig.Controls.Add(this.label1);
            this.panelProxyConfig.Location = new System.Drawing.Point(12, 28);
            this.panelProxyConfig.Name = "panelProxyConfig";
            this.panelProxyConfig.Size = new System.Drawing.Size(1070, 32);
            this.panelProxyConfig.TabIndex = 0;
            // 
            // comboBoxListenAddress
            // 
            this.comboBoxListenAddress.FormattingEnabled = true;
            this.comboBoxListenAddress.Location = new System.Drawing.Point(98, 6);
            this.comboBoxListenAddress.Name = "comboBoxListenAddress";
            this.comboBoxListenAddress.Size = new System.Drawing.Size(139, 21);
            this.comboBoxListenAddress.TabIndex = 7;
            this.comboBoxListenAddress.Text = "127.0.0.1";
            // 
            // comboBoxLoginURL
            // 
            this.comboBoxLoginURL.FormattingEnabled = true;
            this.comboBoxLoginURL.Items.AddRange(new object[] {
            "https://login.agni.lindenlab.com/cgi-bin/login.cgi",
            "https://login.aditi.lindenlab.com/cgi-bin/login.cgi",
            "http://127.0.0.1:8002",
            "http://osgrid.org:8002"});
            this.comboBoxLoginURL.Location = new System.Drawing.Point(393, 6);
            this.comboBoxLoginURL.Name = "comboBoxLoginURL";
            this.comboBoxLoginURL.Size = new System.Drawing.Size(252, 21);
            this.comboBoxLoginURL.TabIndex = 6;
            this.comboBoxLoginURL.Text = "https://login.agni.lindenlab.com/cgi-bin/login.cgi";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(329, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Login URL";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Listen IP Address";
            // 
            // textBoxProxyPort
            // 
            this.textBoxProxyPort.Location = new System.Drawing.Point(275, 6);
            this.textBoxProxyPort.Name = "textBoxProxyPort";
            this.textBoxProxyPort.Size = new System.Drawing.Size(48, 20);
            this.textBoxProxyPort.TabIndex = 3;
            this.textBoxProxyPort.Text = "8080";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(651, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Start Proxy";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.buttonStartProxy_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(243, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Port";
            // 
            // panelMainWindow
            // 
            this.panelMainWindow.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelMainWindow.Controls.Add(this.splitContainerSessionsTabs);
            this.panelMainWindow.Location = new System.Drawing.Point(12, 66);
            this.panelMainWindow.Name = "panelMainWindow";
            this.panelMainWindow.Size = new System.Drawing.Size(1087, 428);
            this.panelMainWindow.TabIndex = 1;
            // 
            // splitContainerSessionsTabs
            // 
            this.splitContainerSessionsTabs.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainerSessionsTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerSessionsTabs.Location = new System.Drawing.Point(0, 0);
            this.splitContainerSessionsTabs.Name = "splitContainerSessionsTabs";
            // 
            // splitContainerSessionsTabs.Panel1
            // 
            this.splitContainerSessionsTabs.Panel1.Controls.Add(this.listViewSessions);
            // 
            // splitContainerSessionsTabs.Panel2
            // 
            this.splitContainerSessionsTabs.Panel2.Controls.Add(this.tabControl1);
            this.splitContainerSessionsTabs.Size = new System.Drawing.Size(1087, 428);
            this.splitContainerSessionsTabs.SplitterDistance = 469;
            this.splitContainerSessionsTabs.SplitterWidth = 5;
            this.splitContainerSessionsTabs.TabIndex = 0;
            // 
            // listViewSessions
            // 
            this.listViewSessions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderCounter,
            this.columnHeaderProtocol,
            this.columnHeaderType,
            this.columnHeaderSize,
            this.columnHeaderUrl,
            this.columnHeaderContentType});
            this.listViewSessions.ContextMenuStrip = this.contextMenuStripSessions;
            this.listViewSessions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewSessions.FullRowSelect = true;
            this.listViewSessions.GridLines = true;
            this.listViewSessions.HideSelection = false;
            this.listViewSessions.Location = new System.Drawing.Point(0, 0);
            this.listViewSessions.Name = "listViewSessions";
            this.listViewSessions.Size = new System.Drawing.Size(469, 428);
            this.listViewSessions.SmallImageList = this.imageList1;
            this.listViewSessions.TabIndex = 0;
            this.listViewSessions.UseCompatibleStateImageBehavior = false;
            this.listViewSessions.View = System.Windows.Forms.View.Details;
            this.listViewSessions.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewSessions_ItemSelectionChanged);
            // 
            // columnHeaderCounter
            // 
            this.columnHeaderCounter.Text = "#";
            this.columnHeaderCounter.Width = 54;
            // 
            // columnHeaderProtocol
            // 
            this.columnHeaderProtocol.Text = "Protocol";
            this.columnHeaderProtocol.Width = 59;
            // 
            // columnHeaderType
            // 
            this.columnHeaderType.Text = "Packet Type";
            this.columnHeaderType.Width = 151;
            // 
            // columnHeaderSize
            // 
            this.columnHeaderSize.Text = "Bytes";
            this.columnHeaderSize.Width = 64;
            // 
            // columnHeaderUrl
            // 
            this.columnHeaderUrl.Text = "Host/Address";
            this.columnHeaderUrl.Width = 312;
            // 
            // columnHeaderContentType
            // 
            this.columnHeaderContentType.Text = "Content Type";
            this.columnHeaderContentType.Width = 250;
            // 
            // contextMenuStripSessions
            // 
            this.contextMenuStripSessions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemAutoScroll,
            this.toolStripSeparator13,
            this.toolStripMenuSessionsRemove,
            this.selectToolStripMenuItem2,
            this.toolStripSeparatorFilterPacketByName,
            this.enableDisableFilterByNameToolStripMenuItem,
            this.toolStripSeparator15,
            this.markToolStripMenuItem2,
            this.toolStripSeparator16,
            this.findToolStripMenuItem1});
            this.contextMenuStripSessions.Name = "contextMenuStripSessions";
            this.contextMenuStripSessions.Size = new System.Drawing.Size(180, 160);
            this.contextMenuStripSessions.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripSessions_Opening);
            // 
            // toolStripMenuItemAutoScroll
            // 
            this.toolStripMenuItemAutoScroll.CheckOnClick = true;
            this.toolStripMenuItemAutoScroll.Name = "toolStripMenuItemAutoScroll";
            this.toolStripMenuItemAutoScroll.Size = new System.Drawing.Size(179, 22);
            this.toolStripMenuItemAutoScroll.Text = "Auto Scroll";
            this.toolStripMenuItemAutoScroll.CheckedChanged += new System.EventHandler(this.sessionEnableAutoScroll_CheckedChanged);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(176, 6);
            // 
            // toolStripMenuSessionsRemove
            // 
            this.toolStripMenuSessionsRemove.DropDown = this.contextMenuStripRemove;
            this.toolStripMenuSessionsRemove.Name = "toolStripMenuSessionsRemove";
            this.toolStripMenuSessionsRemove.Size = new System.Drawing.Size(179, 22);
            this.toolStripMenuSessionsRemove.Text = "Remove";
            // 
            // contextMenuStripRemove
            // 
            this.contextMenuStripRemove.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemRemoveAll,
            this.toolStripMenuItemRemoveSelected,
            this.toolStripMenuItemRemoveUnselected});
            this.contextMenuStripRemove.Name = "contextMenuStripRemove";
            this.contextMenuStripRemove.OwnerItem = this.toolStripMenuSessionsRemove;
            this.contextMenuStripRemove.Size = new System.Drawing.Size(149, 70);
            // 
            // toolStripMenuItemRemoveAll
            // 
            this.toolStripMenuItemRemoveAll.Name = "toolStripMenuItemRemoveAll";
            this.toolStripMenuItemRemoveAll.Size = new System.Drawing.Size(148, 22);
            this.toolStripMenuItemRemoveAll.Text = "All";
            this.toolStripMenuItemRemoveAll.Click += new System.EventHandler(this.sessionRemoveAll_Click);
            // 
            // toolStripMenuItemRemoveSelected
            // 
            this.toolStripMenuItemRemoveSelected.Name = "toolStripMenuItemRemoveSelected";
            this.toolStripMenuItemRemoveSelected.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.toolStripMenuItemRemoveSelected.Size = new System.Drawing.Size(148, 22);
            this.toolStripMenuItemRemoveSelected.Text = "Selected";
            this.toolStripMenuItemRemoveSelected.Click += new System.EventHandler(this.sessionRemoveSelected_Click);
            // 
            // toolStripMenuItemRemoveUnselected
            // 
            this.toolStripMenuItemRemoveUnselected.Name = "toolStripMenuItemRemoveUnselected";
            this.toolStripMenuItemRemoveUnselected.Size = new System.Drawing.Size(148, 22);
            this.toolStripMenuItemRemoveUnselected.Text = "Unselected";
            this.toolStripMenuItemRemoveUnselected.Click += new System.EventHandler(this.sessionRemoveUnselected_Click);
            // 
            // removeToolStripMenuItem2
            // 
            this.removeToolStripMenuItem2.DropDown = this.contextMenuStripRemove;
            this.removeToolStripMenuItem2.Name = "removeToolStripMenuItem2";
            this.removeToolStripMenuItem2.Size = new System.Drawing.Size(143, 22);
            this.removeToolStripMenuItem2.Text = "Remove";
            // 
            // selectToolStripMenuItem2
            // 
            this.selectToolStripMenuItem2.DropDown = this.contextMenuStripSelect;
            this.selectToolStripMenuItem2.Name = "selectToolStripMenuItem2";
            this.selectToolStripMenuItem2.Size = new System.Drawing.Size(179, 22);
            this.selectToolStripMenuItem2.Text = "Select";
            // 
            // contextMenuStripSelect
            // 
            this.contextMenuStripSelect.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allToolStripMenuItem4,
            this.invertToolStripMenuItem1,
            this.noneToolStripMenuItem2,
            this.toolStripSeparatorSelectPacketProto,
            this.toolStripMenuItemSelectPacketName});
            this.contextMenuStripSelect.Name = "contextMenuStripSelect";
            this.contextMenuStripSelect.OwnerItem = this.selectToolStripMenuItem2;
            this.contextMenuStripSelect.Size = new System.Drawing.Size(167, 98);
            // 
            // allToolStripMenuItem4
            // 
            this.allToolStripMenuItem4.Name = "allToolStripMenuItem4";
            this.allToolStripMenuItem4.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.allToolStripMenuItem4.Size = new System.Drawing.Size(166, 22);
            this.allToolStripMenuItem4.Text = "All";
            this.allToolStripMenuItem4.Click += new System.EventHandler(this.sessionSelectAll_Click);
            // 
            // invertToolStripMenuItem1
            // 
            this.invertToolStripMenuItem1.Name = "invertToolStripMenuItem1";
            this.invertToolStripMenuItem1.Size = new System.Drawing.Size(166, 22);
            this.invertToolStripMenuItem1.Text = "Invert";
            this.invertToolStripMenuItem1.Click += new System.EventHandler(this.sessionInvertSelection_Click);
            // 
            // noneToolStripMenuItem2
            // 
            this.noneToolStripMenuItem2.Name = "noneToolStripMenuItem2";
            this.noneToolStripMenuItem2.Size = new System.Drawing.Size(166, 22);
            this.noneToolStripMenuItem2.Text = "None";
            this.noneToolStripMenuItem2.Click += new System.EventHandler(this.sessionSelectNone_Click);
            // 
            // toolStripSeparatorSelectPacketProto
            // 
            this.toolStripSeparatorSelectPacketProto.Name = "toolStripSeparatorSelectPacketProto";
            this.toolStripSeparatorSelectPacketProto.Size = new System.Drawing.Size(163, 6);
            // 
            // toolStripMenuItemSelectPacketName
            // 
            this.toolStripMenuItemSelectPacketName.Name = "toolStripMenuItemSelectPacketName";
            this.toolStripMenuItemSelectPacketName.Size = new System.Drawing.Size(166, 22);
            this.toolStripMenuItemSelectPacketName.Text = "All (Packet Type)";
            this.toolStripMenuItemSelectPacketName.Click += new System.EventHandler(this.sessionSelectAllPacketType_Click);
            // 
            // selectToolStripMenuItem1
            // 
            this.selectToolStripMenuItem1.DropDown = this.contextMenuStripSelect;
            this.selectToolStripMenuItem1.Name = "selectToolStripMenuItem1";
            this.selectToolStripMenuItem1.Size = new System.Drawing.Size(143, 22);
            this.selectToolStripMenuItem1.Text = "Select";
            // 
            // toolStripSeparatorFilterPacketByName
            // 
            this.toolStripSeparatorFilterPacketByName.Name = "toolStripSeparatorFilterPacketByName";
            this.toolStripSeparatorFilterPacketByName.Size = new System.Drawing.Size(176, 6);
            // 
            // enableDisableFilterByNameToolStripMenuItem
            // 
            this.enableDisableFilterByNameToolStripMenuItem.Checked = true;
            this.enableDisableFilterByNameToolStripMenuItem.CheckOnClick = true;
            this.enableDisableFilterByNameToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableDisableFilterByNameToolStripMenuItem.Name = "enableDisableFilterByNameToolStripMenuItem";
            this.enableDisableFilterByNameToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.enableDisableFilterByNameToolStripMenuItem.Text = "Filter (Packet Type)";
            this.enableDisableFilterByNameToolStripMenuItem.CheckedChanged += new System.EventHandler(this.filterDisableByPacketName_CheckedChanged);
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(176, 6);
            // 
            // markToolStripMenuItem2
            // 
            this.markToolStripMenuItem2.DropDown = this.contextMenuStripMark;
            this.markToolStripMenuItem2.Name = "markToolStripMenuItem2";
            this.markToolStripMenuItem2.Size = new System.Drawing.Size(179, 22);
            this.markToolStripMenuItem2.Text = "Mark";
            // 
            // contextMenuStripMark
            // 
            this.contextMenuStripMark.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.redToolStripMenuItem2,
            this.goldToolStripMenuItem2,
            this.greenToolStripMenuItem1,
            this.blueToolStripMenuItem1,
            this.orangeToolStripMenuItem1,
            this.toolStripSeparator17,
            this.unmarkToolStripMenuItem});
            this.contextMenuStripMark.Name = "contextMenuStripMarkDropdown";
            this.contextMenuStripMark.OwnerItem = this.markToolStripMenuItem1;
            this.contextMenuStripMark.Size = new System.Drawing.Size(122, 142);
            // 
            // redToolStripMenuItem2
            // 
            this.redToolStripMenuItem2.Name = "redToolStripMenuItem2";
            this.redToolStripMenuItem2.Size = new System.Drawing.Size(121, 22);
            this.redToolStripMenuItem2.Text = "Red";
            this.redToolStripMenuItem2.Click += new System.EventHandler(this.sessionMarkSelected_Click);
            // 
            // goldToolStripMenuItem2
            // 
            this.goldToolStripMenuItem2.Name = "goldToolStripMenuItem2";
            this.goldToolStripMenuItem2.Size = new System.Drawing.Size(121, 22);
            this.goldToolStripMenuItem2.Text = "Gold";
            this.goldToolStripMenuItem2.Click += new System.EventHandler(this.sessionMarkSelected_Click);
            // 
            // greenToolStripMenuItem1
            // 
            this.greenToolStripMenuItem1.Name = "greenToolStripMenuItem1";
            this.greenToolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.greenToolStripMenuItem1.Text = "Green";
            this.greenToolStripMenuItem1.Click += new System.EventHandler(this.sessionMarkSelected_Click);
            // 
            // blueToolStripMenuItem1
            // 
            this.blueToolStripMenuItem1.Name = "blueToolStripMenuItem1";
            this.blueToolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.blueToolStripMenuItem1.Text = "Blue";
            this.blueToolStripMenuItem1.Click += new System.EventHandler(this.sessionMarkSelected_Click);
            // 
            // orangeToolStripMenuItem1
            // 
            this.orangeToolStripMenuItem1.Name = "orangeToolStripMenuItem1";
            this.orangeToolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.orangeToolStripMenuItem1.Text = "Orange";
            this.orangeToolStripMenuItem1.Click += new System.EventHandler(this.sessionMarkSelected_Click);
            // 
            // toolStripSeparator17
            // 
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            this.toolStripSeparator17.Size = new System.Drawing.Size(118, 6);
            // 
            // unmarkToolStripMenuItem
            // 
            this.unmarkToolStripMenuItem.Name = "unmarkToolStripMenuItem";
            this.unmarkToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.unmarkToolStripMenuItem.Text = "Unmark";
            this.unmarkToolStripMenuItem.Click += new System.EventHandler(this.sessionUnmarkSelected_Click);
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(176, 6);
            // 
            // findToolStripMenuItem1
            // 
            this.findToolStripMenuItem1.Name = "findToolStripMenuItem1";
            this.findToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findToolStripMenuItem1.Size = new System.Drawing.Size(179, 22);
            this.findToolStripMenuItem1.Text = "Find";
            this.findToolStripMenuItem1.Click += new System.EventHandler(this.findSessions_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "req_in.png");
            this.imageList1.Images.SetKeyName(1, "computer_go.png");
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageSummary);
            this.tabControl1.Controls.Add(this.tabPageFilters);
            this.tabControl1.Controls.Add(this.tabPageInspect);
            this.tabControl1.Controls.Add(this.tabPageInject);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Padding = new System.Drawing.Point(10, 6);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(613, 428);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPageSummary
            // 
            this.tabPageSummary.Controls.Add(this.panelStats);
            this.tabPageSummary.Location = new System.Drawing.Point(4, 28);
            this.tabPageSummary.Name = "tabPageSummary";
            this.tabPageSummary.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSummary.Size = new System.Drawing.Size(605, 396);
            this.tabPageSummary.TabIndex = 0;
            this.tabPageSummary.Text = "Summary";
            this.tabPageSummary.UseVisualStyleBackColor = true;
            // 
            // panelStats
            // 
            this.panelStats.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelStats.Controls.Add(this.groupBox2);
            this.panelStats.Controls.Add(this.groupBox1);
            this.panelStats.Location = new System.Drawing.Point(6, 6);
            this.panelStats.Name = "panelStats";
            this.panelStats.Size = new System.Drawing.Size(592, 89);
            this.panelStats.TabIndex = 2;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.labelPacketsTotal);
            this.groupBox2.Controls.Add(this.label1PacketsOut);
            this.groupBox2.Controls.Add(this.labelPacketsIn);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Location = new System.Drawing.Point(6, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(221, 80);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "UDP Packets";
            // 
            // labelPacketsTotal
            // 
            this.labelPacketsTotal.AutoSize = true;
            this.labelPacketsTotal.Location = new System.Drawing.Point(81, 58);
            this.labelPacketsTotal.Name = "labelPacketsTotal";
            this.labelPacketsTotal.Size = new System.Drawing.Size(56, 13);
            this.labelPacketsTotal.TabIndex = 8;
            this.labelPacketsTotal.Text = "0 (0 bytes)";
            // 
            // label1PacketsOut
            // 
            this.label1PacketsOut.AutoSize = true;
            this.label1PacketsOut.Location = new System.Drawing.Point(81, 37);
            this.label1PacketsOut.Name = "label1PacketsOut";
            this.label1PacketsOut.Size = new System.Drawing.Size(56, 13);
            this.label1PacketsOut.TabIndex = 7;
            this.label1PacketsOut.Text = "0 (0 bytes)";
            // 
            // labelPacketsIn
            // 
            this.labelPacketsIn.AutoSize = true;
            this.labelPacketsIn.Location = new System.Drawing.Point(81, 16);
            this.labelPacketsIn.Name = "labelPacketsIn";
            this.labelPacketsIn.Size = new System.Drawing.Size(56, 13);
            this.labelPacketsIn.TabIndex = 6;
            this.labelPacketsIn.Text = "0 (0 bytes)";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 58);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(76, 13);
            this.label10.TabIndex = 5;
            this.label10.Text = "Packets Total:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(13, 37);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(69, 13);
            this.label9.TabIndex = 4;
            this.label9.Text = "Packets Out:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(21, 16);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(61, 13);
            this.label8.TabIndex = 3;
            this.label8.Text = "Packets In:";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.labelCapsTotal);
            this.groupBox1.Controls.Add(this.labelCapsOut);
            this.groupBox1.Controls.Add(this.labelCapsIn);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Location = new System.Drawing.Point(368, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(221, 80);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Caps Messages";
            // 
            // labelCapsTotal
            // 
            this.labelCapsTotal.AutoSize = true;
            this.labelCapsTotal.Location = new System.Drawing.Point(81, 58);
            this.labelCapsTotal.Name = "labelCapsTotal";
            this.labelCapsTotal.Size = new System.Drawing.Size(56, 13);
            this.labelCapsTotal.TabIndex = 5;
            this.labelCapsTotal.Text = "0 (0 bytes)";
            // 
            // labelCapsOut
            // 
            this.labelCapsOut.AutoSize = true;
            this.labelCapsOut.Location = new System.Drawing.Point(81, 37);
            this.labelCapsOut.Name = "labelCapsOut";
            this.labelCapsOut.Size = new System.Drawing.Size(56, 13);
            this.labelCapsOut.TabIndex = 4;
            this.labelCapsOut.Text = "0 (0 bytes)";
            // 
            // labelCapsIn
            // 
            this.labelCapsIn.AutoSize = true;
            this.labelCapsIn.Location = new System.Drawing.Point(81, 16);
            this.labelCapsIn.Name = "labelCapsIn";
            this.labelCapsIn.Size = new System.Drawing.Size(56, 13);
            this.labelCapsIn.TabIndex = 3;
            this.labelCapsIn.Text = "0 (0 bytes)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(21, 37);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(54, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Caps Out:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(29, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(46, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Caps In:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(14, 58);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(61, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "Caps Total:";
            // 
            // tabPageFilters
            // 
            this.tabPageFilters.Controls.Add(this.splitContainerFilters);
            this.tabPageFilters.Location = new System.Drawing.Point(4, 28);
            this.tabPageFilters.Name = "tabPageFilters";
            this.tabPageFilters.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageFilters.Size = new System.Drawing.Size(605, 396);
            this.tabPageFilters.TabIndex = 1;
            this.tabPageFilters.Text = "Filters";
            this.tabPageFilters.UseVisualStyleBackColor = true;
            // 
            // splitContainerFilters
            // 
            this.splitContainerFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerFilters.Location = new System.Drawing.Point(3, 3);
            this.splitContainerFilters.Name = "splitContainerFilters";
            // 
            // splitContainerFilters.Panel1
            // 
            this.splitContainerFilters.Panel1.Controls.Add(this.checkBoxCheckAllPackets);
            this.splitContainerFilters.Panel1.Controls.Add(this.grpUDPFilters);
            // 
            // splitContainerFilters.Panel2
            // 
            this.splitContainerFilters.Panel2.Controls.Add(this.checkBoxCheckAllMessages);
            this.splitContainerFilters.Panel2.Controls.Add(this.grpCapsFilters);
            this.splitContainerFilters.Size = new System.Drawing.Size(599, 390);
            this.splitContainerFilters.SplitterDistance = 298;
            this.splitContainerFilters.SplitterWidth = 5;
            this.splitContainerFilters.TabIndex = 0;
            // 
            // checkBoxCheckAllPackets
            // 
            this.checkBoxCheckAllPackets.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxCheckAllPackets.AutoSize = true;
            this.checkBoxCheckAllPackets.Checked = true;
            this.checkBoxCheckAllPackets.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.checkBoxCheckAllPackets.Location = new System.Drawing.Point(6, 369);
            this.checkBoxCheckAllPackets.Name = "checkBoxCheckAllPackets";
            this.checkBoxCheckAllPackets.Size = new System.Drawing.Size(120, 17);
            this.checkBoxCheckAllPackets.TabIndex = 1;
            this.checkBoxCheckAllPackets.Text = "Check/Uncheck All";
            this.checkBoxCheckAllPackets.UseVisualStyleBackColor = true;
            this.checkBoxCheckAllPackets.CheckedChanged += new System.EventHandler(this.checkBoxCheckAllPackets_CheckedChanged);
            // 
            // grpUDPFilters
            // 
            this.grpUDPFilters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpUDPFilters.Controls.Add(this.listViewPacketFilters);
            this.grpUDPFilters.Enabled = false;
            this.grpUDPFilters.Location = new System.Drawing.Point(3, 3);
            this.grpUDPFilters.Name = "grpUDPFilters";
            this.grpUDPFilters.Size = new System.Drawing.Size(292, 357);
            this.grpUDPFilters.TabIndex = 0;
            this.grpUDPFilters.TabStop = false;
            this.grpUDPFilters.Text = "UDP Packets";
            // 
            // listViewPacketFilters
            // 
            this.listViewPacketFilters.CheckBoxes = true;
            this.listViewPacketFilters.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderPacketName,
            this.columnHeaderPacketType});
            this.listViewPacketFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewPacketFilters.FullRowSelect = true;
            this.listViewPacketFilters.GridLines = true;
            this.listViewPacketFilters.Location = new System.Drawing.Point(3, 16);
            this.listViewPacketFilters.MultiSelect = false;
            this.listViewPacketFilters.Name = "listViewPacketFilters";
            this.listViewPacketFilters.Size = new System.Drawing.Size(286, 338);
            this.listViewPacketFilters.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewPacketFilters.TabIndex = 0;
            this.listViewPacketFilters.UseCompatibleStateImageBehavior = false;
            this.listViewPacketFilters.View = System.Windows.Forms.View.Details;
            this.listViewPacketFilters.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewPacketFilters_ItemChecked);
            this.listViewPacketFilters.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewFilterSorter_ColumnClick);
            // 
            // columnHeaderPacketName
            // 
            this.columnHeaderPacketName.Text = "Packet Name";
            this.columnHeaderPacketName.Width = 215;
            // 
            // columnHeaderPacketType
            // 
            this.columnHeaderPacketType.Text = "Type";
            // 
            // checkBoxCheckAllMessages
            // 
            this.checkBoxCheckAllMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxCheckAllMessages.AutoSize = true;
            this.checkBoxCheckAllMessages.Checked = true;
            this.checkBoxCheckAllMessages.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.checkBoxCheckAllMessages.Location = new System.Drawing.Point(6, 370);
            this.checkBoxCheckAllMessages.Name = "checkBoxCheckAllMessages";
            this.checkBoxCheckAllMessages.Size = new System.Drawing.Size(120, 17);
            this.checkBoxCheckAllMessages.TabIndex = 2;
            this.checkBoxCheckAllMessages.Text = "Check/Uncheck All";
            this.checkBoxCheckAllMessages.UseVisualStyleBackColor = true;
            this.checkBoxCheckAllMessages.CheckedChanged += new System.EventHandler(this.checkBoxCheckallCaps_CheckedChanged);
            // 
            // grpCapsFilters
            // 
            this.grpCapsFilters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpCapsFilters.Controls.Add(this.listViewMessageFilters);
            this.grpCapsFilters.Enabled = false;
            this.grpCapsFilters.Location = new System.Drawing.Point(3, 3);
            this.grpCapsFilters.Name = "grpCapsFilters";
            this.grpCapsFilters.Size = new System.Drawing.Size(290, 357);
            this.grpCapsFilters.TabIndex = 1;
            this.grpCapsFilters.TabStop = false;
            this.grpCapsFilters.Text = "Capabilities Messages";
            // 
            // listViewMessageFilters
            // 
            this.listViewMessageFilters.CheckBoxes = true;
            this.listViewMessageFilters.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderMessageType});
            this.listViewMessageFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewMessageFilters.FullRowSelect = true;
            this.listViewMessageFilters.GridLines = true;
            this.listViewMessageFilters.Location = new System.Drawing.Point(3, 16);
            this.listViewMessageFilters.MultiSelect = false;
            this.listViewMessageFilters.Name = "listViewMessageFilters";
            this.listViewMessageFilters.Size = new System.Drawing.Size(284, 338);
            this.listViewMessageFilters.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewMessageFilters.TabIndex = 1;
            this.listViewMessageFilters.UseCompatibleStateImageBehavior = false;
            this.listViewMessageFilters.View = System.Windows.Forms.View.Details;
            this.listViewMessageFilters.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewMessageFilters_ItemChecked);
            this.listViewMessageFilters.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewFilterSorter_ColumnClick);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Message Name";
            this.columnHeaderName.Width = 181;
            // 
            // columnHeaderMessageType
            // 
            this.columnHeaderMessageType.Text = "Type";
            this.columnHeaderMessageType.Width = 92;
            // 
            // tabPageInspect
            // 
            this.tabPageInspect.Controls.Add(this.splitContainerInspectorTab);
            this.tabPageInspect.Location = new System.Drawing.Point(4, 28);
            this.tabPageInspect.Name = "tabPageInspect";
            this.tabPageInspect.Size = new System.Drawing.Size(605, 396);
            this.tabPageInspect.TabIndex = 3;
            this.tabPageInspect.Text = "Inspector";
            this.tabPageInspect.UseVisualStyleBackColor = true;
            // 
            // splitContainerInspectorTab
            // 
            this.splitContainerInspectorTab.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainerInspectorTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerInspectorTab.ForeColor = System.Drawing.SystemColors.ControlText;
            this.splitContainerInspectorTab.Location = new System.Drawing.Point(0, 0);
            this.splitContainerInspectorTab.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerInspectorTab.Name = "splitContainerInspectorTab";
            this.splitContainerInspectorTab.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerInspectorTab.Panel1
            // 
            this.splitContainerInspectorTab.Panel1.Controls.Add(this.tabControlInspectorRequest);
            // 
            // splitContainerInspectorTab.Panel2
            // 
            this.splitContainerInspectorTab.Panel2.Controls.Add(this.tabControlInspectorResponse);
            this.splitContainerInspectorTab.Size = new System.Drawing.Size(605, 396);
            this.splitContainerInspectorTab.SplitterDistance = 179;
            this.splitContainerInspectorTab.SplitterWidth = 5;
            this.splitContainerInspectorTab.TabIndex = 1;
            // 
            // tabControlInspectorRequest
            // 
            this.tabControlInspectorRequest.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControlInspectorRequest.Controls.Add(this.tabPageDecodedRequest);
            this.tabControlInspectorRequest.Controls.Add(this.tabPageRawRequest);
            this.tabControlInspectorRequest.Controls.Add(this.tabPageXMLRequest);
            this.tabControlInspectorRequest.Controls.Add(this.tabPageRequestJson);
            this.tabControlInspectorRequest.Controls.Add(this.tabPageHexRequest);
            this.tabControlInspectorRequest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlInspectorRequest.ImageList = this.imageList1;
            this.tabControlInspectorRequest.Location = new System.Drawing.Point(0, 0);
            this.tabControlInspectorRequest.Name = "tabControlInspectorRequest";
            this.tabControlInspectorRequest.SelectedIndex = 0;
            this.tabControlInspectorRequest.Size = new System.Drawing.Size(605, 179);
            this.tabControlInspectorRequest.TabIndex = 0;
            // 
            // tabPageDecodedRequest
            // 
            this.tabPageDecodedRequest.Controls.Add(this.richTextBoxDecodedRequest);
            this.tabPageDecodedRequest.ImageIndex = 1;
            this.tabPageDecodedRequest.Location = new System.Drawing.Point(4, 26);
            this.tabPageDecodedRequest.Name = "tabPageDecodedRequest";
            this.tabPageDecodedRequest.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDecodedRequest.Size = new System.Drawing.Size(597, 149);
            this.tabPageDecodedRequest.TabIndex = 4;
            this.tabPageDecodedRequest.Text = "Request";
            this.tabPageDecodedRequest.UseVisualStyleBackColor = true;
            // 
            // richTextBoxDecodedRequest
            // 
            this.richTextBoxDecodedRequest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxDecodedRequest.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxDecodedRequest.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxDecodedRequest.Name = "richTextBoxDecodedRequest";
            this.richTextBoxDecodedRequest.ShowSelectionMargin = true;
            this.richTextBoxDecodedRequest.Size = new System.Drawing.Size(591, 143);
            this.richTextBoxDecodedRequest.TabIndex = 0;
            this.richTextBoxDecodedRequest.Text = "";
            // 
            // tabPageRawRequest
            // 
            this.tabPageRawRequest.Controls.Add(this.richTextBoxRawRequest);
            this.tabPageRawRequest.ImageIndex = 1;
            this.tabPageRawRequest.Location = new System.Drawing.Point(4, 26);
            this.tabPageRawRequest.Name = "tabPageRawRequest";
            this.tabPageRawRequest.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageRawRequest.Size = new System.Drawing.Size(597, 149);
            this.tabPageRawRequest.TabIndex = 0;
            this.tabPageRawRequest.Text = "Raw";
            this.tabPageRawRequest.UseVisualStyleBackColor = true;
            // 
            // richTextBoxRawRequest
            // 
            this.richTextBoxRawRequest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxRawRequest.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxRawRequest.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxRawRequest.Name = "richTextBoxRawRequest";
            this.richTextBoxRawRequest.ShowSelectionMargin = true;
            this.richTextBoxRawRequest.Size = new System.Drawing.Size(591, 143);
            this.richTextBoxRawRequest.TabIndex = 1;
            this.richTextBoxRawRequest.Text = "";
            this.richTextBoxRawRequest.WordWrap = false;
            // 
            // tabPageXMLRequest
            // 
            this.tabPageXMLRequest.Controls.Add(this.treeViewXMLRequest);
            this.tabPageXMLRequest.ImageIndex = 1;
            this.tabPageXMLRequest.Location = new System.Drawing.Point(4, 26);
            this.tabPageXMLRequest.Name = "tabPageXMLRequest";
            this.tabPageXMLRequest.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageXMLRequest.Size = new System.Drawing.Size(597, 149);
            this.tabPageXMLRequest.TabIndex = 1;
            this.tabPageXMLRequest.Text = "XML";
            this.tabPageXMLRequest.UseVisualStyleBackColor = true;
            // 
            // treeViewXMLRequest
            // 
            this.treeViewXMLRequest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewXMLRequest.Location = new System.Drawing.Point(3, 3);
            this.treeViewXMLRequest.Name = "treeViewXMLRequest";
            this.treeViewXMLRequest.Size = new System.Drawing.Size(591, 143);
            this.treeViewXMLRequest.TabIndex = 1;
            // 
            // tabPageRequestJson
            // 
            this.tabPageRequestJson.Controls.Add(this.richTextBoxNotationRequest);
            this.tabPageRequestJson.ImageIndex = 1;
            this.tabPageRequestJson.Location = new System.Drawing.Point(4, 26);
            this.tabPageRequestJson.Name = "tabPageRequestJson";
            this.tabPageRequestJson.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageRequestJson.Size = new System.Drawing.Size(597, 149);
            this.tabPageRequestJson.TabIndex = 3;
            this.tabPageRequestJson.Text = "Notation";
            this.tabPageRequestJson.UseVisualStyleBackColor = true;
            // 
            // richTextBoxNotationRequest
            // 
            this.richTextBoxNotationRequest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxNotationRequest.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxNotationRequest.Name = "richTextBoxNotationRequest";
            this.richTextBoxNotationRequest.ShowSelectionMargin = true;
            this.richTextBoxNotationRequest.Size = new System.Drawing.Size(591, 143);
            this.richTextBoxNotationRequest.TabIndex = 0;
            this.richTextBoxNotationRequest.Text = "";
            // 
            // tabPageHexRequest
            // 
            this.tabPageHexRequest.Controls.Add(this.statusStrip2);
            this.tabPageHexRequest.Controls.Add(this.hexBoxRequest);
            this.tabPageHexRequest.ImageIndex = 1;
            this.tabPageHexRequest.Location = new System.Drawing.Point(4, 26);
            this.tabPageHexRequest.Name = "tabPageHexRequest";
            this.tabPageHexRequest.Size = new System.Drawing.Size(597, 149);
            this.tabPageHexRequest.TabIndex = 2;
            this.tabPageHexRequest.Text = "Hex";
            this.tabPageHexRequest.UseVisualStyleBackColor = true;
            // 
            // statusStrip2
            // 
            this.statusStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelRequestHex});
            this.statusStrip2.Location = new System.Drawing.Point(0, 127);
            this.statusStrip2.Name = "statusStrip2";
            this.statusStrip2.Size = new System.Drawing.Size(585, 22);
            this.statusStrip2.TabIndex = 3;
            this.statusStrip2.Text = "statusStrip2";
            this.statusStrip2.Visible = false;
            // 
            // labelRequestHex
            // 
            this.labelRequestHex.Name = "labelRequestHex";
            this.labelRequestHex.Size = new System.Drawing.Size(0, 17);
            // 
            // hexBoxRequest
            // 
            this.hexBoxRequest.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hexBoxRequest.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hexBoxRequest.LineInfoForeColor = System.Drawing.Color.Empty;
            this.hexBoxRequest.Location = new System.Drawing.Point(3, 3);
            this.hexBoxRequest.Name = "hexBoxRequest";
            this.hexBoxRequest.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
            this.hexBoxRequest.Size = new System.Drawing.Size(591, 120);
            this.hexBoxRequest.StringViewVisible = true;
            this.hexBoxRequest.TabIndex = 2;
            this.hexBoxRequest.UseFixedBytesPerLine = true;
            this.hexBoxRequest.VScrollBarVisible = true;
            this.hexBoxRequest.CurrentPositionInLineChanged += new System.EventHandler(this.RequestPosition_Changed);
            this.hexBoxRequest.CurrentLineChanged += new System.EventHandler(this.RequestPosition_Changed);
            // 
            // tabControlInspectorResponse
            // 
            this.tabControlInspectorResponse.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControlInspectorResponse.Controls.Add(this.tabPageDecodeResponse);
            this.tabControlInspectorResponse.Controls.Add(this.tabPageInspectorRAWResponse);
            this.tabControlInspectorResponse.Controls.Add(this.tabPageInspectorXMLResponse);
            this.tabControlInspectorResponse.Controls.Add(this.tabPageResponseJson);
            this.tabControlInspectorResponse.Controls.Add(this.tabPageHexViewResponse);
            this.tabControlInspectorResponse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlInspectorResponse.ImageList = this.imageList1;
            this.tabControlInspectorResponse.Location = new System.Drawing.Point(0, 0);
            this.tabControlInspectorResponse.Multiline = true;
            this.tabControlInspectorResponse.Name = "tabControlInspectorResponse";
            this.tabControlInspectorResponse.SelectedIndex = 0;
            this.tabControlInspectorResponse.Size = new System.Drawing.Size(605, 212);
            this.tabControlInspectorResponse.TabIndex = 0;
            // 
            // tabPageDecodeResponse
            // 
            this.tabPageDecodeResponse.Controls.Add(this.richTextBoxDecodedResponse);
            this.tabPageDecodeResponse.ImageIndex = 0;
            this.tabPageDecodeResponse.Location = new System.Drawing.Point(4, 26);
            this.tabPageDecodeResponse.Name = "tabPageDecodeResponse";
            this.tabPageDecodeResponse.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDecodeResponse.Size = new System.Drawing.Size(597, 182);
            this.tabPageDecodeResponse.TabIndex = 6;
            this.tabPageDecodeResponse.Text = "Response";
            this.tabPageDecodeResponse.UseVisualStyleBackColor = true;
            // 
            // richTextBoxDecodedResponse
            // 
            this.richTextBoxDecodedResponse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxDecodedResponse.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxDecodedResponse.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxDecodedResponse.Name = "richTextBoxDecodedResponse";
            this.richTextBoxDecodedResponse.ShowSelectionMargin = true;
            this.richTextBoxDecodedResponse.Size = new System.Drawing.Size(591, 176);
            this.richTextBoxDecodedResponse.TabIndex = 0;
            this.richTextBoxDecodedResponse.Text = "";
            // 
            // tabPageInspectorRAWResponse
            // 
            this.tabPageInspectorRAWResponse.Controls.Add(this.richTextBoxRawResponse);
            this.tabPageInspectorRAWResponse.ImageIndex = 0;
            this.tabPageInspectorRAWResponse.Location = new System.Drawing.Point(4, 26);
            this.tabPageInspectorRAWResponse.Name = "tabPageInspectorRAWResponse";
            this.tabPageInspectorRAWResponse.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageInspectorRAWResponse.Size = new System.Drawing.Size(597, 182);
            this.tabPageInspectorRAWResponse.TabIndex = 0;
            this.tabPageInspectorRAWResponse.Text = "Raw";
            this.tabPageInspectorRAWResponse.UseVisualStyleBackColor = true;
            // 
            // richTextBoxRawResponse
            // 
            this.richTextBoxRawResponse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxRawResponse.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxRawResponse.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxRawResponse.Name = "richTextBoxRawResponse";
            this.richTextBoxRawResponse.ShowSelectionMargin = true;
            this.richTextBoxRawResponse.Size = new System.Drawing.Size(591, 176);
            this.richTextBoxRawResponse.TabIndex = 0;
            this.richTextBoxRawResponse.Text = "";
            this.richTextBoxRawResponse.WordWrap = false;
            // 
            // tabPageInspectorXMLResponse
            // 
            this.tabPageInspectorXMLResponse.Controls.Add(this.treeViewXmlResponse);
            this.tabPageInspectorXMLResponse.ImageIndex = 0;
            this.tabPageInspectorXMLResponse.Location = new System.Drawing.Point(4, 26);
            this.tabPageInspectorXMLResponse.Name = "tabPageInspectorXMLResponse";
            this.tabPageInspectorXMLResponse.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageInspectorXMLResponse.Size = new System.Drawing.Size(597, 182);
            this.tabPageInspectorXMLResponse.TabIndex = 1;
            this.tabPageInspectorXMLResponse.Text = "XML";
            this.tabPageInspectorXMLResponse.UseVisualStyleBackColor = true;
            // 
            // treeViewXmlResponse
            // 
            this.treeViewXmlResponse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewXmlResponse.Location = new System.Drawing.Point(3, 3);
            this.treeViewXmlResponse.Name = "treeViewXmlResponse";
            this.treeViewXmlResponse.Size = new System.Drawing.Size(591, 176);
            this.treeViewXmlResponse.TabIndex = 0;
            // 
            // tabPageResponseJson
            // 
            this.tabPageResponseJson.Controls.Add(this.richTextBoxNotationResponse);
            this.tabPageResponseJson.ImageIndex = 0;
            this.tabPageResponseJson.Location = new System.Drawing.Point(4, 26);
            this.tabPageResponseJson.Name = "tabPageResponseJson";
            this.tabPageResponseJson.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageResponseJson.Size = new System.Drawing.Size(597, 182);
            this.tabPageResponseJson.TabIndex = 5;
            this.tabPageResponseJson.Text = "Notation";
            this.tabPageResponseJson.UseVisualStyleBackColor = true;
            // 
            // richTextBoxNotationResponse
            // 
            this.richTextBoxNotationResponse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxNotationResponse.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxNotationResponse.Name = "richTextBoxNotationResponse";
            this.richTextBoxNotationResponse.ShowSelectionMargin = true;
            this.richTextBoxNotationResponse.Size = new System.Drawing.Size(591, 176);
            this.richTextBoxNotationResponse.TabIndex = 0;
            this.richTextBoxNotationResponse.Text = "";
            // 
            // tabPageHexViewResponse
            // 
            this.tabPageHexViewResponse.Controls.Add(this.statusStrip1);
            this.tabPageHexViewResponse.Controls.Add(this.hexBoxResponse);
            this.tabPageHexViewResponse.ImageIndex = 0;
            this.tabPageHexViewResponse.Location = new System.Drawing.Point(4, 26);
            this.tabPageHexViewResponse.Name = "tabPageHexViewResponse";
            this.tabPageHexViewResponse.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageHexViewResponse.Size = new System.Drawing.Size(597, 182);
            this.tabPageHexViewResponse.TabIndex = 4;
            this.tabPageHexViewResponse.Text = "Hex";
            this.tabPageHexViewResponse.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelResponseHex});
            this.statusStrip1.Location = new System.Drawing.Point(3, 157);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(591, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // labelResponseHex
            // 
            this.labelResponseHex.Name = "labelResponseHex";
            this.labelResponseHex.Size = new System.Drawing.Size(0, 17);
            // 
            // hexBoxResponse
            // 
            this.hexBoxResponse.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hexBoxResponse.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hexBoxResponse.LineInfoForeColor = System.Drawing.Color.Empty;
            this.hexBoxResponse.Location = new System.Drawing.Point(3, 3);
            this.hexBoxResponse.Name = "hexBoxResponse";
            this.hexBoxResponse.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
            this.hexBoxResponse.Size = new System.Drawing.Size(591, 152);
            this.hexBoxResponse.StringViewVisible = true;
            this.hexBoxResponse.TabIndex = 1;
            this.hexBoxResponse.UseFixedBytesPerLine = true;
            this.hexBoxResponse.VScrollBarVisible = true;
            this.hexBoxResponse.CurrentPositionInLineChanged += new System.EventHandler(this.ReplyPosition_Changed);
            this.hexBoxResponse.CurrentLineChanged += new System.EventHandler(this.ReplyPosition_Changed);
            // 
            // tabPageInject
            // 
            this.tabPageInject.Controls.Add(this.radioButtonViewer);
            this.tabPageInject.Controls.Add(this.radioButtonSimulator);
            this.tabPageInject.Controls.Add(this.button3);
            this.tabPageInject.Controls.Add(this.buttonInjectPacket);
            this.tabPageInject.Controls.Add(this.richTextBoxInject);
            this.tabPageInject.Location = new System.Drawing.Point(4, 28);
            this.tabPageInject.Name = "tabPageInject";
            this.tabPageInject.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageInject.Size = new System.Drawing.Size(605, 396);
            this.tabPageInject.TabIndex = 2;
            this.tabPageInject.Text = "Inject";
            this.tabPageInject.UseVisualStyleBackColor = true;
            // 
            // radioButtonViewer
            // 
            this.radioButtonViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonViewer.AutoSize = true;
            this.radioButtonViewer.Location = new System.Drawing.Point(397, 370);
            this.radioButtonViewer.Name = "radioButtonViewer";
            this.radioButtonViewer.Size = new System.Drawing.Size(97, 17);
            this.radioButtonViewer.TabIndex = 4;
            this.radioButtonViewer.Text = "Send to Viewer";
            this.radioButtonViewer.UseVisualStyleBackColor = true;
            // 
            // radioButtonSimulator
            // 
            this.radioButtonSimulator.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonSimulator.AutoSize = true;
            this.radioButtonSimulator.Checked = true;
            this.radioButtonSimulator.Location = new System.Drawing.Point(283, 370);
            this.radioButtonSimulator.Name = "radioButtonSimulator";
            this.radioButtonSimulator.Size = new System.Drawing.Size(108, 17);
            this.radioButtonSimulator.TabIndex = 3;
            this.radioButtonSimulator.TabStop = true;
            this.radioButtonSimulator.Text = "Send to Simulator";
            this.radioButtonSimulator.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button3.Enabled = false;
            this.button3.Location = new System.Drawing.Point(6, 367);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(105, 23);
            this.button3.TabIndex = 2;
            this.button3.Text = "Packet Builder";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // buttonInjectPacket
            // 
            this.buttonInjectPacket.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInjectPacket.Enabled = false;
            this.buttonInjectPacket.Location = new System.Drawing.Point(522, 367);
            this.buttonInjectPacket.Name = "buttonInjectPacket";
            this.buttonInjectPacket.Size = new System.Drawing.Size(75, 23);
            this.buttonInjectPacket.TabIndex = 1;
            this.buttonInjectPacket.Text = "Inject";
            this.buttonInjectPacket.UseVisualStyleBackColor = true;
            this.buttonInjectPacket.Click += new System.EventHandler(this.buttonInjectPacket_Click);
            // 
            // richTextBoxInject
            // 
            this.richTextBoxInject.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxInject.Location = new System.Drawing.Point(6, 6);
            this.richTextBoxInject.Name = "richTextBoxInject";
            this.richTextBoxInject.Size = new System.Drawing.Size(591, 355);
            this.richTextBoxInject.TabIndex = 0;
            this.richTextBoxInject.Text = "";
            this.richTextBoxInject.TextChanged += new System.EventHandler(this.richTextBoxInject_TextChanged);
            // 
            // markToolStripMenuItem1
            // 
            this.markToolStripMenuItem1.DropDown = this.contextMenuStripMark;
            this.markToolStripMenuItem1.Name = "markToolStripMenuItem1";
            this.markToolStripMenuItem1.Size = new System.Drawing.Size(143, 22);
            this.markToolStripMenuItem1.Text = "Mark";
            // 
            // toolStripLabelHexEditorRequest
            // 
            this.toolStripLabelHexEditorRequest.Name = "toolStripLabelHexEditorRequest";
            this.toolStripLabelHexEditorRequest.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripFileMenu,
            this.EditToolStripButton,
            this.toolStripDropDownButton5,
            this.toolStripDropDownButton4});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1111, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripFileMenu
            // 
            this.toolStripFileMenu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripFileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.captureToolStripMenuItem,
            this.toolStripSeparator7,
            this.saveSessionArchiveToolStripMenuItem,
            this.loadSessionArchiveToolStripMenuItem,
            this.toolStripSeparator8,
            this.settingsToolStripMenuItem,
            this.toolStripSeparator9,
            this.toolStripMenuItem1,
            this.toolStripSeparator14,
            this.exitToolStripMenuItem1});
            this.toolStripFileMenu.Image = ((System.Drawing.Image)(resources.GetObject("toolStripFileMenu.Image")));
            this.toolStripFileMenu.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripFileMenu.Name = "toolStripFileMenu";
            this.toolStripFileMenu.ShowDropDownArrow = false;
            this.toolStripFileMenu.Size = new System.Drawing.Size(27, 22);
            this.toolStripFileMenu.Text = "&File";
            this.toolStripFileMenu.DropDownOpening += new System.EventHandler(this.EditToolStripButton_DropDownOpening);
            // 
            // captureToolStripMenuItem
            // 
            this.captureToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.captureToolStripMenuItem.Name = "captureToolStripMenuItem";
            this.captureToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F12;
            this.captureToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.captureToolStripMenuItem.Text = "Capture";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(184, 6);
            // 
            // saveSessionArchiveToolStripMenuItem
            // 
            this.saveSessionArchiveToolStripMenuItem.Name = "saveSessionArchiveToolStripMenuItem";
            this.saveSessionArchiveToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.saveSessionArchiveToolStripMenuItem.Text = "Save Session Archive";
            this.saveSessionArchiveToolStripMenuItem.Click += new System.EventHandler(this.saveSessionArchiveToolStripMenuItem_Click);
            // 
            // loadSessionArchiveToolStripMenuItem
            // 
            this.loadSessionArchiveToolStripMenuItem.Name = "loadSessionArchiveToolStripMenuItem";
            this.loadSessionArchiveToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.loadSessionArchiveToolStripMenuItem.Text = "Load Session Archive";
            this.loadSessionArchiveToolStripMenuItem.Click += new System.EventHandler(this.loadSessionArchiveToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(184, 6);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autoScrollSessionsToolStripMenuItem,
            this.enableStatisticsToolStripMenuItem,
            this.saveOptionsOnExitToolStripMenuItem,
            this.startProxyOnStartupToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // autoScrollSessionsToolStripMenuItem
            // 
            this.autoScrollSessionsToolStripMenuItem.CheckOnClick = true;
            this.autoScrollSessionsToolStripMenuItem.Name = "autoScrollSessionsToolStripMenuItem";
            this.autoScrollSessionsToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.autoScrollSessionsToolStripMenuItem.Text = "Auto Scroll Sessions";
            this.autoScrollSessionsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.sessionEnableAutoScroll_CheckedChanged);
            // 
            // enableStatisticsToolStripMenuItem
            // 
            this.enableStatisticsToolStripMenuItem.Checked = true;
            this.enableStatisticsToolStripMenuItem.CheckOnClick = true;
            this.enableStatisticsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableStatisticsToolStripMenuItem.Name = "enableStatisticsToolStripMenuItem";
            this.enableStatisticsToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.enableStatisticsToolStripMenuItem.Text = "Enable Statistics";
            this.enableStatisticsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.enableStatisticsToolStripMenuItem_CheckedChanged);
            // 
            // saveOptionsOnExitToolStripMenuItem
            // 
            this.saveOptionsOnExitToolStripMenuItem.Checked = true;
            this.saveOptionsOnExitToolStripMenuItem.CheckOnClick = true;
            this.saveOptionsOnExitToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.saveOptionsOnExitToolStripMenuItem.Name = "saveOptionsOnExitToolStripMenuItem";
            this.saveOptionsOnExitToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.saveOptionsOnExitToolStripMenuItem.Text = "Save Current Profile on Exit";
            // 
            // startProxyOnStartupToolStripMenuItem
            // 
            this.startProxyOnStartupToolStripMenuItem.Checked = true;
            this.startProxyOnStartupToolStripMenuItem.CheckOnClick = true;
            this.startProxyOnStartupToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.startProxyOnStartupToolStripMenuItem.Name = "startProxyOnStartupToolStripMenuItem";
            this.startProxyOnStartupToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.startProxyOnStartupToolStripMenuItem.Text = "Start Proxy on Startup";
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(184, 6);
            // 
            // exitToolStripMenuItem1
            // 
            this.exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
            this.exitToolStripMenuItem1.Size = new System.Drawing.Size(187, 22);
            this.exitToolStripMenuItem1.Text = "Exit";
            this.exitToolStripMenuItem1.Click += new System.EventHandler(this.exitToolStripMenuItem1_Click);
            // 
            // EditToolStripButton
            // 
            this.EditToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.EditToolStripButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem1,
            this.removeToolStripMenuItem2,
            this.selectToolStripMenuItem1,
            this.toolStripSeparator10,
            this.markToolStripMenuItem1,
            this.toolStripSeparator12,
            this.findToolStripMenuItem});
            this.EditToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("EditToolStripButton.Image")));
            this.EditToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.EditToolStripButton.Name = "EditToolStripButton";
            this.EditToolStripButton.ShowDropDownArrow = false;
            this.EditToolStripButton.Size = new System.Drawing.Size(29, 22);
            this.EditToolStripButton.Text = "&Edit";
            this.EditToolStripButton.DropDownOpening += new System.EventHandler(this.EditToolStripButton_DropDownOpening);
            // 
            // copyToolStripMenuItem1
            // 
            this.copyToolStripMenuItem1.DropDown = this.contextMenuStripCopy;
            this.copyToolStripMenuItem1.Name = "copyToolStripMenuItem1";
            this.copyToolStripMenuItem1.Size = new System.Drawing.Size(143, 22);
            this.copyToolStripMenuItem1.Text = "Copy";
            this.copyToolStripMenuItem1.Visible = false;
            // 
            // contextMenuStripCopy
            // 
            this.contextMenuStripCopy.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.requestDataToolStripMenuItem,
            this.responseDataToolStripMenuItem,
            this.hostAddressToolStripMenuItem,
            this.packetMessageTypeToolStripMenuItem});
            this.contextMenuStripCopy.Name = "contextMenuStripCopy";
            this.contextMenuStripCopy.OwnerItem = this.copyToolStripMenuItem1;
            this.contextMenuStripCopy.Size = new System.Drawing.Size(194, 92);
            // 
            // requestDataToolStripMenuItem
            // 
            this.requestDataToolStripMenuItem.Name = "requestDataToolStripMenuItem";
            this.requestDataToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.requestDataToolStripMenuItem.Text = "Request Data";
            // 
            // responseDataToolStripMenuItem
            // 
            this.responseDataToolStripMenuItem.Name = "responseDataToolStripMenuItem";
            this.responseDataToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.responseDataToolStripMenuItem.Text = "Response Data";
            // 
            // hostAddressToolStripMenuItem
            // 
            this.hostAddressToolStripMenuItem.Name = "hostAddressToolStripMenuItem";
            this.hostAddressToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.hostAddressToolStripMenuItem.Text = "Host/Address";
            // 
            // packetMessageTypeToolStripMenuItem
            // 
            this.packetMessageTypeToolStripMenuItem.Name = "packetMessageTypeToolStripMenuItem";
            this.packetMessageTypeToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.packetMessageTypeToolStripMenuItem.Text = "Packet/Message Name";
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(140, 6);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(140, 6);
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.findToolStripMenuItem.Text = "Find";
            this.findToolStripMenuItem.Click += new System.EventHandler(this.findSessions_Click);
            // 
            // toolStripDropDownButton5
            // 
            this.toolStripDropDownButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton5.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveFilterSelectionsToolStripMenuItem,
            this.loadFilterSelectionsToolStripMenuItem,
            this.toolStripSeparator6,
            this.optionsToolStripMenuItem});
            this.toolStripDropDownButton5.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton5.Image")));
            this.toolStripDropDownButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton5.Name = "toolStripDropDownButton5";
            this.toolStripDropDownButton5.ShowDropDownArrow = false;
            this.toolStripDropDownButton5.Size = new System.Drawing.Size(40, 22);
            this.toolStripDropDownButton5.Text = "F&ilters";
            this.toolStripDropDownButton5.DropDownOpening += new System.EventHandler(this.EditToolStripButton_DropDownOpening);
            // 
            // saveFilterSelectionsToolStripMenuItem
            // 
            this.saveFilterSelectionsToolStripMenuItem.Name = "saveFilterSelectionsToolStripMenuItem";
            this.saveFilterSelectionsToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.saveFilterSelectionsToolStripMenuItem.Text = "Save Filter Selections";
            this.saveFilterSelectionsToolStripMenuItem.Click += new System.EventHandler(this.saveFilterSelectionsToolStripMenuItem_Click);
            // 
            // loadFilterSelectionsToolStripMenuItem
            // 
            this.loadFilterSelectionsToolStripMenuItem.Name = "loadFilterSelectionsToolStripMenuItem";
            this.loadFilterSelectionsToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.loadFilterSelectionsToolStripMenuItem.Text = "Load Filter Selections";
            this.loadFilterSelectionsToolStripMenuItem.Click += new System.EventHandler(this.loadFilterSelectionsToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(184, 6);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autoAddNewDiscoveredMessagesToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // autoAddNewDiscoveredMessagesToolStripMenuItem
            // 
            this.autoAddNewDiscoveredMessagesToolStripMenuItem.Checked = true;
            this.autoAddNewDiscoveredMessagesToolStripMenuItem.CheckOnClick = true;
            this.autoAddNewDiscoveredMessagesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoAddNewDiscoveredMessagesToolStripMenuItem.Name = "autoAddNewDiscoveredMessagesToolStripMenuItem";
            this.autoAddNewDiscoveredMessagesToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.autoAddNewDiscoveredMessagesToolStripMenuItem.Text = "Autocheck new Capabilities";
            // 
            // toolStripDropDownButton4
            // 
            this.toolStripDropDownButton4.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.toolStripDropDownButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton4.Name = "toolStripDropDownButton4";
            this.toolStripDropDownButton4.ShowDropDownArrow = false;
            this.toolStripDropDownButton4.Size = new System.Drawing.Size(32, 22);
            this.toolStripDropDownButton4.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutWinGridProxyToolStripMenuItem_Click);
            // 
            // aboutWinGridProxyToolStripMenuItem
            // 
            this.aboutWinGridProxyToolStripMenuItem.Name = "aboutWinGridProxyToolStripMenuItem";
            this.aboutWinGridProxyToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.aboutWinGridProxyToolStripMenuItem.Text = "About WinGridProxy";
            this.aboutWinGridProxyToolStripMenuItem.Click += new System.EventHandler(this.aboutWinGridProxyToolStripMenuItem_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.captureTrafficToolStripMenuItem,
            this.toolStripSeparator1,
            this.saveSessionsToolStripMenuItem,
            this.loadSessionsToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(36, 22);
            this.toolStripDropDownButton1.Text = "File";
            // 
            // captureTrafficToolStripMenuItem
            // 
            this.captureTrafficToolStripMenuItem.Checked = true;
            this.captureTrafficToolStripMenuItem.CheckOnClick = true;
            this.captureTrafficToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.captureTrafficToolStripMenuItem.Name = "captureTrafficToolStripMenuItem";
            this.captureTrafficToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F12;
            this.captureTrafficToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.captureTrafficToolStripMenuItem.Text = "Capture Traffic";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(180, 6);
            // 
            // saveSessionsToolStripMenuItem
            // 
            this.saveSessionsToolStripMenuItem.Name = "saveSessionsToolStripMenuItem";
            this.saveSessionsToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.saveSessionsToolStripMenuItem.Text = "Save Sessions";
            // 
            // loadSessionsToolStripMenuItem
            // 
            this.loadSessionsToolStripMenuItem.Name = "loadSessionsToolStripMenuItem";
            this.loadSessionsToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.loadSessionsToolStripMenuItem.Text = "Load Sessions";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(180, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // toolStripDropDownButton2
            // 
            this.toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.removeToolStripMenuItem,
            this.selectToolStripMenuItem,
            this.toolStripSeparator3,
            this.markToolStripMenuItem,
            this.toolStripSeparator4,
            this.findSessionToolStripMenuItem});
            this.toolStripDropDownButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton2.Image")));
            this.toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            this.toolStripDropDownButton2.Size = new System.Drawing.Size(38, 22);
            this.toolStripDropDownButton2.Text = "Edit";
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allToolStripMenuItem,
            this.selectedToolStripMenuItem,
            this.unselectedSessionsToolStripMenuItem});
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            // 
            // allToolStripMenuItem
            // 
            this.allToolStripMenuItem.Name = "allToolStripMenuItem";
            this.allToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.allToolStripMenuItem.Text = "All";
            // 
            // selectedToolStripMenuItem
            // 
            this.selectedToolStripMenuItem.Name = "selectedToolStripMenuItem";
            this.selectedToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.selectedToolStripMenuItem.Text = "Selected Sessions";
            // 
            // unselectedSessionsToolStripMenuItem
            // 
            this.unselectedSessionsToolStripMenuItem.Name = "unselectedSessionsToolStripMenuItem";
            this.unselectedSessionsToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.unselectedSessionsToolStripMenuItem.Text = "Unselected Sessions";
            // 
            // selectToolStripMenuItem
            // 
            this.selectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allToolStripMenuItem1,
            this.noneToolStripMenuItem,
            this.invertSelectionsToolStripMenuItem});
            this.selectToolStripMenuItem.Name = "selectToolStripMenuItem";
            this.selectToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.selectToolStripMenuItem.Text = "Select";
            // 
            // allToolStripMenuItem1
            // 
            this.allToolStripMenuItem1.Name = "allToolStripMenuItem1";
            this.allToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.allToolStripMenuItem1.Size = new System.Drawing.Size(166, 22);
            this.allToolStripMenuItem1.Text = "All";
            this.allToolStripMenuItem1.Click += new System.EventHandler(this.sessionSelectAll_Click);
            // 
            // noneToolStripMenuItem
            // 
            this.noneToolStripMenuItem.Name = "noneToolStripMenuItem";
            this.noneToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.noneToolStripMenuItem.Text = "None";
            this.noneToolStripMenuItem.Click += new System.EventHandler(this.sessionSelectNone_Click);
            // 
            // invertSelectionsToolStripMenuItem
            // 
            this.invertSelectionsToolStripMenuItem.Name = "invertSelectionsToolStripMenuItem";
            this.invertSelectionsToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.invertSelectionsToolStripMenuItem.Text = "Invert Selections";
            this.invertSelectionsToolStripMenuItem.Click += new System.EventHandler(this.sessionInvertSelection_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(179, 6);
            // 
            // markToolStripMenuItem
            // 
            this.markToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.redToolStripMenuItem,
            this.goldToolStripMenuItem,
            this.orangeToolStripMenuItem,
            this.purpleToolStripMenuItem,
            this.yellowToolStripMenuItem,
            this.toolStripSeparator5,
            this.removeToolStripMenuItem1});
            this.markToolStripMenuItem.Name = "markToolStripMenuItem";
            this.markToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.markToolStripMenuItem.Text = "Mark";
            // 
            // redToolStripMenuItem
            // 
            this.redToolStripMenuItem.Name = "redToolStripMenuItem";
            this.redToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.redToolStripMenuItem.Text = "Red";
            this.redToolStripMenuItem.Click += new System.EventHandler(this.sessionMarkSelected_Click);
            // 
            // goldToolStripMenuItem
            // 
            this.goldToolStripMenuItem.Name = "goldToolStripMenuItem";
            this.goldToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.goldToolStripMenuItem.Text = "Gold";
            this.goldToolStripMenuItem.Click += new System.EventHandler(this.sessionMarkSelected_Click);
            // 
            // orangeToolStripMenuItem
            // 
            this.orangeToolStripMenuItem.Name = "orangeToolStripMenuItem";
            this.orangeToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.orangeToolStripMenuItem.Text = "Orange";
            this.orangeToolStripMenuItem.Click += new System.EventHandler(this.sessionMarkSelected_Click);
            // 
            // purpleToolStripMenuItem
            // 
            this.purpleToolStripMenuItem.Name = "purpleToolStripMenuItem";
            this.purpleToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.purpleToolStripMenuItem.Text = "Purple";
            this.purpleToolStripMenuItem.Click += new System.EventHandler(this.sessionMarkSelected_Click);
            // 
            // yellowToolStripMenuItem
            // 
            this.yellowToolStripMenuItem.Name = "yellowToolStripMenuItem";
            this.yellowToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.yellowToolStripMenuItem.Text = "Yellow";
            this.yellowToolStripMenuItem.Click += new System.EventHandler(this.sessionMarkSelected_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(121, 6);
            // 
            // removeToolStripMenuItem1
            // 
            this.removeToolStripMenuItem1.Name = "removeToolStripMenuItem1";
            this.removeToolStripMenuItem1.Size = new System.Drawing.Size(124, 22);
            this.removeToolStripMenuItem1.Text = "Remove";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(179, 6);
            // 
            // findSessionToolStripMenuItem
            // 
            this.findSessionToolStripMenuItem.Name = "findSessionToolStripMenuItem";
            this.findSessionToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findSessionToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.findSessionToolStripMenuItem.Text = "Find Session";
            // 
            // toolStripDropDownButton3
            // 
            this.toolStripDropDownButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton3.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.loadToolStripMenuItem});
            this.toolStripDropDownButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton3.Image")));
            this.toolStripDropDownButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton3.Name = "toolStripDropDownButton3";
            this.toolStripDropDownButton3.Size = new System.Drawing.Size(49, 22);
            this.toolStripDropDownButton3.Text = "Filters";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.saveToolStripMenuItem.Text = "Save Selections";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.loadToolStripMenuItem.Text = "Load Selections";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(29, 22);
            this.toolStripButton1.Text = "toolStripButton1";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "osd";
            this.saveFileDialog1.FileName = "sessions";
            this.saveFileDialog1.Filter = "Session Files|*.osd|All Files|*.*";
            this.saveFileDialog1.RestoreDirectory = true;
            this.saveFileDialog1.Title = "Save Sessions";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "osd";
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Session Files|*.osd|All Files|*.*";
            this.openFileDialog1.Title = "Open Session Archive";
            // 
            // saveFileDialog2
            // 
            this.saveFileDialog2.DefaultExt = "osd";
            this.saveFileDialog2.FileName = "filters";
            this.saveFileDialog2.Title = "Save Filters";
            // 
            // openFileDialog2
            // 
            this.openFileDialog2.DefaultExt = "osd";
            this.openFileDialog2.FileName = "openFileDialog2";
            this.openFileDialog2.Filter = "Filter Files|*.osd|All Files|*.*";
            this.openFileDialog2.Title = "Load Saved Filter Settings";
            // 
            // statusStrip3
            // 
            this.statusStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMainLabel});
            this.statusStrip3.Location = new System.Drawing.Point(0, 497);
            this.statusStrip3.Name = "statusStrip3";
            this.statusStrip3.Size = new System.Drawing.Size(1111, 22);
            this.statusStrip3.TabIndex = 5;
            this.statusStrip3.Text = "statusStrip3";
            // 
            // toolStripMainLabel
            // 
            this.toolStripMainLabel.Name = "toolStripMainLabel";
            this.toolStripMainLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // contextMenuStripFilterOptions
            // 
            this.contextMenuStripFilterOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.uncheckAllToolStripMenuItem,
            this.toolStripSeparator11,
            this.autoColorizeToolStripMenuItem});
            this.contextMenuStripFilterOptions.Name = "contextMenuStripFilterOptions";
            this.contextMenuStripFilterOptions.ShowImageMargin = false;
            this.contextMenuStripFilterOptions.Size = new System.Drawing.Size(125, 76);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(124, 22);
            this.toolStripMenuItem2.Text = "Check All";
            // 
            // uncheckAllToolStripMenuItem
            // 
            this.uncheckAllToolStripMenuItem.Name = "uncheckAllToolStripMenuItem";
            this.uncheckAllToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.uncheckAllToolStripMenuItem.Text = "Uncheck All";
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(121, 6);
            // 
            // autoColorizeToolStripMenuItem
            // 
            this.autoColorizeToolStripMenuItem.Name = "autoColorizeToolStripMenuItem";
            this.autoColorizeToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.autoColorizeToolStripMenuItem.Text = "Auto Colorize";
            this.autoColorizeToolStripMenuItem.Click += new System.EventHandler(this.autoColorizeToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(187, 22);
            this.toolStripMenuItem1.Text = "Plugins";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(184, 6);
            // 
            // FormWinGridProxy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1111, 519);
            this.Controls.Add(this.statusStrip3);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.panelMainWindow);
            this.Controls.Add(this.panelProxyConfig);
            this.Name = "FormWinGridProxy";
            this.Text = "WinGridProxy";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.panelProxyConfig.ResumeLayout(false);
            this.panelProxyConfig.PerformLayout();
            this.panelMainWindow.ResumeLayout(false);
            this.splitContainerSessionsTabs.Panel1.ResumeLayout(false);
            this.splitContainerSessionsTabs.Panel2.ResumeLayout(false);
            this.splitContainerSessionsTabs.ResumeLayout(false);
            this.contextMenuStripSessions.ResumeLayout(false);
            this.contextMenuStripRemove.ResumeLayout(false);
            this.contextMenuStripSelect.ResumeLayout(false);
            this.contextMenuStripMark.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPageSummary.ResumeLayout(false);
            this.panelStats.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPageFilters.ResumeLayout(false);
            this.splitContainerFilters.Panel1.ResumeLayout(false);
            this.splitContainerFilters.Panel1.PerformLayout();
            this.splitContainerFilters.Panel2.ResumeLayout(false);
            this.splitContainerFilters.Panel2.PerformLayout();
            this.splitContainerFilters.ResumeLayout(false);
            this.grpUDPFilters.ResumeLayout(false);
            this.grpCapsFilters.ResumeLayout(false);
            this.tabPageInspect.ResumeLayout(false);
            this.splitContainerInspectorTab.Panel1.ResumeLayout(false);
            this.splitContainerInspectorTab.Panel2.ResumeLayout(false);
            this.splitContainerInspectorTab.ResumeLayout(false);
            this.tabControlInspectorRequest.ResumeLayout(false);
            this.tabPageDecodedRequest.ResumeLayout(false);
            this.tabPageRawRequest.ResumeLayout(false);
            this.tabPageXMLRequest.ResumeLayout(false);
            this.tabPageRequestJson.ResumeLayout(false);
            this.tabPageHexRequest.ResumeLayout(false);
            this.tabPageHexRequest.PerformLayout();
            this.statusStrip2.ResumeLayout(false);
            this.statusStrip2.PerformLayout();
            this.tabControlInspectorResponse.ResumeLayout(false);
            this.tabPageDecodeResponse.ResumeLayout(false);
            this.tabPageInspectorRAWResponse.ResumeLayout(false);
            this.tabPageInspectorXMLResponse.ResumeLayout(false);
            this.tabPageResponseJson.ResumeLayout(false);
            this.tabPageHexViewResponse.ResumeLayout(false);
            this.tabPageHexViewResponse.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabPageInject.ResumeLayout(false);
            this.tabPageInject.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.contextMenuStripCopy.ResumeLayout(false);
            this.statusStrip3.ResumeLayout(false);
            this.statusStrip3.PerformLayout();
            this.contextMenuStripFilterOptions.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelProxyConfig;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxProxyPort;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelMainWindow;
        private System.Windows.Forms.SplitContainer splitContainerSessionsTabs;
        private ListViewNoFlicker listViewSessions;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageSummary;
        private System.Windows.Forms.TabPage tabPageFilters;
        private System.Windows.Forms.SplitContainer splitContainerFilters;
        private System.Windows.Forms.GroupBox grpUDPFilters;
        private System.Windows.Forms.TabPage tabPageInject;
        private System.Windows.Forms.RichTextBox richTextBoxInject;
        private System.Windows.Forms.CheckBox checkBoxCheckAllPackets;
        private System.Windows.Forms.ColumnHeader columnHeaderCounter;
        private System.Windows.Forms.ColumnHeader columnHeaderProtocol;
        private System.Windows.Forms.ColumnHeader columnHeaderType;
        private System.Windows.Forms.ColumnHeader columnHeaderUrl;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem captureTrafficToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem saveSessionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadSessionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.TabPage tabPageInspect;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.TabControl tabControlInspectorResponse;
        private System.Windows.Forms.TabPage tabPageInspectorRAWResponse;
        private System.Windows.Forms.TabPage tabPageInspectorXMLResponse;
        private System.Windows.Forms.RichTextBox richTextBoxRawResponse;
        private System.Windows.Forms.TreeView treeViewXmlResponse;
        private System.Windows.Forms.TabPage tabPageHexViewResponse;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private Be.Windows.Forms.HexBox hexBoxResponse;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Button buttonInjectPacket;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panelStats;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelCapsTotal;
        private System.Windows.Forms.Label labelCapsOut;
        private System.Windows.Forms.Label labelCapsIn;
        private System.Windows.Forms.Label labelPacketsTotal;
        private System.Windows.Forms.Label label1PacketsOut;
        private System.Windows.Forms.Label labelPacketsIn;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton2;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unselectedSessionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem noneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem invertSelectionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem markToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem findSessionToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton3;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.SplitContainer splitContainerInspectorTab;
        private System.Windows.Forms.TabControl tabControlInspectorRequest;
        private System.Windows.Forms.TabPage tabPageRawRequest;
        private System.Windows.Forms.RichTextBox richTextBoxRawRequest;
        private System.Windows.Forms.TabPage tabPageXMLRequest;
        private System.Windows.Forms.TreeView treeViewXMLRequest;
        private System.Windows.Forms.TabPage tabPageHexRequest;
        private System.Windows.Forms.StatusStrip statusStrip2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripLabelHexEditorRequest;
        private Be.Windows.Forms.HexBox hexBoxRequest;
        private System.Windows.Forms.ToolStripMenuItem redToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem goldToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem orangeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem purpleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem yellowToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripButton1;
        private System.Windows.Forms.ToolStripMenuItem aboutWinGridProxyToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxCheckAllMessages;
        private System.Windows.Forms.GroupBox grpCapsFilters;
        private System.Windows.Forms.ToolStripStatusLabel labelRequestHex;
        private System.Windows.Forms.ToolStripStatusLabel labelResponseHex;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton4;
        private System.Windows.Forms.ToolStripDropDownButton toolStripFileMenu;
        private System.Windows.Forms.ToolStripMenuItem captureToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem loadSessionArchiveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveSessionArchiveToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem1;
        private System.Windows.Forms.ToolStripDropDownButton EditToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem selectToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem markToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripSessions;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAutoScroll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuSessionsRemove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorFilterPacketByName;
        private System.Windows.Forms.ToolStripMenuItem enableDisableFilterByNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator15;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator16;
        private System.Windows.Forms.ToolStripMenuItem markToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem autoScrollSessionsToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripMark;
        private System.Windows.Forms.ToolStripMenuItem redToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem goldToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem greenToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem blueToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem orangeToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator17;
        private System.Windows.Forms.ToolStripMenuItem unmarkToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripCopy;
        private System.Windows.Forms.ToolStripMenuItem requestDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem responseDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hostAddressToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem packetMessageTypeToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripSelect;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripRemove;
        private System.Windows.Forms.ToolStripMenuItem allToolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem invertToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem noneToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem selectToolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorSelectPacketProto;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSelectPacketName;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRemoveAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRemoveSelected;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRemoveUnselected;
        private System.Windows.Forms.ToolStripMenuItem enableStatisticsToolStripMenuItem;
        private System.Windows.Forms.RadioButton radioButtonSimulator;
        private System.Windows.Forms.RadioButton radioButtonViewer;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton5;
        private System.Windows.Forms.ToolStripMenuItem saveFilterSelectionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadFilterSelectionsToolStripMenuItem;
        private ListViewNoFlicker listViewPacketFilters;
        private System.Windows.Forms.ColumnHeader columnHeaderPacketName;
        private ListViewNoFlicker listViewMessageFilters;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ToolStripMenuItem saveOptionsOnExitToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog2;
        private System.Windows.Forms.OpenFileDialog openFileDialog2;
        private System.Windows.Forms.ToolStripMenuItem startProxyOnStartupToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoAddNewDiscoveredMessagesToolStripMenuItem;
        private System.Windows.Forms.ComboBox comboBoxLoginURL;
        private System.Windows.Forms.ColumnHeader columnHeaderMessageType;
        private System.Windows.Forms.ColumnHeader columnHeaderPacketType;
        private System.Windows.Forms.TabPage tabPageRequestJson;
        private System.Windows.Forms.RichTextBox richTextBoxNotationRequest;
        private System.Windows.Forms.TabPage tabPageResponseJson;
        private System.Windows.Forms.RichTextBox richTextBoxNotationResponse;
        private System.Windows.Forms.TabPage tabPageDecodedRequest;
        private System.Windows.Forms.RichTextBox richTextBoxDecodedRequest;
        private System.Windows.Forms.TabPage tabPageDecodeResponse;
        private System.Windows.Forms.RichTextBox richTextBoxDecodedResponse;
        private System.Windows.Forms.StatusStrip statusStrip3;
        private System.Windows.Forms.ToolStripStatusLabel toolStripMainLabel;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripFilterOptions;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem uncheckAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripMenuItem autoColorizeToolStripMenuItem;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.ColumnHeader columnHeaderContentType;
        private System.Windows.Forms.ComboBox comboBoxListenAddress;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
    }
}

