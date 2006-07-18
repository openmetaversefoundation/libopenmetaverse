using System;
using System.Collections;

using libsecondlife;
//using libsecondlife.InventorySystem;

namespace libsecondlife.InventorySystem.PacketHelpers
{
	/// <summary>
	/*
		Low 00321 - UpdateInventoryItem - Untrusted - Unencoded
			0065 InventoryData (Variable)
				0047 GroupOwned (BOOL / 1)
				0149 CRC (U32 / 1)
				0159 CreationDate (S32 / 1)
				0345 SaleType (U8 / 1)
				0395 BaseMask (U32 / 1)
				0506 Name (Variable / 1)
				0562 InvType (S8 / 1)
				0630 Type (S8 / 1)
				0680 AssetID (LLUUID / 1)
				0699 GroupID (LLUUID / 1)
				0716 SalePrice (S32 / 1)
				0719 OwnerID (LLUUID / 1)
				0736 CreatorID (LLUUID / 1)
				0968 ItemID (LLUUID / 1)
				1025 FolderID (LLUUID / 1)
				1084 EveryoneMask (U32 / 1)
				1101 Description (Variable / 1)
				1189 Flags (U32 / 1)
				1348 NextOwnerMask (U32 / 1)
				1452 GroupMask (U32 / 1)
				1505 OwnerMask (U32 / 1)
			1297 AgentData (01)
				0219 AgentID (LLUUID / 1)
				
	
			----- UpdateInventoryItem -----
			InventoryData
				GroupOwned: False
				CRC: 3330379543
				CreationDate: 1152566548
				SaleType: 0
				BaseMask: 2147483647
				Name: New Note
				InvType: 7
				Type: 7
				AssetID: 00000000000000000000000000000000
				GroupID: 00000000000000000000000000000000
				SalePrice: 10
				OwnerID: 25472683cb324516904a6cd0ecabf128
				CreatorID: 25472683cb324516904a6cd0ecabf128
				ItemID: 6f11a788c6478fb50610b65b4a8f9c11
				FolderID: a4947fc066c247518d9854aaf90097f4
				EveryoneMask: 0
				Description: 2006-07-10 14:22:38 note card
				Flags: 0
				NextOwnerMask: 2147483647
				GroupMask: 0
				OwnerMask: 2147483647
			AgentData
				AgentID: 25472683cb324516904a6cd0ecabf128
	
	*/
	/// </summary>
	public class UpdateInventoryItem
	{
		public static Packet BuildPacket(ProtocolManager protocol, InventoryItem iitem, LLUUID agentID)
		{
			return BuildPacket( protocol
				, iitem.GroupOwned
				, BuildCRC(iitem)
				, iitem.CreationDate
				, iitem.SaleType
				, iitem.BaseMask
				, iitem.Name
				, iitem.InvType
				, iitem.Type
				, iitem.AssetID
				, iitem.GroupID
				, iitem.SalePrice
				, iitem.OwnerID
				, iitem.CreatorID
				, iitem.ItemID
				, iitem.FolderID
				, iitem.EveryoneMask
				, iitem.Description
				, iitem.Flags
				, iitem.NextOwnerMask
				, iitem.GroupMask
				, iitem.OwnerMask
				, agentID

				);
		}

		private static Packet BuildPacket(ProtocolManager protocol
			, bool groupOwned
			, uint crc
			, int creationDate
			, byte saleType
			, uint baseMask
			, string name
			, sbyte invType, sbyte type
			, LLUUID assetID
			, LLUUID groupID
			, int salePrice
			, LLUUID ownerID
			, LLUUID creatorID
			, LLUUID itemID
			, LLUUID folderID
			, uint everyoneMask
			, string description
			, uint flags
			, uint nextOwnerMask
			, uint groupMask
			, uint ownerMask
			, LLUUID agentID
			)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["GroupOwned"]	= groupOwned;
			fields["CRC"]			= crc;
			fields["CreationDate"]	= creationDate;
			fields["SaleType"]		= saleType;
			fields["BaseMask"]		= baseMask;
			fields["Name"]			= name;
			fields["InvType"]		= invType;
			fields["Type"]			= type;
			fields["AssetID"]		= assetID;
			fields["GroupID"]		= groupID;
			fields["SalePrice"]		= salePrice;
			fields["OwnerID"]		= ownerID;
			fields["CreatorID"]		= creatorID;
			fields["ItemID"]		= itemID;
			fields["FolderID"]		= folderID;
			fields["EveryoneMask"]	= everyoneMask;
			fields["Description"]	= description;
			fields["Flags"]			= flags;
			fields["NextOwnerMask"]	= nextOwnerMask;
			fields["GroupMask"]		= groupMask;
			fields["OwnerMask"]		= ownerMask;
			blocks[fields]			= "InventoryData";

			fields = new Hashtable();
			fields["AgentID"] = agentID;
			blocks[fields] = "AgentData";

			return PacketBuilder.BuildPacket("UpdateInventoryItem", protocol, blocks, Helpers.MSG_RELIABLE);
		}


		public static uint BuildCRC(InventoryItem iitem)
		{
			return BuildCRC( 
				iitem.CreationDate
				, iitem.SaleType
				, iitem.InvType
				, iitem.Type
				, iitem.AssetID
				, iitem.GroupID
				, iitem.SalePrice
				, iitem.OwnerID
				, iitem.CreatorID
				, iitem.ItemID
				, iitem.FolderID
				, iitem.EveryoneMask
				, iitem.Flags
				, iitem.NextOwnerMask
				, iitem.GroupMask
				, iitem.OwnerMask
				);
		}

		private static uint BuildCRC ( 
			int creationDate
			, byte saleType
			, sbyte invType, sbyte type
			, LLUUID assetID
			, LLUUID groupID
			, int salePrice
			, LLUUID ownerID
			, LLUUID creatorID
			, LLUUID itemID
			, LLUUID folderID
			, uint everyoneMask
			, uint flags
			, uint nextOwnerMask
			, uint groupMask
			, uint ownerMask
			)
		{

			uint CRC = 0;

			/* IDs */
			CRC += assetID.CRC(); // AssetID
			CRC += folderID.CRC(); // FolderID
			CRC += itemID.CRC(); // ItemID

			/* Permission stuff */
			CRC += creatorID.CRC(); // CreatorID
			CRC += ownerID.CRC(); // OwnerID
			CRC += groupID.CRC(); // GroupID

			/* CRC += another 4 words which always seem to be zero -- unclear if this is a LLUUID or what */
			CRC += ownerMask; //owner_mask;      // Either owner_mask or next_owner_mask may need to be
			CRC += nextOwnerMask; //next_owner_mask; // switched with base_mask -- 2 values go here and in my
			CRC += everyoneMask; //everyone_mask;   // study item, the three were identical.
			CRC += groupMask; //group_mask;

			/* The rest of the CRC fields */
			CRC += flags; // Flags
			CRC += (uint)invType; // InvType
			CRC += (uint)type; // Type 
			CRC += (uint)creationDate; // CreationDate
			CRC += (uint)salePrice;    // SalePrice
			CRC += (uint)((uint)saleType * 0x07073096); // SaleType

			return CRC;
		}

	}
}
