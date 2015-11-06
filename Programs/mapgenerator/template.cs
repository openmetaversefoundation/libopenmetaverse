/*
 * Copyright (c) 2006-2016, openmetaverse.co
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
using System.Text;
using OpenMetaverse;

namespace OpenMetaverse
{
    /// <summary>
    /// 
    /// </summary>
    public enum PacketFrequency : byte
    {
        /// <summary></summary>
        Low,
        /// <summary></summary>
        Medium,
        /// <summary></summary>
        High
    }
}
    
namespace OpenMetaverse.Packets
{
    /// <summary>
    /// Thrown when a packet could not be successfully deserialized
    /// </summary>
    public class MalformedDataException : ApplicationException
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MalformedDataException() { }

        /// <summary>
        /// Constructor that takes an additional error message
        /// </summary>
        /// <param name="Message">An error message to attach to this exception</param>
        public MalformedDataException(string Message)
            : base(Message)
        {
            this.Source = "Packet decoding";
        }
    }
    
    /// <summary>
    /// The header of a message template packet. Holds packet flags, sequence
    /// number, packet ID, and any ACKs that will be appended at the end of
    /// the packet
    /// </summary>
    public struct Header
    {
        public bool Reliable;
        public bool Resent;
        public bool Zerocoded;
        public bool AppendedAcks;
        public uint Sequence;
        public ushort ID;
        public PacketFrequency Frequency;
        public uint[] AckList;

        public void ToBytes(byte[] bytes, ref int i)
        {
            byte flags = 0;
            if (Reliable) flags |= Helpers.MSG_RELIABLE;
            if (Resent) flags |= Helpers.MSG_RESENT;
            if (Zerocoded) flags |= Helpers.MSG_ZEROCODED;
            if (AppendedAcks) flags |= Helpers.MSG_APPENDED_ACKS;

            // Flags
            bytes[i++] = flags;
            
            // Sequence number
            Utils.UIntToBytesBig(Sequence, bytes, i);
            i += 4;

            // Extra byte
            bytes[i++] = 0;

            // Packet ID
            switch (Frequency)
            {
                case PacketFrequency.High:
                    // 1 byte ID
                    bytes[i++] = (byte)ID;
                    break;
                case PacketFrequency.Medium:
                    // 2 byte ID
                    bytes[i++] = 0xFF;
                    bytes[i++] = (byte)ID;
                    break;
                case PacketFrequency.Low:
                    // 4 byte ID
                    bytes[i++] = 0xFF;
                    bytes[i++] = 0xFF;
                    Utils.UInt16ToBytesBig(ID, bytes, i);
                    i += 2;
                    break;
            }
        }

        public void FromBytes(byte[] bytes, ref int pos, ref int packetEnd)
        {
            this = BuildHeader(bytes, ref pos, ref packetEnd);
        }

        /// <summary>
        /// Convert the AckList to a byte array, used for packet serializing
        /// </summary>
        /// <param name="bytes">Reference to the target byte array</param>
        /// <param name="i">Beginning position to start writing to in the byte
        /// array, will be updated with the ending position of the ACK list</param>
        public void AcksToBytes(byte[] bytes, ref int i)
        {
            foreach (uint ack in AckList)
            {
                Utils.UIntToBytesBig(ack, bytes, i);
                i += 4;
            }
            if (AckList.Length > 0) { bytes[i++] = (byte)AckList.Length; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="pos"></param>
        /// <param name="packetEnd"></param>
        /// <returns></returns>
        public static Header BuildHeader(byte[] bytes, ref int pos, ref int packetEnd)
        {
            Header header;
            byte flags = bytes[pos];

            header.AppendedAcks = (flags & Helpers.MSG_APPENDED_ACKS) != 0;
            header.Reliable = (flags & Helpers.MSG_RELIABLE) != 0;
            header.Resent = (flags & Helpers.MSG_RESENT) != 0;
            header.Zerocoded = (flags & Helpers.MSG_ZEROCODED) != 0;
            header.Sequence = (uint)((bytes[pos + 1] << 24) + (bytes[pos + 2] << 16) + (bytes[pos + 3] << 8) + bytes[pos + 4]);

            // Set the frequency and packet ID number
            if (bytes[pos + 6] == 0xFF)
            {
                if (bytes[pos + 7] == 0xFF)
                {
                    header.Frequency = PacketFrequency.Low;
                    if (header.Zerocoded && bytes[pos + 8] == 0)
                        header.ID = bytes[pos + 10];
                    else
                        header.ID = (ushort)((bytes[pos + 8] << 8) + bytes[pos + 9]);
                    
                    pos += 10;
                }
                else
                {
                    header.Frequency = PacketFrequency.Medium;
                    header.ID = bytes[pos + 7];

                    pos += 8;
                }
            }
            else
            {
                header.Frequency = PacketFrequency.High;
                header.ID = bytes[pos + 6];

                pos += 7;
            }

            header.AckList = null;
            CreateAckList(ref header, bytes, ref packetEnd);

            return header;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="bytes"></param>
        /// <param name="packetEnd"></param>
        static void CreateAckList(ref Header header, byte[] bytes, ref int packetEnd)
        {
            if (header.AppendedAcks)
            {
                int count = bytes[packetEnd--];
                header.AckList = new uint[count];
                
                for (int i = 0; i < count; i++)
                {
                    header.AckList[i] = (uint)(
                        (bytes[(packetEnd - i * 4) - 3] << 24) |
                        (bytes[(packetEnd - i * 4) - 2] << 16) |
                        (bytes[(packetEnd - i * 4) - 1] <<  8) |
                        (bytes[(packetEnd - i * 4)    ]));
                }

                packetEnd -= (count * 4);
            }
        }
    }

    /// <summary>
    /// A block of data in a packet. Packets are composed of one or more blocks,
    /// each block containing one or more fields
    /// </summary>
    public abstract class PacketBlock
    {
        /// <summary>Current length of the data in this packet</summary>
        public abstract int Length { get; }

        /// <summary>
        /// Create a block from a byte array
        /// </summary>
        /// <param name="bytes">Byte array containing the serialized block</param>
        /// <param name="i">Starting position of the block in the byte array.
        /// This will point to the data after the end of the block when the
        /// call returns</param>
        public abstract void FromBytes(byte[] bytes, ref int i);

        /// <summary>
        /// Serialize this block into a byte array
        /// </summary>
        /// <param name="bytes">Byte array to serialize this block into</param>
        /// <param name="i">Starting position in the byte array to serialize to.
        /// This will point to the position directly after the end of the
        /// serialized block when the call returns</param>
        public abstract void ToBytes(byte[] bytes, ref int i);
    }
