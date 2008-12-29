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
using System.Text;
using System.Reflection;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Http;
using OpenMetaverse.Packets;

namespace OpenMetaverse
{
    #region Enums

    /// <summary>
    /// Permission request flags, asked when a script wants to control an Avatar
    /// </summary>
    [Flags]
    public enum ScriptPermission : int
    {
        /// <summary>Placeholder for empty values, shouldn't ever see this</summary>
        None = 0,
        /// <summary>Script wants ability to take money from you</summary>
        Debit = 1 << 1,
        /// <summary>Script wants to take camera controls for you</summary>
        TakeControls = 1 << 2,
        /// <summary>Script wants to remap avatars controls</summary>
        RemapControls = 1 << 3,
        /// <summary>Script wants to trigger avatar animations</summary>
        /// <remarks>This function is not implemented on the grid</remarks>
        TriggerAnimation = 1 << 4,
        /// <summary>Script wants to attach or detach the prim or primset to your avatar</summary>
        Attach = 1 << 5,
        /// <summary>Script wants permission to release ownership</summary>
        /// <remarks>This function is not implemented on the grid
        /// The concept of "public" objects does not exist anymore.</remarks>
        ReleaseOwnership = 1 << 6,
        /// <summary>Script wants ability to link/delink with other prims</summary>
        ChangeLinks = 1 << 7,
        /// <summary>Script wants permission to change joints</summary>
        /// <remarks>This function is not implemented on the grid</remarks>
        ChangeJoints = 1 << 8,
        /// <summary>Script wants permissions to change permissions</summary>
        /// <remarks>This function is not implemented on the grid</remarks>
        ChangePermissions = 1 << 9,
        /// <summary>Script wants to track avatars camera position and rotation </summary>
        TrackCamera = 1 << 10,
        /// <summary>Script wants to control your camera</summary>
        ControlCamera = 1 << 11
    }

    /// <summary>
    /// Special commands used in Instant Messages
    /// </summary>
    public enum InstantMessageDialog : byte
    {
        /// <summary>Indicates a regular IM from another agent</summary>
        MessageFromAgent = 0,
        /// <summary>Simple notification box with an OK button</summary>
        MessageBox = 1,
        // <summary>Used to show a countdown notification with an OK
        // button, deprecated now</summary>
        //[Obsolete]
        //MessageBoxCountdown = 2,
        /// <summary>You've been invited to join a group.</summary>
        GroupInvitation = 3,
        /// <summary>Inventory offer</summary>
        InventoryOffered = 4,
        /// <summary>Accepted inventory offer</summary>
        InventoryAccepted = 5,
        /// <summary>Declined inventory offer</summary>
        InventoryDeclined = 6,
        /// <summary>Group vote</summary>
        GroupVote = 7,
        // <summary>A message to everyone in the agent's group, no longer
        // used</summary>
        //[Obsolete]
        //DeprecatedGroupMessage = 8,
        /// <summary>An object is offering its inventory</summary>
        TaskInventoryOffered = 9,
        /// <summary>Accept an inventory offer from an object</summary>
        TaskInventoryAccepted = 10,
        /// <summary>Decline an inventory offer from an object</summary>
        TaskInventoryDeclined = 11,
        /// <summary>Unknown</summary>
        NewUserDefault = 12,
        /// <summary>Start a session, or add users to a session</summary>
        SessionAdd = 13,
        /// <summary>Start a session, but don't prune offline users</summary>
        SessionOfflineAdd = 14,
        /// <summary>Start a session with your group</summary>
        SessionGroupStart = 15,
        /// <summary>Start a session without a calling card (finder or objects)</summary>
        SessionCardlessStart = 16,
        /// <summary>Send a message to a session</summary>
        SessionSend = 17,
        /// <summary>Leave a session</summary>
        SessionDrop = 18,
        /// <summary>Indicates that the IM is from an object</summary>
        MessageFromObject = 19,
        /// <summary>Sent an IM to a busy user, this is the auto response</summary>
        BusyAutoResponse = 20,
        /// <summary>Shows the message in the console and chat history</summary>
        ConsoleAndChatHistory = 21,
        /// <summary>Send a teleport lure</summary>
        RequestTeleport = 22,
        /// <summary>Response sent to the agent which inititiated a teleport invitation</summary>
        AcceptTeleport = 23,
        /// <summary>Response sent to the agent which inititiated a teleport invitation</summary>
        DenyTeleport = 24,
        /// <summary>Only useful if you have Linden permissions</summary>
        GodLikeRequestTeleport = 25,
        /// <summary>A placeholder type for future expansion, currently not
        /// used</summary>
        CurrentlyUnused = 26,
        // <summary>Notification of a new group election, this is 
        // deprecated</summary>
        //[Obsolete]
        //DeprecatedGroupElection = 27,
        /// <summary>IM to tell the user to go to an URL</summary>
        GotoUrl = 28,
        /// <summary>IM for help</summary>
        Session911Start = 29,
        /// <summary>IM sent automatically on call for help, sends a lure 
        /// to each Helper reached</summary>
        Lure911 = 30,
        /// <summary>Like an IM but won't go to email</summary>
        FromTaskAsAlert = 31,
        /// <summary>IM from a group officer to all group members</summary>
        GroupNotice = 32,
        /// <summary>Unknown</summary>
        GroupNoticeInventoryAccepted = 33,
        /// <summary>Unknown</summary>
        GroupNoticeInventoryDeclined = 34,
        /// <summary>Accept a group invitation</summary>
        GroupInvitationAccept = 35,
        /// <summary>Decline a group invitation</summary>
        GroupInvitationDecline = 36,
        /// <summary>Unknown</summary>
        GroupNoticeRequested = 37,
        /// <summary>An avatar is offering you friendship</summary>
        FriendshipOffered = 38,
        /// <summary>An avatar has accepted your friendship offer</summary>
        FriendshipAccepted = 39,
        /// <summary>An avatar has declined your friendship offer</summary>
        FriendshipDeclined = 40,
        /// <summary>Indicates that a user has started typing</summary>
        StartTyping = 41,
        /// <summary>Indicates that a user has stopped typing</summary>
        StopTyping = 42
    }

    /// <summary>
    /// Flag in Instant Messages, whether the IM should be delivered to
    /// offline avatars as well
    /// </summary>
    public enum InstantMessageOnline
    {
        /// <summary>Only deliver to online avatars</summary>
        Online = 0,
        /// <summary>If the avatar is offline the message will be held until
        /// they login next, and possibly forwarded to their e-mail account</summary>
        Offline = 1
    }

    /// <summary>
    /// Conversion type to denote Chat Packet types in an easier-to-understand format
    /// </summary>
    public enum ChatType : byte
    {
        /// <summary>Whisper (5m radius)</summary>
        Whisper = 0,
        /// <summary>Normal chat (10/20m radius), what the official viewer typically sends</summary>
        Normal = 1,
        /// <summary>Shouting! (100m radius)</summary>
        Shout = 2,
        // <summary>Say chat (10/20m radius) - The official viewer will 
        // print "[4:15] You say, hey" instead of "[4:15] You: hey"</summary>
        //[Obsolete]
        //Say = 3,
        /// <summary>Event message when an Avatar has begun to type</summary>
        StartTyping = 4,
        /// <summary>Event message when an Avatar has stopped typing</summary>
        StopTyping = 5,
        /// <summary>Unknown</summary>
        Debug = 6,
        /// <summary>Event message when an object uses llOwnerSay</summary>
        OwnerSay = 8
    }

    /// <summary>
    /// Identifies the source of a chat message
    /// </summary>
    public enum ChatSourceType : byte
    {
        /// <summary>Chat from the grid or simulator</summary>
        System = 0,
        /// <summary>Chat from another avatar</summary>
        Agent = 1,
        /// <summary>Chat from an object</summary>
        Object = 2
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ChatAudibleLevel : sbyte
    {
        /// <summary></summary>
        Not = -1,
        /// <summary></summary>
        Barely = 0,
        /// <summary></summary>
        Fully = 1
    }

    /// <summary>
    /// Effect type used in ViewerEffect packets
    /// </summary>
    public enum EffectType : byte
    {
        /// <summary></summary>
        Text = 0,
        /// <summary></summary>
        Icon,
        /// <summary></summary>
        Connector,
        /// <summary></summary>
        FlexibleObject,
        /// <summary></summary>
        AnimalControls,
        /// <summary></summary>
        AnimationObject,
        /// <summary></summary>
        Cloth,
        /// <summary>Project a beam from a source to a destination, such as
        /// the one used when editing an object</summary>
        Beam,
        /// <summary></summary>
        Glow,
        /// <summary></summary>
        Point,
        /// <summary></summary>
        Trail,
        /// <summary>Create a swirl of particles around an object</summary>
        Sphere,
        /// <summary></summary>
        Spiral,
        /// <summary></summary>
        Edit,
        /// <summary>Cause an avatar to look at an object</summary>
        LookAt,
        /// <summary>Cause an avatar to point at an object</summary>
        PointAt
    }

    /// <summary>
    /// The action an avatar is doing when looking at something, used in 
    /// ViewerEffect packets for the LookAt effect
    /// </summary>
    public enum LookAtType : byte
    {
        /// <summary></summary>
        None,
        /// <summary></summary>
        Idle,
        /// <summary></summary>
        AutoListen,
        /// <summary></summary>
        FreeLook,
        /// <summary></summary>
        Respond,
        /// <summary></summary>
        Hover,
        /// <summary>Deprecated</summary>
        [Obsolete]
        Conversation,
        /// <summary></summary>
        Select,
        /// <summary></summary>
        Focus,
        /// <summary></summary>
        Mouselook,
        /// <summary></summary>
        Clear
    }

    /// <summary>
    /// The action an avatar is doing when pointing at something, used in
    /// ViewerEffect packets for the PointAt effect
    /// </summary>
    public enum PointAtType : byte
    {
        /// <summary></summary>
        None,
        /// <summary></summary>
        Select,
        /// <summary></summary>
        Grab,
        /// <summary></summary>
        Clear
    }

    /// <summary>
    /// Money transaction types
    /// </summary>
    public enum MoneyTransactionType : int
    {
        /// <summary></summary>
        None = 0,
        /// <summary></summary>
        FailSimulatorTimeout = 1,
        /// <summary></summary>
        FailDataserverTimeout = 2,
        /// <summary></summary>
        ObjectClaim = 1000,
        /// <summary></summary>
        LandClaim = 1001,
        /// <summary></summary>
        GroupCreate = 1002,
        /// <summary></summary>
        ObjectPublicClaim = 1003,
        /// <summary></summary>
        GroupJoin = 1004,
        /// <summary></summary>
        TeleportCharge = 1100,
        /// <summary></summary>
        UploadCharge = 1101,
        /// <summary></summary>
        LandAuction = 1102,
        /// <summary></summary>
        ClassifiedCharge = 1103,
        /// <summary></summary>
        ObjectTax = 2000,
        /// <summary></summary>
        LandTax = 2001,
        /// <summary></summary>
        LightTax = 2002,
        /// <summary></summary>
        ParcelDirFee = 2003,
        /// <summary></summary>
        GroupTax = 2004,
        /// <summary></summary>
        ClassifiedRenew = 2005,
        /// <summary></summary>
        GiveInventory = 3000,
        /// <summary></summary>
        ObjectSale = 5000,
        /// <summary></summary>
        Gift = 5001,
        /// <summary></summary>
        LandSale = 5002,
        /// <summary></summary>
        ReferBonus = 5003,
        /// <summary></summary>
        InventorySale = 5004,
        /// <summary></summary>
        RefundPurchase = 5005,
        /// <summary></summary>
        LandPassSale = 5006,
        /// <summary></summary>
        DwellBonus = 5007,
        /// <summary></summary>
        PayObject = 5008,
        /// <summary></summary>
        ObjectPays = 5009,
        /// <summary></summary>
        GroupLandDeed = 6001,
        /// <summary></summary>
        GroupObjectDeed = 6002,
        /// <summary></summary>
        GroupLiability = 6003,
        /// <summary></summary>
        GroupDividend = 6004,
        /// <summary></summary>
        GroupMembershipDues = 6005,
        /// <summary></summary>
        ObjectRelease = 8000,
        /// <summary></summary>
        LandRelease = 8001,
        /// <summary></summary>
        ObjectDelete = 8002,
        /// <summary></summary>
        ObjectPublicDecay = 8003,
        /// <summary></summary>
        ObjectPublicDelete = 8004,
        /// <summary></summary>
        LindenAdjustment = 9000,
        /// <summary></summary>
        LindenGrant = 9001,
        /// <summary></summary>
        LindenPenalty = 9002,
        /// <summary></summary>
        EventFee = 9003,
        /// <summary></summary>
        EventPrize = 9004,
        /// <summary></summary>
        StipendBasic = 10000,
        /// <summary></summary>
        StipendDeveloper = 10001,
        /// <summary></summary>
        StipendAlways = 10002,
        /// <summary></summary>
        StipendDaily = 10003,
        /// <summary></summary>
        StipendRating = 10004,
        /// <summary></summary>
        StipendDelta = 10005
    }
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum TransactionFlags : byte
    {
        /// <summary></summary>
        None = 0,
        /// <summary></summary>
        SourceGroup = 1,
        /// <summary></summary>
        DestGroup = 2,
        /// <summary></summary>
        OwnerGroup = 4,
        /// <summary></summary>
        SimultaneousContribution = 8,
        /// <summary></summary>
        ContributionRemoval = 16
    }
    /// <summary>
    /// 
    /// </summary>
    public enum MeanCollisionType : byte
    {
        /// <summary></summary>
        None,
        /// <summary></summary>
        Bump,
        /// <summary></summary>
        LLPushObject,
        /// <summary></summary>
        SelectedObjectCollide,
        /// <summary></summary>
        ScriptedObjectCollide,
        /// <summary></summary>
        PhysicalObjectCollide
    }

    /// <summary>
    /// Flags sent when a script takes or releases a control
    /// </summary>
    /// <remarks>NOTE: (need to verify) These might be a subset of the ControlFlags enum in Movement,</remarks>
    [Flags]
    public enum ScriptControlChange : uint
    {
        /// <summary>No Flags set</summary>
        None = 0,
        /// <summary>Forward (W or up Arrow)</summary>
        Forward = 1,
        /// <summary>Back (S or down arrow)</summary>
        Back = 2,
        /// <summary>Move left (shift+A or left arrow)</summary>
        Left = 4,
        /// <summary>Move right (shift+D or right arrow)</summary>
        Right = 8,
        /// <summary>Up (E or PgUp)</summary>
        Up = 16,
        /// <summary>Down (C or PgDown</summary>
        Down = 32,
        /// <summary>Rotate left (A or left arrow)</summary>
        RotateLeft = 256,
        /// <summary>Rotate right (D or right arrow)</summary>
        RotateRight = 512,
        /// <summary>Left Mouse Button</summary>
        LeftButton = 268435456,
        /// <summary>Left Mouse button in MouseLook</summary>
        MouseLookLeftButton = 1073741824
    }
    #endregion Enums

    #region Structs

    /// <summary>
    /// Instant Message
    /// </summary>
    public struct InstantMessage
    {
        /// <summary>Key of sender</summary>
        public UUID FromAgentID;
        /// <summary>Name of sender</summary>
        public string FromAgentName;
        /// <summary>Key of destination avatar</summary>
        public UUID ToAgentID;
        /// <summary>ID of originating estate</summary>
        public uint ParentEstateID;
        /// <summary>Key of originating region</summary>
        public UUID RegionID;
        /// <summary>Coordinates in originating region</summary>
        public Vector3 Position;
        /// <summary>Instant message type</summary>
        public InstantMessageDialog Dialog;
        /// <summary>Group IM session toggle</summary>
        public bool GroupIM;
        /// <summary>Key of IM session, for Group Messages, the groups UUID</summary>
        public UUID IMSessionID;
        /// <summary>Timestamp of the instant message</summary>
        public DateTime Timestamp;
        /// <summary>Instant message text</summary>
        public string Message;
        /// <summary>Whether this message is held for offline avatars</summary>
        public InstantMessageOnline Offline;
        /// <summary>Context specific packed data</summary>
        public byte[] BinaryBucket;
        //Print the contents of a message
        public override string ToString()
        {
            string result = "";
            Type imType = this.GetType();
            FieldInfo[] fields = imType.GetFields();
            foreach (FieldInfo field in fields)
            {
                result += (field.Name + " = " + field.GetValue(this) + " ");
            }
            return result;

        }
    }

