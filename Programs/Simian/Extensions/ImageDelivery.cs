using System;
using System.Collections.Generic;
using System.Threading;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Imaging;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    public class ImageDownload
    {
        public const int FIRST_IMAGE_PACKET_SIZE = 600;
        public const int IMAGE_PACKET_SIZE = 1000;

        public AssetTexture Texture;
        public int DiscardLevel;
        public float Priority;
        public int CurrentPacket;
        public int StopPacket;

        public ImageDownload(AssetTexture texture, int discardLevel, float priority, int packet)
        {
            Texture = texture;
            Update(discardLevel, priority, packet);
        }

        /// <summary>
        /// Updates an image transfer with new information and recalculates
        /// offsets
        /// </summary>
        /// <param name="discardLevel">New requested discard level</param>
        /// <param name="priority">New requested priority</param>
        /// <param name="packet">New requested packet offset</param>
        public void Update(int discardLevel, float priority, int packet)
        {
            Priority = priority;
            DiscardLevel = Utils.Clamp(discardLevel, 0, Texture.LayerInfo.Length - 1);
            StopPacket = GetPacketForBytePosition(Texture.LayerInfo[(Texture.LayerInfo.Length - 1) - DiscardLevel].End);
            CurrentPacket = Utils.Clamp(packet, 1, TexturePacketCount());
        }

        /// <summary>
        /// Returns the total number of packets needed to transfer this texture,
        /// including the first packet of size FIRST_IMAGE_PACKET_SIZE
        /// </summary>
        /// <returns>Total number of packets needed to transfer this texture</returns>
        public int TexturePacketCount()
        {
            return ((Texture.AssetData.Length - FIRST_IMAGE_PACKET_SIZE + IMAGE_PACKET_SIZE - 1) / IMAGE_PACKET_SIZE) + 1;
        }

        /// <summary>
        /// Returns the current byte offset for this transfer, calculated from
        /// the CurrentPacket
        /// </summary>
        /// <returns>Current byte offset for this transfer</returns>
        public int CurrentBytePosition()
        {
            return FIRST_IMAGE_PACKET_SIZE + (CurrentPacket - 1) * IMAGE_PACKET_SIZE;
        }

        /// <summary>
        /// Returns the size, in bytes, of the last packet. This will be somewhere
        /// between 1 and IMAGE_PACKET_SIZE bytes
        /// </summary>
        /// <returns>Size of the last packet in the transfer</returns>
        public int LastPacketSize()
        {
            return Texture.AssetData.Length - (FIRST_IMAGE_PACKET_SIZE + ((TexturePacketCount() - 2) * IMAGE_PACKET_SIZE));
        }

        /// <summary>
        /// Find the packet number that contains a given byte position
        /// </summary>
        /// <param name="bytePosition">Byte position</param>
        /// <returns>Packet number that contains the given byte position</returns>
        int GetPacketForBytePosition(int bytePosition)
        {
            return ((bytePosition - FIRST_IMAGE_PACKET_SIZE + IMAGE_PACKET_SIZE - 1) / IMAGE_PACKET_SIZE);
        }
    }

    public class ImageDelivery : IExtension<Simian>
    {
        Simian server;
        Dictionary<UUID, ImageDownload> CurrentDownloads = new Dictionary<UUID, ImageDownload>();

        public ImageDelivery()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;

            server.UDP.RegisterPacketCallback(PacketType.RequestImage, new PacketCallback(RequestImageHandler));
        }

        public void Stop()
        {
        }

        void RequestImageHandler(Packet packet, Agent agent)
        {
            RequestImagePacket request = (RequestImagePacket)packet;

            for (int i = 0; i < request.RequestImage.Length; i++)
            {
                RequestImagePacket.RequestImageBlock block = request.RequestImage[i];

                ImageDownload download;
                bool downloadFound = CurrentDownloads.TryGetValue(block.Image, out download);

                if (downloadFound)
                {
                    lock (download)
                    {
                        if (block.DiscardLevel == -1 && block.DownloadPriority == 0.0f)
                        {
                            Logger.DebugLog(String.Format("Image download {0} is aborting", block.Image));
                        }
                        else
                        {
                            if (block.DiscardLevel < download.DiscardLevel)
                                Logger.DebugLog(String.Format("Image download {0} is changing from DiscardLevel {1} to {2}",
                                    block.Image, download.DiscardLevel, block.DiscardLevel));

                            if (block.DownloadPriority != download.Priority)
                                Logger.DebugLog(String.Format("Image download {0} is changing from Priority {1} to {2}",
                                    block.Image, download.Priority, block.DownloadPriority));

                            if (block.Packet != download.CurrentPacket)
                                Logger.DebugLog(String.Format("Image download {0} is changing from Packet {1} to {2}",
                                    block.Image, download.CurrentPacket, block.Packet));
                        }

                        // Update download
                        download.Update(block.DiscardLevel, block.DownloadPriority, (int)block.Packet);
                    }
                }
                else if (block.DiscardLevel == -1 && block.DownloadPriority == 0.0f)
                {
                    // Aborting a download we are not tracking, ignore
                }
                else
                {
                    bool bake = ((ImageType)block.Type == ImageType.Baked);

                    // New download, check if we have this image
                    Asset asset;
                    if (server.Assets.TryGetAsset(block.Image, out asset) && asset is AssetTexture)
                    {
                        download = new ImageDownload((AssetTexture)asset, block.DiscardLevel, block.DownloadPriority,
                            (int)block.Packet);

                        Logger.DebugLog(String.Format(
                            "Starting new download for {0}, DiscardLevel: {1}, Priority: {2}, Start: {3}, End: {4}, Total: {5}",
                            block.Image, block.DiscardLevel, block.DownloadPriority, download.CurrentPacket, download.StopPacket,
                            download.TexturePacketCount()));

                        // Send initial data
                        ImageDataPacket data = new ImageDataPacket();
                        data.ImageID.Codec = (byte)ImageCodec.J2C;
                        data.ImageID.ID = download.Texture.AssetID;
                        data.ImageID.Packets = (ushort)download.TexturePacketCount();
                        data.ImageID.Size = (uint)download.Texture.AssetData.Length;

                        // The first bytes of the image are always sent in the ImageData packet
                        data.ImageData = new ImageDataPacket.ImageDataBlock();
                        int imageDataSize = (download.Texture.AssetData.Length >= ImageDownload.FIRST_IMAGE_PACKET_SIZE) ?
                            ImageDownload.FIRST_IMAGE_PACKET_SIZE : download.Texture.AssetData.Length;
                        try
                        {
                            data.ImageData.Data = new byte[imageDataSize];
                            Buffer.BlockCopy(download.Texture.AssetData, 0, data.ImageData.Data, 0, imageDataSize);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(String.Format("{0}: imageDataSize={1}", ex.Message, imageDataSize),
                                Helpers.LogLevel.Error);
                        }

                        server.UDP.SendPacket(agent.AgentID, data, PacketCategory.Texture);

                        // Check if ImagePacket packets need to be sent to complete this transfer
                        if (download.CurrentPacket <= download.StopPacket)
                        {
                            // Insert this download into the dictionary
                            lock (CurrentDownloads)
                                CurrentDownloads[block.Image] = download;

                            // Send all of the remaining packets
                            ThreadPool.QueueUserWorkItem(
                                delegate(object obj)
                                {
                                    while (download.CurrentPacket <= download.StopPacket)
                                    {
                                        if (download.Priority == 0.0f && download.DiscardLevel == -1)
                                            break;

                                        lock (download)
                                        {
                                            int imagePacketSize = (download.CurrentPacket == download.TexturePacketCount() - 1) ?
                                                download.LastPacketSize() : ImageDownload.IMAGE_PACKET_SIZE;

                                            ImagePacketPacket transfer = new ImagePacketPacket();
                                            transfer.ImageID.ID = block.Image;
                                            transfer.ImageID.Packet = (ushort)download.CurrentPacket;
                                            transfer.ImageData.Data = new byte[imagePacketSize];

                                            try
                                            {
                                                Buffer.BlockCopy(download.Texture.AssetData, download.CurrentBytePosition(),
                                                    transfer.ImageData.Data, 0, imagePacketSize);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Log(String.Format(
                                                    "{0}: CurrentBytePosition()={1}, AssetData.Length={2} imagePacketSize={3}",
                                                    ex.Message, download.CurrentBytePosition(), download.Texture.AssetData.Length,
                                                    imagePacketSize), Helpers.LogLevel.Error);
                                            }

                                            server.UDP.SendPacket(agent.AgentID, transfer, PacketCategory.Texture);

                                            ++download.CurrentPacket;
                                        }
                                    }

                                    Logger.DebugLog("Completed image transfer for " + block.Image.ToString());

                                    // Transfer is complete, remove the reference
                                    lock (CurrentDownloads)
                                        CurrentDownloads.Remove(block.Image);
                                }
                            );
                        }
                    }
                    else
                    {
                        Logger.Log("Request for a missing texture " + block.Image.ToString(), Helpers.LogLevel.Warning);

                        ImageNotInDatabasePacket notfound = new ImageNotInDatabasePacket();
                        notfound.ImageID.ID = block.Image;
                        server.UDP.SendPacket(agent.AgentID, notfound, PacketCategory.Texture);
                    }
                }
            }
        }
    }
}
