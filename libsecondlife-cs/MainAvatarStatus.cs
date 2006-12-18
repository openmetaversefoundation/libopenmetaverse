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

            private SecondLife Client;
            private static uint agentControls;
            /// <summary>
            /// Timer for sending AgentUpdate packets, disabled by default
            /// </summary>
            public Timer UpdateTimer;
            private static bool getControlFlag(Avatar.AgentUpdateFlags flag)
            {
                uint control = (uint)flag;
                return ((agentControls & control) == control);
            }

            private static void setControlFlag(Avatar.AgentUpdateFlags flag, bool value)
            {
                uint control = (uint)flag;
                if (value && ((agentControls & control) != control)) agentControls ^= control;
                else if (!value && ((agentControls & control) == control)) agentControls ^= control;
            }
            /// <summary>
            /// Holds control flags
            /// </summary>
            public ControlStatus Controls;
            /// <summary>
            /// Holds camera flags
            /// </summary>
            public CameraStatus Camera;

            /// <summary>Constructor for class MainAvatarStatus</summary>
            public MainAvatarStatus(SecondLife client)
            {
                Client = client;
                UpdateTimer = new Timer();
                UpdateTimer.Elapsed += new ElapsedEventHandler(UpdateTimer_Elapsed);
                UpdateTimer.Interval = 500;
                //Update Timer Disabled By Default --Jesse Malthus
                UpdateTimer.Enabled = false;
            }
            /// <summary>
            /// Event handler for UpdateTimer that sends an AgentUpdate packet
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
            {
                //Send an AgentUpdate packet
                SendUpdate();
            }

            public uint AgentControls
            {
                get { return agentControls; }
            }
            /// <summary>
            /// Send new AgentUpdate
            /// </summary>
            public void SendUpdate()
            {
                AgentUpdatePacket update = new AgentUpdatePacket();
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

                Client.Network.SendPacket(update);
            }

            public struct ControlStatus
            {

                public bool AtPos
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_AT_POS); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_AT_POS,value); }
                }
                public bool AtNeg
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_AT_NEG); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_AT_NEG, value); }
                }
                public bool LeftPos
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_LEFT_POS); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_LEFT_POS, value); }
                }
                public bool LeftNeg
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_LEFT_NEG); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_LEFT_NEG, value); }
                }
                public bool UpPos
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_UP_POS); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_UP_POS, value); }
                }
                public bool UpNeg
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_UP_NEG); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_UP_NEG, value); }
                }
                public bool PitchPos
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_PITCH_POS); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_PITCH_POS, value); }
                }
                public bool PitchNeg
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_PITCH_NEG); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_PITCH_NEG, value); }
                }
                public bool YawPos
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_YAW_POS); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_YAW_POS, value); }
                }
                public bool YawNeg
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_YAW_NEG); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_YAW_NEG, value); }
                }
                public bool FastAt
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_FAST_AT); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_FAST_AT, value); }
                }
                public bool FastLeft
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_FAST_LEFT); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_FAST_LEFT, value); }
                }
                public bool FastUp
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_FAST_UP); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_FAST_UP, value); }
                }
                public bool Fly
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_FLY); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_FLY, value); }
                }
                public bool Stop
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_STOP); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_STOP, value); }
                }
                public bool FinishAnim
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_FINISH_ANIM); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_FINISH_ANIM, value); }
                }
                public bool StandUp
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_STAND_UP); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_STAND_UP, value); }
                }
                public bool SitOnGround
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_SIT_ON_GROUND); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_SIT_ON_GROUND, value); }
                }
                public bool Mouselook
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_MOUSELOOK); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_MOUSELOOK, value); }
                }
                public bool NudgeAtPos
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_AT_POS); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_AT_POS, value); }
                }
                public bool NudgeAtNeg
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_AT_NEG); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_AT_NEG, value); }
                }
                public bool NudgeLeftPos
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_LEFT_POS); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_LEFT_POS, value); }
                }
                public bool NudgeLeftNeg
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_LEFT_NEG); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_LEFT_NEG, value); }
                }
                public bool NudgeUpPos
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_UP_POS); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_UP_POS, value); }
                }
                public bool NudgeUpNeg
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_UP_NEG); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_NUDGE_UP_NEG, value); }
                }
                public bool TurnLeft
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_TURN_LEFT); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_TURN_LEFT, value); }
                }
                public bool TurnRight
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_TURN_RIGHT); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_TURN_RIGHT, value); }
                }
                public bool Away
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_AWAY); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_AWAY, value); }
                }
                public bool LButtonDown
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_LBUTTON_DOWN); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_LBUTTON_DOWN, value); }
                }
                public bool LButtonUp
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_LBUTTON_UP); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_LBUTTON_UP, value); }
                }
                public bool MLButtonDown
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_ML_LBUTTON_DOWN); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_ML_LBUTTON_DOWN, value); }
                }
                public bool MLButtonUp
                {
                    get { return getControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_ML_LBUTTON_UP); }
                    set { setControlFlag(Avatar.AgentUpdateFlags.AGENT_CONTROL_ML_LBUTTON_UP, value); }
                }

            }

            public struct CameraStatus
            {
                public LLQuaternion BodyRotation;
                public LLQuaternion HeadRotation;
                public LLVector3 CameraAtAxis;
                public LLVector3 CameraCenter;
                public LLVector3 CameraLeftAxis;
                public LLVector3 CameraUpAxis;
                public float Far;
            }

        }
    }
}