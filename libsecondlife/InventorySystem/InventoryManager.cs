/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
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

//#define DEBUG_PACKETS

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using libsecondlife;
using libsecondlife.AssetSystem;
using libsecondlife.Packets;


namespace libsecondlife.InventorySystem
{

    /// <summary>
    /// Summary description for Inventory.
    /// </summary>
    public class InventoryManager
    {

        // Reference to the SLClient Library
        private SecondLife slClient;
//        private ManualResetEvent InventoryManagerInitialized = new ManualResetEvent(false);

        // Reference to the Asset Manager
        internal libsecondlife.AssetSystem.AssetManager AssetManager
        {
            get { return slClient.Assets; }
        }


        // Packet assembly helper
        public InventoryPacketHelper InvPacketHelper = null;

        // Setup a dictionary to easily lookup folders by UUID
        private Dictionary<LLUUID, InventoryFolder> FoldersByUUID = new Dictionary<LLUUID, InventoryFolder>();

        // Setup a dictionary to track download progress
//        protected Dictionary<LLUUID, DownloadRequest_Folder> FolderDownloadStatus = new Dictionary<LLUUID, DownloadRequest_Folder>();
        protected List<DownloadRequest_Folder> FolderRequests = new List<DownloadRequest_Folder>();
        protected bool CurrentlyDownloadingAFolder = false;
        protected DownloadRequest_Folder CurrentlyDownloadingRequest = null;
        private Mutex CurrentlyDownloadingMutex = new Mutex();

        protected Dictionary<sbyte, InventoryFolder> FolderByType = new Dictionary<sbyte, InventoryFolder>();

        // Used to track current item being created
        private InventoryItem iiCreationInProgress;
        public ManualResetEvent ItemCreationCompleted;

        // Used to track to see if a download has timed out or not
//        private int LastPacketRecievedAtTick;

        public enum InventoryType : sbyte
        {
            Unknown = -1,
            Texture = 0,
            Sound = 1,
            CallingCard = 2,
            Landmark = 3,
            [Obsolete]
            Script = 4,
            [Obsolete]
            Clothing = 5,
            Object = 6,
            Notecard = 7,
            Category = 8,
            Folder = 8,
            RootCategory = 0,
            LSL = 10,
            [Obsolete]
            LSLBytecode = 11,
            [Obsolete]
            TextureTGA = 12,
            [Obsolete]
            Bodypart = 13,
            [Obsolete]
            Trash = 14,
            Snapshot = 15,
            [Obsolete]
            LostAndFound = 16,
            Attachment = 17,
            Wearable = 18,
            Animation = 19,
            Gesture = 20
        }

        /// <summary>
        /// Used to turn on debug logging of descendant downloading.
        /// </summary>
        public bool LogDescendantQueue = false;

        /// <summary>
        /// Download event singalling that folder contents have been downloaded.
        /// </summary>
        /// <param name="InventoryFolder">The Inventory Folder that was updated</param>
        /// <param name="e"></param>
        public delegate void On_RequestDownloadContents_Finished(object iFolder, EventArgs e);
        public event On_RequestDownloadContents_Finished OnRequestDownloadFinishedEvent;

        public delegate void On_InventoryItemReceived(LLUUID fromAgentID, string fromAgentName, uint parentEstateID, LLUUID regionID, LLVector3 position, DateTime timestamp, InventoryItem item);
        public event On_InventoryItemReceived OnInventoryItemReceived;

        public delegate void On_InventoryFolderReceived(LLUUID fromAgentID, string fromAgentName, uint parentEstateID, LLUUID regionID, LLVector3 position, DateTime timestamp, InventoryFolder folder);
        public event On_InventoryFolderReceived OnInventoryFolderReceived;

        /// <summary>
        /// Primary constructor
        /// </summary>
        /// <param name="client"></param>
        public InventoryManager(SecondLife client)
        {
            slClient = client;

            InvPacketHelper = new InventoryPacketHelper(slClient);

            // Need to know what when we're connected/disconnected
            slClient.Network.OnConnected += new NetworkManager.ConnectedCallback(Network_OnConnected);
            slClient.Network.OnDisconnected += new NetworkManager.DisconnectedCallback(Network_OnDisconnected);

            // Setup the callback for Inventory Downloads
            slClient.Network.RegisterCallback(PacketType.InventoryDescendents, new NetworkManager.PacketCallback(InventoryDescendentsHandler));

            // Setup the callback for Inventory Creation Update
            slClient.Network.RegisterCallback(PacketType.UpdateCreateInventoryItem, new NetworkManager.PacketCallback(UpdateCreateInventoryItemHandler));

            // Lets listen for inventory being given to us
            slClient.Self.OnInstantMessage += new MainAvatar.InstantMessageCallback(Self_OnInstantMessage);
        }

        #region State Management
        /// <summary>
        /// Inventory Management state should be cleared on connect/disconnect.
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="message"></param>
        void Network_OnDisconnected(NetworkManager.DisconnectType reason, string message)
        {
            // Clear out current state
            ClearState();
        }

        /// <summary>
        /// Inventory Management state should be cleared on connect/disconnect.
        /// </summary>
        /// <param name="sender"></param>
        void Network_OnConnected(object sender)
        {
            // Clear out current state
            ClearState();
        }

