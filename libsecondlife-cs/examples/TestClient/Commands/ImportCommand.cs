using System;
using System.Collections.Generic;
using libsecondlife;
using libsecondlife.Packets;
using System.Xml;
using System.Threading;
using System.Xml.Serialization;
using System.IO;

namespace libsecondlife.TestClient
{
    public class Linkset
    {
        public PrimObject RootPrim;
        public List<PrimObject> Children;

        public Linkset()
        {
            RootPrim = new PrimObject();
            Children = new List<PrimObject>();
        }

        public Linkset(PrimObject rootPrim)
        {
            RootPrim = rootPrim;
            Children = new List<PrimObject>();
        }
    }

    public class ImportCommand : Command
    {
        PrimObject currentPrim;
        LLVector3 currentPosition;
        SecondLife currentClient;
        ManualResetEvent primDone;
        List<PrimObject> primsCreated;
        uint rootLocalID = 0;
        bool registeredCreateEvent = false;
        bool rezzingRootPrim = false;
        bool linking = false;

        public ImportCommand()
        {
            Name = "import";
            Description = "Import prims from an exported xml file. Usage: import [filename.xml]";
            primDone = new ManualResetEvent(false);
            registeredCreateEvent = false;
        }

        public override string Execute(SecondLife Client, string[] args, LLUUID fromAgentID)
        {
            if (args.Length != 1)
                return "Usage: import inputfile.xml";

            string name = args[0];
            Dictionary<uint, PrimObject> prims;

            currentClient = Client;

            try
            {
                XmlReader reader = XmlReader.Create(name);
                List<PrimObject> listprims = Helpers.PrimListFromXml(reader);
                reader.Close();

                // Create a dictionary indexed by the old local ID of the prims
                prims = new Dictionary<uint, PrimObject>();
                foreach (PrimObject prim in listprims)
                {
                    prims.Add(prim.LocalID, prim);
                }
            }
            catch (Exception ex)
            {
                return "Deserialize failed: " + ex.ToString();
            }

            if (!registeredCreateEvent)
            {
                TestClient.OnPrimCreated += new TestClient.PrimCreatedCallback(TestClient_OnPrimCreated);
                registeredCreateEvent = true;
            }

            // Build an organized structure from the imported prims
            Dictionary<uint, Linkset> linksets = new Dictionary<uint, Linkset>();
            foreach (PrimObject prim in prims.Values)
            {
                if (prim.ParentID == 0)
                {
                    if (linksets.ContainsKey(prim.LocalID))
                        linksets[prim.LocalID].RootPrim = prim;
                    else
                        linksets[prim.LocalID] = new Linkset(prim);
                }
                else
                {
                    if (!linksets.ContainsKey(prim.ParentID))
                        linksets[prim.ParentID] = new Linkset();

                    linksets[prim.ParentID].Children.Add(prim);
                }
            }
            
            primsCreated = new List<PrimObject>();
            linking = false;
            Console.WriteLine("Importing " + linksets.Count + " structures.");

            foreach (Linkset linkset in linksets.Values)
            {
                if (linkset.RootPrim.LocalID != 0)
                {
                    // HACK: Offset the root prim position so it's not lying on top of the original
                    // We need a more elaborate solution for importing with relative or absolute offsets
                    linkset.RootPrim.Position.Z += 3.0f;
                    currentPosition = linkset.RootPrim.Position;

                    // Rez the root prim with no rotation
                    LLQuaternion rootRotation = linkset.RootPrim.Rotation;
                    linkset.RootPrim.Rotation = LLQuaternion.Identity;

                    rezzingRootPrim = true;
                    currentPrim = linkset.RootPrim;

                    Client.Objects.AddPrim(Client.Network.CurrentSim, linkset.RootPrim, linkset.RootPrim.Position);

                    if (!primDone.WaitOne(10000, false))
                        return "Rez failed, timed out while creating a prim.";
                    primDone.Reset();

                    rezzingRootPrim = false;

                    // Rez the child prims
                    foreach (PrimObject prim in linkset.Children)
                    {
                        currentPrim = prim;
                        currentPosition = prim.Position + linkset.RootPrim.Position;

                        Client.Objects.AddPrim(Client.Network.CurrentSim, prim, currentPosition);

                        if (!primDone.WaitOne(10000, false))
                            return "Rez failed, timed out while creating a prim.";
                        primDone.Reset();
                    }

                    // Create a list of the local IDs of the newly created prims
                    List<uint> primIDs = new List<uint>();
                    foreach (PrimObject prim in primsCreated)
                    {
                        if (prim.LocalID != rootLocalID)
                            primIDs.Add(prim.LocalID);
                    }
                    // Make sure the root object is the last in our list so it becomes the new root
                    primIDs.Add(rootLocalID);

                    // Link and set the permissions + rotation
                    linking = true;

                    Client.Objects.LinkPrims(Client.Network.CurrentSim, primIDs);

                    Client.Objects.SetPermissions(Client.Network.CurrentSim, primIDs,
                        Helpers.PermissionWho.Everyone | Helpers.PermissionWho.Group | Helpers.PermissionWho.NextOwner,
                        Helpers.PermissionType.Copy | Helpers.PermissionType.Modify | Helpers.PermissionType.Move |
                        Helpers.PermissionType.Transfer, true);

                    Client.Objects.SetRotation(Client.Network.CurrentSim, rootLocalID, rootRotation);

                    for (int i = 0; i < linkset.Children.Count + 1; i++)
                    {
                        primDone.WaitOne(10000, false);
                        primDone.Reset();
                    }

                    linking = false;
                }
                else
                {
                    // Skip linksets with a missing root prim
                    Console.WriteLine("WARNING: Skipping a linkset with a missing root prim");
                }

                // Reset everything for the next linkset
                primsCreated.Clear();
            }

            return "Import complete.";
        }

        void TestClient_OnPrimCreated(Simulator simulator, PrimObject prim)
        {
            if (rezzingRootPrim)
            {
                rootLocalID = prim.LocalID;
            }

            if (!linking)
            {
                Console.WriteLine("Setting properties for " + prim.LocalID);

                primsCreated.Add(prim);

                // FIXME: Replace these individual calls with a single ObjectUpdate that sets the 
                // particle system and everything
                currentClient.Objects.SetPosition(simulator, prim.LocalID, currentPosition);
                currentClient.Objects.SetTextures(simulator, prim.LocalID, currentPrim.Textures);
                //currentClient.Objects.SetLight(simulator, prim.LocalID, currentPrim.Light);
                //currentClient.Objects.SetFlexible(simulator, prim.LocalID, currentPrim.Flexible);
            }

            primDone.Set();
        }

        void OnUnknownAttribute(object obj, XmlAttributeEventArgs args)
        {
            // This hasn't happened for me
            Console.WriteLine("OnUnknownAttribute: " + args.Attr.Name);
        }

        void OnUnknownElement(object obj, XmlElementEventArgs args)
        {
            // Breakpoint here and look at the args class
            Console.WriteLine(args.Element.Name);
        }

        void OnUnknownNode(object obj, XmlNodeEventArgs args)
        {
            // Breakpoint here and look at the args class
            Console.WriteLine(args.Name);
        }

        void OnUnreferenced(object obj, UnreferencedObjectEventArgs args)
        {
            // This hasn't happened for me
            Console.WriteLine("OnUnreferenced: " + args.UnreferencedObject.ToString());
        }
    }
}
