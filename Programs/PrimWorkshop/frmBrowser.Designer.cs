namespace PrimWorkshop
{
    partial class frmBrowser
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
            GlacialComponents.Controls.GLColumn glColumn1 = new GlacialComponents.Controls.GLColumn();
            GlacialComponents.Controls.GLColumn glColumn2 = new GlacialComponents.Controls.GLColumn();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.glControl = new Tao.Platform.Windows.SimpleOpenGlControl();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtPass = new System.Windows.Forms.TextBox();
            this.txtLast = new System.Windows.Forms.TextBox();
            this.txtFirst = new System.Windows.Forms.TextBox();
            this.cmdLogin = new System.Windows.Forms.Button();
            this.lstDownloads = new GlacialComponents.Controls.GlacialList();
            this.progPrims = new System.Windows.Forms.ProgressBar();
            this.lblPrims = new System.Windows.Forms.Label();
            this.cboServer = new System.Windows.Forms.ComboBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.txtSim = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtX = new System.Windows.Forms.TextBox();
            this.txtY = new System.Windows.Forms.TextBox();
            this.txtZ = new System.Windows.Forms.TextBox();
            this.cmdTeleport = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 175F));
            this.tableLayoutPanel1.Controls.Add(this.glControl, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lstDownloads, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.progPrims, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblPrims, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 2, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(1, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(799, 612);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // glControl
            // 
            this.glControl.AccumBits = ((byte)(0));
            this.glControl.AutoCheckErrors = false;
            this.glControl.AutoFinish = false;
            this.glControl.AutoMakeCurrent = true;
            this.glControl.AutoSwapBuffers = true;
            this.glControl.BackColor = System.Drawing.Color.Black;
            this.glControl.ColorBits = ((byte)(32));
            this.tableLayoutPanel1.SetColumnSpan(this.glControl, 2);
            this.glControl.DepthBits = ((byte)(16));
            this.glControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl.Location = new System.Drawing.Point(3, 3);
            this.glControl.Name = "glControl";
            this.glControl.Size = new System.Drawing.Size(618, 411);
            this.glControl.StencilBits = ((byte)(0));
            this.glControl.TabIndex = 7;
            this.glControl.TabStop = false;
            this.glControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseMove);
            this.glControl.MouseClick += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseClick);
            this.glControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseDown);
            this.glControl.Resize += new System.EventHandler(this.glControl_Resize);
            this.glControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseUp);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.panel1, 2);
            this.panel1.Controls.Add(this.cboServer);
            this.panel1.Controls.Add(this.txtPass);
            this.panel1.Controls.Add(this.txtLast);
            this.panel1.Controls.Add(this.txtFirst);
            this.panel1.Controls.Add(this.cmdLogin);
            this.panel1.Location = new System.Drawing.Point(3, 450);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(618, 39);
            this.panel1.TabIndex = 8;
            // 
            // txtPass
            // 
            this.txtPass.Location = new System.Drawing.Point(215, 11);
            this.txtPass.Name = "txtPass";
            this.txtPass.Size = new System.Drawing.Size(100, 20);
            this.txtPass.TabIndex = 2;
            this.txtPass.UseSystemPasswordChar = true;
            this.txtPass.Enter += new System.EventHandler(this.txtLogin_Enter);
            // 
            // txtLast
            // 
            this.txtLast.Location = new System.Drawing.Point(109, 11);
            this.txtLast.Name = "txtLast";
            this.txtLast.Size = new System.Drawing.Size(100, 20);
            this.txtLast.TabIndex = 1;
            this.txtLast.Enter += new System.EventHandler(this.txtLogin_Enter);
            // 
            // txtFirst
            // 
            this.txtFirst.Location = new System.Drawing.Point(3, 11);
            this.txtFirst.Name = "txtFirst";
            this.txtFirst.Size = new System.Drawing.Size(100, 20);
            this.txtFirst.TabIndex = 0;
            this.txtFirst.Enter += new System.EventHandler(this.txtLogin_Enter);
            // 
            // cmdLogin
            // 
            this.cmdLogin.Location = new System.Drawing.Point(321, 9);
            this.cmdLogin.Name = "cmdLogin";
            this.cmdLogin.Size = new System.Drawing.Size(75, 23);
            this.cmdLogin.TabIndex = 3;
            this.cmdLogin.Text = "Login";
            this.cmdLogin.UseVisualStyleBackColor = true;
            this.cmdLogin.Click += new System.EventHandler(this.cmdLogin_Click);
            // 
            // lstDownloads
            // 
            this.lstDownloads.AllowColumnResize = true;
            this.lstDownloads.AllowMultiselect = false;
            this.lstDownloads.AlternateBackground = System.Drawing.Color.DarkGreen;
            this.lstDownloads.AlternatingColors = false;
            this.lstDownloads.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstDownloads.AutoHeight = true;
            this.lstDownloads.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lstDownloads.BackgroundStretchToFit = true;
            glColumn1.ActivatedEmbeddedType = GlacialComponents.Controls.GLActivatedEmbeddedTypes.None;
            glColumn1.CheckBoxes = false;
            glColumn1.ImageIndex = -1;
            glColumn1.Name = "colTextureID";
            glColumn1.NumericSort = false;
            glColumn1.Text = "Texture ID";
            glColumn1.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            glColumn1.Width = 220;
            glColumn2.ActivatedEmbeddedType = GlacialComponents.Controls.GLActivatedEmbeddedTypes.None;
            glColumn2.CheckBoxes = false;
            glColumn2.ImageIndex = -1;
            glColumn2.Name = "colProgress";
            glColumn2.NumericSort = false;
            glColumn2.Text = "Download Progress";
            glColumn2.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            glColumn2.Width = 220;
            this.lstDownloads.Columns.AddRange(new GlacialComponents.Controls.GLColumn[] {
            glColumn1,
            glColumn2});
            this.tableLayoutPanel1.SetColumnSpan(this.lstDownloads, 2);
            this.lstDownloads.ControlStyle = GlacialComponents.Controls.GLControlStyles.Normal;
            this.lstDownloads.FullRowSelect = true;
            this.lstDownloads.GridColor = System.Drawing.Color.LightGray;
            this.lstDownloads.GridLines = GlacialComponents.Controls.GLGridLines.gridBoth;
            this.lstDownloads.GridLineStyle = GlacialComponents.Controls.GLGridLineStyles.gridSolid;
            this.lstDownloads.GridTypes = GlacialComponents.Controls.GLGridTypes.gridOnExists;
            this.lstDownloads.HeaderHeight = 22;
            this.lstDownloads.HeaderVisible = true;
            this.lstDownloads.HeaderWordWrap = false;
            this.lstDownloads.HotColumnTracking = false;
            this.lstDownloads.HotItemTracking = false;
            this.lstDownloads.HotTrackingColor = System.Drawing.Color.LightGray;
            this.lstDownloads.HoverEvents = false;
            this.lstDownloads.HoverTime = 1;
            this.lstDownloads.ImageList = null;
            this.lstDownloads.ItemHeight = 17;
            this.lstDownloads.ItemWordWrap = false;
            this.lstDownloads.Location = new System.Drawing.Point(3, 495);
            this.lstDownloads.Name = "lstDownloads";
            this.lstDownloads.Selectable = true;
            this.lstDownloads.SelectedTextColor = System.Drawing.Color.White;
            this.lstDownloads.SelectionColor = System.Drawing.Color.DarkBlue;
            this.lstDownloads.ShowBorder = true;
            this.lstDownloads.ShowFocusRect = false;
            this.lstDownloads.Size = new System.Drawing.Size(618, 114);
            this.lstDownloads.SortType = GlacialComponents.Controls.SortTypes.InsertionSort;
            this.lstDownloads.SuperFlatHeaderColor = System.Drawing.Color.White;
            this.lstDownloads.TabIndex = 9;
            this.lstDownloads.Text = "Texture Downloads";
            // 
            // progPrims
            // 
            this.progPrims.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progPrims.Location = new System.Drawing.Point(133, 420);
            this.progPrims.Name = "progPrims";
            this.progPrims.Size = new System.Drawing.Size(488, 24);
            this.progPrims.TabIndex = 10;
            // 
            // lblPrims
            // 
            this.lblPrims.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPrims.AutoSize = true;
            this.lblPrims.Location = new System.Drawing.Point(3, 425);
            this.lblPrims.Name = "lblPrims";
            this.lblPrims.Size = new System.Drawing.Size(61, 13);
            this.lblPrims.TabIndex = 11;
            this.lblPrims.Text = "Prims: 0 / 0";
            // 
            // cboServer
            // 
            this.cboServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cboServer.FormattingEnabled = true;
            this.cboServer.Location = new System.Drawing.Point(405, 11);
            this.cboServer.Name = "cboServer";
            this.cboServer.Size = new System.Drawing.Size(204, 21);
            this.cboServer.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.cmdTeleport);
            this.panel2.Controls.Add(this.txtZ);
            this.panel2.Controls.Add(this.txtY);
            this.panel2.Controls.Add(this.txtX);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.txtSim);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(627, 3);
            this.panel2.Name = "panel2";
            this.tableLayoutPanel1.SetRowSpan(this.panel2, 4);
            this.panel2.Size = new System.Drawing.Size(169, 606);
            this.panel2.TabIndex = 12;
            // 
            // txtSim
            // 
            this.txtSim.Location = new System.Drawing.Point(36, 8);
            this.txtSim.Name = "txtSim";
            this.txtSim.Size = new System.Drawing.Size(126, 20);
            this.txtSim.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Sim:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 37);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "X:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(58, 37);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Y:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(112, 37);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Z:";
            // 
            // txtX
            // 
            this.txtX.Location = new System.Drawing.Point(24, 34);
            this.txtX.MaxLength = 3;
            this.txtX.Name = "txtX";
            this.txtX.Size = new System.Drawing.Size(30, 20);
            this.txtX.TabIndex = 6;
            this.txtX.Text = "128";
            // 
            // txtY
            // 
            this.txtY.Location = new System.Drawing.Point(78, 34);
            this.txtY.MaxLength = 3;
            this.txtY.Name = "txtY";
            this.txtY.Size = new System.Drawing.Size(30, 20);
            this.txtY.TabIndex = 7;
            this.txtY.Text = "128";
            // 
            // txtZ
            // 
            this.txtZ.Location = new System.Drawing.Point(132, 34);
            this.txtZ.MaxLength = 3;
            this.txtZ.Name = "txtZ";
            this.txtZ.Size = new System.Drawing.Size(30, 20);
            this.txtZ.TabIndex = 8;
            this.txtZ.Text = "0";
            // 
            // cmdTeleport
            // 
            this.cmdTeleport.Enabled = false;
            this.cmdTeleport.Location = new System.Drawing.Point(87, 60);
            this.cmdTeleport.Name = "cmdTeleport";
            this.cmdTeleport.Size = new System.Drawing.Size(75, 23);
            this.cmdTeleport.TabIndex = 9;
            this.cmdTeleport.Text = "Teleport";
            this.cmdTeleport.UseVisualStyleBackColor = true;
            this.cmdTeleport.Click += new System.EventHandler(this.cmdTeleport_Click);
            // 
            // frmBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 612);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "frmBrowser";
            this.Text = "World Browser";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmBrowser_FormClosing);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Tao.Platform.Windows.SimpleOpenGlControl glControl;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txtPass;
        private System.Windows.Forms.TextBox txtLast;
        private System.Windows.Forms.TextBox txtFirst;
        private System.Windows.Forms.Button cmdLogin;
        private GlacialComponents.Controls.GlacialList lstDownloads;
        private System.Windows.Forms.ProgressBar progPrims;
        private System.Windows.Forms.Label lblPrims;
        private System.Windows.Forms.ComboBox cboServer;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox txtZ;
        private System.Windows.Forms.TextBox txtY;
        private System.Windows.Forms.TextBox txtX;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSim;
        private System.Windows.Forms.Button cmdTeleport;

    }
}