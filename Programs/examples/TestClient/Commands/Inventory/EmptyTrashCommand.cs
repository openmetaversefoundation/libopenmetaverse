using System;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class EmptyTrashCommand : Command
    {
        /// <summary>
        /// TestClient command to download and display a notecard asset
        /// </summary>
        /// <param name="testClient"></param>
        public EmptyTrashCommand(TestClient testClient)
        {
            Name = "emptytrash";
            Description = "Empty inventory Trash folder";
            Category = CommandCategory.Inventory;
        }

        /// <summary>
        /// Exectute the command
        /// </summary>
        /// <param name="args"></param>
        /// <param name="fromAgentID"></param>
        /// <returns></returns>
        public override string Execute(string[] args, UUID fromAgentID)
        {
            Client.Inventory.EmptyTrash();
            return "Trash Emptied";
        }
    }
}
