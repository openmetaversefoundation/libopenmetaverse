using System;
using System.Collections.Generic;
using System.Text;

namespace OpenMetaverse.TestClient.Commands.Inventory.Shell
{
    class GiveItemCommand : Command
    {
        private InventoryManager Manager;
        private OpenMetaverse.Inventory Inventory;
        public GiveItemCommand(TestClient client)
        {
            Name = "give";
            Description = "Gives items from the current working directory to an avatar.";
            Category = CommandCategory.Inventory;
        }
        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length < 2)
            {
                return "Usage: give <agent uuid> <item1> [item2] [item3] [...]";
            }
            UUID dest;
            if (!UUID.TryParse(args[0], out dest))
            {
                return "First argument expected agent UUID.";
            }
            Manager = Client.Inventory;
            Inventory = Manager.Store;
            string ret = "";
            string nl = "\n";
            for (int i = 1; i < args.Length; ++i)
            {
                string inventoryName = args[i];
                // WARNING: Uses local copy of inventory contents, need to download them first.
                List<InventoryBase> contents = Inventory.GetContents(Client.CurrentDirectory);
                bool found = false;
                foreach (InventoryBase b in contents)
                {
                    if (inventoryName == b.Name || inventoryName == b.UUID.ToString())
                    {
                        found = true;
                        if (b is InventoryItem)
                        {
                            InventoryItem item = b as InventoryItem;
                            Manager.GiveItem(item.UUID, item.Name, item.AssetType, dest, true);
                            ret += "Gave " + item.Name + nl;
                        }
                        else
                        {
                            ret += "Unable to give folder " + b.Name + nl;
                        }
                    }
                }
                if (!found)
                    ret += "No inventory item named " + inventoryName + " found." + nl;
            }
            return ret;
        }
    }
}
