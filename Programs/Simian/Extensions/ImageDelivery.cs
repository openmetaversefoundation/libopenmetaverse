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
        byte[] DefaultJP2;
        byte[] DefaultBakedJP2;

        public ImageDelivery(Simian server)
        {
            Server = server;
        }

        public void Start()
        {
            Server.UDPServer.RegisterPacketCallback(PacketType.RequestImage, new UDPServer.PacketCallback(RequestImageHandler));

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

            DefaultJP2 = OpenJPEG.Encode(defaultManaged, true);
            DefaultBakedJP2 = OpenJPEG.Encode(defaultBaked, true);
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
                    imageData.ImageData.Data = DefaultBakedJP2;
                }
                else
                {
                    Logger.DebugLog(String.Format("Sending default texture for {0}", block.Image));
                    imageData.ImageData.Data = DefaultJP2;
                }
                imageData.ImageID.Size = (uint)imageData.ImageData.Data.Length;

                agent.SendPacket(imageData);
            }
        }
    }
}
