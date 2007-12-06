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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using libsecondlife.StructuredData;
using libsecondlife.Capabilities;
using libsecondlife.Packets;

namespace libsecondlife
{
    /// <summary>
    /// 
    /// </summary>
    public enum GridLayerType : uint
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
    public enum GridItemType : uint
    {
        Telehub = 1,
        PgEvent = 2,
        MatureEvent = 3,
        Popular = 4,
        AgentLocations = 6,
        LandForSale = 7,
        Classified = 8
    }

	/// <summary>
	/// Information about a region on the grid map
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
            output.Append(Name); output.Append(Helpers.NewLine);
            output.Append("RegionHandle: " + RegionHandle); output.Append(Helpers.NewLine);
            output.Append(String.Format("X: {0} Y: {1}", X, Y)); output.Append(Helpers.NewLine);
            output.Append("MapImageID: " + MapImageID.ToString()); output.Append(Helpers.NewLine);
            output.Append("Access: " + Access); output.Append(Helpers.NewLine);
            output.Append("RegionFlags: " + RegionFlags); output.Append(Helpers.NewLine);
            output.Append("WaterHeight: " + WaterHeight); output.Append(Helpers.NewLine);
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
    /// Visual chunk of the grid map
    /// </summary>
    public struct GridLayer
    {
        public int Bottom;
        public int Left;
        public int Top;
        public int Right;
        public LLUUID ImageID;

        public bool ContainsRegion(int x, int y)
        {
            return (x >= Left && x <= Right && y >= Bottom && y <= Top);
        }
    }

    public abstract class GridItem
    {
    }

    public class GridAgentLocation : GridItem
    {
        public uint GlobalX;
        public uint GlobalY;
        public int AvatarCount;
        public string Identifier;

        public uint LocalX { get { return GlobalX % 256; } }
        public uint LocalY { get { return GlobalY % 256; } }

