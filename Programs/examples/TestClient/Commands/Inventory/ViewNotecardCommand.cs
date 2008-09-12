using System;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class ViewNotecardCommand : Command
    {
        /// <summary>
        /// TestClient command to download and display a notecard asset
        /// </summary>
        /// <param name="testClient"></param>
        public ViewNotecardCommand(TestClient testClient)
        {
            Name = "viewnote";
            Description = "Downloads and displays a notecard asset";
            Category = CommandCategory.Inventory;
        }

        /// <summary>
        /// Exectute the command
        /// </summary>
        /// <param name="args"></param>
        /// <param name="fromAgentID"></param>
        /// <returns></returns>
        public override string Execute(string[] args, UUID fromAgentID)
        {

            if (args.Length < 1)
            {
                return "Usage: viewnote [notecard asset uuid]";
            }
            UUID note;
            if (!UUID.TryParse(args[0], out note))
            {
                return "First argument expected agent UUID.";
            }

            System.Threading.AutoResetEvent waitEvent = new System.Threading.AutoResetEvent(false);

            System.Text.StringBuilder result = new System.Text.StringBuilder();


            // define a delegate to handle the reply
            AssetManager.AssetReceivedCallback del = delegate(AssetDownload transfer, Asset asset)
            {
                if (transfer.Success)
                {
                    result.AppendFormat("Raw Notecard Data: " + System.Environment.NewLine + " {0}", Utils.BytesToString(asset.AssetData));
                    waitEvent.Set();
                }
            };

            // verify asset is loaded in store
            if (Client.Inventory.Store.Contains(note))
            {
                // retrieve asset from store
                InventoryItem ii = (InventoryItem)Client.Inventory.Store[note];
                // subscribe to reply event
                Client.Assets.OnAssetReceived += del;

                // make request for asset
                Client.Assets.RequestInventoryAsset(ii, true);

                // wait for reply or timeout
                if (!waitEvent.WaitOne(10000, false))
                {
                    result.Append("Timeout waiting for notecard to download.");
                }
                // unsubscribe from reply event
                Client.Assets.OnAssetReceived -= del;
            }
            else
            {
                result.Append("Cannot find asset in inventory store, use 'i' to populate store");
            }

            // return results
            return result.ToString();
        }
    }
}
