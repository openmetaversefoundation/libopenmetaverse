namespace PrimWorkshop
{
    partial class frmPrimWorkshop
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
                glControl.DestroyContexts();
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
            this.menu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.opToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openPrimXMLToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.textureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.savePrimXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.worldBrowserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.oBJToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wireframeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.glControl = new Tao.Platform.Windows.SimpleOpenGlControl();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.cboFace = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.cboPrim = new System.Windows.Forms.ComboBox();
            this.scrollRoll = new System.Windows.Forms.HScrollBar();
            this.scrollPitch = new System.Windows.Forms.HScrollBar();
            this.scrollYaw = new System.Windows.Forms.HScrollBar();
            this.picTexture = new System.Windows.Forms.PictureBox();
            this.scrollZoom = new System.Windows.Forms.HScrollBar();
            this.menu.SuspendLayout();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTexture)).BeginInit();
            this.SuspendLayout();
            // 
            // menu
            // 
            this.menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menu.Location = new System.Drawing.Point(0, 0);
            this.menu.Name = "menu";
            this.menu.Size = new System.Drawing.Size(996, 24);
            this.menu.TabIndex = 0;
            this.menu.Text = "menu";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.opToolStripMenuItem,
            this.toolStripMenuItem2,
            this.savePrimXMLToolStripMenuItem,
            this.saveTextureToolStripMenuItem,
            this.importToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // opToolStripMenuItem
            // 
            this.opToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openPrimXMLToolStripMenuItem1,
            this.textureToolStripMenuItem});
            this.opToolStripMenuItem.Name = "opToolStripMenuItem";
            this.opToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.opToolStripMenuItem.Text = "Open";
            // 
            // openPrimXMLToolStripMenuItem1
            // 
            this.openPrimXMLToolStripMenuItem1.Name = "openPrimXMLToolStripMenuItem1";
            this.openPrimXMLToolStripMenuItem1.Size = new System.Drawing.Size(197, 22);
            this.openPrimXMLToolStripMenuItem1.Text = "Prim XML / Sculpt Map";
            this.openPrimXMLToolStripMenuItem1.Click += new System.EventHandler(this.openPrimXMLToolStripMenuItem1_Click);
            // 
            // textureToolStripMenuItem
            // 
            this.textureToolStripMenuItem.Name = "textureToolStripMenuItem";
            this.textureToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.textureToolStripMenuItem.Text = "Texture";
            this.textureToolStripMenuItem.Click += new System.EventHandler(this.textureToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(150, 6);
            // 
            // savePrimXMLToolStripMenuItem
            // 
            this.savePrimXMLToolStripMenuItem.Name = "savePrimXMLToolStripMenuItem";
            this.savePrimXMLToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.savePrimXMLToolStripMenuItem.Text = "Save Prim XML";
            this.savePrimXMLToolStripMenuItem.Click += new System.EventHandler(this.savePrimXMLToolStripMenuItem_Click);
            // 
            // saveTextureToolStripMenuItem
            // 
            this.saveTextureToolStripMenuItem.Name = "saveTextureToolStripMenuItem";
            this.saveTextureToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.saveTextureToolStripMenuItem.Text = "Save Texture";
            this.saveTextureToolStripMenuItem.Click += new System.EventHandler(this.saveTextureToolStripMenuItem_Click);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.worldBrowserToolStripMenuItem});
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.importToolStripMenuItem.Text = "Import";
            // 
            // worldBrowserToolStripMenuItem
            // 
            this.worldBrowserToolStripMenuItem.Name = "worldBrowserToolStripMenuItem";
            this.worldBrowserToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.worldBrowserToolStripMenuItem.Text = "World Browser";
            this.worldBrowserToolStripMenuItem.Click += new System.EventHandler(this.worldBrowserToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.oBJToolStripMenuItem});
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.exportToolStripMenuItem.Text = "Export";
            // 
            // oBJToolStripMenuItem
            // 
            this.oBJToolStripMenuItem.Name = "oBJToolStripMenuItem";
            this.oBJToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.oBJToolStripMenuItem.Text = "OBJ Format";
            this.oBJToolStripMenuItem.Click += new System.EventHandler(this.oBJToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(150, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wireframeToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // wireframeToolStripMenuItem
            // 
            this.wireframeToolStripMenuItem.Checked = true;
            this.wireframeToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.wireframeToolStripMenuItem.Name = "wireframeToolStripMenuItem";
            this.wireframeToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.wireframeToolStripMenuItem.Text = "Wireframe";
            this.wireframeToolStripMenuItem.Click += new System.EventHandler(this.wireframeToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 24);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.glControl);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer.Size = new System.Drawing.Size(996, 512);
            this.splitContainer.SplitterDistance = 550;
            this.splitContainer.TabIndex = 6;
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
            this.glControl.DepthBits = ((byte)(16));
            this.glControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl.Location = new System.Drawing.Point(0, 0);
            this.glControl.Name = "glControl";
            this.glControl.Size = new System.Drawing.Size(550, 512);
            this.glControl.StencilBits = ((byte)(0));
            this.glControl.TabIndex = 5;
            this.glControl.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl_Paint);
            this.glControl.Resize += new System.EventHandler(this.glControl_Resize);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.scrollRoll, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.scrollPitch, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.scrollYaw, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.picTexture, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.scrollZoom, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(442, 512);
            this.tableLayoutPanel1.TabIndex = 9;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.cboFace);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 117);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(436, 24);
            this.panel2.TabIndex = 16;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "Face:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboFace
            // 
            this.cboFace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFace.FormattingEnabled = true;
            this.cboFace.Location = new System.Drawing.Point(40, 2);
            this.cboFace.Name = "cboFace";
            this.cboFace.Size = new System.Drawing.Size(174, 21);
            this.cboFace.TabIndex = 15;
            this.cboFace.SelectedIndexChanged += new System.EventHandler(this.cboFace_SelectedIndexChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.cboPrim);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 87);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(436, 24);
            this.panel1.TabIndex = 15;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "Prim:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboPrim
            // 
            this.cboPrim.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPrim.FormattingEnabled = true;
            this.cboPrim.Location = new System.Drawing.Point(40, 2);
            this.cboPrim.Name = "cboPrim";
            this.cboPrim.Size = new System.Drawing.Size(174, 21);
            this.cboPrim.TabIndex = 15;
            this.cboPrim.SelectedIndexChanged += new System.EventHandler(this.cboPrim_SelectedIndexChanged);
            // 
            // scrollRoll
            // 
            this.scrollRoll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollRoll.Location = new System.Drawing.Point(0, 2);
            this.scrollRoll.Maximum = 360;
            this.scrollRoll.Name = "scrollRoll";
            this.scrollRoll.Size = new System.Drawing.Size(442, 16);
            this.scrollRoll.TabIndex = 9;
            this.scrollRoll.ValueChanged += new System.EventHandler(this.scroll_ValueChanged);
            // 
            // scrollPitch
            // 
            this.scrollPitch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollPitch.Location = new System.Drawing.Point(0, 22);
            this.scrollPitch.Maximum = 360;
            this.scrollPitch.Name = "scrollPitch";
            this.scrollPitch.Size = new System.Drawing.Size(442, 16);
            this.scrollPitch.TabIndex = 10;
            this.scrollPitch.ValueChanged += new System.EventHandler(this.scroll_ValueChanged);
            // 
            // scrollYaw
            // 
            this.scrollYaw.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollYaw.Location = new System.Drawing.Point(0, 42);
            this.scrollYaw.Maximum = 360;
            this.scrollYaw.Name = "scrollYaw";
            this.scrollYaw.Size = new System.Drawing.Size(442, 16);
            this.scrollYaw.TabIndex = 11;
            this.scrollYaw.ValueChanged += new System.EventHandler(this.scroll_ValueChanged);
            // 
            // picTexture
            // 
            this.picTexture.BackColor = System.Drawing.Color.Black;
            this.picTexture.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picTexture.Location = new System.Drawing.Point(3, 147);
            this.picTexture.Name = "picTexture";
            this.picTexture.Size = new System.Drawing.Size(436, 362);
            this.picTexture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picTexture.TabIndex = 17;
            this.picTexture.TabStop = false;
            this.picTexture.Paint += new System.Windows.Forms.PaintEventHandler(this.picTexture_Paint);
            this.picTexture.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picTexture_MouseDown);
            this.picTexture.MouseLeave += new System.EventHandler(this.picTexture_MouseLeave);
            this.picTexture.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picTexture_MouseMove);
            this.picTexture.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picTexture_MouseUp);
            // 
            // scrollZoom
            // 
            this.scrollZoom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scrollZoom.LargeChange = 1;
            this.scrollZoom.Location = new System.Drawing.Point(0, 60);
            this.scrollZoom.Maximum = 0;
            this.scrollZoom.Minimum = -800;
            this.scrollZoom.Name = "scrollZoom";
            this.scrollZoom.Size = new System.Drawing.Size(442, 24);
            this.scrollZoom.TabIndex = 19;
            this.scrollZoom.Value = -50;
            this.scrollZoom.ValueChanged += new System.EventHandler(this.scrollZoom_ValueChanged);
            // 
            // frmPrimWorkshop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(996, 536);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.menu);
            this.MainMenuStrip = this.menu;
            this.Name = "frmPrimWorkshop";
            this.Text = "Prim Workshop";
            this.Shown += new System.EventHandler(this.frmPrimWorkshop_Shown);
            this.menu.ResumeLayout(false);
            this.menu.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTexture)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer;
        private Tao.Platform.Windows.SimpleOpenGlControl glControl;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.HScrollBar scrollRoll;
        private System.Windows.Forms.HScrollBar scrollPitch;
        private System.Windows.Forms.HScrollBar scrollYaw;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboFace;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboPrim;
        private System.Windows.Forms.PictureBox picTexture;
        private System.Windows.Forms.ToolStripMenuItem savePrimXMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem saveTextureToolStripMenuItem;
        private System.Windows.Forms.HScrollBar scrollZoom;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem oBJToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wireframeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem worldBrowserToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem opToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openPrimXMLToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem textureToolStripMenuItem;
    }
}

