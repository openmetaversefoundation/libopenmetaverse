/*
 * Copyright (c) 2009, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

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
        public bool AutoScrollEnabled;
        public bool StatisticsEnabled;
        public bool SaveSessionOnExit;
        public bool AutoCheckNewCaps;

        public SettingsStore()
        {
            MessageSessions = new Dictionary<string, bool>();
            PacketSessions = new Dictionary<string, bool>();
        }

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            if (MessageSessions.Count > 0)
            {
                OSDArray messageArray = new OSDArray(MessageSessions.Count);
                foreach (KeyValuePair<string, bool> kvp in MessageSessions)
                {
                    OSDMap sessionMap = new OSDMap(2);
                    sessionMap["Capability"] = OSD.FromString(kvp.Key);
                    sessionMap["Capture"] = OSD.FromBoolean(kvp.Value);
                    messageArray.Add(sessionMap);
                }
                map.Add("message_sessions", messageArray);
            }

            if (PacketSessions.Count > 0)
            {
                OSDArray packetArray = new OSDArray(PacketSessions.Count);
                foreach (KeyValuePair<string, bool> kvp in PacketSessions)
                {
                    OSDMap sessionMap = new OSDMap(2);
                    sessionMap["PacketName"] = OSD.FromString(kvp.Key);
                    sessionMap["Capture"] = OSD.FromBoolean(kvp.Value);
                    packetArray.Add(sessionMap);
                }
                map.Add("packet_sessions", packetArray);
            }

            map.Add("AutoScrollSessions", OSD.FromBoolean(AutoScrollEnabled));
            map.Add("CaptureStatistics", OSD.FromBoolean(StatisticsEnabled));
            map.Add("SaveProfileOnExit", OSD.FromBoolean(SaveSessionOnExit));
            map.Add("AutoCheckNewCaps", OSD.FromBoolean(AutoCheckNewCaps));

            return map;
        }

        public void Deserialize(OSDMap map)
        {

            if (map.ContainsKey("message_sessions"))
            {

                AutoScrollEnabled = map["AutoScrollSessions"].AsBoolean();
                StatisticsEnabled = map["CaptureStatistics"].AsBoolean();
                SaveSessionOnExit = map["SaveProfileOnExit"].AsBoolean();
                AutoCheckNewCaps = map["AutoCheckNewCaps"].AsBoolean();

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

        public bool DeserializeFromFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    OSDMap map = (OSDMap)OSDParser.DeserializeLLSDNotation(File.ReadAllText(fileName));
                    this.Deserialize(map);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception Deserializing From File: {0} {1}", e.Message, e.StackTrace);
                }
            }
            return false;
        }

        public void SerializeToFile(string fileName)
        {
            File.WriteAllText(fileName, this.Serialize().ToString());
        }

    }
}
