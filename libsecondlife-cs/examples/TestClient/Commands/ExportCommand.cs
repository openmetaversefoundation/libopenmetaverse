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
            uint localid = 0;
            int count = 0;
            string file = args[1];

            try
            {
                id = new LLUUID(args[0]);
            }
            catch (Exception)
            {
                return "Usage: export uuid outputfile.xml";
            }
            
            lock (TestClient.SimPrims)
            {
                if (TestClient.SimPrims.ContainsKey(Client.Network.CurrentSim))
                {
                    foreach (PrimObject prim in TestClient.SimPrims[Client.Network.CurrentSim].Values)
                    {
                        if (prim.ID == id)
                        {
                            if (prim.ParentID != 0)
                            {
                                localid = prim.ParentID;
                            }
                            else
                            {
                                localid = prim.LocalID;
                            }

                            break;
                        }
                    }
                }
            }
            
            if (localid != 0)
            {
                try
                {
                    XmlWriter writer = XmlWriter.Create(file);
                    List<PrimObject> prims = new List<PrimObject>();

                    lock (TestClient.SimPrims)
                    {
                        if (TestClient.SimPrims.ContainsKey(Client.Network.CurrentSim))
                        {
                            foreach (PrimObject prim in TestClient.SimPrims[Client.Network.CurrentSim].Values)
                            {
                                if (prim.LocalID == localid || prim.ParentID == localid)
                                {
                                    prims.Add(prim);
                                    count++;
                                }
                            }
                        }
                    }
                    //Serialize it!
                    Helpers.PrimListToXml(prims, writer);
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

                return "Exported " + count + " prims to " + file;
            }
            else
            {
                return "Couldn't find UUID " + id.ToString() + " in the " + 
                    TestClient.SimPrims[Client.Network.CurrentSim].Count + 
                    "objects currently indexed in the current simulator";
            }
        }
    }
}
