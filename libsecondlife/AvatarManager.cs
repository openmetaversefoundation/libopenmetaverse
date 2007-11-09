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
using System.Collections.Generic;
using System.Threading;
using libsecondlife.Packets;

namespace libsecondlife
{
    /// <summary>
    /// Retrieve friend status notifications, and retrieve avatar names and
    /// profiles
    /// </summary>
    public class AvatarManager
    {
        /// <summary>
        /// Triggered when a UUIDNameReply is received
        /// </summary>
        /// <param name="names"></param>
        public delegate void AvatarNamesCallback(Dictionary<LLUUID, string> names);
        /// <summary>
        /// Triggered when a response for avatar statistics (ratings) is returned
        /// </summary>
        /// <param name="avatarID"></param>
        /// <param name="statistics"></param>
        // public delegate void AvatarStatisticsCallback(LLUUID avatarID, Avatar.Statistics statistics);
        /// <summary>
        /// Triggered when a response for avatar interests is returned
        /// </summary>
        /// <param name="avatarID"></param>
        /// <param name="interests"></param>
        public delegate void AvatarInterestsCallback(LLUUID avatarID, Avatar.Interests interests);
        /// <summary>
        /// Triggered when avatar properties are received (AvatarPropertiesReply)
        /// </summary>
        /// <param name="avatarID"></param>
        /// <param name="properties"></param>
        public delegate void AvatarPropertiesCallback(LLUUID avatarID, Avatar.AvatarProperties properties);
        /// <summary>
        /// Triggered when an avatar group list is received (AvatarGroupsReply)
        /// </summary>
        /// <param name="avatarID"></param>
        /// <param name="groups"></param>
        public delegate void AvatarGroupsCallback(LLUUID avatarID, AvatarGroupsReplyPacket.GroupDataBlock[] groups);
        /// <summary>
        /// Triggered when a name search reply is received (AvatarPickerReply)
        /// </summary>
        /// <param name="queryID"></param>
        /// <param name="avatars"></param>
        public delegate void AvatarNameSearchCallback(LLUUID queryID, Dictionary<LLUUID, string> avatars);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceID"></param>
        /// <param name="targetID"></param>
        /// <param name="targetPos"></param>
        /// <param name="pointType"></param>
        /// <param name="duration"></param>
        /// <param name="id"></param>
        public delegate void PointAtCallback(LLUUID sourceID, LLUUID targetID, LLVector3d targetPos, 
            PointAtType pointType, float duration, LLUUID id);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceID"></param>
        /// <param name="targetID"></param>
        /// <param name="targetPos"></param>
        /// <param name="lookType"></param>
        /// <param name="duration"></param>
        /// <param name="id"></param>
        public delegate void LookAtCallback(LLUUID sourceID, LLUUID targetID, LLVector3d targetPos,
            LookAtType lookType, float duration, LLUUID id);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sourceID"></param>
        /// <param name="targetID"></param>
        /// <param name="targetPos"></param>
        /// <param name="duration"></param>
        /// <param name="id"></param>
        public delegate void EffectCallback(EffectType type, LLUUID sourceID, LLUUID targetID,
            LLVector3d targetPos, float duration, LLUUID id);


        /// <summary></summary>
        public event AvatarNamesCallback OnAvatarNames;
        /// <summary></summary>
        // public event AvatarStatisticsCallback OnAvatarStatistics;
        /// <summary></summary>
        public event AvatarInterestsCallback OnAvatarInterests;
        /// <summary></summary>
        public event AvatarPropertiesCallback OnAvatarProperties;
        /// <summary></summary>
        public event AvatarGroupsCallback OnAvatarGroups;
        /// <summary></summary>
        public event AvatarNameSearchCallback OnAvatarNameSearch;
        /// <summary></summary>
        public event PointAtCallback OnPointAt;
        /// <summary></summary>
        public event LookAtCallback OnLookAt;
        /// <summary></summary>
        public event EffectCallback OnEffect;

        private SecondLife Client;

