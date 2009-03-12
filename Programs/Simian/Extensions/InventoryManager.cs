using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Packets;

namespace Simian
{
    public class InventoryManager : IExtension<Simian>, IInventoryProvider, IPersistable
    {
        Simian server;
        /// <summary>Dictionary of inventories for each agent. Each inventory
        /// is also a dictionary itself</summary>
        Dictionary<UUID, Dictionary<UUID, InventoryObject>> Inventory =
            new Dictionary<UUID, Dictionary<UUID, InventoryObject>>();
        /// <summary>Global shared inventory for all agents</summary>
        Dictionary<UUID, InventoryObject> Library = new Dictionary<UUID, InventoryObject>();

        public InventoryManager()
        {
        }

        public bool Start(Simian server)
        {
            this.server = server;
            return true;
        }

        public void Stop()
        {
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

        public InventoryItem CreateItem(UUID agentID, string name, string description, InventoryType invType, AssetType type,
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

                    return item;
                }
                else
                {
                    Logger.Log(String.Format(
                        "Cannot create new inventory item, folder {0} does not exist",
                        parentID), Helpers.LogLevel.Warning);

                    return null;
                }
            }
        }

        public InventoryObject MoveInventory(UUID agentID, UUID objectID, UUID newFolderID, string newName, UUID transactionID, uint callbackID)
        {
            Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agentID);

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

                        return obj;
                    }
                    else
                    {
                        Logger.Log("MoveInventory called with an unknown destination folder " + newFolderID,
                            Helpers.LogLevel.Warning);
                        return null;
                    }
                }
            }
            else
            {
                Logger.Log("MoveInventory called for an unknown object " + objectID,
                    Helpers.LogLevel.Warning);
                return null;
            }
        }

        public void PurgeFolder(UUID agentID, UUID folderID)
        {
            Dictionary<UUID, InventoryObject> agentInventory = GetAgentInventory(agentID);

            InventoryObject obj;
            if (TryGetInventory(agentID, folderID, out obj) && obj is InventoryFolder)
            {
                InventoryFolder folder = (InventoryFolder)obj;

                folder.Children.ForEach(
                    delegate(InventoryObject child)
                    {
                        agentInventory.Remove(child.ID);

                        if (child is InventoryFolder)
                            PurgeFolder(agentID, child.ID);
                    }
                );

                lock (folder.Children.Dictionary)
                    folder.Children.Dictionary.Clear();
            }
            else
            {
                Logger.Log("PurgeFolder called on a missing folder " + folderID, Helpers.LogLevel.Warning);
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

        public bool InventoryExists(UUID agentID)
        {
            return Inventory.ContainsKey(agentID);
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
