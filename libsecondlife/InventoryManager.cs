/*
 * Copyright (c) 2006-2007, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names
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
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife
{
    #region Enums

    public enum InventoryType : sbyte
    {
        Unknown = -1,
        Texture = 0,
        Sound = 1,
        CallingCard = 2,
        Landmark = 3,
        [Obsolete] Script = 4,
        [Obsolete] Clothing = 5,
        Object = 6,
        Notecard = 7,
        Category = 8,
        Folder = 8,
        RootCategory = 0,
        LSL = 10,
        [Obsolete] LSLBytecode = 11,
        [Obsolete] TextureTGA = 12,
        [Obsolete] Bodypart = 13,
        [Obsolete] Trash = 14,
        Snapshot = 15,
        [Obsolete] LostAndFound = 16,
        Attachment = 17,
        Wearable = 18,
        Animation = 19,
        Gesture = 20
    }

    /// <summary>
    /// 
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

    #endregion Enums

    #region Inventory Object Classes

    public abstract class InventoryBase
    {
        public readonly LLUUID UUID;
        public LLUUID ParentUUID;
        public string Name;
        public LLUUID OwnerID;

        public InventoryBase(LLUUID itemID)
        {
            if (itemID == LLUUID.Zero)
                throw new ArgumentException("Inventory item ID cannot be NULL_KEY (LLUUID.Zero)");
            UUID = itemID;
        }

        public override int GetHashCode()
        {
            return UUID.GetHashCode() ^ ParentUUID.GetHashCode() ^ Name.GetHashCode() ^ OwnerID.GetHashCode();
        }

        public override bool Equals(object o)
        {
            InventoryBase inv = o as InventoryBase;
            return inv != null && Equals(inv);
        }

        public virtual bool Equals(InventoryBase o)
        {
            return o.UUID == UUID
                && o.ParentUUID == ParentUUID
                && o.Name == Name
                && o.OwnerID == OwnerID;
        }
    }

    public class InventoryItem : InventoryBase
    {
        public LLUUID AssetUUID;
        public Permissions Permissions;
        public AssetType AssetType;
        public InventoryType InventoryType;
        public LLUUID CreatorID;
        public string Description;
        public LLUUID GroupID;
        public bool GroupOwned;
        public int SalePrice;
        public SaleType SaleType;
        public uint Flags;
        /// <summary>Time and date this inventory item was created, stored as
        /// UTC (Coordinated Universal Time)</summary>
        public DateTime CreationDate;

        public InventoryItem(LLUUID itemID) 
            : base(itemID) { }

        public InventoryItem(InventoryType type, LLUUID itemID) : base(itemID) { InventoryType = type; }

        public override int GetHashCode()
        {
            return AssetUUID.GetHashCode() ^ Permissions.GetHashCode() ^ AssetType.GetHashCode() ^
                InventoryType.GetHashCode() ^ Description.GetHashCode() ^ GroupID.GetHashCode() ^
                GroupOwned.GetHashCode() ^ SalePrice.GetHashCode() ^ SaleType.GetHashCode() ^
                Flags.GetHashCode() ^ CreationDate.GetHashCode();
        }

        public override bool Equals(object o)
        {
            InventoryItem item = o as InventoryItem;
            return item != null && Equals(item);
        }

        public override bool Equals(InventoryBase o)
        {
            InventoryItem item = o as InventoryItem;
            return item != null && Equals(item);
        }

        public bool Equals(InventoryItem o)
        {
            return base.Equals(o as InventoryBase)
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
    }

    public class InventoryTexture     : InventoryItem { public InventoryTexture(LLUUID itemID) : base(itemID) { InventoryType = InventoryType.Texture; } }
    public class InventorySound       : InventoryItem { public InventorySound(LLUUID itemID) : base(itemID) { InventoryType = InventoryType.Sound; } }
    public class InventoryCallingCard : InventoryItem { public InventoryCallingCard(LLUUID itemID) : base(itemID) { InventoryType = InventoryType.CallingCard; } }
    public class InventoryLandmark    : InventoryItem { public InventoryLandmark(LLUUID itemID) : base(itemID) { InventoryType = InventoryType.Landmark; } }
    public class InventoryObject      : InventoryItem { public InventoryObject(LLUUID itemID) : base(itemID) { InventoryType = InventoryType.Object; } }
    public class InventoryNotecard    : InventoryItem { public InventoryNotecard(LLUUID itemID) : base(itemID) { InventoryType = InventoryType.Notecard; } }
    public class InventoryCategory    : InventoryItem { public InventoryCategory(LLUUID itemID) : base(itemID) { InventoryType = InventoryType.Category; } }
    public class InventoryLSL         : InventoryItem { public InventoryLSL(LLUUID itemID) : base(itemID) { InventoryType = InventoryType.LSL; } }
    public class InventorySnapshot    : InventoryItem { public InventorySnapshot(LLUUID itemID) : base(itemID) { InventoryType = InventoryType.Snapshot; } }
    public class InventoryAttachment  : InventoryItem { public InventoryAttachment(LLUUID itemID) : base(itemID) { InventoryType = InventoryType.Attachment; } }

    public class InventoryWearable : InventoryItem
    {
        public InventoryWearable(LLUUID itemID) : base(itemID) { InventoryType = InventoryType.Wearable; }

        public WearableType WearableType
        {
            get { return (WearableType)Flags; }
            set { Flags = (uint)value; }
        }
    }

    public class InventoryAnimation   : InventoryItem { public InventoryAnimation(LLUUID itemID) : base(itemID) { InventoryType = InventoryType.Animation; } }
    public class InventoryGesture     : InventoryItem { public InventoryGesture(LLUUID itemID) : base(itemID) { InventoryType = InventoryType.Gesture; } }

    public class InventoryFolder : InventoryBase
    {
        public AssetType PreferredType;
        public int Version;
        public int DescendentCount;

        public InventoryFolder(LLUUID itemID)
            : base(itemID) { }

        public override int GetHashCode()
        {
            return PreferredType.GetHashCode() ^ Version.GetHashCode() ^ DescendentCount.GetHashCode();
        }

        public override bool Equals(object o)
        {
            InventoryFolder folder = o as InventoryFolder;
            return folder != null && Equals(folder);
        }

        public override bool Equals(InventoryBase o)
        {
            InventoryFolder folder = o as InventoryFolder;
            return folder != null && Equals(folder);
        }

        public bool Equals(InventoryFolder o)
        {
            return base.Equals(o as InventoryBase)
                && o.DescendentCount == DescendentCount
                && o.PreferredType == PreferredType
                && o.Version == Version;
        }
    }

    #endregion Inventory Object Classes

    public class InventoryManager
    {
        protected struct InventorySearch
        {
            public LLUUID Folder;
            public LLUUID Owner;
            public string[] Path;
            public int Level;
        }

        /// <summary>
        /// Callback for inventory item creation finishing
        /// </summary>
        /// <param name="success">Whether the request to create an inventory
        /// item succeeded or not</param>
        /// <param name="item">Inventory item being created. If success is
        /// false this will be null</param>
        public delegate void ItemCreatedCallback(bool success, InventoryItem item);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="success"></param>
        /// <param name="status"></param>
        /// <param name="itemID"></param>
        /// <param name="assetID"></param>
        public delegate void ItemCreatedFromAssetCallback(bool success, string status, LLUUID itemID, LLUUID assetID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        public delegate void ItemCopiedCallback(InventoryBase item);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public delegate void ItemReceivedCallback(InventoryItem item);
        /// <summary>
        /// Callback for an inventory folder updating
        /// </summary>
        /// <param name="folderID">UUID of the folder that was updated</param>
        public delegate void FolderUpdatedCallback(LLUUID folderID);
        /// <summary>
        /// Callback when an inventory object is received from another avatar
        /// or a primitive
        /// </summary>
        /// <param name="fromAgentID"></param>
        /// <param name="fromAgentName"></param>
        /// <param name="parentEstateID"></param>
        /// <param name="regionID"></param>
        /// <param name="position"></param>
        /// <param name="timestamp"></param>
        /// <param name="type"></param>
        /// <param name="objectID">Will be null if offered from a primitive</param>
        /// <param name="fromTask"></param>
        /// <returns>True to accept the inventory offer, false to reject it</returns>
        public delegate bool ObjectOfferedCallback(LLUUID fromAgentID, string fromAgentName, uint parentEstateID, 
            LLUUID regionID, LLVector3 position, DateTime timestamp, AssetType type, LLUUID objectID, bool fromTask);
        /// <summary>
        /// Callback when an inventory object is accepted and received from a
        /// task inventory. This is the callback in which you actually get
        /// the ItemID, as in ObjectOfferedCallback it is null when received
        /// from a task.
        /// </summary>
        /// <param name="ItemID"></param>
        /// <param name="FolderID"></param>
        /// <param name="CreatorID"></param>
        /// <param name="AssetID"></param>
        public delegate void TaskItemReceivedCallback(LLUUID itemID, LLUUID folderID, LLUUID creatorID, 
            LLUUID assetID, InventoryType type);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="inventoryObjectID"></param>
        public delegate void FindObjectByPathCallback(string path, LLUUID inventoryObjectID);
        /// <summary>
        /// Reply received after calling <code>RequestTaskInventory</code>,
        /// contains a filename that can be used in an asset download request
        /// </summary>
        /// <param name="itemID">UUID of the inventory item</param>
        /// <param name="serial">Version number of the task inventory asset</param>
        /// <param name="assetFilename">Filename of the task inventory asset</param>
        public delegate void TaskInventoryReplyCallback(LLUUID itemID, short serial, string assetFilename);

        public event ItemReceivedCallback OnItemReceived;
        public event FolderUpdatedCallback OnFolderUpdated;
        public event ObjectOfferedCallback OnObjectOffered;
        public event TaskItemReceivedCallback OnTaskItemReceived;
        public event FindObjectByPathCallback OnFindObjectByPath;
        public event TaskInventoryReplyCallback OnTaskInventoryReply;

        private SecondLife _Client;
        private Inventory _Store;
        private Random _RandNumbers = new Random();
        private object _CallbacksLock = new object();
        private uint _CallbackPos;
        private Dictionary<uint, ItemCreatedCallback> _ItemCreatedCallbacks = new Dictionary<uint, ItemCreatedCallback>();
        private Dictionary<uint, ItemCopiedCallback> _ItemCopiedCallbacks = new Dictionary<uint,ItemCopiedCallback>();
        private List<InventorySearch> _Searches = new List<InventorySearch>();

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

        public Inventory Store { get { return _Store; } }

        #endregion Properties

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">Reference to the SecondLife client</param>
        public InventoryManager(SecondLife client)
        {
            _Client = client;
            _Store = new Inventory(client, this);

            _Client.Network.RegisterCallback(PacketType.UpdateCreateInventoryItem, new NetworkManager.PacketCallback(UpdateCreateInventoryItemHandler));
            _Client.Network.RegisterCallback(PacketType.SaveAssetIntoInventory, new NetworkManager.PacketCallback(SaveAssetIntoInventoryHandler));
            _Client.Network.RegisterCallback(PacketType.BulkUpdateInventory, new NetworkManager.PacketCallback(BulkUpdateInventoryHandler));
            _Client.Network.RegisterCallback(PacketType.MoveInventoryItem, new NetworkManager.PacketCallback(MoveInventoryItemHandler));
            _Client.Network.RegisterCallback(PacketType.InventoryDescendents, new NetworkManager.PacketCallback(InventoryDescendentsHandler));
            _Client.Network.RegisterCallback(PacketType.FetchInventoryReply, new NetworkManager.PacketCallback(FetchInventoryReplyHandler));
            _Client.Network.RegisterCallback(PacketType.ReplyTaskInventory, new NetworkManager.PacketCallback(ReplyTaskInventoryHandler));
            // Watch for inventory given to us through instant message
            _Client.Self.OnInstantMessage += new AgentManager.InstantMessageCallback(Self_OnInstantMessage);
            // Register extra parameters with login and parse the inventory data that comes back
            _Client.Network.RegisterLoginResponseCallback(new NetworkManager.LoginResponseCallback(Network_OnLoginResponse), new string[] {"inventory-root", "inventory-skeleton", "inventory-lib-root", "inventory-lib-owner", "inventory-skel-lib"} );
        }


        //public IAsyncResult BeginFindObjects(LLUUID baseFolder, string regex, bool recurse, bool refresh, bool firstOnly, AsyncCallback callback, object asyncState)
        //{
        //    return BeginFindObjects(baseFolder, new Regex(regex), recurse, refresh, firstOnly, callback, asyncState);
        //}

        //public IAsyncResult BeginFindObjects(LLUUID baseFolder, Regex regexp, bool recurse, bool refresh, bool firstOnly, AsyncCallback callback, object asyncState)
        //{
        //    FindResult result = new FindResult(regexp, recurse, callback);
        //    result.FirstOnly = firstOnly;
        //    result.AsyncState = asyncState;
        //    result.FoldersWaiting = 1;
        //    if (refresh)
        //    {
        //        lock (FindDescendantsMap)
        //        {
        //            IAsyncResult descendReq = BeginRequestFolderContents(baseFolder, _Client.Self.AgentID, true, true, recurse && !firstOnly, InventorySortOrder.ByName, new AsyncCallback(SearchDescendantsCallback), baseFolder);
        //            FindDescendantsMap.Add(descendReq, result);
        //        }
        //    }
        //    else
        //    {
        //        result.Result = LocalFind(baseFolder, regexp, recurse, firstOnly);
        //        result.CompletedSynchronously = true;
        //        result.IsCompleted = true;
        //    }
        //    return result;
        //}

        //private List<InventoryBase> LocalFind(LLUUID baseFolder, Regex regexp, bool recurse, bool firstOnly)
        //{
        //    List<InventoryBase> objects = new List<InventoryBase>();
        //    List<InventoryFolder> folders = new List<InventoryFolder>();

        //    List<InventoryBase> contents = _Store.GetContents(baseFolder);
        //    foreach (InventoryBase inv in contents)
        //    {
        //        if (regexp.IsMatch(inv.Name))
        //        {
        //            objects.Add(inv);
        //            if (firstOnly)
        //                return objects;
        //        }
        //        if (inv is InventoryFolder)
        //        {
        //            folders.Add(inv as InventoryFolder);
        //        }
        //    }
        //    // Recurse outside of the loop because subsequent calls to FindObjects may
        //    // modify the baseNode.Nodes collection.
        //    // FIXME: I'm pretty sure this is not necessary
        //    if (recurse)
        //    {
        //        foreach (InventoryFolder folder in folders)
        //        {
        //            objects.AddRange(LocalFind(folder.UUID, regexp, true, firstOnly));
        //        }
        //    }
        //    return objects;
        //}

        //public List<InventoryBase> FindObjectsByPath(LLUUID baseFolder, string[] path, bool refresh, bool firstOnly)
        //{
        //    IAsyncResult r = BeginFindObjectsByPath(baseFolder, path, refresh, firstOnly, null, null, true);
        //    return EndFindObjects(r);
        //}
        
        //public IAsyncResult BeginFindObjectsByPath(LLUUID baseFolder, string[] path, bool refresh, bool firstOnly, AsyncCallback callback, object asyncState, bool recurse)
        //{
        //    if (path.Length == 0)
        //        throw new ArgumentException("Empty path is not supported");
        //    FindResult result = new FindResult(new Regex(String.Join("/",path)), recurse, callback);
        //    result.FirstOnly = firstOnly;
        //    result.AsyncState = asyncState;
            
        //    if (refresh)
        //    {
        //        result.FoldersWaiting = 1;
        //        BeginRequestFolderContents(
        //            baseFolder,
        //            _Client.Self.AgentID,
        //            true,
        //            true,
        //            false,
        //            InventorySortOrder.ByName,
        //            new AsyncCallback(FindObjectsByPathCallback),
        //            new FindObjectsByPathState(result, baseFolder, 0));
        //    }
        //    else
        //    {
        //        result.Result = LocalFind(baseFolder, path, 0, firstOnly);
        //        result.CompletedSynchronously = true;
        //        result.IsCompleted = true;
        //    }

        //    return result;
        //}

        #region Fetch

        public InventoryItem FetchItem(LLUUID itemID, LLUUID ownerID, int timeoutMS)
        {
            AutoResetEvent fetchEvent = new AutoResetEvent(false);
            InventoryItem fetchedItem = null;

            ItemReceivedCallback callback =
                delegate(InventoryItem item)
                {
                    if (item.UUID == itemID)
                    {
                        fetchedItem = item;
                        fetchEvent.Set();
                    }
                };

            OnItemReceived += callback;
            RequestFetchInventory(itemID, ownerID);

            fetchEvent.WaitOne(timeoutMS, false);
            OnItemReceived -= callback;

            return fetchedItem;
        }

        public void RequestFetchInventory(LLUUID itemID, LLUUID ownerID)
        {
            FetchInventoryPacket fetch = new FetchInventoryPacket();
            fetch.AgentData = new FetchInventoryPacket.AgentDataBlock();
            fetch.AgentData.AgentID = _Client.Self.AgentID;
            fetch.AgentData.SessionID = _Client.Self.SessionID;

            fetch.InventoryData = new FetchInventoryPacket.InventoryDataBlock[1];
            fetch.InventoryData[0] = new FetchInventoryPacket.InventoryDataBlock();
            fetch.InventoryData[0].ItemID = itemID;
            fetch.InventoryData[0].OwnerID = ownerID;

            _Client.Network.SendPacket(fetch);
        }

        /// <summary>
        /// Request inventory items
        /// </summary>
        /// <param name="itemIDs">Inventory items to request</param>
        /// <param name="ownerIDs">Owners of the inventory items</param>
        public void RequestFetchInventory(List<LLUUID> itemIDs, List<LLUUID> ownerIDs)
        {
            if (itemIDs.Count != ownerIDs.Count)
                throw new ArgumentException("itemIDs and ownerIDs must contain the same number of entries");

            FetchInventoryPacket fetch = new FetchInventoryPacket();
            fetch.AgentData = new FetchInventoryPacket.AgentDataBlock();
            fetch.AgentData.AgentID = _Client.Self.AgentID;
            fetch.AgentData.SessionID = _Client.Self.SessionID;

            fetch.InventoryData = new FetchInventoryPacket.InventoryDataBlock[itemIDs.Count];
            for (int i = 0; i < itemIDs.Count; i++)
            {
                fetch.InventoryData[i] = new FetchInventoryPacket.InventoryDataBlock();
                fetch.InventoryData[i].ItemID = itemIDs[i];
                fetch.InventoryData[i].OwnerID = ownerIDs[i];
            }

            _Client.Network.SendPacket(fetch);
        }

        public List<InventoryBase> FolderContents(LLUUID folder, LLUUID owner, bool folders, bool items,
            InventorySortOrder order, int timeoutMS)
        {
            List<InventoryBase> objects = null;
            AutoResetEvent fetchEvent = new AutoResetEvent(false);

            FolderUpdatedCallback callback =
                delegate(LLUUID folderID)
                {
                    if (folderID == folder)
                        fetchEvent.Set();
                };

            OnFolderUpdated += callback;

            RequestFolderContents(folder, owner, folders, items, order);
            if (fetchEvent.WaitOne(timeoutMS, false))
                objects = _Store.GetContents(folder);

            OnFolderUpdated -= callback;

            return objects;
        }

        public void RequestFolderContents(LLUUID folder, LLUUID owner, bool folders, bool items, 
            InventorySortOrder order)
        {
            FetchInventoryDescendentsPacket fetch = new FetchInventoryDescendentsPacket();
            fetch.AgentData.AgentID = _Client.Self.AgentID;
            fetch.AgentData.SessionID = _Client.Self.SessionID;

            fetch.InventoryData.FetchFolders = folders;
            fetch.InventoryData.FetchItems = items;
            fetch.InventoryData.FolderID = folder;
            fetch.InventoryData.OwnerID = owner;
            fetch.InventoryData.SortOrder = (int)order;

            _Client.Network.SendPacket(fetch);
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
        /// if not found, or LLUUID.Zero on failure</returns>
        public LLUUID FindFolderForType(AssetType type)
        {
            if (_Store == null)
            {
                _Client.Log("Inventory is null, FindFolderForType() lookup cannot continue",
                    Helpers.LogLevel.Error);
                return LLUUID.Zero;
            }

            // Folders go in the root
            if (type == AssetType.Folder)
                return _Store.RootFolder.UUID;

            // Loop through each top-level directory and check if PreferredType
            // matches the requested type
            List<InventoryBase> contents = _Store.GetContents(_Store.RootFolder.UUID);
            foreach (InventoryBase inv in contents)
            {
                if (inv is InventoryFolder)
                {
                    InventoryFolder folder = inv as InventoryFolder;

                    if (folder.PreferredType == type)
                        return folder.UUID;
                }
            }

            // No match found, return Root Folder ID
            return _Store.RootFolder.UUID;
        }

        public LLUUID FindObjectByPath(LLUUID baseFolder, LLUUID inventoryOwner, string path, int timeoutMS)
        {
            AutoResetEvent findEvent = new AutoResetEvent(false);
            LLUUID foundItem = LLUUID.Zero;

            FindObjectByPathCallback callback =
                delegate(string thisPath, LLUUID inventoryObjectID)
                {
                    if (thisPath == path)
                    {
                        foundItem = inventoryObjectID;
                        findEvent.Set();
                    }
                };

            OnFindObjectByPath += callback;

            RequestFindObjectByPath(baseFolder, inventoryOwner, path);
            findEvent.WaitOne(timeoutMS, false);

            OnFindObjectByPath -= callback;

            return foundItem;
        }

        public void RequestFindObjectByPath(LLUUID baseFolder, LLUUID inventoryOwner, string path)
        {

            if (path == null || path.Length == 0)
                throw new ArgumentException("Empty path is not supported");

            // Store this search
            InventorySearch search;
            search.Folder = baseFolder;
            search.Owner = inventoryOwner;
            search.Path = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            search.Level = 0;
            lock (_Searches) _Searches.Add(search);

            // Start the search
            RequestFolderContents(baseFolder, inventoryOwner, true, true, InventorySortOrder.ByName);
        }

        public List<InventoryBase> LocalFind(LLUUID baseFolder, string[] path, int level, bool firstOnly)
        {
            List<InventoryBase> objects = new List<InventoryBase>();
            //List<InventoryFolder> folders = new List<InventoryFolder>();
            List<InventoryBase> contents = _Store.GetContents(baseFolder);

            foreach (InventoryBase inv in contents)
            {
                if (inv.Name.CompareTo(path[level]) == 0)
                {
                    if (level == path.Length - 1)
                    {
                        objects.Add(inv);
                        if (firstOnly) return objects;
                    }
                    else if (inv is InventoryFolder)
                        objects.AddRange(LocalFind(inv.UUID, path, level + 1, firstOnly));
                }
            }

            return objects;
        }

        #endregion Find

        #region Move

        public void Move(InventoryBase item, InventoryFolder newParent)
        {
            if (item is InventoryFolder)
                MoveFolder(item.UUID, newParent.UUID);
            else
                MoveItem(item.UUID, newParent.UUID);
        }

        public void MoveFolder(LLUUID folder, LLUUID newParent)
        {
            lock (Store)
            {
                if (_Store.Contains(folder))
                {
                    InventoryBase inv = Store[folder];
                    inv.ParentUUID = newParent;
                    _Store.UpdateNodeFor(inv);
                }
            }

            MoveInventoryFolderPacket move = new MoveInventoryFolderPacket();
            move.AgentData.AgentID = _Client.Self.AgentID;
            move.AgentData.SessionID = _Client.Self.SessionID;
            move.AgentData.Stamp = false; //FIXME: ??

            move.InventoryData = new MoveInventoryFolderPacket.InventoryDataBlock[1];
            move.InventoryData[0] = new MoveInventoryFolderPacket.InventoryDataBlock();
            move.InventoryData[0].FolderID = folder;
            move.InventoryData[0].ParentID = newParent;

            _Client.Network.SendPacket(move);
        }
 
        /// <summary>
        /// Moves the folders, the keys in the Dictionary parameter,
        /// to a new parents, the value of that folder's key.
        /// </summary>
        /// <param name="FoldersNewParents"></param>
        public void MoveFolders(Dictionary<LLUUID, LLUUID> foldersNewParents)
        {
            // FIXME: Use two List<LLUUID> to stay consistent

            lock (Store)
            {
                foreach (KeyValuePair<LLUUID, LLUUID> entry in foldersNewParents)
                {
                    if (_Store.Contains(entry.Key))
                    {
                        InventoryBase inv = _Store[entry.Key];
                        inv.ParentUUID = entry.Value;
                        _Store.UpdateNodeFor(inv);
                    }
                }
            }

            //TODO: Test if this truly supports multiple-folder move
            MoveInventoryFolderPacket move = new MoveInventoryFolderPacket();
            move.AgentData.AgentID = _Client.Self.AgentID;
            move.AgentData.SessionID = _Client.Self.SessionID;
            move.AgentData.Stamp = false; //FIXME: ??

            move.InventoryData = new MoveInventoryFolderPacket.InventoryDataBlock[foldersNewParents.Count];

            int index = 0;
            foreach (KeyValuePair<LLUUID, LLUUID> folder in foldersNewParents)
            {
                MoveInventoryFolderPacket.InventoryDataBlock block = new MoveInventoryFolderPacket.InventoryDataBlock();
                block.FolderID = folder.Key;
                block.ParentID = folder.Value;
                move.InventoryData[index++] = block;
            }

            _Client.Network.SendPacket(move);
        }

        public void MoveItem(LLUUID item, LLUUID folder)
        {
            MoveItem(item, folder, String.Empty);
        }

        public void MoveItem(LLUUID item, LLUUID folder, string newItemName)
        {
            lock (Store)
            {
                    if (Store.Contains(item))
                    {
                        InventoryBase inv = _Store[item];
                        inv.ParentUUID = folder;
                        _Store.UpdateNodeFor(inv);
                    }
            }

            MoveInventoryItemPacket move = new MoveInventoryItemPacket();
            move.AgentData.AgentID = _Client.Self.AgentID;
            move.AgentData.SessionID = _Client.Self.SessionID;
            move.AgentData.Stamp = false; //FIXME: ??

            move.InventoryData = new MoveInventoryItemPacket.InventoryDataBlock[1];
            move.InventoryData[0] = new MoveInventoryItemPacket.InventoryDataBlock();
            move.InventoryData[0].ItemID = item;
            move.InventoryData[0].FolderID = folder;
            move.InventoryData[0].NewName = Helpers.StringToField(newItemName);

            _Client.Network.SendPacket(move);
        }

        public void MoveItems(Dictionary<LLUUID, LLUUID> itemsNewParents)
        {
            lock (Store)
            {
                foreach (KeyValuePair<LLUUID, LLUUID> entry in itemsNewParents)
                {
                    if (Store.Contains(entry.Key))
                    {
                        InventoryBase inv = Store[entry.Key];
                        inv.ParentUUID = entry.Value;
                        Store.UpdateNodeFor(inv);
                    }
                }
            }

            MoveInventoryItemPacket move = new MoveInventoryItemPacket();
            move.AgentData.AgentID = _Client.Self.AgentID;
            move.AgentData.SessionID = _Client.Self.SessionID;
            move.AgentData.Stamp = false; //FIXME: ??

            move.InventoryData = new MoveInventoryItemPacket.InventoryDataBlock[itemsNewParents.Count];

            int index = 0;
            foreach (KeyValuePair<LLUUID, LLUUID> entry in itemsNewParents)
            {
                MoveInventoryItemPacket.InventoryDataBlock block = new MoveInventoryItemPacket.InventoryDataBlock();
                block.ItemID = entry.Key;
                block.FolderID = entry.Value;
                block.NewName = new byte[0];
                move.InventoryData[index++] = block;
            }

            _Client.Network.SendPacket(move);
        }

        #endregion Move

        #region Remove

        public void RemoveDescendants(LLUUID folder)
        {
            PurgeInventoryDescendentsPacket purge = new PurgeInventoryDescendentsPacket();
            purge.AgentData.AgentID = _Client.Self.AgentID;
            purge.AgentData.SessionID = _Client.Self.SessionID;
            purge.InventoryData.FolderID = folder;
            _Client.Network.SendPacket(purge);

            // Update our local copy
            lock (Store)
            {
                if (Store.Contains(folder))
                {
                    List<InventoryBase> contents = Store.GetContents(folder);
                    foreach (InventoryBase obj in contents)
                    {
                        Store.RemoveNodeFor(obj);
                    }
                }
            }
        }

        public void RemoveItem(LLUUID item)
        {
            List<LLUUID> items = new List<LLUUID>(1);
            items.Add(item);

            Remove(items, null);
        }

        public void RemoveFolder(LLUUID folder)
        {
            List<LLUUID> folders = new List<LLUUID>(1);
            folders.Add(folder);

            Remove(null, folders);
        }

        public void Remove(List<LLUUID> items, List<LLUUID> folders)
        {
            if ((items == null || items.Count == 0) && (folders == null || folders.Count == 0))
                return;

            RemoveInventoryObjectsPacket rem = new RemoveInventoryObjectsPacket();
            rem.AgentData.AgentID = _Client.Self.AgentID;
            rem.AgentData.SessionID = _Client.Self.SessionID;

            if (items == null || items.Count == 0)
            {
                // To indicate that we want no items removed:
                rem.ItemData = new RemoveInventoryObjectsPacket.ItemDataBlock[1];
                rem.ItemData[0] = new RemoveInventoryObjectsPacket.ItemDataBlock();
                rem.ItemData[0].ItemID = LLUUID.Zero;
            }
            else
            {
                lock (_Store)
                {
                    rem.ItemData = new RemoveInventoryObjectsPacket.ItemDataBlock[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        rem.ItemData[i] = new RemoveInventoryObjectsPacket.ItemDataBlock();
                        rem.ItemData[i].ItemID = items[i];

                        // Update local copy
                        if (_Store.Contains(items[i]))
                            _Store.RemoveNodeFor(Store[items[i]]);
                    }
                }
            }

            if (folders == null || folders.Count == 0)
            {
                // To indicate we want no folders removed:
                rem.FolderData = new RemoveInventoryObjectsPacket.FolderDataBlock[1];
                rem.FolderData[0] = new RemoveInventoryObjectsPacket.FolderDataBlock();
                rem.FolderData[0].FolderID = LLUUID.Zero;
            }
            else
            {
                lock (_Store)
                {
                    rem.FolderData = new RemoveInventoryObjectsPacket.FolderDataBlock[folders.Count];
                    for (int i = 0; i < folders.Count; i++)
                    {
                        rem.FolderData[i] = new RemoveInventoryObjectsPacket.FolderDataBlock();
                        rem.FolderData[i].FolderID = folders[i];

                        // Update local copy
                        if (_Store.Contains(folders[i]))
                            _Store.RemoveNodeFor(Store[folders[i]]);
                    }
                }
            }
            _Client.Network.SendPacket(rem);
        }
    
        public void EmptyLostAndFound()
        {
            EmptySystemFolder(AssetType.LostAndFoundFolder);
        }
        public void EmptyTrash()
        {
            EmptySystemFolder(AssetType.TrashFolder);
        }
        private void EmptySystemFolder(AssetType folderType)
        {
            List<InventoryBase> items = _Store.GetContents(_Store.RootFolder);

            LLUUID folderKey = LLUUID.Zero;
            foreach (InventoryBase item in items)
            {
                if ((item as InventoryFolder) != null)
                {
                    InventoryFolder folder = item as InventoryFolder;
                    if (folder.PreferredType == folderType)
                    {
                        folderKey = folder.UUID;
                        break;
                    }
                }
            }
            items = _Store.GetContents(folderKey);
            List<LLUUID> remItems = new List<LLUUID>();
            List<LLUUID> remFolders = new List<LLUUID>();
            foreach (InventoryBase item in items)
            {
                if ((item as InventoryFolder) != null)
                {
                    remFolders.Add(item.UUID);
                }
                else
                {
                    remItems.Add(item.UUID);
                }
            }
            Remove(remItems, remFolders);
        }   
        #endregion Remove

        #region Create

        public void RequestCreateItem(LLUUID parentFolder, string name, string description, AssetType type, 
            InventoryType invType, PermissionMask nextOwnerMask, ItemCreatedCallback callback)
        {
            // Even though WearableType 0 is Shape, in this context it is treated as NOT_WEARABLE
            RequestCreateItem(parentFolder, name, description, type, invType, (WearableType)0, nextOwnerMask, 
                callback);
        }

        public void RequestCreateItem(LLUUID parentFolder, string name, string description, AssetType type, 
            InventoryType invType, WearableType wearableType, PermissionMask nextOwnerMask, 
            ItemCreatedCallback callback)
        {
            CreateInventoryItemPacket create = new CreateInventoryItemPacket();
            create.AgentData.AgentID = _Client.Self.AgentID;
            create.AgentData.SessionID = _Client.Self.SessionID;

            create.InventoryBlock.CallbackID = RegisterItemCreatedCallback(callback);
            create.InventoryBlock.FolderID = parentFolder;
            create.InventoryBlock.TransactionID = LLUUID.Random();
            create.InventoryBlock.NextOwnerMask = (uint)nextOwnerMask;
            create.InventoryBlock.Type = (sbyte)type;
            create.InventoryBlock.InvType = (sbyte)invType;
            create.InventoryBlock.WearableType = (byte)wearableType;
            create.InventoryBlock.Name = Helpers.StringToField(name);
            create.InventoryBlock.Description = Helpers.StringToField(description);

            _Client.Network.SendPacket(create);
        }

        public LLUUID CreateFolder(LLUUID parentID, AssetType preferredType, string name)
        {
            LLUUID id = LLUUID.Random();

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

            // Create the new folder locally
            InventoryFolder newFolder = new InventoryFolder(id);
            newFolder.Version = 1;
            newFolder.DescendentCount = 0;
            newFolder.ParentUUID = parentID;
            newFolder.PreferredType = preferredType;
            newFolder.Name = name;
            newFolder.OwnerID = _Client.Self.AgentID;

            // Update the local store
            try { _Store[newFolder.UUID] = newFolder; }
            catch (InventoryException ie) { _Client.Log(ie.Message, Helpers.LogLevel.Warning); }

            // Create the create folder packet and send it
            CreateInventoryFolderPacket create = new CreateInventoryFolderPacket();
            create.AgentData.AgentID = _Client.Self.AgentID;
            create.AgentData.SessionID = _Client.Self.SessionID;

            create.FolderData.FolderID = id;
            create.FolderData.ParentID = parentID;
            create.FolderData.Type = (sbyte)preferredType;
            create.FolderData.Name = Helpers.StringToField(name);

            _Client.Network.SendPacket(create);

            return id;
        }

        public void RequestCreateItemFromAsset(byte[] data, string name, string description, AssetType assetType,
            InventoryType invType, LLUUID folderID, ItemCreatedFromAssetCallback callback)
        {
            if (_Client.Network.CurrentSim == null || _Client.Network.CurrentSim.Caps == null)
                throw new Exception("NewFileAgentInventory capability is not currently available");

            string url = _Client.Network.CurrentSim.Caps.CapabilityURI("NewFileAgentInventory");

            if (url != String.Empty)
            {
                Dictionary<string, object> query = new Dictionary<string, object>();
                query.Add("folder_id", folderID);
                query.Add("asset_type", AssetTypeToString(assetType));
                query.Add("inventory_type", InventoryTypeToString(invType));
                query.Add("name", name);
                query.Add("description", description);

                byte[] postData = LLSD.LLSDParser.SerializeXmlBytes(query);

                // Make the request
                CapsRequest request = new CapsRequest(url, _Client.Network.CurrentSim);
                request.OnCapsResponse += new CapsRequest.CapsResponseCallback(CreateItemFromAssetResponse);
                request.MakeRequest(postData, "application/xml", 0, new KeyValuePair<ItemCreatedFromAssetCallback, byte[]>(callback, data));
            }
            else
            {
                throw new Exception("NewFileAgentInventory capability is not currently available");
            }
        }

        #endregion Create

        #region Copy

        public void RequestCopyItem(LLUUID item, LLUUID newParent, string newName, ItemCopiedCallback callback)
        {
            RequestCopyItem(item, newParent, newName, _Client.Self.AgentID, callback);
        }

        public void RequestCopyItem(LLUUID item, LLUUID newParent, string newName, LLUUID oldOwnerID,
            ItemCopiedCallback callback)
        {
            List<LLUUID> items = new List<LLUUID>(1);
            items.Add(item);

            List<LLUUID> folders = new List<LLUUID>(1);
            folders.Add(newParent);

            List<string> names = new List<string>(1);
            names.Add(newName);

            RequestCopyItems(items, folders, names, oldOwnerID, callback);
        }

        public void RequestCopyItems(List<LLUUID> items, List<LLUUID> targetFolders, List<string> newNames,
            LLUUID oldOwnerID, ItemCopiedCallback callback)
        {
            if (items.Count != targetFolders.Count || (newNames != null && items.Count != newNames.Count))
                throw new ArgumentException("All list arguments must have an equal number of entries");

            uint callbackID = RegisterItemsCopiedCallback(callback);

            CopyInventoryItemPacket copy = new CopyInventoryItemPacket();
            copy.AgentData.AgentID = _Client.Self.AgentID;
            copy.AgentData.SessionID = _Client.Self.SessionID;

            copy.InventoryData = new CopyInventoryItemPacket.InventoryDataBlock[items.Count];
            for (int i = 0; i < items.Count; ++i)
            {
                copy.InventoryData[i] = new CopyInventoryItemPacket.InventoryDataBlock();
                copy.InventoryData[i].CallbackID = callbackID;
                copy.InventoryData[i].NewFolderID = targetFolders[i];
                copy.InventoryData[i].OldAgentID = oldOwnerID;
                copy.InventoryData[i].OldItemID = items[i];

                if (newNames != null && !String.IsNullOrEmpty(newNames[i]))
                    copy.InventoryData[i].NewName = Helpers.StringToField(newNames[i]);
                else
                    copy.InventoryData[i].NewName = new byte[0];
            }

            _Client.Network.SendPacket(copy);
        }

        public void RequestCopyItemFromNotecard(LLUUID objectID, LLUUID notecardID, LLUUID folderID, LLUUID itemID)
        {
            CopyInventoryFromNotecardPacket copy = new CopyInventoryFromNotecardPacket();
            copy.AgentData.AgentID = _Client.Self.AgentID;
            copy.AgentData.SessionID = _Client.Self.SessionID;

            copy.NotecardData.ObjectID = objectID;
            copy.NotecardData.NotecardItemID = notecardID;

            copy.InventoryData = new CopyInventoryFromNotecardPacket.InventoryDataBlock[1];
            copy.InventoryData[0] = new CopyInventoryFromNotecardPacket.InventoryDataBlock();
            copy.InventoryData[0].FolderID = folderID;
            copy.InventoryData[0].ItemID = itemID;

            _Client.Network.SendPacket(copy);
        }

        #endregion Copy

        #region Update

        public void RequestUpdateItem(InventoryItem item)
        {
            List<InventoryItem> items = new List<InventoryItem>(1);
            items.Add(item);

            RequestUpdateItems(items, LLUUID.Random());
        }

        public void RequestUpdateItems(List<InventoryItem> items)
        {
            RequestUpdateItems(items, LLUUID.Random());
        }

        public void RequestUpdateItems(List<InventoryItem> items, LLUUID transactionID)
        {
            UpdateInventoryItemPacket update = new UpdateInventoryItemPacket();
            update.AgentData.AgentID = _Client.Self.AgentID;
            update.AgentData.SessionID = _Client.Self.SessionID;
            update.AgentData.TransactionID = transactionID;

            update.InventoryData = new UpdateInventoryItemPacket.InventoryDataBlock[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                InventoryItem item = items[i];

                UpdateInventoryItemPacket.InventoryDataBlock block = new UpdateInventoryItemPacket.InventoryDataBlock();
                block.BaseMask = (uint)item.Permissions.BaseMask;
                block.CRC = ItemCRC(item);
                block.CreationDate = (int)Helpers.DateTimeToUnixTime(item.CreationDate);
                block.CreatorID = item.CreatorID;
                block.Description = Helpers.StringToField(item.Description);
                block.EveryoneMask = (uint)item.Permissions.EveryoneMask;
                block.Flags = item.Flags;
                block.FolderID = item.ParentUUID;
                block.GroupID = item.GroupID;
                block.GroupMask = (uint)item.Permissions.GroupMask;
                block.GroupOwned = item.GroupOwned;
                block.InvType = (sbyte)item.InventoryType;
                block.ItemID = item.UUID;
                block.Name = Helpers.StringToField(item.Name);
                block.NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
                block.OwnerID = item.OwnerID;
                block.OwnerMask = (uint)item.Permissions.OwnerMask;
                block.SalePrice = item.SalePrice;
                block.SaleType = (byte)item.SaleType;
                block.TransactionID = LLUUID.Zero;
                block.Type = (sbyte)item.AssetType;

                update.InventoryData[i] = block;
            }

            _Client.Network.SendPacket(update);
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
        public LLUUID RequestRezFromInventory(Simulator simulator, LLQuaternion rotation, LLVector3 position,
            InventoryObject item)
        {
            return RequestRezFromInventory(simulator, rotation, position, item, _Client.Self.ActiveGroup,
                LLUUID.Random(), false);
        }

        /// <summary>
        /// Rez an object from inventory
        /// </summary>
        /// <param name="simulator">Simulator to place object in</param>
        /// <param name="rotation">Rotation of the object when rezzed</param>
        /// <param name="position">Vector of where to place object</param>
        /// <param name="item">InventoryObject object containing item details</param>
        /// <param name="groupOwner">LLUUID of group to own the object</param>
        public LLUUID RequestRezFromInventory(Simulator simulator, LLQuaternion rotation, LLVector3 position,
            InventoryObject item, LLUUID groupOwner)
        {
            return RequestRezFromInventory(simulator, rotation, position, item, groupOwner, LLUUID.Random(), false);
        }

        /// <summary>
        /// Rez an object from inventory
        /// </summary>
        /// <param name="simulator">Simulator to place object in</param>
        /// <param name="rotation">Rotation of the object when rezzed</param>
        /// <param name="position">Vector of where to place object</param>
        /// <param name="item">InventoryObject object containing item details</param>
        /// <param name="groupOwner">LLUUID of group to own the object</param>        
        /// <param name="queryID">User defined queryID to correlate replies</param>
        /// <param name="requestObjectDetails">if set to true the simulator
        /// will automatically send object detail packet(s) back to the client</param>
        public LLUUID RequestRezFromInventory(Simulator simulator, LLQuaternion rotation, LLVector3 position,
            InventoryObject item, LLUUID groupOwner, LLUUID queryID, bool requestObjectDetails)
        {
            RezObjectPacket add = new RezObjectPacket();

            add.AgentData.AgentID = _Client.Self.AgentID;
            add.AgentData.SessionID = _Client.Self.SessionID;
            add.AgentData.GroupID = groupOwner;

            add.RezData.FromTaskID = LLUUID.Zero;
            add.RezData.BypassRaycast = 1;
            add.RezData.RayStart = position;
            add.RezData.RayEnd = position;
            add.RezData.RayTargetID = LLUUID.Zero;
            add.RezData.RayEndIsIntersection = false;
            add.RezData.RezSelected = requestObjectDetails;
            add.RezData.RemoveItem = false;
            add.RezData.ItemFlags = item.Flags;
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
            add.InventoryData.Flags = item.Flags;
            add.InventoryData.SaleType = (byte)item.SaleType;
            add.InventoryData.SalePrice = item.SalePrice;
            add.InventoryData.Name = Helpers.StringToField(item.Name);
            add.InventoryData.Description = Helpers.StringToField(item.Description);
            add.InventoryData.CreationDate = (int)Helpers.DateTimeToUnixTime(item.CreationDate);

            _Client.Network.SendPacket(add, simulator);

            return queryID;
        }

        public void GiveItem(LLUUID itemID, string itemName, AssetType assetType, LLUUID recipient, bool doEffect)
        {
            byte[] bucket = new byte[17];
            bucket[0] = (byte)assetType;
            Buffer.BlockCopy(itemID.GetBytes(), 0, bucket, 1, 16);

            _Client.Self.InstantMessage(
                _Client.Self.Name,
                recipient,
                itemName,
                LLUUID.Random(),
                InstantMessageDialog.InventoryOffered,
                InstantMessageOnline.Online,
                _Client.Self.SimPosition,
                _Client.Network.CurrentSim.ID,
                bucket);

            if (doEffect)
            {
                _Client.Self.BeamEffect(_Client.Self.AgentID, recipient, LLVector3d.Zero,
                    _Client.Settings.DEFAULT_EFFECT_COLOR, 1f, LLUUID.Random());
            }
        }

        #endregion Rez/Give

        #region Task

        public LLUUID UpdateTaskInventory(uint objectLocalID, InventoryItem item)
        {
            LLUUID transactionID = LLUUID.Random();

            UpdateTaskInventoryPacket update = new UpdateTaskInventoryPacket();
            update.AgentData.AgentID = _Client.Self.AgentID;
            update.AgentData.SessionID = _Client.Self.SessionID;
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
            update.InventoryData.Flags = item.Flags;
            update.InventoryData.SaleType = (byte)item.SaleType;
            update.InventoryData.SalePrice = item.SalePrice;
            update.InventoryData.Name = Helpers.StringToField(item.Name);
            update.InventoryData.Description = Helpers.StringToField(item.Description);
            update.InventoryData.CreationDate = (int)Helpers.DateTimeToUnixTime(item.CreationDate);
            update.InventoryData.CRC = ItemCRC(item);

            _Client.Network.SendPacket(update);

            return transactionID;
        }

        public List<InventoryBase> GetTaskInventory(LLUUID objectID, uint objectLocalID, int timeoutMS)
        {
            string filename = null;
            AutoResetEvent taskReplyEvent = new AutoResetEvent(false);

            TaskInventoryReplyCallback callback =
                delegate(LLUUID itemID, short serial, string assetFilename)
                {
                    if (itemID == objectID)
                    {
                        filename = assetFilename;
                        taskReplyEvent.Set();
                    }
                };

            OnTaskInventoryReply += callback;

            RequestTaskInventory(objectLocalID);

            if (taskReplyEvent.WaitOne(timeoutMS, false))
            {
                OnTaskInventoryReply -= callback;

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
                xferID = _Client.Assets.RequestAssetXfer(filename, true, false, LLUUID.Zero, AssetType.Unknown);

                if (taskDownloadEvent.WaitOne(timeoutMS, false))
                {
                    _Client.Assets.OnXferReceived -= xferCallback;

                    string taskList = Helpers.FieldToUTF8String(assetData);
                    return ParseTaskInventory(taskList);
                }
                else
                {
                    _Client.Log("Timed out waiting for task inventory download for " + filename, Helpers.LogLevel.Warning);
                    _Client.Assets.OnXferReceived -= xferCallback;
                    return null;
                }
            }
            else
            {
                _Client.Log("Timed out waiting for task inventory reply for " + objectLocalID, Helpers.LogLevel.Warning);
                OnTaskInventoryReply -= callback;
                return null;
            }
        }

        public void RequestTaskInventory(uint objectLocalID)
        {
            RequestTaskInventory(objectLocalID, _Client.Network.CurrentSim);
        }

        public void RequestTaskInventory(uint objectLocalID, Simulator simulator)
        {
            RequestTaskInventoryPacket request = new RequestTaskInventoryPacket();
            request.AgentData.AgentID = _Client.Self.AgentID;
            request.AgentData.SessionID = _Client.Self.SessionID;
            request.InventoryData.LocalID = objectLocalID;

            _Client.Network.SendPacket(request, simulator);
        }

        #endregion Task

        #region Helper Functions

        public static string AssetTypeToString(AssetType type)
        {
            return _AssetTypeNames[(int)type];
        }

        public static AssetType StringToAssetType(string type)
        {
            for (int i = 0; i < _AssetTypeNames.Length; i++)
            {
                if (_AssetTypeNames[i] == type)
                    return (AssetType)i;
            }

            return AssetType.Unknown;
        }

        public static string InventoryTypeToString(InventoryType type)
        {
            return _InventoryTypeNames[(int)type];
        }

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
                    _Client.Log("Overwriting an existing ItemCreatedCallback", Helpers.LogLevel.Warning);

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
                    _Client.Log("Overwriting an existing ItemsCopiedCallback", Helpers.LogLevel.Warning);

                _ItemCopiedCallbacks[_CallbackPos] = callback;

                return _CallbackPos;
            }
        }

        public static uint ItemCRC(InventoryItem iitem)
        {
            uint CRC = 0;

            // IDs
            CRC += iitem.AssetUUID.CRC(); // AssetID
            CRC += iitem.ParentUUID.CRC(); // FolderID
            CRC += iitem.UUID == null ? LLUUID.Zero.CRC() : iitem.UUID.CRC(); // ItemID

            // Permission stuff
            CRC += iitem.CreatorID.CRC(); // CreatorID
            CRC += iitem.OwnerID.CRC(); // OwnerID
            CRC += iitem.GroupID.CRC(); // GroupID

            // CRC += another 4 words which always seem to be zero -- unclear if this is a LLUUID or what
            CRC += (uint)iitem.Permissions.OwnerMask; //owner_mask;      // Either owner_mask or next_owner_mask may need to be
            CRC += (uint)iitem.Permissions.NextOwnerMask; //next_owner_mask; // switched with base_mask -- 2 values go here and in my
            CRC += (uint)iitem.Permissions.EveryoneMask; //everyone_mask;   // study item, the three were identical.
            CRC += (uint)iitem.Permissions.GroupMask; //group_mask;

            // The rest of the CRC fields
            CRC += iitem.Flags; // Flags
            CRC += (uint)iitem.InventoryType; // InvType
            CRC += (uint)iitem.AssetType; // Type 
            CRC += (uint)Helpers.DateTimeToUnixTime(iitem.CreationDate); // CreationDate
            CRC += (uint)iitem.SalePrice;    // SalePrice
            CRC += (uint)((uint)iitem.SaleType * 0x07073096); // SaleType

            return CRC;
        }

        public static InventoryItem CreateInventoryItem(InventoryType type, LLUUID id)
        {
            switch (type)
            {
                case InventoryType.Texture: return new InventoryTexture(id);
                case InventoryType.Sound: return new InventorySound(id);
                case InventoryType.CallingCard: return new InventoryCallingCard(id);
                case InventoryType.Landmark: return new InventoryLandmark(id);
                case InventoryType.Object: return new InventoryObject(id);
                case InventoryType.Notecard: return new InventoryNotecard(id);
                case InventoryType.Category: return new InventoryCategory(id);
                case InventoryType.LSL: return new InventoryLSL(id);
                case InventoryType.Snapshot: return new InventorySnapshot(id);
                case InventoryType.Attachment: return new InventoryAttachment(id);
                case InventoryType.Wearable: return new InventoryWearable(id);
                case InventoryType.Animation: return new InventoryAnimation(id);
                case InventoryType.Gesture: return new InventoryGesture(id);
                default: return new InventoryItem(type, id);
            }
        }

        private InventoryItem SafeCreateInventoryItem(InventoryType InvType, LLUUID ItemID)
        {
            InventoryItem ret = null;

            if (_Store.Contains(ItemID))
                ret = Store[ItemID] as InventoryItem;

            if (ret == null)
                ret = CreateInventoryItem(InvType, ItemID);

            return ret;
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

        public static List<InventoryBase> ParseTaskInventory(string taskData)
        {
            List<InventoryBase> items = new List<InventoryBase>();
            int lineNum = 0;
            string[] lines = taskData.Replace("\r\n", "\n").Split(new char[] { '\n' });

            while (lineNum < lines.Length)
            {
                string key, value;
                if (ParseLine(lines[lineNum++], out key, out value))
                {
                    if (key == "inv_object")
                    {
                        #region inv_object

                        // In practice this appears to only be used for folders
                        LLUUID itemID = LLUUID.Zero;
                        LLUUID parentID = LLUUID.Zero;
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
                                    LLUUID.TryParse(value, out itemID);
                                }
                                else if (key == "parent_id")
                                {
                                    LLUUID.TryParse(value, out parentID);
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
                            InventoryFolder folder = new InventoryFolder(itemID);
                            folder.Name = name;
                            folder.ParentUUID = parentID;

                            items.Add(folder);
                        }
                        else
                        {
                            InventoryItem item = new InventoryItem(itemID);
                            item.Name = name;
                            item.ParentUUID = parentID;
                            item.AssetType = assetType;

                            items.Add(item);
                        }

                        #endregion inv_object
                    }
                    else if (key == "inv_item")
                    {
                        #region inv_item

                        // Any inventory item that links to an assetID, has permissions, etc
                        LLUUID itemID = LLUUID.Zero;
                        LLUUID assetID = LLUUID.Zero;
                        LLUUID parentID = LLUUID.Zero;
                        LLUUID creatorID = LLUUID.Zero;
                        LLUUID ownerID = LLUUID.Zero;
                        LLUUID lastOwnerID = LLUUID.Zero;
                        LLUUID groupID = LLUUID.Zero;
                        bool groupOwned = false;
                        string name = String.Empty;
                        string desc = String.Empty;
                        AssetType assetType = AssetType.Unknown;
                        InventoryType inventoryType = InventoryType.Unknown;
                        DateTime creationDate = Helpers.Epoch;
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
                                    LLUUID.TryParse(value, out itemID);
                                }
                                else if (key == "parent_id")
                                {
                                    LLUUID.TryParse(value, out parentID);
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
                                                if (UInt32.TryParse(value, System.Globalization.NumberStyles.HexNumber,
                                                    Helpers.EnUsCulture.NumberFormat, out val))
                                                    perms.BaseMask = (PermissionMask)val;
                                            }
                                            else if (key == "base_mask")
                                            {
                                                uint val;
                                                if (UInt32.TryParse(value, System.Globalization.NumberStyles.HexNumber,
                                                    Helpers.EnUsCulture.NumberFormat, out val))
                                                    perms.BaseMask = (PermissionMask)val;
                                            }
                                            else if (key == "owner_mask")
                                            {
                                                uint val;
                                                if (UInt32.TryParse(value, System.Globalization.NumberStyles.HexNumber,
                                                    Helpers.EnUsCulture.NumberFormat, out val))
                                                    perms.OwnerMask = (PermissionMask)val;
                                            }
                                            else if (key == "group_mask")
                                            {
                                                uint val;
                                                if (UInt32.TryParse(value, System.Globalization.NumberStyles.HexNumber,
                                                    Helpers.EnUsCulture.NumberFormat, out val))
                                                    perms.GroupMask = (PermissionMask)val;
                                            }
                                            else if (key == "everyone_mask")
                                            {
                                                uint val;
                                                if (UInt32.TryParse(value, System.Globalization.NumberStyles.HexNumber,
                                                    Helpers.EnUsCulture.NumberFormat, out val))
                                                    perms.EveryoneMask = (PermissionMask)val;
                                            }
                                            else if (key == "next_owner_mask")
                                            {
                                                uint val;
                                                if (UInt32.TryParse(value, System.Globalization.NumberStyles.HexNumber,
                                                    Helpers.EnUsCulture.NumberFormat, out val))
                                                    perms.NextOwnerMask = (PermissionMask)val;
                                            }
                                            else if (key == "creator_id")
                                            {
                                                LLUUID.TryParse(value, out creatorID);
                                            }
                                            else if (key == "owner_id")
                                            {
                                                LLUUID.TryParse(value, out ownerID);
                                            }
                                            else if (key == "last_owner_id")
                                            {
                                                LLUUID.TryParse(value, out lastOwnerID);
                                            }
                                            else if (key == "group_id")
                                            {
                                                LLUUID.TryParse(value, out groupID);
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
                                    LLUUID.TryParse(value, out assetID);
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
                                        creationDate = Helpers.UnixTimeToDateTime(timestamp);
                                    else
                                        SecondLife.LogStatic("Failed to parse creation_date " + value, Helpers.LogLevel.Warning);
                                }
                            }
                        }

                        InventoryItem item = CreateInventoryItem(inventoryType, itemID);
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
                        SecondLife.LogStatic("Unrecognized token " + key + " in: " + Environment.NewLine + taskData,
                            Helpers.LogLevel.Error);
                    }
                }
            }

            return items;
        }
        
        #endregion Helper Functions

        #region Callbacks

        private void CreateItemFromAssetResponse(object response, HttpRequestState state)
        {
            Dictionary<string, object> contents = (Dictionary<string, object>)response;
            KeyValuePair<ItemCreatedFromAssetCallback, byte[]> kvp = (KeyValuePair<ItemCreatedFromAssetCallback, byte[]>)state.State;
            ItemCreatedFromAssetCallback callback = kvp.Key;
            byte[] itemData = (byte[])kvp.Value;

            string status = (string)contents["state"];

            if (status == "upload")
            {
                string uploadURL = (string)contents["uploader"];

                // This makes the assumption that all uploads go to CurrentSim, to avoid
                // the problem of HttpRequestState not knowing anything about simulators
                CapsRequest upload = new CapsRequest(uploadURL, _Client.Network.CurrentSim);
                upload.OnCapsResponse += new CapsRequest.CapsResponseCallback(CreateItemFromAssetResponse);
                upload.MakeRequest(itemData, "application/octet-stream", 0, kvp);
            }
            else if (status == "complete")
            {
                if (contents.ContainsKey("new_inventory_item") && contents.ContainsKey("new_asset"))
                {
                    try { callback(true, String.Empty, (LLUUID)contents["new_inventory_item"], (LLUUID)contents["new_asset"]); }
                    catch (Exception e) { _Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
                else
                {
                    try { callback(false, "Failed to parse asset and item UUIDs", LLUUID.Zero, LLUUID.Zero); }
                    catch (Exception e) { _Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
            }
            else
            {
                // Failure
                try { callback(false, status, LLUUID.Zero, LLUUID.Zero); }
                catch (Exception e) { _Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        private void SaveAssetIntoInventoryHandler(Packet packet, Simulator simulator)
        {
            SaveAssetIntoInventoryPacket save = (SaveAssetIntoInventoryPacket)packet;

            // FIXME: Find this item in the inventory structure and mark the parent as needing an update
            //save.InventoryData.ItemID;
            _Client.Log("SaveAssetIntoInventory packet received, someone write this function!",
                Helpers.LogLevel.Error);
        }

        private void InventoryDescendentsHandler(Packet packet, Simulator simulator)
        {
            InventoryDescendentsPacket reply = (InventoryDescendentsPacket)packet;

            if (reply.AgentData.Descendents > 0)
            {
                // InventoryDescendantsReply sends a null folder if the parent doesnt contain any folders
                if (reply.FolderData[0].FolderID != LLUUID.Zero)
                {
                    // Iterate folders in this packet
                    for (int i = 0; i < reply.FolderData.Length; i++)
                    {
                        InventoryFolder folder = new InventoryFolder(reply.FolderData[i].FolderID);
                        folder.ParentUUID = reply.FolderData[i].ParentID;
                        folder.Name = Helpers.FieldToUTF8String(reply.FolderData[i].Name);
                        folder.PreferredType = (AssetType)reply.FolderData[i].Type;
                        folder.OwnerID = reply.AgentData.OwnerID;

                        _Store[folder.UUID] = folder;
                    }
                }

                // InventoryDescendantsReply sends a null item if the parent doesnt contain any items.
                if (reply.ItemData[0].ItemID != LLUUID.Zero)
                {
                    // Iterate items in this packet
                    for (int i = 0; i < reply.ItemData.Length; i++)
                    {
                        if (reply.ItemData[i].ItemID != LLUUID.Zero)
                        {
                            InventoryItem item = CreateInventoryItem((InventoryType)reply.ItemData[i].InvType,reply.ItemData[i].ItemID);
                            item.ParentUUID = reply.ItemData[i].FolderID;
                            item.CreatorID = reply.ItemData[i].CreatorID;
                            item.AssetType = (AssetType)reply.ItemData[i].Type;
                            item.AssetUUID = reply.ItemData[i].AssetID;
                            item.CreationDate = Helpers.UnixTimeToDateTime((uint)reply.ItemData[i].CreationDate);
                            item.Description = Helpers.FieldToUTF8String(reply.ItemData[i].Description);
                            item.Flags = reply.ItemData[i].Flags;
                            item.Name = Helpers.FieldToUTF8String(reply.ItemData[i].Name);
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

                            _Store[item.UUID] = item;
                        }
                    }
                }
            }

            InventoryFolder parentFolder = null;

            if (_Store.Contains(reply.AgentData.FolderID) &&
                _Store[reply.AgentData.FolderID] is InventoryFolder)
            {
                parentFolder = _Store[reply.AgentData.FolderID] as InventoryFolder;
            }
            else
            {
                _Client.Log("Don't have a reference to FolderID " + reply.AgentData.FolderID.ToStringHyphenated() +
                    " or it is not a folder", Helpers.LogLevel.Error);
                return;
            }

            if (reply.AgentData.Version < parentFolder.Version)
            {
                _Client.Log("Got an outdated InventoryDescendents packet for folder " + parentFolder.Name +
                    ", this version = " + reply.AgentData.Version + ", latest version = " + parentFolder.Version,
                    Helpers.LogLevel.Warning);
                return;
            }

            parentFolder.Version = reply.AgentData.Version;
            // FIXME: reply.AgentData.Descendants is not parentFolder.DescendentCount if we didn't 
            // request items and folders
            parentFolder.DescendentCount = reply.AgentData.Descendents;

            #region FindObjectsByPath Handling

            if (_Searches.Count > 0)
            {
                lock (_Searches)
                {
                StartSearch:

                    // Iterate over all of the outstanding searches
                    for (int i = 0; i < _Searches.Count; i++)
                    {
                        InventorySearch search = _Searches[i];
                        List<InventoryBase> folderContents = _Store.GetContents(search.Folder);

                        // Iterate over all of the inventory objects in the base search folder
                        for (int j = 0; j < folderContents.Count; j++)
                        {
                            // Check if this inventory object matches the current path node
                            if (folderContents[j].Name == search.Path[search.Level])
                            {
                                if (search.Level == search.Path.Length - 1)
                                {
                                    _Client.DebugLog("Finished patch search of " + String.Join("/", search.Path));

                                    // This is the last node in the path, fire the callback and clean up
                                    if (OnFindObjectByPath != null)
                                    {
                                        try { OnFindObjectByPath(String.Join("/", search.Path), folderContents[j].UUID); }
                                        catch (Exception e) { _Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                                    }

                                    // Remove this entry and restart the loop since we are changing the collection size
                                    _Searches.RemoveAt(i);
                                    goto StartSearch;
                                }
                                else
                                {
                                    // We found a match but it is not the end of the path, request the next level
                                    _Client.DebugLog(String.Format("Matched level {0}/{1} in a path search of {2}",
                                        search.Level, search.Path.Length - 1, String.Join("/", search.Path)));

                                    search.Folder = folderContents[j].UUID;
                                    search.Level++;
                                    _Searches[i] = search;

                                    RequestFolderContents(search.Folder, search.Owner, true, true, 
                                        InventorySortOrder.ByName);
                                }
                            }
                        }
                    }
                }
            }

            #endregion FindObjectsByPath Handling

            // Callback for inventory folder contents being updated
            if (OnFolderUpdated != null)
            {
                try { OnFolderUpdated(parentFolder.UUID); }
                catch (Exception e) { _Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
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
                    _Client.Log("Received InventoryFolder in an UpdateCreateInventoryItem packet, this should not happen!",
                        Helpers.LogLevel.Error);
                    continue;
                }

                InventoryItem item = CreateInventoryItem((InventoryType)dataBlock.InvType,dataBlock.ItemID);
                item.AssetType = (AssetType)dataBlock.Type;
                item.AssetUUID = dataBlock.AssetID;
                item.CreationDate = DateTime.FromBinary(dataBlock.CreationDate);
                item.CreatorID = dataBlock.CreatorID;
                item.Description = Helpers.FieldToUTF8String(dataBlock.Description);
                item.Flags = dataBlock.Flags;
                item.GroupID = dataBlock.GroupID;
                item.GroupOwned = dataBlock.GroupOwned;
                item.Name = Helpers.FieldToUTF8String(dataBlock.Name);
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

                // Update the local copy
                _Store[item.UUID] = item;

                // Look for an "item created" callback
                ItemCreatedCallback createdCallback;
                if (_ItemCreatedCallbacks.TryGetValue(dataBlock.CallbackID, out createdCallback))
                {
                    _ItemCreatedCallbacks.Remove(dataBlock.CallbackID);

                    try { createdCallback(true, item); }
                    catch (Exception e) { _Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }

                // TODO: Is this callback even triggered when items are copied?
                // Look for an "item copied" callback
                ItemCopiedCallback copyCallback;
                if (_ItemCopiedCallbacks.TryGetValue(dataBlock.CallbackID, out copyCallback))
                {
                    _ItemCopiedCallbacks.Remove(dataBlock.CallbackID);

                    try { copyCallback(item); }
                    catch (Exception e) { _Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
                
                //This is triggered when an item is received from a task
                if (OnTaskItemReceived != null)
                {
                    try { OnTaskItemReceived(item.UUID, dataBlock.FolderID, item.CreatorID, item.AssetUUID, 
                        item.InventoryType); }
                    catch (Exception e) { _Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
            }
        }

        private void MoveInventoryItemHandler(Packet packet, Simulator simulator)
        {
            MoveInventoryItemPacket move = (MoveInventoryItemPacket)packet;

            for (int i = 0; i < move.InventoryData.Length; i++)
            {
                // FIXME: Do something here
                string newName = Helpers.FieldToUTF8String(move.InventoryData[i].NewName);

                _Client.Log(String.Format(
                    "MoveInventoryItemHandler: Item {0} is moving to Folder {1} with new name \"{2}\". Someone write this function!",
                    move.InventoryData[i].ItemID.ToStringHyphenated(), move.InventoryData[i].FolderID.ToStringHyphenated(),
                    newName), Helpers.LogLevel.Warning);
            }
        }

        private void BulkUpdateInventoryHandler(Packet packet, Simulator simulator)
        {
            BulkUpdateInventoryPacket update = packet as BulkUpdateInventoryPacket;

            if (update.FolderData.Length > 0 && update.FolderData[0].FolderID != LLUUID.Zero)
            {
                foreach (BulkUpdateInventoryPacket.FolderDataBlock dataBlock in update.FolderData)
                {
                    if (!_Store.Contains(dataBlock.FolderID))
                        _Client.Log("Received BulkUpdate for unknown folder: " + dataBlock.FolderID, Helpers.LogLevel.Warning);

                    InventoryFolder folder = new InventoryFolder(dataBlock.FolderID);
                    folder.Name = Helpers.FieldToUTF8String(dataBlock.Name);
                    folder.OwnerID = update.AgentData.AgentID;
                    folder.ParentUUID = dataBlock.ParentID;
                    _Store[folder.UUID] = folder;
                }
            }

            if (update.ItemData.Length > 0 && update.ItemData[0].ItemID != LLUUID.Zero)
            {
                for (int i = 0; i < update.ItemData.Length; i++)
                {
                    BulkUpdateInventoryPacket.ItemDataBlock dataBlock = update.ItemData[i];

                    if (!_Store.Contains(dataBlock.ItemID))
                        _Client.Log("Received BulkUpdate for unknown item: " + dataBlock.ItemID, Helpers.LogLevel.Warning);

                    InventoryItem item = SafeCreateInventoryItem((InventoryType)dataBlock.InvType, dataBlock.ItemID);

                    item.AssetType = (AssetType)dataBlock.Type;
                    if (dataBlock.AssetID != LLUUID.Zero) item.AssetUUID = dataBlock.AssetID;
                    item.CreationDate = DateTime.FromBinary(dataBlock.CreationDate);
                    item.CreatorID = dataBlock.CreatorID;
                    item.Description = Helpers.FieldToUTF8String(dataBlock.Description);
                    item.Flags = dataBlock.Flags;
                    item.GroupID = dataBlock.GroupID;
                    item.GroupOwned = dataBlock.GroupOwned;
                    item.Name = Helpers.FieldToUTF8String(dataBlock.Name);
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

                    _Store[item.UUID] = item;

                    // Look for an "item created" callback
                    ItemCreatedCallback callback;
                    if (_ItemCreatedCallbacks.TryGetValue(dataBlock.CallbackID, out callback))
                    {
                        _ItemCreatedCallbacks.Remove(dataBlock.CallbackID);

                        try { callback(true, item); }
                        catch (Exception e) { _Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                    }

                    // Look for an "item copied" callback
                    ItemCopiedCallback copyCallback;
                    if (_ItemCopiedCallbacks.TryGetValue(dataBlock.CallbackID, out copyCallback))
                    {
                        _ItemCopiedCallbacks.Remove(dataBlock.CallbackID);

                        try { copyCallback(item); }
                        catch (Exception e) { _Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                    }
                }
            }
        }

        private void FetchInventoryReplyHandler(Packet packet, Simulator simulator)
        {
            FetchInventoryReplyPacket reply = packet as FetchInventoryReplyPacket;

            foreach (FetchInventoryReplyPacket.InventoryDataBlock dataBlock in reply.InventoryData) 
            {
                if (dataBlock.InvType == (sbyte)InventoryType.Folder)
                {
                    _Client.Log("Received FetchInventoryReply for an inventory folder, this should not happen!",
                        Helpers.LogLevel.Error);
                    continue;
                }

                InventoryItem item = CreateInventoryItem((InventoryType)dataBlock.InvType,dataBlock.ItemID);
                item.AssetType = (AssetType)dataBlock.Type;
                item.AssetUUID = dataBlock.AssetID;
                item.CreationDate = DateTime.FromBinary(dataBlock.CreationDate);
                item.CreatorID = dataBlock.CreatorID;
                item.Description = Helpers.FieldToUTF8String(dataBlock.Description);
                item.Flags = dataBlock.Flags;
                item.GroupID = dataBlock.GroupID;
                item.GroupOwned = dataBlock.GroupOwned;
                item.Name = Helpers.FieldToUTF8String(dataBlock.Name);
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

                _Store[item.UUID] = item;

                // Fire the callback for an item being fetched
                if (OnItemReceived != null)
                {
                    try { OnItemReceived(item); }
                    catch (Exception e) { _Client.Log(e.ToString(), Helpers.LogLevel.Error); }
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
                        Helpers.FieldToUTF8String(reply.InventoryData.Filename));
                }
                catch (Exception e) { _Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        private void Self_OnInstantMessage(InstantMessage im, Simulator simulator)
        {
            // TODO: MainAvatar.InstantMessageDialog.GroupNotice can also be an inventory offer, should we
            // handle it here?

            if (OnObjectOffered != null && 
                (im.Dialog == InstantMessageDialog.InventoryOffered || im.Dialog == InstantMessageDialog.TaskInventoryOffered))
            {
                AssetType type = AssetType.Unknown;
                LLUUID objectID = LLUUID.Zero;
                bool fromTask = false;

                if (im.Dialog == InstantMessageDialog.InventoryOffered)
                {
                    if (im.BinaryBucket.Length == 17)
                    {
                        type = (AssetType)im.BinaryBucket[0];
                        objectID = new LLUUID(im.BinaryBucket, 1);
                        fromTask = false;
                    }
                    else
                    {
                        _Client.Log("Malformed inventory offer from agent", Helpers.LogLevel.Warning);
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
                        _Client.Log("Malformed inventory offer from object", Helpers.LogLevel.Warning);
                        return;
                    }
                }

                // Find the folder where this is going to go
                LLUUID destinationFolderID = FindFolderForType(type);

                // Fire the callback
                try
                {
                    ImprovedInstantMessagePacket imp = new ImprovedInstantMessagePacket();
                    imp.AgentData.AgentID = _Client.Self.AgentID;
                    imp.AgentData.SessionID = _Client.Self.SessionID;
                    imp.MessageBlock.FromGroup = false;
                    imp.MessageBlock.ToAgentID = im.FromAgentID;
                    imp.MessageBlock.Offline = 0;
                    imp.MessageBlock.ID = im.IMSessionID;
                    imp.MessageBlock.Timestamp = 0;
                    imp.MessageBlock.FromAgentName = Helpers.StringToField(_Client.Self.Name);
                    imp.MessageBlock.Message = new byte[0];
                    imp.MessageBlock.ParentEstateID = 0;
                    imp.MessageBlock.RegionID = LLUUID.Zero;
                    imp.MessageBlock.Position = _Client.Self.SimPosition;

                    if (OnObjectOffered(im.FromAgentID, im.FromAgentName, im.ParentEstateID, im.RegionID, im.Position,
                        im.Timestamp, type, objectID, fromTask))
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

                    _Client.Network.SendPacket(imp, simulator);
                }
                catch (Exception e)
                {
                    _Client.Log(e.ToString(), Helpers.LogLevel.Error);
                }
            }
        }
        
        private void Network_OnLoginResponse(bool loginSuccess, bool redirect, string message, string reason, NetworkManager.LoginResponseData replyData)
        {
            if (loginSuccess)
            {
                _Client.DebugLog("Setting InventoryRoot to " + replyData.InventoryRoot.ToStringHyphenated());
                InventoryFolder rootFolder = new InventoryFolder(replyData.InventoryRoot);
                rootFolder.Name = String.Empty;
                rootFolder.ParentUUID = LLUUID.Zero;
                _Store.RootFolder = rootFolder;

                foreach (InventoryFolder folder in replyData.InventorySkeleton)
                    _Store.UpdateNodeFor(folder);
            }
        }

        #endregion Callbacks
    }
}
