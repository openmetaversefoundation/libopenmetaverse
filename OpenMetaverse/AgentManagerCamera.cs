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
    public partial class AgentManager
    {
        public partial class AgentMovement
        {
            /// <summary>
            /// Camera controls for the agent, mostly a thin wrapper around
            /// CoordinateFrame. This class is only responsible for state
            /// tracking and math, it does not send any packets
            /// </summary>
            public class AgentCamera
            {
                /// <summary></summary>
                public float Far;

                /// <summary>The camera is a local frame of reference inside of
                /// the larger grid space. This is where the math happens</summary>
                private CoordinateFrame Frame;

                /// <summary></summary>
                public Vector3 Position
                {
                    get { return Frame.Origin; }
                    set { Frame.Origin = value; }
                }
                /// <summary></summary>
                public Vector3 AtAxis
                {
                    get { return Frame.YAxis; }
                    set { Frame.YAxis = value; }
                }
                /// <summary></summary>
                public Vector3 LeftAxis
                {
                    get { return Frame.XAxis; }
                    set { Frame.XAxis = value; }
                }
                /// <summary></summary>
                public Vector3 UpAxis
                {
                    get { return Frame.ZAxis; }
                    set { Frame.ZAxis = value; }
                }

                /// <summary>
                /// Default constructor
                /// </summary>
                public AgentCamera()
                {
                    Frame = new CoordinateFrame(new Vector3(128f, 128f, 20f));
                    Far = 128f;
                }

                public void Roll(float angle)
                {
                    Frame.Roll(angle);
                }

                public void Pitch(float angle)
                {
                    Frame.Pitch(angle);
                }

                public void Yaw(float angle)
                {
                    Frame.Yaw(angle);
                }

                public void LookDirection(Vector3 target)
                {
                    Frame.LookDirection(target);
                }

                public void LookDirection(Vector3 target, Vector3 upDirection)
                {
                    Frame.LookDirection(target, upDirection);
                }

                public void LookDirection(double heading)
                {
                    Frame.LookDirection(heading);
                }

                public void LookAt(Vector3 position, Vector3 target)
                {
                    Frame.LookAt(position, target);
                }

                public void LookAt(Vector3 position, Vector3 target, Vector3 upDirection)
                {
                    Frame.LookAt(position, target, upDirection);
                }

                public void SetPositionOrientation(Vector3 position, float roll, float pitch, float yaw)
                {
                    Frame.Origin = position;

                    Frame.ResetAxes();

                    Frame.Roll(roll);
                    Frame.Pitch(pitch);
                    Frame.Yaw(yaw);
                }
            }
        }
    }
}
