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
using System.Runtime.InteropServices;
using System.Globalization;

namespace OpenMetaverse
{
    /// <summary>
    /// A three-dimensional vector with floating-point values
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3 : IComparable<Vector3>, IEquatable<Vector3>
    {
        /// <summary>X value</summary>
        public float X;
        /// <summary>Y value</summary>
        public float Y;
        /// <summary>Z value</summary>
        public float Z;

        #region Constructors

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3(float value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        public Vector3(Vector2 value, float z)
        {
            X = value.X;
            Y = value.Y;
            Z = z;
        }

        public Vector3(Vector3d vector)
        {
            X = (float)vector.X;
            Y = (float)vector.Y;
            Z = (float)vector.Z;
        }

        /// <summary>
        /// Constructor, builds a vector from a byte array
        /// </summary>
        /// <param name="byteArray">Byte array containing three four-byte floats</param>
        /// <param name="pos">Beginning position in the byte array</param>
        public Vector3(byte[] byteArray, int pos)
        {
            X = Y = Z = 0f;
            FromBytes(byteArray, pos);
        }

        public Vector3(Vector3 vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }

        #endregion Constructors

        #region Public Methods

        public float Length()
        {
            return (float)Math.Sqrt(DistanceSquared(this, Zero));
        }

        public float LengthSquared()
        {
            return DistanceSquared(this, Zero);
        }

        public void Normalize()
        {
            this = Normalize(this);
        }

        /// <summary>
        /// Test if this vector is equal to another vector, within a given
        /// tolerance range
        /// </summary>
        /// <param name="vec">Vector to test against</param>
        /// <param name="tolerance">The acceptable magnitude of difference
        /// between the two vectors</param>
        /// <returns>True if the magnitude of difference between the two vectors
        /// is less than the given tolerance, otherwise false</returns>
        public bool ApproxEquals(Vector3 vec, float tolerance)
        {
            Vector3 diff = this - vec;
            return (diff.LengthSquared() <= tolerance * tolerance);
        }

        /// <summary>
        /// IComparable.CompareTo implementation
        /// </summary>
        public int CompareTo(Vector3 vector)
        {
            return Length().CompareTo(vector.Length());
        }

        /// <summary>
        /// Test if this vector is composed of all finite numbers
        /// </summary>
        public bool IsFinite()
        {
            return (Utils.IsFinite(X) && Utils.IsFinite(Y) && Utils.IsFinite(Z));
        }

        /// <summary>
        /// Builds a vector from a byte array
        /// </summary>
        /// <param name="byteArray">Byte array containing a 12 byte vector</param>
        /// <param name="pos">Beginning position in the byte array</param>
        public void FromBytes(byte[] byteArray, int pos)
        {
            if (!BitConverter.IsLittleEndian)
            {
                // Big endian architecture
                byte[] conversionBuffer = new byte[12];

                Buffer.BlockCopy(byteArray, pos, conversionBuffer, 0, 12);

                Array.Reverse(conversionBuffer, 0, 4);
                Array.Reverse(conversionBuffer, 4, 4);
                Array.Reverse(conversionBuffer, 8, 4);

                X = BitConverter.ToSingle(conversionBuffer, 0);
                Y = BitConverter.ToSingle(conversionBuffer, 4);
                Z = BitConverter.ToSingle(conversionBuffer, 8);
            }
            else
            {
                // Little endian architecture
                X = BitConverter.ToSingle(byteArray, pos);
                Y = BitConverter.ToSingle(byteArray, pos + 4);
                Z = BitConverter.ToSingle(byteArray, pos + 8);
            }
        }

        /// <summary>
        /// Returns the raw bytes for this vector
        /// </summary>
        /// <returns>A 12 byte array containing X, Y, and Z</returns>
        public byte[] GetBytes()
        {
            byte[] byteArray = new byte[12];
            ToBytes(byteArray, 0);
            return byteArray;
        }

        /// <summary>
        /// Writes the raw bytes for this vector to a byte array
        /// </summary>
        /// <param name="dest">Destination byte array</param>
        /// <param name="pos">Position in the destination array to start
        /// writing. Must be at least 12 bytes before the end of the array</param>
        public void ToBytes(byte[] dest, int pos)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(X), 0, dest, pos + 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Y), 0, dest, pos + 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Z), 0, dest, pos + 8, 4);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(dest, pos + 0, 4);
                Array.Reverse(dest, pos + 4, 4);
                Array.Reverse(dest, pos + 8, 4);
            }
        }

        #endregion Public Methods

        #region Static Methods

        public static Vector3 Add(Vector3 value1, Vector3 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Z += value2.Z;
            return value1;
        }

        public static Vector3 Clamp(Vector3 value1, Vector3 min, Vector3 max)
        {
            return new Vector3(
                Utils.Clamp(value1.X, min.X, max.X),
                Utils.Clamp(value1.Y, min.Y, max.Y),
                Utils.Clamp(value1.Z, min.Z, max.Z));
        }

        public static Vector3 Cross(Vector3 value1, Vector3 value2)
        {
            return new Vector3(
                value1.Y * value2.Z - value2.Y * value1.Z,
                value1.Z * value2.X - value2.Z * value1.X,
                value1.X * value2.Y - value2.X * value1.Y);
        }

        public static float Distance(Vector3 value1, Vector3 value2)
        {
            return (float)Math.Sqrt(DistanceSquared(value1, value2));
        }

        public static float DistanceSquared(Vector3 value1, Vector3 value2)
        {
            return
                (value1.X - value2.X) * (value1.X - value2.X) +
                (value1.Y - value2.Y) * (value1.Y - value2.Y) +
                (value1.Z - value2.Z) * (value1.Z - value2.Z);
        }

        public static Vector3 Divide(Vector3 value1, Vector3 value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            value1.Z /= value2.Z;
            return value1;
        }

        public static Vector3 Divide(Vector3 value1, float value2)
        {
            float factor = 1f / value2;
            value1.X *= factor;
            value1.Y *= factor;
            value1.Z *= factor;
            return value1;
        }

        public static float Dot(Vector3 value1, Vector3 value2)
        {
            return value1.X * value2.X + value1.Y * value2.Y + value1.Z * value2.Z;
        }

        public static Vector3 Lerp(Vector3 value1, Vector3 value2, float amount)
        {
            return new Vector3(
                Utils.Lerp(value1.X, value2.X, amount),
                Utils.Lerp(value1.Y, value2.Y, amount),
                Utils.Lerp(value1.Z, value2.Z, amount));
        }

        public static float Mag(Vector3 value)
        {
            return (float)Math.Sqrt((value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z));
        }

        public static Vector3 Max(Vector3 value1, Vector3 value2)
        {
            return new Vector3(
                Math.Max(value1.X, value2.X),
                Math.Max(value1.Y, value2.Y),
                Math.Max(value1.Z, value2.Z));
        }

        public static Vector3 Min(Vector3 value1, Vector3 value2)
        {
            return new Vector3(
                Math.Min(value1.X, value2.X),
                Math.Min(value1.Y, value2.Y),
                Math.Min(value1.Z, value2.Z));
        }

        public static Vector3 Multiply(Vector3 value1, Vector3 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            value1.Z *= value2.Z;
            return value1;
        }

        public static Vector3 Multiply(Vector3 value1, float scaleFactor)
        {
            value1.X *= scaleFactor;
            value1.Y *= scaleFactor;
            value1.Z *= scaleFactor;
            return value1;
        }

        public static Vector3 Negate(Vector3 value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            value.Z = -value.Z;
            return value;
        }

        public static Vector3 Normalize(Vector3 value)
        {
            const float MAG_THRESHOLD = 0.0000001f;
            float factor = Distance(value, Zero);
            if (factor > MAG_THRESHOLD)
            {
                factor = 1f / factor;
                value.X *= factor;
                value.Y *= factor;
                value.Z *= factor;
            }
            else
            {
                value.X = 0f;
                value.Y = 0f;
                value.Z = 0f;
            }
            return value;
        }

        /// <summary>
        /// Parse a vector from a string
        /// </summary>
        /// <param name="val">A string representation of a 3D vector, enclosed 
        /// in arrow brackets and separated by commas</param>
        public static Vector3 Parse(string val)
        {
            char[] splitChar = { ',' };
            string[] split = val.Replace("<", String.Empty).Replace(">", String.Empty).Split(splitChar);
            return new Vector3(
                Single.Parse(split[0].Trim(), Utils.EnUsCulture),
                Single.Parse(split[1].Trim(), Utils.EnUsCulture),
                Single.Parse(split[2].Trim(), Utils.EnUsCulture));
        }

        public static bool TryParse(string val, out Vector3 result)
        {
            try
            {
                result = Parse(val);
                return true;
            }
            catch (Exception)
            {
                result = Vector3.Zero;
                return false;
            }
        }

        /// <summary>
        /// Calculate the rotation between two vectors
        /// </summary>
        /// <param name="a">Normalized directional vector (such as 1,0,0 for forward facing)</param>
        /// <param name="b">Normalized target vector</param>
        public static Quaternion RotationBetween(Vector3 a, Vector3 b)
        {
            float dotProduct = Dot(a, b);
            Vector3 crossProduct = Cross(a, b);
            float magProduct = a.Length() * b.Length();
            double angle = Math.Acos(dotProduct / magProduct);
            Vector3 axis = Normalize(crossProduct);
            float s = (float)Math.Sin(angle / 2d);

            return new Quaternion(
                axis.X * s,
                axis.Y * s,
                axis.Z * s,
                (float)Math.Cos(angle / 2d));
        }

        /// <summary>
        /// Interpolates between two vectors using a cubic equation
        /// </summary>
        public static Vector3 SmoothStep(Vector3 value1, Vector3 value2, float amount)
        {
            return new Vector3(
                Utils.SmoothStep(value1.X, value2.X, amount),
                Utils.SmoothStep(value1.Y, value2.Y, amount),
                Utils.SmoothStep(value1.Z, value2.Z, amount));
        }

        public static Vector3 Subtract(Vector3 value1, Vector3 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Z -= value2.Z;
            return value1;
        }

        public static Vector3 Transform(Vector3 position, Matrix4 matrix)
        {
            return new Vector3(
                (position.X * matrix.M11) + (position.Y * matrix.M21) + (position.Z * matrix.M31) + matrix.M41,
                (position.X * matrix.M12) + (position.Y * matrix.M22) + (position.Z * matrix.M32) + matrix.M42,
                (position.X * matrix.M13) + (position.Y * matrix.M23) + (position.Z * matrix.M33) + matrix.M43);
        }

        public static Vector3 TransformNormal(Vector3 position, Matrix4 matrix)
        {
            return new Vector3(
                (position.X * matrix.M11) + (position.Y * matrix.M21) + (position.Z * matrix.M31),
                (position.X * matrix.M12) + (position.Y * matrix.M22) + (position.Z * matrix.M32),
                (position.X * matrix.M13) + (position.Y * matrix.M23) + (position.Z * matrix.M33));
        }

        #endregion Static Methods

        #region Overrides

        public override bool Equals(object obj)
        {
            return (obj is Vector3) ? this == (Vector3)obj : false;
        }

        public bool Equals(Vector3 other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        /// <summary>
        /// Get a formatted string representation of the vector
        /// </summary>
        /// <returns>A string representation of the vector</returns>
        public override string ToString()
        {
            return String.Format(Utils.EnUsCulture, "<{0}, {1}, {2}>", X, Y, Z);
        }

        /// <summary>
        /// Get a string representation of the vector elements with up to three
        /// decimal digits and separated by spaces only
        /// </summary>
        /// <returns>Raw string representation of the vector</returns>
        public string ToRawString()
        {
            CultureInfo enUs = new CultureInfo("en-us");
            enUs.NumberFormat.NumberDecimalDigits = 3;

            return String.Format(enUs, "{0} {1} {2}", X, Y, Z);
        }

        #endregion Overrides

        #region Operators

        public static bool operator ==(Vector3 value1, Vector3 value2)
        {
            return value1.X == value2.X
                && value1.Y == value2.Y
                && value1.Z == value2.Z;
        }

        public static bool operator !=(Vector3 value1, Vector3 value2)
        {
            return !(value1 == value2);
        }

        public static Vector3 operator +(Vector3 value1, Vector3 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Z += value2.Z;
            return value1;
        }

        public static Vector3 operator -(Vector3 value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            value.Z = -value.Z;
            return value;
        }

        public static Vector3 operator -(Vector3 value1, Vector3 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Z -= value2.Z;
            return value1;
        }

        public static Vector3 operator *(Vector3 value1, Vector3 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            value1.Z *= value2.Z;
            return value1;
        }

        public static Vector3 operator *(Vector3 value, float scaleFactor)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            value.Z *= scaleFactor;
            return value;
        }

        public static Vector3 operator *(Vector3 vec, Quaternion rot)
        {
            float rw = -rot.X * vec.X - rot.Y * vec.Y - rot.Z * vec.Z;
            float rx = rot.W * vec.X + rot.Y * vec.Z - rot.Z * vec.Y;
            float ry = rot.W * vec.Y + rot.Z * vec.X - rot.X * vec.Z;
            float rz = rot.W * vec.Z + rot.X * vec.Y - rot.Y * vec.X;

            vec.X = -rw * rot.X + rx * rot.W - ry * rot.Z + rz * rot.Y;
            vec.Y = -rw * rot.Y + ry * rot.W - rz * rot.X + rx * rot.Z;
            vec.Z = -rw * rot.Z + rz * rot.W - rx * rot.Y + ry * rot.X;

            return vec;
        }

        public static Vector3 operator *(Vector3 vector, Matrix4 matrix)
        {
            return Transform(vector, matrix);
        }

        public static Vector3 operator /(Vector3 value1, Vector3 value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            value1.Z /= value2.Z;
            return value1;
        }

        public static Vector3 operator /(Vector3 value, float divider)
        {
            float factor = 1f / divider;
            value.X *= factor;
            value.Y *= factor;
            value.Z *= factor;
            return value;
        }

        /// <summary>
        /// Cross product between two vectors
        /// </summary>
        public static Vector3 operator %(Vector3 value1, Vector3 value2)
        {
            return Cross(value1, value2);
        }

        #endregion Operators

        /// <summary>A vector with a value of 0,0,0</summary>
        public readonly static Vector3 Zero = new Vector3();
        /// <summary>A vector with a value of 1,1,1</summary>
        public readonly static Vector3 One = new Vector3(1f, 1f, 1f);
        /// <summary>A unit vector facing forward (X axis), value 1,0,0</summary>
        public readonly static Vector3 UnitX = new Vector3(1f, 0f, 0f);
        /// <summary>A unit vector facing left (Y axis), value 0,1,0</summary>
        public readonly static Vector3 UnitY = new Vector3(0f, 1f, 0f);
        /// <summary>A unit vector facing up (Z axis), value 0,0,1</summary>
        public readonly static Vector3 UnitZ = new Vector3(0f, 0f, 1f);
    }
}
