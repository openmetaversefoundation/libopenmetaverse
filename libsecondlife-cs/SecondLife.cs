using System;

namespace libsecondlife
{
	public class Helpers
	{
		public const string VERSION = "libsecondlife-cs 0.0.3";

		public const byte MSG_APPENDED_ACKS = 0x10;
		public const byte MSG_RESENT = 0x20;
		public const byte MSG_RELIABLE = 0x40;
		public const byte MSG_ZEROCODED = 0x80;
		public const ushort MSG_FREQ_HIGH = 0x0000;
		public const ushort MSG_FREQ_MED = 0xFF00;
		public const ushort MSG_FREQ_LOW = 0xFFFF;

		public enum LogLevel 
		{
			Info,
			Warning,
			Error
		};

		public static void Log(string message, LogLevel level)
		{
			Console.WriteLine(level.ToString() + ": " + message);
		}

		public static int ZeroDecode(byte[] src, int srclen, byte[] dest)
		{
			int zerolen = 0;

			Array.Copy(src, 0, dest, 0, 4);
			zerolen += 4;

			for (int i = zerolen; i < srclen; i++) 
			{
				if (src[i] == 0x00) 
				{
					for (byte j = 0; j < src[i + 1]; j++) 
					{
						dest[zerolen++] = 0x00;
					}

					i++;
				} 
				else 
				{
					dest[zerolen++] = src[i];
				}
			}

			return zerolen;
		}

		public static int ZeroEncode(byte[] src, int srclen, byte[] dest)
		{
			int zerolen = 0;
			byte zerocount = 0;

			Array.Copy(src, 0, dest, 0, 4);
			zerolen += 4;

			for (int i = zerolen; i < srclen; i++) 
			{
				if (src[i] == 0x00) 
				{
					zerocount++;

					if (zerocount == 0) 
					{
						dest[zerolen++] = 0x00;
						dest[zerolen++] = 0xff;
						zerocount++;
					}
				}
				else 
				{
					if (zerocount != 0) 
					{
						dest[zerolen++] = 0x00;
						dest[zerolen++] = (byte)zerocount;
						zerocount = 0;
					}

					dest[zerolen++] = src[i];
				}
			}

			if (zerocount != 0) 
			{
				dest[zerolen++] = 0x00;
				dest[zerolen++] = (byte)zerocount;
			}

			return zerolen;
		}
	}

	public class PacketWrapper
	{

	}

	/// <summary>
	/// FIXME: Fill this in
	/// </summary>
	public class SecondLife
	{
		public ProtocolManager Protocol;
		public NetworkManager Network;

		public SecondLife(string keywordFile, string mapFile)
		{
			Protocol = new ProtocolManager(keywordFile, mapFile);
			Network = new NetworkManager(Protocol);
		}

		public void Tick()
		{
			System.Threading.Thread.Sleep(0);
		}
	}
}
