using System;
using System.Collections.Generic;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class WearCommand : Command
    {
        public WearCommand(TestClient testClient)
        {
            Client = testClient;
            Name = "wear";
            Description = "Wear an outfit folder from inventory. Usage: wear [outfit name]";
            Category = CommandCategory.Appearance;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length < 1)
                return "Usage: wear [outfit name] eg: 'wear Clothing/My Outfit";

            string target = String.Empty;

            for (int ct = 0; ct < args.Length; ct++)
            {
                target += args[ct] + " ";
            }

            target = target.TrimEnd();

            UUID folder = Client.Inventory.FindObjectByPath(Client.Inventory.Store.RootFolder.UUID, Client.Self.AgentID, target, 20 * 1000);

            if (folder == UUID.Zero)
            {
                return "Outfit path " + target + " not found";
            }

            List<InventoryBase> contents =  Client.Inventory.FolderContents(folder, Client.Self.AgentID, true, true, InventorySortOrder.ByName, 20 * 1000);
            List<InventoryItem> items = new List<InventoryItem>();

            if (contents == null)
            {
                return "Failed to get contents of " + target;
            }

            foreach (InventoryBase item in contents)
            {
                if (item is InventoryItem)
                    items.Add((InventoryItem)item);
            }

            Client.Appearance.ReplaceOutfit(items);

            return "Starting to change outfit to " + target;

        }
    }
}
