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
    public class CreateFolderCommand : Command
    {
        public CreateFolderCommand(TestClient testClient)
        {
            Name = "createFolder";
            Description = "creates a folder in inventory.";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            if (args.Length != 1)
                return "Usage: createfolder name";

            string newfolderName = string.Empty;
            for (int i = 0; i < args.Length; i++)
                newfolderName += args[i] + " ";
            newfolderName = newfolderName.TrimEnd();

            LLUUID newFolder = LLUUID.Zero;

            newFolder = Client.Inventory.CreateFolder(Client.Inventory.Store.RootFolder.UUID, newfolderName);
            if (!newFolder.Equals(LLUUID.Zero))
                return String.Format("created folder {0} {1}", newfolderName, newFolder);
            else
                return String.Format("unable to create folder {0}", newfolderName);
        }
    }
}
