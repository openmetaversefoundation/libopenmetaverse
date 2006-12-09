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
    public class ImportCommand : Command
    {
        PrimObject currentPrim;
        LLVector3 currentPosition;
        SecondLife currentClient;
        ManualResetEvent primDone;
        List<PrimObject> primsCreated;
        bool registeredCreateEvent;

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

            try
            {
                //XmlReader reader = XmlReader.Create(name);
                //prims = Helpers.PrimListFromXml(reader);
                //reader.Close();

                XmlSerializer serializer = new XmlSerializer(typeof(List<PrimObject>));
                XmlDeserializationEvents events = new XmlDeserializationEvents();
                events.OnUnknownAttribute += new XmlAttributeEventHandler(OnUnknownAttribute);
                events.OnUnknownElement += new XmlElementEventHandler(OnUnknownElement);
                events.OnUnknownNode += new XmlNodeEventHandler(OnUnknownNode);
                events.OnUnreferencedObject += new UnreferencedObjectEventHandler(OnUnreferenced);
                List<PrimObject> listprims = (List<PrimObject>)serializer.Deserialize(XmlReader.Create(name), events);

                prims = new Dictionary<uint, PrimObject>();
                foreach (PrimObject prim in listprims)
                {
                    prims.Add(prim.LocalID, prim);
                }
                //return list;
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
            
            primsCreated = new List<PrimObject>();
            Console.WriteLine("Importing " + prims.Count + " prims.");

            foreach (PrimObject prim in prims.Values)
            {
                currentPrim = prim;
                currentClient = Client;
                if (prim.ParentID != 0)
                {
                    if (prims.ContainsKey(prim.ParentID))
                    {
                        currentPosition = prims[prim.ParentID].Position + prim.Position;
                    }
                    else
                    {
                        return "Rez failed, a child prim did not have a matching parent prim. ";
                    }
                }
                else
                {
                    currentPosition = prim.Position;
                }
                Client.Objects.AddPrim(Client.Network.CurrentSim, prim, currentPosition);
                if (!primDone.WaitOne(10000, false))
                    return "Rez failed, timed out while creating a prim.";
                primDone.Reset();
            }

            return "Import complete.";
        }

        void TestClient_OnPrimCreated(Simulator simulator, PrimObject prim)
        {
            primsCreated.Add(prim);
            currentClient.Objects.SetPosition(simulator, prim.LocalID, currentPosition);
            currentClient.Objects.SetTextures(simulator, prim.LocalID, currentPrim.Textures);
            //currentClient.Objects.SetLight(simulator, prim.LocalID, currentPrim.Light);
            //currentClient.Objects.SetFlexible(simulator, prim.LocalID, currentPrim.Flexible);
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
