using System;
using System.Collections.Generic;
using System.Drawing;
using OpenMetaverse;
using OpenMetaverse.Imaging;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    public class ImageDelivery : ISimianExtension
    {
        Simian Server;
        AssetTexture defaultJP2;
        AssetTexture defaultBakedJP2;

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
