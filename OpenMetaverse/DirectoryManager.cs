/*
 * Copyright (c) 2006-2009, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
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
using System.Threading;
using System.Collections.Generic;
using OpenMetaverse.Packets;
using OpenMetaverse.Interfaces;
using OpenMetaverse.Messages.Linden;

namespace OpenMetaverse
{
    /// <summary>
    /// Access to the data server which allows searching for land, events, people, etc
    /// </summary>
    public class DirectoryManager
    {
        #region Enums
        /// <summary>Classified Ad categories</summary>
        public enum ClassifiedCategories
        {
            /// <summary>Classified is listed in the Any category</summary>
            Any = 0,
            /// <summary>Classified is shopping related</summary>
            Shopping,
            /// <summary>Classified is </summary>
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

        /// <summary>Event Categories</summary>
        public enum EventCategories
        {
            /// <summary></summary>
            All = 0,
            /// <summary></summary>
            Discussion = 18,
            /// <summary></summary>
            Sports = 19,
            /// <summary></summary>
            LiveMusic = 20,
            /// <summary></summary>
            Commercial = 22,
            /// <summary></summary>
            Nightlife = 23,
            /// <summary></summary>
            Games = 24,
            /// <summary></summary>
            Pageants = 25,
            /// <summary></summary>
            Education = 26,
            /// <summary></summary>
            Arts = 27,
            /// <summary></summary>
            Charity = 28,
            /// <summary></summary>
            Miscellaneous = 29
        }

        /// <summary>
        /// Query Flags used in many of the DirectoryManager methods to specify which query to execute and how to return the results.
        /// 
        /// Flags can be combined using the | (pipe) character, not all flags are available in all queries
        /// </summary>
        [Flags]
        public enum DirFindFlags
        {
            /// <summary>Query the People database</summary>
            People = 1 << 0,
            /// <summary></summary>
            Online = 1 << 1,
            // <summary></summary>
            //[Obsolete]
            //Places = 1 << 2,
            /// <summary></summary>
            Events = 1 << 3,
            /// <summary>Query the Groups database</summary>
            Groups = 1 << 4,
            /// <summary>Query the Events database</summary>
            DateEvents = 1 << 5,
            /// <summary>Query the land holdings database for land owned by the currently connected agent</summary>
            AgentOwned = 1 << 6,
            /// <summary></summary>
            ForSale = 1 << 7,
            /// <summary>Query the land holdings database for land which is owned by a Group</summary>
            GroupOwned = 1 << 8,
            // <summary></summary>
            //[Obsolete]
            //Auction = 1 << 9,
            /// <summary>Specifies the query should pre sort the results based upon traffic
            /// when searching the Places database</summary>
            DwellSort = 1 << 10,
            /// <summary></summary>
            PgSimsOnly = 1 << 11,
            /// <summary></summary>
            PicturesOnly = 1 << 12,
            /// <summary></summary>
            PgEventsOnly = 1 << 13,
            /// <summary></summary>
            MatureSimsOnly = 1 << 14,
            /// <summary>Specifies the query should pre sort the results in an ascending order when searching the land sales database. 
            /// This flag is only used when searching the land sales database</summary>
            SortAsc = 1 << 15,
            /// <summary>Specifies the query should pre sort the results using the SalePrice field when searching the land sales database. 
            /// This flag is only used when searching the land sales database</summary>
            PricesSort = 1 << 16,
            /// <summary>Specifies the query should pre sort the results by calculating the average price/sq.m (SalePrice / Area) when searching the land sales database. 
            /// This flag is only used when searching the land sales database</summary>
            PerMeterSort = 1 << 17,
            /// <summary>Specifies the query should pre sort the results using the ParcelSize field when searching the land sales database. 
            /// This flag is only used when searching the land sales database</summary>
            AreaSort = 1 << 18,
            /// <summary>Specifies the query should pre sort the results using the Name field when searching the land sales database. 
            /// This flag is only used when searching the land sales database</summary>
            NameSort = 1 << 19,
            /// <summary>When set, only parcels less than the specified Price will be included when searching the land sales database.
            /// This flag is only used when searching the land sales database</summary>
            LimitByPrice = 1 << 20,
            /// <summary>When set, only parcels greater than the specified Size will be included when searching the land sales database.
            /// This flag is only used when searching the land sales database</summary>
            LimitByArea = 1 << 21,
            /// <summary></summary>
            FilterMature = 1 << 22,
            /// <summary></summary>
            PGOnly = 1 << 23,
            /// <summary>Include PG land in results. This flag is used when searching both the Events and Land sales databases</summary>
            IncludePG = 1 << 24,
            /// <summary>Include Mature land in results. This flag is used when searching both the Events and Land sales databases</summary>
            IncludeMature = 1 << 25,
            /// <summary>Include Adult land in results. This flag is used when searching both the Events and Land sales databases</summary>
            IncludeAdult = 1 << 26,
            /// <summary></summary>
            AdultOnly = 1 << 27
        }

        /// <summary>
        /// Land types to search dataserver for
        /// </summary>
        [Flags]
        public enum SearchTypeFlags
        {
            /// <summary>Search Auction, Mainland and Estate</summary>
            Any = -1,
            /// <summary>Land which is currently up for auction</summary>
            Auction = 1 << 1,
            // <summary>Land available to new landowners (formerly the FirstLand program)</summary>
            //[Obsolete]
            //Newbie = 1 << 2,
            /// <summary>Parcels which are on the mainland (Linden owned) continents</summary>
            Mainland = 1 << 3,
            /// <summary>Parcels which are on privately owned simulators</summary>
            Estate = 1 << 4
        }

        /// <summary>
        /// The content rating of the event
        /// </summary>
        public enum EventFlags
        {
            /// <summary>Event is PG</summary>
            PG = 0,
            /// <summary>Event is Mature</summary>
            Mature = 1,
            /// <summary>Event is Adult</summary>
            Adult = 2
        }

        /// <summary>
        /// Classified Ad Options
        /// </summary>
        /// <remarks>There appear to be two formats the flags are packed in.
        /// This set of flags is for the newer style</remarks>
        [Flags]
        public enum ClassifiedFlags : byte
        {
            None = 1 << 0,
            Mature = 1 << 1,
            Enabled = 1 << 2,
            // HasPrice = 1 << 3, // Deprecated
            UpdateTime = 1 << 4,
            AutoRenew = 1 << 5
        }

        /// <summary>
        /// Classified ad query options
        /// </summary>
        [Flags]
        public enum ClassifiedQueryFlags
        {
            /// <summary>Include all ads in results</summary>
            All = PG | Mature | Adult,
            /// <summary>Include PG ads in results</summary>
            PG = 1 << 2,
            /// <summary>Include Mature ads in results</summary>
            Mature = 1 << 3,
            /// <summary>Include Adult ads in results</summary>
            Adult = 1 << 6,            
        }

        /// <summary>
        /// The For Sale flag in PlacesReplyData
        /// </summary>
        public enum PlacesFlags : byte
        {
            /// <summary>Parcel is not listed for sale</summary>
            NotForSale = 0,
            /// <summary>Parcel is For Sale</summary>
            ForSale = 128
        }

        #endregion
        #region Structs
        /// <summary>
        /// A classified ad on the grid
        /// </summary>
        public struct Classified
        {
            /// <summary>UUID for this ad, useful for looking up detailed
            /// information about it</summary>
            public UUID ID;
            /// <summary>The title of this classified ad</summary>
            public string Name;
            /// <summary>Flags that show certain options applied to the classified</summary>
            public ClassifiedFlags Flags;
            /// <summary>Creation date of the ad</summary>
            public DateTime CreationDate;
            /// <summary>Expiration date of the ad</summary>
            public DateTime ExpirationDate;
            /// <summary>Price that was paid for this ad</summary>
            public int Price;

            /// <summary>Print the struct data as a string</summary>
            /// <returns>A string containing the field name, and field value</returns>
            public override string ToString()
            {
                return Helpers.StructToString(this);
            }
        }

        /// <summary>
        /// A parcel retrieved from the dataserver such as results from the 
        /// "For-Sale" listings or "Places" Search
        /// </summary>
        public struct DirectoryParcel
        {
            /// <summary>The unique dataserver parcel ID</summary>
            /// <remarks>This id is used to obtain additional information from the entry
            /// by using the <see cref="ParcelManager.InfoRequest"/> method</remarks>
            public UUID ID;
            /// <summary>A string containing the name of the parcel</summary>
            public string Name;
            /// <summary>The size of the parcel</summary>
            /// <remarks>This field is not returned for Places searches</remarks>
            public int ActualArea;
            /// <summary>The price of the parcel</summary>
            /// <remarks>This field is not returned for Places searches</remarks>
            public int SalePrice;
            /// <summary>If True, this parcel is flagged to be auctioned</summary>
            public bool Auction;
            /// <summary>If true, this parcel is currently set for sale</summary>
            public bool ForSale;
            /// <summary>Parcel traffic</summary>
            public float Dwell;

            /// <summary>Print the struct data as a string</summary>
            /// <returns>A string containing the field name, and field value</returns>
            public override string ToString()
            {
                return Helpers.StructToString(this);
            }
        }

        /// <summary>
        /// An Avatar returned from the dataserver
        /// </summary>
        public struct AgentSearchData
        {
            /// <summary>Online status of agent</summary>
            /// <remarks>This field appears to be obsolete and always returns false</remarks>
            public bool Online;
            /// <summary>The agents first name</summary>
            public string FirstName;
            /// <summary>The agents last name</summary>
            public string LastName;
            /// <summary>The agents <see cref="UUID"/></summary>
            public UUID AgentID;

            /// <summary>Print the struct data as a string</summary>
            /// <returns>A string containing the field name, and field value</returns>
            public override string ToString()
            {
                return Helpers.StructToString(this);
            }
        }

        /// <summary>
        ///  Response to a "Groups" Search
        /// </summary>
        public struct GroupSearchData
        {
            /// <summary>The Group ID</summary>
            public UUID GroupID;
            /// <summary>The name of the group</summary>
            public string GroupName;
            /// <summary>The current number of members</summary>
            public int Members;

            /// <summary>Print the struct data as a string</summary>
            /// <returns>A string containing the field name, and field value</returns>
            public override string ToString()
            {
                return Helpers.StructToString(this);
            }
        }

        /// <summary>
        /// Parcel information returned from a <see cref="StartPlacesSearch"/> request
        /// <para>
        /// Represents one of the following:
        /// A parcel of land on the grid that has its Show In Search flag set
        /// A parcel of land owned by the agent making the request
        /// A parcel of land owned by a group the agent making the request is a member of
        /// </para>
        /// <para>
        /// In a request for Group Land, the First record will contain an empty record
        /// </para>
        /// Note: This is not the same as searching the land for sale data source
        /// </summary>
        public struct PlacesSearchData
        {
            /// <summary>The ID of the Agent of Group that owns the parcel</summary>
            public UUID OwnerID;
            /// <summary>The name</summary>
            public string Name;
            /// <summary>The description</summary>
            public string Desc;
            /// <summary>The Size of the parcel</summary>
            public int ActualArea;
            /// <summary>The billable Size of the parcel, for mainland
            /// parcels this will match the ActualArea field. For Group owned land this will be 10 percent smaller
            /// than the ActualArea. For Estate land this will always be 0</summary>
            public int BillableArea;
            /// <summary>Indicates the ForSale status of the parcel</summary>
            public PlacesFlags Flags;
            /// <summary>The Gridwide X position</summary>
            public float GlobalX;
            /// <summary>The Gridwide Y position</summary>
            public float GlobalY;
            /// <summary>The Z position of the parcel, or 0 if no landing point set</summary>
            public float GlobalZ;
            /// <summary>The name of the Region the parcel is located in</summary>
            public string SimName;
            /// <summary>The Asset ID of the parcels Snapshot texture</summary>
            public UUID SnapshotID;
            /// <summary>The calculated visitor traffic</summary>
            public float Dwell;
            /// <summary>The billing product SKU</summary>
            /// <remarks>Known values are:
            /// <list type="table">
            /// <item><term>023</term><description>Mainland / Full Region</description></item>
            /// <item><term>024</term><description>Estate / Full Region</description></item>
            /// <item><term>027</term><description>Estate / Openspace</description></item>
            /// <item><term>029</term><description>Estate / Homestead</description></item>
            /// <item><term>129</term><description>Mainland / Homestead (Linden Owned)</description></item>
            /// </list>
            /// </remarks>
            public string SKU;
            /// <summary>No longer used, will always be 0</summary>
            public int Price;

            /// <summary>Get a SL URL for the parcel</summary>
            /// <returns>A string, containing a standard SLURL</returns>
            public string ToSLurl()
            {
                float x, y;
                Helpers.GlobalPosToRegionHandle(this.GlobalX, this.GlobalY, out x, out y);
                return "secondlife://" + this.SimName + "/" + x + "/" + y + "/" + this.GlobalZ;
            }

            /// <summary>Print the struct data as a string</summary>
            /// <returns>A string containing the field name, and field value</returns>
            public override string ToString()
            {
                return Helpers.StructToString(this);
            }
        }

        /// <summary>
        /// An "Event" Listing summary
        /// </summary>
        public struct EventsSearchData
        {
            /// <summary>The ID of the event creator</summary>
            public UUID Owner;
            /// <summary>The name of the event</summary>
            public string Name;
            /// <summary>The events ID</summary>
            public uint ID;
            /// <summary>A string containing the short date/time the event will begin</summary>
            public string Date;
            /// <summary>The event start time in Unixtime (seconds since epoch)</summary>
            public uint Time;
            /// <summary>The events maturity rating</summary>
            public EventFlags Flags;

            /// <summary>Print the struct data as a string</summary>
            /// <returns>A string containing the field name, and field value</returns>
            public override string ToString()
            {
                return Helpers.StructToString(this);
            }
        }


        /// <summary>
        /// The details of an "Event"
        /// </summary>
        public struct EventInfo
        {
            /// <summary>The events ID</summary>
            public uint ID;
            /// <summary>The ID of the event creator</summary>
            public UUID Creator;
            /// <summary>The name of the event</summary>
            public string Name;
            /// <summary>The category</summary>
            public EventCategories Category;
            /// <summary>The events description</summary>
            public string Desc;
            /// <summary>The short date/time the event will begin</summary>
            public string Date;
            /// <summary>The event start time in Unixtime (seconds since epoch) UTC adjusted</summary>
            public uint DateUTC;
            /// <summary>The length of the event in minutes</summary>
            public uint Duration;
            /// <summary>0 if no cover charge applies</summary>
            public uint Cover;
            /// <summary>The cover charge amount in L$ if applicable</summary>
            public uint Amount;
            /// <summary>The name of the region where the event is being held</summary>
            public string SimName;
            /// <summary>The gridwide location of the event</summary>
            public Vector3d GlobalPos;
            /// <summary>The maturity rating</summary>
            public EventFlags Flags;

            /// <summary>Get a SL URL for the parcel where the event is hosted</summary>
            /// <returns>A string, containing a standard SLURL</returns>
            public string ToSLurl()
            {
                float x, y;
                Helpers.GlobalPosToRegionHandle((float)this.GlobalPos.X, (float)this.GlobalPos.Y, out x, out y);
                return "secondlife://" + this.SimName + "/" + x + "/" + y + "/" + this.GlobalPos.Z;
            }

            /// <summary>Print the struct data as a string</summary>
            /// <returns>A string containing the field name, and field value</returns>
            public override string ToString()
            {
                return Helpers.StructToString(this);
            }
        }

        #endregion Structs
        
        #region Event delegates, Raise Events
        
        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<EventInfoReplyEventArgs> m_EventInfoReply;

        /// <summary>Raises the EventInfoReply event</summary>
        /// <param name="e">An EventInfoReplyEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnEventInfo(EventInfoReplyEventArgs e)
        {
            EventHandler<EventInfoReplyEventArgs> handler = m_EventInfoReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_EventDetailLock = new object();

        /// <summary>Raised when the data server responds to a <see cref="EventInfoRequest"/> request.</summary>
        public event EventHandler<EventInfoReplyEventArgs> EventInfoReply
        {            
            add { lock (m_EventDetailLock) { m_EventInfoReply += value; } }
            remove { lock (m_EventDetailLock) { m_EventInfoReply -= value; } }
        }
        
        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<DirEventsReplyEventArgs> m_DirEvents;

        /// <summary>Raises the DirEventsReply event</summary>
        /// <param name="e">An DirEventsReplyEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnDirEvents(DirEventsReplyEventArgs e)
        {
            EventHandler<DirEventsReplyEventArgs> handler = m_DirEvents;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_DirEventsLock = new object();

        /// <summary>Raised when the data server responds to a <see cref="StartEventsSearch"/> request.</summary>
        public event EventHandler<DirEventsReplyEventArgs> DirEventsReply
        {
            add { lock (m_DirEventsLock) { m_DirEvents += value; } }
            remove { lock (m_DirEventsLock) { m_DirEvents -= value; } }
        }
        
        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<PlacesReplyEventArgs> m_Places;

        /// <summary>Raises the PlacesReply event</summary>
        /// <param name="e">A PlacesReplyEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnPlaces(PlacesReplyEventArgs e)
        {
            EventHandler<PlacesReplyEventArgs> handler = m_Places;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_PlacesLock = new object();

        /// <summary>Raised when the data server responds to a <see cref="StartPlacesSearch"/> request.</summary>
        public event EventHandler<PlacesReplyEventArgs> PlacesReply
        {
            add { lock (m_PlacesLock) { m_Places += value; } }
            remove { lock (m_PlacesLock) { m_Places -= value; } }
        }
        
        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<DirPlacesReplyEventArgs> m_DirPlaces;

        /// <summary>Raises the DirPlacesReply event</summary>
        /// <param name="e">A DirPlacesReplyEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnDirPlaces(DirPlacesReplyEventArgs e)
        {
            EventHandler<DirPlacesReplyEventArgs> handler = m_DirPlaces;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_DirPlacesLock = new object();

        /// <summary>Raised when the data server responds to a <see cref="StartDirPlacesSearch"/> request.</summary>
        public event EventHandler<DirPlacesReplyEventArgs> DirPlacesReply
        {
            add { lock (m_DirPlacesLock) { m_DirPlaces += value; } }
            remove { lock (m_DirPlacesLock) { m_DirPlaces -= value; } }
        }
        
        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<DirClassifiedsReplyEventArgs> m_DirClassifieds;

        /// <summary>Raises the DirClassifiedsReply event</summary>
        /// <param name="e">A DirClassifiedsReplyEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnDirClassifieds(DirClassifiedsReplyEventArgs e)
        {
            EventHandler<DirClassifiedsReplyEventArgs> handler = m_DirClassifieds;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_DirClassifiedsLock = new object();

        /// <summary>Raised when the data server responds to a <see cref="StartClassifiedSearch"/> request.</summary>
        public event EventHandler<DirClassifiedsReplyEventArgs> DirClassifiedsReply
        {
            add { lock (m_DirClassifiedsLock) { m_DirClassifieds += value; } }
            remove { lock (m_DirClassifiedsLock) { m_DirClassifieds -= value; } }
        }
        
        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<DirGroupsReplyEventArgs> m_DirGroups;

        /// <summary>Raises the DirGroupsReply event</summary>
        /// <param name="e">A DirGroupsReplyEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnDirGroups(DirGroupsReplyEventArgs e)
        {
            EventHandler<DirGroupsReplyEventArgs> handler = m_DirGroups;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_DirGroupsLock = new object();

        /// <summary>Raised when the data server responds to a <see cref="StartGroupSearch"/> request.</summary>
        public event EventHandler<DirGroupsReplyEventArgs> DirGroupsReply
        {
            add { lock (m_DirGroupsLock) { m_DirGroups += value; } }
            remove { lock (m_DirGroupsLock) { m_DirGroups -= value; } }
        }
        
        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<DirPeopleReplyEventArgs> m_DirPeople;

        /// <summary>Raises the DirPeopleReply event</summary>
        /// <param name="e">A DirPeopleReplyEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnDirPeople(DirPeopleReplyEventArgs e)
        {
            EventHandler<DirPeopleReplyEventArgs> handler = m_DirPeople;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_DirPeopleLock = new object();

        /// <summary>Raised when the data server responds to a <see cref="StartPeopleSearch"/> request.</summary>
        public event EventHandler<DirPeopleReplyEventArgs> DirPeopleReply
        {
            add { lock (m_DirPeopleLock) { m_DirPeople += value; } }
            remove { lock (m_DirPeopleLock) { m_DirPeople -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<DirLandReplyEventArgs> m_DirLandReply;

        /// <summary>Raises the DirLandReply event</summary>
        /// <param name="e">A DirLandReplyEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnDirLand(DirLandReplyEventArgs e)
        {
            EventHandler<DirLandReplyEventArgs> handler = m_DirLandReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_DirLandLock = new object();

        /// <summary>Raised when the data server responds to a <see cref="StartLandSearch"/> request.</summary>
        public event EventHandler<DirLandReplyEventArgs> DirLandReply
        {
            add { lock (m_DirLandLock) { m_DirLandReply += value; } }
            remove { lock (m_DirLandLock) { m_DirLandReply -= value; } }
        }        

        #endregion

        #region Private Members
        private GridClient Client;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a new instance of the DirectoryManager class
        /// </summary>
        /// <param name="client">An instance of GridClient</param>
        public DirectoryManager(GridClient client)
        {
            Client = client;

            Client.Network.RegisterCallback(PacketType.DirClassifiedReply, new NetworkManager.PacketCallback(DirClassifiedReplyHandler));
            // Deprecated, replies come in over capabilities
            Client.Network.RegisterCallback(PacketType.DirLandReply, new NetworkManager.PacketCallback(DirLandReplyHandler));
            Client.Network.RegisterEventCallback("DirLandReply", DirLandReplyEventHandler);
            Client.Network.RegisterCallback(PacketType.DirPeopleReply, new NetworkManager.PacketCallback(DirPeopleReplyHandler));
            Client.Network.RegisterCallback(PacketType.DirGroupsReply, new NetworkManager.PacketCallback(DirGroupsReplyHandler));
            // Deprecated as of viewer 1.2.3
            Client.Network.RegisterCallback(PacketType.PlacesReply, new NetworkManager.PacketCallback(PlacesReplyHandler));
            Client.Network.RegisterEventCallback("PlacesReply", PlacesReplyEventHandler);
            Client.Network.RegisterCallback(PacketType.DirEventsReply, new NetworkManager.PacketCallback(EventsReplyHandler));
            Client.Network.RegisterCallback(PacketType.EventInfoReply, new NetworkManager.PacketCallback(EventInfoReplyHandler));
            Client.Network.RegisterCallback(PacketType.DirPlacesReply, new NetworkManager.PacketCallback(DirPlacesReplyHandler));
        }

        #endregion

        #region Public Methods
        // Obsoleted due to new Adult search option
        [Obsolete("Use Overload with ClassifiedQueryFlags option instead")]
        public UUID StartClassifiedSearch(string searchText, ClassifiedCategories category, bool mature)
        {
            return UUID.Zero;
        }

        /// <summary>
        /// Query the data server for a list of classified ads containing the specified string.
        /// Defaults to searching for classified placed in any category, and includes PG, Adult and Mature 
        /// results.
        /// 
        /// Responses are sent 16 per response packet, there is no way to know how many results a query reply will contain however assuming
        /// the reply packets arrived ordered, a response with less than 16 entries would indicate all results have been received
        /// 
        /// The <see cref="OnClassifiedReply"/> event is raised when a response is received from the simulator
        /// </summary>
        /// <param name="searchText">A string containing a list of keywords to search for</param>
        /// <returns>A UUID to correlate the results when the <see cref="OnClassifiedReply"/> event is raised</returns>
        public UUID StartClassifiedSearch(string searchText)
        {
            return StartClassifiedSearch(searchText, ClassifiedCategories.Any, ClassifiedQueryFlags.All);
        }

        /// <summary>
        /// Query the data server for a list of classified ads which contain specified keywords (Overload)
        /// 
        /// The <see cref="OnClassifiedReply"/> event is raised when a response is received from the simulator
        /// </summary>
        /// <param name="searchText">A string containing a list of keywords to search for</param>
        /// <param name="category">The category to search</param>
        /// <param name="queryFlags">A set of flags which can be ORed to modify query options 
        /// such as classified maturity rating.</param>
        /// <returns>A UUID to correlate the results when the <see cref="OnClassifiedReply"/> event is raised</returns>
        /// <example>
        /// Search classified ads containing the key words "foo" and "bar" in the "Any" category that are either PG or Mature
        /// <code>
        /// UUID searchID = StartClassifiedSearch("foo bar", ClassifiedCategories.Any, ClassifiedQueryFlags.PG | ClassifiedQueryFlags.Mature);
        /// </code>
        /// </example>
        /// <remarks>        
        /// Responses are sent 16 at a time, there is no way to know how many results a query reply will contain however assuming
        /// the reply packets arrived ordered, a response with less than 16 entries would indicate all results have been received
        /// </remarks>
        public UUID StartClassifiedSearch(string searchText, ClassifiedCategories category, ClassifiedQueryFlags queryFlags)
        {
            DirClassifiedQueryPacket query = new DirClassifiedQueryPacket();
            UUID queryID = UUID.Random();

            query.AgentData.AgentID = Client.Self.AgentID;
            query.AgentData.SessionID = Client.Self.SessionID;

            query.QueryData.Category = (uint)category;
            query.QueryData.QueryFlags = (uint)queryFlags;
            query.QueryData.QueryID = queryID;
            query.QueryData.QueryText = Utils.StringToBytes(searchText);

            Client.Network.SendPacket(query);

            return queryID;
        }

        /// <summary>
        /// Starts search for places (Overloaded)
        /// 
        /// The <see cref="OnDirPlacesReply"/> event is raised when a response is received from the simulator
        /// </summary>
        /// <param name="searchText">Search text</param>
        /// <param name="queryStart">Each request is limited to 100 places
        /// being returned. To get the first 100 result entries of a request use 0,
        /// from 100-199 use 1, 200-299 use 2, etc.</param>        
        /// <returns>A UUID to correlate the results when the <see cref="OnDirPlacesReply"/> event is raised</returns>
        public UUID StartDirPlacesSearch(string searchText, int queryStart)
        {
            return StartDirPlacesSearch(searchText, DirFindFlags.DwellSort | DirFindFlags.IncludePG | DirFindFlags.IncludeMature
                | DirFindFlags.IncludeAdult, ParcelCategory.Any, queryStart);
        }

        /// <summary>
        /// Queries the dataserver for parcels of land which are flagged to be shown in search
        /// 
        /// The <see cref="OnDirPlacesReply"/> event is raised when a response is received from the simulator
        /// </summary>
        /// <param name="searchText">A string containing a list of keywords to search for separated by a space character</param>
        /// <param name="queryFlags">A set of flags which can be ORed to modify query options 
        /// such as classified maturity rating.</param>
        /// <param name="category">The category to search</param>        
        /// <param name="queryStart">Each request is limited to 100 places
        /// being returned. To get the first 100 result entries of a request use 0,
        /// from 100-199 use 1, 200-299 use 2, etc.</param>        
        /// <returns>A UUID to correlate the results when the <see cref="OnDirPlacesReply"/> event is raised</returns>
        /// <example>
        /// Search places containing the key words "foo" and "bar" in the "Any" category that are either PG or Adult
        /// <code>
        /// UUID searchID = StartDirPlacesSearch("foo bar", DirFindFlags.DwellSort | DirFindFlags.IncludePG | DirFindFlags.IncludeAdult, ParcelCategory.Any, 0);
        /// </code>
        /// </example>
        /// <remarks>        
        /// Additional information on the results can be obtained by using the ParcelManager.InfoRequest method
        /// </remarks>
        public UUID StartDirPlacesSearch(string searchText, DirFindFlags queryFlags, ParcelCategory category, int queryStart)
        {
            DirPlacesQueryPacket query = new DirPlacesQueryPacket();

            UUID queryID = UUID.Random();

            query.AgentData.AgentID = Client.Self.AgentID;
            query.AgentData.SessionID = Client.Self.SessionID;

            query.QueryData.Category = (sbyte)category;
            query.QueryData.QueryFlags = (uint)queryFlags;

            query.QueryData.QueryID = queryID;
            query.QueryData.QueryText = Utils.StringToBytes(searchText);
            query.QueryData.QueryStart = queryStart;
            query.QueryData.SimName = Utils.StringToBytes(string.Empty);

            Client.Network.SendPacket(query);

            return queryID;

        }

        /// <summary>
        /// Starts a search for land sales using the directory
        /// 
        /// The <see cref="OnDirLandReply"/> event is raised when a response is received from the simulator
        /// </summary>
        /// <param name="typeFlags">What type of land to search for. Auction, 
        /// estate, mainland, "first land", etc</param>        
        /// <remarks>The OnDirLandReply event handler must be registered before
        /// calling this function. There is no way to determine how many 
        /// results will be returned, or how many times the callback will be 
        /// fired other than you won't get more than 100 total parcels from 
        /// each query.</remarks>
        public void StartLandSearch(SearchTypeFlags typeFlags)
        {
            StartLandSearch(DirFindFlags.SortAsc | DirFindFlags.PerMeterSort, typeFlags, 0, 0, 0);
        }

        /// <summary>
        /// Starts a search for land sales using the directory
        /// 
        /// The <seealso cref="OnDirLandReply"/> event is raised when a response is received from the simulator
        /// </summary>
        /// <param name="typeFlags">What type of land to search for. Auction, 
        /// estate, mainland, "first land", etc</param>
        /// <param name="priceLimit">Maximum price to search for</param>
        /// <param name="areaLimit">Maximum area to search for</param>
        /// <param name="queryStart">Each request is limited to 100 parcels
        /// being returned. To get the first 100 parcels of a request use 0,
        /// from 100-199 use 1, 200-299 use 2, etc.</param>        
        /// <remarks>The OnDirLandReply event handler must be registered before
        /// calling this function. There is no way to determine how many 
        /// results will be returned, or how many times the callback will be 
        /// fired other than you won't get more than 100 total parcels from 
        /// each query.</remarks>
        public void StartLandSearch(SearchTypeFlags typeFlags, int priceLimit, int areaLimit, int queryStart)
        {
            StartLandSearch(DirFindFlags.SortAsc | DirFindFlags.PerMeterSort | DirFindFlags.LimitByPrice |
                DirFindFlags.LimitByArea, typeFlags, priceLimit, areaLimit, queryStart);
        }

        /// <summary>
        /// Send a request to the data server for land sales listings
        /// </summary>
        /// 
        /// <param name="findFlags">Flags sent to specify query options
        /// 
        /// Available flags:
        /// Specify the parcel rating with one or more of the following:
        ///     IncludePG IncludeMature IncludeAdult
        /// 
        /// Specify the field to pre sort the results with ONLY ONE of the following:
        ///     PerMeterSort NameSort AreaSort PricesSort
        ///     
        /// Specify the order the results are returned in, if not specified the results are pre sorted in a Descending Order
        ///     SortAsc
        ///     
        /// Specify additional filters to limit the results with one or both of the following:
        ///     LimitByPrice LimitByArea
        ///     
        /// Flags can be combined by separating them with the | (pipe) character
        /// 
        /// Additional details can be found in <see cref="DirFindFlags"/>
        /// </param>
        /// <param name="typeFlags">What type of land to search for. Auction, 
        /// Estate or Mainland</param>
        /// <param name="priceLimit">Maximum price to search for when the 
        /// DirFindFlags.LimitByPrice flag is specified in findFlags</param>
        /// <param name="areaLimit">Maximum area to search for when the
        /// DirFindFlags.LimitByArea flag is specified in findFlags</param>
        /// <param name="queryStart">Each request is limited to 100 parcels
        /// being returned. To get the first 100 parcels of a request use 0,
        /// from 100-199 use 100, 200-299 use 200, etc.</param>
        /// <remarks><para>The <seealso cref="OnDirLandReply"/> event will be raised with the response from the simulator 
        /// 
        /// There is no way to determine how many results will be returned, or how many times the callback will be 
        /// fired other than you won't get more than 100 total parcels from 
        /// each reply.</para>
        /// 
        /// <para>Any land set for sale to either anybody or specific to the connected agent will be included in the
        /// results if the land is included in the query</para></remarks>
        /// <example>
        /// <code>
        /// // request all mainland, any maturity rating that is larger than 512 sq.m
        /// StartLandSearch(DirFindFlags.SortAsc | DirFindFlags.PerMeterSort | DirFindFlags.LimitByArea | DirFindFlags.IncludePG | DirFindFlags.IncludeMature | DirFindFlags.IncludeAdult, SearchTypeFlags.Mainland, 0, 512, 0);
        /// </code></example>
        public void StartLandSearch(DirFindFlags findFlags, SearchTypeFlags typeFlags, int priceLimit,
            int areaLimit, int queryStart)
        {
            DirLandQueryPacket query = new DirLandQueryPacket();
            query.AgentData.AgentID = Client.Self.AgentID;
            query.AgentData.SessionID = Client.Self.SessionID;
            query.QueryData.Area = areaLimit;
            query.QueryData.Price = priceLimit;
            query.QueryData.QueryStart = queryStart;
            query.QueryData.SearchType = (uint)typeFlags;
            query.QueryData.QueryFlags = (uint)findFlags;
            query.QueryData.QueryID = UUID.Random();

            Client.Network.SendPacket(query);            
        }
       
        /// <summary>
        /// Search for Groups
        /// </summary>
        /// <param name="searchText">The name or portion of the name of the group you wish to search for</param>
        /// <param name="queryStart"></param>
        /// <returns></returns>
        public UUID StartGroupSearch(string searchText, int queryStart)
        {
            DirFindQueryPacket find = new DirFindQueryPacket();
            find.AgentData.AgentID = Client.Self.AgentID;
            find.AgentData.SessionID = Client.Self.SessionID;
            find.QueryData.QueryFlags = (uint)DirFindFlags.Groups;
            find.QueryData.QueryText = Utils.StringToBytes(searchText);
            find.QueryData.QueryID = UUID.Random();
            find.QueryData.QueryStart = queryStart;

            Client.Network.SendPacket(find);

            return find.QueryData.QueryID;
        }
        
        /// <summary>
        /// Search the People directory for other avatars
        /// </summary>
        /// <param name="searchText">The name or portion of the name of the avatar you wish to search for</param>
        /// <param name="queryStart"></param>
        /// <returns></returns>
        public UUID StartPeopleSearch(string searchText, int queryStart)
        {
            DirFindQueryPacket find = new DirFindQueryPacket();
            find.AgentData.AgentID = Client.Self.AgentID;
            find.AgentData.SessionID = Client.Self.SessionID;
            find.QueryData.QueryFlags = (uint)DirFindFlags.People;
            find.QueryData.QueryText = Utils.StringToBytes(searchText);
            find.QueryData.QueryID = UUID.Random();
            find.QueryData.QueryStart = queryStart;

            Client.Network.SendPacket(find);

            return find.QueryData.QueryID;
        }
       
        /// <summary>
        /// Search Places for parcels of land you personally own
        /// </summary>
        public UUID StartPlacesSearch()
        {
            return StartPlacesSearch(DirFindFlags.AgentOwned, ParcelCategory.Any, String.Empty, String.Empty,
                UUID.Zero, UUID.Random());
        }

        /// <summary>
        /// Searches Places for land owned by the specified group
        /// </summary>
        /// <param name="groupID">ID of the group you want to recieve land list for (You must be a member of the group)</param>
        /// <returns>Transaction (Query) ID which can be associated with results from your request.</returns>
        public UUID StartPlacesSearch(UUID groupID)
        {
            return StartPlacesSearch(DirFindFlags.GroupOwned, ParcelCategory.Any, String.Empty, String.Empty, 
                groupID, UUID.Random());
        }

        /// <summary>
        /// Search the Places directory for parcels that are listed in search and contain the specified keywords
        /// </summary>
        /// <param name="searchText">A string containing the keywords to search for</param>
        /// <returns>Transaction (Query) ID which can be associated with results from your request.</returns>
        public UUID StartPlacesSearch(string searchText)
        {
            return StartPlacesSearch(DirFindFlags.DwellSort | DirFindFlags.IncludePG | DirFindFlags.IncludeMature | DirFindFlags.IncludeAdult, 
                ParcelCategory.Any, searchText, String.Empty, UUID.Zero, UUID.Random());
        }

        /// <summary>
        /// Search Places - All Options
        /// </summary>
        /// <param name="findFlags">One of the Values from the DirFindFlags struct, ie: AgentOwned, GroupOwned, etc.</param>
        /// <param name="searchCategory">One of the values from the SearchCategory Struct, ie: Any, Linden, Newcomer</param>
        /// <param name="searchText">A string containing a list of keywords to search for separated by a space character</param>
        /// <param name="simulatorName">String Simulator Name to search in</param>
        /// <param name="groupID">LLUID of group you want to recieve results for</param>
        /// <param name="transactionID">Transaction (Query) ID which can be associated with results from your request.</param>
        /// <returns>Transaction (Query) ID which can be associated with results from your request.</returns>
        public UUID StartPlacesSearch(DirFindFlags findFlags, ParcelCategory searchCategory, string searchText, string simulatorName, 
            UUID groupID, UUID transactionID)
        {
            PlacesQueryPacket find = new PlacesQueryPacket();
            find.AgentData.AgentID = Client.Self.AgentID;
            find.AgentData.SessionID = Client.Self.SessionID;
            find.AgentData.QueryID = groupID;

            find.TransactionData.TransactionID = transactionID;

            find.QueryData.QueryText = Utils.StringToBytes(searchText);
            find.QueryData.QueryFlags = (uint)findFlags;
            find.QueryData.Category = (sbyte)searchCategory;
            find.QueryData.SimName = Utils.StringToBytes(simulatorName);

            Client.Network.SendPacket(find);
            return transactionID;
        }

        /// <summary>
        /// Search All Events with specifid searchText in all categories, includes PG, Mature and Adult
        /// </summary>
        /// <param name="searchText">A string containing a list of keywords to search for separated by a space character</param>
        /// <param name="queryStart">Each request is limited to 100 entries
        /// being returned. To get the first group of entries of a request use 0,
        /// from 100-199 use 100, 200-299 use 200, etc.</param>
        /// <returns>UUID of query to correlate results in callback.</returns>
        public UUID StartEventsSearch(string searchText, uint queryStart)
        {
            return StartEventsSearch(searchText, DirFindFlags.DateEvents | DirFindFlags.IncludePG | DirFindFlags.IncludeMature | DirFindFlags.IncludeAdult, 
                "u", queryStart, EventCategories.All);
        }

        /// <summary>
        /// Search Events
        /// </summary>
        /// <param name="searchText">A string containing a list of keywords to search for separated by a space character</param>
        /// <param name="queryFlags">One or more of the following flags: DateEvents, IncludePG, IncludeMature, IncludeAdult
        /// from the <see cref="DirFindFlags"/> Enum
        /// 
        /// Multiple flags can be combined by separating the flags with the | (pipe) character</param>
        /// <param name="eventDay">"u" for in-progress and upcoming events, -or- number of days since/until event is scheduled
        /// For example "0" = Today, "1" = tomorrow, "2" = following day, "-1" = yesterday, etc.</param>
        /// <param name="queryStart">Each request is limited to 100 entries
        /// being returned. To get the first group of entries of a request use 0,
        /// from 100-199 use 100, 200-299 use 200, etc.</param>
        /// <param name="category">EventCategory event is listed under.</param>
        /// <returns>UUID of query to correlate results in callback.</returns>
        public UUID StartEventsSearch(string searchText, DirFindFlags queryFlags, string eventDay, uint queryStart, EventCategories category)
        {
            DirFindQueryPacket find = new DirFindQueryPacket();
            find.AgentData.AgentID = Client.Self.AgentID;
            find.AgentData.SessionID = Client.Self.SessionID;

            UUID queryID = UUID.Random();

            find.QueryData.QueryID = queryID;
            find.QueryData.QueryText = Utils.StringToBytes(eventDay + "|" + (int)category + "|" + searchText);
            find.QueryData.QueryFlags = (uint)queryFlags;
            find.QueryData.QueryStart = (int)queryStart;

            Client.Network.SendPacket(find);
            return queryID;
        }

        /// <summary>Requests Event Details</summary>
        /// <param name="eventID">ID of Event returned from the <see cref="StartEventsSearch"/> method</param>
        public void EventInfoRequest(uint eventID)
        {
            EventInfoRequestPacket find = new EventInfoRequestPacket();
            find.AgentData.AgentID = Client.Self.AgentID;
            find.AgentData.SessionID = Client.Self.SessionID;

            find.EventData.EventID = eventID;

            Client.Network.SendPacket(find);
        }
        #endregion

        #region Blocking Functions

        [Obsolete("Use the async StartPeoplSearch method instead")]
        public bool PeopleSearch(DirFindFlags findFlags, string searchText, int queryStart,
            int timeoutMS, out List<AgentSearchData> results)
        {
            AutoResetEvent searchEvent = new AutoResetEvent(false);
            UUID id = UUID.Zero;
            List<AgentSearchData> people = null;

            EventHandler<DirPeopleReplyEventArgs> callback =
                delegate(object sender, DirPeopleReplyEventArgs e)
                {
                    if (id == e.QueryID)
                    {
                        people = e.MatchedPeople;
                        searchEvent.Set();
                    }
                };

            DirPeopleReply += callback;
            
            id = StartPeopleSearch(searchText, queryStart);
            searchEvent.WaitOne(timeoutMS, false);
            DirPeopleReply -= callback;

            results = people;
            return (results != null);
        }

        #endregion Blocking Functions

        #region Packet Handlers

        /// <summary>Process an incoming <see cref="DirClassifiedReplyPacket"/> packet</summary>
        /// <param name="packet">The <see cref="DirClassifiedReplyPacket"/> packet containing the data</param>
        /// <param name="simulator">The simulator the packet originated from</param>
        protected void DirClassifiedReplyHandler(Packet packet, Simulator simulator)
        {
            if (m_DirClassifieds != null)
            {
                DirClassifiedReplyPacket reply = (DirClassifiedReplyPacket)packet;
                List<Classified> classifieds = new List<Classified>();

                foreach (DirClassifiedReplyPacket.QueryRepliesBlock block in reply.QueryReplies)
                {
                    Classified classified = new Classified();

                    classified.CreationDate = Utils.UnixTimeToDateTime(block.CreationDate);
                    classified.ExpirationDate = Utils.UnixTimeToDateTime(block.ExpirationDate);
                    classified.Flags = (ClassifiedFlags)block.ClassifiedFlags;
                    classified.ID = block.ClassifiedID;
                    classified.Name = Utils.BytesToString(block.Name);
                    classified.Price = block.PriceForListing;

                    classifieds.Add(classified);
                }

                OnDirClassifieds(new DirClassifiedsReplyEventArgs(classifieds));                
            }
        }

        /// <summary>Process an incoming <see cref="DirLandReplyPacket"/> packet</summary>
        /// <param name="packet">The <see cref="DirLandReplyPacket"/> packet containing the data</param>
        /// <param name="simulator">The simulator the packet originated from</param>
        protected void DirLandReplyHandler(Packet packet, Simulator simulator)
        {
            if (m_DirLandReply != null)
            {
                List<DirectoryParcel> parcelsForSale = new List<DirectoryParcel>();
                DirLandReplyPacket reply = (DirLandReplyPacket)packet;

                foreach (DirLandReplyPacket.QueryRepliesBlock block in reply.QueryReplies)
                {
                    DirectoryParcel dirParcel = new DirectoryParcel();

                    dirParcel.ActualArea = block.ActualArea;
                    dirParcel.ID = block.ParcelID;
                    dirParcel.Name = Utils.BytesToString(block.Name);
                    dirParcel.SalePrice = block.SalePrice;
                    dirParcel.Auction = block.Auction;
                    dirParcel.ForSale = block.ForSale;

                    parcelsForSale.Add(dirParcel);
                }
                OnDirLand(new DirLandReplyEventArgs(parcelsForSale));                
            }
        }

        /// <summary>Process an incoming <see cref="DirLandReplyMessage"/> event message</summary>
        /// <param name="capsKey">The Unique Capabilities Key</param>
        /// <param name="message">The <see cref="DirLandReplyMessage"/> event message containing the data</param>
        /// <param name="simulator">The simulator the message originated from</param>
        protected void DirLandReplyEventHandler(string capsKey, IMessage message, Simulator simulator)
        {
            if (m_DirLandReply != null)
            {
                List<DirectoryParcel> parcelsForSale = new List<DirectoryParcel>();

                DirLandReplyMessage reply = (DirLandReplyMessage)message;

                foreach (DirLandReplyMessage.QueryReply block in reply.QueryReplies)
                {
                    DirectoryParcel dirParcel = new DirectoryParcel();

                    dirParcel.ActualArea = block.ActualArea;
                    dirParcel.ID = block.ParcelID;
                    dirParcel.Name = block.Name;
                    dirParcel.SalePrice = block.SalePrice;
                    dirParcel.Auction = block.Auction;
                    dirParcel.ForSale = block.ForSale;

                    parcelsForSale.Add(dirParcel);
                }

                OnDirLand(new DirLandReplyEventArgs(parcelsForSale));
            }
        }

        /// <summary>Process an incoming <see cref="DirPeopleReplyPacket"/> packet</summary>
        /// <param name="packet">The <see cref="DirPeopleReplyPacket"/> packet containing the data</param>
        /// <param name="simulator">The simulator the packet originated from</param>
        protected void DirPeopleReplyHandler(Packet packet, Simulator simulator)
        {
            if (m_DirPeople != null)
            {
                DirPeopleReplyPacket peopleReply = packet as DirPeopleReplyPacket;
                List<AgentSearchData> matches = new List<AgentSearchData>(peopleReply.QueryReplies.Length);
                foreach (DirPeopleReplyPacket.QueryRepliesBlock reply in peopleReply.QueryReplies)
                {
                    AgentSearchData searchData = new AgentSearchData();
                    searchData.Online = reply.Online;
                    searchData.FirstName = Utils.BytesToString(reply.FirstName);
                    searchData.LastName = Utils.BytesToString(reply.LastName);
                    searchData.AgentID = reply.AgentID;
                    matches.Add(searchData);
                }

                OnDirPeople(new DirPeopleReplyEventArgs(peopleReply.QueryData.QueryID, matches));
            }
        }

        /// <summary>Process an incoming <see cref="DirGroupsReplyPacket"/> packet</summary>
        /// <param name="packet">The <see cref="DirGroupsReplyPacket"/> packet containing the data</param>
        /// <param name="simulator">The simulator the packet originated from</param>
        protected void DirGroupsReplyHandler(Packet packet, Simulator simulator)
        {
            if (m_DirGroups != null)
            {
                DirGroupsReplyPacket groupsReply = (DirGroupsReplyPacket)packet;
                List<GroupSearchData> matches = new List<GroupSearchData>(groupsReply.QueryReplies.Length);
                foreach (DirGroupsReplyPacket.QueryRepliesBlock reply in groupsReply.QueryReplies)
                {
                    GroupSearchData groupsData = new GroupSearchData();
                    groupsData.GroupID = reply.GroupID;
                    groupsData.GroupName = Utils.BytesToString(reply.GroupName);
                    groupsData.Members = reply.Members;
                    matches.Add(groupsData);
                }

                OnDirGroups(new DirGroupsReplyEventArgs(groupsReply.QueryData.QueryID, matches));
            }
        }

        /// <summary>Process an incoming <see cref="PlacesReplyMessage"/> event message</summary>
        /// <param name="capsKey">The Unique Capabilities Key</param>
        /// <param name="message">The <see cref="PlacesReplyMessage"/> event message containing the data</param>
        /// <param name="simulator">The simulator the message originated from</param>
        protected void PlacesReplyEventHandler(string capsKey, IMessage message, Simulator simulator)
        {
            if (m_Places != null)
            {
                PlacesReplyMessage replyMessage = (PlacesReplyMessage)message;
                List<PlacesSearchData> places = new List<PlacesSearchData>();

                for (int i = 0; i < replyMessage.QueryDataBlocks.Length; i++)
                {
                    PlacesSearchData place = new PlacesSearchData();
                    place.ActualArea = replyMessage.QueryDataBlocks[i].ActualArea;
                    place.BillableArea = replyMessage.QueryDataBlocks[i].BillableArea;
                    place.Desc = replyMessage.QueryDataBlocks[i].Description;
                    place.Dwell = replyMessage.QueryDataBlocks[i].Dwell;
                    place.Flags = (DirectoryManager.PlacesFlags)(byte)replyMessage.QueryDataBlocks[i].Flags;
                    place.GlobalX = replyMessage.QueryDataBlocks[i].GlobalX;
                    place.GlobalY = replyMessage.QueryDataBlocks[i].GlobalY;
                    place.GlobalZ = replyMessage.QueryDataBlocks[i].GlobalZ;
                    place.Name = replyMessage.QueryDataBlocks[i].Name;
                    place.OwnerID = replyMessage.QueryDataBlocks[i].OwnerID;
                    place.Price = replyMessage.QueryDataBlocks[i].Price;
                    place.SimName = replyMessage.QueryDataBlocks[i].SimName;
                    place.SnapshotID = replyMessage.QueryDataBlocks[i].SnapShotID;
                    place.SKU = replyMessage.QueryDataBlocks[i].ProductSku;
                    places.Add(place);
                }

                OnPlaces(new PlacesReplyEventArgs(replyMessage.QueryID, places));
            }
        }

        /// <summary>Process an incoming <see cref="PlacesReplyPacket"/> packet</summary>
        /// <param name="packet">The <see cref="PlacesReplyPacket"/> packet containing the data</param>
        /// <param name="simulator">The simulator the packet originated from</param>
        protected void PlacesReplyHandler(Packet packet, Simulator simulator)
        {
            if (m_Places != null)
            {
                PlacesReplyPacket placesReply = packet as PlacesReplyPacket;
                List<PlacesSearchData> places = new List<PlacesSearchData>();

                foreach (PlacesReplyPacket.QueryDataBlock block in placesReply.QueryData)
                {
                    PlacesSearchData place = new PlacesSearchData();
                    place.OwnerID = block.OwnerID;
                    place.Name = Utils.BytesToString(block.Name);
                    place.Desc = Utils.BytesToString(block.Desc);
                    place.ActualArea = block.ActualArea;
                    place.BillableArea = block.BillableArea;
                    place.Flags = (PlacesFlags)block.Flags;
                    place.GlobalX = block.GlobalX;
                    place.GlobalY = block.GlobalY;
                    place.GlobalZ = block.GlobalZ;
                    place.SimName = Utils.BytesToString(block.SimName);
                    place.SnapshotID = block.SnapshotID;
                    place.Dwell = block.Dwell;
                    place.Price = block.Price;

                    places.Add(place);
                }

                OnPlaces(new PlacesReplyEventArgs(placesReply.TransactionData.TransactionID, places));                
            }
        }

        /// <summary>Process an incoming <see cref="DirEventsReplyPacket"/> packet</summary>
        /// <param name="packet">The <see cref="DirEventsReplyPacket"/> packet containing the data</param>
        /// <param name="simulator">The simulator the packet originated from</param>
        protected void EventsReplyHandler(Packet packet, Simulator simulator)
        {
            if (m_DirEvents != null)
            {
                DirEventsReplyPacket eventsReply = (DirEventsReplyPacket)packet;
                List<EventsSearchData> matches = new List<EventsSearchData>(eventsReply.QueryReplies.Length);

                foreach (DirEventsReplyPacket.QueryRepliesBlock reply in eventsReply.QueryReplies)
                {
                    EventsSearchData eventsData = new EventsSearchData();
                    eventsData.Owner = reply.OwnerID;
                    eventsData.Name = Utils.BytesToString(reply.Name);
                    eventsData.ID = reply.EventID;
                    eventsData.Date = Utils.BytesToString(reply.Date);
                    eventsData.Time = reply.UnixTime;
                    eventsData.Flags = (EventFlags)reply.EventFlags;
                    matches.Add(eventsData);
                }

                OnDirEvents(new DirEventsReplyEventArgs(eventsReply.QueryData.QueryID, matches));
            }
        }

        /// <summary>Process an incoming <see cref="EventInfoReplyPacket"/> packet</summary>
        /// <param name="packet">The <see cref="EventInfoReplyPacket"/> packet containing the data</param>
        /// <param name="simulator">The simulator the packet originated from</param>
        protected void EventInfoReplyHandler(Packet packet, Simulator simulator)
        {
            if (m_EventInfoReply != null)
            {
                EventInfoReplyPacket eventReply = (EventInfoReplyPacket)packet;
                EventInfo evinfo = new EventInfo();
                evinfo.ID = eventReply.EventData.EventID;
                evinfo.Name = Utils.BytesToString(eventReply.EventData.Name);
                evinfo.Desc = Utils.BytesToString(eventReply.EventData.Desc);
                evinfo.Amount = eventReply.EventData.Amount;
                evinfo.Category = (EventCategories)Utils.BytesToUInt(eventReply.EventData.Category);
                evinfo.Cover = eventReply.EventData.Cover;
                evinfo.Creator = (UUID)Utils.BytesToString(eventReply.EventData.Creator);
                evinfo.Date = Utils.BytesToString(eventReply.EventData.Date);
                evinfo.DateUTC = eventReply.EventData.DateUTC;
                evinfo.Duration = eventReply.EventData.Duration;
                evinfo.Flags = (EventFlags)eventReply.EventData.EventFlags;
                evinfo.SimName = Utils.BytesToString(eventReply.EventData.SimName);
                evinfo.GlobalPos = eventReply.EventData.GlobalPos;

                OnEventInfo(new EventInfoReplyEventArgs(evinfo));
            }
        }

        /// <summary>Process an incoming <see cref="DirPlacesReplyPacket"/> packet</summary>
        /// <param name="packet">The <see cref="DirPlacesReplyPacket"/> packet containing the data</param>
        /// <param name="simulator">The simulator the packet originated from</param>
        protected void DirPlacesReplyHandler(Packet packet, Simulator simulator)
        {
            if (m_DirPlaces != null)
            {
                DirPlacesReplyPacket reply = (DirPlacesReplyPacket)packet;
                List<DirectoryParcel> result = new List<DirectoryParcel>();

                for (int i = 0; i < reply.QueryReplies.Length; i++)
                {
                    DirectoryParcel p = new DirectoryParcel();

                    p.ID = reply.QueryReplies[i].ParcelID;
                    p.Name = Utils.BytesToString(reply.QueryReplies[i].Name);
                    p.Dwell = reply.QueryReplies[i].Dwell;
                    p.Auction = reply.QueryReplies[i].Auction;
                    p.ForSale = reply.QueryReplies[i].ForSale;

                    result.Add(p);
                }

                OnDirPlaces(new DirPlacesReplyEventArgs(reply.QueryData[0].QueryID, result));                
            }
        }

        #endregion Packet Handlers
    }

    #region DirectoryManager EventArgs Classes

    /// <summary>Contains the Event data returned from the data server from an EventInfoRequest</summary>
    public class EventInfoReplyEventArgs : EventArgs
    {
        private readonly DirectoryManager.EventInfo m_MatchedEvent;

        /// <summary>
        /// A single EventInfo object containing the details of an event
        /// </summary>
        public DirectoryManager.EventInfo MatchedEvent { get { return m_MatchedEvent; } }

        /// <summary>Construct a new instance of the EventInfoReplyEventArgs class</summary>
        /// <param name="matchedEvent">A single EventInfo object containing the details of an event</param>
        public EventInfoReplyEventArgs(DirectoryManager.EventInfo matchedEvent)
        {
            this.m_MatchedEvent = matchedEvent;
        }        
    }

    /// <summary>Contains the "Event" detail data returned from the data server</summary>
    public class DirEventsReplyEventArgs : EventArgs
    {
        private readonly UUID m_QueryID;
        /// <summary>The ID returned by <see cref="DirectoryManager.StartEventsSearch"/></summary>
        public UUID QueryID { get { return m_QueryID; } }

        private readonly List<DirectoryManager.EventsSearchData> m_matchedEvents;

        /// <summary>A list of "Events" returned by the data server</summary>
        public List<DirectoryManager.EventsSearchData> MatchedEvents { get { return m_matchedEvents; } }

        /// <summary>Construct a new instance of the DirEventsReplyEventArgs class</summary>
        /// <param name="queryID">The ID of the query returned by the data server. 
        /// This will correlate to the ID returned by the <see cref="StartEventsSearch"/> method</param>
        /// <param name="matchedEvents">A list containing the "Events" returned by the search query</param>
        public DirEventsReplyEventArgs(UUID queryID, List<DirectoryManager.EventsSearchData> matchedEvents)
        {
            this.m_QueryID = queryID;
            this.m_matchedEvents = matchedEvents;
        }
    }

    /// <summary>Contains the "Event" list data returned from the data server</summary>
    public class PlacesReplyEventArgs : EventArgs
    {
        private readonly UUID m_QueryID;
        /// <summary>The ID returned by <see cref="DirectoryManager.StartPlacesSearch"/></summary>
        public UUID QueryID { get { return m_QueryID; } }

        private readonly List<DirectoryManager.PlacesSearchData> m_MatchedPlaces;

        /// <summary>A list of "Places" returned by the data server</summary>
        public List<DirectoryManager.PlacesSearchData> MatchedPlaces { get { return m_MatchedPlaces; } }

        /// <summary>Construct a new instance of PlacesReplyEventArgs class</summary>
        /// <param name="queryID">The ID of the query returned by the data server. 
        /// This will correlate to the ID returned by the <see cref="StartPlacesSearch"/> method</param>
        /// <param name="matchedPlaces">A list containing the "Places" returned by the data server query</param>
        public PlacesReplyEventArgs(UUID queryID, List<DirectoryManager.PlacesSearchData> matchedPlaces)
        {
            this.m_QueryID = queryID;
            this.m_MatchedPlaces = matchedPlaces;
        }
    }

    /// <summary>Contains the places data returned from the data server</summary>
    public class DirPlacesReplyEventArgs : EventArgs
    {
        private readonly UUID m_QueryID;
        /// <summary>The ID returned by <see cref="DirectoryManager.StartDirPlacesSearch"/></summary>
        public UUID QueryID { get { return m_QueryID; } }

        private readonly List<DirectoryManager.DirectoryParcel> m_MatchedParcels;

        /// <summary>A list containing Places data returned by the data server</summary>
        public List<DirectoryManager.DirectoryParcel> MatchedParcels { get { return m_MatchedParcels; } }
        
        /// <summary>Construct a new instance of the DirPlacesReplyEventArgs class</summary>
        /// <param name="queryID">The ID of the query returned by the data server. 
        /// This will correlate to the ID returned by the <see cref="StartDirPlacesSearch"/> method</param>
        /// <param name="matchedParcels">A list containing land data returned by the data server</param>
        public DirPlacesReplyEventArgs(UUID queryID, List<DirectoryManager.DirectoryParcel> matchedParcels)
        {
            this.m_QueryID = queryID;
            this.m_MatchedParcels = matchedParcels;
        }
    }

    /// <summary>Contains the classified data returned from the data server</summary>
    public class DirClassifiedsReplyEventArgs : EventArgs
    {
        private readonly List<DirectoryManager.Classified> m_Classifieds;
        /// <summary>A list containing Classified Ads returned by the data server</summary>
        public List<DirectoryManager.Classified> Classifieds { get { return m_Classifieds; } }

        /// <summary>Construct a new instance of the DirClassifiedsReplyEventArgs class</summary>
        /// <param name="classifieds">A list of classified ad data returned from the data server</param>
        public DirClassifiedsReplyEventArgs(List<DirectoryManager.Classified> classifieds)
        {
            this.m_Classifieds = classifieds;
        }
    }

    /// <summary>Contains the group data returned from the data server</summary>
    public class DirGroupsReplyEventArgs : EventArgs
    {
        private readonly UUID m_QueryID;
        /// <summary>The ID returned by <see cref="DirectoryManager.StartGroupSearch"/></summary>
        public UUID QueryID { get { return m_QueryID; } }

        private readonly List<DirectoryManager.GroupSearchData> m_matchedGroups;

        /// <summary>A list containing Groups data returned by the data server</summary>
        public List<DirectoryManager.GroupSearchData> MatchedGroups { get { return m_matchedGroups; } }

        /// <summary>Construct a new instance of the DirGroupsReplyEventArgs class</summary>
        /// <param name="queryID">The ID of the query returned by the data server. 
        /// This will correlate to the ID returned by the <see cref="StartGroupSearch"/> method</param>
        /// <param name="matchedGroups">A list of groups data returned by the data server</param>
        public DirGroupsReplyEventArgs(UUID queryID, List<DirectoryManager.GroupSearchData> matchedGroups)
        {
            this.m_QueryID = queryID;
            this.m_matchedGroups = matchedGroups;
        }
    }

    /// <summary>Contains the people data returned from the data server</summary>
    public class DirPeopleReplyEventArgs : EventArgs
    {
        private readonly UUID m_QueryID;
        /// <summary>The ID returned by <see cref="DirectoryManager.StartPeopleSearch"/></summary>
        public UUID QueryID { get { return m_QueryID; } }

        private readonly List<DirectoryManager.AgentSearchData> m_MatchedPeople;

        /// <summary>A list containing People data returned by the data server</summary>
        public List<DirectoryManager.AgentSearchData> MatchedPeople { get { return m_MatchedPeople; } }

        /// <summary>Construct a new instance of the DirPeopleReplyEventArgs class</summary>
        /// <param name="queryID">The ID of the query returned by the data server. 
        /// This will correlate to the ID returned by the <see cref="StartPeopleSearch"/> method</param>
        /// <param name="matchedPeople">A list of people data returned by the data server</param>
        public DirPeopleReplyEventArgs(UUID queryID, List<DirectoryManager.AgentSearchData> matchedPeople)
        {
            this.m_QueryID = queryID;
            this.m_MatchedPeople = matchedPeople;
        }
    }

    /// <summary>Contains the land sales data returned from the data server</summary>
    public class DirLandReplyEventArgs : EventArgs
    {
        private readonly List<DirectoryManager.DirectoryParcel> m_DirParcels;

        /// <summary>A list containing land forsale data returned by the data server</summary>
        public List<DirectoryManager.DirectoryParcel> DirParcels { get { return m_DirParcels; } }

        /// <summary>Construct a new instance of the DirLandReplyEventArgs class</summary>
        /// <param name="dirParcels">A list of parcels for sale returned by the data server</param>
        public DirLandReplyEventArgs(List<DirectoryManager.DirectoryParcel> dirParcels)
        {
            this.m_DirParcels = dirParcels;
        }
    }
    #endregion
}
