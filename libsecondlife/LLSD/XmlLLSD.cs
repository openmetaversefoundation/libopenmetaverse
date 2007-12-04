using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Text;

namespace libsecondlife.StructuredData
{
    public static partial class LLSDParser
    {
        private static XmlSchema XmlSchema;
        private static XmlTextReader XmlTextReader;
        private static string LastXmlErrors = String.Empty;
        private static object XmlValidationLock = new object();

        public static LLSD DeserializeXml(byte[] xmlData)
        {
            return DeserializeXml(new XmlTextReader(new MemoryStream(xmlData, false)));
        }

        public static LLSD DeserializeXml(string xmlData)
        {
            byte[] bytes = Helpers.StringToField(xmlData);
            return DeserializeXml(new XmlTextReader(new MemoryStream(bytes, false)));
        }

        public static LLSD DeserializeXml(XmlTextReader xmlData)
        {
            xmlData.Read();
            SkipWhitespace(xmlData);

            xmlData.Read();
            LLSD ret = ParseXmlElement(xmlData);

            return ret;
        }

        public static byte[] SerializeXmlBytes(LLSD data)
        {
            return Encoding.UTF8.GetBytes(SerializeXmlString(data));
        }

        public static string SerializeXmlString(LLSD data)
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

        public static void SerializeXmlElement(XmlTextWriter writer, LLSD data)
        {
            switch (data.Type)
            {
                case LLSDType.Unknown:
                    writer.WriteStartElement(String.Empty, "undef", String.Empty);
                    writer.WriteEndElement();
                    break;
                case LLSDType.Boolean:
                    writer.WriteStartElement(String.Empty, "boolean", String.Empty);
                    writer.WriteString(data.AsString());
                    writer.WriteEndElement();
                    break;
                case LLSDType.Integer:
                    writer.WriteStartElement(String.Empty, "integer", String.Empty);
                    writer.WriteString(data.AsString());
                    writer.WriteEndElement();
                    break;
                case LLSDType.Real:
                    writer.WriteStartElement(String.Empty, "real", String.Empty);
                    writer.WriteString(data.AsString());
                    writer.WriteEndElement();
                    break;
                case LLSDType.String:
                    writer.WriteStartElement(String.Empty, "string", String.Empty);
                    writer.WriteString(data.AsString());
                    writer.WriteEndElement();
                    break;
                case LLSDType.UUID:
                    writer.WriteStartElement(String.Empty, "uuid", String.Empty);
                    writer.WriteString(data.AsString());
                    writer.WriteEndElement();
                    break;
                case LLSDType.Date:
                    writer.WriteStartElement(String.Empty, "date", String.Empty);
                    writer.WriteString(data.AsString());
                    writer.WriteEndElement();
                    break;
                case LLSDType.URI:
                    writer.WriteStartElement(String.Empty, "uri", String.Empty);
                    writer.WriteString(data.AsString());
                    writer.WriteEndElement();
                    break;
                case LLSDType.Binary:
                    writer.WriteStartElement(String.Empty, "binary", String.Empty);
                        writer.WriteStartAttribute(String.Empty, "encoding", String.Empty);
                        writer.WriteString("base64");
                        writer.WriteEndAttribute();
                    writer.WriteString(data.AsString());
                    writer.WriteEndElement();
                    break;
                case LLSDType.Map:
                    LLSDMap map = (LLSDMap)data;
                    writer.WriteStartElement(String.Empty, "map", String.Empty);
                    foreach (KeyValuePair<string, LLSD> kvp in map)
                    {
                        writer.WriteStartElement(String.Empty, "key", String.Empty);
                        writer.WriteString(kvp.Key);
                        writer.WriteEndElement();

                        SerializeXmlElement(writer, kvp.Value);
                    }
                    writer.WriteEndElement();
                    break;
                case LLSDType.Array:
                    LLSDArray array = (LLSDArray)data;
                    writer.WriteStartElement(String.Empty, "array", String.Empty);
                    for (int i = 0; i < array.Count; i++)
                    {
                        SerializeXmlElement(writer, array[i]);
                    }
                    writer.WriteEndElement();
                    break;
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

        private static LLSD ParseXmlElement(XmlTextReader reader)
        {
            SkipWhitespace(reader);

            if (reader.NodeType != XmlNodeType.Element)
                throw new LLSDException("Expected an element");

            string type = reader.LocalName;
            LLSD ret;

            switch (type)
            {
                case "undef":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return new LLSD();
                    }

                    reader.Read();
                    SkipWhitespace(reader);
                    ret = new LLSD();
                    break;
                case "boolean":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return LLSD.FromBoolean(false);
                    }

                    if (reader.Read())
                    {
                        string s = reader.ReadString().Trim();

                        if (!String.IsNullOrEmpty(s) && (s == "true" || s == "1"))
                        {
                            ret = LLSD.FromBoolean(true);
                            break;
                        }
                    }

                    ret = LLSD.FromBoolean(false);
                    break;
                case "integer":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return LLSD.FromInteger(0);
                    }

                    if (reader.Read())
                    {
                        int value = 0;
                        Helpers.TryParse(reader.ReadString().Trim(), out value);
                        ret = LLSD.FromInteger(value);
                        break;
                    }

                    ret = LLSD.FromInteger(0);
                    break;
                case "real":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return LLSD.FromReal(0d);
                    }

