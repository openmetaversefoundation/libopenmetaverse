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

        /// <summary>
        /// Positive and negative ratings
        /// </summary>
        public struct Statistics
        {
            /// <summary>Positive ratings for Behavior</summary>
            public int BehaviorPositive;
            /// <summary>Negative ratings for Behavior</summary>
            public int BehaviorNegative;
            /// <summary>Positive ratings for Appearance</summary>
            public int AppearancePositive;
            /// <summary>Negative ratings for Appearance</summary>
            public int AppearanceNegative;
            /// <summary>Positive ratings for Building</summary>
            public int BuildingPositive;
            /// <summary>Negative ratings for Building</summary>
            public int BuildingNegative;
        }

        /// <summary>
        /// Avatar properties including about text, profile URL, image IDs and 
        /// publishing settings
        /// </summary>
        public struct Properties
        {
            /// <summary>Should this profile be published on the web</summary>
            public bool AllowPublish;
            /// <summary>First Life about text</summary>
            public string FirstLifeText;
            /// <summary>First Life image ID</summary>
            public LLUUID FirstLifeImage;
            /// <summary></summary>
            public LLUUID Partner;
            /// <summary></summary>
            public string AboutText;
            /// <summary></summary>
            public string BornOn;
            /// <summary></summary>
            public string CharterMember;
            /// <summary>Profile image ID</summary>
            public LLUUID ProfileImage;
            /// <summary>Is this a mature profile</summary>
            public bool MaturePublish;
            /// <summary></summary>
            public bool Identified;
            /// <summary></summary>
            public bool Transacted;
            /// <summary>Web URL for this profile</summary>
            public string ProfileURL;
        }

        /// <summary>
        /// Avatar interests including spoken languages, skills, and "want to"
        /// choices
        /// </summary>
        public struct Interests
        {
            /// <summary>Languages profile field</summary>
            public string LanguagesText;
            /// <summary></summary>
            public uint SkillsMask;
            /// <summary></summary>
            public string SkillsText;
            /// <summary></summary>
            public uint WantToMask;
            /// <summary></summary>
            public string WantToText;
        }


        /// <summary>UUID for this avatar</summary>
        public LLUUID ID = LLUUID.Zero;
        /// <summary>Temporary ID for this avatar, local to the current region</summary>
        public uint LocalID = 0;
        /// <summary>Full name</summary>
        public string Name = String.Empty;
        /// <summary>Active group</summary>
        public string GroupName = String.Empty;
        /// <summary>Online status</summary>
        public bool Online = false;
        /// <summary>Positive and negative ratings</summary>
        public Statistics ProfileStatistics = new Statistics();
        /// <summary>Avatar properties including about text, profile URL, image IDs and 
        /// publishing settings</summary>
        public Properties ProfileProperties = new Properties();
        /// <summary>Avatar interests including spoken languages, skills, and "want to"
        /// choices</summary>
        public Interests ProfileInterests = new Interests();
        /// <summary>Local location, relative to the sim or what the avatar is
        /// sitting on</summary>
        public LLVector3 Position = LLVector3.Zero;
        /// <summary>Rotational position, relative to the sim or what the avatar
        /// is sitting on</summary>
        public LLQuaternion Rotation = LLQuaternion.Identity;
        /// <summary>Region the avatar is in</summary>
        public Region CurrentRegion = null;
        /// <summary>Textures for this avatars clothing</summary>
        public TextureEntry Textures = new TextureEntry();

        /// <summary>Gets the local ID of the prim the avatar is sitting on,
        /// zero if the avatar is not currently sitting</summary>
        public uint SittingOn
        {
            get { return sittingOn; }
        }

        internal uint sittingOn = 0;

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

        public Avatar()
        {
        }
    }
}
