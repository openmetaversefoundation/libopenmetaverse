/*
 * Copyright (c) 2006-2008, openmetaverse.org
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
using System.Collections.Generic;
using System.Text;
using OpenMetaverse.Packets;

namespace OpenMetaverse
{
    /// <summary>
    /// Static helper functions and global variables
    /// </summary>
    public static class Helpers
    {
        /// <summary>This header flag signals that ACKs are appended to the packet</summary>
        public const byte MSG_APPENDED_ACKS = 0x10;
        /// <summary>This header flag signals that this packet has been sent before</summary>
        public const byte MSG_RESENT = 0x20;
        /// <summary>This header flags signals that an ACK is expected for this packet</summary>
        public const byte MSG_RELIABLE = 0x40;
        /// <summary>This header flag signals that the message is compressed using zerocoding</summary>
        public const byte MSG_ZEROCODED = 0x80;

        public static readonly string NewLine = Environment.NewLine;

        /// <summary>
        /// Passed to Logger.Log() to identify the severity of a log entry
        /// </summary>
        public enum LogLevel
        {
            /// <summary>No logging information will be output</summary>
            None,
            /// <summary>Non-noisy useful information, may be helpful in 
            /// debugging a problem</summary>
            Info,
            /// <summary>A non-critical error occurred. A warning will not 
            /// prevent the rest of the library from operating as usual, 
            /// although it may be indicative of an underlying issue</summary>
            Warning,
            /// <summary>A critical error has occurred. Generally this will 
            /// be followed by the network layer shutting down, although the 
            /// stability of the library after an error is uncertain</summary>
            Error,
            /// <summary>Used for internal testing, this logging level can 
            /// generate very noisy (long and/or repetitive) messages. Don't
            /// pass this to the Log() function, use DebugLog() instead.
            /// </summary>
            Debug
        };

        /// <summary>Provide a single instance of the CultureInfo class to
        /// help parsing in situations where the grid assumes an en-us 
        /// culture</summary>
        public static readonly System.Globalization.CultureInfo EnUsCulture =
            new System.Globalization.CultureInfo("en-us");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static short TEOffsetShort(float offset)
        {
            offset = Utils.Clamp(offset, -1.0f, 1.0f);
            offset *= 32767.0f;
            return (short)Math.Round(offset);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static float TEOffsetFloat(byte[] bytes, int pos)
        {
            float offset = (float)BitConverter.ToInt16(bytes, pos);
            return offset / 32767.0f;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static short TERotationShort(float rotation)
        {
            const double TWO_PI = Math.PI * 2.0d;
            double remainder = Math.IEEERemainder(rotation, TWO_PI);
            return (short)Math.Round((remainder / TWO_PI) * 32767.0d);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static float TERotationFloat(byte[] bytes, int pos)
        {
            const float TWO_PI = (float)(Math.PI * 2.0d);
            return (float)((bytes[pos] | (bytes[pos + 1] << 8)) / 32767.0f) * TWO_PI;
        }

        public static byte TEGlowByte(float glow)
        {
            return (byte)(glow * 255.0f);
        }

        public static float TEGlowFloat(byte[] bytes, int pos)
        {
            return (float)bytes[pos] / 255.0f;
        }

        /// <summary>
        /// Converts an unsigned integer to a hexadecimal string
        /// </summary>
        /// <param name="i">An unsigned integer to convert to a string</param>
        /// <returns>A hexadecimal string 10 characters long</returns>
        /// <example>0x7fffffff</example>
        public static string UIntToHexString(uint i)
        {
            return string.Format("{0:x8}", i);
        }

        /// <summary>
        /// Packs to 32-bit unsigned integers in to a 64-bit unsigned integer
        /// </summary>
        /// <param name="a">The left-hand (or X) value</param>
        /// <param name="b">The right-hand (or Y) value</param>
        /// <returns>A 64-bit integer containing the two 32-bit input values</returns>
        public static ulong UIntsToLong(uint a, uint b)
        {
            return ((ulong)a << 32) | (ulong)b;
        }

        /// <summary>
        /// Given an X/Y location in absolute (grid-relative) terms, a region
        /// handle is returned along with the local X/Y location in that region
        /// </summary>
        /// <param name="globalX">The absolute X location, a number such as 
        /// 255360.35</param>
        /// <param name="globalY">The absolute Y location, a number such as
        /// 255360.35</param>
        /// <param name="localX">The sim-local X position of the global X
        /// position, a value from 0.0 to 256.0</param>
        /// <param name="localY">The sim-local Y position of the global Y
        /// position, a value from 0.0 to 256.0</param>
        /// <returns>A 64-bit region handle that can be used to teleport to</returns>
        public static ulong GlobalPosToRegionHandle(float globalX, float globalY, out float localX, out float localY)
        {
            uint x = ((uint)globalX / 256) * 256;
            uint y = ((uint)globalY / 256) * 256;
            localX = globalX - (float)x;
            localY = globalY - (float)y;
            return UIntsToLong(x, y);
        }

        /// <summary>
        /// Unpacks two 32-bit unsigned integers from a 64-bit unsigned integer
        /// </summary>
        /// <param name="a">The 64-bit input integer</param>
        /// <param name="b">The left-hand (or X) output value</param>
        /// <param name="c">The right-hand (or Y) output value</param>
        public static void LongToUInts(ulong a, out uint b, out uint c)
        {
            b = (uint)(a >> 32);
            c = (uint)(a & 0x00000000FFFFFFFF);
        }

        /// <summary>
        /// Convert an integer to a byte array in little endian format
        /// </summary>
        /// <param name="x">The integer to convert</param>
        /// <returns>A four byte little endian array</returns>
        public static byte[] IntToBytes(int x)
        {
            byte[] bytes = new byte[4];

            bytes[0]= (byte)(x % 256);
            bytes[1] = (byte)((x >> 8) % 256);
            bytes[2] = (byte)((x >> 16) % 256);
            bytes[3] = (byte)((x >> 24) % 256);

            return bytes;
        }

        /// <summary>
        /// Convert the first four bytes starting at the given position in
        /// little endian ordering to a signed integer
        /// </summary>
        /// <param name="bytes">Byte array containing the int</param>
        /// <param name="pos">Position to start reading the int from</param>
        /// <returns>A signed integer, will be zero if an int can't be read
        /// at the given position</returns>
        public static int BytesToInt(byte[] bytes, int pos)
        {
            if (bytes.Length < pos + 4) return 0;
            return (int)(bytes[pos + 0] + (bytes[pos + 1] << 8) + (bytes[pos + 2] << 16) + (bytes[pos + 3] << 24));
        }

        /// <summary>
        /// Convert the first four bytes of the given array in little endian
        /// ordering to a signed integer
        /// </summary>
        /// <param name="bytes">An array four bytes or longer</param>
        /// <returns>A signed integer, will be zero if the array contains
        /// less than four bytes</returns>
        public static int BytesToInt(byte[] bytes)
        {
            return BytesToInt(bytes, 0);
        }

        /// <summary>
        /// Convert the first two bytes starting at the given position in
        /// little endian ordering to an unsigned short
        /// </summary>
        /// <param name="bytes">Byte array containing the ushort</param>
        /// <param name="pos">Position to start reading the ushort from</param>
        /// <returns>An unsigned short, will be zero if a ushort can't be read
        /// at the given position</returns>
        public static ushort BytesToUInt16(byte[] bytes, int pos)
        {
            if (bytes.Length <= pos + 1) return 0;
            return (ushort)(bytes[pos] + (bytes[pos + 1] << 8));
        }

        /// <summary>
        /// Convert the first four bytes starting at the given position in
        /// little endian ordering to an unsigned integer
        /// </summary>
        /// <param name="bytes">Byte array containing the uint</param>
        /// <param name="pos">Position to start reading the uint from</param>
        /// <returns>An unsigned integer, will be zero if a uint can't be read
        /// at the given position</returns>
        public static uint BytesToUInt(byte[] bytes, int pos)
        {
            if (bytes.Length < pos + 4) return 0;
            return (uint)(bytes[pos + 0] + (bytes[pos + 1] << 8) + (bytes[pos + 2] << 16) + (bytes[pos + 3] << 24));
        }

        /// <summary>
        /// Convert the first four bytes of the given array in little endian
        /// ordering to an unsigned integer
        /// </summary>
        /// <param name="bytes">An array four bytes or longer</param>
        /// <returns>An unsigned integer, will be zero if the array contains
        /// less than four bytes</returns>
        public static uint BytesToUInt(byte[] bytes)
        {
            return BytesToUInt(bytes, 0);
        }

        /// <summary>
        /// Convert the first four bytes starting at the given position in
        /// big endian ordering to an unsigned integer
        /// </summary>
        /// <param name="bytes">Byte array containing the uint</param>
        /// <param name="pos">Position to start reading the uint from</param>
        /// <returns>An unsigned integer, will be zero if a uint can't be read
        /// at the given position</returns>
        public static uint BytesToUIntBig(byte[] bytes, int pos)
        {
            if (bytes.Length < pos + 4) return 0;
            return (uint)(bytes[pos + 3] + (bytes[pos + 2] << 8) + (bytes[pos + 1] << 16) + (bytes[pos + 0] << 24));
        }

        /// <summary>
        /// Convert the first four bytes of the given array in big endian
        /// ordering to an unsigned integer
        /// </summary>
        /// <param name="bytes">An array four bytes or longer</param>
        /// <returns>An unsigned integer, will be zero if the array contains
        /// less than four bytes</returns>
        public static uint BytesToUIntBig(byte[] bytes)
        {
            return BytesToUIntBig(bytes, 0);
        }

        /// <summary>
        /// Convert the first eight bytes of the given array in little endian
        /// ordering to an unsigned 64-bit integer
        /// </summary>
        /// <param name="bytes">An array eight bytes or longer</param>
        /// <returns>An unsigned 64-bit integer, will be zero if the array
        /// contains less than eight bytes</returns>
        public static ulong BytesToUInt64(byte[] bytes)
        {
            if (bytes.Length < 8) return 0;
            return (ulong)
                ((ulong)bytes[0] +
                ((ulong)bytes[1] << 8) +
                ((ulong)bytes[2] << 16) +
                ((ulong)bytes[3] << 24) +
                ((ulong)bytes[4] << 32) +
                ((ulong)bytes[5] << 40) +
                ((ulong)bytes[6] << 48) +
                ((ulong)bytes[7] << 56));
        }

        /// <summary>
        /// Convert four bytes in little endian ordering to a floating point
        /// value
        /// </summary>
        /// <param name="bytes">Byte array containing a little ending floating
        /// point value</param>
        /// <param name="pos">Starting position of the floating point value in
        /// the byte array</param>
        /// <returns>Single precision value</returns>
        public static float BytesToFloat(byte[] bytes, int pos)
        {
            if (!BitConverter.IsLittleEndian) Array.Reverse(bytes, pos, 4);
            return BitConverter.ToSingle(bytes, pos);
        }

        /// <summary>
        /// Convert a floating point value to four bytes in little endian
        /// ordering
        /// </summary>
        /// <param name="value">A floating point value</param>
        /// <returns>A four byte array containing the value in little endian
        /// ordering</returns>
        public static byte[] FloatToBytes(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        /// <summary>
        /// Converts a floating point number to a terse string format used for
        /// transmitting numbers in wearable asset files
        /// </summary>
        /// <param name="val">Floating point number to convert to a string</param>
        /// <returns>A terse string representation of the input number</returns>
        public static string FloatToTerseString(float val)
        {
            string s = string.Format("{0:.00}", val);

            // Trim trailing zeroes
            while (s[s.Length - 1] == '0')
                s = s.Remove(s.Length - 1, 1);

            // Remove superfluous decimal places after the trim
            if (s[s.Length - 1] == '.')
                s = s.Remove(s.Length - 1, 1);
            // Remove leading zeroes after a negative sign
            else if (s[0] == '-' && s[1] == '0')
                s = s.Remove(1, 1);
            // Remove leading zeroes in positive numbers
            else if (s[0] == '0')
                s = s.Remove(0, 1);

            return s;
        }

        /// <summary>
        /// Convert a float value to a byte given a minimum and maximum range
        /// </summary>
        /// <param name="val">Value to convert to a byte</param>
        /// <param name="lower">Minimum value range</param>
        /// <param name="upper">Maximum value range</param>
        /// <returns>A single byte representing the original float value</returns>
        public static byte FloatToByte(float val, float lower, float upper)
        {
            val = Utils.Clamp(val, lower, upper);
            // Normalize the value
            val -= lower;
            val /= (upper - lower);

            return (byte)Math.Floor(val * (float)byte.MaxValue);
        }

        /// <summary>
        /// Convert a byte to a float value given a minimum and maximum range
        /// </summary>
        /// <param name="bytes">Byte array to get the byte from</param>
        /// <param name="pos">Position in the byte array the desired byte is at</param>
        /// <param name="lower">Minimum value range</param>
        /// <param name="upper">Maximum value range</param>
        /// <returns>A float value inclusively between lower and upper</returns>
        public static float ByteToFloat(byte[] bytes, int pos, float lower, float upper)
        {
            if (bytes.Length <= pos) return 0;
            return ByteToFloat(bytes[pos], lower, upper);
        }

        /// <summary>
        /// Convert a byte to a float value given a minimum and maximum range
        /// </summary>
        /// <param name="val">Byte to convert to a float value</param>
        /// <param name="lower">Minimum value range</param>
        /// <param name="upper">Maximum value range</param>
        /// <returns>A float value inclusively between lower and upper</returns>
        public static float ByteToFloat(byte val, float lower, float upper)
        {
            const float ONE_OVER_BYTEMAX = 1.0f / (float)byte.MaxValue;

            float fval = (float)val * ONE_OVER_BYTEMAX;
            float delta = (upper - lower);
            fval *= delta;
            fval += lower;

            // Test for values very close to zero
            float error = delta * ONE_OVER_BYTEMAX;
            if (Math.Abs(fval) < error)
                fval = 0.0f;

            return fval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="pos"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static float UInt16ToFloat(byte[] bytes, int pos, float lower, float upper)
        {
            ushort val = BytesToUInt16(bytes, pos);
            return UInt16ToFloat(val, lower, upper);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static float UInt16ToFloat(ushort val, float lower, float upper)
        {
            const float ONE_OVER_U16_MAX = 1.0f / 65535.0f;

            float fval = (float)val * ONE_OVER_U16_MAX;
            float delta = upper - lower;
            fval *= delta;
            fval += lower;

            // Make sure zeroes come through as zero
            float maxError = delta * ONE_OVER_U16_MAX;
            if (Math.Abs(fval) < maxError)
                fval = 0.0f;

            return fval;
        }

        /// <summary>
        /// Convert a variable length field (byte array) to a string
        /// </summary>
        /// <remarks>If the byte array has unprintable characters in it, a 
        /// hex dump will be written instead</remarks>
        /// <param name="output">The StringBuilder object to write to</param>
        /// <param name="bytes">The byte array to convert to a string</param>
        internal static void FieldToString(StringBuilder output, byte[] bytes)
        {
            FieldToString(output, bytes, String.Empty);
        }

        /// <summary>
        /// Convert a variable length field (byte array) to a string, with a
        /// field name prepended to each line of the output
        /// </summary>
        /// <remarks>If the byte array has unprintable characters in it, a 
        /// hex dump will be written instead</remarks>
        /// <param name="output">The StringBuilder object to write to</param>
        /// <param name="bytes">The byte array to convert to a string</param>
        /// <param name="fieldName">A field name to prepend to each line of output</param>
        internal static void FieldToString(StringBuilder output, byte[] bytes, string fieldName)
        {
            // Check for a common case
            if (bytes.Length == 0) return;

            bool printable = true;

            for (int i = 0; i < bytes.Length; ++i)
            {
                // Check if there are any unprintable characters in the array
                if ((bytes[i] < 0x20 || bytes[i] > 0x7E) && bytes[i] != 0x09
                    && bytes[i] != 0x0D && bytes[i] != 0x0A && bytes[i] != 0x00)
                {
                    printable = false;
                    break;
                }
            }

            if (printable)
            {
                if (fieldName.Length > 0)
                {
                    output.Append(fieldName);
                    output.Append(": ");
                }

                if (bytes[bytes.Length - 1] == 0x00)
                    output.Append(UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length - 1));
                else
                    output.Append(UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length));
            }
            else
            {
                for (int i = 0; i < bytes.Length; i += 16)
                {
                    if (i != 0)
                        output.Append('\n');
                    if (fieldName.Length > 0)
                    {
                        output.Append(fieldName);
                        output.Append(": ");
                    }

                    for (int j = 0; j < 16; j++)
                    {
                        if ((i + j) < bytes.Length)
                            output.Append(String.Format("{0:X2} ", bytes[i + j]));
                        else
                            output.Append("   ");
                    }
                }
            }
        }

        /// <summary>
        /// Decode a zerocoded byte array, used to decompress packets marked
        /// with the zerocoded flag
        /// </summary>
        /// <remarks>Any time a zero is encountered, the next byte is a count 
        /// of how many zeroes to expand. One zero is encoded with 0x00 0x01, 
        /// two zeroes is 0x00 0x02, three zeroes is 0x00 0x03, etc. The 
        /// first four bytes are copied directly to the output buffer.
        /// </remarks>
        /// <param name="src">The byte array to decode</param>
        /// <param name="srclen">The length of the byte array to decode. This 
        /// would be the length of the packet up to (but not including) any
        /// appended ACKs</param>
        /// <param name="dest">The output byte array to decode to</param>
        /// <returns>The length of the output buffer</returns>
        public static int ZeroDecode(byte[] src, int srclen, byte[] dest)
        {
            uint zerolen = 0;
            int bodylen = 0;
            uint i = 0;

            try
            {
                Buffer.BlockCopy(src, 0, dest, 0, 6);
                zerolen = 6;
                bodylen = srclen;

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

                // Copy appended ACKs
                for (; i < srclen; i++)
                {
                    dest[zerolen++] = src[i];
                }

                return (int)zerolen;
            }
            catch (Exception)
            {
                Logger.Log(String.Format("Zerodecoding error: i={0}, srclen={1}, bodylen={2}, zerolen={3}\n{4}",
                    i, srclen, bodylen, zerolen, Utils.BytesToHexString(src, srclen, null)), LogLevel.Error);
            }

            return 0;
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

            Buffer.BlockCopy(src, 0, dest, 0, 6);
            zerolen += 6;

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

        /// <summary>
        /// Calculates the CRC (cyclic redundancy check) needed to upload inventory.
        /// </summary>
        /// <param name="creationDate">Creation date</param>
        /// <param name="saleType">Sale type</param>
        /// <param name="invType">Inventory type</param>
        /// <param name="type">Type</param>
        /// <param name="assetID">Asset ID</param>
        /// <param name="groupID">Group ID</param>
        /// <param name="salePrice">Sale price</param>
        /// <param name="ownerID">Owner ID</param>
        /// <param name="creatorID">Creator ID</param>
        /// <param name="itemID">Item ID</param>
        /// <param name="folderID">Folder ID</param>
        /// <param name="everyoneMask">Everyone mask (permissions)</param>
        /// <param name="flags">Flags</param>
        /// <param name="nextOwnerMask">Next owner mask (permissions)</param>
        /// <param name="groupMask">Group mask (permissions)</param>
        /// <param name="ownerMask">Owner mask (permisions)</param>
        /// <returns>The calculated CRC</returns>
        public static uint InventoryCRC(int creationDate, byte saleType, sbyte invType, sbyte type,
            UUID assetID, UUID groupID, int salePrice, UUID ownerID, UUID creatorID,
            UUID itemID, UUID folderID, uint everyoneMask, uint flags, uint nextOwnerMask,
            uint groupMask, uint ownerMask)
        {
            uint CRC = 0;

            // IDs
            CRC += assetID.CRC(); // AssetID
            CRC += folderID.CRC(); // FolderID
            CRC += itemID.CRC(); // ItemID

            // Permission stuff
            CRC += creatorID.CRC(); // CreatorID
            CRC += ownerID.CRC(); // OwnerID
            CRC += groupID.CRC(); // GroupID

            // CRC += another 4 words which always seem to be zero -- unclear if this is a UUID or what
            CRC += ownerMask;
            CRC += nextOwnerMask;
            CRC += everyoneMask;
            CRC += groupMask;

            // The rest of the CRC fields
            CRC += flags; // Flags
            CRC += (uint)invType; // InvType
            CRC += (uint)type; // Type 
            CRC += (uint)creationDate; // CreationDate
            CRC += (uint)salePrice;    // SalePrice
            CRC += (uint)((uint)saleType * 0x07073096); // SaleType

            return CRC;
        }

        /// <summary>
        /// Attempts to load a file embedded in the assembly
        /// </summary>
        /// <param name="resourceName">The filename of the resource to load</param>
        /// <returns>A Stream for the requested file, or null if the resource
        /// was not successfully loaded</returns>
        public static System.IO.Stream GetResourceStream(string resourceName)
        {
            return GetResourceStream(resourceName, Settings.RESOURCE_DIR);
        }
        
        /// <summary>
        /// Attempts to load a file either embedded in the assembly or found in
        /// a given search path
        /// </summary>
        /// <param name="resourceName">The filename of the resource to load</param>
        /// <param name="searchPath">An optional path that will be searched if
        /// the asset is not found embedded in the assembly</param>
        /// <returns>A Stream for the requested file, or null if the resource
        /// was not successfully loaded</returns>
        public static System.IO.Stream GetResourceStream(string resourceName, string searchPath)
        {
            try
            {
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
                System.IO.Stream s = a.GetManifestResourceStream("OpenMetaverse.Resources." + resourceName);
                if (s != null) return s;
            }
            catch (Exception)
            {
                // Failed to load the resource from the assembly itself
            }

            try
            {
                return new System.IO.FileStream(
                    System.IO.Path.Combine(System.IO.Path.Combine(System.Environment.CurrentDirectory, searchPath), resourceName),
                    System.IO.FileMode.Open);
            }
            catch (Exception)
            {
                // Failed to load the resource from the given path
            }

            return null;
        }

        /// <summary>
        /// Converts a list of primitives to an object that can be serialized
        /// with the LLSD system
        /// </summary>
        /// <param name="prims">Primitives to convert to a serializable object</param>
        /// <returns>An object that can be serialized with LLSD</returns>
        public static StructuredData.LLSD PrimListToLLSD(List<Primitive> prims)
        {
            StructuredData.LLSDMap map = new OpenMetaverse.StructuredData.LLSDMap(prims.Count);

            for (int i = 0; i < prims.Count; i++)
                map.Add(prims[i].LocalID.ToString(), prims[i].GetLLSD());

            return map;
        }

        /// <summary>
        /// Deserializes LLSD in to a list of primitives
        /// </summary>
        /// <param name="llsd">Structure holding the serialized primitive list,
        /// must be of the LLSDMap type</param>
        /// <returns>A list of deserialized primitives</returns>
        public static List<Primitive> LLSDToPrimList(StructuredData.LLSD llsd)
        {
            if (llsd.Type != StructuredData.LLSDType.Map)
                throw new ArgumentException("LLSD must be in the Map structure");

            StructuredData.LLSDMap map = (StructuredData.LLSDMap)llsd;
            List<Primitive> prims = new List<Primitive>(map.Count);

            foreach (KeyValuePair<string, StructuredData.LLSD> kvp in map)
            {
                Primitive prim = Primitive.FromLLSD(kvp.Value);
                prim.LocalID = UInt32.Parse(kvp.Key);
                prims.Add(prim);
            }

            return prims;
        }

        public static AttachmentPoint StateToAttachmentPoint(uint state)
        {
            const uint ATTACHMENT_MASK = 0xF0;
            uint fixedState = (((byte)state & ATTACHMENT_MASK) >> 4) | (((byte)state & ~ATTACHMENT_MASK) << 4);
            return (AttachmentPoint)fixedState;
        }
    }
}
