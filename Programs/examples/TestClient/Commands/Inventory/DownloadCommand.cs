using System;
using System.IO;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Assets;

namespace OpenMetaverse.TestClient
{
    public class DownloadCommand : Command
    {
        UUID AssetID;
        AssetType assetType;
        AutoResetEvent DownloadHandle = new AutoResetEvent(false);
        bool Success;
        string usage = "Usage: download [uuid] [assetType]";

        public DownloadCommand(TestClient testClient)
        {
            Name = "download";
            Description = "Downloads the specified asset. Usage: download [uuid] [assetType]";
            Category = CommandCategory.Inventory;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length != 2)
                return usage;

            Success = false;
            AssetID = UUID.Zero;
            assetType = AssetType.Unknown;
            DownloadHandle.Reset();

            if (!UUID.TryParse(args[0], out AssetID))
            	return usage;

			try {
				assetType = (AssetType)Enum.Parse(typeof(AssetType), args[1], ignoreCase: true);
			} catch (ArgumentException) {
				return usage;
			}
			if (!Enum.IsDefined(typeof(AssetType), assetType))
				return usage;

			// Start the asset download
			Client.Assets.RequestAsset(AssetID, assetType, true, Assets_OnAssetReceived);

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
