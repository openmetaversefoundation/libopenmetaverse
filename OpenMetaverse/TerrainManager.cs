/*
 * Copyright (c) 2007-2008, openmetaverse.org
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
using OpenMetaverse.Packets;

namespace OpenMetaverse
{
    public class TerrainManager
    {
        #region EventHandling
        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<LandPatchReceivedEventArgs> m_LandPatchReceivedEvent;

        /// <summary>Raises the LandPatchReceived event</summary>
        /// <param name="e">A LandPatchReceivedEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnLandPatchReceived(LandPatchReceivedEventArgs e)
        {
            EventHandler<LandPatchReceivedEventArgs> handler = m_LandPatchReceivedEvent;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_LandPatchReceivedLock = new object();

        /// <summary>Raised when the simulator responds sends </summary>
        public event EventHandler<LandPatchReceivedEventArgs> LandPatchReceived
        {
            add { lock (m_LandPatchReceivedLock) { m_LandPatchReceivedEvent += value; } }
            remove { lock (m_LandPatchReceivedLock) { m_LandPatchReceivedEvent -= value; } }
        }
        #endregion

        private GridClient Client;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client"></param>
        public TerrainManager(GridClient client)
        {
            Client = client;
            Client.Network.RegisterCallback(PacketType.LayerData, LayerDataHandler);
        }

        private void DecompressLand(Simulator simulator, BitPack bitpack, TerrainPatch.GroupHeader group)
        {
            int x;
            int y;
            int[] patches = new int[32 * 32];
            int count = 0;

            while (true)
            {
                TerrainPatch.Header header = TerrainCompressor.DecodePatchHeader(bitpack);

                if (header.QuantWBits == TerrainCompressor.END_OF_PATCHES)
                    break;

                x = header.X;
                y = header.Y;

                if (x >= TerrainCompressor.PATCHES_PER_EDGE || y >= TerrainCompressor.PATCHES_PER_EDGE)
                {
                    Logger.Log(String.Format(
                        "Invalid LayerData land packet, x={0}, y={1}, dc_offset={2}, range={3}, quant_wbits={4}, patchids={5}, count={6}",
                        x, y, header.DCOffset, header.Range, header.QuantWBits, header.PatchIDs, count),
                        Helpers.LogLevel.Warning, Client);
                    return;
                }

                // Decode this patch
                TerrainCompressor.DecodePatch(patches, bitpack, header, group.PatchSize);

                // Decompress this patch
                float[] heightmap = TerrainCompressor.DecompressPatch(patches, header, group);

                count++;

                try { OnLandPatchReceived(new LandPatchReceivedEventArgs(simulator, x, y, group.PatchSize, heightmap)); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }

                if (Client.Settings.STORE_LAND_PATCHES)
                {
                    TerrainPatch patch = new TerrainPatch();
                    patch.Data = heightmap;
                    patch.X = x;
                    patch.Y = y;
                    simulator.Terrain[y * 16 + x] = patch;
                }
            }
        }

        private void DecompressWind(Simulator simulator, BitPack bitpack, TerrainPatch.GroupHeader group)
        {
            int[] patches = new int[32 * 32];

            // Ignore the simulator stride value
            group.Stride = group.PatchSize;

            // Each wind packet contains the wind speeds and direction for the entire simulator
            // stored as two float arrays. The first array is the X value of the wind speed at
            // each 16x16m block, second is the Y value.
            // wind_speed = distance(x,y to 0,0)
            // wind_direction = vec2(x,y)

            // X values
            TerrainPatch.Header header = TerrainCompressor.DecodePatchHeader(bitpack);
            TerrainCompressor.DecodePatch(patches, bitpack, header, group.PatchSize);
            float[] xvalues = TerrainCompressor.DecompressPatch(patches, header, group);

            // Y values
            header = TerrainCompressor.DecodePatchHeader(bitpack);
            TerrainCompressor.DecodePatch(patches, bitpack, header, group.PatchSize);
            float[] yvalues = TerrainCompressor.DecompressPatch(patches, header, group);

            if (simulator.Client.Settings.STORE_LAND_PATCHES)
            {
                for (int i = 0; i < 256; i++)
                    simulator.WindSpeeds[i] = new Vector2(xvalues[i], yvalues[i]);
            }
        }

        private void DecompressCloud(Simulator simulator, BitPack bitpack, TerrainPatch.GroupHeader group)
        {
            // FIXME:
        }

        private void LayerDataHandler(object sender, PacketReceivedEventArgs e)
        {
            LayerDataPacket layer = (LayerDataPacket)e.Packet;
            BitPack bitpack = new BitPack(layer.LayerData.Data, 0);
            TerrainPatch.GroupHeader header = new TerrainPatch.GroupHeader();
            TerrainPatch.LayerType type = (TerrainPatch.LayerType)layer.LayerID.Type;

            // Stride
            header.Stride = bitpack.UnpackBits(16);
            // Patch size
            header.PatchSize = bitpack.UnpackBits(8);
            // Layer type
            header.Type = (TerrainPatch.LayerType)bitpack.UnpackBits(8);

            switch (type)
            {
                case TerrainPatch.LayerType.Land:
                    if (m_LandPatchReceivedEvent != null || Client.Settings.STORE_LAND_PATCHES)
                        DecompressLand(e.Simulator, bitpack, header);
                    break;
                case TerrainPatch.LayerType.Water:
                    Logger.Log("Got a Water LayerData packet, implement me!", Helpers.LogLevel.Error, Client);
                    break;
                case TerrainPatch.LayerType.Wind:
                    DecompressWind(e.Simulator, bitpack, header);
                    break;
                case TerrainPatch.LayerType.Cloud:
                    DecompressCloud(e.Simulator, bitpack, header);
                    break;
                default:
                    Logger.Log("Unrecognized LayerData type " + type.ToString(), Helpers.LogLevel.Warning, Client);
                    break;
            }
        }
    }

    #region EventArgs classes
    // <summary>Provides data for LandPatchReceived</summary>
    public class LandPatchReceivedEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly int m_X;
        private readonly int m_Y;
        private readonly int m_PatchSize;
        private readonly float[] m_HeightMap;

        /// <summary>Simulator from that sent tha data</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary>Sim coordinate of the patch</summary>
        public int X { get { return m_X; } }
        /// <summary>Sim coordinate of the patch</summary>
        public int Y { get { return m_Y; } }
        /// <summary>Size of tha patch</summary>
        public int PatchSize { get { return m_PatchSize; } }
        /// <summary>Heightmap for the patch</summary>
        public float[] HeightMap { get { return m_HeightMap; } }

        public LandPatchReceivedEventArgs(Simulator simulator, int x, int y, int patchSize, float[] heightMap)
        {
            this.m_Simulator = simulator;
            this.m_X = x;
            this.m_Y = y;
            this.m_PatchSize = patchSize;
            this.m_HeightMap = heightMap;
        }
    }
    #endregion
}
