/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
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

using System;
using System.Text;
using System.Timers;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;
using Nwc.XmlRpc;

namespace libsecondlife
{
	public delegate void PacketCallback(Packet packet, Circuit circuit);

	internal class AcceptAllCertificatePolicy : ICertificatePolicy
	{
		public AcceptAllCertificatePolicy()
		{
		}

		public bool CheckValidationResult(ServicePoint sPoint, 
			System.Security.Cryptography.X509Certificates.X509Certificate cert, 
			WebRequest wRequest,int certProb)
		{
			// Always accept
			return true;
		}
	}

	public class Circuit
	{
		public uint CircuitCode;
		public bool Opened;
		public ushort Sequence;
		public IPEndPoint ipEndPoint;

		private EndPoint endPoint;
		private ProtocolManager Protocol;
		private NetworkManager Network;
		private byte[] Buffer;
		private Socket Connection;
		private AsyncCallback ReceivedData;
		private System.Timers.Timer OpenTimer;
		private System.Timers.Timer ACKTimer;
		private bool Timeout;
		private ArrayList AckOutbox;
		private Mutex AckOutboxMutex;
		private Hashtable NeedAck;
		private Mutex NeedAckMutex;
		private int ResendTick;

		public Circuit(ProtocolManager protocol, NetworkManager network, uint circuitCode)
		{
			Protocol = protocol;
			Network = network;
			CircuitCode = circuitCode;
			Sequence = 0;
			Buffer = new byte[4096];
			Connection = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			Opened = false;
			Timeout = false;

			// Initialize the queue of ACKs that need to be sent to the server
			AckOutbox = new ArrayList();

			// Initialize the hashtable for reliable packets waiting on ACKs from the server
			NeedAck = new Hashtable();

			// Create a timer to test if the connection times out
			OpenTimer = new System.Timers.Timer(10000);
			OpenTimer.Elapsed += new ElapsedEventHandler(OpenTimerEvent);

			// Create a timer to send PacketAcks and resend unACKed packets
			ACKTimer = new System.Timers.Timer(1000);
			ACKTimer.Elapsed += new ElapsedEventHandler(ACKTimerEvent);

			AckOutboxMutex = new Mutex(false, "AckOutboxMutex");
			NeedAckMutex = new Mutex(false, "NeedAckMutex");

			ResendTick = 0;
		}

		~Circuit()
		{
			Stop();
			Connection.Close();
		}

		public bool Open(string ip, int port)
		{
			try
			{
				// Setup the callback
				ReceivedData = new AsyncCallback(this.OnReceivedData);

				// Create an endpoint that we will be communicating with (need it in two types due to
				// .NET weirdness)
				ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
				endPoint = (EndPoint)ipEndPoint;

				// Associate this circuit's socket with the given ip and port and start listening
				Connection.Connect(endPoint);
				Connection.BeginReceiveFrom(Buffer, 0, Buffer.Length, SocketFlags.None, ref endPoint, ReceivedData, null);

				// Start the circuit opening timeout
				OpenTimer.Start();

				// Start the packet resend timer
				ACKTimer.Start();

				// Send the UseCircuitCode packet to initiate the connection
				Packet packet = PacketBuilder.UseCircuitCode(Protocol, Network.AgentID, 
					Network.SessionID, CircuitCode);

				// Send the initial packet out
				SendPacket(packet, true);

				while (!Timeout)
				{
					if (Opened)
					{
						return true;
					}

					Thread.Sleep(0);
				}
			}
			catch (Exception e)
			{
				Helpers.Log(e.ToString(), Helpers.LogLevel.Error);
			}

			return false;
		}

		public void Close()
		{
			try
			{
				Stop();

				// Send the CloseCircuit notice
				Packet packet = new Packet("CloseCircuit", Protocol, 8);
				SendPacket(packet, true);

				// Send any last ACKs before closing the circuit
				SendACKs();

				Connection.Close();
			}
			catch (Exception e)
			{
				Helpers.Log(e.ToString(), Helpers.LogLevel.Error);
			}
		}

