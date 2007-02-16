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
using System.Timers;
using System.Collections.Generic;
using libsecondlife.Packets;

namespace libsecondlife
{
    /// <summary>
    /// Parcel information retrieved from a simulator
    /// </summary>
    public class Parcel
    {
        /// <summary>
        /// 
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
            /// <summary></summary>
            AllowTerraform = 1 << 4,
            /// <summary>Avatars have health and can take damage on this parcel</summary>
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
        }


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
        /// <summary></summary>
        public int LocalID;
        /// <summary>Key of land owner</summary>
        public LLUUID OwnerID;
        /// <summary>Is the land group owned</summary>
        public bool IsGroupOwned;
        /// <summary></summary>
        public uint AuctionID;
        /// <summary>Presumably for first land</summary>
        public bool ReservedNewbie;
        /// <summary>Date land was claimed</summary>
        public int ClaimDate;
        /// <summary></summary>
        public int ClaimPrice;
        /// <summary></summary>
        public int RentPrice;
        /// <summary></summary>
        public LLVector3 AABBMin;
        /// <summary></summary>
        public LLVector3 AABBMax;
        /// <summary>Bitmap describing land layout in 4x4m squares across the entire region</summary>
        public byte[] Bitmap;
        /// <summary>Total land area</summary>
        public int Area;
        /// <summary></summary>
        public byte Status;
        /// <summary>Max objects across region</summary>
        public int SimWideMaxObjects;
        /// <summary>Total objects across region</summary>
        public int SimWideTotalObjects;
        /// <summary>Max objects for parcel</summary>
        public int MaxObjects;
        /// <summary>Total objects in parcel</summary>
        public int TotalObjects;
        /// <summary>Total objects for owner</summary>
        public int OwnerObjects;
        /// <summary>Total objects for group</summary>
        public int GroupObjects;
        /// <summary>Total for other objects</summary>
        public int OtherObjects;
        /// <summary></summary>
        public float ParcelObjectBonus;
        /// <summary></summary>
        public int OtherCleanTime;
        /// <summary></summary>
        public ParcelFlags Flags;
        /// <summary></summary>
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
        public byte Category;
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
        public Simulator Simulator;

        private void init()
        {
            OwnerID = LLUUID.Zero;
            AABBMin = LLVector3.Zero;
            AABBMax = LLVector3.Zero;
            Bitmap = new byte[512];
            MediaID = LLUUID.Zero;
            GroupID = LLUUID.Zero;
            AuthBuyerID = LLUUID.Zero;
            SnapshotID = LLUUID.Zero;
            UserLocation = LLVector3.Zero;
            UserLookAt = LLVector3.Zero;
        }

