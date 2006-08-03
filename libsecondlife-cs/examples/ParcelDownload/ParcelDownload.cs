using System;
using System.Collections;
using libsecondlife;

namespace ParcelDownloader
{
	/// <summary>
	/// Summary description for ParcelDownload.
	/// </summary>
	class ParcelDownload
	{
		static SecondLife client;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{

			if (args.Length < 3)
			{
				Console.WriteLine("Usage: ParcelDownloader [loginfirstname] [loginlastname] [password]");
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

			Hashtable loginParams = NetworkManager.DefaultLoginValues(args[0], args[1], args[2], "00:00:00:00:00:00",
				"last", 1, 50, 50, 50, "Win", "0", "ParcelDownload", "Adam \"Zaius\" Frisby <adam@gwala.net>");

			if (!client.Network.Login(loginParams))
			{
				// Login failed
				Console.WriteLine("ERROR: " + client.Network.LoginError);
				return;
			}

			// The magic happens in these three lines
			client.CurrentRegion.FillParcels();			// Tell libsl to download parcels
			System.Threading.Thread.Sleep(10000);		// Give it some time to do it
			client.Tick();								// Let things happen

			// Dump some info about our parcels
			foreach(int pkey in client.CurrentRegion.Parcels.Keys) 
			{
				Parcel parcel = (Parcel)client.CurrentRegion.Parcels[pkey];
				parcel.Buy(client,false,new LLUUID());
				Console.WriteLine("<Parcel>");
				Console.WriteLine("\tName: " + parcel.Name);
				Console.WriteLine("\tSize: " + parcel.ActualArea);
				Console.WriteLine("\tDesc: " + parcel.Desc);
			}

			client.Network.Logout();
			return;
		}
	}
}
