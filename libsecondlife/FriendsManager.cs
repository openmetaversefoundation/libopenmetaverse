/*
 * Copyright (c) 2007, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
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
using System.Text;
using libsecondlife.Packets;

namespace libsecondlife
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
    /// way clients of this class will use it.  The second interface is through two bitmap properties.  While 
    /// the bitmap interface is public, it is intended for use the libsecondlife framework.
    /// </summary>
    public class FriendInfo
    {
        private LLUUID m_id;
        private string m_name;
        private bool m_isOnline;
        private bool m_canSeeMeOnline;
        private bool m_canSeeMeOnMap;
        private bool m_canModifyMyObjects;
        private bool m_canSeeThemOnline;
        private bool m_canSeeThemOnMap;
        private bool m_canModifyTheirObjects;

        /// <summary>
        /// Used by the libsecondlife framework when building the initial list of friends
        /// at login time.  This constructor should not be called by consummer of this class.
        /// </summary>
        /// <param name="id">System ID of the avatar being prepesented</param>
        /// <param name="theirRights">Rights the friend has to see you online and to modify your objects</param>
        /// <param name="myRights">Rights you have to see your friend online and to modify their objects</param>
        public FriendInfo(LLUUID id, FriendRights theirRights, FriendRights myRights)
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
        /// System ID of the avatar
        /// </summary>
        public LLUUID UUID { get { return m_id; } }

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
        /// Triggered in response to a call to the GrantRighs() method, or when a friend changes your rights
        /// </summary>
        /// <param name="friend"> System ID of the avatar you changed the right of</param>
        public delegate void FriendRightsEvent(FriendInfo friend);
        /// <summary>
        /// Triggered when someone offers you friendship
        /// </summary>
        /// <param name="agentID">System ID of the agent offering friendship</param>
        /// <param name="agentName">full name of the agent offereing friendship</param>
        /// <param name="IMSessionID">session ID need when accepting/declining the offer</param>
        /// <returns>Return true to accept the friendship, false to deny it</returns>
        public delegate void FriendshipOfferedEvent(LLUUID agentID, string agentName, LLUUID imSessionID);
        /// <summary>
        /// Trigger when your friendship offer has been accepted or declined
        /// </summary>
        /// <param name="agentID">System ID of the avatar who accepted your friendship offer</param>
        /// <param name="agentName">Full name of the avatar who accepted your friendship offer</param>
        /// <param name="accepted">Whether the friendship request was accepted or declined</param>
        public delegate void FriendshipResponseEvent(LLUUID agentID, string agentName, bool accepted);
        /// <summary>
        /// Trigger when someone terminates your friendship.
        /// </summary>
        /// <param name="agentID">System ID of the avatar who terminated your friendship</param>
        /// <param name="agentName">Full name of the avatar who terminated your friendship</param>
        public delegate void FriendshipTerminatedEvent(LLUUID agentID, string agentName);

        /// <summary>
        /// Triggered in response to a FindFriend request
        /// </summary>
        /// <param name="agentID">Friends Key</param>
        /// <param name="regionHandle">region handle friend is in</param>
        /// <param name="location">X/Y location of friend</param>
        public delegate void FriendFoundEvent(LLUUID agentID, ulong regionHandle, LLVector3 location);

        #endregion Delegates

        #region Events

        public event FriendOnlineEvent OnFriendOnline;
        public event FriendOfflineEvent OnFriendOffline;
        public event FriendRightsEvent OnFriendRights;
        public event FriendshipOfferedEvent OnFriendshipOffered;
        public event FriendshipResponseEvent OnFriendshipResponse;
        public event FriendshipTerminatedEvent OnFriendshipTerminated;
        public event FriendFoundEvent OnFriendFound;

        #endregion Events

        private SecondLife Client;
        private Dictionary<LLUUID, FriendInfo> _Friends = new Dictionary<LLUUID, FriendInfo>();
        private Dictionary<LLUUID, LLUUID> _Requests = new Dictionary<LLUUID, LLUUID>();

        /// <summary>
        /// This constructor is intened to for use only the the libsecondlife framework
        /// </summary>
        /// <param name="client"></param>
        public FriendsManager(SecondLife client)
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
        /// Get a list of all the friends we are currently aware of
        /// </summary>
        /// <remarks>
        /// This function performs a shallow copy from the internal dictionary
        /// in FriendsManager. Avoid calling it multiple times when it is not 
        /// necessary to as it can be expensive memory-wise
        /// </remarks>
        public List<FriendInfo> FriendsList()
        {
            List<FriendInfo> friends = new List<FriendInfo>();

            lock (_Friends)
            {
                foreach (FriendInfo info in _Friends.Values)
                    friends.Add(info);
            }

            return friends;
        }

        /// <summary>
        /// Dictionary of unanswered friendship offers
        /// </summary>
        public Dictionary<LLUUID, LLUUID> PendingOffers()
        {
            Dictionary<LLUUID, LLUUID> requests = new Dictionary<LLUUID,LLUUID>();

            lock (_Requests)
            {
                foreach(KeyValuePair<LLUUID, LLUUID> req in _Requests)
                    requests.Add(req.Key, req.Value);
            }

            return requests;
        }

        /// <summary>
        /// Accept a friendship request
        /// </summary>
        /// <param name="imSessionID">imSessionID of the friendship request message</param>
        public void AcceptFriendship(LLUUID fromAgentID, LLUUID imSessionID)
        {
            LLUUID callingCardFolder = Client.Inventory.FindFolderForType(AssetType.CallingCard);

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
            lock (_Friends)
            {
                if(!_Friends.ContainsKey(fromAgentID))  _Friends.Add(friend.UUID, friend);
            }
            lock (_Requests) { if (_Requests.ContainsKey(fromAgentID)) _Requests.Remove(fromAgentID); }

            Client.Avatars.RequestAvatarName(fromAgentID);
        }

        /// <summary>
        /// Decline a friendship request
        /// </summary>
        /// <param name="imSessionID">imSessionID of the friendship request message</param>
        public void DeclineFriendship(LLUUID fromAgentID, LLUUID imSessionID)
        {
            DeclineFriendshipPacket request = new DeclineFriendshipPacket();
            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.TransactionBlock.TransactionID = imSessionID;
            Client.Network.SendPacket(request);

            lock (_Requests) { if (_Requests.ContainsKey(fromAgentID)) _Requests.Remove(fromAgentID); }
        }

        /// <summary>
        /// Offer friendship to an avatar.
        /// </summary>
        /// <param name="agentID">System ID of the avatar you are offering friendship to</param>
        public void OfferFriendship(LLUUID agentID)
        {
            // HACK: folder id stored as "message"
            LLUUID callingCardFolder = Client.Inventory.FindFolderForType(AssetType.CallingCard);
            Client.Self.InstantMessage(Client.Self.Name,
                agentID,
                callingCardFolder.ToString(),
                LLUUID.Random(),
                InstantMessageDialog.FriendshipOffered,
                InstantMessageOnline.Online,
                Client.Self.SimPosition,
                Client.Network.CurrentSim.ID,
                new byte[0]);
        }


        /// <summary>
        /// Terminate a friendship with an avatar
        /// </summary>
        /// <param name="agentID">System ID of the avatar you are terminating the friendship with</param>
        public void TerminateFriendship(LLUUID agentID)
        {
            if (_Friends.ContainsKey(agentID))
            {
                TerminateFriendshipPacket request = new TerminateFriendshipPacket();
                request.AgentData.AgentID = Client.Self.AgentID;
                request.AgentData.SessionID = Client.Self.SessionID;
                request.ExBlock.OtherID = agentID;

                Client.Network.SendPacket(request);

                lock (_Friends)
                {
                    if (_Friends.ContainsKey(agentID))
                        _Friends.Remove(agentID);
                }
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
            lock (_Friends)
            {
                if (_Friends.ContainsKey(itsOver.ExBlock.OtherID))
                {
                    name = _Friends[itsOver.ExBlock.OtherID].Name;
                    _Friends.Remove(itsOver.ExBlock.OtherID);
                }
            }
            if (OnFriendshipTerminated != null)
            {
                OnFriendshipTerminated(itsOver.ExBlock.OtherID, name);
            }
        }
        /// <summary>
        /// Change the rights of a friend avatar.  To use this routine, first change the right of the
        /// avatar stored in the item property.
        /// </summary>
        /// <param name="agentID">System ID of the avatar you are changing the rights of</param>
        public void GrantRights(LLUUID agentID)
        {
            GrantUserRightsPacket request = new GrantUserRightsPacket();
            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.Rights = new GrantUserRightsPacket.RightsBlock[1];
            request.Rights[0] = new GrantUserRightsPacket.RightsBlock();
            request.Rights[0].AgentRelated = agentID;
            request.Rights[0].RelatedRights = (int)(_Friends[agentID].TheirFriendRights);

            Client.Network.SendPacket(request);
        }

        /// <summary>
        /// Use to map a friends location on the grid.
        /// </summary>
        /// <param name="friendID">Friends UUID to find</param>
        /// <remarks><seealso cref="E:OnFriendFound"/></remarks>
        public void MapFriend(LLUUID friendID)
        {
            FindAgentPacket stalk = new FindAgentPacket();
            stalk.AgentBlock.Hunter = Client.Self.AgentID;
            stalk.AgentBlock.Prey = friendID;
            Console.WriteLine(stalk.ToString());

            Client.Network.SendPacket(stalk);
        }

        /// <summary>
        /// Use to track a friends movement on the grid
        /// </summary>
        /// <param name="friendID">Friends Key</param>
        public void TrackFriend(LLUUID friendID)
        {
            TrackAgentPacket stalk = new TrackAgentPacket();
            stalk.AgentData.AgentID = Client.Self.AgentID;
            stalk.AgentData.SessionID = Client.Self.SessionID;
            stalk.TargetData.PreyID = friendID;

            Console.WriteLine(stalk.ToString());

            Client.Network.SendPacket(stalk);
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
            List<LLUUID> names = new List<LLUUID>();

            if ( _Friends.Count > 0 )
            {
                lock (_Friends)
                {
                    foreach (KeyValuePair<LLUUID, FriendInfo> kvp in _Friends)
                    {
                        if (String.IsNullOrEmpty(kvp.Value.Name))
                            names.Add(kvp.Key);
                    }
                }

                Client.Avatars.RequestAvatarNames(names);
            }
        }


        /// <summary>
        /// This handles the asynchronous response of a RequestAvatarNames call.
        /// </summary>
        /// <param name="names">names cooresponding to the the list of IDs sent the the RequestAvatarNames call.</param>
        private void Avatars_OnAvatarNames(Dictionary<LLUUID, string> names)
        {
            lock (_Friends)
            {
                foreach (KeyValuePair<LLUUID, string> kvp in names)
                {
                    if (_Friends.ContainsKey(kvp.Key))
                        _Friends[kvp.Key].Name = names[kvp.Key];
                }
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

                    lock (_Friends)
                    {
                        if (!_Friends.ContainsKey(block.AgentID))
                        {
                            friend = new FriendInfo(block.AgentID, FriendRights.CanSeeOnline,
                                FriendRights.CanSeeOnline);
                            _Friends.Add(block.AgentID, friend);
                        }
                        else
                        {
                            friend = _Friends[block.AgentID];
                        }
                    }

                    bool doNotify = !friend.IsOnline;
                    friend.IsOnline = true;

                    if (OnFriendOnline != null && doNotify)
                    {
                        try { OnFriendOnline(friend); }
                        catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
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
                    FriendInfo friend;

                    lock (_Friends)
                    {
                        if (!_Friends.ContainsKey(block.AgentID))
                            _Friends.Add(block.AgentID, new FriendInfo(block.AgentID, FriendRights.CanSeeOnline, FriendRights.CanSeeOnline));

                        friend = _Friends[block.AgentID];
                        friend.IsOnline = false;
                    }

                    if (OnFriendOffline != null)
                    {
                        try { OnFriendOffline(friend); }
                        catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
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
                    if (_Friends.TryGetValue(block.AgentRelated, out friend))
                    {
                        friend.TheirFriendRights = newRights;
                        if (OnFriendRights != null)
                        {
                            try { OnFriendRights(friend); }
                            catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                        }
                    }
                    else if (block.AgentRelated == Client.Self.AgentID)
                    {
                        if (_Friends.TryGetValue(rights.AgentData.AgentID, out friend))
                        {
                            friend.MyFriendRights = newRights;
                            if (OnFriendRights != null)
                            {
                                try { OnFriendRights(friend); }
                                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
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
            LLUUID prey = reply.AgentBlock.Prey;
            ulong regionHandle = Helpers.GlobalPosToRegionHandle((float)reply.LocationBlock[0].GlobalX, 
                (float)reply.LocationBlock[0].GlobalY, out x, out y);
            LLVector3 xyz = new LLVector3(x, y, 0f);

            try { OnFriendFound(prey, regionHandle, xyz); }
            catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
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
                    lock (_Requests)
                    {
                        if (_Requests.ContainsKey(im.FromAgentID))
                        	_Requests[im.FromAgentID] = im.IMSessionID;
                        else
                        	_Requests.Add(im.FromAgentID, im.IMSessionID);
                    }
                    try { OnFriendshipOffered(im.FromAgentID, im.FromAgentName, im.IMSessionID); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
            }
            else if (im.Dialog == InstantMessageDialog.FriendshipAccepted)
            {
                FriendInfo friend = new FriendInfo(im.FromAgentID, FriendRights.CanSeeOnline,
                    FriendRights.CanSeeOnline);
                friend.Name = im.FromAgentName;
                lock (_Friends) _Friends[friend.UUID] = friend;

                if (OnFriendshipResponse != null)
                {
                    try { OnFriendshipResponse(im.FromAgentID, im.FromAgentName, true); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
            }
            else if (im.Dialog == InstantMessageDialog.FriendshipDeclined)
            {
                if (OnFriendshipResponse != null)
                {
                    try { OnFriendshipResponse(im.FromAgentID, im.FromAgentName, false); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
            }
        }

        private void Network_OnLoginResponse(bool loginSuccess, bool redirect, string message, string reason,
            LoginResponseData replyData)
        {
            if (loginSuccess && replyData.BuddyList != null)
            {
                lock (_Friends)
                {
                    for (int i = 0; i < replyData.BuddyList.Length; i++)
                    {
                        FriendInfo friend = replyData.BuddyList[i];
                        _Friends[friend.UUID] = friend;
                    }
                }
            }
        }
    }
}
