using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace VisualParamGenerator
{
    class VisualParamGenerator
    {
        public static readonly System.Globalization.CultureInfo EnUsCulture = 
            new System.Globalization.CultureInfo("en-us");

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: VisualParamGenerator.exe [template.cs] [_VisualParams_.cs]");
                return;
            }
            else if (!File.Exists(args[0]))
            {
                Console.WriteLine("Couldn't find file " + args[0]);
                return;
            }

            XmlNodeList list;
            TextWriter writer;

            try
            {
                writer = new StreamWriter(args[1]);
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't open " + args[1] + " for writing");
                return;
            }

            try
            {
                // Read in the template.cs file and write it to our output
                TextReader reader = new StreamReader(args[0]);
                writer.WriteLine(reader.ReadToEnd());
                reader.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't read from file " + args[0]);
                return;
            }

            try
            {
                // Read in avatar_lad.xml
                Stream stream = OpenMetaverse.Helpers.GetResourceStream("avatar_lad.xml");
                StreamReader reader = new StreamReader(stream);

                if (stream != null)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(reader.ReadToEnd());
                    list = doc.GetElementsByTagName("param");
                }
                else
                {
                    Console.WriteLine("Failed to load resource avatar_lad.xml. Are you missing a data folder?");
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }

            SortedList<int, string> IDs = new SortedList<int, string>();
            StringWriter output = new StringWriter();

            // Make sure we end up with 218 Group-0 VisualParams as a sanity check
            int count = 0;

            foreach (XmlNode node in list)
            {
                if (node.Attributes["group"] == null)
                {
                    // Sanity check that a group is assigned
                    Console.WriteLine("Encountered a param with no group set!");
                    continue;
                }
                if ((node.Attributes["shared"] != null) && (node.Attributes["shared"].Value.Equals("1")))
                {
                    // This param will have been already been defined
                    continue;
                }
                if ((node.Attributes["edit_group"] == null))
                {
                    // This param is calculated by the client based on other params
                    continue;
                }

                // Confirm this is a valid VisualParam
                if (node.Attributes["id"] != null &&
                    node.Attributes["name"] != null)
                {
                    try
                    {
                        int id = Int32.Parse(node.Attributes["id"].Value);

                        // Check for duplicates
                        if (IDs.ContainsKey(id))
                            continue;

                        string name = node.Attributes["name"].Value;
                        int group = Int32.Parse(node.Attributes["group"].Value);

                        string wearable = "null";
                        if (node.Attributes["wearable"] != null)
                            wearable = "\"" + node.Attributes["wearable"].Value + "\"";

                        string label = "String.Empty";
                        if (node.Attributes["label"] != null)
                            label = "\"" + node.Attributes["label"].Value + "\"";

                        string label_min = "String.Empty";
                        if (node.Attributes["label_min"] != null)
                            label_min = "\"" + node.Attributes["label_min"].Value + "\"";

                        string label_max = "String.Empty";
                        if (node.Attributes["label_max"] != null)
                            label_max = "\"" + node.Attributes["label_max"].Value + "\"";

                        float min = Single.Parse(node.Attributes["value_min"].Value, 
                            System.Globalization.NumberStyles.Float, EnUsCulture.NumberFormat);
                        float max = Single.Parse(node.Attributes["value_max"].Value,
                            System.Globalization.NumberStyles.Float, EnUsCulture.NumberFormat);

                        float def;
                        if (node.Attributes["value_default"] != null)
                            def = Single.Parse(node.Attributes["value_default"].Value,
                                System.Globalization.NumberStyles.Float, EnUsCulture.NumberFormat);
                        else
                            def = min;

                        IDs.Add(id,
                            String.Format("            Params[{0}] = new VisualParam({0}, \"{1}\", {2}, {3}, {4}, {5}, {6}, {7}f, {8}f, {9}f);{10}",
                            id, name, group, wearable, label, label_min, label_max, def, min, max, Environment.NewLine));

                        if (group == 0)
                            ++count;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }

            if (count != 218)
            {
                Console.WriteLine("Ended up with the wrong number of Group-8 VisualParams! Exiting...");
                return;
            }

            // Now that we've collected all the entries and sorted them, add them to our output buffer
            foreach (string line in IDs.Values)
            {
                output.Write(line);
            }

            output.Write("        }" + Environment.NewLine);
            output.Write("    }" + Environment.NewLine);
            output.Write("}" + Environment.NewLine);

            try
            {
                writer.Write(output.ToString());
                writer.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
