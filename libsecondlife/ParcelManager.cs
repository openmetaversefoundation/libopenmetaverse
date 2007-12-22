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
using System.Timers;
using System.Collections.Generic;
using libsecondlife.Packets;

namespace libsecondlife
{
    #region Structs

    /// <summary>
    /// Some information about a parcel of land
    /// </summary>
    public struct ParcelInfo
    {
        /// <summary></summary>
        public LLUUID ID;
        /// <summary></summary>
        public LLUUID OwnerID;
        /// <summary></summary>
        public string Name;
        /// <summary></summary>
        public string Description;
        /// <summary></summary>
        public int ActualArea;
        /// <summary></summary>
        public int BillableArea;
        /// <summary></summary>
        public bool Mature;
        /// <summary></summary>
        public float GlobalX;
        /// <summary></summary>
        public float GlobalY;
        /// <summary></summary>
        public float GlobalZ;
        /// <summary></summary>
        public string SimName;
        /// <summary></summary>
        public LLUUID SnapshotID;
        /// <summary></summary>
        public float Dwell;
        /// <summary></summary>
        public int SalePrice;
        /// <summary></summary>
        public int AuctionID;
    }

    #endregion Structs

    #region Parcel Class

    /// <summary>
    /// Parcel of land, a portion of virtual real estate in a simulator
    /// </summary>
    public class Parcel
    {
        #region Enums

        /// <summary>
        /// Various parcel properties
        /// </summary>
        [Flags]
        public enum ParcelFlags : uint
        {
            /// <summary>No flags set</summary>
            None = 0,
            /// <summary>Allow avatars to fly (a client-side only restriction)</summary>
            AllowFly = 1 << 0,
            /// <summary>Allow foreign scripts to run</summary>
            AllowOtherScripts = 1 << 1,
            /// <summary>This parcel is for sale</summary>
            ForSale = 1 << 2,
            /// <summary>Allow avatars to create a landmark on this parcel</summary>
            AllowLandmark = 1 << 3,
            /// <summary>Allows all avatars to edit the terrain on this parcel</summary>
            AllowTerraform = 1 << 4,
            /// <summary>Avatars have health and can take damage on this parcel.
            /// If set, avatars can be killed and sent home here</summary>
            AllowDamage = 1 << 5,
            /// <summary>Foreign avatars can create objects here</summary>
            CreateObjects = 1 << 6,
            /// <summary>All objects on this parcel can be purchased</summary>
            ForSaleObjects = 1 << 7,
            /// <summary>Access is restricted to a group</summary>
            UseAccessGroup = 1 << 8,
            /// <summary>Access is restricted to a whitelist</summary>
            UseAccessList = 1 << 9,
            /// <summary>Ban blacklist is enabled</summary>
            UseBanList = 1 << 10,
            /// <summary>Unknown</summary>
            UsePassList = 1 << 11,
            /// <summary>List this parcel in the search directory</summary>
            ShowDirectory = 1 << 12,
            /// <summary>Unknown</summary>
            AllowDeedToGroup = 1 << 13,
            /// <summary>Unknown</summary>
            ContributeWithDeed = 1 << 14,
            /// <summary>Restrict sounds originating on this parcel to the 
            /// parcel boundaries</summary>
            SoundLocal = 1 << 15,
            /// <summary>Objects on this parcel are sold when the land is 
            /// purchsaed</summary>
            SellParcelObjects = 1 << 16,
            /// <summary>Allow this parcel to be published on the web</summary>
            AllowPublish = 1 << 17,
            /// <summary>The information for this parcel is mature content</summary>
            MaturePublish = 1 << 18,
            /// <summary>The media URL is an HTML page</summary>
            UrlWebPage = 1 << 19,
            /// <summary>The media URL is a raw HTML string</summary>
            UrlRawHtml = 1 << 20,
            /// <summary>Restrict foreign object pushes</summary>
            RestrictPushObject = 1 << 21,
            /// <summary>Ban all non identified/transacted avatars</summary>
            DenyAnonymous = 1 << 22,
            /// <summary>Ban all identified avatars</summary>
            DenyIdentified = 1 << 23,
            /// <summary>Ban all transacted avatars</summary>
            DenyTransacted = 1 << 24,
            /// <summary>Allow group-owned scripts to run</summary>
            AllowGroupScripts = 1 << 25,
            /// <summary>Allow object creation by group members or group 
            /// objects</summary>
            CreateGroupObjects = 1 << 26,
            /// <summary>Allow all objects to enter this parcel</summary>
            AllowAllObjectEntry = 1 << 27,
            /// <summary>Only allow group and owner objects to enter this parcel</summary>
            AllowGroupObjectEntry = 1 << 28,
            /// <summary>Voice Enabled on this parcel</summary>
            AllowVoiceChat = 1 << 29,
            /// <summary>Use Estate Voice channel for Voice on this parcel</summary>
            UseEstateVoiceChan = 1 << 30
        }

