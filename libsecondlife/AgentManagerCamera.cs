using System;

namespace libsecondlife
{
    public partial class AgentManager
    {
        public partial class AgentMovement
        {
            /// <summary>
            /// 
            /// </summary>
            public class AgentCamera
            {
                /// <summary></summary>
                public float Far;

                /// <summary>The camera is a local frame of reference inside of
                /// the larger grid space. This is where the math happens</summary>
                private CoordinateFrame Frame;

                /// <summary></summary>
                public LLVector3 Position
                {
                    get { return Frame.Origin; }
                    set { Frame.Origin = value; }
                }
                /// <summary></summary>
                public LLVector3 AtAxis
                {
                    get { return Frame.YAxis; }
                    set { Frame.YAxis = value; }
                }
                /// <summary></summary>
                public LLVector3 LeftAxis
                {
                    get { return Frame.XAxis; }
                    set { Frame.XAxis = value; }
                }
                /// <summary></summary>
                public LLVector3 UpAxis
                {
                    get { return Frame.ZAxis; }
                    set { Frame.ZAxis = value; }
                }

                /// <summary>
                /// Default constructor
                /// </summary>
                public AgentCamera()
                {
                    Frame = new CoordinateFrame(new LLVector3(128f, 128f, 20f));
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

                public void LookDirection(LLVector3 target)
                {
                    Frame.LookDirection(target);
                }

                public void LookDirection(LLVector3 target, LLVector3 upDirection)
                {
                    Frame.LookDirection(target, upDirection);
                }

                public void LookDirection(double heading)
                {
                    Frame.LookDirection(heading);
                }

                public void LookAt(LLVector3 position, LLVector3 target)
                {
                    Frame.LookAt(position, target);
                }

                public void LookAt(LLVector3 position, LLVector3 target, LLVector3 upDirection)
                {
                    Frame.LookAt(position, target, upDirection);
                }

                public void SetPositionOrientation(LLVector3 position, float roll, float pitch, float yaw)
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
