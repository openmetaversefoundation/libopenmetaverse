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
            Inventory = Client.InventoryStore;
            string ret = "";
            string nl = "\n";
            for (int i = 1; i < args.Length; ++i)
            {
                string itemPath = args[i];

                List<InventoryBase> results = Inventory.InventoryFromPath(itemPath, Client.CurrentDirectory, true);

                if (results.Count == 0)
                {
                    ret += "No inventory item at " + itemPath + " found." + nl;
                }
                else
                {
                    results[0].Give(dest, true);
                    ret += "Gave " + results[0].Name + nl;
                }
            }
            return ret;
        }
    }
}
