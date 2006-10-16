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
using System.Collections;
using libsecondlife.Packets;

namespace libsecondlife
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Message"></param>
    /// <param name="Audible"></param>
    /// <param name="Type"></param>
    /// <param name="Sourcetype"></param>
    /// <param name="FromName"></param>
    /// <param name="ID"></param>
    public delegate void ChatCallback(string message, byte audible, byte type, byte sourcetype,
        string fromName, LLUUID id);

    /// <summary>
    /// Event is triggered when the L$ account balance for this avatar changes.
    /// </summary>
    /// <param name="balance">The new account balance</param>
    public delegate void BalanceCallback(int balance);

    /// <summary>
    /// Triggered whenever an instant message is received.
    /// </summary>
    /// <param name="FromAgentID"></param>
    /// <param name="FromAgentName"></param>
    /// <param name="ToAgentID"></param>
    /// <param name="ParentEstateID"></param>
    /// <param name="RegionID"></param>
    /// <param name="Position"></param>
    /// <param name="Dialog"></param>
    /// <param name="GroupIM"></param>
    /// <param name="IMSessionID"></param>
    /// <param name="Timestamp"></param>
    /// <param name="Message"></param>
    public delegate void InstantMessageCallback(LLUUID fromAgentID, string fromAgentName, 
        LLUUID toAgentID, uint parentEstateID, LLUUID regionID, LLVector3 position, 
        bool dialog, bool groupIM, LLUUID imSessionID, DateTime timestamp, string message, 
        byte offline, byte[] binaryBucket);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="AgentID"></param>
    /// <param name="Online"></param>
    public delegate void FriendNotificationCallback(LLUUID agentID, bool online);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public delegate void TeleportCallback(string message);

    /// <summary>
    /// 
    /// </summary>
    public class Avatar
    {
        /// <summary></summary>
        public LLUUID ID;
        /// <summary></summary>
        public uint LocalID;
        /// <summary></summary>
        public string Name;
        /// <summary></summary>
        public string GroupName;
        /// <summary></summary>
        public bool Online;
        /// <summary></summary>
        public LLVector3 Position;
        /// <summary></summary>
        public LLQuaternion Rotation;
        /// <summary></summary>
        public Region CurrentRegion;
    }

    /// <summary>
    /// 
    /// </summary>
    public class MainAvatar
    {
        /// <summary></summary>
        public event ChatCallback OnChat;
        /// <summary></summary>
        public event InstantMessageCallback OnInstantMessage;
        /// <summary></summary>
        public event FriendNotificationCallback OnFriendNotification;
        /// <summary></summary>
        public event TeleportCallback OnTeleport;
        /// <summary></summary>
        public event BalanceCallback OnBalanceUpdated;

        /// <summary></summary>
        public LLUUID ID;
        /// <summary></summary>
        public uint LocalID;
        /// <summary></summary>
        public string FirstName;
        /// <summary></summary>
        public string LastName;
        /// <summary></summary>
        public string TeleportMessage;
        /// <summary></summary>
        public LLVector3 Position;
        /// <summary>Current rotation of the avatar</summary>
        public LLQuaternion Rotation;
        /// <summary>The point the avatar is currently looking at
        /// (may not stay updated)</summary>
        public LLVector3 LookAt;
        /// <summary></summary>
        public LLVector3 HomePosition;
        /// <summary></summary>
        public LLVector3 HomeLookAt;
        /// <summary>Gets the health of the current agent</summary>
        protected float health;
        public float Health
        {
            get { return health; }
        }

        private SecondLife Client;
        private int TeleportStatus;
        private Timer TeleportTimer;
        private bool TeleportTimeout;
        private uint HeightWidthGenCounter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public MainAvatar(SecondLife client)
        {
            PacketCallback callback;
            Client = client;
            TeleportMessage = "";

            // Create emtpy vectors for now
            HomeLookAt = HomePosition = Position = LookAt = new LLVector3();
            Rotation = new LLQuaternion();

            // Coarse location callback
            Client.Network.RegisterCallback(PacketType.CoarseLocationUpdate, new PacketCallback(CoarseLocationHandler));

            // Teleport callbacks
            callback = new PacketCallback(TeleportHandler);
            Client.Network.RegisterCallback(PacketType.TeleportStart, callback);
            Client.Network.RegisterCallback(PacketType.TeleportProgress, callback);
            Client.Network.RegisterCallback(PacketType.TeleportFailed, callback);
            Client.Network.RegisterCallback(PacketType.TeleportFinish, callback);

            // Instant Message callback
            Client.Network.RegisterCallback(PacketType.ImprovedInstantMessage, new PacketCallback(InstantMessageHandler));

            // Chat callback
            Client.Network.RegisterCallback(PacketType.ChatFromSimulator, new PacketCallback(ChatHandler));

            // Friend notification callback
            callback = new PacketCallback(FriendNotificationHandler);
            Client.Network.RegisterCallback(PacketType.OnlineNotification, callback);
            Client.Network.RegisterCallback(PacketType.OfflineNotification, callback);

            TeleportTimer = new Timer(18000);
            TeleportTimer.Elapsed += new ElapsedEventHandler(TeleportTimerEvent);
            TeleportTimeout = false;

            // Movement complete callback
            Client.Network.RegisterCallback(PacketType.AgentMovementComplete, new PacketCallback(MovementCompleteHandler));

            // Health callback
            Client.Network.RegisterCallback(PacketType.HealthMessage, new PacketCallback(HealthHandler));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="message"></param>
        public void InstantMessage(LLUUID target, string message)
        {
            InstantMessage(FirstName + " " + LastName, LLUUID.GenerateUUID(), target, message, null, LLUUID.GenerateUUID());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="message"></param>
        /// <param name="IMSessionID"></param>
        public void InstantMessage(LLUUID target, string message, LLUUID IMSessionID)
        {
            InstantMessage(FirstName + " " + LastName, LLUUID.GenerateUUID(), target, message, null, IMSessionID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromName"></param>
        /// <param name="sessionID"></param>
        /// <param name="target"></param>
        /// <param name="message"></param>
        /// <param name="conferenceIDs"></param>
        public void InstantMessage(string fromName, LLUUID sessionID, LLUUID target, string message, LLUUID[] conferenceIDs)
        {
            InstantMessage(fromName, sessionID, target, message, conferenceIDs, LLUUID.GenerateUUID());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromName"></param>
        /// <param name="sessionID"></param>
        /// <param name="target"></param>
        /// <param name="message"></param>
        /// <param name="conferenceIDs"></param>
        /// <param name="IMSessionID"></param>
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
            im.MessageBlock.Position = new LLVector3();
            im.MessageBlock.RegionID = new LLUUID(); //TODO: Allow region id to be correctly set by caller or fetched from Client.*


            // Send the message
            Client.Network.SendPacket((Packet)im);
        }

        /// <summary>
        /// 
        /// </summary>
        public enum ChatType
        {
            /// <summary></summary>
            Whisper = 0,
            /// <summary></summary>
            Normal = 1,
            /// <summary></summary>
            Shout = 2,
            /// <summary></summary>
            Say = 3,
            /// <summary></summary>
            StartTyping = 4,
            /// <summary></summary>
            StopTyping = 5
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="channel"></param>
        /// <param name="type"></param>
        public void Chat(string message, int channel, ChatType type)
        {
            ChatFromViewerPacket chat = new ChatFromViewerPacket();
            chat.AgentData.AgentID = this.ID;
            chat.AgentData.SessionID = Client.Network.SessionID;
            chat.ChatData.Channel = channel;
            chat.ChatData.Message = Helpers.StringToField(message);
            chat.ChatData.Type = (byte)type;

            Client.Network.SendPacket((Packet)chat);
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

            Client.Network.SendPacket((Packet)heightwidth);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="amount"></param>
        /// <param name="description"></param>
        public void GiveMoney(LLUUID target, int amount, string description)
        {
            // 5001 - transaction type for av to av money transfers
            GiveMoney(target, amount, description, 5001);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="amount"></param>
        /// <param name="description"></param>
        /// <param name="transactiontype"></param>
        public void GiveMoney(LLUUID target, int amount, string description, int transactiontype)
        {
            MoneyTransferRequestPacket money = new MoneyTransferRequestPacket();
            money.AgentData.AgentID = this.ID;
            money.AgentData.SessionID = Client.Network.SessionID;
            money.MoneyData.Description = Helpers.StringToField(description);
            money.MoneyData.DestID = target;
            money.MoneyData.SourceID = this.ID;
            money.MoneyData.TransactionType = transactiontype;

            Client.Network.SendPacket((Packet)money);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regionHandle"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool Teleport(ulong regionHandle, LLVector3 position)
        {
            return Teleport(regionHandle, position, new LLVector3(position.X + 1.0F, position.Y, position.Z));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regionHandle"></param>
        /// <param name="position"></param>
        /// <param name="lookAt"></param>
        /// <returns></returns>
        public bool Teleport(ulong regionHandle, LLVector3 position, LLVector3 lookAt)
        {
            TeleportStatus = 0;

            TeleportLocationRequestPacket teleport = new TeleportLocationRequestPacket();
            teleport.AgentData.AgentID = Client.Network.AgentID;
            teleport.AgentData.SessionID = Client.Network.SessionID;
            teleport.Info.LookAt = lookAt;
            teleport.Info.Position = position;
            
            teleport.Info.RegionHandle = regionHandle;
            teleport.Header.Reliable = true;

            Client.Log("Teleporting to region " + regionHandle.ToString(), Helpers.LogLevel.Info);

            // Start the timeout check
            TeleportTimeout = false;
            TeleportTimer.Start();

            Client.Network.SendPacket((Packet)teleport);

            while (TeleportStatus == 0 && !TeleportTimeout)
            {
                Client.Tick();
            }

            TeleportTimer.Stop();

            if (TeleportTimeout)
            {
                if (OnTeleport != null) { OnTeleport("Teleport timed out."); }
            }
            else
            {
                if (OnTeleport != null) { OnTeleport(TeleportMessage); }
            }

            return (TeleportStatus == 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simName"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool Teleport(string simName, LLVector3 position)
        {
            position.Z = 0;
            return Teleport(simName, position, new LLVector3(0, 1.0F, 0));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simName"></param>
        /// <param name="position"></param>
        /// <param name="lookAt"></param>
        /// <returns></returns>
        public bool Teleport(string simName, LLVector3 position, LLVector3 lookAt)
        {
            Client.Grid.AddSim(simName);
            int attempts = 0;

            while (attempts++ < 5)
            {
                if (Client.Grid.Regions.ContainsKey(simName.ToLower()))
                {
                    return Teleport(((GridRegion)Client.Grid.Regions[simName]).RegionHandle, position, lookAt);
                }
                else
                {
                    System.Threading.Thread.Sleep(1000);
                    Client.Grid.AddSim(simName);
                    Client.Tick();
                }
            }
            if (OnTeleport != null)
            {
                OnTeleport("Unable to resolve name: " + simName);
            }
            return false;
        }

        /// <summary>
        /// 
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

        public void UpdateCamera(bool reliable)
        {
            AgentUpdatePacket update = new AgentUpdatePacket();
            update.AgentData.AgentID = Client.Network.AgentID;
            update.AgentData.SessionID = Client.Network.SessionID;
            update.AgentData.State = 0;
            update.AgentData.BodyRotation = new LLQuaternion(0, 0.6519076f, 0, 0);
            update.AgentData.HeadRotation = new LLQuaternion();
            // Semi-sane default values
            update.AgentData.CameraCenter = new LLVector3(9.549901f, 7.033957f, 11.75f);
            update.AgentData.CameraAtAxis = new LLVector3(0.7f, 0.7f, 0);
            update.AgentData.CameraLeftAxis = new LLVector3(-0.7f, 0.7f, 0);
            update.AgentData.CameraUpAxis = new LLVector3(0.1822026f, 0.9828722f, 0);
            update.AgentData.Far = 384;
            update.AgentData.ControlFlags = 0; // TODO: What is this?
            update.AgentData.Flags = 0;
            update.Header.Reliable = reliable;

            Client.Network.SendPacket(update);

            // Send an AgentFOV packet widening our field of vision
            /*AgentFOVPacket fovPacket = new AgentFOVPacket();
            fovPacket.AgentData.AgentID = this.ID;
            fovPacket.AgentData.SessionID = Client.Network.SessionID;
            fovPacket.AgentData.CircuitCode = simulator.CircuitCode;
            fovPacket.FOVBlock.GenCounter = 0;
            fovPacket.FOVBlock.VerticalAngle = 6.28318531f;
            fovPacket.Header.Reliable = true;
            Client.Network.SendPacket((Packet)fovPacket);*/
        }

        private void FriendNotificationHandler(Packet packet, Simulator simulator)
        {
            // If the agent is online...
            if (packet.Type == PacketType.OnlineNotification)
            {
                foreach (OnlineNotificationPacket.AgentBlockBlock block in ((OnlineNotificationPacket)packet).AgentBlock)
                {
                    Client.AddAvatar(block.AgentID);
                    #region AvatarsMutex
                    Client.AvatarsMutex.WaitOne();
                    ((Avatar)Client.Avatars[block.AgentID]).Online = true;
                    Client.AvatarsMutex.ReleaseMutex();
                    #endregion AvatarsMutex

                    if (OnFriendNotification != null)
                    {
                        OnFriendNotification(block.AgentID, true);
                    }
                }
            }

            // If the agent is Offline...
            if (packet.Type == PacketType.OfflineNotification)
            {
                foreach (OfflineNotificationPacket.AgentBlockBlock block in ((OfflineNotificationPacket)packet).AgentBlock)
                {
                    Client.AddAvatar(block.AgentID);
                    #region AvatarsMutex
                    Client.AvatarsMutex.WaitOne();
                    ((Avatar)Client.Avatars[block.AgentID]).Online = false;
                    Client.AvatarsMutex.ReleaseMutex();
                    #endregion AvatarsMutex

                    if (OnFriendNotification != null)
                    {
                        OnFriendNotification(block.AgentID, true);
                    }
                }
            }
        }

        private void CoarseLocationHandler(Packet packet, Simulator simulator)
        {
            // TODO: This will be useful one day
        }

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
                        , Convert.ToBoolean(im.MessageBlock.Dialog)
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

        private void ChatHandler(Packet packet, Simulator simulator)
        {
            if (packet.Type == PacketType.ChatFromSimulator)
            {
                ChatFromSimulatorPacket chat = (ChatFromSimulatorPacket)packet;

                if (OnChat != null)
                {
                    OnChat(Helpers.FieldToString(chat.ChatData.Message), chat.ChatData.Audible, 
                        chat.ChatData.ChatType, chat.ChatData.SourceType, 
                        Helpers.FieldToString(chat.ChatData.FromName), chat.ChatData.SourceID);
                }
            }
        }

        private void MovementCompleteHandler(Packet packet, Simulator simulator)
        {
            AgentMovementCompletePacket movement = (AgentMovementCompletePacket)packet;

            this.Position = movement.Data.Position;
            this.LookAt = movement.Data.LookAt;
        }

        private void HealthHandler(Packet packet, Simulator simulator)
        {
            health = ((HealthMessagePacket)packet).HealthData.Health;
        }

        private void TeleportHandler(Packet packet, Simulator simulator)
        {
            if (packet.Type == PacketType.TeleportStart)
            {
                TeleportMessage = "Teleport started";
            }
            else if (packet.Type == PacketType.TeleportProgress)
            {
                TeleportMessage = Helpers.FieldToString(((TeleportProgressPacket)packet).Info.Message);
            }
            else if (packet.Type == PacketType.TeleportFailed)
            {
                TeleportMessage = Helpers.FieldToString(((TeleportFailedPacket)packet).Info.Reason);
                TeleportStatus = -1;
            }
            else if (packet.Type == PacketType.TeleportFinish)
            {
                TeleportFinishPacket finish = (TeleportFinishPacket)packet;
                TeleportMessage = "Teleport finished";

                Simulator sim = Client.Network.Connect(new IPAddress((long)finish.Info.SimIP), finish.Info.SimPort, 
                    simulator.CircuitCode, true);
                        
                if ( sim != null)
                {
                    // Sync the current region and current simulator
                    Client.CurrentRegion = Client.Network.CurrentSim.Region;

                    // Move the avatar in to this sim
                    CompleteAgentMovementPacket move = new CompleteAgentMovementPacket();
                    move.AgentData.AgentID = this.ID;
                    move.AgentData.SessionID = Client.Network.SessionID;
                    move.AgentData.CircuitCode = simulator.CircuitCode;
                    Client.Network.SendPacket((Packet)move);

                    Console.WriteLine(move);

                    Client.Log("Moved to new sim " + Client.Network.CurrentSim.Region.Name + "(" + 
                        Client.Network.CurrentSim.IPEndPoint.ToString() + ")",
                        Helpers.LogLevel.Info);

                    // Sleep a little while so we can collect parcel information
                    System.Threading.Thread.Sleep(1000);

                    TeleportStatus = 1;
                }
                else
                {
                    TeleportStatus = -1;
                }
            }
        }

        private void TeleportTimerEvent(object source, System.Timers.ElapsedEventArgs ea)
        {
            TeleportTimeout = true;
        }
    }
}
