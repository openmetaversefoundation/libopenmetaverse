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

        public override bool Equals(object obj)
        {
            if (!(obj is InventoryItem))
                return false;

            InventoryItem o = (InventoryItem)obj;
            return o.ID == ID
                && o.ParentID == ParentID
                && o.Name == Name
                && o.OwnerID == OwnerID
                && o.AssetType == AssetType
                && o.AssetID == AssetID
                && o.CreationDate == CreationDate
                && o.Description == Description
                && o.Flags == Flags
                && o.GroupID == GroupID
                && o.GroupOwned == GroupOwned
                && o.InventoryType == InventoryType
                && o.Permissions.Equals(Permissions)
                && o.SalePrice == SalePrice
                && o.SaleType == SaleType;
        }

        public static bool operator ==(InventoryItem lhs, InventoryItem rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(InventoryItem lhs, InventoryItem rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Returns the InventoryItem in the hierarchical bracket format
        /// used in the Second Life client's notecards and inventory cache.
        /// </summary>
        /// <returns>a string representation of this InventoryItem</returns>
        public override string ToString()
        {
            StringWriter writer = new StringWriter();
            ToString(writer);
            return writer.ToString();
        }

        /// <summary>
        /// Writes the inventory item to the TextWriter in the hierarchical bracket format
        /// used in the Second Life client's notecards and inventory cache.
        /// </summary>
        /// <param name="writer">Writer to write to.</param>
        public void ToString(TextWriter writer)
        {
            writer.WriteLine("inv_item\t0");
            writer.WriteLine('{');
            writer.WriteLine("\titem_id\t{0}", ID.ToString());
            writer.WriteLine("\tparent_id\t{0}", ParentID.ToString());
            // Permissions:
            writer.WriteLine("permissions\t0");
            writer.WriteLine('{');
            writer.WriteLine("\tbase_mask\t{0}", String.Format("{0:x}", (uint)Permissions.BaseMask).PadLeft(8, '0'));
            writer.WriteLine("\towner_mask\t{0}", String.Format("{0:x}", (uint)Permissions.OwnerMask).PadLeft(8, '0'));
            writer.WriteLine("\tgroup_mask\t{0}", String.Format("{0:x}", (uint)Permissions.GroupMask).PadLeft(8, '0'));
            writer.WriteLine("\teveryone_mask\t{0}", String.Format("{0:x}", (uint)Permissions.EveryoneMask).PadLeft(8, '0'));
            writer.WriteLine("\tnext_owner_mask\t{0}", String.Format("{0:x}", (uint)Permissions.NextOwnerMask).PadLeft(8, '0'));
            writer.WriteLine("\tcreator_id\t{0}", CreatorID.ToString());
            writer.WriteLine("\towner_id\t{0}", OwnerID.ToString());
            writer.WriteLine("\tlast_owner_id\t{0}", UUID.Zero); // FIXME?
            writer.WriteLine("\tgroup_id\t{0}", GroupID.ToString());
            writer.WriteLine('}');

            writer.WriteLine("\tasset_id\t{0}", AssetID.ToString());
            writer.WriteLine("\ttype\t{0}", AssetTypeParser.StringValueOf(AssetType));
            writer.WriteLine("\tinv_type\t{0}", InventoryTypeParser.StringValueOf(InventoryType));
            writer.WriteLine("\tflags\t{0}", string.Format("{0:x}", Flags).PadLeft(8, '0'));

            // Sale info:
            writer.WriteLine("sale_info\t0");
            writer.WriteLine('{');
            writer.WriteLine("\tsale_type\t{0}", SaleTypeParser.StringValueOf(SaleType));
            writer.WriteLine("\tsale_price\t{0}", SalePrice);
            writer.WriteLine('}');

            writer.WriteLine("\tname\t{0}|", Name);
            writer.WriteLine("\tdesc\t{0}|", Description);
            writer.WriteLine("\tcreation_date\t{0}", Utils.DateTimeToUnixTime(CreationDate));
            writer.WriteLine('}');
        }

        /// <summary>
        /// Reads the InventoryItem from a string source. The string is wrapped
        /// in a <seealso cref="System.IO.StringReader"/> and passed to the
        /// other <seealso cref="Parse"/> method.
        /// </summary>
        /// <param name="src">String to parse InventoryItem from.</param>
        /// <returns>Parsed InventoryItem</returns>
        public static InventoryItem Parse(string src)
        {
            return Parse(new StringReader(src));
        }

        /// <summary>
        /// Reads an InventoryItem from a TextReader source. The format of the text
        /// should be the same as the one used by Second Life Notecards. The TextReader should
        /// be placed ideally on the line containing "inv_item" but parsing will succeed as long
        /// as it is before the opening bracket immediately following the inv_item line.
        /// The TextReader will be placed on the line following the inv_item's closing bracket.
        /// </summary>
        /// <param name="reader">text source</param>
        /// <returns>Parsed item.</returns>
        public static InventoryItem Parse(TextReader reader)
        {
            InventoryItem item = new InventoryItem();
            #region Parsing
            TextData invItem = TextHierarchyParser.Parse(reader);
            Console.WriteLine(invItem);
            //if (invItem.Name == "inv_item") // YAY
            item.ID = new UUID(invItem.Nested["item_id"].Value);
            item.ParentID = new UUID(invItem.Nested["parent_id"].Value);
            item.AssetID = new UUID(invItem.Nested["asset_id"].Value);
            item.AssetType = AssetTypeParser.Parse(invItem.Nested["type"].Value);
            item.InventoryType = InventoryTypeParser.Parse(invItem.Nested["inv_type"].Value);
            Utils.TryParseHex(invItem.Nested["flags"].Value, out item.Flags);
            string rawName = invItem.Nested["name"].Value;
            item.Name = rawName.Substring(0, rawName.LastIndexOf('|'));
            string rawDesc = invItem.Nested["desc"].Value;
            item.Description = rawDesc.Substring(0, rawDesc.LastIndexOf('|'));
            item.CreationDate = Utils.UnixTimeToDateTime(uint.Parse(invItem.Nested["creation_date"].Value));

            // Sale info:
            TextData saleInfo = invItem.Nested["sale_info"];
            item.SalePrice = int.Parse(saleInfo.Nested["sale_price"].Value);
            item.SaleType = SaleTypeParser.Parse(saleInfo.Nested["sale_type"].Value);

            TextData permissions = invItem.Nested["permissions"];
            item.Permissions = new Permissions();
            item.Permissions.BaseMask = (PermissionMask)uint.Parse(permissions.Nested["base_mask"].Value, NumberStyles.HexNumber);
            item.Permissions.EveryoneMask = (PermissionMask)uint.Parse(permissions.Nested["everyone_mask"].Value, NumberStyles.HexNumber);
            item.Permissions.GroupMask = (PermissionMask)uint.Parse(permissions.Nested["group_mask"].Value, NumberStyles.HexNumber);
            item.Permissions.OwnerMask = (PermissionMask)uint.Parse(permissions.Nested["owner_mask"].Value, NumberStyles.HexNumber);
            item.Permissions.NextOwnerMask = (PermissionMask)uint.Parse(permissions.Nested["next_owner_mask"].Value, NumberStyles.HexNumber);
            item.CreatorID = new UUID(permissions.Nested["creator_id"].Value);
            item.OwnerID = new UUID(permissions.Nested["owner_id"].Value);
            item.GroupID = new UUID(permissions.Nested["group_id"].Value);
            // permissions.Nested["last_owner_id"]  // FIXME?
            #endregion
            return item;
        }

        /// <summary>
        /// <seealso cref="InventoryItem.Parse"/>
        /// </summary>
        /// <param name="str">String to parse from</param>
        /// <param name="item">Parsed InventoryItem</param>
        /// <returns><code>true</code> if successful, <code>false</code> otherwise.</returns>
        public static bool TryParse(string str, out InventoryItem item)
        {
            return TryParse(new StringReader(str), out item);
        }

        /// <summary>
        /// <seealso cref="InventoryItem.Parse"/>
        /// </summary>
        /// <param name="reader">Text source.</param>
        /// <param name="item">Parsed InventoryItem.</param>
        /// <returns><code>true</code> if successful <code>false</code> otherwise.</returns>
        public static bool TryParse(TextReader reader, out InventoryItem item)
        {
            try
            {
                item = Parse(reader);
            }
            catch (Exception e)
            {
                item = new InventoryItem();
                Logger.Log(e.Message, Helpers.LogLevel.Error, e);
                return false;
            }

            return true;
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

        /// <summary>
        /// Returns the InventoryFolder in the hierarchical bracket format
        /// used in the Second Life client's notecards and inventory cache.
        /// </summary>
        /// <returns>a string representation of this InventoryFolder</returns>
        public override string ToString()
        {
            StringWriter writer = new StringWriter();
            ToString(writer);
            return writer.ToString();
        }

        /// <summary>
        /// Writes the InventoryFolder to the TextWriter in the hierarchical bracket format
        /// used in the Second Life client's notecards and inventory cache.
        /// </summary>
        /// <param name="writer">Writer to write to.</param>
        public void ToString(TextWriter writer)
        {
            writer.WriteLine("inv_category\t0");
            writer.WriteLine('{');
            writer.WriteLine("\tcat_id\t{0}", ID.ToString());
            writer.WriteLine("\tparent_id\t{0}", ParentID.ToString());
            writer.WriteLine("\ttype\tcategory");
            // TODO:  Some folders have "-1" as their perf_type, investigate this.
            writer.WriteLine("\tpref_type\t{0}", AssetTypeParser.StringValueOf(PreferredType));
            writer.WriteLine("\tname\t{0}|", Name);
            writer.WriteLine("\towner_id\t{0}", OwnerID.ToString());
            writer.WriteLine("\tversion\t{0}", Version);
            writer.WriteLine('}');
        }

        /// <summary>
        /// Reads the InventoryFolder from a string source. The string is wrapped
        /// in a <seealso cref="System.IO.StringReader"/> and passed to the
        /// other <seealso cref="Parse"/> method.
        /// </summary>
        /// <param name="src">String to parse InventoryFolder from.</param>
        /// <returns>Parsed InventoryFolder</returns>
        public static InventoryFolder Parse(string src)
        {
            return Parse(new StringReader(src));
        }

        /// <summary>
        /// Reads an InventoryItem from a TextReader source. The format of the text
        /// should be the same as the one used by Second Life Notecards. The TextReader should
        /// be placed ideally on the line containing "inv_category" but parsing will succeed as long
        /// as it is before the opening bracket immediately following the inv_category line.
        /// The TextReader will be placed on the line following the inv_category's closing bracket.
        /// </summary>
        /// <param name="reader">text source</param>
        /// <returns>Parsed item.</returns>
        public static InventoryFolder Parse(TextReader reader)
        {
            InventoryFolder folder = new InventoryFolder();
            #region Parsing
            TextData invCategory = TextHierarchyParser.Parse(reader);

            //if (invCategory.Name == "inv_category") // YAY
            folder.ID = new UUID(invCategory.Nested["cat_id"].Value);
            string rawName = invCategory.Nested["name"].Value;
            folder.Name = rawName.Substring(0, rawName.LastIndexOf('|'));
            folder.OwnerID = new UUID(invCategory.Nested["owner_id"].Value);
            folder.ParentID = new UUID(invCategory.Nested["parent_id"].Value);
            folder.PreferredType = AssetTypeParser.Parse(invCategory.Nested["pref_type"].Value);
            folder.Version = int.Parse(invCategory.Nested["version"].Value);
            // TODO: Investigate invCategory.Nested["type"]
            #endregion
            return folder;
        }

        public static bool TryParse(string str, out InventoryFolder folder)
        {
            return TryParse(new StringReader(str), out folder);
        }

        public static bool TryParse(TextReader reader, out InventoryFolder folder)
        {
            try
            {
                folder = Parse(reader);
            }
            catch (Exception e)
            {
                folder = new InventoryFolder();
                Logger.Log(e.Message, Helpers.LogLevel.Error, e);
                return false;
            }
            return true;
        }
    }

    #endregion Inventory Item Containers
}
