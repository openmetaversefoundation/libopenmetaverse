using System;
using System.Collections.Generic;
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
                    {
                        return new OSD();
                    }
                    else
                    {
                        switch (str[0])
                        {
                            case 'd':
                                if (str.StartsWith("date::"))
                                {
                                    DateTime dt;
                                    if (DateTime.TryParse(str.Substring(6), out dt))
                                        return OSD.FromDate(dt);
                                }
                                break;
                            case 'u':
                                if (str.StartsWith("uuid::"))
                                {
                                    UUID id;
                                    if (UUID.TryParse(str.Substring(6), out id))
                                        return OSD.FromUUID(id);
                                }
                                else if (str.StartsWith("uri::"))
                                {
                                    try
                                    {
                                        Uri uri = new Uri(str.Substring(5));
                                        return OSD.FromUri(uri);
                                    }
                                    catch (UriFormatException) { }
                                }
                                break;
                            case 'b':
                                if (str.StartsWith("b64::"))
                                {
                                    try
                                    {
                                        byte[] data = Convert.FromBase64String(str.Substring(5));
                                        return OSD.FromBinary(data);
                                    }
                                    catch (FormatException) { }
                                }
                                break;
                        }

                        return OSD.FromString((string)json);
                    }
                case JsonType.Array:
                    OSDArray array = new OSDArray(json.Count);
                    for (int i = 0; i < json.Count; i++)
                        array.Add(DeserializeJson(json[i]));
                    return array;
                case JsonType.Object:
                    OSDMap map = new OSDMap(json.Count);
                    foreach (KeyValuePair<string, JsonData> kvp in json)
                        map.Add(kvp.Key, DeserializeJson(kvp.Value));
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
                    return new JsonData(osd.AsString());
                case OSDType.Date:
                    return new JsonData("date::" + osd.AsString());
                case OSDType.URI:
                    return new JsonData("uri::" + osd.AsString());
                case OSDType.UUID:
                    return new JsonData("uuid::" + osd.AsString());
                case OSDType.Binary:
                    return new JsonData("b64::" + Convert.ToBase64String(osd.AsBinary()));
                case OSDType.Array:
                    JsonData jsonarray = new JsonData();
                    OSDArray array = (OSDArray)osd;
                    for (int i = 0; i < array.Count; i++)
                        jsonarray.Add(SerializeJson(array[i]));
                    return jsonarray;
                case OSDType.Map:
                    JsonData jsonmap = new JsonData();
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