        /// <summary>
        /// Parcel ownership status
        /// </summary>
        public enum ParcelStatus : sbyte
        {
            /// <summary></summary>
            None = -1,
            /// <summary></summary>
            Leased = 0,
            /// <summary></summary>
            LeasePending = 1,
            /// <summary></summary>
            Abandoned = 2
        }

        /// <summary>
        /// 
        /// </summary>
        public enum ParcelCategory : sbyte
        {
            /// <summary>No assigned category</summary>
            None = 0,
            /// <summary></summary>
            Linden,
            /// <summary></summary>
            Adult,
            /// <summary></summary>
            Arts,
            /// <summary></summary>
            Business,
            /// <summary></summary>
            Educational,
            /// <summary></summary>
            Gaming,
            /// <summary></summary>
            Hangout,
            /// <summary></summary>
            Newcomer,
            /// <summary></summary>
            Park,
            /// <summary></summary>
            Residential,
            /// <summary></summary>
            Shopping,
            /// <summary></summary>
            Stage,
            /// <summary></summary>
            Other,
            /// <summary>Not an actual category, only used for queries</summary>
            Any = -1
        }

        #endregion Enums

        /// <summary></summary>
        public int RequestResult;
        /// <summary></summary>
        public int SequenceID;
        /// <summary></summary>
        public bool SnapSelection;
        /// <summary></summary>
        public int SelfCount;
        /// <summary></summary>
        public int OtherCount;
        /// <summary></summary>
        public int PublicCount;
        /// <summary>Simulator-local ID of this parcel</summary>
        public int LocalID { get { return localid; } }
        /// <summary>UUID of the owner of this parcel</summary>
        public LLUUID OwnerID;
        /// <summary>Whether the land is deeded to a group or not</summary>
        public bool IsGroupOwned;
        /// <summary></summary>
        public uint AuctionID;
        /// <summary>Date land was claimed</summary>
        public DateTime ClaimDate;
        /// <summary>Appears to always be zero</summary>
        public int ClaimPrice;
        /// <summary></summary>
        public int RentPrice;
        /// <summary>Minimum corner of the axis-aligned bounding box for this
        /// parcel</summary>
        public LLVector3 AABBMin;
        /// <summary>Maximum corner of the axis-aligned bounding box for this
        /// parcel</summary>
        public LLVector3 AABBMax;
        /// <summary>Bitmap describing land layout in 4x4m squares across the 
        /// entire region</summary>
        public byte[] Bitmap;
        /// <summary>Total parcel land area</summary>
        public int Area;
        /// <summary></summary>
        public ParcelStatus Status;
        /// <summary>Maximum primitives across the entire simulator</summary>
        public int SimWideMaxPrims;
        /// <summary>Total primitives across the entire simulator</summary>
        public int SimWideTotalPrims;
        /// <summary>Maximum number of primitives this parcel supports</summary>
        public int MaxPrims;
        /// <summary>Total number of primitives on this parcel</summary>
        public int TotalPrims;
        /// <summary>Total number of primitives owned by the parcel owner on 
        /// this parcel</summary>
        public int OwnerPrims;
        /// <summary>Total number of primitives owned by the parcel group on 
        /// this parcel</summary>
        public int GroupPrims;
        /// <summary>Total number of other primitives on this parcel</summary>
        public int OtherPrims;
        /// <summary>Total number of primitives you are currently selecting and
        /// sitting on</summary>
        public int SelectedPrims;
        /// <summary></summary>
        public float ParcelPrimBonus;
        /// <summary>Autoreturn value in minutes for others' objects</summary>
        public int OtherCleanTime;
        /// <summary></summary>
        public ParcelFlags Flags;
        /// <summary>Sale price of the parcel, only useful if ForSale is set</summary>
        /// <remarks>The SalePrice will remain the same after an ownership
        /// transfer (sale), so it can be used to see the purchase price after
        /// a sale if the new owner has not changed it</remarks>
        public int SalePrice;
        /// <summary>Parcel Name</summary>
        public string Name;
        /// <summary>Parcel Description</summary>
        public string Desc;
        /// <summary>URL For Music Stream</summary>
        public string MusicURL;
        /// <summary>URL For other Media</summary>
        public string MediaURL;
        /// <summary>Key to Picture for Media Placeholder</summary>
        public LLUUID MediaID;
        /// <summary></summary>
        public byte MediaAutoScale;
        /// <summary></summary>
        public LLUUID GroupID;
        /// <summary>Price for a temporary pass</summary>
        public int PassPrice;
        /// <summary>How long is pass valid for</summary>
        public float PassHours;
        /// <summary></summary>
        public ParcelCategory Category;
        /// <summary>Key of authorized buyer</summary>
        public LLUUID AuthBuyerID;
        /// <summary>Key of parcel snapshot</summary>
        public LLUUID SnapshotID;
        /// <summary></summary>
        public LLVector3 UserLocation;
        /// <summary></summary>
        public LLVector3 UserLookAt;
        /// <summary></summary>
        public byte LandingType;
        /// <summary></summary>
        public float Dwell;
        /// <summary></summary>
        public bool RegionDenyAnonymous;
        /// <summary></summary>
        public bool RegionDenyIdentified;
        /// <summary></summary>
        public bool RegionDenyTransacted;
        /// <summary></summary>
        public bool RegionPushOverride;
        /// <summary></summary>
        public Simulator Simulator;
        /// <summary>Access list of who is whitelisted or blacklisted on this
        /// parcel</summary>
        public List<ParcelManager.ParcelAccessEntry> AccessList;

