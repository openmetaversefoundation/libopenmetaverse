using System;
using System.Collections;

using libsecondlife;
using libsecondlife.InventorySystem;

namespace libsecondlife
{
	/// <summary>
	/// Summary description for InventoryApp.
	/// </summary>
	abstract public class InventoryApp
	{
		protected SecondLife client;
		protected InventoryManager AgentInventory;



		protected void Connect(string[] args)
		{
			if (args.Length < 3)
			{
				Console.WriteLine("Usage: InventoryDump [loginfirstname] [loginlastname] [password]");
				return;
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
				"last", 1, 10, 10, 10, "Win", "0", "createnotecard", "static.sprocket@gmail.com");
			Hashtable loginReply = new Hashtable();

			// Request information on the Root Inventory Folder, and Inventory Skeleton
			ArrayList alAdditionalInfo = new ArrayList();
			alAdditionalInfo.Add("inventory-root");
			alAdditionalInfo.Add("inventory-skeleton");
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
			LLUUID agentRootFolderID = new LLUUID( (string)htInventoryRoot["folder_id"] );

			// Initialize Inventory Manager object
			AgentInventory = new InventoryManager(client, agentRootFolderID);

			// and request an inventory download
			AgentInventory.DownloadInventory();


		}

		protected void Disconnect()
		{
			// Logout of Second Life

			Console.WriteLine("Request logout");
			client.Network.Logout();
		}

		abstract protected void doStuff();
	}
}
