using System;
using System.Collections.Generic;
using libsecondlife;
using libsecondlife.Packets;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace libsecondlife.TestClient
{
    public class ImportCommand : Command
    {
        public ImportCommand()
        {
            Name = "import";
            Description = "Import prims from an exported xml file. Usage: import [filename.xml]";

//			Execute(null, new string[] {"prims.xml"}, LLUUID.Zero);
        }

        public override string Execute(SecondLife Client, string[] args, LLUUID fromAgentID)
        {
            if (args.Length != 1)
                return "Usage: import inputfile.xml";

            string name = args[0];
            List<PrimObject> prims;

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
                prims = (List<PrimObject>)serializer.Deserialize(XmlReader.Create(name), events);
                //return list;
            }
            catch (Exception ex)
            {
                return "Deserialize failed: " + ex.ToString();
            }

            // deserialization done, just need to code to rez and link.
            return "Deserialized  prims, rez code missing.";
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
