/*
 * Copyright (c) 2006-2008, Second Life Reverse Engineering Team
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
using System.Threading;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife
{
    public partial class AgentManager
    {
        #region Enums

        /// <summary>
        /// Used to specify movement actions for your agent
        /// </summary>
        [Flags]
        public enum ControlFlags
        {
            /// <summary>Empty flag</summary>
            NONE = 0,
            /// <summary>Move Forward (SL Keybinding: W/Up Arrow)</summary>
            AGENT_CONTROL_AT_POS = 0x1 << CONTROL_AT_POS_INDEX,
            /// <summary>Move Backward (SL Keybinding: S/Down Arrow)</summary>
            AGENT_CONTROL_AT_NEG = 0x1 << CONTROL_AT_NEG_INDEX,
            /// <summary>Move Left (SL Keybinding: Shift-(A/Left Arrow))</summary>
            AGENT_CONTROL_LEFT_POS = 0x1 << CONTROL_LEFT_POS_INDEX,
            /// <summary>Move Right (SL Keybinding: Shift-(D/Right Arrow))</summary>
            AGENT_CONTROL_LEFT_NEG = 0x1 << CONTROL_LEFT_NEG_INDEX,
            /// <summary>Not Flying: Jump/Flying: Move Up (SL Keybinding: E)</summary>
            AGENT_CONTROL_UP_POS = 0x1 << CONTROL_UP_POS_INDEX,
            /// <summary>Not Flying: Croutch/Flying: Move Down (SL Keybinding: C)</summary>
            AGENT_CONTROL_UP_NEG = 0x1 << CONTROL_UP_NEG_INDEX,
            /// <summary>Unused</summary>
            AGENT_CONTROL_PITCH_POS = 0x1 << CONTROL_PITCH_POS_INDEX,
            /// <summary>Unused</summary>
            AGENT_CONTROL_PITCH_NEG = 0x1 << CONTROL_PITCH_NEG_INDEX,
            /// <summary>Unused</summary>
            AGENT_CONTROL_YAW_POS = 0x1 << CONTROL_YAW_POS_INDEX,
            /// <summary>Unused</summary>
            AGENT_CONTROL_YAW_NEG = 0x1 << CONTROL_YAW_NEG_INDEX,
            /// <summary>ORed with AGENT_CONTROL_AT_* if the keyboard is being used</summary>
            AGENT_CONTROL_FAST_AT = 0x1 << CONTROL_FAST_AT_INDEX,
            /// <summary>ORed with AGENT_CONTROL_LEFT_* if the keyboard is being used</summary>
            AGENT_CONTROL_FAST_LEFT = 0x1 << CONTROL_FAST_LEFT_INDEX,
            /// <summary>ORed with AGENT_CONTROL_UP_* if the keyboard is being used</summary>
            AGENT_CONTROL_FAST_UP = 0x1 << CONTROL_FAST_UP_INDEX,
            /// <summary>Fly</summary>
            AGENT_CONTROL_FLY = 0x1 << CONTROL_FLY_INDEX,
            /// <summary></summary>
            AGENT_CONTROL_STOP = 0x1 << CONTROL_STOP_INDEX,
            /// <summary>Finish our current animation</summary>
            AGENT_CONTROL_FINISH_ANIM = 0x1 << CONTROL_FINISH_ANIM_INDEX,
            /// <summary>Stand up from the ground or a prim seat</summary>
            AGENT_CONTROL_STAND_UP = 0x1 << CONTROL_STAND_UP_INDEX,
            /// <summary>Sit on the ground at our current location</summary>
            AGENT_CONTROL_SIT_ON_GROUND = 0x1 << CONTROL_SIT_ON_GROUND_INDEX,
            /// <summary>Whether mouselook is currently enabled</summary>
            AGENT_CONTROL_MOUSELOOK = 0x1 << CONTROL_MOUSELOOK_INDEX,
            /// <summary>Legacy, used if a key was pressed for less than a certain amount of time</summary>
            AGENT_CONTROL_NUDGE_AT_POS = 0x1 << CONTROL_NUDGE_AT_POS_INDEX,
            /// <summary>Legacy, used if a key was pressed for less than a certain amount of time</summary>
            AGENT_CONTROL_NUDGE_AT_NEG = 0x1 << CONTROL_NUDGE_AT_NEG_INDEX,
            /// <summary>Legacy, used if a key was pressed for less than a certain amount of time</summary>
            AGENT_CONTROL_NUDGE_LEFT_POS = 0x1 << CONTROL_NUDGE_LEFT_POS_INDEX,
            /// <summary>Legacy, used if a key was pressed for less than a certain amount of time</summary>
            AGENT_CONTROL_NUDGE_LEFT_NEG = 0x1 << CONTROL_NUDGE_LEFT_NEG_INDEX,
            /// <summary>Legacy, used if a key was pressed for less than a certain amount of time</summary>
            AGENT_CONTROL_NUDGE_UP_POS = 0x1 << CONTROL_NUDGE_UP_POS_INDEX,
            /// <summary>Legacy, used if a key was pressed for less than a certain amount of time</summary>
            AGENT_CONTROL_NUDGE_UP_NEG = 0x1 << CONTROL_NUDGE_UP_NEG_INDEX,
            /// <summary></summary>
            AGENT_CONTROL_TURN_LEFT = 0x1 << CONTROL_TURN_LEFT_INDEX,
            /// <summary></summary>
            AGENT_CONTROL_TURN_RIGHT = 0x1 << CONTROL_TURN_RIGHT_INDEX,
            /// <summary>Set when the avatar is idled or set to away. Note that the away animation is 
            /// activated separately from setting this flag</summary>
            AGENT_CONTROL_AWAY = 0x1 << CONTROL_AWAY_INDEX,
            /// <summary></summary>
            AGENT_CONTROL_LBUTTON_DOWN = 0x1 << CONTROL_LBUTTON_DOWN_INDEX,
            /// <summary></summary>
            AGENT_CONTROL_LBUTTON_UP = 0x1 << CONTROL_LBUTTON_UP_INDEX,
            /// <summary></summary>
            AGENT_CONTROL_ML_LBUTTON_DOWN = 0x1 << CONTROL_ML_LBUTTON_DOWN_INDEX,
            /// <summary></summary>
            AGENT_CONTROL_ML_LBUTTON_UP = 0x1 << CONTROL_ML_LBUTTON_UP_INDEX
        }

        #endregion Enums

        #region AgentUpdate Constants

        private const int CONTROL_AT_POS_INDEX = 0;
        private const int CONTROL_AT_NEG_INDEX = 1;
        private const int CONTROL_LEFT_POS_INDEX = 2;
        private const int CONTROL_LEFT_NEG_INDEX = 3;
        private const int CONTROL_UP_POS_INDEX = 4;
        private const int CONTROL_UP_NEG_INDEX = 5;
        private const int CONTROL_PITCH_POS_INDEX = 6;
        private const int CONTROL_PITCH_NEG_INDEX = 7;
        private const int CONTROL_YAW_POS_INDEX = 8;
        private const int CONTROL_YAW_NEG_INDEX = 9;
        private const int CONTROL_FAST_AT_INDEX = 10;
        private const int CONTROL_FAST_LEFT_INDEX = 11;
        private const int CONTROL_FAST_UP_INDEX = 12;
        private const int CONTROL_FLY_INDEX = 13;
        private const int CONTROL_STOP_INDEX = 14;
        private const int CONTROL_FINISH_ANIM_INDEX = 15;
        private const int CONTROL_STAND_UP_INDEX = 16;
        private const int CONTROL_SIT_ON_GROUND_INDEX = 17;
        private const int CONTROL_MOUSELOOK_INDEX = 18;
        private const int CONTROL_NUDGE_AT_POS_INDEX = 19;
        private const int CONTROL_NUDGE_AT_NEG_INDEX = 20;
        private const int CONTROL_NUDGE_LEFT_POS_INDEX = 21;
        private const int CONTROL_NUDGE_LEFT_NEG_INDEX = 22;
        private const int CONTROL_NUDGE_UP_POS_INDEX = 23;
        private const int CONTROL_NUDGE_UP_NEG_INDEX = 24;
        private const int CONTROL_TURN_LEFT_INDEX = 25;
        private const int CONTROL_TURN_RIGHT_INDEX = 26;
        private const int CONTROL_AWAY_INDEX = 27;
        private const int CONTROL_LBUTTON_DOWN_INDEX = 28;
        private const int CONTROL_LBUTTON_UP_INDEX = 29;
        private const int CONTROL_ML_LBUTTON_DOWN_INDEX = 30;
        private const int CONTROL_ML_LBUTTON_UP_INDEX = 31;
        private const int TOTAL_CONTROLS = 32;

        #endregion AgentUpdate Constants

        /// <summary> 
        /// Agent movement and camera control
        /// 
        /// Agent movement is controlled by setting specific <seealso cref="T:AgentManager.ControlFlags"/>
        /// After the control flags are set, An AgentUpdate is required to update the simulator of the specified flags
        /// This is most easily accomplished by setting one or more of the AgentMovement properties
        /// 
        /// Movement of an avatar is always based on a compass direction, for example AtPos will move the 
        /// agent from West to East or forward on the X Axis, AtNeg will of course move agent from 
        /// East to West or backward on the X Axis, LeftPos will be South to North or forward on the Y Axis
        /// The Z axis is Up, finer grained control of movements can be done using the Nudge properties
        /// </summary> 
        public partial class AgentMovement
        {
            #region Properties

            /// <summary>Move agent positive along the X axis</summary>
            public bool AtPos
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_AT_POS); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_AT_POS, value); }
            }
            /// <summary>Move agent negative along the X axis</summary>
            public bool AtNeg
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_AT_NEG); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_AT_NEG, value); }
            }
            /// <summary>Move agent positive along the Y axis</summary>
            public bool LeftPos
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_LEFT_POS); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_LEFT_POS, value); }
            }
            /// <summary>Move agent negative along the Y axis</summary>
            public bool LeftNeg
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_LEFT_NEG); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_LEFT_NEG, value); }
            }
            /// <summary>Move agent positive along the Z axis</summary>
            public bool UpPos
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_UP_POS); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_UP_POS, value); }
            }
            /// <summary>Move agent negative along the Z axis</summary>
            public bool UpNeg
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_UP_NEG); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_UP_NEG, value); }
            }
            /// <summary></summary>
            public bool PitchPos
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_PITCH_POS); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_PITCH_POS, value); }
            }
            /// <summary></summary>
            public bool PitchNeg
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_PITCH_NEG); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_PITCH_NEG, value); }
            }
            /// <summary></summary>
            public bool YawPos
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_YAW_POS); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_YAW_POS, value); }
            }
            /// <summary></summary>
            public bool YawNeg
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_YAW_NEG); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_YAW_NEG, value); }
            }
            /// <summary></summary>
            public bool FastAt
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_FAST_AT); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_FAST_AT, value); }
            }
            /// <summary></summary>
            public bool FastLeft
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_FAST_LEFT); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_FAST_LEFT, value); }
            }
            /// <summary></summary>
            public bool FastUp
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_FAST_UP); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_FAST_UP, value); }
            }
            /// <summary>Causes simulator to make agent fly</summary>
            public bool Fly
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_FLY); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_FLY, value); }
            }
            /// <summary>Stop movement</summary>
            public bool Stop
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_STOP); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_STOP, value); }
            }
            /// <summary>Finish animation</summary>
            public bool FinishAnim
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_FINISH_ANIM); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_FINISH_ANIM, value); }
            }
            /// <summary>Stand up from a sit</summary>
            public bool StandUp
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_STAND_UP); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_STAND_UP, value); }
            }
            /// <summary>Tells simulator to sit agent on ground</summary>
            public bool SitOnGround
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_SIT_ON_GROUND); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_SIT_ON_GROUND, value); }
            }
            /// <summary>Place agent into mouselook mode</summary>
            public bool Mouselook
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_MOUSELOOK); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_MOUSELOOK, value); }
            }
            /// <summary>Nudge agent positive along the X axis</summary>
            public bool NudgeAtPos
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_NUDGE_AT_POS); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_NUDGE_AT_POS, value); }
            }
            /// <summary>Nudge agent negative along the X axis</summary>
            public bool NudgeAtNeg
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_NUDGE_AT_NEG); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_NUDGE_AT_NEG, value); }
            }
            /// <summary>Nudge agent positive along the Y axis</summary>
            public bool NudgeLeftPos
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_NUDGE_LEFT_POS); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_NUDGE_LEFT_POS, value); }
            }
            /// <summary>Nudge agent negative along the Y axis</summary>
            public bool NudgeLeftNeg
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_NUDGE_LEFT_NEG); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_NUDGE_LEFT_NEG, value); }
            }
            /// <summary>Nudge agent positive along the Z axis</summary>
            public bool NudgeUpPos
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_NUDGE_UP_POS); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_NUDGE_UP_POS, value); }
            }
            /// <summary>Nudge agent negative along the Z axis</summary>
            public bool NudgeUpNeg
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_NUDGE_UP_NEG); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_NUDGE_UP_NEG, value); }
            }
            /// <summary></summary>
            public bool TurnLeft
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_TURN_LEFT); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_TURN_LEFT, value); }
            }
            /// <summary></summary>
            public bool TurnRight
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_TURN_RIGHT); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_TURN_RIGHT, value); }
            }
            /// <summary>Tell simulator to mark agent as away</summary>
            public bool Away
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_AWAY); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_AWAY, value); }
            }
            /// <summary></summary>
            public bool LButtonDown
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_LBUTTON_DOWN); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_LBUTTON_DOWN, value); }
            }
            /// <summary></summary>
            public bool LButtonUp
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_LBUTTON_UP); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_LBUTTON_UP, value); }
            }
            /// <summary></summary>
            public bool MLButtonDown
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_ML_LBUTTON_DOWN); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_ML_LBUTTON_DOWN, value); }
            }
            /// <summary></summary>
            public bool MLButtonUp
            {
                get { return GetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_ML_LBUTTON_UP); }
                set { SetControlFlag(AgentManager.ControlFlags.AGENT_CONTROL_ML_LBUTTON_UP, value); }
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
                    run.AgentData.AgentID = Client.Self.AgentID;
                    run.AgentData.SessionID = Client.Self.SessionID;
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
            /// <summary>Gets or sets the interval in milliseconds at which
            /// AgentUpdate packets are sent to the current simulator. Setting
            /// this to a non-zero value will also enable the packet sending if
            /// it was previously off, and setting it to zero will disable</summary>
            public int UpdateInterval
            {
                get
                {
                    return updateInterval;
                }
                set
                {
                    if (value > 0)
                    {
                        updateTimer.Change(value, value);
                        updateInterval = value;
                    }
                    else
                    {
                        updateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        updateInterval = 0;
                    }
                }
            }
            /// <summary>Gets or sets whether AgentUpdate packets are sent to
            /// the current simulator</summary>
            public bool UpdateEnabled
            {
                get { return (updateInterval != 0); }
            }

            /// <summary>Reset movement controls every time we send an update</summary>
            public bool AutoResetControls
            {
                get { return autoResetControls; }
                set { autoResetControls = value; }
            }

            #endregion Properties

            /// <summary>Agent camera controls</summary>
            public AgentCamera Camera;
            /// <summary>Currently only used for hiding your group title</summary>
            public AgentFlags Flags = AgentFlags.None;
            /// <summary>Action state of the avatar, which can currently be
            /// typing and editing</summary>
            public AgentState State = AgentState.None;
            /// <summary></summary>
            public LLQuaternion BodyRotation = LLQuaternion.Identity;
            /// <summary></summary>
            public LLQuaternion HeadRotation = LLQuaternion.Identity;

            #region Change tracking
            /// <summary></summary>
            private LLQuaternion LastBodyRotation;
            /// <summary></summary>
            private LLQuaternion LastHeadRotation;
            /// <summary></summary>
            private LLVector3 LastCameraCenter;
            /// <summary></summary>
            private LLVector3 LastCameraXAxis;
            /// <summary></summary>
            private LLVector3 LastCameraYAxis;
            /// <summary></summary>
            private LLVector3 LastCameraZAxis;
            /// <summary></summary>
            private float LastFar;
            #endregion Change tracking

            private bool alwaysRun;
            private SecondLife Client;
            private uint agentControls;
            private int duplicateCount;
            private AgentState lastState;
            /// <summary>Timer for sending AgentUpdate packets</summary>
            private Timer updateTimer;
            private int updateInterval;
            private bool autoResetControls = true;

            /// <summary>Default constructor</summary>
            public AgentMovement(SecondLife client)
            {
                Client = client;
                Camera = new AgentCamera();

                updateInterval = Settings.DEFAULT_AGENT_UPDATE_INTERVAL;
                updateTimer = new Timer(new TimerCallback(UpdateTimer_Elapsed), null, Settings.DEFAULT_AGENT_UPDATE_INTERVAL,
                    Settings.DEFAULT_AGENT_UPDATE_INTERVAL);
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
                Camera.Position = Client.Self.SimPosition;
                Camera.LookDirection(heading);
                
                BodyRotation.Z = (float)Math.Sin(heading / 2.0d);
                BodyRotation.W = (float)Math.Cos(heading / 2.0d);
                HeadRotation = BodyRotation;

                SendUpdate(reliable);
            }

            /// <summary>
            /// Rotates the avatar body and camera toward a target position.
            /// This will also anchor the camera position on the avatar
            /// </summary>
            /// <param name="target">Region coordinates to turn toward</param>
            public bool TurnToward(LLVector3 target)
            {
                if (Client.Settings.SEND_AGENT_UPDATES)
                {
                    LLVector3 myPos = Client.Self.SimPosition;
                    LLVector3 forward = new LLVector3(1, 0, 0);
                    LLVector3 offset = LLVector3.Norm(target - myPos);
                    LLQuaternion newRot = LLVector3.RotBetween(forward, offset);

                    BodyRotation = newRot;
                    HeadRotation = newRot;
                    Camera.LookAt(myPos, target);

                    SendUpdate();

                    return true;
                }
                else
                {
                    Logger.Log("Attempted TurnToward but agent updates are disabled", Helpers.LogLevel.Warning, Client);
                    return false;
                }
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
                LLVector3 origin = Camera.Position;
                LLVector3 xAxis = Camera.LeftAxis;
                LLVector3 yAxis = Camera.AtAxis;
                LLVector3 zAxis = Camera.UpAxis;

                // Attempted to sort these in a rough order of how often they might change
                if (agentControls == 0 &&
                    yAxis == LastCameraYAxis &&
                    origin == LastCameraCenter &&
                    State == lastState &&
                    HeadRotation == LastHeadRotation &&
                    BodyRotation == LastBodyRotation &&
                    xAxis == LastCameraXAxis &&
                    Camera.Far == LastFar &&
                    zAxis == LastCameraZAxis)
                {
                    ++duplicateCount;
                }
                else
                {
                    duplicateCount = 0;
                }

                if (Client.Settings.DISABLE_AGENT_UPDATE_DUPLICATE_CHECK || duplicateCount < 10)
                {
                    // Store the current state to do duplicate checking
                    LastHeadRotation = HeadRotation;
                    LastBodyRotation = BodyRotation;
                    LastCameraYAxis = yAxis;
                    LastCameraCenter = origin;
                    LastCameraXAxis = xAxis;
                    LastCameraZAxis = zAxis;
                    LastFar = Camera.Far;
                    lastState = State;

                    // Build the AgentUpdate packet and send it
                    AgentUpdatePacket update = new AgentUpdatePacket();
                    update.Header.Reliable = reliable;

                    update.AgentData.AgentID = Client.Self.AgentID;
                    update.AgentData.SessionID = Client.Self.SessionID;
                    update.AgentData.HeadRotation = HeadRotation;
                    update.AgentData.BodyRotation = BodyRotation;
                    update.AgentData.CameraAtAxis = yAxis;
                    update.AgentData.CameraCenter = origin;
                    update.AgentData.CameraLeftAxis = xAxis;
                    update.AgentData.CameraUpAxis = zAxis;
                    update.AgentData.Far = Camera.Far;
                    update.AgentData.State = (byte)State;
                    update.AgentData.ControlFlags = agentControls;
                    update.AgentData.Flags = (byte)Flags;

                    Client.Network.SendPacket(update, simulator);

                    if (autoResetControls) {
                        ResetControlFlags();
                    }
                }
            }

            /// <summary>
            /// Builds an AgentUpdate packet entirely from parameters. This
            /// will not touch the state of Self.Movement or
            /// Self.Movement.Camera in any way
            /// </summary>
            /// <param name="controlFlags"></param>
            /// <param name="position"></param>
            /// <param name="forwardAxis"></param>
            /// <param name="leftAxis"></param>
            /// <param name="upAxis"></param>
            /// <param name="bodyRotation"></param>
            /// <param name="headRotation"></param>
            /// <param name="farClip"></param>
            /// <param name="reliable"></param>
            /// <param name="flags"></param>
            /// <param name="state"></param>
            public void SendManualUpdate(AgentManager.ControlFlags controlFlags, LLVector3 position, LLVector3 forwardAxis,
                LLVector3 leftAxis, LLVector3 upAxis, LLQuaternion bodyRotation, LLQuaternion headRotation, float farClip,
                AgentFlags flags, AgentState state, bool reliable)
            {
                AgentUpdatePacket update = new AgentUpdatePacket();

                update.AgentData.AgentID = Client.Self.AgentID;
                update.AgentData.SessionID = Client.Self.SessionID;
                update.AgentData.BodyRotation = bodyRotation;
                update.AgentData.HeadRotation = headRotation;
                update.AgentData.CameraCenter = position;
                update.AgentData.CameraAtAxis = forwardAxis;
                update.AgentData.CameraLeftAxis = leftAxis;
                update.AgentData.CameraUpAxis = upAxis;
                update.AgentData.Far = farClip;
                update.AgentData.ControlFlags = (uint)controlFlags;
                update.AgentData.Flags = (byte)flags;
                update.AgentData.State = (byte)state;

                update.Header.Reliable = reliable;

                Client.Network.SendPacket(update);
            }

            private bool GetControlFlag(ControlFlags flag)
            {
                return (agentControls & (uint)flag) != 0;
            }

            private void SetControlFlag(ControlFlags flag, bool value)
            {
                if (value) agentControls |= (uint)flag;
                else agentControls &= ~((uint)flag);
            }

            private void ResetControlFlags()
            {
                // Reset all of the flags except for persistent settings like
                // away, fly, mouselook, and crouching
                agentControls &=
                    (uint)(ControlFlags.AGENT_CONTROL_AWAY |
                    ControlFlags.AGENT_CONTROL_FLY |
                    ControlFlags.AGENT_CONTROL_MOUSELOOK |
                    ControlFlags.AGENT_CONTROL_UP_NEG);
            }

            private void UpdateTimer_Elapsed(object obj)
            {
                if (Client.Network.Connected && Client.Settings.SEND_AGENT_UPDATES)
                {
                    //Send an AgentUpdate packet
                    SendUpdate(false, Client.Network.CurrentSim);
                }
            }
        }
    }
}
