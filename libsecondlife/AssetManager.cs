using System;
using System.Collections.Generic;
using System.Threading;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife
{
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

    /// <summary>
    /// 
    /// </summary>
    public class Transfer
    {
        public LLUUID ID = LLUUID.Zero;
        public int Size = 0;
        public byte[] AssetData = new byte[0];
        public int Transferred = 0;
        public bool Success = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public class AssetDownload : Transfer
    {
        public LLUUID AssetID = LLUUID.Zero;
        public ChannelType Channel = ChannelType.Unknown;
        public SourceType Source = SourceType.Unknown;
        public TargetType Target = TargetType.Unknown;
        public StatusCode Status = StatusCode.Unknown;
        public float Priority = 0.0f;
        public Simulator Simulator;

        internal AutoResetEvent HeaderReceivedEvent = new AutoResetEvent(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public class ImageDownload : Transfer
    {
        public ushort PacketCount = 0;
        public int Codec = 0;
        public bool NotFound = false;
        public Simulator Simulator;

        internal int InitialDataSize = 0;
        internal AutoResetEvent HeaderReceivedEvent = new AutoResetEvent(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public class AssetUpload : Transfer
    {
        public LLUUID AssetID = LLUUID.Zero;
        public AssetType Type = AssetType.Unknown;
        public ulong XferID = 0;
        public uint PacketNum = 0;
    }


    /// <summary>
    /// 
    /// </summary>
    public class AssetManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="asset"></param>
        public delegate void AssetReceivedCallback(AssetDownload asset);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        public delegate void ImageReceivedCallback(ImageDownload image);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="upload"></param>
        public delegate void AssetUploadedCallback(AssetUpload upload);


        /// <summary>
        /// 
        /// </summary>
        public event AssetReceivedCallback OnAssetReceived;
        /// <summary>
        /// 
        /// </summary>
        public event ImageReceivedCallback OnImageReceived;
        /// <summary>
        /// 
        /// </summary>
        public event AssetUploadedCallback OnAssetUploaded;

        private SecondLife Client;
        private Dictionary<LLUUID, Transfer> Transfers = new Dictionary<LLUUID, Transfer>();

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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetID"></param>
        /// <param name="type"></param>
        /// <param name="priority"></param>
        public void RequestAsset(LLUUID assetID, AssetType type, bool priority)
        {
            AssetDownload transfer = new AssetDownload();
            transfer.ID = LLUUID.Random();
            transfer.AssetID = assetID;
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
        public void RequestInventoryAsset(LLUUID assetID, LLUUID itemID, LLUUID taskID, LLUUID ownerID, AssetType type,
            bool priority)
        {
            AssetDownload transfer = new AssetDownload();
            transfer.ID = LLUUID.Random();
            transfer.AssetID = assetID;
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
            Array.Copy(Client.Network.AgentID.GetBytes(), 0, paramField, 0, 16);
            Array.Copy(Client.Network.SessionID.GetBytes(), 0, paramField, 16, 16);
            Array.Copy(ownerID.GetBytes(), 0, paramField, 32, 16);
            Array.Copy(taskID.GetBytes(), 0, paramField, 48, 16);
            Array.Copy(itemID.GetBytes(), 0, paramField, 64, 16);
            Array.Copy(assetID.GetBytes(), 0, paramField, 80, 16);
            Array.Copy(Helpers.IntToBytes((int)type), 0, paramField, 96, 4);
            request.TransferInfo.Params = paramField;

            Client.Network.SendPacket(request, transfer.Simulator);
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
                transfer.ID = imageID;
                transfer.Simulator = Client.Network.CurrentSim;

                // Add this transfer to the dictionary
                lock (Transfers) Transfers[transfer.ID] = transfer;

                // Build and send the request packet
                RequestImagePacket request = new RequestImagePacket();
                request.AgentData.AgentID = Client.Network.AgentID;
                request.AgentData.SessionID = Client.Network.SessionID;
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
        /// 
        /// </summary>
        /// <param name="transactionID">Usually a randomly generated UUID</param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="tempFile"></param>
        /// <param name="storeLocal"></param>
        /// <param name="isPriority"></param>
        public void RequestUpload(LLUUID transactionID, AssetType type, byte[] data, bool tempFile, bool storeLocal, 
            bool isPriority)
        {
            if (!Transfers.ContainsKey(transactionID))
            {
                AssetUpload upload = new AssetUpload();
                upload.AssetData = data;
                upload.ID = transactionID;
                upload.AssetID = ((transactionID == LLUUID.Zero) ? transactionID : transactionID.Combine(Client.Network.SecureSessionID));
                upload.Size = data.Length;
                upload.XferID = 0;

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

                // Add this upload to the Transfers dictionary using the assetID as the key.
                // Once the simulator assigns an actual identifier for this upload it will be
                // removed from Transfers and reinserted with the proper identifier
                lock (Transfers) Transfers[upload.AssetID] = upload;

                Client.Network.SendPacket(request);
            }
            else
            {
                Client.Log("RequestUpload() called for an asset we are already uploading, ignoring",
                    Helpers.LogLevel.Info);
            }
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

        private void TransferInfoHandler(Packet packet, Simulator simulator)
        {
            if (OnAssetReceived != null)
            {
                TransferInfoPacket info = (TransferInfoPacket)packet;

                if (Transfers.ContainsKey(info.TransferInfo.TransferID))
                {
                    AssetDownload transfer = (AssetDownload)Transfers[info.TransferInfo.TransferID];

                    transfer.Channel = (ChannelType)info.TransferInfo.ChannelType;
                    transfer.Status = (StatusCode)info.TransferInfo.Status;
                    transfer.Target = (TargetType)info.TransferInfo.TargetType;
                    transfer.Size = info.TransferInfo.Size;

                    // TODO: Once we support mid-transfer status checking and aborting this
                    // will need to become smarter
                    if (transfer.Status != StatusCode.OK)
                    {
                        Client.Log("Transfer failed with status code " + transfer.Status, Helpers.LogLevel.Warning);

                        lock (Transfers) Transfers.Remove(transfer.ID);

                        // No data could have been received before the TransferInfo packet
                        transfer.AssetData = null;

                        // Fire the event with our transfer that contains Success = false;
                        try { OnAssetReceived(transfer); }
                        catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                    }
                    else
                    {
                        transfer.AssetData = new byte[transfer.Size];

                        if (transfer.Source == SourceType.Asset && info.TransferInfo.Params.Length == 20)
                        {
                            transfer.AssetID = new LLUUID(info.TransferInfo.Params, 0);
                            AssetType type = (AssetType)(int)Helpers.BytesToUInt(info.TransferInfo.Params, 16);

                            //Client.DebugLog(String.Format("TransferInfo packet received. AssetID: {0} Type: {1}",
                            //    transfer.AssetID, type));
                        }
                        else if (transfer.Source == SourceType.SimInventoryItem && info.TransferInfo.Params.Length == 100)
                        {
                            // TODO: Can we use these?
                            LLUUID agentID = new LLUUID(info.TransferInfo.Params, 0);
                            LLUUID sessionID = new LLUUID(info.TransferInfo.Params, 16);
                            LLUUID ownerID = new LLUUID(info.TransferInfo.Params, 32);
                            LLUUID taskID = new LLUUID(info.TransferInfo.Params, 48);
                            LLUUID itemID = new LLUUID(info.TransferInfo.Params, 64);
                            transfer.AssetID = new LLUUID(info.TransferInfo.Params, 80);
                            AssetType type = (AssetType)(int)Helpers.BytesToUInt(info.TransferInfo.Params, 96);

                            //Client.DebugLog(String.Format("TransferInfo packet received. AgentID: {0} SessionID: {1} " + 
                            //    "OwnerID: {2} TaskID: {3} ItemID: {4} AssetID: {5} Type: {6}", agentID, sessionID, 
                            //    ownerID, taskID, itemID, transfer.AssetID, type));
                        }
                        else
                        {
                            Client.Log("Received a TransferInfo packet with a SourceType of " + transfer.Source.ToString() +
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

            if (Transfers.ContainsKey(asset.TransferData.TransferID))
            {
                AssetDownload transfer = (AssetDownload)Transfers[asset.TransferData.TransferID];

                if (transfer.Size == 0)
                {
                    Client.DebugLog("TransferPacket received ahead of the transfer header, blocking...");

                    // We haven't received the header yet, block until it's received or times out
                    transfer.HeaderReceivedEvent.WaitOne(1000 * 20, false);

                    if (transfer.Size == 0)
                    {
                        Client.Log("Timed out while waiting for the asset header to download for " +
                            transfer.ID.ToStringHyphenated(), Helpers.LogLevel.Warning);

                        // Abort the transfer
                        TransferAbortPacket abort = new TransferAbortPacket();
                        abort.TransferInfo.ChannelType = (int)transfer.Channel;
                        abort.TransferInfo.TransferID = transfer.ID;
                        Client.Network.SendPacket(abort, transfer.Simulator);

                        transfer.Success = false;
                        lock (Transfers) Transfers.Remove(transfer.ID);

                        // Fire the event with our transfer that contains Success = false
                        if (OnAssetReceived != null)
                        {
                            try { OnAssetReceived(transfer); }
                            catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                        }

                        return;
                    }
                }

                // This assumes that every transfer packet except the last one is exactly 1000 bytes,
                // hopefully that is a safe assumption to make
                Buffer.BlockCopy(asset.TransferData.Data, 0, transfer.AssetData, 1000 * asset.TransferData.Packet,
                    asset.TransferData.Data.Length);
                transfer.Transferred += asset.TransferData.Data.Length;

                //Client.DebugLog(String.Format("Transfer packet {0}, received {1}/{2}/{3} bytes for asset {4}",
                //    asset.TransferData.Packet, asset.TransferData.Data.Length, transfer.Transferred, transfer.Size,
                //    transfer.AssetID.ToStringHyphenated()));

                // Check if we downloaded the full asset
                if (transfer.Transferred >= transfer.Size)
                {
                    Client.DebugLog("Transfer for asset " + transfer.AssetID.ToStringHyphenated() + " completed");

                    transfer.Success = true;
                    lock (Transfers) Transfers.Remove(transfer.ID);

                    if (OnAssetReceived != null)
                    {
                        try { OnAssetReceived(transfer); }
                        catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                    }
                }
            }
            else
            {
                //Client.DebugLog("Received a TransferPacket for unknown transfer " +
                //    asset.TransferData.TransferID.ToStringHyphenated());
            }
        }

        private void RequestXferHandler(Packet packet, Simulator simulator)
        {
            AssetUpload upload = null;
            RequestXferPacket request = (RequestXferPacket)packet;

            // The Xfer system sucks. This will thankfully die soon when uploads are
            // moved to HTTP
            lock (Transfers)
            {
                // Associate the XferID with an upload. If an upload is initiated
                // before the previous one is associated with an XferID one or both
                // of them will undoubtedly fail
                foreach (Transfer transfer in Transfers.Values)
                {
                    if (transfer is AssetUpload)
                    {
                        if (((AssetUpload)transfer).XferID == 0)
                        {
                            // First match, use it
                            upload = (AssetUpload)transfer;
                            upload.XferID = request.XferID.ID;
                            upload.Type = (AssetType)request.XferID.VFileType;

                            // Remove this upload from the Transfers dictionary and re-insert
                            // it using the transferID as the key instead of the assetID
                            Transfers.Remove(upload.AssetID);

                            LLUUID transferID = new LLUUID(upload.XferID);
                            Transfers[transferID] = upload;

                            // Send the first packet containing actual asset data
                            SendNextUploadPacket(upload);

                            return;
                        }
                    }
                }
            }
        }

        private void ConfirmXferPacketHandler(Packet packet, Simulator simulator)
        {
            ConfirmXferPacketPacket confirm = (ConfirmXferPacketPacket)packet;

            // Building a new UUID every time an ACK is received for an upload is a horrible
            // thing, but this whole Xfer system is horrible
            LLUUID transferID = new LLUUID(confirm.XferID.ID);
            AssetUpload upload = null;

            lock (Transfers)
            {
                if (Transfers.ContainsKey(transferID))
                {
                    upload = (AssetUpload)Transfers[transferID];

                    //Client.DebugLog(String.Format("ACK for upload {0} of asset type {1} ({2}/{3})",
                    //    upload.AssetID.ToStringHyphenated(), upload.Type, upload.Transferred, upload.Size));

                    if (upload.Transferred < upload.Size)
                        SendNextUploadPacket((AssetUpload)Transfers[transferID]);
                }
            }
        }

        private void AssetUploadCompleteHandler(Packet packet, Simulator simulator)
        {
            AssetUploadCompletePacket complete = (AssetUploadCompletePacket)packet;

            Client.DebugLog(complete.ToString());
            
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

                        if (upload.AssetID == complete.AssetBlock.UUID)
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

                if (OnAssetUploaded != null)
                {
                    try { OnAssetUploaded((AssetUpload)foundTransfer.Value); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
            }
        }

        /// <summary>
        /// Handles the Image Data packet which includes the ID and Size of the image,
        /// along with the first block of data for the image. If the image is small enough
        /// there will be no additional packets
        /// </summary>
        public void ImageDataHandler(Packet packet, Simulator simulator)
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
                    try { OnImageReceived(transfer); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
            }
        }

        public void LogTransfer(LLUUID id)
        {
            lock (Transfers)
            {
                if (Transfers.ContainsKey(id))
                    Client.DebugLog("Transfer " + id + ": " + Transfers[id].Transferred + "/" + Transfers[id].Size + "\n");
            }
        }

        public void LogAllTransfers()
        {
            lock (Transfers)
            {
                foreach (KeyValuePair<LLUUID,Transfer> transfer in Transfers)
                    Client.DebugLog("Transfer " + transfer.Value.ID + ": " + transfer.Value.Transferred + "/" + transfer.Value.Size + "\n");
            }
        }
        
        /// <summary>
        /// Handles the remaining Image data that did not fit in the initial ImageData packet
        /// </summary>
        public void ImagePacketHandler(Packet packet, Simulator simulator)
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
                try { OnImageReceived(transfer); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        /// <summary>
        /// The requested image does not exist on the asset server
        /// </summary>
        public void ImageNotInDatabaseHandler(Packet packet, Simulator simulator)
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
                try { OnImageReceived(transfer); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }
    }
}
