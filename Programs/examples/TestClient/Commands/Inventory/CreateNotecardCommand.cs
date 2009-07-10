using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Assets;

namespace OpenMetaverse.TestClient
{
    public class CreateNotecardCommand : Command
    {
        const int NOTECARD_CREATE_TIMEOUT = 10 * 1000;
        const int NOTECARD_FETCH_TIMEOUT = 10 * 1000;

        public CreateNotecardCommand(TestClient testClient)
        {
            Name = "createnotecard";
            Description = "Creates a notecard from a local text file.";
            Category = CommandCategory.Inventory;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length < 1)
                return "Usage: createnotecard filename.txt";

            UUID notecardItemID = UUID.Zero, notecardAssetID = UUID.Zero;
            bool success = false;
            string message = String.Empty;
            AutoResetEvent notecardEvent = new AutoResetEvent(false);

            #region File Loading

            string file = String.Empty;
            for (int ct = 0; ct < args.Length; ct++)
                file = file + args[ct] + " ";
            file = file.TrimEnd();

            if (!File.Exists(file))
                return String.Format("Filename '{0}' does not exist", file);

            StreamReader reader = new StreamReader(file);
            string body = reader.ReadToEnd();

            #endregion File Loading

            // Notecard creation
            AssetNotecard notecard = new AssetNotecard();
            notecard.BodyText = body;
            notecard.Encode();

            Client.Inventory.RequestCreateItem(Client.Inventory.FindFolderForType(AssetType.Notecard),
                file, file + " created by OpenMetaverse TestClient " + DateTime.Now, AssetType.Notecard,
                UUID.Random(), InventoryType.Notecard, PermissionMask.All,
                delegate(bool createSuccess, InventoryItem item)
                {
                    if (createSuccess)
                    {
                        Client.Inventory.RequestUploadNotecardAsset(notecard.AssetData, item.UUID,
                            delegate(bool uploadSuccess, string status, UUID itemID, UUID assetID)
                            {
                                notecardItemID = itemID;
                                notecardAssetID = assetID;
                                success = uploadSuccess;
                                message = status ?? "Unknown error uploading notecard asset";
                                notecardEvent.Set();
                            });
                    }
                    else
                    {
                        message = "Notecard item creation failed";
                        notecardEvent.Set();
                    }
                }
            );

            notecardEvent.WaitOne(NOTECARD_CREATE_TIMEOUT, false);

            if (success)
            {
                Logger.Log("Notecard successfully created, ItemID " + notecardItemID + " AssetID " + notecardAssetID, Helpers.LogLevel.Info);
                return DownloadNotecard(notecardItemID, notecardAssetID);
            }
            else
                return "Notecard creation failed: " + message;
        }

        string DownloadNotecard(UUID itemID, UUID assetID)
        {
            UUID transferID = UUID.Zero;
            AutoResetEvent assetDownloadEvent = new AutoResetEvent(false);
            byte[] notecardData = null;
            string error = "Timeout";

            AssetManager.AssetReceivedCallback assetCallback =
                delegate(AssetDownload transfer, Asset asset)
                {
                    if (transfer.ID == transferID)
                    {
                        if (transfer.Success)
                            notecardData = transfer.AssetData;
                        else
                            error = transfer.Status.ToString();
                        assetDownloadEvent.Set();
                    }
                };

            Client.Assets.OnAssetReceived += assetCallback;

            transferID = Client.Assets.RequestInventoryAsset(assetID, itemID, UUID.Zero, Client.Self.AgentID, AssetType.Notecard, true);

            assetDownloadEvent.WaitOne(NOTECARD_FETCH_TIMEOUT, false);

            Client.Assets.OnAssetReceived -= assetCallback;

            if (notecardData != null)
                return Encoding.UTF8.GetString(notecardData);
            else
                return "Error downloading notecard asset: " + error;
        }
    }
}