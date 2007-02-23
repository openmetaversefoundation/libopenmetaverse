/*
 * Copyright (c) 2007, Second Life Reverse Engineering Team
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
    /// <summary>
    /// Wrapper around a byte array that allows bit to be packed and unpacked
    /// one at a time or by a variable amount. Useful for very tightly packed
    /// data like LayerData packets
    /// </summary>
    public class BitPack
    {
        private const int MAX_BITS = 8;

        private byte[] Data;
        private int bytePos;
        private int bitPos;

        /// <summary>
        /// Default constructor, initialize the bit packer / bit unpacker
        /// with a byte array and starting position
        /// </summary>
        /// <param name="data">Byte array to pack bits in to or unpack from</param>
        /// <param name="pos">Starting position in the byte array</param>
        public BitPack(byte[] data, int pos)
        {
            Data = data;
            bytePos = pos;
        }

        /// <summary>
        /// Pack a floating point value in to the data
        /// </summary>
        /// <param name="data">Floating point value to pack</param>
        public void PackFloat(float data)
        {
            byte[] input = BitConverter.GetBytes(data);
            PackBitArray(input, 32);
        }

        /// <summary>
        /// Pack part or all of an integer in to the data
        /// </summary>
        /// <param name="data">Integer containing the data to pack</param>
        /// <param name="totalCount">Number of bits of the integer to pack</param>
        public void PackBits(int data, int totalCount)
        {
            byte[] input = BitConverter.GetBytes(data);
            PackBitArray(input, totalCount);
        }

        /// <summary>
        /// Unpacking a floating point value from the data
        /// </summary>
        /// <returns>Unpacked floating point value</returns>
        public float UnpackFloat()
        {
            byte[] output = UnpackBitsArray(32);

            if (!BitConverter.IsLittleEndian) Array.Reverse(output);
            return BitConverter.ToSingle(output, 0);
        }

        /// <summary>
        /// Unpack a variable number of bits from the data in to integer format
        /// </summary>
        /// <param name="totalCount">Number of bits to unpack</param>
        /// <returns>An integer containing the unpacked bits</returns>
        /// <remarks>This function is only useful up to 32 bits</remarks>
        public int UnpackBits(int totalCount)
        {
            byte[] output = UnpackBitsArray(totalCount);

            if (!BitConverter.IsLittleEndian) Array.Reverse(output);
            return BitConverter.ToInt32(output, 0);
        }

        private void PackBitArray(byte[] data, int totalCount)
        {
            int count = 0;
            int curBytePos = 0;
            int curBitPos = 0;

            while (totalCount > 0)
            {
                if (totalCount > MAX_BITS)
                {
                    count = MAX_BITS;
                    totalCount -= MAX_BITS;
                }
                else
                {
                    count = totalCount;
                    totalCount = 0;
                }

                while (count > 0)
                {
                    if ((data[curBytePos] & (0x80 >> curBitPos)) != 0)
                        Data[bytePos] |= (byte)(0x80 >> bitPos++);

                    --count;
                    ++curBitPos;

                    if (bitPos >= MAX_BITS)
                    {
                        bitPos = 0;
                        ++bytePos;
                    }
                    if (curBitPos >= MAX_BITS)
                    {
                        curBitPos = 0;
                        ++curBytePos;
                    }
                }
            }
        }

        private byte[] UnpackBitsArray(int totalCount)
        {
            int count = 0;
            byte[] output = new byte[4];
            int curBytePos = 0;
            int curBitPos = 0;

            while (totalCount > 0)
            {
                if (totalCount > MAX_BITS)
                {
                    count = MAX_BITS;
                    totalCount -= MAX_BITS;
                }
                else
                {
                    count = totalCount;
                    totalCount = 0;
                }

                while (count > 0)
                {
                    // Shift the previous bits
                    output[curBytePos] <<= 1;

                    // Grab one bit
                    if ((Data[bytePos] & (0x80 >> bitPos++)) != 0)
                        ++output[curBytePos];

                    --count;
                    ++curBitPos;

                    if (bitPos >= MAX_BITS)
                    {
                        bitPos = 0;
                        ++bytePos;
                    }
                    if (curBitPos >= MAX_BITS)
                    {
                        curBitPos = 0;
                        ++curBytePos;
                    }
                }
            }

            return output;
        }
    }
}
