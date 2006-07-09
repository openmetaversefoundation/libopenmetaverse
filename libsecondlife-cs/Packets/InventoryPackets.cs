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

namespace libsecondlife.Packets
{
	public class Inventory
	{
		public static Packet FetchInventoryDescendents(ProtocolManager protocol, LLUUID ownerID, 
			LLUUID folderID, LLUUID agentID)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["OwnerID"] = ownerID;
			fields["FolderID"] = folderID;
			fields["SortOrder"] = (int)1; // TODO: Any ideas on valid values for this field?
			fields["FetchFolders"] = true;
			fields["FetchItems"] = true;
			blocks[fields] = "InventoryData";

			fields = new Hashtable();
			fields["AgentID"] = agentID;
			blocks[fields] = "AgentData";

			return PacketBuilder.BuildPacket("FetchInventoryDescendents", protocol, blocks, Helpers.MSG_RELIABLE);
		}

		public static Packet RequestInventoryAsset(ProtocolManager protocol, LLUUID agentID, 
			LLUUID queryID, LLUUID ownerID, LLUUID itemID)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["AgentID"] = agentID;
			fields["QueryID"] = queryID;
			fields["OwnerID"] = ownerID;
			fields["ItemID"] = itemID;
			blocks[fields] = "QueryData";

			return PacketBuilder.BuildPacket("RequestInventoryAsset", protocol, blocks, Helpers.MSG_RELIABLE + Helpers.MSG_ZEROCODED);
		}

		public static Packet UpdateInventoryItem(ProtocolManager protocol, string name, string description,
			sbyte inventoryType, sbyte type, LLUUID assetID, LLUUID groupID, LLUUID creatorID, LLUUID ownerID, 
			LLUUID folderID, LLUUID itemID, int salePrice, byte saleType, uint baseMask, uint everyoneMask, 
			uint nextOwnerMask, uint groupMask, uint ownerMask, bool groupOwned, uint flags)
		{
			TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
			int now = (int)t.TotalSeconds;

			uint CRC = 0;

			// IDs
			CRC += assetID.CRC();
			CRC += folderID.CRC();
			CRC += itemID.CRC();

			// Permission stuff
			CRC += creatorID.CRC();
			CRC += ownerID.CRC();
			CRC += groupID.CRC();

			// CRC += another 4 words which always seem to be zero -- unclear if this is a LLUUID or what
			CRC += ownerMask;     // Either owner_mask or next_owner_mask may need to be
			CRC += nextOwnerMask; // switched with base_mask -- 2 values go here and in my
			CRC += everyoneMask;  // study item, the three were identical.
			CRC += groupMask;

			// The rest of the CRC fields
			CRC += flags;
			CRC += (uint)inventoryType;
			CRC += (uint)type;
			CRC += (uint)now;
			CRC += (uint)salePrice;
			CRC += (uint)((uint)saleType * 0x07073096);

			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["GroupOwned"] = groupOwned;
			fields["CRC"] = CRC;
			fields["CreationDate"] = now;
			fields["SaleType"] = saleType;
			fields["BaseMask"] = baseMask;
			fields["Name"] = name;
			fields["InvType"] = inventoryType;
			fields["Type"] = type;
			fields["AssetID"] = assetID;
			fields["GroupID"] = groupID;
			fields["SalePrice"] = salePrice;
			fields["OwnerID"] = ownerID;
			fields["CreatorID"] = creatorID;
			fields["ItemID"] = itemID;
			fields["FolderID"] = folderID;
			fields["EveryoneMask"] = everyoneMask;
			fields["Description"] = description;
			fields["Flags"] = flags;
			fields["NextOwnerMask"] = nextOwnerMask;
			fields["GroupMask"] = groupMask;
			fields["OwnerMask"] = ownerMask;

			return null;
		}
	}
}
