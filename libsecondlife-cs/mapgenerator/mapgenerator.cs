using System;
using System.Text;
using System.IO;
using libsecondlife;

namespace mapgenerator
{
    class mapgenerator
    {
        static void WriteFieldMember(MapField field)
        {
            string type = "";

            switch (field.Type)
            {
                case FieldType.BOOL:
                    type = "bool";
                    break;
                case FieldType.F32:
                    type = "float";
                    break;
                case FieldType.F64:
                    type = "double";
                    break;
                case FieldType.IPPORT:
                case FieldType.U16:
                    type = "ushort";
                    break;
                case FieldType.IPADDR:
                case FieldType.U32:
                    type = "uint";
                    break;
                case FieldType.LLQuaternion:
                    type = "LLQuaternion";
                    break;
                case FieldType.LLUUID:
                    type = "LLUUID";
                    break;
                case FieldType.LLVector3:
                    type = "LLVector3";
                    break;
                case FieldType.LLVector3d:
                    type = "LLVector3d";
                    break;
                case FieldType.LLVector4:
                    type = "LLVector4";
                    break;
                case FieldType.S16:
                    type = "short";
                    break;
                case FieldType.S32:
                    type = "int";
                    break;
                case FieldType.S8:
                    type = "sbyte";
                    break;
                case FieldType.U64:
                    type = "ulong";
                    break;
                case FieldType.U8:
                    type = "byte";
                    break;
                case FieldType.Fixed:
                    type = "byte[]";
                    break;
            }
            if (field.Type != FieldType.Variable)
            {
                Console.WriteLine("            /// <summary>" + field.Name + " field</summary>");
                Console.WriteLine("            public " + type + " " + field.Name + ";");
            }
            else
            {
                Console.WriteLine("            private byte[] _" + field.Name.ToLower() + ";");
                Console.WriteLine("            /// <summary>" + field.Name + " field</summary>");
                Console.WriteLine("            public byte[] " + field.Name + "\n            {");
                Console.WriteLine("                get { return _" + field.Name.ToLower() + "; }");
                Console.WriteLine("                set\n                {");
                Console.WriteLine("                    if (value == null) { _" + 
                    field.Name.ToLower() + " = null; return; }");
                Console.WriteLine("                    if (value.Length > " + 
                    ((field.Count == 1) ? "255" : "1024") + ") { throw new OverflowException(" + 
                    "\"Value exceeds " + ((field.Count == 1) ? "255" : "1024") + " characters\"); }");
                Console.WriteLine("                    else { _" + field.Name.ToLower() + 
                    " = new byte[value.Length]; Array.Copy(value, _" + 
                    field.Name.ToLower() + ", value.Length); }");
                Console.WriteLine("                }\n            }");
            }
        }

