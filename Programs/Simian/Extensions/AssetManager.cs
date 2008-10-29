using System;
using System.Collections.Generic;
using System.IO;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Imaging;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    public class AssetManager : IExtension<Simian>, IAssetProvider
    {
        public const string UPLOAD_DIR = "uploadedAssets";

        Simian server;
        Dictionary<UUID, Asset> AssetStore = new Dictionary<UUID, Asset>();
        Dictionary<ulong, Asset> CurrentUploads = new Dictionary<ulong, Asset>();
        string UploadDir;

        public AssetManager()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;

            UploadDir = Path.Combine(server.DataDir, UPLOAD_DIR);

            // Try to create the data directories if they don't already exist
            if (!Directory.Exists(server.DataDir))
            {
                try { Directory.CreateDirectory(server.DataDir); }
                catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Warning, ex); }
            }
            if (!Directory.Exists(UploadDir))
            {
                try { Directory.CreateDirectory(UploadDir); }
                catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Warning, ex); }
            }

            LoadAssets(server.DataDir);
            LoadAssets(UploadDir);

            server.UDP.RegisterPacketCallback(PacketType.AssetUploadRequest, new PacketCallback(AssetUploadRequestHandler));
            server.UDP.RegisterPacketCallback(PacketType.SendXferPacket, new PacketCallback(SendXferPacketHandler));
            server.UDP.RegisterPacketCallback(PacketType.AbortXfer, new PacketCallback(AbortXferHandler));
            server.UDP.RegisterPacketCallback(PacketType.TransferRequest, new PacketCallback(TransferRequestHandler));
        }

        public void Stop()
        {
        }

        public void StoreAsset(Asset asset)
        {
            if (asset is AssetTexture)
            {
                AssetTexture texture = (AssetTexture)asset;

                if (texture.DecodeLayerBoundaries())
                {
                    lock (AssetStore)
                        AssetStore[asset.AssetID] = texture;
                    if (!asset.Temporary)
                        SaveAsset(texture);
                }
                else
                {
                    Logger.Log(String.Format("Failed to decoded layer boundaries on texture {0}", texture.AssetID),
                        Helpers.LogLevel.Warning);
                }
            }
            else
            {
                if (asset.Decode())
                {
                    lock (AssetStore)
                        AssetStore[asset.AssetID] = asset;
                    if (!asset.Temporary)
                        SaveAsset(asset);
                }
                else
                {
                    Logger.Log(String.Format("Failed to decode {0} asset {1}", asset.AssetType, asset.AssetID),
                        Helpers.LogLevel.Warning);
                }
            }
        }

        public bool TryGetAsset(UUID id, out Asset asset)
        {
            return AssetStore.TryGetValue(id, out asset);
        }

        #region Xfer System

        void AssetUploadRequestHandler(Packet packet, Agent agent)
        {
            AssetUploadRequestPacket request = (AssetUploadRequestPacket)packet;
            UUID assetID = UUID.Combine(request.AssetBlock.TransactionID, agent.SecureSessionID);

            // Check if the asset is small enough to fit in a single packet
            if (request.AssetBlock.AssetData.Length != 0)
            {
                // Create a new asset from the completed upload
                Asset asset = CreateAsset((AssetType)request.AssetBlock.Type, assetID, request.AssetBlock.AssetData);
                if (asset == null)
                {
                    Logger.Log("Failed to create asset from uploaded data", Helpers.LogLevel.Warning);
                    return;
                }

                Logger.DebugLog(String.Format("Storing uploaded asset {0} ({1})", assetID, asset.AssetType));

                asset.Temporary = (request.AssetBlock.Tempfile | request.AssetBlock.StoreLocal);

                // Store the asset
                StoreAsset(asset);

                // Send a success response
                AssetUploadCompletePacket complete = new AssetUploadCompletePacket();
                complete.AssetBlock.Success = true;
                complete.AssetBlock.Type = request.AssetBlock.Type;
                complete.AssetBlock.UUID = assetID;
                server.UDP.SendPacket(agent.AgentID, complete, PacketCategory.Inventory);
            }
            else
            {
                // Create a new (empty) asset for the upload
                Asset asset = CreateAsset((AssetType)request.AssetBlock.Type, assetID, null);
                if (asset == null)
                {
                    Logger.Log("Failed to create asset from uploaded data", Helpers.LogLevel.Warning);
                    return;
                }

                Logger.DebugLog(String.Format("Starting upload for {0} ({1})", assetID, asset.AssetType));

                asset.Temporary = (request.AssetBlock.Tempfile | request.AssetBlock.StoreLocal);

                RequestXferPacket xfer = new RequestXferPacket();
                xfer.XferID.DeleteOnCompletion = request.AssetBlock.Tempfile;
                xfer.XferID.FilePath = 0;
                xfer.XferID.Filename = new byte[0];
                xfer.XferID.ID = request.AssetBlock.TransactionID.GetULong();
                xfer.XferID.UseBigPackets = false;
                xfer.XferID.VFileID = asset.AssetID;
                xfer.XferID.VFileType = request.AssetBlock.Type;

                // Add this asset to the current upload list
                lock (CurrentUploads)
                    CurrentUploads[xfer.XferID.ID] = asset;

                server.UDP.SendPacket(agent.AgentID, xfer, PacketCategory.Inventory);
            }
        }

        void SendXferPacketHandler(Packet packet, Agent agent)
        {
            SendXferPacketPacket xfer = (SendXferPacketPacket)packet;

            Asset asset;
            if (CurrentUploads.TryGetValue(xfer.XferID.ID, out asset))
            {
                if (asset.AssetData == null)
                {
                    if (xfer.XferID.Packet != 0)
                    {
                        Logger.Log(String.Format("Received Xfer packet {0} before the first packet!",
                            xfer.XferID.Packet), Helpers.LogLevel.Error);
                        return;
                    }

                    uint size = Utils.BytesToUInt(xfer.DataPacket.Data);
                    asset.AssetData = new byte[size];

                    Buffer.BlockCopy(xfer.DataPacket.Data, 4, asset.AssetData, 0, xfer.DataPacket.Data.Length - 4);

                    // Confirm the first upload packet
                    ConfirmXferPacketPacket confirm = new ConfirmXferPacketPacket();
                    confirm.XferID.ID = xfer.XferID.ID;
                    confirm.XferID.Packet = xfer.XferID.Packet;
                    server.UDP.SendPacket(agent.AgentID, confirm, PacketCategory.Asset);
                }
                else
                {
                    Buffer.BlockCopy(xfer.DataPacket.Data, 0, asset.AssetData, (int)xfer.XferID.Packet * 1000,
                        xfer.DataPacket.Data.Length);

                    // Confirm this upload packet
                    ConfirmXferPacketPacket confirm = new ConfirmXferPacketPacket();
                    confirm.XferID.ID = xfer.XferID.ID;
                    confirm.XferID.Packet = xfer.XferID.Packet;
                    server.UDP.SendPacket(agent.AgentID, confirm, PacketCategory.Asset);

                    if ((xfer.XferID.Packet & (uint)0x80000000) != 0)
                    {
                        // Asset upload finished
                        Logger.DebugLog(String.Format("Completed Xfer upload of asset {0} ({1}", asset.AssetID, asset.AssetType));

                        lock (CurrentUploads)
                            CurrentUploads.Remove(xfer.XferID.ID);

                        StoreAsset(asset);

                        AssetUploadCompletePacket complete = new AssetUploadCompletePacket();
                        complete.AssetBlock.Success = true;
                        complete.AssetBlock.Type = (sbyte)asset.AssetType;
                        complete.AssetBlock.UUID = asset.AssetID;
                        server.UDP.SendPacket(agent.AgentID, complete, PacketCategory.Asset);
                    }
                }
            }
            else
            {
                Logger.DebugLog("Received a SendXferPacket for an unknown upload");
            }
        }

        void AbortXferHandler(Packet packet, Agent agent)
        {
            AbortXferPacket abort = (AbortXferPacket)packet;
            
            lock (CurrentUploads)
            {
                if (CurrentUploads.ContainsKey(abort.XferID.ID))
                {
                    Logger.DebugLog(String.Format("Aborting Xfer {0}, result: {1}", abort.XferID.ID,
                        (TransferError)abort.XferID.Result));

                    CurrentUploads.Remove(abort.XferID.ID);
                }
                else
                {
                    Logger.DebugLog(String.Format("Received an AbortXfer for an unknown xfer {0}",
                        abort.XferID.ID));
                }
            }
        }

        #endregion Xfer System

        #region Transfer System

        void TransferRequestHandler(Packet packet, Agent agent)
        {
            TransferRequestPacket request = (TransferRequestPacket)packet;

            ChannelType channel = (ChannelType)request.TransferInfo.ChannelType;
            SourceType source = (SourceType)request.TransferInfo.SourceType;

            if (channel == ChannelType.Asset)
            {
                // Construct the response packet
                TransferInfoPacket response = new TransferInfoPacket();
                response.TransferInfo = new TransferInfoPacket.TransferInfoBlock();
                response.TransferInfo.TransferID = request.TransferInfo.TransferID;

                if (source == SourceType.Asset)
                {
                    // Parse the request
                    UUID assetID = new UUID(request.TransferInfo.Params, 0);
                    AssetType type = (AssetType)(sbyte)Utils.BytesToInt(request.TransferInfo.Params, 16);

                    // Set the response channel type
                    response.TransferInfo.ChannelType = (int)ChannelType.Asset;

                    // Params
                    response.TransferInfo.Params = new byte[20];
                    Buffer.BlockCopy(assetID.GetBytes(), 0, response.TransferInfo.Params, 0, 16);
                    Buffer.BlockCopy(Utils.IntToBytes((int)type), 0, response.TransferInfo.Params, 16, 4);

                    // Check if we have this asset
                    Asset asset;
                    if (AssetStore.TryGetValue(assetID, out asset))
                    {
                        if (asset.AssetType == type)
                        {
                            Logger.DebugLog(String.Format("Transferring asset {0} ({1})", asset.AssetID, asset.AssetType));

                            // Asset found
                            response.TransferInfo.Size = asset.AssetData.Length;
                            response.TransferInfo.Status = (int)StatusCode.OK;
                            response.TransferInfo.TargetType = (int)TargetType.Unknown; // Doesn't seem to be used by the client

                            server.UDP.SendPacket(agent.AgentID, response, PacketCategory.Asset);

                            // Transfer system does not wait for ACKs, just sends all of the
                            // packets for this transfer out
                            const int MAX_CHUNK_SIZE = Settings.MAX_PACKET_SIZE - 100;
                            int processedLength = 0;
                            int packetNum = 0;
                            while (processedLength < asset.AssetData.Length)
                            {
                                TransferPacketPacket transfer = new TransferPacketPacket();
                                transfer.TransferData.ChannelType = (int)ChannelType.Asset;
                                transfer.TransferData.TransferID = request.TransferInfo.TransferID;
                                transfer.TransferData.Packet = packetNum++;

                                int chunkSize = Math.Min(asset.AssetData.Length - processedLength, MAX_CHUNK_SIZE);
                                transfer.TransferData.Data = new byte[chunkSize];
                                Buffer.BlockCopy(asset.AssetData, processedLength, transfer.TransferData.Data, 0, chunkSize);
                                processedLength += chunkSize;

                                if (processedLength >= asset.AssetData.Length)
                                    transfer.TransferData.Status = (int)StatusCode.Done;
                                else
                                    transfer.TransferData.Status = (int)StatusCode.OK;

                                server.UDP.SendPacket(agent.AgentID, transfer, PacketCategory.Asset);
                            }
                        }
                        else
                        {
                            Logger.Log(String.Format(
                                "Request for asset {0} with type {1} does not match actual asset type {2}",
                                assetID, type, asset.AssetType), Helpers.LogLevel.Warning);
                        }
                    }
                    else
                    {
                        Logger.Log(String.Format("Request for missing asset {0} with type {1}",
                            assetID, type), Helpers.LogLevel.Warning);

                        // Asset not found
                        response.TransferInfo.Size = 0;
                        response.TransferInfo.Status = (int)StatusCode.UnknownSource;
                        response.TransferInfo.TargetType = (int)TargetType.Unknown;

                        server.UDP.SendPacket(agent.AgentID, response, PacketCategory.Asset);
                    }
                }
                else if (source == SourceType.SimEstate)
                {
                    UUID agentID = new UUID(request.TransferInfo.Params, 0);
                    UUID sessionID = new UUID(request.TransferInfo.Params, 16);
                    EstateAssetType type = (EstateAssetType)Utils.BytesToInt(request.TransferInfo.Params, 32);

                    Logger.Log("Please implement estate asset transfers", Helpers.LogLevel.Warning);
                }
                else if (source == SourceType.SimInventoryItem)
                {
                    UUID agentID = new UUID(request.TransferInfo.Params, 0);
                    UUID sessionID = new UUID(request.TransferInfo.Params, 16);
                    UUID ownerID = new UUID(request.TransferInfo.Params, 32);
                    UUID taskID = new UUID(request.TransferInfo.Params, 48);
                    UUID itemID = new UUID(request.TransferInfo.Params, 64);
                    UUID assetID = new UUID(request.TransferInfo.Params, 80);
                    AssetType type = (AssetType)(sbyte)Utils.BytesToInt(request.TransferInfo.Params, 96);

                    if (taskID != UUID.Zero)
                    {
                        // Task (prim) inventory request
                        Logger.Log("Please implement task inventory transfers", Helpers.LogLevel.Warning);
                    }
                    else
                    {
                        // Agent inventory request
                        Logger.Log("Please implement agent inventory transfer", Helpers.LogLevel.Warning);
                    }
                }
                else
                {
                    Logger.Log(String.Format(
                        "Received a TransferRequest that we don't know how to handle. Channel: {0}, Source: {1}",
                        channel, source), Helpers.LogLevel.Warning);
                }
            }
            else
            {
                Logger.Log(String.Format(
                    "Received a TransferRequest that we don't know how to handle. Channel: {0}, Source: {1}",
                    channel, source), Helpers.LogLevel.Warning);
            }
        }

        #endregion Transfer System

        void SaveAsset(Asset asset)
        {
            try
            {
                File.WriteAllBytes(Path.Combine(UploadDir, String.Format("{0}.{1}", asset.AssetID,
                    asset.AssetType.ToString().ToLower())), asset.AssetData);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Helpers.LogLevel.Warning, ex);
            }
        }

        Asset CreateAsset(AssetType type, UUID assetID, byte[] data)
        {
            switch (type)
            {
                case AssetType.Bodypart:
                    return new AssetBodypart(assetID, data);
                case AssetType.Clothing:
                    return new AssetClothing(assetID, data);
                case AssetType.LSLBytecode:
                    return new AssetScriptBinary(assetID, data);
                case AssetType.LSLText:
                    return new AssetScriptText(assetID, data);
                case AssetType.Notecard:
                    return new AssetNotecard(assetID, data);
                case AssetType.Texture:
                    return new AssetTexture(assetID, data);
                case AssetType.Animation:
                    return new AssetAnimation(assetID, data);
                case AssetType.CallingCard:
                case AssetType.Folder:
                case AssetType.Gesture:
                case AssetType.ImageJPEG:
                case AssetType.ImageTGA:
                case AssetType.Landmark:
                case AssetType.LostAndFoundFolder:
                case AssetType.Object:
                case AssetType.RootFolder:
                case AssetType.Simstate:
                case AssetType.SnapshotFolder:
                case AssetType.Sound:
                    return new AssetSound(assetID, data);
                case AssetType.SoundWAV:
                case AssetType.TextureTGA:
                case AssetType.TrashFolder:
                case AssetType.Unknown:
                default:
                    Logger.Log("Asset type " + type.ToString() + " not implemented!", Helpers.LogLevel.Warning);
                    return null;
            }
        }

        void LoadAssets(string path)
        {
            try
            {
                string[] textures = Directory.GetFiles(path, "*.jp2", SearchOption.TopDirectoryOnly);
                string[] clothing = Directory.GetFiles(path, "*.clothing", SearchOption.TopDirectoryOnly);
                string[] bodyparts = Directory.GetFiles(path, "*.bodypart", SearchOption.TopDirectoryOnly);
                string[] sounds = Directory.GetFiles(path, "*.ogg", SearchOption.TopDirectoryOnly);

                for (int i = 0; i < textures.Length; i++)
                {
                    UUID assetID = ParseUUIDFromFilename(textures[i]);
                    Asset asset = new AssetTexture(assetID, File.ReadAllBytes(textures[i]));
                    asset.Temporary = true;
                    StoreAsset(asset);
                }

                for (int i = 0; i < clothing.Length; i++)
                {
                    UUID assetID = ParseUUIDFromFilename(clothing[i]);
                    Asset asset = new AssetClothing(assetID, File.ReadAllBytes(clothing[i]));
                    asset.Temporary = true;
                    StoreAsset(asset);
                }

                for (int i = 0; i < bodyparts.Length; i++)
                {
                    UUID assetID = ParseUUIDFromFilename(bodyparts[i]);
                    Asset asset = new AssetBodypart(assetID, File.ReadAllBytes(bodyparts[i]));
                    asset.Temporary = true;
                    StoreAsset(asset);
                }

                for (int i = 0; i < sounds.Length; i++)
                {
                    UUID assetID = ParseUUIDFromFilename(sounds[i]);
                    Asset asset = new AssetSound(assetID, File.ReadAllBytes(sounds[i]));
                    asset.Temporary = true;
                    StoreAsset(asset);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Helpers.LogLevel.Warning, ex);
            }
        }

        static UUID ParseUUIDFromFilename(string filename)
        {
            int dot = filename.LastIndexOf('.');

            if (dot > 35)
            {
                // Grab the last 36 characters of the filename
                string uuidString = filename.Substring(dot - 36, 36);
                UUID uuid;
                UUID.TryParse(uuidString, out uuid);
                return uuid;
            }
            else
            {
                return UUID.Zero;
            }
        }
    }
}
