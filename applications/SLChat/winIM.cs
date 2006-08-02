/*
 * Created by SharpDevelop.
 * User: Oz
 * Date: 7/19/2006
 * Time: 1:09 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SLChat
{
	/// <summary>
	/// Description of winIM.
	/// </summary>
	public partial class winIM
	{
		public static winIM that;
		RichTextBox rChatThing = new System.Windows.Forms.RichTextBox();
		IMwin[] arrIM = new IMwin[4];
		public int tabCount;
		
		public winIM()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			that = this;
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		void BtnSendClick(object sender, System.EventArgs e)
		{
			for(int i=0;i<arrIM.Length;i++)
			{
				if(arrIM[i]==null)
				{
					txtIMEntry.Text = "POP";
				}else if(arrIM[i].name=="Oz Spade")
				{
					arrIM[i].Text(txtIMEntry.Text);
					break;
				}
				if(i+1==arrIM.Length){
					CreateTab();
				}
			}
			/*
			int i = tabMain.SelectedIndex - 1;
			if(i>=0)
			{
				arrIM[i].Text(txtIMEntry.Text);
			}*/
		}
		
		void BtnCreateClick(object sender, System.EventArgs e)
		{
			CreateTab();
		}
		
		public void CreateTab()
		{
			int i = tabCount;
			arrIM[i] = new IMwin();
			that.txtIMEntry.Text = arrIM.Length.ToString();
			arrIM[i].Tabs();
			tabCount++;
		}
		
		public class IMwin
		{
			public string name;
			
			public IMwin()
			{
				
			}
			
			public void Tabs()
			{
				//that.txtIMEntry.Text = "test";
				this.tabNext = new System.Windows.Forms.TabPage();
				this.txtIMThing = new System.Windows.Forms.TextBox();
				this.btnDel = new System.Windows.Forms.Button();
				this.btnTes = new System.Windows.Forms.Button();
				this.tabNext.SuspendLayout();
				// 
				// tabGroups
				// 
				this.tabNext.Controls.Add(this.btnTes);
				this.tabNext.Controls.Add(this.btnDel);
				this.tabNext.Location = new System.Drawing.Point(4, 22);
				this.tabNext.Name = "tabNext";
				this.tabNext.Padding = new System.Windows.Forms.Padding(3);
				this.tabNext.Size = new System.Drawing.Size(421, 170);
				this.tabNext.TabIndex = 0;
				this.tabNext.Text = "Next";
				this.tabNext.UseVisualStyleBackColor = true;
				// 
				// txtIMEntry
				// 
				this.txtIMThing.Location = new System.Drawing.Point(3, 6);
				this.txtIMThing.Name = "txtIMThing";
				this.txtIMThing.Size = new System.Drawing.Size(412, 160);
				this.txtIMThing.TabIndex = 1;
				// 
				// button1
				// 
				this.btnDel.Location = new System.Drawing.Point(290, 125);
				this.btnDel.Name = "btnDel";
				this.btnDel.Size = new System.Drawing.Size(108, 28);
				this.btnDel.TabIndex = 1;
				this.btnDel.Text = "Delete";
				this.btnDel.UseVisualStyleBackColor = true;
				this.btnDel.Click += new System.EventHandler(this.BtnDelClick);
				// 
				// button2
				// 
				this.btnTes.Location = new System.Drawing.Point(178, 125);
				this.btnTes.Name = "btnTes";
				this.btnTes.Size = new System.Drawing.Size(106, 28);
				this.btnTes.TabIndex = 2;
				this.btnTes.Text = "Test";
				this.btnTes.UseVisualStyleBackColor = true;
				this.btnTes.Click += new System.EventHandler(this.BtnTesClick);
			
				this.tabNext.Controls.Add(this.txtIMThing);
				that.tabMain.Controls.Add(this.tabNext);
				this.tabNext.ResumeLayout(false);
			
				/*
				string title = "Oz Spade";
				TabPage myTabPage = new TabPage(title);
				tabMain.TabPages.Add(myTabPage);
				myTabPage.Controls.Add(rChatThing);*/
			}
			private System.Windows.Forms.Button btnDel;
			private System.Windows.Forms.Button btnTes;
			private System.Windows.Forms.TextBox txtIMThing;
			private System.Windows.Forms.TabPage tabNext;

			void BtnTesClick(object sender, System.EventArgs e)
			{
				name = "Oz Spade";
				this.tabNext.Text = name;
			}
			
			void BtnDelClick(object sender, System.EventArgs e)
			{
				that.tabMain.Controls.Remove(this.tabNext);
				Array.Clear(that.arrIM,that.tabMain.SelectedIndex,1);
				that.tabCount--;
			}
			
			public void Text(string txt)
			{
				txtIMThing.Text = txt;
				if(!this.tabNext.Visible)
				{
					this.tabNext.Text = name + " (N)";
				}else{
					this.tabNext.Text = name;
				}
			}
		}
	}
}
