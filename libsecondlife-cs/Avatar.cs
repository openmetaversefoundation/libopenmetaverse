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
using System.Timers;
using System.Net;
using System.Collections;

namespace libsecondlife
{
	public delegate void ChatCallback(string message, byte audible,	byte type, byte sourcetype, 
	string name, LLUUID id, byte command, LLUUID commandID);

	public delegate void InstantMessageCallback(LLUUID FromAgentID, LLUUID ToAgentID, 
	uint ParentEstateID, LLUUID RegionID, LLVector3 Position, byte Offline, byte Dialog, 
	LLUUID ID, uint Timestamp, string AgentName, string Message, string Bucket);
	
	public class Avatar
	{
		public LLUUID ID;
		public string Name;
	}

	public class MainAvatar
	{
		public LLUUID ID;
		public string FirstName;
		public string LastName;
		public string TeleportMessage;
		public LLVector3d Position;
		public LLVector3d LookAt;
		public LLVector3d HomePosition;
		public LLVector3d HomeLookAt;

		private SecondLife Client;
		private int TeleportStatus;
		private Timer TeleportTimer;
		private bool TeleportTimeout;
		
		public event ChatCallback OnChat;
		public event InstantMessageCallback OnInstantMessage;

		public MainAvatar(SecondLife client)
		{
			Client = client;
			TeleportMessage = "";

			// Create emtpy vectors for now
			HomeLookAt = HomePosition = LookAt = Position = new LLVector3d();

			// Location callback
			PacketCallback callback = new PacketCallback(LocationHandler);
			Client.Network.InternalCallbacks["CoarseLocationUpdate"] = callback;

			// Teleport callbacks
			callback = new PacketCallback(TeleportHandler);
			Client.Network.InternalCallbacks["TeleportStart"] = callback;
			Client.Network.InternalCallbacks["TeleportProgress"] = callback;
			Client.Network.InternalCallbacks["TeleportFailed"] = callback;
			Client.Network.InternalCallbacks["TeleportFinish"] = callback;

			// Instant Message Callback
			callback = new PacketCallback(InstantMessageHandler);
			Client.Network.InternalCallbacks["ImprovedInstantMessage"] = callback;

			// Chat Callback
			callback = new PacketCallback(ChatHandler);
			Client.Network.InternalCallbacks["ChatFromSimulator"] = callback;

			TeleportTimer = new Timer(8000);
			TeleportTimer.Elapsed += new ElapsedEventHandler(TeleportTimerEvent);
			TeleportTimeout = false;
		}

		private void LocationHandler(Packet packet, Circuit circuit)
		{
			foreach (Block block in packet.Blocks())
			{
				foreach (Field field in block.Fields)
				{
					if (field.Layout.Name == "X")
					{
						Position.X = Convert.ToDouble((byte)field.Data);
					}
					else if (field.Layout.Name == "Y")
					{
						Position.Y = Convert.ToDouble((byte)field.Data);
					}
					else if (field.Layout.Name == "Z")
					{
						Position.Z = Convert.ToDouble((byte)field.Data);
					}
				}
			}

			// Send an AgentUpdate packet with the new camera location
			packet = Packets.Sim.AgentUpdate(Client.Protocol, Client.Network.AgentID, 56.0F, 
				new LLVector3((float)Position.X, (float)Position.Y, (float)Position.Z));
			Client.Network.SendPacket(packet);
		}

