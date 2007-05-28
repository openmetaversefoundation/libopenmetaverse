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
using libsecondlife.Packets;

namespace libsecondlife
{
    #region Structs

    /// <summary>
    /// Avatar group management
    /// </summary>
    public struct GroupMember
    {
        /// <summary>Key of Group Member</summary>
        public LLUUID ID;
        /// <summary>Total land contribution</summary>
        public int Contribution;
        /// <summary>Online status information</summary>
        public string OnlineStatus;
        /// <summary>Abilities that the Group Member has</summary>
        public ulong Powers;
        /// <summary>Current group title</summary>
        public string Title;
        /// <summary>Is a group owner</summary>
        public bool IsOwner;
    }

    /// <summary>
    /// Role manager for a group
    /// </summary>
    public struct GroupRole
    {
        /// <summary>Key of Role</summary>
        public LLUUID ID;
        /// <summary>Name of Role</summary>
        public string Name;
        /// <summary>Group Title associated with Role</summary>
        public string Title;
        /// <summary>Description of Role</summary>
        public string Description;
        /// <summary>Abilities Associated with Role</summary>
        public ulong Powers;
        /// <summary>
        /// Returns the role's title
        /// </summary>
        /// <returns>The role's title</returns>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Class to represent Group Title
    /// </summary>
    public struct GroupTitle
    {
        /// <summary>Group Title</summary>
        public string Title;
        /// <summary>Whether title is Active</summary>
        public bool Selected;
    }

    /// <summary>
    /// Represents a group in Second Life
    /// </summary>
    public struct Group
    {
        /// <summary>Key of Group</summary>
        public LLUUID ID;
        /// <summary>Key of Group Insignia</summary>
        public LLUUID InsigniaID;
        /// <summary>Key of Group Founder</summary>
        public LLUUID FounderID;
        /// <summary>Key of Group Role for Owners</summary>
        public LLUUID OwnerRole;
        /// <summary>Name of Group</summary>
        public string Name;
        /// <summary>Text of Group Charter</summary>
        public string Charter;
        /// <summary>Title of "everyone" role</summary>
        public string MemberTitle;
        /// <summary>Is the group open for enrolement to everyone</summary>
        public bool OpenEnrollment;
        /// <summary>Will group show up in search</summary>
        public bool ShowInList;
        /// <summary></summary>
        public ulong Powers;
        /// <summary></summary>
        public bool AcceptNotices;
        /// <summary></summary>
        public bool AllowPublish;
        /// <summary>Is the group Mature</summary>
        public bool MaturePublish;
        /// <summary>Cost of group membership</summary>
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
        /// Returns the name of the group
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Profile of a group
    /// </summary>
    public struct GroupProfile
    {
        /// <summary></summary>
        public LLUUID ID;
        /// <summary>Key of Group Insignia</summary>
        public LLUUID InsigniaID;
        /// <summary>Key of Group Founder</summary>
        public LLUUID FounderID;
        /// <summary>Key of Group Role for Owners</summary>
        public LLUUID OwnerRole;
        /// <summary>Name of Group</summary>
        public string Name;
        /// <summary>Text of Group Charter</summary>
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
    /// A group Vote
    /// </summary>
    public struct Vote
    {
        /// <summary>Key of Avatar who created Vote</summary>
        public LLUUID Candidate;
        /// <summary>Text of the Vote proposal</summary>
        public string VoteString;
        /// <summary>Total number of votes</summary>
        public int NumVotes;
    }

    public struct GroupProposal
    {

        public string VoteText;

        public int Quorum;

        public float Majority;

        public int Duration;
    }

    /// <summary>
    /// 
    /// </summary>
    public struct GroupAccountSummary
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
    public struct GroupAccountDetails
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
    public struct GroupAccountTransactions
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
    public struct Transaction
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
    /// Struct representing a group notice
    /// </summary>
    public struct GroupNotice
    {
        /// <summary></summary>
        public string Subject;
        /// <summary></summary>
        public string Message;
        /// <summary></summary>
        public LLUUID AttachmentID;
        /// <summary></summary>
        public LLUUID OwnerID;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] SerializeAttachment()
        {
            if (OwnerID == null || OwnerID == LLUUID.Zero || AttachmentID == null || AttachmentID == LLUUID.Zero)
                return new byte[0];
            //I guess this is how this works, no gaurentees
            string lsd = "<llsd><item_id>" + AttachmentID.ToString() + "</item_id><owner_id>"
                + OwnerID.ToString() + "</owner_id></llsd>";
            return Helpers.StringToField(lsd);
        }
    }

