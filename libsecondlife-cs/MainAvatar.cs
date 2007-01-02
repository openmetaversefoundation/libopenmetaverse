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
using System.Timers;
using System.Net;
using System.Collections.Generic;
using libsecondlife.Packets;

namespace libsecondlife
{
    /// <summary>
    /// Class to hold Client Avatar's data
    /// </summary>
    public partial class MainAvatar
    {
        /// <summary>
        /// Current teleport status
        /// </summary>
        public enum TeleportStatus
        {
            /// <summary></summary>
            None,
            /// <summary>Teleport Start</summary>
            Start,
            /// <summary>Teleport in Progress</summary>
            Progress,
            /// <summary>Teleport Failed</summary>
            Failed,
            /// <summary>Teleport Completed</summary>
            Finished
        }

        /// <summary>
        /// Special commands used in Instant Messages
        /// </summary>
        public enum InstantMessageDialog
        {
            /// <summary>Indicates a regular IM from another agent</summary>
            MessageFromAgent = 0,
            /// <summary>Indicates that someone has given the user an object</summary>
            GiveInventory = 4,
            /// <summary>Indicates that someone has given the user a notecard</summary>
            GiveNotecard = 9,
            /// <summary>Indicates that the IM is from an object</summary>
            MessageFromObject = 19,
            /// <summary>Indicates that the IM is a teleport invitation</summary>
            RequestTeleport = 22,
            /// <summary>Response sent to the agent which inititiated a teleport invitation</summary>
            AcceptTeleport = 23,
            /// <summary>Response sent to the agent which inititiated a teleport invitation</summary>
            DenyTeleport = 24,
            /// <summary>Indicates that a user has started typing</summary>
            StartTyping = 41,
            /// <summary>Indicates that a user has stopped typing</summary>
            StopTyping = 42
        }

        /// <summary>
        /// Conversion type to denote Chat Packet types in an easier-to-understand format
        /// </summary>
        public enum ChatType
        {
            /// <summary>Whispers (5m radius)</summary>
            Whisper = 0,
            /// <summary>Normal chat (10/20m radius), what the official viewer typically sends</summary>
            Normal = 1,
            /// <summary>Shouting! (100m radius)</summary>
            Shout = 2,
            /// <summary>Say chat (10/20m radius) - The official viewer will 
            /// print "[4:15] You say, hey" instead of "[4:15] You: hey"</summary>
            Say = 3,
            /// <summary>Event message when an Avatar has begun to type</summary>
            StartTyping = 4,
            /// <summary>Event message when an Avatar has stopped typing</summary>
            StopTyping = 5
        }


        /// <summary>
        /// Triggered on incoming chat messages
        /// </summary>
        /// <param name="Message">Text of chat message</param>
        /// <param name="Audible">Is this normal audible chat or not.</param>
        /// <param name="Type">Type of chat (whisper,shout,status,etc)</param>
        /// <param name="Sourcetype">Type of source (Agent / Object / ???)</param>
        /// <param name="FromName">Text name of sending Avatar/Object</param>
        /// <param name="ID"></param>
        public delegate void ChatCallback(string message, byte audible, byte type, byte sourcetype,
            string fromName, LLUUID id, LLUUID ownerid, LLVector3 position);

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
        /// Triggered when the L$ account balance for this avatar changes
        /// </summary>
        /// <param name="balance">The new account balance</param>
        public delegate void BalanceCallback(int balance);

        /// <summary>
        /// Tiggered on incoming instant messages
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
        /// <param name="offline"></param>
        /// <param name="binaryBucket"></param>
        public delegate void InstantMessageCallback(LLUUID fromAgentID, string fromAgentName,
            LLUUID toAgentID, uint parentEstateID, LLUUID regionID, LLVector3 position,
            byte dialog, bool groupIM, LLUUID imSessionID, DateTime timestamp, string message,
            byte offline, byte[] binaryBucket);

