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
using System.Net;
using System.Collections;

namespace libsecondlife
{
	public struct Field
	{
		public object Data;
		public MapField Layout;
	}

	public struct Block
	{
		public ArrayList Fields;
		public MapBlock Layout;
	}

	public class Packet
	{
		public byte[] Data;
		public MapPacket Layout;
		public ushort Sequence
		{
			get
			{
				short sequence = BitConverter.ToInt16(Data, 2);
				// TODO: To support big endian platforms, we need to replace NetworkToHostOrder with
				//       a big endian to little endian function
				return (ushort)IPAddress.NetworkToHostOrder(sequence);
			}

			set
			{
				byte[] sequence = BitConverter.GetBytes(value);

				if (BitConverter.IsLittleEndian)
				{
					Data[2] = sequence[1];
					Data[3] = sequence[0];
				}
				else
				{
					Data[2] = sequence[0];
					Data[3] = sequence[1];
				}
			}
		}

		private ProtocolManager Protocol;

		public Packet(string command, ProtocolManager protocol, int length)
		{
			Protocol = protocol;
			Data = new byte[length];
			Layout = protocol.Command(command);

			if (Layout == null)
			{
				Helpers.Log("Attempting to build a packet with invalid command \"" + command + "\"", 
					Helpers.LogLevel.Error);
			}

			switch (Layout.Frequency)
			{
				case PacketFrequency.Low:
					// Set the low frequency identifier bits
					byte[] lowHeader = {0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF};
					Array.Copy(lowHeader, 0, Data, 0, 6);
					// Store the packet ID in big endian format
					short id = IPAddress.HostToNetworkOrder((short)Layout.ID);
					Array.Copy(BitConverter.GetBytes(id), 0, Data, 6, 2);
					break;
				case PacketFrequency.Medium:
					// Set the medium frequency identifier bit
					byte[] mediumHeader = {0x00, 0x00, 0x00, 0x00, 0xFF};
					Array.Copy(mediumHeader, 0, Data, 0, 5);
					Data[5] = (byte)Layout.ID;
					break;
				case PacketFrequency.High:
					byte[] highHeader = {0x00, 0x00, 0x00, 0x00};
					Array.Copy(highHeader, 0, Data, 0, 4);
					Data[4] = (byte)Layout.ID;
					break;
			}
		}

		public Packet(byte[] data, int length, ProtocolManager protocol)
		{
			Protocol = protocol;
			ushort command;
			Data = new byte[length];

			if (length < 5)
			{
				Helpers.Log("Received a packet with less than 5 bytes", Helpers.LogLevel.Warning);
				
				// Create an empty MapPacket
				Layout = new MapPacket();
				Layout.Blocks = new ArrayList();

				return;
			}

			if (data[4] == 0xFF)
			{
				if ((byte)data[5] == 0xFF)
				{
					// Low frequency
					command = BitConverter.ToUInt16(data, 6);
					command = (ushort)IPAddress.NetworkToHostOrder((short)command);

					Layout = protocol.Command(command, PacketFrequency.Low);
				}
				else
				{
					// Medium frequency
					command = (ushort)data[5];
					Layout = protocol.Command(command, PacketFrequency.Medium);
				}
			}
			else
			{
				// High frequency
				command = (ushort)data[4];
				Layout = protocol.Command(command, PacketFrequency.High);
			}

			if (Layout == null)
			{
				Helpers.Log("Received a packet with an unknown command ID", Helpers.LogLevel.Warning);

				// Create an empty MapPacket
				Layout = new MapPacket();
				Layout.Blocks = new ArrayList();
			}

			// Copy the network byte array to this packet's byte array
			Array.Copy(data, 0, Data, 0, length);
		}

