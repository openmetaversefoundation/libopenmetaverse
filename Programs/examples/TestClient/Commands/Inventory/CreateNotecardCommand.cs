using System;
using System.Collections.Generic;
using System.IO;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class CreateNotecardCommand : Command
    {
        public CreateNotecardCommand(TestClient testClient)
        {
            Name = "createnotecard";
            Description = "Creates a notecard from a local text file.";
            Category = CommandCategory.Inventory;
        }

        void OnNoteUpdate(bool success, string status, Guid itemID, Guid assetID)
        {
            if (success)
                Console.WriteLine("Notecard successfully uploaded, ItemID {0} AssetID {1}", itemID, assetID);
        }

        public override string Execute(string[] args, Guid fromAgentID)
        {
            if (args.Length < 1)
                return "Usage: createnotecard filename.txt";

            string file = String.Empty;
            for (int ct = 0; ct < args.Length; ct++)
                file = file + args[ct] + " ";
            file = file.TrimEnd();

            Console.WriteLine("Filename: {0}", file);
            if (!File.Exists(file))
                return String.Format("Filename '{0}' does not exist", file);

            System.IO.StreamReader reader = new StreamReader(file);
            string body = reader.ReadToEnd();

            // FIXME: Upload the notecard asset first. When that completes, call RequestCreateItem
            try
            {
                string desc = String.Format("{0} created by OpenMetaverse TestClient {1}", file, DateTime.Now);
                // create the asset

                Client.Inventory.RequestCreateItem(Client.Inventory.FindFolderForType(AssetType.Notecard),
                    file, desc, AssetType.Notecard, Guid.NewGuid(), InventoryType.Notecard, PermissionMask.All,
                    delegate(bool success, InventoryItem item)
                    {
                        if (success) // upload the asset
                            Client.Inventory.RequestUploadNotecardAsset(CreateNotecardAsset(body), item.Guid, new InventoryManager.NotecardUploadedAssetCallback(OnNoteUpdate));
                    }
                );
                return "Done";

            }
            catch (System.Exception e)
            {
                Logger.Log(e.ToString(), Helpers.LogLevel.Error, Client);
                return "Error creating notecard.";
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="body"></param>
        public static byte[] CreateNotecardAsset(string body)
        {
            // Format the string body into Linden text
            string lindenText = "Linden text version 1\n";
            lindenText += "{\n";
            lindenText += "LLEmbeddedItems version 1\n";
            lindenText += "{\n";
            lindenText += "count 0\n";
            lindenText += "}\n";
            lindenText += "Text length " + body.Length + "\n";
            lindenText += body;
            lindenText += "}\n";

            // Assume this is a string, add 1 for the null terminator
            byte[] stringBytes = System.Text.Encoding.UTF8.GetBytes(lindenText);
            byte[] assetData = new byte[stringBytes.Length]; //+ 1];
            Array.Copy(stringBytes, 0, assetData, 0, stringBytes.Length);

            return assetData;
        }
    }
}