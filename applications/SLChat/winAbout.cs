/*
 * Created by SharpDevelop.
 * User: Oz
 * Date: 7/22/2006
 * Time: 10:39 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using System.Windows.Forms;

namespace SLChat
{
	/// <summary>
	/// Description of winAbout.
	/// About form, add crazy shit here.
	/// Add your name to the credits if you've made a contribution
	/// to this project! :D
	/// </summary>
	public partial class winAbout
	{
		public winAbout()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		void btnCloseClick(object sender, System.EventArgs e)
		{
			this.Dispose();
		}
	}
}
