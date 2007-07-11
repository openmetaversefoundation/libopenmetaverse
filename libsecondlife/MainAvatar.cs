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
using System.Timers;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using libsecondlife.Packets;

namespace libsecondlife
{
    /// <summary>
    /// Class to hold Client Avatar's data
    /// </summary>
    public partial class MainAvatar
    {
        #region Enums

        /// <summary>
        /// Used to specify movement actions for your agent
        /// </summary>
        [Flags]
        public enum ControlFlags
        {
            /// <summary>Empty flag</summary>
            NONE = 0,
            /// <summary>Move Forward (SL Keybinding: W/Up Arrow)</summary>
            AGENT_CONTROL_AT_POS = 0x1 << CONTROL_AT_POS_INDEX,
            /// <summary>Move Backward (SL Keybinding: S/Down Arrow)</summary>
            AGENT_CONTROL_AT_NEG = 0x1 << CONTROL_AT_NEG_INDEX,
            /// <summary>Move Left (SL Keybinding: Shift-(A/Left Arrow))</summary>
            AGENT_CONTROL_LEFT_POS = 0x1 << CONTROL_LEFT_POS_INDEX,
            /// <summary>Move Right (SL Keybinding: Shift-(D/Right Arrow))</summary>
            AGENT_CONTROL_LEFT_NEG = 0x1 << CONTROL_LEFT_NEG_INDEX,
            /// <summary>Not Flying: Jump/Flying: Move Up (SL Keybinding: E)</summary>
            AGENT_CONTROL_UP_POS = 0x1 << CONTROL_UP_POS_INDEX,
            /// <summary>Not Flying: Croutch/Flying: Move Down (SL Keybinding: C)</summary>
            AGENT_CONTROL_UP_NEG = 0x1 << CONTROL_UP_NEG_INDEX,
            /// <summary>Unused</summary>
            AGENT_CONTROL_PITCH_POS = 0x1 << CONTROL_PITCH_POS_INDEX,
            /// <summary>Unused</summary>
            AGENT_CONTROL_PITCH_NEG = 0x1 << CONTROL_PITCH_NEG_INDEX,
            /// <summary>Unused</summary>
            AGENT_CONTROL_YAW_POS = 0x1 << CONTROL_YAW_POS_INDEX,
            /// <summary>Unused</summary>
            AGENT_CONTROL_YAW_NEG = 0x1 << CONTROL_YAW_NEG_INDEX,
            /// <summary>ORed with AGENT_CONTROL_AT_* if the keyboard is being used</summary>
            AGENT_CONTROL_FAST_AT = 0x1 << CONTROL_FAST_AT_INDEX,
            /// <summary>ORed with AGENT_CONTROL_LEFT_* if the keyboard is being used</summary>
            AGENT_CONTROL_FAST_LEFT = 0x1 << CONTROL_FAST_LEFT_INDEX,
            /// <summary>ORed with AGENT_CONTROL_UP_* if the keyboard is being used</summary>
            AGENT_CONTROL_FAST_UP = 0x1 << CONTROL_FAST_UP_INDEX,
            /// <summary>Fly</summary>
            AGENT_CONTROL_FLY = 0x1 << CONTROL_FLY_INDEX,
            /// <summary></summary>
            AGENT_CONTROL_STOP = 0x1 << CONTROL_STOP_INDEX,
            /// <summary>Finish our current animation</summary>
            AGENT_CONTROL_FINISH_ANIM = 0x1 << CONTROL_FINISH_ANIM_INDEX,
            /// <summary>Stand up from the ground or a prim seat</summary>
            AGENT_CONTROL_STAND_UP = 0x1 << CONTROL_STAND_UP_INDEX,
            /// <summary>Sit on the ground at our current location</summary>
            AGENT_CONTROL_SIT_ON_GROUND = 0x1 << CONTROL_SIT_ON_GROUND_INDEX,
            /// <summary>Whether mouselook is currently enabled</summary>
            AGENT_CONTROL_MOUSELOOK = 0x1 << CONTROL_MOUSELOOK_INDEX,
            /// <summary>Legacy, used if a key was pressed for less than a certain amount of time</summary>
            AGENT_CONTROL_NUDGE_AT_POS = 0x1 << CONTROL_NUDGE_AT_POS_INDEX,
            /// <summary>Legacy, used if a key was pressed for less than a certain amount of time</summary>
            AGENT_CONTROL_NUDGE_AT_NEG = 0x1 << CONTROL_NUDGE_AT_NEG_INDEX,
            /// <summary>Legacy, used if a key was pressed for less than a certain amount of time</summary>
            AGENT_CONTROL_NUDGE_LEFT_POS = 0x1 << CONTROL_NUDGE_LEFT_POS_INDEX,
            /// <summary>Legacy, used if a key was pressed for less than a certain amount of time</summary>
            AGENT_CONTROL_NUDGE_LEFT_NEG = 0x1 << CONTROL_NUDGE_LEFT_NEG_INDEX,
            /// <summary>Legacy, used if a key was pressed for less than a certain amount of time</summary>
            AGENT_CONTROL_NUDGE_UP_POS = 0x1 << CONTROL_NUDGE_UP_POS_INDEX,
            /// <summary>Legacy, used if a key was pressed for less than a certain amount of time</summary>
            AGENT_CONTROL_NUDGE_UP_NEG = 0x1 << CONTROL_NUDGE_UP_NEG_INDEX,
            /// <summary></summary>
            AGENT_CONTROL_TURN_LEFT = 0x1 << CONTROL_TURN_LEFT_INDEX,
            /// <summary></summary>
            AGENT_CONTROL_TURN_RIGHT = 0x1 << CONTROL_TURN_RIGHT_INDEX,
            /// <summary>Set when the avatar is idled or set to away. Note that the away animation is 
            /// activated separately from setting this flag</summary>
            AGENT_CONTROL_AWAY = 0x1 << CONTROL_AWAY_INDEX,
            /// <summary></summary>
            AGENT_CONTROL_LBUTTON_DOWN = 0x1 << CONTROL_LBUTTON_DOWN_INDEX,
            /// <summary></summary>
            AGENT_CONTROL_LBUTTON_UP = 0x1 << CONTROL_LBUTTON_UP_INDEX,
            /// <summary></summary>
            AGENT_CONTROL_ML_LBUTTON_DOWN = 0x1 << CONTROL_ML_LBUTTON_DOWN_INDEX,
            /// <summary></summary>
            AGENT_CONTROL_ML_LBUTTON_UP = 0x1 << CONTROL_ML_LBUTTON_UP_INDEX
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
        /// 
        /// </summary>
        [Flags]
        public enum ScriptPermission : int
        {
            /// <summary>Placeholder for empty values, shouldn't ever see this</summary>
            None = 0,
            /// <summary>Script wants to take money from you</summary>
            Debit = 1 << 1,
            /// <summary></summary>
            TakeControls = 1 << 2,
            /// <summary></summary>
            RemapControls = 1 << 3,
            /// <summary>Script wants to trigger avatar animations</summary>
            TriggerAnimation = 1 << 4,
            /// <summary></summary>
            Attach = 1 << 5,
            /// <summary></summary>
            ReleaseOwnership = 1 << 6,
            /// <summary></summary>
            ChangeLinks = 1 << 7,
            /// <summary></summary>
            ChangeJoints = 1 << 8,
            /// <summary></summary>
            ChangePermissions = 1 << 9,
            /// <summary></summary>
            TrackCamera = 1 << 10,
            /// <summary>Script wants to control your camera</summary>
            ControlCamera = 1 << 11
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
        /// Special commands used in Instant Messages
        /// </summary>
        public enum InstantMessageDialog : byte
        {
            /// <summary>Indicates a regular IM from another agent</summary>
            MessageFromAgent = 0,
			/// <summary>Simple notification box with an OK button</summary>
			MessageBox = 1,
            /// <summary>Used to show a countdown notification with an OK
            /// button, deprecated now</summary>
            [Obsolete]
            MessageBoxCountdown = 2,
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
            /// <summary>A message to everyone in the agent's group, no longer
            /// used</summary>
            [Obsolete]
            DeprecatedGroupMessage = 8,
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
			/// <summary>sent an IM to a busy user, this is the auto response</summary>
			BusyAutoResponse = 20,
			/// <summary>Shows the message in the console and chat history</summary>
			ConsoleAndChatHistory = 21,
			/// <summary>IM Types used for luring your friends</summary>
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
            /// <summary>Notification of a new group election, this is 
            /// deprecated</summary>
            [Obsolete]
            DeprecatedGroupElection = 27,
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
            /// <summary>Say chat (10/20m radius) - The official viewer will 
            /// print "[4:15] You say, hey" instead of "[4:15] You: hey"</summary>
            [Obsolete]
            Say = 3,
            /// <summary>Event message when an Avatar has begun to type</summary>
            StartTyping = 4,
            /// <summary>Event message when an Avatar has stopped typing</summary>
            StopTyping = 5,
            /// <summary>Unknown</summary>
            Debug = 6
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
            /// <summary>Place floating text above an object</summary>
		    Text = 0,
            /// <summary>Unknown, probably places an icon above an object</summary>
		    Icon,
            /// <summary>Unknown</summary>
		    Connector,
            /// <summary>Unknown</summary>
		    FlexibleObject,
            /// <summary>Unknown</summary>
		    AnimalControls,
            /// <summary>Unknown</summary>
		    AnimationObject,
            /// <summary>Unknown</summary>
		    Cloth,
            /// <summary>Project a beam from a source to a destination, such as
            /// the one used when editing an object</summary>
		    Beam,
            /// <summary>Not implemented yet</summary>
		    Glow,
            /// <summary>Unknown</summary>
		    Point,
            /// <summary>Unknown</summary>
		    Trail,
            /// <summary>Create a swirl of particles around an object</summary>
		    Sphere,
            /// <summary>Unknown</summary>
		    Spiral,
            /// <summary>Unknown</summary>
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
        /// 
        /// </summary>
        [Flags]
        public enum TeleportFlags : uint
        {
            /// <summary></summary>
            Default         =      0,
            /// <summary></summary>
            SetHomeToTarget = 1 << 0,
            /// <summary></summary>
            SetLastToTarget = 1 << 1,
            /// <summary></summary>
            ViaLure         = 1 << 2,
            /// <summary></summary>
            ViaLandmark     = 1 << 3,
            /// <summary></summary>
            ViaLocation     = 1 << 4,
            /// <summary></summary>
            ViaHome         = 1 << 5,
            /// <summary></summary>
            ViaTelehub      = 1 << 6,
            /// <summary></summary>
            ViaLogin        = 1 << 7,
            /// <summary></summary>
            ViaGodlikeLure  = 1 << 8,
            /// <summary></summary>
            Godlike         = 1 << 9,
            /// <summary></summary>
            NineOneOne      = 1 << 10,
            /// <summary></summary>
            DisableCancel   = 1 << 11,
            /// <summary></summary>
            ViaRegionID     = 1 << 12,
            /// <summary></summary>
            IsFlying        = 1 << 13
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

