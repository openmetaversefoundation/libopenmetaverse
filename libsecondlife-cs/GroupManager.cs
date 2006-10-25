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
using libsecondlife.Packets;

namespace libsecondlife
{
    public class GroupMember
    {
        public LLUUID ID;
        public int Contribution;
        public string OnlineStatus;
        public ulong Powers;
        public string Title;
        public bool IsOwner;

        public GroupMember(LLUUID id)
        {
            ID = id;
        }
    }

    public class GroupRole
    {
        public LLUUID ID;
        public string Name;
        public string Title;
        public string Description;
        public ulong Powers;

        public GroupRole(LLUUID id)
        {
            ID = id;
        }
    }

    public class GroupTitle
    {
        public string Title;
        public bool Selected;
    }

    /// <summary>
    /// Represents a group in Second Life
    /// </summary>
    public class Group
    {
        public LLUUID ID;
        public LLUUID InsigniaID;
        public LLUUID FounderID;
        public LLUUID OwnerRole;
        public string Name;
        public string Charter;
        public string MemberTitle;
        public bool OpenEnrollment;
        public bool ShowInList;
        public ulong Powers;
        public bool AcceptNotices;
        public bool AllowPublish;
        public bool MaturePublish;
        public int MembershipFee;
        public int Money;
        public int Contribution;
        public int GroupMembershipCount;
        public int GroupRolesCount;