        private int localid;


        /// <summary>
        /// Defalt constructor
        /// </summary>
        /// <param name="simulator">Simulator this parcel resides in</param>
        /// <param name="localID">Local ID of this parcel</param>
        public Parcel(Simulator simulator, int localID)
        {
            Simulator = simulator;
            localid = localID;
        }

        /// <summary>
        /// Update the simulator with any local changes to this Parcel object
        /// </summary>
        /// <param name="wantReply">Whether we want the simulator to confirm
        /// the update with a reply packet or not</param>
        public void Update(bool wantReply)
        {
            ParcelPropertiesUpdatePacket request = new ParcelPropertiesUpdatePacket();

            request.AgentData.AgentID = Simulator.Client.Self.AgentID;
            request.AgentData.SessionID = Simulator.Client.Self.SessionID;

            request.ParcelData.LocalID = this.LocalID;

            request.ParcelData.AuthBuyerID = this.AuthBuyerID;
            request.ParcelData.Category = (byte)this.Category;
            request.ParcelData.Desc = Helpers.StringToField(this.Desc);
            request.ParcelData.GroupID = this.GroupID;
            request.ParcelData.LandingType = this.LandingType;
            request.ParcelData.MediaAutoScale = this.MediaAutoScale;
            request.ParcelData.MediaID = this.MediaID;
            request.ParcelData.MediaURL = Helpers.StringToField(this.MediaURL);
            request.ParcelData.MusicURL = Helpers.StringToField(this.MusicURL);
            request.ParcelData.Name = Helpers.StringToField(this.Name);
            if (wantReply) request.ParcelData.Flags = 1;
            request.ParcelData.ParcelFlags = (uint)this.Flags;
            request.ParcelData.PassHours = this.PassHours;
            request.ParcelData.PassPrice = this.PassPrice;
            request.ParcelData.SalePrice = this.SalePrice;
            request.ParcelData.SnapshotID = this.SnapshotID;
            request.ParcelData.UserLocation = this.UserLocation;
            request.ParcelData.UserLookAt = this.UserLookAt;

            Simulator.Client.Network.SendPacket(request, Simulator);

            UpdateOtherCleanTime();
        }

        public void UpdateOtherCleanTime()
        {
            ParcelSetOtherCleanTimePacket request = new ParcelSetOtherCleanTimePacket();
            request.AgentData.AgentID = Simulator.Client.Self.AgentID;
            request.AgentData.SessionID = Simulator.Client.Self.SessionID;
            request.ParcelData.LocalID = this.LocalID;
            request.ParcelData.OtherCleanTime = this.OtherCleanTime;

            Simulator.Client.Network.SendPacket(request, Simulator);
        }
    }

    #endregion Parcel Class

    /// <summary>
    /// Parcel (subdivided simulator lots) subsystem
    /// </summary>
    public class ParcelManager
    {
        #region Enums

        /// <summary>
        /// Type of return to use when returning objects from a parcel
        /// </summary>
        public enum ObjectReturnType : uint
        {
            /// <summary></summary>
            None = 0,
            /// <summary></summary>
            Owner = 1 << 1,
            /// <summary></summary>
            Group = 1 << 2,
            /// <summary></summary>
            Other = 1 << 3,
            /// <summary></summary>
            List = 1 << 4,
            /// <summary></summary>
            Sell = 1 << 5
        }

        /// <summary>
        /// 
        /// </summary>
        public enum ParcelAccessFlags : uint
        {
            /// <summary></summary>
            NoAccess = 0,
            /// <summary></summary>
            Access = 1
        }

