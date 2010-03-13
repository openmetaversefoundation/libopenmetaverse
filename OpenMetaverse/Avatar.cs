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
using OpenMetaverse.StructuredData;
using System.Reflection;

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

            public OSD GetOSD()
            {
                OSDMap tex = new OSDMap(8);
                tex["behavior_positive"] = OSD.FromInteger(BehaviorPositive);
                tex["behavior_negative"] = OSD.FromInteger(BehaviorNegative);
                tex["appearance_positive"] = OSD.FromInteger(AppearancePositive);
                tex["appearance_negative"] = OSD.FromInteger(AppearanceNegative);
                tex["buildings_positive"] = OSD.FromInteger(BuildingPositive);
                tex["buildings_negative"] = OSD.FromInteger(BuildingNegative);
                tex["given_positive"] = OSD.FromInteger(GivenPositive);
                tex["given_negative"] = OSD.FromInteger(GivenNegative);
                return tex;
            }

            public static Statistics FromOSD(OSD O)
            {
                Statistics S = new Statistics();
                OSDMap tex = (OSDMap)O;

                S.BehaviorPositive = tex["behavior_positive"].AsInteger();
                S.BuildingNegative = tex["behavior_negative"].AsInteger();
                S.AppearancePositive = tex["appearance_positive"].AsInteger();
                S.AppearanceNegative = tex["appearance_negative"].AsInteger();
                S.BuildingPositive = tex["buildings_positive"].AsInteger();
                S.BuildingNegative = tex["buildings_negative"].AsInteger();
                S.GivenPositive = tex["given_positive"].AsInteger();
                S.GivenNegative = tex["given_negative"].AsInteger();


                return S;

            }
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

            public OSD GetOSD()
            {
                OSDMap tex = new OSDMap(9);
                tex["first_life_text"] = OSD.FromString(FirstLifeText);
                tex["first_life_image"] = OSD.FromUUID(FirstLifeImage);
                tex["partner"] = OSD.FromUUID(Partner);
                tex["about_text"] = OSD.FromString(AboutText);
                tex["born_on"] = OSD.FromString(BornOn);
                tex["charter_member"] = OSD.FromString(CharterMember);
                tex["profile_image"] = OSD.FromUUID(ProfileImage);
                tex["flags"] = OSD.FromInteger((byte)Flags);
                tex["profile_url"] = OSD.FromString(ProfileURL);
                return tex;
            }

            public static AvatarProperties FromOSD(OSD O)
            {
                AvatarProperties A = new AvatarProperties();
                OSDMap tex = (OSDMap)O;

                A.FirstLifeText = tex["first_life_text"].AsString();
                A.FirstLifeImage = tex["first_life_image"].AsUUID();
                A.Partner = tex["partner"].AsUUID();
                A.AboutText = tex["about_text"].AsString();
                A.BornOn = tex["born_on"].AsString();
                A.CharterMember = tex["chart_member"].AsString();
                A.ProfileImage = tex["profile_image"].AsUUID();
                A.Flags = (ProfileFlags)tex["flags"].AsInteger();
                A.ProfileURL = tex["profile_url"].AsString();

                return A;

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

            public OSD GetOSD()
            {
                OSDMap InterestsOSD = new OSDMap(5);
                InterestsOSD["languages_text"] = OSD.FromString(LanguagesText);
                InterestsOSD["skills_mask"] = OSD.FromUInteger(SkillsMask);
                InterestsOSD["skills_text"] = OSD.FromString(SkillsText);
                InterestsOSD["want_to_mask"] = OSD.FromUInteger(WantToMask);
                InterestsOSD["want_to_text"] = OSD.FromString(WantToText);
                return InterestsOSD;
            }

            public static Interests FromOSD(OSD O)
            {
                Interests I = new Interests();
                OSDMap tex = (OSDMap)O;

                I.LanguagesText = tex["languages_text"].AsString();
                I.SkillsMask = tex["skills_mask"].AsUInteger();
                I.SkillsText = tex["skills_text"].AsString();
                I.WantToMask = tex["want_to_mask"].AsUInteger();
                I.WantToText = tex["want_to_text"].AsString();

                return I;

            }
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

        /// <summary>
        /// Contains the visual parameters describing the deformation of the avatar
        /// </summary>
        public byte[] VisualParameters = null;

        #endregion Public Members

        protected string name;
        protected string groupName;

        #region Properties

        /// <summary>First name</summary>
        public string FirstName
        {
            get
            {
                for (int i = 0; i < NameValues.Length; i++)
                {
                    if (NameValues[i].Name == "FirstName" && NameValues[i].Type == NameValue.ValueType.String)
                        return (string)NameValues[i].Value;
                }

                return String.Empty;
            }
        }

        /// <summary>Last name</summary>
        public string LastName
        {
            get
            {
                for (int i = 0; i < NameValues.Length; i++)
                {
                    if (NameValues[i].Name == "LastName" && NameValues[i].Type == NameValue.ValueType.String)
                        return (string)NameValues[i].Value;
                }

                return String.Empty;
            }
        }

        /// <summary>Full name</summary>
        public string Name
        {
            get
            {
                if (!String.IsNullOrEmpty(name))
                {
                    return name;
                }
                else if (NameValues != null && NameValues.Length > 0)
                {
                    lock (NameValues)
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
                else
                {
                    return String.Empty;
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
                    if (NameValues == null || NameValues.Length == 0)
                    {
                        return String.Empty;
                    }
                    else
                    {
                        lock (NameValues)
                        {
                            for (int i = 0; i < NameValues.Length; i++)
                            {
                                if (NameValues[i].Name == "Title" && NameValues[i].Type == NameValue.ValueType.String)
                                {
                                    groupName = (string)NameValues[i].Value;
                                    return groupName;
                                }
                            }
                        }
                        return String.Empty;
                    }
                }
            }
        }

        public override OSD GetOSD()
        {
            OSDMap Avi = (OSDMap)base.GetOSD();

            OSDArray grp = new OSDArray();
            Groups.ForEach(delegate(UUID u) { grp.Add(OSD.FromUUID(u)); });

            OSDArray vp = new OSDArray();

            for (int i = 0; i < VisualParameters.Length; i++)
            {
                vp.Add(OSD.FromInteger(VisualParameters[i]));
            }

            Avi["groups"] = grp;
            Avi["profile_statistics"] = ProfileStatistics.GetOSD();
            Avi["profile_properties"] = ProfileProperties.GetOSD();
            Avi["profile_interest"] = ProfileInterests.GetOSD();
            Avi["control_flags"] = OSD.FromInteger((byte)ControlFlags);
            Avi["visual_parameters"] = vp;
            Avi["first_name"] = OSD.FromString(FirstName);
            Avi["last_name"] = OSD.FromString(LastName);
            Avi["group_name"] = OSD.FromString(GroupName);

            return Avi;

        }

        public static new Avatar FromOSD(OSD O)
        {

            OSDMap tex = (OSDMap)O;

            Avatar A = new Avatar();
            
            Primitive P = Primitive.FromOSD(O);

            Type Prim = typeof(Primitive);

            FieldInfo[] Fields = Prim.GetFields();

            for (int x = 0; x < Fields.Length; x++)
            {
                Logger.Log("Field Matched in FromOSD: "+Fields[x].Name, Helpers.LogLevel.Debug);
                Fields[x].SetValue(A, Fields[x].GetValue(P));
            }            

            A.Groups = new List<UUID>();

            foreach (OSD U in (OSDArray)tex["groups"])
            {
                A.Groups.Add(U.AsUUID());
            }

            A.ProfileStatistics = Statistics.FromOSD(tex["profile_statistics"]);
            A.ProfileProperties = AvatarProperties.FromOSD(tex["profile_properties"]);
            A.ProfileInterests = Interests.FromOSD(tex["profile_interest"]);
            A.ControlFlags = (AgentManager.ControlFlags)tex["control_flags"].AsInteger();

            OSDArray vp = (OSDArray)tex["visual_parameters"];
            A.VisualParameters = new byte[vp.Count];

            for (int i = 0; i < vp.Count; i++)
            {
                A.VisualParameters[i] = (byte)vp[i].AsInteger();
            }

            // *********************From Code Above *******************************
            /*if (NameValues[i].Name == "FirstName" && NameValues[i].Type == NameValue.ValueType.String)
                              firstName = (string)NameValues[i].Value;
                          else if (NameValues[i].Name == "LastName" && NameValues[i].Type == NameValue.ValueType.String)
                              lastName = (string)NameValues[i].Value;*/
            // ********************************************************************

            A.NameValues = new NameValue[3];

            NameValue First = new NameValue();
            First.Name = "FirstName";
            First.Type = NameValue.ValueType.String;
            First.Value = tex["first_name"].AsString();

            NameValue Last = new NameValue();
            Last.Name = "LastName";
            Last.Type = NameValue.ValueType.String;
            Last.Value = tex["last_name"].AsString();

            // ***************From Code Above***************
            // if (NameValues[i].Name == "Title" && NameValues[i].Type == NameValue.ValueType.String)
            // *********************************************

            NameValue Group = new NameValue();
            Group.Name = "Title";
            Group.Type = NameValue.ValueType.String;
            Group.Value = tex["group_name"].AsString();



            A.NameValues[0] = First;
            A.NameValues[1] = Last;
            A.NameValues[2] = Group;

            return A;


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