        /// <summary>
        /// Triggered for any status updates of a teleport (progress, failed, succeeded)
        /// </summary>
        /// <param name="currentSim">The simulator the avatar is currently residing in</param>
        /// <param name="message">A message about the current teleport status</param>
        /// <param name="status">The current status of the teleport</param>
        public delegate void TeleportCallback(Simulator currentSim, string message, TeleportStatus status);

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


        /// <summary>Callback for incoming chat packets</summary>
        public event ChatCallback OnChat;
        /// <summary>Callback for pop-up dialogs from scripts</summary>
        public event ScriptDialogCallback OnScriptDialog;
        /// <summary>Callback for incoming IMs</summary>
        public event InstantMessageCallback OnInstantMessage;
        /// <summary>Callback for Teleport request update</summary>
        public event TeleportCallback OnTeleport;
        /// <summary>Callback for incoming change in L$ balance</summary>
        public event BalanceCallback OnBalanceUpdated;
        /// <summary>Callback reply for an attempt to join a group</summary>
        public event JoinGroupCallback OnJoinGroup;
        /// <summary>Callback reply for an attempt to leave a group</summary>
        public event LeaveGroupCallback OnLeaveGroup;
        /// <summary>Callback for informing the avatar that it is no longer a member of a group</summary>
        public event GroupDroppedCallback OnGroupDropped;

        /// <summary>Your (client) Avatar UUID, asset server</summary>
        public LLUUID ID = LLUUID.Zero;
        /// <summary>Your (client) Avatar ID, local to Region/sim</summary>
        public uint LocalID;
        /// <summary>Avatar First Name (i.e. Philip)</summary>
        public string FirstName = String.Empty;
        /// <summary>Avatar Last Name (i.e. Linden)</summary>
        public string LastName = String.Empty;
        /// <summary>Positive and negative ratings</summary>
        /// <remarks>This information is read-only and any changes will not be
        /// reflected on the server</remarks>
        public Avatar.Statistics ProfileStatistics = new Avatar.Statistics();
        /// <summary>Avatar properties including about text, profile URL, image IDs and 
        /// publishing settings</summary>
        /// <remarks>If you change fields in this struct, the changes will not
        /// be reflected on the server until you call SetAvatarInformation</remarks>
        public Avatar.Properties ProfileProperties = new Avatar.Properties();
        /// <summary>Avatar interests including spoken languages, skills, and "want to"
        /// choices</summary>
        /// <remarks>If you change fields in this struct, the changes will not
        /// be reflected on the server until you call SetAvatarInformation</remarks>
        public Avatar.Interests ProfileInterests = new Avatar.Interests();
        /// <summary>Current position of avatar</summary>
        public LLVector3 Position = LLVector3.Zero;
        /// <summary>Current rotation of avatar</summary>
        public LLQuaternion Rotation = LLQuaternion.Identity;
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
        public LLUUID InventoryRootFolderUUID;
        
        
        /// <summary>Gets the health of the agent</summary>
        public float Health
        {
            get { return health; }
        }
        
        /// <summary>Gets the current balance of the agent</summary>
        public int Balance
        {
            get { return balance; }
        }

        /// <summary>Gets the local ID of the prim the avatar is sitting on,
        /// zero if the avatar is not currently sitting</summary>
        public uint SittingOn
        {
            get { return sittingOn; }
        }

        internal uint sittingOn = 0;
        internal string teleportMessage = String.Empty;

        private SecondLife Client;
        private TeleportCallback OnBeginTeleport;
        private TeleportStatus TeleportStat;
        private Timer TeleportTimer;
        private bool TeleportTimeout;
        private uint HeightWidthGenCounter;
        private float health = 0.0f;
        private int balance = 0;

