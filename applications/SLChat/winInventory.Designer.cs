/*
 * Created by SharpDevelop.
 * User: ${USER}
 * Date: ${DATE}
 * Time: ${TIME}
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace SLChat
{
	partial class winInventory : System.Windows.Forms.Form
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
			this.richPrint = new System.Windows.Forms.RichTextBox();
			this.btnLoad = new System.Windows.Forms.Button();
			this.statBar = new System.Windows.Forms.StatusStrip();
			this.treeInv = new System.Windows.Forms.TreeView();
			this.SuspendLayout();
			// 
			// richPrint
			// 
			this.richPrint.Location = new System.Drawing.Point(2, 254);
			this.richPrint.Name = "richPrint";
			this.richPrint.Size = new System.Drawing.Size(185, 33);
			this.richPrint.TabIndex = 0;
			this.richPrint.Text = "";
			// 
			// btnLoad
			// 
			this.btnLoad.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnLoad.Location = new System.Drawing.Point(193, 254);
			this.btnLoad.Name = "btnLoad";
			this.btnLoad.Size = new System.Drawing.Size(95, 33);
			this.btnLoad.TabIndex = 1;
			this.btnLoad.Text = "Load Inventory";
			this.btnLoad.UseVisualStyleBackColor = true;
			//this.btnLoad.Click += new System.EventHandler(this.BtnLoadClick);
			// 
			// statBar
			// 
			this.statBar.Location = new System.Drawing.Point(0, 290);
			this.statBar.Name = "statBar";
			this.statBar.Size = new System.Drawing.Size(292, 22);
			this.statBar.TabIndex = 2;
			this.statBar.Text = "Status Bar";
			// 
			// treeInv
			// 
			this.treeInv.Location = new System.Drawing.Point(2, 4);
			this.treeInv.Name = "treeInv";
			this.treeInv.Size = new System.Drawing.Size(286, 244);
			this.treeInv.TabIndex = 3;
			// 
			// winInventory
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(292, 312);
			this.Controls.Add(this.treeInv);
			this.Controls.Add(this.statBar);
			this.Controls.Add(this.btnLoad);
			this.Controls.Add(this.richPrint);
			this.Name = "winInventory";
			this.Text = "Inventory";
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.TreeView treeInv;
		private System.Windows.Forms.StatusStrip statBar;
		private System.Windows.Forms.Button btnLoad;
		private System.Windows.Forms.RichTextBox richPrint;
	}
}
