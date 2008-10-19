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
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Capabilities;
using OpenMetaverse.Packets;

namespace OpenMetaverse
{
    #region Enums

    /// <summary>
    /// Map layer request type
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
    /// Type of grid item, such as telehub, event, populator location, etc.
    /// </summary>
    public enum GridItemType : uint
    {
        /// <summary>Telehub</summary>
        Telehub = 1,
        /// <summary>PG rated event</summary>
        PgEvent = 2,
        /// <summary>Mature rated event</summary>
        MatureEvent = 3,
        /// <summary>Popular location</summary>
        Popular = 4,
        /// <summary>Location belonging to the current agent</summary>
        AgentLocations = 6,
        /// <summary>Land for sale</summary>
        LandForSale = 7,
        /// <summary>Classified ad</summary>
        Classified = 8
    }

    #endregion Enums

    #region Structs

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
		public SimAccess Access;
        /// <summary>Appears to always be zero (None)</summary>
        public RegionFlags RegionFlags;
        /// <summary>Sim's defined Water Height</summary>
		public byte WaterHeight;
        /// <summary></summary>
		public byte Agents;
        /// <summary>UUID of the World Map image</summary>
		public UUID MapImageID;
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
        public UUID ImageID;

        public bool ContainsRegion(int x, int y)
        {
            return (x >= Left && x <= Right && y >= Bottom && y <= Top);
        }
    }

    #endregion Structs

    #region Grid Item Classes

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
            get { return Utils.UIntsToLong((uint)(GlobalX - (GlobalX % 256)), (uint)(GlobalY - (GlobalY % 256))); }
        }
    }

    #endregion Grid Item Classes

    /// <summary>
	/// Manages grid-wide tasks such as the world map
	/// </summary>
	public class GridManager
    {
        #region Delegates

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sim"></param>
        public delegate void CoarseLocationUpdateCallback(Simulator sim);
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="regionID"></param>
        /// <param name="regionHandle"></param>
        public delegate void RegionHandleReplyCallback(UUID regionID, ulong regionHandle);

        #endregion Delegates

        #region Events

        /// <summary>Triggered when coarse locations (minimap dots) are updated by the simulator</summary>
        public event CoarseLocationUpdateCallback OnCoarseLocationUpdate;
        /// <summary>Triggered when a new region is discovered through GridManager</summary>
        public event GridRegionCallback OnGridRegion;
        /// <summary></summary>
        public event GridLayerCallback OnGridLayer;
        /// <summary></summary>
        public event GridItemsCallback OnGridItems;
        /// <summary></summary>
        public event RegionHandleReplyCallback OnRegionHandleReply;

        #endregion Events

        /// <summary>Unknown</summary>
        public float SunPhase { get { return sunPhase; } }
		/// <summary>Current direction of the sun</summary>
        public Vector3 SunDirection { get { return sunDirection; } }
        /// <summary>Current angular velocity of the sun</summary>
        public Vector3 SunAngVelocity { get { return sunAngVelocity; } }

        /// <summary>A dictionary of all the regions, indexed by region name</summary>
        internal Dictionary<string, GridRegion> Regions = new Dictionary<string, GridRegion>();
        /// <summary>A dictionary of all the regions, indexed by region handle</summary>
        internal Dictionary<ulong, GridRegion> RegionsByHandle = new Dictionary<ulong, GridRegion>();

		private GridClient Client;
        private float sunPhase;
        private Vector3 sunDirection;
        private Vector3 sunAngVelocity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client">Instance of GridClient object to associate with this GridManager instance</param>
		public GridManager(GridClient client)
		{
			Client = client;

            Client.Network.RegisterCallback(PacketType.MapBlockReply, new NetworkManager.PacketCallback(MapBlockReplyHandler));
            Client.Network.RegisterCallback(PacketType.MapItemReply, new NetworkManager.PacketCallback(MapItemReplyHandler));
            Client.Network.RegisterCallback(PacketType.SimulatorViewerTimeMessage, new NetworkManager.PacketCallback(TimeMessageHandler));
            Client.Network.RegisterCallback(PacketType.CoarseLocationUpdate, new NetworkManager.PacketCallback(CoarseLocationHandler));
            Client.Network.RegisterCallback(PacketType.RegionIDAndHandleReply, new NetworkManager.PacketCallback(RegionHandleReplyHandler));
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
            request.NameData.Name = Utils.StringToBytes(regionName.ToLower());

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
        /// Request the region handle for the specified region UUID
        /// </summary>
        /// <param name="regionID">UUID of the region to look up</param>
        public void RequestRegionHandle(UUID regionID)
        {
            RegionHandleRequestPacket request = new RegionHandleRequestPacket();
            request.RequestBlock = new RegionHandleRequestPacket.RequestBlockBlock();
            request.RequestBlock.RegionID = regionID;
            Client.Network.SendPacket(request);
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
                Logger.Log("GetGridRegion called with a null or empty region name", Helpers.LogLevel.Error, Client);
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
                    Logger.Log("Couldn't find region " + name, Helpers.LogLevel.Warning, Client);
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
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }

            if (body.ContainsKey("MapBlocks"))
            {
                // TODO: At one point this will become activated
                Logger.Log("Got MapBlocks through CAPS, please finish this function!", Helpers.LogLevel.Error, Client);
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
                    region.Name = Utils.BytesToString(block.Name);
                    // RegionFlags seems to always be zero here?
                    region.RegionFlags = (RegionFlags)block.RegionFlags;
                    region.WaterHeight = block.WaterHeight;
                    region.Agents = block.Agents;
                    region.Access = (SimAccess)block.Access;
                    region.MapImageID = block.MapImageID;
                    region.RegionHandle = Utils.UIntsToLong((uint)(region.X * 256), (uint)(region.Y * 256));

                    lock (Regions)
                    {
                        Regions[region.Name.ToLower()] = region;
                        RegionsByHandle[region.RegionHandle] = region;
                    }

                    if (OnGridRegion != null)
                    {
                        try { OnGridRegion(region); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
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
                    string name = Utils.BytesToString(reply.Data[i].Name);

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
                            Logger.Log("FIXME", Helpers.LogLevel.Error, Client);
                            break;
                        case GridItemType.LandForSale:
                            //FIXME:
                            Logger.Log("FIXME", Helpers.LogLevel.Error, Client);
                            break;
                        case GridItemType.MatureEvent:
                        case GridItemType.PgEvent:
                            //FIXME:
                            Logger.Log("FIXME", Helpers.LogLevel.Error, Client);
                            break;
                        case GridItemType.Popular:
                            //FIXME:
                            Logger.Log("FIXME", Helpers.LogLevel.Error, Client);
                            break;
                        case GridItemType.Telehub:
                            //FIXME:
                            Logger.Log("FIXME", Helpers.LogLevel.Error, Client);
                            break;
                        default:
                            Logger.Log("Unknown map item type " + type, Helpers.LogLevel.Warning, Client);
                            break;
                    }
                }

                try { OnGridItems(type, items); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
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
                    simulator.avatarPositions.Add(new Vector3(coarse.Location[i].X, coarse.Location[i].Y,
                        coarse.Location[i].Z * 4));
                }

                if (OnCoarseLocationUpdate != null)
                {
                    try { OnCoarseLocationUpdate(simulator); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }
        }

        private void RegionHandleReplyHandler(Packet packet, Simulator simulator)
        {
            RegionIDAndHandleReplyPacket reply = (RegionIDAndHandleReplyPacket)packet;
            if (OnRegionHandleReply != null)
            {
                try { OnRegionHandleReply(reply.ReplyBlock.RegionID, reply.ReplyBlock.RegionHandle); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

    }
}
