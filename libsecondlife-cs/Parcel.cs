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
		public U64 RegionHandle;
		public string Name;
		public string SimName;
		public string Desc;
		public int SalePrice;
		public int ActualArea;
		public LLVector3 GlobalPosition;
		public LLVector3 SimPosition;
		public float Dwell;

		public Parcel()
		{
			GlobalPosition = new LLVector3();
			SimPosition = new LLVector3();
		}

		public bool Buy(SecondLife client, bool forGroup, LLUUID groupID)
		{
			//SIDENOTE: Teleport should not finish until we update the current region name!

			// Sanity check to make sure we're in the same sim

			// Attempt to buy the parcel

			// Check if the purchase was successful (look for money packet?)
			return false;
		}
	}

	public class ParcelManager
	{
		public ArrayList ParcelsForSale;

		private SecondLife Client;
		private bool ReservedNewbie;
		private bool ForSale;
		private bool Auction;
		private bool Finished;
		private Timer DirLandTimer;
		private bool DirLandTimeout;
		private bool ParcelInfoTimeout;
		private Parcel ParcelInfoParcel;

		public ParcelManager(SecondLife client)
		{
			Client = client;
			ParcelsForSale = new ArrayList();

			// Setup the callbacks
			PacketCallback callback = new PacketCallback(DirLandReplyHandler);
			Client.Network.RegisterCallback("DirLandReply", callback);
			callback = new PacketCallback(ParcelInfoReplyHandler);
			Client.Network.RegisterCallback("ParcelInfoReply", callback);
		}

		public bool RequestParcelInfo(Parcel parcel)
		{
			int attempts = 0;

		Beginning:
			if (attempts++ > 3)
			{
				return false;
			}

			Finished = false;
			ParcelInfoTimeout = false;
			ParcelInfoParcel = parcel;

			// Setup the timer
			Timer ParcelInfoTimer = new Timer(5000);
			ParcelInfoTimer.Elapsed += new ElapsedEventHandler(ParcelInfoTimerEvent);
			ParcelInfoTimeout = false;

			// Build the ParcelInfoRequest packet
			Packet parcelInfoPacket = Packets.Parcel.ParcelInfoRequest(Client.Protocol, parcel.ID, 
				Client.Network.AgentID, Client.Network.SessionID);

			// Start the timer
			ParcelInfoTimer.Start();

			Client.Network.SendPacket(parcelInfoPacket);

			while (!Finished)
			{
				if (ParcelInfoTimeout)
				{
					goto Beginning;
				}

				Client.Tick();
			}

			return true;
		}

		private void ParcelInfoReplyHandler(Packet packet, Circuit circuit)
		{
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

			try
			{
				foreach (Block block in packet.Blocks())
				{
					foreach (Field field in block.Fields)
					{
						if (field.Layout.Name == "ParcelID")
						{
							parcelID = (LLUUID)field.Data;

							if (!parcelID.Equals(ParcelInfoParcel.ID))
							{
								Helpers.Log("Received a ParcelInfoReply for " + parcelID.ToString() + 
									", looking for " + ParcelInfoParcel.ID.ToString(), Helpers.LogLevel.Warning);
								
								// Build and resend the ParcelInfoRequest packet
								Packet parcelInfoPacket = Packets.Parcel.ParcelInfoRequest(Client.Protocol, ParcelInfoParcel.ID, 
									Client.Network.AgentID, Client.Network.SessionID);

								Client.Network.SendPacket(parcelInfoPacket);

								return;
							}
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
							name = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
						}
						else if (field.Layout.Name == "SimName") 
						{
							simName = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
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
							desc = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
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

				ParcelInfoParcel.SimName = simName;
				ParcelInfoParcel.ActualArea = actualArea;
				ParcelInfoParcel.GlobalPosition.X = globalX;
				ParcelInfoParcel.GlobalPosition.Y = globalY;
				ParcelInfoParcel.GlobalPosition.Z = globalZ;
				ParcelInfoParcel.Name = name;
				ParcelInfoParcel.Desc = desc;
				ParcelInfoParcel.SalePrice = salePrice;
				ParcelInfoParcel.OwnerID = ownerID;
				ParcelInfoParcel.SnapshotID = snapshotID;
				ParcelInfoParcel.Dwell = dwell;

				// Get RegionHandle from GlobalX/GlobalY
				uint handleX = (uint)Math.Floor(ParcelInfoParcel.GlobalPosition.X / 256.0F);
				handleX *= 256;
				uint handleY = (uint)Math.Floor(ParcelInfoParcel.GlobalPosition.Y / 256.0F);
				handleY *= 256;
				ParcelInfoParcel.RegionHandle = new U64(handleX, handleY);

				// Get SimPosition from GlobalX/GlobalY and RegionHandle
				ParcelInfoParcel.SimPosition.X = ParcelInfoParcel.GlobalPosition.X - (float)handleX;
				ParcelInfoParcel.SimPosition.Y = ParcelInfoParcel.GlobalPosition.Y - (float)handleY;
				ParcelInfoParcel.SimPosition.Z = ParcelInfoParcel.GlobalPosition.Z;
			}
			catch (Exception e)
			{
				Helpers.Log(e.ToString(), Helpers.LogLevel.Error);
			}

			Finished = true;
		}

		private void ParcelInfoTimerEvent(object source, System.Timers.ElapsedEventArgs ea)
		{
			ParcelInfoTimeout = true;
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
			DirLandTimer = new Timer(15000);
			DirLandTimer.Elapsed += new ElapsedEventHandler(DirLandTimerEvent);
			DirLandTimeout = false;
			DirLandTimer.Start();

			LLUUID queryID = new LLUUID();
			Packet landQuery = Packets.Sim.DirLandQuery(Client.Protocol, ReservedNewbie, ForSale, queryID, 
				Auction, 0, Client.Network.AgentID, Client.Network.SessionID);
			Client.Network.SendPacket(landQuery);

			while (!DirLandTimeout)
			{
				Client.Tick();
			}

			// Double check the timer is actually stopped
			DirLandTimer.Stop();

			return ParcelsForSale.Count;
		}

		private void DirLandReplyHandler(Packet packet, Circuit circuit)
		{
			if (!DirLandTimeout)
			{
				// Reset the timer
				DirLandTimer.Stop();
				DirLandTimer.Start();

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
								parcel.Name = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
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

		private void DirLandTimerEvent(object source, System.Timers.ElapsedEventArgs ea)
		{
			DirLandTimer.Stop();
			DirLandTimeout = true;
		}
	}
}