        #endregion

        #region Callbacks & Events
        /// <summary>
        /// Triggered on incoming chat messages
        /// </summary>
        /// <param name="message">Text of chat message</param>
        /// <param name="audible">Audible level of this chat message</param>
        /// <param name="type">Type of chat (whisper, shout, status, etc.)</param>
        /// <param name="sourceType">Source of the chat message</param>
        /// <param name="fromName">Name of the sending object</param>
        /// <param name="id"></param>
        /// <param name="ownerid"></param>
        /// <param name="position"></param>
        public delegate void ChatCallback(string message, ChatAudibleLevel audible, ChatType type, 
            ChatSourceType sourceType, string fromName, LLUUID id, LLUUID ownerid, LLVector3 position);

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
        public delegate void ScriptDialogCallback(string message, string objectName, LLUUID imageID,
            LLUUID objectID, string firstName, string lastName, int chatChannel, List<string> buttons);

        /// <summary>
        /// Triggered when a script asks for permissions
        /// </summary>
        /// <param name="taskID">Task ID of the script requesting permissions</param>
        /// <param name="itemID">ID of the object containing the script</param>
        /// <param name="objectName">Name of the object containing the script</param>
        /// <param name="objectOwner">Name of the object's owner</param>
        /// <param name="questions">Bitwise value representing the requested permissions</param>
        public delegate void ScriptQuestionCallback(LLUUID taskID, LLUUID itemID, string objectName, string objectOwner, ScriptPermission questions);

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
        /// <param name="metersCredit"></param>
        /// <param name="metersCommitted"></param>
        /// <param name="description"></param>
        public delegate void MoneyBalanceReplyCallback(LLUUID transactionID, bool transactionSuccess, int balance, int metersCredit, int metersCommitted, string description);

        /// <summary>
        /// Triggered on incoming instant messages
        /// </summary>
        /// <param name="fromAgentID">Key of sender</param>
        /// <param name="fromAgentName">Name of sender</param>
        /// <param name="toAgentID">Key of destination Avatar</param>
        /// <param name="parentEstateID">ID of originating Estate</param>
        /// <param name="regionID">Key of originating Region</param>
        /// <param name="position">Coordinates in originating Region</param>
        /// <param name="dialog"></param>
        /// <param name="groupIM">Group IM session toggle</param>
        /// <param name="imSessionID">Key of IM Session</param>
        /// <param name="timestamp">Timestamp of message</param>
        /// <param name="message">Text of message</param>
        /// <param name="offline">Enum of whether this message is held for 
        /// offline avatars</param>
        /// <param name="binaryBucket"></param>
        public delegate void InstantMessageCallback(LLUUID fromAgentID, string fromAgentName,
            LLUUID toAgentID, uint parentEstateID, LLUUID regionID, LLVector3 position,
            InstantMessageDialog dialog, bool groupIM, LLUUID imSessionID, DateTime timestamp, string message,
            InstantMessageOnline offline, byte[] binaryBucket);

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
        public delegate void JoinGroupCallback(LLUUID groupID, bool success);

        /// <summary>
        /// Reply to a request to leave a group, informs whether it was successful or not
        /// </summary>
        /// <param name="groupID">The group we attempted to leave</param>
        /// <param name="success">Whether we left the group or not</param>
        public delegate void LeaveGroupCallback(LLUUID groupID, bool success);

        /// <summary>
        /// Informs the avatar that it is no longer a member of a group
        /// </summary>
        /// <param name="groupID">The group we are no longer a member of</param>
        public delegate void GroupDroppedCallback(LLUUID groupID);

        /// <summary>
        /// Informs the avatar that the active group has changed
        /// </summary>
        /// <param name="groupID">The group that is now the active group</param>
        public delegate void ActiveGroupChangedCallback(LLUUID groupID);


        /// <summary>Callback for incoming chat packets</summary>
        public event ChatCallback OnChat;
        /// <summary>Callback for pop-up dialogs from scripts</summary>
        public event ScriptDialogCallback OnScriptDialog;
        /// <summary>Callback for pop-up dialogs regarding permissions</summary>
        public event ScriptQuestionCallback OnScriptQuestion;
        /// <summary>Callback for incoming IMs</summary>
        public event InstantMessageCallback OnInstantMessage;
        /// <summary>Callback for Teleport request update</summary>
        public event TeleportCallback OnTeleport;
        /// <summary>Callback for incoming change in L$ balance</summary>
        public event BalanceCallback OnBalanceUpdated;
        /// <summary>Callback for incoming Money Balance Replies</summary>
        public event MoneyBalanceReplyCallback OnMoneyBalanceReplyReceived;
        /// <summary>Callback reply for an attempt to join a group</summary>
        public event JoinGroupCallback OnJoinGroup;
        /// <summary>Callback reply for an attempt to leave a group</summary>
        public event LeaveGroupCallback OnLeaveGroup;
        /// <summary>Callback for informing the avatar that it is no longer a member of a group</summary>
        public event GroupDroppedCallback OnGroupDropped;
        /// <summary>Callback reply for the current group changing</summary>
        public event ActiveGroupChangedCallback OnActiveGroupChanged;

        #endregion

        #region Public Members

        /// <summary>Your (client) avatar UUID</summary>
        public LLUUID ID = LLUUID.Zero;
        /// <summary>Your (client) avatar ID, local to the current region/sim</summary>
        public uint LocalID = 0;
        /// <summary>Avatar First Name (i.e. Philip)</summary>
        public string FirstName = String.Empty;
        /// <summary>Avatar Last Name (i.e. Linden)</summary>
        public string LastName = String.Empty;
        /// <summary>Where the avatar started at login. Can be "last", "home" 
        /// or a login URI</summary>
        public string StartLocation = String.Empty;
        /// <summary>The access level of this agent, usually M or PG</summary>
        public string AgentAccess = String.Empty;
        /// <summary>Positive and negative ratings</summary>
        /// <remarks>This information is read-only and any changes will not be
        /// reflected on the server</remarks>
        public Avatar.Statistics ProfileStatistics = new Avatar.Statistics();
        /// <summary>Avatar properties including about text, profile URL, image IDs and 
        /// publishing settings</summary>
        /// <remarks>If you change fields in this struct, the changes will not
        /// be reflected on the server until you call SetAvatarInformation</remarks>
        public Avatar.AvatarProperties ProfileProperties = new Avatar.AvatarProperties();
        /// <summary>Avatar interests including spoken languages, skills, and "want to"
        /// choices</summary>
        /// <remarks>If you change fields in this struct, the changes will not
        /// be reflected on the server until you call SetAvatarInformation</remarks>
        public Avatar.Interests ProfileInterests = new Avatar.Interests();
        /// <summary>Current position of avatar</summary>
        public LLVector3 Position = LLVector3.Zero;
        /// <summary>Current rotation of avatar</summary>
        public LLQuaternion Rotation = LLQuaternion.Identity;
        /// <summary></summary>
        public LLVector4 CollisionPlane = LLVector4.Zero;
        /// <summary></summary>
        public LLVector3 Velocity = LLVector3.Zero;
        /// <summary></summary>
        public LLVector3 Acceleration = LLVector3.Zero;
        /// <summary></summary>
        public LLVector3 AngularVelocity = LLVector3.Zero;
        /// <summary>The point the avatar is currently looking at
        /// (may not stay updated)</summary>
        public LLVector3 LookAt = LLVector3.Zero;
        /// <summary>Position avatar client will goto when login to 'home' or during
        /// teleport request to 'home' region.</summary>
        public LLVector3 HomePosition = LLVector3.Zero;
        /// <summary>LookAt point saved/restored with HomePosition</summary>
        public LLVector3 HomeLookAt = LLVector3.Zero;
        /// <summary>Used for camera and control key state tracking</summary>
        public MainAvatarStatus Status;
        /// <summary>The UUID of your root inventory folder</summary>
        public LLUUID InventoryRootFolderUUID = LLUUID.Zero;

