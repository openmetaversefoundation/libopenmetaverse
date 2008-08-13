using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using System.Threading;
namespace OpenMetaverse.TestClient.Commands.Inventory.Shell
{
    public class PrintAssetCommand : Command
    {
        public ManualResetEvent completeEvent = new ManualResetEvent(false);
        public byte[] AssetData;
        public AssetType AssetType;
        public UUID AssetID;
        public StatusCode Status;

        public PrintAssetCommand(TestClient client)
        {
            Name = "cat";
            Description = "Prints the inventory item's asset data to the console.";
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length == 0)
                return "Usage cat <item path>";
            string path = args[0];
            for (int i = 1; i < args.Length; ++i)
                path += " " + args[i];

            List<InventoryBase> results = Client.InventoryStore.InventoryFromPath(path, Client.CurrentDirectory, true);
            if (results.Count == 0)
                return "No folder or item at " + path;
            
            InventoryItem item = null;
            foreach (InventoryBase ib in results)
                if (ib is InventoryItem)
                    item = ib as InventoryItem;
            if (item == null)
                return path + " is a directory.";

            InventoryAssetTransferRequest request =
                new InventoryAssetTransferRequest(Client.Network, Client.Self.SessionID,
                    Client.Self.AgentID, item.UUID, item.OwnerUUID, item.Data.AssetType, true);
            request.OnError += new TransferRequest.Error(request_OnError);
            request.OnComplete += new TransferRequest.Complete(request_OnComplete);
            request.OnProgress += new TransferRequest.Progress(request_OnProgress);
            request.Start();
            if (!completeEvent.WaitOne(TimeSpan.FromSeconds(30), false))
                return "Request timed out.";

            if (Status != StatusCode.Done)
            {
                return "An error occured: " + Status.ToString();
            }
            else
            {
                string str = Encoding.UTF8.GetString(AssetData);
                return str;
            }
        }

        void request_OnProgress(TransferRequest request, StatusCode status, int received, int size)
        {
            Console.WriteLine("Progress {0}, {1}/{2}", status, received, size);
        }

        void request_OnComplete(TransferRequest request, AssetType type, UUID assetID, byte[] data)
        {
            AssetType = type;
            AssetID = assetID;
            AssetData = data;
            Status = StatusCode.Done;
            completeEvent.Set();
        }

        void request_OnError(TransferRequest request, StatusCode error)
        {
            Status = error;
            completeEvent.Set();
        }
    }
}
