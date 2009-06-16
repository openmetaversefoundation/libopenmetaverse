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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="data"></param>
        public delegate void LandPatchCallback(Simulator simulator, int x, int y, int width, float[] data);

        /// <summary></summary>
        public event LandPatchCallback OnLandPatch;

        #region Settings
        /// <summary>Enable/disable storing terrain heightmaps in the 
        /// TerrainManager</summary>
        public bool StoreLandPatches { get { return storeLandPatches; } set { storeLandPatches = value;  } }
        private bool storeLandPatches = false;
        #endregion Settings

        public InternalDictionary<ulong, TerrainPatch[]> SimPatches = new InternalDictionary<ulong, TerrainPatch[]>();
        public Vector2[] WindSpeeds = new Vector2[256];

        private LoggerInstance Log;
        private NetworkManager Network;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client"></param>
        public TerrainManager(LoggerInstance log, NetworkManager network)
        {
            Log = log;
            Network = network;
            Network.RegisterCallback(PacketType.LayerData, new NetworkManager.PacketCallback(LayerDataHandler));
        }

        /// <summary>
        /// Retrieve the terrain height at a given coordinate
        /// </summary>
        /// <param name="regionHandle">The region that the point of interest is in</param>
        /// <param name="x">Sim X coordinate, valid range is from 0 to 255</param>
        /// <param name="y">Sim Y coordinate, valid range is from 0 to 255</param>
        /// <param name="height">The terrain height at the given point if the
        /// lookup was successful, otherwise 0.0f</param>
        /// <returns>True if the lookup was successful, otherwise false</returns>
        public bool TerrainHeightAtPoint(ulong regionHandle, int x, int y, out float height)
        {
            if (x >= 0 && x < 256 && y >= 0 && y < 256)
            {
                lock (SimPatches)
                {
                    if (SimPatches.ContainsKey(regionHandle))
                    {
                        int patchX = x / 16;
                        int patchY = y / 16;
                        x = x % 16;
                        y = y % 16;

                        if (SimPatches[regionHandle][patchY * 16 + patchX] != null)
                        {
                            height = SimPatches[regionHandle][patchY * 16 + patchX].Data[y * 16 + x];
                            return true;
                        }
                    }
                }
            }

            height = 0.0f;
            return false;
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
                    Log.Log(String.Format(
                        "Invalid LayerData land packet, x={0}, y={1}, dc_offset={2}, range={3}, quant_wbits={4}, patchids={5}, count={6}",
                        x, y, header.DCOffset, header.Range, header.QuantWBits, header.PatchIDs, count),
                        Helpers.LogLevel.Warning);
                    return;
                }

                // Decode this patch
                TerrainCompressor.DecodePatch(patches, bitpack, header, group.PatchSize);

                // Decompress this patch
                float[] heightmap = TerrainCompressor.DecompressPatch(patches, header, group);

                count++;

                if (OnLandPatch != null)
                {
                    try { OnLandPatch(simulator, x, y, group.PatchSize, heightmap); }
                    catch (Exception e) { Log.Log(e.Message, Helpers.LogLevel.Error, e); }
                }

                if (StoreLandPatches)
                {
                    lock (SimPatches)
                    {
                        if (!SimPatches.ContainsKey(simulator.Handle))
                            SimPatches.Add(simulator.Handle, new TerrainPatch[16 * 16]);

                        TerrainPatch patch = new TerrainPatch();
                        patch.Data = heightmap;
                        patch.X = x;
                        patch.Y = y;

                        SimPatches[simulator.Handle][y * 16 + x] = patch;
                    }
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

            for (int i = 0; i < 256; i++)
                WindSpeeds[i] = new Vector2(xvalues[i], yvalues[i]);
        }

        private void DecompressCloud(Simulator simulator, BitPack bitpack, TerrainPatch.GroupHeader group)
        {
            // FIXME:
        }

        private void LayerDataHandler(Packet packet, Simulator simulator)
        {
            LayerDataPacket layer = (LayerDataPacket)packet;
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
                    if (OnLandPatch != null || StoreLandPatches)
                        DecompressLand(simulator, bitpack, header);
                    break;
                case TerrainPatch.LayerType.Water:
                    Log.Log("Got a Water LayerData packet, implement me!", Helpers.LogLevel.Error);
                    break;
                case TerrainPatch.LayerType.Wind:
                    DecompressWind(simulator, bitpack, header);
                    break;
                case TerrainPatch.LayerType.Cloud:
                    DecompressCloud(simulator, bitpack, header);
                    break;
                default:
                    Log.Log("Unrecognized LayerData type " + type.ToString(), Helpers.LogLevel.Warning);
                    break;
            }
        }
    }
}
