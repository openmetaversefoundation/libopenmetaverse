using System;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    public class InventoryManager : ISimianExtension, IInventoryProvider
    {
        Simian Server;

        public InventoryManager(Simian server)
        {
            Server = server;
        }

        public void Start()
        {
            Server.UDPServer.RegisterPacketCallback(PacketType.CreateInventoryItem, new UDPServer.PacketCallback(CreateInventoryItemHandler));
            Server.UDPServer.RegisterPacketCallback(PacketType.CreateInventoryFolder, new UDPServer.PacketCallback(CreateInventoryFolderHandler));
            Server.UDPServer.RegisterPacketCallback(PacketType.UpdateInventoryItem, new UDPServer.PacketCallback(UpdateInventoryItemHandler));
            Server.UDPServer.RegisterPacketCallback(PacketType.FetchInventoryDescendents, new UDPServer.PacketCallback(FetchInventoryDescendentsHandler));
        }

        public void Stop()
        {
        }

        void CreateInventoryItemHandler(Packet packet, Agent agent)
        {
            CreateInventoryItemPacket create = (CreateInventoryItemPacket)packet;
            UUID assetID = UUID.Combine(create.InventoryBlock.TransactionID, agent.SecureSessionID);
            UUID parentID = (create.InventoryBlock.FolderID != UUID.Zero) ? create.InventoryBlock.FolderID : agent.InventoryRoot;

            CreateItem(agent, Utils.BytesToString(create.InventoryBlock.Name), "Created in Simian",
                (InventoryType)create.InventoryBlock.InvType, (AssetType)create.InventoryBlock.Type, assetID,
                parentID, PermissionMask.All, (PermissionMask)create.InventoryBlock.NextOwnerMask, agent.AgentID,
                agent.AgentID, create.InventoryBlock.TransactionID, create.InventoryBlock.CallbackID);
            
            /*uint fullPerms = (uint)PermissionMask.All;
            

            lock (agent.Inventory)
            {
                InventoryObject parent;
                if (agent.Inventory.TryGetValue(parentID, out parent))
                {
                    InventoryFolder parentFolder = (InventoryFolder)parent;

                    // Create an item from the incoming request
                    InventoryItem item = new InventoryItem();
                    item.ID = UUID.Random();
                    item.InventoryType = (InventoryType)create.InventoryBlock.InvType;
                    item.ParentID = parentID;
                    item.Parent = parentFolder;
                    item.Name = Utils.BytesToString(create.InventoryBlock.Name);
                    item.Permissions.OwnerMask = (PermissionMask)fullPerms;
                    item.Permissions.NextOwnerMask = (PermissionMask)create.InventoryBlock.NextOwnerMask;
                    item.AssetType = (AssetType)create.InventoryBlock.Type;
                    item.OwnerID = agent.AgentID;
                    item.CreatorID = agent.AgentID;
                    item.CreationDate = DateTime.Now;

                    Logger.DebugLog(String.Format("Creating inventory item {0} (InvType: {1}, AssetType: {2})", item.Name,
                        item.InventoryType, item.AssetType));

                    // Store the inventory item
                    agent.Inventory[item.ID] = item;
                    lock (parentFolder.Children.Dictionary)
                        parentFolder.Children.Dictionary[item.ID] = item;

                    // Send a success response
                    UpdateCreateInventoryItemPacket update = new UpdateCreateInventoryItemPacket();
                    update.AgentData.AgentID = agent.AgentID;
                    update.AgentData.SimApproved = true;
                    update.AgentData.TransactionID = create.InventoryBlock.TransactionID;
                    update.InventoryData = new UpdateCreateInventoryItemPacket.InventoryDataBlock[1];
                    update.InventoryData[0] = new UpdateCreateInventoryItemPacket.InventoryDataBlock();
                    update.InventoryData[0].AssetID = assetID;
                    update.InventoryData[0].BaseMask = fullPerms;
                    update.InventoryData[0].CallbackID = create.InventoryBlock.CallbackID;
                    update.InventoryData[0].CreationDate = (int)Utils.DateTimeToUnixTime(item.CreationDate);
                    update.InventoryData[0].CRC =
                        Helpers.InventoryCRC(update.InventoryData[0].CreationDate, (byte)item.SaleType,
                        (sbyte)item.InventoryType, (sbyte)item.AssetType, item.AssetID, item.GroupID, item.SalePrice,
                        item.OwnerID, item.CreatorID, item.ID, item.ParentID, (uint)item.Permissions.EveryoneMask,
                        item.Flags, (uint)item.Permissions.NextOwnerMask, (uint)item.Permissions.GroupMask,
                        (uint)item.Permissions.OwnerMask);
                    update.InventoryData[0].CreatorID = item.CreatorID;
                    update.InventoryData[0].Description = Utils.StringToBytes("Created in Simian");
                    update.InventoryData[0].EveryoneMask = (uint)item.Permissions.EveryoneMask;
                    update.InventoryData[0].Flags = item.Flags;
                    update.InventoryData[0].FolderID = item.ParentID;
                    update.InventoryData[0].GroupID = item.GroupID;
                    update.InventoryData[0].GroupMask = (uint)item.Permissions.GroupMask;
                    update.InventoryData[0].GroupOwned = item.GroupOwned;
                    update.InventoryData[0].InvType = (sbyte)item.InventoryType;
                    update.InventoryData[0].ItemID = item.ID;
                    update.InventoryData[0].Name = Utils.StringToBytes(item.Name);
                    update.InventoryData[0].NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
                    update.InventoryData[0].OwnerID = item.OwnerID;
                    update.InventoryData[0].OwnerMask = (uint)item.Permissions.OwnerMask;
                    update.InventoryData[0].SalePrice = item.SalePrice;
                    update.InventoryData[0].SaleType = (byte)item.SaleType;
                    update.InventoryData[0].Type = (sbyte)item.AssetType;

                    agent.SendPacket(update);
                }
                else
                {
                    Logger.Log(String.Format(
                        "Cannot created new inventory item, folder {0} does not exist",
                        parentID), Helpers.LogLevel.Warning);
                }
            }*/
        }

        void CreateInventoryFolderHandler(Packet packet, Agent agent)
        {
            CreateInventoryFolderPacket create = (CreateInventoryFolderPacket)packet;

            lock (agent.Inventory)
            {
                InventoryObject parent;
                if (agent.Inventory.TryGetValue(create.FolderData.ParentID, out parent))
                {
                    InventoryFolder parentFolder = (InventoryFolder)parent;

                    if (!agent.Inventory.ContainsKey(create.FolderData.FolderID))
                    {
                        InventoryFolder folder = new InventoryFolder();
                        folder.Name = Utils.BytesToString(create.FolderData.Name);
                        folder.OwnerID = agent.AgentID;
                        folder.ParentID = create.FolderData.ParentID;
                        folder.Parent = parentFolder;
                        folder.PreferredType = (AssetType)create.FolderData.Type;
                        folder.ID = create.FolderData.FolderID;
                        folder.Version = 1;

                        Logger.DebugLog(String.Format("Creating inventory folder {0}", folder.Name));

                        agent.Inventory[folder.ID] = folder;
                        lock (parentFolder.Children.Dictionary)
                            parentFolder.Children.Dictionary[folder.ID] = folder;
                    }
                    else
                    {
                        Logger.Log(String.Format(
                            "Cannot create new inventory folder, item {0} already exists",
                            create.FolderData.FolderID), Helpers.LogLevel.Warning);
                    }
                }
                else
                {
                    Logger.Log(String.Format(
                        "Cannot create new inventory folder, parent folder {0} does not exist",
                        create.FolderData.ParentID), Helpers.LogLevel.Warning);
                }
            }
        }

        void UpdateInventoryItemHandler(Packet packet, Agent agent)
        {
            UpdateInventoryItemPacket update = (UpdateInventoryItemPacket)packet;

            // TODO: Are we supposed to send this here?
            /*BulkUpdateInventoryPacket response = new BulkUpdateInventoryPacket();
            response.AgentData.AgentID = agent.AgentID;
            response.AgentData.TransactionID = update.InventoryData[0].TransactionID;
            response.FolderData = new BulkUpdateInventoryPacket.FolderDataBlock();

            response.ItemData = new BulkUpdateInventoryPacket.ItemDataBlock[update.InventoryData.Length];
            for (int i = 0; i < update.InventoryData.Length; i++)
            {
                response.ItemData[i] = new BulkUpdateInventoryPacket.ItemDataBlock();
                response.ItemData[i].AssetID = ...;
            }*/
        }

        void FetchInventoryDescendentsHandler(Packet packet, Agent agent)
        {
            FetchInventoryDescendentsPacket fetch = (FetchInventoryDescendentsPacket)packet;
            bool sendFolders = fetch.InventoryData.FetchFolders;
            bool sendItems = fetch.InventoryData.FetchItems;
            InventorySortOrder order = (InventorySortOrder)fetch.InventoryData.SortOrder;

            // TODO: Use OwnerID
            // TODO: Do we need to obey InventorySortOrder?
            // FIXME: This packet can become huge very quickly. Add logic to break it up into multiple packets

            InventoryObject invObject;
            if (agent.Inventory.TryGetValue(fetch.InventoryData.FolderID, out invObject) && invObject is InventoryFolder)
            {
                InventoryFolder folder = (InventoryFolder)invObject;

                lock (folder.Children.Dictionary)
                {
                    InventoryDescendentsPacket descendents = new InventoryDescendentsPacket();
                    descendents.AgentData.AgentID = agent.AgentID;
                    descendents.AgentData.Descendents = folder.Children.Count;
                    descendents.AgentData.FolderID = folder.ID;
                    descendents.AgentData.OwnerID = folder.OwnerID;
                    descendents.AgentData.Version = folder.Version;

                    descendents.FolderData = new InventoryDescendentsPacket.FolderDataBlock[0];
                    descendents.ItemData = new InventoryDescendentsPacket.ItemDataBlock[0];

                    if (sendItems || sendFolders)
                    {
                        List<InventoryItem> items = new List<InventoryItem>();
                        List<InventoryFolder> folders = new List<InventoryFolder>();

                        folder.Children.ForEach(
                            delegate(InventoryObject obj)
                            {
                                if (obj is InventoryItem)
                                    items.Add((InventoryItem)obj);
                                else
                                    folders.Add((InventoryFolder)obj);
                            }
                        );

                        if (sendItems)
                        {
                            descendents.ItemData = new InventoryDescendentsPacket.ItemDataBlock[items.Count];
                            for (int i = 0; i < items.Count; i++)
                            {
                                InventoryItem currentItem = items[i];

                                descendents.ItemData[i] = new InventoryDescendentsPacket.ItemDataBlock();
                                descendents.ItemData[i].AssetID = currentItem.AssetID;
                                descendents.ItemData[i].BaseMask = (uint)currentItem.Permissions.BaseMask;
                                descendents.ItemData[i].CRC = Helpers.InventoryCRC(
                                    (int)Utils.DateTimeToUnixTime(currentItem.CreationDate),
                                    (byte)currentItem.SaleType, (sbyte)currentItem.InventoryType,
                                    (sbyte)currentItem.AssetType, currentItem.AssetID, currentItem.GroupID,
                                    currentItem.SalePrice, currentItem.OwnerID, currentItem.CreatorID, currentItem.ID,
                                    currentItem.ParentID, (uint)currentItem.Permissions.EveryoneMask, currentItem.Flags,
                                    (uint)currentItem.Permissions.NextOwnerMask, (uint)currentItem.Permissions.GroupMask,
                                    (uint)currentItem.Permissions.OwnerMask);
                                descendents.ItemData[i].CreationDate = (int)Utils.DateTimeToUnixTime(currentItem.CreationDate);
                                descendents.ItemData[i].CreatorID = currentItem.CreatorID;
                                descendents.ItemData[i].Description = Utils.StringToBytes(currentItem.Description);
                                descendents.ItemData[i].EveryoneMask = (uint)currentItem.Permissions.EveryoneMask;
                                descendents.ItemData[i].Flags = currentItem.Flags;
                                descendents.ItemData[i].FolderID = currentItem.ParentID;
                                descendents.ItemData[i].GroupID = currentItem.GroupID;
                                descendents.ItemData[i].GroupMask = (uint)currentItem.Permissions.GroupMask;
                                descendents.ItemData[i].GroupOwned = currentItem.GroupOwned;
                                descendents.ItemData[i].InvType = (sbyte)currentItem.InventoryType;
                                descendents.ItemData[i].ItemID = currentItem.ID;
                                descendents.ItemData[i].Name = Utils.StringToBytes(currentItem.Name);
                                descendents.ItemData[i].NextOwnerMask = (uint)currentItem.Permissions.NextOwnerMask;
                                descendents.ItemData[i].OwnerID = currentItem.OwnerID;
                                descendents.ItemData[i].OwnerMask = (uint)currentItem.Permissions.OwnerMask;
                                descendents.ItemData[i].SalePrice = currentItem.SalePrice;
                                descendents.ItemData[i].SaleType = (byte)currentItem.SaleType;
                                descendents.ItemData[i].Type = (sbyte)currentItem.AssetType;
                            }
                        }

                        if (sendFolders)
                        {
                            descendents.FolderData = new InventoryDescendentsPacket.FolderDataBlock[folders.Count];
                            for (int i = 0; i < folders.Count; i++)
                            {
                                InventoryFolder currentFolder = folders[i];

                                descendents.FolderData[i] = new InventoryDescendentsPacket.FolderDataBlock();
                                descendents.FolderData[i].FolderID = currentFolder.ID;
                                descendents.FolderData[i].Name = Utils.StringToBytes(currentFolder.Name);
                                descendents.FolderData[i].ParentID = currentFolder.ParentID;
                                descendents.FolderData[i].Type = (sbyte)currentFolder.PreferredType;
                            }
                        }
                    }

                    agent.SendPacket(descendents);
                }
            }
            else
            {
                Logger.Log(String.Format(
                    "FetchInventoryDescendents called for an unknown folder {0}",
                    fetch.InventoryData.FolderID), Helpers.LogLevel.Warning);
            }
        }

        public UUID CreateItem(Agent agent, string name, string description, InventoryType invType, AssetType type,
            UUID assetID, UUID parentID, PermissionMask ownerMask, PermissionMask nextOwnerMask, UUID ownerID,
            UUID creatorID, UUID transactionID, uint callbackID)
        {
            if (parentID == UUID.Zero)
                parentID = agent.InventoryRoot;

            lock (agent.Inventory)
            {
                InventoryObject parent;
                if (agent.Inventory.TryGetValue(parentID, out parent) && parent is InventoryFolder)
                {
                    InventoryFolder parentFolder = (InventoryFolder)parent;

                    // Create an item
                    InventoryItem item = new InventoryItem();
                    item.ID = UUID.Random();
                    item.InventoryType = invType;
                    item.ParentID = parentID;
                    item.Parent = parentFolder;
                    item.Name = name;
                    item.Description = description;
                    item.Permissions.OwnerMask = ownerMask;
                    item.Permissions.NextOwnerMask = nextOwnerMask;
                    item.AssetType = type;
                    item.OwnerID = agent.AgentID;
                    item.CreatorID = agent.AgentID;
                    item.CreationDate = DateTime.Now;

                    Logger.DebugLog(String.Format("Creating inventory item {0} (InvType: {1}, AssetType: {2})", item.Name,
                        item.InventoryType, item.AssetType));

                    // Store the inventory item
                    agent.Inventory[item.ID] = item;
                    lock (parentFolder.Children.Dictionary)
                        parentFolder.Children.Dictionary[item.ID] = item;

                    // Send a success response
                    UpdateCreateInventoryItemPacket update = new UpdateCreateInventoryItemPacket();
                    update.AgentData.AgentID = agent.AgentID;
                    update.AgentData.SimApproved = true;
                    update.AgentData.TransactionID = transactionID;
                    update.InventoryData = new UpdateCreateInventoryItemPacket.InventoryDataBlock[1];
                    update.InventoryData[0] = new UpdateCreateInventoryItemPacket.InventoryDataBlock();
                    update.InventoryData[0].AssetID = assetID;
                    update.InventoryData[0].BaseMask = (uint)PermissionMask.All;
                    update.InventoryData[0].CallbackID = callbackID;
                    update.InventoryData[0].CreationDate = (int)Utils.DateTimeToUnixTime(item.CreationDate);
                    update.InventoryData[0].CRC =
                        Helpers.InventoryCRC((int)Utils.DateTimeToUnixTime(item.CreationDate), (byte)item.SaleType,
                        (sbyte)item.InventoryType, (sbyte)item.AssetType, item.AssetID, item.GroupID, item.SalePrice,
                        item.OwnerID, item.CreatorID, item.ID, item.ParentID, (uint)item.Permissions.EveryoneMask,
                        item.Flags, (uint)item.Permissions.NextOwnerMask, (uint)item.Permissions.GroupMask,
                        (uint)item.Permissions.OwnerMask);
                    update.InventoryData[0].CreatorID = item.CreatorID;
                    update.InventoryData[0].Description = Utils.StringToBytes(item.Description);
                    update.InventoryData[0].EveryoneMask = (uint)item.Permissions.EveryoneMask;
                    update.InventoryData[0].Flags = item.Flags;
                    update.InventoryData[0].FolderID = item.ParentID;
                    update.InventoryData[0].GroupID = item.GroupID;
                    update.InventoryData[0].GroupMask = (uint)item.Permissions.GroupMask;
                    update.InventoryData[0].GroupOwned = item.GroupOwned;
                    update.InventoryData[0].InvType = (sbyte)item.InventoryType;
                    update.InventoryData[0].ItemID = item.ID;
                    update.InventoryData[0].Name = Utils.StringToBytes(item.Name);
                    update.InventoryData[0].NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
                    update.InventoryData[0].OwnerID = item.OwnerID;
                    update.InventoryData[0].OwnerMask = (uint)item.Permissions.OwnerMask;
                    update.InventoryData[0].SalePrice = item.SalePrice;
                    update.InventoryData[0].SaleType = (byte)item.SaleType;
                    update.InventoryData[0].Type = (sbyte)item.AssetType;

                    agent.SendPacket(update);

                    return item.ID;
                }
                else
                {
                    Logger.Log(String.Format(
                        "Cannot created new inventory item, folder {0} does not exist",
                        parentID), Helpers.LogLevel.Warning);

                    return UUID.Zero;
                }
            }
        }
    }
}
