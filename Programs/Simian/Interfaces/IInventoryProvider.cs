using System;
using OpenMetaverse;

namespace Simian
{
    public interface IInventoryProvider
    {
        UUID CreateItem(UUID agentID, string name, string description, InventoryType invType, AssetType type,
            UUID assetID, UUID parentID, PermissionMask ownerMask, PermissionMask nextOwnerMask, UUID ownerID,
            UUID creatorID, UUID transactionID, uint callbackID, bool sendPacket);
        bool CreateFolder(UUID agentID, UUID folderID, string name, AssetType preferredType, UUID parentID,
            UUID ownerID);
        bool CreateRootFolder(UUID agentID, UUID folderID, string name, UUID ownerID);
        OpenMetaverse.InventoryFolder[] CreateInventorySkeleton(UUID agentID);
        bool TryGetInventory(UUID agentID, UUID objectID, out InventoryObject obj);
    }
}
