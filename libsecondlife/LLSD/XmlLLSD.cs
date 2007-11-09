using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Text;

namespace libsecondlife.LLSD
{
    public static partial class LLSDParser
    {
        private static XmlSchema XmlSchema;
        private static XmlTextReader XmlTextReader;
        private static string LastXmlErrors = String.Empty;
        private static object XmlValidationLock = new object();

        public static object DeserializeXml(byte[] xmlData)
        {
            return DeserializeXml(new XmlTextReader(new MemoryStream(xmlData, false)));
        }

        public static object DeserializeXml(XmlTextReader xmlData)
        {
            xmlData.Read();
            SkipWhitespace(xmlData);

            xmlData.Read();
            object ret = ParseXmlElement(xmlData);

            return ret;
        }

        public static byte[] SerializeXmlBytes(object data)
        {
            return Encoding.UTF8.GetBytes(SerializeXmlString(data));
        }

        public static string SerializeXmlString(object data)
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter writer = new XmlTextWriter(sw);
            writer.Formatting = Formatting.None;

            writer.WriteStartElement(String.Empty, "llsd", String.Empty);
            SerializeXmlElement(writer, data);
            writer.WriteEndElement();

            writer.Close();

            return sw.ToString();
        }

        public static void SerializeXmlElement(XmlTextWriter writer, object obj)
        {
            if (obj == null)
            {
                writer.WriteStartElement(String.Empty, "undef", String.Empty);
                writer.WriteEndElement();
            }
            else if (obj is string)
            {
                writer.WriteStartElement(String.Empty, "string", String.Empty);
                writer.WriteString((string)obj);
                writer.WriteEndElement();
            }
            else if (obj is int || obj is uint || obj is short || obj is ushort || obj is byte || obj is sbyte)
            {
                writer.WriteStartElement(String.Empty, "integer", String.Empty);
                writer.WriteString(obj.ToString());
                writer.WriteEndElement();
            }
            else if (obj is double)
            {
                double value = (double)obj;

                writer.WriteStartElement(String.Empty, "real", String.Empty);
                writer.WriteString(value.ToString(Helpers.EnUsCulture));
                writer.WriteEndElement();
            }
            else if (obj is float)
            {
                float value = (float)obj;

                writer.WriteStartElement(String.Empty, "real", String.Empty);
                writer.WriteString(value.ToString(Helpers.EnUsCulture));
                writer.WriteEndElement();
            }
            else if (obj is long)
            {
                // 64-bit integers are not natively supported in LLSD, so we convert to a byte array
                long value = (long)obj;

                byte[] bytes = BitConverter.GetBytes(value);

                writer.WriteStartElement(String.Empty, "binary", String.Empty);

                writer.WriteStartAttribute(String.Empty, "encoding", String.Empty);
                writer.WriteString("base64");
                writer.WriteEndAttribute();

                writer.WriteString(Convert.ToBase64String(bytes));
                writer.WriteEndElement();
            }
            else if (obj is ulong)
            {
                // 64-bit integers are not natively supported in LLSD, so we convert to a byte array
                ulong value = (ulong)obj;

                byte[] bytes = BitConverter.GetBytes(value);

                writer.WriteStartElement(String.Empty, "binary", String.Empty);

                writer.WriteStartAttribute(String.Empty, "encoding", String.Empty);
                writer.WriteString("base64");
                writer.WriteEndAttribute();

                writer.WriteString(Convert.ToBase64String(bytes));
                writer.WriteEndElement();
            }
            else if (obj is bool)
            {
                bool b = (bool)obj;
                writer.WriteStartElement(String.Empty, "boolean", String.Empty);
                writer.WriteString(b ? "1" : "0");
                writer.WriteEndElement();
            }
            else if (obj is LLUUID)
            {
                LLUUID u = (LLUUID)obj;
                writer.WriteStartElement(String.Empty, "uuid", String.Empty);
                writer.WriteString(u.ToStringHyphenated());
                writer.WriteEndElement();
            }
            else if (obj is Dictionary<string, object>)
            {
                Dictionary<string, object> d = obj as Dictionary<string, object>;

                writer.WriteStartElement(String.Empty, "map", String.Empty);
                foreach (string key in d.Keys)
                {
                    writer.WriteStartElement(String.Empty, "key", String.Empty);
                    writer.WriteString(key);
                    writer.WriteEndElement();

                    SerializeXmlElement(writer, d[key]);
                }
                writer.WriteEndElement();
            }
            else if (obj is System.Collections.Hashtable)
            {
                System.Collections.Hashtable h = obj as System.Collections.Hashtable;

                writer.WriteStartElement(String.Empty, "map", String.Empty);
                foreach (string key in h.Keys)
                {
                    writer.WriteStartElement(String.Empty, "key", String.Empty);
                    writer.WriteString(key);
                    writer.WriteEndElement();

                    SerializeXmlElement(writer, h[key]);
                }
                writer.WriteEndElement();
            }
            else if (obj is List<object>)
            {
                List<object> l = obj as List<object>;

                writer.WriteStartElement(String.Empty, "array", String.Empty);
                for (int i = 0; i < l.Count; i++)
                {
                    SerializeXmlElement(writer, l[i]);
                }
                writer.WriteEndElement();
            }
            else if (obj is System.Collections.ArrayList)
            {
                System.Collections.ArrayList a = obj as System.Collections.ArrayList;

                writer.WriteStartElement(String.Empty, "array", String.Empty);
                for (int i = 0; i < a.Count; i++)
                {
                    SerializeXmlElement(writer, a[i]);
                }
                writer.WriteEndElement();
            }
            else if (obj is byte[])
            {
                writer.WriteStartElement(String.Empty, "binary", String.Empty);

                writer.WriteStartAttribute(String.Empty, "encoding", String.Empty);
                writer.WriteString("base64");
                writer.WriteEndAttribute();

                writer.WriteString(Convert.ToBase64String((byte[])obj));
                writer.WriteEndElement();
            }
            else if (obj.GetType().IsArray)
            {
                Array a = (Array)obj;

                writer.WriteStartElement(String.Empty, "array", String.Empty);
                for (int i = 0; i < a.Length; i++)
                {
                    SerializeXmlElement(writer, a.GetValue(i));
                }
                writer.WriteEndElement();
            }
            else
            {
                throw new LLSDException("Unknown type " + obj.GetType().Name);
            }
        }

