/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names
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
using System.IO;

namespace OpenMetaverse.Rendering
{
    /// <summary>
    /// Binary reader, which is endian aware
    /// </summary>
    public class EndianAwareBinaryReader : BinaryReader
    {
        /// What is the format of the source file
        public enum SourceFormat
        {
            BigEndian,                          //!< The stream is big endian, SPARC, Arm and friends 
            LittleEndian                        //!< x86 and friends
        }

        private byte[] m_a16 = new byte[2];                         //!< Temporary storage area for 2 byte values
        private byte[] m_a32 = new byte[4];                         //!< Temporary storage area for 4 byte values
        private byte[] m_a64 = new byte[8];                         //!< Temporary storage area for 8 byte values

        private readonly bool m_shouldReverseOrder;                 //!< true if the file is in a different endian format than the system

        /// <summary>
        /// Construct a reader from a stream
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        public EndianAwareBinaryReader(Stream stream)
            : this(stream, SourceFormat.LittleEndian) {}

        /// <summary>
        /// Construct a reader from a stream
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <param name="format">What is the format of the file, assumes PC and similar architecture</param>
        public EndianAwareBinaryReader(Stream stream, SourceFormat format)
            : base(stream)
        {
            if ((format == SourceFormat.BigEndian && BitConverter.IsLittleEndian) ||
                (format == SourceFormat.LittleEndian && !BitConverter.IsLittleEndian))
                m_shouldReverseOrder = true;
        }

        /// <summary>
        /// Read a 32 bit integer
        /// </summary>
        /// <returns>A 32 bit integer in the system's endianness</returns>
        public override int ReadInt32()
        {
            m_a32 = base.ReadBytes(4);
            if (m_shouldReverseOrder)
                Array.Reverse(m_a32);
            return BitConverter.ToInt32(m_a32, 0);
        }

        /// <summary>
        /// Read a 16 bit integer
        /// </summary>
        /// <returns>A 16 bit integer in the system's endianness</returns>
        public override Int16 ReadInt16()
        {
            m_a16 = base.ReadBytes(2);
            if (m_shouldReverseOrder)
                Array.Reverse(m_a16);
            return BitConverter.ToInt16(m_a16, 0);
        }

        /// <summary>
        /// Read a 64 bit integer
        /// </summary>
        /// <returns>A 64 bit integer in the system's endianness</returns>
        public override Int64 ReadInt64()
        {
            m_a64 = base.ReadBytes(8);
            if (m_shouldReverseOrder)
                Array.Reverse(m_a64);
            return BitConverter.ToInt64(m_a64, 0);
        }

        /// <summary>
        /// Read an unsigned 32 bit integer
        /// </summary>
        /// <returns>A 32 bit unsigned integer in the system's endianness</returns>
        public override UInt32 ReadUInt32()
        {
            m_a32 = base.ReadBytes(4);
            if (m_shouldReverseOrder)
                Array.Reverse(m_a32);
            return BitConverter.ToUInt32(m_a32, 0);
        }

        /// <summary>
        /// Read a single precision floating point value
        /// </summary>
        /// <returns>A single precision floating point value in the system's endianness</returns>
        public override float ReadSingle()
        {
            m_a32 = base.ReadBytes(4);
            if (m_shouldReverseOrder)
                Array.Reverse(m_a32);
            return BitConverter.ToSingle(m_a32, 0);
        }

        /// <summary>
        /// Read a double precision floating point value
        /// </summary>
        /// <returns>A double precision floating point value in the system's endianness</returns>
        public override double ReadDouble()
        {
            m_a64 = base.ReadBytes(8);
            if (m_shouldReverseOrder)
                Array.Reverse(m_a64);
            return BitConverter.ToDouble(m_a64, 0);
        }

        /// <summary>
        /// Read a UTF-8 string
        /// </summary>
        /// <returns>A standard system string</returns>
        public override string ReadString()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                byte b = ReadByte();
                while (b != 0)
                {
                    ms.WriteByte(b);
                    b = ReadByte();
                }
                return System.Text.Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Position);
            }
        }

        /// <summary>
        /// Read a UTF-8 string
        /// </summary>
        /// <param name="size">length of string to read</param>
        /// <returns>A standard system string</returns>
        public string ReadString(int size)
        {
            byte[] buffer = ReadBytes(size);
            return System.Text.Encoding.UTF8.GetString(buffer).Trim();
        }
    }
}
