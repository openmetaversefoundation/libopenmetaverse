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
			string protocolMap = "";
			int end;
			int cmdStart = 0;
			int cmdEnd;
			int cmdChildren = 0;
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

			StreamReader r = new StreamReader(map);        
			r.BaseStream.Seek(0, SeekOrigin.Begin);

			while (r.Peek() > -1) 
			{
				// While not at the end of the file  
				protocolMap += r.ReadLine();
			}

			r.Close();
			map.Close();

			cmdEnd = end = protocolMap.Length;

			while (GetBlockMarkers(protocolMap, ref cmdStart, ref cmdEnd, ref cmdChildren)) 
			{
				int length = (cmdChildren != 0 ? cmdChildren - 1 : cmdEnd - 1) - cmdStart;
				string temp = protocolMap.Substring(cmdStart + 1, length).Trim();

				char[] seps = {' '};
				string[] headerTokens = temp.Split(seps);
				
				// Get the frequency first so we know where to put this command
				temp = headerTokens[1];
				if (temp == "Fixed") {
					// Get the fixed position
					temp = headerTokens[2];

					// Remove the leading "0x"
					if (temp.Substring(0, 2) == "0x")
					{
						temp = temp.Substring(2, temp.Length - 2);
					}

					// Convert the hex string to an integer
					uint fix;

					try
					{
						fix = UInt32.Parse(temp, System.Globalization.NumberStyles.HexNumber);
					}
					catch (Exception e)
					{
						Helpers.Log("Protocol map parsing error: " + e.Message, Helpers.LogLevel.Error);
						throw new Exception("Map file error", e);
					}

					// Truncate it to a short
					fix ^= 0xffff0000;
					LowMaps[fix] = new MapPacket();
					LowMaps[fix].ID = (ushort)fix;
					LowMaps[fix].Frequency = PacketFrequency.Low;
					LowMaps[fix].Name = headerTokens[0];
					LowMaps[fix].Trusted = (headerTokens[2] == "Trusted");
					LowMaps[fix].Encoded = (headerTokens[3] == "Zerocoded");
					LowMaps[fix].Blocks = new ArrayList();

					// Get the blocks
					GetBlocks(ref LowMaps[fix], protocolMap, cmdStart + 1, cmdEnd - 1);
				} else if (temp == "Low") {
					LowMaps[low] = new MapPacket();
					LowMaps[low].ID = low;
					LowMaps[low].Frequency = PacketFrequency.Low;
					LowMaps[low].Name = headerTokens[0];
					LowMaps[low].Trusted = (headerTokens[2] == "Trusted");
					LowMaps[low].Encoded = (headerTokens[3] == "Zerocoded");
					LowMaps[low].Blocks = new ArrayList();

					GetBlocks(ref LowMaps[low], protocolMap, cmdStart + 1, cmdEnd - 1);
					low++;
				} else if (temp == "Medium") {
					MediumMaps[medium] = new MapPacket();
					MediumMaps[medium].ID = medium;
					MediumMaps[medium].Frequency = PacketFrequency.Medium;
					MediumMaps[medium].Name = headerTokens[0];
					MediumMaps[medium].Trusted = (headerTokens[2] == "Trusted");
					MediumMaps[medium].Encoded = (headerTokens[3] == "Zerocoded");
					MediumMaps[medium].Blocks = new ArrayList();

					GetBlocks(ref MediumMaps[medium], protocolMap, cmdStart + 1, cmdEnd - 1);
					medium++;
				} else if (temp == "High") {
					HighMaps[high] = new MapPacket();
					HighMaps[high].ID = high;
					HighMaps[high].Frequency = PacketFrequency.High;
					HighMaps[high].Name = headerTokens[0];
					HighMaps[high].Trusted = (headerTokens[2] == "Trusted");
					HighMaps[high].Encoded = (headerTokens[3] == "Zerocoded");
					HighMaps[high].Blocks = new ArrayList();

					GetBlocks(ref HighMaps[high], protocolMap, cmdStart + 1, cmdEnd - 1);
					high++;
				} else {
					Helpers.Log("Unknown frequency \"" + temp + "\"", Helpers.LogLevel.Error);
					throw new Exception("Unknown frequency \"" + temp + "\"");
				}

				// Increment our position in protocol map
				cmdStart = cmdEnd + 1;
				cmdEnd = end;
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

		private bool GetBlockMarkers(string protocolMap, ref int start, ref int end, ref int children)
		{
			int startBlock = 0;
			int depth = 0;
	
			children = 0;

			for (int i = start; i < end; ++i) 
			{
				if (protocolMap[i] == '{') 
				{
					depth++;

					if (depth == 1) 
					{
						startBlock = i;
					} 
					else if (depth == 2 && children == 0) 
					{
						children = i;
					}
				} 
				else if (protocolMap[i] == '}') 
				{
					depth--;

					if (depth == 0 && startBlock != 0) 
					{
						start = startBlock;
						end = i;

						return true;
					}
				}
			}

			return false;
		}

		private bool GetFields(ref MapBlock block, string protocolMap, int start, int end)
		{
			int fieldStart = start;
			int fieldEnd = end;
			int children = 0;
			MapField field;

			while(GetBlockMarkers(protocolMap, ref fieldStart, ref fieldEnd, ref children)) 
			{
				if (children != 0) 
				{
					Helpers.Log("Found fourth tier elements in the protocol map", Helpers.LogLevel.Error);
					return false;
				}

				string temp = protocolMap.Substring(fieldStart + 1, (fieldEnd - 1) - fieldStart).Trim();
				field = new MapField();

				int delimiter = temp.IndexOf(" ");
				if (delimiter == -1) 
				{
					Helpers.Log("Couldn't parse protocol map field: " + temp, Helpers.LogLevel.Error);
					return false;
				}

				// Get the field name
				field.Name = temp.Substring(0, delimiter);

				// Get the keyword position
				field.KeywordPosition = KeywordPosition(field.Name);

				temp = temp.Substring(delimiter + 1, temp.Length - delimiter - 1);

				// Get the field count
				delimiter = temp.IndexOf(" ");
				if (delimiter != -1) 
				{
					try 
					{
						field.Count = Int32.Parse(temp.Substring(delimiter + 1, temp.Length - delimiter - 1));
					} 
					catch (Exception e) 
					{
						Helpers.Log("Error parsing protocol map field count: " + e.Message, Helpers.LogLevel.Error);
					}

					temp = temp.Substring(0, delimiter);
				} 
				else 
				{
					field.Count = 1;
				}

				// Get the field type
				field.Type = (FieldType)Enum.Parse(typeof(FieldType), temp, true);

				// Add this field to the linked list
				block.Fields.Add(field);

				fieldStart = fieldEnd + 1;
				fieldEnd = end;
			}

			// Sort the fields based on the keyword position
			block.Fields.Sort();

			return true;
		}

		private bool GetBlocks(ref MapPacket packet, string protocolMap, int start, int end)
		{
			int blockStart = start;
			int blockEnd = end;
			int children = 0;
			MapBlock block;

			while (GetBlockMarkers(protocolMap, ref blockStart, ref blockEnd, ref children)) 
			{
				int length = (children != 0 ? children - 1 : blockEnd - 1) - blockStart;
				string temp = protocolMap.Substring(blockStart + 1, length).Trim();

				char[] seps = {' '};
				string[] blockTokens = temp.Split(seps);

				block = new MapBlock();

				// Get the block name
				block.Name = blockTokens[0];

				// Find the frequency of this block (-1 for variable, 1 for single)
				temp = blockTokens[1];

				if (temp == "Variable")
				{
					block.Count = -1;
				}
				else if (temp == "Single")
				{
					block.Count = 1;
				}
				else if (temp == "Multiple")
				{
					try
					{
						block.Count = Int32.Parse(blockTokens[2]);
					}
					catch (Exception e)
					{
						Helpers.Log("Error parsing block frequency : " + e.Message, Helpers.LogLevel.Error);
						throw new Exception("Keyword file error", e);
					}
				}
				else
				{
					//TODO: Why is this five?
					block.Count = 5;
				}

				// Get the keyword position of this block
				block.KeywordPosition = KeywordPosition(block.Name);

				// Initialize the ArrayList of fields
				block.Fields = new ArrayList();

				// Add this block to the linked list
				packet.Blocks.Add(block);

				// Populate the fields linked list
				GetFields(ref block, protocolMap, blockStart + 1, blockEnd - 1);

				blockStart = blockEnd + 1;
				blockEnd = end;
			}

			// Sort the blocks based on the keyword position
			packet.Blocks.Sort();

			return true;
		}
	}
}