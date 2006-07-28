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
		// verbose: whether or not to print informative messages and warnings
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

		// Start: begin accepting clients
		public void Start() {
			RunSimProxy();
			(new Thread(new ThreadStart(RunLoginProxy))).Start();

			IPEndPoint endPoint = (IPEndPoint)loginServer.LocalEndPoint;
			IPAddress displayAddress;
			if (endPoint.Address == IPAddress.Any)
				displayAddress = IPAddress.Loopback;
			else
				displayAddress = endPoint.Address;
			Log("proxy ready at http://" + displayAddress + ":" + endPoint.Port + "/");
		}

		// SetLoginRequestDelegate: specify a callback loginRequestDelegate that will be called when the client requests login
		public void SetLoginRequestDelegate(XmlRpcRequestDelegate loginRequestDelegate) {
			this.loginRequestDelegate = loginRequestDelegate;
		}

		// SetLoginResponseDelegate: specify a callback loginResponseDelegate that will be called when the server responds to login
		public void SetLoginResponseDelegate(XmlRpcResponseDelegate loginResponseDelegate) {
			this.loginResponseDelegate = loginResponseDelegate;
		}

		// AddDelegate: add callback packetDelegate for packets of type packetName going direction
		public void AddDelegate(string packetName, Direction direction, PacketDelegate packetDelegate) {
			Hashtable table = direction == Direction.Incoming ? incomingDelegates : outgoingDelegates;
			lock(table)
				table[packetName] = packetDelegate;
		}

		// RemoveDelegate: remove callback for packets of type packetName going direction
		public void RemoveDelegate(string packetName, Direction direction) {
			Hashtable table = direction == Direction.Incoming ? incomingDelegates : outgoingDelegates;
			lock(table)
				table.Remove(packetName);
		}

		// InjectPacket: send packet to the client or server when direction is Incoming or Outgoing, respectively
		public void InjectPacket(Packet packet, Direction direction) {
			if (activeCircuit == null) {
				// no active circuit; queue the packet for injection once we have one
				ArrayList queue = direction == Direction.Incoming ? queuedIncomingInjections : queuedOutgoingInjections;
				queue.Add(packet);
			} else {
				// tell the active sim proxy to inject the packet
				lock(activeCircuit) {
					SimProxy sim;
					lock(simProxies)
						sim = (SimProxy)simProxies[activeCircuit];

					sim.Inject(packet, direction);
				}
			}
		}

		// Log: write message to the console if in verbose mode
		private void Log(object message) {
			if (proxyConfig.verbose)
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

				Log("handling login request from " + clientEndPoint);

				NetworkStream networkStream = new NetworkStream(client);
				StreamReader networkReader = new StreamReader(networkStream);
				StreamWriter networkWriter = new StreamWriter(networkStream);

				try {
					ProxyLogin(networkReader, networkWriter);
				} catch (Exception e) {
					Log("login failed: " + e.Message);
				}

				networkWriter.Close();
				networkReader.Close();
				networkStream.Close();

				client.Close();

				// send any packets queued for injection
				if (activeCircuit != null) {
					SimProxy activeProxy = (SimProxy)simProxies[activeCircuit];
					lock(queuedOutgoingInjections) {
						foreach (Packet packet in queuedOutgoingInjections)
							activeProxy.Inject(packet, Direction.Outgoing);
						queuedOutgoingInjections = new ArrayList();
					}
				}
			}
		    } catch (Exception e) {
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
		    }
		}

		// ProxyLogin: proxy a login request
		private void ProxyLogin(StreamReader reader, StreamWriter writer) {
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
			XmlRpcRequest request = (XmlRpcRequest)XmlRpcRequestDeserializer.Singleton.Deserialize(new String(content));

			// call the loginRequestDelegate
			if (loginRequestDelegate != null)
				try {
					loginRequestDelegate(request);
				} catch (Exception e) {
					Log("exception in login request deligate: " + e.Message);
					Log(e.StackTrace);
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
					Log("exception in login response delegate: " + e.Message);
					Log(e.StackTrace);
				}
			}

			// forward the XML-RPC response to the client
			XmlTextWriter responseWriter = new XmlTextWriter(writer);
			XmlRpcResponseSerializer.Singleton.Serialize(responseWriter, response);
			responseWriter.Close();
		}

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

		// initialize the sim proxy
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

		// start listening for packets from remote sims
		private void RunSimProxy() {
			simFacingSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref remoteEndPoint, new AsyncCallback(ReceiveFromSim), null);
		}

		// packet received from a remote sim
		private void ReceiveFromSim(IAsyncResult ar) {
		    try {
			// pause listening and get the length of the packet
			int length;
			lock(simFacingSocket)
				length = simFacingSocket.EndReceiveFrom(ar, ref remoteEndPoint);

			lock(proxyHandlers)
				if (proxyHandlers.Contains(remoteEndPoint)) {
					// find the proxy responsible for forwarding this packet
					SimProxy simProxy = (SimProxy)proxyHandlers[remoteEndPoint];

					// interpret the packet according to the SL protocol
					Packet packet;
					if ((receiveBuffer[0] & Helpers.MSG_ZEROCODED) == 0)
						packet = new Packet(receiveBuffer, length, proxyConfig.protocol);
					else
						lock(zeroBuffer) {
							int zeroLength = Helpers.ZeroDecode(receiveBuffer, length, zeroBuffer);
							packet = new Packet(zeroBuffer, zeroLength, proxyConfig.protocol);
						}

					// check for ACKs we're waiting for
					packet = simProxy.CheckAcks(packet, Direction.Incoming);

					// modify sequence numbers to account for injections
					packet = simProxy.ModifySequence(packet, Direction.Incoming);

					// check the packet for addresses that need proxying
					if (incomingCheckers.Contains(packet.Layout.Name)) {
						Packet newPacket = ((AddressChecker)incomingCheckers[packet.Layout.Name])(packet);
						SwapPacket(packet, newPacket);
						packet = newPacket;
					}

					// pass the packet to any callback delegates
					lock(incomingDelegates)
						if (incomingDelegates.Contains(packet.Layout.Name)) {
							try {
								Packet newPacket = ((PacketDelegate)incomingDelegates[packet.Layout.Name])(packet, (IPEndPoint)remoteEndPoint);
								if (newPacket == null) {
									if ((packet.Data[0] & Helpers.MSG_RELIABLE) != 0)
										simProxy.Inject(SpoofAck(packet), Direction.Outgoing);
	
									if ((packet.Data[0] & Helpers.MSG_APPENDED_ACKS) != 0)
										packet = SeparateAck(packet);
									else
										packet = null;
								} else {
									bool oldReliable = (packet.Data[0] & Helpers.MSG_RELIABLE) != 0;
									bool newReliable = (newPacket.Data[0] & Helpers.MSG_RELIABLE) != 0;
									if (oldReliable && !newReliable)
										SendPacket(SpoofAck(packet), (IPEndPoint)remoteEndPoint);
									else if (!oldReliable && newReliable)
										simProxy.WaitForAck(packet, Direction.Incoming);

									SwapPacket(packet, newPacket);
									packet = newPacket;
								}
							} catch (Exception e) {
								Log("exception in incoming delegate: " + e.Message);
								Log(e.StackTrace);
							}
						}

					if (packet != null)
						// forward the packet to the client via the appropriate fake sim endpoint
						simProxy.HandlePacket(packet);
				} else
					// ignore packets from unknown peers
					Log("dropping packet from " + remoteEndPoint);

			// resume listening
			lock(simFacingSocket)
				simFacingSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref remoteEndPoint, new AsyncCallback(ReceiveFromSim), null);
		    } catch (Exception e) {
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
		    }
		}

		// HandlePacket: forward a packet to a sim from our fake client endpoint
		private void HandlePacket(Packet packet, IPEndPoint endPoint, SimProxy proxy) {
			// check the packet for addresses that need proxying
			if (outgoingCheckers.Contains(packet.Layout.Name)) {
				Packet newPacket = ((AddressChecker)outgoingCheckers[packet.Layout.Name])(packet);
				SwapPacket(packet, newPacket);
				packet = newPacket;
			}

			// pass the packet to any callback delegates
			lock(outgoingDelegates)
				if (outgoingDelegates.Contains(packet.Layout.Name)) {
					try {
						Packet newPacket = ((PacketDelegate)outgoingDelegates[packet.Layout.Name])(packet, endPoint);
						if (newPacket == null) {
							if ((packet.Data[0] & Helpers.MSG_RELIABLE) != 0)
								proxy.Inject(SpoofAck(packet), Direction.Incoming);
	
							if ((packet.Data[0] & Helpers.MSG_APPENDED_ACKS) != 0)
								packet = SeparateAck(packet);
							else
								packet = null;
						} else {
							bool oldReliable = (packet.Data[0] & Helpers.MSG_RELIABLE) != 0;
							bool newReliable = (newPacket.Data[0] & Helpers.MSG_RELIABLE) != 0;
							if (oldReliable && !newReliable)
								proxy.SendPacket(SpoofAck(packet));
							else if (!oldReliable && newReliable)
								proxy.WaitForAck(packet, Direction.Outgoing);

							SwapPacket(packet, newPacket);
							packet = newPacket;
						}
					} catch (Exception e) {
						Log("exception in outgoing delegate: " + e.Message);
						Log(e.StackTrace);
					}
				}

			if (packet != null)
				// send the packet
				SendPacket(packet, endPoint);
		}

		// SendPacket: send a packet to a sim from our fake client endpoint
		public void SendPacket(Packet packet, IPEndPoint endPoint) {
			lock(simFacingSocket)
				if ((packet.Data[0] & Helpers.MSG_ZEROCODED) == 0)
					simFacingSocket.SendTo(packet.Data, packet.Data.Length, SocketFlags.None, endPoint);
				else
					lock(zeroBuffer) {
						int zeroLength = Helpers.ZeroEncode(packet.Data, packet.Data.Length, zeroBuffer);
						simFacingSocket.SendTo(zeroBuffer, zeroLength, SocketFlags.None, endPoint);
					}
		}

		// SpoofAck: create an ACK for the given packet
		private Packet SpoofAck(Packet packet) {
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();
			fields["ID"] = (uint)packet.Sequence;
			blocks[fields] = "Packets";
			return PacketBuilder.BuildPacket("PacketAck", proxyConfig.protocol, blocks, Helpers.MSG_ZEROCODED);
		}

		// SeparateAck: create a standalone PacketAck for packet's appended ACKs
		private Packet SeparateAck(Packet packet) {
			int ackCount = ((packet.Data[0] & Helpers.MSG_APPENDED_ACKS) == 0 ? 0 : (int)packet.Data[packet.Data.Length - 1]);
			Hashtable blocks = new Hashtable();
			for (int i = 0; i < ackCount; ++i) {
				Hashtable fields = new Hashtable();
				int offset = packet.Data.Length - (ackCount - i) * 4 - 1;
				fields["ID"] = (int)
					  (packet.Data[offset++] <<  0)
					+ (packet.Data[offset++] <<  8)
					+ (packet.Data[offset++] << 16)
					+ (packet.Data[offset++] << 24)
					;
				blocks[fields] = "Packets";
			}

			Packet ack = PacketBuilder.BuildPacket("PacketAck", proxyConfig.protocol, blocks, Helpers.MSG_ZEROCODED);
			ack.Sequence = packet.Sequence;
			return ack;
		}

		// SwapPacket: copy the sequence number and appended ACKs from one packet to another
		private static void SwapPacket(Packet oldPacket, Packet newPacket) {
			newPacket.Sequence = oldPacket.Sequence;

			int oldAcks = (oldPacket.Data[0] & Helpers.MSG_APPENDED_ACKS) == 0 ? 0 : (int)oldPacket.Data[oldPacket.Data.Length - 1];
			int newAcks = (newPacket.Data[0] & Helpers.MSG_APPENDED_ACKS) == 0 ? 0 : (int)newPacket.Data[newPacket.Data.Length - 1];

			if (oldAcks != 0 || newAcks != 0) {
				int oldAckSize = oldAcks == 0 ? 0 : oldAcks * 4 + 1;
				int newAckSize = newAcks == 0 ? 0 : newAcks * 4 + 1;

				byte[] newData = new byte[newPacket.Data.Length - newAckSize + oldAckSize];
				Array.Copy(newPacket.Data, 0, newData, 0, newPacket.Data.Length - newAckSize);

				if (newAcks != 0)
					newData[0] ^= Helpers.MSG_APPENDED_ACKS;

				if (oldAcks != 0) {
					newData[0] |= Helpers.MSG_APPENDED_ACKS;
					Array.Copy(oldPacket.Data, oldPacket.Data.Length - oldAckSize, newData, newPacket.Data.Length - newAckSize, oldAckSize);
				}

				newPacket.Data = newData;
			}
		}

		// ProxySim: return the proxy for the specified sim, creating it if it doesn't exist
		private IPEndPoint ProxySim(IPEndPoint simEndPoint) {
			lock(proxyEndPoints)
				if (proxyEndPoints.Contains(simEndPoint))
					// return the existing proxy
					return (IPEndPoint)proxyEndPoints[simEndPoint];
				else {
					// return a new proxy
					SimProxy simProxy = new SimProxy(proxyConfig, simEndPoint, this);
					IPEndPoint fakeSim = simProxy.LocalEndPoint();
					Log("creating proxy for " + simEndPoint + " at " + fakeSim);
					simProxy.Run();
					proxyEndPoints.Add(simEndPoint, fakeSim);
					simProxies.Add(simEndPoint, simProxy);
					return fakeSim;
				}
		}

		// AddHandler: remember which sim proxy corresponds to a given sim
		private void AddHandler(EndPoint endPoint, SimProxy proxy) {
			lock(proxyHandlers)
				proxyHandlers.Add(endPoint, proxy);
		}

		// SimProxy: proxy for a single simulator
		private class SimProxy {
			private ProxyConfig proxyConfig;
			private IPEndPoint remoteEndPoint;
			private Proxy proxy;
			private Socket socket;
			private object sequenceLock = new Object();
			private ushort incomingSequence;
			private ushort outgoingSequence;
			private ArrayList incomingInjections;
			private ArrayList outgoingInjections;
			private Hashtable incomingAcks;
			private Hashtable outgoingAcks;

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
				lock(sequenceLock) {
					incomingSequence = 0;
					outgoingSequence = 0;
					incomingInjections = new ArrayList();
					outgoingInjections = new ArrayList();
					incomingAcks = new Hashtable();
					outgoingAcks = new Hashtable();
				}
			}

			// ResendPackets: resend packets that haven't been ACKed
			private void ResendPackets() {
			    try {
				for (;;Thread.Sleep(1000))
					lock(sequenceLock) {
						foreach (Packet packet in incomingAcks.Values) {
							packet.Data[0] |= Helpers.MSG_RESENT;
							SendPacket(packet);
						}

						foreach (Packet packet in outgoingAcks.Values) {
							packet.Data[0] |= Helpers.MSG_RESENT;
							proxy.SendPacket(packet, remoteEndPoint);
						}
					}
			    } catch (Exception e) {
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			    }
			}

			// return the endpoint that the client should communicate with
			public IPEndPoint LocalEndPoint() {
				lock(socket)
					return (IPEndPoint)socket.LocalEndPoint;
			}

			private byte[] receiveBuffer = new byte[8192];
			private byte[] zeroBuffer = new byte[8192];
			private EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
			bool firstReceive = true;

			// Run: forward packets from the client to the sim
			public void Run() {
				(new Thread(new ThreadStart(ResendPackets))).Start();
				socket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref clientEndPoint, new AsyncCallback(ReceiveFromClient), null);
			}

			// ReceiveFromClient: packet received from the client
			private void ReceiveFromClient(IAsyncResult ar) {
			    try {
				// pause listening and fetch the packet
				int length;
				lock(clientEndPoint)
					lock(socket)
						length = socket.EndReceiveFrom(ar, ref clientEndPoint);
				Packet packet;
				if ((receiveBuffer[0] & Helpers.MSG_ZEROCODED) == 0)
					packet = new Packet(receiveBuffer, length, proxyConfig.protocol);
				else
					lock(zeroBuffer) {
						int zeroLength = Helpers.ZeroDecode(receiveBuffer, length, zeroBuffer);
						packet = new Packet(zeroBuffer, zeroLength, proxyConfig.protocol);
					}

				// keep track of sequence numbers
				lock(sequenceLock)
					if (packet.Sequence > incomingSequence)
						incomingSequence = packet.Sequence;

				// look for ACKs we're waiting for
				packet = CheckAcks(packet, Direction.Outgoing);
				
				// modify sequence numbers to account for injections
				packet = ModifySequence(packet, Direction.Outgoing);

				// forward packet via our fake client endpoint
				proxy.HandlePacket(packet, remoteEndPoint, this);

				// send any packets queued for injection
				if (firstReceive) {
					firstReceive = false;
					lock(proxy.queuedIncomingInjections) {
						foreach (Packet queuedPacket in proxy.queuedIncomingInjections)
							Inject(queuedPacket, Direction.Incoming);
						proxy.queuedIncomingInjections = new ArrayList();
					}
				}

				// resume listening
				lock(clientEndPoint)
					lock(socket)
						socket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref clientEndPoint, new AsyncCallback(ReceiveFromClient), null);
			    } catch (Exception e) {
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			    }
			}

			// HandlePacket: forward a packet from the sim to the client via our fake sim endpoint
			public void HandlePacket(Packet packet) {
				// keep track of sequence numbers
				lock(sequenceLock)
					if (packet.Sequence > outgoingSequence)
						outgoingSequence = packet.Sequence;

				// send the packet
				SendPacket(packet);
			}

			// SendPacket: send a packet from the sim to the client via our fake sim endpoint
			public void SendPacket(Packet packet) {
				lock(clientEndPoint)
					lock(socket)
						if ((packet.Data[0] & Helpers.MSG_ZEROCODED) == 0)
							socket.SendTo(packet.Data, packet.Data.Length, SocketFlags.None, clientEndPoint);
						else
							lock(zeroBuffer) {
								int zeroLength = Helpers.ZeroEncode(packet.Data, packet.Data.Length, zeroBuffer);
								socket.SendTo(zeroBuffer, zeroLength, SocketFlags.None, clientEndPoint);
							}
			}

			// Inject: inject a packet
			public void Inject(Packet packet, Direction direction) {
				lock(sequenceLock) {
					if (direction == Direction.Incoming) {
						if (firstReceive) {
							lock(proxy.queuedIncomingInjections)
								proxy.queuedIncomingInjections.Add(packet);
							return;
						}

						incomingInjections.Add(++incomingSequence);
						packet.Sequence = incomingSequence;
					} else {
						outgoingInjections.Add(++outgoingSequence);
						packet.Sequence = outgoingSequence;
					}
				}

				if ((packet.Data[0] & Helpers.MSG_RELIABLE) != 0)
					WaitForAck(packet, direction);

				if (direction == Direction.Incoming) {
					lock (clientEndPoint)
						lock (socket)
							if ((packet.Data[0] & Helpers.MSG_ZEROCODED) == 0)
								socket.SendTo(packet.Data, packet.Data.Length, SocketFlags.None, clientEndPoint);
							else
								lock(zeroBuffer) {
									int zeroLength = Helpers.ZeroEncode(packet.Data, packet.Data.Length, zeroBuffer);
									socket.SendTo(zeroBuffer, zeroLength, SocketFlags.None, clientEndPoint);
								}
				} else
					proxy.SendPacket(packet, remoteEndPoint);
			}

			// WaitForAck: take care of resending a packet until it's ACKed
			public void WaitForAck(Packet packet, Direction direction) {
				lock(sequenceLock) {
					Hashtable table = direction == Direction.Incoming ? incomingAcks : outgoingAcks;
					table.Add(packet.Sequence, packet);
				}
			}

			// CheckAcks: check for and remove ACKs of packets we've injected
			public Packet CheckAcks(Packet packet, Direction direction) {
				lock(sequenceLock) {
					Hashtable acks = direction == Direction.Incoming ? outgoingAcks : incomingAcks;

					if (acks.Count == 0)
						return packet;

					// check for embedded ACKs
					if (packet.Layout.Name == "PacketAck") {
						bool changed = false;
						Hashtable blocks = PacketUtility.Unbuild(packet);
						Hashtable newBlocks = new Hashtable();
						foreach (Hashtable fields in blocks.Keys) {
							ushort id = (ushort)((uint)fields["ID"]);
							if (acks.Contains(id)) {
								acks.Remove(id); // FIXME: we could see this packet again
								changed = true;
							} else
								newBlocks.Add(fields, blocks[fields]);
						}
						if (changed) {
							Packet newPacket = PacketBuilder.BuildPacket("PacketAck", proxyConfig.protocol, newBlocks, packet.Data[0]);
							SwapPacket(packet, newPacket);
							packet = newPacket;
						}
					}

					// check for appended ACKs
					if ((packet.Data[0] & Helpers.MSG_APPENDED_ACKS) != 0) {
						byte ackCount = packet.Data[packet.Data.Length - 1];
						for (int i = 0; i < ackCount;) {
							int offset = packet.Data.Length - (ackCount - i) * 4 - 1;
							ushort ackID = (ushort)(packet.Data[offset] + (packet.Data[offset + 1] << 8));
							if (acks.Contains(ackID)) {
								byte[] newData = new byte[packet.Data.Length - 4];
								Array.Copy(packet.Data, 0, newData, 0, offset);
								Array.Copy(packet.Data, offset + 4, newData, offset, packet.Data.Length - offset - 4);
								--newData[newData.Length - 1];
								packet.Data = newData;
								--ackCount;
								acks.Remove(ackID); // FIXME: we could see this packet again
							} else
								++i;
						}
						if (ackCount == 0) {
							byte[] newData = new byte[packet.Data.Length - 1];
							Array.Copy(packet.Data, 0, newData, 0, packet.Data.Length - 1);
							newData[0] ^= Helpers.MSG_APPENDED_ACKS;
							packet.Data = newData;
						}
					}
				}

				return packet;
			}

			// ModifySequence: modify a packet's sequence number and ACK IDs to account for injections
			public Packet ModifySequence(Packet packet, Direction direction) {
				// TODO: after a period of time, roll injections into a base offset to avoid unbounded memory consumption.

				lock(sequenceLock) {
					ArrayList ourInjections = direction == Direction.Outgoing ? outgoingInjections : incomingInjections;
					ArrayList theirInjections = direction == Direction.Incoming ? outgoingInjections : incomingInjections;

					if (ourInjections.Count != 0) {
						ushort newSequence = packet.Sequence;
						foreach (ushort injection in ourInjections)
							if (newSequence >= injection)
								++newSequence;
						packet.Sequence = newSequence;
					}

					if (theirInjections.Count != 0) {
						if ((packet.Data[0] & Helpers.MSG_APPENDED_ACKS) != 0) {
							int ackCount = packet.Data[packet.Data.Length - 1];
							for (int i = 0; i < ackCount; ++i) {
								int offset = packet.Data.Length - (ackCount - i) * 4 - 2;
								uint ackID = (uint)(packet.Data[offset] + (packet.Data[offset + 1] << 8));
								for (int j = theirInjections.Count - 1; j >= 0; --j)
									if (ackID >= (ushort)theirInjections[j])
										--ackID;
								packet.Data[offset + 0] = (byte)(ackID % 256); ackID >>= 8;
								packet.Data[offset + 1] = (byte)(ackID % 256); ackID >>= 8;
								packet.Data[offset + 2] = (byte)(ackID % 256); ackID >>= 8;
								packet.Data[offset + 3] = (byte)(ackID % 256); ackID >>= 8;
							}
						}

						if (packet.Layout.Name == "PacketAck") {
							Hashtable blocks = PacketUtility.Unbuild(packet);
							foreach (Hashtable fields in blocks.Keys) {
								if ((string)blocks[fields] == "Packets") {
									uint ackID = (uint)fields["ID"];
									for (int i = theirInjections.Count - 1; i >= 0; --i)
										if (ackID >= (ushort)theirInjections[i])
											--ackID;
									fields["ID"] = ackID;
								}
							}
							Packet newPacket = PacketBuilder.BuildPacket("PacketAck", proxyConfig.protocol, blocks, packet.Data[0]);
							SwapPacket(packet, newPacket);
							packet = newPacket;
						}
					}
				}

				return packet;
			}
		}

		delegate Packet AddressChecker(Packet packet);

		Hashtable incomingCheckers = new Hashtable();
		Hashtable outgoingCheckers = new Hashtable();

		// InitializeAddressCheckers: initialize delegates that check packets for addresses that need proxying
		private void InitializeAddressCheckers() {
			// TODO: Packets that I've never seen that appear to
			// require checking are considered unhandled; these
			// should be checked in a stable release.  Packets that
			// I've never seen that contain an IP and don't appear
			// to require checking are considered mystery; these
			// should be ignored in a stable release.  Packets that
			// are checked are considered unexpected if they come
			// in the wrong direction; these should be ignored in a
			// stable release.
			AddChecker("SimulatorAssign", Direction.Incoming, new AddressChecker(LogUnhandledPacket));
			AddChecker("SimulatorStart", Direction.Incoming, new AddressChecker(LogUnhandledPacket));
			AddChecker("SimulatorPresentAtLocation", Direction.Incoming, new AddressChecker(LogUnhandledPacket));
			AddChecker("RegionPresenceResponse", Direction.Incoming, new AddressChecker(LogUnhandledPacket));
			AddChecker("AgentPresenceResponse", Direction.Incoming, new AddressChecker(LogUnhandledPacket));
			AddMystery("TrackAgentSession");
			AddMystery("ClearAgentSessions");
			AddChecker("LogFailedMoneyTransaction", Direction.Incoming, new AddressChecker(LogUnhandledPacket));
			AddMystery("DirFindQueryBackend");
			AddMystery("DirPeopleQueryBackend");
			AddMystery("OnlineStatusRequest");
			AddChecker("SpaceLocationTeleportReply", Direction.Incoming, new AddressChecker(LogUnhandledPacket));
			AddChecker("TeleportFinish", Direction.Incoming, new AddressChecker(CheckTeleportFinish));
			AddMystery("AddModifyAbility");
			AddMystery("RemoveModifyAbility");
			//AddMystery("ViewerStats"); IP is 0.0.0.0
			AddChecker("EnableSimulator", Direction.Incoming, new AddressChecker(CheckEnableSimulator));
			//AddMystery("KickUser"); IP is 0.0.0.0
			AddMystery("LogLogin");
			AddMystery("DataServerLogout");
			AddMystery("RequestLocationGetAccess");
			AddMystery("RequestLocationGetAccessReply");
			AddMystery("FindAgent");
			AddMystery("RoutedMoneyBalanceReply");
			AddChecker("UserLoginLocationReply", Direction.Incoming, new AddressChecker(LogUnhandledPacket));
			AddChecker("SpaceLoginLocationReply", Direction.Incoming, new AddressChecker(LogUnhandledPacket));
			AddMystery("RemoveMemeberFromGroup");
			AddMystery("RpcScriptRequestInboundForward");
			AddMystery("MailPingBounce");
			AddMystery("OpenCircuit");
			AddChecker("ClosestSimulator", Direction.Incoming, new AddressChecker(LogUnhandledPacket));
			AddChecker("CrossedRegion", Direction.Incoming, new AddressChecker(CheckCrossedRegion));
			AddChecker("NeighborList", Direction.Incoming, new AddressChecker(LogUnhandledPacket));
			AddChecker("AgentToNewRegion", Direction.Incoming, new AddressChecker(LogUnhandledPacket));
		}

		// AddChecker: add a checker delegate
		private void AddChecker(String name, Direction direction, AddressChecker checker) {
			(direction == Direction.Incoming ? incomingCheckers : outgoingCheckers).Add(name, checker);
			(direction == Direction.Incoming ? outgoingCheckers : incomingCheckers).Add(name, new AddressChecker(LogUnexpectedPacket));
		}

		// AddMystery: add a checker delegate that logs packets we're watching for development purposes
		private void AddMystery(String name) {
			incomingCheckers.Add(name, new AddressChecker(LogIncomingMysteryPacket));
			outgoingCheckers.Add(name, new AddressChecker(LogOutgoingMysteryPacket));
		}

		// GenericCheck: replace the sim address in a packet with our proxy address
		private Packet GenericCheck(Packet packet, string block, string fieldIP, string fieldPort, bool active) {
			Hashtable blocks = PacketUtility.Unbuild(packet);

			IPEndPoint realSim = new IPEndPoint((IPAddress)PacketUtility.GetField(blocks, block, fieldIP), Convert.ToInt32(PacketUtility.GetField(blocks, block, fieldPort)));
			IPEndPoint fakeSim = ProxySim(realSim);
			PacketUtility.SetField(blocks, block, fieldIP, fakeSim.Address);
			PacketUtility.SetField(blocks, block, fieldPort, (ushort)fakeSim.Port);

			if (active)
				activeCircuit = realSim;

			return PacketBuilder.BuildPacket(packet.Layout.Name, proxyConfig.protocol, blocks, packet.Data[0]);
		}

		// CheckTeleportFinish: check TeleportFinish packets
		private Packet CheckTeleportFinish(Packet packet) {
			return GenericCheck(packet, "Info", "SimIP", "SimPort", true);
		}

		// CheckEnableSimulator: check EnableSimulator packets
		private Packet CheckEnableSimulator(Packet packet) {
			return GenericCheck(packet, "SimulatorInfo", "IP", "Port", false);
		}

		// CheckCrossedregion: check CrossedRegion packets
		private Packet CheckCrossedRegion(Packet packet) {
			return GenericCheck(packet, "RegionData", "SimIP", "SimPort", true);
		}

		// LogPacket: log a packet dump
		private Packet LogPacket(Packet packet, string type) {
			Log(type + " packet:");
			Log(packet);

			return packet;
		}

		// LogUnhandledPacket: log a packet that probably ought to have been checked
		private Packet LogUnhandledPacket(Packet packet) {
			return LogPacket(packet, "unhandled");
		}

		// LogUnexpectedPacket: log a packet that we expected to be going the opposite direction
		private Packet LogUnexpectedPacket(Packet packet) {
			return LogPacket(packet, "unexpected");
		}

		// LogIncomingMysteryPacket: log an incoming packet we're watching for development purposes
		private Packet LogIncomingMysteryPacket(Packet packet) {
			return LogPacket(packet, "incoming mystery");
		}

		// LogOutgoingMysteryPacket: log an outgoing packet we're watching for development purposes
		private Packet LogOutgoingMysteryPacket(Packet packet) {
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

	// PacketUtility: provides various utility methods for working with libsecondlife Packet objects
	public class PacketUtility {
		// Unbuild: deconstruct a packet into a Hashtable of blocks suitable for passing to PacketBuilder
		public static Hashtable Unbuild(Packet packet) {
			Hashtable blockTable = new Hashtable();
			foreach (Block block in packet.Blocks()) {
				Hashtable fieldTable = new Hashtable();
				foreach (Field field in block.Fields)
					fieldTable[field.Layout.Name] = field.Data;
				blockTable[fieldTable] = block.Layout.Name;
			}

			return blockTable;
		}

		// GetField: given a table of blocks, return the value of the specified block and field
		// In the case of packets with variable blocks, an arbitrary block will be used.
		public static object GetField(Hashtable blocks, string block, string field) {
			foreach (Hashtable fields in blocks.Keys)
				if ((string)blocks[fields] == block)
					if (fields.Contains(field))
						return fields[field];

			return null;
		}

		// SetField: given a table of blocks, update the value of the specified block and field
		// In the case of packets with variable blocks, all blocks will be updated.
		public static void SetField(Hashtable blocks, string block, string field, object value) {
			foreach (Hashtable fields in blocks.Keys)
				if ((string)blocks[fields] == block)
					if (fields.Contains(field))
						fields[field] = value;
		}

		// VariableToString: convert a variable field to a string
		// Returns an empty string if the field can't be decoded as UTF-8
		public static string VariableToString(byte[] field) {
			try {
				byte[] withoutNull = new byte[field.Length - 1];
				Array.Copy(field, 0, withoutNull, 0, field.Length - 1);
				return System.Text.Encoding.UTF8.GetString(withoutNull);
			} catch {
				return "";
			}
		}

		// StringtoVariable: convert a string to a variable field
		// Returns an empty field if the string can't be encoded as UTF-8
		public static byte[] StringToVariable(string str) {
			try {
				byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
				byte[] withNull = new byte[bytes.Length + 1];
				Array.Copy(bytes, 0, withNull, 0, bytes.Length);
				withNull[withNull.Length - 1] = 0;
				return withNull;
			} catch {
				byte[] empty = new byte[1];
				empty[0] = 0;
				return empty;
			}
		}
	}
}