        static void WriteFieldFromBytes(MapField field)
        {
            switch (field.Type)
            {
                case FieldType.BOOL:
                    Console.WriteLine("                    " +
                        field.Name + " = (bytes[i++] != 0) ? (bool)true : (bool)false;");
                    break;
                case FieldType.F32:
                    Console.WriteLine("                    " + 
                        "if (!BitConverter.IsLittleEndian) Array.Reverse(bytes, i, 4);");
                    Console.WriteLine("                    " +
                        field.Name + " = BitConverter.ToSingle(bytes, i); i += 4;");
                    break;
                case FieldType.F64:
                    Console.WriteLine("                    " +
                        "if (!BitConverter.IsLittleEndian) Array.Reverse(bytes, i, 8);");
                    Console.WriteLine("                    " +
                        field.Name + " = BitConverter.ToDouble(bytes, i); i += 8;");
                    break;
                case FieldType.Fixed:
                    Console.WriteLine("                    " + field.Name + " = new byte[" + field.Count + "];");
                    Console.WriteLine("                    Array.Copy(bytes, i, " + field.Name +
                        ", 0, " + field.Count + "); i += " + field.Count + ";");
                    break;
                case FieldType.IPADDR:
                case FieldType.U32:
                    Console.WriteLine("                    " + field.Name + 
                        " = (uint)(bytes[i++] + (bytes[i++] << 8) + (bytes[i++] << 16) + (bytes[i++] << 24));");
                    break;
                case FieldType.IPPORT:
                    // IPPORT is big endian while U16/S16 are little endian. Go figure
                    Console.WriteLine("                    " + field.Name +
                        " = (ushort)((bytes[i++] << 8) + bytes[i++]);");
                    break;
                case FieldType.U16:
                    Console.WriteLine("                    " + field.Name + 
                        " = (ushort)(bytes[i++] + (bytes[i++] << 8));");
                    break;
                case FieldType.LLQuaternion:
                    //Console.WriteLine("                    " + field.Name + 
                    //    " = new LLQuaternion(bytes, i); i += 16;");
                    Console.WriteLine("                    " + field.Name +
                        " = new LLQuaternion(bytes, i, true); i += 12;");
                    break;
                case FieldType.LLUUID:
                    Console.WriteLine("                    " + field.Name +
                        " = new LLUUID(bytes, i); i += 16;");
                    break;
                case FieldType.LLVector3:
                    Console.WriteLine("                    " + field.Name +
                        " = new LLVector3(bytes, i); i += 12;");
                    break;
                case FieldType.LLVector3d:
                    Console.WriteLine("                    " + field.Name +
                        " = new LLVector3d(bytes, i); i += 24;");
                    break;
                case FieldType.LLVector4:
                    Console.WriteLine("                    " + field.Name +
                        " = new LLVector4(bytes, i); i += 16;");
                    break;
                case FieldType.S16:
                    Console.WriteLine("                    " + field.Name +
                        " = (short)(bytes[i++] + (bytes[i++] << 8));");
                    break;
                case FieldType.S32:
                    Console.WriteLine("                    " + field.Name +
                        " = (int)(bytes[i++] + (bytes[i++] << 8) + (bytes[i++] << 16) + (bytes[i++] << 24));");
                    break;
                case FieldType.S8:
                    Console.WriteLine("                    " + field.Name +
                        " = (sbyte)bytes[i++];");
                    break;
                case FieldType.U64:
                    Console.WriteLine("                    " + field.Name +
                        " = (ulong)((ulong)bytes[i++] + ((ulong)bytes[i++] << 8) + " +
                        "((ulong)bytes[i++] << 16) + ((ulong)bytes[i++] << 24) + " +
                        "((ulong)bytes[i++] << 32) + ((ulong)bytes[i++] << 40) + " +
                        "((ulong)bytes[i++] << 48) + ((ulong)bytes[i++] << 56));");
                    break;
                case FieldType.U8:
                    Console.WriteLine("                    " + field.Name +
                        " = (byte)bytes[i++];");
                    break;
                case FieldType.Variable:
                    if (field.Count == 1)
                    {
                        Console.WriteLine("                    length = (ushort)bytes[i++];");
                    }
                    else
                    {
                        Console.WriteLine("                    length = (ushort)(bytes[i++] + (bytes[i++] << 8));");
                    }
                    Console.WriteLine("                    _" + field.Name.ToLower() + " = new byte[length];");
                    Console.WriteLine("                    Array.Copy(bytes, i, _" + field.Name.ToLower() +
                        ", 0, length); i += length;");
                    break;
                default:
                    Console.WriteLine("!!! ERROR: Unhandled FieldType: " + field.Type.ToString() + " !!!");
                    break;
            }
        }

