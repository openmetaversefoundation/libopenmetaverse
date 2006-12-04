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
    /// <summary>
    /// 
    /// </summary>
    public class GroupMember
    {
        /// <summary></summary>
        public LLUUID ID;
        /// <summary></summary>
        public int Contribution;
        /// <summary></summary>
        public string OnlineStatus;
        /// <summary></summary>
        public ulong Powers;
        /// <summary></summary>
        public string Title;
        /// <summary></summary>
        public bool IsOwner;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public GroupMember(LLUUID id)
        {
            ID = id;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GroupRole
    {
        /// <summary></summary>
        public LLUUID ID;
        /// <summary></summary>
        public string Name;
        /// <summary></summary>
        public string Title;
        /// <summary></summary>
        public string Description;
        /// <summary></summary>
        public ulong Powers;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public GroupRole(LLUUID id)
        {
            ID = id;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GroupTitle
    {
        /// <summary></summary>
        public string Title;
        /// <summary></summary>
        public bool Selected;
    }

    /// <summary>
    /// Represents a group in Second Life
    /// </summary>
    public class Group
    {
        /// <summary></summary>
        public LLUUID ID;
        /// <summary></summary>
        public LLUUID InsigniaID;
        /// <summary></summary>
        public LLUUID FounderID;
        /// <summary></summary>
        public LLUUID OwnerRole;
        /// <summary></summary>
        public string Name;
        /// <summary></summary>
        public string Charter;
        /// <summary></summary>
        public string MemberTitle;
        /// <summary></summary>
        public bool OpenEnrollment;
        /// <summary></summary>
        public bool ShowInList;
        /// <summary></summary>
        public ulong Powers;
        /// <summary></summary>
        public bool AcceptNotices;
        /// <summary></summary>
        public bool AllowPublish;
        /// <summary></summary>
        public bool MaturePublish;
        /// <summary></summary>
        public int MembershipFee;
        /// <summary></summary>
        public int Money;
        /// <summary></summary>
        public int Contribution;
        /// <summary></summary>
        public int GroupMembershipCount;
        /// <summary></summary>
        public int GroupRolesCount;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public Group(LLUUID id)
        {
            ID = id;
            InsigniaID = LLUUID.Zero;
        }

        /// <summary>
        /// Returns the name of the group
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GroupProfile
    {
        /// <summary></summary>
        public LLUUID ID;
        /// <summary></summary>
        public LLUUID InsigniaID;
        /// <summary></summary>
        public LLUUID FounderID;
        /// <summary></summary>
        public LLUUID OwnerRole;
        /// <summary></summary>
        public string Name;
        /// <summary></summary>
        public string Charter;
        /// <summary></summary>
        public string MemberTitle;
        /// <summary></summary>
        public bool OpenEnrollment;
        /// <summary></summary>
        public bool ShowInList;
        /// <summary></summary>
        public ulong Powers;
        /// <summary></summary>
        public bool AcceptNotices;
        /// <summary></summary>
        public bool AllowPublish;
        /// <summary></summary>
        public bool MaturePublish;
        /// <summary></summary>
        public int MembershipFee;
        /// <summary></summary>
        public int Money;
        /// <summary></summary>
        public int Contribution;
        /// <summary></summary>
        public int GroupMembershipCount;
        /// <summary></summary>
        public int GroupRolesCount;
    }

    /// <summary>
    /// 
    /// </summary>
    public class Vote
    {
        /// <summary></summary>
        public LLUUID Candidate;
        /// <summary></summary>
        public string VoteString;
        /// <summary></summary>
        public int NumVotes;
    }

    /// <summary>
    /// 
    /// </summary>
    public class GroupAccountSummary
    {
        /// <summary></summary>
        public int IntervalDays;
        /// <summary></summary>
        public int CurrentInterval;
        /// <summary></summary>
        public string StartDate;
        /// <summary></summary>
        public int Balance;
        /// <summary></summary>
        public int TotalCredits;
        /// <summary></summary>
        public int TotalDebits;
        /// <summary></summary>
        public int ObjectTaxCurrent;
        /// <summary></summary>
        public int LightTaxCurrent;
        /// <summary></summary>
        public int LandTaxCurrent;
        /// <summary></summary>
        public int GroupTaxCurrent;
        /// <summary></summary>
        public int ParcelDirFeeCurrent;
        /// <summary></summary>
        public int ObjectTaxEstimate;
        /// <summary></summary>
        public int LightTaxEstimate;
        /// <summary></summary>
        public int LandTaxEstimate;
        /// <summary></summary>
        public int GroupTaxEstimate;
        /// <summary></summary>
        public int ParcelDirFeeEstimate;
        /// <summary></summary>
        public int NonExemptMembers;
        /// <summary></summary>
        public string LastTaxDate;
        /// <summary></summary>
        public string TaxDate;
    }

    /// <summary>
    /// 
    /// </summary>
    public class GroupAccountDetails
    {
        /// <summary></summary>
        public int IntervalDays;
        /// <summary></summary>
        public int CurrentInterval;
        /// <summary></summary>
        public string StartDate;
        /// <summary>A list of description/amount pairs making up the account 
        /// history</summary>
        public List<KeyValuePair<string, int>> HistoryItems;
    }

    /// <summary>
    /// 
    /// </summary>
    public class GroupAccountTransactions
    {
        /// <summary></summary>
        public int IntervalDays;
        /// <summary></summary>
        public int CurrentInterval;
        /// <summary></summary>
        public string StartDate;
        /// <summary>List of all the transactions for this group</summary>
        public List<Transaction> Transactions;
    }

    /// <summary>
    /// A single transaction made by a group
    /// </summary>
    public class Transaction
    {
        /// <summary></summary>
        public string Time;
        /// <summary></summary>
        public string User;
        /// <summary></summary>
        public int Type;
        /// <summary></summary>
        public string Item;
        /// <summary></summary>
        public int Amount;
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
        public delegate void CurrentGroupsCallback(Dictionary<LLUUID, Group> groups);
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
        /// Callback for a pairing of roles to members
        /// </summary>
        /// <param name="rolesMembers"></param>
        public delegate void GroupRolesMembersCallback(List<KeyValuePair<LLUUID, LLUUID>> rolesMembers);
        /// <summary>
        /// Callback for the title list of a group
        /// </summary>
        /// <param name="titles"></param>
        public delegate void GroupTitlesCallback(Dictionary<LLUUID, GroupTitle> titles);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="summary"></param>
        public delegate void GroupAccountSummaryCallback(GroupAccountSummary summary);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="details"></param>
        public delegate void GroupAccountDetailsCallback(GroupAccountDetails details);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactions"></param>
        public delegate void GroupAccountTransactionsCallback(GroupAccountTransactions transactions);

        private SecondLife Client;
        // No need for concurrency with the current group list request
        private CurrentGroupsCallback OnCurrentGroups;
        private Dictionary<LLUUID, GroupProfileCallback> GroupProfileCallbacks;
        private Dictionary<LLUUID, GroupMembersCallback> GroupMembersCallbacks;
        private Dictionary<LLUUID, GroupRolesCallback> GroupRolesCallbacks;
        private Dictionary<LLUUID, GroupRolesMembersCallback> GroupRolesMembersCallbacks;
        private Dictionary<LLUUID, GroupTitlesCallback> GroupTitlesCallbacks;
        private Dictionary<LLUUID, GroupAccountSummaryCallback> GroupAccountSummaryCallbacks;
        private Dictionary<LLUUID, GroupAccountDetailsCallback> GroupAccountDetailsCallbacks;
        private Dictionary<LLUUID, GroupAccountTransactionsCallback> GroupAccountTransactionsCallbacks;
        /// <summary>A list of all the lists of group members, indexed by the request ID</summary>
        private Dictionary<LLUUID, Dictionary<LLUUID, GroupMember>> GroupMembersCaches;
        /// <summary>A list of all the lists of group roles, indexed by the request ID</summary>
        private Dictionary<LLUUID, Dictionary<LLUUID, GroupRole>> GroupRolesCaches;
        /// <summary>A list of all the role to member mappings</summary>
        private Dictionary<LLUUID, List<KeyValuePair<LLUUID, LLUUID>>> GroupRolesMembersCaches;

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
            GroupRolesMembersCallbacks = new Dictionary<LLUUID, GroupRolesMembersCallback>();
            GroupTitlesCallbacks = new Dictionary<LLUUID, GroupTitlesCallback>();
            GroupAccountSummaryCallbacks = new Dictionary<LLUUID, GroupAccountSummaryCallback>();
            GroupAccountDetailsCallbacks = new Dictionary<LLUUID, GroupAccountDetailsCallback>();
            GroupAccountTransactionsCallbacks = new Dictionary<LLUUID, GroupAccountTransactionsCallback>();

            GroupMembersCaches = new Dictionary<LLUUID, Dictionary<LLUUID, GroupMember>>();
            GroupRolesCaches = new Dictionary<LLUUID, Dictionary<LLUUID, GroupRole>>();
            GroupRolesMembersCaches = new Dictionary<LLUUID, List<KeyValuePair<LLUUID, LLUUID>>>();

            Client.Network.RegisterCallback(PacketType.AgentGroupDataUpdate, new NetworkManager.PacketCallback(GroupDataHandler));
            Client.Network.RegisterCallback(PacketType.GroupTitlesReply, new NetworkManager.PacketCallback(GroupTitlesHandler));
            Client.Network.RegisterCallback(PacketType.GroupProfileReply, new NetworkManager.PacketCallback(GroupProfileHandler));
            Client.Network.RegisterCallback(PacketType.GroupMembersReply, new NetworkManager.PacketCallback(GroupMembersHandler));
            Client.Network.RegisterCallback(PacketType.GroupRoleDataReply, new NetworkManager.PacketCallback(GroupRoleDataHandler));
            Client.Network.RegisterCallback(PacketType.GroupRoleMembersReply, new NetworkManager.PacketCallback(GroupRoleMembersHandler));
            Client.Network.RegisterCallback(PacketType.GroupActiveProposalItemReply, new NetworkManager.PacketCallback(GroupActiveProposalItemHandler));
            Client.Network.RegisterCallback(PacketType.GroupVoteHistoryItemReply, new NetworkManager.PacketCallback(GroupVoteHistoryItemHandler));
            Client.Network.RegisterCallback(PacketType.GroupAccountSummaryReply, new NetworkManager.PacketCallback(GroupAccountSummaryHandler));
            Client.Network.RegisterCallback(PacketType.GroupAccountDetailsReply, new NetworkManager.PacketCallback(GroupAccountDetailsHandler));
            Client.Network.RegisterCallback(PacketType.GroupAccountTransactionsReply, new NetworkManager.PacketCallback(GroupAccountTransactionsHandler));
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
            LLUUID requestID = LLUUID.Random();

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <param name="grc"></param>
        public void BeginGetGroupRoles(LLUUID group, GroupRolesCallback grc)
        {
            LLUUID requestID = LLUUID.Random();

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <param name="gtc"></param>
        public void BeginGetGroupTitles(LLUUID group, GroupTitlesCallback gtc)
        {
            LLUUID requestID = LLUUID.Random();

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
            List<KeyValuePair<LLUUID, LLUUID>> groupRoleMemberCache = null;

            lock (GroupRolesMembersCaches)
            {
                // If nothing is registered to receive this RequestID drop the data
                if (GroupRolesMembersCaches.ContainsKey(members.AgentData.RequestID))
                {
                    groupRoleMemberCache = GroupRolesMembersCaches[members.AgentData.RequestID];
                    int i = 0;

                    foreach (GroupRoleMembersReplyPacket.MemberDataBlock block in members.MemberData)
                    {
                        KeyValuePair<LLUUID, LLUUID> rolemember = 
                            new KeyValuePair<LLUUID, LLUUID>(block.RoleID, block.MemberID);

                        groupRoleMemberCache[i++] = rolemember;
                    }
                }
            }

            // Check if we've received all the pairs that are showing up
            if (groupRoleMemberCache != null && groupRoleMemberCache.Count >= members.AgentData.TotalPairs)
            {
                GroupRolesMembersCallbacks[members.AgentData.GroupID](groupRoleMemberCache);
            }
        }

        private void GroupActiveProposalItemHandler(Packet packet, Simulator simulator)
        {
            //GroupActiveProposalItemReplyPacket proposal = (GroupActiveProposalItemReplyPacket)packet;

            // TODO: Create a proposal class to represent the fields in a proposal item
        }

        private void GroupVoteHistoryItemHandler(Packet packet, Simulator simulator)
        {
            //GroupVoteHistoryItemReplyPacket history = (GroupVoteHistoryItemReplyPacket)packet;

            // TODO: This was broken in the official viewer when I was last trying to work  on it
        }

        private void GroupAccountSummaryHandler(Packet packet, Simulator simulator)
        {
            GroupAccountSummaryReplyPacket summary = (GroupAccountSummaryReplyPacket)packet;

            if (GroupAccountSummaryCallbacks.ContainsKey(summary.AgentData.GroupID))
            {
                GroupAccountSummary account = new GroupAccountSummary();

                account.Balance = summary.MoneyData.Balance;
                account.CurrentInterval = summary.MoneyData.CurrentInterval;
                account.GroupTaxCurrent = summary.MoneyData.GroupTaxCurrent;
                account.GroupTaxEstimate = summary.MoneyData.GroupTaxEstimate;
                account.IntervalDays = summary.MoneyData.IntervalDays;
                account.LandTaxCurrent = summary.MoneyData.LandTaxCurrent;
                account.LandTaxEstimate = summary.MoneyData.LandTaxEstimate;
                account.LastTaxDate = Helpers.FieldToString(summary.MoneyData.LastTaxDate);
                account.LightTaxCurrent = summary.MoneyData.LightTaxCurrent;
                account.LightTaxEstimate = summary.MoneyData.LightTaxEstimate;
                account.NonExemptMembers = summary.MoneyData.NonExemptMembers;
                account.ObjectTaxCurrent = summary.MoneyData.ObjectTaxCurrent;
                account.ObjectTaxEstimate = summary.MoneyData.ObjectTaxEstimate;
                account.ParcelDirFeeCurrent = summary.MoneyData.ParcelDirFeeCurrent;
                account.ParcelDirFeeEstimate = summary.MoneyData.ParcelDirFeeEstimate;
                account.StartDate = Helpers.FieldToString(summary.MoneyData.StartDate);
                account.TaxDate = Helpers.FieldToString(summary.MoneyData.TaxDate);
                account.TotalCredits = summary.MoneyData.TotalCredits;
                account.TotalDebits = summary.MoneyData.TotalDebits;

                GroupAccountSummaryCallbacks[summary.AgentData.GroupID](account);
            }
        }

        private void GroupAccountDetailsHandler(Packet packet, Simulator simulator)
        {
            GroupAccountDetailsReplyPacket details = (GroupAccountDetailsReplyPacket)packet;

            if (GroupAccountDetailsCallbacks.ContainsKey(details.AgentData.GroupID))
            {
                GroupAccountDetails account = new GroupAccountDetails();

                account.CurrentInterval = details.MoneyData.CurrentInterval;
                account.IntervalDays = details.MoneyData.IntervalDays;
                account.StartDate = Helpers.FieldToString(details.MoneyData.StartDate);

                account.HistoryItems = new List<KeyValuePair<string, int>>();

                foreach (GroupAccountDetailsReplyPacket.HistoryDataBlock block in details.HistoryData)
                {
                    KeyValuePair<string, int> item = 
                        new KeyValuePair<string, int>(Helpers.FieldToString(block.Description), block.Amount);

                    account.HistoryItems.Add(item);
                }

                GroupAccountDetailsCallbacks[details.AgentData.GroupID](account);
            }
        }

        private void GroupAccountTransactionsHandler(Packet packet, Simulator simulator)
        {
            //GroupAccountTransactionsReplyPacket transactions = (GroupAccountTransactionsReplyPacket)packet;

            // TODO: This one is slightly different than the previous two

            //if (GroupAccountTransactionsCallbacks.ContainsKey(transactions.AgentData.GroupID))
            //{
            //    GroupAccountTransactions account = new GroupAccountTransactions();

            //    ;

            //    GroupAccountTransactionsCallbacks[transactions.AgentData.GroupID](account);
            //}
        }
    }
}
