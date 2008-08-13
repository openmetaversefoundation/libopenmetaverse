using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace OpenMetaverse.TestClient.Commands.Inventory.Shell
{
    public class ChangeDirectoryCommand : Command
    {
        private InventoryManager Manager;
        private OpenMetaverse.Inventory Inventory;

        public ChangeDirectoryCommand(TestClient client)
        {
            Name = "cd";
            Description = "Changes the current working inventory folder.";
            Category = CommandCategory.Inventory;
        }
        public override string Execute(string[] args, UUID fromAgentID)
        {
            Manager = Client.Inventory;
            Inventory = Client.InventoryStore;

            if (args.Length == 0)
                return "Current folder: " + Client.CurrentDirectory.Name;

            string path = args[0];
            for(int i = 1; i < args.Length; ++i)
            {
                path += " " + args[i];
            }

            List<InventoryBase> results = Inventory.InventoryFromPath(path, Client.CurrentDirectory, true);
            if (results.Count == 0)
                return "Can not find inventory at: " + path;
            InventoryFolder destFolder = null;
            foreach (InventoryBase ib in results)
            {
                if (ib is InventoryFolder) 
                {
                    destFolder = ib as InventoryFolder;
                    break;
                }
            }
            if (destFolder == null)
                return path + " is not a folder.";

            Client.CurrentDirectory = destFolder;
            return "Current folder: " + Client.CurrentDirectory.Name;
        }
    }
}
