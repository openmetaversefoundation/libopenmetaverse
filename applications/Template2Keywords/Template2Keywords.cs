using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Template2Keywords
{
    class Template2Keywords
    {
        static List<string> keywords;
        static Dictionary<uint, string> table;

        static void Main(string[] args)
        {
            table = new Dictionary<uint, string>();
            keywords = new List<string>();

            //Read the keywords from message_template in the order they appear.
            //This is kinda sloppy because I just wanted to see if it worked.
            StreamReader freader = File.OpenText("message_template.msg");
            char[] sep = new char[2];
            sep[0] = ' ';
            sep[1] = '\t';
            while (!freader.EndOfStream)
            {
                string line = freader.ReadLine();
                int pos = line.IndexOf("//");
                if (pos != -1)
                    line = line.Substring(0, pos);

                string[] words = line.Split(sep);
                foreach (string s in words)
                    if (s.Length > 0 && char.IsUpper(s[0]))
                    {
                        keywords.Add(s);
                        break;
                    }
            }

            //Hash the keywords
            foreach (string s in keywords)
            {
                hash(s);
            }

            //Write the output, should be identical to keywords.txt
            StreamWriter fwriter = File.CreateText("output.txt");
            for (uint i = 0; i < 0x1FFF; i++)
            {
                if (table.ContainsKey(i))
                {
                    fwriter.WriteLine(table[i]);
                }
            }
            fwriter.Close();
        }

        static uint hash(string s)
        {
            uint b = 0;
            for (int k = 1; k < s.Length; k++)
            {
                b = (b + (uint)(s[k])) * 2;
            }
            b *= 2;
            b &= 0x1FFF;

            while (table.ContainsKey(b))
            {
                if (table[b] == s)
                    return b;

                b++;
                if (b > 0x1FFF)
                    return 0; //Give up looking, went past the end. (Shouldn't happen)
            }

            table[b] = s;
            return b;
        }
    }
}
