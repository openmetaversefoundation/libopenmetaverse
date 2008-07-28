/*
 * Copyright (c) 2007-2008, openmetaverse.org
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
using System.Collections.Generic;
using System.IO;

namespace OpenMetaverse
{
    /// <summary>
    /// 
    /// </summary>
	public enum PacketFrequency
	{
        /// <summary></summary>
		Low,
        /// <summary></summary>
		Medium,
        /// <summary></summary>
		High
	}

    /// <summary>
    /// 
    /// </summary>
	public enum FieldType
	{
        /// <summary></summary>
		U8,
        /// <summary></summary>
		U16,
        /// <summary></summary>
		U32,
        /// <summary></summary>
		U64,
        /// <summary></summary>
		S8,
        /// <summary></summary>
		S16,
        /// <summary></summary>
		S32,
        /// <summary></summary>
		F32,
        /// <summary></summary>
		F64,
        /// <summary></summary>
		UUID,
        /// <summary></summary>
		BOOL,
        /// <summary></summary>
		Vector3,
        /// <summary></summary>
		Vector3d,
        /// <summary></summary>
		Vector4,
        /// <summary></summary>
		Quaternion,
        /// <summary></summary>
		IPADDR,
        /// <summary></summary>
		IPPORT,
        /// <summary></summary>
		Variable,
        /// <summary></summary>
		Fixed,
        /// <summary></summary>
		Single,
        /// <summary></summary>
		Multiple
	}

    /// <summary>
    /// 
    /// </summary>
	public class MapField : IComparable
	{
        /// <summary></summary>
		public int KeywordPosition;
        /// <summary></summary>
		public string Name;
        /// <summary></summary>
		public FieldType Type;
        /// <summary></summary>
		public int Count;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
		public int CompareTo(object obj)
		{
			MapField temp = (MapField)obj;

			if (this.KeywordPosition > temp.KeywordPosition)
			{
				return 1;
			}
			else
			{
				if(temp.KeywordPosition == this.KeywordPosition)
				{
					return 0;
				}
				else
				{
					return -1;
				}
			}
		}
	}

    /// <summary>
    /// 
    /// </summary>
	public class MapBlock : IComparable
	{
        /// <summary></summary>
		public int KeywordPosition;
        /// <summary></summary>
		public string Name;
        /// <summary></summary>
		public int Count;
        /// <summary></summary>
		public List<MapField> Fields;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
		public int CompareTo(object obj)
		{
			MapBlock temp = (MapBlock)obj;

			if (this.KeywordPosition > temp.KeywordPosition)
			{
				return 1;
			}
			else
			{
				if(temp.KeywordPosition == this.KeywordPosition)
				{
					return 0;
				}
				else
				{
					return -1;
				}
			}
		}
	}

    /// <summary>
    /// 
    /// </summary>
	public class MapPacket
	{
        /// <summary></summary>
		public ushort ID;
        /// <summary></summary>
		public string Name;
        /// <summary></summary>
		public PacketFrequency Frequency;
        /// <summary></summary>
		public bool Trusted;
        /// <summary></summary>
		public bool Encoded;
        /// <summary></summary>
		public List<MapBlock> Blocks;
	}

    /// <summary>
    /// 
    /// </summary>
	public class ProtocolManager
	{
        /// <summary></summary>
		public Dictionary<FieldType, int> TypeSizes;
        /// <summary></summary>
		public Dictionary<string, int> KeywordPositions;
        /// <summary></summary>
		public MapPacket[] LowMaps;
        /// <summary></summary>
		public MapPacket[] MediumMaps;
        /// <summary></summary>
		public MapPacket[] HighMaps;

        private GridClient Client;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapFile"></param>
        /// <param name="client"></param>
		public ProtocolManager(string mapFile, GridClient client)
		{
            Client = client;

			// Initialize the map arrays
			LowMaps = new MapPacket[65536];
			MediumMaps = new MapPacket[256];
			HighMaps = new MapPacket[256];

			// Build the type size hash table
			TypeSizes = new Dictionary<FieldType,int>();
			TypeSizes.Add(FieldType.U8, 1);
			TypeSizes.Add(FieldType.U16, 2);
			TypeSizes.Add(FieldType.U32, 4);
			TypeSizes.Add(FieldType.U64, 8);
			TypeSizes.Add(FieldType.S8, 1);
			TypeSizes.Add(FieldType.S16, 2);
			TypeSizes.Add(FieldType.S32, 4);
			TypeSizes.Add(FieldType.F32, 4);
			TypeSizes.Add(FieldType.F64, 8);
			TypeSizes.Add(FieldType.UUID, 16);
			TypeSizes.Add(FieldType.BOOL, 1);
			TypeSizes.Add(FieldType.Vector3, 12);
			TypeSizes.Add(FieldType.Vector3d, 24);
			TypeSizes.Add(FieldType.Vector4, 16);
			TypeSizes.Add(FieldType.Quaternion, 16);
			TypeSizes.Add(FieldType.IPADDR, 4);
			TypeSizes.Add(FieldType.IPPORT, 2);
			TypeSizes.Add(FieldType.Variable, -1);
			TypeSizes.Add(FieldType.Fixed, -2);

            KeywordPositions = new Dictionary<string, int>();
			LoadMapFile(mapFile);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
		public MapPacket Command(string command)
		{
			foreach (MapPacket map in HighMaps)
			{
				if (map != null)
				{
					if (command == map.Name)
					{
						return map;
					}
				}
			}

			foreach (MapPacket map in MediumMaps)
			{
				if (map != null)
				{
					if (command == map.Name)
					{
						return map;
					}
				}
			}

			foreach (MapPacket map in LowMaps)
			{
				if (map != null)
				{
					if (command == map.Name)
					{
						return map;
					}
				}
			}

			throw new Exception("Cannot find map for command \"" + command + "\"");
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
		public MapPacket Command(byte[] data)
		{
			ushort command;

			if (data.Length < 5)
			{
				return null;
			}

			if (data[4] == 0xFF)
			{
				if ((byte)data[5] == 0xFF)
				{
					// Low frequency
					command = (ushort)(data[6] * 256 + data[7]);
					return Command(command, PacketFrequency.Low);
				}
				else
				{
					// Medium frequency
					command = (ushort)data[5];
					return Command(command, PacketFrequency.Medium);
				}
			}
			else
			{
				// High frequency
				command = (ushort)data[4];
				return Command(command, PacketFrequency.High);
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
		public MapPacket Command(ushort command, PacketFrequency frequency)
		{
			switch (frequency)
			{
				case PacketFrequency.High:
					return HighMaps[command];
				case PacketFrequency.Medium:
					return MediumMaps[command];
				case PacketFrequency.Low:
					return LowMaps[command];
			}

			throw new Exception("Cannot find map for command \"" + command + "\" with frequency \"" + frequency + "\"");
		}

        /// <summary>
        /// 
        /// </summary>
		public void PrintMap()
		{
			PrintOneMap(LowMaps,    "Low   ");
			PrintOneMap(MediumMaps, "Medium");
			PrintOneMap(HighMaps,   "High  ");
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="map"></param>
        /// <param name="frequency"></param>
		private void PrintOneMap(MapPacket[] map, string frequency) {
			int i;

			for (i = 0; i < map.Length; ++i)
			{
				if (map[i] != null)
				{
					Console.WriteLine("{0} {1,5} - {2} - {3} - {4}", frequency, i, map[i].Name,
						map[i].Trusted ? "Trusted" : "Untrusted",
						map[i].Encoded ? "Unencoded" : "Zerocoded");

					foreach (MapBlock block in map[i].Blocks)
					{
						if (block.Count == -1) 
						{
							Console.WriteLine("\t{0,4} {1} (Variable)", block.KeywordPosition, block.Name);
						} 
						else 
						{
							Console.WriteLine("\t{0,4} {1} ({2})", block.KeywordPosition, block.Name, block.Count);
						}

						foreach (MapField field in block.Fields)
						{
							Console.WriteLine("\t\t{0,4} {1} ({2} / {3})", field.KeywordPosition, field.Name,
								field.Type, field.Count);
						}
					}
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapFile"></param>
        /// <param name="outputFile"></param>
		public static void DecodeMapFile(string mapFile, string outputFile)
		{
			byte magicKey = 0;
			byte[] buffer = new byte[2048];
			int nread;
			BinaryReader map;
			BinaryWriter output;

			try
			{
				map = new BinaryReader(new FileStream(mapFile, FileMode.Open));
			}
			catch(Exception e)
			{
				throw new Exception("Map file error", e);
			}

			try
			{
				output = new BinaryWriter(new FileStream(outputFile, FileMode.CreateNew));
			}
			catch(Exception e)
			{
				throw new Exception("Map file error", e);
			}

			while ((nread = map.Read(buffer, 0, 2048)) != 0)
			{
				for (int i = 0; i < nread; ++i)
				{
					buffer[i] ^= magicKey;
					magicKey += 43;
				}

				output.Write(buffer, 0, nread);
			}

			map.Close();
			output.Close();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapFile"></param>
		private void LoadMapFile(string mapFile)
		{
			FileStream map;
			ushort low = 1;
			ushort medium = 1;
			ushort high = 1;

			// Load the protocol map file
			try
			{
				map = new FileStream(mapFile, FileMode.Open, FileAccess.Read); 
			}
			catch(Exception e) 
			{
				throw new Exception("Map file loading error", e);
			}

			try
			{
				StreamReader r = new StreamReader(map);        
				r.BaseStream.Seek(0, SeekOrigin.Begin);
				string newline;
				string trimmedline;
				bool inPacket = false;
				bool inBlock = false;
				MapPacket currentPacket = null;
				MapBlock currentBlock = null;
				char[] trimArray = new char[] {' ', '\t'};

				// While not at the end of the file
				while (r.Peek() > -1) 
				{
					#region ParseMap

					newline = r.ReadLine();
					trimmedline = System.Text.RegularExpressions.Regex.Replace(newline, @"\s+", " ");
					trimmedline = trimmedline.Trim(trimArray);

					if (!inPacket)
					{
						// Outside of all packet blocks

						if (trimmedline == "{")
						{
							inPacket = true;
						}
					}
					else
					{
						// Inside of a packet block

						if (!inBlock)
						{
							// Inside a packet block, outside of the blocks

							if (trimmedline == "{")
							{
								inBlock = true;
							}
							else if (trimmedline == "}")
							{
								// Reached the end of the packet
								currentPacket.Blocks.Sort();
								inPacket = false;
							}
							else 
							{
								// The packet header
								#region ParsePacketHeader

								// Splice the string in to tokens
								string[] tokens = trimmedline.Split(new char[] {' ', '\t'});

								if (tokens.Length > 3)
								{
                                    //Hash packet name to insure correct keyword ordering
                                    KeywordPosition(tokens[0]);

									if (tokens[1] == "Fixed")
									{
										// Remove the leading "0x"
										if (tokens[2].Substring(0, 2) == "0x")
										{
											tokens[2] = tokens[2].Substring(2, tokens[2].Length - 2);
										}

										uint fixedID = UInt32.Parse(tokens[2], System.Globalization.NumberStyles.HexNumber);
										// Truncate the id to a short
										fixedID ^= 0xFFFF0000;
										LowMaps[fixedID] = new MapPacket();
										LowMaps[fixedID].ID = (ushort)fixedID;
										LowMaps[fixedID].Frequency = PacketFrequency.Low;
										LowMaps[fixedID].Name = tokens[0];
										LowMaps[fixedID].Trusted = (tokens[3] == "Trusted");
										LowMaps[fixedID].Encoded = (tokens[4] == "Zerocoded");
										LowMaps[fixedID].Blocks = new List<MapBlock>();

										currentPacket = LowMaps[fixedID];
									}
									else if (tokens[1] == "Low")
									{
										LowMaps[low] = new MapPacket();
										LowMaps[low].ID = low;
										LowMaps[low].Frequency = PacketFrequency.Low;
										LowMaps[low].Name = tokens[0];
										LowMaps[low].Trusted = (tokens[2] == "Trusted");
										LowMaps[low].Encoded = (tokens[3] == "Zerocoded");
										LowMaps[low].Blocks = new List<MapBlock>();

										currentPacket = LowMaps[low];

										low++;
									}
									else if (tokens[1] == "Medium")
									{
										MediumMaps[medium] = new MapPacket();
										MediumMaps[medium].ID = medium;
										MediumMaps[medium].Frequency = PacketFrequency.Medium;
										MediumMaps[medium].Name = tokens[0];
										MediumMaps[medium].Trusted = (tokens[2] == "Trusted");
										MediumMaps[medium].Encoded = (tokens[3] == "Zerocoded");
										MediumMaps[medium].Blocks = new List<MapBlock>();

										currentPacket = MediumMaps[medium];

										medium++;
									}
									else if (tokens[1] == "High")
									{
										HighMaps[high] = new MapPacket();
										HighMaps[high].ID = high;
										HighMaps[high].Frequency = PacketFrequency.High;
										HighMaps[high].Name = tokens[0];
										HighMaps[high].Trusted = (tokens[2] == "Trusted");
										HighMaps[high].Encoded = (tokens[3] == "Zerocoded");
										HighMaps[high].Blocks = new List<MapBlock>();

										currentPacket = HighMaps[high];

										high++;
									}
									else
									{
										Logger.Log("Unknown packet frequency", Helpers.LogLevel.Error, Client);
									}
								}

								#endregion
							}
						}
						else
						{
							if (trimmedline.Length > 0 && trimmedline.Substring(0, 1) == "{")
							{
								// A field
								#region ParseField

								MapField field = new MapField();

								// Splice the string in to tokens
								string[] tokens = trimmedline.Split(new char[] {' ', '\t'});

								field.Name = tokens[1];
								field.KeywordPosition = KeywordPosition(field.Name);
								field.Type = (FieldType)Enum.Parse(typeof(FieldType), tokens[2], true);

								if (tokens[3] != "}")
								{
									field.Count = Int32.Parse(tokens[3]);
								}
								else
								{
									field.Count = 1;
								}

								// Save this field to the current block
								currentBlock.Fields.Add(field);

								#endregion
							}
							else if (trimmedline == "}")
							{
								currentBlock.Fields.Sort();
								inBlock = false;
							}
							else if (trimmedline.Length != 0 && trimmedline.Substring(0, 2) != "//")
							{
								// The block header
								#region ParseBlockHeader

								currentBlock = new MapBlock();

								// Splice the string in to tokens
								string[] tokens = trimmedline.Split(new char[] {' ', '\t'});

								currentBlock.Name = tokens[0];
								currentBlock.KeywordPosition = KeywordPosition(currentBlock.Name);
								currentBlock.Fields = new List<MapField>();
								currentPacket.Blocks.Add(currentBlock);

								if (tokens[1] == "Single")
								{
									currentBlock.Count = 1;
								}
								else if (tokens[1] == "Multiple")
								{
									currentBlock.Count = Int32.Parse(tokens[2]);
								}
								else if (tokens[1] == "Variable")
								{
									currentBlock.Count = -1;
								}
								else
								{
									Logger.Log("Unknown block frequency", Helpers.LogLevel.Error, Client);
								}

								#endregion
							}
						}
					}

					#endregion
				}

				r.Close();
				map.Close();
			}
			catch (Exception e)
			{
                throw new Exception("Map file parsing error", e); ;
			}
		}

		private int KeywordPosition(string keyword)
		{
            if (KeywordPositions.ContainsKey(keyword))
            {
                return KeywordPositions[keyword];
            }

            int hash = 0;
            for (int i = 1; i < keyword.Length; i++)
            {
                hash = (hash + (int)(keyword[i])) * 2;
            }
            hash *= 2;
            hash &= 0x1FFF;

            int startHash = hash;

            while (KeywordPositions.ContainsValue(hash))
            {
                hash++;
                hash &= 0x1FFF;
                if (hash == startHash)
                {
                    //Give up looking, went through all values and they were all taken.
                    throw new Exception("All hash values are taken. Failed to add keyword: " + keyword);
                }
            }

            KeywordPositions[keyword] = hash;
            return hash;
		}
	}
}