        public static bool TryValidate(XmlTextReader xmlData, out string error)
        {
            lock (XmlValidationLock)
            {
                LastXmlErrors = String.Empty;
                XmlTextReader = xmlData;

                CreateSchema();

                XmlReaderSettings readerSettings = new XmlReaderSettings();
                readerSettings.ValidationType = ValidationType.Schema;
                readerSettings.Schemas.Add(XmlSchema);
                readerSettings.ValidationEventHandler += new ValidationEventHandler(SchemaValidationHandler);

                XmlReader reader = XmlReader.Create(xmlData, readerSettings);

                try
                {
                    while (reader.Read()) { }
                }
                catch (XmlException)
                {
                    error = LastXmlErrors;
                    return false;
                }

                if (LastXmlErrors == String.Empty)
                {
                    error = null;
                    return true;
                }
                else
                {
                    error = LastXmlErrors;
                    return false;
                }
            }
        }

        private static object ParseXmlElement(XmlTextReader reader)
        {
            SkipWhitespace(reader);

            if (reader.NodeType != XmlNodeType.Element)
                throw new LLSDException("Expected an element");

            string type = reader.LocalName;
            object ret = null;

            switch (type)
            {
                case "undef":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return null;
                    }

                    reader.Read();
                    SkipWhitespace(reader);
                    ret = null;
                    break;
                case "boolean":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return false;
                    }

                    if (reader.Read())
                    {
                        string s = reader.ReadString().Trim();

                        if (!String.IsNullOrEmpty(s) && (s == "true" || s == "1"))
                        {
                            ret = true;
                            break;
                        }
                    }