        /// <summary>
        /// The result of a request for parcel properties
        /// </summary>
        public enum ParcelResult : int
        {
            /// <summary>No matches were found for the request</summary>
            NoData = -1,
            /// <summary>Request matched a single parcel</summary>
            Single = 0,
            /// <summary>Request matched multiple parcels</summary>
            Multiple = 1
        }

        /// <summary>
        /// Flags used in the ParcelAccessListRequest packet to specify whether
        /// we want the access list (whitelist), ban list (blacklist), or both
        /// </summary>
        [Flags]
        public enum AccessList : uint
        {
            /// <summary>Request the access list</summary>
            Access = 1 << 0,
            /// <summary>Request the ban list</summary>
            Ban = 1 << 1,
            /// <summary>Request both the access list and ban list</summary>
            Both = Access | Ban
        }

        #endregion Enums

        #region Structs

        /// <summary>
        /// 
        /// </summary>
        public struct ParcelAccessEntry
        {
            /// <summary></summary>
            public LLUUID AgentID;
            /// <summary></summary>
            public DateTime Time;
            /// <summary></summary>
            public AccessList Flags;
        }

        public struct ParcelPrimOwners
        {
            public LLUUID OwnerID;
            public bool IsGroupOwned;
            public int Count;
            public bool OnlineStatus;
        }

        #endregion Structs

        #region Delegates

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parcelID">UUID of the requested parcel</param>
        /// <param name="localID">Simulator-local ID of the requested parcel</param>
        /// <param name="dwell">Dwell value of the requested parcel</param>
        public delegate void ParcelDwellCallback(LLUUID parcelID, int localID, float dwell);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parcel"></param>
        public delegate void ParcelInfoCallback(ParcelInfo parcel);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parcel">Full properties for a single parcel. If result
        /// is NoData this will be incomplete or incorrect data</param>
        /// <param name="result">Success of the query</param>
        /// <param name="sequenceID">User-assigned identifier for the query</param>
        /// <param name="snapSelection">User-assigned boolean for the query</param>
        public delegate void ParcelPropertiesCallback(Parcel parcel, ParcelResult result, int sequenceID, bool snapSelection);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sequenceID"></param>
        /// <param name="localID"></param>
        /// <param name="flags"></param>
        /// <param name="accessEntries"></param>
        public delegate void ParcelAccessListReplyCallback(Simulator simulator, int sequenceID, int localID, uint flags, List<ParcelAccessEntry> accessEntries);

        /// <summary>
        /// Responses to a request for prim owners on a parcel.
        /// </summary>
        /// <param name="simulator">simulator parcel is in</param>
        /// <param name="localID">LocalID of parcel.</param>
        /// <param name="primownersEntries">List containing details or prim ownership.</param>
        public delegate void ParcelObjectOwnersListReplyCallback(Simulator simulator,  List<ParcelPrimOwners> primOwners);

        /// <summary>
        /// Fired when all parcels are downloaded from simulator.
        /// </summary>
        /// <param name="simulator">simulator parcel is in</param>
        /// <param name="simParcels">Dictionary containing parcel details in simulator.</param>
        /// <param name="parcelMap">64,64 array containing sim position -> localID mapping.</param>
        public delegate void SimParcelsDownloaded(Simulator simulator, SafeDictionary<int, Parcel> simParcels, int[,] parcelMap);

        #endregion Delegates

        #region Events

        /// <summary></summary>
        public event ParcelDwellCallback OnParcelDwell;
        /// <summary></summary>
        public event ParcelInfoCallback OnParcelInfo;
        /// <summary></summary>
        public event ParcelPropertiesCallback OnParcelProperties;
        /// <summary></summary>
        public event ParcelAccessListReplyCallback OnAccessListReply;
        /// <summary></summary>
        public event ParcelObjectOwnersListReplyCallback OnPrimOwnersListReply;
        /// <summary></summary>
        public event SimParcelsDownloaded OnSimParcelsDownloaded;

        #endregion Events

        private SecondLife Client;

        #region Public Methods

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">A reference to the SecondLife client</param>
        public ParcelManager(SecondLife client)
        {
            Client = client;
            // Setup the callbacks
            Client.Network.RegisterCallback(PacketType.ParcelInfoReply, new NetworkManager.PacketCallback(ParcelInfoReplyHandler));
            Client.Network.RegisterCallback(PacketType.ParcelProperties, new NetworkManager.PacketCallback(ParcelPropertiesHandler));
            Client.Network.RegisterCallback(PacketType.ParcelDwellReply, new NetworkManager.PacketCallback(ParcelDwellReplyHandler));
            Client.Network.RegisterCallback(PacketType.ParcelAccessListReply, new NetworkManager.PacketCallback(ParcelAccessListReplyHandler));
            Client.Network.RegisterCallback(PacketType.ParcelObjectOwnersReply, new NetworkManager.PacketCallback(ParcelObjectOwnersReplyHandler));
        }