    #endregion Structs

    /// <summary>
    /// Manager class for our own avatar
    /// </summary>
    public partial class AgentManager
    {
        #region Enums

        /// <summary>
        /// Currently only used to hide your group title
        /// </summary>
        [Flags]
        public enum AgentFlags : byte
        {
            /// <summary>No flags set</summary>
            None = 0,
            /// <summary>Hide your group title</summary>
            HideTitle = 0x01,
        }

        /// <summary>
        /// Action state of the avatar, which can currently be typing and
        /// editing
        /// </summary>
        [Flags]
        public enum AgentState : byte
        {
            /// <summary></summary>
            None = 0x00,
            /// <summary></summary>
            Typing = 0x04,
            /// <summary></summary>
            Editing = 0x10
        }

        /// <summary>
        /// Current teleport status
        /// </summary>
        public enum TeleportStatus
        {
            /// <summary>Unknown status</summary>
            None,
            /// <summary>Teleport initialized</summary>
            Start,
            /// <summary>Teleport in progress</summary>
            Progress,
            /// <summary>Teleport failed</summary>
            Failed,
            /// <summary>Teleport completed</summary>
            Finished,
            /// <summary>Teleport cancelled</summary>
            Cancelled
        }

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum TeleportFlags : uint
        {
            /// <summary>No flags set, or teleport failed</summary>
            Default = 0,
            /// <summary>Set when newbie leaves help island for first time</summary>
            SetHomeToTarget = 1 << 0,
            /// <summary></summary>
            SetLastToTarget = 1 << 1,
            /// <summary>Via Lure</summary>
            ViaLure = 1 << 2,
            /// <summary>Via Landmark</summary>
            ViaLandmark = 1 << 3,
            /// <summary>Via Location</summary>
            ViaLocation = 1 << 4,
            /// <summary>Via Home</summary>
            ViaHome = 1 << 5,
            /// <summary>Via Telehub</summary>
            ViaTelehub = 1 << 6,
            /// <summary>Via Login</summary>
            ViaLogin = 1 << 7,
            /// <summary>Linden Summoned</summary>
            ViaGodlikeLure = 1 << 8,
            /// <summary>Linden Forced me</summary>
            Godlike = 1 << 9,
            /// <summary></summary>
            NineOneOne = 1 << 10,
            /// <summary>Agent Teleported Home via Script</summary>
            DisableCancel = 1 << 11,
            /// <summary></summary>
            ViaRegionID = 1 << 12,
            /// <summary></summary>
            IsFlying = 1 << 13,
            /// <summary></summary>
            ResetHome = 1 << 14,
            /// <summary>forced to new location for example when avatar is banned or ejected</summary>
            ForceRedirect = 1 << 15,
            /// <summary>Teleport Finished via a Lure</summary>
            FinishedViaLure = 1 << 26,
            /// <summary>Finished, Sim Changed</summary>
            FinishedViaNewSim = 1 << 28,
            /// <summary>Finished, Same Sim</summary>
            FinishedViaSameSim = 1 << 29
        }

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum TeleportLureFlags
        {
            /// <summary></summary>
            NormalLure = 0,
            /// <summary></summary>
            GodlikeLure = 1,
            /// <summary></summary>
            GodlikePursuit = 2
        }

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum ScriptSensorTypeFlags
        {
            /// <summary></summary>
            Agent = 1,
            /// <summary></summary>
            Active = 2,
            /// <summary></summary>
            Passive = 4,
            /// <summary></summary>
            Scripted = 8,
        }

        #endregion Enums

        #region Callbacks

        /// <summary>
        /// Triggered on incoming chat messages
        /// </summary>
        /// <param name="message">Text of chat message</param>
        /// <param name="audible">Audible level of this chat message</param>
        /// <param name="type">Type of chat (whisper, shout, status, etc.)</param>
        /// <param name="sourceType">Source of the chat message</param>
        /// <param name="fromName">Name of the sending object</param>
        /// <param name="id">Key of source</param>
        /// <param name="ownerid">Key of the sender</param>
        /// <param name="position">Senders position</param>
        public delegate void ChatCallback(string message, ChatAudibleLevel audible, ChatType type,
            ChatSourceType sourceType, string fromName, UUID id, UUID ownerid, Vector3 position);

        /// <summary>
        /// Triggered when a script pops up a dialog box
        /// </summary>
        /// <param name="message">The dialog box message</param>
        /// <param name="objectName">Name of the object that sent the dialog</param>
        /// <param name="imageID">Image to be displayed in the dialog</param>
        /// <param name="objectID">ID of the object that sent the dialog</param>
        /// <param name="firstName">First name of the object owner</param>
        /// <param name="lastName">Last name of the object owner</param>
        /// <param name="chatChannel">Chat channel that the object is communicating on</param>
        /// <param name="buttons">List of button labels</param>
        public delegate void ScriptDialogCallback(string message, string objectName, UUID imageID,
            UUID objectID, string firstName, string lastName, int chatChannel, List<string> buttons);

        /// <summary>
        /// Triggered when a script asks for permissions
        /// </summary>
        /// <param name="simulator">Simulator object this request comes from</param>
        /// <param name="taskID">Task ID of the script requesting permissions</param>
        /// <param name="itemID">ID of the object containing the script</param>
        /// <param name="objectName">Name of the object containing the script</param>
        /// <param name="objectOwner">Name of the object's owner</param>
        /// <param name="questions">Bitwise value representing the requested permissions</param>
        public delegate void ScriptQuestionCallback(Simulator simulator, UUID taskID, UUID itemID, string objectName, string objectOwner, ScriptPermission questions);

        /// <summary>
        /// Triggered when a script displays a URL via llLoadURL
        /// </summary>
        /// <param name="objectName">Name of the scripted object</param>
        /// <param name="objectID">ID of the scripted object</param>
        /// <param name="ownerID">ID of the object's owner</param>
        /// <param name="ownerIsGroup">Whether or not ownerID is a group</param>
        /// <param name="message">Message displayed along with URL</param>
        /// <param name="URL">Offered URL</param>
        public delegate void LoadURLCallback(string objectName, UUID objectID, UUID ownerID, bool ownerIsGroup, string message, string URL);

        /// <summary>
        /// Triggered when the L$ account balance for this avatar changes
        /// </summary>
        /// <param name="balance">The new account balance</param>
        public delegate void BalanceCallback(int balance);

        /// <summary>
        /// Triggered on Money Balance Reply
        /// </summary>
        /// <param name="transactionID">ID provided in Request Money Balance, or auto-generated by system events</param>
        /// <param name="transactionSuccess">Was the transaction successful</param>
        /// <param name="balance">Current balance</param>
        /// <param name="metersCredit">Land use credits you have</param>
        /// <param name="metersCommitted">Tier committed to group(s)</param>
        /// <param name="description">Description of the transaction</param>
        public delegate void MoneyBalanceReplyCallback(UUID transactionID, bool transactionSuccess, int balance, int metersCredit, int metersCommitted, string description);

        /// <summary>
        /// Triggered on incoming instant messages
        /// </summary>
        /// <param name="im">Instant message data structure</param>
        /// <param name="simulator">Simulator where this IM was received from</param>
        public delegate void InstantMessageCallback(InstantMessage im, Simulator simulator);

        /// <summary>
        /// Triggered for any status updates of a teleport (progress, failed, succeeded)
        /// </summary>
        /// <param name="message">A message about the current teleport status</param>
        /// <param name="status">The current status of the teleport</param>
        /// <param name="flags">Various flags describing the teleport</param>
        public delegate void TeleportCallback(string message, TeleportStatus status, TeleportFlags flags);

        /// <summary>
        /// Reply to a request to join a group, informs whether it was successful or not
        /// </summary>
        /// <param name="groupID">The group we attempted to join</param>
        /// <param name="success">Whether we joined the group or not</param>
        public delegate void JoinGroupCallback(UUID groupID, bool success);

        /// <summary>
        /// Reply to a request to leave a group, informs whether it was successful or not
        /// </summary>
        /// <param name="groupID">The group we attempted to leave</param>
        /// <param name="success">Whether we left the group or not</param>
        public delegate void LeaveGroupCallback(UUID groupID, bool success);

        /// <summary>
        /// Informs the avatar that it is no longer a member of a group
        /// </summary>
        /// <param name="groupID">The group Key we are no longer a member of</param>
        public delegate void GroupDroppedCallback(UUID groupID);

        /// <summary>
        /// Reply to an AgentData request
        /// </summary>
        /// <param name="firstName">First name of Avatar</param>
        /// <param name="lastName">Last name of Avatar</param>
        /// <param name="activeGroupID">Key of Group Avatar has active</param>
        /// <param name="groupTitle">Avatars Active Title</param>
        /// <param name="groupPowers">Powers Avatar has in group</param>
        /// <param name="groupName">Name of the Group</param>
        public delegate void AgentDataCallback(string firstName, string lastName, UUID activeGroupID,
            string groupTitle, GroupPowers groupPowers, string groupName);

        /// <summary>
        /// Triggered when the current agent animations change
        /// </summary>
        /// <param name="agentAnimations">A convenience reference to the
        /// SignaledAnimations collection</param>
        public delegate void AnimationsChangedCallback(InternalDictionary<UUID, int> agentAnimations);

        /// <summary>
        /// Triggered when an object or avatar forcefully collides with our
        /// agent
        /// </summary>
        /// <param name="type">Collision type</param>
        /// <param name="perp">Colliding object or avatar ID</param>
        /// <param name="victim">Victim ID, should be our own AgentID</param>
        /// <param name="magnitude">Velocity or total force of the collision</param>
        /// <param name="time">Time the collision occurred</param>
        public delegate void MeanCollisionCallback(MeanCollisionType type, UUID perp, UUID victim,
            float magnitude, DateTime time);

        /// <summary>
        /// Triggered when the agent physically moves in to a neighboring region
        /// </summary>
        /// <param name="oldSim">Simulator agent was previously occupying</param>
        /// <param name="newSim">Simulator agent is now currently occupying</param>
        public delegate void RegionCrossedCallback(Simulator oldSim, Simulator newSim);

        /// <summary>
        /// Fired when group chat session confirmed joined</summary>
        /// <param name="groupChatSessionID">Key of Session (groups UUID)</param>
        /// <param name="tmpSessionID">Temporary session Key</param>
        /// <param name="sessionName">A string representation of the session name</param>
        /// <param name="success"><see langword="true"/> if session start successful, 
        /// <see langword="false"/> otherwise</param>
        public delegate void GroupChatJoinedCallback(UUID groupChatSessionID, string sessionName, UUID tmpSessionID, bool success);

        /// <summary>Fired when agent group chat session terminated</summary>
        /// <param name="groupchatSessionID">Key of Session (groups UUID)</param>
        public delegate void GroupChatLeftCallback(UUID groupchatSessionID);

        /// <summary>
        /// Fired when alert message received from simulator
        /// </summary>
        /// <param name="message">the message sent from the grid to our avatar.</param>
        public delegate void AlertMessageCallback(string message);

        /// <summary>
        /// Fired when a script wants to give or release controls.
        /// </summary>
        /// <param name="controls">Control to give or take</param>
        /// <param name="pass">true of passing control to agent</param>
        /// <param name="take">true of taking control from agent</param>
        public delegate void ScriptControlCallback(ScriptControlChange controls, bool pass, bool take);

        /// <summary>
        /// Fired when camera tries to view beyond its view limits
        /// </summary>
        /// <param name="collidePlane"><seealso cref="Vector4"/> representing plane where constraints were hit</param>
        public delegate void CameraConstraintCallback(Vector4 collidePlane);

        /// <summary>
        /// Fired when script sensor reply is received
        /// </summary>
        /// <param name="requestorID">requestors UUID</param>
        /// <param name="groupID">Sources Group UUID</param>
        /// <param name="name">Sources Name</param>
        /// <param name="objectID">Objects UUID</param>
        /// <param name="ownerID">Object owners UUID</param>
        /// <param name="position">Position of Object</param>
        /// <param name="range">Range of Object</param>
        /// <param name="rotation">Rotation of object</param>
        /// <param name="type">Objects Type</param>
        /// <param name="velocity"><seealso cref="Vector3"/> representing the velocity of object</param>
        /// TODO: this should probably be a struct, and there should be an enum added for type
        public delegate void ScriptSensorReplyCallback(UUID requestorID, UUID groupID, string name,
            UUID objectID, UUID ownerID, Vector3 position, float range, Quaternion rotation,
            ScriptSensorTypeFlags type, Vector3 velocity);

        /// <summary>
        /// Fired in response to a RequestSit()
        /// </summary>
        /// <param name="objectID">ID of primitive avatar will be sitting on</param>
        /// <param name="autoPilot">true of avatar autopiloted there</param>
        /// <param name="cameraAtOffset">Camera offset when avatar is seated</param>
        /// <param name="cameraEyeOffset">Camera eye offset when avatar is seated</param>
        /// <param name="forceMouselook">true of sitting on this object will force mouselook</param>
        /// <param name="sitPosition">position avatar will be in when seated</param>
        /// <param name="sitRotation">rotation avatar will be in when seated</param>
        public delegate void AvatarSitResponseCallback(UUID objectID, bool autoPilot, Vector3 cameraAtOffset,
            Vector3 cameraEyeOffset, bool forceMouselook, Vector3 sitPosition, Quaternion sitRotation);

        /// <summary>
        /// Fired when a new member joins a Group chat session
        /// </summary>
        /// <param name="sessionID">the ID of the session</param>
        /// <param name="agent_key">the ID of the avatar that joined</param>
        public delegate void ChatSessionMemberAddedCallback(UUID sessionID, UUID agent_key);
        /// <summary>
        /// Fired when a member of a Group chat leaves the session
        /// </summary>
        /// <param name="sessionID">the ID of the session</param>
        /// <param name="agent_key">the ID of the avatar that joined</param>
        public delegate void ChatSessionMemberLeftCallback(UUID sessionID, UUID agent_key);

        #endregion Callbacks

        #region Events

        /// <summary>Fired when a <see cref="ChatFromSimulatorPacket"/> is received from the simulator, Contains
        /// Any Whisper, Shout, or Say within range of avatar</summary>
        public event ChatCallback OnChat;
        /// <summary>Fired when a <see cref="ScriptDialogPacket"/> is received, use <seealso cref="ReplyToScriptDialog"/> 
        /// to respond to dialog</summary>
        public event ScriptDialogCallback OnScriptDialog;
        /// <summary>Fired when a <seealso cref="ScriptQuestionPacket"/> is received in response to a 
        /// scripted object requesting permissions, Use <seealso cref="ScriptQuestionReply"/> to reply</summary>
        public event ScriptQuestionCallback OnScriptQuestion;
        /// <summary>Fired when a <seealso cref="LoadURLPacket"/> is received, contains a URL pasted in Chat</summary>
        public event LoadURLCallback OnLoadURL;
        /// <summary>Fired when a <seealso cref="ImprovedInstantMessagePacket"/> or a ChatterBoxInvitation is received</summary>
        public event InstantMessageCallback OnInstantMessage;
        /// <summary>Fired when a <seealso cref="TeleportLocalPacket"/> is received, occurs when a
        /// <seealso cref="RequestTeleport"/> or <seealso cref="Teleport"/> is called</summary>
        public event TeleportCallback OnTeleport;
        /// <summary>Fired when a <seealso cref="MoneyBalanceReplyPacket"/> indicating the agents
        /// balance has changed by spending, sending, or receiving L$, Contains the Avatars new balance</summary>
        public event BalanceCallback OnBalanceUpdated;
        /// <summary>Fired when a <seealso cref="MoneyBalanceReplyPacket"/> is received, contains L$ balance and additional
        /// details of the transaction</summary>
        public event MoneyBalanceReplyCallback OnMoneyBalanceReplyReceived;
        /// <summary>Fired when a <seealso cref="AgentDataUpdatePacket"/> is received, caused by changing the
        /// Agents active group with <seealso cref="ActivateGroup"/></summary>
        public event AgentDataCallback OnAgentDataUpdated;
        /// <summary>Fired when a <seealso cref="AvatarAnimationPacket"/> is received, will contain a Dictionary
        /// of animations currently being played</summary>
        public event AnimationsChangedCallback OnAnimationsChanged;
        /// <summary>Callback for an object or avatar forcefully colliding
        /// with the agent</summary>
        public event MeanCollisionCallback OnMeanCollision;
        /// <summary>Callback for the agent moving in to a neighboring sim</summary>
        public event RegionCrossedCallback OnRegionCrossed;
        /// <summary>Callback for when agent is confirmed joined group chat session.</summary>
        public event GroupChatJoinedCallback OnGroupChatJoin;
        /// <summary>Callback for when agent is confirmed to have left group chat session.</summary>
        public event GroupChatLeftCallback OnGroupChatLeft;
        /// <summary>Alert messages sent to client from simulator</summary>
        public event AlertMessageCallback OnAlertMessage;
        /// <summary>Fired when a script wants to take or release control of your avatar.</summary>
        public event ScriptControlCallback OnScriptControlChange;
        /// <summary>Fired when our avatar camera reaches the maximum possible point</summary>
        public event CameraConstraintCallback OnCameraConstraint;
        /// <summary>Fired when a script sensor reply is received</summary>
        public event ScriptSensorReplyCallback OnScriptSensorReply;
        /// <summary>Fired in response to a sit request</summary>
        public event AvatarSitResponseCallback OnAvatarSitResponse;
        /// <summary>Fired when a new member joins an active ChatterBoxSession session</summary>
        public event ChatSessionMemberAddedCallback OnChatSessionMemberAdded;
        /// <summary>Fired when a member of an active ChatterBoxSession leaves the session</summary>
        public event ChatSessionMemberLeftCallback OnChatSessionMemberLeft;

