using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using libsecondlife;

namespace libsecondlife.TestClient
{
    public class DeleteFolderCommand : Command
    {
		public DeleteFolderCommand(TestClient testClient)
        {
            Name = "deleteFolder";
            Description = "Deletes a folder from inventory.";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            if (args.Length != 1)
                return "Usage: deletefolder UUID";

            LLUUID target;

            if (LLUUID.TryParse(args[0], out target))
            {
                Client.Inventory.RemoveFolder(target);
                return String.Format("removed folder {0}", target);
            } else
            return "Did not delete folder, check uuid with inventory command";
		}
	}
}
