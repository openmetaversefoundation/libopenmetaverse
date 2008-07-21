namespace Baker
{
    partial class frmBaker
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
            this.pic1 = new System.Windows.Forms.PictureBox();
            this.cmdLoadShirt = new System.Windows.Forms.Button();
            this.scrollWeight = new System.Windows.Forms.HScrollBar();
            this.cmdLoadSkin = new System.Windows.Forms.Button();
            this.cboMask = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.pic1)).BeginInit();
            this.SuspendLayout();
            // 
            // pic1
            // 
            this.pic1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pic1.Location = new System.Drawing.Point(12, 41);
            this.pic1.Name = "pic1";
            this.pic1.Size = new System.Drawing.Size(512, 483);
            this.pic1.TabIndex = 0;
            this.pic1.TabStop = false;
            // 
            // cmdLoadShirt
            // 
            this.cmdLoadShirt.Location = new System.Drawing.Point(449, 530);
            this.cmdLoadShirt.Name = "cmdLoadShirt";
            this.cmdLoadShirt.Size = new System.Drawing.Size(75, 23);
            this.cmdLoadShirt.TabIndex = 1;
            this.cmdLoadShirt.Text = "Load Shirt";
            this.cmdLoadShirt.UseVisualStyleBackColor = true;
            // 
            // scrollWeight
            // 
            this.scrollWeight.Location = new System.Drawing.Point(12, 530);
            this.scrollWeight.Maximum = 255;
            this.scrollWeight.Name = "scrollWeight";
            this.scrollWeight.Size = new System.Drawing.Size(345, 23);
            this.scrollWeight.TabIndex = 0;
            this.scrollWeight.Scroll += new System.Windows.Forms.ScrollEventHandler(this.scrollWeight_Scroll);
            // 
            // cmdLoadSkin
            // 
            this.cmdLoadSkin.Location = new System.Drawing.Point(366, 530);
            this.cmdLoadSkin.Name = "cmdLoadSkin";
            this.cmdLoadSkin.Size = new System.Drawing.Size(75, 23);
            this.cmdLoadSkin.TabIndex = 2;
            this.cmdLoadSkin.Text = "Load Skin";
            this.cmdLoadSkin.UseVisualStyleBackColor = true;
            this.cmdLoadSkin.Click += new System.EventHandler(this.cmdLoadSkin_Click);
            // 
            // cboMask
            // 
            this.cboMask.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMask.FormattingEnabled = true;
            this.cboMask.Items.AddRange(new object[] {
            "glove_length_alpha",
            "gloves_fingers_alpha",
            "jacket_length_lower_alpha",
            "jacket_length_upper_alpha",
            "jacket_open_lower_alpha",
            "jacket_open_upper_alpha",
            "pants_length_alpha",
            "pants_waist_alpha",
            "shirt_bottom_alpha",
            "shirt_collar_alpha",
            "shirt_collar_back_alpha",
            "shirt_sleeve_alpha",
            "shoe_height_alpha",
            "skirt_length_alpha",
            "skirt_slit_front_alpha",
            "skirt_slit_left_alpha",
            "skirt_slit_right_alpha"});
            this.cboMask.Location = new System.Drawing.Point(358, 12);
            this.cboMask.Name = "cboMask";
            this.cboMask.Size = new System.Drawing.Size(166, 21);
            this.cboMask.TabIndex = 3;
            this.cboMask.SelectedIndexChanged += new System.EventHandler(this.cboMask_SelectedIndexChanged);
            // 
            // frmBaker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(538, 563);
            this.Controls.Add(this.cboMask);
            this.Controls.Add(this.cmdLoadSkin);
            this.Controls.Add(this.scrollWeight);
            this.Controls.Add(this.cmdLoadShirt);
            this.Controls.Add(this.pic1);
            this.Name = "frmBaker";
            this.Text = "Baker";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmBaker_FormClosing);
            this.Load += new System.EventHandler(this.frmBaker_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pic1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pic1;
        private System.Windows.Forms.HScrollBar scrollWeight;
        private System.Windows.Forms.Button cmdLoadShirt;
        private System.Windows.Forms.Button cmdLoadSkin;
        private System.Windows.Forms.ComboBox cboMask;

    }
}


