/* 
	* Copyright (c) 2006-2007, Second Life Reverse Engineering Team 
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
using System.Timers;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife
{
    public partial class MainAvatar
    {
        /// <summary> 
        /// Holds current camera and control key status
        /// </summary> 
        public class MainAvatarStatus
        {
            #region Structs

            /// <summary>
            /// 
            /// </summary>
            public struct CameraStatus
            {
                /// <summary></summary>
                public LLQuaternion BodyRotation;
                /// <summary></summary>
                public LLQuaternion HeadRotation;
                /// <summary></summary>
                public LLVector3 CameraAtAxis;
                /// <summary></summary>
                public LLVector3 CameraCenter;
                /// <summary></summary>
                public LLVector3 CameraLeftAxis;
                /// <summary></summary>
                public LLVector3 CameraUpAxis;
                /// <summary></summary>
                public float Far;
                /// <summary></summary>
                internal LLQuaternion LastBodyRotation;
                /// <summary></summary>
                internal LLQuaternion LastHeadRotation;
                /// <summary></summary>
                internal LLVector3 LastCameraAtAxis;
                /// <summary></summary>
                internal LLVector3 LastCameraCenter;
                /// <summary></summary>
                internal LLVector3 LastCameraLeftAxis;
                /// <summary></summary>
                internal LLVector3 LastCameraUpAxis;
                /// <summary></summary>
                internal float LastFar;
            }

            #endregion Structs

            #region Properties

            /// <summary></summary>
            public bool AtPos
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_AT_POS); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_AT_POS, value); }
            }
            /// <summary></summary>
            public bool AtNeg
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_AT_NEG); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_AT_NEG, value); }
            }
            /// <summary></summary>
            public bool LeftPos
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_LEFT_POS); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_LEFT_POS, value); }
            }
            /// <summary></summary>
            public bool LeftNeg
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_LEFT_NEG); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_LEFT_NEG, value); }
            }
            /// <summary></summary>
            public bool UpPos
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_UP_POS); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_UP_POS, value); }
            }
            /// <summary></summary>
            public bool UpNeg
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_UP_NEG); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_UP_NEG, value); }
            }
            /// <summary></summary>
            public bool PitchPos
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_PITCH_POS); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_PITCH_POS, value); }
            }
            /// <summary></summary>
            public bool PitchNeg
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_PITCH_NEG); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_PITCH_NEG, value); }
            }
            /// <summary></summary>
            public bool YawPos
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_YAW_POS); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_YAW_POS, value); }
            }
            /// <summary></summary>
            public bool YawNeg
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_YAW_NEG); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_YAW_NEG, value); }
            }
            /// <summary></summary>
            public bool FastAt
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_FAST_AT); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_FAST_AT, value); }
            }
            /// <summary></summary>
            public bool FastLeft
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_FAST_LEFT); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_FAST_LEFT, value); }
            }
            /// <summary></summary>
            public bool FastUp
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_FAST_UP); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_FAST_UP, value); }
            }
            /// <summary></summary>
            public bool Fly
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_FLY); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_FLY, value); }
            }
            /// <summary></summary>
            public bool Stop
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_STOP); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_STOP, value); }
            }
            /// <summary></summary>
            public bool FinishAnim
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_FINISH_ANIM); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_FINISH_ANIM, value); }
            }
            /// <summary></summary>
            public bool StandUp
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_STAND_UP); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_STAND_UP, value); }
            }
            /// <summary></summary>
            public bool SitOnGround
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_SIT_ON_GROUND); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_SIT_ON_GROUND, value); }
            }
            /// <summary></summary>
            public bool Mouselook
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_MOUSELOOK); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_MOUSELOOK, value); }
            }
            /// <summary></summary>
            public bool NudgeAtPos
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_NUDGE_AT_POS); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_NUDGE_AT_POS, value); }
            }
            /// <summary></summary>
            public bool NudgeAtNeg
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_NUDGE_AT_NEG); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_NUDGE_AT_NEG, value); }
            }
            /// <summary></summary>
            public bool NudgeLeftPos
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_NUDGE_LEFT_POS); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_NUDGE_LEFT_POS, value); }
            }
            /// <summary></summary>
            public bool NudgeLeftNeg
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_NUDGE_LEFT_NEG); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_NUDGE_LEFT_NEG, value); }
            }
            /// <summary></summary>
            public bool NudgeUpPos
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_NUDGE_UP_POS); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_NUDGE_UP_POS, value); }
            }
            /// <summary></summary>
            public bool NudgeUpNeg
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_NUDGE_UP_NEG); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_NUDGE_UP_NEG, value); }
            }
            /// <summary></summary>
            public bool TurnLeft
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_TURN_LEFT); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_TURN_LEFT, value); }
            }
            /// <summary></summary>
            public bool TurnRight
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_TURN_RIGHT); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_TURN_RIGHT, value); }
            }
            /// <summary></summary>
            public bool Away
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_AWAY); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_AWAY, value); }
            }
            /// <summary></summary>
            public bool LButtonDown
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_LBUTTON_DOWN); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_LBUTTON_DOWN, value); }
            }
            /// <summary></summary>
            public bool LButtonUp
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_LBUTTON_UP); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_LBUTTON_UP, value); }
            }
            /// <summary></summary>
            public bool MLButtonDown
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_ML_LBUTTON_DOWN); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_ML_LBUTTON_DOWN, value); }
            }
            /// <summary></summary>
            public bool MLButtonUp
            {
                get { return GetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_ML_LBUTTON_UP); }
                set { SetControlFlag(MainAvatar.ControlFlags.AGENT_CONTROL_ML_LBUTTON_UP, value); }
            }
            /// <summary>
            /// Returns "always run" value, or changes it by sending a SetAlwaysRunPacket
            /// </summary>
            public bool AlwaysRun
            {
                get
                {
                    return alwaysRun;
                }
                set
                {
                    alwaysRun = value;
                    SetAlwaysRunPacket run = new SetAlwaysRunPacket();
                    run.AgentData.AgentID = Client.Network.AgentID;
                    run.AgentData.SessionID = Client.Network.SessionID;
                    run.AgentData.AlwaysRun = alwaysRun;
                    Client.Network.SendPacket(run);
                }
            }
            /// <summary>The current value of the agent control flags</summary>
            public uint AgentControls
            {
                get
                {
                    return agentControls;
                }
            }

            #endregion Properties


            /// <summary>Timer for sending AgentUpdate packets, disabled by
            /// default</summary>
            public Timer UpdateTimer;
            /// <summary>Holds camera flags</summary>
            public CameraStatus Camera;
            /// <summary>Currently only used for hiding your group title</summary>
            public AgentFlags Flags = AgentFlags.None;
            /// <summary>Action state of the avatar, which can currently be
            /// typing and editing</summary>
            public AgentState State = AgentState.None;

            private bool alwaysRun = false;
            private SecondLife Client;
            private uint agentControls;
            private int duplicateCount = 0;
            private AgentState lastState;


            /// <summary>Constructor for class MainAvatarStatus</summary>
            public MainAvatarStatus(SecondLife client)
            {
                Client = client;

                // Lets set some default camera values
                Camera.BodyRotation = LLQuaternion.Identity;
                Camera.HeadRotation = LLQuaternion.Identity;
                Camera.CameraCenter = new LLVector3(128,128,20);
                Camera.CameraAtAxis = new LLVector3(0, 0.9999f, 0);
                Camera.CameraLeftAxis = new LLVector3(0.9999f, 0, 0);
                Camera.CameraUpAxis = new LLVector3(0, 0, 0.9999f);
                Camera.Far = 384.0f;

                UpdateTimer = new Timer(Settings.AGENT_UPDATE_INTERVAL);
                UpdateTimer.Elapsed += new ElapsedEventHandler(UpdateTimer_Elapsed);
                UpdateTimer.Enabled = Client.Settings.SEND_AGENT_UPDATES;
            }

            /// <summary>
            /// Send an AgentUpdate with the camera set at the current agent
            /// position and pointing towards the heading specified
            /// </summary>
            /// <param name="heading">Camera rotation in radians</param>
            /// <param name="reliable">Whether to send the AgentUpdate reliable
            /// or not</param>
            public void UpdateFromHeading(double heading, bool reliable)
            {
                Client.Self.Status.Camera.CameraCenter = Client.Self.Position;
                Client.Self.Status.Camera.CameraAtAxis.X = (float)Math.Cos(heading);
                Client.Self.Status.Camera.CameraAtAxis.Y = (float)Math.Sin(heading);
                Client.Self.Status.Camera.CameraLeftAxis.X = (float)-Math.Sin(heading);
                Client.Self.Status.Camera.CameraLeftAxis.Y = (float)Math.Cos(heading);
                Client.Self.Status.Camera.BodyRotation.Z = (float)Math.Sin(heading / 2.0d);
                Client.Self.Status.Camera.BodyRotation.W = (float)Math.Cos(heading / 2.0d);
                Client.Self.Status.Camera.HeadRotation = Client.Self.Status.Camera.BodyRotation;

                Client.Self.Status.SendUpdate(reliable);
            }

            /// <summary>
            /// Send new AgentUpdate packet to update our current camera 
            /// position and rotation
            /// </summary>
            public void SendUpdate()
            {
                SendUpdate(false, Client.Network.CurrentSim);
            }

            /// <summary>
            /// Send new AgentUpdate packet to update our current camera 
            /// position and rotation
            /// </summary>
            /// <param name="reliable">Whether to require server acknowledgement
            /// of this packet</param>
            public void SendUpdate(bool reliable)
            {
                SendUpdate(reliable, Client.Network.CurrentSim);
            }

            /// <summary>
            /// Send new AgentUpdate packet to update our current camera 
            /// position and rotation
            /// </summary>
            /// <param name="reliable">Whether to require server acknowledgement
            /// of this packet</param>
            /// <param name="simulator">Simulator to send the update to</param>
            public void SendUpdate(bool reliable, Simulator simulator)
            {
                // Attempted to sort these in a rough order of how often they might change
                if (agentControls == 0 &&
                    Camera.CameraAtAxis == Camera.LastCameraAtAxis &&
                    Camera.CameraCenter == Camera.LastCameraCenter &&
                    State != lastState &&
                    Camera.HeadRotation == Camera.LastHeadRotation &&
                    Camera.BodyRotation == Camera.LastBodyRotation &&
                    Camera.CameraLeftAxis == Camera.LastCameraLeftAxis &&
                    Camera.Far == Camera.LastFar &&
                    Camera.CameraUpAxis == Camera.LastCameraUpAxis)
                {
                    ++duplicateCount;
                }
                else
                {
                    duplicateCount = 0;
                }

                if (duplicateCount < 2)
                {
                    // Store the current state to do duplicate checking in the future
                    Camera.LastHeadRotation = Camera.HeadRotation;
                    Camera.LastBodyRotation = Camera.BodyRotation;
                    Camera.LastCameraAtAxis = Camera.CameraAtAxis;
                    Camera.LastCameraCenter = Camera.CameraCenter;
                    Camera.LastCameraLeftAxis = Camera.CameraLeftAxis;
                    Camera.LastCameraUpAxis = Camera.CameraUpAxis;
                    Camera.LastFar = Camera.Far;
                    lastState = State;

                    // Build the AgentUpdate packet and send it
                    AgentUpdatePacket update = new AgentUpdatePacket();
                    update.Header.Reliable = reliable;

                    update.AgentData.AgentID = Client.Network.AgentID;
                    update.AgentData.SessionID = Client.Network.SessionID;
                    update.AgentData.HeadRotation = Camera.HeadRotation;
                    update.AgentData.BodyRotation = Camera.BodyRotation;
                    update.AgentData.CameraAtAxis = Camera.CameraAtAxis;
                    update.AgentData.CameraCenter = Camera.CameraCenter;
                    update.AgentData.CameraLeftAxis = Camera.CameraLeftAxis;
                    update.AgentData.CameraUpAxis = Camera.CameraUpAxis;
                    update.AgentData.Far = Camera.Far;
                    update.AgentData.State = (byte)State;
                    update.AgentData.ControlFlags = agentControls;
                    update.AgentData.Flags = (byte)Flags;

                    Client.Network.SendPacket(update, simulator);
                }
            }

            private bool GetControlFlag(MainAvatar.ControlFlags flag)
            {
                return (agentControls & (uint)flag) != 0;
            }

            private void SetControlFlag(MainAvatar.ControlFlags flag, bool value)
            {
                if (value) agentControls |= (uint)flag;
                else agentControls &= ~((uint)flag);
            }

            private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
            {
                if (Client.Network.Connected)
                {
                    //Send an AgentUpdate packet
                    SendUpdate();
                }
            }
        }
    }
}
