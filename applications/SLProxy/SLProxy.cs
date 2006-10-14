/*
 * SLProxy.cs: implementation of Second Life proxy library
 *
 * Copyright (c) 2006 Austin Jennings
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

//#define DEBUG_SEQUENCE

using Nwc.XmlRpc;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using libsecondlife;
using libsecondlife.Packets;

// SLProxy: proxy library for Second Life
namespace SLProxy {
	// ProxyConfig: configuration for proxy objects
	public class ProxyConfig {
		// userAgent: name of the proxy application
		public string userAgent;
		// author: email address of the proxy application's author
		public string author;
		// protocol: libsecondlife ProtocolManager
		public ProtocolManager protocol;
		// loginPort: port that the login proxy will listen on
		public ushort loginPort = 8080;
		// clientFacingAddress: address from which to communicate with the client
		public IPAddress clientFacingAddress = IPAddress.Loopback;
		// remoteFacingAddress: address from which to communicate with the server
		public IPAddress remoteFacingAddress = IPAddress.Any;
		// remoteLoginUri: URI for Second Life's login server
		public Uri remoteLoginUri = new Uri("https://login.agni.lindenlab.com/cgi-bin/login.cgi");
		// verbose: whether or not to print informative messages
		public bool verbose = true;

		// ProxyConfig: construct a default proxy configuration with the specified userAgent, author, and protocol
		public ProxyConfig(string userAgent, string author, ProtocolManager protocol) {
			this.userAgent = userAgent;
			this.author = author;
			this.protocol = protocol;
		}

		// ProxyConfig: construct a default proxy configuration, parsing command line arguments (try --proxy-help)
		public ProxyConfig(string userAgent, string author, ProtocolManager protocol, string[] args) : this(userAgent, author, protocol) {
			Hashtable argumentParsers = new Hashtable();
			argumentParsers["proxy-help"] = new ArgumentParser(ParseHelp);
			argumentParsers["proxy-login-port"] = new ArgumentParser(ParseLoginPort);
			argumentParsers["proxy-client-facing-address"] = new ArgumentParser(ParseClientFacingAddress);
			argumentParsers["proxy-remote-facing-address"] = new ArgumentParser(ParseRemoteFacingAddress);
			argumentParsers["proxy-remote-login-uri"] = new ArgumentParser(ParseRemoteLoginUri);
			argumentParsers["proxy-verbose"] = new ArgumentParser(ParseVerbose);
			argumentParsers["proxy-quiet"] = new ArgumentParser(ParseQuiet);

			foreach (string arg in args)
				foreach (string argument in argumentParsers.Keys) {
					Match match = (new Regex("^--" + argument + "(?:=(.*))?$")).Match(arg);
					if (match.Success) {
						string value;
						if (match.Groups[1].Captures.Count == 1)
							value = match.Groups[1].Captures[0].ToString();
						else
							value = null;
						try {
							((ArgumentParser)argumentParsers[argument])(value);
						} catch {
							Console.WriteLine("invalid value for --" + argument);
							ParseHelp(null);
						}
					}
				}
		}

		private delegate void ArgumentParser(string value);

		private void ParseHelp(string value) {
			Console.WriteLine("Proxy command-line arguments:"                                                   );
			Console.WriteLine("  --proxy-help                        display this help"                         );
			Console.WriteLine("  --proxy-login-port=<port>           listen for logins on <port>"               );
			Console.WriteLine("  --proxy-client-facing-address=<IP>  communicate with client via <IP>"          );
			Console.WriteLine("  --proxy-remote-facing-address=<IP>  communicate with server via <IP>"          );
			Console.WriteLine("  --proxy-remote-login-uri=<URI>      use SL login server at <URI>"              );
			Console.WriteLine("  --proxy-verbose                     display proxy notifications"               );
			Console.WriteLine("  --proxy-quiet                       suppress proxy notifications"              );

			Environment.Exit(1);
		}

		private void ParseLoginPort(string value) {
			loginPort = Convert.ToUInt16(value);
		}

		private void ParseClientFacingAddress(string value) {
			clientFacingAddress = IPAddress.Parse(value);
		}

		private void ParseRemoteFacingAddress(string value) {
			remoteFacingAddress = IPAddress.Parse(value);
		}

		private void ParseRemoteLoginUri(string value) {
			remoteLoginUri = new Uri(value);
		}

		private void ParseVerbose(string value) {
			if (value != null)
				throw new Exception();

			verbose = true;
		}

		private void ParseQuiet(string value) {
			if (value != null)
				throw new Exception();

			verbose = false;
		}
	}

	// Proxy: Second Life proxy server
	// A Proxy instance is only prepared to deal with one client at a time.
	public class Proxy {
		private ProxyConfig proxyConfig;

		/*
		 * Proxy Management
		 */

		// Proxy: construct a proxy server with the given configuration
		public Proxy(ProxyConfig proxyConfig) {
			this.proxyConfig = proxyConfig;

			InitializeLoginProxy();
			InitializeSimProxy();
		}

		object keepAliveLock = new Object();

		// Start: begin accepting clients
		public void Start() { lock(this) {
			System.Threading.Monitor.Enter(keepAliveLock);
			(new Thread(new ThreadStart(KeepAlive))).Start();

			RunSimProxy();
			Thread runLoginProxy = new Thread(new ThreadStart(RunLoginProxy));
			runLoginProxy.IsBackground = true;
			runLoginProxy.Start();

			IPEndPoint endPoint = (IPEndPoint)loginServer.LocalEndPoint;
			IPAddress displayAddress;
			if (endPoint.Address == IPAddress.Any)
				displayAddress = IPAddress.Loopback;
			else
				displayAddress = endPoint.Address;
			Log("proxy ready at http://" + displayAddress + ":" + endPoint.Port + "/", false);
		}}

		// Stop: allow foreground threads to die
		public void Stop() { lock(this) {
			System.Threading.Monitor.Exit(keepAliveLock);
		}}

		// KeepAlive: blocks until the proxy is free to shut down
		public void KeepAlive() {
			lock(keepAliveLock);
		}

		// SetLoginRequestDelegate: specify a callback loginRequestDelegate that will be called when the client requests login
		public void SetLoginRequestDelegate(XmlRpcRequestDelegate loginRequestDelegate) { lock(this) {
			this.loginRequestDelegate = loginRequestDelegate;
		}}

		// SetLoginResponseDelegate: specify a callback loginResponseDelegate that will be called when the server responds to login
		public void SetLoginResponseDelegate(XmlRpcResponseDelegate loginResponseDelegate) { lock(this) {
			this.loginResponseDelegate = loginResponseDelegate;
		}}

		// AddDelegate: add callback packetDelegate for packets of type packetName going direction
		public void AddDelegate(PacketType type, Direction direction, PacketDelegate packetDelegate)
        {
            lock(this)
            {
			    (direction == Direction.Incoming ? incomingDelegates : outgoingDelegates)[type] = packetDelegate;
		    }
        }

		// RemoveDelegate: remove callback for packets of type packetName going direction
        public void RemoveDelegate(PacketType type, Direction direction)
        {
            lock (this)
            {
			    (direction == Direction.Incoming ? incomingDelegates : outgoingDelegates).Remove(type);
		    }
        }

		// InjectPacket: send packet to the client or server when direction is Incoming or Outgoing, respectively
		public void InjectPacket(Packet packet, Direction direction) { lock(this) {
			if (activeCircuit == null) {
				// no active circuit; queue the packet for injection once we have one
				ArrayList queue = direction == Direction.Incoming ? queuedIncomingInjections : queuedOutgoingInjections;
				queue.Add(packet);
			} else
				// tell the active sim proxy to inject the packet
				((SimProxy)simProxies[activeCircuit]).Inject(packet, direction);
		}}

		// Log: write message to the console if in verbose mode
		private void Log(object message, bool important) {
			if (proxyConfig.verbose || important)
				Console.WriteLine(message);
		}

		/*
		 * Login Proxy
		 */

		private Socket loginServer;

		// InitializeLoginProxy: initialize the login proxy
		private void InitializeLoginProxy() {
			loginServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			loginServer.Bind(new IPEndPoint(proxyConfig.clientFacingAddress, proxyConfig.loginPort));
			loginServer.Listen(1);
		}

		// RunLoginProxy: process login requests from clients
		private void RunLoginProxy() {
		    try {
			for (;;) {
				Socket client = loginServer.Accept();
				IPEndPoint clientEndPoint = (IPEndPoint)client.RemoteEndPoint;

				Log("handling login request from " + clientEndPoint, false);

				NetworkStream networkStream = new NetworkStream(client);
				StreamReader networkReader = new StreamReader(networkStream);
				StreamWriter networkWriter = new StreamWriter(networkStream);

				try {
					ProxyLogin(networkReader, networkWriter);
				} catch (Exception e) {
					Log("login failed: " + e.Message, false);
				}

				networkWriter.Close();
				networkReader.Close();
				networkStream.Close();

				client.Close();

				// send any packets queued for injection
				if (activeCircuit != null) lock(this) {
					SimProxy activeProxy = (SimProxy)simProxies[activeCircuit];
					foreach (Packet packet in queuedOutgoingInjections)
						activeProxy.Inject(packet, Direction.Outgoing);
					queuedOutgoingInjections = new ArrayList();
				}
			}
		    } catch (Exception e) {
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
		    }
		}

		// ProxyLogin: proxy a login request
		private void ProxyLogin(StreamReader reader, StreamWriter writer) { lock(this) {
			string line;
			int contentLength = 0;

			// read HTTP header
			do {
				// read one line of the header
				line = reader.ReadLine();

				// check for premature EOF
				if (line == null)
					throw new Exception("EOF in client HTTP header");

				// look for Content-Length
				Match match = (new Regex(@"Content-Length: (\d+)$")).Match(line);
				if (match.Success)
					contentLength = Convert.ToInt32(match.Groups[1].Captures[0].ToString());
			} while (line != "");

			// read the HTTP body into a buffer
			char[] content = new char[contentLength];
			reader.Read(content, 0, contentLength);

			// convert the body into an XML-RPC request
			XmlRpcRequest request = (XmlRpcRequest)(new XmlRpcRequestDeserializer()).Deserialize(new String(content));

			// call the loginRequestDelegate
			if (loginRequestDelegate != null)
				try {
					loginRequestDelegate(request);
				} catch (Exception e) {
					Log("exception in login request deligate: " + e.Message, true);
					Log(e.StackTrace, true);
				}

			// add our userAgent and author to the request
			Hashtable requestParams = new Hashtable();
			if (proxyConfig.userAgent != null)
				requestParams["user-agent"] = proxyConfig.userAgent;
			if (proxyConfig.author != null)
				requestParams["author"] = proxyConfig.author;
			request.Params.Add(requestParams);

			// forward the XML-RPC request to the server
			XmlRpcResponse response = (XmlRpcResponse)request.Send(proxyConfig.remoteLoginUri.ToString());
			Hashtable responseData = (Hashtable)response.Value;

			// proxy any simulator address given in the XML-RPC response
			if (responseData.Contains("sim_ip") && responseData.Contains("sim_port")) {
				IPEndPoint realSim = new IPEndPoint(IPAddress.Parse((string)responseData["sim_ip"]), Convert.ToUInt16(responseData["sim_port"]));
				IPEndPoint fakeSim = ProxySim(realSim);
				responseData["sim_ip"] = fakeSim.Address.ToString();
				responseData["sim_port"] = fakeSim.Port;
				activeCircuit = realSim;
			}

			// start a new proxy session
			Reset();

			// call the loginResponseDelegate
			if (loginResponseDelegate != null) {
				try {
					loginResponseDelegate(response);
				} catch (Exception e) {
					Log("exception in login response delegate: " + e.Message, true);
					Log(e.StackTrace, true);
				}
			}

			// forward the XML-RPC response to the client
			XmlTextWriter responseWriter = new XmlTextWriter(writer);
			XmlRpcResponseSerializer.Singleton.Serialize(responseWriter, response);
			responseWriter.Close();
		}}

		/*
		 * Sim Proxy
		 */

		private Socket simFacingSocket;
		private IPEndPoint activeCircuit = null;
		private Hashtable proxyEndPoints = new Hashtable();
		private Hashtable simProxies = new Hashtable();
		private Hashtable proxyHandlers = new Hashtable();
		private XmlRpcRequestDelegate loginRequestDelegate = null;
		private XmlRpcResponseDelegate loginResponseDelegate = null;
		private Hashtable incomingDelegates = new Hashtable();
		private Hashtable outgoingDelegates = new Hashtable();
		private ArrayList queuedIncomingInjections = new ArrayList();
		private ArrayList queuedOutgoingInjections = new ArrayList();

		// InitializeSimProxy: initialize the sim proxy
		private void InitializeSimProxy() {
			InitializeAddressCheckers();

			simFacingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			simFacingSocket.Bind(new IPEndPoint(proxyConfig.remoteFacingAddress, 0));
			Reset();
		}

		// Reset: start a new session
		private void Reset() {
			foreach (SimProxy simProxy in simProxies.Values)
				simProxy.Reset();
		}

		private byte[] receiveBuffer = new byte[8192];
		private byte[] zeroBuffer = new byte[8192];
		private EndPoint remoteEndPoint = (EndPoint)new IPEndPoint(IPAddress.Any, 0);

		// RunSimProxy: start listening for packets from remote sims
		private void RunSimProxy() {
			simFacingSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref remoteEndPoint, new AsyncCallback(ReceiveFromSim), null);
		}

		// ReceiveFromSim: packet received from a remote sim
        private void ReceiveFromSim(IAsyncResult ar)
        {
            lock (this) try
                {
                    int length = simFacingSocket.EndReceiveFrom(ar, ref remoteEndPoint);
                    Packet packet = null;

                    if (proxyHandlers.Contains(remoteEndPoint))
                    {
                        // find the proxy responsible for forwarding this packet
                        SimProxy simProxy = (SimProxy)proxyHandlers[remoteEndPoint];

                        int packetEnd = length - 1;
                        //Header header = Header.BuildHeader(receiveBuffer, ref pos, ref packetEnd);
                        packet = Packet.BuildPacket(receiveBuffer, ref packetEnd);

                        // check for ACKs we're waiting for
                        packet = simProxy.CheckAcks(packet, Direction.Incoming);

                        // modify sequence numbers to account for injections
                        ushort oldSequence = packet.Header.Sequence;
                        //simProxy.ModifySequence(header, Direction.Incoming);

                        // keep track of sequence numbers
                        if (packet.Header.Sequence > simProxy.incomingSequence)
                            simProxy.incomingSequence = packet.Header.Sequence;

                        PacketType type = Packet.GetType(packet.Header.ID, packet.Header.Frequency);

                        // check the packet for addresses that need proxying
                        if (incomingCheckers.Contains(type))
                        {
                            packet = ((AddressChecker)incomingCheckers[packet.Type])(packet);
                        }

                        // pass the packet to any callback delegates
                        if (incomingDelegates.Contains(type))
                        {
                            try
                            {
                                Packet newPacket = ((PacketDelegate)incomingDelegates[packet.Type])(packet, (IPEndPoint)remoteEndPoint);

                                if (newPacket == null)
                                {
                                    if (packet.Header.Reliable)
                                        simProxy.Inject(SpoofAck(oldSequence), Direction.Outgoing);
                                    if (packet.Header.AppendedAcks)
                                        packet = SeparateAck(packet.Header.AckList);
                                    else
                                        packet = null;
                                }
                                else
                                {
                                    if (packet.Header.Reliable && !newPacket.Header.Reliable)
                                        simProxy.Inject(SpoofAck(packet.Header.Sequence), Direction.Outgoing);
                                    else if (!packet.Header.Reliable && newPacket.Header.Reliable)
                                        simProxy.WaitForAck(packet, Direction.Incoming);

                                    SwapPacket(packet, newPacket);
                                    packet = newPacket;
                                }
                            }
                            catch (Exception e)
                            {
                                Log("exception in incoming delegate: " + e.Message, true);
                                Log(e.StackTrace, true);
                            }

                            if (packet != null)
                                simProxy.SendPacket(packet);
                        }
                        else
                        {
                            // TODO: We need a way to send raw data, so every packet doesn't have to be unserialized
                            simProxy.SendPacket(packet);
                        }
                    }
                    else
                        // ignore packets from unknown peers
                        Log("dropping packet from " + remoteEndPoint, false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
                finally
                {
                    // resume listening
                    simFacingSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref remoteEndPoint, new AsyncCallback(ReceiveFromSim), null);
                }
        }

		// SendPacket: send a packet to a sim from our fake client endpoint
		public void SendPacket(Packet packet, IPEndPoint endPoint) {
            byte[] buffer = packet.ToBytes();
            int bytes = buffer.Length;

            // Zerocode if needed
            if (packet.Header.Zerocoded)
            {
                byte[] zeroBuffer = new byte[4096];
                bytes = Helpers.ZeroEncode(buffer, bytes, zeroBuffer);
                buffer = zeroBuffer;
            }

		    simFacingSocket.SendTo(buffer, bytes, SocketFlags.None, endPoint);
		}

		// SpoofAck: create an ACK for the given packet
		public Packet SpoofAck(ushort sequence) {
            PacketAckPacket ack = new PacketAckPacket();
            ack.Packets = new PacketAckPacket.PacketsBlock[1];
            ack.Packets[0] = new PacketAckPacket.PacketsBlock();
            ack.Packets[0].ID = sequence;
            ack.Header.Reliable = false;

            return ack;
		}

		// SeparateAck: create a standalone PacketAck for packet's appended ACKs
		public Packet SeparateAck(uint[] acks) {
            PacketAckPacket ack = new PacketAckPacket();
            ack.Packets = new PacketAckPacket.PacketsBlock[acks.Length];

            for (int i = 0; i < acks.Length; i++)
            {
                ack.Packets[i] = new PacketAckPacket.PacketsBlock();
                ack.Packets[i].ID = acks[i];
            }

            ack.Header.Reliable = false;

            return ack;
		}

		// SwapPacket: copy the sequence number and appended ACKs from one packet to another
		public static void SwapPacket(Packet oldPacket, Packet newPacket) {
            newPacket.Header.Sequence = oldPacket.Header.Sequence;
            newPacket.Header.AckList = oldPacket.Header.AckList;
		}

		// ProxySim: return the proxy for the specified sim, creating it if it doesn't exist
		private IPEndPoint ProxySim(IPEndPoint simEndPoint) {
			if (proxyEndPoints.Contains(simEndPoint))
				// return the existing proxy
				return (IPEndPoint)proxyEndPoints[simEndPoint];
			else {
				// return a new proxy
				SimProxy simProxy = new SimProxy(proxyConfig, simEndPoint, this);
				IPEndPoint fakeSim = simProxy.LocalEndPoint();
				Log("creating proxy for " + simEndPoint + " at " + fakeSim, false);
				simProxy.Run();
				proxyEndPoints.Add(simEndPoint, fakeSim);
				simProxies.Add(simEndPoint, simProxy);
				return fakeSim;
			}
		}

		// AddHandler: remember which sim proxy corresponds to a given sim
		private void AddHandler(EndPoint endPoint, SimProxy proxy) {
			proxyHandlers.Add(endPoint, proxy);
		}

		// SimProxy: proxy for a single simulator
		private class SimProxy {
			private ProxyConfig proxyConfig;
			private IPEndPoint remoteEndPoint;
			private Proxy proxy;
			private Socket socket;
			public ushort incomingSequence;
			public ushort outgoingSequence;
			private ArrayList incomingInjections;
			private ArrayList outgoingInjections;
			private ushort incomingOffset = 0;
			private ushort outgoingOffset = 0;
			private Hashtable incomingAcks;
			private Hashtable outgoingAcks;
			private ArrayList incomingSeenAcks;
			private ArrayList outgoingSeenAcks;

			// SimProxy: construct a proxy for a single simulator
			public SimProxy(ProxyConfig proxyConfig, IPEndPoint simEndPoint, Proxy proxy) {
				this.proxyConfig = proxyConfig;
				remoteEndPoint = new IPEndPoint(simEndPoint.Address, simEndPoint.Port);
				this.proxy = proxy;
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				socket.Bind(new IPEndPoint(proxyConfig.clientFacingAddress, 0));
				proxy.AddHandler(remoteEndPoint, this);
				Reset();
			}

			// Reset: start a new session
			public void Reset() {
				incomingSequence = 0;
				outgoingSequence = 0;
				incomingInjections = new ArrayList();
				outgoingInjections = new ArrayList();
				incomingAcks = new Hashtable();
				outgoingAcks = new Hashtable();
				incomingSeenAcks = new ArrayList();
				outgoingSeenAcks = new ArrayList();
			}

			// BackgroundTasks: resend unacknowledged packets and keep data structures clean
			private void BackgroundTasks() { try {
				int tick = 1;
				int incomingInjectionsPoint = 0;
				int outgoingInjectionsPoint = 0;
				int incomingSeenAcksPoint = 0;
				int outgoingSeenAcksPoint = 0;

				for (;; Thread.Sleep(1000)) lock(proxy) {
					if ((tick = (tick + 1) % 60) == 0) {
						for (int i = 0; i < incomingInjectionsPoint; ++i) {
							incomingInjections.RemoveAt(0);
							++incomingOffset;
#if DEBUG_SEQUENCE
							Console.WriteLine("incomingOffset = " + incomingOffset);
#endif
						}
						incomingInjectionsPoint = incomingInjections.Count;

						for (int i = 0; i < outgoingInjectionsPoint; ++i) {
							outgoingInjections.RemoveAt(0);
							++outgoingOffset;
#if DEBUG_SEQUENCE
							Console.WriteLine("outgoingOffset = " + outgoingOffset);
#endif
						}
						outgoingInjectionsPoint = outgoingInjections.Count;

						for (int i = 0; i < incomingSeenAcksPoint; ++i) {
#if DEBUG_SEQUENCE
							Console.WriteLine("incomingAcks.Remove(" + incomingSeenAcks[0] + ")");
#endif
							incomingAcks.Remove(incomingSeenAcks[0]);
							incomingSeenAcks.RemoveAt(0);
						}
						incomingSeenAcksPoint = incomingSeenAcks.Count;

						for (int i = 0; i < outgoingSeenAcksPoint; ++i) {
#if DEBUG_SEQUENCE
							Console.WriteLine("outgoingAcks.Remove(" + outgoingSeenAcks[0] + ")");
#endif
							outgoingAcks.Remove(outgoingSeenAcks[0]);
							outgoingSeenAcks.RemoveAt(0);
						}
						outgoingSeenAcksPoint = outgoingSeenAcks.Count;
					}

					foreach (ushort id in incomingAcks.Keys)
						if (!incomingSeenAcks.Contains(id)) {
							Packet packet = (Packet)incomingAcks[id];
                            packet.Header.Resent = true;
#if DEBUG_SEQUENCE
							Console.WriteLine("RESEND <- " + packet.Layout.Name + " #" + packet.Sequence);
#endif
							SendPacket(packet);
						}

					foreach (ushort id in outgoingAcks.Keys)
						if (!outgoingSeenAcks.Contains(id)) {
							Packet packet = (Packet)outgoingAcks[id];
                            packet.Header.Resent = true;
#if DEBUG_SEQUENCE
							Console.WriteLine("RESEND -> " + packet.Layout.Name + " #" + packet.Sequence);
#endif
							proxy.SendPacket(packet, remoteEndPoint);
						}
				}
			} catch (Exception e) {
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}}

			// LocalEndPoint: return the endpoint that the client should communicate with
			public IPEndPoint LocalEndPoint() {
				return (IPEndPoint)socket.LocalEndPoint;
			}

			private byte[] receiveBuffer = new byte[8192];
			private byte[] zeroBuffer = new byte[8192];
			private EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
			bool firstReceive = true;

			// Run: forward packets from the client to the sim
			public void Run() {
				Thread backgroundTasks = new Thread(new ThreadStart(BackgroundTasks));
				backgroundTasks.IsBackground = true;
				backgroundTasks.Start();
				socket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref clientEndPoint, new AsyncCallback(ReceiveFromClient), null);
			}

			// ReceiveFromClient: packet received from the client
			private void ReceiveFromClient(IAsyncResult ar) { lock(proxy) try {
				// pause listening and fetch the packet
				int length = socket.EndReceiveFrom(ar, ref clientEndPoint);
                int packetEnd = length - 1;

				// interpret the packet according to the SL protocol
                Packet packet = Packet.BuildPacket(receiveBuffer, ref packetEnd);

                // resume listening
                socket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref clientEndPoint, new AsyncCallback(ReceiveFromClient), null);

				// check for ACKs we're waiting for
				packet = CheckAcks(packet, Direction.Outgoing);

				// modify sequence numbers to account for injections
				ushort oldSequence = packet.Header.Sequence;
				ModifySequence(packet.Header, Direction.Outgoing);

				// keep track of sequence numbers
                if (packet.Header.Sequence > outgoingSequence)
                    outgoingSequence = packet.Header.Sequence;

				// check the packet for addresses that need proxying
				if (proxy.outgoingCheckers.Contains(packet.Type)) {
					Packet newPacket = ((AddressChecker)proxy.outgoingCheckers[packet.Type])(packet);
					SwapPacket(packet, newPacket);
					packet = newPacket;
				}

				// pass the packet to any callback delegates
				if (proxy.outgoingDelegates.Contains(packet.Type)) {
					try {
						Packet newPacket = ((PacketDelegate)proxy.outgoingDelegates[packet.Type])(packet, remoteEndPoint);
						if (newPacket == null) {
							if (packet.Header.Reliable)
								Inject(proxy.SpoofAck(oldSequence), Direction.Incoming);

							if (packet.Header.AppendedAcks)
								packet = proxy.SeparateAck(packet.Header.AckList);
							else
								packet = null;
						} else {
							if (packet.Header.Reliable && !newPacket.Header.Reliable)
								Inject(proxy.SpoofAck(oldSequence), Direction.Incoming);
                            else if (!packet.Header.Reliable && newPacket.Header.Reliable)	
								WaitForAck(packet, Direction.Outgoing);

							SwapPacket(packet, newPacket);
							packet = newPacket;
						}
					} catch (Exception e) {
						proxy.Log("exception in outgoing delegate: " + e.Message, true);
						proxy.Log(e.StackTrace, true);
					}

					if (packet != null)
						proxy.SendPacket(packet, remoteEndPoint);
				} else
					proxy.SendPacket(packet, remoteEndPoint);

				// send any packets queued for injection
				if (firstReceive) {
					firstReceive = false;
					foreach (Packet queuedPacket in proxy.queuedIncomingInjections)
						Inject(queuedPacket, Direction.Incoming);
					proxy.queuedIncomingInjections = new ArrayList();
				}
			} catch (Exception e) {
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}}

			// SendPacket: send a packet from the sim to the client via our fake sim endpoint
			public void SendPacket(Packet packet) {
                byte[] buffer = packet.ToBytes();
                int bytes = buffer.Length;

                // Zerocode if needed
                if (packet.Header.Zerocoded)
                {
                    byte[] zeroBuffer = new byte[4096];
                    bytes = Helpers.ZeroEncode(buffer, bytes, zeroBuffer);
                    buffer = zeroBuffer;
                }

                socket.SendTo(buffer, bytes, SocketFlags.None, clientEndPoint);
			}

			// Inject: inject a packet
			public void Inject(Packet packet, Direction direction) {
				if (direction == Direction.Incoming) {
					if (firstReceive) {
						proxy.queuedIncomingInjections.Add(packet);
						return;
					}

					incomingInjections.Add(++incomingSequence);
					packet.Header.Sequence = incomingSequence;
				} else {
					outgoingInjections.Add(++outgoingSequence);
					packet.Header.Sequence = outgoingSequence;
				}

#if DEBUG_SEQUENCE
				Console.WriteLine("INJECT " + (direction == Direction.Incoming ? "<-" : "->") + " " + packet.Layout.Name + " #" + packet.Sequence);
#endif

				if (packet.Header.Reliable)
					WaitForAck(packet, direction);

                if (direction == Direction.Incoming)
                {
                    //byte[] bytes = packet.ToBytes();
                    //socket.SendTo(bytes, bytes.Length, SocketFlags.None, clientEndPoint);
                    SendPacket(packet);
                }
                else
                {
                    proxy.SendPacket(packet, remoteEndPoint);
                }
			}

			// WaitForAck: take care of resending a packet until it's ACKed
			public void WaitForAck(Packet packet, Direction direction) {
				Hashtable table = direction == Direction.Incoming ? incomingAcks : outgoingAcks;
				table.Add(packet.Header.Sequence, packet);
			}

			// CheckAcks: check for and remove ACKs of packets we've injected
			public Packet CheckAcks(Packet packet, Direction direction) {
				Hashtable acks = direction == Direction.Incoming ? outgoingAcks : incomingAcks;
				ArrayList seenAcks = direction == Direction.Incoming ? outgoingSeenAcks : incomingSeenAcks;

				if (acks.Count == 0)
					return packet;

				// check for embedded ACKs
				if (packet.Type == PacketType.PacketAck)
                {
				    bool changed = false;
                    ArrayList newAcks = new ArrayList();
                    PacketAckPacket ackPacket = (PacketAckPacket)packet;

                    foreach (PacketAckPacket.PacketsBlock block in ackPacket.Packets)
                    {
                        if (acks.Contains(block.ID))
                        {
                            seenAcks.Add(block.ID);
                            changed = true;
                        }
                        else
                        {
                            newAcks.Add(block.ID);
                        }
                    }

                    if (changed)
                    {
                        ackPacket.Packets = new PacketAckPacket.PacketsBlock[newAcks.Count];

                        for (int i = 0; i < newAcks.Count; i++)
                        {
                            ackPacket.Packets[i] = new PacketAckPacket.PacketsBlock();
                            ackPacket.Packets[i].ID = (uint)newAcks[i];
                        }
                    }
				}

				// check for appended ACKs
				if (packet.Header.AppendedAcks) {
                    bool changed = false;
                    ArrayList newAcks = new ArrayList();

                    foreach (ushort ack in packet.Header.AckList)
                    {
                        if (acks.Contains(ack))
                        {
                            seenAcks.Add(ack);
                            changed = true;
                        }
                        else
                        {
                            newAcks.Add(ack);
                        }
                    }

                    if (changed)
                    {
                        packet.Header.AckList = new uint[newAcks.Count];

                        for (int i = 0; i < newAcks.Count; i++)
                        {
                            packet.Header.AckList[i] = (uint)newAcks[i];
                        }
                    }
				}

				return packet;
			}

			// ModifySequence: modify a packet's sequence number and ACK IDs to account for injections
			public void ModifySequence(Header header, Direction direction) {
                ArrayList ourInjections = direction == Direction.Outgoing ? outgoingInjections : incomingInjections;
                ArrayList theirInjections = direction == Direction.Incoming ? outgoingInjections : incomingInjections;
                ushort ourOffset = direction == Direction.Outgoing ? outgoingOffset : incomingOffset;
                ushort theirOffset = direction == Direction.Incoming ? outgoingOffset : incomingOffset;

                ushort newSequence = (ushort)(header.Sequence + ourOffset);
                foreach (ushort injection in ourInjections)
                    if (newSequence >= injection)
                        ++newSequence;

                header.Sequence = newSequence;

                //FIXME:
                //if (packet.Header.AppendedAcks) {
                //    int ackCount = packet.Data[length - 1];
                //    for (int i = 0; i < ackCount; ++i) {
                //        int offset = length - (ackCount - i) * 4 - 1;
                //        uint ackID = (uint)(packet.Data[offset + 3] + (packet.Data[offset + 2] << 8)) - theirOffset;

                //        for (int j = theirInjections.Count - 1; j >= 0; --j)
                //            if (ackID >= (ushort)theirInjections[j])
                //                --ackID;

                //        packet.Data[offset + 3] = (byte)(ackID % 256);
                //        packet.Data[offset + 2] = (byte)(ackID / 256);
                //    }
                //}
			}
		}

		// Checkers swap proxy addresses in for real addresses.  A few constraints:
		//   - Checkers must not alter the incoming packet.
		//   - Checkers must return a freshly built packet, even if nothing's changed.
		//   - The incoming packet's buffer may be longer than the length of the data it contains.
		//   - The incoming packet's buffer must not be used after the checker returns.
		// This is all because checkers may be operating on data that's still in a scratch buffer.
		delegate Packet AddressChecker(Packet packet);

		Hashtable incomingCheckers = new Hashtable();
		Hashtable outgoingCheckers = new Hashtable();

		// InitializeAddressCheckers: initialize delegates that check packets for addresses that need proxying
		private void InitializeAddressCheckers() {
			// TODO: what do we do with mysteries and empty IPs?
			AddMystery(PacketType.OpenCircuit);
			AddMystery(PacketType.AgentPresenceResponse);
			incomingCheckers.Add(PacketType.TeleportFinish, new AddressChecker(CheckTeleportFinish));
			// ViewerStats: IP is 0.0.0.0
			incomingCheckers.Add(PacketType.AgentToNewRegion, new AddressChecker(CheckAgentToNewRegion));
			incomingCheckers.Add(PacketType.CrossedRegion, new AddressChecker(CheckCrossedRegion));
			incomingCheckers.Add(PacketType.EnableSimulator, new AddressChecker(CheckEnableSimulator));
			// KickUser: IP is 0.0.0.0
            incomingCheckers.Add(PacketType.UserLoginLocationReply, new AddressChecker(CheckUserLoginLocationReply));
		}

		// AddMystery: add a checker delegate that logs packets we're watching for development purposes
		private void AddMystery(PacketType type) {
			incomingCheckers.Add(type, new AddressChecker(LogIncomingMysteryPacket));
            outgoingCheckers.Add(type, new AddressChecker(LogOutgoingMysteryPacket));
		}

		// CheckTeleportFinish: check TeleportFinish packets
		private Packet CheckTeleportFinish(Packet packet) {
            TeleportFinishPacket teleport = (TeleportFinishPacket)packet;
            IPEndPoint realSim = new IPEndPoint(teleport.Info.SimIP, teleport.Info.SimPort);
            IPEndPoint fakeSim = ProxySim(realSim);

            teleport.Info.SimIP = (uint)fakeSim.Address.Address;
            teleport.Info.SimPort = (ushort)fakeSim.Port;

            return teleport;
		}

		// CheckAgentToNewRegion: check AgentToNewRegion packets
		private Packet CheckAgentToNewRegion(Packet packet) {
            AgentToNewRegionPacket newregion = (AgentToNewRegionPacket)packet;
            IPEndPoint realSim = new IPEndPoint(newregion.RegionData.IP, newregion.RegionData.Port);
            IPEndPoint fakeSim = ProxySim(realSim);

            newregion.RegionData.IP = (uint)fakeSim.Address.Address;
            newregion.RegionData.Port = (ushort)fakeSim.Port;

            return newregion;
		}

		// CheckEnableSimulator: check EnableSimulator packets
		private Packet CheckEnableSimulator(Packet packet) {
            EnableSimulatorPacket enable = (EnableSimulatorPacket)packet;
            IPEndPoint realSim = new IPEndPoint(enable.SimulatorInfo.IP, enable.SimulatorInfo.Port);
            IPEndPoint fakeSim = ProxySim(realSim);

            enable.SimulatorInfo.IP = (uint)fakeSim.Address.Address;
            enable.SimulatorInfo.Port = (ushort)fakeSim.Port;

            return enable;
		}

		// CheckCrossedRegion: check CrossedRegion packets
		private Packet CheckCrossedRegion(Packet packet) {
            CrossedRegionPacket crossed = (CrossedRegionPacket)packet;
            IPEndPoint realSim = new IPEndPoint(crossed.RegionData.SimIP, crossed.RegionData.SimPort);
            IPEndPoint fakeSim = ProxySim(realSim);

            crossed.RegionData.SimIP = (uint)fakeSim.Address.Address;
            crossed.RegionData.SimPort = (ushort)fakeSim.Port;

            return crossed;
		}

		// CheckUserLoginLocationReply: check UserLoginLocationReply packets
		private Packet CheckUserLoginLocationReply(Packet packet) {
            UserLoginLocationReplyPacket location = (UserLoginLocationReplyPacket)packet;
            IPEndPoint realSim = new IPEndPoint(location.SimulatorBlock.IP, location.SimulatorBlock.Port);
            IPEndPoint fakeSim = ProxySim(realSim);

            location.SimulatorBlock.IP = (uint)fakeSim.Address.Address;
            location.SimulatorBlock.Port = (ushort)fakeSim.Port;

            return location;
		}

		// LogPacket: log a packet dump
        private Packet LogPacket(Packet packet, string type)
        {
            Log(type + " packet:", true);
			Log(packet, true);
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
	}

	// XmlRpcRequestDelegate: specifies a delegate to be called for XML-RPC requests
	public delegate void XmlRpcRequestDelegate(XmlRpcRequest request);

	// XmlRpcResponseDelegate: specifies a delegate to be called for XML-RPC responses
	public delegate void XmlRpcResponseDelegate(XmlRpcResponse response);

	// PacketDelegate: specifies a delegate to be called when a packet passes through the proxy
	public delegate Packet PacketDelegate(Packet packet, IPEndPoint endPoint);

	// Direction: specifies whether a packet is going to the client (Incoming) or to a sim (Outgoing)
	public enum Direction {
		Incoming,
		Outgoing
	}
}
