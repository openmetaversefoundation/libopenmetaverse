/*
 * Copyright (c) 2009, openmetaverse.org
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
using Mono.Simd;

namespace OpenMetaverse
{
    public static class Vector4fExtensions
    {
        /// <summary>
        /// Builds a vector from a byte array
        /// </summary>
        /// <param name="byteArray">Byte array containing a 16 byte vector</param>
        /// <param name="pos">Beginning position in the byte array</param>
        public static void FromBytes(this Vector4f vector, byte[] byteArray, int pos)
        {
            if (!BitConverter.IsLittleEndian)
            {
                // Big endian architecture
                byte[] conversionBuffer = new byte[16];

                Buffer.BlockCopy(byteArray, pos, conversionBuffer, 0, 16);

                Array.Reverse(conversionBuffer, 0, 4);
                Array.Reverse(conversionBuffer, 4, 4);
                Array.Reverse(conversionBuffer, 8, 4);
                Array.Reverse(conversionBuffer, 12, 4);

                vector.X = BitConverter.ToSingle(conversionBuffer, 0);
                vector.Y = BitConverter.ToSingle(conversionBuffer, 4);
                vector.Z = BitConverter.ToSingle(conversionBuffer, 8);
                vector.W = BitConverter.ToSingle(conversionBuffer, 12);
            }
            else
            {
                // Little endian architecture
                vector.X = BitConverter.ToSingle(byteArray, pos);
                vector.Y = BitConverter.ToSingle(byteArray, pos + 4);
                vector.Z = BitConverter.ToSingle(byteArray, pos + 8);
                vector.W = BitConverter.ToSingle(byteArray, pos + 12);
            }
        }

        /// <summary>
        /// Returns the raw bytes for this vector
        /// </summary>
        /// <returns>A 16 byte array containing X, Y, Z, and W</returns>
        public static byte[] GetBytes(this Vector4f vector)
        {
            byte[] byteArray = new byte[16];

            Buffer.BlockCopy(BitConverter.GetBytes(vector.X), 0, byteArray, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(vector.Y), 0, byteArray, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(vector.Z), 0, byteArray, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(vector.W), 0, byteArray, 12, 4);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(byteArray, 0, 4);
                Array.Reverse(byteArray, 4, 4);
                Array.Reverse(byteArray, 8, 4);
                Array.Reverse(byteArray, 12, 4);
            }

            return byteArray;
        }
    }
}
