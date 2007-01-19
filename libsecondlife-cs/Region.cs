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
    /// Represents a region (also known as a sim) in Second Life.
    /// </summary>
    public class Region
    {
        /// <summary></summary>
        public LLUUID ID = LLUUID.Zero;
        /// <summary></summary>
        public ulong Handle;
        /// <summary></summary>
        public string Name = "";
        /// <summary></summary>
        public byte[] ParcelOverlay = new byte[4096];
        /// <summary></summary>
        public int ParcelOverlaysReceived;
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
        public LLUUID SimOwner = LLUUID.Zero;
        /// <summary></summary>
        public LLUUID TerrainBase0 = LLUUID.Zero;
        /// <summary></summary>
        public LLUUID TerrainBase1 = LLUUID.Zero;
        /// <summary></summary>
        public LLUUID TerrainBase2 = LLUUID.Zero;
        /// <summary></summary>
        public LLUUID TerrainBase3 = LLUUID.Zero;
        /// <summary></summary>
        public LLUUID TerrainDetail0 = LLUUID.Zero;
        /// <summary></summary>
        public LLUUID TerrainDetail1 = LLUUID.Zero;
        /// <summary></summary>
        public LLUUID TerrainDetail2 = LLUUID.Zero;
        /// <summary></summary>
        public LLUUID TerrainDetail3 = LLUUID.Zero;
        /// <summary></summary>
        public bool IsEstateManager;
        /// <summary></summary>
        public EstateTools Estate;

        /// <summary></summary>
        /// <remarks>This may cause your code to block while the GridRegion data is fetched for the first time</remarks>
        private GridRegion _GridRegionData = null;
        public GridRegion GridRegionData
        {
            get
            {
                if (_GridRegionData == null)
                {
                    if ((Name != null) && (!Name.Equals("")))
                    {
                        _GridRegionData = Client.Grid.GetGridRegion(Client.Network.CurrentSim.Region.Name);
                    }
                }
                return _GridRegionData;
            }
        }

        private SecondLife Client;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public Region(SecondLife client)
        {
            Estate = new EstateTools(client);
            Client = client;
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
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
