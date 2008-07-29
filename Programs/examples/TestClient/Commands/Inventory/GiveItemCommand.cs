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
                string inventoryName = args[i];

                if (Client.CurrentDirectory.IsStale)
                {
                    Client.CurrentDirectory.DownloadContents(TimeSpan.FromSeconds(30));
                }

                bool found = false;
                foreach (InventoryBase b in Client.CurrentDirectory) {
                    string name = b.Name;
                    if (inventoryName == name || inventoryName == b.UUID.ToString())
                    {
                        found = true;
                        b.Give(dest, true);
                        ret += "Gave " + name + nl;
                    }
                }
                if (!found)
                    ret += "No inventory item named " + inventoryName + " found." + nl;
            }
            return ret;
        }
    }
}
