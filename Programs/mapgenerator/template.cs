/*
 * Copyright (c) 2006-2008, openmetaverse.org
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
    /// The header of a message template packet. Either 5, 6, or 8 bytes in
    /// length at the beginning of the packet, and encapsulates any 
    /// appended ACKs at the end of the packet as well
    /// </summary>
    public abstract class Header
    {
        /// <summary>Raw header data, does not include appended ACKs</summary>
        public byte[] Data;
        /// <summary>Raw value of the flags byte</summary>
        public byte Flags
        {
            get { return Data[0]; }
            set { Data[0] = value; }
        }
        /// <summary>Reliable flag, whether this packet requires an ACK</summary>
        public bool Reliable
        {
            get { return (Data[0] & Helpers.MSG_RELIABLE) != 0; }
            set { if (value) { Data[0] |= (byte)Helpers.MSG_RELIABLE; } else { byte mask = (byte)Helpers.MSG_RELIABLE ^ 0xFF; Data[0] &= mask; } }
        }
        /// <summary>Resent flag, whether this same packet has already been 
        /// sent</summary>
        public bool Resent
        {
            get { return (Data[0] & Helpers.MSG_RESENT) != 0; }
            set { if (value) { Data[0] |= (byte)Helpers.MSG_RESENT; } else { byte mask = (byte)Helpers.MSG_RESENT ^ 0xFF; Data[0] &= mask; } }
        }
        /// <summary>Zerocoded flag, whether this packet is compressed with 
        /// zerocoding</summary>
        public bool Zerocoded
        {
            get { return (Data[0] & Helpers.MSG_ZEROCODED) != 0; }
            set { if (value) { Data[0] |= (byte)Helpers.MSG_ZEROCODED; } else { byte mask = (byte)Helpers.MSG_ZEROCODED ^ 0xFF; Data[0] &= mask; } }
        }
        /// <summary>Appended ACKs flag, whether this packet has ACKs appended
        /// to the end</summary>
        public bool AppendedAcks
        {
            get { return (Data[0] & Helpers.MSG_APPENDED_ACKS) != 0; }
            set { if (value) { Data[0] |= (byte)Helpers.MSG_APPENDED_ACKS; } else { byte mask = (byte)Helpers.MSG_APPENDED_ACKS ^ 0xFF; Data[0] &= mask; } }
        }
        /// <summary>Packet sequence number</summary>
        public uint Sequence
        {
            get { return (uint)((Data[1] << 24) + (Data[2] << 16) + (Data[3] << 8) + Data[4]); }
            set
            {
			    Data[1] = (byte)(value >> 24); Data[2] = (byte)(value >> 16); 
			    Data[3] = (byte)(value >> 8);  Data[4] = (byte)(value % 256); 
		    }
        }
        /// <summary>Numeric ID number of this packet</summary>
        public abstract ushort ID { get; set; }
        /// <summary>Frequency classification of this packet, Low Medium or 
        /// High</summary>
        public abstract PacketFrequency Frequency { get; }
        /// <summary>Convert this header to a byte array, not including any
        /// appended ACKs</summary>
        public abstract void ToBytes(byte[] bytes, ref int i);
        /// <summary>Array containing all the appended ACKs of this packet</summary>
        public uint[] AckList;

        public abstract void FromBytes(byte[] bytes, ref int pos, ref int packetEnd);

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
                bytes[i++] = (byte)((ack >> 24) % 256);
                bytes[i++] = (byte)((ack >> 16) % 256);
                bytes[i++] = (byte)((ack >> 8) % 256);
                bytes[i++] = (byte)(ack % 256);
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
            if (bytes[6] == 0xFF)
            {
                if (bytes[7] == 0xFF)
                {
                    return new LowHeader(bytes, ref pos, ref packetEnd);
                }
                else
                {
                    return new MediumHeader(bytes, ref pos, ref packetEnd);
                }
            }
            else
            {
                return new HighHeader(bytes, ref pos, ref packetEnd);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="packetEnd"></param>
        protected void CreateAckList(byte[] bytes, ref int packetEnd)
        {
            if (AppendedAcks)
            {
                try
                {
                    int count = bytes[packetEnd--];
                    AckList = new uint[count];
                    
                    for (int i = 0; i < count; i++)
                    {
                        AckList[i] = (uint)(
                            (bytes[(packetEnd - i * 4) - 3] << 24) |
                            (bytes[(packetEnd - i * 4) - 2] << 16) |
                            (bytes[(packetEnd - i * 4) - 1] <<  8) |
                            (bytes[(packetEnd - i * 4)    ]));
                    }

                    packetEnd -= (count * 4);
                }
                catch (Exception)
                {
                    AckList = new uint[0];
                    throw new MalformedDataException();
                }
            }
            else
            {
                AckList = new uint[0];
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LowHeader : Header
    {
        /// <summary></summary>
        public override ushort ID
        {
            get { return (ushort)((Data[8] << 8) + Data[9]); }
            set { Data[8] = (byte)(value >> 8); Data[9] = (byte)(value % 256); }
        }
        /// <summary></summary>
        public override PacketFrequency Frequency { get { return PacketFrequency.Low; } }

        /// <summary>
        /// 
        /// </summary>
        public LowHeader()
        {
            Data = new byte[10];
            Data[6] = Data[7] = 0xFF;
            AckList = new uint[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="pos"></param>
        /// <param name="packetEnd"></param>
        public LowHeader(byte[] bytes, ref int pos, ref int packetEnd)
        {
            FromBytes(bytes, ref pos, ref packetEnd);
        }

        override public void FromBytes(byte[] bytes, ref int pos, ref int packetEnd)
        {
            if (bytes.Length < 10) { throw new MalformedDataException(); }
            Data = new byte[10];
            Buffer.BlockCopy(bytes, 0, Data, 0, 10);

            if ((bytes[0] & Helpers.MSG_ZEROCODED) != 0 && bytes[8] == 0)
            {
                if (bytes[9] == 1)
                {
                    Data[9] = bytes[10];
                }
                else
                {
                    throw new MalformedDataException();
                }
            }

            pos = 10;
            CreateAckList(bytes, ref packetEnd);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="i"></param>
        public override void ToBytes(byte[] bytes, ref int i)
        {
            Buffer.BlockCopy(Data, 0, bytes, i, 10);
            i += 10;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MediumHeader : Header
    {
        /// <summary></summary>
        public override ushort ID
        {
            get { return (ushort)Data[7]; }
            set { Data[7] = (byte)value; }
        }
        /// <summary></summary>
        public override PacketFrequency Frequency { get { return PacketFrequency.Medium; } }

        /// <summary>
        /// 
        /// </summary>
        public MediumHeader()
        {
            Data = new byte[8];
            Data[6] = 0xFF;
            AckList = new uint[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="pos"></param>
        /// <param name="packetEnd"></param>
        public MediumHeader(byte[] bytes, ref int pos, ref int packetEnd)
        {
            FromBytes(bytes, ref pos, ref packetEnd);
        }

        override public void FromBytes(byte[] bytes, ref int pos, ref int packetEnd)
        {
            if (bytes.Length < 8) { throw new MalformedDataException(); }
            Data = new byte[8];
            Buffer.BlockCopy(bytes, 0, Data, 0, 8);
            pos = 8;
            CreateAckList(bytes, ref packetEnd);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="i"></param>
        public override void ToBytes(byte[] bytes, ref int i)
        {
            Buffer.BlockCopy(Data, 0, bytes, i, 8);
            i += 8;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class HighHeader : Header
    {
        /// <summary></summary>
        public override ushort ID
        {
            get { return (ushort)Data[6]; }
            set { Data[6] = (byte)value; }
        }
        /// <summary></summary>
        public override PacketFrequency Frequency { get { return PacketFrequency.High; } }

        /// <summary>
        /// 
        /// </summary>
        public HighHeader()
        {
            Data = new byte[7];
            AckList = new uint[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="pos"></param>
        /// <param name="packetEnd"></param>
        public HighHeader(byte[] bytes, ref int pos, ref int packetEnd)
        {
            FromBytes(bytes, ref pos, ref packetEnd);
        }

        override public void FromBytes(byte[] bytes, ref int pos, ref int packetEnd)
        {
            if (bytes.Length < 7) { throw new MalformedDataException(); }
            Data = new byte[7];
            Buffer.BlockCopy(bytes, 0, Data, 0, 7);
            pos = 7;
            CreateAckList(bytes, ref packetEnd);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="i"></param>
        public override void ToBytes(byte[] bytes, ref int i)
        {
            Buffer.BlockCopy(Data, 0, bytes, i, 7);
            i += 7;
        }
    }
