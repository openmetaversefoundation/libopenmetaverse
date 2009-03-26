using System;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class EmptyLostAndCommand : Command
    {
        /// <summary>
        /// TestClient command to download and display a notecard asset
        /// </summary>
        /// <param name="testClient"></param>
        public EmptyLostAndCommand(TestClient testClient)
        {
            Name = "emptylostandfound";
            Description = "Empty inventory Lost And Found folder";
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
            Client.Inventory.EmptyLostAndFound();
            return "Lost And Found Emptied";
        }
    }
}
