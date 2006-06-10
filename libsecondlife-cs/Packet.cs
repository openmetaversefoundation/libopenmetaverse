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
						for (int j = 0; j < fieldMap.Count; ++j)
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
										goto Done;
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
										goto Done;
									}
								}
							}
							else
							{
								fieldSize = (int)Protocol.TypeSizes[fieldMap.Type];
							}

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
								goto Done;
							}
						}
					}

					blocks.Add(block);
				}
			}

			Done:
				return blocks;
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
					return (char)byteArray[pos];
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

		public static Packet CompletePingCheck(ProtocolManager protocol, byte PingID)
		{
			Packet packet = new Packet("CompletePingCheck", protocol, 6);

			// Append the payload
			packet.Data[5] = PingID;

			// Set the packet flags
			packet.Data[0] = Helpers.MSG_ZEROCODED;

			return packet;
		}
	}
}