		public void Stop()
		{
			try
			{
				// Stop the resend timer
				ACKTimer.Stop();

				// Stop the open circuit timer (just in case it's still running)
				OpenTimer.Stop();

				// TODO: Is this safe? Using the mutex throws an exception about a disposed object
				NeedAck.Clear();
			}
			catch (Exception e)
			{
				Helpers.Log(e.ToString(), Helpers.LogLevel.Error);
			}
		}

		public void SendPacket(Packet packet, bool incrementSequence)
		{
			byte[] zeroBuffer = new byte[4096];
			int zeroBytes;

			// DEBUG
			//Console.WriteLine("Sending " + packet.Data.Length + " byte " + packet.Layout.Name);

			try
			{
				if ((packet.Data[0] & Helpers.MSG_RELIABLE) != 0 && incrementSequence)
				{
					if (!NeedAck.ContainsKey(packet))
					{
						// This packet needs an ACK, keep track of when it was sent out
						NeedAckMutex.WaitOne();
						NeedAck.Add(packet, Environment.TickCount);
						NeedAckMutex.ReleaseMutex();
					}
				}

				if (incrementSequence)
				{
					// Set the sequence number here since we are manually serializing the packet
					packet.Sequence = ++Sequence;
				}

				// Zerocode if needed
				if ((packet.Data[0] & Helpers.MSG_ZEROCODED) != 0)
				{
					zeroBytes = Helpers.ZeroEncode(packet.Data, packet.Data.Length, zeroBuffer);
				}
				else
				{
					// Normal packet, copy it straight over to the zeroBuffer
					Array.Copy(packet.Data, 0, zeroBuffer, 0, packet.Data.Length);
					zeroBytes = packet.Data.Length;
				}

				// The incrementSequence check prevents a possible deadlock situation
				if (AckOutbox.Count != 0 && incrementSequence && packet.Layout.Name != "PacketAck" && 
					packet.Layout.Name != "LogoutRequest")
				{
					// Claim the mutex on the AckOutbox
					AckOutboxMutex.WaitOne();

					//TODO: Make sure we aren't appending more than 255 ACKs

					// Append each ACK needing to be sent out to this packet
					foreach (uint ack in AckOutbox)
					{
						Array.Copy(BitConverter.GetBytes(ack), 0, zeroBuffer, zeroBytes - 1, 4);
						zeroBytes += 4;
					}

					// Last byte is the number of ACKs
					zeroBuffer[zeroBytes - 1] = (byte)AckOutbox.Count;
					zeroBytes += 1;

					AckOutbox.Clear();

					// Release the mutex
					AckOutboxMutex.ReleaseMutex();

					// Set the flag that this packet has ACKs appended to it
					zeroBuffer[0] += Helpers.MSG_APPENDED_ACKS;
				}

				int numSent = Connection.Send(zeroBuffer, zeroBytes, SocketFlags.None);

				// DEBUG
				//Console.WriteLine("Sent " + numSent + " bytes");
			}
			catch (Exception e)
			{
				Helpers.Log(e.ToString(), Helpers.LogLevel.Error);
			}
		}

		private void SendACKs()
		{
			// Claim the mutex on the AckOutbox
			AckOutboxMutex.WaitOne();

			if (AckOutbox.Count != 0)
			{
				try
				{
					Packet packet = PacketBuilder.PacketAck(Protocol, AckOutbox);

					if (packet.Data.Length < 13)
					{
						Helpers.Log("Trying to send a PacketAck with no ACKs, cancelling", Helpers.LogLevel.Warning);
						// Release the mutex
						AckOutboxMutex.ReleaseMutex();

						return;
					}

					// Set the sequence number
					packet.Sequence = ++Sequence;

					// Bypass SendPacket since we are taking care of the AckOutbox ourself
					int numSent = Connection.Send(packet.Data);

					// DEBUG
					//Console.WriteLine("Sent " + numSent + " byte " + packet.Layout.Name);

					AckOutbox.Clear();
				}
				catch (Exception e)
				{
					Helpers.Log(e.ToString(), Helpers.LogLevel.Error);
				}
			}

			// Release the mutex
			AckOutboxMutex.ReleaseMutex();
		}

