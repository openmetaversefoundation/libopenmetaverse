using System;
using System.Collections.Generic;
using OpenMetaverse;

namespace Simian
{
    public interface ITaskInventoryProvider
    {
        UUID CreateItem(UUID agentID, UUID containerObjectID, string name, string description, InventoryType invType,
            AssetType type, UUID assetID, UUID parentID, PermissionMask ownerMask, PermissionMask nextOwnerMask,
            UUID ownerID, UUID creatorID, UUID transactionID, uint callbackID, bool sendPacket);
        bool RemoveItem(UUID agentID, UUID containerObjectID, UUID itemID);

        bool TryGetAsset(UUID containerObjectID, UUID assetID, out Asset asset);

        void ForEachItem(UUID containerObjectID, Action<InventoryTaskItem> action);
        InventoryTaskItem FindItem(UUID containerObjectID, Predicate<InventoryTaskItem> match);
        List<InventoryTaskItem> FindAllItems(UUID containerObjectID, Predicate<InventoryTaskItem> match);
    }
}
