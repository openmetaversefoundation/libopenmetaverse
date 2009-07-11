/*
 * Copyright (c) 2009, openmetaverse.org
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
using System.Net;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Interfaces;

namespace OpenMetaverse.Messages.Linden
{
    #region Teleport/Region/Movement Messages

    /// <summary>
    /// Sent to the client to indicate a teleport request has completed
    /// </summary>
    public class TeleportFinishMessage : IMessage
    {
        /// <summary>The <see cref="UUID"/> of the agent</summary>
        public UUID AgentID;
        /// <summary></summary>
        public int LocationID;
        /// <summary>The simulators handle the agent teleported to</summary>
        public ulong RegionHandle;
        /// <summary>A Uri which contains a list of Capabilities the simulator supports</summary>
        public Uri SeedCapability;
        /// <summary>Indicates the level of access required
        /// to access the simulator, or the content rating, or the simulators 
        /// map status</summary>
        public SimAccess SimAccess;
        /// <summary>The IP Address of the simulator</summary>
        public IPAddress IP;
        /// <summary>The UDP Port the simulator will listen for UDP traffic on</summary>
        public int Port;
        /// <summary>Status flags indicating the state of the Agent upon arrival, Flying, etc.</summary>
        public TeleportFlags Flags;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);

            OSDArray infoArray = new OSDArray(1);

            OSDMap info = new OSDMap(8);
            info.Add("AgentID", OSD.FromUUID(AgentID));
            info.Add("LocationID", OSD.FromInteger(LocationID)); // Unused by the client
            info.Add("RegionHandle", OSD.FromULong(RegionHandle));
            info.Add("SeedCapability", OSD.FromUri(SeedCapability));
            info.Add("SimAccess", OSD.FromInteger((byte)SimAccess));
            info.Add("SimIP", MessageUtils.FromIP(IP));
            info.Add("SimPort", OSD.FromInteger(Port));
            info.Add("TeleportFlags", OSD.FromUInteger((uint)Flags));

            infoArray.Add(info);

            map.Add("Info", infoArray);

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            OSDArray array = (OSDArray)map["Info"];
            OSDMap blockMap = (OSDMap)array[0];

            AgentID = blockMap["AgentID"].AsUUID();
            LocationID = blockMap["LocationID"].AsInteger();
            RegionHandle = blockMap["RegionHandle"].AsULong();
            SeedCapability = blockMap["SeedCapability"].AsUri();
            SimAccess = (SimAccess)blockMap["SimAccess"].AsInteger();
            IP = MessageUtils.ToIP(blockMap["SimIP"]);
            Port = blockMap["SimPort"].AsInteger();
            Flags = (TeleportFlags)blockMap["TeleportFlags"].AsUInteger();
        }
    }

    /// <summary>
    /// Sent to the viewer when a neighboring simulator is requesting the agent make a connection to it.
    /// </summary>
    public class EstablishAgentCommunicationMessage : IMessage
    {
        public UUID AgentID;
        public IPAddress Address;
        public int Port;
        public Uri SeedCapability;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);
            map["agent-id"] = OSD.FromUUID(AgentID);
            map["sim-ip-and-port"] = OSD.FromString(String.Format("{0}:{1}", Address, Port));
            map["seed-capability"] = OSD.FromUri(SeedCapability);
            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            string ipAndPort = map["sim-ip-and-port"].AsString();
            int i = ipAndPort.IndexOf(':');

            AgentID = map["agent-id"].AsUUID();
            Address = IPAddress.Parse(ipAndPort.Substring(0, i));
            Port = Int32.Parse(ipAndPort.Substring(i + 1));
            SeedCapability = map["seed-capability"].AsUri();
        }
    }

    public class CrossedRegionMessage : IMessage
    {
        public Vector3 LookAt;
        public Vector3 Position;
        public UUID AgentID;
        public UUID SessionID;
        public ulong RegionHandle;
        public Uri SeedCapability;
        public IPAddress IP;
        public int Port;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);

            OSDArray infoArray = new OSDArray(1);
            OSDMap infoMap = new OSDMap(2);
            infoMap["LookAt"] = OSD.FromVector3(LookAt);
            infoMap["Position"] = OSD.FromVector3(Position);
            infoArray.Add(infoMap);
            map["Info"] = infoArray;

            OSDArray agentDataArray = new OSDArray(1);
            OSDMap agentDataMap = new OSDMap(2);
            agentDataMap["AgentID"] = OSD.FromUUID(AgentID);
            agentDataMap["SessionID"] = OSD.FromUUID(SessionID);
            agentDataArray.Add(agentDataMap);
            map["AgentData"] = agentDataArray;

            OSDArray regionDataArray = new OSDArray(1);
            OSDMap regionDataMap = new OSDMap(4);
            regionDataMap["RegionHandle"] = OSD.FromULong(RegionHandle);
            regionDataMap["SeedCapability"] = OSD.FromUri(SeedCapability);
            regionDataMap["SimIP"] = MessageUtils.FromIP(IP);
            regionDataMap["SimPort"] = OSD.FromInteger(Port);
            regionDataArray.Add(regionDataMap);
            map["RegionData"] = regionDataArray;

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            OSDMap infoMap = (OSDMap)((OSDArray)map["Info"])[0];
            LookAt = infoMap["LookAt"].AsVector3();
            Position = infoMap["Position"].AsVector3();

            OSDMap agentDataMap = (OSDMap)((OSDArray)map["AgentData"])[0];
            AgentID = agentDataMap["AgentID"].AsUUID();
            SessionID = agentDataMap["SessionID"].AsUUID();

            OSDMap regionDataMap = (OSDMap)((OSDArray)map["RegionData"])[0];
            RegionHandle = regionDataMap["RegionHandle"].AsULong();
            SeedCapability = regionDataMap["SeedCapability"].AsUri();
            IP = MessageUtils.ToIP(regionDataMap["SimIP"]);
            Port = regionDataMap["SimPort"].AsInteger();
        }
    }

    public class EnableSimulatorMessage : IMessage
    {
        public class SimulatorInfoBlock
        {
            public ulong RegionHandle;
            public IPAddress IP;
            public int Port;
        }

        public SimulatorInfoBlock[] Simulators;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);

            OSDArray array = new OSDArray(Simulators.Length);
            for (int i = 0; i < Simulators.Length; i++)
            {
                SimulatorInfoBlock block = Simulators[i];

                OSDMap blockMap = new OSDMap(3);
                blockMap["Handle"] = OSD.FromULong(block.RegionHandle);
                blockMap["IP"] = MessageUtils.FromIP(block.IP);
                blockMap["Port"] = OSD.FromInteger(block.Port);
                array.Add(blockMap);
            }

            map["SimulatorInfo"] = array;
            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            OSDArray array = (OSDArray)map["SimulatorInfo"];
            Simulators = new SimulatorInfoBlock[array.Count];

            for (int i = 0; i < array.Count; i++)
            {
                OSDMap blockMap = (OSDMap)array[i];

                SimulatorInfoBlock block = new SimulatorInfoBlock();
                block.RegionHandle = blockMap["Handle"].AsULong();
                block.IP = MessageUtils.ToIP(blockMap["IP"]);
                block.Port = blockMap["Port"].AsInteger();
                Simulators[i] = block;
            }
        }
    }

    /// <summary>
    /// A message sent to the client which indicates a teleport request has failed
    /// and contains some information on why it failed
    /// </summary>
    public class TeleportFailedMessage : IMessage
    {
        /// <summary></summary>
        public string ExtraParams;
        /// <summary>A string key of the reason the teleport failed e.g. CouldntTPCloser
        /// Which could be used to look up a value in a dictionary or enum</summary>
        public string MessageKey;
        /// <summary>The <see cref="UUID"/> of the Agent</summary>
        public UUID AgentID;
        /// <summary>A string human readable message containing the reason </summary>
        /// <remarks>An example: Could not teleport closer to destination</remarks>
        public string Reason;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);

            OSDMap alertInfoMap = new OSDMap(2);

            alertInfoMap["ExtraParams"] = OSD.FromString(ExtraParams);
            alertInfoMap["Message"] = OSD.FromString(MessageKey);
            OSDArray alertArray = new OSDArray();
            alertArray.Add(alertInfoMap);
            map["AlertInfo"] = alertArray;

            OSDMap infoMap = new OSDMap(2);
            infoMap["AgentID"] = OSD.FromUUID(AgentID);
            infoMap["Reason"] = OSD.FromString(Reason);
            OSDArray infoArray = new OSDArray();
            infoArray.Add(infoMap);
            map["Info"] = infoArray;

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {

            OSDArray alertInfoArray = (OSDArray)map["AlertInfo"];

            OSDMap alertInfoMap = (OSDMap)alertInfoArray[0];
            ExtraParams = alertInfoMap["ExtraParams"].AsString();
            MessageKey = alertInfoMap["Message"].AsString();

            OSDArray infoArray = (OSDArray)map["Info"];
            OSDMap infoMap = (OSDMap)infoArray[0];
            AgentID = infoMap["AgentID"].AsUUID();
            Reason = infoMap["Reason"].AsString();
        }
    }

    public class LandStatReplyMessage : IMessage
    {
        public int ReporType;
        public int RequestFlags;
        public int TotalObjectCount;

        public class ReportDataBlock
        {
            public Vector3 Location;
            public string OwnerName;
            public float Score;
            public UUID TaskID;
            public uint TaskLocalID;
            public string TaskName;
            public float MonoScore;
            public DateTime TimeStamp;
        }

        public ReportDataBlock[] ReportDataBlocks;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);

            OSDMap requestDataMap = new OSDMap(3);
            requestDataMap["ReportType"] = OSD.FromInteger(this.ReporType);
            requestDataMap["RequestFlags"] = OSD.FromInteger(this.RequestFlags);
            requestDataMap["TotalObjectCount"] = OSD.FromInteger(this.TotalObjectCount);

            OSDArray requestDatArray = new OSDArray();
            requestDatArray.Add(requestDataMap);
            map["RequestData"] = requestDatArray;

            OSDArray reportDataArray = new OSDArray();
            OSDArray dataExtendedArray = new OSDArray();
            for (int i = 0; i < ReportDataBlocks.Length; i++)
            {
                OSDMap reportMap = new OSDMap(8);
                reportMap["LocationX"] = OSD.FromReal(ReportDataBlocks[i].Location.X);
                reportMap["LocationY"] = OSD.FromReal(ReportDataBlocks[i].Location.Y);
                reportMap["LocationZ"] = OSD.FromReal(ReportDataBlocks[i].Location.Z);
                reportMap["OwnerName"] = OSD.FromString(ReportDataBlocks[i].OwnerName);
                reportMap["Score"] = OSD.FromReal(ReportDataBlocks[i].Score);
                reportMap["TaskID"] = OSD.FromUUID(ReportDataBlocks[i].TaskID);
                reportMap["TaskLocalID"] = OSD.FromReal(ReportDataBlocks[i].TaskLocalID);
                reportMap["TaskName"] = OSD.FromString(ReportDataBlocks[i].TaskName);
                reportDataArray.Add(reportMap);

                OSDMap extendedMap = new OSDMap(2);
                extendedMap["MonoScore"] = OSD.FromReal(ReportDataBlocks[i].MonoScore);
                extendedMap["TimeStamp"] = OSD.FromDate(ReportDataBlocks[i].TimeStamp);
                dataExtendedArray.Add(extendedMap);
            }

            map["ReportData"] = reportDataArray;
            map["DataExtended"] = dataExtendedArray;

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {

            OSDArray requestDataArray = (OSDArray)map["RequestData"];
            OSDMap requestMap = (OSDMap)requestDataArray[0];

            this.ReporType = requestMap["ReportType"].AsInteger();
            this.RequestFlags = requestMap["RequestFlags"].AsInteger();
            this.TotalObjectCount = requestMap["TotalObjectCount"].AsInteger();

            OSDArray dataArray = (OSDArray)map["ReportData"];
            OSDArray dataExtendedArray = (OSDArray)map["DataExtended"];

            ReportDataBlocks = new ReportDataBlock[dataArray.Count];
            for (int i = 0; i < dataArray.Count; i++)
            {
                OSDMap blockMap = (OSDMap)dataArray[i];
                OSDMap extMap = (OSDMap)dataExtendedArray[i];
                ReportDataBlock block = new ReportDataBlock();
                block.Location = new Vector3(
                    (float)blockMap["LocationX"].AsReal(),
                    (float)blockMap["LocationY"].AsReal(),
                    (float)blockMap["LocationZ"].AsReal());
                block.OwnerName = blockMap["OwnerName"].AsString();
                block.Score = (float)blockMap["Score"].AsReal();
                block.TaskID = blockMap["TaskID"].AsUUID();
                block.TaskLocalID = blockMap["TaskLocalID"].AsUInteger();
                block.TaskName = blockMap["TaskName"].AsString();
                block.MonoScore = (float)extMap["MonoScore"].AsReal();
                block.TimeStamp = Utils.UnixTimeToDateTime(extMap["TimeStamp"].AsUInteger());

                ReportDataBlocks[i] = block;
            }
        }
    }

    #endregion

    #region Parcel Messages

    /// <summary>
    /// Contains a list of prim owner information for a specific parcel in a simulator
    /// </summary>
    /// <remarks>
    /// A Simulator will always return at least 1 entry
    /// If agent does not have proper permission the OwnerID will be UUID.Zero
    /// If agent does not have proper permission OR there are no primitives on parcel
    /// the DataBlocksExtended map will not be sent from the simulator
    /// </remarks>
    public class ParcelObjectOwnersReplyMessage : IMessage
    {
        /// <summary>
        /// Prim ownership information for a specified owner on a single parcel
        /// </summary>
        public class PrimOwner
        {
            /// <summary>The <see cref="UUID"/> of the prim owner, 
            /// UUID.Zero if agent has no permission to view prim owner information</summary>
            public UUID OwnerID;
            /// <summary>The total number of prims</summary>
            public int Count;
            /// <summary>True if the OwnerID is a <see cref="Group"/></summary>
            public bool IsGroupOwned;
            /// <summary>True if the owner is online 
            /// <remarks>This is no longer used by the LL Simulators</remarks></summary>
            public bool OnlineStatus;
            /// <summary>The date the most recent prim was rezzed</summary>
            public DateTime TimeStamp;
        }

        /// <summary>An Array of <see cref="PrimOwner"/> objects</summary>
        public PrimOwner[] PrimOwnersBlock;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDArray dataArray = new OSDArray(PrimOwnersBlock.Length);
            OSDArray dataExtendedArray = new OSDArray();

            for (int i = 0; i < PrimOwnersBlock.Length; i++)
            {
                OSDMap dataMap = new OSDMap(4);
                dataMap["OwnerID"] = OSD.FromUUID(PrimOwnersBlock[i].OwnerID);
                dataMap["Count"] = OSD.FromInteger(PrimOwnersBlock[i].Count);
                dataMap["IsGroupOwned"] = OSD.FromBoolean(PrimOwnersBlock[i].IsGroupOwned);
                dataMap["OnlineStatus"] = OSD.FromBoolean(PrimOwnersBlock[i].OnlineStatus);
                dataArray.Add(dataMap);

                OSDMap dataExtendedMap = new OSDMap(1);
                dataExtendedMap["TimeStamp"] = OSD.FromDate(PrimOwnersBlock[i].TimeStamp);
                dataExtendedArray.Add(dataExtendedMap);
            }

            OSDMap map = new OSDMap();
            map.Add("Data", dataArray);
            if (dataExtendedArray.Count > 0)
                map.Add("DataExtended", dataExtendedArray);

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            OSDArray dataArray = (OSDArray)map["Data"];

            // DataExtended is optional, will not exist of parcel contains zero prims
            OSDArray dataExtendedArray;
            if (map.ContainsKey("DataExtended"))
            {
                dataExtendedArray = (OSDArray)map["DataExtended"];
            }
            else
            {
                dataExtendedArray = new OSDArray();
            }

            PrimOwnersBlock = new PrimOwner[dataArray.Count];

            for (int i = 0; i < dataArray.Count; i++)
            {
                OSDMap dataMap = (OSDMap)dataArray[i];
                PrimOwner block = new PrimOwner();
                block.OwnerID = dataMap["OwnerID"].AsUUID();
                block.Count = dataMap["Count"].AsInteger();
                block.IsGroupOwned = dataMap["IsGroupOwned"].AsBoolean();
                block.OnlineStatus = dataMap["OnlineStatus"].AsBoolean(); // deprecated

                /* if the agent has no permissions, or there are no prims, the counts
                 * should not match up, so we don't decode the DataExtended map */
                if (dataExtendedArray.Count == dataArray.Count)
                {
                    OSDMap dataExtendedMap = (OSDMap)dataExtendedArray[i];
                    block.TimeStamp = Utils.UnixTimeToDateTime(dataExtendedMap["TimeStamp"].AsUInteger());
                }

                PrimOwnersBlock[i] = block;
            }
        }
    }

    /// <summary>
    /// The details of a single parcel in a region, also contains some regionwide globals
    /// </summary>
    [Serializable]
    public class ParcelPropertiesMessage : IMessage
    {
        /// <summary>Simulator-local ID of this parcel</summary>
        public int LocalID;
        /// <summary>Maximum corner of the axis-aligned bounding box for this
        /// parcel</summary>
        public Vector3 AABBMax;
        /// <summary>Minimum corner of the axis-aligned bounding box for this
        /// parcel</summary>
        public Vector3 AABBMin;
        /// <summary>Total parcel land area</summary>
        public int Area;
        /// <summary></summary>
        public uint AuctionID;
        /// <summary>Key of authorized buyer</summary>
        public UUID AuthBuyerID;
        /// <summary>Bitmap describing land layout in 4x4m squares across the 
        /// entire region</summary>
        public byte[] Bitmap;
        /// <summary></summary>
        public ParcelCategory Category;
        /// <summary>Date land was claimed</summary>
        public DateTime ClaimDate;
        /// <summary>Appears to always be zero</summary>
        public int ClaimPrice;
        /// <summary>Parcel Description</summary>
        public string Desc;
        /// <summary></summary>
        public ParcelFlags ParcelFlags;
        /// <summary></summary>
        public UUID GroupID;
        /// <summary>Total number of primitives owned by the parcel group on 
        /// this parcel</summary>
        public int GroupPrims;
        /// <summary>Whether the land is deeded to a group or not</summary>
        public bool IsGroupOwned;
        /// <summary></summary>
        public LandingType LandingType;
        /// <summary>Maximum number of primitives this parcel supports</summary>
        public int MaxPrims;
        /// <summary>The Asset UUID of the Texture which when applied to a 
        /// primitive will display the media</summary>
        public UUID MediaID;
        /// <summary>A URL which points to any Quicktime supported media type</summary>
        public string MediaURL;
        /// <summary>A byte, if 0x1 viewer should auto scale media to fit object</summary>
        public bool MediaAutoScale;
        /// <summary>URL For Music Stream</summary>
        public string MusicURL;
        /// <summary>Parcel Name</summary>
        public string Name;
        /// <summary>Autoreturn value in minutes for others' objects</summary>
        public int OtherCleanTime;
        /// <summary></summary>
        public int OtherCount;
        /// <summary>Total number of other primitives on this parcel</summary>
        public int OtherPrims;
        /// <summary>UUID of the owner of this parcel</summary>
        public UUID OwnerID;
        /// <summary>Total number of primitives owned by the parcel owner on 
        /// this parcel</summary>
        public int OwnerPrims;
        /// <summary></summary>
        public float ParcelPrimBonus;
        /// <summary>How long is pass valid for</summary>
        public float PassHours;
        /// <summary>Price for a temporary pass</summary>
        public int PassPrice;
        /// <summary></summary>
        public int PublicCount;
        /// <summary></summary>
        public bool RegionDenyAnonymous;
        /// <summary></summary>
        public bool RegionPushOverride;
        /// <summary>This field is no longer used</summary>
        public int RentPrice;
        /// The result of a request for parcel properties
        public ParcelResult RequestResult;
        /// <summary>Sale price of the parcel, only useful if ForSale is set</summary>
        /// <remarks>The SalePrice will remain the same after an ownership
        /// transfer (sale), so it can be used to see the purchase price after
        /// a sale if the new owner has not changed it</remarks>
        public int SalePrice;
        /// <summary>
        /// Number of primitives your avatar is currently
        /// selecting and sitting on in this parcel
        /// </summary>
        public int SelectedPrims;
        /// <summary></summary>
        public int SelfCount;
        /// <summary>
        /// A number which increments by 1, starting at 0 for each ParcelProperties request. 
        /// Can be overriden by specifying the sequenceID with the ParcelPropertiesRequest being sent. 
        /// a Negative number indicates the action in <seealso cref="ParcelPropertiesStatus"/> has occurred. 
        /// </summary>
        public int SequenceID;
        /// <summary>Maximum primitives across the entire simulator</summary>
        public int SimWideMaxPrims;
        /// <summary>Total primitives across the entire simulator</summary>
        public int SimWideTotalPrims;
        /// <summary></summary>
        public bool SnapSelection;
        /// <summary>Key of parcel snapshot</summary>
        public UUID SnapshotID;
        /// <summary>Parcel ownership status</summary>
        public ParcelStatus Status;
        /// <summary>Total number of primitives on this parcel</summary>
        public int TotalPrims;
        /// <summary></summary>
        public Vector3 UserLocation;
        /// <summary></summary>
        public Vector3 UserLookAt;
        /// <summary>TRUE of region denies access to age unverified users</summary>
        public bool RegionDenyAgeUnverified;
        /// <summary>A description of the media</summary>
        public string MediaDesc;
        /// <summary>An Integer which represents the height of the media</summary>
        public int MediaHeight;
        /// <summary>An integer which represents the width of the media</summary>
        public int MediaWidth;
        /// <summary>A boolean, if true the viewer should loop the media</summary>
        public bool MediaLoop;
        /// <summary>A string which contains the mime type of the media</summary>
        public string MediaType;
        /// <summary>true to obscure (hide) media url</summary>
        public bool ObscureMedia;
        /// <summary>true to obscure (hide) music url</summary>
        public bool ObscureMusic;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);

            OSDArray dataArray = new OSDArray(1);
            OSDMap parcelDataMap = new OSDMap(47);
            parcelDataMap["LocalID"] = OSD.FromInteger(LocalID);
            parcelDataMap["AABBMax"] = OSD.FromVector3(AABBMax);
            parcelDataMap["AABBMin"] = OSD.FromVector3(AABBMin);
            parcelDataMap["Area"] = OSD.FromInteger(Area);
            parcelDataMap["AuctionID"] = OSD.FromInteger(AuctionID);
            parcelDataMap["AuthBuyerID"] = OSD.FromUUID(AuthBuyerID);
            parcelDataMap["Bitmap"] = OSD.FromBinary(Bitmap);
            parcelDataMap["Category"] = OSD.FromInteger((int)Category);
            parcelDataMap["ClaimDate"] = OSD.FromDate(ClaimDate);
            parcelDataMap["ClaimPrice"] = OSD.FromInteger(ClaimPrice);
            parcelDataMap["Desc"] = OSD.FromString(Desc);
            parcelDataMap["ParcelFlags"] = OSD.FromUInteger((uint)ParcelFlags);
            parcelDataMap["GroupID"] = OSD.FromUUID(GroupID);
            parcelDataMap["GroupPrims"] = OSD.FromInteger(GroupPrims);
            parcelDataMap["IsGroupOwned"] = OSD.FromBoolean(IsGroupOwned);
            parcelDataMap["LandingType"] = OSD.FromInteger((int)LandingType);
            parcelDataMap["MaxPrims"] = OSD.FromInteger(MaxPrims);
            parcelDataMap["MediaID"] = OSD.FromUUID(MediaID);
            parcelDataMap["MediaURL"] = OSD.FromString(MediaURL);
            parcelDataMap["MediaAutoScale"] = OSD.FromBoolean(MediaAutoScale);
            parcelDataMap["MusicURL"] = OSD.FromString(MusicURL);
            parcelDataMap["Name"] = OSD.FromString(Name);
            parcelDataMap["OtherCleanTime"] = OSD.FromInteger(OtherCleanTime);
            parcelDataMap["OtherCount"] = OSD.FromInteger(OtherCount);
            parcelDataMap["OtherPrims"] = OSD.FromInteger(OtherPrims);
            parcelDataMap["OwnerID"] = OSD.FromUUID(OwnerID);
            parcelDataMap["OwnerPrims"] = OSD.FromInteger(OwnerPrims);
            parcelDataMap["ParcelPrimBonus"] = OSD.FromReal((float)ParcelPrimBonus);
            parcelDataMap["PassHours"] = OSD.FromReal((float)PassHours);
            parcelDataMap["PassPrice"] = OSD.FromInteger(PassPrice);
            parcelDataMap["PublicCount"] = OSD.FromInteger(PublicCount);
            parcelDataMap["RegionDenyAnonymous"] = OSD.FromBoolean(RegionDenyAnonymous);
            parcelDataMap["RegionPushOverride"] = OSD.FromBoolean(RegionPushOverride);
            parcelDataMap["RentPrice"] = OSD.FromInteger(RentPrice);
            parcelDataMap["RequestResult"] = OSD.FromInteger((int)RequestResult);
            parcelDataMap["SalePrice"] = OSD.FromInteger(SalePrice);
            parcelDataMap["SelectedPrims"] = OSD.FromInteger(SelectedPrims);
            parcelDataMap["SelfCount"] = OSD.FromInteger(SelfCount);
            parcelDataMap["SequenceID"] = OSD.FromInteger(SequenceID);
            parcelDataMap["SimWideMaxPrims"] = OSD.FromInteger(SimWideMaxPrims);
            parcelDataMap["SimWideTotalPrims"] = OSD.FromInteger(SimWideTotalPrims);
            parcelDataMap["SnapSelection"] = OSD.FromBoolean(SnapSelection);
            parcelDataMap["SnapshotID"] = OSD.FromUUID(SnapshotID);
            parcelDataMap["Status"] = OSD.FromInteger((int)Status);
            parcelDataMap["TotalPrims"] = OSD.FromInteger(TotalPrims);
            parcelDataMap["UserLocation"] = OSD.FromVector3(UserLocation);
            parcelDataMap["UserLookAt"] = OSD.FromVector3(UserLookAt);
            dataArray.Add(parcelDataMap);
            map["ParcelData"] = dataArray;

            OSDArray mediaDataArray = new OSDArray(1);
            OSDMap mediaDataMap = new OSDMap(7);
            mediaDataMap["MediaDesc"] = OSD.FromString(MediaDesc);
            mediaDataMap["MediaHeight"] = OSD.FromInteger(MediaHeight);
            mediaDataMap["MediaWidth"] = OSD.FromInteger(MediaWidth);
            mediaDataMap["MediaLoop"] = OSD.FromBoolean(MediaLoop);
            mediaDataMap["MediaType"] = OSD.FromString(MediaType);
            mediaDataMap["ObscureMedia"] = OSD.FromBoolean(ObscureMedia);
            mediaDataMap["ObscureMusic"] = OSD.FromBoolean(ObscureMusic);
            mediaDataArray.Add(mediaDataMap);
            map["MediaData"] = mediaDataArray;

            OSDArray ageVerificationBlockArray = new OSDArray(1);
            OSDMap ageVerificationBlockMap = new OSDMap(1);
            ageVerificationBlockMap["RegionDenyAgeUnverified"] = OSD.FromBoolean(RegionDenyAgeUnverified);
            ageVerificationBlockArray.Add(ageVerificationBlockMap);
            map["AgeVerificationBlock"] = ageVerificationBlockArray;

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            OSDMap parcelDataMap = (OSDMap)((OSDArray)map["ParcelData"])[0];
            LocalID = parcelDataMap["LocalID"].AsInteger();
            AABBMax = parcelDataMap["AABBMax"].AsVector3();
            AABBMin = parcelDataMap["AABBMin"].AsVector3();
            Area = parcelDataMap["Area"].AsInteger();
            AuctionID = (uint)parcelDataMap["AuctionID"].AsInteger();
            AuthBuyerID = parcelDataMap["AuthBuyerID"].AsUUID();
            Bitmap = parcelDataMap["Bitmap"].AsBinary();
            Category = (ParcelCategory)parcelDataMap["Category"].AsInteger();
            ClaimDate = Utils.UnixTimeToDateTime((uint)parcelDataMap["ClaimDate"].AsInteger());
            ClaimPrice = parcelDataMap["ClaimPrice"].AsInteger();
            Desc = parcelDataMap["Desc"].AsString();

            // LL sends this as binary, we'll convert it here
            if (parcelDataMap["ParcelFlags"].Type == OSDType.Binary)
            {
                byte[] bytes = parcelDataMap["ParcelFlags"].AsBinary();
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);
                ParcelFlags = (ParcelFlags)BitConverter.ToUInt32(bytes, 0);
            }
            else
            {
                ParcelFlags = (ParcelFlags)parcelDataMap["ParcelFlags"].AsUInteger();
            }
            GroupID = parcelDataMap["GroupID"].AsUUID();
            GroupPrims = parcelDataMap["GroupPrims"].AsInteger();
            IsGroupOwned = parcelDataMap["IsGroupOwned"].AsBoolean();
            LandingType = (LandingType)parcelDataMap["LandingType"].AsInteger();
            MaxPrims = parcelDataMap["MaxPrims"].AsInteger();
            MediaID = parcelDataMap["MediaID"].AsUUID();
            MediaURL = parcelDataMap["MediaURL"].AsString();
            MediaAutoScale = parcelDataMap["MediaAutoScale"].AsBoolean(); // 0x1 = yes
            MusicURL = parcelDataMap["MusicURL"].AsString();
            Name = parcelDataMap["Name"].AsString();
            OtherCleanTime = parcelDataMap["OtherCleanTime"].AsInteger();
            OtherCount = parcelDataMap["OtherCount"].AsInteger();
            OtherPrims = parcelDataMap["OtherPrims"].AsInteger();
            OwnerID = parcelDataMap["OwnerID"].AsUUID();
            OwnerPrims = parcelDataMap["OwnerPrims"].AsInteger();
            ParcelPrimBonus = (float)parcelDataMap["ParcelPrimBonus"].AsReal();
            PassHours = (float)parcelDataMap["PassHours"].AsReal();
            PassPrice = parcelDataMap["PassPrice"].AsInteger();
            PublicCount = parcelDataMap["PublicCount"].AsInteger();
            RegionDenyAnonymous = parcelDataMap["RegionDenyAnonymous"].AsBoolean();
            RegionPushOverride = parcelDataMap["RegionPushOverride"].AsBoolean();
            RentPrice = parcelDataMap["RentPrice"].AsInteger();
            RequestResult = (ParcelResult)parcelDataMap["RequestResult"].AsInteger();
            SalePrice = parcelDataMap["SalePrice"].AsInteger();
            SelectedPrims = parcelDataMap["SelectedPrims"].AsInteger();
            SelfCount = parcelDataMap["SelfCount"].AsInteger();
            SequenceID = parcelDataMap["SequenceID"].AsInteger();
            SimWideMaxPrims = parcelDataMap["SimWideMaxPrims"].AsInteger();
            SimWideTotalPrims = parcelDataMap["SimWideTotalPrims"].AsInteger();
            SnapSelection = parcelDataMap["SnapSelection"].AsBoolean();
            SnapshotID = parcelDataMap["SnapshotID"].AsUUID();
            Status = (ParcelStatus)parcelDataMap["Status"].AsInteger();
            TotalPrims = parcelDataMap["TotalPrims"].AsInteger();
            UserLocation = parcelDataMap["UserLocation"].AsVector3();
            UserLookAt = parcelDataMap["UserLookAt"].AsVector3();

            if (map.ContainsKey("MediaData")) // temporary, OpenSim doesn't send this block
            {
                OSDMap mediaDataMap = (OSDMap)((OSDArray)map["MediaData"])[0];
                MediaDesc = mediaDataMap["MediaDesc"].AsString();
                MediaHeight = mediaDataMap["MediaHeight"].AsInteger();
                MediaWidth = mediaDataMap["MediaWidth"].AsInteger();
                MediaLoop = mediaDataMap["MediaLoop"].AsBoolean();
                MediaType = mediaDataMap["MediaType"].AsString();
                ObscureMedia = mediaDataMap["ObscureMedia"].AsBoolean();
                ObscureMusic = mediaDataMap["ObscureMusic"].AsBoolean();
            }

            OSDMap ageVerificationBlockMap = (OSDMap)((OSDArray)map["AgeVerificationBlock"])[0];
            RegionDenyAgeUnverified = ageVerificationBlockMap["RegionDenyAgeUnverified"].AsBoolean();
        }
    }

    /// <summary>A message sent from the viewer to the simulator to updated a specific parcels settings</summary>
    public class ParcelPropertiesUpdateMessage : IMessage
    {
        /// <summary>The <seealso cref="UUID"/> of the agent authorized to purchase this
        /// parcel of land or a NULL <seealso cref="UUID"/> if the sale is authorized to anyone</summary>
        public UUID AuthBuyerID;
        /// <summary>true to enable auto scaling of the parcel media</summary>
        public bool MediaAutoScale;
        /// <summary>The category of this parcel used when search is enabled to restrict
        /// search results</summary>
        public ParcelCategory Category;
        /// <summary>A string containing the description to set</summary>
        public string Desc;
        /// <summary>The <seealso cref="UUID"/> of the <seealso cref="Group"/> which allows for additional
        /// powers and restrictions.</summary>
        public UUID GroupID;
        /// <summary>The <seealso cref="LandingType"/> which specifies how avatars which teleport
        /// to this parcel are handled</summary>
        public LandingType Landing;
        /// <summary>The LocalID of the parcel to update settings on</summary>
        public int LocalID;
        /// <summary>A string containing the description of the media which can be played
        /// to visitors</summary>
        public string MediaDesc;
        /// <summary></summary>
        public int MediaHeight;
        /// <summary></summary>
        public bool MediaLoop;
        /// <summary></summary>
        public UUID MediaID;
        /// <summary></summary>
        public string MediaType;
        /// <summary></summary>
        public string MediaURL;
        /// <summary></summary>
        public int MediaWidth;
        /// <summary></summary>
        public string MusicURL;
        /// <summary></summary>
        public string Name;
        /// <summary></summary>
        public bool ObscureMedia;
        /// <summary></summary>
        public bool ObscureMusic;
        /// <summary></summary>
        public ParcelFlags ParcelFlags;
        /// <summary></summary>
        public float PassHours;
        /// <summary></summary>
        public uint PassPrice;
        /// <summary></summary>
        public uint SalePrice;
        /// <summary></summary>
        public UUID SnapshotID;
        /// <summary></summary>
        public Vector3 UserLocation;
        /// <summary></summary>
        public Vector3 UserLookAt;

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            AuthBuyerID = map["auth_buyer_id"].AsUUID();
            MediaAutoScale = map["auto_scale"].AsBoolean();
            Category = (ParcelCategory)map["category"].AsInteger();
            Desc = map["description"].AsString();
            GroupID = map["group_id"].AsUUID();
            Landing = (LandingType)map["landing_type"].AsUInteger();
            LocalID = map["local_id"].AsInteger();
            MediaDesc = map["media_desc"].AsString();
            MediaHeight = map["media_height"].AsInteger();
            MediaLoop = map["media_loop"].AsBoolean();
            MediaID = map["media_id"].AsUUID();
            MediaType = map["media_type"].AsString();
            MediaURL = map["media_url"].AsString();
            MediaWidth = map["media_width"].AsInteger();
            MusicURL = map["music_url"].AsString();
            Name = map["name"].AsString();
            ObscureMedia = map["obscure_media"].AsBoolean();
            ObscureMusic = map["obscure_music"].AsBoolean();
            ParcelFlags = (ParcelFlags)map["parcel_flags"].AsUInteger();
            PassHours = (float)map["pass_hours"].AsReal();
            PassPrice = map["pass_price"].AsUInteger();
            SalePrice = map["sale_price"].AsUInteger();
            SnapshotID = map["snapshot_id"].AsUUID();
            UserLocation = map["user_location"].AsVector3();
            UserLookAt = map["user_look_at"].AsVector3();
        }

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["auth_buyer_id"] = OSD.FromUUID(AuthBuyerID);
            map["auto_scale"] = OSD.FromBoolean(MediaAutoScale);
            map["category"] = OSD.FromInteger((byte)Category);
            map["description"] = OSD.FromString(Desc);
            map["flags"] = OSD.FromBinary(Utils.EmptyBytes);
            map["group_id"] = OSD.FromUUID(GroupID);
            map["landing_type"] = OSD.FromInteger((byte)Landing);
            map["local_id"] = OSD.FromInteger(LocalID);
            map["media_desc"] = OSD.FromString(MediaDesc);
            map["media_height"] = OSD.FromInteger(MediaHeight);
            map["media_id"] = OSD.FromUUID(MediaID);
            map["media_loop"] = OSD.FromBoolean(MediaLoop);
            map["media_type"] = OSD.FromString(MediaType);
            map["media_url"] = OSD.FromString(MediaURL);
            map["media_width"] = OSD.FromInteger(MediaWidth);
            map["music_url"] = OSD.FromString(MusicURL);
            map["name"] = OSD.FromString(Name);
            map["obscure_media"] = OSD.FromBoolean(ObscureMedia);
            map["obscure_music"] = OSD.FromBoolean(ObscureMusic);
            map["parcel_flags"] = OSD.FromUInteger((uint)ParcelFlags);
            map["pass_hours"] = OSD.FromReal(PassHours);
            map["pass_price"] = OSD.FromInteger(PassPrice);
            map["sale_price"] = OSD.FromInteger(SalePrice);
            map["snapshot_id"] = OSD.FromUUID(SnapshotID);
            map["user_location"] = OSD.FromVector3(UserLocation);
            map["user_look_at"] = OSD.FromVector3(UserLookAt);

            return map;
        }
    }

    /// <summary>Base class used for the RemoteParcelRequest message</summary>
    public abstract class RemoteParcelRequestBlock
    {
        public abstract OSDMap Serialize();
        public abstract void Deserialize(OSDMap map);
    }

    /// <summary>
    /// A message sent from the viewer to the simulator to request information
    /// on a remote parcel
    /// </summary>
    public class RemoteParcelRequestRequest : RemoteParcelRequestBlock
    {
        /// <summary>Local sim position of the parcel we are looking up</summary>
        public Vector3 Location;
        /// <summary>Region handle of the parcel we are looking up</summary>
        public ulong RegionHandle;
        /// <summary>Region <see cref="UUID"/> of the parcel we are looking up</summary>
        public UUID RegionID;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public override OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);
            map["location"] = OSD.FromVector3(Location);
            map["region_handle"] = OSD.FromULong(RegionHandle);
            map["region_id"] = OSD.FromUUID(RegionID);
            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public override void Deserialize(OSDMap map)
        {
            Location = map["location"].AsVector3();
            RegionHandle = map["region_handle"].AsULong();
            RegionID = map["region_id"].AsUUID();
        }

    }

    /// <summary>
    /// A message sent from the simulator to the viewer in response to a <see cref="RemoteParcelRequestRequest"/> 
    /// which will contain parcel information
    /// </summary>
    public class RemoteParcelRequestReply : RemoteParcelRequestBlock
    {
        /// <summary>The grid-wide unique parcel ID</summary>
        public UUID ParcelID;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public override OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);
            map["parcel_id"] = OSD.FromUUID(ParcelID);
            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public override void Deserialize(OSDMap map)
        {
            ParcelID = map["parcel_id"].AsUUID();
        }

    }

    /// <summary>
    /// A message containing a request for a remote parcel from a viewer, or a response
    /// from the simulator to that request
    /// </summary>
    [Serializable]
    public class RemoteParcelRequestMessage : IMessage
    {
        /// <summary>The request or response details block</summary>
        public RemoteParcelRequestBlock Request;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            return Request.Serialize();
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            if (map.ContainsKey("parcel_id"))
                Request = new RemoteParcelRequestReply();
            else if (map.ContainsKey("location"))
                Request = new RemoteParcelRequestRequest();
            else
                Logger.Log("Unable to deserialize RemoteParcelRequest: No message handler exists for method ", Helpers.LogLevel.Warning);

            if(Request != null)
                Request.Deserialize(map);
        }
    }
    #endregion

    #region Inventory Messages

    public class NewFileAgentInventoryMessage
    {
        public UUID FolderID;
        public AssetType AssetType;
        public InventoryType InventoryType;
        public string Name;
        public string Description;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(5);
            map["folder_id"] = OSD.FromUUID(FolderID);
            map["asset_type"] = OSD.FromString(Utils.AssetTypeToString(AssetType));
            map["inventory_type"] = OSD.FromString(Utils.InventoryTypeToString(InventoryType));
            map["name"] = OSD.FromString(Name);
            map["description"] = OSD.FromString(Description);

            return map;

        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            FolderID = map["folder_id"].AsUUID();
            AssetType = Utils.StringToAssetType(map["asset_type"].AsString());
            InventoryType = Utils.StringToInventoryType(map["inventory_type"].AsString());
            Name = map["name"].AsString();
            Description = map["description"].AsString();
        }
    }

    #endregion

    #region Agent Messages

    /// <summary>
    /// A message sent from the simulator to an agent which contains
    /// the groups the agent is in
    /// </summary>
    public class AgentGroupDataUpdateMessage : IMessage
    {
        /// <summary>The Agent receiving the message</summary>
        public UUID AgentID;

        /// <summary>Group Details specific to the agent</summary>
        public class GroupData
        {
            /// <summary>true of the agent accepts group notices</summary>
            public bool AcceptNotices;
            /// <summary>The agents tier contribution to the group</summary>
            public int Contribution;
            /// <summary>The Groups <seealso cref="UUID"/></summary>
            public UUID GroupID;
            /// <summary>The <seealso cref="UUID"/> of the groups insignia</summary>
            public UUID GroupInsigniaID;
            /// <summary>The name of the group</summary>
            public string GroupName;
            /// <summary>The aggregate permissions the agent has in the group for all roles the agent
            /// is assigned</summary>
            public GroupPowers GroupPowers;
        }

        /// <summary>An optional block containing additional agent specific information</summary>
        public class NewGroupData
        {
            /// <summary>true of the agent allows this group to be
            /// listed in their profile</summary>
            public bool ListInProfile;
        }

        /// <summary>An array containing <seealso cref="GroupData"/> information
        /// for each <see cref="Group"/> the agent is a member of</summary>
        public GroupData[] GroupDataBlock;
        /// <summary>An array containing <seealso cref="NewGroupData"/> information
        /// for each <see cref="Group"/> the agent is a member of</summary>
        public NewGroupData[] NewGroupDataBlock;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);

            OSDMap agent = new OSDMap(1);
            agent["AgentID"] = OSD.FromUUID(AgentID);

            OSDArray agentArray = new OSDArray();
            agentArray.Add(agent);

            map["AgentData"] = agentArray;

            OSDArray groupDataArray = new OSDArray(GroupDataBlock.Length);

            for (int i = 0; i < GroupDataBlock.Length; i++)
            {
                OSDMap group = new OSDMap(6);
                group["AcceptNotices"] = OSD.FromBoolean(GroupDataBlock[i].AcceptNotices);
                group["Contribution"] = OSD.FromInteger(GroupDataBlock[i].Contribution);
                group["GroupID"] = OSD.FromUUID(GroupDataBlock[i].GroupID);
                group["GroupInsigniaID"] = OSD.FromUUID(GroupDataBlock[i].GroupInsigniaID);
                group["GroupName"] = OSD.FromString(GroupDataBlock[i].GroupName);
                group["GroupPowers"] = OSD.FromLong((long)GroupDataBlock[i].GroupPowers);
                groupDataArray.Add(group);
            }

            map["GroupData"] = groupDataArray;

            OSDArray newGroupDataArray = new OSDArray(NewGroupDataBlock.Length);

            for (int i = 0; i < NewGroupDataBlock.Length; i++)
            {
                OSDMap group = new OSDMap(1);
                group["ListInProfile"] = OSD.FromBoolean(NewGroupDataBlock[i].ListInProfile);
                newGroupDataArray.Add(group);
            }

            map["NewGroupData"] = newGroupDataArray;

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            OSDArray agentArray = (OSDArray)map["AgentData"];
            OSDMap agentMap = (OSDMap)agentArray[0];
            AgentID = agentMap["AgentID"].AsUUID();

            OSDArray groupArray = (OSDArray)map["GroupData"];

            GroupDataBlock = new GroupData[groupArray.Count];

            for (int i = 0; i < groupArray.Count; i++)
            {
                OSDMap groupMap = (OSDMap)groupArray[i];


                GroupData groupData = new GroupData();

                groupData.GroupID = groupMap["GroupID"].AsUUID();
                groupData.Contribution = groupMap["Contribution"].AsInteger();
                groupData.GroupInsigniaID = groupMap["GroupInsigniaID"].AsUUID();
                groupData.GroupName = groupMap["GroupName"].AsString();
                groupData.GroupPowers = (GroupPowers)groupMap["GroupPowers"].AsLong();
                groupData.AcceptNotices = groupMap["AcceptNotices"].AsBoolean();
                GroupDataBlock[i] = groupData;
            }

            // If request for current groups came very close to login
            // the Linden sim will not include the NewGroupData block, but
            // it will instead set all ListInProfile fields to false
            if (map.ContainsKey("NewGroupData"))
            {
                OSDArray newGroupArray = (OSDArray)map["NewGroupData"];

                NewGroupDataBlock = new NewGroupData[newGroupArray.Count];

                for (int i = 0; i < newGroupArray.Count; i++)
                {
                    OSDMap newGroupMap = (OSDMap)newGroupArray[i];
                    NewGroupData newGroupData = new NewGroupData();
                    newGroupData.ListInProfile = newGroupMap["ListInProfile"].AsBoolean();
                    NewGroupDataBlock[i] = newGroupData;
                }
            }
            else
            {
                NewGroupDataBlock = new NewGroupData[GroupDataBlock.Length];
                for (int i = 0; i < NewGroupDataBlock.Length; i++)
                {
                    NewGroupData newGroupData = new NewGroupData();
                    newGroupData.ListInProfile = false;
                    NewGroupDataBlock[i] = newGroupData;
                }
            }
        }
    }

    /// <summary>
    /// A message sent from the viewer to the simulator which 
    /// specifies the language and permissions for others to detect
    /// the language specified
    /// </summary>
    public class UpdateAgentLanguageMessage : IMessage
    {
        /// <summary>A string containng the default language 
        /// to use for the agent</summary>
        public string Language;
        /// <summary>true of others are allowed to
        /// know the language setting</summary>
        public bool LanguagePublic;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);

            map["language"] = OSD.FromString(Language);
            map["language_is_public"] = OSD.FromBoolean(LanguagePublic);

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            LanguagePublic = map["language_is_public"].AsBoolean();
            Language = map["language"].AsString();
        }
    }

    /// <summary>
    /// An EventQueue message sent from the simulator to an agent when the agent
    /// leaves a group
    /// </summary>
    public class AgentDropGroupMessage : IMessage
    {
        /// <summary>An object containing the Agents UUID, and the Groups UUID</summary>
        public class AgentData
        {
            /// <summary>The ID of the Agent leaving the group</summary>
            public UUID AgentID;
            /// <summary>The GroupID the Agent is leaving</summary>
            public UUID GroupID;
        }

        /// <summary>
        /// An Array containing the AgentID and GroupID
        /// </summary>
        public AgentData[] AgentDataBlock;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);

            OSDArray agentDataArray = new OSDArray(AgentDataBlock.Length);

            for (int i = 0; i < AgentDataBlock.Length; i++)
            {
                OSDMap agentMap = new OSDMap(2);
                agentMap["AgentID"] = OSD.FromUUID(AgentDataBlock[i].AgentID);
                agentMap["GroupID"] = OSD.FromUUID(AgentDataBlock[i].GroupID);
                agentDataArray.Add(agentMap);
            }
            map["AgentData"] = agentDataArray;

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            OSDArray agentDataArray = (OSDArray)map["AgentData"];

            AgentDataBlock = new AgentData[agentDataArray.Count];

            for (int i = 0; i < agentDataArray.Count; i++)
            {
                OSDMap agentMap = (OSDMap)agentDataArray[i];
                AgentData agentData = new AgentData();

                agentData.AgentID = agentMap["AgentID"].AsUUID();
                agentData.GroupID = agentMap["GroupID"].AsUUID();

                AgentDataBlock[i] = agentData;
            }
        }
    }

    /// <summary>Base class for Asset uploads/results via Capabilities</summary>
    public abstract class AssetUploaderBlock
    {
        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public abstract OSDMap Serialize();

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public abstract void Deserialize(OSDMap map);
    }

    /// <summary>
    /// A message sent from the viewer to the simulator to request a temporary upload capability
    /// which allows an asset to be uploaded
    /// </summary>
    public class UploaderRequestUpload : AssetUploaderBlock
    {
        /// <summary>The request state (Always "upload")</summary>
        public string State = "upload";
        /// <summary>The Capability URL sent by the simulator to upload the baked texture to</summary>
        public string Url;

        public override OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);
            map["state"] = OSD.FromString(State);
            map["uploader"] = OSD.FromString(Url);

            return map;
        }

        public override void Deserialize(OSDMap map)
        {
            Url = map["uploader"].AsString();
            State = map["state"].AsString();
        }
    }

    /// <summary>
    /// A message sent from the simulator that will inform the agent the upload is complete, 
    /// and the UUID of the uploaded asset
    /// </summary>
    public class UploaderRequestComplete : AssetUploaderBlock
    {
        /// <summary>The request state (Always "complete")</summary>
        public string State = "complete";
        /// <summary>The uploaded texture asset ID</summary>
        public UUID AssetID;

        public override OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);
            map["state"] = OSD.FromString(State);
            map["new_asset"] = OSD.FromUUID(AssetID);

            return map;
        }

        public override void Deserialize(OSDMap map)
        {
            AssetID = map["new_asset"].AsUUID();
        }
    }

    /// <summary>
    /// A message sent from the viewer to the simulator to request a temporary
    /// capability URI which is used to upload an agents baked appearance textures
    /// </summary>
    public class UploadBakedTextureMessage : IMessage
    {
        /// <summary>Object containing request or response</summary>
        public AssetUploaderBlock Request;
        
        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            return Request.Serialize();
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            if (map.ContainsKey("state") && map["state"].AsString().Equals("upload"))
                Request = new UploaderRequestUpload();
            else if (map.ContainsKey("state") && map["state"].AsString().Equals("complete"))
                Request = new UploaderRequestComplete();
            else
            {
                Logger.Log("Unable to deserialize UploadBakedTexture: No message handler exists for state " + map["state"].AsString(), Helpers.LogLevel.Warning);
                
            }

            if(Request != null)
                Request.Deserialize(map);
        }
    }
    #endregion

    #region Voice Messages
    public class RequiredVoiceVersionMessage : IMessage
    {
        public int MajorVersion;
        public int MinorVersion;
        public string RegionName;
        public string Message = "RequiredVoiceVersion";

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(4);
            map["major_version"] = OSD.FromInteger(MajorVersion);
            map["minor_version"] = OSD.FromInteger(MinorVersion);
            map["region_name"] = OSD.FromString(RegionName);
            map["message"] = OSD.FromString(Message);
            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            MajorVersion = map["major_version"].AsInteger();
            MinorVersion = map["minor_version"].AsInteger();
            RegionName = map["region_name"].AsString();
            Message = map["message"].AsString();
        }
    }

    public class ParcelVoiceInfoRequestMessage : IMessage
    {
        public int ParcelID;
        public string RegionName;
        public Uri SipChannelUri;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);
            map["parcel_local_id"] = OSD.FromInteger(ParcelID);
            map["region_name"] = OSD.FromString(RegionName);

            OSDMap vcMap = new OSDMap(1);
            vcMap["channel_uri"] = OSD.FromUri(SipChannelUri);

            map["voice_credentials"] = vcMap;

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            ParcelID = map["parcel_local_id"].AsInteger();
            RegionName = map["region_name"].AsString();

            OSDMap vcMap = (OSDMap)map["voice_credentials"];
            SipChannelUri = vcMap["channel_uri"].AsUri();
        }
    }

    public class ProvisionVoiceAccountRequestMessage : IMessage
    {
        public string Password;
        public string Username;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);

            map["username"] = OSD.FromString(Username);
            map["password"] = OSD.FromString(Password);

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            Username = map["username"].AsString();
            Password = map["password"].AsString();
        }
    }

    #endregion

    #region Script/Notecards Messages
    /// <summary>
    /// A message sent by the viewer to the simulator to request a temporary
    /// capability for a script contained with in a Tasks inventory to be updated
    /// </summary>
    public class UploadScriptTaskMessage : IMessage
    {
        /// <summary>Object containing request or response</summary>
        public AssetUploaderBlock Request;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            return Request.Serialize();
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            if (map.ContainsKey("state") && map["state"].Equals("upload"))
                Request = new UploaderRequestUpload();
            else if (map.ContainsKey("state") && map["state"].Equals("complete"))
                Request = new UploaderRequestComplete();
            else
                Logger.Log("Unable to deserialize UploadScriptTask: No message handler exists for state " + map["state"].AsString(), Helpers.LogLevel.Warning);

            Request.Deserialize(map);
        }
    }

    /// <summary>
    /// A message sent from the simulator to the viewer to indicate
    /// a Tasks scripts status.
    /// </summary>
    public class ScriptRunningReplyMessage : IMessage
    {
        public UUID ItemID;
        public bool Mono;
        public UUID ObjectID;
        public bool Running;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);

            OSDMap scriptMap = new OSDMap(4);
            scriptMap["ItemID"] = OSD.FromUUID(ItemID);
            scriptMap["Mono"] = OSD.FromBoolean(Mono);
            scriptMap["ObjectID"] = OSD.FromUUID(ObjectID);
            scriptMap["Running"] = OSD.FromBoolean(Running);

            OSDArray scriptArray = new OSDArray(1);
            scriptArray.Add((OSD)scriptMap);

            map["Script"] = scriptArray;

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            OSDArray scriptArray = (OSDArray)map["Script"];

            OSDMap scriptMap = (OSDMap)scriptArray[0];

            ItemID = scriptMap["ItemID"].AsUUID();
            Mono = scriptMap["Mono"].AsBoolean();
            ObjectID = scriptMap["ObjectID"].AsUUID();
            Running = scriptMap["Running"].AsBoolean();
        }
    }

    /// <summary>
    /// A message request/response which is used to update a notecard contained within
    /// a tasks inventory
    /// </summary>
    public class UpdateNotecardTaskInventoryMessage : IMessage
    {
        /// <summary>The <seealso cref="UUID"/> of the Task containing the notecard asset to update</summary>
        public UUID TaskID;
        /// <summary>The notecard assets <seealso cref="UUID"/> contained in the tasks inventory</summary>
        public UUID ItemID;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);
            map["task_id"] = OSD.FromUUID(TaskID);
            map["item_id"] = OSD.FromUUID(ItemID);

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            TaskID = map["task_id"].AsUUID();
            ItemID = map["item_id"].AsUUID();
        }
    }

    // TODO: Add Test
    /// <summary>
    /// A message sent from the viewer to the simulator to request a temporary uploader capability
    /// used to update a notecard contained in the agents inventory
    /// </summary>
    public class UpdateNotecardAgentInventoryUpdateMessage : AssetUploaderBlock
    {
        /// <summary>
        /// The Notecard AssetID to replace
        /// </summary>
        public UUID ItemID;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public override OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);
            map["item_id"] = OSD.FromUUID(ItemID);

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public override void Deserialize(OSDMap map)
        {
            ItemID = map["item_id"].AsUUID();
        }
    }

    /// <summary>
    /// A message containing the request/response used for updating a notecard
    /// contained with an agents inventory
    /// </summary>
    public class UpdateNotecardAgentInventoryMessage : IMessage
    {
        /// <summary>Object containing request or response</summary>
        public AssetUploaderBlock Request;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            return Request.Serialize();
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            if (map.ContainsKey("item_id"))
                Request = new UpdateNotecardAgentInventoryUpdateMessage();
            else if (map.ContainsKey("state") && map["state"].AsString().Equals("upload"))
                Request = new UploaderRequestUpload();
            else if (map.ContainsKey("state") && map["state"].AsString().Equals("complete"))
                Request = new UploaderRequestComplete();
            else
                Logger.Log("Unable to deserialize UpdateNotecardAgentInventory: No message handler exists for state " + map["state"].AsString(), Helpers.LogLevel.Warning);

            if(Request != null)
                Request.Deserialize(map);
        }
    }

    public class CopyInventoryFromNotecardMessage : IMessage
    {
        public int CallbackID;
        public UUID FolderID;
        public UUID ItemID;
        public UUID NotecardID;
        public UUID ObjectID;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(5);
            map["callback-id"] = OSD.FromInteger(CallbackID);
            map["folder-id"] = OSD.FromUUID(FolderID);
            map["item-id"] = OSD.FromUUID(ItemID);
            map["notecard-id"] = OSD.FromUUID(NotecardID);
            map["object-id"] = OSD.FromUUID(ObjectID);

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            CallbackID = map["callback-id"].AsInteger();
            FolderID = map["folder-id"].AsUUID();
            ItemID = map["item-id"].AsUUID();
            NotecardID = map["notecard-id"].AsUUID();
            ObjectID = map["object-id"].AsUUID();
        }
    }

    /// <summary>
    /// A message sent from the simulator to the viewer which indicates
    /// an error occurred while attempting to update a script in an agents or tasks 
    /// inventory
    /// </summary>
    public class UploaderScriptRequestError : AssetUploaderBlock
    {
        /// <summary>true of the script was successfully compiled by the simulator</summary>
        public bool Compiled;
        /// <summary>The state of the request</summary>
        public string State;
        /// <summary>A string containing the error which occured while trying
        /// to update the script</summary>
        public string Error;
        /// <summary>A new AssetID assigned to the script</summary>
        public UUID AssetID;

        public override OSDMap Serialize()
        {
            OSDMap map = new OSDMap(4);
            map["state"] = OSD.FromString(State);
            map["new_asset"] = OSD.FromUUID(AssetID);
            map["compiled"] = OSD.FromBoolean(Compiled);

            OSDArray errorsArray = new OSDArray();
                errorsArray.Add(Error);
            

            map["errors"] = errorsArray;
            return map;
        }

        public override void Deserialize(OSDMap map)
        {
            AssetID = map["new_asset"].AsUUID();
            Compiled = map["compiled"].AsBoolean();
            State = map["state"].AsString();

            OSDArray errorsArray = (OSDArray)map["errors"];
            Error = errorsArray[0].AsString();
            
        }
    }

    /// <summary>
    /// A message sent from the viewer to the simulator
    /// requesting the update of an existing script contained
    /// within a tasks inventory
    /// </summary>
    public class UpdateScriptTaskUpdateMessage : AssetUploaderBlock
    {
        /// <summary>if true, set the script mode to running</summary>
        public bool ScriptRunning;
        /// <summary>The scripts InventoryItem ItemID to update</summary>
        public UUID ItemID;
        /// <summary>A lowercase string containing either "mono" or "lsl2" which 
        /// specifies the script is compiled and ran on the mono runtime, or the older
        /// lsl runtime</summary>
        public string Target; // mono or lsl2
        /// <summary>The tasks <see cref="UUID"/> which contains the script to update</summary>
        public UUID TaskID;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public override OSDMap Serialize()
        {
            OSDMap map = new OSDMap(4);
            map["is_script_running"] = OSD.FromBoolean(ScriptRunning);
            map["item_id"] = OSD.FromUUID(ItemID);
            map["target"] = OSD.FromString(Target);
            map["task_id"] = OSD.FromUUID(TaskID);
            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public override void Deserialize(OSDMap map)
        {
            ScriptRunning = map["is_script_running"].AsBoolean();
            ItemID = map["item_id"].AsUUID();
            Target = map["target"].AsString();
            TaskID = map["task_id"].AsUUID();
        }
    }

    /// <summary>
    /// A message containing either the request or response used in updating a script inside
    /// a tasks inventory
    /// </summary>
    public class UpdateScriptTaskMessage : IMessage
    {
        /// <summary>Object containing request or response</summary>
        public AssetUploaderBlock Request;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            return Request.Serialize();
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            if (map.ContainsKey("task_id"))
                Request = new UpdateScriptTaskUpdateMessage();
            else if (map.ContainsKey("state") && map["state"].AsString().Equals("upload"))
                Request = new UploaderRequestUpload();
            else if (map.ContainsKey("state") && map["state"].AsString().Equals("complete")
                && map.ContainsKey("errors"))
                Request = new UploaderScriptRequestError();
            else if (map.ContainsKey("state") && map["state"].AsString().Equals("complete"))
                Request = new UploaderRequestScriptComplete();
            else
                Logger.Log("Unable to deserialize UpdateScriptTaskMessage: No message handler exists for state " + map["state"].AsString(), Helpers.LogLevel.Warning);

            if (Request != null)
                Request.Deserialize(map);
        }
    }    

    /// <summary>
    /// Response from the simulator to notify the viewer the upload is completed, and
    /// the UUID of the script asset and its compiled status
    /// </summary>
    public class UploaderRequestScriptComplete : AssetUploaderBlock
    {
        /// <summary>The request state (Always "complete")</summary>
        public string State = "complete";
        /// <summary>The uploaded texture asset ID</summary>
        public UUID AssetID;
        /// <summary>true of the script was compiled successfully</summary>
        public bool Compiled;

        public override OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);
            map["state"] = OSD.FromString(State);
            map["new_asset"] = OSD.FromUUID(AssetID);
            map["compiled"] = OSD.FromBoolean(Compiled);
            return map;
        }

        public override void Deserialize(OSDMap map)
        {
            AssetID = map["new_asset"].AsUUID();
            Compiled = map["compiled"].AsBoolean();
        }
    }

    /// <summary>
    /// A message sent from a viewer to the simulator requesting a temporary uploader capability
    /// used to update a script contained in an agents inventory
    /// </summary>
    public class UpdateScriptAgentRequestMessage : AssetUploaderBlock
    {
        /// <summary>The existing asset if of the script in the agents inventory to replace</summary>
        public UUID ItemID;
        /// <summary>The language of the script</summary>
        /// <remarks>Defaults to lsl version 2, "mono" might be another possible option</remarks>
        public string Target = "lsl2"; // lsl2

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public override OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);
            map["item_id"] = OSD.FromUUID(ItemID);
            map["target"] = OSD.FromString(Target);
            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public override void Deserialize(OSDMap map)
        {
            ItemID = map["item_id"].AsUUID();
            Target = map["target"].AsString();
        }
    }

    /// <summary>
    /// A message containing either the request or response used in updating a script inside
    /// an agents inventory
    /// </summary>
    public class UpdateScriptAgentMessage : IMessage
    {
        /// <summary>Object containing request or response</summary>
        public AssetUploaderBlock Request;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            return Request.Serialize();
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            if (map.ContainsKey("item_id"))
                Request = new UpdateScriptAgentRequestMessage();
            else if(map.ContainsKey("errors"))
                Request = new UploaderScriptRequestError();
            else if (map.ContainsKey("state") && map["state"].AsString().Equals("upload"))
                Request = new UploaderRequestUpload();
            else if (map.ContainsKey("state") && map["state"].AsString().Equals("complete"))
                Request = new UploaderRequestScriptComplete();
            else
                Logger.Log("Unable to deserialize UpdateScriptAgent: No message handler exists for state " + map["state"].AsString(), Helpers.LogLevel.Warning);

            if(Request != null)
                Request.Deserialize(map);
        }
    }


    public class SendPostcardMessage : IMessage
    {
        public string FromEmail;
        public string Message;
        public string FromName;
        public Vector3 GlobalPosition;
        public string Subject;
        public string ToEmail;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(6);
            map["from"] = OSD.FromString(FromEmail);
            map["msg"] = OSD.FromString(Message);
            map["name"] = OSD.FromString(FromName);
            map["pos-global"] = OSD.FromVector3(GlobalPosition);
            map["subject"] = OSD.FromString(Subject);
            map["to"] = OSD.FromString(ToEmail);
            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            FromEmail = map["from"].AsString();
            Message = map["msg"].AsString();
            FromName = map["name"].AsString();
            GlobalPosition = map["pos-global"].AsVector3();
            Subject = map["subject"].AsString();
            ToEmail = map["to"].AsString();
        }
    }

    #endregion

    #region Grid/Maps

    public class MapLayerMessage : IMessage
    {
        // AgentData -> Flags
        public int Flags;

        public class LayerData
        {
            public UUID ImageID;
            public int Bottom;
            public int Left;
            public int Right;
            public int Top;
        }

        public LayerData[] LayerDataBlocks;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);
            OSDMap agentMap = new OSDMap(1);
            agentMap["Flags"] = OSD.FromInteger(Flags);
            map["AgentData"] = agentMap;

            OSDArray layerArray = new OSDArray(LayerDataBlocks.Length);

            for (int i = 0; i < LayerDataBlocks.Length; i++)
            {
                OSDMap layerMap = new OSDMap(5);
                layerMap["ImageID"] = OSD.FromUUID(LayerDataBlocks[i].ImageID);
                layerMap["Bottom"] = OSD.FromInteger(LayerDataBlocks[i].Bottom);
                layerMap["Left"] = OSD.FromInteger(LayerDataBlocks[i].Left);
                layerMap["Top"] = OSD.FromInteger(LayerDataBlocks[i].Top);
                layerMap["Right"] = OSD.FromInteger(LayerDataBlocks[i].Right);

                layerArray.Add(layerMap);
            }

            map["LayerData"] = layerArray;

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            OSDMap agentMap = (OSDMap)map["AgentData"];
            Flags = agentMap["Flags"].AsInteger();

            OSDArray layerArray = (OSDArray)map["LayerData"];

            LayerDataBlocks = new LayerData[layerArray.Count];

            for (int i = 0; i < LayerDataBlocks.Length; i++)
            {
                OSDMap layerMap = (OSDMap)layerArray[i];

                LayerData layer = new LayerData();
                layer.ImageID = layerMap["ImageID"].AsUUID();
                layer.Top = layerMap["Top"].AsInteger();
                layer.Right = layerMap["Right"].AsInteger();
                layer.Left = layerMap["Left"].AsInteger();
                layer.Bottom = layerMap["Bottom"].AsInteger();

                LayerDataBlocks[i] = layer;
            }
        }
    }

    #endregion

    #region Session/Communication

    /// <summary>
    /// New as of 1.23 RC1, no details yet.
    /// </summary>
    public class ProductInfoRequestMessage : IMessage
    {
        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            throw new NotImplementedException();
        }

    }

    #region ChatSessionRequestMessage

    public abstract class SearchStatRequestBlock
    {
        public abstract OSDMap Serialize();
        public abstract void Deserialize(OSDMap map);
    }

    // variant A - the request to the simulator
    public class SearchStatRequestRequest : SearchStatRequestBlock
    {
        public UUID ClassifiedID;

        public override OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);
            map["classified_id"] = OSD.FromUUID(ClassifiedID);
            return map;
        }

        public override void Deserialize(OSDMap map)
        {
            ClassifiedID = map["classified_id"].AsUUID();
        }
    }

    public class SearchStatRequestReply : SearchStatRequestBlock
    {
        public int MapClicks;
        public int ProfileClicks;
        public int SearchMapClicks;
        public int SearchProfileClicks;
        public int SearchTeleportClicks;
        public int TeleportClicks;

        public override OSDMap Serialize()
        {
            OSDMap map = new OSDMap(6);
            map["map_clicks"] = OSD.FromInteger(MapClicks);
            map["profile_clicks"] = OSD.FromInteger(ProfileClicks);
            map["search_map_clicks"] = OSD.FromInteger(SearchMapClicks);
            map["search_profile_clicks"] = OSD.FromInteger(SearchProfileClicks);
            map["search_teleport_clicks"] = OSD.FromInteger(SearchTeleportClicks);
            map["teleport_clicks"] = OSD.FromInteger(TeleportClicks);
            return map;
        }

        public override void Deserialize(OSDMap map)
        {
            MapClicks = map["map_clicks"].AsInteger();
            ProfileClicks = map["profile_clicks"].AsInteger();
            SearchMapClicks = map["search_map_clicks"].AsInteger();
            SearchProfileClicks = map["search_profile_clicks"].AsInteger();
            SearchTeleportClicks = map["search_teleport_clicks"].AsInteger();
            TeleportClicks = map["teleport_clicks"].AsInteger();
        }
    }

    public class SearchStatRequestMessage : IMessage
    {
        public SearchStatRequestBlock Request;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            return Request.Serialize();
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            if (map.ContainsKey("map_clicks"))
                Request = new SearchStatRequestReply();
            else if (map.ContainsKey("classified_id"))
                Request = new SearchStatRequestRequest();
            else
                Logger.Log("Unable to deserialize SearchStatRequest: No message handler exists for method " + map["method"].AsString(), Helpers.LogLevel.Warning);

            Request.Deserialize(map);
        }
    }

    public abstract class ChatSessionRequestBlock
    {
        /// <summary>A string containing the method used</summary>
        public string Method;

        public abstract OSDMap Serialize();
        public abstract void Deserialize(OSDMap map);
    }

    /// <summary>
    /// A request sent from an agent to the Simulator to begin a new conference.
    /// Contains a list of Agents to be in the conference
    /// </summary>
    /* VARIANT A:
    {
        'method':'start conference'
        ,
        'params':
        [
            u69d068f3-824f-43ca-8b9a-1542065669b9,
            ua11dccfb-a55f-406a-bad2-f851af5979be
        ]
        ,
        'session-id':u30dcb931-c2f8-469e-662d-f7c2daacffee
    } */
    public class ChatSessionRequestStartConference : ChatSessionRequestBlock
    {
        /// <summary>An array containing the <see cref="UUID"/> of the agents invited to this conference</summary>
        public UUID[] AgentsBlock;
        /// <summary>The conferences Session ID</summary>
        public UUID SessionID;

        public ChatSessionRequestStartConference()
        {
            Method = "start conference";
        }

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public override OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);
            map["method"] = OSD.FromString(Method);
            OSDArray agentsArray = new OSDArray();
            for (int i = 0; i < AgentsBlock.Length; i++)
            {
                agentsArray.Add(OSD.FromUUID(AgentsBlock[i]));
            }
            map["params"] = agentsArray;
            map["session-id"] = OSD.FromUUID(SessionID);

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public override void Deserialize(OSDMap map)
        {
            Method = map["method"].AsString();
            OSDArray agentsArray = (OSDArray)map["params"];

            AgentsBlock = new UUID[agentsArray.Count];

            for (int i = 0; i < agentsArray.Count; i++)
            {
                AgentsBlock[i] = agentsArray[i].AsUUID();
            }

            SessionID = map["session-id"].AsUUID();
        }
    }

    /// <summary>
    /// A moderation request sent from a conference moderator
    /// Contains an agent and an optional action to take
    /// </summary>
    /* VARIANT B:
    {
            'method':'mute update'
            ,
            'params':
            {
            'agent_id':u69d068f3-824f-43ca-8b9a-1542065669b9
            ,
            'mute_info':
            {
                'text':t
            }
        }
        ,
        'session-id':ufdf0ac75-73b7-0320-96a8-ddd295ba7d8c
    } */
    public class ChatSessionRequestMuteUpdate : ChatSessionRequestBlock
    {
        /// <summary>The Session ID</summary>
        public UUID SessionID;
        /// <summary></summary>
        public UUID AgentID;
        /// <summary>A list containing Key/Value pairs, known valid values:
        /// key: text value: true/false - allow/disallow specified agents ability to use text in session
        /// key: voice value: true/false - allow/disallow specified agents ability to use voice in session
        /// </summary>
        /// <remarks>"text" or "voice"</remarks>
        public string RequestKey;
        /// <summary></summary>
        public bool RequestValue;

        public ChatSessionRequestMuteUpdate()
        {
            Method = "mute update";
        }

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public override OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);
            map["method"] = OSD.FromString(Method);

            OSDMap muteMap = new OSDMap(1);
            muteMap[RequestKey] = OSD.FromBoolean(RequestValue);

            OSDMap paramMap = new OSDMap(2);
            paramMap["agent_id"] = OSD.FromUUID(AgentID);
            paramMap["mute_info"] = muteMap;

            map["params"] = paramMap;
            map["session-id"] = OSD.FromUUID(SessionID);

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public override void Deserialize(OSDMap map)
        {
            Method = map["method"].AsString();
            SessionID = map["session-id"].AsUUID();

            OSDMap paramsMap = (OSDMap)map["params"];
            OSDMap muteMap = (OSDMap)paramsMap["mute_info"];

            AgentID = paramsMap["agent_id"].AsUUID();

            if (muteMap.ContainsKey("text"))
                RequestKey = "text";
            else if (muteMap.ContainsKey("voice"))
                RequestKey = "voice";

            RequestValue = muteMap[RequestKey].AsBoolean();
        }
    }


    /*
     * VARIANT C
    {
     'method':'accept invitation'
     ,
     'session-id':u30867cec-7906-2ca1-400f-742ad88e4928
    }*/
    /// <summary>
    /// A message sent from the agent to the simulator which tells the 
    /// simulator we've accepted a conference invitation
    /// </summary>
    public class ChatSessionAcceptInvitation : ChatSessionRequestBlock
    {
        /// <summary>The conference SessionID</summary>
        public UUID SessionID;

        public ChatSessionAcceptInvitation()
        {
            Method = "accept invitation";
        }

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public override OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);
            map["method"] = OSD.FromString(Method);
            map["session-id"] = OSD.FromUUID(SessionID);
            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public override void Deserialize(OSDMap map)
        {
            Method = map["method"].AsString();
            SessionID = map["session-id"].AsUUID();
        }
    }

    public class ChatSessionRequestMessage : IMessage
    {
        public ChatSessionRequestBlock Request;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            return Request.Serialize();
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            if (map.ContainsKey("method") && map["method"].AsString().Equals("start conference"))
                Request = new ChatSessionRequestStartConference();
            else if (map.ContainsKey("method") && map["method"].AsString().Equals("mute update"))
                Request = new ChatSessionRequestMuteUpdate();
            else if (map.ContainsKey("method") && map["method"].AsString().Equals("accept invitation"))
                Request = new ChatSessionAcceptInvitation();
            else
                Logger.Log("Unable to deserialize ChatSessionRequest: No message handler exists for method " + map["method"].AsString(), Helpers.LogLevel.Warning);

            Request.Deserialize(map);
        }
    }

    #endregion

    public class ChatterboxSessionEventReplyMessage : IMessage
    {
        public UUID SessionID;
        public bool Success;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);
            map["success"] = OSD.FromBoolean(Success);
            map["session_id"] = OSD.FromUUID(SessionID); // FIXME: Verify this is correct map name

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            Success = map["success"].AsBoolean();
            SessionID = map["session_id"].AsUUID();
        }
    }

    public class ChatterBoxSessionStartReplyMessage : IMessage
    {
        public UUID SessionID;
        public UUID TempSessionID;
        public bool Success;

        public string SessionName;
        // FIXME: Replace int with an enum
        public int Type;
        public bool VoiceEnabled;
        public bool ModeratedVoice;

        /* Is Text moderation possible? */

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap moderatedMap = new OSDMap(1);
            moderatedMap["voice"] = OSD.FromBoolean(ModeratedVoice);

            OSDMap sessionMap = new OSDMap(4);
            sessionMap["type"] = OSD.FromInteger(Type);
            sessionMap["session_name"] = OSD.FromString(SessionName);
            sessionMap["voice_enabled"] = OSD.FromBoolean(VoiceEnabled);
            sessionMap["moderated_mode"] = moderatedMap;

            OSDMap map = new OSDMap(4);
            map["session_id"] = OSD.FromUUID(SessionID);
            map["temp_session_id"] = OSD.FromUUID(TempSessionID);
            map["success"] = OSD.FromBoolean(Success);
            map["session_info"] = sessionMap;

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            SessionID = map["session_id"].AsUUID();
            TempSessionID = map["temp_session_id"].AsUUID();
            Success = map["success"].AsBoolean();

            if (Success)
            {
                OSDMap sessionMap = (OSDMap)map["session_info"];
                SessionName = sessionMap["session_name"].AsString();
                Type = sessionMap["type"].AsInteger();
                VoiceEnabled = sessionMap["voice_enabled"].AsBoolean();

                OSDMap moderatedModeMap = (OSDMap)sessionMap["moderated_mode"];
                ModeratedVoice = moderatedModeMap["voice"].AsBoolean();
            }
        }
    }

    public class ChatterBoxInvitationMessage : IMessage
    {
        /// <summary>Key of sender</summary>
        public UUID FromAgentID;
        /// <summary>Name of sender</summary>
        public string FromAgentName;
        /// <summary>Key of destination avatar</summary>
        public UUID ToAgentID;
        /// <summary>ID of originating estate</summary>
        public uint ParentEstateID;
        /// <summary>Key of originating region</summary>
        public UUID RegionID;
        /// <summary>Coordinates in originating region</summary>
        public Vector3 Position;
        /// <summary>Instant message type</summary>
        public InstantMessageDialog Dialog;
        /// <summary>Group IM session toggle</summary>
        public bool GroupIM;
        /// <summary>Key of IM session, for Group Messages, the groups UUID</summary>
        public UUID IMSessionID;
        /// <summary>Timestamp of the instant message</summary>
        public DateTime Timestamp;
        /// <summary>Instant message text</summary>
        public string Message;
        /// <summary>Whether this message is held for offline avatars</summary>
        public InstantMessageOnline Offline;
        /// <summary>Context specific packed data</summary>
        public byte[] BinaryBucket;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap dataMap = new OSDMap(3);
            dataMap["timestamp"] = OSD.FromDate(Timestamp);
            dataMap["type"] = OSD.FromInteger((uint)Dialog);
            dataMap["binary_bucket"] = OSD.FromBinary(BinaryBucket);

            OSDMap paramsMap = new OSDMap(11);
            paramsMap["from_id"] = OSD.FromUUID(FromAgentID);
            paramsMap["from_name"] = OSD.FromString(FromAgentName);
            paramsMap["to_id"] = OSD.FromUUID(ToAgentID);
            paramsMap["parent_estate_id"] = OSD.FromInteger(ParentEstateID);
            paramsMap["region_id"] = OSD.FromUUID(RegionID);
            paramsMap["position"] = OSD.FromVector3(Position);
            paramsMap["from_group"] = OSD.FromBoolean(GroupIM);
            paramsMap["id"] = OSD.FromUUID(IMSessionID);
            paramsMap["message"] = OSD.FromString(Message);
            paramsMap["offline"] = OSD.FromInteger((uint)Offline);

            paramsMap["data"] = dataMap;

            OSDMap imMap = new OSDMap(1);
            imMap["message_params"] = paramsMap;

            OSDMap map = new OSDMap(1);
            map["instantmessage"] = imMap;


            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            OSDMap im = (OSDMap)map["instantmessage"];
            OSDMap msg = (OSDMap)im["message_params"];
            OSDMap msgdata = (OSDMap)msg["data"];

            FromAgentID = msg["from_id"].AsUUID();
            FromAgentName = msg["from_name"].AsString();
            ToAgentID = msg["to_id"].AsUUID();
            ParentEstateID = (uint)msg["parent_estate_id"].AsInteger();
            RegionID = msg["region_id"].AsUUID();
            Position = msg["position"].AsVector3();
            GroupIM = msg["from_group"].AsBoolean();
            IMSessionID = msg["id"].AsUUID();
            Message = msg["message"].AsString();
            Offline = (InstantMessageOnline)msg["offline"].AsInteger();
            Dialog = (InstantMessageDialog)msgdata["type"].AsInteger();
            BinaryBucket = msgdata["binary_bucket"].AsBinary();
            Timestamp = msgdata["timestamp"].AsDate();
        }
    }

    /// <summary>
    /// Sent from the simulator to the viewer.
    /// 
    /// When an agent initially joins a session the AgentUpdatesBlock object will contain a list of session members including
    /// a boolean indicating they can use voice chat in this session, a boolean indicating they are allowed to moderate 
    /// this session, and lastly a string which indicates another agent is entering the session with the Transition set to "ENTER"
    /// 
    /// During the session lifetime updates on individuals are sent. During the update the booleans sent during the initial join are
    /// excluded with the exception of the Transition field. This indicates a new user entering or exiting the session with
    /// the string "ENTER" or "LEAVE" respectively.
    /// </summary>
    public class ChatterBoxSessionAgentListUpdatesMessage : IMessage
    {
        // initial when agent joins session
        // <llsd><map><key>events</key><array><map><key>body</key><map><key>agent_updates</key><map><key>32939971-a520-4b52-8ca5-6085d0e39933</key><map><key>info</key><map><key>can_voice_chat</key><boolean>1</boolean><key>is_moderator</key><boolean>1</boolean></map><key>transition</key><string>ENTER</string></map><key>ca00e3e1-0fdb-4136-8ed4-0aab739b29e8</key><map><key>info</key><map><key>can_voice_chat</key><boolean>1</boolean><key>is_moderator</key><boolean>0</boolean></map><key>transition</key><string>ENTER</string></map></map><key>session_id</key><string>be7a1def-bd8a-5043-5d5b-49e3805adf6b</string><key>updates</key><map><key>32939971-a520-4b52-8ca5-6085d0e39933</key><string>ENTER</string><key>ca00e3e1-0fdb-4136-8ed4-0aab739b29e8</key><string>ENTER</string></map></map><key>message</key><string>ChatterBoxSessionAgentListUpdates</string></map><map><key>body</key><map><key>agent_updates</key><map><key>32939971-a520-4b52-8ca5-6085d0e39933</key><map><key>info</key><map><key>can_voice_chat</key><boolean>1</boolean><key>is_moderator</key><boolean>1</boolean></map></map></map><key>session_id</key><string>be7a1def-bd8a-5043-5d5b-49e3805adf6b</string><key>updates</key><map /></map><key>message</key><string>ChatterBoxSessionAgentListUpdates</string></map></array><key>id</key><integer>5</integer></map></llsd>

        // a message containing only moderator updates
        // <llsd><map><key>events</key><array><map><key>body</key><map><key>agent_updates</key><map><key>ca00e3e1-0fdb-4136-8ed4-0aab739b29e8</key><map><key>info</key><map><key>mutes</key><map><key>text</key><boolean>1</boolean></map></map></map></map><key>session_id</key><string>be7a1def-bd8a-5043-5d5b-49e3805adf6b</string><key>updates</key><map /></map><key>message</key><string>ChatterBoxSessionAgentListUpdates</string></map></array><key>id</key><integer>7</integer></map></llsd>

        public UUID SessionID;
        public string Message = "ChatterBoxSessionAgentListUpdates"; // message

        public class AgentUpdatesBlock
        {
            public UUID AgentID;

            public bool CanVoiceChat;
            public bool IsModerator;
            // transition "transition" = "ENTER" or "LEAVE"
            public string Transition;   //  TODO: switch to an enum "ENTER" or "LEAVE"

            public bool MuteText;
            public bool MuteVoice;
        }

        public AgentUpdatesBlock[] Updates;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();

            OSDMap agent_updatesMap = new OSDMap(1);
            for (int i = 0; i < Updates.Length; i++)
            {
                OSDMap mutesMap = new OSDMap(2);
                mutesMap["text"] = OSD.FromBoolean(Updates[i].MuteText);
                mutesMap["voice"] = OSD.FromBoolean(Updates[i].MuteVoice);

                OSDMap infoMap = new OSDMap(4);
                infoMap["can_voice_chat"] = OSD.FromBoolean((bool)Updates[i].CanVoiceChat);
                infoMap["is_moderator"] = OSD.FromBoolean((bool)Updates[i].IsModerator);

                OSDMap imap = new OSDMap(1);
                imap["info"] = infoMap;
                imap["transition"] = OSD.FromString(Updates[i].Transition);
                imap.Add("mutes", mutesMap);

                agent_updatesMap.Add(Updates[i].AgentID.ToString(), imap);
            }

            map.Add("agent_updates", agent_updatesMap);

            map["session_id"] = OSD.FromUUID(SessionID);

            map["message"] = OSD.FromString(Message);

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {

            OSDMap agent_updates = (OSDMap)map["agent_updates"];
            SessionID = map["session_id"].AsUUID();
            Message = map["message"].AsString();

            List<AgentUpdatesBlock> updatesList = new List<AgentUpdatesBlock>();

            foreach (KeyValuePair<string, OSD> kvp in agent_updates)
            {

                if (kvp.Key == "updates")
                {
                    // This appears to be redundant and duplicated by the info block, more dumps will confirm this
                    /* <key>32939971-a520-4b52-8ca5-6085d0e39933</key>
                            <string>ENTER</string> */
                }
                else if (kvp.Key == "session_id")
                {
                    // I am making the assumption that each osdmap will contain the information for a 
                    // single session. This is how the map appears to read however more dumps should be taken
                    // to confirm this.
                    /* <key>session_id</key>
                            <string>984f6a1e-4ceb-6366-8d5e-a18c6819c6f7</string> */

                }
                else  // key is an agent uuid (we hope!)
                {
                    // should be the agents uuid as the key, and "info" as the datablock
                    /* <key>32939971-a520-4b52-8ca5-6085d0e39933</key>
                            <map>
                                <key>info</key>
                                    <map>
                                        <key>can_voice_chat</key>
                                            <boolean>1</boolean>
                                        <key>is_moderator</key>
                                            <boolean>1</boolean>
                                    </map>
                                <key>transition</key>
                                    <string>ENTER</string>
                            </map>*/
                    AgentUpdatesBlock block = new AgentUpdatesBlock();
                    block.AgentID = UUID.Parse(kvp.Key);

                    OSDMap infoMap = (OSDMap)agent_updates[kvp.Key];

                    OSDMap agentPermsMap = (OSDMap)infoMap["info"];

                    block.CanVoiceChat = agentPermsMap["can_voice_chat"].AsBoolean();
                    block.IsModerator = agentPermsMap["is_moderator"].AsBoolean();

                    block.Transition = infoMap["transition"].AsString();

                    if (infoMap.ContainsKey("mutes"))
                    {
                        OSDMap mutesMap = (OSDMap)infoMap["mutes"];
                        block.MuteText = mutesMap["text"].AsBoolean();
                        block.MuteVoice = mutesMap["voice"].AsBoolean();
                    }
                    updatesList.Add(block);
                }
            }

            Updates = new AgentUpdatesBlock[updatesList.Count];

            for (int i = 0; i < updatesList.Count; i++)
            {
                AgentUpdatesBlock block = new AgentUpdatesBlock();
                block.AgentID = updatesList[i].AgentID;
                block.CanVoiceChat = updatesList[i].CanVoiceChat;
                block.IsModerator = updatesList[i].IsModerator;
                block.MuteText = updatesList[i].MuteText;
                block.MuteVoice = updatesList[i].MuteVoice;
                block.Transition = updatesList[i].Transition;
                Updates[i] = block;
            }
        }
    }

    /// <summary>
    /// An EventQueue message sent when the agent is forcibly removed from a chatterbox session
    /// </summary>
    public class ForceCloseChatterBoxSessionMessage : IMessage
    {
        /// <summary>
        /// A string containing the reason the agent was removed
        /// </summary>
        public string Reason;
        /// <summary>
        /// The ChatterBoxSession's SessionID
        /// </summary>
        public UUID SessionID;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);
            map["reason"] = OSD.FromString(Reason);
            map["session_id"] = OSD.FromUUID(SessionID);

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            Reason = map["reason"].AsString();
            SessionID = map["session_id"].AsUUID();
        }
    }

    #endregion

    #region EventQueue

    public abstract class EventMessageBlock
    {
        public abstract OSDMap Serialize();
        public abstract void Deserialize(OSDMap map);
    }

    public class EventQueueAck : EventMessageBlock
    {
        public int AckID;
        public bool Done;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public override OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["ack"] = OSD.FromInteger(AckID);
            map["done"] = OSD.FromBoolean(Done);
            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public override void Deserialize(OSDMap map)
        {
            AckID = map["ack"].AsInteger();
            Done = map["done"].AsBoolean();
        }
    }

    public class EventQueueEvent : EventMessageBlock
    {
        public class QueueEvent
        {
            public IMessage EventMessage;
            public string MessageKey;
        }

        public int Sequence;
        public QueueEvent[] MessageEvents;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public override OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);

            OSDArray eventsArray = new OSDArray();

            for (int i = 0; i < MessageEvents.Length; i++)
            {
                OSDMap eventMap = new OSDMap(2);
                eventMap["body"] = MessageEvents[i].EventMessage.Serialize();
                eventMap["message"] = OSD.FromString(MessageEvents[i].MessageKey);
                eventsArray.Add(eventMap);
            }

            map["events"] = eventsArray;
            map["id"] = OSD.FromInteger(Sequence);

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public override void Deserialize(OSDMap map)
        {
            Sequence = map["id"].AsInteger();
            OSDArray arrayEvents = (OSDArray)map["events"];

            MessageEvents = new QueueEvent[arrayEvents.Count];

            for (int i = 0; i < arrayEvents.Count; i++)
            {
                OSDMap eventMap = (OSDMap)arrayEvents[i];
                QueueEvent ev = new QueueEvent();

                ev.MessageKey = eventMap["message"].AsString();
                ev.EventMessage = MessageUtils.DecodeEvent(ev.MessageKey, (OSDMap)eventMap["body"]);
                MessageEvents[i] = ev;
            }
        }
    }

    public class EventQueueGetMessage : IMessage
    {
        public EventMessageBlock Messages;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            return Messages.Serialize();
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            if (map.ContainsKey("ack"))
                Messages = new EventQueueAck();
            else if (map.ContainsKey("events"))
                Messages = new EventQueueEvent();
            else
                Logger.Log("Unable to deserialize EventQueueGetMessage: No message handler exists for event", Helpers.LogLevel.Warning);

            Messages.Deserialize(map);
        }
    }

    #endregion

    #region Stats Messages

    public class ViewerStatsMessage : IMessage
    {
        public int AgentsInView;
        public float AgentFPS;
        public string AgentLanguage;
        public float AgentMemoryUsed;
        public float MetersTraveled;
        public float AgentPing;
        public int RegionsVisited;
        public float AgentRuntime;
        public float SimulatorFPS;
        public DateTime AgentStartTime;
        public string AgentVersion;

        public float object_kbytes;
        public float texture_kbytes;
        public float world_kbytes;

        public float MiscVersion;
        public bool VertexBuffersEnabled;

        public UUID SessionID;

        public int StatsDropped;
        public int StatsFailedResends;
        public int FailuresInvalid;
        public int FailuresOffCircuit;
        public int FailuresResent;
        public int FailuresSendPacket;

        public int MiscInt1;
        public int MiscInt2;
        public string MiscString1;

        public int InCompressedPackets;
        public float InKbytes;
        public float InPackets;
        public float InSavings;

        public int OutCompressedPackets;
        public float OutKbytes;
        public float OutPackets;
        public float OutSavings;

        public string SystemCPU;
        public string SystemGPU;
        public int SystemGPUClass;
        public string SystemGPUVendor;
        public string SystemGPUVersion;
        public string SystemOS;
        public int SystemInstalledRam;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(5);
            map["session_id"] = OSD.FromUUID(SessionID);

            OSDMap agentMap = new OSDMap(11);
            agentMap["agents_in_view"] = OSD.FromInteger(AgentsInView);
            agentMap["fps"] = OSD.FromReal(AgentFPS);
            agentMap["language"] = OSD.FromString(AgentLanguage);
            agentMap["mem_use"] = OSD.FromReal(AgentMemoryUsed);
            agentMap["meters_traveled"] = OSD.FromReal(MetersTraveled);
            agentMap["ping"] = OSD.FromReal(AgentPing);
            agentMap["regions_visited"] = OSD.FromInteger(RegionsVisited);
            agentMap["run_time"] = OSD.FromReal(AgentRuntime);
            agentMap["sim_fps"] = OSD.FromReal(SimulatorFPS);
            agentMap["start_time"] = OSD.FromUInteger(Utils.DateTimeToUnixTime(AgentStartTime));
            agentMap["version"] = OSD.FromString(AgentVersion);
            map["agent"] = agentMap;


            OSDMap downloadsMap = new OSDMap(3); // downloads
            downloadsMap["object_kbytes"] = OSD.FromReal(object_kbytes);
            downloadsMap["texture_kbytes"] = OSD.FromReal(texture_kbytes);
            downloadsMap["world_kbytes"] = OSD.FromReal(world_kbytes);
            map["downloads"] = downloadsMap;

            OSDMap miscMap = new OSDMap(2);
            miscMap["Version"] = OSD.FromReal(MiscVersion);
            miscMap["Vertex Buffers Enabled"] = OSD.FromBoolean(VertexBuffersEnabled);
            map["misc"] = miscMap;

            OSDMap statsMap = new OSDMap(2);

            OSDMap failuresMap = new OSDMap(6);
            failuresMap["dropped"] = OSD.FromInteger(StatsDropped);
            failuresMap["failed_resends"] = OSD.FromInteger(StatsFailedResends);
            failuresMap["invalid"] = OSD.FromInteger(FailuresInvalid);
            failuresMap["off_circuit"] = OSD.FromInteger(FailuresOffCircuit);
            failuresMap["resent"] = OSD.FromInteger(FailuresResent);
            failuresMap["send_packet"] = OSD.FromInteger(FailuresSendPacket);
            statsMap["failures"] = failuresMap;

            OSDMap statsMiscMap = new OSDMap(3);
            statsMiscMap["int_1"] = OSD.FromInteger(MiscInt1);
            statsMiscMap["int_2"] = OSD.FromInteger(MiscInt2);
            statsMiscMap["string_1"] = OSD.FromString(MiscString1);
            statsMap["misc"] = statsMiscMap;

            OSDMap netMap = new OSDMap(3);

            // in
            OSDMap netInMap = new OSDMap(4);
            netInMap["compressed_packets"] = OSD.FromInteger(InCompressedPackets);
            netInMap["kbytes"] = OSD.FromReal(InKbytes);
            netInMap["packets"] = OSD.FromReal(InPackets);
            netInMap["savings"] = OSD.FromReal(InSavings);
            netMap["in"] = netInMap;
            // out
            OSDMap netOutMap = new OSDMap(4);
            netOutMap["compressed_packets"] = OSD.FromInteger(OutCompressedPackets);
            netOutMap["kbytes"] = OSD.FromReal(OutKbytes);
            netOutMap["packets"] = OSD.FromReal(OutPackets);
            netOutMap["savings"] = OSD.FromReal(OutSavings);
            netMap["out"] = netOutMap;

            statsMap["net"] = netMap;

            //system
            OSDMap systemStatsMap = new OSDMap(7);
            systemStatsMap["cpu"] = OSD.FromString(SystemCPU);
            systemStatsMap["gpu"] = OSD.FromString(SystemGPU);
            systemStatsMap["gpu_class"] = OSD.FromInteger(SystemGPUClass);
            systemStatsMap["gpu_vendor"] = OSD.FromString(SystemGPUVendor);
            systemStatsMap["gpu_version"] = OSD.FromString(SystemGPUVersion);
            systemStatsMap["os"] = OSD.FromString(SystemOS);
            systemStatsMap["ram"] = OSD.FromInteger(SystemInstalledRam);
            map["system"] = systemStatsMap;

            map["stats"] = statsMap;
            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            SessionID = map["session_id"].AsUUID();

            OSDMap agentMap = (OSDMap)map["agent"];
            AgentsInView = agentMap["agents_in_view"].AsInteger();
            AgentFPS = (float)agentMap["fps"].AsReal();
            AgentLanguage = agentMap["language"].AsString();
            AgentMemoryUsed = (float)agentMap["mem_use"].AsReal();
            MetersTraveled = agentMap["meters_traveled"].AsInteger();
            AgentPing = (float)agentMap["ping"].AsReal();
            RegionsVisited = agentMap["regions_visited"].AsInteger();
            AgentRuntime = (float)agentMap["run_time"].AsReal();
            SimulatorFPS = (float)agentMap["sim_fps"].AsReal();
            AgentStartTime = Utils.UnixTimeToDateTime(agentMap["start_time"].AsUInteger());
            AgentVersion = agentMap["version"].AsString();

            OSDMap downloadsMap = (OSDMap)map["downloads"];
            object_kbytes = (float)downloadsMap["object_kbytes"].AsReal();
            texture_kbytes = (float)downloadsMap["texture_kbytes"].AsReal();
            world_kbytes = (float)downloadsMap["world_kbytes"].AsReal();

            OSDMap miscMap = (OSDMap)map["misc"];
            MiscVersion = (float)miscMap["Version"].AsReal();
            VertexBuffersEnabled = miscMap["Vertex Buffers Enabled"].AsBoolean();

            OSDMap statsMap = (OSDMap)map["stats"];
            OSDMap failuresMap = (OSDMap)statsMap["failures"];
            StatsDropped = failuresMap["dropped"].AsInteger();
            StatsFailedResends = failuresMap["failed_resends"].AsInteger();
            FailuresInvalid = failuresMap["invalid"].AsInteger();
            FailuresOffCircuit = failuresMap["off_circuit"].AsInteger();
            FailuresResent = failuresMap["resent"].AsInteger();
            FailuresSendPacket = failuresMap["send_packet"].AsInteger();

            OSDMap statsMiscMap = (OSDMap)statsMap["misc"];
            MiscInt1 = statsMiscMap["int_1"].AsInteger();
            MiscInt2 = statsMiscMap["int_2"].AsInteger();
            MiscString1 = statsMiscMap["string_1"].AsString();
            OSDMap netMap = (OSDMap)statsMap["net"];
            // in
            OSDMap netInMap = (OSDMap)netMap["in"];
            InCompressedPackets = netInMap["compressed_packets"].AsInteger();
            InKbytes = netInMap["kbytes"].AsInteger();
            InPackets = netInMap["packets"].AsInteger();
            InSavings = netInMap["savings"].AsInteger();
            // out
            OSDMap netOutMap = (OSDMap)netMap["out"];
            OutCompressedPackets = netOutMap["compressed_packets"].AsInteger();
            OutKbytes = netOutMap["kbytes"].AsInteger();
            OutPackets = netOutMap["packets"].AsInteger();
            OutSavings = netOutMap["savings"].AsInteger();

            //system
            OSDMap systemStatsMap = (OSDMap)map["system"];
            SystemCPU = systemStatsMap["cpu"].AsString();
            SystemGPU = systemStatsMap["gpu"].AsString();
            SystemGPUClass = systemStatsMap["gpu_class"].AsInteger();
            SystemGPUVendor = systemStatsMap["gpu_vendor"].AsString();
            SystemGPUVersion = systemStatsMap["gpu_version"].AsString();
            SystemOS = systemStatsMap["os"].AsString();
            SystemInstalledRam = systemStatsMap["ram"].AsInteger();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PlacesReplyMessage : IMessage
    {
        public UUID AgentID;
        public UUID QueryID;
        public UUID TransactionID;

        public class QueryData
        {
            public int ActualArea;
            public int BillableArea;
            public string Description;
            public float Dwell;
            public int Flags;
            public float GlobalX;
            public float GlobalY;
            public float GlobalZ;
            public string Name;
            public UUID OwnerID;
            public string SimName;
            public UUID SnapShotID;
            public string ProductSku;
            public int Price;
        }

        public QueryData[] QueryDataBlocks;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);

            // add the AgentData map
            OSDMap agentIDmap = new OSDMap(2);
            agentIDmap["AgentID"] = OSD.FromUUID(AgentID);
            agentIDmap["QueryID"] = OSD.FromUUID(QueryID);

            OSDArray agentDataArray = new OSDArray();
            agentDataArray.Add(agentIDmap);

            map["AgentData"] = agentDataArray;

            // add the QueryData map
            OSDArray dataBlocksArray = new OSDArray(QueryDataBlocks.Length);
            for (int i = 0; i < QueryDataBlocks.Length; i++)
            {
                OSDMap queryDataMap = new OSDMap(14);
                queryDataMap["ActualArea"] = OSD.FromInteger(QueryDataBlocks[i].ActualArea);
                queryDataMap["BillableArea"] = OSD.FromInteger(QueryDataBlocks[i].BillableArea);
                queryDataMap["Desc"] = OSD.FromString(QueryDataBlocks[i].Description);
                queryDataMap["Dwell"] = OSD.FromReal(QueryDataBlocks[i].Dwell);
                queryDataMap["Flags"] = OSD.FromInteger(QueryDataBlocks[i].Flags);
                queryDataMap["GlobalX"] = OSD.FromReal(QueryDataBlocks[i].GlobalX);
                queryDataMap["GlobalY"] = OSD.FromReal(QueryDataBlocks[i].GlobalY);
                queryDataMap["GlobalZ"] = OSD.FromReal(QueryDataBlocks[i].GlobalZ);
                queryDataMap["Name"] = OSD.FromString(QueryDataBlocks[i].Name);
                queryDataMap["OwnerID"] = OSD.FromUUID(QueryDataBlocks[i].OwnerID);
                queryDataMap["Price"] = OSD.FromInteger(QueryDataBlocks[i].Price);
                queryDataMap["SimName"] = OSD.FromString(QueryDataBlocks[i].SimName);
                queryDataMap["SnapshotID"] = OSD.FromUUID(QueryDataBlocks[i].SnapShotID);
                queryDataMap["ProductSKU"] = OSD.FromString(QueryDataBlocks[i].ProductSku);
                dataBlocksArray.Add(queryDataMap);
            }

            map["QueryData"] = dataBlocksArray;

            // add the TransactionData map
            OSDMap transMap = new OSDMap(1);
            transMap["TransactionID"] = OSD.FromUUID(TransactionID);
            OSDArray transArray = new OSDArray();
            transArray.Add(transMap);
            map["TransactionData"] = transArray;

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            OSDArray agentDataArray = (OSDArray)map["AgentData"];

            OSDMap agentDataMap = (OSDMap)agentDataArray[0];
            AgentID = agentDataMap["AgentID"].AsUUID();
            QueryID = agentDataMap["QueryID"].AsUUID();


            OSDArray dataBlocksArray = (OSDArray)map["QueryData"];
            QueryDataBlocks = new QueryData[dataBlocksArray.Count];
            for (int i = 0; i < dataBlocksArray.Count; i++)
            {
                OSDMap dataMap = (OSDMap)dataBlocksArray[i];
                QueryData data = new QueryData();
                data.ActualArea = dataMap["ActualArea"].AsInteger();
                data.BillableArea = dataMap["BillableArea"].AsInteger();
                data.Description = dataMap["Desc"].AsString();
                data.Dwell = (float)dataMap["Dwell"].AsReal();
                data.Flags = dataMap["Flags"].AsInteger();
                data.GlobalX = (float)dataMap["GlobalX"].AsReal();
                data.GlobalY = (float)dataMap["GlobalY"].AsReal();
                data.GlobalZ = (float)dataMap["GlobalZ"].AsReal();
                data.Name = dataMap["Name"].AsString();
                data.OwnerID = dataMap["OwnerID"].AsUUID();
                data.Price = dataMap["Price"].AsInteger();
                data.SimName = dataMap["SimName"].AsString();
                data.SnapShotID = dataMap["SnapshotID"].AsUUID();
                data.ProductSku = dataMap["ProductSKU"].AsString();
                QueryDataBlocks[i] = data;
            }

            OSDArray transactionArray = (OSDArray)map["TransactionData"];
            OSDMap transactionDataMap = (OSDMap)transactionArray[0];
            TransactionID = transactionDataMap["TransactionID"].AsUUID();
        }
    }

    public class UpdateAgentInformationMessage : IMessage
    {
        public string MaxAccess; // PG, A, or M

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);
            OSDMap prefsMap = new OSDMap(1);
            prefsMap["max"] = OSD.FromString(MaxAccess);
            map["access_prefs"] = prefsMap;
            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            OSDMap prefsMap = (OSDMap)map["access_prefs"];
            MaxAccess = prefsMap["max"].AsString();
        }
    }

    [Serializable]
    public class DirLandReplyMessage : IMessage
    {
        public UUID AgentID;
        public UUID QueryID;

        [Serializable]
        public class QueryReply
        {
            public int ActualArea;
            public bool Auction;
            public bool ForSale;
            public string Name;
            public UUID ParcelID;
            public string ProductSku;
            public int SalePrice;
        }

        public QueryReply[] QueryReplies;

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>An <see cref="OSDMap"/> containing the objects data</returns>
        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);

            OSDMap agentMap = new OSDMap(1);
            agentMap["AgentID"] = OSD.FromUUID(AgentID);
            OSDArray agentDataArray = new OSDArray(1);
            agentDataArray.Add(agentMap);
            map["AgentData"] = agentDataArray;

            OSDMap queryMap = new OSDMap(1);
            queryMap["QueryID"] = OSD.FromUUID(QueryID);
            OSDArray queryDataArray = new OSDArray(1);
            queryDataArray.Add(queryMap);
            map["QueryData"] = queryDataArray;

            OSDArray queryReplyArray = new OSDArray();
            for (int i = 0; i < QueryReplies.Length; i++)
            {
                OSDMap queryReply = new OSDMap(100);
                queryReply["ActualArea"] = OSD.FromInteger(QueryReplies[i].ActualArea);
                queryReply["Auction"] = OSD.FromBoolean(QueryReplies[i].Auction);
                queryReply["ForSale"] = OSD.FromBoolean(QueryReplies[i].ForSale);
                queryReply["Name"] = OSD.FromString(QueryReplies[i].Name);
                queryReply["ParcelID"] = OSD.FromUUID(QueryReplies[i].ParcelID);
                queryReply["ProductSKU"] = OSD.FromString(QueryReplies[i].ProductSku);
                queryReply["SalePrice"] = OSD.FromInteger(QueryReplies[i].SalePrice);

                queryReplyArray.Add(queryReply);
            }
            map["QueryReplies"] = queryReplyArray;

            return map;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="map">An <see cref="OSDMap"/> containing the data</param>
        public void Deserialize(OSDMap map)
        {
            OSDArray agentDataArray = (OSDArray)map["AgentData"];
            OSDMap agentDataMap = (OSDMap)agentDataArray[0];
            AgentID = agentDataMap["AgentID"].AsUUID();

            OSDArray queryDataArray = (OSDArray)map["QueryData"];
            OSDMap queryDataMap = (OSDMap)queryDataArray[0];
            QueryID = queryDataMap["QueryID"].AsUUID();

            OSDArray queryRepliesArray = (OSDArray)map["QueryReplies"];

            QueryReplies = new QueryReply[queryRepliesArray.Count];
            for (int i = 0; i < queryRepliesArray.Count; i++)
            {
                QueryReply reply = new QueryReply();
                OSDMap replyMap = (OSDMap)queryRepliesArray[i];
                reply.ActualArea = replyMap["ActualArea"].AsInteger();
                reply.Auction = replyMap["Auction"].AsBoolean();
                reply.ForSale = replyMap["ForSale"].AsBoolean();
                reply.Name = replyMap["Name"].AsString();
                reply.ParcelID = replyMap["ParcelID"].AsUUID();
                reply.ProductSku = replyMap["ProductSKU"].AsString();
                reply.SalePrice = replyMap["SalePrice"].AsInteger();

                QueryReplies[i] = reply;
            }
        }
    }

    #endregion
}