        #endregion Events

        /// <summary>Reference to the GridClient object</summary>
        public readonly GridClient Client;
        /// <summary>Used for movement and camera tracking</summary>
        public readonly AgentMovement Movement;
        /// <summary>Currently playing animations for the agent. Can be used to
        /// check the current movement status such as walking, hovering, aiming,
        /// etc. by checking for system animations in the Animations class</summary>
        public InternalDictionary<UUID, int> SignaledAnimations = new InternalDictionary<UUID, int>();
        /// <summary>
        /// Dictionary containing current Group Chat sessions and members
        /// </summary>
        public InternalDictionary<UUID, List<ChatSessionMember>> GroupChatSessions = new InternalDictionary<UUID, List<ChatSessionMember>>();

        #region Properties

        /// <summary>Your (client) avatars <seealso cref="UUID"/></summary>
        /// <remarks>"client", "agent", and "avatar" all represent the same thing</remarks>
        public UUID AgentID { get { return id; } }
        /// <summary>Temporary <seealso cref="UUID"/> assigned to this session, used for 
        /// verifying our identity in packets</summary>
        public UUID SessionID { get { return sessionID; } }
        /// <summary>Shared secret <seealso cref="UUID"/> that is never sent over the wire</summary>
        public UUID SecureSessionID { get { return secureSessionID; } }
        /// <summary>Your (client) avatar ID, local to the current region/sim</summary>
        public uint LocalID { get { return localID; } }
        /// <summary>Where the avatar started at login. Can be "last", "home" 
        /// or a login <seealso cref="T:OpenMetaverse.URI"/></summary>
        public string StartLocation { get { return startLocation; } }
        /// <summary>The access level of this agent, usually M or PG</summary>
        public string AgentAccess { get { return agentAccess; } }
        /// <summary>The CollisionPlane of Agent</summary>
        public Vector4 CollisionPlane { get { return collisionPlane; } }
        /// <summary>An <seealso cref="Vector3"/> representing the velocity of our agent</summary>
        public Vector3 Velocity { get { return velocity; } }
        /// <summary>An <seealso cref="Vector3"/> representing the acceleration of our agent</summary>
        public Vector3 Acceleration { get { return acceleration; } }
        /// <summary>A <seealso cref="Vector3"/> which specifies the angular speed, and axis about which an Avatar is rotating.</summary>
        public Vector3 AngularVelocity { get { return angularVelocity; } }
        /// <summary>Position avatar client will goto when login to 'home' or during
        /// teleport request to 'home' region.</summary>
        public Vector3 HomePosition { get { return homePosition; } }
        /// <summary>LookAt point saved/restored with HomePosition</summary>
        public Vector3 HomeLookAt { get { return homeLookAt; } }
        /// <summary>Avatar First Name (i.e. Philip)</summary>
        public string FirstName { get { return firstName; } }
        /// <summary>Avatar Last Name (i.e. Linden)</summary>
        public string LastName { get { return lastName; } }
        /// <summary>Avatar Full Name (i.e. Philip Linden)</summary>
        public string Name
        {
            get
            {
                // This is a fairly common request, so assume the name doesn't
                // change mid-session and cache the result
                if (fullName == null || fullName.Length < 2)
                    fullName = String.Format("{0} {1}", firstName, lastName);
                return fullName;
            }
        }
        /// <summary>Gets the health of the agent</summary>
        public float Health { get { return health; } }
        /// <summary>Gets the current balance of the agent</summary>
        public int Balance { get { return balance; } }
        /// <summary>Gets the local ID of the prim the agent is sitting on,
        /// zero if the avatar is not currently sitting</summary>
        public uint SittingOn { get { return sittingOn; } }
        /// <summary>Gets the <seealso cref="UUID"/> of the agents active group.</summary>
        public UUID ActiveGroup { get { return activeGroup; } }
        /// <summary>Gets the Agents powers in the currently active group</summary>
        public GroupPowers ActiveGroupPowers { get { return activeGroupPowers; } }
        /// <summary>Current status message for teleporting</summary>
        public string TeleportMessage { get { return teleportMessage; } }
        /// <summary>Current position of the agent as a relative offset from
        /// the simulator, or the parent object if we are sitting on something</summary>
        public Vector3 RelativePosition { get { return relativePosition; } }
        /// <summary>Current rotation of the agent as a relative rotation from
        /// the simulator, or the parent object if we are sitting on something</summary>
        public Quaternion RelativeRotation { get { return relativeRotation; } }
        /// <summary>Current position of the agent in the simulator</summary>
        public Vector3 SimPosition
        {
            get
            {
                if (sittingOn != 0)
                {
                    Primitive parent;
                    if (Client.Network.CurrentSim != null && Client.Network.CurrentSim.ObjectsPrimitives.TryGetValue(sittingOn, out parent))
                    {
                        return parent.Position + Vector3.Transform(relativePosition, Matrix4.CreateFromQuaternion(parent.Rotation));
                    }
                    else
                    {
                        Logger.Log("Currently sitting on object " + sittingOn + " which is not tracked, SimPosition will be inaccurate",
                            Helpers.LogLevel.Warning, Client);
                        return relativePosition;
                    }
                }
                else
                {
                    return relativePosition;
                }
            }
        }
        /// <summary>
        /// A <seealso cref="Quaternion"/> representing the agents current rotation
        /// </summary>
        public Quaternion SimRotation
        {
            get
            {
                if (sittingOn != 0)
                {
                    Primitive parent;
                    if (Client.Network.CurrentSim != null && Client.Network.CurrentSim.ObjectsPrimitives.TryGetValue(sittingOn, out parent))
                    {
                        return relativeRotation * parent.Rotation;
                    }
                    else
                    {
                        Logger.Log("Currently sitting on object " + sittingOn + " which is not tracked, SimRotation will be inaccurate",
                            Helpers.LogLevel.Warning, Client);
                        return relativeRotation;
                    }
                }
                else
                {
                    return relativeRotation;
                }
            }
        }
        /// <summary>Returns the global grid position of the avatar</summary>
        public Vector3d GlobalPosition
        {
            get
            {
                if (Client.Network.CurrentSim != null)
                {
                    uint globalX, globalY;
                    Utils.LongToUInts(Client.Network.CurrentSim.Handle, out globalX, out globalY);
                    Vector3 pos = SimPosition;

                    return new Vector3d(
                        (double)globalX + (double)pos.X,
                        (double)globalY + (double)pos.Y,
                        (double)pos.Z);
                }
                else
                    return Vector3d.Zero;
            }
        }

        #endregion Properties

        internal uint localID;
        internal Vector3 relativePosition;
        internal Quaternion relativeRotation = Quaternion.Identity;
        internal Vector4 collisionPlane;
        internal Vector3 velocity;
        internal Vector3 acceleration;
        internal Vector3 angularVelocity;
        internal uint sittingOn;
        internal int lastInterpolation;

        #region Private Members

        private UUID id;
        private UUID sessionID;
        private UUID secureSessionID;
        private string startLocation = String.Empty;
        private string agentAccess = String.Empty;
        private Vector3 homePosition;
        private Vector3 homeLookAt;
        private string firstName = String.Empty;
        private string lastName = String.Empty;
        private string fullName;
        private string teleportMessage = String.Empty;
        private TeleportStatus teleportStat = TeleportStatus.None;
        private ManualResetEvent teleportEvent = new ManualResetEvent(false);
        private uint heightWidthGenCounter;
        private float health;
        private int balance;
        private UUID activeGroup;
        private GroupPowers activeGroupPowers;
        #endregion Private Members

        /// <summary>
        /// Constructor, setup callbacks for packets related to our avatar
        /// </summary>
        /// <param name="client">A reference to the <seealso cref="T:OpenMetaverse.GridClient"/> Class</param>
        public AgentManager(GridClient client)
        {
            Client = client;
            Movement = new AgentMovement(Client);
            NetworkManager.PacketCallback callback;

            Client.Network.OnDisconnected += new NetworkManager.DisconnectedCallback(Network_OnDisconnected);

            // Teleport callbacks
            callback = new NetworkManager.PacketCallback(TeleportHandler);
            Client.Network.RegisterCallback(PacketType.TeleportStart, callback);
            Client.Network.RegisterCallback(PacketType.TeleportProgress, callback);
            Client.Network.RegisterCallback(PacketType.TeleportFailed, callback);
            Client.Network.RegisterEventCallback("TeleportFinish", new Caps.EventQueueCallback(TeleportFinishEventHandler));
            Client.Network.RegisterCallback(PacketType.TeleportCancel, callback);
            Client.Network.RegisterCallback(PacketType.TeleportLocal, callback);

            // Instant message callback
            Client.Network.RegisterCallback(PacketType.ImprovedInstantMessage, new NetworkManager.PacketCallback(InstantMessageHandler));
            // Chat callback
            Client.Network.RegisterCallback(PacketType.ChatFromSimulator, new NetworkManager.PacketCallback(ChatHandler));
            // Script dialog callback
            Client.Network.RegisterCallback(PacketType.ScriptDialog, new NetworkManager.PacketCallback(ScriptDialogHandler));
            // Script question callback
            Client.Network.RegisterCallback(PacketType.ScriptQuestion, new NetworkManager.PacketCallback(ScriptQuestionHandler));
            // Script URL callback
            Client.Network.RegisterCallback(PacketType.LoadURL, new NetworkManager.PacketCallback(LoadURLHandler));
            // Movement complete callback
            Client.Network.RegisterCallback(PacketType.AgentMovementComplete, new NetworkManager.PacketCallback(MovementCompleteHandler));
            // Health callback
            Client.Network.RegisterCallback(PacketType.HealthMessage, new NetworkManager.PacketCallback(HealthHandler));
            // Money callback
            Client.Network.RegisterCallback(PacketType.MoneyBalanceReply, new NetworkManager.PacketCallback(BalanceHandler));
            //Agent update callback
            Client.Network.RegisterCallback(PacketType.AgentDataUpdate, new NetworkManager.PacketCallback(AgentDataUpdateHandler));
            // Animation callback
            Client.Network.RegisterCallback(PacketType.AvatarAnimation, new NetworkManager.PacketCallback(AvatarAnimationHandler));
            // Object colliding into our agent callback
            Client.Network.RegisterCallback(PacketType.MeanCollisionAlert, new NetworkManager.PacketCallback(MeanCollisionAlertHandler));
            // Region Crossing
            Client.Network.RegisterCallback(PacketType.CrossedRegion, new NetworkManager.PacketCallback(CrossedRegionHandler));
            // CAPS callbacks
            Client.Network.RegisterEventCallback("EstablishAgentCommunication", new Caps.EventQueueCallback(EstablishAgentCommunicationEventHandler));
            // Incoming Group Chat
            Client.Network.RegisterEventCallback("ChatterBoxInvitation", new Caps.EventQueueCallback(ChatterBoxInvitationHandler));
            // Outgoing Group Chat Reply
            Client.Network.RegisterEventCallback("ChatterBoxSessionEventReply", new Caps.EventQueueCallback(ChatterBoxSessionEventHandler));
            Client.Network.RegisterEventCallback("ChatterBoxSessionStartReply", new Caps.EventQueueCallback(ChatterBoxSessionStartReplyHandler));
            Client.Network.RegisterEventCallback("ChatterBoxSessionAgentListUpdates", new Caps.EventQueueCallback(ChatterBoxSessionAgentListReplyHandler));
            // Login
            Client.Network.RegisterLoginResponseCallback(new NetworkManager.LoginResponseCallback(Network_OnLoginResponse));
            // Alert Messages
            Client.Network.RegisterCallback(PacketType.AlertMessage, new NetworkManager.PacketCallback(AlertMessageHandler));
            // script control change messages, ie: when an in-world LSL script wants to take control of your agent.
            Client.Network.RegisterCallback(PacketType.ScriptControlChange, new NetworkManager.PacketCallback(ScriptControlChangeHandler));
            // Camera Constraint (probably needs to move to AgentManagerCamera TODO:
            Client.Network.RegisterCallback(PacketType.CameraConstraint, new NetworkManager.PacketCallback(CameraConstraintHandler));
            Client.Network.RegisterCallback(PacketType.ScriptSensorReply, new NetworkManager.PacketCallback(ScriptSensorReplyHandler));
            Client.Network.RegisterCallback(PacketType.AvatarSitResponse, new NetworkManager.PacketCallback(AvatarSitResponseHandler));

        }

        #region Chat and instant messages

        /// <summary>
        /// Send a chat message
        /// </summary>
        /// <param name="message">The Message you're sending out.</param>
        /// <param name="channel">Channel number (0 would be default 'Say' message, other numbers 
        /// denote the equivalent of /# in normal client).</param>
        /// <param name="type">Chat Type, see above.</param>
        public void Chat(string message, int channel, ChatType type)
        {
            ChatFromViewerPacket chat = new ChatFromViewerPacket();
            chat.AgentData.AgentID = this.id;
            chat.AgentData.SessionID = Client.Self.SessionID;
            chat.ChatData.Channel = channel;
            chat.ChatData.Message = Utils.StringToBytes(message);
            chat.ChatData.Type = (byte)type;

            Client.Network.SendPacket(chat);
        }

        /// <summary>Requests missed/offline messages</summary>
        public void RetrieveInstantMessages()
        {
            RetrieveInstantMessagesPacket p = new RetrieveInstantMessagesPacket();
            p.AgentData.AgentID = Client.Self.AgentID;
            p.AgentData.SessionID = Client.Self.SessionID;
            Client.Network.SendPacket(p);
        }

        /// <summary>
        /// Send an Instant Message
        /// </summary>
        /// <param name="target">Target of the Instant Message</param>
        /// <param name="message">Text message being sent</param>
        public void InstantMessage(UUID target, string message)
        {
            InstantMessage(Name, target, message, AgentID.Equals(target) ? AgentID : target ^ AgentID,
                InstantMessageDialog.MessageFromAgent, InstantMessageOnline.Offline, this.SimPosition,
                UUID.Zero, new byte[0]);
        }

        /// <summary>
        /// Send an Instant Message
        /// </summary>
        /// <param name="target">Target of the Instant Message</param>
        /// <param name="message">Text message being sent</param>
        /// <param name="imSessionID">IM session ID (to differentiate between IM windows)</param>
        public void InstantMessage(UUID target, string message, UUID imSessionID)
        {
            InstantMessage(Name, target, message, imSessionID,
                InstantMessageDialog.MessageFromAgent, InstantMessageOnline.Offline, this.SimPosition,
                UUID.Zero, new byte[0]);
        }

