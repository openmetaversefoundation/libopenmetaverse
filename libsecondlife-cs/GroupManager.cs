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
        /// <summary>Contains all the group members belonging to this role</summary>
        public Dictionary<LLUUID,GroupMember> Members;

        public GroupRole(LLUUID id)
        {
            ID = id;
            Members = new Dictionary<LLUUID,GroupMember>();
        }
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
        /// <summary></summary>
        public Dictionary<LLUUID,LLUUID> Titles;
        /// <summary>List of all the roles in this group</summary>
        public Dictionary<LLUUID,GroupRole> Roles;
        /// <summary>List of all the members in this group</summary>
        public Dictionary<LLUUID,GroupMember> Members;
        /// <summary>Used for internal state tracking</summary>
        public LLUUID TitlesRequestID;
        /// <summary>Used for internal state tracking</summary>
        public LLUUID RolesRequestID;
        /// <summary>Used for internal state tracking</summary>
        public LLUUID MembersRequestID;

        public Group(LLUUID id)
        {
            ID = id;
            InsigniaID = new LLUUID();

            Titles = new Dictionary<LLUUID,LLUUID>();
            Roles = new Dictionary<LLUUID,GroupRole>();
            Members = new Dictionary<LLUUID,GroupMember>();

            TitlesRequestID = new LLUUID();
            RolesRequestID = new LLUUID();
            MembersRequestID = new LLUUID();
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class GroupManager
    {
        /// <summary>
        /// Called whenever the group membership list is updated
        /// </summary>
        /// <param name="groups"></param>
        public delegate void GroupsUpdatedCallback();

        /// <summary></summary>
        public Dictionary<LLUUID,Group> Groups;
        private Mutex GroupsMutex;

        /// <summary>Called whenever the group membership list is updated</summary>
        public event GroupsUpdatedCallback OnGroupsUpdated;

        private SecondLife Client;
        
        public GroupManager(SecondLife client)
        {
            Groups = new Dictionary<LLUUID,Group>();
            GroupsMutex = new Mutex(false, "GroupsMutex");
            Client = client;

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

        private void GroupDataHandler(Packet packet, Simulator simulator)
        {
            // FIXME: Add an additional list, such as MyGroups that will distinguish the groups we are
            // actually in versus the ones we are just collecting data on. Or should there be a better 
            // way to temporarily collect data for transient requests?
            AgentGroupDataUpdatePacket update = (AgentGroupDataUpdatePacket)packet;

            #region GroupsMutex
            GroupsMutex.WaitOne();

            // Flush out the groups list
            Groups.Clear();

            foreach (AgentGroupDataUpdatePacket.GroupDataBlock block in update.GroupData)
            {
                Group group = new Group(block.GroupID);

                group.InsigniaID = block.GroupInsigniaID;
                group.Name = Helpers.FieldToString(block.GroupName);
                group.Powers = block.GroupPowers;
                group.Contribution = block.Contribution;
                group.AcceptNotices = block.AcceptNotices;

                Groups[group.ID] = group;
            }

            GroupsMutex.ReleaseMutex();
            #endregion GroupsMutex

            if (OnGroupsUpdated != null)
            {
                OnGroupsUpdated();
            }
        }

        private void GroupTitlesHandler(Packet packet, Simulator simulator)
        {
            Group thisGroup;
            GroupTitlesReplyPacket titles = (GroupTitlesReplyPacket)packet;

            #region GroupsMutex
            GroupsMutex.WaitOne();

            // Attempt to locate the group that these titles belong to
            if (Groups.ContainsKey(titles.AgentData.GroupID))
            {
                thisGroup = Groups[titles.AgentData.GroupID];
            }
            else
            {
                // Avoid throwing this data away by creating a new group
                thisGroup = new Group(titles.AgentData.GroupID);
                thisGroup.TitlesRequestID = titles.AgentData.RequestID;
                Groups[titles.AgentData.GroupID] = thisGroup;
            }

            if (titles.AgentData.RequestID == thisGroup.TitlesRequestID)
            {
                foreach (GroupTitlesReplyPacket.GroupDataBlock block in titles.GroupData)
                {
                    thisGroup.Titles[Helpers.FieldToString(block.Title)] = block.RoleID;
                    // TODO: Do something with block.Selected
                }
            }

            GroupsMutex.ReleaseMutex();
            #endregion
        }

        private void GroupProfileHandler(Packet packet, Simulator simulator)
        {
            Group thisGroup;
            GroupProfileReplyPacket profile = (GroupProfileReplyPacket)packet;
            
            #region GroupsMutex
            GroupsMutex.WaitOne();

            // Attempt to locate the group that these titles belong to
            if (Groups.ContainsKey(profile.GroupData.GroupID))
            {
                thisGroup = Groups[profile.GroupData.GroupID];
            }
            else
            {
                thisGroup = new Group(profile.GroupData.GroupID);
                Groups[profile.GroupData.GroupID] = thisGroup;
            }

            thisGroup.AllowPublish = profile.GroupData.AllowPublish;
            thisGroup.Charter = Helpers.FieldToString(profile.GroupData.Charter);
            thisGroup.FounderID = profile.GroupData.FounderID;
            thisGroup.GroupMembershipCount = profile.GroupData.GroupMembershipCount;
            thisGroup.GroupRolesCount = profile.GroupData.GroupRolesCount;
            thisGroup.InsigniaID = profile.GroupData.InsigniaID;
            thisGroup.MaturePublish = profile.GroupData.MaturePublish;
            thisGroup.MembershipFee = profile.GroupData.MembershipFee;
            thisGroup.MemberTitle = Helpers.FieldToString(profile.GroupData.MemberTitle);
            thisGroup.Money = profile.GroupData.Money;
            thisGroup.Name = Helpers.FieldToString(profile.GroupData.Name);
            thisGroup.OpenEnrollment = profile.GroupData.OpenEnrollment;
            thisGroup.OwnerRole = profile.GroupData.OwnerRole;
            thisGroup.Powers = profile.GroupData.PowersMask;
            thisGroup.ShowInList = profile.GroupData.ShowInList;

            GroupsMutex.ReleaseMutex();
            #endregion
        }

        private void GroupMembersHandler(Packet packet, Simulator simulator)
        {
            Group thisGroup;
            GroupMembersReplyPacket members = (GroupMembersReplyPacket)packet;

            #region GroupsMutex
            GroupsMutex.WaitOne();

            if (Groups.ContainsKey(members.GroupData.GroupID))
            {
                thisGroup = Groups[members.GroupData.GroupID];
            }
            else
            {
                thisGroup = new Group(members.GroupData.GroupID);
                Groups[members.GroupData.GroupID] = thisGroup;
            }

            if (members.GroupData.RequestID == thisGroup.MembersRequestID)
            {
                foreach (GroupMembersReplyPacket.MemberDataBlock block in members.MemberData)
                {
                    GroupMember member = new GroupMember(block.AgentID);

                    member.Contribution = block.Contribution;
                    member.ID = block.AgentID;
                    member.IsOwner = block.IsOwner;
                    member.OnlineStatus = Helpers.FieldToString(block.OnlineStatus);
                    member.Powers = block.AgentPowers;
                    member.Title = Helpers.FieldToString(block.Title);

                    thisGroup.Members[member.ID] = member;
                }
            }

            GroupsMutex.ReleaseMutex();
            #endregion
        }

        private void GroupRoleDataHandler(Packet packet, Simulator simulator)
        {
            Group thisGroup;
            GroupRole thisRole;
            GroupRoleDataReplyPacket roles = (GroupRoleDataReplyPacket)packet;

            #region GroupsMutex
            GroupsMutex.WaitOne();

            if (Groups.ContainsKey(roles.GroupData.GroupID))
            {
                thisGroup = Groups[roles.GroupData.GroupID];
            }
            else
            {
                thisGroup = new Group(roles.GroupData.GroupID);
                Groups[roles.GroupData.GroupID] = thisGroup;
            }

            foreach (GroupRoleDataReplyPacket.RoleDataBlock block in roles.RoleData)
            {
                if (thisGroup.Roles.ContainsKey(block.RoleID))
                {
                    thisRole = thisGroup.Roles[block.RoleID];
                }
                else
                {
                    thisRole = new GroupRole(block.RoleID);
                    thisGroup.Roles[block.RoleID] = thisRole;
                }

                thisRole.Description = Helpers.FieldToString(block.Description);
                thisRole.Name = Helpers.FieldToString(block.Name);
                thisRole.Powers = block.Powers;
                thisRole.Title = Helpers.FieldToString(block.Title);
            }

            GroupsMutex.ReleaseMutex();
            #endregion
        }

        private void GroupRoleMembersHandler(Packet packet, Simulator simulator)
        {
            Group thisGroup;
            GroupRole thisRole;
            GroupMember thisMember;
            GroupRoleMembersReplyPacket members = (GroupRoleMembersReplyPacket)packet;

            #region GroupsMutex
            GroupsMutex.WaitOne();

            if (Groups.ContainsKey(members.AgentData.GroupID))
            {
                thisGroup = Groups[members.AgentData.GroupID];
            }
            else
            {
                thisGroup = new Group(members.AgentData.GroupID);
                Groups[members.AgentData.GroupID] = thisGroup;
            }

            foreach (GroupRoleMembersReplyPacket.MemberDataBlock block in members.MemberData)
            {
                if (thisGroup.Roles.ContainsKey(block.RoleID))
                {
                    thisRole = thisGroup.Roles[block.RoleID];
                }
                else
                {
                    thisRole = new GroupRole(block.RoleID);
                    thisGroup.Roles[block.RoleID] = thisRole;
                }

                if (thisGroup.Members.ContainsKey(block.MemberID))
                {
                    thisMember = thisGroup.Members[block.MemberID];
                }
                else
                {
                    thisMember = new GroupMember(block.MemberID);
                    thisGroup.Members[block.MemberID] = thisMember;
                }

                // Add this member to this block if it doesn't already exist there
                if (!thisRole.Members.ContainsKey(block.MemberID))
                {
                    thisRole.Members[block.MemberID] = thisMember;
                }
            }

            GroupsMutex.ReleaseMutex();
            #endregion
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
