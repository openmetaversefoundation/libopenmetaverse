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
    public delegate void AddRegionCallback(GridRegion region);

	/// <summary>
	/// Class for regions on the world map
	/// </summary>
	public class GridRegion
	{
        /// <summary></summary>
		public int X;
        /// <summary></summary>
		public int Y;
        /// <summary></summary>
		public string Name;
        /// <summary></summary> 
		public byte Access;
        /// <summary></summary>
		public uint RegionFlags;
        /// <summary></summary>
		public byte WaterHeight;
        /// <summary></summary>
		public byte Agents;
        /// <summary></summary>
		public LLUUID MapImageID;
        /// <summary>Used for teleporting</summary>
		public ulong RegionHandle;

        /// <summary>
        /// 
        /// </summary>
		public GridRegion() 
		{
		}
	}

	/// <summary>
	/// Manages grid-wide tasks such as the world map
	/// </summary>
	public class GridManager
	{
        public event AddRegionCallback OnRegionAdd;

        /// <summary>A hashtable of all the regions, indexed by region ID</summary>
		public Dictionary<LLUUID,GridRegion> Regions;
        /// <summary>Current direction of the sun</summary>
        public LLVector3 SunDirection;

		private SecondLife Client;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
		public GridManager(SecondLife client)
		{
			Client = client;
			Regions = new Dictionary<LLUUID,GridRegion>();
            SunDirection = new LLVector3();

			Client.Network.RegisterCallback(PacketType.MapBlockReply, new PacketCallback(MapBlockReplyHandler));
            Client.Network.RegisterCallback(PacketType.SimulatorViewerTimeMessage, new PacketCallback(TimeMessageHandler));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
		public void AddSim(string name) 
		{
			if(!Regions.ContainsKey(name)) 
			{
                MapNameRequestPacket map = new MapNameRequestPacket();
                map.AgentData.AgentID   = Client.Network.AgentID;
                map.AgentData.SessionID = Client.Network.SessionID;

                map.NameData.Name = Helpers.StringToField(name.ToLower());

                Client.Network.SendPacket((Packet)map);
			}
		}

        public void AddEstateSims()
        {
            MapLayerRequestPacket request = new MapLayerRequestPacket();
            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;
            request.AgentData.Godlike = true;
            request.AgentData.Flags = 0;
            request.AgentData.EstateID = 0; // TODO get a better value here.

            Client.Network.SendPacket((Packet)request);
        }

        public void AddLindenSims()
        {
            MapBlockRequestPacket request = new MapBlockRequestPacket();
            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;
            request.AgentData.EstateID = 0;
            request.AgentData.Flags = 0;
            request.PositionData.MaxX = 65535;
            request.PositionData.MaxY = 65535;
            request.PositionData.MinX = 0;
            request.PositionData.MinY = 0;

            Client.Network.SendPacket((Packet)request);
        }

        /// <summary>
        /// 
        /// </summary>
		public void AddAllSims() 
		{
            AddLindenSims();
            AddEstateSims();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
		public GridRegion GetSim(string name) 
		{
			if(Regions.ContainsKey(name)) 
				return Regions[name];

			AddSim(name);
			System.Threading.Thread.Sleep(1000);

			if(Regions.ContainsKey(name)) 
				return Regions[name];
			else 
			{
				//TODO: Put some better handling inplace here with some retry code
				Client.Log("GetSim(): Returned a sim that we aren't tracking",Helpers.LogLevel.Warning);
				return new GridRegion();
			}
		}

		private void MapBlockReplyHandler(Packet packet, Simulator simulator) 
		{
			GridRegion region;
            MapBlockReplyPacket map = (MapBlockReplyPacket)packet;

            foreach (MapBlockReplyPacket.DataBlock block in map.Data)
            {
                region = new GridRegion();

                region.X = block.X;
                region.Y = block.Y;
                region.Name = Helpers.FieldToString(block.Name);
                region.RegionFlags = block.RegionFlags;
                region.WaterHeight = block.WaterHeight;
                region.Agents = block.Agents;
                region.Access = block.Access;
                region.MapImageID = block.MapImageID;
                region.RegionHandle = Helpers.UIntsToLong((uint)region.X * (uint)256, (uint)region.Y * (uint)256);

                if (region.Name != "" && region.X != 0 && region.Y != 0)
                {
                    Regions[region.Name.ToLower()] = region;
                }

                if (OnRegionAdd != null)
                {
                    OnRegionAdd(region);
                }
            }
		}

        private void TimeMessageHandler(Packet packet, Simulator simulator)
        {
            SunDirection = ((SimulatorViewerTimeMessagePacket)packet).TimeInfo.SunDirection;
        }
	}
}
