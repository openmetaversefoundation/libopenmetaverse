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
using System.Collections.Generic;
using System.Threading;
using libsecondlife.Packets;

namespace libsecondlife
{
    /// <summary>
    /// Class to manage multiple Avatars
    /// </summary>
    public class AvatarManager
    {
        /// <summary>
        /// Triggered after friend request packet is sent out
        /// </summary>
        /// <param name="AgentID"></param>
        /// <param name="Online"></param>
        public delegate void FriendNotificationCallback(LLUUID agentID, bool online);
        /// <summary>
        /// Triggered when a UUIDNameReply is received
        /// </summary>
        /// <param name="names"></param>
        public delegate void AgentNamesCallback(Dictionary<LLUUID, string> names);
        /// <summary>
        /// Triggered when Avatar properties are received (AvatarPropertiesReply)
        /// </summary>
        /// <param name="avatar"></param>
        public delegate void AvatarPropertiesCallback(Avatar avatar);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="avatar"></param>
        public delegate void AvatarNameCallback(Avatar avatar);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="avatar"></param>
        public delegate void AvatarStatisticsCallback(Avatar avatar);
        /// <summary>
        /// Triggered when a response for Avatar Interests is returned
        /// </summary>
        /// <param name="avatar"></param>
        public delegate void AvatarInterestsCallback(Avatar avatar);

        /// <summary>Triggered whenever a friend comes online or goes offline</summary>
        public event FriendNotificationCallback OnFriendNotification;

		private Dictionary<LLUUID, Avatar> Avatars;
        private SecondLife Client;
        private AgentNamesCallback OnAgentNames;
        private Dictionary<LLUUID, AvatarPropertiesCallback> AvatarPropertiesCallbacks;
	    private Dictionary<LLUUID, AvatarStatisticsCallback> AvatarStatisticsCallbacks;
        private Dictionary<LLUUID, AvatarInterestsCallback> AvatarInterestsCallbacks;

        /// <summary>
        /// Represents other avatars
        /// </summary>
        /// <param name="client"></param>
        public AvatarManager(SecondLife client)
        {
            Client = client;
            Avatars = new Dictionary<LLUUID, Avatar>();
            //Callback Dictionaries
            AvatarPropertiesCallbacks = new Dictionary<LLUUID, AvatarPropertiesCallback>();
	        AvatarStatisticsCallbacks = new Dictionary<LLUUID, AvatarStatisticsCallback>();
            AvatarInterestsCallbacks = new Dictionary<LLUUID, AvatarInterestsCallback>();
            // Friend notification callback
            NetworkManager.PacketCallback callback = new NetworkManager.PacketCallback(FriendNotificationHandler);
            Client.Network.RegisterCallback(PacketType.OnlineNotification, callback);
            Client.Network.RegisterCallback(PacketType.OfflineNotification, callback);
            Client.Network.RegisterCallback(PacketType.UUIDNameReply, new NetworkManager.PacketCallback(GetAgentNameHandler));
            Client.Network.RegisterCallback(PacketType.AvatarPropertiesReply, new NetworkManager.PacketCallback(AvatarPropertiesHandler));
            Client.Network.RegisterCallback(PacketType.AvatarStatisticsReply, new NetworkManager.PacketCallback(AvatarStatisticsHandler));
            Client.Network.RegisterCallback(PacketType.AvatarInterestsReply, new NetworkManager.PacketCallback(AvatarInterestsHandler));
        }
              

        /// <summary>
        /// Add an Avatar into the Avatars Dictionary
        /// </summary>
        /// <param name="avatar">Filled-out Avatar class to insert</param>
        public void AddAvatar(Avatar avatar)
        {
            lock (Avatars)
            {
                Avatars[avatar.ID] = avatar;
            }
        }

        /// <summary>
        /// Used to search all known Avatars for a particular Avatar Key
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Contains(LLUUID id)
        {
            return Avatars.ContainsKey(id);
        }

