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

        // Reference to the Asset Manager
        private static AssetManager slAssetManager;
        internal AssetManager AssetManager
        {
            get { return slAssetManager; }
        }

        public InventoryPacketHelper InvPacketHelper = null;

        // UUID of Root Inventory Folder
        private LLUUID uuidRootFolder;

        // Setup a dictionary to easily lookup folders by UUID
        private Dictionary<LLUUID, InventoryFolder> htFoldersByUUID = new Dictionary<LLUUID, InventoryFolder>();

        // Setup a dictionary to track download progress
        private Dictionary<LLUUID, DescendentRequest> htFolderDownloadStatus;
        private List<DescendentRequest> alFolderRequestQueue;

        // Used to track current item being created
        private InventoryItem iiCreationInProgress;
        public ManualResetEvent ItemCreationCompleted;

        private uint LastPacketRecieved;

        // Each InventorySystem needs to be initialized with a client and root folder.
        public InventoryManager(SecondLife client, LLUUID rootFolder)
        {
            slClient = client;
            if (slAssetManager == null)
            {
                slAssetManager = new AssetManager(slClient);
            }

            InvPacketHelper = new InventoryPacketHelper(slClient.Network.AgentID, slClient.Network.SessionID);

            uuidRootFolder = rootFolder;

            resetFoldersByUUID();

            // Setup the callback for Inventory Downloads
            PacketCallback InventoryDescendentsCallback = new PacketCallback(InventoryDescendentsHandler);
            slClient.Network.RegisterCallback(PacketType.InventoryDescendents, InventoryDescendentsCallback);

            // Setup the callback for Inventory Creation Update
            PacketCallback UpdateCreateInventoryItemCallback = new PacketCallback(UpdateCreateInventoryItemHandler);
            slClient.Network.RegisterCallback(PacketType.UpdateCreateInventoryItem, UpdateCreateInventoryItemCallback);

        }

        // Used primarily for debugging and testing
        public AssetManager getAssetManager()
        {
            Console.WriteLine("It is not recommended that you access the asset manager directly");
            return AssetManager;
        }

        private void resetFoldersByUUID()
        {
            // Init folder structure with root
            htFoldersByUUID = new Dictionary<LLUUID,InventoryFolder>();

            InventoryFolder ifRootFolder = new InventoryFolder(this, "My Inventory", uuidRootFolder, null);
            htFoldersByUUID[uuidRootFolder] = ifRootFolder;
        }

        public InventoryFolder getRootFolder()
        {
            return htFoldersByUUID[uuidRootFolder];
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
                return getRootFolder();
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
            return getFolder(qFolderPath, getRootFolder());
        }

        private InventoryFolder getFolder(Queue<string> qFolderPath, InventoryFolder ifRoot)
        {
            string sCurFolder = qFolderPath.Dequeue();

            foreach (InventoryBase ibFolder in ifRoot.alContents)
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

        private void RequestFolder(DescendentRequest dr)
        {
            Packet packet = InvPacketHelper.FetchInventoryDescendents(
                            dr.FolderID
                            , dr.FetchFolders
                            , dr.FetchItems);

            htFolderDownloadStatus[dr.FolderID] = dr;

            slClient.Network.SendPacket(packet);
        }

        internal InventoryFolder FolderCreate(String name, LLUUID parentid)
        {
            InventoryFolder ifolder = new InventoryFolder(this, name, LLUUID.GenerateUUID(), parentid);
            ifolder._Type = -1;

            if (htFoldersByUUID.ContainsKey(ifolder.ParentID))
            {
                if (((InventoryFolder)htFoldersByUUID[ifolder.ParentID]).alContents.Contains(ifolder) == false)
                {
                    // Add new folder to the contents of the parent folder.
                    ((InventoryFolder)htFoldersByUUID[ifolder.ParentID]).alContents.Add(ifolder);
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

            ItemCreationCompleted = new ManualResetEvent(false);
            iiCreationInProgress = iitem;
            

            Packet packet = InvPacketHelper.CreateInventoryItem(iitem);
            slClient.Network.SendPacket(packet);

            #if DEBUG_PACKETS
                Console.WriteLine(packet);
            #endif

            ItemCreationCompleted.WaitOne();

            iiCreationInProgress = null;
        }

        internal void ItemUpdate(InventoryItem iitem)
        {
            Packet packet = InvPacketHelper.UpdateInventoryItem(iitem);
            slClient.Network.SendPacket(packet);

            #if DEBUG_PACKETS
                Console.WriteLine(packet); 
            #endif         
        }

        internal void ItemCopy(LLUUID ItemID, LLUUID TargetFolderID)
        {
            Packet packet = InvPacketHelper.CopyInventoryItem(ItemID, TargetFolderID);
            slClient.Network.SendPacket(packet);

            #if DEBUG_PACKETS
                Console.WriteLine(packet); 
            #endif         
        }

        internal void ItemGiveTo(InventoryItem iitem, LLUUID ToAgentID)
        {
            LLUUID MessageID = LLUUID.GenerateUUID();

            Packet packet = InvPacketHelper.ImprovedInstantMessage(
                MessageID
                , ToAgentID
                , slClient.Self.FirstName + " " + slClient.Self.LastName
                , slClient.Self.Position
                , iitem
                );

            slClient.Network.SendPacket(packet);

            #if DEBUG_PACKETS
                Console.WriteLine(packet); 
            #endif         
        }

        internal void ItemRemove(InventoryItem iitem)
        {
            InventoryFolder ifolder = getFolder(iitem.FolderID);
            ifolder.alContents.Remove(iitem);

            Packet packet = InvPacketHelper.RemoveInventoryItem(iitem.ItemID);
            slClient.Network.SendPacket(packet);

            #if DEBUG_PACKETS
                Console.WriteLine(packet); 
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
            resetFoldersByUUID();

            if (htFolderDownloadStatus == null)
            {
                // Create status table
                htFolderDownloadStatus = new Dictionary<LLUUID,DescendentRequest>();
            }
            else
            {
                if (htFolderDownloadStatus.Count != 0)
                {
                    throw new Exception("Inventory Download requested while previous download in progress.");
                }
            }

            if (alFolderRequestQueue == null)
            {
                alFolderRequestQueue = new List<DescendentRequest>();
            }

            // Set last packet received to now, just so out time-out timer works
            LastPacketRecieved = Helpers.GetUnixTime();

            // Send Packet requesting the root Folder, 
            // this should recurse through all folders
            RequestFolder(new DescendentRequest(uuidRootFolder));

            while ((htFolderDownloadStatus.Count > 0) || (alFolderRequestQueue.Count > 0))
            {
                if (htFolderDownloadStatus.Count == 0)
                {
                    DescendentRequest dr = alFolderRequestQueue[0];
                    alFolderRequestQueue.RemoveAt(0);
                    RequestFolder(dr);
                }

                if ((Helpers.GetUnixTime() - LastPacketRecieved) > 10)
                {
                    Console.WriteLine("Time-out while waiting for packets (" + (Helpers.GetUnixTime() - LastPacketRecieved) + " seconds since last packet)");
                    Console.WriteLine("Current Status:");

                    // have to make a seperate list otherwise we run into modifying the original array
                    // while still enumerating it.
                    List<DescendentRequest> alRestartList = new List<DescendentRequest>();

                    //if (htFolderDownloadStatus[0] != null)
                    //{
                    //    Console.WriteLine(htFolderDownloadStatus[0].GetType());
                    //}

                    foreach (DescendentRequest dr in htFolderDownloadStatus.Values)
                    {
                        Console.WriteLine(dr.FolderID + " " + dr.Expected + " / " + dr.Received + " / " + dr.LastReceived);

                        alRestartList.Add(dr);
                    }

                    LastPacketRecieved = Helpers.GetUnixTime();
                    foreach (DescendentRequest dr in alRestartList)
                    {
                        RequestFolder(dr);
                    }

                }
                slClient.Tick();

            }
        }



        public void UpdateCreateInventoryItemHandler(Packet packet, Simulator simulator)
        {
            #if DEBUG_PACKETS
                Console.WriteLine(packet);
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
                Console.WriteLine(packet);
                throw new Exception("Received a packet for item creation, but no such response was expected.  This is probably a bad thing...");
            }
        }


        public void InventoryDescendentsHandler(Packet packet, Simulator simulator)
        {
            InventoryDescendentsPacket reply = (InventoryDescendentsPacket)packet;

            LastPacketRecieved = Helpers.GetUnixTime();

            InventoryItem invItem;
            InventoryFolder invFolder;

            LLUUID uuidFolderID = new LLUUID();

            int iDescendentsExpected = int.MaxValue;
            int iDescendentsReceivedThisBlock = 0;

            foreach (InventoryDescendentsPacket.ItemDataBlock itemBlock in reply.ItemData)
            {
                // There is always an item block, even if there isn't any items
                // the "filler" block will not have a name
                if (itemBlock.Name.Length != 0)
                {
                    iDescendentsReceivedThisBlock++;

                    invItem = new InventoryItem(this, itemBlock);

                    InventoryFolder ifolder = (InventoryFolder)htFoldersByUUID[invItem.FolderID];

                    if (ifolder.alContents.Contains(invItem) == false)
                    {
                        if ((invItem.InvType == 7) && (invItem.Type == Asset.ASSET_TYPE_NOTECARD))
                        {
                            InventoryItem temp = new InventoryNotecard(this, invItem);
                            invItem = temp;
                        }

                        if ((invItem.InvType == 0) && (invItem.Type == Asset.ASSET_TYPE_IMAGE))
                        {
                            InventoryItem temp = new InventoryImage(this, invItem);
                            invItem = temp;
                        }

                        ifolder.alContents.Add(invItem);
                    }

                }
            }


            foreach (InventoryDescendentsPacket.FolderDataBlock folderBlock in reply.FolderData)
            {
                String name = System.Text.Encoding.UTF8.GetString(folderBlock.Name).Trim().Replace("\0", "");
                LLUUID folderid = folderBlock.FolderID;
                LLUUID parentid = folderBlock.ParentID;
                sbyte type = folderBlock.Type;

                // There is always an folder block, even if there isn't any folders
                // the "filler" block will not have a name
                if (folderBlock.Name.Length != 0)
                {
                    invFolder = new InventoryFolder(this, name, folderid, parentid);

                    iDescendentsReceivedThisBlock++;

                    // Add folder to Parent
                    InventoryFolder ifolder = (InventoryFolder)htFoldersByUUID[invFolder.ParentID];
                    if (ifolder.alContents.Contains(invFolder) == false)
                    {
                        ifolder.alContents.Add(invFolder);
                    }


                    // Add folder to UUID Lookup
                    htFoldersByUUID[invFolder.FolderID] = invFolder;


                    // It's not the root, should be safe to "recurse"
                    if (!invFolder.FolderID.Equals(uuidRootFolder))
                    {
                        bool alreadyQueued = false;
                        foreach (DescendentRequest dr in alFolderRequestQueue)
                        {
                            if (dr.FolderID == invFolder.FolderID)
                            {
                                alreadyQueued = true;
                                break;
                            }
                        }

                        if (!alreadyQueued)
                        {
                            alFolderRequestQueue.Add(new DescendentRequest(invFolder.FolderID));
                        }
                    }
                }
            }


            // Check how many descendents we're actually supposed to receive
            iDescendentsExpected = reply.AgentData.Descendents;
            uuidFolderID = reply.AgentData.FolderID;

            // Update download status for this folder
            if (iDescendentsReceivedThisBlock >= iDescendentsExpected)
            {
                // We received all the descendents we're expecting for this folder
                // in this packet, so go ahead and remove folder from status list.
                htFolderDownloadStatus.Remove(uuidFolderID);
            }
            else
            {

                // This one packet didn't have all the descendents we're expecting
                // so update the total we're expecting, and update the total downloaded

                DescendentRequest dr = (DescendentRequest)htFolderDownloadStatus[uuidFolderID];
                dr.Expected = iDescendentsExpected;
                dr.Received += iDescendentsReceivedThisBlock;
                dr.LastReceived = Helpers.GetUnixTime();

                if (dr.Received >= dr.Expected)
                {
                    // Looks like after updating, we have all the descendents, 
                    // remove from folder status.
                    htFolderDownloadStatus.Remove(uuidFolderID);
                }
                else
                {
                    htFolderDownloadStatus[uuidFolderID] = dr;
                    //					Console.WriteLine( uuidFolderID + " is expecting " + (iDescendentsExpected - iStatus[1]) + " more packets." );
                }
            }
        }

        private class DescendentRequest
        {
            public LLUUID FolderID;

            public int Expected = int.MaxValue;
            public int Received = 0;
            public uint LastReceived = 0;

            public bool FetchFolders = true;
            public bool FetchItems = true;

            public DescendentRequest(LLUUID folderID)
            {
                FolderID = folderID;
                LastReceived = Helpers.GetUnixTime();
            }

            public DescendentRequest(LLUUID folderID, bool fetchFolders, bool fetchItems)
            {
                FolderID = folderID;
                FetchFolders = fetchFolders;
                FetchItems = fetchItems;
                LastReceived = Helpers.GetUnixTime();
            }

        }
    }
}
