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
using System.Collections;
using System.Text;
using System.Threading;

namespace OpenMetaverse
{
    /// <summary>
    /// Inventory is responsible for managing inventory items and folders.
    /// It updates the InventoryFolders and InventoryItems with new FolderData
    /// and ItemData received from the InventoryManager. Updates to inventory 
    /// folders/items that are not explicitly managed (via the Manage method) 
    /// are ignored.
    /// 
    /// When the FolderData and/or ItemData indicates an inventory change that is not 
    /// reflected locally, then the Inventory will update the local inventory state
    /// accordingly. Under normal circumstances (when inventory is modified 
    /// by the local client via the InventoryBase, InventoryFolder and InventoryItem 
    /// interfaces) it doesn't have to do this.
    /// </summary>
    public class Inventory : IEnumerable<InventoryBase>
    {
        /// <summary>
        /// Delegate for <seealso cref="OnInventoryUpdate"/>.
        /// </summary>
        /// <param name="inventory">Inventory that was updated.</param>
        public delegate void InventoryUpdate(InventoryBase inventory);

        /// <summary>
        /// Triggered when a managed inventory item or folder is updated.
        /// </summary>
        public event InventoryUpdate OnInventoryUpdate;


        /// <summary>
        /// Delegate for <seealso cref="OnInventoryManaged"/>
        /// </summary>
        /// <param name="inventory">The Inventory that is managing <paramref name="ibase"/></param>
        /// <param name="ibase">The inventory item or folder that was managed.</param>
        public delegate void InventoryManaged(Inventory inventory, InventoryBase ibase);

        /// <summary>
        /// Triggered when an inventory item is first managed.
        /// </summary>
        public event InventoryManaged OnInventoryManaged;

        /// <summary>
        /// Retrieves a managed InventoryBase from the Inventory.
        /// Returns null if the UUID isn't managed by this Inventory.
        /// </summary>
        /// <param name="uuid">The UUID of the InventoryBase to retrieve.</param>
        /// <returns>A managed InventoryBase.</returns>
        public InventoryBase this[UUID uuid]
        {
            get
            {
                InventoryBase item;
                if (Items.TryGetValue(uuid, out item))
                    return item;
                return null;
            }
        }

        protected Dictionary<UUID, InventoryBase> Items;
        private InventoryManager _Manager;

        private UUID _Owner;
        /// <summary>
        /// The owner of this inventory. Inventorys can only manage items
        /// owned by the same agent.
        /// </summary>
        public UUID Owner
        {
            get { return _Owner; }
            private set { _Owner = value; }
        }

        private UUID _RootUUID;
        /// <summary>
        /// The UUID of the root folder.
        /// </summary>
        public UUID RootUUID
        {
            get { return _RootUUID; }
            private set { _RootUUID = value; }
        }

        /// <summary>
        /// Reference to the InventoryFolder representing the 
        /// inventory root folder, if it has been managed.
        /// </summary>
        public InventoryFolder RootFolder
        {
            get { return this[RootUUID] as InventoryFolder; }
        }

        /// <summary>
        /// Initializes an empty, rootless, ownerless inventory.
        /// This is used so that we can have an Inventory instance before
        /// the owner and root data is known.
        /// </summary>
        /// <param name="manager">Manager for remote updates.</param>
        public Inventory(InventoryManager manager)
            : this(manager, UUID.Zero, UUID.Zero) { }

        /// <summary>
        /// Creates a new Inventory. Remote updates are sent via the manager
        /// passed to this constructor. All folders contained within the InventorySkeleton
        /// are automatically managed. The inventory takes on the owner of the skeleton.
        /// </summary>
        /// <param name="manager">Manager for remote updates.</param>
        /// <param name="skeleton">Skeleton of folders, inventory owner.</param>
        public Inventory(InventoryManager manager, InventorySkeleton skeleton)
            : this(manager, skeleton.Owner, skeleton.RootUUID)
        {
            ManageSkeleton(skeleton);
        }

        /// <summary>
        /// Creates a new inventory. Remote updates are sent via the manager
        /// passed to this constructor. This creates an empty inventory, with no managed items.
        /// </summary>
        /// <param name="manager">Manager for remote updates.</param>
        /// <param name="owner">Owner of this inventory.</param>
        /// <param name="root"></param>
        public Inventory(InventoryManager manager, UUID owner, UUID root)
        {
            _Manager = manager;
            Owner = owner;
            _RootUUID = root;
            if (Items == null)
                Items = new Dictionary<UUID, InventoryBase>();
            RegisterInventoryCallbacks();
        }

