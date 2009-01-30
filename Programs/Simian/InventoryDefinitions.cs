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
        /// <summary><seealso cref="OpenMetaverse.Guid"/> of the inventory item</summary>
        public Guid ID;
        /// <summary><seealso cref="OpenMetaverse.Guid"/> of the parent folder</summary>
        public Guid ParentID;
        /// <summary>Item name</summary>
        public string Name = String.Empty;
        /// <summary>Item owner <seealso cref="OpenMetaverse.Guid"/></summary>
        public Guid OwnerID;
        /// <summary>Parent folder</summary>
        public InventoryObject Parent;

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }

    /// <summary>
    /// Inventory item
    /// </summary>
    public class InventoryItem : InventoryObject
    {
        /// <summary><seealso cref="OpenMetaverse.Guid"/> of the asset this item points to</summary>
        public Guid AssetID;
        /// <summary>The type of item from <seealso cref="OpenMetaverse.AssetType"/></summary>
        public AssetType AssetType;
        /// <summary>The type of item from the <seealso cref="OpenMetaverse.InventoryType"/> enum</summary>
        public InventoryType InventoryType;
        /// <summary>The <seealso cref="OpenMetaverse.Guid"/> of the creator of this item</summary>
        public Guid CreatorID;
        /// <summary>The <seealso cref="OpenMetaverse.Group"/>s <seealso cref="OpenMetaverse.Guid"/> this item is set to or owned by</summary>
        public Guid GroupID;
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

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is InventoryItem))
                return false;

            InventoryItem o = (InventoryItem)obj;
            return o.ID == ID;
        }

        public static bool operator ==(InventoryItem lhs, InventoryItem rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(InventoryItem lhs, InventoryItem rhs)
        {
            return !(lhs == rhs);
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
        public InternalDictionary<Guid, InventoryObject> Children = new InternalDictionary<Guid, InventoryObject>();

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

    #endregion Inventory Item Containers
}