        /// <summary>
        /// Refresh Avatar Profile information
        /// </summary>
        /// <param name="a"></param>
        public void UpdateAvatar(Avatar a)
        {
            //Basic profile properties
            AvatarPropertiesUpdatePacket apup = new AvatarPropertiesUpdatePacket();
            AvatarPropertiesUpdatePacket.AgentDataBlock adb = new AvatarPropertiesUpdatePacket.AgentDataBlock();
            adb.AgentID = a.ID;
            adb.SessionID = Client.Network.SessionID;
            apup.AgentData = adb;
            AvatarPropertiesUpdatePacket.PropertiesDataBlock pdb = new AvatarPropertiesUpdatePacket.PropertiesDataBlock();
            pdb.AllowPublish = a.AllowPublish;
            pdb.FLAboutText = Helpers.StringToField(a.FirstLifeText);
            pdb.FLImageID = a.FirstLifeImage;
            pdb.ImageID = a.ProfileImage;
            pdb.MaturePublish = a.MaturePublish;
            pdb.ProfileURL = Helpers.StringToField(a.ProfileURL);
            apup.PropertiesData = pdb;
            //Intrests
            AvatarInterestsUpdatePacket aiup = new AvatarInterestsUpdatePacket();
            AvatarInterestsUpdatePacket.AgentDataBlock iadb = new AvatarInterestsUpdatePacket.AgentDataBlock();
            iadb.AgentID = a.ID;
            iadb.SessionID = Client.Network.SessionID;
            aiup.AgentData = iadb;
            AvatarInterestsUpdatePacket.PropertiesDataBlock ipdb = new AvatarInterestsUpdatePacket.PropertiesDataBlock();
            ipdb.LanguagesText = Helpers.StringToField(a.LanguagesText);
            ipdb.SkillsMask = a.SkillsMask;
            ipdb.SkillsText = Helpers.StringToField(a.SkillsText);
            ipdb.WantToMask = a.WantToMask;
            ipdb.WantToText = Helpers.StringToField(a.WantToText);
            aiup.PropertiesData = ipdb;
            //Send packets
            Client.Network.SendPacket(apup);
            Client.Network.SendPacket(aiup);
        }