        protected internal void InitializeFromSkeleton(InventorySkeleton skeleton)
        {
            Owner = skeleton.Owner;
            RootUUID = skeleton.RootUUID;
            Items = new Dictionary<UUID, InventoryBase>(skeleton.Folders.Length);
            ManageSkeleton(skeleton);
        }

        /// <summary>
        /// Manages all the folders in the skeleton, if the skeleton is owned
        /// by the same agent. 
        /// </summary>
        /// <param name="skeleton">The skeleton with folders to manage.</param>
        /// <returns>true if Inventory's owner is skeleton's owner and management succeeded, false otherwise.</returns>
        protected bool ManageSkeleton(InventorySkeleton skeleton)
        {
            if (skeleton.Owner != Owner)
                return false;
            foreach (FolderData folder in skeleton.Folders)
            {
                Manage(folder);
            }
            return true;
        }

        /// <summary>
        /// Registers InventoryManager callbacks for inventory updates.
        /// </summary>
        protected virtual void RegisterInventoryCallbacks()
        {
            _Manager.OnFolderUpdate += new InventoryManager.FolderUpdate(manager_OnFolderUpdate);
            _Manager.OnItemUpdate += new InventoryManager.ItemUpdate(manager_OnItemUpdate);
            _Manager.OnAssetUpdate += new InventoryManager.AssetUpdate(manager_OnAssetUpdate);
            _Manager.OnItemCreated += new InventoryManager.ItemCreatedCallback(_Manager_OnItemCreated);
        }

        void _Manager_OnItemCreated(bool success, ItemData itemData)
        {
            if (Items.ContainsKey(itemData.ParentUUID))
                Manage(itemData);
        }

        /// <summary>
        /// Updates the AssetUUID of a managed InventoryItem's ItemData.
        /// If the InventoryItem is not managed, the update is ignored.
        /// </summary>
        /// <param name="itemID">UUID of the item to update.</param>
        /// <param name="newAssetID">The item's new asset UUID.</param>
        protected void manager_OnAssetUpdate(UUID itemID, UUID newAssetID)
        {
            InventoryBase b;
            if (Items.TryGetValue(itemID, out b))
            {
                if (b is InventoryItem)
                {
                    (b as InventoryItem).Data.AssetUUID = newAssetID;
                }
            }
        }

        /// <summary>
        /// Updates the ItemData of a managed InventoryItem. This may
        /// change local inventory state if the local inventory is not
        /// consistant with the new data. (Parent change, rename, etc)
        /// If the item is not managed, the update is ignored.
        /// </summary>
        /// <param name="itemData">The updated ItemData.</param>
        protected void manager_OnItemUpdate(ItemData itemData)
        {
            InventoryBase item;
            if (Items.TryGetValue(itemData.UUID, out item))
            {
                if (item is InventoryItem)
                {
                    Update(item as InventoryItem, itemData);
                }
            }
            else
            {
                // Check if it's a child of a managed folder.
                if (Items.ContainsKey(itemData.ParentUUID))
                    Manage(itemData);
            }
        }

        /// <summary>
        /// Updates the FolderData of a managed InventoryFolder. This 
        /// may change local inventory state if the local inventory is not
        /// consistant with the new data (Parent change, rename, etc)
        /// If the folder is not managed, the update is ignored.
        /// </summary>
        /// <param name="folderData">The updated FolderData.</param>
        protected void manager_OnFolderUpdate(FolderData folderData)
        {
            InventoryBase folder;
            if (Items.TryGetValue(folderData.UUID, out folder))
            {
                if (folder is InventoryFolder)
                {
                    Update(folder as InventoryFolder, folderData);
                }
            }
            else
            {
                // Check if it's a child of a managed folder.
                if (Items.ContainsKey(folderData.ParentUUID))
                    Manage(folderData);
            }
        }

        /// <summary>
        /// Wraps the ItemData in a new InventoryItem.
        /// You may override this method to use your own subclass of 
        /// InventoryItem.
        /// </summary>
        /// <param name="data">The ItemData to wrap.</param>
        /// <returns>A new InventoryItem wrapper for the ItemData.</returns>
        protected virtual InventoryItem WrapItemData(ItemData data)
        {
            return new InventoryItem(_Manager, this, data);
        }

        /// <summary>
        /// Wraps the FolderData in a new InventoryFolder.
        /// You may override this method to use your own subclass of
        /// InventoryFolder.
        /// </summary>
        /// <param name="data">The FolderData to wrap.</param>
        /// <returns>A new InventoryFolder wrapper for the FolderData.</returns>
        protected virtual InventoryFolder WrapFolderData(FolderData data)
        {
            return new InventoryFolder(_Manager, this, data);
        }

