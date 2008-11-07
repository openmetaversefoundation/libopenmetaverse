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
using System.Globalization;
using Mono.Simd;

namespace OpenMetaverse
{
    public static class Vector4
    {
        /// <summary>A vector with a value of 0,0,0,0</summary>
        public static readonly Vector4f Zero = new Vector4f();
        /// <summary>A vector with a value of 1,1,1,1</summary>
        public static readonly Vector4f One = new Vector4f(1f, 1f, 1f, 1f);
        /// <summary>A vector with a value of -1,-1,-1,-1</summary>
        public static readonly Vector4f MinusOne = new Vector4f(-1f, -1f, -1f, -1f);
        /// <summary>A vector with a value of 1,0,0,0</summary>
        public readonly static Vector4f UnitX = new Vector4f(1f, 0f, 0f, 0f);
        /// <summary>A vector with a value of 0,1,0,0</summary>
        public readonly static Vector4f UnitY = new Vector4f(0f, 1f, 0f, 0f);
        /// <summary>A vector with a value of 0,0,1,0</summary>
        public readonly static Vector4f UnitZ = new Vector4f(0f, 0f, 1f, 0f);
        /// <summary>A vector with a value of 0,0,0,1</summary>
        public readonly static Vector4f UnitW = new Vector4f(0f, 0f, 0f, 1f);

        /// <summary>
        /// Computes the distance formula between this vector and the origin
        /// (0,0,0,0)
        /// </summary>
        public static float Length(this Vector4f vec)
        {
            return (float)Math.Sqrt(DistanceSquared(vec, Zero));
        }

        /// <summary>
        /// Computes the distance formula between this vector and the origin
        /// (0,0,0,0) without taking the square root of the result
        /// </summary>
        public static float LengthSquared(this Vector4f vec)
        {
            return DistanceSquared(vec, Zero);
        }

        /// <summary>
        /// Test if one vector is equal to another vector within a given
        /// tolerance range
        /// </summary>
        public static bool ApproxEquals(this Vector4f vec1, Vector4f vec2, float tolerance)
        {
            return (vec1 - vec2).Length() <= tolerance;
        }

        /// <summary>
        /// Test if this vector is composed of all finite numbers
        /// </summary>
        public static bool IsFinite(this Vector4f vec)
        {
            return (Utils.IsFinite(vec.X) && Utils.IsFinite(vec.Y) && Utils.IsFinite(vec.Z) && Utils.IsFinite(vec.W));
        }

        /// <summary>
        /// Returns the raw bytes for this vector
        /// </summary>
        /// <returns>A 16 byte array containing X, Y, Z, and W</returns>
        public static byte[] GetBytes(this Vector4f vec)
        {
            byte[] byteArray = new byte[16];

            Buffer.BlockCopy(BitConverter.GetBytes(vec.X), 0, byteArray, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(vec.Y), 0, byteArray, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(vec.Z), 0, byteArray, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(vec.W), 0, byteArray, 12, 4);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(byteArray, 0, 4);
                Array.Reverse(byteArray, 4, 4);
                Array.Reverse(byteArray, 8, 4);
                Array.Reverse(byteArray, 12, 4);
            }

            return byteArray;
        }

        public static Vector4f Clamp(this Vector4f vec, Vector4f min, Vector4f max)
        {
            return Vector4f.Max(Vector4f.Min(vec, min), max);
        }

        public static float Distance(this Vector4f vec1, Vector4f vec2)
        {
            Vector4f diff = vec1 - vec2;
            diff = diff * diff;
            return (float)Math.Sqrt(diff.X + diff.Y + diff.Z + diff.W);
        }

        public static float DistanceSquared(this Vector4f vec1, Vector4f vec2)
        {
            Vector4f diff = vec1 - vec2;
            diff = diff * diff;
            return diff.X + diff.Y + diff.Z + diff.W;
        }

