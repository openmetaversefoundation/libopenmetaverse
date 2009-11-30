/*
 * Copyright (c) 2007-2009, openmetaverse.org
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
using System.Reflection;
using System.Collections.Generic;
using OpenMetaverse.Http;
using OpenMetaverse.Packets;
using OpenMetaverse.Interfaces;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Messages.Linden;

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
    /// The tool to use when modifying terrain levels
    /// </summary>
    public enum TerraformAction : byte
    {
        /// <summary>Level the terrain</summary>
        Level = 0,
        /// <summary>Raise the terrain</summary>
        Raise = 1,
        /// <summary>Lower the terrain</summary>
        Lower = 2,
        /// <summary>Smooth the terrain</summary>
        Smooth = 3,
        /// <summary>Add random noise to the terrain</summary>
        Noise = 4,
        /// <summary>Revert terrain to simulator default</summary>
        Revert = 5
    }

    /// <summary>
    /// The tool size to use when changing terrain levels
    /// </summary>
    public enum TerraformBrushSize : byte
    {
        /// <summary>Small</summary>
        Small = 1,
        /// <summary>Medium</summary>
        Medium = 2,
        /// <summary>Large</summary>
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
    [Flags]
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

    /// <summary>
    /// Parcel Media Command used in ParcelMediaCommandMessage
    /// </summary>
    public enum ParcelMediaCommand : uint
    {
        /// <summary>Stop the media stream and go back to the first frame</summary>
        Stop = 0,
        /// <summary>Pause the media stream (stop playing but stay on current frame)</summary>
        Pause,
        /// <summary>Start the current media stream playing and stop when the end is reached</summary>
        Play,
        /// <summary>Start the current media stream playing, 
        /// loop to the beginning when the end is reached and continue to play</summary>
        Loop,
        /// <summary>Specifies the texture to replace with video</summary>
        /// <remarks>If passing the key of a texture, it must be explicitly typecast as a key, 
        /// not just passed within double quotes.</remarks>
        Texture,
        /// <summary>Specifies the movie URL (254 characters max)</summary>
        URL,
        /// <summary>Specifies the time index at which to begin playing</summary>
        Time,
        /// <summary>Specifies a single agent to apply the media command to</summary>
        Agent,
        /// <summary>Unloads the stream. While the stop command sets the texture to the first frame of the movie, 
        /// unload resets it to the real texture that the movie was replacing.</summary>
        Unload,
        /// <summary>Turn on/off the auto align feature, similar to the auto align checkbox in the parcel media properties 
        /// (NOT to be confused with the "align" function in the textures view of the editor!) Takes TRUE or FALSE as parameter.</summary>
        AutoAlign,
        /// <summary>Allows a Web page or image to be placed on a prim (1.19.1 RC0 and later only). 
        /// Use "text/html" for HTML.</summary>
        Type,
        /// <summary>Resizes a Web page to fit on x, y pixels (1.19.1 RC0 and later only).</summary>
        /// <remarks>This might still not be working</remarks>
        Size,
        /// <summary>Sets a description for the media being displayed (1.19.1 RC0 and later only).</summary>
        Desc
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
        public bool MediaAutoScale;
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
        /// <summary>The total number of contiguous 4x4 meter blocks your agent owns within this parcel</summary>        
        public int SelfCount;
        /// <summary>The total number of contiguous 4x4 meter blocks contained in this parcel</summary>
        public int OtherCount;
        /// <summary>Deprecated, Value appears to always be 0</summary>
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
        /// <summary>Maximum primitives across the entire simulator owned by the same agent or group that owns this parcel that can be used</summary>
        public int SimWideMaxPrims;
        /// <summary>Total primitives across the entire simulator calculated by combining the allowed prim counts for each parcel
        /// owned by the agent or group that owns this parcel</summary>
        public int SimWideTotalPrims;
        /// <summary>Maximum number of primitives this parcel supports</summary>
        public int MaxPrims;
        /// <summary>Total number of primitives on this parcel</summary>
        public int TotalPrims;
        /// <summary>Total number of primitives owned by the parcel owner on 
        /// this parcel, for parcels owned by an individual this value will be 0</summary>
        public int OwnerPrims;
        /// <summary>Total number of primitives owned by the parcel group on 
        /// this parcel, or for parcels owned by an individual with a group set the
        /// total number of prims set to that group.</summary>
        public int GroupPrims;
        /// <summary>Total number of prims owned by other avatars that are not set to group, or not the parcel owner</summary>
        public int OtherPrims;
        /// <summary>A bonus multiplier which allows parcel prim counts to go over times this amount, this does not affect
        /// the max prims per simulator. e.g: 117 prim parcel limit x 1.5 bonus = 175 allowed</summary>
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
        /// <summary>The landing point location</summary>
        public Vector3 UserLocation;
        /// <summary>The landing point LookAt</summary>
        public Vector3 UserLookAt;
        /// <summary>The type of landing enforced from the <see cref="LandingType"/> enum</summary>
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
                ParcelPropertiesUpdateMessage req = new ParcelPropertiesUpdateMessage();
                req.AuthBuyerID = this.AuthBuyerID;
                req.Category = this.Category;
                req.Desc = this.Desc;
                req.GroupID = this.GroupID;
                req.Landing = this.Landing;
                req.LocalID = this.LocalID;
                req.MediaAutoScale = this.Media.MediaAutoScale;
                req.MediaDesc = this.Media.MediaDesc;
                req.MediaHeight = this.Media.MediaHeight;
                req.MediaID = this.Media.MediaID;
                req.MediaLoop = this.Media.MediaLoop;
                req.MediaType = this.Media.MediaType;
                req.MediaURL = this.Media.MediaURL;
                req.MediaWidth = this.Media.MediaWidth;
                req.MusicURL = this.MusicURL;
                req.Name = this.Name;
                req.ObscureMedia = this.ObscureMedia;
                req.ObscureMusic = this.ObscureMusic;
                req.ParcelFlags = this.Flags;
                req.PassHours = this.PassHours;
                req.PassPrice = (uint)this.PassPrice;
                req.SalePrice = (uint)this.SalePrice;
                req.SnapshotID = this.SnapshotID;
                req.UserLocation = this.UserLocation;
                req.UserLookAt = this.UserLookAt;
               
                OSDMap body = req.Serialize();

                CapsClient capsPost = new CapsClient(url);
                capsPost.BeginGetResponse(body, OSDFormat.Xml, simulator.Client.Settings.CAPS_TIMEOUT);
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
                request.ParcelData.MediaAutoScale = (this.Media.MediaAutoScale) ? (byte)0x1 : (byte)0x0;
                request.ParcelData.MediaID = this.Media.MediaID;
                request.ParcelData.MediaURL = Utils.StringToBytes(this.Media.MediaURL.ToString());
                request.ParcelData.MusicURL = Utils.StringToBytes(this.MusicURL.ToString());
                request.ParcelData.Name = Utils.StringToBytes(this.Name);
                if (wantReply) request.ParcelData.Flags = 1;
                request.ParcelData.ParcelFlags = (uint)this.Flags;
                request.ParcelData.PassHours = this.PassHours;
                request.ParcelData.PassPrice = this.PassPrice;
                request.ParcelData.SalePrice = this.SalePrice;
                request.ParcelData.SnapshotID = this.SnapshotID;
                request.ParcelData.UserLocation = this.UserLocation;
                request.ParcelData.UserLookAt = this.UserLookAt;

                simulator.SendPacket(request);
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

            simulator.SendPacket(request);
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

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ParcelDwellReplyEventArgs> m_DwellReply;

        /// <summary>Raises the ParcelDwellReply event</summary>
        /// <param name="e">A ParcelDwellReplyEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnParcelDwellReply(ParcelDwellReplyEventArgs e)
        {
            EventHandler<ParcelDwellReplyEventArgs> handler = m_DwellReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_DwellReplyLock = new object();
        
        /// <summary>Raised when the simulator responds to a <see cref="RequestDwell"/> request</summary>
        public event EventHandler<ParcelDwellReplyEventArgs> ParcelDwellReply
        {
            add { lock (m_DwellReplyLock) { m_DwellReply += value; } }
            remove { lock (m_DwellReplyLock) { m_DwellReply -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ParcelInfoReplyEventArgs> m_ParcelInfo;

        /// <summary>Raises the ParcelInfoReply event</summary>
        /// <param name="e">A ParcelInfoReplyEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnParcelInfoReply(ParcelInfoReplyEventArgs e)
        {
            EventHandler<ParcelInfoReplyEventArgs> handler = m_ParcelInfo;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ParcelInfoLock = new object();

        /// <summary>Raised when the simulator responds to a <see cref="RequestParcelInfo"/> request</summary>
        public event EventHandler<ParcelInfoReplyEventArgs> ParcelInfoReply
        {
            add { lock (m_ParcelInfoLock) { m_ParcelInfo += value; } }
            remove { lock (m_ParcelInfoLock) { m_ParcelInfo -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ParcelPropertiesEventArgs> m_ParcelProperties;

        /// <summary>Raises the ParcelProperties event</summary>
        /// <param name="e">A ParcelPropertiesEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnParcelProperties(ParcelPropertiesEventArgs e)
        {
            EventHandler<ParcelPropertiesEventArgs> handler = m_ParcelProperties;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ParcelPropertiesLock = new object();

        /// <summary>Raised when the simulator responds to a <see cref="RequestParcelProperties"/> request</summary>
        public event EventHandler<ParcelPropertiesEventArgs> ParcelProperties
        {
            add { lock (m_ParcelPropertiesLock) { m_ParcelProperties += value; } }
            remove { lock (m_ParcelPropertiesLock) { m_ParcelProperties -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ParcelAccessListReplyEventArgs> m_ParcelACL;

        /// <summary>Raises the ParcelAccessListReply event</summary>
        /// <param name="e">A ParcelAccessListReplyEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnParcelAccessListReply(ParcelAccessListReplyEventArgs e)
        {
            EventHandler<ParcelAccessListReplyEventArgs> handler = m_ParcelACL;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ParcelACLLock = new object();

        /// <summary>Raised when the simulator responds to a <see cref="RequestParcelAccessList"/> request</summary>
        public event EventHandler<ParcelAccessListReplyEventArgs> ParcelAccessListReply
        {
            add { lock (m_ParcelACLLock) { m_ParcelACL += value; } }
            remove { lock (m_ParcelACLLock) { m_ParcelACL -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ParcelObjectOwnersReplyEventArgs> m_ParcelObjectOwnersReply;

        /// <summary>Raises the ParcelObjectOwnersReply event</summary>
        /// <param name="e">A ParcelObjectOwnersReplyEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnParcelObjectOwnersReply(ParcelObjectOwnersReplyEventArgs e)
        {
            EventHandler<ParcelObjectOwnersReplyEventArgs> handler = m_ParcelObjectOwnersReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ParcelObjectOwnersLock = new object();

        /// <summary>Raised when the simulator responds to a <see cref="RequestObjectOwners"/> request</summary>
        public event EventHandler<ParcelObjectOwnersReplyEventArgs> ParcelObjectOwnersReply
        {
            add { lock (m_ParcelObjectOwnersLock) { m_ParcelObjectOwnersReply += value; } }
            remove { lock (m_ParcelObjectOwnersLock) { m_ParcelObjectOwnersReply -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<SimParcelsDownloadedEventArgs> m_SimParcelsDownloaded;

        /// <summary>Raises the SimParcelsDownloaded event</summary>
        /// <param name="e">A SimParcelsDownloadedEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnSimParcelsDownloaded(SimParcelsDownloadedEventArgs e)
        {
            EventHandler<SimParcelsDownloadedEventArgs> handler = m_SimParcelsDownloaded;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_SimParcelsDownloadedLock = new object();

        /// <summary>Raised when the simulator responds to a <see cref="RequestAllSimParcels"/> request</summary>
        public event EventHandler<SimParcelsDownloadedEventArgs> SimParcelsDownloaded
        {
            add { lock (m_SimParcelsDownloadedLock) { m_SimParcelsDownloaded += value; } }
            remove { lock (m_SimParcelsDownloadedLock) { m_SimParcelsDownloaded -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ForceSelectObjectsReplyEventArgs> m_ForceSelectObjects;

        /// <summary>Raises the ForceSelectObjectsReply event</summary>
        /// <param name="e">A ForceSelectObjectsReplyEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnForceSelectObjectsReply(ForceSelectObjectsReplyEventArgs e)
        {
            EventHandler<ForceSelectObjectsReplyEventArgs> handler = m_ForceSelectObjects;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ForceSelectObjectsLock = new object();

        /// <summary>Raised when the simulator responds to a <see cref="RequestForceSelectObjects"/> request</summary>
        public event EventHandler<ForceSelectObjectsReplyEventArgs> ForceSelectObjectsReply
        {
            add { lock (m_ForceSelectObjectsLock) { m_ForceSelectObjects += value; } }
            remove { lock (m_ForceSelectObjectsLock) { m_ForceSelectObjects -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ParcelMediaUpdateReplyEventArgs> m_ParcelMediaUpdateReply;

        /// <summary>Raises the ParcelMediaUpdateReply event</summary>
        /// <param name="e">A ParcelMediaUpdateReplyEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnParcelMediaUpdateReply(ParcelMediaUpdateReplyEventArgs e)
        {
            EventHandler<ParcelMediaUpdateReplyEventArgs> handler = m_ParcelMediaUpdateReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ParcelMediaUpdateReplyLock = new object();

        /// <summary>Raised when the simulator responds to a Parcel Update request</summary>
        public event EventHandler<ParcelMediaUpdateReplyEventArgs> ParcelMediaUpdateReply
        {
            add { lock (m_ParcelMediaUpdateReplyLock) { m_ParcelMediaUpdateReply += value; } }
            remove { lock (m_ParcelMediaUpdateReplyLock) { m_ParcelMediaUpdateReply -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ParcelMediaCommandEventArgs> m_ParcelMediaCommand;

        /// <summary>Raises the ParcelMediaCommand event</summary>
        /// <param name="e">A ParcelMediaCommandEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnParcelMediaCommand(ParcelMediaCommandEventArgs e)
        {
            EventHandler<ParcelMediaCommandEventArgs> handler = m_ParcelMediaCommand;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ParcelMediaCommandLock = new object();

        /// <summary>Raised when the parcel your agent is located sends a ParcelMediaCommand</summary>
        public event EventHandler<ParcelMediaCommandEventArgs> ParcelMediaCommand
        {
            add { lock (m_ParcelMediaCommandLock) { m_ParcelMediaCommand += value; } }
            remove { lock (m_ParcelMediaCommandLock) { m_ParcelMediaCommand -= value; } }
        }
        #endregion Delegates


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
            Client.Network.RegisterCallback(PacketType.ParcelInfoReply, ParcelInfoReplyHandler);
            Client.Network.RegisterEventCallback("ParcelObjectOwnersReply", new Caps.EventQueueCallback(ParcelObjectOwnersReplyHandler));
            // CAPS packet handler, to allow for Media Data not contained in the message template
            Client.Network.RegisterEventCallback("ParcelProperties", new Caps.EventQueueCallback(ParcelPropertiesReplyHandler));
            Client.Network.RegisterCallback(PacketType.ParcelDwellReply, ParcelDwellReplyHandler);
            Client.Network.RegisterCallback(PacketType.ParcelAccessListReply, ParcelAccessListReplyHandler);
            Client.Network.RegisterCallback(PacketType.ForceObjectSelect, SelectParcelObjectsReplyHandler);
            Client.Network.RegisterCallback(PacketType.ParcelMediaUpdate, ParcelMediaUpdateHandler);
            Client.Network.RegisterCallback(PacketType.ParcelOverlay, ParcelOverlayHandler);
            Client.Network.RegisterCallback(PacketType.ParcelMediaCommandMessage, ParcelMediaCommandMessagePacketHandler);
        }

        /// <summary>
        /// Request basic information for a single parcel
        /// </summary>
        /// <param name="parcelID">Simulator-local ID of the parcel</param>
        public void RequestParcelInfo(UUID parcelID)
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
        public void RequestParcelProperties(Simulator simulator, int localID, int sequenceID)
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
        public void RequestParcelAccessList(Simulator simulator, int localID, AccessList flags, int sequenceID)
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
        public void RequestParcelProperties(Simulator simulator, float north, float east, float south, float west,
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
                            Client.Parcels.RequestParcelProperties(simulator,
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
        public void RequestDwell(Simulator simulator, int localID)
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
        public void RequestObjectOwners(Simulator simulator, int localID)
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
        public void RequestSelectObjects(int localID, ObjectReturnType selectType, UUID ownerID)
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

        /// <summary>
        /// Requests the UUID of the parcel in a remote region at a specified location
        /// </summary>
        /// <param name="location">Location of the parcel in the remote region</param>
        /// <param name="regionHandle">Remote region handle</param>
        /// <param name="regionID">Remote region UUID</param>
        /// <returns>If successful UUID of the remote parcel, UUID.Zero otherwise</returns>
        public UUID RequestRemoteParcelID(Vector3 location, ulong regionHandle, UUID regionID)
        {
            Uri url = Client.Network.CurrentSim.Caps.CapabilityURI("RemoteParcelRequest");

            if (url != null)
            {
                RemoteParcelRequestRequest msg = new RemoteParcelRequestRequest();
                msg.Location = location;
                msg.RegionHandle = regionHandle;
                msg.RegionID = regionID;

                try
                {
                    CapsClient request = new CapsClient(url);
                    OSD result = request.GetResponse(msg.Serialize(), OSDFormat.Xml, Client.Settings.CAPS_TIMEOUT);
                    RemoteParcelRequestReply response = new RemoteParcelRequestReply();
                    response.Deserialize((OSDMap)result);
                    return response.ParcelID;
                }
                catch (Exception)
                {
                    Logger.Log("Failed to fetch remote parcel ID", Helpers.LogLevel.Debug, Client);
                }
            }
            
            return UUID.Zero;

        }
        #endregion Public Methods

        #region Packet Handlers

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        /// <remarks>Raises the <see cref="ParcelDwellReply"/> event</remarks>
        protected void ParcelDwellReplyHandler(object sender, PacketReceivedEventArgs e)
        {            
            if (m_DwellReply != null || Client.Settings.ALWAYS_REQUEST_PARCEL_DWELL == true)
            {
                Packet packet = e.Packet;
                Simulator simulator = e.Simulator;

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

                if (m_DwellReply != null)
                {
                    OnParcelDwellReply(new ParcelDwellReplyEventArgs(dwell.Data.ParcelID, dwell.Data.LocalID, dwell.Data.Dwell));
                }
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        /// <remarks>Raises the <see cref="ParcelInfoReply"/> event</remarks>
        protected void ParcelInfoReplyHandler(object sender, PacketReceivedEventArgs e)
        {            
            if (m_ParcelInfo != null)
            {
                Packet packet = e.Packet;
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

                OnParcelInfoReply(new ParcelInfoReplyEventArgs(parcelInfo));                
            }
        }

        protected void ParcelPropertiesReplyHandler(string capsKey, IMessage message, Simulator simulator)
        {                        
            if (m_ParcelProperties != null || Client.Settings.PARCEL_TRACKING == true)
            {
                ParcelPropertiesMessage msg = (ParcelPropertiesMessage)message;
                
                Parcel parcel = new Parcel(msg.LocalID);

                parcel.AABBMax = msg.AABBMax;
                parcel.AABBMin = msg.AABBMin;
                parcel.Area = msg.Area;
                parcel.AuctionID = msg.AuctionID;
                parcel.AuthBuyerID = msg.AuthBuyerID;
                parcel.Bitmap = msg.Bitmap;
                parcel.Category = msg.Category;
                parcel.ClaimDate = msg.ClaimDate;
                parcel.ClaimPrice = msg.ClaimPrice;
                parcel.Desc = msg.Desc;
                parcel.Flags = msg.ParcelFlags;
                parcel.GroupID = msg.GroupID;
                parcel.GroupPrims = msg.GroupPrims;
                parcel.IsGroupOwned = msg.IsGroupOwned;
                parcel.Landing = msg.LandingType;
                parcel.MaxPrims = msg.MaxPrims;
                parcel.Media.MediaAutoScale = msg.MediaAutoScale;
                parcel.Media.MediaID = msg.MediaID;
                parcel.Media.MediaURL = msg.MediaURL;
                parcel.MusicURL = msg.MusicURL;
                parcel.Name = msg.Name;
                parcel.OtherCleanTime = msg.OtherCleanTime;
                parcel.OtherCount = msg.OtherCount;
                parcel.OtherPrims = msg.OtherPrims;
                parcel.OwnerID = msg.OwnerID;
                parcel.OwnerPrims = msg.OwnerPrims;
                parcel.ParcelPrimBonus = msg.ParcelPrimBonus;
                parcel.PassHours = msg.PassHours;
                parcel.PassPrice = msg.PassPrice;
                parcel.PublicCount = msg.PublicCount;
                parcel.RegionDenyAgeUnverified = msg.RegionDenyAgeUnverified;
                parcel.RegionDenyAnonymous = msg.RegionDenyAnonymous;
                parcel.RegionPushOverride = msg.RegionPushOverride;
                parcel.RentPrice = msg.RentPrice;
                ParcelResult result = msg.RequestResult;
                parcel.SalePrice = msg.SalePrice;
                int selectedPrims = msg.SelectedPrims;
                parcel.SelfCount = msg.SelfCount;
                int sequenceID = msg.SequenceID;
                parcel.SimWideMaxPrims = msg.SimWideMaxPrims;
                parcel.SimWideTotalPrims = msg.SimWideTotalPrims;
                bool snapSelection = msg.SnapSelection;
                parcel.SnapshotID = msg.SnapshotID;
                parcel.Status = msg.Status;
                parcel.TotalPrims = msg.TotalPrims;
                parcel.UserLocation = msg.UserLocation;
                parcel.UserLookAt = msg.UserLookAt;
                parcel.Media.MediaDesc = msg.MediaDesc;
                parcel.Media.MediaHeight = msg.MediaHeight;
                parcel.Media.MediaWidth = msg.MediaWidth;
                parcel.Media.MediaLoop = msg.MediaLoop;
                parcel.Media.MediaType = msg.MediaType;
                parcel.ObscureMedia = msg.ObscureMedia;
                parcel.ObscureMusic = msg.ObscureMusic;

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
                    Client.Parcels.RequestParcelAccessList(simulator, parcel.LocalID,
                        AccessList.Both, sequenceID);

                // auto request dwell, will be stored in parcel tracking dictionary if enables
                if (Client.Settings.ALWAYS_REQUEST_PARCEL_DWELL)
                    Client.Parcels.RequestDwell(simulator, parcel.LocalID);

                // Fire the callback for parcel properties being received
                if (m_ParcelProperties != null)
                {
                    OnParcelProperties(new ParcelPropertiesEventArgs(simulator, parcel, result, selectedPrims, sequenceID, snapSelection));                    
                }

                // Check if all of the simulator parcels have been retrieved, if so fire another callback
                if (simulator.IsParcelMapFull() && m_SimParcelsDownloaded != null)
                {
                    OnSimParcelsDownloaded(new SimParcelsDownloadedEventArgs(simulator, simulator.Parcels, simulator.ParcelMap));
                }
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        /// <remarks>Raises the <see cref="ParcelAccessListReply"/> event</remarks>
        protected void ParcelAccessListReplyHandler(object sender, PacketReceivedEventArgs e)
        {
            if (m_ParcelACL != null || Client.Settings.ALWAYS_REQUEST_PARCEL_ACL == true)
            {
                Packet packet = e.Packet;
                Simulator simulator = e.Simulator;

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
                

                if (m_ParcelACL != null)
                {
                    OnParcelAccessListReply(new ParcelAccessListReplyEventArgs(simulator, reply.Data.SequenceID, reply.Data.LocalID, 
                        reply.Data.Flags, accessList));                    
                }
            }
        }
        
        protected void ParcelObjectOwnersReplyHandler(string capsKey, IMessage message, Simulator simulator)
        {
            if (m_ParcelObjectOwnersReply != null)
            {
                List<ParcelPrimOwners> primOwners = new List<ParcelPrimOwners>();

                ParcelObjectOwnersReplyMessage msg = (ParcelObjectOwnersReplyMessage)message;
                
                for (int i = 0; i < msg.PrimOwnersBlock.Length; i++)
                {
                    ParcelPrimOwners primOwner = new ParcelPrimOwners();
                    primOwner.OwnerID = msg.PrimOwnersBlock[i].OwnerID;
                    primOwner.Count = msg.PrimOwnersBlock[i].Count;
                    primOwner.IsGroupOwned = msg.PrimOwnersBlock[i].IsGroupOwned;
                    primOwner.OnlineStatus = msg.PrimOwnersBlock[i].OnlineStatus;
                    primOwner.NewestPrim = msg.PrimOwnersBlock[i].TimeStamp;

                    primOwners.Add(primOwner);
                }

                OnParcelObjectOwnersReply(new ParcelObjectOwnersReplyEventArgs(simulator, primOwners));
            }                
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        /// <remarks>Raises the <see cref="ForceSelectObjectsReply"/> event</remarks>
        protected void SelectParcelObjectsReplyHandler(object sender, PacketReceivedEventArgs e)
        {
            if (m_ForceSelectObjects != null)
            {
                Packet packet = e.Packet;
                Simulator simulator = e.Simulator;

                ForceObjectSelectPacket reply = (ForceObjectSelectPacket)packet;
                List<uint> objectIDs = new List<uint>(reply.Data.Length);

                for (int i = 0; i < reply.Data.Length; i++)
                {
                    objectIDs.Add(reply.Data[i].LocalID);
                }

                OnForceSelectObjectsReply(new ForceSelectObjectsReplyEventArgs(simulator, objectIDs, reply._Header.ResetList));
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        /// <remarks>Raises the <see cref="ParcelMediaUpdateReply"/> event</remarks>
        protected void ParcelMediaUpdateHandler(object sender, PacketReceivedEventArgs e)
        {
            if (m_ParcelMediaUpdateReply != null)
            {
                Packet packet = e.Packet;
                Simulator simulator = e.Simulator;

                ParcelMediaUpdatePacket reply = (ParcelMediaUpdatePacket)packet;
                ParcelMedia media = new ParcelMedia();

                media.MediaAutoScale = (reply.DataBlock.MediaAutoScale == (byte)0x1) ? true : false;
                media.MediaID = reply.DataBlock.MediaID;
                media.MediaDesc = Utils.BytesToString(reply.DataBlockExtended.MediaDesc);
                media.MediaHeight = reply.DataBlockExtended.MediaHeight;
                media.MediaLoop = ((reply.DataBlockExtended.MediaLoop & 1) != 0) ? true : false;
                media.MediaType = Utils.BytesToString(reply.DataBlockExtended.MediaType);
                media.MediaWidth = reply.DataBlockExtended.MediaWidth;
                media.MediaURL = Utils.BytesToString(reply.DataBlock.MediaURL);

                OnParcelMediaUpdateReply(new ParcelMediaUpdateReplyEventArgs(simulator, media));
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void ParcelOverlayHandler(object sender, PacketReceivedEventArgs e)
        {
            const int OVERLAY_COUNT = 4;
            Packet packet = e.Packet;
            Simulator simulator = e.Simulator;

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

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        /// <remarks>Raises the <see cref="ParcelMediaCommand"/> event</remarks>
        protected void ParcelMediaCommandMessagePacketHandler(object sender, PacketReceivedEventArgs e)
        {
            if (m_ParcelMediaCommand != null)
            {
                Packet packet = e.Packet;
                Simulator simulator = e.Simulator;

                ParcelMediaCommandMessagePacket pmc = (ParcelMediaCommandMessagePacket)packet;
                ParcelMediaCommandMessagePacket.CommandBlockBlock block = pmc.CommandBlock;

                OnParcelMediaCommand(new ParcelMediaCommandEventArgs(simulator, pmc.Header.Sequence, (ParcelFlags)block.Flags, 
                    (ParcelMediaCommand)block.Command, block.Time));                
            }
        }

        #endregion Packet Handlers
    }
    #region EventArgs classes
    
    /// <summary>Contains a parcels dwell data returned from the simulator in response to an <see cref="RequestParcelDwell"/></summary>
    public class ParcelDwellReplyEventArgs : EventArgs
    {
        private readonly UUID m_ParcelID;
        private readonly int m_LocalID;
        private readonly float m_Dwell;

        /// <summary>Get the global ID of the parcel</summary>
        public UUID ParcelID { get { return m_ParcelID; } }
        /// <summary>Get the simulator specific ID of the parcel</summary>
        public int LocalID { get { return m_LocalID; } }
        /// <summary>Get the calculated dwell</summary>
        public float Dwell { get { return m_Dwell; } }

        /// <summary>
        /// Construct a new instance of the ParcelDwellReplyEventArgs class
        /// </summary>
        /// <param name="parcelID">The global ID of the parcel</param>
        /// <param name="localID">The simulator specific ID of the parcel</param>
        /// <param name="dwell">The calculated dwell for the parcel</param>
        public ParcelDwellReplyEventArgs(UUID parcelID, int localID, float dwell)
        {
            this.m_ParcelID = parcelID;
            this.m_LocalID = localID;
            this.m_Dwell = dwell;
        }
    }

    /// <summary>Contains basic parcel information data returned from the 
    /// simulator in response to an <see cref="RequestParcelInfo"/> request</summary>
    public class ParcelInfoReplyEventArgs : EventArgs
    {        
        private readonly ParcelInfo m_Parcel;

        /// <summary>Get the <see cref="ParcelInfo"/> object containing basic parcel info</summary>
        public ParcelInfo Parcel { get { return m_Parcel; } }

        /// <summary>
        /// Construct a new instance of the ParcelInfoReplyEventArgs class
        /// </summary>
        /// <param name="parcel">The <see cref="ParcelInfo"/> object containing basic parcel info</param>
        public ParcelInfoReplyEventArgs(ParcelInfo parcel)
        {
            this.m_Parcel = parcel;
        }
    }

    /// <summary>Contains basic parcel information data returned from the simulator in response to an <see cref="RequestParcelInfo"/> request</summary>
    public class ParcelPropertiesEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private Parcel m_Parcel;
        private readonly ParcelResult m_Result;
        private readonly int m_SelectedPrims;
        private readonly int m_SequenceID;
        private readonly bool m_SnapSelection;

        /// <summary>Get the simulator the parcel is located in</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary>Get the <see cref="Parcel"/> object containing the details</summary>
        /// <remarks>If Result is NoData, this object will not contain valid data</remarks>
        public Parcel Parcel { get { return m_Parcel; } }
        /// <summary>Get the result of the request</summary>
        public ParcelResult Result { get { return m_Result; } }
        /// <summary>Get the number of primitieves your agent is 
        /// currently selecting and or sitting on in this parcel</summary>
        public int SelectedPrims { get { return m_SelectedPrims; } }
        /// <summary>Get the user assigned ID used to correlate a request with
        /// these results</summary>
        public int SequenceID { get { return m_SequenceID; } }
        /// <summary>TODO:</summary>
        public bool SnapSelection { get { return m_SnapSelection; } }

        /// <summary>
        /// Construct a new instance of the ParcelPropertiesEventArgs class
        /// </summary>
        /// <param name="simulator">The <see cref="Parcel"/> object containing the details</param>
        /// <param name="parcel">The <see cref="Parcel"/> object containing the details</param>
        /// <param name="result">The result of the request</param>
        /// <param name="selectedPrims">The number of primitieves your agent is 
        /// currently selecting and or sitting on in this parcel</param>
        /// <param name="sequenceID">The user assigned ID used to correlate a request with
        /// these results</param>
        /// <param name="snapSelection">TODO:</param>
        public ParcelPropertiesEventArgs(Simulator simulator, Parcel parcel, ParcelResult result, int selectedPrims,
            int sequenceID, bool snapSelection)
        {
            this.m_Simulator = simulator;
            this.m_Parcel = parcel;
            this.m_Result = result;
            this.m_SelectedPrims = selectedPrims;
            this.m_SequenceID = sequenceID;
            this.m_SnapSelection = snapSelection;
        }
    }
    
    /// <summary>Contains blacklist and whitelist data returned from the simulator in response to an <see cref="RequestParcelAccesslist"/> request</summary>
    public class ParcelAccessListReplyEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly int m_SequenceID;
        private readonly int m_LocalID;
        private readonly uint m_Flags;
        private readonly List<ParcelManager.ParcelAccessEntry> m_AccessList;

        /// <summary>Get the simulator the parcel is located in</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary>Get the user assigned ID used to correlate a request with
        /// these results</summary>
        public int SequenceID { get { return m_SequenceID; } }
        /// <summary>Get the simulator specific ID of the parcel</summary>
        public int LocalID { get { return m_LocalID; } }
        /// <summary>TODO:</summary>
        public uint Flags { get { return m_Flags; } }
        /// <summary>Get the list containing the white/blacklisted agents for the parcel</summary>
        public List<ParcelManager.ParcelAccessEntry> AccessList { get { return m_AccessList; } }

        /// <summary>
        /// Construct a new instance of the ParcelAccessListReplyEventArgs class
        /// </summary>
        /// <param name="simulator">The simulator the parcel is located in</param>
        /// <param name="sequenceID">The user assigned ID used to correlate a request with
        /// these results</param>
        /// <param name="localID">The simulator specific ID of the parcel</param>
        /// <param name="flags">TODO:</param>
        /// <param name="accessEntries">The list containing the white/blacklisted agents for the parcel</param>
        public ParcelAccessListReplyEventArgs(Simulator simulator, int sequenceID, int localID, uint flags, List<ParcelManager.ParcelAccessEntry> accessEntries)
        {
            this.m_Simulator = simulator;
            this.m_SequenceID = sequenceID;
            this.m_LocalID = localID;
            this.m_Flags = flags;
            this.m_AccessList = accessEntries;
        }
    }
    
    /// <summary>Contains blacklist and whitelist data returned from the 
    /// simulator in response to an <see cref="RequestParcelAccesslist"/> request</summary>
    public class ParcelObjectOwnersReplyEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly List<ParcelManager.ParcelPrimOwners> m_Owners;

        /// <summary>Get the simulator the parcel is located in</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary>Get the list containing prim ownership counts</summary>
        public List<ParcelManager.ParcelPrimOwners> PrimOwners { get { return m_Owners; } }

        /// <summary>
        /// Construct a new instance of the ParcelObjectOwnersReplyEventArgs class
        /// </summary>
        /// <param name="simulator">The simulator the parcel is located in</param>
        /// <param name="primOwners">The list containing prim ownership counts</param>
        public ParcelObjectOwnersReplyEventArgs(Simulator simulator, List<ParcelManager.ParcelPrimOwners> primOwners)
        {
            this.m_Simulator = simulator;
            this.m_Owners = primOwners;
        }
    }

    /// <summary>Contains the data returned when all parcel data has been retrieved from a simulator</summary>
    public class SimParcelsDownloadedEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly InternalDictionary<int, Parcel> m_Parcels;
        private readonly int[,] m_ParcelMap;

        /// <summary>Get the simulator the parcel data was retrieved from</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary>A dictionary containing the parcel data where the key correlates to the ParcelMap entry</summary>
        public InternalDictionary<int, Parcel> Parcels { get { return m_Parcels; } }
        /// <summary>Get the multidimensional array containing a x,y grid mapped
        /// to each 64x64 parcel's LocalID.</summary>
        public int[,] ParcelMap { get { return m_ParcelMap; } }

        /// <summary>
        /// Construct a new instance of the SimParcelsDownloadedEventArgs class
        /// </summary>
        /// <param name="simulator">The simulator the parcel data was retrieved from</param>
        /// <param name="simParcels">The dictionary containing the parcel data</param>
        /// <param name="parcelMap">The multidimensional array containing a x,y grid mapped
        /// to each 64x64 parcel's LocalID.</param>
        public SimParcelsDownloadedEventArgs(Simulator simulator, InternalDictionary<int, Parcel> simParcels, int[,] parcelMap)
        {
            this.m_Simulator = simulator;
            this.m_Parcels = simParcels;
            this.m_ParcelMap = parcelMap;
        }
    }
    
    /// <summary>Contains the data returned when a <see cref="RequestForceSelectObjects"/> request</summary>
    public class ForceSelectObjectsReplyEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly List<uint> m_ObjectIDs;
        private readonly bool m_ResetList;

        /// <summary>Get the simulator the parcel data was retrieved from</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary>Get the list of primitive IDs</summary>
        public List<uint> ObjectIDs { get { return m_ObjectIDs; } }
        /// <summary>true if the list is clean and contains the information
        /// only for a given request</summary>
        public bool ResetList { get { return m_ResetList; } }

        /// <summary>
        /// Construct a new instance of the ForceSelectObjectsReplyEventArgs class
        /// </summary>
        /// <param name="simulator">The simulator the parcel data was retrieved from</param>
        /// <param name="objectIDs">The list of primitive IDs</param>
        /// <param name="resetList">true if the list is clean and contains the information
        /// only for a given request</param>
        public ForceSelectObjectsReplyEventArgs(Simulator simulator, List<uint> objectIDs, bool resetList)
        {
            this.m_Simulator = simulator;
            this.m_ObjectIDs = objectIDs;
            this.m_ResetList = resetList;
        }
    }
   
    /// <summary>Contains data when the media data for a parcel the avatar is on changes</summary>
    public class ParcelMediaUpdateReplyEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly ParcelMedia m_ParcelMedia;

        /// <summary>Get the simulator the parcel media data was updated in</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary>Get the updated media information</summary>
        public ParcelMedia Media { get { return m_ParcelMedia; } }
        
        /// <summary>
        /// Construct a new instance of the ParcelMediaUpdateReplyEventArgs class
        /// </summary>
        /// <param name="simulator">the simulator the parcel media data was updated in</param>
        /// <param name="media">The updated media information</param>
        public ParcelMediaUpdateReplyEventArgs(Simulator simulator, ParcelMedia media)
        {
            this.m_Simulator = simulator;
            this.m_ParcelMedia = media;
        }
    }

    /// <summary>Contains the media command for a parcel the agent is currently on</summary>
    public class ParcelMediaCommandEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly uint m_Sequence;
        private readonly ParcelFlags m_ParcelFlags;
        private readonly ParcelMediaCommand m_MediaCommand;
        private readonly float m_Time;

        /// <summary>Get the simulator the parcel media command was issued in</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary></summary>
        public uint Sequence { get { return m_Sequence; } }
        /// <summary></summary>
        public ParcelFlags ParcelFlags { get { return m_ParcelFlags; } }
        /// <summary>Get the media command that was sent</summary>
        public ParcelMediaCommand MediaCommand { get { return m_MediaCommand; } }
        /// <summary></summary>
        public float Time { get { return m_Time; } }

        /// <summary>
        /// Construct a new instance of the ParcelMediaCommandEventArgs class
        /// </summary>
        /// <param name="simulator">The simulator the parcel media command was issued in</param>
        /// <param name="sequence"></param>
        /// <param name="flags"></param>
        /// <param name="command">The media command that was sent</param>
        /// <param name="time"></param>
        public ParcelMediaCommandEventArgs(Simulator simulator, uint sequence, ParcelFlags flags, ParcelMediaCommand command, float time)
        {
            this.m_Simulator = simulator;
            this.m_Sequence = sequence;
            this.m_ParcelFlags = flags;
            this.m_MediaCommand = command;
            this.m_Time = time;
        }
    }
    #endregion
}
