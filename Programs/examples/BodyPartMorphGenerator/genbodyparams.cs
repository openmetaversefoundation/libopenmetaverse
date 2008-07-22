using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace BodyPartMorphGenerator
{
    class GenBodyParams
    {
        static void Main(string[] args)
        {
            string FileName = "avatar_lad.xml";
            string outFileParts = "_BodyShapeParams_.cs";

            if (args.Length < 1)
            {
                if (!File.Exists(FileName))
                {
                    Console.WriteLine("Usage: genbodyparams [" + FileName + "]");
                    return;
                }

            }
            else
            {
                FileName = args[0];
                if (!File.Exists(FileName))
                {
                    Console.WriteLine("Could not find file: " + FileName);
                    return;
                }
            }

            if (args.Length == 2)
            {
                outFileParts = args[1];
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(File.ReadAllText(FileName));

            XmlNodeList list = doc.GetElementsByTagName("param");

            StringWriter MasterClass = new StringWriter();
            MasterClass.WriteLine("using System;");
            MasterClass.WriteLine("using System.Collections.Generic;");
            MasterClass.WriteLine("using System.IO;");
            MasterClass.WriteLine("using System.Text;");
//            MasterClass.WriteLine();
//            MasterClass.WriteLine("using libsecondlife.AssetSystem.BodyShape;");
            MasterClass.WriteLine();
            MasterClass.WriteLine("namespace libsecondlife.AssetSystem");
            MasterClass.WriteLine("{");
            MasterClass.WriteLine("    public class BodyShapeParams");
            MasterClass.WriteLine("    {");

            StringWriter Labels = new StringWriter();
            Labels.WriteLine("        public static string GetLabel( uint Param )");
            Labels.WriteLine("        {");
            Labels.WriteLine("            switch( Param )");
            Labels.WriteLine("            {");
            Labels.WriteLine("                default:");
            Labels.WriteLine("                    return \"\";");

            StringWriter LabelMin = new StringWriter();
            LabelMin.WriteLine("        public static string GetLabelMin( uint Param )");
            LabelMin.WriteLine("        {");
            LabelMin.WriteLine("            switch( Param )");
            LabelMin.WriteLine("            {");
            LabelMin.WriteLine("                default:");
            LabelMin.WriteLine("                    throw new Exception(\"Unknown Body Part Parameter: \" + Param);");

            StringWriter LabelMax = new StringWriter();
            LabelMax.WriteLine("        public static string GetLabelMax( uint Param )");
            LabelMax.WriteLine("        {");
            LabelMax.WriteLine("            switch( Param )");
            LabelMax.WriteLine("            {");
            LabelMax.WriteLine("                default:");
            LabelMax.WriteLine("                    throw new Exception(\"Unknown Body Part Parameter: \" + Param);");


            StringWriter Names = new StringWriter();
            Names.WriteLine("        public static string GetName( uint Param )");
            Names.WriteLine("        {");
            Names.WriteLine("            switch( Param )");
            Names.WriteLine("            {");
            Names.WriteLine("                default:");
            Names.WriteLine("                    throw new Exception(\"Unknown Body Part Parameter: \" + Param);");

            StringWriter ValueMin = new StringWriter();
            ValueMin.WriteLine("        public static float GetValueMin( uint Param )");
            ValueMin.WriteLine("        {");
            ValueMin.WriteLine("            switch( Param )");
            ValueMin.WriteLine("            {");
            ValueMin.WriteLine("                default:");
            ValueMin.WriteLine("                    throw new Exception(\"Unknown Body Part Parameter: \" + Param);");

            StringWriter ValueMax = new StringWriter();
            ValueMax.WriteLine("        public static float GetValueMax( uint Param )");
            ValueMax.WriteLine("        {");
            ValueMax.WriteLine("            switch( Param )");
            ValueMax.WriteLine("            {");
            ValueMax.WriteLine("                default:");
            ValueMax.WriteLine("                    throw new Exception(\"Unknown Body Part Parameter: \" + Param);");

            StringWriter ValueDefault = new StringWriter();
            ValueDefault.WriteLine("        public static float GetValueDefault( uint Param )");
            ValueDefault.WriteLine("        {");
            ValueDefault.WriteLine("            switch( Param )");
            ValueDefault.WriteLine("            {");
            ValueDefault.WriteLine("                default:");
            ValueDefault.WriteLine("                    throw new Exception(\"Unknown Body Part Parameter: \" + Param);");

            StringWriter ValueValid = new StringWriter();
            ValueValid.WriteLine("        public static bool IsValueValid( uint Param, float Value )");
            ValueValid.WriteLine("        {");
            ValueValid.WriteLine("            switch( Param )");
            ValueValid.WriteLine("            {");
            ValueValid.WriteLine("                default:");
            ValueValid.WriteLine("                    throw new Exception(\"Unknown Body Part Parameter: \" + Param);");


            List<string> EditGroups = new List<string>();

//            XmlTextWriter xtw = new XmlTextWriter(sw);

            File.Delete("ParamTable.csv");
            StreamWriter ParamTable = File.AppendText("ParamTable.csv");
            ParamTable.WriteLine("ID\tName\tEditGroup\tLabel\tLabelMin\tLabelMax\tMinValue\tMaxValue");

            foreach( XmlNode node in list )
            {
                if ((node.Attributes["shared"] != null) && (node.Attributes["shared"].Value.Equals("1")))
                {
                    // this param will have been already been defined
                    continue;
                }

                if (
                    (node.Attributes["edit_group"] != null)
                     && ( node.Attributes["edit_group"].Value.Equals("driven")
//                         || node.Attributes["edit_group"].Value.Equals("dummy")
                        )
                    )
                {
                    // this param is calculated by the client based on other params
                    continue;
                }
/*
                if( node.Attributes["edit_group"] != null )
                {
                    if( !EditGroups.Contains( node.Attributes["edit_group"].Value ) )
                    {
                        EditGroups.Add(node.Attributes["edit_group"].Value);
                        Console.WriteLine(node.Attributes["edit_group"].Value);
                    }
                }
*/
                // Used to identify which nodes are bodyshape parameter nodes
                if ( node.Attributes["id"] != null
                     && node.Attributes["name"] != null
                     && node.Attributes["wearable"] != null
                     // && ( (node.Attributes["label"] != null) || (node.Attributes["label_min"] != null) )
                    )
                {
                    string ParamName = node.Attributes["name"].Value;
                    Console.WriteLine(ParamName);
                    string ParamEditGroup = "";
                    if( node.Attributes["edit_group"] != null )
                    {
                        ParamEditGroup = node.Attributes["edit_group"].Value;
                    }

                    string ParamLabel = "";
                    string ParamLabelMin = "";
                    string ParamLabelMax = "";
                    string ParamMin = "";
                    string ParamMax = "";


                    string ID = node.Attributes["id"].Value;

                    

                    if (node.Attributes["label"] != null)
                    {
                        // Label
                        Labels.WriteLine("                case " + ID + ":");
                        Labels.WriteLine("                    return \"" + node.Attributes["label"].Value + "\";");

                        ParamLabel = node.Attributes["label"].Value;
                    }

                    if (node.Attributes["label_min"] != null)
                    {
                        // Label Min
                        LabelMin.WriteLine("                case " + ID + ":");
                        LabelMin.WriteLine("                    return \"" + node.Attributes["label_min"].Value + "\";");

                        ParamLabelMin = node.Attributes["label_min"].Value;
                    }

                    if (node.Attributes["label_max"] != null)
                    {
                        // Label Max
                        LabelMax.WriteLine("                case " + ID + ":");
                        LabelMax.WriteLine("                    return \"" + node.Attributes["label_max"].Value + "\";");

                        ParamLabelMax = node.Attributes["label_max"].Value;
                    }

                    // Name
                    Names.WriteLine("                case " + ID + ":");
                    Names.WriteLine("                    return \"" + node.Attributes["name"].Value + "\";");

                    // Min Values
                    ValueMin.WriteLine("                case " + ID + ":");
                    ValueMin.WriteLine("                    return " + node.Attributes["value_min"].Value + "f;");

                    ParamMin = node.Attributes["value_min"].Value;

                    // Max Values
                    ValueMax.WriteLine("                case " + ID + ":");
                    ValueMax.WriteLine("                    return " + node.Attributes["value_max"].Value + "f;");

                    ParamMax = node.Attributes["value_max"].Value;

                    // Default values
                    if (node.Attributes["value_default"] != null)
                    {
                        ValueDefault.WriteLine("                case " + ID + ":");
                        ValueDefault.WriteLine("                    return " + node.Attributes["value_default"].Value + "f;");
                    }
                    else
                    {
                        ValueDefault.WriteLine("                case " + ID + ":");
                        ValueDefault.WriteLine("                    return " + node.Attributes["value_min"].Value + "f;");
                    }

                    // Validation Values
                    ValueValid.WriteLine("                case " + node.Attributes["id"].Value + ":");
                    ValueValid.WriteLine("                    return ( (Value >= " + node.Attributes["value_min"].Value + "f) && (Value <= " + node.Attributes["value_max"].Value + "f) );");

                    ParamTable.WriteLine(    ID
                                    + "\t" + ParamName
                                    + "\t" + ParamEditGroup
                                    + "\t" + ParamLabel
                                    + "\t" + ParamLabelMin
                                    + "\t" + ParamLabelMax
                                    + "\t" + ParamMin 
                                    + "\t" + ParamMax);
                }
            }

            ParamTable.Flush();
            ParamTable.Close();

            // Finish up name stuff
            Names.WriteLine("            }"); // Close switch
            Names.WriteLine("        }"); // Close method

            // Finish up label stuff
            Labels.WriteLine("            }"); // Close  switch
            Labels.WriteLine("        }"); // Close  method

            // Finish up min label stuff
            LabelMin.WriteLine("            }"); // Close  switch
            LabelMin.WriteLine("        }"); // Close  method

            // Finish up max label stuff
            LabelMax.WriteLine("            }"); // Close  switch
            LabelMax.WriteLine("        }"); // Close  method

            // Finish up Max Value stuff
            ValueMin.WriteLine("            }"); // Close  switch
            ValueMin.WriteLine("        }"); // Close  method

            // Finish up Min Value stuff
            ValueMax.WriteLine("            }"); // Close  switch
            ValueMax.WriteLine("        }"); // Close  method

            // Finish up Default Value stuff
            ValueDefault.WriteLine("            }"); // Close  switch
            ValueDefault.WriteLine("        }"); // Close  method

            // Finish up Value Valid stuff
            ValueValid.WriteLine("            }"); // Close  switch
            ValueValid.WriteLine("        }"); // Close  method


            StringWriter ValidateAll = new StringWriter();
            ValidateAll.WriteLine("        public static bool IsValid( Dictionary<uint,float> BodyShape )");
            ValidateAll.WriteLine("        {");
            ValidateAll.WriteLine("            foreach(KeyValuePair<uint, float> kvp in BodyShape)");
            ValidateAll.WriteLine("            {");
            ValidateAll.WriteLine("                if( !IsValueValid(kvp.Key, kvp.Value) ) { return false; }");
            ValidateAll.WriteLine("            }");
            ValidateAll.WriteLine("");
            ValidateAll.WriteLine("            return true;");
            ValidateAll.WriteLine("        }");

            StringWriter ToString = new StringWriter();
            ToString.WriteLine("        public static string ToString( Dictionary<uint,float> BodyShape )");
            ToString.WriteLine("        {");
            ToString.WriteLine("            StringWriter sw = new StringWriter();");
            ToString.WriteLine("");
            ToString.WriteLine("            foreach(KeyValuePair<uint, float> kvp in BodyShape)");
            ToString.WriteLine("            {");
            ToString.WriteLine("                sw.Write( kvp.Key + \":\" );");
            ToString.WriteLine("                sw.Write( GetLabel(kvp.Key) + \":\" );");
            ToString.WriteLine("                sw.WriteLine( kvp.Value );");
            ToString.WriteLine("            }");
            ToString.WriteLine("");
            ToString.WriteLine("            return sw.ToString();");
            ToString.WriteLine("        }");



            // Combine Master Class
            MasterClass.Write(Names.ToString());
            MasterClass.Write(Labels.ToString());
            MasterClass.Write(LabelMin.ToString());
            MasterClass.Write(LabelMax.ToString());
            MasterClass.Write(ValueMin.ToString());
            MasterClass.Write(ValueMax.ToString());
            MasterClass.Write(ValueDefault.ToString());
            MasterClass.Write(ValueValid.ToString());
            MasterClass.Write(ValidateAll.ToString());
            MasterClass.Write(ToString.ToString());

            // Finish up the file
            MasterClass.WriteLine("    }"); // Close Class
            MasterClass.WriteLine("}"); // Close Namespace

            Console.WriteLine("Writing " + outFileParts + "...");
            File.WriteAllText(outFileParts, MasterClass.ToString());

            // Console.WriteLine(PartClasses.ToString());
        }
    }
}
