using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Threading;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class ExportCommand : Command
    {
        SecondLife Client;
        ManualResetEvent GotPermissionsEvent = new ManualResetEvent(false);
        ObjectProperties Properties = null;
        bool GotPermissions = false;

        public ExportCommand(TestClient testClient)
        {
            TestClient = testClient;
            Client = (SecondLife)TestClient;
            Client.Objects.OnObjectProperties += new ObjectManager.ObjectPropertiesFamilyCallback(Objects_OnObjectProperties);

            Name = "export";
            Description = "Exports an object to an xml file. Usage: export uuid outputfile.xml";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
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
                // Check for export permission first
                Client.Objects.RequestObjectPropertiesFamily(Client.Network.CurrentSim, id);
                GotPermissionsEvent.WaitOne(5000, false);

                if (!GotPermissions)
                {
                    return "Couldn't fetch permissions for the requested object, try again";
                }
                else 
                {
                    GotPermissions = false;

                    if (Properties == null)
                    {
                        return "Null object properties returned, may be a bug. Try again";
                    }

                    if (Properties.OwnerID != Client.Network.AgentID)
                    {
                        // We need a MasterID field, those exports should be allowed as well
                        return "That object is owned by " + Properties.OwnerID + ", we don't have permission " +
                            "to export it";
                    }
                }

                try
                {
					XmlWriterSettings settings = new XmlWriterSettings();
					settings.Indent = true;
                    XmlWriter writer = XmlWriter.Create(file, settings);

					try
					{
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
					}
					finally
					{
						writer.Close();
					}
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

        void Objects_OnObjectProperties(Simulator simulator, ObjectProperties properties)
        {
            Properties = properties;
            GotPermissions = true;
            GotPermissionsEvent.Set();
        }
    }
}