        public static Vector4f Divide(this Vector4f vec1, float divider)
        {
            Vector4f vec2 = new Vector4f();
            vec2.X = divider;
            Vector4f.Shuffle(vec2, ShuffleSel.ExpandX);

            return vec1 / vec2;
        }

        public static float Dot(this Vector4f vec1, Vector4f vec2)
        {
            Vector4f mult = vec1 * vec2;
            return (mult.X + mult.Y + mult.Z + mult.W);
        }

        public static Vector4f Lerp(this Vector4f vec1, Vector4f vec2, float amount)
        {
            Vector4f scale = new Vector4f();
            scale.X = amount;
            Vector4f.Shuffle(scale, ShuffleSel.ExpandX);

            Vector4f lerp = vec2 - vec1;
            lerp *= scale;
            return lerp + vec1;
        }

        public static Vector4f Multiply(this Vector4f vec1, float scaleFactor)
        {
            Vector4f scale = new Vector4f();
            scale.X = scaleFactor;
            Vector4f.Shuffle(scale, ShuffleSel.ExpandX);

            return vec1 * scale;
        }

        public static Vector4f Negate(this Vector4f vec1)
        {
            return vec1 * MinusOne;
        }

        public static Vector4f Normalize(this Vector4f vec1)
        {
            const float MAG_THRESHOLD = 0.0000001f;
            float factor = DistanceSquared(vec1, Zero);

            if (factor > MAG_THRESHOLD)
            {
                Vector4f factorvec = new Vector4f();
                factorvec.X = 1f / (float)Math.Sqrt(factor);
                factorvec = Vector4f.Shuffle(factorvec, ShuffleSel.ExpandX);

                return vec1 * factorvec;
            }
            else
            {
                return Zero;
            }
        }

        public static Vector4f SmoothStep(this Vector4f vec1, Vector4f vec2, float amount)
        {
            // TODO: Convert this to SIMD instructions
            return new Vector4f(
                Utils.SmoothStep(vec1.X, vec2.X, amount),
                Utils.SmoothStep(vec1.Y, vec2.Y, amount),
                Utils.SmoothStep(vec1.Z, vec2.Z, amount),
                Utils.SmoothStep(vec1.W, vec2.W, amount));
        }

        public static Vector4f Transform2(this Vector4f position, Matrix4 matrix)
        {
            return new Vector4f(
                (position.X * matrix.M11) + (position.Y * matrix.M21) + matrix.M41,
                (position.X * matrix.M12) + (position.Y * matrix.M22) + matrix.M42,
                (position.X * matrix.M13) + (position.Y * matrix.M23) + matrix.M43,
                (position.X * matrix.M14) + (position.Y * matrix.M24) + matrix.M44);
        }

        public static Vector4f Transform3(this Vector4f position, Matrix4 matrix)
        {
            return new Vector4f(
                (position.X * matrix.M11) + (position.Y * matrix.M21) + (position.Z * matrix.M31) + matrix.M41,
                (position.X * matrix.M12) + (position.Y * matrix.M22) + (position.Z * matrix.M32) + matrix.M42,
                (position.X * matrix.M13) + (position.Y * matrix.M23) + (position.Z * matrix.M33) + matrix.M43,
                (position.X * matrix.M14) + (position.Y * matrix.M24) + (position.Z * matrix.M34) + matrix.M44);
        }

        public static Vector4f Transform(this Vector4f vector, Matrix4 matrix)
        {
            return new Vector4f(
                (vector.X * matrix.M11) + (vector.Y * matrix.M21) + (vector.Z * matrix.M31) + (vector.W * matrix.M41),
                (vector.X * matrix.M12) + (vector.Y * matrix.M22) + (vector.Z * matrix.M32) + (vector.W * matrix.M42),
                (vector.X * matrix.M13) + (vector.Y * matrix.M23) + (vector.Z * matrix.M33) + (vector.W * matrix.M43),
                (vector.X * matrix.M14) + (vector.Y * matrix.M24) + (vector.Z * matrix.M34) + (vector.W * matrix.M44));
        }
    }
}
