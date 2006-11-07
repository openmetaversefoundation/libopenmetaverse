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
    /// <summary>
    /// A 128-bit Universally Unique Identifier, used throughout the Second
    /// Life networking protocol
    /// </summary>
	public class LLUUID
	{
        /// <summary>Get a byte array of the 16 raw bytes making up the UUID</summary>
		public byte[] Data
		{
			get { return data; }
		}

        private byte[] data = null;

        /// <summary>
        /// 
        /// </summary>
		public LLUUID()
		{
			data = new byte[16];
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
		public LLUUID(string val)
		{
			data = new byte[16];

			if (val.Length == 36) val = val.Replace("-", "");
			
			if (val.Length != 32) throw new Exception("Malformed data passed to LLUUID constructor: " + val);

			for(int i = 0; i < 16; ++i)
			{
				data[i] = Convert.ToByte(val.Substring(i * 2, 2), 16);
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="byteArray"></param>
        /// <param name="pos"></param>
		public LLUUID(byte[] byteArray, int pos)
		{
			data = new byte[16];

			Array.Copy(byteArray, pos, data, 0, 16);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="randomize"></param>
		public LLUUID(bool randomize)
		{
			if (randomize) data = Guid.NewGuid().ToByteArray();
			else           data = new byte[16];
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            return Data;
        }

		/// <summary>
		/// Calculate an LLCRC (cyclic redundancy check) for this LLUUID
		/// </summary>
		/// <returns>The CRC checksum for this LLUUID</returns>
		public uint CRC() 
		{
			uint retval = 0;

			retval += (uint)((Data[3] << 24) + (Data[2] << 16) + (Data[1] << 8) + Data[0]);
			retval += (uint)((Data[7] << 24) + (Data[6] << 16) + (Data[5] << 8) + Data[4]);
			retval += (uint)((Data[11] << 24) + (Data[10] << 16) + (Data[9] << 8) + Data[8]);
			retval += (uint)((Data[15] << 24) + (Data[14] << 16) + (Data[13] << 8) + Data[12]);

			return retval;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public static LLUUID GenerateUUID()
		{
			return new LLUUID(Guid.NewGuid().ToByteArray(), 0);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
		public override bool Equals(object o)
		{
			if (!(o is LLUUID)) return false;

			LLUUID uuid = (LLUUID)o;

			for (int i = 0; i < 16; ++i)
			{
				if (Data[i] != uuid.Data[i]) return false;
			}

			return true;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
		public static bool operator==(LLUUID lhs, LLUUID rhs)
		{
			if(object.ReferenceEquals(lhs, rhs))  return true;
			if(object.ReferenceEquals(lhs, null)) return false;
			if(object.ReferenceEquals(rhs, null)) return false;

			for (int i = 0; i < 16; ++i)
			{
				if (lhs.Data[i] != rhs.Data[i]) return false;
			}

			return true;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
		public static bool operator!=(LLUUID lhs, LLUUID rhs)
		{
			return !(lhs == rhs);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static LLUUID operator ^(LLUUID lhs, LLUUID rhs)
        {
            LLUUID returnUUID = new LLUUID();

            for (int count = 0; count < returnUUID.Data.Length; count++)
            {
                returnUUID.Data[count] = (byte)(lhs.Data[count] ^ rhs.Data[count]);
            }

            return returnUUID;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static implicit operator LLUUID(string val)
		{
			return new LLUUID(val);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public override string ToString()
		{
			string uuid = "";

			for (int i = 0; i < 16; ++i)
			{
				uuid += Data[i].ToString("x2");
			}

			return uuid;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public string ToStringHyphenated()
		{
			string uuid = "";

			for (int i = 0; i < 16; ++i)
			{
				uuid += Data[i].ToString("x2");

			}
			uuid = uuid.Insert(20,"-");
			uuid = uuid.Insert(16,"-");
			uuid = uuid.Insert(12,"-");
			uuid = uuid.Insert(8,"-");
			
			return uuid;
		}

        /// <summary>
        /// An LLUUID with a value of all zeroes
        /// </summary>
        public readonly static LLUUID Zero = new LLUUID();
	}

    /// <summary>
    /// 
    /// </summary>
	public class LLVector3
	{
        /// <summary></summary>
		public float X;
        /// <summary></summary>
		public float Y;
        /// <summary></summary>
		public float Z;

        /// <summary>
        /// 
        /// </summary>
		public LLVector3()
		{
			X = Y = Z = 0.0F;
		}
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
		public LLVector3(LLVector3d vector)
		{
			X = (float)vector.X;
			Y = (float)vector.Y;
			Z = (float)vector.Z;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="byteArray"></param>
        /// <param name="pos"></param>
		public LLVector3(byte[] byteArray, int pos)
		{
			if(!BitConverter.IsLittleEndian) 
			{
				Array.Reverse(byteArray, pos, 4);
				Array.Reverse(byteArray, pos + 4, 4);
				Array.Reverse(byteArray, pos + 8, 4);
			}

			X = BitConverter.ToSingle(byteArray, pos);
			Y = BitConverter.ToSingle(byteArray, pos + 4);
			Z = BitConverter.ToSingle(byteArray, pos + 8);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
		public LLVector3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public byte[] GetBytes()
		{
			byte[] byteArray = new byte[12];

			Array.Copy(BitConverter.GetBytes(X), 0, byteArray, 0, 4);
			Array.Copy(BitConverter.GetBytes(Y), 0, byteArray, 4, 4);
			Array.Copy(BitConverter.GetBytes(Z), 0, byteArray, 8, 4);

			if(!BitConverter.IsLittleEndian) {
				Array.Reverse(byteArray, 0, 4);
				Array.Reverse(byteArray, 4, 4);
				Array.Reverse(byteArray, 8, 4);
			}

			return byteArray;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public override string ToString()
		{
			return X.ToString() + " " + Y.ToString() + " " + Z.ToString();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public override int GetHashCode()
		{
			int x = (int)X;
			int y = (int)Y;
			int z = (int)Z;

			return (x ^ y ^ z);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
		public override bool Equals(object o)
		{
			if (!(o is LLVector3)) return false;

			LLVector3 vector = (LLVector3)o;

			return (X == vector.X && Y == vector.Y && Z == vector.Z);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
		public static bool operator==(LLVector3 lhs, LLVector3 rhs)
		{
			if(object.ReferenceEquals(lhs, rhs))  return true;
			if(object.ReferenceEquals(lhs, null)) return false;
			if(object.ReferenceEquals(rhs, null)) return false;

			return (lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
		public static bool operator!=(LLVector3 lhs, LLVector3 rhs)
		{
			return !(lhs == rhs);
		}

        public static LLVector3 operator +(LLVector3 lhs, LLVector3 rhs)
        {
            return new LLVector3(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static LLVector3 operator -(LLVector3 lhs, LLVector3 rhs)
        {
            return new LLVector3(lhs.X - rhs.X,lhs.Y - rhs.Y, lhs.Z - rhs.Z);
        }

        /// <summary>
        /// An LLVector3 with a value of <0,0,0>
        /// </summary>
        public readonly static LLVector3 Zero = new LLVector3();
	}

    /// <summary>
    /// 
    /// </summary>
	public class LLVector3d
	{
        /// <summary></summary>
		public double X;
        /// <summary></summary>
		public double Y;
        /// <summary></summary>
		public double Z;

        /// <summary>
        /// 
        /// </summary>
		public LLVector3d()
		{
			X = Y = Z = 0.0D;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
		public LLVector3d(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="byteArray"></param>
        /// <param name="pos"></param>
		public LLVector3d(byte[] byteArray, int pos)
		{
			if(!BitConverter.IsLittleEndian) {
				Array.Reverse(byteArray, pos, 8);
				Array.Reverse(byteArray, pos + 8, 8);
				Array.Reverse(byteArray, pos + 16, 8);
			}

			X = BitConverter.ToDouble(byteArray, pos);
			Y = BitConverter.ToDouble(byteArray, pos + 8);
			Z = BitConverter.ToDouble(byteArray, pos + 16);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public byte[] GetBytes()
		{
			byte[] byteArray = new byte[24];

			Array.Copy(BitConverter.GetBytes(X), 0, byteArray, 0, 8);
			Array.Copy(BitConverter.GetBytes(Y), 0, byteArray, 8, 8);
			Array.Copy(BitConverter.GetBytes(Z), 0, byteArray, 16, 8);

			if(!BitConverter.IsLittleEndian) {
				Array.Reverse(byteArray, 0, 8);
				Array.Reverse(byteArray, 8, 8);
				Array.Reverse(byteArray, 16, 8);
			}

			return byteArray;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public override string ToString()
		{
			return X.ToString() + " " + Y.ToString() + " " + Z.ToString();
		}

        /// <summary>
        /// An LLVector3d with a value of <0,0,0>
        /// </summary>
        public readonly static LLVector3d Zero = new LLVector3d();
	}

    /// <summary>
    /// 
    /// </summary>
	public class LLVector4
	{
        /// <summary></summary>
		public float X;
        /// <summary></summary>
		public float Y;
        /// <summary></summary>
		public float Z;
        /// <summary></summary>
		public float S;

        /// <summary>
        /// 
        /// </summary>
		public LLVector4()
		{
			X = Y = Z = S = 0.0F;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="byteArray"></param>
        /// <param name="pos"></param>
		public LLVector4(byte[] byteArray, int pos)
		{
			if(!BitConverter.IsLittleEndian) {
				Array.Reverse(byteArray, pos, 4);
				Array.Reverse(byteArray, pos + 4, 4);
				Array.Reverse(byteArray, pos + 8, 4);
				Array.Reverse(byteArray, pos + 12, 4);
			}

			X = BitConverter.ToSingle(byteArray, pos);
			Y = BitConverter.ToSingle(byteArray, pos + 4);
			Z = BitConverter.ToSingle(byteArray, pos + 8);
			S = BitConverter.ToSingle(byteArray, pos + 12);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public byte[] GetBytes()
		{
			byte[] byteArray = new byte[16];

			Array.Copy(BitConverter.GetBytes(X), 0, byteArray, 0, 4);
			Array.Copy(BitConverter.GetBytes(Y), 0, byteArray, 4, 4);
			Array.Copy(BitConverter.GetBytes(Z), 0, byteArray, 8, 4);
			Array.Copy(BitConverter.GetBytes(S), 0, byteArray, 12, 4);

			if(!BitConverter.IsLittleEndian) {
				Array.Reverse(byteArray, 0, 4);
				Array.Reverse(byteArray, 4, 4);
				Array.Reverse(byteArray, 8, 4);
				Array.Reverse(byteArray, 12, 4);
			}

			return byteArray;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public override string ToString()
		{
			return X.ToString() + " " + Y.ToString() + " " + Z.ToString() + " " + S.ToString();
		}

        /// <summary>
        /// An LLVector4 with a value of <0,0,0,0>
        /// </summary>
        public readonly static LLVector4 Zero = new LLVector4();
	}

    /// <summary>
    /// 
    /// </summary>
	public class LLQuaternion
	{
        /// <summary></summary>
		public float X;
        /// <summary></summary>
		public float Y;
        /// <summary></summary>
		public float Z;
        /// <summary></summary>
		public float W;

        /// <summary>
        /// 
        /// </summary>
		public LLQuaternion()
		{
			X = Y = Z = 0.0f;
            W = 1.0f;
		}

        /// <summary>
        /// Build a quaternion object from a byte array
        /// </summary>
        /// <param name="byteArray">The source byte array</param>
        /// <param name="pos">Offset in the byte array to start reading at</param>
        /// <param name="normalized">Whether the source data is normalized or
        /// not. If this is true 12 bytes will be read, otherwise 16 bytes will
        /// be read.</param>
        public LLQuaternion(byte[] byteArray, int pos, bool normalized)
        {
            if (!normalized)
            {
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(byteArray, pos, 4);
                    Array.Reverse(byteArray, pos + 4, 4);
                    Array.Reverse(byteArray, pos + 8, 4);
                    Array.Reverse(byteArray, pos + 12, 4);
                }

                X = BitConverter.ToSingle(byteArray, pos);
                Y = BitConverter.ToSingle(byteArray, pos + 4);
                Z = BitConverter.ToSingle(byteArray, pos + 8);
                W = BitConverter.ToSingle(byteArray, pos + 12);
            }
            else
            {
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(byteArray, pos, 4);
                    Array.Reverse(byteArray, pos + 4, 4);
                    Array.Reverse(byteArray, pos + 8, 4);
                }

                X = BitConverter.ToSingle(byteArray, pos);
                Y = BitConverter.ToSingle(byteArray, pos + 4);
                Z = BitConverter.ToSingle(byteArray, pos + 8);

                float xyzsum = 1 - X * X - Y * Y - Z * Z;
                W = (xyzsum > 0) ? (float)Math.Sqrt(xyzsum) : 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="w"></param>
		public LLQuaternion(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public byte[] GetBytes()
		{
            byte[] bytes = new byte[12];
            float norm;

            norm = (float)Math.Sqrt(X*X + Y*Y + Z*Z + W*W);

            if (norm != 0)
            {
                norm = 1 / norm;

                Array.Copy(BitConverter.GetBytes(norm * X), 0, bytes, 0, 4);
                Array.Copy(BitConverter.GetBytes(norm * Y), 0, bytes, 4, 4);
                Array.Copy(BitConverter.GetBytes(norm * Z), 0, bytes, 8, 4);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytes, 0, 4);
                    Array.Reverse(bytes, 4, 4);
                    Array.Reverse(bytes, 8, 4);
                }
            }
            else
            {
                throw new Exception("Quaternion <" + X + "," + Y + "," + Z + "," + W + "> normalized to zero");
            }

			return bytes;
		}

        public static LLQuaternion operator *(LLQuaternion lhs, LLQuaternion rhs)
        {
            float A = (lhs.W + lhs.X) * (rhs.W + rhs.X);
            float B = (lhs.Z - lhs.Y) * (rhs.Y - rhs.Z);
            float C = (lhs.X - lhs.W) * (rhs.Y - rhs.Z);
            float D = (lhs.Y + lhs.Z) * (rhs.X - rhs.W);
            float E = (lhs.X + lhs.Z) * (rhs.X + rhs.Y);
            float F = (lhs.X - lhs.Z) * (rhs.X - rhs.Y);
            float G = (lhs.W + lhs.Y) * (rhs.W - rhs.Z);
            float H = (lhs.W - lhs.Y) * (rhs.W + rhs.Z);

            return new LLQuaternion(
                 A - ( E + F + G + H) / 2,  // X
                -C + ( E - F + G - H) / 2,  // Y
                -D + ( E - F - G + H) / 2,  // Z
                 B + (-E - F + G + H) / 2); // W
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public override string ToString()
		{
			return X.ToString() + " " + Y.ToString() + " " + Z.ToString() + " " + W.ToString();
		}

        /// <summary>
        /// An LLQuaternion with a value of <0,0,0,1>
        /// </summary>
        public readonly static LLQuaternion Identity = new LLQuaternion();
	}
}
