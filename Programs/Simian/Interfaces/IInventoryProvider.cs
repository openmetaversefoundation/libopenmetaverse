using System;
using OpenMetaverse;

namespace Simian
{
    public interface IInventoryProvider
    {
        Guid CreateItem(Guid agentID, string name, string description, InventoryType invType, AssetType type,
            Guid assetID, Guid parentID, PermissionMask ownerMask, PermissionMask nextOwnerMask, Guid ownerID,
            Guid creatorID, Guid transactionID, uint callbackID, bool sendPacket);
        bool CreateFolder(Guid agentID, Guid folderID, string name, AssetType preferredType, Guid parentID,
            Guid ownerID);
        bool CreateRootFolder(Guid agentID, Guid folderID, string name, Guid ownerID);
        OpenMetaverse.InventoryFolder[] CreateInventorySkeleton(Guid agentID);
        bool TryGetInventory(Guid agentID, Guid objectID, out InventoryObject obj);
    }
}
