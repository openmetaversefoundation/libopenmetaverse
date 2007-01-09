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
        /// The type of land to be searched for in a DirLandQuery
        /// </summary>
        public enum LandFlags
        {
            /// <summary>Include everything</summary>
            All = 0,
            /// <summary>PG land only</summary>
            PGOnly = 2048,
            /// <summary>Mature land only</summary>
            MatureOnly = 16384
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
        public struct ClassifiedFull
        {
            /// <summary></summary>
            public Classified Classified;
        }

        /// <summary>
        /// 
        /// </summary>
        public struct ParcelSale
        {
            /// <summary></summary>
            public LLUUID ParcelID;
            /// <summary></summary>
            public string Name;
            /// <summary></summary>
            public int SalePrice;
            /// <summary></summary>
            public int ActualArea;
            /// <summary></summary>
            public bool ReservedNewbie;
            /// <summary></summary>
            public bool ForSale;
            /// <summary></summary>
            public bool Auction;
        }

        /// <summary>
        /// 
        /// </summary>
        public struct ParcelSaleFull
        {
            /// <summary></summary>
            public ParcelSale ParcelSale;
            /// <summary></summary>
            public string SimName;
            /// <summary></summary>
            public int BillableArea;
            /// <summary></summary>
            public float GlobalX;
            /// <summary></summary>
            public float GlobalY;
            /// <summary></summary>
            public float GlobalZ;
            /// <summary></summary>
            public string Description;
            /// <summary></summary>
            public LLUUID OwnerID;
            /// <summary></summary>
            public LLUUID SnapshotID;
            /// <summary></summary>
            public LLUUID AuctionID;
            /// <summary></summary>
            public int Flags;
            /// <summary></summary>
            public int Dwell;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryID"></param>
        /// <param name="classifieds"></param>
        public delegate void ClassifiedReplyCallback(LLUUID queryID, List<Classified> classifieds);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryID"></param>
        /// <param name="sales"></param>
        public delegate void LandReplyCallback(LLUUID queryID, List<ParcelSale> sales);


        /// <summary></summary>
        public event ClassifiedReplyCallback OnClassifiedReply;
        /// <summary></summary>
        public event LandReplyCallback OnLandReply;


        private SecondLife Client;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">Reference to the client</param>
        public DirectoryManager(SecondLife client)
        {
            Client = client;

            Client.Network.RegisterCallback(PacketType.DirClassifiedReply, new NetworkManager.PacketCallback(DirClassifiedReplyHandler));
            Client.Network.RegisterCallback(PacketType.DirLandReply, new NetworkManager.PacketCallback(DirLandReplyHandler));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="categories"></param>
        /// <param name="mature"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public LLUUID StartLandSearch(bool auction, bool forSale, bool reservedNewbie, LandFlags flags)
        {
            DirLandQueryPacket query = new DirLandQueryPacket();
            LLUUID queryID = LLUUID.Random();

            query.AgentData.AgentID = Client.Network.AgentID;
            query.AgentData.SessionID = Client.Network.SessionID;
            query.QueryData.Auction = auction;
            query.QueryData.ForSale = forSale;
            query.QueryData.ReservedNewbie = reservedNewbie;
            query.QueryData.QueryFlags = (uint)flags;
            query.QueryData.QueryID = queryID;

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

                OnClassifiedReply(reply.QueryData.QueryID, classifieds);
            }
        }

        private void DirLandReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnLandReply != null)
            {
                DirLandReplyPacket reply = (DirLandReplyPacket)packet;
                List<ParcelSale> sales = new List<ParcelSale>();

                foreach (DirLandReplyPacket.QueryRepliesBlock block in reply.QueryReplies)
                {
                    ParcelSale sale = new ParcelSale();

                    sale.ActualArea = block.ActualArea;
                    sale.Auction = block.Auction;
                    sale.ForSale = block.ForSale;
                    sale.Name = Helpers.FieldToString(block.Name);
                    sale.ParcelID = block.ParcelID;
                    sale.ReservedNewbie = block.ReservedNewbie;
                    sale.SalePrice = block.SalePrice;

                    sales.Add(sale);
                }

                OnLandReply(reply.QueryData.QueryID, sales);
            }
        }
    }
}
