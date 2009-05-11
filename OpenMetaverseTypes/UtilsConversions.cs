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
using System.Text;

namespace OpenMetaverse
{
    public static partial class Utils
    {
        #region String Arrays

        private static readonly string[] _AssetTypeNames = new string[]
        {
            "texture",
	        "sound",
	        "callcard",
	        "landmark",
	        "script",
	        "clothing",
	        "object",
	        "notecard",
	        "category",
	        "root",
	        "lsltext",
	        "lslbyte",
	        "txtr_tga",
	        "bodypart",
	        "trash",
	        "snapshot",
	        "lstndfnd",
	        "snd_wav",
	        "img_tga",
	        "jpeg",
	        "animatn",
	        "gesture",
	        "simstate"
        };

        private static readonly string[] _InventoryTypeNames = new string[]
        {
            "texture",
	        "sound",
	        "callcard",
	        "landmark",
	        String.Empty,
	        String.Empty,
	        "object",
	        "notecard",
	        "category",
	        "root",
	        "script",
	        String.Empty,
	        String.Empty,
	        String.Empty,
	        String.Empty,
	        "snapshot",
	        String.Empty,
	        "attach",
	        "wearable",
	        "animation",
	        "gesture",
        };

        private static readonly string[] _SaleTypeNames = new string[]
        {
            "not",
            "orig",
            "copy",
            "cntn"
        };

        #endregion String Arrays

        #region BytesTo

        /// <summary>
        /// Convert the first two bytes starting in the byte array in
        /// little endian ordering to a signed short integer
        /// </summary>
        /// <param name="bytes">An array two bytes or longer</param>
        /// <returns>A signed short integer, will be zero if a short can't be
        /// read at the given position</returns>
        public static short BytesToInt16(byte[] bytes)
        {
            return BytesToInt16(bytes, 0);
        }

        /// <summary>
        /// Convert the first two bytes starting at the given position in
        /// little endian ordering to a signed short integer
        /// </summary>
        /// <param name="bytes">An array two bytes or longer</param>
        /// <param name="pos">Position in the array to start reading</param>
        /// <returns>A signed short integer, will be zero if a short can't be
        /// read at the given position</returns>
        public static short BytesToInt16(byte[] bytes, int pos)
        {
            if (bytes.Length <= pos + 1) return 0;
            return (short)(bytes[pos] + (bytes[pos + 1] << 8));
        }

        /// <summary>
        /// Convert the first four bytes starting at the given position in
        /// little endian ordering to a signed integer
        /// </summary>
        /// <param name="bytes">An array four bytes or longer</param>
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
        /// Convert the first eight bytes of the given array in little endian
        /// ordering to a signed long integer
        /// </summary>
        /// <param name="bytes">An array eight bytes or longer</param>
        /// <returns>A signed long integer, will be zero if the array contains
        /// less than eight bytes</returns>
        public static long BytesToInt64(byte[] bytes)
        {
            return BytesToInt64(bytes, 0);
        }

