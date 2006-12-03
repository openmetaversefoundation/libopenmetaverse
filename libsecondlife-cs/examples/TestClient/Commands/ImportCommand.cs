using System;
using System.Collections.Generic;
using libsecondlife;
using libsecondlife.Packets;
using System.Xml;
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
            if (args.Length != 1)
                return "Usage: import inputfile.xml";
            string name = args[0];
            try
            {
                XmlReader reader = XmlReader.Create(name);
                List<PrimObject> prims = Helpers.PrimListFromXml(reader);
                reader.Close();
            }
            catch (Exception ex)
            {
                return "Deserialize failed: " + ex.ToString();
            }
            // deserialization done, just need to code to rez and link.
            return "Deserialized, rez code missing.";
        }
    }
}