		public ArrayList Blocks()
		{
			Field field;
			Block block;
			byte blockCount;
			int fieldSize;

			// Get the starting position of the SL payload (different than the UDP payload)
			int pos = HeaderLength();

			// Initialize the block list we are returning
			ArrayList blocks = new ArrayList();

			foreach (MapBlock blockMap in Layout.Blocks)
			{
				if (blockMap.Count == -1)
				{
					// Variable count block
					if (pos < Data.Length)
					{
						blockCount = Data[pos];
						pos++;
					}
					else
					{
						Helpers.Log("getBlocks(): goto 1 reached", Helpers.LogLevel.Warning);
						goto Done;
					}
				}
				else
				{
					blockCount = (byte)blockMap.Count;
				}

				for (int i = 0; i < blockCount; ++i)
				{
					// Create a new block to push back on the list
					block = new Block();
					block.Layout = blockMap;
					block.Fields = new ArrayList();

					foreach (MapField fieldMap in blockMap.Fields)
					{
						if (fieldMap.Type == FieldType.Variable)
						{
							if (fieldMap.Count == 1)
							{
								// Field length described with one byte
								if (pos < Data.Length)
								{
									fieldSize = (ushort)Data[pos];
									pos++;
								}
								else
								{
									Helpers.Log("getBlocks(): goto 2 reached", Helpers.LogLevel.Warning);
									goto BlockDone;
								}
							}
							else // (fieldMap.Count == 2)
							{
								// Field length described with two bytes
								if (pos + 1 < Data.Length)
								{
									fieldSize = BitConverter.ToUInt16(Data, pos);
									pos += 2;
								}
								else
								{
									Helpers.Log("getBlocks(): goto 3 reached", Helpers.LogLevel.Warning);
									goto BlockDone;
								}
							}

							if (fieldSize != 0)
							{
								if (pos + fieldSize <= Data.Length)
								{
									// Create a new field to add to the fields for this block
									field = new Field();
									field.Data = GetField(Data, pos, fieldMap.Type, fieldSize);
									field.Layout = fieldMap;

									block.Fields.Add(field);

									pos += fieldSize;
								}
								else
								{
									Helpers.Log("getBlocks(): goto 4 reached", Helpers.LogLevel.Warning);
									goto BlockDone;
								}
							}
						}
						else if (fieldMap.Type == FieldType.Fixed)
						{
							fieldSize = fieldMap.Count;

							if (pos + fieldSize <= Data.Length)
							{
								// Create a new field to add to the fields for this block
								field = new Field();
								field.Data = GetField(Data, pos, fieldMap.Type, fieldSize);
								field.Layout = fieldMap;

								block.Fields.Add(field);

								pos += fieldSize;
							}
							else
							{
								Helpers.Log("getBlocks(): goto 4 reached", Helpers.LogLevel.Warning);
								goto BlockDone;
							}
						}
						else
						{
							for (int j = 0; j < fieldMap.Count; ++j)
							{
								fieldSize = (int)Protocol.TypeSizes[fieldMap.Type];

								if (pos + fieldSize <= Data.Length)
								{
									// Create a new field to add to the fields for this block
									field = new Field();
									field.Data = GetField(Data, pos, fieldMap.Type, fieldSize);
									field.Layout = fieldMap;

									block.Fields.Add(field);

									pos += fieldSize;
								}
								else
								{
									Helpers.Log("getBlocks(): goto 5 reached", Helpers.LogLevel.Warning);
									goto BlockDone;
								}
							}
						}
					}

				BlockDone:
					blocks.Add(block);
				}
			}

		Done:
			return blocks;
		}

		public override string ToString()
		{
			string output = "";
			ArrayList blocks = Blocks();
			
			output += "---- " + Layout.Name + " ----\n";

			foreach (Block block in blocks)
			{
				output += block.Layout.Name + "\n";

				foreach (Field field in block.Fields)
				{
					output += " " + field.Layout.Name + ": " + field.Data.ToString() + "\n";
				}
			}

			return output;
		}

		private object GetField(byte[] byteArray, int pos, FieldType type, int fieldSize)
		{
			switch (type)
			{
				case FieldType.U8:
					return byteArray[pos];
				case FieldType.U16:
					return BitConverter.ToUInt16(byteArray, pos);
				case FieldType.U32:
					return BitConverter.ToUInt32(byteArray, pos);
				case FieldType.U64:
					return BitConverter.ToUInt64(byteArray, pos);
				case FieldType.S8:
					return (sbyte)byteArray[pos];
				case FieldType.S16:
					return BitConverter.ToInt16(byteArray, pos);
				case FieldType.S32:
					return BitConverter.ToInt32(byteArray, pos);
				case FieldType.S64:
					return BitConverter.ToInt64(byteArray, pos);
				case FieldType.F32:
					return BitConverter.ToSingle(byteArray, pos);
				case FieldType.F64:
					return BitConverter.ToDouble(byteArray, pos);
				case FieldType.LLUUID:
					return new LLUUID(byteArray, pos);
				case FieldType.BOOL:
					return (byteArray[pos] != 0) ? (bool)true : (bool)false;
				case FieldType.LLVector3:
					return new LLVector3(byteArray, pos);
				case FieldType.LLVector3d:
					return new LLVector3d(byteArray, pos);
				case FieldType.LLVector4:
					return new LLVector4(byteArray, pos);
				case FieldType.LLQuaternion:
					return new LLQuaternion(byteArray, pos);
				case FieldType.IPADDR:
					return new IPAddress(BitConverter.ToInt32(byteArray, pos));
				case FieldType.IPPORT:
					return BitConverter.ToUInt16(byteArray, pos);
				case FieldType.Variable:
				case FieldType.Fixed:
					byte[] bytes = new byte[fieldSize];

					for (int i = 0; i < fieldSize; ++i)
					{
						bytes[i] = byteArray[pos + i];
					}

					return bytes;
			}

