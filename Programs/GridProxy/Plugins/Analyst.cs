/*
 * Analyst.cs: proxy that makes packet inspection and modifcation interactive
 *   See the README for usage instructions.
 *
 * Copyright (c) 2006 Austin Jennings
 * Modified by "qode" and "mcortez" on December 21st, 2006 to work with the new
 * pregen
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;
using Nwc.XmlRpc;
using OpenMetaverse;
using OpenMetaverse.Packets;
using GridProxy;


public class Analyst : ProxyPlugin
{
    private ProxyFrame frame;
    private Proxy proxy;
    private Hashtable loggedPackets = new Hashtable();
    private string logGrep = null;
    private Dictionary<PacketType, Dictionary<BlockField, object>> modifiedPackets = new Dictionary<PacketType, Dictionary<BlockField, object>>();
    private Assembly openmvAssembly;
    private StreamWriter output;

    public Analyst(ProxyFrame frame)
    {
        this.frame = frame;
        this.proxy = frame.proxy;
    }

    ~Analyst()
    {
        if (output != null)
            output.Close();
    }

    public override void Init()
    {
        openmvAssembly = Assembly.Load("OpenMetaverse");
        if (openmvAssembly == null) throw new Exception("Assembly load exception");

        // build the table of /command delegates
        InitializeCommandDelegates();

        //  handle command line arguments
        foreach (string arg in frame.Args)
            if (arg == "--log-all")
                LogAll();
            else if (arg.Contains("--log-whitelist="))
                LogWhitelist(arg.Substring(arg.IndexOf('=') + 1));
            else if (arg.Contains("--output="))
                SetOutput(arg.Substring(arg.IndexOf('=') + 1));

        Console.WriteLine("Analyst loaded");
    }

    // InitializeCommandDelegates: configure Analyst's commands
    private void InitializeCommandDelegates()
    {
        frame.AddCommand("/log", new ProxyFrame.CommandDelegate(CmdLog));
        frame.AddCommand("/-log", new ProxyFrame.CommandDelegate(CmdNoLog));
        frame.AddCommand("/grep", new ProxyFrame.CommandDelegate(CmdGrep));
        frame.AddCommand("/drop", new ProxyFrame.CommandDelegate(CmdDrop));
        frame.AddCommand("/-drop", new ProxyFrame.CommandDelegate(CmdNoDrop));
        frame.AddCommand("/set", new ProxyFrame.CommandDelegate(CmdSet));
        frame.AddCommand("/-set", new ProxyFrame.CommandDelegate(CmdNoSet));
        frame.AddCommand("/inject", new ProxyFrame.CommandDelegate(CmdInject));
        frame.AddCommand("/in", new ProxyFrame.CommandDelegate(CmdInject));
    }

    private static PacketType packetTypeFromName(string name)
    {
        Type packetTypeType = typeof(PacketType);
        System.Reflection.FieldInfo f = packetTypeType.GetField(name);
        if (f == null) throw new ArgumentException("Bad packet type");
        return (PacketType)Enum.ToObject(packetTypeType, (int)f.GetValue(packetTypeType));
    }

    // CmdLog: handle a /log command
    private void CmdLog(string[] words)
    {
        if (words.Length != 2)
            SayToUser("Usage: /log <packet name>");
        else if (words[1] == "*")
        {
            LogAll();
            SayToUser("logging all packets");
        }
        else
        {
            PacketType pType;
            try
            {
                pType = packetTypeFromName(words[1]);
            }
            catch (ArgumentException)
            {
                SayToUser("Bad packet name: " + words[1]);
                return;
            }
            loggedPackets[pType] = null;
            proxy.AddDelegate(pType, Direction.Incoming, new PacketDelegate(LogPacketIn));
            proxy.AddDelegate(pType, Direction.Outgoing, new PacketDelegate(LogPacketOut));
            SayToUser("logging " + words[1]);
        }
    }

    // CmdNoLog: handle a /-log command
    private void CmdNoLog(string[] words)
    {
        if (words.Length != 2)
            SayToUser("Usage: /-log <packet name>");
        else if (words[1] == "*")
        {
            NoLogAll();
            SayToUser("stopped logging all packets");
        }
        else
        {
            PacketType pType = packetTypeFromName(words[1]);
            loggedPackets.Remove(pType);

            proxy.RemoveDelegate(pType, Direction.Incoming, new PacketDelegate(LogPacketIn));
            proxy.RemoveDelegate(pType, Direction.Outgoing, new PacketDelegate(LogPacketOut));
            SayToUser("stopped logging " + words[1]);
        }
    }

    // CmdGrep: handle a /grep command
    private void CmdGrep(string[] words)
    {
        if (words.Length == 1)
        {
            logGrep = null;
            SayToUser("stopped filtering logs");
        }
        else
        {
            string[] regexArray = new string[words.Length - 1];
            Array.Copy(words, 1, regexArray, 0, words.Length - 1);
            logGrep = String.Join(" ", regexArray);
            SayToUser("filtering log with " + logGrep);
        }
    }

    private void CmdDrop(string[] words)
    {
        throw new NotImplementedException();
    }

    private void CmdNoDrop(string[] words)
    {
        throw new NotImplementedException();
    }

    // CmdSet: handle a /set command
    private void CmdSet(string[] words)
    {
        if (words.Length < 5)
            SayToUser("Usage: /set <packet name> <block> <field> <value>");
        else
        {
            PacketType pType;
            try
            {
                pType = packetTypeFromName(words[1]);
            }
            catch (ArgumentException)
            {
                SayToUser("Bad packet name: " + words[1]);
                return;
            }

            string[] valueArray = new string[words.Length - 4];
            Array.Copy(words, 4, valueArray, 0, words.Length - 4);
            string valueString = String.Join(" ", valueArray);
            object value;
            try
            {
                value = MagicCast(words[1], words[2], words[3], valueString);
            }
            catch (Exception e)
            {
                SayToUser(e.Message);
                return;
            }

            Dictionary<BlockField, object> fields;
            if (modifiedPackets.ContainsKey(pType))
                fields = (Dictionary<BlockField, object>)modifiedPackets[pType];
            else
                fields = new Dictionary<BlockField, object>();

            fields[new BlockField(words[2], words[3])] = value;
            modifiedPackets[pType] = fields;

            proxy.AddDelegate(pType, Direction.Incoming, new PacketDelegate(ModifyIn));
            proxy.AddDelegate(pType, Direction.Outgoing, new PacketDelegate(ModifyOut));

            SayToUser("setting " + words[1] + "." + words[2] + "." + words[3] + " = " + valueString);
        }
    }

    // CmdNoSet: handle a /-set command
    private void CmdNoSet(string[] words)
    {
        if (words.Length == 2 && words[1] == "*")
        {
            foreach (PacketType pType in modifiedPackets.Keys)
            {
                proxy.RemoveDelegate(pType, Direction.Incoming, new PacketDelegate(ModifyIn));
                proxy.RemoveDelegate(pType, Direction.Outgoing, new PacketDelegate(ModifyOut));
            }
            modifiedPackets = new Dictionary<PacketType, Dictionary<BlockField, object>>();

            SayToUser("stopped setting all fields");
        }
        else if (words.Length == 4)
        {
            PacketType pType;
            try
            {
                pType = packetTypeFromName(words[1]);
            }
            catch (ArgumentException)
            {
                SayToUser("Bad packet name: " + words[1]);
                return;
            }


            if (modifiedPackets.ContainsKey(pType))
            {
                Dictionary<BlockField, object> fields = modifiedPackets[pType];
                fields.Remove(new BlockField(words[2], words[3]));

                if (fields.Count == 0)
                {
                    modifiedPackets.Remove(pType);

                    proxy.RemoveDelegate(pType, Direction.Incoming, new PacketDelegate(ModifyIn));
                    proxy.RemoveDelegate(pType, Direction.Outgoing, new PacketDelegate(ModifyOut));
                }
            }

            SayToUser("stopped setting " + words[1] + "." + words[2] + "." + words[3]);
        }
        else
            SayToUser("Usage: /-set <packet name> <block> <field>");
    }


    // CmdInject: handle an /inject command
    private void CmdInject(string[] words)
    {
        if (words.Length < 2)
            SayToUser("Usage: /inject <packet file> [value]");
        else
        {
            string[] valueArray = new string[words.Length - 2];
            Array.Copy(words, 2, valueArray, 0, words.Length - 2);
            string value = String.Join(" ", valueArray);

            FileStream fs = null;
            StreamReader sr = null;
            Direction direction = Direction.Incoming;
            string name = null;
            string block = null;
            object blockObj = null;
            Type packetClass = null;
            Packet packet = null;

            try
            {
                fs = File.OpenRead(words[1] + ".packet");
                sr = new StreamReader(fs);

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Match match;

                    if (name == null)
                    {
                        match = (new Regex(@"^\s*(in|out)\s+(\w+)\s*$")).Match(line);
                        if (!match.Success)
                        {
                            SayToUser("expecting direction and packet name, got: " + line);
                            return;
                        }

                        string lineDir = match.Groups[1].Captures[0].ToString();
                        string lineName = match.Groups[2].Captures[0].ToString();

                        if (lineDir == "in")
                            direction = Direction.Incoming;
                        else if (lineDir == "out")
                            direction = Direction.Outgoing;
                        else
                        {
                            SayToUser("expecting 'in' or 'out', got: " + line);
                            return;
                        }

                        name = lineName;
                        packetClass = openmvAssembly.GetType("OpenMetaverse.Packets." + name + "Packet");
                        if (packetClass == null) throw new Exception("Couldn't get class " + name + "Packet");
                        ConstructorInfo ctr = packetClass.GetConstructor(new Type[] { });
                        if (ctr == null) throw new Exception("Couldn't get suitable constructor for " + name + "Packet");
                        packet = (Packet)ctr.Invoke(new object[] { });
                        //Console.WriteLine("Created new " + name + "Packet");
                    }
                    else
                    {
                        match = (new Regex(@"^\s*\[(\w+)\]\s*$")).Match(line);
                        if (match.Success)
                        {
                            block = match.Groups[1].Captures[0].ToString();
                            FieldInfo blockField = packetClass.GetField(block);
                            if (blockField == null) throw new Exception("Couldn't get " + name + "Packet." + block);
                            Type blockClass = blockField.FieldType;
                            if (blockClass.IsArray)
                            {
                                blockClass = blockClass.GetElementType();
                                ConstructorInfo ctr = blockClass.GetConstructor(new Type[] { });
                                if (ctr == null) throw new Exception("Couldn't get suitable constructor for " + blockClass.Name);
                                blockObj = ctr.Invoke(new object[] { });
                                object[] arr = (object[])blockField.GetValue(packet);
                                object[] narr = (object[])Array.CreateInstance(blockClass, arr.Length + 1);
                                Array.Copy(arr, narr, arr.Length);
                                narr[arr.Length] = blockObj;
                                blockField.SetValue(packet, narr);
                                //Console.WriteLine("Added block "+block);
                            }
                            else
                            {
                                blockObj = blockField.GetValue(packet);
                            }
                            if (blockObj == null) throw new Exception("Got " + name + "Packet." + block + " == null");
                            //Console.WriteLine("Got block " + name + "Packet." + block);

                            continue;
                        }

                        if (block == null)
                        {
                            SayToUser("expecting block name, got: " + line);
                            return;
                        }

                        match = (new Regex(@"^\s*(\w+)\s*=\s*(.*)$")).Match(line);
                        if (match.Success)
                        {
                            string lineField = match.Groups[1].Captures[0].ToString();
                            string lineValue = match.Groups[2].Captures[0].ToString();
                            object fval;

                            //FIXME: use of MagicCast inefficient
                            if (lineValue == "$Value")
                                fval = MagicCast(name, block, lineField, value);
                            else if (lineValue == "$UUID")
                                fval = UUID.Random();
                            else if (lineValue == "$AgentID")
                                fval = frame.AgentID;
                            else if (lineValue == "$SessionID")
                                fval = frame.SessionID;
                            else
                                fval = MagicCast(name, block, lineField, lineValue);

                            MagicSetField(blockObj, lineField, fval);
                            continue;
                        }

                        SayToUser("expecting block name or field, got: " + line);
                        return;
                    }
                }

                if (name == null)
                {
                    SayToUser("expecting direction and packet name, got EOF");
                    return;
                }

                packet.Header.Flags |= Helpers.MSG_RELIABLE;
                //if (protocolManager.Command(name).Encoded)
                //	packet.Header.Flags |= Helpers.MSG_ZEROCODED;
                proxy.InjectPacket(packet, direction);

                SayToUser("injected " + words[1]);
            }
            catch (Exception e)
            {
                SayToUser("failed to inject " + words[1] + ": " + e.Message);
                Console.WriteLine("failed to inject " + words[1] + ": " + e.Message + "\n" + e.StackTrace);
            }
            finally
            {
                if (fs != null)
                    fs.Close();
                if (sr != null)
                    sr.Close();
            }
        }
    }

    // SayToUser: send a message to the user as in-world chat
    private void SayToUser(string message)
    {
        ChatFromSimulatorPacket packet = new ChatFromSimulatorPacket();
        packet.ChatData.FromName = Utils.StringToBytes("Analyst");
        packet.ChatData.SourceID = UUID.Random();
        packet.ChatData.OwnerID = frame.AgentID;
        packet.ChatData.SourceType = (byte)2;
        packet.ChatData.ChatType = (byte)1;
        packet.ChatData.Audible = (byte)1;
        packet.ChatData.Position = new Vector3(0, 0, 0);
        packet.ChatData.Message = Utils.StringToBytes(message);
        proxy.InjectPacket(packet, Direction.Incoming);
    }

    // BlockField: product type for a block name and field name
    private struct BlockField
    {
        public string block;
        public string field;


        public BlockField(string block, string field)
        {
            this.block = block;
            this.field = field;
        }
    }

    private static void MagicSetField(object obj, string field, object val)
    {
        Type cls = obj.GetType();

        FieldInfo fieldInf = cls.GetField(field);
        if (fieldInf == null)
        {
            PropertyInfo prop = cls.GetProperty(field);
            if (prop == null) throw new Exception("Couldn't find field " + cls.Name + "." + field);
            prop.SetValue(obj, val, null);
            //throw new Exception("FIXME: can't set properties");
        }
        else
        {
            fieldInf.SetValue(obj, val);
        }
    }

    // MagicCast: given a packet/block/field name and a string, convert the string to a value of the appropriate type
    private object MagicCast(string name, string block, string field, string value)
    {
        Type packetClass = openmvAssembly.GetType("OpenMetaverse.Packets." + name + "Packet");
        if (packetClass == null) throw new Exception("Couldn't get class " + name + "Packet");

        FieldInfo blockField = packetClass.GetField(block);
        if (blockField == null) throw new Exception("Couldn't get " + name + "Packet." + block);
        Type blockClass = blockField.FieldType;
        if (blockClass.IsArray) blockClass = blockClass.GetElementType();
        // Console.WriteLine("DEBUG: " + blockClass.Name);

        FieldInfo fieldField = blockClass.GetField(field); PropertyInfo fieldProp = null;
        Type fieldClass = null;
        if (fieldField == null)
        {
            fieldProp = blockClass.GetProperty(field);
            if (fieldProp == null) throw new Exception("Couldn't get " + name + "Packet." + block + "." + field);
            fieldClass = fieldProp.PropertyType;
        }
        else
        {
            fieldClass = fieldField.FieldType;
        }

        try
        {
            if (fieldClass == typeof(byte))
            {
                return Convert.ToByte(value);
            }
            else if (fieldClass == typeof(ushort))
            {
                return Convert.ToUInt16(value);
            }
            else if (fieldClass == typeof(uint))
            {
                return Convert.ToUInt32(value);
            }
            else if (fieldClass == typeof(ulong))
            {
                return Convert.ToUInt64(value);
            }
            else if (fieldClass == typeof(sbyte))
            {
                return Convert.ToSByte(value);
            }
            else if (fieldClass == typeof(short))
            {
                return Convert.ToInt16(value);
            }
            else if (fieldClass == typeof(int))
            {
                return Convert.ToInt32(value);
            }
            else if (fieldClass == typeof(long))
            {
                return Convert.ToInt64(value);
            }
            else if (fieldClass == typeof(float))
            {
                return Convert.ToSingle(value);
            }
            else if (fieldClass == typeof(double))
            {
                return Convert.ToDouble(value);
            }
            else if (fieldClass == typeof(UUID))
            {
                return new UUID(value);
            }
            else if (fieldClass == typeof(bool))
            {
                if (value.ToLower() == "true")
                    return true;
                else if (value.ToLower() == "false")
                    return false;
                else
                    throw new Exception();
            }
            else if (fieldClass == typeof(byte[]))
            {
                return Utils.StringToBytes(value);
            }
            else if (fieldClass == typeof(Vector3))
            {
                Vector3 result;
                if (Vector3.TryParse(value, out result))
                    return result;
                else
                    throw new Exception();
            }
            else if (fieldClass == typeof(Vector3d))
            {
                Vector3d result;
                if (Vector3d.TryParse(value, out result))
                    return result;
                else
                    throw new Exception();
            }
            else if (fieldClass == typeof(Vector4))
            {
                Vector4 result;
                if (Vector4.TryParse(value, out result))
                    return result;
                else
                    throw new Exception();
            }
            else if (fieldClass == typeof(Quaternion))
            {
                Quaternion result;
                if (Quaternion.TryParse(value, out result))
                    return result;
                else
                    throw new Exception();
            }
            else
            {
                throw new Exception("unsupported field type " + fieldClass);
            }
        }
        catch
        {
            throw new Exception("unable to interpret " + value + " as " + fieldClass);
        }
    }

    // ModifyIn: modify an incoming packet
    private Packet ModifyIn(Packet packet, IPEndPoint endPoint)
    {
        return Modify(packet, endPoint, Direction.Incoming);
    }

    // ModifyOut: modify an outgoing packet
    private Packet ModifyOut(Packet packet, IPEndPoint endPoint)
    {
        return Modify(packet, endPoint, Direction.Outgoing);
    }

    // Modify: modify a packet
    private Packet Modify(Packet packet, IPEndPoint endPoint, Direction direction)
    {
        if (modifiedPackets.ContainsKey(packet.Type))
        {
            try
            {
                Dictionary<BlockField, object> changes = modifiedPackets[packet.Type];
                Type packetClass = packet.GetType();

                foreach (KeyValuePair<BlockField, object> change in changes)
                {
                    BlockField bf = change.Key;
                    FieldInfo blockField = packetClass.GetField(bf.block);
                    if (blockField.FieldType.IsArray) // We're modifying a variable block.
                    {
                        // Modify each block in the variable block identically.
                        // This is really simple, can probably be improved.
                        object[] blockArray = (object[])blockField.GetValue(packet);
                        foreach (object blockElement in blockArray)
                        {
                            MagicSetField(blockElement, bf.field, change.Value);
                        }
                    }
                    else
                    {
                        //Type blockClass = blockField.FieldType;
                        object blockObject = blockField.GetValue(packet);
                        MagicSetField(blockObject, bf.field, change.Value);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("failed to modify " + packet.Type + ": " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        return packet;
    }

    // LogPacketIn: log an incoming packet
    private Packet LogPacketIn(Packet packet, IPEndPoint endPoint)
    {
        LogPacket(packet, endPoint, Direction.Incoming);
        return packet;
    }

    // LogPacketOut: log an outgoing packet
    private Packet LogPacketOut(Packet packet, IPEndPoint endPoint)
    {
        LogPacket(packet, endPoint, Direction.Outgoing);
        return packet;
    }

    // LogAll: register logging delegates for all packets
    private void LogAll()
    {
        Type packetTypeType = typeof(PacketType);
        System.Reflection.MemberInfo[] packetTypes = packetTypeType.GetMembers();

        for (int i = 0; i < packetTypes.Length; i++)
        {
            if (packetTypes[i].MemberType == System.Reflection.MemberTypes.Field && packetTypes[i].DeclaringType == packetTypeType)
            {
                string name = packetTypes[i].Name;
                PacketType pType;

                try
                {
                    pType = packetTypeFromName(name);
                }
                catch (Exception)
                {
                    continue;
                }

                loggedPackets[pType] = null;

                proxy.AddDelegate(pType, Direction.Incoming, new PacketDelegate(LogPacketIn));
                proxy.AddDelegate(pType, Direction.Outgoing, new PacketDelegate(LogPacketOut));
            }
        }
    }

    private void LogWhitelist(string whitelistFile)
    {
        try
        {
            string[] lines = File.ReadAllLines(whitelistFile);
            int count = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0)
                    continue;

                PacketType pType;

                try
                {
                    pType = packetTypeFromName(line);
                    proxy.AddDelegate(pType, Direction.Incoming, new PacketDelegate(LogPacketIn));
                    proxy.AddDelegate(pType, Direction.Outgoing, new PacketDelegate(LogPacketOut));
                    ++count;
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Bad packet name: " + line);
                }
            }

            Console.WriteLine(String.Format("Logging {0} packet types loaded from whitelist", count));
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to load packet whitelist from " + whitelistFile);
        }
    }

    private void SetOutput(string outputFile)
    {
        try
        {
            output = new StreamWriter(outputFile, false);
            Console.WriteLine("Logging packets to " + outputFile);
        }
        catch (Exception)
        {
            Console.WriteLine(String.Format("Failed to open {0} for logging", outputFile));
        }
    }

    // NoLogAll: unregister logging delegates for all packets
    private void NoLogAll()
    {
        Type packetTypeType = typeof(PacketType);
        System.Reflection.MemberInfo[] packetTypes = packetTypeType.GetMembers();

        for (int i = 0; i < packetTypes.Length; i++)
        {
            if (packetTypes[i].MemberType == System.Reflection.MemberTypes.Field && packetTypes[i].DeclaringType == packetTypeType)
            {
                string name = packetTypes[i].Name;
                PacketType pType;

                try
                {
                    pType = packetTypeFromName(name);
                }
                catch (Exception)
                {
                    continue;
                }

                loggedPackets.Remove(pType);

                proxy.RemoveDelegate(pType, Direction.Incoming, new PacketDelegate(LogPacketIn));
                proxy.RemoveDelegate(pType, Direction.Outgoing, new PacketDelegate(LogPacketOut));
            }
        }
    }

    // LogPacket: dump a packet to the console
    private void LogPacket(Packet packet, IPEndPoint endPoint, Direction direction)
    {
        string packetText = Helpers.PacketToString(packet);

        if (logGrep == null || (logGrep != null && Regex.IsMatch(packetText, logGrep)))
        {
            string line = String.Format("{0} {1,21} {2,5} {3}{4}{5}"
                , direction == Direction.Incoming ? "<--" : "-->"
                , endPoint
                , packet.Header.Sequence
                , InterpretOptions(packet.Header.Flags)
                , Environment.NewLine
                , packetText
                );

            Console.WriteLine(line);

            if (output != null)
                output.WriteLine(line);
        }
    }

    // InterpretOptions: produce a string representing a packet's header options
    private static string InterpretOptions(byte options)
    {
        return "["
             + ((options & Helpers.MSG_APPENDED_ACKS) != 0 ? "Ack" : "   ")
             + " "
             + ((options & Helpers.MSG_RESENT) != 0 ? "Res" : "   ")
             + " "
             + ((options & Helpers.MSG_RELIABLE) != 0 ? "Rel" : "   ")
             + " "
             + ((options & Helpers.MSG_ZEROCODED) != 0 ? "Zer" : "   ")
             + "]"
             ;
    }
}
