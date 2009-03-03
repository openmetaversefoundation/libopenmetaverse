using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    public class InventoryManager : IExtension<Simian>, IInventoryProvider, IPersistable
    {
        Simian server;
        /// <summary>Dictionary of inventories for each agent. Each inventory
        /// is also a dictionary itself</summary>
        Dictionary<UUID, Dictionary<UUID, InventoryObject>> Inventory =
            new Dictionary<UUID, Dictionary<UUID, InventoryObject>>();
        /// <summary>Global shared inventory for all agent</summary>
        Dictionary<UUID, InventoryObject> Library = new Dictionary<UUID, InventoryObject>();

        public InventoryManager()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;

            server.UDP.RegisterPacketCallback(PacketType.CreateInventoryItem, new PacketCallback(CreateInventoryItemHandler));
            server.UDP.RegisterPacketCallback(PacketType.CreateInventoryFolder, new PacketCallback(CreateInventoryFolderHandler));
            server.UDP.RegisterPacketCallback(PacketType.UpdateInventoryItem, new PacketCallback(UpdateInventoryItemHandler));
            server.UDP.RegisterPacketCallback(PacketType.FetchInventoryDescendents, new PacketCallback(FetchInventoryDescendentsHandler));
            server.UDP.RegisterPacketCallback(PacketType.FetchInventory, new PacketCallback(FetchInventoryHandler));
            server.UDP.RegisterPacketCallback(PacketType.CopyInventoryItem, new PacketCallback(CopyInventoryItemHandler));
            server.UDP.RegisterPacketCallback(PacketType.MoveInventoryItem, new PacketCallback(MoveInventoryItemHandler));
            server.UDP.RegisterPacketCallback(PacketType.MoveInventoryFolder, new PacketCallback(MoveInventoryFolderHandler));
            server.UDP.RegisterPacketCallback(PacketType.PurgeInventoryDescendents, new PacketCallback(PurgeInventoryDescendentsHandler));
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
            CreateItem(agent.ID, Utils.BytesToString(create.InventoryBlock.Name), "Created in Simian",
                (InventoryType)create.InventoryBlock.InvType, assetType, assetID, parentID,
                PermissionMask.All, (PermissionMask)create.InventoryBlock.NextOwnerMask, agent.ID,
                agent.ID, create.InventoryBlock.TransactionID, create.InventoryBlock.CallbackID, true);
        }

        void CreateInventoryFolderHandler(Packet packet, Agent agent)
        {
            CreateInventoryFolderPacket create = (CreateInventoryFolderPacket)packet;

            UUID folderID = create.FolderData.FolderID;
            if (folderID == UUID.Zero)
                folderID = agent.InventoryRoot;

            CreateFolder(agent.ID, folderID, Utils.BytesToString(create.FolderData.Name),
                (AssetType)create.FolderData.Type, create.FolderData.ParentID, agent.ID);
        }

        void UpdateInventoryItemHandler(Packet packet, Agent agent)
        {
            UpdateInventoryItemPacket update = (UpdateInventoryItemPacket)packet;

            // No packet is sent back to the client, we just need to update the
            // inventory item locally
            for (int i = 0; i < update.InventoryData.Length; i++)
            {
                UpdateInventoryItemPacket.InventoryDataBlock block = update.InventoryData[i];
                Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agent.ID);

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
                    item.AssetID = UUID.Combine(block.TransactionID, agent.SecureSessionID);

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
            // A very safe estimate of the fixed minimum packet size
            const int PACKET_OVERHEAD = 96;

            FetchInventoryDescendentsPacket fetch = (FetchInventoryDescendentsPacket)packet;
            bool sendFolders = fetch.InventoryData.FetchFolders;
            bool sendItems = fetch.InventoryData.FetchItems;
            // TODO: Obey SortOrder
            //InventorySortOrder order = (InventorySortOrder)fetch.InventoryData.SortOrder;

            Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agent.ID);

            // TODO: Use OwnerID
            // TODO: Do we need to obey InventorySortOrder?
            InventoryObject invObject;
            if (agentInventory.TryGetValue(fetch.InventoryData.FolderID, out invObject) && invObject is InventoryFolder)
            {
                InventoryFolder folder = (InventoryFolder)invObject;

                int descendCount;
                int version;

                List<InventoryItem> items = new List<InventoryItem>();
                List<InventoryFolder> folders = new List<InventoryFolder>();
                InventoryDescendentsPacket.FolderDataBlock[] folderBlocks;
                InventoryDescendentsPacket.ItemDataBlock[] itemBlocks;

                lock (folder.Children.Dictionary)
                {
                    // These two are coupled to the actual items in the dictionary,
                    // so they are set inside the lock
                    descendCount = folder.Children.Count;
                    version = folder.Version;

                    if (sendItems || sendFolders)
                    {
                        // Create a list of all of the folders and items under this folder
                        folder.Children.ForEach(
                            delegate(InventoryObject obj)
                            {
                                if (obj is InventoryItem)
                                    items.Add((InventoryItem)obj);
                                else
                                    folders.Add((InventoryFolder)obj);
                            }
                        );
                    }
                }

                if (sendFolders)
                {
                    folderBlocks = new InventoryDescendentsPacket.FolderDataBlock[folders.Count];
                    for (int i = 0; i < folders.Count; i++)
                    {
                        InventoryFolder currentFolder = folders[i];

                        folderBlocks[i] = new InventoryDescendentsPacket.FolderDataBlock();
                        folderBlocks[i].FolderID = currentFolder.ID;
                        folderBlocks[i].Name = Utils.StringToBytes(currentFolder.Name);
                        folderBlocks[i].ParentID = currentFolder.ParentID;
                        folderBlocks[i].Type = (sbyte)currentFolder.PreferredType;
                    }
                }
                else
                {
                    folderBlocks = new InventoryDescendentsPacket.FolderDataBlock[0];
                }

                if (sendItems)
                {
                    itemBlocks = new InventoryDescendentsPacket.ItemDataBlock[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        InventoryItem currentItem = items[i];

                        itemBlocks[i] = new InventoryDescendentsPacket.ItemDataBlock();
                        itemBlocks[i].AssetID = currentItem.AssetID;
                        itemBlocks[i].BaseMask = (uint)currentItem.Permissions.BaseMask;
                        itemBlocks[i].CRC = currentItem.CRC;
                        itemBlocks[i].CreationDate = (int)Utils.DateTimeToUnixTime(currentItem.CreationDate);
                        itemBlocks[i].CreatorID = currentItem.CreatorID;
                        itemBlocks[i].Description = Utils.StringToBytes(currentItem.Description);
                        itemBlocks[i].EveryoneMask = (uint)currentItem.Permissions.EveryoneMask;
                        itemBlocks[i].Flags = currentItem.Flags;
                        itemBlocks[i].FolderID = currentItem.ParentID;
                        itemBlocks[i].GroupID = currentItem.GroupID;
                        itemBlocks[i].GroupMask = (uint)currentItem.Permissions.GroupMask;
                        itemBlocks[i].GroupOwned = currentItem.GroupOwned;
                        itemBlocks[i].InvType = (sbyte)currentItem.InventoryType;
                        itemBlocks[i].ItemID = currentItem.ID;
                        itemBlocks[i].Name = Utils.StringToBytes(currentItem.Name);
                        itemBlocks[i].NextOwnerMask = (uint)currentItem.Permissions.NextOwnerMask;
                        itemBlocks[i].OwnerID = currentItem.OwnerID;
                        itemBlocks[i].OwnerMask = (uint)currentItem.Permissions.OwnerMask;
                        itemBlocks[i].SalePrice = currentItem.SalePrice;
                        itemBlocks[i].SaleType = (byte)currentItem.SaleType;
                        itemBlocks[i].Type = (sbyte)currentItem.AssetType;
                    }
                }
                else
                {
                    itemBlocks = new InventoryDescendentsPacket.ItemDataBlock[0];
                }

                // FolderDataBlock and ItemDataBlock are both variable and possibly very large,
                // so we handle the splitting separately. This could be replaced by some custom
                // splitting
                if (folderBlocks.Length > 0)
                {
                    List<int> splitPoints = Helpers.SplitBlocks(folderBlocks, PACKET_OVERHEAD);
                    Logger.DebugLog(String.Format("Sending {0} InventoryDescendents packets containing {1} folders",
                        splitPoints.Count, folderBlocks.Length));

                    for (int i = 0; i < splitPoints.Count; i++)
                    {
                        int count = (i != splitPoints.Count - 1) ? splitPoints[i + 1] - splitPoints[i] :
                            folderBlocks.Length - splitPoints[i];

                        InventoryDescendentsPacket descendents = new InventoryDescendentsPacket();
                        descendents.AgentData.AgentID = agent.ID;
                        descendents.AgentData.FolderID = folder.ID;
                        descendents.AgentData.OwnerID = folder.OwnerID;
                        descendents.AgentData.Descendents = descendCount;
                        descendents.AgentData.Version = version;
                        descendents.FolderData = new InventoryDescendentsPacket.FolderDataBlock[count];
                        descendents.ItemData = new InventoryDescendentsPacket.ItemDataBlock[0];

                        for (int j = 0; j < count; j++)
                            descendents.FolderData[j] = folderBlocks[splitPoints[i] + j];

                        server.UDP.SendPacket(agent.ID, descendents, PacketCategory.Inventory);
                    }
                }
                else
                {
                    Logger.DebugLog("Sending a single InventoryDescendents for folders");

                    InventoryDescendentsPacket descendents = new InventoryDescendentsPacket();
                    descendents.AgentData.AgentID = agent.ID;
                    descendents.AgentData.FolderID = folder.ID;
                    descendents.AgentData.OwnerID = folder.OwnerID;
                    descendents.AgentData.Descendents = descendCount;
                    descendents.AgentData.Version = version;
                    descendents.FolderData = new InventoryDescendentsPacket.FolderDataBlock[0];
                    descendents.ItemData = new InventoryDescendentsPacket.ItemDataBlock[0];

                    server.UDP.SendPacket(agent.ID, descendents, PacketCategory.Inventory);
                }

                if (itemBlocks.Length > 0)
                {
                    List<int> splitPoints = Helpers.SplitBlocks(itemBlocks, PACKET_OVERHEAD);
                    Logger.DebugLog(String.Format("Sending {0} InventoryDescendents packets containing {1} items",
                        splitPoints.Count, itemBlocks.Length));

                    for (int i = 0; i < splitPoints.Count; i++)
                    {
                        int count = (i != splitPoints.Count - 1) ? splitPoints[i + 1] - splitPoints[i] :
                            itemBlocks.Length - splitPoints[i];

                        InventoryDescendentsPacket descendents = new InventoryDescendentsPacket();
                        descendents.AgentData.AgentID = agent.ID;
                        descendents.AgentData.FolderID = folder.ID;
                        descendents.AgentData.OwnerID = folder.OwnerID;
                        descendents.AgentData.Descendents = descendCount;
                        descendents.AgentData.Version = version;
                        descendents.FolderData = new InventoryDescendentsPacket.FolderDataBlock[0];
                        descendents.ItemData = new InventoryDescendentsPacket.ItemDataBlock[count];

                        for (int j = 0; j < count; j++)
                            descendents.ItemData[j] = itemBlocks[splitPoints[i] + j];

                        server.UDP.SendPacket(agent.ID, descendents, PacketCategory.Inventory);
                    }
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
            // This is probably too large, but better to be on the safe side
            const int PACKET_OVERHEAD = 32;

            FetchInventoryPacket fetch = (FetchInventoryPacket)packet;
            
            // Create all of the blocks first. These will be split up into different packets
            FetchInventoryReplyPacket.InventoryDataBlock[] blocks =
                new FetchInventoryReplyPacket.InventoryDataBlock[fetch.InventoryData.Length];

            for (int i = 0; i < fetch.InventoryData.Length; i++)
            {
                UUID itemID = fetch.InventoryData[i].ItemID;
                Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agent.ID);

                blocks[i] = new FetchInventoryReplyPacket.InventoryDataBlock();
                blocks[i].ItemID = itemID;

                InventoryObject obj;
                if (agentInventory.TryGetValue(itemID, out obj) && obj is InventoryItem)
                {
                    InventoryItem item = (InventoryItem)obj;

                    blocks[i].AssetID = item.AssetID;
                    blocks[i].BaseMask = (uint)item.Permissions.BaseMask;
                    blocks[i].CRC = item.CRC;
                    blocks[i].CreationDate = (int)Utils.DateTimeToUnixTime(item.CreationDate);
                    blocks[i].CreatorID = item.CreatorID;
                    blocks[i].Description = Utils.StringToBytes(item.Description);
                    blocks[i].EveryoneMask = (uint)item.Permissions.EveryoneMask;
                    blocks[i].Flags = item.Flags;
                    blocks[i].FolderID = item.ParentID;
                    blocks[i].GroupID = item.GroupID;
                    blocks[i].GroupMask = (uint)item.Permissions.GroupMask;
                    blocks[i].GroupOwned = item.GroupOwned;
                    blocks[i].InvType = (sbyte)item.InventoryType;
                    blocks[i].Name = Utils.StringToBytes(item.Name);
                    blocks[i].NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
                    blocks[i].OwnerID = item.OwnerID;
                    blocks[i].OwnerMask = (uint)item.Permissions.OwnerMask;
                    blocks[i].SalePrice = item.SalePrice;
                    blocks[i].SaleType = (byte)item.SaleType;
                    blocks[i].Type = (sbyte)item.AssetType;
                }
                else
                {
                    Logger.Log("FetchInventory called for an unknown item " + itemID.ToString(),
                        Helpers.LogLevel.Warning);

                    blocks[i].Name = new byte[0];
                    blocks[i].Description = new byte[0];
                }
            }

            // Split the blocks up into multiple packets
            List<int> splitPoints = Helpers.SplitBlocks(blocks, PACKET_OVERHEAD);
            for (int i = 0; i < splitPoints.Count; i++)
            {
                int count = (i != splitPoints.Count - 1) ? splitPoints[i + 1] - splitPoints[i] :
                    blocks.Length - splitPoints[i];

                FetchInventoryReplyPacket reply = new FetchInventoryReplyPacket();
                reply.AgentData.AgentID = agent.ID;
                reply.InventoryData = new FetchInventoryReplyPacket.InventoryDataBlock[count];

                for (int j = 0; j < count; j++)
                    reply.InventoryData[j] = blocks[splitPoints[i] + j];

                server.UDP.SendPacket(agent.ID, reply, PacketCategory.Inventory);
            }
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
                        CreateItem(agent.ID, newName, item.Description, item.InventoryType, item.AssetType,
                            item.AssetID, folderObj.ID, item.Permissions.OwnerMask, item.Permissions.NextOwnerMask,
                            agent.ID, item.CreatorID, UUID.Zero, block.CallbackID, true);
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

            Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agent.ID);

            for (int i = 0; i < move.InventoryData.Length; i++)
            {
                MoveInventoryItemPacket.InventoryDataBlock block = move.InventoryData[i];
                UUID newFolderID = block.FolderID;
                if (newFolderID == UUID.Zero)
                    newFolderID = agent.InventoryRoot;
                MoveInventory(agent, agentInventory, block.ItemID, newFolderID, Utils.BytesToString(block.NewName),
                    UUID.Zero, 0);
            }
        }

        void MoveInventoryFolderHandler(Packet packet, Agent agent)
        {
            MoveInventoryFolderPacket move = (MoveInventoryFolderPacket)packet;
            // TODO: What is move.AgentData.Stamp for?

            Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agent.ID);

            for (int i = 0; i < move.InventoryData.Length; i++)
            {
                MoveInventoryFolderPacket.InventoryDataBlock block = move.InventoryData[i];
                UUID newFolderID = block.ParentID;
                if (newFolderID == UUID.Zero)
                    newFolderID = agent.InventoryRoot;
                MoveInventory(agent, agentInventory, block.FolderID, newFolderID, null, UUID.Zero, 0);
            }
        }

        void SendBulkUpdate(Agent agent, InventoryObject obj, UUID transactionID, uint callbackID)
        {
            BulkUpdateInventoryPacket update = new BulkUpdateInventoryPacket();
            update.AgentData.AgentID = agent.ID;
            update.AgentData.TransactionID = transactionID;

            if (obj is InventoryItem)
            {
                InventoryItem item = (InventoryItem)obj;

                update.FolderData = new BulkUpdateInventoryPacket.FolderDataBlock[0];
                update.ItemData = new BulkUpdateInventoryPacket.ItemDataBlock[1];
                update.ItemData[0] = new BulkUpdateInventoryPacket.ItemDataBlock();
                update.ItemData[0].AssetID = item.AssetID;
                update.ItemData[0].BaseMask = (uint)item.Permissions.BaseMask;
                update.ItemData[0].CallbackID = callbackID;
                update.ItemData[0].CRC = item.CRC;
                update.ItemData[0].CreationDate = (int)Utils.DateTimeToUnixTime(item.CreationDate);
                update.ItemData[0].CreatorID = item.CreatorID;
                update.ItemData[0].Description = Utils.StringToBytes(item.Description);
                update.ItemData[0].EveryoneMask = (uint)item.Permissions.EveryoneMask;
                update.ItemData[0].Flags = item.Flags;
                update.ItemData[0].FolderID = item.ParentID;
                update.ItemData[0].GroupID = item.GroupID;
                update.ItemData[0].GroupMask = (uint)item.Permissions.GroupMask;
                update.ItemData[0].GroupOwned = item.GroupOwned;
                update.ItemData[0].InvType = (sbyte)item.InventoryType;
                update.ItemData[0].ItemID = item.ID;
                update.ItemData[0].Name = Utils.StringToBytes(item.Name);
                update.ItemData[0].NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
                update.ItemData[0].OwnerID = item.OwnerID;
                update.ItemData[0].OwnerMask = (uint)item.Permissions.OwnerMask;
                update.ItemData[0].SalePrice = item.SalePrice;
                update.ItemData[0].SaleType = (byte)item.SaleType;
                update.ItemData[0].Type = (sbyte)item.InventoryType;
            }
            else
            {
                InventoryFolder folder = (InventoryFolder)obj;

                update.ItemData = new BulkUpdateInventoryPacket.ItemDataBlock[0];
                update.FolderData = new BulkUpdateInventoryPacket.FolderDataBlock[1];
                update.FolderData[0] = new BulkUpdateInventoryPacket.FolderDataBlock();
                update.FolderData[0].FolderID = folder.ID;
                update.FolderData[0].Name = Utils.StringToBytes(folder.Name);
                update.FolderData[0].ParentID = folder.ParentID;
                update.FolderData[0].Type = (sbyte)folder.PreferredType;
            }

            Logger.DebugLog("Sending bulk update for inventory object " + obj.ID);

            server.UDP.SendPacket(agent.ID, update, PacketCategory.Inventory);
        }

        void MoveInventory(Agent agent, Dictionary<UUID, InventoryObject> agentInventory, UUID objectID,
            UUID newFolderID, string newName, UUID transactionID, uint callbackID)
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

                    SendBulkUpdate(agent, obj, transactionID, callbackID);
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

            Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agent.ID);

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
            UUID creatorID, UUID transactionID, uint callbackID, bool sendPacket)
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
                    update.InventoryData[0].CRC = item.CRC;
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

                    if (sendPacket)
                        server.UDP.SendPacket(agentID, update, PacketCategory.Inventory);

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

        OSDMap SerializeItem(InventoryItem item)
        {
            OSDMap itemMap = new OSDMap(16);
            itemMap.Add("ID", OSD.FromUUID(item.ID));
            itemMap.Add("ParentID", OSD.FromUUID(item.ParentID));
            itemMap.Add("Name", OSD.FromString(item.Name));
            itemMap.Add("OwnerID", OSD.FromUUID(item.OwnerID));
            itemMap.Add("AssetID", OSD.FromUUID(item.AssetID));
            itemMap.Add("AssetType", OSD.FromInteger((sbyte)item.AssetType));
            itemMap.Add("InventoryType", OSD.FromInteger((sbyte)item.InventoryType));
            itemMap.Add("CreatorID", OSD.FromUUID(item.CreatorID));
            itemMap.Add("GroupID", OSD.FromUUID(item.GroupID));
            itemMap.Add("Description", OSD.FromString(item.Description));
            itemMap.Add("GroupOwned", OSD.FromBoolean(item.GroupOwned));
            itemMap.Add("Permissions", item.Permissions.GetOSD());
            itemMap.Add("SalePrice", OSD.FromInteger(item.SalePrice));
            itemMap.Add("SaleType", OSD.FromInteger((byte)item.SaleType));
            itemMap.Add("Flags", OSD.FromUInteger((uint)item.Flags));
            itemMap.Add("CreationDate", OSD.FromDate(item.CreationDate));
            return itemMap;
        }

        OSDMap SerializeFolder(InventoryFolder folder)
        {
            OSDMap folderMap = new OSDMap(6);
            folderMap.Add("ID", OSD.FromUUID(folder.ID));
            folderMap.Add("ParentID", OSD.FromUUID(folder.ParentID));
            folderMap.Add("Name", OSD.FromString(folder.Name));
            folderMap.Add("OwnerID", OSD.FromUUID(folder.OwnerID));
            folderMap.Add("PreferredType", OSD.FromInteger((sbyte)folder.PreferredType));
            folderMap.Add("Version", OSD.FromInteger(folder.Version));
            return folderMap;
        }

        OSDMap SerializeInventory(Dictionary<UUID, InventoryObject> agentInventory)
        {
            OSDMap map = new OSDMap(agentInventory.Count);

            foreach (KeyValuePair<UUID, InventoryObject> kvp in agentInventory)
            {
                OSD value;
                if (kvp.Value is InventoryItem)
                    value = SerializeItem((InventoryItem)kvp.Value);
                else
                    value = SerializeFolder((InventoryFolder)kvp.Value);

                map.Add(kvp.Key.ToString(), value);
            }

            return map;
        }

        public OSD Serialize()
        {
            OSDMap map = new OSDMap(Inventory.Count);
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

        InventoryItem DeserializeItem(OSDMap itemMap)
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
            item.Permissions = Permissions.FromOSD(itemMap["Permissions"]);
            item.SalePrice = itemMap["SalePrice"].AsInteger();
            item.SaleType = (SaleType)itemMap["SaleType"].AsInteger();
            item.Flags = Utils.BytesToUInt(itemMap["Flags"].AsBinary());
            item.CreationDate = itemMap["CreationDate"].AsDate();
            return item;
        }

        InventoryFolder DeserializeFolder(OSDMap folderMap)
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

        Dictionary<UUID, InventoryObject> DeserializeInventory(OSDMap map)
        {
            Dictionary<UUID, InventoryObject> inventory = new Dictionary<UUID, InventoryObject>();

            foreach (KeyValuePair<string, OSD> kvp in map)
            {
                UUID objectID = (UUID)kvp.Key;
                OSDMap objectMap = (OSDMap)kvp.Value;
                InventoryObject obj;

                if (objectMap.ContainsKey("AssetID"))
                    obj = DeserializeItem(objectMap);
                else
                    obj = DeserializeFolder(objectMap);

                inventory[objectID] = obj;
            }

            return inventory;
        }

        public void Deserialize(OSD serialized)
        {
            int itemCount = 0;
            OSDMap map = (OSDMap)serialized;

            lock (Inventory)
            {
                Inventory.Clear();

                foreach (KeyValuePair<string, OSD> kvp in map)
                {
                    UUID agentID = (UUID)kvp.Key;
                    Dictionary<UUID, InventoryObject> agentInventory = DeserializeInventory((OSDMap)kvp.Value);
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
