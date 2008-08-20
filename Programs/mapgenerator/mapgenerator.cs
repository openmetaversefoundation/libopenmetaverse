using System;
using System.Collections.Generic;
using System.IO;

namespace mapgenerator
{
    class mapgenerator
    {
        static void WriteFieldMember(TextWriter writer, MapField field)
        {
            string type = String.Empty;

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
                    type = "Quaternion";
                    break;
                case FieldType.LLUUID:
                    type = "UUID";
                    break;
                case FieldType.LLVector3:
                    type = "Vector3";
                    break;
                case FieldType.LLVector3d:
                    type = "Vector3d";
                    break;
                case FieldType.LLVector4:
                    type = "Vector4";
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
                //writer.WriteLine("            /// <summary>" + field.Name + " field</summary>");
                writer.WriteLine("            public " + type + " " + field.Name + ";");
            }
            else
            {
                writer.WriteLine("            private byte[] _" + field.Name.ToLower() + ";");
                //writer.WriteLine("            /// <summary>" + field.Name + " field</summary>");
                writer.WriteLine("            public byte[] " + field.Name + Environment.NewLine + "            {");
                writer.WriteLine("                get { return _" + field.Name.ToLower() + "; }");
                writer.WriteLine("                set" + Environment.NewLine + "                {");
                writer.WriteLine("                    if (value == null) { _" + 
                    field.Name.ToLower() + " = null; return; }");
                writer.WriteLine("                    if (value.Length > " + 
                    ((field.Count == 1) ? "255" : "1100") + ") { throw new OverflowException(" + 
                    "\"Value exceeds " + ((field.Count == 1) ? "255" : "1100") + " characters\"); }");
                writer.WriteLine("                    else { _" + field.Name.ToLower() +
                    " = new byte[value.Length]; Buffer.BlockCopy(value, 0, _" + 
                    field.Name.ToLower() + ", 0, value.Length); }");
                writer.WriteLine("                }" + Environment.NewLine + "            }");
            }
        }

        static void WriteFieldFromBytes(TextWriter writer, MapField field)
        {
            switch (field.Type)
            {
                case FieldType.BOOL:
                    writer.WriteLine("                    " +
                        field.Name + " = (bytes[i++] != 0) ? (bool)true : (bool)false;");
                    break;
                case FieldType.F32:
                    writer.WriteLine("                    " + 
                        "if (!BitConverter.IsLittleEndian) Array.Reverse(bytes, i, 4);");
                    writer.WriteLine("                    " +
                        field.Name + " = BitConverter.ToSingle(bytes, i); i += 4;");
                    break;
                case FieldType.F64:
                    writer.WriteLine("                    " +
                        "if (!BitConverter.IsLittleEndian) Array.Reverse(bytes, i, 8);");
                    writer.WriteLine("                    " +
                        field.Name + " = BitConverter.ToDouble(bytes, i); i += 8;");
                    break;
                case FieldType.Fixed:
                    writer.WriteLine("                    " + field.Name + " = new byte[" + field.Count + "];");
                    writer.WriteLine("                    Buffer.BlockCopy(bytes, i, " + field.Name +
                        ", 0, " + field.Count + "); i += " + field.Count + ";");
                    break;
                case FieldType.IPADDR:
                case FieldType.U32:
                    writer.WriteLine("                    " + field.Name + 
                        " = (uint)(bytes[i++] + (bytes[i++] << 8) + (bytes[i++] << 16) + (bytes[i++] << 24));");
                    break;
                case FieldType.IPPORT:
                    // IPPORT is big endian while U16/S16 are little endian. Go figure
                    writer.WriteLine("                    " + field.Name +
                        " = (ushort)((bytes[i++] << 8) + bytes[i++]);");
                    break;
                case FieldType.U16:
                    writer.WriteLine("                    " + field.Name + 
                        " = (ushort)(bytes[i++] + (bytes[i++] << 8));");
                    break;
                case FieldType.LLQuaternion:
                    writer.WriteLine("                    " + field.Name + ".FromBytes(bytes, i, true); i += 12;");
                    break;
                case FieldType.LLUUID:
                    writer.WriteLine("                    " + field.Name + ".FromBytes(bytes, i); i += 16;");
                    break;
                case FieldType.LLVector3:
                    writer.WriteLine("                    " + field.Name + ".FromBytes(bytes, i); i += 12;");
                    break;
                case FieldType.LLVector3d:
                    writer.WriteLine("                    " + field.Name + ".FromBytes(bytes, i); i += 24;");
                    break;
                case FieldType.LLVector4:
                    writer.WriteLine("                    " + field.Name + ".FromBytes(bytes, i); i += 16;");
                    break;
                case FieldType.S16:
                    writer.WriteLine("                    " + field.Name +
                        " = (short)(bytes[i++] + (bytes[i++] << 8));");
                    break;
                case FieldType.S32:
                    writer.WriteLine("                    " + field.Name +
                        " = (int)(bytes[i++] + (bytes[i++] << 8) + (bytes[i++] << 16) + (bytes[i++] << 24));");
                    break;
                case FieldType.S8:
                    writer.WriteLine("                    " + field.Name +
                        " = (sbyte)bytes[i++];");
                    break;
                case FieldType.U64:
                    writer.WriteLine("                    " + field.Name +
                        " = (ulong)((ulong)bytes[i++] + ((ulong)bytes[i++] << 8) + " +
                        "((ulong)bytes[i++] << 16) + ((ulong)bytes[i++] << 24) + " +
                        "((ulong)bytes[i++] << 32) + ((ulong)bytes[i++] << 40) + " +
                        "((ulong)bytes[i++] << 48) + ((ulong)bytes[i++] << 56));");
                    break;
                case FieldType.U8:
                    writer.WriteLine("                    " + field.Name +
                        " = (byte)bytes[i++];");
                    break;
                case FieldType.Variable:
                    if (field.Count == 1)
                    {
                        writer.WriteLine("                    length = (ushort)bytes[i++];");
                    }
                    else
                    {
                        writer.WriteLine("                    length = (ushort)(bytes[i++] + (bytes[i++] << 8));");
                    }
                    writer.WriteLine("                    _" + field.Name.ToLower() + " = new byte[length];");
                    writer.WriteLine("                    Buffer.BlockCopy(bytes, i, _" + field.Name.ToLower() +
                        ", 0, length); i += length;");
                    break;
                default:
                    writer.WriteLine("!!! ERROR: Unhandled FieldType: " + field.Type.ToString() + " !!!");
                    break;
            }
        }

