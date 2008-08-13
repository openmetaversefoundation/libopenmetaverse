using System;
using OpenMetaverse;
using System.Collections.Generic;

namespace OpenMetaverse.TestClient
{
    public class WearCommand : Command
    {
		public WearCommand(TestClient testClient)
        {
            Client = testClient;
            Name = "wear";
            Description = "Wear an outfit folder from inventory. Usage: wear [outfit name] [nobake]";
            Category = CommandCategory.Appearance;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length < 1)
                return "Usage: wear [outfit name] [nobake] eg: 'wear \"/Clothing/My Outfit\" [nobake]'";

            string target = String.Empty;
            bool bake = true;

            for (int ct = 0; ct < args.Length; ct++)
            {
                if (args[ct].Equals("nobake"))
                    bake = false;
                else
                    target = target + args[ct] + " ";
            }

            List<InventoryBase> results = Client.InventoryStore.InventoryFromPath(target, Client.CurrentDirectory, true);
            if (results.Count == 0 || !(results[0] is InventoryFolder))
                return "Unable to find folder at " + target;

            try
            {
                Client.Appearance.WearOutfit(results[0] as InventoryFolder, bake);
            }
            catch (InvalidOutfitException ex)
            {
                return "Invalid outfit (" + ex.Message + ")";
            }

            return String.Empty;
        }
    }
}
