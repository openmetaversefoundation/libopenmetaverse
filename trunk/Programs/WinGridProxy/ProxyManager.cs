/*
 * Copyright (c) 2009, openmetaverse.org
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
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using GridProxy;
using Nwc.XmlRpc;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;
using OpenMetaverse;

namespace WinGridProxy
{
    public class ProxyManager
    {
        // fired when a new packet arrives
        public delegate void PacketLogHandler(Packet packet, Direction direction, IPEndPoint endpoint);
        public static event PacketLogHandler OnPacketLog;

        // fired when a message arrives over a known capability
        public delegate void MessageLogHandler(CapsRequest req, CapsStage stage);
        public static event MessageLogHandler OnMessageLog;

        // handle login request/response data
        public delegate void LoginLogHandler(object request, Direction direction);
        public static event LoginLogHandler OnLoginResponse;

        // fired when a new Capability is added to the KnownCaps Dictionary
        public delegate void CapsAddedHandler(CapInfo cap);
        public static event CapsAddedHandler OnCapabilityAdded;

        // Handle messages sent via the EventQueue
        public delegate void EventQueueMessageHandler(CapsRequest req, CapsStage stage);
        public static event EventQueueMessageHandler OnEventMessageLog;

        private string _Port;
        private string _ListenIP;
        private string _LoginURI;

        public ProxyFrame Proxy;

        private Assembly openmvAssembly;

        public ProxyManager(string port, string listenIP, string loginUri)
        {
            openmvAssembly = Assembly.Load("OpenMetaverse");
            if (openmvAssembly == null) throw new Exception("Assembly load exception");

            _Port = string.Format("--proxy-login-port={0}", port);

            IPAddress remoteIP; // not used
            if (IPAddress.TryParse(listenIP, out remoteIP))
                _ListenIP = String.Format("--proxy-client-facing-address={0}", listenIP);
            else
                _ListenIP = "--proxy-client-facing-address=127.0.0.1";

            if (String.IsNullOrEmpty(loginUri))
                _LoginURI = "--proxy-remote-login-uri=https://login.agni.lindenlab.com/cgi-bin/login.cgi";
            else
                _LoginURI = "--proxy-remote-login-uri=" + loginUri;


            string[] args = { _Port, _ListenIP, _LoginURI };
            /*
                help
                proxy-help
                proxy-login-port
                proxy-client-facing-address
                proxy-remote-facing-address
                proxy-remote-login-uri
                verbose
                quiet
             */

            ProxyConfig pc = new ProxyConfig("WinGridProxy", "Jim Radford", args);

            Proxy = new ProxyFrame(args, pc);
            
            Proxy.proxy.AddLoginRequestDelegate(new XmlRpcRequestDelegate(LoginRequest));
            Proxy.proxy.AddLoginResponseDelegate(new XmlRpcResponseDelegate(LoginResponse));

            Proxy.proxy.AddCapsDelegate("EventQueueGet", new CapsDelegate(EventQueueGetHandler));

            // this is so we are informed of any new capabilities that are added to the KnownCaps dictionary
            Proxy.proxy.KnownCaps.AddDelegate(OpenMetaverse.DictionaryEventAction.Add, new OpenMetaverse.DictionaryChangeCallback(KnownCapsAddedHandler));
        }

        public void Start()
        {
            Proxy.proxy.Start();
        }

        public void Stop()
        {
            Proxy.proxy.Stop();
        }

        public void KnownCapsAddedHandler(OpenMetaverse.DictionaryEventAction action, System.Collections.DictionaryEntry de)
        {
            if (OnCapabilityAdded != null)
                OnCapabilityAdded((CapInfo)de.Value);
        }
        
        private void LoginRequest(object sender, XmlRpcRequestEventArgs e)
        {
            if (OnLoginResponse != null)
                OnLoginResponse(e.m_Request, Direction.Outgoing);
        }

        private void LoginResponse(XmlRpcResponse response)
        {
            if (OnLoginResponse != null)
                OnLoginResponse(response, Direction.Incoming);
        }


        internal OpenMetaverse.ObservableDictionary<string, CapInfo> GetCapabilities()
        {
            return Proxy.proxy.KnownCaps;
        }

        internal void AddCapsDelegate(string capsKey, bool add)
        {
            if (add)
                Proxy.proxy.AddCapsDelegate(capsKey, new CapsDelegate(CapsHandler));
            else
                Proxy.proxy.RemoveCapRequestDelegate(capsKey, new CapsDelegate(CapsHandler));

        }

        private bool CapsHandler(CapsRequest req, CapsStage stage)
        {
            if (OnMessageLog != null)
                OnMessageLog(req, stage);
            return false;
        }

        /// <summary>
        /// Process individual messages that arrive via the EventQueue and convert each indvidual event into a format
        /// suitable for processing by the IMessage system
        /// </summary>
        /// <param name="req"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        private bool EventQueueGetHandler(CapsRequest req, CapsStage stage)
        {
            if (stage == CapsStage.Response)
            {
                OSDMap map = (OSDMap)req.Response;
                OSDArray eventsArray = (OSDArray)map["events"];

                for (int i = 0; i < eventsArray.Count; i++)
                {
                    OSDMap bodyMap = (OSDMap)eventsArray[i];
                    if (OnEventMessageLog != null)
                    {
                        CapInfo capInfo = new CapInfo(req.Info.URI, req.Info.Sim, bodyMap["message"].AsString());
                        CapsRequest capReq = new CapsRequest(capInfo);
                        capReq.RequestHeaders = req.RequestHeaders;
                        capReq.ResponseHeaders = req.ResponseHeaders;
                        capReq.Request = null;// req.Request;
                        capReq.RawRequest = null;// req.RawRequest;
                        capReq.RawResponse = OSDParser.SerializeLLSDXmlBytes(bodyMap);
                        capReq.Response = bodyMap;

                        OnEventMessageLog(capReq, CapsStage.Response);
                    }
                }
            }
            return false;
        }

        internal void AddUDPDelegate(PacketType packetType, bool add)
        {
            if (add)
            {
                Proxy.proxy.AddDelegate(packetType, Direction.Incoming, new PacketDelegate(PacketInHandler));
                Proxy.proxy.AddDelegate(packetType, Direction.Outgoing, new PacketDelegate(PacketOutHandler));
            }
            else
            {
                Proxy.proxy.RemoveDelegate(packetType, Direction.Incoming, new PacketDelegate(PacketInHandler));
                Proxy.proxy.RemoveDelegate(packetType, Direction.Outgoing, new PacketDelegate(PacketOutHandler));
            }
        }

        private Packet PacketInHandler(Packet packet, IPEndPoint endPoint)
        {
            if (OnPacketLog != null)
                OnPacketLog(packet, Direction.Incoming, endPoint);

            return packet;
        }

        private Packet PacketOutHandler(Packet packet, IPEndPoint endPoint)
        {
            if (OnPacketLog != null)
                OnPacketLog(packet, Direction.Outgoing, endPoint);

            return packet;
        }

        internal void InjectPacket(string packetData, bool toSimulator)
        {
            Direction direction = Direction.Incoming;
            string name = null;
            string block = null;
            object blockObj = null;
            Type packetClass = null;
            Packet packet = null;

            try
            {
                foreach (string line in packetData.Split(new[] { '\n' }))
                {
                    Match match;

                    if (name == null)
                    {
                        match = (new Regex(@"^\s*(in|out)\s+(\w+)\s*$")).Match(line);
                        if (!match.Success)
                        {
                            OpenMetaverse.Logger.Log("expecting direction and packet name, got: " + line, OpenMetaverse.Helpers.LogLevel.Error);
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
                            OpenMetaverse.Logger.Log("expecting 'in' or 'out', got: " + line, OpenMetaverse.Helpers.LogLevel.Error);
                            return;
                        }

                        name = lineName;
                        packetClass = openmvAssembly.GetType("OpenMetaverse.Packets." + name + "Packet");
                        if (packetClass == null) throw new Exception("Couldn't get class " + name + "Packet");
                        ConstructorInfo ctr = packetClass.GetConstructor(new Type[] { });
                        if (ctr == null) throw new Exception("Couldn't get suitable constructor for " + name + "Packet");
                        packet = (Packet)ctr.Invoke(new object[] { });
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
                            OpenMetaverse.Logger.Log("expecting block name, got: " + line, OpenMetaverse.Helpers.LogLevel.Error);
                            return;
                        }

                        match = (new Regex(@"^\s*(\w+)\s*=\s*(.*)$")).Match(line);
                        if (match.Success)
                        {
                            string lineField = match.Groups[1].Captures[0].ToString();
                            string lineValue = match.Groups[2].Captures[0].ToString();
                            object fval;

                            //FIXME: use of MagicCast inefficient
                            //if (lineValue == "$Value")
                            //    fval = MagicCast(name, block, lineField, value);
                            if (lineValue == "$UUID")
                                fval = UUID.Random();
                            else if (lineValue == "$AgentID")
                                fval = Proxy.AgentID;
                            else if (lineValue == "$SessionID")
                                fval = Proxy.SessionID;
                            else
                                fval = MagicCast(name, block, lineField, lineValue);

                            MagicSetField(blockObj, lineField, fval);
                            continue;
                        }
                        OpenMetaverse.Logger.Log("expecting block name or field, got: " + line, OpenMetaverse.Helpers.LogLevel.Error);
                        return;
                    }
                }

                if (name == null)
                {

                    OpenMetaverse.Logger.Log("expecting direction and packet name, got EOF", OpenMetaverse.Helpers.LogLevel.Error);
                    return;
                }

                packet.Header.Reliable = true;

                Proxy.proxy.InjectPacket(packet, direction);

                OpenMetaverse.Logger.Log("Injected " + name, OpenMetaverse.Helpers.LogLevel.Info);                
            }
            catch (Exception e)
            {
                OpenMetaverse.Logger.Log("failed to injected " + name, OpenMetaverse.Helpers.LogLevel.Error, e);
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
    }



}
