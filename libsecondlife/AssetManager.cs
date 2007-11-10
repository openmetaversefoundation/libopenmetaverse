using System;
using System.Collections.Generic;
using System.Threading;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife
{
    #region Enums

    /// <summary>
    /// The different types of assets in Second Life
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
        /// <summary>Legacy script asset, you should never see one of these</summary>
        [Obsolete]
        Script = 4,
        /// <summary>Collection of textures and parameters that can be 
        /// worn by an avatar</summary>
        Clothing = 5,
        /// <summary>Primitive that can contain textures, sounds, 
        /// scripts and more</summary>
        Primitive = 6,
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
        /// <summary>Arbitrary system files off the server</summary>
        [Obsolete]
        File = 1,
        /// <summary>Asset from the asset server</summary>
        Asset = 2,
        /// <summary>Inventory item</summary>
        SimInventoryItem = 3,
        /// <summary></summary>
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
        File,
        /// <summary></summary>
        VFile
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

    #endregion Enums

    #region Transfer Classes

    /// <summary>
    /// 
    /// </summary>
    public class Transfer
    {
        public delegate void Timeout(Transfer transfer);

        public event Timeout OnTimeout;

        public LLUUID ID;
        public int Size;
        public byte[] AssetData = new byte[0];
        public int Transferred;
        public bool Success;
        public AssetType AssetType;

        internal System.Timers.Timer TransferTimer = new System.Timers.Timer(Settings.TRANSFER_TIMEOUT);

        public Transfer()
        {
            TransferTimer.AutoReset = false;
            TransferTimer.Elapsed += new System.Timers.ElapsedEventHandler(TransferTimer_Elapsed);
        }

        private void TransferTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (OnTimeout != null)
            {
                try { OnTimeout(this); }
                catch (Exception ex) { SecondLife.LogStatic(ex.ToString(), Helpers.LogLevel.Error); }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AssetDownload : Transfer
    {
        public LLUUID AssetID;
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
        public LLUUID VFileID;
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
        public int Codec;
        public bool NotFound;
        public Simulator Simulator;

        internal int InitialDataSize;
        internal AutoResetEvent HeaderReceivedEvent = new AutoResetEvent(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public class AssetUpload : Transfer
    {
        public LLUUID AssetID;
        public AssetType Type;
        public ulong XferID;
        public uint PacketNum;
    }
    public class ImageRequest
    {
        public ImageRequest(LLUUID imageid, ImageType type, float priority, int discardLevel)
        {
            ImageID = imageid;
            Type = type;
            Priority = priority;
            DiscardLevel = discardLevel;
        }
        public LLUUID ImageID;
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
        public delegate void ImageReceivedCallback(ImageDownload image, AssetTexture asset);
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
        public event AssetUploadedCallback OnAssetUploaded;
        /// <summary></summary>
        public event UploadProgressCallback OnUploadProgress;

        #endregion Events

        private SecondLife Client;
        private Dictionary<LLUUID, Transfer> Transfers = new Dictionary<LLUUID, Transfer>();
        private AssetUpload PendingUpload;
        private AutoResetEvent PendingUploadEvent = new AutoResetEvent(true);

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">A reference to the SecondLife client object</param>
        public AssetManager(SecondLife client)
        {
            Client = client;

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
        }

        /// <summary>
        /// Request an asset download
        /// </summary>
        /// <param name="assetID">Asset UUID</param>
        /// <param name="type">Asset type, must be correct for the transfer to succeed</param>
        /// <param name="priority">Whether to give this transfer an elevated priority</param>
        /// <returns>The transaction ID generated for this transfer</returns>
        public LLUUID RequestAsset(LLUUID assetID, AssetType type, bool priority)
        {
            AssetDownload transfer = new AssetDownload();
            transfer.ID = LLUUID.Random();
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
            Array.Copy(assetID.GetBytes(), 0, paramField, 0, 16);
            Array.Copy(Helpers.IntToBytes((int)type), 0, paramField, 16, 4);
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
        public ulong RequestAssetXfer(string filename, bool deleteOnCompletion, bool useBigPackets, LLUUID vFileID, AssetType vFileType)
        {
            LLUUID uuid = LLUUID.Random();
            ulong id = uuid.ToULong();

            XferDownload transfer = new XferDownload();
            transfer.XferID = id;
            transfer.ID = new LLUUID(id); // Our dictionary tracks transfers with LLUUIDs, so convert the ulong back
            transfer.Filename = filename;
            transfer.VFileID = vFileID;
            transfer.AssetType = vFileType;

            // Add this transfer to the dictionary
            lock (Transfers) Transfers[transfer.ID] = transfer;

            RequestXferPacket request = new RequestXferPacket();
            request.XferID.ID = id;
            request.XferID.Filename = Helpers.StringToField(filename);
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
        /// <param name="assetID">Use LLUUID.Zero if you do not have the 
        /// asset ID but have all the necessary permissions</param>
        /// <param name="itemID">The item ID of this asset in the inventory</param>
        /// <param name="taskID">Use LLUUID.Zero if you are not requesting an 
        /// asset from an object inventory</param>
        /// <param name="ownerID">The owner of this asset</param>
        /// <param name="type">Asset type</param>
        /// <param name="priority">Whether to prioritize this asset download or not</param>
        public LLUUID RequestInventoryAsset(LLUUID assetID, LLUUID itemID, LLUUID taskID, LLUUID ownerID, AssetType type, bool priority)
        {
            AssetDownload transfer = new AssetDownload();
            transfer.ID = LLUUID.Random();
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
            Buffer.BlockCopy(Helpers.IntToBytes((int)type), 0, paramField, 96, 4);
            request.TransferInfo.Params = paramField;

            Client.Network.SendPacket(request, transfer.Simulator);
            return transfer.ID;
        }

        public LLUUID RequestInventoryAsset(InventoryItem item, bool priority)
        {
            return RequestInventoryAsset(item.AssetUUID, item.UUID, LLUUID.Zero, item.OwnerID, item.AssetType, priority);
        }

        public void RequestEstateAsset()
        {
            throw new Exception("This function is not implemented yet!");
        }

        /// <summary>
        /// Initiate an image download. This is an asynchronous function
        /// </summary>
        /// <param name="imageID">The image to download</param>
        /// <param name="type"></param>
        /// <param name="priority"></param>
        /// <param name="discardLevel"></param>
        public void RequestImage(LLUUID imageID, ImageType type, float priority, int discardLevel)
        {
            if (!Transfers.ContainsKey(imageID))
            {
                ImageDownload transfer = new ImageDownload();
                //transfer.AssetType = AssetType.Texture // Handled in ImageDataHandler.
                transfer.ID = imageID;
                transfer.Simulator = Client.Network.CurrentSim;

                // Add this transfer to the dictionary
                lock (Transfers) Transfers[transfer.ID] = transfer;

                // Build and send the request packet
                RequestImagePacket request = new RequestImagePacket();
                request.AgentData.AgentID = Client.Self.AgentID;
                request.AgentData.SessionID = Client.Self.SessionID;
                request.RequestImage = new RequestImagePacket.RequestImageBlock[1];
                request.RequestImage[0] = new RequestImagePacket.RequestImageBlock();
                request.RequestImage[0].DiscardLevel = (sbyte)discardLevel;
                request.RequestImage[0].DownloadPriority = priority;
                request.RequestImage[0].Packet = 0;
                request.RequestImage[0].Image = imageID;
                request.RequestImage[0].Type = (byte)type;

                Client.Network.SendPacket(request, transfer.Simulator);
            }
            else
            {
                Client.Log("RequestImage() called for an image we are already downloading, ignoring",
                    Helpers.LogLevel.Info);
            }
        }
        /// <summary>
        /// Requests multiple Images
        /// </summary>
        /// <param name="Images">List of requested images</param>
        public void RequestImages(List<ImageRequest> Images)
        {
            for (int iri = Images.Count; iri > 0; --iri)
            {
                if (Transfers.ContainsKey(Images[iri].ImageID))
                {
                    Images.RemoveAt(iri);
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
                Client.Log("RequestImages() called for an image(s) we are already downloading or an empty list, ignoring",
                    Helpers.LogLevel.Info);
            }
        }
        public LLUUID RequestUpload(Asset asset, bool tempFile, bool storeLocal, bool isPriority)
        {
            if (asset.AssetData == null)
                throw new ArgumentException("Can't upload an asset with no data (did you forget to call Encode?)");

            LLUUID assetID;
            LLUUID transferID = RequestUpload(out assetID, asset.AssetType, asset.AssetData, tempFile, storeLocal, isPriority);
            asset.AssetID = assetID;
            return transferID;
        }
        
        public LLUUID RequestUpload(AssetType type, byte[] data, bool tempFile, bool storeLocal, bool isPriority)
        {
            LLUUID assetID;
            return RequestUpload(out assetID, type, data, tempFile, storeLocal, isPriority);
        }

        /// <summary>
        /// Initiate an asset upload
        /// </summary>
        /// <param name="transactionID">The ID this asset will have if the
        /// upload succeeds</param>
        /// <param name="type">Asset type to upload this data as</param>
        /// <param name="data">Raw asset data to upload</param>
        /// <param name="tempFile">Whether this is a temporary file or not</param>
        /// <param name="storeLocal">Whether to store this asset on the local
        /// simulator or the grid-wide asset server</param>
        /// <param name="isPriority">Give this upload a higher priority</param>
        /// <returns>The transaction ID of this transfer</returns>
        public LLUUID RequestUpload(out LLUUID assetID, AssetType type, byte[] data, bool tempFile, bool storeLocal, bool isPriority)
        {
            AssetUpload upload = new AssetUpload();
            upload.AssetData = data;
            upload.AssetType = type;
            upload.ID = LLUUID.Random();
            assetID = LLUUID.Combine(upload.ID, Client.Self.SecureSessionID);
            upload.AssetID = assetID;
            upload.Size = data.Length;
            upload.XferID = 0;
            upload.TransferTimer.Interval = 10 * 1000; // 10 second timeout for no upload packet confirmation
            upload.OnTimeout += new Transfer.Timeout(Transfer_OnTimeout); 

            // Build and send the upload packet
            AssetUploadRequestPacket request = new AssetUploadRequestPacket();
            request.AssetBlock.StoreLocal = storeLocal;
            request.AssetBlock.Tempfile = tempFile;
            request.AssetBlock.TransactionID = upload.ID;
            request.AssetBlock.Type = (sbyte)type;

            if (data.Length + 100 < Settings.MAX_PACKET_SIZE)
            {
                Client.Log(
                    String.Format("Beginning asset upload [Single Packet], ID: {0}, AssetID: {1}, Size: {2}",
                    upload.ID.ToStringHyphenated(), upload.AssetID.ToStringHyphenated(), upload.Size),
                    Helpers.LogLevel.Info);

                // The whole asset will fit in this packet, makes things easy
                request.AssetBlock.AssetData = data;
                upload.Transferred = data.Length;
            }
            else
            {
                Client.Log(
                    String.Format("Beginning asset upload [Multiple Packets], ID: {0}, AssetID: {1}, Size: {2}",
                    upload.ID.ToStringHyphenated(), upload.AssetID.ToStringHyphenated(), upload.Size),
                    Helpers.LogLevel.Info);

                // Asset is too big, send in multiple packets
                request.AssetBlock.AssetData = new byte[0];
            }

            //Client.DebugLog(request.ToString());

            /*
            // Add this upload to the Transfers dictionary using the assetID as the key.
            // Once the simulator assigns an actual identifier for this upload it will be
            // removed from Transfers and reinserted with the proper identifier
            lock (Transfers) Transfers[upload.AssetID] = upload;
            */

            // Wait for the previous upload to receive a RequestXferPacket
            if (PendingUploadEvent.WaitOne(10000, false))
            {
                PendingUpload = upload;

                Client.Network.SendPacket(request);

                return upload.ID;
            }
            else
                throw new Exception("Timeout waiting for previous asset upload to begin");
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
                case AssetType.Primitive:
                    asset = new AssetPrim();
                    break;
                case AssetType.Clothing:
                    asset = new AssetClothing();
                    break;
                case AssetType.Bodypart:
                    asset = new AssetBodypart();
                    break;
                default:
                    Client.Log("Unimplemented asset type: " + type, Helpers.LogLevel.Error);
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
                Buffer.BlockCopy(Helpers.IntToBytes(upload.Size), 0, send.DataPacket.Data, 0, 4);
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

        private void Transfer_OnTimeout(Transfer transfer)
        {
            if (transfer is AssetUpload)
            {
                AssetUpload upload = (AssetUpload)transfer;
                LLUUID transferID = new LLUUID(upload.XferID);

                if (Transfers.ContainsKey(transferID))
                {
                    Client.Log(String.Format(
                        "Timed out waiting for an ACK during asset upload {0}, rolling back to packet number {1}",
                        upload.AssetID.ToStringHyphenated(), (upload.PacketNum - 1)), Helpers.LogLevel.Info);

                    // Resend the last block of data and reset the timeout timer
                    upload.PacketNum--;
                    upload.Transferred -= 1000;
                    upload.TransferTimer.Start();

                    SendNextUploadPacket(upload);
                }
                else
                {
                    Client.Log(String.Format("Upload {0} (Type: {1}, Success: {2}) timed out but is not being tracked",
                        upload.ID.ToStringHyphenated(), upload.AssetType, upload.Success), Helpers.LogLevel.Warning);
                }
            }
            else
            {
                if (Transfers.ContainsKey(transfer.ID))
                {
                    // TODO: Implement something here when timeouts for downloads are turned on
                }
            }
        }

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
                        Client.Log("Transfer failed with status code " + download.Status, Helpers.LogLevel.Warning);

                        lock (Transfers) Transfers.Remove(download.ID);

                        // No data could have been received before the TransferInfo packet
                        download.AssetData = null;

                        // Fire the event with our transfer that contains Success = false;
                        try { OnAssetReceived(download, null); }
                        catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                    }
                    else
                    {
                        download.AssetData = new byte[download.Size];

                        if (download.Source == SourceType.Asset && info.TransferInfo.Params.Length == 20)
                        {
                            download.AssetID = new LLUUID(info.TransferInfo.Params, 0);
                            download.AssetType = (AssetType)(sbyte)info.TransferInfo.Params[16];

                            //Client.DebugLog(String.Format("TransferInfo packet received. AssetID: {0} Type: {1}",
                            //    transfer.AssetID, type));
                        }
                        else if (download.Source == SourceType.SimInventoryItem && info.TransferInfo.Params.Length == 100)
                        {
                            // TODO: Can we use these?
                            LLUUID agentID = new LLUUID(info.TransferInfo.Params, 0);
                            LLUUID sessionID = new LLUUID(info.TransferInfo.Params, 16);
                            LLUUID ownerID = new LLUUID(info.TransferInfo.Params, 32);
                            LLUUID taskID = new LLUUID(info.TransferInfo.Params, 48);
                            LLUUID itemID = new LLUUID(info.TransferInfo.Params, 64);
                            download.AssetID = new LLUUID(info.TransferInfo.Params, 80);
                            download.AssetType = (AssetType)(sbyte)info.TransferInfo.Params[96];

                            //Client.DebugLog(String.Format("TransferInfo packet received. AgentID: {0} SessionID: {1} " + 
                            //    "OwnerID: {2} TaskID: {3} ItemID: {4} AssetID: {5} Type: {6}", agentID, sessionID, 
                            //    ownerID, taskID, itemID, transfer.AssetID, type));
                        }
                        else
                        {
                            Client.Log("Received a TransferInfo packet with a SourceType of " + download.Source.ToString() +
                                " and a Params field length of " + info.TransferInfo.Params.Length,
                                Helpers.LogLevel.Warning);
                        }
                    }
                }
                else
                {
                    Client.Log("Received a TransferInfo packet for an asset we didn't request, TransferID: " +
                        info.TransferInfo.TransferID, Helpers.LogLevel.Warning);
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

                // Reset the transfer timer
                download.TransferTimer.Stop();
                download.TransferTimer.Start();

                if (download.Size == 0)
                {
                    Client.DebugLog("TransferPacket received ahead of the transfer header, blocking...");

                    // We haven't received the header yet, block until it's received or times out
                    download.HeaderReceivedEvent.WaitOne(1000 * 20, false);

                    if (download.Size == 0)
                    {
                        Client.Log("Timed out while waiting for the asset header to download for " +
                            download.ID.ToStringHyphenated(), Helpers.LogLevel.Warning);

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
                            catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
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
                //    transfer.AssetID.ToStringHyphenated()));

                // Check if we downloaded the full asset
                if (download.Transferred >= download.Size)
                {
                    Client.DebugLog("Transfer for asset " + download.AssetID.ToStringHyphenated() + " completed");

                    download.Success = true;
                    lock (Transfers) Transfers.Remove(download.ID);

                    if (OnAssetReceived != null)
                    {
                        try { OnAssetReceived(download, WrapAsset(download)); }
                        catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                    }
                }
            }
        }

        #endregion Transfer Callbacks

        #region Xfer Callbacks

        private void RequestXferHandler(Packet packet, Simulator simulator)
        {
            if (PendingUpload == null)
                Client.Log("Received a RequestXferPacket for an unknown asset upload", Helpers.LogLevel.Warning);
            else
            {
                AssetUpload upload = PendingUpload;
                PendingUpload = null;
                PendingUploadEvent.Set();
                RequestXferPacket request = (RequestXferPacket)packet;

                upload.XferID = request.XferID.ID;
                upload.Type = (AssetType)request.XferID.VFileType;

                LLUUID transferID = new LLUUID(upload.XferID);
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
            LLUUID transferID = new LLUUID(confirm.XferID.ID);
            Transfer transfer;
            AssetUpload upload = null;

            if (Transfers.TryGetValue(transferID, out transfer))
            {
                upload = (AssetUpload)transfer;

                //Client.DebugLog(String.Format("ACK for upload {0} of asset type {1} ({2}/{3})",
                //    upload.AssetID.ToStringHyphenated(), upload.Type, upload.Transferred, upload.Size));

                // Reset the transfer timer
                upload.TransferTimer.Stop();
                upload.TransferTimer.Start();

                if (OnUploadProgress != null)
                {
                    try { OnUploadProgress(upload); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }

                if (upload.Transferred < upload.Size)
                    SendNextUploadPacket(upload);
            }
        }

        private void AssetUploadCompleteHandler(Packet packet, Simulator simulator)
        {
            AssetUploadCompletePacket complete = (AssetUploadCompletePacket)packet;

            if (OnAssetUploaded != null)
            {
                bool found = false;
                KeyValuePair<LLUUID, Transfer> foundTransfer = new KeyValuePair<LLUUID, Transfer>();

                // Xfer system sucks really really bad. Where is the damn XferID?
                lock (Transfers)
                {
                    foreach (KeyValuePair<LLUUID, Transfer> transfer in Transfers)
                    {
                        if (transfer.Value.GetType() == typeof(AssetUpload))
                        {
                            AssetUpload upload = (AssetUpload)transfer.Value;

                            if ((upload).AssetID == complete.AssetBlock.UUID)
                            {
                                // Stop the resend timer for this transfer
                                upload.TransferTimer.Stop();

                                found = true;
                                foundTransfer = transfer;
                                upload.Success = complete.AssetBlock.Success;
                                upload.Type = (AssetType)complete.AssetBlock.Type;
                                found = true;
                                break;
                            }
                        }
                    }
                }

                if (found)
                {
                    lock (Transfers) Transfers.Remove(foundTransfer.Key);

                    try { OnAssetUploaded((AssetUpload)foundTransfer.Value); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
            }
        }

        private void SendXferPacketHandler(Packet packet, Simulator simulator)
        {
            SendXferPacketPacket xfer = (SendXferPacketPacket)packet;

            // Lame ulong to LLUUID conversion, please go away Xfer system
            LLUUID transferID = new LLUUID(xfer.XferID.ID);
            Transfer transfer;
            XferDownload download = null;

            if (Transfers.TryGetValue(transferID, out transfer))
            {
                download = (XferDownload)transfer;

                // Reset the transfer timer
                download.TransferTimer.Stop();
                download.TransferTimer.Start();

                // Apply a mask to get rid of the "end of transfer" bit
                uint packetNum = xfer.XferID.Packet & 0x0FFFFFFF;

                // Check for out of order packets, possibly indicating a resend
                if (packetNum != download.PacketNum)
                {
                    if (packetNum == download.PacketNum - 1)
                    {
                        Client.DebugLog("Resending Xfer download confirmation for packet " + packetNum);
                        SendConfirmXferPacket(download.XferID, packetNum);
                    }
                    else
                    {
                        Client.Log("Out of order Xfer packet in a download, got " + packetNum + " expecting " + download.PacketNum,
                            Helpers.LogLevel.Warning);
                    }

                    return;
                }

                if (packetNum == 0)
                {
                    // This is the first packet received in the download, the first four bytes are a network order size integer
                    download.Size = (int)Helpers.BytesToUIntBig(xfer.DataPacket.Data);
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
                        Client.DebugLog("Xfer download for asset " + download.Filename + " completed");
                    else
                        Client.DebugLog("Xfer download for asset " + download.VFileID.ToStringHyphenated() + " completed");

                    download.Success = true;
                    lock (Transfers) Transfers.Remove(download.ID);

                    if (OnXferReceived != null)
                    {
                        try { OnXferReceived(download); }
                        catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
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

            lock (Transfers)
            {
                if (Transfers.ContainsKey(data.ImageID.ID))
                {
                    transfer = (ImageDownload)Transfers[data.ImageID.ID];

                    //Client.DebugLog("Received first " + data.ImageData.Data.Length + " bytes for image " +
                    //    data.ImageID.ID.ToStringHyphenated());

                    transfer.Codec = data.ImageID.Codec;
                    transfer.PacketCount = data.ImageID.Packets;
                    transfer.Size = (int)data.ImageID.Size;
                    transfer.AssetData = new byte[transfer.Size];
                    transfer.AssetType = AssetType.Texture;
                    Buffer.BlockCopy(data.ImageData.Data, 0, transfer.AssetData, 0, data.ImageData.Data.Length);
                    transfer.InitialDataSize = data.ImageData.Data.Length;
                    transfer.Transferred += data.ImageData.Data.Length;

                    // Check if we downloaded the full image
                    if (transfer.Transferred >= transfer.Size)
                    {
                        Transfers.Remove(transfer.ID);
                        transfer.Success = true;
                    }
                }
            }

            if (transfer != null)
            {
                transfer.HeaderReceivedEvent.Set();

                if (OnImageReceived != null && transfer.Transferred >= transfer.Size)
                {
                    try { OnImageReceived(transfer, new AssetTexture(transfer.AssetData)); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
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
                        transfer.HeaderReceivedEvent.WaitOne(1000 * 20, false);

                        if (transfer.Size == 0)
                        {
                            Client.Log("Timed out while waiting for the image header to download for " +
                                transfer.ID.ToStringHyphenated(), Helpers.LogLevel.Warning);

                            transfer.Success = false;
                            Transfers.Remove(transfer.ID);
                            goto Callback;
                        }
                    }

                    // The header is downloaded, we can insert this data in to the proper position
                    Array.Copy(image.ImageData.Data, 0, transfer.AssetData, transfer.InitialDataSize +
                        (1000 * (image.ImageID.Packet - 1)), image.ImageData.Data.Length);
                    transfer.Transferred += image.ImageData.Data.Length;

                    //Client.DebugLog("Received " + image.ImageData.Data.Length + "/" + transfer.Transferred +
                    //    "/" + transfer.Size + " bytes for image " + image.ImageID.ID.ToStringHyphenated());

                    // Check if we downloaded the full image
                    if (transfer.Transferred >= transfer.Size)
                    {
                        transfer.Success = true;
                        Transfers.Remove(transfer.ID);
                    }
                }
            }

        Callback:

            if (transfer != null && OnImageReceived != null && (transfer.Transferred >= transfer.Size || transfer.Size == 0))
            {
                try { OnImageReceived(transfer, new AssetTexture(transfer.AssetData)); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
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
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        #endregion Image Callbacks
    }
}
