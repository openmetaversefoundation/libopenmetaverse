/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names
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
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Quaternion : IEquatable<Quaternion>
    {
        /// <summary>X value</summary>
        public float X;
        /// <summary>Y value</summary>
        public float Y;
        /// <summary>Z value</summary>
        public float Z;
        /// <summary>W value</summary>
        public float W;

        #region Constructors

        public Quaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Quaternion(Vector3 vectorPart, float scalarPart)
        {
            X = vectorPart.X;
            Y = vectorPart.Y;
            Z = vectorPart.Z;
            W = scalarPart;
        }

        /// <summary>
        /// Build a quaternion from normalized float values
        /// </summary>
        /// <param name="x">X value from -1.0 to 1.0</param>
        /// <param name="y">Y value from -1.0 to 1.0</param>
        /// <param name="z">Z value from -1.0 to 1.0</param>
        public Quaternion(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;

            float xyzsum = 1 - X * X - Y * Y - Z * Z;
            W = (xyzsum > 0) ? (float)Math.Sqrt(xyzsum) : 0;
        }

        /// <summary>
        /// Constructor, builds a quaternion object from a byte array
        /// </summary>
        /// <param name="byteArray">Byte array containing four four-byte floats</param>
        /// <param name="pos">Offset in the byte array to start reading at</param>
        /// <param name="normalized">Whether the source data is normalized or
        /// not. If this is true 12 bytes will be read, otherwise 16 bytes will
        /// be read.</param>
        public Quaternion(byte[] byteArray, int pos, bool normalized)
        {
            X = Y = Z = W = 0;
            FromBytes(byteArray, pos, normalized);
        }

        public Quaternion(Quaternion q)
        {
            X = q.X;
            Y = q.Y;
            Z = q.Z;
            W = q.W;
        }

        #endregion Constructors

        #region Public Methods

        public bool ApproxEquals(Quaternion quat, float tolerance)
        {
            Quaternion diff = this - quat;
            return (diff.LengthSquared() <= tolerance * tolerance);
        }

        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
        }

        public float LengthSquared()
        {
            return (X * X + Y * Y + Z * Z + W * W);
        }

        /// <summary>
        /// Normalizes the quaternion
        /// </summary>
        public void Normalize()
        {
            this = Normalize(this);
        }

        /// <summary>
        /// Builds a quaternion object from a byte array
        /// </summary>
        /// <param name="byteArray">The source byte array</param>
        /// <param name="pos">Offset in the byte array to start reading at</param>
        /// <param name="normalized">Whether the source data is normalized or
        /// not. If this is true 12 bytes will be read, otherwise 16 bytes will
        /// be read.</param>
        public void FromBytes(byte[] byteArray, int pos, bool normalized)
        {
            if (!normalized)
            {
                if (!BitConverter.IsLittleEndian)
                {
                    // Big endian architecture
                    byte[] conversionBuffer = new byte[16];

                    Buffer.BlockCopy(byteArray, pos, conversionBuffer, 0, 16);

                    Array.Reverse(conversionBuffer, 0, 4);
                    Array.Reverse(conversionBuffer, 4, 4);
                    Array.Reverse(conversionBuffer, 8, 4);
                    Array.Reverse(conversionBuffer, 12, 4);

                    X = BitConverter.ToSingle(conversionBuffer, 0);
                    Y = BitConverter.ToSingle(conversionBuffer, 4);
                    Z = BitConverter.ToSingle(conversionBuffer, 8);
                    W = BitConverter.ToSingle(conversionBuffer, 12);
                }
                else
                {
                    // Little endian architecture
                    X = BitConverter.ToSingle(byteArray, pos);
                    Y = BitConverter.ToSingle(byteArray, pos + 4);
                    Z = BitConverter.ToSingle(byteArray, pos + 8);
                    W = BitConverter.ToSingle(byteArray, pos + 12);
                }
            }
            else
            {
                if (!BitConverter.IsLittleEndian)
                {
                    // Big endian architecture
                    byte[] conversionBuffer = new byte[16];

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

                float xyzsum = 1f - X * X - Y * Y - Z * Z;
                W = (xyzsum > 0f) ? (float)Math.Sqrt(xyzsum) : 0f;
            }
        }

        /// <summary>
        /// Normalize this quaternion and serialize it to a byte array
        /// </summary>
        /// <returns>A 12 byte array containing normalized X, Y, and Z floating
        /// point values in order using little endian byte ordering</returns>
        public byte[] GetBytes()
        {
            byte[] bytes = new byte[12];
            ToBytes(bytes, 0);
            return bytes;
        }

        /// <summary>
        /// Writes the raw bytes for this quaternion to a byte array
        /// </summary>
        /// <param name="dest">Destination byte array</param>
        /// <param name="pos">Position in the destination array to start
        /// writing. Must be at least 12 bytes before the end of the array</param>
        public void ToBytes(byte[] dest, int pos)
        {
            float norm = (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

            if (norm != 0f)
            {
                norm = 1f / norm;

                float x, y, z;
                if (W >= 0f)
                {
                    x = X; y = Y; z = Z;
                }
                else
                {
                    x = -X; y = -Y; z = -Z;
                }

                Buffer.BlockCopy(BitConverter.GetBytes(norm * x), 0, dest, pos + 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(norm * y), 0, dest, pos + 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(norm * z), 0, dest, pos + 8, 4);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(dest, pos + 0, 4);
                    Array.Reverse(dest, pos + 4, 4);
                    Array.Reverse(dest, pos + 8, 4);
                }
            }
            else
            {
                throw new InvalidOperationException(String.Format(
                    "Quaternion {0} normalized to zero", ToString()));
            }
        }

        /// <summary>
        /// Convert this quaternion to euler angles
        /// </summary>
        /// <param name="roll">X euler angle</param>
        /// <param name="pitch">Y euler angle</param>
        /// <param name="yaw">Z euler angle</param>
        public void GetEulerAngles(out float roll, out float pitch, out float yaw)
        {
            roll = 0f;
            pitch = 0f;
            yaw = 0f;

            Quaternion t = new Quaternion(this.X * this.X, this.Y * this.Y, this.Z * this.Z, this.W * this.W);

            float m = (t.X + t.Y + t.Z + t.W);
            if (Math.Abs(m) < 0.001d) return;
            float n = 2 * (this.Y * this.W + this.X * this.Z);
            float p = m * m - n * n;

            if (p > 0f)
            {
                roll = (float)Math.Atan2(2.0f * (this.X * this.W - this.Y * this.Z), (-t.X - t.Y + t.Z + t.W));
                pitch = (float)Math.Atan2(n, Math.Sqrt(p));
                yaw = (float)Math.Atan2(2.0f * (this.Z * this.W - this.X * this.Y), t.X - t.Y - t.Z + t.W);
            }
            else if (n > 0f)
            {
                roll = 0f;
                pitch = (float)(Math.PI / 2d);
                yaw = (float)Math.Atan2((this.Z * this.W + this.X * this.Y), 0.5f - t.X - t.Y);
            }
            else
            {
                roll = 0f;
                pitch = -(float)(Math.PI / 2d);
                yaw = (float)Math.Atan2((this.Z * this.W + this.X * this.Y), 0.5f - t.X - t.Z);
            }

            //float sqx = X * X;
            //float sqy = Y * Y;
            //float sqz = Z * Z;
            //float sqw = W * W;

            //// Unit will be a correction factor if the quaternion is not normalized
            //float unit = sqx + sqy + sqz + sqw;
            //double test = X * Y + Z * W;

            //if (test > 0.499f * unit)
            //{
            //    // Singularity at north pole
            //    yaw = 2f * (float)Math.Atan2(X, W);
            //    pitch = (float)Math.PI / 2f;
            //    roll = 0f;
            //}
            //else if (test < -0.499f * unit)
            //{
            //    // Singularity at south pole
            //    yaw = -2f * (float)Math.Atan2(X, W);
            //    pitch = -(float)Math.PI / 2f;
            //    roll = 0f;
            //}
            //else
            //{
            //    yaw = (float)Math.Atan2(2f * Y * W - 2f * X * Z, sqx - sqy - sqz + sqw);
            //    pitch = (float)Math.Asin(2f * test / unit);
            //    roll = (float)Math.Atan2(2f * X * W - 2f * Y * Z, -sqx + sqy - sqz + sqw);
            //}
        }

        /// <summary>
        /// Convert this quaternion to an angle around an axis
        /// </summary>
        /// <param name="axis">Unit vector describing the axis</param>
        /// <param name="angle">Angle around the axis, in radians</param>
        public void GetAxisAngle(out Vector3 axis, out float angle)
        {
            Quaternion q = Normalize(this);

            float sin = (float)Math.Sqrt(1.0f - q.W * q.W);
            if (sin >= 0.001)
            {
                float invSin = 1.0f / sin;
                if (q.W < 0) invSin = -invSin;
                axis = new Vector3(q.X, q.Y, q.Z) * invSin;

                angle = 2.0f * (float)Math.Acos(q.W);
                if (angle > Math.PI)
                    angle = 2.0f * (float)Math.PI - angle;
            }
            else
            {
                axis = Vector3.UnitX;
                angle = 0f;
            }
        }

        #endregion Public Methods

        #region Static Methods

        public static Quaternion Add(Quaternion quaternion1, Quaternion quaternion2)
        {
            quaternion1.X += quaternion2.X;
            quaternion1.Y += quaternion2.Y;
            quaternion1.Z += quaternion2.Z;
            quaternion1.W += quaternion2.W;
            return quaternion1;
        }

        /// <summary>
        /// Returns the conjugate (spatial inverse) of a quaternion
        /// </summary>
        public static Quaternion Conjugate(Quaternion quaternion)
        {
            quaternion.X = -quaternion.X;
            quaternion.Y = -quaternion.Y;
            quaternion.Z = -quaternion.Z;
            return quaternion;
        }

        /// <summary>
        /// Build a quaternion from an axis and an angle of rotation around
        /// that axis
        /// </summary>
        public static Quaternion CreateFromAxisAngle(float axisX, float axisY, float axisZ, float angle)
        {
            Vector3 axis = new Vector3(axisX, axisY, axisZ);
            return CreateFromAxisAngle(axis, angle);
        }

        /// <summary>
        /// Build a quaternion from an axis and an angle of rotation around
        /// that axis
        /// </summary>
        /// <param name="axis">Axis of rotation</param>
        /// <param name="angle">Angle of rotation</param>
        public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
        {
            Quaternion q;
            axis = Vector3.Normalize(axis);

            angle *= 0.5f;
            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);

            q.X = axis.X * s;
            q.Y = axis.Y * s;
            q.Z = axis.Z * s;
            q.W = c;

            return Quaternion.Normalize(q);
        }

        /// <summary>
        /// Creates a quaternion from a vector containing roll, pitch, and yaw
        /// in radians
        /// </summary>
        /// <param name="eulers">Vector representation of the euler angles in
        /// radians</param>
        /// <returns>Quaternion representation of the euler angles</returns>
        public static Quaternion CreateFromEulers(Vector3 eulers)
        {
            return CreateFromEulers(eulers.X, eulers.Y, eulers.Z);
        }

        /// <summary>
        /// Creates a quaternion from roll, pitch, and yaw euler angles in
        /// radians
        /// </summary>
        /// <param name="roll">X angle in radians</param>
        /// <param name="pitch">Y angle in radians</param>
        /// <param name="yaw">Z angle in radians</param>
        /// <returns>Quaternion representation of the euler angles</returns>
        public static Quaternion CreateFromEulers(float roll, float pitch, float yaw)
        {
            if (roll > Utils.TWO_PI || pitch > Utils.TWO_PI || yaw > Utils.TWO_PI)
                throw new ArgumentException("Euler angles must be in radians");

            double atCos = Math.Cos(roll / 2f);
            double atSin = Math.Sin(roll / 2f);
            double leftCos = Math.Cos(pitch / 2f);
            double leftSin = Math.Sin(pitch / 2f);
            double upCos = Math.Cos(yaw / 2f);
            double upSin = Math.Sin(yaw / 2f);
            double atLeftCos = atCos * leftCos;
            double atLeftSin = atSin * leftSin;
            return new Quaternion(
                (float)(atSin * leftCos * upCos + atCos * leftSin * upSin),
                (float)(atCos * leftSin * upCos - atSin * leftCos * upSin),
                (float)(atLeftCos * upSin + atLeftSin * upCos),
                (float)(atLeftCos * upCos - atLeftSin * upSin)
            );
        }

        public static Quaternion CreateFromRotationMatrix(Matrix4 matrix)
        {
            float num8 = (matrix.M11 + matrix.M22) + matrix.M33;
            Quaternion quaternion = new Quaternion();
            if (num8 > 0f)
            {
                float num = (float)Math.Sqrt((double)(num8 + 1f));
                quaternion.W = num * 0.5f;
                num = 0.5f / num;
                quaternion.X = (matrix.M23 - matrix.M32) * num;
                quaternion.Y = (matrix.M31 - matrix.M13) * num;
                quaternion.Z = (matrix.M12 - matrix.M21) * num;
                return quaternion;
            }
            if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                float num7 = (float)Math.Sqrt((double)(((1f + matrix.M11) - matrix.M22) - matrix.M33));
                float num4 = 0.5f / num7;
                quaternion.X = 0.5f * num7;
                quaternion.Y = (matrix.M12 + matrix.M21) * num4;
                quaternion.Z = (matrix.M13 + matrix.M31) * num4;
                quaternion.W = (matrix.M23 - matrix.M32) * num4;
                return quaternion;
            }
            if (matrix.M22 > matrix.M33)
            {
                float num6 = (float)Math.Sqrt((double)(((1f + matrix.M22) - matrix.M11) - matrix.M33));
                float num3 = 0.5f / num6;
                quaternion.X = (matrix.M21 + matrix.M12) * num3;
                quaternion.Y = 0.5f * num6;
                quaternion.Z = (matrix.M32 + matrix.M23) * num3;
                quaternion.W = (matrix.M31 - matrix.M13) * num3;
                return quaternion;
            }
            float num5 = (float)Math.Sqrt((double)(((1f + matrix.M33) - matrix.M11) - matrix.M22));
            float num2 = 0.5f / num5;
            quaternion.X = (matrix.M31 + matrix.M13) * num2;
            quaternion.Y = (matrix.M32 + matrix.M23) * num2;
            quaternion.Z = 0.5f * num5;
            quaternion.W = (matrix.M12 - matrix.M21) * num2;

            return quaternion;
        }

        public static Quaternion Divide(Quaternion q1, Quaternion q2)
        {
            return Quaternion.Inverse(q1) * q2;
        }

        public static float Dot(Quaternion q1, Quaternion q2)
        {
            return (q1.X * q2.X) + (q1.Y * q2.Y) + (q1.Z * q2.Z) + (q1.W * q2.W);
        }

        /// <summary>
        /// Conjugates and renormalizes a vector
        /// </summary>
        public static Quaternion Inverse(Quaternion quaternion)
        {
            float norm = quaternion.LengthSquared();

            if (norm == 0f)
            {
                quaternion.X = quaternion.Y = quaternion.Z = quaternion.W = 0f;
            }
            else
            {
                float oonorm = 1f / norm;
                quaternion = Conjugate(quaternion);
                
                quaternion.X *= oonorm;
                quaternion.Y *= oonorm;
                quaternion.Z *= oonorm;
                quaternion.W *= oonorm;
            }

            return quaternion;
        }

        /// <summary>
        /// Spherical linear interpolation between two quaternions
        /// </summary>
        public static Quaternion Slerp(Quaternion q1, Quaternion q2, float amount)
        {
            float angle = Dot(q1, q2);

            if (angle < 0f)
            {
                q1 *= -1f;
                angle *= -1f;
            }

            float scale;
            float invscale;

            if ((angle + 1f) > 0.05f)
            {
                if ((1f - angle) >= 0.05f)
                {
                    // slerp
                    float theta = (float)Math.Acos(angle);
                    float invsintheta = 1f / (float)Math.Sin(theta);
                    scale = (float)Math.Sin(theta * (1f - amount)) * invsintheta;
                    invscale = (float)Math.Sin(theta * amount) * invsintheta;
                }
                else
                {
                    // lerp
                    scale = 1f - amount;
                    invscale = amount;
                }
            }
            else
            {
                q2.X = -q1.Y;
                q2.Y = q1.X;
                q2.Z = -q1.W;
                q2.W = q1.Z;

                scale = (float)Math.Sin(Utils.PI * (0.5f - amount));
                invscale = (float)Math.Sin(Utils.PI * amount);
            }

            return (q1 * scale) + (q2 * invscale);
        }

        public static Quaternion Subtract(Quaternion quaternion1, Quaternion quaternion2)
        {
            quaternion1.X -= quaternion2.X;
            quaternion1.Y -= quaternion2.Y;
            quaternion1.Z -= quaternion2.Z;
            quaternion1.W -= quaternion2.W;
            return quaternion1;
        }

        public static Quaternion Multiply(Quaternion a, Quaternion b)
        {
            return new Quaternion(
                a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
                a.W * b.Y + a.Y * b.W + a.Z * b.X - a.X * b.Z,
                a.W * b.Z + a.Z * b.W + a.X * b.Y - a.Y * b.X,
                a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z
            );
        }

        public static Quaternion Multiply(Quaternion quaternion, float scaleFactor)
        {
            quaternion.X *= scaleFactor;
            quaternion.Y *= scaleFactor;
            quaternion.Z *= scaleFactor;
            quaternion.W *= scaleFactor;
            return quaternion;
        }

        public static Quaternion Negate(Quaternion quaternion)
        {
            quaternion.X = -quaternion.X;
            quaternion.Y = -quaternion.Y;
            quaternion.Z = -quaternion.Z;
            quaternion.W = -quaternion.W;
            return quaternion;
        }

        public static Quaternion Normalize(Quaternion q)
        {
            const float MAG_THRESHOLD = 0.0000001f;
            float mag = q.Length();

            // Catch very small rounding errors when normalizing
            if (mag > MAG_THRESHOLD)
            {
                float oomag = 1f / mag;
                q.X *= oomag;
                q.Y *= oomag;
                q.Z *= oomag;
                q.W *= oomag;
            }
            else
            {
                q.X = 0f;
                q.Y = 0f;
                q.Z = 0f;
                q.W = 1f;
            }

            return q;
        }

        public static Quaternion Parse(string val)
        {
            char[] splitChar = { ',' };
            string[] split = val.Replace("<", String.Empty).Replace(">", String.Empty).Split(splitChar);
            if (split.Length == 3)
            {
                return new Quaternion(
                    float.Parse(split[0].Trim(), Utils.EnUsCulture),
                    float.Parse(split[1].Trim(), Utils.EnUsCulture),
                    float.Parse(split[2].Trim(), Utils.EnUsCulture));
            }
            else
            {
                return new Quaternion(
                    float.Parse(split[0].Trim(), Utils.EnUsCulture),
                    float.Parse(split[1].Trim(), Utils.EnUsCulture),
                    float.Parse(split[2].Trim(), Utils.EnUsCulture),
                    float.Parse(split[3].Trim(), Utils.EnUsCulture));
            }
        }

        public static bool TryParse(string val, out Quaternion result)
        {
            try
            {
                result = Parse(val);
                return true;
            }
            catch (Exception)
            {
                result = new Quaternion();
                return false;
            }
        }

        #endregion Static Methods

        #region Overrides

        public override bool Equals(object obj)
        {
            return (obj is Quaternion) ? this == (Quaternion)obj : false;
        }

        public bool Equals(Quaternion other)
        {
            return W == other.W
                && X == other.X
                && Y == other.Y
                && Z == other.Z;
        }

        public override int GetHashCode()
        {
            return (X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode());
        }

        public override string ToString()
        {
            return String.Format(Utils.EnUsCulture, "<{0}, {1}, {2}, {3}>", X, Y, Z, W);
        }

        /// <summary>
        /// Get a string representation of the quaternion elements with up to three
        /// decimal digits and separated by spaces only
        /// </summary>
        /// <returns>Raw string representation of the quaternion</returns>
        public string ToRawString()
        {
            CultureInfo enUs = new CultureInfo("en-us");
            enUs.NumberFormat.NumberDecimalDigits = 3;

            return String.Format(enUs, "{0} {1} {2} {3}", X, Y, Z, W);
        }

        #endregion Overrides

        #region Operators

        public static bool operator ==(Quaternion quaternion1, Quaternion quaternion2)
        {
            return quaternion1.Equals(quaternion2);
        }

        public static bool operator !=(Quaternion quaternion1, Quaternion quaternion2)
        {
            return !(quaternion1 == quaternion2);
        }

        public static Quaternion operator +(Quaternion quaternion1, Quaternion quaternion2)
        {
            return Add(quaternion1, quaternion2);
        }

        public static Quaternion operator -(Quaternion quaternion)
        {
            return Negate(quaternion);
        }

        public static Quaternion operator -(Quaternion quaternion1, Quaternion quaternion2)
        {
            return Subtract(quaternion1, quaternion2);
        }

        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            return Multiply(a, b);
        }

        public static Quaternion operator *(Quaternion quaternion, float scaleFactor)
        {
            return Multiply(quaternion, scaleFactor);
        }

        public static Quaternion operator /(Quaternion quaternion1, Quaternion quaternion2)
        {
            return Divide(quaternion1, quaternion2);
        }

        #endregion Operators

        /// <summary>A quaternion with a value of 0,0,0,1</summary>
        public readonly static Quaternion Identity = new Quaternion(0f, 0f, 0f, 1f);
    }
}
