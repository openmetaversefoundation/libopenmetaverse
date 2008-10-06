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

            // Create the inventory item
            CreateItem(agent, Utils.BytesToString(create.InventoryBlock.Name), "Created in Simian",
                (InventoryType)create.InventoryBlock.InvType, assetType, assetID, parentID,
                PermissionMask.All, (PermissionMask)create.InventoryBlock.NextOwnerMask, agent.AgentID,
                agent.AgentID, create.InventoryBlock.TransactionID, create.InventoryBlock.CallbackID);
        }

        void CreateInventoryFolderHandler(Packet packet, Agent agent)
        {
            CreateInventoryFolderPacket create = (CreateInventoryFolderPacket)packet;

            CreateFolder(agent, create.FolderData.FolderID, Utils.BytesToString(create.FolderData.Name),
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

                    //block.CRC;
                    item.Permissions.BaseMask = (PermissionMask)block.BaseMask;
                    item.CreationDate = Utils.UnixTimeToDateTime(block.CreationDate);
                    item.CreatorID = block.CreatorID;
                    item.Name = Utils.BytesToString(block.Description);
                    item.Permissions.EveryoneMask = (PermissionMask)block.EveryoneMask;
                    item.Flags = block.Flags;
                    item.ParentID = block.FolderID;
                    item.GroupID = block.GroupID;
                    item.Permissions.GroupMask = (PermissionMask)block.GroupMask;
                    item.GroupOwned = block.GroupOwned;
                    item.InventoryType = (InventoryType)block.InvType;
                    item.Name = Utils.BytesToString(block.Name);
                    item.Permissions.NextOwnerMask = (PermissionMask)block.NextOwnerMask;
                    item.OwnerID = block.OwnerID;
                    item.Permissions.OwnerMask = (PermissionMask)block.OwnerMask;
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

        public bool CreateRootFolder(Agent agent, UUID folderID, string name, UUID ownerID)
        {
            Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agent.AgentID);

            lock (agentInventory)
            {
                if (!agentInventory.ContainsKey(folderID))
                {
                    InventoryFolder folder = new InventoryFolder();
                    folder.Name = name;
                    folder.OwnerID = agent.AgentID;
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

        public bool CreateFolder(Agent agent, UUID folderID, string name, AssetType preferredType,
            UUID parentID, UUID ownerID)
        {
            if (parentID == UUID.Zero)
                parentID = agent.InventoryRoot;

            Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agent.AgentID);

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
                        folder.OwnerID = agent.AgentID;
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

        public UUID CreateItem(Agent agent, string name, string description, InventoryType invType, AssetType type,
            UUID assetID, UUID parentID, PermissionMask ownerMask, PermissionMask nextOwnerMask, UUID ownerID,
            UUID creatorID, UUID transactionID, uint callbackID)
        {
            if (parentID == UUID.Zero)
                parentID = agent.InventoryRoot;

            Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agent.AgentID);

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
                    item.Permissions.OwnerMask = ownerMask;
                    item.Permissions.NextOwnerMask = nextOwnerMask;
                    item.AssetType = type;
                    item.AssetID = assetID;
                    item.OwnerID = agent.AgentID;
                    item.CreatorID = agent.AgentID;
                    item.CreationDate = DateTime.Now;

                    Logger.DebugLog(String.Format("Creating inventory item {0} (InvType: {1}, AssetType: {2})", item.Name,
                        item.InventoryType, item.AssetType));

                    // Store the inventory item
                    agentInventory[item.ID] = item;
                    lock (parentFolder.Children.Dictionary)
                        parentFolder.Children.Dictionary[item.ID] = item;

                    // Send a success response
                    UpdateCreateInventoryItemPacket update = new UpdateCreateInventoryItemPacket();
                    update.AgentData.AgentID = agent.AgentID;
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

                    Server.UDP.SendPacket(agent.AgentID, update, PacketCategory.Inventory);
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

        #region Persistance

        LLSDMap SerializeInventoryItem(InventoryItem item)
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

        LLSDMap SerializeInventoryFolder(InventoryFolder folder)
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

            // FIXME:

            return map;
        }

        public LLSD Serialize()
        {
            LLSDMap map = new LLSDMap(Inventory.Count);
            int itemCount = 0;

            foreach (KeyValuePair<UUID, Dictionary<UUID, InventoryObject>> kvp in Inventory)
            {
                map.Add(kvp.Key.ToString(), SerializeInventory(kvp.Value));
                itemCount += kvp.Value.Count;
            }

            Logger.Log(String.Format("Serializing the inventory store with {0} items", itemCount),
                Helpers.LogLevel.Info);

            return map;
        }

        public void Deserialize(LLSD serialized)
        {
            //accounts.Clear();

            //LLSDArray array = (LLSDArray)serialized;

            //for (int i = 0; i < array.Count; i++)
            //{
            //    Agent agent = new Agent();
            //    object agentRef = (object)agent;
            //    LLSD.DeserializeMembers(ref agentRef, (LLSDMap)array[i]);
            //    agent = (Agent)agentRef;

            //    accounts.Add(agent.FullName, agent.AgentID, agent);
            //}

            //Logger.Log(String.Format("Deserialized the agent store with {0} entries", accounts.Count),
            //    Helpers.LogLevel.Info);
        }

        #endregion Persistance
    }
}
