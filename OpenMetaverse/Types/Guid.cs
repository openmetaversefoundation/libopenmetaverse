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
    public static class GuidExtensions
    {
        /// <summary>
        /// Assigns this Guid from 16 bytes out of a byte array
        /// </summary>
        /// <param name="source">Byte array containing the Guid to assign this Guid to</param>
        /// <param name="pos">Starting position of the Guid in the byte array</param>
        public static void FromBytes(this Guid guid, byte[] source, int pos)
        {
            int a = (source[pos + 0] << 24) | (source[pos + 1] << 16) | (source[pos + 2] << 8) | source[pos + 3];
            short b = (short)((source[pos + 4] << 8) | source[pos + 5]);
            short c = (short)((source[pos + 6] << 8) | source[pos + 7]);

            guid = new Guid(a, b, c, source[pos + 8], source[pos + 9], source[pos + 10], source[pos + 11],
                source[pos + 12], source[pos + 13], source[pos + 14], source[pos + 15]);
        }

        /// <summary>
        /// Returns a copy of the raw bytes for this Guid
        /// </summary>
        /// <returns>A 16 byte array containing this Guid</returns>
        public static byte[] GetBytes(this Guid guid)
        {
            byte[] bytes = guid.ToByteArray();
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

        /// <summary>
        /// Calculate a cyclic redundancy check value for this Guid
        /// </summary>
        /// <returns>The CRC checksum for this Guid</returns>
        public static uint CRC(this Guid guid)
        {
            uint retval = 0;
            byte[] bytes = guid.GetBytes();

            retval += (uint)((bytes[3] << 24) + (bytes[2] << 16) + (bytes[1] << 8) + bytes[0]);
            retval += (uint)((bytes[7] << 24) + (bytes[6] << 16) + (bytes[5] << 8) + bytes[4]);
            retval += (uint)((bytes[11] << 24) + (bytes[10] << 16) + (bytes[9] << 8) + bytes[8]);
            retval += (uint)((bytes[15] << 24) + (bytes[14] << 16) + (bytes[13] << 8) + bytes[12]);

            return retval;
        }

        /// <summary>
        /// Creates a Guid by setting the last eight bytes to the given 64-bit
        /// unsigned long
        /// </summary>
        /// <param name="value">Value to convert to a Guid</param>
        public static void FromULong(this Guid guid, ulong value)
        {
            byte[] end = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(end);

            guid = new Guid(0, 0, 0, end);
        }

        /// <summary>
        /// Create a 64-bit integer representation from the second half of this Guid
        /// </summary>
        /// <returns>An integer created from the last eight bytes of this Guid</returns>
        public static ulong GetULong(this Guid guid)
        {
            byte[] bytes = guid.ToByteArray();

            return (ulong)
                ((ulong)bytes[8] +
                ((ulong)bytes[9] << 8) +
                ((ulong)bytes[10] << 16) +
                ((ulong)bytes[11] << 24) +
                ((ulong)bytes[12] << 32) +
                ((ulong)bytes[13] << 40) +
                ((ulong)bytes[14] << 48) +
                ((ulong)bytes[15] << 56));
        }

        /// <summary>
        /// Combine two Guids together by taking the MD5 hash of a byte array
        /// containing both Guids
        /// </summary>
        /// <param name="guid">Guid to combine with the current Guid</param>
        public static void Combine(this Guid thisGuid, Guid guid)
        {
            // Construct the buffer that MD5ed
            byte[] input = new byte[32];
            Buffer.BlockCopy(thisGuid.GetBytes(), 0, input, 0, 16);
            Buffer.BlockCopy(guid.GetBytes(), 0, input, 16, 16);

            thisGuid.FromBytes(Utils.MD5(input), 0);
        }

        /// <summary>
        /// Combine two Guids with the XOR operator
        /// </summary>
        /// <param name="guid">Guid to XOR with the current Guid</param>
        public static void Xor(this Guid thisGuid, Guid guid)
        {
            byte[] lhsbytes = thisGuid.GetBytes();
            byte[] rhsbytes = guid.GetBytes();
            byte[] output = new byte[16];

            for (int i = 0; i < 16; i++)
                output[i] = (byte)(lhsbytes[i] ^ rhsbytes[i]);

            thisGuid.FromBytes(output, 0);
        }

        /// <summary>
        /// Generate a Guid from a string
        /// </summary>
        /// <param name="val">A string representation of a Guid, case 
        /// insensitive and can either be hyphenated or non-hyphenated</param>
        /// <param name="guid">Guid to generate</param>
        /// <returns>True if the string was successfully parsed, otherwise false</returns>
        public static bool TryParse(string val, out Guid guid)
        {
            try
            {
                guid = new Guid(val);
                return true;
            }
            catch (Exception)
            {
                guid = Guid.Empty;
                return false;
            }
        }
    }
}