        /// <summary>
        /// Attempts to fetch and manage an inventory item from its item UUID. 
        /// This method will block until the item's ItemData is fetched from 
        /// the remote inventory. If the item is already managed by the inventory
        /// returns the local managed InventoryItem wrapper.
        /// </summary>
        /// <param name="itemID">The ItemID of the inventory item to fetch.</param>
        /// <returns>Managed InventoryItem, null if fetch fails.</returns>
        public InventoryItem Manage(UUID itemID)
        {
            InventoryBase ib;
            if (Items.TryGetValue(itemID, out ib))
            {
                // This method shouldn't really be used for retrieving items.
                return ib as InventoryItem;
            }
            else
            {
                ItemData item;
                if (_Manager.FetchItem(itemID, Owner, TimeSpan.FromSeconds(30), out item))
                {
                    return Manage(item);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Explicitly manage the inventory item given its ItemData.
        /// If the item isn't managed, a new wrapper for it is created
        /// and it is added to the local inventory. 
        /// If the item is already managed, this method returns its wrapper, 
        /// updating it with the ItemData passed to this method. 
        /// </summary>
        /// <param name="item">The ItemData of the item to manage.</param>
        /// <returns>The managed InventoryItem wrapper.</returns>
        public virtual InventoryItem Manage(ItemData item)
        {
            if (item.OwnerID == Owner)
            {
                InventoryBase b;
                if (Items.TryGetValue(item.UUID, out b))
                {
                    //Logger.DebugLog(String.Format("{0}: {1} already managed, updating.", (RootFolder == null) ? RootUUID.ToString() : RootFolder.Name, item.Name));
                    Update(b as InventoryItem, item);
                    return b as InventoryItem;
                }
                else
                {
                    InventoryItem wrapper = WrapItemData(item);
                    //Logger.DebugLog(String.Format("{0}: {1} managed, {2} total.", (RootFolder == null) ? RootUUID.ToString() : RootFolder.Name, item.Name, Items.Count));
                    lock (Items)
                        Items.Add(item.UUID, wrapper);

                    if (OnInventoryManaged != null)
                    {
                        try { OnInventoryManaged(this, wrapper); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, e); }
                    }

                    return wrapper;
                }
            }
            else
            {
                //Logger.DebugLog(String.Format("{0}: {1} is not owned by this inventory.", (RootFolder == null) ? RootUUID.ToString() : RootFolder.Name, item.Name));
                return null;
            }
        }


        /// <summary>
        /// Explicitly manage the inventory folder given its FolderData.
        /// If the folder isn't managed, a new wrapper for it is created, 
        /// it is added to the local inventory, and any known children of 
        /// the folder are added to the folder's Contents.
        /// If the folder is already managed, this method returns the folder's
        /// wrapper, updating it with the FolderData passed to this method.
        /// </summary>
        /// <param name="folder">The FolderData of the folder to manage.</param>
        /// <returns>The managed InventoryFolder wrapper.</returns>
        public virtual InventoryFolder Manage(FolderData folder)
        {
            if (folder.OwnerID == Owner)
            {
                InventoryBase b;
                if (Items.TryGetValue(folder.UUID, out b))
                {
                    //Logger.DebugLog(String.Format("{0}: {1} already managed, updating.", (RootFolder == null) ? RootUUID.ToString() : RootFolder.Name, folder.Name));
                    Update(b as InventoryFolder, folder);
                    return b as InventoryFolder;
                }
                else
                {
                    InventoryFolder wrapper = WrapFolderData(folder);
                    lock (Items)
                        Items.Add(folder.UUID, wrapper);
                    //Logger.DebugLog(String.Format("{0}: {1} managed, {2} total.", (RootFolder == null) ? RootUUID.ToString() : RootFolder.Name, folder.Name, Items.Count));
                    // Folder is now managed, update its contents with known children.
                    foreach (InventoryBase item in Items.Values)
                    {
                        if (item.ParentUUID == folder.UUID)
                        {
                            wrapper.AddChild(item);
                        }
                    }
                    if (OnInventoryManaged != null)
                    {
                        try { OnInventoryManaged(this, wrapper); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, e); }
                    }
                    return wrapper;
                }
            }
            else
            {
                //Logger.DebugLog(String.Format("{0}: {1} is not owned by this inventory.", (RootFolder == null) ? RootUUID.ToString() : RootFolder.Name, folder.Name));
                return null;
            }
        }

        /// <summary>
        /// Unmanages an inventory item or folder. The item or folder will no 
        /// longer be automatically updated.
        /// </summary>
        /// <param name="item"></param>
        public virtual void Unmanage(InventoryBase item)
        {
            lock (Items)
                Items.Remove(item.UUID);
            //Logger.DebugLog("Unmanaging " + item.Name);
        }

        protected void Update(InventoryItem item, ItemData update)
        {
            if (item.Data != update)
            {
                // Check for parent change:
                if (item.Data.ParentUUID != update.ParentUUID)
                {
                    item.LocalMove(update.ParentUUID, this[update.ParentUUID] as InventoryFolder);
                }
                item.Data = update;
                FireInventoryUpdate(item);
            }
        }

        protected void Update(InventoryFolder folder, FolderData update)
        {
            if (folder.Data != update)
            {
                // Check for parent change:
                if (folder.Data.ParentUUID != update.ParentUUID)
                {
                    folder.LocalMove(update.ParentUUID, this[update.ParentUUID] as InventoryFolder);
                }
                folder.Data = update;
                FireInventoryUpdate(folder);
            }
        }

        protected void FireInventoryUpdate(InventoryBase updatedInventory)
        {
            if (OnInventoryUpdate != null)
            {
                try { OnInventoryUpdate(updatedInventory); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, e); }
            }
        }