        /// <summary>
        /// 
        /// </summary>
        public Parcel()
        {
            init();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        public Parcel(Simulator simulator)
        {
            Simulator = simulator;
            init();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public void GetDwell(SecondLife client)
        {
            ParcelDwellRequestPacket request = new ParcelDwellRequestPacket();
            request.AgentData.AgentID = client.Network.AgentID;
            request.AgentData.SessionID = client.Network.SessionID;
            request.Data.LocalID = LocalID;
            request.Data.ParcelID = LLUUID.Zero;

            client.Network.SendPacket((Packet)request, Simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="forGroup"></param>
        /// <param name="groupID"></param>
        /// <param name="removeContribution"></param>
        /// <returns></returns>
        public bool Buy(SecondLife client, bool forGroup, LLUUID groupID, bool removeContribution)
        {
            ParcelBuyPacket request = new ParcelBuyPacket();

            request.AgentData.AgentID = client.Network.AgentID;
            request.AgentData.SessionID = client.Network.SessionID;

            request.Data.Final = true;
            request.Data.GroupID = groupID;
            request.Data.LocalID = this.LocalID;
            request.Data.IsGroupOwned = forGroup;
            request.Data.RemoveContribution = removeContribution;

            client.Network.SendPacket((Packet)request, Simulator);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool Reclaim(SecondLife client)
        {
            ParcelReclaimPacket request = new ParcelReclaimPacket();
            request.AgentData.AgentID = client.Network.AgentID;
            request.AgentData.SessionID = client.Network.SessionID;

            request.Data.LocalID = this.LocalID;

            client.Network.SendPacket((Packet)request, Simulator);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="groupID"></param>
        /// <returns></returns>
        public bool Deed(SecondLife client, LLUUID groupID)
        {
            ParcelDeedToGroupPacket request = new ParcelDeedToGroupPacket();
            request.AgentData.AgentID = client.Network.AgentID;
            request.AgentData.SessionID = client.Network.SessionID;

            request.Data.LocalID = this.LocalID;
            request.Data.GroupID = groupID;

            client.Network.SendPacket((Packet)request, Simulator);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public void Update(SecondLife client)
        {
            ParcelPropertiesUpdatePacket request = new ParcelPropertiesUpdatePacket();

            request.AgentData.AgentID = client.Network.AgentID;
            request.AgentData.SessionID = client.Network.SessionID;

            request.ParcelData.Flags = 0xFFFFFFFF; // TODO: Probably very important
            request.ParcelData.LocalID = this.LocalID;

            request.ParcelData.AuthBuyerID = this.AuthBuyerID;
            request.ParcelData.Category = this.Category;
            request.ParcelData.Desc = Helpers.StringToField(this.Desc);
            request.ParcelData.GroupID = this.GroupID;
            request.ParcelData.LandingType = this.LandingType;
            request.ParcelData.MediaAutoScale = this.MediaAutoScale;
            request.ParcelData.MediaID = this.MediaID;
            request.ParcelData.MediaURL = Helpers.StringToField(this.MediaURL);
            request.ParcelData.MusicURL = Helpers.StringToField(this.MusicURL);
            request.ParcelData.Name = Helpers.StringToField(this.Name);
            request.ParcelData.Flags = (uint)this.Flags;
            request.ParcelData.PassHours = this.PassHours;
            request.ParcelData.PassPrice = this.PassPrice;
            request.ParcelData.SalePrice = this.SalePrice;
            request.ParcelData.SnapshotID = this.SnapshotID;
            request.ParcelData.UserLocation = this.UserLocation;
            request.ParcelData.UserLookAt = this.UserLookAt;

            client.Network.SendPacket(request, Simulator);
            //Packet updatePacket = Packets.Parcel.ParcelPropertiesUpdate(client.Protocol, client.Avatar.ID, client.Network.SessionID, this);
            //Sim.SendPacket(updatePacket, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="returnType"></param>
        public void ReturnObjects(SecondLife client, uint returnType)
        {
            // TODO: ENUM for returnType

            ParcelReturnObjectsPacket request = new ParcelReturnObjectsPacket();
            request.AgentData.AgentID = client.Network.AgentID;
            request.AgentData.SessionID = client.Network.SessionID;

            request.ParcelData.LocalID = this.LocalID;
            request.ParcelData.ReturnType = returnType;

            // TODO: Handling of TaskIDs and OwnerIDs
            request.OwnerIDs = new ParcelReturnObjectsPacket.OwnerIDsBlock[0];
            request.TaskIDs = new ParcelReturnObjectsPacket.TaskIDsBlock[1];

            client.Network.SendPacket((Packet)request, Simulator);
        }
    }

    public struct ParcelInfo
    {
        public LLUUID ID;
        public LLUUID OwnerID;
        public string Name;
        public string Description;
        public int ActualArea;
        public int BillableArea;
        public bool Mature;
        public float GlobalX;
        public float GlobalY;
        public float GlobalZ;
        public string SimName;
        public LLUUID SnapshotID;
        public float Dwell;
        public int SalePrice;
        public int AuctionID;
    }

    /// <summary>
    /// Parcel (subdivided simulator lots) subsystem
    /// </summary>
    public class ParcelManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parcelID"></param>
        /// <param name="localID"></param>
        /// <param name="dwell"></param>
        public delegate void ParcelDwellCallback(LLUUID parcelID, int localID, float dwell);
        public delegate void ParcelInfoCallback(ParcelInfo parcel);
        public delegate void ParcelPropertiesCallback(Parcel parcel);

        /// <summary>
        /// 
        /// </summary>
        public event ParcelDwellCallback OnParcelDwell;
        public event ParcelInfoCallback OnParcelInfo;
        public event ParcelPropertiesCallback OnParcelProperties;


        private SecondLife Client;

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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parcelID"></param>
        public void ParcelInfoRequest(LLUUID parcelID)
        {
            ParcelInfoRequestPacket request = new ParcelInfoRequestPacket();
            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;
            request.Data.ParcelID = parcelID;

            Client.Network.SendPacket((Packet)request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="north"></param>
        /// <param name="east"></param>
        /// <param name="south"></param>
        /// <param name="west"></param>
        /// <param name="sequenceID"></param>
        /// <param name="snapSelection"></param>
        public void ParcelPropertiesRequest(Simulator simulator, float north, float east, float south, float west,
            int sequenceID, bool snapSelection)
        {
            ParcelPropertiesRequestPacket request = new ParcelPropertiesRequestPacket();

            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;
            request.ParcelData.North = north;
            request.ParcelData.East = east;
            request.ParcelData.South = south;
            request.ParcelData.West = west;
            request.ParcelData.SequenceID = sequenceID;
            request.ParcelData.SnapSelection = snapSelection;

            Client.Network.SendPacket(request, simulator);
        }

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
                parcelInfo.Description = Helpers.FieldToString(info.Data.Desc);
                parcelInfo.Dwell = info.Data.Dwell;
                parcelInfo.GlobalX = info.Data.GlobalX;
                parcelInfo.GlobalY = info.Data.GlobalY;
                parcelInfo.GlobalZ = info.Data.GlobalZ;
                parcelInfo.ID = info.Data.ParcelID;
                parcelInfo.Mature = ((info.Data.Flags & 1) != 0) ? true : false;
                parcelInfo.Name = Helpers.FieldToString(info.Data.Name);
                parcelInfo.OwnerID = info.Data.OwnerID;
                parcelInfo.SalePrice = info.Data.SalePrice;
                parcelInfo.SimName = Helpers.FieldToString(info.Data.SimName);
                parcelInfo.SnapshotID = info.Data.SnapshotID;

                try { OnParcelInfo(parcelInfo); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        private void ParcelPropertiesHandler(Packet packet, Simulator simulator)
        {
            if (OnParcelProperties != null)
            {
                ParcelPropertiesPacket properties = (ParcelPropertiesPacket)packet;

                Parcel parcel = new Parcel(simulator);

                // TODO: Lots of values are not being stored, Parcel needs to be expanded to take all the data.
                // August2006:  God help me should I have to type this out again... argh.
                // October2006: I really shouldnt have typed that.
                parcel.RequestResult = properties.ParcelData.RequestResult;
                parcel.SequenceID = properties.ParcelData.SequenceID;
                parcel.SnapSelection = properties.ParcelData.SnapSelection;
                parcel.SelfCount = properties.ParcelData.SelfCount;
                parcel.OtherCount = properties.ParcelData.OtherCount;
                parcel.PublicCount = properties.ParcelData.PublicCount;
                parcel.LocalID = properties.ParcelData.LocalID;
                parcel.OwnerID = properties.ParcelData.OwnerID;
                parcel.IsGroupOwned = properties.ParcelData.IsGroupOwned;
                parcel.AuctionID = properties.ParcelData.AuctionID;
                parcel.ReservedNewbie = properties.ParcelData.ReservedNewbie;
                parcel.ClaimDate = properties.ParcelData.ClaimDate;
                parcel.ClaimPrice = properties.ParcelData.ClaimPrice;
                parcel.RentPrice = properties.ParcelData.RentPrice;
                parcel.AABBMin = properties.ParcelData.AABBMin;
                parcel.AABBMax = properties.ParcelData.AABBMax;
                parcel.Bitmap = properties.ParcelData.Bitmap;
                parcel.Area = properties.ParcelData.Area;
                parcel.Status = properties.ParcelData.Status;
                parcel.SimWideMaxObjects = properties.ParcelData.SimWideMaxPrims;
                parcel.SimWideTotalObjects = properties.ParcelData.SimWideTotalPrims;
                parcel.MaxObjects = properties.ParcelData.MaxPrims;
                parcel.TotalObjects = properties.ParcelData.TotalPrims;
                parcel.OwnerObjects = properties.ParcelData.OwnerPrims;
                parcel.GroupObjects = properties.ParcelData.GroupPrims;
                parcel.OtherObjects = properties.ParcelData.OtherPrims;
                parcel.ParcelObjectBonus = properties.ParcelData.ParcelPrimBonus;
                parcel.OtherCleanTime = properties.ParcelData.OtherCleanTime;
                parcel.Flags = (Parcel.ParcelFlags)properties.ParcelData.ParcelFlags;
                parcel.SalePrice = properties.ParcelData.SalePrice;
                parcel.Name = Helpers.FieldToString(properties.ParcelData.Name);
                parcel.Desc = Helpers.FieldToString(properties.ParcelData.Desc);
                parcel.MusicURL = Helpers.FieldToString(properties.ParcelData.MusicURL);
                parcel.MediaURL = Helpers.FieldToString(properties.ParcelData.MediaURL);
                parcel.MediaID = properties.ParcelData.MediaID;
                parcel.MediaAutoScale = properties.ParcelData.MediaAutoScale;
                parcel.GroupID = properties.ParcelData.GroupID;
                parcel.PassPrice = properties.ParcelData.PassPrice;
                parcel.PassHours = properties.ParcelData.PassHours;
                parcel.Category = properties.ParcelData.Category;
                parcel.AuthBuyerID = properties.ParcelData.AuthBuyerID;
                parcel.SnapshotID = properties.ParcelData.SnapshotID;
                parcel.UserLocation = properties.ParcelData.UserLocation;
                parcel.UserLookAt = properties.ParcelData.UserLookAt;
                parcel.LandingType = properties.ParcelData.LandingType;

                // Fire the callback
                try { OnParcelProperties(parcel); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }
    }
}
