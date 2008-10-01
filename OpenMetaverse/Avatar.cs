/*
 * Copyright (c) 2006-2008, openmetaverse.org
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
using System.Net;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse.Packets;

namespace OpenMetaverse
{
    #region Enums

    /// <summary>
    /// Avatar profile flags
    /// </summary>
    [Flags]
    public enum ProfileFlags : uint
    {
        AllowPublish = 1,
        MaturePublish = 2,
        Identified = 4,
        Transacted = 8,
        Online = 16
    }

    #endregion Enums

    /// <summary>
    /// Represents an avatar (other than your own)
    /// </summary>
    public class Avatar : Primitive
    {
        #region Subclasses

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
            /// <summary>Positive ratings given by this avatar</summary>
            public int GivenPositive;
            /// <summary>Negative ratings given by this avatar</summary>
            public int GivenNegative;
        }

        /// <summary>
        /// Avatar properties including about text, profile URL, image IDs and 
        /// publishing settings
        /// </summary>
        public struct AvatarProperties
        {
            /// <summary>First Life about text</summary>
            public string FirstLifeText;
            /// <summary>First Life image ID</summary>
            public UUID FirstLifeImage;
            /// <summary></summary>
            public UUID Partner;
            /// <summary></summary>
            public string AboutText;
            /// <summary></summary>
            public string BornOn;
            /// <summary></summary>
            public string CharterMember;
            /// <summary>Profile image ID</summary>
            public UUID ProfileImage;
            /// <summary>Flags of the profile</summary>
            public ProfileFlags Flags;
            /// <summary>Web URL for this profile</summary>
            public string ProfileURL;

            #region Properties

            /// <summary>Should this profile be published on the web</summary>
            public bool AllowPublish
            {
                get { return ((Flags & ProfileFlags.AllowPublish) != 0); }
                set
                {
                    if (value == true)
                        Flags |= ProfileFlags.AllowPublish;
                    else
                        Flags &= ~ProfileFlags.AllowPublish;
                }
            }
            /// <summary>Avatar Online Status</summary>
            public bool Online
            {
                get { return ((Flags & ProfileFlags.Online) != 0); }
                set
                {
                    if (value == true)
                        Flags |= ProfileFlags.Online;
                    else
                        Flags &= ~ProfileFlags.Online;
                }
            }
            /// <summary>Is this a mature profile</summary>
            public bool MaturePublish
            {
                get { return ((Flags & ProfileFlags.MaturePublish) != 0); }
                set
                {
                    if (value == true)
                        Flags |= ProfileFlags.MaturePublish;
                    else
                        Flags &= ~ProfileFlags.MaturePublish;
                }
            }
            /// <summary></summary>
            public bool Identified
            {
                get { return ((Flags & ProfileFlags.Identified) != 0); }
                set
                {
                    if (value == true)
                        Flags |= ProfileFlags.Identified;
                    else
                        Flags &= ~ProfileFlags.Identified;
                }
            }
            /// <summary></summary>
            public bool Transacted
            {
                get { return ((Flags & ProfileFlags.Transacted) != 0); }
                set
                {
                    if (value == true)
                        Flags |= ProfileFlags.Transacted;
                    else
                        Flags &= ~ProfileFlags.Transacted;
                }
            }

            #endregion Properties
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
            // FIXME:
            public uint SkillsMask;
            /// <summary></summary>
            public string SkillsText;
            /// <summary></summary>
            // FIXME:
            public uint WantToMask;
            /// <summary></summary>
            public string WantToText;
        }

        #endregion Subclasses

        #region Public Members

        /// <summary>Groups that this avatar is a member of</summary>
        public List<UUID> Groups = new List<UUID>();
        /// <summary>Positive and negative ratings</summary>
        public Statistics ProfileStatistics;
        /// <summary>Avatar properties including about text, profile URL, image IDs and 
        /// publishing settings</summary>
        public AvatarProperties ProfileProperties;
        /// <summary>Avatar interests including spoken languages, skills, and "want to"
        /// choices</summary>
        public Interests ProfileInterests;
        /// <summary>Movement control flags for avatars. Typically not set or used by
        /// clients. To move your avatar, use Client.Self.Movement instead</summary>
        public AgentManager.ControlFlags ControlFlags;

        #endregion Public Members

        protected string name;
        protected string groupName;

        #region Properties

        /// <summary>Full name</summary>
        public string Name
        {
            get
            {
                if (!String.IsNullOrEmpty(name))
                {
                    return name;
                }
                else
                {
                    lock (NameValues)
                    {
                        if (NameValues == null || NameValues.Length == 0)
                        {
                            return String.Empty;
                        }
                        else
                        {
                            string firstName = String.Empty;
                            string lastName = String.Empty;

                            for (int i = 0; i < NameValues.Length; i++)
                            {
                                if (NameValues[i].Name == "FirstName" && NameValues[i].Type == NameValue.ValueType.String)
                                    firstName = (string)NameValues[i].Value;
                                else if (NameValues[i].Name == "LastName" && NameValues[i].Type == NameValue.ValueType.String)
                                    lastName = (string)NameValues[i].Value;
                            }

                            if (firstName != String.Empty && lastName != String.Empty)
                            {
                                name = String.Format("{0} {1}", firstName, lastName);
                                return name;
                            }
                            else
                            {
                                return String.Empty;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>Active group</summary>
        public string GroupName
        {
            get
            {
                if (!String.IsNullOrEmpty(groupName))
                {
                    return groupName;
                }
                else
                {
                    lock (NameValues)
                    {
                        if (NameValues == null || NameValues.Length == 0)
                        {
                            return String.Empty;
                        }
                        else
                        {
                            for (int i = 0; i < NameValues.Length; i++)
                            {
                                if (NameValues[i].Name == "Title" && NameValues[i].Type == NameValue.ValueType.String)
                                {
                                    groupName = (string)NameValues[i].Value;
                                    return groupName;
                                }
                            }

                            return String.Empty;
                        }
                    }
                }
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public Avatar()
        {
        }

        #endregion Constructors
    }
}