			return null;
		}

		public int HeaderLength()
		{
			switch (Layout.Frequency)
			{
				case PacketFrequency.High:
					return 5;
				case PacketFrequency.Medium:
					return 6;
				case PacketFrequency.Low:
					return 8;
			}

			return 0;
		}
	}

	public class PacketBuilder
	{
		public static Packet UseCircuitCode(ProtocolManager protocol, LLUUID agentID, LLUUID sessionID, uint code)
		{
			Packet packet = new Packet("UseCircuitCode", protocol, 44);

			// Append the payload
			Array.Copy(agentID.Data, 0, packet.Data, 8, 16);
			Array.Copy(sessionID.Data, 0, packet.Data, 24, 16);
			Array.Copy(BitConverter.GetBytes(code), 0, packet.Data, 40, 4);

			// Set the packet flags
			packet.Data[0] = Helpers.MSG_RELIABLE;

			return packet;
		}

		public static Packet CompleteAgentMovement(ProtocolManager protocol, LLUUID agentID, LLUUID sessionID, uint code)
		{
			Packet packet = new Packet("CompleteAgentMovement", protocol, 44);

			// Append the payload
			Array.Copy(agentID.Data, 0, packet.Data, 8, 16);
			Array.Copy(sessionID.Data, 0, packet.Data, 24, 16);
			Array.Copy(BitConverter.GetBytes(code), 0, packet.Data, 40, 4);

			// Set the packet flags
			packet.Data[0] = Helpers.MSG_RELIABLE;

			return packet;
		}
		
		public static Packet PacketAck(ProtocolManager protocol, ArrayList acks)
		{
			int pos = 9;

			// Size of this packet is header (8) + block count (1) + 4 * number of blocks
			Packet packet = new Packet("PacketAck", protocol, 9 + acks.Count * 4);

			// Set the payload size
			packet.Data[8] = (byte)acks.Count;

			// Append the payload
			foreach (uint ack in acks)
			{
				Array.Copy(BitConverter.GetBytes(ack), 0, packet.Data, pos, 4);
				pos += 4;
			}

			return packet;
		}

		public static Packet CompletePingCheck(ProtocolManager protocol, byte pingID)
		{
			Packet packet = new Packet("CompletePingCheck", protocol, 6);

			// Append the payload
			packet.Data[5] = pingID;

			// Set the packet flags
			packet.Data[0] = Helpers.MSG_ZEROCODED;

			return packet;
		}

		public static Packet LogoutRequest(ProtocolManager protocol, LLUUID agentID, LLUUID sessionID)
		{
			Packet packet = new Packet("LogoutRequest", protocol, 40);

			Array.Copy(agentID.Data, 0, packet.Data, 8, 16);
			Array.Copy(sessionID.Data, 0, packet.Data, 24, 16);

			packet.Data[0] = Helpers.MSG_RELIABLE + Helpers.MSG_ZEROCODED;

			return packet;
		}

		public static Packet TeleportLocationRequest(ProtocolManager protocol, long regionHandle, LLVector3 lookAt,
			LLVector3 position, LLUUID agentID, LLUUID sessionID)
		{
			Packet packet = new Packet("LogoutRequest", protocol, 72);

			Array.Copy(BitConverter.GetBytes(regionHandle), 0, packet.Data, 8, 8);
			Array.Copy(lookAt.GetBytes(), 0, packet.Data, 16, 12);
			Array.Copy(position.GetBytes(), 0, packet.Data, 28, 12);
			Array.Copy(agentID.Data, 0, packet.Data, 40, 16);
			Array.Copy(sessionID.Data, 0, packet.Data, 56, 16);

			packet.Data[0] = Helpers.MSG_RELIABLE;

			return packet;
		}

