using System;
using System.Collections;

using libsecondlife;

namespace libsecondlife.InventorySystem.PacketHelpers
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

			Packet packet = new Packet("AgentWearablesRequest", protocol, packetLength );

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
	}

}
