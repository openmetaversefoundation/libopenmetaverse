using System;
using System.Collections;

using libsecondlife;
using libsecondlife.InventorySystem;

namespace libsecondlife.Packets
{
	/// <summary>
	/// Summary description for Other.
	/// </summary>
	public class InventoryPackets
	{
		private InventoryPackets()
		{
			//Prevents this class from being instantiated.
		}



		public const int FETCH_INVENTORY_SORT_NAME = 0;
		public const int FETCH_INVENTORY_SORT_TIME = 1;

		public static Packet FetchInventoryDescendents(ProtocolManager protocol, LLUUID ownerID, 
			LLUUID folderID, LLUUID agentID)
		{
			return FetchInventoryDescendents(protocol, ownerID, folderID, agentID, true, true);
		}

		public static Packet FetchInventoryDescendents(ProtocolManager protocol, LLUUID ownerID, 
			LLUUID folderID, LLUUID agentID, bool fetchFolders, bool fetchItems)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["OwnerID"] = ownerID;
			fields["FolderID"] = folderID;
			fields["SortOrder"] = FETCH_INVENTORY_SORT_NAME;
			fields["FetchFolders"] = fetchFolders;
			fields["FetchItems"] = fetchItems;
			blocks[fields] = "InventoryData";

			fields = new Hashtable();
			fields["AgentID"] = agentID;
			blocks[fields] = "AgentData";

			return PacketBuilder.BuildPacket("FetchInventoryDescendents", protocol, blocks, Helpers.MSG_RELIABLE);
		}

		/*
			Low 00334 - FetchInventory - Untrusted - Unencoded
				0065 InventoryData (Variable)
					0719 OwnerID (LLUUID / 1)
					0968 ItemID (LLUUID / 1)
				1297 AgentData (01)
					0219 AgentID (LLUUID / 1)
			*/
		public static Packet FetchInventory(ProtocolManager protocol, LLUUID ownerID, LLUUID itemID, LLUUID agentID)
		{
			int packetLength = 8; // header
			packetLength += 16; // OwnerID (UUID)
			packetLength += 16; // ItemID (UUID)
			packetLength += 16; // AgentID (UUID)

			Packet packet = new Packet("AgentWearablesRequest", protocol, packetLength);

			int pos = 8; // Leave room for header

			// OwnerID
			Array.Copy(ownerID.Data, 0, packet.Data, pos, 16);
			pos += 16;

			// ItemID
			Array.Copy(itemID.Data, 0, packet.Data, pos, 16);
			pos += 16;

			// AgentID
			Array.Copy(agentID.Data, 0, packet.Data, pos, 16);
			pos += 16;

			// Set the packet flags
			//			packet.Data[0] = Helpers.MSG_ZEROCODED + Helpers.MSG_RELIABLE;
			//			packet.Data[0] = Helpers.MSG_RELIABLE;

			return packet;
		}

		public static Packet AgentWearablesRequest(ProtocolManager protocol, LLUUID agentID)
		{
			int packetLength = 8; // header
			packetLength += 16; // AgentID (UUID)

			Packet packet = new Packet("AgentWearablesRequest", protocol, packetLength );

			int pos = 8; // Leave room for header

			// AgentID
			Array.Copy(agentID.Data, 0, packet.Data, pos, 16);
			pos += 16;

			// Set the packet flags
			packet.Data[0] = Helpers.MSG_ZEROCODED + Helpers.MSG_RELIABLE;

			return packet;
		}

		/*
			Low 00328 - CreateInventoryFolder - Untrusted - Zerocoded
				1297 AgentData (01)
					0219 AgentID (LLUUID / 1)
				1298 FolderData (01)
					0506 Name (Variable / 1)
					0558 ParentID (LLUUID / 1)
					0630 Type (S8 / 1)
					1025 FolderID (LLUUID / 1)

			----- CreateInventoryFolder -----
			AgentData
				AgentID: 25472683cb324516904a6cd0ecabf128
			FolderData
				Name: New Folder
				ParentID: a4947fc066c247518d9854aaf90097f4
				Type: 255
				FolderID: fdc8b4cc8ff9d678a8e15aa6ea700271
		*/
		public static Packet CreateInventoryFolder(ProtocolManager protocol
			, LLUUID agentID
			, string name
			, LLUUID parentID
			, sbyte  type
			, LLUUID folderID
			)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["AgentID"] = agentID;
			blocks[fields] = "AgentData";

