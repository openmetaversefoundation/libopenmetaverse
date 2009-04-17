namespace WinGridProxy
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.panelProxyConfig = new System.Windows.Forms.Panel();
            this.textBoxLoginURL = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxProxyPort = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxProxyListenIP = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.listViewSessions = new WinGridProxy.ListViewNoFlicker();
            this.columnHeaderCounter = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderProtocol = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderType = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderSize = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderUrl = new System.Windows.Forms.ColumnHeader();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageSummary = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
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
            this.label4 = new System.Windows.Forms.Label();
            this.tabPageFilters = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.grpUDPFilters = new System.Windows.Forms.GroupBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkedListBoxFiltersPackets = new System.Windows.Forms.CheckedListBox();
            this.grpCapsFilters = new System.Windows.Forms.GroupBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.checkedListBoxFiltersMessages = new System.Windows.Forms.CheckedListBox();
            this.tabPageInspect = new System.Windows.Forms.TabPage();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.tabControlInspectorRequest = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.richTextBoxRawLogRequest = new System.Windows.Forms.RichTextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.treeViewRequestXml = new System.Windows.Forms.TreeView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.statusStrip2 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.hexBoxRequest = new Be.Windows.Forms.HexBox();
            this.tabControlInspectorResponse = new System.Windows.Forms.TabControl();
            this.tabPageInspectorRAW = new System.Windows.Forms.TabPage();
            this.richTextBoxRawLogResponse = new System.Windows.Forms.RichTextBox();
            this.tabPageInspectorXML = new System.Windows.Forms.TabPage();
            this.treeViewResponseXml = new System.Windows.Forms.TreeView();
            this.tabPageHexView = new System.Windows.Forms.TabPage();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.hexBoxResponse = new Be.Windows.Forms.HexBox();
            this.tabPageInject = new System.Windows.Forms.TabPage();
            this.button3 = new System.Windows.Forms.Button();
            this.buttonInjectPacket = new System.Windows.Forms.Button();
            this.richTextBoxInject = new System.Windows.Forms.RichTextBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
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
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panelProxyConfig.SuspendLayout();
            this.panel2.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageSummary.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPageFilters.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.grpUDPFilters.SuspendLayout();
            this.grpCapsFilters.SuspendLayout();
            this.tabPageInspect.SuspendLayout();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.tabControlInspectorRequest.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.statusStrip2.SuspendLayout();
            this.tabControlInspectorResponse.SuspendLayout();
            this.tabPageInspectorRAW.SuspendLayout();
            this.tabPageInspectorXML.SuspendLayout();
            this.tabPageHexView.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabPageInject.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelProxyConfig
            // 
            this.panelProxyConfig.Controls.Add(this.textBoxLoginURL);
            this.panelProxyConfig.Controls.Add(this.label3);
            this.panelProxyConfig.Controls.Add(this.label2);
            this.panelProxyConfig.Controls.Add(this.textBoxProxyPort);
            this.panelProxyConfig.Controls.Add(this.button1);
            this.panelProxyConfig.Controls.Add(this.label1);
            this.panelProxyConfig.Controls.Add(this.textBoxProxyListenIP);
            this.panelProxyConfig.Location = new System.Drawing.Point(12, 28);
            this.panelProxyConfig.Name = "panelProxyConfig";
            this.panelProxyConfig.Size = new System.Drawing.Size(1070, 32);
            this.panelProxyConfig.TabIndex = 0;
            // 
            // textBoxLoginURL
            // 
            this.textBoxLoginURL.Location = new System.Drawing.Point(406, 6);
            this.textBoxLoginURL.Name = "textBoxLoginURL";
            this.textBoxLoginURL.Size = new System.Drawing.Size(252, 20);
            this.textBoxLoginURL.TabIndex = 6;
            this.textBoxLoginURL.Text = "https://login.agni.lindenlab.com/cgi-bin/login.cgi";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(342, 9);
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
            this.textBoxProxyPort.Location = new System.Drawing.Point(236, 6);
            this.textBoxProxyPort.Name = "textBoxProxyPort";
            this.textBoxProxyPort.Size = new System.Drawing.Size(100, 20);
            this.textBoxProxyPort.TabIndex = 3;
            this.textBoxProxyPort.Text = "8080";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(664, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Start Proxy";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(204, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Port";
            // 
            // textBoxProxyListenIP
            // 
            this.textBoxProxyListenIP.Location = new System.Drawing.Point(98, 6);
            this.textBoxProxyListenIP.Name = "textBoxProxyListenIP";
            this.textBoxProxyListenIP.Size = new System.Drawing.Size(100, 20);
            this.textBoxProxyListenIP.TabIndex = 0;
            this.textBoxProxyListenIP.Text = "127.0.0.1";
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.splitContainer1);
            this.panel2.Location = new System.Drawing.Point(12, 66);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1070, 431);
            this.panel2.TabIndex = 1;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.listViewSessions);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(1070, 428);
            this.splitContainer1.SplitterDistance = 464;
            this.splitContainer1.TabIndex = 0;
            // 
            // listViewSessions
            // 
            this.listViewSessions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderCounter,
            this.columnHeaderProtocol,
            this.columnHeaderType,
            this.columnHeaderSize,
            this.columnHeaderUrl});
            this.listViewSessions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewSessions.FullRowSelect = true;
            this.listViewSessions.GridLines = true;
            this.listViewSessions.HideSelection = false;
            this.listViewSessions.Location = new System.Drawing.Point(0, 0);
            this.listViewSessions.Name = "listViewSessions";
            this.listViewSessions.Size = new System.Drawing.Size(464, 428);
            this.listViewSessions.SmallImageList = this.imageList1;
            this.listViewSessions.TabIndex = 0;
            this.listViewSessions.UseCompatibleStateImageBehavior = false;
            this.listViewSessions.View = System.Windows.Forms.View.Details;
            this.listViewSessions.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewSessions_ItemSelectionChanged);
            // 
            // columnHeaderCounter
            // 
            this.columnHeaderCounter.Text = "#";
            this.columnHeaderCounter.Width = 42;
            // 
            // columnHeaderProtocol
            // 
            this.columnHeaderProtocol.Text = "Protocol";
            this.columnHeaderProtocol.Width = 55;
            // 
            // columnHeaderType
            // 
            this.columnHeaderType.Text = "Packet Type";
            this.columnHeaderType.Width = 139;
            // 
            // columnHeaderSize
            // 
            this.columnHeaderSize.Text = "Bytes";
            // 
            // columnHeaderUrl
            // 
            this.columnHeaderUrl.Text = "URL";
            this.columnHeaderUrl.Width = 312;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "req_in.png");
            this.imageList1.Images.SetKeyName(1, "server_go.png");
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPageSummary);
            this.tabControl1.Controls.Add(this.tabPageFilters);
            this.tabControl1.Controls.Add(this.tabPageInspect);
            this.tabControl1.Controls.Add(this.tabPageInject);
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(601, 422);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPageSummary
            // 
            this.tabPageSummary.Controls.Add(this.panel1);
            this.tabPageSummary.Controls.Add(this.label4);
            this.tabPageSummary.Location = new System.Drawing.Point(4, 22);
            this.tabPageSummary.Name = "tabPageSummary";
            this.tabPageSummary.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSummary.Size = new System.Drawing.Size(593, 396);
            this.tabPageSummary.TabIndex = 0;
            this.tabPageSummary.Text = "Summary";
            this.tabPageSummary.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Location = new System.Drawing.Point(28, 39);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(227, 188);
            this.panel1.TabIndex = 2;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.labelPacketsTotal);
            this.groupBox2.Controls.Add(this.label1PacketsOut);
            this.groupBox2.Controls.Add(this.labelPacketsIn);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Location = new System.Drawing.Point(3, 89);
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
            this.groupBox1.Controls.Add(this.labelCapsTotal);
            this.groupBox1.Controls.Add(this.labelCapsOut);
            this.groupBox1.Controls.Add(this.labelCapsIn);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
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
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(25, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(524, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Use the Filters tab to enable/disable packets/messages to capture, use Inspector " +
                "tab to view the packet data";
            // 
            // tabPageFilters
            // 
            this.tabPageFilters.Controls.Add(this.splitContainer2);
            this.tabPageFilters.Location = new System.Drawing.Point(4, 22);
            this.tabPageFilters.Name = "tabPageFilters";
            this.tabPageFilters.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageFilters.Size = new System.Drawing.Size(593, 396);
            this.tabPageFilters.TabIndex = 1;
            this.tabPageFilters.Text = "Filters";
            this.tabPageFilters.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.grpUDPFilters);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.grpCapsFilters);
            this.splitContainer2.Size = new System.Drawing.Size(587, 390);
            this.splitContainer2.SplitterDistance = 294;
            this.splitContainer2.TabIndex = 0;
            // 
            // grpUDPFilters
            // 
            this.grpUDPFilters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpUDPFilters.Controls.Add(this.checkBox1);
            this.grpUDPFilters.Controls.Add(this.checkedListBoxFiltersPackets);
            this.grpUDPFilters.Enabled = false;
            this.grpUDPFilters.Location = new System.Drawing.Point(3, 3);
            this.grpUDPFilters.Name = "grpUDPFilters";
            this.grpUDPFilters.Size = new System.Drawing.Size(285, 374);
            this.grpUDPFilters.TabIndex = 0;
            this.grpUDPFilters.TabStop = false;
            this.grpUDPFilters.Text = "UDP Packets";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(6, 351);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(120, 17);
            this.checkBox1.TabIndex = 1;
            this.checkBox1.Text = "Check/Uncheck All";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // checkedListBoxFiltersPackets
            // 
            this.checkedListBoxFiltersPackets.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListBoxFiltersPackets.CheckOnClick = true;
            this.checkedListBoxFiltersPackets.FormattingEnabled = true;
            this.checkedListBoxFiltersPackets.Location = new System.Drawing.Point(6, 19);
            this.checkedListBoxFiltersPackets.Name = "checkedListBoxFiltersPackets";
            this.checkedListBoxFiltersPackets.Size = new System.Drawing.Size(273, 319);
            this.checkedListBoxFiltersPackets.TabIndex = 0;
            this.checkedListBoxFiltersPackets.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBox1_ItemCheck);
            // 
            // grpCapsFilters
            // 
            this.grpCapsFilters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpCapsFilters.Controls.Add(this.checkBox2);
            this.grpCapsFilters.Controls.Add(this.button2);
            this.grpCapsFilters.Controls.Add(this.checkedListBoxFiltersMessages);
            this.grpCapsFilters.Enabled = false;
            this.grpCapsFilters.Location = new System.Drawing.Point(3, 3);
            this.grpCapsFilters.Name = "grpCapsFilters";
            this.grpCapsFilters.Size = new System.Drawing.Size(286, 374);
            this.grpCapsFilters.TabIndex = 1;
            this.grpCapsFilters.TabStop = false;
            this.grpCapsFilters.Text = "Capabilities Messages";
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(6, 351);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(120, 17);
            this.checkBox2.TabIndex = 2;
            this.checkBox2.Text = "Check/Uncheck All";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBoxCheckallCaps_CheckedChanged);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(205, 348);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 20);
            this.button2.TabIndex = 1;
            this.button2.Text = "Refresh";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.buttonRefreshCapsList_Click);
            // 
            // checkedListBoxFiltersMessages
            // 
            this.checkedListBoxFiltersMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListBoxFiltersMessages.CheckOnClick = true;
            this.checkedListBoxFiltersMessages.FormattingEnabled = true;
            this.checkedListBoxFiltersMessages.Location = new System.Drawing.Point(6, 19);
            this.checkedListBoxFiltersMessages.Name = "checkedListBoxFiltersMessages";
            this.checkedListBoxFiltersMessages.Size = new System.Drawing.Size(274, 319);
            this.checkedListBoxFiltersMessages.TabIndex = 0;
            this.checkedListBoxFiltersMessages.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBoxCaps_ItemCheck);
            // 
            // tabPageInspect
            // 
            this.tabPageInspect.Controls.Add(this.splitContainer3);
            this.tabPageInspect.Location = new System.Drawing.Point(4, 22);
            this.tabPageInspect.Name = "tabPageInspect";
            this.tabPageInspect.Size = new System.Drawing.Size(593, 396);
            this.tabPageInspect.TabIndex = 3;
            this.tabPageInspect.Text = "Inspector";
            this.tabPageInspect.UseVisualStyleBackColor = true;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.tabControlInspectorRequest);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.tabControlInspectorResponse);
            this.splitContainer3.Size = new System.Drawing.Size(593, 396);
            this.splitContainer3.SplitterDistance = 156;
            this.splitContainer3.TabIndex = 1;
            // 
            // tabControlInspectorRequest
            // 
            this.tabControlInspectorRequest.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControlInspectorRequest.Controls.Add(this.tabPage1);
            this.tabControlInspectorRequest.Controls.Add(this.tabPage2);
            this.tabControlInspectorRequest.Controls.Add(this.tabPage3);
            this.tabControlInspectorRequest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlInspectorRequest.ImageList = this.imageList1;
            this.tabControlInspectorRequest.Location = new System.Drawing.Point(0, 0);
            this.tabControlInspectorRequest.Name = "tabControlInspectorRequest";
            this.tabControlInspectorRequest.SelectedIndex = 0;
            this.tabControlInspectorRequest.Size = new System.Drawing.Size(593, 156);
            this.tabControlInspectorRequest.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.richTextBoxRawLogRequest);
            this.tabPage1.ImageIndex = 1;
            this.tabPage1.Location = new System.Drawing.Point(4, 26);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(585, 126);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Raw";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // richTextBoxRawLogRequest
            // 
            this.richTextBoxRawLogRequest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxRawLogRequest.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxRawLogRequest.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxRawLogRequest.Name = "richTextBoxRawLogRequest";
            this.richTextBoxRawLogRequest.Size = new System.Drawing.Size(579, 120);
            this.richTextBoxRawLogRequest.TabIndex = 1;
            this.richTextBoxRawLogRequest.Text = "";
            this.richTextBoxRawLogRequest.WordWrap = false;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.treeViewRequestXml);
            this.tabPage2.ImageIndex = 1;
            this.tabPage2.Location = new System.Drawing.Point(4, 26);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(585, 126);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "XML";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // treeViewRequestXml
            // 
            this.treeViewRequestXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewRequestXml.Location = new System.Drawing.Point(3, 3);
            this.treeViewRequestXml.Name = "treeViewRequestXml";
            this.treeViewRequestXml.Size = new System.Drawing.Size(579, 120);
            this.treeViewRequestXml.TabIndex = 1;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.statusStrip2);
            this.tabPage3.Controls.Add(this.hexBoxRequest);
            this.tabPage3.ImageIndex = 1;
            this.tabPage3.Location = new System.Drawing.Point(4, 26);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(585, 126);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Hex";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // statusStrip2
            // 
            this.statusStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel2});
            this.statusStrip2.Location = new System.Drawing.Point(0, 104);
            this.statusStrip2.Name = "statusStrip2";
            this.statusStrip2.Size = new System.Drawing.Size(585, 22);
            this.statusStrip2.TabIndex = 3;
            this.statusStrip2.Text = "statusStrip2";
            this.statusStrip2.Visible = false;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(0, 17);
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
            this.hexBoxRequest.Size = new System.Drawing.Size(579, 97);
            this.hexBoxRequest.StringViewVisible = true;
            this.hexBoxRequest.TabIndex = 2;
            this.hexBoxRequest.UseFixedBytesPerLine = true;
            this.hexBoxRequest.VScrollBarVisible = true;
            // 
            // tabControlInspectorResponse
            // 
            this.tabControlInspectorResponse.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControlInspectorResponse.Controls.Add(this.tabPageInspectorRAW);
            this.tabControlInspectorResponse.Controls.Add(this.tabPageInspectorXML);
            this.tabControlInspectorResponse.Controls.Add(this.tabPageHexView);
            this.tabControlInspectorResponse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlInspectorResponse.ImageList = this.imageList1;
            this.tabControlInspectorResponse.Location = new System.Drawing.Point(0, 0);
            this.tabControlInspectorResponse.Multiline = true;
            this.tabControlInspectorResponse.Name = "tabControlInspectorResponse";
            this.tabControlInspectorResponse.SelectedIndex = 0;
            this.tabControlInspectorResponse.Size = new System.Drawing.Size(593, 236);
            this.tabControlInspectorResponse.TabIndex = 0;
            // 
            // tabPageInspectorRAW
            // 
            this.tabPageInspectorRAW.Controls.Add(this.richTextBoxRawLogResponse);
            this.tabPageInspectorRAW.ImageIndex = 0;
            this.tabPageInspectorRAW.Location = new System.Drawing.Point(4, 26);
            this.tabPageInspectorRAW.Name = "tabPageInspectorRAW";
            this.tabPageInspectorRAW.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageInspectorRAW.Size = new System.Drawing.Size(585, 206);
            this.tabPageInspectorRAW.TabIndex = 0;
            this.tabPageInspectorRAW.Text = "Raw";
            this.tabPageInspectorRAW.UseVisualStyleBackColor = true;
            // 
            // richTextBoxRawLogResponse
            // 
            this.richTextBoxRawLogResponse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxRawLogResponse.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxRawLogResponse.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxRawLogResponse.Name = "richTextBoxRawLogResponse";
            this.richTextBoxRawLogResponse.Size = new System.Drawing.Size(579, 200);
            this.richTextBoxRawLogResponse.TabIndex = 0;
            this.richTextBoxRawLogResponse.Text = "";
            this.richTextBoxRawLogResponse.WordWrap = false;
            // 
            // tabPageInspectorXML
            // 
            this.tabPageInspectorXML.Controls.Add(this.treeViewResponseXml);
            this.tabPageInspectorXML.ImageIndex = 0;
            this.tabPageInspectorXML.Location = new System.Drawing.Point(4, 26);
            this.tabPageInspectorXML.Name = "tabPageInspectorXML";
            this.tabPageInspectorXML.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageInspectorXML.Size = new System.Drawing.Size(585, 206);
            this.tabPageInspectorXML.TabIndex = 1;
            this.tabPageInspectorXML.Text = "XML";
            this.tabPageInspectorXML.UseVisualStyleBackColor = true;
            // 
            // treeViewResponseXml
            // 
            this.treeViewResponseXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewResponseXml.Location = new System.Drawing.Point(3, 3);
            this.treeViewResponseXml.Name = "treeViewResponseXml";
            this.treeViewResponseXml.Size = new System.Drawing.Size(579, 200);
            this.treeViewResponseXml.TabIndex = 0;
            // 
            // tabPageHexView
            // 
            this.tabPageHexView.Controls.Add(this.statusStrip1);
            this.tabPageHexView.Controls.Add(this.hexBoxResponse);
            this.tabPageHexView.ImageIndex = 0;
            this.tabPageHexView.Location = new System.Drawing.Point(4, 26);
            this.tabPageHexView.Name = "tabPageHexView";
            this.tabPageHexView.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageHexView.Size = new System.Drawing.Size(585, 206);
            this.tabPageHexView.TabIndex = 4;
            this.tabPageHexView.Text = "Hex";
            this.tabPageHexView.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(3, 181);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(579, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
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
            this.hexBoxResponse.Size = new System.Drawing.Size(579, 176);
            this.hexBoxResponse.StringViewVisible = true;
            this.hexBoxResponse.TabIndex = 1;
            this.hexBoxResponse.UseFixedBytesPerLine = true;
            this.hexBoxResponse.VScrollBarVisible = true;
            this.hexBoxResponse.CurrentPositionInLineChanged += new System.EventHandler(this.Position_Changed);
            this.hexBoxResponse.CurrentLineChanged += new System.EventHandler(this.Position_Changed);
            // 
            // tabPageInject
            // 
            this.tabPageInject.Controls.Add(this.button3);
            this.tabPageInject.Controls.Add(this.buttonInjectPacket);
            this.tabPageInject.Controls.Add(this.richTextBoxInject);
            this.tabPageInject.Location = new System.Drawing.Point(4, 22);
            this.tabPageInject.Name = "tabPageInject";
            this.tabPageInject.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageInject.Size = new System.Drawing.Size(593, 396);
            this.tabPageInject.TabIndex = 2;
            this.tabPageInject.Text = "Inject";
            this.tabPageInject.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
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
            this.buttonInjectPacket.Enabled = false;
            this.buttonInjectPacket.Location = new System.Drawing.Point(117, 367);
            this.buttonInjectPacket.Name = "buttonInjectPacket";
            this.buttonInjectPacket.Size = new System.Drawing.Size(75, 23);
            this.buttonInjectPacket.TabIndex = 1;
            this.buttonInjectPacket.Text = "Inject";
            this.buttonInjectPacket.UseVisualStyleBackColor = true;
            // 
            // richTextBoxInject
            // 
            this.richTextBoxInject.Location = new System.Drawing.Point(6, 6);
            this.richTextBoxInject.Name = "richTextBoxInject";
            this.richTextBoxInject.Size = new System.Drawing.Size(569, 355);
            this.richTextBoxInject.TabIndex = 0;
            this.richTextBoxInject.Text = "";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1,
            this.toolStripDropDownButton2,
            this.toolStripDropDownButton3});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1094, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
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
            this.allToolStripMenuItem.Click += new System.EventHandler(this.allToolStripMenuItem_Click);
            // 
            // selectedToolStripMenuItem
            // 
            this.selectedToolStripMenuItem.Name = "selectedToolStripMenuItem";
            this.selectedToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.selectedToolStripMenuItem.Text = "Selected Sessions";
            this.selectedToolStripMenuItem.Click += new System.EventHandler(this.selectedToolStripMenuItem_Click);
            // 
            // unselectedSessionsToolStripMenuItem
            // 
            this.unselectedSessionsToolStripMenuItem.Name = "unselectedSessionsToolStripMenuItem";
            this.unselectedSessionsToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.unselectedSessionsToolStripMenuItem.Text = "Unselected Sessions";
            this.unselectedSessionsToolStripMenuItem.Click += new System.EventHandler(this.unselectedSessionsToolStripMenuItem_Click);
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
            this.allToolStripMenuItem1.Click += new System.EventHandler(this.allToolStripMenuItem1_Click);
            // 
            // noneToolStripMenuItem
            // 
            this.noneToolStripMenuItem.Name = "noneToolStripMenuItem";
            this.noneToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.noneToolStripMenuItem.Text = "None";
            this.noneToolStripMenuItem.Click += new System.EventHandler(this.noneToolStripMenuItem_Click);
            // 
            // invertSelectionsToolStripMenuItem
            // 
            this.invertSelectionsToolStripMenuItem.Name = "invertSelectionsToolStripMenuItem";
            this.invertSelectionsToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.invertSelectionsToolStripMenuItem.Text = "Invert Selections";
            this.invertSelectionsToolStripMenuItem.Click += new System.EventHandler(this.invertSelectionsToolStripMenuItem_Click);
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
            this.redToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.redToolStripMenuItem.Text = "Red";
            this.redToolStripMenuItem.Click += new System.EventHandler(this.MarkColorToolStripMenuItem_Click);
            // 
            // goldToolStripMenuItem
            // 
            this.goldToolStripMenuItem.Name = "goldToolStripMenuItem";
            this.goldToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.goldToolStripMenuItem.Text = "Gold";
            this.goldToolStripMenuItem.Click += new System.EventHandler(this.MarkColorToolStripMenuItem_Click);
            // 
            // orangeToolStripMenuItem
            // 
            this.orangeToolStripMenuItem.Name = "orangeToolStripMenuItem";
            this.orangeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.orangeToolStripMenuItem.Text = "Orange";
            this.orangeToolStripMenuItem.Click += new System.EventHandler(this.MarkColorToolStripMenuItem_Click);
            // 
            // purpleToolStripMenuItem
            // 
            this.purpleToolStripMenuItem.Name = "purpleToolStripMenuItem";
            this.purpleToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.purpleToolStripMenuItem.Text = "Purple";
            this.purpleToolStripMenuItem.Click += new System.EventHandler(this.MarkColorToolStripMenuItem_Click);
            // 
            // yellowToolStripMenuItem
            // 
            this.yellowToolStripMenuItem.Name = "yellowToolStripMenuItem";
            this.yellowToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.yellowToolStripMenuItem.Text = "Yellow";
            this.yellowToolStripMenuItem.Click += new System.EventHandler(this.MarkColorToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(149, 6);
            // 
            // removeToolStripMenuItem1
            // 
            this.removeToolStripMenuItem1.Name = "removeToolStripMenuItem1";
            this.removeToolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
            this.removeToolStripMenuItem1.Text = "Remove";
            this.removeToolStripMenuItem1.Click += new System.EventHandler(this.removeToolStripMenuItem1_Click);
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
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1094, 509);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panelProxyConfig);
            this.Name = "Form1";
            this.Text = "WinGridProxy";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.panelProxyConfig.ResumeLayout(false);
            this.panelProxyConfig.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPageSummary.ResumeLayout(false);
            this.tabPageSummary.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPageFilters.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.grpUDPFilters.ResumeLayout(false);
            this.grpUDPFilters.PerformLayout();
            this.grpCapsFilters.ResumeLayout(false);
            this.grpCapsFilters.PerformLayout();
            this.tabPageInspect.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            this.tabControlInspectorRequest.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.statusStrip2.ResumeLayout(false);
            this.statusStrip2.PerformLayout();
            this.tabControlInspectorResponse.ResumeLayout(false);
            this.tabPageInspectorRAW.ResumeLayout(false);
            this.tabPageInspectorXML.ResumeLayout(false);
            this.tabPageHexView.ResumeLayout(false);
            this.tabPageHexView.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabPageInject.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
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
        private System.Windows.Forms.TextBox textBoxProxyListenIP;
        private System.Windows.Forms.TextBox textBoxLoginURL;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private ListViewNoFlicker listViewSessions;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageSummary;
        private System.Windows.Forms.TabPage tabPageFilters;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.CheckedListBox checkedListBoxFiltersPackets;
        private System.Windows.Forms.CheckedListBox checkedListBoxFiltersMessages;
        private System.Windows.Forms.GroupBox grpUDPFilters;
        private System.Windows.Forms.GroupBox grpCapsFilters;
        private System.Windows.Forms.TabPage tabPageInject;
        private System.Windows.Forms.RichTextBox richTextBoxInject;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox checkBox2;
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
        private System.Windows.Forms.TabPage tabPageInspectorRAW;
        private System.Windows.Forms.TabPage tabPageInspectorXML;
        private System.Windows.Forms.RichTextBox richTextBoxRawLogResponse;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TreeView treeViewResponseXml;
        private System.Windows.Forms.TabPage tabPageHexView;
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
        private System.Windows.Forms.Panel panel1;
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
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.TabControl tabControlInspectorRequest;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RichTextBox richTextBoxRawLogRequest;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TreeView treeViewRequestXml;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.StatusStrip statusStrip2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private Be.Windows.Forms.HexBox hexBoxRequest;
        private System.Windows.Forms.ToolStripMenuItem redToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem goldToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem orangeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem purpleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem yellowToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem1;
    }
}

