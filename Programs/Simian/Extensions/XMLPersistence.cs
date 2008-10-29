using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace Simian.Extensions
{
    public class XMLPersistence : IExtension<Simian>, IPersistenceProvider
    {
        Simian server;

        public XMLPersistence()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;

            LLSD llsd;

            try
            {
                XmlTextReader reader = new XmlTextReader(File.OpenRead(server.DataDir + "simiandata.xml"));
                llsd = LLSDParser.DeserializeXml(reader);
                reader.Close();
            }
            catch (FileNotFoundException)
            {
                return;
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to load saved data: " + ex.Message, Helpers.LogLevel.Error);
                return;
            }

            if (llsd is LLSDMap)
            {
                LLSDMap dictionary = (LLSDMap)llsd;

                for (int i = 0; i < server.PersistentExtensions.Count; i++)
                {
                    IPersistable persistable = server.PersistentExtensions[i];

                    LLSD savedData;
                    if (dictionary.TryGetValue(persistable.ToString(), out savedData))
                    {
                        Logger.DebugLog("Loading saved data for " + persistable.ToString());
                        persistable.Deserialize(savedData);
                    }
                    else
                    {
                        Logger.DebugLog("No saved data found for " + persistable.ToString());
                    }
                }
            }
        }

        public void Stop()
        {
            LLSDMap dictionary = new LLSDMap(server.PersistentExtensions.Count);

            for (int i = 0; i < server.PersistentExtensions.Count; i++)
            {
                IPersistable persistable = server.PersistentExtensions[i];

                Logger.DebugLog("Storing persistant data for " + persistable.ToString());
                dictionary.Add(persistable.ToString(), persistable.Serialize());
            }

            try
            {
                XmlTextWriter writer = new XmlTextWriter(server.DataDir + "simiandata.xml", System.Text.Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartElement("llsd");
                LLSDParser.SerializeXmlElement(writer, dictionary);
                writer.WriteEndElement();
                writer.Close();
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to save persistance data: " + ex.Message, Helpers.LogLevel.Error);
            }
        }
    }
}