                    if (reader.Read())
                    {
                        double value = 0d;
                        string str = reader.ReadString().Trim().ToLower();

                        if (str == "nan")
                            value = Double.NaN;
                        else
                            Helpers.TryParse(str, out value);

                        ret = LLSD.FromReal(value);
                        break;
                    }

                    ret = LLSD.FromReal(0d);
                    break;
                case "uuid":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return LLSD.FromUUID(LLUUID.Zero);
                    }

                    if (reader.Read())
                    {
                        LLUUID value = LLUUID.Zero;
                        LLUUID.TryParse(reader.ReadString().Trim(), out value);
                        ret = LLSD.FromUUID(value);
                        break;
                    }

                    ret = LLSD.FromUUID(LLUUID.Zero);
                    break;
                case "date":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return LLSD.FromDate(Helpers.Epoch);
                    }

                    if (reader.Read())
                    {
                        DateTime value = Helpers.Epoch;
                        Helpers.TryParse(reader.ReadString().Trim(), out value);
                        ret = LLSD.FromDate(value);
                        break;
                    }

                    ret = LLSD.FromDate(Helpers.Epoch);
                    break;
                case "string":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return LLSD.FromString(String.Empty);
                    }

                    if (reader.Read())
                    {
                        ret = LLSD.FromString(reader.ReadString());
                        break;
                    }

                    ret = LLSD.FromString(String.Empty);
                    break;
                case "binary":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return LLSD.FromBinary(new byte[0]);
                    }

                    if (reader.GetAttribute("encoding") != null && reader.GetAttribute("encoding") != "base64")
                        throw new LLSDException("Unsupported binary encoding: " + reader.GetAttribute("encoding"));

                    if (reader.Read())
                    {
                        try
                        {
                            ret = LLSD.FromBinary(Convert.FromBase64String(reader.ReadString().Trim()));
                            break;
                        }
                        catch (FormatException ex)
                        {
                            throw new LLSDException("Binary decoding exception: " + ex.Message);
                        }
                    }

                    ret = LLSD.FromBinary(new byte[0]);
                    break;
                case "uri":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return LLSD.FromUri(new Uri(String.Empty, UriKind.RelativeOrAbsolute));
                    }

                    if (reader.Read())
                    {
                        ret = LLSD.FromUri(new Uri(reader.ReadString(), UriKind.RelativeOrAbsolute));
                        break;
                    }

                    ret = LLSD.FromUri(new Uri(String.Empty, UriKind.RelativeOrAbsolute));
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

        private static LLSDMap ParseXmlMap(XmlTextReader reader)
        {
            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "map")
                throw new NotImplementedException("Expected <map>");

            LLSDMap map = new LLSDMap();

            if (reader.IsEmptyElement)
            {
                reader.Read();
                return map;
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
                        map[key] = ParseXmlElement(reader);
                    else
                        throw new LLSDException("Failed to parse a value for key " + key);
                }
            }

            return map;
        }

        private static LLSDArray ParseXmlArray(XmlTextReader reader)
        {
            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "array")
                throw new LLSDException("Expected <array>");

            LLSDArray array = new LLSDArray();

            if (reader.IsEmptyElement)
            {
                reader.Read();
                return array;
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

                    array.Add(ParseXmlElement(reader));
                }
            }

            return array;
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
                LastXmlErrors += Helpers.NewLine + error;
        }
    }
}
