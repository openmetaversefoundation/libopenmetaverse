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
using System.Collections;
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
    }

    public class GroupRole
    {
        public LLUUID ID;
        public string Name;
        public string Title;
        public string Description;
        public ulong Powers;
        ArrayList Members;
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
        public Hashtable Titles;
        public Hashtable Roles;
        public Hashtable Members;

        public Group(LLUUID id)
        {
            ID = id;
            InsigniaID = new LLUUID();

            Titles = new Hashtable();
            Roles = new Hashtable();
            Members = new Hashtable();
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
        public Hashtable Groups;
        private Mutex GroupsMutex;

        /// <summary>Called whenever the group membership list is updated</summary>
        public event GroupsUpdatedCallback OnGroupsUpdated;

        private SecondLife Client;
        
        public GroupManager(SecondLife client)
        {
            Groups = new Hashtable();
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
            AgentGroupDataUpdatePacket update = (AgentGroupDataUpdatePacket)packet;

            #region GroupsMutex
            GroupsMutex.WaitOne();

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
                thisGroup = (Group)Groups[titles.AgentData.GroupID];
            }
            else
            {
                thisGroup = new Group(titles.AgentData.GroupID);
                Groups[titles.AgentData.GroupID] = thisGroup;
            }

            foreach (GroupTitlesReplyPacket.GroupDataBlock block in titles.GroupData)
            {
                thisGroup.Titles[Helpers.FieldToString(block.Title)] = block.RoleID;

                // TODO: Do something with block.Selected
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
                thisGroup = (Group)Groups[profile.GroupData.GroupID];
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
                thisGroup = (Group)Groups[members.GroupData.GroupID];
            }
            else
            {
                thisGroup = new Group(members.GroupData.GroupID);
                Groups[members.GroupData.GroupID] = thisGroup;
            }

            foreach (GroupMembersReplyPacket.MemberDataBlock block in members.MemberData)
            {
                GroupMember member = new GroupMember();

                member.Contribution = block.Contribution;
                member.ID = block.AgentID;
                member.IsOwner = block.IsOwner;
                member.OnlineStatus = Helpers.FieldToString(block.OnlineStatus);
                member.Powers = block.AgentPowers;
                member.Title = Helpers.FieldToString(block.Title);

                thisGroup.Members[member.ID] = member;
            }

            GroupsMutex.ReleaseMutex();
            #endregion
        }

        private void GroupRoleDataHandler(Packet packet, Simulator simulator)
        {
            ;
        }

        private void GroupRoleMembersHandler(Packet packet, Simulator simulator)
        {
            ;
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