		private void OnReceivedData(IAsyncResult result)
		{
			Packet packet;

			try
			{
				// For the UseCircuitCode timeout
				Opened = true;
				OpenTimer.Stop();

				// Retrieve the incoming packet
				int numBytes = Connection.EndReceiveFrom(result, ref endPoint);

				if ((Buffer[Buffer.Length - 1] & Helpers.MSG_APPENDED_ACKS) != 0)
				{
					// Grab the ACKs that are appended to this packet
					byte numAcks = Buffer[Buffer.Length - 1];

					Helpers.Log("Found " + numAcks + " appended acks", Helpers.LogLevel.Info);

					// Claim the NeedAck mutex
					NeedAckMutex.WaitOne();

					for (int i = 1; i <= numAcks; ++i)
					{
						uint ack = BitConverter.ToUInt32(Buffer, numBytes - i * 4 - 1);

					Beginning:
						ICollection reliablePackets = NeedAck.Keys;

						// Remove this packet if it exists
						foreach (Packet reliablePacket in reliablePackets)
						{
							if ((uint)reliablePacket.Sequence == ack)
							{
								NeedAck.Remove(reliablePacket);
								goto Beginning;
							}
						}
					}

					// Release the mutex
					NeedAckMutex.ReleaseMutex();

					// Adjust the packet length
					numBytes = numBytes - numAcks * 4 - 1;
				}

				if ((Buffer[0] & Helpers.MSG_ZEROCODED) != 0)
				{
					// Allocate a temporary buffer for the zerodecoded packet
					byte[] zeroBuffer = new byte[4096];
					int zeroBytes = Helpers.ZeroDecode(Buffer, numBytes, zeroBuffer);
					packet = new Packet(zeroBuffer, zeroBytes, Protocol);
					numBytes = zeroBytes;
				}
				else
				{
					// Create the packet object from our byte array
					packet = new Packet(Buffer, numBytes, Protocol);
				}

				// DEBUG
				//Console.WriteLine("Received a " + numBytes + " byte " + packet.Layout.Name);

				// Start listening again since we're done with Buffer
				Connection.BeginReceiveFrom(Buffer, 0, Buffer.Length, SocketFlags.None, ref endPoint, ReceivedData, null);

				if ((packet.Data[0] & Helpers.MSG_RELIABLE) != 0)
				{
					if (!AckOutbox.Contains((uint)packet.Sequence))
					{
						// This packet needs to be ACKed, push its sequence number on to the queue
						AckOutboxMutex.WaitOne();
						AckOutbox.Add((uint)packet.Sequence);
						AckOutboxMutex.ReleaseMutex();
					}
					else
					{
						if ((packet.Data[0] & Helpers.MSG_RESENT) != 0)
						{
							// We received a resent packet
							Helpers.Log("Received a resent packet, sequence=" + packet.Sequence, Helpers.LogLevel.Warning);
							return;
						}
						else
						{
							// We received a resent packet
							Helpers.Log("Received a duplicate sequence number? sequence=" + packet.Sequence
								+ ", name=" + packet.Layout.Name, Helpers.LogLevel.Warning);
						}
					}
				}

				if (packet.Layout.Name == null)
				{
					Helpers.Log("Received an unrecognized packet", Helpers.LogLevel.Warning);
					return;
				}
				else if (packet.Layout.Name == "PacketAck")
				{
					// PacketAck is handled directly instead of using a callback to simplify access to 
					// the NeedAck hashtable and its mutex

					ArrayList blocks = packet.Blocks();

					NeedAckMutex.WaitOne();

					// Remove each ACK in this packet from the NeedAck waiting list
					foreach (Block block in blocks)
					{
						foreach (Field field in block.Fields)
						{
						Beginning:
							ICollection reliablePackets = NeedAck.Keys;

							// Remove this packet if it exists
							foreach (Packet reliablePacket in reliablePackets)
							{
								if ((uint)reliablePacket.Sequence == (uint)field.Data)
								{
									NeedAck.Remove(reliablePacket);
									// Restart the loop to avoid upsetting the enumerator
									goto Beginning;
								}
							}
						}
					}

					NeedAckMutex.ReleaseMutex();
				}
				
				// Fire any internal callbacks registered with this packet type
				PacketCallback callback = (PacketCallback)Network.InternalCallbacks[packet.Layout.Name];

				if (callback != null)
				{
					callback(packet, this);
				}

				// Fire any user callbacks registered with this packet type
				callback = (PacketCallback)Network.UserCallbacks[packet.Layout.Name];
				
				if (callback != null)
				{
					callback(packet, this);
				}
				else
				{
					// Attempt to fire a default user callback
					callback = (PacketCallback)Network.UserCallbacks["Default"];

					if (callback != null)
					{
						callback(packet, this);
					}
				}
			}
			catch (Exception e)
			{
				Helpers.Log(e.ToString(), Helpers.LogLevel.Error);
			}
		}