		private void InstantMessageHandler(Packet packet, Circuit circuit)
		{
			if (packet.Layout.Name == "ImprovedInstantMessage")
			{
				LLUUID FromAgentID	= new LLUUID();
				LLUUID ToAgentID	= new LLUUID();
				uint ParentEstateID	= 0;
				LLUUID RegionID		= new LLUUID();
				LLVector3 Position	= new LLVector3();
				byte Offline		= 0;
				byte Dialog			= 0;
				LLUUID ID			= new LLUUID();
				uint Timestamp		= 0;
				string AgentName	= "";
				string Message		= "";
				string Bucket		= "";

				ArrayList blocks;

				blocks = packet.Blocks();

				foreach (Block block in blocks)
				{
					foreach (Field field in block.Fields)
					{
						if(field.Layout.Name == "FromAgentID")
						{
							FromAgentID = (LLUUID)field.Data;
						}
						else if(field.Layout.Name == "ToAgentID")
						{
							ToAgentID = (LLUUID)field.Data;
						}
						else if(field.Layout.Name == "ParentEstateID")
						{
							ParentEstateID = (uint)field.Data;
						}
						else if(field.Layout.Name == "RegionID")
						{
							RegionID = (LLUUID)field.Data;
						}
						else if(field.Layout.Name == "Position")
						{
							Position = (LLVector3)field.Data;
						}
						else if(field.Layout.Name == "Offline")
						{
							Offline = (byte)field.Data;
						}
						else if(field.Layout.Name == "Dialog")
						{
							Dialog = (byte)field.Data;
						}
						else if(field.Layout.Name == "ID")
						{
							ID = (LLUUID)field.Data;
						}
						else if(field.Layout.Name == "Timestamp")
						{
							Timestamp = (uint)field.Data;
						}
						else if(field.Layout.Name == "FromAgentName")
						{
							AgentName = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
						}
						else if(field.Layout.Name == "Message")
						{
							Message = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
						}
						else if(field.Layout.Name == "BinaryBucket")
						{
							Bucket = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
						}
					}
				}
				OnInstantMessage(FromAgentID,ToAgentID,ParentEstateID,RegionID,Position,
					Offline,Dialog,ID,Timestamp,AgentName,Message,Bucket);
			} 
		}

		private void ChatHandler(Packet packet, Circuit circuit) 
		{
			if (packet.Layout.Name == "ChatFromSimulator")
			{
				string message		= "";
				byte audible		= 0;
				byte type			= 0;
				byte sourcetype		= 0;
				string name			= "";
				LLUUID id			= new LLUUID();
				byte command		= 0;
				LLUUID commandID	= new LLUUID();

				ArrayList blocks;

				blocks = packet.Blocks();

				foreach (Block block in blocks)
				{
					foreach (Field field in block.Fields)
					{
						if (field.Layout.Name == "ID")
						{
							id = (LLUUID)field.Data;
						} 
						else if(field.Layout.Name == "Name")
						{
							name = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
						}
						else if(field.Layout.Name == "SourceType")
						{
							sourcetype = (byte)field.Data;
						}
						else if(field.Layout.Name == "Type")
						{
							type = (byte)field.Data;
						}
						else if(field.Layout.Name == "Audible")
						{
							audible = (byte)field.Data;
						}
						else if(field.Layout.Name == "Message")
						{
							message = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
						}
						else if(field.Layout.Name == "Command")
						{
							command = (byte)field.Data;
						}
						else if(field.Layout.Name == "CommandID")
						{
							commandID = (LLUUID)field.Data;
						}
						
					}
				}

				//DEBUG
				//Helpers.Log("Chat: Message=" + message + ", Type=" + type, Helpers.LogLevel.Info);

				if (OnChat != null)
				{
					OnChat(message, audible, type, sourcetype, name, id, command, commandID);
				}
			}
		}

		public void InstantMessage(LLUUID target, string message) 
		{
			TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
			uint now = (uint)(t.TotalSeconds);
			string name = FirstName + " " + LastName;

			InstantMessage(name, LLUUID.GenerateUUID(), target, message, null);
		}

		public void InstantMessage(string fromName, LLUUID sessionID, LLUUID target, string message, LLUUID[] conferenceIDs)
		{
			TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
			uint now = (uint)(t.TotalSeconds);

			byte[] binaryBucket;
			
			if (conferenceIDs != null && conferenceIDs.Length > 0)
			{
				binaryBucket = new byte[16 * conferenceIDs.Length];

				for (int i = 0; i < conferenceIDs.Length; ++i)
				{
					Array.Copy(conferenceIDs[i].Data, 0, binaryBucket, 16 * i, 16);
				}
			}
			else
			{
				binaryBucket = new byte[0];
			}

			// Build the packet
			Packet packet = Packets.Communication.ImprovedInstantMessage(Client.Protocol, target, Client.Network.AgentID, 0, 
				Client.CurrentRegion.ID, new LLVector3((float)Position.X, (float)Position.Y, (float)Position.Z), 
				0, 0, sessionID, now, fromName, message, binaryBucket);

			// Send the message
			Client.Network.SendPacket(packet);
		}

		public void Chat(string message, byte type) 
		{
			LLUUID CommandID = new LLUUID();
			LLVector3 Position = new LLVector3(0.0F,0.0F,0.0F);

			Packet packet = Packets.Communication.ChatFromViewer(Client.Protocol, Client.Avatar.ID, Client.Network.SessionID,
				message, type, 0, 0, CommandID, 20, Position);

			Client.Network.SendPacket(packet);
		}

