/*
 * Copyright (c) 2006-2007, Second Life Reverse Engineering Team
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
using System.ComponentModel;
using System.Net;
using System.Xml.Serialization;

namespace libsecondlife
{
    /// <summary>
    /// A 128-bit Universally Unique Identifier, used throughout the Second
    /// Life networking protocol
    /// </summary>
    [Serializable]
    public struct LLUUID
	{
        /// <summary>The System.Guid object this struct wraps around</summary>
        [XmlAttribute]
        public Guid UUID;

        /// <summary>Get a byte array of the 16 raw bytes making up the UUID</summary>
        public byte[] Data { get { return GetBytes(); } }

        /// <summary>
        /// Constructor that takes a string UUID representation
        /// </summary>
        /// <param name="val">A string representation of a UUID, case 
        /// insensitive and can either be hyphenated or non-hyphenated</param>
        /// <example>LLUUID("11f8aa9c-b071-4242-836b-13b7abe0d489")</example>
		public LLUUID(string val)
		{
            if (val == null)
                UUID = new Guid();
            else
                UUID = new Guid(val);
		}

        /// <summary>
        /// Constructor that takes a System.Guid object
        /// </summary>
        /// <param name="val">A Guid object that contains the unique identifier
        /// to be represented by this LLUUID</param>
        public LLUUID(Guid val)
        {
            UUID = val;
        }

        /// <summary>
        /// Constructor that takes a byte array containing a UUID
        /// </summary>
        /// <param name="source">Byte array containing a 16 byte UUID</param>
        /// <param name="pos">Beginning offset in the array</param>
        public LLUUID(byte[] source, int pos)
        {
            UUID = new Guid(
                (source[pos + 0] << 24) | (source[pos + 1] << 16) | (source[pos + 2] << 8) | source[pos + 3],
                (short)((source[pos + 4] << 8) | source[pos + 5]),
                (short)((source[pos + 6] << 8) | source[pos + 7]),
                source[pos +  8], source[pos +  9], source[pos + 10], source[pos + 11],
                source[pos + 12], source[pos + 13], source[pos + 14], source[pos + 15]);
        }

        /// <summary>
        /// Constructor that takes an unsigned 64-bit unsigned integer to 
        /// convert to a UUID
        /// </summary>
        /// <param name="val">64-bit unsigned integer to convert to a UUID</param>
        public LLUUID(ulong val)
        {
            UUID = new Guid(0, 0, 0, BitConverter.GetBytes(val));
        }

        /// <summary>
        /// Returns the raw bytes for this UUID
        /// </summary>
        /// <returns>A 16 byte array containing this UUID</returns>
        public byte[] GetBytes()
        {
            byte[] bytes = UUID.ToByteArray();

            if (BitConverter.IsLittleEndian)
            {
                byte[] output = new byte[16];
                output[0] = bytes[3];
                output[1] = bytes[2];
                output[2] = bytes[1];
                output[3] = bytes[0];
                output[4] = bytes[5];
                output[5] = bytes[4];
                output[6] = bytes[7];
                output[7] = bytes[6];
                Buffer.BlockCopy(bytes, 8, output, 8, 8);

                return output;
            }
            else
            {
                return bytes;
            }
        }

		/// <summary>
		/// Calculate an LLCRC (cyclic redundancy check) for this LLUUID
		/// </summary>
		/// <returns>The CRC checksum for this LLUUID</returns>
		public uint CRC() 
		{
			uint retval = 0;
            byte[] bytes = GetBytes();

            retval += (uint)((bytes[ 3] << 24) + (bytes[ 2] << 16) + (bytes[ 1] << 8) + bytes[ 0]);
            retval += (uint)((bytes[ 7] << 24) + (bytes[ 6] << 16) + (bytes[ 5] << 8) + bytes[ 4]);
            retval += (uint)((bytes[11] << 24) + (bytes[10] << 16) + (bytes[ 9] << 8) + bytes[ 8]);
            retval += (uint)((bytes[15] << 24) + (bytes[14] << 16) + (bytes[13] << 8) + bytes[12]);

			return retval;
		}

        /// <summary>
        /// Combine two UUIDs together by taking the MD5 hash of a byte array
        /// containing both UUIDs
        /// </summary>
        /// <param name="other">The UUID to combine with this one</param>
        /// <returns>The UUID product of the combination</returns>
        public LLUUID Combine(LLUUID other)
        {
            // Build the buffer to MD5
            byte[] input = new byte[32];
            Buffer.BlockCopy(GetBytes(), 0, input, 0, 16);
            Buffer.BlockCopy(other.GetBytes(), 0, input, 16, 16);

            return new LLUUID(Helpers.MD5Builder.ComputeHash(input), 0);
        }

        /// <summary>
        /// Generate a LLUUID from a string
        /// </summary>
        /// <param name="val">A string representation of a UUID, case 
        /// insensitive and can either be hyphenated or non-hyphenated</param>
        /// <example>LLUUID.Parse("11f8aa9c-b071-4242-836b-13b7abe0d489")</example>
        public static LLUUID Parse(string val)
        {
            return new LLUUID(val);
        }

        /// <summary>
        /// Generate a LLUUID from a string
        /// </summary>
        /// <param name="val">A string representation of a UUID, case 
        /// insensitive and can either be hyphenated or non-hyphenated</param>
        /// <param name="result">Will contain the parsed UUID if successful,
        /// otherwise null</param>
        /// <returns>True if the string was successfully parse, otherwise false</returns>
        /// <example>LLUUID.TryParse("11f8aa9c-b071-4242-836b-13b7abe0d489", result)</example>
        public static bool TryParse(string val, out LLUUID result)
        {
            try
            {
                result = Parse(val);
                return true;
            }
            catch (Exception)
            {
                result = LLUUID.Zero;
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public static LLUUID Random()
		{
			return new LLUUID(Guid.NewGuid());
		}

        /// <summary>
        /// Return a hash code for this UUID, used by .NET for hash tables
        /// </summary>
        /// <returns>An integer composed of all the UUID bytes XORed together</returns>
		public override int GetHashCode()
		{
            return UUID.GetHashCode();
		}

        /// <summary>
        /// Comparison function
        /// </summary>
        /// <param name="o">An object to compare to this UUID</param>
        /// <returns>False if the object is not an LLUUID, true if it is and
        /// byte for byte identical to this</returns>
		public override bool Equals(object o)
		{
			if (!(o is LLUUID)) return false;

			LLUUID uuid = (LLUUID)o;
            return UUID == uuid.UUID;
		}

        /// <summary>
        /// Equals operator
        /// </summary>
        /// <param name="lhs">First LLUUID for comparison</param>
        /// <param name="rhs">Second LLUUID for comparison</param>
        /// <returns>True if the UUIDs are byte for byte equal, otherwise false</returns>
		public static bool operator==(LLUUID lhs, LLUUID rhs)
		{
            return lhs.UUID == rhs.UUID;
		}

        /// <summary>
        /// Not equals operator
        /// </summary>
        /// <param name="lhs">First LLUUID for comparison</param>
        /// <param name="rhs">Second LLUUID for comparison</param>
        /// <returns>True if the UUIDs are not equal, otherwise true</returns>
		public static bool operator!=(LLUUID lhs, LLUUID rhs)
		{
			return !(lhs == rhs);
		}

        /// <summary>
        /// XOR operator
        /// </summary>
        /// <param name="lhs">First LLUUID</param>
        /// <param name="rhs">Second LLUUID</param>
        /// <returns>A UUID that is a XOR combination of the two input UUIDs</returns>
        public static LLUUID operator ^(LLUUID lhs, LLUUID rhs)
        {
            byte[] lhsbytes = lhs.GetBytes();
            byte[] rhsbytes = rhs.GetBytes();
            byte[] output = new byte[16];

            for (int i = 0; i < 16; i++)
            {
                output[i] = (byte)(lhsbytes[i] ^ rhsbytes[i]);
            }

            return new LLUUID(output, 0);
        }

        /// <summary>
        /// String typecasting operator
        /// </summary>
        /// <param name="val">A UUID in string form. Case insensitive, 
        /// hyphenated or non-hyphenated</param>
        /// <returns>A UUID built from the string representation</returns>
        public static implicit operator LLUUID(string val)
		{
			return new LLUUID(val);
		}

        /// <summary>
        /// Get a string representation of this UUID
        /// </summary>
        /// <returns>A string representation of this UUID, lowercase and 
        /// without hyphens</returns>
        /// <example>11f8aa9cb0714242836b13b7abe0d489</example>
		public override string ToString()
		{
			string uuid = UUID.ToString();
			return uuid.Replace("-", String.Empty);
		}

        /// <summary>
        /// Get a hyphenated string representation of this UUID
        /// </summary>
        /// <returns>A string representation of this UUID, lowercase and 
        /// with hyphens</returns>
        /// <example>11f8aa9c-b071-4242-836b-13b7abe0d489</example>
		public string ToStringHyphenated()
		{
            return UUID.ToString();
		}

        /// <summary>
        /// An LLUUID with a value of all zeroes
        /// </summary>
        public static readonly LLUUID Zero = new LLUUID();
	}

    /// <summary>
    /// A three-dimensional vector with floating-point values
    /// </summary>
    [Serializable]
	public struct LLVector3
	{
        /// <summary>X value</summary>
        [XmlAttribute]
        public float X;
		/// <summary>Y value</summary>
        [XmlAttribute]
        public float Y;
        /// <summary>Z value</summary>
        [XmlAttribute]
        public float Z;

        /// <summary>
        /// Constructor, builds a single-precision vector from a 
        /// double-precision one
        /// </summary>
        /// <param name="vector">A double-precision vector</param>
		public LLVector3(LLVector3d vector)
		{
			X = (float)vector.X;
			Y = (float)vector.Y;
			Z = (float)vector.Z;
		}

        /// <summary>
        /// Constructor, builds a vector from a byte array
        /// </summary>
        /// <param name="byteArray">Byte array containing a 12 byte vector</param>
        /// <param name="pos">Beginning position in the byte array</param>
		public LLVector3(byte[] byteArray, int pos)
		{
            if (!BitConverter.IsLittleEndian)
            {
                byte[] newArray = new byte[12];
                Buffer.BlockCopy(byteArray, pos, newArray, 0, 12);

                Array.Reverse(newArray, 0, 4);
                Array.Reverse(newArray, 4, 4);
                Array.Reverse(newArray, 8, 4);

				X = BitConverter.ToSingle(newArray, 0);
                Y = BitConverter.ToSingle(newArray, 4);
                Z = BitConverter.ToSingle(newArray, 8);
            }
            else
            {
                X = BitConverter.ToSingle(byteArray, pos);
                Y = BitConverter.ToSingle(byteArray, pos + 4);
                Z = BitConverter.ToSingle(byteArray, pos + 8);
            }
		}

        /// <summary>
        /// Constructor, builds a vector for individual float values
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        /// <param name="z">Z value</param>
		public LLVector3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

        /// <summary>
        /// Returns the raw bytes for this vector
        /// </summary>
        /// <returns>A 12 byte array containing X, Y, and Z</returns>
		public byte[] GetBytes()
		{
			byte[] byteArray = new byte[12];

            Buffer.BlockCopy(BitConverter.GetBytes(X), 0, byteArray, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Y), 0, byteArray, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Z), 0, byteArray, 8, 4);

			if(!BitConverter.IsLittleEndian) {
				Array.Reverse(byteArray, 0, 4);
				Array.Reverse(byteArray, 4, 4);
				Array.Reverse(byteArray, 8, 4);
			}

			return byteArray;
		}

        /// <summary>
        /// Get the distance to point
        /// </summary>
        /// <param name="Pt"></param>
        /// <returns></returns>
        public double GetDistanceTo(LLVector3 Pt)
        {
            return Math.Sqrt(((X - Pt.X) * (X - Pt.X)) + ((Y - Pt.Y) * (Y - Pt.Y)) + ((Z - Pt.Z) * (Z - Pt.Z)));
        }

        /// <summary>
        /// Get a formatted string representation of the vector
        /// </summary>
        /// <returns>A string representation of the vector, similar to the LSL
        /// vector to string conversion in Second Life</returns>
		public override string ToString()
		{
            return String.Format("<{0}, {1}, {2}>", X, Y, Z);
		}

        /// <summary>
        /// A hash of the vector, used by .NET for hash tables
        /// </summary>
        /// <returns>The hashes of the individual components XORed together</returns>
		public override int GetHashCode()
		{
			return (X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode());
		}

        /// <summary>
        /// Generate an LLVector3 from a string
        /// </summary>
        /// <param name="val">A string representation of a 3D vector, enclosed 
        /// in arrow brackets and separated by commas</param>
        public static LLVector3 Parse(string val)
        {
            char[] splitChar = { ',', ' ' };
            string[] split = val.Replace("<","").Replace(">","").Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
            return new LLVector3(float.Parse(split[0].Trim()), float.Parse(split[1].Trim()), float.Parse(split[2].Trim()));
        }

        public static bool TryParse(string val, out LLVector3 result)
        {
            try
            {
                result = Parse(val);
                return true;
            }
            catch (Exception)
            {
                result = new LLVector3();
                return false;
            }
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
            // If both are null, or both are same instance, return true
            if (System.Object.ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)lhs == null) || ((object)rhs == null))
            {
                return false;
            }

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
        /// 
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static LLVector3 operator *(LLVector3 vec, float val)
        {
            return new LLVector3(vec.X * val, vec.Y * val, vec.Z * val);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static LLVector3 operator *(float val, LLVector3 vec)
        {
            return new LLVector3(vec.X * val, vec.Y * val, vec.Z * val);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static LLVector3 operator *(LLVector3 lhs, LLVector3 rhs)
        {
            return new LLVector3(lhs.X * rhs.X, lhs.Y * rhs.Y, lhs.Z * rhs.Z);
        }

        public static LLVector3 operator *(LLVector3 vec, LLQuaternion quat)
        {
            LLQuaternion vq = new LLQuaternion(vec.X, vec.Y, vec.Z, 0);
            LLQuaternion nq = new LLQuaternion(-quat.X, -quat.Y, -quat.Z, quat.W);

            LLQuaternion result = (quat * vq) * nq;

            return new LLVector3(result.X, result.Y, result.Z);
        }

        /// <summary>
        /// An LLVector3 with a value of 0,0,0
        /// </summary>
        public readonly static LLVector3 Zero = new LLVector3();
	}

    /// <summary>
    /// A double-precision three-dimensional vector
    /// </summary>
    [Serializable]
	public struct LLVector3d
	{
        /// <summary>X value</summary>
        [XmlAttribute]
        public double X;
        /// <summary>Y value</summary>
        [XmlAttribute]
        public double Y;
        /// <summary>Z value</summary>
        [XmlAttribute]
        public double Z;

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
        /// Create a double precision vector from a float vector
        /// </summary>
        /// <param name="llv3"></param>
        public LLVector3d(LLVector3 llv3)
        {
            X = llv3.X;
            Y = llv3.Y;
            Z = llv3.Z;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="byteArray"></param>
        /// <param name="pos"></param>
		public LLVector3d(byte[] byteArray, int pos)
		{
            if (!BitConverter.IsLittleEndian)
            {
                byte[] newArray = new byte[24];
                Buffer.BlockCopy(byteArray, pos, newArray, 0, 24);

                Array.Reverse(newArray, 0, 8);
                Array.Reverse(newArray, 8, 8);
                Array.Reverse(newArray, 16, 8);

                X = BitConverter.ToDouble(newArray, 0);
                Y = BitConverter.ToDouble(newArray, 8);
                Z = BitConverter.ToDouble(newArray, 16);
            }
            else
            {
                X = BitConverter.ToDouble(byteArray, pos);
                Y = BitConverter.ToDouble(byteArray, pos + 8);
                Z = BitConverter.ToDouble(byteArray, pos + 16);
            }
		}

        /// <summary>
        /// A hash of the vector, used by .NET for hash tables
        /// </summary>
        /// <returns>The hashes of the individual components XORed together</returns>
        public override int GetHashCode()
        {
            return (X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public override bool Equals(object o)
        {
            if (!(o is LLVector3d)) return false;

            LLVector3d vector = (LLVector3d)o;

            return (X == vector.X && Y == vector.Y && Z == vector.Z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator ==(LLVector3d lhs, LLVector3d rhs)
        {
            // If both are null, or both are same instance, return true
            if (System.Object.ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)lhs == null) || ((object)rhs == null))
            {
                return false;
            }

            return (lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator !=(LLVector3d lhs, LLVector3d rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Get the distance to point
        /// </summary>
        /// <param name="Pt"></param>
        /// <returns></returns>
        public double GetDistanceTo(LLVector3d Pt)
        {
            return Math.Sqrt(((X - Pt.X) * (X - Pt.X)) + ((Y - Pt.Y) * (Y - Pt.Y)) + ((Z - Pt.Z) * (Z - Pt.Z)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public byte[] GetBytes()
		{
			byte[] byteArray = new byte[24];

            Buffer.BlockCopy(BitConverter.GetBytes(X), 0, byteArray, 0, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(Y), 0, byteArray, 8, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(Z), 0, byteArray, 16, 8);

			if(!BitConverter.IsLittleEndian)
            {
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
            return String.Format("<{0}, {1}, {2}>", X, Y, Z);
		}

        /// <summary>
        /// An LLVector3d with a value of 0,0,0
        /// </summary>
        public static readonly LLVector3d Zero = new LLVector3d();
	}

    /// <summary>
    /// A four-dimensional vector
    /// </summary>
    [Serializable]
	public struct LLVector4
	{
        /// <summary></summary>
        [XmlAttribute]
        public float X;
        /// <summary></summary>
        [XmlAttribute]
        public float Y;
        /// <summary></summary>
        [XmlAttribute]
        public float Z;
        /// <summary></summary>
        [XmlAttribute]
        public float S;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="byteArray"></param>
        /// <param name="pos"></param>
		public LLVector4(byte[] byteArray, int pos)
		{
            if (!BitConverter.IsLittleEndian)
            {
                byte[] newArray = new byte[16];
                Buffer.BlockCopy(byteArray, pos, newArray, 0, 16);

                Array.Reverse(newArray, 0, 4);
                Array.Reverse(newArray, 4, 4);
                Array.Reverse(newArray, 8, 4);
                Array.Reverse(newArray, 12, 4);

                X = BitConverter.ToSingle(newArray, 0);
                Y = BitConverter.ToSingle(newArray, 4);
                Z = BitConverter.ToSingle(newArray, 8);
                S = BitConverter.ToSingle(newArray, 12);
            }
            else
            {
                X = BitConverter.ToSingle(byteArray, pos);
                Y = BitConverter.ToSingle(byteArray, pos + 4);
                Z = BitConverter.ToSingle(byteArray, pos + 8);
                S = BitConverter.ToSingle(byteArray, pos + 12);
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public byte[] GetBytes()
		{
			byte[] byteArray = new byte[16];

            Buffer.BlockCopy(BitConverter.GetBytes(X), 0, byteArray, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Y), 0, byteArray, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Z), 0, byteArray, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(S), 0, byteArray, 12, 4);

			if(!BitConverter.IsLittleEndian)
            {
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
            return String.Format("<{0}, {1}, {2}, {3}>", X, Y, Z, S);
		}

        /// <summary>
        /// An LLVector4 with a value of 0,0,0,0
        /// </summary>
        public readonly static LLVector4 Zero = new LLVector4();
	}

    /// <summary>
    /// An 8-bit color structure including an alpha channel
    /// </summary>
    [Serializable]
    public struct LLColor
    {
        /// <summary>Red</summary>
        [XmlAttribute]
        public float R;
        /// <summary>Green</summary>
        [XmlAttribute]
        public float G;
        /// <summary>Blue</summary>
        [XmlAttribute]
        public float B;
        /// <summary>Alpha</summary>
        [XmlAttribute]
        public float A;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public LLColor(byte r, byte g, byte b, byte a)
        {
            float quanta = 1.0f / 255.0f;

            R = (float)r * quanta;
            G = (float)g * quanta;
            B = (float)b * quanta;
            A = (float)a * quanta;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="byteArray"></param>
        /// <param name="pos"></param>
        public LLColor(byte[] byteArray, int pos)
		{
            float quanta = 1.0f / 255.0f;

            R = (float)byteArray[pos] * quanta;
            G = (float)byteArray[pos + 1] * quanta;
            B = (float)byteArray[pos + 2] * quanta;
            A = (float)byteArray[pos + 3] * quanta;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            byte[] byteArray = new byte[4];

            byteArray[0] = Helpers.FloatToByte(R, 0.0f, 255.0f);
            byteArray[1] = Helpers.FloatToByte(G, 0.0f, 255.0f);
            byteArray[2] = Helpers.FloatToByte(B, 0.0f, 255.0f);
            byteArray[3] = Helpers.FloatToByte(A, 0.0f, 255.0f);

            return byteArray;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("<{0}, {1}, {2}, {3}>", R, G, B, A);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToStringRGB()
        {
            return String.Format("<{0}, {1}, {2}>", R, G, B);
        }

        /// <summary>
        /// An LLColor with a value of 0,0,0,255
        /// </summary>
        public readonly static LLColor Black = new LLColor(0, 0, 0, 255);
    }

    /// <summary>
    /// A quaternion, used for rotations
    /// </summary>
    [Serializable]
	public struct LLQuaternion
	{
        /// <summary>X value</summary>
        [XmlAttribute]
        public float X;
        /// <summary>Y value</summary>
        [XmlAttribute]
        public float Y;
        /// <summary>Z value</summary>
        [XmlAttribute]
        public float Z;
        /// <summary>W value</summary>
        [XmlAttribute]
        public float W;

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
                    byte[] newArray = new byte[16];
                    Buffer.BlockCopy(byteArray, pos, newArray, 0, 16);

                    Array.Reverse(newArray, 0, 4);
                    Array.Reverse(newArray, 4, 4);
                    Array.Reverse(newArray, 8, 4);
                    Array.Reverse(newArray, 12, 4);

                    X = BitConverter.ToSingle(newArray, 0);
                    Y = BitConverter.ToSingle(newArray, 4);
                    Z = BitConverter.ToSingle(newArray, 8);
                    W = BitConverter.ToSingle(newArray, 12);
                }
                else
                {
                    X = BitConverter.ToSingle(byteArray, pos);
                    Y = BitConverter.ToSingle(byteArray, pos + 4);
                    Z = BitConverter.ToSingle(byteArray, pos + 8);
                    W = BitConverter.ToSingle(byteArray, pos + 12);
                }
            }
            else
            {
                if (!BitConverter.IsLittleEndian)
                {
                    byte[] newArray = new byte[12];
                    Buffer.BlockCopy(byteArray, pos, newArray, 0, 12);

                    Array.Reverse(newArray, 0, 4);
                    Array.Reverse(newArray, 4, 4);
                    Array.Reverse(newArray, 8, 4);

                    X = BitConverter.ToSingle(newArray, 0);
                    Y = BitConverter.ToSingle(newArray, 4);
                    Z = BitConverter.ToSingle(newArray, 8);
                }
                else
                {
                    X = BitConverter.ToSingle(byteArray, pos);
                    Y = BitConverter.ToSingle(byteArray, pos + 4);
                    Z = BitConverter.ToSingle(byteArray, pos + 8);
                }

                float xyzsum = 1 - X * X - Y * Y - Z * Z;
                W = (xyzsum > 0) ? (float)Math.Sqrt(xyzsum) : 0;
            }
        }

        /// <summary>
        /// Build a quaternion from normalized float values
        /// </summary>
        /// <param name="x">X value from -1.0 to 1.0</param>
        /// <param name="y">Y value from -1.0 to 1.0</param>
        /// <param name="z">Z value from -1.0 to 1.0</param>
        public LLQuaternion(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;

            float xyzsum = 1 - X * X - Y * Y - Z * Z;
            W = (xyzsum > 0) ? (float)Math.Sqrt(xyzsum) : 0;
        }

        /// <summary>
        /// Build a quaternion from individual float values
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        /// <param name="z">Z value</param>
        /// <param name="w">W value</param>
		public LLQuaternion(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

        /// <summary>
        /// Normalize this quaternion and serialize it to a byte array
        /// </summary>
        /// <returns>A 12 byte array containing normalized X, Y, and Z floating
        /// point values in order using little endian byte ordering</returns>
		public byte[] GetBytes()
		{
            byte[] bytes = new byte[12];
            float norm;

            norm = (float)Math.Sqrt(X*X + Y*Y + Z*Z + W*W);

            if (norm != 0)
            {
                norm = 1 / norm;

                Buffer.BlockCopy(BitConverter.GetBytes(norm * X), 0, bytes, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(norm * Y), 0, bytes, 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(norm * Z), 0, bytes, 8, 4);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytes, 0, 4);
                    Array.Reverse(bytes, 4, 4);
                    Array.Reverse(bytes, 8, 4);
                }
            }
            else
            {
                throw new Exception("Quaternion " + this.ToString() + " normalized to zero");
            }

			return bytes;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public override bool Equals(object o)
        {
            if (!(o is LLQuaternion)) return false;

            LLQuaternion quaternion = (LLQuaternion)o;

            return X == quaternion.X && Y == quaternion.Y && Z == quaternion.Z && W == quaternion.W;
        }

        /// <summary>
        /// Comparison operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator ==(LLQuaternion lhs, LLQuaternion rhs)
        {
            // If both are null, or both are same instance, return true
            if (System.Object.ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)lhs == null) || ((object)rhs == null))
            {
                return false;
            }

            // Return true if the fields match:
            return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z && lhs.W == rhs.W;
        }

        /// <summary>
        /// Not comparison operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator !=(LLQuaternion lhs, LLQuaternion rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Multiplication operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static LLQuaternion operator *(LLQuaternion lhs, LLQuaternion rhs)
        {
            LLQuaternion ret = new LLQuaternion();
            ret.W = lhs.W * rhs.W - lhs.X * rhs.X - lhs.Y * rhs.Y - lhs.Z * rhs.Z;
            ret.X = lhs.W * rhs.X + lhs.X * rhs.W + lhs.Y * rhs.Z - lhs.Z * rhs.Y;
            ret.Y = lhs.W * rhs.Y + lhs.Y * rhs.W + lhs.Z * rhs.X - lhs.X * rhs.Z;
            ret.Z = lhs.W * rhs.Z + lhs.Z * rhs.W + lhs.X * rhs.Y - lhs.Y * rhs.X;
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public override string ToString()
		{
			return "<" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ", " + W.ToString() + ">";
		}

        /// <summary>
        /// An LLQuaternion with a value of 0,0,0,1
        /// </summary>
        public readonly static LLQuaternion Identity = new LLQuaternion(0, 0, 0, 1);
	}
}