		public static Packet DirLandQuery(ProtocolManager protocol, bool reservedNewbie, bool forSale, 
			LLUUID queryID, bool auction, uint queryFlags, LLUUID agentID, LLUUID sessionID)
		{
			Packet packet = new Packet("DirLandQuery", protocol, 63);

			packet.Data[8] = BitConverter.GetBytes(reservedNewbie)[0];
			packet.Data[9] = BitConverter.GetBytes(forSale)[0];
			Array.Copy(queryID.Data, 0, packet.Data, 10, 16);
			packet.Data[26] = BitConverter.GetBytes(auction)[0];
			Array.Copy(BitConverter.GetBytes(queryFlags), 0, packet.Data, 27, 4);
			Array.Copy(agentID.Data, 0, packet.Data, 31, 16);
			Array.Copy(sessionID.Data, 0, packet.Data, 47, 16);

			// Set the packet flags
			packet.Data[0] = Helpers.MSG_RELIABLE;

			return packet;
		}

		public static Packet DirFindQuery(ProtocolManager protocol, string queryText, LLUUID queryID,
			LLUUID agentID, LLUUID sessionID)
		{
			int pos = 8;
			Packet packet = new Packet("DirFindQuery", protocol, 66 + queryText.Length);

			// QueryID
			Array.Copy(queryID.Data, 0, packet.Data, pos, 16);
			pos += 16;

			// QueryFlags
			packet.Data[pos] = 1;
			pos += 8;

			// Set the QueryText field and it's length byte
			packet.Data[pos] = (byte)(queryText.Length + 1);
			pos++;
			System.Text.Encoding.UTF8.GetBytes(queryText, 0, queryText.Length, packet.Data, pos);
			pos += queryText.Length + 1;

			// Set the AgentID and SessionID
			Array.Copy(agentID.Data, 0, packet.Data, pos, 16);
			pos += 16;
			Array.Copy(sessionID.Data, 0, packet.Data, pos, 16);

			// Set the packet flags
			packet.Data[0] = Helpers.MSG_ZEROCODED + Helpers.MSG_RELIABLE;

			return packet;
		}

		public static Packet DirPeopleQuerySimple(ProtocolManager protocol, string name, bool online, 
			LLUUID agentID, LLUUID sessionID)
		{
			int pos = 12;
			Packet packet = new Packet("DirPeopleQuery", protocol, 71 + name.Length);

			// Set the name field and it's length byte
			packet.Data[pos] = (byte)(name.Length + 1);
			pos++;
			System.Text.Encoding.UTF8.GetBytes(name, 0, name.Length, packet.Data, pos);
			pos += name.Length + 1;

			// Set the QueryID to 1
			pos += 15;
			packet.Data[pos] = 1;
			pos ++;

			// Set the online field
			packet.Data[pos] = BitConverter.GetBytes(online)[0];
			pos++;

			// Skip some fields
			pos += 8;

			// Set the AgentID and SessionID
			Array.Copy(agentID.Data, 0, packet.Data, pos, 16);
			pos += 16;
			Array.Copy(sessionID.Data, 0, packet.Data, pos, 16);

			// Set the packet flags
			packet.Data[0] = Helpers.MSG_ZEROCODED + Helpers.MSG_RELIABLE;

			return packet;
		}

		public static Packet ParcelInfoRequest(ProtocolManager protocol, LLUUID parcelID, LLUUID agentID, 
			LLUUID sessionID)
		{
			Packet packet = new Packet("ParcelInfoRequest", protocol, 56);
			
			Array.Copy(parcelID.Data, 0, packet.Data, 8, 16);
			Array.Copy(agentID.Data, 0, packet.Data, 24, 16);
			Array.Copy(sessionID.Data, 0, packet.Data, 40, 16);

			packet.Data[0] = Helpers.MSG_RELIABLE + Helpers.MSG_ZEROCODED;

			return packet;
		}

