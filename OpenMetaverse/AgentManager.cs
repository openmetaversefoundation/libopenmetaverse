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
using System.Net;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Http;
using OpenMetaverse.Assets;
using OpenMetaverse.Packets;
using OpenMetaverse.Interfaces;
using OpenMetaverse.Messages.Linden;

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
        /// <summary>Send the message to the debug channel</summary>
        Debug = 6,
        /// <summary>Event message when an object uses llOwnerSay</summary>
        OwnerSay = 8,
        /// <summary>Special value to support llRegionSay, never sent to the client</summary>
        RegionSay = Byte.MaxValue,
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
        /// <summary>Down (C or PgDown)</summary>
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

        /// <summary>Print the struct data as a string</summary>
        /// <returns>A string containing the field name, and field value</returns>
        public override string ToString()
        {
            return Helpers.StructToString(this);
        }
    }

    #endregion Structs

    /// <summary>
    /// Manager class for our own avatar
    /// </summary>
    public partial class AgentManager
    {
        #region Delegates
        /// <summary>
        /// Called once attachment resource usage information has been collected
        /// </summary>
        /// <param name="success">Indicates if operation was successfull</param>
        /// <param name="info">Attachment resource usage information</param>
        public delegate void AttachmentResourcesCallback(bool success, AttachmentResourcesMessage info);
        #endregion Delegates

        #region Event Delegates

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ChatEventArgs> m_Chat;

        /// <summary>Raises the ChatFromSimulator event</summary>
        /// <param name="e">A ChatEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnChat(ChatEventArgs e)
        {
            EventHandler<ChatEventArgs> handler = m_Chat;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ChatLock = new object();

        /// <summary>Raised when a scripted object or agent within range sends a public message</summary>
        public event EventHandler<ChatEventArgs> ChatFromSimulator
        {
            add { lock (m_ChatLock) { m_Chat += value; } }
            remove { lock (m_ChatLock) { m_Chat -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ScriptDialogEventArgs> m_ScriptDialog;

        /// <summary>Raises the ScriptDialog event</summary>
        /// <param name="e">A SctriptDialogEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnScriptDialog(ScriptDialogEventArgs e)
        {
            EventHandler<ScriptDialogEventArgs> handler = m_ScriptDialog;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ScriptDialogLock = new object();
        /// <summary>Raised when a scripted object sends a dialog box containing possible
        /// options an agent can respond to</summary>
        public event EventHandler<ScriptDialogEventArgs> ScriptDialog
        {
            add { lock (m_ScriptDialogLock) { m_ScriptDialog += value; } }
            remove { lock (m_ScriptDialogLock) { m_ScriptDialog -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ScriptQuestionEventArgs> m_ScriptQuestion;

        /// <summary>Raises the ScriptQuestion event</summary>
        /// <param name="e">A ScriptQuestionEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnScriptQuestion(ScriptQuestionEventArgs e)
        {
            EventHandler<ScriptQuestionEventArgs> handler = m_ScriptQuestion;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ScriptQuestionLock = new object();
        /// <summary>Raised when an object requests a change in the permissions an agent has permitted</summary>
        public event EventHandler<ScriptQuestionEventArgs> ScriptQuestion
        {
            add { lock (m_ScriptQuestionLock) { m_ScriptQuestion += value; } }
            remove { lock (m_ScriptQuestionLock) { m_ScriptQuestion -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<LoadUrlEventArgs> m_LoadURL;

        /// <summary>Raises the LoadURL event</summary>
        /// <param name="e">A LoadUrlEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnLoadURL(LoadUrlEventArgs e)
        {
            EventHandler<LoadUrlEventArgs> handler = m_LoadURL;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_LoadUrlLock = new object();
        /// <summary>Raised when a script requests an agent open the specified URL</summary>
        public event EventHandler<LoadUrlEventArgs> LoadURL
        {
            add { lock (m_LoadUrlLock) { m_LoadURL += value; } }
            remove { lock (m_LoadUrlLock) { m_LoadURL -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<BalanceEventArgs> m_Balance;

        /// <summary>Raises the MoneyBalance event</summary>
        /// <param name="e">A BalanceEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnBalance(BalanceEventArgs e)
        {
            EventHandler<BalanceEventArgs> handler = m_Balance;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_BalanceLock = new object();

        /// <summary>Raised when an agents currency balance is updated</summary>
        public event EventHandler<BalanceEventArgs> MoneyBalance
        {
            add { lock (m_BalanceLock) { m_Balance += value; } }
            remove { lock (m_BalanceLock) { m_Balance -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<MoneyBalanceReplyEventArgs> m_MoneyBalance;

        /// <summary>Raises the MoneyBalanceReply event</summary>
        /// <param name="e">A MoneyBalanceReplyEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnMoneyBalanceReply(MoneyBalanceReplyEventArgs e)
        {
            EventHandler<MoneyBalanceReplyEventArgs> handler = m_MoneyBalance;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_MoneyBalanceReplyLock = new object();

        /// <summary>Raised when a transaction occurs involving currency such as a land purchase</summary>
        public event EventHandler<MoneyBalanceReplyEventArgs> MoneyBalanceReply
        {
            add { lock (m_MoneyBalanceReplyLock) { m_MoneyBalance += value; } }
            remove { lock (m_MoneyBalanceReplyLock) { m_MoneyBalance -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<InstantMessageEventArgs> m_InstantMessage;

        /// <summary>Raises the IM event</summary>
        /// <param name="e">A InstantMessageEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnInstantMessage(InstantMessageEventArgs e)
        {
            EventHandler<InstantMessageEventArgs> handler = m_InstantMessage;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_InstantMessageLock = new object();
        /// <summary>Raised when an ImprovedInstantMessage packet is recieved from the simulator, this is used for everything from
        /// private messaging to friendship offers. The Dialog field defines what type of message has arrived</summary>
        public event EventHandler<InstantMessageEventArgs> IM
        {
            add { lock (m_InstantMessageLock) { m_InstantMessage += value; } }
            remove { lock (m_InstantMessageLock) { m_InstantMessage -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<TeleportEventArgs> m_Teleport;

        /// <summary>Raises the TeleportProgress event</summary>
        /// <param name="e">A TeleportEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnTeleport(TeleportEventArgs e)
        {
            EventHandler<TeleportEventArgs> handler = m_Teleport;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_TeleportLock = new object();
        /// <summary>Raised when an agent has requested a teleport to another location, or when responding to a lure. Raised multiple times
        /// for each teleport indicating the progress of the request</summary>
        public event EventHandler<TeleportEventArgs> TeleportProgress
        {
            add { lock (m_TeleportLock) { m_Teleport += value; } }
            remove { lock (m_TeleportLock) { m_Teleport += value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<AgentDataReplyEventArgs> m_AgentData;

        /// <summary>Raises the AgentDataReply event</summary>
        /// <param name="e">A AgentDataReplyEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnAgentData(AgentDataReplyEventArgs e)
        {
            EventHandler<AgentDataReplyEventArgs> handler = m_AgentData;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_AgentDataLock = new object();

        /// <summary>Raised when a simulator sends agent specific information for our avatar.</summary>
        public event EventHandler<AgentDataReplyEventArgs> AgentDataReply
        {
            add { lock (m_AgentDataLock) { m_AgentData += value; } }
            remove { lock (m_AgentDataLock) { m_AgentData -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<AnimationsChangedEventArgs> m_AnimationsChanged;

        /// <summary>Raises the AnimationsChanged event</summary>
        /// <param name="e">A AnimationsChangedEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnAnimationsChanged(AnimationsChangedEventArgs e)
        {
            EventHandler<AnimationsChangedEventArgs> handler = m_AnimationsChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_AnimationsChangedLock = new object();

        /// <summary>Raised when our agents animation playlist changes</summary>
        public event EventHandler<AnimationsChangedEventArgs> AnimationsChanged
        {
            add { lock (m_AnimationsChangedLock) { m_AnimationsChanged += value; } }
            remove { lock (m_AnimationsChangedLock) { m_AnimationsChanged -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<MeanCollisionEventArgs> m_MeanCollision;

        /// <summary>Raises the MeanCollision event</summary>
        /// <param name="e">A MeanCollisionEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnMeanCollision(MeanCollisionEventArgs e)
        {
            EventHandler<MeanCollisionEventArgs> handler = m_MeanCollision;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_MeanCollisionLock = new object();

        /// <summary>Raised when an object or avatar forcefully collides with our agent</summary>
        public event EventHandler<MeanCollisionEventArgs> MeanCollision
        {
            add { lock (m_MeanCollisionLock) { m_MeanCollision += value; } }
            remove { lock (m_MeanCollisionLock) { m_MeanCollision -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<RegionCrossedEventArgs> m_RegionCrossed;

        /// <summary>Raises the RegionCrossed event</summary>
        /// <param name="e">A RegionCrossedEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnRegionCrossed(RegionCrossedEventArgs e)
        {
            EventHandler<RegionCrossedEventArgs> handler = m_RegionCrossed;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_RegionCrossedLock = new object();

        /// <summary>Raised when our agent crosses a region border into another region</summary>
        public event EventHandler<RegionCrossedEventArgs> RegionCrossed
        {
            add { lock (m_RegionCrossedLock) { m_RegionCrossed += value; } }
            remove { lock (m_RegionCrossedLock) { m_RegionCrossed -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<GroupChatJoinedEventArgs> m_GroupChatJoined;

        /// <summary>Raises the GroupChatJoined event</summary>
        /// <param name="e">A GroupChatJoinedEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnGroupChatJoined(GroupChatJoinedEventArgs e)
        {
            EventHandler<GroupChatJoinedEventArgs> handler = m_GroupChatJoined;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_GroupChatJoinedLock = new object();

        /// <summary>Raised when our agent succeeds or fails to join a group chat session</summary>
        public event EventHandler<GroupChatJoinedEventArgs> GroupChatJoined
        {
            add { lock (m_GroupChatJoinedLock) { m_GroupChatJoined += value; } }
            remove { lock (m_GroupChatJoinedLock) { m_GroupChatJoined -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<AlertMessageEventArgs> m_AlertMessage;

        /// <summary>Raises the AlertMessage event</summary>
        /// <param name="e">A AlertMessageEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnAlertMessage(AlertMessageEventArgs e)
        {
            EventHandler<AlertMessageEventArgs> handler = m_AlertMessage;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_AlertMessageLock = new object();

        /// <summary>Raised when a simulator sends an urgent message usually indication the recent failure of
        /// another action we have attempted to take such as an attempt to enter a parcel where we are denied access</summary>
        public event EventHandler<AlertMessageEventArgs> AlertMessage
        {
            add { lock (m_AlertMessageLock) { m_AlertMessage += value; } }
            remove { lock (m_AlertMessageLock) { m_AlertMessage -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ScriptControlEventArgs> m_ScriptControl;

        /// <summary>Raises the ScriptControlChange event</summary>
        /// <param name="e">A ScriptControlEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnScriptControlChange(ScriptControlEventArgs e)
        {
            EventHandler<ScriptControlEventArgs> handler = m_ScriptControl;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ScriptControlLock = new object();

        /// <summary>Raised when a script attempts to take or release specified controls for our agent</summary>
        public event EventHandler<ScriptControlEventArgs> ScriptControlChange
        {
            add { lock (m_ScriptControlLock) { m_ScriptControl += value; } }
            remove { lock (m_ScriptControlLock) { m_ScriptControl -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<CameraConstraintEventArgs> m_CameraConstraint;

        /// <summary>Raises the CameraConstraint event</summary>
        /// <param name="e">A CameraConstraintEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnCameraConstraint(CameraConstraintEventArgs e)
        {
            EventHandler<CameraConstraintEventArgs> handler = m_CameraConstraint;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_CameraConstraintLock = new object();

        /// <summary>Raised when the simulator detects our agent is trying to view something
        /// beyond its limits</summary>
        public event EventHandler<CameraConstraintEventArgs> CameraConstraint
        {
            add { lock (m_CameraConstraintLock) { m_CameraConstraint += value; } }
            remove { lock (m_CameraConstraintLock) { m_CameraConstraint -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ScriptSensorReplyEventArgs> m_ScriptSensorReply;

        /// <summary>Raises the ScriptSensorReply event</summary>
        /// <param name="e">A ScriptSensorReplyEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnScriptSensorReply(ScriptSensorReplyEventArgs e)
        {
            EventHandler<ScriptSensorReplyEventArgs> handler = m_ScriptSensorReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ScriptSensorReplyLock = new object();

        /// <summary>Raised when a script sensor reply is received from a simulator</summary>
        public event EventHandler<ScriptSensorReplyEventArgs> ScriptSensorReply
        {
            add { lock (m_ScriptSensorReplyLock) { m_ScriptSensorReply += value; } }
            remove { lock (m_ScriptSensorReplyLock) { m_ScriptSensorReply -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<AvatarSitResponseEventArgs> m_AvatarSitResponse;

        /// <summary>Raises the AvatarSitResponse event</summary>
        /// <param name="e">A AvatarSitResponseEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnAvatarSitResponse(AvatarSitResponseEventArgs e)
        {
            EventHandler<AvatarSitResponseEventArgs> handler = m_AvatarSitResponse;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_AvatarSitResponseLock = new object();

        /// <summary>Raised in response to a <see cref="RequestSit"/> request</summary>
        public event EventHandler<AvatarSitResponseEventArgs> AvatarSitResponse
        {
            add { lock (m_AvatarSitResponseLock) { m_AvatarSitResponse += value; } }
            remove { lock (m_AvatarSitResponseLock) { m_AvatarSitResponse -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ChatSessionMemberAddedEventArgs> m_ChatSessionMemberAdded;

        /// <summary>Raises the ChatSessionMemberAdded event</summary>
        /// <param name="e">A ChatSessionMemberAddedEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnChatSessionMemberAdded(ChatSessionMemberAddedEventArgs e)
        {
            EventHandler<ChatSessionMemberAddedEventArgs> handler = m_ChatSessionMemberAdded;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ChatSessionMemberAddedLock = new object();

        /// <summary>Raised when an avatar enters a group chat session we are participating in</summary>
        public event EventHandler<ChatSessionMemberAddedEventArgs> ChatSessionMemberAdded
        {
            add { lock (m_ChatSessionMemberAddedLock) { m_ChatSessionMemberAdded += value; } }
            remove { lock (m_ChatSessionMemberAddedLock) { m_ChatSessionMemberAdded -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ChatSessionMemberLeftEventArgs> m_ChatSessionMemberLeft;

        /// <summary>Raises the ChatSessionMemberLeft event</summary>
        /// <param name="e">A ChatSessionMemberLeftEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnChatSessionMemberLeft(ChatSessionMemberLeftEventArgs e)
        {
            EventHandler<ChatSessionMemberLeftEventArgs> handler = m_ChatSessionMemberLeft;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ChatSessionMemberLeftLock = new object();

        /// <summary>Raised when an agent exits a group chat session we are participating in</summary>
        public event EventHandler<ChatSessionMemberLeftEventArgs> ChatSessionMemberLeft
        {
            add { lock (m_ChatSessionMemberLeftLock) { m_ChatSessionMemberLeft += value; } }
            remove { lock (m_ChatSessionMemberLeftLock) { m_ChatSessionMemberLeft -= value; } }
        }
        #endregion Callbacks

        #region Events

        #endregion Events

        /// <summary>Reference to the GridClient instance</summary>
        private readonly GridClient Client;
        /// <summary>Used for movement and camera tracking</summary>
        public readonly AgentMovement Movement;
        /// <summary>Currently playing animations for the agent. Can be used to
        /// check the current movement status such as walking, hovering, aiming,
        /// etc. by checking against system animations found in the Animations class</summary>
        public InternalDictionary<UUID, int> SignaledAnimations = new InternalDictionary<UUID, int>();
        /// <summary>Dictionary containing current Group Chat sessions and members</summary>
        public InternalDictionary<UUID, List<ChatSessionMember>> GroupChatSessions = new InternalDictionary<UUID, List<ChatSessionMember>>();

        #region Properties

        /// <summary>Your (client) avatars <see cref="UUID"/></summary>
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
        public Vector3 RelativePosition { get { return relativePosition; } set { relativePosition = value; } }
        /// <summary>Current rotation of the agent as a relative rotation from
        /// the simulator, or the parent object if we are sitting on something</summary>
        public Quaternion RelativeRotation { get { return relativeRotation; } set { relativeRotation = value; } }
        /// <summary>Current position of the agent in the simulator</summary>
        public Vector3 SimPosition
        {
            get
            {
                // simple case, agent not seated
                if (sittingOn == 0)
                {
                    return relativePosition;
                }

                // a bit more complicatated, agent sitting on a prim
                Primitive p = null;
                Vector3 fullPosition = relativePosition;
                Client.Network.CurrentSim.ObjectsPrimitives.TryGetValue(sittingOn, out p);

                // go up the hiearchy trying to find the root prim
                while (p != null && p.ParentID != 0)
                {
                    Avatar av;
                    if (Client.Network.CurrentSim.ObjectsAvatars.TryGetValue(p.ParentID, out av))
                    {
                        p = av;
                        fullPosition += p.Position;
                    }
                    else
                    {
                        if (Client.Network.CurrentSim.ObjectsPrimitives.TryGetValue(p.ParentID, out p))
                        {
                            fullPosition += p.Position;
                        }
                    }
                }

                if (p != null) // we found the root prim
                {
                    return fullPosition;
                }

                // Didn't find the seat's root prim, try returning coarse loaction
                if (Client.Network.CurrentSim.avatarPositions.TryGetValue(AgentID, out fullPosition))
                {
                    return fullPosition;
                }

                Logger.Log("Failed to determine agents sim position", Helpers.LogLevel.Warning, Client);
                return relativePosition;
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
        private Dictionary<UUID, AssetGesture> gestureCache = new Dictionary<UUID, AssetGesture>();
        #endregion Private Members

        /// <summary>
        /// Constructor, setup callbacks for packets related to our avatar
        /// </summary>
        /// <param name="client">A reference to the <seealso cref="T:OpenMetaverse.GridClient"/> Class</param>
        public AgentManager(GridClient client)
        {
            Client = client;

            Movement = new AgentMovement(Client);

            Client.Network.Disconnected += Network_OnDisconnected;

            // Teleport callbacks            
            Client.Network.RegisterCallback(PacketType.TeleportStart, TeleportHandler);
            Client.Network.RegisterCallback(PacketType.TeleportProgress, TeleportHandler);
            Client.Network.RegisterCallback(PacketType.TeleportFailed, TeleportHandler);
            Client.Network.RegisterCallback(PacketType.TeleportCancel, TeleportHandler);
            Client.Network.RegisterCallback(PacketType.TeleportLocal, TeleportHandler);
            // these come in via the EventQueue
            Client.Network.RegisterEventCallback("TeleportFailed", new Caps.EventQueueCallback(TeleportFailedEventHandler));
            Client.Network.RegisterEventCallback("TeleportFinish", new Caps.EventQueueCallback(TeleportFinishEventHandler));

            // Instant message callback
            Client.Network.RegisterCallback(PacketType.ImprovedInstantMessage, InstantMessageHandler);
            // Chat callback
            Client.Network.RegisterCallback(PacketType.ChatFromSimulator, ChatHandler);
            // Script dialog callback
            Client.Network.RegisterCallback(PacketType.ScriptDialog, ScriptDialogHandler);
            // Script question callback
            Client.Network.RegisterCallback(PacketType.ScriptQuestion, ScriptQuestionHandler);
            // Script URL callback
            Client.Network.RegisterCallback(PacketType.LoadURL, LoadURLHandler);
            // Movement complete callback
            Client.Network.RegisterCallback(PacketType.AgentMovementComplete, MovementCompleteHandler);
            // Health callback
            Client.Network.RegisterCallback(PacketType.HealthMessage, HealthHandler);
            // Money callback
            Client.Network.RegisterCallback(PacketType.MoneyBalanceReply, MoneyBalanceReplyHandler);
            //Agent update callback
            Client.Network.RegisterCallback(PacketType.AgentDataUpdate, AgentDataUpdateHandler);
            // Animation callback
            Client.Network.RegisterCallback(PacketType.AvatarAnimation, AvatarAnimationHandler);
            // Object colliding into our agent callback
            Client.Network.RegisterCallback(PacketType.MeanCollisionAlert, MeanCollisionAlertHandler);
            // Region Crossing
            Client.Network.RegisterCallback(PacketType.CrossedRegion, CrossedRegionHandler);
            Client.Network.RegisterEventCallback("CrossedRegion", new Caps.EventQueueCallback(CrossedRegionEventHandler));
            // CAPS callbacks
            Client.Network.RegisterEventCallback("EstablishAgentCommunication", new Caps.EventQueueCallback(EstablishAgentCommunicationEventHandler));
            // Incoming Group Chat
            Client.Network.RegisterEventCallback("ChatterBoxInvitation", new Caps.EventQueueCallback(ChatterBoxInvitationEventHandler));
            // Outgoing Group Chat Reply
            Client.Network.RegisterEventCallback("ChatterBoxSessionEventReply", new Caps.EventQueueCallback(ChatterBoxSessionEventReplyEventHandler));
            Client.Network.RegisterEventCallback("ChatterBoxSessionStartReply", new Caps.EventQueueCallback(ChatterBoxSessionStartReplyEventHandler));
            Client.Network.RegisterEventCallback("ChatterBoxSessionAgentListUpdates", new Caps.EventQueueCallback(ChatterBoxSessionAgentListUpdatesEventHandler));
            // Login
            Client.Network.RegisterLoginResponseCallback(new NetworkManager.LoginResponseCallback(Network_OnLoginResponse));
            // Alert Messages
            Client.Network.RegisterCallback(PacketType.AlertMessage, AlertMessageHandler);
            // script control change messages, ie: when an in-world LSL script wants to take control of your agent.
            Client.Network.RegisterCallback(PacketType.ScriptControlChange, ScriptControlChangeHandler);
            // Camera Constraint (probably needs to move to AgentManagerCamera TODO:
            Client.Network.RegisterCallback(PacketType.CameraConstraint, CameraConstraintHandler);
            Client.Network.RegisterCallback(PacketType.ScriptSensorReply, ScriptSensorReplyHandler);
            Client.Network.RegisterCallback(PacketType.AvatarSitResponse, AvatarSitResponseHandler);
        }

        #region Chat and instant messages

        /// <summary>
        /// Send a text message from the Agent to the Simulator
        /// </summary>
        /// <param name="message">A <see cref="string"/> containing the message</param>
        /// <param name="channel">The channel to send the message on, 0 is the public channel. Channels above 0
        /// can be used however only scripts listening on the specified channel will see the message</param>
        /// <param name="type">Denotes the type of message being sent, shout, whisper, etc.</param>
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

        /// <summary>
        /// Request any instant messages sent while the client was offline to be resent.
        /// </summary>
        public void RetrieveInstantMessages()
        {
            RetrieveInstantMessagesPacket p = new RetrieveInstantMessagesPacket();
            p.AgentData.AgentID = Client.Self.AgentID;
            p.AgentData.SessionID = Client.Self.SessionID;
            Client.Network.SendPacket(p);
        }

        /// <summary>
        /// Send an Instant Message to another Avatar
        /// </summary>
        /// <param name="target">The recipients <see cref="UUID"/></param>
        /// <param name="message">A <see cref="string"/> containing the message to send</param>
        public void InstantMessage(UUID target, string message)
        {
            InstantMessage(Name, target, message, AgentID.Equals(target) ? AgentID : target ^ AgentID,
                InstantMessageDialog.MessageFromAgent, InstantMessageOnline.Offline, this.SimPosition,
                UUID.Zero, Utils.EmptyBytes);
        }

        /// <summary>
        /// Send an Instant Message to an existing group chat or conference chat
        /// </summary>
        /// <param name="target">The recipients <see cref="UUID"/></param>
        /// <param name="message">A <see cref="string"/> containing the message to send</param>
        /// <param name="imSessionID">IM session ID (to differentiate between IM windows)</param>
        public void InstantMessage(UUID target, string message, UUID imSessionID)
        {
            InstantMessage(Name, target, message, imSessionID,
                InstantMessageDialog.MessageFromAgent, InstantMessageOnline.Offline, this.SimPosition,
                UUID.Zero, Utils.EmptyBytes);
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
                binaryBucket = Utils.EmptyBytes;
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
                    im.MessageBlock.BinaryBucket = Utils.EmptyBytes;

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
        /// Send an Instant Message to a group the agent is a member of
        /// </summary>
        /// <param name="fromName">The name this IM will show up as being from</param>
        /// <param name="groupID"><seealso cref="UUID"/> of the group to send message to</param>
        /// <param name="message">Text message being sent</param>
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
            im.MessageBlock.Message = Utils.EmptyBytes;
            im.MessageBlock.ParentEstateID = 0;
            im.MessageBlock.Offline = 0;
            im.MessageBlock.ID = groupID;
            im.MessageBlock.ToAgentID = groupID;
            im.MessageBlock.BinaryBucket = Utils.EmptyBytes;
            im.MessageBlock.Position = Client.Self.SimPosition;
            im.MessageBlock.RegionID = UUID.Zero;

            Client.Network.SendPacket(im);
        }

        /// <summary>
        /// Exit a group chat session. This will stop further Group chat messages
        /// from being sent until session is rejoined.
        /// </summary>
        /// <param name="groupID"><seealso cref="UUID"/> of Group chat session to leave</param>
        public void RequestLeaveGroupChat(UUID groupID)
        {
            ImprovedInstantMessagePacket im = new ImprovedInstantMessagePacket();

            im.AgentData.AgentID = Client.Self.AgentID;
            im.AgentData.SessionID = Client.Self.SessionID;
            im.MessageBlock.Dialog = (byte)InstantMessageDialog.SessionDrop;
            im.MessageBlock.FromAgentName = Utils.StringToBytes(Client.Self.Name);
            im.MessageBlock.FromGroup = false;
            im.MessageBlock.Message = Utils.EmptyBytes;
            im.MessageBlock.Offline = 0;
            im.MessageBlock.ID = groupID;
            im.MessageBlock.ToAgentID = groupID;
            im.MessageBlock.BinaryBucket = Utils.EmptyBytes;
            im.MessageBlock.Position = Vector3.Zero;
            im.MessageBlock.RegionID = UUID.Zero;

            Client.Network.SendPacket(im);

            lock (GroupChatSessions.Dictionary)
                if (GroupChatSessions.ContainsKey(groupID))
                    GroupChatSessions.Remove(groupID);
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

        /// <summary>
        /// Accept invite for to a chatterbox session
        /// </summary>
        /// <param name="session_id"><seealso cref="UUID"/> of session to accept invite to</param>
        public void ChatterBoxAcceptInvite(UUID session_id)
        {
            if (Client.Network.CurrentSim == null || Client.Network.CurrentSim.Caps == null)
                throw new Exception("ChatSessionRequest capability is not currently available");

            Uri url = Client.Network.CurrentSim.Caps.CapabilityURI("ChatSessionRequest");

            if (url != null)
            {
                ChatSessionAcceptInvitation acceptInvite = new ChatSessionAcceptInvitation();
                acceptInvite.SessionID = session_id;

                CapsClient request = new CapsClient(url);
                request.BeginGetResponse(acceptInvite.Serialize(), OSDFormat.Xml, Client.Settings.CAPS_TIMEOUT);

                lock (GroupChatSessions.Dictionary)
                    if (!GroupChatSessions.ContainsKey(session_id))
                        GroupChatSessions.Add(session_id, new List<ChatSessionMember>());
            }
            else
            {
                throw new Exception("ChatSessionRequest capability is not currently available");
            }

        }

        /// <summary>
        /// Start a friends conference
        /// </summary>
        /// <param name="participants"><seealso cref="UUID"/> List of UUIDs to start a conference with</param>
        /// <param name="tmp_session_id">the temportary session ID returned in the <see cref="OnJoinedGroupChat"/> callback></param>
        public void StartIMConference(List<UUID> participants, UUID tmp_session_id)
        {
            if (Client.Network.CurrentSim == null || Client.Network.CurrentSim.Caps == null)
                throw new Exception("ChatSessionRequest capability is not currently available");

            Uri url = Client.Network.CurrentSim.Caps.CapabilityURI("ChatSessionRequest");

            if (url != null)
            {
                ChatSessionRequestStartConference startConference = new ChatSessionRequestStartConference();

                startConference.AgentsBlock = new UUID[participants.Count];
                for (int i = 0; i < participants.Count; i++)
                    startConference.AgentsBlock[i] = participants[i];

                startConference.SessionID = tmp_session_id;

                CapsClient request = new CapsClient(url);
                request.BeginGetResponse(startConference.Serialize(), OSDFormat.Xml, Client.Settings.CAPS_TIMEOUT);
            }
            else
            {
                throw new Exception("ChatSessionRequest capability is not currently available");
            }
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
                    duration = 2.0f;
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
            Buffer.BlockCopy(sourceAvatar.GetBytes(), 0, typeData, 0, 16);
            Buffer.BlockCopy(targetObject.GetBytes(), 0, typeData, 16, 16);
            Buffer.BlockCopy(globalOffset.GetBytes(), 0, typeData, 32, 24);
            typeData[56] = (byte)type;

            effect.Effect[0].TypeData = typeData;

            Client.Network.SendPacket(effect);
        }

        /// <summary>
        /// Create a particle beam between an avatar and an primitive 
        /// </summary>
        /// <param name="sourceAvatar">The ID of source avatar</param>
        /// <param name="targetObject">The ID of the target primitive</param>
        /// <param name="globalOffset">global offset</param>
        /// <param name="color">A <see cref="Color4"/> object containing the combined red, green, blue and alpha 
        /// color values of particle beam</param>
        /// <param name="duration">a float representing the duration the parcicle beam will last</param>
        /// <param name="effectID">A Unique ID for the beam</param>
        /// <seealso cref="ViewerEffectPacket"/>
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

        /// <summary>
        /// Create a particle swirl around a target position using a <seealso cref="ViewerEffectPacket"/> packet
        /// </summary>
        /// <param name="globalOffset">global offset</param>
        /// <param name="color">A <see cref="Color4"/> object containing the combined red, green, blue and alpha 
        /// color values of particle beam</param>
        /// <param name="duration">a float representing the duration the parcicle beam will last</param>
        /// <param name="effectID">A Unique ID for the beam</param>
        public void SphereEffect(Vector3d globalOffset, Color4 color, float duration, UUID effectID)
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
            effect.Effect[0].Type = (byte)EffectType.Sphere;

            byte[] typeData = new byte[56];
            Buffer.BlockCopy(UUID.Zero.GetBytes(), 0, typeData, 0, 16);
            Buffer.BlockCopy(UUID.Zero.GetBytes(), 0, typeData, 16, 16);
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
        /// <seealso cref="Simulator.ObjectsPrimitives"/>
        public void Grab(uint objectLocalID)
        {
            Grab(objectLocalID, Vector3.Zero, Vector3.Zero, Vector3.Zero, 0, Vector3.Zero, Vector3.Zero, Vector3.Zero);
        }

        /// <summary>
        /// Overload: Grab a simulated object
        /// </summary>
        /// <param name="objectLocalID">an unsigned integer of the objects ID within the simulator</param>
        /// <param name="grabOffset"></param>
        /// <param name="uvCoord">The texture coordinates to grab</param>
        /// <param name="stCoord">The surface coordinates to grab</param>
        /// <param name="faceIndex">The face of the position to grab</param>
        /// <param name="position">The region coordinates of the position to grab</param>
        /// <param name="normal">The surface normal of the position to grab (A normal is a vector perpindicular to the surface)</param>
        /// <param name="binormal">The surface binormal of the position to grab (A binormal is a vector tangen to the surface
        /// pointing along the U direction of the tangent space</param>
        public void Grab(uint objectLocalID, Vector3 grabOffset, Vector3 uvCoord, Vector3 stCoord, int faceIndex, Vector3 position,
            Vector3 normal, Vector3 binormal)
        {
            ObjectGrabPacket grab = new ObjectGrabPacket();

            grab.AgentData.AgentID = Client.Self.AgentID;
            grab.AgentData.SessionID = Client.Self.SessionID;

            grab.ObjectData.LocalID = objectLocalID;
            grab.ObjectData.GrabOffset = grabOffset;

            grab.SurfaceInfo = new ObjectGrabPacket.SurfaceInfoBlock[1];
            grab.SurfaceInfo[0] = new ObjectGrabPacket.SurfaceInfoBlock();
            grab.SurfaceInfo[0].UVCoord = uvCoord;
            grab.SurfaceInfo[0].STCoord = stCoord;
            grab.SurfaceInfo[0].FaceIndex = faceIndex;
            grab.SurfaceInfo[0].Position = position;
            grab.SurfaceInfo[0].Normal = normal;
            grab.SurfaceInfo[0].Binormal = binormal;

            Client.Network.SendPacket(grab);
        }

        /// <summary>
        /// Drag an object
        /// </summary>
        /// <param name="objectID"><seealso cref="UUID"/> of the object to drag</param>
        /// <param name="grabPosition">Drag target in region coordinates</param>
        public void GrabUpdate(UUID objectID, Vector3 grabPosition)
        {
            GrabUpdate(objectID, grabPosition, Vector3.Zero, Vector3.Zero, Vector3.Zero, 0, Vector3.Zero, Vector3.Zero, Vector3.Zero);
        }

        /// <summary>
        /// Overload: Drag an object
        /// </summary>
        /// <param name="objectID"><seealso cref="UUID"/> of the object to drag</param>
        /// <param name="grabPosition">Drag target in region coordinates</param>
        /// <param name="grabOffset"></param>
        /// <param name="uvCoord">The texture coordinates to grab</param>
        /// <param name="stCoord">The surface coordinates to grab</param>
        /// <param name="faceIndex">The face of the position to grab</param>
        /// <param name="position">The region coordinates of the position to grab</param>
        /// <param name="normal">The surface normal of the position to grab (A normal is a vector perpindicular to the surface)</param>
        /// <param name="binormal">The surface binormal of the position to grab (A binormal is a vector tangen to the surface
        /// pointing along the U direction of the tangent space</param>
        public void GrabUpdate(UUID objectID, Vector3 grabPosition, Vector3 grabOffset, Vector3 uvCoord, Vector3 stCoord, int faceIndex, Vector3 position,
            Vector3 normal, Vector3 binormal)
        {
            ObjectGrabUpdatePacket grab = new ObjectGrabUpdatePacket();
            grab.AgentData.AgentID = Client.Self.AgentID;
            grab.AgentData.SessionID = Client.Self.SessionID;

            grab.ObjectData.ObjectID = objectID;
            grab.ObjectData.GrabOffsetInitial = grabOffset;
            grab.ObjectData.GrabPosition = grabPosition;
            grab.ObjectData.TimeSinceLast = 0;

            grab.SurfaceInfo = new ObjectGrabUpdatePacket.SurfaceInfoBlock[1];
            grab.SurfaceInfo[0] = new ObjectGrabUpdatePacket.SurfaceInfoBlock();
            grab.SurfaceInfo[0].UVCoord = uvCoord;
            grab.SurfaceInfo[0].STCoord = stCoord;
            grab.SurfaceInfo[0].FaceIndex = faceIndex;
            grab.SurfaceInfo[0].Position = position;
            grab.SurfaceInfo[0].Normal = normal;
            grab.SurfaceInfo[0].Binormal = binormal;

            Client.Network.SendPacket(grab);
        }

        /// <summary>
        /// Release a grabbed object
        /// </summary>
        /// <param name="objectLocalID">The Objects Simulator Local ID</param>
        /// <seealso cref="Simulator.ObjectsPrimitives"/>
        /// <seealso cref="Grab"/>
        /// <seealso cref="GrabUpdate"/>
        public void DeGrab(uint objectLocalID)
        {
            DeGrab(objectLocalID, Vector3.Zero, Vector3.Zero, 0, Vector3.Zero, Vector3.Zero, Vector3.Zero);
        }

        /// <summary>
        /// Release a grabbed object
        /// </summary>
        /// <param name="objectLocalID">The Objects Simulator Local ID</param>
        /// <param name="uvCoord">The texture coordinates to grab</param>
        /// <param name="stCoord">The surface coordinates to grab</param>
        /// <param name="faceIndex">The face of the position to grab</param>
        /// <param name="position">The region coordinates of the position to grab</param>
        /// <param name="normal">The surface normal of the position to grab (A normal is a vector perpindicular to the surface)</param>
        /// <param name="binormal">The surface binormal of the position to grab (A binormal is a vector tangen to the surface
        /// pointing along the U direction of the tangent space</param>
        public void DeGrab(uint objectLocalID, Vector3 uvCoord, Vector3 stCoord, int faceIndex, Vector3 position,
            Vector3 normal, Vector3 binormal)
        {
            ObjectDeGrabPacket degrab = new ObjectDeGrabPacket();
            degrab.AgentData.AgentID = Client.Self.AgentID;
            degrab.AgentData.SessionID = Client.Self.SessionID;

            degrab.ObjectData.LocalID = objectLocalID;

            degrab.SurfaceInfo = new ObjectDeGrabPacket.SurfaceInfoBlock[1];
            degrab.SurfaceInfo[0] = new ObjectDeGrabPacket.SurfaceInfoBlock();
            degrab.SurfaceInfo[0].UVCoord = uvCoord;
            degrab.SurfaceInfo[0].STCoord = stCoord;
            degrab.SurfaceInfo[0].FaceIndex = faceIndex;
            degrab.SurfaceInfo[0].Position = position;
            degrab.SurfaceInfo[0].Normal = normal;
            degrab.SurfaceInfo[0].Binormal = binormal;

            Client.Network.SendPacket(degrab);
        }

        /// <summary>
        /// Touches an object
        /// </summary>
        /// <param name="objectLocalID">an unsigned integer of the objects ID within the simulator</param>
        /// <seealso cref="Simulator.ObjectsPrimitives"/>
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
        /// Give Money to destination Object or Avatar
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

        #region Gestures
        /// <summary>
        /// Plays a gesture
        /// </summary>
        /// <param name="gestureID">Asset <seealso cref="UUID"/> of the gesture</param>
        public void PlayGesture(UUID gestureID)
        {
            Thread t = new Thread(new ThreadStart(delegate()
                {
                    // First fetch the guesture
                    AssetGesture gesture = null;

                    if (gestureCache.ContainsKey(gestureID))
                    {
                        gesture = gestureCache[gestureID];
                    }
                    else
                    {
                        AutoResetEvent gotAsset = new AutoResetEvent(false);

                        Client.Assets.RequestAsset(gestureID, AssetType.Gesture, true,
                                                    delegate(AssetDownload transfer, Asset asset)
                                                    {
                                                        if (transfer.Success)
                                                        {
                                                            gesture = (AssetGesture)asset;
                                                        }

                                                        gotAsset.Set();
                                                    }
                        );

                        gotAsset.WaitOne(30 * 1000, false);

                        if (gesture != null && gesture.Decode())
                        {
                            lock (gestureCache)
                            {
                                if (!gestureCache.ContainsKey(gestureID))
                                {
                                    gestureCache[gestureID] = gesture;
                                }
                            }
                        }
                    }

                    // We got it, now we play it
                    if (gesture != null)
                    {
                        for (int i = 0; i < gesture.Sequence.Count; i++)
                        {
                            GestureStep step = gesture.Sequence[i];

                            switch (step.GestureStepType)
                            {
                                case GestureStepType.Chat:
                                    Chat(((GestureStepChat)step).Text, 0, ChatType.Normal);
                                    break;

                                case GestureStepType.Animation:
                                    GestureStepAnimation anim = (GestureStepAnimation)step;

                                    if (anim.AnimationStart)
                                    {
                                        if (SignaledAnimations.ContainsKey(anim.ID))
                                        {
                                            AnimationStop(anim.ID, true);
                                        }
                                        AnimationStart(anim.ID, true);
                                    }
                                    else
                                    {
                                        AnimationStop(anim.ID, true);
                                    }
                                    break;

                                case GestureStepType.Sound:
                                    Client.Sound.PlaySound(((GestureStepSound)step).ID);
                                    break;

                                case GestureStepType.Wait:
                                    GestureStepWait wait = (GestureStepWait)step;
                                    if (wait.WaitForTime)
                                    {
                                        Thread.Sleep((int)(1000f * wait.WaitTime));
                                    }
                                    if (wait.WaitForAnimation)
                                    {
                                        // TODO: implement waiting for all animations to end that were triggered
                                        // during playing of this guesture sequence
                                    }
                                    break;
                            }
                        }
                    }
                }));

            t.IsBackground = true;
            t.Name = "Gesture thread: " + gestureID;
            t.Start();
        }

        /// <summary>
        /// Mark gesture active
        /// </summary>
        /// <param name="invID">Inventory <seealso cref="UUID"/> of the gesture</param>
        /// <param name="assetID">Asset <seealso cref="UUID"/> of the gesture</param>
        public void ActivateGesture(UUID invID, UUID assetID)
        {
            ActivateGesturesPacket p = new ActivateGesturesPacket();

            p.AgentData.AgentID = AgentID;
            p.AgentData.SessionID = SessionID;
            p.AgentData.Flags = 0x00;

            ActivateGesturesPacket.DataBlock b = new ActivateGesturesPacket.DataBlock();
            b.ItemID = invID;
            b.AssetID = assetID;
            b.GestureFlags = 0x00;

            p.Data = new ActivateGesturesPacket.DataBlock[1];
            p.Data[0] = b;

            Client.Network.SendPacket(p);

        }

        /// <summary>
        /// Mark gesture inactive
        /// </summary>
        /// <param name="invID">Inventory <seealso cref="UUID"/> of the gesture</param>
        public void DeactivateGesture(UUID invID)
        {
            DeactivateGesturesPacket p = new DeactivateGesturesPacket();

            p.AgentData.AgentID = AgentID;
            p.AgentData.SessionID = SessionID;
            p.AgentData.Flags = 0x00;

            DeactivateGesturesPacket.DataBlock b = new DeactivateGesturesPacket.DataBlock();
            b.ItemID = invID;
            b.GestureFlags = 0x00;

            p.Data = new DeactivateGesturesPacket.DataBlock[1];
            p.Data[0] = b;

            Client.Network.SendPacket(p);
        }
        #endregion

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

            // TODO: Implement support for this
            animate.PhysicalAvatarEventList = new AgentAnimationPacket.PhysicalAvatarEventListBlock[0];

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
            if (Client.Network.CurrentSim == null)
                return false;

            teleportStat = TeleportStatus.None;

            if (simName != Client.Network.CurrentSim.Name)
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
                EventHandler<EventQueueRunningEventArgs> queueCallback =
                    delegate(object sender, EventQueueRunningEventArgs e)
                    {
                        if (e.Simulator == Client.Network.CurrentSim)
                            queueEvent.Set();
                    };

                Client.Network.EventQueueRunning += queueCallback;
                queueEvent.WaitOne(10 * 1000, false);
                Client.Network.EventQueueRunning -= queueCallback;
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
                InstantMessageOnline.Offline, this.SimPosition, UUID.Zero, Utils.EmptyBytes);

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
                InstantMessageOnline.Offline, Vector3.Zero, UUID.Zero, Utils.EmptyBytes);
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
            pick.Data.PosGlobal = globalPosition;
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

        /// <summary>
        /// Create or update profile Classified
        /// </summary>
        /// <param name="classifiedID">UUID of the classified to update, or random UUID to create a new classified</param>
        /// <param name="category">Defines what catagory the classified is in</param>
        /// <param name="snapshotID">UUID of the image displayed with the classified</param>
        /// <param name="price">Price that the classified will cost to place for a week</param>
        /// <param name="position">Global position of the classified landmark</param>
        /// <param name="name">Name of the classified</param>
        /// <param name="desc">Long description of the classified</param>
        /// <param name="autoRenew">if true, auto renew classified after expiration</param>
        public void UpdateClassifiedInfo(UUID classifiedID, DirectoryManager.ClassifiedCategories category,
            UUID snapshotID, int price, Vector3d position, string name, string desc, bool autoRenew)
        {
            ClassifiedInfoUpdatePacket classified = new ClassifiedInfoUpdatePacket();
            classified.AgentData.AgentID = Client.Self.AgentID;
            classified.AgentData.SessionID = Client.Self.SessionID;

            classified.Data.ClassifiedID = classifiedID;
            classified.Data.Category = (uint)category;

            classified.Data.ParcelID = UUID.Zero;
            // TODO: verify/fix ^
            classified.Data.ParentEstate = 0;
            // TODO: verify/fix ^

            classified.Data.SnapshotID = snapshotID;
            classified.Data.PosGlobal = position;

            classified.Data.ClassifiedFlags = autoRenew ? (byte)32 : (byte)0;
            // TODO: verify/fix ^

            classified.Data.PriceForListing = price;
            classified.Data.Name = Utils.StringToBytes(name);
            classified.Data.Desc = Utils.StringToBytes(desc);
            Client.Network.SendPacket(classified);
        }

        /// <summary>
        /// Create or update profile Classified
        /// </summary>
        /// <param name="classifiedID">UUID of the classified to update, or random UUID to create a new classified</param>
        /// <param name="category">Defines what catagory the classified is in</param>
        /// <param name="snapshotID">UUID of the image displayed with the classified</param>
        /// <param name="price">Price that the classified will cost to place for a week</param>
        /// <param name="name">Name of the classified</param>
        /// <param name="desc">Long description of the classified</param>
        /// <param name="autoRenew">if true, auto renew classified after expiration</param>
        public void UpdateClassifiedInfo(UUID classifiedID, DirectoryManager.ClassifiedCategories category, UUID snapshotID, int price, string name, string desc, bool autoRenew)
        {
            UpdateClassifiedInfo(classifiedID, category, snapshotID, price, Client.Self.GlobalPosition, name, desc, autoRenew);
        }

        /// <summary>
        /// Delete a classified ad
        /// </summary>
        /// <param name="classifiedID">The classified ads ID</param>
        public void DeleteClassfied(UUID classifiedID)
        {
            ClassifiedDeletePacket classified = new ClassifiedDeletePacket();
            classified.AgentData.AgentID = Client.Self.AgentID;
            classified.AgentData.SessionID = Client.Self.SessionID;

            classified.Data.ClassifiedID = classifiedID;
            Client.Network.SendPacket(classified);
        }

        /// <summary>
        /// Fetches resource usage by agents attachmetns
        /// </summary>
        /// <param name="callback">Called when the requested information is collected</param>
        public void GetAttachmentResources(AttachmentResourcesCallback callback)
        {
            try
            {
                Uri url = Client.Network.CurrentSim.Caps.CapabilityURI("AttachmentResources");
                CapsClient request = new CapsClient(url);

                request.OnComplete += delegate(CapsClient client, OSD result, Exception error)
                {
                    try
                    {
                        if (result == null || error != null)
                        {
                            callback(false, null);
                        }
                        AttachmentResourcesMessage info = AttachmentResourcesMessage.FromOSD(result);
                        callback(true, info);

                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Failed fetching AttachmentResources", Helpers.LogLevel.Error, Client, ex);
                        callback(false, null);
                    }
                };

                request.BeginGetResponse(Client.Settings.CAPS_TIMEOUT);
            }
            catch (Exception ex)
            {
                Logger.Log("Failed fetching AttachmentResources", Helpers.LogLevel.Error, Client, ex);
                callback(false, null);
            }
        }

        #endregion Misc

        #region Packet Handlers

        /// <summary>
        /// Take an incoming ImprovedInstantMessage packet, auto-parse, and if
        /// OnInstantMessage is defined call that with the appropriate arguments
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void InstantMessageHandler(object sender, PacketReceivedEventArgs e)
        {
            Packet packet = e.Packet;
            Simulator simulator = e.Simulator;

            if (packet.Type == PacketType.ImprovedInstantMessage)
            {
                ImprovedInstantMessagePacket im = (ImprovedInstantMessagePacket)packet;

                if (m_InstantMessage != null)
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

                    OnInstantMessage(new InstantMessageEventArgs(message, simulator));
                }
            }
        }

        /// <summary>
        /// Take an incoming Chat packet, auto-parse, and if OnChat is defined call 
        ///   that with the appropriate arguments.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void ChatHandler(object sender, PacketReceivedEventArgs e)
        {
            if (m_Chat != null)
            {
                Packet packet = e.Packet;

                ChatFromSimulatorPacket chat = (ChatFromSimulatorPacket)packet;

                OnChat(new ChatEventArgs(e.Simulator, Utils.BytesToString(chat.ChatData.Message),
                    (ChatAudibleLevel)chat.ChatData.Audible,
                    (ChatType)chat.ChatData.ChatType,
                    (ChatSourceType)chat.ChatData.SourceType,
                    Utils.BytesToString(chat.ChatData.FromName),
                    chat.ChatData.SourceID,
                    chat.ChatData.OwnerID,
                    chat.ChatData.Position));
            }
        }

        /// <summary>
        /// Used for parsing llDialogs
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void ScriptDialogHandler(object sender, PacketReceivedEventArgs e)
        {
            if (m_ScriptDialog != null)
            {
                Packet packet = e.Packet;

                ScriptDialogPacket dialog = (ScriptDialogPacket)packet;
                List<string> buttons = new List<string>();

                foreach (ScriptDialogPacket.ButtonsBlock button in dialog.Buttons)
                {
                    buttons.Add(Utils.BytesToString(button.ButtonLabel));
                }

                OnScriptDialog(new ScriptDialogEventArgs(Utils.BytesToString(dialog.Data.Message),
                    Utils.BytesToString(dialog.Data.ObjectName),
                    dialog.Data.ImageID,
                    dialog.Data.ObjectID,
                    Utils.BytesToString(dialog.Data.FirstName),
                    Utils.BytesToString(dialog.Data.LastName),
                    dialog.Data.ChatChannel,
                    buttons));
            }
        }

        /// <summary>
        /// Used for parsing llRequestPermissions dialogs
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void ScriptQuestionHandler(object sender, PacketReceivedEventArgs e)
        {
            if (m_ScriptQuestion != null)
            {
                Packet packet = e.Packet;
                Simulator simulator = e.Simulator;

                ScriptQuestionPacket question = (ScriptQuestionPacket)packet;

                OnScriptQuestion(new ScriptQuestionEventArgs(simulator,
                        question.Data.TaskID,
                        question.Data.ItemID,
                        Utils.BytesToString(question.Data.ObjectName),
                        Utils.BytesToString(question.Data.ObjectOwner),
                        (ScriptPermission)question.Data.Questions));
            }
        }

        /// <summary>
        /// Handles Script Control changes when Script with permissions releases or takes a control
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        private void ScriptControlChangeHandler(object sender, PacketReceivedEventArgs e)
        {
            if (m_ScriptControl != null)
            {
                Packet packet = e.Packet;

                ScriptControlChangePacket change = (ScriptControlChangePacket)packet;
                for (int i = 0; i < change.Data.Length; i++)
                {
                    OnScriptControlChange(new ScriptControlEventArgs((ScriptControlChange)change.Data[i].Controls,
                            change.Data[i].PassToAgent,
                            change.Data[i].TakeControls));
                }
            }
        }

        /// <summary>
        /// Used for parsing llLoadURL Dialogs
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void LoadURLHandler(object sender, PacketReceivedEventArgs e)
        {

            if (m_LoadURL != null)
            {
                Packet packet = e.Packet;

                LoadURLPacket loadURL = (LoadURLPacket)packet;

                OnLoadURL(new LoadUrlEventArgs(
                    Utils.BytesToString(loadURL.Data.ObjectName),
                    loadURL.Data.ObjectID,
                    loadURL.Data.OwnerID,
                    loadURL.Data.OwnerIsGroup,
                    Utils.BytesToString(loadURL.Data.Message),
                    Utils.BytesToString(loadURL.Data.URL)
                ));
            }
        }

        /// <summary>
        /// Update client's Position, LookAt and region handle from incoming packet
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        /// <remarks>This occurs when after an avatar moves into a new sim</remarks>
        private void MovementCompleteHandler(object sender, PacketReceivedEventArgs e)
        {
            Packet packet = e.Packet;
            Simulator simulator = e.Simulator;

            AgentMovementCompletePacket movement = (AgentMovementCompletePacket)packet;

            relativePosition = movement.Data.Position;
            Movement.Camera.LookDirection(movement.Data.LookAt);
            simulator.Handle = movement.Data.RegionHandle;
            simulator.SimVersion = Utils.BytesToString(movement.SimData.ChannelVersion);
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void HealthHandler(object sender, PacketReceivedEventArgs e)
        {
            Packet packet = e.Packet;
            health = ((HealthMessagePacket)packet).HealthData.Health;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void AgentDataUpdateHandler(object sender, PacketReceivedEventArgs e)
        {
            Packet packet = e.Packet;
            Simulator simulator = e.Simulator;

            AgentDataUpdatePacket p = (AgentDataUpdatePacket)packet;

            if (p.AgentData.AgentID == simulator.Client.Self.AgentID)
            {
                firstName = Utils.BytesToString(p.AgentData.FirstName);
                lastName = Utils.BytesToString(p.AgentData.LastName);
                activeGroup = p.AgentData.ActiveGroupID;
                activeGroupPowers = (GroupPowers)p.AgentData.GroupPowers;

                if (m_AgentData != null)
                {
                    string groupTitle = Utils.BytesToString(p.AgentData.GroupTitle);
                    string groupName = Utils.BytesToString(p.AgentData.GroupName);

                    OnAgentData(new AgentDataReplyEventArgs(firstName, lastName, activeGroup, groupTitle, activeGroupPowers, groupName));
                }
            }
            else
            {
                Logger.Log("Got an AgentDataUpdate packet for avatar " + p.AgentData.AgentID.ToString() +
                    " instead of " + Client.Self.AgentID.ToString() + ", this shouldn't happen", Helpers.LogLevel.Error, Client);
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void MoneyBalanceReplyHandler(object sender, PacketReceivedEventArgs e)
        {
            Packet packet = e.Packet;

            if (packet.Type == PacketType.MoneyBalanceReply)
            {
                MoneyBalanceReplyPacket reply = (MoneyBalanceReplyPacket)packet;
                this.balance = reply.MoneyData.MoneyBalance;

                if (m_MoneyBalance != null)
                {
                    OnMoneyBalanceReply(new MoneyBalanceReplyEventArgs(reply.MoneyData.TransactionID,
                        reply.MoneyData.TransactionSuccess,
                        reply.MoneyData.MoneyBalance,
                        reply.MoneyData.SquareMetersCredit,
                        reply.MoneyData.SquareMetersCommitted,
                        Utils.BytesToString(reply.MoneyData.Description)));
                }
            }

            if (m_Balance != null)
            {
                OnBalance(new BalanceEventArgs(balance));
            }
        }

        protected void EstablishAgentCommunicationEventHandler(string capsKey, IMessage message, Simulator simulator)
        {
            EstablishAgentCommunicationMessage msg = (EstablishAgentCommunicationMessage)message;

            if (Client.Settings.MULTIPLE_SIMS)
            {

                IPEndPoint endPoint = new IPEndPoint(msg.Address, msg.Port);
                Simulator sim = Client.Network.FindSimulator(endPoint);

                if (sim == null)
                {
                    Logger.Log("Got EstablishAgentCommunication for unknown sim " + msg.Address + ":" + msg.Port,
                        Helpers.LogLevel.Error, Client);

                    // FIXME: Should we use this opportunity to connect to the simulator?
                }
                else
                {
                    Logger.Log("Got EstablishAgentCommunication for " + sim.ToString(),
                        Helpers.LogLevel.Info, Client);

                    sim.SetSeedCaps(msg.SeedCapability.ToString());
                }
            }
        }

        /// <summary>
        /// Process TeleportFailed message sent via EventQueue, informs agent its last teleport has failed and why.
        /// </summary>
        /// <param name="messageKey">The Message Key</param>
        /// <param name="message">An IMessage object Deserialized from the recieved message event</param>
        /// <param name="simulator">The simulator originating the event message</param>
        public void TeleportFailedEventHandler(string messageKey, IMessage message, Simulator simulator)
        {
            TeleportFailedMessage msg = (TeleportFailedMessage)message;

            TeleportFailedPacket failedPacket = new TeleportFailedPacket();
            failedPacket.Info.AgentID = msg.AgentID;
            failedPacket.Info.Reason = Utils.StringToBytes(msg.Reason);

            TeleportHandler(this, new PacketReceivedEventArgs(failedPacket, simulator));
        }

        /// <summary>
        /// Process TeleportFinish from Event Queue and pass it onto our TeleportHandler
        /// </summary>
        /// <param name="capsKey">The message system key for this event</param>
        /// <param name="message">IMessage object containing decoded data from OSD</param>
        /// <param name="simulator">The simulator originating the event message</param>
        private void TeleportFinishEventHandler(string capsKey, IMessage message, Simulator simulator)
        {
            TeleportFinishMessage msg = (TeleportFinishMessage)message;

            TeleportFinishPacket p = new TeleportFinishPacket();
            p.Info.AgentID = msg.AgentID;
            p.Info.LocationID = (uint)msg.LocationID;
            p.Info.RegionHandle = msg.RegionHandle;
            p.Info.SeedCapability = Utils.StringToBytes(msg.SeedCapability.ToString()); // FIXME: Check This
            p.Info.SimAccess = (byte)msg.SimAccess;
            p.Info.SimIP = Utils.IPToUInt(msg.IP);
            p.Info.SimPort = (ushort)msg.Port;
            p.Info.TeleportFlags = (uint)msg.Flags;

            // pass the packet onto the teleport handler
            TeleportHandler(this, new PacketReceivedEventArgs(p, simulator));
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void TeleportHandler(object sender, PacketReceivedEventArgs e)
        {
            Packet packet = e.Packet;
            Simulator simulator = e.Simulator;

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

            if (m_Teleport != null)
            {
                OnTeleport(new TeleportEventArgs(teleportMessage, teleportStat, flags));
            }

            if (finished) teleportEvent.Set();
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void AvatarAnimationHandler(object sender, PacketReceivedEventArgs e)
        {
            Packet packet = e.Packet;
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
            }

            if (m_AnimationsChanged != null)
            {
                OnAnimationsChanged(new AnimationsChangedEventArgs(this.SignaledAnimations));
            }

        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void MeanCollisionAlertHandler(object sender, PacketReceivedEventArgs e)
        {
            if (m_MeanCollision != null)
            {
                Packet packet = e.Packet;
                MeanCollisionAlertPacket collision = (MeanCollisionAlertPacket)packet;

                for (int i = 0; i < collision.MeanCollision.Length; i++)
                {
                    MeanCollisionAlertPacket.MeanCollisionBlock block = collision.MeanCollision[i];

                    DateTime time = Utils.UnixTimeToDateTime(block.Time);
                    MeanCollisionType type = (MeanCollisionType)block.Type;

                    OnMeanCollision(new MeanCollisionEventArgs(type, block.Perp, block.Victim, block.Mag, time));
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

        private void Network_OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            // Null out the cached fullName since it can change after logging
            // in again (with a different account name or different login
            // server but using the same GridClient object
            fullName = null;
        }

        /// <summary>
        /// Crossed region handler for message that comes across the EventQueue. Sent to an agent
        /// when the agent crosses a sim border into a new region.
        /// </summary>
        /// <param name="capsKey">The message key</param>
        /// <param name="message">the IMessage object containing the deserialized data sent from the simulator</param>
        /// <param name="simulator">The <see cref="Simulator"/> which originated the packet</param>
        private void CrossedRegionEventHandler(string capsKey, IMessage message, Simulator simulator)
        {
            CrossedRegionMessage crossed = (CrossedRegionMessage)message;

            IPEndPoint endPoint = new IPEndPoint(crossed.IP, crossed.Port);

            Logger.DebugLog("Crossed in to new region area, attempting to connect to " + endPoint.ToString(), Client);

            Simulator oldSim = Client.Network.CurrentSim;
            Simulator newSim = Client.Network.Connect(endPoint, crossed.RegionHandle, true, crossed.SeedCapability.ToString());

            if (newSim != null)
            {
                Logger.Log("Finished crossing over in to region " + newSim.ToString(), Helpers.LogLevel.Info, Client);

                if (m_RegionCrossed != null)
                {
                    OnRegionCrossed(new RegionCrossedEventArgs(oldSim, newSim));
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

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        /// <remarks>This packet is now being sent via the EventQueue</remarks>
        protected void CrossedRegionHandler(object sender, PacketReceivedEventArgs e)
        {
            Packet packet = e.Packet;
            CrossedRegionPacket crossing = (CrossedRegionPacket)packet;
            string seedCap = Utils.BytesToString(crossing.RegionData.SeedCapability);
            IPEndPoint endPoint = new IPEndPoint(crossing.RegionData.SimIP, crossing.RegionData.SimPort);

            Logger.DebugLog("Crossed in to new region area, attempting to connect to " + endPoint.ToString(), Client);

            Simulator oldSim = Client.Network.CurrentSim;
            Simulator newSim = Client.Network.Connect(endPoint, crossing.RegionData.RegionHandle, true, seedCap);

            if (newSim != null)
            {
                Logger.Log("Finished crossing over in to region " + newSim.ToString(), Helpers.LogLevel.Info, Client);

                if (m_RegionCrossed != null)
                {
                    OnRegionCrossed(new RegionCrossedEventArgs(oldSim, newSim));
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
        /// <param name="message">IMessage object containing decoded data from OSD</param>
        /// <param name="simulator"></param>
        protected void ChatterBoxSessionEventReplyEventHandler(string capsKey, IMessage message, Simulator simulator)
        {
            ChatterboxSessionEventReplyMessage msg = (ChatterboxSessionEventReplyMessage)message;

            if (!msg.Success)
            {
                Logger.Log("Attempt to send group chat to non-existant session for group " + msg.SessionID,
                    Helpers.LogLevel.Info, Client);
            }
        }

        /// <summary>
        /// Response from request to join a group chat
        /// </summary>
        /// <param name="capsKey"></param>
        /// <param name="message">IMessage object containing decoded data from OSD</param>
        /// <param name="simulator"></param>
        protected void ChatterBoxSessionStartReplyEventHandler(string capsKey, IMessage message, Simulator simulator)
        {
            ChatterBoxSessionStartReplyMessage msg = (ChatterBoxSessionStartReplyMessage)message;

            if (msg.Success)
            {
                lock (GroupChatSessions.Dictionary)
                    if (!GroupChatSessions.ContainsKey(msg.SessionID))
                        GroupChatSessions.Add(msg.SessionID, new List<ChatSessionMember>());
            }

            OnGroupChatJoined(new GroupChatJoinedEventArgs(msg.SessionID, msg.SessionName, msg.TempSessionID, msg.Success));
        }

        /// <summary>
        /// Someone joined or left group chat
        /// </summary>
        /// <param name="capsKey"></param>
        /// <param name="message">IMessage object containing decoded data from OSD</param>
        /// <param name="simulator"></param>
        private void ChatterBoxSessionAgentListUpdatesEventHandler(string capsKey, IMessage message, Simulator simulator)
        {
            ChatterBoxSessionAgentListUpdatesMessage msg = (ChatterBoxSessionAgentListUpdatesMessage)message;

            lock (GroupChatSessions.Dictionary)
                if (!GroupChatSessions.ContainsKey(msg.SessionID))
                    GroupChatSessions.Add(msg.SessionID, new List<ChatSessionMember>());

            for (int i = 0; i < msg.Updates.Length; i++)
            {
                ChatSessionMember fndMbr;
                lock (GroupChatSessions.Dictionary)
                {
                    fndMbr = GroupChatSessions[msg.SessionID].Find(delegate(ChatSessionMember member)
                    {
                        return member.AvatarKey == msg.Updates[i].AgentID;
                    });
                }

                if (msg.Updates[i].Transition != null)
                {
                    if (msg.Updates[i].Transition.Equals("ENTER"))
                    {
                        if (fndMbr.AvatarKey == UUID.Zero)
                        {
                            fndMbr = new ChatSessionMember();
                            fndMbr.AvatarKey = msg.Updates[i].AgentID;

                            lock (GroupChatSessions.Dictionary)
                                GroupChatSessions[msg.SessionID].Add(fndMbr);

                            if (m_ChatSessionMemberAdded != null)
                            {
                                OnChatSessionMemberAdded(new ChatSessionMemberAddedEventArgs(msg.SessionID, fndMbr.AvatarKey));
                            }
                        }
                    }
                    else if (msg.Updates[i].Transition.Equals("LEAVE"))
                    {
                        if (fndMbr.AvatarKey != UUID.Zero)
                            lock (GroupChatSessions.Dictionary)
                                GroupChatSessions[msg.SessionID].Remove(fndMbr);

                        if (m_ChatSessionMemberLeft != null)
                        {
                            OnChatSessionMemberLeft(new ChatSessionMemberLeftEventArgs(msg.SessionID, msg.Updates[i].AgentID));
                        }
                    }
                }

                // handle updates
                ChatSessionMember update_member = GroupChatSessions.Dictionary[msg.SessionID].Find(delegate(ChatSessionMember m)
                {
                    return m.AvatarKey == msg.Updates[i].AgentID;
                });


                update_member.MuteText = msg.Updates[i].MuteText;
                update_member.MuteVoice = msg.Updates[i].MuteVoice;

                update_member.CanVoiceChat = msg.Updates[i].CanVoiceChat;
                update_member.IsModerator = msg.Updates[i].IsModerator;

                // replace existing member record
                lock (GroupChatSessions.Dictionary)
                {
                    int found = GroupChatSessions.Dictionary[msg.SessionID].FindIndex(delegate(ChatSessionMember m)
                    {
                        return m.AvatarKey == msg.Updates[i].AgentID;
                    });

                    if (found >= 0)
                        GroupChatSessions.Dictionary[msg.SessionID][found] = update_member;
                }
            }
        }

        /// <summary>
        /// Handle a group chat Invitation
        /// </summary>
        /// <param name="capsKey">Caps Key</param>
        /// <param name="message">IMessage object containing decoded data from OSD</param>
        /// <param name="simulator">Originating Simulator</param>
        private void ChatterBoxInvitationEventHandler(string capsKey, IMessage message, Simulator simulator)
        {
            if (m_InstantMessage != null)
            {
                ChatterBoxInvitationMessage msg = (ChatterBoxInvitationMessage)message;

                //TODO: do something about invitations to voice group chat/friends conference
                //Skip for now
                if (msg.Voice) return;

                InstantMessage im = new InstantMessage();

                im.FromAgentID = msg.FromAgentID;
                im.FromAgentName = msg.FromAgentName;
                im.ToAgentID = msg.ToAgentID;
                im.ParentEstateID = (uint)msg.ParentEstateID;
                im.RegionID = msg.RegionID;
                im.Position = msg.Position;
                im.Dialog = msg.Dialog;
                im.GroupIM = msg.GroupIM;
                im.IMSessionID = msg.IMSessionID;
                im.Timestamp = msg.Timestamp;
                im.Message = msg.Message;
                im.Offline = msg.Offline;
                im.BinaryBucket = msg.BinaryBucket;
                try
                {
                    ChatterBoxAcceptInvite(msg.IMSessionID);
                }
                catch (Exception ex)
                {
                    Logger.Log("Failed joining IM:", Helpers.LogLevel.Warning, Client, ex);
                }
                OnInstantMessage(new InstantMessageEventArgs(im, simulator));
            }
        }


        /// <summary>
        /// Moderate a chat session
        /// </summary>
        /// <param name="sessionID">the <see cref="UUID"/> of the session to moderate, for group chats this will be the groups UUID</param>
        /// <param name="memberID">the <see cref="UUID"/> of the avatar to moderate</param>
        /// <param name="key">Either "voice" to moderate users voice, or "text" to moderate users text session</param>
        /// <param name="moderate">true to moderate (silence user), false to allow avatar to speak</param>
        public void ModerateChatSessions(UUID sessionID, UUID memberID, string key, bool moderate)
        {
            if (Client.Network.CurrentSim == null || Client.Network.CurrentSim.Caps == null)
                throw new Exception("ChatSessionRequest capability is not currently available");

            Uri url = Client.Network.CurrentSim.Caps.CapabilityURI("ChatSessionRequest");

            if (url != null)
            {
                ChatSessionRequestMuteUpdate req = new ChatSessionRequestMuteUpdate();

                req.RequestKey = key;
                req.RequestValue = moderate;
                req.SessionID = sessionID;
                req.AgentID = memberID;

                CapsClient request = new CapsClient(url);
                request.BeginGetResponse(req.Serialize(), OSDFormat.Xml, Client.Settings.CAPS_TIMEOUT);
            }
            else
            {
                throw new Exception("ChatSessionRequest capability is not currently available");
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void AlertMessageHandler(object sender, PacketReceivedEventArgs e)
        {
            if (m_AlertMessage != null)
            {
                Packet packet = e.Packet;

                AlertMessagePacket alert = (AlertMessagePacket)packet;

                OnAlertMessage(new AlertMessageEventArgs(Utils.BytesToString(alert.AlertData.Message)));
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void CameraConstraintHandler(object sender, PacketReceivedEventArgs e)
        {
            if (m_CameraConstraint != null)
            {
                Packet packet = e.Packet;

                CameraConstraintPacket camera = (CameraConstraintPacket)packet;
                OnCameraConstraint(new CameraConstraintEventArgs(camera.CameraCollidePlane.Plane));
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void ScriptSensorReplyHandler(object sender, PacketReceivedEventArgs e)
        {
            if (m_ScriptSensorReply != null)
            {
                Packet packet = e.Packet;

                ScriptSensorReplyPacket reply = (ScriptSensorReplyPacket)packet;

                for (int i = 0; i < reply.SensedData.Length; i++)
                {
                    ScriptSensorReplyPacket.SensedDataBlock block = reply.SensedData[i];
                    ScriptSensorReplyPacket.RequesterBlock requestor = reply.Requester;

                    OnScriptSensorReply(new ScriptSensorReplyEventArgs(requestor.SourceID, block.GroupID, Utils.BytesToString(block.Name),
                      block.ObjectID, block.OwnerID, block.Position, block.Range, block.Rotation, (ScriptSensorTypeFlags)block.Type, block.Velocity));
                }
            }

        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void AvatarSitResponseHandler(object sender, PacketReceivedEventArgs e)
        {
            if (m_AvatarSitResponse != null)
            {
                Packet packet = e.Packet;

                AvatarSitResponsePacket sit = (AvatarSitResponsePacket)packet;

                OnAvatarSitResponse(new AvatarSitResponseEventArgs(sit.SitObject.ID, sit.SitTransform.AutoPilot, sit.SitTransform.CameraAtOffset,
                  sit.SitTransform.CameraEyeOffset, sit.SitTransform.ForceMouselook, sit.SitTransform.SitPosition,
                  sit.SitTransform.SitRotation));
            }
        }

        #endregion Packet Handlers
    }

    #region Event Argument Classes

    /// <summary>
    /// 
    /// </summary>
    public class ChatEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly string m_Message;
        private readonly ChatAudibleLevel m_AudibleLevel;
        private readonly ChatType m_Type;
        private readonly ChatSourceType m_SourceType;
        private readonly string m_FromName;
        private readonly UUID m_SourceID;
        private readonly UUID m_OwnerID;
        private readonly Vector3 m_Position;

        /// <summary>Get the simulator sending the message</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary>Get the message sent</summary>
        public string Message { get { return m_Message; } }
        /// <summary>Get the audible level of the message</summary>
        public ChatAudibleLevel AudibleLevel { get { return m_AudibleLevel; } }
        /// <summary>Get the type of message sent: whisper, shout, etc</summary>
        public ChatType Type { get { return m_Type; } }
        /// <summary>Get the source type of the message sender</summary>
        public ChatSourceType SourceType { get { return m_SourceType; } }
        /// <summary>Get the name of the agent or object sending the message</summary>
        public string FromName { get { return m_FromName; } }
        /// <summary>Get the ID of the agent or object sending the message</summary>
        public UUID SourceID { get { return m_SourceID; } }
        /// <summary>Get the ID of the object owner, or the agent ID sending the message</summary>
        public UUID OwnerID { get { return m_OwnerID; } }
        /// <summary>Get the position of the agent or object sending the message</summary>
        public Vector3 Position { get { return m_Position; } }

        /// <summary>
        /// Construct a new instance of the ChatEventArgs object
        /// </summary>
        /// <param name="simulator">Sim from which the message originates</param>
        /// <param name="message">The message sent</param>
        /// <param name="audible">The audible level of the message</param>
        /// <param name="type">The type of message sent: whisper, shout, etc</param>
        /// <param name="sourceType">The source type of the message sender</param>
        /// <param name="fromName">The name of the agent or object sending the message</param>
        /// <param name="sourceId">The ID of the agent or object sending the message</param>
        /// <param name="ownerid">The ID of the object owner, or the agent ID sending the message</param>
        /// <param name="position">The position of the agent or object sending the message</param>
        public ChatEventArgs(Simulator simulator, string message, ChatAudibleLevel audible, ChatType type,
        ChatSourceType sourceType, string fromName, UUID sourceId, UUID ownerid, Vector3 position)
        {
            this.m_Simulator = simulator;
            this.m_Message = message;
            this.m_AudibleLevel = audible;
            this.m_Type = type;
            this.m_SourceType = sourceType;
            this.m_FromName = fromName;
            this.m_SourceID = sourceId;
            this.m_Position = position;
            this.m_OwnerID = ownerid;
        }
    }

    /// <summary>Contains the data sent when a primitive opens a dialog with this agent</summary>
    public class ScriptDialogEventArgs : EventArgs
    {
        private readonly string m_Message;
        private readonly string m_ObjectName;
        private readonly UUID m_ImageID;
        private readonly UUID m_ObjectID;
        private readonly string m_FirstName;
        private readonly string m_LastName;
        private readonly int m_Channel;
        private readonly List<string> m_ButtonLabels;

        /// <summary>Get the dialog message</summary>
        public string Message { get { return m_Message; } }
        /// <summary>Get the name of the object that sent the dialog request</summary>
        public string ObjectName { get { return m_ObjectName; } }
        /// <summary>Get the ID of the image to be displayed</summary>
        public UUID ImageID { get { return m_ImageID; } }
        /// <summary>Get the ID of the primitive sending the dialog</summary>
        public UUID ObjectID { get { return m_ObjectID; } }
        /// <summary>Get the first name of the senders owner</summary>
        public string FirstName { get { return m_FirstName; } }
        /// <summary>Get the last name of the senders owner</summary>
        public string LastName { get { return m_LastName; } }
        /// <summary>Get the communication channel the dialog was sent on, responses
        /// should also send responses on this same channel</summary>
        public int Channel { get { return m_Channel; } }
        /// <summary>Get the string labels containing the options presented in this dialog</summary>
        public List<string> ButtonLabels { get { return m_ButtonLabels; } }

        /// <summary>
        /// Construct a new instance of the ScriptDialogEventArgs
        /// </summary>
        /// <param name="message">The dialog message</param>
        /// <param name="objectName">The name of the object that sent the dialog request</param>
        /// <param name="imageID">The ID of the image to be displayed</param>
        /// <param name="objectID">The ID of the primitive sending the dialog</param>
        /// <param name="firstName">The first name of the senders owner</param>
        /// <param name="lastName">The last name of the senders owner</param>
        /// <param name="chatChannel">The communication channel the dialog was sent on</param>
        /// <param name="buttons">The string labels containing the options presented in this dialog</param>
        public ScriptDialogEventArgs(string message, string objectName, UUID imageID,
            UUID objectID, string firstName, string lastName, int chatChannel, List<string> buttons)
        {
            this.m_Message = message;
            this.m_ObjectName = objectName;
            this.m_ImageID = imageID;
            this.m_ObjectID = objectID;
            this.m_FirstName = firstName;
            this.m_LastName = lastName;
            this.m_Channel = chatChannel;
            this.m_ButtonLabels = buttons;
        }
    }

    /// <summary>Contains the data sent when a primitive requests debit or other permissions
    /// requesting a YES or NO answer</summary>
    public class ScriptQuestionEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly UUID m_TaskID;
        private readonly UUID m_ItemID;
        private readonly string m_ObjectName;
        private readonly string m_ObjectOwnerName;
        private readonly ScriptPermission m_Questions;

        /// <summary>Get the simulator containing the object sending the request</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary>Get the ID of the script making the request</summary>
        public UUID TaskID { get { return m_TaskID; } }
        /// <summary>Get the ID of the primitive containing the script making the request</summary>
        public UUID ItemID { get { return m_ItemID; } }
        /// <summary>Get the name of the primitive making the request</summary>
        public string ObjectName { get { return m_ObjectName; } }
        /// <summary>Get the name of the owner of the object making the request</summary>
        public string ObjectOwnerName { get { return m_ObjectOwnerName; } }
        /// <summary>Get the permissions being requested</summary>
        public ScriptPermission Questions { get { return m_Questions; } }

        /// <summary>
        /// Construct a new instance of the ScriptQuestionEventArgs
        /// </summary>
        /// <param name="simulator">The simulator containing the object sending the request</param>
        /// <param name="taskID">The ID of the script making the request</param>
        /// <param name="itemID">The ID of the primitive containing the script making the request</param>
        /// <param name="objectName">The name of the primitive making the request</param>
        /// <param name="objectOwner">The name of the owner of the object making the request</param>
        /// <param name="questions">The permissions being requested</param>
        public ScriptQuestionEventArgs(Simulator simulator, UUID taskID, UUID itemID, string objectName, string objectOwner, ScriptPermission questions)
        {
            this.m_Simulator = simulator;
            this.m_TaskID = taskID;
            this.m_ItemID = itemID;
            this.m_ObjectName = objectName;
            this.m_ObjectOwnerName = objectOwner;
            this.m_Questions = questions;
        }

    }

    /// <summary>Contains the data sent when a primitive sends a request 
    /// to an agent to open the specified URL</summary>
    public class LoadUrlEventArgs : EventArgs
    {
        private readonly string m_ObjectName;
        private readonly UUID m_ObjectID;
        private readonly UUID m_OwnerID;
        private readonly bool m_OwnerIsGroup;
        private readonly string m_Message;
        private readonly string m_URL;

        /// <summary>Get the name of the object sending the request</summary>
        public string ObjectName { get { return m_ObjectName; } }
        /// <summary>Get the ID of the object sending the request</summary>
        public UUID ObjectID { get { return m_ObjectID; } }
        /// <summary>Get the ID of the owner of the object sending the request</summary>
        public UUID OwnerID { get { return m_OwnerID; } }
        /// <summary>True if the object is owned by a group</summary>
        public bool OwnerIsGroup { get { return m_OwnerIsGroup; } }
        /// <summary>Get the message sent with the request</summary>
        public string Message { get { return m_Message; } }
        /// <summary>Get the URL the object sent</summary>
        public string URL { get { return m_URL; } }

        /// <summary>
        /// Construct a new instance of the LoadUrlEventArgs
        /// </summary>
        /// <param name="objectName">The name of the object sending the request</param>
        /// <param name="objectID">The ID of the object sending the request</param>
        /// <param name="ownerID">The ID of the owner of the object sending the request</param>
        /// <param name="ownerIsGroup">True if the object is owned by a group</param>
        /// <param name="message">The message sent with the request</param>
        /// <param name="URL">The URL the object sent</param>
        public LoadUrlEventArgs(string objectName, UUID objectID, UUID ownerID, bool ownerIsGroup, string message, string URL)
        {
            this.m_ObjectName = objectName;
            this.m_ObjectID = objectID;
            this.m_OwnerID = ownerID;
            this.m_OwnerIsGroup = ownerIsGroup;
            this.m_Message = message;
            this.m_URL = URL;
        }
    }

    /// <summary>The date received from an ImprovedInstantMessage</summary>
    public class InstantMessageEventArgs : EventArgs
    {
        private readonly InstantMessage m_IM;
        private readonly Simulator m_Simulator;

        /// <summary>Get the InstantMessage object</summary>
        public InstantMessage IM { get { return m_IM; } }
        /// <summary>Get the simulator where the InstantMessage origniated</summary>
        public Simulator Simulator { get { return m_Simulator; } }

        /// <summary>
        /// Construct a new instance of the InstantMessageEventArgs object
        /// </summary>
        /// <param name="im">the InstantMessage object</param>
        /// <param name="simulator">the simulator where the InstantMessage origniated</param>
        public InstantMessageEventArgs(InstantMessage im, Simulator simulator)
        {
            this.m_IM = im;
            this.m_Simulator = simulator;
        }
    }

    /// <summary>Contains the currency balance</summary>
    public class BalanceEventArgs : EventArgs
    {
        private readonly int m_Balance;

        /// <summary>
        /// Get the currenct balance
        /// </summary>
        public int Balance { get { return m_Balance; } }

        /// <summary>
        /// Construct a new BalanceEventArgs object
        /// </summary>
        /// <param name="balance">The currenct balance</param>
        public BalanceEventArgs(int balance)
        {
            this.m_Balance = balance;
        }
    }

    /// <summary>Contains the transaction summary when an item is purchased, 
    /// money is given, or land is purchased</summary>
    public class MoneyBalanceReplyEventArgs : EventArgs
    {
        private readonly UUID m_TransactionID;
        private readonly bool m_Success;
        private readonly int m_Balance;
        private readonly int m_MetersCredit;
        private readonly int m_MetersCommitted;
        private readonly string m_Description;

        /// <summary>Get the ID of the transaction</summary>
        public UUID TransactionID { get { return m_TransactionID; } }
        /// <summary>True of the transaction was successful</summary>
        public bool Success { get { return m_Success; } }
        /// <summary>Get the remaining currency balance</summary>
        public int Balance { get { return m_Balance; } }
        /// <summary>Get the meters credited</summary>
        public int MetersCredit { get { return m_MetersCredit; } }
        /// <summary>Get the meters comitted</summary>
        public int MetersCommitted { get { return m_MetersCommitted; } }
        /// <summary>Get the description of the transaction</summary>
        public string Description { get { return m_Description; } }

        /// <summary>
        /// Construct a new instance of the MoneyBalanceReplyEventArgs object
        /// </summary>
        /// <param name="transactionID">The ID of the transaction</param>
        /// <param name="transactionSuccess">True of the transaction was successful</param>
        /// <param name="balance">The current currency balance</param>
        /// <param name="metersCredit">The meters credited</param>
        /// <param name="metersCommitted">The meters comitted</param>
        /// <param name="description">A brief description of the transaction</param>
        public MoneyBalanceReplyEventArgs(UUID transactionID, bool transactionSuccess, int balance, int metersCredit, int metersCommitted, string description)
        {
            this.m_TransactionID = transactionID;
            this.m_Success = transactionSuccess;
            this.m_Balance = balance;
            this.m_MetersCredit = metersCredit;
            this.m_MetersCommitted = metersCommitted;
            this.m_Description = description;
        }
    }

    // string message, TeleportStatus status, TeleportFlags flags
    public class TeleportEventArgs : EventArgs
    {
        private readonly string m_Message;
        private readonly TeleportStatus m_Status;
        private readonly TeleportFlags m_Flags;

        public string Message { get { return m_Message; } }
        public TeleportStatus Status { get { return m_Status; } }
        public TeleportFlags Flags { get { return m_Flags; } }

        public TeleportEventArgs(string message, TeleportStatus status, TeleportFlags flags)
        {
            this.m_Message = message;
            this.m_Status = status;
            this.m_Flags = flags;
        }
    }

    /// <summary>Data sent from the simulator containing information about your agent and active group information</summary>
    public class AgentDataReplyEventArgs : EventArgs
    {
        private readonly string m_FirstName;
        private readonly string m_LastName;
        private readonly UUID m_ActiveGroupID;
        private readonly string m_GroupTitle;
        private readonly GroupPowers m_GroupPowers;
        private readonly string m_GroupName;

        /// <summary>Get the agents first name</summary>
        public string FirstName { get { return m_FirstName; } }
        /// <summary>Get the agents last name</summary>
        public string LastName { get { return m_LastName; } }
        /// <summary>Get the active group ID of your agent</summary>
        public UUID ActiveGroupID { get { return m_ActiveGroupID; } }
        /// <summary>Get the active groups title of your agent</summary>
        public string GroupTitle { get { return m_GroupTitle; } }
        /// <summary>Get the combined group powers of your agent</summary>
        public GroupPowers GroupPowers { get { return m_GroupPowers; } }
        /// <summary>Get the active group name of your agent</summary>
        public string GroupName { get { return m_GroupName; } }

        /// <summary>
        /// Construct a new instance of the AgentDataReplyEventArgs object
        /// </summary>
        /// <param name="firstName">The agents first name</param>
        /// <param name="lastName">The agents last name</param>
        /// <param name="activeGroupID">The agents active group ID</param>
        /// <param name="groupTitle">The group title of the agents active group</param>
        /// <param name="groupPowers">The combined group powers the agent has in the active group</param>
        /// <param name="groupName">The name of the group the agent has currently active</param>
        public AgentDataReplyEventArgs(string firstName, string lastName, UUID activeGroupID,
            string groupTitle, GroupPowers groupPowers, string groupName)
        {
            this.m_FirstName = firstName;
            this.m_LastName = lastName;
            this.m_ActiveGroupID = activeGroupID;
            this.m_GroupTitle = groupTitle;
            this.m_GroupPowers = groupPowers;
            this.m_GroupName = groupName;
        }
    }

    /// <summary>Data sent by the simulator to indicate the active/changed animations
    /// applied to your agent</summary>
    public class AnimationsChangedEventArgs : EventArgs
    {
        private readonly InternalDictionary<UUID, int> m_Animations;

        /// <summary>Get the dictionary that contains the changed animations</summary>
        public InternalDictionary<UUID, int> Animations { get { return m_Animations; } }

        /// <summary>
        /// Construct a new instance of the AnimationsChangedEventArgs class
        /// </summary>
        /// <param name="agentAnimations">The dictionary that contains the changed animations</param>
        public AnimationsChangedEventArgs(InternalDictionary<UUID, int> agentAnimations)
        {
            this.m_Animations = agentAnimations;
        }

    }

    /// <summary>
    /// Data sent from a simulator indicating a collision with your agent
    /// </summary>
    public class MeanCollisionEventArgs : EventArgs
    {
        private readonly MeanCollisionType m_Type;
        private readonly UUID m_Aggressor;
        private readonly UUID m_Victim;
        private readonly float m_Magnitude;
        private readonly DateTime m_Time;

        /// <summary>Get the Type of collision</summary>
        public MeanCollisionType Type { get { return m_Type; } }
        /// <summary>Get the ID of the agent or object that collided with your agent</summary>
        public UUID Aggressor { get { return m_Aggressor; } }
        /// <summary>Get the ID of the agent that was attacked</summary>
        public UUID Victim { get { return m_Victim; } }
        /// <summary>A value indicating the strength of the collision</summary>
        public float Magnitude { get { return m_Magnitude; } }
        /// <summary>Get the time the collision occurred</summary>
        public DateTime Time { get { return m_Time; } }

        /// <summary>
        /// Construct a new instance of the MeanCollisionEventArgs class
        /// </summary>
        /// <param name="type">The type of collision that occurred</param>
        /// <param name="perp">The ID of the agent or object that perpetrated the agression</param>
        /// <param name="victim">The ID of the Victim</param>
        /// <param name="magnitude">The strength of the collision</param>
        /// <param name="time">The Time the collision occurred</param>
        public MeanCollisionEventArgs(MeanCollisionType type, UUID perp, UUID victim,
            float magnitude, DateTime time)
        {
            this.m_Type = type;
            this.m_Aggressor = perp;
            this.m_Victim = victim;
            this.m_Magnitude = magnitude;
            this.m_Time = time;
        }
    }

    /// <summary>Data sent to your agent when it crosses region boundaries</summary>
    public class RegionCrossedEventArgs : EventArgs
    {
        private readonly Simulator m_OldSimulator;
        private readonly Simulator m_NewSimulator;

        /// <summary>Get the simulator your agent just left</summary>
        public Simulator OldSimulator { get { return m_OldSimulator; } }
        /// <summary>Get the simulator your agent is now in</summary>
        public Simulator NewSimulator { get { return m_NewSimulator; } }

        /// <summary>
        /// Construct a new instance of the RegionCrossedEventArgs class
        /// </summary>
        /// <param name="oldSim">The simulator your agent just left</param>
        /// <param name="newSim">The simulator your agent is now in</param>
        public RegionCrossedEventArgs(Simulator oldSim, Simulator newSim)
        {
            this.m_OldSimulator = oldSim;
            this.m_NewSimulator = newSim;
        }
    }

    /// <summary>Data sent from the simulator when your agent joins a group chat session</summary>
    public class GroupChatJoinedEventArgs : EventArgs
    {
        private readonly UUID m_SessionID;
        private readonly string m_SessionName;
        private readonly UUID m_TmpSessionID;
        private readonly bool m_Success;

        /// <summary>Get the ID of the group chat session</summary>
        public UUID SessionID { get { return m_SessionID; } }
        /// <summary>Get the name of the session</summary>
        public string SessionName { get { return m_SessionName; } }
        /// <summary>Get the temporary session ID used for establishing new sessions</summary>
        public UUID TmpSessionID { get { return m_TmpSessionID; } }
        /// <summary>True if your agent successfully joined the session</summary>
        public bool Success { get { return m_Success; } }

        /// <summary>
        /// Construct a new instance of the GroupChatJoinedEventArgs class
        /// </summary>
        /// <param name="groupChatSessionID">The ID of the session</param>
        /// <param name="sessionName">The name of the session</param>
        /// <param name="tmpSessionID">A temporary session id used for establishing new sessions</param>
        /// <param name="success">True of your agent successfully joined the session</param>
        public GroupChatJoinedEventArgs(UUID groupChatSessionID, string sessionName, UUID tmpSessionID, bool success)
        {
            this.m_SessionID = groupChatSessionID;
            this.m_SessionName = sessionName;
            this.m_TmpSessionID = tmpSessionID;
            this.m_Success = success;
        }
    }

    /// <summary>Data sent by the simulator containing urgent messages</summary>
    public class AlertMessageEventArgs : EventArgs
    {
        private readonly string m_Message;

        /// <summary>Get the alert message</summary>
        public string Message { get { return m_Message; } }

        /// <summary>
        /// Construct a new instance of the AlertMessageEventArgs class
        /// </summary>
        /// <param name="message">The alert message</param>
        public AlertMessageEventArgs(string message)
        {
            this.m_Message = message;
        }
    }

    /// <summary>Data sent by a script requesting to take or release specified controls to your agent</summary>
    public class ScriptControlEventArgs : EventArgs
    {
        private readonly ScriptControlChange m_Controls;
        private readonly bool m_Pass;
        private readonly bool m_Take;

        /// <summary>Get the controls the script is attempting to take or release to the agent</summary>
        public ScriptControlChange Controls { get { return m_Controls; } }
        /// <summary>True if the script is passing controls back to the agent</summary>
        public bool Pass { get { return m_Pass; } }
        /// <summary>True if the script is requesting controls be released to the script</summary>
        public bool Take { get { return m_Take; } }

        /// <summary>
        /// Construct a new instance of the ScriptControlEventArgs class
        /// </summary>
        /// <param name="controls">The controls the script is attempting to take or release to the agent</param>
        /// <param name="pass">True if the script is passing controls back to the agent</param>
        /// <param name="take">True if the script is requesting controls be released to the script</param>
        public ScriptControlEventArgs(ScriptControlChange controls, bool pass, bool take)
        {
            m_Controls = controls;
            m_Pass = pass;
            m_Take = take;
        }
    }

    /// <summary>
    /// Data sent from the simulator to an agent to indicate its view limits
    /// </summary>
    public class CameraConstraintEventArgs : EventArgs
    {
        private readonly Vector4 m_CollidePlane;

        /// <summary>Get the collision plane</summary>
        public Vector4 CollidePlane { get { return m_CollidePlane; } }

        /// <summary>
        /// Construct a new instance of the CameraConstraintEventArgs class
        /// </summary>
        /// <param name="collidePlane">The collision plane</param>
        public CameraConstraintEventArgs(Vector4 collidePlane)
        {
            m_CollidePlane = collidePlane;
        }
    }

    /// <summary>
    /// Data containing script sensor requests which allow an agent to know the specific details
    /// of a primitive sending script sensor requests
    /// </summary>
    public class ScriptSensorReplyEventArgs : EventArgs
    {
        private readonly UUID m_RequestorID;
        private readonly UUID m_GroupID;
        private readonly string m_Name;
        private readonly UUID m_ObjectID;
        private readonly UUID m_OwnerID;
        private readonly Vector3 m_Position;
        private readonly float m_Range;
        private readonly Quaternion m_Rotation;
        private readonly ScriptSensorTypeFlags m_Type;
        private readonly Vector3 m_Velocity;

        /// <summary>Get the ID of the primitive sending the sensor</summary>
        public UUID RequestorID { get { return m_RequestorID; } }
        /// <summary>Get the ID of the group associated with the primitive</summary>
        public UUID GroupID { get { return m_GroupID; } }
        /// <summary>Get the name of the primitive sending the sensor</summary>
        public string Name { get { return m_Name; } }
        /// <summary>Get the ID of the primitive sending the sensor</summary>
        public UUID ObjectID { get { return m_ObjectID; } }
        /// <summary>Get the ID of the owner of the primitive sending the sensor</summary>
        public UUID OwnerID { get { return m_OwnerID; } }
        /// <summary>Get the position of the primitive sending the sensor</summary>
        public Vector3 Position { get { return m_Position; } }
        /// <summary>Get the range the primitive specified to scan</summary>
        public float Range { get { return m_Range; } }
        /// <summary>Get the rotation of the primitive sending the sensor</summary>
        public Quaternion Rotation { get { return m_Rotation; } }
        /// <summary>Get the type of sensor the primitive sent</summary>
        public ScriptSensorTypeFlags Type { get { return m_Type; } }
        /// <summary>Get the velocity of the primitive sending the sensor</summary>
        public Vector3 Velocity { get { return m_Velocity; } }

        /// <summary>
        /// Construct a new instance of the ScriptSensorReplyEventArgs
        /// </summary>
        /// <param name="requestorID">The ID of the primitive sending the sensor</param>
        /// <param name="groupID">The ID of the group associated with the primitive</param>
        /// <param name="name">The name of the primitive sending the sensor</param>
        /// <param name="objectID">The ID of the primitive sending the sensor</param>
        /// <param name="ownerID">The ID of the owner of the primitive sending the sensor</param>
        /// <param name="position">The position of the primitive sending the sensor</param>
        /// <param name="range">The range the primitive specified to scan</param>
        /// <param name="rotation">The rotation of the primitive sending the sensor</param>
        /// <param name="type">The type of sensor the primitive sent</param>
        /// <param name="velocity">The velocity of the primitive sending the sensor</param>
        public ScriptSensorReplyEventArgs(UUID requestorID, UUID groupID, string name,
            UUID objectID, UUID ownerID, Vector3 position, float range, Quaternion rotation,
            ScriptSensorTypeFlags type, Vector3 velocity)
        {
            this.m_RequestorID = requestorID;
            this.m_GroupID = groupID;
            this.m_Name = name;
            this.m_ObjectID = objectID;
            this.m_OwnerID = ownerID;
            this.m_Position = position;
            this.m_Range = range;
            this.m_Rotation = rotation;
            this.m_Type = type;
            this.m_Velocity = velocity;
        }
    }

    /// <summary>Contains the response data returned from the simulator in response to a <see cref="RequestSit"/></summary>
    public class AvatarSitResponseEventArgs : EventArgs
    {
        private readonly UUID m_ObjectID;
        private readonly bool m_Autopilot;
        private readonly Vector3 m_CameraAtOffset;
        private readonly Vector3 m_CameraEyeOffset;
        private readonly bool m_ForceMouselook;
        private readonly Vector3 m_SitPosition;
        private readonly Quaternion m_SitRotation;

        /// <summary>Get the ID of the primitive the agent will be sitting on</summary>
        public UUID ObjectID { get { return m_ObjectID; } }
        /// <summary>True if the simulator Autopilot functions were involved</summary>
        public bool Autopilot { get { return m_Autopilot; } }
        /// <summary>Get the camera offset of the agent when seated</summary>
        public Vector3 CameraAtOffset { get { return m_CameraAtOffset; } }
        /// <summary>Get the camera eye offset of the agent when seated</summary>
        public Vector3 CameraEyeOffset { get { return m_CameraEyeOffset; } }
        /// <summary>True of the agent will be in mouselook mode when seated</summary>
        public bool ForceMouselook { get { return m_ForceMouselook; } }
        /// <summary>Get the position of the agent when seated</summary>
        public Vector3 SitPosition { get { return m_SitPosition; } }
        /// <summary>Get the rotation of the agent when seated</summary>
        public Quaternion SitRotation { get { return m_SitRotation; } }

        /// <summary>Construct a new instance of the AvatarSitResponseEventArgs object</summary>
        public AvatarSitResponseEventArgs(UUID objectID, bool autoPilot, Vector3 cameraAtOffset,
            Vector3 cameraEyeOffset, bool forceMouselook, Vector3 sitPosition, Quaternion sitRotation)
        {
            this.m_ObjectID = objectID;
            this.m_Autopilot = autoPilot;
            this.m_CameraAtOffset = cameraAtOffset;
            this.m_CameraEyeOffset = cameraEyeOffset;
            this.m_ForceMouselook = forceMouselook;
            this.m_SitPosition = sitPosition;
            this.m_SitRotation = sitRotation;
        }
    }

    /// <summary>Data sent when an agent joins a chat session your agent is currently participating in</summary>
    public class ChatSessionMemberAddedEventArgs : EventArgs
    {
        private readonly UUID m_SessionID;
        private readonly UUID m_AgentID;

        /// <summary>Get the ID of the chat session</summary>
        public UUID SessionID { get { return m_SessionID; } }
        /// <summary>Get the ID of the agent that joined</summary>
        public UUID AgentID { get { return m_AgentID; } }

        /// <summary>
        /// Construct a new instance of the ChatSessionMemberAddedEventArgs object
        /// </summary>
        /// <param name="sessionID">The ID of the chat session</param>
        /// <param name="agentID">The ID of the agent joining</param>
        public ChatSessionMemberAddedEventArgs(UUID sessionID, UUID agentID)
        {
            this.m_SessionID = sessionID;
            this.m_AgentID = agentID;
        }
    }

    /// <summary>Data sent when an agent exits a chat session your agent is currently participating in</summary>
    public class ChatSessionMemberLeftEventArgs : EventArgs
    {
        private readonly UUID m_SessionID;
        private readonly UUID m_AgentID;

        /// <summary>Get the ID of the chat session</summary>
        public UUID SessionID { get { return m_SessionID; } }
        /// <summary>Get the ID of the agent that left</summary>
        public UUID AgentID { get { return m_AgentID; } }

        /// <summary>
        /// Construct a new instance of the ChatSessionMemberLeftEventArgs object
        /// </summary>
        /// <param name="sessionID">The ID of the chat session</param>
        /// <param name="agentID">The ID of the Agent that left</param>
        public ChatSessionMemberLeftEventArgs(UUID sessionID, UUID agentID)
        {
            this.m_SessionID = sessionID;
            this.m_AgentID = agentID;
        }
    }
    #endregion
}
