/*
 * Copyright (c) 2007-2008, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
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
using System.Text;
using OpenMetaverse.Packets;

namespace OpenMetaverse
{
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum FriendRights : int
    {
        /// <summary>The avatar has no rights</summary>
        None = 0,
        /// <summary>The avatar can see the online status of the target avatar</summary>
        CanSeeOnline = 1,
        /// <summary>The avatar can see the location of the target avatar on the map</summary>
        CanSeeOnMap = 2,
        /// <summary>The avatar can modify the ojects of the target avatar </summary>
        CanModifyObjects = 4
    }

    /// <summary>
    /// This class holds information about an avatar in the friends list.  There are two ways 
    /// to interface to this class.  The first is through the set of boolean properties.  This is the typical
    /// way clients of this class will use it.  The second interface is through two bitflag properties,
    /// TheirFriendsRights and MyFriendsRights
    /// </summary>
    public class FriendInfo
    {
        private UUID m_id;
        private string m_name;
        private bool m_isOnline;
        private bool m_canSeeMeOnline;
        private bool m_canSeeMeOnMap;
        private bool m_canModifyMyObjects;
        private bool m_canSeeThemOnline;
        private bool m_canSeeThemOnMap;
        private bool m_canModifyTheirObjects;

        #region Properties

        /// <summary>
        /// System ID of the avatar
        /// </summary>
        public UUID UUID { get { return m_id; } }

        /// <summary>
        /// full name of the avatar
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// True if the avatar is online
        /// </summary>
        public bool IsOnline
        {
            get { return m_isOnline; }
            set { m_isOnline = value; }
        }

        /// <summary>
        /// True if the friend can see if I am online
        /// </summary>
        public bool CanSeeMeOnline
        {
            get { return m_canSeeMeOnline; }
            set
            {
                m_canSeeMeOnline = value;

                // if I can't see them online, then I can't see them on the map
                if (!m_canSeeMeOnline)
                    m_canSeeMeOnMap = false;
            }
        }

        /// <summary>
        /// True if the friend can see me on the map 
        /// </summary>
        public bool CanSeeMeOnMap
        {
            get { return m_canSeeMeOnMap; }
            set
            {
                // if I can't see them online, then I can't see them on the map
                if (m_canSeeMeOnline)
                    m_canSeeMeOnMap = value;
            }
        }

        /// <summary>
        /// True if the freind can modify my objects
        /// </summary>
        public bool CanModifyMyObjects
        {
            get { return m_canModifyMyObjects; }
            set { m_canModifyMyObjects = value; }
        }

        /// <summary>
        /// True if I can see if my friend is online
        /// </summary>
        public bool CanSeeThemOnline { get { return m_canSeeThemOnline; } }

        /// <summary>
        /// True if I can see if my friend is on the map
        /// </summary>
        public bool CanSeeThemOnMap { get { return m_canSeeThemOnMap; } }

        /// <summary>
        /// True if I can modify my friend's objects
        /// </summary>
        public bool CanModifyTheirObjects { get { return m_canModifyTheirObjects; } }

        /// <summary>
        /// My friend's rights represented as bitmapped flags
        /// </summary>
        public FriendRights TheirFriendRights
        {
            get
            {
                FriendRights results = FriendRights.None;
                if (m_canSeeMeOnline)
                    results |= FriendRights.CanSeeOnline;
                if (m_canSeeMeOnMap)
                    results |= FriendRights.CanSeeOnMap;
                if (m_canModifyMyObjects)
                    results |= FriendRights.CanModifyObjects;

                return results;
            }
            set
            {
                m_canSeeMeOnline = (value & FriendRights.CanSeeOnline) != 0;
                m_canSeeMeOnMap = (value & FriendRights.CanSeeOnMap) != 0;
                m_canModifyMyObjects = (value & FriendRights.CanModifyObjects) != 0;
            }
        }

        /// <summary>
        /// My rights represented as bitmapped flags
        /// </summary>
        public FriendRights MyFriendRights
        {
            get
            {
                FriendRights results = FriendRights.None;
                if (m_canSeeThemOnline)
                    results |= FriendRights.CanSeeOnline;
                if (m_canSeeThemOnMap)
                    results |= FriendRights.CanSeeOnMap;
                if (m_canModifyTheirObjects)
                    results |= FriendRights.CanModifyObjects;

                return results;
            }
            set
            {
                m_canSeeThemOnline = (value & FriendRights.CanSeeOnline) != 0;
                m_canSeeThemOnMap = (value & FriendRights.CanSeeOnMap) != 0;
                m_canModifyTheirObjects = (value & FriendRights.CanModifyObjects) != 0;
            }
        }

        #endregion Properties

        /// <summary>
        /// Used internally when building the initial list of friends at login time
        /// </summary>
        /// <param name="id">System ID of the avatar being prepesented</param>
        /// <param name="theirRights">Rights the friend has to see you online and to modify your objects</param>
        /// <param name="myRights">Rights you have to see your friend online and to modify their objects</param>
        internal FriendInfo(UUID id, FriendRights theirRights, FriendRights myRights)
        {
            m_id = id;
            m_canSeeMeOnline = (theirRights & FriendRights.CanSeeOnline) != 0;
            m_canSeeMeOnMap = (theirRights & FriendRights.CanSeeOnMap) != 0;
            m_canModifyMyObjects = (theirRights & FriendRights.CanModifyObjects) != 0;

            m_canSeeThemOnline = (myRights & FriendRights.CanSeeOnline) != 0;
            m_canSeeThemOnMap = (myRights & FriendRights.CanSeeOnMap) != 0;
            m_canModifyTheirObjects = (myRights & FriendRights.CanModifyObjects) != 0;
        }

        /// <summary>
        /// FriendInfo represented as a string
        /// </summary>
        /// <returns>A string reprentation of both my rights and my friends rights</returns>
        public override string ToString()
        {
            if (!String.IsNullOrEmpty(m_name))
                return String.Format("{0} (Their Rights: {1}, My Rights: {2})", m_name, TheirFriendRights,
                    MyFriendRights);
            else
                return String.Format("{0} (Their Rights: {1}, My Rights: {2})", m_id, TheirFriendRights,
                    MyFriendRights);
        }
    }

    /// <summary>
    /// This class is used to add and remove avatars from your friends list and to manage their permission.  
    /// </summary>
    public class FriendsManager
    {
        #region Delegates

        /// <summary>
        /// Triggered when an avatar in your friends list comes online
        /// </summary>
        /// <param name="friend"> System ID of the avatar</param>
        public delegate void FriendOnlineEvent(FriendInfo friend);
        /// <summary>
        /// Triggered when an avatar in your friends list goes offline
        /// </summary>
        /// <param name="friend"> System ID of the avatar</param>
        public delegate void FriendOfflineEvent(FriendInfo friend);
        /// <summary>
        /// Triggered in response to a call to the FriendRights() method, or when a friend changes your rights
        /// </summary>
        /// <param name="friend"> System ID of the avatar you changed the right of</param>
        public delegate void FriendRightsEvent(FriendInfo friend);
        /// <summary>
        /// Triggered when names on the friend list are received after the initial request upon login
        /// </summary>
        /// <param name="names"></param>
        public delegate void FriendNamesReceived(Dictionary<UUID, string> names);
        /// <summary>
        /// Triggered when someone offers you friendship
        /// </summary>
        /// <param name="agentID">System ID of the agent offering friendship</param>
        /// <param name="agentName">full name of the agent offereing friendship</param>
        /// <param name="imSessionID">session ID need when accepting/declining the offer</param>
        /// <returns>Return true to accept the friendship, false to deny it</returns>
        public delegate void FriendshipOfferedEvent(UUID agentID, string agentName, UUID imSessionID);
        /// <summary>
        /// Trigger when your friendship offer has been accepted or declined
        /// </summary>
        /// <param name="agentID">System ID of the avatar who accepted your friendship offer</param>
        /// <param name="agentName">Full name of the avatar who accepted your friendship offer</param>
        /// <param name="accepted">Whether the friendship request was accepted or declined</param>
        public delegate void FriendshipResponseEvent(UUID agentID, string agentName, bool accepted);
        /// <summary>
        /// Trigger when someone terminates your friendship.
        /// </summary>
        /// <param name="agentID">System ID of the avatar who terminated your friendship</param>
        /// <param name="agentName">Full name of the avatar who terminated your friendship</param>
        public delegate void FriendshipTerminatedEvent(UUID agentID, string agentName);

        /// <summary>
        /// Triggered in response to a FindFriend request
        /// </summary>
        /// <param name="agentID">Friends Key</param>
        /// <param name="regionHandle">region handle friend is in</param>
        /// <param name="location">X/Y location of friend</param>
        public delegate void FriendFoundEvent(UUID agentID, ulong regionHandle, Vector3 location);

        #endregion Delegates

        #region Events

        public event FriendNamesReceived OnFriendNamesReceived;
        public event FriendOnlineEvent OnFriendOnline;
        public event FriendOfflineEvent OnFriendOffline;
        public event FriendRightsEvent OnFriendRights;
        public event FriendshipOfferedEvent OnFriendshipOffered;
        public event FriendshipResponseEvent OnFriendshipResponse;
        public event FriendshipTerminatedEvent OnFriendshipTerminated;
        public event FriendFoundEvent OnFriendFound;

        #endregion Events

        private GridClient Client;
        /// <summary>
        /// A dictionary of key/value pairs containing known friends of this avatar. 
        /// 
        /// The Key is the <seealso cref="UUID"/> of the friend, the value is a <seealso cref="FriendInfo"/>
        /// object that contains detailed information including permissions you have and have given to the friend
        /// </summary>
        public InternalDictionary<UUID, FriendInfo> FriendList = new InternalDictionary<UUID, FriendInfo>();

        /// <summary>
        /// A Dictionary of key/value pairs containing current pending frienship offers.
        /// 
        /// The key is the <seealso cref="UUID"/> of the avatar making the request, 
        /// the value is the <seealso cref="UUID"/> of the request which is used to accept
        /// or decline the friendship offer
        /// </summary>
        public InternalDictionary<UUID, UUID> FriendRequests = new InternalDictionary<UUID, UUID>();

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="client">A reference to the GridClient Object</param>
        internal FriendsManager(GridClient client)
        {
            Client = client;

            Client.Network.OnConnected += new NetworkManager.ConnectedCallback(Network_OnConnect);
            Client.Avatars.OnAvatarNames += new AvatarManager.AvatarNamesCallback(Avatars_OnAvatarNames);
            Client.Self.OnInstantMessage += new AgentManager.InstantMessageCallback(MainAvatar_InstantMessage);

            Client.Network.RegisterCallback(PacketType.OnlineNotification, OnlineNotificationHandler);
            Client.Network.RegisterCallback(PacketType.OfflineNotification, OfflineNotificationHandler);
            Client.Network.RegisterCallback(PacketType.ChangeUserRights, ChangeUserRightsHandler);
            Client.Network.RegisterCallback(PacketType.TerminateFriendship, TerminateFriendshipHandler);
            Client.Network.RegisterCallback(PacketType.FindAgent, OnFindAgentReplyHandler);

            Client.Network.RegisterLoginResponseCallback(new NetworkManager.LoginResponseCallback(Network_OnLoginResponse),
                new string[] { "buddy-list" });
        }
        #region Public Methods
        
        /// <summary>
        /// Accept a friendship request
        /// </summary>
        /// <param name="fromAgentID">agentID of avatatar to form friendship with</param>
        /// <param name="imSessionID">imSessionID of the friendship request message</param>
        public void AcceptFriendship(UUID fromAgentID, UUID imSessionID)
        {
            UUID callingCardFolder = Client.Inventory.FindFolderForType(AssetType.CallingCard);

            AcceptFriendshipPacket request = new AcceptFriendshipPacket();
            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.TransactionBlock.TransactionID = imSessionID;
            request.FolderData = new AcceptFriendshipPacket.FolderDataBlock[1];
            request.FolderData[0] = new AcceptFriendshipPacket.FolderDataBlock();
            request.FolderData[0].FolderID = callingCardFolder;

            Client.Network.SendPacket(request);

            FriendInfo friend = new FriendInfo(fromAgentID, FriendRights.CanSeeOnline, 
                FriendRights.CanSeeOnline);

            if (!FriendList.ContainsKey(fromAgentID))
                FriendList.Add(friend.UUID, friend);

            if (FriendRequests.ContainsKey(fromAgentID))
                FriendRequests.Remove(fromAgentID);

            Client.Avatars.RequestAvatarName(fromAgentID);
        }

        /// <summary>
        /// Decline a friendship request
        /// </summary>
        /// <param name="fromAgentID"><seealso cref="UUID"/> of friend</param>
        /// <param name="imSessionID">imSessionID of the friendship request message</param>
        public void DeclineFriendship(UUID fromAgentID, UUID imSessionID)
        {
            DeclineFriendshipPacket request = new DeclineFriendshipPacket();
            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.TransactionBlock.TransactionID = imSessionID;
            Client.Network.SendPacket(request);

            if (FriendRequests.ContainsKey(fromAgentID))
                FriendRequests.Remove(fromAgentID);
        }

        /// <summary>
        /// Overload: Offer friendship to an avatar.
        /// </summary>
        /// <param name="agentID">System ID of the avatar you are offering friendship to</param>
        public void OfferFriendship(UUID agentID)
        {
            OfferFriendship(agentID, "Do ya wanna be my buddy?");
        }

        /// <summary>
        /// Offer friendship to an avatar.
        /// </summary>
        /// <param name="agentID">System ID of the avatar you are offering friendship to</param>
        /// <param name="message">A message to send with the request</param>
        public void OfferFriendship(UUID agentID, string message)
        {
            Client.Self.InstantMessage(Client.Self.Name,
                agentID,
                message,
                UUID.Random(),
                InstantMessageDialog.FriendshipOffered,
                InstantMessageOnline.Offline,
                Client.Self.SimPosition,
                Client.Network.CurrentSim.ID,
                null);
        }


        /// <summary>
        /// Terminate a friendship with an avatar
        /// </summary>
        /// <param name="agentID">System ID of the avatar you are terminating the friendship with</param>
        public void TerminateFriendship(UUID agentID)
        {
            if (FriendList.ContainsKey(agentID))
            {
                TerminateFriendshipPacket request = new TerminateFriendshipPacket();
                request.AgentData.AgentID = Client.Self.AgentID;
                request.AgentData.SessionID = Client.Self.SessionID;
                request.ExBlock.OtherID = agentID;

                Client.Network.SendPacket(request);

                if (FriendList.ContainsKey(agentID))
                    FriendList.Remove(agentID);
            }
        }
        /// <summary>
        /// Fired when another friend terminates friendship. We need to remove them from
        /// our cached list.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void TerminateFriendshipHandler(Packet packet, Simulator simulator)
        {
            TerminateFriendshipPacket itsOver = (TerminateFriendshipPacket)packet;
            string name = String.Empty;

            if (FriendList.ContainsKey(itsOver.ExBlock.OtherID))
            {
                name = FriendList[itsOver.ExBlock.OtherID].Name;
                FriendList.Remove(itsOver.ExBlock.OtherID);
            }

            if (OnFriendshipTerminated != null)
            {
                OnFriendshipTerminated(itsOver.ExBlock.OtherID, name);
            }
        }

        /// <summary>
        /// Change the rights of a friend avatar.
        /// </summary>
        /// <param name="friendID">the <seealso cref="UUID"/> of the friend</param>
        /// <param name="rights">the new rights to give the friend</param>
        /// <remarks>This method will implicitly set the rights to those passed in the rights parameter.</remarks>
        public void GrantRights(UUID friendID, FriendRights rights)
        {
            GrantUserRightsPacket request = new GrantUserRightsPacket();
            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.Rights = new GrantUserRightsPacket.RightsBlock[1];
            request.Rights[0] = new GrantUserRightsPacket.RightsBlock();
            request.Rights[0].AgentRelated = friendID;
            request.Rights[0].RelatedRights = (int)rights;

            Client.Network.SendPacket(request);
        }

        /// <summary>
        /// Use to map a friends location on the grid.
        /// </summary>
        /// <param name="friendID">Friends UUID to find</param>
        /// <remarks><seealso cref="E:OnFriendFound"/></remarks>
        public void MapFriend(UUID friendID)
        {
            FindAgentPacket stalk = new FindAgentPacket();
            stalk.AgentBlock.Hunter = Client.Self.AgentID;
            stalk.AgentBlock.Prey = friendID;
            stalk.AgentBlock.SpaceIP = 0; // Will be filled in by the simulator
            stalk.LocationBlock = new FindAgentPacket.LocationBlockBlock[1];
            stalk.LocationBlock[0] = new FindAgentPacket.LocationBlockBlock();
            stalk.LocationBlock[0].GlobalX = 0.0; // Filled in by the simulator
            stalk.LocationBlock[0].GlobalY = 0.0;

            Client.Network.SendPacket(stalk);
        }

        /// <summary>
        /// Use to track a friends movement on the grid
        /// </summary>
        /// <param name="friendID">Friends Key</param>
        public void TrackFriend(UUID friendID)
        {
            TrackAgentPacket stalk = new TrackAgentPacket();
            stalk.AgentData.AgentID = Client.Self.AgentID;
            stalk.AgentData.SessionID = Client.Self.SessionID;
            stalk.TargetData.PreyID = friendID;

            Client.Network.SendPacket(stalk);
        }

        /// <summary>
        /// Ask for a notification of friend's online status
        /// </summary>
        /// <param name="friendID">Friend's UUID</param>
        public void RequestOnlineNotification(UUID friendID)
        {
            GenericMessagePacket gmp = new GenericMessagePacket();

            gmp.AgentData.AgentID = Client.Self.AgentID;
            gmp.AgentData.SessionID = Client.Self.SessionID;
            gmp.AgentData.TransactionID = UUID.Zero;

            gmp.MethodData.Method = Utils.StringToBytes("requestonlinenotification");
            gmp.MethodData.Invoice = UUID.Zero;
            gmp.ParamList = new GenericMessagePacket.ParamListBlock[1];
            gmp.ParamList[0] = new GenericMessagePacket.ParamListBlock();
            gmp.ParamList[0].Parameter = Utils.StringToBytes(friendID.ToString());

            Client.Network.SendPacket(gmp);
        }

        #endregion

        #region Internal events
        /// <summary>
        /// Called when a connection to the SL server is established.  The list of friend avatars 
        /// is populated from XML returned by the login server.  That list contains the avatar's id 
        /// and right, but no names.  Here is where those names are requested.
        /// </summary>
        /// <param name="sender"></param>
        private void Network_OnConnect(object sender)
        {
            List<UUID> names = new List<UUID>();

            if (FriendList.Count > 0)
            {
                FriendList.ForEach(
                    delegate(KeyValuePair<UUID, FriendInfo> kvp)
                    {
                        if (String.IsNullOrEmpty(kvp.Value.Name))
                            names.Add(kvp.Key);
                    }
                );

                Client.Avatars.RequestAvatarNames(names);
            }
        }


        /// <summary>
        /// This handles the asynchronous response of a RequestAvatarNames call.
        /// </summary>
        /// <param name="names">names cooresponding to the the list of IDs sent the the RequestAvatarNames call.</param>
        private void Avatars_OnAvatarNames(Dictionary<UUID, string> names)
        {
            Dictionary<UUID, string> newNames = new Dictionary<UUID, string>();

            foreach (KeyValuePair<UUID, string> kvp in names)
            {
                FriendInfo friend;
                lock (FriendList.Dictionary)
                {
                    if (FriendList.TryGetValue(kvp.Key, out friend))
                    {
                        if (friend.Name == null)
                            newNames.Add(kvp.Key, names[kvp.Key]);

                        friend.Name = names[kvp.Key];
                        FriendList[kvp.Key] = friend;
                    }
                }
            }

            if (newNames.Count > 0 && OnFriendNamesReceived != null)
            {
                try { OnFriendNamesReceived(newNames); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }
        #endregion

        #region Packet Handlers

        /// <summary>
        /// Handle notifications sent when a friends has come online.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void OnlineNotificationHandler(Packet packet, Simulator simulator)
        {
            if (packet.Type == PacketType.OnlineNotification)
            {
                OnlineNotificationPacket notification = ((OnlineNotificationPacket)packet);

                foreach (OnlineNotificationPacket.AgentBlockBlock block in notification.AgentBlock)
                {
                    FriendInfo friend;
                    lock (FriendList.Dictionary)
                    {
                        if (!FriendList.ContainsKey(block.AgentID))
                        {
                            friend = new FriendInfo(block.AgentID, FriendRights.CanSeeOnline,
                                FriendRights.CanSeeOnline);
                            FriendList.Add(block.AgentID, friend);
                        }
                        else
                        {
                            friend = FriendList[block.AgentID];
                        }
                    }

                    bool doNotify = !friend.IsOnline;
                    friend.IsOnline = true;

                    if (OnFriendOnline != null && doNotify)
                    {
                        try { OnFriendOnline(friend); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                    }
                }
            }
        }

        /// <summary>
        /// Handle notifications sent when a friends has gone offline.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void OfflineNotificationHandler(Packet packet, Simulator simulator)
        {
            if (packet.Type == PacketType.OfflineNotification)
            {
                OfflineNotificationPacket notification = ((OfflineNotificationPacket)packet);

                foreach (OfflineNotificationPacket.AgentBlockBlock block in notification.AgentBlock)
                {
                    FriendInfo friend = new FriendInfo(block.AgentID, FriendRights.CanSeeOnline, FriendRights.CanSeeOnline);

                    lock (FriendList.Dictionary)
                    {
                        if (!FriendList.Dictionary.ContainsKey(block.AgentID))
                            FriendList.Dictionary[block.AgentID] = friend;

                        friend = FriendList.Dictionary[block.AgentID];
                    }

                    friend.IsOnline = false;

                    if (OnFriendOffline != null)
                    {
                        try { OnFriendOffline(friend); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                    }
                }
            }
        }


        /// <summary>
        /// Handle notifications sent when a friend rights change.  This notification is also received
        /// when my own rights change.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void ChangeUserRightsHandler(Packet packet, Simulator simulator)
        {
            if (packet.Type == PacketType.ChangeUserRights)
            {
                FriendInfo friend;
                ChangeUserRightsPacket rights = (ChangeUserRightsPacket)packet;

                foreach (ChangeUserRightsPacket.RightsBlock block in rights.Rights)
                {
                    FriendRights newRights = (FriendRights)block.RelatedRights;
                    if (FriendList.TryGetValue(block.AgentRelated, out friend))
                    {
                        friend.TheirFriendRights = newRights;
                        if (OnFriendRights != null)
                        {
                            try { OnFriendRights(friend); }
                            catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                        }
                    }
                    else if (block.AgentRelated == Client.Self.AgentID)
                    {
                        if (FriendList.TryGetValue(rights.AgentData.AgentID, out friend))
                        {
                            friend.MyFriendRights = newRights;
                            if (OnFriendRights != null)
                            {
                                try { OnFriendRights(friend); }
                                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handle friend location updates
        /// </summary>
        /// <param name="packet">The Packet</param>
        /// <param name="simulator">The Simulator</param>
        public void OnFindAgentReplyHandler(Packet packet, Simulator simulator)
        {
            if(OnFriendFound != null)
            {
            FindAgentPacket reply = (FindAgentPacket)packet;

            float x,y;
            UUID prey = reply.AgentBlock.Prey;
            ulong regionHandle = Helpers.GlobalPosToRegionHandle((float)reply.LocationBlock[0].GlobalX, 
                (float)reply.LocationBlock[0].GlobalY, out x, out y);
            Vector3 xyz = new Vector3(x, y, 0f);

            try { OnFriendFound(prey, regionHandle, xyz); }
            catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        #endregion

        /// <summary>
        /// Handles relevant messages from the server encapsulated in instant messages.
        /// </summary>
        /// <param name="im">InstantMessage object containing encapsalated instant message</param>
        /// <param name="simulator">Originating Simulator</param>
        private void MainAvatar_InstantMessage(InstantMessage im, Simulator simulator)
        {
            if (im.Dialog == InstantMessageDialog.FriendshipOffered)
            {
                if (OnFriendshipOffered != null)
                {
                    if (FriendRequests.ContainsKey(im.FromAgentID))
                        FriendRequests[im.FromAgentID] = im.IMSessionID;
                    else
                        FriendRequests.Add(im.FromAgentID, im.IMSessionID);

                    try { OnFriendshipOffered(im.FromAgentID, im.FromAgentName, im.IMSessionID); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }
            else if (im.Dialog == InstantMessageDialog.FriendshipAccepted)
            {
                FriendInfo friend = new FriendInfo(im.FromAgentID, FriendRights.CanSeeOnline,
                    FriendRights.CanSeeOnline);
                friend.Name = im.FromAgentName;
                lock (FriendList.Dictionary) FriendList[friend.UUID] = friend;

                if (OnFriendshipResponse != null)
                {
                    try { OnFriendshipResponse(im.FromAgentID, im.FromAgentName, true); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
                RequestOnlineNotification(im.FromAgentID);
            }
            else if (im.Dialog == InstantMessageDialog.FriendshipDeclined)
            {
                if (OnFriendshipResponse != null)
                {
                    try { OnFriendshipResponse(im.FromAgentID, im.FromAgentName, false); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }
        }

        /// <summary>
        /// Populate FriendList <seealso cref="InternalDictionary"/> with data from the login reply
        /// </summary>
        /// <param name="loginSuccess">true if login was successful</param>
        /// <param name="redirect">true if login request is requiring a redirect</param>
        /// <param name="message">A string containing the response to the login request</param>
        /// <param name="reason">A string containing the reason for the request</param>
        /// <param name="replyData">A <seealso cref="LoginResponseData"/> object containing the decoded 
        /// reply from the login server</param>
        private void Network_OnLoginResponse(bool loginSuccess, bool redirect, string message, string reason,
            LoginResponseData replyData)
        {
            if (loginSuccess && replyData.BuddyList != null)
            {
                if (replyData.BuddyList != null)
                {
                    foreach (BuddyListEntry buddy in replyData.BuddyList)
                    {
                        UUID bubid = UUID.Parse(buddy.buddy_id);
                        lock (FriendList.Dictionary)
                        {
                            if (!FriendList.ContainsKey(bubid))
                            {
                                FriendList.Add(bubid,
                                new FriendInfo(UUID.Parse(buddy.buddy_id),
                                (FriendRights)buddy.buddy_rights_given,
                                (FriendRights)buddy.buddy_rights_has));
                            }
                        }
                    }
                }
            }
        }
    }
}
