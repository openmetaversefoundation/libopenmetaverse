using System;
using System.IO;
using System.Threading;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class DownloadTextureCommand : Command
    {
        UUID TextureID;
        AutoResetEvent DownloadHandle = new AutoResetEvent(false);
        AssetTexture Asset;
        TextureRequestState resultState;

        public DownloadTextureCommand(TestClient testClient)
        {
            Name = "downloadtexture";
            Description = "Downloads the specified texture. " +
                "Usage: downloadtexture [texture-uuid] [discardlevel]";
            Category = CommandCategory.Inventory;

        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length != 1 && args.Length != 2)
                return "Usage: downloadtexture [texture-uuid] [discardlevel]";

            TextureID = UUID.Zero;
            DownloadHandle.Reset();
            Asset = null;

            if (UUID.TryParse(args[0], out TextureID))
            {
                int discardLevel = 0;

                if (args.Length > 1)
                {
                    if (!Int32.TryParse(args[1], out discardLevel))
                        return "Usage: downloadtexture [texture-uuid] [discardlevel]";
                }

                Client.Assets.RequestImage(TextureID, ImageType.Normal, Assets_OnImageReceived);

                if (DownloadHandle.WaitOne(120 * 1000, false))
                {
                    if (resultState == TextureRequestState.Finished)
                    {
                        if (Asset != null && Asset.Decode())
                        {
                            try { File.WriteAllBytes(Asset.AssetID + ".jp2", Asset.AssetData); }
                            catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }

                            return String.Format("Saved {0}.jp2 ({1}x{2})", Asset.AssetID, Asset.Image.Width, Asset.Image.Height);
                        }
                        else
                        {
                            return "Failed to decode texture " + TextureID.ToString();
                        }
                    }
                    else if (resultState == TextureRequestState.NotFound)
                    {
                        return "Simulator reported texture not found: " + TextureID.ToString();
                    }
                    else
                    {
                        return "Download failed for texture " + TextureID + " " +  resultState;
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

        private void Assets_OnImageReceived(TextureRequestState state, AssetTexture asset)
        {
            resultState = state;
            Asset = asset;

            DownloadHandle.Set();
        }
    }
}
