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

namespace libsecondlife
{
	/// <summary>
	/// Class for regions on the world map
	/// </summary>
	public class GridRegion
	{
		public int X;
		public int Y;
		public string Name;
		public byte Access;
		public uint RegionFlags;
		public byte WaterHeight;
		public byte Agents;
		public LLUUID MapImageID;
		public U64 RegionHandle; // Used for teleporting

		public GridRegion() 
		{

		}
	}

	/// <summary>
	/// Manages grid-wide tasks such as the world map
	/// </summary>
	public class GridManager
	{
		public Hashtable Regions;
		SecondLife Client;

		public GridManager(SecondLife client)
		{
			Client = client;
			Regions = new Hashtable();
			PacketCallback callback = new PacketCallback(MapBlockReplyHandler);
			Client.Network.RegisterCallback("MapBlockReply", callback);
		}

		public void AddSim(string name) 
		{
			if(!Regions.ContainsKey(name)) 
			{
				Client.Network.SendPacket(Packets.Sim.MapNameRequest(Client.Protocol,Client.Avatar.ID,0,0,false,name));
//				Client.Network.SendPacket(Packets.Sim.MapNameRequest(Client.Protocol,Client.Avatar.ID,2,0,false,name));
//				Client.Network.SendPacket(Packets.Sim.MapNameRequest(Client.Protocol,Client.Avatar.ID,512,0,false,name));
			}
		}

		public void AddAllSims() 
		{
//			uint flags = 2;
//			Client.Network.SendPacket(Packets.Sim.MapBlockRequest(Client.Protocol,Client.Avatar.ID,flags,0,false,0,65535,0,65535));
			uint flags = 0;
			Client.Network.SendPacket(Packets.Sim.MapBlockRequest(Client.Protocol,Client.Avatar.ID,flags,0,false,0,65535,0,65535));
		}

		public GridRegion GetSim(string name) 
		{
			if(Regions.ContainsKey(name)) 
				return (GridRegion)Regions[name];

			AddSim(name);
			System.Threading.Thread.Sleep(1000);

			if(Regions.ContainsKey(name)) 
				return (GridRegion)Regions[name];
			else 
			{
				/* TODO: Put some better handling inplace here with some retry code */
				Client.Log("Error returned sim that didnt exist",Helpers.LogLevel.Warning);
				return new GridRegion();
			}
		}

		private void MapBlockReplyHandler(Packet packet, Simulator simulator) 
		{
			GridRegion region;

			foreach (Block block in packet.Blocks())
			{
				if(block.Layout.Name == "Data") 
				{
					region = new GridRegion();
					foreach (Field field in block.Fields)
					{
						if (field.Layout.Name == "X") 
						{
							if(System.BitConverter.IsLittleEndian) 
							{
								ushort temp = (ushort)field.Data;
								region.X = ((temp << 8) & 0xFF00) | ((temp >> 8) & 0x00FF);
							} 
							else 
							{
								region.X = (ushort)field.Data;
							}
						}
						else if(field.Layout.Name == "Y")
						{
							if(System.BitConverter.IsLittleEndian) 
							{
								ushort temp = (ushort)field.Data;
								region.Y = ((temp << 8) & 0xFF00) | ((temp >> 8) & 0x00FF);
							} 
							else 
							{
								region.Y = (ushort)field.Data;
							}
						}
						else if(field.Layout.Name == "Name")
							region.Name = Helpers.FieldToString(field.Data);
						else if(field.Layout.Name == "RegionFlags")
							region.RegionFlags = (uint)field.Data;
						else if(field.Layout.Name == "WaterHeight")
							region.WaterHeight = (byte)field.Data;
						else if(field.Layout.Name == "Agents")
							region.Agents = (byte)field.Data;
						else if(field.Layout.Name == "MapImageID")
							region.MapImageID = (LLUUID)field.Data;
					}

					region.RegionHandle = new U64(region.X * 256,region.Y * 256);

					if(region.Name != "" && (region.X != 0 && region.Y != 0))
						Regions[region.Name] = region;
				}
			}
		}
	}
}
