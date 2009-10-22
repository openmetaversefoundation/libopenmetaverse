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
using System.Text;
using System.Collections.Generic;
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
        
        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<FriendInfoEventArgs> m_FriendOnline;

        /// <summary>Raises the FriendOnline event</summary>
        /// <param name="e">A FriendInfoEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnFriendOnline(FriendInfoEventArgs e)
        {
            EventHandler<FriendInfoEventArgs> handler = m_FriendOnline;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_FriendOnlineLock = new object();

        /// <summary>Raised when the simulator sends notification one of the members in our friends list comes online</summary>
        public event EventHandler<FriendInfoEventArgs> FriendOnline
        {
            add { lock (m_FriendOnlineLock) { m_FriendOnline += value; } }
            remove { lock (m_FriendOnlineLock) { m_FriendOnline -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<FriendInfoEventArgs> m_FriendOffline;

        /// <summary>Raises the FriendOffline event</summary>
        /// <param name="e">A FriendInfoEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnFriendOffline(FriendInfoEventArgs e)
        {
            EventHandler<FriendInfoEventArgs> handler = m_FriendOffline;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_FriendOfflineLock = new object();

        /// <summary>Raised when the simulator sends notification one of the members in our friends list goes offline</summary>
        public event EventHandler<FriendInfoEventArgs> FriendOffline
        {
            add { lock (m_FriendOfflineLock) { m_FriendOffline += value; } }
            remove { lock (m_FriendOfflineLock) { m_FriendOffline -= value; } }
        }
        
        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<FriendInfoEventArgs> m_FriendRights;

        /// <summary>Raises the FriendRightsUpdate event</summary>
        /// <param name="e">A FriendInfoEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnFriendRights(FriendInfoEventArgs e)
        {
            EventHandler<FriendInfoEventArgs> handler = m_FriendRights;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_FriendRightsLock = new object();

        /// <summary>Raised when the simulator sends notification one of the members in our friends list grants or revokes permissions</summary>
        public event EventHandler<FriendInfoEventArgs> FriendRightsUpdate
        {
            add { lock (m_FriendRightsLock) { m_FriendRights += value; } }
            remove { lock (m_FriendRightsLock) { m_FriendRights -= value; } }
        }        

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<FriendNamesEventArgs> m_FriendNames;

        /// <summary>Raises the FriendNames event</summary>
        /// <param name="e">A FriendNamesEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnFriendNames(FriendNamesEventArgs e)
        {
            EventHandler<FriendNamesEventArgs> handler = m_FriendNames;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_FriendNamesLock = new object();

        /// <summary>Raised when the simulator sends us the names on our friends list</summary>
        public event EventHandler<FriendNamesEventArgs> FriendNames
        {
            add { lock (m_FriendNamesLock) { m_FriendNames += value; } }
            remove { lock (m_FriendNamesLock) { m_FriendNames -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<FriendshipOfferedEventArgs> m_FriendshipOffered;

        /// <summary>Raises the FriendshipOffered event</summary>
        /// <param name="e">A FriendshipOfferedEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnFriendshipOffered(FriendshipOfferedEventArgs e)
        {
            EventHandler<FriendshipOfferedEventArgs> handler = m_FriendshipOffered;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_FriendshipOfferedLock = new object();

        /// <summary>Raised when the simulator sends notification another agent is offering us friendship</summary>
        public event EventHandler<FriendshipOfferedEventArgs> FriendshipOffered
        {
            add { lock (m_FriendshipOfferedLock) { m_FriendshipOffered += value; } }
            remove { lock (m_FriendshipOfferedLock) { m_FriendshipOffered -= value; } }
        }        

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<FriendshipResponseEventArgs> m_FriendshipResponse;

        /// <summary>Raises the FriendshipResponse event</summary>
        /// <param name="e">A FriendshipResponseEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnFriendshipResponse(FriendshipResponseEventArgs e)
        {
            EventHandler<FriendshipResponseEventArgs> handler = m_FriendshipResponse;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_FriendshipResponseLock = new object();

        /// <summary>Raised when a request we sent to friend another agent is accepted or declined</summary>
        public event EventHandler<FriendshipResponseEventArgs> FriendshipResponse
        {
            add { lock (m_FriendshipResponseLock) { m_FriendshipResponse += value; } }
            remove { lock (m_FriendshipResponseLock) { m_FriendshipResponse -= value; } }
        }
        
        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<FriendshipTerminatedEventArgs> m_FriendshipTerminated;

        /// <summary>Raises the FriendshipTerminated event</summary>
        /// <param name="e">A FriendshipTerminatedEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnFriendshipTerminated(FriendshipTerminatedEventArgs e)
        {
            EventHandler<FriendshipTerminatedEventArgs> handler = m_FriendshipTerminated;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_FriendshipTerminatedLock = new object();

        /// <summary>Raised when the simulator sends notification one of the members in our friends list has terminated 
        /// our friendship</summary>
        public event EventHandler<FriendshipTerminatedEventArgs> FriendshipTerminated
        {
            add { lock (m_FriendshipTerminatedLock) { m_FriendshipTerminated += value; } }
            remove { lock (m_FriendshipTerminatedLock) { m_FriendshipTerminated -= value; } }
        }        

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<FriendFoundReplyEventArgs> m_FriendFound;

        /// <summary>Raises the FriendFoundReply event</summary>
        /// <param name="e">A FriendFoundReplyEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnFriendFoundReply(FriendFoundReplyEventArgs e)
        {
            EventHandler<FriendFoundReplyEventArgs> handler = m_FriendFound;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_FriendFoundLock = new object();

        /// <summary>Raised when the simulator sends the location of a friend we have 
        /// requested map location info for</summary>
        public event EventHandler<FriendFoundReplyEventArgs> FriendFoundReply
        {
            add { lock (m_FriendFoundLock) { m_FriendFound += value; } }
            remove { lock (m_FriendFoundLock) { m_FriendFound -= value; } }
        }

        #endregion Delegates

        #region Events

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
            Client.Avatars.UUIDNameReply += new EventHandler<UUIDNameReplyEventArgs>(Avatars_OnAvatarNames);
            Client.Self.IM += Self_IM;

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

            if (m_FriendshipTerminated != null)
            {
                OnFriendshipTerminated(new FriendshipTerminatedEventArgs(itsOver.ExBlock.OtherID, name));
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
        /// <param name="sender"></param>
        /// <param name="e">names cooresponding to the the list of IDs sent the the RequestAvatarNames call.</param>
        private void Avatars_OnAvatarNames(object sender, UUIDNameReplyEventArgs e)
        {
            Dictionary<UUID, string> newNames = new Dictionary<UUID, string>();

            foreach (KeyValuePair<UUID, string> kvp in e.Names)
            {
                FriendInfo friend;
                lock (FriendList.Dictionary)
                {
                    if (FriendList.TryGetValue(kvp.Key, out friend))
                    {
                        if (friend.Name == null)
                            newNames.Add(kvp.Key, e.Names[kvp.Key]);

                        friend.Name = e.Names[kvp.Key];
                        FriendList[kvp.Key] = friend;
                    }
                }
            }

            if (newNames.Count > 0 && m_FriendNames != null)
            {
                OnFriendNames(new FriendNamesEventArgs(newNames));
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

                    if (m_FriendOnline != null && doNotify)
                    {
                        OnFriendOnline(new FriendInfoEventArgs(friend));
                    }
                }
            }
        }

        /// <summary>
        /// Handle notifications sent when a friends has gone offline.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        protected void OfflineNotificationHandler(Packet packet, Simulator simulator)
        {
            if (packet.Type == PacketType.OfflineNotification)
            {
                OfflineNotificationPacket notification = (OfflineNotificationPacket)packet;

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

                    if (m_FriendOffline != null)
                    {
                        OnFriendOffline(new FriendInfoEventArgs(friend));
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
                        if (m_FriendRights != null)
                        {
                            OnFriendRights(new FriendInfoEventArgs(friend));
                        }
                    }
                    else if (block.AgentRelated == Client.Self.AgentID)
                    {
                        if (FriendList.TryGetValue(rights.AgentData.AgentID, out friend))
                        {
                            friend.MyFriendRights = newRights;
                            if (m_FriendRights != null)
                            {
                                OnFriendRights(new FriendInfoEventArgs(friend));
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
            if (m_FriendFound != null)
            {
                FindAgentPacket reply = (FindAgentPacket)packet;

                float x, y;
                UUID prey = reply.AgentBlock.Prey;
                ulong regionHandle = Helpers.GlobalPosToRegionHandle((float)reply.LocationBlock[0].GlobalX,
                    (float)reply.LocationBlock[0].GlobalY, out x, out y);
                Vector3 xyz = new Vector3(x, y, 0f);

                OnFriendFoundReply(new FriendFoundReplyEventArgs(prey, regionHandle, xyz));
            }
        }

        #endregion

        void Self_IM(object sender, InstantMessageEventArgs e)
        {
            if (e.IM.Dialog == InstantMessageDialog.FriendshipOffered)
            {
                if (m_FriendshipOffered != null)
                {
                    if (FriendRequests.ContainsKey(e.IM.FromAgentID))
                        FriendRequests[e.IM.FromAgentID] = e.IM.IMSessionID;
                    else
                        FriendRequests.Add(e.IM.FromAgentID, e.IM.IMSessionID);

                    OnFriendshipOffered(new FriendshipOfferedEventArgs(e.IM.FromAgentID, e.IM.FromAgentName, e.IM.IMSessionID));
                }
            }
            else if (e.IM.Dialog == InstantMessageDialog.FriendshipAccepted)
            {
                FriendInfo friend = new FriendInfo(e.IM.FromAgentID, FriendRights.CanSeeOnline,
                    FriendRights.CanSeeOnline);
                friend.Name = e.IM.FromAgentName;
                lock (FriendList.Dictionary) FriendList[friend.UUID] = friend;

                if (m_FriendshipResponse != null)
                {
                    OnFriendshipResponse(new FriendshipResponseEventArgs(e.IM.FromAgentID, e.IM.FromAgentName, true));
                }
                RequestOnlineNotification(e.IM.FromAgentID);
            }
            else if (e.IM.Dialog == InstantMessageDialog.FriendshipDeclined)
            {
                if (m_FriendshipResponse != null)
                {
                    OnFriendshipResponse(new FriendshipResponseEventArgs(e.IM.FromAgentID, e.IM.FromAgentName, false));
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
    #region EventArgs

    /// <summary>Contains information on a member of our friends list</summary>
    public class FriendInfoEventArgs : EventArgs
    {
        private readonly FriendInfo m_Friend;

        /// <summary>Get the FriendInfo</summary>
        public FriendInfo Friend { get { return m_Friend; } }

        /// <summary>
        /// Construct a new instance of the FriendInfoEventArgs class
        /// </summary>
        /// <param name="friend">The FriendInfo</param>
        public FriendInfoEventArgs(FriendInfo friend)
        {
            this.m_Friend = friend;
        }
    }

    /// <summary>Contains Friend Names</summary>
    public class FriendNamesEventArgs : EventArgs
    {
        private readonly Dictionary<UUID, string> m_Names;

        /// <summary>A dictionary where the Key is the ID of the Agent, 
        /// and the Value is a string containing their name</summary>
        public Dictionary<UUID, string> Names { get { return m_Names; } }

        /// <summary>
        /// Construct a new instance of the FriendNamesEventArgs class
        /// </summary>
        /// <param name="names">A dictionary where the Key is the ID of the Agent, 
        /// and the Value is a string containing their name</param>
        public FriendNamesEventArgs(Dictionary<UUID, string> names)
        {
            this.m_Names = names;
        }
    }
    
    /// <summary>Sent when another agent requests a friendship with our agent</summary>
    public class FriendshipOfferedEventArgs : EventArgs
    {
        private readonly UUID m_AgentID;
        private readonly string m_AgentName;
        private readonly UUID m_SessionID;

        /// <summary>Get the ID of the agent requesting friendship</summary>
        public UUID AgentID { get { return m_AgentID; } }
        /// <summary>Get the name of the agent requesting friendship</summary>
        public string AgentName { get { return m_AgentName; } }
        /// <summary>Get the ID of the session, used in accepting or declining the 
        /// friendship offer</summary>
        public UUID SessionID { get { return m_SessionID; } }

        /// <summary>
        /// Construct a new instance of the FriendshipOfferedEventArgs class
        /// </summary>
        /// <param name="agentID">The ID of the agent requesting friendship</param>
        /// <param name="agentName">The name of the agent requesting friendship</param>
        /// <param name="imSessionID">The ID of the session, used in accepting or declining the 
        /// friendship offer</param>
        public FriendshipOfferedEventArgs(UUID agentID, string agentName, UUID imSessionID)
        {
            this.m_AgentID = agentID;
            this.m_AgentName = agentName;
            this.m_SessionID = imSessionID;
        }
    }
    
    /// <summary>A response containing the results of our request to form a friendship with another agent</summary>
    public class FriendshipResponseEventArgs : EventArgs
    {
        private readonly UUID m_AgentID;
        private readonly string m_AgentName;
        private readonly bool m_Accepted;

        /// <summary>Get the ID of the agent we requested a friendship with</summary>
        public UUID AgentID { get { return m_AgentID; } }
        /// <summary>Get the name of the agent we requested a friendship with</summary>
        public string AgentName { get { return m_AgentName; } }
        /// <summary>true if the agent accepted our friendship offer</summary>
        public bool Accepted { get { return m_Accepted; } }

        /// <summary>
        /// Construct a new instance of the FriendShipResponseEventArgs class
        /// </summary>
        /// <param name="agentID">The ID of the agent we requested a friendship with</param>
        /// <param name="agentName">The name of the agent we requested a friendship with</param>
        /// <param name="accepted">true if the agent accepted our friendship offer</param>
        public FriendshipResponseEventArgs(UUID agentID, string agentName, bool accepted)
        {
            this.m_AgentID = agentID;
            this.m_AgentName = agentName;
            this.m_Accepted = accepted;
        }
    }
    
    /// <summary>Contains data sent when a friend terminates a friendship with us</summary>
    public class FriendshipTerminatedEventArgs : EventArgs
    {
        private readonly UUID m_AgentID;
        private readonly string m_AgentName;

        /// <summary>Get the ID of the agent that terminated the friendship with us</summary>
        public UUID AgentID { get { return m_AgentID; } }
        /// <summary>Get the name of the agent that terminated the friendship with us</summary>
        public string AgentName { get { return m_AgentName; } }

        /// <summary>
        /// Construct a new instance of the FrindshipTerminatedEventArgs class
        /// </summary>
        /// <param name="agentID">The ID of the friend who terminated the friendship with us</param>
        /// <param name="agentName">The name of the friend who terminated the friendship with us</param>
        public FriendshipTerminatedEventArgs(UUID agentID, string agentName)
        {
            this.m_AgentID = agentID;
            this.m_AgentName = agentName;
        }
    }
    
    /// <summary>
    /// Data sent in response to a <see cref="FindFriend"/> request which contains the information to allow us to map the friends location
    /// </summary>
    public class FriendFoundReplyEventArgs : EventArgs
    {
        private readonly UUID m_AgentID;
        private readonly ulong m_RegionHandle;
        private readonly Vector3 m_Location;

        /// <summary>Get the ID of the agent we have received location information for</summary>
        public UUID AgentID { get { return m_AgentID; } }
        /// <summary>Get the region handle where our mapped friend is located</summary>
        public ulong RegionHandle { get { return m_RegionHandle; } }
        /// <summary>Get the simulator local position where our friend is located</summary>
        public Vector3 Location { get { return m_Location; } }

        /// <summary>
        /// Construct a new instance of the FriendFoundReplyEventArgs class
        /// </summary>
        /// <param name="agentID">The ID of the agent we have requested location information for</param>
        /// <param name="regionHandle">The region handle where our friend is located</param>
        /// <param name="location">The simulator local position our friend is located</param>
        public FriendFoundReplyEventArgs(UUID agentID, ulong regionHandle, Vector3 location)
        {
            this.m_AgentID = agentID;
            this.m_RegionHandle = regionHandle;
            this.m_Location = location;
        }
    }
    #endregion
}
