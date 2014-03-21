/*
 * GridProxy.cs: implementation of OpenMetaverse proxy library
 *
 * Copyright (c) 2006 Austin Jennings
 * Pregen modifications made by Andrew Ortman on Dec 10, 2006 -> Dec 20, 2006
 * 
 * 
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
using System.IO;
using System.Net;
using System.Xml;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenMetaverse;
using OpenMetaverse.Http;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;
using log4net;
using Nwc.XmlRpc;
using Logger = Nwc.XmlRpc.Logger;

namespace GridProxy
{
    /// <summary>
    /// Proxy Configuration Class
    /// </summary>
    public class ProxyConfig
    {
        /// <summary>
        /// The user agent reported to the remote server
        /// </summary>
        public string userAgent;
        /// <summary>
        /// Email address of the proxy application's author
        /// </summary>
        public string author;
        /// <summary>
        /// The port the proxy server will listen on
        /// </summary>
        public ushort loginPort = 8080;
        /// <summary>
        /// The IP Address the proxy server will communication with the client on
        /// </summary>
        public IPAddress clientFacingAddress = IPAddress.Loopback;
        /// <summary>
        /// The IP Address the proxy server will communicate with the server on
        /// </summary>
        public IPAddress remoteFacingAddress = IPAddress.Any;
        /// <summary>
        /// The URI of the login server
        /// </summary>
        public Uri remoteLoginUri = new Uri("https://login.agni.lindenlab.com/cgi-bin/login.cgi");

        /// <summary>
        /// construct a default proxy configuration with the specified userAgent and author
        /// </summary>
        /// <param name="userAgent">The user agent reported to the remote server</param>
        /// <param name="author">Email address of the proxy application's author</param>
        public ProxyConfig(string userAgent, string author)
        {
            this.userAgent = userAgent;
            this.author = author;
        }

        /// <summary>
        /// construct a default proxy configuration, parsing command line arguments (try --help)
        /// </summary>
        /// <param name="userAgent">The user agent reported to the remote server</param>
        /// <param name="author">Email address of the proxy application's author</param>
        /// <param name="args">An array containing the parameters to use to override the proxy
        /// servers default settings</param>
        public ProxyConfig(string userAgent, string author, string[] args, bool exitOnError)
            : this(userAgent, author)
        {
            Dictionary<string, ArgumentParser> argumentParsers = new Dictionary<string, ArgumentParser>();
            argumentParsers["help"] = new ArgumentParser(ParseHelp);
            argumentParsers["proxy-help"] = new ArgumentParser(ParseHelp);
            argumentParsers["proxy-login-port"] = new ArgumentParser(ParseLoginPort);
            argumentParsers["proxy-client-facing-address"] = new ArgumentParser(ParseClientFacingAddress);
            argumentParsers["proxy-remote-facing-address"] = new ArgumentParser(ParseRemoteFacingAddress);
            argumentParsers["proxy-remote-login-uri"] = new ArgumentParser(ParseRemoteLoginUri);

            foreach (string arg in args)
            {
                foreach (string argument in argumentParsers.Keys)
                {
                    Match match = (new Regex("^--" + argument + "(?:=(.*))?$")).Match(arg);
                    if (match.Success)
                    {
                        string value;
                        if (match.Groups[1].Captures.Count == 1)
                            value = match.Groups[1].Captures[0].ToString();
                        else
                            value = null;
                        try
                        {
                            ((ArgumentParser)argumentParsers[argument])(value);
                        }
                        catch
                        {
                            Console.WriteLine("invalid value for --" + argument);
                            if (exitOnError)
                            {
                                ParseHelp(null);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }
            }
        }

        private delegate void ArgumentParser(string value);

        private void ParseHelp(string value)
        {
            Console.WriteLine("Proxy command-line arguments:");
            Console.WriteLine("  --help                              display this help");
            Console.WriteLine("  --proxy-login-port=<port>           listen for logins on <port>");
            Console.WriteLine("  --proxy-client-facing-address=<IP>  communicate with client via <IP>");
            Console.WriteLine("  --proxy-remote-facing-address=<IP>  communicate with server via <IP>");
            Console.WriteLine("  --proxy-remote-login-uri=<URI>      use SL login server at <URI>");
            Console.WriteLine("  --log-all                           log all packets by default in Analyst");
            Console.WriteLine("  --log-whitelist=<file>              log packets listed in file, one name per line");
            Console.WriteLine("  --no-log-blacklist=<file>           don't log packets in file, one name per line");
            Console.WriteLine("  --output=<logfile>                  log Analyst output to a file");

            Environment.Exit(1);
        }

        private void ParseLoginPort(string value)
        {
            loginPort = Convert.ToUInt16(value);
        }

        private void ParseClientFacingAddress(string value)
        {
            clientFacingAddress = IPAddress.Parse(value);
        }

        private void ParseRemoteFacingAddress(string value)
        {
            remoteFacingAddress = IPAddress.Parse(value);
        }

        private void ParseRemoteLoginUri(string value)
        {
            remoteLoginUri = new Uri(value);
        }
    }

    // Proxy: OpenMetaverse proxy server
    // A Proxy instance is only prepared to deal with one client at a time.
    public class Proxy
    {
        public ProxyConfig proxyConfig;
        private string loginURI;
        
        static List<string> BinaryResponseCaps = new List<string>()
        {
            "GetTexture",
            "GetMesh",
            "GetMesh2"
        };

        /*
         * Proxy Management
         */

        // Proxy: construct a proxy server with the given configuration
        public Proxy(ProxyConfig proxyConfig)
        {
            this.proxyConfig = proxyConfig;

            ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
            ServicePointManager.Expect100Continue = false;
            // Even though this will compile on Mono 2.4, it throws a runtime exception
            //ServicePointManager.ServerCertificateValidationCallback = TrustAllCertificatePolicy.TrustAllCertificateHandler;

            InitializeLoginProxy();
            InitializeSimProxy();
            InitializeCaps();
        }

        object keepAliveLock = new object();

        // Start: begin accepting clients
        public void Start()
        {
            lock (this)
            {
                System.Threading.Monitor.Enter(keepAliveLock);
                (new Thread(new ThreadStart(KeepAlive))).Start();

                RunSimProxy();

                Thread runLoginProxy = new Thread(new ThreadStart(RunLoginProxy));
                runLoginProxy.IsBackground = true;
                runLoginProxy.Name = "Login Proxy";
                runLoginProxy.Start();

                IPEndPoint endPoint = (IPEndPoint)loginServer.LocalEndPoint;
                IPAddress displayAddress;
                if (endPoint.Address == IPAddress.Any)
                    displayAddress = IPAddress.Loopback;
                else
                    displayAddress = endPoint.Address;
                loginURI = "http://" + displayAddress + ":" + endPoint.Port + "/";

                OpenMetaverse.Logger.Log("Proxy ready at " + loginURI, Helpers.LogLevel.Info);
            }
        }

        // Stop: allow foreground threads to die
        public void Stop()
        {
            lock (this)
            {
                System.Threading.Monitor.Exit(keepAliveLock);
            }
        }

        // KeepAlive: blocks until the proxy is free to shut down
        public void KeepAlive()
        {

            OpenMetaverse.Logger.Log(">T> KeepAlive", Helpers.LogLevel.Debug);

            lock (keepAliveLock) { };

            if (loginServer.Connected)
            {
                loginServer.Disconnect(false);
                loginServer.Shutdown(SocketShutdown.Both);
            }

            loginServer.Close();

            OpenMetaverse.Logger.Log("<T< KeepAlive", Helpers.LogLevel.Debug);
        }

        // AddDelegate: add callback packetDelegate for packets of type packetName going direction
        public void AddDelegate(PacketType packetType, Direction direction, PacketDelegate packetDelegate)
        {
            lock (this)
            {
                Dictionary<PacketType, List<PacketDelegate>> delegates = (direction == Direction.Incoming ? incomingDelegates : outgoingDelegates);
                if (!delegates.ContainsKey(packetType))
                {
                    delegates[packetType] = new List<PacketDelegate>();
                }
                List<PacketDelegate> delegateArray = delegates[packetType];
                if (!delegateArray.Contains(packetDelegate))
                {
                    delegateArray.Add(packetDelegate);
                }
            }
        }

        // RemoveDelegate: remove callback for packets of type packetName going direction
        public void RemoveDelegate(PacketType packetType, Direction direction, PacketDelegate packetDelegate)
        {
            lock (this)
            {
                Dictionary<PacketType, List<PacketDelegate>> delegates = (direction == Direction.Incoming ? incomingDelegates : outgoingDelegates);
                if (!delegates.ContainsKey(packetType))
                {
                    return;
                }
                List<PacketDelegate> delegateArray = delegates[packetType];
                if (delegateArray.Contains(packetDelegate))
                {
                    delegateArray.Remove(packetDelegate);
                }
            }
        }

        private Packet callDelegates(Dictionary<PacketType, List<PacketDelegate>> delegates, Packet packet, IPEndPoint remoteEndPoint)
        {
            PacketType origType = packet.Type;
            foreach (PacketDelegate del in delegates[origType])
            {
                try { packet = del(packet, remoteEndPoint); }
                catch (Exception ex) { OpenMetaverse.Logger.Log("Error in packet delegate", Helpers.LogLevel.Warning, ex); }

                // FIXME: how should we handle the packet type changing?
                if (packet == null || packet.Type != origType) break;
            }
            return packet;
        }

        // InjectPacket: send packet to the client or server when direction is Incoming or Outgoing, respectively
        public void InjectPacket(Packet packet, Direction direction)
        {
            lock (this)
            {
                if (activeCircuit == null)
                {
                    // no active circuit; queue the packet for injection once we have one
                    List<Packet> queue = direction == Direction.Incoming ? queuedIncomingInjections : queuedOutgoingInjections;
                    queue.Add(packet);
                }
                else
                    // tell the active sim proxy to inject the packet
                    ((SimProxy)simProxies[activeCircuit]).Inject(packet, direction);
            }
        }

        /*
         * Login Proxy
         */

        private Socket loginServer;
        private int capsReqCount = 0;

        // InitializeLoginProxy: initialize the login proxy
        private void InitializeLoginProxy()
        {
            try
            {
                loginServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                loginServer.Bind(new IPEndPoint(proxyConfig.clientFacingAddress, proxyConfig.loginPort));
                loginServer.Listen(1);
            }
            catch (SocketException e)
            {
                OpenMetaverse.Logger.Log("Socket Exception", Helpers.LogLevel.Error, e);
            }
            catch (ObjectDisposedException e)
            {
                OpenMetaverse.Logger.Log("Socket Object is disposed Exception", Helpers.LogLevel.Error, e);
            }
        }

        // RunLoginProxy: process login requests from clients
        private void RunLoginProxy()
        {
            OpenMetaverse.Logger.Log(">T> RunLoginProxy", Helpers.LogLevel.Debug);

            try
            {
                for (; ; )
                {
                    try
                    {
                        Socket client = loginServer.Accept();

                        Thread connThread = new Thread((ThreadStart)delegate
                        {
                            OpenMetaverse.Logger.Log(">T> LoginProxy", Helpers.LogLevel.Debug);
                            ProxyHTTP(client);
                            OpenMetaverse.Logger.Log("<T< LoginProxy", Helpers.LogLevel.Debug);
                        });

                        connThread.IsBackground = true;
                        connThread.Name = "LoginProxy";
                        connThread.Start();
                    }
                    catch (SocketException e)
                    {
                        // indicates we've told the listener to shutdown
                        if (e.SocketErrorCode == SocketError.Interrupted)
                            break;

                        OpenMetaverse.Logger.Log("Login Failed", Helpers.LogLevel.Error, e);
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                    // send any packets queued for injection
                    if (activeCircuit != null)
                    {
                        lock (this)
                        {
                            SimProxy activeProxy = (SimProxy)simProxies[activeCircuit];
                            foreach (Packet packet in queuedOutgoingInjections)
                                activeProxy.Inject(packet, Direction.Outgoing);
                            queuedOutgoingInjections = new List<Packet>();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                OpenMetaverse.Logger.Log("Exception in RunLoginProxy", Helpers.LogLevel.Error, e);
            }

            OpenMetaverse.Logger.Log("<T< RunLoginProxy", Helpers.LogLevel.Debug);
        }

        private class HandyNetReader
        {
            private NetworkStream netStream;
            private const int BUF_SIZE = 8192;
            private byte[] buf = new byte[BUF_SIZE];
            private int bufFill = 0;

            public HandyNetReader(NetworkStream s)
            {
                netStream = s;
            }

            public byte[] ReadLine()
            {
                int i = -1;
                while (true)
                {
                    i = Array.IndexOf(buf, (byte)'\n', 0, bufFill);
                    if (i >= 0) break;
                    if (bufFill >= BUF_SIZE) return null;
                    if (!ReadMore()) return null;
                }
                if (bufFill < (i + 1)) return null;
                byte[] ret = new byte[i];
                Array.Copy(buf, ret, i);
                Array.Copy(buf, i + 1, buf, 0, bufFill - (i + 1));
                bufFill -= i + 1;
                return ret;
            }

            private bool ReadMore()
            {
                try
                {
                    int n = netStream.Read(buf, bufFill, BUF_SIZE - bufFill);
                    bufFill += n;
                    return n > 0;
                }
                catch
                {
                    return false;
                }
            }

            public int Read(byte[] rbuf, int start, int len)
            {
                int read = 0;
                while (len > bufFill)
                {
                    Array.Copy(buf, 0, rbuf, start, bufFill);
                    start += bufFill; len -= bufFill;
                    read += bufFill; bufFill = 0;
                    if (!ReadMore()) break;
                }
                if (bufFill < len) return 0;
                Array.Copy(buf, 0, rbuf, start, len);
                Array.Copy(buf, len, buf, 0, bufFill - len);
                bufFill -= len; read += len;
                return read;
            }
        }

        // ProxyHTTP: proxy a HTTP request
        private void ProxyHTTP(Socket client)
        {
            NetworkStream netStream = new NetworkStream(client);
            HandyNetReader reader = new HandyNetReader(netStream);

            string line = null;
            int reqNo;
            int contentLength = 0;
            string contentType = "";
            Match match;
            string uri;
            string meth;
            Dictionary<string, string> headers = new Dictionary<string, string>();

            lock (this)
            {
                capsReqCount++; reqNo = capsReqCount;
            }

            byte[] byteLine = reader.ReadLine();
            if (byteLine == null)
            {
                //This dirty hack is part of the LIBOMV-457 workaround
                //The connecting libomv client being proxied can manage to trigger a null from the ReadLine()
                //The happens just after the seed request and is not seen again. TODO find this bug in the library.
                netStream.Close(); client.Close();
                return;
            }

            if (byteLine != null) line = Encoding.UTF8.GetString(byteLine).Replace("\r", "");

            if (line == null)
                throw new Exception("EOF in client HTTP header");

            match = new Regex(@"^(\S+)\s+(\S+)\s+(HTTP/\d\.\d)$").Match(line);

            if (!match.Success)
            {
                OpenMetaverse.Logger.Log("[" + reqNo + "] Bad request!", Helpers.LogLevel.Warning);
                byte[] wr = Encoding.ASCII.GetBytes("HTTP/1.0 400 Bad Request\r\nContent-Length: 0\r\n\r\n");
                netStream.Write(wr, 0, wr.Length);
                netStream.Close(); client.Close();
                return;
            }

            meth = match.Groups[1].Captures[0].ToString();
            uri = match.Groups[2].Captures[0].ToString();

            OpenMetaverse.Logger.Log(String.Format("[{0}] {1}:{2}", reqNo, meth, uri), Helpers.LogLevel.Debug);

            // read HTTP header
            do
            {
                // read one line of the header
                line = Encoding.UTF8.GetString(reader.ReadLine()).Replace("\r", "");

                // check for premature EOF
                if (line == null)
                    throw new Exception("EOF in client HTTP header");

                if (line == "") break;

                match = new Regex(@"^([^:]+):\s*(.*)$").Match(line);

                if (!match.Success)
                {
                    OpenMetaverse.Logger.Log(String.Format("[{0}] Bad Header: '{1}'", reqNo, line), Helpers.LogLevel.Warning);
                    byte[] wr = Encoding.ASCII.GetBytes("HTTP/1.0 400 Bad Request\r\nContent-Length: 0\r\n\r\n");
                    netStream.Write(wr, 0, wr.Length);
                    netStream.Close(); client.Close();
                    return;
                }

                string key = match.Groups[1].Captures[0].ToString();
                string val = match.Groups[2].Captures[0].ToString();
                headers[key.ToLower()] = val;
            } while (line != "");

            if (headers.ContainsKey("content-length"))
            {
                contentLength = Convert.ToInt32(headers["content-length"]);
            }

            if (headers.ContainsKey("content-type"))
            {
                contentType = headers["content-type"];
            }

            // read the HTTP body into a buffer
            byte[] content = new byte[contentLength];
            reader.Read(content, 0, contentLength);

            if (contentLength < 8192)
                OpenMetaverse.Logger.Log(String.Format("[{0}] request length={1}:\n{2}", reqNo, contentLength, Utils.BytesToString(content)), Helpers.LogLevel.Debug);

            if (uri == "/")
            {
                if (contentType == "application/xml+llsd" || contentType == "application/xml")
                {
                    ProxyLoginSD(netStream, content);
                }
                else
                {
                    ProxyLogin(netStream, content);
                }
            }
            else if (new Regex(@"^/https?://.*$").Match(uri).Success)
            {
                ProxyCaps(netStream, meth, uri.Substring(1), headers, content, reqNo);
            }
            else if (new Regex(@"^/https?:/.*$").Match(uri).Success)
            {
                //This is a libomv client and the proxy CAPS URI has been munged by the C# URI class
                //Part of the LIBOMV-457 work around, TODO make this much nicer.
                uri = uri.Replace(":/", "://");
                ProxyCaps(netStream, meth, uri.Substring(1), headers, content, reqNo);
            }
            else
            {
                OpenMetaverse.Logger.Log("404 not found: " + uri, Helpers.LogLevel.Error);
                byte[] wr = Encoding.ASCII.GetBytes("HTTP/1.0 404 Not Found\r\nContent-Length: 0\r\n\r\n");
                netStream.Write(wr, 0, wr.Length);
                netStream.Close(); client.Close();
                return;
            }

            netStream.Close();
            client.Close();

        }

        public ObservableDictionary<string, CapInfo> KnownCaps = new ObservableDictionary<string, CapInfo>();
        //private Dictionary<string, bool> SubHack = new Dictionary<string, bool>();

        private void ProxyCaps(NetworkStream netStream, string meth, string uri, Dictionary<string, string> headers, byte[] content, int reqNo)
        {
            Match match = new Regex(@"^(https?)://([^:/]+)(:\d+)?(/.*)$").Match(uri);
            if (!match.Success)
            {
                OpenMetaverse.Logger.Log("[" + reqNo + "] Malformed proxy URI: " + uri, Helpers.LogLevel.Error);
                byte[] wr = Encoding.ASCII.GetBytes("HTTP/1.0 404 Not Found\r\nContent-Length: 0\r\n\r\n");
                netStream.Write(wr, 0, wr.Length);
                return;
            }

            CapInfo cap = null;
            lock (this)
            {
                string capuri = Regex.Replace(uri, @"/?\?.*$", string.Empty);
               
                if (KnownCaps.ContainsKey(capuri))
                {
                    cap = KnownCaps[capuri];
                }
            }

            CapsRequest capReq = null; bool shortCircuit = false; bool requestFailed = false;
            if (cap != null)
            {
                capReq = new CapsRequest(cap);

                if (cap.ReqFmt == CapsDataFormat.OSD)
                {
                    capReq.Request = OSDParser.DeserializeLLSDXml(content);
                }
                else
                {
                    capReq.Request = OSDParser.DeserializeLLSDXml(content);
                }

                capReq.RawRequest = content;
                capReq.FullUri = uri;

                foreach (CapsDelegate d in cap.GetDelegates())
                {
                    if (d(capReq, CapsStage.Request)) { shortCircuit = true; break; }
                }
            }

            byte[] respBuf = null;
            string consoleMsg = String.Empty;

            if (shortCircuit)
            {
                byte[] wr = Encoding.UTF8.GetBytes("HTTP/1.0 200 OK\r\n");
                netStream.Write(wr, 0, wr.Length);
            }
            else
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(uri);
                req.KeepAlive = false;

                foreach (string header in headers.Keys)
                {
                    if (header == "connection" ||
                       header == "content-length" || header == "date" || header == "expect" ||
                       header == "host" || header == "if-modified-since" || header == "referer" ||
                       header == "transfer-encoding" || header == "user-agent" ||
                       header == "proxy-connection" || header == "accept-encoding")
                    {
                        // can't touch these!
                    }
                    else if (header == "accept")
                    {
                        req.Accept = headers["accept"];
                    }
                    else if (header == "content-type")
                    {
                        req.ContentType = headers["content-type"];
                    }
                    else if (header == "range")
                    {
                        string rangeHeader = headers[header];
                        string[] parts = rangeHeader.Split('=');

                        if (parts.Length == 2)
                        {
                            string[] range = parts[1].Split('-');
                            int from;
                            int to;

                            if (range.Length == 2)
                            {
                                if (int.TryParse(range[0], out from)
                                    && int.TryParse(range[1], out to))
                                {
                                    req.AddRange(parts[0], from, to);
                                }
                            }
                            else if (range.Length == 1 && int.TryParse(range[0], out to))
                            {
                                req.AddRange(parts[0], to);
                            }
                        }
                    }
                    else
                    {
                        req.Headers[header] = headers[header];
                    }
                }
                if (capReq != null)
                {
                    capReq.RequestHeaders = req.Headers;
                }

                req.Method = meth;

                // can't do gets on requests with a content body
                // without throwing a protocol exception. So force it to post 
                // incase our parser stupidly set it to GET due to the viewer 
                // doing something stupid like sending an empty request
                if (content.Length > 0)
                    req.Method = "POST";

                req.ContentLength = content.Length;

                HttpWebResponse resp;
                try
                {
                    if (content.Length > 0)
                    {
                        Stream reqStream = req.GetRequestStream();
                        reqStream.Write(content, 0, content.Length);
                        reqStream.Close();
                    }
                    else if (cap == null)
                    {
                        OpenMetaverse.Logger.Log(string.Format("{0} {1}", req.Method, req.Address.ToString()), Helpers.LogLevel.Info);
                    }
                    resp = (HttpWebResponse)req.GetResponse();
                }

                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.Timeout || e.Status == WebExceptionStatus.SendFailure)
                    {
                        OpenMetaverse.Logger.Log("Request timeout", Helpers.LogLevel.Warning, e);
                        byte[] wr = Encoding.ASCII.GetBytes("HTTP/1.0 504 Proxy Request Timeout\r\nContent-Length: 0\r\n\r\n");
                        netStream.Write(wr, 0, wr.Length);
                        return;
                    }
                    else if (e.Status == WebExceptionStatus.ProtocolError && e.Response != null)
                    {
                        resp = (HttpWebResponse)e.Response; requestFailed = true;
                    }
                    else
                    {
                        OpenMetaverse.Logger.Log("Request error", Helpers.LogLevel.Error, e);
                        byte[] wr = Encoding.ASCII.GetBytes("HTTP/1.0 502 Gateway Error\r\nContent-Length: 0\r\n\r\n"); // FIXME
                        netStream.Write(wr, 0, wr.Length);
                        return;
                    }
                }

                try
                {
                    Stream respStream = resp.GetResponseStream();
                    int read;
                    int length = 0;
                    respBuf = new byte[256];

                    do
                    {
                        read = respStream.Read(respBuf, length, 256);
                        if (read > 0)
                        {
                            length += read;
                            Array.Resize(ref respBuf, length + 256);
                        }
                    } while (read > 0);

                    Array.Resize(ref respBuf, length);

                    if (capReq != null && !requestFailed)
                    {
                        if (cap.RespFmt == CapsDataFormat.OSD)
                        {
                            capReq.Response = OSDParser.DeserializeLLSDXml(respBuf);
                        }
                        else
                        {
                            capReq.Response = OSDParser.DeserializeLLSDXml(respBuf);
                        }
                        capReq.RawResponse = respBuf;

                    }

                    consoleMsg += "[" + reqNo + "] Response from " + uri + "\nStatus: " + (int)resp.StatusCode + " " + resp.StatusDescription + "\n";

                    {
                        byte[] wr = Encoding.UTF8.GetBytes("HTTP/1.0 " + (int)resp.StatusCode + " " + resp.StatusDescription + "\r\n");
                        netStream.Write(wr, 0, wr.Length);
                    }

                    if (capReq != null)
                        capReq.ResponseHeaders = resp.Headers;

                    for (int i = 0; i < resp.Headers.Count; i++)
                    {
                        string key = resp.Headers.Keys[i];
                        string val = resp.Headers[i];
                        string lkey = key.ToLower();
                        if (lkey != "content-length" && lkey != "transfer-encoding" && lkey != "connection")
                        {
                            consoleMsg += key + ": " + val + "\n";
                            byte[] wr = Encoding.UTF8.GetBytes(key + ": " + val + "\r\n");
                            netStream.Write(wr, 0, wr.Length);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // TODO: Should we handle this somehow?
                    OpenMetaverse.Logger.DebugLog("Failed writing output: " + ex.Message);
                }
            }

            if (cap != null && !requestFailed && !capReq.Response.ToString().Equals("undef"))
            {
                foreach (CapsDelegate d in cap.GetDelegates())
                {
                    try
                    {
                        if (d(capReq, CapsStage.Response)) { break; }
                    }
                    catch (InvalidCastException ex)
                    {
                        OpenMetaverse.Logger.Log("Invalid Cast thrown trying to cast OSD to OSDMap: \n'" + capReq.Response.AsString() + "' Length=" + capReq.RawResponse.Length.ToString() + "\n",
                            Helpers.LogLevel.Error, ex);
                    }
                    catch (Exception ex)
                    {
                        OpenMetaverse.Logger.Log("Error firing delegate", Helpers.LogLevel.Error, ex);
                    }
                }

                if (cap.RespFmt == CapsDataFormat.OSD)
                {
                    respBuf = OSDParser.SerializeLLSDXmlBytes((OSD)capReq.Response);
                }
                else
                {
                    respBuf = OSDParser.SerializeLLSDXmlBytes(capReq.Response);
                }
            }


            string respString;
            if (cap == null || cap.RespFmt == CapsDataFormat.Binary)
            {
                respString = "<data>";
            }
            else
            {
                respString = Encoding.UTF8.GetString(respBuf);
            }

            consoleMsg += "\n" + respString + "\n--------";
            OpenMetaverse.Logger.Log(consoleMsg, Helpers.LogLevel.Debug);
            OpenMetaverse.Logger.Log("[" + reqNo + "] Fixed-up response:\n" + respString + "\n--------", Helpers.LogLevel.Debug);

            try
            {
                byte[] wr2 = Encoding.UTF8.GetBytes("Content-Length: " + respBuf.Length + "\r\n\r\n");
                netStream.Write(wr2, 0, wr2.Length);

                netStream.Write(respBuf, 0, respBuf.Length);
            }
            catch (SocketException) { }
            catch (IOException) { }
            catch (Exception e)
            {
                OpenMetaverse.Logger.Log("Exception: Error writing to stream " + e, Helpers.LogLevel.Error, e);
            }

            return;
        }

        private bool FixupSeedCapsResponse(CapsRequest capReq, CapsStage stage)
        {
            if (stage != CapsStage.Response) return false;

            OSDMap nm = new OSDMap();

            if (capReq.Response.Type == OSDType.Map)
            {
                OSDMap m = (OSDMap)capReq.Response;

                foreach (string key in m.Keys)
                {
                    string val = m[key].AsString();

                    if (!String.IsNullOrEmpty(val))
                    {
                        if (!KnownCaps.ContainsKey(val))
                        {
                            CapsDataFormat resFmt = BinaryResponseCaps.Contains(key) ? CapsDataFormat.Binary : CapsDataFormat.OSD;
                            CapsDataFormat reqFmt = CapsDataFormat.OSD;
                            CapInfo newCap = new CapInfo(val, capReq.Info.Sim, key, reqFmt, resFmt);
                            newCap.AddDelegate(new CapsDelegate(KnownCapDelegate));
                            lock (this) { KnownCaps[val] = newCap; }
                        }
                        nm[key] = OSD.FromString(loginURI + val);
                    }
                    else
                    {
                        nm[key] = OSD.FromString(val);
                    }
                }
            }

            capReq.Response = nm;
            return false;
        }

        private Dictionary<string, List<CapsDelegate>> KnownCapsDelegates = new Dictionary<string, List<CapsDelegate>>();


        private void InitializeCaps()
        {
            AddCapsDelegate("EventQueueGet", new CapsDelegate(FixupEventQueueGet));
        }

        public void AddCapsDelegate(string CapName, CapsDelegate capsDelegate)
        {
            lock (this)
            {

                if (!KnownCapsDelegates.ContainsKey(CapName))
                {
                    KnownCapsDelegates[CapName] = new List<CapsDelegate>();
                }
                List<CapsDelegate> delegateArray = KnownCapsDelegates[CapName];
                if (!delegateArray.Contains(capsDelegate))
                {
                    delegateArray.Add(capsDelegate);
                }
            }
        }

        public void RemoveCapRequestDelegate(string CapName, CapsDelegate capsDelegate)
        {
            lock (this)
            {

                if (!KnownCapsDelegates.ContainsKey(CapName))
                {
                    return;
                }
                List<CapsDelegate> delegateArray = KnownCapsDelegates[CapName];
                if (delegateArray.Contains(capsDelegate))
                {
                    delegateArray.Remove(capsDelegate);
                }
            }
        }

        private bool KnownCapDelegate(CapsRequest capReq, CapsStage stage)
        {
            lock (this)
            {
                if (!KnownCapsDelegates.ContainsKey(capReq.Info.CapType))
                    return false;

                if (stage == CapsStage.Response)
                {
                    if (capReq.Response != null && capReq.Response is OSDMap)
                    {
                        OSDMap map = (OSDMap)capReq.Response;

                        if (map.ContainsKey("uploader"))
                        {
                            string val = map["uploader"].AsString();

                            if (!KnownCaps.ContainsKey(val))
                            {
                                CapInfo newCap = new CapInfo(val, capReq.Info.Sim, capReq.Info.CapType, CapsDataFormat.Binary, CapsDataFormat.OSD);
                                newCap.AddDelegate(new CapsDelegate(KnownCapDelegate));
                                lock (this) { KnownCaps[val] = newCap; }
                            }

                            map["uploader"] = OSD.FromString(loginURI + val);
                        }
                    }
                }

                List<CapsDelegate> delegates = KnownCapsDelegates[capReq.Info.CapType];

                foreach (CapsDelegate d in delegates)
                {
                    if (d(capReq, stage)) { return true; }
                }
            }

            return false;
        }

        private bool FixupEventQueueGet(CapsRequest capReq, CapsStage stage)
        {
            if (stage != CapsStage.Response) return false;

            OSDMap map = null;
            if (capReq.Response is OSDMap)
                map = (OSDMap)capReq.Response;
            else return false;

            OSDArray array = null;
            if (map.ContainsKey("events") && map["events"] is OSDArray)
                array = (OSDArray)map["events"];
            else
                return false;

            for (int i = 0; i < array.Count; i++)
            {
                OSDMap evt = (OSDMap)array[i];

                string message = evt["message"].AsString();
                OSDMap body = (OSDMap)evt["body"];

                if (message == "TeleportFinish" || message == "CrossedRegion")
                {
                    OSDMap info = null;
                    if (message == "TeleportFinish")
                        info = (OSDMap)(((OSDArray)body["Info"])[0]);
                    else
                        info = (OSDMap)(((OSDArray)body["RegionData"])[0]);
                    byte[] bytes = info["SimIP"].AsBinary();
                    uint simIP = Utils.BytesToUInt(bytes);
                    ushort simPort = (ushort)info["SimPort"].AsInteger();
                    string capsURL = info["SeedCapability"].AsString();

                    GenericCheck(ref simIP, ref simPort, ref capsURL, capReq.Info.Sim == activeCircuit);

                    info["SeedCapability"] = OSD.FromString(capsURL);
                    bytes[0] = (byte)(simIP % 256);
                    bytes[1] = (byte)((simIP >> 8) % 256);
                    bytes[2] = (byte)((simIP >> 16) % 256);
                    bytes[3] = (byte)((simIP >> 24) % 256);
                    info["SimIP"] = OSD.FromBinary(bytes);
                    info["SimPort"] = OSD.FromInteger(simPort);
                }
                else if (message == "EnableSimulator")
                {
                    OSDMap info = null;
                    info = (OSDMap)(((OSDArray)body["SimulatorInfo"])[0]);
                    byte[] bytes = info["IP"].AsBinary();
                    uint IP = Utils.BytesToUInt(bytes);
                    ushort Port = (ushort)info["Port"].AsInteger();
                    string capsURL = null;

                    GenericCheck(ref IP, ref Port, ref capsURL, capReq.Info.Sim == activeCircuit);

                    bytes[0] = (byte)(IP % 256);
                    bytes[1] = (byte)((IP >> 8) % 256);
                    bytes[2] = (byte)((IP >> 16) % 256);
                    bytes[3] = (byte)((IP >> 24) % 256);
                    info["IP"] = OSD.FromBinary(bytes);
                    info["Port"] = OSD.FromInteger(Port);
                }
                else if (message == "EstablishAgentCommunication")
                {
                    string ipAndPort = body["sim-ip-and-port"].AsString();
                    string[] pieces = ipAndPort.Split(':');
                    byte[] bytes = IPAddress.Parse(pieces[0]).GetAddressBytes();
                    uint simIP = Utils.BytesToUInt(bytes);
                    ushort simPort = (ushort)Convert.ToInt32(pieces[1]);

                    string capsURL = body["seed-capability"].AsString();

                    OpenMetaverse.Logger.Log("DEBUG: Got EstablishAgentCommunication for " + ipAndPort + " with seed cap " + capsURL, Helpers.LogLevel.Debug);

                    GenericCheck(ref simIP, ref simPort, ref capsURL, false);
                    body["seed-capability"] = OSD.FromString(capsURL);
                    string ipport = String.Format("{0}:{1}", new IPAddress(simIP), simPort);
                    body["sim-ip-and-port"] = OSD.FromString(ipport);

                    OpenMetaverse.Logger.Log("DEBUG: Modified EstablishAgentCommunication to " + body["sim-ip-and-port"].AsString() + " with seed cap " + capsURL, Helpers.LogLevel.Debug);
                }
            }
            return false;
        }

        private void ProxyLogin(NetworkStream netStream, byte[] content)
        {
            lock (this)
            {
                // incase some silly person tries to access with their web browser
                if (content.Length <= 0)
                    return;

                // convert the body into an XML-RPC request
                XmlRpcRequest request = (XmlRpcRequest)(new XmlRpcRequestDeserializer()).Deserialize(Encoding.UTF8.GetString(content));

                // call the loginRequestDelegate
                lock (loginRequestDelegates)
                {
                    foreach (XmlRpcRequestDelegate d in loginRequestDelegates)
                    {
                        try { d(this, new XmlRpcRequestEventArgs(request)); }
                        //try { d(request); }
                        catch (Exception e) { OpenMetaverse.Logger.Log("Exception in login request delegate" + e, Helpers.LogLevel.Error, e); }
                    }
                }
                XmlRpcResponse response;
                try
                {
                    // forward the XML-RPC request to the server
                    response = (XmlRpcResponse)request.Send(proxyConfig.remoteLoginUri.ToString(),
                        30 * 1000); // 30 second timeout
                }
                catch (Exception e)
                {
                    OpenMetaverse.Logger.Log("Error during login response", Helpers.LogLevel.Error, e);
                    return;
                }

                System.Collections.Hashtable responseData;
                try
                {
                    responseData = (System.Collections.Hashtable)response.Value;
                }
                catch (Exception e)
                {
                    OpenMetaverse.Logger.Log(e.Message, Helpers.LogLevel.Error);
                    return;
                }

                // proxy any simulator address given in the XML-RPC response
                if (responseData.Contains("sim_ip") && responseData.Contains("sim_port"))
                {
                    IPEndPoint realSim = new IPEndPoint(IPAddress.Parse((string)responseData["sim_ip"]), Convert.ToUInt16(responseData["sim_port"]));
                    IPEndPoint fakeSim = ProxySim(realSim);
                    responseData["sim_ip"] = fakeSim.Address.ToString();
                    responseData["sim_port"] = fakeSim.Port;
                    activeCircuit = realSim;
                }

                // start a new proxy session
                Reset();

                if (responseData.Contains("seed_capability"))
                {
                    CapInfo info = new CapInfo((string)responseData["seed_capability"], activeCircuit, "SeedCapability");
                    info.AddDelegate(new CapsDelegate(FixupSeedCapsResponse));

                    KnownCaps[(string)responseData["seed_capability"]] = info;
                    responseData["seed_capability"] = loginURI + responseData["seed_capability"];
                }

                // forward the XML-RPC response to the client
                StreamWriter writer = new StreamWriter(netStream);
                writer.Write("HTTP/1.0 200 OK\r\n");
                writer.Write("Content-type: text/xml\r\n");
                writer.Write("\r\n");

                XmlTextWriter responseWriter = new XmlTextWriter(writer);
                XmlRpcResponseSerializer.Singleton.Serialize(responseWriter, response);
                responseWriter.Close(); writer.Close();

                lock (loginResponseDelegates)
                {
                    foreach (XmlRpcResponseDelegate d in loginResponseDelegates)
                    {
                        try { d(response); }
                        catch (Exception e) { OpenMetaverse.Logger.Log("Exception in login response delegate" + e, Helpers.LogLevel.Error, e); }
                    }
                }

            }
        }

        private void ProxyLoginSD(NetworkStream netStream, byte[] content)
        {
            lock (this)
            {
                AutoResetEvent remoteComplete = new AutoResetEvent(false);
                CapsClient loginRequest = new CapsClient(proxyConfig.remoteLoginUri);
                OSD response = null;
                loginRequest.OnComplete += new CapsClient.CompleteCallback(
                    delegate(CapsClient client, OSD result, Exception error)
                    {
                        if (error == null)
                        {
                            if (result != null && result.Type == OSDType.Map)
                            {
                                response = result;
                            }
                        }
                        remoteComplete.Set();
                    }
                    );
                loginRequest.BeginGetResponse(content, "application/llsd+xml", 1000 * 100);
                remoteComplete.WaitOne(1000 * 100, false);

                if (response == null)
                {
                    byte[] wr = Encoding.ASCII.GetBytes("HTTP/1.0 500 Internal Server Error\r\nContent-Length: 0\r\n\r\n");
                    netStream.Write(wr, 0, wr.Length);
                    return;
                }

                OSDMap map = (OSDMap)response;

                OSD llsd;
                string sim_port = null, sim_ip = null, seed_capability = null;
                map.TryGetValue("sim_port", out llsd);
                if (llsd != null) sim_port = llsd.AsString();
                map.TryGetValue("sim_ip", out llsd);
                if (llsd != null) sim_ip = llsd.AsString();
                map.TryGetValue("seed_capability", out llsd);
                if (llsd != null) seed_capability = llsd.AsString();

                if (sim_port == null || sim_ip == null || seed_capability == null)
                {
                    if (map != null)
                    {
                        OpenMetaverse.Logger.Log("Connection to server failed, returned LLSD error follows:\n" + map.ToString(), Helpers.LogLevel.Error);
                    }
                    byte[] wr = Encoding.ASCII.GetBytes("HTTP/1.0 500 Internal Server Error\r\nContent-Length: 0\r\n\r\n");
                    netStream.Write(wr, 0, wr.Length);
                    return;
                }

                IPEndPoint realSim = new IPEndPoint(IPAddress.Parse(sim_ip), Convert.ToUInt16(sim_port));
                IPEndPoint fakeSim = ProxySim(realSim);
                map["sim_ip"] = OSD.FromString(fakeSim.Address.ToString());
                map["sim_port"] = OSD.FromInteger(fakeSim.Port);
                activeCircuit = realSim;

                // start a new proxy session
                Reset();

                CapInfo info = new CapInfo(seed_capability, activeCircuit, "SeedCapability");
                info.AddDelegate(new CapsDelegate(FixupSeedCapsResponse));

                KnownCaps[seed_capability] = info;
                map["seed_capability"] = OSD.FromString(loginURI + seed_capability);

                StreamWriter writer = new StreamWriter(netStream);
                writer.Write("HTTP/1.0 200 OK\r\n");
                writer.Write("Content-type: application/xml+llsd\r\n");
                writer.Write("\r\n");
                writer.Write(OSDParser.SerializeLLSDXmlString(response));
                writer.Close();
            }
        }

        /*
         * Sim Proxy
         */

        private Socket simFacingSocket;
        public IPEndPoint activeCircuit = null;
        private Dictionary<IPEndPoint, IPEndPoint> proxyEndPoints = new Dictionary<IPEndPoint, IPEndPoint>();
        private Dictionary<IPEndPoint, SimProxy> simProxies = new Dictionary<IPEndPoint, SimProxy>();
        private Dictionary<EndPoint, SimProxy> proxyHandlers = new Dictionary<EndPoint, SimProxy>();
        //private XmlRpcRequestDelegate loginRequestDelegate = null;
        //private XmlRpcResponseDelegate loginResponseDelegate = null;

        public List<XmlRpcRequestDelegate> loginRequestDelegates = new List<XmlRpcRequestDelegate>();
        public List<XmlRpcResponseDelegate> loginResponseDelegates = new List<XmlRpcResponseDelegate>();

        private Dictionary<PacketType, List<PacketDelegate>> incomingDelegates = new Dictionary<PacketType, List<PacketDelegate>>();
        private Dictionary<PacketType, List<PacketDelegate>> outgoingDelegates = new Dictionary<PacketType, List<PacketDelegate>>();
        private List<Packet> queuedIncomingInjections = new List<Packet>();
        private List<Packet> queuedOutgoingInjections = new List<Packet>();

        // InitializeSimProxy: initialize the sim proxy
        private void InitializeSimProxy()
        {
            InitializeAddressCheckers();

            simFacingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            simFacingSocket.Bind(new IPEndPoint(proxyConfig.remoteFacingAddress, 0));
            Reset();
        }

        // Reset: start a new session
        private void Reset()
        {
            foreach (SimProxy simProxy in simProxies.Values)
                simProxy.Reset();

            KnownCaps.Clear();
        }

        private byte[] receiveBuffer = new byte[8192];
        private byte[] zeroBuffer = new byte[8192];
        private EndPoint remoteEndPoint = (EndPoint)new IPEndPoint(IPAddress.Any, 0);

        // RunSimProxy: start listening for packets from remote sims
        private void RunSimProxy()
        {
            simFacingSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref remoteEndPoint, new AsyncCallback(ReceiveFromSim), null);
        }

        // ReceiveFromSim: packet received from a remote sim
        private void ReceiveFromSim(IAsyncResult ar)
        {
            lock (this)
                try
                {
                    //if (!simFacingSocket.Connected) return;
                    // pause listening and fetch the packet
                    bool needsZero = false;
                    bool needsCopy = true;
                    int length;
                    length = simFacingSocket.EndReceiveFrom(ar, ref remoteEndPoint);

                    if (proxyHandlers.ContainsKey(remoteEndPoint))
                    {
                        // find the proxy responsible for forwarding this packet
                        SimProxy simProxy = (SimProxy)proxyHandlers[remoteEndPoint];

                        // interpret the packet according to the SL protocol
                        Packet packet;
                        int end = length - 1;

                        packet = Packet.BuildPacket(receiveBuffer, ref end, zeroBuffer);

                        // check for ACKs we're waiting for
                        packet = simProxy.CheckAcks(packet, Direction.Incoming, ref length, ref needsCopy);

                        // modify sequence numbers to account for injections
                        uint oldSequence = packet.Header.Sequence;
                        packet = simProxy.ModifySequence(packet, Direction.Incoming, ref length, ref needsCopy);

                        // keep track of sequence numbers
                        if (packet.Header.Sequence > simProxy.incomingSequence)
                            simProxy.incomingSequence = packet.Header.Sequence;

                        // check the packet for addresses that need proxying
                        if (incomingCheckers.ContainsKey(packet.Type))
                        {
                            /* if (needsZero) {
                                length = Helpers.ZeroDecode(packet.Header.Data, length, zeroBuffer);
                                packet.Header.Data = zeroBuffer;
                                needsZero = false;
                            } */

                            Packet newPacket = ((AddressChecker)incomingCheckers[packet.Type])(packet);
                            SwapPacket(packet, newPacket);
                            packet = newPacket;
                            needsCopy = false;
                        }

                        // pass the packet to any callback delegates
                        if (incomingDelegates.ContainsKey(packet.Type))
                        {
                            /* if (needsZero) {
                                length = Helpers.ZeroDecode(packet.Header.Data, length, zeroBuffer);
                                packet.Header.Data = zeroBuffer;
                                needsCopy = true;
                            } */

                            if (packet.Header.AckList != null && needsCopy)
                            {
                                uint[] newAcks = new uint[packet.Header.AckList.Length];
                                Array.Copy(packet.Header.AckList, 0, newAcks, 0, newAcks.Length);
                                packet.Header.AckList = newAcks; // FIXME
                            }

                            try
                            {
                                Packet newPacket = callDelegates(incomingDelegates, packet, (IPEndPoint)remoteEndPoint);
                                if (newPacket == null)
                                {
                                    if (packet.Header.Reliable)
                                        simProxy.Inject(SpoofAck(oldSequence), Direction.Outgoing);

                                    if (packet.Header.AppendedAcks)
                                        packet = SeparateAck(packet);
                                    else
                                        packet = null;
                                }
                                else
                                {
                                    bool oldReliable = packet.Header.Reliable;
                                    bool newReliable = newPacket.Header.Reliable;
                                    if (oldReliable && !newReliable)
                                        simProxy.Inject(SpoofAck(oldSequence), Direction.Outgoing);
                                    else if (!oldReliable && newReliable)
                                        simProxy.WaitForAck(packet, Direction.Incoming);

                                    SwapPacket(packet, newPacket);
                                    packet = newPacket;
                                }
                            }
                            catch (Exception e)
                            {
                                OpenMetaverse.Logger.Log("Exception in incoming delegate", Helpers.LogLevel.Error, e);
                            }

                            if (packet != null)
                                simProxy.SendPacket(packet, false);
                        }
                        else
                            simProxy.SendPacket(packet, needsZero);
                    }
                    else
                        // ignore packets from unknown peers
                        OpenMetaverse.Logger.Log("Dropping packet from unknown peer " + remoteEndPoint, Helpers.LogLevel.Warning);
                }
                catch (Exception e)
                {
                    OpenMetaverse.Logger.Log("Error processing incoming packet from simulator", Helpers.LogLevel.Error, e);
                }
                finally
                {
                    // resume listening
                    try
                    {
                        simFacingSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None,
                            ref remoteEndPoint, new AsyncCallback(ReceiveFromSim), null);
                    }
                    catch (Exception e)
                    {
                        OpenMetaverse.Logger.Log("Listener Socket Exception", Helpers.LogLevel.Error, e);
                    }
                }
        }

        // SendPacket: send a packet to a sim from our fake client endpoint
        public void SendPacket(Packet packet, IPEndPoint endPoint, bool skipZero)
        {

            byte[] buffer = packet.ToBytes();
            if (skipZero || !packet.Header.Zerocoded)
                simFacingSocket.SendTo(buffer, buffer.Length, SocketFlags.None, endPoint);
            else
            {
                int zeroLength = Helpers.ZeroEncode(buffer, buffer.Length, zeroBuffer);
                simFacingSocket.SendTo(zeroBuffer, zeroLength, SocketFlags.None, endPoint);
            }
        }

        // SpoofAck: create an ACK for the given packet
        public Packet SpoofAck(uint sequence)
        {
            PacketAckPacket spoof = new PacketAckPacket();
            spoof.Packets = new PacketAckPacket.PacketsBlock[1];
            spoof.Packets[0] = new PacketAckPacket.PacketsBlock();
            spoof.Packets[0].ID = sequence;
            return (Packet)spoof;
        }

        // SeparateAck: create a standalone PacketAck for packet's appended ACKs
        public Packet SeparateAck(Packet packet)
        {
            PacketAckPacket seperate = new PacketAckPacket();
            seperate.Packets = new PacketAckPacket.PacketsBlock[packet.Header.AckList.Length];

            for (int i = 0; i < packet.Header.AckList.Length; ++i)
            {
                seperate.Packets[i] = new PacketAckPacket.PacketsBlock();
                seperate.Packets[i].ID = packet.Header.AckList[i];
            }

            Packet ack = seperate;
            ack.Header.Sequence = packet.Header.Sequence;
            return ack;
        }

        // SwapPacket: copy the sequence number and appended ACKs from one packet to another
        public static void SwapPacket(Packet oldPacket, Packet newPacket)
        {
            newPacket.Header.Sequence = oldPacket.Header.Sequence;

            int oldAcks = oldPacket.Header.AppendedAcks ? oldPacket.Header.AckList.Length : 0;
            int newAcks = newPacket.Header.AppendedAcks ? newPacket.Header.AckList.Length : 0;

            if (oldAcks != 0 || newAcks != 0)
            {
                uint[] newAckList = new uint[oldAcks];
                Array.Copy(oldPacket.Header.AckList, 0, newAckList, 0, oldAcks);

                newPacket.Header.AckList = newAckList;
                newPacket.Header.AppendedAcks = oldPacket.Header.AppendedAcks;
            }
        }

        // ProxySim: return the proxy for the specified sim, creating it if it doesn't exist
        private IPEndPoint ProxySim(IPEndPoint simEndPoint)
        {
            if (proxyEndPoints.ContainsKey(simEndPoint))
                // return the existing proxy
                return (IPEndPoint)proxyEndPoints[simEndPoint];
            else
            {
                // return a new proxy
                SimProxy simProxy = new SimProxy(proxyConfig, simEndPoint, this);
                IPEndPoint fakeSim = simProxy.LocalEndPoint();
                OpenMetaverse.Logger.Log("Creating proxy for " + simEndPoint + " at " + fakeSim, Helpers.LogLevel.Info);
                simProxy.Run();
                proxyEndPoints.Add(simEndPoint, fakeSim);
                simProxies.Add(simEndPoint, simProxy);
                return fakeSim;
            }
        }

        // AddHandler: remember which sim proxy corresponds to a given sim
        private void AddHandler(EndPoint endPoint, SimProxy proxy)
        {
            proxyHandlers.Add(endPoint, proxy);
        }

        // SimProxy: proxy for a single simulator
        private class SimProxy
        {
            //private ProxyConfig proxyConfig;
            private IPEndPoint remoteEndPoint;
            private Proxy proxy;
            private Socket socket;
            public uint incomingSequence;
            public uint outgoingSequence;
            private List<uint> incomingInjections;
            private List<uint> outgoingInjections;
            private uint incomingOffset = 0;
            private uint outgoingOffset = 0;
            private Dictionary<uint, Packet> incomingAcks;
            private Dictionary<uint, Packet> outgoingAcks;
            private List<uint> incomingSeenAcks;
            private List<uint> outgoingSeenAcks;

            // SimProxy: construct a proxy for a single simulator
            public SimProxy(ProxyConfig proxyConfig, IPEndPoint simEndPoint, Proxy proxy)
            {
                //this.proxyConfig = proxyConfig;
                remoteEndPoint = new IPEndPoint(simEndPoint.Address, simEndPoint.Port);
                this.proxy = proxy;
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Bind(new IPEndPoint(proxyConfig.clientFacingAddress, 0));
                proxy.AddHandler(remoteEndPoint, this);
                Reset();
            }

            // Reset: start a new session
            public void Reset()
            {
                incomingSequence = 0;
                outgoingSequence = 0;
                incomingInjections = new List<uint>();
                outgoingInjections = new List<uint>();
                incomingAcks = new Dictionary<uint, Packet>();
                outgoingAcks = new Dictionary<uint, Packet>();
                incomingSeenAcks = new List<uint>();
                outgoingSeenAcks = new List<uint>();
            }

            // BackgroundTasks: resend unacknowledged packets and keep data structures clean
            private void BackgroundTasks()
            {
                try
                {
                    int tick = 1;
                    int incomingInjectionsPoint = 0;
                    int outgoingInjectionsPoint = 0;
                    int incomingSeenAcksPoint = 0;
                    int outgoingSeenAcksPoint = 0;

                    for (; ; Thread.Sleep(1000)) lock (proxy)
                        {
                            if ((tick = (tick + 1) % 60) == 0)
                            {
                                for (int i = 0; i < incomingInjectionsPoint; ++i)
                                {
                                    incomingInjections.RemoveAt(0);
                                    ++incomingOffset;
                                }
                                incomingInjectionsPoint = incomingInjections.Count;

                                for (int i = 0; i < outgoingInjectionsPoint; ++i)
                                {
                                    outgoingInjections.RemoveAt(0);
                                    ++outgoingOffset;
                                }
                                outgoingInjectionsPoint = outgoingInjections.Count;

                                for (int i = 0; i < incomingSeenAcksPoint; ++i)
                                {
                                    incomingAcks.Remove(incomingSeenAcks[0]);
                                    incomingSeenAcks.RemoveAt(0);
                                }
                                incomingSeenAcksPoint = incomingSeenAcks.Count;

                                for (int i = 0; i < outgoingSeenAcksPoint; ++i)
                                {
                                    outgoingAcks.Remove(outgoingSeenAcks[0]);
                                    outgoingSeenAcks.RemoveAt(0);
                                }
                                outgoingSeenAcksPoint = outgoingSeenAcks.Count;
                            }

                            foreach (uint id in incomingAcks.Keys)
                                if (!incomingSeenAcks.Contains(id))
                                {
                                    Packet packet = (Packet)incomingAcks[id];
                                    packet.Header.Resent = true;
                                    SendPacket(packet, false);
                                }

                            foreach (uint id in outgoingAcks.Keys)
                                if (!outgoingSeenAcks.Contains(id))
                                {
                                    Packet packet = (Packet)outgoingAcks[id];
                                    packet.Header.Resent = true;
                                    proxy.SendPacket(packet, remoteEndPoint, false);
                                }
                        }
                }
                catch (Exception e)
                {
                    OpenMetaverse.Logger.Log("Exception running BackgroundTasks", Helpers.LogLevel.Error, e);
                }
            }

            // LocalEndPoint: return the endpoint that the client should communicate with
            public IPEndPoint LocalEndPoint()
            {
                return (IPEndPoint)socket.LocalEndPoint;
            }

            private byte[] receiveBuffer = new byte[8192];
            private byte[] zeroBuffer = new byte[8192];
            private EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            bool firstReceive = true;

            // Run: forward packets from the client to the sim
            public void Run()
            {
                Thread backgroundTasks = new Thread(new ThreadStart(BackgroundTasks));
                backgroundTasks.IsBackground = true;
                backgroundTasks.Start();
                socket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref clientEndPoint, new AsyncCallback(ReceiveFromClient), null);
            }

            // ReceiveFromClient: packet received from the client
            private void ReceiveFromClient(IAsyncResult ar)
            {
                lock (proxy)
                {
                    try
                    {
                        // pause listening and fetch the packet
                        bool needsZero = false;
                        bool needsCopy = true;
                        int length = 0;

                        try { length = socket.EndReceiveFrom(ar, ref clientEndPoint); }
                        catch (SocketException) { }

                        if (length != 0)
                        {
                            // interpret the packet according to the SL protocol
                            int end = length - 1;
                            Packet packet = OpenMetaverse.Packets.Packet.BuildPacket(receiveBuffer, ref end, zeroBuffer);

                            //OpenMetaverse.Logger.Log("-> " + packet.Type + " #" + packet.Header.Sequence, Helpers.LogLevel.Debug);

                            // check for ACKs we're waiting for
                            packet = CheckAcks(packet, Direction.Outgoing, ref length, ref needsCopy);

                            // modify sequence numbers to account for injections
                            uint oldSequence = packet.Header.Sequence;
                            packet = ModifySequence(packet, Direction.Outgoing, ref length, ref needsCopy);

                            // keep track of sequence numbers
                            if (packet.Header.Sequence > outgoingSequence)
                                outgoingSequence = packet.Header.Sequence;

                            // check the packet for addresses that need proxying
                            if (proxy.outgoingCheckers.ContainsKey(packet.Type))
                            {
                                /* if (packet.Header.Zerocoded) {
                                    length = Helpers.ZeroDecode(packet.Header.Data, length, zeroBuffer);
                                    packet.Header.Data = zeroBuffer;
                                    needsZero = false;
                                } */

                                Packet newPacket = ((AddressChecker)proxy.outgoingCheckers[packet.Type])(packet);
                                SwapPacket(packet, newPacket);
                                packet = newPacket;
                                needsCopy = false;
                            }

                            // pass the packet to any callback delegates
                            if (proxy.outgoingDelegates.ContainsKey(packet.Type))
                            {
                                if (packet.Header.AckList != null && needsCopy)
                                {
                                    uint[] newAcks = new uint[packet.Header.AckList.Length];
                                    Array.Copy(packet.Header.AckList, 0, newAcks, 0, newAcks.Length);
                                    packet.Header.AckList = newAcks; // FIXME
                                }

                                try
                                {
                                    Packet newPacket = proxy.callDelegates(proxy.outgoingDelegates, packet, remoteEndPoint);
                                    if (newPacket == null)
                                    {
                                        if (packet.Header.Reliable)
                                            Inject(proxy.SpoofAck(oldSequence), Direction.Incoming);

                                        if (packet.Header.AppendedAcks)
                                            packet = proxy.SeparateAck(packet);
                                        else
                                            packet = null;
                                    }
                                    else
                                    {
                                        bool oldReliable = packet.Header.Reliable;
                                        bool newReliable = newPacket.Header.Reliable;
                                        if (oldReliable && !newReliable)
                                            Inject(proxy.SpoofAck(oldSequence), Direction.Incoming);
                                        else if (!oldReliable && newReliable)
                                            WaitForAck(packet, Direction.Outgoing);

                                        SwapPacket(packet, newPacket);
                                        packet = newPacket;
                                    }
                                }
                                catch (Exception e)
                                {
                                    OpenMetaverse.Logger.Log("exception in outgoing delegate", Helpers.LogLevel.Error, e);
                                }

                                if (packet != null)
                                    proxy.SendPacket(packet, remoteEndPoint, false);
                            }
                            else
                                proxy.SendPacket(packet, remoteEndPoint, needsZero);

                            // send any packets queued for injection
                            if (firstReceive)
                            {
                                firstReceive = false;
                                foreach (Packet queuedPacket in proxy.queuedIncomingInjections)
                                    Inject(queuedPacket, Direction.Incoming);
                                proxy.queuedIncomingInjections = new List<Packet>();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        OpenMetaverse.Logger.Log("Proxy error sending packet", Helpers.LogLevel.Error, e);
                    }
                    finally
                    {
                        // resume listening
                        try
                        {
                            socket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None,
                                ref clientEndPoint, new AsyncCallback(ReceiveFromClient), null);
                        }
                        catch (SocketException e)
                        {
                            OpenMetaverse.Logger.Log("Socket Shutdown: " + e.SocketErrorCode, Helpers.LogLevel.Warning);
                        }
                    }
                }
            }

            // SendPacket: send a packet from the sim to the client via our fake sim endpoint
            public void SendPacket(Packet packet, bool skipZero)
            {
                byte[] buffer = packet.ToBytes();
                if (skipZero || !packet.Header.Zerocoded)
                    socket.SendTo(buffer, buffer.Length, SocketFlags.None, clientEndPoint);
                else
                {
                    int zeroLength = Helpers.ZeroEncode(buffer, buffer.Length, zeroBuffer);
                    socket.SendTo(zeroBuffer, zeroLength, SocketFlags.None, clientEndPoint);
                }
            }

            // Inject: inject a packet
            public void Inject(Packet packet, Direction direction)
            {
                if (direction == Direction.Incoming)
                {
                    if (firstReceive)
                    {
                        proxy.queuedIncomingInjections.Add(packet);
                        return;
                    }

                    incomingInjections.Add(++incomingSequence);
                    packet.Header.Sequence = incomingSequence;
                }
                else
                {
                    outgoingInjections.Add(++outgoingSequence);
                    packet.Header.Sequence = outgoingSequence;
                }

                if (packet.Header.Reliable)
                    WaitForAck(packet, direction);

                if (direction == Direction.Incoming)
                {
                    byte[] buffer = packet.ToBytes();
                    if (!packet.Header.Zerocoded)
                        socket.SendTo(buffer, buffer.Length, SocketFlags.None, clientEndPoint);
                    else
                    {
                        int zeroLength = Helpers.ZeroEncode(buffer, buffer.Length, zeroBuffer);
                        socket.SendTo(zeroBuffer, zeroLength, SocketFlags.None, clientEndPoint);
                    }
                }
                else
                    proxy.SendPacket(packet, remoteEndPoint, false);
            }

            // WaitForAck: take care of resending a packet until it's ACKed
            public void WaitForAck(Packet packet, Direction direction)
            {
                Dictionary<uint, Packet> table = direction == Direction.Incoming ? incomingAcks : outgoingAcks;
                table.Add(packet.Header.Sequence, packet);
            }

            // CheckAcks: check for and remove ACKs of packets we've injected
            public Packet CheckAcks(Packet packet, Direction direction, ref int length, ref bool needsCopy)
            {
                Dictionary<uint, Packet> acks = direction == Direction.Incoming ? outgoingAcks : incomingAcks;
                List<uint> seenAcks = direction == Direction.Incoming ? outgoingSeenAcks : incomingSeenAcks;

                if (acks.Count == 0)
                    return packet;

                // check for embedded ACKs
                if (packet.Type == PacketType.PacketAck)
                {
                    bool changed = false;
                    List<PacketAckPacket.PacketsBlock> newPacketBlocks = new List<PacketAckPacket.PacketsBlock>();
                    foreach (PacketAckPacket.PacketsBlock pb in ((PacketAckPacket)packet).Packets)
                    {
                        uint id = pb.ID;
                        if (acks.ContainsKey(id))
                        {
                            acks.Remove(id);
                            seenAcks.Add(id);
                            changed = true;
                        }
                        else
                        {
                            newPacketBlocks.Add(pb);
                        }
                    }
                    if (changed)
                    {
                        PacketAckPacket newPacket = new PacketAckPacket();
                        newPacket.Packets = new PacketAckPacket.PacketsBlock[newPacketBlocks.Count];

                        int a = 0;
                        foreach (PacketAckPacket.PacketsBlock pb in newPacketBlocks)
                        {
                            newPacket.Packets[a++] = pb;
                        }

                        SwapPacket(packet, (Packet)newPacket);
                        packet = newPacket;
                        needsCopy = false;
                    }
                }

                // check for appended ACKs
                if (packet.Header.AppendedAcks)
                {
                    int ackCount = packet.Header.AckList.Length;
                    for (int i = 0; i < ackCount; )
                    {
                        uint ackID = packet.Header.AckList[i]; // FIXME FIXME FIXME

                        if (acks.ContainsKey(ackID))
                        {
                            uint[] newAcks = new uint[ackCount - 1];
                            Array.Copy(packet.Header.AckList, 0, newAcks, 0, i);
                            Array.Copy(packet.Header.AckList, i + 1, newAcks, i, ackCount - i - 1);
                            packet.Header.AckList = newAcks;
                            --ackCount;
                            acks.Remove(ackID);
                            seenAcks.Add(ackID);
                            needsCopy = false;
                        }
                        else
                            ++i;
                    }
                    if (ackCount == 0)
                    {
                        packet.Header.AppendedAcks = false;
                        packet.Header.AckList = new uint[0];
                    }
                }

                return packet;
            }

            // ModifySequence: modify a packet's sequence number and ACK IDs to account for injections
            public Packet ModifySequence(Packet packet, Direction direction, ref int length, ref bool needsCopy)
            {
                List<uint> ourInjections = direction == Direction.Outgoing ? outgoingInjections : incomingInjections;
                List<uint> theirInjections = direction == Direction.Incoming ? outgoingInjections : incomingInjections;
                uint ourOffset = direction == Direction.Outgoing ? outgoingOffset : incomingOffset;
                uint theirOffset = direction == Direction.Incoming ? outgoingOffset : incomingOffset;

                uint newSequence = (uint)(packet.Header.Sequence + ourOffset);
                foreach (uint injection in ourInjections)
                    if (newSequence >= injection)
                        ++newSequence;

                packet.Header.Sequence = newSequence;

                if (packet.Header.AppendedAcks)
                {
                    int ackCount = packet.Header.AckList.Length;
                    for (int i = 0; i < ackCount; ++i)
                    {
                        //int offset = length - (ackCount - i) * 4 - 1;
                        uint ackID = packet.Header.AckList[i] - theirOffset;

                        for (int j = theirInjections.Count - 1; j >= 0; --j)
                            if (ackID >= (uint)theirInjections[j])
                                --ackID;

                        packet.Header.AckList[i] = ackID;
                    }
                }

                if (packet.Type == PacketType.PacketAck)
                {
                    PacketAckPacket pap = (PacketAckPacket)packet;
                    foreach (PacketAckPacket.PacketsBlock pb in pap.Packets)
                    {
                        uint ackID = (uint)pb.ID - theirOffset;

                        for (int i = theirInjections.Count - 1; i >= 0; --i)
                            if (ackID >= (uint)theirInjections[i])
                                --ackID;

                        pb.ID = ackID;

                    }

                    switch (packet.Header.Frequency)
                    {
                        case PacketFrequency.High: length = 7; break;
                        case PacketFrequency.Medium: length = 8; break;
                        case PacketFrequency.Low: length = 10; break;
                    }

                    needsCopy = false;
                }

                return packet;
            }
        }

        // Checkers swap proxy addresses in for real addresses.  A few constraints:
        //   - Checkers must not alter the incoming packet.
        //   - Checkers must return a freshly built packet, even if nothing's changed.
        //   - The incoming packet's buffer may be longer than the length of the data it contains.
        //   - The incoming packet's buffer must not be used after the checker returns.
        // This is all because checkers may be operating on data that's still in a scratch buffer.
        delegate Packet AddressChecker(Packet packet);

        Dictionary<PacketType, AddressChecker> incomingCheckers = new Dictionary<PacketType, AddressChecker>();
        Dictionary<PacketType, AddressChecker> outgoingCheckers = new Dictionary<PacketType, AddressChecker>();

        // InitializeAddressCheckers: initialize delegates that check packets for addresses that need proxying
        private void InitializeAddressCheckers()
        {
            // TODO: what do we do with mysteries and empty IPs?
            AddMystery(PacketType.OpenCircuit);
            //AddMystery(PacketType.AgentPresenceResponse);

            incomingCheckers.Add(PacketType.TeleportFinish, new AddressChecker(CheckTeleportFinish));
            incomingCheckers.Add(PacketType.CrossedRegion, new AddressChecker(CheckCrossedRegion));
            incomingCheckers.Add(PacketType.EnableSimulator, new AddressChecker(CheckEnableSimulator));
            //incomingCheckers.Add("UserLoginLocationReply", new AddressChecker(CheckUserLoginLocationReply));
        }

        // AddMystery: add a checker delegate that logs packets we're watching for development purposes
        private void AddMystery(PacketType type)
        {
            incomingCheckers.Add(type, new AddressChecker(LogIncomingMysteryPacket));
            outgoingCheckers.Add(type, new AddressChecker(LogOutgoingMysteryPacket));
        }

        // GenericCheck: replace the sim address in a packet with our proxy address
        private void GenericCheck(ref uint simIP, ref ushort simPort, ref string simCaps, bool active)
        {
            IPAddress sim_ip = new IPAddress((long)simIP);

            IPEndPoint realSim = new IPEndPoint(sim_ip, Convert.ToInt32(simPort));
            IPEndPoint fakeSim = ProxySim(realSim);

            simPort = (ushort)fakeSim.Port;
            byte[] bytes = fakeSim.Address.GetAddressBytes();
            simIP = Utils.BytesToUInt(bytes);
            if (simCaps != null && simCaps.Length > 0)
            {
                CapInfo info = new CapInfo(simCaps, realSim, "SeedCapability");
                info.AddDelegate(new CapsDelegate(FixupSeedCapsResponse));
                lock (this)
                {
                    KnownCaps[simCaps] = info;
                }
                simCaps = loginURI + simCaps;
            }

            if (active)
                activeCircuit = realSim;
        }

        // CheckTeleportFinish: check TeleportFinish packets
        private Packet CheckTeleportFinish(Packet packet)
        {
            TeleportFinishPacket tfp = (TeleportFinishPacket)packet;
            string simCaps = Encoding.UTF8.GetString(tfp.Info.SeedCapability).Replace("\0", "");
            GenericCheck(ref tfp.Info.SimIP, ref tfp.Info.SimPort, ref simCaps, true);
            tfp.Info.SeedCapability = Utils.StringToBytes(simCaps);
            return (Packet)tfp;
        }

        // CheckEnableSimulator: check EnableSimulator packets
        private Packet CheckEnableSimulator(Packet packet)
        {
            EnableSimulatorPacket esp = (EnableSimulatorPacket)packet;
            string simCaps = null;
            GenericCheck(ref esp.SimulatorInfo.IP, ref esp.SimulatorInfo.Port, ref simCaps, false);
            return (Packet)esp;
        }

        // CheckCrossedRegion: check CrossedRegion packets
        private Packet CheckCrossedRegion(Packet packet)
        {
            CrossedRegionPacket crp = (CrossedRegionPacket)packet;
            string simCaps = Encoding.UTF8.GetString(crp.RegionData.SeedCapability).Replace("\0", "");
            GenericCheck(ref crp.RegionData.SimIP, ref crp.RegionData.SimPort, ref simCaps, true);
            crp.RegionData.SeedCapability = Utils.StringToBytes(simCaps);
            return (Packet)crp;
        }

        // LogPacket: log a packet dump
        private Packet LogPacket(Packet packet, string type)
        {
            OpenMetaverse.Logger.Log(type + " packet:\n" + packet, Helpers.LogLevel.Info);
            return packet;
        }

        // LogIncomingMysteryPacket: log an incoming packet we're watching for development purposes
        private Packet LogIncomingMysteryPacket(Packet packet)
        {
            return LogPacket(packet, "incoming mystery");
        }

        // LogOutgoingMysteryPacket: log an outgoing packet we're watching for development purposes
        private Packet LogOutgoingMysteryPacket(Packet packet)
        {
            return LogPacket(packet, "outgoing mystery");
        }

        public void AddLoginRequestDelegate(XmlRpcRequestDelegate xmlRpcRequestDelegate)
        {
            lock (loginRequestDelegates)
                if (!loginRequestDelegates.Contains(xmlRpcRequestDelegate))
                    loginRequestDelegates.Add(xmlRpcRequestDelegate);

        }

        public void AddLoginResponseDelegate(XmlRpcResponseDelegate xmlRpcResponseDelegate)
        {
            lock (loginResponseDelegates)
                if (!loginResponseDelegates.Contains(xmlRpcResponseDelegate))
                    loginResponseDelegates.Add(xmlRpcResponseDelegate);
        }
    }


    // Describes the data format of a capability
    public enum CapsDataFormat
    {
        Binary = 0,
        OSD = 1
    }

    // Describes a caps URI
    public class CapInfo
    {
        private string uri;
        private IPEndPoint sim;
        private string type;
        private CapsDataFormat reqFmt;
        private CapsDataFormat respFmt;

        private List<CapsDelegate> Delegates = new List<CapsDelegate>();


        public CapInfo(string URI, IPEndPoint Sim, string CapType)
            :
            this(URI, Sim, CapType, CapsDataFormat.OSD, CapsDataFormat.OSD) { }
        public CapInfo(string URI, IPEndPoint Sim, string CapType, CapsDataFormat ReqFmt, CapsDataFormat RespFmt)
        {
            uri = URI; sim = Sim; type = CapType; reqFmt = ReqFmt; respFmt = RespFmt;
        }
        public string URI
        {
            get { return uri; }
        }
        public string CapType
        {
            get { return type; } /* EventQueueGet, etc */
        }
        public IPEndPoint Sim
        {
            get { return sim; }
        }
        public CapsDataFormat ReqFmt
        {
            get { return reqFmt; } /* expected request format */
        }
        public CapsDataFormat RespFmt
        {
            get { return respFmt; } /* expected response format */
        }

        public void AddDelegate(CapsDelegate deleg)
        {
            lock (this)
            {
                if (!Delegates.Contains(deleg))
                {
                    Delegates.Add(deleg);
                }
            }
        }
        public void RemoveDelegate(CapsDelegate deleg)
        {
            lock (this)
            {
                if (Delegates.Contains(deleg))
                {
                    Delegates.Remove(deleg);
                }
            }
        }

        // inefficient, but avoids potential deadlocks.
        public List<CapsDelegate> GetDelegates()
        {
            lock (this)
            {
                return new List<CapsDelegate>(Delegates);
            }
        }
    }

    // Information associated with a caps request/response
    public class CapsRequest
    {
        public CapsRequest(CapInfo info)
        {
            Info = info;
        }

        public readonly CapInfo Info;

        // The request
        public OSD Request = null;

        // The corresponding response
        public OSD Response = null;

        public byte[] RawRequest = null;
        public byte[] RawResponse = null;

        public WebHeaderCollection RequestHeaders = new WebHeaderCollection();
        public WebHeaderCollection ResponseHeaders = new WebHeaderCollection();

        public string FullUri = string.Empty;

    }

    // XmlRpcRequestDelegate: specifies a delegate to be called for XML-RPC requests
    public delegate void XmlRpcRequestDelegate(object sender, XmlRpcRequestEventArgs e);

    // XmlRpcResponseDelegate: specifies a delegate to be called for XML-RPC responses
    public delegate void XmlRpcResponseDelegate(XmlRpcResponse response);

    // PacketDelegate: specifies a delegate to be called when a packet passes through the proxy
    public delegate Packet PacketDelegate(Packet packet, IPEndPoint endPoint);

    // Delegate for a caps request. Generally called twice - first with stage = CapsStage.Request
    // before the request is sent, then with stage = CapsStage.Response when the response is
    // received. Returning true causes all the subsequent delegates in that stage to be skipped,
    // and in the case of CapsStage.Request also prevents the request being forwarded. In this
    // case, you should set req.Response to the response you want to return.
    // Can modify req.Request and req.Response, with the expected effects.
    public delegate bool CapsDelegate(CapsRequest req, CapsStage stage);

    // Direction: specifies whether a packet is going to the client (Incoming) or to a sim (Outgoing)
    public enum Direction
    {
        Incoming,
        Outgoing
    }
    public enum CapsStage
    {
        Request,
        Response
    }

    public class XmlRpcRequestEventArgs : EventArgs
    {
        public XmlRpcRequest m_Request;

        public XmlRpcRequestEventArgs(XmlRpcRequest request)
        {
            this.m_Request = request;
        }
    }
}