        #region Pathing

        /// <summary>
        /// Fetches an inventory item or folder by path.
        /// If the path starts with /, <paramref name="currentDirectory"/> is ignored
        /// and the path is located from the root.
        /// </summary>
        /// <remarks>If <paramref name="fetchStale"/> is <code>true</code>, this method may take a while to return.</remarks>
        /// <param name="path">A "/"-seperated path, UNIX style. Accepts UUIDs or folder names.</param>
        /// <param name="currentDirectory">The directory to begin in if the path does not start at the root.</param>
        /// <param name="fetchStale">Whether to fetch folder contents when they're needed.</param>
        /// <returns>Multiple items if the path is ambiguous.</returns>
        public List<InventoryBase> InventoryFromPath(string path, InventoryFolder currentDirectory, bool fetchStale)
        {
            path = path.Trim();
            if (path.StartsWith("/"))
            {
                currentDirectory = RootFolder;
                path = path.Remove(0, 1);
            }
            return InventoryFromPath(path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries), currentDirectory, fetchStale);
        }

        /// <summary>
        /// Fetches an inventory item or folder by path, without fetching anything
        /// from the remote inventory. 
        /// </summary>
        /// <remarks>If <paramref name="fetchStale"/> is <code>true</code>, this method may take a while to return.</remarks>
        /// <param name="path">Array whose elements are names or UUIDs of folders, representing the path to the desired item or folder.</param>
        /// <returns>Multiple items if the path is ambiguous.</returns>
        public List<InventoryBase> InventoryFromPath(string[] path)
        {
            return InventoryFromPath(path, false);
        }

        /// <summary>
        /// Fetches an inventory item or folder by path, starting at the inventory's root folder.
        /// </summary>
        /// <remarks>If <paramref name="fetchStale"/> is <code>true</code>, this method may take a while to return.</remarks>
        /// <param name="path">Array whose elements are names or UUIDs of folders, representing the path to the desired item or folder.</param>
        /// <param name="fetchStale">If a folder is stale, fetch its contents.</param>
        /// <returns>Multiple items if the path is ambiguous.</returns>
        public List<InventoryBase> InventoryFromPath(string[] path, bool fetchStale)
        {
            return InventoryFromPath(path, RootFolder, fetchStale);
        }

