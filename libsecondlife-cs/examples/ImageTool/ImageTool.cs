using System;
using System.Collections;
using System.IO;

using libsecondlife;
using libsecondlife.InventorySystem;
using libsecondlife.AssetSystem;
using libsecondlife.Utils;

namespace ImageTool
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class ImageTool : libsecondlife.Utils.InventoryApp
	{
		private LLUUID _ImageID;
		private string _FileName;
		private bool   _Put;

		/// <summary>
		/// Sample texture for downloading: 0444bf21-f77e-7f63-89e9-b839ec66bc15
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			if( ImageTools.Check4Tools() == false )
			{
				return;
			}

			if (args.Length < 5)
			{
				Console.WriteLine("Usage: ImageTool [first] [last] [password] [get] [uuid] [(filename)]");
				Console.WriteLine("Usage: ImageTool [first] [last] [password] [put] [filename]");

				Console.WriteLine();
				Console.WriteLine("Example: ImageTool John Doe Password get 0444bf21-f77e-7f63-89e9-b839ec66bc15 cloud.tif");
				Console.WriteLine("Example: ImageTool John Doe Password put Sample.tif");
				return;
			}


			LLUUID id = null;
			string filename = "";
			bool put = false;
			if( args[3].ToLower().Equals("put") )
			{
				put = true;
				filename = args[4];
			} 
			else 
			{
				id = new LLUUID(args[4]);
				if( args.Length == 6 )
				{
					filename = args[5];
				} 
				else 
				{
					filename = args[4] + ".tif";
				}
			}

			ImageTool it = new ImageTool( id, filename, put );
			it.Connect(args);
			it.doStuff();
			it.Disconnect();

			System.Threading.Thread.Sleep(500);
		}

		protected ImageTool( LLUUID imageID, string filename, bool put )
		{
			_ImageID  = imageID;
			_FileName = filename;
			_Put      = put;
		}

		protected override void doStuff()
		{
			if( _Put )
			{
				Console.WriteLine("Reading: " + _FileName);
				
				byte[] j2cdata = ImageTools.ReadJ2CData( _FileName );

				Console.WriteLine("Connecting to your Texture folder...");
				InventoryFolder iFolder = AgentInventory.getFolder("Textures");

				Console.WriteLine("Uploading Texture...");
				iFolder.NewImage( _FileName, "ImageTool Upload", j2cdata );
			} 
			else 
			{
				Console.WriteLine("Downloading: " + _ImageID);

				ImageManager im = new ImageManager( base.client );
				byte[] j2cdata = im.RequestImage( _ImageID );

				Console.WriteLine("Writing to: " + _FileName);
				ImageTools.WriteJ2CAsTiff( _FileName, j2cdata );
			}

			Console.WriteLine("Done...");

		}

	}
}