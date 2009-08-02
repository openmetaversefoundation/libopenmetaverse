using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
            Dictionary<int, string> Alphas = new Dictionary<int, string>();
            Dictionary<int, string> Colors = new Dictionary<int, string>();

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

                        string bumpAttrib = "false";
                        bool skipColor = false;

                        if (node.ParentNode.Name == "layer")
                        {
                            if (node.ParentNode.Attributes["render_pass"] != null && node.ParentNode.Attributes["render_pass"].Value == "bump")
                            {
                                bumpAttrib = "true";
                            }

                            for (int nodeNr = 0; nodeNr < node.ParentNode.ChildNodes.Count; nodeNr++)
                            {
                                XmlNode lnode = node.ParentNode.ChildNodes[nodeNr];

                                if (lnode.Name == "texture")
                                {
                                    if (lnode.Attributes["local_texture_alpha_only"] != null && lnode.Attributes["local_texture_alpha_only"].Value.ToLower() == "true")
                                    {
                                        skipColor = true;
                                    }
                                }
                            }
                        }


                        if (node.HasChildNodes)
                        {
                            for (int nodeNr = 0; nodeNr < node.ChildNodes.Count; nodeNr++)
                            {
                                #region Alpha mask and bumps
                                if (node.ChildNodes[nodeNr].Name == "param_alpha")
                                {
                                    XmlNode anode = node.ChildNodes[nodeNr];
                                    string tga_file = "string.Empty";
                                    string skip_if_zero = "false";
                                    string multiply_blend = "false";
                                    string domain = "0";

                                    if (anode.Attributes["domain"] != null)
                                        domain = anode.Attributes["domain"].Value;

                                    if (anode.Attributes["tga_file"] != null)
                                        tga_file = string.Format("\"{0}\"", anode.Attributes["tga_file"].Value); ;

                                    if (anode.Attributes["skip_if_zero"] != null && anode.Attributes["skip_if_zero"].Value.ToLower() == "true")
                                        skip_if_zero = "true";

                                    if (anode.Attributes["multiply_blend"] != null && anode.Attributes["multiply_blend"].Value.ToLower() == "true")
                                        multiply_blend = "true";

                                    Alphas.Add(id, string.Format("new VisualAlphaParam({0}f, {1}, {2}, {3})", domain, tga_file, skip_if_zero, multiply_blend));
                                }
                                #endregion
                                #region Colors
                                else if (node.ChildNodes[nodeNr].Name == "param_color" && node.ChildNodes[nodeNr].HasChildNodes)
                                {
                                    XmlNode cnode = node.ChildNodes[nodeNr];
                                    string operation = "VisualColorOperation.None";
                                    List<string> colors = new List<string>();

                                    if (cnode.Attributes["operation"] != null)
                                    {

                                        switch (cnode.Attributes["operation"].Value)
                                        {
                                            case "blend":
                                                operation = "VisualColorOperation.Blend";
                                                break;

                                            case "multiply":
                                                operation = "VisualColorOperation.Blend";
                                                break;
                                        }
                                    }

                                    foreach (XmlNode cvalue in cnode.ChildNodes)
                                    {
                                        if (cvalue.Name == "value" && cvalue.Attributes["color"] != null)
                                        {
                                            Match m = Regex.Match(cvalue.Attributes["color"].Value, @"((?<val>\d+)(?:, *)?){4}");
                                            if (!m.Success)
                                            {
                                                continue;
                                            }
                                            CaptureCollection val = m.Groups["val"].Captures;
                                            colors.Add(string.Format("System.Drawing.Color.FromArgb({0}, {1}, {2}, {3})", val[3], val[0], val[1], val[2]));
                                        }
                                    }

                                    if (colors.Count > 0 && !skipColor)
                                    {
                                        string colorsStr = string.Join(", ", colors.ToArray());
                                        Colors.Add(id, string.Format("new VisualColorParam({0}, new System.Drawing.Color[] {{ {1} }})", operation, colorsStr));
                                    }
                                }
                                #endregion

                            }
                        }

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

                        string drivers = "null";
                        if (node.HasChildNodes)
                        {
                            for (int nodeNr = 0; nodeNr < node.ChildNodes.Count; nodeNr++)
                            {
                                XmlNode cnode = node.ChildNodes[nodeNr];

                                if (cnode.Name == "param_driver" && cnode.HasChildNodes)
                                {
                                    List<string> driverIDs = new List<string>();
                                    foreach (XmlNode dnode in cnode.ChildNodes)
                                    {
                                        if (dnode.Name == "driven" && dnode.Attributes["id"] != null)
                                        {
                                            driverIDs.Add(dnode.Attributes["id"].Value);
                                        }
                                    }

                                    if (driverIDs.Count > 0)
                                    {
                                        drivers = string.Format("new int[] {{ {0} }}", string.Join(", ", driverIDs.ToArray()));
                                    }

                                }
                            }
                        }

                        IDs.Add(id,
                            String.Format("            Params[{0}] = new VisualParam({0}, \"{1}\", {2}, {3}, {4}, {5}, {6}, {7}f, {8}f, {9}f, {10}, {11}, ",
                            id, name, group, wearable, label, label_min, label_max, def, min, max, bumpAttrib, drivers));

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
                Console.WriteLine("Ended up with the wrong number of Group-0 VisualParams! Exiting...");
                return;
            }

            // Now that we've collected all the entries and sorted them, add them to our output buffer
            foreach (KeyValuePair<int, string> line in IDs)
            {
                output.Write(line.Value);

                if (Alphas.ContainsKey(line.Key))
                {
                    output.Write(Alphas[line.Key] + ", ");
                }
                else
                {
                    output.Write("null, ");
                }

                if (Colors.ContainsKey(line.Key))
                {
                    output.Write(Colors[line.Key]);
                }
                else
                {
                    output.Write("null");
                }


                output.WriteLine(");");
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
