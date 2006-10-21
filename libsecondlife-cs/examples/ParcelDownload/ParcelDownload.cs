/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names 
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 */

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

			client = new SecondLife();

			Hashtable loginParams = NetworkManager.DefaultLoginValues(args[0], args[1], args[2], "00:00:00:00:00:00",
				"last", "Win", "0", "ParcelDownload", "Adam \"Zaius\" Frisby <adam@gwala.net>");

			if (!client.Network.Login(loginParams))
			{
				// Login failed
				Console.WriteLine("ERROR: " + client.Network.LoginError);
				return;
			}

			// The magic happens in these three lines
			client.Network.CurrentSim.Region.FillParcels();			// Tell libsl to download parcels
			System.Threading.Thread.Sleep(10000);		// Give it some time to do it
			client.Tick();								// Let things happen

			// Dump some info about our parcels
            foreach (int pkey in client.Network.CurrentSim.Region.Parcels.Keys) 
			{
                Parcel parcel = (Parcel)client.Network.CurrentSim.Region.Parcels[pkey];
				// Probably should comment this out :-)
				//parcel.Buy(client,false,new LLUUID());
				Console.WriteLine("<Parcel>");
				Console.WriteLine("\tName: " + parcel.Name);
				Console.WriteLine("\tSize: " + parcel.Area);
				Console.WriteLine("\tDesc: " + parcel.Desc);
			}

			client.Network.Logout();
			return;
		}
	}
}
