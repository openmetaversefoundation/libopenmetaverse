using OpenMetaverse;
using OpenMetaverse.Imaging;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Simian.Extensions
{
    public class ImageDelivery : ISimianExtension
    {
        Simian Server;
        Bitmap DefaultImage;

        public ImageDelivery(Simian server)
        {
            Server = server;
        }

        public void Start()
        {
            Server.UDPServer.RegisterPacketCallback(PacketType.RequestImage, new UDPServer.PacketCallback(RequestImageHandler));

            if (DefaultImage != null) DefaultImage = null;
            DefaultImage = new Bitmap(32, 32);
            Graphics gfx = Graphics.FromImage(DefaultImage);
            gfx.Clear(Color.White);
            gfx.FillRectangles(Brushes.LightGray, new Rectangle[] { new Rectangle(16, 16, 16, 16), new Rectangle(0, 0, 16, 16) });
            gfx.DrawImage(DefaultImage, 0, 0, 32, 32);  
        }

        public void Stop()
        {
        }

        void RequestImageHandler(Packet packet, Agent agent)
        {
            RequestImagePacket request = (RequestImagePacket)packet;

            foreach (RequestImagePacket.RequestImageBlock block in request.RequestImage)
            {
                //ImageNotInDatabasePacket missing = new ImageNotInDatabasePacket();
                //missing.ImageID.ID = block.Image;
                //agent.SendPacket(missing);

                ImageDataPacket imageData = new ImageDataPacket();
                imageData.ImageData.Data = OpenJPEG.EncodeFromImage(DefaultImage, true);
                imageData.ImageID.ID = block.Image;
                imageData.ImageID.Codec = 1;
                imageData.ImageID.Packets = 1;
                imageData.ImageID.Size = (uint)imageData.ImageData.Data.Length;

                agent.SendPacket(imageData);
            }
        }

    }
}
