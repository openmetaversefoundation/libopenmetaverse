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
 * - Neither the name of the Second Life Reverse Engineering Team nor the names 
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

using SLProxy;
using libsecondlife;
using Nwc.XmlRpc;
using libsecondlife.Packets;
using System.Reflection;

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

public class Analyst
{
    private static Proxy proxy;
    private static Hashtable commandDelegates = new Hashtable();
    private static Hashtable loggedPackets = new Hashtable();
    // private static string logGrep = null;
    private static Hashtable modifiedPackets = new Hashtable();
    private static LLUUID agentID;
    private static LLUUID sessionID;
    private static bool logLogin = false;
    private static Assembly libslAssembly;

    public static void Main(string[] args)
    {

        libslAssembly = Assembly.Load("libsecondlife");
        if (libslAssembly == null) throw new Exception("Assembly load exception");

        ProxyConfig proxyConfig = new ProxyConfig("Analyst V2", "Austin Jennings / Andrew Ortman", args);
        proxy = new Proxy(proxyConfig);

        // build the table of /command delegates
        InitializeCommandDelegates();

        // add delegates for login
        proxy.SetLoginRequestDelegate(new XmlRpcRequestDelegate(LoginRequest));
        proxy.SetLoginResponseDelegate(new XmlRpcResponseDelegate(LoginResponse));

        // add a delegate for outgoing chat
        proxy.AddDelegate(PacketType.ChatFromViewer, Direction.Incoming, new PacketDelegate(ChatFromViewerIn));
        proxy.AddDelegate(PacketType.ChatFromViewer, Direction.Outgoing, new PacketDelegate(ChatFromViewerOut));

        //  handle command line arguments
        foreach (string arg in args)
            if (arg == "--log-all")
                LogAll();
            else if (arg == "--log-login")
                logLogin = true;

        // start the proxy
        proxy.Start();
    }

    // LoginRequest: dump a login request to the console
    private static void LoginRequest(XmlRpcRequest request)
    {
        if (logLogin)
        {
            Console.WriteLine("==> Login Request");
            Console.WriteLine(request);
        }
    }

    // Loginresponse: dump a login response to the console
    private static void LoginResponse(XmlRpcResponse response)
    {
        Hashtable values = (Hashtable)response.Value;
        if (values.Contains("agent_id"))
            agentID = new LLUUID((string)values["agent_id"]);
        if (values.Contains("session_id"))
            sessionID = new LLUUID((string)values["session_id"]);

        if (logLogin)
        {
            Console.WriteLine("<== Login Response");
            Console.WriteLine(response);
        }
    }

    // ChatFromViewerIn: incoming ChatFromViewer delegate; shouldn't be possible, but just in case...
    private static Packet ChatFromViewerIn(Packet packet, IPEndPoint sim)
    {
        if (loggedPackets.Contains(PacketType.ChatFromViewer) || modifiedPackets.Contains(PacketType.ChatFromViewer))
            // user has asked to log or modify this packet
            return Analyze(packet, sim, Direction.Incoming);
        else
            // return the packet unmodified
            return packet;
    }

    // ChatFromViewerOut: outgoing ChatFromViewer delegate; check for Analyst commands
    private static Packet ChatFromViewerOut(Packet packet, IPEndPoint sim)
    {
        // deconstruct the packet
        ChatFromViewerPacket cpacket = (ChatFromViewerPacket)packet;
        string message = System.Text.Encoding.UTF8.GetString(cpacket.ChatData.Message).Replace("\0", "");

        if (message.Length > 1 && message[0] == '/')
        {
            string[] words = message.Split(' ');
            if (commandDelegates.Contains(words[0]))
            {
                // this is an Analyst command; act on it and drop the chat packet
                ((CommandDelegate)commandDelegates[words[0]])(words);
                return null;
            }
        }

        if (loggedPackets.Contains(PacketType.ChatFromViewer) || modifiedPackets.Contains(PacketType.ChatFromViewer))
            // user has asked to log or modify this packet
            return Analyze(packet, sim, Direction.Outgoing);
        else
            // return the packet unmodified
            return packet;
    }