        /// <summary>
        /// Convert the first eight bytes starting at the given position in
        /// little endian ordering to a signed long integer
        /// </summary>
        /// <param name="bytes">An array eight bytes or longer</param>
        /// <param name="pos">Position to start reading the long from</param>
        /// <returns>A signed long integer, will be zero if a long can't be read
        /// at the given position</returns>
        public static long BytesToInt64(byte[] bytes, int pos)
        {
            if (bytes.Length < 8) return 0;
            return (long)
                ((long)bytes[0] +
                ((long)bytes[1] << 8) +
                ((long)bytes[2] << 16) +
                ((long)bytes[3] << 24) +
                ((long)bytes[4] << 32) +
                ((long)bytes[5] << 40) +
                ((long)bytes[6] << 48) +
                ((long)bytes[7] << 56));
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
        /// Convert two bytes in little endian ordering to an unsigned short
        /// </summary>
        /// <param name="bytes">Byte array containing the ushort</param>
        /// <returns>An unsigned short, will be zero if a ushort can't be
        /// read</returns>
        public static ushort BytesToUInt16(byte[] bytes)
        {
            return BytesToUInt16(bytes, 0);
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
            if (!BitConverter.IsLittleEndian)
            {
                byte[] newBytes = new byte[4];
                Buffer.BlockCopy(bytes, pos, newBytes, 0, 4);
                Array.Reverse(newBytes, 0, 4);
                return BitConverter.ToSingle(newBytes, 0);
            }
            else
            {
                return BitConverter.ToSingle(bytes, pos);
            }
        }

        public static double BytesToDouble(byte[] bytes, int pos)
        {
            if (!BitConverter.IsLittleEndian)
            {
                byte[] newBytes = new byte[8];
                Buffer.BlockCopy(bytes, pos, newBytes, 0, 8);
                Array.Reverse(newBytes, 0, 8);
                return BitConverter.ToDouble(newBytes, 0);
            }
            else
            {
                return BitConverter.ToDouble(bytes, pos);
            }
        }

        #endregion BytesTo

        #region ToBytes

        public static byte[] Int16ToBytes(short value)
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(value % 256);
            bytes[1] = (byte)((value >> 8) % 256);
            return bytes;
        }

        public static void Int16ToBytes(short value, byte[] dest, int pos)
        {
            dest[pos] = (byte)(value % 256);
            dest[pos + 1] = (byte)((value >> 8) % 256);
        }

        public static byte[] UInt16ToBytes(ushort value)
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(value % 256);
            bytes[1] = (byte)((value >> 8) % 256);
            return bytes;
        }

        public static void UInt16ToBytes(ushort value, byte[] dest, int pos)
        {
            dest[pos] = (byte)(value % 256);
            dest[pos + 1] = (byte)((value >> 8) % 256);
        }

        public static void UInt16ToBytesBig(ushort value, byte[] dest, int pos)
        {
            dest[pos] = (byte)((value >> 8) % 256);
            dest[pos + 1] = (byte)(value % 256);
        }

        /// <summary>
        /// Convert an integer to a byte array in little endian format
        /// </summary>
        /// <param name="value">The integer to convert</param>
        /// <returns>A four byte little endian array</returns>
        public static byte[] IntToBytes(int value)
        {
            byte[] bytes = new byte[4];

            bytes[0] = (byte)(value % 256);
            bytes[1] = (byte)((value >> 8) % 256);
            bytes[2] = (byte)((value >> 16) % 256);
            bytes[3] = (byte)((value >> 24) % 256);

            return bytes;
        }

        /// <summary>
        /// Convert an integer to a byte array in big endian format
        /// </summary>
        /// <param name="value">The integer to convert</param>
        /// <returns>A four byte big endian array</returns>
        public static byte[] IntToBytesBig(int value)
        {
            byte[] bytes = new byte[4];

            bytes[0] = (byte)((value >> 24) % 256);
            bytes[1] = (byte)((value >> 16) % 256);
            bytes[2] = (byte)((value >> 8) % 256);
            bytes[3] = (byte)(value % 256);

            return bytes;
        }

        public static void IntToBytes(int value, byte[] dest, int pos)
        {
            dest[pos] = (byte)(value % 256);
            dest[pos + 1] = (byte)((value >> 8) % 256);
            dest[pos + 2] = (byte)((value >> 16) % 256);
            dest[pos + 3] = (byte)((value >> 24) % 256);
        }

