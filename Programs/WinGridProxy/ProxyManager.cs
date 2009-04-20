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
using GridProxy;
using Nwc.XmlRpc;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;

namespace WinGridProxy
{
    public class ProxyManager
    {
        public delegate void PacketLogHandler(Packet packet, Direction direction, IPEndPoint endpoint);
        public static event PacketLogHandler OnPacketLog;

        public delegate void MessageLogHandler(CapsRequest req, CapsStage stage);
        public static event MessageLogHandler OnMessageLog;

        public delegate void LoginLogHandler(object request, Direction direction);
        public static event LoginLogHandler OnLoginResponse;

        public delegate void CapsAddedHandler(CapInfo cap);
        public static event CapsAddedHandler OnCapabilityAdded;

        private string _Port;
        private string _ListenIP;
        private string _LoginURI;

        ProxyFrame Proxy;

        public ProxyManager(string port, string listenIP, string loginUri)
        {

            _Port = string.Format("--proxy-login-port={0}", port);

            IPAddress remoteIP; // not used
            if (IPAddress.TryParse(listenIP, out remoteIP))
                _ListenIP = String.Format("--proxy-client-facing-address={0}", listenIP);
            else
                _ListenIP = "--proxy-client-facing-address=127.0.0.1";

            if (String.IsNullOrEmpty(loginUri))
                _LoginURI = "https://login.agni.lindenlab.com/cgi-bin/login.cgi";
            else
                _LoginURI = loginUri;


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
            

            Proxy.proxy.SetLoginRequestDelegate(new XmlRpcRequestDelegate(LoginRequest));
            Proxy.proxy.SetLoginResponseDelegate(new XmlRpcResponseDelegate(LoginResponse));

            Proxy.proxy.AddCapsDelegate("EventQueueGet", new CapsDelegate(EventQueueGetHandler));

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

        private void LoginRequest(XmlRpcRequest request)
        {
            if (OnLoginResponse != null)
                OnLoginResponse(request, Direction.Outgoing);
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

        private bool EventQueueGetHandler(CapsRequest req, CapsStage stage)
        {
            if (stage == CapsStage.Response)
            {
                OSDMap map = (OSDMap)req.Response;
                OSDArray eventsArray = (OSDArray)map["events"];

                for (int i = 0; i < eventsArray.Count; i++)
                {
                    OSDMap bodyMap = (OSDMap)eventsArray[i];
                    if (OnMessageLog != null)
                    {
                        CapInfo capInfo = new CapInfo(req.Info.URI, req.Info.Sim, bodyMap["message"].AsString());
                        CapsRequest capReq = new CapsRequest(capInfo);
                        capReq.Request = req.Request;
                        capReq.Response = bodyMap;

                        OnMessageLog(capReq, CapsStage.Response);
                    }

                    //Console.WriteLine("[" + i + "] " + bodyMap["message"].AsString());
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
            
        }
    }
}
