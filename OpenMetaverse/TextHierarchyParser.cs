/*
 * Copyright (c) 2008, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names 
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenMetaverse
{
    public class IndentWriter : TextWriter
    {
        private string Prefix = String.Empty;
        public string Indent = String.Empty;
        private bool AddPrefix = false;

        private StringBuilder builder = new StringBuilder();

        private void TryAddPrefix()
        {
            if (AddPrefix)
            {
                builder.Append(Prefix);
                AddPrefix = false;
            }
        }

        public override void Write(string str)
        {
            TryAddPrefix();
            builder.Append(str);
        }

        public override void Write(char c)
        {
            if (c == '}')
                Prefix.Remove(0, Indent.Length);

            TryAddPrefix();

            builder.Append(c);

            if (c == '{')
                Prefix += Indent;

        }

        public override void Write(object o)
        {
            TryAddPrefix();
            builder.Append(o);
        }

        public override void WriteLine(string line)
        {
            TryAddPrefix();
            builder.AppendLine(line);
            AddPrefix = true;
        }

        public override void WriteLine(char c)
        {
            if (c == '}')
                Prefix = Prefix.Remove(0, Indent.Length);

            TryAddPrefix();
            builder.Append(c);
            builder.AppendLine();

            if (c == '{')
                Prefix += Indent;

            AddPrefix = true;
        }

        public override void WriteLine(object o)
        {
            TryAddPrefix();
            builder.Append(o);
            builder.AppendLine();
            AddPrefix = true;
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public override string ToString()
        {
            return builder.ToString();
        }
    }

    public class TextData
    {
        public string Name;
        public string Value;
        public Dictionary<string, TextData> Nested = new Dictionary<string, TextData>();
        public void ParseLine(string line)
        {
            int firstSpace = line.IndexOfAny(TextHierarchyParser.WordSeperators);
            Name = line.Substring(0, firstSpace);
            Value = line.Substring(firstSpace + 1).Trim();
        }

        public override string ToString()
        {
            IndentWriter writer = new IndentWriter();
            writer.Indent = "\t";
            ToString(writer);
            return writer.ToString();
        }

        public void ToString(TextWriter sink)
        {
            sink.Write(Name);
            sink.Write('\t');
            sink.WriteLine(Value);
            if (Nested.Count > 0)
            {
                sink.WriteLine('{');
                foreach (TextData child in Nested.Values)
                {
                    child.ToString(sink);
                }
                sink.WriteLine('}');
            }
        }
    }

    public class TextHierarchyParser
    {
        protected internal static readonly char[] WordSeperators = new char[] { ' ', '\t' };

        public static TextData Parse(TextReader source)
        {
            string prevLine = null;
            string startLine = source.ReadLine().Trim();
            while (startLine[0] != '{')
            {
                prevLine = startLine;
                startLine = source.ReadLine().Trim();
            }

            TextData startParent = new TextData();
            if (prevLine != null)
                startParent.ParseLine(prevLine);
            ParseNested(startParent, source);
            return startParent;
        }

        private static void ParseNested(TextData parent, TextReader source)
        {
            string line = null;
            TextData current = null;
            do
            {
                line = source.ReadLine().Trim();
                if (line.Length > 0)
                {
                    if (line[0] == '{')
                    {
                        if (current == null)
                            current = new TextData();
                        ParseNested(current, source);
                    }
                    else if (line[0] == '}')
                    {
                        return;
                    }
                    else
                    {
                        current = new TextData();
                        current.ParseLine(line);
                    }
                }

                if (current != null)
                    parent.Nested[current.Name] = current;
            } while (line != null);
        }
    }
}
