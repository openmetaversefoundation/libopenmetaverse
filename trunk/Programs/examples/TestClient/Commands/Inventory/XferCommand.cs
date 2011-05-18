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
            byte[] assetData = RequestXfer(assetID, AssetType.Object, out filename);

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

        byte[] RequestXfer(UUID assetID, AssetType type, out string filename)
        {
            AutoResetEvent xferEvent = new AutoResetEvent(false);
            ulong xferID = 0;
            byte[] data = null;

            EventHandler<XferReceivedEventArgs> xferCallback =
                delegate(object sender, XferReceivedEventArgs e)
                {
                    if (e.Xfer.XferID == xferID)
                    {
                        if (e.Xfer.Success)
                            data = e.Xfer.AssetData;
                        xferEvent.Set();
                    }
                };

            Client.Assets.XferReceived += xferCallback;

            filename = assetID + ".asset";
            xferID = Client.Assets.RequestAssetXfer(filename, false, true, assetID, type, false);

            xferEvent.WaitOne(FETCH_ASSET_TIMEOUT, false);

            Client.Assets.XferReceived -= xferCallback;

            return data;
        }
    }
}
