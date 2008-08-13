using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    /// <summary>
    /// Inventory Example, Moves a folder to the Trash folder
    /// </summary>
    public class DeleteFolderCommand : Command
    {
		public DeleteFolderCommand(TestClient testClient)
        {
            Name = "deleteFolder";
            Description = "Moves a folder to the Trash Folder";
            Category = CommandCategory.Inventory;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            // parse the command line
            string target = String.Empty;
            for (int ct = 0; ct < args.Length; ct++)
                target = target + args[ct] + " ";

            // initialize results list
            List<InventoryBase> found = new List<InventoryBase>();
            try
            {
                // find the folder

                found = Client.InventoryStore.InventoryFromPath(target, Client.CurrentDirectory, true);
                if (found.Count > 0)
                {
                    InventoryBase item = found[0];
                    InventoryFolder trash = Client.InventoryStore[Client.Inventory.FindFolderForType(AssetType.TrashFolder)] as InventoryFolder;
                    if (trash != null)
                    {
                        item.Move(trash);
                        return String.Format("Moved folder {0} ({1}) to Trash", item.Name, item.UUID);
                    }
                }
                else
                {
                    return String.Format("Unable to locate {0}", target);
                }
            }
            catch (InvalidOutfitException ex)
            {
                return "Folder Not Found: (" + ex.Message + ")";
            }
            return string.Empty;
		}
	}
}
