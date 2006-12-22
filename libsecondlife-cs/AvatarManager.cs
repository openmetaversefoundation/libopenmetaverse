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
        /// Triggered after friend request packet is sent out
        /// </summary>
        /// <param name="agentID"></param>
        /// <param name="online"></param>
        public delegate void FriendNotificationCallback(LLUUID agentID, bool online);
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
        public delegate void AvatarStatisticsCallback(LLUUID avatarID, Avatar.Statistics statistics);
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
        public delegate void AvatarPropertiesCallback(LLUUID avatarID, Avatar.Properties properties);
        /// <summary>
        /// Triggered when a name search reply is received (AvatarPickerReply)
        /// </summary>
        /// <param name="queryID"></param>
        /// <param name="avatars"></param>
        public delegate void AvatarNameSearchCallback(LLUUID queryID, Dictionary<LLUUID, string> avatars);


        /// <summary>Triggered whenever a friend comes online or goes offline</summary>
        public event FriendNotificationCallback OnFriendNotification;
        /// <summary></summary>
        public event AvatarNamesCallback OnAvatarNames;
        /// <summary></summary>
        public event AvatarStatisticsCallback OnAvatarStatistics;
        /// <summary></summary>
        public event AvatarInterestsCallback OnAvatarInterests;
        /// <summary></summary>
        public event AvatarPropertiesCallback OnAvatarProperties;
        /// <summary></summary>
        public event AvatarNameSearchCallback OnAvatarNameSearch;

        private SecondLife Client;

        /// <summary>
        /// Represents other avatars
        /// </summary>
        /// <param name="client"></param>
        public AvatarManager(SecondLife client)
        {
            Client = client;

            // Friend notification callback
            NetworkManager.PacketCallback callback = new NetworkManager.PacketCallback(FriendNotificationHandler);
            Client.Network.RegisterCallback(PacketType.OnlineNotification, callback);
            Client.Network.RegisterCallback(PacketType.OfflineNotification, callback);

            // Avatar profile callbacks
            Client.Network.RegisterCallback(PacketType.AvatarPropertiesReply, new NetworkManager.PacketCallback(AvatarPropertiesHandler));
            Client.Network.RegisterCallback(PacketType.AvatarStatisticsReply, new NetworkManager.PacketCallback(AvatarStatisticsHandler));
            Client.Network.RegisterCallback(PacketType.AvatarInterestsReply, new NetworkManager.PacketCallback(AvatarInterestsHandler));

            // Other callbacks
            Client.Network.RegisterCallback(PacketType.UUIDNameReply, new NetworkManager.PacketCallback(AvatarNameHandler));
            Client.Network.RegisterCallback(PacketType.AvatarPickerReply, new NetworkManager.PacketCallback(AvatarPickerReplyHandler));
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
        /// <param name="aic"></param>
        /// <param name="asc"></param>
        /// <param name="apc"></param>
        public void RequestAvatarProperties(LLUUID avatarid)
        {
            AvatarPropertiesRequestPacket aprp = new AvatarPropertiesRequestPacket();
            
            aprp.AgentData.AgentID = Client.Network.AgentID;
            aprp.AgentData.SessionID = Client.Network.SessionID;
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

            aprp.AgentData.AgentID = Client.Network.AgentID;
            aprp.AgentData.SessionID = Client.Network.SessionID;
            aprp.AgentData.QueryID = queryID;
            aprp.Data.Name = Helpers.StringToField(name);

            Client.Network.SendPacket(aprp);
        }

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
                    names[block.ID] = Helpers.FieldToString(block.FirstName) +
                        " " + Helpers.FieldToString(block.LastName);
                }
                
                OnAvatarNames(names);
            }
        }

        /// <summary>
        /// Handle incoming friend notifications
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void FriendNotificationHandler(Packet packet, Simulator simulator)
        {
            if (OnFriendNotification != null)
            {
                if (packet.Type == PacketType.OnlineNotification)
                {
                    // If the agent is online
                    foreach (OnlineNotificationPacket.AgentBlockBlock block in ((OnlineNotificationPacket)packet).AgentBlock)
                        OnFriendNotification(block.AgentID, true);
                }
                else if (packet.Type == PacketType.OfflineNotification)
                {
                    // If the agent is offline
                    foreach (OfflineNotificationPacket.AgentBlockBlock block in ((OfflineNotificationPacket)packet).AgentBlock)
                        OnFriendNotification(block.AgentID, true);
                }
            }
        }

        /// <summary>
        /// Handles incoming avatar statistics, such as ratings
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void AvatarStatisticsHandler(Packet packet, Simulator simulator)
        {
            if (OnAvatarStatistics != null)
            {
                AvatarStatisticsReplyPacket asr = (AvatarStatisticsReplyPacket)packet;
                Avatar.Statistics stats = new Avatar.Statistics();

                foreach (AvatarStatisticsReplyPacket.StatisticsDataBlock b in asr.StatisticsData)
                {
                    string n = Helpers.FieldToString(b.Name);

                    switch (n)
                    {
                        case "Behavior":
                            stats.BehaviorPositive = b.Positive;
                            stats.BehaviorNegative = b.Negative;
                            break;
                        case "Appearance":
                            stats.AppearancePositive = b.Positive;
                            stats.AppearanceNegative = b.Negative;
                            break;
                        case "Building":
                            stats.AppearancePositive = b.Positive;
                            stats.AppearanceNegative = b.Negative;
                            break;
                        default:
                            Client.Log("Got an AvatarStatistics block with the name " + n, Helpers.LogLevel.Warning);
                            break;
                    }
                }

                OnAvatarStatistics(asr.AvatarData.AvatarID, stats);
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
                Avatar.Properties properties = new Avatar.Properties();

                properties.ProfileImage = reply.PropertiesData.ImageID;
                properties.FirstLifeImage = reply.PropertiesData.FLImageID;
                properties.Partner = reply.PropertiesData.PartnerID;
                properties.AboutText = Helpers.FieldToString(reply.PropertiesData.AboutText);
                properties.FirstLifeText = Helpers.FieldToString(reply.PropertiesData.FLAboutText);
                properties.BornOn = Helpers.FieldToString(reply.PropertiesData.BornOn);
                properties.CharterMember = Helpers.FieldToString(reply.PropertiesData.CharterMember);
                properties.AllowPublish = reply.PropertiesData.AllowPublish;
                properties.MaturePublish = reply.PropertiesData.MaturePublish;
                properties.Identified = reply.PropertiesData.Identified;
                properties.Transacted = reply.PropertiesData.Transacted;
                properties.ProfileURL = Helpers.FieldToString(reply.PropertiesData.ProfileURL);

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
                interests.WantToText = Helpers.FieldToString(airp.PropertiesData.WantToText);
                interests.SkillsMask = airp.PropertiesData.SkillsMask;
                interests.SkillsText = Helpers.FieldToString(airp.PropertiesData.SkillsText);
                interests.LanguagesText = Helpers.FieldToString(airp.PropertiesData.LanguagesText);

                OnAvatarInterests(airp.AgentData.AvatarID, interests);
            }
        }

        private void AvatarPickerReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnAvatarNameSearch != null)
            {
                AvatarPickerReplyPacket reply = (AvatarPickerReplyPacket)packet;
                Dictionary<LLUUID, string> avatars = new Dictionary<LLUUID,string>();

                foreach (AvatarPickerReplyPacket.DataBlock block in reply.Data)
                {
                    avatars[block.AvatarID] = Helpers.FieldToString(block.FirstName) +
                        " " + Helpers.FieldToString(block.LastName);
                }

                OnAvatarNameSearch(reply.AgentData.QueryID, avatars);
            }
        }
    }
}
