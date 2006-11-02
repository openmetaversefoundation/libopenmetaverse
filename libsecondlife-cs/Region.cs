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
using libsecondlife.Packets;

namespace libsecondlife
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="region"></param>
    public delegate void ParcelCompleteCallback(Region region);

    /// <summary>
    /// Represents a region (also known as a sim) in Second Life.
    /// </summary>
    public class Region
    {
        /// <summary></summary>
        public event ParcelCompleteCallback OnParcelCompletion;

        // FIXME: This whole setup is fscked in a really bad way. We can't be 
        // locking on a publically accessible container, and we shouldn't have
        // publically accessible containers anyways because external programs 
        // might be iterating through them or modifying them when internally 
        // we are doing the opposite. The best way to fix this will be 
        // privatizing and adding helper functions to access the dictionary
        public Dictionary<int, Parcel> Parcels;

        /// <summary></summary>
        public LLUUID ID;
        /// <summary></summary>
        public ulong Handle;
        /// <summary></summary>
        public string Name;
        /// <summary></summary>
        public byte[] ParcelOverlay;
        /// <summary></summary>
        public int ParcelOverlaysReceived;
        /// <summary>64x64 Array of parcels which have been successfully downloaded 
        /// (and their LocalID's, 0 = Null)</summary>
        public int[,] ParcelMarked;
        /// <summary>Flag to indicate whether we are downloading a sim's parcels</summary>
        public bool ParcelDownloading;
        /// <summary>Flag to indicate whether to get Dwell values automatically (NOT USED YET). Call Parcel.GetDwell() instead</summary>
        public bool ParcelDwell;
        /// <summary></summary>
        public float TerrainHeightRange00;
        /// <summary></summary>
        public float TerrainHeightRange01;
        /// <summary></summary>
        public float TerrainHeightRange10;
        /// <summary></summary>
        public float TerrainHeightRange11;
        /// <summary></summary>
        public float TerrainStartHeight00;
        /// <summary></summary>
        public float TerrainStartHeight01;
        /// <summary></summary>
        public float TerrainStartHeight10;
        /// <summary></summary>
        public float TerrainStartHeight11;
        /// <summary></summary>
        public float WaterHeight;
        /// <summary></summary>
        public LLUUID SimOwner;
        /// <summary></summary>
        public LLUUID TerrainBase0;
        /// <summary></summary>
        public LLUUID TerrainBase1;
        /// <summary></summary>
        public LLUUID TerrainBase2;
        /// <summary></summary>
        public LLUUID TerrainBase3;
        /// <summary></summary>
        public LLUUID TerrainDetail0;
        /// <summary></summary>
        public LLUUID TerrainDetail1;
        /// <summary></summary>
        public LLUUID TerrainDetail2;
        /// <summary></summary>
        public LLUUID TerrainDetail3;
        /// <summary></summary>
        public bool IsEstateManager;
        /// <summary></summary>
        public EstateTools Estate;

        private SecondLife Client;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public Region(SecondLife client)
        {
            Estate = new EstateTools(client);
            Client = client;
            ID = new LLUUID();
            ParcelOverlay = new byte[4096];
            ParcelMarked = new int[64, 64];

            Parcels = new Dictionary<int, Parcel>();

            SimOwner = new LLUUID();
            TerrainBase0 = new LLUUID();
            TerrainBase1 = new LLUUID();
            TerrainBase2 = new LLUUID();
            TerrainBase3 = new LLUUID();
            TerrainDetail0 = new LLUUID();
            TerrainDetail1 = new LLUUID();
            TerrainDetail2 = new LLUUID();
            TerrainDetail3 = new LLUUID();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="id"></param>
        /// <param name="handle"></param>
        /// <param name="name"></param>
        /// <param name="heightList"></param>
        /// <param name="simOwner"></param>
        /// <param name="terrainImages"></param>
        /// <param name="isEstateManager"></param>
        public Region(SecondLife client, LLUUID id, ulong handle, string name, float[] heightList,
                LLUUID simOwner, LLUUID[] terrainImages, bool isEstateManager)
        {
            Client = client;
            Estate = new EstateTools(client);
            ID = id;
            Handle = handle;
            Name = name;
            ParcelOverlay = new byte[4096];
            ParcelMarked = new int[64, 64];
            ParcelDownloading = false;
            ParcelDwell = false;

            TerrainHeightRange00 = heightList[0];
            TerrainHeightRange01 = heightList[1];
            TerrainHeightRange10 = heightList[2];
            TerrainHeightRange11 = heightList[3];
            TerrainStartHeight00 = heightList[4];
            TerrainStartHeight01 = heightList[5];
            TerrainStartHeight10 = heightList[6];
            TerrainStartHeight11 = heightList[7];
            WaterHeight = heightList[8];

            SimOwner = simOwner;

            TerrainBase0 = terrainImages[0];
            TerrainBase1 = terrainImages[1];
            TerrainBase2 = terrainImages[2];
            TerrainBase3 = terrainImages[3];
            TerrainDetail0 = terrainImages[4];
            TerrainDetail1 = terrainImages[5];
            TerrainDetail2 = terrainImages[6];
            TerrainDetail3 = terrainImages[7];

            IsEstateManager = isEstateManager;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="west"></param>
        /// <param name="south"></param>
        /// <param name="east"></param>
        /// <param name="north"></param>
        public void ParcelSubdivide(float west, float south, float east, float north)
        {
            ParcelDividePacket divide = new ParcelDividePacket();
            divide.AgentData.AgentID = Client.Network.AgentID;
            divide.AgentData.SessionID = Client.Network.SessionID;
            divide.ParcelData.East = east;
            divide.ParcelData.North = north;
            divide.ParcelData.South = south;
            divide.ParcelData.West = west;

            // FIXME: Region needs a reference to it's parent Simulator
            //Client.Network.SendPacket((Packet)divide, this.Simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="west"></param>
        /// <param name="south"></param>
        /// <param name="east"></param>
        /// <param name="north"></param>
        public void ParcelJoin(float west, float south, float east, float north)
        {
            ParcelJoinPacket join = new ParcelJoinPacket();
            join.AgentData.AgentID = Client.Network.AgentID;
            join.AgentData.SessionID = Client.Network.SessionID;
            join.ParcelData.East = east;
            join.ParcelData.North = north;
            join.ParcelData.South = south;
            join.ParcelData.West = west;

            // FIXME: Region needs a reference to it's parent Simulator
            //Client.Network.SendPacket((Packet)join, this.Simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prim"></param>
        /// <param name="position"></param>
        /// <param name="avatarPosition"></param>
        public void RezObject(PrimObject prim, LLVector3 position, LLVector3 rayStart, 
            LLUUID groupID)
        {
            ObjectAddPacket add = new ObjectAddPacket();
            add.AgentData.AgentID = Client.Network.AgentID;
            add.AgentData.SessionID = Client.Network.SessionID;
            add.AgentData.GroupID = groupID;
            // TODO: Why 2?
            add.ObjectData.AddFlags = 2;
            add.ObjectData.BypassRaycast = 1;
            add.ObjectData.Material = (byte)prim.Material;
            add.ObjectData.PathBegin = PrimObject.PathBeginByte(prim.PathBegin);
            add.ObjectData.PathCurve = (byte)prim.PathCurve;
            add.ObjectData.PathEnd = PrimObject.PathEndByte(prim.PathEnd);
            add.ObjectData.PathRadiusOffset = PrimObject.PathRadiusOffsetByte(prim.PathRadiusOffset);
            add.ObjectData.PathRevolutions = PrimObject.PathRevolutionsByte(prim.PathRevolutions);
            add.ObjectData.PathScaleX = PrimObject.PathScaleByte(prim.PathScaleX);
            add.ObjectData.PathScaleY = PrimObject.PathScaleByte(prim.PathScaleY);
            add.ObjectData.PathShearX = PrimObject.PathShearByte(prim.PathShearX);
            add.ObjectData.PathShearY = PrimObject.PathShearByte(prim.PathShearY);
            add.ObjectData.PathSkew = PrimObject.PathSkewByte(prim.PathSkew);
            add.ObjectData.PathTaperX = PrimObject.PathTaperByte(prim.PathTaperX);
            add.ObjectData.PathTaperY = PrimObject.PathTaperByte(prim.PathTaperY);
            add.ObjectData.PathTwist = PrimObject.PathTwistByte(prim.PathTwist);
            add.ObjectData.PathTwistBegin = PrimObject.PathTwistByte(prim.PathTwistBegin);
            add.ObjectData.PCode = (byte)prim.PCode;
            add.ObjectData.ProfileBegin = PrimObject.ProfileBeginByte(prim.ProfileBegin);
            add.ObjectData.ProfileCurve = (byte)prim.ProfileCurve;
            add.ObjectData.ProfileEnd = PrimObject.ProfileEndByte(prim.ProfileEnd);
            add.ObjectData.ProfileHollow = (byte)prim.ProfileHollow;
            add.ObjectData.RayEnd = position;
            add.ObjectData.RayEndIsIntersection = 0;
            add.ObjectData.RayStart = rayStart;
            add.ObjectData.RayTargetID = LLUUID.GenerateUUID();
            add.ObjectData.Rotation = prim.Rotation;
            add.ObjectData.Scale = prim.Scale;
            add.ObjectData.State = (byte)prim.State;
            if (prim.Textures != null)
            {
                add.ObjectData.TextureEntry = prim.Textures.ToBytes();
            }
            else
            {
                add.ObjectData.TextureEntry = new byte[0];
            }

            Client.Network.SendPacket(add);
        }

        /// <summary>
        /// 
        /// </summary>
        public void FillParcels()
        {
            // Begins filling parcels
            ParcelDownloading = true;

            ParcelPropertiesRequestPacket tPacket = new ParcelPropertiesRequestPacket();
            tPacket.AgentData.AgentID = Client.Self.ID;
            tPacket.AgentData.SessionID = Client.Network.SessionID;
            tPacket.ParcelData.SequenceID = -10000;
            tPacket.ParcelData.West = 0.0f;
            tPacket.ParcelData.South = 0.0f;
            tPacket.ParcelData.East = 0.0f;
            tPacket.ParcelData.North = 0.0f;

            Client.Network.SendPacket((Packet)tPacket);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetParcelDownload()
        {
            Parcels = new Dictionary<int, Parcel>();
            ParcelMarked = new int[64, 64];
        }

        /// <summary>
        /// 
        /// </summary>
        public void FilledParcels()
        {
            if (OnParcelCompletion != null)
            {
                OnParcelCompletion(this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public override bool Equals(object o)
        {
            if (!(o is Region))
            {
                return false;
            }

            Region region = (Region)o;

            return (region.ID == ID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator ==(Region lhs, Region rhs)
        {
            try
            {
                return (lhs.ID == rhs.ID);
            }
            catch (NullReferenceException)
            {
                bool lhsnull = false;
                bool rhsnull = false;

                if (lhs == null || lhs.ID == null || lhs.ID.Data == null || lhs.ID.Data.Length == 0)
                {
                    lhsnull = true;
                }

                if (rhs == null || rhs.ID == null || rhs.ID.Data == null || rhs.ID.Data.Length == 0)
                {
                    rhsnull = true;
                }

                return (lhsnull == rhsnull);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator !=(Region lhs, Region rhs)
        {
            return !(lhs == rhs);
        }
    }
}
