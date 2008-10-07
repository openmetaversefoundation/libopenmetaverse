using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    public class InventoryManager : IExtension, IInventoryProvider, IPersistable
    {
        Simian Server;
        /// <summary>Dictionary of inventories for each agent. Each inventory
        /// is also a dictionary itself</summary>
        Dictionary<UUID, Dictionary<UUID, InventoryObject>> Inventory =
            new Dictionary<UUID, Dictionary<UUID, InventoryObject>>();
        /// <summary>Global shared inventory for all agent</summary>
        Dictionary<UUID, InventoryObject> Library = new Dictionary<UUID, InventoryObject>();

        public InventoryManager(Simian server)
        {
            Server = server;
        }

        public void Start()
        {
            Server.UDP.RegisterPacketCallback(PacketType.CreateInventoryItem, new PacketCallback(CreateInventoryItemHandler));
            Server.UDP.RegisterPacketCallback(PacketType.CreateInventoryFolder, new PacketCallback(CreateInventoryFolderHandler));
            Server.UDP.RegisterPacketCallback(PacketType.UpdateInventoryItem, new PacketCallback(UpdateInventoryItemHandler));
            Server.UDP.RegisterPacketCallback(PacketType.FetchInventoryDescendents, new PacketCallback(FetchInventoryDescendentsHandler));
            Server.UDP.RegisterPacketCallback(PacketType.FetchInventory, new PacketCallback(FetchInventoryHandler));
            Server.UDP.RegisterPacketCallback(PacketType.CopyInventoryItem, new PacketCallback(CopyInventoryItemHandler));
            Server.UDP.RegisterPacketCallback(PacketType.MoveInventoryItem, new PacketCallback(MoveInventoryItemHandler));
            Server.UDP.RegisterPacketCallback(PacketType.MoveInventoryFolder, new PacketCallback(MoveInventoryFolderHandler));
            Server.UDP.RegisterPacketCallback(PacketType.PurgeInventoryDescendents, new PacketCallback(PurgeInventoryDescendentsHandler));
        }

        public void Stop()
        {
        }

        Dictionary<UUID, InventoryObject> GetAgentInventory(UUID agentID)
        {
            Dictionary<UUID, InventoryObject> agentInventory;
            if (!Inventory.TryGetValue(agentID, out agentInventory))
            {
                Logger.Log("Creating an empty inventory store for agent " + agentID.ToString(),
                    Helpers.LogLevel.Info);

                agentInventory = new Dictionary<UUID, InventoryObject>();
                lock (Inventory)
                    Inventory[agentID] = agentInventory;
            }

            return agentInventory;
        }

        void CreateInventoryItemHandler(Packet packet, Agent agent)
        {
            CreateInventoryItemPacket create = (CreateInventoryItemPacket)packet;
            UUID assetID;
            if (create.InventoryBlock.TransactionID != UUID.Zero)
                assetID = UUID.Combine(create.InventoryBlock.TransactionID, agent.SecureSessionID);
            else
                assetID = UUID.Random();

            UUID parentID = (create.InventoryBlock.FolderID != UUID.Zero) ? create.InventoryBlock.FolderID : agent.InventoryRoot;
            AssetType assetType = (AssetType)create.InventoryBlock.Type;

            switch (assetType)
            {
                case AssetType.Gesture:
                    Logger.Log("Need to create a default gesture asset!", Helpers.LogLevel.Warning);
                    break;
            }

            if (parentID == UUID.Zero)
                parentID = agent.InventoryRoot;

            // Create the inventory item
            CreateItem(agent.AgentID, Utils.BytesToString(create.InventoryBlock.Name), "Created in Simian",
                (InventoryType)create.InventoryBlock.InvType, assetType, assetID, parentID,
                PermissionMask.All, (PermissionMask)create.InventoryBlock.NextOwnerMask, agent.AgentID,
                agent.AgentID, create.InventoryBlock.TransactionID, create.InventoryBlock.CallbackID);
        }

        void CreateInventoryFolderHandler(Packet packet, Agent agent)
        {
            CreateInventoryFolderPacket create = (CreateInventoryFolderPacket)packet;

            UUID folderID = create.FolderData.FolderID;
            if (folderID == UUID.Zero)
                folderID = agent.InventoryRoot;

            CreateFolder(agent.AgentID, folderID, Utils.BytesToString(create.FolderData.Name),
                (AssetType)create.FolderData.Type, create.FolderData.ParentID, agent.AgentID);
        }

        void UpdateInventoryItemHandler(Packet packet, Agent agent)
        {
            UpdateInventoryItemPacket update = (UpdateInventoryItemPacket)packet;

            // No packet is sent back to the client, we just need to update the
            // inventory item locally
            for (int i = 0; i < update.InventoryData.Length; i++)
            {
                UpdateInventoryItemPacket.InventoryDataBlock block = update.InventoryData[i];
                Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agent.AgentID);

                InventoryObject obj;
                if (agentInventory.TryGetValue(block.ItemID, out obj) && obj is InventoryItem)
                {
                    InventoryItem item = (InventoryItem)obj;

                    //item.Permissions.BaseMask = (PermissionMask)block.BaseMask;
                    item.Permissions.BaseMask = PermissionMask.All;
                    //item.Permissions.EveryoneMask = (PermissionMask)block.EveryoneMask;
                    item.Permissions.EveryoneMask = PermissionMask.All;
                    //item.Permissions.GroupMask = (PermissionMask)block.GroupMask;
                    item.Permissions.GroupMask = PermissionMask.All;
                    //item.Permissions.NextOwnerMask = (PermissionMask)block.NextOwnerMask;
                    item.Permissions.NextOwnerMask = PermissionMask.All;
                    //item.Permissions.OwnerMask = (PermissionMask)block.OwnerMask;
                    item.Permissions.OwnerMask = PermissionMask.All;

                    //block.CRC;
                    item.CreationDate = Utils.UnixTimeToDateTime(block.CreationDate);
                    item.CreatorID = block.CreatorID;
                    item.Name = Utils.BytesToString(block.Description);
                    item.Flags = block.Flags;
                    item.ParentID = block.FolderID;
                    item.GroupID = block.GroupID;
                    item.GroupOwned = block.GroupOwned;
                    item.InventoryType = (InventoryType)block.InvType;
                    item.Name = Utils.BytesToString(block.Name);
                    item.OwnerID = block.OwnerID;
                    item.SalePrice = block.SalePrice;
                    item.SaleType = (SaleType)block.SaleType;
                    item.AssetType = (AssetType)block.Type;

                    Logger.DebugLog(String.Format(
                        "UpdateInventoryItem: CallbackID: {0}, TransactionID: {1}",
                        block.CallbackID, block.TransactionID));
                }
                else
                {
                    Logger.Log("Received an UpdateInventoryItem packet for unknown inventory item " +
                        block.ItemID.ToString(), Helpers.LogLevel.Warning);
                }
            }
        }

        void FetchInventoryDescendentsHandler(Packet packet, Agent agent)
        {
            FetchInventoryDescendentsPacket fetch = (FetchInventoryDescendentsPacket)packet;
            bool sendFolders = fetch.InventoryData.FetchFolders;
            bool sendItems = fetch.InventoryData.FetchItems;
            InventorySortOrder order = (InventorySortOrder)fetch.InventoryData.SortOrder;

            Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agent.AgentID);

            // TODO: Use OwnerID
            // TODO: Do we need to obey InventorySortOrder?
            // FIXME: This packet can become huge very quickly. Add logic to break it up into multiple packets

            InventoryObject invObject;
            if (agentInventory.TryGetValue(fetch.InventoryData.FolderID, out invObject) && invObject is InventoryFolder)
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

                    Server.UDP.SendPacket(agent.AgentID, descendents, PacketCategory.Inventory);
                }
            }
            else
            {
                Logger.Log("FetchInventoryDescendents called for an unknown folder " + fetch.InventoryData.FolderID,
                    Helpers.LogLevel.Warning);
            }
        }

        void FetchInventoryHandler(Packet packet, Agent agent)
        {
            FetchInventoryPacket fetch = (FetchInventoryPacket)packet;

            FetchInventoryReplyPacket reply = new FetchInventoryReplyPacket();
            reply.AgentData.AgentID = agent.AgentID;
            reply.InventoryData = new FetchInventoryReplyPacket.InventoryDataBlock[fetch.InventoryData.Length];

            for (int i = 0; i < fetch.InventoryData.Length; i++)
            {
                UUID itemID = fetch.InventoryData[i].ItemID;
                Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agent.AgentID);

                reply.InventoryData[i] = new FetchInventoryReplyPacket.InventoryDataBlock();
                reply.InventoryData[i].ItemID = itemID;

                InventoryObject obj;
                if (agentInventory.TryGetValue(itemID, out obj) && obj is InventoryItem)
                {
                    InventoryItem item = (InventoryItem)obj;

                    reply.InventoryData[i].AssetID = item.AssetID;
                    reply.InventoryData[i].BaseMask = (uint)item.Permissions.BaseMask;
                    reply.InventoryData[i].CRC = Helpers.InventoryCRC((int)Utils.DateTimeToUnixTime(item.CreationDate),
                        (byte)item.SaleType, (sbyte)item.InventoryType, (sbyte)item.AssetType, item.AssetID, item.GroupID,
                        item.SalePrice, item.OwnerID, item.CreatorID, item.ID, item.ParentID,
                        (uint)item.Permissions.EveryoneMask, item.Flags, (uint)item.Permissions.NextOwnerMask,
                        (uint)item.Permissions.GroupMask, (uint)item.Permissions.OwnerMask);
                    reply.InventoryData[i].CreationDate = (int)Utils.DateTimeToUnixTime(item.CreationDate);
                    reply.InventoryData[i].CreatorID = item.CreatorID;
                    reply.InventoryData[i].Description = Utils.StringToBytes(item.Description);
                    reply.InventoryData[i].EveryoneMask = (uint)item.Permissions.EveryoneMask;
                    reply.InventoryData[i].Flags = item.Flags;
                    reply.InventoryData[i].FolderID = item.ParentID;
                    reply.InventoryData[i].GroupID = item.GroupID;
                    reply.InventoryData[i].GroupMask = (uint)item.Permissions.GroupMask;
                    reply.InventoryData[i].GroupOwned = item.GroupOwned;
                    reply.InventoryData[i].InvType = (sbyte)item.InventoryType;
                    reply.InventoryData[i].Name = Utils.StringToBytes(item.Name);
                    reply.InventoryData[i].NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
                    reply.InventoryData[i].OwnerID = item.OwnerID;
                    reply.InventoryData[i].OwnerMask = (uint)item.Permissions.OwnerMask;
                    reply.InventoryData[i].SalePrice = item.SalePrice;
                    reply.InventoryData[i].SaleType = (byte)item.SaleType;
                    reply.InventoryData[i].Type = (sbyte)item.AssetType;
                }
                else
                {
                    Logger.Log("FetchInventory called for an unknown item " + itemID.ToString(),
                        Helpers.LogLevel.Warning);

                    reply.InventoryData[i].Name = new byte[0];
                    reply.InventoryData[i].Description = new byte[0];
                }
            }

            Server.UDP.SendPacket(agent.AgentID, reply, PacketCategory.Inventory);
        }

        void CopyInventoryItemHandler(Packet packet, Agent agent)
        {
            CopyInventoryItemPacket copy = (CopyInventoryItemPacket)packet;

            for (int i = 0; i < copy.InventoryData.Length; i++)
            {
                CopyInventoryItemPacket.InventoryDataBlock block = copy.InventoryData[i];

                // TODO: This allows someone to copy objects from another
                // agent's inventory. Should we allow that, or do any 
                // permission checks?
                Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(block.OldAgentID);

                // Get the original object
                InventoryObject obj;
                if (agentInventory.TryGetValue(block.OldItemID, out obj) && obj is InventoryItem)
                {
                    InventoryItem item = (InventoryItem)obj;

                    // Get the new folder
                    InventoryObject folderObj;
                    if (agentInventory.TryGetValue(block.NewFolderID, out folderObj) && folderObj is InventoryFolder)
                    {
                        string newName = Utils.BytesToString(block.NewName);
                        if (String.IsNullOrEmpty(newName))
                            newName = item.Name;

                        // Create the copy
                        CreateItem(agent.AgentID, newName, item.Description, item.InventoryType, item.AssetType,
                            item.AssetID, folderObj.ID, item.Permissions.OwnerMask, item.Permissions.NextOwnerMask,
                            agent.AgentID, item.CreatorID, UUID.Zero, block.CallbackID);
                    }
                    else
                    {
                        Logger.Log("CopyInventoryItem called with an unknown target folder " + block.NewFolderID,
                            Helpers.LogLevel.Warning);
                    }
                    
                }
                else
                {
                    Logger.Log("CopyInventoryItem called for an unknown item " + block.OldItemID,
                        Helpers.LogLevel.Warning);
                }
            }
        }

        void MoveInventoryItemHandler(Packet packet, Agent agent)
        {
            MoveInventoryItemPacket move = (MoveInventoryItemPacket)packet;
            // TODO: What is move.AgentData.Stamp for?

            Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agent.AgentID);

            for (int i = 0; i < move.InventoryData.Length; i++)
            {
                MoveInventoryItemPacket.InventoryDataBlock block = move.InventoryData[i];
                UUID newFolderID = block.FolderID;
                if (newFolderID == UUID.Zero)
                    newFolderID = agent.InventoryRoot;
                MoveInventory(agentInventory, block.ItemID, newFolderID, Utils.BytesToString(block.NewName));
            }
        }

        void MoveInventoryFolderHandler(Packet packet, Agent agent)
        {
            MoveInventoryFolderPacket move = (MoveInventoryFolderPacket)packet;
            // TODO: What is move.AgentData.Stamp for?

            Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agent.AgentID);

            for (int i = 0; i < move.InventoryData.Length; i++)
            {
                MoveInventoryFolderPacket.InventoryDataBlock block = move.InventoryData[i];
                UUID newFolderID = block.ParentID;
                if (newFolderID == UUID.Zero)
                    newFolderID = agent.InventoryRoot;
                MoveInventory(agentInventory, block.FolderID, newFolderID, null);
            }
        }

        void MoveInventory(Dictionary<UUID, InventoryObject> agentInventory, UUID objectID, UUID newFolderID, string newName)
        {
            InventoryObject obj;
            if (agentInventory.TryGetValue(objectID, out obj))
            {
                lock (agentInventory)
                {
                    InventoryObject newParentObj;
                    if (agentInventory.TryGetValue(newFolderID, out newParentObj) && newParentObj is InventoryFolder)
                    {
                        // Remove this item from the current parent
                        if (obj.Parent != null)
                        {
                            InventoryFolder parent = (InventoryFolder)obj.Parent;
                            lock (parent.Children.Dictionary)
                                parent.Children.Dictionary.Remove(obj.ID);
                        }

                        // Update the new parent
                        InventoryFolder newParent = (InventoryFolder)newParentObj;
                        newParent.Children.Dictionary[obj.ID] = obj;

                        // Update the object
                        obj.ParentID = newParent.ID;
                        obj.Parent = newParent;
                        if (!String.IsNullOrEmpty(newName))
                            obj.Name = newName;
                    }
                    else
                    {
                        Logger.Log("MoveInventory called with an unknown destination folder " + newFolderID,
                            Helpers.LogLevel.Warning);
                    }
                }
            }
            else
            {
                Logger.Log("MoveInventory called for an unknown object " + objectID,
                    Helpers.LogLevel.Warning);
            }
        }

        void PurgeInventoryDescendentsHandler(Packet packet, Agent agent)
        {
            PurgeInventoryDescendentsPacket purge = (PurgeInventoryDescendentsPacket)packet;

            Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agent.AgentID);

            InventoryObject obj;
            if (agentInventory.TryGetValue(purge.InventoryData.FolderID, out obj) && obj is InventoryFolder)
            {
                lock (agentInventory)
                    PurgeFolder(agentInventory, (InventoryFolder)obj);
            }
            else
            {
                Logger.Log("PurgeInventoryDescendents called on a missing folder " + purge.InventoryData.FolderID,
                    Helpers.LogLevel.Warning);
            }
        }

        void PurgeFolder(Dictionary<UUID, InventoryObject> inventory, InventoryFolder folder)
        {
            folder.Children.ForEach(
                delegate(InventoryObject child)
                {
                    inventory.Remove(child.ID);

                    if (child is InventoryFolder)
                    {
                        InventoryFolder childFolder = (InventoryFolder)child;
                        PurgeFolder(inventory, childFolder);
                    }
                }
            );

            lock (folder.Children.Dictionary)
                folder.Children.Dictionary.Clear();
        }

        public bool CreateRootFolder(UUID agentID, UUID folderID, string name, UUID ownerID)
        {
            Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agentID);

            lock (agentInventory)
            {
                if (!agentInventory.ContainsKey(folderID))
                {
                    InventoryFolder folder = new InventoryFolder();
                    folder.Name = name;
                    folder.OwnerID = agentID;
                    folder.ParentID = UUID.Zero;
                    folder.Parent = null;
                    folder.PreferredType = AssetType.Folder;
                    folder.ID = folderID;
                    folder.Version = 1;

                    Logger.DebugLog("Creating root inventory folder " + folder.Name);

                    agentInventory[folder.ID] = folder;
                    return true;
                }
                else
                {
                    Logger.Log(String.Format(
                        "Cannot create root inventory folder, item {0} already exists", folderID),
                        Helpers.LogLevel.Warning);
                }
            }

            return false;
        }

        public bool CreateFolder(UUID agentID, UUID folderID, string name, AssetType preferredType,
            UUID parentID, UUID ownerID)
        {
            Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agentID);

            lock (agentInventory)
            {
                InventoryObject parent;
                if (agentInventory.TryGetValue(parentID, out parent) && parent is InventoryFolder)
                {
                    InventoryFolder parentFolder = (InventoryFolder)parent;

                    if (!agentInventory.ContainsKey(folderID))
                    {
                        InventoryFolder folder = new InventoryFolder();
                        folder.Name = name;
                        folder.OwnerID = agentID;
                        folder.ParentID = parentID;
                        folder.Parent = parentFolder;
                        folder.PreferredType = preferredType;
                        folder.ID = folderID;
                        folder.Version = 1;

                        Logger.DebugLog("Creating inventory folder " + folder.Name);

                        agentInventory[folder.ID] = folder;
                        lock (parentFolder.Children.Dictionary)
                            parentFolder.Children.Dictionary[folder.ID] = folder;

                        return true;
                    }
                    else
                    {
                        Logger.Log(String.Format(
                            "Cannot create new inventory folder, item {0} already exists", folderID),
                            Helpers.LogLevel.Warning);
                    }
                }
                else
                {
                    Logger.Log(String.Format(
                        "Cannot create new inventory folder, parent folder {0} does not exist", parentID),
                        Helpers.LogLevel.Warning);
                }
            }

            return false;
        }

        public UUID CreateItem(UUID agentID, string name, string description, InventoryType invType, AssetType type,
            UUID assetID, UUID parentID, PermissionMask ownerMask, PermissionMask nextOwnerMask, UUID ownerID,
            UUID creatorID, UUID transactionID, uint callbackID)
        {
            Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agentID);

            lock (agentInventory)
            {
                InventoryObject parent;
                if (agentInventory.TryGetValue(parentID, out parent) && parent is InventoryFolder)
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
                    item.Permissions.BaseMask = PermissionMask.All;
                    item.Permissions.EveryoneMask = PermissionMask.All;
                    item.Permissions.GroupMask = PermissionMask.All;
                    //item.Permissions.OwnerMask = ownerMask;
                    //item.Permissions.NextOwnerMask = nextOwnerMask;
                    item.Permissions.OwnerMask = PermissionMask.All;
                    item.Permissions.NextOwnerMask = PermissionMask.All;
                    item.AssetType = type;
                    item.AssetID = assetID;
                    item.OwnerID = agentID;
                    item.CreatorID = agentID;
                    item.CreationDate = DateTime.Now;

                    Logger.DebugLog(String.Format("Creating inventory item {0} (InvType: {1}, AssetType: {2})", item.Name,
                        item.InventoryType, item.AssetType));

                    // Store the inventory item
                    agentInventory[item.ID] = item;
                    lock (parentFolder.Children.Dictionary)
                        parentFolder.Children.Dictionary[item.ID] = item;

                    // Send a success response
                    UpdateCreateInventoryItemPacket update = new UpdateCreateInventoryItemPacket();
                    update.AgentData.AgentID = agentID;
                    update.AgentData.SimApproved = true;
                    if (transactionID != UUID.Zero)
                        update.AgentData.TransactionID = transactionID;
                    else
                        update.AgentData.TransactionID = UUID.Random();
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

                    Server.UDP.SendPacket(agentID, update, PacketCategory.Inventory);
                    return item.ID;
                }
                else
                {
                    Logger.Log(String.Format(
                        "Cannot create new inventory item, folder {0} does not exist",
                        parentID), Helpers.LogLevel.Warning);

                    return UUID.Zero;
                }
            }
        }

        public bool TryGetInventory(UUID agentID, UUID objectID, out InventoryObject obj)
        {
            Dictionary<UUID, InventoryObject> inventory;

            if (Inventory.TryGetValue(agentID, out inventory))
            {
                if (inventory.TryGetValue(objectID, out obj))
                    return true;
            }

            obj = null;
            return false;
        }

        public OpenMetaverse.InventoryFolder[] CreateInventorySkeleton(UUID agentID)
        {
            Dictionary<UUID, InventoryObject> inventory;
            if (Inventory.TryGetValue(agentID, out inventory))
            {
                List<InventoryFolder> folderList = new List<InventoryFolder>();

                lock (inventory)
                {
                    foreach (InventoryObject obj in inventory.Values)
                    {
                        if (obj is InventoryFolder)
                            folderList.Add((InventoryFolder)obj);
                    }
                }

                OpenMetaverse.InventoryFolder[] folders = new OpenMetaverse.InventoryFolder[folderList.Count];

                for (int i = 0; i < folderList.Count; i++)
                {
                    InventoryFolder folder = folderList[i];

                    folders[i] = new OpenMetaverse.InventoryFolder(folder.ID);
                    folders[i].DescendentCount = folder.Children.Count;
                    folders[i].Name = folder.Name;
                    folders[i].OwnerID = folder.OwnerID;
                    folders[i].ParentUUID = folder.ParentID;
                    folders[i].PreferredType = folder.PreferredType;
                    folders[i].Version = folder.Version;
                }

                return folders;
            }
            else
            {
                Logger.Log("CreateInventorySkeleton() called with an unknown agent " + agentID.ToString(),
                    Helpers.LogLevel.Warning);
                return null;
            }
        }

        #region Persistence

        LLSDMap SerializeItem(InventoryItem item)
        {
            LLSDMap itemMap = new LLSDMap(16);
            itemMap.Add("ID", LLSD.FromUUID(item.ID));
            itemMap.Add("ParentID", LLSD.FromUUID(item.ParentID));
            itemMap.Add("Name", LLSD.FromString(item.Name));
            itemMap.Add("OwnerID", LLSD.FromUUID(item.OwnerID));
            itemMap.Add("AssetID", LLSD.FromUUID(item.AssetID));
            itemMap.Add("AssetType", LLSD.FromInteger((sbyte)item.AssetType));
            itemMap.Add("InventoryType", LLSD.FromInteger((sbyte)item.InventoryType));
            itemMap.Add("CreatorID", LLSD.FromUUID(item.CreatorID));
            itemMap.Add("GroupID", LLSD.FromUUID(item.GroupID));
            itemMap.Add("Description", LLSD.FromString(item.Description));
            itemMap.Add("GroupOwned", LLSD.FromBoolean(item.GroupOwned));
            itemMap.Add("Permissions", item.Permissions.GetLLSD());
            itemMap.Add("SalePrice", LLSD.FromInteger(item.SalePrice));
            itemMap.Add("SaleType", LLSD.FromInteger((byte)item.SaleType));
            itemMap.Add("Flags", LLSD.FromUInteger((uint)item.Flags));
            itemMap.Add("CreationDate", LLSD.FromDate(item.CreationDate));
            return itemMap;
        }

        LLSDMap SerializeFolder(InventoryFolder folder)
        {
            LLSDMap folderMap = new LLSDMap(6);
            folderMap.Add("ID", LLSD.FromUUID(folder.ID));
            folderMap.Add("ParentID", LLSD.FromUUID(folder.ParentID));
            folderMap.Add("Name", LLSD.FromString(folder.Name));
            folderMap.Add("OwnerID", LLSD.FromUUID(folder.OwnerID));
            folderMap.Add("PreferredType", LLSD.FromInteger((sbyte)folder.PreferredType));
            folderMap.Add("Version", LLSD.FromInteger(folder.Version));
            return folderMap;
        }

        LLSDMap SerializeInventory(Dictionary<UUID, InventoryObject> agentInventory)
        {
            LLSDMap map = new LLSDMap(agentInventory.Count);

            foreach (KeyValuePair<UUID, InventoryObject> kvp in agentInventory)
            {
                LLSD value;
                if (kvp.Value is InventoryItem)
                    value = SerializeItem((InventoryItem)kvp.Value);
                else
                    value = SerializeFolder((InventoryFolder)kvp.Value);

                map.Add(kvp.Key.ToString(), value);
            }

            return map;
        }

        public LLSD Serialize()
        {
            LLSDMap map = new LLSDMap(Inventory.Count);
            int itemCount = 0;

            lock (Inventory)
            {
                foreach (KeyValuePair<UUID, Dictionary<UUID, InventoryObject>> kvp in Inventory)
                {
                    map.Add(kvp.Key.ToString(), SerializeInventory(kvp.Value));
                    itemCount += kvp.Value.Count;
                }
            }

            Logger.Log(String.Format("Serializing the inventory store with {0} items", itemCount),
                Helpers.LogLevel.Info);

            return map;
        }

        InventoryItem DeserializeItem(LLSDMap itemMap)
        {
            InventoryItem item = new InventoryItem();
            item.ID = itemMap["ID"].AsUUID();
            item.ParentID = itemMap["ParentID"].AsUUID();
            item.Name = itemMap["Name"].AsString();
            item.OwnerID = itemMap["OwnerID"].AsUUID();
            item.AssetID = itemMap["AssetID"].AsUUID();
            item.AssetType = (AssetType)itemMap["AssetType"].AsInteger();
            item.InventoryType = (InventoryType)itemMap["InventoryType"].AsInteger();
            item.CreatorID = itemMap["CreatorID"].AsUUID();
            item.GroupID = itemMap["GroupID"].AsUUID();
            item.Description = itemMap["Description"].AsString();
            item.GroupOwned = itemMap["GroupOwned"].AsBoolean();
            item.Permissions = Permissions.FromLLSD(itemMap["Permissions"]);
            item.SalePrice = itemMap["SalePrice"].AsInteger();
            item.SaleType = (SaleType)itemMap["SaleType"].AsInteger();
            item.Flags = Utils.BytesToUInt(itemMap["Flags"].AsBinary());
            item.CreationDate = itemMap["CreationDate"].AsDate();
            return item;
        }

        InventoryFolder DeserializeFolder(LLSDMap folderMap)
        {
            InventoryFolder folder = new InventoryFolder();
            folder.ID = folderMap["ID"].AsUUID();
            folder.ParentID = folderMap["ParentID"].AsUUID();
            folder.Name = folderMap["Name"].AsString();
            folder.OwnerID = folderMap["OwnerID"].AsUUID();
            folder.PreferredType = (AssetType)folderMap["PreferredType"].AsInteger();
            folder.Version = folderMap["Version"].AsInteger();
            return folder;
        }

        Dictionary<UUID, InventoryObject> DeserializeInventory(LLSDMap map)
        {
            Dictionary<UUID, InventoryObject> inventory = new Dictionary<UUID, InventoryObject>();

            foreach (KeyValuePair<string, LLSD> kvp in map)
            {
                UUID objectID = (UUID)kvp.Key;
                LLSDMap objectMap = (LLSDMap)kvp.Value;
                InventoryObject obj;

                if (objectMap.ContainsKey("AssetID"))
                    obj = DeserializeItem(objectMap);
                else
                    obj = DeserializeFolder(objectMap);

                inventory[objectID] = obj;
            }

            return inventory;
        }

        public void Deserialize(LLSD serialized)
        {
            int itemCount = 0;
            LLSDMap map = (LLSDMap)serialized;

            lock (Inventory)
            {
                Inventory.Clear();

                foreach (KeyValuePair<string, LLSD> kvp in map)
                {
                    UUID agentID = (UUID)kvp.Key;
                    Dictionary<UUID, InventoryObject> agentInventory = DeserializeInventory((LLSDMap)kvp.Value);
                    itemCount += agentInventory.Count;

                    Inventory[agentID] = agentInventory;
                }

                // Iterate over the inventory objects and connect them to each other
                foreach (Dictionary<UUID, InventoryObject> inventory in Inventory.Values)
                {
                    foreach (InventoryObject obj in inventory.Values)
                    {
                        if (obj.ParentID != UUID.Zero)
                        {
                            InventoryObject parentObj;
                            if (inventory.TryGetValue(obj.ParentID, out parentObj) && parentObj is InventoryFolder)
                            {
                                InventoryFolder parent = (InventoryFolder)parentObj;
                                obj.Parent = parent;
                                parent.Children.Dictionary[obj.ID] = obj;
                            }
                            else
                            {
                                Logger.Log(String.Format("Cannot find parent folder {0} for inventory item {1}",
                                    obj.ParentID, obj.ID), Helpers.LogLevel.Warning);
                            }
                        }
                    }
                }
            }

            Logger.Log(String.Format("Deserialized the inventory store with {0} items", itemCount),
                Helpers.LogLevel.Info);
        }

        #endregion Persistence
    }
}
