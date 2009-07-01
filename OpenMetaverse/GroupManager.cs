/*
 * Copyright (c) 2007-2009, openmetaverse.org
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
using OpenMetaverse.StructuredData;
using OpenMetaverse.Messages.Linden;
using OpenMetaverse.Interfaces;

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
        /// <summary>Key of the group</summary>
        public UUID GroupID;
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
        /// <summary>Key of the group</summary>
        public UUID GroupID;
        /// <summary>ID of the role title belongs to</summary>
        public UUID RoleID;
        /// <summary>Group Title</summary>
        public string Title;
        /// <summary>Whether title is Active</summary>
        public bool Selected;
        /// <summary>Returns group title</summary>
        public override string ToString()
        {
            return Title;
        }
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
        /// <summary>Show this group in agent's profile</summary>
        public bool ListInProfile;
       
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
                return Utils.EmptyBytes;
            
            OpenMetaverse.StructuredData.OSDMap att = new OpenMetaverse.StructuredData.OSDMap();
            att.Add("item_id", OpenMetaverse.StructuredData.OSD.FromUUID(AttachmentID));
            att.Add("owner_id", OpenMetaverse.StructuredData.OSD.FromUUID(OwnerID));

            return OpenMetaverse.StructuredData.OSDParser.SerializeLLSDXmlBytes(att);

            /*
            //I guess this is how this works, no gaurentees
            string lsd = "<llsd><item_id>" + AttachmentID.ToString() + "</item_id><owner_id>"
                + OwnerID.ToString() + "</owner_id></llsd>";
            return Utils.StringToBytes(lsd);
             */
        }
    }

    /// <summary>
    /// Struct representing a group notice list entry
    /// </summary>
    public struct GroupNoticeList
    {
        /// <summary>Notice ID</summary>
        public UUID NoticeID;
        /// <summary>Creation timestamp of notice</summary>
        public uint Timestamp;
        /// <summary>Agent name who created notice</summary>
        public string FromName;
        /// <summary>Notice subject</summary>
        public string Subject;
        /// <summary>Is there an attachment?</summary>
        public bool HasAttachment;
        /// <summary>Attachment Type</summary>
        public AssetType AssetType;

    }

    /// <summary>
    /// Struct representing a member of a group chat session and their settings
    /// </summary>
    public struct ChatSessionMember
    {
        /// <summary>The <see cref="UUID"/> of the Avatar</summary>
        public UUID AvatarKey;
        /// <summary>True if user has voice chat enabled</summary>
        public bool CanVoiceChat;
        /// <summary>True of Avatar has moderator abilities</summary>
        public bool IsModerator;
        /// <summary>True if a moderator has muted this avatars chat</summary>
        public bool MuteText;
        /// <summary>True if a moderator has muted this avatars voice</summary>
        public bool MuteVoice;
    }

    #endregion Structs

    #region Enums

    /// <summary>
    /// Role update flags
    /// </summary>
    public enum GroupRoleUpdate : uint
    {
        /// <summary></summary>
        NoUpdate,
        /// <summary></summary>
        UpdateData,
        /// <summary></summary>
        UpdatePowers,
        /// <summary></summary>
        UpdateAll,
        /// <summary></summary>
        Create,
        /// <summary></summary>
        Delete
    }

    [Flags]
    public enum GroupPowers : ulong
    {
        /// <summary></summary>
        None = 0,

        // Membership
        /// <summary>Can send invitations to groups default role</summary>
        Invite = 1UL << 1,
        /// <summary>Can eject members from group</summary>
        Eject = 1UL << 2,
        /// <summary>Can toggle 'Open Enrollment' and change 'Signup fee'</summary>
        ChangeOptions = 1UL << 3,
        /// <summary>Member is visible in the public member list</summary>
        MemberVisible = 1UL << 47,

        // Roles
        /// <summary>Can create new roles</summary>
        CreateRole = 1UL << 4,
        /// <summary>Can delete existing roles</summary>
        DeleteRole = 1UL << 5,
        /// <summary>Can change Role names, titles and descriptions</summary>
        RoleProperties = 1UL << 6,
        /// <summary>Can assign other members to assigners role</summary>
        AssignMemberLimited = 1UL << 7,
        /// <summary>Can assign other members to any role</summary>
        AssignMember = 1UL << 8,
        /// <summary>Can remove members from roles</summary>
        RemoveMember = 1UL << 9,
        /// <summary>Can assign and remove abilities in roles</summary>
        ChangeActions = 1UL << 10,

        // Identity
        /// <summary>Can change group Charter, Insignia, 'Publish on the web' and which
        /// members are publicly visible in group member listings</summary>
        ChangeIdentity = 1UL << 11,

        // Parcel management
        /// <summary>Can buy land or deed land to group</summary>
        LandDeed = 1UL << 12,
        /// <summary>Can abandon group owned land to Governor Linden on mainland, or Estate owner for
        /// private estates</summary>
        LandRelease = 1UL << 13,
        /// <summary>Can set land for-sale information on group owned parcels</summary>
        LandSetSale = 1UL << 14,
        /// <summary>Can subdivide and join parcels</summary>
        LandDivideJoin = 1UL << 15,


        // Chat
        /// <summary>Can join group chat sessions</summary>
        JoinChat = 1UL << 16,
        /// <summary>Can use voice chat in Group Chat sessions</summary>
        AllowVoiceChat = 1UL << 27,
        /// <summary>Can moderate group chat sessions</summary>
        ModerateChat = 1UL << 37,

        // Parcel identity
        /// <summary>Can toggle "Show in Find Places" and set search category</summary>
        FindPlaces = 1UL << 17,
        /// <summary>Can change parcel name, description, and 'Publish on web' settings</summary>
        LandChangeIdentity = 1UL << 18,
        /// <summary>Can set the landing point and teleport routing on group land</summary>
        SetLandingPoint = 1UL << 19,

        // Parcel settings
        /// <summary>Can change music and media settings</summary>
        ChangeMedia = 1UL << 20,
        /// <summary>Can toggle 'Edit Terrain' option in Land settings</summary>
        LandEdit = 1UL << 21,
        /// <summary>Can toggle various About Land > Options settings</summary>
        LandOptions = 1UL << 22,

        // Parcel powers
        /// <summary>Can always terraform land, even if parcel settings have it turned off</summary>
        AllowEditLand = 1UL << 23,
        /// <summary>Can always fly while over group owned land</summary>
        AllowFly = 1UL << 24,
        /// <summary>Can always rez objects on group owned land</summary>
        AllowRez = 1UL << 25,
        /// <summary>Can always create landmarks for group owned parcels</summary>
        AllowLandmark = 1UL << 26,
        /// <summary>Can set home location on any group owned parcel</summary>
        AllowSetHome = 1UL << 28,


        // Parcel access
        /// <summary>Can modify public access settings for group owned parcels</summary>
        LandManageAllowed = 1UL << 29,
        /// <summary>Can manager parcel ban lists on group owned land</summary>
        LandManageBanned = 1UL << 30,
        /// <summary>Can manage pass list sales information</summary>
        LandManagePasses = 1UL << 31,
        /// <summary>Can eject and freeze other avatars on group owned land</summary>
        LandEjectAndFreeze = 1UL << 32,

        // Parcel content
        /// <summary>Can return objects set to group</summary>
        ReturnGroupSet = 1UL << 33,
        /// <summary>Can return non-group owned/set objects</summary>
        ReturnNonGroup = 1UL << 34,
        /// <summary>Can return group owned objects</summary>
        ReturnGroupOwned = 1UL << 48,

        /// <summary>Can landscape using Linden plants</summary>
        LandGardening = 1UL << 35,

        // Objects
        /// <summary>Can deed objects to group</summary>
        DeedObject = 1UL << 36,
        /// <summary>Can move group owned objects</summary>
        ObjectManipulate = 1UL << 38,
        /// <summary>Can set group owned objects for-sale</summary>
        ObjectSetForSale = 1UL << 39,

        /// <summary>Pay group liabilities and receive group dividends</summary>
        Accountable = 1UL << 40,

        // Notices and proposals
        /// <summary>Can send group notices</summary>
        SendNotices = 1UL << 42,
        /// <summary>Can receive group notices</summary>
        ReceiveNotices = 1UL << 43,
        /// <summary>Can create group proposals</summary>
        StartProposal = 1UL << 44,
        /// <summary>Can vote on group proposals</summary>
        VoteOnProposal = 1UL << 45
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
        /// <param name="groups">A dictionary containing the groups an avatar is a member of,
        /// where the Key is the group <seealso cref="UUID"/>, and the values are the groups</param>
        public delegate void CurrentGroupsCallback(Dictionary<UUID, Group> groups);

        /// <summary>
        /// Callback for a list of group names
        /// </summary>
        /// <param name="groupNames">A dictionary containing the the group names requested
        /// where the Key is the group <seealso cref="UUID"/>, and the values are the names</param>
        public delegate void GroupNamesCallback(Dictionary<UUID, string> groupNames);

        /// <summary>
        /// Callback for the profile of a group
        /// </summary>
        /// <param name="group">The group profile</param>
        public delegate void GroupProfileCallback(Group group);

        /// <summary>
        /// Callback for the member list of a group
        /// </summary>
        /// <param name="requestID"><seealso cref="UUID"/> returned by RequestGroupMembers</param>
        /// <param name="groupID"><seealso cref="UUID"/> of the group</param>
        /// <param name="members">A dictionary containing the members of a group
        /// where key is member <seealso cref="UUID"/> and value is <seealso cref="GroupMember"/> struct</param>
        public delegate void GroupMembersCallback(UUID requestID, UUID groupID, Dictionary<UUID, GroupMember> members);

        /// <summary>
        /// Callback for retrieving group roles
        /// </summary>
        /// <param name="requestID"><seealso cref="UUID"/> of the request returned from RequestGroupRoles</param>
        /// <param name="groupID"><seealso cref="UUID"/> of the group</param>
        /// <param name="roles">A dictionary containing role <seealso cref="UUID"/>s as the key
        /// and <seealso cref="GroupRole"/> structs as values</param>
        public delegate void GroupRolesCallback(UUID requestID, UUID groupID, Dictionary<UUID, GroupRole> roles);

        /// <summary>
        /// Callback for a pairing of roles to members
        /// </summary>
        /// <param name="requestID"><seealso cref="UUID"/> of the request returned from RequestGroupRolesMembers</param>
        /// <param name="groupID"><seealso cref="UUID"/> of the group</param>
        /// <param name="rolesMembers">List containing role/member pairs</param>
        public delegate void GroupRolesMembersCallback(UUID requestID, UUID groupID, List<KeyValuePair<UUID, UUID>> rolesMembers);

        /// <summary>
        /// Callback for the title list of a group
        /// </summary>
        /// <param name="requestID"><seealso cref="UUID"/> of the request returned from RequestGroupTitles</param>
        /// <param name="groupID">Group <seealso cref="UUID"/></param>
        /// <param name="titles">A dictionary containing the titles of a group
        /// where the Key is the role <seealso cref="UUID"/>, and the values are the title details</param>
        public delegate void GroupTitlesCallback(UUID requestID, UUID groupID, Dictionary<UUID, GroupTitle> titles);

        /// <summary>
        /// Callback fired when group account summary information is received
        /// </summary>
        /// <param name="groupID">Group <seealso cref="UUID"/></param>
        /// <param name="summary">The group account summary information</param>
        public delegate void GroupAccountSummaryCallback(UUID groupID, GroupAccountSummary summary);

        /// <summary>
        /// Callback fired after an attempt to create a group
        /// </summary>
        /// <param name="groupID">The new groups <seealso cref="UUID"/></param>
        /// <param name="success">True of creation was successful</param>
        /// <param name="message">A string, containing a message from the simulator</param>
        public delegate void GroupCreatedCallback(UUID groupID, bool success, string message);

        /// <summary>
        /// Callback fired when the avatar has joined a group
        /// </summary>
        /// <param name="groupID">The <see cref="UUID"/> of the group joined</param>
        /// <param name="success">True if the join was successful</param>
        public delegate void GroupJoinedCallback(UUID groupID, bool success);

        /// <summary>
        /// Callback fired when the avatar leaves a group
        /// </summary>
        /// <param name="groupID">The <see cref="UUID"/> of the group joined</param>
        /// <param name="success">True if the part was successful</param>
        public delegate void GroupLeftCallback(UUID groupID, bool success);

        /// <summary>
        /// Fired when a group is dropped, likely because it did not keep the required (2) avatar
        /// minimum
        /// </summary>
        /// <param name="groupID">The <see cref="UUID"/> of the group which was dropped</param>
        public delegate void GroupDroppedCallback(UUID groupID);

        /// <summary>
        /// Fired when a member of a group is ejected, 
        /// Does not provide member information, only 
        /// group ID and whether it was successful or not
        /// </summary>
        /// <param name="groupID">The Group UUID the member was ejected from</param>
        /// <param name="success">true of member was successfully ejected</param>
        public delegate void GroupMemberEjectedCallback(UUID groupID, bool success);

        /// <summary>
        /// Fired when the list of group notices is recievied
        /// </summary>
        /// <param name="groupID">The <see cref="UUID"/> of the group for which the notice list entry was recievied</param>
        /// <param name="notice">The Notice list entry</param>
        public delegate void GroupNoticesListCallback(UUID groupID, GroupNoticeList notice);

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
        /// <summary>Fired when the list of group notices is recievied</summary>
        public event GroupNoticesListCallback OnGroupNoticesList;

        #endregion Events

        /// <summary>A reference to the current <seealso cref="GridClient"/> instance</summary>
        private GridClient Client;
        /// <summary>Currently-active group members requests</summary>
        private List<UUID> GroupMembersRequests;
        /// <summary>Currently-active group roles requests</summary>
        private List<UUID> GroupRolesRequests;
        /// <summary>Currently-active group role-member requests</summary>
        private List<UUID> GroupRolesMembersRequests;
        /// <summary>Dictionary keeping group members while request is in progress</summary>
        private InternalDictionary<UUID, Dictionary<UUID, GroupMember>> TempGroupMembers;
        /// <summary>Dictionary keeping mebmer/role mapping while request is in progress</summary>
        private InternalDictionary<UUID, List<KeyValuePair<UUID, UUID>>> TempGroupRolesMembers;
        /// <summary>Dictionary keeping GroupRole information while request is in progress</summary>
        private InternalDictionary<UUID, Dictionary<UUID, GroupRole>> TempGroupRoles;
        /// <summary>Caches group name lookups</summary>
        public InternalDictionary<UUID, string> GroupName2KeyCache;
        /// <summary>
        /// Group Management Routines, Methods and Packet Handlers
        /// </summary>
        /// <param name="client">A reference to the current <seealso cref="GridClient"/> instance</param>
        public GroupManager(GridClient client)
        {
            Client = client;

            TempGroupMembers = new InternalDictionary<UUID, Dictionary<UUID, GroupMember>>();
            GroupMembersRequests = new List<UUID>();
            TempGroupRoles = new InternalDictionary<UUID, Dictionary<UUID, GroupRole>>();
            GroupRolesRequests = new List<UUID>();
            TempGroupRolesMembers = new InternalDictionary<UUID, List<KeyValuePair<UUID, UUID>>>();
            GroupRolesMembersRequests = new List<UUID>();
            GroupName2KeyCache  = new InternalDictionary<UUID, string>();

            Client.Network.RegisterEventCallback("AgentGroupDataUpdate", new Caps.EventQueueCallback(AgentGroupDataUpdateHandler));
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
            Client.Network.RegisterCallback(PacketType.GroupNoticesListReply, new NetworkManager.PacketCallback(GroupNoticesListReplyHandler));
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
        /// <returns>UUID of the request, use to index into cache</returns>
        public UUID RequestGroupMembers(UUID group)
        {
            UUID requestID = UUID.Random();
            lock (GroupMembersRequests) GroupMembersRequests.Add(requestID);

            GroupMembersRequestPacket request = new GroupMembersRequestPacket();

            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.GroupData.GroupID = group;
            request.GroupData.RequestID = requestID;

            Client.Network.SendPacket(request);
            return requestID;
        }

        /// <summary>Request group roles</summary>
        /// <remarks>Subscribe to <code>OnGroupRoles</code> event to receive the results.</remarks>
        /// <param name="group">group ID (UUID)</param>
        /// <returns>UUID of the request, use to index into cache</returns>
        public UUID RequestGroupRoles(UUID group)
        {
            UUID requestID = UUID.Random();
            lock (GroupRolesRequests) GroupRolesRequests.Add(requestID);

            GroupRoleDataRequestPacket request = new GroupRoleDataRequestPacket();

            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.GroupData.GroupID = group;
            request.GroupData.RequestID = requestID;

            Client.Network.SendPacket(request);
            return requestID;
        }

        /// <summary>Request members (members,role) role mapping for a group.</summary>
        /// <remarks>Subscribe to <code>OnGroupRolesMembers</code> event to receive the results.</remarks>
        /// <param name="group">group ID (UUID)</param>
        /// <returns>UUID of the request, use to index into cache</returns>
        public UUID RequestGroupRoleMembers(UUID group)
        {
            UUID requestID = UUID.Random();
            lock (GroupRolesRequests) GroupRolesMembersRequests.Add(requestID);

            GroupRoleMembersRequestPacket request = new GroupRoleMembersRequestPacket();
            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.GroupData.GroupID = group;
            request.GroupData.RequestID = requestID;
            Client.Network.SendPacket(request);
            return requestID;
        }

        /// <summary>Request a groups Titles</summary>
        /// <remarks>Subscribe to <code>OnGroupTitles</code> event to receive the results.</remarks>
        /// <param name="group">group ID (UUID)</param>
        /// <returns>UUID of the request, use to index into cache</returns>
        public UUID RequestGroupTitles(UUID group)
        {
            UUID requestID = UUID.Random();

            GroupTitlesRequestPacket request = new GroupTitlesRequestPacket();

            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.AgentData.GroupID = group;
            request.AgentData.RequestID = requestID;

            Client.Network.SendPacket(request);
            return requestID;
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

        /// <summary>
        /// Save wheather agent wants to accept group notices and list this group in their profile
        /// </summary>
        /// <param name="groupID">Group <see cref="UUID"/></param>
        /// <param name="acceptNotices">Accept notices from this group</param>
        /// <param name="listInProfile">List this group in the profile</param>
        public void SetGroupAcceptNotices(UUID groupID, bool acceptNotices, bool listInProfile)
        {
            SetGroupAcceptNoticesPacket p = new SetGroupAcceptNoticesPacket();
            p.AgentData.AgentID = Client.Self.AgentID;
            p.AgentData.SessionID = Client.Self.SessionID;
            p.Data.GroupID = groupID;
            p.Data.AcceptNotices = acceptNotices;
            p.NewData.ListInProfile = listInProfile;

            Client.Network.SendPacket(p);
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
            cgrp.AgentData = new CreateGroupRequestPacket.AgentDataBlock();
            cgrp.AgentData.AgentID = Client.Self.AgentID;
            cgrp.AgentData.SessionID = Client.Self.SessionID;

            cgrp.GroupData = new CreateGroupRequestPacket.GroupDataBlock();
            cgrp.GroupData.AllowPublish = group.AllowPublish;
            cgrp.GroupData.Charter = Utils.StringToBytes(group.Charter);
            cgrp.GroupData.InsigniaID = group.InsigniaID;
            cgrp.GroupData.MaturePublish = group.MaturePublish;
            cgrp.GroupData.MembershipFee = group.MembershipFee;
            cgrp.GroupData.Name = Utils.StringToBytes(group.Name);
            cgrp.GroupData.OpenEnrollment = group.OpenEnrollment;
            cgrp.GroupData.ShowInList = group.ShowInList;

            Client.Network.SendPacket(cgrp);
        }

        /// <summary>Update a group's profile and other information</summary>
        /// <param name="id">Groups ID (UUID) to update.</param>
        /// <param name="group">Group struct to update.</param>
        public void UpdateGroup(UUID id, Group group)
        {
            OpenMetaverse.Packets.UpdateGroupInfoPacket cgrp = new UpdateGroupInfoPacket();
            cgrp.AgentData = new UpdateGroupInfoPacket.AgentDataBlock();
            cgrp.AgentData.AgentID = Client.Self.AgentID;
            cgrp.AgentData.SessionID = Client.Self.SessionID;
            
            cgrp.GroupData = new UpdateGroupInfoPacket.GroupDataBlock();
            cgrp.GroupData.GroupID = id;
            cgrp.GroupData.AllowPublish = group.AllowPublish;
            cgrp.GroupData.Charter = Utils.StringToBytes(group.Charter);
            cgrp.GroupData.InsigniaID = group.InsigniaID;
            cgrp.GroupData.MaturePublish = group.MaturePublish;
            cgrp.GroupData.MembershipFee = group.MembershipFee;
            cgrp.GroupData.OpenEnrollment = group.OpenEnrollment;
            cgrp.GroupData.ShowInList = group.ShowInList;
            
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
            
            eject.GroupData = new EjectGroupMemberRequestPacket.GroupDataBlock();
            eject.GroupData.GroupID = group;
            
            eject.EjectData = new EjectGroupMemberRequestPacket.EjectDataBlock[1];
            eject.EjectData[0] = new EjectGroupMemberRequestPacket.EjectDataBlock();
            eject.EjectData[0].EjecteeID = member;
            
            Client.Network.SendPacket(eject);
        }

        /// <summary>Update role information</summary>
        /// <param name="role">Modified role to be updated</param>
        public void UpdateRole(GroupRole role)
        {
            OpenMetaverse.Packets.GroupRoleUpdatePacket gru = new GroupRoleUpdatePacket();
            gru.AgentData.AgentID = Client.Self.AgentID;
            gru.AgentData.SessionID = Client.Self.SessionID;
            gru.AgentData.GroupID = role.GroupID;
            gru.RoleData = new GroupRoleUpdatePacket.RoleDataBlock[1];
            gru.RoleData[0] = new GroupRoleUpdatePacket.RoleDataBlock();
            gru.RoleData[0].Name = Utils.StringToBytes(role.Name);
            gru.RoleData[0].Description = Utils.StringToBytes(role.Description);
            gru.RoleData[0].Powers = (ulong)role.Powers;
            gru.RoleData[0].RoleID = role.ID;
            gru.RoleData[0].Title = Utils.StringToBytes(role.Title);
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
            gru.RoleData[0].Name = Utils.StringToBytes(role.Name);
            gru.RoleData[0].Description = Utils.StringToBytes(role.Description);
            gru.RoleData[0].Powers = (ulong)role.Powers;
            gru.RoleData[0].Title = Utils.StringToBytes(role.Title);
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
            //1 = Remove From Role TODO: this should be in an enum
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
            //0 = Add to Role TODO: this should be in an enum
            grc.RoleChange[0].Change = 0;
            Client.Network.SendPacket(grc);
        }

        /// <summary>Request the group notices list</summary>
        /// <param name="group">Group ID to fetch notices for</param>
        public void RequestGroupNoticeList(UUID group)
        {
            OpenMetaverse.Packets.GroupNoticesListRequestPacket gnl = new GroupNoticesListRequestPacket();
            gnl.AgentData.AgentID = Client.Self.AgentID;
            gnl.AgentData.SessionID = Client.Self.SessionID;
            gnl.Data.GroupID = group;
            Client.Network.SendPacket(gnl);
        }

        /// <summary>Request a group notice by key</summary>
        /// <param name="noticeID">ID of group notice</param>
        public void RequestGroupNotice(UUID noticeID)
        {
            OpenMetaverse.Packets.GroupNoticeRequestPacket gnr = new GroupNoticeRequestPacket();
            gnr.AgentData.AgentID = Client.Self.AgentID;
            gnr.AgentData.SessionID = Client.Self.SessionID;
            gnr.Data.GroupNoticeID = noticeID;
            Client.Network.SendPacket(gnr);
        }


        private void GroupNoticesListReplyHandler(Packet packet, Simulator simulator)
        {
            GroupNoticesListReplyPacket reply = (GroupNoticesListReplyPacket)packet;
           
            foreach (GroupNoticesListReplyPacket.DataBlock entry in reply.Data)
            {
                GroupNoticeList notice = new GroupNoticeList();
                notice.FromName = Utils.BytesToString(entry.FromName);
                notice.Subject = Utils.BytesToString(entry.Subject);
                notice.NoticeID = entry.NoticeID;
                notice.Timestamp = entry.Timestamp;
                notice.HasAttachment = entry.HasAttachment;
                notice.AssetType = (AssetType)entry.AssetType;

                if (OnGroupNoticesList != null)
                {
                    try { OnGroupNoticesList(reply.AgentData.GroupID, notice); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                } 
            } 
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
            p.ProposalData.ProposalText = Utils.StringToBytes(prop.VoteText);
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

        private void AgentGroupDataUpdateHandler(string capsKey, IMessage message, Simulator simulator)
        {
            if (OnCurrentGroups != null)
            {
                AgentGroupDataUpdateMessage msg = (AgentGroupDataUpdateMessage)message;

                Dictionary<UUID, Group> currentGroups = new Dictionary<UUID, Group>();
                for (int i = 0; i < msg.GroupDataBlock.Length; i++)
                {
                    Group group = new Group();
                    group.ID = msg.GroupDataBlock[i].GroupID;
                    group.InsigniaID = msg.GroupDataBlock[i].GroupInsigniaID;
                    group.Name = msg.GroupDataBlock[i].GroupName;
                    group.Contribution = msg.GroupDataBlock[i].Contribution;
                    group.AcceptNotices = msg.GroupDataBlock[i].AcceptNotices;
                    group.Powers = msg.GroupDataBlock[i].GroupPowers;
                    group.ListInProfile = msg.NewGroupDataBlock[i].ListInProfile;

                    currentGroups.Add(group.ID, group);

                    lock (GroupName2KeyCache.Dictionary)
                    {
                        if (!GroupName2KeyCache.Dictionary.ContainsKey(group.ID))
                            GroupName2KeyCache.Dictionary.Add(group.ID, group.Name);
                    }
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
                group.Charter = Utils.BytesToString(profile.GroupData.Charter);
                group.FounderID = profile.GroupData.FounderID;
                group.GroupMembershipCount = profile.GroupData.GroupMembershipCount;
                group.GroupRolesCount = profile.GroupData.GroupRolesCount;
                group.InsigniaID = profile.GroupData.InsigniaID;
                group.MaturePublish = profile.GroupData.MaturePublish;
                group.MembershipFee = profile.GroupData.MembershipFee;
                group.MemberTitle = Utils.BytesToString(profile.GroupData.MemberTitle);
                group.Money = profile.GroupData.Money;
                group.Name = Utils.BytesToString(profile.GroupData.Name);
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

                    groupTitle.GroupID = titles.AgentData.GroupID;
                    groupTitle.RoleID = block.RoleID;
                    groupTitle.Title = Utils.BytesToString(block.Title);
                    groupTitle.Selected = block.Selected;

                    groupTitleCache[block.RoleID] = groupTitle;
                }

                try { OnGroupTitles(titles.AgentData.RequestID, titles.AgentData.GroupID, groupTitleCache); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void GroupMembersHandler(Packet packet, Simulator simulator)
        {
            GroupMembersReplyPacket members = (GroupMembersReplyPacket)packet;
            Dictionary<UUID, GroupMember> groupMemberCache = null;

            lock (GroupMembersRequests)
            {
                // If nothing is registered to receive this RequestID drop the data
                if (GroupMembersRequests.Contains(members.GroupData.RequestID))
                {
                    lock (TempGroupMembers.Dictionary)
                    {
                        if (!TempGroupMembers.TryGetValue(members.GroupData.RequestID, out groupMemberCache))
                        {
                            groupMemberCache = new Dictionary<UUID, GroupMember>();
                            TempGroupMembers[members.GroupData.RequestID] = groupMemberCache;
                        }

                        foreach (GroupMembersReplyPacket.MemberDataBlock block in members.MemberData)
                        {
                            GroupMember groupMember = new GroupMember();

                            groupMember.ID = block.AgentID;
                            groupMember.Contribution = block.Contribution;
                            groupMember.IsOwner = block.IsOwner;
                            groupMember.OnlineStatus = Utils.BytesToString(block.OnlineStatus);
                            groupMember.Powers = (GroupPowers)block.AgentPowers;
                            groupMember.Title = Utils.BytesToString(block.Title);

                            groupMemberCache[block.AgentID] = groupMember;
                        }

                        if (groupMemberCache.Count >= members.GroupData.MemberCount)
                        {
                            GroupMembersRequests.Remove(members.GroupData.RequestID);
                            TempGroupMembers.Remove(members.GroupData.RequestID);
                        }
                    }
                }
            }

            if (OnGroupMembers != null && groupMemberCache != null && groupMemberCache.Count >= members.GroupData.MemberCount)
            {
                try { OnGroupMembers(members.GroupData.RequestID, members.GroupData.GroupID, groupMemberCache); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void GroupRoleDataHandler(Packet packet, Simulator simulator)
        {
            GroupRoleDataReplyPacket roles = (GroupRoleDataReplyPacket)packet;
            Dictionary<UUID, GroupRole> groupRoleCache = null;

            lock (GroupRolesRequests)
            {
                // If nothing is registered to receive this RequestID drop the data
                if (GroupRolesRequests.Contains(roles.GroupData.RequestID))
                {
                    GroupRolesRequests.Remove(roles.GroupData.RequestID);

                    lock (TempGroupRoles.Dictionary)
                    {
                        if (!TempGroupRoles.TryGetValue(roles.GroupData.RequestID, out groupRoleCache))
                        {
                            groupRoleCache = new Dictionary<UUID, GroupRole>();
                            TempGroupRoles[roles.GroupData.RequestID] = groupRoleCache;
                        }

                        foreach (GroupRoleDataReplyPacket.RoleDataBlock block in roles.RoleData)
                        {
                            GroupRole groupRole = new GroupRole();

                            groupRole.GroupID = roles.GroupData.GroupID;
                            groupRole.ID = block.RoleID;
                            groupRole.Description = Utils.BytesToString(block.Description);
                            groupRole.Name = Utils.BytesToString(block.Name);
                            groupRole.Powers = (GroupPowers)block.Powers;
                            groupRole.Title = Utils.BytesToString(block.Title);

                            groupRoleCache[block.RoleID] = groupRole;
                        }

                        if (groupRoleCache.Count >= roles.GroupData.RoleCount)
                        {
                            GroupRolesRequests.Remove(roles.GroupData.RequestID);
                            TempGroupRoles.Remove(roles.GroupData.RequestID);
                        }
                    }
                }
            }

            if (OnGroupRoles != null && groupRoleCache != null && groupRoleCache.Count >= roles.GroupData.RoleCount)
            {
                try { OnGroupRoles(roles.GroupData.RequestID, roles.GroupData.GroupID, groupRoleCache); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void GroupRoleMembersHandler(Packet packet, Simulator simulator)
        {
            GroupRoleMembersReplyPacket members = (GroupRoleMembersReplyPacket)packet;
            List<KeyValuePair<UUID, UUID>> groupRoleMemberCache = null;

            try
            {
                lock (GroupRolesMembersRequests)
                {
                    // If nothing is registered to receive this RequestID drop the data
                    if (GroupRolesMembersRequests.Contains(members.AgentData.RequestID))
                    {
                        lock (TempGroupRolesMembers.Dictionary)
                        {
                            if (!TempGroupRolesMembers.TryGetValue(members.AgentData.RequestID, out groupRoleMemberCache))
                            {
                                groupRoleMemberCache = new List<KeyValuePair<UUID, UUID>>();
                                TempGroupRolesMembers[members.AgentData.RequestID] = groupRoleMemberCache;
                            }

                            foreach (GroupRoleMembersReplyPacket.MemberDataBlock block in members.MemberData)
                            {
                                KeyValuePair<UUID, UUID> rolemember =
                                    new KeyValuePair<UUID, UUID>(block.RoleID, block.MemberID);

                                groupRoleMemberCache.Add(rolemember);
                            }

                            if (groupRoleMemberCache.Count >= members.AgentData.TotalPairs)
                            {
                                GroupRolesMembersRequests.Remove(members.AgentData.RequestID);
                                TempGroupRolesMembers.Remove(members.AgentData.RequestID);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e);
            }

            if (OnGroupRolesMembers != null && groupRoleMemberCache != null && groupRoleMemberCache.Count >= members.AgentData.TotalPairs)
            {
                try { OnGroupRolesMembers(members.AgentData.RequestID, members.AgentData.GroupID, groupRoleMemberCache); }
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
                account.LastTaxDate = Utils.BytesToString(summary.MoneyData.LastTaxDate);
                account.LightTaxCurrent = summary.MoneyData.LightTaxCurrent;
                account.LightTaxEstimate = summary.MoneyData.LightTaxEstimate;
                account.NonExemptMembers = summary.MoneyData.NonExemptMembers;
                account.ObjectTaxCurrent = summary.MoneyData.ObjectTaxCurrent;
                account.ObjectTaxEstimate = summary.MoneyData.ObjectTaxEstimate;
                account.ParcelDirFeeCurrent = summary.MoneyData.ParcelDirFeeCurrent;
                account.ParcelDirFeeEstimate = summary.MoneyData.ParcelDirFeeEstimate;
                account.StartDate = Utils.BytesToString(summary.MoneyData.StartDate);
                account.TaxDate = Utils.BytesToString(summary.MoneyData.TaxDate);
                account.TotalCredits = summary.MoneyData.TotalCredits;
                account.TotalDebits = summary.MoneyData.TotalDebits;

                try { OnGroupAccountSummary(summary.AgentData.GroupID, account); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void CreateGroupReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnGroupCreated != null)
            {
                CreateGroupReplyPacket reply = (CreateGroupReplyPacket)packet;

                string message = Utils.BytesToString(reply.ReplyData.Message);

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
                groupNames.Add(block.ID, Utils.BytesToString(block.GroupName));
                    if (!GroupName2KeyCache.ContainsKey(block.ID))
                        GroupName2KeyCache.Add(block.ID, Utils.BytesToString(block.GroupName));
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
