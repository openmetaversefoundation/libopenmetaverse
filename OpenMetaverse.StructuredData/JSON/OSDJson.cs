using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using LitJson;

namespace OpenMetaverse.StructuredData
{
    public static partial class OSDParser
    {
        public static OSD DeserializeJson(Stream json)
        {
            using (StreamReader streamReader = new StreamReader(json))
            {
                JsonReader reader = new JsonReader(streamReader);
                return DeserializeJson(JsonMapper.ToObject(reader));
            }
        }

        public static OSD DeserializeJson(string json)
        {
            return DeserializeJson(JsonMapper.ToObject(json));
        }

        public static OSD DeserializeJson(JsonData json)
        {
            switch (json.GetJsonType())
            {
                case JsonType.Boolean:
                    return OSD.FromBoolean((bool)json);
                case JsonType.Int:
                    return OSD.FromInteger((int)json);
                case JsonType.Long:
                    return OSD.FromLong((long)json);
                case JsonType.Double:
                    return OSD.FromReal((double)json);
                case JsonType.String:
                    string str = (string)json;
                    if (String.IsNullOrEmpty(str))
                        return new OSD();
                    else
                        return OSD.FromString(str);
                case JsonType.Array:
                    OSDArray array = new OSDArray(json.Count);
                    for (int i = 0; i < json.Count; i++)
                        array.Add(DeserializeJson(json[i]));
                    return array;
                case JsonType.Object:
                    OSDMap map = new OSDMap(json.Count);
                    IDictionaryEnumerator e = ((IOrderedDictionary)json).GetEnumerator();
                    while (e.MoveNext())
                        map.Add((string)e.Key, DeserializeJson((JsonData)e.Value));
                    return map;
                case JsonType.None:
                default:
                    return new OSD();
            }
        }

        public static string SerializeJsonString(OSD osd)
        {
            return SerializeJson(osd).ToJson();
        }

        public static void SerializeJsonString(OSD osd, ref JsonWriter writer)
        {
            SerializeJson(osd).ToJson(writer);
        }

        public static JsonData SerializeJson(OSD osd)
        {
            switch (osd.Type)
            {
                case OSDType.Boolean:
                    return new JsonData(osd.AsBoolean());
                case OSDType.Integer:
                    return new JsonData(osd.AsInteger());
                case OSDType.Real:
                    return new JsonData(osd.AsReal());
                case OSDType.String:
                case OSDType.Date:
                case OSDType.URI:
                case OSDType.UUID:
                    return new JsonData(osd.AsString());
                case OSDType.Binary:
                    byte[] binary = osd.AsBinary();
                    JsonData jsonbinarray = new JsonData();
                    jsonbinarray.SetJsonType(JsonType.Array);
                    for (int i = 0; i < binary.Length; i++)
                        jsonbinarray.Add(new JsonData(binary[i]));
                    return jsonbinarray;
                case OSDType.Array:
                    JsonData jsonarray = new JsonData();
                    jsonarray.SetJsonType(JsonType.Array);
                    OSDArray array = (OSDArray)osd;
                    for (int i = 0; i < array.Count; i++)
                        jsonarray.Add(SerializeJson(array[i]));
                    return jsonarray;
                case OSDType.Map:
                    JsonData jsonmap = new JsonData();
                    jsonmap.SetJsonType(JsonType.Object);
                    OSDMap map = (OSDMap)osd;
                    foreach (KeyValuePair<string, OSD> kvp in map)
                        jsonmap[kvp.Key] = SerializeJson(kvp.Value);
                    return jsonmap;
                case OSDType.Unknown:
                default:
                    return new JsonData();
            }
        }
    }
}