		private void OpenTimerEvent(object source, System.Timers.ElapsedEventArgs ea)
		{
			try
			{
				Timeout = true;
				OpenTimer.Stop();
			}
			catch (Exception e)
			{
				Helpers.Log(e.ToString(), Helpers.LogLevel.Error);
			}
		}

		private void ACKTimerEvent(object source, System.Timers.ElapsedEventArgs ea)
		{
			try
			{
				// Send any ACKs in the queue
				SendACKs();

				ResendTick++;

				if (ResendTick >= 3)
				{
					ResendTick = 0;

					// Claim the NeedAck mutex
					NeedAckMutex.WaitOne();

				Beginning:

					// Check if any reliable packets haven't been ACKed by the server
					IDictionaryEnumerator packetEnum = NeedAck.GetEnumerator();

					while (packetEnum.MoveNext())
					{
						int ticks = (int)packetEnum.Value;

						// TODO: Is this hardcoded value correct? Should it be a higher level define or a 
						//       changeable property?
						if (Environment.TickCount - ticks > 3000)
						{
							Packet packet = (Packet)packetEnum.Key;

							// Adjust the timeout value for this packet
							NeedAck[packet] = Environment.TickCount;
							
							// Add the resent flag
							packet.Data[0] += Helpers.MSG_RESENT;
							
							// Resend the packet
							SendPacket((Packet)packet, false);

							// Restart the loop since we modified a value and the iterator will fail
							goto Beginning;
						}
					}

					// Release the mutex
					NeedAckMutex.ReleaseMutex();
				}
			}
			catch (Exception e)
			{
				Helpers.Log(e.ToString(), Helpers.LogLevel.Error);
			}
		}
	}

	public class NetworkManager
	{
		public LLUUID AgentID;
		public LLUUID SessionID;
		public string LoginError;
		public Hashtable UserCallbacks;
		public Hashtable InternalCallbacks;
		public Circuit CurrentCircuit;

		private ProtocolManager Protocol;
		private string LoginBuffer;
		private ArrayList Circuits;

		public NetworkManager(ProtocolManager protocol)
		{
			Protocol = protocol;
			Circuits = new ArrayList();
			UserCallbacks = new Hashtable();
			InternalCallbacks = new Hashtable();
			CurrentCircuit = null;

			// Register the internal callbacks
			PacketCallback callback = new PacketCallback(RegionHandshakeHandler);
			InternalCallbacks["RegionHandshake"] = callback;
			callback = new PacketCallback(StartPingCheckHandler);
			InternalCallbacks["StartPingCheck"] = callback;
		}

