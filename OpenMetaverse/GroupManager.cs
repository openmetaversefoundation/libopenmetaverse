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
using System.Collections.Generic;
using OpenMetaverse.Packets;

namespace OpenMetaverse
{
    #region Structs

    /// <summary>
    /// Avatar group management
    /// </summary>
    public struct GroupMember
    {
        /// <summary>Key of Group Member</summary>
        public UUID ID;
        /// <summary>Total land contribution</summary>
        public int Contribution;
        /// <summary>Online status information</summary>
        public string OnlineStatus;
        /// <summary>Abilities that the Group Member has</summary>
        public GroupPowers Powers;
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
        public UUID ID;
        /// <summary>Name of Role</summary>
        public string Name;
        /// <summary>Group Title associated with Role</summary>
        public string Title;
        /// <summary>Description of Role</summary>
        public string Description;
        /// <summary>Abilities Associated with Role</summary>
        public GroupPowers Powers;
        /// <summary>Returns the role's title</summary>
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
    /// Represents a group on the grid
    /// </summary>
    public struct Group
    {
        /// <summary>Key of Group</summary>
        public UUID ID;
        /// <summary>Key of Group Insignia</summary>
        public UUID InsigniaID;
        /// <summary>Key of Group Founder</summary>
        public UUID FounderID;
        /// <summary>Key of Group Role for Owners</summary>
        public UUID OwnerRole;
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
        public GroupPowers Powers;
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
        /// <summary>The total number of current members this group has</summary>
        public int GroupMembershipCount;
        /// <summary>The number of roles this group has configured</summary>
        public int GroupRolesCount;

        /// <summary>Returns the name of the group</summary>
        /// <returns>A string containing the name of the group</returns>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// A group Vote
    /// </summary>
    public struct Vote
    {
        /// <summary>Key of Avatar who created Vote</summary>
        public UUID Candidate;
        /// <summary>Text of the Vote proposal</summary>
        public string VoteString;
        /// <summary>Total number of votes</summary>
        public int NumVotes;
    }

