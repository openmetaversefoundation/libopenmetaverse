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
        private static AssetManager slAssetManager;
        internal AssetManager AssetManager
        {
            get { return slAssetManager; }
        }

        public InventoryPacketHelper InvPacketHelper = null;

        // Setup a dictionary to easily lookup folders by UUID
        private Dictionary<LLUUID, InventoryFolder> htFoldersByUUID = new Dictionary<LLUUID, InventoryFolder>();

        // Setup a dictionary to track download progress
        private Dictionary<LLUUID, DownloadRequest_Folder> FolderDownloadStatus = new Dictionary<LLUUID, DownloadRequest_Folder>();
        private List<DownloadRequest_Folder> alFolderRequestQueue = new List<DownloadRequest_Folder>();

        // Used to track current item being created
        private InventoryItem iiCreationInProgress;
        public ManualResetEvent ItemCreationCompleted;

        private int LastPacketRecievedAtTick;


        /// <summary>
        /// Download event singalling that folder contents have been downloaded.
        /// </summary>
        /// <param name="InventoryFolder">The Inventory Folder that was updated</param>
        /// <param name="e"></param>
        public delegate void On_RequestDownloadContents_Finished(object InventoryFolder, EventArgs e);
        public event On_RequestDownloadContents_Finished RequestDownloadFinishedEvent;

        // Each InventorySystem needs to be initialized with a client and root folder.
        public InventoryManager(SecondLife client)
        {
            slClient = client;
            slAssetManager = slClient.Assets;

            InvPacketHelper = new InventoryPacketHelper(slClient);

            // Need to know what when we're connected/disconnected
            slClient.Network.OnConnected += new NetworkManager.ConnectedCallback(Network_OnConnected);
            slClient.Network.OnDisconnected += new NetworkManager.DisconnectCallback(Network_OnDisconnected);

            // Setup the callback for Inventory Downloads
            slClient.Network.RegisterCallback(PacketType.InventoryDescendents, new NetworkManager.PacketCallback(InventoryDescendentsHandler));

            // Setup the callback for Inventory Creation Update
            slClient.Network.RegisterCallback(PacketType.UpdateCreateInventoryItem, new NetworkManager.PacketCallback(UpdateCreateInventoryItemHandler));

        }

        void Network_OnDisconnected(NetworkManager.DisconnectType reason, string message)
        {
            // Clear out current state
            ClearState();
        }

        void Network_OnConnected(object sender)
        {
            // Clear out current state
            ClearState();
        }

        private void ClearState()
        {
            htFoldersByUUID.Clear();
            FolderDownloadStatus.Clear();
            alFolderRequestQueue.Clear();

            if (slClient.Self.InventoryRootFolderUUID != null)
            {
                // Init folder structure with root
                InventoryFolder ifRootFolder = new InventoryFolder(this, "My Inventory", slClient.Self.InventoryRootFolderUUID, null);
                htFoldersByUUID[slClient.Self.InventoryRootFolderUUID] = ifRootFolder;
            }
        }

        public InventoryFolder GetRootFolder()
        {
            return htFoldersByUUID[slClient.Self.InventoryRootFolderUUID];
        }

        public InventoryFolder getFolder(LLUUID folderID)
        {
            return htFoldersByUUID[folderID];
        }

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
        private InventoryFolder getFolder(Queue<string> qFolderPath)
        {
            return getFolder(qFolderPath, GetRootFolder());
        }

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

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr"></param>
        internal void RequestFolder(DownloadRequest_Folder dr)
        {
            Packet packet = InvPacketHelper.FetchInventoryDescendents(
                            dr.FolderID
                            , dr.FetchFolders
                            , dr.FetchItems);

            FolderDownloadStatus[dr.FolderID] = dr;

            slClient.Network.SendPacket(packet);
        }

        internal InventoryFolder FolderCreate(String name, LLUUID parentid)
        {
            InventoryFolder ifolder = new InventoryFolder(this, name, LLUUID.Random(), parentid);
            ifolder._Type = -1;

            if (htFoldersByUUID.ContainsKey(ifolder.ParentID))
            {
                if (((InventoryFolder)htFoldersByUUID[ifolder.ParentID])._Contents.Contains(ifolder) == false)
                {
                    // Add new folder to the contents of the parent folder.
                    ((InventoryFolder)htFoldersByUUID[ifolder.ParentID])._Contents.Add(ifolder);
                }
            }
            else
            {
                throw new Exception("Parent Folder " + ifolder.ParentID + " does not exist in this Inventory Manager.");
            }

            if (htFoldersByUUID.ContainsKey(ifolder.FolderID) == false)
            {
                htFoldersByUUID[ifolder.FolderID] = ifolder;
            }

            Packet packet = InvPacketHelper.CreateInventoryFolder(ifolder.Name, ifolder.ParentID, ifolder.Type, ifolder.FolderID);
            slClient.Network.SendPacket(packet);

            return ifolder;
        }

        internal void FolderRemove(InventoryFolder ifolder)
        {
            FolderRemove(ifolder.FolderID);
        }

        internal void FolderRemove(LLUUID folderID)
        {
            htFoldersByUUID.Remove(folderID);
            Packet packet = InvPacketHelper.RemoveInventoryFolder(folderID);
            slClient.Network.SendPacket(packet);
        }

        internal void FolderMove(InventoryFolder iFolder, LLUUID newParentID)
        {
            Packet packet = InvPacketHelper.MoveInventoryFolder(newParentID, iFolder.FolderID);
            slClient.Network.SendPacket(packet);
        }

        internal void FolderRename(InventoryFolder ifolder)
        {
            Packet packet = InvPacketHelper.UpdateInventoryFolder(ifolder.Name, ifolder.ParentID, ifolder.Type, ifolder.FolderID);
            slClient.Network.SendPacket(packet);
        }

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

        internal void ItemUpdate(InventoryItem iitem)
        {
            Packet packet = InvPacketHelper.UpdateInventoryItem(iitem);
            slClient.Network.SendPacket(packet);

            #if DEBUG_PACKETS
                slClient.DebugLog(packet); 
            #endif         
        }

        internal void ItemCopy(LLUUID ItemID, LLUUID TargetFolderID)
        {
            Packet packet = InvPacketHelper.CopyInventoryItem(ItemID, TargetFolderID);
            slClient.Network.SendPacket(packet);

            #if DEBUG_PACKETS
                slClient.DebugLog(packet); 
            #endif         
        }

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
            RequestFolder(new DownloadRequest_Folder(slClient.Self.InventoryRootFolderUUID));

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

        #region libsecondlife callback handlers

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
        /// Returned in response to a InventoryDescendantRequest.  Contains information about the
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
            InventoryFolder InvFolderUpdating = (InventoryFolder)htFoldersByUUID[uuidFolderID];

            // Update Inventory Manager's last tick point, used for timeouts and such
            LastPacketRecievedAtTick = Environment.TickCount;

            // Some temp variables to be reused as we're parsing the packet
            InventoryItem TempInvItem;
            InventoryFolder TempInvFolder;

            // Used to count the number of descendants received to see if we're finished or not.
            int iDescendentsExpected = reply.AgentData.Descendents;
            int iDescendentsReceivedThisBlock = 0;



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
                        TempInvItem = new InventoryItem(this, itemBlock);

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


            foreach (InventoryDescendentsPacket.FolderDataBlock folderBlock in reply.FolderData)
            {
                String name = System.Text.Encoding.UTF8.GetString(folderBlock.Name).Trim().Replace("\0", "");
                LLUUID folderid = folderBlock.FolderID;
                LLUUID parentid = folderBlock.ParentID;
// unused?      sbyte type = folderBlock.Type;

                // There is always an folder block, even if there isn't any folders
                // the "filler" block will not have a name
                if (folderBlock.Name.Length != 0)
                {
                    TempInvFolder = new InventoryFolder(this, name, folderid, parentid);

                    iDescendentsReceivedThisBlock++;

                    // Add folder to Parent
                    if (InvFolderUpdating._Contents.Contains(TempInvFolder) == false)
                    {
                        InvFolderUpdating._Contents.Add(TempInvFolder);
                    }


                    // Add folder to UUID Lookup
                    htFoldersByUUID[TempInvFolder.FolderID] = TempInvFolder;


                    // Do we recurse?
                    if (dr.Recurse)
                    {
                        // It's not the root, should be safe to "recurse"
                        if (!TempInvFolder.FolderID.Equals(slClient.Self.InventoryRootFolderUUID))
                        {
                            bool alreadyQueued = false;
                            foreach (DownloadRequest_Folder adr in alFolderRequestQueue)
                            {
                                if (adr.FolderID == TempInvFolder.FolderID)
                                {
                                    alreadyQueued = true;
                                    break;
                                }
                            }

                            if (!alreadyQueued)
                            {
                                alFolderRequestQueue.Add(new DownloadRequest_Folder(TempInvFolder.FolderID));
                            }
                        }
                    }
                }
            }



            // Update download status for this folder
            if (iDescendentsReceivedThisBlock >= iDescendentsExpected)
            {
                // We received all the descendents we're expecting for this folder
                // in this packet, so go ahead and remove folder from status list.
                FolderDownloadStatus.Remove(uuidFolderID);
                dr.RequestComplete.Set();
            }
            else
            {
                // This one packet didn't have all the descendents we're expecting
                // so update the total we're expecting, and update the total downloaded
                dr.Expected = iDescendentsExpected;
                dr.Received += iDescendentsReceivedThisBlock;
                dr.LastReceivedAtTick = Environment.TickCount;

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
                        RequestDownloadFinishedEvent(InvFolderUpdating, e);
                    }
                }
                else
                {
                    FolderDownloadStatus[uuidFolderID] = dr;
                }
            }
        }
        #endregion
    }
}