        public Group(LLUUID id)
        {
            ID = id;
            InsigniaID = new LLUUID();
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class GroupProfile
    {
        public LLUUID ID;
        public LLUUID InsigniaID;
        public LLUUID FounderID;
        public LLUUID OwnerRole;
        public string Name;
        public string Charter;
        public string MemberTitle;
        public bool OpenEnrollment;
        public bool ShowInList;
        public ulong Powers;
        public bool AcceptNotices;
        public bool AllowPublish;
        public bool MaturePublish;
        public int MembershipFee;
        public int Money;
        public int Contribution;
        public int GroupMembershipCount;
        public int GroupRolesCount;
    }

    /// <summary>
    /// Handles all network traffic related to reading and writing group
    /// information
    /// </summary>
    public class GroupManager
    {
        /// <summary>
        /// Callback for the list of groups the avatar is currently a member of
        /// </summary>
        /// <param name="groups"></param>
        public delegate void CurrentGroupsCallback(Dictionary<LLUUID,Group> groups);
        /// <summary>
        /// Callback for the profile of a group
        /// </summary>
        /// <param name="group"></param>
        public delegate void GroupProfileCallback(GroupProfile group);
        /// <summary>
        /// Callback for the member list of a group
        /// </summary>
        /// <param name="members"></param>
        public delegate void GroupMembersCallback(Dictionary<LLUUID, GroupMember> members);
        /// <summary>
        /// Callback for the role list of a group
        /// </summary>
        /// <param name="roles"></param>
        public delegate void GroupRolesCallback(Dictionary<LLUUID, GroupRole> roles);
        /// <summary>
        /// Callback for the title list of a group
        /// </summary>
        /// <param name="titles"></param>
        public delegate void GroupTitlesCallback(Dictionary<LLUUID, GroupTitle> titles);

        private SecondLife Client;
        // No need for concurrency with the current group list request
        private CurrentGroupsCallback OnCurrentGroups;
        private Dictionary<LLUUID, GroupProfileCallback> GroupProfileCallbacks;
        private Dictionary<LLUUID, GroupMembersCallback> GroupMembersCallbacks;
        private Dictionary<LLUUID, GroupRolesCallback> GroupRolesCallbacks;
        private Dictionary<LLUUID, GroupTitlesCallback> GroupTitlesCallbacks;
        /// <summary>A list of all the lists of group members, indexed by the request ID</summary>
        private Dictionary<LLUUID, Dictionary<LLUUID, GroupMember>> GroupMembersCaches;
        /// <summary>A list of all the lists of group roles, indexed by the request ID</summary>
        private Dictionary<LLUUID, Dictionary<LLUUID, GroupRole>> GroupRolesCaches;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public GroupManager(SecondLife client)
        {
            Client = client;

            GroupProfileCallbacks = new Dictionary<LLUUID, GroupProfileCallback>();
            GroupMembersCallbacks = new Dictionary<LLUUID, GroupMembersCallback>();
            GroupRolesCallbacks = new Dictionary<LLUUID, GroupRolesCallback>();
            GroupTitlesCallbacks = new Dictionary<LLUUID, GroupTitlesCallback>();

            GroupMembersCaches = new Dictionary<LLUUID, Dictionary<LLUUID, GroupMember>>();
            GroupRolesCaches = new Dictionary<LLUUID, Dictionary<LLUUID, GroupRole>>();

            Client.Network.RegisterCallback(PacketType.AgentGroupDataUpdate, new PacketCallback(GroupDataHandler));
            Client.Network.RegisterCallback(PacketType.GroupTitlesReply, new PacketCallback(GroupTitlesHandler));
            Client.Network.RegisterCallback(PacketType.GroupProfileReply, new PacketCallback(GroupProfileHandler));
            Client.Network.RegisterCallback(PacketType.GroupMembersReply, new PacketCallback(GroupMembersHandler));
            Client.Network.RegisterCallback(PacketType.GroupRoleDataReply, new PacketCallback(GroupRoleDataHandler));
            Client.Network.RegisterCallback(PacketType.GroupRoleMembersReply, new PacketCallback(GroupRoleMembersHandler));
            Client.Network.RegisterCallback(PacketType.GroupActiveProposalItemReply, new PacketCallback(GroupActiveProposalItemHandler));
            Client.Network.RegisterCallback(PacketType.GroupVoteHistoryItemReply, new PacketCallback(GroupVoteHistoryItemHandler));
            Client.Network.RegisterCallback(PacketType.GroupAccountSummaryReply, new PacketCallback(GroupAccountSummaryHandler));
            Client.Network.RegisterCallback(PacketType.GroupAccountDetailsReply, new PacketCallback(GroupAccountDetailsHandler));
            Client.Network.RegisterCallback(PacketType.GroupAccountTransactionsReply, new PacketCallback(GroupAccountTransactionsHandler));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cgc"></param>
        public void BeginGetCurrentGroups(CurrentGroupsCallback cgc)
        {
            OnCurrentGroups = cgc;

            AgentDataUpdateRequestPacket request = new AgentDataUpdateRequestPacket();

            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;

            Client.Network.SendPacket(request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <param name="gpc"></param>
        public void BeginGetGroupProfile(LLUUID group, GroupProfileCallback gpc)
        {
            GroupProfileCallbacks[group] = gpc;

            GroupProfileRequestPacket request = new GroupProfileRequestPacket();

            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;
            request.GroupData.GroupID = group;

            Client.Network.SendPacket(request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <param name="gmc"></param>
        public void BeginGetGroupMembers(LLUUID group, GroupMembersCallback gmc)
        {
            LLUUID requestID = LLUUID.GenerateUUID();

            lock (GroupMembersCaches)
            {
                GroupMembersCaches[requestID] = new Dictionary<LLUUID, GroupMember>();
            }

            GroupMembersCallbacks[group] = gmc;

            GroupMembersRequestPacket request = new GroupMembersRequestPacket();

            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;
            request.GroupData.GroupID = group;
            request.GroupData.RequestID = requestID;

            Client.Network.SendPacket(request);
        }

        public void BeginGetGroupRoles(LLUUID group, GroupRolesCallback grc)
        {
            LLUUID requestID = LLUUID.GenerateUUID();

            lock (GroupRolesCaches)
            {
                GroupRolesCaches[requestID] = new Dictionary<LLUUID, GroupRole>();
            }

            GroupRolesCallbacks[group] = grc;

            GroupRoleDataRequestPacket request = new GroupRoleDataRequestPacket();

            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;
            request.GroupData.GroupID = group;
            request.GroupData.RequestID = requestID;

            Client.Network.SendPacket(request);
        }

        public void BeginGetGroupTitles(LLUUID group, GroupTitlesCallback gtc)
        {
            LLUUID requestID = LLUUID.GenerateUUID();

            GroupTitlesCallbacks[group] = gtc;

            GroupTitlesRequestPacket request = new GroupTitlesRequestPacket();

            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;
            request.AgentData.GroupID = group;
            request.AgentData.RequestID = requestID;

            Client.Network.SendPacket(request);
        }

        private void GroupDataHandler(Packet packet, Simulator simulator)
        {
            if (OnCurrentGroups != null)
            {
                AgentGroupDataUpdatePacket update = (AgentGroupDataUpdatePacket)packet;

                Dictionary<LLUUID, Group> currentGroups = new Dictionary<LLUUID, Group>();

                foreach (AgentGroupDataUpdatePacket.GroupDataBlock block in update.GroupData)
                {
                    Group group = new Group(block.GroupID);

                    group.InsigniaID = block.GroupInsigniaID;
                    group.Name = Helpers.FieldToString(block.GroupName);
                    group.Powers = block.GroupPowers;
                    group.Contribution = block.Contribution;
                    group.AcceptNotices = block.AcceptNotices;

                    currentGroups[block.GroupID] = group;
                }

                if (OnCurrentGroups != null)
                {
                    OnCurrentGroups(currentGroups);
                }
            }
        }

        private void GroupProfileHandler(Packet packet, Simulator simulator)
        {
            GroupProfileReplyPacket profile = (GroupProfileReplyPacket)packet;

            if (GroupProfileCallbacks.ContainsKey(profile.GroupData.GroupID))
            {
                GroupProfile group = new GroupProfile();

                group.AllowPublish = profile.GroupData.AllowPublish;
                group.Charter = Helpers.FieldToString(profile.GroupData.Charter);
                group.FounderID = profile.GroupData.FounderID;
                group.GroupMembershipCount = profile.GroupData.GroupMembershipCount;
                group.GroupRolesCount = profile.GroupData.GroupRolesCount;
                group.InsigniaID = profile.GroupData.InsigniaID;
                group.MaturePublish = profile.GroupData.MaturePublish;
                group.MembershipFee = profile.GroupData.MembershipFee;
                group.MemberTitle = Helpers.FieldToString(profile.GroupData.MemberTitle);
                group.Money = profile.GroupData.Money;
                group.Name = Helpers.FieldToString(profile.GroupData.Name);
                group.OpenEnrollment = profile.GroupData.OpenEnrollment;
                group.OwnerRole = profile.GroupData.OwnerRole;
                group.Powers = profile.GroupData.PowersMask;
                group.ShowInList = profile.GroupData.ShowInList;

                GroupProfileCallbacks[profile.GroupData.GroupID](group);
            }
        }

        private void GroupTitlesHandler(Packet packet, Simulator simulator)
        {
            GroupTitlesReplyPacket titles = (GroupTitlesReplyPacket)packet;

            Dictionary<LLUUID, GroupTitle> groupTitleCache = new Dictionary<LLUUID, GroupTitle>();

            foreach (GroupTitlesReplyPacket.GroupDataBlock block in titles.GroupData)
            {
                GroupTitle groupTitle = new GroupTitle();

                groupTitle.Title = Helpers.FieldToString(block.Title);
                groupTitle.Selected = block.Selected;

                groupTitleCache[block.RoleID] = groupTitle;
            }

            GroupTitlesCallbacks[titles.AgentData.GroupID](groupTitleCache);
        }

        private void GroupMembersHandler(Packet packet, Simulator simulator)
        {
            GroupMembersReplyPacket members = (GroupMembersReplyPacket)packet;
            Dictionary<LLUUID, GroupMember> groupMemberCache = null;

            lock (GroupMembersCaches)
            {
                // If nothing is registered to receive this RequestID drop the data
                if (GroupMembersCaches.ContainsKey(members.GroupData.RequestID))
                {
                    groupMemberCache = GroupMembersCaches[members.GroupData.RequestID];

                    foreach (GroupMembersReplyPacket.MemberDataBlock block in members.MemberData)
                    {
                        GroupMember groupMember = new GroupMember(block.AgentID);

                        groupMember.Contribution = block.Contribution;
                        groupMember.IsOwner = block.IsOwner;
                        groupMember.OnlineStatus = Helpers.FieldToString(block.OnlineStatus);
                        groupMember.Powers = block.AgentPowers;
                        groupMember.Title = Helpers.FieldToString(block.Title);

                        groupMemberCache[block.AgentID] = groupMember;
                    }
                }
            }

            // Check if we've received all the group members that are showing up
            if (groupMemberCache != null && groupMemberCache.Count >= members.GroupData.MemberCount)
            {
                GroupMembersCallbacks[members.GroupData.GroupID](groupMemberCache);
            }
        }

        private void GroupRoleDataHandler(Packet packet, Simulator simulator)
        {
            GroupRoleDataReplyPacket roles = (GroupRoleDataReplyPacket)packet;
            Dictionary<LLUUID, GroupRole> groupRoleCache = null;

            lock (GroupRolesCaches)
            {
                // If nothing is registered to receive this RequestID drop the data
                if (GroupRolesCaches.ContainsKey(roles.GroupData.RequestID))
                {
                    groupRoleCache = GroupRolesCaches[roles.GroupData.RequestID];

                    foreach (GroupRoleDataReplyPacket.RoleDataBlock block in roles.RoleData)
                    {
                        GroupRole groupRole = new GroupRole(block.RoleID);

                        groupRole.Description = Helpers.FieldToString(block.Description);
                        groupRole.Name = Helpers.FieldToString(block.Name);
                        groupRole.Powers = block.Powers;
                        groupRole.Title = Helpers.FieldToString(block.Title);

                        groupRoleCache[block.RoleID] = groupRole;
                    }
                }
            }

            // Check if we've received all the group members that are showing up
            if (groupRoleCache != null && groupRoleCache.Count >= roles.GroupData.RoleCount)
            {
                GroupRolesCallbacks[roles.GroupData.GroupID](groupRoleCache);
            }
        }

        private void GroupRoleMembersHandler(Packet packet, Simulator simulator)
        {
            GroupRoleMembersReplyPacket members = (GroupRoleMembersReplyPacket)packet;
        }

        private void GroupActiveProposalItemHandler(Packet packet, Simulator simulator)
        {
            ;
        }

        private void GroupVoteHistoryItemHandler(Packet packet, Simulator simulator)
        {
            ;
        }

        private void GroupAccountSummaryHandler(Packet packet, Simulator simulator)
        {
            ;
        }

        private void GroupAccountDetailsHandler(Packet packet, Simulator simulator)
        {
            ;
        }

        private void GroupAccountTransactionsHandler(Packet packet, Simulator simulator)
        {
            ;
        }
    }
}
