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
using OpenMetaverse.Http;
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
        /// <summary>Locations of avatar groups in a region</summary>
        AgentLocations = 6,
        /// <summary>Land for sale</summary>
        LandForSale = 7,
        /// <summary>Classified ad</summary>
        Classified = 8,
        /// <summary>Adult rated event</summary>
        AdultEvent = 9,
        /// <summary>Adult land for sale</summary>
        AdultLandForSale = 10
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
            return String.Format("{0} ({1}/{2}), Handle: {3}, MapImage: {4}, Access: {5}, Flags: {6}",
                Name, X, Y, RegionHandle, MapImageID, Access, RegionFlags);
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

    #region Map Item Classes

    /// <summary>
    /// Base class for Map Items
    /// </summary>
    public abstract class MapItem
    {
        /// <summary>The Global X position of the item</summary>
        public uint GlobalX;
        /// <summary>The Global Y position of the item</summary>
        public uint GlobalY;

        /// <summary>Get the Local X position of the item</summary>
        public uint LocalX { get { return GlobalX % 256; } }
        /// <summary>Get the Local Y position of the item</summary>
        public uint LocalY { get { return GlobalY % 256; } }

        /// <summary>Get the Handle of the region</summary>
        public ulong RegionHandle
        {
            get { return Utils.UIntsToLong((uint)(GlobalX - (GlobalX % 256)), (uint)(GlobalY - (GlobalY % 256))); }
        }
    }

    /// <summary>
    /// Represents an agent or group of agents location
    /// </summary>
    public class MapAgentLocation : MapItem
    {       
        public int AvatarCount;
        public string Identifier;
    }

    /// <summary>
    /// Represents a Telehub location
    /// </summary>
    public class MapTelehub : MapItem
    {        
    }

    /// <summary>
    /// Represents a non-adult parcel of land for sale
    /// </summary>
    public class MapLandForSale : MapItem
    {        
        public int Size;
        public int Price;
        public string Name;
        public UUID ID;        
    }

    /// <summary>
    /// Represents an Adult parcel of land for sale
    /// </summary>
    public class MapAdultLandForSale : MapItem
    {     
        public int Size;
        public int Price;
        public string Name;
        public UUID ID;
    }

    /// <summary>
    /// Represents a PG Event
    /// </summary>
    public class MapPGEvent : MapItem
    {
        public DirectoryManager.EventFlags Flags; // Extra
        public DirectoryManager.EventCategories Category; // Extra2
        public string Description;
    }

    /// <summary>
    /// Represents a Mature event
    /// </summary>
    public class MapMatureEvent : MapItem
    {
        public DirectoryManager.EventFlags Flags; // Extra
        public DirectoryManager.EventCategories Category; // Extra2
        public string Description;
    }

    /// <summary>
    /// Represents an Adult event
    /// </summary>
    public class MapAdultEvent : MapItem
    {
        public DirectoryManager.EventFlags Flags; // Extra
        public DirectoryManager.EventCategories Category; // Extra2
        public string Description;
    }
    #endregion Grid Item Classes

    /// <summary>
	/// Manages grid-wide tasks such as the world map
	/// </summary>
	public class GridManager
    {
        #region Delegates

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<CoarseLocationUpdateEventArgs> m_CoarseLocationUpdate;

        /// <summary>Raises the CoarseLocationUpdate event</summary>
        /// <param name="e">A CoarseLocationUpdateEventArgs object containing the
        /// data sent by simulator</param>
        protected virtual void OnCoarseLocationUpdate(CoarseLocationUpdateEventArgs e)
        {
            EventHandler<CoarseLocationUpdateEventArgs> handler = m_CoarseLocationUpdate;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_CoarseLocationUpdateLock = new object();

        /// <summary>Raised when the simulator sends a <see cref="CoarseLocationUpdatePacket"/> 
        /// containing the location of agents in the simulator</summary>
        public event EventHandler<CoarseLocationUpdateEventArgs> CoarseLocationUpdate
        {
            add { lock (m_CoarseLocationUpdateLock) { m_CoarseLocationUpdate += value; } }
            remove { lock (m_CoarseLocationUpdateLock) { m_CoarseLocationUpdate -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<GridRegionEventArgs> m_GridRegion;

        /// <summary>Raises the GridRegion event</summary>
        /// <param name="e">A GridRegionEventArgs object containing the
        /// data sent by simulator</param>
        protected virtual void OnGridRegion(GridRegionEventArgs e)
        {
            EventHandler<GridRegionEventArgs> handler = m_GridRegion;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_GridRegionLock = new object();

        /// <summary>Raised when the simulator sends a Region Data in response to 
        /// a Map request</summary>
        public event EventHandler<GridRegionEventArgs> GridRegion
        {
            add { lock (m_GridRegionLock) { m_GridRegion += value; } }
            remove { lock (m_GridRegionLock) { m_GridRegion -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<GridLayerEventArgs> m_GridLayer;

        /// <summary>Raises the GridLayer event</summary>
        /// <param name="e">A GridLayerEventArgs object containing the
        /// data sent by simulator</param>
        protected virtual void OnGridLayer(GridLayerEventArgs e)
        {
            EventHandler<GridLayerEventArgs> handler = m_GridLayer;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_GridLayerLock = new object();

        /// <summary>Raised when the simulator sends GridLayer object containing
        /// a map tile coordinates and texture information</summary>
        public event EventHandler<GridLayerEventArgs> GridLayer
        {
            add { lock (m_GridLayerLock) { m_GridLayer += value; } }
            remove { lock (m_GridLayerLock) { m_GridLayer -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<GridItemsEventArgs> m_GridItems;

        /// <summary>Raises the GridItems event</summary>
        /// <param name="e">A GridItemEventArgs object containing the
        /// data sent by simulator</param>
        protected virtual void OnGridItems(GridItemsEventArgs e)
        {
            EventHandler<GridItemsEventArgs> handler = m_GridItems;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_GridItemsLock = new object();

        /// <summary>Raised when the simulator sends GridItems object containing
        /// details on events, land sales at a specific location</summary>
        public event EventHandler<GridItemsEventArgs> GridItems
        {
            add { lock (m_GridItemsLock) { m_GridItems += value; } }
            remove { lock (m_GridItemsLock) { m_GridItems -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<RegionHandleReplyEventArgs> m_RegionHandleReply;

        /// <summary>Raises the RegionHandleReply event</summary>
        /// <param name="e">A RegionHandleReplyEventArgs object containing the
        /// data sent by simulator</param>
        protected virtual void OnRegionHandleReply(RegionHandleReplyEventArgs e)
        {
            EventHandler<RegionHandleReplyEventArgs> handler = m_RegionHandleReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_RegionHandleReplyLock = new object();

        /// <summary>Raised in response to a Region lookup</summary>
        public event EventHandler<RegionHandleReplyEventArgs> RegionHandleReply
        {
            add { lock (m_RegionHandleReplyLock) { m_RegionHandleReply += value; } }
            remove { lock (m_RegionHandleReplyLock) { m_RegionHandleReply -= value; } }
        }

        #endregion Delegates

        /// <summary>Unknown</summary>
        public float SunPhase { get { return sunPhase; } }
		/// <summary>Current direction of the sun</summary>
        public Vector3 SunDirection { get { return sunDirection; } }
        /// <summary>Current angular velocity of the sun</summary>
        public Vector3 SunAngVelocity { get { return sunAngVelocity; } }
        /// <summary>Current world time</summary>
        public DateTime WorldTime { get { return WorldTime; } }

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

            //Client.Network.RegisterCallback(PacketType.MapLayerReply, MapLayerReplyHandler);
            Client.Network.RegisterCallback(PacketType.MapBlockReply, MapBlockReplyHandler);
            Client.Network.RegisterCallback(PacketType.MapItemReply, MapItemReplyHandler);
            Client.Network.RegisterCallback(PacketType.SimulatorViewerTimeMessage, SimulatorViewerTimeMessageHandler);
            Client.Network.RegisterCallback(PacketType.CoarseLocationUpdate, CoarseLocationHandler);
            Client.Network.RegisterCallback(PacketType.RegionIDAndHandleReply, RegionHandleReplyHandler);
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
                OSDMap body = new OSDMap();
                body["Flags"] = OSD.FromInteger((int)layer);

                CapsClient request = new CapsClient(url);
                request.OnComplete += new CapsClient.CompleteCallback(MapLayerResponseHandler);
                request.BeginGetResponse(body, OSDFormat.Xml, Client.Settings.CAPS_TIMEOUT);
            }
        }

        /// <summary>
        /// Request a map layer
        /// </summary>
        /// <param name="regionName">The name of the region</param>
        /// <param name="layer">The type of layer</param>
        public void RequestMapRegion(string regionName, GridLayerType layer)
        {
            MapNameRequestPacket request = new MapNameRequestPacket();

            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.AgentData.Flags = (uint)layer;
            request.AgentData.EstateID = 0; // Filled in on the sim
            request.AgentData.Godlike = false; // Filled in on the sim
            request.NameData.Name = Utils.StringToBytes(regionName);

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
        public List<MapItem> MapItems(ulong regionHandle, GridItemType item, GridLayerType layer, int timeoutMS)
        {
            List<MapItem> itemList = null;
            AutoResetEvent itemsEvent = new AutoResetEvent(false);

            EventHandler<GridItemsEventArgs> callback =
                delegate(object sender, GridItemsEventArgs e)
                {
                    if (e.Type == GridItemType.AgentLocations)
                    {
                        itemList = e.Items;
                        itemsEvent.Set();
                    }
                };

            GridItems += callback;

            RequestMapItems(regionHandle, item, layer);
            itemsEvent.WaitOne(timeoutMS, false);

            GridItems -= callback;

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

            if (Regions.ContainsKey(name))
            {
                // We already have this GridRegion structure
                region = Regions[name];
                return true;
            }
            else
            {
                AutoResetEvent regionEvent = new AutoResetEvent(false);
                EventHandler<GridRegionEventArgs> callback =
                    delegate(object sender, GridRegionEventArgs e)
                    {
                        if (e.Region.Name == name)
                            regionEvent.Set();
                    };
                GridRegion += callback;

                RequestMapRegion(name, layer);
                regionEvent.WaitOne(Client.Settings.MAP_REQUEST_TIMEOUT, false);

                GridRegion -= callback;

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
        
        protected void MapLayerResponseHandler(CapsClient client, OSD result, Exception error)
        {
            OSDMap body = (OSDMap)result;
            OSDArray layerData = (OSDArray)body["LayerData"];

            if (m_GridLayer != null)
            {
                for (int i = 0; i < layerData.Count; i++)
                {
                    OSDMap thisLayerData = (OSDMap)layerData[i];

                    GridLayer layer;
                    layer.Bottom = thisLayerData["Bottom"].AsInteger();
                    layer.Left = thisLayerData["Left"].AsInteger();
                    layer.Top = thisLayerData["Top"].AsInteger();
                    layer.Right = thisLayerData["Right"].AsInteger();
                    layer.ImageID = thisLayerData["ImageID"].AsUUID();

                    OnGridLayer(new GridLayerEventArgs(layer));                    
                }
            }

            if (body.ContainsKey("MapBlocks"))
            {
                // TODO: At one point this will become activated
                Logger.Log("Got MapBlocks through CAPS, please finish this function!", Helpers.LogLevel.Error, Client);
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void MapBlockReplyHandler(object sender, PacketReceivedEventArgs e)
        {
            MapBlockReplyPacket map = (MapBlockReplyPacket)e.Packet;

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
                        Regions[region.Name] = region;
                        RegionsByHandle[region.RegionHandle] = region;
                    }

                    if (m_GridRegion != null)
                    {
                        OnGridRegion(new GridRegionEventArgs(region));
                    }
                }
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void MapItemReplyHandler(object sender, PacketReceivedEventArgs e)
        {
            if (m_GridItems != null)
            {
                MapItemReplyPacket reply = (MapItemReplyPacket)e.Packet;
                GridItemType type = (GridItemType)reply.RequestData.ItemType;
                List<MapItem> items = new List<MapItem>();

                for (int i = 0; i < reply.Data.Length; i++)
                {
                    string name = Utils.BytesToString(reply.Data[i].Name);

                    switch (type)
                    {
                        case GridItemType.AgentLocations:
                            MapAgentLocation location = new MapAgentLocation();
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
                            MapLandForSale landsale = new MapLandForSale();
                            landsale.GlobalX = reply.Data[i].X;
                            landsale.GlobalY = reply.Data[i].Y;
                            landsale.ID = reply.Data[i].ID;
                            landsale.Name = name;
                            landsale.Size = reply.Data[i].Extra;
                            landsale.Price = reply.Data[i].Extra2;
                            items.Add(landsale);
                            break;
                        case GridItemType.MatureEvent:
                            MapMatureEvent matureEvent = new MapMatureEvent();
                            matureEvent.GlobalX = reply.Data[i].X;
                            matureEvent.GlobalY = reply.Data[i].Y;
                            matureEvent.Description = name;
                            matureEvent.Flags = (DirectoryManager.EventFlags)reply.Data[i].Extra2;
                            items.Add(matureEvent);
                            break;
                        case GridItemType.PgEvent:
                            MapPGEvent PGEvent = new MapPGEvent();
                            PGEvent.GlobalX = reply.Data[i].X;
                            PGEvent.GlobalY = reply.Data[i].Y;
                            PGEvent.Description = name;
                            PGEvent.Flags = (DirectoryManager.EventFlags)reply.Data[i].Extra2;
                            items.Add(PGEvent);
                            break;
                        case GridItemType.Popular:
                            //FIXME:
                            Logger.Log("FIXME", Helpers.LogLevel.Error, Client);
                            break;
                        case GridItemType.Telehub:
                            MapTelehub teleHubItem = new MapTelehub();
                            teleHubItem.GlobalX = reply.Data[i].X;
                            teleHubItem.GlobalY = reply.Data[i].Y;
                            items.Add(teleHubItem);
                            break;
                        case GridItemType.AdultLandForSale:
                            MapAdultLandForSale adultLandsale = new MapAdultLandForSale();
                            adultLandsale.GlobalX = reply.Data[i].X;
                            adultLandsale.GlobalY = reply.Data[i].Y;
                            adultLandsale.ID = reply.Data[i].ID;
                            adultLandsale.Name = name;
                            adultLandsale.Size = reply.Data[i].Extra;
                            adultLandsale.Price = reply.Data[i].Extra2;
                            items.Add(adultLandsale);
                            break;
                        case GridItemType.AdultEvent:
                            MapAdultEvent adultEvent = new MapAdultEvent();
                            adultEvent.GlobalX = reply.Data[i].X;
                            adultEvent.GlobalY = reply.Data[i].Y;
                            adultEvent.Description = Utils.BytesToString(reply.Data[i].Name);
                            adultEvent.Flags = (DirectoryManager.EventFlags)reply.Data[i].Extra2;
                            items.Add(adultEvent);
                            break;
                        default:
                            Logger.Log("Unknown map item type " + type, Helpers.LogLevel.Warning, Client);
                            break;
                    }
                }

                OnGridItems(new GridItemsEventArgs(type, items));
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void SimulatorViewerTimeMessageHandler(object sender, PacketReceivedEventArgs e)
        {
            SimulatorViewerTimeMessagePacket time = (SimulatorViewerTimeMessagePacket)e.Packet;
            
            sunPhase = time.TimeInfo.SunPhase;
            sunDirection = time.TimeInfo.SunDirection;
            sunAngVelocity = time.TimeInfo.SunAngVelocity;
            
            // TODO: Does anyone have a use for the time stuff?
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void CoarseLocationHandler(object sender, PacketReceivedEventArgs e)
        {
            CoarseLocationUpdatePacket coarse = (CoarseLocationUpdatePacket)e.Packet;

            // populate a dictionary from the packet, for local use
            Dictionary<UUID, Vector3> coarseEntries = new Dictionary<UUID, Vector3>();
            for (int i = 0; i < coarse.AgentData.Length; i++)
            {
                if(coarse.Location.Length > 0)
                    coarseEntries[coarse.AgentData[i].AgentID] = new Vector3((int)coarse.Location[i].X, (int)coarse.Location[i].Y, (int)coarse.Location[i].Z * 4);

                // the friend we are tracking on radar
                if (i == coarse.Index.Prey)
                    e.Simulator.preyID = coarse.AgentData[i].AgentID;
            }

            // find stale entries (people who left the sim)
            List<UUID> removedEntries = e.Simulator.avatarPositions.FindAll(delegate(UUID findID) { return !coarseEntries.ContainsKey(findID); });

            // anyone who was not listed in the previous update
            List<UUID> newEntries = new List<UUID>();

            lock (e.Simulator.avatarPositions.Dictionary)
            {
                // remove stale entries
                foreach(UUID trackedID in removedEntries)
                    e.Simulator.avatarPositions.Dictionary.Remove(trackedID);

                // add or update tracked info, and record who is new
                foreach (KeyValuePair<UUID, Vector3> entry in coarseEntries)
                {
                    if (!e.Simulator.avatarPositions.Dictionary.ContainsKey(entry.Key))
                        newEntries.Add(entry.Key);

                    e.Simulator.avatarPositions.Dictionary[entry.Key] = entry.Value;
                }
            }

            if (m_CoarseLocationUpdate != null)
            {
                OnCoarseLocationUpdate(new CoarseLocationUpdateEventArgs(e.Simulator, newEntries, removedEntries));
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void RegionHandleReplyHandler(object sender, PacketReceivedEventArgs e)
        {            
            if (m_RegionHandleReply != null)
            {
                RegionIDAndHandleReplyPacket reply = (RegionIDAndHandleReplyPacket)e.Packet;
                OnRegionHandleReply(new RegionHandleReplyEventArgs(reply.ReplyBlock.RegionID, reply.ReplyBlock.RegionHandle));
            }
        }

    }
    #region EventArgs classes

    public class CoarseLocationUpdateEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly List<UUID> m_NewEntries;
        private readonly List<UUID> m_RemovedEntries;

        public Simulator Simulator { get { return m_Simulator; } }
        public List<UUID> NewEntries { get { return m_NewEntries; } }
        public List<UUID> RemovedEntries { get { return m_RemovedEntries; } }

        public CoarseLocationUpdateEventArgs(Simulator simulator, List<UUID> newEntries, List<UUID> removedEntries)
        {
            this.m_Simulator = simulator;
            this.m_NewEntries = newEntries;
            this.m_RemovedEntries = removedEntries;
        }
    }

    public class GridRegionEventArgs : EventArgs
    {
        private readonly GridRegion m_Region;
        public GridRegion Region { get { return m_Region; } }

        public GridRegionEventArgs(GridRegion region)
        {
            this.m_Region = region;
        }
    }

    public class GridLayerEventArgs : EventArgs
    {
        private readonly GridLayer m_Layer;

        public GridLayer Layer { get { return m_Layer; } }

        public GridLayerEventArgs(GridLayer layer)
        {
            this.m_Layer = layer;
        }
    }

    public class GridItemsEventArgs : EventArgs
    {
        private readonly GridItemType m_Type;
        private readonly List<MapItem> m_Items;

        public GridItemType Type { get { return m_Type; } }
        public List<MapItem> Items { get { return m_Items; } }

        public GridItemsEventArgs(GridItemType type, List<MapItem> items)
        {
            this.m_Type = type;
            this.m_Items = items;
        }
    }

    public class RegionHandleReplyEventArgs : EventArgs
    {
        private readonly UUID m_RegionID;
        private readonly ulong m_RegionHandle;

        public UUID RegionID { get { return m_RegionID; } }
        public ulong RegionHandle { get { return m_RegionHandle; } }

        public RegionHandleReplyEventArgs(UUID regionID, ulong regionHandle)
        {
            this.m_RegionID = regionID;
            this.m_RegionHandle = regionHandle;
        }
    }

    #endregion
}
