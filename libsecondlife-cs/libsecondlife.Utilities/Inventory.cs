using System;
using System.Collections.Generic;
using System.Threading;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.Utilities.Inventory
{
    public enum InventoryType
    {
        Unknown = -1,
        Texture = 0,
        Sound = 1,
        CallingCard = 2,
        Landmark = 3,
        [Obsolete]
        Script = 4,
        [Obsolete]
        Clothing = 5,
        Object = 6,
        Notecard = 7,
        Category = 8,
        RootCategory = 0,
        LSL = 10,
        [Obsolete]
        LSLBytecode = 11,
        [Obsolete]
        TextureTGA = 12,
        [Obsolete]
        Bodypart = 13,
        [Obsolete]
        Trash = 14,
        Snapshot = 15,
        [Obsolete]
        LostAndFound = 16,
        Attachment = 17,
        Wearable = 18,
        Animation = 19,
        Gesture = 20
    }

    public struct Permissions
    {
        public uint BaseMask;
        public uint EveryoneMask;
        public uint GroupMask;
        public uint NextOwnerMask;
        public uint OwnerMask;
    }

    public struct InventoryItem
    {
        public string Name;
        public string Description;
        public InventoryType InvType;
        public Assets.AssetType AssetType;
        public LLUUID AssetID;
        public LLUUID ItemID;
        public LLUUID OwnerID;
        public LLUUID GroupID;
        public LLUUID CreatorID;
        public LLUUID FolderID;
        public bool GroupOwned;
        public ObjectManager.SaleType SaleType;
        public int SalePrice;
        public Permissions Permissions;
        public uint Flags;

        public override string ToString()
        {
            return String.Format("{0} ({1}) InvType: {2} AssetType: {3} AssetID: {4} ItemID: {5}", Name, Description,
                InvType.ToString(), AssetType.ToString(), AssetID.ToStringHyphenated(), ItemID.ToStringHyphenated());
        }
    }


    public class InventoryManager
    {
        public delegate void NewInventoryCallback(InventoryItem item);


        public event NewInventoryCallback OnNewInventory;


        private SecondLife Client;


        public InventoryManager(SecondLife client)
        {
            Client = client;

            Client.Network.RegisterCallback(PacketType.UpdateCreateInventoryItem, 
                new NetworkManager.PacketCallback(UpdateCreateInventoryItemHandler));
        }

        public void AttachFromInventory(InventoryItem item, ObjectManager.AttachmentPoint attachPoint)
        {
            AttachFromInventory(item.ItemID, item.Name, item.Description, item.OwnerID, item.Permissions.EveryoneMask,
                item.Permissions.GroupMask, item.Permissions.NextOwnerMask, item.Flags, attachPoint);
        }

        public void AttachFromInventory(LLUUID itemID, string name, string description, LLUUID ownerID, 
            uint everyoneMask, uint groupMask, uint nextOwnerMask, uint flags, 
            ObjectManager.AttachmentPoint attachPoint)
        {
            RezSingleAttachmentFromInvPacket rez = new RezSingleAttachmentFromInvPacket();

            rez.AgentData.AgentID = Client.Network.AgentID;
            rez.AgentData.SessionID = Client.Network.SessionID;

            rez.ObjectData.AttachmentPt = (byte)attachPoint;
            rez.ObjectData.Description = Helpers.StringToField(description);
            rez.ObjectData.EveryoneMask = everyoneMask;
            rez.ObjectData.GroupMask = groupMask;
            rez.ObjectData.ItemFlags = flags;
            rez.ObjectData.ItemID = itemID;
            rez.ObjectData.Name = Helpers.StringToField(name);
            rez.ObjectData.NextOwnerMask = nextOwnerMask;
            rez.ObjectData.OwnerID = ownerID;

            Client.Network.SendPacket(rez);
        }

        private void UpdateCreateInventoryItemHandler(Packet packet, Simulator simulator)
        {
            UpdateCreateInventoryItemPacket create = (UpdateCreateInventoryItemPacket)packet;

            for (int i = 0; i < create.InventoryData.Length; i++)
            {
                UpdateCreateInventoryItemPacket.InventoryDataBlock block = create.InventoryData[i];

                InventoryItem item = new InventoryItem();
                item.AssetID = block.AssetID;
                item.AssetType = (Assets.AssetType)block.Type;
                item.CreatorID = block.CreatorID;
                item.Description = Helpers.FieldToUTF8String(block.Description);
                item.Flags = block.Flags;
                item.FolderID = block.FolderID;
                item.GroupID = block.GroupID;
                item.GroupOwned = block.GroupOwned;
                item.InvType = (InventoryType)block.InvType;
                item.ItemID = block.ItemID;
                item.Name = Helpers.FieldToUTF8String(block.Name);
                item.OwnerID = block.OwnerID;
                item.SalePrice = block.SalePrice;
                item.SaleType = (ObjectManager.SaleType)block.SaleType;

                item.Permissions.BaseMask = block.BaseMask;
                item.Permissions.EveryoneMask = block.EveryoneMask;
                item.Permissions.GroupMask = block.GroupMask;
                item.Permissions.NextOwnerMask = block.NextOwnerMask;
                item.Permissions.OwnerMask = block.OwnerMask;

                if (OnNewInventory != null)
                {
                    try { OnNewInventory(item); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
            }
        }
    }
}
