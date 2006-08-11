namespace SLChat
{
    partial class IMTabWindow
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.rtbIMText = new System.Windows.Forms.RichTextBox();
            this.cbxInput = new System.Windows.Forms.ComboBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rtbIMText
            // 
            this.rtbIMText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbIMText.BackColor = System.Drawing.Color.White;
            this.rtbIMText.HideSelection = false;
            this.rtbIMText.Location = new System.Drawing.Point(3, 3);
            this.rtbIMText.Name = "rtbIMText";
            this.rtbIMText.ReadOnly = true;
            this.rtbIMText.Size = new System.Drawing.Size(354, 197);
            this.rtbIMText.TabIndex = 0;
            this.rtbIMText.Text = "";
            // 
            // cbxInput
            // 
            this.cbxInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxInput.FormattingEnabled = true;
            this.cbxInput.Location = new System.Drawing.Point(3, 208);
            this.cbxInput.Name = "cbxInput";
            this.cbxInput.Size = new System.Drawing.Size(273, 21);
            this.cbxInput.TabIndex = 1;
            this.cbxInput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cbxInput_KeyUp);
            this.cbxInput.TextChanged += new System.EventHandler(this.cbxInput_TextChanged);
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Enabled = false;
            this.btnSend.Location = new System.Drawing.Point(282, 206);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 23);
            this.btnSend.TabIndex = 2;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // IMTabWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbxInput);
            this.Controls.Add(this.rtbIMText);
            this.Controls.Add(this.btnSend);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "IMTabWindow";
            this.Size = new System.Drawing.Size(360, 232);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbIMText;
        private System.Windows.Forms.ComboBox cbxInput;
        private System.Windows.Forms.Button btnSend;
    }
}
