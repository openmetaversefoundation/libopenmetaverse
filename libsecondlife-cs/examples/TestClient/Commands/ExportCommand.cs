using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class ExportCommand : Command
    {
        public ExportCommand()
        {
            Name = "export";
            Description = "Exports an object to an xml file. Usage: export uuid outputfile.xml";
        }

        public override string Execute(SecondLife Client, string[] args, LLUUID fromAgentID)
        {
            if (args.Length != 2)
                return "Usage: export uuid outputfile.xml";

            LLUUID id;
            string file = args[1];

            try
            {
                id = new LLUUID(args[0]);
            }
            catch (Exception)
            {
                return "Usage: export uuid outputfile.xml";
            }

            foreach (PrimObject prim in TestClient.Prims.Values)
            {
                if (prim.ID == id)
                {
                    try
                    {
                        XmlWriter writer = XmlWriter.Create(file);
                        prim.ToXml(writer);
                        writer.Close();
                    }
                    catch (Exception e)
                    {
                        string ret = "Failed to write to " + file + ":" + e.ToString();
                        if (ret.Length > 1000)
                        {
                            ret = ret.Remove(1000);
                        }
                        return ret;
                    }

                    return "Exported " + id.ToString() + " to " + file;
                }
            }

            return "Couldn't find UUID " + id.ToString() + " in the " + TestClient.Prims.Count + 
                "objects currently indexed";
        }
    }
}