        /// <summary>
        /// Constructor, setup callbacks for packets related to our avatar
        /// </summary>
        /// <param name="client"></param>
        public MainAvatar(SecondLife client)
        {
            NetworkManager.PacketCallback callback;
            Client = client;

            Status = new MainAvatarStatus(Client);

            // Coarse location callback
            Client.Network.RegisterCallback(PacketType.CoarseLocationUpdate, new NetworkManager.PacketCallback(CoarseLocationHandler));

            // Teleport callbacks
            callback = new NetworkManager.PacketCallback(TeleportHandler);
            Client.Network.RegisterCallback(PacketType.TeleportStart, callback);
            Client.Network.RegisterCallback(PacketType.TeleportProgress, callback);
            Client.Network.RegisterCallback(PacketType.TeleportFailed, callback);
            Client.Network.RegisterCallback(PacketType.TeleportFinish, callback);

            // Instant Message callback
            Client.Network.RegisterCallback(PacketType.ImprovedInstantMessage, new NetworkManager.PacketCallback(InstantMessageHandler));

            // Chat callback
            Client.Network.RegisterCallback(PacketType.ChatFromSimulator, new NetworkManager.PacketCallback(ChatHandler));

            // Script dialog callback
            Client.Network.RegisterCallback(PacketType.ScriptDialog, new NetworkManager.PacketCallback(ScriptDialogHandler));

            // Teleport timeout timer
            TeleportTimer = new Timer(Client.Settings.TELEPORT_TIMEOUT);
            TeleportTimer.Elapsed += new ElapsedEventHandler(TeleportTimerEvent);
            TeleportTimeout = false;

            // Movement complete callback
            Client.Network.RegisterCallback(PacketType.AgentMovementComplete, new NetworkManager.PacketCallback(MovementCompleteHandler));

            // Health callback
            Client.Network.RegisterCallback(PacketType.HealthMessage, new NetworkManager.PacketCallback(HealthHandler));

            // Money callbacks
            callback = new NetworkManager.PacketCallback(BalanceHandler);
            Client.Network.RegisterCallback(PacketType.MoneyBalanceReply, callback);
            Client.Network.RegisterCallback(PacketType.MoneySummaryReply, callback);
            Client.Network.RegisterCallback(PacketType.AdjustBalance, callback);

            // Group callbacks
            Client.Network.RegisterCallback(PacketType.JoinGroupReply, new NetworkManager.PacketCallback(JoinGroupHandler));
            Client.Network.RegisterCallback(PacketType.LeaveGroupReply, new NetworkManager.PacketCallback(LeaveGroupHandler));
            Client.Network.RegisterCallback(PacketType.AgentDropGroup, new NetworkManager.PacketCallback(DropGroupHandler));
        }

        /// <summary>
        /// Send an Instant Message
        /// </summary>
        /// <param name="target">Key of Avatar</param>
        /// <param name="message">Text Message being sent.</param>
        public void InstantMessage(LLUUID target, string message)
        {
            InstantMessage(FirstName + " " + LastName, LLUUID.Random(), target, message, null, LLUUID.Random());
        }

        /// <summary>
        /// Send an Instant Message
        /// </summary>
        /// <param name="target">Key of Avatar</param>
        /// <param name="message">Text Message being sent.</param>
        /// <param name="IMSessionID">IM Session ID</param>
        public void InstantMessage(LLUUID target, string message, LLUUID IMSessionID)
        {
            InstantMessage(FirstName + " " + LastName, LLUUID.Random(), target, message, null, IMSessionID);
        }

        /// <summary>
        /// Send an Instant Message
        /// </summary>
        /// <param name="fromName">Client's Avatar</param>
        /// <param name="sessionID">SessionID of current connection to grid</param>
        /// <param name="target">Key of Avatar</param>
        /// <param name="message">Text Message being sent.</param>
        /// <param name="conferenceIDs"></param>
        public void InstantMessage(string fromName, LLUUID sessionID, LLUUID target, string message, LLUUID[] conferenceIDs)
        {
            InstantMessage(fromName, sessionID, target, message, conferenceIDs, LLUUID.Random());
        }

