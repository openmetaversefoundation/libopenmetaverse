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

        public override string Execute(string[] args, UUID fromAgentID)
        {
            Manager = Client.Inventory;
            Inventory = Client.InventoryStore;

            StringBuilder result = new StringBuilder();

            InventoryFolder rootFolder = Inventory.RootFolder;
            PrintFolder(rootFolder, result, 0);

            return result.ToString();
        }

        void PrintFolder(InventoryFolder folder, StringBuilder result, int indent)
        {
            folder.DownloadContents(TimeSpan.FromSeconds(10));
            foreach (InventoryBase b in folder)
            {
                if (b is InventoryFolder)
                {
                    result.Append(Print(b as InventoryFolder, indent));
                    PrintFolder(b as InventoryFolder, result, indent + 1);
                }
                else if (b is InventoryItem)
                {
                    result.Append(Print(b as InventoryItem, indent));
                }
            }
        }
        string Print(InventoryItem item, int indent)
        {
            return string.Format("{0}{1} ({2})\n", new String(' ', indent * 2), item.Data.Name, item.UUID);
        }
        string Print(InventoryFolder folder, int indent)
        {
            return string.Format("{0}{1} ({2})\n", new String(' ', indent * 2), folder.Data.Name, folder.UUID);
        }
	}
}
