using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace VisualParamGenerator
{
    class VisualParamGenerator
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: VisualParamGenerator.exe [avatar_lad.xml] [_VisualParams_.cs]");
                return;
            }
            else if (!File.Exists(args[0]))
            {
                Console.WriteLine("Couldn't find file " + args[0]);
                return;
            }

            XmlNodeList list;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(File.ReadAllText(args[0]));
                list = doc.GetElementsByTagName("param");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }

            StringWriter output = new StringWriter();
            bool first = true;

            output.Write("using System;" + Environment.NewLine + Environment.NewLine);
            output.Write("namespace libsecondlife" + Environment.NewLine);
            output.Write("{" + Environment.NewLine);
            output.Write("    public static class VisualParams" + Environment.NewLine);
            output.Write("    {" + Environment.NewLine);
            output.Write("        public static VisualParam[] Params = new VisualParam[]" + Environment.NewLine);
            output.Write("        {" + Environment.NewLine);

            foreach (XmlNode node in list)
            {
                if ((node.Attributes["shared"] != null) && (node.Attributes["shared"].Value.Equals("1")))
                {
                    // This param will have been already been defined
                    continue;
                }
                if ((node.Attributes["edit_group"] != null) && (node.Attributes["edit_group"].Value.Equals("driven")))
                {
                    // This param is calculated by the client based on other params
                    continue;
                }

                // Confirm this is a valid VisualParam
                if (node.Attributes["id"] != null &&
                    node.Attributes["name"] != null &&
                    node.Attributes["wearable"] != null)
                {
                    if (!first)
                    {
                        output.Write("," + Environment.NewLine);
                    }
                    else
                    {
                        first = false;
                    }

                    try
                    {
                        int id = Int32.Parse(node.Attributes["id"].Value);
                        string name = node.Attributes["name"].Value;
                        int group = Int32.Parse(node.Attributes["group"].Value);
                        string wearable = node.Attributes["wearable"].Value;

                        string label = String.Empty;
                        if (node.Attributes["label"] != null)
                            label = node.Attributes["label"].Value;

                        string label_min = String.Empty;
                        if (node.Attributes["label_min"] != null)
                            label_min = node.Attributes["label_min"].Value;

                        string label_max = String.Empty;
                        if (node.Attributes["label_max"] != null)
                            label_max = node.Attributes["label_max"].Value;

                        float min = Single.Parse(node.Attributes["value_min"].Value);
                        float max = Single.Parse(node.Attributes["value_max"].Value);

                        float def;
                        if (node.Attributes["value_default"] != null)
                            def = Single.Parse(node.Attributes["value_default"].Value);
                        else
                            def = min;
                        

                        output.Write("            new VisualParam(" + id + ", \"" + name + "\", " + group + 
                            ", \"" + wearable + "\", \"" + label + "\", \"" + label_min + "\", \"" + label_max + 
                            "\", " + def + "f, " + min + "f, " + max + "f)");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }

            output.Write(Environment.NewLine + "        };" + Environment.NewLine);
            output.Write("    }" + Environment.NewLine);
            output.Write("}" + Environment.NewLine);

            try
            {
                File.WriteAllText(args[1], output.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
