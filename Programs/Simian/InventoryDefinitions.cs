using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using OpenMetaverse;

namespace Simian
{
    [Flags]
    public enum InventorySortOrder : uint
    {
        // ItemsByName = 0,
        /// <summary></summary>
        ByDate = 1,
        /// <summary></summary>
        FoldersByName = 2,
        /// <summary></summary>
        SystemFoldersToTop = 4
    }

    #region Inventory Item Containers

    /// <summary>
    /// Base class that inventory items and folders inherit from
    /// </summary>
    public abstract class InventoryObject
    {
        /// <summary><seealso cref="OpenMetaverse.UUID"/> of the inventory item</summary>
        public UUID ID;
        /// <summary><seealso cref="OpenMetaverse.UUID"/> of the parent folder</summary>
        public UUID ParentID;
        /// <summary>Item name</summary>
        public string Name = String.Empty;
        /// <summary>Item owner <seealso cref="OpenMetaverse.UUID"/></summary>
        public UUID OwnerID;
        /// <summary>Parent folder</summary>
        public InventoryObject Parent;

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is InventoryObject))
                return false;

            InventoryObject o = (InventoryObject)obj;
            return o.ID == ID;
        }

        public static bool operator ==(InventoryObject lhs, InventoryObject rhs)
        {
            if ((object)lhs == null)
                return (object)rhs == null;
            else
                return lhs.Equals(rhs);
        }

        public static bool operator !=(InventoryObject lhs, InventoryObject rhs)
        {
            return !(lhs == rhs);
        }
    }

    /// <summary>
    /// Inventory item
    /// </summary>
    public class InventoryItem : InventoryObject
    {
        /// <summary><seealso cref="OpenMetaverse.UUID"/> of the asset this item points to</summary>
        public UUID AssetID;
        /// <summary>The type of item from <seealso cref="OpenMetaverse.AssetType"/></summary>
        public AssetType AssetType;
        /// <summary>The type of item from the <seealso cref="OpenMetaverse.InventoryType"/> enum</summary>
        public InventoryType InventoryType;
        /// <summary>The <seealso cref="OpenMetaverse.UUID"/> of the creator of this item</summary>
        public UUID CreatorID;
        /// <summary>The <seealso cref="OpenMetaverse.Group"/>s <seealso cref="OpenMetaverse.UUID"/> this item is set to or owned by</summary>
        public UUID GroupID;
        /// <summary>A Description of this item</summary>
        public string Description = String.Empty;
        /// <summary>If true, item is owned by a group</summary>
        public bool GroupOwned;
        /// <summary>The combined <seealso cref="OpenMetaverse.Permissions"/> of this item</summary>
        public Permissions Permissions;
        /// <summary>The price this item can be purchased for</summary>
        public int SalePrice;
        /// <summary>The type of sale from the <seealso cref="OpenMetaverse.SaleType"/> enum</summary>
        public SaleType SaleType;
        /// <summary>Combined flags from <seealso cref="OpenMetaverse.InventoryItemFlags"/></summary>
        public uint Flags;
        /// <summary>Time and date this inventory item was created, stored as
        /// UTC (Coordinated Universal Time)</summary>
        public DateTime CreationDate;

        /// <summary>Cyclic redundancy check for this inventory item, calculated by adding most of
        /// the fields together</summary>
        public uint CRC
        {
            get
            {
                return Helpers.InventoryCRC((int)Utils.DateTimeToUnixTime(CreationDate), (byte)SaleType,
                    (sbyte)InventoryType, (sbyte)AssetType, AssetID, GroupID, SalePrice,
                    OwnerID, CreatorID, ID, ParentID, (uint)Permissions.EveryoneMask,
                    Flags, (uint)Permissions.NextOwnerMask, (uint)Permissions.GroupMask,
                    (uint)Permissions.OwnerMask);
            }
        }
    }

    /// <summary>
    /// Inventory folder
    /// </summary>
    public class InventoryFolder : InventoryObject
    {
        /// <summary>The Preferred <seealso cref="T:OpenMetaverse.AssetType"/> for a folder.</summary>
        public AssetType PreferredType;
        /// <summary>The Version of this folder</summary>
        public int Version;
        /// <summary>Number of child items this folder contains</summary>
        public InternalDictionary<UUID, InventoryObject> Children = new InternalDictionary<UUID, InventoryObject>();

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is InventoryFolder))
                return false;

            InventoryFolder o = (InventoryFolder)obj;
            return o.ID == ID;
        }

        public static bool operator ==(InventoryFolder lhs, InventoryFolder rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(InventoryFolder lhs, InventoryFolder rhs)
        {
            return !(lhs == rhs);
        }
    }

    public class InventoryTaskItem : InventoryItem
    {
        public UUID ParentObjectID;
        public UUID PermissionGranter;
        public uint GrantedPermissions;
    }

    #endregion Inventory Item Containers
}