        /// <summary>
        /// Send an Instant Message
        /// </summary>
        /// <param name="fromName">The name this IM will show up as being from</param>
        /// <param name="target">Key of Avatar</param>
        /// <param name="message">Text message being sent</param>
        /// <param name="imSessionID">IM session ID (to differentiate between IM windows)</param>
        /// <param name="conferenceIDs">IDs of sessions for a conference</param>
        public void InstantMessage(string fromName, UUID target, string message, UUID imSessionID,
            UUID[] conferenceIDs)
        {
            byte[] binaryBucket;

            if (conferenceIDs != null && conferenceIDs.Length > 0)
            {
                binaryBucket = new byte[16 * conferenceIDs.Length];
                for (int i = 0; i < conferenceIDs.Length; ++i)
                    Buffer.BlockCopy(conferenceIDs[i].GetBytes(), 0, binaryBucket, i * 16, 16);
            }
            else
            {
                binaryBucket = new byte[0];
            }

            InstantMessage(fromName, target, message, imSessionID, InstantMessageDialog.MessageFromAgent,
                InstantMessageOnline.Offline, Vector3.Zero, UUID.Zero, binaryBucket);
        }

        /// <summary>
        /// Send an Instant Message
        /// </summary>
        /// <param name="fromName">The name this IM will show up as being from</param>
        /// <param name="target">Key of Avatar</param>
        /// <param name="message">Text message being sent</param>
        /// <param name="imSessionID">IM session ID (to differentiate between IM windows)</param>
        /// <param name="dialog">Type of instant message to send</param>
        /// <param name="offline">Whether to IM offline avatars as well</param>
        /// <param name="position">Senders Position</param>
        /// <param name="regionID">RegionID Sender is In</param>
        /// <param name="binaryBucket">Packed binary data that is specific to
        /// the dialog type</param>
        public void InstantMessage(string fromName, UUID target, string message, UUID imSessionID,
            InstantMessageDialog dialog, InstantMessageOnline offline, Vector3 position, UUID regionID,
            byte[] binaryBucket)
        {
            if (target != UUID.Zero)
            {
                ImprovedInstantMessagePacket im = new ImprovedInstantMessagePacket();

                if (imSessionID.Equals(UUID.Zero) || imSessionID.Equals(AgentID))
                    imSessionID = AgentID.Equals(target) ? AgentID : target ^ AgentID;

                im.AgentData.AgentID = Client.Self.AgentID;
                im.AgentData.SessionID = Client.Self.SessionID;

                im.MessageBlock.Dialog = (byte)dialog;
                im.MessageBlock.FromAgentName = Utils.StringToBytes(fromName);
                im.MessageBlock.FromGroup = false;
                im.MessageBlock.ID = imSessionID;
                im.MessageBlock.Message = Utils.StringToBytes(message);
                im.MessageBlock.Offline = (byte)offline;
                im.MessageBlock.ToAgentID = target;

                if (binaryBucket != null)
                    im.MessageBlock.BinaryBucket = binaryBucket;
                else
                    im.MessageBlock.BinaryBucket = new byte[0];

                // These fields are mandatory, even if we don't have valid values for them
                im.MessageBlock.Position = Vector3.Zero;
                //TODO: Allow region id to be correctly set by caller or fetched from Client.*
                im.MessageBlock.RegionID = regionID;

                // Send the message
                Client.Network.SendPacket(im);
            }
            else
            {
                Logger.Log(String.Format("Suppressing instant message \"{0}\" to UUID.Zero", message),
                    Helpers.LogLevel.Error, Client);
            }
        }

        /// <summary>
        /// Send an Instant Message to a group
        /// </summary>
        /// <param name="groupID"><seealso cref="UUID"/> of the group to send message to</param>
        /// <param name="message">Text Message being sent.</param>
        public void InstantMessageGroup(UUID groupID, string message)
        {
            InstantMessageGroup(Name, groupID, message);
        }

        /// <summary>
        /// Send an Instant Message to a group
        /// </summary>
        /// <param name="fromName">The name this IM will show up as being from</param>
        /// <param name="groupID"><seealso cref="UUID"/> of the group to send message to</param>
        /// <param name="message">Text message being sent</param>
        /// <remarks>This does not appear to function with groups the agent is not in</remarks>
        public void InstantMessageGroup(string fromName, UUID groupID, string message)
        {
            lock (GroupChatSessions.Dictionary)
                if (GroupChatSessions.ContainsKey(groupID))
                {
                    ImprovedInstantMessagePacket im = new ImprovedInstantMessagePacket();

                    im.AgentData.AgentID = Client.Self.AgentID;
                    im.AgentData.SessionID = Client.Self.SessionID;
                    im.MessageBlock.Dialog = (byte)InstantMessageDialog.SessionSend;
                    im.MessageBlock.FromAgentName = Utils.StringToBytes(fromName);
                    im.MessageBlock.FromGroup = false;
                    im.MessageBlock.Message = Utils.StringToBytes(message);
                    im.MessageBlock.Offline = 0;
                    im.MessageBlock.ID = groupID;
                    im.MessageBlock.ToAgentID = groupID;
                    im.MessageBlock.Position = Vector3.Zero;
                    im.MessageBlock.RegionID = UUID.Zero;
                    im.MessageBlock.BinaryBucket = Utils.StringToBytes("\0");

                    Client.Network.SendPacket(im);
                }
                else
                {
                    Logger.Log("No Active group chat session appears to exist, use RequestJoinGroupChat() to join one",
                        Helpers.LogLevel.Error, Client);
                }
        }

        /// <summary>
        /// Send a request to join a group chat session
        /// </summary>
        /// <param name="groupID"><seealso cref="UUID"/> of Group to leave</param>
        public void RequestJoinGroupChat(UUID groupID)
        {
            ImprovedInstantMessagePacket im = new ImprovedInstantMessagePacket();

            im.AgentData.AgentID = Client.Self.AgentID;
            im.AgentData.SessionID = Client.Self.SessionID;
            im.MessageBlock.Dialog = (byte)InstantMessageDialog.SessionGroupStart;
            im.MessageBlock.FromAgentName = Utils.StringToBytes(Client.Self.Name);
            im.MessageBlock.FromGroup = false;
            im.MessageBlock.Message = new byte[0];
            im.MessageBlock.ParentEstateID = 0;
            im.MessageBlock.Offline = 0;
            im.MessageBlock.ID = groupID;
            im.MessageBlock.ToAgentID = groupID;
            im.MessageBlock.BinaryBucket = new byte[0];
            im.MessageBlock.Position = Client.Self.SimPosition;
            im.MessageBlock.RegionID = UUID.Zero;

            Client.Network.SendPacket(im);
        }
        /// <summary>
        /// Request self terminates group chat. This will stop Group IM's from showing up
        /// until session is rejoined or expires.
        /// </summary>
        /// <param name="groupID"><seealso cref="UUID"/> of Group to leave</param>
        public void RequestLeaveGroupChat(UUID groupID)
        {
            ImprovedInstantMessagePacket im = new ImprovedInstantMessagePacket();

            im.AgentData.AgentID = Client.Self.AgentID;
            im.AgentData.SessionID = Client.Self.SessionID;
            im.MessageBlock.Dialog = (byte)InstantMessageDialog.SessionDrop;
            im.MessageBlock.FromAgentName = Utils.StringToBytes(Client.Self.Name);
            im.MessageBlock.FromGroup = false;
            im.MessageBlock.Message = new byte[0];
            im.MessageBlock.Offline = 0;
            im.MessageBlock.ID = groupID;
            im.MessageBlock.ToAgentID = groupID;
            im.MessageBlock.BinaryBucket = new byte[0];
            im.MessageBlock.Position = Vector3.Zero;
            im.MessageBlock.RegionID = UUID.Zero;

            Client.Network.SendPacket(im);
        }

        /// <summary>
        /// Reply to script dialog questions. 
        /// </summary>
        /// <param name="channel">Channel initial request came on</param>
        /// <param name="buttonIndex">Index of button you're "clicking"</param>
        /// <param name="buttonlabel">Label of button you're "clicking"</param>
        /// <param name="objectID"><seealso cref="UUID"/> of Object that sent the dialog request</param>
        /// <seealso cref="OnScriptDialog"/>
        public void ReplyToScriptDialog(int channel, int buttonIndex, string buttonlabel, UUID objectID)
        {
            ScriptDialogReplyPacket reply = new ScriptDialogReplyPacket();

            reply.AgentData.AgentID = Client.Self.AgentID;
            reply.AgentData.SessionID = Client.Self.SessionID;

            reply.Data.ButtonIndex = buttonIndex;
            reply.Data.ButtonLabel = Utils.StringToBytes(buttonlabel);
            reply.Data.ChatChannel = channel;
            reply.Data.ObjectID = objectID;

            Client.Network.SendPacket(reply);
        }

        #endregion Chat and instant messages

        #region Viewer Effects

        /// <summary>
        /// Start a particle stream between an agent and an object
        /// </summary>
        /// <param name="sourceAvatar"><seealso cref="UUID"/> Key of the source agent</param>
        /// <param name="targetObject"><seealso cref="UUID"/> Key of the target object</param>
        /// <param name="globalOffset"></param>
        /// <param name="type">The type from the <seealso cref="T:PointAtType"/> enum</param>
        /// <param name="effectID">A unique <seealso cref="UUID"/> for this effect</param>
        public void PointAtEffect(UUID sourceAvatar, UUID targetObject, Vector3d globalOffset, PointAtType type,
            UUID effectID)
        {
            ViewerEffectPacket effect = new ViewerEffectPacket();

            effect.AgentData.AgentID = Client.Self.AgentID;
            effect.AgentData.SessionID = Client.Self.SessionID;

            effect.Effect = new ViewerEffectPacket.EffectBlock[1];
            effect.Effect[0] = new ViewerEffectPacket.EffectBlock();
            effect.Effect[0].AgentID = Client.Self.AgentID;
            effect.Effect[0].Color = new byte[4];
            effect.Effect[0].Duration = (type == PointAtType.Clear) ? 0.0f : Single.MaxValue / 4.0f;
            effect.Effect[0].ID = effectID;
            effect.Effect[0].Type = (byte)EffectType.PointAt;

            byte[] typeData = new byte[57];
            if (sourceAvatar != UUID.Zero)
                Buffer.BlockCopy(sourceAvatar.GetBytes(), 0, typeData, 0, 16);
            if (targetObject != UUID.Zero)
                Buffer.BlockCopy(targetObject.GetBytes(), 0, typeData, 16, 16);
            Buffer.BlockCopy(globalOffset.GetBytes(), 0, typeData, 32, 24);
            typeData[56] = (byte)type;

            effect.Effect[0].TypeData = typeData;

            Client.Network.SendPacket(effect);
        }

        /// <summary>
        /// Start a particle stream between an agent and an object
        /// </summary>
        /// <param name="sourceAvatar"><seealso cref="UUID"/> Key of the source agent</param>
        /// <param name="targetObject"><seealso cref="UUID"/> Key of the target object</param>
        /// <param name="globalOffset">A <seealso cref="Vector3d"/> representing the beams offset from the source</param>
        /// <param name="type">A <seealso cref="T:PointAtType"/> which sets the avatars lookat animation</param>
        /// <param name="effectID"><seealso cref="UUID"/> of the Effect</param>
        public void LookAtEffect(UUID sourceAvatar, UUID targetObject, Vector3d globalOffset, LookAtType type,
            UUID effectID)
        {
            ViewerEffectPacket effect = new ViewerEffectPacket();

            effect.AgentData.AgentID = Client.Self.AgentID;
            effect.AgentData.SessionID = Client.Self.SessionID;

            float duration;

            switch (type)
            {
                case LookAtType.Clear:
                    duration = 0.0f;
                    break;
                case LookAtType.Hover:
                    duration = 1.0f;
                    break;
                case LookAtType.FreeLook:
                    duration = 2.0f;
                    break;
                case LookAtType.Idle:
                    duration = 3.0f;
                    break;
                case LookAtType.AutoListen:
                case LookAtType.Respond:
                    duration = 4.0f;
                    break;
                case LookAtType.None:
                case LookAtType.Select:
                case LookAtType.Focus:
                case LookAtType.Mouselook:
                    duration = Single.MaxValue / 2.0f;
                    break;
                default:
                    duration = 0.0f;
                    break;
            }

            effect.Effect = new ViewerEffectPacket.EffectBlock[1];
            effect.Effect[0] = new ViewerEffectPacket.EffectBlock();
            effect.Effect[0].AgentID = Client.Self.AgentID;
            effect.Effect[0].Color = new byte[4];
            effect.Effect[0].Duration = duration;
            effect.Effect[0].ID = effectID;
            effect.Effect[0].Type = (byte)EffectType.LookAt;

            byte[] typeData = new byte[57];
            if (sourceAvatar != UUID.Zero)
                Buffer.BlockCopy(sourceAvatar.GetBytes(), 0, typeData, 0, 16);
            if (targetObject != UUID.Zero)
                Buffer.BlockCopy(targetObject.GetBytes(), 0, typeData, 16, 16);
            typeData[56] = (byte)type;

            effect.Effect[0].TypeData = typeData;

            Client.Network.SendPacket(effect);
        }

        /// <summary>
        /// Create a particle beam between an avatar and an primitive
        /// </summary>
        /// <param name="sourceAvatar"><seealso cref="UUID"/> of sources avatar</param>
        /// <param name="targetObject"><seealso cref="UUID"/> of the target</param>
        /// <param name="globalOffset"><seealso cref="Vector3d"/>global offset</param>
        /// <param name="color"><seealso cref="Color4"/>Color values of beam</param>
        /// <param name="duration">a float representing the duration the beam will last</param>
        /// <param name="effectID"><seealso cref="UUID"/> of the Effect</param>
        public void BeamEffect(UUID sourceAvatar, UUID targetObject, Vector3d globalOffset, Color4 color,
            float duration, UUID effectID)
        {
            ViewerEffectPacket effect = new ViewerEffectPacket();

            effect.AgentData.AgentID = Client.Self.AgentID;
            effect.AgentData.SessionID = Client.Self.SessionID;

            effect.Effect = new ViewerEffectPacket.EffectBlock[1];
            effect.Effect[0] = new ViewerEffectPacket.EffectBlock();
            effect.Effect[0].AgentID = Client.Self.AgentID;
            effect.Effect[0].Color = color.GetBytes();
            effect.Effect[0].Duration = duration;
            effect.Effect[0].ID = effectID;
            effect.Effect[0].Type = (byte)EffectType.Beam;

            byte[] typeData = new byte[56];
            Buffer.BlockCopy(sourceAvatar.GetBytes(), 0, typeData, 0, 16);
            Buffer.BlockCopy(targetObject.GetBytes(), 0, typeData, 16, 16);
            Buffer.BlockCopy(globalOffset.GetBytes(), 0, typeData, 32, 24);

            effect.Effect[0].TypeData = typeData;

            Client.Network.SendPacket(effect);
        }

        #endregion Viewer Effects

        #region Movement Actions

        /// <summary>
        /// Sends a request to sit on the specified object
        /// </summary>
        /// <param name="targetID"><seealso cref="UUID"/> of the object to sit on</param>
        /// <param name="offset">Sit at offset</param>
        public void RequestSit(UUID targetID, Vector3 offset)
        {
            AgentRequestSitPacket requestSit = new AgentRequestSitPacket();
            requestSit.AgentData.AgentID = Client.Self.AgentID;
            requestSit.AgentData.SessionID = Client.Self.SessionID;
            requestSit.TargetObject.TargetID = targetID;
            requestSit.TargetObject.Offset = offset;
            Client.Network.SendPacket(requestSit);
        }

        /// <summary>
        /// Follows a call to <seealso cref="RequestSit"/> to actually sit on the object
        /// </summary>
        public void Sit()
        {
            AgentSitPacket sit = new AgentSitPacket();
            sit.AgentData.AgentID = Client.Self.AgentID;
            sit.AgentData.SessionID = Client.Self.SessionID;
            Client.Network.SendPacket(sit);
        }

        /// <summary>Stands up from sitting on a prim or the ground</summary>
        /// <returns>true of AgentUpdate was sent</returns>
        public bool Stand()
        {
            if (Client.Settings.SEND_AGENT_UPDATES)
            {
                Movement.SitOnGround = false;
                Movement.StandUp = true;
                Movement.SendUpdate();
                Movement.StandUp = false;
                Movement.SendUpdate();
                return true;
            }
            else
            {
                Logger.Log("Attempted to Stand() but agent updates are disabled", Helpers.LogLevel.Warning, Client);
                return false;
            }
        }

        /// <summary>
        /// Does a "ground sit" at the avatar's current position
        /// </summary>
        public void SitOnGround()
        {
            Movement.SitOnGround = true;
            Movement.SendUpdate(true);
        }

