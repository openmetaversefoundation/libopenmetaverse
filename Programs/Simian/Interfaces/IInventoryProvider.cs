using System;
using System.Collections.Generic;
using OpenMetaverse;

namespace Simian
{
    public interface IInventoryProvider
    {
        InventoryItem CreateItem(UUID agentID, string name, string description, InventoryType invType, AssetType type,
            UUID assetID, UUID parentID, PermissionMask ownerMask, PermissionMask nextOwnerMask, UUID ownerID,
            UUID creatorID, UUID transactionID, uint callbackID);
        bool CreateFolder(UUID agentID, UUID folderID, string name, AssetType preferredType, UUID parentID, UUID ownerID);
        bool CreateRootFolder(UUID agentID, UUID folderID, string name, UUID ownerID);
        OpenMetaverse.InventoryFolder[] CreateInventorySkeleton(UUID agentID);
        InventoryObject MoveInventory(UUID agentID, UUID objectID, UUID newFolderID, string newName, UUID transactionID,
            uint callbackID);
        void PurgeFolder(UUID agentID, UUID folderID);
        bool TryGetInventory(UUID agentID, UUID objectID, out InventoryObject obj);
        bool InventoryExists(UUID agentID);
    }
}
