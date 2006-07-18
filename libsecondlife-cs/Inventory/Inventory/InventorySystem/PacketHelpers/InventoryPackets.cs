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
