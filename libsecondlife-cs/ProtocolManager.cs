using System;
using System.Collections;
using System.IO;

namespace libsecondlife
{
	public enum PacketFrequency
	{
		Low,
		Medium,
		High
	}

	public enum FieldType
	{
		U8,
		U16,
		U32,
		U64,
		S8,
		S16,
		S32,
		S64,
		F32,
		F64,
		LLUUID,
		BOOL,
		LLVector3,
		LLVector3d,
		LLVector4,
		LLQuaternion,
		IPADDR,
		IPPORT,
		Variable,
		Fixed,
		Single,
		Multiple
	}

	public class MapField : IComparable
	{
		public int KeywordPosition;
		public string Name;
		public FieldType Type;
		public int Count;

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

	public class MapBlock : IComparable
	{
		public int KeywordPosition;
		public string Name;
		public int Count;
		public ArrayList Fields;

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

	public class MapPacket
	{
		public ushort ID;
		public string Name;
		public PacketFrequency Frequency;
		public bool Trusted;
		public bool Encoded;
		public ArrayList Blocks;
	}

	public class ProtocolManager
	{
		public Hashtable TypeSizes;
		public Hashtable KeywordPositions;
		public MapPacket[] LowMaps;
		public MapPacket[] MediumMaps;
		public MapPacket[] HighMaps;
		int i = 0;

		public ProtocolManager(string keywordFile, string mapFile)
		{
			// Initialize the map arrays
			LowMaps = new MapPacket[65536];
			MediumMaps = new MapPacket[256];
			HighMaps = new MapPacket[256];

			// Build the type size hash table
			TypeSizes = new Hashtable();
			TypeSizes.Add(FieldType.U8, 1);
			TypeSizes.Add(FieldType.U16, 2);
			TypeSizes.Add(FieldType.U32, 4);
			TypeSizes.Add(FieldType.U64, 8);
			TypeSizes.Add(FieldType.S8, 1);
			TypeSizes.Add(FieldType.S16, 2);
			TypeSizes.Add(FieldType.S32, 4);
			TypeSizes.Add(FieldType.S64, 8);
			TypeSizes.Add(FieldType.F32, 4);
			TypeSizes.Add(FieldType.F64, 8);
			TypeSizes.Add(FieldType.LLUUID, 16);
			TypeSizes.Add(FieldType.BOOL, 1);
			TypeSizes.Add(FieldType.LLVector3, 12);
			TypeSizes.Add(FieldType.LLVector3d, 24);
			TypeSizes.Add(FieldType.LLVector4, 16);
			TypeSizes.Add(FieldType.LLQuaternion, 16);
			TypeSizes.Add(FieldType.IPADDR, 4);
			TypeSizes.Add(FieldType.IPPORT, 2);
			TypeSizes.Add(FieldType.Variable, -1);
			TypeSizes.Add(FieldType.Fixed, -2);

			LoadKeywordFile(keywordFile);
			LoadMapFile(mapFile);
		}

		public MapPacket Command(string command)
		{
			//TODO: Get a hashtable in here quick!

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

			// This will speed things up for now
			if (command == LowMaps[65531].Name)
			{
				return LowMaps[65531];
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

		public void PrintMap()
		{
			PrintOneMap(LowMaps,    "Low   ");
			PrintOneMap(MediumMaps, "Medium");
			PrintOneMap(HighMaps,   "High  ");
		}

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

		private void LoadKeywordFile(string keywordFile)
		{
			string line;
			StreamReader file;

			KeywordPositions = new Hashtable();

			// Load the keyword file
			try
			{
				file = File.OpenText(keywordFile);
			}
			catch(Exception e)
			{
				Helpers.Log("Error opening \"" + keywordFile + "\": " + e.Message, Helpers.LogLevel.Error);
				throw new Exception("Keyword file error", e);
			}

			while((line = file.ReadLine()) != null)
			{
				KeywordPositions.Add(line.Trim(), i++);
			}

			file.Close();
		}

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
				Helpers.Log("Error opening \"" + mapFile + "\": " + e.Message, Helpers.LogLevel.Error);
				throw new Exception("Map file error", e);
			}

			try
			{
				output = new BinaryWriter(new FileStream(outputFile, FileMode.CreateNew));
			}
			catch(Exception e)
			{
				Helpers.Log("Error opening \"" + outputFile + "\": " + e.Message, Helpers.LogLevel.Error);
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
				Helpers.Log("Error opening \"" + mapFile + "\": " + e.Message, Helpers.LogLevel.Error);
				throw new Exception("Map file error", e);
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
										LowMaps[fixedID].Blocks = new ArrayList();

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
										LowMaps[low].Blocks = new ArrayList();

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
										MediumMaps[medium].Blocks = new ArrayList();

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
										HighMaps[high].Blocks = new ArrayList();

										currentPacket = HighMaps[high];

										high++;
									}
									else
									{
										Helpers.Log("!!!", Helpers.LogLevel.Error);
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
								currentBlock.Fields = new ArrayList();
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
									Helpers.Log("Unknown block frequency!", Helpers.LogLevel.Error);
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
				Helpers.Log(e.ToString(), Helpers.LogLevel.Error);
			}
		}

		private int KeywordPosition(string keyword)
		{
			if (KeywordPositions.ContainsKey(keyword)) 
			{
				return (int)KeywordPositions[keyword];
			} 
			else 
			{
				Helpers.Log("Couldn't find keyword: " + keyword, Helpers.LogLevel.Warning);
				return -1;
			}
		}
	}
}