        /// <summary>
        /// Send an Instant Message
        /// </summary>
        /// <param name="fromName">The name this IM will show up as being from</param>
        /// <param name="sessionID">Session ID of current connection to grid</param>
        /// <param name="target">Key of Avatar</param>
        /// <param name="message">Text message being sent</param>
        /// <param name="conferenceIDs"></param>
        /// <param name="IMSessionID">IM session ID (to differentiate between IM windows)</param>
        public void InstantMessage(string fromName, LLUUID sessionID, LLUUID target, string message,
            LLUUID[] conferenceIDs, LLUUID IMSessionID)
        {
            ImprovedInstantMessagePacket im = new ImprovedInstantMessagePacket();
            im.AgentData.AgentID = this.ID;
            im.AgentData.SessionID = Client.Network.SessionID;
            im.MessageBlock.Dialog = 0;
            im.MessageBlock.FromAgentName = Helpers.StringToField(fromName);
            im.MessageBlock.FromGroup = false;
            im.MessageBlock.ID = IMSessionID;
            im.MessageBlock.Message = Helpers.StringToField(message);
            im.MessageBlock.Offline = 1;
            im.MessageBlock.ToAgentID = target;
            if (conferenceIDs != null && conferenceIDs.Length > 0)
            {
                im.MessageBlock.BinaryBucket = new byte[16 * conferenceIDs.Length];

                for (int i = 0; i < conferenceIDs.Length; ++i)
                {
                    Array.Copy(conferenceIDs[i].Data, 0, im.MessageBlock.BinaryBucket, i * 16, 16);
                }
            }
            else
            {
                im.MessageBlock.BinaryBucket = new byte[0];
            }

            // These fields are mandatory, even if we don't have valid values for them
            im.MessageBlock.Position = LLVector3.Zero;
            //TODO: Allow region id to be correctly set by caller or fetched from Client.*
            im.MessageBlock.RegionID = LLUUID.Zero;

            // Send the message
            Client.Network.SendPacket(im);
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

            // Interests
            AvatarInterestsUpdatePacket aiup = new AvatarInterestsUpdatePacket();

            aiup.AgentData.AgentID = this.ID;
            aiup.AgentData.SessionID = Client.Network.SessionID;
            aiup.PropertiesData.LanguagesText = Helpers.StringToField(this.ProfileInterests.LanguagesText);
            aiup.PropertiesData.SkillsMask = this.ProfileInterests.SkillsMask;
            aiup.PropertiesData.SkillsText = Helpers.StringToField(this.ProfileInterests.SkillsText);
            aiup.PropertiesData.WantToMask = this.ProfileInterests.WantToMask;
            aiup.PropertiesData.WantToText = Helpers.StringToField(this.ProfileInterests.WantToText);

            //Send packets
            Client.Network.SendPacket(apup);
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
        /// Set the height and the width of your avatar. This is used to scale
        /// the avatar mesh.
        /// </summary>
        /// <param name="height">New height of the avatar</param>
        /// <param name="width">New width of the avatar</param>
        public void SetHeightWidth(ushort height, ushort width)
        {
            AgentHeightWidthPacket heightwidth = new AgentHeightWidthPacket();
            heightwidth.AgentData.AgentID = Client.Network.AgentID;
            heightwidth.AgentData.SessionID = Client.Network.SessionID;
            heightwidth.AgentData.CircuitCode = Client.Network.CurrentSim.CircuitCode;
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
        /// Set the always running toggle on the server
        /// </summary>
        /// <param name="alwaysRun">Whether the avatar should always run or not</param>
        public void SetAlwaysRun(bool alwaysRun)
        {
            SetAlwaysRunPacket run = new SetAlwaysRunPacket();
            run.AgentData.AgentID = Client.Network.AgentID;
            run.AgentData.SessionID = Client.Network.SessionID;
            run.AgentData.AlwaysRun = alwaysRun;

            Client.Network.SendPacket(run);
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
        /// <example>AutoPilot(252620, 247078, 20.2674);</example>
        public void AutoPilotLocal(int localX, int localY, float z)
        {
            uint x, y;
            Helpers.LongToUInts(Client.Network.CurrentSim.Region.Handle, out x, out y);
            AutoPilot((ulong)(x + localX), (ulong)(y + localY), z);
        }

        /// <summary>
        /// Start a teleport process
        /// </summary>
        /// <param name="regionHandle"></param>
        /// <param name="position">Position for Teleport</param>
        /// <param name="tc">Callback ID</param>
        public void BeginTeleport(ulong regionHandle, LLVector3 position, TeleportCallback tc)
        {
            BeginTeleport(regionHandle, position, new LLVector3(position.X + 1.0f, position.Y, position.Z), tc);
        }

        /// <summary>
        /// Start a teleport process
        /// </summary>
        /// <param name="regionHandle"></param>
        /// <param name="position">Position for Teleport</param>
        /// <param name="lookAt">Target to look at</param>
        /// <param name="tc">Callback ID</param>
        public void BeginTeleport(ulong regionHandle, LLVector3 position, LLVector3 lookAt, TeleportCallback tc)
        {
            OnBeginTeleport = tc;

            TeleportLocationRequestPacket teleport = new TeleportLocationRequestPacket();
            teleport.AgentData.AgentID = Client.Network.AgentID;
            teleport.AgentData.SessionID = Client.Network.SessionID;
            teleport.Info.LookAt = lookAt;
            teleport.Info.Position = position;
            teleport.Info.RegionHandle = regionHandle;

            Client.Log("Teleporting to region " + regionHandle.ToString(), Helpers.LogLevel.Info);

            Client.Network.SendPacket(teleport);
        }

        /// <summary>
        /// Start a teleport process
        /// </summary>
        /// <param name="regionHandle"></param>
        /// <param name="position">Position for Teleport</param>
        /// <returns></returns>
        public bool Teleport(ulong regionHandle, LLVector3 position)
        {
            return Teleport(regionHandle, position, new LLVector3(position.X + 1.0f, position.Y, position.Z));
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

            TeleportLocationRequestPacket teleport = new TeleportLocationRequestPacket();
            teleport.AgentData.AgentID = Client.Network.AgentID;
            teleport.AgentData.SessionID = Client.Network.SessionID;
            teleport.Info.LookAt = lookAt;
            teleport.Info.Position = position;

            teleport.Info.RegionHandle = regionHandle;

            Client.Log("Teleporting to region " + regionHandle.ToString(), Helpers.LogLevel.Info);

            // Start the timeout check
            TeleportTimeout = false;
            TeleportTimer.Start();

            Client.Network.SendPacket(teleport);

            // FIXME: Use a ManualResetEvent, Client.Tick() is bad
            while (TeleportStat != TeleportStatus.Failed && TeleportStat != TeleportStatus.Finished && !TeleportTimeout)
            {
                Client.Tick();
            }

            TeleportTimer.Stop();

            if (TeleportTimeout)
            {
                teleportMessage = "Teleport timed out.";
                TeleportStat = TeleportStatus.Failed;

                if (OnTeleport != null) { OnTeleport(Client.Network.CurrentSim, teleportMessage, TeleportStat); }
            }
            else
            {
                if (OnTeleport != null) { OnTeleport(Client.Network.CurrentSim, teleportMessage, TeleportStat); }
            }

            return (TeleportStat == TeleportStatus.Finished);
        }

        /// <summary>
        /// Generic Teleport Function
        /// </summary>
        /// <param name="simName">Region name</param>
        /// <param name="position">Position for Teleport</param>
        /// <returns></returns>
        public bool Teleport(string simName, LLVector3 position)
        {
            //position.Z = 0; //why was this here?
            return Teleport(simName, position, new LLVector3(0, 1.0F, 0));
        }

        /// <summary>
        /// Teleport Function
        /// </summary>
        /// <param name="simName">Region name</param>
        /// <param name="position">Position for Teleport</param>
        /// <param name="lookAt">Target to look at</param>
        /// <returns></returns>
        public bool Teleport(string simName, LLVector3 position, LLVector3 lookAt)
        {
            int attempts = 0;
            TeleportStat = TeleportStatus.None;

            simName = simName.ToLower();

            GridRegion region = Client.Grid.GetGridRegion(simName);

            if (region != null)
            {
                return Teleport(region.RegionHandle, position, lookAt);
            }
            else
            {
                while (attempts++ < 5)
                {
                    region = Client.Grid.GetGridRegion(simName);

                    if (region != null)
                    {
                        return Teleport(region.RegionHandle, position, lookAt);
                    }
                    else
                    {
                        // Request the region info again
                        Client.Grid.AddSim(simName);

                        System.Threading.Thread.Sleep(1000);
                    }
                }
            }

            if (OnTeleport != null)
            {
                teleportMessage = "Unable to resolve name: " + simName;
                TeleportStat = TeleportStatus.Failed;
                OnTeleport(Client.Network.CurrentSim, teleportMessage, TeleportStat);
            }

            return false;
        }

        /// <summary>
        /// Respond to a teleport lure by either accepting it and initiating 
        /// the teleport, or denying it
        /// </summary>
        /// <param name="requesterID">UUID of the avatar requesting the teleport</param>
        /// <param name="accept">Accept the teleport request or deny it</param>
        public void TeleportLureRespond(LLUUID requesterID, bool accept)
        {
            ImprovedInstantMessagePacket im = new ImprovedInstantMessagePacket();

            im.AgentData.AgentID = Client.Network.AgentID;
            im.AgentData.SessionID = Client.Network.SessionID;
            im.MessageBlock.BinaryBucket = new byte[0];
            im.MessageBlock.FromAgentName = Helpers.StringToField(this.FirstName + " " + this.LastName);
            im.MessageBlock.FromGroup = false;
            im.MessageBlock.ID = Client.Network.AgentID;
            im.MessageBlock.Message = new byte[0];
            im.MessageBlock.Offline = 0;
            im.MessageBlock.ParentEstateID = 0;
            im.MessageBlock.Position = this.Position;
            im.MessageBlock.RegionID = LLUUID.Zero;
            im.MessageBlock.Timestamp = 0;
            im.MessageBlock.ToAgentID = requesterID;

            if (accept)
            {
                im.MessageBlock.Dialog = (byte)InstantMessageDialog.AcceptTeleport;

                Client.Network.SendPacket(im);

                TeleportLureRequestPacket lure = new TeleportLureRequestPacket();

                lure.Info.AgentID = Client.Network.AgentID;
                lure.Info.SessionID = Client.Network.SessionID;
                lure.Info.LureID = Client.Network.AgentID;
                lure.Info.TeleportFlags = 4; // TODO: What does this mean?

                Client.Network.SendPacket(lure);
            }
            else
            {
                im.MessageBlock.Dialog = (byte)InstantMessageDialog.DenyTeleport;

                Client.Network.SendPacket(im);
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
        /// Move an agent in to a simulator. This packet is the last packet
        /// needed to complete the transition in to a new simulator
        /// </summary>
        /// <param name="simulator"></param>
        public void CompleteAgentMovement(Simulator simulator)
        {
            CompleteAgentMovementPacket move = new CompleteAgentMovementPacket();

            move.AgentData.AgentID = Client.Network.AgentID;
            move.AgentData.SessionID = Client.Network.SessionID;
            move.AgentData.CircuitCode = simulator.CircuitCode;

            Client.Network.SendPacket(move, simulator);
        }

        /// <summary>
        /// Sends camera and action updates to the server including the 
        /// position and orientation of our camera, and a ControlFlags field
        /// specifying our current movement actions
        /// </summary>
        /// <param name="reliable">Whether to ensure this packet makes it to the server</param>
        public void UpdateCamera(Avatar.AgentUpdateFlags controlFlags, LLVector3 position, LLVector3 forwardAxis,
            LLVector3 leftAxis, LLVector3 upAxis, LLQuaternion bodyRotation, LLQuaternion headRotation, float farClip,
            bool reliable)
        {
            AgentUpdatePacket update = new AgentUpdatePacket();

            update.AgentData.AgentID = Client.Network.AgentID;
            update.AgentData.SessionID = Client.Network.SessionID;
            update.AgentData.State = 0;
            update.AgentData.BodyRotation = bodyRotation;
            update.AgentData.HeadRotation = headRotation;
            update.AgentData.CameraCenter = position;
            update.AgentData.CameraAtAxis = forwardAxis;
            update.AgentData.CameraLeftAxis = leftAxis;
            update.AgentData.CameraUpAxis = upAxis;
            update.AgentData.Far = farClip;
            update.AgentData.ControlFlags = (uint)controlFlags;
            update.AgentData.Flags = 0;
            update.Header.Reliable = reliable;

            Client.Network.SendPacket(update);
        }

        /// <summary>
        /// [UNUSED - for now]
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void CoarseLocationHandler(Packet packet, Simulator simulator)
        {
            // TODO: This will be useful one day
        }

        /// <summary>
        /// Take an incoming ImprovedInstantMessage packet, auto-parse, and if
        ///   OnInstantMessage is defined call that with the appropriate arguments.
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
                        , Helpers.FieldToString(im.MessageBlock.FromAgentName),
                        im.MessageBlock.ToAgentID
                        , im.MessageBlock.ParentEstateID
                        , im.MessageBlock.RegionID
                        , im.MessageBlock.Position
                        , im.MessageBlock.Dialog
                        , im.MessageBlock.FromGroup
                        , im.MessageBlock.ID
                        , new DateTime(im.MessageBlock.Timestamp)
                        , Helpers.FieldToString(im.MessageBlock.Message)
                        , im.MessageBlock.Offline
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

                OnChat(Helpers.FieldToString(chat.ChatData.Message)
                    , chat.ChatData.Audible
                    , chat.ChatData.ChatType
                    , chat.ChatData.SourceType
                    , Helpers.FieldToString(chat.ChatData.FromName)
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
                    buttons.Add(Helpers.FieldToString(button.ButtonLabel));
                }

                OnScriptDialog(Helpers.FieldToString(dialog.Data.Message),
                    Helpers.FieldToString(dialog.Data.ObjectName),
                    dialog.Data.ImageID,
                    dialog.Data.ObjectID,
                    Helpers.FieldToString(dialog.Data.FirstName),
                    Helpers.FieldToString(dialog.Data.LastName),
                    dialog.Data.ChatChannel,
                    buttons);
            }
        }

        /// <summary>
        /// Update client's Position and LookAt from incoming packet
        /// </summary>
        /// <param name="packet">Incoming AgentMovementCompletePacket</param>
        /// <param name="simulator">Unused</param>
        private void MovementCompleteHandler(Packet packet, Simulator simulator)
        {
            AgentMovementCompletePacket movement = (AgentMovementCompletePacket)packet;

            this.Position = movement.Data.Position;
            this.LookAt = movement.Data.LookAt;
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
            if (packet.Type == PacketType.MoneyBalanceReply)
            {
                balance = ((MoneyBalanceReplyPacket)packet).MoneyData.MoneyBalance;
            }
            else if (packet.Type == PacketType.MoneySummaryReply)
            {
                balance = ((MoneySummaryReplyPacket)packet).MoneyData.Balance;
            }
            else if (packet.Type == PacketType.AdjustBalance)
            {
                balance += ((AdjustBalancePacket)packet).AgentData.Delta;
            }

            if (OnBalanceUpdated != null)
            {
                OnBalanceUpdated(balance);
            }
        }

        /// <summary>
        /// Handler for teleport Requests
        /// </summary>
        /// <param name="packet">Incoming TeleportHandler packet</param>
        /// <param name="simulator">Simulator sending teleport information</param>
        private void TeleportHandler(Packet packet, Simulator simulator)
        {
            if (packet.Type == PacketType.TeleportStart)
            {
                Client.DebugLog("TeleportStart received from " + simulator.ToString());

                teleportMessage = "Teleport started";
                TeleportStat = TeleportStatus.Start;

                if (OnBeginTeleport != null)
                {
                    OnBeginTeleport(Client.Network.CurrentSim, teleportMessage, TeleportStat);
                }
            }
            else if (packet.Type == PacketType.TeleportProgress)
            {
                Client.DebugLog("TeleportProgress received from " + simulator.ToString());

                teleportMessage = Helpers.FieldToString(((TeleportProgressPacket)packet).Info.Message);
                TeleportStat = TeleportStatus.Progress;

                if (OnBeginTeleport != null)
                {
                    OnBeginTeleport(Client.Network.CurrentSim, teleportMessage, TeleportStat);
                }
            }
            else if (packet.Type == PacketType.TeleportFailed)
            {
                Client.DebugLog("TeleportFailed received from " + simulator.ToString());

                teleportMessage = Helpers.FieldToString(((TeleportFailedPacket)packet).Info.Reason);
                TeleportStat = TeleportStatus.Failed;

                if (OnBeginTeleport != null)
                {
                    OnBeginTeleport(Client.Network.CurrentSim, teleportMessage, TeleportStat);
                }

                OnBeginTeleport = null;
            }
            else if (packet.Type == PacketType.TeleportFinish)
            {
                Client.DebugLog("TeleportFinish received from " + simulator.ToString());

                TeleportFinishPacket finish = (TeleportFinishPacket)packet;
                Simulator previousSim = Client.Network.CurrentSim;

                // Connect to the new sim
                Simulator sim = Client.Network.Connect(new IPAddress((long)finish.Info.SimIP), finish.Info.SimPort,
                    simulator.CircuitCode, true);

                if (sim != null)
                {
                    teleportMessage = "Teleport finished";
                    TeleportStat = TeleportStatus.Finished;

                    // Move the avatar in to the new sim
                    CompleteAgentMovementPacket move = new CompleteAgentMovementPacket();
                    move.AgentData.AgentID = Client.Network.AgentID;
                    move.AgentData.SessionID = Client.Network.SessionID;
                    move.AgentData.CircuitCode = simulator.CircuitCode;
                    Client.Network.SendPacket(move, sim);

                    // Disconnect from the previous sim
                    Client.Network.DisconnectSim(previousSim);

                    Client.Log("Moved to new sim " + sim.ToString(), Helpers.LogLevel.Info);

                    if (OnBeginTeleport != null)
                    {
                        OnBeginTeleport(sim, teleportMessage, TeleportStat);
                    }
                    else
                    {
                        // Sleep a little while so we can collect parcel information
                        // NOTE: This doesn't belong in libsecondlife
                        // System.Threading.Thread.Sleep(1000);
                    }
                }
                else
                {
                    teleportMessage = "Failed to connect to the new sim after a teleport";
                    TeleportStat = TeleportStatus.Failed;

                    // FIXME: Set the previous CurrentSim to the current simulator again

                    Client.Log(teleportMessage, Helpers.LogLevel.Warning);

                    if (OnBeginTeleport != null)
                    {
                        OnBeginTeleport(Client.Network.CurrentSim, teleportMessage, TeleportStat);
                    }
                }

                OnBeginTeleport = null;
            }
        }

        /// <summary>
        /// Teleport Timer Event Handler. Used for enforcing timeouts.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="ea"></param>
        private void TeleportTimerEvent(object source, System.Timers.ElapsedEventArgs ea)
        {
            TeleportTimeout = true;
        }
    }
}