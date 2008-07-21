/*
 * Copyright (c) 2006-2008, Second Life Reverse Engineering Team
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
    /// Access to the Linden dataserver which allows searching for land, events, people, etc
    /// </summary>
    /// <remarks>This class is automatically instantiated by the SecondLife class</remarks>
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

        public enum EventCategories
        {
            All = 0,
            Discussion = 18,
            Sports = 19,
            LiveMusic = 20,
            Commercial = 22,
            Nightlife = 23,
            Games = 24,
            Pageants = 25,
            Education = 26,
            Arts = 27,
            Charity = 28,
            Miscellaneous = 29
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
            //[Obsolete]
            //Places = 1 << 2,
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
            //[Obsolete]
            //Auction = 1 << 9,
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
        /// Land types to search dataserver for
        /// </summary>
        [Flags]
        public enum SearchTypeFlags
        {
            /// <summary>Do not search</summary>
            None = 0,
            /// <summary>Land which is currently up for auction</summary>
            Auction = 1 << 1,
            /// <summary>Land available to new landowners (formerly the FirstLand program)</summary>
            //[Obsolete]
            //Newbie = 1 << 2,
            /// <summary>Parcels which are on the mainland (Linden owned) continents</summary>
            Mainland = 1 << 3,
            /// <summary>Parcels which are on privately owned simulators</summary>
            Estate = 1 << 4
        }

        [Flags]
        public enum EventFlags
        {
            None = 0,
            Mature = 1 << 1
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

        /// <summary>
        /// An Avatar returned from the dataserver
        /// </summary>
        public struct AgentSearchData 
        {
            /// <summary>Online status of agent</summary>
            public bool Online;
            /// <summary>Agents first name</summary>
            public string FirstName;
            /// <summary>Agents last name</summary>
            public string LastName;
            /// <summary>Agents <seealso cref="T:libsecondlife.LLUUID"/></summary>
            public LLUUID AgentID;
        }
        /// <summary>
        ///  Response to a "Groups" Search
        /// </summary>
        public struct GroupSearchData
        {
            public LLUUID GroupID;
            public string GroupName;
            public int Members;
        }

        /// <summary>
        /// Response to a "Places" Search
        /// Note: This is not DirPlacesReply
        /// </summary>
        public struct PlacesSearchData
        {
            public LLUUID OwnerID;
            public string Name;
            public string Desc;
            public int ActualArea;
            public int BillableArea;
            public byte Flags;
            public float GlobalX;
            public float GlobalY;
            public float GlobalZ;
            public string SimName;
            public LLUUID SnapshotID;
            public float Dwell;
            public int Price;   
        }

        /// <summary>
        /// Response to "Events" search
        /// </summary>
        public struct EventsSearchData
        {
            public LLUUID Owner;
            public string Name;
            public uint ID;
            public string Date;
            public uint Time;
            public EventFlags Flags;
        }


        /// <summary>
        /// an Event returned from the dataserver
        /// </summary>
        public struct EventInfo
        {
			public uint ID;
			public LLUUID Creator;
            public string Name;
            public EventCategories Category;
            public string Desc;
            public string Date;
            public UInt32 DateUTC;
            public UInt32 Duration;
            public UInt32 Cover;
            public UInt32 Amount;
            public string SimName;
            public LLVector3d GlobalPos;
            public EventFlags Flags;
        }

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
        /// <param name="queryID"></param>
        /// <param name="matchedPeople"></param>
        public delegate void DirPeopleReplyCallback(LLUUID queryID, List<AgentSearchData> matchedPeople);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryID"></param>
        /// <param name="matchedGroups"></param>
        public delegate void DirGroupsReplyCallback(LLUUID queryID, List<GroupSearchData> matchedGroups);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryID"></param>
        /// <param name="matchedPlaces"></param>
        public delegate void PlacesReplyCallback(LLUUID queryID, List<PlacesSearchData> matchedPlaces);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryID"></param>
        /// <param name="matchedEvents"></param>
        public delegate void EventReplyCallback(LLUUID queryID, List<EventsSearchData> matchedEvents);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matchedEvent"></param>
        public delegate void EventInfoCallback(EventInfo matchedEvent);

        /// <summary>
        /// 
        /// </summary>
        public event ClassifiedReplyCallback OnClassifiedReply;
        /// <summary>
        /// 
        /// </summary>
        public event DirLandReplyCallback OnDirLandReply;

        public event DirPeopleReplyCallback OnDirPeopleReply;

        public event DirGroupsReplyCallback OnDirGroupsReply;

        public event PlacesReplyCallback OnPlacesReply;

        // List of Events
        public event EventReplyCallback OnEventsReply;

        // Event Details
        public event EventInfoCallback OnEventInfo;

        private SecondLife Client;


        public DirectoryManager(SecondLife client)
        {
            Client = client;

            Client.Network.RegisterCallback(PacketType.DirClassifiedReply, new NetworkManager.PacketCallback(DirClassifiedReplyHandler));
            Client.Network.RegisterCallback(PacketType.DirLandReply, new NetworkManager.PacketCallback(DirLandReplyHandler));
            Client.Network.RegisterCallback(PacketType.DirPeopleReply, new NetworkManager.PacketCallback(DirPeopleReplyHandler));
            Client.Network.RegisterCallback(PacketType.DirGroupsReply, new NetworkManager.PacketCallback(DirGroupsReplyHandler));
            Client.Network.RegisterCallback(PacketType.PlacesReply, new NetworkManager.PacketCallback(PlacesReplyHandler));
            Client.Network.RegisterCallback(PacketType.DirEventsReply, new NetworkManager.PacketCallback(EventsReplyHandler));
            Client.Network.RegisterCallback(PacketType.EventInfoReply, new NetworkManager.PacketCallback(EventInfoReplyHandler));    

        }

        public LLUUID StartClassifiedSearch(string searchText, ClassifiedCategories categories, bool mature)
        {
            DirClassifiedQueryPacket query = new DirClassifiedQueryPacket();
            LLUUID queryID = LLUUID.Random();

            query.AgentData.AgentID = Client.Self.AgentID;
            query.AgentData.SessionID = Client.Self.SessionID;
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
            query.AgentData.AgentID = Client.Self.AgentID;
            query.AgentData.SessionID = Client.Self.SessionID;
            query.QueryData.Area = areaLimit;
            query.QueryData.Price = priceLimit;
            query.QueryData.QueryStart = queryStart;
            query.QueryData.SearchType = (uint)typeFlags;
            query.QueryData.QueryFlags = (uint)findFlags;
            query.QueryData.QueryID = queryID;

            Client.Network.SendPacket(query);

            return queryID;
        }
        /// <summary>
        /// Starts a search for a Group in the directory manager
        /// </summary>
        /// <param name="findFlags"></param>
        /// <param name="searchText">The text to search for</param>
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
        public LLUUID StartGroupSearch(DirFindFlags findFlags, string searchText, int queryStart)
        {
            return StartGroupSearch(findFlags, searchText, queryStart, LLUUID.Random());
        }

        public LLUUID StartGroupSearch(DirFindFlags findFlags, string searchText, int queryStart, LLUUID queryID)
        {
            DirFindQueryPacket find = new DirFindQueryPacket();
            find.AgentData.AgentID = Client.Self.AgentID;
            find.AgentData.SessionID = Client.Self.SessionID;
            find.QueryData.QueryFlags = (uint)findFlags;
            find.QueryData.QueryText = Helpers.StringToField(searchText);
            find.QueryData.QueryID = queryID;
            find.QueryData.QueryStart = queryStart;
            Client.Network.SendPacket(find);
            return queryID;
        }

        public LLUUID StartPeopleSearch(DirFindFlags findFlags, string searchText, int queryStart)
        {
            return StartPeopleSearch(findFlags, searchText, queryStart, LLUUID.Random());
        }

        public LLUUID StartPeopleSearch(DirFindFlags findFlags, string searchText, int queryStart, LLUUID queryID)
        {
            DirFindQueryPacket find = new DirFindQueryPacket();
            find.AgentData.AgentID = Client.Self.AgentID;
            find.AgentData.SessionID = Client.Self.SessionID;
            find.QueryData.QueryFlags = (uint)findFlags;
            find.QueryData.QueryText = Helpers.StringToField(searchText);
            find.QueryData.QueryID = queryID;
            find.QueryData.QueryStart = queryStart;

            Client.Network.SendPacket(find);

            return queryID;
        }

        /// <summary>
        /// Search "places" for Land you personally own
        /// </summary>
        public LLUUID StartPlacesSearch()
        {
            return StartPlacesSearch(DirFindFlags.AgentOwned, Parcel.ParcelCategory.Any, String.Empty, String.Empty, 
                LLUUID.Zero, LLUUID.Zero);
        }

        /// <summary>
        /// Searches Places for Land owned by a specific user or group
        /// </summary>
        /// <param name="findFlags">One of the Values from the DirFindFlags struct, ie: AgentOwned, GroupOwned, etc.</param>
        /// <param name="groupID">LLUID of group you want to recieve land list for (You must be in group), or
        /// LLUID.Zero for Your own land</param>
        /// <returns>Transaction (Query) ID which can be associated with results from your request.</returns>
        public LLUUID StartPlacesSearch(DirFindFlags findFlags, LLUUID groupID)
        {
            return StartPlacesSearch(findFlags, Parcel.ParcelCategory.Any, String.Empty, String.Empty, groupID, 
                LLUUID.Random());
        }

        /// <summary>
        ///  Search Places 
        /// </summary>
        /// <param name="findFlags">One of the Values from the DirFindFlags struct, ie: AgentOwned, GroupOwned, etc.</param>
        /// <param name="searchCategory">One of the values from the SearchCategory Struct, ie: Any, Linden, Newcomer</param>
        /// <param name="groupID">LLUID of group you want to recieve results for</param>
        /// <param name="transactionID">Transaction (Query) ID which can be associated with results from your request.</param>
        /// <returns>Transaction (Query) ID which can be associated with results from your request.</returns>
        public LLUUID StartPlacesSearch(DirFindFlags findFlags, Parcel.ParcelCategory searchCategory, LLUUID groupID, LLUUID transactionID)
        {
            return StartPlacesSearch(findFlags, searchCategory, String.Empty, String.Empty, groupID, transactionID);
        }

        /// <summary>
        /// Search Places - All Options
        /// </summary>
        /// <param name="findFlags">One of the Values from the DirFindFlags struct, ie: AgentOwned, GroupOwned, etc.</param>
        /// <param name="searchCategory">One of the values from the SearchCategory Struct, ie: Any, Linden, Newcomer</param>
        /// <param name="searchText">String Text to search for</param>
        /// <param name="simulatorName">String Simulator Name to search in</param>
        /// <param name="groupID">LLUID of group you want to recieve results for</param>
        /// <param name="transactionID">Transaction (Query) ID which can be associated with results from your request.</param>
        /// <returns>Transaction (Query) ID which can be associated with results from your request.</returns>
        public LLUUID StartPlacesSearch(DirFindFlags findFlags, Parcel.ParcelCategory searchCategory, string searchText, string simulatorName, LLUUID groupID, LLUUID transactionID)
        {
            PlacesQueryPacket find = new PlacesQueryPacket();
            find.AgentData.AgentID = Client.Self.AgentID;
            find.AgentData.SessionID = Client.Self.SessionID;
            find.AgentData.QueryID = groupID;

            find.TransactionData.TransactionID = transactionID;

            find.QueryData.QueryText = Helpers.StringToField(searchText);
            find.QueryData.QueryFlags = (uint)findFlags;
            find.QueryData.Category = (sbyte)searchCategory;
            find.QueryData.SimName = Helpers.StringToField(simulatorName);
            
            Client.Network.SendPacket(find);
            return transactionID;
        }


        /// <summary>
        /// Search All Events with specifid searchText in all categories, includes Mature
        /// </summary>
        /// <param name="searchText">Text to search for</param>
        /// <returns>UUID of query to correlate results in callback.</returns>
        public LLUUID StartEventsSearch(string searchText)
        {
            return StartEventsSearch(searchText, true, EventCategories.All);
        }

        /// <summary>
        /// Search Events with Options to specify category and Mature events.
        /// </summary>
        /// <param name="searchText">Text to search for</param>
        /// <param name="showMature">true to include Mature events</param>
        /// <param name="category">category to search</param>
        /// <returns>UUID of query to correlate results in callback.</returns>
        public LLUUID StartEventsSearch(string searchText, bool showMature, EventCategories category)
        {
            return StartEventsSearch(searchText, showMature, "u", 0, category, LLUUID.Random());
        }

        /// <summary>
        /// Search Events - ALL options
        /// </summary>
        /// <param name="searchText">string text to search for e.g.: live music</param>
        /// <param name="showMature">Include mature events in results</param>
        /// <param name="eventDay">"u" for now and upcoming events, -or- number of days since/until event is scheduled
        /// For example "0" = Today, "1" = tomorrow, "2" = following day, "-1" = yesterday, etc.</param>
        /// <param name="queryStart">Page # to show, 0 for First Page</param>
        /// <param name="category">EventCategory event is listed under.</param>
        /// <param name="queryID">a LLUUID that can be used to track queries with results.</param>
        /// <returns>UUID of query to correlate results in callback.</returns>
        public LLUUID StartEventsSearch(string searchText, bool showMature, string eventDay, uint queryStart, EventCategories category, LLUUID queryID)
        {
            DirFindQueryPacket find = new DirFindQueryPacket();
            find.AgentData.AgentID = Client.Self.AgentID;
            find.AgentData.SessionID = Client.Self.SessionID;
            
            find.QueryData.QueryID = queryID;
            find.QueryData.QueryText = Helpers.StringToField(eventDay + "|" + (int)category + "|" + searchText);
            find.QueryData.QueryFlags = showMature ? (uint)32 : (uint)8224;
            find.QueryData.QueryStart = (int)queryStart;

            Client.Network.SendPacket(find);
            return queryID;
        }

        /// <summary>Requests Event Details</summary>
        /// <param name="eventID">ID of Event returned from Places Search</param>
        public void EventInfoRequest(uint eventID)
        {
            EventInfoRequestPacket find = new EventInfoRequestPacket();
            find.AgentData.AgentID = Client.Self.AgentID;
            find.AgentData.SessionID = Client.Self.SessionID;

            find.EventData.EventID = eventID;

            Client.Network.SendPacket(find);
        }

        #region Blocking Functions

        public bool PeopleSearch(DirFindFlags findFlags, string searchText, int queryStart,
            int timeoutMS, out List<AgentSearchData> results)
        {
            AutoResetEvent searchEvent = new AutoResetEvent(false);
            LLUUID id = LLUUID.Random();
            List<AgentSearchData> people = null;

            DirPeopleReplyCallback callback =
                delegate(LLUUID queryid, List<AgentSearchData> matches)
                {
                    if (id == queryid)
                    {
                        people = matches;
                        searchEvent.Set();
                    }
                };

            OnDirPeopleReply += callback;
            StartPeopleSearch(findFlags, searchText, queryStart, id);
            searchEvent.WaitOne(timeoutMS, false);
            OnDirPeopleReply -= callback;

            results = people;
            return (results != null);
        }

        #endregion Blocking Functions

        #region Packet Handlers

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
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
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
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
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
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        protected void DirGroupsReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnDirGroupsReply != null)
            {
                DirGroupsReplyPacket groupsReply = packet as DirGroupsReplyPacket;
                List<GroupSearchData> matches = new List<GroupSearchData>(groupsReply.QueryReplies.Length);
                foreach (DirGroupsReplyPacket.QueryRepliesBlock reply in groupsReply.QueryReplies)
                {
                    GroupSearchData groupsData = new GroupSearchData();
                    groupsData.GroupID = reply.GroupID;
                    groupsData.GroupName = Helpers.FieldToUTF8String(reply.GroupName);
                    groupsData.Members = reply.Members;
                    matches.Add(groupsData);
                }
                try { OnDirGroupsReply(groupsReply.QueryData.QueryID, matches); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void PlacesReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnPlacesReply != null)
            {
                PlacesReplyPacket placesReply = packet as PlacesReplyPacket;
                List<PlacesSearchData> places = new List<PlacesSearchData>();

                foreach (PlacesReplyPacket.QueryDataBlock block in placesReply.QueryData)
                {
                    PlacesSearchData place = new PlacesSearchData();
                    place.OwnerID = block.OwnerID;
                    place.Name = Helpers.FieldToUTF8String(block.Name);
                    place.Desc = Helpers.FieldToUTF8String(block.Desc);
                    place.ActualArea = block.ActualArea;
                    place.BillableArea = block.BillableArea;
                    place.Flags = block.Flags;
                    place.GlobalX = block.GlobalX;
                    place.GlobalY = block.GlobalY;
                    place.GlobalZ = block.GlobalZ;
                    place.SimName = Helpers.FieldToUTF8String(block.SimName);
                    place.SnapshotID = block.SnapshotID;
                    place.Dwell = block.Dwell;
                    place.Price = block.Price;
                    
                    places.Add(place);
                }
                try { OnPlacesReply(placesReply.TransactionData.TransactionID, places); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void EventsReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnEventsReply != null)
            {
                DirEventsReplyPacket eventsReply = packet as DirEventsReplyPacket;
                List<EventsSearchData> matches = new List<EventsSearchData>(eventsReply.QueryReplies.Length);

                foreach (DirEventsReplyPacket.QueryRepliesBlock reply in eventsReply.QueryReplies)
                {
                    EventsSearchData eventsData = new EventsSearchData();
                    eventsData.Owner = reply.OwnerID;
                    eventsData.Name = Helpers.FieldToUTF8String(reply.Name);
                    eventsData.ID = reply.EventID;
                    eventsData.Date = Helpers.FieldToUTF8String(reply.Date);
                    eventsData.Time = reply.UnixTime;
                    eventsData.Flags = (EventFlags)reply.EventFlags;
                    matches.Add(eventsData);
                }

                try { OnEventsReply(eventsReply.QueryData.QueryID, matches); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }
        
        private void EventInfoReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnEventInfo != null)
            {
                EventInfoReplyPacket eventReply = (EventInfoReplyPacket)packet;
                EventInfo evinfo = new EventInfo();
                evinfo.ID = eventReply.EventData.EventID;
                evinfo.Name = Helpers.FieldToUTF8String(eventReply.EventData.Name);
                evinfo.Desc = Helpers.FieldToUTF8String(eventReply.EventData.Desc);
                evinfo.Amount = eventReply.EventData.Amount;
                evinfo.Category = (EventCategories)Helpers.BytesToUInt(eventReply.EventData.Category);
                evinfo.Cover = eventReply.EventData.Cover;
                evinfo.Creator = (LLUUID)Helpers.FieldToUTF8String(eventReply.EventData.Creator);
                evinfo.Date = Helpers.FieldToUTF8String(eventReply.EventData.Date);
                evinfo.DateUTC = eventReply.EventData.DateUTC;
                evinfo.Duration = eventReply.EventData.Duration;
                evinfo.Flags = (EventFlags)eventReply.EventData.EventFlags;
                evinfo.SimName = Helpers.FieldToUTF8String(eventReply.EventData.SimName);
                evinfo.GlobalPos = eventReply.EventData.GlobalPos;

                try { OnEventInfo(evinfo); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        #endregion Packet Handlers
    }
}
