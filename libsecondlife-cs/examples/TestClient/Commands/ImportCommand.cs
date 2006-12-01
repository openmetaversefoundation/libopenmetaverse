using System;
using System.Collections.Generic;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class ImportCommand : Command
    {
        public ImportCommand()
        {
            Name = "import";
            Description = "Import prims from an exported xml file. Usage: import [filename.xml]";
        }

        public override string Execute(SecondLife Client, string[] args, LLUUID fromAgentID)
        {
            // How do we deserialize multiple PrimObject classes from an xml file anyways?
            return "FIXME";
        }
    }
}
