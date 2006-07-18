using System;
using System.Collections;
using System.IO;

using libsecondlife;
using libsecondlife.InventorySystem;

namespace InventoryDump
{
	class InventoryDump
	{
		static private SecondLife client;

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

			String sOutputFile = "output.xml";
			if( args.Length == 4 )
			{
				sOutputFile = args[3];
			}

			try
			{
				client = new SecondLife("keywords.txt", "protocol.txt");
			}
			catch (Exception e)
			{
				// Error initializing the client, probably missing file(s)
				Console.WriteLine(e.ToString());
				return;
			}


			// Setup Login to Second Life
			Hashtable loginParams = NetworkManager.DefaultLoginValues(args[0], args[1], args[2], "00:00:00:00:00:00",
				"last", 1, 10, 10, 10, "Win", "0", "inventorydump", "static.sprocket@gmail.com");
			Hashtable loginReply = new Hashtable();

			// Request information on the Root Inventory Folder, and Inventory Skeleton
			//			alAdditionalInfo.Add("inventory-skeleton");

			ArrayList alAdditionalInfo = new ArrayList();
			alAdditionalInfo.Add("inventory-root");
			loginParams.Add("options",alAdditionalInfo);

			// Login
			if (!client.Network.Login(loginParams))
			{
				// Login failed
				Console.WriteLine("Error logging in: " + client.Network.LoginError);
				return;
			}


			// Login was successful


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

				Console.WriteLine("Writing Inventory to " + sOutputFile);
				// Save inventory to file.
				StreamWriter sw = File.CreateText(sOutputFile);
				sw.Write(slInventory.getRootFolder().toXML() );
				sw.Close();
				Console.WriteLine("Done.");
			} catch ( Exception e ) {
				Console.WriteLine( e.Message );
				Console.WriteLine("An error occured while downloading inventory, please report this along with any output to Static Sprocket.");
			}



			// Logout of Second Life
			client.Network.Logout();
		}
	}
}

