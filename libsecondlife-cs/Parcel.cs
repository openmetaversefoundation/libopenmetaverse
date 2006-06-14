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
using System.Timers;
using System.Collections;

namespace libsecondlife
{
	public class Parcel
	{
		public LLUUID ID;
		public LLUUID OwnerID;
		public LLUUID SnapshotID;
		public ulong RegionHandle;
		public string Name;
		public string SimName;
		public string Desc;
		public int SalePrice;
		public int ActualArea;
		public float GlobalX;
		public float GlobalY;
		public float GlobalZ;
		public float Dwell;
	}

	public class ParcelManager
	{
		public ArrayList ParcelsForSale;

		private SecondLife Client;
		private bool ReservedNewbie;
		private bool ForSale;
		private bool Auction;
		private System.Timers.Timer Timer;
		private bool Timeout;
		private bool DirLandTimeout;
		private ArrayList ParcelList;

		public ParcelManager(SecondLife client)
		{
			Client = client;
			ParcelsForSale = new ArrayList();
			Timer = new System.Timers.Timer();
			Timer.Elapsed += new ElapsedEventHandler(TimerEvent);
			Timeout = false;

			// Setup the callbacks
			PacketCallback callback = new PacketCallback(DirLandReplyHandler);
			Client.Network.InternalCallbacks["DirLandReply"] = callback;
			callback = new PacketCallback(ParcelInfoReplyHandler);
			Client.Network.InternalCallbacks["ParcelInfoReply"] = callback;
		}

		public void RequestParcelInfo(ArrayList parcels)
		{
			ParcelList = parcels;

			// Setup the timer
			Timer.Interval = 8000;
			Timeout = false;
			Timer.Stop();
			Timer.Start();

			foreach (Parcel parcel in ParcelList)
			{
				if (parcel != null)
				{
					Packet parcelInfoPacket = PacketBuilder.ParcelInfoRequest(Client.Protocol, parcel.ID, 
						Client.Network.LoginValues.AgentID, Client.Network.LoginValues.SessionID);
					Client.Network.SendPacket(parcelInfoPacket);

					// Rate limiting
					System.Threading.Thread.Sleep(10);
				}
				else
				{
					Helpers.Log("Null parcel in ParcelList", Helpers.LogLevel.Info);
				}
			}

			while (!Timeout)
			{
				System.Threading.Thread.Sleep(0);
			}
		}

		private void ParcelInfoReplyHandler(Packet packet, Circuit circuit)
		{
			// Reset the timer
			Timer.Stop();
			Timer.Start();

			string simName = "";
			int actualArea = 0;
			float globalX = 0.0F;
			float globalY = 0.0F;
			float globalZ = 0.0F;
			LLUUID parcelID = null;
			string name = "";
			string desc = "";
			int salePrice = 0;
			LLUUID ownerID = null;
			LLUUID snapshotID = null;
			float dwell = 0.0F;
			bool found = false;

			foreach (Block block in packet.Blocks())
			{
				foreach (Field field in block.Fields)
				{
					if (field.Layout.Name == "ParcelID")
					{
						parcelID = (LLUUID)field.Data;
					}
					else if (field.Layout.Name == "ActualArea")
					{
						actualArea = (int)field.Data;
					}
					else if (field.Layout.Name == "SalePrice")
					{
						salePrice = (int)field.Data;
					}
					else if (field.Layout.Name == "Name") 
					{
						name = System.Text.Encoding.UTF8.GetString((byte[])field.Data);
					}
					else if (field.Layout.Name == "SimName") 
					{
						simName = System.Text.Encoding.UTF8.GetString((byte[])field.Data);
					} 
					else if (field.Layout.Name == "GlobalX") 
					{
						globalX = (float)field.Data;
					} 
					else if (field.Layout.Name == "GlobalY") 
					{
						globalY = (float)field.Data;
					} 
					else if (field.Layout.Name == "GlobalZ") 
					{
						globalZ = (float)field.Data;
					} 
					else if (field.Layout.Name == "Desc") 
					{
						desc = System.Text.Encoding.UTF8.GetString((byte[])field.Data);
					} 
					else if (field.Layout.Name == "OwnerID") 
					{
						ownerID = (LLUUID)field.Data;
					} 
					else if (field.Layout.Name == "SnapshotID") 
					{
						snapshotID = (LLUUID)field.Data;
					} 
					else if (field.Layout.Name == "Dwell") 
					{
						dwell = (float)field.Data;
					}
				}
			}

			// Find this parcel in the list that was passed in
			foreach (Parcel parcel in ParcelList)
			{
				if (parcel.ID == parcelID)
				{
					parcel.SimName = simName;
					parcel.ActualArea = actualArea;
					parcel.GlobalX = globalX;
					parcel.GlobalY = globalY;
					parcel.GlobalZ = globalZ;
					parcel.Name = name;
					parcel.Desc = desc;
					parcel.SalePrice = salePrice;
					parcel.OwnerID = ownerID;
					parcel.SnapshotID = snapshotID;
					parcel.Dwell = dwell;

					// Get RegionHandle from GlobalX/GlobalY
					uint handleX = (uint)Math.Floor(parcel.GlobalX / 256.0F);
					uint handleY = (uint)Math.Floor(parcel.GlobalY / 256.0F);
					parcel.RegionHandle = Helpers.BuildULong(handleX, handleY);

					found = true;
					break;
				}
			}

			if (!found)
			{
				Helpers.Log("Got a ParcelInfoReply on " + parcelID.ToString() + " that we never requested",
					Helpers.LogLevel.Warning);
			}
		}

