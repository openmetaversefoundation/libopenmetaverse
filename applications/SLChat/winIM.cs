/*
 * Created by SharpDevelop.
 * User: Oz
 * Date: 7/19/2006
 * Time: 1:09 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using System.Windows.Forms;

namespace SLChat
{
	/// <summary>
	/// Description of winIM.
	/// </summary>
	public partial class winIM
	{
		public winIM()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		void BtnSendClick(object sender, System.EventArgs e)
		{
			string title = "Oz Spade";
			TabPage myTabPage = new TabPage(title);
			tabMain.TabPages.Add(myTabPage);
			myTabPage.Controls.Add(new RichTextBox());
		}
	}
}