        /// <summary>
        /// Request basic information for a single parcel
        /// </summary>
        /// <param name="parcelID">Simulator-local ID of the parcel</param>
        public void InfoRequest(LLUUID parcelID)
        {
            ParcelInfoRequestPacket request = new ParcelInfoRequestPacket();
            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.Data.ParcelID = parcelID;

            Client.Network.SendPacket(request);
        }

        /// <summary>
        /// Request properties of a single parcel
        /// </summary>
        /// <param name="simulator">Simulator containing the parcel</param>
        /// <param name="localID">Simulator-local ID of the parcel</param>
        /// <param name="sequenceID">An arbitrary integer that will be returned
        /// with the ParcelProperties reply, useful for distinguishing between
        /// multiple simultaneous requests</param>
        public void PropertiesRequest(Simulator simulator, int localID, int sequenceID)
        {
            ParcelPropertiesRequestByIDPacket request = new ParcelPropertiesRequestByIDPacket();

            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;

            request.ParcelData.LocalID = localID;
            request.ParcelData.SequenceID = sequenceID;

            Client.Network.SendPacket(request, simulator);
        }

        /// <summary>
        /// Request the access list for a single parcel
        /// </summary>
        /// <param name="simulator">Simulator containing the parcel</param>
        /// <param name="localID">Simulator-local ID of the parcel</param>
        /// <param name="sequenceID">An arbitrary integer that will be returned
        /// with the ParcelAccessList reply, useful for distinguishing between
        /// multiple simultaneous requests</param>
        public void AccessListRequest(Simulator simulator, int localID, AccessList flags, int sequenceID)
        {
            ParcelAccessListRequestPacket request = new ParcelAccessListRequestPacket();

            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.Data.LocalID = localID;
            request.Data.Flags = (uint)flags;
            request.Data.SequenceID = sequenceID;

            Client.Network.SendPacket(request, simulator);
        }

        /// <summary>
        /// Request properties of parcels using a bounding box selection
        /// </summary>
        /// <param name="simulator">Simulator containing the parcel</param>
        /// <param name="north">Northern boundary of the parcel selection</param>
        /// <param name="east">Eastern boundary of the parcel selection</param>
        /// <param name="south">Southern boundary of the parcel selection</param>
        /// <param name="west">Western boundary of the parcel selection</param>
        /// <param name="sequenceID">An arbitrary integer that will be returned
        /// with the ParcelProperties reply, useful for distinguishing between
        /// different types of parcel property requests</param>
        /// <param name="snapSelection">A boolean that is returned with the
        /// ParcelProperties reply, useful for snapping focus to a single
        /// parcel</param>
        public void PropertiesRequest(Simulator simulator, float north, float east, float south, float west,
            int sequenceID, bool snapSelection)
        {
            ParcelPropertiesRequestPacket request = new ParcelPropertiesRequestPacket();

            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.ParcelData.North = north;
            request.ParcelData.East = east;
            request.ParcelData.South = south;
            request.ParcelData.West = west;
            request.ParcelData.SequenceID = sequenceID;
            request.ParcelData.SnapSelection = snapSelection;

            Client.Network.SendPacket(request, simulator);
        }

