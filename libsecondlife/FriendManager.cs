/*
 * Copyright (c) 2007, Second Life Reverse Engineering Team
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

using libsecondlife;
using libsecondlife.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace libsecondlife
{
    /// <summary>
    /// 
    /// </summary>
    public class FriendManager
    {
        private SecondLife Client;

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum UserRights
        {
            /// <summary></summary>
            None = 0x00,
            /// <summary></summary>
            ViewOnlineStatus = 0x01,
            /// <summary></summary>
            ViewMapLocation = 0x02,
            /// <summary></summary>
            ModifyObjects = 0x04,
            /// <summary></summary>
            AllUserRights = 0x07
        }

        /// <summary>Constructor for FriendManager</summary>
        public FriendManager(SecondLife client)
        {
            Client = client;
        }

        /// <summary>
        /// Grants the specified user rights such as map or modify
        /// </summary>
        /// <param name="targetID">The user to grant rights to</param>
        /// <param name="viewOnlineStatus">User can see your online status</param>
        /// <param name="viewMapLocation">User can see your map location</param>
        /// <param name="modifyObjects">User can modify your objects</param>
        public void GrantUserRights(LLUUID targetID, bool viewOnlineStatus, bool viewMapLocation, bool modifyObjects)
        {
            UserRights rights = UserRights.None;
            if (viewOnlineStatus) rights |= UserRights.ViewOnlineStatus;
            if (viewMapLocation) rights |= UserRights.ViewMapLocation;
            if (modifyObjects) rights |= UserRights.ModifyObjects;
            GrantUserRightsPacket p = new GrantUserRightsPacket();
            p.AgentData.AgentID = Client.Network.AgentID;
            p.AgentData.SessionID = Client.Network.SessionID;
            p.Rights = new GrantUserRightsPacket.RightsBlock[1];
            p.Rights[0] = new GrantUserRightsPacket.RightsBlock();
            p.Rights[0].AgentRelated = targetID;
            p.Rights[0].RelatedRights = (int)rights;
            Client.Network.SendPacket(p);
        }

        /// <summary>
        /// Offers friendship to the specified user
        /// </summary>
        public void RequestFriendship(LLUUID targetID)
        {
            Client.Self.InstantMessage(String.Empty, targetID, String.Empty, LLUUID.Zero, 
                MainAvatar.InstantMessageDialog.FriendshipOffered, MainAvatar.InstantMessageOnline.Online, 
                Client.Self.Position, Client.Network.CurrentSim.ID, new byte[0]);
        }

        /// <summary>
        /// Revokes friendship from the specified user
        /// </summary>
        public void RemoveFriend(LLUUID targetID)
        {
            TerminateFriendshipPacket p = new TerminateFriendshipPacket();
            p.AgentData.AgentID = Client.Network.AgentID;
            p.AgentData.SessionID = Client.Network.SessionID;
            p.ExBlock.OtherID = targetID;
            Client.Network.SendPacket(p);
        }
    }
}
