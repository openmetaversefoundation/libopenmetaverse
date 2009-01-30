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

        public override string Execute(string[] args, Guid fromAgentID)
        {
            if (args.Length != 1)
                return "Usage: objectinventory [objectID]";

            uint objectLocalID;
            Guid objectID;
            if (!GuidExtensions.TryParse(args[0], out objectID))
                return "Usage: objectinventory [objectID]";

            Primitive found = Client.Network.CurrentSim.ObjectsPrimitives.Find(delegate(Primitive prim) { return prim.ID == objectID; });
            if (found != null)
                objectLocalID = found.LocalID;
            else
                return "Couldn't find prim " + objectID.ToString();

            List<InventoryBase> items = Client.Inventory.GetTaskInventory(objectID, objectLocalID, 1000 * 30);

            if (items != null)
            {
                string result = String.Empty;

                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i] is InventoryFolder)
                    {
                        result += String.Format("[Folder] Name: {0}", items[i].Name) + Environment.NewLine;
                    }
                    else
                    {
                        InventoryItem item = (InventoryItem)items[i];
                        result += String.Format("[Item] Name: {0} Desc: {1} Type: {2}", item.Name, item.Description,
                            item.AssetType) + Environment.NewLine;
                    }
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
