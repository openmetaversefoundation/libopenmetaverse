/*
 * Copyright (c) 2006-2008, openmetaverse.org
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
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Http;

namespace OpenMetaverse
{
    #region Enums

    /// <summary>
    /// Type of return to use when returning objects from a parcel
    /// </summary>
    public enum ObjectReturnType : uint
    {
        /// <summary></summary>
        None = 0,
        /// <summary>Return objects owned by parcel owner</summary>
        Owner = 1 << 1,
        /// <summary>Return objects set to group</summary>
        Group = 1 << 2,
        /// <summary>Return objects not owned by parcel owner or set to group</summary>
        Other = 1 << 3,
        /// <summary>Return a specific list of objects on parcel</summary>
        List = 1 << 4,
        /// <summary>Return objects that are marked for-sale</summary>
        Sell = 1 << 5
    }

    /// <summary>
    /// Blacklist/Whitelist flags used in parcels Access List
    /// </summary>
    public enum ParcelAccessFlags : uint
    {
        /// <summary>Agent is denied access</summary>
        NoAccess = 0,
        /// <summary>Agent is granted access</summary>
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
        /// <summary>Request both White and Black lists</summary>
        Both = Access | Ban
    }

    /// <summary>
    /// Sequence ID in ParcelPropertiesReply packets (sent when avatar
    /// tries to cross a parcel border)
    /// </summary>
    public enum ParcelPropertiesStatus : int
    {
        /// <summary>Parcel is currently selected</summary>
        ParcelSelected = -10000,
        /// <summary>Parcel restricted to a group the avatar is not a
        /// member of</summary>
        CollisionNotInGroup = -20000,
        /// <summary>Avatar is banned from the parcel</summary>
        CollisionBanned = -30000,
        /// <summary>Parcel is restricted to an access list that the
        /// avatar is not on</summary>
        CollisionNotOnAccessList = -40000,
        /// <summary>Response to hovering over a parcel</summary>
        HoveredOverParcel = -50000
    }

    /// <summary>
    /// 
    /// </summary>
    public enum TerraformAction : byte
    {
        /// <summary></summary>
        Level = 0,
        /// <summary></summary>
        Raise = 1,
        /// <summary></summary>
        Lower = 2,
        /// <summary></summary>
        Smooth = 3,
        /// <summary></summary>
        Noise = 4,
        /// <summary></summary>
        Revert = 5
    }

    /// <summary>
    /// 
    /// </summary>
    public enum TerraformBrushSize : byte
    {
        /// <summary></summary>
        Small = 1,
        /// <summary></summary>
        Medium = 2,
        /// <summary></summary>
        Large = 4
    }

    /// <summary>
    /// Reasons agent is denied access to a parcel on the simulator
    /// </summary>
    public enum AccessDeniedReason : byte
    {
        /// <summary>Agent is not denied, access is granted</summary>
        NotDenied = 0,
        /// <summary>Agent is not a member of the group set for the parcel, or which owns the parcel</summary>
        NotInGroup = 1,
        /// <summary>Agent is not on the parcels specific allow list</summary>
        NotOnAllowList = 2,
        /// <summary>Agent is on the parcels ban list</summary>
        BannedFromParcel = 3,
        /// <summary>Unknown</summary>
        NoAccess = 4,
        /// <summary>Agent is not age verified and parcel settings deny access to non age verified avatars</summary>
        NotAgeVerified = 5
    }

    /// <summary>
    /// Parcel overlay type. This is used primarily for highlighting and
    /// coloring which is why it is a single integer instead of a set of
    /// flags
    /// </summary>
    /// <remarks>These values seem to be poorly thought out. The first three
    /// bits represent a single value, not flags. For example Auction (0x05) is
    /// not a combination of OwnedByOther (0x01) and ForSale(0x04). However,
    /// the BorderWest and BorderSouth values are bit flags that get attached
    /// to the value stored in the first three bits. Bits four, five, and six
    /// are unused</remarks>
    public enum ParcelOverlayType : byte
    {
        /// <summary>Public land</summary>
        Public = 0,
        /// <summary>Land is owned by another avatar</summary>
        OwnedByOther = 1,
        /// <summary>Land is owned by a group</summary>
        OwnedByGroup = 2,
        /// <summary>Land is owned by the current avatar</summary>
        OwnedBySelf = 3,
        /// <summary>Land is for sale</summary>
        ForSale = 4,
        /// <summary>Land is being auctioned</summary>
        Auction = 5,
        /// <summary>To the west of this area is a parcel border</summary>
        BorderWest = 64,
        /// <summary>To the south of this area is a parcel border</summary>
        BorderSouth = 128
    }

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
        /// <summary>Allow personally owned parcels to be deeded to group</summary>
        AllowDeedToGroup = 1 << 13,
        /// <summary>If Deeded, owner contributes required tier to group parcel is deeded to</summary>
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
        // <summary>Ban all identified avatars [OBSOLETE]</summary>
        //[Obsolete]
        //DenyIdentified = 1 << 23,
        // <summary>Ban all transacted avatars [OBSOLETE]</summary>
        //[Obsolete]
        //DenyTransacted = 1 << 24,
        /// <summary>Allow group-owned scripts to run</summary>
        AllowGroupScripts = 1 << 25,
        /// <summary>Allow object creation by group members or group 
        /// objects</summary>
        CreateGroupObjects = 1 << 26,
        /// <summary>Allow all objects to enter this parcel</summary>
        AllowAPrimitiveEntry = 1 << 27,
        /// <summary>Only allow group and owner objects to enter this parcel</summary>
        AllowGroupObjectEntry = 1 << 28,
        /// <summary>Voice Enabled on this parcel</summary>
        AllowVoiceChat = 1 << 29,
        /// <summary>Use Estate Voice channel for Voice on this parcel</summary>
        UseEstateVoiceChan = 1 << 30,
        /// <summary>Deny Age Unverified Users</summary>
        DenyAgeUnverified = 1U << 31
    }

    /// <summary>
    /// Parcel ownership status
    /// </summary>
    public enum ParcelStatus : sbyte
    {
        /// <summary>Placeholder</summary>
        None = -1,
        /// <summary>Parcel is leased (owned) by an avatar or group</summary>
        Leased = 0,
        /// <summary>Parcel is in process of being leased (purchased) by an avatar or group</summary>
        LeasePending = 1,
        /// <summary>Parcel has been abandoned back to Governor Linden</summary>
        Abandoned = 2
    }

    /// <summary>
    /// Category parcel is listed in under search
    /// </summary>
    public enum ParcelCategory : sbyte
    {
        /// <summary>No assigned category</summary>
        None = 0,
        /// <summary>Linden Infohub or public area</summary>
        Linden,
        /// <summary>Adult themed area</summary>
        Adult,
        /// <summary>Arts and Culture</summary>
        Arts,
        /// <summary>Business</summary>
        Business,
        /// <summary>Educational</summary>
        Educational,
        /// <summary>Gaming</summary>
        Gaming,
        /// <summary>Hangout or Club</summary>
        Hangout,
        /// <summary>Newcomer friendly</summary>
        Newcomer,
        /// <summary>Parks and Nature</summary>
        Park,
        /// <summary>Residential</summary>
        Residential,
        /// <summary>Shopping</summary>
        Shopping,
        /// <summary>Not Used?</summary>
        Stage,
        /// <summary>Other</summary>
        Other,
        /// <summary>Not an actual category, only used for queries</summary>
        Any = -1
    }

    /// <summary>
    /// Type of teleport landing for a parcel
    /// </summary>
    public enum LandingType : byte
    {
        /// <summary>Unset, simulator default</summary>
        None = 0,
        /// <summary>Specific landing point set for this parcel</summary>
        LandingPoint = 1,
        /// <summary>No landing point set, direct teleports enabled for
        /// this parcel</summary>
        Direct = 2
    }

    #endregion Enums

    #region Structs

    /// <summary>
    /// Some information about a parcel of land returned from a DirectoryManager search
    /// </summary>
    public struct ParcelInfo
    {
        /// <summary>Global Key of record</summary>
        public UUID ID;
        /// <summary>Parcel Owners <seealso cref="UUID"/></summary>
        public UUID OwnerID;
        /// <summary>Name field of parcel, limited to 128 characters</summary>
        public string Name;
        /// <summary>Description field of parcel, limited to 256 characters</summary>
        public string Description;
        /// <summary>Total Square meters of parcel</summary>
        public int ActualArea;
        /// <summary>Total area billable as Tier, for group owned land this will be 10% less than ActualArea</summary>
        public int BillableArea;
        /// <summary>True of parcel is in Mature simulator</summary>
        public bool Mature;
        /// <summary>Grid global X position of parcel</summary>
        public float GlobalX;
        /// <summary>Grid global Y position of parcel</summary>
        public float GlobalY;
        /// <summary>Grid global Z position of parcel (not used)</summary>
        public float GlobalZ;
        /// <summary>Name of simulator parcel is located in</summary>
        public string SimName;
        /// <summary>Texture <seealso cref="T:OpenMetaverse.UUID"/> of parcels display picture</summary>
        public UUID SnapshotID;
        /// <summary>Float representing calculated traffic based on time spent on parcel by avatars</summary>
        public float Dwell;
        /// <summary>Sale price of parcel (not used)</summary>
        public int SalePrice;
        /// <summary>Auction ID of parcel</summary>
        public int AuctionID;
    }

    /// <summary>
    /// Parcel Media Information
    /// </summary>
    public struct ParcelMedia
    {
        /// <summary>A byte, if 0x1 viewer should auto scale media to fit object</summary>
        public byte MediaAutoScale;
        /// <summary>A boolean, if true the viewer should loop the media</summary>
        public bool MediaLoop;
        /// <summary>The Asset UUID of the Texture which when applied to a 
        /// primitive will display the media</summary>
        public UUID MediaID;
        /// <summary>A URL which points to any Quicktime supported media type</summary>
        public string MediaURL;
        /// <summary>A description of the media</summary>
        public string MediaDesc;
        /// <summary>An Integer which represents the height of the media</summary>
        public int MediaHeight;
        /// <summary>An integer which represents the width of the media</summary>
        public int MediaWidth;
        /// <summary>A string which contains the mime type of the media</summary>
        public string MediaType;
    }

    #endregion Structs

    #region Parcel Class

    /// <summary>
    /// Parcel of land, a portion of virtual real estate in a simulator
    /// </summary>
    public struct Parcel
    {
        /// <summary></summary>
        public int SelfCount;
        /// <summary></summary>
        public int OtherCount;
        /// <summary></summary>
        public int PublicCount;
        /// <summary>Simulator-local ID of this parcel</summary>
        public int LocalID;
        /// <summary>UUID of the owner of this parcel</summary>
        public UUID OwnerID;
        /// <summary>Whether the land is deeded to a group or not</summary>
        public bool IsGroupOwned;
        /// <summary></summary>
        public uint AuctionID;
        /// <summary>Date land was claimed</summary>
        public DateTime ClaimDate;
        /// <summary>Appears to always be zero</summary>
        public int ClaimPrice;
        /// <summary>This field is no longer used</summary>
        public int RentPrice;
        /// <summary>Minimum corner of the axis-aligned bounding box for this
        /// parcel</summary>
        public Vector3 AABBMin;
        /// <summary>Maximum corner of the axis-aligned bounding box for this
        /// parcel</summary>
        public Vector3 AABBMax;
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
        /// <summary></summary>
        public UUID GroupID;
        /// <summary>Price for a temporary pass</summary>
        public int PassPrice;
        /// <summary>How long is pass valid for</summary>
        public float PassHours;
        /// <summary></summary>
        public ParcelCategory Category;
        /// <summary>Key of authorized buyer</summary>
        public UUID AuthBuyerID;
        /// <summary>Key of parcel snapshot</summary>
        public UUID SnapshotID;
        /// <summary></summary>
        public Vector3 UserLocation;
        /// <summary></summary>
        public Vector3 UserLookAt;
        /// <summary></summary>
        public LandingType Landing;
        /// <summary></summary>
        public float Dwell;
        /// <summary></summary>
        public bool RegionDenyAnonymous;
        /// <summary></summary>
        public bool RegionPushOverride;
        /// <summary>Access list of who is whitelisted on this
        /// parcel</summary>
        public List<ParcelManager.ParcelAccessEntry> AccessWhiteList;
        /// <summary>Access list of who is blacklisted on this
        /// parcel</summary>
        public List<ParcelManager.ParcelAccessEntry> AccessBlackList;
        /// <summary>TRUE of region denies access to age unverified users</summary>
        public bool RegionDenyAgeUnverified;
        /// <summary>true to obscure (hide) media url</summary>
        public bool ObscureMedia;
        /// <summary>true to obscure (hide) music url</summary>
        public bool ObscureMusic;
        /// <summary>A struct containing media details</summary>
        public ParcelMedia Media;

        /// <summary>
        /// Displays a parcel object in string format
        /// </summary>
        /// <returns>string containing key=value pairs of a parcel object</returns>
        public override string ToString()
        {
            string result = "";
            Type parcelType = this.GetType();
            FieldInfo[] fields = parcelType.GetFields();
            foreach (FieldInfo field in fields)
            {
                result += (field.Name + " = " + field.GetValue(this) + " ");
            }
            return result;
        }
        /// <summary>
        /// Defalt constructor
        /// </summary>
        /// <param name="localID">Local ID of this parcel</param>
        public Parcel(int localID)
        {
            LocalID = localID;
            SelfCount = 0;
            OtherCount = 0;
            PublicCount = 0;
            OwnerID = UUID.Zero;
            IsGroupOwned = false;
            AuctionID = 0;
            ClaimDate = Utils.Epoch;
            ClaimPrice = 0;
            RentPrice = 0;
            AABBMin = Vector3.Zero;
            AABBMax = Vector3.Zero;
            Bitmap = Utils.EmptyBytes;
            Area = 0;
            Status = ParcelStatus.None;
            SimWideMaxPrims = 0;
            SimWideTotalPrims = 0;
            MaxPrims = 0;
            TotalPrims = 0;
            OwnerPrims = 0;
            GroupPrims = 0;
            OtherPrims = 0;
            ParcelPrimBonus = 0;
            OtherCleanTime = 0;
            Flags = ParcelFlags.None;
            SalePrice = 0;
            Name = String.Empty;
            Desc = String.Empty;
            MusicURL = String.Empty;
            GroupID = UUID.Zero;
            PassPrice = 0;
            PassHours = 0;
            Category = ParcelCategory.None;
            AuthBuyerID = UUID.Zero;
            SnapshotID = UUID.Zero;
            UserLocation = Vector3.Zero;
            UserLookAt = Vector3.Zero;
            Landing = LandingType.None;
            Dwell = 0;
            RegionDenyAnonymous = false;
            RegionPushOverride = false;
            AccessWhiteList = new List<ParcelManager.ParcelAccessEntry>();
            AccessBlackList = new List<ParcelManager.ParcelAccessEntry>(0);
            RegionDenyAgeUnverified = false;
            Media = new ParcelMedia();
            ObscureMedia = false;
            ObscureMusic = false;
        }

        /// <summary>
        /// Update the simulator with any local changes to this Parcel object
        /// </summary>
        /// <param name="simulator">Simulator to send updates to</param>
        /// <param name="wantReply">Whether we want the simulator to confirm
        /// the update with a reply packet or not</param>
        public void Update(Simulator simulator, bool wantReply)
        {
            Uri url = simulator.Caps.CapabilityURI("ParcelPropertiesUpdate");

            if (url != null)
            {
                OSDMap body = new OSDMap();
                body["auth_buyer_id"] =  OSD.FromUUID(this.AuthBuyerID);
                body["auto_scale"] =  OSD.FromInteger(this.Media.MediaAutoScale);
                body["category"] = OSD.FromInteger((byte)this.Category);
                body["description"] = OSD.FromString(this.Desc);
                body["flags"] =  OSD.FromBinary(Utils.EmptyBytes);
                body["group_id"] = OSD.FromUUID(this.GroupID);
                body["landing_type"] = OSD.FromInteger((byte)this.Landing);
                body["local_id"] = OSD.FromInteger(this.LocalID);
                body["media_desc"] = OSD.FromString(this.Media.MediaDesc);
                body["media_height"] = OSD.FromInteger(this.Media.MediaHeight);
                body["media_id"] = OSD.FromUUID(this.Media.MediaID);
                body["media_loop"] = OSD.FromInteger(this.Media.MediaLoop ? 1 : 0);
                body["media_type"] = OSD.FromString(this.Media.MediaType);
                body["media_url"] = OSD.FromString(this.Media.MediaURL);
                body["media_width"] = OSD.FromInteger(this.Media.MediaWidth);
                body["music_url"] = OSD.FromString(this.MusicURL);
                body["name"] = OSD.FromString(this.Name);
                body["obscure_media"]= OSD.FromInteger(this.ObscureMedia ? 1 : 0);
                body["obscure_music"] = OSD.FromInteger(this.ObscureMusic ? 1 : 0);

                byte[] flags = Utils.IntToBytes((int)this.Flags); ;
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(flags);
                body["parcel_flags"] = OSD.FromBinary(flags);

                body["pass_hours"] = OSD.FromReal(this.PassHours);
                body["pass_price"] = OSD.FromInteger(this.PassPrice);
                body["sale_price"] = OSD.FromInteger(this.SalePrice);
                body["snapshot_id"] = OSD.FromUUID(this.SnapshotID);
                OSDArray uloc = new OSDArray();
                uloc.Add(OSD.FromReal(this.UserLocation.X));
                uloc.Add(OSD.FromReal(this.UserLocation.Y));
                uloc.Add(OSD.FromReal(this.UserLocation.Z));
                body["user_location"] = uloc;
                OSDArray ulat = new OSDArray();
                ulat.Add(OSD.FromReal(this.UserLocation.X));
                ulat.Add(OSD.FromReal(this.UserLocation.Y));
                ulat.Add(OSD.FromReal(this.UserLocation.Z));
                body["user_look_at"] = ulat;

                //Console.WriteLine("OSD REQUEST\n{0}", body.ToString());

                byte[] postData = StructuredData.OSDParser.SerializeLLSDXmlBytes(body);
                //Console.WriteLine("{0}", OSDParser.SerializeLLSDXmlString(body));
                CapsClient capsPost = new CapsClient(url);
                capsPost.StartRequest(postData);

            }
            else
            {

                ParcelPropertiesUpdatePacket request = new ParcelPropertiesUpdatePacket();

                request.AgentData.AgentID = simulator.Client.Self.AgentID;
                request.AgentData.SessionID = simulator.Client.Self.SessionID;

                request.ParcelData.LocalID = this.LocalID;

                request.ParcelData.AuthBuyerID = this.AuthBuyerID;
                request.ParcelData.Category = (byte)this.Category;
                request.ParcelData.Desc = Utils.StringToBytes(this.Desc);
                request.ParcelData.GroupID = this.GroupID;
                request.ParcelData.LandingType = (byte)this.Landing;
                request.ParcelData.MediaAutoScale = this.Media.MediaAutoScale;
                request.ParcelData.MediaID = this.Media.MediaID;
                request.ParcelData.MediaURL = Utils.StringToBytes(this.Media.MediaURL);
                request.ParcelData.MusicURL = Utils.StringToBytes(this.MusicURL);
                request.ParcelData.Name = Utils.StringToBytes(this.Name);
                if (wantReply) request.ParcelData.Flags = 1;
                request.ParcelData.ParcelFlags = (uint)this.Flags;
                request.ParcelData.PassHours = this.PassHours;
                request.ParcelData.PassPrice = this.PassPrice;
                request.ParcelData.SalePrice = this.SalePrice;
                request.ParcelData.SnapshotID = this.SnapshotID;
                request.ParcelData.UserLocation = this.UserLocation;
                request.ParcelData.UserLookAt = this.UserLookAt;

                simulator.SendPacket(request, true);
            }

            UpdateOtherCleanTime(simulator);
            
        }

        /// <summary>
        /// Set Autoreturn time
        /// </summary>
        /// <param name="simulator">Simulator to send the update to</param>
        public void UpdateOtherCleanTime(Simulator simulator)
        {
            ParcelSetOtherCleanTimePacket request = new ParcelSetOtherCleanTimePacket();
            request.AgentData.AgentID = simulator.Client.Self.AgentID;
            request.AgentData.SessionID = simulator.Client.Self.SessionID;
            request.ParcelData.LocalID = this.LocalID;
            request.ParcelData.OtherCleanTime = this.OtherCleanTime;

            simulator.SendPacket(request, true);
        }
    }

    #endregion Parcel Class

    /// <summary>
    /// Parcel (subdivided simulator lots) subsystem
    /// </summary>
    public class ParcelManager
    {
        #region Structs

        /// <summary>
        /// Parcel Accesslist
        /// </summary>
        public struct ParcelAccessEntry
        {
            /// <summary>Agents <seealso cref="T:OpenMetaverse.UUID"/></summary>
            public UUID AgentID;
            /// <summary></summary>
            public DateTime Time;
            /// <summary>Flags for specific entry in white/black lists</summary>
            public AccessList Flags;
        }

        /// <summary>
        /// Owners of primitives on parcel
        /// </summary>
        public struct ParcelPrimOwners
        {
            /// <summary>Prim Owners <seealso cref="T:OpenMetaverse.UUID"/></summary>
            public UUID OwnerID;
            /// <summary>True of owner is group</summary>
            public bool IsGroupOwned;
            /// <summary>Total count of prims owned by OwnerID</summary>
            public int Count;
            /// <summary>true of OwnerID is currently online and is not a group</summary>
            public bool OnlineStatus;
            /// <summary>The date of the most recent prim left by OwnerID</summary>
            public DateTime NewestPrim;
        }

        

        #endregion Structs

        #region Delegates

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parcelID">UUID of the requested parcel</param>
        /// <param name="localID">Simulator-local ID of the requested parcel</param>
        /// <param name="dwell">Dwell value of the requested parcel</param>
        public delegate void ParcelDwellCallback(UUID parcelID, int localID, float dwell);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parcel"></param>
        public delegate void ParcelInfoCallback(ParcelInfo parcel);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator">Simulator the parcel is in</param>
        /// <param name="parcel">Full properties for a single parcel. If result
        /// is NoData this will be incomplete or incorrect data</param>
        /// <param name="result">Success of the query</param>
        /// <param name="selectedPrims">Number of primitives your avatar is currently
        /// selecting and sitting on in this parcel</param>
        /// <param name="sequenceID">User-assigned identifier for the query</param>
        /// <param name="snapSelection">User-assigned boolean for the query</param>
        public delegate void ParcelPropertiesCallback(Simulator simulator, Parcel parcel, ParcelResult result, int selectedPrims,
            int sequenceID, bool snapSelection);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator">Simulator the parcel is in</param>
        /// <param name="sequenceID"></param>
        /// <param name="localID"></param>
        /// <param name="flags"></param>
        /// <param name="accessEntries"></param>
        public delegate void ParcelAccessListReplyCallback(Simulator simulator, int sequenceID, int localID, uint flags,
            List<ParcelAccessEntry> accessEntries);

        /// <summary>
        /// Responses to a request for prim owners on a parcel
        /// </summary>
        /// <param name="simulator">simulator parcel is in</param>
        /// <param name="primOwners">List containing details or prim ownership</param>
        public delegate void ParcelObjectOwnersListReplyCallback(Simulator simulator, List<ParcelPrimOwners> primOwners);

        /// <summary>
        /// Fired when all parcels are downloaded from simulator
        /// </summary>
        /// <param name="simulator">Simulator the parcel is in</param>
        /// <param name="simParcels">Read-only dictionary containing parcel details for the simulator</param>
        /// <param name="parcelMap">64,64 array containing sim position to localID mapping</param>
        public delegate void SimParcelsDownloaded(Simulator simulator, InternalDictionary<int, Parcel> simParcels, int[,] parcelMap);

        /// <summary>
        /// Fired in response to SelectParcelObjects
        /// </summary>
        /// <param name="simulator">simulator the objects are in</param>
        /// <param name="objectIDs">Local IDs of the selected objects</param>
        /// <param name="resetList">If true, list is start of a new selection</param>
        public delegate void ForceSelectObjects(Simulator simulator, List<uint> objectIDs, bool resetList);

        /// <summary>
        /// Fired when a ParcelMediaUpdate packet is received, this occurs when the media on the parcel an avatar
        /// is over changes
        /// </summary>
        /// <param name="simulator">A reference to the simulator object</param>
        /// <param name="media">A struct containing updated media information</param>
        public delegate void ParcelMediaUpdateReplyCallback(Simulator simulator, ParcelMedia media);

        #endregion Delegates

        #region Events
        
        /// <summary>Fired when a <seealso cref="Packets.ParcelDwellReplyPacket"/> is received,
        /// in response to a <seealso cref="DwellRequest"/></summary>
        public event ParcelDwellCallback OnParcelDwell;
        /// <summary>Fired when a <seealso cref="Packets.ParcelInfoReplyPacket"/> is received, 
        /// in response to a <seealso cref="InfoRequest"/></summary>
        public event ParcelInfoCallback OnParcelInfo;
        /// <summary>Fired when a ParcelProperties Packet is received over the <seealso cref="OpenMetaverse.Capabilities"/> subsystem,
        /// in response to a <seealso cref="PropertiesRequest"/></summary>
        public event ParcelPropertiesCallback OnParcelProperties;
        /// <summary>Fired when a <seealso cref="Packets.ParcelAccessListReplyPacket"/> is received,
        /// in response to a <seealso cref="AccessListRequest"/></summary>
        public event ParcelAccessListReplyCallback OnAccessListReply;
        /// <summary>Fired when the Agent receives a <seealso cref="Packets.ParcelObjectOwnersReplyPacket"/>,
        /// in response to <seealso cref="ObjectOwnersRequest"/></summary>
        public event ParcelObjectOwnersListReplyCallback OnPrimOwnersListReply;
        /// <summary>Fired when the simulator parcel dictionary is populated in response
        /// to a <seealso cref="RequestAllSimParcels"/> request</summary>
        public event SimParcelsDownloaded OnSimParcelsDownloaded;
        /// <summary>Fired when the Agent receives a <seealso cref="Packets.ParcelSelectObjectsPacket"/>,
        /// in response to a <seealso cref="SelectObjects"/> request</summary>
        public event ForceSelectObjects OnParcelSelectedObjects;
        /// <summary>Fired when the Agent receives a <seealso cref="Packets.ParcelMediaUpdatePacket"/> which
        /// occurs when the parcel media information is changed for the current parcel the Agent is over</summary>
        public event ParcelMediaUpdateReplyCallback OnParcelMediaUpdate;

        #endregion Events

        private GridClient Client;

        private AutoResetEvent WaitForSimParcel;
        #region Public Methods

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">A reference to the GridClient object</param>
        public ParcelManager(GridClient client)
        {
            Client = client;
            // Setup the callbacks
            Client.Network.RegisterCallback(PacketType.ParcelInfoReply, new NetworkManager.PacketCallback(ParcelInfoReplyHandler));
            Client.Network.RegisterEventCallback("ParcelObjectOwnersReply", new Caps.EventQueueCallback(ParcelObjectOwnersReplyHandler));
            // CAPS packet handler, to allow for Media Data not contained in the message template
            Client.Network.RegisterEventCallback("ParcelProperties", new Caps.EventQueueCallback(ParcelPropertiesReplyHandler));
            Client.Network.RegisterCallback(PacketType.ParcelDwellReply, new NetworkManager.PacketCallback(ParcelDwellReplyHandler));
            Client.Network.RegisterCallback(PacketType.ParcelAccessListReply, new NetworkManager.PacketCallback(ParcelAccessListReplyHandler));
            Client.Network.RegisterCallback(PacketType.ForceObjectSelect, new NetworkManager.PacketCallback(SelectParcelObjectsReplyHandler));
            Client.Network.RegisterCallback(PacketType.ParcelMediaUpdate, new NetworkManager.PacketCallback(ParcelMediaUpdateHandler));
            Client.Network.RegisterCallback(PacketType.ParcelOverlay, new NetworkManager.PacketCallback(ParcelOverlayHandler));
        }

        /// <summary>
        /// Request basic information for a single parcel
        /// </summary>
        /// <param name="parcelID">Simulator-local ID of the parcel</param>
        public void InfoRequest(UUID parcelID)
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
        /// <param name="flags"></param>
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
        /// Request all simulator parcel properties (used for populating the <code>Simulator.Parcels</code> 
        /// dictionary)
        /// </summary>
        /// <param name="simulator">Simulator to request parcels from (must be connected)</param>
        public void RequestAllSimParcels(Simulator simulator)
        {
            RequestAllSimParcels(simulator, false, 750);
        }

        /// <summary>
        /// Request all simulator parcel properties (used for populating the <code>Simulator.Parcels</code> 
        /// dictionary)
        /// </summary>
        /// <param name="simulator">Simulator to request parcels from (must be connected)</param>
        /// <param name="refresh">If TRUE, will force a full refresh</param>
        /// <param name="msDelay">Number of milliseconds to pause in between each request</param>
        public void RequestAllSimParcels(Simulator simulator, bool refresh, int msDelay)
        {
            if (simulator.DownloadingParcelMap)
            {
                Logger.Log("Already downloading parcels in " + simulator.Name, Helpers.LogLevel.Info, Client);
                return;
            }
            else
            {
                simulator.DownloadingParcelMap = true;
                WaitForSimParcel = new AutoResetEvent(false);
            }

            if (refresh)
            {
                    for (int y = 0; y < 64; y++)
                        for (int x = 0; x < 64; x++)
                            simulator.ParcelMap[y, x] = 0;
            }

            Thread th = new Thread(delegate()
            {
                int count = 0, timeouts = 0, y, x;

                for (y = 0; y < 64; y++)
                {
                    for (x = 0; x < 64; x++)
                    {
                        if (!Client.Network.Connected)
                            return;

                        if (simulator.ParcelMap[y, x] == 0)
                        {
                            Client.Parcels.PropertiesRequest(simulator,
                                                             (y + 1) * 4.0f, (x + 1) * 4.0f,
                                                             y * 4.0f, x * 4.0f, int.MaxValue, false);

                            // Wait the given amount of time for a reply before sending the next request
                            if (!WaitForSimParcel.WaitOne(msDelay, false))
                                ++timeouts;

                            ++count;
                        }
                    }
                }

                Logger.Log(String.Format(
                    "Full simulator parcel information retrieved. Sent {0} parcel requests. Current outgoing queue: {1}, Retry Count {2}",
                    count, Client.Network.OutboxCount, timeouts), Helpers.LogLevel.Info, Client);

                simulator.DownloadingParcelMap = false;
            });

            th.Start();
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
            request.Data.ParcelID = UUID.Zero; // Not used by clients

            Client.Network.SendPacket(request, simulator);
        }

        /// <summary>
        /// Send a request to Purchase a parcel of land
        /// </summary>
        /// <param name="simulator">The Simulator the parcel is located in</param>
        /// <param name="localID">The parcels region specific local ID</param>
        /// <param name="forGroup">true if this parcel is being purchased by a group</param>
        /// <param name="groupID">The groups <seealso cref="T:OpenMetaverse.UUID"/></param>
        /// <param name="removeContribution">true to remove tier contribution if purchase is successful</param>
        /// <param name="parcelArea">The parcels size</param>
        /// <param name="parcelPrice">The purchase price of the parcel</param>
        /// <returns></returns>
        public void Buy(Simulator simulator, int localID, bool forGroup, UUID groupID,
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
        /// Reclaim a parcel of land
        /// </summary>
        /// <param name="simulator">The simulator the parcel is in</param>
        /// <param name="localID">The parcels region specific local ID</param>
        public void Reclaim(Simulator simulator, int localID)
        {
            ParcelReclaimPacket request = new ParcelReclaimPacket();
            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;

            request.Data.LocalID = localID;

            Client.Network.SendPacket(request, simulator);
        }

        /// <summary>
        /// Deed a parcel to a group
        /// </summary>
        /// <param name="simulator">The simulator the parcel is in</param>
        /// <param name="localID">The parcels region specific local ID</param>
        /// <param name="groupID">The groups <seealso cref="T:OpenMetaverse.UUID"/></param>
        public void DeedToGroup(Simulator simulator, int localID, UUID groupID)
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
        /// <param name="localID">The parcels region specific local ID</param>
        public void ObjectOwnersRequest(Simulator simulator, int localID)
        {
            ParcelObjectOwnersRequestPacket request = new ParcelObjectOwnersRequestPacket();

            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;

            request.ParcelData.LocalID = localID;
            Client.Network.SendPacket(request, simulator);
        }

        /// <summary>
        /// Return objects from a parcel
        /// </summary>
        /// <param name="simulator">Simulator parcel is in</param>
        /// <param name="localID">The parcels region specific local ID</param>
        /// <param name="type">the type of objects to return, <seealso cref="T:OpenMetaverse.ObjectReturnType"/></param>
        /// <param name="ownerIDs">A list containing object owners <seealso cref="OpenMetaverse.UUID"/>s to return</param>
        public void ReturnObjects(Simulator simulator, int localID, ObjectReturnType type, List<UUID> ownerIDs)
        {
            ParcelReturnObjectsPacket request = new ParcelReturnObjectsPacket();
            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;

            request.ParcelData.LocalID = localID;
            request.ParcelData.ReturnType = (uint)type;

            // A single null TaskID is (not) used for parcel object returns
            request.TaskIDs = new ParcelReturnObjectsPacket.TaskIDsBlock[1];
            request.TaskIDs[0] = new ParcelReturnObjectsPacket.TaskIDsBlock();
            request.TaskIDs[0].TaskID = UUID.Zero;

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
        /// Subdivide (split) a parcel
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
        /// Join two parcels of land creating a single parcel
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
        /// Get a parcels LocalID
        /// </summary>
        /// <param name="simulator">Simulator parcel is in</param>
        /// <param name="position">Vector3 position in simulator (Z not used)</param>
        /// <returns>0 on failure, or parcel LocalID on success.</returns>
        /// <remarks>A call to <code>Parcels.RequestAllSimParcels</code> is required to populate map and
        /// dictionary.</remarks>
        public int GetParcelLocalID(Simulator simulator, Vector3 position)
        {
            if (simulator.ParcelMap[(byte)position.Y / 4, (byte)position.X / 4] > 0)
            {
                return simulator.ParcelMap[(byte)position.Y / 4, (byte)position.X / 4];
            }
            else
            {
                Logger.Log(String.Format("ParcelMap returned an default/invalid value for location {0}/{1} Did you use RequestAllSimParcels() to populate the dictionaries?", (byte)position.Y / 4, (byte)position.X / 4 ), Helpers.LogLevel.Warning);
                return 0;
            }
        }

        /// <summary>
        /// Terraform (raise, lower, etc) an area or whole parcel of land
        /// </summary>
        /// <param name="simulator">Simulator land area is in.</param>
        /// <param name="localID">LocalID of parcel, or -1 if using bounding box</param>
        /// <param name="action">From Enum, Raise, Lower, Level, Smooth, Etc.</param>
        /// <param name="brushSize">Size of area to modify</param>
        /// <returns>true on successful request sent.</returns>
        /// <remarks>Settings.STORE_LAND_PATCHES must be true, 
        /// Parcel information must be downloaded using <code>RequestAllSimParcels()</code></remarks>
        public bool Terraform(Simulator simulator, int localID, TerraformAction action, TerraformBrushSize brushSize)
        {
            return Terraform(simulator, localID, 0f, 0f, 0f, 0f, action, brushSize, 1);
        }

        /// <summary>
        /// Terraform (raise, lower, etc) an area or whole parcel of land
        /// </summary>
        /// <param name="simulator">Simulator land area is in.</param>
        /// <param name="west">west border of area to modify</param>
        /// <param name="south">south border of area to modify</param>
        /// <param name="east">east border of area to modify</param>
        /// <param name="north">north border of area to modify</param>
        /// <param name="action">From Enum, Raise, Lower, Level, Smooth, Etc.</param>
        /// <param name="brushSize">Size of area to modify</param>
        /// <returns>true on successful request sent.</returns>
        /// <remarks>Settings.STORE_LAND_PATCHES must be true, 
        /// Parcel information must be downloaded using <code>RequestAllSimParcels()</code></remarks>
        public bool Terraform(Simulator simulator, float west, float south, float east, float north,
            TerraformAction action, TerraformBrushSize brushSize)
        {
            return Terraform(simulator, -1, west, south, east, north, action, brushSize, 1);
        }

        /// <summary>
        /// Terraform (raise, lower, etc) an area or whole parcel of land
        /// </summary>
        /// <param name="simulator">Simulator land area is in.</param>
        /// <param name="localID">LocalID of parcel, or -1 if using bounding box</param>
        /// <param name="west">west border of area to modify</param>
        /// <param name="south">south border of area to modify</param>
        /// <param name="east">east border of area to modify</param>
        /// <param name="north">north border of area to modify</param>
        /// <param name="action">From Enum, Raise, Lower, Level, Smooth, Etc.</param>
        /// <param name="brushSize">Size of area to modify</param>
        /// <param name="seconds">How many meters + or - to lower, 1 = 1 meter</param>
        /// <returns>true on successful request sent.</returns>
        /// <remarks>Settings.STORE_LAND_PATCHES must be true, 
        /// Parcel information must be downloaded using <code>RequestAllSimParcels()</code></remarks>
        public bool Terraform(Simulator simulator, int localID, float west, float south, float east, float north,
            TerraformAction action, TerraformBrushSize brushSize, int seconds)
        {
            float height = 0f;
            int x, y;
            if (localID == -1)
            {
                x = (int)east - (int)west / 2;
                y = (int)north - (int)south / 2;
            }
            else
            {
                Parcel p;
                if (!simulator.Parcels.TryGetValue(localID, out p))
                {
                    Logger.Log(String.Format("Can't find parcel {0} in simulator {1}", localID, simulator),
                        Helpers.LogLevel.Warning, Client);
                    return false;
                }

                x = (int)p.AABBMax.X - (int)p.AABBMin.X / 2;
                y = (int)p.AABBMax.Y - (int)p.AABBMin.Y / 2;
            }

            if (!Client.Terrain.TerrainHeightAtPoint(simulator.Handle, x, y, out height))
            {
                Logger.Log("Land Patch not stored for location", Helpers.LogLevel.Warning, Client);
                return false;
            }

            Terraform(simulator, localID, west, south, east, north, action, brushSize, seconds, height);
            return true;
        }

        /// <summary>
        /// Terraform (raise, lower, etc) an area or whole parcel of land
        /// </summary>
        /// <param name="simulator">Simulator land area is in.</param>
        /// <param name="localID">LocalID of parcel, or -1 if using bounding box</param>
        /// <param name="west">west border of area to modify</param>
        /// <param name="south">south border of area to modify</param>
        /// <param name="east">east border of area to modify</param>
        /// <param name="north">north border of area to modify</param>
        /// <param name="action">From Enum, Raise, Lower, Level, Smooth, Etc.</param>
        /// <param name="brushSize">Size of area to modify</param>
        /// <param name="seconds">How many meters + or - to lower, 1 = 1 meter</param>
        /// <param name="height">Height at which the terraform operation is acting at</param>
        public void Terraform(Simulator simulator, int localID, float west, float south, float east, float north,
            TerraformAction action, TerraformBrushSize brushSize, int seconds, float height)
        {
            ModifyLandPacket land = new ModifyLandPacket();
            land.AgentData.AgentID = Client.Self.AgentID;
            land.AgentData.SessionID = Client.Self.SessionID;

            land.ModifyBlock.Action = (byte)action;
            land.ModifyBlock.BrushSize = (byte)brushSize;
            land.ModifyBlock.Seconds = seconds;
            land.ModifyBlock.Height = height;

            land.ParcelData = new ModifyLandPacket.ParcelDataBlock[1];
            land.ParcelData[0] = new ModifyLandPacket.ParcelDataBlock();
            land.ParcelData[0].LocalID = localID;
            land.ParcelData[0].West = west;
            land.ParcelData[0].South = south;
            land.ParcelData[0].East = east;
            land.ParcelData[0].North = north;

            Client.Network.SendPacket(land, simulator);
        }

        /// <summary>
        /// Sends a request to the simulator to return a list of objects owned by specific owners
        /// </summary>
        /// <param name="localID">Simulator local ID of parcel</param>
        /// <param name="selectType">Owners, Others, Etc</param>
        /// <param name="ownerID">List containing keys of avatars objects to select; 
        /// if List is null will return Objects of type <c>selectType</c></param>
        /// <remarks>Response data is returned in the event <seealso cref="E:OnParcelSelectedObjects"/></remarks>
        public void SelectObjects(int localID, ObjectReturnType selectType, UUID ownerID)
        {
            if (OnParcelSelectedObjects != null)
            {
                ParcelSelectObjectsPacket select = new ParcelSelectObjectsPacket();
                select.AgentData.AgentID = Client.Self.AgentID;
                select.AgentData.SessionID = Client.Self.SessionID;

                select.ParcelData.LocalID = localID;
                select.ParcelData.ReturnType = (uint)selectType;

                select.ReturnIDs = new ParcelSelectObjectsPacket.ReturnIDsBlock[1];
                select.ReturnIDs[0] = new ParcelSelectObjectsPacket.ReturnIDsBlock();
                select.ReturnIDs[0].ReturnID = ownerID;
                Client.Network.SendPacket(select);
            }
        }

        /// <summary>
        /// Eject and optionally ban a user from a parcel
        /// </summary>
        /// <param name="targetID">target key of avatar to eject</param>
        /// <param name="ban">true to also ban target</param>
        public void EjectUser(UUID targetID, bool ban)
        {
            EjectUserPacket eject = new EjectUserPacket();
            eject.AgentData.AgentID = Client.Self.AgentID;
            eject.AgentData.SessionID = Client.Self.SessionID;
            eject.Data.TargetID = targetID;
            if (ban) eject.Data.Flags = 1;
            else eject.Data.Flags = 0;

            Client.Network.SendPacket(eject);
        }

        /// <summary>
        /// Freeze or unfreeze an avatar over your land
        /// </summary>
        /// <param name="targetID">target key to freeze</param>
        /// <param name="freeze">true to freeze, false to unfreeze</param>
        public void FreezeUser(UUID targetID, bool freeze)
        {
            FreezeUserPacket frz = new FreezeUserPacket();
            frz.AgentData.AgentID = Client.Self.AgentID;
            frz.AgentData.SessionID = Client.Self.SessionID;
            frz.Data.TargetID = targetID;
            if (freeze) frz.Data.Flags = 0;
            else frz.Data.Flags = 1;

            Client.Network.SendPacket(frz);
        }

        /// <summary>
        /// Abandon a parcel of land
        /// </summary>
        /// <param name="simulator">Simulator parcel is in</param>
        /// <param name="localID">Simulator local ID of parcel</param>
        public void ReleaseParcel(Simulator simulator, int localID)
        {
            ParcelReleasePacket abandon = new ParcelReleasePacket();
            abandon.AgentData.AgentID = Client.Self.AgentID;
            abandon.AgentData.SessionID = Client.Self.SessionID;
            abandon.Data.LocalID = localID;

            Client.Network.SendPacket(abandon, simulator);
        }
        #endregion Public Methods

        #region Packet Handlers

        private void ParcelDwellReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnParcelDwell != null || Client.Settings.ALWAYS_REQUEST_PARCEL_DWELL == true)
            {
                ParcelDwellReplyPacket dwell = (ParcelDwellReplyPacket)packet;

                lock (simulator.Parcels.Dictionary)
                {
                    if (simulator.Parcels.Dictionary.ContainsKey(dwell.Data.LocalID))
                    {
                        Parcel parcel = simulator.Parcels.Dictionary[dwell.Data.LocalID];
                        parcel.Dwell = dwell.Data.Dwell;
                        simulator.Parcels.Dictionary[dwell.Data.LocalID] = parcel;
                    }
                }

                if (OnParcelDwell != null)
                {
                    try { OnParcelDwell(dwell.Data.ParcelID, dwell.Data.LocalID, dwell.Data.Dwell); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
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
                parcelInfo.Description = Utils.BytesToString(info.Data.Desc);
                parcelInfo.Dwell = info.Data.Dwell;
                parcelInfo.GlobalX = info.Data.GlobalX;
                parcelInfo.GlobalY = info.Data.GlobalY;
                parcelInfo.GlobalZ = info.Data.GlobalZ;
                parcelInfo.ID = info.Data.ParcelID;
                parcelInfo.Mature = ((info.Data.Flags & 1) != 0) ? true : false;
                parcelInfo.Name = Utils.BytesToString(info.Data.Name);
                parcelInfo.OwnerID = info.Data.OwnerID;
                parcelInfo.SalePrice = info.Data.SalePrice;
                parcelInfo.SimName = Utils.BytesToString(info.Data.SimName);
                parcelInfo.SnapshotID = info.Data.SnapshotID;

                try { OnParcelInfo(parcelInfo); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }
        /// <summary>
        /// ParcelProperties replies sent over CAPS
        /// </summary>
        /// <param name="capsKey">Not used (will always be ParcelProperties)</param>
        /// <param name="llsd">LLSD Structured data</param>
        /// <param name="simulator">Object representing simulator</param>
        private void ParcelPropertiesReplyHandler(string capsKey, OSD llsd, Simulator simulator)
        {
            if (OnParcelProperties != null || Client.Settings.PARCEL_TRACKING == true)
            {             
                OSDMap map = (OSDMap)llsd;
                OSDMap parcelDataBlock = (OSDMap)(((OSDArray)map["ParcelData"])[0]);
                OSDMap ageVerifyBlock = (OSDMap)(((OSDArray)map["AgeVerificationBlock"])[0]);

                OSDMap mediaDataBlock=null;
                if(map.ContainsKey("MediaData")) //OpenSim compatability, does not yet do this via caps so make it optional
                    mediaDataBlock = (OSDMap)(((OSDArray)map["MediaData"])[0]);

                Parcel parcel = new Parcel(parcelDataBlock["LocalID"].AsInteger());

                parcel.AABBMax = ((OSDArray)parcelDataBlock["AABBMax"]).AsVector3();
                parcel.AABBMin = ((OSDArray)parcelDataBlock["AABBMin"]).AsVector3();
                parcel.Area = parcelDataBlock["Area"].AsInteger();
                parcel.AuctionID = (uint)parcelDataBlock["AuctionID"].AsInteger();
                parcel.AuthBuyerID = parcelDataBlock["AuthBuyerID"].AsUUID();
                parcel.Bitmap = parcelDataBlock["Bitmap"].AsBinary();
                parcel.Category = (ParcelCategory)parcelDataBlock["Category"].AsInteger();
                parcel.ClaimDate = Utils.UnixTimeToDateTime((uint)parcelDataBlock["ClaimDate"].AsInteger());
                parcel.ClaimPrice = parcelDataBlock["ClaimPrice"].AsInteger();
                parcel.Desc = parcelDataBlock["Desc"].AsString();
                
                // TODO: this probably needs to happen when the packet is deserialized.
                byte[] bytes = parcelDataBlock["ParcelFlags"].AsBinary();
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);
                parcel.Flags = (ParcelFlags)BitConverter.ToUInt32(bytes, 0);
                parcel.GroupID = parcelDataBlock["GroupID"].AsUUID();
                parcel.GroupPrims = parcelDataBlock["GroupPrims"].AsInteger();
                parcel.IsGroupOwned = parcelDataBlock["IsGroupOwned"].AsBoolean();
                parcel.Landing = (LandingType)(byte)parcelDataBlock["LandingType"].AsInteger();
                parcel.LocalID = parcelDataBlock["LocalID"].AsInteger();
                parcel.MaxPrims = parcelDataBlock["MaxPrims"].AsInteger();
                parcel.Media.MediaAutoScale = (byte)parcelDataBlock["MediaAutoScale"].AsInteger(); 
                parcel.Media.MediaID = parcelDataBlock["MediaID"].AsUUID();
                parcel.Media.MediaURL = parcelDataBlock["MediaURL"].AsString();
                parcel.MusicURL = parcelDataBlock["MusicURL"].AsString();
                parcel.Name = parcelDataBlock["Name"].AsString();
                parcel.OtherCleanTime = parcelDataBlock["OtherCleanTime"].AsInteger();
                parcel.OtherCount = parcelDataBlock["OtherCount"].AsInteger();
                parcel.OtherPrims = parcelDataBlock["OtherPrims"].AsInteger();
                parcel.OwnerID = parcelDataBlock["OwnerID"].AsUUID();
                parcel.OwnerPrims = parcelDataBlock["OwnerPrims"].AsInteger();
                parcel.ParcelPrimBonus = (float)parcelDataBlock["ParcelPrimBonus"].AsReal();
                parcel.PassHours = (float)parcelDataBlock["PassHours"].AsReal();
                parcel.PassPrice = parcelDataBlock["PassPrice"].AsInteger();
                parcel.PublicCount = parcelDataBlock["PublicCount"].AsInteger();
                parcel.RegionDenyAgeUnverified = ageVerifyBlock["RegionDenyAgeUnverified"].AsBoolean();
                parcel.RegionDenyAnonymous = parcelDataBlock["RegionDenyAnonymous"].AsBoolean();
                parcel.RegionPushOverride = parcelDataBlock["RegionPushOverride"].AsBoolean();
                parcel.RentPrice = parcelDataBlock["RentPrice"].AsInteger();
                ParcelResult result = (ParcelResult)parcelDataBlock["RequestResult"].AsInteger();
                parcel.SalePrice = parcelDataBlock["SalePrice"].AsInteger();
                int selectedPrims = parcelDataBlock["SelectedPrims"].AsInteger();
                parcel.SelfCount = parcelDataBlock["SelfCount"].AsInteger();
                int sequenceID = parcelDataBlock["SequenceID"].AsInteger();
                parcel.SimWideMaxPrims = parcelDataBlock["SimWideMaxPrims"].AsInteger();
                parcel.SimWideTotalPrims = parcelDataBlock["SimWideTotalPrims"].AsInteger();
                bool snapSelection = parcelDataBlock["SnapSelection"].AsBoolean();
                parcel.SnapshotID = parcelDataBlock["SnapshotID"].AsUUID();
                parcel.Status = (ParcelStatus)parcelDataBlock["Status"].AsInteger();
                parcel.TotalPrims = parcelDataBlock["TotalPrims"].AsInteger();
                parcel.UserLocation = ((OSDArray)parcelDataBlock["UserLocation"]).AsVector3();
                parcel.UserLookAt = ((OSDArray)parcelDataBlock["UserLookAt"]).AsVector3();

                if(mediaDataBlock!=null)
                {
                    parcel.Media.MediaDesc = mediaDataBlock["MediaDesc"].AsString();
                    parcel.Media.MediaHeight = mediaDataBlock["MediaHeight"].AsInteger();
                    parcel.Media.MediaWidth = mediaDataBlock["MediaWidth"].AsInteger();
                    parcel.Media.MediaLoop = mediaDataBlock["MediaLoop"].AsBoolean();
                    parcel.Media.MediaType = mediaDataBlock["MediaType"].AsString();
                    parcel.ObscureMedia = mediaDataBlock["ObscureMedia"].AsBoolean();
                    parcel.ObscureMusic = mediaDataBlock["ObscureMusic"].AsBoolean();
                }

                if (Client.Settings.PARCEL_TRACKING)
                {
                    lock (simulator.Parcels.Dictionary)
                        simulator.Parcels.Dictionary[parcel.LocalID] = parcel;

                    bool set = false;
                    int y, x, index, bit;
                    for (y = 0; y < 64; y++)
                    {
                        for (x = 0; x < 64; x++)
                        {
                            index = (y * 64) + x;
                            bit = index % 8;
                            index >>= 3;

                            if ((parcel.Bitmap[index] & (1 << bit)) != 0)
                            {
                                simulator.ParcelMap[y, x] = parcel.LocalID;
                                set = true;
                            }
                        }
                    }

                    if (!set)
                    {
                        Logger.Log("Received a parcel with a bitmap that did not map to any locations",
                            Helpers.LogLevel.Warning);
                    }
                }

                if (sequenceID.Equals(int.MaxValue) && WaitForSimParcel != null)
                    WaitForSimParcel.Set();

                // auto request acl, will be stored in parcel tracking dictionary if enabled
                if (Client.Settings.ALWAYS_REQUEST_PARCEL_ACL)
                    Client.Parcels.AccessListRequest(simulator, parcel.LocalID,
                        AccessList.Both, sequenceID);

                // auto request dwell, will be stored in parcel tracking dictionary if enables
                if (Client.Settings.ALWAYS_REQUEST_PARCEL_DWELL)
                    Client.Parcels.DwellRequest(simulator, parcel.LocalID);

                // Fire the callback for parcel properties being received
                if (OnParcelProperties != null)
                {
                    try { OnParcelProperties(simulator, parcel, result, selectedPrims, sequenceID, snapSelection); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }

                // Check if all of the simulator parcels have been retrieved, if so fire another callback
                if (simulator.IsParcelMapFull() && OnSimParcelsDownloaded != null)
                {
                    try { OnSimParcelsDownloaded(simulator, simulator.Parcels, simulator.ParcelMap); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        protected void ParcelAccessListReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnAccessListReply != null || Client.Settings.ALWAYS_REQUEST_PARCEL_ACL == true)
            {
                ParcelAccessListReplyPacket reply = (ParcelAccessListReplyPacket)packet;

                List<ParcelAccessEntry> accessList = new List<ParcelAccessEntry>(reply.List.Length);
                   
                    for (int i = 0; i < reply.List.Length; i++)
                    {
                        ParcelAccessEntry pae = new ParcelAccessEntry();
                        pae.AgentID = reply.List[i].ID;
                        pae.Time = Utils.UnixTimeToDateTime((uint)reply.List[i].Time);
                        pae.Flags = (AccessList)reply.List[i].Flags;

                        accessList.Add(pae);
                    }

                    lock (simulator.Parcels.Dictionary)
                    {
                        if (simulator.Parcels.Dictionary.ContainsKey(reply.Data.LocalID))
                        {
                            Parcel parcel = simulator.Parcels.Dictionary[reply.Data.LocalID];
                            if ((AccessList)reply.Data.Flags == AccessList.Ban)
                                parcel.AccessBlackList = accessList;
                            else
                                parcel.AccessWhiteList = accessList;

                            simulator.Parcels.Dictionary[reply.Data.LocalID] = parcel;
                        }
                    }
                

                if (OnAccessListReply != null)
                {
                    try
                    {
                        OnAccessListReply(simulator, reply.Data.SequenceID, reply.Data.LocalID, reply.Data.Flags,
                      accessList);
                    }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="capsKey"></param>
        /// <param name="llsd"></param>
        /// <param name="simulator"></param>
        private void ParcelObjectOwnersReplyHandler(string capsKey, OSD llsd, Simulator simulator)
        {
            if (OnPrimOwnersListReply != null)
            {

                OSDMap map = (OSDMap)llsd;
                List<ParcelPrimOwners> primOwners = new List<ParcelPrimOwners>();

                if (map.ContainsKey("Data") && map.ContainsKey("DataExtended"))
                {

                    OSDArray dataBlock = (OSDArray)map["Data"];
                    OSDArray dataExtendedBlock = (OSDArray)map["DataExtended"];

                    for (int i = 0; i < dataBlock.Count; i++)
                    {
                        ParcelPrimOwners poe = new ParcelPrimOwners();
                        poe.OwnerID = ((OSDMap)dataBlock[i])["OwnerID"].AsUUID();
                        poe.Count = ((OSDMap)dataBlock[i])["Count"].AsInteger();
                        poe.IsGroupOwned = ((OSDMap)dataBlock[i])["IsGroupOwned"].AsBoolean();
                        poe.OnlineStatus = ((OSDMap)dataBlock[i])["OnlineStatus"].AsBoolean();
                        if (((OSDMap)dataExtendedBlock[i]).ContainsKey("TimeStamp"))
                        {
                            byte[] bytes = (((OSDMap)dataExtendedBlock[i])["TimeStamp"].AsBinary());

                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(bytes);

                            uint value = Utils.BytesToUInt(bytes);

                            poe.NewestPrim = Utils.UnixTimeToDateTime(value);
                        }

                        primOwners.Add(poe);
                    }

                   
                }
                else
                {
                    // the server will send back a response even when there are no prims
                    primOwners.Add(new ParcelPrimOwners());    
                }

                if (OnPrimOwnersListReply != null)
                {
                    try { OnPrimOwnersListReply(simulator, primOwners); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void SelectParcelObjectsReplyHandler(Packet packet, Simulator simulator)
        {
            ForceObjectSelectPacket reply = (ForceObjectSelectPacket)packet;
            List<uint> objectIDs = new List<uint>(reply.Data.Length);

            for (int i = 0; i < reply.Data.Length; i++)
            {
                objectIDs.Add(reply.Data[i].LocalID);
            }

            if (OnParcelSelectedObjects != null)
            {
                try { OnParcelSelectedObjects(simulator, objectIDs, reply._Header.ResetList); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void ParcelMediaUpdateHandler(Packet packet, Simulator simulator)
        {
            ParcelMediaUpdatePacket reply = (ParcelMediaUpdatePacket)packet;
            ParcelMedia media = new ParcelMedia();

            media.MediaAutoScale = reply.DataBlock.MediaAutoScale;
            media.MediaID = reply.DataBlock.MediaID;
            media.MediaDesc = Utils.BytesToString(reply.DataBlockExtended.MediaDesc);
            media.MediaHeight = reply.DataBlockExtended.MediaHeight;
            media.MediaLoop = ((reply.DataBlockExtended.MediaLoop & 1) != 0) ? true : false;
            media.MediaType = Utils.BytesToString(reply.DataBlockExtended.MediaType);
            media.MediaWidth = reply.DataBlockExtended.MediaWidth;
            media.MediaURL = Utils.BytesToString(reply.DataBlock.MediaURL);

            if (OnParcelMediaUpdate != null)
            {
                try { OnParcelMediaUpdate(simulator, media); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void ParcelOverlayHandler(Packet packet, Simulator simulator)
        {
            const int OVERLAY_COUNT = 4;

            ParcelOverlayPacket overlay = (ParcelOverlayPacket)packet;

            if (overlay.ParcelData.SequenceID >= 0 && overlay.ParcelData.SequenceID < OVERLAY_COUNT)
            {
                int length = overlay.ParcelData.Data.Length;

                Buffer.BlockCopy(overlay.ParcelData.Data, 0, simulator.ParcelOverlay,
                    overlay.ParcelData.SequenceID * length, length);
                simulator.ParcelOverlaysReceived++;

                if (simulator.ParcelOverlaysReceived >= OVERLAY_COUNT)
                {
                    // TODO: ParcelOverlaysReceived should become internal, and reset to zero every 
                    // time it hits four. Also need a callback here
                }
            }
            else
            {
                Logger.Log("Parcel overlay with sequence ID of " + overlay.ParcelData.SequenceID +
                    " received from " + simulator.ToString(), Helpers.LogLevel.Warning, Client);
            }
        }

        #endregion Packet Handlers
    }
}
