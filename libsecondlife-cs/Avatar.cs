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
using System.Net;
using System.Collections.Generic;
using libsecondlife.Packets;

namespace libsecondlife
{
    /// <summary>
    /// Basic class to hold other Avatar's data.
    /// </summary>
    public class Avatar
    {
        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum AgentUpdateFlags
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
            /// <summary></summary>
            AGENT_CONTROL_FLY = 0x1 << CONTROL_FLY_INDEX,
            /// <summary></summary>
            AGENT_CONTROL_STOP = 0x1 << CONTROL_STOP_INDEX,
            /// <summary></summary>
            AGENT_CONTROL_FINISH_ANIM = 0x1 << CONTROL_FINISH_ANIM_INDEX,
            /// <summary></summary>
            AGENT_CONTROL_STAND_UP = 0x1 << CONTROL_STAND_UP_INDEX,
            /// <summary></summary>
            AGENT_CONTROL_SIT_ON_GROUND = 0x1 << CONTROL_SIT_ON_GROUND_INDEX,
            /// <summary></summary>
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

        /// <summary>The Avatar's UUID, asset server</summary>
        public LLUUID ID;
        /// <summary>Avatar ID in Region (sim) it is in</summary>
        public uint LocalID;
        /// <summary>Full Name of Avatar</summary>
        public string Name;
        /// <summary>Active Group of Avatar</summary>
        public string GroupName;
        /// <summary>Online Status of Avatar</summary>
        public bool Online;
        /// <summary>Location of Avatar (x,y,z probably)</summary>
        public LLVector3 Position;
        /// <summary>Rotational Position of Avatar</summary>
        public LLQuaternion Rotation;
        /// <summary>Region (aka sim) the Avatar is in</summary>
        public Region CurrentRegion;
        /// <summary></summary>
        public string BornOn;
        /// <summary></summary>
        public LLUUID ProfileImage;
        /// <summary></summary>
        public LLUUID PartnerID;
        /// <summary></summary>
        public string AboutText;
        /// <summary></summary>
        public uint WantToMask;
        /// <summary></summary>
        public string WantToText;
        /// <summary></summary>
        public uint SkillsMask;
        /// <summary></summary>
        public string SkillsText;
        /// <summary></summary>
        public string FirstLifeText;
        /// <summary></summary>
        public LLUUID FirstLifeImage;
        /// <summary></summary>
        public bool Identified;
        /// <summary></summary>
        public bool Transacted;
        /// <summary></summary>
        public bool AllowPublish;
        /// <summary></summary>
        public bool MaturePublish;
        /// <summary></summary>
        public string CharterMember;
        /// <summary></summary>
        public float Behavior;
        /// <summary></summary>
        public float Appearance;
        /// <summary></summary>
        public float Building;
        /// <summary></summary>
        public string LanguagesText;
        /// <summary></summary>
        public TextureEntry Textures;
        /// <summary></summary>
        public string ProfileURL;

        protected const int CONTROL_AT_POS_INDEX = 0;
        protected const int CONTROL_AT_NEG_INDEX = 1;
        protected const int CONTROL_LEFT_POS_INDEX = 2;
        protected const int CONTROL_LEFT_NEG_INDEX = 3;
        protected const int CONTROL_UP_POS_INDEX = 4;
        protected const int CONTROL_UP_NEG_INDEX = 5;
        protected const int CONTROL_PITCH_POS_INDEX = 6;
        protected const int CONTROL_PITCH_NEG_INDEX = 7;
        protected const int CONTROL_YAW_POS_INDEX = 8;
        protected const int CONTROL_YAW_NEG_INDEX = 9;
        protected const int CONTROL_FAST_AT_INDEX = 10;
        protected const int CONTROL_FAST_LEFT_INDEX = 11;
        protected const int CONTROL_FAST_UP_INDEX = 12;
        protected const int CONTROL_FLY_INDEX = 13;
        protected const int CONTROL_STOP_INDEX = 14;
        protected const int CONTROL_FINISH_ANIM_INDEX = 15;
        protected const int CONTROL_STAND_UP_INDEX = 16;
        protected const int CONTROL_SIT_ON_GROUND_INDEX = 17;
        protected const int CONTROL_MOUSELOOK_INDEX = 18;
        protected const int CONTROL_NUDGE_AT_POS_INDEX = 19;
        protected const int CONTROL_NUDGE_AT_NEG_INDEX = 20;
        protected const int CONTROL_NUDGE_LEFT_POS_INDEX = 21;
        protected const int CONTROL_NUDGE_LEFT_NEG_INDEX = 22;
        protected const int CONTROL_NUDGE_UP_POS_INDEX = 23;
        protected const int CONTROL_NUDGE_UP_NEG_INDEX = 24;
        protected const int CONTROL_TURN_LEFT_INDEX = 25;
        protected const int CONTROL_TURN_RIGHT_INDEX = 26;
        protected const int CONTROL_AWAY_INDEX = 27;
        protected const int CONTROL_LBUTTON_DOWN_INDEX = 28;
        protected const int CONTROL_LBUTTON_UP_INDEX = 29;
        protected const int CONTROL_ML_LBUTTON_DOWN_INDEX = 30;
        protected const int CONTROL_ML_LBUTTON_UP_INDEX = 31;
        protected const int TOTAL_CONTROLS = 32;
    }

}
