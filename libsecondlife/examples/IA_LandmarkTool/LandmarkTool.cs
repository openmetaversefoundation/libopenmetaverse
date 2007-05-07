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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

using libsecondlife;
using libsecondlife.Packets;
using libsecondlife.InventorySystem;

namespace IA_NotecardTool
{
    class LandmarkTool
    {
        private SecondLife _Client;
        private ManualResetEvent ConnectedSignal = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            if (args.Length < 8)
            {
                Console.WriteLine("Usage: NotecardTool [first] [last] [password] [landmarkname] [sim] [x] [y] [z]");
                return;
            }

            LandmarkTool tool = new LandmarkTool();
			int x = 0;
			int y = 0;
			int z = 0;
			try {
				x = int.Parse(args[5]);
				y = int.Parse(args[6]);
				z = int.Parse(args[7]);
			} catch {
				Console.WriteLine("Usage: NotecardTool [first] [last] [password] [landmarkname] [sim] [x] [y] [z]");
                return;
			}
            tool.Connect(args[0], args[1], args[2], args[4], x, y, z);

            if (tool.ConnectedSignal.WaitOne(TimeSpan.FromMinutes(1), false))
            {
                tool.doStuff(args[3]);
                tool.Disconnect();
            }
        }

        private void doStuff(string LandmarkName)
        {
			Console.WriteLine("Located in " + _Client.Network.CurrentSim.Name + " at " + _Client.Self.Position.ToString());
			Console.WriteLine("Ensuring root folder is there");
			_Client.Inventory.GetRootFolder().RequestDownloadContents(false, true, false).RequestComplete.WaitOne();
            Console.WriteLine("Getting Landmark Folder");
			InventoryFolder iFolder =  _Client.Inventory.getFolder("Landmarks");
			Console.WriteLine("Folder retrieved - " + iFolder.FolderID.ToString());
            Console.WriteLine("Creating Landmark");
            iFolder.NewLandmark(LandmarkName, "IA_LandmarkTool");
			Console.WriteLine("Done, now Reading");
			iFolder.RequestDownloadContents(false,false, true).RequestComplete.WaitOne();
			List<InventoryBase> Landmarks = iFolder.GetItemByName(LandmarkName);
			Console.WriteLine(Landmarks.Count.ToString() + " items read");
			foreach ( InventoryBase i in Landmarks) {
				if ( i is InventoryItem ) {
					InventoryItem ii = (InventoryItem)i;
					Console.WriteLine( ii.Name + " - " + ii.ItemID + " is a " + i.GetType());
					if ( ii is InventoryLandmark ) {
						InventoryLandmark l = (InventoryLandmark)ii;
						Console.WriteLine( "Version is " + l.Version.ToString() + " RegionID is " + l.Region.ToString() + " and pos is " + l.Pos.ToString());
					}
				}
			}
        }

        public LandmarkTool()
        {
            try
            {
                _Client = new SecondLife();
				//iManager = new InventoryManager(_Client);
				_Client.Settings.MULTIPLE_SIMS = false;
				_Client.Settings.DEBUG = false;
                _Client.Network.OnConnected += new NetworkManager.ConnectedCallback(Network_OnConnected);
				_Client.Network.RegisterCallback(PacketType.RegionHandshake, new NetworkManager.PacketCallback(OnRegionHandshake));
				_Client.Network.OnLogin += new NetworkManager.LoginCallback(Network_OnLoginChange);
            }
            catch (Exception e)
            {
                // Error initializing the client
                Console.WriteLine();
                Console.WriteLine(e.ToString());
            }
        }

        void Network_OnConnected(object sender)
        {        
			Console.WriteLine("Connected");
		}
		void OnRegionHandshake(Packet packet, Simulator simulator)
        {
			Console.WriteLine("Handshake received for region " + simulator.Name);
            ConnectedSignal.Set();
        }
		void Network_OnLoginChange(NetworkManager.LoginStatus login, string message){
			Console.WriteLine("Login Status Changed - " + message);
		}
        protected bool Connect(string FirstName, string LastName, string Password, string sim, int x, int y, int z)
        {
            Console.WriteLine("Attempting to connect and login to SecondLife.");
            // Login
            _Client.Network.Login(FirstName, LastName, Password, "createlandmark", NetworkManager.StartLocation(sim, x, y, z),  "jef@pleiades.ca");
            return true;
        }

        protected void Disconnect()
        {
            // Logout of Second Life
            Console.WriteLine("Request logout");
            _Client.Network.Logout();
        }
    }
}