		public void SendPacket(Packet packet)
		{
			if (CurrentCircuit != null)
			{
				CurrentCircuit.SendPacket(packet, true);
			}
			else
			{
				Helpers.Log("Trying to send a packet when there is no current circuit", Helpers.LogLevel.Error);
			}
		}

		public void SendPacket(Packet packet, Circuit circuit)
		{
			circuit.SendPacket(packet, true);
		}

		public static Hashtable DefaultLoginValues(string firstName, string lastName, string password, string mac,
			string startLocation, int major, int minor, int patch, int build, string platform, string viewerDigest, 
			string userAgent, string author)
		{
			Hashtable values = new Hashtable();

			// Generate an MD5 hash of the password
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(password));
			StringBuilder passwordDigest = new StringBuilder();
			// Convert the hash to a hex string
			foreach(byte b in hash)
			{
				passwordDigest.AppendFormat("{0:x2}", b);
			}

			values["first"] = firstName;
			values["last"] = lastName;
			values["passwd"] = "$1$" + passwordDigest;
			values["start"] = startLocation;
			values["major"] = major;
			values["minor"] = minor;
			values["patch"] = patch;
			values["build"] = build;
			values["platform"] = platform;
			values["mac"] = mac;
			values["viewer_digest"] = viewerDigest;
			values["user-agent"] = userAgent + " (" + Helpers.VERSION + ")";
			values["author"] = author;

			return values;
		}

		public bool Login(Hashtable loginParams, out Hashtable values)
		{
			XmlRpcResponse result;

			XmlRpcRequest xmlrpc = new XmlRpcRequest();
			xmlrpc.MethodName = "login_to_simulator";
			xmlrpc.Params.Clear();
			xmlrpc.Params.Add(loginParams);

			try
			{
				result = (XmlRpcResponse)xmlrpc.Send("https://login.agni.lindenlab.com/cgi-bin/login.cgi");
			}
			catch (Exception e)
			{
				Helpers.Log(e.ToString(), Helpers.LogLevel.Error);
				LoginError = e.Message;
				values = null;
				return false;
			}

			if (result.IsFault)
			{
				Helpers.Log("Fault " + result.FaultCode + ": " + result.FaultString, Helpers.LogLevel.Error);
				LoginError = "Fault " + result.FaultCode + ": " + result.FaultString;
				values = null;
				return false;
			}

			values = (Hashtable)result.Value;

			if ((string)values["login"] == "false")
			{
				LoginError = values["reason"] + ": " + values["message"];
				return false;
			}

			AgentID = new LLUUID((string)values["agent_id"]);
			SessionID = new LLUUID((string)values["session_id"]);
			uint circuitCode = (uint)(int)values["circuit_code"];

			/*LoginValues.SessionID = RpcGetString(LoginBuffer.ToString(), "<name>session_id</name>");
			LoginValues.SecureSessionID = RpcGetString(LoginBuffer.ToString(), "<name>secure_session_id</name>");
			LoginValues.StartLocation = RpcGetString(LoginBuffer.ToString(), "<name>start_location</name>");
			LoginValues.FirstName = RpcGetString(LoginBuffer.ToString(), "<name>first_name</name>");
			LoginValues.LastName = RpcGetString(LoginBuffer.ToString(), "<name>last_name</name>");
			LoginValues.RegionX = RpcGetInt(LoginBuffer.ToString(), "<name>region_x</name>");
			LoginValues.RegionY = RpcGetInt(LoginBuffer.ToString(), "<name>region_y</name>");
			LoginValues.Home = RpcGetString(LoginBuffer.ToString(), "<name>home</name>");
			LoginValues.Message = RpcGetString(LoginBuffer.ToString(), "<name>message</name>").Replace("\r\n", "");
			LoginValues.CircuitCode = (uint)RpcGetInt(LoginBuffer.ToString(), "<name>circuit_code</name>");
			LoginValues.Port = RpcGetInt(LoginBuffer.ToString(), "<name>sim_port</name>");
			LoginValues.IP = RpcGetString(LoginBuffer.ToString(), "<name>sim_ip</name>");
			LoginValues.LookAt = RpcGetString(LoginBuffer.ToString(), "<name>look_at</name>");
			LoginValues.AgentID = RpcGetString(LoginBuffer.ToString(), "<name>agent_id</name>");
			LoginValues.SecondsSinceEpoch = (uint)RpcGetInt(LoginBuffer.ToString(), "<name>seconds_since_epoch</name>");*/

			// Connect to the sim given in the login reply
			Circuit circuit = new Circuit(Protocol, this, circuitCode);
			if (!circuit.Open((string)values["sim_ip"], (int)values["sim_port"]))
			{
				return false;
			}

			// Circuit was successfully opened, add it to the list and set it as default
			Circuits.Add(circuit);
			CurrentCircuit = circuit;

			// Move our agent in to the sim to complete the connection
			Packet packet = PacketBuilder.CompleteAgentMovement(Protocol, AgentID, SessionID, circuitCode);
			SendPacket(packet);

			return true;
		}

