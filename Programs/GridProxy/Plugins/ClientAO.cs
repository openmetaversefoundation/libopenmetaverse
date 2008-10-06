/*
 * ClientAO.cs: GridProxy application that acts as a client side animation overrider.
 * The application will start and stop animations corresponding to the movements
 * of the avatar on screen.
 *
 * Copyright (c) 2007 Gilbert Roulot
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
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Reflection;
using OpenMetaverse;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;
using Nwc.XmlRpc;
using GridProxy;


public class ClientAO : ProxyPlugin
{
    private ProxyFrame frame;
    private Proxy proxy;
    private UUID[] wetikonanims = {
            Animations.WALK,
            Animations.RUN,
            Animations.CROUCHWALK,
            Animations.FLY,
            Animations.TURNLEFT,
            Animations.TURNRIGHT,
            Animations.JUMP,
            Animations.HOVER_UP,
            Animations.CROUCH,
            Animations.HOVER_DOWN,
            Animations.STAND,
            Animations.STAND_1,
            Animations.STAND_2,
            Animations.STAND_3,
            Animations.STAND_4,
            Animations.HOVER,
            Animations.SIT,
            Animations.PRE_JUMP,
            Animations.FALLDOWN,
            Animations.LAND,
            Animations.STANDUP,
            Animations.FLYSLOW,
            Animations.SIT_GROUND_staticRAINED,
            UUID.Zero, //swimming doesnt exist
            UUID.Zero,
            UUID.Zero,
            UUID.Zero
        };

    private string[] wetikonanimnames = {
            "walk",
            "run",
            "crouch walk",
            "fly",
            "turn left",
            "turn right",
            "jump",
            "hover up",
            "crouch",
            "hover down",
            "stand",
            "stand 2",
            "stand 3",
            "stand 4",
            "stand 5",
            "hover",
            "sit",
            "pre jump",
            "fall down",
            "land",
            "stand up",
            "fly slow",
            "sit on ground",
            "swim (ignored)", //swimming doesnt exist
            "swim (ignored)",
            "swim (ignored)",
            "swim (ignored)"
        };

    private Dictionary<UUID, string> animuid2name;
    
    //private Assembly libslAssembly;

    #region Packet delegates members
    private PacketDelegate _packetDelegate;
    private PacketDelegate packetDelegate 
    {
        get
        {
            if (_packetDelegate == null) 
            {
                _packetDelegate = new PacketDelegate(AnimationPacketHandler);
            }
            return _packetDelegate;
        }
    }

    private PacketDelegate _inventoryPacketDelegate;
    private PacketDelegate inventoryPacketDelegate
    {
        get
        {
            if (_inventoryPacketDelegate == null)
            {
                _inventoryPacketDelegate = new PacketDelegate(InventoryDescendentsHandler);
            }
            return _inventoryPacketDelegate;
        }
    }

    private PacketDelegate _transferPacketDelegate;
    private PacketDelegate transferPacketDelegate
    {
        get
        {
            if (_transferPacketDelegate == null)
            {
                _transferPacketDelegate = new PacketDelegate(TransferPacketHandler);
            }
            return _transferPacketDelegate;
        }
    }

    private PacketDelegate _transferInfoDelegate;
    private PacketDelegate transferInfoDelegate
    {
        get
        {
            if (_transferInfoDelegate == null)
            {
                _transferInfoDelegate = new PacketDelegate(TransferInfoHandler);
            }
            return _transferInfoDelegate;
        }
    }
    #endregion


    //map of built in SL animations and their overrides
    private Dictionary<UUID,UUID> overrides = new Dictionary<UUID,UUID>();

    //list of animations currently running
    private Dictionary<UUID, int> SignaledAnimations = new Dictionary<UUID, int>();

    //playing status of animations'override animation
    private Dictionary<UUID, bool> overrideanimationisplaying;

    //Current inventory path search
    string[] searchPath;
    //Search level
    int searchLevel;
    //Current folder
    UUID currentFolder;
    // Number of directory descendents received
    int nbdescendantsreceived;
    //List of items in the current folder
    Dictionary<string,InventoryItem> currentFolderItems;
    //Asset download request ID
    UUID assetdownloadID;
    //Downloaded bytes so far
    int downloadedbytes;
    //size of download
    int downloadsize;
    //data buffer
    byte[] buffer;


    public ClientAO(ProxyFrame frame)
    {        
        this.frame = frame;
        this.proxy = frame.proxy;
    }

    //Initialise the plugin
    public override void Init()
    {
        //libslAssembly = Assembly.Load("libsecondlife");
        //if (libslAssembly == null) throw new Exception("Assembly load exception");

        // build the table of /command delegates
        InitializeCommandDelegates();                         

        SayToUser("ClientAO loaded");
    }

    // InitializeCommandDelegates: configure ClientAO's commands
    private void InitializeCommandDelegates()
    {
        //The ClientAO responds to command beginning with /ao
        frame.AddCommand("/ao", new ProxyFrame.CommandDelegate(CmdAO));
    }

    //Process commands from the user
    private void CmdAO(string[] words) {
        if (words.Length < 2) 
        {
            SayToUser("Usage: /ao on/off/notecard path");
        }
        else if (words[1] == "on")
        {
            //Turn AO on
            AOOn();
            SayToUser("AO started");
        }
        else if (words[1] == "off")
        {
            //Turn AO off
            AOOff();
            SayToUser("AO stopped");
        }
        else
        {
            //Load notecard from path
            //exemple: /ao Objects/My AOs/wetikon/config.txt
            string[] tmp = new string[words.Length - 1];
            //join the arguments together with spaces, to
            //take care of folder and item names with spaces in them
            for (int i = 1; i < words.Length; i++)
            {
                tmp[i - 1] = words[i];
            }            
            // add a delegate to monitor inventory infos
            proxy.AddDelegate(PacketType.InventoryDescendents, Direction.Incoming, this.inventoryPacketDelegate);            
            RequestFindObjectByPath(frame.InventoryRoot, String.Join(" ", tmp));
        }
    }

    private void AOOn()
    {
        // add a delegate to track agent movements
        proxy.AddDelegate(PacketType.AvatarAnimation, Direction.Incoming, this.packetDelegate);
    }

    private void AOOff()
    {
        // remove the delegate to track agent movements
        proxy.RemoveDelegate(PacketType.AvatarAnimation, Direction.Incoming, this.packetDelegate);
        //Stop all override animations
        foreach (UUID tmp in overrides.Values)
        {
            Animate(tmp, false);
        }
    }

    // Inventory functions

    //start requesting an item by its path
    public void RequestFindObjectByPath(UUID baseFolder, string path)
    {
        if (path == null || path.Length == 0)
            throw new ArgumentException("Empty path is not supported");
        currentFolder = baseFolder;
        //split path by '/'
        searchPath = path.Split('/');
        //search for first element in the path
        searchLevel = 0;

        // Start the search
        RequestFolderContents(baseFolder,
            true, 
            (searchPath.Length == 1) ? true : false, 
            InventorySortOrder.ByName);
    }

    //request a folder content
    public void RequestFolderContents(UUID folder, bool folders, bool items,
        InventorySortOrder order)
    {
        //empty the dictionnary containing current folder items by name
        currentFolderItems = new Dictionary<string, InventoryItem>();
        //reset the number of descendants received
        nbdescendantsreceived = 0;
        //build a packet to request the content
        FetchInventoryDescendentsPacket fetch = new FetchInventoryDescendentsPacket();
        fetch.AgentData.AgentID = frame.AgentID;
        fetch.AgentData.SessionID = frame.SessionID;

        fetch.InventoryData.FetchFolders = folders;
        fetch.InventoryData.FetchItems = items;
        fetch.InventoryData.FolderID = folder;
        fetch.InventoryData.OwnerID = frame.AgentID; //is it correct?
        fetch.InventoryData.SortOrder = (int)order;

        //send packet to SL
        proxy.InjectPacket(fetch, Direction.Outgoing);
    }

    //process the reply from SL
    private Packet InventoryDescendentsHandler(Packet packet, IPEndPoint sim)
    {
        bool intercept = false;
        InventoryDescendentsPacket reply = (InventoryDescendentsPacket)packet;

        if (reply.AgentData.Descendents > 0 
            && reply.AgentData.FolderID == currentFolder)
        {            
            //SayToUser("nb descendents: " + reply.AgentData.Descendents);
            //this packet concerns the folder we asked for            
            if (reply.FolderData[0].FolderID != UUID.Zero 
                && searchLevel < searchPath.Length - 1)
            {
                nbdescendantsreceived += reply.FolderData.Length;
                //SayToUser("nb received: " + nbdescendantsreceived);
                //folders are present, and we are not at end of path.
                //look at them
                for (int i = 0; i < reply.FolderData.Length; i++)
                {
                    //SayToUser("Folder: " + Utils.BytesToString(reply.FolderData[i].Name));
                    if (searchPath[searchLevel] == Utils.BytesToString(reply.FolderData[i].Name)) {
                        //We found the next folder in the path                        
                        currentFolder = reply.FolderData[i].FolderID;                       
                        if (searchLevel < searchPath.Length - 1)                        
                        {
                            // ask for next item in path
                            searchLevel++;
                            RequestFolderContents(currentFolder,
                                true,
                                (searchLevel < searchPath.Length - 1) ? false : true, 
                                InventorySortOrder.ByName);
                            //Jump to end
                            goto End;
                        }                        
                    }
                }
                if (nbdescendantsreceived >= reply.AgentData.Descendents)
                {
                    //We have not found the folder. The user probably mistyped it
                    SayToUser("Didn't find folder " + searchPath[searchLevel]);
                    //Stop looking at packets
                    proxy.RemoveDelegate(PacketType.InventoryDescendents, Direction.Incoming, this.inventoryPacketDelegate);
                }
            }
            else if (searchLevel < searchPath.Length - 1)
            {
                //There are no folders in the packet ; but we are looking for one!
                //We have not found the folder. The user probably mistyped it
                SayToUser("Didn't find folder " + searchPath[searchLevel]);
                //Stop looking at packets
                proxy.RemoveDelegate(PacketType.InventoryDescendents, Direction.Incoming, this.inventoryPacketDelegate);
            }
            else
            {
                //There are folders in the packet. And we are at the end of 
                //the path, count their number in nbdescendantsreceived
                nbdescendantsreceived += reply.FolderData.Length;
                //SayToUser("nb received: " + nbdescendantsreceived);                
            }
            if (reply.ItemData[0].ItemID != UUID.Zero
                && searchLevel == searchPath.Length - 1)
            {
                //there are items returned and we are looking for one 
                //(end of search path)                
                //count them
                nbdescendantsreceived += reply.ItemData.Length;
                //SayToUser("nb received: " + nbdescendantsreceived);
                for (int i = 0; i < reply.ItemData.Length; i++)
                {
                    //we are going to store info on all items. we'll need
                    //it to get the asset ID of animations refered to by the
                    //configuration notecard
                    if (reply.ItemData[i].ItemID != UUID.Zero)
                    {
                        InventoryItem item = CreateInventoryItem((InventoryType)reply.ItemData[i].InvType, reply.ItemData[i].ItemID);
                        item.ParentUUID = reply.ItemData[i].FolderID;
                        item.CreatorID = reply.ItemData[i].CreatorID;
                        item.AssetType = (AssetType)reply.ItemData[i].Type;
                        item.AssetUUID = reply.ItemData[i].AssetID;
                        item.CreationDate = Utils.UnixTimeToDateTime((uint)reply.ItemData[i].CreationDate);
                        item.Description = Utils.BytesToString(reply.ItemData[i].Description);
                        item.Flags = (uint)reply.ItemData[i].Flags;
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

                        //SayToUser("item in folder: " + item.Name);

                        //Add the item to the name -> item hash
                        currentFolderItems.Add(item.Name, item);                        
                    }
                }
                if (nbdescendantsreceived >= reply.AgentData.Descendents)
                {
                    //We have received all the items in the last folder
                    //Let's look for the item we are looking for
                    if (currentFolderItems.ContainsKey(searchPath[searchLevel]))
                    {
                        //We found what we where looking for
                        //Stop looking at packets
                        proxy.RemoveDelegate(PacketType.InventoryDescendents, Direction.Incoming, this.inventoryPacketDelegate);
                        //Download the notecard
                        assetdownloadID = RequestInventoryAsset(currentFolderItems[searchPath[searchLevel]]);
                    }
                    else
                    {
                        //We didnt find the item, the user probably mistyped its name
                        SayToUser("Didn't find notecard " + searchPath[searchLevel]);
                        //TODO: keep looking for a moment, or else reply packets may still
                        //come in case of a very large inventory folder
                        //Stop looking at packets
                        proxy.RemoveDelegate(PacketType.InventoryDescendents, Direction.Incoming, this.inventoryPacketDelegate);
                    }
                }
            }
            else if (searchLevel == searchPath.Length - 1 && nbdescendantsreceived >= reply.AgentData.Descendents)
            {
                //There are no items in the packet, but we are looking for one!
                //We didnt find the item, the user probably mistyped its name
                SayToUser("Didn't find notecard " + searchPath[searchLevel]);
                //TODO: keep looking for a moment, or else reply packets may still
                //come in case of a very large inventory folder
                //Stop looking at packets
                proxy.RemoveDelegate(PacketType.InventoryDescendents, Direction.Incoming, this.inventoryPacketDelegate);
            }
            //Intercept the packet, it was a reply to our request. No need
            //to confuse the actual SL client
            intercept = true;
        }
        End:
        if (intercept)
        {
            //stop packet
            return null;
        }
        else
        {
            //let packet go to client
            return packet;
        }
    }

    public static InventoryItem CreateInventoryItem(InventoryType type, UUID id)
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

    //Ask for download of an item
    public UUID RequestInventoryAsset(InventoryItem item)
    {
        // Build the request packet and send it
        TransferRequestPacket request = new TransferRequestPacket();
        request.TransferInfo.ChannelType = (int)ChannelType.Asset;
        request.TransferInfo.Priority = 101.0f;
        request.TransferInfo.SourceType = (int)SourceType.SimInventoryItem;
        UUID transferID = UUID.Random();
        request.TransferInfo.TransferID = transferID;

        byte[] paramField = new byte[100];
        Buffer.BlockCopy(frame.AgentID.GetBytes(), 0, paramField, 0, 16);
        Buffer.BlockCopy(frame.SessionID.GetBytes(), 0, paramField, 16, 16);
        Buffer.BlockCopy(item.OwnerID.GetBytes(), 0, paramField, 32, 16);
        Buffer.BlockCopy(UUID.Zero.GetBytes(), 0, paramField, 48, 16);
        Buffer.BlockCopy(item.UUID.GetBytes(), 0, paramField, 64, 16);
        Buffer.BlockCopy(item.AssetUUID.GetBytes(), 0, paramField, 80, 16);
        Buffer.BlockCopy(Utils.IntToBytes((int)item.AssetType), 0, paramField, 96, 4);
        request.TransferInfo.Params = paramField;

        // add a delegate to monitor configuration notecards download
        proxy.AddDelegate(PacketType.TransferPacket, Direction.Incoming, this.transferPacketDelegate);

        //send packet to SL
        proxy.InjectPacket(request, Direction.Outgoing);

        //so far we downloaded 0 bytes
        downloadedbytes = 0;
        //the total size of the download is yet unknown
        downloadsize = 0;
        //A 100K buffer should be enough for everyone
        buffer =  new byte[1024 * 100];
        //Return the transfer ID
        return transferID;
    }

    // SayToUser: send a message to the user as in-world chat
    private void SayToUser(string message)
    {
        ChatFromSimulatorPacket packet = new ChatFromSimulatorPacket();
        packet.ChatData.FromName = Utils.StringToBytes("ClientAO");
        packet.ChatData.SourceID = UUID.Random();
        packet.ChatData.OwnerID = frame.AgentID;
        packet.ChatData.SourceType = (byte)2;
        packet.ChatData.ChatType = (byte)1;
        packet.ChatData.Audible = (byte)1;
        packet.ChatData.Position = new Vector3(0, 0, 0);
        packet.ChatData.Message = Utils.StringToBytes(message);
        proxy.InjectPacket(packet, Direction.Incoming);
    }

    //start or stop an animation
    public void Animate(UUID animationuuid, bool run)
    {
        AgentAnimationPacket animate = new AgentAnimationPacket();
        animate.Header.Reliable = true;
        animate.AgentData.AgentID = frame.AgentID;
        animate.AgentData.SessionID = frame.SessionID;
        //We send one animation
        animate.AnimationList = new AgentAnimationPacket.AnimationListBlock[1];
        animate.AnimationList[0] = new AgentAnimationPacket.AnimationListBlock();
        animate.AnimationList[0].AnimID = animationuuid;
        animate.AnimationList[0].StartAnim = run;

        //SayToUser("anim " + animname(animationuuid) + " " + run);
        proxy.InjectPacket(animate, Direction.Outgoing);
    }

    //return the name of an animation by its UUID
    private string animname(UUID arg)
    {
        return animuid2name[arg];
    }

    //handle animation packets from simulator
    private Packet AnimationPacketHandler(Packet packet, IPEndPoint sim) {        
        AvatarAnimationPacket animation = (AvatarAnimationPacket)packet;

        if (animation.Sender.ID == frame.AgentID)
        {
            //the received animation packet is about our Agent, handle it
            lock (SignaledAnimations)
            {
                // Reset the signaled animation list
                SignaledAnimations.Clear();
                //fill it with the fresh list from simulator
                for (int i = 0; i < animation.AnimationList.Length; i++)
                {
                    UUID animID = animation.AnimationList[i].AnimID;
                    int sequenceID = animation.AnimationList[i].AnimSequenceID;

                    // Add this animation to the list of currently signaled animations
                    SignaledAnimations[animID] = sequenceID;
                    //SayToUser("Animation: " + animname(animID));
                }
            }

            //we now have a list of currently running animations
            //Start override animations if necessary
            foreach (UUID key in overrides.Keys) 
            {                
                //For each overriden animation key, test if its override is running
                if (SignaledAnimations.ContainsKey(key) && (!overrideanimationisplaying[key] ))
                {
                    //An overriden animation is present and its override animation
                    //isnt currently playing                    
                    //Start the override animation
                    //SayToUser("animation " + animname(key) + " started, will override with " + animname(overrides[key]));
                    overrideanimationisplaying[key] = true;
                    Animate(overrides[key], true);                    
                }
                else if ((!SignaledAnimations.ContainsKey(key)) && overrideanimationisplaying[key])
                {
                    //an override animation is currently playing, but it's overriden 
                    //animation is not.                    
                    //stop the override animation
                    //SayToUser("animation " + animname(key) + " stopped, will override with " + animname(overrides[key]));
                    overrideanimationisplaying[key] = false;
                    Animate(overrides[key], false);
                }
            }            
        }
        //Let the packet go to the client
        return packet;
    }

    //handle packets that contain info about the notecard data transfer
    private Packet TransferInfoHandler(Packet packet, IPEndPoint simulator)
    {
        TransferInfoPacket info = (TransferInfoPacket)packet;
        
        if (info.TransferInfo.TransferID == assetdownloadID)
        {
            //this is our requested tranfer, handle it
            downloadsize = info.TransferInfo.Size;

            if ((StatusCode)info.TransferInfo.Status != StatusCode.OK)
            {
                SayToUser("Failed to read notecard");
            }
            if (downloadedbytes >= downloadsize)
            {
                //Download already completed!
                downloadCompleted();
            }
            //intercept packet
            return null;
        }
        return packet;
    }

    //handle packets which contain the notecard data
    private Packet TransferPacketHandler(Packet packet, IPEndPoint simulator)
    {
        TransferPacketPacket asset = (TransferPacketPacket)packet;      

        if (asset.TransferData.TransferID == assetdownloadID) {                    
            Buffer.BlockCopy(asset.TransferData.Data, 0, buffer, 1000 * asset.TransferData.Packet,
                asset.TransferData.Data.Length);
            downloadedbytes += asset.TransferData.Data.Length;

            // Check if we downloaded the full asset
            if (downloadedbytes >= downloadsize)
            {
                downloadCompleted();
            }
            //Intercept packet
            return null;
        }
        return packet;
    }

    private void downloadCompleted()
    {
        //We have the notecard.
        //Stop looking at transfer packets
        proxy.RemoveDelegate(PacketType.TransferPacket, Direction.Incoming, this.transferPacketDelegate);
        //crop the buffer size
        byte[] tmp = new byte[downloadedbytes];
        Buffer.BlockCopy(buffer, 0, tmp, 0, downloadedbytes);
        buffer = tmp;       
        String notecardtext = getNotecardText(Utils.BytesToString(buffer));        

        //Load config, wetikon format
        loadWetIkon(notecardtext);
    }

    private void loadWetIkon(string config)
    {
        //Reinitialise override table
        overrides = new Dictionary<UUID,UUID>();
        overrideanimationisplaying = new Dictionary<UUID, bool>();

        animuid2name = new Dictionary<UUID,string>();
        foreach (UUID key in wetikonanims )
        {
            animuid2name[key] = wetikonanimnames[Array.IndexOf(wetikonanims, key)];            
        }        

        //list of animations in wetikon                

        //read every second line in the config
        char[] sep = { '\n' };
        string[] lines = config.Split(sep);
        int length = lines.Length;
        int i = 1;
        while (i < length) {
            //Read animation name and look it up
            string animname = lines[i].Trim();
            //SayToUser("anim: " + animname);
            if (animname != "")
            {
                if (currentFolderItems.ContainsKey(animname))
                {
                    UUID over = currentFolderItems[animname].AssetUUID;
                    UUID orig = wetikonanims[((i + 1) / 2) - 1];
                    //put it in overrides
                    animuid2name[over] = animname;                    
                    overrides[orig] = over;
                    overrideanimationisplaying[orig] = false;
                    //SayToUser(wetikonanimnames[((i + 1) / 2) - 1] + " overriden by " + animname + " ( " + over + ")");
                }
                else
                {
                    //Not found
                    SayToUser(animname + " not found.");
                }
            }
            i += 2;
        }
        SayToUser("Notecard read, " + overrides.Count + " animations found");
    }

    private string getNotecardText(string data)
    {
        // Version 1 format:
        //              Linden text version 1
        //              {
        //                      <EmbeddedItemList chunk>
        //                      Text length
        //                      <ASCII text; 0x80 | index = embedded item>
        //              }

        // Version 2 format: (NOTE: Imports identically to version 1)
        //              Linden text version 2
        //              {
        //                      <EmbeddedItemList chunk>
        //                      Text length
        //                      <UTF8 text; FIRST_EMBEDDED_CHAR + index = embedded item>
        //              }
        int i = 0;
        char[] sep = { '\n' };
        string[] lines = data.Split(sep);
        int length = lines.Length;
        string result = "";

        //check format
        if (!lines[i].StartsWith("Linden text version "))
        {
            SayToUser("error");
            return "";
        }

        //{
        i++;
        if (lines[i] != "{")
        {
            SayToUser("error");
            return "";
        }

        i++;
        if (lines[i] != "LLEmbeddedItems version 1")
        {
            SayToUser("error");
            return "";
        }

        //{
        i++;
        if (lines[i] != "{")
        {
            SayToUser("error");
            return "";
        }

        //count ...
        i++;
        if (!lines[i].StartsWith("count "))
        {
            SayToUser("error");
            return "";
        }

        //}
        i++;
        if (lines[i] != "}")
        {
            SayToUser("error");
            return "";
        }

        //Text length ...
        i++;
        if (!lines[i].StartsWith("Text length "))
        {
            SayToUser("error");
            return "";
        }

        i++;
        while (i < length)
        {            
            result += lines[i] + "\n";
            i++;
        }
        result = result.Substring(0, result.Length - 3);        
        return result;
    }
}
