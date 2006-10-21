namespace SLChat
{
    partial class frmAbout
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
        	this.btnClose = new System.Windows.Forms.Button();
        	this.rtbCredits = new System.Windows.Forms.RichTextBox();
        	this.SuspendLayout();
        	// 
        	// btnClose
        	// 
        	this.btnClose.Location = new System.Drawing.Point(227, 231);
        	this.btnClose.Name = "btnClose";
        	this.btnClose.Size = new System.Drawing.Size(75, 23);
        	this.btnClose.TabIndex = 1;
        	this.btnClose.Text = "Close";
        	this.btnClose.UseVisualStyleBackColor = true;
        	this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
        	// 
        	// rtbCredits
        	// 
        	this.rtbCredits.BackColor = System.Drawing.Color.White;
        	this.rtbCredits.Location = new System.Drawing.Point(12, 12);
        	this.rtbCredits.Name = "rtbCredits";
        	this.rtbCredits.ReadOnly = true;
        	this.rtbCredits.Size = new System.Drawing.Size(290, 209);
        	this.rtbCredits.TabIndex = 2;
        	this.rtbCredits.Text = "";
        	this.rtbCredits.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.Link_Clicked);
        	// 
        	// frmAbout
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(314, 266);
        	this.Controls.Add(this.rtbCredits);
        	this.Controls.Add(this.btnClose);
        	this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        	this.MaximizeBox = false;
        	this.MinimizeBox = false;
        	this.Name = "frmAbout";
        	this.ShowIcon = false;
        	this.ShowInTaskbar = false;
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        	this.Text = "About SLChat";
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.RichTextBox rtbCredits;

        #endregion

        private System.Windows.Forms.Button btnClose;
    }
}
