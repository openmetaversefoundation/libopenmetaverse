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
	partial class winAbout : System.Windows.Forms.Form
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
			this.grpLabels = new System.Windows.Forms.GroupBox();
			this.lblCreditsNames = new System.Windows.Forms.Label();
			this.lblCredits = new System.Windows.Forms.Label();
			this.lblURL = new System.Windows.Forms.Label();
			this.lblLib = new System.Windows.Forms.Label();
			this.lblName = new System.Windows.Forms.Label();
			this.btnClose = new System.Windows.Forms.Button();
			this.lblRandomQuote = new System.Windows.Forms.Label();
			this.grpLabels.SuspendLayout();
			this.SuspendLayout();
			// 
			// grpLabels
			// 
			this.grpLabels.Controls.Add(this.lblCreditsNames);
			this.grpLabels.Controls.Add(this.lblCredits);
			this.grpLabels.Controls.Add(this.lblURL);
			this.grpLabels.Controls.Add(this.lblLib);
			this.grpLabels.Controls.Add(this.lblName);
			this.grpLabels.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.grpLabels.Location = new System.Drawing.Point(5, 2);
			this.grpLabels.Name = "grpLabels";
			this.grpLabels.Size = new System.Drawing.Size(275, 219);
			this.grpLabels.TabIndex = 0;
			this.grpLabels.TabStop = false;
			this.grpLabels.Text = "About This Program:";
			// 
			// lblCreditsNames
			// 
			this.lblCreditsNames.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblCreditsNames.Location = new System.Drawing.Point(6, 131);
			this.lblCreditsNames.Name = "lblCreditsNames";
			this.lblCreditsNames.Size = new System.Drawing.Size(260, 85);
			this.lblCreditsNames.TabIndex = 4;
			this.lblCreditsNames.Text = "Oz Spade";
			// 
			// lblCredits
			// 
			this.lblCredits.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblCredits.Location = new System.Drawing.Point(83, 101);
			this.lblCredits.Name = "lblCredits";
			this.lblCredits.Size = new System.Drawing.Size(75, 30);
			this.lblCredits.TabIndex = 3;
			this.lblCredits.Text = "Credits:";
			// 
			// lblURL
			// 
			this.lblURL.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblURL.Location = new System.Drawing.Point(74, 73);
			this.lblURL.Name = "lblURL";
			this.lblURL.Size = new System.Drawing.Size(94, 18);
			this.lblURL.TabIndex = 2;
			this.lblURL.Text = "URL goes here";
			// 
			// lblLib
			// 
			this.lblLib.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblLib.Location = new System.Drawing.Point(50, 51);
			this.lblLib.Name = "lblLib";
			this.lblLib.Size = new System.Drawing.Size(216, 22);
			this.lblLib.TabIndex = 1;
			this.lblLib.Text = "Created using libsecondlife";
			// 
			// lblName
			// 
			this.lblName.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblName.Location = new System.Drawing.Point(83, 26);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(75, 25);
			this.lblName.TabIndex = 0;
			this.lblName.Text = "SLChat:";
			// 
			// btnClose
			// 
			this.btnClose.Location = new System.Drawing.Point(220, 227);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(60, 30);
			this.btnClose.TabIndex = 1;
			this.btnClose.Text = "Close";
			this.btnClose.UseVisualStyleBackColor = true;
			this.btnClose.Click += new System.EventHandler(this.btnCloseClick);
			// 
			// lblRandomQuote
			// 
			this.lblRandomQuote.Location = new System.Drawing.Point(5, 241);
			this.lblRandomQuote.Name = "lblRandomQuote";
			this.lblRandomQuote.Size = new System.Drawing.Size(196, 16);
			this.lblRandomQuote.TabIndex = 2;
			this.lblRandomQuote.Text = "You\'re going to have to cheat.";
			// 
			// winAbout
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(289, 262);
			this.Controls.Add(this.lblRandomQuote);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.grpLabels);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(297, 292);
			this.Name = "winAbout";
			this.ShowInTaskbar = false;
			this.Text = "About SLChat";
			this.grpLabels.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Label lblRandomQuote;
		private System.Windows.Forms.GroupBox grpLabels;
		private System.Windows.Forms.Label lblCreditsNames;
		private System.Windows.Forms.Label lblCredits;
		private System.Windows.Forms.Label lblURL;
		private System.Windows.Forms.Label lblLib;
		private System.Windows.Forms.Label lblName;
		private System.Windows.Forms.Button btnClose;
	}
}