        public ulong RegionHandle
        {
            get { return Helpers.UIntsToLong((uint)(GlobalX - (GlobalX % 256)), (uint)(GlobalY - (GlobalY % 256))); }
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
        /// <param name="region"></param>
        public delegate void GridRegionCallback(GridRegion region);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer"></param>
        public delegate void GridLayerCallback(GridLayer layer);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="items"></param>
        public delegate void GridItemsCallback(GridItemType type, List<GridItem> items);

        /// <summary>Triggered when a new region is discovered through GridManager</summary>
        public event GridRegionCallback OnGridRegion;
        /// <summary></summary>
        public event GridLayerCallback OnGridLayer;
        /// <summary></summary>
        public event GridItemsCallback OnGridItems;

        /// <summary>Unknown</summary>
        public float SunPhase { get { return sunPhase; } }
		/// <summary>Current direction of the sun</summary>
        public LLVector3 SunDirection { get { return sunDirection; } }
        /// <summary>Current angular velocity of the sun</summary>
        public LLVector3 SunAngVelocity { get { return sunAngVelocity; } }

        /// <summary>A dictionary of all the regions, indexed by region name</summary>
        internal Dictionary<string, GridRegion> Regions = new Dictionary<string, GridRegion>();
        /// <summary>A dictionary of all the regions, indexed by region handle</summary>
        internal Dictionary<ulong, GridRegion> RegionsByHandle = new Dictionary<ulong, GridRegion>();

		private SecondLife Client;
        private float sunPhase;
        private LLVector3 sunDirection;
        private LLVector3 sunAngVelocity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client">Instance of type SecondLife to associate with this GridManager instance</param>
		public GridManager(SecondLife client)
		{
			Client = client;

            Client.Network.RegisterCallback(PacketType.MapBlockReply, new NetworkManager.PacketCallback(MapBlockReplyHandler));
            Client.Network.RegisterCallback(PacketType.MapItemReply, new NetworkManager.PacketCallback(MapItemReplyHandler));
            Client.Network.RegisterCallback(PacketType.SimulatorViewerTimeMessage, new NetworkManager.PacketCallback(TimeMessageHandler));
            Client.Network.RegisterCallback(PacketType.CoarseLocationUpdate, new NetworkManager.PacketCallback(CoarseLocationHandler));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer"></param>
        public void RequestMapLayer(GridLayerType layer)
        {
            Uri url = Client.Network.CurrentSim.Caps.CapabilityURI("MapLayer");

            if (url != null)
            {
                LLSDMap body = new LLSDMap();
                body["Flags"] = LLSD.FromInteger((int)layer);

                CapsClient request = new CapsClient(url);
                request.OnComplete += new CapsClient.CompleteCallback(MapLayerResponseHandler);
                request.StartRequest(body);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regionName"></param>
        /// <param name="layer"></param>
        public void RequestMapRegion(string regionName, GridLayerType layer)
        {
            MapNameRequestPacket request = new MapNameRequestPacket();

            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.AgentData.Flags = (uint)layer;
            request.AgentData.EstateID = 0; // Filled in on the sim
            request.AgentData.Godlike = false; // Filled in on the sim
            request.NameData.Name = Helpers.StringToField(regionName.ToLower());

            Client.Network.SendPacket(request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="minX"></param>
        /// <param name="minY"></param>
        /// <param name="maxX"></param>
        /// <param name="maxY"></param>
        /// <param name="returnNonExistent"></param>
        public void RequestMapBlocks(GridLayerType layer, ushort minX, ushort minY, ushort maxX, ushort maxY, 
            bool returnNonExistent)
        {
            MapBlockRequestPacket request = new MapBlockRequestPacket();

            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
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
        /// 
        /// </summary>
        /// <param name="regionHandle"></param>
        /// <param name="item"></param>
        /// <param name="layer"></param>
        /// <param name="timeoutMS"></param>
        /// <returns></returns>
        public List<GridItem> MapItems(ulong regionHandle, GridItemType item, GridLayerType layer, int timeoutMS)
        {
            List<GridItem> itemList = null;
            AutoResetEvent itemsEvent = new AutoResetEvent(false);

            GridItemsCallback callback =
                delegate(GridItemType type, List<GridItem> items)
                {
                    if (type == GridItemType.AgentLocations)
                    {
                        itemList = items;
                        itemsEvent.Set();
                    }
                };

            OnGridItems += callback;

            RequestMapItems(regionHandle, item, layer);
            itemsEvent.WaitOne(timeoutMS, false);

            OnGridItems -= callback;

            return itemList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regionHandle"></param>
        /// <param name="item"></param>
        /// <param name="layer"></param>
        public void RequestMapItems(ulong regionHandle, GridItemType item, GridLayerType layer)
        {
            MapItemRequestPacket request = new MapItemRequestPacket();
            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.AgentData.Flags = (uint)layer;
            request.AgentData.Godlike = false; // Filled in on the sim
            request.AgentData.EstateID = 0; // Filled in on the sim

            request.RequestData.ItemType = (uint)item;
            request.RequestData.RegionHandle = regionHandle;

            Client.Network.SendPacket(request);
        }

        /// <summary>
        /// Request data for all mainland (Linden managed) simulators
        /// </summary>
        public void RequestMainlandSims(GridLayerType layer)
        {
            RequestMapBlocks(layer, 0, 0, 65535, 65535, false);
        }

        /// <summary>
        /// Get grid region information using the region name, this function
        /// will block until it can find the region or gives up
        /// </summary>
        /// <param name="name">Name of sim you're looking for</param>
        /// <param name="layer">Layer that you are requesting</param>
        /// <param name="region">Will contain a GridRegion for the sim you're
        /// looking for if successful, otherwise an empty structure</param>
        /// <returns>True if the GridRegion was successfully fetched, otherwise
        /// false</returns>
        public bool GetGridRegion(string name, GridLayerType layer, out GridRegion region)
        {
            if (String.IsNullOrEmpty(name))
            {
                Client.Log("GetGridRegion called with a null or empty region name", Helpers.LogLevel.Error);
                region = new GridRegion();
                return false;
            }

            // All lookups are done using lowercase sim names
            name = name.ToLower();

            if (Regions.ContainsKey(name))
            {
                // We already have this GridRegion structure
                region = Regions[name];
                return true;
            }
            else
            {
                AutoResetEvent regionEvent = new AutoResetEvent(false);
                GridRegionCallback callback =
                    delegate(GridRegion gridRegion)
                    {
                        if (gridRegion.Name == name)
                            regionEvent.Set();
                    };
                OnGridRegion += callback;

                RequestMapRegion(name, layer);
                regionEvent.WaitOne(Client.Settings.MAP_REQUEST_TIMEOUT, false);

                OnGridRegion -= callback;

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

        private void MapLayerResponseHandler(CapsClient client, LLSD result, Exception error)
        {
            LLSDMap body = (LLSDMap)result;
            LLSDArray layerData = (LLSDArray)body["LayerData"];

            if (OnGridLayer != null)
            {
                for (int i = 0; i < layerData.Count; i++)
                {
                    LLSDMap thisLayerData = (LLSDMap)layerData[i];

                    GridLayer layer;
                    layer.Bottom = thisLayerData["Bottom"].AsInteger();
                    layer.Left = thisLayerData["Left"].AsInteger();
                    layer.Top = thisLayerData["Top"].AsInteger();
                    layer.Right = thisLayerData["Right"].AsInteger();
                    layer.ImageID = thisLayerData["ImageID"].AsUUID();

                    try { OnGridLayer(layer); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
            }

            if (body.ContainsKey("MapBlocks"))
            {
                // TODO: At one point this will become activated
                Client.Log("Got MapBlocks through CAPS, please finish this function!", Helpers.LogLevel.Error);
            }
        }

        /// <summary>
        /// Populate Grid info based on data from MapBlockReplyPacket
        /// </summary>
        /// <param name="packet">Incoming MapBlockReplyPacket packet</param>
        /// <param name="simulator">Unused</param>
        private void MapBlockReplyHandler(Packet packet, Simulator simulator)
        {
            MapBlockReplyPacket map = (MapBlockReplyPacket)packet;

            foreach (MapBlockReplyPacket.DataBlock block in map.Data)
            {
                if (block.X != 0 && block.Y != 0)
                {
                    GridRegion region;

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

                    lock (Regions)
                    {
                        Regions[region.Name.ToLower()] = region;
                        RegionsByHandle[region.RegionHandle] = region;
                    }

                    if (OnGridRegion != null)
                    {
                        try { OnGridRegion(region); }
                        catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                    }
                }
            }
        }

        private void MapItemReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnGridItems != null)
            {
                MapItemReplyPacket reply = (MapItemReplyPacket)packet;
                GridItemType type = (GridItemType)reply.RequestData.ItemType;
                List<GridItem> items = new List<GridItem>();

                for (int i = 0; i < reply.Data.Length; i++)
                {
                    string name = Helpers.FieldToUTF8String(reply.Data[i].Name);

                    switch (type)
                    {
                        case GridItemType.AgentLocations:
                            GridAgentLocation location = new GridAgentLocation();
                            location.GlobalX = reply.Data[i].X;
                            location.GlobalY = reply.Data[i].Y;
                            location.Identifier = name;
                            location.AvatarCount = reply.Data[i].Extra;

                            items.Add(location);

                            break;
                        case GridItemType.Classified:
                            //FIXME:
                            Client.Log("FIXME", Helpers.LogLevel.Error);
                            break;
                        case GridItemType.LandForSale:
                            //FIXME:
                            Client.Log("FIXME", Helpers.LogLevel.Error);
                            break;
                        case GridItemType.MatureEvent:
                        case GridItemType.PgEvent:
                            //FIXME:
                            Client.Log("FIXME", Helpers.LogLevel.Error);
                            break;
                        case GridItemType.Popular:
                            //FIXME:
                            Client.Log("FIXME", Helpers.LogLevel.Error);
                            break;
                        case GridItemType.Telehub:
                            //FIXME:
                            Client.Log("FIXME", Helpers.LogLevel.Error);
                            break;
                        default:
                            Client.Log("Unknown map item type " + type, Helpers.LogLevel.Warning);
                            break;
                    }
                }

                try { OnGridItems(type, items); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
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
                    if (i == coarse.Index.You)
                    {
                        simulator.positionIndexYou = i;
                    }
                    else if (i == coarse.Index.Prey)
                    {
                        simulator.positionIndexPrey = i;
                    }
                    simulator.avatarPositions.Add(new LLVector3(coarse.Location[i].X, coarse.Location[i].Y,
                        coarse.Location[i].Z));
                }
            }
        }
    }
}