        /// <summary>Gets the health of the agent</summary>
        public float Health { get { return health; } }
        /// <summary>Gets the current balance of the agent</summary>
        public int Balance { get { return balance; } }
        /// <summary>Gets the local ID of the prim the avatar is sitting on,
        /// zero if the avatar is not currently sitting</summary>
        public uint SittingOn { get { return sittingOn; } }
		/// <summary>Gets the UUID of the active group.</summary>
		public LLUUID ActiveGroup { get { return activeGroup; } }
        /// <summary>Current status message for teleporting</summary>
        public string TeleportMessage { get { return teleportMessage; } }

        #endregion Public Members

        internal string teleportMessage = String.Empty;
        internal uint sittingOn = 0;
        internal DateTime lastInterpolation;

        private SecondLife Client;
        private TeleportStatus TeleportStat = TeleportStatus.None;
        private ManualResetEvent TeleportEvent = new ManualResetEvent(false);
        private uint HeightWidthGenCounter = 0;
        private float health = 0.0f;
        private int balance = 0;
		private LLUUID activeGroup = LLUUID.Zero;

        #region AgentUpdate Constants

        private const int CONTROL_AT_POS_INDEX = 0;
        private const int CONTROL_AT_NEG_INDEX = 1;
        private const int CONTROL_LEFT_POS_INDEX = 2;
        private const int CONTROL_LEFT_NEG_INDEX = 3;
        private const int CONTROL_UP_POS_INDEX = 4;
        private const int CONTROL_UP_NEG_INDEX = 5;
        private const int CONTROL_PITCH_POS_INDEX = 6;
        private const int CONTROL_PITCH_NEG_INDEX = 7;
        private const int CONTROL_YAW_POS_INDEX = 8;
        private const int CONTROL_YAW_NEG_INDEX = 9;
        private const int CONTROL_FAST_AT_INDEX = 10;
        private const int CONTROL_FAST_LEFT_INDEX = 11;
        private const int CONTROL_FAST_UP_INDEX = 12;
        private const int CONTROL_FLY_INDEX = 13;
        private const int CONTROL_STOP_INDEX = 14;
        private const int CONTROL_FINISH_ANIM_INDEX = 15;
        private const int CONTROL_STAND_UP_INDEX = 16;
        private const int CONTROL_SIT_ON_GROUND_INDEX = 17;
        private const int CONTROL_MOUSELOOK_INDEX = 18;
        private const int CONTROL_NUDGE_AT_POS_INDEX = 19;
        private const int CONTROL_NUDGE_AT_NEG_INDEX = 20;
        private const int CONTROL_NUDGE_LEFT_POS_INDEX = 21;
        private const int CONTROL_NUDGE_LEFT_NEG_INDEX = 22;
        private const int CONTROL_NUDGE_UP_POS_INDEX = 23;
        private const int CONTROL_NUDGE_UP_NEG_INDEX = 24;
        private const int CONTROL_TURN_LEFT_INDEX = 25;
        private const int CONTROL_TURN_RIGHT_INDEX = 26;
        private const int CONTROL_AWAY_INDEX = 27;
        private const int CONTROL_LBUTTON_DOWN_INDEX = 28;
        private const int CONTROL_LBUTTON_UP_INDEX = 29;
        private const int CONTROL_ML_LBUTTON_DOWN_INDEX = 30;
        private const int CONTROL_ML_LBUTTON_UP_INDEX = 31;
        private const int TOTAL_CONTROLS = 32;

        #endregion AgentUpdate Constants


        /// <summary>
        /// Constructor, setup callbacks for packets related to our avatar
        /// </summary>
        /// <param name="client"></param>
        public MainAvatar(SecondLife client)
        {
            Client = client;
            Status = new MainAvatarStatus(Client);
            NetworkManager.PacketCallback callback;

            // Teleport callbacks
            callback = new NetworkManager.PacketCallback(TeleportHandler);
            Client.Network.RegisterCallback(PacketType.TeleportStart, callback);
            Client.Network.RegisterCallback(PacketType.TeleportProgress, callback);
            Client.Network.RegisterCallback(PacketType.TeleportFailed, callback);
            Client.Network.RegisterCallback(PacketType.TeleportFinish, callback);
            Client.Network.RegisterCallback(PacketType.TeleportCancel, callback);
            Client.Network.RegisterCallback(PacketType.TeleportLocal, callback);

            // Instant Message callback
            Client.Network.RegisterCallback(PacketType.ImprovedInstantMessage, new NetworkManager.PacketCallback(InstantMessageHandler));

            // Chat callback
            Client.Network.RegisterCallback(PacketType.ChatFromSimulator, new NetworkManager.PacketCallback(ChatHandler));

            // Script dialog callback
            Client.Network.RegisterCallback(PacketType.ScriptDialog, new NetworkManager.PacketCallback(ScriptDialogHandler));

            // Script question callback
            Client.Network.RegisterCallback(PacketType.ScriptQuestion, new NetworkManager.PacketCallback(ScriptQuestionHandler));

            // Movement complete callback
            Client.Network.RegisterCallback(PacketType.AgentMovementComplete, new NetworkManager.PacketCallback(MovementCompleteHandler));

            // Health callback
            Client.Network.RegisterCallback(PacketType.HealthMessage, new NetworkManager.PacketCallback(HealthHandler));

            // Money callback
            Client.Network.RegisterCallback(PacketType.MoneyBalanceReply, new NetworkManager.PacketCallback(BalanceHandler));

            // Group callbacks
            Client.Network.RegisterCallback(PacketType.JoinGroupReply, new NetworkManager.PacketCallback(JoinGroupHandler));
            Client.Network.RegisterCallback(PacketType.LeaveGroupReply, new NetworkManager.PacketCallback(LeaveGroupHandler));
            Client.Network.RegisterCallback(PacketType.AgentDropGroup, new NetworkManager.PacketCallback(DropGroupHandler));
			
			//Agent Update Callback
			Client.Network.RegisterCallback(PacketType.AgentDataUpdate, new NetworkManager.PacketCallback(AgentDataUpdateHandler));

	        // Event queue callback (used for Caps teleports currently)
	        Client.Network.RegisterEventCallback(new Caps.EventQueueCallback(EventQueueHandler));
        }

        /// <summary>
        /// Send an Instant Message
        /// </summary>
        /// <param name="target">Target of the Instant Message</param>
        /// <param name="message">Text message being sent</param>
        public void InstantMessage(LLUUID target, string message)
        {
            InstantMessage(Client.ToString(), target, message, LLUUID.Random(),
                InstantMessageDialog.MessageFromAgent, InstantMessageOnline.Offline, this.Position,
                LLUUID.Zero, new byte[0]);
        }

        /// <summary>
        /// Send an Instant Message
        /// </summary>
        /// <param name="target">Target of the Instant Message</param>
        /// <param name="message">Text message being sent</param>
        /// <param name="imSessionID">IM session ID (to differentiate between IM windows)</param>
        public void InstantMessage(LLUUID target, string message, LLUUID imSessionID)
        {
            InstantMessage(Client.ToString(), target, message, imSessionID,
                InstantMessageDialog.MessageFromAgent, InstantMessageOnline.Offline, this.Position,
                LLUUID.Zero, new byte[0]);
        }

