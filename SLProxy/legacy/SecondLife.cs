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
using System.Collections;
using System.Threading;

namespace libsecondlife
{
    /// <summary>
    /// Main class to expose Second Life functionality to clients. All of the
    /// classes are accessible through this class.
    /// </summary>
    public class SecondLife
    {
        public ProtocolManager Protocol;
	public bool Debug;

        public SecondLife(string keywordFile, string mapFile)
        {
            Protocol = new ProtocolManager(keywordFile, mapFile, this);
	    Debug = true;
        }

        /// <summary>
        /// Send a log message to the debugging output system
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="level">From the LogLevel enum, either Info, Warning, or Error</param>
        public void Log(string message, Helpers.LogLevel level)
        {
            if (Debug)
            {
                Console.WriteLine(level.ToString() + ": " + message);
            }
        }
    }

    public class Helpers
    {
        public readonly static string VERSION = "libsecondlife 0.0.9";

        public const byte MSG_APPENDED_ACKS = 0x10;
        public const byte MSG_RESENT = 0x20;
        public const byte MSG_RELIABLE = 0x40;
        public const byte MSG_ZEROCODED = 0x80;
        public const ushort MSG_FREQ_HIGH = 0x0000;
        public const ushort MSG_FREQ_MED = 0xFF00;
        public const ushort MSG_FREQ_LOW = 0xFFFF;

        public enum LogLevel
        {
            Info,
            Warning,
            Error
        };

        /// <summary>
        /// Converting a variable length field (byte array) to a string
        /// </summary>
        /// <param name="data">The Data member of the Field class you are converting</param>
        public static string FieldToString(object data)
        {
            byte[] byteArray;

            try
            {
                byteArray = (byte[])data;
            }
            catch (Exception)
            {
                return "[object]";
            }

            return System.Text.Encoding.ASCII.GetString(byteArray).Replace("\0", "");
        }

        /// <summary>
        /// Decode a zerocoded byte array. Used to decompress packets marked
        /// with the zerocoded flag. Any time a zero is encountered, the
        /// next byte is a count of how many zeroes to expand. One zero is
        /// encoded with 0x00 0x01, two zeroes is 0x00 0x02, three zeroes is
        /// 0x00 0x03, etc. The first four bytes are copied directly to the
        /// output buffer.
        /// </summary>
        /// <param name="src">The byte array to decode</param>
        /// <param name="srclen">The length of the byte array to decode</param>
        /// <param name="dest">The output byte array to decode to</param>
        /// <returns>The length of the output buffer</returns>
        public static int ZeroDecode(byte[] src, int srclen, byte[] dest)
        {
            uint zerolen = 0;

            Array.Copy(src, 0, dest, 0, 4);
            zerolen += 4;

            int bodylen;
            if ((src[0] & MSG_APPENDED_ACKS) == 0)
            {
                bodylen = srclen;
            }
            else
            {
                bodylen = srclen - src[srclen - 1] * 4 - 1;
            }

            uint i;
            for (i = zerolen; i < bodylen; i++)
            {
                if (src[i] == 0x00)
                {
                    for (byte j = 0; j < src[i + 1]; j++)
                    {
                        dest[zerolen++] = 0x00;
                    }

                    i++;
                }
                else
                {
                    dest[zerolen++] = src[i];
                }
            }

            // HACK: Fix truncated zerocoded messages
            for (uint j = zerolen; j < zerolen + 16; j++)
            {
                dest[j] = 0;
            }
            zerolen += 16;

            // copy appended ACKs
            for (; i < srclen; i++)
            {
                dest[zerolen++] = src[i];
            }

            return (int)zerolen;
        }

        /// <summary>
        /// Decode enough of a byte array to get the packet ID.  Data before and
        /// after the packet ID is undefined.
        /// </summary>
        /// <param name="src">The byte array to decode</param>
        /// <param name="dest">The output byte array to encode to</param>
        public static void ZeroDecodeCommand(byte[] src, byte[] dest)
        {
            for (int srcPos = 4, destPos = 4; destPos < 8; ++srcPos)
            {
                if (src[srcPos] == 0x00)
                {
                    for (byte j = 0; j < src[srcPos + 1]; ++j)
                    {
                        dest[destPos++] = 0x00;
                    }

                    ++srcPos;
                }
                else
                {
                    dest[destPos++] = src[srcPos];
                }
            }
        }

        /// <summary>
        /// Encode a byte array with zerocoding. Used to compress packets marked
        /// with the zerocoded flag. Any zeroes in the array are compressed down
        /// to a single zero byte followed by a count of how many zeroes to expand
        /// out. A single zero becomes 0x00 0x01, two zeroes becomes 0x00 0x02,
        /// three zeroes becomes 0x00 0x03, etc. The first four bytes are copied
        /// directly to the output buffer.
        /// </summary>
        /// <param name="src">The byte array to encode</param>
        /// <param name="srclen">The length of the byte array to encode</param>
        /// <param name="dest">The output byte array to encode to</param>
        /// <returns>The length of the output buffer</returns>
        public static int ZeroEncode(byte[] src, int srclen, byte[] dest)
        {
            uint zerolen = 0;
            byte zerocount = 0;

            Array.Copy(src, 0, dest, 0, 4);
            zerolen += 4;

            int bodylen;
            if ((src[0] & MSG_APPENDED_ACKS) == 0)
            {
                bodylen = srclen;
            }
            else
            {
                bodylen = srclen - src[srclen - 1] * 4 - 1;
            }

            uint i;
            for (i = zerolen; i < bodylen; i++)
            {
                if (src[i] == 0x00)
                {
                    zerocount++;

                    if (zerocount == 0)
                    {
                        dest[zerolen++] = 0x00;
                        dest[zerolen++] = 0xff;
                        zerocount++;
                    }
                }
                else
                {
                    if (zerocount != 0)
                    {
                        dest[zerolen++] = 0x00;
                        dest[zerolen++] = (byte)zerocount;
                        zerocount = 0;
                    }

                    dest[zerolen++] = src[i];
                }
            }

            if (zerocount != 0)
            {
                dest[zerolen++] = 0x00;
                dest[zerolen++] = (byte)zerocount;
            }

            // copy appended ACKs
            for (; i < srclen; i++)
            {
                dest[zerolen++] = src[i];
            }

            return (int)zerolen;
        }
    }
}