		public bool Login(string firstName, string lastName, string password, string mac,
			int major, int minor, int patch, int build, string platform, string viewerDigest, 
			string userAgent, string author)
		{
			return Login(firstName, lastName, password, mac, major, minor, patch, build, platform,
				viewerDigest, userAgent, author, "https://login.agni.lindenlab.com/cgi-bin/login.cgi");
		}

		public bool Login(string firstName, string lastName, string password, string mac,
			int major, int minor, int patch, int build, string platform, string viewerDigest, 
			string userAgent, string author, string url)
		{
			WebRequest login;
			WebResponse response;
			
			// Generate an MD5 hash of the password
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(password));
			StringBuilder passwordDigest = new StringBuilder();
			// Convert the hash to a hex string
			foreach(byte b in hash)
			{
				passwordDigest.AppendFormat("{0:x2}", b);
			}

			string loginRequest = 
				"<?xml version=\"1.0\"?><methodCall><methodName>login_to_simulator</methodName>" +
				"<params><param><value><struct>" +
				"<member><name>first</name><value><string>" + firstName + "</string></value></member>" +
				"<member><name>last</name><value><string>" + lastName + "</string></value></member>" +
				"<member><name>passwd</name><value><string>$1$" + passwordDigest + "</string></value></member>" +
				"<member><name>start</name><value><string>last</string></value></member>" +
				"<member><name>major</name><value><string>" + major + "</string></value></member>" +
				"<member><name>minor</name><value><string>" + minor + "</string></value></member>" +
				"<member><name>patch</name><value><string>" + patch + "</string></value></member>" +
				"<member><name>build</name><value><string>" + build + "</string></value></member>" +
				"<member><name>platform</name><value><string>" + platform + "</string></value></member>" +
				"<member><name>mac</name><value><string>" + mac + "</string></value></member>" +
				"<member><name>viewer_digest</name><value><string>" + viewerDigest + "</string></value></member>" +
				"<member><name>user-agent</name><value><string>" + userAgent + 
				" (" + Helpers.VERSION + ")</string></value></member>" +
				"<member><name>author</name><value><string>" + author + "</string></value></member>" +
				"</struct></value></param></params></methodCall>"
				;

			// Override SSL authentication mechanisms
			ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();

			login = WebRequest.Create(url);
			login.ContentType = "text/xml";
			login.Method = "POST";
			login.Timeout = 12000;
			byte[] request = System.Text.Encoding.ASCII.GetBytes(loginRequest);
			login.ContentLength = request.Length;

