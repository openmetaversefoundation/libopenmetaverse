using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian
{
    public class LLInventory : IExtension<ISceneProvider>
    {
        ISceneProvider scene;

        public LLInventory()
        {
        }

        public bool Start(ISceneProvider scene)
        {
            this.scene = scene;

            scene.UDP.RegisterPacketCallback(PacketType.CreateInventoryItem, CreateInventoryItemHandler);
            scene.UDP.RegisterPacketCallback(PacketType.CreateInventoryFolder, CreateInventoryFolderHandler);
            scene.UDP.RegisterPacketCallback(PacketType.UpdateInventoryItem, UpdateInventoryItemHandler);
            scene.UDP.RegisterPacketCallback(PacketType.FetchInventoryDescendents, FetchInventoryDescendentsHandler);
            scene.UDP.RegisterPacketCallback(PacketType.FetchInventory, FetchInventoryHandler);
            scene.UDP.RegisterPacketCallback(PacketType.CopyInventoryItem, CopyInventoryItemHandler);
            scene.UDP.RegisterPacketCallback(PacketType.MoveInventoryItem, MoveInventoryItemHandler);
            scene.UDP.RegisterPacketCallback(PacketType.MoveInventoryFolder, MoveInventoryFolderHandler);
            scene.UDP.RegisterPacketCallback(PacketType.PurgeInventoryDescendents, PurgeInventoryDescendentsHandler);
            scene.UDP.RegisterPacketCallback(PacketType.DeRezObject, DeRezObjectHandler);

            return true;
        }

        public void Stop()
        {
        }

        void CreateInventoryItemHandler(Packet packet, Agent agent)
        {
            CreateInventoryItemPacket create = (CreateInventoryItemPacket)packet;
            UUID assetID;
            if (create.InventoryBlock.TransactionID != UUID.Zero)
                assetID = UUID.Combine(create.InventoryBlock.TransactionID, agent.SecureSessionID);
            else
                assetID = UUID.Random();

            UUID parentID = (create.InventoryBlock.FolderID != UUID.Zero) ? create.InventoryBlock.FolderID : agent.Info.InventoryRoot;
            AssetType assetType = (AssetType)create.InventoryBlock.Type;

            switch (assetType)
            {
                case AssetType.Gesture:
                    Logger.Log("Need to create a default gesture asset!", Helpers.LogLevel.Warning);
                    break;
            }

            if (parentID == UUID.Zero)
                parentID = agent.Info.InventoryRoot;

            // Create the inventory item
            InventoryItem item = scene.Server.Inventory.CreateItem(agent.ID, Utils.BytesToString(create.InventoryBlock.Name),
                "Created in Simian", (InventoryType)create.InventoryBlock.InvType, assetType, assetID, parentID,
                PermissionMask.All, (PermissionMask)create.InventoryBlock.NextOwnerMask, agent.ID,
                agent.ID, create.InventoryBlock.TransactionID, create.InventoryBlock.CallbackID);

            // Send a success response
            SendItemCreatedPacket(agent, item, create.InventoryBlock.TransactionID, create.InventoryBlock.CallbackID);
        }

        void CreateInventoryFolderHandler(Packet packet, Agent agent)
        {
            CreateInventoryFolderPacket create = (CreateInventoryFolderPacket)packet;

            UUID folderID = create.FolderData.FolderID;
            if (folderID == UUID.Zero)
                folderID = agent.Info.InventoryRoot;

            scene.Server.Inventory.CreateFolder(agent.ID, folderID, Utils.BytesToString(create.FolderData.Name),
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

                InventoryObject obj;
                if (scene.Server.Inventory.TryGetInventory(agent.ID, block.ItemID, out obj) && obj is InventoryItem)
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

            // TODO: Use OwnerID
            // TODO: Do we need to obey InventorySortOrder?
            InventoryObject invObject;
            if (scene.Server.Inventory.TryGetInventory(agent.ID, fetch.InventoryData.FolderID, out invObject) && invObject is InventoryFolder)
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

                        scene.UDP.SendPacket(agent.ID, descendents, PacketCategory.Inventory);
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

                    scene.UDP.SendPacket(agent.ID, descendents, PacketCategory.Inventory);
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

                        scene.UDP.SendPacket(agent.ID, descendents, PacketCategory.Inventory);
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

                blocks[i] = new FetchInventoryReplyPacket.InventoryDataBlock();
                blocks[i].ItemID = itemID;

                InventoryObject obj;
                if (scene.Server.Inventory.TryGetInventory(agent.ID, itemID, out obj) && obj is InventoryItem)
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

                    blocks[i].Name = Utils.EmptyBytes;
                    blocks[i].Description = Utils.EmptyBytes;
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

                scene.UDP.SendPacket(agent.ID, reply, PacketCategory.Inventory);
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

                // Get the original object
                InventoryObject obj;
                if (scene.Server.Inventory.TryGetInventory(agent.ID, block.OldItemID, out obj) && obj is InventoryItem)
                {
                    InventoryItem item = (InventoryItem)obj;

                    // Get the new folder
                    InventoryObject folderObj;
                    if (scene.Server.Inventory.TryGetInventory(agent.ID, block.NewFolderID, out folderObj) && folderObj is InventoryFolder)
                    {
                        string newName = Utils.BytesToString(block.NewName);
                        if (String.IsNullOrEmpty(newName))
                            newName = item.Name;

                        // Create the copy
                        InventoryItem newItem = scene.Server.Inventory.CreateItem(agent.ID, newName, item.Description, item.InventoryType,
                            item.AssetType, item.AssetID, folderObj.ID, item.Permissions.OwnerMask, item.Permissions.NextOwnerMask,
                            agent.ID, item.CreatorID, UUID.Zero, block.CallbackID);

                        SendItemCreatedPacket(agent, newItem, UUID.Zero, block.CallbackID);
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

            List<InventoryObject> objs = new List<InventoryObject>(move.InventoryData.Length);

            for (int i = 0; i < move.InventoryData.Length; i++)
            {
                MoveInventoryItemPacket.InventoryDataBlock block = move.InventoryData[i];
                UUID newFolderID = block.FolderID;
                if (newFolderID == UUID.Zero)
                    newFolderID = agent.Info.InventoryRoot;
                InventoryObject obj = scene.Server.Inventory.MoveInventory(agent.ID, block.ItemID, newFolderID,
                    Utils.BytesToString(block.NewName), UUID.Zero, 0);

                if (obj != null) objs.Add(obj);
            }

            SendBulkUpdate(agent, objs, UUID.Zero, 0);
        }

        void MoveInventoryFolderHandler(Packet packet, Agent agent)
        {
            MoveInventoryFolderPacket move = (MoveInventoryFolderPacket)packet;
            // TODO: What is move.AgentData.Stamp for?

            List<InventoryObject> objs = new List<InventoryObject>(move.InventoryData.Length);

            for (int i = 0; i < move.InventoryData.Length; i++)
            {
                MoveInventoryFolderPacket.InventoryDataBlock block = move.InventoryData[i];
                UUID newFolderID = block.ParentID;
                if (newFolderID == UUID.Zero)
                    newFolderID = agent.Info.InventoryRoot;
                InventoryObject obj = scene.Server.Inventory.MoveInventory(agent.ID, block.FolderID, newFolderID,
                    null, UUID.Zero, 0);

                if (obj != null) objs.Add(obj);
            }

            SendBulkUpdate(agent, objs, UUID.Zero, 0);
        }

        void PurgeInventoryDescendentsHandler(Packet packet, Agent agent)
        {
            PurgeInventoryDescendentsPacket purge = (PurgeInventoryDescendentsPacket)packet;
            scene.Server.Inventory.PurgeFolder(agent.ID, purge.InventoryData.FolderID);
        }

        void DeRezObjectHandler(Packet packet, Agent agent)
        {
            DeRezObjectPacket derez = (DeRezObjectPacket)packet;
            DeRezDestination destination = (DeRezDestination)derez.AgentBlock.Destination;

            // TODO: Check permissions
            for (int i = 0; i < derez.ObjectData.Length; i++)
            {
                uint localID = derez.ObjectData[i].ObjectLocalID;

                SimulationObject obj;
                if (scene.TryGetObject(localID, out obj))
                {
                    switch (destination)
                    {
                        case DeRezDestination.AgentInventorySave:
                            Logger.Log("DeRezObject: Got an AgentInventorySave, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                        case DeRezDestination.AgentInventoryCopy:
                            Logger.Log("DeRezObject: Got an AgentInventorySave, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                        case DeRezDestination.TaskInventory:
                            Logger.Log("DeRezObject: Got a TaskInventory, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                        case DeRezDestination.Attachment:
                            Logger.Log("DeRezObject: Got an Attachment, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                        case DeRezDestination.AgentInventoryTake:
                            Logger.Log("DeRezObject: Got an AgentInventoryTake, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                        case DeRezDestination.ForceToGodInventory:
                            Logger.Log("DeRezObject: Got a ForceToGodInventory, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                        case DeRezDestination.TrashFolder:
                            InventoryObject invObj;
                            if (scene.Server.Inventory.TryGetInventory(agent.ID, derez.AgentBlock.DestinationID, out invObj) &&
                                invObj is InventoryFolder)
                            {
                                // FIXME: Handle children
                                InventoryFolder trash = (InventoryFolder)invObj;
                                InventoryItem item = scene.Server.Inventory.CreateItem(agent.ID, obj.Prim.Properties.Name,
                                    obj.Prim.Properties.Description, InventoryType.Object, AssetType.Object, obj.Prim.ID,
                                    trash.ID, PermissionMask.All, PermissionMask.All, agent.ID, obj.Prim.Properties.CreatorID,
                                    derez.AgentBlock.TransactionID, 0);
                                scene.ObjectRemove(this, obj.Prim.LocalID);

                                SendItemCreatedPacket(agent, item, derez.AgentBlock.TransactionID, 0);
                                Logger.DebugLog(String.Format("Derezzed prim {0} to agent inventory trash", obj.Prim.LocalID));
                            }
                            else
                            {
                                Logger.Log("DeRezObject: Got a TrashFolder with an invalid trash folder: " +
                                    derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            }
                            break;
                        case DeRezDestination.AttachmentToInventory:
                            Logger.Log("DeRezObject: Got an AttachmentToInventory, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                        case DeRezDestination.AttachmentExists:
                            Logger.Log("DeRezObject: Got an AttachmentExists, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                        case DeRezDestination.ReturnToOwner:
                            Logger.Log("DeRezObject: Got a ReturnToOwner, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                        case DeRezDestination.ReturnToLastOwner:
                            Logger.Log("DeRezObject: Got a ReturnToLastOwner, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                    }
                }
            }
        }

        void SendItemCreatedPacket(Agent agent, InventoryItem item, UUID transactionID, uint callbackID)
        {
            UpdateCreateInventoryItemPacket update = new UpdateCreateInventoryItemPacket();
            update.AgentData.AgentID = agent.ID;
            update.AgentData.SimApproved = true;
            if (transactionID != UUID.Zero)
                update.AgentData.TransactionID = transactionID;
            else
                update.AgentData.TransactionID = UUID.Random();
            update.InventoryData = new UpdateCreateInventoryItemPacket.InventoryDataBlock[1];
            update.InventoryData[0] = new UpdateCreateInventoryItemPacket.InventoryDataBlock();
            update.InventoryData[0].AssetID = item.AssetID;
            update.InventoryData[0].BaseMask = (uint)item.Permissions.BaseMask;
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

            scene.UDP.SendPacket(agent.ID, update, PacketCategory.Inventory);
        }

        void SendBulkUpdate(Agent agent, List<InventoryObject> objs, UUID transactionID, uint callbackID)
        {
            BulkUpdateInventoryPacket update = new BulkUpdateInventoryPacket();
            update.AgentData.AgentID = agent.ID;
            update.AgentData.TransactionID = transactionID;

            // Count the number of folders and items
            int items = 0;
            int folders = 0;
            for (int i = 0; i < objs.Count; i++)
            {
                if (objs[i] is InventoryItem)
                    ++items;
                else
                    ++folders;
            }

            update.FolderData = new BulkUpdateInventoryPacket.FolderDataBlock[folders];
            update.ItemData = new BulkUpdateInventoryPacket.ItemDataBlock[items];

            items = 0;
            folders = 0;

            for (int i = 0; i < objs.Count; i++)
            {
                InventoryObject obj = objs[i];

                if (obj is InventoryItem)
                {
                    InventoryItem item = (InventoryItem)obj;

                    update.ItemData[items] = new BulkUpdateInventoryPacket.ItemDataBlock();
                    update.ItemData[items].AssetID = item.AssetID;
                    update.ItemData[items].BaseMask = (uint)item.Permissions.BaseMask;
                    update.ItemData[items].CallbackID = callbackID;
                    update.ItemData[items].CRC = item.CRC;
                    update.ItemData[items].CreationDate = (int)Utils.DateTimeToUnixTime(item.CreationDate);
                    update.ItemData[items].CreatorID = item.CreatorID;
                    update.ItemData[items].Description = Utils.StringToBytes(item.Description);
                    update.ItemData[items].EveryoneMask = (uint)item.Permissions.EveryoneMask;
                    update.ItemData[items].Flags = item.Flags;
                    update.ItemData[items].FolderID = item.ParentID;
                    update.ItemData[items].GroupID = item.GroupID;
                    update.ItemData[items].GroupMask = (uint)item.Permissions.GroupMask;
                    update.ItemData[items].GroupOwned = item.GroupOwned;
                    update.ItemData[items].InvType = (sbyte)item.InventoryType;
                    update.ItemData[items].ItemID = item.ID;
                    update.ItemData[items].Name = Utils.StringToBytes(item.Name);
                    update.ItemData[items].NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
                    update.ItemData[items].OwnerID = item.OwnerID;
                    update.ItemData[items].OwnerMask = (uint)item.Permissions.OwnerMask;
                    update.ItemData[items].SalePrice = item.SalePrice;
                    update.ItemData[items].SaleType = (byte)item.SaleType;
                    update.ItemData[items].Type = (sbyte)item.InventoryType;

                    ++items;
                }
                else
                {
                    InventoryFolder folder = (InventoryFolder)obj;

                    update.FolderData[folders] = new BulkUpdateInventoryPacket.FolderDataBlock();
                    update.FolderData[folders].FolderID = folder.ID;
                    update.FolderData[folders].Name = Utils.StringToBytes(folder.Name);
                    update.FolderData[folders].ParentID = folder.ParentID;
                    update.FolderData[folders].Type = (sbyte)folder.PreferredType;

                    ++folders;
                }
            }

            Logger.DebugLog("Sending bulk update for " + items + " items and " + folders + " folders");
            scene.UDP.SendPacket(agent.ID, update, PacketCategory.Inventory);
        }
    }
}