        /// <summary>
        /// Fetches an inventory item or folder by path, starting at <paramref name="baseFolder"/>.
        /// </summary>
        /// <remarks>If <paramref name="fetchStale"/> is <code>true</code>, this method may take a while to return.</remarks>
        /// <param name="path">Array whose elements are names or UUIDs of folders, representing the path to the desired item or folder.</param>
        /// <param name="baseFolder">Folder to start the path from.</param>
        /// <param name="fetchStale">Whether to fetch folder contents when they're needed.</param>
        /// <returns>Multiple items if the path is ambiguous.</returns>
        public List<InventoryBase> InventoryFromPath(string[] path, InventoryFolder baseFolder, bool fetchStale)
        {
            if (path == null || path.Length == 0)
            {
                List<InventoryBase> one = new List<InventoryBase>(1);
                one.Add(baseFolder);
                return one;
            }

            // Agenda stores an object[] which contains an InventoryFolder and an int
            // the int represents the level in the path that the children of the InventoryFolder
            // should satasfy. 
            List<InventoryBase> results = new List<InventoryBase>();
            Stack<object[]> agenda = new Stack<object[]>();
            agenda.Push(new object[] { baseFolder, 0 });
            while (agenda.Count > 0)
            {
                object[] currentData = agenda.Pop();
                InventoryFolder currentFolder = currentData[0] as InventoryFolder;
                int goalLevel = (int)currentData[1];

                if (path[goalLevel] == "..")
                {
                    // The unix behavior at root is that it's its own parent.
                    InventoryFolder parent = currentFolder != RootFolder ? currentFolder.Parent : RootFolder;
                    if (goalLevel == path.Length - 1) // End of the path.
                    {
                        results.Add(parent);
                    }
                    else
                    {
                        agenda.Push(new object[] { parent, goalLevel + 1 });
                    }
                }
                else if (path[goalLevel] == "." || String.IsNullOrEmpty(path[goalLevel]))
                {
                    if (goalLevel == path.Length - 1) // End of the path. 
                    {
                        results.Add(currentFolder);
                    }
                    else
                    {
                        agenda.Push(new object[] { currentFolder, goalLevel + 1 });
                    }
                }
                else // We need to look at the children
                {
                    if (fetchStale && currentFolder.IsStale)
                        currentFolder.DownloadContents(TimeSpan.FromSeconds(10));

                    foreach (InventoryBase child in currentFolder)
                    {
                        if (child.Name == path[goalLevel] || child.UUID.ToString() == path[goalLevel])
                        {
                            if (goalLevel == path.Length - 1) // End of the path.
                            {
                                results.Add(child);
                            }
                            else
                            {
                                if (child is InventoryFolder)
                                {
                                    agenda.Push(new object[] { child, goalLevel + 1 });
                                }
                            }
                        }
                    }
                }
            }
            return results;
        }

        #endregion Pathing


        #region IEnumerable<InventoryBase> Members

