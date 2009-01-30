/*
 * Copyright (c) 2007-2008, openmetaverse.org
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
using Mono.Simd.Math;

namespace OpenMetaverse
{
    public class CoordinateFrame
    {
        public static readonly Vector3f X_AXIS = new Vector3f(1f, 0f, 0f);
        public static readonly Vector3f Y_AXIS = new Vector3f(0f, 1f, 0f);
        public static readonly Vector3f Z_AXIS = new Vector3f(0f, 0f, 1f);

        /// <summary>Origin position of this coordinate frame</summary>
        public Vector3f Origin
        {
            get { return origin; }
            set
            {
                if (!value.IsFinite())
                    throw new ArgumentException("Non-finite in CoordinateFrame.Origin assignment");
                origin = value;
            }
        }
        /// <summary>X axis of this coordinate frame, or Forward/At in grid terms</summary>
        public Vector3f XAxis
        {
            get { return xAxis; }
            set
            {
                if (!value.IsFinite())
                    throw new ArgumentException("Non-finite in CoordinateFrame.XAxis assignment");
                xAxis = value;
            }
        }
        /// <summary>Y axis of this coordinate frame, or Left in grid terms</summary>
        public Vector3f YAxis
        {
            get { return yAxis; }
            set
            {
                if (!value.IsFinite())
                    throw new ArgumentException("Non-finite in CoordinateFrame.YAxis assignment");
                yAxis = value;
            }
        }
        /// <summary>Z axis of this coordinate frame, or Up in grid terms</summary>
        public Vector3f ZAxis
        {
            get { return zAxis; }
            set
            {
                if (!value.IsFinite())
                    throw new ArgumentException("Non-finite in CoordinateFrame.ZAxis assignment");
                zAxis = value;
            }
        }

        protected Vector3f origin;
        protected Vector3f xAxis;
        protected Vector3f yAxis;
        protected Vector3f zAxis;

        #region Constructors

        public CoordinateFrame(Vector3f origin)
        {
            this.origin = origin;
            xAxis = X_AXIS;
            yAxis = Y_AXIS;
            zAxis = Z_AXIS;

            if (!this.origin.IsFinite())
                throw new ArgumentException("Non-finite in CoordinateFrame constructor");
        }

        public CoordinateFrame(Vector3f origin, Vector3f direction)
        {
            this.origin = origin;
            LookDirection(direction);

            if (!IsFinite())
                throw new ArgumentException("Non-finite in CoordinateFrame constructor");
        }

        public CoordinateFrame(Vector3f origin, Vector3f xAxis, Vector3f yAxis, Vector3f zAxis)
        {
            this.origin = origin;
            this.xAxis = xAxis;
            this.yAxis = yAxis;
            this.zAxis = zAxis;

            if (!IsFinite())
                throw new ArgumentException("Non-finite in CoordinateFrame constructor");
        }

        public CoordinateFrame(Vector3f origin, Matrix4f rotation)
        {
            this.origin = origin;
            xAxis = new Vector3f(rotation.M11, rotation.M21, rotation.M31);
            yAxis = new Vector3f(rotation.M12, rotation.M22, rotation.M32);
            zAxis = new Vector3f(rotation.M13, rotation.M23, rotation.M33);

            if (!IsFinite())
                throw new ArgumentException("Non-finite in CoordinateFrame constructor");
        }

        public CoordinateFrame(Vector3f origin, Quaternionf rotation)
        {
            Matrix4f m = new Matrix4f();
            m.FromQuaternion(rotation);

            this.origin = origin;
            xAxis = new Vector3f(m.M11, m.M21, m.M31);
            yAxis = new Vector3f(m.M12, m.M22, m.M32);
            zAxis = new Vector3f(m.M13, m.M23, m.M33);

            if (!IsFinite())
                throw new ArgumentException("Non-finite in CoordinateFrame constructor");
        }

        #endregion Constructors

        #region Public Methods

        public void ResetAxes()
        {
            xAxis = X_AXIS;
            yAxis = Y_AXIS;
            zAxis = Z_AXIS;
        }

        public void Rotate(float angle, Vector3f rotationAxis)
        {
            Quaternionf q = new Quaternionf();
            q.FromAxisAngle(rotationAxis, angle);
            Rotate(q);
        }

        public void Rotate(Quaternionf q)
        {
            Matrix4f m = new Matrix4f();
            m.FromQuaternion(q);
            Rotate(m);
        }

        public void Rotate(Matrix4f m)
        {
            xAxis = xAxis.Transform(ref m);
            yAxis = yAxis.Transform(ref m);

            Orthonormalize();

            if (!IsFinite())
                throw new Exception("Non-finite in CoordinateFrame.Rotate()");
        }

        public void Roll(float angle)
        {
            Quaternionf q = new Quaternionf();
            q.FromAxisAngle(xAxis, angle);
            Matrix4f m = new Matrix4f();
            m.FromQuaternion(q);
            Rotate(m);

            if (!yAxis.IsFinite() || !zAxis.IsFinite())
                throw new Exception("Non-finite in CoordinateFrame.Roll()");
        }

        public void Pitch(float angle)
        {
            Quaternionf q = new Quaternionf();
            q.FromAxisAngle(yAxis, angle);
            Matrix4f m = new Matrix4f();
            m.FromQuaternion(q);
            Rotate(m);

            if (!xAxis.IsFinite() || !zAxis.IsFinite())
                throw new Exception("Non-finite in CoordinateFrame.Pitch()");
        }

        public void Yaw(float angle)
        {
            Quaternionf q = new Quaternionf();
            q.FromAxisAngle(zAxis, angle);
            Matrix4f m = new Matrix4f();
            m.FromQuaternion(q);
            Rotate(m);

            if (!xAxis.IsFinite() || !yAxis.IsFinite())
                throw new Exception("Non-finite in CoordinateFrame.Yaw()");
        }

        public void LookDirection(Vector3f at)
        {
            LookDirection(at, Z_AXIS);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="at">Looking direction, must be a normalized vector</param>
        /// <param name="upDirection">Up direction, must be a normalized vector</param>
        public void LookDirection(Vector3f at, Vector3f upDirection)
        {
            // The two parameters cannot be parallel
            Vector3f left = upDirection.Cross(ref at);
            if (left == Vector3f.Zero)
            {
                // Prevent left from being zero
                at.X += 0.01f;
                at.Normalize();
                left = upDirection.Cross(ref at);
            }
            left.Normalize();

            xAxis = at;
            yAxis = left;
            zAxis = at.Cross(ref left);
        }

        /// <summary>
        /// Align the coordinate frame X and Y axis with a given rotation
        /// around the Z axis in radians
        /// </summary>
        /// <param name="heading">Absolute rotation around the Z axis in
        /// radians</param>
        public void LookDirection(double heading)
        {
            yAxis.X = (float)Math.Cos(heading);
            yAxis.Y = (float)Math.Sin(heading);
            xAxis.X = (float)-Math.Sin(heading);
            xAxis.Y = (float)Math.Cos(heading);
        }

        public void LookAt(Vector3f origin, Vector3f target)
        {
            LookAt(origin, target, new Vector3f(0f, 0f, 1f));
        }

        public void LookAt(Vector3f origin, Vector3f target, Vector3f upDirection)
        {
            this.origin = origin;
            Vector3f at = target - origin;
            at.Normalize();

            LookDirection(at, upDirection);
        }

        #endregion Public Methods

        protected bool IsFinite()
        {
            if (xAxis.IsFinite() && yAxis.IsFinite() && zAxis.IsFinite())
                return true;
            else
                return false;
        }

        protected void Orthonormalize()
        {
            // Make sure the axis are orthagonal and normalized
            xAxis.Normalize();
            yAxis -= xAxis * (xAxis * yAxis);
            yAxis.Normalize();
            zAxis = xAxis.Cross(ref yAxis);
        }
    }
}
