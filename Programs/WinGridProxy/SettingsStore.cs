using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse.StructuredData;
using System.IO;

namespace WinGridProxy
{
    class SettingsStore
    {
        public Dictionary<string, bool> MessageSessions;

        public Dictionary<string, bool> PacketSessions;

        public SettingsStore()
        {
            MessageSessions = new Dictionary<string, bool>();
            PacketSessions = new Dictionary<string, bool>();
        }

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);
            OSDArray messageArray = new OSDArray(MessageSessions.Count);
            foreach (KeyValuePair<string, bool> kvp in MessageSessions)
            {
                OSDMap sessionMap = new OSDMap(2);
                sessionMap["Capability"] = OSD.FromString(kvp.Key);
                sessionMap["Capture"] = OSD.FromBoolean(kvp.Value);
                messageArray.Add(sessionMap);
            }
            map["message_sessions"] = messageArray;

            OSDArray packetArray = new OSDArray(PacketSessions.Count);
            foreach (KeyValuePair<string, bool> kvp in PacketSessions)
            {
                OSDMap sessionMap = new OSDMap(2);
                sessionMap["PacketName"] = OSD.FromString(kvp.Key);
                sessionMap["Capture"] = OSD.FromBoolean(kvp.Value);
                packetArray.Add(sessionMap);
            }
            map["packet_sessions"] = packetArray;

            return map;
        }

        public void Deserialize(OSDMap map)
        {

            if (map.ContainsKey("message_sessions"))
            {
                OSDArray messageArray = (OSDArray)map["message_sessions"];

                MessageSessions = new Dictionary<string, bool>(messageArray.Count);

                for (int i = 0; i < messageArray.Count; i++)
                {
                    OSDMap m = (OSDMap)messageArray[i];
                    MessageSessions.Add(m["Capability"].AsString(), m["Capture"].AsBoolean());
                    
                }
            }
            else
            {
                //MessageSessions = new Dictionary<string, bool>();
            }


            if (map.ContainsKey("packet_sessions"))
            {
                OSDArray packetArray = (OSDArray)map["packet_sessions"];

                PacketSessions = new Dictionary<string, bool>(packetArray.Count);

                for (int i = 0; i < packetArray.Count; i++)
                {
                    OSDMap packetMap = (OSDMap)packetArray[i];
                    
                    PacketSessions.Add(packetMap["PacketName"].AsString(), packetMap["Capture"].AsBoolean());
                }
            }
            else
            {
                //PacketSessions = new Dictionary<string, bool>();
            }

        }

        public void DeserializeFromFile(string fileName)
        {
            
            if(File.Exists(fileName))
            {
                OSDMap map = (OSDMap)OSDParser.DeserializeLLSDNotation(File.ReadAllText(fileName));
                this.Deserialize(map);
            }
        }

        public void SerializeToFile(string fileName)
        {
            File.WriteAllText(fileName, this.Serialize().ToString());
        }

    }
}
