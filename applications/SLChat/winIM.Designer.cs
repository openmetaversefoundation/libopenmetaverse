/*
 * Created by SharpDevelop.
 * User: ${USER}
 * Date: ${DATE}
 * Time: ${TIME}
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace SLChat
{
	partial class winIM : System.Windows.Forms.Form
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.tabMain = new System.Windows.Forms.TabControl();
			this.tabGroups = new System.Windows.Forms.TabPage();
			this.listGroups = new System.Windows.Forms.ListBox();
			this.txtIMEntry = new System.Windows.Forms.TextBox();
			this.btnSend = new System.Windows.Forms.Button();
			this.tabMain.SuspendLayout();
			this.tabGroups.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabMain
			// 
			this.tabMain.Controls.Add(this.tabGroups);
			this.tabMain.Location = new System.Drawing.Point(3, 1);
			this.tabMain.Name = "tabMain";
			this.tabMain.SelectedIndex = 0;
			this.tabMain.Size = new System.Drawing.Size(429, 196);
			this.tabMain.TabIndex = 0;
			// 
			// tabGroups
			// 
			this.tabGroups.Controls.Add(this.listGroups);
			this.tabGroups.Location = new System.Drawing.Point(4, 22);
			this.tabGroups.Name = "tabGroups";
			this.tabGroups.Padding = new System.Windows.Forms.Padding(3);
			this.tabGroups.Size = new System.Drawing.Size(421, 170);
			this.tabGroups.TabIndex = 0;
			this.tabGroups.Text = "Groups";
			this.tabGroups.UseVisualStyleBackColor = true;
			// 
			// listGroups
			// 
			this.listGroups.FormattingEnabled = true;
			this.listGroups.Location = new System.Drawing.Point(3, 6);
			this.listGroups.Name = "listGroups";
			this.listGroups.Size = new System.Drawing.Size(412, 160);
			this.listGroups.TabIndex = 0;
			// 
			// txtIMEntry
			// 
			this.txtIMEntry.Location = new System.Drawing.Point(3, 210);
			this.txtIMEntry.Name = "txtIMEntry";
			this.txtIMEntry.Size = new System.Drawing.Size(363, 21);
			this.txtIMEntry.TabIndex = 1;
			// 
			// btnSend
			// 
			this.btnSend.Location = new System.Drawing.Point(372, 210);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(60, 26);
			this.btnSend.TabIndex = 2;
			this.btnSend.Text = "Send";
			this.btnSend.UseVisualStyleBackColor = true;
			this.btnSend.Click += new System.EventHandler(this.BtnSendClick);
			// 
			// winIM
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(437, 242);
			this.Controls.Add(this.btnSend);
			this.Controls.Add(this.txtIMEntry);
			this.Controls.Add(this.tabMain);
			this.Name = "winIM";
			this.Text = "Instant Messages";
			this.tabMain.ResumeLayout(false);
			this.tabGroups.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.Button btnSend;
		private System.Windows.Forms.TextBox txtIMEntry;
		private System.Windows.Forms.ListBox listGroups;
		private System.Windows.Forms.TabPage tabGroups;
		private System.Windows.Forms.TabControl tabMain;
	}
}
