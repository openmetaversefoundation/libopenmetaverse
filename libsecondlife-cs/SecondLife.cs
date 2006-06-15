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
			uint zerolen = 0;

			try
			{
				Array.Copy(src, 0, dest, 0, 4);
				zerolen += 4;

				for (uint i = zerolen; i < srclen; i++) 
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
			}
			catch (Exception e)
			{
				Helpers.Log(e.ToString(), Helpers.LogLevel.Error);
			}

			return (int)zerolen;
		}

		public static int ZeroEncode(byte[] src, int srclen, byte[] dest)
		{
			uint zerolen = 0;
			byte zerocount = 0;

			Array.Copy(src, 0, dest, 0, 4);
			zerolen += 4;

			for (uint i = zerolen; i < srclen; i++) 
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

			return (int)zerolen;
		}

		public static ulong BuildULong(uint left, uint right)
		{
			// TODO: Make sure this is cross platform to big endian architecture
			byte[] byteArray = new byte[8];

            Array.Copy(BitConverter.GetBytes(left), 0, byteArray, 0, 4);
			Array.Copy(BitConverter.GetBytes(right), 0, byteArray, 4, 4);

			return BitConverter.ToUInt64(byteArray, 0);
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
		public ParcelManager Parcels;
		public MainAvatar Avatar;

		public SecondLife(string keywordFile, string mapFile)
		{
			Protocol = new ProtocolManager(keywordFile, mapFile);
			Network = new NetworkManager(Protocol);
			Parcels = new ParcelManager(this);
			Avatar = new MainAvatar(this);
		}

		public void Tick()
		{
			System.Threading.Thread.Sleep(0);
		}
	}
}