    /// <summary>
    /// A group proposal
    /// </summary>
    public struct GroupProposal
    {
        /// <summary>The Text of the proposal</summary>
        public string VoteText;
        /// <summary>The minimum number of members that must vote before proposal passes or failes</summary>
        public int Quorum;
        /// <summary>The required ration of yes/no votes required for vote to pass</summary>
        /// <remarks>The three options are Simple Majority, 2/3 Majority, and Unanimous</remarks>
        /// TODO: this should be an enum
        public float Majority;
        /// <summary>The duration in days votes are accepted</summary>
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
    /// Struct representing a group notice
    /// </summary>
    public struct GroupNotice
    {
        /// <summary></summary>
        public string Subject;
        /// <summary></summary>
        public string Message;
        /// <summary></summary>
        public UUID AttachmentID;
        /// <summary></summary>
        public UUID OwnerID;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] SerializeAttachment()
        {
            if (OwnerID == UUID.Zero || AttachmentID == UUID.Zero)
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

    [Flags]
    public enum GroupPowers : ulong
    {
        MemberInvite = 2,
        MemberEject = 4,
        MemberOptions = 8,
        MemberVisibleInDir = 140737488355328,
        RoleCreate = 16,
        RoleDelete = 32,
        RoleProperties = 64,
        RoleAssignMemberLimited = 128,
        RoleAssignMember = 256,
        RoleRemoveMember = 512,
        RoleChangeActions = 1024,
        GroupChangeIdentity = 2048,
        LandDeed = 4096,
        LandRelease = 8192,
        LandSetSaleInfo = 16384,
        LandDivideJoin = 32768,
        LandFindPlaces = 131072,
        LandChangeIdentity = 262144,
        LandSetLandingPoint = 524288,
        LandChangeMedia = 1048576,
        LandEdit = 2097152,
        LandOptions = 4194304,
        LandAllowEditLand = 8388608,
        LandAllowFly = 16777216,
        LandAllowCreate = 33554432,
        LandAllowLandmark = 67108864,
        LandAllowSetHome = 268435456,
        LandManageAllowed = 536870912,
        LandManageBanned = 1073741824,
        LandManagePasses = 2147483648,
        LandAdmin = 4294967296,
        LandReturnGroupOwned = 281474976710656,
        LandReturnGroupSet = 8589934592,
        LandReturnNonGroup = 17179869184,
        LandReturn = LandReturnGroupOwned | LandReturnGroupSet | LandReturnNonGroup,
        LandGardening = 34359738368,
        ObjectDeed = 68719476736,
        ObjectManipulate = 274877906944,
        ObjectSetSale = 549755813888,
        AccountingAccountable = 1099511627776,
        NoticesSend = 4398046511104,
        NoticesReceive = 8796093022208,
        ProposalStart = 17592186044416,
        ProposalVote = 35184372088832
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
        public delegate void CurrentGroupsCallback(Dictionary<UUID, Group> groups);
        /// <summary>
        /// Callback for a list of group names
        /// </summary>
        /// <param name="groupNames"></param>
        public delegate void GroupNamesCallback(Dictionary<UUID, string> groupNames);
        /// <summary>
        /// Callback for the profile of a group
        /// </summary>
        /// <param name="group"></param>
        public delegate void GroupProfileCallback(Group group);
        /// <summary>
        /// Callback for the member list of a group
        /// </summary>
        /// <param name="members"></param>
        public delegate void GroupMembersCallback(Dictionary<UUID, GroupMember> members);
        /// <summary>
        /// Callback for the role list of a group
        /// </summary>
        /// <param name="roles"></param>
        public delegate void GroupRolesCallback(Dictionary<UUID, GroupRole> roles);
        /// <summary>
        /// Callback for a pairing of roles to members
        /// </summary>
        /// <param name="rolesMembers"></param>
        public delegate void GroupRolesMembersCallback(List<KeyValuePair<UUID, UUID>> rolesMembers);
        /// <summary>
        /// Callback for the title list of a group
        /// </summary>
        /// <param name="titles"></param>
        public delegate void GroupTitlesCallback(Dictionary<UUID, GroupTitle> titles);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="summary"></param>
        public delegate void GroupAccountSummaryCallback(GroupAccountSummary summary);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="success"></param>
        /// <param name="message"></param>
        public delegate void GroupCreatedCallback(UUID groupID, bool success, string message);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="success"></param>
        public delegate void GroupJoinedCallback(UUID groupID, bool success);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="success"></param>
        public delegate void GroupLeftCallback(UUID groupID, bool success);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupID"></param>
        public delegate void GroupDroppedCallback(UUID groupID);

        /// <summary>
        /// Fired when a member of a group is ejected, 
        /// Does not provide member information, only 
        /// group ID and whether it was successful or not
        /// </summary>
        /// <param name="groupID">The Group UUID the member was ejected from</param>
        /// <param name="success">true of member was successfully ejected</param>
        public delegate void GroupMemberEjectedCallback(UUID groupID, bool success);

        #endregion Delegates

        #region Events

        /// <summary>Fired when a <seealso cref="T:OpenMetaverse.Packets.AgentGroupDataUpdatePacket"/> is received, contains a list of 
        /// groups avatar is currently a member of</summary>
        public event CurrentGroupsCallback OnCurrentGroups;
        /// <summary>Fired when a UUIDGroupNameReply packet is receiived, 
        /// contains name of group requested</summary>
        public event GroupNamesCallback OnGroupNames;
        /// <summary>Fired when a GroupProfileReply packet is received,
        /// contains group profile information for requested group.</summary>
        public event GroupProfileCallback OnGroupProfile;
        /// <summary>Fired when a GroupMembersReply packet is received,
        /// contains a list of group members for requested group</summary>
        public event GroupMembersCallback OnGroupMembers;
        /// <summary>Fired when a GroupRoleDataReply packet is received,
        /// contains details on roles for requested group</summary>
        public event GroupRolesCallback OnGroupRoles;
        /// <summary>Fired when a <seealso cref="T:OpenMetaverse.Packets.GroupRoleMembersReplyPacket"/> is received,
        /// Contains group member to group role mappings</summary>
        public event GroupRolesMembersCallback OnGroupRolesMembers;
        /// <summary>Fired when a GroupTitlesReply packet is received,
        /// sets the active role title for the current Agent</summary>
        public event GroupTitlesCallback OnGroupTitles;
        /// <summary>Fired when a GroupAccountSummaryReply packet is received,
        /// Contains a summary of group financial information</summary>
        public event GroupAccountSummaryCallback OnGroupAccountSummary;
        /// <summary>Fired when a CreateGroupReply packet is received, indicates
        /// the successful creation of a new group</summary>
        public event GroupCreatedCallback OnGroupCreated;
        /// <summary>Fired when a JoinGroupReply packet is received, indicates
        /// the Avatar has successfully joined a new group either by <seealso cref="RequestJoinGroup"/>
        /// or by accepting a group join invitation with <seealso cref="AgentManager.GroupInviteRespond"/></summary>
        public event GroupJoinedCallback OnGroupJoined;
        /// <summary>Fired when a LeaveGroupReply packet is received, indicates
        /// the Avatar has successfully left a group</summary>
        /// <seealso cref="LeaveGroup"/>
        public event GroupLeftCallback OnGroupLeft;
        /// <summary>Fired when a AgentDropGroup packet is received, contains
        /// the <seealso cref="Group.ID"/> of the group dropped</summary>
        public event GroupDroppedCallback OnGroupDropped;
        /// <summary>Fired when a GroupMemberEjected packet is received,
        /// indicates a member of a group has been ejected</summary>
        public event GroupMemberEjectedCallback OnGroupMemberEjected;

        #endregion Events

        /// <summary>A reference to the current <seealso cref="GridClient"/> instance</summary>
        private GridClient Client;
        /// <summary>A list of all the lists of group members, indexed by the request ID</summary>
        private Dictionary<UUID, Dictionary<UUID, GroupMember>> GroupMembersCaches;
        /// <summary>A list of all the lists of group roles, indexed by the request ID</summary>
        private Dictionary<UUID, Dictionary<UUID, GroupRole>> GroupRolesCaches;
        /// <summary>A list of all the role to member mappings</summary>
        private Dictionary<UUID, List<KeyValuePair<UUID, UUID>>> GroupRolesMembersCaches;
        /// <summary>Caches group name lookups</summary>
        public InternalDictionary<UUID, string> GroupName2KeyCache;

        /// <summary>
        /// Group Management Routines, Methods and Packet Handlers
        /// </summary>
        /// <param name="client">A reference to the current <seealso cref="GridClient"/> instance</param>
        public GroupManager(GridClient client)
        {
            Client = client;

            GroupMembersCaches = new Dictionary<UUID, Dictionary<UUID, GroupMember>>();
            GroupRolesCaches = new Dictionary<UUID, Dictionary<UUID, GroupRole>>();
            GroupRolesMembersCaches = new Dictionary<UUID, List<KeyValuePair<UUID, UUID>>>();
            GroupName2KeyCache  = new InternalDictionary<UUID, string>();

            Client.Network.RegisterCallback(PacketType.AgentGroupDataUpdate, new NetworkManager.PacketCallback(GroupDataHandler));
            Client.Network.RegisterCallback(PacketType.AgentDropGroup, new NetworkManager.PacketCallback(AgentDropGroupHandler));
            Client.Network.RegisterCallback(PacketType.GroupTitlesReply, new NetworkManager.PacketCallback(GroupTitlesHandler));
            Client.Network.RegisterCallback(PacketType.GroupProfileReply, new NetworkManager.PacketCallback(GroupProfileHandler));
            Client.Network.RegisterCallback(PacketType.GroupMembersReply, new NetworkManager.PacketCallback(GroupMembersHandler));
            Client.Network.RegisterCallback(PacketType.GroupRoleDataReply, new NetworkManager.PacketCallback(GroupRoleDataHandler));
            Client.Network.RegisterCallback(PacketType.GroupRoleMembersReply, new NetworkManager.PacketCallback(GroupRoleMembersHandler));
            Client.Network.RegisterCallback(PacketType.GroupActiveProposalItemReply, new NetworkManager.PacketCallback(GroupActiveProposalItemHandler));
            Client.Network.RegisterCallback(PacketType.GroupVoteHistoryItemReply, new NetworkManager.PacketCallback(GroupVoteHistoryItemHandler));
            Client.Network.RegisterCallback(PacketType.GroupAccountSummaryReply, new NetworkManager.PacketCallback(GroupAccountSummaryHandler));
            Client.Network.RegisterCallback(PacketType.CreateGroupReply, new NetworkManager.PacketCallback(CreateGroupReplyHandler));
            Client.Network.RegisterCallback(PacketType.JoinGroupReply, new NetworkManager.PacketCallback(JoinGroupReplyHandler));
            Client.Network.RegisterCallback(PacketType.LeaveGroupReply, new NetworkManager.PacketCallback(LeaveGroupReplyHandler));
            Client.Network.RegisterCallback(PacketType.UUIDGroupNameReply, new NetworkManager.PacketCallback(UUIDGroupNameReplyHandler));
            Client.Network.RegisterCallback(PacketType.EjectGroupMemberReply, new NetworkManager.PacketCallback(EjectGroupMemberReplyHandler));
        }

        /// <summary>
        /// Request a current list of groups the avatar is a member of.
        /// </summary>
        /// <remarks>CAPS Event Queue must be running for this to work since the results
        /// come across CAPS.</remarks>
        public void RequestCurrentGroups()
        {
            AgentDataUpdateRequestPacket request = new AgentDataUpdateRequestPacket();

            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            
            Client.Network.SendPacket(request);
        }

        /// <summary>
        /// Lookup name of group based on groupID
        /// </summary>
        /// <param name="groupID">groupID of group to lookup name for.</param>
        public void RequestGroupName(UUID groupID)
        {
            // if we already have this in the cache, return from cache instead of making a request
                if (GroupName2KeyCache.ContainsKey(groupID))
                {
                    Dictionary<UUID, string> groupNames = new Dictionary<UUID, string>();
                    lock(GroupName2KeyCache.Dictionary)
                    groupNames.Add(groupID, GroupName2KeyCache.Dictionary[groupID]);
                    if (OnGroupNames != null)
                    {
                           
                        try { OnGroupNames(groupNames); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                    }
                }
            
                else
                {
                    UUIDGroupNameRequestPacket req = new UUIDGroupNameRequestPacket();
                    UUIDGroupNameRequestPacket.UUIDNameBlockBlock[] block = new UUIDGroupNameRequestPacket.UUIDNameBlockBlock[1];
                    block[0] = new UUIDGroupNameRequestPacket.UUIDNameBlockBlock();
                    block[0].ID = groupID;
                    req.UUIDNameBlock = block;
                    Client.Network.SendPacket(req);
                }
        }

        /// <summary>
        /// Request lookup of multiple group names
        /// </summary>
        /// <param name="groupIDs">List of group IDs to request.</param>
        public void RequestGroupNames(List<UUID> groupIDs)
        {
            Dictionary<UUID, string> groupNames = new Dictionary<UUID, string>();
            lock (GroupName2KeyCache.Dictionary)
            {
                foreach (UUID groupID in groupIDs)
                {
                    if (GroupName2KeyCache.ContainsKey(groupID))
                        groupNames[groupID] = GroupName2KeyCache.Dictionary[groupID];
                }
            }        
            
            if (groupIDs.Count > 0)
            {
                UUIDGroupNameRequestPacket req = new UUIDGroupNameRequestPacket();
                UUIDGroupNameRequestPacket.UUIDNameBlockBlock[] block = new UUIDGroupNameRequestPacket.UUIDNameBlockBlock[groupIDs.Count];

                for (int i = 0; i < groupIDs.Count; i++)
                {
                    block[i] = new UUIDGroupNameRequestPacket.UUIDNameBlockBlock();
                    block[i].ID = groupIDs[i];
                }

                req.UUIDNameBlock = block;
                Client.Network.SendPacket(req);
            }

            // fire handler from cache
            if(groupNames.Count > 0 && OnGroupNames != null)
                try { OnGroupNames(groupNames); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
        }

        /// <summary>Lookup group profile data such as name, enrollment, founder, logo, etc</summary>
        /// <remarks>Subscribe to <code>OnGroupProfile</code> event to receive the results.</remarks>
        /// <param name="group">group ID (UUID)</param>
        public void RequestGroupProfile(UUID group)
        {
            GroupProfileRequestPacket request = new GroupProfileRequestPacket();

            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.GroupData.GroupID = group;

            Client.Network.SendPacket(request);
        }

        /// <summary>Request a list of group members.</summary>
        /// <remarks>Subscribe to <code>OnGroupMembers</code> event to receive the results.</remarks>
        /// <param name="group">group ID (UUID)</param>
        public void RequestGroupMembers(UUID group)
        {
            UUID requestID = UUID.Random();
            lock (GroupMembersCaches) GroupMembersCaches[requestID] = new Dictionary<UUID, GroupMember>();

            GroupMembersRequestPacket request = new GroupMembersRequestPacket();

            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.GroupData.GroupID = group;
            request.GroupData.RequestID = requestID;

            Client.Network.SendPacket(request);
        }

        /// <summary>Request group roles</summary>
        /// <remarks>Subscribe to <code>OnGroupRoles</code> event to receive the results.</remarks>
        /// <param name="group">group ID (UUID)</param>
        public void RequestGroupRoles(UUID group)
        {
            UUID requestID = UUID.Random();
            lock (GroupRolesCaches) GroupRolesCaches[requestID] = new Dictionary<UUID, GroupRole>();

            GroupRoleDataRequestPacket request = new GroupRoleDataRequestPacket();

            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.GroupData.GroupID = group;
            request.GroupData.RequestID = requestID;

            Client.Network.SendPacket(request);
        }

        /// <summary>Request members (members,role) role mapping for a group.</summary>
        /// <remarks>Subscribe to <code>OnGroupRolesMembers</code> event to receive the results.</remarks>
        /// <param name="group">group ID (UUID)</param>
        public void RequestGroupRoleMembers(UUID group)
        {
            UUID requestID = UUID.Random();
            lock (GroupRolesMembersCaches)
            {
                GroupRolesMembersCaches[requestID] = new List<KeyValuePair<UUID, UUID>>();
            }

            GroupRoleMembersRequestPacket request = new GroupRoleMembersRequestPacket();
            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.GroupData.GroupID = group;
            request.GroupData.RequestID = requestID;
            Client.Network.SendPacket(request);
        }

        /// <summary>Request a groups Titles</summary>
        /// <remarks>Subscribe to <code>OnGroupTitles</code> event to receive the results.</remarks>
        /// <param name="group">group ID (UUID)</param>
        public void RequestGroupTitles(UUID group)
        {
            UUID requestID = UUID.Random();

            GroupTitlesRequestPacket request = new GroupTitlesRequestPacket();

            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.AgentData.GroupID = group;
            request.AgentData.RequestID = requestID;

            Client.Network.SendPacket(request);
        }

        /// <summary>Begin to get the group account summary</summary>
        /// <remarks>Subscribe to the <code>OnGroupAccountSummary</code> event to receive the results.</remarks>
        /// <param name="group">group ID (UUID)</param>
        /// <param name="intervalDays">How long of an interval</param>
        /// <param name="currentInterval">Which interval (0 for current, 1 for last)</param>
        public void RequestGroupAccountSummary(UUID group, int intervalDays, int currentInterval)
        {
            GroupAccountSummaryRequestPacket p = new GroupAccountSummaryRequestPacket();
            p.AgentData.AgentID = Client.Self.AgentID;
            p.AgentData.SessionID = Client.Self.SessionID;
            p.AgentData.GroupID = group;
            p.MoneyData.RequestID = UUID.Random();
            p.MoneyData.CurrentInterval = currentInterval;
            p.MoneyData.IntervalDays = intervalDays;
            Client.Network.SendPacket(p);
        }

        /// <summary>Invites a user to a group</summary>
        /// <param name="group">The group to invite to</param>
        /// <param name="roles">A list of roles to invite a person to</param>
        /// <param name="personkey">Key of person to invite</param>
        public void Invite(UUID group, List<UUID> roles, UUID personkey)
        {
            InviteGroupRequestPacket igp = new InviteGroupRequestPacket();

            igp.AgentData = new InviteGroupRequestPacket.AgentDataBlock();
            igp.AgentData.AgentID = Client.Self.AgentID;
            igp.AgentData.SessionID = Client.Self.SessionID;

            igp.GroupData = new InviteGroupRequestPacket.GroupDataBlock();
            igp.GroupData.GroupID = group;

            igp.InviteData = new InviteGroupRequestPacket.InviteDataBlock[roles.Count];

            for (int i = 0; i < roles.Count; i++)
            {
                igp.InviteData[i] = new InviteGroupRequestPacket.InviteDataBlock();
                igp.InviteData[i].InviteeID = personkey;
                igp.InviteData[i].RoleID = roles[i];
            }

            Client.Network.SendPacket(igp);
        }

        /// <summary>Set a group as the current active group</summary>
        /// <param name="id">group ID (UUID)</param>
        public void ActivateGroup(UUID id)
        {
            ActivateGroupPacket activate = new ActivateGroupPacket();
            activate.AgentData.AgentID = Client.Self.AgentID;
            activate.AgentData.SessionID = Client.Self.SessionID;
            activate.AgentData.GroupID = id;

            Client.Network.SendPacket(activate);
        }

        /// <summary>Change the role that determines your active title</summary>
        /// <param name="group">Group ID to use</param>
        /// <param name="role">Role ID to change to</param>
        public void ActivateTitle(UUID group, UUID role)
        {
            GroupTitleUpdatePacket gtu = new GroupTitleUpdatePacket();
            gtu.AgentData.AgentID = Client.Self.AgentID;
            gtu.AgentData.SessionID = Client.Self.SessionID;
            gtu.AgentData.TitleRoleID = role;
            gtu.AgentData.GroupID = group;

            Client.Network.SendPacket(gtu);
        }

        /// <summary>Set this avatar's tier contribution</summary>
        /// <param name="group">Group ID to change tier in</param>
        /// <param name="contribution">amount of tier to donate</param>
        public void SetGroupContribution(UUID group, int contribution)
        {
            SetGroupContributionPacket sgp = new SetGroupContributionPacket();
            sgp.AgentData.AgentID = Client.Self.AgentID;
            sgp.AgentData.SessionID = Client.Self.SessionID;
            sgp.Data.GroupID = group;
            sgp.Data.Contribution = contribution;

            Client.Network.SendPacket(sgp);
        }

        /// <summary>Request to join a group</summary>
        /// <remarks>Subscribe to <code>OnGroupJoined</code> event for confirmation.</remarks>
        /// <param name="id">group ID (UUID) to join.</param>
        public void RequestJoinGroup(UUID id)
        {
            JoinGroupRequestPacket join = new JoinGroupRequestPacket();
            join.AgentData.AgentID = Client.Self.AgentID;
            join.AgentData.SessionID = Client.Self.SessionID;

            join.GroupData.GroupID = id;

            Client.Network.SendPacket(join);
        }

        /// <summary>
        /// Request to create a new group. If the group is successfully
        /// created, L$100 will automatically be deducted
        /// </summary>
        /// <remarks>Subscribe to <code>OnGroupCreated</code> event to receive confirmation.</remarks>
        /// <param name="group">Group struct containing the new group info</param>
        public void RequestCreateGroup(Group group)
        {
            OpenMetaverse.Packets.CreateGroupRequestPacket cgrp = new CreateGroupRequestPacket();
            //Fill in agent data
            cgrp.AgentData = new CreateGroupRequestPacket.AgentDataBlock();
            cgrp.AgentData.AgentID = Client.Self.AgentID;
            cgrp.AgentData.SessionID = Client.Self.SessionID;
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

        /// <summary>Update a group's profile and other information</summary>
        /// <param name="id">Groups ID (UUID) to update.</param>
        /// <param name="group">Group struct to update.</param>
        public void UpdateGroup(UUID id, Group group)
        {
            OpenMetaverse.Packets.UpdateGroupInfoPacket cgrp = new UpdateGroupInfoPacket();
            //Fill in agent data
            cgrp.AgentData = new UpdateGroupInfoPacket.AgentDataBlock();
            cgrp.AgentData.AgentID = Client.Self.AgentID;
            cgrp.AgentData.SessionID = Client.Self.SessionID;
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

        /// <summary>Eject a user from a group</summary>
        /// <param name="group">Group ID to eject the user from</param>
        /// <param name="member">Avatar's key to eject</param>
        public void EjectUser(UUID group, UUID member)
        {
            OpenMetaverse.Packets.EjectGroupMemberRequestPacket eject = new EjectGroupMemberRequestPacket();
            eject.AgentData = new EjectGroupMemberRequestPacket.AgentDataBlock();
            eject.AgentData.AgentID = Client.Self.AgentID;
            eject.AgentData.SessionID = Client.Self.SessionID;
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

        /// <summary>Update role information</summary>
        /// <param name="group">Group to update</param>
        /// <param name="role">Role to update</param>
        public void UpdateRole(UUID group, GroupRole role)
        {
            OpenMetaverse.Packets.GroupRoleUpdatePacket gru = new GroupRoleUpdatePacket();
            gru.AgentData.AgentID = Client.Self.AgentID;
            gru.AgentData.SessionID = Client.Self.SessionID;
            gru.AgentData.GroupID = group;
            gru.RoleData = new GroupRoleUpdatePacket.RoleDataBlock[1];
            gru.RoleData[0].Name = Helpers.StringToField(role.Name);
            gru.RoleData[0].Description = Helpers.StringToField(role.Description);
            gru.RoleData[0].Powers = (ulong)role.Powers;
            gru.RoleData[0].Title = Helpers.StringToField(role.Title);
            gru.RoleData[0].UpdateType = (byte)GroupRoleUpdate.UpdateAll;
            Client.Network.SendPacket(gru);
        }

        /// <summary>Create a new group role</summary>
        /// <param name="group">Group ID to update</param>
        /// <param name="role">Role to create</param>
        public void CreateRole(UUID group, GroupRole role)
        {
            OpenMetaverse.Packets.GroupRoleUpdatePacket gru = new GroupRoleUpdatePacket();
            gru.AgentData.AgentID = Client.Self.AgentID;
            gru.AgentData.SessionID = Client.Self.SessionID;
            gru.AgentData.GroupID = group;
            gru.RoleData = new GroupRoleUpdatePacket.RoleDataBlock[1];
            gru.RoleData[0].Name = Helpers.StringToField(role.Name);
            gru.RoleData[0].Description = Helpers.StringToField(role.Description);
            gru.RoleData[0].Powers = (ulong)role.Powers;
            gru.RoleData[0].Title = Helpers.StringToField(role.Title);
            gru.RoleData[0].UpdateType = (byte)GroupRoleUpdate.Create;
            Client.Network.SendPacket(gru);
        }

        /// <summary>Remove an avatar from a role</summary>
        /// <param name="group">Group ID to update</param>
        /// <param name="role">Role ID to be removed from</param>
        /// <param name="member">Avatar's Key to remove</param>
        public void RemoveFromRole(UUID group, UUID role, UUID member)
        {
            OpenMetaverse.Packets.GroupRoleChangesPacket grc = new GroupRoleChangesPacket();
            grc.AgentData.AgentID = Client.Self.AgentID;
            grc.AgentData.SessionID = Client.Self.SessionID;
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

        /// <summary>Assign an avatar to a role</summary>
        /// <param name="group">Group ID to update</param>
        /// <param name="role">Role ID to assign to</param>
        /// <param name="member">Avatar's ID to assign to role</param>
        public void AddToRole(UUID group, UUID role, UUID member)
        {
            OpenMetaverse.Packets.GroupRoleChangesPacket grc = new GroupRoleChangesPacket();
            grc.AgentData.AgentID = Client.Self.AgentID;
            grc.AgentData.SessionID = Client.Self.SessionID;
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

        /// <summary>Send out a group notice</summary>
        /// <param name="group">Group ID to update</param>
        /// <param name="notice"><code>GroupNotice</code> structure containing notice data</param>
        public void SendGroupNotice(UUID group, GroupNotice notice)
        {
            Client.Self.InstantMessage(Client.Self.Name, group, notice.Subject + "|" + notice.Message,
                UUID.Zero, InstantMessageDialog.GroupNotice, InstantMessageOnline.Online, 
                Vector3.Zero, UUID.Zero, notice.SerializeAttachment());
        }

        /// <summary>Start a group proposal (vote)</summary>
        /// <param name="group">The Group ID to send proposal to</param>
        /// <param name="prop"><code>GroupProposal</code> structure containing the proposal</param>
        public void StartProposal(UUID group, GroupProposal prop)
        {
            StartGroupProposalPacket p = new StartGroupProposalPacket();
            p.AgentData.AgentID = Client.Self.AgentID;
            p.AgentData.SessionID = Client.Self.SessionID;
            p.ProposalData.GroupID = group;
            p.ProposalData.ProposalText = Helpers.StringToField(prop.VoteText);
            p.ProposalData.Quorum = prop.Quorum;
            p.ProposalData.Majority = prop.Majority;
            p.ProposalData.Duration = prop.Duration;
            Client.Network.SendPacket(p);
        }

        /// <summary>Request to leave a group</summary>
        /// <remarks>Subscribe to <code>OnGroupLeft</code> event to receive confirmation</remarks>
        /// <param name="groupID">The group to leave</param>
        public void LeaveGroup(UUID groupID)
        {
            LeaveGroupRequestPacket p = new LeaveGroupRequestPacket();
            p.AgentData.AgentID = Client.Self.AgentID;
            p.AgentData.SessionID = Client.Self.SessionID;
            p.GroupData.GroupID = groupID;
            Client.Network.SendPacket(p);
        }

        #region Packet Handlers

        private void GroupDataHandler(Packet packet, Simulator simulator)
        {
            if (OnCurrentGroups != null)
            {
                AgentGroupDataUpdatePacket update = (AgentGroupDataUpdatePacket)packet;

                Dictionary<UUID, Group> currentGroups = new Dictionary<UUID, Group>();

                foreach (AgentGroupDataUpdatePacket.GroupDataBlock block in update.GroupData)
                {
                    Group group = new Group();

                    group.ID = block.GroupID;
                    group.InsigniaID = block.GroupInsigniaID;
                    group.Name = Helpers.FieldToUTF8String(block.GroupName);
                    group.Powers = (GroupPowers)block.GroupPowers;
                    group.Contribution = block.Contribution;
                    group.AcceptNotices = block.AcceptNotices;

                    currentGroups[block.GroupID] = group;

                    if (!GroupName2KeyCache.ContainsKey(block.GroupID))
                        GroupName2KeyCache.SafeAdd(block.GroupID, Helpers.FieldToUTF8String(block.GroupName));
                }

                try { OnCurrentGroups(currentGroups); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void AgentDropGroupHandler(Packet packet, Simulator simulator)
        {
            if (OnGroupDropped != null)
            {
                try { OnGroupDropped(((AgentDropGroupPacket)packet).AgentData.GroupID); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void GroupProfileHandler(Packet packet, Simulator simulator)
        {
            if (OnGroupProfile != null)
            {
                GroupProfileReplyPacket profile = (GroupProfileReplyPacket)packet;
                Group group = new Group();

                group.ID = profile.GroupData.GroupID;
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
                group.Powers = (GroupPowers)profile.GroupData.PowersMask;
                group.ShowInList = profile.GroupData.ShowInList;

                try { OnGroupProfile(group); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void GroupTitlesHandler(Packet packet, Simulator simulator)
        {
            if (OnGroupTitles != null)
            {
                GroupTitlesReplyPacket titles = (GroupTitlesReplyPacket)packet;
                Dictionary<UUID, GroupTitle> groupTitleCache = new Dictionary<UUID, GroupTitle>();

                foreach (GroupTitlesReplyPacket.GroupDataBlock block in titles.GroupData)
                {
                    GroupTitle groupTitle = new GroupTitle();

                    groupTitle.Title = Helpers.FieldToUTF8String(block.Title);
                    groupTitle.Selected = block.Selected;

                    groupTitleCache[block.RoleID] = groupTitle;
                }

                try { OnGroupTitles(groupTitleCache); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void GroupMembersHandler(Packet packet, Simulator simulator)
        {
            GroupMembersReplyPacket members = (GroupMembersReplyPacket)packet;
            Dictionary<UUID, GroupMember> groupMemberCache = null;

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
                        groupMember.Powers = (GroupPowers)block.AgentPowers;
                        groupMember.Title = Helpers.FieldToUTF8String(block.Title);

                        groupMemberCache[block.AgentID] = groupMember;
                    }
                }
            }

            // Check if we've received all the group members that are showing up
            if (OnGroupMembers != null && groupMemberCache != null && groupMemberCache.Count >= members.GroupData.MemberCount)
            {
                try { OnGroupMembers(groupMemberCache); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void GroupRoleDataHandler(Packet packet, Simulator simulator)
        {
            GroupRoleDataReplyPacket roles = (GroupRoleDataReplyPacket)packet;
            Dictionary<UUID, GroupRole> groupRoleCache = null;

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
                        groupRole.Powers = (GroupPowers)block.Powers;
                        groupRole.Title = Helpers.FieldToUTF8String(block.Title);

                        groupRoleCache[block.RoleID] = groupRole;
                    }
                }
            }

            // Check if we've received all the group members that are showing up
            if (OnGroupRoles != null && groupRoleCache != null && groupRoleCache.Count >= roles.GroupData.RoleCount)
            {
                try { OnGroupRoles(groupRoleCache); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void GroupRoleMembersHandler(Packet packet, Simulator simulator)
        {
            GroupRoleMembersReplyPacket members = (GroupRoleMembersReplyPacket)packet;
            List<KeyValuePair<UUID, UUID>> groupRoleMemberCache = null;

            try
            {
                lock (GroupRolesMembersCaches)
                {
                    // If nothing is registered to receive this RequestID drop the data
                    if (GroupRolesMembersCaches.ContainsKey(members.AgentData.RequestID))
                    {
                        groupRoleMemberCache = GroupRolesMembersCaches[members.AgentData.RequestID];

                        foreach (GroupRoleMembersReplyPacket.MemberDataBlock block in members.MemberData)
                        {
                            KeyValuePair<UUID, UUID> rolemember =
                                new KeyValuePair<UUID, UUID>(block.RoleID, block.MemberID);

                            groupRoleMemberCache.Add(rolemember);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e);
            }

            //Client.DebugLog("Pairs Ratio: " + groupRoleMemberCache.Count + "/" + members.AgentData.TotalPairs);
            
            // Check if we've received all the pairs that are showing up
            if (OnGroupRolesMembers != null && groupRoleMemberCache != null && groupRoleMemberCache.Count >= members.AgentData.TotalPairs)
            {
                try { OnGroupRolesMembers(groupRoleMemberCache); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
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
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void CreateGroupReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnGroupCreated != null)
            {
                CreateGroupReplyPacket reply = (CreateGroupReplyPacket)packet;

                string message = Helpers.FieldToUTF8String(reply.ReplyData.Message);

                try { OnGroupCreated(reply.ReplyData.GroupID, reply.ReplyData.Success, message); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void JoinGroupReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnGroupJoined != null)
            {
                JoinGroupReplyPacket reply = (JoinGroupReplyPacket)packet;

                try { OnGroupJoined(reply.GroupData.GroupID, reply.GroupData.Success); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void LeaveGroupReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnGroupLeft != null)
            {
                LeaveGroupReplyPacket reply = (LeaveGroupReplyPacket)packet;

                try { OnGroupLeft(reply.GroupData.GroupID, reply.GroupData.Success); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void UUIDGroupNameReplyHandler(Packet packet, Simulator simulator)
        {
            UUIDGroupNameReplyPacket reply = (UUIDGroupNameReplyPacket)packet;
            UUIDGroupNameReplyPacket.UUIDNameBlockBlock[] blocks = reply.UUIDNameBlock;
            
            Dictionary<UUID, string> groupNames = new Dictionary<UUID, string>();

            foreach (UUIDGroupNameReplyPacket.UUIDNameBlockBlock block in blocks) 
            {
                groupNames.Add(block.ID, Helpers.FieldToUTF8String(block.GroupName));
                    if (!GroupName2KeyCache.ContainsKey(block.ID))
                        GroupName2KeyCache.SafeAdd(block.ID, Helpers.FieldToUTF8String(block.GroupName));
            }

            if (OnGroupNames != null)
            {    
                try { OnGroupNames(groupNames); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary>
        /// Packet Handler for EjectGroupMemberReply, fired when an avatar is ejected from 
        /// a group.
        /// </summary>
        /// <param name="packet">The EjectGroupMemberReply packet</param>
        /// <param name="simulator">The simulator where the message originated</param>
        /// <remarks>This is a silly packet, it doesn't provide you with the ejectees UUID</remarks>
        private void EjectGroupMemberReplyHandler(Packet packet, Simulator simulator)
        {
            EjectGroupMemberReplyPacket reply = (EjectGroupMemberReplyPacket)packet;

            // TODO: On Success remove the member from the cache(s)
            
            if(OnGroupMemberEjected != null)
            {
                try { OnGroupMemberEjected(reply.GroupData.GroupID, reply.EjectData.Success); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        #endregion Packet Handlers
    }
}
