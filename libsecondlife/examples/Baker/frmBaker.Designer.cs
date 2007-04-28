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
            this.cmdLoadPic1 = new System.Windows.Forms.Button();
            this.pic2 = new System.Windows.Forms.PictureBox();
            this.pic3 = new System.Windows.Forms.PictureBox();
            this.cmdLoadPic2 = new System.Windows.Forms.Button();
            this.cmdLoadPic3 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pic1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic3)).BeginInit();
            this.SuspendLayout();
            // 
            // pic1
            // 
            this.pic1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pic1.Location = new System.Drawing.Point(12, 12);
            this.pic1.Name = "pic1";
            this.pic1.Size = new System.Drawing.Size(256, 256);
            this.pic1.TabIndex = 0;
            this.pic1.TabStop = false;
            this.pic1.AutoSize = true;
            // 
            // cmdLoadPic1
            // 
            this.cmdLoadPic1.Location = new System.Drawing.Point(193, 274);
            this.cmdLoadPic1.Name = "cmdLoadPic1";
            this.cmdLoadPic1.Size = new System.Drawing.Size(75, 23);
            this.cmdLoadPic1.TabIndex = 1;
            this.cmdLoadPic1.Text = "Load";
            this.cmdLoadPic1.UseVisualStyleBackColor = true;
            this.cmdLoadPic1.Click += new System.EventHandler(this.cmdLoadPic_Click);
            // 
            // pic2
            // 
            this.pic2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pic2.Location = new System.Drawing.Point(274, 12);
            this.pic2.Name = "pic2";
            this.pic2.Size = new System.Drawing.Size(256, 256);
            this.pic2.TabIndex = 2;
            this.pic2.TabStop = false;
            this.pic1.AutoSize = true;
            // 
            // pic3
            // 
            this.pic3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pic3.Location = new System.Drawing.Point(536, 12);
            this.pic3.Name = "pic3";
            this.pic3.Size = new System.Drawing.Size(256, 256);
            this.pic3.TabIndex = 3;
            this.pic3.TabStop = false;
            this.pic1.AutoSize = true;
            // 
            // cmdLoadPic2
            // 
            this.cmdLoadPic2.Location = new System.Drawing.Point(455, 274);
            this.cmdLoadPic2.Name = "cmdLoadPic2";
            this.cmdLoadPic2.Size = new System.Drawing.Size(75, 23);
            this.cmdLoadPic2.TabIndex = 4;
            this.cmdLoadPic2.Text = "Load";
            this.cmdLoadPic2.UseVisualStyleBackColor = true;
            this.cmdLoadPic2.Click += new System.EventHandler(this.cmdLoadPic_Click);
            // 
            // cmdLoadPic3
            // 
            this.cmdLoadPic3.Location = new System.Drawing.Point(717, 274);
            this.cmdLoadPic3.Name = "cmdLoadPic3";
            this.cmdLoadPic3.Size = new System.Drawing.Size(75, 23);
            this.cmdLoadPic3.TabIndex = 5;
            this.cmdLoadPic3.Text = "Load";
            this.cmdLoadPic3.UseVisualStyleBackColor = true;
            this.cmdLoadPic3.Click += new System.EventHandler(this.cmdLoadPic_Click);
            // 
            // frmBaker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(806, 498);
            this.Controls.Add(this.cmdLoadPic3);
            this.Controls.Add(this.cmdLoadPic2);
            this.Controls.Add(this.pic3);
            this.Controls.Add(this.pic2);
            this.Controls.Add(this.cmdLoadPic1);
            this.Controls.Add(this.pic1);
            this.Name = "frmBaker";
            this.Text = "Baker";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmBaker_FormClosing);
            this.Load += new System.EventHandler(this.frmBaker_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pic1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic3)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pic1;
        private System.Windows.Forms.Button cmdLoadPic1;
        private System.Windows.Forms.PictureBox pic2;
        private System.Windows.Forms.PictureBox pic3;
        private System.Windows.Forms.Button cmdLoadPic2;
        private System.Windows.Forms.Button cmdLoadPic3;

    }
}