        static void WriteFieldToBytes(TextWriter writer, MapField field)
        {
            writer.Write("                ");

            switch (field.Type)
            {
                case FieldType.BOOL:
                    writer.WriteLine("bytes[i++] = (byte)((" + field.Name + ") ? 1 : 0);");
                    break;
                case FieldType.F32:
                    writer.WriteLine("ba = BitConverter.GetBytes(" + field.Name + ");" + Environment.NewLine +
                        "                if(!BitConverter.IsLittleEndian) { Array.Reverse(ba, 0, 4); }" + Environment.NewLine +
                        "                Buffer.BlockCopy(ba, 0, bytes, i, 4); i += 4;");
                    break;
                case FieldType.F64:
                    writer.WriteLine("ba = BitConverter.GetBytes(" + field.Name + ");" + Environment.NewLine +
                        "                if(!BitConverter.IsLittleEndian) { Array.Reverse(ba, 0, 8); }" + Environment.NewLine +
                        "                Buffer.BlockCopy(ba, 0, bytes, i, 8); i += 8;");
                    break;
                case FieldType.Fixed:
                    writer.WriteLine("                Buffer.BlockCopy(" + field.Name + ", 0, bytes, i, " + field.Count + ");" + 
                        "i += " + field.Count + ";");
                    break;
                case FieldType.IPPORT:
                    // IPPORT is big endian while U16/S16 is little endian. Go figure
                    writer.WriteLine("bytes[i++] = (byte)((" + field.Name + " >> 8) % 256);");
                    writer.WriteLine("                bytes[i++] = (byte)(" + field.Name + " % 256);");
                    break;
                case FieldType.U16:
                case FieldType.S16:
                    writer.WriteLine("bytes[i++] = (byte)(" + field.Name + " % 256);");
                    writer.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 8) % 256);");
                    break;
                case FieldType.LLUUID:
                    writer.WriteLine("Buffer.BlockCopy(" + field.Name + ".GetBytes(), 0, bytes, i, 16); i += 16;");
                    break;
                case FieldType.LLVector4:
                    writer.WriteLine("Buffer.BlockCopy(" + field.Name + ".GetBytes(), 0, bytes, i, 16); i += 16;");
                    break;
                case FieldType.LLQuaternion:
                case FieldType.LLVector3:
                    writer.WriteLine("Buffer.BlockCopy(" + field.Name + ".GetBytes(), 0, bytes, i, 12); i += 12;");
                    break;
                case FieldType.LLVector3d:
                    writer.WriteLine("Buffer.BlockCopy(" + field.Name + ".GetBytes(), 0, bytes, i, 24); i += 24;");
                    break;
                case FieldType.U8:
                    writer.WriteLine("bytes[i++] = " + field.Name + ";");
                    break;
                case FieldType.S8:
                    writer.WriteLine("bytes[i++] = (byte)" + field.Name + ";");
                    break;
                case FieldType.IPADDR:
                case FieldType.U32:
                case FieldType.S32:
                    writer.WriteLine("bytes[i++] = (byte)(" + field.Name + " % 256);");
                    writer.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 8) % 256);");
                    writer.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 16) % 256);");
                    writer.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 24) % 256);");
                    break;
                case FieldType.U64:
                    writer.WriteLine("bytes[i++] = (byte)(" + field.Name + " % 256);");
                    writer.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 8) % 256);");
                    writer.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 16) % 256);");
                    writer.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 24) % 256);");
                    writer.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 32) % 256);");
                    writer.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 40) % 256);");
                    writer.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 48) % 256);");
                    writer.WriteLine("                bytes[i++] = (byte)((" + field.Name + " >> 56) % 256);");
                    break;
                case FieldType.Variable:
                    writer.WriteLine("if(" + field.Name + " == null) { Console.WriteLine(\"Warning: " + field.Name + " is null, in \" + this.GetType()); }");
                    writer.Write("                ");
                    if (field.Count == 1)
                    {
                        writer.WriteLine("bytes[i++] = (byte)" + field.Name + ".Length;");
                    }
                    else
                    {
                        writer.WriteLine("bytes[i++] = (byte)(" + field.Name + ".Length % 256);");
                        writer.WriteLine("                bytes[i++] = (byte)((" + 
                            field.Name + ".Length >> 8) % 256);");
                    }
                    writer.WriteLine("                Buffer.BlockCopy(" + field.Name + ", 0, bytes, i, " + 
                        field.Name + ".Length); " + "i += " + field.Name + ".Length;");
                    break;
                default:
                    writer.WriteLine("!!! ERROR: Unhandled FieldType: " + field.Type.ToString() + " !!!");
                    break;
            }
        }

        static int GetFieldLength(TextWriter writer, MapField field)
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
                    return 16;
                case FieldType.LLVector3d:
                    return 24;
                case FieldType.Fixed:
                    return field.Count;
                case FieldType.Variable:
                    return 0;
                default:
                    writer.WriteLine("!!! ERROR: Unhandled FieldType " + field.Type.ToString() + " !!!");
                    return 0;
            }
        }

        static void WriteBlockClass(TextWriter writer, MapBlock block, MapPacket packet)
        {
            bool variableFields = false;
            bool floatFields = false;

            //writer.WriteLine("        /// <summary>" + block.Name + " block</summary>");
            writer.WriteLine("        /// <exclude/>");
            //writer.WriteLine("        [XmlType(\"" + packet.Name.ToLower() + "_" + block.Name.ToLower() + "\")]");
            writer.WriteLine("        public class " + block.Name + "Block" + Environment.NewLine + "        {");

            foreach (MapField field in block.Fields)
            {
                WriteFieldMember(writer, field);

                if (field.Type == FieldType.Variable) { variableFields = true; }
                if (field.Type == FieldType.F32 || field.Type == FieldType.F64) { floatFields = true; }
            }

            // Length property
            writer.WriteLine("");
            //writer.WriteLine("            /// <summary>Length of this block serialized in bytes</summary>");
            writer.WriteLine(//"            [XmlIgnore]" + Environment.NewLine +
                             "            public int Length" + Environment.NewLine +
                             "            {" + Environment.NewLine + 
                             "                get" + Environment.NewLine +
                             "                {");
            int length = 0;

            // Figure out the length of this block
            foreach (MapField field in block.Fields)
            {
                length += GetFieldLength(writer, field);
            }

            if (!variableFields)
            {
                writer.WriteLine("                    return " + length + ";");
            }
            else
            {
                writer.WriteLine("                    int length = " + length + ";");

                foreach (MapField field in block.Fields)
                {
                    if (field.Type == FieldType.Variable)
                    {
                        writer.WriteLine("                    if (" + field.Name +
                            " != null) { length += " + field.Count + " + " + field.Name + ".Length; }");
                    }
                }

                writer.WriteLine("                    return length;");
            }

            writer.WriteLine("                }" + Environment.NewLine + "            }" + Environment.NewLine);

            // Default constructor
            //writer.WriteLine("            /// <summary>Default constructor</summary>");
            writer.WriteLine("            public " + block.Name + "Block() { }");

            // Constructor for building the class from bytes
            //writer.WriteLine("            /// <summary>Constructor for building the block from a byte array</summary>");
            writer.WriteLine("            public " + block.Name + "Block(byte[] bytes, ref int i)" + Environment.NewLine + 
                "            {" + Environment.NewLine + 
                "                FromBytes(bytes, ref i);" + Environment.NewLine + 
                "            }" + Environment.NewLine);

            // Initiates instance variables from a byte message
            writer.WriteLine("            public void FromBytes(byte[] bytes, ref int i)" + Environment.NewLine + 
                "            {");

            // Declare a length variable if we need it for variable fields in this constructor
            if (variableFields) { writer.WriteLine("                int length;"); }

            // Start of the try catch block
            writer.WriteLine("                try" + Environment.NewLine + "                {");

            foreach (MapField field in block.Fields)
            {
                WriteFieldFromBytes(writer, field);
            }

            writer.WriteLine("                }" + Environment.NewLine + 
                "                catch (Exception)" + Environment.NewLine +
                "                {" + Environment.NewLine + 
                "                    throw new MalformedDataException();" + Environment.NewLine +
                "                }" + Environment.NewLine + "            }" + Environment.NewLine);

            // ToBytes() function
            //writer.WriteLine("            /// <summary>Serialize this block to a byte array</summary>");
            writer.WriteLine("            public void ToBytes(byte[] bytes, ref int i)" + Environment.NewLine + 
                "            {");

            // Declare a byte[] variable if we need it for floating point field conversions
            if (floatFields) { writer.WriteLine("                byte[] ba;"); }

            foreach (MapField field in block.Fields)
            {
                WriteFieldToBytes(writer, field);
            }

            writer.WriteLine("            }" + Environment.NewLine);

            // ToString() function
            writer.WriteLine("            public override string ToString()" + Environment.NewLine + "            {");
            writer.WriteLine("                StringBuilder output = new StringBuilder();");
            writer.WriteLine("                output.AppendLine(\"-- " + block.Name + " --\");");

            for (int i = 0; i < block.Fields.Count; i++)
            {
                MapField field = block.Fields[i];

                if (field.Type == FieldType.Variable || field.Type == FieldType.Fixed)
                {
                    writer.WriteLine("                Helpers.FieldToString(output, " + field.Name + ", \"" + field.Name + "\");");
                    if (i != block.Fields.Count - 1) writer.WriteLine("                output.Append(Environment.NewLine);");
                }
                else
                {
                    if (i != block.Fields.Count - 1) writer.WriteLine("                output.AppendLine(String.Format(\"" + field.Name + ": {0}\", " + field.Name + "));");
                    else writer.WriteLine("                output.Append(String.Format(\"" + field.Name + ": {0}\", " + field.Name + "));");
                }
            }

            writer.WriteLine("                return output.ToString();" + Environment.NewLine + "            }");
            writer.WriteLine("        }" + Environment.NewLine);
        }

        static void WritePacketClass(TextWriter writer, MapPacket packet)
        {
            string sanitizedName;

            //writer.WriteLine("    /// <summary>" + packet.Name + " packet</summary>");
            writer.WriteLine("    /// <exclude/>");
            writer.WriteLine("    public class " + packet.Name + "Packet : Packet" + Environment.NewLine + "    {");

            // Write out each block class
            foreach (MapBlock block in packet.Blocks)
            {
                WriteBlockClass(writer, block, packet);
            }

            // Header member
            writer.WriteLine("        private Header header;");
            //writer.WriteLine("        /// <summary>The header for this packet</summary>");
            writer.WriteLine("        public override Header Header { get { return header; } set { header = value; } }");

            // PacketType member
            //writer.WriteLine("        /// <summary>Will return PacketType." + packet.Name+ "</summary>");
            writer.WriteLine("        public override PacketType Type { get { return PacketType." + 
                packet.Name + "; } }");

            // Block members
            foreach (MapBlock block in packet.Blocks)
            {
                // TODO: More thorough name blacklisting
                if (block.Name == "Header") { sanitizedName = "_" + block.Name; }
                else { sanitizedName = block.Name; }

                //writer.WriteLine("        /// <summary>" + block.Name + " block</summary>");
                writer.WriteLine("        public " + block.Name + "Block" +
                    ((block.Count != 1) ? "[]" : "") + " " + sanitizedName + ";");
            }

            writer.WriteLine("");

            // Default constructor
            //writer.WriteLine("        /// <summary>Default constructor</summary>");
            writer.WriteLine("        public " + packet.Name + "Packet()" + Environment.NewLine + "        {");
            writer.WriteLine("            Header = new " + packet.Frequency.ToString() + "Header();");
            writer.WriteLine("            Header.ID = " + packet.ID + ";");
            writer.WriteLine("            Header.Reliable = true;"); // Turn the reliable flag on by default
            if (packet.Encoded) { writer.WriteLine("            Header.Zerocoded = true;"); }
            foreach (MapBlock block in packet.Blocks)
            {
                if (block.Name == "Header") { sanitizedName = "_" + block.Name; }
                else { sanitizedName = block.Name; }

                if (block.Count == 1)
                {
                    // Single count block
                    writer.WriteLine("            " + sanitizedName + " = new " + block.Name + "Block();");
                }
                else if (block.Count == -1)
                {
                    // Variable count block
                    writer.WriteLine("            " + sanitizedName + " = new " + block.Name + "Block[0];");
                }
                else
                {
                    // Multiple count block
                    writer.WriteLine("            " + sanitizedName + " = new " + block.Name + "Block[" + block.Count + "];");
                }
            }
            writer.WriteLine("        }" + Environment.NewLine);

            // Constructor that takes a byte array and beginning position only (no prebuilt header)
            bool seenVariable = false;
            //writer.WriteLine("        /// <summary>Constructor that takes a byte array and beginning position (no prebuilt header)</summary>");
            writer.WriteLine("        public " + packet.Name + "Packet(byte[] bytes, ref int i) : this()" + Environment.NewLine +
                "        {" + Environment.NewLine +
                "            int packetEnd = bytes.Length - 1;" + Environment.NewLine +
                "            FromBytes(bytes, ref i, ref packetEnd, null);" + Environment.NewLine +
                "        }" + Environment.NewLine);

            writer.WriteLine("        override public void FromBytes(byte[] bytes, ref int i, ref int packetEnd, byte[] zeroBuffer)" + Environment.NewLine + "        {");
            writer.WriteLine("            header.FromBytes(bytes, ref i, ref packetEnd);");
            writer.WriteLine("            if (header.Zerocoded && zeroBuffer != null)");
            writer.WriteLine("            {");
            writer.WriteLine("                packetEnd = Helpers.ZeroDecode(bytes, packetEnd + 1, zeroBuffer) - 1;");
            writer.WriteLine("                bytes = zeroBuffer;");
            writer.WriteLine("            }");

            foreach (MapBlock block in packet.Blocks)
            {
                if (block.Name == "Header") { sanitizedName = "_" + block.Name; }
                else { sanitizedName = block.Name; }

                if (block.Count == 1)
                {
                    // Single count block
                    writer.WriteLine("            " + sanitizedName + ".FromBytes(bytes, ref i);");
                }
                else if (block.Count == -1)
                {
                    // Variable count block
                    if (!seenVariable)
                    {
                        writer.WriteLine("            int count = (int)bytes[i++];");
                        seenVariable = true;
                    }
                    else
                    {
                        writer.WriteLine("            count = (int)bytes[i++];");
                    }
                    writer.WriteLine("            if(" + sanitizedName + ".Length < count) {");
                    writer.WriteLine("                " + sanitizedName + " = new " + block.Name + "Block[count];");
                    writer.WriteLine("                for(int j = 0; j < count; j++) " + sanitizedName + "[j] = new " + block.Name + "Block();");
                    writer.WriteLine("            }");
                    writer.WriteLine("            for (int j = 0; j < count; j++)");
                    writer.WriteLine("            { " + sanitizedName + "[j].FromBytes(bytes, ref i); }");
                }
                else
                {
                    // Multiple count block
                    writer.WriteLine("            if(" + sanitizedName + ".Length < " + block.Count+") {");
                    writer.WriteLine("                " + sanitizedName + " = new " + block.Name + "Block[" + block.Count + "];");
                    writer.WriteLine("                for(int j = 0; j < " + block.Count + "; j++) " + sanitizedName + "[j] = new " + block.Name + "Block();");
                    writer.WriteLine("            }");
                    writer.WriteLine("            for (int j = 0; j < " + block.Count + "; j++)");
                    writer.WriteLine("            { " + sanitizedName + "[j].FromBytes(bytes, ref i); }");
                }
            }
            writer.WriteLine("        }" + Environment.NewLine);

            seenVariable = false;

            // Constructor that takes a byte array and a prebuilt header
            //writer.WriteLine("        /// <summary>Constructor that takes a byte array and a prebuilt header</summary>");
            writer.WriteLine("        public " + packet.Name + "Packet(Header head, byte[] bytes, ref int i): this()" + Environment.NewLine +
                "        {" + Environment.NewLine +
                "            int packetEnd = bytes.Length - 1;" + Environment.NewLine +
                "            FromBytes(head, bytes, ref i, ref packetEnd, null);" + Environment.NewLine +
                "        }" + Environment.NewLine);

            writer.WriteLine("        override public void FromBytes(Header head, byte[] bytes, ref int i, ref int packetEnd, byte[] zeroBuffer)" + Environment.NewLine + "        {");
            writer.WriteLine("            Header = head;");
            writer.WriteLine("            if (head.Zerocoded && zeroBuffer != null)");
            writer.WriteLine("            {");
            writer.WriteLine("                packetEnd = Helpers.ZeroDecode(bytes, packetEnd + 1, zeroBuffer) - 1;");
            writer.WriteLine("                bytes = zeroBuffer;");
            writer.WriteLine("            }");
            foreach (MapBlock block in packet.Blocks)
            {
                if (block.Name == "Header") { sanitizedName = "_" + block.Name; }
                else { sanitizedName = block.Name; }

                if (block.Count == 1)
                {
                    // Single count block
                    writer.WriteLine("            " + sanitizedName + ".FromBytes(bytes, ref i);");
                }
                else if (block.Count == -1)
                {
                    // Variable count block
                    if (!seenVariable)
                    {
                        writer.WriteLine("            int count = (int)bytes[i++];");
                        seenVariable = true;
                    }
                    else
                    {
                        writer.WriteLine("            count = (int)bytes[i++];");
                    }
                    writer.WriteLine("            if(" + sanitizedName + ".Length < count) {");
                    writer.WriteLine("                " + sanitizedName + " = new " + block.Name + "Block[count];");
                    writer.WriteLine("                for(int j = 0; j < count; j++) " + sanitizedName + "[j] = new " + block.Name + "Block();");
                    writer.WriteLine("            }");
                    writer.WriteLine("            for (int j = 0; j < count; j++)");
                    writer.WriteLine("            { " + sanitizedName + "[j].FromBytes(bytes, ref i); }");
                }
                else
                {
                    // Multiple count block
                    writer.WriteLine("            if(" + sanitizedName + ".Length < " + block.Count+") {");
                    writer.WriteLine("                " + sanitizedName + " = new " + block.Name + "Block[" + block.Count + "];");
                    writer.WriteLine("                for(int j = 0; j < " + block.Count + "; j++) " + sanitizedName + "[j] = new " + block.Name + "Block();");
                    writer.WriteLine("            }");
                    writer.WriteLine("            for (int j = 0; j < " + block.Count + "; j++)");
                    writer.WriteLine("            { " + sanitizedName + "[j].FromBytes(bytes, ref i); }");
                }
            }
            writer.WriteLine("        }" + Environment.NewLine);

            // ToBytes() function
            //writer.WriteLine("        /// <summary>Serialize this packet to a byte array</summary><returns>A byte array containing the serialized packet</returns>");
            writer.WriteLine("        public override byte[] ToBytes()" + Environment.NewLine + "        {");

            writer.Write("            int length = ");
            if (packet.Frequency == PacketFrequency.Low) { writer.WriteLine("10;"); }
            else if (packet.Frequency == PacketFrequency.Medium) { writer.WriteLine("8;"); }
            else { writer.WriteLine("7;"); }

            foreach (MapBlock block in packet.Blocks)
            {
                if (block.Name == "Header") { sanitizedName = "_" + block.Name; }
                else { sanitizedName = block.Name; }

                if (block.Count == 1)
                {
                    // Single count block
                    writer.Write("            length += " + sanitizedName + ".Length;");
                }
            }
            writer.WriteLine(";");

            foreach (MapBlock block in packet.Blocks)
            {
                if (block.Name == "Header") { sanitizedName = "_" + block.Name; }
                else { sanitizedName = block.Name; }

                if (block.Count == -1)
                {
                    writer.WriteLine("            length++;");
                    writer.WriteLine("            for (int j = 0; j < " + sanitizedName +
                        ".Length; j++) { length += " + sanitizedName + "[j].Length; }");
                }
                else if (block.Count > 1)
                {
                    writer.WriteLine("            for (int j = 0; j < " + block.Count +
                        "; j++) { length += " + sanitizedName + "[j].Length; }");
                }
            }

            writer.WriteLine("            if (header.AckList.Length > 0) { length += header.AckList.Length * 4 + 1; }");
            writer.WriteLine("            byte[] bytes = new byte[length];");
            writer.WriteLine("            int i = 0;");
            writer.WriteLine("            header.ToBytes(bytes, ref i);");
            foreach (MapBlock block in packet.Blocks)
            {
                if (block.Name == "Header") { sanitizedName = "_" + block.Name; }
                else { sanitizedName = block.Name; }

                if (block.Count == -1)
                {
                    // Variable count block
                    writer.WriteLine("            bytes[i++] = (byte)" + sanitizedName + ".Length;");
                    writer.WriteLine("            for (int j = 0; j < " + sanitizedName +
                        ".Length; j++) { " + sanitizedName + "[j].ToBytes(bytes, ref i); }");
                }
                else if (block.Count == 1)
                {
                    writer.WriteLine("            " + sanitizedName + ".ToBytes(bytes, ref i);");
                }
                else
                {
                    // Multiple count block
                    writer.WriteLine("            for (int j = 0; j < " + block.Count +
                        "; j++) { " + sanitizedName + "[j].ToBytes(bytes, ref i); }");
                }
            }

            writer.WriteLine("            if (header.AckList.Length > 0) { header.AcksToBytes(bytes, ref i); }");
            writer.WriteLine("            return bytes;" + Environment.NewLine + "        }" + Environment.NewLine);

            // ToString() function
            //writer.WriteLine("        /// <summary>Serialize this packet to a string</summary><returns>A string containing the serialized packet</returns>");
            writer.WriteLine("        public override string ToString()" + Environment.NewLine + "        {");
            writer.WriteLine("            string output = \"--- " + packet.Name + " ---\" + Environment.NewLine;");

            foreach (MapBlock block in packet.Blocks)
            {
                if (block.Name == "Header") { sanitizedName = "_" + block.Name; }
                else { sanitizedName = block.Name; }

                if (block.Count == -1)
                {
                    // Variable count block
                    writer.WriteLine("            for (int j = 0; j < " + 
                        sanitizedName + ".Length; j++)" + Environment.NewLine + "            {");
                    writer.WriteLine("                output += " + sanitizedName + 
                        "[j].ToString() + Environment.NewLine;" + Environment.NewLine + "            }");
                }
                else if (block.Count == 1)
                {
                    writer.WriteLine("                output += " + sanitizedName + ".ToString() + Environment.NewLine;");
                }
                else
                {
                    // Multiple count block
                    writer.WriteLine("            for (int j = 0; j < " + 
                        block.Count + "; j++)" + Environment.NewLine + "            {");
                    writer.WriteLine("                output += " + sanitizedName + 
                        "[j].ToString() + Environment.NewLine;" + Environment.NewLine + "            }");
                }
            }

            writer.WriteLine("            return output;" + Environment.NewLine + "        }" + Environment.NewLine);

            // Closing function bracket
            writer.WriteLine("    }" + Environment.NewLine);
        }

        static int Main(string[] args)
        {
            ProtocolManager protocol;
            List<string> unused = new List<string>();
            TextWriter writer;

            try
            {
                if (args.Length != 4)
                {
                    Console.WriteLine("Usage: [message_template.msg] [template.cs] [unusedpackets.txt] [_Packets_.cs]");
                    return -1;
                }

                writer = new StreamWriter(args[3]);
                protocol = new ProtocolManager(args[0]);

                // Build a list of unused packets
                using (StreamReader unusedReader = new StreamReader(args[2])) 
                {
                    while (unusedReader.Peek() >= 0) 
                    {
                        unused.Add(unusedReader.ReadLine().Trim());
                    }
                }

                // Read in the template.cs file and write it to our output
                TextReader reader = new StreamReader(args[1]);
                writer.WriteLine(reader.ReadToEnd());
                reader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return -2;
            }


            // Prune all of the unused packets out of the protocol
            int i = 0;
            foreach (MapPacket packet in protocol.LowMaps)
            {
                if (packet != null && unused.Contains(packet.Name))
                    protocol.LowMaps[i] = null;
                i++;
            }
            i = 0;
            foreach (MapPacket packet in protocol.MediumMaps)
            {
                if (packet != null && unused.Contains(packet.Name))
                    protocol.MediumMaps[i] = null;
                i++;
            }
            i = 0;
            foreach (MapPacket packet in protocol.HighMaps)
            {
                if (packet != null && unused.Contains(packet.Name))
                    protocol.HighMaps[i] = null;
                i++;
            }


            // Write the PacketType enum
            writer.WriteLine("    public enum PacketType" + Environment.NewLine + "    {" + Environment.NewLine +
                "        /// <summary>A generic value, not an actual packet type</summary>" + Environment.NewLine +
                "        Default,");
            foreach (MapPacket packet in protocol.LowMaps)
                if (packet != null)
                    writer.WriteLine("        " + packet.Name + " = " + (0x10000 | packet.ID)  + ",");
            foreach (MapPacket packet in protocol.MediumMaps)
                if (packet != null)
                    writer.WriteLine("        " + packet.Name + " = " + (0x20000 | packet.ID) + ",");
            foreach (MapPacket packet in protocol.HighMaps)
                if (packet != null)
                    writer.WriteLine("        " + packet.Name + " = " + (0x30000 | packet.ID) + ",");
            writer.WriteLine("    }" + Environment.NewLine);


            // Write all of the XmlInclude statements for the Packet class to allow packet serialization
            //writer.WriteLine("#if PACKETSERIALIZE");
            //foreach (MapPacket packet in protocol.LowMaps)
            //    if (packet != null)
            //        writer.WriteLine("    [XmlInclude(typeof(" + packet.Name + "Packet))]");
            //foreach (MapPacket packet in protocol.MediumMaps)
            //    if (packet != null)
            //        writer.WriteLine("    [XmlInclude(typeof(" + packet.Name + "Packet))]");
            //foreach (MapPacket packet in protocol.HighMaps)
            //    if (packet != null)
            //        writer.WriteLine("    [XmlInclude(typeof(" + packet.Name + "Packet))]");
            //writer.WriteLine("#endif");

            // Write the base Packet class
            writer.WriteLine(
                "    public abstract partial class Packet" + Environment.NewLine + "    {" + Environment.NewLine + 
                "        public abstract Header Header { get; set; }" + Environment.NewLine +
                "        public abstract PacketType Type { get; }" + Environment.NewLine +
                "        public abstract void FromBytes(byte[] bytes, ref int i, ref int packetEnd, byte[] zeroBuffer);" + Environment.NewLine + 
                "        public abstract void FromBytes(Header header, byte[] bytes, ref int i, ref int packetEnd, byte[] zeroBuffer);" + Environment.NewLine +
                "        public int ResendCount;" + Environment.NewLine +
                "        public int TickCount;" + Environment.NewLine + Environment.NewLine +
                "        public abstract byte[] ToBytes();" //+ Environment.NewLine + Environment.NewLine +
                //"        public void ToXml(XmlWriter xmlWriter)" + Environment.NewLine +
                //"        {" + Environment.NewLine +
                //"            XmlSerializer serializer = new XmlSerializer(typeof(Packet));" + Environment.NewLine +
                //"            serializer.Serialize(xmlWriter, this);" + Environment.NewLine +
                //"        }");
            );


            // Write the Packet.GetType() function
            writer.WriteLine(
                "        public static PacketType GetType(ushort id, PacketFrequency frequency)" + Environment.NewLine +
                "        {" + Environment.NewLine +
                "            switch (frequency)" + Environment.NewLine + 
                "            {" + Environment.NewLine + 
                "                case PacketFrequency.Low:" + Environment.NewLine +
                "                    switch (id)" + Environment.NewLine + 
                "                    {");
            foreach (MapPacket packet in protocol.LowMaps)
                if (packet != null)
                    writer.WriteLine("                        case " + packet.ID + ": return PacketType." + packet.Name + ";");
            writer.WriteLine("                    }" + Environment.NewLine + 
                "                    break;" + Environment.NewLine +
                "                case PacketFrequency.Medium:" + Environment.NewLine +
                "                    switch (id)" + Environment.NewLine + "                    {");
            foreach (MapPacket packet in protocol.MediumMaps)
                if (packet != null)
                    writer.WriteLine("                        case " + packet.ID + ": return PacketType." + packet.Name + ";");
            writer.WriteLine("                    }" + Environment.NewLine + 
                "                    break;" + Environment.NewLine +
                "                case PacketFrequency.High:" + Environment.NewLine +
                "                    switch (id)" + Environment.NewLine + "                    {");
            foreach (MapPacket packet in protocol.HighMaps)
                if (packet != null)
                    writer.WriteLine("                        case " + packet.ID + ": return PacketType." + packet.Name + ";");
            writer.WriteLine("                    }" + Environment.NewLine +
                "                    break;" + Environment.NewLine + "            }" + Environment.NewLine + Environment.NewLine +
                "            return PacketType.Default;" + Environment.NewLine + "        }" + Environment.NewLine);
            
            // TODO: Not sure if this function is useful to anyone, but if it is it needs to be
            // rewritten to not overwrite passed in pointers and decode the entire packet just
            // to read the header

            // Write the Packet.GetType() function
            //writer.WriteLine(
            //    "        public static PacketType GetType(byte[] bytes, int packetEnd, byte[] zeroBuffer)" + Environment.NewLine +
            //    "        {" + Environment.NewLine + "            ushort id; PacketFrequency freq;" + Environment.NewLine + 
            //    "            int i = 0, end = packetEnd;" + Environment.NewLine +
            //    "            Header header = Header.BuildHeader(bytes, ref i, ref end);" + Environment.NewLine +
            //    "            if (header.Zerocoded)" + Environment.NewLine + "            {" + Environment.NewLine +
            //    "                end = Helpers.ZeroDecode(bytes, end + 1, zeroBuffer) - 1;" + Environment.NewLine +
            //    "                bytes = zeroBuffer;" + Environment.NewLine + "            }" + Environment.NewLine + Environment.NewLine +
            //    "            if (bytes[6] == 0xFF)" + Environment.NewLine + "            {" + Environment.NewLine +
            //    "                if (bytes[7] == 0xFF)" + Environment.NewLine + "                {" + Environment.NewLine +
            //    "                    id = (ushort)((bytes[8] << 8) + bytes[9]); freq = PacketFrequency.Low;" + Environment.NewLine +
            //    "                }" + Environment.NewLine + 
            //    "                else" + Environment.NewLine +
            //    "                {" + Environment.NewLine + "                    id = (ushort)bytes[7];  freq = PacketFrequency.Medium;" +
            //    Environment.NewLine + "                }" + Environment.NewLine + "            }" + Environment.NewLine +
            //    "            else" + Environment.NewLine + "            {" + Environment.NewLine +
            //    "                id = (ushort)bytes[6];  freq = PacketFrequency.High;" + Environment.NewLine +
            //    "            }" + Environment.NewLine +
            //    "            return GetType(id, freq);" + Environment.NewLine +
            //    "        }" + Environment.NewLine);

            // Write the Packet.BuildPacket(PacketType) function
            writer.WriteLine("        public static Packet BuildPacket(PacketType type)");
            writer.WriteLine("        {");
            foreach (MapPacket packet in protocol.HighMaps)
                if (packet != null)
                    writer.WriteLine("            if(type == PacketType." + packet.Name  + ") return new " + packet.Name +"Packet();");
            foreach (MapPacket packet in protocol.MediumMaps)
                if (packet != null)
                    writer.WriteLine("            if(type == PacketType." + packet.Name  + ") return new " + packet.Name +"Packet();");
            foreach (MapPacket packet in protocol.LowMaps)
                if (packet != null)
                    writer.WriteLine("            if(type == PacketType." + packet.Name  + ") return new " + packet.Name +"Packet();");
            writer.WriteLine("            return null;" + Environment.NewLine);
            writer.WriteLine("        }" + Environment.NewLine);

            // Write the Packet.BuildPacket() function
            writer.WriteLine(
                "        public static Packet BuildPacket(byte[] packetBuffer, ref int packetEnd, byte[] zeroBuffer)" + Environment.NewLine +
                "        {" + Environment.NewLine +
                "            byte[] bytes; ushort id; PacketFrequency freq;" + Environment.NewLine + 
                "            int i = 0;" + Environment.NewLine +
                "            Header header = Header.BuildHeader(packetBuffer, ref i, ref packetEnd);" + Environment.NewLine +
                "            if (header.Zerocoded)" + Environment.NewLine +
                "            {" + Environment.NewLine +
                "                packetEnd = Helpers.ZeroDecode(packetBuffer, packetEnd + 1, zeroBuffer) - 1;" + Environment.NewLine +
                "                bytes = zeroBuffer;" + Environment.NewLine +
                "            }" + Environment.NewLine +
                "            else" + Environment.NewLine +
                "            {" + Environment.NewLine +
                "                bytes = packetBuffer;" + Environment.NewLine +
                "            }" + Environment.NewLine + Environment.NewLine +
                "            if (bytes[6] == 0xFF)" + Environment.NewLine +
                "            {" + Environment.NewLine +
                "                if (bytes[7] == 0xFF)" + Environment.NewLine +
                "                {" + Environment.NewLine +
                "                    id = (ushort)((bytes[8] << 8) + bytes[9]); freq = PacketFrequency.Low;" + Environment.NewLine +
                "                    switch (id)" + Environment.NewLine +
                "                    {");
            foreach (MapPacket packet in protocol.LowMaps)
                if (packet != null)
                    writer.WriteLine("                        case " + packet.ID + ": return new " + packet.Name + "Packet(header, bytes, ref i);");
            writer.WriteLine("                    }" + Environment.NewLine + "                }" + Environment.NewLine + 
                "                else" + Environment.NewLine +
                "                {" + Environment.NewLine + "                    id = (ushort)bytes[7];  freq = PacketFrequency.Medium;" + Environment.NewLine +
                "                    switch (id)" + Environment.NewLine + "                    {");
            foreach (MapPacket packet in protocol.MediumMaps)
                if (packet != null)
                    writer.WriteLine("                        case " + packet.ID + ": return new " + packet.Name + "Packet(header, bytes, ref i);");
            writer.WriteLine("                    }" + Environment.NewLine + "                }" + Environment.NewLine + "            }" + Environment.NewLine +
                "            else" + Environment.NewLine + "            {" + Environment.NewLine +
                "                id = (ushort)bytes[6];  freq = PacketFrequency.High;" + Environment.NewLine +
                "                switch (id)" + Environment.NewLine + "                    {");
            foreach (MapPacket packet in protocol.HighMaps)
                if (packet != null)
                    writer.WriteLine("                        case " + packet.ID + ": return new " + packet.Name + "Packet(header, bytes, ref i);");
            writer.WriteLine("                }" + Environment.NewLine + "            }" + Environment.NewLine + Environment.NewLine +
                "            throw new MalformedDataException(\"Unknown packet ID \"+freq+\" \"+id);" + Environment.NewLine +
                "        }" + Environment.NewLine + "    }" + Environment.NewLine);


            // Write the packet classes
            foreach (MapPacket packet in protocol.LowMaps)
                if (packet != null) { WritePacketClass(writer, packet); }
            foreach (MapPacket packet in protocol.MediumMaps)
                if (packet != null) { WritePacketClass(writer, packet); }
            foreach (MapPacket packet in protocol.HighMaps)
                if (packet != null) { WritePacketClass(writer, packet); }


            // Finish up
            writer.WriteLine("}");
            writer.Close();
            return 0;
        }
    }
}
