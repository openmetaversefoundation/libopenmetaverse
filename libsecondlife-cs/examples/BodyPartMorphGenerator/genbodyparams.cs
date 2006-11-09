using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace BodyPartMorphGenerator
{
    class genbodyparams
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
            MasterClass.WriteLine("using libsecondlife.AssetSystem.BodyShape;");
            MasterClass.WriteLine();
            MasterClass.WriteLine("namespace libsecondlife.AssetSystem.BodyShape");
            MasterClass.WriteLine("{");
            MasterClass.WriteLine("    class BodyShapeParams");
            MasterClass.WriteLine("    {");

            StringWriter Labels = new StringWriter();
            Labels.WriteLine("        public string GetLabel( uint Param )");
            Labels.WriteLine("        {");
            Labels.WriteLine("            switch( Param )");
            Labels.WriteLine("            {");
            Labels.WriteLine("                default:");
            Labels.WriteLine("                    throw new Exception(\"Unknown Body Part Parameter: \" + Param);");

            StringWriter Names = new StringWriter();
            Names.WriteLine("        public string GetName( uint Param )");
            Names.WriteLine("        {");
            Names.WriteLine("            switch( Param )");
            Names.WriteLine("            {");
            Names.WriteLine("                default:");
            Names.WriteLine("                    throw new Exception(\"Unknown Body Part Parameter: \" + Param);");

            StringWriter ValueMin = new StringWriter();
            ValueMin.WriteLine("        public float GetValueMin( uint Param )");
            ValueMin.WriteLine("        {");
            ValueMin.WriteLine("            switch( Param )");
            ValueMin.WriteLine("            {");
            ValueMin.WriteLine("                default:");
            ValueMin.WriteLine("                    throw new Exception(\"Unknown Body Part Parameter: \" + Param);");

            StringWriter ValueMax = new StringWriter();
            ValueMax.WriteLine("        public float GetValueMax( uint Param )");
            ValueMax.WriteLine("        {");
            ValueMax.WriteLine("            switch( Param )");
            ValueMax.WriteLine("            {");
            ValueMax.WriteLine("                default:");
            ValueMax.WriteLine("                    throw new Exception(\"Unknown Body Part Parameter: \" + Param);");

            StringWriter ValueDefault = new StringWriter();
            ValueDefault.WriteLine("        public float GetValueDefault( uint Param )");
            ValueDefault.WriteLine("        {");
            ValueDefault.WriteLine("            switch( Param )");
            ValueDefault.WriteLine("            {");
            ValueDefault.WriteLine("                default:");
            ValueDefault.WriteLine("                    throw new Exception(\"Unknown Body Part Parameter: \" + Param);");

            StringWriter ValueValid = new StringWriter();
            ValueValid.WriteLine("        public bool IsValueValid( uint Param, float Value )");
            ValueValid.WriteLine("        {");
            ValueValid.WriteLine("            switch( Param )");
            ValueValid.WriteLine("            {");
            ValueValid.WriteLine("                default:");
            ValueValid.WriteLine("                    throw new Exception(\"Unknown Body Part Parameter: \" + Param);");


            List<string> EditGroups = new List<string>();

//            XmlTextWriter xtw = new XmlTextWriter(sw);

            foreach( XmlNode node in list )
            {
                if ((node.Attributes["shared"] != null) && (node.Attributes["shared"].Value.Equals("1")))
                {
                    // this param will have been already been defined
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
                     && node.Attributes["label"] != null
                    )
                {
                    string ID = node.Attributes["id"].Value;

                    // Label
                    Labels.WriteLine("                case " + ID + ":");
                    Labels.WriteLine("                    return \"" + node.Attributes["label"].Value + "\";");

                    // Name
                    Names.WriteLine("                case " + ID + ":");
                    Names.WriteLine("                    return \"" + node.Attributes["name"].Value + "\";");

                    // Min Values
                    ValueMin.WriteLine("                case " + ID + ":");
                    ValueMin.WriteLine("                    return " + node.Attributes["value_min"].Value + "f;");

                    // Max Values
                    ValueMax.WriteLine("                case " + ID + ":");
                    ValueMax.WriteLine("                    return " + node.Attributes["value_max"].Value + "f;");

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

                    // Max Values
                    ValueValid.WriteLine("                case " + node.Attributes["id"].Value + ":");
                    ValueValid.WriteLine("                    return ( (Value > " + node.Attributes["value_min"].Value + "f) && (Value < " + node.Attributes["value_max"].Value + "f) );");


                }
            }

            // Finish up label stuff
            Labels.WriteLine("            }"); // Close  switch
            Labels.WriteLine("        }"); // Close  method

            // Finish up name stuff
            Names.WriteLine("            }"); // Close switch
            Names.WriteLine("        }"); // Close method

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


            // Combine Master Class
            MasterClass.Write(Labels.ToString());
            MasterClass.Write(Names.ToString());
            MasterClass.Write(ValueMin.ToString());
            MasterClass.Write(ValueMax.ToString());
            MasterClass.Write(ValueDefault.ToString());
            MasterClass.Write(ValueValid.ToString());

            // Finish up the file
            MasterClass.WriteLine("    }"); // Close Class
            MasterClass.WriteLine("}"); // Close Namespace

            Console.WriteLine("Writing " + outFileParts + "...");
            File.WriteAllText(outFileParts, MasterClass.ToString());

            // Console.WriteLine(PartClasses.ToString());
        }
    }
}
