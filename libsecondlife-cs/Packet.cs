using System;
using System.Net;
using System.Collections;

namespace libsecondlife
{
	public struct Block
	{
		public ArrayList Fields;
		public MapBlock Layout;
	}

	public class Packet
	{
		public ArrayList Data;
		public MapPacket Layout;

		private ProtocolManager Protocol;

		public Packet(string command, ProtocolManager protocol)
		{
			Protocol = protocol;
			Data = new ArrayList();
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
					Data.AddRange(lowHeader);
					// Store the packet ID in big endian format
					short id = IPAddress.HostToNetworkOrder((short)Layout.ID);
					Data.AddRange(BitConverter.GetBytes(id));
					break;
				case PacketFrequency.Medium:
					// Set the medium frequency identifier bit
					byte[] mediumHeader = {0x00, 0x00, 0x00, 0x00, 0xFF};
					Data.AddRange(mediumHeader);
					Data.Add((byte)Layout.ID);
					break;
				case PacketFrequency.High:
					byte[] highHeader = {0x00, 0x00, 0x00, 0x00};
					Data.AddRange(highHeader);
					Data.Add((byte)Layout.ID);
					break;
			}
		}

		public Packet(byte[] data, int length, ProtocolManager protocol)
		{
			Protocol = protocol;
			ushort command;
			Data = new ArrayList();

			if (length < 5)
			{
				Helpers.Log("Received a packet with less than 5 bytes", Helpers.LogLevel.Warning);
				Layout = null;
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
					command = BitConverter.ToUInt16(data, 5);
					command = (ushort)IPAddress.NetworkToHostOrder((short)command);
					Layout = protocol.Command(command, PacketFrequency.Medium);
				}
			}
			else
			{
				// High frequency
				command = BitConverter.ToUInt16(data, 4);
				command = (ushort)IPAddress.NetworkToHostOrder((short)command);
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
			for (int i = 0; i < length; ++i)
			{
				Data.Add(data[i]);
			}
		}

		public ArrayList Blocks()
		{
			Block block;
			byte blockCount;
			ushort fieldSize;

			// Create a temporary byte array for quicker conversions
			byte[] byteArray = (byte[])Data.ToArray(typeof(Byte));

			// Get the starting position of the SL payload (different than the UDP payload)
			int pos = HeaderLength();

			// Initialize the block list we are returning
			ArrayList blocks = new ArrayList();

			foreach (MapBlock blockMap in Layout.Blocks)
			{
				if (blockMap.Count == -1)
				{
					// Variable count block
					if (pos < byteArray.Length)
					{
						blockCount = (byte)byteArray[pos];
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
									if (pos < byteArray.Length)
									{
										fieldSize = (ushort)byteArray[pos];
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
									if (pos + 1 < byteArray.Length)
									{
										fieldSize = BitConverter.ToUInt16(byteArray, pos);
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
								fieldSize = (ushort)Protocol.TypeSizes[fieldMap.Type];
							}

							if (pos + fieldSize <= byteArray.Length)
							{
								block.Fields.Add(GetField(byteArray, pos, fieldMap.Type));

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

		private object GetField(byte[] byteArray, int pos, FieldType type)
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
					return BitConverter.ToString(byteArray, pos);
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
			Packet packet = new Packet("UseCircuitCode", protocol);

			// Append the payload
			packet.Data.AddRange(agentID.Data);
			packet.Data.AddRange(sessionID.Data);
			packet.Data.AddRange(BitConverter.GetBytes(code));

			// Set the packet flags
			packet.Data[0] = Helpers.MSG_RELIABLE;

			return packet;
		}
	}
}
