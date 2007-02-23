/* 
	* Copyright (c) 2006, Second Life Reverse Engineering Team 
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
            /// <summary>
            /// Contains properties for client control key states
            /// </summary>
            public struct ControlStatus
            {
                /// <summary></summary>
                public bool AtPos
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_AT_POS); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_AT_POS, value); }
                }
                /// <summary></summary>
                public bool AtNeg
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_AT_NEG); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_AT_NEG, value); }
                }
                /// <summary></summary>
                public bool LeftPos
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_LEFT_POS); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_LEFT_POS, value); }
                }
                /// <summary></summary>
                public bool LeftNeg
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_LEFT_NEG); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_LEFT_NEG, value); }
                }
                /// <summary></summary>
                public bool UpPos
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_UP_POS); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_UP_POS, value); }
                }
                /// <summary></summary>
                public bool UpNeg
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_UP_NEG); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_UP_NEG, value); }
                }
                /// <summary></summary>
                public bool PitchPos
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_PITCH_POS); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_PITCH_POS, value); }
                }
                /// <summary></summary>
                public bool PitchNeg
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_PITCH_NEG); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_PITCH_NEG, value); }
                }
                /// <summary></summary>
                public bool YawPos
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_YAW_POS); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_YAW_POS, value); }
                }
                /// <summary></summary>
                public bool YawNeg
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_YAW_NEG); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_YAW_NEG, value); }
                }
                /// <summary></summary>
                public bool FastAt
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_FAST_AT); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_FAST_AT, value); }
                }
                /// <summary></summary>
                public bool FastLeft
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_FAST_LEFT); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_FAST_LEFT, value); }
                }
                /// <summary></summary>
                public bool FastUp
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_FAST_UP); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_FAST_UP, value); }
                }
                /// <summary></summary>
                public bool Fly
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_FLY); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_FLY, value); }
                }
                /// <summary></summary>
                public bool Stop
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_STOP); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_STOP, value); }
                }
                /// <summary></summary>
                public bool FinishAnim
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_FINISH_ANIM); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_FINISH_ANIM, value); }
                }
                /// <summary></summary>
                public bool StandUp
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_STAND_UP); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_STAND_UP, value); }
                }
                /// <summary></summary>
                public bool SitOnGround
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_SIT_ON_GROUND); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_SIT_ON_GROUND, value); }
                }
                /// <summary></summary>
                public bool Mouselook
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_MOUSELOOK); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_MOUSELOOK, value); }
                }
                /// <summary></summary>
                public bool NudgeAtPos
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_AT_POS); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_AT_POS, value); }
                }
                /// <summary></summary>
                public bool NudgeAtNeg
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_AT_NEG); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_AT_NEG, value); }
                }
                /// <summary></summary>
                public bool NudgeLeftPos
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_LEFT_POS); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_LEFT_POS, value); }
                }
                /// <summary></summary>
                public bool NudgeLeftNeg
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_LEFT_NEG); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_LEFT_NEG, value); }
                }
                /// <summary></summary>
                public bool NudgeUpPos
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_UP_POS); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_UP_POS, value); }
                }
                /// <summary></summary>
                public bool NudgeUpNeg
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_UP_NEG); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_UP_NEG, value); }
                }
                /// <summary></summary>
                public bool TurnLeft
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_TURN_LEFT); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_TURN_LEFT, value); }
                }
                /// <summary></summary>
                public bool TurnRight
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_TURN_RIGHT); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_TURN_RIGHT, value); }
                }
                /// <summary></summary>
                public bool Away
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_AWAY); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_AWAY, value); }
                }
                /// <summary></summary>
                public bool LButtonDown
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_LBUTTON_DOWN); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_LBUTTON_DOWN, value); }
                }
                /// <summary></summary>
                public bool LButtonUp
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_LBUTTON_UP); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_LBUTTON_UP, value); }
                }
                /// <summary></summary>
                public bool MLButtonDown
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_ML_LBUTTON_DOWN); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_ML_LBUTTON_DOWN, value); }
                }
                /// <summary></summary>
                public bool MLButtonUp
                {
                    get { return GetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_ML_LBUTTON_UP); }
                    set { SetControlFlag(MainAvatar.AgentUpdateFlags.AGENT_CONTROL_ML_LBUTTON_UP, value); }
                }
            }

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
            }


            /// <summary>
            /// Timer for sending AgentUpdate packets, disabled by default
            /// </summary>
            public Timer UpdateTimer;
            /// <summary>
            /// Holds control flags
            /// </summary>
            public ControlStatus Controls;
            /// <summary>
            /// Holds camera flags
            /// </summary>
            public CameraStatus Camera;

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
            public uint AgentControls { get { return agentControls; } }


            private bool alwaysRun = false;
            private SecondLife Client;
            private static uint agentControls;


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
                Camera.Far = 128.0f;

                UpdateTimer = new Timer(Client.Settings.AGENT_UPDATE_INTERVAL);
                UpdateTimer.Elapsed += new ElapsedEventHandler(UpdateTimer_Elapsed);
                UpdateTimer.Enabled = Client.Settings.SEND_AGENTUPDATES;
                if (Client.Settings.SEND_AGENTUPDATES)
                    UpdateTimer.Start();
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
                update.AgentData.ControlFlags = agentControls;
                update.AgentData.Far = Camera.Far;

                Client.Network.SendPacket(update, simulator);
            }

            private static bool GetControlFlag(MainAvatar.AgentUpdateFlags flag)
            {
                uint control = (uint)flag;
                return ((agentControls & control) == control);
            }

            private static void SetControlFlag(MainAvatar.AgentUpdateFlags flag, bool value)
            {
                uint control = (uint)flag;
                if (value && ((agentControls & control) != control)) agentControls ^= control;
                else if (!value && ((agentControls & control) == control)) agentControls ^= control;
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
