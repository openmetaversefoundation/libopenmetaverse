using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using libsecondlife;
using System.Security.Cryptography;
using System.Text;

namespace libsecondlife
{
    /// <summary>
    /// 
    /// </summary>
    public class LLSD
    {
        /// <summary>
        /// 
        /// </summary>
        public class LLSDParseException : Exception
        {
            public LLSDParseException(string message) : base(message) { }
        }

        /// <summary>
        /// 
        /// </summary>
        public class LLSDSerializeException : Exception
        {
            public LLSDSerializeException(string message) : base(message) { }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static object LLSDDeserialize(byte[] b)
        {
            return LLSDDeserialize(new MemoryStream(b, false));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        public static object LLSDDeserialize(Stream st)
        {
            XmlTextReader reader = new XmlTextReader(st);
            reader.Read(); SkipWS(reader);
            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "llsd")
            {
                throw new LLSDParseException("Expected <llsd>");
            }
            reader.Read();
            object ret = LLSDParseOne(reader);
            SkipWS(reader);
            if (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "llsd")
                throw new LLSDParseException("Expected </llsd>");
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] LLSDSerialize(object obj)
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter writer = new XmlTextWriter(sw);
            writer.Formatting = Formatting.None;
            writer.WriteStartElement("", "llsd", "");
            LLSDWriteOne(writer, obj);
            writer.WriteEndElement();
            writer.Close();
            return Encoding.UTF8.GetBytes(sw.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="obj"></param>
        public static void LLSDWriteOne(XmlTextWriter writer, object obj)
        {
            if (obj == null)
            {
                writer.WriteStartElement("", "undef", "");
                writer.WriteEndElement();
                return;
            }

            Type t = obj.GetType();
            if (t == typeof(string))
            {
                writer.WriteStartElement("", "string", "");
                writer.WriteString((string)obj);
                writer.WriteEndElement();
            }
            else if (t == typeof(long))
            {
                writer.WriteStartElement("", "integer", "");
                writer.WriteString(obj.ToString());
                writer.WriteEndElement();
            }
            else if (t == typeof(double))
            {
                writer.WriteStartElement("", "real", "");
                writer.WriteString(obj.ToString());
                writer.WriteEndElement();
            }
            else if (t == typeof(bool))
            {
                bool b = (bool)obj;
                writer.WriteStartElement("", "boolean", "");
                if (b)
                    writer.WriteString("1");
                else writer.WriteString("0");
                writer.WriteEndElement();
            }
            else if (t == typeof(LLUUID))
            {
                LLUUID u = (LLUUID)obj;
                writer.WriteStartElement("", "uuid", "");
                writer.WriteString(u.ToStringHyphenated());
                writer.WriteEndElement();
            }
            else if (t == typeof(Hashtable))
            {
                Hashtable h = (Hashtable)obj;
                writer.WriteStartElement("", "map", "");
                foreach (string key in h.Keys)
                {
                    writer.WriteStartElement("", "key", "");
                    writer.WriteString(key);
                    writer.WriteEndElement();
                    LLSDWriteOne(writer, h[key]);
                }
                writer.WriteEndElement();
            }
            else if (t == typeof(ArrayList))
            {
                ArrayList a = (ArrayList)obj;
                writer.WriteStartElement("", "array", "");
                foreach (object item in a)
                {
                    LLSDWriteOne(writer, item);
                }
                writer.WriteEndElement();
            }
            else if (t == typeof(byte[]))
            {
                byte[] b = (byte[])obj;
                writer.WriteStartElement("", "binary", "");
                writer.WriteStartAttribute("", "encoding", "");
                writer.WriteString("base64");
                writer.WriteEndAttribute();
                char[] tmp = new char[b.Length * 2]; // too much
                int i = Convert.ToBase64CharArray(b, 0, b.Length, tmp, 0);
                Array.Resize(ref tmp, i);
                writer.WriteString(new String(tmp));
                writer.WriteEndElement();

            }
            else
            {
                throw new LLSDSerializeException("Unknown type " + t.Name);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static object LLSDParseOne(XmlTextReader reader)
        {
            SkipWS(reader);
            if (reader.NodeType != XmlNodeType.Element)
                throw new LLSDParseException("Expected an element");
            string dtype = reader.LocalName; object ret = null;
            //bool st = false;

            switch (dtype)
            {
                case "undef":
                    {
                        if (reader.IsEmptyElement)
                        {
                            reader.Read(); return null;
                        }
                        reader.Read(); SkipWS(reader); ret = null; break;
                    }
                case "boolean":
                    {
                        if (reader.IsEmptyElement)
                        {
                            reader.Read(); return false;
                        }
                        reader.Read();
                        string s = reader.ReadString().Trim();
                        if (s == "" || s == "false" || s == "0")
                        {
                            ret = false;
                        }
                        else if (s == "true" || s == "1")
                        {
                            ret = true;
                        }
                        else
                        {
                            throw new LLSDParseException("Bad boolean value " + s);
                        }
                        break;
                    }
                case "integer":
                    {
                        if (reader.IsEmptyElement)
                        {
                            reader.Read(); return 0L;
                        }
                        reader.Read();
                        ret = Convert.ToInt64(reader.ReadString().Trim());
                        break;
                    }
                case "real":
                    {
                        if (reader.IsEmptyElement)
                        {
                            reader.Read(); return 0.0f;
                        }
                        reader.Read();
                        ret = Convert.ToDouble(reader.ReadString().Trim());
                        break;
                    }
                case "uuid":
                    {
                        if (reader.IsEmptyElement)
                        {
                            reader.Read(); return new LLUUID();
                        }
                        reader.Read();
                        ret = new LLUUID(reader.ReadString().Trim());
                        break;
                    }
                case "string":
                    {
                        if (reader.IsEmptyElement)
                        {
                            reader.Read(); return String.Empty;
                        }
                        reader.Read();
                        ret = reader.ReadString();
                        break;
                    }
                case "binary":
                    {
                        if (reader.IsEmptyElement)
                        {
                            reader.Read(); return new byte[0];
                        }
                        if (reader.GetAttribute("encoding") != null &&
                           reader.GetAttribute("encoding") != "base64")
                            throw new LLSDParseException("Unknown encoding: " +
                                reader.GetAttribute("encoding"));
                        reader.Read();
                        FromBase64Transform b64 = new FromBase64Transform(FromBase64TransformMode.IgnoreWhiteSpaces);
                        byte[] inp = Encoding.ASCII.GetBytes(reader.ReadString());
                        ret = b64.TransformFinalBlock(inp, 0, inp.Length);
                        break;
                    }
                case "date":
                    {
                        reader.Read();
                        throw new Exception("LLSD TODO: date");
                    }
                case "map":
                    {
                        return LLSDParseMap(reader);
                    }
                case "array":
                    {
                        return LLSDParseArray(reader);
                    }
                default:
                    throw new LLSDParseException("Unknown element <" + dtype + ">");
            }
            if (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != dtype)
            {
                throw new LLSDParseException("Expected </" + dtype + ">");
            }
            reader.Read();
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Hashtable LLSDParseMap(XmlTextReader reader)
        {
            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "map")
                throw new LLSDParseException("Expected <map>");
            if (reader.IsEmptyElement)
            {
                reader.Read(); return new Hashtable();
            }
            reader.Read();

            Hashtable ret = new Hashtable();

            while (true)
            {
                SkipWS(reader);
                if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "map")
                {
                    reader.Read(); break;
                }
                if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "key")
                    throw new LLSDParseException("Expected <key>");
                string key = reader.ReadString();
                if (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "key")
                    throw new LLSDParseException("Expected </key>");
                reader.Read();
                object val = LLSDParseOne(reader);
                ret[key] = val;
            }
            return ret; // TODO
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ArrayList LLSDParseArray(XmlTextReader reader)
        {
            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "array")
                throw new LLSDParseException("Expected <array>");
            if (reader.IsEmptyElement)
            {
                reader.Read(); return new ArrayList();
            }
            reader.Read();

            ArrayList ret = new ArrayList();

            while (true)
            {
                SkipWS(reader);
                if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "array")
                {
                    reader.Read(); break;
                }
                ret.Insert(ret.Count, LLSDParseOne(reader));
            }
            return ret; // TODO
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private static string GetSpaces(int count)
        {
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < count; i++) b.Append(" ");
            return b.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="indent"></param>
        /// <returns></returns>
        public static String LLSDDump(object obj, int indent)
        {
            if (obj == null)
            {
                return GetSpaces(indent) + "- undef\n";
            }
            else if (obj.GetType() == typeof(string))
            {
                return GetSpaces(indent) + "- string \"" + (string)obj + "\"\n";
            }
            else if (obj.GetType() == typeof(long))
            {
                return GetSpaces(indent) + "- integer " + obj.ToString() + "\n";
            }
            else if (obj.GetType() == typeof(double))
            {
                return GetSpaces(indent) + "- float " + obj.ToString() + "\n";
            }
            else if (obj.GetType() == typeof(LLUUID))
            {
                return GetSpaces(indent) + "- uuid " + ((LLUUID)obj).ToStringHyphenated() + Environment.NewLine;
            }
            else if (obj.GetType() == typeof(Hashtable))
            {
                StringBuilder ret = new StringBuilder();
                ret.Append(GetSpaces(indent) + "- map" + Environment.NewLine);
                Hashtable map = (Hashtable)obj;

                foreach (string key in map.Keys)
                {
                    ret.Append(GetSpaces(indent + 2) + "- key \"" + key + "\"" + Environment.NewLine);
                    ret.Append(LLSDDump(map[key], indent + 3));
                }

                return ret.ToString();
            }
            else if (obj.GetType() == typeof(ArrayList))
            {
                StringBuilder ret = new StringBuilder();
                ret.Append(GetSpaces(indent) + "- array\n");
                ArrayList list = (ArrayList)obj;

                foreach (object item in list)
                {
                    ret.Append(LLSDDump(item, indent + 2));
                }

                return ret.ToString();
            }
            else if (obj.GetType() == typeof(byte[]))
            {
                return GetSpaces(indent) + "- binary\n" + Helpers.FieldToHexString((byte[])obj, "") + Environment.NewLine;
            }
            else
            {
                return GetSpaces(indent) + "- unknown type " + obj.GetType().Name + Environment.NewLine;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        private static void SkipWS(XmlTextReader reader)
        {
            while (reader.NodeType == XmlNodeType.Comment || reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.SignificantWhitespace || reader.NodeType == XmlNodeType.XmlDeclaration) reader.Read();
        }
    }
}
