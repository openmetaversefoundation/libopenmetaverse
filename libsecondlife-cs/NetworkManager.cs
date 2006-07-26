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
	public delegate void PacketCallback(Packet packet, Simulator simulator);

	public class NotConnectedException : ApplicationException { }

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

	public class Simulator
	{
		private ProtocolManager Protocol;
		private NetworkManager Network;
		private Hashtable Callbacks;
		private ushort Sequence;
		private byte[] RecvBuffer;
		private Mutex RecvBufferMutex = new Mutex(false, "RecvBufferMutex");
		private Socket Connection;
		private AsyncCallback ReceivedData;
		private Hashtable NeedAck;
		private Mutex NeedAckMutex;
		private SortedList Inbox;
		private Mutex InboxMutex;
		private bool connected;

		private uint circuitCode;
		private IPEndPoint ipEndPoint;
		private EndPoint endPoint;

		public uint CircuitCode
		{
			get { return circuitCode; }
		}
		public IPEndPoint IPEndPoint
		{
			get { return ipEndPoint; }
		}
		public bool Connected
		{
			get { return connected; }
		}
		public Region Region;

		public Simulator(SecondLife client, Hashtable callbacks, uint circuit, 
			IPAddress ip, int port)
		{
			Initialize(client, callbacks, circuit, ip, port);
		}

		private void Initialize(SecondLife client, Hashtable callbacks, uint circuit, 
			IPAddress ip, int port)
		{
			Protocol = client.Protocol;
			Network = client.Network;
			Callbacks = callbacks;
			Region = new Region(client);
			circuitCode = circuit;
			Sequence = 0;
			RecvBuffer = new byte[2048];
			Connection = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			connected = false;

			// Initialize the hashtable for reliable packets waiting on ACKs from the server
			NeedAck = new Hashtable();

			// Initialize the list of sequence numbers we've received so far
			Inbox = new SortedList();

			NeedAckMutex = new Mutex(false, "NeedAckMutex");
			InboxMutex = new Mutex(false, "InboxMutex");

			Helpers.Log("Connecting to " + ip.ToString() + ":" + port, Helpers.LogLevel.Info);

			try
			{
				// Setup the callback
				ReceivedData = new AsyncCallback(OnReceivedData);

				// Create an endpoint that we will be communicating with (need it in two 
				// types due to .NET weirdness)
				ipEndPoint = new IPEndPoint(ip, port);
				endPoint = (EndPoint)ipEndPoint;

				// Associate this simulator's socket with the given ip/port and start listening
				Connection.Connect(endPoint);
				Connection.BeginReceiveFrom(RecvBuffer, 0, RecvBuffer.Length, SocketFlags.None, ref endPoint, ReceivedData, null);

				// Send the UseCircuitCode packet to initiate the connection
				Packet packet = Packets.Network.UseCircuitCode(Protocol, Network.AgentID, 
					Network.SessionID, CircuitCode);

				// Send the initial packet out
				SendPacket(packet, true);

				// Track the current time for timeout purposes
				DateTime start = DateTime.Now;
				TimeSpan timeTaken;

				while (true)
				{
					timeTaken = DateTime.Now - start;
					if (connected || timeTaken.Milliseconds > 5000) { return; }

					Thread.Sleep(10);
				}
			}
			catch (Exception e)
			{
				Helpers.Log(e.ToString(), Helpers.LogLevel.Error);
			}
		}

		public void Disconnect()
		{
			try
			{
				connected = false;

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

		public void SendPacket(Packet packet, bool incrementSequence)
		{
			byte[] buffer;
			int bytes;

			if (!connected && packet.Layout.Name != "UseCircuitCode")
			{
				Helpers.Log("Trying to send a " + packet.Layout.Name + " packet when the socket is closed",
					Helpers.LogLevel.Warning);
				
				throw new NotConnectedException();
			}

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
					byte[] zeroBuffer = new byte[4096];
					bytes = Helpers.ZeroEncode(packet.Data, packet.Data.Length, zeroBuffer);
					buffer = zeroBuffer;
				}
				else
				{
					// Normal packet, no zerocoding required
					buffer = packet.Data;
					bytes = buffer.Length;
				}

				// Append any ACKs that need to be sent out to this packet
				/*if (AckOutbox.Count != 0 && incrementSequence && 
					packet.Layout.Name != "PacketAck" && 
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
				}*/

				Connection.Send(buffer, bytes, SocketFlags.None);
			}
			catch (Exception e)
			{
				Helpers.Log(e.ToString(), Helpers.LogLevel.Error);
			}
		}

		private void SendACK(uint id)
		{
			try
			{
				Packet packet = new Packet("PacketAck", Protocol, 13);
				packet.Data[8] = 1;
				Array.Copy(BitConverter.GetBytes(id), 0, packet.Data, 9, 4);

				// Set the sequence number
				packet.Sequence = ++Sequence;

				// Bypass SendPacket and send the ACK directly
				int numSent = Connection.Send(packet.Data);
			}
			catch (Exception e)
			{
				Helpers.Log(e.ToString(), Helpers.LogLevel.Error);
			}
		}

		private void OnReceivedData(IAsyncResult result)
		{
			Packet packet;

			try
			{
				RecvBufferMutex.WaitOne();

				// If we're receiving data the sim connection is open
				connected = true;

				// Retrieve the incoming packet
				int numBytes = Connection.EndReceiveFrom(result, ref endPoint);

				if ((RecvBuffer[0] & Helpers.MSG_APPENDED_ACKS) != 0)
				{
					// Grab the ACKs that are appended to this packet
					byte numAcks = RecvBuffer[numBytes - 1];

					Helpers.Log("Found " + numAcks + " appended acks", Helpers.LogLevel.Info);

					NeedAckMutex.WaitOne();
					for (int i = 1; i <= numAcks; ++i)
					{
						ushort ack = (ushort)BitConverter.ToUInt32(RecvBuffer, numBytes - i * 4 - 1);

					Beginning:
						ICollection reliablePackets = NeedAck.Keys;

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

				// Zerodecode this packet if necessary
				if ((RecvBuffer[0] & Helpers.MSG_ZEROCODED) != 0)
				{
					// Allocate a temporary buffer for the zerocoded packet
					byte[] zeroBuffer = new byte[4096];
					int zeroBytes = Helpers.ZeroDecode(RecvBuffer, numBytes, zeroBuffer);
					numBytes = zeroBytes;
					packet = new Packet(zeroBuffer, numBytes, Protocol);
				}
				else
				{
					// Create the packet object from our byte array
					packet = new Packet(RecvBuffer, numBytes, Protocol);
				}

				// Start listening again since we're done with RecvBuffer
				Connection.BeginReceiveFrom(RecvBuffer, 0, RecvBuffer.Length, SocketFlags.None, ref endPoint, ReceivedData, null);

				RecvBufferMutex.ReleaseMutex();

				// Track the sequence number for this packet if it's marked as reliable
				if ((packet.Data[0] & Helpers.MSG_RELIABLE) != 0)
				{
					// Send the ACK for this packet
					// TODO: If we can make it stable, go back to the periodic ACK system
					SendACK((uint)packet.Sequence);

					// Check if we already received this packet
					InboxMutex.WaitOne();
					if (Inbox.Contains(packet.Sequence))
					{
						Helpers.Log("Received a duplicate " + packet.Layout.Name + ", sequence=" +
							packet.Sequence + ", resent=" + 
							(((packet.Data[0] & Helpers.MSG_RESENT) != 0) ? "Yes" : "No"), 
							Helpers.LogLevel.Info);

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
	}

	public class NetworkManager
	{
		public LLUUID AgentID;
		public LLUUID SessionID;
		public string LoginError;
		public Simulator CurrentSim;
		public Hashtable LoginValues;

		private Hashtable Callbacks;
		private SecondLife Client;
		private ProtocolManager Protocol;
		private ArrayList Simulators;
		private Mutex SimulatorsMutex;

		public NetworkManager(SecondLife client, ProtocolManager protocol)
		{
			Client = client;
			Protocol = protocol;
			Simulators = new ArrayList();
			SimulatorsMutex = new Mutex(false, "SimulatorsMutex");
			Callbacks = new Hashtable();
			CurrentSim = null;
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
			if (CurrentSim != null)
			{
				CurrentSim.SendPacket(packet, true);
			}
			else
			{
				Helpers.Log("Trying to send a packet when there is no current simulator", Helpers.LogLevel.Error);
			}
		}

		public void SendPacket(Packet packet, Simulator simulator)
		{
			simulator.SendPacket(packet, true);
		}

		public static Hashtable DefaultLoginValues(string firstName, string lastName, string password,
			string userAgent, string author)
		{
			return DefaultLoginValues(firstName, lastName, password, "00:00:00:00:00:00", "last", 
				1, 50, 50, 50, "Win", "0", userAgent, author);
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

			if ((string)LoginValues["login"] == "false")
			{
				LoginError = LoginValues["reason"] + ": " + LoginValues["message"];
				return false;
			}

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

			Hashtable home = null;

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
				home = new Hashtable();
				home["position"] = posVector;
				home["look_at"] = lookatVector;
				home["region_handle"] = regionHandle;

				LoginValues["home"] = home;
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
			Simulator simulator = new Simulator(Client, this.Callbacks, circuitCode, 
				IPAddress.Parse((string)LoginValues["sim_ip"]), (int)LoginValues["sim_port"]);
			if (!simulator.Connected)
			{
				return false;
			}

			// Set the current region
			Client.CurrentRegion = simulator.Region;

			// Simulator is successfully connected, add it to the list and set it as default
			SimulatorsMutex.WaitOne();
			Simulators.Add(simulator);
			SimulatorsMutex.ReleaseMutex();
			CurrentSim = simulator;

			// Move our agent in to the sim to complete the connection
			Packet packet = Packets.Sim.CompleteAgentMovement(Protocol, AgentID, SessionID, circuitCode);
			SendPacket(packet, simulator);

			return true;
		}

		public bool Connect(IPAddress ip, ushort port, uint circuitCode, bool setDefault)
		{
			Simulator simulator = new Simulator(Client, this.Callbacks, circuitCode,
				ip, (int)port);

			if (!simulator.Connected)
			{
				return false;
			}

			SimulatorsMutex.WaitOne();
			Simulators.Add(simulator);
			SimulatorsMutex.ReleaseMutex();

			if (setDefault)
			{
				CurrentSim = simulator;
			}

			return true;
		}

		public void Disconnect(Simulator simulator)
		{
			// FIXME: Properly handle disconnects
			/*
			if (circuit == CurrentCircuit)
			{
				Helpers.Log("Disconnecting simulator " + simulator.IPEndPoint.ToString(), Helpers.LogLevel.Info);

				circuit.CloseCircuit();

				CircuitsMutex.WaitOne();
				Circuits.Remove(circuit);
				if (Circuits.Count > 0)
				{
					CurrentCircuit = (Circuit)Circuits[0];
					Helpers.Log("Switched current simulator to " + CurrentCircuit.ipEndPoint.ToString(), 
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
			*/
		}

		public void Logout()
		{
			try
			{
			Beginning:
				// Disconnect all simulators except the current one
				SimulatorsMutex.WaitOne();
				if (Simulators.Count > 1)
				{
					foreach (Simulator simulator in Simulators)
					{
						if (simulator.CircuitCode != CurrentSim.CircuitCode)
						{
							Disconnect(simulator);
							goto Beginning;
						}
					}
				}

				Packet packet = Packets.Network.LogoutRequest(Protocol, AgentID, SessionID);
				CurrentSim.SendPacket(packet, true);

				Simulators.Clear();
				CurrentSim = null;
				SimulatorsMutex.ReleaseMutex();

				// TODO: We should probably check if the server actually received the logout request
				// Instead we'll use this silly Sleep() to keep from accidentally flooding the login server
				System.Threading.Thread.Sleep(1000);
			}
			catch (Exception e)
			{
				Helpers.Log("Logout error: " + e.ToString(), Helpers.LogLevel.Error);
			}
		}

		private void StartPingCheckHandler(Packet packet, Simulator simulator)
		{
			// Respond to the ping request
			Packet pingPacket = Packets.Network.CompletePingCheck(Protocol, (byte)packet.Field("PingID"));
			SendPacket(pingPacket, simulator);
		}

		private void RegionHandshakeHandler(Packet packet, Simulator simulator)
		{
			try
			{
				foreach (Block block in packet.Blocks())
				{
					foreach (Field field in block.Fields)
					{
						if (field.Layout.Name == "TerrainHeightRange00")
						{
							simulator.Region.TerrainHeightRange00 = (float)field.Data;
						}
						else if (field.Layout.Name == "TerrainHeightRange01")
						{
							simulator.Region.TerrainHeightRange01 = (float)field.Data;
						}
						else if (field.Layout.Name == "TerrainHeightRange10")
						{
							simulator.Region.TerrainHeightRange10 = (float)field.Data;
						}
						else if (field.Layout.Name == "TerrainHeightRange11")
						{
							simulator.Region.TerrainHeightRange11 = (float)field.Data;
						}
						else if (field.Layout.Name == "TerrainStartHeight00")
						{
							simulator.Region.TerrainStartHeight00 = (float)field.Data;
						}
						else if (field.Layout.Name == "TerrainStartHeight01")
						{
							simulator.Region.TerrainStartHeight01 = (float)field.Data;
						}
						else if (field.Layout.Name == "TerrainStartHeight10")
						{
							simulator.Region.TerrainStartHeight10 = (float)field.Data;
						}
						else if (field.Layout.Name == "TerrainStartHeight11")
						{
							simulator.Region.TerrainStartHeight11 = (float)field.Data;
						}
						else if (field.Layout.Name == "WaterHeight")
						{
							simulator.Region.WaterHeight = (float)field.Data;
						}
						else if (field.Layout.Name == "SimOwner")
						{
							simulator.Region.SimOwner = (LLUUID)field.Data;
						}
						else if (field.Layout.Name == "TerrainBase0")
						{
							simulator.Region.TerrainBase0 = (LLUUID)field.Data;
						}
						else if (field.Layout.Name == "TerrainBase1")
						{
							simulator.Region.TerrainBase1 = (LLUUID)field.Data;
						}
						else if (field.Layout.Name == "TerrainBase2")
						{
							simulator.Region.TerrainBase2 = (LLUUID)field.Data;
						}
						else if (field.Layout.Name == "TerrainBase3")
						{
							simulator.Region.TerrainBase3 = (LLUUID)field.Data;
						}
						else if (field.Layout.Name == "TerrainDetail0")
						{
							simulator.Region.TerrainDetail0 = (LLUUID)field.Data;
						}
						else if (field.Layout.Name == "TerrainDetail1")
						{
							simulator.Region.TerrainDetail1 = (LLUUID)field.Data;
						}
						else if (field.Layout.Name == "TerrainDetail2")
						{
							simulator.Region.TerrainDetail2 = (LLUUID)field.Data;
						}
						else if (field.Layout.Name == "TerrainDetail3")
						{
							simulator.Region.TerrainDetail3 = (LLUUID)field.Data;
						}
						else if (field.Layout.Name == "IsEstateManager")
						{
							simulator.Region.IsEstateManager = (bool)field.Data;
						}
						else if (field.Layout.Name == "SimName")
						{
							byte[] byteArray = (byte[])field.Data;
							simulator.Region.Name = System.Text.Encoding.UTF8.GetString(byteArray, 0, byteArray.Length).Replace("\0", "");
						}
						else if (field.Layout.Name == "CacheID")
						{
							simulator.Region.ID = (LLUUID)field.Data;
						}
					}
				}

				Helpers.Log("Received a region handshake for " + simulator.Region.Name, Helpers.LogLevel.Info);

				// Send a RegionHandshakeReply
				Packet replyPacket = new Packet("RegionHandshakeReply", Protocol, 12);
				SendPacket(replyPacket, simulator);
			}
			catch (Exception e)
			{
				Helpers.Log(e.ToString(), Helpers.LogLevel.Warning);
			}
		}

		private void ParcelOverlayHandler(Packet packet, Simulator simulator)
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
				Array.Copy(byteArray, 0, simulator.Region.ParcelOverlay, sequenceID * 1024, 1024);
				simulator.Region.ParcelOverlaysReceived++;

				if (simulator.Region.ParcelOverlaysReceived > 3)
				{
					simulator.Region.ParcelOverlaysReceived = 0;
					Helpers.Log("Finished building the " + simulator.Region.Name + " parcel overlay", 
						Helpers.LogLevel.Info);

					// The int i = 0; is just there so I could break on it and check the 
					// value of debug
					//int[,] debug = Region.ParcelOverlayToParcels(Client.CurrentRegion);
					//int i = 0;
				}
			}
			else
			{
				Helpers.Log("Parcel overlay with sequence ID of " + sequenceID + " received from " + 
					simulator.Region.Name, Helpers.LogLevel.Error);
			}
		}

		private void EnableSimulatorHandler(Packet packet, Simulator simulator)
		{
			// TODO: Actually connect to the simulator

			// TODO: Sending ConfirmEnableSimulator completely screws things up. :-?

			// Respond to the simulator connection request
			//Packet replyPacket = Packets.Network.ConfirmEnableSimulator(Protocol, AgentID, SessionID);
			//SendPacket(replyPacket, circuit);
		}
	}
}
