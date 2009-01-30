using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class InventoryCommand : Command
    {
        private Inventory Inventory;
        private InventoryManager Manager;

        public InventoryCommand(TestClient testClient)
        {
            Name = "i";
            Description = "Prints out inventory.";
            Category = CommandCategory.Inventory;
        }

        public override string Execute(string[] args, Guid fromAgentID)
        {
            Manager = Client.Inventory;
            Inventory = Manager.Store;

            StringBuilder result = new StringBuilder();

            InventoryFolder rootFolder = Inventory.RootFolder;
            PrintFolder(rootFolder, result, 0);

            return result.ToString();
        }

        void PrintFolder(InventoryFolder f, StringBuilder result, int indent)
        {
            List<InventoryBase> contents = Manager.FolderContents(f.Guid, Client.Self.AgentID,
                true, true, InventorySortOrder.ByName, 3000);

            if (contents != null)
            {
                foreach (InventoryBase i in contents)
                {
                    result.AppendFormat("{0}{1} ({2})\n", new String(' ', indent * 2), i.Name, i.Guid);
                    if (i is InventoryFolder)
                    {
                        InventoryFolder folder = (InventoryFolder)i;
                        PrintFolder(folder, result, indent + 1);
                    }
                }
            }
        }
    }
}