		public int DirLandRequest(bool reservedNewbie, bool forSale, bool auction)
		{
			// Set the class-wide variables so the callback has them
			ReservedNewbie = reservedNewbie;
			ForSale = forSale;
			Auction = auction;

			// Clear the list
            ParcelsForSale.Clear();

			// Setup the timer
			Timer.Interval = 15000;
			DirLandTimeout = false;
			Timer.Stop();
			Timer.Start();

			LLUUID queryID = new LLUUID();
			Packet landQuery = PacketBuilder.DirLandQuery(Client.Protocol, ReservedNewbie, ForSale, queryID, 
				Auction, 0, Client.Network.LoginValues.AgentID, Client.Network.LoginValues.SessionID);
			Client.Network.SendPacket(landQuery);

			while (!DirLandTimeout)
			{
				System.Threading.Thread.Sleep(0);
			}

			return ParcelsForSale.Count;
		}

		private void DirLandReplyHandler(Packet packet, Circuit circuit)
		{
			if (!DirLandTimeout)
			{
				// Reset the timer
				Timer.Stop();
				Timer.Start();

				foreach (Block block in packet.Blocks())
				{
					Parcel parcel = new Parcel();

					if (block.Layout.Name == "QueryReplies")
					{
						foreach (Field field in block.Fields)
						{
							if (field.Layout.Name == "ReservedNewbie")
							{
								if ((bool)field.Data != ReservedNewbie)
								{
									goto Skip;
								}
							}
							else if (field.Layout.Name == "Auction")
							{
								if ((bool)field.Data != Auction)
								{
									goto Skip;
								}
							}
							else if (field.Layout.Name == "ForSale")
							{
								if ((bool)field.Data != ForSale)
								{
									goto Skip;
								}
							}
							else if (field.Layout.Name == "ParcelID")
							{
								parcel.ID = (LLUUID)field.Data;
							}
							else if (field.Layout.Name == "Name")
							{
								parcel.Name = System.Text.Encoding.UTF8.GetString((byte[])field.Data);
							}
							else if (field.Layout.Name == "ActualArea")
							{
								parcel.ActualArea = (int)field.Data;
							}
							else if (field.Layout.Name == "SalePrice")
							{
								parcel.SalePrice = (int)field.Data;
							}
						}

						if (parcel.ID != null)
						{
							ParcelsForSale.Add(parcel);
						}
						else
						{
							Helpers.Log("Parcel with no ID found in DirLandReply, skipping", Helpers.LogLevel.Warning);
						}
					}

				Skip:
					;
				}
			}
			else
			{
				Console.WriteLine("Received a DirLandReply after the timeout!");
			}
		}

		private void TimerEvent(object source, System.Timers.ElapsedEventArgs ea)
		{
			Timer.Stop();
			Timeout = true;
			DirLandTimeout = true;
		}
	}
}