        /// <summary>
        /// Reset the current state of the InventorySystem
        /// </summary>
        private void ClearState()
        {
            FoldersByUUID.Clear();

            lock (FolderRequests)
            {
                FolderRequests.Clear();
            }
            if (slClient.Self.InventoryRootFolderUUID != null)
            {
                // Init folder structure with root
                InventoryFolder ifRootFolder = new InventoryFolder(this, "My Inventory", slClient.Self.InventoryRootFolderUUID, null);
                FoldersByUUID[slClient.Self.InventoryRootFolderUUID] = ifRootFolder;
            }
        }
        #endregion

        #region Folder Navigation
        /// <summary>
        /// Get the root folder of a client's inventory
        /// </summary>
        /// <returns></returns>
        public InventoryFolder GetRootFolder()
        {
            return FoldersByUUID[slClient.Self.InventoryRootFolderUUID];
        }

        /// <summary>
        /// Get a specific folder by FolderID from the local cached inventory information
        /// </summary>
        /// <param name="folderID"></param>
        /// <returns>Returns null if the folder doesn't exist in cached inventory</returns>
        public InventoryFolder getFolder(LLUUID folderID)
        {
            if (FoldersByUUID.ContainsKey(folderID))
            {
                return FoldersByUUID[folderID];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get a specific folder by Name from the local cached inventory information
        /// </summary>
        /// <param name="sFolderPath"></param>
        /// <returns></returns>
        public InventoryFolder getFolder(String sFolderPath)
        {
            string sSecretConst = "+@#%$#$%^%^%$^$%SV$#%FR$G";
            sFolderPath = sFolderPath.Replace("//", sSecretConst);

            if (sFolderPath.StartsWith("/"))
            {
                sFolderPath = sFolderPath.Remove(0, 1);
            }

            if (sFolderPath.Length == 0)
            {
                return GetRootFolder();
            }

            char[] seperators = { '/' };
            string[] sFolderPathParts = sFolderPath.Split(seperators);

            for (int i = 0; i < sFolderPathParts.Length; i++)
            {
                sFolderPathParts[i] = sFolderPathParts[i].Replace(sSecretConst, "/");
            }

            Queue<string> pathParts = new Queue<string>(sFolderPathParts);

            return getFolder(pathParts);
        }

        /// <summary>
        /// Recursive helper function for public InventoryFolder getFolder(String sFolderPath)
        /// </summary>
        /// <param name="qFolderPath">Queue</param>
        /// <returns></returns>
        private InventoryFolder getFolder(Queue<string> qFolderPath)
        {
            return getFolder(qFolderPath, GetRootFolder());
        }

        /// <summary>
        /// Recursive helper function for public InventoryFolder getFolder(String sFolderPath)
        /// </summary>
        /// <param name="qFolderPath"></param>
        /// <param name="ifRoot"></param>
        /// <returns></returns>
        private InventoryFolder getFolder(Queue<string> qFolderPath, InventoryFolder ifRoot)
        {
            string sCurFolder = qFolderPath.Dequeue();

            foreach (InventoryBase ibFolder in ifRoot._Contents)
            {
                if (ibFolder is libsecondlife.InventorySystem.InventoryFolder)
                {
                    if (((InventoryFolder)ibFolder).Name.Equals(sCurFolder))
                    {
                        if (qFolderPath.Count == 0)
                        {
                            return (InventoryFolder)ibFolder;
                        }
                        else
                        {
                            return getFolder(qFolderPath, (InventoryFolder)ibFolder);
                        }
                    }
                }
            }

            // Try updating the current level's child folders, then look again
            if (ifRoot.RequestDownloadContents(false, true, false).RequestComplete.WaitOne(1000, false))
            {
                foreach (InventoryBase ibFolder in ifRoot._Contents)
                {
                    if (ibFolder is libsecondlife.InventorySystem.InventoryFolder)
                    {
                        if (((InventoryFolder)ibFolder).Name.Equals(sCurFolder))
                        {
                            // NOTE: We only found it because we did a folder download, 
                            // perhaps we should initiate a recursive download at this point

                            if (qFolderPath.Count == 0)
                            {
                                return (InventoryFolder)ibFolder;
                            }
                            else
                            {
                                return getFolder(qFolderPath, (InventoryFolder)ibFolder);
                            }
                        }
                    }
                }
            }

            return null;
        }
        #endregion

        #region Inventory Creation Functions

        /// <summary>
        /// Request that a folder be created
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parentid"></param>
        internal LLUUID FolderCreate(String name, LLUUID parentid)
        {
            LLUUID requestedFolderID = LLUUID.Random();
            InventoryFolder ifolder = new InventoryFolder(this, name, requestedFolderID, parentid);
            ifolder._Type = -1;

            if (FoldersByUUID.ContainsKey(ifolder.ParentID))
            {
                if (((InventoryFolder)FoldersByUUID[ifolder.ParentID])._Contents.Contains(ifolder) == false)
                {
                    // Add new folder to the contents of the parent folder.
                    ((InventoryFolder)FoldersByUUID[ifolder.ParentID])._Contents.Add(ifolder);
                }
            }
            else
            {
                throw new Exception("Parent Folder " + ifolder.ParentID + " does not exist in this Inventory Manager.");
            }

            if (FoldersByUUID.ContainsKey(ifolder.FolderID) == false)
            {
                FoldersByUUID[ifolder.FolderID] = ifolder;
            }

            Packet packet = InvPacketHelper.CreateInventoryFolder(ifolder.Name, ifolder.ParentID, ifolder.Type, ifolder.FolderID);
            slClient.Network.SendPacket(packet);

            return requestedFolderID;
        }
        /// <summary>
        /// Create a new notecard
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Description"></param>
        /// <param name="Body"></param>
        /// <param name="FolderID"></param>
        /// <returns></returns>
        internal InventoryNotecard NewNotecard(string Name, string Description, string Body, LLUUID FolderID)
        {
            InventoryNotecard iNotecard = new InventoryNotecard(this, Name, Description, FolderID, slClient.Network.AgentID);

            // Create this notecard on the server.
            ItemCreate(iNotecard);

            if ((Body != null) && (Body.Equals("") != true))
            {
                iNotecard.Body = Body;
            }

            return iNotecard;
        }

		internal InventoryLandmark NewLandmark(string Name, string Description, LLUUID FolderID)
        {
            InventoryLandmark iLandmark = new InventoryLandmark(this, Name, Description, FolderID, slClient.Network.AgentID);

            // Create this notecard on the server.
            ItemCreate(iLandmark);

            return iLandmark;
        }
        /// <summary>
        /// Create a new image
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Description"></param>
        /// <param name="j2cdata"></param>
        /// <param name="FolderID"></param>
        /// <returns></returns>
        internal InventoryImage NewImage(string Name, string Description, byte[] j2cdata, LLUUID FolderID)
        {
            InventoryImage iImage = new InventoryImage(this, Name, Description, FolderID, slClient.Network.AgentID);

            // Create this image on the server.
            ItemCreate(iImage);

            if ((j2cdata != null) && (j2cdata.Length != 0))
            {
                iImage.J2CData = j2cdata;
            }

            return iImage;
        }
        #endregion

        #region Folder Management

        /// <summary>
        /// Flushes the local cache of this folder's contents
        /// </summary>
        /// <param name="iFolder"></param>
        /// <param name="Folders">Clear Folders</param>
        /// <param name="Items">Clear Items</param>
        public void FolderClearContents(InventoryFolder iFolder, bool Folders, bool Items)
        {
            // Need to recursively do this...
            while( iFolder._Contents.Count > 0 )
            {
                InventoryBase ib = iFolder._Contents[0];

                if ((ib is InventoryFolder) && Folders)
                {
                    InventoryFolder ChildFolder = (InventoryFolder)ib;
                    FolderClearContents(ChildFolder, Folders, Items);

                    if (FoldersByUUID.ContainsKey(ChildFolder.FolderID))
                    {
                        FoldersByUUID.Remove(ChildFolder.FolderID);
                    }
                    iFolder._Contents.Remove(ib);
                }
                else if (Items)
                {
                    iFolder._Contents.Remove(ib);
                }
            }
        }

        /// <summary>
        /// Delete/Remove a folder
        /// </summary>
        /// <param name="ifolder"></param>
        internal void FolderRemove(InventoryFolder ifolder)
        {
            // Need to recursively remove children
            foreach (InventoryBase ib in ifolder.GetContents())
            {
                if (ib is InventoryFolder)
                {
                    InventoryFolder ifChild = (InventoryFolder)ib;
                    FolderRemove(ifChild);
                }
            }

            // Remove from parent
            if (FoldersByUUID.ContainsKey(ifolder.ParentID))
            {
                InventoryFolder ifParent = FoldersByUUID[ifolder.ParentID];
                if (ifParent._Contents.Contains(ifolder))
                {
                    ifParent._Contents.Remove(ifolder);
                }
            }

            // Remove from lookup cache
            if (FoldersByUUID.ContainsKey(ifolder.FolderID))
            {
                FoldersByUUID.Remove(ifolder.FolderID);
            }

            Packet packet = InvPacketHelper.RemoveInventoryFolder(ifolder.FolderID);
            slClient.Network.SendPacket(packet);
        }

        /// <summary>
        /// Delete/Remove a folder
        /// </summary>
        /// <param name="folderID"></param>
        internal void FolderRemove(LLUUID folderID)
        {
            if (FoldersByUUID.ContainsKey(folderID))
            {
                FolderRemove(FoldersByUUID[folderID]);
            }
        }

        /// <summary>
        /// Move a folder
        /// </summary>
        /// <param name="iFolder"></param>
        /// <param name="newParentID"></param>
        internal void FolderMove(InventoryFolder iFolder, LLUUID newParentID)
        {
            //Remove this folder from the old parent
            if (FoldersByUUID.ContainsKey(iFolder.ParentID))
            {
                InventoryFolder ParentFolder = FoldersByUUID[iFolder.ParentID];
                if (ParentFolder._Contents.Contains(iFolder))
                {
                    ParentFolder._Contents.Remove(iFolder);
                }
            }

            // Set Parent ID
            iFolder._ParentID = newParentID;

            // Add to Parent's contents
            if (FoldersByUUID.ContainsKey(iFolder.ParentID))
            {
                InventoryFolder ParentFolder = FoldersByUUID[iFolder.ParentID];
                if (!ParentFolder._Contents.Contains(iFolder))
                {
                    ParentFolder._Contents.Add(iFolder);
                }
            }


            Packet packet = InvPacketHelper.MoveInventoryFolder(newParentID, iFolder.FolderID);
            slClient.Network.SendPacket(packet);
        }

        /// <summary>
        /// Rename a folder
        /// </summary>
        /// <param name="ifolder"></param>
        internal void FolderRename(InventoryFolder ifolder)
        {
            Packet packet = InvPacketHelper.UpdateInventoryFolder(ifolder.Name, ifolder.ParentID, ifolder.Type, ifolder.FolderID);
            slClient.Network.SendPacket(packet);
        }
        #endregion

        #region Item Management

        /// <summary>
        /// Create a new inventory item
        /// </summary>
        /// <param name="iitem"></param>
        internal void ItemCreate(InventoryItem iitem)
        {
            if( iiCreationInProgress != null )
            {
                throw new Exception("Can only create one item at a time, and an item creation is already in progress.");
            }

            try
            {
                ItemCreationCompleted = new ManualResetEvent(false);
                iiCreationInProgress = iitem;


                Packet packet = InvPacketHelper.CreateInventoryItem(iitem);
                int i = 0;
                do
                {
                    if (i++ > 10)
                        throw new Exception("Could not create " + iitem.Name);
                    slClient.Network.SendPacket(packet);

#if DEBUG_PACKETS
                slClient.DebugLog(packet);
#endif
                } while (!ItemCreationCompleted.WaitOne(5000, false));
            }
            finally
            {
                iiCreationInProgress = null;
            }
        }

        /// <summary>
        /// Update an existing item
        /// </summary>
        /// <param name="iitem"></param>
        internal void ItemUpdate(InventoryItem iitem)
        {
            Packet packet = InvPacketHelper.UpdateInventoryItem(iitem);
            slClient.Network.SendPacket(packet);

            #if DEBUG_PACKETS
                slClient.DebugLog(packet); 
            #endif         
        }

        /// <summary>
        /// Copy an item
        /// </summary>
        /// <param name="ItemID"></param>
        /// <param name="TargetFolderID"></param>
        internal void ItemCopy(LLUUID ItemID, LLUUID TargetFolderID)
        {
            Packet packet = InvPacketHelper.CopyInventoryItem(ItemID, TargetFolderID);
            slClient.Network.SendPacket(packet);

            #if DEBUG_PACKETS
                slClient.DebugLog(packet); 
            #endif         
        }


        internal void MoveItem(LLUUID itemID, LLUUID targetFolderID)
        {
            Packet packet = InvPacketHelper.MoveInventoryItem(itemID, targetFolderID);
            slClient.Network.SendPacket(packet);

            #if DEBUG_PACKETS
                slClient.Log(packet.ToString(), Helpers.LogLevel.Info);
            #endif
        }

        /// <summary>
        /// Give an item to someone
        /// </summary>
        /// <param name="iitem"></param>
        /// <param name="ToAgentID"></param>
        internal void ItemGiveTo(InventoryItem iitem, LLUUID ToAgentID)
        {
            LLUUID MessageID = LLUUID.Random();

            Packet packet = InvPacketHelper.GiveItemViaImprovedInstantMessage(
                MessageID
                , ToAgentID
                , slClient.Self.FirstName + " " + slClient.Self.LastName
                , slClient.Self.Position
                , iitem
                );

            slClient.Network.SendPacket(packet);

            #if DEBUG_PACKETS
                slClient.DebugLog(packet); 
            #endif         
        }

        /// <summary>
        /// Remove/Delete an item
        /// </summary>
        /// <param name="iitem"></param>
        internal void ItemRemove(InventoryItem iitem)
        {
            InventoryFolder ifolder = getFolder(iitem.FolderID);
            ifolder._Contents.Remove(iitem);

            Packet packet = InvPacketHelper.RemoveInventoryItem(iitem.ItemID);
            slClient.Network.SendPacket(packet);

            #if DEBUG_PACKETS
                slClient.DebugLog(packet); 
            #endif         
        }

        #endregion

        #region Misc

        /// <summary>
        /// Rez the given item into the given sim.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="TargetSim">You can specify null to use the current sim</param>
        /// <param name="TargetPos">Position is in Region coordinates</param>
        internal void ItemRezObject(InventoryItem item, Simulator TargetSim, LLVector3 TargetPos)
        {
            Packet packet = InvPacketHelper.RezObject(item, TargetPos);
            if (TargetSim == null)
            {
                slClient.Network.SendPacket(packet);
            }
            else
            {
                slClient.Network.SendPacket(packet, TargetSim);
            }
        }

        /// <summary>
        /// Attempt to rez and attach an inventory item 
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="AttachmentPt"></param>
        internal void ItemRezAttach(InventoryItem Item, ObjectManager.AttachmentPoint AttachmentPt)
        {
            Packet p = InvPacketHelper.RezSingleAttachmentFromInv(Item, AttachmentPt);
            slClient.Network.SendPacket(p);
        }

        /// <summary>
        /// Attempt to detach and return an item to your inventory
        /// </summary>
        /// <param name="Item"></param>
        internal void ItemDetach(InventoryItem Item)
        {
            Packet p = InvPacketHelper.DetachAttachmentIntoInv(Item.ItemID);
            slClient.Network.SendPacket(p);
        }
        #endregion


        #region Folder Downloading

        protected void LogDescendantQueueEvent(string msg)
        {
            if (LogDescendantQueue)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("==============================");
                sb.AppendLine(msg);

                if (CurrentlyDownloadingRequest == null)
                {
                    sb.AppendLine("CurrentlyDownloadingRequest: NULL");
                }
                else
                {
                    sb.AppendLine("CurrentlyDownloadingRequest: " + CurrentlyDownloadingRequest.ToString());
                }

                sb.AppendLine("Current queue status:");

                lock (FolderRequests)
                {
                    if (FolderRequests.Count == 0)
                    {
                        sb.AppendLine(" *** Download Queue Empty ***");
                    }
                    else
                    {
                        foreach (DownloadRequest_Folder dr in FolderRequests)
                        {
                            sb.AppendLine(" * " + dr.ToString());
                        }
                    }
                }
                
                slClient.Log(sb.ToString(), Helpers.LogLevel.Info);
            }
        }

        /// <summary>
        /// Append a request to the end of the queue.
        /// </summary>
        internal DownloadRequest_Folder FolderRequestAppend(LLUUID folderID, bool recurse, bool fetchFolders, bool fetchItems, string requestName)
        {

            DownloadRequest_Folder dr = new DownloadRequest_Folder(folderID, recurse, fetchFolders, fetchItems, requestName);

            // Add new request to the tail of the queue
            lock (FolderRequests)
            {
                if (FolderRequests.Contains(dr))
                {
                    foreach (DownloadRequest_Folder existing in FolderRequests)
                    {
                        if (dr.Equals(existing))
                        {
                            dr = existing;
                            break;
                        }
                    }
                    LogDescendantQueueEvent("Append(returned existing): " + dr.ToString());
                }
                else
                {
                    FolderRequests.Add(dr);
                    LogDescendantQueueEvent("Append: " + dr.ToString());
                }
            }

            FolderRequestBegin();
            return dr;
        }

        protected DownloadRequest_Folder FolderRequestPrepend(LLUUID folderID, bool recurse, bool fetchFolders, bool fetchItems, string requestName)
        {
            DownloadRequest_Folder dr = new DownloadRequest_Folder(folderID, recurse, fetchFolders, fetchItems, requestName);

            // Prepend the request at the head of the queue
            lock (FolderRequests)
            {
                if (FolderRequests.Contains(dr))
                {
                    foreach (DownloadRequest_Folder existing in FolderRequests)
                    {
                        if (dr.Equals(existing))
                        {
                            dr = existing;
                            break;
                        }
                    }

                    LogDescendantQueueEvent("Append(returned existing): " + dr.ToString());
                }
                else
                {
                    FolderRequests.Insert(0, dr);
                    LogDescendantQueueEvent("Prepend: " + dr.ToString());
                }
            }
            return dr;
        }

        /// <summary>
        /// If not currently downloading a request, dequeue the next request and start it.
        /// </summary>
        protected void FolderRequestBegin()
        {
            // Wait until it's safe to be modifying what is currently downloading.
            CurrentlyDownloadingMutex.WaitOne();

            // If we not already downloading stuff, then lets start
            if (CurrentlyDownloadingAFolder == false)
            {
                // Start downloading the first thing at the head of the queue
                lock (FolderRequests)
                {
                    while ((FolderRequests.Count > 0) && (FolderRequests[0].IsCompleted))
                    {
                        LogDescendantQueueEvent("Head request completed, notify recurse completed: " + FolderRequests[0]);
                        FolderRequests.RemoveAt(0);
                    }

                    if (FolderRequests.Count > 0)
                    {
                        CurrentlyDownloadingRequest = FolderRequests[0];
                        LogDescendantQueueEvent("Starting download of head of queue: " + FolderRequests[0].ToString());
                    }
                    else
                    {
                        // Nothing to do

                        // Release so that we can let other things look at and modify what is currently downloading.
                        CurrentlyDownloadingMutex.ReleaseMutex();

                        return;
                    }
                }

                // Mark that we're currently downloading
                CurrentlyDownloadingAFolder = true;

                // Download!
                Packet packet = InvPacketHelper.FetchInventoryDescendents(
                                CurrentlyDownloadingRequest.FolderID
                                , CurrentlyDownloadingRequest.FetchFolders
                                , CurrentlyDownloadingRequest.FetchItems);

                slClient.Network.SendPacket(packet);
            }

            // Release so that we can let other things look at and modify what is currently downloading.
            CurrentlyDownloadingMutex.ReleaseMutex();
        }

        /// <summary>
        /// Issue a RequestDownload Finished event.  Happens after each download request completes.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        protected void FireRequestDownloadFinishedEvent(object o, EventArgs e)
        {
            if (OnRequestDownloadFinishedEvent != null)
            {
                OnRequestDownloadFinishedEvent(o, e);
            }
        }

        #endregion

        #region libsecondlife callback handlers

        /// <summary>
        /// Used to track when inventory is dropped onto/into agent
        /// </summary>
        /// <param name="fromAgentID"></param>
        /// <param name="fromAgentName"></param>
        /// <param name="toAgentID"></param>
        /// <param name="parentEstateID"></param>
        /// <param name="regionID"></param>
        /// <param name="position"></param>
        /// <param name="dialog"></param>
        /// <param name="groupIM"></param>
        /// <param name="imSessionID"></param>
        /// <param name="timestamp"></param>
        /// <param name="message"></param>
        /// <param name="offline"></param>
        /// <param name="binaryBucket"></param>
        void Self_OnInstantMessage(LLUUID fromAgentID, string fromAgentName, LLUUID toAgentID, uint parentEstateID, 
            LLUUID regionID, LLVector3 position, MainAvatar.InstantMessageDialog dialog, bool groupIM, 
            LLUUID imSessionID, DateTime timestamp,  string message, MainAvatar.InstantMessageOnline offline, 
            byte[] binaryBucket)
        {
            if ((dialog == MainAvatar.InstantMessageDialog.InventoryOffered) && ((OnInventoryItemReceived != null) || (OnInventoryFolderReceived !=null)))
            {
                sbyte IncomingItemType = (sbyte)binaryBucket[0];
                LLUUID IncomingUUID = new LLUUID(binaryBucket, 1);

                // Update root folders
                InventoryFolder root = GetRootFolder();
                if (root.GetContents().Count == 0)
                {
                    root.RequestDownloadContents(false, true, false).RequestComplete.WaitOne(3000, false);
                }

                // Handle the case of the incoming inventory folder
                if (IncomingItemType == (sbyte)InventoryManager.InventoryType.Folder)
                {
                    if (OnInventoryFolderReceived == null)
                    {
                        // Short-circuit early exit, we're not interested...
                        return;
                    }

                    InventoryFolder iFolder = null;
                    int numAttempts = 6;
                    int timeBetweenAttempts = 500;
                    while( numAttempts-- > 0 )
                    {
                        foreach( InventoryBase ib in root.GetContents() )
                        {
                            if (ib is InventoryFolder)
                            {
                                InventoryFolder tiFolder = (InventoryFolder)ib;
                                if (tiFolder.FolderID == IncomingUUID)
                                {
                                    iFolder = tiFolder;
                                    break;
                                }
                            }
                        }
                        if ( iFolder != null)
                        {
                            try { OnInventoryFolderReceived(fromAgentID, fromAgentName, parentEstateID, regionID, position, timestamp, iFolder); }
                            catch (Exception e) { slClient.Log(e.ToString(), Helpers.LogLevel.Error); }
                            return;
                        } else {
                            Thread.Sleep(timeBetweenAttempts);
                            timeBetweenAttempts *= 2;
                            root.RequestDownloadContents(false, true, false).RequestComplete.WaitOne(3000, false);
                        }
                    }

                    slClient.Log("Incoming folder [" + IncomingUUID.ToStringHyphenated() + "] not found in inventory.", Helpers.LogLevel.Error);
                    return;
                }

                if (OnInventoryItemReceived == null)
                {
                    // Short-circuit, early exit, we're not interested
                    return;
                }

                // Make sure we have a folder lookup by type table ready.
                lock (FolderByType)
                {
                    if (FolderByType.Count == 0)
                    {
                        foreach (InventoryBase ib in root.GetContents())
                        {
                            if (ib is InventoryFolder)
                            {
                                InventoryFolder iFolder = (InventoryFolder)ib;
                                FolderByType[iFolder.Type] = iFolder;
                            }
                        }
                    }
                }

                // Get a reference to the incoming/receiving folder
                if (!FolderByType.ContainsKey(IncomingItemType))
                {
                    slClient.Log("Incoming item specifies type (" + IncomingItemType  + ") with no matching inventory folder found.", Helpers.LogLevel.Error);
                }


                InventoryFolder incomingFolder = FolderByType[IncomingItemType];
                InventoryItem incomingItem = null;

                // lock just incase another item comes into the same directory while processing this one.
                lock (incomingFolder)
                {
                    // Refresh contents of receiving folder
                    incomingFolder.RequestDownloadContents(false, false, true).RequestComplete.WaitOne(3000, false);

                    int numAttempts = 2;
                    while( numAttempts-- > 0 )
                    {
                        // Search folder for incoming item
                        foreach (InventoryBase ib2 in incomingFolder.GetContents())
                        {
                            if (ib2 is InventoryItem)
                            {
                                InventoryItem tiItem = (InventoryItem)ib2;

                                if (tiItem.ItemID == IncomingUUID)
                                {
                                    incomingItem = tiItem;
                                    break;
                                }
                            }
                        }
                        // If found, send out notification
                        if (incomingItem != null)
                        {
                            try { OnInventoryItemReceived(fromAgentID, fromAgentName, parentEstateID, regionID, position, timestamp, incomingItem); }
                            catch (Exception e) { slClient.Log(e.ToString(), Helpers.LogLevel.Error); }
                            return;
                        }
                        else
                        {
                            Thread.Sleep(500);
                            incomingFolder.RequestDownloadContents(false, false, true).RequestComplete.WaitOne(3000, false);
                        }
                    }
                }
                slClient.Log("Incoming item/folder [" + IncomingUUID.ToStringHyphenated() + "] not found in inventory.", Helpers.LogLevel.Error);

            }
        }



        /// <summary>
        /// This is called in response to an item creation request
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        public void UpdateCreateInventoryItemHandler(Packet packet, Simulator simulator)
        {
            #if DEBUG_PACKETS
                slClient.DebugLog(packet);
            #endif

            if (iiCreationInProgress != null)
            {
                UpdateCreateInventoryItemPacket reply = (UpdateCreateInventoryItemPacket)packet;

                // Use internal variable references, so we don't fire off any update code by using the public accessors

                iiCreationInProgress._ItemID = reply.InventoryData[0].ItemID;

                iiCreationInProgress._GroupOwned = reply.InventoryData[0].GroupOwned;
                iiCreationInProgress._SaleType = reply.InventoryData[0].SaleType;
                iiCreationInProgress._CreationDate = reply.InventoryData[0].CreationDate;
                iiCreationInProgress._BaseMask = reply.InventoryData[0].BaseMask;

                iiCreationInProgress._Name = Helpers.FieldToUTF8String(reply.InventoryData[0].Name);
                iiCreationInProgress._InvType = reply.InventoryData[0].InvType;
                iiCreationInProgress._Type = reply.InventoryData[0].Type;
                iiCreationInProgress._AssetID = reply.InventoryData[0].AssetID;
                iiCreationInProgress._GroupID = reply.InventoryData[0].GroupID;
                iiCreationInProgress._SalePrice = reply.InventoryData[0].SalePrice;
                iiCreationInProgress._OwnerID = reply.InventoryData[0].OwnerID;
                iiCreationInProgress._CreatorID = reply.InventoryData[0].CreatorID;
                iiCreationInProgress._ItemID = reply.InventoryData[0].ItemID;
                iiCreationInProgress._FolderID = reply.InventoryData[0].FolderID;
                iiCreationInProgress._EveryoneMask = reply.InventoryData[0].EveryoneMask;
                iiCreationInProgress._Description = Helpers.FieldToUTF8String(reply.InventoryData[0].Description);
                iiCreationInProgress._NextOwnerMask = reply.InventoryData[0].NextOwnerMask;
                iiCreationInProgress._GroupMask = reply.InventoryData[0].GroupMask;
                iiCreationInProgress._OwnerMask = reply.InventoryData[0].OwnerMask;

                // NOT USED YET: iiCreationInProgress._CallbackID = reply.InventoryData[0].CallbackID;

                ItemCreationCompleted.Set();
            }
            else
            {
                slClient.DebugLog(packet.ToString());

                // TODO:  Looks like this packet may be sent in response to a Buy.  
                //        Should probably use it to update local cached inventory, to show the bought item(s)
                //
                // throw new Exception("Received a packet for item creation, but no such response was expected.  This is probably a bad thing...");
            }
        }

        /// <summary>
        /// Returned in response to a FetchInventoryDescendents request.  Contains information about the
        /// contents of a folder.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        public void InventoryDescendentsHandler(Packet packet, Simulator simulator)
        {
            InventoryDescendentsPacket reply = (InventoryDescendentsPacket)packet;

            // The UUID of this folder.
            LLUUID uuidFolderID = reply.AgentData.FolderID;

            // Wait until it's safe to be looking at what is currently downloading.
            CurrentlyDownloadingMutex.WaitOne();

            // Make sure this request matches the one we believe is the currently downloading request
            if (((CurrentlyDownloadingRequest != null) && (CurrentlyDownloadingRequest.FolderID != uuidFolderID)) || (CurrentlyDownloadingRequest == null))
            {
                // Release so that we can let other things look at and modify what is currently downloading.
                CurrentlyDownloadingMutex.ReleaseMutex();

                // Log problem
                LogDescendantQueueEvent("Unexpected descendent packet for folder: " + uuidFolderID.ToStringHyphenated());

                // Just discard this packet...
                return;
            }

            // Get the Inventory folder that we'll be updating
            InventoryFolder InvFolderUpdating = (InventoryFolder)FoldersByUUID[uuidFolderID];


            // Update Inventory Manager's last tick point, used for timeouts and such
//            LastPacketRecievedAtTick = Environment.TickCount;

            // Used to count the number of descendants received to see if we're finished or not.
            int iDescendentsExpected = reply.AgentData.Descendents;
            int iDescendentsReceivedThisBlock = 0;

            #region Handle Child Items
            foreach (InventoryDescendentsPacket.ItemDataBlock itemBlock in reply.ItemData)
            {
                // There is always an item block, even if there isn't any items
                // the "filler" block will not have a name
                if (itemBlock.Name.Length != 0)
                {
                    iDescendentsReceivedThisBlock++;

                    if (itemBlock.ItemID == LLUUID.Zero)
                    {
                        // this shouldn't ever happen, unless you've uploaded an invalid item
                        // to yourself while developping inventory code :-(
                    }
                    else
                    {
                        InventoryItem TempInvItem = new InventoryItem(this, itemBlock);

                        if (InvFolderUpdating._Contents.Contains(TempInvItem) == false)
                        {
                            #region Create an instance of the appriopriate Inventory class
                            if ((TempInvItem.InvType == 7) && (TempInvItem.Type == (sbyte)Asset.AssetType.Notecard))
                            {
                                InventoryItem temp = new InventoryNotecard(this, TempInvItem);
                                TempInvItem = temp;
                            }
                            if ((TempInvItem.InvType == 3) && (TempInvItem.Type == (sbyte)Asset.AssetType.Landmark))
                            {
                                InventoryItem temp = new InventoryLandmark(this, TempInvItem);
                                TempInvItem = temp;
                            }

                            if ((TempInvItem.InvType == 0) && (TempInvItem.Type == (sbyte)Asset.AssetType.Texture))
                            {
                                InventoryItem temp = new InventoryImage(this, TempInvItem);
                                TempInvItem = temp;
                            }

                            if ((TempInvItem.InvType == 10) && (TempInvItem.Type == (sbyte)Asset.AssetType.LSLText))
                            {
                                InventoryItem temp = new InventoryScript(this, TempInvItem);
                                TempInvItem = temp;
                            }

                            if ((TempInvItem.InvType == 18) &&
                                (
                                    (TempInvItem.Type == (sbyte)Asset.AssetType.Bodypart)
                                    || (TempInvItem.Type == (sbyte)Asset.AssetType.Clothing)
                                )
                               )
                            {
                                InventoryItem temp = new InventoryWearable(this, TempInvItem);
                                TempInvItem = temp;
                            }
                            #endregion

                            InvFolderUpdating._Contents.Add(TempInvItem);
                        }
                    }
                }
            }
            #endregion

            #region Handle Child Folders
            foreach (InventoryDescendentsPacket.FolderDataBlock folderBlock in reply.FolderData)
            {
                String IncomingName = System.Text.Encoding.UTF8.GetString(folderBlock.Name).Trim().Replace("\0", "");
                LLUUID IncomingFolderID = folderBlock.FolderID;
                LLUUID IncomingParentID = folderBlock.ParentID;
                sbyte IncomingType = folderBlock.Type;

                // There is always an folder block, even if there isn't any folders
                // the "filler" block will not have a name
                if (folderBlock.Name.Length != 0)
                {
                    iDescendentsReceivedThisBlock++;

                    // See if the Incoming Folder already exists locally
                    if (FoldersByUUID.ContainsKey(IncomingFolderID))
                    {
                        InventoryFolder existingFolder = FoldersByUUID[IncomingFolderID];
                        existingFolder._Name = IncomingName;
                        existingFolder._Type = IncomingType;

                        // Check if parent of existing is the same as the incoming
                        if (!existingFolder.ParentID.Equals(IncomingParentID))
                        {
                            // Remove existing from old parent
                            if (FoldersByUUID.ContainsKey(existingFolder.ParentID))
                            {
                                InventoryFolder ExistingParent = FoldersByUUID[existingFolder.ParentID];
                                if (ExistingParent._Contents.Contains(existingFolder))
                                {
                                    ExistingParent._Contents.Remove(existingFolder);
                                }
                            }

                            // Set existings parent to new
                            existingFolder._ParentID = IncomingParentID;

                            // Connect existing folder to parent specified in new
                            if (FoldersByUUID.ContainsKey(IncomingParentID))
                            {
                                InventoryFolder ExistingParent = FoldersByUUID[IncomingParentID];
                                if (!ExistingParent._Contents.Contains(existingFolder))
                                {
                                    ExistingParent._Contents.Add(existingFolder);
                                }
                            }
                        }
                    }
                    else
                    {
                        InventoryFolder TempInvFolder = new InventoryFolder(this, IncomingName, IncomingFolderID, IncomingParentID, IncomingType);

                        // Add folder to Parent
                        if (InvFolderUpdating._Contents.Contains(TempInvFolder) == false)
                        {
                            InvFolderUpdating._Contents.Add(TempInvFolder);
                        }

                        // Add folder to local cache lookup
                        FoldersByUUID[TempInvFolder.FolderID] = TempInvFolder;

                    }


                    // Do we recurse?
                    if (CurrentlyDownloadingRequest.Recurse)
                    {
                        // It's not the root, should be safe to "recurse"
                        if (!IncomingFolderID.Equals(slClient.Self.InventoryRootFolderUUID))
                        {
                            FolderRequestPrepend(IncomingFolderID, CurrentlyDownloadingRequest.Recurse, CurrentlyDownloadingRequest.FetchFolders, CurrentlyDownloadingRequest.FetchItems, CurrentlyDownloadingRequest.Name + "/" + IncomingName);
                        }
                    }
                }
            }
            #endregion

            // Update total number of descendants expected , and update the total downloaded
            CurrentlyDownloadingRequest.Expected = iDescendentsExpected;
            CurrentlyDownloadingRequest.Received += iDescendentsReceivedThisBlock;
            CurrentlyDownloadingRequest.LastReceivedAtTick = Environment.TickCount;

            if ((iDescendentsExpected > 1) && (iDescendentsReceivedThisBlock == 0))
            {
                slClient.Log("Received an InventoryDescendant packet where it indicated that there should be at least 1 descendant, but none were present... [" + CurrentlyDownloadingRequest.Name + "]", Helpers.LogLevel.Warning); 
                CurrentlyDownloadingRequest.Expected = 0;
            }

            if (LogDescendantQueue)
            {
                slClient.Log("Received packet for: " + CurrentlyDownloadingRequest.ToString(), Helpers.LogLevel.Info);
            }

            // Check if we're finished
            if (CurrentlyDownloadingRequest.Received >= CurrentlyDownloadingRequest.Expected)
            {
                LogDescendantQueueEvent("Done downloading request: " + CurrentlyDownloadingRequest);

                // Singal anyone that was waiting for this request to finish
                CurrentlyDownloadingRequest.RequestComplete.Set();

                // Raise an event for anyone that cares to listen for downloaded folder events
                if (OnRequestDownloadFinishedEvent != null)
                {
                    DownloadRequest_EventArgs e = new DownloadRequest_EventArgs();
                    e.DownloadRequest = CurrentlyDownloadingRequest;
                    FireRequestDownloadFinishedEvent(InvFolderUpdating, e);
                }

                // Set Inventory Manager state to reflect that we're done with the current download
                CurrentlyDownloadingAFolder = false;
                CurrentlyDownloadingRequest = null;
            }

            // Release so that we can let other things look at and modify what is currently downloading.
            CurrentlyDownloadingMutex.ReleaseMutex();


            // If there's any more download requests queued, grab one, and go
            FolderRequestBegin();
        }
        #endregion
    }
}
