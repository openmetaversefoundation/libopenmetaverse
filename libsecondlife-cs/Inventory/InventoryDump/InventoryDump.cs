using System;
using System.Collections;
using System.IO;

using libsecondlife;
using libsecondlife.InventorySystem;

namespace InventoryDump
{
	class InventoryDump : libsecondlife.InventoryApp
	{
		private string sOutputFile;
		private bool   bOutputAssets;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length < 3)
			{
				Console.WriteLine("Usage: InventoryDump [loginfirstname] [loginlastname] [password] [output.xml]");
				return;
			}

			

			InventoryDump id = new InventoryDump();
			
			if( args.Length == 4 )
			{
				id.sOutputFile = args[3];
			} else {
				id.sOutputFile = "output.xml";
			}

			id.bOutputAssets = false;

			id.Connect(args);
			id.doStuff();
			id.Disconnect();

			System.Threading.Thread.Sleep(500);
		}

		override protected void doStuff()
		{
			if( AgentInventory == null )
			{
				return;
			}

			// Request Inventory Download
			try
			{
				AgentInventory.DownloadInventory();

				Console.WriteLine("Writing Inventory to " + sOutputFile);
				// Save inventory to file.
				StreamWriter sw = File.CreateText(sOutputFile);
				sw.Write(AgentInventory.getRootFolder().toXML( bOutputAssets ) );
				sw.Close();
				Console.WriteLine("Done.");
			} 
			catch ( Exception e ) 
			{
				Console.WriteLine( e.Message );
				Console.WriteLine("An error occured while downloading inventory, please report this along with any output to Static Sprocket.");
			}
		}
	}
}