        /// <summary>
        /// Request the dwell value for a parcel
        /// </summary>
        /// <param name="simulator">Simulator containing the parcel</param>
        /// <param name="localID">Simulator-local ID of the parcel</param>
        public void DwellRequest(Simulator simulator, int localID)
        {
            ParcelDwellRequestPacket request = new ParcelDwellRequestPacket();
            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.Data.LocalID = localID;
            request.Data.ParcelID = LLUUID.Zero; // Not used by clients

            Client.Network.SendPacket(request, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        /// <param name="forGroup"></param>
        /// <param name="groupID"></param>
        /// <param name="removeContribution"></param>
        /// <returns></returns>
        public void Buy(Simulator simulator, int localID, bool forGroup, LLUUID groupID, 
            bool removeContribution, int parcelArea, int parcelPrice)
        {
            ParcelBuyPacket request = new ParcelBuyPacket();

            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;

            request.Data.Final = true;
            request.Data.GroupID = groupID;
            request.Data.LocalID = localID;
            request.Data.IsGroupOwned = forGroup;
            request.Data.RemoveContribution = removeContribution;

            request.ParcelData.Area = parcelArea;
            request.ParcelData.Price = parcelPrice;

            Client.Network.SendPacket(request, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        public void Reclaim(Simulator simulator, int localID)
        {
            ParcelReclaimPacket request = new ParcelReclaimPacket();
            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;

            request.Data.LocalID = localID;

            Client.Network.SendPacket(request, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        /// <param name="groupID"></param>
        public void DeedToGroup(Simulator simulator, int localID, LLUUID groupID)
        {
            ParcelDeedToGroupPacket request = new ParcelDeedToGroupPacket();
            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;

            request.Data.LocalID = localID;
            request.Data.GroupID = groupID;

            Client.Network.SendPacket(request, simulator);
        }

        /// <summary>
        /// Request prim owners of a parcel of land.
        /// </summary>
        /// <param name="simulator">Simulator parcel is in</param>
        /// <param name="localID">local ID # of parcel</param>
        public void ObjectOwnersRequest(Simulator simulator, int localID)
        {
            ParcelObjectOwnersRequestPacket request = new ParcelObjectOwnersRequestPacket();

            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;

            request.ParcelData.LocalID = localID;
            Client.Network.SendPacket(request, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        /// <param name="type"></param>
        /// <param name="ownerIDs"></param>
        public void ReturnObjects(Simulator simulator, int localID, ObjectReturnType type, List<LLUUID> ownerIDs)
        {
            ParcelReturnObjectsPacket request = new ParcelReturnObjectsPacket();
            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;

            request.ParcelData.LocalID = localID;
            request.ParcelData.ReturnType = (uint)type;

            // A single null TaskID is (not) used for parcel object returns
            request.TaskIDs = new ParcelReturnObjectsPacket.TaskIDsBlock[1];
            request.TaskIDs[0] = new ParcelReturnObjectsPacket.TaskIDsBlock();
            request.TaskIDs[0].TaskID = LLUUID.Zero;

            // Convert the list of owner UUIDs to packet blocks if a list is given
            if (ownerIDs != null)
            {
                request.OwnerIDs = new ParcelReturnObjectsPacket.OwnerIDsBlock[ownerIDs.Count];

                for (int i = 0; i < ownerIDs.Count; i++)
                {
                    request.OwnerIDs[i] = new ParcelReturnObjectsPacket.OwnerIDsBlock();
                    request.OwnerIDs[i].OwnerID = ownerIDs[i];
                }
            }
            else
            {
                request.OwnerIDs = new ParcelReturnObjectsPacket.OwnerIDsBlock[0];
            }

            Client.Network.SendPacket(request, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="west"></param>
        /// <param name="south"></param>
        /// <param name="east"></param>
        /// <param name="north"></param>
        public void ParcelSubdivide(Simulator simulator, float west, float south, float east, float north)
        {
            ParcelDividePacket divide = new ParcelDividePacket();
            divide.AgentData.AgentID = Client.Self.AgentID;
            divide.AgentData.SessionID = Client.Self.SessionID;
            divide.ParcelData.East = east;
            divide.ParcelData.North = north;
            divide.ParcelData.South = south;
            divide.ParcelData.West = west;

            Client.Network.SendPacket(divide, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="west"></param>
        /// <param name="south"></param>
        /// <param name="east"></param>
        /// <param name="north"></param>
        public void ParcelJoin(Simulator simulator, float west, float south, float east, float north)
        {
            ParcelJoinPacket join = new ParcelJoinPacket();
            join.AgentData.AgentID = Client.Self.AgentID;
            join.AgentData.SessionID = Client.Self.SessionID;
            join.ParcelData.East = east;
            join.ParcelData.North = north;
            join.ParcelData.South = south;
            join.ParcelData.West = west;

            Client.Network.SendPacket(join, simulator);
        }

           /// <summary>
        /// Request all simulator parcel properties (used for populating the <code>Simulator.Parcels</code> 
        /// dictionary)
        /// </summary>
        /// <param name="simulator">Simulator to request parcels from (must be connected)</param>
        public void RequestAllSimParcels(Simulator simulator)
        {
            System.Threading.Thread th = new System.Threading.Thread(delegate()
            {
                int y, x;
                for (y = 0; y < 64; y++)
                {
                    for (x = 0; x < 64; x++)
                    {
                        if (simulator.ParcelMap[y, x] == 0)
                        {
                            Client.Parcels.PropertiesRequest(simulator,
                                                             (y + 1) * 4.0f, (x + 1) * 4.0f,
                                                             y * 4.0f, x * 4.0f, 0, false);
                            // Pause for 50 ms after every request to avoid flooding the sim
                            System.Threading.Thread.Sleep(50);
                        }
                    }
                }
            });
            th.Start();
        }

        /// <summary>
        /// Gets a parcel LocalID
        /// </summary>
        /// <param name="simulator">Simulator parcel is in</param>
        /// <param name="position">llVector3 position in simulator (Z not used)</param>
        /// <returns>0 on failure, or parcel LocalID on success.</returns>
        /// <remarks>A call to <code>Parcels.RequestAllSimParcels</code> is required to populate map &
        /// dictionary.</remarks>
        public int GetParcelLocalID(Simulator simulator, LLVector3 position)
        {
            return simulator.ParcelMap[(byte)position.Y / 4, (byte)position.X / 4];
        }
        #endregion Public Methods

        #region Packet Handlers

        private void ParcelDwellReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnParcelDwell != null)
            {
                ParcelDwellReplyPacket dwell = (ParcelDwellReplyPacket)packet;

                try { OnParcelDwell(dwell.Data.ParcelID, dwell.Data.LocalID, dwell.Data.Dwell); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        private void ParcelInfoReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnParcelInfo != null)
            {
                ParcelInfoReplyPacket info = (ParcelInfoReplyPacket)packet;

                ParcelInfo parcelInfo = new ParcelInfo();

                parcelInfo.ActualArea = info.Data.ActualArea;
                parcelInfo.AuctionID = info.Data.AuctionID;
                parcelInfo.BillableArea = info.Data.BillableArea;
                parcelInfo.Description = Helpers.FieldToUTF8String(info.Data.Desc);
                parcelInfo.Dwell = info.Data.Dwell;
                parcelInfo.GlobalX = info.Data.GlobalX;
                parcelInfo.GlobalY = info.Data.GlobalY;
                parcelInfo.GlobalZ = info.Data.GlobalZ;
                parcelInfo.ID = info.Data.ParcelID;
                parcelInfo.Mature = ((info.Data.Flags & 1) != 0) ? true : false;
                parcelInfo.Name = Helpers.FieldToUTF8String(info.Data.Name);
                parcelInfo.OwnerID = info.Data.OwnerID;
                parcelInfo.SalePrice = info.Data.SalePrice;
                parcelInfo.SimName = Helpers.FieldToUTF8String(info.Data.SimName);
                parcelInfo.SnapshotID = info.Data.SnapshotID;

                try { OnParcelInfo(parcelInfo); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        private void ParcelPropertiesHandler(Packet packet, Simulator simulator)
        {
            if (OnParcelProperties != null || Client.Settings.PARCEL_TRACKING == true)
            {
                ParcelPropertiesPacket properties = (ParcelPropertiesPacket)packet;

                Parcel parcel = new Parcel(simulator, properties.ParcelData.LocalID);

                parcel.AABBMax = properties.ParcelData.AABBMax;
                parcel.AABBMin = properties.ParcelData.AABBMin;
                parcel.Area = properties.ParcelData.Area;
                parcel.AuctionID = properties.ParcelData.AuctionID;
                parcel.AuthBuyerID = properties.ParcelData.AuthBuyerID;
                parcel.Bitmap = properties.ParcelData.Bitmap;
                parcel.Category = (Parcel.ParcelCategory)(sbyte)properties.ParcelData.Category;
                parcel.ClaimDate = Helpers.UnixTimeToDateTime((uint)properties.ParcelData.ClaimDate);
                // ClaimPrice seems to always be zero?
                parcel.ClaimPrice = properties.ParcelData.ClaimPrice;
                parcel.Desc = Helpers.FieldToUTF8String(properties.ParcelData.Desc);
                parcel.GroupID = properties.ParcelData.GroupID;
                parcel.GroupPrims = properties.ParcelData.GroupPrims;
                parcel.IsGroupOwned = properties.ParcelData.IsGroupOwned;
                parcel.LandingType = properties.ParcelData.LandingType;
                parcel.MaxPrims = properties.ParcelData.MaxPrims;
                parcel.MediaAutoScale = properties.ParcelData.MediaAutoScale;
                parcel.MediaID = properties.ParcelData.MediaID;
                parcel.MediaURL = Helpers.FieldToUTF8String(properties.ParcelData.MediaURL);
                parcel.MusicURL = Helpers.FieldToUTF8String(properties.ParcelData.MusicURL);
                parcel.Name = Helpers.FieldToUTF8String(properties.ParcelData.Name);
                parcel.OtherCleanTime = properties.ParcelData.OtherCleanTime;
                parcel.OtherCount = properties.ParcelData.OtherCount;
                parcel.OtherPrims = properties.ParcelData.OtherPrims;
                parcel.OwnerID = properties.ParcelData.OwnerID;
                parcel.OwnerPrims = properties.ParcelData.OwnerPrims;
                parcel.Flags = (Parcel.ParcelFlags)properties.ParcelData.ParcelFlags;
                parcel.ParcelPrimBonus = properties.ParcelData.ParcelPrimBonus;
                parcel.PassHours = properties.ParcelData.PassHours;
                parcel.PassPrice = properties.ParcelData.PassPrice;
                parcel.PublicCount = properties.ParcelData.PublicCount;
                parcel.RegionDenyAnonymous = properties.ParcelData.RegionDenyAnonymous;
                parcel.RegionDenyIdentified = properties.ParcelData.RegionDenyIdentified;
                parcel.RegionDenyTransacted = properties.ParcelData.RegionDenyTransacted;
                parcel.RegionPushOverride = properties.ParcelData.RegionPushOverride;
                parcel.RentPrice = properties.ParcelData.RentPrice;
                parcel.SalePrice = properties.ParcelData.SalePrice;
                parcel.SelectedPrims = properties.ParcelData.SelectedPrims;
                parcel.SelfCount = properties.ParcelData.SelfCount;
                parcel.SimWideMaxPrims = properties.ParcelData.SimWideMaxPrims;
                parcel.SimWideTotalPrims = properties.ParcelData.SimWideTotalPrims;
                parcel.SnapshotID = properties.ParcelData.SnapshotID;
                parcel.Status = (Parcel.ParcelStatus)(sbyte)properties.ParcelData.Status;
                parcel.TotalPrims = properties.ParcelData.TotalPrims;
                parcel.UserLocation = properties.ParcelData.UserLocation;
                parcel.UserLookAt = properties.ParcelData.UserLookAt;
                // store parcel in dictionary
                if (Client.Settings.PARCEL_TRACKING)
                {
                    lock (simulator.Parcels.Dictionary)
                        simulator.Parcels.Dictionary[parcel.LocalID] = parcel;

                    int y, x, index, bit;
                    for (y = 0; y < simulator.ParcelMap.GetLength(0); y++)
                    {
                        for (x = 0; x < simulator.ParcelMap.GetLength(1); x++)
                        {
                            if (simulator.ParcelMap[y, x] == 0)
                            {
                                index = (y * 64) + x;
                                bit = index % 8;
                                index >>= 3;

                                if ((parcel.Bitmap[index] & (1 << bit)) != 0)
                                    simulator.ParcelMap[y, x] = parcel.LocalID;
                            }
                        }

                    }
                }

                  // Fire the callback for parcel properties being received
                if (OnParcelProperties != null)
                {
                    try
                    {
                        OnParcelProperties(parcel, (ParcelResult)properties.ParcelData.RequestResult,
                            properties.ParcelData.SequenceID, properties.ParcelData.SnapSelection);
                    }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }

                // Check if all of the simulator parcels have been retrieved, if so fire another callback
                if (OnSimParcelsDownloaded != null && simulator.IsParcelMapFull())
                {
                    try { OnSimParcelsDownloaded(simulator, simulator.Parcels, simulator.ParcelMap); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
            }
        }

        protected void ParcelAccessListReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnAccessListReply != null)
            {
                ParcelAccessListReplyPacket reply = (ParcelAccessListReplyPacket)packet;
                List<ParcelAccessEntry> accessList = new List<ParcelAccessEntry>(reply.List.Length);

                for (int i = 0; i < reply.List.Length; i++)
                {
                    ParcelAccessEntry pae = new ParcelAccessEntry();
                    pae.AgentID = reply.List[i].ID;
                    pae.Flags = (AccessList)reply.List[i].Flags;
                    pae.Time = Helpers.UnixTimeToDateTime((uint)reply.List[i].Time);

                    accessList.Add(pae);
                }

                try { OnAccessListReply(simulator, reply.Data.SequenceID, reply.Data.LocalID, reply.Data.Flags, 
                    accessList); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        private void ParcelObjectOwnersReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnPrimOwnersListReply != null)
            {
                ParcelObjectOwnersReplyPacket reply = (ParcelObjectOwnersReplyPacket)packet;
                List<ParcelPrimOwners> primOwners = new List<ParcelPrimOwners>();
                
                for (int i = 0; i < reply.Data.Length; i++)
                {
                    ParcelPrimOwners poe = new ParcelPrimOwners();
                    
                    poe.OwnerID = reply.Data[i].OwnerID;
                    poe.IsGroupOwned = reply.Data[i].IsGroupOwned;
                    poe.Count = reply.Data[i].Count;
                    poe.OnlineStatus = reply.Data[i].OnlineStatus;
                    primOwners.Add(poe);
                }
                try { OnPrimOwnersListReply(simulator, primOwners); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }

        }

        #endregion Packet Handlers
    }
}
