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

        public static readonly byte[] EmptyBytes = Utils.EmptyBytes;

        /// <summary>Provide a single instance of the MD5 class to avoid making
        /// duplicate copies and handle thread safety</summary>
        private static readonly System.Security.Cryptography.MD5 MD5Builder =
            new System.Security.Cryptography.MD5CryptoServiceProvider();

        /// <summary>Provide a single instance of the SHA-1 class to avoid
        /// making duplicate copies and handle thread safety</summary>
        private static readonly System.Security.Cryptography.SHA1 SHA1Builder =
            new System.Security.Cryptography.SHA1CryptoServiceProvider();

        /// <summary>Provide a single instance of a random number generator
        /// to avoid making duplicate copies and handle thread safety</summary>
        private static readonly Random RNG = new Random();

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
        /// Clamp a given value between a range
        /// </summary>
        /// <param name="value">Value to clamp</param>
        /// <param name="min">Minimum allowable value</param>
        /// <param name="max">Maximum allowable value</param>
        /// <returns>A value inclusively between lower and upper</returns>
        public static int Clamp(int value, int min, int max)
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
        /// Compute the SHA-1 hash for a byte array
        /// </summary>
        /// <param name="data">Byte array to compute the hash for</param>
        /// <returns>SHA-1 hash of the input data</returns>
        public static byte[] SHA1(byte[] data)
        {
            lock (SHA1Builder)
                return SHA1Builder.ComputeHash(data);
        }

        /// <summary>
        /// Calculate the SHA1 hash of a given string
        /// </summary>
        /// <param name="value">The string to hash</param>
        /// <returns>The SHA1 hash as a string</returns>
        public static string SHA1String(string value)
        {
            StringBuilder digest = new StringBuilder(40);
            byte[] hash = SHA1(Encoding.UTF8.GetBytes(value));

            // Convert the hash to a hex string
            foreach (byte b in hash)
                digest.AppendFormat(Utils.EnUsCulture, "{0:x2}", b);

            return digest.ToString();
        }

        /// <summary>
        /// Calculate the MD5 hash of a given string
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>An MD5 hash in string format, with $1$ prepended</returns>
        public static string MD5(string password)
        {
            StringBuilder digest = new StringBuilder(32);
            byte[] hash = MD5(ASCIIEncoding.Default.GetBytes(password));

            // Convert the hash to a hex string
            foreach (byte b in hash)
                digest.AppendFormat(Utils.EnUsCulture, "{0:x2}", b);

            return "$1$" + digest.ToString();
        }

        /// <summary>
        /// Calculate the MD5 hash of a given string
        /// </summary>
        /// <param name="value">The string to hash</param>
        /// <returns>The MD5 hash as a string</returns>
        public static string MD5String(string value)
        {
            StringBuilder digest = new StringBuilder(32);
            byte[] hash = MD5(Encoding.UTF8.GetBytes(value));

            // Convert the hash to a hex string
            foreach (byte b in hash)
                digest.AppendFormat(Utils.EnUsCulture, "{0:x2}", b);

            return digest.ToString();
        }

        /// <summary>
        /// Generate a random double precision floating point value
        /// </summary>
        /// <returns>Random value of type double</returns>
        public static double RandomDouble()
        {
            lock (RNG)
                return RNG.NextDouble();
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
    }
}
