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
    public static class Utils
    {
        /// <summary>
        /// Operating system
        /// </summary>
        public enum Platform
        {
            /// <summary>Unknown</summary>
            Unknown,
            /// <summary>Microsoft Windows</summary>
            Windows,
            /// <summary>Microsoft Windows CE</summary>
            WindowsCE,
            /// <summary>Linux</summary>
            Linux,
            /// <summary>Apple OSX</summary>
            OSX
        }

        /// <summary>
        /// Runtime platform
        /// </summary>
        public enum Runtime
        {
            /// <summary>.NET runtime</summary>
            Windows,
            /// <summary>Mono runtime: http://www.mono-project.com/</summary>
            Mono
        }

        public const float E = (float)Math.E;
        public const float LOG10E = 0.4342945f;
        public const float LOG2E = 1.442695f;
        public const float PI = (float)Math.PI;
        public const float TWO_PI = (float)(Math.PI * 2.0d);
        public const float PI_OVER_TWO = (float)(Math.PI / 2.0d);
        public const float PI_OVER_FOUR = (float)(Math.PI / 4.0d);
        /// <summary>Used for converting degrees to radians</summary>
        public const float DEG_TO_RAD = (float)(Math.PI / 180.0d);
        /// <summary>Used for converting radians to degrees</summary>
        public const float RAD_TO_DEG = (float)(180.0d / Math.PI);

        /// <summary>Provide a single instance of the CultureInfo class to
        /// help parsing in situations where the grid assumes an en-us 
        /// culture</summary>
        public static readonly System.Globalization.CultureInfo EnUsCulture =
            new System.Globalization.CultureInfo("en-us");

        /// <summary>UNIX epoch in DateTime format</summary>
        public static readonly DateTime Epoch = new DateTime(1970, 1, 1);

        /// <summary>Provide a single instance of the MD5 class to avoid making
        /// duplicate copies</summary>
        private static readonly System.Security.Cryptography.MD5 MD5Builder =
            new System.Security.Cryptography.MD5CryptoServiceProvider();

        #region Math

        /// <summary>
        /// Clamp a given value between a range
        /// </summary>
        /// <param name="value">Value to clamp</param>
        /// <param name="min">Minimum allowable value</param>
        /// <param name="max">Maximum allowable value</param>
        /// <returns>A value inclusively between lower and upper</returns>
        public static float Clamp(float value, float min, float max)
        {
            // First we check to see if we're greater than the max
            value = (value > max) ? max : value;

            // Then we check to see if we're less than the min.
            value = (value < min) ? min : value;

            // There's no check to see if min > max.
            return value;
        }

        /// <summary>
        /// Clamp a given value between a range
        /// </summary>
        /// <param name="value">Value to clamp</param>
        /// <param name="min">Minimum allowable value</param>
        /// <param name="max">Maximum allowable value</param>
        /// <returns>A value inclusively between lower and upper</returns>
        public static double Clamp(double value, double min, double max)
        {
            // First we check to see if we're greater than the max
            value = (value > max) ? max : value;

            // Then we check to see if we're less than the min.
            value = (value < min) ? min : value;

            // There's no check to see if min > max.
            return value;
        }

        /// <summary>
        /// Round a floating-point value to the nearest integer
        /// </summary>
        /// <param name="val">Floating point number to round</param>
        /// <returns>Integer</returns>
        public static int Round(float val)
        {
            return (int)Math.Floor(val + 0.5f);
        }

        /// <summary>
        /// Test if a single precision float is a finite number
        /// </summary>
        public static bool IsFinite(float value)
        {
            return !(Single.IsNaN(value) || Single.IsInfinity(value));
        }

        /// <summary>
        /// Test if a double precision float is a finite number
        /// </summary>
        public static bool IsFinite(double value)
        {
            return !(Double.IsNaN(value) || Double.IsInfinity(value));
        }

        /// <summary>
        /// Get the distance between two floating-point values
        /// </summary>
        /// <param name="value1">First value</param>
        /// <param name="value2">Second value</param>
        /// <returns>The distance between the two values</returns>
        public static float Distance(float value1, float value2)
        {
            return Math.Abs(value1 - value2);
        }

        public static float Hermite(float value1, float tangent1, float value2, float tangent2, float amount)
        {
            // All transformed to double not to lose precission
            // Otherwise, for high numbers of param:amount the result is NaN instead of Infinity
            double v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount, result;
            double sCubed = s * s * s;
            double sSquared = s * s;

            if (amount == 0f)
                result = value1;
            else if (amount == 1f)
                result = value2;
            else
                result = (2d * v1 - 2d * v2 + t2 + t1) * sCubed +
                    (3d * v2 - 3d * v1 - 2d * t1 - t2) * sSquared +
                    t1 * s + v1;
            return (float)result;
        }

        public static double Hermite(double value1, double tangent1, double value2, double tangent2, double amount)
        {
            // All transformed to double not to lose precission
            // Otherwise, for high numbers of param:amount the result is NaN instead of Infinity
            double v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount, result;
            double sCubed = s * s * s;
            double sSquared = s * s;

            if (amount == 0d)
                result = value1;
            else if (amount == 1f)
                result = value2;
            else
                result = (2d * v1 - 2d * v2 + t2 + t1) * sCubed +
                    (3d * v2 - 3d * v1 - 2d * t1 - t2) * sSquared +
                    t1 * s + v1;
            return result;
        }

        public static float Lerp(float value1, float value2, float amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        public static double Lerp(double value1, double value2, double amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        public static float SmoothStep(float value1, float value2, float amount)
        {
            // It is expected that 0 < amount < 1
            // If amount < 0, return value1
            // If amount > 1, return value2
            float result = Utils.Clamp(amount, 0f, 1f);
            return Utils.Hermite(value1, 0f, value2, 0f, result);
        }

        public static double SmoothStep(double value1, double value2, double amount)
        {
            // It is expected that 0 < amount < 1
            // If amount < 0, return value1
            // If amount > 1, return value2
            double result = Utils.Clamp(amount, 0f, 1f);
            return Utils.Hermite(value1, 0f, value2, 0f, result);
        }

        public static float ToDegrees(float radians)
        {
            // This method uses double precission internally,
            // though it returns single float
            // Factor = 180 / pi
            return (float)(radians * 57.295779513082320876798154814105);
        }

        public static float ToRadians(float degrees)
        {
            // This method uses double precission internally,
            // though it returns single float
            // Factor = pi / 180
            return (float)(degrees * 0.017453292519943295769236907684886);
        }

        /// <summary>
        /// Compute the MD5 hash for a byte array
        /// </summary>
        /// <param name="data">Byte array to compute the hash for</param>
        /// <returns>MD5 hash of the input data</returns>
        public static byte[] MD5(byte[] data)
        {
            lock (MD5Builder)
                return MD5Builder.ComputeHash(data);
        }

        /// <summary>
        /// Calculate the MD5 hash of a given string
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>An MD5 hash in string format, with $1$ prepended</returns>
        public static string MD5(string password)
        {
            StringBuilder digest = new StringBuilder();
            byte[] hash = MD5(ASCIIEncoding.Default.GetBytes(password));

            // Convert the hash to a hex string
            foreach (byte b in hash)
            {
                digest.AppendFormat(Utils.EnUsCulture, "{0:x2}", b);
            }

            return "$1$" + digest.ToString();
        }

        #endregion Math

        #region Platform

        /// <summary>
        /// Get the current running platform
        /// </summary>
        /// <returns>Enumeration of the current platform we are running on</returns>
        public static Platform GetRunningPlatform()
        {
            const string OSX_CHECK_FILE = "/Library/Extensions.kextcache";

            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                return Platform.WindowsCE;
            }
            else
            {
                int plat = (int)Environment.OSVersion.Platform;

                if ((plat != 4) && (plat != 128))
                {
                    return Platform.Windows;
                }
                else
                {
                    if (System.IO.File.Exists(OSX_CHECK_FILE))
                        return Platform.OSX;
                    else
                        return Platform.Linux;
                }
            }
        }

        /// <summary>
        /// Get the current running runtime
        /// </summary>
        /// <returns>Enumeration of the current runtime we are running on</returns>
        public static Runtime GetRunningRuntime()
        {
            Type t = Type.GetType("Mono.Runtime");
            if (t != null)
                return Runtime.Mono;
            else
                return Runtime.Windows;
        }

        #endregion Platform

        #region Conversion

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
                        output.Append(String.Format("{0:X2} ", bytes[i + j]));
                    else
                        output.Append("   ");
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
            if (str.Length == 0) { return new byte[0]; }
            if (!str.EndsWith("\0")) { str += "\0"; }
            return System.Text.UTF8Encoding.UTF8.GetBytes(str);
        }

        ///// <summary>
        ///// Converts a string containing hexadecimal characters to a byte array
        ///// </summary>
        ///// <param name="hexString">String containing hexadecimal characters</param>
        ///// <returns>The converted byte array</returns>
        //public static byte[] HexStringToBytes(string hexString)
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
            System.DateTime dateTime = Epoch;

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
            return DateTime.FromBinary(timestamp);
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

        #endregion Conversion
    }
}
