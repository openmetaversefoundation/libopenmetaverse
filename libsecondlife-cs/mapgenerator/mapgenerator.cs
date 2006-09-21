using System;
using System.Text;
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
                case FieldType.S64:
                    type = "long";
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
                Console.WriteLine("            public " + type + " " + field.Name + ";");
            }
            else
            {
                Console.WriteLine("            private byte[] " + field.Name.ToLower() + ";");
                Console.WriteLine("            public byte[] " + field.Name + "\n            {");
                Console.WriteLine("                get { return " + field.Name.ToLower() + "; }");
                Console.WriteLine("                set\n                {");
                Console.WriteLine("                    if (value == null) { " + 
                    field.Name.ToLower() + " = null; return; }");
                Console.WriteLine("                    if (value.Length > " + 
                    ((field.Count == 1) ? "255" : "1024") + ") { throw new OverflowException(" + 
                    "\"Value exceeds " + ((field.Count == 1) ? "255" : "1024") + " characters\"); }");
                Console.WriteLine("                    else { " + field.Name.ToLower() + 
                    "= new byte[value.Length]; Array.Copy(value, " + 
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
                case FieldType.U16:
                    Console.WriteLine("                    " + field.Name + 
                        " = (ushort)(bytes[i++] + (bytes[i++] << 8));");
                    break;
                case FieldType.LLQuaternion:
                    Console.WriteLine("                    " + field.Name + 
                        " = new LLQuaternion(bytes, i); i += 16;");
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
                case FieldType.S64:
                    Console.WriteLine("                    " + field.Name +
                        " = (long)(bytes[i++] + (bytes[i++] << 8) + " +
                        "(bytes[i++] << 16) + (bytes[i++] << 24) + " +
                        "(bytes[i++] << 32) + (bytes[i++] << 40) + " +
                        "(bytes[i++] << 48) + (bytes[i++] << 56));");
                    break;
                case FieldType.S8:
                    Console.WriteLine("                    " + field.Name +
                        " = (sbyte)bytes[i++];");
                    break;
                case FieldType.U64:
                    Console.WriteLine("                    " + field.Name +
                        " = (ulong)(bytes[i++] + (bytes[i++] << 8) + " +
                        "(bytes[i++] << 16) + (bytes[i++] << 24) + " +
                        "(bytes[i++] << 32) + (bytes[i++] << 40) + " +
                        "(bytes[i++] << 48) + (bytes[i++] << 56));");
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
                    Console.WriteLine("                    " + field.Name.ToLower() + " = new byte[length];");
                    Console.WriteLine("                    Array.Copy(bytes, i, " + field.Name.ToLower() +
                        ", 0, length); i += length;");
                    break;
            }
        }

        static void WriteBlockClass(MapBlock block)
        {
            Console.WriteLine("        public class " + block.Name + "Block\n        {");

            foreach (MapField field in block.Fields)
            {
                WriteFieldMember(field);
            }

            Console.WriteLine("");

            // Constructors
            Console.WriteLine("            public " + block.Name + "Block() { }");
            Console.WriteLine("            public " + block.Name + "Block(byte[] bytes, ref int i)" +
                "\n            {\n                int length;\n                \n" +
                "                try\n                {");

            foreach (MapField field in block.Fields)
            {
                WriteFieldFromBytes(field);
            }

            Console.WriteLine("                }\n                catch (Exception)\n" +
                "                {\n                    throw new MalformedDataException();\n" +
                "                }\n            }\n");

            // FIXME: Write the ToBytes() function
            Console.WriteLine("            public byte[] ToBytes() { return null; }");

            Console.WriteLine("        }\n");
        }

        static void WritePacketClass(MapPacket packet)
        {
            Console.WriteLine("    class " + packet.Name + "Packet\n    {");

            // Write out each block class
            foreach (MapBlock block in packet.Blocks)
            {
                WriteBlockClass(block);
            }

            // public LowHeader Header;

            Console.WriteLine("        " + packet.Name + "Packet() { }");

            Console.WriteLine("        " + packet.Name + "Packet(byte[] bytes) { }");

            Console.WriteLine("        public byte[] ToBytes() { return null; }");

            Console.WriteLine("    }\n");
        }

        static void Main(string[] args)
        {
            SecondLife libsl = new SecondLife("keywords.txt", "message_template.msg");
            int i = 0;

            Console.WriteLine("using System;\nusing libsecondlife;\n\nnamespace libsecondlife\n{");

            foreach (MapPacket packet in libsl.Protocol.LowMaps)
            {
                if (packet != null)
                {
                    WritePacketClass(packet);
                    if (i++ > 10) break;
                }
            }

            Console.WriteLine("}");
        }
    }
}