        public IEnumerator<InventoryBase> GetEnumerator()
        {
            lock (Items)
            {
                foreach (KeyValuePair<UUID, InventoryBase> kvp in Items)
                {
                    yield return kvp.Value;
                }
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    public abstract class InventoryBase
    {
        /// <summary>
        /// InventoryBase's item UUID as obtained from ItemData or FolderData.
        /// </summary>
        public abstract UUID UUID
        {
            get;
        }

        /// <summary>
        /// InventoryBase parent's item UUID, as obtained from ItemData or FolderData.
        /// Setting this will not modify the remote inventory, it will only modify the local 
        /// ItemData or FolderData struct.
        /// </summary>
        public abstract UUID ParentUUID
        {
            get;
            set;
        }

        /// <summary>
        /// Inventory base's name, as obtained from ItemData or FolderData.
        /// Setting this will not modify the remote inventory, it will only modify
        /// the local ItemData or FolderData struct.
        /// </summary>
        public abstract string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Inventory base's owner ID, as obtained from ItemData or FolderData.
        /// </summary>
        public abstract UUID OwnerUUID
        {
            get;
        }

        /// <summary>
        /// Gets the parent InventoryFolder referenced by ParentUUID. Returns null
        /// if parent is not managed by the Inventory.
        /// </summary>
        public InventoryFolder Parent
        {
            get
            {
                return Inventory[ParentUUID] as InventoryFolder;
            }
        }

        private Inventory _Inventory;
        protected Inventory Inventory
        {
            get { return _Inventory; }
            private set { _Inventory = value; }
        }

        private InventoryManager _Manager;
        protected InventoryManager Manager
        {
            get { return _Manager; }
            private set { _Manager = value; }
        }

        public InventoryBase(InventoryManager manager, Inventory inventory)
        {
            Inventory = inventory;
            Manager = manager;
        }

        /// <summary>
        /// Moves this InventoryBase to a new folder. Updates local and 
        /// remote inventory.
        /// </summary>
        /// <param name="destination">The folder to move this InventoryBase to.</param>
        public virtual void Move(InventoryFolder destination)
        {
            if (destination.UUID != ParentUUID)
            {
                if (Parent != null)
                {
                    Parent.RemoveChild(this);
                }
                ParentUUID = destination.UUID;
                destination.AddChild(this);
            }
            // Subclass will call the InventoryManager method.
        }

        /// <summary>
        /// Removes this InventoryBase from the local Inventory and its parent's
        /// Contents.
        /// </summary>
        protected internal virtual void LocalRemove()
        {
            if (Parent != null)
                Parent.RemoveChild(this);
            Inventory.Unmanage(this);
        }

        /// <summary>
        /// Changes the parent of this InventoryBase. Removing it from old parent's contents
        /// and adding it to new parent's contents.
        /// </summary>
        /// <param name="newParentUUID">The UUID of the new parent.</param>
        /// <param name="newParent">The InventoryFolder of the new parent. (may be null, if unmanaged)</param>
        protected internal virtual void LocalMove(UUID newParentUUID, InventoryFolder newParent)
        {
            if (ParentUUID != newParentUUID)
            {
                if (Parent != null)
                {
                    Parent.RemoveChild(this);
                }
                if (newParent != null)
                {
                    newParent.AddChild(this);
                }
                ParentUUID = newParentUUID;
            }
        }

        /// <summary>
        /// Removes this InventoryBase from the remote and local inventory.
        /// </summary>
        public virtual void Remove()
        {
            LocalRemove();
            // Subclass will call the InventoryManager method.
        }

        /// <summary>
        /// Renames this InventoryBase, without moving it.
        /// </summary>
        /// <param name="newName">The InventoryBase's new name.</param>
        public virtual void Rename(string newName)
        {
            Name = newName;
            // Subclass will call InventoryManager method.
        }

        public abstract void Give(UUID recipiant, bool particleEffect);

        public void Give(UUID recipiant)
        {
            Give(recipiant, false);
        }

        public override bool Equals(object obj)
        {
            return (obj is InventoryBase) ? (obj as InventoryBase).UUID == UUID : false;
        }

        public override int GetHashCode()
        {
            return UUID.GetHashCode();
        }
    }

    public class InventoryItem : InventoryBase
    {
        public ItemData Data;
        private Asset _Asset;
        public virtual Asset Asset
        {
            get { return _Asset; }
            protected set { _Asset = value; }
        }

        public InventoryItem(InventoryManager manager, Inventory inv, UUID uuid, InventoryType type)
            : this(manager, inv, new ItemData(uuid, type)) { }

        public InventoryItem(InventoryManager manager, Inventory inv, ItemData data)
            : base(manager, inv)
        {
            Data = data;
            if (Parent != null)
                Parent.AddChild(this);
        }

        #region InventoryBase Members

        public override UUID UUID
        {
            get { return Data.UUID; }
        }

        public override UUID ParentUUID
        {
            get { return Data.ParentUUID; }
            set { Data.ParentUUID = value; }
        }

        public override string Name
        {
            get { return Data.Name; }
            set { Data.Name = value; }
        }

        public override UUID OwnerUUID
        {
            get { return Data.OwnerID; }
        }

        /// <summary>
        /// Requests that a copy of this item be made and placed in the <paramref name="destination"/>
        /// folder. This method is not synchronous, and returns immediately. The callback is called
        /// with the new item's inventory data. 
        /// </summary>
        /// <param name="destination">The InventoryFolder to copy this item to.</param>
        /// <param name="callback">The callback to call when the copy is complete.</param>
        public void Copy(InventoryFolder destination, InventoryManager.ItemCopiedCallback callback)
        {
            Manager.RequestCopyItem(UUID, destination.UUID, Data.Name, callback);
        }

        /// <summary>
        /// Synchronously requests a copy of this item be made and placed in the <paramref name="destination"/>
        /// folder. The copy is automatically managed.
        /// </summary>
        /// <param name="destination">Location for the new copy.</param>
        /// <param name="timeout">Amount of time to wait for a server response.</param>
        /// <returns>A managed InventoryItem if copy successful, null if not.</returns>
        public InventoryItem Copy(InventoryFolder destination, TimeSpan timeout)
        {
            ItemData copy;
            if (Manager.CopyItem(UUID, destination.UUID, Name, timeout, out copy))
                return Inventory.Manage(copy) as InventoryItem;
            else
                return null;
        }

        public override void Move(InventoryFolder destination)
        {
            base.Move(destination);
            Manager.MoveItem(UUID, destination.UUID);
        }

        public override void Remove()
        {
            base.Remove();
            Manager.RemoveItem(UUID);
        }

        public override void Rename(string newName)
        {
            base.Rename(newName);
            Manager.RenameItem(UUID, ParentUUID, newName);
        }

        public override void Give(UUID recipiant, bool particleEffect)
        {
            Manager.GiveItem(UUID, Data.Name, Data.AssetType, recipiant, particleEffect);
        }

        #endregion

        /// <summary>
        /// Updates the remote inventory item with the local inventory
        /// ItemData.
        /// </summary>
        public void Update()
        {
            Manager.RequestUpdateItem(Data);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class InventoryFolder : InventoryBase, IEnumerable<InventoryBase>
    {
        /// <summary>
        /// Delegate for <code>InventoryFolder.OnContentsRetrieved</code>
        /// </summary>
        /// <param name="folder">The folder whose contents were retrieved.</param>
        public delegate void ContentsRetrieved(InventoryFolder folder);

        /// <summary>
        /// Delegate for <code>InventoryFolder.OnPartialContents</code>
        /// </summary>
        /// <param name="folder">The folder we're concerned with.</param>
        /// <param name="contents">The contents that were just retrieved.</param>
        /// <param name="remaining">Number of contents remaining to be retrieved.</param>
        public delegate void PartialContents(InventoryFolder folder, ICollection<InventoryBase> contents, int remaining);

        /// <summary>
        /// Triggered when the InventoryFolder's contents have been completely retrieved.
        /// </summary>
        public event ContentsRetrieved OnContentsRetrieved;

        /// <summary>
        /// Triggered when the InventoryFolder's contents have been partially retrieved.
        /// </summary>
        public event PartialContents OnPartialContents;

        public FolderData Data;

        /// <summary>
        /// The local contents of this InventoryFolder. This returns a copy of the 
        /// internal collection, so incurs a memory and CPU penalty. Consider enumerating
        /// directly over the InventoryFolder.
        /// </summary>
        protected Dictionary<UUID, InventoryBase> _Contents;
        public List<InventoryBase> Contents
        {
            get
            {
                lock (_Contents)
                {
                    return new List<InventoryBase>(_Contents.Values);
                }
            }
        }

        /// <summary>
        /// <code>true</code> if all the folder's contents have been downloaded and managed.
        /// <code>false</code> otherwise.
        /// </summary>
        public bool IsStale
        {
            get { return Data.DescendentCount == 0 || _Contents.Count != Data.DescendentCount; }
        }

        public InventoryFolder(InventoryManager manager, Inventory inv, UUID uuid)
            : this(manager, inv, new FolderData(uuid)) { }

        public InventoryFolder(InventoryManager manager, Inventory inv, FolderData data)
            : base(manager, inv)
        {
            Data = data;
            if (data.DescendentCount > 0)
                _Contents = new Dictionary<UUID, InventoryBase>(data.DescendentCount);
            else
                _Contents = new Dictionary<UUID, InventoryBase>();

            if (Parent != null)
                Parent.AddChild(this);
        }

        protected internal void AddChild(InventoryBase child)
        {
            lock (_Contents)
            {
                _Contents[child.UUID] = child;
            }
        }

        protected internal void RemoveChild(InventoryBase child)
        {
            lock (_Contents)
            {
                _Contents.Remove(child.UUID);
            }
        }

        #region InventoryBase Members

        public override UUID UUID
        {
            get { return Data.UUID; }
        }

        public override UUID ParentUUID
        {
            get { return Data.ParentUUID; }
            set { Data.ParentUUID = value; }
        }

        public override string Name
        {
            get { return Data.Name; }
            set { Data.Name = value; }
        }

        public override UUID OwnerUUID
        {
            get { return Data.OwnerID; }
        }

        public override void Move(InventoryFolder destination)
        {
            base.Move(destination);
            Manager.MoveFolder(UUID, destination.UUID);
        }

        public override void Remove()
        {
            base.Remove();
            Manager.RemoveFolder(UUID);
        }

        protected internal override void LocalRemove()
        {
            // Recursively remove all children.

            // First we need to copy the children into our own list, because 
            // calling LocalRemove on the child causes the child to modify
            // our Contents dictionary (through our RemoveChild method)
            // and C# doesn't like iterating through a modified collection.
            List<InventoryBase> children = new List<InventoryBase>(_Contents.Count);
            lock (_Contents)
            {
                foreach (KeyValuePair<UUID, InventoryBase> child in _Contents)
                {
                    children.Add(child.Value);
                }
            }
            // Now actually do the removal:
            foreach (InventoryBase child in children)
            {
                child.LocalRemove();
            }
            base.LocalRemove();
        }

        public override void Rename(string newName)
        {
            base.Rename(newName);
            Manager.RenameFolder(UUID, ParentUUID, newName);
        }

        public override void Give(UUID recipiant, bool particleEffect)
        {
            // Attempt to use local copy of contents, so we dont block waiting to
            // download contents.
            if (!IsStale)
            {
                List<ItemData> itemContents = new List<ItemData>(_Contents.Count);
                foreach (InventoryBase ib in this)
                {
                    if (ib is InventoryItem)
                    {
                        itemContents.Add((ib as InventoryItem).Data);
                    }
                }
                //FIXME: Will we ever want to pass anything other then AssetType.Folder?
                Manager.GiveFolder(UUID, Name, AssetType.Folder, recipiant, particleEffect, itemContents);
            }
            else
            {
                Manager.GiveFolder(UUID, Name, AssetType.Folder, recipiant, particleEffect);
            }
        }

        #endregion

        /// <summary>
        /// Empties the folder, remotely and locally removing all items
        /// in the folder RECURSIVELY. Be careful with this!
        /// </summary>
        public void Empty()
        {
            Manager.RemoveDescendants(UUID);


            List<InventoryBase> children = null;
            lock (_Contents)
            {
                // We need to copy the collection before removing, see comment
                // in LocalRemove method.
                children = new List<InventoryBase>(_Contents.Values);
            }

            foreach (InventoryBase child in children)
            {
                child.LocalRemove();
            }
        }

        /// <summary>
        /// Retrieves the contents of this folder from the remote inventory.
        /// This method is synchronous, and blocks until the contents are retrieved or
        /// the timeout has expired. The contents are written to the 
        /// <code>InventoryFolder.Contents</code> dictionary. If the method times out, 
        /// <code>InventoryFolder.Contents</code> is left unchanged.
        /// The contents retrieved (if successful) are automatically managed.
        /// </summary>
        /// <param name="timeout">TimeSpan to wait for a reply.</param>
        /// <returns><code>true</code> if the contents were retrieved, <code>false</code> if timed out.</returns>
        public bool DownloadContents(TimeSpan timeout)
        {
            List<ItemData> items;
            List<FolderData> folders;
            bool success = Manager.FolderContents(UUID, Data.OwnerID, true, true, InventorySortOrder.ByName,
                timeout, out items, out folders);
            if (success)
            {
                Dictionary<UUID, InventoryBase> contents = new Dictionary<UUID, InventoryBase>(items.Count + folders.Count);
                foreach (ItemData item in items)
                    contents.Add(item.UUID, Inventory.Manage(item));
                foreach (FolderData folder in folders)
                    contents.Add(folder.UUID, Inventory.Manage(folder));

                lock (_Contents)
                    _Contents = contents;

                Data.DescendentCount = Contents.Count;
            }
            return success;
        }

        /// <summary>
        /// Override for RequestContents that retrieves results 
        /// in order by name. 
        /// </summary>
        public void RequestContents()
        {
            RequestContents(InventorySortOrder.ByName);
        }

        /// <summary>
        /// Asynchronously requests the folder's contents from the remote inventory. 
        /// The <code>InventoryFolder.OnContentsRetrieved</code> event 
        /// is raised when the new contents are written to the 
        /// <code>InventoryFolder.Contents</code> Dictionary.
        /// The contents retrieved are automatically managed.
        /// </summary>
        /// <param name="sortOrder">The order in which results are returned.</param>
        public void RequestContents(InventorySortOrder sortOrder)
        {

            Dictionary<UUID, InventoryBase> contents = null;
            if (Data.DescendentCount > 0)
                contents = new Dictionary<UUID, InventoryBase>(Data.DescendentCount);
            else
                contents = new Dictionary<UUID, InventoryBase>();

            InventoryManager.PartialContentsCallback callback =
                delegate(UUID folderid, ItemData[] items, FolderData[] folders, int remaining)
                {
                    if (folderid != UUID)
                        return;

                    if (items != null)
                        foreach (ItemData item in items)
                            contents.Add(item.UUID, Inventory.Manage(item));

                    if (folders != null)
                        foreach (FolderData folder in folders)
                            contents.Add(folder.UUID, Inventory.Manage(folder));

                    if (OnPartialContents != null)
                    {
                        try { OnPartialContents(this, contents.Values, remaining); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, e); }
                    }
                    if (remaining == 0)
                    {
                        lock (_Contents)
                            _Contents = contents;
                        Data.DescendentCount = contents.Count;
                        if (OnContentsRetrieved != null)
                        {
                            try { OnContentsRetrieved(this); }
                            catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, e); }
                        }
                    }
                };
            Manager.RequestFolderContents(UUID, Data.OwnerID, true, true, sortOrder,
                null, callback);
        }

        #region IEnumerable<InventoryBase> Members

        /// <summary>
        /// Enumerates over the local contents of this folder.
        /// Consider calling GetContents or RequestContents before enumerating
        /// to synchronize the local folder contents with the remote folder contents.
        /// </summary>
        /// <returns>An enumerator for this InventoryFolder.</returns>
        public IEnumerator<InventoryBase> GetEnumerator()
        {
            lock (_Contents)
            {
                foreach (KeyValuePair<UUID, InventoryBase> child in _Contents)
                {
                    yield return child.Value;
                }
            }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// <seealso cref="GetEnumerator"/>
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