        /// <summary>
        /// Starts or stops flying
        /// </summary>
        /// <param name="start">True to start flying, false to stop flying</param>
        public void Fly(bool start)
        {
            if (start)
                Movement.Fly = true;
            else
                Movement.Fly = false;

            Movement.SendUpdate(true);
        }

        /// <summary>
        /// Starts or stops crouching
        /// </summary>
        /// <param name="crouching">True to start crouching, false to stop crouching</param>
        public void Crouch(bool crouching)
        {
            Movement.UpNeg = crouching;
            Movement.SendUpdate(true);
        }

        /// <summary>
        /// Starts a jump (begin holding the jump key)
        /// </summary>
        public void Jump(bool jumping)
        {
            Movement.UpPos = jumping;
            Movement.FastUp = jumping;
            Movement.SendUpdate(true);
        }

        /// <summary>
        /// Use the autopilot sim function to move the avatar to a new
        /// position. Uses double precision to get precise movements
        /// </summary>
        /// <remarks>The z value is currently not handled properly by the simulator</remarks>
        /// <param name="globalX">Global X coordinate to move to</param>
        /// <param name="globalY">Global Y coordinate to move to</param>
        /// <param name="z">Z coordinate to move to</param>
        public void AutoPilot(double globalX, double globalY, double z)
        {
            GenericMessagePacket autopilot = new GenericMessagePacket();

            autopilot.AgentData.AgentID = Client.Self.AgentID;
            autopilot.AgentData.SessionID = Client.Self.SessionID;
            autopilot.AgentData.TransactionID = UUID.Zero;
            autopilot.MethodData.Invoice = UUID.Zero;
            autopilot.MethodData.Method = Utils.StringToBytes("autopilot");
            autopilot.ParamList = new GenericMessagePacket.ParamListBlock[3];
            autopilot.ParamList[0] = new GenericMessagePacket.ParamListBlock();
            autopilot.ParamList[0].Parameter = Utils.StringToBytes(globalX.ToString());
            autopilot.ParamList[1] = new GenericMessagePacket.ParamListBlock();
            autopilot.ParamList[1].Parameter = Utils.StringToBytes(globalY.ToString());
            autopilot.ParamList[2] = new GenericMessagePacket.ParamListBlock();
            autopilot.ParamList[2].Parameter = Utils.StringToBytes(z.ToString());

            Client.Network.SendPacket(autopilot);
        }

        /// <summary>
        /// Use the autopilot sim function to move the avatar to a new position
        /// </summary>
        /// <remarks>The z value is currently not handled properly by the simulator</remarks>
        /// <param name="globalX">Integer value for the global X coordinate to move to</param>
        /// <param name="globalY">Integer value for the global Y coordinate to move to</param>
        /// <param name="z">Floating-point value for the Z coordinate to move to</param>
        public void AutoPilot(ulong globalX, ulong globalY, float z)
        {
            GenericMessagePacket autopilot = new GenericMessagePacket();

            autopilot.AgentData.AgentID = Client.Self.AgentID;
            autopilot.AgentData.SessionID = Client.Self.SessionID;
            autopilot.AgentData.TransactionID = UUID.Zero;
            autopilot.MethodData.Invoice = UUID.Zero;
            autopilot.MethodData.Method = Utils.StringToBytes("autopilot");
            autopilot.ParamList = new GenericMessagePacket.ParamListBlock[3];
            autopilot.ParamList[0] = new GenericMessagePacket.ParamListBlock();
            autopilot.ParamList[0].Parameter = Utils.StringToBytes(globalX.ToString());
            autopilot.ParamList[1] = new GenericMessagePacket.ParamListBlock();
            autopilot.ParamList[1].Parameter = Utils.StringToBytes(globalY.ToString());
            autopilot.ParamList[2] = new GenericMessagePacket.ParamListBlock();
            autopilot.ParamList[2].Parameter = Utils.StringToBytes(z.ToString());

            Client.Network.SendPacket(autopilot);
        }

        /// <summary>
        /// Use the autopilot sim function to move the avatar to a new position
        /// </summary>
        /// <remarks>The z value is currently not handled properly by the simulator</remarks>
        /// <param name="localX">Integer value for the local X coordinate to move to</param>
        /// <param name="localY">Integer value for the local Y coordinate to move to</param>
        /// <param name="z">Floating-point value for the Z coordinate to move to</param>
        public void AutoPilotLocal(int localX, int localY, float z)
        {
            uint x, y;
            Utils.LongToUInts(Client.Network.CurrentSim.Handle, out x, out y);
            AutoPilot((ulong)(x + localX), (ulong)(y + localY), z);
        }

        /// <summary>Macro to cancel autopilot sim function</summary>
        /// <remarks>Not certain if this is how it is really done</remarks>
        /// <returns>true if control flags were set and AgentUpdate was sent to the simulator</returns>
        public bool AutoPilotCancel()
        {
            if (Client.Settings.SEND_AGENT_UPDATES)
            {
                Movement.AtPos = true;
                Movement.SendUpdate();
                Movement.AtPos = false;
                Movement.SendUpdate();
                return true;
            }
            else
            {
                Logger.Log("Attempted to AutoPilotCancel() but agent updates are disabled", Helpers.LogLevel.Warning, Client);
                return false;
            }
        }

        #endregion Movement actions

        #region Touch and grab

        /// <summary>
        /// Grabs an object
        /// </summary>
        /// <param name="objectLocalID">an unsigned integer of the objects ID within the simulator</param>
        /// <seealso cref="T:OpenMetaverse.NetworkManager.CurrentSim.ObjectsPrimitives"/>
        public void Grab(uint objectLocalID)
        {
            ObjectGrabPacket grab = new ObjectGrabPacket();
            grab.AgentData.AgentID = Client.Self.AgentID;
            grab.AgentData.SessionID = Client.Self.SessionID;
            grab.ObjectData.LocalID = objectLocalID;
            grab.ObjectData.GrabOffset = new Vector3(0, 0, 0);
            Client.Network.SendPacket(grab);
        }

        /// <summary>
        /// Drags an object
        /// </summary>
        /// <param name="objectID"><seealso cref="UUID"/> of the object to drag</param>
        /// <param name="grabPosition">Drag target in region coordinates</param>
        public void GrabUpdate(UUID objectID, Vector3 grabPosition)
        {
            ObjectGrabUpdatePacket grab = new ObjectGrabUpdatePacket();
            grab.AgentData.AgentID = Client.Self.AgentID;
            grab.AgentData.SessionID = Client.Self.SessionID;
            grab.ObjectData.ObjectID = objectID;
            grab.ObjectData.GrabOffsetInitial = new Vector3(0, 0, 0);
            grab.ObjectData.GrabPosition = grabPosition;
            grab.ObjectData.TimeSinceLast = 0;
            Client.Network.SendPacket(grab);
        }

        /// <summary>
        /// Releases a grabbed object
        /// </summary>
        /// <param name="objectLocalID">The Objects Simulator Local ID</param>
        /// <seealso cref="T:OpenMetaverse.NetworkManager.CurrentSim.ObjectsPrimitives"/>
        /// <seealso cref="Grab"/>
        /// <seealso cref="GrabUpdate"/>
        public void DeGrab(uint objectLocalID)
        {
            ObjectDeGrabPacket degrab = new ObjectDeGrabPacket();
            degrab.AgentData.AgentID = Client.Self.AgentID;
            degrab.AgentData.SessionID = Client.Self.SessionID;
            degrab.ObjectData.LocalID = objectLocalID;
            Client.Network.SendPacket(degrab);
        }

        /// <summary>
        /// Touches an object
        /// </summary>
        /// <param name="objectLocalID">an unsigned integer of the objects ID within the simulator</param>
        /// <seealso cref="T:OpenMetaverse.NetworkManager.CurrentSim.ObjectsPrimitives"/>
        public void Touch(uint objectLocalID)
        {
            Client.Self.Grab(objectLocalID);
            Client.Self.DeGrab(objectLocalID);
        }

        #endregion Touch and grab

        #region Money

        /// <summary>
        /// Request the current L$ balance
        /// </summary>
        public void RequestBalance()
        {
            MoneyBalanceRequestPacket money = new MoneyBalanceRequestPacket();
            money.AgentData.AgentID = Client.Self.AgentID;
            money.AgentData.SessionID = Client.Self.SessionID;
            money.MoneyData.TransactionID = UUID.Zero;

            Client.Network.SendPacket(money);
        }

        /// <summary>
        /// Give Money to destination Avatar
        /// </summary>
        /// <param name="target">UUID of the Target Avatar</param>
        /// <param name="amount">Amount in L$</param>
        public void GiveAvatarMoney(UUID target, int amount)
        {
            GiveMoney(target, amount, String.Empty, MoneyTransactionType.Gift, TransactionFlags.None);
        }

        /// <summary>
        /// Give Money to destination Avatar
        /// </summary>
        /// <param name="target">UUID of the Target Avatar</param>
        /// <param name="amount">Amount in L$</param>
        /// <param name="description">Description that will show up in the
        /// recipients transaction history</param>
        public void GiveAvatarMoney(UUID target, int amount, string description)
        {
            GiveMoney(target, amount, description, MoneyTransactionType.Gift, TransactionFlags.None);
        }

        /// <summary>
        /// Give L$ to an object
        /// </summary>
        /// <param name="target">object <seealso cref="UUID"/> to give money to</param>
        /// <param name="amount">amount of L$ to give</param>
        /// <param name="objectName">name of object</param>
        public void GiveObjectMoney(UUID target, int amount, string objectName)
        {
            GiveMoney(target, amount, objectName, MoneyTransactionType.PayObject, TransactionFlags.None);
        }

        /// <summary>
        /// Give L$ to a group
        /// </summary>
        /// <param name="target">group <seealso cref="UUID"/> to give money to</param>
        /// <param name="amount">amount of L$ to give</param>
        public void GiveGroupMoney(UUID target, int amount)
        {
            GiveMoney(target, amount, String.Empty, MoneyTransactionType.Gift, TransactionFlags.DestGroup);
        }

        /// <summary>
        /// Give L$ to a group
        /// </summary>
        /// <param name="target">group <seealso cref="UUID"/> to give money to</param>
        /// <param name="amount">amount of L$ to give</param>
        /// <param name="description">description of transaction</param>
        public void GiveGroupMoney(UUID target, int amount, string description)
        {
            GiveMoney(target, amount, description, MoneyTransactionType.Gift, TransactionFlags.DestGroup);
        }

        /// <summary>
        /// Pay texture/animation upload fee
        /// </summary>
        public void PayUploadFee()
        {
            GiveMoney(UUID.Zero, Client.Settings.UPLOAD_COST, String.Empty, MoneyTransactionType.UploadCharge,
                TransactionFlags.None);
        }

        /// <summary>
        /// Pay texture/animation upload fee
        /// </summary>
        /// <param name="description">description of the transaction</param>
        public void PayUploadFee(string description)
        {
            GiveMoney(UUID.Zero, Client.Settings.UPLOAD_COST, description, MoneyTransactionType.UploadCharge,
                TransactionFlags.None);
        }

        /// <summary>
        /// Give Money to destionation Object or Avatar
        /// </summary>
        /// <param name="target">UUID of the Target Object/Avatar</param>
        /// <param name="amount">Amount in L$</param>
        /// <param name="description">Reason (Optional normally)</param>
        /// <param name="type">The type of transaction</param>
        /// <param name="flags">Transaction flags, mostly for identifying group
        /// transactions</param>
        public void GiveMoney(UUID target, int amount, string description, MoneyTransactionType type, TransactionFlags flags)
        {
            MoneyTransferRequestPacket money = new MoneyTransferRequestPacket();
            money.AgentData.AgentID = this.id;
            money.AgentData.SessionID = Client.Self.SessionID;
            money.MoneyData.Description = Utils.StringToBytes(description);
            money.MoneyData.DestID = target;
            money.MoneyData.SourceID = this.id;
            money.MoneyData.TransactionType = (int)type;
            money.MoneyData.AggregatePermInventory = 0; // This is weird, apparently always set to zero though
            money.MoneyData.AggregatePermNextOwner = 0; // This is weird, apparently always set to zero though
            money.MoneyData.Flags = (byte)flags;
            money.MoneyData.Amount = amount;

            Client.Network.SendPacket(money);
        }

        #endregion Money

        #region Animations

        /// <summary>
        /// Send an AgentAnimation packet that toggles a single animation on
        /// </summary>
        /// <param name="animation">The <seealso cref="UUID"/> of the animation to start playing</param>
        /// <param name="reliable">Whether to ensure delivery of this packet or not</param>
        public void AnimationStart(UUID animation, bool reliable)
        {
            Dictionary<UUID, bool> animations = new Dictionary<UUID, bool>();
            animations[animation] = true;

            Animate(animations, reliable);
        }

        /// <summary>
        /// Send an AgentAnimation packet that toggles a single animation off
        /// </summary>
        /// <param name="animation">The <seealso cref="UUID"/> of a 
        /// currently playing animation to stop playing</param>
        /// <param name="reliable">Whether to ensure delivery of this packet or not</param>
        public void AnimationStop(UUID animation, bool reliable)
        {
            Dictionary<UUID, bool> animations = new Dictionary<UUID, bool>();
            animations[animation] = false;

            Animate(animations, reliable);
        }

        /// <summary>
        /// Send an AgentAnimation packet that will toggle animations on or off
        /// </summary>
        /// <param name="animations">A list of animation <seealso cref="UUID"/>s, and whether to
        /// turn that animation on or off</param>
        /// <param name="reliable">Whether to ensure delivery of this packet or not</param>
        public void Animate(Dictionary<UUID, bool> animations, bool reliable)
        {
            AgentAnimationPacket animate = new AgentAnimationPacket();
            animate.Header.Reliable = reliable;

            animate.AgentData.AgentID = Client.Self.AgentID;
            animate.AgentData.SessionID = Client.Self.SessionID;
            animate.AnimationList = new AgentAnimationPacket.AnimationListBlock[animations.Count];
            int i = 0;

            foreach (KeyValuePair<UUID, bool> animation in animations)
            {
                animate.AnimationList[i] = new AgentAnimationPacket.AnimationListBlock();
                animate.AnimationList[i].AnimID = animation.Key;
                animate.AnimationList[i].StartAnim = animation.Value;

                i++;
            }

            Client.Network.SendPacket(animate);
        }

        #endregion Animations

        #region Teleporting

        /// <summary>
        /// Teleports agent to their stored home location
        /// </summary>
        /// <returns>true on successful teleport to home location</returns>
        public bool GoHome()
        {
            return Teleport(UUID.Zero);
        }

        /// <summary>
        /// Teleport agent to a landmark
        /// </summary>
        /// <param name="landmark"><seealso cref="UUID"/> of the landmark to teleport agent to</param>
        /// <returns>true on success, false on failure</returns>
        public bool Teleport(UUID landmark)
        {
            teleportStat = TeleportStatus.None;
            teleportEvent.Reset();
            TeleportLandmarkRequestPacket p = new TeleportLandmarkRequestPacket();
            p.Info = new TeleportLandmarkRequestPacket.InfoBlock();
            p.Info.AgentID = Client.Self.AgentID;
            p.Info.SessionID = Client.Self.SessionID;
            p.Info.LandmarkID = landmark;
            Client.Network.SendPacket(p);

            teleportEvent.WaitOne(Client.Settings.TELEPORT_TIMEOUT, false);

            if (teleportStat == TeleportStatus.None ||
                teleportStat == TeleportStatus.Start ||
                teleportStat == TeleportStatus.Progress)
            {
                teleportMessage = "Teleport timed out.";
                teleportStat = TeleportStatus.Failed;
            }

            return (teleportStat == TeleportStatus.Finished);
        }

        /// <summary>
        /// Attempt to look up a simulator name and teleport to the discovered
        /// destination
        /// </summary>
        /// <param name="simName">Region name to look up</param>
        /// <param name="position">Position to teleport to</param>
        /// <returns>True if the lookup and teleport were successful, otherwise
        /// false</returns>
        public bool Teleport(string simName, Vector3 position)
        {
            return Teleport(simName, position, new Vector3(0, 1.0f, 0));
        }

