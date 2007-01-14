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
        private ManualResetEvent InventoryManagerInitialized = new ManualResetEvent(false);

        // Reference to the Asset Manager
        internal AssetManager AssetManager
        {
            get { return slClient.Assets; }
        }


        // Packet assembly helper
        public InventoryPacketHelper InvPacketHelper = null;

        // Setup a dictionary to easily lookup folders by UUID
        private Dictionary<LLUUID, InventoryFolder> FoldersByUUID = new Dictionary<LLUUID, InventoryFolder>();

        // Setup a dictionary to track download progress
        private Dictionary<LLUUID, DownloadRequest_Folder> FolderDownloadStatus = new Dictionary<LLUUID, DownloadRequest_Folder>();
        private List<DownloadRequest_Folder> alFolderRequestQueue = new List<DownloadRequest_Folder>();

        // Used to track current item being created
        private InventoryItem iiCreationInProgress;
        public ManualResetEvent ItemCreationCompleted;

        // Used to track to see if a download has timed out or not
        private int LastPacketRecievedAtTick;


        /// <summary>
        /// Download event singalling that folder contents have been downloaded.
        /// </summary>
        /// <param name="InventoryFolder">The Inventory Folder that was updated</param>
        /// <param name="e"></param>
        public delegate void On_RequestDownloadContents_Finished(object iFolder, EventArgs e);
        public event On_RequestDownloadContents_Finished RequestDownloadFinishedEvent;

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
            slClient.Network.OnDisconnected += new NetworkManager.DisconnectCallback(Network_OnDisconnected);

            // Setup the callback for Inventory Downloads
            slClient.Network.RegisterCallback(PacketType.InventoryDescendents, new NetworkManager.PacketCallback(InventoryDescendentsHandler));

            // Setup the callback for Inventory Creation Update
            slClient.Network.RegisterCallback(PacketType.UpdateCreateInventoryItem, new NetworkManager.PacketCallback(UpdateCreateInventoryItemHandler));

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
            FolderDownloadStatus.Clear();
            alFolderRequestQueue.Clear();

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
        /// <param name="qFolderPath">Queue<\string\></param>
        /// <returns></returns>
        private InventoryFolder getFolder(Queue<string> qFolderPath)
        {
            return getFolder(qFolderPath, GetRootFolder());
        }

        /// <summary>
        /// Recursive helper function for public InventoryFolder getFolder(String sFolderPath)
        /// </summary>
        /// <see cref="getFolder(string sFolderPath)"/>
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
            if (ifRoot.RequestDownloadContents(false, true, false, false).RequestComplete.WaitOne(1000, false))
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
        /// Create a folder
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parentid"></param>
        /// <returns></returns>
        internal InventoryFolder FolderCreate(String name, LLUUID parentid)
        {
            InventoryFolder ifolder = new InventoryFolder(this, name, LLUUID.Random(), parentid);
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

            return ifolder;
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
        internal void FolderClearContents(InventoryFolder iFolder, bool Folders, bool Items)
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

        /// <summary>
        /// Request the download of a folder's contents
        /// </summary>
        /// <param name="dr"></param>
        internal void RequestFolder(DownloadRequest_Folder dr)
        {
            
            FolderDownloadStatus[dr.FolderID] = dr;

            Packet packet = InvPacketHelper.FetchInventoryDescendents(
                            dr.FolderID
                            , dr.FetchFolders
                            , dr.FetchItems);

            slClient.Network.SendPacket(packet);
        }

        /// <summary>
        /// Downloads your entire inventory structure.
        /// </summary>
        /// <remarks>Not recommended!  This can take a long time for larger downloads and is subject to timeouts</remarks>
        [Obsolete("This is a big nasty evil blocking function that you shouldn't use any more.")]
        public void DownloadInventory()
        {
            ClearState();

            if (FolderDownloadStatus == null)
            {
                // Create status table
                FolderDownloadStatus = new Dictionary<LLUUID, DownloadRequest_Folder>();
            }
            else
            {
                if (FolderDownloadStatus.Count != 0)
                {
                    throw new Exception("Inventory Download requested while previous download in progress.");
                }
            }

            if (alFolderRequestQueue == null)
            {
                alFolderRequestQueue = new List<DownloadRequest_Folder>();
            }

            // Set last packet received to now, just so out time-out timer works
            LastPacketRecievedAtTick = Environment.TickCount;

            // Send Packet requesting the root Folder, 
            // this should recurse through all folders
            RequestFolder(new DownloadRequest_Folder(slClient.Self.InventoryRootFolderUUID, true, true, true));

            while ((FolderDownloadStatus.Count > 0) || (alFolderRequestQueue.Count > 0))
            {
                if (FolderDownloadStatus.Count == 0)
                {
                    DownloadRequest_Folder dr = alFolderRequestQueue[0];
                    alFolderRequestQueue.RemoveAt(0);
                    RequestFolder(dr);
                }

                int curTick = Environment.TickCount;
                if ((curTick - LastPacketRecievedAtTick) > 10000)
                {
                    slClient.Log("Time-out while waiting for packets (" +
                        ((curTick - LastPacketRecievedAtTick) / 1000) + " seconds since last packet)",
                        Helpers.LogLevel.Warning);

                    // have to make a seperate list otherwise we run into modifying the original array
                    // while still enumerating it.
                    List<DownloadRequest_Folder> alRestartList = new List<DownloadRequest_Folder>();


                    foreach (DownloadRequest_Folder dr in FolderDownloadStatus.Values)
                    {
                        alRestartList.Add(dr);
                    }

                    LastPacketRecievedAtTick = Environment.TickCount;
                    foreach (DownloadRequest_Folder dr in alRestartList)
                    {
                        RequestFolder(dr);
                    }
                }

                slClient.Tick();
            }
        }

        protected void FireRequestDownloadFinishedEvent(object o, EventArgs e)
        {
            if (RequestDownloadFinishedEvent != null)
            {
                RequestDownloadFinishedEvent(o, e);
            }
        }

        #endregion

        #region libsecondlife callback handlers

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

                iiCreationInProgress._Name = Helpers.FieldToString(reply.InventoryData[0].Name);
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
                iiCreationInProgress._Description = Helpers.FieldToString(reply.InventoryData[0].Description);
                iiCreationInProgress._NextOwnerMask = reply.InventoryData[0].NextOwnerMask;
                iiCreationInProgress._GroupMask = reply.InventoryData[0].GroupMask;
                iiCreationInProgress._OwnerMask = reply.InventoryData[0].OwnerMask;

                // NOT USED YET: iiCreationInProgress._CallbackID = reply.InventoryData[0].CallbackID;

                ItemCreationCompleted.Set();
            }
            else
            {
                slClient.DebugLog(packet.ToString());
                throw new Exception("Received a packet for item creation, but no such response was expected.  This is probably a bad thing...");
            }
        }

        /// <summary>
        /// Returned in response to a FetchInventoryDescendents request.  Contains information about the
        /// contents of a folder.
        /// </summary>
        /// <seealso cref="InventoryManager.RequestFolder"/>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        public void InventoryDescendentsHandler(Packet packet, Simulator simulator)
        {
            InventoryDescendentsPacket reply = (InventoryDescendentsPacket)packet;

            // The UUID of this folder.
            LLUUID uuidFolderID = reply.AgentData.FolderID;

            // Get the original Descendent Request for this Packet
            DownloadRequest_Folder dr = (DownloadRequest_Folder)FolderDownloadStatus[uuidFolderID];

            // Get the Inventory folder that we'll be updating
            InventoryFolder InvFolderUpdating = (InventoryFolder)FoldersByUUID[uuidFolderID];

            // Update Inventory Manager's last tick point, used for timeouts and such
            LastPacketRecievedAtTick = Environment.TickCount;

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
                            if ((TempInvItem.InvType == 7) && (TempInvItem.Type == Asset.ASSET_TYPE_NOTECARD))
                            {
                                InventoryItem temp = new InventoryNotecard(this, TempInvItem);
                                TempInvItem = temp;
                            }

                            if ((TempInvItem.InvType == 0) && (TempInvItem.Type == Asset.ASSET_TYPE_IMAGE))
                            {
                                InventoryItem temp = new InventoryImage(this, TempInvItem);
                                TempInvItem = temp;
                            }

                            if ( (TempInvItem.InvType == 10) && (TempInvItem.Type == Asset.ASSET_TYPE_SCRIPT) )
                            {
                                InventoryItem temp = new InventoryScript(this, TempInvItem);
                                TempInvItem = temp;
                            }

                            if ((TempInvItem.InvType == 18) && 
                                (
                                    (TempInvItem.Type == Asset.ASSET_TYPE_WEARABLE_BODY)
                                    || (TempInvItem.Type == Asset.ASSET_TYPE_WEARABLE_CLOTHING)
                                )
                               )

                            {
                                InventoryItem temp = new InventoryWearable(this, TempInvItem);
                                TempInvItem = temp;
                            }

                            InvFolderUpdating._Contents.Add(TempInvItem);
                        }
                    }
                }
            }
            #endregion

            #region Handle Child Folders
            foreach (InventoryDescendentsPacket.FolderDataBlock folderBlock in reply.FolderData)
            {
                String IncomingName     = System.Text.Encoding.UTF8.GetString(folderBlock.Name).Trim().Replace("\0", "");
                LLUUID IncomingFolderID = folderBlock.FolderID;
                LLUUID IncomingParentID = folderBlock.ParentID;
                sbyte  IncomingType     = folderBlock.Type;

                // There is always an folder block, even if there isn't any folders
                // the "filler" block will not have a name
                if (folderBlock.Name.Length != 0)
                {
                    iDescendentsReceivedThisBlock++;

                    // See if the Incoming Folder already exists locally
                    if (FoldersByUUID.ContainsKey(IncomingFolderID))
                    {
                        Console.WriteLine("Updating existing folder entry.");

                        InventoryFolder existingFolder = FoldersByUUID[IncomingFolderID];
                        existingFolder._Name = IncomingName;
                        existingFolder._Type = IncomingType;

                        // Check if parent of existing is the same as the incoming
                        if (!existingFolder.ParentID.Equals(IncomingParentID))
                        {
                            Console.WriteLine("* New Parent :: " + existingFolder.ParentID + " != " + IncomingParentID);
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
                        InventoryFolder TempInvFolder = new InventoryFolder(this, IncomingName, IncomingFolderID, IncomingParentID);

                        // Add folder to Parent
                        if (InvFolderUpdating._Contents.Contains(TempInvFolder) == false)
                        {
                            InvFolderUpdating._Contents.Add(TempInvFolder);
                        }

                        // Add folder to local cache lookup
                        FoldersByUUID[TempInvFolder.FolderID] = TempInvFolder;

                    }


                    // Do we recurse?
                    if (dr.Recurse)
                    {
                        // It's not the root, should be safe to "recurse"
                        if (!IncomingFolderID.Equals(slClient.Self.InventoryRootFolderUUID))
                        {
                            // Check if a download for this folder is already queued
                            bool alreadyQueued = false;
                            foreach (DownloadRequest_Folder adr in alFolderRequestQueue)
                            {
                                if (adr.FolderID == IncomingFolderID)
                                {
                                    alreadyQueued = true;
                                    break;
                                }
                            }

                            // If not, then queue the stucker
                            if (!alreadyQueued)
                            {
                                alFolderRequestQueue.Add(new DownloadRequest_Folder(IncomingFolderID, dr.Recurse, dr.FetchFolders, dr.FetchItems));
                            }
                        }
                    }
                }
            }
            #endregion

            // Update total number of descendants expected , and update the total downloaded
            dr.Expected = iDescendentsExpected;
            dr.Received += iDescendentsReceivedThisBlock;
            dr.LastReceivedAtTick = Environment.TickCount;

            // Check if we're finished
            if (dr.Received >= dr.Expected)
            {
                // Looks like after updating, we have all the descendents, 
                // remove from folder status.
                FolderDownloadStatus.Remove(uuidFolderID);
                dr.RequestComplete.Set();
                if (RequestDownloadFinishedEvent != null)
                {
                    DownloadRequest_EventArgs e = new DownloadRequest_EventArgs();
                    e.DownloadRequest = dr;
                    FireRequestDownloadFinishedEvent(InvFolderUpdating, e);
                }
            }
        }
        #endregion
    }
}
