/*
 * Copyright (c) 2007, Second Life Reverse Engineering Team
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

namespace libsecondlife
{
    public class CoordinateFrame
    {
        public LLVector3 Origin { get { return origin; } }
        public LLVector3 XAxis { get { return xAxis; } }
        public LLVector3 YAxis { get { return yAxis; } }
        public LLVector3 ZAxis { get { return zAxis; } }

        public LLVector3 AtAxis { get { return xAxis; } }
        public LLVector3 LeftAxis { get { return yAxis; } }
        public LLVector3 UpAxis { get { return zAxis; } }

        protected LLVector3 origin;
        protected LLVector3 xAxis;
        protected LLVector3 yAxis;
        protected LLVector3 zAxis;

        public CoordinateFrame()
        {
        }

        public CoordinateFrame(LLVector3 origin, LLVector3 xAxis, LLVector3 yAxis, LLVector3 zAxis)
        {
            this.origin = origin;
            this.xAxis = xAxis;
            this.yAxis = yAxis;
            this.zAxis = zAxis;
        }

        //public void SetAxes(LLQuaternion rotation)
        //{
        //    // FIXME: Convert to a matrix and call SetAxes()
        //}

        //public void SetAxes(LLMatrix3 rotation)
        //{
        //    // FIXME: Implement
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="at">Looking direction, must be a normalized vector</param>
        /// <param name="upDirection">Up direction, must be a normalized vector</param>
        public void LookDirection(LLVector3 at, LLVector3 upDirection)
        {
            // The two parameters cannot be parallel
            LLVector3 left = Helpers.VecCross(upDirection, at);
            if (left == LLVector3.Zero)
            {
                // Prevent left from being zero
                at.X += 0.01f;
                at = Helpers.VecNorm(at);
                left = Helpers.VecCross(upDirection, at);
            }
            left = Helpers.VecNorm(left);

            xAxis = at;
            yAxis = left;
            zAxis = Helpers.VecCross(at, left);
        }

        public void LookAt(LLVector3 origin, LLVector3 target, LLVector3 upDirection)
        {
            this.origin = origin;
            LLVector3 at = new LLVector3(target - origin);
            at = Helpers.VecNorm(at);

            LookDirection(at, upDirection);
        }
    }
}