        /// <summary>
        /// Represents other avatars
        /// </summary>
        /// <param name="client"></param>
        public AvatarManager(SecondLife client)
        {
            Client = client;

            // Avatar profile callbacks
            Client.Network.RegisterCallback(PacketType.AvatarPropertiesReply, new NetworkManager.PacketCallback(AvatarPropertiesHandler));
            // Client.Network.RegisterCallback(PacketType.AvatarStatisticsReply, new NetworkManager.PacketCallback(AvatarStatisticsHandler));
            Client.Network.RegisterCallback(PacketType.AvatarInterestsReply, new NetworkManager.PacketCallback(AvatarInterestsHandler));

            // Avatar group callback
            Client.Network.RegisterCallback(PacketType.AvatarGroupsReply, new NetworkManager.PacketCallback(AvatarGroupsHandler));

            // Viewer effect callback
            Client.Network.RegisterCallback(PacketType.ViewerEffect, new NetworkManager.PacketCallback(ViewerEffectHandler));

            // Other callbacks
            Client.Network.RegisterCallback(PacketType.UUIDNameReply, new NetworkManager.PacketCallback(AvatarNameHandler));
            Client.Network.RegisterCallback(PacketType.AvatarPickerReply, new NetworkManager.PacketCallback(AvatarPickerReplyHandler));
	        Client.Network.RegisterCallback(PacketType.AvatarAnimation, new NetworkManager.PacketCallback(AvatarAnimationHandler));
        }

        /// <summary>Tracks the specified avatar on your map</summary>
        /// <param name="preyID">Avatar ID to track</param>
        public void TrackAvatar(LLUUID preyID)
        {
            TrackAgentPacket p = new TrackAgentPacket();
            p.AgentData.AgentID = Client.Self.AgentID;
            p.AgentData.SessionID = Client.Self.SessionID;
            p.TargetData.PreyID = preyID;
            Client.Network.SendPacket(p);
        }

        /// <summary>
        /// Request a single avatar name
        /// </summary>
        /// <param name="id">The avatar key to retrieve a name for</param>
        public void RequestAvatarName(LLUUID id)
        {
            UUIDNameRequestPacket request = new UUIDNameRequestPacket();
            request.UUIDNameBlock = new UUIDNameRequestPacket.UUIDNameBlockBlock[1];
            request.UUIDNameBlock[0] = new UUIDNameRequestPacket.UUIDNameBlockBlock();
            request.UUIDNameBlock[0].ID = id;

            Client.Network.SendPacket(request);
        }

        /// <summary>
        /// Request a list of avatar names
        /// </summary>
        /// <param name="ids">The avatar keys to retrieve names for</param>
        public void RequestAvatarNames(List<LLUUID> ids)
        {
            UUIDNameRequestPacket request = new UUIDNameRequestPacket();
            request.UUIDNameBlock = new UUIDNameRequestPacket.UUIDNameBlockBlock[ids.Count];

            for (int i = 0; i < ids.Count; i++)
            {
                request.UUIDNameBlock[i] = new UUIDNameRequestPacket.UUIDNameBlockBlock();
                request.UUIDNameBlock[i].ID = ids[i];
            }

            Client.Network.SendPacket(request);
        }

        /// <summary>
        /// Start a request for Avatar Properties
        /// </summary>
        /// <param name="avatarid"></param>
        public void RequestAvatarProperties(LLUUID avatarid)
        {
            AvatarPropertiesRequestPacket aprp = new AvatarPropertiesRequestPacket();
            
            aprp.AgentData.AgentID = Client.Self.AgentID;
            aprp.AgentData.SessionID = Client.Self.SessionID;
            aprp.AgentData.AvatarID = avatarid;

            Client.Network.SendPacket(aprp);
        }

        /// <summary>
        /// Search for an avatar (first name, last name, and uuid)
        /// </summary>
        /// <param name="name">The name to search for</param>
        /// <param name="queryID">An ID to associate with this query</param>
        public void RequestAvatarNameSearch(string name, LLUUID queryID)
        {
            AvatarPickerRequestPacket aprp = new AvatarPickerRequestPacket();

            aprp.AgentData.AgentID = Client.Self.AgentID;
            aprp.AgentData.SessionID = Client.Self.SessionID;
            aprp.AgentData.QueryID = queryID;
            aprp.Data.Name = Helpers.StringToField(name);

            Client.Network.SendPacket(aprp);
        }

        #region Packet Handlers

