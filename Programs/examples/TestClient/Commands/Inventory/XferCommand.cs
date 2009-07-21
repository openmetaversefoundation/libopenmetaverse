using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace OpenMetaverse.TestClient
{
    public class XferCommand : Command
    {
        const int FETCH_ASSET_TIMEOUT = 1000 * 10;

        public XferCommand(TestClient testClient)
        {
            Name = "xfer";
            Description = "Downloads the specified asset using the Xfer system. Usage: xfer [uuid]";
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            UUID assetID;

            if (args.Length != 1 || !UUID.TryParse(args[0], out assetID))
                return "Usage: xfer [uuid]";

            string filename;
            byte[] assetData = RequestXferPrim(assetID, out filename);

            if (assetData != null)
            {
                try
                {
                    File.WriteAllBytes(filename, assetData);
                    return "Saved asset " + filename;
                }
                catch (Exception ex)
                {
                    return "Failed to save asset " + filename + ": " + ex.Message;
                }
            }
            else
            {
                return "Failed to xfer asset " + assetID;
            }
        }

        byte[] RequestXferPrim(UUID assetID, out string filename)
        {
            AutoResetEvent xferEvent = new AutoResetEvent(false);
            ulong xferID = 0;
            byte[] data = null;

            AssetManager.XferReceivedCallback xferCallback =
                delegate(XferDownload xfer)
                {
                    if (xfer.XferID == xferID)
                    {
                        if (xfer.Success)
                            data = xfer.AssetData;
                        xferEvent.Set();
                    }
                };

            Client.Assets.OnXferReceived += xferCallback;

            filename = assetID + ".asset";
            xferID = Client.Assets.RequestAssetXfer(filename, false, true, assetID, AssetType.Object, false);

            xferEvent.WaitOne(FETCH_ASSET_TIMEOUT, false);

            Client.Assets.OnXferReceived -= xferCallback;

            return data;
        }
    }
}
