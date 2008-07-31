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

namespace OpenMetaverse
{
    public class CoordinateFrame
    {
        public static readonly Vector3 X_AXIS = new Vector3(1f, 0f, 0f);
        public static readonly Vector3 Y_AXIS = new Vector3(0f, 1f, 0f);
        public static readonly Vector3 Z_AXIS = new Vector3(0f, 0f, 1f);

        /// <summary>Origin position of this coordinate frame</summary>
        public Vector3 Origin
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
        public Vector3 XAxis
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
        public Vector3 YAxis
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
        public Vector3 ZAxis
        {
            get { return zAxis; }
            set
            {
                if (!value.IsFinite())
                    throw new ArgumentException("Non-finite in CoordinateFrame.ZAxis assignment");
                zAxis = value;
            }
        }

        protected Vector3 origin;
        protected Vector3 xAxis;
        protected Vector3 yAxis;
        protected Vector3 zAxis;

        #region Constructors

        public CoordinateFrame(Vector3 origin)
        {
            this.origin = origin;
            xAxis = X_AXIS;
            yAxis = Y_AXIS;
            zAxis = Z_AXIS;

            if (!this.origin.IsFinite())
                throw new ArgumentException("Non-finite in CoordinateFrame constructor");
        }

        public CoordinateFrame(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            LookDirection(direction);

            if (!IsFinite())
                throw new ArgumentException("Non-finite in CoordinateFrame constructor");
        }

        public CoordinateFrame(Vector3 origin, Vector3 xAxis, Vector3 yAxis, Vector3 zAxis)
        {
            this.origin = origin;
            this.xAxis = xAxis;
            this.yAxis = yAxis;
            this.zAxis = zAxis;

            if (!IsFinite())
                throw new ArgumentException("Non-finite in CoordinateFrame constructor");
        }

        public CoordinateFrame(Vector3 origin, Matrix3 rotation)
        {
            this.origin = origin;
            xAxis = rotation[0];
            yAxis = rotation[1];
            zAxis = rotation[2];

            if (!IsFinite())
                throw new ArgumentException("Non-finite in CoordinateFrame constructor");
        }

        public CoordinateFrame(Vector3 origin, Quaternion rotation)
        {
            Matrix3 m = rotation.ToMatrix3();

            this.origin = origin;
            xAxis = m[0];
            yAxis = m[1];
            zAxis = m[2];

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

        public void Rotate(float angle, Vector3 rotationAxis)
        {
            Quaternion q = new Quaternion(angle, rotationAxis);
            Rotate(q);
        }

        public void Rotate(Quaternion q)
        {
            Matrix3 m = q.ToMatrix3();
            Rotate(m);
        }

        public void Rotate(Matrix3 m)
        {
            xAxis = Vector3.Rot(xAxis, m);
            yAxis = Vector3.Rot(yAxis, m);

            Orthonormalize();

            if (!IsFinite())
                throw new Exception("Non-finite in CoordinateFrame.Rotate()");
        }

        public void Roll(float angle)
        {
            Quaternion q = new Quaternion(angle, xAxis);
            Matrix3 m = q.ToMatrix3();
            Rotate(m);

            if (!yAxis.IsFinite() || !zAxis.IsFinite())
                throw new Exception("Non-finite in CoordinateFrame.Roll()");
        }

        public void Pitch(float angle)
        {
            Quaternion q = new Quaternion(angle, yAxis);
            Matrix3 m = q.ToMatrix3();
            Rotate(m);

            if (!xAxis.IsFinite() || !zAxis.IsFinite())
                throw new Exception("Non-finite in CoordinateFrame.Pitch()");
        }

        public void Yaw(float angle)
        {
            Quaternion q = new Quaternion(angle, zAxis);
            Matrix3 m = q.ToMatrix3();
            Rotate(m);

            if (!xAxis.IsFinite() || !yAxis.IsFinite())
                throw new Exception("Non-finite in CoordinateFrame.Yaw()");
        }

        public void LookDirection(Vector3 at)
        {
            LookDirection(at, Z_AXIS);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="at">Looking direction, must be a normalized vector</param>
        /// <param name="upDirection">Up direction, must be a normalized vector</param>
        public void LookDirection(Vector3 at, Vector3 upDirection)
        {
            // The two parameters cannot be parallel
            Vector3 left = Vector3.Cross(upDirection, at);
            if (left == Vector3.Zero)
            {
                // Prevent left from being zero
                at.X += 0.01f;
                at = Vector3.Norm(at);
                left = Vector3.Cross(upDirection, at);
            }
            left = Vector3.Norm(left);

            xAxis = at;
            yAxis = left;
            zAxis = Vector3.Cross(at, left);
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

        public void LookAt(Vector3 origin, Vector3 target)
        {
            LookAt(origin, target, new Vector3(0f, 0f, 1f));
        }

        public void LookAt(Vector3 origin, Vector3 target, Vector3 upDirection)
        {
            this.origin = origin;
            Vector3 at = new Vector3(target - origin);
            at = Vector3.Norm(at);

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
            xAxis = Vector3.Norm(xAxis);
            yAxis -= xAxis * (xAxis * yAxis);
            yAxis = Vector3.Norm(yAxis);
            zAxis = Vector3.Cross(xAxis, yAxis);
        }
    }
}
