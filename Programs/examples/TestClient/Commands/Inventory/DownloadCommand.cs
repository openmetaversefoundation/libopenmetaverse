using System;
using System.IO;
using System.Threading;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class DownloadCommand : Command
    {
        Guid AssetID;
        AssetType assetType;
        AutoResetEvent DownloadHandle = new AutoResetEvent(false);
        bool Success;

        public DownloadCommand(TestClient testClient)
        {
            Name = "download";
            Description = "Downloads the specified asset. Usage: download [Guid] [assetType]";
            Category = CommandCategory.Inventory;

            testClient.Assets.OnAssetReceived += new AssetManager.AssetReceivedCallback(Assets_OnAssetReceived);
        }

        public override string Execute(string[] args, Guid fromAgentID)
        {
            if (args.Length != 2)
                return "Usage: download [Guid] [assetType]";

            Success = false;
            AssetID = Guid.Empty;
            assetType = AssetType.Unknown;
            DownloadHandle.Reset();

            if (GuidExtensions.TryParse(args[0], out AssetID))
            {
                int typeInt;
                if (Int32.TryParse(args[1], out typeInt) && typeInt >= 0 && typeInt <= 22)
                {
                    assetType = (AssetType)typeInt;

                    // Start the asset download
                    Client.Assets.RequestAsset(AssetID, assetType, true);

                    if (DownloadHandle.WaitOne(120 * 1000, false))
                    {
                        if (Success)
                            return String.Format("Saved {0}.{1}", AssetID, assetType.ToString().ToLower());
                        else
                            return String.Format("Failed to download asset {0}, perhaps {1} is the incorrect asset type?",
                                AssetID, assetType);
                    }
                    else
                    {
                        return "Timed out waiting for texture download";
                    }
                }
                else
                {
                    return "Usage: download [Guid] [assetType]";
                }
            }
            else
            {
                return "Usage: download [Guid] [assetType]";
            }
        }

        private void Assets_OnAssetReceived(AssetDownload transfer, Asset asset)
        {
            if (transfer.AssetID == AssetID)
            {
                if (transfer.Success)
                {
                    try
                    {
                        File.WriteAllBytes(String.Format("{0}.{1}", AssetID,
                            assetType.ToString().ToLower()), asset.AssetData);
                        Success = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex.Message, Helpers.LogLevel.Error, ex);
                    }
                }

                DownloadHandle.Set();
            }
        }
    }
}
