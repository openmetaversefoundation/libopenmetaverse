namespace WinGridProxy
{
    partial class FormSessionSearch
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxFind = new System.Windows.Forms.TextBox();
            this.labelMarkColor = new System.Windows.Forms.Label();
            this.checkBoxMatchCase = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panelColor = new System.Windows.Forms.Panel();
            this.buttonPickColor = new System.Windows.Forms.Button();
            this.checkBoxUnmark = new System.Windows.Forms.CheckBox();
            this.checkBoxSelectMatches = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxPacketsOrMessages = new System.Windows.Forms.ComboBox();
            this.checkBoxSearchSelected = new System.Windows.Forms.CheckBox();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonFind = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.groupBox1.SuspendLayout();
            this.panelColor.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Find:";
            // 
            // textBoxFind
            // 
            this.textBoxFind.Location = new System.Drawing.Point(48, 12);
            this.textBoxFind.Name = "textBoxFind";
            this.textBoxFind.Size = new System.Drawing.Size(231, 20);
            this.textBoxFind.TabIndex = 1;
            this.textBoxFind.TextChanged += new System.EventHandler(this.textBoxFind_TextChanged);
            // 
            // labelMarkColor
            // 
            this.labelMarkColor.AutoSize = true;
            this.labelMarkColor.BackColor = System.Drawing.Color.Transparent;
            this.labelMarkColor.Location = new System.Drawing.Point(27, 7);
            this.labelMarkColor.Name = "labelMarkColor";
            this.labelMarkColor.Size = new System.Drawing.Size(72, 13);
            this.labelMarkColor.TabIndex = 3;
            this.labelMarkColor.Text = "Mark Results:";
            // 
            // checkBoxMatchCase
            // 
            this.checkBoxMatchCase.AutoSize = true;
            this.checkBoxMatchCase.Location = new System.Drawing.Point(9, 46);
            this.checkBoxMatchCase.Name = "checkBoxMatchCase";
            this.checkBoxMatchCase.Size = new System.Drawing.Size(83, 17);
            this.checkBoxMatchCase.TabIndex = 4;
            this.checkBoxMatchCase.Text = "Match Case";
            this.checkBoxMatchCase.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panelColor);
            this.groupBox1.Controls.Add(this.checkBoxUnmark);
            this.groupBox1.Controls.Add(this.checkBoxSelectMatches);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.comboBoxPacketsOrMessages);
            this.groupBox1.Controls.Add(this.checkBoxSearchSelected);
            this.groupBox1.Controls.Add(this.checkBoxMatchCase);
            this.groupBox1.Location = new System.Drawing.Point(15, 38);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(271, 145);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Options";
            // 
            // panelColor
            // 
            this.panelColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.panelColor.Controls.Add(this.buttonPickColor);
            this.panelColor.Controls.Add(this.labelMarkColor);
            this.panelColor.Location = new System.Drawing.Point(10, 111);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(255, 28);
            this.panelColor.TabIndex = 11;
            // 
            // buttonPickColor
            // 
            this.buttonPickColor.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonPickColor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonPickColor.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPickColor.Location = new System.Drawing.Point(134, 3);
            this.buttonPickColor.Name = "buttonPickColor";
            this.buttonPickColor.Size = new System.Drawing.Size(66, 21);
            this.buttonPickColor.TabIndex = 10;
            this.buttonPickColor.Text = "Pick Color";
            this.buttonPickColor.UseVisualStyleBackColor = false;
            this.buttonPickColor.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBoxUnmark
            // 
            this.checkBoxUnmark.AutoSize = true;
            this.checkBoxUnmark.Checked = true;
            this.checkBoxUnmark.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxUnmark.Location = new System.Drawing.Point(144, 92);
            this.checkBoxUnmark.Name = "checkBoxUnmark";
            this.checkBoxUnmark.Size = new System.Drawing.Size(120, 17);
            this.checkBoxUnmark.TabIndex = 9;
            this.checkBoxUnmark.Text = "Unmark Old Results";
            this.checkBoxUnmark.UseVisualStyleBackColor = true;
            // 
            // checkBoxSelectMatches
            // 
            this.checkBoxSelectMatches.AutoSize = true;
            this.checkBoxSelectMatches.Location = new System.Drawing.Point(9, 92);
            this.checkBoxSelectMatches.Name = "checkBoxSelectMatches";
            this.checkBoxSelectMatches.Size = new System.Drawing.Size(100, 17);
            this.checkBoxSelectMatches.TabIndex = 8;
            this.checkBoxSelectMatches.Text = "Select Matches";
            this.checkBoxSelectMatches.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Search:";
            // 
            // comboBoxPacketsOrMessages
            // 
            this.comboBoxPacketsOrMessages.FormattingEnabled = true;
            this.comboBoxPacketsOrMessages.Items.AddRange(new object[] {
            "Both",
            "Messages",
            "Packets"});
            this.comboBoxPacketsOrMessages.Location = new System.Drawing.Point(84, 19);
            this.comboBoxPacketsOrMessages.Name = "comboBoxPacketsOrMessages";
            this.comboBoxPacketsOrMessages.Size = new System.Drawing.Size(180, 21);
            this.comboBoxPacketsOrMessages.TabIndex = 6;
            this.comboBoxPacketsOrMessages.Text = "Both";
            // 
            // checkBoxSearchSelected
            // 
            this.checkBoxSearchSelected.AutoSize = true;
            this.checkBoxSearchSelected.Enabled = false;
            this.checkBoxSearchSelected.Location = new System.Drawing.Point(9, 69);
            this.checkBoxSearchSelected.Name = "checkBoxSearchSelected";
            this.checkBoxSearchSelected.Size = new System.Drawing.Size(168, 17);
            this.checkBoxSearchSelected.TabIndex = 5;
            this.checkBoxSearchSelected.Text = "Search only selected sessions";
            this.checkBoxSearchSelected.UseVisualStyleBackColor = true;
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Location = new System.Drawing.Point(15, 195);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 6;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonFind
            // 
            this.buttonFind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFind.Enabled = false;
            this.buttonFind.Location = new System.Drawing.Point(166, 195);
            this.buttonFind.Name = "buttonFind";
            this.buttonFind.Size = new System.Drawing.Size(120, 23);
            this.buttonFind.TabIndex = 7;
            this.buttonFind.Text = "Find Sessions";
            this.buttonFind.UseVisualStyleBackColor = true;
            this.buttonFind.Click += new System.EventHandler(this.buttonFind_Click);
            // 
            // colorDialog1
            // 
            this.colorDialog1.Color = System.Drawing.Color.Yellow;
            this.colorDialog1.FullOpen = true;
            // 
            // FormSessionSearch
            // 
            this.AcceptButton = this.buttonFind;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(298, 230);
            this.Controls.Add(this.buttonFind);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textBoxFind);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSessionSearch";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Find Sessions";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panelColor.ResumeLayout(false);
            this.panelColor.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxFind;
        private System.Windows.Forms.Label labelMarkColor;
        private System.Windows.Forms.CheckBox checkBoxMatchCase;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonFind;
        private System.Windows.Forms.CheckBox checkBoxUnmark;
        private System.Windows.Forms.CheckBox checkBoxSelectMatches;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxPacketsOrMessages;
        private System.Windows.Forms.CheckBox checkBoxSearchSelected;
        private System.Windows.Forms.Button buttonPickColor;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Panel panelColor;
    }
}