			try
			{
				System.IO.Stream stream = login.GetRequestStream();

				stream.Write(request, 0, request.Length);
				stream.Close();
				response = login.GetResponse();

				if (response == null)
				{
					LoginError = "Error logging in: (Unknown)";
					Helpers.Log(LoginError, Helpers.LogLevel.Warning);
					return false;
				}

				//TODO: To support UTF8 avatar names the encoding should be handled better
				System.IO.StreamReader streamReader = new System.IO.StreamReader(response.GetResponseStream(), 
					System.Text.Encoding.ASCII);
				LoginBuffer = streamReader.ReadToEnd();
				streamReader.Close();
				response.Close();
			}
			catch (Exception e)
			{
				LoginError = "Caught an exception logging in: " + e.ToString();
				Helpers.Log(LoginError, Helpers.LogLevel.Warning);
			}

			// Parse the login reply and put the returned variables in to a struct
			/*if (!ParseLoginReply())
			{
				return false;
			}

			// Connect to the sim given in the login reply
			Circuit circuit = new Circuit(Protocol, this, UserCallbacks, InternalCallbacks, LoginValues.CircuitCode);
			if (!circuit.Open(LoginValues.IP, LoginValues.Port))
			{
				return false;
			}

			// Circuit was successfully opened, add it to the list and set it as default
			Circuits.Add(circuit);
			CurrentCircuit = circuit;

			// Move our agent in to the sim to complete the connection
			Packet packet = PacketBuilder.CompleteAgentMovement(Protocol, LoginValues.AgentID, LoginValues.SessionID,
				LoginValues.CircuitCode);
			SendPacket(packet);

			return true;*/

			return false;
		}

		public void Logout()
		{
			// TODO: Close all circuits except the current one

			// Halt all timers on the current circuit
			CurrentCircuit.Stop();

			Packet packet = PacketBuilder.LogoutRequest(Protocol, AgentID, SessionID);
			SendPacket(packet);

			// TODO: We should probably check if the server actually received the logout request
			// Instead we'll use this silly Sleep()
			System.Threading.Thread.Sleep(1000);
		}

		string RpcGetString(string rpc, string name)
		{
			int pos = rpc.IndexOf(name);
			int pos2;

			if (pos == -1)
			{
				return "";
			}

			rpc = rpc.Substring(pos, rpc.Length - pos);
			pos = rpc.IndexOf("<string>");

			if (pos == -1)
			{
				return "";
			}

			rpc = rpc.Substring(pos + 8, rpc.Length - (pos + 8));

			pos2 = rpc.IndexOf("</string>");

			if (pos2 == -1)
			{
				return "";
			}

			return rpc.Substring(0, pos2);
		}

		int RpcGetInt(string rpc, string name)
		{
			int pos = rpc.IndexOf(name);
			int pos2;

			if (pos == -1)
			{
				return -1;
			}

			rpc = rpc.Substring(pos, rpc.Length - pos);
			pos = rpc.IndexOf("<i4>");

			if (pos == -1)
			{
				return -1;
			}

			rpc = rpc.Substring(pos + 4, rpc.Length - (pos + 4));

			pos2 = rpc.IndexOf("</i4>");
			
			if (pos2 == -1)
			{
				return -1;
			}

			return Int32.Parse(rpc.Substring(0, pos2));
		}

		private void StartPingCheckHandler(Packet packet, Circuit circuit)
		{
			//TODO: Should we care about OldestUnacked?

			// Respond to the ping request
			Packet pingPacket = PacketBuilder.CompletePingCheck(Protocol, packet.Data[5]);
			SendPacket(pingPacket, circuit);
		}

		private void RegionHandshakeHandler(Packet packet, Circuit circuit)
		{
			// Send a RegionHandshakeReply
			Packet replyPacket = new Packet("RegionHandshakeReply", Protocol, 12);
			SendPacket(replyPacket, circuit);
		}
	}
}
