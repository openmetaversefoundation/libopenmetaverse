?using System;
using System.IO;

namespace OpenMetaverse.TestClient
{
    /// <summary>
    /// Example of how to put a new script in your inventory
    /// </summary>
    public class UploadScriptCommand : Command
    {
        /// <summary>
        ///  The default constructor for TestClient commands
        /// </summary>
        /// <param name="testClient"></param>
        public UploadScriptCommand(TestClient testClient)
        {
            Name = "uploadscript";
            Description = "Upload a local .lsl file file into your inventory.";
            Category = CommandCategory.Inventory;
        }

        /// <summary>
        /// The default override for TestClient commands
        /// </summary>
        /// <param name="args"></param>
        /// <param name="fromAgentID"></param>
        /// <returns></returns>
        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length < 1)
                return "Usage: uploadscript filename.lsl";

            string file = String.Empty;
            for (int ct = 0; ct < args.Length; ct++)
                file = String.Format("{0}{1} ", file, args[ct]);
            file = file.TrimEnd();

            if (!File.Exists(file))
                return String.Format("Filename '{0}' does not exist", file);

            string ret = String.Format("Filename: {0}", file);

            try
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    string body = reader.ReadToEnd();
                    string desc = String.Format("{0} created by OpenMetaverse BotClient {1}", file, DateTime.Now);
                    // create the asset
                    Client.Inventory.RequestCreateItem(Client.Inventory.FindFolderForType(AssetType.LSLText), file, desc, AssetType.LSLText, UUID.Random(), InventoryType.LSL, PermissionMask.All,
                    delegate(bool success, InventoryItem item)
                    {
                        if (success)
                            // upload the asset
                            Client.Inventory.RequestUpdateScriptAgentInventory(EncodeScript(body), item.UUID, new InventoryManager.ScriptUpdatedCallback(delegate(bool success1, string status, UUID itemid, UUID assetid)
                            {
                                if (success1)
                                    ret += String.Format("Script successfully uploaded, ItemID {0} AssetID {1}", itemid, assetid);
                            }));
                    });
                }
                return ret;

            }
            catch (System.Exception e)
            {
                Logger.Log(e.ToString(), Helpers.LogLevel.Error, Client);
                return String.Format("Error creating script for {0}", ret);
            }
        }
        /// <summary>
        /// Encodes the script text for uploading
        /// </summary>
        /// <param name="body"></param>
        public static byte[] EncodeScript(string body)
        {
            // Assume this is a string, add 1 for the null terminator ?
            byte[] stringBytes = System.Text.Encoding.UTF8.GetBytes(body);
            byte[] assetData = new byte[stringBytes.Length]; //+ 1];
            Array.Copy(stringBytes, 0, assetData, 0, stringBytes.Length);
            return assetData;
        }
    }

}