        /// <summary>
        /// Process an incoming UUIDNameReply Packet and insert Full Names into the Avatars Dictionary
        /// </summary>
        /// <param name="packet">Incoming Packet to process</param>
        /// <param name="simulator">Unused</param>
        private void AvatarNameHandler(Packet packet, Simulator simulator)
        {
            if (OnAvatarNames != null)
            {
                Dictionary<LLUUID, string> names = new Dictionary<LLUUID, string>();
                UUIDNameReplyPacket reply = (UUIDNameReplyPacket)packet;

                foreach (UUIDNameReplyPacket.UUIDNameBlockBlock block in reply.UUIDNameBlock)
                {
                    names[block.ID] = Helpers.FieldToUTF8String(block.FirstName) +
                        " " + Helpers.FieldToUTF8String(block.LastName);
                }
                
                OnAvatarNames(names);
            }
        }

        /// <summary>
        /// Process incoming avatar properties (profile data)
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="sim"></param>
        private void AvatarPropertiesHandler(Packet packet, Simulator sim)
        {
            if (OnAvatarProperties != null)
            {
                AvatarPropertiesReplyPacket reply = (AvatarPropertiesReplyPacket)packet;
                Avatar.AvatarProperties properties = new Avatar.AvatarProperties();

                properties.ProfileImage = reply.PropertiesData.ImageID;
                properties.FirstLifeImage = reply.PropertiesData.FLImageID;
                properties.Partner = reply.PropertiesData.PartnerID;
                properties.AboutText = Helpers.FieldToUTF8String(reply.PropertiesData.AboutText);
                properties.FirstLifeText = Helpers.FieldToUTF8String(reply.PropertiesData.FLAboutText);
                properties.BornOn = Helpers.FieldToUTF8String(reply.PropertiesData.BornOn);
                //properties.CharterMember = Helpers.FieldToUTF8String(reply.PropertiesData.CharterMember);
                uint charter = Helpers.BytesToUInt(reply.PropertiesData.CharterMember);
                if ( charter == 0 ) {
                    properties.CharterMember = "Resident";
                } else if ( charter == 2 ) {
                    properties.CharterMember = "Charter";
                } else if ( charter == 3 ) {
                    properties.CharterMember = "Linden";
                } else {
                    properties.CharterMember = Helpers.FieldToUTF8String(reply.PropertiesData.CharterMember);
                }
                properties.Flags = (Avatar.ProfileFlags)reply.PropertiesData.Flags;
                properties.ProfileURL = Helpers.FieldToUTF8String(reply.PropertiesData.ProfileURL);

                OnAvatarProperties(reply.AgentData.AvatarID, properties);
            }
        }

        /// <summary>
        /// Process incoming Avatar Interests information
        /// </summary>
        private void AvatarInterestsHandler(Packet packet, Simulator simulator)
        {
            if (OnAvatarInterests != null)
            {
                AvatarInterestsReplyPacket airp = (AvatarInterestsReplyPacket)packet;
                Avatar.Interests interests = new Avatar.Interests();

                interests.WantToMask = airp.PropertiesData.WantToMask;
                interests.WantToText = Helpers.FieldToUTF8String(airp.PropertiesData.WantToText);
                interests.SkillsMask = airp.PropertiesData.SkillsMask;
                interests.SkillsText = Helpers.FieldToUTF8String(airp.PropertiesData.SkillsText);
                interests.LanguagesText = Helpers.FieldToUTF8String(airp.PropertiesData.LanguagesText);

                OnAvatarInterests(airp.AgentData.AvatarID, interests);
            }
        }

        private void AvatarGroupsHandler(Packet packet, Simulator simulator)
        {
            if (OnAvatarGroups != null)
            {
                AvatarGroupsReplyPacket groups = (AvatarGroupsReplyPacket)packet;

                // FIXME: Build a little struct to represent the groups.GroupData blocks so we keep
                // libsecondlife.Packets abstracted away
                OnAvatarGroups(groups.AgentData.AvatarID, groups.GroupData);
            }
        }