        /// <summary>
        /// This function will only check if the avatar name exists locally,
        /// it will not do any networking calls to fetch the name
        /// </summary>
        /// <returns>The avatar name, or an empty string if it's not found</returns>
        public string LocalAvatarNameLookup(LLUUID id)
        {
            string name = "";

            lock (Avatars)
            {
                if (Avatars.ContainsKey(id))
                {
                    name = Avatars[id].Name;
                }
            }

            return name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void BeginGetAvatarName(LLUUID id, AgentNamesCallback anc)
        {
            // TODO: BeginGetAvatarNames is pretty bulky, rewrite a simple version here

            List<LLUUID> ids = new List<LLUUID>();
            ids.Add(id);

            BeginGetAvatarNames(ids, anc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        public void BeginGetAvatarNames(List<LLUUID> ids, AgentNamesCallback anc)
        {
            if (anc != null)
            {
                OnAgentNames = anc;
            }

            Dictionary<LLUUID, string> havenames = new Dictionary<LLUUID,string>();
            List<LLUUID> neednames = new List<LLUUID>();

            // Fire callbacks for the ones we already have cached
            foreach (LLUUID id in ids)
            {
                if (Avatars.ContainsKey(id))
                {
                    havenames[id] = Avatars[id].Name;
                }
                else
                {
                    neednames.Add(id);
                }
            }

            if (havenames.Count > 0 && OnAgentNames != null)
            {
                OnAgentNames(havenames);
            }

            if (neednames.Count > 0)
            {
                UUIDNameRequestPacket request = new UUIDNameRequestPacket();

                request.UUIDNameBlock = new UUIDNameRequestPacket.UUIDNameBlockBlock[neednames.Count];

                for (int i = 0; i < neednames.Count; i++)
                {
                    request.UUIDNameBlock[i] = new UUIDNameRequestPacket.UUIDNameBlockBlock();
                    request.UUIDNameBlock[i].ID = neednames[i];
                }

                Client.Network.SendPacket(request);
            }
        }

        /// <summary>
        /// Process an incoming UUIDNameReply Packet and insert Full Names into the Avatars Dictionary
        /// </summary>
        /// <param name="packet">Incoming Packet to process</param>
        /// <param name="simulator">Unused</param>
        private void GetAgentNameHandler(Packet packet, Simulator simulator)
        {
            Dictionary<LLUUID, string> names = new Dictionary<LLUUID, string>();
            UUIDNameReplyPacket reply = (UUIDNameReplyPacket)packet;

            lock (Avatars)
            {
                foreach (UUIDNameReplyPacket.UUIDNameBlockBlock block in reply.UUIDNameBlock)
                {
                    if (!Avatars.ContainsKey(block.ID))
                    {
                        Avatars[block.ID] = new Avatar();
                        Avatars[block.ID].ID = block.ID;
                    }

                    Avatars[block.ID].Name = Helpers.FieldToString(block.FirstName) +
                        " " + Helpers.FieldToString(block.LastName);

                    names[block.ID] = Avatars[block.ID].Name;
                }
            }

            if (OnAgentNames != null)
            {
                OnAgentNames(names);
            }
        }
        /// <summary>
        /// Handle incoming friend notifications
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void FriendNotificationHandler(Packet packet, Simulator simulator)
        {
            List<LLUUID> requestids = new List<LLUUID>();

            if (packet.Type == PacketType.OnlineNotification)
            {
                // If the agent is online...
                foreach (OnlineNotificationPacket.AgentBlockBlock block in ((OnlineNotificationPacket)packet).AgentBlock)
                {
                    lock (Avatars)
                    {
                        if (!Avatars.ContainsKey(block.AgentID))
                        {
                            // Mark this avatar for a name request
                            requestids.Add(block.AgentID);

                            Avatars[block.AgentID] = new Avatar();
                            Avatars[block.AgentID].ID = block.AgentID;
                        }

                        Avatars[block.AgentID].Online = true;
                    }

                    if (OnFriendNotification != null)
                    {
                        OnFriendNotification(block.AgentID, true);
                    }
                }
            }
            else if (packet.Type == PacketType.OfflineNotification)
            {
                // If the agent is Offline...
                foreach (OfflineNotificationPacket.AgentBlockBlock block in ((OfflineNotificationPacket)packet).AgentBlock)
                {
                    lock (Avatars)
                    {
                        if (!Avatars.ContainsKey(block.AgentID))
                        {
                            // Mark this avatar for a name request
                            requestids.Add(block.AgentID);

                            Avatars[block.AgentID] = new Avatar();
                            Avatars[block.AgentID].ID = block.AgentID;
                        }

                        Avatars[block.AgentID].Online = false;
                    }

                    if (OnFriendNotification != null)
                    {
                        OnFriendNotification(block.AgentID, true);
                    }
                }
            }

            if (requestids.Count > 0)
            {
                BeginGetAvatarNames(requestids, null);
            }
        }
        /// <summary>
        /// Handles incoming avatar statistics. What are those ?
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void AvatarStatisticsHandler(Packet packet, Simulator simulator)
        {
	    AvatarStatisticsReplyPacket asr = (AvatarStatisticsReplyPacket)packet;
            lock(Avatars)
            {
		Avatar av;
		if (!Avatars.ContainsKey(asr.AvatarData.AvatarID))
		{
			 av = new Avatar();
			 av.ID = asr.AvatarData.AvatarID;
		}
		else
		{
			 av = Avatars[asr.AvatarData.AvatarID];
		}

                foreach(AvatarStatisticsReplyPacket.StatisticsDataBlock b in asr.StatisticsData)
		{
			string n = Helpers.FieldToString(b.Name);
			if(n.Equals("Behavior"))
			{
				av.Behavior = b.Positive;
			}
			else if(n.Equals("Appearance"))
			{
				av.Appearance = b.Positive;
			}
			else if(n.Equals("Building"))
			{
				av.Building = b.Positive;
			}
		}
		
		//Call it
        if (AvatarStatisticsCallbacks.ContainsKey(av.ID) && AvatarStatisticsCallbacks[av.ID] != null)
	                AvatarStatisticsCallbacks[av.ID](av);
            }
        }
        /// <summary>
        /// Process incoming Avatar properties (profile data)
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="sim"></param>
        private void AvatarPropertiesHandler(Packet packet, Simulator sim)
        {
            Avatar av;
            AvatarPropertiesReplyPacket reply = (AvatarPropertiesReplyPacket)packet;
            lock(Avatars)
            {
            if (!Avatars.ContainsKey(reply.AgentData.AvatarID))
            {
                //not in our "cache", create a new object
                av = new Avatar();
            }
            else
            {
                //Cache hit, modify existing avatar
                av = Avatars[reply.AgentData.AvatarID];
            }
            av.ID = reply.AgentData.AvatarID;
            av.ProfileImage = reply.PropertiesData.ImageID;
            av.FirstLifeImage = reply.PropertiesData.FLImageID;
            av.PartnerID = reply.PropertiesData.PartnerID;
            av.AboutText = Helpers.FieldToString(reply.PropertiesData.AboutText);
	    
            av.FirstLifeText = Helpers.FieldToString(reply.PropertiesData.FLAboutText);
            av.BornOn = Helpers.FieldToString(reply.PropertiesData.BornOn);
            av.CharterMember = Helpers.FieldToString(reply.PropertiesData.CharterMember);
            av.AllowPublish = reply.PropertiesData.AllowPublish;
            av.MaturePublish = reply.PropertiesData.MaturePublish;
            av.Identified = reply.PropertiesData.Identified;
            av.Transacted = reply.PropertiesData.Transacted;
            av.ProfileURL = Helpers.FieldToString(reply.PropertiesData.ProfileURL);
            //reassign in the cache
            Avatars[av.ID] = av;
            //Heaven forbid that we actually get a packet we didn't ask for.
            if (AvatarPropertiesCallbacks.ContainsKey(av.ID) && AvatarPropertiesCallbacks[av.ID] != null)
                AvatarPropertiesCallbacks[av.ID](av);
            }
        }
        /// <summary>
        /// Start a request for Avatar Properties
        /// </summary>
        /// <param name="avatarid"></param>
        /// <param name="aic"></param>
        /// <param name="asc"></param>
        /// <param name="apc"></param>
        public void BeginAvatarPropertiesRequest(LLUUID avatarid, AvatarPropertiesCallback apc, AvatarStatisticsCallback asc, AvatarInterestsCallback aic)
        {
            //Set teh callback!
            AvatarPropertiesCallbacks[avatarid] = apc;
	        AvatarStatisticsCallbacks[avatarid] = asc;
            AvatarInterestsCallbacks[avatarid] = aic;
            //Oh noes
            //Packet construction, good times
            AvatarPropertiesRequestPacket aprp = new AvatarPropertiesRequestPacket();
            AvatarPropertiesRequestPacket.AgentDataBlock adb = new AvatarPropertiesRequestPacket.AgentDataBlock();
            adb.AgentID = Client.Network.AgentID;
            adb.SessionID = Client.Network.SessionID;
            adb.AvatarID = avatarid;
            aprp.AgentData = adb;
            //send the packet!
            Client.Network.SendPacket(aprp);
        }
        /// <summary>
        /// Process incoming Avatar Interests information
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void AvatarInterestsHandler(Packet packet, Simulator simulator)
        {
            AvatarInterestsReplyPacket airp = (AvatarInterestsReplyPacket)packet;
            Avatar av;
            lock (Avatars)
            {
                if (!Avatars.ContainsKey(airp.AgentData.AvatarID))
                {
                    //not in our "cache", create a new object
                    av = new Avatar();
                    av.ID = airp.AgentData.AvatarID;
                }
                else
                {
                    //Cache hit, modify existing avatar
                    av = Avatars[airp.AgentData.AvatarID];
                }
                //The rest of the properties, thanks LL.
                av.WantToMask = airp.PropertiesData.WantToMask;
                av.WantToText = Helpers.FieldToString(airp.PropertiesData.WantToText);
                av.SkillsMask = airp.PropertiesData.SkillsMask;
                av.SkillsText = Helpers.FieldToString(airp.PropertiesData.SkillsText);
                av.LanguagesText = Helpers.FieldToString(airp.PropertiesData.LanguagesText);
            }
            if (AvatarInterestsCallbacks.ContainsKey(airp.AgentData.AvatarID) && AvatarInterestsCallbacks[airp.AgentData.AvatarID] != null)
                AvatarInterestsCallbacks[airp.AgentData.AvatarID](av);
        }
    }
}
