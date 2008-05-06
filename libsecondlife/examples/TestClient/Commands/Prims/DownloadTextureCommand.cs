using System;
using System.IO;
using System.Threading;
using libsecondlife;

namespace libsecondlife.TestClient
{
    public class DownloadTextureCommand : Command
    {
        LLUUID TextureID;
        AutoResetEvent DownloadHandle = new AutoResetEvent(false);
        ImageDownload Image;
        AssetTexture Asset;

        public DownloadTextureCommand(TestClient testClient)
        {
            Name = "downloadtexture";
            Description = "Downloads the specified texture. " +
                "Usage: downloadtexture [texture-uuid]";

            testClient.Assets.OnImageReceiveProgress += new AssetManager.ImageReceiveProgressCallback(Assets_OnImageReceiveProgress);
            testClient.Assets.OnImageReceived += new AssetManager.ImageReceivedCallback(Assets_OnImageReceived);
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            if (args.Length != 1)
                return "Usage: downloadtexture [texture-uuid]";

            TextureID = LLUUID.Zero;
            DownloadHandle.Reset();
            Image = null;
            Asset = null;

            if (LLUUID.TryParse(args[0], out TextureID))
            {
                Client.Assets.RequestImage(TextureID, ImageType.Normal);
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
                return "Usage: downloadtexture [texture-uuid]";
            }
        }

        private void Assets_OnImageReceived(ImageDownload image, AssetTexture asset)
        {
            Image = image;
            Asset = asset;

            DownloadHandle.Set();
        }

        private void Assets_OnImageReceiveProgress(LLUUID image, int recieved, int total)
        {
            Console.WriteLine(String.Format("Texture {0}: Received {1} / {2}", image, recieved, total));
        }
    }
}
