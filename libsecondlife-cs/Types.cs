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

namespace libsecondlife
{
	public class U64
	{
		public uint[] Data;

		public U64()
		{
			Data = new uint[2];
			Data[0] = 0;
			Data[1] = 1;
		}

		public U64(uint left, uint right)
		{
			Data = new uint[2];
			// Backwards... don't ask me, it works
			Data[0] = right;
			Data[1] = left;
		}

		public U64(int left, int right)
		{
			Data = new uint[2];
			// Backwards... don't ask me, it works
			Data[0] = (uint)right;
			Data[1] = (uint)left;
		}

		public U64(byte[] bA, int pos)
		{
			Data = new uint[2];
			Data[0] = DataConvert.toU32(bA, pos);
			Data[1] = DataConvert.toU32(bA, pos + 4);
		}

		public byte[] GetBytes()
		{
			byte[] bA = new byte[8];

			Array.Copy(bA, 0, DataConvert.from(Data[0]), 0, 4);
			Array.Copy(bA, 4, DataConvert.from(Data[1]), 0, 4);
			return bA;
		}

		public override int GetHashCode()
		{
			return (int)(Data[0] ^ Data[1]);
		}

		public override bool Equals(object o)
		{
			if (!(o is U64))
			{
				return false;
			}

			U64 u64 = (U64)o;

			if (u64.Data[0] == Data[0] && u64.Data[1] == Data[1])
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool operator==(U64 lhs, U64 rhs)
		{
			try
			{
				if (lhs.Data[0] == rhs.Data[0] && lhs.Data[1] == rhs.Data[1])
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (NullReferenceException)
			{
				uint test;
				bool lhsnull = false;
				bool rhsnull = false;

				try
				{
					test = lhs.Data[0];
				}
				catch (NullReferenceException)
				{
					lhsnull = true;
				}

				try
				{
					test = rhs.Data[0];
				}
				catch (NullReferenceException)
				{
					rhsnull = true;
				}
				
				return (lhsnull == rhsnull);
			}
		}

		public static bool operator!=(U64 lhs, U64 rhs)
		{
			return !(lhs == rhs);
		}

		public static bool operator==(U64 lhs, int rhs)
		{
			try
			{
				if (lhs.Data[1] == rhs)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (NullReferenceException)
			{
				return (rhs == 0);
			}
		}

		public static bool operator!=(U64 lhs, int rhs)
		{
			return !(lhs == rhs);
		}

		public override string ToString()
		{
			ulong u64 = (Data[1] << 32) + Data[0];
			return u64.ToString();
		}
	}

	public class LLUUID
	{
		private byte[] data = null;
		public byte[] Data
		{
			get { return data; }
		}

		public LLUUID()
		{
			data = new byte[16];
		}

		public LLUUID(string val)
		{
			data = new byte[16];

			if (val.Length == 36)
			{
				val = val.Replace("-", "");
			}
			
			if (val.Length != 32)
			{
				return;
			}

			for(int i = 0; i < 16; ++i)
			{
				data[i] = Convert.ToByte(val.Substring(i * 2, 2), 16);
			}
		}

		public LLUUID(byte[] byteArray, int pos)
		{
			data = new byte[16];

			Array.Copy(byteArray, pos, data, 0, 16);
		}

		public LLUUID(byte[] byteArray)
		{
			data = new byte[16];

			Array.Copy(byteArray, 0, data, 0, 16);
		}

		public LLUUID(bool randomize)
		{
			
			if (randomize)
			{
				data = Guid.NewGuid().ToByteArray();
			}
			else
			{
				data = new byte[16];
			}
		}

		/// <summary>
		/// Calculate an LLCRC for the given LLUUID
		/// </summary>
		/// <param name="uuid">The LLUUID to calculate the CRC value for</param>
		/// <returns>The CRC checksum for this LLUUID</returns>
		public uint CRC() 
		{
			uint retval = 0;

			retval += DataConvert.toU32(Data, 0);
			retval += DataConvert.toU32(Data, 4);
			retval += DataConvert.toU32(Data, 8);
			retval += DataConvert.toU32(Data, 12);

			return retval;
		}

		public static LLUUID GenerateUUID()
		{
			return new LLUUID(Guid.NewGuid().ToByteArray(), 0);
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public override bool Equals(object o)
		{
			if (!(o is LLUUID))
			{
				return false;
			}

			LLUUID uuid = (LLUUID)o;

			for (int i = 0; i < 16; ++i)
			{
				if (Data[i] != uuid.Data[i])
				{
					return false;
				}
			}

			return true;
		}

		public static bool operator==(LLUUID lhs, LLUUID rhs)
		{
			try
			{
				for (int i = 0; i < 16; ++i)
				{
					if (lhs.Data[i] != rhs.Data[i])
					{
						return false;
					}
				}
			}
			catch (NullReferenceException)
			{
				byte test;
				bool lhsnull = false;
				bool rhsnull = false;

				try
				{
					test = lhs.Data[0];
				}
				catch (NullReferenceException)
				{
					lhsnull = true;
				}

				try
				{
					test = rhs.Data[0];
				}
				catch (NullReferenceException)
				{
					rhsnull = true;
				}
				
				return (lhsnull == rhsnull);
			}

			return true;
		}

		public static bool operator!=(LLUUID lhs, LLUUID rhs)
		{
			return !(lhs == rhs);
		}

		public static implicit operator LLUUID(string val)
		{
			return new LLUUID(val);
		}

		public override string ToString()
		{
			string uuid = "";

			for (int i = 0; i < 16; ++i)
			{
				uuid += Data[i].ToString("x2");
			}

			return uuid;
		}
	}

	public class LLVector3
	{
		public float X;
		public float Y;
		public float Z;

		public LLVector3()
		{
			X = Y = Z = 0.0F;
		}
		
		public LLVector3(LLVector3d vector)
		{
			X = (float)vector.X;
			Y = (float)vector.Y;
			Z = (float)vector.Z;
		}

		public LLVector3(byte[] byteArray, int pos)
		{
			X = DataConvert.toFloat(byteArray, pos);
			Y = DataConvert.toFloat(byteArray, pos + 4);
			Z = DataConvert.toFloat(byteArray, pos + 8);
		}

		public LLVector3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public byte[] GetBytes()
		{
			byte[] byteArray = new byte[12];

			Array.Copy(DataConvert.from(X), 0, byteArray, 0, 4);
			Array.Copy(DataConvert.from(Y), 0, byteArray, 4, 4);
			Array.Copy(DataConvert.from(Z), 0, byteArray, 8, 4);

			return byteArray;
		}

		public override string ToString()
		{
			return X.ToString() + " " + Y.ToString() + " " + Z.ToString();
		}

		public override int GetHashCode()
		{
			uint x = DataConvert.toU32(DataConvert.from(X));
			uint y = DataConvert.toU32(DataConvert.from(Y));
			uint z = DataConvert.toU32(DataConvert.from(Z));

			return (int)(x ^ y ^ z);
		}

		public override bool Equals(object o)
		{
			if (!(o is LLVector3))
			{
				return false;
			}

			LLVector3 vector = (LLVector3)o;

			if (X == vector.X && Y == vector.Y && Z == vector.Z)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool operator==(LLVector3 lhs, LLVector3 rhs)
		{
			try
			{
				if (lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (NullReferenceException)
			{
				float test;
				bool lhsnull = false;
				bool rhsnull = false;

				try
				{
					test = lhs.X;
				}
				catch (NullReferenceException)
				{
					lhsnull = true;
				}

				try
				{
					test = rhs.X;
				}
				catch (NullReferenceException)
				{
					rhsnull = true;
				}
				
				return (lhsnull == rhsnull);
			}
		}

		public static bool operator!=(LLVector3 lhs, LLVector3 rhs)
		{
			return !(lhs == rhs);
		}
	}

	public class LLVector3d
	{
		public double X;
		public double Y;
		public double Z;

		public LLVector3d()
		{
			X = Y = Z = 0.0D;
		}

		public LLVector3d(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public LLVector3d(byte[] byteArray, int pos)
		{
			X = DataConvert.toDouble(byteArray, pos);
			Y = DataConvert.toDouble(byteArray, pos + 8);
			Z = DataConvert.toDouble(byteArray, pos + 16);
		}

		public byte[] GetBytes()
		{
			byte[] byteArray = new byte[24];

			Array.Copy(DataConvert.from(X), 0, byteArray, 0, 8);
			Array.Copy(DataConvert.from(Y), 0, byteArray, 8, 8);
			Array.Copy(DataConvert.from(Z), 0, byteArray, 16, 8);
			return byteArray;
		}

		public override string ToString()
		{
			return X.ToString() + " " + Y.ToString() + " " + Z.ToString();
		}
	}

	public class LLVector4
	{
		public float X;
		public float Y;
		public float Z;
		public float S;

		public LLVector4()
		{
			X = Y = Z = S = 0.0F;
		}

		public LLVector4(byte[] byteArray, int pos)
		{
			X = DataConvert.toFloat(byteArray, pos);
			Y = DataConvert.toFloat(byteArray, pos + 4);
			Z = DataConvert.toFloat(byteArray, pos + 8);
			S = DataConvert.toFloat(byteArray, pos + 12);
		}

		public byte[] GetBytes()
		{
			byte[] byteArray = new byte[16];

			Array.Copy(DataConvert.from(X), 0, byteArray, 0, 4);
			Array.Copy(DataConvert.from(Y), 0, byteArray, 4, 4);
			Array.Copy(DataConvert.from(Z), 0, byteArray, 8, 4);
			Array.Copy(DataConvert.from(S), 0, byteArray, 12, 4);

			return byteArray;
		}

		public override string ToString()
		{
			return X.ToString() + " " + Y.ToString() + " " + Z.ToString() + " " + S.ToString();
		}
	}

	public class LLQuaternion
	{
		public float X;
		public float Y;
		public float Z;
		public float S;

		public LLQuaternion()
		{
			X = Y = Z = S = 0.0F;
		}

		public LLQuaternion(byte[] byteArray, int pos)
		{
			X = DataConvert.toFloat(byteArray, pos);
			Y = DataConvert.toFloat(byteArray, pos + 4);
			Z = DataConvert.toFloat(byteArray, pos + 8);
			S = DataConvert.toFloat(byteArray, pos + 12);
		}

		public LLQuaternion(float x, float y, float z, float s)
		{
			X = x;
			Y = y;
			Z = z;
			S = s;
		}

		public byte[] GetBytes()
		{
			byte[] byteArray = new byte[16];

			Array.Copy(DataConvert.from(X), 0, byteArray, 0, 4);
			Array.Copy(DataConvert.from(Y), 0, byteArray, 4, 4);
			Array.Copy(DataConvert.from(Z), 0, byteArray, 8, 4);
			Array.Copy(DataConvert.from(S), 0, byteArray, 12, 4);

			return byteArray;
		}

		public override string ToString()
		{
			return X.ToString() + " " + Y.ToString() + " " + Z.ToString() + " " + S.ToString();
		}
	}

	public class DataConvert
	{
		private static byte[] ba;

		public static float toFloat(object Data, int offset)
		{
			ba = (byte[])Data;
			if(!BitConverter.IsLittleEndian) 
				Array.Reverse(ba, offset, 4);
			return BitConverter.ToSingle(ba, offset);
		}

		public static float toFloat(object Data)
		{
			return toFloat(Data, 0);
		}

		public static double toDouble(object Data, int offset)
		{
			ba = (byte[])Data;
			if(!BitConverter.IsLittleEndian) 
				Array.Reverse(ba, offset, 8);
			return BitConverter.ToDouble(ba, offset);
		}

		public static double toDouble(object Data)
		{
			return toDouble(Data, 0);
		}

		public static byte toU8(object Data, int offset)
		{
			ba = (byte[])Data;
			return ba[offset];
		}

		public static byte toU8(object Data)
		{
			return toU8(Data,0);
		}

		public static ushort toU16(object Data, int offset)
		{
			return (ushort)(toU8(Data,0) + toU8(Data,1) * 256);
		}

		public static ushort toU16(object Data)
		{
			return toU16(Data, 0);
		}

		public static uint toU32(object Data, int offset)
		{
			return (uint)(toU16(Data,0) + toU16(Data,2) * 16384);
		}

		public static uint toU32(object Data)
		{
			return toU32(Data, 0);
		}

		public static String toChoppedString(object Data)
		{
			return System.Text.Encoding.UTF8.GetString((byte[])Data).Replace("\0", "");
		}

		public static byte[] from(byte data) {
			return new byte[1]{data};
		}

		public static byte[] from(ushort data) {
			return new byte[2]{(byte)(data%256),(byte)(data>>8)};
		}

		public static byte[] from(uint data) {
			return new byte[4] {(byte)      (data%256),(byte)((data>> 8)%256),
					    (byte)((data>>16)%256),(byte)((data>>24)%256)};
		}
			
		public static byte[] from(float data) 
		{
			ba = BitConverter.GetBytes(data);
			if(!BitConverter.IsLittleEndian) 
				Array.Reverse(ba, 0, 4);
			return ba;
		}

		public static byte[] from(double data) 
		{
			ba = BitConverter.GetBytes(data);
			if(!BitConverter.IsLittleEndian) 
				Array.Reverse(ba, 0, 8);
			return ba;
		}

	}
}
