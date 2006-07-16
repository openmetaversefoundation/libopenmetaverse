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

namespace libsecondlife
{
	/// <summary>
	/// Represents a region (also known as a sim) in Second Life.
	/// </summary>
	public class Region
	{
		public LLUUID ID;
		public U64 Handle;
		public string Name;
		public byte[] ParcelOverlay;

		public float TerrainHeightRange00;
		public float TerrainHeightRange01;
		public float TerrainHeightRange10;
		public float TerrainHeightRange11;
		public float TerrainStartHeight00;
		public float TerrainStartHeight01;
		public float TerrainStartHeight10;
		public float TerrainStartHeight11;
		public float WaterHeight;

		public LLUUID SimOwner;

		public LLUUID TerrainBase0;
		public LLUUID TerrainBase1;
		public LLUUID TerrainBase2;
		public LLUUID TerrainBase3;
		public LLUUID TerrainDetail0;
		public LLUUID TerrainDetail1;
		public LLUUID TerrainDetail2;
		public LLUUID TerrainDetail3;

		public bool IsEstateManager;
		
		private SecondLife Client;

		public Region(SecondLife client, LLUUID id, U64 handle, string name, float[] heightList, 
			LLUUID simOwner, LLUUID[] terrainImages, bool isEstateManager)
		{
			Client = client;
			ID = id;
			Handle = handle;
			Handle = new U64();
			Name = name;
			ParcelOverlay = new byte[4096];

			TerrainHeightRange00 = heightList[0];
			TerrainHeightRange01 = heightList[1];
			TerrainHeightRange10 = heightList[2];
			TerrainHeightRange11 = heightList[3];
			TerrainStartHeight00 = heightList[4];
			TerrainStartHeight01 = heightList[5];
			TerrainStartHeight10 = heightList[6];
			TerrainStartHeight11 = heightList[7];
			WaterHeight = heightList[8];

			SimOwner = simOwner;

			TerrainBase0 = terrainImages[0];
			TerrainBase1 = terrainImages[1];
			TerrainBase2 = terrainImages[2];
			TerrainBase3 = terrainImages[3];
			TerrainDetail0 = terrainImages[4];
			TerrainDetail1 = terrainImages[5];
			TerrainDetail2 = terrainImages[6];
			TerrainDetail3 = terrainImages[7];

			IsEstateManager = isEstateManager;
		}
		
		public void RezObject(PrimObject prim, LLVector3 position, LLVector3 avatarPosition)
		{
			byte[] textureEntry = new byte[40];
			Array.Copy(prim.Texture.Data, textureEntry, 16);
			textureEntry[35] = 0xe0; // No clue
			
			Packet objectAdd = libsecondlife.Packets.Object.ObjectAdd(Client.Protocol, Client.Network.AgentID,
			                                                          LLUUID.GenerateUUID(), avatarPosition,
			                                                          position, prim, textureEntry);
			Client.Network.SendPacket(objectAdd);
		}

		public override int GetHashCode()
		{
			return ID.GetHashCode();
		}

		public override bool Equals(object o)
		{
			if (!(o is Region))
			{
				return false;
			}

			Region region = (Region)o;

			return (region.ID == ID);
		}

		public static bool operator==(Region lhs, Region rhs)
		{
			try
			{
				return (lhs.ID == rhs.ID);
			}
			catch (NullReferenceException)
			{
				byte test;
				bool lhsnull = false;
				bool rhsnull = false;

				try
				{
					test = lhs.ID.Data[0];
				}
				catch (NullReferenceException)
				{
					lhsnull = true;
				}

				try
				{
					test = rhs.ID.Data[0];
				}
				catch (NullReferenceException)
				{
					rhsnull = true;
				}
				
				return (lhsnull == rhsnull);
			}
		}

		public static bool operator!=(Region lhs, Region rhs)
		{
			return !(lhs == rhs);
		}
	}
}
