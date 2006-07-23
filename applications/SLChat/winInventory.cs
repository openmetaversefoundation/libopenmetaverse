/*
 * Created by SharpDevelop.
 * User: Oz
 * Date: 7/18/2006
 * Time: 7:32 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Threading;
using libsecondlife;
using libsecondlife.InventorySystem;

namespace SLChat
{
	/// <summary>
	/// Description of winInventory.
	/// </summary>
	public partial class winInventory
	{
		private SecondLife client;
		public static ChatScreen winChat;
		string newline = System.Environment.NewLine;
		
		public winInventory(ChatScreen chat, SecondLife clien)
		{
			//Just so we can communicate easily back to our parent, dirty hack probably.
			client = clien;
			winChat = chat;
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
			//richPrint.Text += "WARNING: Loading inventory is highly volital and should only be done on *SMALL* inventories.";
		}
		/*
		void BtnLoadClick(object sender, System.EventArgs e)
		{
			Thread invthread = new Thread(new ThreadStart(LoadInventory));
			invthread.Start();
		}
		
		public void LoadInventory()
		{
			string sOutputFile = "output.xml";
			// Get Root Inventory Folder UUID
				ArrayList alInventoryRoot = (ArrayList)client.Network.LoginValues["inventory-root"];
				Hashtable htInventoryRoot = (Hashtable)alInventoryRoot[0];
				LLUUID uuidRootFolder = new LLUUID( (string)htInventoryRoot["folder_id"] );

				// Initialize Inventory object
				InventoryManager slInventory = new InventoryManager(client, uuidRootFolder);

				// Request Inventory Download
				try
				{
					slInventory.DownloadInventory();

					statBar.Text = "Writing Inventory to " + sOutputFile;
					// Save inventory to file.
					StreamWriter sw = File.CreateText(sOutputFile);
					sw.Write(slInventory.getRootFolder().toXML() );
					sw.Close();
					statBar.Text = "Write completed.";
				} catch ( Exception e ) {
					richPrint.Text += e.Message ;
					richPrint.Text += "An error occured while downloading inventory, please report this along with any output to Static Sprocket." + newline;
				}
		}
		*/
	}
}