    #endregion Structs

    #region Enums

    /// <summary>
    /// Role update flags
    /// </summary>
    public enum GroupRoleUpdate : uint
    {
        NoUpdate,
        UpdateData,
        UpdatePowers,
        UpdateAll,
        Create,
        Delete
    }

    /// <summary>
    /// Group role powers flags
    /// </summary>
    [Flags]
    public enum GroupRolePowers : long
    {
        None = 0,
        Invite = 1 << 1,
        Eject = 1 << 2,
        ChangeOptions = 1 << 3,
        CreateRole = 1 << 4,
        DeleteRole = 1 << 5,
        RoleProperties = 1 << 6,
        AssignMemberLimited = 1 << 7,
        AssignMember = 1 << 8,
        RemoveMember = 1 << 9,
        ChangeActions = 1 << 10,
        ChangeIdentity = 1 << 11,
        LandDeed = 1 << 12,
        LandRelease = 1 << 13,
        LandSetSale = 1 << 14,
        LandDevideJoin = 1 << 15,
        FindPlaces = 1 << 17,
        LandChangeIdentity = 1 << 18,
        SetLandingPoint = 1 << 19,
        ChangeMedia = 1 << 20,
        LandEdit = 1 << 21,
        LandOptions = 1 << 22,
        AllowEditLand = 1 << 23,
        AllowFly = 1 << 24,
        AllowRez = 1 << 25,
        AllowLandmark = 1 << 26,
        AllowSetHome = 1 << 28,
        LandManageAllowed = 1 << 29,
        LandManageBanned = 1 << 30,
        LandManagePasses = 1 << 31,
        LandEjectAndFreeze = 1 << 32,
        ReturnGroupOwned = 1 << 48,
        ReturnGroupSet = 1 << 33,
        ReturnNonGroup = 1 << 34,
        LandGardening = 1 << 35,
        DeedObject = 1 << 36,
        ObjectManipulate = 1 << 38,
        ObjectSetForSale = 1 << 39,
        Accountable = 1 << 40,
        SendNotices = 1 << 42,
        ReceiveNotices = 1 << 43,
        StartProposal = 1 << 44,
        VoteOnProposal = 1 << 45
    }

    #endregion Enums

    /// <summary>
    /// Handles all network traffic related to reading and writing group
    /// information
    /// </summary>
    public class GroupManager
    {
        #region Delegates

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

        #endregion Delegates

        #region Events

        /// <summary></summary>
        public event CurrentGroupsCallback OnCurrentGroups;
        /// <summary></summary>
        public event GroupProfileCallback OnGroupProfile;
        /// <summary></summary>
        public event GroupMembersCallback OnGroupMembers;
        /// <summary></summary>
        public event GroupRolesCallback OnGroupRoles;
        /// <summary></summary>
        public event GroupRolesMembersCallback OnGroupRolesMembers;
        /// <summary></summary>
        public event GroupTitlesCallback OnGroupTitles;
        /// <summary></summary>
        public event GroupAccountSummaryCallback OnGroupAccountSummary;
        /// <summary></summary>
        public event GroupAccountDetailsCallback OnGroupAccountDetails;
        /// <summary></summary>
        //public event GroupAccountTransactionsCallback OnGroupAccountTransactions;

        #endregion Events


