using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;

namespace libsecondlife.TestClient.Commands.Inventory.Shell
{
    public class ListContentsCommand : Command
    {
        private InventoryManager Manager;
        private libsecondlife.Inventory Inventory;
        public ListContentsCommand(TestClient client)
        {
            Name = "ls";
            Description = "Lists the contents of the current working inventory folder.";
        }
        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            if (args.Length > 1)
                return "Usage: ls [-l]";
            bool longDisplay = false;
            if (args.Length > 0 && args[0] == "-l")
                longDisplay = true;

            Manager = Client.Inventory;
            Inventory = Manager.Store;
            // WARNING: Uses local copy of inventory contents, need to download them first.
            List<InventoryBase> contents = Inventory.GetContents(Client.CurrentDirectory);
            string displayString = "";
            string nl = "\n"; // New line character
            // Pretty simple, just print out the contents.
            foreach (InventoryBase b in contents)
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
                        displayString += PermMaskString(item.Permissions.OwnerMask);
                        displayString += PermMaskString(item.Permissions.GroupMask);
                        displayString += PermMaskString(item.Permissions.EveryoneMask);
                        displayString += " " + item.UUID;
                        displayString += " " + item.Name;
                    }
                }
                else
                {
                    displayString += b.Name;
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