		public static Packet ObjectAddSimple(ProtocolManager protocol, PrimObject objectData, LLUUID senderID, 
			LLVector3 position, LLVector3 rayStart)
		{
			LLUUID woodTexture = new LLUUID("8955674724cb43ed920b47caed15465f");
			LLUUID rayTargetID = new LLUUID("0f5d10f1f0a38634e893b70e00000000");
			int length = 6 + 60 + 2 + objectData.NameValue.Length + 1 + 36 + 2 + 40 + 29;
			Packet packet = new Packet("ObjectAdd", protocol, length);
			int pos = 6;

			// InventoryData appears 0 times
			packet.Data[pos] = 0;
			pos++;

			// InventoryFile.Filename is of 1 length
			packet.Data[pos] = 1;
			pos++;

			// InventoryFile.Filename is just a null terminator
			packet.Data[pos] = 0;
			pos++;

			// U32 AddFlags = 2
			uint addFlags = 2;
			Array.Copy(BitConverter.GetBytes(addFlags), 0, packet.Data, pos, 4);
			pos += 4;

			packet.Data[pos] = (byte)objectData.PathTwistBegin;
			pos++;

			packet.Data[pos] = objectData.PathEnd;
			pos++;

			packet.Data[pos] = objectData.ProfileBegin;
			pos++;

			packet.Data[pos] = (byte)objectData.PathRadiusOffset;
			pos++;

			packet.Data[pos] = (byte)objectData.PathSkew;
			pos++;

			// SenderID
			Array.Copy(senderID.Data, 0, packet.Data, pos, 16);
			pos += 16;

			// RayStart
			Array.Copy(rayStart.GetBytes(), 0, packet.Data, pos, 12);
			pos += 12;

			packet.Data[pos] = objectData.ProfileCurve;
			pos++;

			packet.Data[pos] = objectData.PathScaleX;
			pos++;

			packet.Data[pos] = objectData.PathScaleY;
			pos++;

			// Set GroupID to zero
			pos += 16;

			packet.Data[pos] = objectData.Material;
			pos++;

			if (objectData.NameValue.Length != 0)
			{
				// NameValue, begins with two bytes describing the size
				Array.Copy(BitConverter.GetBytes((ushort)(objectData.NameValue.Length + 1)), 0, packet.Data, pos, 2);
				pos += 2;
				System.Text.Encoding.UTF8.GetBytes(objectData.NameValue, 0, objectData.NameValue.Length, packet.Data, pos);
				// Jump an extra spot for the null terminator
				pos += objectData.NameValue.Length + 1;
			}
			else
			{
				// Set the two size bytes to zero and increment
				pos += 2;
			}

			packet.Data[pos] = objectData.PathShearX;
			pos++;

			packet.Data[pos] = objectData.PathShearY;
			pos++;

			packet.Data[pos] = (byte)objectData.PathTaperX;
			pos++;

			packet.Data[pos] = (byte)objectData.PathTaperY;
			pos++;

			// RayEndIsIntersection
			packet.Data[pos] = 0;
			pos++;

			// RayEnd is the position to place the object
			Array.Copy(position.GetBytes(), 0, packet.Data, pos, 12);
			pos += 12;

			packet.Data[pos] = objectData.ProfileEnd;
			pos++;

			packet.Data[pos] = objectData.PathBegin;
			pos++;

			// BypassRaycast is 0
			packet.Data[pos] = 0;
			pos++;

			// PCode? set to 9
			packet.Data[pos] = 9;
			pos++;

			packet.Data[pos] = objectData.PathCurve;
			pos++;

			// Scale
			Array.Copy(objectData.Scale.GetBytes(), 0, packet.Data, pos, 12);
			pos += 12;

			// State? is 0
			packet.Data[pos] = 0;
			pos++;

			packet.Data[pos] = (byte)objectData.PathTwist;
			pos++;

			// TextureEntry, starts with two bytes describing the size
			Array.Copy(BitConverter.GetBytes((ushort)40), 0, packet.Data, pos, 2);
			pos += 2;
			Array.Copy(woodTexture.Data, 0, packet.Data, pos, 16);
			pos += 16;
			// Fill in the rest of TextureEntry
			pos += 19;
			packet.Data[pos] = 0xe0;
			pos += 5;

			packet.Data[pos] = objectData.ProfileHollow;
			pos++;

			packet.Data[pos] = objectData.PathRevolutions;
			pos++;

			// Rotation
			Array.Copy(objectData.Rotation.GetBytes(), 0, packet.Data, pos, 16);
			pos += 16;

			// RayTargetID
			Array.Copy(rayTargetID.Data, 0, packet.Data, pos, 12);
			pos += 12;
			
			// Set the packet flags
			packet.Data[0] = /*Helpers.MSG_ZEROCODED +*/ Helpers.MSG_RELIABLE;

			return packet;
		}
	}
}

/* Generic packet builder idea:
 *  - Pass in an ArrayList (blocks) of Hashtables (fields)
 *  - Iterate through the packet layout and add TypeSizes of each field * fieldCount to the total
 *    - Serialize the actual values to a temporary byte array
 *  - Each variable field, find the length of the actual values and add 1 or 2
 *  - Copy the temporary array to the packet array after initialization
 */