        private SecondLife Client;
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
        public void BeginGetCurrentGroups()
        {
            AgentDataUpdateRequestPacket request = new AgentDataUpdateRequestPacket();

            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;

            Client.Network.SendPacket(request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        public void BeginGetGroupProfile(LLUUID group)
        {
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
        public void BeginGetGroupMembers(LLUUID group)
        {
            LLUUID requestID = LLUUID.Random();
            lock (GroupMembersCaches) GroupMembersCaches[requestID] = new Dictionary<LLUUID, GroupMember>();

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
        public void BeginGetGroupRoles(LLUUID group)
        {
            LLUUID requestID = LLUUID.Random();
            lock (GroupRolesCaches) GroupRolesCaches[requestID] = new Dictionary<LLUUID, GroupRole>();

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
        public void BeginGetGroupRoleMembers(LLUUID group)
        {
            LLUUID requestID = LLUUID.Random();
            lock (GroupRolesMembersCaches)
            {
                GroupRolesMembersCaches[requestID] = new List<KeyValuePair<LLUUID, LLUUID>>();
            }

            GroupRoleMembersRequestPacket request = new GroupRoleMembersRequestPacket();
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
        public void BeginGetGroupTitles(LLUUID group)
        {
            LLUUID requestID = LLUUID.Random();

            GroupTitlesRequestPacket request = new GroupTitlesRequestPacket();

            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;
            request.AgentData.GroupID = group;
            request.AgentData.RequestID = requestID;

            Client.Network.SendPacket(request);
        }

        /// <summary>
        /// Begin to get the group account summary
        /// </summary>
        /// <param name="group">The group's ID</param>
        /// <param name="intervalDays">How long of an interval</param>
        /// <param name="currentInterval">Which interval (0 for current, 1 for last)</param>
        public void BeginGetGroupAccountSummary(LLUUID group, int intervalDays, int currentInterval)
        {
            GroupAccountSummaryRequestPacket p = new GroupAccountSummaryRequestPacket();
            p.AgentData.AgentID = Client.Network.AgentID;
            p.AgentData.SessionID = Client.Network.SessionID;
            p.AgentData.GroupID = group;
            p.MoneyData.RequestID = LLUUID.Random();
            p.MoneyData.CurrentInterval = currentInterval;
            p.MoneyData.IntervalDays = intervalDays;
            Client.Network.SendPacket(p);
        }

        /// <summary>
        /// Invites a user to a group
        /// </summary>
        /// <param name="group">The group to invite to.</param>
        /// <param name="roles">A list of roles to invite a person to</param>
        /// <param name="personkey">Key of person to invite</param>
        public void Invite(LLUUID group, List<LLUUID> roles, LLUUID personkey)
        {
            libsecondlife.Packets.InviteGroupRequestPacket igp = new libsecondlife.Packets.InviteGroupRequestPacket();
            igp.AgentData = new libsecondlife.Packets.InviteGroupRequestPacket.AgentDataBlock();
            igp.AgentData.AgentID = Client.Network.AgentID;
            igp.AgentData.SessionID = Client.Network.SessionID;
            igp.GroupData = new libsecondlife.Packets.InviteGroupRequestPacket.GroupDataBlock();
            igp.GroupData.GroupID = group;
            List<libsecondlife.Packets.InviteGroupRequestPacket.InviteDataBlock> idbs = new List<libsecondlife.Packets.InviteGroupRequestPacket.InviteDataBlock>();
            foreach (LLUUID role in roles)
            {
                libsecondlife.Packets.InviteGroupRequestPacket.InviteDataBlock idb = new libsecondlife.Packets.InviteGroupRequestPacket.InviteDataBlock();
                idb.InviteeID = personkey;
                idb.RoleID = role;
                idbs.Add(idb);
            }
            igp.InviteData = idbs.ToArray();
            Client.Network.SendPacket(igp);
        }

        /// <summary>
        /// Create a new group. This method automaticaly deducts the group creation feild
        /// </summary>
        /// <param name="group">Group struct containing the new group info</param>
        public void CreateGroup(Group group)
        {
            libsecondlife.Packets.CreateGroupRequestPacket cgrp = new CreateGroupRequestPacket();
            //Fill in agent data
            cgrp.AgentData = new CreateGroupRequestPacket.AgentDataBlock();
            cgrp.AgentData.AgentID = Client.Network.AgentID;
            cgrp.AgentData.SessionID = Client.Network.SessionID;
            //Fill in group data
            cgrp.GroupData = new CreateGroupRequestPacket.GroupDataBlock();
            cgrp.GroupData.AllowPublish = group.AllowPublish;
            cgrp.GroupData.Charter = Helpers.StringToField(group.Charter);
            cgrp.GroupData.InsigniaID = group.InsigniaID;
            cgrp.GroupData.MaturePublish = group.MaturePublish;
            cgrp.GroupData.MembershipFee = group.MembershipFee;
            cgrp.GroupData.Name = Helpers.StringToField(group.Name);
            cgrp.GroupData.OpenEnrollment = group.OpenEnrollment;
            cgrp.GroupData.ShowInList = group.ShowInList;
            //Send it
            Client.Network.SendPacket(cgrp);
        }

        /// <summary>
        /// Update a group's profile and other information
        /// </summary>
        /// <param name="group">Group struct to update</param>
        public void UpdateGroup(LLUUID id, Group group)
        {
            libsecondlife.Packets.UpdateGroupInfoPacket cgrp = new UpdateGroupInfoPacket();
            //Fill in agent data
            cgrp.AgentData = new UpdateGroupInfoPacket.AgentDataBlock();
            cgrp.AgentData.AgentID = Client.Network.AgentID;
            cgrp.AgentData.SessionID = Client.Network.SessionID;
            //Fill in group data
            cgrp.GroupData = new UpdateGroupInfoPacket.GroupDataBlock();
            cgrp.GroupData.GroupID = id;
            cgrp.GroupData.AllowPublish = group.AllowPublish;
            cgrp.GroupData.Charter = Helpers.StringToField(group.Charter);
            cgrp.GroupData.InsigniaID = group.InsigniaID;
            cgrp.GroupData.MaturePublish = group.MaturePublish;
            cgrp.GroupData.MembershipFee = group.MembershipFee;
            cgrp.GroupData.OpenEnrollment = group.OpenEnrollment;
            cgrp.GroupData.ShowInList = group.ShowInList;
            //Send it
            Client.Network.SendPacket(cgrp);
        }

        /// <summary>
        /// Eject a user from a group
        /// </summary>
        /// <param name="group">Group to eject the user from</param>
        /// <param name="member">Avatar's key to eject</param>
        public void EjectUser(LLUUID group, LLUUID member)
        {
            libsecondlife.Packets.EjectGroupMemberRequestPacket eject = new EjectGroupMemberRequestPacket();
            eject.AgentData = new EjectGroupMemberRequestPacket.AgentDataBlock();
            eject.AgentData.AgentID = Client.Network.AgentID;
            eject.AgentData.SessionID = Client.Network.SessionID;
            //Group
            eject.GroupData = new EjectGroupMemberRequestPacket.GroupDataBlock();
            eject.GroupData.GroupID = group;
            //People to eject
            eject.EjectData = new EjectGroupMemberRequestPacket.EjectDataBlock[1];
            eject.EjectData[0] = new EjectGroupMemberRequestPacket.EjectDataBlock();
            eject.EjectData[0].EjecteeID = member;
            //send it
            Client.Network.SendPacket(eject);
        }

        /// <summary>
        /// Update role information
        /// </summary>
        /// <param name="group">Group to update</param>
        /// <param name="role">Role to update</param>
        public void UpdateRole(LLUUID group, GroupRole role)
        {
            libsecondlife.Packets.GroupRoleUpdatePacket gru = new GroupRoleUpdatePacket();
            gru.AgentData.AgentID = Client.Network.AgentID;
            gru.AgentData.SessionID = Client.Network.SessionID;
            gru.AgentData.GroupID = group;
            gru.RoleData = new GroupRoleUpdatePacket.RoleDataBlock[1];
            gru.RoleData[0].Name = Helpers.StringToField(role.Name);
            gru.RoleData[0].Description = Helpers.StringToField(role.Description);
            gru.RoleData[0].Powers = role.Powers;
            gru.RoleData[0].Title = Helpers.StringToField(role.Title);
            gru.RoleData[0].UpdateType = (byte)GroupRoleUpdate.UpdateAll;
            Client.Network.SendPacket(gru);
        }

        /// <summary>
        /// Create a new role
        /// </summary>
        /// <param name="group">Group to update</param>
        /// <param name="role">Role to create</param>
        public void CreateRole(LLUUID group, GroupRole role)
        {
            libsecondlife.Packets.GroupRoleUpdatePacket gru = new GroupRoleUpdatePacket();
            gru.AgentData.AgentID = Client.Network.AgentID;
            gru.AgentData.SessionID = Client.Network.SessionID;
            gru.AgentData.GroupID = group;
            gru.RoleData = new GroupRoleUpdatePacket.RoleDataBlock[1];
            gru.RoleData[0].Name = Helpers.StringToField(role.Name);
            gru.RoleData[0].Description = Helpers.StringToField(role.Description);
            gru.RoleData[0].Powers = role.Powers;
            gru.RoleData[0].Title = Helpers.StringToField(role.Title);
            gru.RoleData[0].UpdateType = (byte)GroupRoleUpdate.Create;
            Client.Network.SendPacket(gru);
        }

        /// <summary>
        /// Remove an avatar from a role
        /// </summary>
        /// <param name="group">Group to update</param>
        /// <param name="role">Role to be removed from</param>
        /// <param name="member">Avatar to remove</param>
        public void RemoveFromRole(LLUUID group, LLUUID role, LLUUID member)
        {
            libsecondlife.Packets.GroupRoleChangesPacket grc = new GroupRoleChangesPacket();
            grc.AgentData.AgentID = Client.Network.AgentID;
            grc.AgentData.SessionID = Client.Network.SessionID;
            grc.AgentData.GroupID = group;
            grc.RoleChange = new GroupRoleChangesPacket.RoleChangeBlock[1];
            grc.RoleChange[0] = new GroupRoleChangesPacket.RoleChangeBlock();
            //Add to members and role
            grc.RoleChange[0].MemberID = member;
            grc.RoleChange[0].RoleID = role;
            //1 = Remove From Role
            grc.RoleChange[0].Change = 1;
            Client.Network.SendPacket(grc);
        }

        /// <summary>
        /// Assign an avatar to a role
        /// </summary>
        /// <param name="group">Group to update</param>
        /// <param name="role">Role to assign to</param>
        /// <param name="member">Avatar to assign</param>
        public void AddToRole(LLUUID group, LLUUID role, LLUUID member)
        {
            libsecondlife.Packets.GroupRoleChangesPacket grc = new GroupRoleChangesPacket();
            grc.AgentData.AgentID = Client.Network.AgentID;
            grc.AgentData.SessionID = Client.Network.SessionID;
            grc.AgentData.GroupID = group;
            grc.RoleChange = new GroupRoleChangesPacket.RoleChangeBlock[1];
            grc.RoleChange[0] = new GroupRoleChangesPacket.RoleChangeBlock();
            //Add to members and role
            grc.RoleChange[0].MemberID = member;
            grc.RoleChange[0].RoleID = role;
            //0 = Add to Role
            grc.RoleChange[0].Change = 0;
            Client.Network.SendPacket(grc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <param name="notice"></param>
        public void SendGroupNotice(LLUUID group, GroupNotice notice)
        {
            Client.Self.InstantMessage(Client.ToString(), group, notice.Subject + "|" + notice.Message,
                LLUUID.Zero, MainAvatar.InstantMessageDialog.GroupNotice, MainAvatar.InstantMessageOnline.Online, 
                LLVector3.Zero, LLUUID.Zero, notice.SerializeAttachment());
        }

        /// <summary>
        /// Start a group proposal (vote)
        /// </summary>
        /// <param name="group">The group to send it to</param>
        /// <param name="prop">The proposal to start</param>
        public void StartProposal(LLUUID group, GroupProposal prop)
        {
            StartGroupProposalPacket p = new StartGroupProposalPacket();
            p.AgentData.AgentID = Client.Network.AgentID;
            p.AgentData.SessionID = Client.Network.SessionID;
            p.ProposalData.GroupID = group;
            p.ProposalData.ProposalText = Helpers.StringToField(prop.VoteText);
            p.ProposalData.Quorum = prop.Quorum;
            p.ProposalData.Majority = prop.Majority;
            p.ProposalData.Duration = prop.Duration;
            Client.Network.SendPacket(p);
        }

        #region Packet Handlers

        private void GroupDataHandler(Packet packet, Simulator simulator)
        {
            if (OnCurrentGroups != null)
            {
                AgentGroupDataUpdatePacket update = (AgentGroupDataUpdatePacket)packet;

                Dictionary<LLUUID, Group> currentGroups = new Dictionary<LLUUID, Group>();

                foreach (AgentGroupDataUpdatePacket.GroupDataBlock block in update.GroupData)
                {
                    Group group = new Group();

                    group.ID = block.GroupID;
                    group.InsigniaID = block.GroupInsigniaID;
                    group.Name = Helpers.FieldToUTF8String(block.GroupName);
                    group.Powers = block.GroupPowers;
                    group.Contribution = block.Contribution;
                    group.AcceptNotices = block.AcceptNotices;

                    currentGroups[block.GroupID] = group;
                }

                try { OnCurrentGroups(currentGroups); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        private void GroupProfileHandler(Packet packet, Simulator simulator)
        {
            if (OnGroupProfile != null)
            {
                GroupProfileReplyPacket profile = (GroupProfileReplyPacket)packet;
                GroupProfile group = new GroupProfile();

                group.AllowPublish = profile.GroupData.AllowPublish;
                group.Charter = Helpers.FieldToUTF8String(profile.GroupData.Charter);
                group.FounderID = profile.GroupData.FounderID;
                group.GroupMembershipCount = profile.GroupData.GroupMembershipCount;
                group.GroupRolesCount = profile.GroupData.GroupRolesCount;
                group.InsigniaID = profile.GroupData.InsigniaID;
                group.MaturePublish = profile.GroupData.MaturePublish;
                group.MembershipFee = profile.GroupData.MembershipFee;
                group.MemberTitle = Helpers.FieldToUTF8String(profile.GroupData.MemberTitle);
                group.Money = profile.GroupData.Money;
                group.Name = Helpers.FieldToUTF8String(profile.GroupData.Name);
                group.OpenEnrollment = profile.GroupData.OpenEnrollment;
                group.OwnerRole = profile.GroupData.OwnerRole;
                group.Powers = profile.GroupData.PowersMask;
                group.ShowInList = profile.GroupData.ShowInList;

                try { OnGroupProfile(group); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        private void GroupTitlesHandler(Packet packet, Simulator simulator)
        {
            if (OnGroupTitles != null)
            {
                GroupTitlesReplyPacket titles = (GroupTitlesReplyPacket)packet;
                Dictionary<LLUUID, GroupTitle> groupTitleCache = new Dictionary<LLUUID, GroupTitle>();

                foreach (GroupTitlesReplyPacket.GroupDataBlock block in titles.GroupData)
                {
                    GroupTitle groupTitle = new GroupTitle();

                    groupTitle.Title = Helpers.FieldToUTF8String(block.Title);
                    groupTitle.Selected = block.Selected;

                    groupTitleCache[block.RoleID] = groupTitle;
                }

                try { OnGroupTitles(groupTitleCache); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
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
                        GroupMember groupMember = new GroupMember();

                        groupMember.ID = block.AgentID;
                        groupMember.Contribution = block.Contribution;
                        groupMember.IsOwner = block.IsOwner;
                        groupMember.OnlineStatus = Helpers.FieldToUTF8String(block.OnlineStatus);
                        groupMember.Powers = block.AgentPowers;
                        groupMember.Title = Helpers.FieldToUTF8String(block.Title);

                        groupMemberCache[block.AgentID] = groupMember;
                    }
                }
            }

            // Check if we've received all the group members that are showing up
            if (OnGroupMembers != null && groupMemberCache != null && groupMemberCache.Count >= members.GroupData.MemberCount)
            {
                try { OnGroupMembers(groupMemberCache); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
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
                        GroupRole groupRole = new GroupRole();

                        groupRole.ID = block.RoleID;
                        groupRole.Description = Helpers.FieldToUTF8String(block.Description);
                        groupRole.Name = Helpers.FieldToUTF8String(block.Name);
                        groupRole.Powers = block.Powers;
                        groupRole.Title = Helpers.FieldToUTF8String(block.Title);

                        groupRoleCache[block.RoleID] = groupRole;
                    }
                }
            }

            // Check if we've received all the group members that are showing up
            if (OnGroupRoles != null && groupRoleCache != null && groupRoleCache.Count >= roles.GroupData.RoleCount)
            {
                try { OnGroupRoles(groupRoleCache); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
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

                    foreach (GroupRoleMembersReplyPacket.MemberDataBlock block in members.MemberData)
                    {
                        KeyValuePair<LLUUID, LLUUID> rolemember = 
                            new KeyValuePair<LLUUID, LLUUID>(block.RoleID, block.MemberID);

                        groupRoleMemberCache.Add(rolemember);
                    }
                }
            }

            Client.DebugLog("Pairs Ratio: " + groupRoleMemberCache.Count + "/" + members.AgentData.TotalPairs);
            
            // Check if we've received all the pairs that are showing up
            if (OnGroupRolesMembers != null && groupRoleMemberCache != null && groupRoleMemberCache.Count >= members.AgentData.TotalPairs)
            {
                try { OnGroupRolesMembers(groupRoleMemberCache); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        private void GroupActiveProposalItemHandler(Packet packet, Simulator simulator)
        {
            //GroupActiveProposalItemReplyPacket proposal = (GroupActiveProposalItemReplyPacket)packet;

            // TODO: Create a proposal struct to represent the fields in a proposal item
        }

        private void GroupVoteHistoryItemHandler(Packet packet, Simulator simulator)
        {
            //GroupVoteHistoryItemReplyPacket history = (GroupVoteHistoryItemReplyPacket)packet;

            // TODO: This was broken in the official viewer when I was last trying to work  on it
        }

        private void GroupAccountSummaryHandler(Packet packet, Simulator simulator)
        {
            if (OnGroupAccountSummary != null)
            {
                GroupAccountSummaryReplyPacket summary = (GroupAccountSummaryReplyPacket)packet;
                GroupAccountSummary account = new GroupAccountSummary();

                account.Balance = summary.MoneyData.Balance;
                account.CurrentInterval = summary.MoneyData.CurrentInterval;
                account.GroupTaxCurrent = summary.MoneyData.GroupTaxCurrent;
                account.GroupTaxEstimate = summary.MoneyData.GroupTaxEstimate;
                account.IntervalDays = summary.MoneyData.IntervalDays;
                account.LandTaxCurrent = summary.MoneyData.LandTaxCurrent;
                account.LandTaxEstimate = summary.MoneyData.LandTaxEstimate;
                account.LastTaxDate = Helpers.FieldToUTF8String(summary.MoneyData.LastTaxDate);
                account.LightTaxCurrent = summary.MoneyData.LightTaxCurrent;
                account.LightTaxEstimate = summary.MoneyData.LightTaxEstimate;
                account.NonExemptMembers = summary.MoneyData.NonExemptMembers;
                account.ObjectTaxCurrent = summary.MoneyData.ObjectTaxCurrent;
                account.ObjectTaxEstimate = summary.MoneyData.ObjectTaxEstimate;
                account.ParcelDirFeeCurrent = summary.MoneyData.ParcelDirFeeCurrent;
                account.ParcelDirFeeEstimate = summary.MoneyData.ParcelDirFeeEstimate;
                account.StartDate = Helpers.FieldToUTF8String(summary.MoneyData.StartDate);
                account.TaxDate = Helpers.FieldToUTF8String(summary.MoneyData.TaxDate);
                account.TotalCredits = summary.MoneyData.TotalCredits;
                account.TotalDebits = summary.MoneyData.TotalDebits;

                try { OnGroupAccountSummary(account); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        private void GroupAccountDetailsHandler(Packet packet, Simulator simulator)
        {
            if (OnGroupAccountDetails != null)
            {
                GroupAccountDetailsReplyPacket details = (GroupAccountDetailsReplyPacket)packet;
                GroupAccountDetails account = new GroupAccountDetails();

                account.CurrentInterval = details.MoneyData.CurrentInterval;
                account.IntervalDays = details.MoneyData.IntervalDays;
                account.StartDate = Helpers.FieldToUTF8String(details.MoneyData.StartDate);

                account.HistoryItems = new List<KeyValuePair<string, int>>();

                foreach (GroupAccountDetailsReplyPacket.HistoryDataBlock block in details.HistoryData)
                {
                    KeyValuePair<string, int> item =
                        new KeyValuePair<string, int>(Helpers.FieldToUTF8String(block.Description), block.Amount);

                    account.HistoryItems.Add(item);
                }

                try { OnGroupAccountDetails(account); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
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

            //    try { OnGroupAccountTransactions(account); }
            //    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            //}
        }

        #endregion Packet Handlers
    }
}