        static void WriteFieldToBytes(MapField field)
        {
            Console.Write("                ");

            switch (field.Type)
            {
                case FieldType.BOOL:
                    Console.WriteLine("bytes[i++] = (byte)((" + field.Name + ") ? 1 : 0);");
                    break;
                case FieldType.F32:
                    Console.WriteLine("ba = BitConverter.GetBytes(" + field.Name + ");\n" +
                        "                if(!BitConverter.IsLittleEndian) { Array.Reverse(ba, 0, 4); }\n" +
                        "                Array.Copy(ba, 0, bytes, i, 4); i += 4;");
                    break;
                case FieldType.F64:
                    Console.WriteLine("ba = BitConverter.GetBytes(" + field.Name + ");\n" +
                        "                if(!BitConverter.IsLittleEndian) { Array.Reverse(ba, 0, 8); }\n" +
                        "                Array.Copy(ba, 0, bytes, i, 8); i += 8;");
                    break;
                case FieldType.Fixed:
                    Console.WriteLine("                Array.Copy(" + field.Name + ", 0, bytes, i, " + field.Count + ");" + 
                        "i += " + field.Count + ";");
                    break;
                case FieldType.IPPORT:
                    // IPPORT is big endian while U16/S16 is little endian. Go figure
                    Console.WriteLine("bytes[i++] = (byte)((" + field.Name + " >> 8) % 256);");
                    Console.WriteLine("                bytes[i++] = (byte)(" + field.Name + " % 256);");
                    break;
                case FieldType.U16:
                case FieldType.S16:
                    Console.WriteLine("bytes[i++] = (byte)(" + field.Name + " % 256);");
                    Console.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 8) % 256);");
                    break;
                //case FieldType.LLQuaternion:
                case FieldType.LLUUID:
                case FieldType.LLVector4:
                    Console.WriteLine("if(" + field.Name + " == null) { Console.WriteLine(\"Warning: " + field.Name + " is null, in \" + this.GetType()); }");
                    Console.Write("                ");
                    Console.WriteLine("Array.Copy(" + field.Name + ".GetBytes(), 0, bytes, i, 16); i += 16;");
                    break;
                case FieldType.LLQuaternion:
                case FieldType.LLVector3:
                    Console.WriteLine("if(" + field.Name + " == null) { Console.WriteLine(\"Warning: " + field.Name + " is null, in \" + this.GetType()); }");
                    Console.Write("                ");
                    Console.WriteLine("Array.Copy(" + field.Name + ".GetBytes(), 0, bytes, i, 12); i += 12;");
                    break;
                case FieldType.LLVector3d:
                    Console.WriteLine("if(" + field.Name + " == null) { Console.WriteLine(\"Warning: " + field.Name + " is null, in \" + this.GetType()); }");
                    Console.Write("                ");
                    Console.WriteLine("Array.Copy(" + field.Name + ".GetBytes(), 0, bytes, i, 24); i += 24;");
                    break;
                case FieldType.U8:
                    Console.WriteLine("bytes[i++] = " + field.Name + ";");
                    break;
                case FieldType.S8:
                    Console.WriteLine("bytes[i++] = (byte)" + field.Name + ";");
                    break;
                case FieldType.IPADDR:
                case FieldType.U32:
                case FieldType.S32:
                    Console.WriteLine("bytes[i++] = (byte)(" + field.Name + " % 256);");
                    Console.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 8) % 256);");
                    Console.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 16) % 256);");
                    Console.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 24) % 256);");
                    break;
                case FieldType.U64:
                    Console.WriteLine("bytes[i++] = (byte)(" + field.Name + " % 256);");
                    Console.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 8) % 256);");
                    Console.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 16) % 256);");
                    Console.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 24) % 256);");
                    Console.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 32) % 256);");
                    Console.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 40) % 256);");
                    Console.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 48) % 256);");
                    Console.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 56) % 256);");
                    break;
                case FieldType.Variable:
                    Console.WriteLine("if(" + field.Name + " == null) { Console.WriteLine(\"Warning: " + field.Name + " is null, in \" + this.GetType()); }");
                    Console.Write("                ");
                    if (field.Count == 1)
                    {
                        Console.WriteLine("bytes[i++] = (byte)" + field.Name + ".Length;");
                    }
                    else
                    {
                        Console.WriteLine("bytes[i++] = (byte)(" + field.Name + ".Length % 256);");
                        Console.WriteLine("                bytes[i++] = (byte)((" + 
                            field.Name + ".Length >> 8) % 256);");
                    }
                    Console.WriteLine("                Array.Copy(" + field.Name + ", 0, bytes, i, " + 
                        field.Name + ".Length); " + "i += " + field.Name + ".Length;");
                    break;
                default:
                    Console.WriteLine("!!! ERROR: Unhandled FieldType: " + field.Type.ToString() + " !!!");
                    break;
            }
        }

        static int GetFieldLength(MapField field)
        {
            switch(field.Type)
            {
                case FieldType.BOOL:
                case FieldType.U8:
                case FieldType.S8:
                    return 1;
                case FieldType.U16:
                case FieldType.S16:
                case FieldType.IPPORT:
                    return 2;
                case FieldType.U32:
                case FieldType.S32:
                case FieldType.F32:
                case FieldType.IPADDR:
                    return 4;
                case FieldType.U64:
                case FieldType.F64:
                    return 8;
                case FieldType.LLVector3:
                case FieldType.LLQuaternion:
                    return 12;
                case FieldType.LLUUID:
                case FieldType.LLVector4:
                //case FieldType.LLQuaternion:
                    return 16;
                case FieldType.LLVector3d:
                    return 24;
                case FieldType.Fixed:
                    return field.Count;
                case FieldType.Variable:
                    return 0;
                default:
                    Console.WriteLine("!!! ERROR: Unhandled FieldType " + field.Type.ToString() + " !!!");
                    return 0;
            }
        }

        static void WriteBlockClass(MapBlock block)
        {
            bool variableFields = false;
            bool floatFields = false;

            Console.WriteLine("        /// <summary>" + block.Name + " block</summary>");
            Console.WriteLine("        public class " + block.Name + "Block\n        {");

            foreach (MapField field in block.Fields)
            {
                WriteFieldMember(field);

                if (field.Type == FieldType.Variable) { variableFields = true; }
                if (field.Type == FieldType.F32 || field.Type == FieldType.F64) { floatFields = true; }
            }

            // Length property
            Console.WriteLine("");
            Console.WriteLine("            /// <summary>Length of this block serialized in bytes</summary>");
            Console.WriteLine("            public int Length\n            {\n                get\n" +
                "                {");
            int length = 0;
            foreach (MapField field in block.Fields)
            {
                length += GetFieldLength(field);
            }

            if (!variableFields)
            {
                Console.WriteLine("                    return " + length + ";");
            }
            else
            {
                Console.WriteLine("                    int length = " + length + ";");

                foreach (MapField field in block.Fields)
                {
                    if (field.Type == FieldType.Variable)
                    {
                        Console.WriteLine("                    if (" + field.Name +
                            " != null) { length += " + field.Count + " + " + field.Name + ".Length; }");
                    }
                }

                Console.WriteLine("                    return length;");
            }

            Console.WriteLine("                }\n            }\n");

            // Default constructor
            Console.WriteLine("            /// <summary>Default constructor</summary>");
            Console.WriteLine("            public " + block.Name + "Block() { }");

            // Constructor for building the class from bytes
            Console.WriteLine("            /// <summary>Constructor for building the block from a byte array</summary>");
            Console.WriteLine("            public " + block.Name + "Block(byte[] bytes, ref int i)" +
                "\n            {");

            // Declare a length variable if we need it for variable fields in this constructor
            if (variableFields) { Console.WriteLine("                int length;"); }

            // Start of the try catch block
            Console.WriteLine("                try\n                {");

            foreach (MapField field in block.Fields)
            {
                WriteFieldFromBytes(field);
            }

            Console.WriteLine("                }\n                catch (Exception)\n" +
                "                {\n                    throw new MalformedDataException();\n" +
                "                }\n            }\n");

            // ToBytes() function
            Console.WriteLine("            /// <summary>Serialize this block to a byte array</summary>");
            Console.WriteLine("            public void ToBytes(byte[] bytes, ref int i)\n            {");

            // Declare a byte[] variable if we need it for floating point field conversions
            if (floatFields) { Console.WriteLine("                byte[] ba;"); }

            foreach (MapField field in block.Fields)
            {
                WriteFieldToBytes(field);
            }

            Console.WriteLine("            }\n");

            // ToString() function
            Console.WriteLine("            /// <summary>Serialize this block to a string</summary><returns>A string containing the serialized block</returns>");
            Console.WriteLine("            public override string ToString()\n            {");
            Console.WriteLine("                string output = \"-- " + block.Name + " --\\n\";");

            foreach (MapField field in block.Fields)
            {
                if (field.Type == FieldType.Variable)
                {
                    Console.WriteLine("                output += \"" + field.Name + ": \" + Helpers.FieldToString(" + field.Name + ", \"" + field.Name + "\") + \"\\n\";");
                }
                else if (field.Type == FieldType.Fixed)
                {
                    Console.WriteLine("                output += \"" + field.Name + ": \" + Helpers.FieldToString(" + field.Name + ", \"" + field.Name + "\") + \"\\n\";");
                }
                else
                {
                    Console.WriteLine("                output += \"" + field.Name + ": \" + " + field.Name + ".ToString() + \"\\n\";");
                }
            }

            Console.WriteLine("                output = output.Trim();\n                return output;\n            }");

            Console.WriteLine("        }\n");
        }

        static void WritePacketClass(MapPacket packet)
        {
            string sanitizedName;

            Console.WriteLine("    /// <summary>" + packet.Name + " packet</summary>");
            Console.WriteLine("    public class " + packet.Name + "Packet : Packet\n    {");

            // Write out each block class
            foreach (MapBlock block in packet.Blocks)
            {
                WriteBlockClass(block);
            }

            // Header member
            Console.WriteLine("        private Header header;");
            Console.WriteLine("        /// <summary>The header for this packet</summary>");
            Console.WriteLine("        public override Header Header { get { return header; } set { header = value; } }");

            // PacketType member
            Console.WriteLine("        /// <summary>Will return PacketType." + packet.Name+ "</summary>");
            Console.WriteLine("        public override PacketType Type { get { return PacketType." + 
                packet.Name + "; } }");

            // Block members
            foreach (MapBlock block in packet.Blocks)
            {
                // TODO: More thorough name blacklisting
                if (block.Name == "Header") { sanitizedName = "_" + block.Name; }
                else { sanitizedName = block.Name; }

                Console.WriteLine("        /// <summary>" + block.Name + " block</summary>");
                Console.WriteLine("        public " + block.Name + "Block" +
                    ((block.Count != 1) ? "[]" : "") + " " + sanitizedName + ";");
            }

            Console.WriteLine("");

            // Default constructor
            Console.WriteLine("        /// <summary>Default constructor</summary>");
            Console.WriteLine("        public " + packet.Name + "Packet()\n        {");
            Console.WriteLine("            Header = new " + packet.Frequency.ToString() + "Header();");
            Console.WriteLine("            Header.ID = " + packet.ID + ";");
            Console.WriteLine("            Header.Reliable = true;"); // Turn the reliable flag on by default
            if (packet.Encoded) { Console.WriteLine("            Header.Zerocoded = true;"); }
            foreach (MapBlock block in packet.Blocks)
            {
                if (block.Name == "Header") { sanitizedName = "_" + block.Name; }
                else { sanitizedName = block.Name; }

                if (block.Count == 1)
                {
                    // Single count block
                    Console.WriteLine("            " + sanitizedName + " = new " + block.Name + "Block();");
                }
                else if (block.Count == -1)
                {
                    // Variable count block
                    Console.WriteLine("            " + sanitizedName + " = new " + block.Name + "Block[0];");
                }
                else
                {
                    // Multiple count block
                    Console.WriteLine("            " + sanitizedName + " = new " + block.Name + 
                        "Block[" + block.Count + "];");
                }
            }
            Console.WriteLine("        }\n");

            // Constructor that takes a byte array and beginning position only (no prebuilt header)
            bool seenVariable = false;
            Console.WriteLine("        /// <summary>Constructor that takes a byte array and beginning position (no prebuilt header)</summary>");
            Console.WriteLine("        public " + packet.Name + "Packet(byte[] bytes, ref int i)\n        {");
            Console.WriteLine("            int packetEnd = bytes.Length - 1;");
            Console.WriteLine("            Header = new " + packet.Frequency.ToString() + 
                "Header(bytes, ref i, ref packetEnd);");
            foreach (MapBlock block in packet.Blocks)
            {
                if (block.Name == "Header") { sanitizedName = "_" + block.Name; }
                else { sanitizedName = block.Name; }

                if (block.Count == 1)
                {
                    // Single count block
                    Console.WriteLine("            " + sanitizedName + " = new " + block.Name + "Block(bytes, ref i);");
                }
                else if (block.Count == -1)
                {
                    // Variable count block
                    if (!seenVariable)
                    {
                        Console.WriteLine("            int count = (int)bytes[i++];");
                        seenVariable = true;
                    }
                    else
                    {
                        Console.WriteLine("            count = (int)bytes[i++];");
                    }
                    Console.WriteLine("            " + sanitizedName + " = new " + block.Name + "Block[count];");
                    Console.WriteLine("            for (int j = 0; j < count; j++)");
                    Console.WriteLine("            { " + sanitizedName + "[j] = new " +
                        block.Name + "Block(bytes, ref i); }");
                }
                else
                {
                    // Multiple count block
                    Console.WriteLine("            " + sanitizedName + " = new " + block.Name + 
                        "Block[" + block.Count + "];");
                    Console.WriteLine("            for (int j = 0; j < " + block.Count + "; j++)");
                    Console.WriteLine("            { " + sanitizedName + "[j] = new " + 
                        block.Name + "Block(bytes, ref i); }");
                }
            }
            Console.WriteLine("        }\n");

            seenVariable = false;

            // Constructor that takes a byte array and a prebuilt header
            Console.WriteLine("        /// <summary>Constructor that takes a byte array and a prebuilt header</summary>");
            Console.WriteLine("        public " + packet.Name + "Packet(Header head, byte[] bytes, ref int i)\n        {");
            Console.WriteLine("            Header = head;");
            foreach (MapBlock block in packet.Blocks)
            {
                if (block.Name == "Header") { sanitizedName = "_" + block.Name; }
                else { sanitizedName = block.Name; }

                if (block.Count == 1)
                {
                    // Single count block
                    Console.WriteLine("            " + sanitizedName + " = new " + block.Name + "Block(bytes, ref i);");
                }
                else if (block.Count == -1)
                {
                    // Variable count block
                    if (!seenVariable)
                    {
                        Console.WriteLine("            int count = (int)bytes[i++];");
                        seenVariable = true;
                    }
                    else
                    {
                        Console.WriteLine("            count = (int)bytes[i++];");
                    }
                    Console.WriteLine("            " + sanitizedName + " = new " + block.Name + "Block[count];");
                    Console.WriteLine("            for (int j = 0; j < count; j++)");
                    Console.WriteLine("            { " + sanitizedName + "[j] = new " +
                        block.Name + "Block(bytes, ref i); }");
                }
                else
                {
                    // Multiple count block
                    Console.WriteLine("            " + sanitizedName + " = new " + block.Name +
                        "Block[" + block.Count + "];");
                    Console.WriteLine("            for (int j = 0; j < " + block.Count + "; j++)");
                    Console.WriteLine("            { " + sanitizedName + "[j] = new " +
                        block.Name + "Block(bytes, ref i); }");
                }
            }
            Console.WriteLine("        }\n");

            // ToBytes() function
            Console.WriteLine("        /// <summary>Serialize this packet to a byte array</summary><returns>A byte array containing the serialized packet</returns>");
            Console.WriteLine("        public override byte[] ToBytes()\n        {");

            Console.Write("            int length = ");
            if (packet.Frequency == PacketFrequency.Low) { Console.WriteLine("8;"); }
            else if (packet.Frequency == PacketFrequency.Medium) { Console.WriteLine("6;"); }
            else { Console.WriteLine("5;"); }

            foreach (MapBlock block in packet.Blocks)
            {
                if (block.Name == "Header") { sanitizedName = "_" + block.Name; }
                else { sanitizedName = block.Name; }

                if (block.Count == 1)
                {
                    // Single count block
                    Console.Write("            length += " + sanitizedName + ".Length;");
                }
            }
            Console.WriteLine(";");

            foreach (MapBlock block in packet.Blocks)
            {
                if (block.Name == "Header") { sanitizedName = "_" + block.Name; }
                else { sanitizedName = block.Name; }

                if (block.Count == -1)
                {
                    Console.WriteLine("            length++;");
                    Console.WriteLine("            for (int j = 0; j < " + sanitizedName +
                        ".Length; j++) { length += " + sanitizedName + "[j].Length; }");
                }
                else if (block.Count > 1)
                {
                    Console.WriteLine("            for (int j = 0; j < " + block.Count +
                        "; j++) { length += " + sanitizedName + "[j].Length; }");
                }
            }

            Console.WriteLine("            if (header.AckList.Length > 0) { length += header.AckList.Length * 4 + 1; }");
            Console.WriteLine("            byte[] bytes = new byte[length];");
            Console.WriteLine("            int i = 0;");
            Console.WriteLine("            header.ToBytes(bytes, ref i);");
            foreach (MapBlock block in packet.Blocks)
            {
                if (block.Name == "Header") { sanitizedName = "_" + block.Name; }
                else { sanitizedName = block.Name; }

                if (block.Count == -1)
                {
                    // Variable count block
                    Console.WriteLine("            bytes[i++] = (byte)" + sanitizedName + ".Length;");
                    Console.WriteLine("            for (int j = 0; j < " + sanitizedName +
                        ".Length; j++) { " + sanitizedName + "[j].ToBytes(bytes, ref i); }");
                }
                else if (block.Count == 1)
                {
                    Console.WriteLine("            " + sanitizedName + ".ToBytes(bytes, ref i);");
                }
                else
                {
                    // Multiple count block
                    Console.WriteLine("            for (int j = 0; j < " + block.Count +
                        "; j++) { " + sanitizedName + "[j].ToBytes(bytes, ref i); }");
                }
            }

            Console.WriteLine("            if (header.AckList.Length > 0) { header.AcksToBytes(bytes, ref i); }");
            Console.WriteLine("            return bytes;\n        }\n");

            // ToString() function
            Console.WriteLine("        /// <summary>Serialize this packet to a string</summary><returns>A string containing the serialized packet</returns>");
            Console.WriteLine("        public override string ToString()\n        {");
            Console.WriteLine("            string output = \"--- " + packet.Name + " ---\\n\";");

            foreach (MapBlock block in packet.Blocks)
            {
                if (block.Name == "Header") { sanitizedName = "_" + block.Name; }
                else { sanitizedName = block.Name; }

                if (block.Count == -1)
                {
                    // Variable count block
                    Console.WriteLine("            for (int j = 0; j < " + sanitizedName + ".Length; j++)\n            {");
                    Console.WriteLine("                output += " + sanitizedName + "[j].ToString() + \"\\n\";\n            }");
                }
                else if (block.Count == 1)
                {
                    Console.WriteLine("                output += " + sanitizedName + ".ToString() + \"\\n\";");
                }
                else
                {
                    // Multiple count block
                    Console.WriteLine("            for (int j = 0; j < " + block.Count + "; j++)\n            {");
                    Console.WriteLine("                output += " + sanitizedName + "[j].ToString() + \"\\n\";\n            }");
                }
            }

            Console.WriteLine("            return output;\n        }\n");

            // Closing function bracket
            Console.WriteLine("    }\n");
        }

        static void Main(string[] args)
        {
            SecondLife libsl = new SecondLife();
            ProtocolManager protocol = new ProtocolManager("keywords.txt", "message_template.msg", libsl);

            TextReader reader = new StreamReader("template.cs");
            Console.WriteLine(reader.ReadToEnd());
            reader.Close();

            // Write the PacketType enum
            Console.WriteLine("    /// <summary>Used to identify the type of a packet</summary>");
            Console.WriteLine("    public enum PacketType\n    {\n" +
                "        /// <summary>A generic value, not an actual packet type</summary>\n" +
                "        Default,");
            foreach (MapPacket packet in protocol.LowMaps)
            {
                if (packet != null)
                {
                    Console.WriteLine("        /// <summary>" + packet.Name + "</summary>");
                    Console.WriteLine("        " + packet.Name + ",");
                }
            }
            foreach (MapPacket packet in protocol.MediumMaps)
            {
                if (packet != null)
                {
                    Console.WriteLine("        /// <summary>" + packet.Name + "</summary>");
                    Console.WriteLine("        " + packet.Name + ",");
                }
            }
            foreach (MapPacket packet in protocol.HighMaps)
            {
                if (packet != null)
                {
                    Console.WriteLine("        /// <summary>" + packet.Name + "</summary>");
                    Console.WriteLine("        " + packet.Name + ",");
                }
            }
            Console.WriteLine("    }\n");

            // Write the base Packet class
            Console.WriteLine("    /// <summary>Base class for all packet classes</summary>\n" +
                "    public abstract class Packet\n    {\n" + 
                "        /// <summary>Either a LowHeader, MediumHeader, or HighHeader representing the first bytes of the packet</summary>\n" +
                "        public abstract Header Header { get; set; }\n" +
                "        /// <summary>The type of this packet, identified by it's frequency and ID</summary>\n" +
                "        public abstract PacketType Type { get; }\n" +
                "        /// <summary>Used internally to track timeouts, do not use</summary>\n" +
                "        public int TickCount;\n\n" +
                "        /// <summary>Serializes the packet in to a byte array</summary>\n" +
                "        /// <returns>A byte array containing the serialized packet payload, ready to be sent across the wire</returns>\n" +
                "        public abstract byte[] ToBytes();\n\n" +
                "        /// <summary>Get the PacketType for a given packet id and packet frequency</summary>\n" +
                "        /// <param name=\"id\">The packet ID from the header</param>\n" +
                "        /// <param name=\"frequency\">Frequency of this packet</param>\n" +
                "        /// <returns>The packet type, or PacketType.Default</returns>\n" +
                "        public static PacketType GetType(ushort id, PacketFrequency frequency)\n        {\n" +
                "            switch (frequency)\n            {\n                case PacketFrequency.Low:\n" +
                "                    switch (id)\n                    {");

            foreach (MapPacket packet in protocol.LowMaps)
            {
                if (packet != null)
                {
                    Console.WriteLine("                        case " + packet.ID + 
                        ": return PacketType." + packet.Name + ";");
                }
            }

            Console.WriteLine("                    }\n                    break;\n" +
                "                case PacketFrequency.Medium:\n                    switch (id)\n                    {");

            foreach (MapPacket packet in protocol.MediumMaps)
            {
                if (packet != null)
                {
                    Console.WriteLine("                        case " + packet.ID +
                        ": return PacketType." + packet.Name + ";");
                }
            }

            Console.WriteLine("                    }\n                    break;\n" +
                "                case PacketFrequency.High:\n                    switch (id)\n                    {");

            foreach (MapPacket packet in protocol.HighMaps)
            {
                if (packet != null)
                {
                    Console.WriteLine("                        case " + packet.ID +
                        ": return PacketType." + packet.Name + ";");
                }
            }

            Console.WriteLine("                    }\n                    break;\n            }\n\n" +
                "            return PacketType.Default;\n        }\n");

            Console.WriteLine("        /// <summary>Construct a packet in it's native class from a byte array</summary>\n" +
                "        /// <param name=\"bytes\">Byte array containing the packet, starting at position 0</param>\n" +
                "        /// <param name=\"packetEnd\">The last byte of the packet. If the packet was 76 bytes long, packetEnd would be 75</param>\n" +
                "        /// <returns>The native packet class for this type of packet, typecasted to the generic Packet</returns>\n" +
                "        public static Packet BuildPacket(byte[] bytes, ref int packetEnd)\n" +
                "        {\n            ushort id;\n            int i = 0;\n" +
                "            Header header = Header.BuildHeader(bytes, ref i, ref packetEnd);\n" +
                "            if (header.Zerocoded)\n            {\n" +
                "                byte[] zeroBuffer = new byte[8192];\n" +
                "                packetEnd = Helpers.ZeroDecode(bytes, packetEnd + 1, zeroBuffer) - 1;\n" +
                "                bytes = zeroBuffer;\n            }\n\n" + 
                "            if (bytes[4] == 0xFF)\n            {\n" +
                "                if (bytes[5] == 0xFF)\n                {\n" +
                "                    id = (ushort)((bytes[6] << 8) + bytes[7]);\n" +
                "                    switch (id)\n                    {");
            foreach (MapPacket packet in protocol.LowMaps)
            {
                if (packet != null)
                {
                    Console.WriteLine("                        case " + packet.ID + 
                        ": return new " + packet.Name + "Packet(header, bytes, ref i);");
                }
            }
            Console.WriteLine("                    }\n                }\n                else\n" +
                "                {\n                    id = (ushort)bytes[5];\n" +
                "                    switch (id)\n                    {");
            foreach (MapPacket packet in protocol.MediumMaps)
            {
                if (packet != null)
                {
                    Console.WriteLine("                        case " + packet.ID + 
                        ": return new " + packet.Name + "Packet(header, bytes, ref i);");
                }
            }
            Console.WriteLine("                    }\n                }\n            }\n" + 
                "            else\n            {\n" + 
                "                id = (ushort)bytes[4];\n" + 
                "                switch (id)\n                    {");
            foreach (MapPacket packet in protocol.HighMaps)
            {
                if (packet != null)
                {
                    Console.WriteLine("                        case " + packet.ID + 
                        ": return new " + packet.Name + "Packet(header, bytes, ref i);");
                }
            }
            Console.WriteLine("                }\n            }\n\n" +
                "            throw new MalformedDataException(\"Unknown packet ID\");\n" + 
                "        }\n    }\n");

            // Write the packet classes
            foreach (MapPacket packet in protocol.LowMaps)
            {
                if (packet != null) { WritePacketClass(packet); }
            }

            foreach (MapPacket packet in protocol.MediumMaps)
            {
                if (packet != null) { WritePacketClass(packet); }
            }

            foreach (MapPacket packet in protocol.HighMaps)
            {
                if (packet != null) { WritePacketClass(packet); }
            }

            Console.WriteLine("}");
        }
    }
}
