using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace OpenMetaverse.TestClient.Commands.Inventory.Shell
{
    public class ListContentsCommand : Command
    {
        private InventoryManager Manager;
        private OpenMetaverse.Inventory Inventory;
        public ListContentsCommand(TestClient client)
        {
            Name = "ls";
            Description = "Lists the contents of the current working inventory folder.";
            Category = CommandCategory.Inventory;
        }
        public override string Execute(string[] args, UUID fromAgentID)
        {
            Manager = Client.Inventory;
            Inventory = Client.InventoryStore;
            if (args.Length > 1)
                return "Usage: ls [-l] [directory path]";
            bool longDisplay = false;
            InventoryFolder directory = Client.CurrentDirectory;
            if (args.Length > 0)
            {
                int start = 0;
                if (args[0] == "-l")
                {
                    longDisplay = true;
                    start = 1;
                }
                if (start < args.Length)
                {
                    string path = args[start];
                    for (int i = start + 1; i < args.Length; ++i)
                    {
                        path += " " + args[i];
                    }
                    bool found = false;
                    List<InventoryBase> results = Inventory.InventoryFromPath(path, Client.CurrentDirectory, true);
                    foreach (InventoryBase ib in results)
                    {
                        if (ib is InventoryFolder)
                        {
                            directory = ib as InventoryFolder;
                            found = true;
                        }
                    }
                    if (!found)
                        return "Unable to find directory at path: " + path;
                }
            }

            if (directory.IsStale)
                directory.DownloadContents(TimeSpan.FromSeconds(30));

            string displayString = "";
            string nl = "\n"; // New line character
            // Pretty simple, just print out the contents.
            foreach (InventoryBase b in directory)
            {
                if (longDisplay)
                {
                    // Generate a nicely formatted description of the item.
                    // It kinda looks like the output of the unix ls.
                    // starts with 'd' if the inventory is a folder, '-' if not.
                    // 9 character permissions string
                    // UUID of object
                    // Name of object
                    if (b is InventoryFolder)
                    {
                        InventoryFolder folder = b as InventoryFolder;
                        displayString += "d--------- ";
                        displayString += folder.UUID;
                        displayString += " " + folder.Name;
                    }
                    else if (b is InventoryItem)
                    {
                        InventoryItem item = b as InventoryItem;
                        displayString += "-";
                        displayString += PermMaskString(item.Data.Permissions.OwnerMask);
                        displayString += PermMaskString(item.Data.Permissions.GroupMask);
                        displayString += PermMaskString(item.Data.Permissions.EveryoneMask);
                        displayString += " " + item.UUID;
                        displayString += " " + item.Name;
                    }
                }
                else
                {
                    string name = b.Name;
                    displayString += name;
                }
                displayString += nl;
            }
            return displayString;
        }

        /// <summary>
        /// Returns a 3-character summary of the PermissionMask
        /// CMT if the mask allows copy, mod and transfer
        /// -MT if it disallows copy
        /// --T if it only allows transfer
        /// --- if it disallows everything
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        private static string PermMaskString(PermissionMask mask) {
            string str = "";
            if (((uint)mask | (uint)PermissionMask.Copy) == (uint)PermissionMask.Copy)
                str += "C";
            else
                str += "-";
            if (((uint)mask | (uint)PermissionMask.Modify) == (uint)PermissionMask.Modify)
                str += "M";
            else
                str += "-";
            if (((uint)mask | (uint)PermissionMask.Transfer) == (uint)PermissionMask.Transfer)
                str += "T";
            else
                str += "-";
            return str;
        }
    }
}