			fields = new Hashtable();
			fields["Name"]		= name;
			fields["ParentID"]	= parentID;
			fields["Type"]		= type;
			fields["FolderID"]	= folderID;
			blocks[fields]		= "FolderData";


			return PacketBuilder.BuildPacket("CreateInventoryFolder", protocol, blocks, Helpers.MSG_RELIABLE | Helpers.MSG_ZEROCODED);
		}



		/*
			----- MoveInventoryFolder -----
			InventoryData
				ParentID: 4d68743474c3084812d3a3fdda2ca2bd
				FolderID: 8c8412df3064dc40ad676826b03b87d7
			AgentData
				AgentID: 25472683cb324516904a6cd0ecabf128
				Stamp: True

			Low 00330 - MoveInventoryFolder - Untrusted - Unencoded
				0065 InventoryData (Variable)
					0558 ParentID (LLUUID / 1)
					1025 FolderID (LLUUID / 1)
				1297 AgentData (01)
					0219 AgentID (LLUUID / 1)
					1252 Stamp (BOOL / 1)
		*/
		public static Packet MoveInventoryFolder(ProtocolManager protocol
			, LLUUID agentID
			, LLUUID parentID
			, LLUUID folderID
			)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields;

			fields = new Hashtable();
			fields["AgentID"]	= agentID;
			fields["Stamp"]		= true;
			blocks[fields]		= "AgentData";

			fields = new Hashtable();
			fields["ParentID"]	= parentID;
			fields["FolderID"]	= folderID;
			blocks[fields]		= "InventoryData";


			return PacketBuilder.BuildPacket("MoveInventoryFolder", protocol, blocks, Helpers.MSG_RELIABLE | Helpers.MSG_ZEROCODED);
		}

		
		/*
			----- RemoveInventoryFolder -----
			AgentData
				AgentID: 25472683cb324516904a6cd0ecabf128
			FolderData
				FolderID: 8c8412df3064dc40ad676826b03b87d7

			Low 00331 - RemoveInventoryFolder - Untrusted - Zerocoded
				1297 AgentData (01)
					0219 AgentID (LLUUID / 1)
				1298 FolderData (Variable)
					1025 FolderID (LLUUID / 1)
		*/
		public static Packet RemoveInventoryFolder(ProtocolManager protocol
			, LLUUID agentID
			, LLUUID folderID
			)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["AgentID"] = agentID;
			blocks[fields] = "AgentData";

			fields = new Hashtable();
			fields["FolderID"]	= folderID;
			blocks[fields]		= "FolderData";


			return PacketBuilder.BuildPacket("RemoveInventoryFolder", protocol, blocks, Helpers.MSG_RELIABLE | Helpers.MSG_ZEROCODED);
		}


		/*
			----- UpdateInventoryFolder -----
			AgentData
				AgentID: 25472683cb324516904a6cd0ecabf128
			FolderData
				Name: Renamed
				ParentID: a4947fc066c247518d9854aaf90097f4
				Type: 255
				FolderID: 10dce442915c01581a931170664d0616
				
			Low 00329 - UpdateInventoryFolder - Untrusted - Zerocoded
				1297 AgentData (01)
					0219 AgentID (LLUUID / 1)
				1298 FolderData (Variable)
					0506 Name (Variable / 1)
					0558 ParentID (LLUUID / 1)
					0630 Type (S8 / 1)
					1025 FolderID (LLUUID / 1)		
		 */
		public static Packet UpdateInventoryFolder(ProtocolManager protocol
			, LLUUID agentID
			, string name
			, LLUUID parentID
			, sbyte  type
			, LLUUID folderID
			)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["AgentID"] = agentID;
			blocks[fields] = "AgentData";

			fields = new Hashtable();
			fields["Name"]		= name;
			fields["ParentID"]	= parentID;
			fields["Type"]		= type;
			fields["FolderID"]	= folderID;
			blocks[fields]		= "FolderData";


			return PacketBuilder.BuildPacket("UpdateInventoryFolder", protocol, blocks, Helpers.MSG_RELIABLE | Helpers.MSG_ZEROCODED);
		}

		/*
		Low 00323 - MoveInventoryItem - Untrusted - Unencoded
			0065 InventoryData (Variable)
				0968 ItemID (LLUUID / 1)
				1025 FolderID (LLUUID / 1)
			1297 AgentData (01)
				0219 AgentID (LLUUID / 1)
				1252 Stamp (BOOL / 1)
		*/
		public static Packet MoveInventoryItem(ProtocolManager protocol
			, LLUUID agentID
			, LLUUID itemID
			, LLUUID folderID
			)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields;

			fields = new Hashtable();
			fields["ItemID"]	= itemID;
			fields["FolderID"]	= folderID;
			blocks[fields]		= "InventoryData";

			fields = new Hashtable();
			fields["AgentID"]	= agentID;
			fields["Stamp"]		= true;
			blocks[fields]		= "AgentData";

			return PacketBuilder.BuildPacket("MoveInventoryItem", protocol, blocks, Helpers.MSG_RELIABLE);
		}

		/*
		Low 00324 - CopyInventoryItem - Untrusted - Unencoded
			0065 InventoryData (Variable)
				0224 NewFolderID (LLUUID / 1)
				0991 OldItemID (LLUUID / 1)
			1297 AgentData (01)
				0219 AgentID (LLUUID / 1)
		*/
		public static Packet CopyInventoryItem(ProtocolManager protocol
			, LLUUID agentID
			, LLUUID itemID
			, LLUUID folderID
			)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields;

			fields = new Hashtable();
			fields["ItemID"]	= itemID;
			fields["FolderID"]	= folderID;
			blocks[fields]		= "InventoryData";

			fields = new Hashtable();
			fields["AgentID"]	= agentID;
			blocks[fields]		= "AgentData";

			return PacketBuilder.BuildPacket("CopyInventoryItem", protocol, blocks, Helpers.MSG_RELIABLE);
		}

		/*
			Low 00325 - RemoveInventoryItem - Untrusted - Zerocoded
				0065 InventoryData (Variable)
					0968 ItemID (LLUUID / 1)
				1297 AgentData (01)
					0219 AgentID (LLUUID / 1)
		*/
		public static Packet RemoveInventoryItem(ProtocolManager protocol
			, LLUUID agentID
			, LLUUID itemID
			)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["ItemID"]	= itemID;
			blocks[fields]		= "InventoryData";

			fields = new Hashtable();
			fields["AgentID"] = agentID;
			blocks[fields] = "AgentData";



			return PacketBuilder.BuildPacket("RemoveInventoryItem", protocol, blocks, Helpers.MSG_RELIABLE | Helpers.MSG_ZEROCODED);
		}

		/*
			----- ImprovedInstantMessage -----
			MessageBlock
				ID: 8006f744d08bbad1f941d59ffce4059e
				ToAgentID: f6ec1e24fd294f4cb21e23b42841c8c7
				Offline: 0
				Timestamp: 0
				Message: Big Card 2:04 PM
				RegionID: 00000000000000000000000000000000
				FromAgentID: 25472683cb324516904a6cd0ecabf128
				Dialog: 4
				BinaryBucket: 07 9a 31 a7 1a 05 ff 76 4d af 8f ef a0 b3 e7 08 ..1....vM.......
				BinaryBucket: e6                                              .
				ParentEstateID: 0
				FromAgentName: Bot Ringo
				Position: 25.528299, 214.016006, 1.088448
				
			Low 00304 - ImprovedInstantMessage - Untrusted - Unencoded
				1231 MessageBlock (01)
					0030 ID (LLUUID / 1)
					0172 ToAgentID (LLUUID / 1)
					0248 Offline (U8 / 1)
					0369 Timestamp (U32 / 1)
					0389 Message (Variable / 2)
					0488 RegionID (LLUUID / 1)
					0597 FromAgentID (LLUUID / 1)
					0889 Dialog (U8 / 1)
					1124 BinaryBucket (Variable / 2)
					1129 ParentEstateID (U32 / 1)
					1150 FromAgentName (Variable / 1)
					1389 Position (LLVector3 / 1)
		
		 */
		public static Packet ImprovedInstantMessage(ProtocolManager protocol
			, LLUUID ID
			, LLUUID ToAgentID
			, LLUUID FromAgentID
			, String FromAgentName
			, LLVector3 FromAgentLoc
			, InventoryItem Item
			)
		{
			byte[] BinaryBucket = new byte[17];
			BinaryBucket[0] = (byte)Item.Type;
			Array.Copy(Item.ItemID.Data, 0, BinaryBucket, 1, 16);


			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["ID"]			= ID;
			fields["ToAgentID"]		= ToAgentID;
			fields["Offline"]		= (byte)0;
			fields["TimeStamp"]		= (uint)0;
			fields["Message"]		= Item.Name;
			fields["RegionID"]		= new LLUUID();
			fields["FromAgentID"]	= FromAgentID;
			fields["Dialog"]		= (byte)4;
			fields["BinaryBucket"]	= BinaryBucket;
			fields["ParentEstateID"]= (uint)0;
			fields["FromAgentName"]	= FromAgentName;
			fields["Position"]		= FromAgentLoc;
			blocks[fields]			= "MessageBlock";

			return PacketBuilder.BuildPacket("ImprovedInstantMessage", protocol, blocks, Helpers.MSG_RELIABLE );
		}



		/*
		Low 00337 - RequestInventoryAsset - Trusted - Zerocoded
			1266 QueryData (01)
				0219 AgentID (LLUUID / 1)
				0640 QueryID (LLUUID / 1)
				0719 OwnerID (LLUUID / 1)
				0968 ItemID (LLUUID / 1)
		Low 00338 - InventoryAssetResponse - Trusted - Zerocoded
			1266 QueryData (01)
				0640 QueryID (LLUUID / 1)
				0680 AssetID (LLUUID / 1)
				1058 IsReadable (BOOL / 1)
		*/
		public static Packet RequestInventoryAsset(ProtocolManager protocol
			, LLUUID agentID, LLUUID queryUD, LLUUID ownerID, LLUUID itemID )
		{
			int packetLength = 8; // header
			packetLength += 16; // AgentID (UUID)
			packetLength += 16; // QueryID (UUID)
			packetLength += 16; // OwnerID (UUID)
			packetLength += 16; // ItemID (UUID)

			Packet packet = new Packet("RequestInventoryAsset", protocol, packetLength );

			int pos = 8; // Leave room for header

			// AgentID
			Array.Copy(agentID.Data, 0, packet.Data, pos, 16);
			pos += 16;

			// QueryID
			Array.Copy(queryUD.Data, 0, packet.Data, pos, 16);
			pos += 16;

			// OwnerID
			Array.Copy(ownerID.Data, 0, packet.Data, pos, 16);
			pos += 16;

			// ItemID
			Array.Copy(itemID.Data, 0, packet.Data, pos, 16);
			pos += 16;

			// Set the packet flags
			packet.Data[0] = Helpers.MSG_ZEROCODED + Helpers.MSG_RELIABLE;

			return packet;
		}

		/*
		Low 00322 - UpdateInventoryItemAsset - Untrusted - Unencoded
			0065 InventoryData (Variable)
				0680 AssetID (LLUUID / 1)
				0968 ItemID (LLUUID / 1)
			1297 AgentData (01)
				0219 AgentID (LLUUID / 1)
		Low 00326 - ChangeInventoryItemFlags - Untrusted - Zerocoded
			0065 InventoryData (Variable)
				0968 ItemID (LLUUID / 1)
				1189 Flags (U32 / 1)
			1297 AgentData (01)
				0219 AgentID (LLUUID / 1)
		*/

		public static Packet UpdateInventoryItem(ProtocolManager protocol, InventoryItem iitem, LLUUID agentID)
		{
			return UpdateInventoryItem( protocol
				, iitem.GroupOwned
				, InventoryUpdateCRC(iitem)
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
		private static Packet UpdateInventoryItem(ProtocolManager protocol
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


		public static uint InventoryUpdateCRC(InventoryItem iitem)
		{
			return InventoryUpdateCRC( 
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

		private static uint InventoryUpdateCRC ( 
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
