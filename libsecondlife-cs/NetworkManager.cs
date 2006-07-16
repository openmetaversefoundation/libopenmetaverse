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
using Nii.JSON;

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
		private Hashtable Callbacks;
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
		private SortedList Inbox;
		private Mutex InboxMutex;
		private int ResendTick;

		public Circuit(ProtocolManager protocol, NetworkManager network, Hashtable callbacks, 
			uint circuitCode)
		{
			Initialize(protocol, network, callbacks, circuitCode);
		}

		public Circuit(ProtocolManager protocol, NetworkManager network, Hashtable callbacks)
		{
			// Generate a random circuit code
			System.Random random = new System.Random();

			Initialize(protocol, network, callbacks, (uint)random.Next());
		}

		private void Initialize(ProtocolManager protocol, NetworkManager network, Hashtable callbacks, 
			uint circuitCode)
		{
			Protocol = protocol;
			Network = network;
			Callbacks = callbacks;
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

			Inbox = new SortedList();

			// Create a timer to test if the connection times out
			OpenTimer = new System.Timers.Timer(10000);
			OpenTimer.Elapsed += new ElapsedEventHandler(OpenTimerEvent);

			// Create a timer to send PacketAcks and resend unACKed packets
			ACKTimer = new System.Timers.Timer(300);
			ACKTimer.Elapsed += new ElapsedEventHandler(ACKTimerEvent);

			AckOutboxMutex = new Mutex(false, "AckOutboxMutex");
			NeedAckMutex = new Mutex(false, "NeedAckMutex");
			InboxMutex = new Mutex(false, "InboxMutex");

			ResendTick = 0;
		}

		~Circuit()
		{
			Opened = false;
			StopTimers();
		}

		public bool Open(string ip, int port)
		{
			return Open(IPAddress.Parse(ip), port);
		}

		public bool Open(IPAddress ip, int port)
		{
			Helpers.Log("Connecting to " + ip.ToString() + ":" + port, Helpers.LogLevel.Info);

			try
			{
				// Setup the callback
				ReceivedData = new AsyncCallback(this.OnReceivedData);

				// Create an endpoint that we will be communicating with (need it in two types due to
				// .NET weirdness)
				ipEndPoint = new IPEndPoint(ip, port);
				endPoint = (EndPoint)ipEndPoint;

				// Associate this circuit's socket with the given ip and port and start listening
				Connection.Connect(endPoint);
				Connection.BeginReceiveFrom(Buffer, 0, Buffer.Length, SocketFlags.None, ref endPoint, ReceivedData, null);

				// Start the circuit opening timeout
				OpenTimer.Start();

				// Start the packet resend timer
				ACKTimer.Start();

				// Send the UseCircuitCode packet to initiate the connection
				Packet packet = Packets.Network.UseCircuitCode(Protocol, Network.AgentID, 
					Network.SessionID, CircuitCode);

				// Send the initial packet out
				SendPacket(packet, true);

				while (!Timeout)
				{
					if (Opened)
					{
						return true;
					}

					Thread.Sleep(10);
				}
			}
			catch (Exception e)
			{
				Helpers.Log(e.ToString(), Helpers.LogLevel.Error);
			}

			// Opening the connection failed, shut down all the timers
			StopTimers();

			return false;
		}

		public void CloseCircuit()
		{
			try
			{
				Opened = false;

				StopTimers();

				// Send the CloseCircuit notice
				Packet packet = new Packet("CloseCircuit", Protocol, 8);
				Connection.Send(packet.Data);

				// Shut the socket communication down
				Connection.Shutdown(SocketShutdown.Both);
			}
			catch (Exception e)
			{
				Helpers.Log(e.ToString(), Helpers.LogLevel.Error);
			}
		}

		public void StopTimers()
		{
			// Stop the resend timer
			ACKTimer.Stop();

			// Stop the open circuit timer (just in case it's still running)
			OpenTimer.Stop();
		}

		public void SendPacket(Packet packet, bool incrementSequence)
		{
			byte[] zeroBuffer = new byte[4096];
			int zeroBytes;

			if (!Opened && packet.Layout.Name != "UseCircuitCode")
			{
				Helpers.Log("Trying to send a " + packet.Layout.Name + " packet when the socket is closed",
					Helpers.LogLevel.Warning);
				return;
			}

			// DEBUG
			//Console.WriteLine("Sending " + packet.Data.Length + " byte " + packet.Layout.Name);

			try
			{
				if (incrementSequence)
				{
					if ((packet.Data[0] & Helpers.MSG_RELIABLE) != 0)
					{
						NeedAckMutex.WaitOne();
						if (!NeedAck.ContainsKey(packet))
						{
							// This packet needs an ACK, keep track of when it was sent out
							NeedAck.Add(packet, Environment.TickCount);
						}
						NeedAckMutex.ReleaseMutex();
					}

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

					// Append each ACK needing to be sent out to this packet
					foreach (uint ack in AckOutbox)
					{
						Array.Copy(BitConverter.GetBytes(ack), 0, zeroBuffer, zeroBytes, 4);
						zeroBytes += 4;
					}

					// Last byte is the number of ACKs
					zeroBuffer[zeroBytes] = (byte)AckOutbox.Count;
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
			AckOutboxMutex.WaitOne();
			if (AckOutbox.Count != 0)
			{
				try
				{
					Packet packet = Packets.Network.PacketAck(Protocol, AckOutbox);

					// Set the sequence number
					packet.Sequence = ++Sequence;

					// Bypass SendPacket since we are taking care of the AckOutbox ourself
					int numSent = Connection.Send(packet.Data);

					AckOutbox.Clear();
				}
				catch (Exception e)
				{
					Helpers.Log(e.ToString(), Helpers.LogLevel.Error);
				}
			}
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

					NeedAckMutex.WaitOne();
					for (int i = 1; i <= numAcks; ++i)
					{
						ushort ack = (ushort)BitConverter.ToUInt32(Buffer, numBytes - i * 4 - 1);

					Beginning:
						ArrayList reliablePackets = (ArrayList)NeedAck.Keys;

						foreach (Packet reliablePacket in reliablePackets)
						{
							if (reliablePacket.Sequence == ack)
							{
								NeedAck.Remove(reliablePacket);
								goto Beginning;
							}
						}
					}
					NeedAckMutex.ReleaseMutex();

					// Adjust the packet length
					numBytes = numBytes - numAcks * 4 - 1;
				}

				if ((Buffer[0] & Helpers.MSG_ZEROCODED) != 0)
				{
					// Allocate a temporary buffer for the zerocoded packet
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

				// Track the sequence number for this packet if it's marked as reliable
				if ((packet.Data[0] & Helpers.MSG_RELIABLE) != 0)
				{
					// Add this packet to the ACK outbox if it isn't there already
					AckOutboxMutex.WaitOne();
					if (!AckOutbox.Contains((uint)packet.Sequence))
					{
						AckOutbox.Add((uint)packet.Sequence);
					}
					else
					{
						Helpers.Log("ACKs are being sent too slowly!", Helpers.LogLevel.Warning);
					}
					if (AckOutbox.Count > 3)
					{
						AckOutboxMutex.ReleaseMutex();
						SendACKs();
					}
					else
					{
						AckOutboxMutex.ReleaseMutex();
					}

					// Check if we already received this packet
					InboxMutex.WaitOne();
					if (Inbox.Contains(packet.Sequence))
					{
						Helpers.Log("Received a duplicate " + packet.Layout.Name + ", sequence=" +
							packet.Sequence + ", resent=" + 
							(((packet.Data[0] & Helpers.MSG_RESENT) != 0) ? "Yes" : "No"), Helpers.LogLevel.Info);

						// Avoid firing a callback twice for the same packet
						return;
					}
					else
					{
						Inbox.Add(packet.Sequence, packet);
					}
					InboxMutex.ReleaseMutex();
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
					foreach (Block block in blocks)
					{
						foreach (Field field in block.Fields)
						{
							ICollection reliablePackets = NeedAck.Keys;

							// Remove this packet if it exists
							foreach (Packet reliablePacket in reliablePackets)
							{
								if ((uint)reliablePacket.Sequence == (uint)field.Data)
								{
									NeedAck.Remove(reliablePacket);
									break;
								}
							}
						}
					}
					NeedAckMutex.ReleaseMutex();
				}

				if (Callbacks.ContainsKey(packet.Layout.Name))
				{
					ArrayList callbackArray = (ArrayList)Callbacks[packet.Layout.Name];

					// Fire any registered callbacks
					foreach (PacketCallback callback in callbackArray)
					{
						if (callback != null)
						{
							callback(packet, this);
						}
					}
				}
				else if (Callbacks.ContainsKey("Default"))
				{
					ArrayList callbackArray = (ArrayList)Callbacks["Default"];

					// Fire any registered callbacks
					foreach (PacketCallback callback in callbackArray)
					{
						if (callback != null)
						{
							callback(packet, this);
						}
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
					NeedAckMutex.WaitOne();
					ResendTick = 0;

				Beginning:
					IDictionaryEnumerator packetEnum = NeedAck.GetEnumerator();

					// Check if any reliable packets haven't been ACKed by the server
					while (packetEnum.MoveNext())
					{
						int ticks = (int)packetEnum.Value;

						// TODO: Is this hardcoded value correct? Should it be a higher level define or a 
						//       changeable property?
						if (Environment.TickCount - ticks > 4000)
						{
							Packet packet = (Packet)packetEnum.Key;

							if (NeedAck.ContainsKey(packet))
							{
								NeedAck[packet] = Environment.TickCount;

								// Add the resent flag
								packet.Data[0] += Helpers.MSG_RESENT;
							
								// Resend the packet
								SendPacket((Packet)packet, false);

								Helpers.Log("Resending " + packet.Layout.Name + " packet, sequence=" + packet.Sequence, 
									Helpers.LogLevel.Info);

								// Rate limiting
								System.Threading.Thread.Sleep(500);

								// Restart the loop since we modified a value and the iterator will fail
								goto Beginning;
							}
						}
					}

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
		public Circuit CurrentCircuit;
		public Hashtable LoginValues;

		private Hashtable Callbacks;
		private SecondLife Client;
		private ProtocolManager Protocol;
		private ArrayList Circuits;
		private Mutex CircuitsMutex;

		public NetworkManager(SecondLife client, ProtocolManager protocol)
		{
			Client = client;
			Protocol = protocol;
			Circuits = new ArrayList();
			CircuitsMutex = new Mutex(false, "CircuitsMutex");
			Callbacks = new Hashtable();
			CurrentCircuit = null;
			LoginValues = null;

			// Register the internal callbacks
			RegisterCallback("RegionHandshake", new PacketCallback(RegionHandshakeHandler));
			RegisterCallback("StartPingCheck", new PacketCallback(StartPingCheckHandler));
			RegisterCallback("ParcelOverlay",  new PacketCallback(ParcelOverlayHandler));
			RegisterCallback("EnableSimulator",  new PacketCallback(EnableSimulatorHandler));
		}

		public void RegisterCallback(string packet, PacketCallback callback)
		{
			if (!Callbacks.ContainsKey(packet))
			{
				Callbacks[packet] = new ArrayList();
			}

			ArrayList callbackArray = (ArrayList)Callbacks[packet];
			callbackArray.Add(callback);
		}
		
		public void UnregisterCallback(string packet, PacketCallback callback)
		{
			if (!Callbacks.ContainsKey(packet))
			{
				Helpers.Log("Trying to unregister a callback for packet " + packet + 
					" when no callbacks are setup for that packet", Helpers.LogLevel.Info);
				return;
			}

			ArrayList callbackArray = (ArrayList)Callbacks[packet];

			if (callbackArray.Contains(callback))
			{
				callbackArray.Remove(callback);
			}
			else
			{
				Helpers.Log("Trying to unregister a non-existant callback for packet " + packet,
					Helpers.LogLevel.Info);
			}
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
			values["agree_to_tos"] = "true";
			values["viewer_digest"] = viewerDigest;
			values["user-agent"] = userAgent + " (" + Helpers.VERSION + ")";
			values["author"] = author;

			return values;
		}

		public bool Login(Hashtable loginParams)
		{
			return Login(loginParams, "https://login.agni.lindenlab.com/cgi-bin/login.cgi");
		}

		public bool Login(Hashtable loginParams, string url)
		{
			XmlRpcResponse result;
			XmlRpcRequest xmlrpc = new XmlRpcRequest();
			xmlrpc.MethodName = "login_to_simulator";
			xmlrpc.Params.Clear();
			xmlrpc.Params.Add(loginParams);

			try
			{
				result = (XmlRpcResponse)xmlrpc.Send(url);
			}
			catch (Exception e)
			{
				Helpers.Log(e.ToString(), Helpers.LogLevel.Error);
				LoginError = e.Message;
				LoginValues = null;
				return false;
			}

			if (result.IsFault)
			{
				Helpers.Log("Fault " + result.FaultCode + ": " + result.FaultString, Helpers.LogLevel.Error);
				LoginError = "Fault " + result.FaultCode + ": " + result.FaultString;
				LoginValues = null;
				return false;
			}

			LoginValues = (Hashtable)result.Value;

			System.Text.RegularExpressions.Regex LLSDtoJSON = 
				new System.Text.RegularExpressions.Regex(@"('|r([0-9])|r(\-))");
			string json;
			IDictionary jsonObject = null;
			LLVector3d vector = null;
			LLVector3d posVector = null;
			LLVector3d lookatVector = null;
			U64 regionHandle = null;

			if (LoginValues.Contains("look_at"))
			{
				// Replace LLSD variables with object representations

				// Convert LLSD string to JSON
				json = "{vector:" + LLSDtoJSON.Replace((string)LoginValues["look_at"], "$2") + "}";

				// Convert JSON string to a JSON object
				jsonObject = JsonFacade.fromJSON(json);
				JSONArray jsonVector = (JSONArray)jsonObject["vector"];

				// Convert the JSON object to an LLVector3d
				vector = new LLVector3d(Convert.ToDouble(jsonVector[0]), 
					Convert.ToDouble(jsonVector[1]), Convert.ToDouble(jsonVector[2]));

				LoginValues["look_at"] = vector;
			}

			if (LoginValues.Contains("home"))
			{
				// Convert LLSD string to JSON
				json = LLSDtoJSON.Replace((string)LoginValues["home"], "$2");

				// Convert JSON string to an object
				jsonObject = JsonFacade.fromJSON(json);

				// Create the position vector
				JSONArray array = (JSONArray)jsonObject["position"];
				posVector = new LLVector3d(Convert.ToDouble(array[0]), Convert.ToDouble(array[1]), 
					Convert.ToDouble(array[2]));

				// Create the look_at vector
				array = (JSONArray)jsonObject["look_at"];
				lookatVector = new LLVector3d(Convert.ToDouble(array[0]), 
					Convert.ToDouble(array[1]), Convert.ToDouble(array[2]));

				// Create the regionhandle U64
				array = (JSONArray)jsonObject["region_handle"];
				regionHandle = new U64((int)array[0], (int)array[1]);

				// Create a hashtable to hold the home values
				Hashtable home = new Hashtable();
				home["position"] = posVector;
				home["look_at"] = lookatVector;
				home["region_handle"] = regionHandle;

				LoginValues["home"] = home;
			}

			if ((string)LoginValues["login"] == "false")
			{
				LoginError = LoginValues["reason"] + ": " + LoginValues["message"];
				return false;
			}

			AgentID = new LLUUID((string)LoginValues["agent_id"]);
			SessionID = new LLUUID((string)LoginValues["session_id"]);
			Client.Avatar.ID = new LLUUID((string)LoginValues["agent_id"]);
			Client.Avatar.FirstName = (string)LoginValues["first_name"];
			Client.Avatar.LastName = (string)LoginValues["last_name"];
			Client.Avatar.LookAt = vector;
			Client.Avatar.HomePosition = posVector;
			Client.Avatar.HomeLookAt = lookatVector;
			uint circuitCode = (uint)(int)LoginValues["circuit_code"];

			// Connect to the sim given in the login reply
			Circuit circuit = new Circuit(Protocol, this, this.Callbacks, circuitCode);
			if (!circuit.Open((string)LoginValues["sim_ip"], (int)LoginValues["sim_port"]))
			{
				return false;
			}

			// Circuit was successfully opened, add it to the list and set it as default
			CircuitsMutex.WaitOne();
			Circuits.Add(circuit);
			CircuitsMutex.ReleaseMutex();
			CurrentCircuit = circuit;

			// Move our agent in to the sim to complete the connection
			Packet packet = Packets.Sim.CompleteAgentMovement(Protocol, AgentID, SessionID, circuitCode);
			SendPacket(packet);

			return true;
		}

		public bool Connect(IPAddress ip, ushort port, uint circuitCode, bool setDefault)
		{
			Circuit circuit = new Circuit(Protocol, this, this.Callbacks, circuitCode);
			if (!circuit.Open(ip, port))
			{
				return false;
			}

			CircuitsMutex.WaitOne();
			Circuits.Add(circuit);
			CircuitsMutex.ReleaseMutex();

			if (setDefault)
			{
				CurrentCircuit = circuit;
			}

			return true;
		}

		public void Disconnect(Circuit circuit)
		{
			if (circuit == CurrentCircuit)
			{
				Helpers.Log("Disconnecting current circuit " + circuit.ipEndPoint.ToString(), Helpers.LogLevel.Info);

				circuit.CloseCircuit();

				CircuitsMutex.WaitOne();
				Circuits.Remove(circuit);
				if (Circuits.Count > 0)
				{
					CurrentCircuit = (Circuit)Circuits[0];
					Helpers.Log("Switched current circuit to " + CurrentCircuit.ipEndPoint.ToString(), 
						Helpers.LogLevel.Info);
				}
				else
				{
					Helpers.Log("Last circuit disconnected, no open connections left", Helpers.LogLevel.Info);
					CurrentCircuit = null;
				}
				CircuitsMutex.ReleaseMutex();

				return;
			}
			else
			{
				Helpers.Log("Disconnecting circuit " + circuit.ipEndPoint.ToString(), Helpers.LogLevel.Info);

				circuit.CloseCircuit();
				CircuitsMutex.WaitOne();
				Circuits.Remove(circuit);
				CircuitsMutex.ReleaseMutex();
				return;
			}

			//Helpers.Log("Disconnect called with invalid circuit code " + circuitCode, Helpers.LogLevel.Warning);
		}

		public void Logout()
		{
			try
			{
				// Halt all activity on the current circuit
				CurrentCircuit.StopTimers();

			Beginning:
				// Disconnect all circuits except the current one
				CircuitsMutex.WaitOne();
				if (Circuits.Count > 1)
				{
					foreach (Circuit circuit in Circuits)
					{
						if (circuit.CircuitCode != CurrentCircuit.CircuitCode)
						{
							Disconnect(circuit);
							goto Beginning;
						}
					}
				}

				Packet packet = Packets.Network.LogoutRequest(Protocol, AgentID, SessionID);
				SendPacket(packet);

				Circuits.Clear();
				CurrentCircuit = null;
				CircuitsMutex.ReleaseMutex();

				// TODO: We should probably check if the server actually received the logout request
				// Instead we'll use this silly Sleep() to keep from accidentally flooding the login server
				System.Threading.Thread.Sleep(1000);
			}
			catch (Exception e)
			{
				Helpers.Log("Logout error: " + e.ToString(), Helpers.LogLevel.Error);
			}
		}

		private void StartPingCheckHandler(Packet packet, Circuit circuit)
		{
			// Respond to the ping request
			Packet pingPacket = Packets.Network.CompletePingCheck(Protocol, (byte)packet.Field("PingID"));
			SendPacket(pingPacket, circuit);
		}

		private void RegionHandshakeHandler(Packet packet, Circuit circuit)
		{
			ArrayList blocks = packet.Blocks();
			float[] heightList = new float[9];
			LLUUID[] terrainImages = new LLUUID[8];
			string name = "";
			LLUUID id = null;
			LLUUID simOwner = null;
			bool isEstateManager = false;

			foreach (Block block in blocks)
			{
				foreach (Field field in block.Fields)
				{
					//output += "  " + field.Layout.Name + ": " + field.Data.ToString() + "\n";
					if (field.Layout.Name == "TerrainHeightRange00")
					{
						heightList[0] = (float)field.Data;
					}
					else if (field.Layout.Name == "TerrainHeightRange01")
					{
						heightList[1] = (float)field.Data;
					}
					else if (field.Layout.Name == "TerrainHeightRange10")
					{
						heightList[2] = (float)field.Data;
					}
					else if (field.Layout.Name == "TerrainHeightRange11")
					{
						heightList[3] = (float)field.Data;
					}
					else if (field.Layout.Name == "TerrainStartHeight00")
					{
						heightList[4] = (float)field.Data;
					}
					else if (field.Layout.Name == "TerrainStartHeight01")
					{
						heightList[5] = (float)field.Data;
					}
					else if (field.Layout.Name == "TerrainStartHeight10")
					{
						heightList[6] = (float)field.Data;
					}
					else if (field.Layout.Name == "TerrainStartHeight11")
					{
						heightList[7] = (float)field.Data;
					}
					else if (field.Layout.Name == "WaterHeight")
					{
						heightList[8] = (float)field.Data;
					}
					else if (field.Layout.Name == "SimOwner")
					{
						simOwner = (LLUUID)field.Data;
					}
					else if (field.Layout.Name == "TerrainBase0")
					{
						terrainImages[0] = (LLUUID)field.Data;
					}
					else if (field.Layout.Name == "TerrainBase1")
					{
						terrainImages[1] = (LLUUID)field.Data;
					}
					else if (field.Layout.Name == "TerrainBase2")
					{
						terrainImages[2] = (LLUUID)field.Data;
					}
					else if (field.Layout.Name == "TerrainBase3")
					{
						terrainImages[3] = (LLUUID)field.Data;
					}
					else if (field.Layout.Name == "TerrainDetail0")
					{
						terrainImages[4] = (LLUUID)field.Data;
					}
					else if (field.Layout.Name == "TerrainDetail1")
					{
						terrainImages[5] = (LLUUID)field.Data;
					}
					else if (field.Layout.Name == "TerrainDetail2")
					{
						terrainImages[6] = (LLUUID)field.Data;
					}
					else if (field.Layout.Name == "TerrainDetail3")
					{
						terrainImages[7] = (LLUUID)field.Data;
					}
					else if (field.Layout.Name == "IsEstateManager")
					{
						isEstateManager = (bool)field.Data;
					}
					else if (field.Layout.Name == "SimName")
					{
						byte[] byteArray = (byte[])field.Data;
						name = System.Text.Encoding.UTF8.GetString(byteArray, 0, byteArray.Length).Replace("\0", "");
					}
					else if (field.Layout.Name == "CacheID")
					{
						id = (LLUUID)field.Data;
					}
				}
			}

			Hashtable home = (Hashtable)LoginValues["home"];

			Region region = new Region(Client, id, (U64)home["region_handle"], name, heightList, simOwner, terrainImages, isEstateManager);

			Region foundRegion = Client.FindRegion(region.Name);

			Client.RegionsMutex.WaitOne();
			if (foundRegion == null)
			{
				Client.Regions.Add(region);
				Client.CurrentRegion = region;
			}
			else
			{
				Client.CurrentRegion = foundRegion;
			}
			Client.RegionsMutex.ReleaseMutex();

			Helpers.Log("Received a region handshake for " + region.Name, Helpers.LogLevel.Info);

			// Send a RegionHandshakeReply
			Packet replyPacket = new Packet("RegionHandshakeReply", Protocol, 12);
			SendPacket(replyPacket, circuit);
		}

		private void ParcelOverlayHandler(Packet packet, Circuit circuit)
		{
			int sequenceID = -1;
			byte[] byteArray = null;

			foreach (Block block in packet.Blocks())
			{
				foreach (Field field in block.Fields)
				{
					if (field.Layout.Name == "SequenceID")
					{
						sequenceID = (int)field.Data;
					}
					else if (field.Layout.Name == "Data")
					{
						byteArray = (byte[])field.Data;
						if (byteArray.Length != 1024)
						{
							Helpers.Log("Received a parcel overlay packet with " + byteArray.Length + " bytes", 
								Helpers.LogLevel.Error);
						}
					}
				}
			}

			if (sequenceID >= 0 && sequenceID <= 3)
			{
				if (Client.ParcelOverlaysReceived > 3)
				{
					Client.ParcelOverlaysReceived = 0;
					Array.Clear(Client.CurrentParcelOverlay, 0, Client.CurrentParcelOverlay.Length);
					Helpers.Log("Reset current parcel overlay", Helpers.LogLevel.Info);
				}

				Array.Copy(byteArray, 0, Client.CurrentParcelOverlay, sequenceID * 1024, 1024);
				Client.ParcelOverlaysReceived++;

				Helpers.Log("Parcel overlay " + sequenceID + " received", Helpers.LogLevel.Info);
			}
			else
			{
				Helpers.Log("Parcel overlay with sequence ID of " + sequenceID + " received", Helpers.LogLevel.Error);
			}
		}

		private void EnableSimulatorHandler(Packet packet, Circuit circuit)
		{
			// TODO: Actually connect to the simulator

			// TODO: Sending ConfirmEnableSimulator completely screws things up. :-?

			// Respond to the simulator connection request
			//Packet replyPacket = Packets.Network.ConfirmEnableSimulator(Protocol, AgentID, SessionID);
			//SendPacket(replyPacket, circuit);
		}
	}
}
