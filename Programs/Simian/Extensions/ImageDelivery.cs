using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Imaging;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    public struct ImageDownload
    {
        public const int FIRST_IMAGE_PACKET_SIZE = 600;
        public const int IMAGE_PACKET_SIZE = 1000;

        public AssetTexture Texture;
        public int DiscardLevel;
        public float Priority;
        public int Packet;

        public ImageDownload(AssetTexture texture, int discardLevel, float priority)
        {
            Texture = texture;
            DiscardLevel = discardLevel;
            Priority = priority;
            Packet = 0;
        }

        public int GetRemainingBytes()
        {
            return GetEndPosition() - GetBytesSent();
        }

        public int GetPacketCount()
        {
            int length = GetRemainingBytes();
            return ((length - FIRST_IMAGE_PACKET_SIZE + IMAGE_PACKET_SIZE - 1) / IMAGE_PACKET_SIZE) + 1;
        }

        public int GetBytesSent()
        {
            if (Packet < 1)
                return 0;
            else
                return FIRST_IMAGE_PACKET_SIZE + ((Packet - 1) * IMAGE_PACKET_SIZE);
        }

        public int GetEndPosition()
        {
            if (Texture == null || Texture.LayerInfo == null || Texture.AssetData == null)
                throw new InvalidOperationException("Cannot get end position while texture information is null");

            int layerCount = Texture.LayerInfo.Length;
            int requestedLayer = layerCount + DiscardLevel;

            if (requestedLayer == layerCount)
            {
                // No discard, go to the end of the image data
                return Texture.AssetData.Length - 1;
            }
            else if (requestedLayer >= 0 && requestedLayer < layerCount)
            {
                return Texture.LayerInfo[requestedLayer].End;
            }
            else
            {
                Logger.Log(String.Format(
                    "DiscardLevel {0} is out of range for texture {1}, which has {2} decoded layer boundaries",
                    DiscardLevel, Texture.AssetID, Texture.LayerInfo.Length), Helpers.LogLevel.Error);

                return Texture.AssetData.Length - 1;
            }
        }
    }

    public class ImageDelivery : ISimianExtension
    {
        Simian Server;
        AssetTexture defaultJP2;
        AssetTexture defaultBakedJP2;
        Dictionary<UUID, ImageDownload> CurrentDownloads = new Dictionary<UUID, ImageDownload>();
        BlockingQueue<ImageDownload> CurrentDownloadQueue = new BlockingQueue<ImageDownload>();

        public ImageDelivery(Simian server)
        {
            Server = server;
        }

        public void Start()
        {
            Server.UDP.RegisterPacketCallback(PacketType.RequestImage, new PacketCallback(RequestImageHandler));

            // Create default textures for missing images and missing bakes
            Bitmap defaultImage = new Bitmap(32, 32);
            Graphics gfx = Graphics.FromImage(defaultImage);
            gfx.Clear(Color.White);
            gfx.FillRectangles(Brushes.LightGray, new Rectangle[] { new Rectangle(16, 16, 16, 16), new Rectangle(0, 0, 16, 16) });
            gfx.DrawImage(defaultImage, 0, 0, 32, 32);

            ManagedImage defaultManaged = new ManagedImage(defaultImage);

            ManagedImage defaultBaked = new ManagedImage(defaultImage);
            defaultBaked.Channels = ManagedImage.ImageChannels.Color | ManagedImage.ImageChannels.Alpha |
                ManagedImage.ImageChannels.Bump;
            defaultBaked.Alpha = defaultBaked.Red;
            defaultBaked.Bump = defaultBaked.Red;

            defaultJP2 = new AssetTexture(UUID.Zero, OpenJPEG.Encode(defaultManaged, true));
            defaultBakedJP2 = new AssetTexture(UUID.Zero, OpenJPEG.Encode(defaultBaked, true)); 
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
                bool bake = ((ImageType)block.Type == ImageType.Baked);

                // Check if we have this image
                Asset asset;
                ImageDownload download;
                if (Server.Assets.TryGetAsset(block.Image, out asset) && asset is AssetTexture)
                {
                    if (block.DiscardLevel == -1 && block.DownloadPriority == 0.0f)
                    {
                        // FIXME: Cancel download
                        Logger.Log("Canceling image download " + block.Image, Helpers.LogLevel.Info);
                    }
                    else
                    {
                        lock (CurrentDownloads)
                        {
                            if (CurrentDownloads.TryGetValue(block.Image, out download))
                            {
                                // Modifying existing download
                                if (block.DiscardLevel < download.DiscardLevel)
                                {
                                    // FIXME: Do we need to do something here?
                                    Logger.Log(String.Format("Image download {0} is changing from DiscardLevel {1} to {2}",
                                        block.Image, download.DiscardLevel, block.DiscardLevel), Helpers.LogLevel.Info);
                                    download.DiscardLevel = block.DiscardLevel;
                                }

                                download.Priority = block.DownloadPriority;

                                if (block.Packet > 0 && block.Packet < download.Packet)
                                {
                                    Logger.Log(String.Format("Rolling back image download {0} from packet {1} to {2}",
                                        block.Image, download.Packet, block.Packet), Helpers.LogLevel.Warning);
                                    download.Packet = (int)block.Packet;
                                }

                                // Re-insert this download into the dictionary
                                CurrentDownloads[block.Image] = download;
                            }
                            else
                            {
                                // New download
                                download = new ImageDownload((AssetTexture)asset, block.DiscardLevel, block.DownloadPriority);
                                download.Packet = (int)block.Packet;

                                // Send initial data
                                ImageDataPacket data = new ImageDataPacket();
                                data.ImageID.Codec = (byte)ImageCodec.J2C;
                                data.ImageID.ID = download.Texture.AssetID;
                                data.ImageID.Packets = (ushort)download.GetPacketCount();
                                data.ImageID.Size = (uint)download.GetRemainingBytes();
                                // The Linden Lab servers actually prepend two bytes in this data with the
                                // size of the following data. It is redundant and ignored by every client,
                                // so we skip it
                                data.ImageData = new ImageDataPacket.ImageDataBlock();

                                if (data.ImageID.Packets == 1)
                                {
                                    // Single packet image
                                    data.ImageData.Data = new byte[download.Texture.AssetData.Length];
                                    Buffer.BlockCopy(download.Texture.AssetData, download.GetBytesSent(),
                                        data.ImageData.Data, 0, (int)data.ImageID.Size);
                                }
                                else
                                {
                                    // Multi-packet image
                                    data.ImageData.Data = new byte[ImageDownload.FIRST_IMAGE_PACKET_SIZE];
                                    Buffer.BlockCopy(download.Texture.AssetData, download.GetBytesSent(),
                                        data.ImageData.Data, 0, ImageDownload.FIRST_IMAGE_PACKET_SIZE);

                                    // Insert this download into the dictionary
                                    CurrentDownloads[block.Image] = download;

                                    // Send all of the remaining packets
                                    ThreadPool.QueueUserWorkItem(
                                        delegate(object obj)
                                        {
                                            ImagePacketPacket transfer = new ImagePacketPacket();
                                            transfer.ImageID.ID = block.Image;
                                            //transfer.ImageID.Packet = 
                                        }
                                    );
                                }

                                Server.UDP.SendPacket(agent.AgentID, data, PacketCategory.Texture);
                            }
                        }
                    }
                }
                else
                {
                    // TODO: Technically we should return ImageNotInDatabasePacket, but for now return a default texture
                    ImageDataPacket imageData = new ImageDataPacket();
                    imageData.ImageID.ID = block.Image;
                    imageData.ImageID.Codec = 1;
                    imageData.ImageID.Packets = 1;
                    if (bake)
                    {
                        Logger.DebugLog(String.Format("Sending default bake texture for {0}", block.Image));
                        imageData.ImageData.Data = defaultBakedJP2.AssetData;
                    }
                    else
                    {
                        Logger.DebugLog(String.Format("Sending default texture for {0}", block.Image));
                        imageData.ImageData.Data = defaultJP2.AssetData;
                    }
                    imageData.ImageID.Size = (uint)imageData.ImageData.Data.Length;

                    Server.UDP.SendPacket(agent.AgentID, imageData, PacketCategory.Texture);
                }
            }
        }
    }
}