        /// <summary>
        /// Send an Instant Message
        /// </summary>
        /// <param name="fromName">The name this IM will show up as being from</param>
        /// <param name="target">Key of Avatar</param>
        /// <param name="message">Text message being sent</param>
        /// <param name="imSessionID">IM session ID (to differentiate between IM windows)</param>
        /// <param name="conferenceIDs"></param>
		public void InstantMessage(string fromName, LLUUID target, string message, LLUUID imSessionID, 
            LLUUID[] conferenceIDs)
		{
            byte[] binaryBucket;

            if (conferenceIDs != null && conferenceIDs.Length > 0)
            {
                binaryBucket = new byte[16 * conferenceIDs.Length];
                for (int i = 0; i < conferenceIDs.Length; ++i)
                    Buffer.BlockCopy(conferenceIDs[i].Data, 0, binaryBucket, i * 16, 16);
            }
            else
            {
                binaryBucket = new byte[0];
            }

			InstantMessage(fromName, target, message, imSessionID, InstantMessageDialog.MessageFromAgent, 
                InstantMessageOnline.Offline, LLVector3.Zero, LLUUID.Zero, binaryBucket);
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
        /// <param name="position"></param>
        /// <param name="regionID"></param>
        /// <param name="binaryBucket">Packed binary data that is specific to
        /// the dialog type</param>
        public void InstantMessage(string fromName, LLUUID target, string message, LLUUID imSessionID, 
            InstantMessageDialog dialog, InstantMessageOnline offline, LLVector3 position, LLUUID regionID, 
            byte[] binaryBucket)
        {
            ImprovedInstantMessagePacket im = new ImprovedInstantMessagePacket();

            im.AgentData.AgentID = Client.Network.AgentID;
            im.AgentData.SessionID = Client.Network.SessionID;

            im.MessageBlock.Dialog = (byte)dialog;
            im.MessageBlock.FromAgentName = Helpers.StringToField(fromName);
            im.MessageBlock.FromGroup = false;
            im.MessageBlock.ID = imSessionID;
            im.MessageBlock.Message = Helpers.StringToField(message);
            im.MessageBlock.Offline = (byte)offline;
            im.MessageBlock.ToAgentID = target;

            if (binaryBucket != null)
                im.MessageBlock.BinaryBucket = binaryBucket;
            else
                im.MessageBlock.BinaryBucket = new byte[0];

            // These fields are mandatory, even if we don't have valid values for them
            im.MessageBlock.Position = LLVector3.Zero;
            //TODO: Allow region id to be correctly set by caller or fetched from Client.*
            im.MessageBlock.RegionID = regionID;

            // Send the message
            Client.Network.SendPacket(im);
        }

        /// <summary>
        /// Send an Instant Message to a group
        /// </summary>
        /// <param name="groupUUID">Key of Group</param>
        /// <param name="message">Text Message being sent.</param>
        public void InstantMessageGroup(LLUUID groupUUID, string message)
        {
            InstantMessageGroup(Client.ToString(), groupUUID, message);
        }

        /// <summary>
        /// Send an Instant Message to a group
        /// </summary>
        /// <param name="fromName">The name this IM will show up as being from</param>
        /// <param name="groupUUID">Key of the group</param>
        /// <param name="message">Text message being sent</param>
        /// <remarks>This does not appear to function with groups the agent is not in</remarks>
        public void InstantMessageGroup(string fromName, LLUUID groupUUID, string message)
        {
            ImprovedInstantMessagePacket im = new ImprovedInstantMessagePacket();

            im.AgentData.AgentID = Client.Network.AgentID;
            im.AgentData.SessionID = Client.Network.SessionID;
            im.MessageBlock.Dialog = (byte)MainAvatar.InstantMessageDialog.SessionSend;
            im.MessageBlock.FromAgentName = Helpers.StringToField(fromName);
            im.MessageBlock.FromGroup = false;
            im.MessageBlock.Message = Helpers.StringToField(message);
            im.MessageBlock.Offline = 0;
            im.MessageBlock.ID = groupUUID;
            im.MessageBlock.ToAgentID = groupUUID;
            im.MessageBlock.BinaryBucket = new byte[0];
            im.MessageBlock.Position = LLVector3.Zero;
            im.MessageBlock.RegionID = LLUUID.Zero;

            // Send the message
            Client.Network.SendPacket(im);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceAvatar"></param>
        /// <param name="targetObject"></param>
        /// <param name="globalOffset"></param>
        /// <param name="type"></param>
        public void PointAtEffect(LLUUID sourceAvatar, LLUUID targetObject, LLVector3d globalOffset, PointAtType type)
        {
            ViewerEffectPacket effect = new ViewerEffectPacket();

            effect.AgentData.AgentID = Client.Network.AgentID;
            effect.AgentData.SessionID = Client.Network.SessionID;

            effect.Effect = new ViewerEffectPacket.EffectBlock[1];
            effect.Effect[0] = new ViewerEffectPacket.EffectBlock();
            effect.Effect[0].AgentID = Client.Network.AgentID;
            effect.Effect[0].Color = LLColor.Black.GetBytes();
            effect.Effect[0].Duration = (type == PointAtType.Clear) ? 0.0f : Single.MaxValue / 4.0f;
            effect.Effect[0].ID = LLUUID.Random();
            effect.Effect[0].Type = (byte)EffectType.PointAt;

            byte[] typeData = new byte[57];
            if (sourceAvatar != null)
                Buffer.BlockCopy(sourceAvatar.GetBytes(), 0, typeData, 0, 16);
            if (targetObject != null)
                Buffer.BlockCopy(targetObject.GetBytes(), 0, typeData, 16, 16);
            Buffer.BlockCopy(globalOffset.GetBytes(), 0, typeData, 32, 24);
            typeData[56] = (byte)type;

            effect.Effect[0].TypeData = typeData;

            Client.Network.SendPacket(effect);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceAvatar"></param>
        /// <param name="targetObject"></param>
        /// <param name="globalOffset"></param>
        /// <param name="type"></param>
        public void LookAtEffect(LLUUID sourceAvatar, LLUUID targetObject, LLVector3d globalOffset, LookAtType type)
        {
            ViewerEffectPacket effect = new ViewerEffectPacket();

            effect.AgentData.AgentID = Client.Network.AgentID;
            effect.AgentData.SessionID = Client.Network.SessionID;

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
                case LookAtType.Conversation:
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
            effect.Effect[0].AgentID = Client.Network.AgentID;
            effect.Effect[0].Color = LLColor.Black.GetBytes();
            effect.Effect[0].Duration = duration;
            effect.Effect[0].ID = LLUUID.Random();
            effect.Effect[0].Type = (byte)EffectType.LookAt;

            byte[] typeData = new byte[57];
            if (sourceAvatar != null)
                Buffer.BlockCopy(sourceAvatar.GetBytes(), 0, typeData, 0, 16);
            if (targetObject != null)
                Buffer.BlockCopy(targetObject.GetBytes(), 0, typeData, 16, 16);
            typeData[56] = (byte)type;

            effect.Effect[0].TypeData = typeData;

            Client.Network.SendPacket(effect);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceAvatar"></param>
        /// <param name="targetObject"></param>
        /// <param name="globalOffset"></param>
        /// <param name="color"></param>
        /// <param name="duration"></param>
        public void BeamEffect(LLUUID sourceAvatar, LLUUID targetObject, LLVector3d globalOffset, LLColor color, 
            float duration)
        {
            ViewerEffectPacket effect = new ViewerEffectPacket();

            effect.AgentData.AgentID = Client.Network.AgentID;
            effect.AgentData.SessionID = Client.Network.SessionID;

            effect.Effect = new ViewerEffectPacket.EffectBlock[1];
            effect.Effect[0] = new ViewerEffectPacket.EffectBlock();
            effect.Effect[0].AgentID = Client.Network.AgentID;
            effect.Effect[0].Color = color.GetBytes();
            effect.Effect[0].Duration = duration;
            effect.Effect[0].ID = LLUUID.Random();
            effect.Effect[0].Type = (byte)EffectType.Beam;

            byte[] typeData = new byte[56];
            if (sourceAvatar != null)
                Buffer.BlockCopy(sourceAvatar.GetBytes(), 0, typeData, 0, 16);
            if (targetObject != null)
                Buffer.BlockCopy(targetObject.GetBytes(), 0, typeData, 16, 16);
            Buffer.BlockCopy(globalOffset.GetBytes(), 0, typeData, 32, 24);

            effect.Effect[0].TypeData = typeData;

            Client.Network.SendPacket(effect);
        }

        /// <summary>
        /// Synchronize the local profile and interests information to the server
        /// </summary>
        public void SetAvatarInformation()
        {
            // Basic profile properties
            AvatarPropertiesUpdatePacket apup = new AvatarPropertiesUpdatePacket();
            apup.AgentData.AgentID = this.ID;
            apup.AgentData.SessionID = Client.Network.SessionID;
            apup.PropertiesData.AboutText = Helpers.StringToField(this.ProfileProperties.AboutText);
            apup.PropertiesData.AllowPublish = this.ProfileProperties.AllowPublish;
            apup.PropertiesData.FLAboutText = Helpers.StringToField(this.ProfileProperties.FirstLifeText);
            apup.PropertiesData.FLImageID = this.ProfileProperties.FirstLifeImage;
            apup.PropertiesData.ImageID = this.ProfileProperties.ProfileImage;
            apup.PropertiesData.MaturePublish = this.ProfileProperties.MaturePublish;
            apup.PropertiesData.ProfileURL = Helpers.StringToField(this.ProfileProperties.ProfileURL);

            Client.Network.SendPacket(apup);

            // Interests
            AvatarInterestsUpdatePacket aiup = new AvatarInterestsUpdatePacket();
            aiup.AgentData.AgentID = this.ID;
            aiup.AgentData.SessionID = Client.Network.SessionID;
            aiup.PropertiesData.LanguagesText = Helpers.StringToField(this.ProfileInterests.LanguagesText);
            aiup.PropertiesData.SkillsMask = this.ProfileInterests.SkillsMask;
            aiup.PropertiesData.SkillsText = Helpers.StringToField(this.ProfileInterests.SkillsText);
            aiup.PropertiesData.WantToMask = this.ProfileInterests.WantToMask;
            aiup.PropertiesData.WantToText = Helpers.StringToField(this.ProfileInterests.WantToText);

            Client.Network.SendPacket(aiup);
        }

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
            chat.AgentData.AgentID = this.ID;
            chat.AgentData.SessionID = Client.Network.SessionID;
            chat.ChatData.Channel = channel;
            chat.ChatData.Message = Helpers.StringToField(message);
            chat.ChatData.Type = (byte)type;

            Client.Network.SendPacket(chat);
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
            heightwidth.AgentData.AgentID = Client.Network.AgentID;
            heightwidth.AgentData.SessionID = Client.Network.SessionID;
            heightwidth.AgentData.CircuitCode = Client.Network.CircuitCode;
            heightwidth.HeightWidthBlock.Height = height;
            heightwidth.HeightWidthBlock.Width = width;
            heightwidth.HeightWidthBlock.GenCounter = HeightWidthGenCounter++;

            Client.Network.SendPacket(heightwidth);
        }

        /// <summary>
        /// Sends a request to sit on the specified object
        /// </summary>
        /// <param name="targetID">LLUUID of the object to sit on</param>
        /// <param name="offset">Sit at offset</param>
        public void RequestSit(LLUUID targetID, LLVector3 offset)
        {
            AgentRequestSitPacket requestSit = new AgentRequestSitPacket();
            requestSit.AgentData.AgentID = Client.Network.AgentID;
            requestSit.AgentData.SessionID = Client.Network.SessionID;
            requestSit.TargetObject.TargetID = targetID;
            requestSit.TargetObject.Offset = offset;
            Client.Network.SendPacket(requestSit);
        }

        /// <summary>
        /// Request the list of muted things for this avatar
        /// </summary>
        public void RequestMuteList()
        {
            MuteListRequestPacket mute = new MuteListRequestPacket();
            mute.AgentData.AgentID = Client.Network.AgentID;
            mute.AgentData.SessionID = Client.Network.SessionID;
            mute.MuteData.MuteCRC = 0;

            Client.Network.SendPacket(mute);
        }

        /// <summary>
        /// Request the current L$ balance
        /// </summary>
        public void RequestBalance()
        {
            MoneyBalanceRequestPacket money = new MoneyBalanceRequestPacket();
            money.AgentData.AgentID = Client.Network.AgentID;
            money.AgentData.SessionID = Client.Network.SessionID;
            money.MoneyData.TransactionID = LLUUID.Zero;

            Client.Network.SendPacket(money);
        }

		/// <summary>
		/// Sets home location
		/// </summary>		
		public void SetHome()
		{
			SetStartLocationRequestPacket s = new SetStartLocationRequestPacket();
			s.AgentData = new SetStartLocationRequestPacket.AgentDataBlock();
			s.AgentData.AgentID = Client.Network.AgentID;
			s.AgentData.SessionID = Client.Network.SessionID;
			s.StartLocationData = new SetStartLocationRequestPacket.StartLocationDataBlock();
			s.StartLocationData.LocationPos = Client.Self.Position;
			s.StartLocationData.LocationID = 1;
			s.StartLocationData.SimName = Helpers.StringToField("");
			s.StartLocationData.LocationLookAt = Client.Self.LookAt;
			Client.Network.SendPacket(s);
		}
		
		/// <summary>
		/// Teleports the avatar home
		/// </summary>
		public bool GoHome()
		{
			return Teleport(new LLUUID());
		}
		/// <summary>
        /// Follows a call to RequestSit() to actually sit on the object
        /// </summary>
        public void Sit()
        {
            AgentSitPacket sit = new AgentSitPacket();
            sit.AgentData.AgentID = Client.Network.AgentID;
            sit.AgentData.SessionID = Client.Network.SessionID;
            Client.Network.SendPacket(sit);
        }

        /// <summary>
        /// Give Money to destination Avatar
        /// </summary>
        /// <param name="target">UUID of the Target Avatar</param>
        /// <param name="amount">Amount in L$</param>
        /// <param name="description">Reason (optional normally)</param>
        public void GiveMoney(LLUUID target, int amount, string description)
        {
            // 5001 - transaction type for av to av money transfers
            if (amount > 0)
                GiveMoney(target, amount, description, 5001);
            else
                Client.Log("Attempted to pay zero or negative value " + amount, Helpers.LogLevel.Warning);
        }

        /// <summary>
        /// Give Money to destionation Object or Avatar
        /// </summary>
        /// <param name="target">UUID of the Target Object/Avatar</param>
        /// <param name="amount">Amount in L$</param>
        /// <param name="description">Reason (Optional normally)</param>
        /// <param name="transactiontype">The type of transaction.  Currently only 5001 is
        /// documented for Av->Av money transfers.</param>
        public void GiveMoney(LLUUID target, int amount, string description, int transactiontype)
        {
            MoneyTransferRequestPacket money = new MoneyTransferRequestPacket();
            money.AgentData.AgentID = this.ID;
            money.AgentData.SessionID = Client.Network.SessionID;
            money.MoneyData.Description = Helpers.StringToField(description);
            money.MoneyData.DestID = target;
            money.MoneyData.SourceID = this.ID;
            money.MoneyData.TransactionType = transactiontype;
            money.MoneyData.AggregatePermInventory = 0; //TODO: whats this?
            money.MoneyData.AggregatePermNextOwner = 0; //TODO: whats this?
            money.MoneyData.Flags = 0; //TODO: whats this?
            money.MoneyData.Amount = amount;

            Client.Network.SendPacket(money);
        }

        /// <summary>
        /// Send an AgentAnimation packet that toggles a single animation on
        /// </summary>
        /// <param name="animation">The animation to start playing</param>
        public void AnimationStart(LLUUID animation)
        {
            Dictionary<LLUUID, bool> animations = new Dictionary<LLUUID, bool>();
            animations[animation] = true;

            Animate(animations);
        }

        /// <summary>
        /// Send an AgentAnimation packet that toggles a single animation off
        /// </summary>
        /// <param name="animation">The animation to stop playing</param>
        public void AnimationStop(LLUUID animation)
        {
            Dictionary<LLUUID, bool> animations = new Dictionary<LLUUID, bool>();
            animations[animation] = false;

            Animate(animations);
        }

        /// <summary>
        /// Send an AgentAnimation packet that will toggle animations on or off
        /// </summary>
        /// <param name="animations">A list of animation UUIDs, and whether to
        /// turn that animation on or off</param>
        public void Animate(Dictionary<LLUUID, bool> animations)
        {
            AgentAnimationPacket animate = new AgentAnimationPacket();

            animate.AgentData.AgentID = Client.Network.AgentID;
            animate.AgentData.SessionID = Client.Network.SessionID;
            animate.AnimationList = new AgentAnimationPacket.AnimationListBlock[animations.Count];
            int i = 0;

            foreach (KeyValuePair<LLUUID, bool> animation in animations)
            {
                animate.AnimationList[i] = new AgentAnimationPacket.AnimationListBlock();
                animate.AnimationList[i].AnimID = animation.Key;
                animate.AnimationList[i].StartAnim = animation.Value;

                i++;
            }

            Client.Network.SendPacket(animate);
        }

        /// <summary>
        /// Use the autopilot sim function to move the avatar to a new position
        /// </summary>
        /// <remarks>The z value is currently not handled properly by the simulator</remarks>
        /// <param name="globalX">Integer value for the global X coordinate to move to</param>
        /// <param name="globalY">Integer value for the global Y coordinate to move to</param>
        /// <param name="z">Floating-point value for the Z coordinate to move to</param>
        /// <example>AutoPilot(252620, 247078, 20.2674);</example>
        public void AutoPilot(ulong globalX, ulong globalY, float z)
        {
            GenericMessagePacket autopilot = new GenericMessagePacket();

            autopilot.AgentData.AgentID = Client.Network.AgentID;
            autopilot.AgentData.SessionID = Client.Network.SessionID;
            autopilot.AgentData.TransactionID = LLUUID.Zero;
            autopilot.MethodData.Invoice = LLUUID.Zero;
            autopilot.MethodData.Method = Helpers.StringToField("autopilot");
            autopilot.ParamList = new GenericMessagePacket.ParamListBlock[3];
            autopilot.ParamList[0] = new GenericMessagePacket.ParamListBlock();
            autopilot.ParamList[0].Parameter = Helpers.StringToField(globalX.ToString());
            autopilot.ParamList[1] = new GenericMessagePacket.ParamListBlock();
            autopilot.ParamList[1].Parameter = Helpers.StringToField(globalY.ToString());
            autopilot.ParamList[2] = new GenericMessagePacket.ParamListBlock();
            // TODO: Do we need to prevent z coordinates from being sent in 1.4827e-18 notation?
            autopilot.ParamList[2].Parameter = Helpers.StringToField(z.ToString());

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
            Helpers.LongToUInts(Client.Network.CurrentSim.Handle, out x, out y);
            AutoPilot((ulong)(x + localX), (ulong)(y + localY), z);
        }

		/// <summary>Attempt teleport to specified LLUUID</summary>
		public bool Teleport(LLUUID landmark)
		{
			TeleportStat = TeleportStatus.None;
            TeleportEvent.Reset();
			TeleportLandmarkRequestPacket p = new TeleportLandmarkRequestPacket();
			p.Info = new TeleportLandmarkRequestPacket.InfoBlock();
			p.Info.AgentID = Client.Network.AgentID;
			p.Info.SessionID = Client.Network.SessionID;
			p.Info.LandmarkID = landmark;
			Client.Network.SendPacket(p);

            TeleportEvent.WaitOne(Client.Settings.TELEPORT_TIMEOUT, false);

            if (TeleportStat == TeleportStatus.None ||
                TeleportStat == TeleportStatus.Start ||
                TeleportStat == TeleportStatus.Progress)
            {
                teleportMessage = "Teleport timed out.";
                TeleportStat = TeleportStatus.Failed;
            }

            return (TeleportStat == TeleportStatus.Finished);
		}
        /// <summary>
        /// Attempt to look up a simulator name and teleport to the discovered
        /// destination
        /// </summary>
        /// <param name="simName">Region name to look up</param>
        /// <param name="position">Position to teleport to</param>
        /// <returns>True if the lookup and teleport were successful, otherwise
        /// false</returns>
        public bool Teleport(string simName, LLVector3 position)
        {
            return Teleport(simName, position, new LLVector3(0, 1.0f, 0));
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
        public bool Teleport(string simName, LLVector3 position, LLVector3 lookAt)
        {
            TeleportStat = TeleportStatus.None;
            simName = simName.ToLower();

            if (simName != Client.Network.CurrentSim.Name.ToLower())
            {
                // Teleporting to a foreign sim
                GridRegion region;

                if (Client.Grid.GetGridRegion(simName, out region))
                {
                    return Teleport(region.RegionHandle, position, lookAt);
                }
                else
                {
                    teleportMessage = "Unable to resolve name: " + simName;
                    TeleportStat = TeleportStatus.Failed;
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
        /// Start a teleport process
        /// </summary>
        /// <param name="regionHandle"></param>
        /// <param name="position">Position for Teleport</param>
        /// <returns></returns>
        public bool Teleport(ulong regionHandle, LLVector3 position)
        {
            return Teleport(regionHandle, position, new LLVector3(0.0f, 1.0f, 0.0f));
        }

        /// <summary>
        /// Start a teleport process
        /// </summary>
        /// <param name="regionHandle"></param>
        /// <param name="position">Position for Teleport</param>
        /// <param name="lookAt">Target to look at</param>
        /// <returns></returns>
        public bool Teleport(ulong regionHandle, LLVector3 position, LLVector3 lookAt)
        {
            TeleportStat = TeleportStatus.None;
            TeleportEvent.Reset();

            RequestTeleport(regionHandle, position, lookAt);

            TeleportEvent.WaitOne(Client.Settings.TELEPORT_TIMEOUT, false);

            if (TeleportStat == TeleportStatus.None ||
                TeleportStat == TeleportStatus.Start ||
                TeleportStat == TeleportStatus.Progress)
            {
                teleportMessage = "Teleport timed out.";
                TeleportStat = TeleportStatus.Failed;
            }

            return (TeleportStat == TeleportStatus.Finished);
        }

        /// <summary>
        /// Start a teleport process
        /// </summary>
        /// <param name="regionHandle"></param>
        /// <param name="position">Position for Teleport</param>
        public void RequestTeleport(ulong regionHandle, LLVector3 position)
        {
            RequestTeleport(regionHandle, position, new LLVector3(0.0f, 1.0f, 0.0f));
        }

        /// <summary>
        /// Start a teleport process
        /// </summary>
        /// <param name="regionHandle"></param>
        /// <param name="position">Position for Teleport</param>
        /// <param name="lookAt">Target to look at</param>
        public void RequestTeleport(ulong regionHandle, LLVector3 position, LLVector3 lookAt)
        {
            if (Client.Network.CurrentSim != null && Client.Network.CurrentSim.SimCaps != null && Client.Network.CurrentSim.SimCaps.IsEventQueueRunning)
            {
                TeleportLocationRequestPacket teleport = new TeleportLocationRequestPacket();
                teleport.AgentData.AgentID = Client.Network.AgentID;
                teleport.AgentData.SessionID = Client.Network.SessionID;
                teleport.Info.LookAt = lookAt;
                teleport.Info.Position = position;
                teleport.Info.RegionHandle = regionHandle;

                Client.Log("Requesting teleport to region handle " + regionHandle.ToString(), Helpers.LogLevel.Info);

                Client.Network.SendPacket(teleport);
            }
            else
            {
                teleportMessage = "CAPS event queue is not running";
                TeleportEvent.Set();
                TeleportStat = TeleportStatus.Failed;
            }
        }

        public void SendTeleportLure(LLUUID targetID)
        {
            SendTeleportLure(targetID, "Join me in " + Client.Network.CurrentSim.Name + "!");
        }

        public void SendTeleportLure(LLUUID targetID, string message)
        {
            StartLurePacket p = new StartLurePacket();
            p.AgentData.AgentID = Client.Self.ID;
            p.AgentData.SessionID = Client.Network.SessionID;
            p.Info.LureType = 0;
            p.Info.Message = Helpers.StringToField(message);
            p.TargetData = new StartLurePacket.TargetDataBlock[] { new StartLurePacket.TargetDataBlock() };
            p.TargetData[0].TargetID = targetID;
            Client.Network.SendPacket(p);
        }

        /// <summary>
        /// Respond to a teleport lure by either accepting it and initiating 
        /// the teleport, or denying it
        /// </summary>
        /// <param name="requesterID">UUID of the avatar requesting the teleport</param>
        /// <param name="accept">Accept the teleport request or deny it</param>
        public void TeleportLureRespond(LLUUID requesterID, bool accept)
        {
            InstantMessage(Client.ToString(), requesterID, String.Empty, LLUUID.Random(), 
                accept ? InstantMessageDialog.AcceptTeleport : InstantMessageDialog.DenyTeleport,
                InstantMessageOnline.Offline, this.Position, LLUUID.Zero, new byte[0]);

            if (accept)
            {
                TeleportLureRequestPacket lure = new TeleportLureRequestPacket();

                lure.Info.AgentID = Client.Network.AgentID;
                lure.Info.SessionID = Client.Network.SessionID;
                lure.Info.LureID = Client.Network.AgentID;
                lure.Info.TeleportFlags = (uint)TeleportFlags.ViaLure;

                Client.Network.SendPacket(lure);
            }
        }

        /// <summary>
        /// Grabs an object
        /// </summary>
        /// <param name="objectLocalID">Local ID of Object to grab</param>
        public void Grab(uint objectLocalID)
        {
            ObjectGrabPacket grab = new ObjectGrabPacket();
            grab.AgentData.AgentID = Client.Network.AgentID;
            grab.AgentData.SessionID = Client.Network.SessionID;
            grab.ObjectData.LocalID = objectLocalID;
            grab.ObjectData.GrabOffset = new LLVector3(0, 0, 0);
            Client.Network.SendPacket(grab);
        }

        /// <summary>
        /// Drags on an object
        /// </summary>
        /// <param name="objectID">Strangely, LLUID instead of local ID</param>
        /// <param name="grabPosition">Drag target in region coordinates</param>
        public void GrabUpdate(LLUUID objectID, LLVector3 grabPosition)
        {
            ObjectGrabUpdatePacket grab = new ObjectGrabUpdatePacket();
            grab.AgentData.AgentID = Client.Network.AgentID;
            grab.AgentData.SessionID = Client.Network.SessionID;
            grab.ObjectData.ObjectID = objectID;
            grab.ObjectData.GrabOffsetInitial = new LLVector3(0, 0, 0);
            grab.ObjectData.GrabPosition = grabPosition;
            grab.ObjectData.TimeSinceLast = 0;
            Client.Network.SendPacket(grab);
        }

        /// <summary>
        /// Releases a grabbed object
        /// </summary>
        public void DeGrab(uint objectLocalID)
        {
            ObjectDeGrabPacket degrab = new ObjectDeGrabPacket();
            degrab.AgentData.AgentID = Client.Network.AgentID;
            degrab.AgentData.SessionID = Client.Network.SessionID;
            degrab.ObjectData.LocalID = objectLocalID;
            Client.Network.SendPacket(degrab);
        }

        /// <summary>
        /// Touches an object
        /// </summary>
        public void Touch(uint objectLocalID)
        {
            Client.Self.Grab(objectLocalID);
            Client.Self.DeGrab(objectLocalID);
        }

        /// <summary>
        /// Rotates body toward target position
        /// </summary>
        /// <param name="target">Region coordinates to turn toward</param>
        public bool TurnToward(LLVector3 target)
        {
            if (Client.Settings.SEND_AGENT_UPDATES)
            {
                LLQuaternion newRot = Helpers.RotBetween(new LLVector3(1, 0, 0), Helpers.VecNorm(target - Client.Self.Position));
                Client.Self.Status.Camera.BodyRotation = newRot;
                Client.Self.Status.SendUpdate();
                return true;
            }
            else
            {
                Client.Log("Attempted TurnToward but agent updates are disabled", Helpers.LogLevel.Warning);
                return false;
            }
        }

        /// <summary>
        /// Request to join a group. If there is an enrollment fee it will 
        /// automatically be deducted from your balance
        /// </summary>
        /// <param name="groupID">The group to attempt to join</param>
        public void RequestJoinGroup(LLUUID groupID)
        {
            JoinGroupRequestPacket join = new JoinGroupRequestPacket();

            join.AgentData.AgentID = Client.Network.AgentID;
            join.AgentData.SessionID = Client.Network.SessionID;
            join.GroupData.GroupID = groupID;

            Client.Network.SendPacket(join);
        }

        /// <summary>
        /// Request to leave a group
        /// </summary>
        /// <param name="groupID">The group to attempt to leave</param>
        public void RequestLeaveGroup(LLUUID groupID)
        {
            LeaveGroupRequestPacket leave = new LeaveGroupRequestPacket();

            leave.AgentData.AgentID = Client.Network.AgentID;
            leave.AgentData.SessionID = Client.Network.SessionID;
            leave.GroupData.GroupID = groupID;

            Client.Network.SendPacket(leave);
        }

        /// <summary>
        /// Set our current active group
        /// </summary>
        /// <param name="groupID">The group we are a member of that we want to 
        /// activate</param>
        public void ActivateGroup(LLUUID groupID)
        {
            ActivateGroupPacket activate = new ActivateGroupPacket();

            activate.AgentData.AgentID = Client.Network.AgentID;
            activate.AgentData.SessionID = Client.Network.SessionID;
            activate.AgentData.GroupID = groupID;

            Client.Network.SendPacket(activate);
        }

        /// <summary>
        /// Move an agent in to a simulator. This packet is the last packet
        /// needed to complete the transition in to a new simulator
        /// </summary>
        /// <param name="simulator"></param>
        public void CompleteAgentMovement(Simulator simulator)
        {
            CompleteAgentMovementPacket move = new CompleteAgentMovementPacket();

            move.AgentData.AgentID = Client.Network.AgentID;
            move.AgentData.SessionID = Client.Network.SessionID;
            move.AgentData.CircuitCode = Client.Network.CircuitCode;

            Client.Network.SendPacket(move, simulator);
        }

        /// <summary>
        /// Set this avatar's tier contribution
        /// </summary>
        /// <param name="group">Group to change tier in</param>
        /// <param name="contribution">amount of tier to donate</param>
        public void SetGroupContribution(LLUUID group, int contribution)
        {
            libsecondlife.Packets.SetGroupContributionPacket sgp = new SetGroupContributionPacket();
            sgp.AgentData.AgentID = Client.Network.AgentID;
            sgp.AgentData.SessionID = Client.Network.SessionID;
            sgp.Data.GroupID = group;
            sgp.Data.Contribution = contribution;
            Client.Network.SendPacket(sgp);
        }

        /// <summary>
        /// Change the role that determines your active title
        /// </summary>
        /// <param name="group">Group to use</param>
        /// <param name="role">Role to change to</param>
        public void ChangeTitle(LLUUID group, LLUUID role)
        {
            libsecondlife.Packets.GroupTitleUpdatePacket gtu = new GroupTitleUpdatePacket();
            gtu.AgentData.AgentID = Client.Network.AgentID;
            gtu.AgentData.SessionID = Client.Network.SessionID;
            gtu.AgentData.TitleRoleID = role;
            gtu.AgentData.GroupID = group;
            Client.Network.SendPacket(gtu);
        }

        /// <summary>
        /// Sends camera and action updates to the server including the 
        /// position and orientation of our camera, and a ControlFlags field
        /// specifying our current movement actions
        /// </summary>
        /// <param name="controlFlags"></param>
        /// <param name="position"></param>
        /// <param name="forwardAxis"></param>
        /// <param name="leftAxis"></param>
        /// <param name="upAxis"></param>
        /// <param name="bodyRotation"></param>
        /// <param name="headRotation"></param>
        /// <param name="farClip"></param>
        /// <param name="reliable"></param>
        public void UpdateCamera(MainAvatar.ControlFlags controlFlags, LLVector3 position, LLVector3 forwardAxis,
            LLVector3 leftAxis, LLVector3 upAxis, LLQuaternion bodyRotation, LLQuaternion headRotation, float farClip,
            AgentFlags flags, AgentState state, bool reliable)
        {
            AgentUpdatePacket update = new AgentUpdatePacket();

            update.AgentData.AgentID = Client.Network.AgentID;
            update.AgentData.SessionID = Client.Network.SessionID;
            update.AgentData.BodyRotation = bodyRotation;
            update.AgentData.HeadRotation = headRotation;
            update.AgentData.CameraCenter = position;
            update.AgentData.CameraAtAxis = forwardAxis;
            update.AgentData.CameraLeftAxis = leftAxis;
            update.AgentData.CameraUpAxis = upAxis;
            update.AgentData.Far = farClip;
            update.AgentData.ControlFlags = (uint)controlFlags;
            update.AgentData.Flags = (byte)flags;
            update.AgentData.State = (byte)state;
            update.Header.Reliable = reliable;

            Client.Network.SendPacket(update);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="itemID"></param>
        /// <param name="taskID"></param>
        /// <param name="permissions"></param>
        public void ScriptQuestionReply(Simulator simulator, LLUUID itemID, LLUUID taskID, ScriptPermission permissions)
        {
            ScriptAnswerYesPacket yes = new ScriptAnswerYesPacket();
            yes.AgentData.AgentID = Client.Network.AgentID;
            yes.AgentData.SessionID = Client.Network.SessionID;
            yes.Data.ItemID = itemID;
            yes.Data.TaskID = taskID;
            yes.Data.Questions = (int)permissions;

            Client.Network.SendPacket(yes, simulator);
        }

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
                    OnInstantMessage(
                        im.AgentData.AgentID
                        , Helpers.FieldToUTF8String(im.MessageBlock.FromAgentName),
                        im.MessageBlock.ToAgentID
                        , im.MessageBlock.ParentEstateID
                        , im.MessageBlock.RegionID
                        , im.MessageBlock.Position
                        , (InstantMessageDialog)im.MessageBlock.Dialog
                        , im.MessageBlock.FromGroup
                        , im.MessageBlock.ID
                        , new DateTime(im.MessageBlock.Timestamp)
                        , Helpers.FieldToUTF8String(im.MessageBlock.Message)
                        , (InstantMessageOnline)im.MessageBlock.Offline
                        , im.MessageBlock.BinaryBucket
                        );
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

                OnChat(Helpers.FieldToUTF8String(chat.ChatData.Message)
                    , (ChatAudibleLevel)chat.ChatData.Audible
                    , (ChatType)chat.ChatData.ChatType
                    , (ChatSourceType)chat.ChatData.SourceType
                    , Helpers.FieldToUTF8String(chat.ChatData.FromName)
                    , chat.ChatData.SourceID
                    , chat.ChatData.OwnerID
                    , chat.ChatData.Position
                    );
            }
        }

        /// <summary>
        /// Used for parsing llDialog's
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
                    buttons.Add(Helpers.FieldToUTF8String(button.ButtonLabel));
                }

                OnScriptDialog(Helpers.FieldToUTF8String(dialog.Data.Message),
                    Helpers.FieldToUTF8String(dialog.Data.ObjectName),
                    dialog.Data.ImageID,
                    dialog.Data.ObjectID,
                    Helpers.FieldToUTF8String(dialog.Data.FirstName),
                    Helpers.FieldToUTF8String(dialog.Data.LastName),
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
                    OnScriptQuestion(question.Data.TaskID,
                        question.Data.ItemID,
                        Helpers.FieldToUTF8String(question.Data.ObjectName),
                        Helpers.FieldToUTF8String(question.Data.ObjectOwner),
                        (ScriptPermission)question.Data.Questions);
                }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        /// <summary>
        /// Update client's Position, LookAt and region handle from incoming packet
        /// </summary>
        /// <param name="packet">Incoming AgentMovementCompletePacket</param>
        /// <param name="simulator">Unused</param>
        private void MovementCompleteHandler(Packet packet, Simulator simulator)
        {
            AgentMovementCompletePacket movement = (AgentMovementCompletePacket)packet;

            this.Position = movement.Data.Position;
            this.LookAt = movement.Data.LookAt;
            simulator.Handle = movement.Data.RegionHandle;
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

        private void JoinGroupHandler(Packet packet, Simulator simulator)
        {
            if (OnJoinGroup != null)
            {
                JoinGroupReplyPacket reply = (JoinGroupReplyPacket)packet;

                OnJoinGroup(reply.GroupData.GroupID, reply.GroupData.Success);
            }
        }

        private void LeaveGroupHandler(Packet packet, Simulator simulator)
        {
            if (OnLeaveGroup != null)
            {
                LeaveGroupReplyPacket reply = (LeaveGroupReplyPacket)packet;

                OnLeaveGroup(reply.GroupData.GroupID, reply.GroupData.Success);
            }
        }

        public void AgentDataUpdateHandler(Packet packet, Simulator simulator)
        {
            AgentDataUpdatePacket p = (AgentDataUpdatePacket)packet;
            if (p.AgentData.AgentID == simulator.Client.Network.AgentID) {
                if (activeGroup != p.AgentData.ActiveGroupID)
                {
                    activeGroup = p.AgentData.ActiveGroupID;
                    if (OnActiveGroupChanged != null)
                        OnActiveGroupChanged(activeGroup);
                }
            }
        }
		
        private void DropGroupHandler(Packet packet, Simulator simulator)
        {
            if (OnGroupDropped != null)
            {
                OnGroupDropped(((AgentDropGroupPacket)packet).AgentData.GroupID);
            }
        }

        /// <summary>
        /// Update Client Avatar's L$ balance from incoming packet
        /// </summary>
        /// <param name="packet">Incoming MoneyBalanceReplyPacket</param>
        /// <param name="simulator">Unused</param>
        private void BalanceHandler(Packet packet, Simulator simulator)
        {
            MoneyBalanceReplyPacket mbrp = (MoneyBalanceReplyPacket)packet;
            balance = mbrp.MoneyData.MoneyBalance;

            if (OnMoneyBalanceReplyReceived != null)
            {
                try { OnMoneyBalanceReplyReceived(mbrp.MoneyData.TransactionID, 
                    mbrp.MoneyData.TransactionSuccess, mbrp.MoneyData.MoneyBalance, 
                    mbrp.MoneyData.SquareMetersCredit, mbrp.MoneyData.SquareMetersCommitted, 
                    Helpers.FieldToUTF8String(mbrp.MoneyData.Description)); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }

            if (OnBalanceUpdated != null)
            {
                try { OnBalanceUpdated(balance); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

	    private void EventQueueHandler(string message, Hashtable body, Caps caps)
        {
            if (message == "TeleportFinish" && body.ContainsKey("Info"))
            {
                ArrayList infoList = (ArrayList)body["Info"];
                Hashtable info = (Hashtable)infoList[0];

                // Backwards compatibility hack 
                TeleportFinishPacket packet = new TeleportFinishPacket();

                packet.Info.SimIP = Helpers.BytesToUIntBig((byte[])info["SimIP"]);
                packet.Info.LocationID = Helpers.BytesToUInt((byte[])info["LocationID"]);
                packet.Info.TeleportFlags = Helpers.BytesToUInt((byte[])info["TeleportFlags"]);
                packet.Info.AgentID = (LLUUID)info["AgentID"];
                packet.Info.RegionHandle = Helpers.BytesToUInt64((byte[])info["RegionHandle"]);
                packet.Info.SeedCapability = Helpers.StringToField((string)info["SeedCapability"]);
                packet.Info.SimPort = (ushort)(int)info["SimPort"];
                packet.Info.SimAccess = (byte)(int)info["SimAccess"];

                Client.DebugLog(String.Format(
                    "Received a TeleportFinish event from {0}, SimIP: {1}, Location: {2}, RegionHandle: {3}", 
                    caps.Simulator.ToString(), packet.Info.SimIP, packet.Info.LocationID, packet.Info.RegionHandle));

                TeleportHandler(packet, Client.Network.CurrentSim);
	    }
	    else if(message == "EstablishAgentCommunication" && Client.Settings.MULTIPLE_SIMS)
	    {
		string ipAndPort = (string)body["sim-ip-and-port"];
		string[] pieces = ipAndPort.Split(':');
		IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(pieces[0]), Convert.ToInt32(pieces[1]));
		Simulator sim = Client.Network.FindSimulator(endPoint);
		if(sim == null) {
			Client.Log("Got EstablishAgentCommunication for unknown sim "
				+ ipAndPort,  Helpers.LogLevel.Error);
		}
		else
		{
			Client.Log("Got EstablishAgentCommunication for sim "
				+ ipAndPort + ", seed cap " + (string)body["seed-capability"],  Helpers.LogLevel.Info);
			sim.setSeedCaps((string)body["seed-capability"]);
		}
            }
            else
            {
                Client.Log("Received unhandled event " + message + " in the EventQueueHandler", 
                    Helpers.LogLevel.Warning);
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
                TeleportStat = TeleportStatus.Start;

                Client.DebugLog("TeleportStart received from " + simulator.ToString() + ", Flags: " + flags.ToString());
            }
            else if (packet.Type == PacketType.TeleportProgress)
            {
                TeleportProgressPacket progress = (TeleportProgressPacket)packet;

                teleportMessage = Helpers.FieldToUTF8String(progress.Info.Message);
                flags = (TeleportFlags)progress.Info.TeleportFlags;
                TeleportStat = TeleportStatus.Progress;

                Client.DebugLog("TeleportProgress received from " + simulator.ToString() + "Message: " + 
                    teleportMessage + ", Flags: " + flags.ToString());
            }
            else if (packet.Type == PacketType.TeleportFailed)
            {
                TeleportFailedPacket failed = (TeleportFailedPacket)packet;

                teleportMessage = Helpers.FieldToUTF8String(failed.Info.Reason);
                TeleportStat = TeleportStatus.Failed;
                finished = true;

                Client.DebugLog("TeleportFailed received from " + simulator.ToString() + ", Reason: " + teleportMessage);
            }
            else if (packet.Type == PacketType.TeleportFinish)
            {
                TeleportFinishPacket finish = (TeleportFinishPacket)packet;

                flags = (TeleportFlags)finish.Info.TeleportFlags;
                string seedcaps = Helpers.FieldToUTF8String(finish.Info.SeedCapability);
                finished = true;

                Client.DebugLog("TeleportFinish received from " + simulator.ToString() + ", Flags: " + flags.ToString());

                // Connect to the new sim
                Simulator newSimulator = Client.Network.Connect(new IPAddress(finish.Info.SimIP), 
                    finish.Info.SimPort, true, seedcaps);

                if (newSimulator != null)
                {
                    teleportMessage = "Teleport finished";
                    TeleportStat = TeleportStatus.Finished;

                    // Disconnect from the previous sim
                    Client.Network.DisconnectSim(simulator);

                    Client.Log("Moved to new sim " + newSimulator.ToString(), Helpers.LogLevel.Info);
                }
                else
                {
                    teleportMessage = "Failed to connect to the new sim after a teleport";
                    TeleportStat = TeleportStatus.Failed;

                    // Attempt to reconnect to the previous simulator
                    // TODO: This hasn't been tested at all
                    Client.Network.Connect(simulator.IPEndPoint.Address, (ushort)simulator.IPEndPoint.Port,
                        true, simulator.SimCaps.Seedcaps);

                    Client.Log(teleportMessage, Helpers.LogLevel.Warning);
                }
            }
            else if (packet.Type == PacketType.TeleportCancel)
            {
                //TeleportCancelPacket cancel = (TeleportCancelPacket)packet;

                teleportMessage = "Cancelled";
                TeleportStat = TeleportStatus.Cancelled;
                finished = true;

                Client.DebugLog("TeleportCancel received from " + simulator.ToString());
            }
            else if (packet.Type == PacketType.TeleportLocal)
            {
                TeleportLocalPacket local = (TeleportLocalPacket)packet;

                teleportMessage = "Teleport finished";
                flags = (TeleportFlags)local.Info.TeleportFlags;
                TeleportStat = TeleportStatus.Finished;
                LookAt = local.Info.LookAt;
                Position = local.Info.Position;
                // This field is apparently not used for anything
                //local.Info.LocationID;
                finished = true;

                Client.DebugLog("TeleportLocal received from " + simulator.ToString() + ", Flags: " + flags.ToString());
            }

            if (OnTeleport != null)
            {
                try { OnTeleport(teleportMessage, TeleportStat, flags); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }

            if (finished) TeleportEvent.Set();
        }
    }

    #endregion Packet Handlers
}