                    ret = false;
                    break;
                case "integer":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return 0;
                    }

                    if (reader.Read())
                    {
                        int value = 0;
                        Int32.TryParse(reader.ReadString().Trim(), out value);
                        ret = value;
                        break;
                    }

                    ret = 0;
                    break;
                case "real":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return 0d;
                    }

                    if (reader.Read())
                    {
                        double value = 0d;
                        string str = reader.ReadString().Trim().ToLower();

                        if (str == "nan")
                            value = Double.NaN;
                        else
                            Double.TryParse(str, out value);

                        ret = value;
                        break;
                    }

                    ret = 0d;
                    break;
                case "uuid":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return LLUUID.Zero;
                    }

                    if (reader.Read())
                    {
                        LLUUID value = LLUUID.Zero;
                        LLUUID.TryParse(reader.ReadString().Trim(), out value);
                        ret = value;
                        break;
                    }

                    ret = LLUUID.Zero;
                    break;
                case "date":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return Helpers.Epoch;
                    }

                    if (reader.Read())
                    {
                        DateTime value = Helpers.Epoch;
                        DateTime.TryParse(reader.ReadString().Trim(), out value);
                        ret = value;
                        break;
                    }

                    ret = Helpers.Epoch;
                    break;
                case "string":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return String.Empty;
                    }

                    if (reader.Read())
                    {
                        ret = reader.ReadString();
                        break;
                    }

                    ret = String.Empty;
                    break;
                case "binary":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return new byte[0];
                    }

                    if (reader.GetAttribute("encoding") != null && reader.GetAttribute("encoding") != "base64")
                        throw new LLSDException("Unsupported binary encoding: " + reader.GetAttribute("encoding"));

                    if (reader.Read())
                    {
                        try
                        {
                            ret = Convert.FromBase64String(reader.ReadString().Trim());
                            break;
                        }
                        catch (FormatException ex)
                        {
                            throw new LLSDException("Binary decoding exception: " + ex.Message);
                        }
                    }

                    ret = new byte[0];
                    break;
                case "map":
                    return ParseXmlMap(reader);
                case "array":
                    return ParseXmlArray(reader);
                default:
                    reader.Read();
                    ret = null;
                    break;
            }

            if (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != type)
            {
                throw new LLSDException("Expected </" + type + ">");
            }
            else
            {
                reader.Read();
                return ret;
            }
        }

        private static Dictionary<string, object> ParseXmlMap(XmlTextReader reader)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "map")
                throw new NotImplementedException("Expected <map>");

            //while (reader.NodeType != XmlNodeType.Element && reader.LocalName != "map")
            //{
            //    if (!reader.Read())
            //        throw new LLSDException("Couldn't find a map to parse");
            //}

            if (reader.IsEmptyElement)
            {
                reader.Read();
                return dict;
            }

            if (reader.Read())
            {
                while (true)
                {
                    SkipWhitespace(reader);

                    if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "map")
                    {
                        reader.Read();
                        break;
                    }

                    if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "key")
                        throw new LLSDException("Expected <key>");

                    string key = reader.ReadString();

                    if (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "key")
                        throw new LLSDException("Expected </key>");

                    if (reader.Read())
                        dict[key] = ParseXmlElement(reader);
                    else
                        throw new LLSDException("Failed to parse a value for key " + key);
                }
            }

            return dict;
        }

        private static List<object> ParseXmlArray(XmlTextReader reader)
        {
            List<object> list = new List<object>();

            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "array")
                throw new LLSDException("Expected <array>");

            //while (reader.NodeType != XmlNodeType.Element && reader.LocalName != "array")
            //{
            //    if (!reader.Read())
            //        throw new LLSDException("Couldn't find an array to parse");
            //}

            if (reader.IsEmptyElement)
            {
                reader.Read();
                return list;
            }

            if (reader.Read())
            {
                while (true)
                {
                    SkipWhitespace(reader);

                    if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "array")
                    {
                        reader.Read();
                        break;
                    }

                    list.Add(ParseXmlElement(reader));
                }
            }

            return list;
        }        

        private static void SkipWhitespace(XmlTextReader reader)
        {
            while (
                reader.NodeType == XmlNodeType.Comment ||
                reader.NodeType == XmlNodeType.Whitespace ||
                reader.NodeType == XmlNodeType.SignificantWhitespace ||
                reader.NodeType == XmlNodeType.XmlDeclaration)
            {
                reader.Read();
            }
        }

        private static void CreateSchema()
        {
            if (XmlSchema == null)
            {
                #region XSD
                string schemaText = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<xs:schema elementFormDefault=""qualified"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:import schemaLocation=""xml.xsd"" namespace=""http://www.w3.org/XML/1998/namespace"" />
  <xs:element name=""uri"" type=""xs:string"" />
  <xs:element name=""uuid"" type=""xs:string"" />
  <xs:element name=""KEYDATA"">
    <xs:complexType>
      <xs:sequence>
        <xs:element ref=""key"" />
        <xs:element ref=""DATA"" />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
  <xs:element name=""date"" type=""xs:string"" />
  <xs:element name=""key"" type=""xs:string"" />
  <xs:element name=""boolean"" type=""xs:string"" />
  <xs:element name=""undef"">
    <xs:complexType>
      <xs:sequence>
        <xs:element ref=""EMPTY"" />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
  <xs:element name=""map"">
    <xs:complexType>
      <xs:sequence>
        <xs:element minOccurs=""0"" maxOccurs=""unbounded"" ref=""KEYDATA"" />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
  <xs:element name=""real"" type=""xs:string"" />
  <xs:element name=""ATOMIC"">
    <xs:complexType>
      <xs:choice>
        <xs:element ref=""undef"" />
        <xs:element ref=""boolean"" />
        <xs:element ref=""integer"" />
        <xs:element ref=""real"" />
        <xs:element ref=""uuid"" />
        <xs:element ref=""string"" />
        <xs:element ref=""date"" />
        <xs:element ref=""uri"" />
        <xs:element ref=""binary"" />
      </xs:choice>
    </xs:complexType>
  </xs:element>
  <xs:element name=""DATA"">
    <xs:complexType>
      <xs:choice>
        <xs:element ref=""ATOMIC"" />
        <xs:element ref=""map"" />
        <xs:element ref=""array"" />
      </xs:choice>
    </xs:complexType>
  </xs:element>
  <xs:element name=""llsd"">
    <xs:complexType>
      <xs:sequence>
        <xs:element ref=""DATA"" />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
  <xs:element name=""binary"">
    <xs:complexType>
      <xs:simpleContent>
        <xs:extension base=""xs:string"">
          <xs:attribute default=""base64"" name=""encoding"" type=""xs:string"" />
        </xs:extension>
      </xs:simpleContent>
    </xs:complexType>
  </xs:element>
  <xs:element name=""array"">
    <xs:complexType>
      <xs:sequence>
        <xs:element minOccurs=""0"" maxOccurs=""unbounded"" ref=""DATA"" />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
  <xs:element name=""integer"" type=""xs:string"" />
  <xs:element name=""string"">
    <xs:complexType>
      <xs:simpleContent>
        <xs:extension base=""xs:string"">
          <xs:attribute ref=""xml:space"" />
        </xs:extension>
      </xs:simpleContent>
    </xs:complexType>
  </xs:element>
</xs:schema>
";
                #endregion XSD

                MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(schemaText));

                XmlSchema = new XmlSchema();
                XmlSchema = XmlSchema.Read(stream, new ValidationEventHandler(SchemaValidationHandler));
            }
        }

        private static void SchemaValidationHandler(object sender, ValidationEventArgs args)
        {
            string error = String.Format("Line: {0} - Position: {1} - {2}", XmlTextReader.LineNumber, XmlTextReader.LinePosition,
                args.Message);

            if (LastXmlErrors == String.Empty)
                LastXmlErrors = error;
            else
                LastXmlErrors += Environment.NewLine + error;
        }
    }
}
