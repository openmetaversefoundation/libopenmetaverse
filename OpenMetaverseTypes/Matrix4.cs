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

namespace OpenMetaverse
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix4 : IEquatable<Matrix4>
    {
        public float M11, M12, M13, M14;
        public float M21, M22, M23, M24;
        public float M31, M32, M33, M34;
        public float M41, M42, M43, M44;

        #region Properties

        public Vector3 AtAxis
        {
            get
            {
                return new Vector3(M11, M21, M31);
            }
            set
            {
                M11 = value.X;
                M21 = value.Y;
                M31 = value.Z;
            }
        }

        public Vector3 LeftAxis
        {
            get
            {
                return new Vector3(M12, M22, M32);
            }
            set
            {
                M12 = value.X;
                M22 = value.Y;
                M32 = value.Z;
            }
        }

        public Vector3 UpAxis
        {
            get
            {
                return new Vector3(M13, M23, M33);
            }
            set
            {
                M13 = value.X;
                M23 = value.Y;
                M33 = value.Z;
            }
        }

        #endregion Properties

        #region Constructors

        public Matrix4(
            float m11, float m12, float m13, float m14,
            float m21, float m22, float m23, float m24,
            float m31, float m32, float m33, float m34,
            float m41, float m42, float m43, float m44)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M14 = m14;

            M21 = m21;
            M22 = m22;
            M23 = m23;
            M24 = m24;

            M31 = m31;
            M32 = m32;
            M33 = m33;
            M34 = m34;

            M41 = m41;
            M42 = m42;
            M43 = m43;
            M44 = m44;
        }

        public Matrix4(float roll, float pitch, float yaw)
        {
            this = CreateFromEulers(roll, pitch, yaw);
        }

        public Matrix4(Matrix4 m)
        {
            M11 = m.M11;
            M12 = m.M12;
            M13 = m.M13;
            M14 = m.M14;

            M21 = m.M21;
            M22 = m.M22;
            M23 = m.M23;
            M24 = m.M24;

            M31 = m.M31;
            M32 = m.M32;
            M33 = m.M33;
            M34 = m.M34;

            M41 = m.M41;
            M42 = m.M42;
            M43 = m.M43;
            M44 = m.M44;
        }

        #endregion Constructors

        #region Public Methods

        public float Determinant()
        {
            return
                M14 * M23 * M32 * M41 - M13 * M24 * M32 * M41 - M14 * M22 * M33 * M41 + M12 * M24 * M33 * M41 +
                M13 * M22 * M34 * M41 - M12 * M23 * M34 * M41 - M14 * M23 * M31 * M42 + M13 * M24 * M31 * M42 +
                M14 * M21 * M33 * M42 - M11 * M24 * M33 * M42 - M13 * M21 * M34 * M42 + M11 * M23 * M34 * M42 +
                M14 * M22 * M31 * M43 - M12 * M24 * M31 * M43 - M14 * M21 * M32 * M43 + M11 * M24 * M32 * M43 +
                M12 * M21 * M34 * M43 - M11 * M22 * M34 * M43 - M13 * M22 * M31 * M44 + M12 * M23 * M31 * M44 +
                M13 * M21 * M32 * M44 - M11 * M23 * M32 * M44 - M12 * M21 * M33 * M44 + M11 * M22 * M33 * M44;
        }

        public float Determinant3x3()
        {
            float det = 0f;

            float diag1 = M11 * M22 * M33;
            float diag2 = M12 * M23 * M31;
            float diag3 = M13 * M21 * M32;
            float diag4 = M31 * M22 * M13;
            float diag5 = M32 * M23 * M11;
            float diag6 = M33 * M21 * M12;

            det = diag1 + diag2 + diag3 - (diag4 + diag5 + diag6);

            return det;
        }

        public float Trace()
        {
            return M11 + M22 + M33 + M44;
        }

        /// <summary>
        /// Convert this matrix to euler rotations
        /// </summary>
        /// <param name="roll">X euler angle</param>
        /// <param name="pitch">Y euler angle</param>
        /// <param name="yaw">Z euler angle</param>
        public void GetEulerAngles(out float roll, out float pitch, out float yaw)
        {
            double angleX, angleY, angleZ;
            double cx, cy, cz; // cosines
            double sx, sz; // sines

            angleY = Math.Asin(Utils.Clamp(M13, -1f, 1f));
            cy = Math.Cos(angleY);

            if (Math.Abs(cy) > 0.005f)
            {
                // No gimbal lock
                cx = M33 / cy;
                sx = (-M23) / cy;

                angleX = (float)Math.Atan2(sx, cx);

                cz = M11 / cy;
                sz = (-M12) / cy;

                angleZ = (float)Math.Atan2(sz, cz);
            }
            else
            {
                // Gimbal lock
                angleX = 0;

                cz = M22;
                sz = M21;

                angleZ = Math.Atan2(sz, cz);
            }

            // Return only positive angles in [0,360]
            if (angleX < 0) angleX += 360d;
            if (angleY < 0) angleY += 360d;
            if (angleZ < 0) angleZ += 360d;

            roll = (float)angleX;
            pitch = (float)angleY;
            yaw = (float)angleZ;
        }

        /// <summary>
        /// Convert this matrix to a quaternion rotation
        /// </summary>
        /// <returns>A quaternion representation of this rotation matrix</returns>
        public Quaternion GetQuaternion()
        {
            Quaternion quat = new Quaternion();
            float trace = Trace() + 1f;

            if (trace > Single.Epsilon)
            {
                float s = 0.5f / (float)Math.Sqrt(trace);

                quat.X = (M32 - M23) * s;
                quat.Y = (M13 - M31) * s;
                quat.Z = (M21 - M12) * s;
                quat.W = 0.25f / s;
            }
            else
            {
                if (M11 > M22 && M11 > M33)
                {
                    float s = 2.0f * (float)Math.Sqrt(1.0f + M11 - M22 - M33);

                    quat.X = 0.25f * s;
                    quat.Y = (M12 + M21) / s;
                    quat.Z = (M13 + M31) / s;
                    quat.W = (M23 - M32) / s;
                }
                else if (M22 > M33)
                {
                    float s = 2.0f * (float)Math.Sqrt(1.0f + M22 - M11 - M33);

                    quat.X = (M12 + M21) / s;
                    quat.Y = 0.25f * s;
                    quat.Z = (M23 + M32) / s;
                    quat.W = (M13 - M31) / s;
                }
                else
                {
                    float s = 2.0f * (float)Math.Sqrt(1.0f + M33 - M11 - M22);

                    quat.X = (M13 + M31) / s;
                    quat.Y = (M23 + M32) / s;
                    quat.Z = 0.25f * s;
                    quat.W = (M12 - M21) / s;
                }
            }

            return quat;
        }

        public bool Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation)
        {
            translation.X = this.M41;
            translation.Y = this.M42;
            translation.Z = this.M43;

            float xs = (Math.Sign(M11 * M12 * M13 * M14) < 0) ? -1 : 1;
            float ys = (Math.Sign(M21 * M22 * M23 * M24) < 0) ? -1 : 1;
            float zs = (Math.Sign(M31 * M32 * M33 * M34) < 0) ? -1 : 1;

            scale.X = xs * (float)Math.Sqrt(this.M11 * this.M11 + this.M12 * this.M12 + this.M13 * this.M13);
            scale.Y = ys * (float)Math.Sqrt(this.M21 * this.M21 + this.M22 * this.M22 + this.M23 * this.M23);
            scale.Z = zs * (float)Math.Sqrt(this.M31 * this.M31 + this.M32 * this.M32 + this.M33 * this.M33);

            if (scale.X == 0.0 || scale.Y == 0.0 || scale.Z == 0.0)
            {
                rotation = Quaternion.Identity;
                return false;
            }

            Matrix4 m1 = new Matrix4(this.M11 / scale.X, M12 / scale.X, M13 / scale.X, 0,
                                     this.M21 / scale.Y, M22 / scale.Y, M23 / scale.Y, 0,
                                     this.M31 / scale.Z, M32 / scale.Z, M33 / scale.Z, 0,
                                     0, 0, 0, 1);

            rotation = Quaternion.CreateFromRotationMatrix(m1);
            return true;
        }	

        #endregion Public Methods

        #region Static Methods

        public static Matrix4 Add(Matrix4 matrix1, Matrix4 matrix2)
        {
            Matrix4 matrix;
            matrix.M11 = matrix1.M11 + matrix2.M11;
            matrix.M12 = matrix1.M12 + matrix2.M12;
            matrix.M13 = matrix1.M13 + matrix2.M13;
            matrix.M14 = matrix1.M14 + matrix2.M14;

            matrix.M21 = matrix1.M21 + matrix2.M21;
            matrix.M22 = matrix1.M22 + matrix2.M22;
            matrix.M23 = matrix1.M23 + matrix2.M23;
            matrix.M24 = matrix1.M24 + matrix2.M24;

            matrix.M31 = matrix1.M31 + matrix2.M31;
            matrix.M32 = matrix1.M32 + matrix2.M32;
            matrix.M33 = matrix1.M33 + matrix2.M33;
            matrix.M34 = matrix1.M34 + matrix2.M34;

            matrix.M41 = matrix1.M41 + matrix2.M41;
            matrix.M42 = matrix1.M42 + matrix2.M42;
            matrix.M43 = matrix1.M43 + matrix2.M43;
            matrix.M44 = matrix1.M44 + matrix2.M44;
            return matrix;
        }

        public static Matrix4 CreateFromAxisAngle(Vector3 axis, float angle)
        {
            Matrix4 matrix = new Matrix4();

            float x = axis.X;
            float y = axis.Y;
            float z = axis.Z;
            float sin = (float)Math.Sin(angle);
            float cos = (float)Math.Cos(angle);
            float xx = x * x;
            float yy = y * y;
            float zz = z * z;
            float xy = x * y;
            float xz = x * z;
            float yz = y * z;

            matrix.M11 = xx + (cos * (1f - xx));
            matrix.M12 = (xy - (cos * xy)) + (sin * z);
            matrix.M13 = (xz - (cos * xz)) - (sin * y);
            //matrix.M14 = 0f;

            matrix.M21 = (xy - (cos * xy)) - (sin * z);
            matrix.M22 = yy + (cos * (1f - yy));
            matrix.M23 = (yz - (cos * yz)) + (sin * x);
            //matrix.M24 = 0f;

            matrix.M31 = (xz - (cos * xz)) + (sin * y);
            matrix.M32 = (yz - (cos * yz)) - (sin * x);
            matrix.M33 = zz + (cos * (1f - zz));
            //matrix.M34 = 0f;

            //matrix.M41 = matrix.M42 = matrix.M43 = 0f;
            matrix.M44 = 1f;

            return matrix;
        }

        /// <summary>
        /// Construct a matrix from euler rotation values in radians
        /// </summary>
        /// <param name="roll">X euler angle in radians</param>
        /// <param name="pitch">Y euler angle in radians</param>
        /// <param name="yaw">Z euler angle in radians</param>
        public static Matrix4 CreateFromEulers(float roll, float pitch, float yaw)
        {
            Matrix4 m;

            float a, b, c, d, e, f;
            float ad, bd;

            a = (float)Math.Cos(roll);
            b = (float)Math.Sin(roll);
            c = (float)Math.Cos(pitch);
            d = (float)Math.Sin(pitch);
            e = (float)Math.Cos(yaw);
            f = (float)Math.Sin(yaw);

            ad = a * d;
            bd = b * d;

            m.M11 = c * e;
            m.M12 = -c * f;
            m.M13 = d;
            m.M14 = 0f;

            m.M21 = bd * e + a * f;
            m.M22 = -bd * f + a * e;
            m.M23 = -b * c;
            m.M24 = 0f;

            m.M31 = -ad * e + b * f;
            m.M32 = ad * f + b * e;
            m.M33 = a * c;
            m.M34 = 0f;

            m.M41 = m.M42 = m.M43 = 0f;
            m.M44 = 1f;

            return m;
        }

        public static Matrix4 CreateFromQuaternion(Quaternion quaternion)
        {
            Matrix4 matrix;

            float xx = quaternion.X * quaternion.X;
            float yy = quaternion.Y * quaternion.Y;
            float zz = quaternion.Z * quaternion.Z;
            float xy = quaternion.X * quaternion.Y;
            float zw = quaternion.Z * quaternion.W;
            float zx = quaternion.Z * quaternion.X;
            float yw = quaternion.Y * quaternion.W;
            float yz = quaternion.Y * quaternion.Z;
            float xw = quaternion.X * quaternion.W;

            matrix.M11 = 1f - (2f * (yy + zz));
            matrix.M12 = 2f * (xy + zw);
            matrix.M13 = 2f * (zx - yw);
            matrix.M14 = 0f;

            matrix.M21 = 2f * (xy - zw);
            matrix.M22 = 1f - (2f * (zz + xx));
            matrix.M23 = 2f * (yz + xw);
            matrix.M24 = 0f;

            matrix.M31 = 2f * (zx + yw);
            matrix.M32 = 2f * (yz - xw);
            matrix.M33 = 1f - (2f * (yy + xx));
            matrix.M34 = 0f;

            matrix.M41 = matrix.M42 = matrix.M43 = 0f;
            matrix.M44 = 1f;

            return matrix;
        }

        public static Matrix4 CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
        {
            Matrix4 matrix;

            Vector3 z = Vector3.Normalize(cameraPosition - cameraTarget);
            Vector3 x = Vector3.Normalize(Vector3.Cross(cameraUpVector, z));
            Vector3 y = Vector3.Cross(z, x);

            matrix.M11 = x.X;
            matrix.M12 = y.X;
            matrix.M13 = z.X;
            matrix.M14 = 0f;

            matrix.M21 = x.Y;
            matrix.M22 = y.Y;
            matrix.M23 = z.Y;
            matrix.M24 = 0f;

            matrix.M31 = x.Z;
            matrix.M32 = y.Z;
            matrix.M33 = z.Z;
            matrix.M34 = 0f;

            matrix.M41 = -Vector3.Dot(x, cameraPosition);
            matrix.M42 = -Vector3.Dot(y, cameraPosition);
            matrix.M43 = -Vector3.Dot(z, cameraPosition);
            matrix.M44 = 1f;

            return matrix;
        }

        public static Matrix4 CreateRotationX(float radians)
        {
            Matrix4 matrix;

            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            matrix.M11 = 1f;
            matrix.M12 = 0f;
            matrix.M13 = 0f;
            matrix.M14 = 0f;

            matrix.M21 = 0f;
            matrix.M22 = cos;
            matrix.M23 = sin;
            matrix.M24 = 0f;

            matrix.M31 = 0f;
            matrix.M32 = -sin;
            matrix.M33 = cos;
            matrix.M34 = 0f;

            matrix.M41 = 0f;
            matrix.M42 = 0f;
            matrix.M43 = 0f;
            matrix.M44 = 1f;

            return matrix;
        }

        public static Matrix4 CreateRotationY(float radians)
        {
            Matrix4 matrix;

            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            matrix.M11 = cos;
            matrix.M12 = 0f;
            matrix.M13 = -sin;
            matrix.M14 = 0f;

            matrix.M21 = 0f;
            matrix.M22 = 1f;
            matrix.M23 = 0f;
            matrix.M24 = 0f;

            matrix.M31 = sin;
            matrix.M32 = 0f;
            matrix.M33 = cos;
            matrix.M34 = 0f;

            matrix.M41 = 0f;
            matrix.M42 = 0f;
            matrix.M43 = 0f;
            matrix.M44 = 1f;

            return matrix;
        }

        public static Matrix4 CreateRotationZ(float radians)
        {
            Matrix4 matrix;

            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            matrix.M11 = cos;
            matrix.M12 = sin;
            matrix.M13 = 0f;
            matrix.M14 = 0f;

            matrix.M21 = -sin;
            matrix.M22 = cos;
            matrix.M23 = 0f;
            matrix.M24 = 0f;

            matrix.M31 = 0f;
            matrix.M32 = 0f;
            matrix.M33 = 1f;
            matrix.M34 = 0f;

            matrix.M41 = 0f;
            matrix.M42 = 0f;
            matrix.M43 = 0f;
            matrix.M44 = 1f;

            return matrix;
        }

        public static Matrix4 CreateScale(Vector3 scale)
        {
            Matrix4 matrix;

            matrix.M11 = scale.X;
            matrix.M12 = 0f;
            matrix.M13 = 0f;
            matrix.M14 = 0f;

            matrix.M21 = 0f;
            matrix.M22 = scale.Y;
            matrix.M23 = 0f;
            matrix.M24 = 0f;

            matrix.M31 = 0f;
            matrix.M32 = 0f;
            matrix.M33 = scale.Z;
            matrix.M34 = 0f;

            matrix.M41 = 0f;
            matrix.M42 = 0f;
            matrix.M43 = 0f;
            matrix.M44 = 1f;

            return matrix;
        }

        public static Matrix4 CreateTranslation(Vector3 position)
        {
            Matrix4 matrix;

            matrix.M11 = 1f;
            matrix.M12 = 0f;
            matrix.M13 = 0f;
            matrix.M14 = 0f;

            matrix.M21 = 0f;
            matrix.M22 = 1f;
            matrix.M23 = 0f;
            matrix.M24 = 0f;

            matrix.M31 = 0f;
            matrix.M32 = 0f;
            matrix.M33 = 1f;
            matrix.M34 = 0f;

            matrix.M41 = position.X;
            matrix.M42 = position.Y;
            matrix.M43 = position.Z;
            matrix.M44 = 1f;
            
            return matrix;
        }

        public static Matrix4 CreateWorld(Vector3 position, Vector3 forward, Vector3 up)
        {
            Matrix4 result;
            
            // Normalize forward vector
            forward.Normalize();

            // Calculate right vector
            Vector3 right = Vector3.Cross(forward, up);
            right.Normalize();

            // Recalculate up vector
            up = Vector3.Cross(right, forward);
            up.Normalize();

            result.M11 = right.X;
            result.M12 = right.Y;
            result.M13 = right.Z;
            result.M14 = 0.0f;

            result.M21 = up.X;
            result.M22 = up.Y;
            result.M23 = up.Z;
            result.M24 = 0.0f;

            result.M31 = -forward.X;
            result.M32 = -forward.Y;
            result.M33 = -forward.Z;
            result.M34 = 0.0f;

            result.M41 = position.X;
            result.M42 = position.Y;
            result.M43 = position.Z;
            result.M44 = 1.0f;

            return result;
        }

        public static Matrix4 Divide(Matrix4 matrix1, Matrix4 matrix2)
        {
            Matrix4 matrix;

            matrix.M11 = matrix1.M11 / matrix2.M11;
            matrix.M12 = matrix1.M12 / matrix2.M12;
            matrix.M13 = matrix1.M13 / matrix2.M13;
            matrix.M14 = matrix1.M14 / matrix2.M14;

            matrix.M21 = matrix1.M21 / matrix2.M21;
            matrix.M22 = matrix1.M22 / matrix2.M22;
            matrix.M23 = matrix1.M23 / matrix2.M23;
            matrix.M24 = matrix1.M24 / matrix2.M24;

            matrix.M31 = matrix1.M31 / matrix2.M31;
            matrix.M32 = matrix1.M32 / matrix2.M32;
            matrix.M33 = matrix1.M33 / matrix2.M33;
            matrix.M34 = matrix1.M34 / matrix2.M34;

            matrix.M41 = matrix1.M41 / matrix2.M41;
            matrix.M42 = matrix1.M42 / matrix2.M42;
            matrix.M43 = matrix1.M43 / matrix2.M43;
            matrix.M44 = matrix1.M44 / matrix2.M44;

            return matrix;
        }

        public static Matrix4 Divide(Matrix4 matrix1, float divider)
        {
            Matrix4 matrix;

            float oodivider = 1f / divider;
            matrix.M11 = matrix1.M11 * oodivider;
            matrix.M12 = matrix1.M12 * oodivider;
            matrix.M13 = matrix1.M13 * oodivider;
            matrix.M14 = matrix1.M14 * oodivider;

            matrix.M21 = matrix1.M21 * oodivider;
            matrix.M22 = matrix1.M22 * oodivider;
            matrix.M23 = matrix1.M23 * oodivider;
            matrix.M24 = matrix1.M24 * oodivider;

            matrix.M31 = matrix1.M31 * oodivider;
            matrix.M32 = matrix1.M32 * oodivider;
            matrix.M33 = matrix1.M33 * oodivider;
            matrix.M34 = matrix1.M34 * oodivider;

            matrix.M41 = matrix1.M41 * oodivider;
            matrix.M42 = matrix1.M42 * oodivider;
            matrix.M43 = matrix1.M43 * oodivider;
            matrix.M44 = matrix1.M44 * oodivider;

            return matrix;
        }

        public static Matrix4 Lerp(Matrix4 matrix1, Matrix4 matrix2, float amount)
        {
            Matrix4 matrix;

            matrix.M11 = matrix1.M11 + ((matrix2.M11 - matrix1.M11) * amount);
            matrix.M12 = matrix1.M12 + ((matrix2.M12 - matrix1.M12) * amount);
            matrix.M13 = matrix1.M13 + ((matrix2.M13 - matrix1.M13) * amount);
            matrix.M14 = matrix1.M14 + ((matrix2.M14 - matrix1.M14) * amount);

            matrix.M21 = matrix1.M21 + ((matrix2.M21 - matrix1.M21) * amount);
            matrix.M22 = matrix1.M22 + ((matrix2.M22 - matrix1.M22) * amount);
            matrix.M23 = matrix1.M23 + ((matrix2.M23 - matrix1.M23) * amount);
            matrix.M24 = matrix1.M24 + ((matrix2.M24 - matrix1.M24) * amount);

            matrix.M31 = matrix1.M31 + ((matrix2.M31 - matrix1.M31) * amount);
            matrix.M32 = matrix1.M32 + ((matrix2.M32 - matrix1.M32) * amount);
            matrix.M33 = matrix1.M33 + ((matrix2.M33 - matrix1.M33) * amount);
            matrix.M34 = matrix1.M34 + ((matrix2.M34 - matrix1.M34) * amount);

            matrix.M41 = matrix1.M41 + ((matrix2.M41 - matrix1.M41) * amount);
            matrix.M42 = matrix1.M42 + ((matrix2.M42 - matrix1.M42) * amount);
            matrix.M43 = matrix1.M43 + ((matrix2.M43 - matrix1.M43) * amount);
            matrix.M44 = matrix1.M44 + ((matrix2.M44 - matrix1.M44) * amount);

            return matrix;
        }

        public static Matrix4 Multiply(Matrix4 matrix1, Matrix4 matrix2)
        {
            return new Matrix4(
                matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 + matrix1.M13 * matrix2.M31 + matrix1.M14 * matrix2.M41,
                matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 + matrix1.M13 * matrix2.M32 + matrix1.M14 * matrix2.M42,
                matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 + matrix1.M13 * matrix2.M33 + matrix1.M14 * matrix2.M43,
                matrix1.M11 * matrix2.M14 + matrix1.M12 * matrix2.M24 + matrix1.M13 * matrix2.M34 + matrix1.M14 * matrix2.M44,

                matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 + matrix1.M23 * matrix2.M31 + matrix1.M24 * matrix2.M41,
                matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 + matrix1.M23 * matrix2.M32 + matrix1.M24 * matrix2.M42,
                matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 + matrix1.M23 * matrix2.M33 + matrix1.M24 * matrix2.M43,
                matrix1.M21 * matrix2.M14 + matrix1.M22 * matrix2.M24 + matrix1.M23 * matrix2.M34 + matrix1.M24 * matrix2.M44,

                matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix1.M33 * matrix2.M31 + matrix1.M34 * matrix2.M41,
                matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix1.M33 * matrix2.M32 + matrix1.M34 * matrix2.M42,
                matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 + matrix1.M33 * matrix2.M33 + matrix1.M34 * matrix2.M43,
                matrix1.M31 * matrix2.M14 + matrix1.M32 * matrix2.M24 + matrix1.M33 * matrix2.M34 + matrix1.M34 * matrix2.M44,

                matrix1.M41 * matrix2.M11 + matrix1.M42 * matrix2.M21 + matrix1.M43 * matrix2.M31 + matrix1.M44 * matrix2.M41,
                matrix1.M41 * matrix2.M12 + matrix1.M42 * matrix2.M22 + matrix1.M43 * matrix2.M32 + matrix1.M44 * matrix2.M42,
                matrix1.M41 * matrix2.M13 + matrix1.M42 * matrix2.M23 + matrix1.M43 * matrix2.M33 + matrix1.M44 * matrix2.M43,
                matrix1.M41 * matrix2.M14 + matrix1.M42 * matrix2.M24 + matrix1.M43 * matrix2.M34 + matrix1.M44 * matrix2.M44
            );
        }

        public static Matrix4 Multiply(Matrix4 matrix1, float scaleFactor)
        {
            Matrix4 matrix;
            matrix.M11 = matrix1.M11 * scaleFactor;
            matrix.M12 = matrix1.M12 * scaleFactor;
            matrix.M13 = matrix1.M13 * scaleFactor;
            matrix.M14 = matrix1.M14 * scaleFactor;

            matrix.M21 = matrix1.M21 * scaleFactor;
            matrix.M22 = matrix1.M22 * scaleFactor;
            matrix.M23 = matrix1.M23 * scaleFactor;
            matrix.M24 = matrix1.M24 * scaleFactor;

            matrix.M31 = matrix1.M31 * scaleFactor;
            matrix.M32 = matrix1.M32 * scaleFactor;
            matrix.M33 = matrix1.M33 * scaleFactor;
            matrix.M34 = matrix1.M34 * scaleFactor;

            matrix.M41 = matrix1.M41 * scaleFactor;
            matrix.M42 = matrix1.M42 * scaleFactor;
            matrix.M43 = matrix1.M43 * scaleFactor;
            matrix.M44 = matrix1.M44 * scaleFactor;
            return matrix;
        }

        public static Matrix4 Negate(Matrix4 matrix)
        {
            Matrix4 result;
            result.M11 = -matrix.M11;
            result.M12 = -matrix.M12;
            result.M13 = -matrix.M13;
            result.M14 = -matrix.M14;

            result.M21 = -matrix.M21;
            result.M22 = -matrix.M22;
            result.M23 = -matrix.M23;
            result.M24 = -matrix.M24;

            result.M31 = -matrix.M31;
            result.M32 = -matrix.M32;
            result.M33 = -matrix.M33;
            result.M34 = -matrix.M34;

            result.M41 = -matrix.M41;
            result.M42 = -matrix.M42;
            result.M43 = -matrix.M43;
            result.M44 = -matrix.M44;
            return result;
        }

        public static Matrix4 Subtract(Matrix4 matrix1, Matrix4 matrix2)
        {
            Matrix4 matrix;
            matrix.M11 = matrix1.M11 - matrix2.M11;
            matrix.M12 = matrix1.M12 - matrix2.M12;
            matrix.M13 = matrix1.M13 - matrix2.M13;
            matrix.M14 = matrix1.M14 - matrix2.M14;

            matrix.M21 = matrix1.M21 - matrix2.M21;
            matrix.M22 = matrix1.M22 - matrix2.M22;
            matrix.M23 = matrix1.M23 - matrix2.M23;
            matrix.M24 = matrix1.M24 - matrix2.M24;

            matrix.M31 = matrix1.M31 - matrix2.M31;
            matrix.M32 = matrix1.M32 - matrix2.M32;
            matrix.M33 = matrix1.M33 - matrix2.M33;
            matrix.M34 = matrix1.M34 - matrix2.M34;

            matrix.M41 = matrix1.M41 - matrix2.M41;
            matrix.M42 = matrix1.M42 - matrix2.M42;
            matrix.M43 = matrix1.M43 - matrix2.M43;
            matrix.M44 = matrix1.M44 - matrix2.M44;
            return matrix;
        }

        public static Matrix4 Transform(Matrix4 value, Quaternion rotation)
        {
            Matrix4 matrix;

            float x2 = rotation.X + rotation.X;
            float y2 = rotation.Y + rotation.Y;
            float z2 = rotation.Z + rotation.Z;

            float a = (1f - rotation.Y * y2) - rotation.Z * z2;
            float b = rotation.X * y2 - rotation.W * z2;
            float c = rotation.X * z2 + rotation.W * y2;
            float d = rotation.X * y2 + rotation.W * z2;
            float e = (1f - rotation.X * x2) - rotation.Z * z2;
            float f = rotation.Y * z2 - rotation.W * x2;
            float g = rotation.X * z2 - rotation.W * y2;
            float h = rotation.Y * z2 + rotation.W * x2;
            float i = (1f - rotation.X * x2) - rotation.Y * y2;

            matrix.M11 = ((value.M11 * a) + (value.M12 * b)) + (value.M13 * c);
            matrix.M12 = ((value.M11 * d) + (value.M12 * e)) + (value.M13 * f);
            matrix.M13 = ((value.M11 * g) + (value.M12 * h)) + (value.M13 * i);
            matrix.M14 = value.M14;

            matrix.M21 = ((value.M21 * a) + (value.M22 * b)) + (value.M23 * c);
            matrix.M22 = ((value.M21 * d) + (value.M22 * e)) + (value.M23 * f);
            matrix.M23 = ((value.M21 * g) + (value.M22 * h)) + (value.M23 * i);
            matrix.M24 = value.M24;

            matrix.M31 = ((value.M31 * a) + (value.M32 * b)) + (value.M33 * c);
            matrix.M32 = ((value.M31 * d) + (value.M32 * e)) + (value.M33 * f);
            matrix.M33 = ((value.M31 * g) + (value.M32 * h)) + (value.M33 * i);
            matrix.M34 = value.M34;

            matrix.M41 = ((value.M41 * a) + (value.M42 * b)) + (value.M43 * c);
            matrix.M42 = ((value.M41 * d) + (value.M42 * e)) + (value.M43 * f);
            matrix.M43 = ((value.M41 * g) + (value.M42 * h)) + (value.M43 * i);
            matrix.M44 = value.M44;

            return matrix;
        }

        public static Matrix4 Transpose(Matrix4 matrix)
        {
            Matrix4 result;

            result.M11 = matrix.M11;
            result.M12 = matrix.M21;
            result.M13 = matrix.M31;
            result.M14 = matrix.M41;

            result.M21 = matrix.M12;
            result.M22 = matrix.M22;
            result.M23 = matrix.M32;
            result.M24 = matrix.M42;

            result.M31 = matrix.M13;
            result.M32 = matrix.M23;
            result.M33 = matrix.M33;
            result.M34 = matrix.M43;

            result.M41 = matrix.M14;
            result.M42 = matrix.M24;
            result.M43 = matrix.M34;
            result.M44 = matrix.M44;

            return result;
        }

        public static Matrix4 Inverse3x3(Matrix4 matrix)
        {
            if (matrix.Determinant3x3() == 0f)
                throw new ArgumentException("Singular matrix inverse not possible");

            return (Adjoint3x3(matrix) / matrix.Determinant3x3());
        }

        public static Matrix4 Adjoint3x3(Matrix4 matrix)
        {
            Matrix4 adjointMatrix = new Matrix4();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                    adjointMatrix[i,j] = (float)(Math.Pow(-1, i + j) * (Minor(matrix, i, j).Determinant3x3()));
            }

            adjointMatrix = Transpose(adjointMatrix);
            return adjointMatrix;
        }

        public static Matrix4 Inverse(Matrix4 matrix)
        {
            if (matrix.Determinant() == 0f)
                throw new ArgumentException("Singular matrix inverse not possible");

            return (Adjoint(matrix) / matrix.Determinant());
        }

        public static Matrix4 Adjoint(Matrix4 matrix)
        {
            Matrix4 adjointMatrix = new Matrix4();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                    adjointMatrix[i,j] = (float)(Math.Pow(-1, i + j) * ((Minor(matrix, i, j)).Determinant3x3()));
            }

            adjointMatrix = Transpose(adjointMatrix);
            return adjointMatrix;
        }

        public static Matrix4 Minor(Matrix4 matrix, int row, int col)
        {
            Matrix4 minor = new Matrix4();
            int m = 0, n = 0;

            for (int i = 0; i < 4; i++)
            {
                if (i == row)
                    continue;
                n = 0;
                for (int j = 0; j < 4; j++)
                {
                    if (j == col)
                        continue;
                    minor[m,n] = matrix[i,j];
                    n++;
                }
                m++;
            }

            return minor;
        }
        
        #endregion Static Methods

        #region Overrides

        public override bool Equals(object obj)
        {
            return (obj is Matrix4) ? this.Equals((Matrix4)obj) : false;
        }

        public bool Equals(Matrix4 other)
        {
            return M11 == other.M11 && M12 == other.M12 && M13 == other.M13 && M14 == other.M14 &&
                   M21 == other.M21 && M22 == other.M22 && M23 == other.M23 && M24 == other.M24 &&
                   M31 == other.M31 && M32 == other.M32 && M33 == other.M33 && M14 == other.M34 &&
                   M41 == other.M41 && M42 == other.M42 && M43 == other.M43 && M44 == other.M44;
        }

        public override int GetHashCode()
        {
            return
                M11.GetHashCode() ^ M12.GetHashCode() ^ M13.GetHashCode() ^ M14.GetHashCode() ^
                M21.GetHashCode() ^ M22.GetHashCode() ^ M23.GetHashCode() ^ M24.GetHashCode() ^
                M31.GetHashCode() ^ M32.GetHashCode() ^ M33.GetHashCode() ^ M34.GetHashCode() ^
                M41.GetHashCode() ^ M42.GetHashCode() ^ M43.GetHashCode() ^ M44.GetHashCode();
        }

        /// <summary>
        /// Get a formatted string representation of the vector
        /// </summary>
        /// <returns>A string representation of the vector</returns>
        public override string ToString()
        {
            return string.Format(Utils.EnUsCulture,
                "|{0}, {1}, {2}, {3}|\n|{4}, {5}, {6}, {7}|\n|{8}, {9}, {10}, {11}|\n|{12}, {13}, {14}, {15}|",
                M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44);
        }

        #endregion Overrides

        #region Operators

        public static bool operator ==(Matrix4 left, Matrix4 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Matrix4 left, Matrix4 right)
        {
            return !left.Equals(right);
        }

        public static Matrix4 operator +(Matrix4 left, Matrix4 right)
        {
            return Add(left, right);
        }

        public static Matrix4 operator -(Matrix4 matrix)
        {
            return Negate(matrix);
        }

        public static Matrix4 operator -(Matrix4 left, Matrix4 right)
        {
            return Subtract(left, right);
        }

        public static Matrix4 operator *(Matrix4 left, Matrix4 right)
        {
            return Multiply(left, right);
        }

        public static Matrix4 operator *(Matrix4 left, float scalar)
        {
            return Multiply(left, scalar);
        }

        public static Matrix4 operator /(Matrix4 left, Matrix4 right)
        {
            return Divide(left, right);
        }

        public static Matrix4 operator /(Matrix4 matrix, float divider)
        {
            return Divide(matrix, divider);
        }

        public Vector4 this[int row]
        {
            get
            {
                switch (row)
                {
                    case 0:
                        return new Vector4(M11, M12, M13, M14);
                    case 1:
                        return new Vector4(M21, M22, M23, M24);
                    case 2:
                        return new Vector4(M31, M32, M33, M34);
                    case 3:
                        return new Vector4(M41, M42, M43, M44);
                    default:
                        throw new IndexOutOfRangeException("Matrix4 row index must be from 0-3");
                }
            }
            set
            {
                switch (row)
                {
                    case 0:
                        M11 = value.X;
                        M12 = value.Y;
                        M13 = value.Z;
                        M14 = value.W;
                        break;
                    case 1:
                        M21 = value.X;
                        M22 = value.Y;
                        M23 = value.Z;
                        M24 = value.W;
                        break;
                    case 2:
                        M31 = value.X;
                        M32 = value.Y;
                        M33 = value.Z;
                        M34 = value.W;
                        break;
                    case 3:
                        M41 = value.X;
                        M42 = value.Y;
                        M43 = value.Z;
                        M44 = value.W;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Matrix4 row index must be from 0-3");
                }
            }
        }

        public float this[int row, int column]
        {
            get
            {
                switch (row)
                {
                    case 0:
                        switch (column)
                        {
                            case 0:
                                return M11;
                            case 1:
                                return M12;
                            case 2:
                                return M13;
                            case 3:
                                return M14;
                            default:
                                throw new IndexOutOfRangeException("Matrix4 row and column values must be from 0-3");
                        }
                    case 1:
                        switch (column)
                        {
                            case 0:
                                return M21;
                            case 1:
                                return M22;
                            case 2:
                                return M23;
                            case 3:
                                return M24;
                            default:
                                throw new IndexOutOfRangeException("Matrix4 row and column values must be from 0-3");
                        }
                    case 2:
                        switch (column)
                        {
                            case 0:
                                return M31;
                            case 1:
                                return M32;
                            case 2:
                                return M33;
                            case 3:
                                return M34;
                            default:
                                throw new IndexOutOfRangeException("Matrix4 row and column values must be from 0-3");
                        }
                    case 3:
                        switch (column)
                        {
                            case 0:
                                return M41;
                            case 1:
                                return M42;
                            case 2:
                                return M43;
                            case 3:
                                return M44;
                            default:
                                throw new IndexOutOfRangeException("Matrix4 row and column values must be from 0-3");
                        }
                    default:
                        throw new IndexOutOfRangeException("Matrix4 row and column values must be from 0-3");
                }
            }
            set
            {
                switch (row)
                {
                    case 0:
                        switch (column)
                        {
                            case 0:
                                M11 = value; return;
                            case 1:
                                M12 = value; return;
                            case 2:
                                M13 = value; return;
                            case 3:
                                M14 = value; return;
                            default:
                                throw new IndexOutOfRangeException("Matrix4 row and column values must be from 0-3");
                        }
                    case 1:
                        switch (column)
                        {
                            case 0:
                                M21 = value; return;
                            case 1:
                                M22 = value; return;
                            case 2:
                                M23 = value; return;
                            case 3:
                                M24 = value; return;
                            default:
                                throw new IndexOutOfRangeException("Matrix4 row and column values must be from 0-3");
                        }
                    case 2:
                        switch (column)
                        {
                            case 0:
                                M31 = value; return;
                            case 1:
                                M32 = value; return;
                            case 2:
                                M33 = value; return;
                            case 3:
                                M34 = value; return;
                            default:
                                throw new IndexOutOfRangeException("Matrix4 row and column values must be from 0-3");
                        }
                    case 3:
                        switch (column)
                        {
                            case 0:
                                M41 = value; return;
                            case 1:
                                M42 = value; return;
                            case 2:
                                M43 = value; return;
                            case 3:
                                M44 = value; return;
                            default:
                                throw new IndexOutOfRangeException("Matrix4 row and column values must be from 0-3");
                        }
                    default:
                        throw new IndexOutOfRangeException("Matrix4 row and column values must be from 0-3");
                }
            }
        }

        #endregion Operators

        /// <summary>A 4x4 matrix containing all zeroes</summary>
        public static readonly Matrix4 Zero = new Matrix4();

        /// <summary>A 4x4 identity matrix</summary>
        public static readonly Matrix4 Identity = new Matrix4(
            1f, 0f, 0f, 0f,
            0f, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f);
    }
}
