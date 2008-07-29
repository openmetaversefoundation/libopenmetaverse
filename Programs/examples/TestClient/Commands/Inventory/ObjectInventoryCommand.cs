using System;
using System.Collections.Generic;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class ObjectInventoryCommand : Command
    {
        public ObjectInventoryCommand(TestClient testClient)
        {
            Name = "objectinventory";
            Description = "Retrieves a listing of items inside an object (task inventory). Usage: objectinventory [objectID]";
            Category = CommandCategory.Inventory;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length != 1)
                return "Usage: objectinventory [objectID]";

            uint objectLocalID;
            UUID objectID;
            if (!UUID.TryParse(args[0], out objectID))
                return "Usage: objectinventory [objectID]";

            Primitive found = Client.Network.CurrentSim.ObjectsPrimitives.Find(delegate(Primitive prim) { return prim.ID == objectID; });
            if (found != null)
                objectLocalID = found.LocalID;
            else
                return "Couldn't find prim " + objectID.ToString();

            List<ItemData> items;
            List<FolderData> folders;
            Client.Inventory.GetTaskInventory(objectID, objectLocalID, TimeSpan.FromMilliseconds(1000 * 30), out items, out folders);

            if (items != null)
            {
                string result = String.Empty;

                foreach (ItemData item in items)
                {
                    result += String.Format("[Item] Name: {0} Desc: {1} Type: {2}", item.Name, item.Description,
                            item.AssetType) + Environment.NewLine;
                }
                foreach (FolderData folder in folders)
                {
                    result += String.Format("[Folder] Name: {0}", folder.Name) + Environment.NewLine;
                }

                return result;
            }
            else
            {
                return "Failed to download task inventory for " + objectLocalID;
            }
        }
    }
}
