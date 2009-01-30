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
using Mono.Simd.Math;

namespace OpenMetaverse
{
    public static class QuaternionfExtensions
    {
        /// <summary>
        /// Builds a quaternion object from a byte array
        /// </summary>
        /// <param name="byteArray">The source byte array</param>
        /// <param name="pos">Offset in the byte array to start reading at</param>
        /// <param name="normalized">Whether the source data is normalized or
        /// not. If this is true 12 bytes will be read, otherwise 16 bytes will
        /// be read.</param>
        public static void FromBytes(this Quaternionf quaternion, byte[] byteArray, int pos, bool normalized)
        {
            if (!normalized)
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

                    quaternion.X = BitConverter.ToSingle(conversionBuffer, 0);
                    quaternion.Y = BitConverter.ToSingle(conversionBuffer, 4);
                    quaternion.Z = BitConverter.ToSingle(conversionBuffer, 8);
                    quaternion.W = BitConverter.ToSingle(conversionBuffer, 12);
                }
                else
                {
                    // Little endian architecture
                    quaternion.X = BitConverter.ToSingle(byteArray, pos);
                    quaternion.Y = BitConverter.ToSingle(byteArray, pos + 4);
                    quaternion.Z = BitConverter.ToSingle(byteArray, pos + 8);
                    quaternion.W = BitConverter.ToSingle(byteArray, pos + 12);
                }
            }
            else
            {
                if (!BitConverter.IsLittleEndian)
                {
                    // Big endian architecture
                    byte[] conversionBuffer = new byte[16];

                    Buffer.BlockCopy(byteArray, pos, conversionBuffer, 0, 12);

                    Array.Reverse(conversionBuffer, 0, 4);
                    Array.Reverse(conversionBuffer, 4, 4);
                    Array.Reverse(conversionBuffer, 8, 4);

                    quaternion.X = BitConverter.ToSingle(conversionBuffer, 0);
                    quaternion.Y = BitConverter.ToSingle(conversionBuffer, 4);
                    quaternion.Z = BitConverter.ToSingle(conversionBuffer, 8);
                }
                else
                {
                    // Little endian architecture
                    quaternion.X = BitConverter.ToSingle(byteArray, pos);
                    quaternion.Y = BitConverter.ToSingle(byteArray, pos + 4);
                    quaternion.Z = BitConverter.ToSingle(byteArray, pos + 8);
                }

                float xyzsum = 1f - quaternion.X * quaternion.X - quaternion.Y * quaternion.Y - quaternion.Z * quaternion.Z;
                quaternion.W = (xyzsum > 0f) ? (float)Math.Sqrt(xyzsum) : 0f;
            }
        }

        /// <summary>
        /// Normalize this quaternion and serialize it to a byte array
        /// </summary>
        /// <returns>A 12 byte array containing normalized X, Y, and Z floating
        /// point values in order using little endian byte ordering</returns>
        public static byte[] GetBytes(this Quaternionf quaternion)
        {
            byte[] bytes = new byte[12];
            float norm;

            norm = (float)Math.Sqrt(quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W);

            if (norm != 0f)
            {
                norm = 1f / norm;

                float x, y, z;
                if (quaternion.W >= 0f)
                {
                    x = quaternion.X; y = quaternion.Y; z = quaternion.Z;
                }
                else
                {
                    x = -quaternion.X; y = -quaternion.Y; z = -quaternion.Z;
                }

                Buffer.BlockCopy(BitConverter.GetBytes(norm * x), 0, bytes, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(norm * y), 0, bytes, 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(norm * z), 0, bytes, 8, 4);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytes, 0, 4);
                    Array.Reverse(bytes, 4, 4);
                    Array.Reverse(bytes, 8, 4);
                }
            }
            else
            {
                throw new InvalidOperationException(String.Format(
                    "Quaternion {0} normalized to zero", quaternion.ToString()));
            }

            return bytes;
        }
    }
}
