using System;
using System.Collections.Generic;
using System.Threading;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    public class PeriscopeImageDelivery
    {
        public TexturePipeline Pipeline;

        Simian server;
        GridClient client;
        Dictionary<UUID, ImageDownload> currentDownloads = new Dictionary<UUID, ImageDownload>();

        public PeriscopeImageDelivery(Simian server, GridClient client)
        {
            this.server = server;
            this.client = client;

            Pipeline = new TexturePipeline(client, 12);
            Pipeline.OnDownloadFinished += new TexturePipeline.DownloadFinishedCallback(pipeline_OnDownloadFinished);

            server.UDP.RegisterPacketCallback(PacketType.RequestImage, RequestImageHandler);
        }

        public void Stop()
        {
            Pipeline.Shutdown();
        }

        void RequestImageHandler(Packet packet, Agent agent)
        {
            RequestImagePacket request = (RequestImagePacket)packet;

            for (int i = 0; i < request.RequestImage.Length; i++)
            {
                RequestImagePacket.RequestImageBlock block = request.RequestImage[i];

                ImageDownload download;
                bool downloadFound = currentDownloads.TryGetValue(block.Image, out download);

                if (downloadFound)
                {
                    lock (download)
                    {
                        if (block.DiscardLevel == -1 && block.DownloadPriority == 0.0f)
                            Logger.DebugLog(String.Format("Image download {0} is aborting", block.Image));

                        // Update download
                        download.Update(block.DiscardLevel, block.DownloadPriority, (int)block.Packet);
                    }
                }
                else if (block.DiscardLevel == -1 && block.DownloadPriority == 0.0f)
                {
                    // Aborting a download we are not tracking, this may be in the pipeline
                    Pipeline.AbortDownload(block.Image);
                }
                else
                {
                    bool bake = ((ImageType)block.Type == ImageType.Baked);

                    // New download, check if we have this image
                    Asset asset;
                    if (server.Assets.TryGetAsset(block.Image, out asset) && asset is AssetTexture)
                    {
                        SendTexture(agent, (AssetTexture)asset, block.DiscardLevel, (int)block.Packet, block.DownloadPriority);
                    }
                    else
                    {
                        // We don't have this texture, add it to the download queue and see if the bot can get it for us
                        download = new ImageDownload(null, agent, block.DiscardLevel, block.DownloadPriority, (int)block.Packet);
                        lock (currentDownloads)
                            currentDownloads[block.Image] = download;

                        Pipeline.RequestTexture(block.Image, (ImageType)block.Type);
                    }
                }
            }
        }

        void pipeline_OnDownloadFinished(UUID id, bool success)
        {
            ImageDownload download;
            if (currentDownloads.TryGetValue(id, out download))
            {
                lock (currentDownloads)
                    currentDownloads.Remove(id);

                if (success)
                {
                    // Set the texture to the downloaded texture data
                    AssetTexture texture = new AssetTexture(id, Pipeline.GetTextureToRender(id).AssetData);
                    download.Texture = texture;

                    Pipeline.RemoveFromPipeline(id);

                    // Store this texture in the local asset store for later
                    server.Assets.StoreAsset(texture);

                    SendTexture(download.Agent, download.Texture, download.DiscardLevel, download.CurrentPacket, download.Priority);
                }
                else
                {
                    Logger.Log("[Periscope] Failed to download texture " + id.ToString(), Helpers.LogLevel.Warning);

                    ImageNotInDatabasePacket notfound = new ImageNotInDatabasePacket();
                    notfound.ImageID.ID = id;
                    server.UDP.SendPacket(download.Agent.AgentID, notfound, PacketCategory.Texture);
                }
            }
            else
            {
                Logger.Log("[Periscope] Pipeline downloaded a texture we're not tracking, " + id.ToString(), Helpers.LogLevel.Warning);
            }
        }

        void SendTexture(Agent agent, AssetTexture texture, int discardLevel, int packet, float priority)
        {
            ImageDownload download = new ImageDownload(texture, agent, discardLevel, priority, packet);

            Logger.DebugLog(String.Format(
                "[Periscope] Starting new texture transfer for {0}, DiscardLevel: {1}, Priority: {2}, Start: {3}, End: {4}, Total: {5}",
                texture.AssetID, discardLevel, priority, download.CurrentPacket, download.StopPacket, download.TexturePacketCount()));

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
                lock (currentDownloads)
                    currentDownloads[texture.AssetID] = download;

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
                                transfer.ImageID.ID = texture.AssetID;
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

                        Logger.DebugLog("Completed image transfer for " + texture.AssetID.ToString());

                        // Transfer is complete, remove the reference
                        lock (currentDownloads)
                            currentDownloads.Remove(texture.AssetID);
                    }
                );
            }
        }
    }
}
