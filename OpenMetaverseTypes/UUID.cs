/*
 * Copyright (c) 2008, openmetaverse.org
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

namespace OpenMetaverse
{
    /// <summary>
    /// A 128-bit Universally Unique Identifier, used throughout the Second
    /// Life networking protocol
    /// </summary>
    [Serializable]
    public struct UUID : IComparable<UUID>, IEquatable<UUID>
    {
        /// <summary>The System.Guid object this struct wraps around</summary>
        public Guid Guid;

        #region Constructors

        /// <summary>
        /// Constructor that takes a string UUID representation
        /// </summary>
        /// <param name="val">A string representation of a UUID, case 
        /// insensitive and can either be hyphenated or non-hyphenated</param>
        /// <example>UUID("11f8aa9c-b071-4242-836b-13b7abe0d489")</example>
        public UUID(string val)
        {
            if (String.IsNullOrEmpty(val))
                Guid = new Guid();
            else
                Guid = new Guid(val);
        }

        /// <summary>
        /// Constructor that takes a System.Guid object
        /// </summary>
        /// <param name="val">A Guid object that contains the unique identifier
        /// to be represented by this UUID</param>
        public UUID(Guid val)
        {
            Guid = val;
        }

        /// <summary>
        /// Constructor that takes a byte array containing a UUID
        /// </summary>
        /// <param name="source">Byte array containing a 16 byte UUID</param>
        /// <param name="pos">Beginning offset in the array</param>
        public UUID(byte[] source, int pos)
        {
            Guid = UUID.Zero.Guid;
            FromBytes(source, pos);
        }

        /// <summary>
        /// Constructor that takes an unsigned 64-bit unsigned integer to 
        /// convert to a UUID
        /// </summary>
        /// <param name="val">64-bit unsigned integer to convert to a UUID</param>
        public UUID(ulong val)
        {
            byte[] end = BitConverter.GetBytes(val);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(end);

            Guid = new Guid(0, 0, 0, end);
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="val">UUID to copy</param>
        public UUID(UUID val)
        {
            Guid = val.Guid;
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// IComparable.CompareTo implementation
        /// </summary>
        public int CompareTo(UUID id)
        {
            return Guid.CompareTo(id.Guid);
        }

        /// <summary>
        /// Assigns this UUID from 16 bytes out of a byte array
        /// </summary>
        /// <param name="source">Byte array containing the UUID to assign this UUID to</param>
        /// <param name="pos">Starting position of the UUID in the byte array</param>
        public void FromBytes(byte[] source, int pos)
        {
            int a = (source[pos + 0] << 24) | (source[pos + 1] << 16) | (source[pos + 2] << 8) | source[pos + 3];
            short b = (short)((source[pos + 4] << 8) | source[pos + 5]);
            short c = (short)((source[pos + 6] << 8) | source[pos + 7]);

            Guid = new Guid(a, b, c, source[pos + 8], source[pos + 9], source[pos + 10], source[pos + 11],
                source[pos + 12], source[pos + 13], source[pos + 14], source[pos + 15]);
        }

        /// <summary>
        /// Returns a copy of the raw bytes for this UUID
        /// </summary>
        /// <returns>A 16 byte array containing this UUID</returns>
        public byte[] GetBytes()
        {
            byte[] output = new byte[16];
            ToBytes(output, 0);
            return output;
        }

        /// <summary>
        /// Writes the raw bytes for this UUID to a byte array
        /// </summary>
        /// <param name="dest">Destination byte array</param>
        /// <param name="pos">Position in the destination array to start
        /// writing. Must be at least 16 bytes before the end of the array</param>
        public void ToBytes(byte[] dest, int pos)
        {
            byte[] bytes = Guid.ToByteArray();
            dest[pos + 0] = bytes[3];
            dest[pos + 1] = bytes[2];
            dest[pos + 2] = bytes[1];
            dest[pos + 3] = bytes[0];
            dest[pos + 4] = bytes[5];
            dest[pos + 5] = bytes[4];
            dest[pos + 6] = bytes[7];
            dest[pos + 7] = bytes[6];
            Buffer.BlockCopy(bytes, 8, dest, pos + 8, 8);
        }

        /// <summary>
        /// Calculate an LLCRC (cyclic redundancy check) for this UUID
        /// </summary>
        /// <returns>The CRC checksum for this UUID</returns>
        public uint CRC()
        {
            uint retval = 0;
            byte[] bytes = GetBytes();

            retval += (uint)((bytes[3] << 24) + (bytes[2] << 16) + (bytes[1] << 8) + bytes[0]);
            retval += (uint)((bytes[7] << 24) + (bytes[6] << 16) + (bytes[5] << 8) + bytes[4]);
            retval += (uint)((bytes[11] << 24) + (bytes[10] << 16) + (bytes[9] << 8) + bytes[8]);
            retval += (uint)((bytes[15] << 24) + (bytes[14] << 16) + (bytes[13] << 8) + bytes[12]);

            return retval;
        }

        /// <summary>
        /// Create a 64-bit integer representation from the second half of this UUID
        /// </summary>
        /// <returns>An integer created from the last eight bytes of this UUID</returns>
        public ulong GetULong()
        {
            byte[] bytes = Guid.ToByteArray();

            return (ulong)
                ((ulong)bytes[8] +
                ((ulong)bytes[9] << 8) +
                ((ulong)bytes[10] << 16) +
                ((ulong)bytes[12] << 24) +
                ((ulong)bytes[13] << 32) +
                ((ulong)bytes[13] << 40) +
                ((ulong)bytes[14] << 48) +
                ((ulong)bytes[15] << 56));
        }

        #endregion Public Methods

        #region Static Methods

        /// <summary>
        /// Generate a UUID from a string
        /// </summary>
        /// <param name="val">A string representation of a UUID, case 
        /// insensitive and can either be hyphenated or non-hyphenated</param>
        /// <example>UUID.Parse("11f8aa9c-b071-4242-836b-13b7abe0d489")</example>
        public static UUID Parse(string val)
        {
            return new UUID(val);
        }

        /// <summary>
        /// Generate a UUID from a string
        /// </summary>
        /// <param name="val">A string representation of a UUID, case 
        /// insensitive and can either be hyphenated or non-hyphenated</param>
        /// <param name="result">Will contain the parsed UUID if successful,
        /// otherwise null</param>
        /// <returns>True if the string was successfully parse, otherwise false</returns>
        /// <example>UUID.TryParse("11f8aa9c-b071-4242-836b-13b7abe0d489", result)</example>
        public static bool TryParse(string val, out UUID result)
        {
            if (String.IsNullOrEmpty(val) ||
                (val[0] == '{' && val.Length != 38) ||
                (val.Length != 36 && val.Length != 32))
            {
                result = UUID.Zero;
                return false;
            }

            try
            {
                result = Parse(val);
                return true;
            }
            catch (Exception)
            {
                result = UUID.Zero;
                return false;
            }
        }

        /// <summary>
        /// Combine two UUIDs together by taking the MD5 hash of a byte array
        /// containing both UUIDs
        /// </summary>
        /// <param name="first">First UUID to combine</param>
        /// <param name="second">Second UUID to combine</param>
        /// <returns>The UUID product of the combination</returns>
        public static UUID Combine(UUID first, UUID second)
        {
            // Construct the buffer that MD5ed
            byte[] input = new byte[32];
            Buffer.BlockCopy(first.GetBytes(), 0, input, 0, 16);
            Buffer.BlockCopy(second.GetBytes(), 0, input, 16, 16);

            return new UUID(Utils.MD5(input), 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UUID Random()
        {
            return new UUID(Guid.NewGuid());
        }

        #endregion Static Methods

        #region Overrides

        /// <summary>
        /// Return a hash code for this UUID, used by .NET for hash tables
        /// </summary>
        /// <returns>An integer composed of all the UUID bytes XORed together</returns>
        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

        /// <summary>
        /// Comparison function
        /// </summary>
        /// <param name="o">An object to compare to this UUID</param>
        /// <returns>True if the object is a UUID and both UUIDs are equal</returns>
        public override bool Equals(object o)
        {
            if (!(o is UUID)) return false;

            UUID uuid = (UUID)o;
            return Guid == uuid.Guid;
        }

        /// <summary>
        /// Comparison function
        /// </summary>
        /// <param name="uuid">UUID to compare to</param>
        /// <returns>True if the UUIDs are equal, otherwise false</returns>
        public bool Equals(UUID uuid)
        {
            return Guid == uuid.Guid;
        }

        /// <summary>
        /// Get a hyphenated string representation of this UUID
        /// </summary>
        /// <returns>A string representation of this UUID, lowercase and 
        /// with hyphens</returns>
        /// <example>11f8aa9c-b071-4242-836b-13b7abe0d489</example>
        public override string ToString()
        {
            return Guid.ToString();
        }

        #endregion Overrides

        #region Operators

        /// <summary>
        /// Equals operator
        /// </summary>
        /// <param name="lhs">First UUID for comparison</param>
        /// <param name="rhs">Second UUID for comparison</param>
        /// <returns>True if the UUIDs are byte for byte equal, otherwise false</returns>
        public static bool operator ==(UUID lhs, UUID rhs)
        {
            return lhs.Guid == rhs.Guid;
        }

        /// <summary>
        /// Not equals operator
        /// </summary>
        /// <param name="lhs">First UUID for comparison</param>
        /// <param name="rhs">Second UUID for comparison</param>
        /// <returns>True if the UUIDs are not equal, otherwise true</returns>
        public static bool operator !=(UUID lhs, UUID rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// XOR operator
        /// </summary>
        /// <param name="lhs">First UUID</param>
        /// <param name="rhs">Second UUID</param>
        /// <returns>A UUID that is a XOR combination of the two input UUIDs</returns>
        public static UUID operator ^(UUID lhs, UUID rhs)
        {
            byte[] lhsbytes = lhs.GetBytes();
            byte[] rhsbytes = rhs.GetBytes();
            byte[] output = new byte[16];

            for (int i = 0; i < 16; i++)
            {
                output[i] = (byte)(lhsbytes[i] ^ rhsbytes[i]);
            }

            return new UUID(output, 0);
        }

        /// <summary>
        /// String typecasting operator
        /// </summary>
        /// <param name="val">A UUID in string form. Case insensitive, 
        /// hyphenated or non-hyphenated</param>
        /// <returns>A UUID built from the string representation</returns>
        public static explicit operator UUID(string val)
        {
            return new UUID(val);
        }

        #endregion Operators

        /// <summary>An UUID with a value of all zeroes</summary>
        public static readonly UUID Zero = new UUID();
    }
}
