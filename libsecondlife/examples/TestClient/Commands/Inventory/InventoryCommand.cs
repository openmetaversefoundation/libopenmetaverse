using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class InventoryCommand : Command
    {
        private Inventory Inventory;
        private InventoryManager Manager;
        
		public InventoryCommand(TestClient testClient)
        {
            Name = "i";
            Description = "Prints out inventory.";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            Manager = Client.Inventory;
            Inventory = Manager.Store;

            StringBuilder result = new StringBuilder();

            //Client.Inventory.RequestFolderContents(Client.Inventory.Store.RootFolder.UUID, Client.Self.AgentID,
            //    true, true, InventorySortOrder.ByName);

            //PrintFolder(Inventory.RootNode, result, 0);

            //return result.ToString();

            //FIXME:
            return "This function needs a blocking InventoryManager.FolderContents() to work";
        }

        void PrintFolder(InventoryNode f, StringBuilder result, int indent)
        {
            foreach ( InventoryNode i in f.Nodes.Values )
            {
                result.Append(i.Data.Name + "\n");

                if ( i.Nodes.Count > 0 )
                    PrintFolder(i, result, indent + 1);
            }
        }

        //void Indent(StringBuilder output, int indenting)
        //{
        //    for (int count = 0; count < indenting; count++)
        //    {
        //        output.Append("  ");
        //    }
        //}
	}
}