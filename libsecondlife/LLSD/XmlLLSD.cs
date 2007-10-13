using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Text;

namespace libsecondlife.LSD
{
    public static partial class LLSD
    {
        private static XmlSchema XmlSchema;
        private static XmlTextReader XmlTextReader;
        private static string LastXmlErrors = String.Empty;
        private static object XmlValidationLock = new object();
        private static DateTime Epoch = new DateTime(1970, 1, 1);

        public static object DeserializeXml(XmlTextReader xmlData)
        {
            xmlData.Read();
            SkipWhitespace(xmlData);

            xmlData.Read();
            object ret = ParseXmlElement(xmlData);

            return ret;
        }

        public static string SerializeXml(object data)
        {
            throw new NotImplementedException();
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
            while (reader.NodeType != XmlNodeType.Element)
            {
                if (!reader.Read())
                    throw new LLSDException("Couldn't find an element to parse");
            }

            string type = reader.LocalName;

            switch (type)
            {
                case "undef":
                    if (reader.IsEmptyElement)
                        reader.Read();

                    return null;
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
                            return true;
                    }

                    return false;
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
                        return value;
                    }

                    return 0;
                case "real":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return 0d;
                    }

                    if (reader.Read())
                    {
                        double value = 0d;
                        Double.TryParse(reader.ReadString().Trim(), out value);
                        return value;
                    }

                    return 0d;
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
                        return value;
                    }

                    return LLUUID.Zero;
                case "date":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return Epoch;
                    }

                    if (reader.Read())
                    {
                        DateTime value = Epoch;
                        DateTime.TryParse(reader.ReadString().Trim(), out value);
                        return value;
                    }

                    return Epoch;
                case "string":
                    if (reader.IsEmptyElement)
                    {
                        reader.Read();
                        return String.Empty;
                    }

                    if (reader.Read())
                        return reader.ReadString();

                    return String.Empty;
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
                        try { return Convert.FromBase64String(reader.ReadString().Trim()); }
                        catch (FormatException ex) { throw new LLSDException("Binary decoding exception: " + ex.Message); }
                    }

                    return new byte[0];
                case "map":
                    return ParseXmlMap(reader);
                case "array":
                    return ParseXmlArray(reader);
                default:
                    return null;
            }
        }

        private static Dictionary<string, object> ParseXmlMap(XmlTextReader reader)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            while (reader.NodeType != XmlNodeType.Element && reader.LocalName != "map")
            {
                if (!reader.Read())
                    throw new LLSDException("Couldn't find a map to parse");
            }

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
                        break;

                    string key = reader.ReadString();

                    if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "key")
                        reader.Read();

                    dict[key] = ParseXmlElement(reader);
                }
            }

            return dict;
        }

        private static List<object> ParseXmlArray(XmlTextReader reader)
        {
            List<object> list = new List<object>();

            while (reader.NodeType != XmlNodeType.Element && reader.LocalName != "array")
            {
                if (!reader.Read())
                    throw new LLSDException("Couldn't find an array to parse");
            }

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
                reader.NodeType == XmlNodeType.XmlDeclaration ||
                reader.NodeType == XmlNodeType.EndElement)
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