        public static byte[] UIntToBytes(uint value)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(value % 256);
            bytes[1] = (byte)((value >> 8) % 256);
            bytes[2] = (byte)((value >> 16) % 256);
            bytes[3] = (byte)((value >> 24) % 256);
            return bytes;
        }

        public static void UIntToBytes(uint value, byte[] dest, int pos)
        {
            dest[pos] = (byte)(value % 256);
            dest[pos + 1] = (byte)((value >> 8) % 256);
            dest[pos + 2] = (byte)((value >> 16) % 256);
            dest[pos + 3] = (byte)((value >> 24) % 256);
        }

        public static void UIntToBytesBig(uint value, byte[] dest, int pos)
        {
            dest[pos] = (byte)((value >> 24) % 256);
            dest[pos + 1] = (byte)((value >> 16) % 256);
            dest[pos + 2] = (byte)((value >> 8) % 256);
            dest[pos + 3] = (byte)(value % 256);
        }

        /// <summary>
        /// Convert a 64-bit integer to a byte array in little endian format
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>An 8 byte little endian array</returns>
        public static byte[] Int64ToBytes(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return bytes;
        }

        public static void Int64ToBytes(long value, byte[] dest, int pos)
        {
            byte[] bytes = Int64ToBytes(value);
            Buffer.BlockCopy(bytes, 0, dest, pos, 8);
        }

        /// <summary>
        /// Convert a 64-bit unsigned integer to a byte array in little endian
        /// format
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>An 8 byte little endian array</returns>
        public static byte[] UInt64ToBytes(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return bytes;
        }

        public static void UInt64ToBytes(ulong value, byte[] dest, int pos)
        {
            byte[] bytes = UInt64ToBytes(value);
            Buffer.BlockCopy(bytes, 0, dest, pos, 8);
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

        public static void FloatToBytes(float value, byte[] dest, int pos)
        {
            byte[] bytes = FloatToBytes(value);
            Buffer.BlockCopy(bytes, 0, dest, pos, 4);
        }

        public static byte[] DoubleToBytes(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        public static byte[] DoubleToBytesBig(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        public static void DoubleToBytes(double value, byte[] dest, int pos)
        {
            byte[] bytes = DoubleToBytes(value);
            Buffer.BlockCopy(bytes, 0, dest, pos, 8);
        }

        #endregion ToBytes

        #region Strings

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
        /// Convert a variable length UTF8 byte array to a string
        /// </summary>
        /// <param name="bytes">The UTF8 encoded byte array to convert</param>
        /// <returns>The decoded string</returns>
        public static string BytesToString(byte[] bytes)
        {
            if (bytes.Length > 0 && bytes[bytes.Length - 1] == 0x00)
                return UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length - 1);
            else
                return UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public static string BytesToString(byte[] bytes, int index, int count)
        {
            if (bytes.Length > index + count && bytes[index + count - 1] == 0x00)
                return UTF8Encoding.UTF8.GetString(bytes, index, count - 1);
            else
                return UTF8Encoding.UTF8.GetString(bytes, index, count);
        }

        /// <summary>
        /// Converts a byte array to a string containing hexadecimal characters
        /// </summary>
        /// <param name="bytes">The byte array to convert to a string</param>
        /// <param name="fieldName">The name of the field to prepend to each
        /// line of the string</param>
        /// <returns>A string containing hexadecimal characters on multiple
        /// lines. Each line is prepended with the field name</returns>
        public static string BytesToHexString(byte[] bytes, string fieldName)
        {
            return BytesToHexString(bytes, bytes.Length, fieldName);
        }

        /// <summary>
        /// Converts a byte array to a string containing hexadecimal characters
        /// </summary>
        /// <param name="bytes">The byte array to convert to a string</param>
        /// <param name="length">Number of bytes in the array to parse</param>
        /// <param name="fieldName">A string to prepend to each line of the hex
        /// dump</param>
        /// <returns>A string containing hexadecimal characters on multiple
        /// lines. Each line is prepended with the field name</returns>
        public static string BytesToHexString(byte[] bytes, int length, string fieldName)
        {
            StringBuilder output = new StringBuilder();

            for (int i = 0; i < length; i += 16)
            {
                if (i != 0)
                    output.Append('\n');

                if (!String.IsNullOrEmpty(fieldName))
                {
                    output.Append(fieldName);
                    output.Append(": ");
                }

                for (int j = 0; j < 16; j++)
                {
                    if ((i + j) < length)
                    {
                        if (j != 0)
                            output.Append(' ');

                        output.Append(String.Format("{0:X2}", bytes[i + j]));
                    }
                }
            }

            return output.ToString();
        }

        /// <summary>
        /// Convert a string to a UTF8 encoded byte array
        /// </summary>
        /// <param name="str">The string to convert</param>
        /// <returns>A null-terminated UTF8 byte array</returns>
        public static byte[] StringToBytes(string str)
        {
            if (String.IsNullOrEmpty(str)) { return Utils.EmptyBytes; }
            if (!str.EndsWith("\0")) { str += "\0"; }
            return System.Text.UTF8Encoding.UTF8.GetBytes(str);
        }

        /// <summary>
        /// Converts a string containing hexadecimal characters to a byte array
        /// </summary>
        /// <param name="hexString">String containing hexadecimal characters</param>
        /// <param name="handleDirty">If true, gracefully handles null, empty and
        /// uneven strings as well as stripping unconvertable characters</param>
        /// <returns>The converted byte array</returns>
        public static byte[] HexStringToBytes(string hexString, bool handleDirty)
        {
            if (handleDirty)
            {
                if (String.IsNullOrEmpty(hexString))
                    return Utils.EmptyBytes;

                StringBuilder stripped = new StringBuilder(hexString.Length);
                char c;

                // remove all non A-F, 0-9, characters
                for (int i = 0; i < hexString.Length; i++)
                {
                    c = hexString[i];
                    if (IsHexDigit(c))
                        stripped.Append(c);
                }

                hexString = stripped.ToString();

                // if odd number of characters, discard last character
                if (hexString.Length % 2 != 0)
                {
                    hexString = hexString.Substring(0, hexString.Length - 1);
                }
            }

            int byteLength = hexString.Length / 2;
            byte[] bytes = new byte[byteLength];
            int j = 0;

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = HexToByte(hexString.Substring(j, 2));
                j += 2;
            }

            return bytes;
        }

        /// <summary>
        /// Returns true is c is a hexadecimal digit (A-F, a-f, 0-9)
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns>true if hex digit, false if not</returns>
        private static bool IsHexDigit(Char c)
        {
            const int numA = 65;
            const int num0 = 48;

            int numChar;

            c = Char.ToUpper(c);
            numChar = Convert.ToInt32(c);

            if (numChar >= numA && numChar < (numA + 6))
                return true;
            else if (numChar >= num0 && numChar < (num0 + 10))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Converts 1 or 2 character string into equivalant byte value
        /// </summary>
        /// <param name="hex">1 or 2 character string</param>
        /// <returns>byte</returns>
        private static byte HexToByte(string hex)
        {
            if (hex.Length > 2 || hex.Length <= 0)
                throw new ArgumentException("hex must be 1 or 2 characters in length");
            byte newByte = Byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            return newByte;
        }

        #endregion Strings

        #region Packed Values

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

        public static float UInt16ToFloat(byte[] bytes, int pos, float lower, float upper)
        {
            ushort val = BytesToUInt16(bytes, pos);
            return UInt16ToFloat(val, lower, upper);
        }

        public static float UInt16ToFloat(ushort val, float lower, float upper)
        {
            const float ONE_OVER_U16_MAX = 1.0f / (float)UInt16.MaxValue;

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

        public static ushort FloatToUInt16(float value, float lower, float upper)
        {
            float delta = upper - lower;
            value -= lower;
            value /= delta;
            value *= (float)UInt16.MaxValue;

            return (ushort)value;
        }

        #endregion Packed Values

        #region TryParse

        /// <summary>
        /// Attempts to parse a floating point value from a string, using an
        /// EN-US number format
        /// </summary>
        /// <param name="s">String to parse</param>
        /// <param name="result">Resulting floating point number</param>
        /// <returns>True if the parse was successful, otherwise false</returns>
        public static bool TryParseSingle(string s, out float result)
        {
            return Single.TryParse(s, System.Globalization.NumberStyles.Float, EnUsCulture.NumberFormat, out result);
        }

        /// <summary>
        /// Attempts to parse a floating point value from a string, using an
        /// EN-US number format
        /// </summary>
        /// <param name="s">String to parse</param>
        /// <param name="result">Resulting floating point number</param>
        /// <returns>True if the parse was successful, otherwise false</returns>
        public static bool TryParseDouble(string s, out double result)
        {
            // NOTE: Double.TryParse can't parse Double.[Min/Max]Value.ToString(), see:
            // http://blogs.msdn.com/bclteam/archive/2006/05/24/598169.aspx
            return Double.TryParse(s, System.Globalization.NumberStyles.Float, EnUsCulture.NumberFormat, out result);
        }

        /// <summary>
        /// Tries to parse an unsigned 32-bit integer from a hexadecimal string
        /// </summary>
        /// <param name="s">String to parse</param>
        /// <param name="result">Resulting integer</param>
        /// <returns>True if the parse was successful, otherwise false</returns>
        public static bool TryParseHex(string s, out uint result)
        {
            return UInt32.TryParse(s, System.Globalization.NumberStyles.HexNumber, EnUsCulture.NumberFormat, out result);
        }

        #endregion TryParse

        #region Enum String Conversion

        /// <summary>
        /// Takes an AssetType and returns the string representation
        /// </summary>
        /// <param name="type">The source <seealso cref="AssetType"/></param>
        /// <returns>The string version of the AssetType</returns>
        public static string AssetTypeToString(AssetType type)
        {
            return _AssetTypeNames[(int)type];
        }

        /// <summary>
        /// Translate a string name of an AssetType into the proper Type
        /// </summary>
        /// <param name="type">A string containing the AssetType name</param>
        /// <returns>The AssetType which matches the string name, or AssetType.Unknown if no match was found</returns>
        public static AssetType StringToAssetType(string type)
        {
            for (int i = 0; i < _AssetTypeNames.Length; i++)
            {
                if (_AssetTypeNames[i] == type)
                    return (AssetType)i;
            }

            return AssetType.Unknown;
        }

        /// <summary>
        /// Convert an InventoryType to a string
        /// </summary>
        /// <param name="type">The <seealso cref="T:InventoryType"/> to convert</param>
        /// <returns>A string representation of the source</returns>
        public static string InventoryTypeToString(InventoryType type)
        {
            return _InventoryTypeNames[(int)type];
        }

        /// <summary>
        /// Convert a string into a valid InventoryType
        /// </summary>
        /// <param name="type">A string representation of the InventoryType to convert</param>
        /// <returns>A InventoryType object which matched the type</returns>
        public static InventoryType StringToInventoryType(string type)
        {
            for (int i = 0; i < _InventoryTypeNames.Length; i++)
            {
                if (_InventoryTypeNames[i] == type)
                    return (InventoryType)i;
            }

            return InventoryType.Unknown;
        }

        /// <summary>
        /// Convert a SaleType to a string
        /// </summary>
        /// <param name="type">The <seealso cref="T:SaleType"/> to convert</param>
        /// <returns>A string representation of the source</returns>
        public static string SaleTypeToString(SaleType type)
        {
            return _SaleTypeNames[(int)type];
        }

        /// <summary>
        /// Convert a string into a valid SaleType
        /// </summary>
        /// <param name="value">A string representation of the SaleType to convert</param>
        /// <returns>A SaleType object which matched the type</returns>
        public static SaleType StringToSaleType(string value)
        {
            for (int i = 0; i < _SaleTypeNames.Length; i++)
            {
                if (value == _SaleTypeNames[i])
                    return (SaleType)i;
            }

            return SaleType.Not;
        }

        #endregion Enum String Conversion

        #region Miscellaneous

        /// <summary>
        /// Copy a byte array
        /// </summary>
        /// <param name="bytes">Byte array to copy</param>
        /// <returns>A copy of the given byte array</returns>
        public static byte[] CopyBytes(byte[] bytes)
        {
            if (bytes == null)
                return null;

            byte[] newBytes = new byte[bytes.Length];
            Buffer.BlockCopy(bytes, 0, newBytes, 0, bytes.Length);
            return newBytes;
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
        /// Convert an IP address object to an unsigned 32-bit integer
        /// </summary>
        /// <param name="address">IP address to convert</param>
        /// <returns>32-bit unsigned integer holding the IP address bits</returns>
        public static uint IPToUInt(System.Net.IPAddress address)
        {
            byte[] bytes = address.GetAddressBytes();
            return (uint)((bytes[3] << 24) + (bytes[2] << 16) + (bytes[1] << 8) + bytes[0]);
        }

        /// <summary>
        /// Gets a unix timestamp for the current time
        /// </summary>
        /// <returns>An unsigned integer representing a unix timestamp for now</returns>
        public static uint GetUnixTime()
        {
            return (uint)(DateTime.UtcNow - Epoch).TotalSeconds;
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
            DateTime dateTime = Epoch;

            // Add the number of seconds in our UNIX timestamp
            dateTime = dateTime.AddSeconds(timestamp);

            return dateTime;
        }

        /// <summary>
        /// Convert a UNIX timestamp to a native DateTime object
        /// </summary>
        /// <param name="timestamp">A signed integer representing a UNIX
        /// timestamp</param>
        /// <returns>A DateTime object containing the same time specified in
        /// the given timestamp</returns>
        public static DateTime UnixTimeToDateTime(int timestamp)
        {
            return UnixTimeToDateTime((uint)timestamp);
        }

        /// <summary>
        /// Convert a native DateTime object to a UNIX timestamp
        /// </summary>
        /// <param name="time">A DateTime object you want to convert to a 
        /// timestamp</param>
        /// <returns>An unsigned integer representing a UNIX timestamp</returns>
        public static uint DateTimeToUnixTime(DateTime time)
        {
            TimeSpan ts = (time - new DateTime(1970, 1, 1, 0, 0, 0));
            return (uint)ts.TotalSeconds;
        }

        /// <summary>
        /// Swap two values
        /// </summary>
        /// <typeparam name="T">Type of the values to swap</typeparam>
        /// <param name="lhs">First value</param>
        /// <param name="rhs">Second value</param>
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        /// <summary>
        /// Try to parse an enumeration value from a string
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="strType">String value to parse</param>
        /// <param name="result">Enumeration value on success</param>
        /// <returns>True if the parsing succeeded, otherwise false</returns>
        public static bool EnumTryParse<T>(string strType, out T result)
        {
            Type t = typeof(T);

            if (Enum.IsDefined(t, strType))
            {
                result = (T)Enum.Parse(t, strType, true);
                return true;
            }
            else
            {
                foreach (string value in Enum.GetNames(typeof(T)))
                {
                    if (value.Equals(strType, StringComparison.OrdinalIgnoreCase))
                    {
                        result = (T)Enum.Parse(typeof(T), value);
                        return true;
                    }
                }
                result = default(T);
                return false;
            }
        }

        /// <summary>
        /// Swaps the high and low words in a byte. Converts aaaabbbb to bbbbaaaa
        /// </summary>
        /// <param name="value">Byte to swap the words in</param>
        /// <returns>Byte value with the words swapped</returns>
        public static byte SwapWords(byte value)
        {
            return (byte)(((value & 0xF0) >> 4) | ((value & 0x0F) << 4));
        }

        #endregion Miscellaneous
    }
}