        /// <summary>
        /// Attempt to look up a simulator name and teleport to the discovered
        /// destination
        /// </summary>
        /// <param name="simName">Region name to look up</param>
        /// <param name="position">Position to teleport to</param>
        /// <param name="lookAt">Target to look at</param>
        /// <returns>True if the lookup and teleport were successful, otherwise
        /// false</returns>
        public bool Teleport(string simName, Vector3 position, Vector3 lookAt)
        {
            teleportStat = TeleportStatus.None;
            simName = simName.ToLower();

            if (simName != Client.Network.CurrentSim.Name.ToLower())
            {
                // Teleporting to a foreign sim
                GridRegion region;

                if (Client.Grid.GetGridRegion(simName, GridLayerType.Objects, out region))
                {
                    return Teleport(region.RegionHandle, position, lookAt);
                }
                else
                {
                    teleportMessage = "Unable to resolve name: " + simName;
                    teleportStat = TeleportStatus.Failed;
                    return false;
                }
            }
            else
            {
                // Teleporting to the sim we're already in
                return Teleport(Client.Network.CurrentSim.Handle, position, lookAt);
            }
        }

        /// <summary>
        /// Teleport agent to another region
        /// </summary>
        /// <param name="regionHandle">handle of region to teleport agent to</param>
        /// <param name="position"><seealso cref="Vector3"/> position in destination sim to teleport to</param>
        /// <returns>true on success, false on failure</returns>
        /// <remarks>This call is blocking</remarks>
        public bool Teleport(ulong regionHandle, Vector3 position)
        {
            return Teleport(regionHandle, position, new Vector3(0.0f, 1.0f, 0.0f));
        }

        /// <summary>
        /// Teleport agent to another region
        /// </summary>
        /// <param name="regionHandle">handle of region to teleport agent to</param>
        /// <param name="position"><seealso cref="Vector3"/> position in destination sim to teleport to</param>
        /// <param name="lookAt"><seealso cref="Vector3"/> direction in destination sim agent will look at</param>
        /// <returns>true on success, false on failure</returns>
        /// <remarks>This call is blocking</remarks>
        public bool Teleport(ulong regionHandle, Vector3 position, Vector3 lookAt)
        {
            if (Client.Network.CurrentSim == null ||
                Client.Network.CurrentSim.Caps == null ||
                !Client.Network.CurrentSim.Caps.IsEventQueueRunning)
            {
                // Wait a bit to see if the event queue comes online
                AutoResetEvent queueEvent = new AutoResetEvent(false);
                NetworkManager.EventQueueRunningCallback queueCallback =
                    delegate(Simulator simulator)
                    {
                        if (simulator == Client.Network.CurrentSim)
                            queueEvent.Set();
                    };

                Client.Network.OnEventQueueRunning += queueCallback;
                queueEvent.WaitOne(10 * 1000, false);
                Client.Network.OnEventQueueRunning -= queueCallback;
            }

            teleportStat = TeleportStatus.None;
            teleportEvent.Reset();

            RequestTeleport(regionHandle, position, lookAt);

            teleportEvent.WaitOne(Client.Settings.TELEPORT_TIMEOUT, false);

            if (teleportStat == TeleportStatus.None ||
                teleportStat == TeleportStatus.Start ||
                teleportStat == TeleportStatus.Progress)
            {
                teleportMessage = "Teleport timed out.";
                teleportStat = TeleportStatus.Failed;
            }

            return (teleportStat == TeleportStatus.Finished);
        }

        /// <summary>
        /// Request teleport to a another simulator
        /// </summary>
        /// <param name="regionHandle">handle of region to teleport agent to</param>
        /// <param name="position"><seealso cref="Vector3"/> position in destination sim to teleport to</param>
        public void RequestTeleport(ulong regionHandle, Vector3 position)
        {
            RequestTeleport(regionHandle, position, new Vector3(0.0f, 1.0f, 0.0f));
        }

        /// <summary>
        /// Request teleport to a another simulator
        /// </summary>
        /// <param name="regionHandle">handle of region to teleport agent to</param>
        /// <param name="position"><seealso cref="Vector3"/> position in destination sim to teleport to</param>
        /// <param name="lookAt"><seealso cref="Vector3"/> direction in destination sim agent will look at</param>
        public void RequestTeleport(ulong regionHandle, Vector3 position, Vector3 lookAt)
        {
            if (Client.Network.CurrentSim != null &&
                Client.Network.CurrentSim.Caps != null &&
                Client.Network.CurrentSim.Caps.IsEventQueueRunning)
            {
                TeleportLocationRequestPacket teleport = new TeleportLocationRequestPacket();
                teleport.AgentData.AgentID = Client.Self.AgentID;
                teleport.AgentData.SessionID = Client.Self.SessionID;
                teleport.Info.LookAt = lookAt;
                teleport.Info.Position = position;
                teleport.Info.RegionHandle = regionHandle;

                Logger.Log("Requesting teleport to region handle " + regionHandle.ToString(), Helpers.LogLevel.Info, Client);

                Client.Network.SendPacket(teleport);
            }
            else
            {
                teleportMessage = "CAPS event queue is not running";
                teleportEvent.Set();
                teleportStat = TeleportStatus.Failed;
            }
        }

        /// <summary>
        /// Teleport agent to a landmark
        /// </summary>
        /// <param name="landmark"><seealso cref="UUID"/> of the landmark to teleport agent to</param>
        public void RequestTeleport(UUID landmark)
        {
            TeleportLandmarkRequestPacket p = new TeleportLandmarkRequestPacket();
            p.Info = new TeleportLandmarkRequestPacket.InfoBlock();
            p.Info.AgentID = Client.Self.AgentID;
            p.Info.SessionID = Client.Self.SessionID;
            p.Info.LandmarkID = landmark;
            Client.Network.SendPacket(p);
        }

        /// <summary>
        /// Send a teleport lure to another avatar with default "Join me in ..." invitation message
        /// </summary>
        /// <param name="targetID">target avatars <seealso cref="UUID"/> to lure</param>
        public void SendTeleportLure(UUID targetID)
        {
            SendTeleportLure(targetID, "Join me in " + Client.Network.CurrentSim.Name + "!");
        }

        /// <summary>
        /// Send a teleport lure to another avatar with custom invitation message
        /// </summary>
        /// <param name="targetID">target avatars <seealso cref="UUID"/> to lure</param>
        /// <param name="message">custom message to send with invitation</param>
        public void SendTeleportLure(UUID targetID, string message)
        {
            StartLurePacket p = new StartLurePacket();
            p.AgentData.AgentID = Client.Self.id;
            p.AgentData.SessionID = Client.Self.SessionID;
            p.Info.LureType = 0;
            p.Info.Message = Utils.StringToBytes(message);
            p.TargetData = new StartLurePacket.TargetDataBlock[] { new StartLurePacket.TargetDataBlock() };
            p.TargetData[0].TargetID = targetID;
            Client.Network.SendPacket(p);
        }

        /// <summary>
        /// Respond to a teleport lure by either accepting it and initiating 
        /// the teleport, or denying it
        /// </summary>
        /// <param name="requesterID"><seealso cref="UUID"/> of the avatar sending the lure</param>
        /// <param name="accept">true to accept the lure, false to decline it</param>
        public void TeleportLureRespond(UUID requesterID, bool accept)
        {
            InstantMessage(Name, requesterID, String.Empty, UUID.Random(),
                accept ? InstantMessageDialog.AcceptTeleport : InstantMessageDialog.DenyTeleport,
                InstantMessageOnline.Offline, this.SimPosition, UUID.Zero, new byte[0]);

            if (accept)
            {
                TeleportLureRequestPacket lure = new TeleportLureRequestPacket();

                lure.Info.AgentID = Client.Self.AgentID;
                lure.Info.SessionID = Client.Self.SessionID;
                lure.Info.LureID = Client.Self.AgentID;
                lure.Info.TeleportFlags = (uint)TeleportFlags.ViaLure;

                Client.Network.SendPacket(lure);
            }
        }

        #endregion Teleporting

        #region Misc

        /// <summary>
        /// Update agent profile
        /// </summary>
        /// <param name="profile"><seealso cref="OpenMetaverse.Avatar.AvatarProperties"/> struct containing updated 
        /// profile information</param>
        public void UpdateProfile(Avatar.AvatarProperties profile)
        {
            AvatarPropertiesUpdatePacket apup = new AvatarPropertiesUpdatePacket();
            apup.AgentData.AgentID = id;
            apup.AgentData.SessionID = sessionID;
            apup.PropertiesData.AboutText = Utils.StringToBytes(profile.AboutText);
            apup.PropertiesData.AllowPublish = profile.AllowPublish;
            apup.PropertiesData.FLAboutText = Utils.StringToBytes(profile.FirstLifeText);
            apup.PropertiesData.FLImageID = profile.FirstLifeImage;
            apup.PropertiesData.ImageID = profile.ProfileImage;
            apup.PropertiesData.MaturePublish = profile.MaturePublish;
            apup.PropertiesData.ProfileURL = Utils.StringToBytes(profile.ProfileURL);

            Client.Network.SendPacket(apup);
        }

        /// <summary>
        /// Update agents profile interests
        /// </summary>
        /// <param name="interests">selection of interests from <seealso cref="T:OpenMetaverse.Avatar.Interests"/> struct</param>
        public void UpdateInterests(Avatar.Interests interests)
        {
            AvatarInterestsUpdatePacket aiup = new AvatarInterestsUpdatePacket();
            aiup.AgentData.AgentID = id;
            aiup.AgentData.SessionID = sessionID;
            aiup.PropertiesData.LanguagesText = Utils.StringToBytes(interests.LanguagesText);
            aiup.PropertiesData.SkillsMask = interests.SkillsMask;
            aiup.PropertiesData.SkillsText = Utils.StringToBytes(interests.SkillsText);
            aiup.PropertiesData.WantToMask = interests.WantToMask;
            aiup.PropertiesData.WantToText = Utils.StringToBytes(interests.WantToText);

            Client.Network.SendPacket(aiup);
        }

        /// <summary>
        /// Set the height and the width of the client window. This is used
        /// by the server to build a virtual camera frustum for our avatar
        /// </summary>
        /// <param name="height">New height of the viewer window</param>
        /// <param name="width">New width of the viewer window</param>
        public void SetHeightWidth(ushort height, ushort width)
        {
            AgentHeightWidthPacket heightwidth = new AgentHeightWidthPacket();
            heightwidth.AgentData.AgentID = Client.Self.AgentID;
            heightwidth.AgentData.SessionID = Client.Self.SessionID;
            heightwidth.AgentData.CircuitCode = Client.Network.CircuitCode;
            heightwidth.HeightWidthBlock.Height = height;
            heightwidth.HeightWidthBlock.Width = width;
            heightwidth.HeightWidthBlock.GenCounter = heightWidthGenCounter++;

            Client.Network.SendPacket(heightwidth);
        }

        /// <summary>
        /// Request the list of muted objects and avatars for this agent
        /// </summary>
        public void RequestMuteList()
        {
            MuteListRequestPacket mute = new MuteListRequestPacket();
            mute.AgentData.AgentID = Client.Self.AgentID;
            mute.AgentData.SessionID = Client.Self.SessionID;
            mute.MuteData.MuteCRC = 0;

            Client.Network.SendPacket(mute);
        }

        /// <summary>
        /// Sets home location to agents current position
        /// </summary>
        /// <remarks>will fire an AlertMessage (<seealso cref="E:OpenMetaverse.AgentManager.OnAlertMessage"/>) with 
        /// success or failure message</remarks>
        public void SetHome()
        {
            SetStartLocationRequestPacket s = new SetStartLocationRequestPacket();
            s.AgentData = new SetStartLocationRequestPacket.AgentDataBlock();
            s.AgentData.AgentID = Client.Self.AgentID;
            s.AgentData.SessionID = Client.Self.SessionID;
            s.StartLocationData = new SetStartLocationRequestPacket.StartLocationDataBlock();
            s.StartLocationData.LocationPos = Client.Self.SimPosition;
            s.StartLocationData.LocationID = 1;
            s.StartLocationData.SimName = Utils.StringToBytes(String.Empty);
            s.StartLocationData.LocationLookAt = Movement.Camera.AtAxis;
            Client.Network.SendPacket(s);
        }

        /// <summary>
        /// Move an agent in to a simulator. This packet is the last packet
        /// needed to complete the transition in to a new simulator
        /// </summary>
        /// <param name="simulator"><seealso cref="T:OpenMetaverse.Simulator"/> Object</param>
        public void CompleteAgentMovement(Simulator simulator)
        {
            CompleteAgentMovementPacket move = new CompleteAgentMovementPacket();

            move.AgentData.AgentID = Client.Self.AgentID;
            move.AgentData.SessionID = Client.Self.SessionID;
            move.AgentData.CircuitCode = Client.Network.CircuitCode;

            Client.Network.SendPacket(move, simulator);
        }

        /// <summary>
        /// Reply to script permissions request
        /// </summary>
        /// <param name="simulator"><seealso cref="T:OpenMetaverse.Simulator"/> Object</param>
        /// <param name="itemID"><seealso cref="UUID"/> of the itemID requesting permissions</param>
        /// <param name="taskID"><seealso cref="UUID"/> of the taskID requesting permissions</param>
        /// <param name="permissions"><seealso cref="OpenMetaverse.ScriptPermission"/> list of permissions to allow</param>
        public void ScriptQuestionReply(Simulator simulator, UUID itemID, UUID taskID, ScriptPermission permissions)
        {
            ScriptAnswerYesPacket yes = new ScriptAnswerYesPacket();
            yes.AgentData.AgentID = Client.Self.AgentID;
            yes.AgentData.SessionID = Client.Self.SessionID;
            yes.Data.ItemID = itemID;
            yes.Data.TaskID = taskID;
            yes.Data.Questions = (int)permissions;

            Client.Network.SendPacket(yes, simulator);
        }

        /// <summary>
        /// Respond to a group invitation by either accepting or denying it
        /// </summary>
        /// <param name="groupID">UUID of the group (sent in the AgentID field of the invite message)</param>
        /// <param name="imSessionID">IM Session ID from the group invitation message</param>
        /// <param name="accept">Accept the group invitation or deny it</param>
        public void GroupInviteRespond(UUID groupID, UUID imSessionID, bool accept)
        {
            InstantMessage(Name, groupID, String.Empty, imSessionID,
                accept ? InstantMessageDialog.GroupInvitationAccept : InstantMessageDialog.GroupInvitationDecline,
                InstantMessageOnline.Offline, Vector3.Zero, UUID.Zero, new byte[0]);
        }

        /// <summary>
        /// Requests script detection of objects and avatars
        /// </summary>
        /// <param name="name">name of the object/avatar to search for</param>
        /// <param name="searchID">UUID of the object or avatar to search for</param>
        /// <param name="type">Type of search from ScriptSensorTypeFlags</param>
        /// <param name="range">range of scan (96 max?)</param>
        /// <param name="arc">the arc in radians to search within</param>
        /// <param name="requestID">an user generated ID to correlate replies with</param>
        /// <param name="sim">Simulator to perform search in</param>
        public void RequestScriptSensor(string name, UUID searchID, ScriptSensorTypeFlags type, float range, float arc, UUID requestID, Simulator sim)
        {
            ScriptSensorRequestPacket request = new ScriptSensorRequestPacket();
            request.Requester.Arc = arc;
            request.Requester.Range = range;
            request.Requester.RegionHandle = sim.Handle;
            request.Requester.RequestID = requestID;
            request.Requester.SearchDir = Quaternion.Identity; // TODO: this needs to be tested
            request.Requester.SearchID = searchID;
            request.Requester.SearchName = Utils.StringToBytes(name);
            request.Requester.SearchPos = Vector3.Zero;
            request.Requester.SearchRegions = 0; // TODO: ?
            request.Requester.SourceID = Client.Self.AgentID;
            request.Requester.Type = (int)type;

            Client.Network.SendPacket(request, sim);
        }

        /// <summary>
        /// Create or update profile pick
        /// </summary>
        /// <param name="pickID">UUID of the pick to update, or random UUID to create a new pick</param>
        /// <param name="topPick">Is this a top pick? (typically false)</param>
        /// <param name="parcelID">UUID of the parcel (UUID.Zero for the current parcel)</param>
        /// <param name="name">Name of the pick</param>
        /// <param name="globalPosition">Global position of the pick landmark</param>
        /// <param name="textureID">UUID of the image displayed with the pick</param>
        /// <param name="description">Long description of the pick</param>
        public void PickInfoUpdate(UUID pickID, bool topPick, UUID parcelID, string name, Vector3d globalPosition, UUID textureID, string description)
        {
            PickInfoUpdatePacket pick = new PickInfoUpdatePacket();
            pick.AgentData.AgentID = Client.Self.AgentID;
            pick.AgentData.SessionID = Client.Self.SessionID;
            pick.Data.PickID = pickID;
            pick.Data.Desc = Utils.StringToBytes(description);
            pick.Data.CreatorID = Client.Self.AgentID;
            pick.Data.TopPick = topPick;
            pick.Data.ParcelID = parcelID;
            pick.Data.Name = Utils.StringToBytes(name);
            pick.Data.SnapshotID = textureID;
            pick.Data.PosGlobal = Client.Self.GlobalPosition;
            pick.Data.SortOrder = 0;
            pick.Data.Enabled = false;

            Client.Network.SendPacket(pick);
        }

