/*
 * Copyright (c) 2007-2008, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names 
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Globalization;
using System.Text;
using OpenMetaverse.Capabilities;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Packets;
using System.IO;

namespace OpenMetaverse
{
    #region Enums
    /// <summary>
    /// Inventory Item Types, eg Script, Notecard, Folder, etc
    /// </summary>
    public enum InventoryType : sbyte
    {
        /// <summary>Unknown</summary>
        Unknown = -1,
        /// <summary>Texture</summary>
        Texture = 0,
        /// <summary>Sound</summary>
        Sound = 1,
        /// <summary>Calling Card</summary>
        CallingCard = 2,
        /// <summary>Landmark</summary>
        Landmark = 3,
        /// <summary>Script</summary>
        [Obsolete("See LSL")]
        Script = 4,
        /// <summary>Clothing</summary>
        [Obsolete("See Wearable")]
        Clothing = 5,
        /// <summary>Object, both single and coalesced</summary>
        Object = 6,
        /// <summary>Notecard</summary>
        Notecard = 7,
        /// <summary></summary>
        Category = 8,
        /// <summary>Folder</summary>
        Folder = 8,
        /// <summary></summary>
        RootCategory = 9,
        /// <summary>an LSL Script</summary>
        LSL = 10,
        /// <summary></summary>
        [Obsolete("See LSL")]
        LSLBytecode = 11,
        /// <summary></summary>
        [Obsolete("See Texture")]
        TextureTGA = 12,
        /// <summary></summary>
        [Obsolete]
        Bodypart = 13,
        /// <summary></summary>
        [Obsolete]
        Trash = 14,
        /// <summary></summary>
        Snapshot = 15,
        /// <summary></summary>
        [Obsolete]
        LostAndFound = 16,
        /// <summary></summary>
        Attachment = 17,
        /// <summary></summary>
        Wearable = 18,
        /// <summary></summary>
        Animation = 19,
        /// <summary></summary>
        Gesture = 20
    }

    public static class InventoryTypeParser
    {
        private static readonly ReversableDictionary<string, InventoryType> InventoryTypeMap = new ReversableDictionary<string, InventoryType>();

        static InventoryTypeParser()
        {
            InventoryTypeMap.Add("sound", InventoryType.Sound);
            InventoryTypeMap.Add("wearable", InventoryType.Wearable);
            InventoryTypeMap.Add("gesture", InventoryType.Gesture);
            InventoryTypeMap.Add("script", InventoryType.LSL);
            InventoryTypeMap.Add("texture", InventoryType.Texture);
            InventoryTypeMap.Add("landmark", InventoryType.Landmark);
            InventoryTypeMap.Add("notecard", InventoryType.Notecard);
            InventoryTypeMap.Add("object", InventoryType.Object);
            InventoryTypeMap.Add("animation", InventoryType.Animation);
            InventoryTypeMap.Add("snapshot", InventoryType.Snapshot);
            InventoryTypeMap.Add("attach", InventoryType.Attachment);
            InventoryTypeMap.Add("callcard", InventoryType.CallingCard);
        }

        public static InventoryType Parse(string str)
        {
            InventoryType t;
            if (InventoryTypeMap.TryGetValue(str, out t))
                return t;
            else
                return InventoryType.Unknown;
        }

        public static string StringValueOf(InventoryType type)
        {
            string str;
            if (InventoryTypeMap.TryGetKey(type, out str))
                return str;
            else
                return "unknown";
        }
    }

    /// <summary>
    /// Item Sale Status
    /// </summary>
    public enum SaleType : byte
    {
        /// <summary>Not for sale</summary>
        Not = 0,
        /// <summary>The original is for sale</summary>
        Original = 1,
        /// <summary>Copies are for sale</summary>
        Copy = 2,
        /// <summary>The contents of the object are for sale</summary>
        Contents = 3
    }

    public static class SaleTypeParser
    {
        private static readonly ReversableDictionary<string, SaleType> SaleTypeMap = new ReversableDictionary<string, SaleType>();

        static SaleTypeParser()
        {
            SaleTypeMap.Add("not", SaleType.Not);
            SaleTypeMap.Add("cntn", SaleType.Contents);
            SaleTypeMap.Add("copy", SaleType.Copy);
            SaleTypeMap.Add("orig", SaleType.Original);
        }
        public static SaleType Parse(string str)
        {
            SaleType t;
            if (SaleTypeMap.TryGetValue(str, out t))
                return t;
            else
                return SaleType.Not;
        }
        public static string StringValueOf(SaleType type)
        {
            string str;
            if (SaleTypeMap.TryGetKey(type, out str))
                return str;
            else
                return "not";
        }
    }

    [Flags]
    public enum InventorySortOrder : int
    {
        /// <summary>Sort by name</summary>
        ByName = 0,
        /// <summary>Sort by date</summary>
        ByDate = 1,
        /// <summary>Sort folders by name, regardless of whether items are
        /// sorted by name or date</summary>
        FoldersByName = 2,
        /// <summary>Place system folders at the top</summary>
        SystemFoldersToTop = 4
    }

    /// <summary>
    /// Possible destinations for DeRezObject request
    /// </summary>
    public enum DeRezDestination : byte
    {
        /// <summary>
        /// Take a copy of the item
        /// </summary>
        TakeCopy = 1,
        /// <summary>Derez to TaskInventory</summary>
        TaskInventory = 2,
        /// <summary>Take Object</summary>
        ObjectsFolder = 4,
        /// <summary>Delete Object</summary>
        TrashFolder = 6
    }

    #endregion Enums

    /// <summary>
    /// Struct containing entire inventory state for an item.
    /// </summary>
    public struct ItemData
    {
        /// <summary><seealso cref="OpenMetaverse.UUID"/> of item/folder</summary>
        public UUID UUID;
        /// <summary><seealso cref="OpenMetaverse.UUID"/> of parent folder</summary>
        public UUID ParentUUID;
        /// <summary>Name of item/folder</summary>
        public string Name;
        /// <summary>Item/Folder Owners <seealso cref="OpenMetaverse.UUID"/></summary>
        public UUID OwnerID;
        /// <summary>The <seealso cref="OpenMetaverse.UUID"/> of this item</summary>
        public UUID AssetUUID;
        /// <summary>The combined <seealso cref="OpenMetaverse.Permissions"/> of this item</summary>
        public Permissions Permissions;
        /// <summary>The type of item from <seealso cref="OpenMetaverse.AssetType"/></summary>
        public AssetType AssetType;
        /// <summary>The type of item from the <seealso cref="OpenMetaverse.InventoryType"/> enum</summary>
        public InventoryType InventoryType;
        /// <summary>The <seealso cref="OpenMetaverse.UUID"/> of the creator of this item</summary>
        public UUID CreatorID;
        /// <summary>A Description of this item</summary>
        public string Description;
        /// <summary>The <seealso cref="OpenMetaverse.Group"/>s <seealso cref="OpenMetaverse.UUID"/> this item is set to or owned by</summary>
        public UUID GroupID;
        /// <summary>If true, item is owned by a group</summary>
        public bool GroupOwned;
        /// <summary>The price this item can be purchased for</summary>
        public int SalePrice;
        /// <summary>The type of sale from the <seealso cref="OpenMetaverse.SaleType"/> enum</summary>
        public SaleType SaleType;
        /// <summary>Combined flags from <seealso cref="OpenMetaverse.InventoryItemFlags"/></summary>
        public uint Flags;
        /// <summary>Time and date this inventory item was created, stored as
        /// UTC (Coordinated Universal Time)</summary>
        public DateTime CreationDate;

        public ItemData(InventoryType type)
            : this(UUID.Zero, type) { }
        public ItemData(UUID uuid)
            : this(uuid, InventoryType.Unknown) { }
        public ItemData(UUID uuid, InventoryType type)
        {
            UUID = uuid;
            InventoryType = type;
            ParentUUID = UUID.Zero;
            Name = String.Empty;
            OwnerID = UUID.Zero;
            AssetUUID = UUID.Zero;
            Permissions = new Permissions();
            AssetType = AssetType.Unknown;
            CreatorID = UUID.Zero;
            Description = String.Empty;
            GroupID = UUID.Zero;
            GroupOwned = false;
            SalePrice = 0;
            SaleType = SaleType.Not;
            Flags = 0;
            CreationDate = DateTime.Now;
        }

        public override int GetHashCode()
        {
            return UUID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ItemData))
                return false;

            ItemData o = (ItemData)obj;
            return o.UUID == UUID
                && o.ParentUUID == ParentUUID
                && o.Name == Name
                && o.OwnerID == OwnerID
                && o.AssetType == AssetType
                && o.AssetUUID == AssetUUID
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
        public static bool operator ==(ItemData lhs, ItemData rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ItemData lhs, ItemData rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Returns the ItemData in the hierarchical bracket format
        /// used in the Second Life client's notecards and inventory cache.
        /// </summary>
        /// <returns>a string representation of this ItemData</returns>
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
            writer.WriteLine("\titem_id\t{0}", UUID.ToString());
            writer.WriteLine("\tparent_id\t{0}", ParentUUID.ToString());
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

            writer.WriteLine("\tasset_id\t{0}", AssetUUID.ToString());
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
        /// Reads the ItemData from a string source. The string is wrapped
        /// in a <seealso cref="System.IO.StringReader"/> and passed to the
        /// other <seealso cref="Parse"/> method.
        /// </summary>
        /// <param name="src">String to parse ItemData from.</param>
        /// <returns>Parsed ItemData</returns>
        public static ItemData Parse(string src)
        {
            return Parse(new StringReader(src));
        }

        /// <summary>
        /// Reads an ItemData from a TextReader source. The format of the text
        /// should be the same as the one used by Second Life Notecards. The TextReader should
        /// be placed ideally on the line containing "inv_item" but parsing will succeed as long
        /// as it is before the opening bracket immediately following the inv_item line.
        /// The TextReader will be placed on the line following the inv_item's closing bracket.
        /// </summary>
        /// <param name="reader">text source</param>
        /// <returns>Parsed item.</returns>
        public static ItemData Parse(TextReader reader)
        {
            ItemData item = new ItemData();
            #region Parsing
            TextData invItem = TextHierarchyParser.Parse(reader);
            Console.WriteLine(invItem);
            //if (invItem.Name == "inv_item") // YAY
            item.UUID = new UUID(invItem.Nested["item_id"].Value);
            item.ParentUUID = new UUID(invItem.Nested["parent_id"].Value);
            item.AssetUUID = new UUID(invItem.Nested["asset_id"].Value);
            item.AssetType = AssetTypeParser.Parse(invItem.Nested["type"].Value);
            item.InventoryType = InventoryTypeParser.Parse(invItem.Nested["inv_type"].Value);
            item.Flags = uint.Parse(invItem.Nested["flags"].Value, NumberStyles.HexNumber);
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
        /// <seealso cref="ItemData.Parse"/>
        /// </summary>
        /// <param name="str">String to parse from.</param>
        /// <param name="item">Parsed ItemData.</param>
        /// <returns><code>true</code> if successful, <code>false</code> otherwise.</returns>
        public static bool TryParse(string str, out ItemData item)
        {
            return TryParse(new StringReader(str), out item);
        }

        
        /// <summary>
        /// <seealso cref="ItemData.Parse"/>
        /// </summary>
        /// <param name="reader">Text source.</param>
        /// <param name="item">Parsed ItemData.</param>
        /// <returns><code>true</code> if successful <code>false</code> otherwise.</returns>
        public static bool TryParse(TextReader reader, out ItemData item)
        {
            try
            {
                item = Parse(reader);
            }
            catch (Exception e)
            {
                item = new ItemData();
                Logger.Log(e.Message, Helpers.LogLevel.Error, e);
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Struct containing all inventory state for a folder.
    /// </summary>
    public struct FolderData
    {
        /// <summary>The <seealso cref="OpenMetaverse.UUID"/> of this item</summary>
        public UUID UUID;
        /// <summary><seealso cref="OpenMetaverse.UUID"/> of parent folder</summary>
        public UUID ParentUUID;
        /// <summary>Name of item/folder</summary>
        public string Name;
        /// <summary>Item/Folder Owners <seealso cref="OpenMetaverse.UUID"/></summary>
        public UUID OwnerID;
        /// <summary>The Preferred <seealso cref="T:OpenMetaverse.AssetType"/> for a folder.</summary>
        public AssetType PreferredType;
        /// <summary>The Version of this folder</summary>
        public int Version;
        /// <summary>Number of child items this folder contains.</summary>
        public int DescendentCount;

        public FolderData(UUID uuid)
        {
            UUID = uuid;
            ParentUUID = UUID.Zero;
            Name = String.Empty;
            OwnerID = UUID.Zero;
            PreferredType = AssetType.Unknown;
            Version = 0;
            DescendentCount = 0;
        }

        public override int GetHashCode()
        {
            return ParentUUID.GetHashCode() ^ Name.GetHashCode() ^ OwnerID.GetHashCode() ^
                PreferredType.GetHashCode() ^ Version.GetHashCode() ^ DescendentCount.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FolderData))
                return false;

            FolderData o = (FolderData)obj;
            return o.UUID == UUID
                && o.ParentUUID == ParentUUID
                && o.Name == Name
                && o.OwnerID == OwnerID
                && o.DescendentCount == DescendentCount
                && o.PreferredType == PreferredType
                && o.Version == Version;
        }

        public static bool operator ==(FolderData lhs, FolderData rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(FolderData lhs, FolderData rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Returns the FolderData in the hierarchical bracket format
        /// used in the Second Life client's notecards and inventory cache.
        /// </summary>
        /// <returns>a string representation of this FolderData</returns>
        public override string ToString()
        {
            StringWriter writer = new StringWriter();
            ToString(writer);
            return writer.ToString();
        }

        /// <summary>
        /// Writes the FolderData to the TextWriter in the hierarchical bracket format
        /// used in the Second Life client's notecards and inventory cache.
        /// </summary>
        /// <param name="writer">Writer to write to.</param>
        public void ToString(TextWriter writer)
        {
            writer.WriteLine("inv_category\t0");
            writer.WriteLine('{');
            writer.WriteLine("\tcat_id\t{0}", UUID.ToString());
            writer.WriteLine("\tparent_id\t{0}", ParentUUID.ToString());
            writer.WriteLine("\ttype\tcategory");
            // TODO:  Some folders have "-1" as their perf_type, investigate this.
            writer.WriteLine("\tpref_type\t{0}", AssetTypeParser.StringValueOf(PreferredType));
            writer.WriteLine("\tname\t{0}|", Name);
            writer.WriteLine("\towner_id\t{0}", OwnerID.ToString());
            writer.WriteLine("\tversion\t{0}", Version);
            writer.WriteLine('}');
        }

        /// <summary>
        /// Reads the FolderData from a string source. The string is wrapped
        /// in a <seealso cref="System.IO.StringReader"/> and passed to the
        /// other <seealso cref="Parse"/> method.
        /// </summary>
        /// <param name="src">String to parse FolderData from.</param>
        /// <returns>Parsed FolderData</returns>
        public static FolderData Parse(string src)
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
        public static FolderData Parse(TextReader reader)
        {
            FolderData folder = new FolderData();
            #region Parsing
            TextData invCategory = TextHierarchyParser.Parse(reader);

            //if (invCategory.Name == "inv_category") // YAY
            folder.UUID = new UUID(invCategory.Nested["cat_id"].Value);
            string rawName = invCategory.Nested["name"].Value;
            folder.Name = rawName.Substring(0, rawName.LastIndexOf('|'));
            folder.OwnerID = new UUID(invCategory.Nested["owner_id"].Value);
            folder.ParentUUID = new UUID(invCategory.Nested["parent_id"].Value);
            folder.PreferredType = AssetTypeParser.Parse(invCategory.Nested["pref_type"].Value);
            folder.Version = int.Parse(invCategory.Nested["version"].Value);
            // TODO: Investigate invCategory.Nested["type"]
            #endregion
            return folder;
        }

        public static bool TryParse(string str, out FolderData folder)
        {
            return TryParse(new StringReader(str), out folder);
        }

        public static bool TryParse(TextReader reader, out FolderData folder)
        {
            try
            {
                folder = Parse(reader);
            }
            catch (Exception e)
            {
                folder = new FolderData();
                Logger.Log(e.Message, Helpers.LogLevel.Error, e);
                return false;
            }
            return true;
        }
    }

    public class InventorySkeleton
    {
        public UUID RootUUID;
        public UUID Owner;
        public FolderData[] Folders;
        public InventorySkeleton(UUID rootFolder, UUID owner)
        {
            RootUUID = rootFolder;
            Owner = owner;
            Folders = new FolderData[0];
        }
    }

    /// <summary>
    /// Tools for dealing with agents inventory
    /// </summary>
    public class InventoryManager
    {
        protected struct DescendentsRequest
        {
            public UUID Folder;
            public bool ReceivedResponse;
            public FolderContentsCallback Callback;
            public PartialContentsCallback PartialCallback;
            public int Descendents;
            public List<FolderData> FolderContents;
            public List<ItemData> ItemContents;
            public DescendentsRequest(UUID folder, FolderContentsCallback callback)
            {
                Folder = folder;
                Callback = callback;
                ReceivedResponse = false;
                Descendents = 0;
                FolderContents = new List<FolderData>();
                ItemContents = new List<ItemData>();
                PartialCallback = null;
            }
        }

        protected struct FetchRequest
        {
            public int ItemsFetched;
            public Dictionary<UUID, ItemData?> RequestedItems;
            public FetchItemsCallback Callback;

            public FetchRequest(FetchItemsCallback callback, ICollection<UUID> requestedItems)
            {
                ItemsFetched = 0;
                Callback = callback;
                RequestedItems = new Dictionary<UUID, ItemData?>(requestedItems.Count);
                foreach (UUID uuid in requestedItems)
                    RequestedItems.Add(uuid, null);
            }

            public void StoreFetchedItem(ItemData item)
            {
                if (RequestedItems.ContainsKey(item.UUID) && RequestedItems[item.UUID] == null)
                {
                    ++ItemsFetched;
                    RequestedItems[item.UUID] = item;
                }
            }
        }

        #region Delegates

        /// <summary>
        /// Delegate for <seealso cref="OnSkeletonsReceived"/>
        /// </summary>
        public delegate void SkeletonsReceived(InventoryManager manager);

        /// <summary>
        /// Callback for inventory item creation finishing
        /// </summary>
        /// <param name="success">Whether the request to create an inventory
        /// item succeeded or not</param>
        /// <param name="item">Inventory item being created. If success is
        /// false this will be null</param>
        public delegate void ItemCreatedCallback(bool success, ItemData item);

        /// <summary>
        /// Callback for an inventory item being create from an uploaded asset
        /// </summary>
        /// <param name="success">true if inventory item creation was successful</param>
        /// <param name="status"></param>
        /// <param name="itemID"></param>
        /// <param name="assetID"></param>
        public delegate void ItemCreatedFromAssetCallback(bool success, string status, UUID itemID, UUID assetID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemData"></param>
        public delegate void ItemCopiedCallback(ItemData itemData);

        /// <summary>
        /// Use this delegate to create a callback for RequestFolderContents.
        /// </summary>
        /// <param name="folder">The folder whose contents were received.</param>
        /// <param name="Items">The items in <paramref name="folder"/></param>
        /// <param name="Folders">The folders in <paramref name="folder"/></param>
        /// <seealso cref="InventoryManager.RequestFolderContents"/>
        public delegate void FolderContentsCallback(UUID folder, List<ItemData> Items, List<FolderData> Folders);

        /// <summary>
        /// Use this delegate to create a callback for RequestFolderContents
        /// </summary>
        /// <param name="folder">The folder whose contents were received.</param>
        /// <param name="items">The items in <paramref name="folder"/> that were just received, <code>null</code> if none received.</param>
        /// <param name="folders">The folders in <paramref name="folder"/> that were just received, <code>null</code> if none received.</param>
        /// <param name="remaining">Number of item or folders that remain to be downloaded.</param>
        public delegate void PartialContentsCallback(UUID folder, ItemData[] items, FolderData[] folders, int remaining);
        
        /// <summary>
        /// Use this delegate to create a callback for RequestFetchItems.
        /// </summary>
        /// <param name="items">The items retrieved.</param>
        /// <seealso cref="InventoryManager.RequestFetchItems"/>
        public delegate void FetchItemsCallback(List<ItemData> items);

        /// <seealso cref="InventoryManager.OnItemUpdate"/>
        /// <param name="itemData">The updated item data.</param>
        public delegate void ItemUpdate(ItemData itemData);

        /// <seealso cref="InventoryManager.OnFolderUpdate"/>
        /// <param name="folderData">The updated folder data.</param>
        public delegate void FolderUpdate(FolderData folderData);

        public delegate void AssetUpdate(UUID itemID, UUID newAssetID);

        /// <summary>
        /// Callback for when an inventory item is offered to us by another avatar or an object
        /// </summary>
        /// <param name="offerDetails">A <seealso cref="InstantMessage"/> object containing specific
        /// details on the item being offered, eg who its from</param>
        /// <param name="type">The <seealso cref="AssetType"/>AssetType being offered</param>
        /// <param name="objectID">Will be null if item is offered from an object</param>
        /// <param name="fromTask">will be true of item is offered from an object</param>
        /// <returns>Return UUID of destination folder to accept offer, UUID.Zero to decline it.</returns>
        public delegate UUID ObjectOfferedCallback(InstantMessage offerDetails, AssetType type, UUID objectID, bool fromTask);

        /// <summary>
        /// Callback when an inventory object is accepted and received from a
        /// task inventory. This is the callback in which you actually get
        /// the ItemID, as in ObjectOfferedCallback it is null when received
        /// from a task.
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="folderID"></param>
        /// <param name="creatorID"></param>
        /// <param name="assetID"></param>
        /// <param name="type"></param>
        public delegate void TaskItemReceivedCallback(UUID itemID, UUID folderID, UUID creatorID,
            UUID assetID, InventoryType type);

        /// <summary>
        /// Delegate for use with the <seealso cref="FindObjectsByPath"/> and 
        /// <seealso cref="RequestFindObjectsByMath"/> methods. Raised when the path
        /// is resolved to a UUID.
        /// </summary>
        /// <param name="path">A string representing the path to the UUID, with '/' seperators.</param>
        /// <param name="inventoryObjectID">The item's UUID.</param>
        public delegate void FindObjectByPathCallback(string path, UUID inventoryObjectID);

        /// <summary>
        /// Reply received after calling <code>RequestTaskInventory</code>,
        /// contains a filename that can be used in an asset download request
        /// </summary>
        /// <param name="itemID">UUID of the inventory item</param>
        /// <param name="serial">Version number of the task inventory asset</param>
        /// <param name="assetFilename">Filename of the task inventory asset</param>
        public delegate void TaskInventoryReplyCallback(UUID itemID, short serial, string assetFilename);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="success"></param>
        /// <param name="status"></param>
        /// <param name="itemID"></param>
        /// <param name="assetID"></param>
        public delegate void NotecardUploadedAssetCallback(bool success, string status, UUID itemID, UUID assetID);

        #endregion Delegates

        #region Events
        /// <summary>
        /// Raised when the inventory and library skeletons are received.
        /// <seealso cref="InventorySkeleton"/> and <seealso cref="LibrarySkeleton"/>
        /// </summary>
        public event SkeletonsReceived OnSkeletonsReceived;
        public event AssetUpdate OnAssetUpdate;

        public event ItemCreatedCallback OnItemCreated;

        /// <summary>
        /// Fired when a BulkUpdateInventory packet is received containing item data.
        /// </summary>
        /// <seealso cref="InventoryManager.RequestFetchInventory"/>
        public event ItemUpdate OnItemUpdate;

        /// <summary>
        /// Fired when a BulkUpdateInventory packet is received containing folder data.
        /// </summary>
        public event FolderUpdate OnFolderUpdate;

        /// <summary>
        /// Fired when an object or another avatar offers us an inventory item
        /// </summary>
        public event ObjectOfferedCallback OnObjectOffered;

        /// <summary>
        /// Fired when a task inventory item is received
        /// 
        /// This may occur when an object that's rezzed in world is
        /// taken into inventory, when an item is created using the CreateInventoryItem
        /// packet, or when an object is purchased
        /// </summary>
        public event TaskItemReceivedCallback OnTaskItemReceived;

        /// <summary>
        /// Fired in response to a request for a tasks (primitive) inventory
        /// </summary>
        /// <seealso cref="InventoryManager.GetTaskInventory"/>
        /// <seealso cref="InventoryManager.RequestTaskInventory"/>
        public event TaskInventoryReplyCallback OnTaskInventoryReply;

        #endregion Events

        private GridClient _Client;
        private NetworkManager _Network;
        private AgentManager _Agents;
        private InventorySkeleton _InventorySkeleton;
        private InventorySkeleton _LibrarySkeleton;
        private Random _RandNumbers = new Random();
        private object _CallbacksLock = new object();
        private uint _CallbackPos;
        private Dictionary<uint, ItemCreatedCallback> _ItemCreatedCallbacks = new Dictionary<uint, ItemCreatedCallback>();
        private Dictionary<uint, ItemCopiedCallback> _ItemCopiedCallbacks = new Dictionary<uint, ItemCopiedCallback>();
        private List<DescendentsRequest> _DescendentsRequests = new List<DescendentsRequest>();
        private List<FetchRequest> _FetchRequests = new List<FetchRequest>();

        #region String Arrays

        /// <summary>Partial mapping of AssetTypes to folder names</summary>
        private static readonly string[] _NewFolderNames = new string[]
        {
            "Textures",
            "Sounds",
            "Calling Cards",
            "Landmarks",
            "Scripts",
            "Clothing",
            "Objects",
            "Notecards",
            "New Folder",
            "Inventory",
            "Scripts",
            "Scripts",
            "Uncompressed Images",
            "Body Parts",
            "Trash",
            "Photo Album",
            "Lost And Found",
            "Uncompressed Sounds",
            "Uncompressed Images",
            "Uncompressed Images",
            "Animations",
            "Gestures"
        };

        private static readonly string[] _AssetTypeNames = new string[]
        {
            "texture",
	        "sound",
	        "callcard",
	        "landmark",
	        "script",
	        "clothing",
	        "object",
	        "notecard",
	        "category",
	        "root",
	        "lsltext",
	        "lslbyte",
	        "txtr_tga",
	        "bodypart",
	        "trash",
	        "snapshot",
	        "lstndfnd",
	        "snd_wav",
	        "img_tga",
	        "jpeg",
	        "animatn",
	        "gesture",
	        "simstate"
        };

        private static readonly string[] _InventoryTypeNames = new string[]
        {
            "texture",
	        "sound",
	        "callcard",
	        "landmark",
	        String.Empty,
	        String.Empty,
	        "object",
	        "notecard",
	        "category",
	        "root",
	        "script",
	        String.Empty,
	        String.Empty,
	        String.Empty,
	        String.Empty,
	        "snapshot",
	        String.Empty,
	        "attach",
	        "wearable",
	        "animation",
	        "gesture",
        };

        private static readonly string[] _SaleTypeNames = new string[]
        {
            "not",
            "orig",
            "copy",
            "cntn"
        };

        #endregion String Arrays

        #region Properties

        public InventorySkeleton LibrarySkeleton
        {
            get { return _LibrarySkeleton; }
            set { _LibrarySkeleton = value; }
        }
        public InventorySkeleton InventorySkeleton
        {
            get { return _InventorySkeleton; }
            set { _InventorySkeleton = value; }
        }

        #endregion Properties

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">Reference to the SecondLife client</param>
        /// 
        public InventoryManager(GridClient client)
            : this(client, client.Network, client.Self) { }

        public InventoryManager(GridClient client, NetworkManager network, AgentManager agents)
        {
            _Client = client;
            _Network = network;
            _Agents = agents;
            _Network.RegisterCallback(PacketType.UpdateCreateInventoryItem, new NetworkManager.PacketCallback(UpdateCreateInventoryItemHandler));
            _Network.RegisterCallback(PacketType.SaveAssetIntoInventory, new NetworkManager.PacketCallback(SaveAssetIntoInventoryHandler));
            _Network.RegisterCallback(PacketType.BulkUpdateInventory, new NetworkManager.PacketCallback(BulkUpdateInventoryHandler));
            _Network.RegisterCallback(PacketType.InventoryDescendents, new NetworkManager.PacketCallback(InventoryDescendentsHandler));
            _Network.RegisterCallback(PacketType.FetchInventoryReply, new NetworkManager.PacketCallback(FetchInventoryReplyHandler));
            _Network.RegisterCallback(PacketType.ReplyTaskInventory, new NetworkManager.PacketCallback(ReplyTaskInventoryHandler));

            // Watch for inventory given to us through instant message
            _Agents.OnInstantMessage += new AgentManager.InstantMessageCallback(Self_OnInstantMessage);

            // Register extra parameters with login and parse the inventory data that comes back
            List<string> options = new List<string>(5);
            if (Settings.ENABLE_INVENTORY_STORE)
            {
                options.Add("inventory-root");
                options.Add("inventory-skeleton");
            }
            if (Settings.ENABLE_LIBRARY_STORE)
            {
                options.Add("inventory-lib-root");
                options.Add("inventory-lib-owner");
                options.Add("inventory-skel-lib");
            }
            if (Settings.ENABLE_INVENTORY_STORE || Settings.ENABLE_LIBRARY_STORE)
            {
                // Register extra parameters with login and parse the inventory data that comes back
                _Network.RegisterLoginResponseCallback(
                    new NetworkManager.LoginResponseCallback(Network_OnLoginResponse),
                    options.ToArray());
            }
        }

        #region Fetch

        /// <summary>
        /// Fetch a single inventory item.
        /// </summary>
        /// <param name="itemID">The item's <seealso cref="UUID"/></param>
        /// <param name="ownerID">The item owner's <seealso cref="UUID"/></param>
        /// <param name="timeout">The amount of time to wait for results.</param>
        /// <param name="item">The item retrieved.</param>
        /// <returns>true if successful, false if not.</returns>
        public bool FetchItem(UUID itemID, UUID ownerID, TimeSpan timeout, out ItemData item)
        {
            List<ItemData> items = FetchItems(new UUID[] { itemID }, ownerID, timeout);
            if (items == null || items.Count == 0)
            {
                item = new ItemData();
                return false;
            }
            else
            {
                item = items[0];
                return true;
            }
        }

        /// <summary>
        /// Fetch an inventory item from the dataserver
        /// </summary>
        /// <param name="itemIDs">The items <seealso cref="UUID"/></param>
        /// <param name="ownerID">The item Owners <seealso cref="OpenMetaverse.UUID"/></param>
        /// <param name="timeout">a TimeSpan representing the amount of time to wait for results</param>
        /// <returns>An <seealso cref="InventoryItem"/> object on success, or null if no item was found</returns>
        /// <remarks>Items will also be sent to the <seealso cref="InventoryManager.OnItemReceived"/> event</remarks>
        public List<ItemData> FetchItems(ICollection<UUID> itemIDs, UUID ownerID, TimeSpan timeout)
        {
            AutoResetEvent fetchEvent = new AutoResetEvent(false);

            List<ItemData> items = null;
            FetchItemsCallback callback =
                delegate(List<ItemData> fetchedItems)
                {
                    items = fetchedItems;
                    fetchEvent.Set();
                };
            RequestFetchItems(itemIDs, ownerID, callback);
            fetchEvent.WaitOne(timeout, false);
            return items;
        }

        /// <summary>
        /// Request inventory items
        /// </summary>
        /// <param name="itemIDs">Inventory items to request</param>
        /// <param name="ownerID">Owners of the inventory items</param>
        /// <param name="callback"></param>
        /// <seealso cref="InventoryManager.OnItemReceived"/>
        public void RequestFetchItems(ICollection<UUID> itemIDs, UUID ownerID, FetchItemsCallback callback)
        {
            FetchRequest request = new FetchRequest(callback, itemIDs);
            lock (_FetchRequests)
                _FetchRequests.Add(request);

            // Send the packet:
            FetchInventoryPacket fetch = new FetchInventoryPacket();
            fetch.AgentData = new FetchInventoryPacket.AgentDataBlock();
            fetch.AgentData.AgentID = _Agents.AgentID;
            fetch.AgentData.SessionID = _Agents.SessionID;

            fetch.InventoryData = new FetchInventoryPacket.InventoryDataBlock[itemIDs.Count];
            int i = 0;
            foreach (UUID item in itemIDs)
            {
                fetch.InventoryData[i] = new FetchInventoryPacket.InventoryDataBlock();
                fetch.InventoryData[i].ItemID = item;
                fetch.InventoryData[i].OwnerID = ownerID;
                ++i;
            }

            _Network.SendPacket(fetch);
        }

        /// <summary>
        /// Get contents of a folder
        /// </summary>
        /// <param name="folder">The <seealso cref="UUID"/> of the folder to search</param>
        /// <param name="owner">The <seealso cref="UUID"/> of the folders owner</param>
        /// <param name="folders">true to retrieve folders</param>
        /// <param name="items">true to retrieve items</param>
        /// <param name="order">sort order to return results in</param>
        /// <param name="timeout">a TimeSpan representing the amount of time to wait for results</param>
        /// <param name="folderContents">A list of FolderData representing the folders contained in the parent folder.</param>
        /// <param name="itemContents">A list of ItemData representing the items contained in the parent folder.</param>
        /// <returns><code>true</code> if successful, <code>false</code> if timed out</returns>
        /// <seealso cref="InventoryManager.RequestFolderContents"/>
        public bool FolderContents(UUID folder, UUID owner, bool folders, bool items,
            InventorySortOrder order, TimeSpan timeout, out List<ItemData> itemContents, out List<FolderData> folderContents)
        {
            AutoResetEvent lockEvent = new AutoResetEvent(false);
            List<FolderData> _folders = null;
            List<ItemData> _items = null;
            FolderContentsCallback callback = new FolderContentsCallback(
                delegate(UUID folderID, List<ItemData> __items, List<FolderData> __folders)
                {
                    if (folderID == folder)
                    {
                        _folders = __folders;
                        _items = __items;
                        lockEvent.Set();
                    }
                });
            RequestFolderContents(folder, owner, folders, items, order, callback);

            bool success = lockEvent.WaitOne(timeout, false);
            itemContents = _items;
            folderContents = _folders;
            return success;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="owner"></param>
        /// <param name="folders"></param>
        /// <param name="items"></param>
        /// <param name="order"></param>
        /// <param name="callback"></param>
        public void RequestFolderContents(UUID folder, UUID owner, bool folders, bool items, InventorySortOrder order, FolderContentsCallback callback)
        {
            RequestFolderContents(folder, owner, folders, items, order, callback, null);
        }

        /// <summary>
        /// Request the contents of an inventory folder, <paramref name="callback"/> is fired when all of the contents are retrieved.
        /// <paramref name="partialCallback"/> is fired as the contents trickle in from the server. 
        /// </summary>
        /// <param name="folder">The folder to search</param>
        /// <param name="owner">The folder owners <seealso cref="UUID"/></param>
        /// <param name="folders">true to return <seealso cref="InventoryManager.InventoryFolder"/>s contained in folder</param>
        /// <param name="items">true to return <seealso cref="InventoryManager.InventoryItem"/>s containd in folder</param>
        /// <param name="order">the sort order to return items in</param>
        /// <param name="callback">The callback to fire when all the contents are received.</param>
        /// <param name="partialCallback">The callback to fire as the contents are received.</param>
        /// <seealso cref="InventoryManager.FolderContents"/>
        public void RequestFolderContents(UUID folder, UUID owner, bool folders, bool items, InventorySortOrder order, FolderContentsCallback callback, PartialContentsCallback partialCallback)
        {
            DescendentsRequest request = new DescendentsRequest(folder, callback);
            request.PartialCallback = partialCallback;
            lock (_DescendentsRequests)
                _DescendentsRequests.Add(request);

            // Send the packet:
            FetchInventoryDescendentsPacket fetch = new FetchInventoryDescendentsPacket();
            fetch.AgentData.AgentID = _Agents.AgentID;
            fetch.AgentData.SessionID = _Agents.SessionID;

            fetch.InventoryData.FetchFolders = folders;
            fetch.InventoryData.FetchItems = items;
            fetch.InventoryData.FolderID = folder;
            fetch.InventoryData.OwnerID = owner;
            fetch.InventoryData.SortOrder = (int)order;

            _Network.SendPacket(fetch);
        }

        #endregion Fetch

        #region Find

        /// <summary>
        /// Returns the UUID of the folder (category) that defaults to
        /// containing 'type'. The folder is not necessarily only for that
        /// type
        /// </summary>
        /// <remarks>This will return the root folder if one does not exist</remarks>
        /// <param name="type"></param>
        /// <returns>The UUID of the desired folder if found, the UUID of the RootFolder
        /// if not found, or UUID.Zero on failure</returns>
        public UUID FindFolderForType(AssetType type)
        {
            if (_InventorySkeleton == null)
            {
                Logger.Log("Inventory skeleton is null, FindFolderForType() lookup cannot continue",
                    Helpers.LogLevel.Error, _Client);
                return UUID.Zero;
            }

            // Folders go in the root
            if (type == AssetType.Folder)
                return _InventorySkeleton.RootUUID;

            // Loop through each top-level directory and check if PreferredType
            // matches the requested type
            foreach (FolderData folder in _InventorySkeleton.Folders)
            {
                if (folder.PreferredType == type)
                    return folder.UUID;
            }

            // No match found, return Root Folder ID
            return _InventorySkeleton.RootUUID;
        }

        /// <summary>
        /// Find an object in inventory using a specific path to search
        /// </summary>
        /// <param name="baseFolder">The folder to begin the search in</param>
        /// <param name="inventoryOwner">The object owners <seealso cref="UUID"/></param>
        /// <param name="path">A string path to search</param>
        /// <param name="timeout">Time to wait for a reply</param>
        /// <returns>Found items <seealso cref="UUID"/> or <seealso cref="UUID.Zero"/> if 
        /// timeout occurs or item is not found</returns>
        public UUID FindObjectByPath(UUID baseFolder, UUID inventoryOwner, string path, TimeSpan timeout)
        {
            AutoResetEvent findEvent = new AutoResetEvent(false);
            UUID foundItem = UUID.Zero;

            FindObjectByPathCallback callback =
                delegate(string thisPath, UUID inventoryObjectID)
                {
                    if (thisPath == path)
                    {
                        foundItem = inventoryObjectID;
                        findEvent.Set();
                    }
                };

            RequestFindObjectByPath(baseFolder, inventoryOwner, path, callback);
            findEvent.WaitOne(timeout, false);

            return foundItem;
        }

        /// <summary>
        /// Find inventory items by path
        /// </summary>
        /// <param name="baseFolder">The folder to begin the search in</param>
        /// <param name="inventoryOwner">The object owners <seealso cref="UUID"/></param>
        /// <param name="path">A string path to search, folders/objects separated by a '/'</param>
        /// <param name="callback">The callback to fire when the path has been found.</param>
        public void RequestFindObjectByPath(UUID baseFolder, UUID inventoryOwner, string path, FindObjectByPathCallback callback)
        {
            if (path == null || path.Length == 0)
                throw new ArgumentException("Empty path is not supported");
            string[] pathArray = path.Split('/');
            RequestFindObjectByPath(baseFolder, inventoryOwner, pathArray, callback);
        }

        /// <summary>
        /// Find inventory items by path.
        /// </summary>
        /// <param name="baseFolder">The folder to begin the search in.</param>
        /// <param name="inventoryOwner">The object owner's <seealso cref="UUID"/></param>
        /// <param name="pathArray">A string array representing a path already split into individual folder names.</param>
        /// <param name="callback">The callback to fire when the path has been found.</param>
        public void RequestFindObjectByPath(UUID baseFolder, UUID inventoryOwner, string[] pathArray, FindObjectByPathCallback callback)
        {
            // Create the RequestFolderContents callback:
            FolderContentsCallback contentsCallback = ConstructFindContentsHandler(String.Join("/", pathArray), pathArray, 0, callback);
            // Start the search
            RequestFolderContents(baseFolder, inventoryOwner, true, true, InventorySortOrder.ByName, contentsCallback);
        }

        /// <summary>
        /// This constructs the callback that RequestFindObjectByPath needs to call RequestFolderContents.
        /// We need to put it in its own method because the callback will need to create another callback for
        /// recursing into child folders.
        /// <param name="path">Used for display purposes only.</param>
        /// <param name="pathArray"></param>
        /// <param name="level"></param>
        /// <param name="callback"></param>
        /// </summary>
        private FolderContentsCallback ConstructFindContentsHandler(string path, string[] pathArray, int level, FindObjectByPathCallback callback)
        {
            return new FolderContentsCallback(
                delegate(UUID folder, List<ItemData> items, List<FolderData> folders)
                {
                    foreach (FolderData folderData in folders)
                    {
                        if (folderData.Name == pathArray[level])
                        {
                            if (level == pathArray.Length - 1)
                            {
                                Logger.DebugLog("Finished path search of " + path, _Client);

                                // This is the last node in the path, fire the callback and clean up
                                callback(path, folderData.UUID);
                            }
                            else
                            {
                                // Construct the callback that will be called to recurse into the child folder.
                                FolderContentsCallback contentsCallback = ConstructFindContentsHandler(path, pathArray, level + 1, callback);
                                RequestFolderContents(folderData.UUID, folderData.OwnerID, true, true, InventorySortOrder.ByName, contentsCallback);
                            }
                        }
                    }
                    foreach (ItemData item in items)
                    {
                        if (item.Name == pathArray[level])
                        {
                            if (level == pathArray.Length - 1)
                            {
                                Logger.DebugLog("Finished path search of " + path, _Client);

                                // This is the last node in the path, fire the callback and clean up
                                callback(path, item.UUID);
                            }
                            else
                            {
                                Logger.Log("Path search attempted to request the contents of an item.", Helpers.LogLevel.Warning, _Client);
                                callback(path, UUID.Zero);
                            }
                        }
                    }
                });
        }

        #endregion Find

        #region Move/Rename


        /// <summary>
        /// Rename a folder.
        /// </summary>
        /// <param name="folderID">The folder's <seealso cref="UUID"/></param>
        /// <param name="parentID">The folder's parent <seealso cref="UUID"/></param>
        /// <param name="newName">The new name of the folder.</param>
        public void RenameFolder(UUID folderID, UUID parentID, string newName)
        {
            UpdateInventoryFolderPacket move = new UpdateInventoryFolderPacket();
            move.AgentData.AgentID = _Agents.AgentID;
            move.AgentData.SessionID = _Agents.SessionID;
            move.FolderData = new UpdateInventoryFolderPacket.FolderDataBlock[1];
            move.FolderData[0] = new UpdateInventoryFolderPacket.FolderDataBlock();
            move.FolderData[0].FolderID = folderID;
            move.FolderData[0].ParentID = parentID;
            move.FolderData[0].Name = Utils.StringToBytes(newName);
            move.FolderData[0].Type = -1;

            _Network.SendPacket(move);
        }

        /// <summary>
        /// Move a folder
        /// </summary>
        /// <param name="folderID">The source folder's <seealso cref="UUID"/></param>
        /// <param name="newParentID">The destination folder's <seealso cref="UUID"/></param>
        public void MoveFolder(UUID folderID, UUID newParentID)
        {
            MoveFolders(new UUID[] { folderID }, newParentID);
        }

        /// <summary>
        /// Move multiple folders.
        /// </summary>
        /// <param name="newParent">The parent to move the folders to.</param>
        /// <param name="folders">The folders to move.</param>
        public void MoveFolders(ICollection<UUID> folders, UUID newParent)
        {
            //TODO: Test if this truly supports multiple-folder move
            MoveInventoryFolderPacket move = new MoveInventoryFolderPacket();
            move.AgentData.AgentID = _Agents.AgentID;
            move.AgentData.SessionID = _Agents.SessionID;
            move.AgentData.Stamp = false; //FIXME: ??

            move.InventoryData = new MoveInventoryFolderPacket.InventoryDataBlock[folders.Count];

            int i = 0;
            foreach (UUID folder in folders)
            {
                MoveInventoryFolderPacket.InventoryDataBlock block = new MoveInventoryFolderPacket.InventoryDataBlock();
                block.FolderID = folder;
                block.ParentID = newParent;
                move.InventoryData[i] = block;
                ++i;
            }

            _Network.SendPacket(move);
        }

        /// <summary>
        /// Rename an inventory item
        /// </summary>
        /// <param name="itemID">The <seealso cref="UUID"/> of the source item to move</param>
        /// <param name="parentID">The <seealso cref="UUID"/> of the item's parent.</param>
        /// <param name="newName">The name to change the folder to</param>
        public void RenameItem(UUID itemID, UUID parentID, string newName)
        {
            MoveInventoryItemPacket move = new MoveInventoryItemPacket();
            move.AgentData.AgentID = _Agents.AgentID;
            move.AgentData.SessionID = _Agents.SessionID;
            move.AgentData.Stamp = false; //FIXME: ??

            move.InventoryData = new MoveInventoryItemPacket.InventoryDataBlock[1];
            move.InventoryData[0] = new MoveInventoryItemPacket.InventoryDataBlock();
            move.InventoryData[0].ItemID = itemID;
            move.InventoryData[0].FolderID = parentID;
            move.InventoryData[0].NewName = Utils.StringToBytes(newName);

            _Network.SendPacket(move);
        }

        /// <summary>
        /// Move an inventory item to a new folder
        /// </summary>
        /// <param name="itemID">The <seealso cref="UUID"/> of the source item to move</param>
        /// <param name="folderID">The <seealso cref="UUID"/> of the destination folder</param>
        public void MoveItem(UUID itemID, UUID folderID)
        {
            MoveItems(new UUID[] { itemID }, folderID);
        }

        /// <summary>
        /// Move multiple inventory items.
        /// </summary>
        /// <param name="newParentID">The <seealso cref="UUID"/> of the new parent.</param>
        /// <param name="items">The <seealso cref="UUID"/>s of the items.</param>
        public void MoveItems(ICollection<UUID> items, UUID newParentID)
        {
            MoveInventoryItemPacket move = new MoveInventoryItemPacket();
            move.AgentData.AgentID = _Agents.AgentID;
            move.AgentData.SessionID = _Agents.SessionID;
            move.AgentData.Stamp = false; //FIXME: ??

            move.InventoryData = new MoveInventoryItemPacket.InventoryDataBlock[items.Count];

            int i = 0;
            foreach (UUID item in items)
            {
                MoveInventoryItemPacket.InventoryDataBlock block = new MoveInventoryItemPacket.InventoryDataBlock();
                block.ItemID = item;
                block.FolderID = newParentID;
                block.NewName = new byte[0];
                move.InventoryData[i] = block;
                ++i;
            }

            _Network.SendPacket(move);
        }

        #endregion Move

        #region Remove

        /// <summary>
        /// Remove descendants of a folder
        /// </summary>
        /// <param name="folder">The <seealso cref="UUID"/> of the folder</param>
        public void RemoveDescendants(UUID folder)
        {
            PurgeInventoryDescendentsPacket purge = new PurgeInventoryDescendentsPacket();
            purge.AgentData.AgentID = _Agents.AgentID;
            purge.AgentData.SessionID = _Agents.SessionID;
            purge.InventoryData.FolderID = folder;
            _Network.SendPacket(purge);
        }

        /// <summary>
        /// Remove a single item from inventory
        /// </summary>
        /// <param name="item">The <seealso cref="UUID"/> of the inventory item to remove</param>
        public void RemoveItem(UUID item)
        {
            List<UUID> items = new List<UUID>(1);
            items.Add(item);

            Remove(items, null);
        }

        /// <summary>
        /// Remove a folder from inventory
        /// </summary>
        /// <param name="folder">The <seealso cref="UUID"/> of the folder to remove</param>
        public void RemoveFolder(UUID folder)
        {
            List<UUID> folders = new List<UUID>(1);
            folders.Add(folder);

            Remove(null, folders);
        }

        /// <summary>
        /// Remove multiple items or folders from inventory
        /// </summary>
        /// <param name="items">A List containing the <seealso cref="UUID"/>s of items to remove</param>
        /// <param name="folders">A List containing the <seealso cref="UUID"/>s of the folders to remove</param>
        public void Remove(ICollection<UUID> items, ICollection<UUID> folders)
        {
            if ((items == null || items.Count == 0) && (folders == null || folders.Count == 0))
                return;

            RemoveInventoryObjectsPacket rem = new RemoveInventoryObjectsPacket();
            rem.AgentData.AgentID = _Agents.AgentID;
            rem.AgentData.SessionID = _Agents.SessionID;

            if (items == null || items.Count == 0)
            {
                // To indicate that we want no items removed:
                rem.ItemData = new RemoveInventoryObjectsPacket.ItemDataBlock[1];
                rem.ItemData[0] = new RemoveInventoryObjectsPacket.ItemDataBlock();
                rem.ItemData[0].ItemID = UUID.Zero;
            }
            else
            {
                rem.ItemData = new RemoveInventoryObjectsPacket.ItemDataBlock[items.Count];
                int i = 0;
                foreach (UUID item in items)
                {
                    rem.ItemData[i] = new RemoveInventoryObjectsPacket.ItemDataBlock();
                    rem.ItemData[i].ItemID = item;
                    ++i;
                }
            }

            if (folders == null || folders.Count == 0)
            {
                // To indicate we want no folders removed:
                rem.FolderData = new RemoveInventoryObjectsPacket.FolderDataBlock[1];
                rem.FolderData[0] = new RemoveInventoryObjectsPacket.FolderDataBlock();
                rem.FolderData[0].FolderID = UUID.Zero;
            }
            else
            {
                rem.FolderData = new RemoveInventoryObjectsPacket.FolderDataBlock[folders.Count];
                int i = 0;
                foreach (UUID folder in folders)
                {
                    rem.FolderData[i] = new RemoveInventoryObjectsPacket.FolderDataBlock();
                    rem.FolderData[i].FolderID = folder;
                    ++i;
                }
            }
            _Network.SendPacket(rem);
        }

        /// <summary>
        /// Empty the Lost and Found folder
        /// </summary>
        public void EmptyLostAndFound()
        {
            EmptySystemFolder(AssetType.LostAndFoundFolder);
        }

        /// <summary>
        /// Empty the Trash folder
        /// </summary>
        public void EmptyTrash()
        {
            EmptySystemFolder(AssetType.TrashFolder);
        }

        private void EmptySystemFolder(AssetType folderType)
        {
            RemoveDescendants(FindFolderForType(folderType));
        }
        #endregion Remove

        #region Create

        [Obsolete("Wearables must upload an Asset before being created.", false)]
        public void RequestCreateItem(UUID parentFolder, string name, string description, AssetType type,
            InventoryType invType, WearableType wearableType, PermissionMask nextOwnerMask,
            ItemCreatedCallback callback)
        {
            RequestCreateItem(parentFolder, name, description, type, UUID.Zero, invType, wearableType, nextOwnerMask, callback);
        }

        /// <summary>
        /// Creates an inventory item without needing to upload an asset.
        /// In most cases, this means the AssetID of the resulting item is UUID.Zero.
        /// For gestures, the server automatically creates an asset and assigns it an ID.
        /// This is the method the Second Life Client (as of v1.9) uses to create scripts and notecards
        /// </summary>
        /// <param name="parentFolder"><seealso cref="UUID"/> of folder to put item in.</param>
        /// <param name="name">Name of new item.</param>
        /// <param name="description">Description of new item.</param>
        /// <param name="type">Asset type of item.</param>
        /// <param name="invType">Inventory type of item.</param>
        /// <param name="nextOwnerMask">Permissions for the next owner.</param>
        /// <param name="callback">Callback to trigger when item is created.</param>
        public void RequestCreateItem(UUID parentFolder, string name, string description, AssetType type,
            InventoryType invType, PermissionMask nextOwnerMask, ItemCreatedCallback callback)
        {
            // Even though WearableType 0 is Shape, in this context it is treated as NOT_WEARABLE
            RequestCreateItem(parentFolder, name, description, type, UUID.Zero, invType, (WearableType)0, nextOwnerMask,
                callback);
        }

        /// <summary>
        /// Creates an inventory item referenceing an asset upload. This associates
        /// the item with an asset. The resulting ItemData will have the AssetUUID
        /// of the uploaded asset if the upload completed successfully.
        /// This is the method that the Second Life Client (as of v1.9) uses to create gestures.
        /// </summary>
        /// <param name="parentFolder"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="type"></param>
        /// <param name="nextOwnerMask"></param>
        /// <param name="callback"></param>
        /// <param name="invType"></param>
        /// <param name="assetTransactionID">Proper use is to upload the inventory's asset first, then provide the Asset's TransactionID here.</param>
        public void RequestCreateItem(UUID parentFolder, string name, string description, AssetType type, UUID assetTransactionID,
            InventoryType invType, PermissionMask nextOwnerMask, ItemCreatedCallback callback)
        {
            // Even though WearableType 0 is Shape, in this context it is treated as NOT_WEARABLE
            RequestCreateItem(parentFolder, name, description, type, assetTransactionID, invType, (WearableType)0, nextOwnerMask,
                callback);
        }

        /// <summary>
        /// Creates a wearable inventory item referencing an asset upload. 
        /// Second Life v1.9 uses this method to create wearable inventory items.
        /// </summary>
        /// <param name="parentFolder"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="type"></param>
        /// <param name="wearableType"></param>
        /// <param name="invType"></param>
        /// <param name="nextOwnerMask"></param>
        /// <param name="callback"></param>
        /// <param name="assetTransactionID">Proper use is to upload the inventory's asset first, then provide the Asset's TransactionID here.</param>
        public void RequestCreateItem(UUID parentFolder, string name, string description, AssetType type, UUID assetTransactionID,
            InventoryType invType, WearableType wearableType, PermissionMask nextOwnerMask, ItemCreatedCallback callback)
        {
            CreateInventoryItemPacket create = new CreateInventoryItemPacket();
            create.AgentData.AgentID = _Agents.AgentID;
            create.AgentData.SessionID = _Agents.SessionID;

            create.InventoryBlock.CallbackID = RegisterItemCreatedCallback(callback);
            create.InventoryBlock.FolderID = parentFolder;
            create.InventoryBlock.TransactionID = assetTransactionID;
            create.InventoryBlock.NextOwnerMask = (uint)nextOwnerMask;
            create.InventoryBlock.Type = (sbyte)type;
            create.InventoryBlock.InvType = (sbyte)invType;
            create.InventoryBlock.WearableType = (byte)wearableType;
            create.InventoryBlock.Name = Utils.StringToBytes(name);
            create.InventoryBlock.Description = Utils.StringToBytes(description);

            _Network.SendPacket(create);
        }

        /// <summary>
        /// Creates a new inventory folder
        /// </summary>
        /// <param name="parentID">ID of the folder to put this folder in</param>
        /// <param name="name">Name of the folder to create</param>
        /// <returns>The UUID of the newly created folder</returns>
        public UUID CreateFolder(UUID parentID, string name)
        {
            return CreateFolder(parentID, name, AssetType.Unknown);
        }

        /// <summary>
        /// Creates a new inventory folder
        /// </summary>
        /// <param name="parentID">ID of the folder to put this folder in</param>
        /// <param name="name">Name of the folder to create</param>
        /// <param name="preferredType">Sets this folder as the default folder
        /// for new assets of the specified type. Use <code>AssetType.Unknown</code>
        /// to create a normal folder, otherwise it will likely create a
        /// duplicate of an existing folder type</param>
        /// <returns>The UUID of the newly created folder</returns>
        /// <remarks>If you specify a preferred type of <code>AsseType.Folder</code>
        /// it will create a new root folder which may likely cause all sorts
        /// of strange problems</remarks>
        public UUID CreateFolder(UUID parentID, string name, AssetType preferredType)
        {
            UUID id = UUID.Random();

            // Assign a folder name if one is not already set
            if (String.IsNullOrEmpty(name))
            {
                if (preferredType >= AssetType.Texture && preferredType <= AssetType.Gesture)
                {
                    name = _NewFolderNames[(int)preferredType];
                }
                else
                {
                    name = "New Folder";
                }
            }

            // Create the create folder packet and send it
            CreateInventoryFolderPacket create = new CreateInventoryFolderPacket();
            create.AgentData.AgentID = _Agents.AgentID;
            create.AgentData.SessionID = _Agents.SessionID;

            create.FolderData.FolderID = id;
            create.FolderData.ParentID = parentID;
            create.FolderData.Type = (sbyte)preferredType;
            create.FolderData.Name = Utils.StringToBytes(name);

            _Network.SendPacket(create);

            return id;
        }

        public void RequestCreateItemFromAsset(byte[] data, string name, string description, AssetType assetType,
            InventoryType invType, UUID folderID, CapsClient.ProgressCallback progCallback, ItemCreatedFromAssetCallback callback)
        {
            if (_Network.CurrentSim == null || _Network.CurrentSim.Caps == null)
                throw new Exception("NewFileAgentInventory capability is not currently available");

            Uri url = _Network.CurrentSim.Caps.CapabilityURI("NewFileAgentInventory");

            if (url != null)
            {
                LLSDMap query = new LLSDMap();
                query.Add("folder_id", LLSD.FromUUID(folderID));
                query.Add("asset_type", LLSD.FromString(AssetTypeToString(assetType)));
                query.Add("inventory_type", LLSD.FromString(InventoryTypeToString(invType)));
                query.Add("name", LLSD.FromString(name));
                query.Add("description", LLSD.FromString(description));

                // Make the request
                CapsClient request = new CapsClient(url);
                request.OnComplete += new CapsClient.CompleteCallback(CreateItemFromAssetResponse);
                request.UserData = new object[] { progCallback, callback, data };

                request.StartRequest(query);
            }
            else
            {
                throw new Exception("NewFileAgentInventory capability is not currently available");
            }
        }

        #endregion Create

        #region Copy

        public bool CopyItem(UUID itemUUID, UUID newParent, string newName, TimeSpan timeout, out ItemData copy)
        {
            ManualResetEvent mre = new ManualResetEvent(false);
            ItemData _copy = new ItemData();
            ItemCopiedCallback callback =
                delegate(ItemData item)
                {
                    _copy = item;
                    mre.Set();
                };
            RequestCopyItem(itemUUID, newParent, newName, callback);
            if (mre.WaitOne(timeout, false))
            {
                copy = _copy;
                return true;
            }
            else
            {
                copy = _copy;
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="newParent"></param>
        /// <param name="newName"></param>
        /// <param name="callback"></param>
        public void RequestCopyItem(UUID item, UUID newParent, string newName, ItemCopiedCallback callback)
        {
            RequestCopyItem(item, newParent, newName, _Agents.AgentID, callback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="newParent"></param>
        /// <param name="newName"></param>
        /// <param name="oldOwnerID"></param>
        /// <param name="callback"></param>
        public void RequestCopyItem(UUID item, UUID newParent, string newName, UUID oldOwnerID,
            ItemCopiedCallback callback)
        {
            List<UUID> items = new List<UUID>(1);
            items.Add(item);

            List<UUID> folders = new List<UUID>(1);
            folders.Add(newParent);

            List<string> names = new List<string>(1);
            names.Add(newName);

            RequestCopyItems(items, folders, names, oldOwnerID, callback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="targetFolders"></param>
        /// <param name="newNames"></param>
        /// <param name="oldOwnerID"></param>
        /// <param name="callback"></param>
        public void RequestCopyItems(IList<UUID> items, IList<UUID> targetFolders, IList<string> newNames,
            UUID oldOwnerID, ItemCopiedCallback callback)
        {
            if (items.Count != targetFolders.Count || (newNames != null && items.Count != newNames.Count))
                throw new ArgumentException("All list arguments must have an equal number of entries");

            uint callbackID = RegisterItemsCopiedCallback(callback);

            CopyInventoryItemPacket copy = new CopyInventoryItemPacket();
            copy.AgentData.AgentID = _Agents.AgentID;
            copy.AgentData.SessionID = _Agents.SessionID;

            copy.InventoryData = new CopyInventoryItemPacket.InventoryDataBlock[items.Count];
            for (int i = 0; i < items.Count; ++i)
            {
                copy.InventoryData[i] = new CopyInventoryItemPacket.InventoryDataBlock();
                copy.InventoryData[i].CallbackID = callbackID;
                copy.InventoryData[i].NewFolderID = targetFolders[i];
                copy.InventoryData[i].OldAgentID = oldOwnerID;
                copy.InventoryData[i].OldItemID = items[i];

                if (newNames != null && !String.IsNullOrEmpty(newNames[i]))
                    copy.InventoryData[i].NewName = Utils.StringToBytes(newNames[i]);
                else
                    copy.InventoryData[i].NewName = new byte[0];
            }

            _Network.SendPacket(copy);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectID"></param>
        /// <param name="notecardID"></param>
        /// <param name="folderID"></param>
        /// <param name="itemID"></param>
        public void RequestCopyItemFromNotecard(UUID objectID, UUID notecardID, UUID folderID, UUID itemID)
        {
            CopyInventoryFromNotecardPacket copy = new CopyInventoryFromNotecardPacket();
            copy.AgentData.AgentID = _Agents.AgentID;
            copy.AgentData.SessionID = _Agents.SessionID;

            copy.NotecardData.ObjectID = objectID;
            copy.NotecardData.NotecardItemID = notecardID;

            copy.InventoryData = new CopyInventoryFromNotecardPacket.InventoryDataBlock[1];
            copy.InventoryData[0] = new CopyInventoryFromNotecardPacket.InventoryDataBlock();
            copy.InventoryData[0].FolderID = folderID;
            copy.InventoryData[0].ItemID = itemID;

            _Network.SendPacket(copy);
        }

        #endregion Copy

        #region Update

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        public void RequestUpdateItem(ItemData parameters)
        {
            RequestUpdateItems(new ItemData[] { parameters }, UUID.Random());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        public void RequestUpdateItems(ICollection<ItemData> items)
        {
            RequestUpdateItems(items, UUID.Random());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="transactionID"></param>
        public void RequestUpdateItems(ICollection<ItemData> items, UUID transactionID)
        {
            UpdateInventoryItemPacket update = new UpdateInventoryItemPacket();
            update.AgentData.AgentID = _Agents.AgentID;
            update.AgentData.SessionID = _Agents.SessionID;
            update.AgentData.TransactionID = transactionID;

            update.InventoryData = new UpdateInventoryItemPacket.InventoryDataBlock[items.Count];
            int index = 0;
            foreach (ItemData item in items)
            {
                UpdateInventoryItemPacket.InventoryDataBlock block = new UpdateInventoryItemPacket.InventoryDataBlock();
                block.BaseMask = (uint)item.Permissions.BaseMask;
                block.CRC = ItemCRC(item);
                block.CreationDate = (int)Utils.DateTimeToUnixTime(item.CreationDate);
                block.CreatorID = item.CreatorID;
                block.Description = Utils.StringToBytes(item.Description);
                block.EveryoneMask = (uint)item.Permissions.EveryoneMask;
                block.Flags = (uint)item.Flags;
                block.FolderID = item.ParentUUID;
                block.GroupID = item.GroupID;
                block.GroupMask = (uint)item.Permissions.GroupMask;
                block.GroupOwned = item.GroupOwned;
                block.InvType = (sbyte)item.InventoryType;
                block.ItemID = item.UUID;
                block.Name = Utils.StringToBytes(item.Name);
                block.NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
                block.OwnerID = item.OwnerID;
                block.OwnerMask = (uint)item.Permissions.OwnerMask;
                block.SalePrice = item.SalePrice;
                block.SaleType = (byte)item.SaleType;
                block.TransactionID = UUID.Zero;
                block.Type = (sbyte)item.AssetType;

                update.InventoryData[index] = block;
                ++index;
            }

            _Network.SendPacket(update);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="notecardID"></param>
        /// <param name="callback"></param>
        public void RequestUploadNotecardAsset(byte[] data, UUID notecardID, NotecardUploadedAssetCallback callback)
        {
            if (_Network.CurrentSim == null || _Network.CurrentSim.Caps == null)
                throw new Exception("UpdateNotecardAgentInventory capability is not currently available");

            Uri url = _Network.CurrentSim.Caps.CapabilityURI("UpdateNotecardAgentInventory");

            if (url != null)
            {
                LLSDMap query = new LLSDMap();
                query.Add("item_id", LLSD.FromUUID(notecardID));

                byte[] postData = StructuredData.LLSDParser.SerializeXmlBytes(query);

                // Make the request
                CapsClient request = new CapsClient(url);
                request.OnComplete += new CapsClient.CompleteCallback(UploadNotecardAssetResponse);
                request.UserData = new object[2] { new KeyValuePair<NotecardUploadedAssetCallback, byte[]>(callback, data), notecardID };
                request.StartRequest(postData);
            }
            else
            {
                throw new Exception("UpdateNotecardAgentInventory capability is not currently available");
            }
        }
        #endregion Update

        #region Rez/Give

        /// <summary>
        /// Rez an object from inventory
        /// </summary>
        /// <param name="simulator">Simulator to place object in</param>
        /// <param name="rotation">Rotation of the object when rezzed</param>
        /// <param name="position">Vector of where to place object</param>
        /// <param name="item">InventoryObject object containing item details</param>
        public UUID RequestRezFromInventory(Simulator simulator, Quaternion rotation, Vector3 position,
            ItemData item)
        {
            return RequestRezFromInventory(simulator, rotation, position, item, _Agents.ActiveGroup,
                UUID.Random(), false);
        }

        /// <summary>
        /// Rez an object from inventory
        /// </summary>
        /// <param name="simulator">Simulator to place object in</param>
        /// <param name="rotation">Rotation of the object when rezzed</param>
        /// <param name="position">Vector of where to place object</param>
        /// <param name="item">InventoryObject object containing item details</param>
        /// <param name="groupOwner">UUID of group to own the object</param>
        public UUID RequestRezFromInventory(Simulator simulator, Quaternion rotation, Vector3 position,
            ItemData item, UUID groupOwner)
        {
            return RequestRezFromInventory(simulator, rotation, position, item, groupOwner, UUID.Random(), false);
        }

        /// <summary>
        /// Rez an object from inventory
        /// </summary>
        /// <param name="simulator">Simulator to place object in</param>
        /// <param name="rotation">Rotation of the object when rezzed</param>
        /// <param name="position">Vector of where to place object</param>
        /// <param name="item">InventoryObject object containing item details</param>
        /// <param name="groupOwner">UUID of group to own the object</param>        
        /// <param name="queryID">User defined queryID to correlate replies</param>
        /// <param name="requestObjectDetails">if set to true the simulator
        /// will automatically send object detail packet(s) back to the client</param>
        public UUID RequestRezFromInventory(Simulator simulator, Quaternion rotation, Vector3 position,
            ItemData item, UUID groupOwner, UUID queryID, bool requestObjectDetails)
        {
            RezObjectPacket add = new RezObjectPacket();

            add.AgentData.AgentID = _Agents.AgentID;
            add.AgentData.SessionID = _Agents.SessionID;
            add.AgentData.GroupID = groupOwner;

            add.RezData.FromTaskID = UUID.Zero;
            add.RezData.BypassRaycast = 1;
            add.RezData.RayStart = position;
            add.RezData.RayEnd = position;
            add.RezData.RayTargetID = UUID.Zero;
            add.RezData.RayEndIsIntersection = false;
            add.RezData.RezSelected = requestObjectDetails;
            add.RezData.RemoveItem = false;
            add.RezData.ItemFlags = (uint)item.Flags;
            add.RezData.GroupMask = (uint)item.Permissions.GroupMask;
            add.RezData.EveryoneMask = (uint)item.Permissions.EveryoneMask;
            add.RezData.NextOwnerMask = (uint)item.Permissions.NextOwnerMask;

            add.InventoryData.ItemID = item.UUID;
            add.InventoryData.FolderID = item.ParentUUID;
            add.InventoryData.CreatorID = item.CreatorID;
            add.InventoryData.OwnerID = item.OwnerID;
            add.InventoryData.GroupID = item.GroupID;
            add.InventoryData.BaseMask = (uint)item.Permissions.BaseMask;
            add.InventoryData.OwnerMask = (uint)item.Permissions.OwnerMask;
            add.InventoryData.GroupMask = (uint)item.Permissions.GroupMask;
            add.InventoryData.EveryoneMask = (uint)item.Permissions.EveryoneMask;
            add.InventoryData.NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
            add.InventoryData.GroupOwned = item.GroupOwned;
            add.InventoryData.TransactionID = queryID;
            add.InventoryData.Type = (sbyte)item.InventoryType;
            add.InventoryData.InvType = (sbyte)item.InventoryType;
            add.InventoryData.Flags = (uint)item.Flags;
            add.InventoryData.SaleType = (byte)item.SaleType;
            add.InventoryData.SalePrice = item.SalePrice;
            add.InventoryData.Name = Utils.StringToBytes(item.Name);
            add.InventoryData.Description = Utils.StringToBytes(item.Description);
            add.InventoryData.CreationDate = (int)Utils.DateTimeToUnixTime(item.CreationDate);

            _Network.SendPacket(add, simulator);

            return queryID;
        }

        /// <summary>
        /// DeRez an object from the simulator to the agents Objects folder in the agents Inventory
        /// </summary>
        /// <param name="objectLocalID">The simulator Local ID of the object</param>
        public void RequestDeRezToInventory(uint objectLocalID)
        {
            RequestDeRezToInventory(objectLocalID, DeRezDestination.ObjectsFolder,
                _Client.Inventory.FindFolderForType(AssetType.Object), UUID.Random());
        }

        /// <summary>
        /// DeRez an object from the simulator and return to inventory
        /// </summary>
        /// <param name="objectLocalID">The simulator Local ID of the object</param>
        /// <param name="destType">The type of destination from the <seealso cref="DeRezDestination"/> enum</param>
        /// <param name="destFolder">The destination inventory folders <seealso cref="UUID"/> -or- 
        /// if DeRezzing object to a tasks Inventory, the Tasks <seealso cref="UUID"/></param>
        /// <param name="transactionID">The transaction ID for this request which
        /// can be used to correlate this request with other packets</param>
        public void RequestDeRezToInventory(uint objectLocalID, DeRezDestination destType, UUID destFolder, UUID transactionID)
        {
            DeRezObjectPacket take = new DeRezObjectPacket();

            take.AgentData.AgentID = _Agents.AgentID;
            take.AgentData.SessionID = _Agents.SessionID;
            take.AgentBlock = new DeRezObjectPacket.AgentBlockBlock();
            take.AgentBlock.GroupID = UUID.Zero;
            take.AgentBlock.Destination = (byte)destType;
            take.AgentBlock.DestinationID = destFolder;
            take.AgentBlock.PacketCount = 1;
            take.AgentBlock.PacketNumber = 1;
            take.AgentBlock.TransactionID = transactionID;

            take.ObjectData = new DeRezObjectPacket.ObjectDataBlock[1];
            take.ObjectData[0] = new DeRezObjectPacket.ObjectDataBlock();
            take.ObjectData[0].ObjectLocalID = objectLocalID;

            _Network.SendPacket(take);
        }


        /// <summary>
        /// Give an inventory item to another avatar
        /// </summary>
        /// <param name="itemID">The <seealso cref="UUID"/> of the item to give</param>
        /// <param name="itemName">The name of the item</param>
        /// <param name="assetType">The type of the item from the <seealso cref="AssetType"/> enum</param>
        /// <param name="recipient">The <seealso cref="UUID"/> of the recipient</param>
        /// <param name="doEffect">true to generate a beameffect during transfer</param>
        public void GiveItem(UUID itemID, string itemName, AssetType assetType, UUID recipient,
            bool doEffect)
        {
            byte[] bucket;


            bucket = new byte[17];
            bucket[0] = (byte)assetType;
            Buffer.BlockCopy(itemID.GetBytes(), 0, bucket, 1, 16);

            _Agents.InstantMessage(
                    _Agents.Name,
                    recipient,
                    itemName,
                    UUID.Random(),
                    InstantMessageDialog.InventoryOffered,
                    InstantMessageOnline.Online,
                    _Agents.SimPosition,
                    _Network.CurrentSim.ID,
                    bucket);

            if (doEffect)
            {
                _Agents.BeamEffect(_Agents.AgentID, recipient, Vector3d.Zero,
                    _Client.Settings.DEFAULT_EFFECT_COLOR, 1f, UUID.Random());
            }
        }

        /// <summary>
        /// Give an inventory Folder with contents to another avatar
        /// This calls the synchronous <seealso cref="FolderContents"/> method which blocks until
        /// the folder's contents are retrieved, so it might take a while to return. 
        /// For an alternative, specify the folder's contents explicitly using the other
        /// <seealso cref="GiveFolder"/> method.
        /// </summary>
        /// <param name="folderID">The <seealso cref="UUID"/> of the Folder to give</param>
        /// <param name="folderName">The name of the folder</param>
        /// <param name="assetType">The type of the item from the <seealso cref="AssetType"/> enum</param>
        /// <param name="recipient">The <seealso cref="UUID"/> of the recipient</param>
        /// <param name="doEffect">true to generate a beameffect during transfer</param>
        public void GiveFolder(UUID folderID, string folderName, AssetType assetType, UUID recipient,
            bool doEffect)
        {
            List<ItemData> folderContents;
            List<FolderData> placeholder;

            FolderContents(folderID, _Agents.AgentID, false, true, InventorySortOrder.ByDate,
                TimeSpan.FromMilliseconds(1000 * 15),
                out folderContents, out placeholder);

            GiveFolder(folderID, folderName, assetType, recipient, doEffect, folderContents);
        }


        public void GiveFolder(UUID folderID, string folderName, AssetType assetType, UUID recipient,
            bool doEffect, ICollection<ItemData> folderContents)
        {
            byte[] bucket;


            bucket = new byte[17 * (folderContents.Count + 1)];
            //Add parent folder (first item in bucket)
            bucket[0] = (byte)assetType;
            Buffer.BlockCopy(folderID.GetBytes(), 0, bucket, 1, 16);

            //Add contents to bucket after folder
            int index = 1;
            foreach (ItemData item in folderContents)
            {
                bucket[index * 17] = (byte)item.AssetType;
                Buffer.BlockCopy(item.UUID.GetBytes(), 0, bucket, index * 17 + 1, 16);
                ++index;
            }

            _Agents.InstantMessage(
                    _Agents.Name,
                    recipient,
                    folderName,
                    UUID.Random(),
                    InstantMessageDialog.InventoryOffered,
                    InstantMessageOnline.Online,
                    _Agents.SimPosition,
                    _Network.CurrentSim.ID,
                    bucket);

            if (doEffect)
            {
                _Agents.BeamEffect(_Agents.AgentID, recipient, Vector3d.Zero,
                    _Client.Settings.DEFAULT_EFFECT_COLOR, 1f, UUID.Random());
            }
        }

        #endregion Rez/Give

        #region Task

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectLocalID"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public UUID UpdateTaskInventory(uint objectLocalID, ItemData item)
        {
            UUID transactionID = UUID.Random();

            UpdateTaskInventoryPacket update = new UpdateTaskInventoryPacket();
            update.AgentData.AgentID = _Agents.AgentID;
            update.AgentData.SessionID = _Agents.SessionID;
            update.UpdateData.Key = 0;
            update.UpdateData.LocalID = objectLocalID;

            update.InventoryData.ItemID = item.UUID;
            update.InventoryData.FolderID = item.ParentUUID;
            update.InventoryData.CreatorID = item.CreatorID;
            update.InventoryData.OwnerID = item.OwnerID;
            update.InventoryData.GroupID = item.GroupID;
            update.InventoryData.BaseMask = (uint)item.Permissions.BaseMask;
            update.InventoryData.OwnerMask = (uint)item.Permissions.OwnerMask;
            update.InventoryData.GroupMask = (uint)item.Permissions.GroupMask;
            update.InventoryData.EveryoneMask = (uint)item.Permissions.EveryoneMask;
            update.InventoryData.NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
            update.InventoryData.GroupOwned = item.GroupOwned;
            update.InventoryData.TransactionID = transactionID;
            update.InventoryData.Type = (sbyte)item.AssetType;
            update.InventoryData.InvType = (sbyte)item.InventoryType;
            update.InventoryData.Flags = (uint)item.Flags;
            update.InventoryData.SaleType = (byte)item.SaleType;
            update.InventoryData.SalePrice = item.SalePrice;
            update.InventoryData.Name = Utils.StringToBytes(item.Name);
            update.InventoryData.Description = Utils.StringToBytes(item.Description);
            update.InventoryData.CreationDate = (int)Utils.DateTimeToUnixTime(item.CreationDate);
            update.InventoryData.CRC = ItemCRC(item);

            _Network.SendPacket(update);

            return transactionID;
        }

        /// <summary>
        /// Get the inventory of a Task (Primitive)
        /// </summary>
        /// <param name="objectID">The tasks <seealso cref="UUID"/></param>
        /// <param name="objectLocalID">The tasks simulator local ID</param>
        /// <param name="timeout">Time to wait for reply from simulator</param>
        /// <param name="items"></param>
        /// <param name="folders"></param>
        /// <returns>A List containing the inventory items inside the task</returns>
        public void GetTaskInventory(UUID objectID, uint objectLocalID, TimeSpan timeout, out List<ItemData> items, out List<FolderData> folders)
        {
            string filename = null;
            AutoResetEvent taskReplyEvent = new AutoResetEvent(false);

            TaskInventoryReplyCallback callback =
                delegate(UUID itemID, short serial, string assetFilename)
                {
                    if (itemID == objectID)
                    {
                        filename = assetFilename;
                        taskReplyEvent.Set();
                    }
                };

            OnTaskInventoryReply += callback;

            RequestTaskInventory(objectLocalID);

            if (taskReplyEvent.WaitOne(timeout, false))
            {
                OnTaskInventoryReply -= callback;

                if (!String.IsNullOrEmpty(filename))
                {
                    byte[] assetData = null;
                    ulong xferID = 0;
                    AutoResetEvent taskDownloadEvent = new AutoResetEvent(false);

                    AssetManager.XferReceivedCallback xferCallback =
                        delegate(XferDownload xfer)
                        {
                            if (xfer.XferID == xferID)
                            {
                                assetData = xfer.AssetData;
                                taskDownloadEvent.Set();
                            }
                        };

                    _Client.Assets.OnXferReceived += xferCallback;

                    // Start the actual asset xfer
                    xferID = _Client.Assets.RequestAssetXfer(filename, true, false, UUID.Zero, AssetType.Unknown);

                    if (taskDownloadEvent.WaitOne(timeout, false))
                    {
                        _Client.Assets.OnXferReceived -= xferCallback;

                        string taskList = Utils.BytesToString(assetData);
                        ParseTaskInventory(this, taskList, out items, out folders);
                        return;
                    }
                    else
                    {
                        Logger.Log("Timed out waiting for task inventory download for " + filename, Helpers.LogLevel.Warning, _Client);
                        _Client.Assets.OnXferReceived -= xferCallback;
                        items = null;
                        folders = null;
                        return;
                    }
                }
                else
                {
                    Logger.DebugLog("Task is empty for " + objectLocalID, _Client);
                    items = null;
                    folders = null;
                    return;
                }
            }
            else
            {
                Logger.Log("Timed out waiting for task inventory reply for " + objectLocalID, Helpers.LogLevel.Warning, _Client);
                OnTaskInventoryReply -= callback;
                items = null;
                folders = null;
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectLocalID"></param>
        public void RequestTaskInventory(uint objectLocalID)
        {
            RequestTaskInventory(objectLocalID, _Network.CurrentSim);
        }

        /// <summary>
        /// Request the contents of a tasks (primitives) inventory
        /// </summary>
        /// <param name="objectLocalID">The simulator Local ID of the object</param>
        /// <param name="simulator">A reference to the simulator object that contains the object</param>
        public void RequestTaskInventory(uint objectLocalID, Simulator simulator)
        {
            RequestTaskInventoryPacket request = new RequestTaskInventoryPacket();
            request.AgentData.AgentID = _Agents.AgentID;
            request.AgentData.SessionID = _Agents.SessionID;
            request.InventoryData.LocalID = objectLocalID;

            _Network.SendPacket(request, simulator);
        }

        /// <summary>
        /// Moves an Item from an objects (Prim) Inventory to the specified folder in the avatars inventory
        /// </summary>
        /// <param name="objectLocalID">LocalID of the object in the simulator</param>
        /// <param name="taskItemID">UUID of the task item to move</param>
        /// <param name="inventoryFolderID">UUID of the folder to move the item to</param>
        /// <param name="simulator">Simulator Object</param>
        public void MoveTaskInventory(uint objectLocalID, UUID taskItemID, UUID inventoryFolderID, Simulator simulator)
        {
            MoveTaskInventoryPacket request = new MoveTaskInventoryPacket();
            request.AgentData.AgentID = _Agents.AgentID;
            request.AgentData.SessionID = _Agents.SessionID;

            request.AgentData.FolderID = inventoryFolderID;

            request.InventoryData.ItemID = taskItemID;
            request.InventoryData.LocalID = objectLocalID;

            _Network.SendPacket(request, simulator);
        }

        /// <summary>
        /// Remove an item from an objects (Prim) Inventory
        /// </summary>
        /// <param name="objectLocalID">LocalID of the object in the simulator</param>
        /// <param name="taskItemID">UUID of the task item to remove</param>
        /// <param name="simulator">Simulator Object</param>
        public void RemoveTaskInventory(uint objectLocalID, UUID taskItemID, Simulator simulator)
        {
            RemoveTaskInventoryPacket remove = new RemoveTaskInventoryPacket();
            remove.AgentData.AgentID = _Agents.AgentID;
            remove.AgentData.SessionID = _Agents.SessionID;

            remove.InventoryData.ItemID = taskItemID;
            remove.InventoryData.LocalID = objectLocalID;

            _Network.SendPacket(remove, simulator);
        }

        #endregion Task

        #region Helper Functions

        /// <summary>
        /// Takes an AssetType and returns the string representation
        /// </summary>
        /// <param name="type">The source <seealso cref="AssetType"/></param>
        /// <returns>The string version of the AssetType</returns>
        public static string AssetTypeToString(AssetType type)
        {
            return _AssetTypeNames[(int)type];
        }

        /// <summary>
        /// Translate a string name of an AssetType into the proper Type
        /// </summary>
        /// <param name="type">A string containing the AssetType name</param>
        /// <returns>The AssetType which matches the string name, or AssetType.Unknown if no match was found</returns>
        public static AssetType StringToAssetType(string type)
        {
            for (int i = 0; i < _AssetTypeNames.Length; i++)
            {
                if (_AssetTypeNames[i] == type)
                    return (AssetType)i;
            }

            return AssetType.Unknown;
        }

        /// <summary>
        /// Convert an InventoryType to a string
        /// </summary>
        /// <param name="type">The <seealso cref="T:InventoryType"/> to convert</param>
        /// <returns>A string representation of the source </returns>
        public static string InventoryTypeToString(InventoryType type)
        {
            return _InventoryTypeNames[(int)type];
        }

        /// <summary>
        /// Convert a string into a valid InventoryType
        /// </summary>
        /// <param name="type">A string representation of the InventoryType to convert</param>
        /// <returns>A InventoryType object which matched the type</returns>
        public static InventoryType StringToInventoryType(string type)
        {
            for (int i = 0; i < _InventoryTypeNames.Length; i++)
            {
                if (_InventoryTypeNames[i] == type)
                    return (InventoryType)i;
            }

            return InventoryType.Unknown;
        }

        public static string SaleTypeToString(SaleType type)
        {
            return _SaleTypeNames[(int)type];
        }

        public static SaleType StringToSaleType(string value)
        {
            for (int i = 0; i < _SaleTypeNames.Length; i++)
            {
                if (value == _SaleTypeNames[i])
                    return (SaleType)i;
            }

            return SaleType.Not;
        }

        private uint RegisterItemCreatedCallback(ItemCreatedCallback callback)
        {
            lock (_CallbacksLock)
            {
                if (_CallbackPos == UInt32.MaxValue)
                    _CallbackPos = 0;

                _CallbackPos++;

                if (_ItemCreatedCallbacks.ContainsKey(_CallbackPos))
                    Logger.Log("Overwriting an existing ItemCreatedCallback", Helpers.LogLevel.Warning, _Client);

                _ItemCreatedCallbacks[_CallbackPos] = callback;

                return _CallbackPos;
            }
        }

        private uint RegisterItemsCopiedCallback(ItemCopiedCallback callback)
        {
            lock (_CallbacksLock)
            {
                if (_CallbackPos == UInt32.MaxValue)
                    _CallbackPos = 0;

                _CallbackPos++;

                if (_ItemCopiedCallbacks.ContainsKey(_CallbackPos))
                    Logger.Log("Overwriting an existing ItemsCopiedCallback", Helpers.LogLevel.Warning, _Client);

                _ItemCopiedCallbacks[_CallbackPos] = callback;

                return _CallbackPos;
            }
        }

        /// <summary>
        /// Create a CRC from an InventoryItem
        /// </summary>
        /// <param name="iitem">The source InventoryItem</param>
        /// <returns>A uint representing the source InventoryItem as a CRC</returns>
        public static uint ItemCRC(ItemData iitem)
        {
            uint CRC = 0;

            // IDs
            CRC += iitem.AssetUUID.CRC(); // AssetID
            CRC += iitem.ParentUUID.CRC(); // FolderID
            CRC += iitem.UUID.CRC(); // ItemID

            // Permission stuff
            CRC += iitem.CreatorID.CRC(); // CreatorID
            CRC += iitem.OwnerID.CRC(); // OwnerID
            CRC += iitem.GroupID.CRC(); // GroupID

            // CRC += another 4 words which always seem to be zero -- unclear if this is a UUID or what
            CRC += (uint)iitem.Permissions.OwnerMask; //owner_mask;      // Either owner_mask or next_owner_mask may need to be
            CRC += (uint)iitem.Permissions.NextOwnerMask; //next_owner_mask; // switched with base_mask -- 2 values go here and in my
            CRC += (uint)iitem.Permissions.EveryoneMask; //everyone_mask;   // study item, the three were identical.
            CRC += (uint)iitem.Permissions.GroupMask; //group_mask;

            // The rest of the CRC fields
            CRC += (uint)iitem.Flags; // Flags
            CRC += (uint)iitem.InventoryType; // InvType
            CRC += (uint)iitem.AssetType; // Type 
            CRC += (uint)Utils.DateTimeToUnixTime(iitem.CreationDate); // CreationDate
            CRC += (uint)iitem.SalePrice;    // SalePrice
            CRC += (uint)((uint)iitem.SaleType * 0x07073096); // SaleType

            return CRC;
        }

        private static bool ParseLine(string line, out string key, out string value)
        {
            string origLine = line;

            // Clean up and convert tabs to spaces
            line = line.Trim();
            line = line.Replace('\t', ' ');

            // Shrink all whitespace down to single spaces
            while (line.IndexOf("  ") > 0)
                line = line.Replace("  ", " ");

            if (line.Length > 2)
            {
                int sep = line.IndexOf(' ');
                if (sep > 0)
                {
                    key = line.Substring(0, sep);
                    value = line.Substring(sep + 1);

                    return true;
                }
            }
            else if (line.Length == 1)
            {
                key = line;
                value = String.Empty;
                return true;
            }

            key = null;
            value = null;
            return false;
        }

        /// <summary>
        /// Parse the results of a RequestTaskInventory() response
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="taskData">A string which contains the data from the task reply</param>
        /// <param name="items"></param>
        /// <param name="folders"></param>
        /// <returns>A List containing the items contained within the tasks inventory</returns>
        public static void ParseTaskInventory(InventoryManager manager, string taskData, out List<ItemData> items, out List<FolderData> folders)
        {
            items = new List<ItemData>();
            folders = new List<FolderData>();
            int lineNum = 0;
            string[] lines = taskData.Replace("\r\n", "\n").Split('\n');

            while (lineNum < lines.Length)
            {
                string key, value;
                if (ParseLine(lines[lineNum++], out key, out value))
                {
                    if (key == "inv_object")
                    {
                        #region inv_object

                        // In practice this appears to only be used for folders
                        UUID itemID = UUID.Zero;
                        UUID parentID = UUID.Zero;
                        string name = String.Empty;
                        AssetType assetType = AssetType.Unknown;

                        while (lineNum < lines.Length)
                        {
                            if (ParseLine(lines[lineNum++], out key, out value))
                            {
                                if (key == "{")
                                {
                                    continue;
                                }
                                else if (key == "}")
                                {
                                    break;
                                }
                                else if (key == "obj_id")
                                {
                                    UUID.TryParse(value, out itemID);
                                }
                                else if (key == "parent_id")
                                {
                                    UUID.TryParse(value, out parentID);
                                }
                                else if (key == "type")
                                {
                                    assetType = StringToAssetType(value);
                                }
                                else if (key == "name")
                                {
                                    name = value.Substring(0, value.IndexOf('|'));
                                }
                            }
                        }

                        if (assetType == AssetType.Folder)
                        {
                            FolderData folderData = new FolderData(itemID);
                            folderData.Name = name;
                            folderData.ParentUUID = parentID;

                            folders.Add(folderData);
                        }
                        else
                        {
                            ItemData itemParams = new ItemData(itemID);
                            itemParams.Name = name;
                            itemParams.ParentUUID = parentID;
                            itemParams.AssetType = assetType;
                            items.Add(itemParams);
                        }

                        #endregion inv_object
                    }
                    else if (key == "inv_item")
                    {
                        #region inv_item

                        // Any inventory item that links to an assetID, has permissions, etc
                        UUID itemID = UUID.Zero;
                        UUID assetID = UUID.Zero;
                        UUID parentID = UUID.Zero;
                        UUID creatorID = UUID.Zero;
                        UUID ownerID = UUID.Zero;
                        UUID lastOwnerID = UUID.Zero;
                        UUID groupID = UUID.Zero;
                        bool groupOwned = false;
                        string name = String.Empty;
                        string desc = String.Empty;
                        AssetType assetType = AssetType.Unknown;
                        InventoryType inventoryType = InventoryType.Unknown;
                        DateTime creationDate = Utils.Epoch;
                        uint flags = 0;
                        Permissions perms = Permissions.NoPermissions;
                        SaleType saleType = SaleType.Not;
                        int salePrice = 0;

                        while (lineNum < lines.Length)
                        {
                            if (ParseLine(lines[lineNum++], out key, out value))
                            {
                                if (key == "{")
                                {
                                    continue;
                                }
                                else if (key == "}")
                                {
                                    break;
                                }
                                else if (key == "item_id")
                                {
                                    UUID.TryParse(value, out itemID);
                                }
                                else if (key == "parent_id")
                                {
                                    UUID.TryParse(value, out parentID);
                                }
                                else if (key == "permissions")
                                {
                                    #region permissions

                                    while (lineNum < lines.Length)
                                    {
                                        if (ParseLine(lines[lineNum++], out key, out value))
                                        {
                                            if (key == "{")
                                            {
                                                continue;
                                            }
                                            else if (key == "}")
                                            {
                                                break;
                                            }
                                            else if (key == "creator_mask")
                                            {
                                                // Deprecated
                                                uint val;
                                                if (Utils.TryParseHex(value, out val))
                                                    perms.BaseMask = (PermissionMask)val;
                                            }
                                            else if (key == "base_mask")
                                            {
                                                uint val;
                                                if (Utils.TryParseHex(value, out val))
                                                    perms.BaseMask = (PermissionMask)val;
                                            }
                                            else if (key == "owner_mask")
                                            {
                                                uint val;
                                                if (Utils.TryParseHex(value, out val))
                                                    perms.OwnerMask = (PermissionMask)val;
                                            }
                                            else if (key == "group_mask")
                                            {
                                                uint val;
                                                if (Utils.TryParseHex(value, out val))
                                                    perms.GroupMask = (PermissionMask)val;
                                            }
                                            else if (key == "everyone_mask")
                                            {
                                                uint val;
                                                if (Utils.TryParseHex(value, out val))
                                                    perms.EveryoneMask = (PermissionMask)val;
                                            }
                                            else if (key == "next_owner_mask")
                                            {
                                                uint val;
                                                if (Utils.TryParseHex(value, out val))
                                                    perms.NextOwnerMask = (PermissionMask)val;
                                            }
                                            else if (key == "creator_id")
                                            {
                                                UUID.TryParse(value, out creatorID);
                                            }
                                            else if (key == "owner_id")
                                            {
                                                UUID.TryParse(value, out ownerID);
                                            }
                                            else if (key == "last_owner_id")
                                            {
                                                UUID.TryParse(value, out lastOwnerID);
                                            }
                                            else if (key == "group_id")
                                            {
                                                UUID.TryParse(value, out groupID);
                                            }
                                            else if (key == "group_owned")
                                            {
                                                uint val;
                                                if (UInt32.TryParse(value, out val))
                                                    groupOwned = (val != 0);
                                            }
                                        }
                                    }

                                    #endregion permissions
                                }
                                else if (key == "sale_info")
                                {
                                    #region sale_info

                                    while (lineNum < lines.Length)
                                    {
                                        if (ParseLine(lines[lineNum++], out key, out value))
                                        {
                                            if (key == "{")
                                            {
                                                continue;
                                            }
                                            else if (key == "}")
                                            {
                                                break;
                                            }
                                            else if (key == "sale_type")
                                            {
                                                saleType = StringToSaleType(value);
                                            }
                                            else if (key == "sale_price")
                                            {
                                                Int32.TryParse(value, out salePrice);
                                            }
                                        }
                                    }

                                    #endregion sale_info
                                }
                                else if (key == "shadow_id")
                                {
                                    //FIXME:
                                }
                                else if (key == "asset_id")
                                {
                                    UUID.TryParse(value, out assetID);
                                }
                                else if (key == "type")
                                {
                                    assetType = StringToAssetType(value);
                                }
                                else if (key == "inv_type")
                                {
                                    inventoryType = StringToInventoryType(value);
                                }
                                else if (key == "flags")
                                {
                                    UInt32.TryParse(value, out flags);
                                }
                                else if (key == "name")
                                {
                                    name = value.Substring(0, value.IndexOf('|'));
                                }
                                else if (key == "desc")
                                {
                                    desc = value.Substring(0, value.IndexOf('|'));
                                }
                                else if (key == "creation_date")
                                {
                                    uint timestamp;
                                    if (UInt32.TryParse(value, out timestamp))
                                        creationDate = Utils.UnixTimeToDateTime(timestamp);
                                    else
                                        Logger.Log("Failed to parse creation_date " + value, Helpers.LogLevel.Warning);
                                }
                            }
                        }
                        ItemData item = new ItemData(itemID, inventoryType);
                        item.AssetUUID = assetID;
                        item.AssetType = assetType;
                        item.CreationDate = creationDate;
                        item.CreatorID = creatorID;
                        item.Description = desc;
                        item.Flags = flags;
                        item.GroupID = groupID;
                        item.GroupOwned = groupOwned;
                        item.Name = name;
                        item.OwnerID = ownerID;
                        item.ParentUUID = parentID;
                        item.Permissions = perms;
                        item.SalePrice = salePrice;
                        item.SaleType = saleType;
                        items.Add(item);

                        #endregion inv_item
                    }
                    else
                    {
                        Logger.Log("Unrecognized token " + key + " in: " + Helpers.NewLine + taskData,
                            Helpers.LogLevel.Error);
                    }
                }
            }
        }

        #endregion Helper Functions

        #region Callbacks

        private void CreateItemFromAssetResponse(CapsClient client, LLSD result, Exception error)
        {
            object[] args = (object[])client.UserData;
            CapsClient.ProgressCallback progCallback = (CapsClient.ProgressCallback)args[0];
            ItemCreatedFromAssetCallback callback = (ItemCreatedFromAssetCallback)args[1];
            byte[] itemData = (byte[])args[2];

            LLSDMap contents = (LLSDMap)result;

            if (result == null)
            {
                try { callback(false, error.Message, UUID.Zero, UUID.Zero); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                return;
            }

            string status = contents["state"].AsString().ToLower();

            if (status == "upload")
            {
                string uploadURL = contents["uploader"].AsString();

                Logger.DebugLog("CreateItemFromAsset: uploading to " + uploadURL);

                // This makes the assumption that all uploads go to CurrentSim, to avoid
                // the problem of HttpRequestState not knowing anything about simulators
                CapsClient upload = new CapsClient(new Uri(uploadURL));
                upload.OnProgress += progCallback;
                upload.OnComplete += new CapsClient.CompleteCallback(CreateItemFromAssetResponse);
                upload.UserData = new object[] { null, callback, itemData };
                upload.StartRequest(itemData, "application/octet-stream");
            }
            else if (status == "complete")
            {
                Logger.DebugLog("CreateItemFromAsset: completed");

                if (contents.ContainsKey("new_inventory_item") && contents.ContainsKey("new_asset"))
                {
                    try { callback(true, String.Empty, contents["new_inventory_item"].AsUUID(), contents["new_asset"].AsUUID()); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }

                    // Notify everyone of the creation.
                    // TODO: Is there a way to avoid fetching the whole thing?
                    FetchItemsCallback fetchCallback =
                        delegate(List<ItemData> items)
                        {
                            if (items.Count > 0)
                            {
                                ItemData item = items[0];
                                item.AssetUUID = contents["new_asset"].AsUUID();
                                try { OnItemCreated(true, item); }
                                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                            }
                        };
                    RequestFetchItems(new UUID[] { contents["new_inventory_item"].AsUUID() }, _Agents.AgentID, fetchCallback);
                }
                else
                {
                    try { callback(false, "Failed to parse asset and item UUIDs", UUID.Zero, UUID.Zero); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                }
            }
            else
            {
                // Failure
                try { callback(false, status, UUID.Zero, UUID.Zero); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
            }
        }

        private void SaveAssetIntoInventoryHandler(Packet packet, Simulator simulator)
        {
            SaveAssetIntoInventoryPacket save = (SaveAssetIntoInventoryPacket)packet;
            if (OnAssetUpdate != null)
            {
                try { OnAssetUpdate(save.InventoryData.ItemID, save.InventoryData.NewAssetID); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
            }
        }

        private void InventoryDescendentsHandler(Packet packet, Simulator simulator)
        {
            InventoryDescendentsPacket reply = (InventoryDescendentsPacket)packet;
            ItemData[] items = null;
            FolderData[] folders = null;
            if (reply.AgentData.Descendents > 0)
            {
                // InventoryDescendantsReply sends a null folder if the parent doesnt contain any folders
                if (reply.FolderData[0].FolderID != UUID.Zero)
                {
                    folders = new FolderData[reply.FolderData.Length];
                    // Iterate folders in this packet
                    for (int i = 0; i < reply.FolderData.Length; i++)
                    {
                        UUID folderID = reply.FolderData[i].FolderID;
                        FolderData folder = new FolderData(folderID);
                        folder.ParentUUID = reply.FolderData[i].ParentID;
                        folder.Name = Utils.BytesToString(reply.FolderData[i].Name);
                        folder.PreferredType = (AssetType)reply.FolderData[i].Type;
                        folder.OwnerID = reply.AgentData.OwnerID;
                        folders[i] = folder;
                    }
                }

                // InventoryDescendantsReply sends a null item if the parent doesnt contain any items.
                if (reply.ItemData[0].ItemID != UUID.Zero)
                {
                    items = new ItemData[reply.ItemData.Length];
                    // Iterate items in this packet
                    for (int i = 0; i < reply.ItemData.Length; i++)
                    {
                        if (reply.ItemData[i].ItemID != UUID.Zero)
                        {
                            UUID itemID = reply.ItemData[i].ItemID;
                            ItemData item = new ItemData(itemID);
                            /* 
                             * Objects that have been attached in-world prior to being stored on the 
                             * asset server are stored with the InventoryType of 0 (Texture) 
                             * instead of 17 (Attachment) 
                             * 
                             * This corrects that behavior by forcing Object Asset types that have an 
                             * invalid InventoryType with the proper InventoryType of Attachment.
                             */
                            if ((AssetType)reply.ItemData[i].Type == AssetType.Object
                                && (InventoryType)reply.ItemData[i].InvType == InventoryType.Texture)
                            {
                                item.InventoryType = InventoryType.Attachment;
                            }
                            else
                            {
                                item.InventoryType = (InventoryType)reply.ItemData[i].InvType;
                            }

                            item.ParentUUID = reply.ItemData[i].FolderID;
                            item.CreatorID = reply.ItemData[i].CreatorID;
                            item.AssetType = (AssetType)reply.ItemData[i].Type;
                            item.AssetUUID = reply.ItemData[i].AssetID;
                            item.CreationDate = Utils.UnixTimeToDateTime((uint)reply.ItemData[i].CreationDate);
                            item.Description = Utils.BytesToString(reply.ItemData[i].Description);
                            item.Flags = reply.ItemData[i].Flags;
                            item.Name = Utils.BytesToString(reply.ItemData[i].Name);
                            item.GroupID = reply.ItemData[i].GroupID;
                            item.GroupOwned = reply.ItemData[i].GroupOwned;
                            item.Permissions = new Permissions(
                                reply.ItemData[i].BaseMask,
                                reply.ItemData[i].EveryoneMask,
                                reply.ItemData[i].GroupMask,
                                reply.ItemData[i].NextOwnerMask,
                                reply.ItemData[i].OwnerMask);
                            item.SalePrice = reply.ItemData[i].SalePrice;
                            item.SaleType = (SaleType)reply.ItemData[i].SaleType;
                            item.OwnerID = reply.AgentData.OwnerID;
                            items[i] = item;
                        }
                    }
                }
            }

            #region FolderContents Handling
            if (_DescendentsRequests.Count > 0)
            {
                lock (_DescendentsRequests)
                {
                    // Iterate backwards, ensures safe removal:
                    for (int i = _DescendentsRequests.Count - 1; i >= 0; --i)
                    {
                        DescendentsRequest request = _DescendentsRequests[i];
                        if (request.Folder == reply.AgentData.FolderID)
                        {
                            // Store the descendent count if we haven't received a responce yet:
                            if (!request.ReceivedResponse)
                            {
                                request.ReceivedResponse = true;
                                request.Descendents = reply.AgentData.Descendents;
                            }

                            // Store the items and folders:
                            if (folders != null)
                                request.FolderContents.AddRange(folders);
                            if (items != null)
                                request.ItemContents.AddRange(items);

                            _DescendentsRequests[i] = request;

                            int contentsReceived = request.FolderContents.Count + request.ItemContents.Count;

                            // Fire the partial callback, if we have one:
                            if (request.PartialCallback != null)
                            {
                                request.PartialCallback(reply.AgentData.FolderID, items, folders, request.Descendents - contentsReceived);
                            }

                            // Check if we're done:
                            if (contentsReceived >= request.Descendents)
                            {
                                // Fire the callback:
                                if (request.Callback != null)
                                {
                                    request.Callback(reply.AgentData.FolderID, request.ItemContents, request.FolderContents);
                                }
                                _DescendentsRequests.RemoveAt(i);
                            }
                        }
                    }
                }
            }
            #endregion FolderContents Handling
        }

        /// <summary>
        /// UpdateCreateInventoryItem packets are received when a new inventory item 
        /// is created. This may occur when an object that's rezzed in world is
        /// taken into inventory, when an item is created using the CreateInventoryItem
        /// packet, or when an object is purchased
        /// </summary>
        private void UpdateCreateInventoryItemHandler(Packet packet, Simulator simulator)
        {
            UpdateCreateInventoryItemPacket reply = packet as UpdateCreateInventoryItemPacket;

            foreach (UpdateCreateInventoryItemPacket.InventoryDataBlock dataBlock in reply.InventoryData)
            {
                if (dataBlock.InvType == (sbyte)InventoryType.Folder)
                {
                    Logger.Log("Received InventoryFolder in an UpdateCreateInventoryItem packet, this should not happen!",
                        Helpers.LogLevel.Error, _Client);
                    continue;
                }

                ItemData item = new ItemData(dataBlock.ItemID, (InventoryType)dataBlock.InvType);
                item.AssetType = (AssetType)dataBlock.Type;
                item.AssetUUID = dataBlock.AssetID;
                item.CreationDate = Utils.UnixTimeToDateTime(dataBlock.CreationDate);
                item.CreatorID = dataBlock.CreatorID;
                item.Description = Utils.BytesToString(dataBlock.Description);
                item.Flags = dataBlock.Flags;
                item.GroupID = dataBlock.GroupID;
                item.GroupOwned = dataBlock.GroupOwned;
                item.Name = Utils.BytesToString(dataBlock.Name);
                item.OwnerID = dataBlock.OwnerID;
                item.ParentUUID = dataBlock.FolderID;
                item.Permissions = new Permissions(
                        dataBlock.BaseMask,
                        dataBlock.EveryoneMask,
                        dataBlock.GroupMask,
                        dataBlock.NextOwnerMask,
                        dataBlock.OwnerMask);
                item.SalePrice = dataBlock.SalePrice;
                item.SaleType = (SaleType)dataBlock.SaleType;

                // Look for an "item created" callback
                // Let the requester know that its item was created.
                ItemCreatedCallback createdCallback;
                if (_ItemCreatedCallbacks.TryGetValue(dataBlock.CallbackID, out createdCallback))
                {
                    _ItemCreatedCallbacks.Remove(dataBlock.CallbackID);

                    try { createdCallback(true, item); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                }

                // Let everyone know that the item was created
                if (OnItemCreated != null)
                {
                    try { OnItemCreated(true, item); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                }

                //This is triggered when an item is received from a task
                if (OnTaskItemReceived != null)
                {
                    try
                    {
                        OnTaskItemReceived(dataBlock.ItemID, dataBlock.FolderID, item.CreatorID, item.AssetUUID,
                            item.InventoryType);
                    }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                }
            }
        }

        private void BulkUpdateInventoryHandler(Packet packet, Simulator simulator)
        {
            BulkUpdateInventoryPacket update = packet as BulkUpdateInventoryPacket;

            if (update.FolderData.Length > 0 && update.FolderData[0].FolderID != UUID.Zero)
            {
                foreach (BulkUpdateInventoryPacket.FolderDataBlock dataBlock in update.FolderData)
                {
                    if (OnFolderUpdate != null)
                    {
                        FolderData folderParams = new FolderData();
                        folderParams.Name = Utils.BytesToString(dataBlock.Name);
                        folderParams.OwnerID = update.AgentData.AgentID;
                        folderParams.ParentUUID = dataBlock.ParentID;

                        try { OnFolderUpdate(folderParams); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                    }
                }
            }

            if (update.ItemData.Length > 0 && update.ItemData[0].ItemID != UUID.Zero)
            {
                for (int i = 0; i < update.ItemData.Length; i++)
                {
                    BulkUpdateInventoryPacket.ItemDataBlock dataBlock = update.ItemData[i];

                    ItemData item = new ItemData(dataBlock.ItemID, (InventoryType)dataBlock.InvType);
                    item.AssetType = (AssetType)dataBlock.Type;
                    if (dataBlock.AssetID != UUID.Zero) item.AssetUUID = dataBlock.AssetID;
                    item.CreationDate = Utils.UnixTimeToDateTime(dataBlock.CreationDate);
                    item.CreatorID = dataBlock.CreatorID;
                    item.Description = Utils.BytesToString(dataBlock.Description);
                    item.Flags = dataBlock.Flags;
                    item.GroupID = dataBlock.GroupID;
                    item.GroupOwned = dataBlock.GroupOwned;
                    item.Name = Utils.BytesToString(dataBlock.Name);
                    item.OwnerID = dataBlock.OwnerID;
                    item.ParentUUID = dataBlock.FolderID;
                    item.Permissions = new Permissions(
                        dataBlock.BaseMask,
                        dataBlock.EveryoneMask,
                        dataBlock.GroupMask,
                        dataBlock.NextOwnerMask,
                        dataBlock.OwnerMask);
                    item.SalePrice = dataBlock.SalePrice;
                    item.SaleType = (SaleType)dataBlock.SaleType;

                    // Look for an "item created" callback
                    ItemCreatedCallback callback;
                    if (_ItemCreatedCallbacks.TryGetValue(dataBlock.CallbackID, out callback))
                    {
                        _ItemCreatedCallbacks.Remove(dataBlock.CallbackID);

                        try { callback(true, item); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                    }

                    // Look for an "item copied" callback
                    ItemCopiedCallback copyCallback;
                    if (_ItemCopiedCallbacks.TryGetValue(dataBlock.CallbackID, out copyCallback))
                    {
                        _ItemCopiedCallbacks.Remove(dataBlock.CallbackID);

                        try { copyCallback(item); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                    }

                    if (OnItemUpdate != null)
                    {
                        try { OnItemUpdate(item); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                    }
                }
            }
        }

        private void FetchInventoryReplyHandler(Packet packet, Simulator simulator)
        {
            lock (_FetchRequests)
            {
                FetchInventoryReplyPacket reply = packet as FetchInventoryReplyPacket;
                foreach (FetchInventoryReplyPacket.InventoryDataBlock dataBlock in reply.InventoryData)
                {
                    if (dataBlock.InvType == (sbyte)InventoryType.Folder)
                    {
                        Logger.Log("Received FetchInventoryReply for an inventory folder, this should not happen!",
                            Helpers.LogLevel.Error, _Client);
                        continue;
                    }

                    ItemData item = new ItemData(dataBlock.ItemID);
                    item.InventoryType = (InventoryType)dataBlock.InvType;
                    item.AssetType = (AssetType)dataBlock.Type;
                    item.AssetUUID = dataBlock.AssetID;
                    item.CreationDate = Utils.UnixTimeToDateTime(dataBlock.CreationDate);
                    item.CreatorID = dataBlock.CreatorID;
                    item.Description = Utils.BytesToString(dataBlock.Description);
                    item.Flags = dataBlock.Flags;
                    item.GroupID = dataBlock.GroupID;
                    item.GroupOwned = dataBlock.GroupOwned;
                    item.Name = Utils.BytesToString(dataBlock.Name);
                    item.OwnerID = dataBlock.OwnerID;
                    item.ParentUUID = dataBlock.FolderID;
                    item.Permissions = new Permissions(
                        dataBlock.BaseMask,
                        dataBlock.EveryoneMask,
                        dataBlock.GroupMask,
                        dataBlock.NextOwnerMask,
                        dataBlock.OwnerMask);
                    item.SalePrice = dataBlock.SalePrice;
                    item.SaleType = (SaleType)dataBlock.SaleType;

                    #region FetchItems Handling
                    // Iterate backwards through fetch requests, ensures safe removal:
                    for (int i = _FetchRequests.Count - 1; i >= 0; --i)
                    {
                        FetchRequest request = _FetchRequests[i];
                        if (request.RequestedItems.ContainsKey(item.UUID))
                        {
                            request.StoreFetchedItem(item);
                            if (request.ItemsFetched == request.RequestedItems.Count)
                            {
                                // We're done, create the list that the callback needs:
                                List<ItemData> items = new List<ItemData>(request.ItemsFetched);
                                foreach (KeyValuePair<UUID, ItemData?> pair in request.RequestedItems)
                                    items.Add(pair.Value.Value);

                                // Fire the callback:
                                request.Callback(items);
                                _FetchRequests.RemoveAt(i);
                            }
                            _FetchRequests[i] = request;
                        }
                    }
                    #endregion FetchItems Handling
                }
            }
        }

        private void ReplyTaskInventoryHandler(Packet packet, Simulator simulator)
        {
            if (OnTaskInventoryReply != null)
            {
                ReplyTaskInventoryPacket reply = (ReplyTaskInventoryPacket)packet;

                try
                {
                    OnTaskInventoryReply(reply.InventoryData.TaskID, reply.InventoryData.Serial,
                        Utils.BytesToString(reply.InventoryData.Filename));
                }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
            }
        }

        private void Self_OnInstantMessage(InstantMessage im, Simulator simulator)
        {
            // TODO: MainAvatar.InstantMessageDialog.GroupNotice can also be an inventory offer, should we
            // handle it here?

            if (OnObjectOffered != null &&
                (im.Dialog == InstantMessageDialog.InventoryOffered
                || im.Dialog == InstantMessageDialog.TaskInventoryOffered))
            {
                AssetType type = AssetType.Unknown;
                UUID objectID = UUID.Zero;
                bool fromTask = false;

                if (im.Dialog == InstantMessageDialog.InventoryOffered)
                {
                    if (im.BinaryBucket.Length == 17)
                    {
                        type = (AssetType)im.BinaryBucket[0];
                        objectID = new UUID(im.BinaryBucket, 1);
                        fromTask = false;
                    }
                    else
                    {
                        Logger.Log("Malformed inventory offer from agent", Helpers.LogLevel.Warning, _Client);
                        return;
                    }
                }
                else if (im.Dialog == InstantMessageDialog.TaskInventoryOffered)
                {
                    if (im.BinaryBucket.Length == 1)
                    {
                        type = (AssetType)im.BinaryBucket[0];
                        fromTask = true;
                    }
                    else
                    {
                        Logger.Log("Malformed inventory offer from object", Helpers.LogLevel.Warning, _Client);
                        return;
                    }
                }

                // Fire the callback
                try
                {
                    ImprovedInstantMessagePacket imp = new ImprovedInstantMessagePacket();
                    imp.AgentData.AgentID = _Agents.AgentID;
                    imp.AgentData.SessionID = _Agents.SessionID;
                    imp.MessageBlock.FromGroup = false;
                    imp.MessageBlock.ToAgentID = im.FromAgentID;
                    imp.MessageBlock.Offline = 0;
                    imp.MessageBlock.ID = im.IMSessionID;
                    imp.MessageBlock.Timestamp = 0;
                    imp.MessageBlock.FromAgentName = Utils.StringToBytes(_Agents.Name);
                    imp.MessageBlock.Message = new byte[0];
                    imp.MessageBlock.ParentEstateID = 0;
                    imp.MessageBlock.RegionID = UUID.Zero;
                    imp.MessageBlock.Position = _Agents.SimPosition;

                    UUID destinationFolderID = OnObjectOffered(im, type, objectID, fromTask);
                    if (destinationFolderID != UUID.Zero)
                    {
                        // Accept the inventory offer
                        switch (im.Dialog)
                        {
                            case InstantMessageDialog.InventoryOffered:
                                imp.MessageBlock.Dialog = (byte)InstantMessageDialog.InventoryAccepted;
                                break;
                            case InstantMessageDialog.TaskInventoryOffered:
                                imp.MessageBlock.Dialog = (byte)InstantMessageDialog.TaskInventoryAccepted;
                                break;
                            case InstantMessageDialog.GroupNotice:
                                imp.MessageBlock.Dialog = (byte)InstantMessageDialog.GroupNoticeInventoryAccepted;
                                break;
                        }

                        imp.MessageBlock.BinaryBucket = destinationFolderID.GetBytes();
                    }
                    else
                    {
                        // Decline the inventory offer
                        switch (im.Dialog)
                        {
                            case InstantMessageDialog.InventoryOffered:
                                imp.MessageBlock.Dialog = (byte)InstantMessageDialog.InventoryDeclined;
                                break;
                            case InstantMessageDialog.TaskInventoryOffered:
                                imp.MessageBlock.Dialog = (byte)InstantMessageDialog.TaskInventoryDeclined;
                                break;
                            case InstantMessageDialog.GroupNotice:
                                imp.MessageBlock.Dialog = (byte)InstantMessageDialog.GroupNoticeInventoryDeclined;
                                break;
                        }

                        imp.MessageBlock.BinaryBucket = new byte[0];
                    }

                    _Network.SendPacket(imp, simulator);
                }
                catch (Exception e)
                {
                    Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e);
                }
            }
        }

        private void Network_OnLoginResponse(bool loginSuccess, bool redirect, string message, string reason, LoginResponseData replyData)
        {
            if (loginSuccess)
            {
                if (Settings.ENABLE_INVENTORY_STORE)
                {
                    InventorySkeleton = new InventorySkeleton(replyData.InventoryRoot, replyData.AgentID);
                    InventorySkeleton.Folders = replyData.InventoryFolders;
                }
                if (Settings.ENABLE_LIBRARY_STORE)
                {
                    LibrarySkeleton = new InventorySkeleton(replyData.LibraryRoot, replyData.LibraryOwner);
                    LibrarySkeleton.Folders = replyData.LibraryFolders;
                }
                if (OnSkeletonsReceived != null)
                {
                    try { OnSkeletonsReceived(this); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }

                }
            }
        }

        private void UploadNotecardAssetResponse(CapsClient client, LLSD result, Exception error)
        {
            LLSDMap contents = (LLSDMap)result;
            KeyValuePair<NotecardUploadedAssetCallback, byte[]> kvp = (KeyValuePair<NotecardUploadedAssetCallback, byte[]>)(((object[])client.UserData)[0]);
            NotecardUploadedAssetCallback callback = kvp.Key;
            byte[] itemData = (byte[])kvp.Value;

            string status = contents["state"].AsString();

            if (status == "upload")
            {
                string uploadURL = contents["uploader"].AsString();

                // This makes the assumption that all uploads go to CurrentSim, to avoid
                // the problem of HttpRequestState not knowing anything about simulators
                CapsClient upload = new CapsClient(new Uri(uploadURL));
                upload.OnComplete += new CapsClient.CompleteCallback(UploadNotecardAssetResponse);
                upload.UserData = new object[2] { kvp, (UUID)(((object[])client.UserData)[1]) };
                upload.StartRequest(itemData, "application/octet-stream");
            }
            else if (status == "complete")
            {
                if (contents.ContainsKey("new_asset"))
                {
                    try { callback(true, String.Empty, (UUID)(((object[])client.UserData)[1]), contents["new_asset"].AsUUID()); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                }
                else
                {
                    try { callback(false, "Failed to parse asset and item UUIDs", UUID.Zero, UUID.Zero); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                }
            }
            else
            {
                // Failure
                try { callback(false, status, UUID.Zero, UUID.Zero); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
            }
        }

        #endregion Callbacks
    }
}
