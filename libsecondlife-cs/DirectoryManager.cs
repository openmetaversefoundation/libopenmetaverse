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
    /// 
    /// </summary>
    public class DirectoryManager
    {
        /// <summary>
        /// The different categories a classified ad can be placed in
        /// </summary>
        public enum ClassifiedCategories
        {
            /// <summary></summary>
            Any = 0,
            /// <summary></summary>
            Shopping,
            /// <summary></summary>
            LandRental,
            /// <summary></summary>
            PropertyRental,
            /// <summary></summary>
            SpecialAttraction,
            /// <summary></summary>
            NewProducts,
            /// <summary></summary>
            Employment,
            /// <summary></summary>
            Wanted,
            /// <summary></summary>
            Service,
            /// <summary></summary>
            Personal
        }

        /// <summary>
        /// A classified ad in Second Life
        /// </summary>
        public struct Classified
        {
            /// <summary>UUID for this ad, useful for looking up detailed
            /// information about it</summary>
            public LLUUID ID;
            /// <summary>The title of this classified ad</summary>
            public string Name;
            /// <summary>Unknown</summary>
            public byte Flags;
            /// <summary>Creation date of the ad</summary>
            public DateTime CreationDate;
            /// <summary>Expiration date of the ad</summary>
            public DateTime ExpirationDate;
            /// <summary>Price that was paid for this ad</summary>
            public int Price;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="classifieds"></param>
        public delegate void ClassifiedReplyCallback(List<Classified> classifieds);


        /// <summary>
        /// 
        /// </summary>
        public event ClassifiedReplyCallback OnClassifiedReply;


        private SecondLife Client;


        public DirectoryManager(SecondLife client)
        {
            Client = client;

            Client.Network.RegisterCallback(PacketType.DirClassifiedReply, new NetworkManager.PacketCallback(DirClassifiedReplyHandler));
        }

        public LLUUID StartClassifiedSearch(string searchText, ClassifiedCategories categories, bool mature)
        {
            DirClassifiedQueryPacket query = new DirClassifiedQueryPacket();
            LLUUID queryID = LLUUID.Random();

            query.AgentData.AgentID = Client.Network.AgentID;
            query.AgentData.SessionID = Client.Network.SessionID;
            query.QueryData.Category = (uint)categories;
            query.QueryData.QueryFlags = (uint)(mature ? 0 : 2);
            query.QueryData.QueryID = queryID;
            query.QueryData.QueryText = Helpers.StringToField(searchText);

            Client.Network.SendPacket(query);

            return queryID;
        }

        private void DirClassifiedReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnClassifiedReply != null)
            {
                DirClassifiedReplyPacket reply = (DirClassifiedReplyPacket)packet;
                List<Classified> classifieds = new List<Classified>();

                foreach (DirClassifiedReplyPacket.QueryRepliesBlock block in reply.QueryReplies)
                {
                    Classified classified = new Classified();

                    classified.CreationDate = Helpers.UnixTimeToDateTime(block.CreationDate);
                    classified.ExpirationDate = Helpers.UnixTimeToDateTime(block.ExpirationDate);
                    classified.Flags = block.ClassifiedFlags;
                    classified.ID = block.ClassifiedID;
                    classified.Name = Helpers.FieldToString(block.Name);
                    classified.Price = block.PriceForListing;

                    classifieds.Add(classified);
                }

                OnClassifiedReply(classifieds);
            }
        }
    }
}