        /// <summary>
        /// Delete profile pick
        /// </summary>
        /// <param name="pickID">UUID of the pick to delete</param>
        public void PickDelete(UUID pickID)
        {
            PickDeletePacket delete = new PickDeletePacket();
            delete.AgentData.AgentID = Client.Self.AgentID;
            delete.AgentData.SessionID = Client.Self.sessionID;
            delete.Data.PickID = pickID;

            Client.Network.SendPacket(delete);
        }

        #endregion Misc

        #region Packet Handlers

        /// <summary>
        /// Take an incoming ImprovedInstantMessage packet, auto-parse, and if
        /// OnInstantMessage is defined call that with the appropriate arguments
        /// </summary>
        /// <param name="packet">Incoming ImprovedInstantMessagePacket</param>
        /// <param name="simulator">Unused</param>
        private void InstantMessageHandler(Packet packet, Simulator simulator)
        {
            if (packet.Type == PacketType.ImprovedInstantMessage)
            {
                ImprovedInstantMessagePacket im = (ImprovedInstantMessagePacket)packet;

                if (OnInstantMessage != null)
                {
                    InstantMessage message;
                    message.FromAgentID = im.AgentData.AgentID;
                    message.FromAgentName = Utils.BytesToString(im.MessageBlock.FromAgentName);
                    message.ToAgentID = im.MessageBlock.ToAgentID;
                    message.ParentEstateID = im.MessageBlock.ParentEstateID;
                    message.RegionID = im.MessageBlock.RegionID;
                    message.Position = im.MessageBlock.Position;
                    message.Dialog = (InstantMessageDialog)im.MessageBlock.Dialog;
                    message.GroupIM = im.MessageBlock.FromGroup;
                    message.IMSessionID = im.MessageBlock.ID;
                    message.Timestamp = new DateTime(im.MessageBlock.Timestamp);
                    message.Message = Utils.BytesToString(im.MessageBlock.Message);
                    message.Offline = (InstantMessageOnline)im.MessageBlock.Offline;
                    message.BinaryBucket = im.MessageBlock.BinaryBucket;

                    try { OnInstantMessage(message, simulator); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }
        }

        /// <summary>
        /// Take an incoming Chat packet, auto-parse, and if OnChat is defined call 
        ///   that with the appropriate arguments.
        /// </summary>
        /// <param name="packet">Incoming ChatFromSimulatorPacket</param>
        /// <param name="simulator">Unused</param>
        private void ChatHandler(Packet packet, Simulator simulator)
        {
            if (OnChat != null)
            {
                ChatFromSimulatorPacket chat = (ChatFromSimulatorPacket)packet;

                OnChat(Utils.BytesToString(chat.ChatData.Message)
                    , (ChatAudibleLevel)chat.ChatData.Audible
                    , (ChatType)chat.ChatData.ChatType
                    , (ChatSourceType)chat.ChatData.SourceType
                    , Utils.BytesToString(chat.ChatData.FromName)
                    , chat.ChatData.SourceID
                    , chat.ChatData.OwnerID
                    , chat.ChatData.Position
                    );
            }
        }

        /// <summary>
        /// Used for parsing llDialogs
        /// </summary>
        /// <param name="packet">Incoming ScriptDialog packet</param>
        /// <param name="simulator">Unused</param>
        private void ScriptDialogHandler(Packet packet, Simulator simulator)
        {
            if (OnScriptDialog != null)
            {
                ScriptDialogPacket dialog = (ScriptDialogPacket)packet;
                List<string> buttons = new List<string>();

                foreach (ScriptDialogPacket.ButtonsBlock button in dialog.Buttons)
                {
                    buttons.Add(Utils.BytesToString(button.ButtonLabel));
                }

                OnScriptDialog(Utils.BytesToString(dialog.Data.Message),
                    Utils.BytesToString(dialog.Data.ObjectName),
                    dialog.Data.ImageID,
                    dialog.Data.ObjectID,
                    Utils.BytesToString(dialog.Data.FirstName),
                    Utils.BytesToString(dialog.Data.LastName),
                    dialog.Data.ChatChannel,
                    buttons);
            }
        }

        /// <summary>
        /// Used for parsing llRequestPermissions dialogs
        /// </summary>
        /// <param name="packet">Incoming ScriptDialog packet</param>
        /// <param name="simulator">Unused</param>
        private void ScriptQuestionHandler(Packet packet, Simulator simulator)
        {
            if (OnScriptQuestion != null)
            {
                ScriptQuestionPacket question = (ScriptQuestionPacket)packet;

                try
                {
                    OnScriptQuestion(simulator,
                        question.Data.TaskID,
                        question.Data.ItemID,
                        Utils.BytesToString(question.Data.ObjectName),
                        Utils.BytesToString(question.Data.ObjectOwner),
                        (ScriptPermission)question.Data.Questions);
                }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary>
        /// Handles Script Control changes when Script with permissions releases or takes a control
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void ScriptControlChangeHandler(Packet packet, Simulator simulator)
        {
            if (OnScriptControlChange != null)
            {
                ScriptControlChangePacket change = (ScriptControlChangePacket)packet;
                for (int i = 0; i < change.Data.Length; i++)
                {
                    try
                    {
                        OnScriptControlChange((ScriptControlChange)change.Data[i].Controls,
                            change.Data[i].PassToAgent,
                            change.Data[i].TakeControls);
                    }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }
        }

        /// <summary>
        /// Used for parsing llLoadURL Dialogs
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void LoadURLHandler(Packet packet, Simulator simulator)
        {
            LoadURLPacket loadURL = (LoadURLPacket)packet;
            if (OnLoadURL != null)
            {
                try
                {
                    OnLoadURL(
                        Utils.BytesToString(loadURL.Data.ObjectName),
                        loadURL.Data.ObjectID,
                        loadURL.Data.OwnerID,
                        loadURL.Data.OwnerIsGroup,
                        Utils.BytesToString(loadURL.Data.Message),
                        Utils.BytesToString(loadURL.Data.URL)
                    );
                }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary>
        /// Update client's Position, LookAt and region handle from incoming packet
        /// </summary>
        /// <param name="packet">Incoming AgentMovementCompletePacket</param>
        /// <param name="simulator">Unused</param>
        /// <remarks>This occurs when after an avatar moves into a new sim</remarks>
        private void MovementCompleteHandler(Packet packet, Simulator simulator)
        {
            AgentMovementCompletePacket movement = (AgentMovementCompletePacket)packet;

            relativePosition = movement.Data.Position;
            Movement.Camera.LookDirection(movement.Data.LookAt);
            simulator.Handle = movement.Data.RegionHandle;
            simulator.SimVersion = Utils.BytesToString(movement.SimData.ChannelVersion);
        }

        /// <summary>
        /// Update Client Avatar's health via incoming packet
        /// </summary>
        /// <param name="packet">Incoming HealthMessagePacket</param>
        /// <param name="simulator">Unused</param>
        private void HealthHandler(Packet packet, Simulator simulator)
        {
            health = ((HealthMessagePacket)packet).HealthData.Health;
        }

        private void AgentDataUpdateHandler(Packet packet, Simulator simulator)
        {
            AgentDataUpdatePacket p = (AgentDataUpdatePacket)packet;

            if (p.AgentData.AgentID == simulator.Client.Self.AgentID)
            {
                firstName = Utils.BytesToString(p.AgentData.FirstName);
                lastName = Utils.BytesToString(p.AgentData.LastName);
                activeGroup = p.AgentData.ActiveGroupID;
                activeGroupPowers = (GroupPowers)p.AgentData.GroupPowers;

                if (OnAgentDataUpdated != null)
                {
                    string groupTitle = Utils.BytesToString(p.AgentData.GroupTitle);
                    string groupName = Utils.BytesToString(p.AgentData.GroupName);

                    try { OnAgentDataUpdated(firstName, lastName, activeGroup, groupTitle, (GroupPowers)p.AgentData.GroupPowers, groupName); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }
            else
            {
                Logger.Log("Got an AgentDataUpdate packet for avatar " + p.AgentData.AgentID.ToString() +
                    " instead of " + Client.Self.AgentID.ToString() + ", this shouldn't happen", Helpers.LogLevel.Error, Client);
            }
        }

        /// <summary>
        /// Update Client Avatar's L$ balance from incoming packet
        /// </summary>
        /// <param name="packet">Incoming MoneyBalanceReplyPacket</param>
        /// <param name="simulator">Unused</param>
        private void BalanceHandler(Packet packet, Simulator simulator)
        {
            if (packet.Type == PacketType.MoneyBalanceReply)
            {
                MoneyBalanceReplyPacket mbrp = (MoneyBalanceReplyPacket)packet;
                balance = mbrp.MoneyData.MoneyBalance;

                if (OnMoneyBalanceReplyReceived != null)
                {
                    try
                    {
                        OnMoneyBalanceReplyReceived(mbrp.MoneyData.TransactionID,
                      mbrp.MoneyData.TransactionSuccess, mbrp.MoneyData.MoneyBalance,
                      mbrp.MoneyData.SquareMetersCredit, mbrp.MoneyData.SquareMetersCommitted,
                      Utils.BytesToString(mbrp.MoneyData.Description));
                    }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }

            if (OnBalanceUpdated != null)
            {
                try { OnBalanceUpdated(balance); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void EstablishAgentCommunicationEventHandler(string message, OSD osd, Simulator simulator)
        {
            StructuredData.OSDMap body = (StructuredData.OSDMap)osd;


            if (Client.Settings.MULTIPLE_SIMS && body.ContainsKey("sim-ip-and-port"))
            {
                string ipAndPort = body["sim-ip-and-port"].AsString();
                string[] pieces = ipAndPort.Split(':');
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(pieces[0]), Convert.ToInt32(pieces[1]));
                Simulator sim = Client.Network.FindSimulator(endPoint);

                if (sim == null)
                {
                    Logger.Log("Got EstablishAgentCommunication for unknown sim " + ipAndPort,
                        Helpers.LogLevel.Error, Client);

                    // FIXME: Should we use this opportunity to connect to the simulator?
                }
                else
                {
                    Logger.Log("Got EstablishAgentCommunication for " + sim.ToString(),
                        Helpers.LogLevel.Info, Client);

                    sim.SetSeedCaps(body["seed-capability"].AsString());
                }
            }
        }

        /// <summary>
        /// Process TeleportFinish from Event Queue and pass it onto our TeleportHandler
        /// </summary>
        /// <param name="message"></param>
        /// <param name="osd"></param>
        /// <param name="simulator"></param>
        private void TeleportFinishEventHandler(string message, OSD osd, Simulator simulator)
        {
            
            OSDMap map = (OSDMap)osd;
            OSDArray array = (OSDArray)map["Info"];
            for (int i = 0; i < array.Count; i++)
            {
                TeleportFinishPacket p = new TeleportFinishPacket();
                OSDMap data = (OSDMap)array[i];
                p.Info.AgentID = data["AgentID"].AsUUID();
                p.Info.LocationID = Utils.BytesToUInt(data["LocationID"].AsBinary());
                p.Info.RegionHandle = Utils.BytesToUInt64(data["RegionHandle"].AsBinary());
                p.Info.SeedCapability = data["SeedCapability"].AsBinary();
                p.Info.SimAccess = (byte)data["SimAccess"].AsInteger();
                p.Info.SimIP = Utils.BytesToUInt(data["SimIP"].AsBinary());
                p.Info.SimPort = (ushort)data["SimPort"].AsInteger();
                p.Info.TeleportFlags = Utils.BytesToUInt(data["TeleportFlags"].AsBinary());

                // pass the packet onto the teleport handler
                TeleportHandler(p, simulator);

            }
        }

        /// <summary>
        /// Handler for teleport Requests
        /// </summary>
        /// <param name="packet">Incoming TeleportHandler packet</param>
        /// <param name="simulator">Simulator sending teleport information</param>
        private void TeleportHandler(Packet packet, Simulator simulator)
        {
            bool finished = false;
            TeleportFlags flags = TeleportFlags.Default;

            if (packet.Type == PacketType.TeleportStart)
            {
                TeleportStartPacket start = (TeleportStartPacket)packet;

                teleportMessage = "Teleport started";
                flags = (TeleportFlags)start.Info.TeleportFlags;
                teleportStat = TeleportStatus.Start;

                Logger.DebugLog("TeleportStart received, Flags: " + flags.ToString(), Client);
            }
            else if (packet.Type == PacketType.TeleportProgress)
            {
                TeleportProgressPacket progress = (TeleportProgressPacket)packet;

                teleportMessage = Utils.BytesToString(progress.Info.Message);
                flags = (TeleportFlags)progress.Info.TeleportFlags;
                teleportStat = TeleportStatus.Progress;

                Logger.DebugLog("TeleportProgress received, Message: " + teleportMessage + ", Flags: " + flags.ToString(), Client);
            }
            else if (packet.Type == PacketType.TeleportFailed)
            {
                TeleportFailedPacket failed = (TeleportFailedPacket)packet;

                teleportMessage = Utils.BytesToString(failed.Info.Reason);
                teleportStat = TeleportStatus.Failed;
                finished = true;

                Logger.DebugLog("TeleportFailed received, Reason: " + teleportMessage, Client);
            }
            else if (packet.Type == PacketType.TeleportFinish)
            {
                TeleportFinishPacket finish = (TeleportFinishPacket)packet;

                flags = (TeleportFlags)finish.Info.TeleportFlags;
                string seedcaps = Utils.BytesToString(finish.Info.SeedCapability);
                finished = true;

                Logger.DebugLog("TeleportFinish received, Flags: " + flags.ToString(), Client);

                // Connect to the new sim
                Simulator newSimulator = Client.Network.Connect(new IPAddress(finish.Info.SimIP),
                    finish.Info.SimPort, finish.Info.RegionHandle, true, seedcaps);

                if (newSimulator != null)
                {
                    teleportMessage = "Teleport finished";
                    teleportStat = TeleportStatus.Finished;

                    // Disconnect from the previous sim
                    Client.Network.DisconnectSim(simulator, true);

                    Logger.Log("Moved to new sim " + newSimulator.ToString(), Helpers.LogLevel.Info, Client);
                }
                else
                {
                    teleportMessage = "Failed to connect to the new sim after a teleport";
                    teleportStat = TeleportStatus.Failed;

                    // We're going to get disconnected now
                    Logger.Log(teleportMessage, Helpers.LogLevel.Error, Client);
                }
            }
            else if (packet.Type == PacketType.TeleportCancel)
            {
                //TeleportCancelPacket cancel = (TeleportCancelPacket)packet;

                teleportMessage = "Cancelled";
                teleportStat = TeleportStatus.Cancelled;
                finished = true;

                Logger.DebugLog("TeleportCancel received from " + simulator.ToString(), Client);
            }
            else if (packet.Type == PacketType.TeleportLocal)
            {
                TeleportLocalPacket local = (TeleportLocalPacket)packet;

                teleportMessage = "Teleport finished";
                flags = (TeleportFlags)local.Info.TeleportFlags;
                teleportStat = TeleportStatus.Finished;
                relativePosition = local.Info.Position;
                Movement.Camera.LookDirection(local.Info.LookAt);
                // This field is apparently not used for anything
                //local.Info.LocationID;
                finished = true;

                Logger.DebugLog("TeleportLocal received, Flags: " + flags.ToString(), Client);
            }

            if (OnTeleport != null)
            {
                try { OnTeleport(teleportMessage, teleportStat, flags); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }

            if (finished) teleportEvent.Set();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="sim"></param>
        private void AvatarAnimationHandler(Packet packet, Simulator sim)
        {
            AvatarAnimationPacket animation = (AvatarAnimationPacket)packet;

            if (animation.Sender.ID == Client.Self.AgentID)
            {
                lock (SignaledAnimations.Dictionary)
                {
                    // Reset the signaled animation list
                    SignaledAnimations.Dictionary.Clear();

                    for (int i = 0; i < animation.AnimationList.Length; i++)
                    {
                        UUID animID = animation.AnimationList[i].AnimID;
                        int sequenceID = animation.AnimationList[i].AnimSequenceID;

                        // Add this animation to the list of currently signaled animations
                        SignaledAnimations.Dictionary[animID] = sequenceID;

                        if (i < animation.AnimationSourceList.Length)
                        {
                            // FIXME: The server tells us which objects triggered our animations,
                            // we should store this info

                            //animation.AnimationSourceList[i].ObjectID
                        }

                        if (i < animation.PhysicalAvatarEventList.Length)
                        {
                            // FIXME: What is this?
                        }

                        if (Client.Settings.SEND_AGENT_UPDATES)
                        {
                            // We have to manually tell the server to stop playing some animations
                            if (animID == Animations.STANDUP ||
                                animID == Animations.PRE_JUMP ||
                                animID == Animations.LAND ||
                                animID == Animations.MEDIUM_LAND)
                            {
                                Movement.FinishAnim = true;
                                Movement.SendUpdate(true);
                                Movement.FinishAnim = false;
                            }
                        }
                    }
                }

                if (OnAnimationsChanged != null)
                {
                    try { OnAnimationsChanged(SignaledAnimations); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }
        }

        private void MeanCollisionAlertHandler(Packet packet, Simulator sim)
        {
            if (OnMeanCollision != null)
            {
                MeanCollisionAlertPacket collision = (MeanCollisionAlertPacket)packet;

                for (int i = 0; i < collision.MeanCollision.Length; i++)
                {
                    MeanCollisionAlertPacket.MeanCollisionBlock block = collision.MeanCollision[i];

                    DateTime time = Utils.UnixTimeToDateTime(block.Time);
                    MeanCollisionType type = (MeanCollisionType)block.Type;

                    try { OnMeanCollision(type, block.Perp, block.Victim, block.Mag, time); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }
        }

        private void Network_OnLoginResponse(bool loginSuccess, bool redirect, string message, string reason,
            LoginResponseData reply)
        {
            id = reply.AgentID;
            sessionID = reply.SessionID;
            secureSessionID = reply.SecureSessionID;
            firstName = reply.FirstName;
            lastName = reply.LastName;
            startLocation = reply.StartLocation;
            agentAccess = reply.AgentAccess;
            Movement.Camera.LookDirection(reply.LookAt);
            homePosition = reply.HomePosition;
            homeLookAt = reply.HomeLookAt;
        }

        private void Network_OnDisconnected(NetworkManager.DisconnectType reason, string message)
        {
            // Null out the cached fullName since it can change after logging
            // in again (with a different account name or different login
            // server but using the same GridClient object
            fullName = null;
        }

        /// <summary>
        /// Allows agent to cross over (walk, fly, vehicle) in to neighboring
        /// simulators
        /// </summary>
        private void CrossedRegionHandler(Packet packet, Simulator sim)
        {
            CrossedRegionPacket crossing = (CrossedRegionPacket)packet;
            string seedCap = Utils.BytesToString(crossing.RegionData.SeedCapability);
            IPEndPoint endPoint = new IPEndPoint(crossing.RegionData.SimIP, crossing.RegionData.SimPort);

            Logger.DebugLog("Crossed in to new region area, attempting to connect to " + endPoint.ToString(), Client);

            Simulator oldSim = Client.Network.CurrentSim;
            Simulator newSim = Client.Network.Connect(endPoint, crossing.RegionData.RegionHandle, true, seedCap);

            if (newSim != null)
            {
                Logger.Log("Finished crossing over in to region " + newSim.ToString(), Helpers.LogLevel.Info, Client);

                if (OnRegionCrossed != null)
                {
                    try { OnRegionCrossed(oldSim, newSim); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }
            else
            {
                // The old simulator will (poorly) handle our movement still, so the connection isn't
                // completely shot yet
                Logger.Log("Failed to connect to new region " + endPoint.ToString() + " after crossing over",
                    Helpers.LogLevel.Warning, Client);
            }
        }

        /// <summary>
        /// Group Chat event handler
        /// </summary>
        /// <param name="capsKey">The capability Key</param>
        /// <param name="osd"></param>
        /// <param name="simulator"></param>
        private void ChatterBoxSessionEventHandler(string capsKey, OSD osd, Simulator simulator)
        {
            OSDMap map = (OSDMap)osd;
            if (map["success"].AsBoolean() != true)
            {
                Logger.Log("Attempt to send group chat to non-existant session for group " + map["session_id"].AsString(),
                    Helpers.LogLevel.Info, Client);
            }
        }

        /// <summary>
        /// Response from request to join a group chat
        /// </summary>
        /// <param name="capsKey"></param>
        /// <param name="osd"></param>
        /// <param name="simulator"></param>
        private void ChatterBoxSessionStartReplyHandler(string capsKey, OSD osd, Simulator simulator)
        {
            OSDMap map = (OSDMap)osd;
            UUID sessionID = map["session_id"].AsUUID();
            UUID tmpSessionID = map["temp_session_id"].AsUUID();
            
            string sessionName = String.Empty;

            bool success = map["success"].AsBoolean();

            if (success)
            {
                OSDMap sessionInfo = (OSDMap)map["session_info"];
                sessionName = sessionInfo["session_name"].AsString();
                
                /* Parameters we do not currently use for anything */
                // sessionInfo["type"].AsInteger();
                // sessionInfo["voice_enabled"}.AsBoolean();
                // sessionInfo["moderated_mode"] -> ["voice"].AsBoolean()
            }

            if (OnGroupChatJoin != null)
            {
                try { OnGroupChatJoin(sessionID, sessionName, tmpSessionID, success); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary>
        /// Someone joined or left group chat
        /// </summary>
        /// <param name="capsKey"></param>
        /// <param name="osd"></param>
        /// <param name="simulator"></param>
        private void ChatterBoxSessionAgentListReplyHandler(string capsKey, OSD osd, Simulator simulator)
        {
            // parse the SD 
            OSDMap map = (OSDMap)osd;

            // verify sessions exists, if not add it
            UUID sessionID;
            if (map.ContainsKey("session_id"))
            {
                sessionID = map["session_id"].AsUUID();
                lock (GroupChatSessions)
                    if (!GroupChatSessions.ContainsKey(sessionID))
                        GroupChatSessions.Add(sessionID, new List<ChatSessionMember>());
            }
            else
            {
                return;
            }


            //string errormsg = map["error"].AsString();
            //SDMap updates = (SDMap)map["updates"];

            // Handle any agent data updates
            OSDMap agent_updates = (OSDMap)map["agent_updates"];

            foreach (KeyValuePair<string, OSD> kvp in agent_updates)
            {
                UUID agent_key = UUID.Parse(kvp.Key);
                OSDMap record = (OSDMap)kvp.Value;

                // handle joins/parts first
                if (record.ContainsKey("transition"))
                {
                    // find existing record if any
                    ChatSessionMember fndMbr;
                    lock (GroupChatSessions.Dictionary)
                    {
                        fndMbr = GroupChatSessions[sessionID].Find(delegate(ChatSessionMember member)
                           {
                               return member.AvatarKey == agent_key;
                           });
                    }

                    // handle joins
                    if (record["transition"].AsString().Equals("ENTER"))
                    {
                        if (fndMbr.AvatarKey == UUID.Zero)
                        {
                            fndMbr = new ChatSessionMember();
                            fndMbr.AvatarKey = agent_key;

                            lock (GroupChatSessions.Dictionary)
                                GroupChatSessions[sessionID].Add(fndMbr);

                            if (OnChatSessionMemberAdded != null)
                            {
                                try { OnChatSessionMemberAdded(sessionID, agent_key); }
                                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                            }
                        }

                    }
                    // handle parts
                    else if (record["transition"].AsString().Equals("LEAVE"))
                    {
                        if (fndMbr.AvatarKey != UUID.Zero)
                            lock (GroupChatSessions.Dictionary)
                                GroupChatSessions[sessionID].Remove(fndMbr);

                        if (OnChatSessionMemberLeft != null)
                        {
                            try { OnChatSessionMemberLeft(sessionID, agent_key); }
                            catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                        }

                        if (agent_key == Client.Self.AgentID)
                        {
                            try { OnGroupChatLeft(sessionID); }
                            catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                        }

                        // no need to process anything else in this record
                        continue;
                    }
                    // this should only fire if LL adds a new transition but doesn't tell anyone
                    else
                    {
                        Logger.Log("Unknown transition action " + record["transition"], Helpers.LogLevel.Warning, Client);
                    }
                }


                // Handle any updates
                
                // search for member to update
                ChatSessionMember update_member = GroupChatSessions.Dictionary[sessionID].Find(delegate(ChatSessionMember m)
                {
                    return m.AvatarKey == agent_key;
                });

                OSDMap record_info = (OSDMap)record["info"];

                lock (GroupChatSessions.Dictionary)
                {

                    if (record_info.ContainsKey("mutes"))
                    {
                        OSDMap mutes = (OSDMap)record_info["mutes"];
                        foreach (KeyValuePair<string, OSD> muteEntry in mutes)
                        {
                            if (muteEntry.Key == "text")
                            {
                                update_member.MuteText = muteEntry.Value.AsBoolean();
                            }
                            else if (muteEntry.Key == "voice")
                            {
                                update_member.MuteVoice = muteEntry.Value.AsBoolean();
                            }
                        }
                    }

                    if (record_info.ContainsKey("can_voice_chat"))
                    {
                        update_member.CanVoiceChat = record_info["can_voice_chat"].AsBoolean();
                    }

                    if (record_info.ContainsKey("is_moderator"))
                    {
                        update_member.IsModerator = record_info["is_moderator"].AsBoolean();
                    }
                }

                // replace existing member record
                lock (GroupChatSessions.Dictionary)
                {
                    int found = GroupChatSessions.Dictionary[sessionID].FindIndex(delegate(ChatSessionMember m)
                    {
                        return m.AvatarKey == agent_key;
                    });

                    if (found >= 0)
                        GroupChatSessions.Dictionary[sessionID][found] = update_member;
                }

            }

            
            //foreach (KeyValuePair<string, SD> kvp in updates)
            //{
            //    if (kvp.Value.Equals("ENTER"))
            //    {
            //        lock (GroupChatSessions.Dictionary)
            //        {
            //            if (!GroupChatSessions.Dictionary[sessionID].Contains((UUID)kvp.Key))
            //                GroupChatSessions.Dictionary[sessionID].Add((UUID)kvp.Key);
            //        }
            //    }
            //    else if (kvp.Value.Equals("LEAVE"))
            //    {
            //        lock (GroupChatSessions.Dictionary)
            //        {
            //            if (GroupChatSessions.Dictionary[sessionID].Contains((UUID)kvp.Key))
            //                GroupChatSessions.Dictionary[sessionID].Remove((UUID)kvp.Key);

            //            // we left session, remove from dictionary
            //            if (kvp.Key.Equals(Client.Self.id) && OnGroupChatLeft != null)
            //            {
            //                GroupChatSessions.Dictionary.Remove(sessionID);
            //                OnGroupChatLeft(sessionID);
            //            }
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Group Chat Request
        /// </summary>
        /// <param name="capsKey">Caps Key</param>
        /// <param name="osd">SD Map containing invitation</param>
        /// <param name="simulator">Originating Simulator</param>
        private void ChatterBoxInvitationHandler(string capsKey, OSD osd, Simulator simulator)
        {
            if (OnInstantMessage != null)
            {
                OSDMap map = (OSDMap)osd;
                OSDMap im = (OSDMap)map["instantmessage"];
                //SDMap agent = (SDMap)im["agent_params"];
                OSDMap msg = (OSDMap)im["message_params"];
                OSDMap msgdata = (OSDMap)msg["data"];

                InstantMessage message = new InstantMessage();

                message.FromAgentID = map["from_id"].AsUUID();
                message.FromAgentName = map["from_name"].AsString();
                message.ToAgentID = msg["to_id"].AsUUID();
                message.ParentEstateID = (uint)msg["parent_estate_id"].AsInteger();
                message.RegionID = msg["region_id"].AsUUID();
                message.Position = ((OSDArray)msg["position"]).AsVector3();
                message.Dialog = (InstantMessageDialog)msgdata["type"].AsInteger();
                message.GroupIM = true;
                message.IMSessionID = map["session_id"].AsUUID();
                message.Timestamp = new DateTime(msgdata["timestamp"].AsInteger());
                message.Message = msg["message"].AsString();
                message.Offline = (InstantMessageOnline)msg["offline"].AsInteger();
                message.BinaryBucket = msgdata["binary_bucket"].AsBinary();

                try { OnInstantMessage(message, simulator); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }


        /// <summary>
        /// Moderate a chat session
        /// </summary>
        /// <param name="sessionID">the <see cref="UUID"/> of the session to moderate, for group chats this will be the groups UUID</param>
        /// <param name="memberID">the <see cref="UUID"/> of the avatar to moderate</param>
        /// <param name="moderateText">true to moderate (silence user), false to allow avatar to speak</param>
        public void ModerateChatSessions(UUID sessionID, UUID memberID, bool moderateText)
        {
            if (Client.Network.CurrentSim == null || Client.Network.CurrentSim.Caps == null)
                throw new Exception("ChatSessionRequest capability is not currently available");

            Uri url = Client.Network.CurrentSim.Caps.CapabilityURI("ChatSessionRequest");

            if (url != null)
            {
                OSDMap req = new OSDMap();
                req.Add("method", OSD.FromString("mute update"));
                
                OSDMap mute_info = new OSDMap();
                mute_info.Add("text", OSD.FromBoolean(moderateText));

                OSDMap parameters = new OSDMap();
                parameters["agent_id"] = OSD.FromUUID(memberID);
                parameters["mute_info"] = mute_info;

                req["params"] = parameters;
                
                req.Add("session-id", OSD.FromUUID(sessionID));
                
                byte[] postData = StructuredData.OSDParser.SerializeLLSDXmlBytes(req);

                CapsClient request = new CapsClient(url);
                request.StartRequest(postData);
            }
            else
            {
                throw new Exception("ChatSessionRequest capability is not currently available");
            }
        }

        /// <summary>
        /// Alert Message packet handler
        /// </summary>
        /// <param name="packet">AlertMessagePacket</param>
        /// <param name="simulator">not used</param>
        private void AlertMessageHandler(Packet packet, Simulator simulator)
        {
            AlertMessagePacket alert = (AlertMessagePacket)packet;
            string message = Utils.BytesToString(alert.AlertData.Message);

            if (OnAlertMessage != null)
            {
                try { OnAlertMessage(message); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary>
        /// detects camera constraint collisions
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void CameraConstraintHandler(Packet packet, Simulator simulator)
        {
            if (OnCameraConstraint != null)
            {
                CameraConstraintPacket camera = (CameraConstraintPacket)packet;

                try { OnCameraConstraint(camera.CameraCollidePlane.Plane); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary>
        /// Packet handler for ScriptSensorReply packet
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void ScriptSensorReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnScriptSensorReply != null)
            {
                ScriptSensorReplyPacket reply = (ScriptSensorReplyPacket)packet;

                for (int i = 0; i < reply.SensedData.Length; i++)
                {
                    ScriptSensorReplyPacket.SensedDataBlock block = reply.SensedData[i];
                    ScriptSensorReplyPacket.RequesterBlock requestor = reply.Requester;

                    try
                    {
                        OnScriptSensorReply(requestor.SourceID, block.GroupID, Utils.BytesToString(block.Name),
                      block.ObjectID, block.OwnerID, block.Position, block.Range, block.Rotation, (ScriptSensorTypeFlags)block.Type, block.Velocity);
                    }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }

        }

        /// <summary>
        /// Packet handler for AvatarSitResponse packet
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void AvatarSitResponseHandler(Packet packet, Simulator simulator)
        {
            if (OnAvatarSitResponse != null)
            {
                AvatarSitResponsePacket sit = (AvatarSitResponsePacket)packet;

                try
                {
                    OnAvatarSitResponse(sit.SitObject.ID, sit.SitTransform.AutoPilot, sit.SitTransform.CameraAtOffset,
                  sit.SitTransform.CameraEyeOffset, sit.SitTransform.ForceMouselook, sit.SitTransform.SitPosition,
                  sit.SitTransform.SitRotation);
                }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        #endregion Packet Handlers
    }
}
