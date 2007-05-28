/*
 * Copyright (c) 2006-2007, Second Life Reverse Engineering Team
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
        /// 
        /// </summary>
        [Flags]
        public enum DirFindFlags
        {
            /// <summary></summary>
            People = 1 << 0,
            /// <summary></summary>
            Online = 1 << 1,
            /// <summary></summary>
            [Obsolete]
            Places = 1 << 2,
            /// <summary></summary>
            Events = 1 << 3,
            /// <summary></summary>
            Groups = 1 << 4,
            /// <summary></summary>
            DateEvents = 1 << 5,
            /// <summary></summary>
            AgentOwned = 1 << 6,
            /// <summary></summary>
            ForSale = 1 << 7,
            /// <summary></summary>
            GroupOwned = 1 << 8,
            /// <summary></summary>
            [Obsolete]
            Auction = 1 << 9,
            /// <summary></summary>
            DwellSort = 1 << 10,
            /// <summary></summary>
            PgSimsOnly = 1 << 11,
            /// <summary></summary>
            PicturesOnly = 1 << 12,
            /// <summary></summary>
            PgEventsOnly = 1 << 13,
            /// <summary></summary>
            MatureSimsOnly = 1 << 14,
            /// <summary></summary>
            SortAsc = 1 << 15,
            /// <summary></summary>
            PricesSort = 1 << 16,
            /// <summary></summary>
            PerMeterSort = 1 << 17,
            /// <summary></summary>
            AreaSort = 1 << 18,
            /// <summary></summary>
            NameSort = 1 << 19,
            /// <summary></summary>
            LimitByPrice = 1 << 20,
            /// <summary></summary>
            LimitByArea = 1 << 21
        }

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum SearchTypeFlags
        {
            /// <summary></summary>
            None = 0,
            /// <summary></summary>
            Auction = 1 << 1,
            /// <summary></summary>
            Newbie = 1 << 2,
            /// <summary></summary>
            Mainland = 1 << 3,
            /// <summary></summary>
            Estate = 1 << 4
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
        /// A parcel retrieved from the dataserver such as results from the 
        /// "For-Sale" listings
        /// </summary>
        public struct DirectoryParcel
        {
            /// <summary></summary>
            public LLUUID ID;
            /// <summary></summary>
            public string Name;
            /// <summary></summary>
            public int ActualArea;
            /// <summary></summary>
            public int SalePrice;
            /// <summary></summary>
            public bool Auction;
            /// <summary></summary>
            public bool ForSale;
        }

        public struct AgentSearchData {
            public bool Online;
            public string FirstName;
            public string LastName;
            public LLUUID AgentID;
        }

        /*/// <summary></summary>
        public LLUUID OwnerID;
        /// <summary></summary>
        public LLUUID SnapshotID;
        /// <summary></summary>
        public ulong RegionHandle;
        /// <summary></summary>
        public string SimName;
        /// <summary></summary>
        public string Desc;
        /// <summary></summary>
        public LLVector3 GlobalPosition;
        /// <summary></summary>
        public LLVector3 SimPosition;
        /// <summary></summary>
        public float Dwell;*/


        /// <summary>
        /// 
        /// </summary>
        /// <param name="classifieds"></param>
        public delegate void ClassifiedReplyCallback(List<Classified> classifieds);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dirParcels"></param>
        public delegate void DirLandReplyCallback(List<DirectoryParcel> dirParcels);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matchedPeople"></param>
        public delegate void DirPeopleReplyCallback(LLUUID queryID, List<AgentSearchData> matchedPeople);

        /// <summary>
        /// 
        /// </summary>
        public event ClassifiedReplyCallback OnClassifiedReply;
        /// <summary>
        /// 
        /// </summary>
        public event DirLandReplyCallback OnDirLandReply;

        public event DirPeopleReplyCallback OnDirPeopleReply;

        private SecondLife Client;


        public DirectoryManager(SecondLife client)
        {
            Client = client;

            Client.Network.RegisterCallback(PacketType.DirClassifiedReply, new NetworkManager.PacketCallback(DirClassifiedReplyHandler));
            Client.Network.RegisterCallback(PacketType.DirLandReply, new NetworkManager.PacketCallback(DirLandReplyHandler));
            Client.Network.RegisterCallback(PacketType.DirPeopleReply, new NetworkManager.PacketCallback(DirPeopleReplyHandler));

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

        /// <summary>
        /// Starts a search for land sales using the directory
        /// </summary>
        /// <param name="typeFlags">What type of land to search for. Auction, 
        /// estate, mainland, "first land", etc</param>
        /// <returns>A unique identifier that can identify packets associated
        /// with this query from other queries</returns>
        /// <remarks>The OnDirLandReply event handler must be registered before
        /// calling this function. There is no way to determine how many 
        /// results will be returned, or how many times the callback will be 
        /// fired other than you won't get more than 100 total parcels from 
        /// each query.</remarks>
        public LLUUID StartLandSearch(SearchTypeFlags typeFlags)
        {
            return StartLandSearch(DirFindFlags.SortAsc | DirFindFlags.PerMeterSort, typeFlags, 0, 0, 0);
        }

        /// <summary>
        /// Starts a search for land sales using the directory
        /// </summary>
        /// <param name="typeFlags">What type of land to search for. Auction, 
        /// estate, mainland, "first land", etc</param>
        /// <param name="priceLimit">Maximum price to search for</param>
        /// <param name="areaLimit">Maximum area to search for</param>
        /// <param name="queryStart">Each request is limited to 100 parcels
        /// being returned. To get the first 100 parcels of a request use 0,
        /// from 100-199 use 1, 200-299 use 2, etc.</param>
        /// <returns>A unique identifier that can identify packets associated
        /// with this query from other queries</returns>
        /// <remarks>The OnDirLandReply event handler must be registered before
        /// calling this function. There is no way to determine how many 
        /// results will be returned, or how many times the callback will be 
        /// fired other than you won't get more than 100 total parcels from 
        /// each query.</remarks>
        public LLUUID StartLandSearch(SearchTypeFlags typeFlags, int priceLimit, int areaLimit, int queryStart)
        {
            return StartLandSearch(DirFindFlags.SortAsc | DirFindFlags.PerMeterSort | DirFindFlags.LimitByPrice | 
                DirFindFlags.LimitByArea, typeFlags, priceLimit, areaLimit, queryStart);
        }

        /// <summary>
        /// Starts a search for land sales using the directory
        /// </summary>
        /// <param name="findFlags">A flags parameter that can modify the way
        /// search results are returned, for example changing the ordering of
        /// results or limiting based on price or area</param>
        /// <param name="typeFlags">What type of land to search for. Auction, 
        /// estate, mainland, "first land", etc</param>
        /// <param name="priceLimit">Maximum price to search for, the 
        /// DirFindFlags.LimitByPrice flag must be set</param>
        /// <param name="areaLimit">Maximum area to search for, the
        /// DirFindFlags.LimitByArea flag must be set</param>
        /// <param name="queryStart">Each request is limited to 100 parcels
        /// being returned. To get the first 100 parcels of a request use 0,
        /// from 100-199 use 100, 200-299 use 200, etc.</param>
        /// <returns>A unique identifier that can identify packets associated
        /// with this query from other queries</returns>
        /// <remarks>The OnDirLandReply event handler must be registered before
        /// calling this function. There is no way to determine how many 
        /// results will be returned, or how many times the callback will be 
        /// fired other than you won't get more than 100 total parcels from 
        /// each query.</remarks>
        public LLUUID StartLandSearch(DirFindFlags findFlags, SearchTypeFlags typeFlags, int priceLimit,
            int areaLimit, int queryStart)
        {
            LLUUID queryID = LLUUID.Random();

            DirLandQueryPacket query = new DirLandQueryPacket();
            query.AgentData.AgentID = Client.Network.AgentID;
            query.AgentData.SessionID = Client.Network.SessionID;
            query.QueryData.Area = areaLimit;
            query.QueryData.Price = priceLimit;
            query.QueryData.QueryStart = queryStart;
            query.QueryData.SearchType = (uint)typeFlags;
            query.QueryData.QueryFlags = (uint)findFlags;
            query.QueryData.QueryID = queryID;

            Client.Network.SendPacket(query);

            return queryID;
        }

        public LLUUID StartPeopleSearch(DirFindFlags findFlags, string searchText, int queryStart)
        {
            LLUUID queryID = LLUUID.Random();
            DirFindQueryPacket find = new DirFindQueryPacket();
            find.AgentData.AgentID = Client.Network.AgentID;
            find.AgentData.SessionID = Client.Network.SessionID;
            find.QueryData.QueryFlags = (uint)findFlags;
            find.QueryData.QueryText = Helpers.StringToField(searchText);
            find.QueryData.QueryID = queryID;
            find.QueryData.QueryStart = queryStart;
            Client.Network.SendPacket(find);
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
                    classified.Name = Helpers.FieldToUTF8String(block.Name);
                    classified.Price = block.PriceForListing;

                    classifieds.Add(classified);
                }

                try { OnClassifiedReply(classifieds); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        private void DirLandReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnDirLandReply != null)
            {
                List<DirectoryParcel> parcelsForSale = new List<DirectoryParcel>();
                DirLandReplyPacket reply = (DirLandReplyPacket)packet;

                foreach (DirLandReplyPacket.QueryRepliesBlock block in reply.QueryReplies)
                {
                    DirectoryParcel dirParcel = new DirectoryParcel();

                    dirParcel.ActualArea = block.ActualArea;
                    dirParcel.ID = block.ParcelID;
                    dirParcel.Name = Helpers.FieldToUTF8String(block.Name);
                    dirParcel.SalePrice = block.SalePrice;
                    dirParcel.Auction = block.Auction;
                    dirParcel.ForSale = block.ForSale;

                    parcelsForSale.Add(dirParcel);
                }

                try { OnDirLandReply(parcelsForSale); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        protected void DirPeopleReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnDirPeopleReply != null)
            {
                DirPeopleReplyPacket peopleReply = packet as DirPeopleReplyPacket;
                List<AgentSearchData> matches = new List<AgentSearchData>(peopleReply.QueryReplies.Length);
                foreach (DirPeopleReplyPacket.QueryRepliesBlock reply in peopleReply.QueryReplies) {
                    AgentSearchData searchData = new AgentSearchData();
                    searchData.Online = reply.Online;
                    searchData.FirstName = Helpers.FieldToUTF8String(reply.FirstName);
                    searchData.LastName = Helpers.FieldToUTF8String(reply.LastName);
                    searchData.AgentID = reply.AgentID;
                    matches.Add(searchData);
                }
                try { OnDirPeopleReply(peopleReply.QueryData.QueryID, matches); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }
    }
}