		public void Shout(string message) 
		{
			LLUUID CommandID = new LLUUID();
			LLVector3 Position = new LLVector3(0.0F,0.0F,0.0F);

			Packet packet = Packets.Communication.ChatFromViewer(Client.Protocol,Client.Avatar.ID,Client.Network.SessionID,
				message, 0, 0, 0, CommandID,100,Position);

			Client.Network.SendPacket(packet);
		}

		public bool Teleport(U64 regionHandle, LLVector3 position, out string message)
		{
			TeleportStatus = 0;
			LLVector3 lookAt = new LLVector3(position.X + 1.0F, position.Y, position.Z);

			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();
			fields["RegionHandle"] = regionHandle;
			fields["LookAt"] = lookAt;
			fields["Position"] = position;
			blocks[fields] = "Info";
			fields = new Hashtable();
			fields["AgentID"] = Client.Network.AgentID;
			fields["SessionID"] = Client.Network.SessionID;
			blocks[fields] = "AgentData";
			Packet packet = PacketBuilder.BuildPacket("TeleportLocationRequest", Client.Protocol, blocks, 
				Helpers.MSG_RELIABLE + Helpers.MSG_ZEROCODED);

			Helpers.Log("Teleporting to region " + regionHandle.ToString(), Helpers.LogLevel.Info);

			// Start the timeout check
			TeleportTimeout = false;
			TeleportTimer.Start();

			Client.Network.SendPacket(packet);

			while (TeleportStatus == 0 && !TeleportTimeout)
			{
				Client.Tick();
			}

			TeleportTimer.Stop();

			if (TeleportTimeout)
			{
				message = "Teleport timed out";
			}
			else
			{
				message = TeleportMessage;
			}

			return (TeleportStatus == 1);
		}

		private void TeleportHandler(Packet packet, Circuit circuit)
		{
			ArrayList blocks;

			if (packet.Layout.Name == "TeleportStart")
			{
				TeleportMessage = "Teleport started";
			}
			else if (packet.Layout.Name == "TeleportProgress")
			{
				blocks = packet.Blocks();

				foreach (Block block in blocks)
				{
					foreach (Field field in block.Fields)
					{
						if (field.Layout.Name == "Message")
						{
							TeleportMessage = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
							return;
						}
					}
				}
			}
			else if (packet.Layout.Name == "TeleportFailed")
			{
				blocks = packet.Blocks();

				foreach (Block block in blocks)
				{
					foreach (Field field in block.Fields)
					{
						if (field.Layout.Name == "Reason")
						{
							TeleportMessage = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
							TeleportStatus = -1;
							return;
						}
					}
				}
			}
			else if (packet.Layout.Name == "TeleportFinish")
			{
				TeleportMessage = "Teleport finished";

				ushort port = 0;
				IPAddress ip = null;
				U64 regionHandle;

				blocks = packet.Blocks();

				foreach (Block block in blocks)
				{
					foreach (Field field in block.Fields)
					{
						if (field.Layout.Name == "SimPort")
						{
							port = (ushort)field.Data;
						}
						else if (field.Layout.Name == "SimIP")
						{
							ip = (IPAddress)field.Data;
						}
						else if (field.Layout.Name == "RegionHandle")
						{
							regionHandle = (U64)field.Data;
						}
					}
				}

				if (Client.Network.Connect(ip, port, Client.Network.CurrentCircuit.CircuitCode, true))
				{
					// Move the avatar in to this sim
					Packet movePacket = Packets.Sim.CompleteAgentMovement(Client.Protocol, Client.Network.AgentID,
						Client.Network.SessionID, Client.Network.CurrentCircuit.CircuitCode);
					Client.Network.SendPacket(movePacket);

					Helpers.Log("Connected to new sim " + Client.Network.CurrentCircuit.ipEndPoint.ToString(), 
						Helpers.LogLevel.Info);

					// Sleep a little while so we can collect parcel information
					System.Threading.Thread.Sleep(1000);

					TeleportStatus = 1;
					return;
				}
				else
				{
					TeleportStatus = -1;
					return;
				}
			}
		}

		private void TeleportTimerEvent(object source, System.Timers.ElapsedEventArgs ea)
		{
			TeleportTimeout = true;
		}
	}
}
