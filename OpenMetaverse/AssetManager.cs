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
using System.Threading;
using System.IO;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse
{
    #region Enums

    /// <summary>
    /// The different types of grid assets
    /// </summary>
    public enum AssetType : sbyte
    {
        /// <summary>Unknown asset type</summary>
        Unknown = -1,
        /// <summary>Texture asset, stores in JPEG2000 J2C stream format</summary>
        Texture = 0,
        /// <summary>Sound asset</summary>
        Sound = 1,
        /// <summary>Calling card for another avatar</summary>
        CallingCard = 2,
        /// <summary>Link to a location in world</summary>
        Landmark = 3,
        // <summary>Legacy script asset, you should never see one of these</summary>
        //[Obsolete]
        //Script = 4,
        /// <summary>Collection of textures and parameters that can be 
        /// worn by an avatar</summary>
        Clothing = 5,
        /// <summary>Primitive that can contain textures, sounds, 
        /// scripts and more</summary>
        Object = 6,
        /// <summary>Notecard asset</summary>
        Notecard = 7,
        /// <summary>Holds a collection of inventory items</summary>
        Folder = 8,
        /// <summary>Root inventory folder</summary>
        RootFolder = 9,
        /// <summary>Linden scripting language script</summary>
        LSLText = 10,
        /// <summary>LSO bytecode for a script</summary>
        LSLBytecode = 11,
        /// <summary>Uncompressed TGA texture</summary>
        TextureTGA = 12,
        /// <summary>Collection of textures and shape parameters that can
        /// be worn</summary>
        Bodypart = 13,
        /// <summary>Trash folder</summary>
        TrashFolder = 14,
        /// <summary>Snapshot folder</summary>
        SnapshotFolder = 15,
        /// <summary>Lost and found folder</summary>
        LostAndFoundFolder = 16,
        /// <summary>Uncompressed sound</summary>
        SoundWAV = 17,
        /// <summary>Uncompressed TGA non-square image, not to be used as a
        /// texture</summary>
        ImageTGA = 18,
        /// <summary>Compressed JPEG non-square image, not to be used as a
        /// texture</summary>
        ImageJPEG = 19,
        /// <summary>Animation</summary>
        Animation = 20,
        /// <summary>Sequence of animations, sounds, chat, and pauses</summary>
        Gesture = 21,
        /// <summary>Simstate file</summary>
        Simstate = 22,
    }

    public enum EstateAssetType : int
    {
        None = -1,
        Covenant = 0
    }

    /// <summary>
    /// 
    /// </summary>
    public enum StatusCode
    {
        /// <summary>OK</summary>
        OK = 0,
        /// <summary>Transfer completed</summary>
        Done = 1,
        /// <summary></summary>
        Skip = 2,
        /// <summary></summary>
        Abort = 3,
        /// <summary>Unknown error occurred</summary>
        Error = -1,
        /// <summary>Equivalent to a 404 error</summary>
        UnknownSource = -2,
        /// <summary>Client does not have permission for that resource</summary>
        InsufficientPermissiosn = -3,
        /// <summary>Unknown status</summary>
        Unknown = -4
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ChannelType : int
    {
        /// <summary></summary>
        Unknown = 0,
        /// <summary>Unknown</summary>
        Misc = 1,
        /// <summary>Virtually all asset transfers use this channel</summary>
        Asset = 2
    }

    /// <summary>
    /// 
    /// </summary>
    public enum SourceType : int
    {
        /// <summary></summary>
        Unknown = 0,
        /// <summary>Asset from the asset server</summary>
        Asset = 2,
        /// <summary>Inventory item</summary>
        SimInventoryItem = 3,
        /// <summary>Estate asset, such as an estate covenant</summary>
        SimEstate = 4
    }

    /// <summary>
    /// 
    /// </summary>
    public enum TargetType : int
    {
        /// <summary></summary>
        Unknown = 0,
        /// <summary></summary>
        File = 1,
        /// <summary></summary>
        VFile = 2
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ImageType : byte
    {
        /// <summary></summary>
        Normal = 0,
        /// <summary></summary>
        Baked = 1
    }

    /// <summary>
    /// Image file format
    /// </summary>
    public enum ImageCodec : byte
    {
        Invalid = 0,
        RGB = 1,
        J2C = 2,
        BMP = 3,
        TGA = 4,
        JPEG = 5,
        DXT = 6,
        PNG = 7
    }

    public enum TransferError : int
    {
        None = 0,
        Failed = -1,
        AssetNotFound = -3,
        AssetNotFoundInDatabase = -4,
        InsufficientPermissions = -5,
        EOF = -39,
        CannotOpenFile = -42,
        FileNotFound = -43,
        FileIsEmpty = -44,
        TCPTimeout = -23016,
        CircuitGone = -23017
    }

    #endregion Enums
    /*
    public static class AssetTypeParser
    {
        private static readonly ReversableDictionary<string, AssetType> AssetTypeMap = new ReversableDictionary<string, AssetType>();
        static AssetTypeParser()
        {
            AssetTypeMap.Add("animatn", AssetType.Animation);
            AssetTypeMap.Add("clothing", AssetType.Clothing);
            AssetTypeMap.Add("callcard", AssetType.CallingCard);
            AssetTypeMap.Add("object", AssetType.Object);
            AssetTypeMap.Add("texture", AssetType.Texture);
            AssetTypeMap.Add("sound", AssetType.Sound);
            AssetTypeMap.Add("bodypart", AssetType.Bodypart);
            AssetTypeMap.Add("gesture", AssetType.Gesture);
            AssetTypeMap.Add("lsltext", AssetType.LSLText);
            AssetTypeMap.Add("landmark", AssetType.Landmark);
            AssetTypeMap.Add("notecard", AssetType.Notecard);
            AssetTypeMap.Add("category", AssetType.Folder);
        }

        public static AssetType Parse(string str)
        {
            AssetType t;
            if (AssetTypeMap.TryGetValue(str, out t))
                return t;
            else
                return AssetType.Unknown;
        }

        public static string StringValueOf(AssetType type)
        {
            string str;
            if (AssetTypeMap.TryGetKey(type, out str))
                return str;
            else
                return "unknown";
        }
    }
    */
    #region Transfer Classes

    /// <summary>
    /// 
    /// </summary>
    public class Transfer
    {
        public UUID ID;
        public int Size;
        public byte[] AssetData = new byte[0];
        public int Transferred;
        public bool Success;
        public AssetType AssetType;

        private int transferStart;

        /// <summary>Number of milliseconds passed since the last transfer
        /// packet was received</summary>
        public int TimeSinceLastPacket
        {
            get { return Environment.TickCount - transferStart; }
            internal set { transferStart = Environment.TickCount + value; }
        }

        public Transfer()
        {
            AssetData = new byte[0];
            transferStart = Environment.TickCount;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AssetDownload : Transfer
    {
        public UUID AssetID;
        public ChannelType Channel;
        public SourceType Source;
        public TargetType Target;
        public StatusCode Status;
        public float Priority;
        public Simulator Simulator;

        internal AutoResetEvent HeaderReceivedEvent = new AutoResetEvent(false);
    }

    public class XferDownload : Transfer
    {
        public ulong XferID;
        public UUID VFileID;
        public AssetType Type;
        public uint PacketNum;
        public string Filename = String.Empty;
    }

    /// <summary>
    /// 
    /// </summary>
    public class ImageDownload : Transfer
    {
        public ushort PacketCount;
        public ImageCodec Codec;
        public bool NotFound;
        public Simulator Simulator;
        public SortedList<ushort, ushort> PacketsSeen;
        public ImageType ImageType;
        public int DiscardLevel;
        public float Priority;

        internal int InitialDataSize;
        internal AutoResetEvent HeaderReceivedEvent = new AutoResetEvent(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public class AssetUpload : Transfer
    {
        public UUID AssetID;
        public AssetType Type;
        public ulong XferID;
        public uint PacketNum;
    }
    public class ImageRequest
    {
        public ImageRequest(UUID imageid, ImageType type, float priority, int discardLevel)
        {
            ImageID = imageid;
            Type = type;
            Priority = priority;
            DiscardLevel = discardLevel;
        }
        public UUID ImageID;
        public ImageType Type;
        public float Priority;
        public int DiscardLevel;
    }
    #endregion Transfer Classes

    /// <summary>
    /// 
    /// </summary>
    public class AssetManager
    {
        #region Delegates

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transfer"></param>
        /// <param name="asset"></param>
        public delegate void AssetReceivedCallback(AssetDownload transfer, Asset asset);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xfer"></param>
        public delegate void XferReceivedCallback(XferDownload xfer);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="asset"></param>
        public delegate void ImageReceivedCallback(ImageDownload image, AssetTexture asset);
        /// <summary>
        /// 
        /// </summary>
        public delegate void ImageReceiveProgressCallback(UUID image, int lastPacket, int recieved, int total);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="upload"></param>
        public delegate void AssetUploadedCallback(AssetUpload upload);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="upload"></param>
        public delegate void UploadProgressCallback(AssetUpload upload);

        #endregion Delegates

        #region Events

        /// <summary></summary>
        public event AssetReceivedCallback OnAssetReceived;
        /// <summary></summary>
        public event XferReceivedCallback OnXferReceived;
        /// <summary></summary>
        public event ImageReceivedCallback OnImageReceived;
        /// <summary></summary>
        public event ImageReceiveProgressCallback OnImageReceiveProgress;
        /// <summary></summary>
        public event AssetUploadedCallback OnAssetUploaded;
        /// <summary></summary>
        public event UploadProgressCallback OnUploadProgress;

        #endregion Events

        /// <summary>Texture download cache</summary>
        public TextureCache Cache;

        private GridClient Client;
        private Dictionary<UUID, Transfer> Transfers = new Dictionary<UUID, Transfer>();
        private AssetUpload PendingUpload;
        private object PendingUploadLock = new object();
        private volatile bool WaitingForUploadConfirm = false;
        private System.Timers.Timer RefreshDownloadsTimer = new System.Timers.Timer(500.0);
        
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">A reference to the GridClient object</param>
        public AssetManager(GridClient client)
        {
            Client = client;
            Cache = new TextureCache(client);

            // Transfer packets for downloading large assets
            Client.Network.RegisterCallback(PacketType.TransferInfo, new NetworkManager.PacketCallback(TransferInfoHandler));
            Client.Network.RegisterCallback(PacketType.TransferPacket, new NetworkManager.PacketCallback(TransferPacketHandler));

            // Image downloading packets
            Client.Network.RegisterCallback(PacketType.ImageData, new NetworkManager.PacketCallback(ImageDataHandler));
            Client.Network.RegisterCallback(PacketType.ImagePacket, new NetworkManager.PacketCallback(ImagePacketHandler));
            Client.Network.RegisterCallback(PacketType.ImageNotInDatabase, new NetworkManager.PacketCallback(ImageNotInDatabaseHandler));

            // Xfer packets for uploading large assets
            Client.Network.RegisterCallback(PacketType.RequestXfer, new NetworkManager.PacketCallback(RequestXferHandler));
            Client.Network.RegisterCallback(PacketType.ConfirmXferPacket, new NetworkManager.PacketCallback(ConfirmXferPacketHandler));
            Client.Network.RegisterCallback(PacketType.AssetUploadComplete, new NetworkManager.PacketCallback(AssetUploadCompleteHandler));

            // Xfer packet for downloading misc assets
            Client.Network.RegisterCallback(PacketType.SendXferPacket, new NetworkManager.PacketCallback(SendXferPacketHandler));

            // HACK: Re-request stale pending image downloads
            RefreshDownloadsTimer.Elapsed += new System.Timers.ElapsedEventHandler(RefreshDownloadsTimer_Elapsed);
            RefreshDownloadsTimer.Start();
        }

        private void RefreshDownloadsTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (Transfers)
            {
                foreach (Transfer transfer in Transfers.Values)
                {
                    if (transfer is ImageDownload)
                    {
                        ImageDownload download = (ImageDownload)transfer;

                        uint packet = 0;
                        
                        if (download.PacketsSeen != null && download.PacketsSeen.Count > 0)
                        {
                            lock (download.PacketsSeen)
                            {
                                bool first = true;
                                foreach (KeyValuePair<ushort, ushort> packetSeen in download.PacketsSeen)
                                {
                                    if (first)
                                    {
                                        // Initially set this to the earliest packet received in the transfer
                                        packet = packetSeen.Value;
                                        first = false;
                                    }
                                    else
                                    {
                                        ++packet;

                                        // If there is a missing packet in the list, break and request the download
                                        // resume here
                                        if (packetSeen.Value != packet)
                                        {
                                            --packet;
                                            break;
                                        }
                                    }
                                }

                                ++packet;
                            }
                        }

                        if (download.TimeSinceLastPacket > 5000)
                        {
                            --download.DiscardLevel;
                            download.TimeSinceLastPacket = 0;
                            RequestImage(download.ID, download.ImageType, download.Priority, download.DiscardLevel, packet);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Request an asset download
        /// </summary>
        /// <param name="assetID">Asset UUID</param>
        /// <param name="type">Asset type, must be correct for the transfer to succeed</param>
        /// <param name="priority">Whether to give this transfer an elevated priority</param>
        /// <returns>The transaction ID generated for this transfer</returns>
        public UUID RequestAsset(UUID assetID, AssetType type, bool priority)
        {
            AssetDownload transfer = new AssetDownload();
            transfer.ID = UUID.Random();
            transfer.AssetID = assetID;
            //transfer.AssetType = type; // Set in TransferInfoHandler.
            transfer.Priority = 100.0f + (priority ? 1.0f : 0.0f);
            transfer.Channel = ChannelType.Asset;
            transfer.Source = SourceType.Asset;
            transfer.Simulator = Client.Network.CurrentSim;

            // Add this transfer to the dictionary
            lock (Transfers) Transfers[transfer.ID] = transfer;

            // Build the request packet and send it
            TransferRequestPacket request = new TransferRequestPacket();
            request.TransferInfo.ChannelType = (int)transfer.Channel;
            request.TransferInfo.Priority = transfer.Priority;
            request.TransferInfo.SourceType = (int)transfer.Source;
            request.TransferInfo.TransferID = transfer.ID;

            byte[] paramField = new byte[20];
            Buffer.BlockCopy(assetID.GetBytes(), 0, paramField, 0, 16);
            Buffer.BlockCopy(Utils.IntToBytes((int)type), 0, paramField, 16, 4);
            request.TransferInfo.Params = paramField;

            Client.Network.SendPacket(request, transfer.Simulator);
            return transfer.ID;
        }

        /// <summary>
        /// Request an asset download through the almost deprecated Xfer system
        /// </summary>
        /// <param name="filename">Filename of the asset to request</param>
        /// <param name="deleteOnCompletion">Whether or not to delete the asset
        /// off the server after it is retrieved</param>
        /// <param name="useBigPackets">Use large transfer packets or not</param>
        /// <param name="vFileID">UUID of the file to request, if filename is
        /// left empty</param>
        /// <param name="vFileType">Asset type of <code>vFileID</code>, or
        /// <code>AssetType.Unknown</code> if filename is not empty</param>
        /// <returns></returns>
        public ulong RequestAssetXfer(string filename, bool deleteOnCompletion, bool useBigPackets, UUID vFileID, AssetType vFileType)
        {
            UUID uuid = UUID.Random();
            ulong id = uuid.GetULong();

            XferDownload transfer = new XferDownload();
            transfer.XferID = id;
            transfer.ID = new UUID(id); // Our dictionary tracks transfers with UUIDs, so convert the ulong back
            transfer.Filename = filename;
            transfer.VFileID = vFileID;
            transfer.AssetType = vFileType;

            // Add this transfer to the dictionary
            lock (Transfers) Transfers[transfer.ID] = transfer;

            RequestXferPacket request = new RequestXferPacket();
            request.XferID.ID = id;
            request.XferID.Filename = Utils.StringToBytes(filename);
            request.XferID.FilePath = 4; // "Cache". This is a horrible thing that hardcodes a file path enumeration in to the
                                         // protocol. For asset downloads we should only ever need this value
            request.XferID.DeleteOnCompletion = deleteOnCompletion;
            request.XferID.UseBigPackets = useBigPackets;
            request.XferID.VFileID = vFileID;
            request.XferID.VFileType = (short)vFileType;

            Client.Network.SendPacket(request);

            return id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetID">Use UUID.Zero if you do not have the 
        /// asset ID but have all the necessary permissions</param>
        /// <param name="itemID">The item ID of this asset in the inventory</param>
        /// <param name="taskID">Use UUID.Zero if you are not requesting an 
        /// asset from an object inventory</param>
        /// <param name="ownerID">The owner of this asset</param>
        /// <param name="type">Asset type</param>
        /// <param name="priority">Whether to prioritize this asset download or not</param>
        public UUID RequestInventoryAsset(UUID assetID, UUID itemID, UUID taskID, UUID ownerID, AssetType type, bool priority)
        {
            AssetDownload transfer = new AssetDownload();
            transfer.ID = UUID.Random();
            transfer.AssetID = assetID;
            //transfer.AssetType = type; // Set in TransferInfoHandler.
            transfer.Priority = 100.0f + (priority ? 1.0f : 0.0f);
            transfer.Channel = ChannelType.Asset;
            transfer.Source = SourceType.SimInventoryItem;
            transfer.Simulator = Client.Network.CurrentSim;

            // Add this transfer to the dictionary
            lock (Transfers) Transfers[transfer.ID] = transfer;

            // Build the request packet and send it
            TransferRequestPacket request = new TransferRequestPacket();
            request.TransferInfo.ChannelType = (int)transfer.Channel;
            request.TransferInfo.Priority = transfer.Priority;
            request.TransferInfo.SourceType = (int)transfer.Source;
            request.TransferInfo.TransferID = transfer.ID;

            byte[] paramField = new byte[100];
            Buffer.BlockCopy(Client.Self.AgentID.GetBytes(), 0, paramField, 0, 16);
            Buffer.BlockCopy(Client.Self.SessionID.GetBytes(), 0, paramField, 16, 16);
            Buffer.BlockCopy(ownerID.GetBytes(), 0, paramField, 32, 16);
            Buffer.BlockCopy(taskID.GetBytes(), 0, paramField, 48, 16);
            Buffer.BlockCopy(itemID.GetBytes(), 0, paramField, 64, 16);
            Buffer.BlockCopy(assetID.GetBytes(), 0, paramField, 80, 16);
            Buffer.BlockCopy(Utils.IntToBytes((int)type), 0, paramField, 96, 4);
            request.TransferInfo.Params = paramField;

            Client.Network.SendPacket(request, transfer.Simulator);
            return transfer.ID;
        }

        public UUID RequestInventoryAsset(InventoryItem item, bool priority)
        {
            return RequestInventoryAsset(item.AssetUUID, item.UUID, UUID.Zero, item.OwnerID, item.AssetType, priority);
        }

        public void RequestEstateAsset()
        {
            throw new Exception("This function is not implemented yet!");
        }

        /// <summary>
        /// Initiate an image download. This is an asynchronous function
        /// </summary>
        /// <param name="imageID">The image to download</param>
        /// <param name="type">Type of the image to download, either a baked
        /// avatar texture or a normal texture</param>
        public void RequestImage(UUID imageID, ImageType type)
        {
            RequestImage(imageID, type, 1013000.0f, 0, 0);
        }

        /// <summary>
        /// Initiate an image download. This is an asynchronous function
        /// </summary>
        /// <param name="imageID">The image to download</param>
        /// <param name="type">Type of the image to download, either a baked
        /// avatar texture or a normal texture</param>
        /// <param name="priority">Priority level of the download. Default is
        /// <c>1,013,000.0f</c></param>
        /// <param name="discardLevel">Number of quality layers to discard.
        /// This controls the end marker of the data sent</param>
        /// <param name="packetNum">Packet number to start the download at.
        /// This controls the start marker of the data sent</param>
        /// <remarks>Sending a priority of 0 and a discardlevel of -1 aborts
        /// download</remarks>
        public void RequestImage(UUID imageID, ImageType type, float priority, int discardLevel, uint packetNum)
        {
            if (Cache.HasImage(imageID))
            {
                ImageDownload transfer = Cache.GetCachedImage(imageID);
                transfer.ImageType = type;

                if (null != transfer)
                {
                    if (null != OnImageReceived)
                    {
                        AssetTexture asset = new AssetTexture(transfer.ID, transfer.AssetData);

                        try { OnImageReceived(transfer, asset); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                    }
                    return;
                }
            }

            // Priority == 0 && DiscardLevel == -1 means cancel the transfer
            if (priority.Equals(0) && discardLevel.Equals(-1))
            {
                if (Transfers.ContainsKey(imageID))
                    Transfers.Remove(imageID);

                RequestImagePacket cancel = new RequestImagePacket();
                cancel.AgentData.AgentID = Client.Self.AgentID;
                cancel.AgentData.SessionID = Client.Self.SessionID;
                cancel.RequestImage = new RequestImagePacket.RequestImageBlock[1];
                cancel.RequestImage[0] = new RequestImagePacket.RequestImageBlock();
                cancel.RequestImage[0].DiscardLevel = -1;
                cancel.RequestImage[0].DownloadPriority = 0;
                cancel.RequestImage[0].Packet = 0;
                cancel.RequestImage[0].Image = imageID;
                cancel.RequestImage[0].Type = 0;
            }
            else
            {
                Simulator currentSim = Client.Network.CurrentSim;

                if (!Transfers.ContainsKey(imageID))
                {
                    // New download
                    ImageDownload transfer = new ImageDownload();
                    transfer.ID = imageID;
                    transfer.Simulator = currentSim;
                    transfer.ImageType = type;
                    transfer.DiscardLevel = discardLevel;
                    transfer.Priority = priority;

                    // Add this transfer to the dictionary
                    lock (Transfers) Transfers[transfer.ID] = transfer;

                    Logger.DebugLog("Adding image " + imageID.ToString() + " to the download queue");
                }
                else
                {
                    // Already downloading, just updating the priority
                    Transfer transfer = Transfers[imageID];
                    float percentComplete = ((float)transfer.Transferred / (float)transfer.Size) * 100f;
                    if (Single.IsNaN(percentComplete))
                        percentComplete = 0f;

                    Logger.DebugLog(String.Format("Updating priority on image transfer {0}, {1}% complete",
                        imageID, Math.Round(percentComplete, 2)));
                }

                // Build and send the request packet
                RequestImagePacket request = new RequestImagePacket();
                request.AgentData.AgentID = Client.Self.AgentID;
                request.AgentData.SessionID = Client.Self.SessionID;
                request.RequestImage = new RequestImagePacket.RequestImageBlock[1];
                request.RequestImage[0] = new RequestImagePacket.RequestImageBlock();
                request.RequestImage[0].DiscardLevel = (sbyte)discardLevel;
                request.RequestImage[0].DownloadPriority = priority;
                request.RequestImage[0].Packet = packetNum;
                request.RequestImage[0].Image = imageID;
                request.RequestImage[0].Type = (byte)type;

                Client.Network.SendPacket(request, currentSim);
            }
        }

        /// <summary>
        /// Requests multiple Images
        /// </summary>
        /// <param name="Images">List of requested images</param>
        public void RequestImages(List<ImageRequest> Images)
        {
            for (int iri = 0; iri < Images.Count; iri++)
            {
                if (Transfers.ContainsKey(Images[iri].ImageID))
                {
                    Images.RemoveAt(iri);
                }

                if (Cache.HasImage(Images[iri].ImageID))
                {
                    ImageDownload transfer = Cache.GetCachedImage(Images[iri].ImageID);
                    if (null != transfer)
                    {
                        if (null != OnImageReceived)
                        {
                            AssetTexture asset = new AssetTexture(transfer.ID, transfer.AssetData);

                            try { OnImageReceived(transfer, asset); }
                            catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                        }

                        Images.RemoveAt(iri);
                    }
                }
            }

            if (Images.Count > 0)
            {
                // Build and send the request packet
                RequestImagePacket request = new RequestImagePacket();
                request.AgentData.AgentID = Client.Self.AgentID;
                request.AgentData.SessionID = Client.Self.SessionID;
                request.RequestImage = new RequestImagePacket.RequestImageBlock[Images.Count];

                for (int iru = 0; iru < Images.Count; ++iru)
                {
                    ImageDownload transfer = new ImageDownload();
                    //transfer.AssetType = AssetType.Texture // Handled in ImageDataHandler.
                    transfer.ID = Images[iru].ImageID;
                    transfer.Simulator = Client.Network.CurrentSim;
                    transfer.ImageType = Images[iru].Type;
                    transfer.DiscardLevel = Images[iru].DiscardLevel;
                    transfer.Priority = Images[iru].Priority;

                    // Add this transfer to the dictionary
                    lock (Transfers) Transfers[transfer.ID] = transfer;
                    request.RequestImage[iru] = new RequestImagePacket.RequestImageBlock();
                    request.RequestImage[iru].DiscardLevel = (sbyte)Images[iru].DiscardLevel;
                    request.RequestImage[iru].DownloadPriority = Images[iru].Priority;
                    request.RequestImage[iru].Packet = 0;
                    request.RequestImage[iru].Image = Images[iru].ImageID;
                    request.RequestImage[iru].Type = (byte)Images[iru].Type;
                }

                Client.Network.SendPacket(request, Client.Network.CurrentSim);
            }
            else
            {
                Logger.Log("RequestImages() called for an image(s) we are already downloading or an empty list, ignoring",
                    Helpers.LogLevel.Info, Client);
            }
        }

        public UUID RequestUpload(Asset asset, bool storeLocal)
        {
            if (asset.AssetData == null)
                throw new ArgumentException("Can't upload an asset with no data (did you forget to call Encode?)");

            UUID assetID;
            UUID transferID = RequestUpload(out assetID, asset.AssetType, asset.AssetData, storeLocal);
            asset.AssetID = assetID;
            return transferID;
        }
        
        public UUID RequestUpload(AssetType type, byte[] data, bool storeLocal)
        {
            UUID assetID;
            return RequestUpload(out assetID, type, data, storeLocal);
        }

        /// <summary>
        /// Initiate an asset upload
        /// </summary>
        /// <param name="assetID">The ID this asset will have if the
        /// upload succeeds</param>
        /// <param name="type">Asset type to upload this data as</param>
        /// <param name="data">Raw asset data to upload</param>
        /// <param name="storeLocal">Whether to store this asset on the local
        /// simulator or the grid-wide asset server</param>
        /// <returns>The transaction ID of this transfer</returns>
        public UUID RequestUpload(out UUID assetID, AssetType type, byte[] data, bool storeLocal)
        {
            AssetUpload upload = new AssetUpload();
            upload.AssetData = data;
            upload.AssetType = type;
            upload.ID = UUID.Random();
            assetID = UUID.Combine(upload.ID, Client.Self.SecureSessionID);
            upload.AssetID = assetID;
            upload.Size = data.Length;
            upload.XferID = 0;

            // Build and send the upload packet
            AssetUploadRequestPacket request = new AssetUploadRequestPacket();
            request.AssetBlock.StoreLocal = storeLocal;
            request.AssetBlock.Tempfile = false; // This field is deprecated
            request.AssetBlock.TransactionID = upload.ID;
            request.AssetBlock.Type = (sbyte)type;

            if (data.Length + 100 < Settings.MAX_PACKET_SIZE)
            {
                Logger.Log(
                    String.Format("Beginning asset upload [Single Packet], ID: {0}, AssetID: {1}, Size: {2}",
                    upload.ID.ToString(), upload.AssetID.ToString(), upload.Size), Helpers.LogLevel.Info, Client);

                // The whole asset will fit in this packet, makes things easy
                request.AssetBlock.AssetData = data;
                upload.Transferred = data.Length;
            }
            else
            {
                Logger.Log(
                    String.Format("Beginning asset upload [Multiple Packets], ID: {0}, AssetID: {1}, Size: {2}",
                    upload.ID.ToString(), upload.AssetID.ToString(), upload.Size), Helpers.LogLevel.Info, Client);

                // Asset is too big, send in multiple packets
                request.AssetBlock.AssetData = new byte[0];
            }

            // Wait for the previous upload to receive a RequestXferPacket
            lock (PendingUploadLock)
            {
                const int UPLOAD_CONFIRM_TIMEOUT = 10000;
                const int SLEEP_INTERVAL = 50;
                int t = 0;
                while (WaitingForUploadConfirm && t < UPLOAD_CONFIRM_TIMEOUT)
                {
                    System.Threading.Thread.Sleep(SLEEP_INTERVAL);
                    t += SLEEP_INTERVAL;
                }

                if (t < UPLOAD_CONFIRM_TIMEOUT)
                {
                    WaitingForUploadConfirm = true;
                    PendingUpload = upload;
                    Client.Network.SendPacket(request);

                    return upload.ID;
                }
                else
                {
                    throw new Exception("Timeout waiting for previous asset upload to begin");
                }
            }
        }

        #region Helpers

        private Asset CreateAssetWrapper(AssetType type)
        {
            Asset asset;

            switch (type)
            {
                case AssetType.Notecard:
                    asset = new AssetNotecard();
                    break;
                case AssetType.LSLText:
                    asset = new AssetScriptText();
                    break;
                case AssetType.LSLBytecode:
                    asset = new AssetScriptBinary();
                    break;
                case AssetType.Texture:
                    asset = new AssetTexture();
                    break;
                case AssetType.Object:
                    asset = new AssetPrim();
                    break;
                case AssetType.Clothing:
                    asset = new AssetClothing();
                    break;
                case AssetType.Bodypart:
                    asset = new AssetBodypart();
                    break;
                case AssetType.Animation:
                    asset = new AssetAnimation();
                    break;
                default:
                    Logger.Log("Unimplemented asset type: " + type, Helpers.LogLevel.Error, Client);
                    return null;
            }

            return asset;
        }

        private Asset WrapAsset(AssetDownload download)
        {
            Asset asset = CreateAssetWrapper(download.AssetType);
            asset.AssetID = download.AssetID;
            asset.AssetData = download.AssetData;
            return asset;
        }

        private void SendNextUploadPacket(AssetUpload upload)
        {
            SendXferPacketPacket send = new SendXferPacketPacket();

            send.XferID.ID = upload.XferID;
            send.XferID.Packet = upload.PacketNum++;

            if (send.XferID.Packet == 0)
            {
                // The first packet reserves the first four bytes of the data for the
                // total length of the asset and appends 1000 bytes of data after that
                send.DataPacket.Data = new byte[1004];
                Buffer.BlockCopy(Utils.IntToBytes(upload.Size), 0, send.DataPacket.Data, 0, 4);
                Buffer.BlockCopy(upload.AssetData, 0, send.DataPacket.Data, 4, 1000);
                upload.Transferred += 1000;

                lock (Transfers)
                {
                    Transfers.Remove(upload.AssetID);
                    Transfers[upload.ID] = upload;
                }
            }
            else if ((send.XferID.Packet + 1) * 1000 < upload.Size)
            {
                // This packet is somewhere in the middle of the transfer, or a perfectly
                // aligned packet at the end of the transfer
                send.DataPacket.Data = new byte[1000];
                Buffer.BlockCopy(upload.AssetData, upload.Transferred, send.DataPacket.Data, 0, 1000);
                upload.Transferred += 1000;
            }
            else
            {
                // Special handler for the last packet which will be less than 1000 bytes
                int lastlen = upload.Size - ((int)send.XferID.Packet * 1000);
                send.DataPacket.Data = new byte[lastlen];
                Buffer.BlockCopy(upload.AssetData, (int)send.XferID.Packet * 1000, send.DataPacket.Data, 0, lastlen);
                send.XferID.Packet |= (uint)0x80000000; // This signals the final packet
                upload.Transferred += lastlen;
            }

            Client.Network.SendPacket(send);
        }

        private void SendConfirmXferPacket(ulong xferID, uint packetNum)
        {
            ConfirmXferPacketPacket confirm = new ConfirmXferPacketPacket();
            confirm.XferID.ID = xferID;
            confirm.XferID.Packet = packetNum;

            Client.Network.SendPacket(confirm);
        }

        #endregion Helpers

        #region Transfer Callbacks

        private void TransferInfoHandler(Packet packet, Simulator simulator)
        {
            if (OnAssetReceived != null)
            {
                TransferInfoPacket info = (TransferInfoPacket)packet;
                Transfer transfer;
                AssetDownload download;

                if (Transfers.TryGetValue(info.TransferInfo.TransferID, out transfer))
                {
                    download = (AssetDownload)transfer;

                    download.Channel = (ChannelType)info.TransferInfo.ChannelType;
                    download.Status = (StatusCode)info.TransferInfo.Status;
                    download.Target = (TargetType)info.TransferInfo.TargetType;
                    download.Size = info.TransferInfo.Size;

                    // TODO: Once we support mid-transfer status checking and aborting this
                    // will need to become smarter
                    if (download.Status != StatusCode.OK)
                    {
                        Logger.Log("Transfer failed with status code " + download.Status, Helpers.LogLevel.Warning, Client);

                        lock (Transfers) Transfers.Remove(download.ID);

                        // No data could have been received before the TransferInfo packet
                        download.AssetData = null;

                        // Fire the event with our transfer that contains Success = false;
                        try { OnAssetReceived(download, null); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                    }
                    else
                    {
                        download.AssetData = new byte[download.Size];

                        if (download.Source == SourceType.Asset && info.TransferInfo.Params.Length == 20)
                        {
                            download.AssetID = new UUID(info.TransferInfo.Params, 0);
                            download.AssetType = (AssetType)(sbyte)info.TransferInfo.Params[16];

                            //Client.DebugLog(String.Format("TransferInfo packet received. AssetID: {0} Type: {1}",
                            //    transfer.AssetID, type));
                        }
                        else if (download.Source == SourceType.SimInventoryItem && info.TransferInfo.Params.Length == 100)
                        {
                            // TODO: Can we use these?
                            //UUID agentID = new UUID(info.TransferInfo.Params, 0);
                            //UUID sessionID = new UUID(info.TransferInfo.Params, 16);
                            //UUID ownerID = new UUID(info.TransferInfo.Params, 32);
                            //UUID taskID = new UUID(info.TransferInfo.Params, 48);
                            //UUID itemID = new UUID(info.TransferInfo.Params, 64);
                            download.AssetID = new UUID(info.TransferInfo.Params, 80);
                            download.AssetType = (AssetType)(sbyte)info.TransferInfo.Params[96];

                            //Client.DebugLog(String.Format("TransferInfo packet received. AgentID: {0} SessionID: {1} " + 
                            //    "OwnerID: {2} TaskID: {3} ItemID: {4} AssetID: {5} Type: {6}", agentID, sessionID, 
                            //    ownerID, taskID, itemID, transfer.AssetID, type));
                        }
                        else
                        {
                            Logger.Log("Received a TransferInfo packet with a SourceType of " + download.Source.ToString() +
                                " and a Params field length of " + info.TransferInfo.Params.Length,
                                Helpers.LogLevel.Warning, Client);
                        }
                    }
                }
                else
                {
                    Logger.Log("Received a TransferInfo packet for an asset we didn't request, TransferID: " +
                        info.TransferInfo.TransferID, Helpers.LogLevel.Warning, Client);
                }
            }
        }

        private void TransferPacketHandler(Packet packet, Simulator simulator)
        {
            TransferPacketPacket asset = (TransferPacketPacket)packet;
            Transfer transfer;
            AssetDownload download;

            if (Transfers.TryGetValue(asset.TransferData.TransferID, out transfer))
            {
                download = (AssetDownload)transfer;

                if (download.Size == 0)
                {
                    Logger.DebugLog("TransferPacket received ahead of the transfer header, blocking...", Client);

                    // We haven't received the header yet, block until it's received or times out
                    download.HeaderReceivedEvent.WaitOne(1000 * 5, false);

                    if (download.Size == 0)
                    {
                        Logger.Log("Timed out while waiting for the asset header to download for " +
                            download.ID.ToString(), Helpers.LogLevel.Warning, Client);

                        // Abort the transfer
                        TransferAbortPacket abort = new TransferAbortPacket();
                        abort.TransferInfo.ChannelType = (int)download.Channel;
                        abort.TransferInfo.TransferID = download.ID;
                        Client.Network.SendPacket(abort, download.Simulator);

                        download.Success = false;
                        lock (Transfers) Transfers.Remove(download.ID);

                        // Fire the event with our transfer that contains Success = false
                        if (OnAssetReceived != null)
                        {
                            try { OnAssetReceived(download, null); }
                            catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                        }

                        return;
                    }
                }

                // This assumes that every transfer packet except the last one is exactly 1000 bytes,
                // hopefully that is a safe assumption to make
                Buffer.BlockCopy(asset.TransferData.Data, 0, download.AssetData, 1000 * asset.TransferData.Packet,
                    asset.TransferData.Data.Length);
                download.Transferred += asset.TransferData.Data.Length;

                //Client.DebugLog(String.Format("Transfer packet {0}, received {1}/{2}/{3} bytes for asset {4}",
                //    asset.TransferData.Packet, asset.TransferData.Data.Length, transfer.Transferred, transfer.Size,
                //    transfer.AssetID.ToString()));

                // Check if we downloaded the full asset
                if (download.Transferred >= download.Size)
                {
                    Logger.DebugLog("Transfer for asset " + download.AssetID.ToString() + " completed", Client);

                    download.Success = true;
                    lock (Transfers) Transfers.Remove(download.ID);

                    if (OnAssetReceived != null)
                    {
                        try { OnAssetReceived(download, WrapAsset(download)); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                    }
                }
            }
        }

        #endregion Transfer Callbacks

        #region Xfer Callbacks

        private void RequestXferHandler(Packet packet, Simulator simulator)
        {
            if (PendingUpload == null)
                Logger.Log("Received a RequestXferPacket for an unknown asset upload", Helpers.LogLevel.Warning, Client);
            else
            {
                AssetUpload upload = PendingUpload;
                PendingUpload = null;
                WaitingForUploadConfirm = false;
                RequestXferPacket request = (RequestXferPacket)packet;

                upload.XferID = request.XferID.ID;
                upload.Type = (AssetType)request.XferID.VFileType;

                UUID transferID = new UUID(upload.XferID);
                Transfers[transferID] = upload;

                // Send the first packet containing actual asset data
                SendNextUploadPacket(upload);
            }
        }

        private void ConfirmXferPacketHandler(Packet packet, Simulator simulator)
        {
            ConfirmXferPacketPacket confirm = (ConfirmXferPacketPacket)packet;

            // Building a new UUID every time an ACK is received for an upload is a horrible
            // thing, but this whole Xfer system is horrible
            UUID transferID = new UUID(confirm.XferID.ID);
            Transfer transfer;
            AssetUpload upload = null;

            if (Transfers.TryGetValue(transferID, out transfer))
            {
                upload = (AssetUpload)transfer;

                //Client.DebugLog(String.Format("ACK for upload {0} of asset type {1} ({2}/{3})",
                //    upload.AssetID.ToString(), upload.Type, upload.Transferred, upload.Size));

                if (OnUploadProgress != null)
                {
                    try { OnUploadProgress(upload); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }

                if (upload.Transferred < upload.Size)
                    SendNextUploadPacket(upload);
            }
        }

        private void AssetUploadCompleteHandler(Packet packet, Simulator simulator)
        {
            AssetUploadCompletePacket complete = (AssetUploadCompletePacket)packet;

            // If we uploaded an asset in a single packet, RequestXferHandler()
            // will never be called so we need to set this here as well
            WaitingForUploadConfirm = false;

            if (OnAssetUploaded != null)
            {
                bool found = false;
                KeyValuePair<UUID, Transfer> foundTransfer = new KeyValuePair<UUID, Transfer>();

                // Xfer system sucks really really bad. Where is the damn XferID?
                lock (Transfers)
                {
                    foreach (KeyValuePair<UUID, Transfer> transfer in Transfers)
                    {
                        if (transfer.Value.GetType() == typeof(AssetUpload))
                        {
                            AssetUpload upload = (AssetUpload)transfer.Value;

                            if ((upload).AssetID == complete.AssetBlock.UUID)
                            {
                                found = true;
                                foundTransfer = transfer;
                                upload.Success = complete.AssetBlock.Success;
                                upload.Type = (AssetType)complete.AssetBlock.Type;
                                break;
                            }
                        }
                    }
                }

                if (found)
                {
                    lock (Transfers) Transfers.Remove(foundTransfer.Key);

                    try { OnAssetUploaded((AssetUpload)foundTransfer.Value); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
                else
                {
                    Logger.Log(String.Format(
                        "Got an AssetUploadComplete on an unrecognized asset, AssetID: {0}, Type: {1}, Success: {2}",
                        complete.AssetBlock.UUID, (AssetType)complete.AssetBlock.Type, complete.AssetBlock.Success),
                        Helpers.LogLevel.Warning);
                }
            }
        }

        private void SendXferPacketHandler(Packet packet, Simulator simulator)
        {
            SendXferPacketPacket xfer = (SendXferPacketPacket)packet;

            // Lame ulong to UUID conversion, please go away Xfer system
            UUID transferID = new UUID(xfer.XferID.ID);
            Transfer transfer;
            XferDownload download = null;

            if (Transfers.TryGetValue(transferID, out transfer))
            {
                download = (XferDownload)transfer;

                // Apply a mask to get rid of the "end of transfer" bit
                uint packetNum = xfer.XferID.Packet & 0x0FFFFFFF;

                // Check for out of order packets, possibly indicating a resend
                if (packetNum != download.PacketNum)
                {
                    if (packetNum == download.PacketNum - 1)
                    {
                        Logger.DebugLog("Resending Xfer download confirmation for packet " + packetNum, Client);
                        SendConfirmXferPacket(download.XferID, packetNum);
                    }
                    else
                    {
                        Logger.Log("Out of order Xfer packet in a download, got " + packetNum + " expecting " + download.PacketNum,
                            Helpers.LogLevel.Warning, Client);
                    }

                    return;
                }

                if (packetNum == 0)
                {
                    // This is the first packet received in the download, the first four bytes are a network order size integer
                    // FIXME: Is this actually true?
                    byte[] bytes = xfer.DataPacket.Data;
                    download.Size = (bytes[3] + (bytes[2] << 8) + (bytes[1] << 16) + (bytes[0] << 24));
                    download.AssetData = new byte[download.Size];

                    Buffer.BlockCopy(xfer.DataPacket.Data, 4, download.AssetData, 0, xfer.DataPacket.Data.Length - 4);
                    download.Transferred += xfer.DataPacket.Data.Length - 4;
                }
                else
                {
                    Buffer.BlockCopy(xfer.DataPacket.Data, 0, download.AssetData, 1000 * (int)packetNum, xfer.DataPacket.Data.Length);
                    download.Transferred += xfer.DataPacket.Data.Length;
                }

                // Increment the packet number to the packet we are expecting next
                download.PacketNum++;

                // Confirm receiving this packet
                SendConfirmXferPacket(download.XferID, packetNum);

                if ((xfer.XferID.Packet & 0x80000000) != 0)
                {
                    // This is the last packet in the transfer
                    if (!String.IsNullOrEmpty(download.Filename))
                        Logger.DebugLog("Xfer download for asset " + download.Filename + " completed", Client);
                    else
                        Logger.DebugLog("Xfer download for asset " + download.VFileID.ToString() + " completed", Client);

                    download.Success = true;
                    lock (Transfers) Transfers.Remove(download.ID);

                    if (OnXferReceived != null)
                    {
                        try { OnXferReceived(download); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                    }
                }
            }
        }

        #endregion Xfer Callbacks

        #region Image Callbacks

        /// <summary>
        /// Handles the Image Data packet which includes the ID and Size of the image,
        /// along with the first block of data for the image. If the image is small enough
        /// there will be no additional packets
        /// </summary>
        private void ImageDataHandler(Packet packet, Simulator simulator)
        {
            ImageDataPacket data = (ImageDataPacket)packet;
            ImageDownload transfer = null;

            Logger.DebugLog(String.Format("ImageData: Size={0}, Packets={1}", data.ImageID.Size, data.ImageID.Packets));

            lock (Transfers)
            {
                if (Transfers.ContainsKey(data.ImageID.ID))
                {
                    transfer = (ImageDownload)Transfers[data.ImageID.ID];

                    // Don't set header information if we have already
                    // received it (due to re-request)
                    if (transfer.Size == 0)
                    {
                        //Client.DebugLog("Received first " + data.ImageData.Data.Length + " bytes for image " +
                        //    data.ImageID.ID.ToString());

                        if (OnImageReceiveProgress != null)
                        {
                            try { OnImageReceiveProgress(data.ImageID.ID, 0, data.ImageData.Data.Length, transfer.Size); }
                            catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                        }

                        transfer.Codec = (ImageCodec)data.ImageID.Codec;
                        transfer.PacketCount = data.ImageID.Packets;
                        transfer.Size = (int)data.ImageID.Size;
                        transfer.AssetData = new byte[transfer.Size];
                        transfer.AssetType = AssetType.Texture;
                        transfer.PacketsSeen = new SortedList<ushort, ushort>();
                        Buffer.BlockCopy(data.ImageData.Data, 0, transfer.AssetData, 0, data.ImageData.Data.Length);
                        transfer.InitialDataSize = data.ImageData.Data.Length;
                        transfer.Transferred += data.ImageData.Data.Length;
			            
                        // Check if we downloaded the full image
                        if (transfer.Transferred >= transfer.Size)
                        {
                            Transfers.Remove(transfer.ID);
                            transfer.Success = true;
                            Cache.SaveImageToCache(transfer.ID, transfer.AssetData);
                        }
                    }
                }
            }

            if (transfer != null)
            {
                transfer.HeaderReceivedEvent.Set();

                if (OnImageReceived != null && transfer.Transferred >= transfer.Size)
                {
                    AssetTexture asset = new AssetTexture(transfer.ID, transfer.AssetData);

                    try { OnImageReceived(transfer, asset); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }
        }

        /// <summary>
        /// Handles the remaining Image data that did not fit in the initial ImageData packet
        /// </summary>
        private void ImagePacketHandler(Packet packet, Simulator simulator)
        {
            ImagePacketPacket image = (ImagePacketPacket)packet;
            ImageDownload transfer = null;

            lock (Transfers)
            {
                if (Transfers.ContainsKey(image.ImageID.ID))
                {
                    transfer = (ImageDownload)Transfers[image.ImageID.ID];

                    if (transfer.Size == 0)
                    {
                        // We haven't received the header yet, block until it's received or times out
                        transfer.HeaderReceivedEvent.WaitOne(1000 * 5, false);

                        if (transfer.Size == 0)
                        {
                            Logger.Log("Timed out while waiting for the image header to download for " +
                                transfer.ID.ToString(), Helpers.LogLevel.Warning, Client);

                            transfer.Success = false;
                            Transfers.Remove(transfer.ID);
                            goto Callback;
                        }
                    }

                    // The header is downloaded, we can insert this data in to the proper position
                    // Only insert if we haven't seen this packet before
                    lock (transfer.PacketsSeen)
                    {
                        if (!transfer.PacketsSeen.ContainsKey(image.ImageID.Packet))
                        {
                            transfer.PacketsSeen[image.ImageID.Packet] = image.ImageID.Packet;
                            Buffer.BlockCopy(image.ImageData.Data, 0, transfer.AssetData,
                                transfer.InitialDataSize + (1000 * (image.ImageID.Packet - 1)),
                                image.ImageData.Data.Length);
                            transfer.Transferred += image.ImageData.Data.Length;
                        }
                    }

                    //Client.DebugLog("Received " + image.ImageData.Data.Length + "/" + transfer.Transferred +
                    //    "/" + transfer.Size + " bytes for image " + image.ImageID.ID.ToString());

                    transfer.TimeSinceLastPacket = 0;
                    
                    if (OnImageReceiveProgress != null)
                    {
                        try { OnImageReceiveProgress(image.ImageID.ID, image.ImageID.Packet, transfer.Transferred, transfer.Size); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                    }

                    // Check if we downloaded the full image
                    if (transfer.Transferred >= transfer.Size)
                    {
                        Cache.SaveImageToCache(transfer.ID, transfer.AssetData);
                        transfer.Success = true;
                        Transfers.Remove(transfer.ID);
                    }
                }
            }

        Callback:

            if (transfer != null && OnImageReceived != null && (transfer.Transferred >= transfer.Size || transfer.Size == 0))
            {
                AssetTexture asset = new AssetTexture(transfer.ID, transfer.AssetData);

                try { OnImageReceived(transfer, asset); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary>
        /// The requested image does not exist on the asset server
        /// </summary>
        private void ImageNotInDatabaseHandler(Packet packet, Simulator simulator)
        {
            ImageNotInDatabasePacket notin = (ImageNotInDatabasePacket)packet;
            ImageDownload transfer = null;

            lock (Transfers)
            {
                if (Transfers.ContainsKey(notin.ImageID.ID))
                {
                    transfer = (ImageDownload)Transfers[notin.ImageID.ID];
                    transfer.NotFound = true;
                    Transfers.Remove(transfer.ID);
                }
            }

            // Fire the event with our transfer that contains Success = false;
            if (transfer != null && OnImageReceived != null)
            {
                try { OnImageReceived(transfer, null); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        #endregion Image Callbacks
    }

    #region Texture Cache
    /// <summary>
    /// Class that handles the local image cache
    /// </summary>
    public class TextureCache
    {
        private GridClient Client;
        private Thread cleanerThread;
        private System.Timers.Timer cleanerTimer;
        private double pruneInterval = 1000 * 60 * 5;

        /// <summary>
        /// Allows setting weather to periodicale prune the cache if it grows too big
        /// Default is enabled, when caching is enabled
        /// </summary>
        public bool AutoPruneEnabled
        {
            set {
                if (!Operational()) {
                    return;
                } else {
                    cleanerTimer.Enabled = value;
                }
            }
            get { return cleanerTimer.Enabled;}
        }

        /// <summary>
        /// How long (in ms) between cache checks (default is 5 min.) 
        /// </summary>
        public double AutoPruneInterval
        {
            get { return pruneInterval; }
            set
            {
                pruneInterval = value;
                cleanerTimer.Interval = pruneInterval;
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">A reference to the GridClient object</param>
        public TextureCache(GridClient client)
        {
            Client = client;
            cleanerTimer = new System.Timers.Timer(pruneInterval);
            cleanerTimer.Elapsed += new System.Timers.ElapsedEventHandler(cleanerTimer_Elapsed);
            if (Operational()) {
                cleanerTimer.Enabled = true;
            } else {
                cleanerTimer.Enabled = false;
            }
        }

        /// <summary>
        /// Return bytes read from the local image cache, null if it does not exist
        /// </summary>
        /// <param name="imageID">UUID of the image we want to get</param>
        /// <returns>Raw bytes of the image, or null on failure</returns>
        public byte[] GetCachedImageBytes(UUID imageID)
        {
            if (!Operational()) {
                return null;
            }
            try {
                Logger.DebugLog("Reading " + FileName(imageID) + " from texture cache.");
                byte[] data = File.ReadAllBytes(FileName(imageID));
                return data;
            } catch (Exception ex) {
                Logger.Log("Failed reading image from cache (" + ex.Message + ")", Helpers.LogLevel.Warning, Client);
                return null;
            }
        }

        /// <summary>
        /// Returns ImageDownload object of the
        /// image from the local image cache, null if it does not exist
        /// </summary>
        /// <param name="imageID">UUID of the image we want to get</param>
        /// <returns>ImageDownload object containing the image, or null on failure</returns>
        public ImageDownload GetCachedImage(UUID imageID)
        {
            if (!Operational())
                return null;

            byte[] imageData = GetCachedImageBytes(imageID);
            if (imageData == null)
                return null;
            ImageDownload transfer = new ImageDownload();
            transfer.AssetType = AssetType.Texture;
            transfer.ID = imageID;
            transfer.Simulator = Client.Network.CurrentSim;
            transfer.Size = imageData.Length;
            transfer.Success = true;
            transfer.Transferred = imageData.Length;
            transfer.AssetData = imageData;
            return transfer;
        }

        /// <summary>
        /// Constructs a file name of the cached image
        /// </summary>
        /// <param name="imageID">UUID of the image</param>
        /// <returns>String with the file name of the cahced image</returns>
        private string FileName(UUID imageID)
        {
            return Client.Settings.TEXTURE_CACHE_DIR + Path.DirectorySeparatorChar + imageID.ToString();
        }

        /// <summary>
        /// Saves an image to the local cache
        /// </summary>
        /// <param name="imageID">UUID of the image</param>
        /// <param name="imageData">Raw bytes the image consists of</param>
        /// <returns>Weather the operation was successfull</returns>
        public bool SaveImageToCache(UUID imageID, byte[] imageData)
        {
            if (!Operational()) {
                return false;
            }
            
            try {
                Logger.DebugLog("Saving " + FileName(imageID) + " to texture cache.", Client);
                
                if (!Directory.Exists(Client.Settings.TEXTURE_CACHE_DIR)) {
                    Directory.CreateDirectory(Client.Settings.TEXTURE_CACHE_DIR);
                }
                
                File.WriteAllBytes(FileName(imageID), imageData);
            } catch (Exception ex) {
                Logger.Log("Failed saving image to cache (" + ex.Message + ")", Helpers.LogLevel.Warning, Client);
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Get the file name of the asset stored with gived UUID
        /// </summary>
        /// <param name="imageID">UUID of the image</param>
        /// <returns>Null if we don't have that UUID cached on disk, file name if found in the cache folder</returns>
        public string ImageFileName(UUID imageID)
        {
            if (!Operational())
            {
                return null;
            }

            string fileName = FileName(imageID);

            if (File.Exists(fileName))
                return fileName;
            else
                return null;
        }

        /// <summary>
        /// Checks if the image exists in the local cache
        /// </summary>
        /// <param name="imageID">UUID of the image</param>
        /// <returns>True is the image is stored in the cache, otherwise false</returns>
        public bool HasImage(UUID imageID)
        {
            if (!Operational()) {
                return false;
            }
            return File.Exists(FileName(imageID));
        }

        /// <summary>
        /// Wipes out entire cache
        /// </summary>
        public void Clear()
        {
            string cacheDir = Client.Settings.TEXTURE_CACHE_DIR;
            if (!Directory.Exists(cacheDir)) {
                return;
            }

            DirectoryInfo di = new DirectoryInfo(cacheDir);
            // We save file with UUID as file name, only delete those
            FileInfo[] files = di.GetFiles("????????-????-????-????-????????????", SearchOption.TopDirectoryOnly);

            int num = 0;
            foreach (FileInfo file in files) {
                file.Delete();
                ++num;
            }

            Logger.Log("Wiped out " + num + " files from the cache directory.", Helpers.LogLevel.Debug);
        }

        /// <summary>
        /// Brings cache size to the 90% of the max size
        /// </summary>
        public void Prune()
        {
            string cacheDir = Client.Settings.TEXTURE_CACHE_DIR;
            if (!Directory.Exists(cacheDir)) {
                return;
            }
            DirectoryInfo di = new DirectoryInfo(cacheDir);
            // We save file with UUID as file name, only count those
            FileInfo[] files = di.GetFiles("????????-????-????-????-????????????", SearchOption.TopDirectoryOnly);

            long size = GetFileSize(files);

            if (size > Client.Settings.TEXTURE_CACHE_MAX_SIZE) {
                Array.Sort(files, new SortFilesByAccesTimeHelper());
                long targetSize = (long)(Client.Settings.TEXTURE_CACHE_MAX_SIZE * 0.9);
                int num = 0;
                foreach (FileInfo file in files) {
                    ++num;
                    size -= file.Length;
                    file.Delete();
                    if (size < targetSize) {
                        break;
                    }
                }
                Logger.Log(num + " files deleted from the cache, cache size now: " + NiceFileSize(size), Helpers.LogLevel.Debug);
            } else {
                Logger.Log("Cache size is " + NiceFileSize(size) + ", file deletion not needed", Helpers.LogLevel.Debug);
            }

        }

        /// <summary>
        /// Asynchronously brings cache size to the 90% of the max size
        /// </summary>
        public void BeginPrune()
        {
            // Check if the background cache cleaning thread is active first
            if (cleanerThread != null && cleanerThread.IsAlive) {
                return;
            }

            lock (this) {
                cleanerThread = new Thread(new ThreadStart(this.Prune));
                cleanerThread.IsBackground = true;
                cleanerThread.Start();
            }
        }

        /// <summary>
        /// Adds up file sizes passes in a FileInfo array
        /// </summary>
        long GetFileSize(FileInfo[] files)
        {
            long ret = 0;
            foreach (FileInfo file in files) {
                ret += file.Length;
            }
            return ret;
        }

        /// <summary>
        /// Checks whether caching is enabled
        /// </summary>
        private bool Operational()
        {
            return Client.Settings.USE_TEXTURE_CACHE;
        }

        /// <summary>
        /// Periodically prune the cache
        /// </summary>
        private void cleanerTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            BeginPrune();
        }

        /// <summary>
        /// Nicely formats file sizes
        /// </summary>
        /// <param name="byteCount">Byte size we want to output</param>
        /// <returns>String with humanly readable file size</returns>
        private string NiceFileSize(long byteCount)
        {
            string size = "0 Bytes";
            if (byteCount >= 1073741824)
                size = String.Format("{0:##.##}", byteCount / 1073741824) + " GB";
            else if (byteCount >= 1048576)
                size = String.Format("{0:##.##}", byteCount / 1048576) + " MB";
            else if (byteCount >= 1024)
                size = String.Format("{0:##.##}", byteCount / 1024) + " KB";
            else if (byteCount > 0 && byteCount < 1024)
                size = byteCount.ToString() + " Bytes";

            return size;
        }

        /// <summary>
        /// Helper class for sorting files by their last accessed time
        /// </summary>
        private class SortFilesByAccesTimeHelper : IComparer<FileInfo>
        {
            int IComparer<FileInfo>.Compare(FileInfo f1, FileInfo f2)
            {
                if (f1.LastAccessTime > f2.LastAccessTime)
                    return 1;
                if (f1.LastAccessTime < f2.LastAccessTime)
                    return -1;
                else
                    return 0;
            }
        }
    }
    #endregion
}