        private void AvatarPickerReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnAvatarNameSearch != null)
            {
                AvatarPickerReplyPacket reply = (AvatarPickerReplyPacket)packet;
                Dictionary<LLUUID, string> avatars = new Dictionary<LLUUID, string>();

                foreach (AvatarPickerReplyPacket.DataBlock block in reply.Data)
                {
                    avatars[block.AvatarID] = Helpers.FieldToUTF8String(block.FirstName) +
                        " " + Helpers.FieldToUTF8String(block.LastName);
                }

                OnAvatarNameSearch(reply.AgentData.QueryID, avatars);
            }
        }

        /// <summary>
        /// Process an incoming effect
        /// </summary>
        private void ViewerEffectHandler(Packet packet, Simulator simulator)
        {
            ViewerEffectPacket effect = (ViewerEffectPacket)packet;

            foreach (ViewerEffectPacket.EffectBlock block in effect.Effect)
            {
                EffectType type = (EffectType)block.Type;

                //LLColor color;
                //if (block.Color.Length == 4)
                //{
                //    color = new LLColor(block.Color, 0);
                //}
                //else
                //{
                //    Client.Log("Received a ViewerEffect.EffectBlock.Color array with " + block.Color.Length + 
                //        " bytes", Helpers.LogLevel.Warning);
                //    color = LLColor.Black;
                //}

                // Each ViewerEffect type uses it's own custom binary format for additional data. Fun eh?
                switch (type)
                {
                    case EffectType.Text:
                        Client.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning);
                        break;
                    case EffectType.Icon:
                        Client.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning);
                        break;
                    case EffectType.Connector:
                        Client.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning);
                        break;
                    case EffectType.FlexibleObject:
                        Client.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning);
                        break;
                    case EffectType.AnimalControls:
                        Client.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning);
                        break;
                    case EffectType.AnimationObject:
                        Client.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning);
                        break;
                    case EffectType.Cloth:
                        Client.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning);
                        break;
                    case EffectType.Glow:
                        Client.Log("Received a Glow ViewerEffect which is not implemented yet",
                            Helpers.LogLevel.Warning);
                        break;
                    case EffectType.Beam:
                    case EffectType.Point:
                    case EffectType.Trail:
                    case EffectType.Sphere:
                    case EffectType.Spiral:
                    case EffectType.Edit:
                        if (OnEffect != null)
                        {
                            if (block.TypeData.Length == 56)
                            {
                                LLUUID sourceAvatar = new LLUUID(block.TypeData, 0);
                                LLUUID targetObject = new LLUUID(block.TypeData, 16);
                                LLVector3d targetPos = new LLVector3d(block.TypeData, 32);

                                try { OnEffect(type, sourceAvatar, targetObject, targetPos, block.Duration, block.ID); }
                                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                            }
                            else
                            {
                                Client.Log("Received a " + type.ToString() + 
                                    " ViewerEffect with an incorrect TypeData size of " +
                                    block.TypeData.Length + " bytes", Helpers.LogLevel.Warning);
                            }
                        }
                        break;
                    case EffectType.LookAt:
                        if (OnLookAt != null)
                        {
                            if (block.TypeData.Length == 57)
                            {
                                LLUUID sourceAvatar = new LLUUID(block.TypeData, 0);
                                LLUUID targetObject = new LLUUID(block.TypeData, 16);
                                LLVector3d targetPos = new LLVector3d(block.TypeData, 32);
                                LookAtType lookAt = (LookAtType)block.TypeData[56];

                                try { OnLookAt(sourceAvatar, targetObject, targetPos, lookAt, block.Duration,
                                    block.ID); }
                                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                            }
                            else
                            {
                                Client.Log("Received a LookAt ViewerEffect with an incorrect TypeData size of " +
                                    block.TypeData.Length + " bytes", Helpers.LogLevel.Warning);
                            }
                        }
                        break;
                    case EffectType.PointAt:
                        if (OnPointAt != null)
                        {
                            if (block.TypeData.Length == 57)
                            {
                                LLUUID sourceAvatar = new LLUUID(block.TypeData, 0);
                                LLUUID targetObject = new LLUUID(block.TypeData, 16);
                                LLVector3d targetPos = new LLVector3d(block.TypeData, 32);
                                PointAtType pointAt = (PointAtType)block.TypeData[56];

                                try { OnPointAt(sourceAvatar, targetObject, targetPos, pointAt, block.Duration, 
                                    block.ID); }
                                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                            }
                            else
                            {
                                Client.Log("Received a PointAt ViewerEffect with an incorrect TypeData size of " +
                                    block.TypeData.Length + " bytes", Helpers.LogLevel.Warning);
                            }
                        }
                        break;
                    default:
                        Client.Log("Received a ViewerEffect with an unknown type " + type, Helpers.LogLevel.Warning);
                        break;
                }
            }
        }

        protected void AvatarAnimationHandler(Packet packet, Simulator sim)
        {
            //FIXME
        }

        #endregion Packet Handlers
    }
}
