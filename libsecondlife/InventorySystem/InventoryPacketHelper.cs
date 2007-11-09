using System;
using System.Collections.Generic;

using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.InventorySystem
{
	/// <summary>
	/// Summary description for Other.
	/// </summary>
    public class InventoryPacketHelper
	{
        private SecondLife Client;

        private LLUUID AgentID
        {
            get { return Client.Network.AgentID; }
        }

        private LLUUID SessionID
        {
            get { return Client.Network.SessionID; }
        }

        public InventoryPacketHelper(SecondLife client)
		{
            Client = client;
		}

		public const int FETCH_INVENTORY_SORT_NAME = 0;
		public const int FETCH_INVENTORY_SORT_TIME = 1;


		public Packet FetchInventoryDescendents( LLUUID folderID )
		{
			return FetchInventoryDescendents( folderID, true, true );
		}

		public Packet FetchInventoryDescendents( LLUUID folderID, bool fetchFolders, bool fetchItems )
		{
            FetchInventoryDescendentsPacket p = new FetchInventoryDescendentsPacket();
            p.InventoryData.OwnerID      = AgentID;
            p.InventoryData.FolderID     = folderID;
            p.InventoryData.SortOrder    = FETCH_INVENTORY_SORT_NAME;
            p.InventoryData.FetchFolders = fetchFolders;
            p.InventoryData.FetchItems   = fetchItems;

            p.AgentData.AgentID = AgentID;
            p.AgentData.SessionID = SessionID;

            return p;
		}

        public Packet FetchInventory(LLUUID OwnerID, LLUUID ItemID)
        {
            FetchInventoryPacket p = new FetchInventoryPacket();
            p.InventoryData = new FetchInventoryPacket.InventoryDataBlock[1];
            p.InventoryData[0] = new FetchInventoryPacket.InventoryDataBlock();
            p.InventoryData[0].OwnerID = OwnerID;
            p.InventoryData[0].ItemID = ItemID;

            p.AgentData.AgentID = AgentID;
            p.AgentData.SessionID = SessionID;

            return p;
        }

		public Packet CreateInventoryFolder(
			string name
			, LLUUID parentID
			, sbyte  type
			, LLUUID folderID
			)
		{
            CreateInventoryFolderPacket p = new CreateInventoryFolderPacket();
            p.AgentData.AgentID   = AgentID;
            p.AgentData.SessionID = SessionID;

            p.FolderData.Name     = Helpers.StringToField(name);
            p.FolderData.ParentID = parentID;
            p.FolderData.Type     = type;
            p.FolderData.FolderID = folderID;

            return p;
        }



		public Packet MoveInventoryFolder(
			LLUUID parentID
			, LLUUID folderID
			)
		{
            MoveInventoryFolderPacket p = new MoveInventoryFolderPacket();
            p.AgentData.AgentID   = AgentID;
            p.AgentData.SessionID = SessionID;
            p.AgentData.Stamp     = false;

            p.InventoryData = new MoveInventoryFolderPacket.InventoryDataBlock[1];
            p.InventoryData[0] = new MoveInventoryFolderPacket.InventoryDataBlock();

            p.InventoryData[0].ParentID = parentID;
            p.InventoryData[0].FolderID = folderID;

            return p;

		}

		
		public Packet RemoveInventoryFolder(
			LLUUID folderID
			)
		{
            RemoveInventoryFolderPacket p = new RemoveInventoryFolderPacket();
            p.AgentData.AgentID = AgentID;
            p.AgentData.SessionID = SessionID;

            p.FolderData = new RemoveInventoryFolderPacket.FolderDataBlock[1];
            p.FolderData[0] = new RemoveInventoryFolderPacket.FolderDataBlock();

            p.FolderData[0].FolderID = folderID;

            return p;

		}


		public Packet UpdateInventoryFolder(
			string name
			, LLUUID parentID
			, sbyte  type
			, LLUUID folderID
			)
		{
            UpdateInventoryFolderPacket p = new UpdateInventoryFolderPacket();
            p.AgentData.AgentID = AgentID;
            p.AgentData.SessionID = SessionID;

            p.FolderData = new UpdateInventoryFolderPacket.FolderDataBlock[1];
            p.FolderData[0] = new UpdateInventoryFolderPacket.FolderDataBlock();


            p.FolderData[0].Name     = Helpers.StringToField(name);
            p.FolderData[0].ParentID = parentID;
            p.FolderData[0].Type     = type;
            p.FolderData[0].FolderID = folderID;

            return p;

		}

        /// <summary>
        /// Move an item to a new folder
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="folderID"></param>
        /// <returns></returns>
        public Packet MoveInventoryItem(
            LLUUID itemID
            , LLUUID folderID
            )
        {
            return MoveInventoryItem(itemID, folderID, null);
        }

        /// <summary>
        /// Move item to new folder, optionally changing it's name in the process
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="folderID"></param>
        /// <param name="itemName">specify null to not change the name</param>
        /// <returns></returns>
        public Packet MoveInventoryItem(
            LLUUID itemID
            , LLUUID folderID
            , string itemName
            )
        {
            MoveInventoryItemPacket p = new MoveInventoryItemPacket();
            p.AgentData.AgentID = AgentID;
            p.AgentData.SessionID = SessionID;
            p.AgentData.Stamp = false; //the official client uses false.

            p.InventoryData = new MoveInventoryItemPacket.InventoryDataBlock[1];
            p.InventoryData[0] = new MoveInventoryItemPacket.InventoryDataBlock();

            p.InventoryData[0].ItemID = itemID;
            p.InventoryData[0].FolderID = folderID;

            if (itemName == null)
            {
                p.InventoryData[0].NewName = new byte[0];
            }
            else
            {
                p.InventoryData[0].NewName = Helpers.StringToField(itemName);
            }

            return p;

        }

        public Packet CopyInventoryItem(
            LLUUID itemID
            , LLUUID folderID
            )
        {

            return CopyInventoryItem( itemID, folderID, null );
        }

        public Packet CopyInventoryItem(
            LLUUID itemID
            , LLUUID folderID
            , string newItemName
            )
        {
            CopyInventoryItemPacket p = new CopyInventoryItemPacket();
            p.AgentData.AgentID = AgentID;
            p.AgentData.SessionID = SessionID;

            p.InventoryData = new CopyInventoryItemPacket.InventoryDataBlock[1];
            p.InventoryData[0] = new CopyInventoryItemPacket.InventoryDataBlock();

            p.InventoryData[0].CallbackID = 0;
            p.InventoryData[0].OldAgentID = AgentID; //TODO: Find out what this is supposed to be.  Added field 10/11/06, no docs in Message Template

            p.InventoryData[0].OldItemID = itemID;
            p.InventoryData[0].NewFolderID = folderID;

            if (newItemName != null)
            {
                p.InventoryData[0].NewName = Helpers.StringToField(newItemName);
            }

            return p;
        }

        public Packet RemoveInventoryItem(
			LLUUID itemID
			)
		{
            RemoveInventoryItemPacket p = new RemoveInventoryItemPacket();
            p.AgentData.AgentID = AgentID;
            p.AgentData.SessionID = SessionID;

            p.InventoryData = new RemoveInventoryItemPacket.InventoryDataBlock[1];
            p.InventoryData[0] = new RemoveInventoryItemPacket.InventoryDataBlock();

            p.InventoryData[0].ItemID = itemID;

            return p;

        }

		public Packet GiveItemViaImprovedInstantMessage(
			LLUUID ID
			, LLUUID ToAgentID
			, String FromAgentName
			, LLVector3 FromAgentLoc
			, InventoryItem Item
			)
		{
			byte[] BinaryBucket = new byte[17];
			BinaryBucket[0] = (byte)Item.Type;
            Buffer.BlockCopy(Item.ItemID.Data, 0, BinaryBucket, 1, 16);

            ImprovedInstantMessagePacket p = new ImprovedInstantMessagePacket();
            p.AgentData.AgentID   = AgentID;
            p.AgentData.SessionID = SessionID;

            p.MessageBlock.ID        = ID;
            p.MessageBlock.ToAgentID = ToAgentID;
            p.MessageBlock.Offline   = (byte)0;
            p.MessageBlock.Timestamp = Helpers.GetUnixTime();
            p.MessageBlock.Message   = Helpers.StringToField(Item.Name);
            p.MessageBlock.Dialog    = (byte)4;
            p.MessageBlock.BinaryBucket  = BinaryBucket;
            p.MessageBlock.FromAgentName = Helpers.StringToField(FromAgentName);
            p.MessageBlock.Position      = FromAgentLoc;

            // TODO: Either overload this method to allow inclusion of region info or
            // overload the ImprovedInstantMessage in the avatar class to allow item payloads
            p.MessageBlock.RegionID = LLUUID.Zero;
            p.MessageBlock.ParentEstateID = (uint)0;

            return p;
		}



        public Packet CreateInventoryItem(InventoryItem iitem)
        {
            CreateInventoryItemPacket p = new CreateInventoryItemPacket();

            p.InventoryBlock.CallbackID    = 0;
            p.InventoryBlock.TransactionID = LLUUID.Zero;

            p.InventoryBlock.WearableType  = 0; //TODO: Specify the current type here
            p.InventoryBlock.Type    = iitem.Type;
            p.InventoryBlock.InvType = iitem.InvType;

            p.InventoryBlock.Name = Helpers.StringToField(iitem.Name);
            p.InventoryBlock.FolderID = iitem.FolderID;
            p.InventoryBlock.Description = Helpers.StringToField(iitem.Description);

            p.InventoryBlock.NextOwnerMask = iitem.NextOwnerMask;

            p.AgentData.AgentID = AgentID;
            p.AgentData.SessionID = SessionID;

            return p;

        }

        public Packet UpdateInventoryItem(InventoryItem iitem)
        {
            UpdateInventoryItemPacket p = new UpdateInventoryItemPacket();
            p.InventoryData = new UpdateInventoryItemPacket.InventoryDataBlock[1];
            p.InventoryData[0] = new UpdateInventoryItemPacket.InventoryDataBlock();

            p.InventoryData[0].TransactionID = iitem.TransactionID;

            p.InventoryData[0].GroupOwned   = iitem.GroupOwned;
            p.InventoryData[0].CRC          = iitem.CRC;
            p.InventoryData[0].CreationDate = iitem.CreationDate;
			p.InventoryData[0].SaleType		= iitem.SaleType;
			p.InventoryData[0].BaseMask		= iitem.BaseMask;
            p.InventoryData[0].Name         = Helpers.StringToField(iitem.Name);
			p.InventoryData[0].InvType		= iitem.InvType;
			p.InventoryData[0].Type			= iitem.Type;
			p.InventoryData[0].GroupID		= iitem.GroupID;
			p.InventoryData[0].SalePrice	= iitem.SalePrice;
			p.InventoryData[0].OwnerID		= iitem.OwnerID;
			p.InventoryData[0].CreatorID	= iitem.CreatorID;
			p.InventoryData[0].ItemID		= iitem.ItemID;
			p.InventoryData[0].FolderID		= iitem.FolderID;
            p.InventoryData[0].EveryoneMask = iitem.EveryoneMask;
            p.InventoryData[0].Description  = Helpers.StringToField(iitem.Description);
			p.InventoryData[0].Flags		= iitem.Flags;
			p.InventoryData[0].NextOwnerMask= iitem.NextOwnerMask;
			p.InventoryData[0].GroupMask	= iitem.GroupMask;
			p.InventoryData[0].OwnerMask	= iitem.OwnerMask;


			p.AgentData.AgentID   = AgentID;
			p.AgentData.SessionID = SessionID;

            return p;
		}


        public Packet RezObject(InventoryItem iitem, LLVector3 TargetPos)
        {
            if (iitem.Type != 6)
            {
                Client.Log("RezObject has only been tested with items of type 6", Helpers.LogLevel.Warning);
            }

            RezObjectPacket p = new RezObjectPacket();
            p.AgentData.AgentID = AgentID;
            p.AgentData.SessionID = SessionID;
            p.AgentData.GroupID = LLUUID.Zero;

            p.InventoryData.TransactionID = LLUUID.Random();

            p.InventoryData.GroupOwned   = iitem.GroupOwned;
            p.InventoryData.CRC          = iitem.CRC;
            p.InventoryData.CreationDate = iitem.CreationDate;
			p.InventoryData.SaleType	 = iitem.SaleType;
			p.InventoryData.BaseMask	 = iitem.BaseMask;
            p.InventoryData.Name         = Helpers.StringToField(iitem.Name);
			p.InventoryData.InvType		 = iitem.InvType;
			p.InventoryData.Type			= iitem.Type;
			p.InventoryData.GroupID		 = iitem.GroupID;
			p.InventoryData.SalePrice	 = iitem.SalePrice;
			p.InventoryData.OwnerID		 = iitem.OwnerID;
			p.InventoryData.CreatorID	 = iitem.CreatorID;
			p.InventoryData.ItemID		 = iitem.ItemID;
			p.InventoryData.FolderID	 = iitem.FolderID;
            p.InventoryData.EveryoneMask = iitem.EveryoneMask;
            p.InventoryData.Description  = Helpers.StringToField(iitem.Description);
			p.InventoryData.Flags		 = iitem.Flags;
			p.InventoryData.NextOwnerMask= iitem.NextOwnerMask;
			p.InventoryData.GroupMask	 = iitem.GroupMask;
			p.InventoryData.OwnerMask	 = iitem.OwnerMask;

	        p.RezData.FromTaskID = LLUUID.Zero;
		    p.RezData.BypassRaycast = 1;
		    p.RezData.RayStart = Client.Self.Position;
            p.RezData.RayEnd = TargetPos;
		    p.RezData.RayTargetID = LLUUID.Zero;
		    p.RezData.RayEndIsIntersection = false;
		    p.RezData.RezSelected = false;
		    p.RezData.RemoveItem = false;
		    p.RezData.ItemFlags = iitem.Flags;
		    p.RezData.GroupMask = iitem.GroupMask;
		    p.RezData.EveryoneMask = iitem.EveryoneMask;
            p.RezData.NextOwnerMask = iitem.NextOwnerMask;

            return p;
        }

        public Packet RezSingleAttachmentFromInv(InventoryItem item)
        {
            return RezSingleAttachmentFromInv(item, 0); // Use default point
        }

        public Packet RezSingleAttachmentFromInv(InventoryItem item, ObjectManager.AttachmentPoint AttachmentPt)
        {
            RezSingleAttachmentFromInvPacket p = new RezSingleAttachmentFromInvPacket();
            p.AgentData.AgentID = AgentID;
            p.AgentData.SessionID = SessionID;

            p.ObjectData.Description = Helpers.StringToField(item.Description);
            p.ObjectData.EveryoneMask = item.EveryoneMask;
            p.ObjectData.GroupMask = item.GroupMask;
            p.ObjectData.ItemFlags = item.Flags;
            p.ObjectData.ItemID = item.ItemID;
            p.ObjectData.Name = Helpers.StringToField(item.Name);
            p.ObjectData.NextOwnerMask = item.NextOwnerMask;
            p.ObjectData.OwnerID = item.OwnerID;

            p.ObjectData.AttachmentPt = (byte)AttachmentPt; // Default;


            return p;
        }

        public Packet DetachAttachmentIntoInv( LLUUID ItemID )
        {
            DetachAttachmentIntoInvPacket p = new DetachAttachmentIntoInvPacket();
            p.ObjectData.AgentID = AgentID;
            p.ObjectData.ItemID = ItemID;

            return p;
        }

/*
			// Confirm InventoryUpdate CRC
			uint test = libsecondlife.Packets.InventoryPackets.InventoryUpdateCRC
				        (
							(int)1159214416
							, (byte)0
							, (sbyte)7
							, (sbyte)7
							, (LLUUID)"00000000000000000000000000000000"
							, (LLUUID)"00000000000000000000000000000000"
							, (int)10
							, (LLUUID)"25472683cb324516904a6cd0ecabf128"
							, (LLUUID)"25472683cb324516904a6cd0ecabf128"
							, (LLUUID)"77364021f09f13dfb692f036be53b9e2"
							, (LLUUID)"a4947fc066c247518d9854aaf90097f4"
							, (uint)0
							, (uint)0
							, (uint)2147483647
							, (uint)0
							, (uint)2147483647
				        );

			if( test != (uint)895206313 )
			{
				Console.WriteLine("CRC Generation is no longer correct.");
				return;
			}
*/

		public static uint InventoryUpdateCRC(InventoryItem iitem)
		{
			uint CRC = 0;

			/* IDs */
            CRC += iitem.AssetID.CRC(); // AssetID
            CRC += iitem.FolderID.CRC(); // FolderID
            CRC += iitem.ItemID == null ? LLUUID.Zero.CRC() : iitem.ItemID.CRC(); // ItemID

			/* Permission stuff */
            CRC += iitem.CreatorID.CRC(); // CreatorID
            CRC += iitem.OwnerID.CRC(); // OwnerID
            CRC += iitem.GroupID.CRC(); // GroupID

			/* CRC += another 4 words which always seem to be zero -- unclear if this is a LLUUID or what */
            CRC += iitem.OwnerMask; //owner_mask;      // Either owner_mask or next_owner_mask may need to be
            CRC += iitem.NextOwnerMask; //next_owner_mask; // switched with base_mask -- 2 values go here and in my
            CRC += iitem.EveryoneMask; //everyone_mask;   // study item, the three were identical.
            CRC += iitem.GroupMask; //group_mask;

			/* The rest of the CRC fields */
            CRC += iitem.Flags; // Flags
            CRC += (uint)iitem.InvType; // InvType
            CRC += (uint)iitem.Type; // Type 
            CRC += (uint)iitem.CreationDate; // CreationDate
            CRC += (uint)iitem.SalePrice;    // SalePrice
            CRC += (uint)((uint)iitem.SaleType * 0x07073096); // SaleType

			return CRC;
		}
	}
}