    // CommandDelegate: specifies a callback delegate for a /command
    private delegate void CommandDelegate(string[] words);

    // InitializeCommandDelegates: configure Analyst's commands
    private static void InitializeCommandDelegates()
    {
        commandDelegates["/log"] = new CommandDelegate(CmdLog);
        commandDelegates["/-log"] = new CommandDelegate(CmdNoLog);
        // commandDelegates["/grep"] = new CommandDelegate(CmdGrep);
        commandDelegates["/set"] = new CommandDelegate(CmdSet);
        commandDelegates["/-set"] = new CommandDelegate(CmdNoSet);
        commandDelegates["/inject"] = new CommandDelegate(CmdInject);
        commandDelegates["/in"] = new CommandDelegate(CmdInject);
    }

    private static PacketType packetTypeFromName(string name)
    {
        Type packetTypeType = typeof(PacketType);
        System.Reflection.FieldInfo f = packetTypeType.GetField(name);
        if (f == null) throw new ArgumentException("Bad packet type");
        return (PacketType)Enum.ToObject(packetTypeType, (int)f.GetValue(packetTypeType));
    }

    // CmdLog: handle a /log command
    private static void CmdLog(string[] words)
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
            catch (ArgumentException e)
            {
                SayToUser("Bad packet name: " + words[1]);
                return;
            }
            loggedPackets[pType] = null;
            if (words[1] != "ChatFromViewer")
            {
                proxy.AddDelegate(pType, Direction.Incoming, new PacketDelegate(AnalyzeIn));
                proxy.AddDelegate(pType, Direction.Outgoing, new PacketDelegate(AnalyzeOut));
            }
            SayToUser("logging " + words[1]);
        }
    }

    // CmdNoLog: handle a /-log command
    private static void CmdNoLog(string[] words)
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

            if (!modifiedPackets.Contains(words[1]))
            {
                if (words[1] != "ChatFromViewer")
                {
                    proxy.RemoveDelegate(pType, Direction.Incoming);
                    proxy.RemoveDelegate(pType, Direction.Outgoing);
                }
            }

            SayToUser("stopped logging " + words[1]);
        }
    }

    /*	// CmdGrep: handle a /grep command
        private static void CmdGrep(string[] words) {
            if (words.Length == 1) {
                logGrep = null;
                SayToUser("stopped filtering logs");
            } else {
                string[] regexArray = new string[words.Length - 1];
                Array.Copy(words, 1, regexArray, 0, words.Length - 1);
                logGrep = String.Join(" ", regexArray);
                SayToUser("filtering log with " + logGrep);
            }
        } */

    // CmdSet: handle a /set command
    private static void CmdSet(string[] words)
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
            catch (ArgumentException e)
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

            Hashtable fields;
            if (modifiedPackets.Contains(pType))
                fields = (Hashtable)modifiedPackets[pType];
            else
                fields = new Hashtable();

            fields[new BlockField(words[2], words[3])] = value;
            modifiedPackets[pType] = fields;

            if (words[1] != "ChatFromViewer")
            {
                proxy.AddDelegate(pType, Direction.Incoming, new PacketDelegate(AnalyzeIn));
                proxy.AddDelegate(pType, Direction.Outgoing, new PacketDelegate(AnalyzeOut));
            }

            SayToUser("setting " + words[1] + "." + words[2] + "." + words[3] + " = " + valueString);
        }
    }

    // CmdNoSet: handle a /-set command
    private static void CmdNoSet(string[] words)
    {
        if (words.Length == 2 && words[1] == "*")
        {
            foreach (PacketType pType in modifiedPackets.Keys)
                if (!loggedPackets.Contains(pType) && pType != PacketType.ChatFromViewer)
                {
                    proxy.RemoveDelegate(pType, Direction.Incoming);
                    proxy.RemoveDelegate(pType, Direction.Outgoing);
                }
            modifiedPackets = new Hashtable();

            SayToUser("stopped setting all fields");
        }
        else if (words.Length == 4)
        {
            PacketType pType;
            try
            {
                pType = packetTypeFromName(words[1]);
            }
            catch (ArgumentException e)
            {
                SayToUser("Bad packet name: " + words[1]);
                return;
            }


            if (modifiedPackets.Contains(pType))
            {
                Hashtable fields = (Hashtable)modifiedPackets[pType];
                fields.Remove(new BlockField(words[2], words[3]));

                if (fields.Count == 0)
                {
                    modifiedPackets.Remove(pType);

                    if (!loggedPackets.Contains(pType))
                    {
                        if (words[1] != "ChatFromViewer")
                        {
                            proxy.RemoveDelegate(pType, Direction.Incoming);
                            proxy.RemoveDelegate(pType, Direction.Outgoing);
                        }
                    }
                }
            }

            SayToUser("stopped setting " + words[1] + "." + words[2] + "." + words[3]);
        }
        else
            SayToUser("Usage: /-set <packet name> <block> <field>");
    }


    // CmdInject: handle an /inject command
    private static void CmdInject(string[] words)
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
            //Hashtable blocks = new Hashtable();
            string block = null;
            object blockObj = null;
            //Hashtable fields = new Hashtable();
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
                        packetClass = libslAssembly.GetType("libsecondlife.Packets." + name + "Packet");
                        if (packetClass == null) throw new Exception("Couldn't get class " + name + "Packet");
                        ConstructorInfo ctr = packetClass.GetConstructor(new Type[] { });
                        if (ctr == null) throw new Exception("Couldn't get suitable constructor for " + name + "Packet");
                        packet = (Packet)ctr.Invoke(new object[] { });
                        Console.WriteLine("Created new " + name + "Packet");
                    }
                    else
                    {
                        match = (new Regex(@"^\s*\[(\w+)\]\s*$")).Match(line);
                        if (match.Success)
                        {
                            //FIXME: support variable blocks

                            block = match.Groups[1].Captures[0].ToString();
                            FieldInfo blockField = packetClass.GetField(block);
                            if (blockField == null) throw new Exception("Couldn't get " + name + "Packet." + block);
                            blockObj = blockField.GetValue(packet);
                            if (blockObj == null) throw new Exception("Got " + name + "Packet." + block + " == null");
                            Console.WriteLine("Got block " + name + "Packet." + block);

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
                                fval = LLUUID.Random();
                            else if (lineValue == "$AgentID")
                                fval = agentID;
                            else if (lineValue == "$SessionID")
                                fval = sessionID;
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
    private static void SayToUser(string message)
    {
        ChatFromSimulatorPacket packet = new ChatFromSimulatorPacket();
        packet.ChatData.FromName = Helpers.StringToField("Analyst");
        packet.ChatData.SourceID = LLUUID.Random();
        packet.ChatData.OwnerID = agentID;
        packet.ChatData.SourceType = (byte)2;
        packet.ChatData.ChatType = (byte)1;
        packet.ChatData.Audible = (byte)1;
        packet.ChatData.Position = new LLVector3(0, 0, 0);
        packet.ChatData.Message = Helpers.StringToField(message);
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
    private static object MagicCast(string name, string block, string field, string value)
    {
        Type packetClass = libslAssembly.GetType("libsecondlife.Packets." + name + "Packet");
        if (packetClass == null) throw new Exception("Couldn't get class " + name + "Packet");
        /*		try {
                    packetMap = protocolManager.Command(name);
                } catch {
                    throw new Exception("unkown packet " + name);
                } */


        //FIXME: support variable blocks

        FieldInfo blockField = packetClass.GetField(block);
        if (blockField == null) throw new Exception("Couldn't get " + name + "Packet." + block);
        Type blockClass = blockField.FieldType;
        if (blockClass.IsArray) blockClass = blockClass.GetElementType();
        Console.WriteLine("DEBUG: " + blockClass.Name);

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
            else if (fieldClass == typeof(LLUUID))
            {
                return new LLUUID(value);
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
                return Helpers.StringToField(value);
            }
            else if (fieldClass == typeof(LLVector3))
            {
                Match vector3Match = (new Regex(@"<\s*(-?[0-9.]+)\s*,\s*(-?[0-9.]+)\s*,\s*(-?[0-9.]+)\s*>")).Match(value);
                if (!vector3Match.Success)
                    throw new Exception();
                return new LLVector3
                    (Convert.ToSingle(vector3Match.Groups[1].Captures[0].ToString())
                    , Convert.ToSingle(vector3Match.Groups[2].Captures[0].ToString())
                    , Convert.ToSingle(vector3Match.Groups[3].Captures[0].ToString())
                    );
            }
            else if (fieldClass == typeof(LLVector3d))
            {
                Match vector3dMatch = (new Regex(@"<\s*(-?[0-9.]+)\s*,\s*(-?[0-9.]+)\s*,\s*(-?[0-9.]+)\s*>")).Match(value);
                if (!vector3dMatch.Success)
                    throw new Exception();
                return new LLVector3d
                    (Convert.ToDouble(vector3dMatch.Groups[1].Captures[0].ToString())
                    , Convert.ToDouble(vector3dMatch.Groups[2].Captures[0].ToString())
                    , Convert.ToDouble(vector3dMatch.Groups[3].Captures[0].ToString())
                    );
            }
            else if (fieldClass == typeof(LLVector4))
            {
                Match vector4Match = (new Regex(@"<\s*(-?[0-9.]+)\s*,\s*(-?[0-9.]+)\s*,\s*(-?[0-9.]+)\s*,\s*(-?[0-9.]+)\s*>")).Match(value);
                if (!vector4Match.Success)
                    throw new Exception();
                float vector4X = Convert.ToSingle(vector4Match.Groups[1].Captures[0].ToString());
                float vector4Y = Convert.ToSingle(vector4Match.Groups[2].Captures[0].ToString());
                float vector4Z = Convert.ToSingle(vector4Match.Groups[3].Captures[0].ToString());
                float vector4S = Convert.ToSingle(vector4Match.Groups[4].Captures[0].ToString());
                byte[] vector4Bytes = new byte[16];
                Array.Copy(BitConverter.GetBytes(vector4X), 0, vector4Bytes, 0, 4);
                Array.Copy(BitConverter.GetBytes(vector4Y), 0, vector4Bytes, 4, 4);
                Array.Copy(BitConverter.GetBytes(vector4Z), 0, vector4Bytes, 8, 4);
                Array.Copy(BitConverter.GetBytes(vector4S), 0, vector4Bytes, 12, 4);
                return new LLVector4(vector4Bytes, 0);
            }
            else if (fieldClass == typeof(LLQuaternion))
            {
                Match quaternionMatch = (new Regex(@"<\s*(-?[0-9.]+)\s*,\s*(-?[0-9.]+)\s*,\s*(-?[0-9.]+)\s*>")).Match(value);
                if (!quaternionMatch.Success)
                    throw new Exception();
                return new LLQuaternion
                    (Convert.ToSingle(quaternionMatch.Groups[1].Captures[0].ToString())
                    , Convert.ToSingle(quaternionMatch.Groups[2].Captures[0].ToString())
                    , Convert.ToSingle(quaternionMatch.Groups[3].Captures[0].ToString())
                    );
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
        /*				try {
                            switch (fieldMap.Type) {
                                case FieldType.LLVector3:
                                case FieldType.IPADDR:
                                    return IPAddress.Parse(value);
                                case FieldType.IPPORT:
                                    return Convert.ToUInt16(value);
                                case FieldType.Variable:
                                    Match match = Regex.Match(value, @"^0x([0-9a-fA-F]{2})*", RegexOptions.IgnoreCase);
                                    if (match.Success) {
                                        byte[] buf = new byte[match.Groups[1].Captures.Count];
                                        int i = 0;
                                        foreach (Capture capture in match.Groups[1].Captures)
                                            buf[i++] = Byte.Parse(capture.ToString(), NumberStyles.AllowHexSpecifier);
                                        return buf;
                                    } else
                                        return value;
                            }
                        } catch {
                            throw new Exception("unable to interpret " + value + " as " + fieldMap.Type);
                        }

                        throw new Exception("unsupported field type " + fieldMap.Type);
                    }

                    throw new Exception("unknown field " + name + "." + block + "." + field);
                }

                throw new Exception("unknown block " + name + "." + block); */
    }

    // AnalyzeIn: analyze an incoming packet
    private static Packet AnalyzeIn(Packet packet, IPEndPoint endPoint)
    {
        return Analyze(packet, endPoint, Direction.Incoming);
    }

    // AnalyzeOut: analyze an outgoing packet
    private static Packet AnalyzeOut(Packet packet, IPEndPoint endPoint)
    {
        return Analyze(packet, endPoint, Direction.Outgoing);
    }

    // Analyze: modify and/or log a pocket
    private static Packet Analyze(Packet packet, IPEndPoint endPoint, Direction direction)
    {
        if (modifiedPackets.Contains(packet.Type))
            try
            {
                Hashtable changes = (Hashtable)modifiedPackets[packet.Type];
                Type packetClass = packet.GetType();

                foreach (BlockField bf in changes.Keys)
                {
                    //FIXME: support variable blocks

                    FieldInfo blockField = packetClass.GetField(bf.block);
                    //Type blockClass = blockField.FieldType;
                    object blockObject = blockField.GetValue(packet);
                    MagicSetField(blockObject, bf.field, changes[blockField]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("failed to modify " + packet.Type + ": " + e.Message);
                Console.WriteLine(e.StackTrace);
            }

        if (loggedPackets.Contains(packet.Type))
            LogPacket(packet, endPoint, direction);

        return packet;
    }

    // LogAll: register logging delegates for all packets
    private static void LogAll()
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

                if (pType != PacketType.ChatFromViewer)
                {
                    proxy.AddDelegate(pType, Direction.Incoming, new PacketDelegate(AnalyzeIn));
                    proxy.AddDelegate(pType, Direction.Outgoing, new PacketDelegate(AnalyzeOut));
                }
            }
        }
    }

    // NoLogAll: unregister logging delegates for all packets
    private static void NoLogAll()
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

                if (pType != PacketType.ChatFromViewer)
                {
                    proxy.RemoveDelegate(pType, Direction.Incoming);
                    proxy.RemoveDelegate(pType, Direction.Outgoing);
                }
            }
        }
    }

    // LogPacket: dump a packet to the console
    private static void LogPacket(Packet packet, IPEndPoint endPoint, Direction direction)
    {
        /* if (logGrep != null) {
            bool match = false;
            foreach (Block block in packet.Blocks())
                foreach (Field field in block.Fields) {
                    string value;
                    if (field.Layout.Type == FieldType.Variable)
                        value = DataConvert.toChoppedString(field.Data);
                    else
                        value = field.Data.ToString();
                    if (Regex.Match(packet.Layout.Name + "." + block.Layout.Name + "." + field.Layout.Name + " = " + value, logGrep, RegexOptions.IgnoreCase).Success) {
                        match = true;
                        break;
                    }

                    // try matching variable fields in 0x notation
                    if (field.Layout.Type == FieldType.Variable) {
                        StringWriter sw = new StringWriter();
                        sw.Write("0x");
                        foreach (byte b in (byte[])field.Data)
                            sw.Write("{0:x2}", b);
                        if (Regex.Match(packet.Layout.Name + "." + block.Layout.Name + "." + field.Layout.Name + " = " + sw, logGrep, RegexOptions.IgnoreCase).Success) {
                            match = true;
                            break;
                        }
                    }
                }
            if (!match)
                return;
        } */

        Console.WriteLine("{0} {1,21} {2,5} {3}{4}{5}"
                 , direction == Direction.Incoming ? "<--" : "-->"
                 , endPoint
                 , packet.Header.Sequence
                 , InterpretOptions(packet.Header.Flags)
                 , Environment.NewLine
                 , packet
                 );
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
