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
    /// 
    /// </summary>
    public class AvatarManager
    {
        /// <summary>Triggered whenever a friend comes online or goes offline</summary>
        public event FriendNotificationCallback OnFriendNotification;

        private SecondLife Client;
        private Dictionary<LLUUID, Avatar> Avatars;
        private AgentNamesCallback OnAgentNames;

        public AvatarManager(SecondLife client)
        {
            Client = client;
            Avatars = new Dictionary<LLUUID, Avatar>();

            // Friend notification callback
            PacketCallback callback = new PacketCallback(FriendNotificationHandler);
            Client.Network.RegisterCallback(PacketType.OnlineNotification, callback);
            Client.Network.RegisterCallback(PacketType.OfflineNotification, callback);
            Client.Network.RegisterCallback(PacketType.UUIDNameReply, new PacketCallback(GetAgentNameHandler));
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

        public bool Contains(LLUUID id)
        {
            return Avatars.ContainsKey(id);
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
    }
}
