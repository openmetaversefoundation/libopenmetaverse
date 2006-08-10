using System;
using System.Collections;
using System.IO;

using libsecondlife;
using libsecondlife.InventorySystem;
using libsecondlife.AssetSystem.PacketHelpers;

namespace InventoryTools
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class CreateNotecard : libsecondlife.InventoryApp
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			CreateNotecard cn = new CreateNotecard();
			cn.Connect(args);
			cn.doStuff();
			cn.Disconnect();

			System.Threading.Thread.Sleep(500);
		}


		override protected void doStuff()
		{
			if( AgentInventory == null )
			{
				return;
			}

			client.Avatar.OnChat +=new ChatCallback(Avatar_OnChat);

			
			// Find folder to put notecard in
			InventoryFolder ifNotecards = AgentInventory.getFolder("Notecards");

			// Create Notecard
			Console.WriteLine("Create Notecard");
			InventoryItem iiNotecard = ifNotecards.NewNotecard("Test Card " + System.DateTime.Now.ToShortTimeString(),"Test Description", "Test Body");

			iiNotecard.GiveTo("e225438416bc4f1788ac5beb5b41f141");
//			iiNotecard.GiveTo("4403a8f0-245c-a56b-23a7-cc1c72c8f2e9");
//			iiNotecard.GiveTo("25472683-cb32-4516-904a-6cd0ecabf128");

			

/*
			StreamReader sr = System.IO.File.OpenText("bignote.txt");
			String body = sr.ReadToEnd();
			body = "Testing Inventory Delivery";

			// Create Notecard
			Console.WriteLine("Create Notecard");
			InventoryNotecard iNotecard = ifNotecards.NewNotecard("Big Card " + System.DateTime.Now.ToShortTimeString(),"Test Description", body);
*/

			// Delete Notecard
//			Console.WriteLine("Delete Notecard");
//			iNotecard.Delete();


/*
			// Create Folder
			InventoryFolder ifTestFolderA = ifNotecards.CreateFolder("SubA");
			InventoryFolder ifTestFolderB = ifNotecards.CreateFolder("SubB");

			// Move a folder
			ifTestFolderB.MoveTo( ifTestFolderA );

			// Rename a folder
			ifTestFolderA.Name = "Sub1";

			// Delete Folder
			ifTestFolderB.Delete();

			// Create Notecard
			Console.WriteLine("Create Notecard");
			InventoryItem iiNotecard = ifNotecards.NewNotecard("Test Card " + System.DateTime.Now.ToShortTimeString(),"Test Description", "Test Body");

			// Move Notecard
			Console.WriteLine("Move Notecard");
			iiNotecard.MoveTo( ifTestFolderA );			

			// Delete Notecard
			Console.WriteLine("Delete Notecard");
			iiNotecard.Delete();

			// Delete Folder
			ifTestFolderA.Delete();

			// Download inventory and output to visually verify Folder has been deleted
			AgentInventory.DownloadInventory();
			ifNotecards = AgentInventory.getFolder("Notecards");
			Console.WriteLine(ifNotecards.toXML());
*/
			}

		private void Avatar_OnChat(string message, byte audible, byte type, byte sourcetype, string name, LLUUID id, byte command, LLUUID commandID)
		{

		}
	}
}