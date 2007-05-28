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
using System.Text;
using System.Collections.Generic;
using System.Threading;
using libsecondlife.Packets;

namespace libsecondlife
{
	/// <summary>
	/// Region information returned from the spaceserver, used for the world map
	/// </summary>
	public struct GridRegion
	{
        /// <summary>Sim X position on World Map</summary>
		public int X;
        /// <summary>Sim Y position on World Map</summary>
		public int Y;
        /// <summary>Sim Name (NOTE: In lowercase!)</summary>
		public string Name;
        /// <summary></summary>
		public Simulator.SimAccess Access;
        /// <summary>Appears to always be zero (None)</summary>
        public Simulator.RegionFlags RegionFlags;
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
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder("GridRegion: ");
            output.AppendLine(Name);
            output.AppendLine("RegionHandle: " + RegionHandle);
            output.AppendLine(String.Format("X: {0} Y: {1}", X, Y));
            output.AppendLine("MapImageID: " + MapImageID.ToStringHyphenated());
            output.AppendLine("Access: " + Access);
            output.AppendLine("RegionFlags: " + RegionFlags);
            output.AppendLine("WaterHeight: " + WaterHeight);
            output.Append("Agents: " + Agents);

            return output.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is GridRegion)
                return Equals((GridRegion)obj);
            else
                return false;
        }

        private bool Equals(GridRegion region)
        {
            return (this.X == region.X && this.Y == region.Y);
        }
	}

	/// <summary>
	/// Manages grid-wide tasks such as the world map
	/// </summary>
	public class GridManager
	{
        /// <summary>
        /// 
        /// </summary>
        public enum MapLayerType : uint
        {
            /// <summary>Objects and terrain are shown</summary>
            Objects = 0,
            /// <summary>Only the terrain is shown, no objects</summary>
            Terrain = 1,
            /// <summary>Overlay showing land for sale and for auction</summary>
            LandForSale = 2
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="region"></param>
        public delegate void GridRegionCallback(GridRegion region);


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
        private Dictionary<string, ManualResetEvent> RequestingRegions = new Dictionary<string, ManualResetEvent>();

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

        public void RequestMapLayer(MapLayerType layer)
        {
            //if (Client.Network.CurrentCaps.Capabilities.ContainsKey("MapLayer"))
            //if (false)
            //{
                //string url = Client.Network.CurrentCaps.Capabilities["MapLayer"];

                // FIXME: CAPS is currently disabled until the message pumps are implemented
            //}
            //else
            //{
                MapLayerRequestPacket request = new MapLayerRequestPacket();

                request.AgentData.AgentID = Client.Network.AgentID;
                request.AgentData.SessionID = Client.Network.SessionID;
                request.AgentData.Godlike = false; // Filled in at the simulator
                request.AgentData.Flags = (uint)layer;
                request.AgentData.EstateID = 0; // Filled in at the simulator

                Client.Network.SendPacket(request);
            //}
        }

        public void RequestMapRegion(string regionName)
        {
            MapNameRequestPacket request = new MapNameRequestPacket();

            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;
            request.NameData.Name = Helpers.StringToField(regionName.ToLower());

            Client.Network.SendPacket(request);
        }

        public void RequestMapBlocks(MapLayerType layer, ushort minX, ushort minY, ushort maxX, ushort maxY, 
            bool returnNonExistent)
        {
            MapBlockRequestPacket request = new MapBlockRequestPacket();

            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;
            request.AgentData.Flags = (uint)layer;
            request.AgentData.Flags |= (uint)(returnNonExistent ? 0x10000 : 0);
            request.AgentData.EstateID = 0; // Filled in at the simulator
            request.AgentData.Godlike = false; // Filled in at the simulator

            request.PositionData.MinX = minX;
            request.PositionData.MinY = minY;
            request.PositionData.MaxX = maxX;
            request.PositionData.MaxY = maxY;

            Client.Network.SendPacket(request);
        }

        /// <summary>
        /// Request data for all mainland (Linden managed) simulators
        /// </summary>
        public void RequestMainlandSims(MapLayerType layer)
        {
            RequestMapBlocks(layer, 0, 0, 65535, 65535, false);
        }

        /// <summary>
        /// Get grid region information using the region name, this function
        /// will block until it can find the region or gives up
        /// </summary>
        /// <param name="name">Name of sim you're looking for</param>
        /// <param name="region">Will contain a GridRegion for the sim you're
        /// looking for if successful, otherwise an empty structure</param>
        /// <returns>True if the GridRegion was successfully fetched, otherwise
        /// false</returns>
        /// <example>bool success = GetGridRegion("Ahern", out myGridRegion);</example>
        public bool GetGridRegion(string name, out GridRegion region)
        {
            name = name.ToLower();

            if (Regions.ContainsKey(name))
            {
                // We already have this GridRegion structure
                region = Regions[name];
                return true;
            }
            else
            {
                ManualResetEvent requestEvent = new ManualResetEvent(false);

                if (RequestingRegions.ContainsKey(name))
                {
                    Client.Log("GetGridRegion called for " + name + " multiple times, ignoring", 
                        Helpers.LogLevel.Warning);
                    region = new GridRegion();
                    return false;
                }
                else
                {
                    // Add this region request to the list of requests we are tracking
                    lock (RequestingRegions) RequestingRegions.Add(name, requestEvent);
                }

                // Make the request
                RequestMapRegion(name);

                // Wait until an answer is retrieved
                requestEvent.WaitOne(Client.Settings.MAP_REQUEST_TIMEOUT, false);

                // Remove the dictionary entry for this lookup
                lock (RequestingRegions) RequestingRegions.Remove(name);

                if (Regions.ContainsKey(name))
                {
                    // The region was found after our request
                    region = Regions[name];
                    return true;
                }
                else
                {
                    Client.Log("Couldn't find region " + name, Helpers.LogLevel.Warning);
                    region = new GridRegion();
                    return false;
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
                    // RegionFlags seems to always be zero here?
                    region.RegionFlags = (Simulator.RegionFlags)block.RegionFlags;
                    region.WaterHeight = block.WaterHeight;
                    region.Agents = block.Agents;
                    region.Access = (Simulator.SimAccess)block.Access;
                    region.MapImageID = block.MapImageID;
                    region.RegionHandle = Helpers.UIntsToLong((uint)(region.X * 256), (uint)(region.Y * 256));

                    lock (Regions) Regions[region.Name.ToLower()] = region;
					lock (RegionsByHandle) RegionsByHandle[region.RegionHandle] = region;
                    lock (RequestingRegions)
                    {
                        if (RequestingRegions.ContainsKey(region.Name.ToLower()))
                            RequestingRegions[region.Name.ToLower()].Set();
                    }

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
