using System;
using System.IO;
using System.Threading;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class DownloadTextureCommand : Command
    {
        Guid TextureID;
        AutoResetEvent DownloadHandle = new AutoResetEvent(false);
        ImageDownload Image;
        AssetTexture Asset;

        public DownloadTextureCommand(TestClient testClient)
        {
            Name = "downloadtexture";
            Description = "Downloads the specified texture. " +
                "Usage: downloadtexture [texture-Guid] [discardlevel]";
            Category = CommandCategory.Inventory;

            testClient.Assets.OnImageReceiveProgress += new AssetManager.ImageReceiveProgressCallback(Assets_OnImageReceiveProgress);
            testClient.Assets.OnImageReceived += new AssetManager.ImageReceivedCallback(Assets_OnImageReceived);
        }

        public override string Execute(string[] args, Guid fromAgentID)
        {
            if (args.Length != 1 && args.Length != 2)
                return "Usage: downloadtexture [texture-Guid] [discardlevel]";

            TextureID = Guid.Empty;
            DownloadHandle.Reset();
            Image = null;
            Asset = null;

            if (GuidExtensions.TryParse(args[0], out TextureID))
            {
                int discardLevel = 0;

                if (args.Length > 1)
                {
                    if (!Int32.TryParse(args[1], out discardLevel))
                        return "Usage: downloadtexture [texture-Guid] [discardlevel]";
                }

                Client.Assets.RequestImage(TextureID, ImageType.Normal, 1000000.0f, discardLevel, 0);

                if (DownloadHandle.WaitOne(120 * 1000, false))
                {
                    if (Image != null && Image.Success)
                    {
                        if (Asset != null && Asset.Decode())
                        {
                            try { File.WriteAllBytes(Image.ID.ToString() + ".jp2", Asset.AssetData); }
                            catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }

                            return String.Format("Saved {0}.jp2 ({1}x{2})", Image.ID, Asset.Image.Width, Asset.Image.Height);
                        }
                        else
                        {
                            return "Failed to decode texture " + TextureID.ToString();
                        }
                    }
                    else if (Image != null && Image.NotFound)
                    {
                        return "Simulator reported texture not found: " + TextureID.ToString();
                    }
                    else
                    {
                        return "Download failed for texture " + TextureID.ToString();
                    }
                }
                else
                {
                    return "Timed out waiting for texture download";
                }
            }
            else
            {
                return "Usage: downloadtexture [texture-Guid]";
            }
        }

        private void Assets_OnImageReceived(ImageDownload image, AssetTexture asset)
        {
            Image = image;
            Asset = asset;

            DownloadHandle.Set();
        }

        private void Assets_OnImageReceiveProgress(Guid image, int lastPacket, int recieved, int total)
        {
            if (image == TextureID)
                Console.WriteLine(String.Format("Texture {0}: Received {1} / {2} (Packet: {3})", image, recieved, total, lastPacket));
        }
    }
}
