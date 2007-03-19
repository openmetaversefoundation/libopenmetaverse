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
    public delegate void GridRegionCallback(GridRegion region);

	/// <summary>
	/// Region information returned from the spaceserver, used for the world map
	/// </summary>
	public class GridRegion
	{
        /// <summary>Sim X position on World Map</summary>
		public int X;
        /// <summary>Sim Y position on World Map</summary>
		public int Y;
        /// <summary>Sim Name (NOTE: In lowercase!)</summary>
		public string Name;
        /// <summary></summary> 
		public byte Access;
        /// <summary>Various flags for the region (presumably things like PG/Mature)</summary>
		public uint RegionFlags;
        /// <summary>Sim's defined Water Height</summary>
		public byte WaterHeight;
        /// <summary></summary>
		public byte Agents;
        /// <summary>UUID of the World Map image</summary>
		public LLUUID MapImageID;
        /// <summary>Unique identifier for this region, a combination of the X 
        /// and Y position</summary>
		public ulong RegionHandle;

        /// <summary>
        /// Constructor
        /// </summary>
		public GridRegion() 
		{
		}

        public override string ToString()
        {
            string output = "GridRegion";
            output += Environment.NewLine + "Name: " + Name;
            output += Environment.NewLine + "RegionHandle: " + RegionHandle;
            output += Environment.NewLine + "X: " + X;
            output += Environment.NewLine + "Y: " + Y;
            output += Environment.NewLine + "MapImageID: " + MapImageID;
            output += Environment.NewLine + "Access: " + Access;
            output += Environment.NewLine + "RegionFlags: " + RegionFlags;
            output += Environment.NewLine + "WaterHeight: " + WaterHeight;
            output += Environment.NewLine + "Agents: " + Agents;

            return output;
        }
	}

	/// <summary>
	/// Manages grid-wide tasks such as the world map
	/// </summary>
	public class GridManager
	{
        public enum MapLayerType : uint
        {
            Objects = 0,
            Terrain = 1
        }


        /// <summary>
        /// Triggered when a new region is discovered through GridManager
        /// </summary>
        public event GridRegionCallback OnRegionAdd;

        // FIXME: These publically accessible dictionaries are a recipe for multi-threading disaster

        /// <summary>A dictionary of all the regions, indexed by region ID</summary>
		public Dictionary<string, GridRegion> Regions = new Dictionary<string, GridRegion>();
		/// <summary>A dictionary of all the regions, indexed by region handle</summary>
		public Dictionary<ulong, GridRegion> RegionsByHandle = new Dictionary<ulong,GridRegion>();
        /// <summary>Unknown</summary>
        public float SunPhase { get { return sunPhase; } }
		/// <summary>Current direction of the sun</summary>
        public LLVector3 SunDirection { get { return sunDirection; } }
        /// <summary>Current angular velocity of the sun</summary>
        public LLVector3 SunAngVelocity { get { return sunAngVelocity; } }

		private SecondLife Client;
        private float sunPhase = 0.0f;
        private LLVector3 sunDirection = LLVector3.Zero;
        private LLVector3 sunAngVelocity = LLVector3.Zero;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client">Instance of type SecondLife to associate with this GridManager instance</param>
		public GridManager(SecondLife client)
		{
			Client = client;

            Client.Network.RegisterCallback(PacketType.MapBlockReply, new NetworkManager.PacketCallback(MapBlockReplyHandler));
            Client.Network.RegisterCallback(PacketType.SimulatorViewerTimeMessage, new NetworkManager.PacketCallback(TimeMessageHandler));
            Client.Network.RegisterCallback(PacketType.CoarseLocationUpdate, new NetworkManager.PacketCallback(CoarseLocationHandler));
		}

        /// <summary>
        /// Fire off packet for Estate/Island sim data request.
        /// </summary>
        public void RequestEstateSims(MapLayerType layer)
        {
            MapLayerRequestPacket request = new MapLayerRequestPacket();

            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;
            request.AgentData.Godlike = true;
            request.AgentData.Flags = (uint)layer;
            request.AgentData.EstateID = 0; // TODO get a better value here.

            Client.Network.SendPacket(request);
        }

        /// <summary>
        /// Fire off packet for Linden/Mainland sim data request.
        /// </summary>
        public void RequestLindenSims(MapLayerType layer)
        {
            MapBlockRequestPacket request = new MapBlockRequestPacket();

            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;
            request.AgentData.EstateID = 0; // TODO: ?
            request.AgentData.Flags = (uint)layer;
            request.PositionData.MaxX = 65535;
            request.PositionData.MaxY = 65535;
            request.PositionData.MinX = 0;
            request.PositionData.MinY = 0;

            Client.Network.SendPacket(request);
        }

        /// <summary>
        /// Send Request Packets for lists of Linden ('mainland') and Estate (Island) sims.
        /// <remarks>
        /// LL's protocol for some reason uses a different request packet for Estate sims.
        /// </remarks>
        /// </summary>
		public void RequestAllSims(MapLayerType layer) 
		{
            RequestLindenSims(layer);
            RequestEstateSims(layer);
		}

        /// <summary>
        /// Begin process to get information for a Region
        /// </summary>
        /// <param name="name">Region name you're requesting data for</param>
        public void BeginGetGridRegion(string name)
        {
            MapNameRequestPacket map = new MapNameRequestPacket();

            map.AgentData.AgentID = Client.Network.AgentID;
            map.AgentData.SessionID = Client.Network.SessionID;
            map.NameData.Name = Helpers.StringToField(name.ToLower());

            Client.Network.SendPacket(map);
        }

        /// <summary>
        /// Get grid region information using the region name, this function
        /// will block until it can find the region or gives up
        /// </summary>
        /// <param name="name">Name of sim you're looking for</param>
        /// <returns>GridRegion for the sim you're looking for, or null if it's not available</returns>
        /// <example>GridRegion regiondata = GetGridRegion("Ahern");</example>
		public GridRegion GetGridRegion(string name) 
		{
            name = name.ToLower();

            if (Regions.ContainsKey(name))
            {
                return Regions[name];
            }
            else
            {
                BeginGetGridRegion(name);

                // FIXME: We shouldn't be sleeping in a library call, hopefully this goes away soon
                System.Threading.Thread.Sleep(5000);

                if (Regions.ContainsKey(name))
                {
                    return Regions[name];
                }
                else
                {
                    Client.Log("Couldn't find region " + name, Helpers.LogLevel.Warning);
                    return null;
                }
            }
		}

        /// <summary>
        /// Populate Grid info based on data from MapBlockReplyPacket
        /// </summary>
        /// <param name="packet">Incoming MapBlockReplyPacket packet</param>
        /// <param name="simulator">Unused</param>
		private void MapBlockReplyHandler(Packet packet, Simulator simulator) 
		{
			GridRegion region;
            MapBlockReplyPacket map = (MapBlockReplyPacket)packet;

            foreach (MapBlockReplyPacket.DataBlock block in map.Data)
            {
                if (block.X != 0 && block.Y != 0)
                {
                    region = new GridRegion();

                    region.X = block.X;
                    region.Y = block.Y;
                    region.Name = Helpers.FieldToUTF8String(block.Name);
                    region.RegionFlags = block.RegionFlags;
                    region.WaterHeight = block.WaterHeight;
                    region.Agents = block.Agents;
                    region.Access = block.Access;
                    region.MapImageID = block.MapImageID;
                    region.RegionHandle = Helpers.UIntsToLong((uint)region.X * (uint)256, (uint)region.Y * (uint)256);

                    lock (Regions) Regions[region.Name.ToLower()] = region;
					lock (RegionsByHandle) RegionsByHandle[region.RegionHandle] = region;

                    if (OnRegionAdd != null)
                    {
                        try { OnRegionAdd(region); }
                        catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                    }
                }
            }
		}

        /// <summary>
        /// Get sim time from the appropriate packet
        /// </summary>
        /// <param name="packet">Incoming SimulatorViewerTimeMessagePacket from SL</param>
        /// <param name="simulator">Unused</param>
        private void TimeMessageHandler(Packet packet, Simulator simulator)
        {
            SimulatorViewerTimeMessagePacket time = (SimulatorViewerTimeMessagePacket)packet;
            
            sunPhase = time.TimeInfo.SunPhase;
            sunDirection = time.TimeInfo.SunDirection;
            sunAngVelocity = time.TimeInfo.SunAngVelocity;
            
            // TODO: Does anyone have a use for the time stuff?
        }

        private void CoarseLocationHandler(Packet packet, Simulator simulator)
        {
            CoarseLocationUpdatePacket coarse = (CoarseLocationUpdatePacket)packet;

            lock (simulator.avatarPositions)
            {
                simulator.avatarPositions.Clear();

                for (int i = 0; i < coarse.Location.Length; i++)
                {
                    if (i == coarse.Index.Prey)
                    {
                        // TODO: Handle the coarse target position
                    }
                    else if (i != coarse.Index.You)
                    {
                        simulator.avatarPositions.Add(new LLVector3(coarse.Location[i].X, coarse.Location[i].Y,
                            coarse.Location[i].Z));
                    }
                }
            }
        }
    }
}
