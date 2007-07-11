/*
 * Copyright (c) 2006-2007, Second Life Reverse Engineering Team
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
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using libsecondlife.Packets;

namespace libsecondlife
{
    /// <summary>
    /// Static helper functions and global variables
    /// </summary>
    public class Helpers
    {
        /// <summary>This header flag signals that ACKs are appended to the packet</summary>
        public const byte MSG_APPENDED_ACKS = 0x10;
        /// <summary>This header flag signals that this packet has been sent before</summary>
        public const byte MSG_RESENT = 0x20;
        /// <summary>This header flags signals that an ACK is expected for this packet</summary>
        public const byte MSG_RELIABLE = 0x40;
        /// <summary>This header flag signals that the message is compressed using zerocoding</summary>
        public const byte MSG_ZEROCODED = 0x80;
        /// <summary>Used for converting degrees to radians</summary>
        public const double DEG_TO_RAD = Math.PI / 180.0d;
        /// <summary>Used for converting radians to degrees</summary>
        public const double RAD_TO_DEG = 180.0d / Math.PI;

        /// <summary>
        /// Passed to SecondLife.Log() to identify the severity of a log entry
        /// </summary>
        public enum LogLevel
        {
            /// <summary>Non-noisy useful information, may be helpful in 
            /// debugging a problem</summary>
            Info,
            /// <summary>A non-critical error occurred. A warning will not 
            /// prevent the rest of libsecondlife from operating as usual, 
            /// although it may be indicative of an underlying issue</summary>
            Warning,
            /// <summary>A critical error has occurred. Generally this will 
            /// be followed by the network layer shutting down, although the 
            /// stability of libsecondlife after an error is uncertain</summary>
            Error,
            /// <summary>Used for internal testing, this logging level can 
            /// generate very noisy (long and/or repetitive) messages. Don't
            /// pass this to the Log() function, use DebugLog() instead.
            /// </summary>
            Debug
        };

        /// <summary>Provide a single instance of the MD5 class to avoid making
        /// duplicate copies</summary>
        public static System.Security.Cryptography.MD5 MD5Builder = 
            new System.Security.Cryptography.MD5CryptoServiceProvider();

        /// <summary>Provide a single instance of the CultureInfo class to
        /// help parsing in situations where Second Life assumes an en-us 
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
            offset = Clamp(offset, -1.0f, 1.0f);
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
            float offset = (float)BitConverter.ToInt16(bytes, 0);
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
            return (ulong)(((ulong)a << 32) + (ulong)b);
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
            if (bytes.Length <= pos + 4) return 0;
            return (uint)(bytes[pos + 3] + (bytes[pos + 2] << 8) + (bytes[pos + 1] << 16) + (bytes[pos] << 24));
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
            if (bytes.Length < 4) return 0;
            return (uint)(bytes[3] + (bytes[2] << 8) + (bytes[1] << 16) + (bytes[0] << 24));
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
            if (bytes.Length <= pos + 4) return 0;
            return (uint)(bytes[pos] + (bytes[pos + 1] << 8) + (bytes[pos + 2] << 16) + (bytes[pos + 3] << 24));
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
            if (bytes.Length < 4) return 0;
            return (uint)(bytes[0] + (bytes[1] << 8) + (bytes[2] << 16) + (bytes[3] << 24));
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
                ((ulong)bytes[7] +
                ((ulong)bytes[6] << 8) +
                ((ulong)bytes[5] << 16) +
                ((ulong)bytes[4] << 24) +
                ((ulong)bytes[3] << 32) +
                ((ulong)bytes[2] << 40) +
                ((ulong)bytes[1] << 48) +
                ((ulong)bytes[0] << 56));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
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
                s = s.Remove(s.Length - 1);

            // Remove superfluous decimal places after the trim
            if (s[s.Length - 1] == '.')
                s = s.Remove(s.Length - 1);
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
            val = Clamp(val, lower, upper);
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
        /// Clamp a given value between a range
        /// </summary>
        /// <param name="val">Value to clamp</param>
        /// <param name="lower">Minimum allowable value</param>
        /// <param name="upper">Maximum allowable value</param>
        /// <returns>A value inclusively between lower and upper</returns>
        public static float Clamp(float val, float lower, float upper)
        {
            return Math.Min(Math.Max(val, lower), upper);
        }

        /// <summary>
        /// Convert a variable length field (byte array) to a UTF8 string
        /// </summary>
        /// <param name="bytes">The byte array to convert to a string</param>
        /// <returns>A UTF8 string</returns>
        public static string FieldToUTF8String(byte[] bytes)
        {
            if (bytes.Length > 0 && bytes[bytes.Length - 1] == 0x00)
                return UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length - 1);
            else
                return UTF8Encoding.UTF8.GetString(bytes);
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
                    output.Append(UTF8Encoding.UTF8.GetString(bytes));
            }
            else
            {
                for (int i = 0; i < bytes.Length; i += 16)
                {
                    if (i != 0)
                        output.Append(Environment.NewLine);
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

                    for (int j = 0; j < 16 && (i + j) < bytes.Length; j++)
                    {
                        if (bytes[i + j] >= 0x20 && bytes[i + j] < 0x7E)
                            output.Append((char)bytes[i + j]);
                        else
                            output.Append(".");
                    }
                }
            }
        }

        /// <summary>
        /// Converts a byte array to a string containing hexadecimal characters
        /// </summary>
        /// <param name="bytes">The byte array to convert to a string</param>
        /// <param name="fieldName">The name of the field to prepend to each
        /// line of the string</param>
        /// <returns>A string containing hexadecimal characters on multiple
        /// lines. Each line is prepended with the field name</returns>
        public static string FieldToHexString(byte[] bytes, string fieldName)
        {
            StringBuilder output = new StringBuilder();

            for (int i = 0; i < bytes.Length; i += 16)
            {
                if (i != 0)
                    output.Append(Environment.NewLine);

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

                for (int j = 0; j < 16 && (i + j) < bytes.Length; j++)
                {
                    if (bytes[i + j] >= 0x20 && bytes[i + j] < 0x7E)
                        output.Append(bytes[i + j]);
                    else
                        output.Append(".");
                }
            }

            return output.ToString();
        }

        ///// <summary>
        ///// Converts a string containing hexadecimal characters to a byte array
        ///// </summary>
        ///// <param name="hexString">String containing hexadecimal characters</param>
        ///// <returns>The converted byte array</returns>
        //public static byte[] HexStringToField(string hexString)
        //{
        //    string newString = "";
        //    char c;

        //    // FIXME: For each line of the string, if a colon is found
        //    // remove everything before it

        //    // remove all non A-F, 0-9, characters
        //    for (int i = 0; i < hexString.Length; i++)
        //    {
        //        c = hexString[i];
        //        if (IsHexDigit(c))
        //            newString += c;
        //    }

        //    // if odd number of characters, discard last character
        //    if (newString.Length % 2 != 0)
        //    {
        //        newString = newString.Substring(0, newString.Length - 1);
        //    }

        //    int byteLength = newString.Length / 2;
        //    byte[] bytes = new byte[byteLength];
        //    string hex;
        //    int j = 0;
        //    for (int i = 0; i < bytes.Length; i++)
        //    {
        //        hex = new String(new Char[] { newString[j], newString[j + 1] });
        //        bytes[i] = HexToByte(hex);
        //        j = j + 2;
        //    }
        //    return bytes;
        //}

        /// <summary>
        /// Convert a UTF8 string to a byte array
        /// </summary>
        /// <param name="str">The string to convert to a byte array</param>
        /// <returns>A null-terminated byte array</returns>
        public static byte[] StringToField(string str)
        {
            if (str.Length == 0) { return new byte[0]; }
            if (!str.EndsWith("\0")) { str += "\0"; }
            return System.Text.UTF8Encoding.UTF8.GetBytes(str);
        }

        /// <summary>
        /// Gets a unix timestamp for the current time
        /// </summary>
        /// <returns>An unsigned integer representing a unix timestamp for now</returns>
        public static uint GetUnixTime()
        {
            return (uint)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }

        /// <summary>
        /// Convert a UNIX timestamp to a native DateTime object
        /// </summary>
        /// <param name="timestamp">An unsigned integer representing a UNIX
        /// timestamp</param>
        /// <returns>A DateTime object containing the same time specified in
        /// the given timestamp</returns>
        public static DateTime UnixTimeToDateTime(uint timestamp)
        {
            // Make a DateTime equivalent to the UNIX Epoch
            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);

            // Add the number of seconds in our UNIX timestamp
            dateTime = dateTime.AddSeconds(timestamp);

            return dateTime;
        }

        /// <summary>
        /// Convert a native DateTime object to a UNIX timestamp
        /// </summary>
        /// <param name="time">A DateTime object you want to convert to a 
        /// timestamp</param>
        /// <returns>An unsigned integer representing a UNIX timestamp</returns>
        public static uint DateTimeToUnixTime(DateTime time)
        {
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (uint)ts.TotalSeconds;
        }

        /// <summary>
        /// Converts a vector style rotation to a quaternion
        /// </summary>
        /// <param name="a">Axis rotation, such as 0,0,90 for 90 degrees to the right</param>
        /// <returns>A quaternion representing the axes of the supplied vector</returns>
        public static LLQuaternion Axis2Rot(LLVector3 a)
        {
            if (a.X > 180) a.X -= 360; if (a.Y > 180) a.Y -= 360; if (a.Z > 180) a.Z -= 360;
            if (a.X < -180) a.X += 360; if (a.Y < -180) a.Y += 360; if (a.Z < -180) a.Z += 360;

            LLQuaternion rot = LLQuaternion.Identity;
            rot.X = (float)(a.X * DEG_TO_RAD);
            rot.Y = (float)(a.Y * DEG_TO_RAD);
            rot.Z = (float)(a.Z * DEG_TO_RAD);
            if (a.Z > 180) rot.W = 0;

            return rot;
        }

        /// <summary>
        /// Calculates the distance between two vectors
        /// </summary>
        public static float VecDist(LLVector3 pointA, LLVector3 pointB)
        {
            float xd = pointB.X - pointA.X;
            float yd = pointB.Y - pointA.Y;
            float zd = pointB.Z - pointA.Z;
            return (float)Math.Sqrt(xd * xd + yd * yd + zd * zd);
        }

        /// <summary>
        /// Calculate the magnitude of the supplied vector
        /// </summary>
        public static float VecMag(LLVector3 v)
        {
            return (float)Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        }

        /// <summary>
        /// Calculate the magnitude of the supplied quaternion
        /// </summary>
        public static float RotMag(LLQuaternion q)
        {
            return (float)Math.Sqrt(q.W * q.W + q.X * q.X + q.Y * q.Y + q.Z * q.Z);
        }

        /// <summary>
        /// Return the supplied vector in normalized form
        /// </summary>
        public static LLVector3 VecNorm(LLVector3 vector)
        {
            float mag = VecMag(vector);
            return new LLVector3(vector.X / mag, vector.Y / mag, vector.Z / mag);
        }

        /// <summary>
        /// Calculate the rotation between two vectors
        /// </summary>
        /// <param name="a">Directional vector, such as 1,0,0 for the forward face</param>
        /// <param name="b">Target vector - normalize first with VecNorm</param>
        public static LLQuaternion RotBetween(LLVector3 a, LLVector3 b)
        {
            //A and B should both be normalized
            //dotProduct is 0 if a and b are perpendicular. I think that's normal?
            float dotProduct = (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);

            LLVector3 crossProduct = new LLVector3();
            crossProduct.X = a.Y * b.Z - a.Z * b.Y;
            crossProduct.Y = a.Z * b.X - a.X * b.Z;
            crossProduct.Z = a.X * b.Y - a.Y * b.X;

            //float scalarProduct = (a.X * b.Y) + (a.Y * b.Z) + (a.Z * b.X); //not used?
            float magProduct = VecMag(a) * VecMag(b);
            double angle = Math.Acos(dotProduct / magProduct);

            LLVector3 axis = VecNorm(crossProduct);
            float s = (float)Math.Sin(angle / 2);
            return new LLQuaternion(axis.X * s, axis.Y * s, axis.Z * s, (float)Math.Cos(angle / 2));
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
            catch (Exception e)
            {
                Console.WriteLine("Zerodecoding error: " + Environment.NewLine +
                    "i=" + i + "srclen=" + srclen + ", bodylen=" + bodylen + ", zerolen=" + zerolen + Environment.NewLine +
                    FieldToHexString(src, "src") + Environment.NewLine + 
                    e.ToString());
            }

            return 0;
        }

        /// <summary>
        /// Decode enough of a byte array to get the packet ID.  Data before and
        /// after the packet ID is undefined.
        /// </summary>
        /// <param name="src">The byte array to decode</param>
        /// <param name="dest">The output byte array to encode to</param>
        public static void ZeroDecodeCommand(byte[] src, byte[] dest)
        {
            for (int srcPos = 6, destPos = 6; destPos < 10; ++srcPos)
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
            LLUUID assetID, LLUUID groupID, int salePrice, LLUUID ownerID, LLUUID creatorID,
            LLUUID itemID, LLUUID folderID, uint everyoneMask, uint flags, uint nextOwnerMask,
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

            // CRC += another 4 words which always seem to be zero -- unclear if this is a LLUUID or what
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
        /// Calculate the MD5 hash of a given string
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>An MD5 hash in string format, with $1$ prepended</returns>
        public static string MD5(string password)
        {
            StringBuilder digest = new StringBuilder();
            byte[] hash;
            lock(MD5Builder) hash = MD5Builder.ComputeHash(ASCIIEncoding.Default.GetBytes(password));

            // Convert the hash to a hex string
            foreach (byte b in hash)
            {
                digest.AppendFormat("{0:x2}", b);
            }

            return "$1$" + digest.ToString();
        }

        public static void PacketListToXml(List<Packet> packets, XmlWriter xmlWriter)
        {
            //XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            //ns.Add("", "");
            XmlSerializer serializer = new XmlSerializer(typeof(List<Packet>));
            serializer.Serialize(xmlWriter, packets);
        }

        public static void PrimListToXml(List<Primitive> list, XmlWriter xmlWriter)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Primitive>));
            serializer.Serialize(xmlWriter, list);
        }

        public static List<Primitive> PrimListFromXml(XmlReader reader)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Primitive>));
            object list = serializer.Deserialize(reader);
            return (List<Primitive>)list;
        }

        public static List<Packet> PacketListFromXml(XmlReader reader)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Packet>));
            object list = serializer.Deserialize(reader);
            return (List<Packet>)list;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public static System.IO.Stream GetResourceStream(string resourceName)
        {
        	System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
        	return a.GetManifestResourceStream("libsecondlife.Resources." + resourceName);
        }
    }
}
